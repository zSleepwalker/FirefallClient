using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleCapture(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture Parent,
        uint? FreshThresholdGlobalValue,
        uint? FreshThresholdEDXPlus8);

public enum CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
{
    ParentStopped,
    ParentZeroToBounded93AC,
    ParentNotCleanAt93C0,
    ParentEqualityNotSelected,
    ParentEqualityHandoffNotExact,
    MissingDerivedEntryEDX,
    MissingFreshThresholdGlobalValue,
    MissingFreshThresholdEDXPlus8,
    FirstUnsignedBelowBypassesRing,
    SecondUnsignedBelowBypassesRing,
    MissingInheritedEntryESIForFirstWrite,
    ThresholdHandoffNotExact,
    CompletedToBoundaryBeforeDec
}

public enum CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    ThresholdFreshGlobalRead,
    ThresholdFreshEDXPlus8Read,
    RingWriteAttempt
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind Kind,
        uint RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool ValueCaptured,
        bool Attempted,
        bool Committed,
        bool ForwardedFromPriorWrite);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public sealed record CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan(
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? ThresholdRingPlan
        { get; init; }
    public uint? DerivedEntryEDX { get; init; }
    public uint? InheritedEntryESI { get; init; }
    public uint? OriginalEntryEDI { get; init; }
    public bool EntryEDXDerivedFromFreshIndexedRead { get; init; }
    public bool EntryESIInheritedFromOriginalParentCapture { get; init; }
    public bool EntryEDIDerivedFromOriginalIndexedSizeGate { get; init; }
    public bool ParentEqualitySelected { get; init; }
    public bool ThresholdGateEntered => ThresholdRingPlan is not null;
    public bool ThresholdFallthroughSelected =>
        ThresholdRingPlan?.ParentExactFallthroughHandoff == true;
    public bool RingBoundaryReached => ThresholdRingPlan?.ChildBoundaryReached == true;
    public bool RingBoundaryInstructionEntered =>
        ThresholdRingPlan?.ChildBoundaryInstructionEntered == true;
    public bool ParentChildDWORDConsumed => false;
    public bool ParentIterationPlanExists => false;
    public bool FreshThresholdGlobalValueCaptureProvidedButUnused { get; init; }
    public bool FreshThresholdEDXPlus8CaptureProvidedButUnused { get; init; }
    public CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome? ThresholdRingOutcome =>
        ThresholdRingPlan?.Outcome;
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags?
        ParentFlagsAtThresholdEntry { get; init; }
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags?
        FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite> Writes =>
        ThresholdRingPlan?.Writes
        ?? ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>
                .Empty;
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered => false;
    public bool DecAtBoundaryEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Routes the outer 9403 indexed/tag equality into fresh dual-threshold reads and five ring
/// stores, stopping at 942F before the DEC.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.EntryRawFileOffset;
    public const uint OuterEqualityRawFileOffset =
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.ChildEqualDestinationRawFileOffset;
    public const uint ThresholdEntryRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingEntryRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.ChildEntryRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.CompletionRawFileOffset;

    public static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(capture.Parent);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>.Builder
            effects = ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>();
        AddParentEffects(parent, effects);

        uint? derivedEDX = parent.ChildPlan?.ChildEntryEDX;
        uint? inheritedESI = capture.Parent.Parent.EntryESI;
        uint? originalEDI = capture.Parent.Parent.EntryEDI;
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan? indexedTagPlan =
            parent.ChildPlan;
        bool entryEDXDerived =
            derivedEDX is uint edx
            && indexedTagPlan is
            {
                ParentPlan.IndexedDWORDReadCompleted: true,
                ParentPlan.FreshEDX: var freshEDX,
                ChildEntryEDX: var childEDX
            }
            && freshEDX == edx
            && childEDX == edx;
        bool entryESIDerived =
            inheritedESI is uint esi
            && capture.Parent.Parent.EntryESI == esi;
        bool entryEDIDerived =
            originalEDI is uint edi
            && parent.DerivedParentEntryEDI == edi
            && parent.ChildPlan?.ChildEntryEDI == edi;

        if (!IsExactOuterEquality(parent))
        {
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome parentOutcome =
                ParentOutcome(parent);
            bool globalUnused = capture.FreshThresholdGlobalValue.HasValue;
            bool pointerUnused = capture.FreshThresholdEDXPlus8.HasValue;
            return Finish(
                capture,
                parent,
                thresholdRing: null,
                parentOutcome,
                derivedEDX,
                inheritedESI,
                originalEDI,
                entryEDXDerived,
                entryESIDerived,
                entryEDIDerived,
                globalUnused,
                pointerUnused,
                effects,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                exactBoundedCompletion: parent.ExactBoundedCompletion,
                exactCleanCompletion:
                    parent.ExactCleanCompletion
                    && !globalUnused
                    && !pointerUnused);
        }

        if (!entryEDXDerived || derivedEDX is not uint actualEDX)
        {
            return Finish(
                capture,
                parent,
                thresholdRing: null,
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingDerivedEntryEDX,
                derivedEDX,
                inheritedESI,
                originalEDI,
                entryEDXDerived,
                entryESIDerived,
                entryEDIDerived,
                capture.FreshThresholdGlobalValue.HasValue,
                capture.FreshThresholdEDXPlus8.HasValue,
                effects,
                stopAt: OuterEqualityRawFileOffset,
                exactBoundedCompletion: false,
                exactCleanCompletion: false);
        }

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: capture.FreshThresholdGlobalValue,
                ParentEntryEDX: actualEDX,
                FreshEDXPlus8: capture.FreshThresholdEDXPlus8,
                LateEntryESI: inheritedESI));
        AddThresholdRingEffects(thresholdRing, effects);

        bool exactThresholdBypass = IsExactThresholdBypass(thresholdRing);
        bool exactRingCompletion =
            thresholdRing.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && thresholdRing.Writes.Length == 5
            && thresholdRing.Writes.All(write => write.Committed)
            && thresholdRing.ChildBoundaryReached
            && !thresholdRing.ChildBoundaryInstructionEntered
            && thresholdRing.StopAtRawFileOffsetExclusive == CompletionRawFileOffset
            && !thresholdRing.DecAtBoundaryEntered;
        bool exactBoundedCompletion = exactThresholdBypass || exactRingCompletion;
        bool exactCleanCompletion =
            IsExactOuterEquality(parent)
            && exactBoundedCompletion
            && (exactThresholdBypass
                || exactRingCompletion && thresholdRing.ExactCleanCompletion);

        return Finish(
            capture,
            parent,
            thresholdRing,
            ThresholdOutcome(thresholdRing.Outcome),
            derivedEDX,
            inheritedESI,
            originalEDI,
            entryEDXDerived,
            entryESIDerived,
            entryEDIDerived,
            freshGlobalUnused: false,
            freshEDXPlus8Unused:
                thresholdRing.ParentPlan.UnreachedFreshEDXPlus8CaptureProvided,
            effects,
            thresholdRing.StopAtRawFileOffsetExclusive,
            exactBoundedCompletion,
            exactCleanCompletion);
    }

    public static bool IsExactOuterEquality(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildEqualToBounded9403
        && parent.ExactBoundedCompletion
        && parent.ExactCleanCompletion
        && parent.StopAtRawFileOffsetExclusive == OuterEqualityRawFileOffset
        && parent.ChildPlan is
        {
            ExactParentHandoffCompleted: true,
            ChildBoundedTargetNotEntered: true,
            Outcome:
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildEqualToBounded9403,
            ChildPlan:
            {
                ExactCleanBoundedDestination: true,
                JzSelectedDestinationRawFileOffset: OuterEqualityRawFileOffset,
                StopAtRawFileOffsetExclusive: OuterEqualityRawFileOffset
            }
        }
        && !parent.FreshIndexedDWORDCaptureProvidedButUnused
        && !parent.FreshEDXPlus4CaptureProvidedButUnused;

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

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
        ParentOutcome(CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent) =>
        parent.Outcome switch
        {
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .ParentZeroToBounded93AC,
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentNotCleanAt93C0 =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .ParentNotCleanAt93C0,
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildUnequalToFallThrough93E4 =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .ParentEqualityNotSelected,
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildEqualToBounded9403 =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .ParentEqualityHandoffNotExact,
            _ => CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                .ParentStopped
        };

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
        ThresholdOutcome(CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingFreshThresholdGlobalValue,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingParentEntryEDX =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingDerivedEntryEDX,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshEDXPlus8 =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingFreshThresholdEDXPlus8,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .FirstUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .SecondUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .MissingLateEntryESIForFirstWrite =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingInheritedEntryESIForFirstWrite,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec =>
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec,
            _ => CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                .ThresholdHandoffNotExact
        };

    private static void AddParentEffects(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>.Builder
            effects)
    {
        foreach (CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect effect
            in parent.OrderedEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
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

    private static void AddThresholdRingEffects(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan thresholdRing,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>.Builder
            effects)
    {
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect effect
            in thresholdRing.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind kind =
                effect.Kind
                    == CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                        .FreshRead
                    ? effect.RawFileOffset
                        == CapacityFinalCalleeCommonDualPointerThresholdProjection
                            .FreshGlobalReadRawFileOffset
                        ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead
                        : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read
                    : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
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

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? flags) =>
        flags is { } value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags?
        ConvertThresholdFlags(CapacityFinalCalleeCommonDualPointerThresholdFlags? flags) =>
        flags is { } value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan parent,
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome outcome,
        uint? derivedEDX,
        uint? inheritedESI,
        uint? originalEDI,
        bool entryEDXDerived,
        bool entryESIDerived,
        bool entryEDIDerived,
        bool freshGlobalUnused,
        bool freshEDXPlus8Unused,
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect>.Builder
            effects,
        uint stopAt,
        bool exactBoundedCompletion,
        bool exactCleanCompletion)
    {
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags? parentFlags =
            ConvertFlags(parent.FlagsAtStop);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags? thresholdFlags =
            ConvertThresholdFlags(thresholdRing?.FlagsAtStop);
        return new(capture, outcome)
        {
            ParentPlan = parent,
            ThresholdRingPlan = thresholdRing,
            DerivedEntryEDX = derivedEDX,
            InheritedEntryESI = inheritedESI,
            OriginalEntryEDI = originalEDI,
            EntryEDXDerivedFromFreshIndexedRead = entryEDXDerived,
            EntryESIInheritedFromOriginalParentCapture = entryESIDerived,
            EntryEDIDerivedFromOriginalIndexedSizeGate = entryEDIDerived,
            ParentEqualitySelected = IsExactOuterEquality(parent),
            FreshThresholdGlobalValueCaptureProvidedButUnused = freshGlobalUnused,
            FreshThresholdEDXPlus8CaptureProvidedButUnused = freshEDXPlus8Unused,
            ParentFlagsAtThresholdEntry = parentFlags,
            FlagsAtStop = thresholdFlags ?? parentFlags,
            OrderedMemoryEffects = effects.ToImmutable(),
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }
}
