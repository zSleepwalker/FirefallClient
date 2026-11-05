using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture(
    CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture Parent,
    uint? FreshIndexedDWORD,
    uint? FreshEDXPlus4);

public enum CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
{
    ParentStopped,
    ParentZeroToBounded93AC,
    ParentNotCleanAt93C0,
    ChildStoppedAt93C0,
    ChildStoppedAt93C2,
    ChildStoppedAt93D6,
    ChildStoppedAt93D8,
    ChildEqualToBounded9403,
    ChildUnequalToFallThrough93E4,
    InvalidParentHandoff
}

public enum CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
{
    ParentWrite,
    FreshGlobalRead,
    FreshIndexedDWORDRead,
    FreshTagDWORDRead
}

public readonly record struct CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect(
    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed,
    bool ForwardedFromPriorWrite);

public readonly record struct CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan(
    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan ParentPlan { get; init; } = null!;
    public CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan? ChildPlan { get; init; }
    public bool ParentNonzeroAdmission { get; init; }
    public bool ParentBoundedDestinationSelected { get; init; }
    public bool ChildInternalEntry { get; init; }
    public uint? ChildInternalEntryRawFileOffset =>
        ChildInternalEntry
            ? CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset
            : null;
    public uint? DerivedIndexECX { get; init; }
    public uint? DerivedNodeEBX { get; init; }
    public uint? DerivedParentEntryEDI { get; init; }
    public bool FreshIndexedDWORDCaptureProvidedButUnused { get; init; }
    public bool FreshEDXPlus4CaptureProvidedButUnused { get; init; }
    public bool ChildBoundedDestinationSelected { get; init; }
    public bool ChildBoundedDestinationNotEntered { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool ZeroParentDestinationNotEntered => true;
    public bool ChildDestinationNotEntered => ChildPlan?.DestinationEntered == false;
    public bool Duplicate93D8Read => OrderedEffects.Count(effect =>
        effect.Kind == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
            .FreshTagDWORDRead) > 1;
    public ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>
        OrderedEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>.Empty;
    public CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? ParentFlagsAt93C0 { get; init; }
    public CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? FlagsAtStop { get; init; }
    public bool ParentFlagsPreservedWhenIndexedReadOrCompareMissing { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean nonzero node-mask edge at 93C0 to its independent indexed and tag reads.
/// </summary>
public static class CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.EntryRawFileOffset;
    public const uint ParentZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.ZeroDestinationRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset;
    public const uint ChildEqualDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EqualDestinationRawFileOffset;
    public const uint ChildUnequalDestinationRawFileOffset =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.FallThroughDestinationRawFileOffset;

    public static CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(capture.Parent);
        ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>.Builder effects =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>();
        AddParentEffects(parent, effects);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? parentFlags =
            ConvertFlags(parent.FlagsAtStop);
        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.ZeroTestResultToBounded93AC)
        {
            bool exactBoundedCompletion =
                parent.BoundedBranchDestinationSelected
                && parent.ZeroDestinationInstructionEntered == false
                && parent.StopAtRawFileOffsetExclusive == ParentZeroDestinationRawFileOffset;
            bool exactCleanCompletion =
                exactBoundedCompletion
                && parent.ExactCleanExitAdmitted
                && parent.ExactCleanHandoff
                && !capture.FreshIndexedDWORD.HasValue
                && !capture.FreshEDXPlus4.HasValue
                && !parent.LateFreshGlobalValueProvidedButUnused
                && !parent.UnreachedLateFreshGlobalValueCaptureProvided;
            return Finish(
                capture,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC,
                parent,
                effects,
                parentFlags,
                stopAt: ParentZeroDestinationRawFileOffset,
                freshIndexedUnused: capture.FreshIndexedDWORD.HasValue,
                freshTagUnused: capture.FreshEDXPlus4.HasValue,
                parentBoundedSelected: parent.BoundedBranchDestinationSelected,
                exactBoundedCompletion: exactBoundedCompletion,
                exactCleanCompletion: exactCleanCompletion);
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .NonzeroTestResultToBounded93C0
            || !IsExactNonzeroAdmission(parent))
        {
            return Finish(
                capture,
                parent.Outcome
                        == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                            .NonzeroTestResultToBounded93C0
                    ? CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                        .ParentNotCleanAt93C0
                    : CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentStopped,
                parent,
                effects,
                parentFlags,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                freshIndexedUnused: capture.FreshIndexedDWORD.HasValue,
                freshTagUnused: capture.FreshEDXPlus4.HasValue,
                parentBoundedSelected: parent.BoundedBranchDestinationSelected);
        }

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan? initializedNode =
            parent.ParentLifecycle;
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? node =
            initializedNode?.NodeLifecycle;
        uint? derivedIndexECX = parent.ChildECXDerivedFromParentIndex
            ? parent.ChildECX
            : null;
        uint? derivedNodeEBX = node?.DerivedEBX;
        uint? derivedEntryEDI = parent.ParentLifecycle?.Capture.EntryEDI;
        if (initializedNode is null
            || node is null
            || derivedIndexECX is null
            || derivedNodeEBX is null
            || derivedEntryEDI is null
            || node.Capture.EntryECX != derivedIndexECX
            || node.LEAIndex != derivedIndexECX
            || node.DerivedEBX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .DeriveEBX(derivedIndexECX.Value))
        {
            return Finish(
                capture,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.InvalidParentHandoff,
                parent,
                effects,
                parentFlags,
                stopAt: ChildEntryRawFileOffset,
                freshIndexedUnused: capture.FreshIndexedDWORD.HasValue,
                freshTagUnused: capture.FreshEDXPlus4.HasValue,
                parentAdmission: true,
                parentBoundedSelected: true,
                derivedIndexECX: derivedIndexECX,
                derivedNodeEBX: derivedNodeEBX,
                derivedEntryEDI: derivedEntryEDI);
        }

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan child =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(
                        EntryEBX: derivedNodeEBX,
                        FreshIndexedDWORD: capture.FreshIndexedDWORD,
                        LateEntryECX: derivedIndexECX),
                    LateEntryEDI: derivedEntryEDI,
                    FreshEDXPlus4: capture.FreshEDXPlus4));
        AddChildEffects(child, effects);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome outcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ParentBlocked =>
                    ChildClassificationOutcome(child),
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D6 =>
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93D6,
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D8 =>
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93D8,
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildEqualToBounded9403 =>
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildEqualToBounded9403,
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildUnequalToFallThrough93E4 =>
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildUnequalToFallThrough93E4,
                _ => ChildClassificationOutcome(child)
            };

        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? flagsAtStop =
            FlagsAfterChildOrParent(parentFlags, child);
        bool indexedOrCompareMissing =
            child.ParentPlan.Outcome is
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingEntryEBX
                or CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
                or CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2;
        bool cleanCompletion =
            child.ExactParentHandoffCompleted
            && child.ChildBoundedTargetNotEntered
            && child.Outcome is
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
                    .ChildEqualToBounded9403
                or CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
                    .ChildUnequalToFallThrough93E4
            && !child.LateEntryEDICaptureProvidedButUnused
            && !child.FreshEDXPlus4CaptureProvidedButUnused
            && !child.ParentPlan.UnreachedFreshIndexedDWORDObservationProvided
            && !child.ParentPlan.UnreachedLateEntryECXCaptureProvided
            && !parent.LateFreshGlobalValueProvidedButUnused
            && parent.ExactCleanExitAdmitted
            && !parent.UnreachedLateFreshGlobalValueCaptureProvided;

        return new(capture, outcome)
        {
            ParentPlan = parent,
            ChildPlan = child,
            ParentNonzeroAdmission = true,
            ParentBoundedDestinationSelected = true,
            ChildInternalEntry = child.OrderedReads.Any(read =>
                read.Stage
                    == CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ParentIndexedDWORDAt93C0
                && read.ReadReached),
            DerivedIndexECX = derivedIndexECX,
            DerivedNodeEBX = derivedNodeEBX,
            DerivedParentEntryEDI = derivedEntryEDI,
            FreshIndexedDWORDCaptureProvidedButUnused =
                child.ParentPlan.UnreachedFreshIndexedDWORDObservationProvided,
            FreshEDXPlus4CaptureProvidedButUnused =
                child.FreshEDXPlus4CaptureProvidedButUnused,
            ChildBoundedDestinationSelected = child.ChildBoundedTargetNotEntered,
            ChildBoundedDestinationNotEntered = child.ChildBoundedTargetNotEntered,
            ExactBoundedCompletion = child.ChildBoundedTargetNotEntered,
            ExactCleanCompletion = cleanCompletion,
            OrderedEffects = effects.ToImmutable(),
            ParentFlagsAt93C0 = parentFlags,
            FlagsAtStop = flagsAtStop,
            ParentFlagsPreservedWhenIndexedReadOrCompareMissing =
                indexedOrCompareMissing && flagsAtStop == parentFlags,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome outcome,
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent,
        ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>.Builder effects,
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? flags,
        uint stopAt,
        bool freshIndexedUnused,
        bool freshTagUnused,
        bool parentAdmission = false,
        bool parentBoundedSelected = false,
        bool exactBoundedCompletion = false,
        bool exactCleanCompletion = false,
        uint? derivedIndexECX = null,
        uint? derivedNodeEBX = null,
        uint? derivedEntryEDI = null) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            ParentNonzeroAdmission = parentAdmission,
            ParentBoundedDestinationSelected = parentBoundedSelected,
            FreshIndexedDWORDCaptureProvidedButUnused = freshIndexedUnused,
            FreshEDXPlus4CaptureProvidedButUnused = freshTagUnused,
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            OrderedEffects = effects.ToImmutable(),
            ParentFlagsAt93C0 = HasParentNonzeroBranch(parent) ? flags : null,
            FlagsAtStop = flags,
            DerivedIndexECX = derivedIndexECX,
            DerivedNodeEBX = derivedNodeEBX,
            DerivedParentEntryEDI = derivedEntryEDI,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static bool IsExactNonzeroAdmission(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonFreshMaskGatePlan? gate = parent.FreshMaskGate;
        return parent.ExactCleanHandoff
            && parent.ExactCleanExitAdmitted
            && parent.NodeFreshMaskHandoffValidated
            && parent.ParentNodeInitializationCompleted
            && parent.ParentExactCleanAdmission
            && parent.FreshGlobalReadAfterAllNodeWrites
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .NonzeroTestResultToBounded93C0
            && parent.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
                    .NonzeroDestinationRawFileOffset
            && parent.BoundedBranchDestinationSelected
            && parent.NonzeroDestinationInstructionEntered == false
            && gate is
            {
                FreshGlobalReadReached: true,
                FreshGlobalValueCaptured: true,
                ShiftInstructionReached: true,
                TestReached: true,
                TestResult: not 0,
                JnzReached: true,
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset:
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
                        .NonzeroDestinationRawFileOffset,
                ExactCleanBoundedDestination: true,
                ExactCleanExitAdmitted: true,
                EntryEDXOperandConsumed: true,
                EntryECXOperandConsumed: true,
                EntryEDXCaptureProvidedButUnused: false,
                EntryECXCaptureProvidedButUnused: false
            }
            && parent.ChildEDXDerivedFromNodeMove
            && parent.ChildECXDerivedFromParentIndex
            && parent.ChildEDX is not null
            && parent.ChildECX is uint childECX
            && gate.Capture.EntryEDX == parent.ChildEDX
            && gate.Capture.EntryECX == childECX
            && gate.ShiftInputEDX == parent.ChildEDX
            && gate.ShiftCountMasked == (byte)(childECX & 31)
            && gate.ShiftResult == unchecked(1u << (int)(childECX & 31))
            && gate.TestResult
                == (gate.Capture.FreshGlobalValue
                    & unchecked(1u << (int)(childECX & 31)))
            && parent.ParentLifecycle?.Capture.EntryEDI
                == parent.Capture.EntryEDI;
    }

    private static void AddParentEffects(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent,
        ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>.Builder effects)
    {
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan? nodeParent =
            parent.ParentLifecycle;
        if (nodeParent is
            {
                FrameStoreAttempted: true,
                FrameStoreCommitted: false
            })
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreRawFileOffset,
                nodeParent.FrameStoreAddress,
                nodeParent.FrameStoreValue,
                ValueCaptured: nodeParent.FrameStoreValue.HasValue,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }

        foreach (CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write
                    ? CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite
                    : CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshGlobalRead,
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                effect.ValueCaptured,
                Attempted: true,
                Committed: effect.Kind
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write,
                ForwardedFromPriorWrite: effect.ForwardedFromPriorWrite));
        }

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? node =
            nodeParent?.NodeLifecycle;
        if (node is { FirstStoreAttempted: true, FirstStoreCommitted: false })
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .FirstStoreRawFileOffset,
                node.FirstStoreAddress,
                node.FirstStoreValue,
                ValueCaptured: node.FirstStoreValue.HasValue,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }
    }

    private static void AddChildEffects(
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan child,
        ImmutableArray<CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffect>.Builder effects)
    {
        foreach (CapacityFinalCalleeCommonIndexedCountTagLifecycleRead read in child.OrderedReads)
        {
            effects.Add(new(
                read.Stage
                    == CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ParentIndexedDWORDAt93C0
                    ? CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead
                    : CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshTagDWORDRead,
                read.RawFileOffset,
                read.AbsoluteAddress,
                read.Value,
                read.ValueCaptured,
                Attempted: read.ReadReached,
                Committed: false,
                ForwardedFromPriorWrite: false));
        }
    }

    private static bool HasParentNonzeroBranch(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent) =>
        parent.FreshMaskGate is
        {
            JnzReached: true,
            JnzTaken: true,
            JnzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
                    .NonzeroDestinationRawFileOffset
        };

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
        ChildClassificationOutcome(
            CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan child) =>
        child.ParentPlan.Outcome switch
        {
            CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX
            or CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                .MissingFreshIndexedDWORDAt93C0 =>
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93C0,
            CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingLateEntryECXAt93C2 =>
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93C2,
            _ => CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.InvalidParentHandoff
        };

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags?
        FlagsAfterChildOrParent(
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? parentFlags,
            CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan child)
    {
        if (child.ParentPlan.Outcome is
                CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingEntryEBX
                or CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
                or CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2)
        {
            return parentFlags;
        }

        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? childFlags = child.FlagsAtStop;
        return childFlags is { } flags
            ? ConvertFlags(flags)
            : parentFlags;
    }

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
