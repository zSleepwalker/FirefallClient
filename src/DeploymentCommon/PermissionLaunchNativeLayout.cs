namespace Firefall.Reconstruction.DeploymentCommon;

public readonly record struct PermissionLaunchDescriptor(
    int ExecutableSpecialFolderId,
    string ExecutableName,
    string Arguments,
    bool CreateNoWindow,
    bool ErrorDialog,
    bool UseShellExecute,
    bool WaitsForExit,
    bool ReadsExitCode);

/// <summary>Observed launch descriptor only; never starts icacls or changes permissions.</summary>
public static class PermissionLaunchNativeLayout
{
    public const int ResetPermissionsMethodRow = 0x12F;
    public const uint ResetPermissionsBodyRva = 0x6E18;
    public const int SystemSpecialFolderId = 37;
    public const string ExecutableName = "icacls.exe";
    public const string ArgumentFormat = "\"{0}\" /RESET /T /Q /C";
    public const string ExceptionLogPrefix = "Failed to set permissions: ";
    public const string PersistentEventName = "Permissions";
    public const bool DiscardsProcessStartBoolean = true;

    public static PermissionLaunchDescriptor CaptureDescriptor(string? installDirectory) =>
        new(SystemSpecialFolderId, ExecutableName, $"\"{installDirectory}\" /RESET /T /Q /C",
            true, false, false, false, false);
}
