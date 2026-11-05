using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleCapture ParentCapture,
        uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
{
    ParentStopped,
    ParentBypassedRing,
    ParentRingCompletionNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public enum CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
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
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool ValueCaptured,
        bool Attempted,
        bool Committed);

public enum CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstructionPhase
{
    RingInsertion,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool Reached,
        bool Executed);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan ParentPlan
        { get; init; } = null!;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags? ParentFlagsAtStop =>
        ConvertFlags(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags? FlagsAtStop
        { get; init; }
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

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}

/// <summary>
/// Joins the exact five-store node-indexed equality-ring completion to the fresh cleanup-counter gate.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingCompletionRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection.CompletionRawFileOffset;
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

    public static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection.Project(
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
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent)
                : cleanupCounter.Outcome switch
                {
                    CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                            .MissingFreshCounterDword,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                            .DecrementedToZero,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                            .DecrementedNonzero,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                            .ParentRingCompletionNotExact
                };
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>
            orderedMemoryEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags? flagsAtStop =
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
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan? thresholdRing =
            parent.ThresholdRingPlan;
        CapacityFinalCalleeCommonRingInsertionPlan? ring = parent.ThresholdRingPlan?.ChildPlan;
        return parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && parent.ExactChildEqualityHandoff
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset
            && parent.FlagsAtStop is { Carry: false }
            && parent.ThresholdFallthroughSelected
            && thresholdRing is { ChildStoresPreserveParentFlags: true }
            && parent.RingBoundaryReached
            && !parent.RingBoundaryInstructionEntered
            && thresholdRing.DecAtBoundaryEntered == false
            && ring is
            {
                Outcome: CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset
            }
            && thresholdRing.Writes.Length == RingStoreRawFileOffsets.Length
            && thresholdRing.Writes
                .Select(write => write.RawFileOffset)
                .SequenceEqual(RingStoreRawFileOffsets)
            && thresholdRing.Writes.All(write =>
                write.Attempted && write.Committed)
            && thresholdRing.DecAtBoundaryEntered == false;
    }

    private static bool IsExactCleanupCounterCompletion(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent,
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
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentWrite,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ParentFreshTagDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildDWORDRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .FreshTraversalChildDWORDRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildPlus4Read =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .FreshTraversalChildPlus4Read,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                            .RingWriteAttempt
                },
                effects.Count,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed));
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
                    ? CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                effects.Count,
                access.RawFileOffset,
                access.Address,
                AddressCaptured: true,
                access.Value,
                access.ValueCaptured,
                Attempted: true,
                Committed: isWrite && access.ValueCaptured));
        }

        return effects.ToImmutable();
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite write
            in parent.ThresholdRingPlan?.Writes
                ?? ImmutableArray<CapacityFinalCalleeCommonDualThresholdRingLifecycleWrite>.Empty)
        {
            instructions.Add(new(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parent)
    {
        if (parent.Outcome
            is CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                .FirstUnsignedBelowBypassesRing
                or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing)
        {
            return CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                .ParentBypassedRing;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec)
        {
            return CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                .ParentRingCompletionNotExact;
        }

        return CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
            .ParentStopped;
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;
}
