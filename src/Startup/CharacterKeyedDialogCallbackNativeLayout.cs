namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterKeyedDialogCallbackVtableSlotSnapshot(
    int SlotOffset,
    uint MethodFileOffset,
    uint MethodVirtualAddress);

public readonly record struct CharacterKeyedDialogCallbackMethodSnapshot(
    int SlotOffset,
    uint MethodFileOffset,
    bool ReceivesOriginalMessagePointer,
    bool ReadsMessagePayload,
    bool WritesCallbackObjectDirectly,
    string ObservedBehavior);

public readonly record struct CharacterKeyedDialogCallbackContractCandidateSnapshot(
    int SlotOffset,
    uint CandidateMethodFileOffset,
    int ExpectedStackArgumentCount,
    int CalleeStackCleanupBytes,
    uint CallerMethodFileOffset,
    uint CallerCallInstructionFileOffset,
    int ObservedCallerStackArgumentPushCount);

public static class CharacterKeyedDialogCallbackNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;

    public const uint StateFactoryMethodFileOffset = 0xAC9350;
    public const uint StateFactorySelectorMapFileOffset = 0xAC9704;
    public const uint StateFactorySelectorMapVirtualAddress = 0xECA304;
    public const uint StateFactoryJumpTableFileOffset = 0xAC96DC;
    public const uint StateFactoryJumpTableVirtualAddress = 0xECA2DC;
    public const uint GenericCharacterStateFactoryBranchFileOffset = 0xAC9483;
    public const uint UnsupportedNpcStateFactoryBranchFileOffset = 0xAC96A8;
    public const uint GenericCharacterAllocationSizeImmediateFileOffset = 0xAC94B3;
    public const uint GenericCharacterConstructorCallFileOffset = 0xAC94D8;
    public const uint GenericCharacterConstructorFileOffset = 0x422E80;
    public const int GenericCharacterAllocationSize = 0x5AE0;
    public const uint GenericCharacterPrimaryVtableVirtualAddress = 0x1C63998;
    public const uint GenericCharacterPrimaryVtableFileOffset = 0x1862598;
    public const string GenericCharacterRttiName = ".?AVtfGenericCharacter@@";
    public const uint GenericCharacterVtableInstallFileOffset = 0x422FEA;

    public const uint GenericCharacterSetupDispatcherFileOffset = 0x4263C0;
    public const uint GenericCharacterSetupSelectorTwoCaseFileOffset = 0x426436;
    public const uint GenericCharacterSetupMethodFileOffset = 0x426840;
    public const uint StateFactorySetupVirtualCallFileOffset = 0x800A7C;

    public const uint BaseControllerPointerSelectionCallbackFileOffset = 0x4206B0;
    public const uint BaseControllerPointerMapCallbackFileOffset = 0x4206A0;
    public const uint BaseControllerPointerRangeCallbackFileOffset = 0x420720;
    public const uint BaseControllerPointerRangeCallSequenceFileOffset = 0x5404B2;
    public const uint BaseControllerPointerRangeCallInstructionFileOffset = 0x5404B8;
    public const uint PointerMapHelperFileOffset = 0x444AA0;
    public const uint PointerMapInterpolationHelperFileOffset = 0x447220;
    public const uint PointerRangeSearchHelperFileOffset = 0x444BF0;
    public const uint ReversePointerRangeCallbackFileOffset = 0x420780;
    public const uint ReversePointerRangeCallSequenceFileOffset = 0x420707;
    public const uint ReversePointerRangeCallInstructionFileOffset = 0x42070A;

    public static IReadOnlyList<CharacterKeyedDialogCallbackVtableSlotSnapshot> VtableSlots { get; } =
        Array.AsReadOnly(new[]
        {
            Slot(0x10, BaseControllerPointerSelectionCallbackFileOffset),
            Slot(0x14, BaseControllerPointerMapCallbackFileOffset),
            Slot(0x18, BaseControllerPointerRangeCallbackFileOffset),
            Slot(0x1C, ReversePointerRangeCallbackFileOffset),
            Slot(0x20, GenericCharacterSetupDispatcherFileOffset)
        });

    public static IReadOnlyList<CharacterKeyedDialogCallbackMethodSnapshot> CallbackMethods { get; } =
        Array.AsReadOnly(new[]
        {
            new CharacterKeyedDialogCallbackMethodSnapshot(
                0x10,
                BaseControllerPointerSelectionCallbackFileOffset,
                ReceivesOriginalMessagePointer: true,
                ReadsMessagePayload: false,
                WritesCallbackObjectDirectly: false,
                "Returns the input, member +0x1BE4, or the result of the +0x08 then +0x1C callback chain."),
            new CharacterKeyedDialogCallbackMethodSnapshot(
                0x14,
                BaseControllerPointerMapCallbackFileOffset,
                ReceivesOriginalMessagePointer: true,
                ReadsMessagePayload: false,
                WritesCallbackObjectDirectly: false,
                "Returns a range-mapped pointer using the mapping state at +0x1BC0."),
            new CharacterKeyedDialogCallbackMethodSnapshot(
                0x18,
                BaseControllerPointerRangeCallbackFileOffset,
                ReceivesOriginalMessagePointer: true,
                ReadsMessagePayload: false,
                WritesCallbackObjectDirectly: false,
                "When the range table at +0x1BE8 is enabled, adds its selected range delta to the input pointer.")
        });

    public static IReadOnlyList<CharacterKeyedDialogCallbackContractCandidateSnapshot> ContractCandidates { get; } =
        Array.AsReadOnly(new[]
        {
            new CharacterKeyedDialogCallbackContractCandidateSnapshot(
                0x18,
                BaseControllerPointerRangeCallbackFileOffset,
                ExpectedStackArgumentCount: 2,
                CalleeStackCleanupBytes: 8,
                CallerMethodFileOffset: 0x540380,
                CallerCallInstructionFileOffset: BaseControllerPointerRangeCallInstructionFileOffset,
                ObservedCallerStackArgumentPushCount: 1),
            new CharacterKeyedDialogCallbackContractCandidateSnapshot(
                0x1C,
                ReversePointerRangeCallbackFileOffset,
                ExpectedStackArgumentCount: 2,
                CalleeStackCleanupBytes: 8,
                CallerMethodFileOffset: BaseControllerPointerSelectionCallbackFileOffset,
                CallerCallInstructionFileOffset: ReversePointerRangeCallInstructionFileOffset,
                ObservedCallerStackArgumentPushCount: 1)
        });

    private static CharacterKeyedDialogCallbackVtableSlotSnapshot Slot(
        int slotOffset,
        uint methodFileOffset) =>
        new(
            slotOffset,
            methodFileOffset,
            methodFileOffset + RawTextOffsetToVirtualAddressDelta);
}
