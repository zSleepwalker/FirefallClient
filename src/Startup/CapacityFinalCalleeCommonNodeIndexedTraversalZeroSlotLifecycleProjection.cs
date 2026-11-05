using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleCapture(
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture Parent,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
{
    ParentStopped,
    ParentZeroToBounded93AC,
    ParentEqualToBounded9403,
    ParentUnequalNotCleanAt93E4,
    MissingEntryEBXAt93E4,
    MissingLateEntryEDXAt93EE,
    MissingFreshChildDWORDAt93F1,
    ChildZeroHandoffNotExactAt945C,
    ChildZeroMissingDerivedECXAt945C,
    ChildZeroMissingFreshGlobalValueAt945C,
    ChildZeroUnsignedBelowBranchTarget,
    ChildZeroUnsignedNotBelowFallthrough,
    MissingLateFreshChildPlus4At93F9,
    MissingEntryEDIAt93FF,
    MaskedChildEqualsEntryEDIAt9403,
    MaskedChildDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase
{
    ParentTraversal,
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead(
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase Phase,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool ReadReached);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    FreshChildDWORDRead,
    FreshChildPlus4Read,
    ThresholdFreshGlobalRead
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase
{
    ChildIteration,
    ZeroSlotThreshold
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstruction(
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public sealed record CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan(
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan TraversalPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan
        { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstruction>
                .Empty;

    public CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome TraversalOutcome =>
        TraversalPlan.Outcome;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome? ThresholdOutcome =>
        ThresholdPlan?.Outcome;
    public CapacityFinalCalleeCommonTraversalIterationPlan? IterationPlan =>
        TraversalPlan.IterationPlan;
    public uint? ParentDerivedEntryEDX => TraversalPlan.DerivedEntryEDX;
    public uint? ChildFieldAddressAt93EE => IterationPlan?.ChildFieldAddress;
    public uint? ThresholdEntryECX => ThresholdPlan?.Capture.EntryECX;
    public bool ThresholdEntryECXIsDerivedFromChildFieldAddress =>
        ThresholdPlan is { } threshold
        && threshold.Capture.EntryECX == ChildFieldAddressAt93EE;
    public bool ParentUnequalHandoffExact => TraversalPlan.ExactParentUnequalHandoff;
    public bool ChildZero945CSelected =>
        IterationPlan is { } iteration
        && iteration.ChildZeroBranchTaken == true
        && iteration.ChildZeroBranchDestinationRawFileOffset
            == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection
                .ZeroChildBoundaryRawFileOffsetExclusive;
    public bool ExactParentZeroHandoff { get; init; }
    public bool ParentExactBoundedCompletion => TraversalPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => TraversalPlan.ExactCleanCompletion;
    public bool ChildZero945CEntered => Internal945CEntered;
    public bool ChildZero945CBoundaryTargetEntered => false;
    public bool Internal945CEntered => ThresholdPlan?.CompareInstructionReached == true;
    public bool ThresholdCompareAttempted => Internal945CEntered;
    public bool ThresholdGlobalReadReached => ThresholdPlan?.GlobalReadReached == true;
    public bool ThresholdCompareReached => ThresholdPlan?.CompareReached == true;
    public bool? ThresholdBranchTaken => ThresholdPlan?.BranchTaken;
    public uint? ThresholdBranchSelectedDestinationRawFileOffset =>
        ThresholdPlan?.BranchSelectedDestinationRawFileOffset;
    public bool ThresholdMemoryWriteReached => false;
    public bool Outer8E76Entered => false;
    public bool Outer9468Entered => false;
    public bool FreshGlobalValueCaptureProvidedButUnused =>
        ThresholdPlan is null && Capture.FreshGlobalValue.HasValue;
    public bool LateFreshChildPlus4CaptureProvidedButUnused =>
        TraversalPlan.LateFreshChildPlus4CaptureProvidedButUnused;
    public CapacityFinalCalleeCommonTraversalIterationFlags? ParentFlagsAtStop =>
        TraversalPlan.FlagsAtStop;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop =>
        ThresholdPlan is { CompareReached: true, CompareFlags: { } flags }
            ? new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign,
                flags.Overflow)
            : TraversalPlan.FlagsAtStop;
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Adds the 945C threshold gate only to an exact clean zero-child exit from the
/// node-indexed traversal; the gate's ECX is the child's 93EE LEA result.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.EntryRawFileOffset;
    public const uint IterationEntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection
            .IterationEntryRawFileOffset;
    public const uint ZeroChildBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection
            .ChildZeroDestinationRawFileOffset;
    public const uint ThresholdBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
            .BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal =
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.Project(
                capture.Parent);
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect>();
        var reads =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead>();
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstruction>();

        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect effect
            in traversal.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .FreshChildDWORDRead,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .FreshChildPlus4Read
                };
            effects.Add(new(
                kind,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed,
                effect.ForwardedFromPriorWrite));

            if (kind != CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                .ParentWrite)
            {
                reads.Add(new(
                    kind is
                        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                            .FreshChildDWORDRead
                            or CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                                .FreshChildPlus4Read
                        ? CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase
                            .ChildIteration
                        : CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase
                            .ParentTraversal,
                    effect.RawFileOffset,
                    effect.Address,
                    effect.AddressCaptured,
                    effect.ValueCaptured,
                    effect.Value,
                    effect.Attempted));
            }
        }

        if (traversal.IterationPlan is { } iteration)
        {
            foreach (CapacityFinalCalleeCommonTraversalIterationEvent item in iteration.Events)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ChildIteration,
                    item.RawFileOffset));
            }
        }

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = null;
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome outcome =
            MapTraversalOutcome(traversal.Outcome);
        bool exactParentZeroHandoff = IsExactParentZeroHandoff(traversal);
        bool exactBoundedCompletion = traversal.ExactBoundedCompletion;
        bool exactCleanCompletion = traversal.ExactCleanCompletion
            && !capture.FreshGlobalValue.HasValue;
        uint stopAt = traversal.StopAtRawFileOffsetExclusive;

        if (traversal.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ZeroChildToBounded945C
            && exactParentZeroHandoff)
        {
            threshold = CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.Project(
                new(traversal.IterationPlan?.ChildFieldAddress, capture.FreshGlobalValue));
            foreach (CapacityFinalCalleeCommonZeroSlotThresholdGateRead read in threshold.Reads)
            {
                reads.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase
                        .ZeroSlotThreshold,
                    read.RawFileOffset,
                    read.Address,
                    read.AddressCaptured,
                    read.ValueCaptured,
                    read.Value,
                    ReadReached: threshold.GlobalReadReached));
                effects.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                        .ThresholdFreshGlobalRead,
                    read.RawFileOffset,
                    read.Address,
                    read.AddressCaptured,
                    read.Value,
                    read.ValueCaptured,
                    Attempted: threshold.GlobalReadReached,
                    Committed: false,
                    ForwardedFromPriorWrite: false));
            }

            if (threshold.CompareInstructionReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .CompareRawFileOffset));
            }

            if (threshold.BranchReached)
            {
                instructions.Add(new(
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .BranchRawFileOffset));
            }

            outcome = MapThresholdOutcome(threshold.Outcome);
            exactBoundedCompletion =
                threshold.BranchReached
                && threshold.BranchSelectedDestinationRawFileOffset is
                    CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .BranchTargetRawFileOffset
                    or CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .FallthroughRawFileOffset
                && threshold.StopAtRawFileOffsetExclusive
                    == threshold.BranchSelectedDestinationRawFileOffset
                && !threshold.TargetEntered;
            exactCleanCompletion =
                exactParentZeroHandoff
                && exactBoundedCompletion
                && threshold.ExactCleanCompletion;
            stopAt = threshold.StopAtRawFileOffsetExclusive;
        }
        else if (traversal.Outcome
            == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ZeroChildToBounded945C)
        {
            outcome =
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C;
            exactCleanCompletion = false;
            stopAt = ZeroChildBoundaryRawFileOffsetExclusive;
        }

        return new(capture, outcome)
        {
            TraversalPlan = traversal,
            ThresholdPlan = threshold,
            OrderedReads = reads.ToImmutable(),
            OrderedMemoryEffects = effects.ToImmutable(),
            OrderedInstructions = instructions.ToImmutable(),
            ExactParentZeroHandoff = exactParentZeroHandoff,
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactParentZeroHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal) =>
        traversal.Outcome
            == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ZeroChildToBounded945C
        && traversal.ExactParentUnequalHandoff
        && traversal.ExactBoundedCompletion
        && traversal.ExactCleanCompletion
        && traversal.NoRepeated93D8Read
        && traversal.IterationPlan is
        {
            ChildAddressLeaReached: true,
            ChildFieldAddress: not null,
            FreshChildReadReached: true,
            FreshChildDWORDCaptured: true,
            FreshChildDWORD: 0,
            ChildTestFlags:
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            },
            ChildZeroBranchReached: true,
            ChildZeroBranchTaken: true,
            ChildZeroBranchDestinationRawFileOffset:
                ZeroChildBoundaryRawFileOffsetExclusive,
            MoveChildToEDXReached: false,
            FreshChildPlus4ReadReached: false,
            AndEAXReached: false,
            CompareReached: false,
            LoopBranchReached: false,
            TargetEntered: false,
            StopAtRawFileOffsetExclusive: ZeroChildBoundaryRawFileOffsetExclusive,
            UnreachedLateFreshChildPlus4CaptureProvided: false
        }
        && !traversal.LateFreshChildPlus4CaptureProvidedButUnused
        && traversal.StopAtRawFileOffsetExclusive == ZeroChildBoundaryRawFileOffsetExclusive;

    private static CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
        MapTraversalOutcome(
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.ParentZeroToBounded93AC =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentZeroToBounded93AC,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.ParentEqualToBounded9403 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentEqualToBounded9403,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentUnequalNotCleanAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentUnequalNotCleanAt93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.MissingEntryEBXAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingEntryEBXAt93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingLateEntryEDXAt93EE =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingLateEntryEDXAt93EE,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.MissingEntryEDIAt93FF =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingEntryEDIAt93FF,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MaskedChildEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MaskedChildEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ZeroChildToBounded945C =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ParentStopped
        };

    private static CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
        MapThresholdOutcome(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingBothCompareOperands
                or CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough
        };
}
