namespace Firefall.Reconstruction.DeploymentCommon;

public enum ShortcutFolder
{
    SelectedDesktop,
    SelectedStartMenuProductDirectory
}

public readonly record struct ShortcutDefinition(
    ShortcutFolder Folder,
    string Name,
    string Description,
    string TargetInstallRelativePath,
    string WorkingDirectoryInstallRelativePath,
    string? IconInstallRelativePath,
    string? Arguments);

/// <summary>COM property plans and deletion gates only; never creates or deletes anything.</summary>
public static class ShortcutNativeLayout
{
    public const int CreateShortcutMethodRow = 0x125;
    public const int RemoveShortcutsMethodRow = 0x126;
    public const uint CreateShortcutBodyRva = 0x66A0;
    public const uint RemoveShortcutsBodyRva = 0x6854;
    public const string StartMenuProductDirectoryFormat = @"Red 5 Studios\{0}";
    public const bool RecursiveDirectoryRemoval = false;

    public static ShortcutDefinition[] CaptureCreation(string? productDisplay) =>
    [
        new(ShortcutFolder.SelectedDesktop, $"Play {productDisplay}.lnk", $"Play {productDisplay}",
            @"system\bin\Launcher.exe", @"system\bin", @"system\bin\FirefallClient.exe", null),
        new(ShortcutFolder.SelectedStartMenuProductDirectory, $"Play {productDisplay}.lnk", $"Play {productDisplay}",
            @"system\bin\Launcher.exe", @"system\bin", @"system\bin\FirefallClient.exe", null),
        new(ShortcutFolder.SelectedStartMenuProductDirectory, $"Uninstall {productDisplay}.lnk",
            $"Uninstall {productDisplay}", @"uninstall\FirefallInstaller.exe", "uninstall", null, "/uninstall")
    ];

    public static bool CaptureFileRemoval(bool fileExists) => fileExists;

    public static bool CaptureDirectoryRemoval(bool directoryExists, uint remainingEntryCount) =>
        directoryExists && remainingEntryCount == 0;
}
