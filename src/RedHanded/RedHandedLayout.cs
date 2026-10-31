namespace Firefall.Reconstruction.RedHanded;

// Static facts observed in system\bin\RedHanded.exe (x86, GUI, VS2013 CRT).
public static class RedHandedLayout
{
    public const string Sha256 = "1FBD21966A9DA297AB46A061ECE94BE8F4822ACC103BF2D17CB5F076476AED9B";
    public const string Md5 = "1bf8639a2c829374c2aa76dc1d79e041";
    public const long FileSize = 1918168;
    public const uint ImageBase = 0x400000;
    public const uint EntryPointVa = 0x52FD52;
    public const uint WinMainVa = 0x404540;
    public const uint DebugEventHandlerVa = 0x407630;
    public const uint SpawnDumperVa = 0x407040;
    public const uint TelemetryPostVa = 0x4092C0;
    public const string PdbPath = @"C:\buildroot\environment\prod\gen\client\products\client\RedHanded.pdb";

    public const string DefaultDumpTruck = "DumpTruck.exe";
    public const string DefaultDumper = "r5dumper.exe";
    public const int DefaultSourceRev = 117123;
    public const int DefaultBuildNumber = 1962;
    public const int RestartStatus = 75;
    public const uint KillExitCode = 0xBADC0DE;
    public const string TelemetryPath = "/api/v1/client_event";
    public const string ClientApiRegistryValue = "clientapi_host";

    // Options read via the argument parser in wWinMain; positional args after "--" are the monitored command line.
    public static readonly string[] Options =
    {
        "rundir", "dumpurl", "dumptruck-autosubmit", "app", "sourcerev", "buildnumber",
        "envmnemonic", "envname", "hotfix-exe", "dumptruck", "dumper", "bigdump", "debug", "nofear",
    };

    // Switches RedHanded passes to DumpTruck.
    public static readonly string[] DumpTruckSwitches =
        { "--autosubmit", "--url=", "--debug", "--msg=", "--app=", "--dump=", "--auth=", "--log=" };

    // Userdata line sent to r5dumper's pipe: "<streamType>|<str|i32|i64>|<value>".
    public static string FormatUserData(uint streamType, string value) => $"{streamType}|str|{value}";
    public static string FormatUserData(uint streamType, uint value) => $"{streamType}|i32|{value}";
    public static string FormatUserData(uint streamType, ulong value) => $"{streamType}|i64|{value}";
}
