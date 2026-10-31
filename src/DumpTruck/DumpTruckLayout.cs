namespace Firefall.Reconstruction.DumpTruck;

// Static facts observed in system\bin\DumpTruck.exe (x86, GUI).
public static class DumpTruckLayout
{
    public const string Sha256 = "53796D440D586A306C36ADAE86744C580B9F9ECD3987D2C0D3CAB0EA77ADD6E4";
    public const long FileSize = 16675032;
    public const uint ImageBase = 0x400000;
    public const uint EntryPointRva = 0x6E600;
    public const uint InitAndOptionsVa = 0x404BD0;
    public const uint SubmitFlowVa = 0x40A3F0;
    public const uint CrashReportRequestVa = 0x409A60;
    public const uint ConfirmUploadVa = 0x40B100;
    public const uint HwReportRunnerVa = 0x42AB90;
    public const string PdbPath = @"C:\buildroot\environment\prod\gen\client\products\client\DumpTruck.pdb";

    public const string CrashReportUrl = "http://crash.firefall.com/crash_report.xml";
    public const string RequestAuthTemplate = "<report-crash><auth>{0}</auth></report-crash>";
    public const string ConfirmTemplate =
        "<confirm-upload><path>{0}</path><date>{1}</date><awsauth>{2}</awsauth><auth>{3}</auth></confirm-upload>";
    public const string SignatureFormat =
        "ver=%d&tc=%u&nonce=%04x%04x%04x%04x&uid=%s&host=%s&path=%s&hbody=%s&cid=%llu";
    public const string SignatureHeader = "X-Red5-Signature";
    public const string HwReportExe = "HWStats.exe";

    public static readonly string[] Options =
        { "dump", "log", "auth", "app", "msg", "url", "autosubmit", "debug", "debuguser", "nodelete", "nofear", "userlangid", "forge", "dump-config" };
}
