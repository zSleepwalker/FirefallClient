namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixKeyedReceiverFactoryCaseSnapshot(
    int Selector,
    uint TableEntryFileOffset,
    uint CaseTargetFileOffset,
    uint CaseTargetVirtualAddress,
    uint AllocationSize,
    uint AllocatorCallFileOffset,
    uint ConstructorCallFileOffset,
    uint ConstructorFileOffset,
    uint PrimaryVtableStoreImmediateFileOffset,
    string PrimaryVtableStoreOpcodeHex,
    uint PrimaryVtableVirtualAddress,
    uint PrimaryVtableFileOffset,
    uint CompleteObjectLocatorVirtualAddress,
    uint TypeDescriptorVirtualAddress,
    string RttiName,
    uint CompleteObjectLocatorSubobjectOffset);

public readonly record struct MatrixKeyedReceiverFactoryDispatchSnapshot(
    int Selector,
    int TableIndex,
    uint TableEntryFileOffset,
    uint TargetFileOffset,
    uint TargetVirtualAddress,
    bool ConstructsReceiver);

public readonly record struct MatrixKeyedReceiverVtableSlotSnapshot(
    int FactorySelector,
    int SlotOffset,
    uint MethodFileOffset,
    uint MethodVirtualAddress,
    string ImplementationObservation);

public readonly record struct MatrixKeyedReceiverCompactSelectorRouteSnapshot(
    int Selector,
    uint DispatcherCaseFileOffset,
    int? ReceiverVtableSlotOffset,
    uint CallSiteFileOffset,
    string ExpectedCallInstructionHex,
    uint? DirectHelperFileOffset);

/// <summary>
/// Bounded mapping of all 52 entries in the QWORD-keyed
/// receiver factory. Constructed types are receivers, not parser messages.
/// </summary>
public static class MatrixKeyedReceiverFactoryNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;
    public const uint FactoryFileOffset = 0x81B1B0;
    public const uint FactoryVirtualAddress = 0xC1BDB0;
    public const uint FactoryByteDispatchFileOffset = 0x81B1DC;
    public const uint OutOfRangeReturnFileOffset = 0x81BA93;
    public const uint JumpTableFileOffset = 0x81BAA8;
    public const uint JumpTableVirtualAddress = 0xC1C6A8;
    public const int JumpTableEntryCount = 0x34;
    public const int JumpTableEntrySize = 4;
    public const int FactoryIndexSubtract = 2;
    public const int FactoryIndexMaximum = 0x33;
    public const int FactorySelectorMinimum = 2;
    public const int FactorySelectorMaximum = 0x35;
    public const int EnvelopeKeyLowDwordFieldOffset = 0x18;
    public const int EnvelopeKeyHighDwordFieldOffset = 0x1C;
    public const int CompactMessageSelectorFieldOffset = 0x10;
    public const uint AllocatorFileOffset = 0x12B7FB0;
    public const uint SharedBaseConstructorHelperFileOffset = 0xBCAD70;
    public const uint SharedBaseVtableVirtualAddress = 0x1C660B4;

    public const uint CompactSelectorDispatcherFileOffset = 0x800420;
    public const uint CompactSelectorJumpTableFileOffset = 0x800970;
    public const uint CompactSelectorJumpTableVirtualAddress = 0xC01570;
    public const int CompactSelectorFirstValue = 1;
    public const int CompactSelectorLastValue = 9;

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> FirstEightCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x02, 0x81BAA8, 0x81B1F2, 0xC1BDF2, 0xB80, 0x81B1F7, 0x81B21B,
                0x53F980, 0x53F9C7, "C706", 0x1C66080, 0x1864C80,
                0x1CA0FB0, 0x1E0869C, ".?AVBaseController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x03, 0x81BAAC, 0x81B230, 0xC1BE30, 0xD80, 0x81B235, 0x81B259,
                0x54E7B0, 0x54E7F7, "C706", 0x1C65F1C, 0x1864B1C,
                0x1CA0FD8, 0x1E086CC, ".?AVNPCController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x04, 0x81BAB0, 0x81B26E, 0xC1BE6E, 0x11D8, 0x81B273, 0x81B297,
                0x55E830, 0x55E887, "C706", 0x1C65EF8, 0x1864AF8,
                0x1CA0F74, 0x1E0862C, ".?AVMissionAndMarkerController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x05, 0x81BAB4, 0x81B2AC, 0xC1BEAC, 0x12C0, 0x81B2B1, 0x81B2D5,
                0x56B970, 0x56B9CA, "C706", 0x1C65ED4, 0x1864AD4,
                0x1CA0F9C, 0x1E08668, ".?AVCombatController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x06, 0x81BAB8, 0x81B2EA, 0xC1BEEA, 0x538, 0x81B2EF, 0x81B313,
                0x577320, 0x57736A, "C703", 0x1C65E8C, 0x1864A8C,
                0x1CA0F88, 0x1E085BC, ".?AVLocalEffectsController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x07, 0x81BABC, 0x81B328, 0xC1BF28, 0xA0, 0x81B32D, 0x81B351,
                0x57FF80, 0x57FFC2, "C706", 0x1C65EB0, 0x1864AB0,
                0x1CA0F4C, 0x1E085F4, ".?AVSpectatorController@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x08, 0x81BAC0, 0x81B366, 0xC1BF66, 0x4F8, 0x81B36B, 0x81B38F,
                0x581070, 0x5810B7, "C706", 0x1C65E2C, 0x1864A2C,
                0x1CA0F60, 0x1E0855C, ".?AVObserverView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x09, 0x81BAC4, 0x81B3A4, 0xC1BFA4, 0x420, 0x81B3A9, 0x81B3CD,
                0x588830, 0x588872, "C706", 0x1C65DF4, 0x18649F4,
                0x1CA0F24, 0x1E0858C, ".?AVEquipmentView@Character@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryDispatchSnapshot> Selectors0ATo11TableEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0A, 8, 0x81BAC8, 0x81B3E2, 0xC1BFE2, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0B, 9, 0x81BACC, 0x81B41D, 0xC1C01D, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0C, 10, 0x81BAD0, 0x81B45B, 0xC1C05B, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0D, 11, 0x81BAD4, 0x81B499, 0xC1C099, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0E, 12, 0x81BAD8, 0x81B4D7, 0xC1C0D7, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x0F, 13, 0x81BADC, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x10, 14, 0x81BAE0, 0x81B515, 0xC1C115, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x11, 15, 0x81BAE4, 0x81BA93, 0xC1C693, false)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryDispatchSnapshot> Selectors12To19TableEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x12, 16, 0x81BAE8, 0x81B553, 0xC1C153, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x13, 17, 0x81BAEC, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x14, 18, 0x81BAF0, 0x81B58E, 0xC1C18E, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x15, 19, 0x81BAF4, 0x81B5CC, 0xC1C1CC, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x16, 20, 0x81BAF8, 0x81B60A, 0xC1C20A, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x17, 21, 0x81BAFC, 0x81B648, 0xC1C248, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x18, 22, 0x81BB00, 0x81B686, 0xC1C286, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x19, 23, 0x81BB04, 0x81B6C4, 0xC1C2C4, true)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryDispatchSnapshot> Selectors1ATo21TableEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1A, 24, 0x81BB08, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1B, 25, 0x81BB0C, 0x81B702, 0xC1C302, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1C, 26, 0x81BB10, 0x81B740, 0xC1C340, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1D, 27, 0x81BB14, 0x81B77E, 0xC1C37E, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1E, 28, 0x81BB18, 0x81B7BC, 0xC1C3BC, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x1F, 29, 0x81BB1C, 0x81B7FA, 0xC1C3FA, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x20, 30, 0x81BB20, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x21, 31, 0x81BB24, 0x81B838, 0xC1C438, true)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryDispatchSnapshot> Selectors22To29TableEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x22, 32, 0x81BB28, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x23, 33, 0x81BB2C, 0x81B873, 0xC1C473, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x24, 34, 0x81BB30, 0x81B8B1, 0xC1C4B1, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x25, 35, 0x81BB34, 0x81B8EC, 0xC1C4EC, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x26, 36, 0x81BB38, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x27, 37, 0x81BB3C, 0x81B927, 0xC1C527, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x28, 38, 0x81BB40, 0x81B965, 0xC1C565, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x29, 39, 0x81BB44, 0x81BA93, 0xC1C693, false)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryDispatchSnapshot> Selectors2ATo35TableEntries { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2A, 40, 0x81BB48, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2B, 41, 0x81BB4C, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2C, 42, 0x81BB50, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2D, 43, 0x81BB54, 0x81B9A3, 0xC1C5A3, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2E, 44, 0x81BB58, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x2F, 45, 0x81BB5C, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x30, 46, 0x81BB60, 0x81B9E1, 0xC1C5E1, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x31, 47, 0x81BB64, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x32, 48, 0x81BB68, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x33, 49, 0x81BB6C, 0x81BA1F, 0xC1C61F, true),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x34, 50, 0x81BB70, 0x81BA93, 0xC1C693, false),
            new MatrixKeyedReceiverFactoryDispatchSnapshot(0x35, 51, 0x81BB74, 0x81BA59, 0xC1C659, true)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> NextSixCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x0A, 0x81BAC8, 0x81B3E2, 0xC1BFE2, 0x58, 0x81B3E4, 0x81B408,
                0x58A530, 0x58A576, "C706", 0x1C65DAC, 0x18649AC,
                0x1CA0F10, 0x1E08500, ".?AVAIObserverView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x0B, 0x81BACC, 0x81B41D, 0xC1C01D, 0x1258, 0x81B422, 0x81B446,
                0x58B5F0, 0x58B64A, "C706", 0x1C65D88, 0x1864988,
                0x1CA0F38, 0x1E08530, ".?AVCombatView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x0C, 0x81BAD0, 0x81B45B, 0xC1C05B, 0xA8, 0x81B460, 0x81B484,
                0x595680, 0x5956C6, "C706", 0x1C65D58, 0x1864958,
                0x1CA0ED4, 0x1E084A0, ".?AVMovementView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x0D, 0x81BAD4, 0x81B499, 0xC1C099, 0x438, 0x81B49E, 0x81B4C2,
                0x5962A0, 0x5962EB, "C706", 0x1C65D10, 0x1864910,
                0x1CA0EFC, 0x1E084D0, ".?AVTinyObjectView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x0E, 0x81BAD8, 0x81B4D7, 0xC1C0D7, 0x1F8, 0x81B4DC, 0x81B500,
                0x59AEB0, 0x59AEF8, "C706", 0x1C65D34, 0x1864934,
                0x1CA0EE8, 0x1E08468, ".?AVDynamicProjectileView@Character@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x10, 0x81BAE0, 0x81B515, 0xC1C115, 0x88, 0x81B51A, 0x81B53E,
                0xAD9580, 0xAD95C2, "C706", 0x1C723D8, 0x1870FD8,
                0x1CCAA14, 0x1E2A260, ".?AVObserverView@Melding@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> Selectors12To19ConstructingCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x12, 0x81BAE8, 0x81B553, 0xC1C153, 0x58, 0x81B555, 0x81B579,
                0xADA400, 0xADA446, "C706", 0x1C7239C, 0x1870F9C,
                0x1CCAA00, 0x1E2A2C8, ".?AVObserverView@MeldingBubble@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x14, 0x81BAF0, 0x81B58E, 0xC1C18E, 0x158, 0x81B593, 0x81B5B7,
                0xADBCE0, 0xADBD28, "C706", 0x1C72378, 0x1870F78,
                0x1CCA988, 0x1E2A048, ".?AVObserverView@AreaVisualData@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x15, 0x81BAF4, 0x81B5CC, 0xC1C1CC, 0x628, 0x81B5D1, 0x81B5F5,
                0xAE10D0, 0xAE1127, "C706", 0x1C72334, 0x1870F34,
                0x1CCA94C, 0x1E2A108, ".?AVParticleEffectsView@AreaVisualData@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x16, 0x81BAF8, 0x81B60A, 0xC1C20A, 0x760, 0x81B60F, 0x81B633,
                0xAE42E0, 0xAE4337, "C706", 0x1C722EC, 0x1870EEC,
                0x1CCA924, 0x1E2A0D4, ".?AVMapMarkerView@AreaVisualData@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x17, 0x81BAFC, 0x81B648, 0xC1C248, 0x438, 0x81B64D, 0x81B671,
                0xAE9640, 0xAE968B, "C706", 0x1C72310, 0x1870F10,
                0x1CCA938, 0x1E29FA8, ".?AVTinyObjectView@AreaVisualData@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x18, 0x81BB00, 0x81B686, 0xC1C286, 0x758, 0x81B68B, 0x81B6AF,
                0xAEE240, 0xAEE297, "C706", 0x1C722C8, 0x1870EC8,
                0x1CCA974, 0x1E29F70, ".?AVLootObjectView@AreaVisualData@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x19, 0x81BB04, 0x81B6C4, 0xC1C2C4, 0x5C8, 0x81B6C9, 0x81B6ED,
                0xAF1C00, 0xAF1C4B, "C706", 0x1C722A4, 0x1870EA4,
                0x1CCA960, 0x1E2A010, ".?AVForceShieldView@AreaVisualData@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> Selectors1ATo21ConstructingCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x1B, 0x81BB0C, 0x81B702, 0xC1C302, 0x570, 0x81B707, 0x81B72B,
                0xB0A990, 0xB0A9D8, "C706", 0x1C72868, 0x1871468,
                0x1CCBC20, 0x1E2AA88, ".?AVBaseController@Vehicle@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x1C, 0x81BB10, 0x81B740, 0xC1C340, 0x1120, 0x81B745, 0x81B769,
                0xB12490, 0xB124EA, "C706", 0x1C72824, 0x1871424,
                0x1CCBC5C, 0x1E2AB30, ".?AVCombatController@Vehicle@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x1D, 0x81BB14, 0x81B77E, 0xC1C37E, 0x478, 0x81B783, 0x81B7A7,
                0xB19380, 0xB193C8, "C706", 0x1C727E8, 0x18713E8,
                0x1CCBC70, 0x1E2AB04, ".?AVObserverView@Vehicle@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x1E, 0x81BB18, 0x81B7BC, 0xC1C3BC, 0x10F8, 0x81B7C1, 0x81B7E5,
                0xB204A0, 0xB204FA, "C706", 0x1C727C4, 0x18713C4,
                0x1CCBC48, 0x1E2A9F0, ".?AVCombatView@Vehicle@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x1F, 0x81BB1C, 0x81B7FA, 0xC1C3FA, 0xB8, 0x81B7FF, 0x81B823,
                0xB26D40, 0xB26D86, "C706", 0x1C72784, 0x1871384,
                0x1CCBC0C, 0x1E2A9C4, ".?AVMovementView@Vehicle@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x21, 0x81BB24, 0x81B838, 0xC1C438, 0x78, 0x81B83A, 0x81B85E,
                0xAF6AE0, 0xAF6B22, "C706", 0x1C72254, 0x1870E54,
                0x1CCA8FC, 0x1E29FE0, ".?AVAIObserverView@Anchor@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> Selectors22To29ConstructingCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x23, 0x81BB2C, 0x81B873, 0xC1C473, 0x1710, 0x81B878, 0x81B89C,
                0xB281A0, 0xB281F2, "C706", 0x1C72730, 0x1871330,
                0x1CCBB94, 0x1E2A824, ".?AVObserverView@Deployable@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x24, 0x81BB30, 0x81B8B1, 0xC1C4B1, 0x48, 0x81B8B3, 0x81B8D7,
                0xB34FC0, 0xB35002, "C706", 0x1C72700, 0x1871300,
                0x1CCBB80, 0x1E2A7F0, ".?AVNPCObserverView@Deployable@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x25, 0x81BB34, 0x81B8EC, 0xC1C4EC, 0x48, 0x81B8EE, 0x81B912,
                0xB35CC0, 0xB35D02, "C706", 0x1C726C4, 0x18712C4,
                0x1CCBB44, 0x1E2A888, ".?AVHardpointView@Deployable@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x27, 0x81BB3C, 0x81B927, 0xC1C527, 0x128, 0x81B92C, 0x81B950,
                0xB37030, 0xB37074, "C706", 0x1C72648, 0x1871248,
                0x1CCBA68, 0x1E2A574, ".?AVBaseController@Turret@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x28, 0x81BB40, 0x81B965, 0xC1C565, 0xD8, 0x81B96A, 0x81B98E,
                0xB39900, 0xB39942, "C706", 0x1C72618, 0x1871218,
                0x1CCBA2C, 0x1E2A424, ".?AVObserverView@Turret@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverFactoryCaseSnapshot> Selectors2ATo35ConstructingCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x2D, 0x81BB54, 0x81B9A3, 0xC1C5A3, 0x100, 0x81B9A8, 0x81B9CC,
                0xAF7300, 0xAF7344, "C706", 0x1C72278, 0x1870E78,
                0x1CCA910, 0x1E29EC0, ".?AVObserverView@Outpost@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x30, 0x81BB60, 0x81B9E1, 0xC1C5E1, 0x11A0, 0x81B9E6, 0x81BA0A,
                0xB3C0F0, 0xB3C135, "C706", 0x1C725E4, 0x18711E4,
                0x1CCBA18, 0x1E2A3F0, ".?AVObserverView@ResourceNode@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x33, 0x81BB6C, 0x81BA1F, 0xC1C61F, 0x1690, 0x81BA24, 0x81BA44,
                0xAFC100, 0xAFC142, "C706", 0x1C721E0, 0x1870DE0,
                0x1CCA8A0, 0x1E29EEC, ".?AVObserverView@CarryableObject@firefall@@", 0),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x35, 0x81BB74, 0x81BA59, 0xC1C659, 0x758, 0x81BA5E, 0x81BA7E,
                0xB43B10, 0xB43B67, "C706", 0x1C72574, 0x1871174,
                0x1CCBA04, 0x1E2A3B4, ".?AVLootObjectView@LootStoreExtension@firefall@@", 0)
        });

    public static IReadOnlyList<MatrixKeyedReceiverVtableSlotSnapshot> PrimaryVtableSlots { get; } =
        Array.AsReadOnly(new[]
        {
            Slot(0x02, 0x04, 0x540380, "Message-ID dispatcher for 0x53..0xC4, gated by the receiver state flag."),
            Slot(0x02, 0x08, 0x541220, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x02, 0x0C, 0x5441B0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x02, 0x18, 0x54DE00, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x03, 0x04, 0x54F4A0, "Message-ID dispatcher for 0x53..0xC4, gated by the receiver state flag."),
            Slot(0x03, 0x08, 0x5503D0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x03, 0x0C, 0x5536C0, "Code prologue is present; IDA did not recover this method boundary or body."),
            Slot(0x03, 0x18, 0x55E160, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x04, 0x04, 0x55EAA0, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x04, 0x08, 0x55EB10, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x04, 0x0C, 0x561CF0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x04, 0x18, 0x56B310, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x05, 0x04, 0x56BC20, "Message-ID dispatcher for 0x56..0xA8, gated by the receiver state flag."),
            Slot(0x05, 0x08, 0x56C0D0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x05, 0x0C, 0x56F3A0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x05, 0x18, 0x576C80, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x06, 0x04, 0x577410, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x06, 0x08, 0x577480, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x06, 0x0C, 0x5795A0, "Code prologue is present; IDA did not recover this method boundary or body."),
            Slot(0x06, 0x18, 0x57FBD0, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x07, 0x04, 0x5800E0, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x07, 0x08, 0x580150, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x07, 0x0C, 0x580470, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x07, 0x18, 0x580F40, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x08, 0x04, 0x581480, "Message-ID dispatcher for 0x5B..0xAB, gated by the receiver state flag."),
            Slot(0x08, 0x08, 0x5817D0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x08, 0x0C, 0x5835C0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x08, 0x18, 0x588500, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x09, 0x04, 0x5889F0, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x09, 0x08, 0x588A90, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x09, 0x0C, 0x589040, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x09, 0x18, 0x58A440, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x0A, 0x04, 0x58A620, "Message-ID dispatcher for 0x69; invalid IDs call the ID-bearing logger, while the valid path logs received msg-105 and may forward to a nested receiver."),
            Slot(0x0A, 0x08, 0x58A6E0, "Compact-selector body handler target; IDA recovers the disassembly boundary, but decompilation and payload semantics remain unresolved."),
            Slot(0x0A, 0x0C, 0x58AA90, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0A, 0x18, 0x58B4C0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x0B, 0x04, 0x58B850, "Message-ID dispatcher for 0x54..0x6C; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x0B, 0x08, 0x58BA50, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x0B, 0x0C, 0x58EA40, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0B, 0x18, 0x595090, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x0C, 0x04, 0x595760, "Message-ID dispatcher for 0x6E; invalid IDs call the ID-bearing logger, while the valid path logs received msg-110 and may forward to a nested receiver."),
            Slot(0x0C, 0x08, 0x595820, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0C, 0x0C, 0x595E30, "Compact-selector body handler called with a WORD-data pointer and length; body not exhaustively mapped."),
            Slot(0x0C, 0x18, 0x596250, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x0D, 0x04, 0x5963D0, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; logger call has no message-ID argument."),
            Slot(0x0D, 0x08, 0x596470, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0D, 0x0C, 0x597590, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0D, 0x18, 0x59ACA0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x0E, 0x04, 0x59AFC0, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; logger call has no message-ID argument."),
            Slot(0x0E, 0x08, 0x59B060, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x0E, 0x0C, 0x59C600, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x0E, 0x18, 0x59FF40, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x10, 0x04, 0xAD9790, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; logger call has no message-ID argument."),
            Slot(0x10, 0x08, 0xAD9830, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x10, 0x0C, 0xAD9BD0, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x10, 0x18, 0xADA370, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x12, 0x04, 0xADA4D0, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x12, 0x08, 0xADA570, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x12, 0x0C, 0xADAB00, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x12, 0x18, 0xADBA10, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x14, 0x04, 0xADBE80, "Message-ID dispatcher for exactly 0x54; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x14, 0x08, 0xADBF40, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x14, 0x0C, 0xADD790, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x14, 0x18, 0xAE0EF0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x15, 0x04, 0xAE1280, "Message-ID dispatcher for exactly 0x55; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x15, 0x08, 0xAE1340, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x15, 0x0C, 0xAE1E80, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x15, 0x18, 0xAE4170, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x16, 0x04, 0xAE44A0, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x16, 0x08, 0xAE4540, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x16, 0x0C, 0xAE5970, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x16, 0x18, 0xAE9410, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x17, 0x04, 0xAE9770, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x17, 0x08, 0xAE9810, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x17, 0x0C, 0xAEA930, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x17, 0x18, 0xAEE040, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x18, 0x04, 0xAEE3F0, "Message-ID dispatcher for exactly 0x53; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x18, 0x08, 0xAEE4B0, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x18, 0x0C, 0xAEF200, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x18, 0x18, 0xAF1A60, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x19, 0x04, 0xAF1D30, "Optional nested-receiver forwarder with conditional fixed 'this view doesn't have any valid events' logging; no message-ID argument."),
            Slot(0x19, 0x08, 0xAF1DD0, "Compact-selector body handler called with a byte-data pointer and length; body not exhaustively mapped."),
            Slot(0x19, 0x0C, 0xAF3170, "Compact-selector body handler called with a data pointer and length; body not exhaustively mapped."),
            Slot(0x19, 0x18, 0xAF68C0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x1B, 0x04, 0xB0AD20, "Message-ID dispatcher for exactly 0x5B, gated by receiver +0x2C; it forwards to an optional receiver at +0x10 before the ID gate and logs invalid IDs with message +0x10."),
            Slot(0x1B, 0x08, 0xB0ADB0, "Compact-selector 1 body handler; consumes a byte-data pointer and length, dispatches byte values through 0xC1, and logs values above that bound; body not exhaustively mapped."),
            Slot(0x1B, 0x0C, 0xB0CE40, "Compact-selector 3/4 body handler called with data pointer and length; it may forward to a nested receiver at +0x10; body not exhaustively mapped."),
            Slot(0x1B, 0x18, 0xB120B0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x1C, 0x04, 0xB12650, "Message-ID dispatcher for 0x53..0x5A, gated by receiver +0x2C; forwards to a nested receiver at +0x10 and logs invalid IDs with message +0x10."),
            Slot(0x1C, 0x08, 0xB127E0, "Compact-selector 1 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1C, 0x0C, 0xB14E20, "Compact-selector 3/4 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1C, 0x18, 0xB18EB0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x1D, 0x04, 0xB196E0, "Message-ID dispatcher for exactly 0x55; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10, while the valid path logs received msg-85 and may forward to receiver +0x2C."),
            Slot(0x1D, 0x08, 0xB197A0, "Compact-selector 1 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1D, 0x0C, 0xB1B630, "Compact-selector 3/4 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1D, 0x18, 0xB20100, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x1E, 0x04, 0xB20660, "Message-ID dispatcher for exactly 0x57; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10, while the valid path logs received msg-87 and may forward to receiver +0x2C."),
            Slot(0x1E, 0x08, 0xB20720, "Compact-selector 1 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1E, 0x0C, 0xB22A80, "Compact-selector 3/4 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1E, 0x18, 0xB26990, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x1F, 0x04, 0xB26E50, "Message-ID dispatcher for exactly 0x56; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10, while the valid path logs received msg-86 and may forward to receiver +0x2C."),
            Slot(0x1F, 0x08, 0xB26F20, "Compact-selector 1 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1F, 0x0C, 0xB27670, "Compact-selector 3/4 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x1F, 0x18, 0xB27E20, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x21, 0x04, 0xAF6C40, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x21, 0x08, 0xAF6CE0, "Compact-selector 1 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x21, 0x0C, 0xAF6E50, "Compact-selector 3/4 body handler called with data pointer and length; body not exhaustively mapped."),
            Slot(0x21, 0x18, 0xAF7260, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x23, 0x04, 0xB285C0, "Message-ID dispatcher for exactly 0x54 and 0x55; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x23, 0x08, 0xB286B0, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x23, 0x0C, 0xB2BCF0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x23, 0x18, 0xB34910, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x24, 0x04, 0xB35130, "Message-ID dispatcher for exactly 0x53; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x24, 0x08, 0xB35200, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x24, 0x0C, 0xB354B0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x24, 0x18, 0xB35C40, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x25, 0x04, 0xB35DB0, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x25, 0x08, 0xB35E50, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x25, 0x0C, 0xB36010, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x25, 0x18, 0xB364C0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x27, 0x04, 0xB37220, "Optional nested-receiver forwarder with the fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x27, 0x08, 0xB37290, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x27, 0x0C, 0xB37AA0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x27, 0x18, 0xB397C0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x28, 0x04, 0xB39AA0, "Message-ID dispatcher for exactly 0x53; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10."),
            Slot(0x28, 0x08, 0xB39B60, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x28, 0x0C, 0xB3A450, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x28, 0x18, 0xB3BFA0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x2D, 0x04, 0xAF7420, "Optional nested-receiver forwarder with a conditional fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x2D, 0x08, 0xAF74C0, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x2D, 0x0C, 0xAF8DC0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x2D, 0x18, 0xAFBBF0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x30, 0x04, 0xB3C320, "Optional nested-receiver forwarder with a conditional fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x30, 0x08, 0xB3C3C0, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x30, 0x0C, 0xB3EAB0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x30, 0x18, 0xB43640, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x33, 0x04, 0xAFC430, "Optional nested-receiver forwarder with a conditional fixed 'this view doesn't have any valid events' log; no message-ID argument."),
            Slot(0x33, 0x08, 0xAFC4D0, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x33, 0x0C, 0xAFF670, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x33, 0x18, 0xB070E0, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0."),
            Slot(0x35, 0x04, 0xB43CC0, "Message-ID dispatcher for exactly 0x53; invalid IDs call VIBE_LogUnhandledMessageId with message +0x10, while the valid path may log received msg-83 and forward to a nested receiver."),
            Slot(0x35, 0x08, 0xB43D80, "Compact-selector body handler reached through receiver slot +0x08 with data pointer and length; body not exhaustively mapped."),
            Slot(0x35, 0x0C, 0xB44AD0, "Compact-selector body handler reached through receiver slot +0x0C with data pointer and length; body not exhaustively mapped."),
            Slot(0x35, 0x18, 0xB47330, "State-refresh wrapper; stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0.")
        });

    public static IReadOnlyList<MatrixKeyedReceiverCompactSelectorRouteSnapshot> CompactSelectorRoutes { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(1, 0x8006A8, 0x08, 0x8006BD, "FF5208", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(2, 0x8006CE, 0x18, 0x800703, "FF5218", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(3, 0x8007AD, 0x0C, 0x800815, "FF520C", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(4, 0x800839, 0x0C, 0x800874, "FF520C", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(5, 0x80088E, null, 0x8008A7, "E854FAFFFF", 0x800300),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(6, 0x80088E, null, 0x8008A7, "E854FAFFFF", 0x800300),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(7, 0x8008D3, 0x04, 0x8008D8, "FF5004", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(8, 0x8008D3, 0x04, 0x8008D8, "FF5004", null),
            new MatrixKeyedReceiverCompactSelectorRouteSnapshot(9, 0x8008B4, 0x18, 0x8008CE, "FF5218", null)
        });

    private static MatrixKeyedReceiverVtableSlotSnapshot Slot(
        int selector,
        int slotOffset,
        uint methodFileOffset,
        string implementationObservation) =>
        new(
            selector,
            slotOffset,
            methodFileOffset,
            methodFileOffset + RawTextOffsetToVirtualAddressDelta,
            implementationObservation);
}
