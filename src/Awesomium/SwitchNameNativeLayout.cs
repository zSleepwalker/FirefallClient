namespace Firefall.Reconstruction.Awesomium;

/// <summary>Observed per-byte transforms and ASCII admission, not a command-line parser.</summary>
public static class SwitchNameNativeLayout
{
    public const uint WideToNarrowVa = 0x1029A9C0;
    public const uint LowByteCopyVa = 0x1000B350;
    public const uint AsciiLowercaseVa = 0x10007AB0;
    public const uint WideAsciiPredicateVa = 0x10299E00;
    public const uint AsciiValueGetterVa = 0x1028C8A0;
    public const uint DiagnosticDestructorCallVa = 0x1029AA58;

    public static byte CaptureAsciiLowercaseByte(byte value) =>
        value is >= (byte)'A' and <= (byte)'Z' ? (byte)(value + 32) : value;

    /// <summary>Only the conversion path after any diagnostic returns is modeled.</summary>
    public static byte CaptureNarrowedCodeUnit(ushort codeUnit) => unchecked((byte)codeUnit);

    public static bool CaptureWideAsciiAdmission(ReadOnlySpan<char> codeUnits)
    {
        foreach (char codeUnit in codeUnits)
            if (codeUnit > 0x7F)
                return false;
        return true;
    }

    public static bool CaptureConversionDiagnosticAdmission(bool observedModeEqualsOne,
        int observedLoggingThreshold, ReadOnlySpan<char> codeUnits) =>
        observedModeEqualsOne && observedLoggingThreshold <= 3 && !CaptureWideAsciiAdmission(codeUnits);
}
