namespace Firefall.Reconstruction.Awesomium;

public readonly record struct DiagnosticScalarAppend(bool ScalarAccepted, ushort FirstCodeUnit, ushort? SecondCodeUnit);

/// <summary>Scalar-to-UTF-16 decisions after native decoding, not a byte-stream decoder.</summary>
public static class DiagnosticEncodingNativeLayout
{
    public const uint WrapperVa = 0x10296210;
    public const uint ReserveHelperVa = 0x102B7A10;
    public const uint ConversionLoopVa = 0x10295F10;
    public const uint ScalarReaderVa = 0x102B78A0;
    public const uint Utf16AppendVa = 0x102B7C10;
    public const ushort ReplacementCodeUnit = 0xFFFD;
    public const bool WrapperChecksConversionResult = false;

    public static bool CaptureDecodeLoopAdmission(uint observedInputByteLength) =>
        unchecked((int)observedInputByteLength) > 0;

    public static DiagnosticScalarAppend CaptureScalarAppend(uint decodedScalar)
    {
        bool accepted = decodedScalar < 0xD800 || decodedScalar is >= 0xE000 and <= 0x10FFFF;
        if (!accepted)
            return new(false, ReplacementCodeUnit, null);
        if (decodedScalar <= 0xFFFF)
            return new(true, (ushort)decodedScalar, null);
        return new(true, (ushort)((decodedScalar >> 10) - 0x2840),
            (ushort)((decodedScalar & 0x3FF) | 0xDC00));
    }

    public static bool CaptureAggregateConversionSuccess(bool previousSuccess, bool scalarAccepted) =>
        previousSuccess && scalarAccepted;
}
