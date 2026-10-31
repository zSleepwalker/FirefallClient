namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Bounded static observations of FirefallClient.exe crash handling. Addresses are virtual
/// addresses unless named FileOffset; see docs/CrashHandler.md for observed versus interpreted.
/// </summary>
public static class CrashHandlerNativeLayout
{
    public const string DbgHelpDllName = "dbghelp_red5.dll";
    public const string MiniDumpExportName = "MiniDumpWriteDump";
    public const string DumpNameFormat = "%s%s.dmp";
    public const int DbgHelpAnsiNameVirtualAddress = 0x1C16048;
    public const int DbgHelpWideNameVirtualAddress = 0x1C160AC;
    public const int DumpFormatWideVirtualAddress = 0x1C1605C;
    public const int DumpFormatAnsiVirtualAddress = 0x1C16078;
    public const int MiniDumpExportAnsiVirtualAddress = 0x1C16084;

    public const int AnsiDumpWriterFileOffset = 0x12C2F90;
    public const int WideDumpWriterFileOffset = 0x12C3250;
    public const int FatalDispatcherFileOffset = 0x12C3550;
    public const int AssertRaiseFileOffset = 0x12C3650;
    public const int SigAbrtHandlerFileOffset = 0x12C3710;
    public const int TerminateStubFileOffset = 0x12C3770;
    public const int UnexpectedStubFileOffset = 0x12C3780;
    public const int PureCallStubFileOffset = 0x12C3790;
    public const int InvalidParameterStubFileOffset = 0x12C37A0;
    public const int HandlerInstallFileOffset = 0x12C37D0;
    public const int AssertMessageFormatterFileOffset = 0x12C38E0;
    public const int RedHandedLaunchFileOffset = 0x2EA990;

    public const int MiniDumpTypeFullMemory = 2;
    public const int UserStreamCount = 5;
    public const int MaxPath = 260;
    public const int Sigabrt = 22;
    public const int FatalExitCode = 123;
    public const uint AssertExceptionCode = 0x012ED500;
    public const uint AssertContinuableExceptionCode = 0x012ED502;

    public const int StreamName = 0x1000B;
    public const int StreamBuildKind = 0x10002;
    public const int StreamBuildNumber = 0x10005;
    public const int StreamSecondValue = 0x10004;
    public const int StreamMessage = 0x10007;
    public const int BuildNumberValue = 1962;
    public const int SecondValue = 117123;

    public const int TerminateTextVirtualAddress = 0x1C15F48;
    public const int DeathTextVirtualAddress = 0x1C15F90;
    public const int AssertFormatVirtualAddress = 0x1C15FB4;
    public const int AssertMessageFormatVirtualAddress = 0x1C15FD4;
    public const int PureVirtualTextVirtualAddress = 0x1C1600C;
    public const int InvalidParameterTextVirtualAddress = 0x1C16034;

    public const string RedHandedX64 = @"64\RedHanded.exe";
    public const string RedHandedX86 = "RedHanded.exe";
    public const int RedHandedX64VirtualAddress = 0x1B3F170;
    public const string DumpTruckArgument = "--dumptruck";
    public const string BigDumpArgument = "--bigdump";
    public const string WrapperFailureText = "Unable to execute redhanded wrapper. FirefallClient may not be run without it.";
}
