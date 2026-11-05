using System;
using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationCapture(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture FirstTraversal,
        uint? FreshSecondChildDWORD,
        uint? LateFreshSecondChildPlus4);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
{
    FirstTraversalNotEligible,
    MissingSecondEntryEBXAt93E4,
    MissingSecondEntryEDXAt93EE,
    MissingSecondFreshChildDWORDAt93F1,
    SecondZeroChildTo945C,
    MissingSecondFreshChildPlus4At93F9,
    MissingSecondEntryEDIAt93FF,
    SecondMaskedChildEqualsEntryEDIAt9403,
    SecondMaskedChildDiffersFromEntryEDITo93E4
}

public sealed record
    CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan(
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan FirstPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonTraversalIterationPlan? SecondIterationPlan { get; init; }
    public bool FirstLoopBranchSelected { get; init; }
    public uint? FirstLoopBranchDestinationRawFileOffset { get; init; }
    public bool SecondIterationAdmitted { get; init; }
    public uint? SecondEntryEBX { get; init; }
    public uint? SecondEntryEDX { get; init; }
    public uint? SecondEntryEDI { get; init; }
    public bool SecondEBXDerivedFromFirstEBXAfterDouble { get; init; }
    public bool SecondEDXDerivedFromFirstChildMove { get; init; }
    public bool SecondEDIDerivedFromOriginalParentEntryEDI { get; init; }
    public uint? OriginalParentEntryEDI => FirstPlan.DerivedEntryEDI;
    public uint? RootEntryESI =>
        FirstPlan.ParentPlan.ParentPlan.ParentLifecycle?.Capture.EntryESI;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsBeforeSecondEntryMove
        { get; init; }
    public bool SecondEntryMovePreservedFirstCompareFlags { get; init; }
    public bool FreshSecondChildDWORDCaptureProvidedButUnused { get; init; }
    public bool LateFreshSecondChildPlus4CaptureProvidedButUnused { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool SecondIterationTargetEntered => false;
    public bool ThirdIterationTargetEntered => false;
    public uint? SecondSelectedDestinationRawFileOffset =>
        SecondIterationPlan?.LoopBranchSelectedDestinationRawFileOffset
        ?? SecondIterationPlan?.ChildZeroBranchDestinationRawFileOffset;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Adds at most one generic 93E4 traversal iteration after a clean first
/// iteration takes its 9401 branch back to 93E4.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset;
    public const uint ZeroChildBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.ZeroChildBranchTargetRawFileOffset;
    public const uint LoopBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.LoopBranchTargetRawFileOffset;
    public const uint EqualFallthroughRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EqualFallthroughRawFileOffset;

    public static ImmutableArray<CapacityFinalCalleeCommonTraversalIterationInstructionEvidence>
        Instructions => CapacityFinalCalleeCommonTraversalIterationProjection.Instructions;

    public static CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan Project(
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan first =
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.Project(
                capture.FirstTraversal);
        CapacityFinalCalleeCommonTraversalIterationPlan? firstIteration =
            first.IterationPlan;
        bool firstLoopSelected =
            firstIteration is
            {
                Outcome:
                    CapacityFinalCalleeCommonTraversalIterationOutcome
                        .MaskedChildDiffersFromEntryEDITo93E4,
                LoopBranchReached: true,
                LoopBranchTaken: true,
                LoopBranchSelectedDestinationRawFileOffset: LoopBranchTargetRawFileOffset
            };
        bool admitted = first.ExactCleanCompletion && firstLoopSelected;
        bool secondChildProvidedButUnused =
            capture.FreshSecondChildDWORD.HasValue;
        bool secondTagProvidedButUnused =
            capture.LateFreshSecondChildPlus4.HasValue;

        if (!admitted || firstIteration is null)
        {
            return new(
                capture,
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .FirstTraversalNotEligible)
            {
                FirstPlan = first,
                FirstLoopBranchSelected = firstLoopSelected,
                FirstLoopBranchDestinationRawFileOffset =
                    firstIteration?.LoopBranchSelectedDestinationRawFileOffset,
                FreshSecondChildDWORDCaptureProvidedButUnused =
                    secondChildProvidedButUnused,
                LateFreshSecondChildPlus4CaptureProvidedButUnused =
                    secondTagProvidedButUnused,
                ExactBoundedCompletion = first.ExactBoundedCompletion,
                ExactCleanCompletion =
                    first.ExactCleanCompletion
                    && !secondChildProvidedButUnused
                    && !secondTagProvidedButUnused,
                FlagsAtStop = first.FlagsAtStop,
                StopAtRawFileOffsetExclusive = first.StopAtRawFileOffsetExclusive
            };
        }

        uint? secondEntryEBX = firstIteration.EBXAfterDouble;
        uint? secondEntryEDX = firstIteration.EDXAfterChildMove;
        uint? secondEntryEDI = first.DerivedEntryEDI;
        CapacityFinalCalleeCommonTraversalIterationFlags? incomingFlags =
            firstIteration.CompareFlags;
        CapacityFinalCalleeCommonTraversalIterationPlan second =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(
                new(
                    EntryEBX: secondEntryEBX,
                    LateEntryEDX: secondEntryEDX,
                    FreshChildDWORD: capture.FreshSecondChildDWORD,
                    LateFreshChildPlus4: capture.LateFreshSecondChildPlus4,
                    EntryEDI: secondEntryEDI));

        if (!second.Events.IsDefaultOrEmpty)
        {
            second = second with
            {
                Events = second.Events.SetItem(
                    0,
                    second.Events[0] with { Flags = incomingFlags })
            };
        }

        bool secondChildUnused =
            !second.FreshChildReadReached && capture.FreshSecondChildDWORD.HasValue;
        bool secondTagUnused =
            !second.FreshChildPlus4ReadReached
            && capture.LateFreshSecondChildPlus4.HasValue;
        bool bounded = IsExactBoundedIteration(second);
        bool clean =
            bounded
            && first.ExactCleanCompletion
            && !secondChildUnused
            && !secondTagUnused
            && IsCleanIterationExit(second);

        return new(capture, MapOutcome(second.Outcome))
        {
            FirstPlan = first,
            SecondIterationPlan = second,
            FirstLoopBranchSelected = true,
            FirstLoopBranchDestinationRawFileOffset =
                firstIteration.LoopBranchSelectedDestinationRawFileOffset,
            SecondIterationAdmitted = true,
            SecondEntryEBX = secondEntryEBX,
            SecondEntryEDX = secondEntryEDX,
            SecondEntryEDI = secondEntryEDI,
            SecondEBXDerivedFromFirstEBXAfterDouble =
                secondEntryEBX is uint ebx
                && firstIteration.EBXAfterDouble == ebx,
            SecondEDXDerivedFromFirstChildMove =
                secondEntryEDX is uint edx
                && firstIteration.EDXAfterChildMove == edx
                && firstIteration.FreshChildDWORD == edx,
            SecondEDIDerivedFromOriginalParentEntryEDI =
                secondEntryEDI is uint edi
                && first.DerivedEntryEDI == edi
                && first.ParentPlan.ParentPlan.ParentLifecycle?.Capture.EntryEDI == edi,
            FlagsBeforeSecondEntryMove = incomingFlags,
            SecondEntryMovePreservedFirstCompareFlags =
                second.Events.Length > 0
                && second.Events[0].Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX
                && second.Events[0].Flags == incomingFlags,
            FreshSecondChildDWORDCaptureProvidedButUnused = secondChildUnused,
            LateFreshSecondChildPlus4CaptureProvidedButUnused = secondTagUnused,
            ExactBoundedCompletion = bounded,
            ExactCleanCompletion = clean,
            FlagsAtStop = second.FlagsAtStop,
            StopAtRawFileOffsetExclusive = second.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactBoundedIteration(
        CapacityFinalCalleeCommonTraversalIterationPlan iteration)
    {
        if (iteration.Outcome
            == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C)
        {
            return iteration.FreshChildReadReached
                && iteration.FreshChildDWORDCaptured
                && iteration.ChildTestFlags is { Zero: true }
                && iteration.ChildZeroBranchReached
                && iteration.ChildZeroBranchTaken == true
                && iteration.ChildZeroBranchDestinationRawFileOffset
                    == ZeroChildBranchTargetRawFileOffset
                && !iteration.MoveChildToEDXReached
                && !iteration.FreshChildPlus4ReadReached
                && !iteration.AndEAXReached
                && !iteration.CompareReached
                && !iteration.LoopBranchReached
                && iteration.StopAtRawFileOffsetExclusive
                    == ZeroChildBranchTargetRawFileOffset
                && !iteration.TargetEntered;
        }

        if (iteration.Outcome is
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            or CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildDiffersFromEntryEDITo93E4)
        {
            uint expectedDestination =
                iteration.Outcome
                    == CapacityFinalCalleeCommonTraversalIterationOutcome
                        .MaskedChildDiffersFromEntryEDITo93E4
                    ? LoopBranchTargetRawFileOffset
                    : EqualFallthroughRawFileOffset;
            return iteration.FreshChildReadReached
                && iteration.FreshChildDWORDCaptured
                && iteration.FreshChildPlus4ReadReached
                && iteration.FreshChildPlus4Captured
                && iteration.AndEAXReached
                && iteration.CompareReached
                && iteration.EntryEDICaptured
                && iteration.LoopBranchReached
                && iteration.LoopBranchSelectedDestinationRawFileOffset
                    == expectedDestination
                && iteration.StopAtRawFileOffsetExclusive == expectedDestination
                && iteration.ExactSingleIterationEvidence
                && !iteration.TargetEntered;
        }

        return false;
    }

    private static bool IsCleanIterationExit(
        CapacityFinalCalleeCommonTraversalIterationPlan iteration) =>
        iteration.Outcome
            == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C
            ? !iteration.Capture.LateFreshChildPlus4.HasValue
            : iteration.FreshChildPlus4Captured
                && iteration.EntryEDICaptured
                && iteration.ExactSingleIterationEvidence
                && !iteration.UnreachedLateEntryEDXCaptureProvided
                && !iteration.UnreachedFreshChildDWORDCaptureProvided
                && !iteration.UnreachedLateFreshChildPlus4CaptureProvided
                && !iteration.UnreachedEntryEDICaptureProvided;

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
        MapOutcome(CapacityFinalCalleeCommonTraversalIterationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEBXAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondEntryEBXAt93E4,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingLateEntryEDXAt93EE =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondEntryEDXAt93EE,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondZeroChildTo945C,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEDIAt93FF =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondEntryEDIAt93FF,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondMaskedChildEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondMaskedChildDiffersFromEntryEDITo93E4,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
