using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleCapture ParentCapture,
        uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
{
    ParentStopped,
    ParentBypassedRing,
    ParentRingCompletionNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public enum CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    FreshTraversalChildDWORDRead,
    FreshTraversalChildPlus4Read,
    ThresholdFreshGlobalRead,
    ThresholdFreshEDXPlus8Read,
    RingWriteAttempt,
    CleanupCounterRead,
    CleanupCounterWrite
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool ValueCaptured,
        bool Attempted,
        bool Committed,
        bool ForwardedFromPriorWrite);

public enum CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstructionPhase
{
    RingInsertion,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool Reached,
        bool Executed);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan ParentPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags?
        ParentFlagsAtStop => ConvertFlags(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags?
        FlagsAtStop { get; init; }
    public bool ExactParentStoreCompletion { get; init; }
    public bool CleanupCounterGateEntered => ExactParentStoreCompletion;
    public bool ExactCleanupCounterCompletion { get; init; }
    public bool CounterReadAttempted => CleanupCounterPlan?.CounterReadReached == true;
    public bool CounterValueCaptured => CleanupCounterPlan?.FreshCounterDwordCaptured == true;
    public bool CounterWriteAttempted => CleanupCounterPlan?.CounterWriteReached == true;
    public bool CounterWriteCommitted => CounterWriteAttempted;
    public uint? CounterBefore => CleanupCounterPlan?.CounterBefore;
    public uint? CounterAfter => CleanupCounterPlan?.CounterAfter;
    public uint? CounterAddress => CounterReadAttempted
        ? CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
        : null;
    public ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess> CounterAccesses =>
        CleanupCounterPlan?.CounterAccesses
        ?? ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>.Empty;
    public bool? CounterJnzTaken => CleanupCounterPlan?.JnzTaken;
    public uint? CounterJnzDestinationRawFileOffset =>
        CleanupCounterPlan?.SelectedDestinationRawFileOffset;
    public bool ZeroPathMovEcxReached => CleanupCounterPlan?.MovEcxReached == true;
    public uint? ZeroPathEcx => CleanupCounterPlan?.EcxValue;
    public bool FreshCounterCaptureProvidedButUnused =>
        !CleanupCounterGateEntered && Capture.FreshCounterDword.HasValue;
    public bool CounterDecrementPreservedParentCarry =>
        ParentFlagsAtStop is { Carry: not null } parentFlags
        && CleanupCounterPlan?.FlagsAfterDecrement is { } decrementFlags
        && decrementFlags.Carry == parentFlags.Carry;
    public bool CounterNonzeroTargetEntered => false;
    public bool ZeroPathCallReached => false;
    public bool ZeroPathCalleeEntered => false;
    public bool CallReached => false;
    public bool CalleeEntered => false;
    public bool EpilogueEntered => false;
    public bool NativeInstructionsExecuted => false;
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonTraversalIterationFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}

/// <summary>
/// Joins the exact second-equality five-store completion to the existing cleanup-counter gate.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingCompletionRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection
            .RingCompletionRawFileOffset;
    public const uint CleanupCounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterNonzeroTargetRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;
    public const uint CleanupCounterBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    private static readonly ImmutableArray<uint> RingStoreRawFileOffsets =
        ImmutableArray.Create(
            0x012C941Cu,
            0x012C941Fu,
            0x012C9422u,
            0x012C9425u,
            0x012C9428u);

    public static CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection.Project(
                capture.ParentCapture);
        bool exactParentStoreCompletion = IsExactParentStoreCompletion(parent);
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter = exactParentStoreCompletion
            ? CapacityFinalCalleeCleanupCounterGateProjection.Project(new(
                EntryValidated: true,
                FreshCounterDword: capture.FreshCounterDword,
                IncomingCarryFlag: parent.FlagsAtStop?.Carry))
            : null;
        bool exactCleanupCounterCompletion = cleanupCounter is not null
            && IsExactCleanupCounterCompletion(parent, cleanupCounter);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent)
                : cleanupCounter.Outcome switch
                {
                    CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                            .MissingFreshCounterDword,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                            .DecrementedToZero,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                            .DecrementedNonzero,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                            .ParentRingCompletionNotExact
                };
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect>
            orderedMemoryEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags? flagsAtStop =
            cleanupCounter?.FlagsAfterDecrement is { } decrementFlags
                ? new(
                    decrementFlags.Carry,
                    decrementFlags.Parity,
                    decrementFlags.AuxiliaryCarry,
                    decrementFlags.Zero,
                    decrementFlags.Sign,
                    decrementFlags.Overflow)
                : ConvertFlags(parent.FlagsAtStop);
        bool exactBoundedCompletion = cleanupCounter is null
            ? parent.ExactBoundedCompletion
            : cleanupCounter.FreshCounterDwordCaptured
                && exactCleanupCounterCompletion;
        bool exactCleanCompletion = cleanupCounter is null
            ? parent.ExactCleanCompletion && !capture.FreshCounterDword.HasValue
            : exactCleanupCounterCompletion && parent.ExactCleanCompletion;

        return new(capture, outcome)
        {
            ParentPlan = parent,
            CleanupCounterPlan = cleanupCounter,
            OrderedMemoryEffects = orderedMemoryEffects,
            OrderedInstructions = orderedInstructions,
            FlagsAtStop = flagsAtStop,
            ExactParentStoreCompletion = exactParentStoreCompletion,
            ExactCleanupCounterCompletion = exactCleanupCounterCompletion,
            ExactBoundedCompletion = exactBoundedCompletion,
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = cleanupCounter?.StopAtRawFileOffsetExclusive
                ?? parent.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentStoreCompletion(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing =
            parent.ThresholdRingPlan;
        uint? entryEAX = parent.Capture.FreshThresholdEDXPlus8;
        uint? entryEDX = parent.DerivedChildEDX;
        uint? entryESI = parent.OriginalRootEntryESI;
        (uint RawFileOffset, uint? Address, uint? Value)[] expectedWrites =
        [
            (
                CapacityFinalCalleeCommonRingInsertionProjection.FirstWriteRawFileOffset,
                Add(entryEAX, CapacityFinalCalleeCommonRingInsertionProjection.EAXDisplacement),
                entryESI),
            (
                CapacityFinalCalleeCommonRingInsertionProjection.SecondWriteRawFileOffset,
                Add(entryEDX, CapacityFinalCalleeCommonRingInsertionProjection.EDXDisplacement),
                entryESI),
            (
                CapacityFinalCalleeCommonRingInsertionProjection.ThirdWriteRawFileOffset,
                Add(entryESI, CapacityFinalCalleeCommonRingInsertionProjection.ESIPlus8Displacement),
                entryEAX),
            (
                CapacityFinalCalleeCommonRingInsertionProjection.FourthWriteRawFileOffset,
                Add(entryESI, CapacityFinalCalleeCommonRingInsertionProjection.ESIPlusCDisplacement),
                entryEDX),
            (
                CapacityFinalCalleeCommonRingInsertionProjection.FifthWriteRawFileOffset,
                Add(entryESI, CapacityFinalCalleeCommonRingInsertionProjection.ESIPlus18Displacement),
                0)
        ];

        return parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset
            && parent.FlagsAtStop is { Carry: false }
            && CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection
                .IsExactSecondChildEqualityHandoff(parent.SecondTraversalPlan)
            && parent.ExactSecondChildEqualityHandoff
            && parent.ThresholdGateEntered
            && parent.RingBoundaryReached
            && !parent.RingBoundaryInstructionEntered
            && thresholdRing is
            {
                Outcome: CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec,
                ParentExactFallthroughHandoff: true,
                ChildBoundaryReached: true,
                ChildBoundaryInstructionEntered: false,
                DecAtBoundaryEntered: false,
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                ChildStoresPreserveParentFlags: true
            }
            && thresholdRing.Capture.FreshEDXPlus8 == entryEAX
            && thresholdRing.ChildEntryEAX == entryEAX
            && thresholdRing.ChildLateEntryEDX == entryEDX
            && thresholdRing.ChildEntryESI == entryESI
            && thresholdRing.ChildPlan is
            {
                Outcome: CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                ExactCleanCompletion: true
            }
            && thresholdRing.Writes.Length == RingStoreRawFileOffsets.Length
            && thresholdRing.Writes
                .Select(write => write.RawFileOffset)
                .SequenceEqual(RingStoreRawFileOffsets)
            && thresholdRing.Writes.All(write => write.Attempted && write.Committed)
            && parent.Writes.Length == expectedWrites.Length
            && parent.Writes.Zip(expectedWrites).All(pair =>
                pair.First.RawFileOffset == pair.Second.RawFileOffset
                && pair.First.AddressCaptured == pair.Second.Address.HasValue
                && pair.First.Address == pair.Second.Address
                && pair.First.ValueCaptured == pair.Second.Value.HasValue
                && pair.First.Value == pair.Second.Value
                && pair.First.Attempted
                && pair.First.Committed);
    }

    private static bool IsExactCleanupCounterCompletion(
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent,
        CapacityFinalCalleeCleanupCounterGatePlan cleanupCounter)
    {
        if (!IsExactParentStoreCompletion(parent)
            || cleanupCounter.Capture.EntryValidated != true
            || cleanupCounter.Capture.FreshCounterDword is not uint counterBefore
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
            || cleanupCounter.CarryAtStop != parent.FlagsAtStop?.Carry
            || cleanupCounter.FlagsAfterDecrement is not { }
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
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .FreshTraversalChildDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildPlus4Read =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .FreshTraversalChildPlus4Read,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                            .RingWriteAttempt
                },
                effects.Count,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed,
                effect.ForwardedFromPriorWrite));
        }

        if (cleanupCounter is null)
        {
            return effects.ToImmutable();
        }

        foreach (CapacityFinalCalleeCleanupCounterGateCounterAccess access
            in cleanupCounter.CounterAccesses)
        {
            bool isWrite = access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write;
            effects.Add(new(
                isWrite
                    ? CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                effects.Count,
                access.RawFileOffset,
                access.Address,
                AddressCaptured: true,
                access.Value,
                access.ValueCaptured,
                Attempted: true,
                Committed: isWrite && access.ValueCaptured,
                ForwardedFromPriorWrite: false));
        }

        return effects.ToImmutable();
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite write in parent.Writes)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstructionPhase
                    .RingInsertion,
                write.RawFileOffset,
                Reached: write.Attempted,
                Executed: write.Committed));
        }

        if (cleanupCounter is null)
        {
            return instructions.ToImmutable();
        }

        if (cleanupCounter.CounterReadReached)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan parent)
    {
        if (parent.Outcome
            is CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                .FirstUnsignedBelowBypassesRing
                or CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing)
        {
            return CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                .ParentBypassedRing;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec)
        {
            return CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
                .ParentRingCompletionNotExact;
        }

        return CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleOutcome
            .ParentStopped;
    }

    private static uint? Add(uint? value, uint displacement) =>
        value is uint address ? unchecked(address + displacement) : null;

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonTraversalIterationFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}
