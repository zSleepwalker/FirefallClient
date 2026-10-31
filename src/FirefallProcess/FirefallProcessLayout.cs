namespace Firefall.Reconstruction.FirefallProcess;

// Static facts observed in FirefallProcess.exe (Awesomium child-process stub).
public static class FirefallProcessLayout
{
    public const string Sha256 = "A4055F0E08268150625772967E31D575D0E98D234970EC7F1ECEE87BDB6109BC";
    public const string Md5 = "8ae322a83ab5d12b35ff77514bf89767";
    public const long FileSize = 39128;
    public const uint PeTimeDateStamp = 0x5335BBA1;

    public const uint ImageBase = 0x400000;
    public const uint ImageSize = 0xC000;
    public const uint EntryPointVa = 0x4013A5;
    public const uint WinMainVa = 0x401000;
    public const uint WinMainSize = 115;
    public const uint SehScopeTableVa = 0x4078F0;
    public const uint ChildProcessMainIatVa = 0x4060DC;

    public const string ImportModule = "awesomium.dll";
    public const string ImportSymbol = "?ChildProcessMain@Awesomium@@YAHPAUHINSTANCE__@@@Z";
    public const string PdbPath = @"C:\dev\chromium2\chromium\src\build\Release\awesomium_process.pdb";
}
