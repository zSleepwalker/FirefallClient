using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTraversalZeroSlotLifecycleCapture(
    uint? EntryEAX,
    uint? EntryEDI,
    uint? EntryEDX,
    uint? FreshEDXPlus4,
    uint? FreshChildDWORD,
    uint? LateFreshChildPlus4,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
{
    ParentGateStopped,
    ParentEqualBypassesChildTo9403,
    ParentUnequalHandoffNotExact,
    MissingFreshChildDWORDAt93F1,
    ChildZeroHandoffNotExactAt945C,
    ChildZeroMissingDerivedECXAt945C,
    ChildZeroMissingFreshGlobalValueAt945C,
    ChildZeroUnsignedBelowBranchTarget,
    ChildZeroUnsignedNotBelowFallthrough,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase
{
    ParentGate,
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead(
    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase Phase,
    uint? RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool ReadReached);

public enum CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
{
    ParentGate,
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction(
    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public sealed record CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan(
    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleCapture Capture,
    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonTagTraversalLifecyclePlan TraversalPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead> OrderedReads
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction>
                .Empty;

    public CapacityFinalCalleeCommonTagTraversalLifecycleOutcome TraversalOutcome =>
        TraversalPlan.Outcome;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome? ThresholdOutcome =>
        ThresholdPlan?.Outcome;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        TraversalPlan.ChildPlan;
    public uint? ChildFieldAddressAt93EE => ChildPlan?.ChildFieldAddress;
    public uint? ThresholdEntryECX => ThresholdPlan?.Capture.EntryECX;
    public bool ThresholdEntryECXIsDerivedFromChildFieldAddress =>
        ThresholdPlan is { } threshold
        && threshold.Capture.EntryECX == ChildFieldAddressAt93EE;
    public bool ParentUnequalHandoffExact => TraversalPlan.ParentUnequalHandoffExact;
    public bool ParentZeroChildOutcome =>
        TraversalOutcome == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
            .ChildZeroTo945C;
    public bool ChildZero945CSelected => TraversalPlan.ChildZero945CSelected;
    public bool ChildZero945CEntered => Internal945CEntered;
    public bool ChildZero945CBoundaryTargetEntered => false;
    public bool ChildZeroBranchReached => ChildPlan?.ChildZeroBranchReached == true;
    public bool ChildZeroBranchTaken => ChildPlan?.ChildZeroBranchTaken == true;
    public uint? ChildZeroBranchDestinationRawFileOffset =>
        ChildPlan?.ChildZeroBranchDestinationRawFileOffset;
    public bool ChildZeroStopAt945C =>
        ChildPlan?.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonTraversalIterationProjection
                .ZeroChildBranchTargetRawFileOffset;
    public bool ChildPlus4ReadReached => ChildPlan?.FreshChildPlus4ReadReached == true;
    public bool LateFreshChildPlus4CaptureProvidedButUnused =>
        TraversalPlan.LateFreshChildPlus4CaptureProvidedButUnused;
    public bool FreshGlobalValueCaptureProvidedButUnused =>
        ThresholdPlan is null && Capture.FreshGlobalValue.HasValue;
    public bool ExactParentZeroHandoff { get; init; }
    public bool Internal945CEntered => ThresholdPlan?.CompareInstructionReached == true;
    public bool ThresholdCompareAttempted => Internal945CEntered;
    public bool ThresholdCompareReached => ThresholdPlan?.CompareReached == true;
    public bool? ThresholdBranchTaken => ThresholdPlan?.BranchTaken;
    public uint? ThresholdBranchSelectedDestinationRawFileOffset =>
        ThresholdPlan?.BranchSelectedDestinationRawFileOffset;
    public bool Outer8E76Entered => false;
    public bool Outer9468Entered => false;
    public CapacityFinalCalleeCommonTraversalIterationFlags? ParentFlagsAtStop =>
        TraversalPlan.FlagsAtStop;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop =>
        ThresholdPlan is { CompareReached: true, CompareFlags: { } flags }
            ? new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
                flags.Overflow)
            : TraversalPlan.FlagsAtStop;
    public bool ExactCleanCompletion { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the parent tag traversal with its one zero-child edge into the
/// bounded 945C threshold gate. The threshold ECX comes from the child LEA result.
/// </summary>
public static class CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTagTraversalLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonTagTraversalLifecycleProjection.ChildEntryRawFileOffset;
    public const uint ZeroChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTraversalIterationProjection
            .ZeroChildBranchTargetRawFileOffset;
    public const uint ThresholdBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
            .BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan Project(
        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan traversal =
            CapacityFinalCalleeCommonTagTraversalLifecycleProjection.Project(new(
                capture.EntryEAX,
                capture.EntryEDI,
                capture.EntryEDX,
                capture.FreshEDXPlus4,
                capture.FreshChildDWORD,
                capture.LateFreshChildPlus4));
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead>();
        foreach (CapacityFinalCalleeCommonTagTraversalLifecycleRead read
            in traversal.OrderedReads)
        {
            reads.Add(new(
                read.Phase switch
                {
                    CapacityFinalCalleeCommonTagTraversalLifecycleReadPhase.ParentGate =>
                        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase.ParentGate,
                    _ => CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase
                        .ChildIteration
                },
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value,
                read.ReadReached));
        }

        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonTagTraversalLifecycleInstruction instruction
            in traversal.OrderedInstructions)
        {
            instructions.Add(new(
                instruction.Phase
                    == CapacityFinalCalleeCommonTagTraversalLifecycleInstructionPhase.ParentGate
                        ? CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                            .ParentGate
                        : CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                            .ChildIteration,
                instruction.RawFileOffset));
        }

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = null;
        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome outcome =
            MapTraversalOutcome(traversal.Outcome);
        bool exactParentZeroHandoff = IsExactParentZeroHandoff(traversal, capture);
        bool exactCleanCompletion = traversal.ExactCleanCompletion
            && !capture.FreshGlobalValue.HasValue;
        uint stopAt = traversal.StopAtRawFileOffsetExclusive;

        if (traversal.ChildZero945CSelected && exactParentZeroHandoff)
        {
            uint? derivedChildAddress = traversal.ChildPlan?.ChildFieldAddress;
            threshold = CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.Project(
                new(derivedChildAddress, capture.FreshGlobalValue));
            foreach (CapacityFinalCalleeCommonZeroSlotThresholdGateRead read in threshold.Reads)
            {
                reads.Add(new(
                    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleReadPhase
                        .ZeroSlotThreshold,
                    read.RawFileOffset,
                    read.Address,
                    read.AddressCaptured,
                    read.ValueCaptured,
                    read.Value,
                    ReadReached: threshold.GlobalReadReached));
            }

            if (threshold.CompareInstructionReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .CompareRawFileOffset));
            }

            if (threshold.BranchReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .BranchRawFileOffset));
            }

            outcome = MapThresholdOutcome(threshold.Outcome);
            exactCleanCompletion = exactParentZeroHandoff
                && threshold.ExactCleanCompletion;
            stopAt = threshold.StopAtRawFileOffsetExclusive;
        }
        else if (traversal.ChildZero945CSelected)
        {
            outcome =
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C;
            exactCleanCompletion = false;
            stopAt = ZeroChildBoundaryRawFileOffsetExclusive;
        }

        return new(capture, outcome)
        {
            TraversalPlan = traversal,
            ThresholdPlan = threshold,
            OrderedReads = reads.ToImmutable(),
            OrderedInstructions = instructions.ToImmutable(),
            ExactParentZeroHandoff = exactParentZeroHandoff,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactParentZeroHandoff(
        CapacityFinalCalleeCommonTagTraversalLifecyclePlan traversal,
        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleCapture capture) =>
        traversal.Outcome
            == CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C
        && traversal.ParentUnequalHandoffExact
        && traversal.ParentUnequal93E4Selected
        && traversal.Internal93E4HandoffEntered
        && traversal.ExactSingleIterationEvidence
        && traversal.ExactCleanCompletion
        && traversal.StopAtRawFileOffsetExclusive
            == ZeroChildBoundaryRawFileOffsetExclusive
        && traversal.ChildZero945CSelected
        && !traversal.ChildZero945CEntered
        && traversal.ChildPlan is
        {
            EntryMoveReached: true,
            ChildAddressLeaReached: true,
            ChildFieldAddress: not null,
            FreshChildReadReached: true,
            FreshChildDWORDCaptured: true,
            FreshChildDWORD: 0,
            ChildZeroBranchReached: true,
            ChildZeroBranchTaken: true,
            ChildZeroBranchDestinationRawFileOffset:
                ZeroChildBoundaryRawFileOffsetExclusive,
            MoveChildToEDXReached: false,
            FreshChildPlus4ReadReached: false,
            CompareReached: false,
            TargetEntered: false,
            StopAtRawFileOffsetExclusive: ZeroChildBoundaryRawFileOffsetExclusive
        }
        && !traversal.TargetEntered
        && !capture.LateFreshChildPlus4.HasValue
        && !traversal.LateFreshChildPlus4CaptureProvidedButUnused;

    private static CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
        MapTraversalOutcome(CapacityFinalCalleeCommonTagTraversalLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ParentGateStopped =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome.ParentGateStopped,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .ParentEqualBypassesChildTo9403 =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ParentEqualBypassesChildTo9403,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome.ChildZeroTo945C =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTagTraversalLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            _ => CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildMaskedDiffersFromEntryEDITo93E4
        };

    private static CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
        MapThresholdOutcome(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingBothCompareOperands
                or CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            _ => CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough
        };
}
