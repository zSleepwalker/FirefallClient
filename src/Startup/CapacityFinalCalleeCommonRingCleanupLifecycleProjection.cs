using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRingCleanupLifecycleCapture(
    uint? FreshGlobalValue,
    uint? ParentEntryEDX,
    uint? FreshEDXPlus8,
    uint? LateEntryESI,
    uint? FreshCounterDword);

public enum CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
{
    MissingFreshGlobalValue,
    MissingParentEntryEDX,
    MissingFreshEDXPlus8,
    FirstUnsignedBelowBypassesChild,
    SecondUnsignedBelowBypassesChild,
    MissingLateEntryESIForFirstWrite,
    ParentHandoffNotExact,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public enum CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
{
    FreshRead,
    RingWrite,
    CleanupCounterRead,
    CleanupCounterWrite
}

public readonly record struct CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect(
    CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted,
    bool Committed);

public readonly record struct CapacityFinalCalleeCommonRingCleanupLifecycleFlags(
    bool? Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record CapacityFinalCalleeCommonRingCleanupLifecyclePlan(
    CapacityFinalCalleeCommonRingCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonRingCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeCleanupCounterGatePlan? CleanupCounterPlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect>.Empty;

    public CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactFallthroughHandoff =>
        ParentPlan.ParentExactFallthroughHandoff;
    public bool ParentRingBoundaryReached =>
        ParentPlan.ChildBoundaryReached;
    public bool ParentRingBoundaryInstructionEntered =>
        ParentPlan.ChildBoundaryInstructionEntered;
    public bool DestinationEntered => ParentPlan.DestinationEntered;
    public bool CleanupCounterGateAdmitted => CleanupCounterPlan is not null;
    public bool CleanupCounterInstructionEntered =>
        CleanupCounterPlan?.CounterReadReached == true;
    public bool LateEntryESIProvidedButUnused =>
        ParentPlan.LateEntryESIProvidedButUnused;
    public bool LateCounterProvidedButUnused =>
        CleanupCounterPlan is null && Capture.FreshCounterDword.HasValue;
    public CapacityFinalCalleeCommonDualPointerThresholdFlags? ParentFlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public CapacityFinalCalleeCleanupCounterGateFlags? CleanupFlagsAfterDecrement =>
        CleanupCounterPlan?.FlagsAfterDecrement;
    public CapacityFinalCalleeCommonRingCleanupLifecycleFlags? FlagsAtStop =>
        CleanupCounterPlan?.FlagsAfterDecrement is
            CapacityFinalCalleeCleanupCounterGateFlags cleanupFlags
            ? new(
                cleanupFlags.Carry,
                cleanupFlags.Parity,
                cleanupFlags.AuxiliaryCarry,
                cleanupFlags.Zero,
                cleanupFlags.Sign,
                cleanupFlags.Overflow)
            : ParentPlan.FlagsAtStop is
                CapacityFinalCalleeCommonDualPointerThresholdFlags parentFlags
                ? new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : null;
    public bool? CarryAtStop => FlagsAtStop?.Carry;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the exact common-ring fallthrough through its five ordered stores and the
/// independent fresh cleanup-counter read, deriving DEC's incoming CF from the parent's
/// second CMP because the intervening ring stores are MOV instructions.
/// </summary>
public static class CapacityFinalCalleeCommonRingCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.EntryRawFileOffset;
    public const uint RingEntryRawFileOffset =
        CapacityFinalCalleeCommonRingInsertionProjection.EntryRawFileOffset;
    public const uint CounterEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint CleanupZeroPathStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupNonzeroPathStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset;

    public static CapacityFinalCalleeCommonRingCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonRingCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan parent =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                capture.FreshGlobalValue,
                capture.ParentEntryEDX,
                capture.FreshEDXPlus8,
                capture.LateEntryESI));
        ImmutableArray<CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect>.Builder effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect>();

        foreach (CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind
                    == CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                        .FreshRead
                    ? CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind.FreshRead
                    : CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind.RingWrite,
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.Attempted,
                effect.Committed));
        }

        if (!IsExactRingBoundaryHandoff(parent))
        {
            return Finish(
                capture,
                parent,
                cleanupCounter: null,
                outcome: MapParentOutcome(parent.Outcome),
                effects: effects,
                stopAt: parent.StopAtRawFileOffsetExclusive,
                exactCleanCompletion: false);
        }

        CapacityFinalCalleeCleanupCounterGatePlan cleanupCounter =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(new(
                EntryValidated: true,
                FreshCounterDword: capture.FreshCounterDword,
                IncomingCarryFlag: parent.ParentPlan.SecondCompareFlags?.Carry));
        foreach (CapacityFinalCalleeCleanupCounterGateCounterAccess access
            in cleanupCounter.CounterAccesses)
        {
            bool isWrite =
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write;
            effects.Add(new(
                isWrite
                    ? CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite
                    : CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead,
                access.RawFileOffset,
                access.Address,
                AddressCaptured: true,
                Value: access.Value,
                ValueCaptured: access.ValueCaptured,
                Attempted: true,
                Committed: isWrite && access.ValueCaptured));
        }

        CapacityFinalCalleeCommonRingCleanupLifecycleOutcome outcome =
            cleanupCounter.Outcome switch
            {
                CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                    CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                        .MissingFreshCounterDword,
                CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                    CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedToZero,
                CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                    CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedNonzero,
                _ => CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.ParentHandoffNotExact
            };
        bool exactCleanCompletion = IsExactCleanupCompletion(parent, cleanupCounter);

        return Finish(
            capture,
            parent,
            cleanupCounter,
            outcome,
            effects,
            cleanupCounter.StopAtRawFileOffsetExclusive,
            exactCleanCompletion);
    }

    public static bool IsExactRingBoundaryHandoff(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .CompletedToBoundaryBeforeDec
        && parent.ExactCleanCompletion
        && parent.ParentExactFallthroughHandoff
        && parent.ParentFallthroughInstructionEntered
        && !parent.DestinationEntered
        && !parent.LateEntryESIProvidedButUnused
        && !parent.ParentPlan.UnreachedFreshGlobalCaptureProvided
        && !parent.ParentPlan.UnreachedEntryEDXCaptureProvided
        && !parent.ParentPlan.UnreachedFreshEDXPlus8CaptureProvided
        && parent.ChildPlan is
        {
            Outcome: CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec,
            ExactCleanCompletion: true,
            BoundaryReached: true,
            BoundaryInstructionEntered: false,
            DestinationEntered: false,
            LateEntryEDXReadReached: true,
            UnreachedLateEntryEDXCaptureProvided: false,
            MovInstructionsPreserveFlags: true,
            StopAtRawFileOffsetExclusive: CounterEntryRawFileOffset
        }
        && parent.Writes.Length == 5
        && parent.Writes.All(write => write.Committed)
        && parent.Writes.Select(write => write.RawFileOffset).SequenceEqual(
            new[]
            {
                CapacityFinalCalleeCommonRingInsertionProjection.FirstWriteRawFileOffset,
                CapacityFinalCalleeCommonRingInsertionProjection.SecondWriteRawFileOffset,
                CapacityFinalCalleeCommonRingInsertionProjection.ThirdWriteRawFileOffset,
                CapacityFinalCalleeCommonRingInsertionProjection.FourthWriteRawFileOffset,
                CapacityFinalCalleeCommonRingInsertionProjection.FifthWriteRawFileOffset
            })
        && parent.ChildBoundaryReached
        && !parent.ChildBoundaryInstructionEntered
        && parent.StopAtRawFileOffsetExclusive == CounterEntryRawFileOffset
        && parent.ParentPlan.SecondCompareFlags.HasValue
        && parent.FlagsAtStop == parent.ParentPlan.SecondCompareFlags;

    public static bool IsExactCleanupCompletion(
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan parent,
        CapacityFinalCalleeCleanupCounterGatePlan cleanupCounter)
    {
        if (!IsExactRingBoundaryHandoff(parent)
            || !cleanupCounter.ExactBoundedGateCompletion
            || !cleanupCounter.CounterReadReached
            || !cleanupCounter.JnzReached
            || !cleanupCounter.CounterWriteReached
            || cleanupCounter.CounterAccesses.Length != 2
            || cleanupCounter.CounterAccesses[0].Kind
                != CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            || cleanupCounter.CounterAccesses[0].RawFileOffset
                != CounterEntryRawFileOffset
            || cleanupCounter.CounterAccesses[0].Address
                != CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
            || !cleanupCounter.CounterAccesses[0].ValueCaptured
            || cleanupCounter.CounterAccesses[0].Value != cleanupCounter.CounterBefore
            || cleanupCounter.CounterAccesses[1].Kind
                != CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            || cleanupCounter.CounterAccesses[1].RawFileOffset
                != CounterEntryRawFileOffset
            || cleanupCounter.CounterAccesses[1].Address
                != CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
            || !cleanupCounter.CounterAccesses[1].ValueCaptured
            || cleanupCounter.CounterAccesses[1].Value != cleanupCounter.CounterAfter
            || cleanupCounter.UnreachedFreshCounterDwordCaptureProvided
            || cleanupCounter.UnreachedIncomingCarryFlagCaptureProvided
            || !parent.ParentPlan.SecondCompareFlags.HasValue
            || cleanupCounter.Capture.IncomingCarryFlag
                != parent.ParentPlan.SecondCompareFlags.Value.Carry)
        {
            return false;
        }

        if (cleanupCounter.Outcome
            == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero)
        {
            return cleanupCounter.JnzTaken == false
                && cleanupCounter.SelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
                && cleanupCounter.MovEcxReached
                && cleanupCounter.EcxValue
                    == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                && !cleanupCounter.CallReached
                && !cleanupCounter.CalleeEntered
                && cleanupCounter.ReachedInstructions.SequenceEqual(
                    new uint[]
                    {
                        CounterEntryRawFileOffset,
                        CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset,
                        CapacityFinalCalleeCleanupCounterGateProjection.MovEcxRawFileOffset
                    })
                && cleanupCounter.StopAtRawFileOffsetExclusive
                    == CleanupZeroPathStopRawFileOffset;
        }

        return cleanupCounter.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            && cleanupCounter.JnzTaken == true
            && cleanupCounter.SelectedDestinationRawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection
                    .NonzeroBranchTargetRawFileOffset
            && !cleanupCounter.MovEcxReached
            && cleanupCounter.EcxValue is null
            && !cleanupCounter.CallReached
            && !cleanupCounter.CalleeEntered
            && cleanupCounter.ReachedInstructions.SequenceEqual(
                new uint[]
                {
                    CounterEntryRawFileOffset,
                    CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset
                })
            && cleanupCounter.StopAtRawFileOffsetExclusive
                == CleanupNonzeroPathStopRawFileOffset;
    }

    private static CapacityFinalCalleeCommonRingCleanupLifecycleOutcome MapParentOutcome(
        CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.MissingFreshGlobalValue,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingParentEntryEDX =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.MissingParentEntryEDX,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome.MissingFreshEDXPlus8 =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.MissingFreshEDXPlus8,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .FirstUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .FirstUnsignedBelowBypassesChild,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .SecondUnsignedBelowBypassesChild =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .SecondUnsignedBelowBypassesChild,
            CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                .MissingLateEntryESIForFirstWrite =>
                CapacityFinalCalleeCommonRingCleanupLifecycleOutcome
                    .MissingLateEntryESIForFirstWrite,
            _ => CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.ParentHandoffNotExact
        };

    private static CapacityFinalCalleeCommonRingCleanupLifecyclePlan Finish(
        CapacityFinalCalleeCommonRingCleanupLifecycleCapture capture,
        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan parent,
        CapacityFinalCalleeCleanupCounterGatePlan? cleanupCounter,
        CapacityFinalCalleeCommonRingCleanupLifecycleOutcome outcome,
        ImmutableArray<CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffect>.Builder
            effects,
        uint stopAt,
        bool exactCleanCompletion) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            CleanupCounterPlan = cleanupCounter,
            OrderedMemoryEffects = effects.ToImmutable(),
            ExactCleanCompletion = exactCleanCompletion,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
