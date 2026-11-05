using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTraversalSlotStoresLifecycleCapture(
    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleCapture TraversalCapture,
    uint? LateEntryESI);

public enum CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
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
    MissingLateEntryESIForFirstStore,
    MissingLateEntryEDXForSecondStore,
    CompletedToCleanupJump,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
{
    ParentGate,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail
}

public readonly record struct CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction(
    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase Phase,
    uint RawFileOffset);

public readonly record struct CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan(
    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleCapture Capture,
    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan ParentLifecycle { get; init; } =
        null!;
    public CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? ThresholdPlan =>
        ParentLifecycle.ThresholdPlan;
    public CapacityFinalCalleeCommonZeroSlotStoreTailPlan? StoreTail { get; init; }
    public CapacityFinalCalleeCommonTagTraversalLifecyclePlan TraversalPlan =>
        ParentLifecycle.TraversalPlan;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        ParentLifecycle.ChildPlan;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead> OrderedReads =>
        ParentLifecycle.OrderedReads;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction>
        ParentOrderedInstructions => ParentLifecycle.OrderedInstructions;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonTagTraversalLifecycleOutcome TraversalOutcome =>
        ParentLifecycle.TraversalOutcome;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome? ThresholdOutcome =>
        ThresholdPlan?.Outcome;
    public uint? ChildFieldAddressAt93EE => ParentLifecycle.ChildFieldAddressAt93EE;
    public uint? ThresholdEntryECX => ParentLifecycle.ThresholdEntryECX;
    public bool ThresholdEntryECXIsDerivedFromChildFieldAddress =>
        ParentLifecycle.ThresholdEntryECXIsDerivedFromChildFieldAddress;
    public bool ParentUnequalHandoffExact => ParentLifecycle.ParentUnequalHandoffExact;
    public bool ParentZeroChildOutcome => ParentLifecycle.ParentZeroChildOutcome;
    public bool ChildZero945CSelected => ParentLifecycle.ChildZero945CSelected;
    public bool ChildZero945CEntered => ParentLifecycle.ChildZero945CEntered;
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
    public bool Internal945CEntered => ParentLifecycle.Internal945CEntered;
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
    public bool LateEntryESIUnused =>
        StoreTail is null && Capture.LateEntryESI.HasValue;
    public bool StoreTailEntryReached => StoreTail is not null;
    public bool StoreTailEntryInstructionEntered => FirstStoreAttempted;
    public bool Internal9468Entered => StoreTailEntryInstructionEntered;
    public uint? StoreTailEntryECX => StoreTail?.Capture.EntryECX;
    public uint? StoreTailEntryESI => StoreTail?.Capture.EntryESI;
    public uint? StoreTailLateEntryEDX => StoreTail?.Capture.LateEntryEDX;
    public bool StoreTailEntryECXIsDerivedFromThresholdLeft =>
        StoreTail is { } tail
        && ThresholdPlan?.CompareLeft is uint thresholdLeft
        && tail.Capture.EntryECX == thresholdLeft
        && thresholdLeft == ChildFieldAddressAt93EE;
    public bool StoreTailLateEntryEDXIsOriginalTraversalParentPointer =>
        StoreTail is { } tail
        && Capture.TraversalCapture.EntryEDX.HasValue
        && tail.Capture.LateEntryEDX == Capture.TraversalCapture.EntryEDX;
    public bool StoreTailLateEntryEDXDerivedFromChild => false;
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
    public CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags? FlagsAtStop { get; init; }
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Adds the 9468 zero-slot stores to the released tagged traversal/threshold
/// lifecycle, passing the derived child field address and original parent pointer.
/// </summary>
public static class CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection.EntryRawFileOffset;
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

    public static CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan Project(
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan parent =
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection.Project(
                capture.TraversalCapture);
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold = parent.ThresholdPlan;
        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = null;
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome outcome =
            MapParentOutcome(parent.Outcome);
        bool exactBelowRoute = false;
        bool exactStoreRoute = false;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        var orderedInstructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            orderedInstructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                        .ParentGate =>
                        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                            .ParentGate,
                    CapacityFinalCalleeCommonTraversalZeroSlotLifecycleInstructionPhase
                        .ChildIteration =>
                        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                            .ChildIteration,
                    _ =>
                        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
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
                    LateEntryEDX: capture.TraversalCapture.EntryEDX,
                    IncomingFlags: ToStoreTailFlags(thresholdPlan.CompareFlags)));
                outcome = MapStoreTailOutcome(storeTail.Outcome);
                stopAt = storeTail.StopAtRawFileOffsetExclusive;
                foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write
                    in storeTail.Writes)
                {
                    orderedInstructions.Add(new(
                        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                            .StoreTail,
                        write.RawFileOffset));
                }

                if (storeTail.JumpReached)
                {
                    orderedInstructions.Add(new(
                        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
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
            && parent.TraversalPlan.ExactCleanCompletion
            && !capture.TraversalCapture.FreshGlobalValue.HasValue
            && !capture.LateEntryESI.HasValue;
        bool exactBoundedCompletion = exactBelowRoute
            || exactStoreCompletion
            || exactOtherTraversal;
        bool exactCleanCompletion = exactStoreCompletion
            || (exactBelowRoute && !capture.LateEntryESI.HasValue)
            || exactOtherTraversal;

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
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan parent,
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan threshold) =>
        parent.ExactParentZeroHandoff
        && threshold.ExactCleanCompletion
        && threshold.CompareInstructionReached
        && threshold.CompareReached
        && threshold.BranchReached
        && threshold.BranchTaken == true
        && threshold.BranchFlags == threshold.CompareFlags
        && threshold.BranchSelectedDestinationRawFileOffset == BelowBranchTargetRawFileOffset
        && threshold.StopAtRawFileOffsetExclusive == BelowBranchTargetRawFileOffset
        && !threshold.TargetEntered;

    private static bool IsExactStoreTailHandoff(
        CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan parent,
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan threshold) =>
        parent.ExactParentZeroHandoff
        && parent.ExactCleanCompletion
        && parent.ParentZeroChildOutcome
        && parent.ChildZero945CSelected
        && parent.Internal945CEntered
        && !parent.ChildZero945CBoundaryTargetEntered
        && parent.Capture.EntryEDX.HasValue
        && parent.TraversalPlan.ParentPlan.EntryEDXOperandConsumed
        && parent.TraversalPlan.ChildEntryOperandsAreParentDerived
        && parent.ChildPlan?.Capture.LateEntryEDX == parent.Capture.EntryEDX
        && parent.ChildPlan is
        {
            FreshChildDWORD: 0,
            LateEntryEDXCaptured: true,
            MoveChildToEDXReached: false,
            FreshChildPlus4ReadReached: false,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeCommonTraversalZeroSlotLifecycleProjection
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
        && threshold.CompareFlags.HasValue
        && !threshold.TargetEntered
        && threshold.BranchReached
        && threshold.BranchTaken == false
        && threshold.BranchFlags == threshold.CompareFlags
        && threshold.BranchSelectedDestinationRawFileOffset == StoreTailEntryRawFileOffset
        && threshold.StopAtRawFileOffsetExclusive == StoreTailEntryRawFileOffset;

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome.ParentGateStopped =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome.ParentGateStopped,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ParentEqualBypassesChildTo9403 =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ParentEqualBypassesChildTo9403,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroHandoffNotExactAt945C =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroMissingDerivedECXAt945C =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroMissingFreshGlobalValueAt945C =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroUnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            _ => CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildMaskedDiffersFromEntryEDITo93E4
        };

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
        MapThresholdOutcome(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingBothCompareOperands
                or CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.UnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            _ => CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough
        };

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
        MapStoreTailOutcome(CapacityFinalCalleeCommonZeroSlotStoreTailOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore,
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingLateEntryEDXForSecondStore =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore,
            CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget =>
                CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump,
            _ => throw new InvalidOperationException(
                "The exact zero-child threshold handoff must derive its child ECX address.")
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

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags?
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

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags?
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

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags?
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
