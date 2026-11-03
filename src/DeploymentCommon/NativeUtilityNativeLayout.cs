namespace Firefall.Reconstruction.DeploymentCommon;

public enum CommonFolderPathSource
{
    ShellStringBuilder,
    ManagedSpecialFolderFallback
}

/// <summary>Observed managed utility boundaries; no window, shell, or filesystem calls.</summary>
public static class NativeUtilityNativeLayout
{
    public const int ClientToScreenMethodRow = 0x104;
    public const int MoveWindowMethodRow = 0x105;
    public const int ShellFolderImportMethodRow = 0x127;
    public const int GetCommonPathMethodRow = 0x128;
    public const uint GetCommonPathBodyRva = 0x6A10;
    public const int ShellPathBufferCapacity = 260;
    public const bool CreateShellFolder = false;
    public const int CommonDesktopDirectoryId = 25;
    public const int DesktopDirectoryFallbackId = 16;
    public const int CommonStartMenuId = 22;
    public const int StartMenuFallbackId = 11;

    public static CommonFolderPathSource CaptureCommonPathSource(
        bool nativeBoolResult, bool managedExceptionThrown)
    {
        // The IL pops the native BOOL; only a managed exception selects fallback.
        _ = nativeBoolResult;
        return managedExceptionThrown
            ? CommonFolderPathSource.ManagedSpecialFolderFallback
            : CommonFolderPathSource.ShellStringBuilder;
    }
}
