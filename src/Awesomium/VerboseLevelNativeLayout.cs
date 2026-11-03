namespace Firefall.Reconstruction.Awesomium;

/// <summary>Observed verbose lookup caller arithmetic; no source read, diagnostic or rule matching.</summary>
public static class VerboseLevelNativeLayout
{
    public const uint LevelGetterVa = 0x1028A790;
    public const uint ConfigurationPointerVa = 0x11596AFC;

    public static bool CaptureZeroExtentDiagnosticAdmission(
        int observedDiagnosticMode, int observedMinimumSeverity, uint suppliedExtent) =>
        observedDiagnosticMode == 1 && observedMinimumSeverity <= 3 && suppliedExtent == 0;

    public static uint CaptureLookupByteLength(uint suppliedExtent) =>
        unchecked(suppliedExtent - 1);

    public static int CaptureLevelWithoutConfiguration(int observedMinimumSeverity) =>
        Math.Max(unchecked(-observedMinimumSeverity), -1);
}
