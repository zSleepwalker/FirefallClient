using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture(
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleCapture StoreCapture,
    uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
{
    ParentBlocked,
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

public enum CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
{
    ParentRead,
    ParentStore,
    CleanupCounterRead,
    CleanupCounterWrite
}

public enum CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
{
    IndexedCount,
    ParentTagGate,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool Reached,
        bool Executed);

public readonly record struct
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public sealed record CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan(
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome Outcome,
    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan ParentLifecycle)
{
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecyclePlan TraversalLifecycle =>
        ParentLifecycle.ParentLifecycle;
    public CapacityFinalCalleeCommonIndexedTagTraversalLifecyclePlan TraversalPlan =>
        ParentLifecycle.TraversalPlan;
    public CapacityFinalCalleeCommonTraversalIterationPlan? ChildPlan =>
        ParentLifecycle.ChildPlan;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead> OrderedReads =>
        ParentLifecycle.OrderedReads;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> StoreWrites =>
        ParentLifecycle.Writes;
    public ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess> CounterAccesses =>
        CleanupCounterPlan?.CounterAccesses
        ?? ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>.Empty;
    public CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags? ParentFlagsAtStop =>
        ParentLifecycle.FlagsAtStop;
    public CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags? FlagsAtStop { get; init; }
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
public static class CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection.EntryRawFileOffset;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection
            .StoreTailEntryRawFileOffset;
    public const uint CleanupCounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterNonzeroTargetRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;
    public const uint CleanupCounterBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    public static CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent =
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleProjection.Project(
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

        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent.Outcome)
                : MapCleanupOutcome(cleanupCounter.Outcome);
        ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>
            orderedEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags? flagsAtStop =
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
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent,
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture capture)
    {
        uint? derivedChildECX = parent.ChildFieldAddressAt93EE;
        uint? originalTraversalEDX = parent.ParentDerivedTagEDX;
        uint? entryESI = capture.StoreCapture.LateEntryESI;
        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = parent.StoreTail;
        if (storeTail is null)
        {
            return false;
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            || !parent.ExactBoundedCompletion
            || !parent.ExactCleanCompletion
            || parent.StopAtRawFileOffsetExclusive
                != CleanupCounterEntryRawFileOffset
            || parent.DestinationEntered
            || !parent.ExactParentZeroHandoff
            || !parent.Internal945CEntered
            || !parent.ThresholdCompareReached
            || parent.ThresholdPlan is not
            {
                GlobalReadReached: true,
                FreshGlobalValueCaptured: true,
                EntryECXCaptured: true
            }
            || parent.ThresholdBranchTaken != false
            || parent.ThresholdBranchSelectedDestinationRawFileOffset
                != StoreTailEntryRawFileOffset
            || !parent.Internal9468Entered
            || parent.LateFreshChildPlus4CaptureProvidedButUnused
            || parent.FreshGlobalValueCaptureProvidedButUnused
            || parent.ChildPlan is not
            {
                FreshChildDWORD: 0,
                FreshChildPlus4ReadReached: false
            }
            || !parent.StoreTailEntryECXIsDerivedFromThresholdLeft
            || !parent.StoreTailLateEntryEDXIsOriginalParentDerivedTagEDX
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

        if (!parent.OrderedInstructions.TakeLast(5).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473
                }))
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
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent,
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
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            (bool reached, bool executed) = ParentInstructionStatus(parent, instruction);
            instructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .IndexedCount =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .IndexedCount,
                    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .ParentTagGate =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ParentTagGate,
                    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .ChildIteration =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ChildIteration,
                    CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .ZeroSlotThreshold =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ZeroSlotThreshold,
                    _ =>
                        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .StoreTail
                },
                instruction.RawFileOffset,
                Reached: reached,
                Executed: executed));
        }

        if (cleanupCounter is null)
        {
            return instructions.ToImmutable();
        }

        if (cleanupCounter.CounterReadReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static (bool Reached, bool Executed) ParentInstructionStatus(
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent,
        CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstruction instruction)
    {
        switch (instruction.Phase)
        {
            case CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                .IndexedCount:
                CapacityFinalCalleeCommonIndexedCountClassificationPlan count = parent.CountPlan;
                return instruction.RawFileOffset switch
                {
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.FreshLoadRawFileOffset =>
                        (count.IndexedDWORDReadAttempted, count.IndexedDWORDReadCompleted),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareRawFileOffset =>
                        (count.CompareInstructionReached, count.CompareReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.JnzRawFileOffset =>
                        (count.JnzReached, count.JnzReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.XorRawFileOffset =>
                        (count.XorEAXReached, count.XorEAXReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.JumpRawFileOffset =>
                        (count.JumpReached, count.JumpReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.ShiftRawFileOffset =>
                        (count.ShiftECXReached, count.ShiftECXReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.MoveImmediateRawFileOffset =>
                        (count.MoveImmediateEAXReached, count.MoveImmediateEAXReached),
                    CapacityFinalCalleeCommonIndexedCountClassificationProjection.SubtractRawFileOffset =>
                        (count.SubtractReached, count.SubtractReached),
                    _ => (false, false)
                };

            case CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                .ParentTagGate:
                foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateEvent item
                    in parent.TraversalPlan.TagPlan?.OrderedEvents
                        ?? ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>.Empty)
                {
                    if (item.RawFileOffset == instruction.RawFileOffset)
                    {
                        return (item.Reached, item.Completed);
                    }
                }

                return (false, false);

            case CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
                .ChildIteration:
                return ChildInstructionStatus(parent.ChildPlan, instruction.RawFileOffset);

            case CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase
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

            case CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleInstructionPhase.StoreTail:
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
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>();
        for (int index = 0; index < parent.OrderedReads.Length; index++)
        {
            CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead read =
                parent.OrderedReads[index];
            effects.Add(new(
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
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
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
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
                    ? CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                index,
                access.RawFileOffset,
                Attempted: true,
                ValueCaptured: access.ValueCaptured,
                Committed: isWrite && access.ValueCaptured));
        }

        return effects.ToImmutable();
    }

    private static CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome.ParentBlocked =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome.ParentBlocked,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome.ParentTagGateStopped =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome.ParentGateStopped,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ParentEqualBypassesTraversalTo9403 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentEqualBypassesChildTo9403,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ParentUnequalHandoffNotExact =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentUnequalHandoffNotExact,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .MissingFreshChildDWORDAt93F1 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildZeroHandoffNotExactAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroHandoffNotExactAt945C,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildZeroMissingDerivedECXAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroMissingDerivedECXAt945C,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildZeroMissingFreshGlobalValueAt945C =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroMissingFreshGlobalValueAt945C,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildZeroUnsignedBelowBranchTarget =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroUnsignedBelowBranchTarget,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildZeroUnsignedNotBelowFallthrough =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildZeroUnsignedNotBelowFallthrough,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .MissingLateEntryESIForFirstStore =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstStore,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .MissingLateEntryEDXForSecondStore =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .MissingLateFreshChildPlus4At93F9 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildMaskedEqualsEntryEDIAt9403 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildMaskedEqualsEntryEDIAt9403,
            CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleOutcome
                .ChildMaskedDiffersFromEntryEDITo93E4 =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ChildMaskedDiffersFromEntryEDITo93E4,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentStoreCompletionNotExact
        };

    private static CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
        MapCleanupOutcome(CapacityFinalCalleeCleanupCounterGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .MissingFreshCounterDword,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .DecrementedToZero,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .DecrementedNonzero,
            _ =>
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentStoreCompletionNotExact
        };

    private static CapacityFinalCalleeCommonIndexedTraversalSlotStoresLifecycleFlags ToLifecycleFlags(
        CapacityFinalCalleeCleanupCounterGateFlags flags) =>
        new(
            Carry: flags.Carry,
            Parity: flags.Parity,
            AuxiliaryCarry: flags.AuxiliaryCarry,
            Zero: flags.Zero,
            Sign: flags.Sign,
            Overflow: flags.Overflow);
}
