using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleCapture(
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture Traversal,
    uint? FreshChildGlobalValue,
    uint? FreshChildEDXPlus8);

public enum CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
{
    ParentStopped,
    ParentZeroToBounded93AC,
    ParentEqualToBounded9403,
    ParentUnequalNotCleanAt93E4,
    MissingEntryEBXAt93E4,
    MissingLateEntryEDXAt93EE,
    MissingFreshChildDWORDAt93F1,
    ZeroChildToBounded945C,
    MissingLateFreshChildPlus4At93F9,
    MissingEntryEDIAt93FF,
    MaskedChildEqualsEntryEDIAt9403,
    MaskedChildDiffersFromEntryEDITo93E4,
    MissingFreshChildGlobalValue,
    MissingFreshChildEDXPlus8,
    FirstUnsignedBelowBypassesRing,
    SecondUnsignedBelowBypassesRing,
    MissingInheritedEntryESIForFirstWrite,
    ChildHandoffNotExact,
    CompletedToBoundaryBeforeDec
}

public enum CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    FreshTraversalChildDWORDRead,
    FreshTraversalChildPlus4Read,
    ThresholdFreshGlobalRead,
    ThresholdFreshEDXPlus8Read,
    RingWriteAttempt
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public readonly record struct CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan(
    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome Outcome,
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan TraversalPlan)
{
    public CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? ThresholdRingPlan
        { get; init; }
    public uint? DerivedChildEDX { get; init; }
    public uint? InheritedEntryESI { get; init; }
    public bool ChildEDXDerivedFromIterationPlan { get; init; }
    public bool EntryESIInheritedFromTraversalParent { get; init; }
    public bool ExactChildEqualityHandoff { get; init; }
    public bool ThresholdGateEntered => ThresholdRingPlan is not null;
    public bool ThresholdFallthroughSelected =>
        ThresholdRingPlan?.ParentExactFallthroughHandoff == true;
    public bool RingInsertionEntered =>
        ThresholdRingPlan?.ParentFallthroughInstructionEntered == true;
    public bool RingBoundaryReached =>
        ThresholdRingPlan?.ChildBoundaryReached == true;
    public bool RingBoundaryInstructionEntered =>
        ThresholdRingPlan?.ChildBoundaryInstructionEntered == true;
    public bool FreshChildGlobalValueCaptureProvidedButUnused { get; init; }
    public bool FreshChildEDXPlus8CaptureProvidedButUnused { get; init; }
    public CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome TraversalOutcome =>
        TraversalPlan.Outcome;
    public CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome? ThresholdRingOutcome =>
        ThresholdRingPlan?.Outcome;
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? TraversalFlagsAtStop =>
        TraversalPlan.FlagsAtStop is { } flags
            ? new(
                flags.Carry,
                flags.Parity,
                flags.Auxiliary,
                flags.Zero,
                flags.Sign,
                flags.Overflow)
            : null;
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? FlagsAtStop
        { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Empty;
}

/// <summary>
/// Joins the exact one-iteration child equality fallthrough at 9403 to its independent
/// threshold reads and five ring stores, stopping before the 942F DEC.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEqualityFallthroughRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EqualFallthroughRawFileOffset;
    public const uint ThresholdEntryRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingEntryRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.ChildEntryRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.CompletionRawFileOffset;

    public static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal =
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.Project(
                capture.Traversal);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder
            effects = ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>();
        AddTraversalEffects(traversal, effects);

        uint? childEDX =
            traversal.IterationPlan is { MoveChildToEDXReached: true } iteration
                ? iteration.EDXAfterChildMove
                : null;
        uint? inheritedESI = capture.Traversal.Parent.Parent.EntryESI;

        if (!IsExactChildEqualityHandoff(traversal)
            || childEDX is not uint actualChildEDX)
        {
            return Finish(
                capture,
                traversal,
                thresholdRing: null,
                outcome: TraversalOutcome(traversal.Outcome),
                childEDX,
                inheritedESI,
                exactChildEqualityHandoff: false,
                freshGlobalUnused: capture.FreshChildGlobalValue.HasValue,
                freshEDXPlus8Unused: capture.FreshChildEDXPlus8.HasValue,
                effects,
                stopAt: traversal.StopAtRawFileOffsetExclusive,
                exactBoundedCompletion: traversal.ExactBoundedCompletion,
                exactCleanCompletion:
                    traversal.ExactCleanCompletion
                    && !capture.FreshChildGlobalValue.HasValue
                    && !capture.FreshChildEDXPlus8.HasValue);
        }

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: capture.FreshChildGlobalValue,
                ParentEntryEDX: actualChildEDX,
                FreshEDXPlus8: capture.FreshChildEDXPlus8,
                LateEntryESI: inheritedESI));
        AddThresholdRingEffects(thresholdRing, effects);

        bool thresholdBoundedCompletion =
            IsExactThresholdRingBoundedCompletion(thresholdRing);
        bool exactBoundedCompletion =
            traversal.ExactBoundedCompletion
            && thresholdBoundedCompletion;
        bool exactCleanCompletion =
            IsExactChildEqualityHandoff(traversal)
            && exactBoundedCompletion
            && (IsExactThresholdBypass(thresholdRing)
                || thresholdRing.ExactCleanCompletion
                    && thresholdRing.Outcome
                        == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .CompletedToBoundaryBeforeDec
                    && thresholdRing.Writes.Length == 5
                    && thresholdRing.Writes.All(write => write.Committed)
                    && thresholdRing.StopAtRawFileOffsetExclusive == CompletionRawFileOffset
                    && thresholdRing.ChildBoundaryReached
                    && !thresholdRing.ChildBoundaryInstructionEntered
                    && !thresholdRing.DecAtBoundaryEntered);

        return Finish(
            capture,
            traversal,
            thresholdRing,
            ThresholdOutcome(thresholdRing.Outcome),
            childEDX,
            inheritedESI,
            exactChildEqualityHandoff: true,
            freshGlobalUnused: false,
            freshEDXPlus8Unused:
                thresholdRing.ParentPlan.UnreachedFreshEDXPlus8CaptureProvided,
            effects,
            thresholdRing.StopAtRawFileOffsetExclusive,
            exactBoundedCompletion,
            exactCleanCompletion);
    }

    public static bool IsExactChildEqualityHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal)
    {
        CapacityFinalCalleeCommonTraversalIterationPlan? iteration =
            traversal.IterationPlan;
        return traversal.ExactParentUnequalHandoff
            && traversal.ParentNonzeroAdmission
            && traversal.ParentUnequalDestinationSelected
            && traversal.ParentChildTagDestinationRawFileOffset
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection
                    .IterationEntryRawFileOffset
            && traversal.EntryEBXDerivedFromParentChildTagShiftResult
            && traversal.EntryEDXDerivedFromParentFreshIndexedLoad
            && traversal.EntryEDIDerivedFromOriginalParentEntryEDI
            && traversal.NoRepeated93D8Read
            && traversal.ExactBoundedCompletion
            && traversal.ExactCleanCompletion
            && iteration is
            {
                Outcome:
                    CapacityFinalCalleeCommonTraversalIterationOutcome
                        .MaskedChildEqualsEntryEDIAt9403,
                EntryMoveReached: true,
                EntryEBXCaptured: true,
                LateEntryEDXCaptured: true,
                FreshChildReadReached: true,
                FreshChildDWORDCaptured: true,
                ChildZeroBranchReached: true,
                ChildZeroBranchTaken: false,
                MoveChildToEDXReached: true,
                FreshChildPlus4ReadReached: true,
                FreshChildPlus4Captured: true,
                AndEAXReached: true,
                CompareReached: true,
                EntryEDICaptured: true,
                LoopBranchReached: true,
                LoopBranchTaken: false,
                ExactSingleIterationEvidence: true,
                StopAtRawFileOffsetExclusive: ChildEqualityFallthroughRawFileOffset
            }
            && iteration.FreshChildDWORD is uint freshChild
            && freshChild != 0
            && iteration.EDXAfterChildMove == freshChild
            && iteration.ChildZeroBranchDestinationRawFileOffset
                == CapacityFinalCalleeCommonTraversalIterationProjection
                    .MoveChildToEDXRawFileOffset
            && iteration.LoopBranchSelectedDestinationRawFileOffset
                == ChildEqualityFallthroughRawFileOffset
            && iteration.CompareLeft == iteration.Capture.EntryEDI
            && iteration.CompareRight == traversal.DerivedEntryEDI
            && iteration.CompareLeft == iteration.CompareRight
            && iteration.MaskedChildDWORD == iteration.CompareLeft
            && iteration.CompareFlags is { Zero: true }
            && traversal.StopAtRawFileOffsetExclusive
                == ChildEqualityFallthroughRawFileOffset;
    }

    private static bool IsExactThresholdRingBoundedCompletion(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing)
    {
        if (IsExactThresholdBypass(thresholdRing))
        {
            return true;
        }

        return thresholdRing.ParentExactFallthroughHandoff
            && thresholdRing.ChildPlan is
            {
                Outcome:
                    CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                StopAtRawFileOffsetExclusive: CompletionRawFileOffset
            }
            && thresholdRing.Writes.Length == 5
            && thresholdRing.Writes.All(write => write.Committed)
            && thresholdRing.StopAtRawFileOffsetExclusive == CompletionRawFileOffset;
    }

    private static bool IsExactThresholdBypass(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing)
    {
        CapacityFinalCalleeCommonDualPointerThresholdPlan parent = thresholdRing.ParentPlan;
        if (!parent.FreshGlobalReadReached
            || !parent.FreshGlobalCaptured
            || !parent.EntryEDXCaptured
            || !parent.FreshEDXPlus8ReadReached
            || !parent.FreshEDXPlus8Captured
            || thresholdRing.ChildPlan is not null
            || !thresholdRing.Writes.IsEmpty)
        {
            return false;
        }

        return parent.Outcome switch
        {
            CapacityFinalCalleeCommonDualPointerThresholdOutcome.FirstUnsignedBelowBranchTarget =>
                parent.FirstCompareReached
                && parent.FirstJBReached
                && parent.FirstJBTaken == true
                && parent.FirstJBSelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .BranchTargetRawFileOffset
                && !parent.SecondCompareReached
                && parent.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .BranchTargetRawFileOffset,
            CapacityFinalCalleeCommonDualPointerThresholdOutcome.SecondUnsignedBelowBranchTarget =>
                parent.FirstCompareReached
                && parent.FirstJBReached
                && parent.FirstJBTaken == false
                && parent.SecondCompareReached
                && parent.SecondJBReached
                && parent.SecondJBTaken == true
                && parent.SecondJBSelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .BranchTargetRawFileOffset
                && parent.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .BranchTargetRawFileOffset,
            _ => false
        };
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal,
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome outcome,
        uint? childEDX,
        uint? inheritedESI,
        bool exactChildEqualityHandoff,
        bool freshGlobalUnused,
        bool freshEDXPlus8Unused,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder
            effects,
        uint stopAt,
        bool exactBoundedCompletion,
        bool exactCleanCompletion)
    {
        CapacityFinalCalleeCommonDualPointerThresholdFlags? thresholdFlags =
            thresholdRing?.FlagsAtStop;
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? flagsAtStop =
            thresholdFlags is { } knownThresholdFlags
                ? new(
                    knownThresholdFlags.Carry,
                    knownThresholdFlags.Parity,
                    knownThresholdFlags.Auxiliary,
                    knownThresholdFlags.Zero,
                    knownThresholdFlags.Sign,
                    knownThresholdFlags.Overflow)
                : ConvertFlags(traversal.FlagsAtStop);

        return new(capture, outcome, traversal)
        {
            ThresholdRingPlan = thresholdRing,
            DerivedChildEDX = childEDX,
            InheritedEntryESI = inheritedESI,
            ChildEDXDerivedFromIterationPlan =
                childEDX is uint actualChildEDX
                && traversal.IterationPlan?.EDXAfterChildMove == actualChildEDX,
            EntryESIInheritedFromTraversalParent =
                inheritedESI.HasValue
                && inheritedESI == capture.Traversal.Parent.Parent.EntryESI,
            ExactChildEqualityHandoff = exactChildEqualityHandoff,
            FreshChildGlobalValueCaptureProvidedButUnused = freshGlobalUnused,
            FreshChildEDXPlus8CaptureProvidedButUnused = freshEDXPlus8Unused,
            FlagsAtStop = flagsAtStop,
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt,
            OrderedMemoryEffects = effects.ToImmutable()
        };
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
        TraversalOutcome(
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentZeroToBounded93AC =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .ParentZeroToBounded93AC,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentEqualToBounded9403 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .ParentEqualToBounded9403,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentUnequalNotCleanAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .ParentUnequalNotCleanAt93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingEntryEBXAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingEntryEBXAt93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingLateEntryEDXAt93EE =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingLateEntryEDXAt93EE,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ZeroChildToBounded945C =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .ZeroChildToBounded945C,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MissingEntryEDIAt93FF =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingEntryEDIAt93FF,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MaskedChildEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MaskedChildEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .MaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4,
            _ => CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome.ParentStopped
        };

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
        ThresholdOutcome(
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingFreshChildGlobalValue,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshEDXPlus8 =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingFreshChildEDXPlus8,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .FirstUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .SecondUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .MissingLateEntryESIForFirstWrite =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingInheritedEntryESIForFirstWrite,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec =>
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec,
            _ => CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                .ChildHandoffNotExact
        };

    private static void AddTraversalEffects(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder
            effects)
    {
        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect effect
            in traversal.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .FreshTraversalChildDWORDRead,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .FreshTraversalChildPlus4Read
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
        }
    }

    private static void AddThresholdRingEffects(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder
            effects)
    {
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect effect
            in thresholdRing.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind kind =
                effect.Kind
                    == CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                        .FreshRead
                    ? effect.RawFileOffset
                        == CapacityFinalCalleeCommonDualPointerThresholdProjection
                            .FreshGlobalReadRawFileOffset
                        ? CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead
                        : CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read
                    : CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .RingWriteAttempt;
            effects.Add(new(
                kind,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed,
                ForwardedFromPriorWrite: false));
        }
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonTraversalIterationFlags? flags) =>
        flags is { } value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
