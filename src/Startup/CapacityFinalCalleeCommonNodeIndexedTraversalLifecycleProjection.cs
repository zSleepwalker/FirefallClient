using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture(
    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture Parent,
    uint? FreshChildDWORD,
    uint? LateFreshChildPlus4);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
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
    MaskedChildDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    FreshChildDWORDRead,
    FreshChildPlus4Read
}

public readonly record struct CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan(
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonTraversalIterationPlan? IterationPlan { get; init; }
    public bool ParentNonzeroAdmission { get; init; }
    public bool ParentUnequalDestinationSelected { get; init; }
    public uint? ParentInitialDestinationRawFileOffset { get; init; }
    public uint? ParentChildTagDestinationRawFileOffset { get; init; }
    public bool InternalEntryReached => IterationPlan?.EntryMoveReached == true;
    public uint? InternalEntryRawFileOffset =>
        InternalEntryReached
            ? CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset
            : null;
    public uint? DerivedEntryEBX { get; init; }
    public uint? DerivedEntryEDX { get; init; }
    public uint? DerivedEntryEDI { get; init; }
    public bool EntryEBXDerivedFromParentChildTagShiftResult { get; init; }
    public bool EntryEDXDerivedFromParentFreshIndexedLoad { get; init; }
    public bool EntryEDIDerivedFromOriginalParentEntryEDI { get; init; }
    public bool FreshChildDWORDCaptureProvidedButUnused { get; init; }
    public bool LateFreshChildPlus4CaptureProvidedButUnused { get; init; }
    public bool ExactParentUnequalHandoff { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool NoRepeated93D8Read =>
        !ParentPlan.Duplicate93D8Read
        && OrderedMemoryEffects.Count(effect =>
            effect.Kind
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                    .ParentFreshTagDWORDRead) <= 1;
    public bool IterationTargetEntered => IterationPlan?.TargetEntered == true;
    public uint? IterationSelectedDestinationRawFileOffset =>
        IterationPlan?.LoopBranchSelectedDestinationRawFileOffset
        ?? IterationPlan?.ChildZeroBranchDestinationRawFileOffset;
    public CapacityFinalCalleeCommonTraversalIterationFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>
                .Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact indexed/tag unequal edge into one bounded 93E4 traversal iteration.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.EntryRawFileOffset;
    public const uint IterationEntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset;
    public const uint ChildZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.ZeroChildBranchTargetRawFileOffset;
    public const uint LoopDestinationRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.LoopBranchTargetRawFileOffset;
    public const uint EqualDestinationRawFileOffset =
        CapacityFinalCalleeCommonTraversalIterationProjection.EqualFallthroughRawFileOffset;

    public static CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(capture.Parent);
        ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>
            .Builder effects =
                ImmutableArray.CreateBuilder<
                    CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>();
        AddParentEffects(parent, effects);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan? parentTagPlan =
            parent.ChildPlan;
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan? parentTagGate =
            parentTagPlan?.ChildPlan;
        uint? derivedEntryEBX = parentTagGate?.ShiftResult;
        uint? derivedEntryEDX = parentTagPlan?.ChildEntryEDX;
        uint? derivedEntryEDI = parent.DerivedParentEntryEDI;
        uint? parentChildTagDestination =
            parentTagGate?.JzSelectedDestinationRawFileOffset;
        uint? parentInitialDestination =
            parent.ParentPlan.FreshMaskGate?.JnzSelectedDestinationRawFileOffset;

        bool exactParentUnequalHandoff = IsExactParentUnequalHandoff(parent);
        if (!exactParentUnequalHandoff)
        {
            bool unusedChild = capture.FreshChildDWORD.HasValue;
            bool unusedChild4 = capture.LateFreshChildPlus4.HasValue;
            bool exactBounded = IsExactParentBoundedCompletion(parent);
            bool exactClean =
                exactBounded
                && parent.ExactCleanCompletion
                && !unusedChild
                && !unusedChild4;

            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome outcome =
                ParentOutcome(parent);
            return new(capture, outcome)
            {
                ParentPlan = parent,
                ParentNonzeroAdmission = parent.ParentNonzeroAdmission,
                ParentUnequalDestinationSelected =
                    parentChildTagDestination == IterationEntryRawFileOffset,
                ParentInitialDestinationRawFileOffset = parentInitialDestination,
                ParentChildTagDestinationRawFileOffset = parentChildTagDestination,
                DerivedEntryEBX = derivedEntryEBX,
                DerivedEntryEDX = derivedEntryEDX,
                DerivedEntryEDI = derivedEntryEDI,
                EntryEBXDerivedFromParentChildTagShiftResult = false,
                EntryEDXDerivedFromParentFreshIndexedLoad = false,
                EntryEDIDerivedFromOriginalParentEntryEDI = false,
                FreshChildDWORDCaptureProvidedButUnused = unusedChild,
                LateFreshChildPlus4CaptureProvidedButUnused = unusedChild4,
                ExactParentUnequalHandoff = false,
                ExactBoundedCompletion = exactBounded,
                ExactCleanCompletion = exactClean,
                FlagsAtStop = ConvertFlags(parent.FlagsAtStop),
                OrderedMemoryEffects = effects.ToImmutable(),
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTraversalIterationPlan iteration =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(
                new(
                    EntryEBX: derivedEntryEBX,
                    LateEntryEDX: derivedEntryEDX,
                    FreshChildDWORD: capture.FreshChildDWORD,
                    LateFreshChildPlus4: capture.LateFreshChildPlus4,
                    EntryEDI: derivedEntryEDI));
        AddIterationEffects(iteration, effects);

        bool freshChildUnused =
            !iteration.FreshChildReadReached && capture.FreshChildDWORD.HasValue;
        bool freshChild4Unused =
            !iteration.FreshChildPlus4ReadReached && capture.LateFreshChildPlus4.HasValue;
        bool exactBoundedIteration = IsExactBoundedIteration(iteration);
        bool exactCleanIteration =
            exactBoundedIteration
            && iteration.FreshChildReadReached
            && iteration.FreshChildDWORDCaptured
            && !freshChildUnused
            && !freshChild4Unused
            && IsCleanIterationExit(iteration);

        CapacityFinalCalleeCommonTraversalIterationFlags? flagsAtStop =
            iteration.FlagsAtStop
            ?? (!iteration.EntryEBXCaptured ? ConvertFlags(parent.FlagsAtStop) : null);

        return new(capture, IterationOutcome(iteration.Outcome))
        {
            ParentPlan = parent,
            IterationPlan = iteration,
            ParentNonzeroAdmission = true,
            ParentUnequalDestinationSelected = true,
            ParentInitialDestinationRawFileOffset = parentInitialDestination,
            ParentChildTagDestinationRawFileOffset = parentChildTagDestination,
            DerivedEntryEBX = derivedEntryEBX,
            DerivedEntryEDX = derivedEntryEDX,
            DerivedEntryEDI = derivedEntryEDI,
            EntryEBXDerivedFromParentChildTagShiftResult =
                derivedEntryEBX is uint entryEBX
                && parentTagGate?.ShiftResult == entryEBX,
            EntryEDXDerivedFromParentFreshIndexedLoad =
                derivedEntryEDX is uint entryEDX
                && parentTagPlan?.ChildEntryEDX == entryEDX
                && parentTagPlan?.ParentPlan.FreshEDX == entryEDX,
            EntryEDIDerivedFromOriginalParentEntryEDI =
                derivedEntryEDI is uint entryEDI
                && parent.DerivedParentEntryEDI == entryEDI
                && parent.ParentPlan.ParentLifecycle?.Capture.EntryEDI == entryEDI,
            FreshChildDWORDCaptureProvidedButUnused = freshChildUnused,
            LateFreshChildPlus4CaptureProvidedButUnused = freshChild4Unused,
            ExactParentUnequalHandoff = true,
            ExactBoundedCompletion = exactBoundedIteration,
            ExactCleanCompletion = exactCleanIteration,
            FlagsAtStop = flagsAtStop,
            OrderedMemoryEffects = effects.ToImmutable(),
            StopAtRawFileOffsetExclusive = iteration.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentUnequalHandoff(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent) =>
        IsExactParentUnequalRoute(parent)
        && parent.ExactCleanCompletion
        && parent.ParentPlan.ExactCleanExitAdmitted
        && !parent.ParentPlan.LateFreshGlobalValueProvidedButUnused
        && !parent.ParentPlan.UnreachedLateFreshGlobalValueCaptureProvided
        && parent.FreshIndexedDWORDCaptureProvidedButUnused == false
        && parent.FreshEDXPlus4CaptureProvidedButUnused == false
        && parent.ParentPlan.ParentLifecycle?.Capture.EntryEDI
            == parent.DerivedParentEntryEDI;

    private static bool IsExactParentUnequalRoute(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent) =>
        parent.ParentNonzeroAdmission
        && parent.ParentBoundedDestinationSelected
        && parent.ExactBoundedCompletion
        && !parent.Duplicate93D8Read
        && parent.ChildPlan is
        {
            ExactParentHandoffCompleted: true,
            Outcome:
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
                    .ChildUnequalToFallThrough93E4,
            ChildBoundedTargetNotEntered: true,
            StopAtRawFileOffsetExclusive: IterationEntryRawFileOffset,
            LateEntryEDICaptureProvidedButUnused: false,
            FreshEDXPlus4CaptureProvidedButUnused: false,
            ParentPlan:
            {
                IndexedDWORDReadAttempted: true,
                IndexedDWORDReadCompleted: true,
                UnreachedFreshIndexedDWORDObservationProvided: false
            },
            ChildPlan:
            {
                ShiftReached: true,
                CompareReached: true,
                JzReached: true,
                JzTaken: false,
                JzSelectedDestinationRawFileOffset: IterationEntryRawFileOffset,
                StopAtRawFileOffsetExclusive: IterationEntryRawFileOffset,
                ExactCleanBoundedDestination: true,
                EntryEDICaptureProvidedButUnused: false,
                EntryEDXCaptureProvidedButUnused: false,
                FreshEDXPlus4CaptureProvidedButUnused: false
            }
        };

    private static bool IsExactParentBoundedCompletion(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent)
    {
        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                .ParentZeroToBounded93AC)
        {
            return parent.ParentBoundedDestinationSelected
                && parent.ParentPlan.FreshMaskGate is
                {
                    JnzReached: true,
                    JnzTaken: false,
                    JnzSelectedDestinationRawFileOffset:
                        CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
                            .ZeroDestinationRawFileOffset,
                    ExactCleanBoundedDestination: true
                }
                && parent.ZeroParentDestinationNotEntered
                && parent.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
                        .ZeroDestinationRawFileOffset;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                .ChildEqualToBounded9403)
        {
            return parent.ParentBoundedDestinationSelected
                && parent.ChildBoundedDestinationNotEntered
                && parent.ChildPlan?.ChildPlan is
                {
                    JzReached: true,
                    JzTaken: true,
                    JzSelectedDestinationRawFileOffset:
                        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                            .EqualDestinationRawFileOffset,
                    ExactCleanBoundedDestination: true
                }
                && parent.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                        .EqualDestinationRawFileOffset;
        }

        return IsExactParentUnequalRoute(parent);
    }

    private static bool IsExactBoundedIteration(
        CapacityFinalCalleeCommonTraversalIterationPlan iteration)
    {
        if (iteration.Outcome
            == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C)
        {
            return iteration.FreshChildReadReached
                && iteration.FreshChildDWORDCaptured
                && iteration.ChildTestFlags is
                {
                    Zero: true
                }
                && iteration.ChildZeroBranchReached
                && iteration.ChildZeroBranchTaken == true
                && iteration.ChildZeroBranchDestinationRawFileOffset
                    == ChildZeroDestinationRawFileOffset
                && !iteration.MoveChildToEDXReached
                && !iteration.FreshChildPlus4ReadReached
                && !iteration.AndEAXReached
                && !iteration.CompareReached
                && !iteration.LoopBranchReached
                && iteration.StopAtRawFileOffsetExclusive
                    == ChildZeroDestinationRawFileOffset
                && !iteration.TargetEntered;
        }

        if (iteration.Outcome is
                CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            or CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildDiffersFromEntryEDITo93E4)
        {
            uint? expectedDestination =
                iteration.Outcome
                    == CapacityFinalCalleeCommonTraversalIterationOutcome
                        .MaskedChildDiffersFromEntryEDITo93E4
                    ? LoopDestinationRawFileOffset
                    : EqualDestinationRawFileOffset;
            return iteration.FreshChildReadReached
                && iteration.FreshChildDWORDCaptured
                && iteration.FreshChildPlus4ReadReached
                && iteration.FreshChildPlus4Captured
                && iteration.AndEAXReached
                && iteration.CompareReached
                && iteration.LoopBranchReached
                && iteration.LoopBranchSelectedDestinationRawFileOffset
                    == expectedDestination
                && (expectedDestination is LoopDestinationRawFileOffset
                    or EqualDestinationRawFileOffset)
                && iteration.StopAtRawFileOffsetExclusive == expectedDestination
                && !iteration.TargetEntered;
        }

        return false;
    }

    private static bool IsCleanIterationExit(
        CapacityFinalCalleeCommonTraversalIterationPlan iteration) =>
        iteration.Outcome
            == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C
            ? !iteration.Capture.LateFreshChildPlus4.HasValue
            : (iteration.Outcome is
                    CapacityFinalCalleeCommonTraversalIterationOutcome
                        .MaskedChildEqualsEntryEDIAt9403
                or CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4)
                && iteration.FreshChildPlus4Captured
                && iteration.EntryEDICaptured
                && iteration.ExactSingleIterationEvidence
                && !iteration.UnreachedLateEntryEDXCaptureProvided
                && !iteration.UnreachedFreshChildDWORDCaptureProvided
                && !iteration.UnreachedLateFreshChildPlus4CaptureProvided
                && !iteration.UnreachedEntryEDICaptureProvided;

    private static CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
        ParentOutcome(CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent)
    {
        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC)
        {
            return CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentZeroToBounded93AC;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildEqualToBounded9403)
        {
            return CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentEqualToBounded9403;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                .ChildUnequalToFallThrough93E4)
        {
            return CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                .ParentUnequalNotCleanAt93E4;
        }

        return CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.ParentStopped;
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
        IterationOutcome(CapacityFinalCalleeCommonTraversalIterationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEBXAt93E4 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingEntryEBXAt93E4,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingLateEntryEDXAt93EE =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingLateEntryEDXAt93EE,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ZeroChildToBounded945C,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEDIAt93FF =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingEntryEDIAt93FF,
            CapacityFinalCalleeCommonTraversalIterationOutcome
                .MaskedChildEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildEqualsEntryEDIAt9403,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
        };

    private static void AddParentEffects(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent,
        ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>
            .Builder effects)
    {
        foreach (CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect effect
            in parent.OrderedEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                            .ParentFreshTagDWORDRead
                };
            effects.Add(new(
                kind,
                effect.RawFileOffset,
                effect.Address,
                effect.Address.HasValue,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed,
                effect.ForwardedFromPriorWrite));
        }
    }

    private static void AddIterationEffects(
        CapacityFinalCalleeCommonTraversalIterationPlan iteration,
        ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffect>
            .Builder effects)
    {
        foreach (CapacityFinalCalleeCommonTraversalIterationRead read in iteration.Reads)
        {
            effects.Add(new(
                read.RawFileOffset
                    == CapacityFinalCalleeCommonTraversalIterationProjection
                        .FreshChildReadRawFileOffset
                    ? CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildDWORDRead
                    : CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildPlus4Read,
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

    private static CapacityFinalCalleeCommonTraversalIterationFlags? ConvertFlags(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? flags) =>
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
