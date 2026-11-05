using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonZeroSlotStoreTailCapture(
    uint? EntryECX,
    uint? EntryESI,
    uint? LateEntryEDX,
    CapacityFinalCalleeCommonZeroSlotStoreTailFlags? IncomingFlags = null);

public enum CapacityFinalCalleeCommonZeroSlotStoreTailOutcome
{
    MissingEntryECXForFirstStore,
    MissingEntryESIForFirstStore,
    MissingLateEntryEDXForSecondStore,
    CompletedToJumpTarget
}

public readonly record struct CapacityFinalCalleeCommonZeroSlotStoreTailFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonZeroSlotStoreTailWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Attempted => true;
    public bool Committed => Attempted && AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeCommonZeroSlotStoreTailPlan(
    CapacityFinalCalleeCommonZeroSlotStoreTailCapture Capture,
    CapacityFinalCalleeCommonZeroSlotStoreTailOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite>.Empty;
    public uint EntryRawFileOffset { get; init; }
    public uint BoundaryRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence>
        InstructionEvidence { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence>.Empty;
    public bool EntryECXReadReached { get; init; }
    public bool EntryESIReadReached { get; init; }
    public bool LateEntryEDXReadReached { get; init; }
    public bool UnreachedLateEntryEDXCaptureProvided { get; init; }
    public bool LateEntryEDXUnusedBecauseFirstStoreBlocked { get; init; }
    public bool FirstStoreAttempted => Writes.Length >= 1 && Writes[0].Attempted;
    public bool FirstStoreCommitted => Writes.Length >= 1 && Writes[0].Committed;
    public bool SecondStoreAttempted => Writes.Length >= 2 && Writes[1].Attempted;
    public bool SecondStoreCommitted => Writes.Length >= 2 && Writes[1].Committed;
    public bool ThirdStoreAttempted => Writes.Length >= 3 && Writes[2].Attempted;
    public bool ThirdStoreCommitted => Writes.Length >= 3 && Writes[2].Committed;
    public bool FourthStoreAttempted => Writes.Length >= 4 && Writes[3].Attempted;
    public bool FourthStoreCommitted => Writes.Length >= 4 && Writes[3].Committed;
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool JumpTargetEntered => false;
    public CapacityFinalCalleeCommonZeroSlotStoreTailFlags? FlagsAtStop { get; init; }
    public bool IncomingFlagsKnown => Capture.IncomingFlags.HasValue;
    public bool MoveInstructionsPreserveFlags { get; init; }
    public bool JumpPreservesFlags { get; init; }
    public bool IncomingFlagsCanBeRetained { get; init; }
    public bool FlagsPreserved =>
        MoveInstructionsPreserveFlags && (!JumpReached || JumpPreservesFlags);
    public bool BoundaryReached { get; init; }
    public bool BoundaryInstructionEntered { get; init; }
    public bool ExactBoundedExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the zero-slot store tail through its direct jump without entering the target.
/// </summary>
public static class CapacityFinalCalleeCommonZeroSlotStoreTailProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9468;
    public const uint EntryFlatIdaAddress = 0x012C9468;
    public const uint EntryVirtualAddress = 0x016CA068;
    public const uint FirstStoreRawFileOffset = 0x012C9468;
    public const uint SecondStoreRawFileOffset = 0x012C946A;
    public const uint ThirdStoreRawFileOffset = 0x012C946D;
    public const uint FourthStoreRawFileOffset = 0x012C9470;
    public const uint JumpRawFileOffset = 0x012C9473;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9475;
    public const uint EsiPlus18Displacement = 0x18;
    public const uint EsiPlusCDisplacement = 0x0C;
    public const uint EsiPlus8Displacement = 0x08;
    public const uint JumpTargetRawFileOffset = 0x012C942F;
    public const uint JumpTargetVirtualAddress = 0x016CA02F;

    public const string CapturedWindowBytes =
        "89 31 89 56 18 89 76 0C 89 76 08 EB BA";

    public static ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry ESI at entry ECX",
                FirstStoreRawFileOffset,
                "89 31"),
            Instruction(
                "Store late entry EDX at entry ESI+18",
                SecondStoreRawFileOffset,
                "89 56 18"),
            Instruction(
                "Store entry ESI at entry ESI+C",
                ThirdStoreRawFileOffset,
                "89 76 0C"),
            Instruction(
                "Store entry ESI at entry ESI+8",
                FourthStoreRawFileOffset,
                "89 76 08"),
            Instruction(
                "Jump directly to raw offset 012C942F",
                JumpRawFileOffset,
                "EB BA",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEsiPlus18Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus18Displacement);

    public static uint DeriveEsiPlusCAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiPlusCDisplacement);

    public static uint DeriveEsiPlus8Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus8Displacement);

    public static CapacityFinalCalleeCommonZeroSlotStoreTailPlan Project(
        CapacityFinalCalleeCommonZeroSlotStoreTailCapture capture)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonZeroSlotStoreTailWrite>();
        bool lateEntryEDXReadReached = false;
        bool jumpReached = false;

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan Finish(
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome outcome,
            uint stopAt,
            bool exactBoundedExitAdmitted = false,
            bool boundaryReached = false) =>
            new(capture, outcome)
            {
                Writes = writes.ToImmutable(),
                EntryRawFileOffset = EntryRawFileOffset,
                BoundaryRawFileOffsetExclusive = BoundaryRawFileOffsetExclusive,
                InstructionEvidence = Instructions,
                EntryECXReadReached = true,
                EntryESIReadReached = true,
                LateEntryEDXReadReached = lateEntryEDXReadReached,
                UnreachedLateEntryEDXCaptureProvided =
                    !lateEntryEDXReadReached && capture.LateEntryEDX.HasValue,
                LateEntryEDXUnusedBecauseFirstStoreBlocked =
                    !capture.EntryECX.HasValue || !capture.EntryESI.HasValue,
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached ? JumpTargetRawFileOffset : null,
                JumpTargetVirtualAddress = jumpReached ? JumpTargetVirtualAddress : null,
                FlagsAtStop = capture.IncomingFlags,
                MoveInstructionsPreserveFlags = true,
                JumpPreservesFlags = jumpReached,
                IncomingFlagsCanBeRetained = true,
                BoundaryReached = boundaryReached,
                BoundaryInstructionEntered = false,
                ExactBoundedExitAdmitted = exactBoundedExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        AddWrite(
            writes,
            FirstStoreRawFileOffset,
            capture.EntryECX,
            capture.EntryECX.HasValue,
            capture.EntryESI,
            capture.EntryESI.HasValue);
        if (!capture.EntryECX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryECXForFirstStore,
                FirstStoreRawFileOffset);
        }

        if (!capture.EntryESI.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore,
                FirstStoreRawFileOffset);
        }

        lateEntryEDXReadReached = true;
        AddWrite(
            writes,
            SecondStoreRawFileOffset,
            DeriveEsiPlus18Address(capture.EntryESI.Value),
            true,
            capture.LateEntryEDX,
            capture.LateEntryEDX.HasValue);
        if (!capture.LateEntryEDX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingLateEntryEDXForSecondStore,
                SecondStoreRawFileOffset);
        }

        AddWrite(
            writes,
            ThirdStoreRawFileOffset,
            DeriveEsiPlusCAddress(capture.EntryESI.Value),
            true,
            capture.EntryESI,
            true);
        AddWrite(
            writes,
            FourthStoreRawFileOffset,
            DeriveEsiPlus8Address(capture.EntryESI.Value),
            true,
            capture.EntryESI,
            true);

        jumpReached = true;
        return Finish(
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget,
            JumpTargetRawFileOffset,
            exactBoundedExitAdmitted: true,
            boundaryReached: true);
    }

    private static void AddWrite(
        ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite>.Builder writes,
        uint rawFileOffset,
        uint? address,
        bool addressCaptured,
        uint? value,
        bool valueCaptured) =>
        writes.Add(new(
            rawFileOffset,
            address,
            addressCaptured,
            valueCaptured,
            value));

    private static CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
