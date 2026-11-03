namespace Firefall.Reconstruction.HWStats;

public enum OsDescriptionGate
{
    VersionQueryFailed,
    PlatformRejected,
    MajorVersionRejected,
    ProductInfoModuleMissing,
    ProductInfoExportMissing,
    ContinueFormatting
}

public enum SystemInfoSource
{
    GetSystemInfo,
    GetNativeSystemInfo
}

/// <summary>Observed API availability gates only; never queries Windows or formats an OS description.</summary>
public static class OsDescriptionNativeLayout
{
    public const uint BuilderVa = 0x403A20;
    public const int VersionInfoExByteCount = 284;
    public const uint NtPlatformId = 2;
    public const uint MinimumMajorVersion = 5;
    public const uint ProductInfoMajorVersion = 6;
    public const string KernelModule = "kernel32.dll";
    public const string NativeSystemInfoExport = "GetNativeSystemInfo";
    public const string ProductInfoExport = "GetProductInfo";
    public const int NativeCopyCapacityCharacters = 4096;
    public const int ReportCallerBufferCharacters = 2048;
    public const bool ChecksProductInfoCallResult = false;
    public const bool CallerChecksBuilderResult = false;
    public const bool FailureWritesDestination = false;

    public static SystemInfoSource CaptureSystemInfoSource(bool nativeSystemInfoExportAvailable) =>
        nativeSystemInfoExportAvailable ? SystemInfoSource.GetNativeSystemInfo : SystemInfoSource.GetSystemInfo;

    public static OsDescriptionGate CaptureGate(bool versionQuerySucceeded, uint observedPlatform,
        uint observedMajorVersion, bool productInfoModuleAvailable, bool productInfoExportAvailable)
    {
        if (!versionQuerySucceeded)
            return OsDescriptionGate.VersionQueryFailed;
        if (observedPlatform != NtPlatformId)
            return OsDescriptionGate.PlatformRejected;
        if (observedMajorVersion < MinimumMajorVersion)
            return OsDescriptionGate.MajorVersionRejected;
        if (observedMajorVersion == ProductInfoMajorVersion)
        {
            if (!productInfoModuleAvailable)
                return OsDescriptionGate.ProductInfoModuleMissing;
            if (!productInfoExportAvailable)
                return OsDescriptionGate.ProductInfoExportMissing;
        }
        return OsDescriptionGate.ContinueFormatting;
    }
}
