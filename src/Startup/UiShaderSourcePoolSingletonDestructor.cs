using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolSingletonDestructorWrite(
    int ThisRelativeOffset,
    uint InstructionRawFileOffset,
    uint Value);

public readonly record struct UiShaderSourcePoolSingletonDestructorProjection(
    uint ThisPointer,
    byte DeletingFlags,
    ImmutableArray<UiShaderSourcePoolSingletonDestructorWrite> OrderedUnconditionalWrites,
    bool CallsAllocatorBackendHelper,
    uint? AllocatorBackendReceiverInEcx,
    uint ReturnValue,
    byte ReturnPopByteCount);

public readonly record struct UiShaderSourcePoolSingletonDestructorInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ThisRelativeOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Bounded direct-entry projection for the singleton's deleting-disposition body.
/// It does not model effects of the called helper or the adjacent pool-drain routine.
/// </summary>
public static class UiShaderSourcePoolSingletonDestructor
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint SingletonVtableVirtualAddress = 0x1C80CA0;
    public const uint SingletonVtableRawFileOffset = 0x187F8A0;
    public const string SingletonVtableBytes =
        "80 CE 6C 01 10 CD 6C 01 D0 CD 6C 01";
    public const uint SingletonDeletingSlotVirtualAddress = 0x16CCE80;
    public const uint SingletonDeletingSlotRawFileOffset = 0x12CC280;
    public const uint SingletonDetachSlotVirtualAddress = 0x16CCDD0;
    public const uint SingletonDetachSlotRawFileOffset = 0x12CC1D0;

    public const uint EntryRawFileOffset = 0x12CC280;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CC2AC;
    public const uint AdjacentPoolDrainHelperRawFileOffset = 0x12CC2B0;

    public const int ThisVptrOffset = 0;
    public const int ThisClearedWord0Offset = 0x08;
    public const int ThisClearedWord1Offset = 0x0C;
    public const int DeletingFlagsStackArgumentOffsetFromEbp = 0x08;
    public const byte DeletingFlagsMask = 0x01;
    public const uint ReplacementVptrVirtualAddress = 0x1C5E9BC;
    public const uint ConditionalAllocatorBackendHelperRawFileOffset = 0x12C8D60;

    public const uint VptrStoreRawFileOffset = 0x12CC28A;
    public const uint FirstClearRawFileOffset = 0x12CC290;
    public const uint SecondClearRawFileOffset = 0x12CC297;
    public const uint DeletingFlagTestRawFileOffset = 0x12CC283;
    public const uint DeletingFlagBranchRawFileOffset = 0x12CC29E;
    public const uint ConditionalHelperCallRawFileOffset = 0x12CC2A0;
    public const uint ReturnReceiverLoadRawFileOffset = 0x12CC2A5;
    public const uint ReturnInstructionRawFileOffset = 0x12CC2A9;
    public const byte ReturnPopByteCount = 4;

    public static IReadOnlyList<UiShaderSourcePoolSingletonDestructorInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Establish the frame",
                0x12CC280,
                "55"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Set EBP from ESP",
                0x12CC281,
                "8B EC"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Test bit zero of the byte argument at EBP + 8",
                DeletingFlagTestRawFileOffset,
                "F6 45 08 01"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Save ESI",
                0x12CC287,
                "56"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Copy the receiver from ECX to ESI",
                0x12CC288,
                "8B F1"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Store the replacement vptr at this + 0",
                VptrStoreRawFileOffset,
                "C7 06 BC E9 C5 01",
                ThisRelativeOffset: ThisVptrOffset,
                TargetVirtualAddress: ReplacementVptrVirtualAddress),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Clear the DWORD at this + 8",
                FirstClearRawFileOffset,
                "C7 46 08 00 00 00 00",
                ThisRelativeOffset: ThisClearedWord0Offset),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Clear the DWORD at this + 0xC",
                SecondClearRawFileOffset,
                "C7 46 0C 00 00 00 00",
                ThisRelativeOffset: ThisClearedWord1Offset),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Skip the helper call when deleting bit zero is clear",
                DeletingFlagBranchRawFileOffset,
                "74 05"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Call the helper with ECX still holding the receiver",
                ConditionalHelperCallRawFileOffset,
                "E8 BB CA FF FF",
                TargetRawFileOffset: ConditionalAllocatorBackendHelperRawFileOffset),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Load the receiver into EAX for the return value",
                ReturnReceiverLoadRawFileOffset,
                "8B C6"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Restore ESI",
                0x12CC2A7,
                "5E"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Restore EBP",
                0x12CC2A8,
                "5D"),
            new UiShaderSourcePoolSingletonDestructorInstructionEvidence(
                "Return and pop the four-byte stack argument",
                ReturnInstructionRawFileOffset,
                "C2 04 00")
        });

    /// <summary>
    /// Projects the unconditional direct-entry writes, conditional helper dispatch,
    /// and receiver return for an already-entered call. It does not project helper
    /// effects, including any pool/list behavior or allocator/OS behavior.
    /// </summary>
    public static UiShaderSourcePoolSingletonDestructorProjection Project(
        uint thisPointer,
        byte deletingFlags)
    {
        bool callsHelper = (deletingFlags & DeletingFlagsMask) != 0;

        return new UiShaderSourcePoolSingletonDestructorProjection(
            thisPointer,
            deletingFlags,
            ImmutableArray.Create(
                new UiShaderSourcePoolSingletonDestructorWrite(
                    ThisVptrOffset,
                    VptrStoreRawFileOffset,
                    ReplacementVptrVirtualAddress),
                new UiShaderSourcePoolSingletonDestructorWrite(
                    ThisClearedWord0Offset,
                    FirstClearRawFileOffset,
                    0),
                new UiShaderSourcePoolSingletonDestructorWrite(
                    ThisClearedWord1Offset,
                    SecondClearRawFileOffset,
                    0)),
            callsHelper,
            callsHelper ? thisPointer : null,
            thisPointer,
            ReturnPopByteCount);
    }
}
