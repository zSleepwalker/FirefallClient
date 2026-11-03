namespace Firefall.Reconstruction.DeploymentCommon;

/// <summary>Observed managed IL decisions; never calls native OS or process APIs.</summary>
public static class WowNativeLayout
{
    public const uint Is64BitProcessMethodToken = 0x06000053;
    public const uint Is64BitOperatingSystemMethodToken = 0x06000054;
    public const uint ModuleContainsFunctionMethodToken = 0x06000055;
    public const uint Is64BitProcessIlRva = 0x2F61;
    public const uint Is64BitOperatingSystemIlRva = 0x2F78;
    public const uint ModuleContainsFunctionIlRva = 0x2FB0;
    public const string KernelModule = "kernel32.dll";
    public const string Wow64Export = "IsWow64Process";

    public static bool CaptureIs64BitProcess(int pointerByteCount) => pointerByteCount == 8;

    public static bool CaptureIs64BitOperatingSystem(
        int pointerByteCount,
        bool exportAvailable,
        bool apiSucceeded,
        bool reportedWow64) =>
        CaptureIs64BitProcess(pointerByteCount)
        || (exportAvailable && apiSucceeded && reportedWow64);
}
