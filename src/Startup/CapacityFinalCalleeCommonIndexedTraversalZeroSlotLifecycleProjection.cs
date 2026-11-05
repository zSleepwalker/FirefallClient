using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture(
    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleCapture Parent,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
{
    ParentBlocked,
    ParentTagGateStopped,
    ParentEqualBypassesTraversalTo9403,
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

public enum CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase
{
    IndexedCount,
    ParentTagGate,
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead(
    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase Phase,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool ReadReached);

public enum CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
{
    IndexedCount,
    ParentTagGate,
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstruction(
    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public sealed record CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan(
    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan TraversalPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan
        { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstruction>
                .Empty;

    public CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome TraversalOutcome =>
        TraversalPlan.Outcome;
    public CapacityFinalCalleeCommonIndexedCountClassificationPlan CountPlan =>
        TraversalPlan.CountPlan;
    public CapacityFinalCalleeCommonMaskedTagEqualityGatePlan? TagPlan =>
        TraversalPlan.TagPlan;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        TraversalPlan.TraversalPlan;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome? ThresholdOutcome =>
        ThresholdPlan?.Outcome;
    public uint? ChildFieldAddressAt93EE => ChildPlan?.ChildFieldAddress;
    public uint? ThresholdEntryECX => ThresholdPlan?.Capture.EntryECX;
    public bool ThresholdEntryECXIsDerivedFromChildFieldAddress =>
        ThresholdPlan is { } threshold
        && threshold.Capture.EntryECX == ChildFieldAddressAt93EE;
    public bool ParentUnequalHandoffExact => TraversalPlan.ParentUnequalHandoffExact;
    public bool ParentZeroChildOutcome =>
        TraversalOutcome
            == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ChildZeroTo945C;
    public bool ChildZero945CSelected => TraversalPlan.ChildZero945CSelected;
    public bool ExactParentZeroHandoff { get; init; }
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
    public bool Internal945CEntered => ThresholdPlan?.CompareInstructionReached == true;
    public bool ThresholdCompareAttempted => Internal945CEntered;
    public bool ThresholdGlobalReadReached => ThresholdPlan?.GlobalReadReached == true;
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
/// Composes the indexed parent traversal's exact zero-child handoff with its
/// bounded 945C threshold comparison, deriving ECX from the child-slot LEA.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection.EntryRawFileOffset;
    public const uint ZeroChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection
            .ZeroChildBoundaryRawFileOffsetExclusive;
    public const uint ThresholdBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
            .BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan traversal =
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleProjection.Project(
                capture.Parent);
        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead>();
        foreach (CapacityFinalCalleeCommonIndexedTagTraversalLifecycleRead read
            in traversal.OrderedReads)
        {
            reads.Add(new(
                read.Stage switch
                {
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                        .ParentIndexedDWORDAt93C0 =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase
                            .IndexedCount,
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleReadStage
                        .ParentTagDWORDAt93D8 =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase
                            .ParentTagGate,
                    _ =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase
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
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstruction instruction
            in traversal.OrderedInstructions)
        {
            instructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
                        .IndexedCount =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                            .IndexedCount,
                    CapacityFinalCalleeCommonIndexedTagTraversalLifecycleInstructionPhase
                        .ParentTagGate =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                            .ParentTagGate,
                    _ =>
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                            .ChildIteration
                },
                instruction.RawFileOffset));
        }

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = null;
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome outcome =
            MapTraversalOutcome(traversal.Outcome);
        bool exactParentZeroHandoff = IsExactParentZeroHandoff(traversal);
        bool exactCleanCompletion = traversal.ExactCleanCompletion
            && !capture.FreshGlobalValue.HasValue;
        uint stopAt = traversal.StopAtRawFileOffsetExclusive;

        if (traversal.ChildZero945CSelected && exactParentZeroHandoff)
        {
            uint? derivedChildAddress = traversal.TraversalPlan?.ChildFieldAddress;
            threshold = CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.Project(
                new(derivedChildAddress, capture.FreshGlobalValue));
            foreach (CapacityFinalCalleeCommonZeroSlotThresholdGateRead read in threshold.Reads)
            {
                reads.Add(new(
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleReadPhase
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
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .CompareRawFileOffset));
            }

            if (threshold.BranchReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .BranchRawFileOffset));
            }

            outcome = MapThresholdOutcome(threshold.Outcome, derivedChildAddress.HasValue);
            exactCleanCompletion = threshold.ExactCleanCompletion;
            stopAt = threshold.StopAtRawFileOffsetExclusive;
        }
        else if (traversal.ChildZero945CSelected)
        {
            outcome =
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
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
        CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan traversal) =>
        traversal.Outcome
            == CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ChildZeroTo945C
        && traversal.ParentUnequalHandoffExact
        && traversal.ParentUnequal93E4Selected
        && traversal.TraversalEntry93E4Entered
        && traversal.ExactSingleIterationEvidence
        && traversal.ExactCleanCompletion
        && traversal.TraversalInheritedEDIAlreadyConsumed
        && traversal.StopAtRawFileOffsetExclusive
            == ZeroChildBoundaryRawFileOffsetExclusive
        && traversal.ChildZero945CSelected
        && !traversal.ChildZero945CEntered
        && traversal.TraversalPlan is
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
        && !traversal.LateFreshChildPlus4CaptureProvidedButUnused;

    private static CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
        MapTraversalOutcome(
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ParentBlocked =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome.ParentBlocked,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ParentTagGateStopped =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentTagGateStopped,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ParentEqualBypassesTraversalTo9403 =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome.ChildZeroTo945C =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
        };

    private static CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
        MapThresholdOutcome(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome,
            bool derivedChildAddressCaptured) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingBothCompareOperands
                or CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue =>
                derivedChildAddressCaptured
                    ? CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                        .ChildZeroMissingFreshGlobalValueAt945C
                    : CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                        .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough
        };
}
