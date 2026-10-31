namespace Firefall.Reconstruction.HWStats;

// Static facts observed in system\bin\HWStats.exe (x86, console, VS2013 CRT).
public static class HWStatsLayout
{
    public const string Sha256 = "0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D";
    public const long FileSize = 594648;
    public const uint ImageBase = 0x400000;
    public const uint EntryPointRva = 0x3B5D0;
    public const uint MainVa = 0x404480;
    public const uint ReportVa = 0x404340;
    public const string PdbPath = @"C:\buildroot\environment\prod\gen\client\products\client\HWStats.pdb";

    // Report sections in emission order (callee VAs from the report function).
    public static readonly (string Name, uint Va)[] Sections =
    {
        ("macaddr", 0x404340), ("soundplayback/soundrecording (DirectSound)", 0x404340),
        ("ram/nprocessors + CPUID (sub_4035A0 -> sub_403350)", 0x4035A0), ("display registry (sub_4034C0)", 0x4034C0),
        ("memory (sub_4037F0)", 0x4037F0), ("System: OS/DirectX version + BIOS registry values (sub_404020)", 0x404020),
        ("D3D9/CPU features (sub_402250)", 0x402250), ("report object cleanup, not a section (sub_406880)", 0x406880),
    };

    public const uint CpuidSectionVa = 0x403350;
    public const uint ReportCleanupVa = 0x406880;
    public const int ReportObjectFileOffset = 0;
    public const int ReportObjectIndentOffset = 0x54;
    public const int ReportObjectHeapSlots = 20;
}

