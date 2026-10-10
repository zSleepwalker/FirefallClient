namespace Firefall.Reconstruction.Startup;

public enum NativeMetadataTextOutcome
{
    Encoded,
    InvalidUtf16
}

public sealed record NativeMetadataTextResult(
    NativeMetadataTextOutcome Outcome, string ShortenedText, ReadOnlyMemory<byte>? Utf8Bytes,
    int? InvalidCodeUnitIndex, ushort? InvalidCodeUnit);

/// <summary>Metadata conversion tail after captured path normalization and locale towlower; no host path or locale APIs.</summary>
public static class NativeMetadataTextTail
{
    public const uint ShortenFileOffset = 0x1283720;
    public const uint EncodeUtf16RangeFileOffset = 0x1258C70;
    public const uint EncodeScalarFileOffset = 0x1258E90;
    public const uint ShorteningLiteralVirtualAddress = 0x1C7EC5C;
    public const int MaximumCodeUnits = 64;
    public const string CenterMarker = "...";

    public static NativeMetadataTextResult EncodeAfterCapturedNormalization(ReadOnlySpan<char> normalizedLoweredText)
    {
        string text = normalizedLoweredText.Length > MaximumCodeUnits
            ? string.Concat(normalizedLoweredText[..31], CenterMarker.AsSpan(), normalizedLoweredText[^30..])
            : new string(normalizedLoweredText);
        var bytes = new List<byte>(text.Length * 3);
        for (int i = 0; i < text.Length; i++)
        {
            uint scalar = text[i];
            if (scalar is >= 0xD800 and <= 0xDBFF)
            {
                if (i + 1 == text.Length)
                    return Invalid(text, i);
                uint low = text[++i];
                if (low is < 0xDC00 or > 0xDFFF)
                    return Invalid(text, i);
                scalar = 0x10000 + ((scalar - 0xD800) << 10) + low - 0xDC00;
            }
            else if (scalar is >= 0xDC00 and <= 0xDFFF)
                return Invalid(text, i);
            AppendScalar(bytes, scalar);
        }
        return new(NativeMetadataTextOutcome.Encoded, text, bytes.ToArray(), null, null);
    }

    public static byte[] EncodeScalar(uint scalar)
    {
        if (scalar > 0x10FFFF || scalar is >= 0xD800 and <= 0xDFFF)
            throw new ArgumentOutOfRangeException(nameof(scalar), "Native UTF8 scalar encoder rejects surrogate values and values above 10FFFF.");
        var bytes = new List<byte>(4);
        AppendScalar(bytes, scalar);
        return bytes.ToArray();
    }

    private static NativeMetadataTextResult Invalid(string text, int index) =>
        new(NativeMetadataTextOutcome.InvalidUtf16, text, null, index, text[index]);

    private static void AppendScalar(List<byte> bytes, uint scalar)
    {
        if (scalar < 0x80)
            bytes.Add((byte)scalar);
        else if (scalar < 0x800)
        {
            bytes.Add((byte)((scalar >> 6) | 0xC0));
            bytes.Add((byte)((scalar & 0x3F) | 0x80));
        }
        else if (scalar < 0x10000)
        {
            bytes.Add((byte)((scalar >> 12) | 0xE0));
            bytes.Add((byte)(((scalar >> 6) & 0x3F) | 0x80));
            bytes.Add((byte)((scalar & 0x3F) | 0x80));
        }
        else
        {
            bytes.Add((byte)((scalar >> 18) | 0xF0));
            bytes.Add((byte)(((scalar >> 12) & 0x3F) | 0x80));
            bytes.Add((byte)(((scalar >> 6) & 0x3F) | 0x80));
            bytes.Add((byte)((scalar & 0x3F) | 0x80));
        }
    }
}
