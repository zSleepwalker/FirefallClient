namespace Firefall.Reconstruction.Startup;

public enum MatrixSecondaryCommandInputPath
{
    PrimaryByteRouteMap,
    SecondarySelectorTable,
    ReturnZero
}

public readonly record struct MatrixSecondaryCommandRouteRange(
    byte FirstTableIndex, byte LastTableIndex, int RouteIndex);

/// <summary>
/// Static route boundaries for raw 0x81CC50. This records dispatch only; it
/// does not decode command bodies or implement the two native jump tables.
/// </summary>
public static class MatrixSecondaryCommandDispatcherNativeLayout
{
    public const uint MethodFileOffset = 0x81CC50;
    public const uint PrimaryJumpTableVirtualAddress = 0xC223F0;
    public const uint PrimaryJumpTableFileOffset = 0x8217F0;
    public const uint PrimaryByteMapVirtualAddress = 0xC2240C;
    public const uint PrimaryByteMapFileOffset = 0x82180C;
    public const int PrimaryByteMapLength = 0x36;
    public const int PrimaryByteMapMaximumInput = PrimaryByteMapLength - 1;
    public const uint PrimaryZeroReturnFileOffset = 0x8217D9;
    public const uint SecondaryJumpTableVirtualAddress = 0xC22630;
    public const uint SecondaryJumpTableFileOffset = 0x821A30;
    public const int SecondarySelectorMinimum = 0x20;
    public const int SecondarySelectorMaximum = 0x7A;
    public const int SecondaryJumpTableEntryCount = 0x5B;

    public static IReadOnlyList<MatrixSecondaryCommandRouteRange> PrimaryRouteRanges { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixSecondaryCommandRouteRange(0x00, 0x0D, 0),
            new MatrixSecondaryCommandRouteRange(0x0E, 0x11, 1),
            new MatrixSecondaryCommandRouteRange(0x12, 0x18, 2),
            new MatrixSecondaryCommandRouteRange(0x19, 0x1E, 3),
            new MatrixSecondaryCommandRouteRange(0x1F, 0x20, 1),
            new MatrixSecondaryCommandRouteRange(0x21, 0x24, 4),
            new MatrixSecondaryCommandRouteRange(0x25, 0x27, 5),
            new MatrixSecondaryCommandRouteRange(0x28, 0x32, 1),
            new MatrixSecondaryCommandRouteRange(0x33, 0x34, 6),
            new MatrixSecondaryCommandRouteRange(0x35, 0x35, 1)
        });

    public static IReadOnlyList<byte> ExpectedPrimaryByteRouteMap { get; } =
        Array.AsReadOnly(BuildPrimaryByteRouteMap());

    public static IReadOnlyList<uint> PrimaryRouteTargetFileOffsets { get; } =
        Array.AsReadOnly(new uint[]
        {
            0x81CCAD, 0x8217D9, 0x81EE63, 0x81EF6D, 0x81F206, 0x81F2EF, 0x81F34A
        });

    public static MatrixSecondaryCommandInputPath ClassifyLeadingByte(byte leadingByte) =>
        leadingByte == 0 || leadingByte >= 0xF0
            ? MatrixSecondaryCommandInputPath.SecondarySelectorTable
            : leadingByte <= PrimaryByteMapMaximumInput
                ? MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
                : MatrixSecondaryCommandInputPath.ReturnZero;

    public static int? GetPrimaryRouteIndex(byte leadingByte)
    {
        if (ClassifyLeadingByte(leadingByte) != MatrixSecondaryCommandInputPath.PrimaryByteRouteMap)
            return null;

        foreach (var route in PrimaryRouteRanges)
            if (leadingByte >= route.FirstTableIndex && leadingByte <= route.LastTableIndex)
                return route.RouteIndex;
        return null;
    }

    public static uint GetPrimaryTargetFileOffset(int routeIndex)
    {
        if ((uint)routeIndex >= PrimaryRouteTargetFileOffsets.Count)
            throw new ArgumentOutOfRangeException(nameof(routeIndex));
        return PrimaryRouteTargetFileOffsets[routeIndex];
    }

    public static int? GetSecondarySelectorIndex(uint selector) =>
        selector >= SecondarySelectorMinimum && selector <= SecondarySelectorMaximum
            ? (int)(selector - SecondarySelectorMinimum)
            : null;

    private static byte[] BuildPrimaryByteRouteMap()
    {
        var map = new byte[PrimaryByteMapLength];
        foreach (var route in PrimaryRouteRanges)
            Array.Fill(map, (byte)route.RouteIndex, route.FirstTableIndex,
                route.LastTableIndex - route.FirstTableIndex + 1);
        return map;
    }
}

/// <summary>
/// One verified selector case in the secondary dispatcher. Field names stay
/// width/offset based because their semantic meanings are unknown.
/// </summary>
public static class CharacterMarketRequestCompleteNativeLayout
{
    public const int InnerSelector = 0x53;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CCC3;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x524C30;
    public const uint BodyReaderFileOffset = 0x5F6B10;
    public const uint VtableVirtualAddress = 0x1C67278;
    public const string RttiName = ".?AVMarketRequestComplete@Character@firefall@@";
    public const int FirstByteFieldOffset = 0x28;
    public const int RawQwordFieldOffset = 0x30;
    public const int SecondByteFieldOffset = 0x38;
    public const int FirstSharedTextFieldOffset = 0x3C;
    public const int SecondSharedTextFieldOffset = 0x40;
    public const int ThirdSharedTextFieldOffset = 0x44;
}
