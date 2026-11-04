namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static wrapper and backing-record layout anchors. The wrapper's virtual slots and backing-record fields
/// are named separately even where their numeric offsets happen to match. Preserve-path instruction
/// observations are branch-specific. The
/// captured null new-header case is conditional, not an unconditional native failure, and does not establish
/// that the path executes on allocation failure.
/// </summary>
public static class UiShaderSourceWrapperNativeLayout
{
    public const uint WrapperVtableVirtualAddress = 0x1C80CC4;
    public const uint WrapperVtableRva = 0x1880CC4;
    public const uint WrapperVtableRawFileOffset = 0x187F8C4;

    public const uint WrapperVtableSlot4EntryVirtualAddress = 0x16CC560;
    public const uint WrapperVtableSlot4EntryRawFileOffset = 0x12CB960;
    public const uint WrapperVtableSlotCEntryVirtualAddress = 0x16CC5C0;
    public const uint WrapperVtableSlotCEntryRawFileOffset = 0x12CB9C0;

    public const int WrapperVtableSlot4Offset = 0x04;
    public const int WrapperVtableSlotCOffset = 0x0C;
    public const int BackingPointerFieldOffset = 0x04;
    public const int BackingDataPointerFieldOffset = 0x04;
    public const int BackingSizeFieldOffset = 0x0C;

    public const uint PreserveRawHelperEntryRawFileOffset = 0x12CBCB0;
    public const uint PreserveSlowPathAllocationCallRawFileOffset = 0x12CBDA6;
    public const uint PreserveSlowPathAllocationTargetRawFileOffset = 0x12CBF30;

    public static IReadOnlyList<UiShaderSourceWrapperInstructionEvidence> PreserveCopyPathInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceWrapperInstructionEvidence("Read old header +4", 0x12CBDC8),
            new UiShaderSourceWrapperInstructionEvidence("Read new header +4", 0x12CBDD0),
            new UiShaderSourceWrapperInstructionEvidence("Copy call", 0x12CBDD3)
        });

    public static IReadOnlyList<UiShaderSourceWrapperInstructionEvidence> PreserveNoCopyPathInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceWrapperInstructionEvidence("Load new header into EBX", 0x12CBDDB),
            new UiShaderSourceWrapperInstructionEvidence("Compare ESI with [EBX]", 0x12CBDDF)
        });

    public static IReadOnlyList<UiShaderSourceWrapperInstructionEvidence> PreservePostAccessWrites { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceWrapperInstructionEvidence("Write logical size", 0x12CBDFD),
            new UiShaderSourceWrapperInstructionEvidence("Write wrapper +8", 0x12CBE07),
            new UiShaderSourceWrapperInstructionEvidence("Write wrapper +4", 0x12CBE15)
        });
}

/// <summary>
/// A read-only observation of one instruction's raw-file offset, not a behavioral model.
/// </summary>
public readonly record struct UiShaderSourceWrapperInstructionEvidence(string Observation, uint RawFileOffset);

/// <summary>
/// Returns only the size already captured from the backing record. This does not model wrapper slot 4's
/// unresolved argument/resize behavior.
/// </summary>
public static class UiShaderSourceWrapperSizeProjectionRules
{
    public static uint ReadCapturedSize(uint capturedBackingPointer, uint? capturedBackingFieldC)
    {
        if (capturedBackingPointer == 0)
        {
            if (capturedBackingFieldC.HasValue)
                throw new ArgumentException(
                    "A null backing pointer cannot have a captured backing-field size.",
                    nameof(capturedBackingFieldC));

            return 0;
        }

        return capturedBackingFieldC
            ?? throw new ArgumentException(
                "A nonnull backing pointer requires a captured backing-field size.",
                nameof(capturedBackingFieldC));
    }
}
