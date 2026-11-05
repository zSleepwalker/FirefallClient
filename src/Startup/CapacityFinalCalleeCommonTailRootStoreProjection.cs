using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTailRootStoreCapture(
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonTailRootStoreOutcome
{
    MissingEntryEDI,
    StoredAndJumped
}

public readonly record struct CapacityFinalCalleeCommonTailRootStoreWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonTailRootStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? MemoryOperandPEVirtualAddress = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonTailRootStorePlan(
    CapacityFinalCalleeCommonTailRootStoreCapture Capture,
    CapacityFinalCalleeCommonTailRootStoreOutcome Outcome)
{
    public bool EntryEDICaptured { get; init; }
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonTailRootStoreWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonTailRootStoreWrite>.Empty;
    public bool MemoryReadReached => false;
    public bool StorePreservesFlags { get; init; }
    public bool JumpPreservesFlags { get; init; }
    public bool FlagsPreserved =>
        StoreReached && JumpReached && StorePreservesFlags && JumpPreservesFlags;
    public uint? FlagsAtStop { get; init; }
    public bool ExactBoundedExitAdmitted { get; init; }
    public bool EpilogueEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the single absolute root store and its direct jump, stopping at 9441.
/// </summary>
public static class CapacityFinalCalleeCommonTailRootStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92D0;
    public const uint EntryVirtualAddress = 0x016C9ED0;
    public const uint StoreRawFileOffset = 0x012C92D0;
    public const uint JumpRawFileOffset = 0x012C92D6;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92DB;

    public const uint RootStorePEVirtualAddress = 0x01EDADD0;
    public const uint JumpTargetRawFileOffset = 0x012C9441;
    public const uint JumpTargetVirtualAddress = 0x016CA041;

    public const string CapturedWindowBytes =
        "89 3D D0 AD ED 01 E9 66 01 00 00";

    public static ImmutableArray<CapacityFinalCalleeCommonTailRootStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry EDI to the absolute root address",
                StoreRawFileOffset,
                "89 3D D0 AD ED 01",
                memoryOperandPEVirtualAddress: RootStorePEVirtualAddress),
            Instruction(
                "Jump directly to 9441",
                JumpRawFileOffset,
                "E9 66 01 00 00",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonTailRootStorePlan Project(
        CapacityFinalCalleeCommonTailRootStoreCapture capture)
    {
        if (capture.EntryEDI is not uint entryEdi)
        {
            return new(
                capture,
                CapacityFinalCalleeCommonTailRootStoreOutcome.MissingEntryEDI)
            {
                StopAtRawFileOffsetExclusive = StoreRawFileOffset
            };
        }

        var write = new CapacityFinalCalleeCommonTailRootStoreWrite(
            StoreRawFileOffset,
            RootStorePEVirtualAddress,
            entryEdi);

        return new(
            capture,
            CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped)
        {
            EntryEDICaptured = true,
            StoreReached = true,
            StoreAddress = RootStorePEVirtualAddress,
            StoreValue = entryEdi,
            JumpReached = true,
            JumpTargetRawFileOffset = JumpTargetRawFileOffset,
            JumpTargetVirtualAddress = JumpTargetVirtualAddress,
            Writes = ImmutableArray.Create(write),
            StorePreservesFlags = true,
            JumpPreservesFlags = true,
            ExactBoundedExitAdmitted = true,
            StopAtRawFileOffsetExclusive = JumpTargetRawFileOffset
        };
    }

    private static CapacityFinalCalleeCommonTailRootStoreInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? memoryOperandPEVirtualAddress = null,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            memoryOperandPEVirtualAddress,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
