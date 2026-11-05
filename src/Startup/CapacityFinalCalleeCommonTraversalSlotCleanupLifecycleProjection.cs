using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleCapture(
    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleCapture StoreCapture,
    uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
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
    ParentStoreCompletionNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero,
    MissingLateFreshChildPlus4At93F9,
    ChildMaskedEqualsEntryEDIAt9403,
    ChildMaskedDiffersFromEntryEDITo93E4
}

public enum CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
{
    ParentRead,
    ParentStore,
    CleanupCounterRead,
    CleanupCounterWrite
}

public enum CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
{
    ParentGate,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool Reached,
        bool Executed);

public readonly record struct
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public sealed record CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan(
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome Outcome,
    CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan ParentLifecycle)
{
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public CapacityFinalCalleeCommonTraversalZeroSlotLifecyclePlan TraversalLifecycle =>
        ParentLifecycle.ParentLifecycle;
    public CapacityFinalCalleeCommonTagTraversalLifecyclePlan TraversalPlan =>
        ParentLifecycle.TraversalPlan;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        ParentLifecycle.ChildPlan;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead> OrderedReads =>
        ParentLifecycle.OrderedReads;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> StoreWrites =>
        ParentLifecycle.Writes;
    public ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess> CounterAccesses =>
        CleanupCounterPlan?.CounterAccesses
        ?? ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>.Empty;
    public CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags? ParentFlagsAtStop =>
        ParentLifecycle.FlagsAtStop;
    public CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags? FlagsAtStop { get; init; }
    public bool ExactParentStoreCompletion { get; init; }
    public bool ExactCleanupCounterCompletion { get; init; }
    public bool CleanupCounterGateEntered => ExactParentStoreCompletion;
    public bool Internal945CEntered => ParentLifecycle.Internal945CEntered;
    public bool Internal9468Entered => ParentLifecycle.Internal9468Entered;
    public bool FourParentStoresCommitted =>
        ParentLifecycle.Writes.Length == 4
        && ParentLifecycle.Writes.All(write => write.Attempted && write.Committed);
    public bool Internal942FEntered => CleanupCounterPlan?.CounterReadReached == true;
    public bool CounterReadAttempted => CleanupCounterPlan?.CounterReadReached == true;
    public bool CounterValueCaptured => CleanupCounterPlan?.FreshCounterDwordCaptured == true;
    public bool CounterWriteAttempted => CleanupCounterPlan?.CounterWriteReached == true;
    public bool CounterWriteCommitted => CounterWriteAttempted;
    public uint? CounterBefore => CleanupCounterPlan?.CounterBefore;
    public uint? CounterAfter => CleanupCounterPlan?.CounterAfter;
    public uint? CounterAddress =>
        CounterReadAttempted
            ? CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
            : null;
    public bool? CounterJnzTaken => CleanupCounterPlan?.JnzTaken;
    public uint? CounterJnzDestinationRawFileOffset =>
        CleanupCounterPlan?.SelectedDestinationRawFileOffset;
    public bool FreshCounterCaptureProvidedButUnused =>
        !CleanupCounterGateEntered && Capture.FreshCounterDword.HasValue;
    public bool CleanupCarryDerivedFromParent =>
        ExactParentStoreCompletion
        && ParentFlagsAtStop is { Carry: false }
        && CleanupCounterPlan?.Capture.IncomingCarryFlag == false;
    public CapacityFinalCalleeCleanupCounterGateFlags? CleanupFlagsAfterDecrement =>
        CleanupCounterPlan?.FlagsAfterDecrement;
    public bool CounterDecrementPreservedParentCarry =>
        CleanupCarryDerivedFromParent
        && CleanupCounterPlan?.CarryAtStop == ParentFlagsAtStop?.Carry;
    public bool ParentStoreJumpReached => ParentLifecycle.CleanupJumpReached;
    public uint? ParentStoreJumpTargetRawFileOffset =>
        ParentLifecycle.CleanupJumpTargetRawFileOffset;
    public bool ParentStoreJumpTargetEntered => false;
    public bool CounterNonzeroTargetEntered => false;
    public bool Outer9441Entered => false;
    public bool Outer943CCallEntered => false;
    public bool ZeroPathCallReached => false;
    public bool ZeroPathCalleeEntered => false;
    public bool ZeroPathImmediateWasMemoryRead => false;
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact four-store zero-slot handoff to the independent DEC/JNZ cleanup gate.
/// </summary>
public static class CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection.EntryRawFileOffset;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection
            .StoreTailEntryRawFileOffset;
    public const uint CleanupCounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterNonzeroTargetRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;
    public const uint CleanupCounterBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    public static CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent =
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleProjection.Project(
                capture.StoreCapture);
        bool exactParentStoreCompletion = IsExactParentStoreCompletion(parent, capture);
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter = exactParentStoreCompletion
            ? CapacityFinalCalleeCleanupCounterGateProjection.Project(new(
                EntryValidated: true,
                FreshCounterDword: capture.FreshCounterDword,
                IncomingCarryFlag: parent.FlagsAtStop?.Carry))
            : null;
        bool exactCleanupCounterCompletion = cleanupCounter is not null
            && IsExactCleanupCounterCompletion(parent, cleanupCounter);

        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent.Outcome)
                : MapCleanupOutcome(cleanupCounter.Outcome);
        ImmutableArray<CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>
            orderedEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags? flagsAtStop =
            cleanupCounter?.FlagsAfterDecrement is { } decrementFlags
                ? ToLifecycleFlags(decrementFlags)
                : parent.FlagsAtStop;
        bool exactBoundedCompletion = cleanupCounter is null
            ? parent.ExactBoundedCompletion
            : exactCleanupCounterCompletion;
        bool exactCleanCompletion = cleanupCounter is null
            ? parent.ExactCleanCompletion && !capture.FreshCounterDword.HasValue
            : exactCleanupCounterCompletion
                && parent.ExactCleanCompletion;
        uint stopAt = cleanupCounter?.StopAtRawFileOffsetExclusive
            ?? parent.StopAtRawFileOffsetExclusive;

        return new(capture, outcome, parent)
        {
            CleanupCounterPlan = cleanupCounter,
            OrderedInstructions = orderedInstructions,
            OrderedMemoryEffects = orderedEffects,
            FlagsAtStop = flagsAtStop,
            ExactParentStoreCompletion = exactParentStoreCompletion,
            ExactCleanupCounterCompletion = exactCleanupCounterCompletion,
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactParentStoreCompletion(
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent,
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleCapture capture)
    {
        uint? derivedChildECX = parent.ChildFieldAddressAt93EE;
        uint? originalTraversalEDX = capture.StoreCapture.TraversalCapture.EntryEDX;
        uint? entryESI = capture.StoreCapture.LateEntryESI;
        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = parent.StoreTail;
        if (storeTail is null)
        {
            return false;
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            || !parent.ExactBoundedCompletion
            || !parent.ExactCleanCompletion
            || parent.StopAtRawFileOffsetExclusive
                != CleanupCounterEntryRawFileOffset
            || parent.DestinationEntered
            || !parent.ExactParentZeroHandoff
            || !parent.Internal945CEntered
            || !parent.ThresholdCompareReached
            || parent.ThresholdBranchTaken != false
            || parent.ThresholdBranchSelectedDestinationRawFileOffset
                != StoreTailEntryRawFileOffset
            || !parent.Internal9468Entered
            || !parent.StoreTailEntryECXIsDerivedFromThresholdLeft
            || !parent.StoreTailLateEntryEDXIsOriginalTraversalParentPointer
            || parent.StoreTailEntryECX != derivedChildECX
            || parent.StoreTailLateEntryEDX != originalTraversalEDX
            || parent.StoreTailEntryESI != entryESI
            || !parent.ThresholdCompareFlagsPreservedThroughStoreTail
            || parent.FlagsAtStop is not { Carry: false }
            || !parent.CleanupJumpReached
            || parent.CleanupJumpTargetRawFileOffset != CleanupCounterEntryRawFileOffset
            || parent.CleanupJumpTargetEntered
            || !parent.CleanupJumpBoundaryReached
            || parent.CleanupJumpBoundaryInstructionEntered
            || derivedChildECX is not uint childECX
            || originalTraversalEDX is not uint traversalEDX
            || entryESI is not uint esi
            || storeTail.Outcome
                != CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget
            || storeTail.Writes.Length != 4
            || !storeTail.Writes.All(write => write.Attempted && write.Committed)
            || !storeTail.JumpReached
            || storeTail.JumpTargetRawFileOffset != CleanupCounterEntryRawFileOffset
            || storeTail.StopAtRawFileOffsetExclusive != CleanupCounterEntryRawFileOffset
            || storeTail.BoundaryReached != true
            || storeTail.BoundaryInstructionEntered
            || storeTail.JumpTargetEntered)
        {
            return false;
        }

        ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> writes =
            storeTail.Writes;
        return writes[0] is
            {
                RawFileOffset: 0x012C9468,
                Address: var firstAddress,
                AddressCaptured: true,
                Value: var firstValue,
                ValueCaptured: true
            }
            && firstAddress == childECX
            && firstValue == esi
            && writes[1] is
            {
                RawFileOffset: 0x012C946A,
                Address: var secondAddress,
                AddressCaptured: true,
                Value: var secondValue,
                ValueCaptured: true
            }
            && secondAddress
                == CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                    .DeriveEsiPlus18Address(esi)
            && secondValue == traversalEDX
            && writes[2] is
            {
                RawFileOffset: 0x012C946D,
                Address: var thirdAddress,
                AddressCaptured: true,
                Value: var thirdValue,
                ValueCaptured: true
            }
            && thirdAddress
                == CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                    .DeriveEsiPlusCAddress(esi)
            && thirdValue == esi
            && writes[3] is
            {
                RawFileOffset: 0x012C9470,
                Address: var fourthAddress,
                AddressCaptured: true,
                Value: var fourthValue,
                ValueCaptured: true
            }
            && fourthAddress
                == CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                    .DeriveEsiPlus8Address(esi)
            && fourthValue == esi;
    }

    private static bool IsExactCleanupCounterCompletion(
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent,
        CapacityFinalCalleeCleanupCounterGatePlan cleanupCounter)
    {
        if (!parent.ExactCleanCompletion
            || parent.FlagsAtStop is not { Carry: false }
            || cleanupCounter.Capture.EntryValidated != true
            || cleanupCounter.Capture.FreshCounterDword is not uint counterBefore
            || cleanupCounter.Capture.IncomingCarryFlag != false
            || cleanupCounter.CounterBefore != counterBefore
            || cleanupCounter.CounterAfter is not uint counterAfter
            || counterAfter != unchecked(counterBefore - 1)
            || !cleanupCounter.CounterReadReached
            || !cleanupCounter.FreshCounterDwordCaptured
            || !cleanupCounter.CounterWriteReached
            || !cleanupCounter.JnzReached
            || !cleanupCounter.ExactBoundedGateCompletion
            || cleanupCounter.UnreachedFreshCounterDwordCaptureProvided
            || cleanupCounter.UnreachedIncomingCarryFlagCaptureProvided
            || cleanupCounter.CarryAtStop != false
            || cleanupCounter.FlagsAfterDecrement is not { Carry: false }
            || cleanupCounter.CallReached
            || cleanupCounter.CalleeEntered
            || cleanupCounter.CounterAccesses.Length != 2)
        {
            return false;
        }

        bool decrementedToZero = counterAfter == 0;
        uint expectedDestination = decrementedToZero
            ? CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            : CleanupCounterNonzeroTargetRawFileOffset;
        uint expectedStop = decrementedToZero
            ? CleanupCounterBoundaryRawFileOffsetExclusive
            : CleanupCounterNonzeroTargetRawFileOffset;
        ImmutableArray<uint> expectedInstructions = decrementedToZero
            ? ImmutableArray.Create(
                CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.MovEcxRawFileOffset)
            : ImmutableArray.Create(
                CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset);
        CapacityFinalCalleeCleanupCounterGateCounterAccess read =
            cleanupCounter.CounterAccesses[0];
        CapacityFinalCalleeCleanupCounterGateCounterAccess write =
            cleanupCounter.CounterAccesses[1];
        return cleanupCounter.Outcome == (decrementedToZero
                ? CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
                : CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero)
            && read.RawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset
            && read.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && read.Address == CleanupCounterVirtualAddress
            && read.ValueCaptured
            && read.Value == counterBefore
            && write.RawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset
            && write.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            && write.Address == CleanupCounterVirtualAddress
            && write.ValueCaptured
            && write.Value == counterAfter
            && cleanupCounter.JnzTaken == !decrementedToZero
            && cleanupCounter.SelectedDestinationRawFileOffset == expectedDestination
            && cleanupCounter.SelectedDestinationVirtualAddress
                == CapacityFinalCalleeCleanupCounterGateProjection
                    .RawFileOffsetToVirtualAddress(expectedDestination)
            && cleanupCounter.MovEcxReached == decrementedToZero
            && cleanupCounter.EcxValue
                == (decrementedToZero
                    ? CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                    : null)
            && cleanupCounter.StopAtRawFileOffsetExclusive == expectedStop
            && cleanupCounter.ReachedInstructions.SequenceEqual(expectedInstructions);
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            bool executed = ParentInstructionStatus(parent, instruction).Executed;
            instructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                        .ParentGate =>
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                            .ParentGate,
                    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                        .ChildIteration =>
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                            .ChildIteration,
                    CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                        .ZeroSlotThreshold =>
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                            .ZeroSlotThreshold,
                    _ =>
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                            .StoreTail
                },
                instruction.RawFileOffset,
                Reached: true,
                Executed: executed));
        }

        if (cleanupCounter is null)
        {
            return instructions.ToImmutable();
        }

        if (cleanupCounter.CounterReadReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
                Reached: true,
                Executed: cleanupCounter.CounterWriteReached));
        }

        foreach (uint rawFileOffset in cleanupCounter.ReachedInstructions)
        {
            if (rawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset)
            {
                continue;
            }

            instructions.Add(new(
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static (bool Reached, bool Executed) ParentInstructionStatus(
        CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent,
        CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstruction instruction)
    {
        switch (instruction.Phase)
        {
            case CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase.ParentGate:
                foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateEvent item
                    in parent.TraversalPlan.ParentPlan.OrderedEvents)
                {
                    if (item.RawFileOffset == instruction.RawFileOffset)
                    {
                        return (item.Reached, item.Completed);
                    }
                }

                return (false, false);

            case CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                .ChildIteration:
                return ChildInstructionStatus(parent.ChildPlan, instruction.RawFileOffset);

            case CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase
                .ZeroSlotThreshold:
                CapacityFinalCalleeCommonZeroSlotThresholdGatePlan? threshold =
                    parent.ThresholdPlan;
                if (threshold is null)
                {
                    return (false, false);
                }

                return instruction.RawFileOffset
                    == CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                        .CompareRawFileOffset
                    ? (threshold.CompareInstructionReached, threshold.CompareReached)
                    : instruction.RawFileOffset
                        == CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
                            .BranchRawFileOffset
                        ? (threshold.BranchReached, threshold.BranchReached)
                        : (false, false);

            case CapacityFinalCalleeCommonTraversalSlotStoresLifecycleInstructionPhase.StoreTail:
                foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write
                    in parent.Writes)
                {
                    if (write.RawFileOffset == instruction.RawFileOffset)
                    {
                        return (write.Attempted, write.Committed);
                    }
                }

                return instruction.RawFileOffset
                    == CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset
                    ? (parent.CleanupJumpReached, parent.CleanupJumpReached)
                    : (false, false);

            default:
                return (false, false);
        }
    }

    private static (bool Reached, bool Executed) ChildInstructionStatus(
        CapacityFinalCalleeCommonTraversalIterationPlan? child,
        uint rawFileOffset)
    {
        if (child is null)
        {
            return (false, false);
        }

        bool reached = child.Events.Any(item => item.RawFileOffset == rawFileOffset);
        bool executed = rawFileOffset switch
        {
            CapacityFinalCalleeCommonTraversalIterationProjection.MoveEBXToEAXRawFileOffset =>
                child.EntryEBXCaptured,
            CapacityFinalCalleeCommonTraversalIterationProjection.DoubleEBXRawFileOffset =>
                child.DoubleEBXReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.ShiftEAXRawFileOffset =>
                child.ShiftEAXReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.AddFourRawFileOffset =>
                child.AddFourReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.ChildAddressLeaRawFileOffset =>
                child.ChildAddressLeaReached && child.LateEntryEDXCaptured,
            CapacityFinalCalleeCommonTraversalIterationProjection.FreshChildReadRawFileOffset =>
                child.FreshChildReadReached && child.FreshChildDWORDCaptured,
            CapacityFinalCalleeCommonTraversalIterationProjection.ChildTestRawFileOffset =>
                child.ChildTestFlags.HasValue,
            CapacityFinalCalleeCommonTraversalIterationProjection.ChildZeroBranchRawFileOffset =>
                child.ChildZeroBranchReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.MoveChildToEDXRawFileOffset =>
                child.MoveChildToEDXReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.FreshChildPlus4ReadRawFileOffset =>
                child.FreshChildPlus4ReadReached && child.FreshChildPlus4Captured,
            CapacityFinalCalleeCommonTraversalIterationProjection.AndEAXRawFileOffset =>
                child.AndEAXReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.CompareRawFileOffset =>
                child.CompareReached,
            CapacityFinalCalleeCommonTraversalIterationProjection.LoopBranchRawFileOffset =>
                child.LoopBranchReached,
            _ => false
        };
        return (reached, reached && executed);
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>();
        for (int index = 0; index < parent.OrderedReads.Length; index++)
        {
            CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead read =
                parent.OrderedReads[index];
            effects.Add(new(
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                index,
                read.RawFileOffset,
                read.ReadReached,
                read.ValueCaptured,
                Committed: false));
        }

        for (int index = 0; index < parent.Writes.Length; index++)
        {
            CapacityFinalCalleeCommonZeroSlotStoreTailWrite write = parent.Writes[index];
            effects.Add(new(
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                index,
                write.RawFileOffset,
                write.Attempted,
                write.ValueCaptured,
                write.Committed));
        }

        if (cleanupCounter is null)
        {
            return effects.ToImmutable();
        }

        for (int index = 0; index < cleanupCounter.CounterAccesses.Length; index++)
        {
            CapacityFinalCalleeCleanupCounterGateCounterAccess access =
                cleanupCounter.CounterAccesses[index];
            bool isWrite =
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write;
            effects.Add(new(
                isWrite
                    ? CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                index,
                access.RawFileOffset,
                Attempted: true,
                ValueCaptured: access.ValueCaptured,
                Committed: isWrite && access.ValueCaptured));
        }

        return effects.ToImmutable();
    }

    private static CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome.ParentGateStopped =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome.ParentGateStopped,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ParentEqualBypassesChildTo9403 =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ParentEqualBypassesChildTo9403,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroHandoffNotExactAt945C =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroMissingDerivedECXAt945C =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroMissingFreshGlobalValueAt945C =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroUnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .MissingLateEntryESIForFirstStore =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstStore,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .MissingLateEntryEDXForSecondStore =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonTraversalSlotStoresLifecycleOutcome
                .ChildMaskedDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4,
            _ =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ParentStoreCompletionNotExact
        };

    private static CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
        MapCleanupOutcome(CapacityFinalCalleeCleanupCounterGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .MissingFreshCounterDword,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .DecrementedToZero,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .DecrementedNonzero,
            _ =>
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
                    .ParentStoreCompletionNotExact
        };

    private static CapacityFinalCalleeCommonTraversalSlotStoresLifecycleFlags ToLifecycleFlags(
        CapacityFinalCalleeCleanupCounterGateFlags flags) =>
        new(
            Carry: flags.Carry,
            Parity: flags.Parity,
            AuxiliaryCarry: flags.AuxiliaryCarry,
            Zero: flags.Zero,
            Sign: flags.Sign,
            Overflow: flags.Overflow);
}
