namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Bounded instruction evidence for the UI shader-source wrapper destructor.
/// The counter predicates model the flags produced by the observed instructions;
/// they do not assign global ownership semantics to the referenced objects.
/// </summary>
public static class UiShaderSourceDestructorNativeLayout
{
    public const uint WrapperVtableVirtualAddress = 0x1C80CC4;
    public const uint WrapperVtableRawFileOffset = 0x187F8C4;
    public const string WrapperVtableSlot0Bytes = "A0 C3 6C 01";
    public const uint DestructorEntryVirtualAddress = 0x16CC3A0;
    public const uint DestructorEntryRawFileOffset = 0x12CB7A0;
    public const uint CodeVirtualAddressDelta = 0x400C00;
    public const uint DestructorLength = 0x50;

    public const int WrapperVptrOffset = 0x00;
    public const int WrapperBackingPointerOffset = 0x04;
    public const int WrapperControlPointerOffset = 0x08;
    public const int ControlFirstCounterOffset = 0x04;
    public const int ControlSecondCounterOffset = 0x08;
    public const uint DeletingFlagStackArgumentOffset = 0x08;
    public const uint DeletingFlagMask = 0x01;
    public const uint ReplacementVptrVirtualAddress = 0x1C5FC14;

    public const uint ControlPointerReadRawFileOffset = 0x12CB7A7;
    public const uint ControlPointerTestRawFileOffset = 0x12CB7AA;
    public const uint ControlPointerNullBranchRawFileOffset = 0x12CB7AC;
    public const uint FirstCounterXaddRawFileOffset = 0x12CB7B7;
    public const uint FirstCounterJnzRawFileOffset = 0x12CB7BB;
    public const uint ControlVtableSlot0CallRawFileOffset = 0x12CB7C1;
    public const uint SecondCounterXaddRawFileOffset = 0x12CB7C6;
    public const uint SecondCounterDecRawFileOffset = 0x12CB7CA;
    public const uint SecondCounterJnzRawFileOffset = 0x12CB7CB;
    public const uint ControlVtableSlot4CallRawFileOffset = 0x12CB7D1;
    public const uint DeletingFlagTestRawFileOffset = 0x12CB7D5;
    public const uint ReplacementVptrStoreRawFileOffset = 0x12CB7D9;
    public const uint DeletingFlagJzRawFileOffset = 0x12CB7DF;
    public const uint WrapperStorageReleaseThisSetupRawFileOffset = 0x12CB7E1;
    public const uint ConditionalWrapperStorageReleaseCallRawFileOffset = 0x12CB7E3;
    public const uint ReturnRawFileOffset = 0x12CB7ED;
    public const uint WrapperStorageReleaseHelperRawFileOffset = 0x12C8D60;

    public static IReadOnlyList<UiShaderSourceDestructorInstructionEvidence> ControlReleaseInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceDestructorInstructionEvidence(
                "Read wrapper control pointer",
                ControlPointerReadRawFileOffset,
                "8B 7E 08",
                WrapperRelativeOffset: WrapperControlPointerOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Test control pointer and branch if null",
                ControlPointerTestRawFileOffset,
                "85 FF 74 27",
                WrapperRelativeOffset: WrapperControlPointerOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Atomic decrement at control +4; XADD sets flags from the post-decrement sum",
                FirstCounterXaddRawFileOffset,
                "F0 0F C1 02",
                ControlRelativeOffset: ControlFirstCounterOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Immediately following JNZ skips slot 0 when the post-decrement sum is nonzero",
                FirstCounterJnzRawFileOffset,
                "75 17",
                ControlRelativeOffset: ControlFirstCounterOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Call control vtable slot 0 when that JNZ is not taken",
                ControlVtableSlot0CallRawFileOffset,
                "FF 10"),
            new UiShaderSourceDestructorInstructionEvidence(
                "Atomic decrement at control +8; XADD returns the prior value in EBX",
                SecondCounterXaddRawFileOffset,
                "F0 0F C1 18",
                ControlRelativeOffset: ControlSecondCounterOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "DEC EBX overwrites XADD flags using the returned prior value minus one",
                SecondCounterDecRawFileOffset,
                "4B",
                ControlRelativeOffset: ControlSecondCounterOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Following JNZ skips slot +4 when the returned prior value was not one",
                SecondCounterJnzRawFileOffset,
                "75 07",
                ControlRelativeOffset: ControlSecondCounterOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Call control vtable slot +4 when DEC produced zero",
                ControlVtableSlot4CallRawFileOffset,
                "FF 50 04")
        });

    public static IReadOnlyList<UiShaderSourceDestructorInstructionEvidence> WrapperDispositionInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceDestructorInstructionEvidence(
                "Test deleting-flag bit 0",
                DeletingFlagTestRawFileOffset,
                "F6 45 08 01"),
            new UiShaderSourceDestructorInstructionEvidence(
                "Replace wrapper vptr",
                ReplacementVptrStoreRawFileOffset,
                "C7 06 14 FC C5 01",
                WrapperRelativeOffset: WrapperVptrOffset),
            new UiShaderSourceDestructorInstructionEvidence(
                "Branch on the deleting-flag test",
                DeletingFlagJzRawFileOffset,
                "74 07"),
            new UiShaderSourceDestructorInstructionEvidence(
                "Set ECX to the wrapper before the conditional call",
                WrapperStorageReleaseThisSetupRawFileOffset,
                "8B CE"),
            new UiShaderSourceDestructorInstructionEvidence(
                "Call wrapper storage-release helper when deleting-flag bit 0 is set",
                ConditionalWrapperStorageReleaseCallRawFileOffset,
                "E8 78 D5 FF FF"),
            new UiShaderSourceDestructorInstructionEvidence(
                "Return and pop the deleting-flag stack argument",
                ReturnRawFileOffset,
                "C2 04 00")
        });

    /// <summary>
    /// Projects this destructor's counter branches and direct wrapper disposition for a captured
    /// instruction path, assuming reached indirect callbacks return. Counter inputs are required
    /// only when the corresponding instructions run; callback effects are not modeled.
    /// </summary>
    public static UiShaderSourceDestructorProjection Project(
        bool hasControlPointer,
        uint? controlFirstCounterBeforeXadd,
        uint? controlSecondCounterBeforeXadd,
        uint deletingFlags)
    {
        if (!hasControlPointer)
        {
            if (controlFirstCounterBeforeXadd.HasValue || controlSecondCounterBeforeXadd.HasValue)
                throw new ArgumentException(
                    "A null control pointer cannot have captured control-counter values.");

            return new UiShaderSourceDestructorProjection(
                HasControlPointer: false,
                ControlFirstCounterAfterXadd: null,
                CallsControlVtableSlot0: false,
                CallsControlVtableSlot4: false,
                ReplacesWrapperVptr: true,
                CallsWrapperStorageReleaseHelper: (deletingFlags & DeletingFlagMask) != 0,
                DirectlyClearsWrapperBackingPointer: false,
                DirectlyClearsWrapperControlPointer: false);
        }

        if (!controlFirstCounterBeforeXadd.HasValue)
            throw new ArgumentException(
                "A nonnull control pointer requires its captured +4 counter value.",
                nameof(controlFirstCounterBeforeXadd));

        uint firstCounterAfterXadd = unchecked(controlFirstCounterBeforeXadd.Value - 1);
        bool callsControlVtableSlot0 = firstCounterAfterXadd == 0;
        if (callsControlVtableSlot0 != controlSecondCounterBeforeXadd.HasValue)
            throw new ArgumentException(
                "The control +8 counter is captured only when the first post-decrement sum is zero.",
                nameof(controlSecondCounterBeforeXadd));

        bool callsControlVtableSlot4 =
            callsControlVtableSlot0 && unchecked(controlSecondCounterBeforeXadd!.Value - 1) == 0;

        return new UiShaderSourceDestructorProjection(
            HasControlPointer: true,
            ControlFirstCounterAfterXadd: firstCounterAfterXadd,
            CallsControlVtableSlot0: callsControlVtableSlot0,
            CallsControlVtableSlot4: callsControlVtableSlot4,
            ReplacesWrapperVptr: true,
            CallsWrapperStorageReleaseHelper: (deletingFlags & DeletingFlagMask) != 0,
            DirectlyClearsWrapperBackingPointer: false,
            DirectlyClearsWrapperControlPointer: false);
    }
}

public readonly record struct UiShaderSourceDestructorInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? WrapperRelativeOffset = null,
    int? ControlRelativeOffset = null);

public readonly record struct UiShaderSourceDestructorProjection(
    bool HasControlPointer,
    uint? ControlFirstCounterAfterXadd,
    bool CallsControlVtableSlot0,
    bool CallsControlVtableSlot4,
    bool ReplacesWrapperVptr,
    bool CallsWrapperStorageReleaseHelper,
    bool DirectlyClearsWrapperBackingPointer,
    bool DirectlyClearsWrapperControlPointer);
