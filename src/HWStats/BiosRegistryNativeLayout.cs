namespace Firefall.Reconstruction.HWStats;

/// <summary>Inert registry request facts and print admission; never reads the registry.</summary>
public static class BiosRegistryNativeLayout
{
    public const uint SystemSectionVa = 0x404020;
    public const uint ValueWriterVa = 0x4030F0;
    public const uint QueryHelperVa = 0x406F30;
    public const uint SlashCopyHelperVa = 0x406EC0;
    public const uint Hive = 0x80000002;
    public const uint AccessMask = 0xF003F;
    public const int OutputBufferBytes = 1024;
    public const string SourceBasePath = "HARDWARE/DESCRIPTION/SYSTEM";
    public const string OpenedSubkey = @"HARDWARE\DESCRIPTION\SYSTEM\";
    public const string NativeFormat = "%s = %s\n";
    public const uint FailureHResult = 0x80004005;

    private static readonly string[] NativeValueNames =
        ["Identifier", "SystemBiosDate", "SystemBiosVersion", "VideoBiosDate", "VideoBiosVersion"];

    public static ReadOnlySpan<string> ValueNames => NativeValueNames;

    public static uint CaptureQueryHelperResult(uint observedRegistryStatus) =>
        observedRegistryStatus == 0 ? 0 : FailureHResult;

    public static bool CapturePrintAdmission(uint observedHelperResult, uint observedValueType) =>
        observedHelperResult == 0 && observedValueType is 1 or 7;
}
