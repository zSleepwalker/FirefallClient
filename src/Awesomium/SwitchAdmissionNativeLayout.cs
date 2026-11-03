namespace Firefall.Reconstruction.Awesomium;

/// <summary>Switch-scan state around ordinary positionals and the exact "--" marker.</summary>
public static class SwitchAdmissionNativeLayout
{
    public const uint ParsedSwitchLoopVa = 0x1028DCD0;
    public const uint EndOfSwitchComparisonCallVa = 0x1028DD95;
    public const uint ScanStateUpdateVa = 0x1028DD9F;
    public const uint SwitchSplitCallVa = 0x1028DDB4;
    public const uint PositionalAppendCallVa = 0x1028DE03;
    public const uint PrefixValidatorCallVa = 0x1028BCAD;
    public const uint SwitchMapBuilderCallVa = 0x1028DDDC;
    public const uint EndOfSwitchMarkerVa = 0x1127B3F8;
    public const uint SwitchPrefixTableVa = 0x1127B404;

    public static bool CaptureNextSwitchScanningState(
        bool wasScanning,
        bool isExactEndOfSwitchMarker) =>
        wasScanning && !isExactEndOfSwitchMarker;

    public static bool CaptureCurrentSwitchMapAdmission(
        bool wasScanning,
        bool isExactEndOfSwitchMarker,
        bool hasRecognizedSwitchPrefix) =>
        wasScanning && !isExactEndOfSwitchMarker && hasRecognizedSwitchPrefix;
}
