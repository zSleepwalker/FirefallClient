namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterKeyedDialogDispatchCaseSnapshot(
    int FactorySelector,
    string ReceiverRttiName,
    int MessageId,
    uint SelectorMapFileOffset,
    byte SelectorMapValue,
    uint JumpTableEntryFileOffset,
    uint CaseTargetFileOffset,
    uint CaseTargetVirtualAddress,
    bool HasSpecificHandler,
    int? LoggerContextFieldOffset,
    uint? LoggerFormatStringImmediate,
    uint? LoggerMessageLabelImmediate,
    int? CallbackObjectFieldOffset,
    int? CallbackVtableSlotOffset,
    bool ForwardsOriginalMessagePointer,
    bool ReadsMessagePayload);

public static class CharacterKeyedDialogDispatchNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;

    public const int MessageIdFieldOffset = 0x10;
    public const int MessageForwardedToNestedReceiverFieldOffset = 0x24;
    public const int ReceiverOptionalNestedObjectFieldOffset = 0x10;
    public const int ReceiverDispatchGateFieldOffset = 0x2C;
    public const int ReceiverLoggerContextFieldOffset = 0x30;
    public const int DispatchMessageIdMinimum = 0x53;
    public const int DispatchMessageIdMaximum = 0xC4;

    public const uint OuterEnvelopeDispatcherFileOffset = 0x800420;
    public const uint OuterSelectorDefaultBranchFileOffset = 0x800697;
    public const uint OuterSelectorDefaultReceiverCallFileOffset = 0x8008D3;

    public const uint BaseControllerConstructorFileOffset = 0x53F980;
    public const uint BaseControllerPrimaryVtableVirtualAddress = 0x1C66080;
    public const uint BaseControllerMessageDispatcherFileOffset = 0x540380;
    public const uint BaseControllerSelectorMapFileOffset = 0x54119C;
    public const uint BaseControllerSelectorMapVirtualAddress = 0x941D9C;
    public const uint BaseControllerJumpTableFileOffset = 0x54106C;
    public const uint BaseControllerJumpTableVirtualAddress = 0x941C6C;
    public const uint BaseControllerUnsupportedTargetFileOffset = 0x541052;

    public const uint NpcControllerConstructorFileOffset = 0x54E7B0;
    public const uint NpcControllerPrimaryVtableVirtualAddress = 0x1C65F1C;
    public const uint NpcControllerMessageDispatcherFileOffset = 0x54F4A0;
    public const uint NpcControllerSelectorMapFileOffset = 0x55034D;
    public const uint NpcControllerSelectorMapVirtualAddress = 0x950F4D;
    public const uint NpcControllerJumpTableFileOffset = 0x550210;
    public const uint NpcControllerJumpTableVirtualAddress = 0x950E10;
    public const uint NpcControllerUnsupportedTargetFileOffset = 0x5501F6;

    public const uint VibeLogIfSeverityLe2FileOffset = 0x322820;
    public const uint VibeLogUnhandledMessageIdFileOffset = 0x2FC4F0;

    public static IReadOnlyList<CharacterKeyedDialogDispatchCaseSnapshot> Cases { get; } =
        Array.AsReadOnly(new[]
        {
            Case(0x02, ".?AVBaseController@Character@firefall@@", 0x77,
                0x5411C0, 0x08, 0x54108C, 0x540446, 0x941046, true,
                0x1B5D180, 0x1B5BD84, 0x10),
            Case(0x02, ".?AVBaseController@Character@firefall@@", 0x78,
                0x5411C1, 0x09, 0x541090, 0x54046F, 0x94106F, true,
                0x1B5D144, 0x1B5BD8C, 0x14),
            Case(0x02, ".?AVBaseController@Character@firefall@@", 0x79,
                0x5411C2, 0x0A, 0x541094, 0x540498, 0x941098, true,
                0x1B5D138, 0x1B5BD7C, 0x18),
            Case(0x02, ".?AVBaseController@Character@firefall@@", 0x7A,
                0x5411C3, 0x4B, 0x541198, 0x541052, 0x941C52, false,
                null, null, null),

            Case(0x03, ".?AVNPCController@Character@firefall@@", 0x77,
                0x550371, 0x0A, 0x550238, 0x54F58F, 0x95018F, true,
                0x1B5DA6C, 0x1B5BD8C, 0x14),
            Case(0x03, ".?AVNPCController@Character@firefall@@", 0x78,
                0x550372, 0x0B, 0x55023C, 0x54F5B8, 0x9501B8, true,
                0x1B5DA60, 0x1B5BD7C, 0x18),
            Case(0x03, ".?AVNPCController@Character@firefall@@", 0x79,
                0x550373, 0x4E, 0x550348, 0x5501F6, 0x950DF6, false,
                null, null, null),
            Case(0x03, ".?AVNPCController@Character@firefall@@", 0x7A,
                0x550374, 0x4E, 0x550348, 0x5501F6, 0x950DF6, false,
                null, null, null)
        });

    private static CharacterKeyedDialogDispatchCaseSnapshot Case(
        int factorySelector,
        string receiverRttiName,
        int messageId,
        uint selectorMapFileOffset,
        byte selectorMapValue,
        uint jumpTableEntryFileOffset,
        uint caseTargetFileOffset,
        uint caseTargetVirtualAddress,
        bool hasSpecificHandler,
        uint? loggerFormatStringImmediate,
        uint? loggerMessageLabelImmediate,
        int? callbackVtableSlotOffset) =>
        new(
            factorySelector,
            receiverRttiName,
            messageId,
            selectorMapFileOffset,
            selectorMapValue,
            jumpTableEntryFileOffset,
            caseTargetFileOffset,
            caseTargetVirtualAddress,
            hasSpecificHandler,
            hasSpecificHandler ? ReceiverLoggerContextFieldOffset : null,
            loggerFormatStringImmediate,
            loggerMessageLabelImmediate,
            hasSpecificHandler ? ReceiverDispatchGateFieldOffset : null,
            callbackVtableSlotOffset,
            hasSpecificHandler,
            ReadsMessagePayload: false);
}
