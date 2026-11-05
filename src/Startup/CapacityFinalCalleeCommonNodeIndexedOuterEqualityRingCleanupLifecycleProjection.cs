using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleCapture ParentCapture,
        uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
{
    ParentStopped,
    ParentBypassedRing,
    ParentRingCompletionNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public enum CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
{
    ParentWrite,
    ParentFreshGlobalRead,
    ParentFreshIndexedDWORDRead,
    ParentFreshTagDWORDRead,
    ThresholdFreshGlobalRead,
    ThresholdFreshEDXPlus8Read,
    RingWriteAttempt,
    CleanupCounterRead,
    CleanupCounterWrite
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool ValueCaptured,
        bool Attempted,
        bool Committed,
        bool ForwardedFromPriorWrite);

public enum CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstructionPhase
{
    RingInsertion,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool Reached,
        bool Executed);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan ParentPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags?
        ParentFlagsAtStop => ConvertFlags(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags?
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
    public bool ExactBoundedCompletion { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}

/// <summary>
/// Joins the exact outer-equality five-store completion to the fresh cleanup-counter gate.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingCompletionRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection.CompletionRawFileOffset;
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

    public static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection.Project(
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent)
                : cleanupCounter.Outcome switch
                {
                    CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                            .MissingFreshCounterDword,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                            .DecrementedToZero,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                            .DecrementedNonzero,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                            .ParentRingCompletionNotExact
                };
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>
            orderedMemoryEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags? flagsAtStop =
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing =
            parent.ThresholdRingPlan;
        uint? entryEAX = parent.Capture.FreshThresholdEDXPlus8;
        uint? entryEDX = parent.DerivedEntryEDX;
        uint? entryESI = parent.Capture.Parent.Parent.EntryESI;
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
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset
            && parent.FlagsAtStop is { Carry: false }
            && parent.ParentEqualitySelected
            && CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
                .IsExactOuterEquality(parent.ParentPlan)
            && parent.ThresholdFallthroughSelected
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
            && parent.Writes.Length == expectedWrites.Length
            && parent.Writes.Zip(expectedWrites).All(pair =>
                pair.First.RawFileOffset == pair.Second.RawFileOffset
                && pair.First.AddressCaptured == pair.Second.Address.HasValue
                && pair.First.Address == pair.Second.Address
                && pair.First.ValueCaptured == pair.Second.Value.HasValue
                && pair.First.Value == pair.Second.Value
                && pair.First.Attempted
                && pair.First.Committed)
            && thresholdRing.Writes.All(write => write.Attempted && write.Committed);
    }

    private static bool IsExactCleanupCounterCompletion(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent,
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
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
                    ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite write in parent.Writes)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan parent)
    {
        if (parent.Outcome
            is CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                .FirstUnsignedBelowBypassesRing
                or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing)
        {
            return CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                .ParentBypassedRing;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec)
        {
            return CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                .ParentRingCompletionNotExact;
        }

        return CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
            .ParentStopped;
    }

    private static uint? Add(uint? value, uint displacement) =>
        value is uint address ? unchecked(address + displacement) : null;

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}
