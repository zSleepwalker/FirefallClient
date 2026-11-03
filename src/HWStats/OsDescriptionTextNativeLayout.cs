namespace Firefall.Reconstruction.HWStats;

/// <summary>Observed text fragments only, after OS admission; does not emulate CRT stream formatting.</summary>
public static class OsDescriptionTextNativeLayout
{
    public const uint ProductIndexTableVa = 0x403FA8;
    public const uint ProductJumpTableVa = 0x403F5C;
    public const uint WideTextWriterVa = 0x4060C0;
    public const uint ClosingDelimiterWriterVa = 0x406310;
    public const string MicrosoftPrefix = "Microsoft ";
    public const string ServicePackSeparator = " ";
    public const string BuildPrefix = " (build ";
    public const byte ClosingDelimiterInputByte = (byte)')';

    public static string CaptureNt6Name(uint minorVersion, byte productType) => (minorVersion, productType == 1) switch
    {
        (0, true) => "Windows Vista ",
        (0, false) => "Windows Server 2008 ",
        (1, true) => "Windows 7 ",
        (1, false) => "Windows Server 2008 R2 ",
        (2, true) => "Windows 8 ",
        (2, false) => "Windows Server 8 ",
        _ => "Windows ? "
    };

    public static string CaptureNt6Edition(uint observedProductCode) => observedProductCode switch
    {
        1 => "Ultimate Edition",
        2 => "Home Basic Edition",
        3 => "Home Premium Edition",
        4 => "Enterprise Edition",
        6 => "Business Edition",
        7 => "Standard Edition",
        8 => "Datacenter Edition",
        9 => "Small Business Server",
        10 => "Enterprise Edition",
        11 => "Starter Edition",
        12 => "Datacenter Edition (core installation)",
        13 => "Standard Edition (core installation)",
        14 => "Enterprise Edition (core installation)",
        15 => "Enterprise Edition for Itanium - based Systems",
        17 => "Web Server Edition",
        18 => "Cluster Server Edition",
        25 => "Small Business Server Premium Edition",
        48 => "Professional",
        _ => ""
    };

    public static string CaptureNt5Name(uint minorVersion, byte productType,
        ushort processorArchitecture, ushort suiteMask, bool observedServerR2Metric) => minorVersion switch
    {
        0 => "Windows 2000 " + (productType == 1 ? "Professional"
            : (suiteMask & 0x80) != 0 ? "Datacenter Server"
            : (suiteMask & 2) != 0 ? "Advanced Server" : "Server"),
        1 => "Windows XP " + ((suiteMask & 0x200) != 0 ? "Home Edition" : "Professional"),
        2 => CaptureNt52Name(productType, processorArchitecture, suiteMask, observedServerR2Metric),
        _ => ""
    };

    public static string CaptureArchitectureSuffix(uint majorVersion, ushort processorArchitecture) =>
        majorVersion < 6 ? "" : processorArchitecture switch
        {
            9 => ", 64-bit",
            0 => ", 32-bit",
            _ => ""
        };

    private static string CaptureNt52Name(byte productType, ushort architecture, ushort suiteMask, bool serverR2)
    {
        string name = serverR2 ? "Windows Server 2003 R2, "
            : (suiteMask & 0x2000) != 0 ? "Windows Storage Server 2003"
            : (suiteMask & 0x8000) != 0 ? "Windows Home Server"
            : productType == 1 && architecture == 9 ? "Windows XP Professional x64 Edition"
            : "Windows Server 2003, ";
        if (productType == 1)
            return name;
        string edition = architecture switch
        {
            6 => (suiteMask & 0x80) != 0 ? "Datacenter Edition for Itanium-based Systems"
                : (suiteMask & 2) != 0 ? "Enterprise Edition for Itanium-based Systems" : "",
            9 => (suiteMask & 0x80) != 0 ? "Datacenter x64 Edition"
                : (suiteMask & 2) != 0 ? "Enterprise x64 Edition" : "Standard x64 Edition",
            _ => (suiteMask & 0x4000) != 0 ? "Compute Cluster Edition"
                : (suiteMask & 0x80) != 0 ? "Datacenter Edition"
                : (suiteMask & 2) != 0 ? "Enterprise Edition"
                : (suiteMask & 0x400) != 0 ? "Web Edition" : "Standard Edition"
        };
        return name + edition;
    }
}
