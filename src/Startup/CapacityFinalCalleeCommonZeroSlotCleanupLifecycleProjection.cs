using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonZeroSlotCleanupLifecycleCapture(
    uint? ParentEntryECX,
    uint? FreshGlobalValue,
    uint? LateEntryESI,
    uint? LateEntryEDX,
    uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
{
    ThresholdCompareOperandsMissing,
    UnsignedBelowBranchTarget,
    MissingLateEntryESIForFirstStore,
    MissingLateEntryEDXForSecondStore,
    MissingFreshCounterDword,
    DecrementedNonzeroBranchTarget,
    DecrementedToZeroBeforeCall
}

public enum CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect(
    uint RawFileOffset,
    CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind Kind,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct CapacityFinalCalleeCommonZeroSlotCleanupLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan(
    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome Outcome)
{
    public uint EntryRawFileOffset =>
        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.EntryRawFileOffset;
    public CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan StoresLifecycle { get; init; } =
        null!;
    public CapacityFinalCalleeCleanupCounterGatePlan CleanupCounterGate { get; init; } =
        null!;
    public bool CleanupCounterGateEntryReached { get; init; }
    public uint CleanupCounterGateEntryRawFileOffset =>
        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection
            .CleanupCounterGateEntryRawFileOffset;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect>.Empty;
    public bool CounterReadAttempted =>
        CleanupCounterGate.CounterAccesses.Any(
            access => access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Read);
    public bool CounterReadValueCaptured => CleanupCounterGate.FreshCounterDwordCaptured;
    public bool CounterWriteCommitted => CleanupCounterGate.CounterWriteReached;
    public bool CounterReadOccursAfterAllFourStores =>
        CleanupCounterGateEntryReached && AllFourStoresCommitted && CounterReadAttempted &&
        IndexOfEffect(
            CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read,
            CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress)
        > IndexOfEffect(
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.FourthStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Write,
            address: null);
    public bool CounterWriteOccursAfterCounterRead =>
        CounterReadAttempted && CounterWriteCommitted &&
        IndexOfEffect(
            CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Write,
            CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress)
        > IndexOfEffect(
            CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read,
            CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress);
    public bool CounterValueIsIndependentFreshCapture =>
        CleanupCounterGateEntryReached
        && Capture.FreshCounterDword.HasValue
        && CleanupCounterGate.Capture.FreshCounterDword == Capture.FreshCounterDword
        && CleanupCounterGate.CounterBefore == Capture.FreshCounterDword;
    public CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? ParentCompareFlags =>
        StoresLifecycle.ParentCompareFlags;
    public bool? DerivedIncomingCarryFlag =>
        CleanupCounterGate.Capture.IncomingCarryFlag;
    public bool IncomingCarryFlagDerivedFromParentCompare =>
        CleanupCounterGateEntryReached
        && ParentCompareFlags is { Carry: false }
        && DerivedIncomingCarryFlag == false;
    public bool LateEntryESICaptureVisible => StoresLifecycle.LateEntryESICaptureVisible;
    public bool LateEntryEDXCaptureVisible => StoresLifecycle.LateEntryEDXCaptureVisible;
    public bool LateEntryESIUnusedBecauseThresholdBypassed =>
        StoresLifecycle.LateEntryESIUnusedBecauseThresholdBypassed;
    public bool LateEntryEDXUnusedBecauseThresholdBypassed =>
        StoresLifecycle.LateEntryEDXUnusedBecauseThresholdBypassed;
    public bool LateEntryEDXUnusedBecauseFirstStoreBlocked =>
        StoresLifecycle.LateEntryEDXUnusedBecauseFirstStoreBlocked;
    public bool FreshCounterDwordCaptureUnused =>
        !CleanupCounterGateEntryReached && Capture.FreshCounterDword.HasValue;
    public bool AllFourStoresCommitted =>
        StoresLifecycle.Writes.Length == 4
        && StoresLifecycle.Writes.All(write => write.Committed);
    public bool CleanupJumpReached => StoresLifecycle.CleanupJumpReached;
    public uint? CleanupJumpTargetRawFileOffset =>
        StoresLifecycle.CleanupJumpTargetRawFileOffset;
    public bool StoreJumpTargetStoppedBeforeEntry =>
        CleanupJumpReached
        && CleanupJumpTargetRawFileOffset
            == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection
                .CleanupCounterGateEntryRawFileOffset
        && !StoresLifecycle.CleanupJumpTargetEntered
        && StoresLifecycle.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection
                .CleanupCounterGateEntryRawFileOffset;
    /// <summary>Whether the final bounded stop destination itself was entered.</summary>
    public bool DestinationEntered => false;
    public bool NonzeroBranchTargetEntered => false;
    public bool ZeroPathFallthroughEntered => CleanupCounterGate.MovEcxReached;
    public bool DirectCallReached => CleanupCounterGate.CallReached;
    public bool CalleeEntered => CleanupCounterGate.CalleeEntered;
    public bool ExactBoundedCompletion =>
        CleanupCounterGateEntryReached
        && StoresLifecycle.ThresholdGate.ExactCleanCompletion
        && StoresLifecycle.ExactCleanCompletion
        && AllFourStoresCommitted
        && StoreJumpTargetStoppedBeforeEntry
        && CounterReadOccursAfterAllFourStores
        && CounterReadAttempted
        && CounterReadValueCaptured
        && CounterWriteCommitted
        && CounterWriteOccursAfterCounterRead
        && CleanupCounterGate.JnzReached
        && CleanupCounterGate.ExactBoundedGateCompletion
        && !CleanupCounterGate.CallReached
        && !CleanupCounterGate.CalleeEntered
        && !LateEntryESIUnusedBecauseThresholdBypassed
        && !LateEntryEDXUnusedBecauseThresholdBypassed
        && !LateEntryEDXUnusedBecauseFirstStoreBlocked
        && !FreshCounterDwordCaptureUnused
        && !CleanupCounterGate.UnreachedFreshCounterDwordCaptureProvided
        && !CleanupCounterGate.UnreachedIncomingCarryFlagCaptureProvided
        && (CleanupCounterGate.JnzTaken == true
                && CleanupCounterGate.SelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCleanupCounterGateProjection
                        .NonzeroBranchTargetRawFileOffset
                && !CleanupCounterGate.MovEcxReached
                && CleanupCounterGate.ReachedInstructions.SequenceEqual(
                    new uint[]
                    {
                        CapacityFinalCalleeCleanupCounterGateProjection
                            .CounterDecrementRawFileOffset,
                        CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset
                    })
                && StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCleanupCounterGateProjection
                        .NonzeroBranchTargetRawFileOffset
            || CleanupCounterGate.JnzTaken == false
                && CleanupCounterGate.SelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCleanupCounterGateProjection
                        .ZeroPathRawFileOffset
                && CleanupCounterGate.MovEcxReached
                && CleanupCounterGate.EcxValue
                    == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                && CleanupCounterGate.ReachedInstructions.SequenceEqual(
                    new uint[]
                    {
                        CapacityFinalCalleeCleanupCounterGateProjection
                            .CounterDecrementRawFileOffset,
                        CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset,
                        CapacityFinalCalleeCleanupCounterGateProjection.MovEcxRawFileOffset
                    })
                && StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeCleanupCounterGateProjection
                        .BoundaryRawFileOffsetExclusive);
    public uint StopAtRawFileOffsetExclusive =>
        CleanupCounterGateEntryReached
            ? CleanupCounterGate.StopAtRawFileOffsetExclusive
            : StoresLifecycle.StopAtRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailWrite> Writes =>
        StoresLifecycle.Writes;

    public CapacityFinalCalleeCommonZeroSlotCleanupLifecycleFlags? FlagsAtStop =>
        CleanupCounterGate.FlagsAfterDecrement is
            CapacityFinalCalleeCleanupCounterGateFlags decrementFlags
                ? new(
                    decrementFlags.Carry,
                    decrementFlags.Parity,
                    decrementFlags.AuxiliaryCarry,
                    decrementFlags.Zero,
                    decrementFlags.Sign,
                    decrementFlags.Overflow)
                : ToLifecycleFlags(StoresLifecycle.FlagsAtStop);

    private int IndexOfEffect(
        uint rawFileOffset,
        CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind kind,
        uint? address)
    {
        for (int i = 0; i < OrderedMemoryEffects.Length; i++)
        {
            CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect effect =
                OrderedMemoryEffects[i];
            if (effect.RawFileOffset == rawFileOffset
                && effect.Kind == kind
                && (address is null || effect.Address == address))
            {
                return i;
            }
        }

        return -1;
    }

    private static CapacityFinalCalleeCommonZeroSlotCleanupLifecycleFlags? ToLifecycleFlags(
        CapacityFinalCalleeCommonZeroSlotStoreTailFlags? flags) =>
        flags is CapacityFinalCalleeCommonZeroSlotStoreTailFlags known
            ? new(
                Carry: known.Carry,
                Parity: known.Parity,
                AuxiliaryCarry: known.AuxiliaryCarry,
                Zero: known.Zero,
                Sign: known.Sign,
                Overflow: known.Overflow)
            : null;
}

/// <summary>
/// Composes the common zero-slot stores and their exact jump into the cleanup counter gate.
/// </summary>
public static class CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterGateEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupCounterGateCounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    public static CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonZeroSlotStoresLifecyclePlan stores =
            CapacityFinalCalleeCommonZeroSlotStoresLifecycleProjection.Project(new(
                ParentEntryECX: capture.ParentEntryECX,
                FreshGlobalValue: capture.FreshGlobalValue,
                LateEntryESI: capture.LateEntryESI,
                LateEntryEDX: capture.LateEntryEDX));

        bool cleanupCounterGateEntryReached =
            stores.ExactCleanCompletion
            && stores.ThresholdGate.ExactCleanCompletion
            && stores.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .CompletedToCleanupJump
            && stores.CleanupJumpReached
            && stores.CleanupJumpTargetRawFileOffset == CleanupCounterGateEntryRawFileOffset
            && !stores.CleanupJumpTargetEntered
            && stores.StopAtRawFileOffsetExclusive == CleanupCounterGateEntryRawFileOffset
            && stores.Writes.Length == 4
            && stores.Writes.All(write => write.Committed)
            && stores.ParentCompareFlags is
            {
                Carry: false
            };

        CapacityFinalCalleeCleanupCounterGatePlan counterGate =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(new(
                EntryValidated: cleanupCounterGateEntryReached,
                FreshCounterDword: capture.FreshCounterDword,
                IncomingCarryFlag: cleanupCounterGateEntryReached
                    ? stores.ParentCompareFlags!.Value.Carry
                    : null));

        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome outcome =
            (stores.Outcome, counterGate.Outcome) switch
            {
                (CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .ThresholdCompareOperandsMissing, _) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .ThresholdCompareOperandsMissing,
                (CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .UnsignedBelowBranchTarget, _) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .UnsignedBelowBranchTarget,
                (CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .MissingLateEntryESIForFirstStore, _) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .MissingLateEntryESIForFirstStore,
                (CapacityFinalCalleeCommonZeroSlotStoresLifecycleOutcome
                    .MissingLateEntryEDXForSecondStore, _) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .MissingLateEntryEDXForSecondStore,
                (_, CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .MissingFreshCounterDword,
                (_, CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .DecrementedNonzeroBranchTarget,
                (_, CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero) =>
                    CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                        .DecrementedToZeroBeforeCall,
                _ => throw new InvalidOperationException(
                    "The cleanup lifecycle did not reach a modeled bounded outcome.")
            };

        ImmutableArray<CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect>.Builder effects =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect>();
        foreach (CapacityFinalCalleeCommonZeroSlotThresholdGateRead read
            in stores.ThresholdGate.Reads)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.CompareRawFileOffset,
                CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read,
                read.Address,
                read.AddressCaptured,
                read.ValueCaptured,
                read.Value,
                Attempted: true,
                Committed: false));
        }

        foreach (CapacityFinalCalleeCommonZeroSlotStoreTailWrite write in stores.Writes)
        {
            effects.Add(new(
                write.RawFileOffset,
                CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Write,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value,
                write.Attempted,
                write.Committed));
        }

        foreach (CapacityFinalCalleeCleanupCounterGateCounterAccess access
            in counterGate.CounterAccesses)
        {
            effects.Add(new(
                access.RawFileOffset,
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
                    ? CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Read
                    : CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffectKind.Write,
                access.Address,
                AddressCaptured: true,
                ValueCaptured: access.ValueCaptured,
                Value: access.Value,
                Attempted: true,
                Committed: access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
                    && access.ValueCaptured));
        }

        return new(capture, outcome)
        {
            StoresLifecycle = stores,
            CleanupCounterGate = counterGate,
            CleanupCounterGateEntryReached = cleanupCounterGateEntryReached,
            OrderedMemoryEffects = effects.ToImmutable()
        };
    }
}
