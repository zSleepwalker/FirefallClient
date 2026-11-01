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
    string RttiName);

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
/// Bounded mapping of the first eight entries in the QWORD-keyed receiver
/// factory. These entries construct Character controllers, not parser messages.
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
                0x1CA0FB0, 0x1E0869C, ".?AVBaseController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x03, 0x81BAAC, 0x81B230, 0xC1BE30, 0xD80, 0x81B235, 0x81B259,
                0x54E7B0, 0x54E7F7, "C706", 0x1C65F1C, 0x1864B1C,
                0x1CA0FD8, 0x1E086CC, ".?AVNPCController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x04, 0x81BAB0, 0x81B26E, 0xC1BE6E, 0x11D8, 0x81B273, 0x81B297,
                0x55E830, 0x55E887, "C706", 0x1C65EF8, 0x1864AF8,
                0x1CA0F74, 0x1E0862C, ".?AVMissionAndMarkerController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x05, 0x81BAB4, 0x81B2AC, 0xC1BEAC, 0x12C0, 0x81B2B1, 0x81B2D5,
                0x56B970, 0x56B9CA, "C706", 0x1C65ED4, 0x1864AD4,
                0x1CA0F9C, 0x1E08668, ".?AVCombatController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x06, 0x81BAB8, 0x81B2EA, 0xC1BEEA, 0x538, 0x81B2EF, 0x81B313,
                0x577320, 0x57736A, "C703", 0x1C65E8C, 0x1864A8C,
                0x1CA0F88, 0x1E085BC, ".?AVLocalEffectsController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x07, 0x81BABC, 0x81B328, 0xC1BF28, 0xA0, 0x81B32D, 0x81B351,
                0x57FF80, 0x57FFC2, "C706", 0x1C65EB0, 0x1864AB0,
                0x1CA0F4C, 0x1E085F4, ".?AVSpectatorController@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x08, 0x81BAC0, 0x81B366, 0xC1BF66, 0x4F8, 0x81B36B, 0x81B38F,
                0x581070, 0x5810B7, "C706", 0x1C65E2C, 0x1864A2C,
                0x1CA0F60, 0x1E0855C, ".?AVObserverView@Character@firefall@@"),
            new MatrixKeyedReceiverFactoryCaseSnapshot(
                0x09, 0x81BAC4, 0x81B3A4, 0xC1BFA4, 0x420, 0x81B3A9, 0x81B3CD,
                0x588830, 0x588872, "C706", 0x1C65DF4, 0x18649F4,
                0x1CA0F24, 0x1E0858C, ".?AVEquipmentView@Character@firefall@@")
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
            Slot(0x04, 0x04, 0x55EAA0, "Optional nested-receiver forwarder with conditional unhandled-message logging."),
            Slot(0x04, 0x08, 0x55EB10, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x04, 0x0C, 0x561CF0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x04, 0x18, 0x56B310, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x05, 0x04, 0x56BC20, "Message-ID dispatcher for 0x56..0xA8, gated by the receiver state flag."),
            Slot(0x05, 0x08, 0x56C0D0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x05, 0x0C, 0x56F3A0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x05, 0x18, 0x576C80, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x06, 0x04, 0x577410, "Optional nested-receiver forwarder with conditional unhandled-message logging."),
            Slot(0x06, 0x08, 0x577480, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x06, 0x0C, 0x5795A0, "Code prologue is present; IDA did not recover this method boundary or body."),
            Slot(0x06, 0x18, 0x57FBD0, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x07, 0x04, 0x5800E0, "Optional nested-receiver forwarder with conditional unhandled-message logging."),
            Slot(0x07, 0x08, 0x580150, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x07, 0x0C, 0x580470, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x07, 0x18, 0x580F40, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x08, 0x04, 0x581480, "Message-ID dispatcher for 0x5B..0xAB, gated by the receiver state flag."),
            Slot(0x08, 0x08, 0x5817D0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x08, 0x0C, 0x5835C0, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x08, 0x18, 0x588500, "State-refresh wrapper; ignores payload arguments but is not a no-op."),
            Slot(0x09, 0x04, 0x5889F0, "Optional nested-receiver forwarder with conditional unhandled-message logging."),
            Slot(0x09, 0x08, 0x588A90, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x09, 0x0C, 0x589040, "Called with parsed byte data and length; body not exhaustively mapped."),
            Slot(0x09, 0x18, 0x58A440, "State-refresh wrapper; ignores payload arguments but is not a no-op.")
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
