namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Bounded direct-entry observations for the pooled shader-source control vtable.
/// The projections preserve the header action and deleting dispatch as distinct paths;
/// they do not model allocator internals, ownership, or a generic free operation.
/// </summary>
public static class UiShaderSourceControlReleaseNativeLayout
{
    public const string PinnedBinarySha256 =
        "97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445";

    public const uint ControlVtableVirtualAddress = 0x1C80C8C;
    public const uint ControlVtableRawFileOffset = 0x187F88C;
    public const string ControlVtableSlotPointerBytes =
        "30 D5 6C 01 40 D5 6C 01 B0 D5 6C 01";

    public const uint HeaderReleaseSlotVirtualAddress = 0x16CD530;
    public const uint HeaderReleaseSlotRawFileOffset = 0x12CC930;
    public const string HeaderReleaseSlotBytes =
        "8B 49 0C 85 C9 0F 85 D5 00 00 00 C3";
    public const uint HeaderReleaseTailRawFileOffset = 0x12CCA10;
    public const string HeaderReleaseTailEntryBytes =
        "51 56 57 8B F9 83 7F 08 00 74 17";

    public const uint Slot4DispatchVirtualAddress = 0x16CD540;
    public const uint Slot4DispatchRawFileOffset = 0x12CC940;
    public const string Slot4DispatchBytes =
        "85 C9 74 07 8B 01 6A 01 FF 50 08 C3";
    public const uint Slot4TargetVtableByteOffset = 0x08;
    public const uint Slot4DeletingFlagsArgument = 1;

    public const uint DeletingDispatchVirtualAddress = 0x16CD5B0;
    public const uint DeletingDispatchRawFileOffset = 0x12CC9B0;
    public const string DeletingDispatchBytes =
        "55 8B EC F6 45 08 01 56 8B F1 C7 06 7C 07 C6 01 74 05 E8 99 C3 FF FF 8B C6 5E 5D C2 04 00";
    public const uint DeletingFlagsStackArgumentOffset = 0x08;
    public const uint DeletingFlagsMask = 0x01;
    public const uint ReplacementControlVtableVirtualAddress = 0x1C6077C;
    public const uint ConditionalStorageReleaseCallRawFileOffset = 0x12CC9C2;
    public const uint ControlStorageReleaseHelperRawFileOffset = 0x12C8D60;

    public const int ControlVptrOffset = 0;
    public const int ControlHeaderPointerOffset = 0x0C;

    public static IReadOnlyList<UiShaderSourceControlReleaseInstructionEvidence>
        DirectEntryInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceControlReleaseInstructionEvidence(
                "Read control header pointer, test it, and branch to the header-release tail when nonzero",
                HeaderReleaseSlotRawFileOffset,
                HeaderReleaseSlotBytes,
                ControlRelativeOffset: ControlHeaderPointerOffset),
            new UiShaderSourceControlReleaseInstructionEvidence(
                "Header-release tail entry reached only by the nonzero header-pointer branch",
                HeaderReleaseTailRawFileOffset,
                HeaderReleaseTailEntryBytes),
            new UiShaderSourceControlReleaseInstructionEvidence(
                "Skip slot +8 dispatch for a null receiver; otherwise load its vptr, push one, and call vptr +8",
                Slot4DispatchRawFileOffset,
                Slot4DispatchBytes),
            new UiShaderSourceControlReleaseInstructionEvidence(
                "Replace the control vptr unconditionally, then call the storage-release helper only when deleting bit 0 is set",
                DeletingDispatchRawFileOffset,
                DeletingDispatchBytes,
                ControlRelativeOffset: ControlVptrOffset)
        });

    /// <summary>
    /// Projects only the slot-0 null guard and branch to its header-release tail.
    /// No writes to the header pointer are inferred.
    /// </summary>
    public static UiShaderSourceControlHeaderReleaseProjection ProjectHeaderRelease(
        bool capturedHeaderPointerIsNonNull) =>
        new(capturedHeaderPointerIsNonNull, capturedHeaderPointerIsNonNull);

    /// <summary>
    /// Projects slot +4's null-receiver gate and its fixed deleting-flags argument
    /// to the control's vtable slot +8.
    /// </summary>
    public static UiShaderSourceControlSlot4Projection ProjectSlot4Dispatch(
        bool hasReceiver) =>
        new(
            hasReceiver,
            CallsDeletingDispatch: hasReceiver,
            DeletingFlagsArgument: hasReceiver ? Slot4DeletingFlagsArgument : null);

    /// <summary>
    /// Projects slot +8 for a captured deleting-flags word and an already-reached
    /// valid receiver. The vptr replacement is unconditional; only the helper call
    /// is gated by bit 0.
    /// </summary>
    public static UiShaderSourceControlDeletingDispatchProjection ProjectDeletingDispatch(
        uint deletingFlags) =>
        new(
            deletingFlags,
            ReplacesControlVptr: true,
            CallsStorageReleaseHelper: (deletingFlags & DeletingFlagsMask) != 0);
}

public readonly record struct UiShaderSourceControlReleaseInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ControlRelativeOffset = null);

public readonly record struct UiShaderSourceControlHeaderReleaseProjection(
    bool CapturedHeaderPointerIsNonNull,
    bool BranchesToHeaderReleaseTail);

public readonly record struct UiShaderSourceControlSlot4Projection(
    bool HasReceiver,
    bool CallsDeletingDispatch,
    uint? DeletingFlagsArgument);

public readonly record struct UiShaderSourceControlDeletingDispatchProjection(
    uint DeletingFlags,
    bool ReplacesControlVptr,
    bool CallsStorageReleaseHelper);
