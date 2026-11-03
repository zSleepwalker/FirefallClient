namespace Firefall.Reconstruction.DeploymentCommon;

/// <summary>Single-observation model of install-root selection; no WPF or registry access.</summary>
public static class InstallLocationNativeLayout
{
    public const string ProductIdentifier = "Firefall_Beta";
    public const string ProductIdentifierDisplay = "Firefall";
    public const string ApplicationPropertyName = "installDir";
    public const string RegistryValueName = "InstallLocation";
    public const string RemovedCultureSubstring = " --culture";
    public const int ProductIdentifierMethodRow = 0x2A;
    public const int ProductDisplayMethodRow = 0x2B;
    public const int GetInstallLocationMethodRow = 0x31;
    public const int GetInstallRegistryKeyMethodRow = 0x44;
    public const uint ProductIdentifierBodyRva = 0x260E;
    public const uint ProductDisplayBodyRva = 0x2615;
    public const uint GetInstallLocationBodyRva = 0x2658;
    public const uint GetInstallRegistryKeyBodyRva = 0x2A0C;

    public static string? CaptureRegistryString(object? registryValue) => registryValue as string;

    public static string? CaptureInstallLocation(
        bool applicationPropertyPresent, object? applicationProperty, object? registryValue)
    {
        if (applicationPropertyPresent)
        {
            string? overridePath = (string?)applicationProperty;
            if (!string.IsNullOrEmpty(overridePath))
                return overridePath.Replace(RemovedCultureSubstring, "", StringComparison.Ordinal);
        }
        return CaptureRegistryString(registryValue)?.Replace(RemovedCultureSubstring, "", StringComparison.Ordinal);
    }
}
