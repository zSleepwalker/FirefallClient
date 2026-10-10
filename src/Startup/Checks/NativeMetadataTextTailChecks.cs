using System.Text;
using Firefall.Reconstruction.Startup;

internal static class NativeMetadataTextTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        string source = string.Concat(Enumerable.Range(0, 65).Select(index => (char)(0x100 + index)));
        var exactly64 = NativeMetadataTextTail.EncodeAfterCapturedNormalization(source.AsSpan(0, 64));
        var over64 = NativeMetadataTextTail.EncodeAfterCapturedNormalization(source);
        check("Native metadata shortening starts only above64 UTF16 units and retains first31, dots and last30 units",
            exactly64.ShortenedText == source[..64]
            && over64.ShortenedText == source[..31] + "..." + source[^30..]
            && over64.ShortenedText.Length == 64 && over64.Utf8Bytes is { Length: 125 });
        var raw = NativeMetadataTextTail.EncodeAfterCapturedNormalization("A\0\u007F\u0080\u07FF\u0800\uFFFF\U00010000\U0010FFFF");
        check("Native metadata UTF8 tail retains case and embedded zero, emits no BOM/terminator and combines surrogate pairs",
            raw.Utf8Bytes is { } bytes
            && Convert.ToHexString(bytes.Span) == "41007FC280DFBFE0A080EFBFBFF0908080F48FBFBF");
        var highAtEnd = NativeMetadataTextTail.EncodeAfterCapturedNormalization("\uD800");
        var standaloneLow = NativeMetadataTextTail.EncodeAfterCapturedNormalization("\uDC00");
        var mismatchedPair = NativeMetadataTextTail.EncodeAfterCapturedNormalization("\uD800A");
        check("Native metadata invalid surrogates preserve explicit failing code unit instead of replacement UTF8",
            highAtEnd is { Outcome: NativeMetadataTextOutcome.InvalidUtf16, Utf8Bytes: null, InvalidCodeUnitIndex: 0, InvalidCodeUnit: 0xD800 }
            && standaloneLow is { Outcome: NativeMetadataTextOutcome.InvalidUtf16, InvalidCodeUnit: 0xDC00 }
            && mismatchedPair is { Outcome: NativeMetadataTextOutcome.InvalidUtf16, InvalidCodeUnitIndex: 1, InvalidCodeUnit: 0x41 });
        string splitPair = new string('a', 30) + "\U00010000" + new string('b', 33);
        var cut = NativeMetadataTextTail.EncodeAfterCapturedNormalization(splitPair);
        string splitSuffix = new string('a', 34) + "\U00010000" + new string('b', 29);
        var suffixCut = NativeMetadataTextTail.EncodeAfterCapturedNormalization(splitSuffix);
        check("Native shortening is by UTF16 code units and can split otherwise valid surrogate pairs at either retained edge",
            splitPair.Length == 65 && cut is { Outcome: NativeMetadataTextOutcome.InvalidUtf16, InvalidCodeUnitIndex: 31, InvalidCodeUnit: 0x2E }
            && suffixCut is { Outcome: NativeMetadataTextOutcome.InvalidUtf16, InvalidCodeUnitIndex: 34, InvalidCodeUnit: 0xDC00 });
        bool scalarMatches = true;
        for (uint scalar = 0; scalar <= 0x10FFFF; scalar++)
        {
            if (scalar is >= 0xD800 and <= 0xDFFF)
                continue;
            scalarMatches &= NativeMetadataTextTail.EncodeScalar(scalar)
                .SequenceEqual(Encoding.UTF8.GetBytes(char.ConvertFromUtf32((int)scalar)));
        }
        check("Native metadata scalar encoder exactly matches standard UTF8 for every valid Unicode scalar", scalarMatches);
        bool rejectsSurrogate = false, rejectsOutOfRange = false;
        try { NativeMetadataTextTail.EncodeScalar(0xD800); }
        catch (ArgumentOutOfRangeException) { rejectsSurrogate = true; }
        try { NativeMetadataTextTail.EncodeScalar(0x110000); }
        catch (ArgumentOutOfRangeException) { rejectsOutOfRange = true; }
        check("Native metadata scalar encoder rejects surrogate scalar and above10FFFF instead of producing nonstandard sequences",
            rejectsSurrogate && rejectsOutOfRange);
        byte[] drive = new byte[16], adapter = new byte[16];
        var descriptor = new MatrixModeZeroDescriptorSnapshot(0, drive, adapter,
            new(1, raw.Utf8Bytes!.Value, 2, over64.Utf8Bytes!.Value));
        byte[] encoded = MatrixModeZeroDescriptorCodec.Serialize(descriptor);
        check("Recovered metadata tail bytes feed descriptor length-prefixed narrow fields without another encoding pass",
            encoded.AsSpan(44, raw.Utf8Bytes.Value.Length).SequenceEqual(raw.Utf8Bytes.Value.Span)
            && encoded.Length == 50 + raw.Utf8Bytes.Value.Length + over64.Utf8Bytes.Value.Length);
    }
}
