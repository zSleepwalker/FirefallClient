using System;
using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture(
    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleCapture TraversalCapture,
    uint? LateEntryESI);

public enum CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
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
    MissingLateEntryESIForFirstStore,
    MissingLateEntryEDXForSecondStore,
    CompletedToCleanupJump,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
{
    IndexedCount,
    ParentTagGate,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail
}

public readonly record struct
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction(
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase Phase,
        uint RawFileOffset);

public readonly record struct
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? AuxiliaryCarry,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public sealed record CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan(
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan ParentLifecycle
        { get; init; } = null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan =>
        ParentLifecycle.ThresholdPlan;
    public CapacityFinalCalleeCommonZeroSlotStoreTailPlan? StoreTail { get; init; }
    public CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan TraversalPlan =>
        ParentLifecycle.TraversalPlan;
    public CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan IndexedTraversalPlan =>
        ParentLifecycle;
    public CapacityFinalCalleeCommonIndexedCountClassificationPlan CountPlan =>
        ParentLifecycle.CountPlan;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        ParentLifecycle.ChildPlan;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead>
        OrderedReads => ParentLifecycle.OrderedReads;
    public ImmutableArray<
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonIndexedTagTraversalLifecycleOutcome TraversalOutcome =>
        ParentLifecycle.TraversalOutcome;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome? ThresholdOutcome =>
        ThresholdPlan?.Outcome;
    public uint? ChildFieldAddressAt93EE => ParentLifecycle.ChildFieldAddressAt93EE;
    public uint? ThresholdEntryECX => ParentLifecycle.ThresholdEntryECX;
    public bool ThresholdEntryECXIsDerivedFromChildFieldAddress =>
        ParentLifecycle.ThresholdEntryECXIsDerivedFromChildFieldAddress;
    public uint? ParentDerivedTagEDX => TraversalPlan.ParentDerivedTagEDX;
    public bool ParentUnequalHandoffExact => ParentLifecycle.ParentUnequalHandoffExact;
    public bool ParentZeroChildOutcome => ParentLifecycle.ParentZeroChildOutcome;
    public bool ChildZero945CSelected => ParentLifecycle.ChildZero945CSelected;
    public bool ChildZero945CEntered => ParentLifecycle.ChildZero945CEntered;
    public bool Internal945CEntered => ParentLifecycle.Internal945CEntered;
    public bool ChildZeroBranchReached => ParentLifecycle.ChildZeroBranchReached;
    public bool ChildZeroBranchTaken => ParentLifecycle.ChildZeroBranchTaken;
    public uint? ChildZeroBranchDestinationRawFileOffset =>
        ParentLifecycle.ChildZeroBranchDestinationRawFileOffset;
    public bool ChildZeroStopAt945C => ParentLifecycle.ChildZeroStopAt945C;
    public bool ChildPlus4ReadReached => ParentLifecycle.ChildPlus4ReadReached;
    public bool LateFreshChildPlus4CaptureProvidedButUnused =>
        ParentLifecycle.LateFreshChildPlus4CaptureProvidedButUnused;
    public bool FreshGlobalValueCaptureProvidedButUnused =>
        ParentLifecycle.FreshGlobalValueCaptureProvidedButUnused;
    public bool ExactParentZeroHandoff => ParentLifecycle.ExactParentZeroHandoff;
    public bool ThresholdCompareAttempted => ParentLifecycle.ThresholdCompareAttempted;
    public bool ThresholdCompareReached => ParentLifecycle.ThresholdCompareReached;
    public bool? ThresholdBranchTaken => ParentLifecycle.ThresholdBranchTaken;
    public uint? ThresholdBranchSelectedDestinationRawFileOffset =>
        ParentLifecycle.ThresholdBranchSelectedDestinationRawFileOffset;
    public bool LateEntryESICaptureVisible => Capture.LateEntryESI.HasValue;
    public bool LateEntryESIUnusedBecauseThresholdBypassed =>
        ThresholdPlan is null && Capture.LateEntryESI.HasValue;
    public bool LateEntryESIUnusedBecauseExactStoreHandoffBlocked =>
        ThresholdPlan is not null && StoreTail is null && Capture.LateEntryESI.HasValue;
    public bool LateEntryESIUnused => StoreTail is null && Capture.LateEntryESI.HasValue;
    public bool StoreTailEntryReached => StoreTail is not null;
    public bool StoreTailEntryInstructionEntered => FirstStoreAttempted;
    public bool Internal9468Entered => FirstStoreAttempted;
    public uint? StoreTailEntryECX => StoreTail?.Capture.EntryECX;
    public uint? StoreTailEntryESI => StoreTail?.Capture.EntryESI;
    public uint? StoreTailLateEntryEDX => StoreTail?.Capture.LateEntryEDX;
    public bool StoreTailEntryECXIsDerivedFromThresholdLeft =>
        StoreTail is { } tail
        && ThresholdPlan?.CompareLeft is uint thresholdLeft
        && tail.Capture.EntryECX == thresholdLeft
        && thresholdLeft == ChildFieldAddressAt93EE;
    public bool StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX =>
        StoreTail is { } tail
        && ParentDerivedTagEDX is uint originalParentPointer
        && tail.Capture.LateEntryEDX == originalParentPointer;
    public bool StoreTailLateEntryEDXIsIndexedParentSlot =>
        StoreTail is { } tail
        && TraversalPlan.CountPlan.Capture.EntryEBX is uint indexedSlot
        && tail.Capture.LateEntryEDX == indexedSlot;
    public bool StoreTailLateEntryEDXIsZeroChildValue =>
        StoreTail is { } tail
        && ParentLifecycle.ChildPlan?.FreshChildDWORD is uint zeroChildValue
        && tail.Capture.LateEntryEDX == zeroChildValue;
    public CapacityFinalCalleeCommonZeroSlotStoreTailFlags? ChildFlags { get; init; }
    public bool ChildFlagsDerivedFromThresholdCompare =>
        StoreTailEntryReached
        && ThresholdPlan?.CompareFlags.HasValue == true
        && ChildFlags.HasValue;
    public bool ChildFlagsUserProvided => false;
    public bool ThresholdCompareFlagsPreservedThroughStoreTail =>
        StoreTail is { FlagsPreserved: true }
        && ChildFlagsDerivedFromThresholdCompare
        && StoreTail.FlagsAtStop == ChildFlags;
    public bool FirstStoreAttempted => StoreTail?.FirstStoreAttempted == true;
    public bool FirstStoreCommitted => StoreTail?.FirstStoreCommitted == true;
    public bool SecondStoreAttempted => StoreTail?.SecondStoreAttempted == true;
    public bool SecondStoreCommitted => StoreTail?.SecondStoreCommitted == true;
    public bool ThirdStoreAttempted => StoreTail?.ThirdStoreAttempted == true;
    public bool ThirdStoreCommitted => StoreTail?.ThirdStoreCommitted == true;
    public bool FourthStoreAttempted => StoreTail?.FourthStoreAttempted == true;
    public bool FourthStoreCommitted => StoreTail?.FourthStoreCommitted == true;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> Writes =>
        StoreTail?.Writes
        ?? ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite>.Empty;
    public bool CleanupJumpReached => StoreTail?.JumpReached == true;
    public uint? CleanupJumpTargetRawFileOffset => StoreTail?.JumpTargetRawFileOffset;
    public uint? CleanupJumpTargetVirtualAddress => StoreTail?.JumpTargetVirtualAddress;
    public bool CleanupJumpTargetEntered => StoreTail?.JumpTargetEntered == true;
    public bool CleanupJumpBoundaryReached => StoreTail?.BoundaryReached == true;
    public bool CleanupJumpBoundaryInstructionEntered =>
        StoreTail?.BoundaryInstructionEntered == true;
    public bool Outer8E76Entered => false;
    public bool Outer9468Entered => false;
    public bool DestinationEntered => false;
    public CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags? FlagsAtStop
        { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Adds the 9468 stores to the indexed zero-child lifecycle using the derived
/// child-slot address and the original parent-derived EDX pointer.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection.EntryRawFileOffset;
    public const uint ThresholdEntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.EntryRawFileOffset;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryRawFileOffset;
    public const uint BelowBranchTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint CleanupJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpTargetRawFileOffset;
    public const uint CleanupJumpBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonZeroSlotStoreTailProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan parent =
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection.Project(
                capture.TraversalCapture);
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = parent.ThresholdPlan;
        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = null;
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome outcome =
            MapParentOutcome(parent.Outcome);
        bool exactBelowRoute = false;
        bool exactStoreRoute = false;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        var orderedInstructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            orderedInstructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .IndexedCount =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .IndexedCount,
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ParentTagGate =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .ParentTagGate,
                    CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleInstructionPhase
                        .ChildIteration =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .ChildIteration,
                    _ =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .ZeroSlotThreshold
                },
                instruction.RawFileOffset));
        }

        if (threshold is { } thresholdPlan)
        {
            outcome = MapThresholdOutcome(thresholdPlan.Outcome);
            stopAt = thresholdPlan.StopAtRawFileOffsetExclusive;
            exactBelowRoute = IsExactBelowRoute(parent, thresholdPlan);
            exactStoreRoute = IsExactStoreTailHandoff(parent, thresholdPlan);

            if (exactStoreRoute)
            {
                storeTail = CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                    EntryECX: thresholdPlan.CompareLeft,
                    EntryESI: capture.LateEntryESI,
                    LateEntryEDX: parent.TraversalPlan.ParentDerivedTagEDX,
                    IncomingFlags: ToStoreTailFlags(thresholdPlan.CompareFlags)));
                outcome = MapStoreTailOutcome(storeTail.Outcome);
                stopAt = storeTail.StopAtRawFileOffsetExclusive;
                foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write in storeTail.Writes)
                {
                    orderedInstructions.Add(new(
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .StoreTail,
                        write.RawFileOffset));
                }

                if (storeTail.JumpReached)
                {
                    orderedInstructions.Add(new(
                        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                            .StoreTail,
                        CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                            .JumpRawFileOffset));
                }
            }
        }

        bool exactStoreCompletion = exactStoreRoute
            && storeTail is
            {
                Outcome:
                    CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget,
                ExactBoundedExitAdmitted: true,
                JumpReached: true,
                JumpTargetRawFileOffset: CleanupJumpTargetRawFileOffset,
                StopAtRawFileOffsetExclusive: CleanupJumpTargetRawFileOffset,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                JumpTargetEntered: false,
                UnreachedLateEntryEDXCaptureProvided: false,
                Writes.Length: 4
            }
            && storeTail.Writes.All(write => write.Committed);
        bool exactOtherTraversal = threshold is null
            && !parent.ParentZeroChildOutcome
            && parent.TraversalPlan.ExactCleanCompletion;
        bool exactBoundedCompletion = exactBelowRoute
            || exactStoreCompletion
            || exactOtherTraversal;
        bool exactCleanCompletion = exactStoreCompletion
            || (exactBelowRoute && !capture.LateEntryESI.HasValue)
            || (exactOtherTraversal
                && !capture.TraversalCapture.FreshGlobalValue.HasValue
                && !capture.LateEntryESI.HasValue);

        return new(capture, outcome)
        {
            ParentLifecycle = parent,
            StoreTail = storeTail,
            OrderedInstructions = orderedInstructions.ToImmutable(),
            ChildFlags = storeTail?.Capture.IncomingFlags,
            FlagsAtStop = storeTail is not null
                ? ToLifecycleFlags(storeTail.FlagsAtStop)
                : threshold?.CompareFlags is { } compareFlags
                    ? ToLifecycleFlags(compareFlags)
                    : ToLifecycleFlags(parent.FlagsAtStop),
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactBelowRoute(
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan parent,
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan threshold) =>
        parent.ExactParentZeroHandoff
        && parent.Internal945CEntered
        && threshold.ExactCleanCompletion
        && threshold.CompareInstructionReached
        && threshold.CompareReached
        && threshold.BranchReached
        && threshold.BranchTaken == true
        && threshold.CompareFlags is { Carry: true }
        && threshold.BranchFlags == threshold.CompareFlags
        && threshold.BranchSelectedDestinationRawFileOffset == BelowBranchTargetRawFileOffset
        && threshold.StopAtRawFileOffsetExclusive == BelowBranchTargetRawFileOffset
        && !threshold.TargetEntered;

    private static bool IsExactStoreTailHandoff(
        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan parent,
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan threshold) =>
        parent.ExactParentZeroHandoff
        && parent.ExactCleanCompletion
        && parent.ParentZeroChildOutcome
        && parent.ChildZero945CSelected
        && parent.Internal945CEntered
        && !parent.ChildZero945CBoundaryTargetEntered
        && parent.TraversalPlan.ParentDerivedTagEDX.HasValue
        && parent.TraversalPlan.TraversalOperandsAreParentDerived
        && parent.ChildPlan is
        {
            FreshChildDWORD: 0,
            LateEntryEDXCaptured: true,
            MoveChildToEDXReached: false,
            FreshChildPlus4ReadReached: false,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleProjection
                    .ZeroChildBoundaryRawFileOffsetExclusive
        }
        && parent.ThresholdEntryECXIsDerivedFromChildFieldAddress
        && parent.ChildFieldAddressAt93EE.HasValue
        && parent.ThresholdEntryECX == parent.ChildFieldAddressAt93EE
        && threshold.ExactCleanCompletion
        && threshold.CompareInstructionReached
        && threshold.GlobalReadReached
        && threshold.FreshGlobalValueCaptured
        && threshold.EntryECXCaptured
        && threshold.CompareReached
        && threshold.CompareLeft == parent.ChildFieldAddressAt93EE
        && threshold.CompareRight.HasValue
        && threshold.CompareFlags is { Carry: false }
        && !threshold.TargetEntered
        && threshold.BranchReached
        && threshold.BranchTaken == false
        && threshold.BranchFlags == threshold.CompareFlags
        && threshold.BranchSelectedDestinationRawFileOffset == StoreTailEntryRawFileOffset
        && threshold.StopAtRawFileOffsetExclusive == StoreTailEntryRawFileOffset;

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome.ParentBlocked =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome.ParentBlocked,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome.ParentTagGateStopped =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentTagGateStopped,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ParentEqualBypassesTraversalTo9403 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentEqualBypassesTraversalTo9403,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildZeroHandoffNotExactAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildZeroMissingDerivedECXAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildZeroMissingFreshGlobalValueAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildZeroUnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4
        };

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
        MapThresholdOutcome(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingBothCompareOperands
                or CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough
        };

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
        MapStoreTailOutcome(CapacityFinalCalleeCommonZeroSlotStoreTailOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore,
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingLateEntryEDXForSecondStore =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore,
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget =>
                CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump,
            _ => throw new InvalidOperationException(
                "The exact indexed zero-child threshold handoff must derive its child ECX address.")
        };

    private static CapacityFinalCalleeCommonZeroSlotStoreTailFlags? ToStoreTailFlags(
        CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotThresholdGateFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.AuxiliaryCarry,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags?
        ToLifecycleFlags(CapacityFinalCalleeCommonZeroSlotStoreTailFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotStoreTailFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.AuxiliaryCarry,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags?
        ToLifecycleFlags(CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotThresholdGateFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.AuxiliaryCarry,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags?
        ToLifecycleFlags(CapacityFinalCalleeCommonTraversalIterationFlags? flags) =>
        flags is CapacityFinalCalleeCommonTraversalIterationFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.Auxiliary,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;
}
