using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationCapture Traversal,
        uint? FreshThresholdGlobalValue,
        uint? FreshThresholdEDXPlus8);

public enum CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
{
    FirstTraversalNotEligible,
    MissingSecondEntryEBXAt93E4,
    MissingSecondEntryEDXAt93EE,
    MissingSecondFreshChildDWORDAt93F1,
    SecondZeroChildTo945C,
    MissingSecondFreshChildPlus4At93F9,
    MissingSecondEntryEDIAt93FF,
    SecondMaskedChildDiffersFromEntryEDITo93E4,
    SecondEqualityHandoffNotExact,
    MissingFreshThresholdGlobalValue,
    MissingFreshThresholdEDXPlus8,
    FirstUnsignedBelowBypassesRing,
    SecondUnsignedBelowBypassesRing,
    MissingRootEntryESIForFirstWrite,
    RingHandoffNotExact,
    CompletedToBoundaryBeforeDec
}

public sealed record
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan SecondTraversalPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? ThresholdRingPlan
        { get; init; }
    public uint? DerivedChildEDX { get; init; }
    public uint? OriginalEntryEDI { get; init; }
    public uint? OriginalRootEntryESI { get; init; }
    public bool ChildEDXDerivedFromSecondIteration { get; init; }
    public bool EntryEDIDerivedFromSecondIteration { get; init; }
    public bool EntryESIInheritedFromOriginalRoot { get; init; }
    public bool ExactSecondChildEqualityHandoff { get; init; }
    public bool ThresholdGateEntered => ThresholdRingPlan is not null;
    public bool RingBoundaryReached => ThresholdRingPlan?.ChildBoundaryReached == true;
    public bool RingBoundaryInstructionEntered =>
        ThresholdRingPlan?.ChildBoundaryInstructionEntered == true;
    public bool FreshThresholdGlobalValueCaptureProvidedButUnused { get; init; }
    public bool FreshThresholdEDXPlus8CaptureProvidedButUnused { get; init; }
    public bool SecondChildDWORDCaptureProvidedButUnused =>
        SecondTraversalPlan.FreshSecondChildDWORDCaptureProvidedButUnused;
    public bool SecondChildPlus4CaptureProvidedButUnused =>
        SecondTraversalPlan.LateFreshSecondChildPlus4CaptureProvidedButUnused;
    public ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite> Writes =>
        ThresholdRingPlan?.Writes
        ?? ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite>.Empty;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Empty;
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins one admitted second traversal equality to the shared threshold and ring-store helper.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
            .EntryRawFileOffset;
    public const uint ChildEqualityFallthroughRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
            .EqualFallthroughRawFileOffset;
    public const uint RingCompletionRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection
            .CompletionRawFileOffset;

    public static CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan traversal =
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection.Project(
                capture.Traversal);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder effects =
                ImmutableArray.CreateBuilder<
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>();
        AddTraversalEffects(traversal.FirstPlan, effects);
        AddSecondIterationReads(traversal.SecondIterationPlan, effects);

        uint? childEDX = traversal.SecondIterationPlan is
            { MoveChildToEDXReached: true } second
                ? second.EDXAfterChildMove
                : null;
        uint? originalEDI = traversal.SecondEntryEDI;
        uint? originalRootESI = traversal.RootEntryESI;

        if (!IsExactSecondChildEqualityHandoff(traversal)
            || childEDX is not uint actualChildEDX)
        {
            return Finish(
                capture,
                traversal,
                thresholdRing: null,
                OutcomeForTraversal(traversal),
                childEDX,
                originalEDI,
                originalRootESI,
                exactSecondChildEqualityHandoff: false,
                freshGlobalUnused: capture.FreshThresholdGlobalValue.HasValue,
                freshEDXPlus8Unused: capture.FreshThresholdEDXPlus8.HasValue,
                effects,
                traversal.StopAtRawFileOffsetExclusive,
                traversal.ExactBoundedCompletion,
                traversal.ExactCleanCompletion
                    && !capture.FreshThresholdGlobalValue.HasValue
                    && !capture.FreshThresholdEDXPlus8.HasValue);
        }

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: capture.FreshThresholdGlobalValue,
                ParentEntryEDX: actualChildEDX,
                FreshEDXPlus8: capture.FreshThresholdEDXPlus8,
                LateEntryESI: originalRootESI));
        AddThresholdRingEffects(thresholdRing, effects);

        bool bounded = traversal.ExactBoundedCompletion
            && IsExactThresholdRingBoundedCompletion(thresholdRing);
        bool clean = IsExactSecondChildEqualityHandoff(traversal)
            && bounded
            && (IsExactThresholdBypass(thresholdRing)
                || thresholdRing.ExactCleanCompletion
                    && thresholdRing.Outcome
                        == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                            .CompletedToBoundaryBeforeDec
                    && thresholdRing.Writes.Length == 5
                    && thresholdRing.Writes.All(write => write.Committed)
                    && thresholdRing.StopAtRawFileOffsetExclusive
                        == RingCompletionRawFileOffset
                    && thresholdRing.ChildBoundaryReached
                    && !thresholdRing.ChildBoundaryInstructionEntered
                    && !thresholdRing.DecAtBoundaryEntered);

        return Finish(
            capture,
            traversal,
            thresholdRing,
            OutcomeForThreshold(thresholdRing),
            childEDX,
            originalEDI,
            originalRootESI,
            exactSecondChildEqualityHandoff: true,
            freshGlobalUnused: false,
            freshEDXPlus8Unused:
                thresholdRing.ParentPlan.UnreachedFreshEDXPlus8CaptureProvided,
            effects,
            thresholdRing.StopAtRawFileOffsetExclusive,
            bounded,
            clean);
    }

    public static bool IsExactSecondChildEqualityHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan traversal)
    {
        CapacityFinalCalleeCommonTraversalIterationPlan? second =
            traversal.SecondIterationPlan;
        return traversal.FirstPlan.ExactCleanCompletion
            && traversal.FirstLoopBranchSelected
            && traversal.FirstLoopBranchDestinationRawFileOffset
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                    .LoopBranchTargetRawFileOffset
            && traversal.SecondIterationAdmitted
            && traversal.SecondEBXDerivedFromFirstEBXAfterDouble
            && traversal.SecondEDXDerivedFromFirstChildMove
            && traversal.SecondEDIDerivedFromOriginalParentEntryEDI
            && traversal.SecondEntryMovePreservedFirstCompareFlags
            && traversal.ExactCleanCompletion
            && second is
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
            && second.FreshChildDWORD is uint freshChild
            && freshChild != 0
            && second.EDXAfterChildMove == freshChild
            && second.LoopBranchSelectedDestinationRawFileOffset
                == ChildEqualityFallthroughRawFileOffset
            && second.CompareLeft == traversal.SecondEntryEDI
            && second.CompareRight == traversal.SecondEntryEDI
            && second.MaskedChildDWORD == second.CompareLeft
            && second.CompareFlags is { Zero: true }
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
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset
            }
            && thresholdRing.Writes.Length == 5
            && thresholdRing.Writes.All(write => write.Committed)
            && thresholdRing.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset;
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

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan traversal,
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing,
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome outcome,
        uint? childEDX,
        uint? originalEDI,
        uint? originalRootESI,
        bool exactSecondChildEqualityHandoff,
        bool freshGlobalUnused,
        bool freshEDXPlus8Unused,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder effects,
        uint stopAt,
        bool exactBoundedCompletion,
        bool exactCleanCompletion)
    {
        CapacityFinalCalleeCommonDualPointerThresholdFlags? thresholdFlags =
            thresholdRing?.FlagsAtStop;
        CapacityFinalCalleeCommonTraversalIterationFlags? flagsAtStop =
            thresholdFlags is { } known
                ? new(
                    known.Carry,
                    known.Parity,
                    known.Auxiliary,
                    known.Zero,
                    known.Sign,
                    known.Overflow)
                : traversal.FlagsAtStop;

        return new(capture, outcome)
        {
            SecondTraversalPlan = traversal,
            ThresholdRingPlan = thresholdRing,
            DerivedChildEDX = childEDX,
            OriginalEntryEDI = originalEDI,
            OriginalRootEntryESI = originalRootESI,
            ChildEDXDerivedFromSecondIteration =
                childEDX is uint actual
                && traversal.SecondIterationPlan?.EDXAfterChildMove == actual,
            EntryEDIDerivedFromSecondIteration =
                originalEDI.HasValue && originalEDI == traversal.SecondEntryEDI,
            EntryESIInheritedFromOriginalRoot =
                originalRootESI.HasValue && originalRootESI == traversal.RootEntryESI,
            ExactSecondChildEqualityHandoff = exactSecondChildEqualityHandoff,
            FreshThresholdGlobalValueCaptureProvidedButUnused = freshGlobalUnused,
            FreshThresholdEDXPlus8CaptureProvidedButUnused = freshEDXPlus8Unused,
            FlagsAtStop = flagsAtStop,
            OrderedMemoryEffects = effects.ToImmutable(),
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
        OutcomeForTraversal(
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan traversal) =>
        traversal.Outcome switch
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .FirstTraversalNotEligible =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .FirstTraversalNotEligible,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .MissingSecondEntryEBXAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingSecondEntryEBXAt93E4,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .MissingSecondEntryEDXAt93EE =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingSecondEntryEDXAt93EE,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .MissingSecondFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingSecondFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .SecondZeroChildTo945C =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondZeroChildTo945C,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .MissingSecondFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingSecondFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .MissingSecondEntryEDIAt93FF =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingSecondEntryEDIAt93FF,
            CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                .SecondMaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondMaskedChildDiffersFromEntryEDITo93E4,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondEqualityHandoffNotExact
        };

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
        OutcomeForThreshold(
            CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing) =>
        thresholdRing.Outcome switch
        {
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingFreshThresholdGlobalValue,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshEDXPlus8 =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingFreshThresholdEDXPlus8,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .FirstUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .SecondUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .MissingLateEntryESIForFirstWrite =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingRootEntryESIForFirstWrite,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .RingHandoffNotExact
        };

    private static void AddTraversalEffects(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan traversal,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder effects)
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

    private static void AddSecondIterationReads(
        CapacityFinalCalleeCommonTraversalIterationPlan? iteration,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder effects)
    {
        if (iteration is null)
        {
            return;
        }

        foreach (CapacityFinalCalleeCommonTraversalIterationRead read in iteration.Reads)
        {
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind kind =
                read.RawFileOffset
                    == CapacityFinalCalleeCommonTraversalIterationProjection
                        .FreshChildReadRawFileOffset
                    ? CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildDWORDRead
                    : CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildPlus4Read;
            effects.Add(new(
                kind,
                read.RawFileOffset,
                read.Address,
                read.AddressCaptured,
                read.Value,
                read.ValueCaptured,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }
    }

    private static void AddThresholdRingEffects(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect>.Builder effects)
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
}
