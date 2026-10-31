namespace Firefall.Reconstruction.Awesomium;

/// <summary>
/// Facts observed statically in awesomium.dll (third party, x86). All *Va values are image VAs (base 0x10000000).
/// </summary>
public static class AwesomiumLayout
{
    public const string Md5 = "60602fe6daf7bb25336c9a3bc515394d";
    public const string Sha256 = "de27851602cb1387ef50086761b417d3d39a2a2648b5490939f843a711ff7f6e";
    public const uint ImageBase = 0x10000000;
    public const uint ImageSize = 0x16A5000;
    public const int ExportCount = 855;
    public const int AweCExportCount = 530;
    public const int CppExportCount = 317;

    // Function VAs
    public const uint IsChildProcessVa = 0x1003A630;
    public const uint ChildProcessMainVa = 0x1003A710;
    public const uint AweIsChildProcessVa = 0x1004C820;
    public const uint AweChildProcessMainVa = 0x1004C840;
    public const uint WebCoreInitVa = 0x1000CF80;
    public const uint ChildPathResolveVa = 0x1000CE50;
    public const uint ChildLauncherVa = 0x10189DD0;
    public const uint ContentMainVa = 0x10064B90;
    public const uint AweWebConfigChildProcessPathSetVa = 0x1004B2E0;

    // Command-line switch names (observed strings)
    public const string SwitchBrowserSubprocessPath = "browser-subprocess-path";
    public const string SwitchType = "type";
    public const string SwitchNoSandbox = "no-sandbox";
    public const string SwitchSingleProcess = "single-process";
    public const string SwitchLogPath = "awesomium-log-path";
    public const string DefaultLogFile = "./awesomium.log";
    public const string RendererTypeArg = " --type=renderer ";
    public const string PrefetchFormat = "/prefetch:%d";
    public const string IpcPipePrefix = @"\\.\pipe\chrome."; // UTF-16 in image
}
