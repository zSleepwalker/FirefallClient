using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleCapture StoreCapture,
        uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
{
    ParentStopped,
    ParentOtherBoundedExit,
    ParentStoreCompletionNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
{
    ParentRead,
    ParentStore,
    CleanupCounterRead,
    CleanupCounterWrite
}

public enum CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
{
    ParentTraversal,
    ChildIteration,
    ZeroSlotThreshold,
    StoreTail,
    CleanupCounter
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase Phase,
        uint RawFileOffset,
        bool? Reached,
        bool? Executed);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan ParentStorePlan
        { get; init; } = null!;
    public CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan ParentLifecycle =>
        ParentStorePlan.ParentLifecycle;
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome ParentOutcome =>
        ParentStorePlan.Outcome;
    public ImmutableArray<CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleRead>
        OrderedReads => ParentStorePlan.OrderedReads;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> StoreWrites =>
        ParentStorePlan.Writes;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess> CounterAccesses =>
        CleanupCounterPlan?.CounterAccesses
        ?? ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>
                .Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>
        OrderedInstructions { get; init; } =
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>
                .Empty;
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags?
        ParentFlagsAtStop => ParentStorePlan.FlagsAtStop is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null;
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags?
        FlagsAtStop { get; init; }
    public bool ExactParentStoreCompletion { get; init; }
    public bool ExactCleanupCounterCompletion { get; init; }
    public bool CleanupCounterGateEntered => ExactParentStoreCompletion;
    public bool ParentCleanupJumpReached => ParentStorePlan.CleanupJumpReached;
    public bool ParentCleanupJumpTargetEntered => false;
    public bool CounterReadAttempted => CleanupCounterPlan?.CounterReadReached == true;
    public bool CounterValueCaptured => CleanupCounterPlan?.FreshCounterDwordCaptured == true;
    public bool CounterWriteAttempted => CleanupCounterPlan?.CounterWriteReached == true;
    public bool CounterWriteCommitted => CounterWriteAttempted;
    public uint? CounterBefore => CleanupCounterPlan?.CounterBefore;
    public uint? CounterAfter => CleanupCounterPlan?.CounterAfter;
    public uint? CounterAddress => CounterReadAttempted
        ? CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
        : null;
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
}

/// <summary>
/// Joins the node-indexed four-store jump to the fresh cleanup-counter gate.
/// </summary>
public static class CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleProjection
            .EntryRawFileOffset;
    public const uint StoreTailEntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleProjection
            .StoreTailEntryRawFileOffset;
    public const uint CleanupCounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterNonzeroTargetRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;
    public const uint CleanupCounterBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    public static CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleProjection.Project(
                capture.StoreCapture);
        bool exactParentStoreCompletion = IsExactParentStoreCompletion(parent);
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter = exactParentStoreCompletion
            ? CapacityFinalCalleeCleanupCounterGateProjection.Project(new(
                EntryValidated: true,
                FreshCounterDword: capture.FreshCounterDword,
                IncomingCarryFlag: parent.FlagsAtStop?.Carry))
            : null;
        bool exactCleanupCounterCompletion = cleanupCounter is not null
            && IsExactCleanupCounterCompletion(parent, cleanupCounter);

        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome outcome =
            cleanupCounter is null
                ? MapParentOutcome(parent)
                : cleanupCounter.Outcome switch
                {
                    CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                            .MissingFreshCounterDword,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                            .DecrementedToZero,
                    CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                            .DecrementedNonzero,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                            .ParentStoreCompletionNotExact
                };
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>
            orderedEffects = BuildOrderedMemoryEffects(parent, cleanupCounter);
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>
            orderedInstructions = BuildOrderedInstructions(parent, cleanupCounter);
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags? flagsAtStop =
            cleanupCounter?.FlagsAfterDecrement is { } decrementFlags
                ? ToLifecycleFlags(decrementFlags)
                : parent.FlagsAtStop is { } parentFlags
                    ? ToLifecycleFlags(parentFlags)
                    : null;
        bool exactBoundedCompletion = cleanupCounter is null
            ? parent.ExactBoundedCompletion
            : exactCleanupCounterCompletion;
        bool exactCleanCompletion = cleanupCounter is null
            ? parent.ExactCleanCompletion && !capture.FreshCounterDword.HasValue
            : exactCleanupCounterCompletion && parent.ExactCleanCompletion;

        return new(capture, outcome)
        {
            ParentStorePlan = parent,
            CleanupCounterPlan = cleanupCounter,
            OrderedMemoryEffects = orderedEffects,
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
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonZeroSlotStoreTailPlan? storeTail = parent.StoreTail;
        return parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.StopAtRawFileOffsetExclusive == CleanupCounterEntryRawFileOffset
            && parent.FlagsAtStop is { Carry: false }
            && parent.ThresholdCompareFlagsPreservedThroughStores
            && parent.CleanupJumpReached
            && storeTail is
            {
                Outcome: CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget,
                ExactBoundedExitAdmitted: true,
                JumpReached: true,
                JumpTargetRawFileOffset: CleanupCounterEntryRawFileOffset,
                StopAtRawFileOffsetExclusive: CleanupCounterEntryRawFileOffset,
                BoundaryReached: true,
                BoundaryInstructionEntered: false,
                JumpTargetEntered: false,
                Writes.Length: 4
            }
            && parent.Writes.Select(write => write.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C9468,
                0x012C946A,
                0x012C946D,
                0x012C9470
            })
            && parent.Writes.All(write => write.Attempted && write.Committed)
            && parent.OrderedInstructions.TakeLast(5).Select(instruction =>
                instruction.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9468,
                    0x012C946A,
                    0x012C946D,
                    0x012C9470,
                    0x012C9473
                });
    }

    private static bool IsExactCleanupCounterCompletion(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent,
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
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind
                    is CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind
                        .ParentWrite
                    or CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleEffectKind
                        .StoreWrite
                    ? CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentStore
                    : CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentRead,
                effects.Count,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.Attempted,
                effect.ValueCaptured,
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
                    ? CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                effects.Count,
                access.RawFileOffset,
                access.Address,
                AddressCaptured: true,
                access.Value,
                Attempted: true,
                access.ValueCaptured,
                Committed: isWrite && access.ValueCaptured));
        }

        return effects.ToImmutable();
    }

    private static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>
        BuildOrderedInstructions(
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent,
            CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter)
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>();
        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstruction instruction
            in parent.OrderedInstructions)
        {
            bool? executed = null;
            foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write in parent.Writes)
            {
                if (write.RawFileOffset == instruction.RawFileOffset)
                {
                    executed = write.Committed;
                    break;
                }
            }

            if (instruction.RawFileOffset
                == CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset)
            {
                executed = parent.CleanupJumpReached;
            }

            instructions.Add(new(
                instruction.Phase switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .ChildIteration =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ChildIteration,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .ZeroSlotThreshold =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ZeroSlotThreshold,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleInstructionPhase
                        .StoreTail =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .StoreTail,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
                            .ParentTraversal
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
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
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
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstructionPhase
                    .CleanupCounter,
                rawFileOffset,
                Reached: true,
                Executed: true));
        }

        return instructions.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
        MapParentOutcome(
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecyclePlan parent) =>
        parent.Outcome switch
        {
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                .ParentOtherBoundedExit =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentOtherBoundedExit,
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleOutcome
                .CompletedToCleanupJump =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentStoreCompletionNotExact,
            _ =>
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                    .ParentStopped
        };

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeCleanupCounterGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeCommonNodeIndexedTraversalSlotStoresLifecycleFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}
