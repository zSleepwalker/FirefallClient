using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonWritesRootLifecycleCapture(
    uint? EntryEDI,
    uint? EntryESI,
    uint? FreshGlobalDWORD);

public enum CapacityFinalCalleeCommonWritesRootLifecycleOutcome
{
    CommonTailMissingEntryEDI,
    CommonTailMissingEntryESI,
    CommonTailHandoffRejected,
    RootGateMissingFreshGlobalDWORD,
    RootGateEqualToFallthrough92D0,
    RootGateUnequalToBranch92EC
}

public enum CapacityFinalCalleeCommonWritesRootLifecycleWriteKind
{
    CommonTailFirstStore,
    CommonTailSecondStore
}

public readonly record struct CapacityFinalCalleeCommonWritesRootLifecycleWrite(
    CapacityFinalCalleeCommonWritesRootLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonWritesRootLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonWritesRootLifecyclePlan(
    CapacityFinalCalleeCommonWritesRootLifecycleCapture Capture,
    CapacityFinalCalleeCommonTailTagWritesPlan CommonTailTagWrites,
    CapacityFinalCalleeCommonTailRootGatePlan? CommonTailRootGate,
    CapacityFinalCalleeCommonWritesRootLifecycleOutcome Outcome)
{
    public bool RootGateReached => CommonTailRootGate is not null;
    public bool ChildEntryESIRetained => CommonTailTagWrites.Outcome
        == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed;
    public bool ChildEntryESIDerived => ChildEntryESIRetained;
    public uint? ChildEntryESI => ChildEntryESIRetained
        ? CommonTailTagWrites.Capture.EntryESI
        : null;
    public bool FreshGlobalReadReached =>
        CommonTailRootGate?.FreshGlobalReadReached == true;
    public uint? FreshGlobalAddress => CommonTailRootGate?.FreshGlobalAddress;
    public bool FreshGlobalCaptured =>
        CommonTailRootGate?.FreshGlobalCaptured == true;
    public uint? FreshGlobalDWORD => CommonTailRootGate?.FreshGlobalDWORD;
    public bool UnreachedFreshGlobalCaptureProvided =>
        !RootGateReached && Capture.FreshGlobalDWORD.HasValue;
    public ImmutableArray<CapacityFinalCalleeCommonWritesRootLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonWritesRootLifecycleWrite>.Empty;
    public CapacityFinalCalleeCommonTailTagWritesFlags? OrFlags =>
        CommonTailTagWrites.OrFlags;
    public CapacityFinalCalleeCommonTailRootGateFlags? RootCompareFlags =>
        CommonTailRootGate?.CompareFlags;
    public CapacityFinalCalleeCommonTailTagWritesFlags? FlagsAtStop =>
        RootCompareFlags is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary,
                flags.Zero, flags.Sign, flags.Overflow)
            : OrFlags;
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean common-tag tail at 92C8 to its root compare gate. The
/// child ESI is the retained entry ESI; the global DWORD is a separate fresh
/// read, independent of either preceding store, including when their address
/// aliases the global.
/// </summary>
public static class CapacityFinalCalleeCommonWritesRootLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryVirtualAddress;
    public const uint CommonTailBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint RootCompareRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset;
    public const uint FallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;
    public const uint BranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint FreshGlobalDWORDVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailRootGateProjection.BoundaryRawFileOffsetExclusive;

    public const string CapturedWindowBytes =
        "8B C7 83 C8 01 89 46 04 89 3C 37 3B 35 DC AD ED 01 75 1C";

    public static ImmutableArray<CapacityFinalCalleeCommonWritesRootLifecycleInstructionEvidence>
        Instructions { get; } =
            ImmutableArray.Create(
                Instruction("Load entry EDI into EAX",
                    CapacityFinalCalleeCommonTailTagWritesProjection.LoadEAXRawFileOffset,
                    "8B C7"),
                Instruction("OR EAX with 1",
                    CapacityFinalCalleeCommonTailTagWritesProjection.OrEAXRawFileOffset,
                    "83 C8 01"),
                Instruction("Store EAX at entry ESI+4",
                    CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset,
                    "89 46 04"),
                Instruction("Store entry EDI at entry EDI+ESI",
                    CapacityFinalCalleeCommonTailTagWritesProjection.SecondStoreRawFileOffset,
                    "89 3C 37"),
                Instruction("Compare retained entry ESI with the fresh global DWORD",
                    CapacityFinalCalleeCommonTailRootGateProjection.CompareRawFileOffset,
                    "3B 35 DC AD ED 01"),
                Instruction("Jump to 92EC when the compare is not equal",
                    CapacityFinalCalleeCommonTailRootGateProjection.JnzRawFileOffset,
                    "75 1C",
                    CapacityFinalCalleeCommonTailRootGateProjection
                        .BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonWritesRootLifecyclePlan Project(
        CapacityFinalCalleeCommonWritesRootLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                capture.EntryEDI,
                capture.EntryESI));
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonWritesRootLifecycleWrite>();

        foreach (CapacityFinalCalleeCommonTailTagWritesWrite write in commonTail.Writes)
        {
            writes.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset
                    ? CapacityFinalCalleeCommonWritesRootLifecycleWriteKind
                        .CommonTailFirstStore
                    : CapacityFinalCalleeCommonWritesRootLifecycleWriteKind
                        .CommonTailSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        if (commonTail.Outcome
            != CapacityFinalCalleeCommonTailTagWritesOutcome.Completed)
        {
            return new(
                capture,
                commonTail,
                CommonTailRootGate: null,
                commonTail.Outcome
                    == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
                    ? CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                        .CommonTailMissingEntryEDI
                    : CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                        .CommonTailMissingEntryESI)
            {
                OrderedWrites = writes.ToImmutable(),
                StopAtRawFileOffsetExclusive =
                    commonTail.StopAtRawFileOffsetExclusive
            };
        }

        if (!commonTail.ExactCleanExitAdmitted
            || commonTail.StopAtRawFileOffsetExclusive != CommonTailBoundaryRawFileOffsetExclusive
            || commonTail.Writes.Length != 2
            || commonTail.UnreachedEntryESICaptureProvided
            || commonTail.Capture.EntryESI is not uint retainedEntryEsi)
        {
            return new(capture, commonTail, null,
                CapacityFinalCalleeCommonWritesRootLifecycleOutcome.CommonTailHandoffRejected)
            {
                OrderedWrites = writes.ToImmutable(),
                StopAtRawFileOffsetExclusive = commonTail.StopAtRawFileOffsetExclusive
            };
        }
        CapacityFinalCalleeCommonTailRootGatePlan rootGate =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: retainedEntryEsi,
                FreshGlobalDWORD: capture.FreshGlobalDWORD));

        bool rootGateCompleted = rootGate.ExactCleanAdmission
            && rootGate.JnzReached
            && rootGate.StopAtRawFileOffsetExclusive
                == rootGate.JnzSelectedDestinationRawFileOffset
            && rootGate.JnzSelectedDestinationRawFileOffset is
                CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset
                or CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
        CapacityFinalCalleeCommonWritesRootLifecycleOutcome outcome =
            rootGate.Outcome switch
            {
                CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD =>
                    CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                        .RootGateMissingFreshGlobalDWORD,
                CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                        .RootGateEqualToFallthrough92D0,
                CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                        .RootGateUnequalToBranch92EC,
                _ => throw new InvalidOperationException(
                    "The common-tail root gate returned an unexpected outcome.")
            };

        return new(capture, commonTail, rootGate, outcome)
        {
            OrderedWrites = writes.ToImmutable(),
            ExactCleanAdmission = commonTail.ExactCleanExitAdmitted && rootGateCompleted,
            StopAtRawFileOffsetExclusive =
                rootGate.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeCommonWritesRootLifecycleInstructionEvidence
        Instruction(
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
