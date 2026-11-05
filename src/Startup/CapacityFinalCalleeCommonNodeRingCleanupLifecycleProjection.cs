using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeRingCleanupLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP,
    uint? EntryESI,
    uint? LateFreshGlobalValue,
    uint? LateFreshCleanupCounterDword);

public enum CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
{
    MissingEntryEDIAt9354,
    MissingLateEntryEBPAt9374,
    MissingEntryESIAt9384,
    ParentNotCleanAt93A1,
    MissingFreshGlobalValueAt93A1,
    NonzeroTestResultBypassesCleanupAt93C0,
    InvalidParentHandoff,
    InvalidNodeHandoff,
    InvalidFreshMaskHandoff,
    InvalidZeroChildHandoff,
    MissingBothLatePointers,
    MissingLateEBX,
    MissingLateESI,
    InvalidInternal942FEntry,
    MissingFreshCounterDwordAt942F,
    CounterDecrementedToZeroBeforeCall943C,
    CounterDecrementedNonzeroTo9441
}

public enum CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
{
    ParentFrameWrite,
    ParentNodeWrite,
    IndependentFreshGlobalRead,
    ChildGlobalWrite,
    ChildPointerStore,
    CleanupCounterRead,
    CleanupCounterWrite
}

public readonly record struct CapacityFinalCalleeCommonNodeRingCleanupLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct
    CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffect(
        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind Kind,
        uint RawFileOffset,
        uint Address,
        uint? Value,
        bool ValueCaptured,
        bool Committed,
        bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan(
    CapacityFinalCalleeCommonNodeRingCleanupLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan ParentChild { get; init; } =
        null!;
    public CapacityFinalCalleeCleanupCounterGatePlan CounterGate { get; init; } = null!;
    public ImmutableArray<CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffect>
                .Empty;

    public bool ExactParentZeroChildHandoff => ParentChild.ExactZeroChildHandoff;
    public bool ExactParentNonzeroBypass =>
        ParentChild.ExactNonzeroParentBypass
        && !LateFreshCleanupCounterDwordProvidedButUnused
        && CounterGate.Outcome
            == CapacityFinalCalleeCleanupCounterGateOutcome.EntryNotValidated
        && CounterGate.CounterAccesses.IsEmpty
        && !CounterGate.CounterReadReached
        && !CounterGate.CounterWriteReached;
    public bool ChildReached => ParentChild.ChildReached;
    public bool ChildORReached => ParentChild.ChildORReached;
    public bool ExactFiveChildWrites =>
        ParentChild.Child is CapacityFinalCalleeCommonSelfRingTailPlan child
        && child.Writes.Length == 5
        && child.ExactBoundedExitAdmitted
        && child.Writes.Select(write => write.RawFileOffset).SequenceEqual(
            new[]
            {
                CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FirstLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.SecondLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.ThirdLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FourthLocalStoreRawFileOffset
            });
    public bool ExactParentWrites => ParentChild.ExactParentWrites;
    public bool ExactParentChildCompletion => ParentChild.ExactChildCompletion;
    public bool Internal942FEntryReached =>
        CounterGate.Capture.EntryValidated && ExactCounterGateAdmission;
    public bool ExactCounterGateAdmission =>
        ParentChild.Outcome
            == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
        && ParentChild.ExactZeroChildHandoff
        && ParentChild.ExactChildCompletion
        && ParentChild.ExactParentWrites
        && ParentChild.ExactOrderedMemoryEffects
        && ExactFiveChildWrites
        && ParentChild.ChildORReached
        && ParentChild.ChildORFlags is
            CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags { Carry: false }
        && CounterGate.Capture.IncomingCarryFlag == false
        && ParentChild.ChildJumpReached
        && !ParentChild.ChildJumpTargetEntered
        && ParentChild.ChildJumpTargetRawFileOffset
            == CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset
        && ParentChild.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset
        && CounterGate.Capture.EntryValidated;
    public bool CleanupCounterReadAttempted => CounterGate.CounterReadReached;
    public bool CleanupCounterValueCaptured => CounterGate.FreshCounterDwordCaptured;
    public bool CleanupCounterWriteReached => CounterGate.CounterWriteReached;
    public bool CleanupCounterDecrementExecuted => CounterGate.CounterWriteReached;
    public bool CleanupCounterWriteCommitted =>
        CounterGate.CounterWriteReached
        && CounterGate.CounterAccesses.Any(access =>
            access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            && access.ValueCaptured);
    public bool LateFreshCleanupCounterDwordProvidedButUnused =>
        !CounterGate.CounterReadReached && Capture.LateFreshCleanupCounterDword.HasValue;
    public bool UnreachedLateFreshCleanupCounterDwordCaptureProvided =>
        CounterGate.UnreachedFreshCounterDwordCaptureProvided;
    public uint? CounterBefore => CounterGate.CounterBefore;
    public uint? CounterAfter => CounterGate.CounterAfter;
    public bool? JnzTaken => CounterGate.JnzTaken;
    public uint? SelectedDestinationRawFileOffset =>
        CounterGate.SelectedDestinationRawFileOffset
        ?? ParentChild.Parent.FreshMaskGate?.JnzSelectedDestinationRawFileOffset;
    public bool FollowingCallReached => CounterGate.CallReached;
    public bool CalleeEntered => CounterGate.CalleeEntered;
    public bool DestinationEntered => false;
    public bool CleanupJnzTargetEntered => false;
    public bool IncomingCarryDerivedFromChildOR =>
        ChildORReached
        && ParentChild.ChildORFlags
            is CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags childFlags
        && childFlags.Carry.HasValue
        && CounterGate.Capture.IncomingCarryFlag == childFlags.Carry;
    public bool? DerivedIncomingCarryFlag => ParentChild.ChildORFlags?.Carry;
    public CapacityFinalCalleeCommonNodeRingCleanupLifecycleFlags? ParentORFlags =>
        ParentChild.ChildORFlags is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null;
    public CapacityFinalCalleeCommonNodeRingCleanupLifecycleFlags? FlagsAtStop =>
        CounterGate.FlagsAfterDecrement is { } counterFlags
            ? new(
                counterFlags.Carry,
                counterFlags.Parity,
                counterFlags.AuxiliaryCarry,
                counterFlags.Zero,
                counterFlags.Sign,
                counterFlags.Overflow)
            : ParentChild.FlagsAtStop is { } parentFlags
                ? new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : null;
    public bool ParentORFlagsPreservedWhenCounterUnavailable =>
        CounterGate.Outcome
            == CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword
        && ParentORFlags.HasValue
        && FlagsAtStop == ParentORFlags
        && ParentChild.ExactChildCompletion
        && ParentChild.ExactOrderedMemoryEffects;
    public bool FreshCleanupCounterForwardedFromPriorWrite =>
        OrderedMemoryEffects.Any(effect =>
            effect.Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead
            && effect.ForwardedFromPriorWrite);
    public bool CounterReadAfterParentMemoryEffects =>
        CounterGate.CounterReadReached
        && ParentChild.OrderedMemoryEffects.Length
            <= OrderedMemoryEffects.Length - CounterGate.CounterAccesses.Length
        && OrderedMemoryEffects
            .Skip(ParentChild.OrderedMemoryEffects.Length)
            .Take(CounterGate.CounterAccesses.Length)
            .Select(effect => effect.Kind)
            .SequenceEqual(CounterGate.CounterAccesses.Select(access =>
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
                    ? CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead
                    : CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite));
    public bool ExactOrderedMemoryEffects =>
        ParentChild.ExactOrderedMemoryEffects
        && OrderedMemoryEffects.SequenceEqual(
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
                .BuildOrderedMemoryEffects(ParentChild, CounterGate))
        && OrderedMemoryEffects.All(effect => !effect.ForwardedFromPriorWrite);
    public bool ExactCleanupCounterGateCompletion => IsExactCleanupCounterGateCompletion();
    public bool ExactCleanExitAdmitted =>
        ExactParentNonzeroBypass && !LateFreshCleanupCounterDwordProvidedButUnused
        || ExactCleanupCounterGateCompletion;
    public bool ExactBoundedExitAdmitted =>
        ExactParentNonzeroBypass || ExactCleanupCounterGateCompletion;
    public uint StopAtRawFileOffsetExclusive =>
        CounterGate.Capture.EntryValidated
            ? CounterGate.StopAtRawFileOffsetExclusive
            : ParentChild.StopAtRawFileOffsetExclusive;

    private bool IsExactCleanupCounterGateCompletion()
    {
        ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess> accesses =
            CounterGate.CounterAccesses;
        if (!ExactCounterGateAdmission
            || Outcome is not
                (CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedToZeroBeforeCall943C
                or CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzeroTo9441)
            || !CounterGate.ExactBoundedGateCompletion
            || !CounterGate.CounterReadReached
            || !CounterGate.FreshCounterDwordCaptured
            || !CounterGate.CounterWriteReached
            || !CounterGate.JnzReached
            || CounterGate.CallReached
            || CounterGate.CalleeEntered
            || DestinationEntered
            || CleanupJnzTargetEntered
            || LateFreshCleanupCounterDwordProvidedButUnused
            || CounterGate.UnreachedFreshCounterDwordCaptureProvided
            || CounterGate.UnreachedIncomingCarryFlagCaptureProvided
            || CounterGate.Capture.IncomingCarryFlag != false
            || !IncomingCarryDerivedFromChildOR
            || !CounterReadAfterParentMemoryEffects
            || !ExactOrderedMemoryEffects
            || accesses.Length != 2
            || accesses[0].RawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset
            || accesses[0].Kind != CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            || accesses[0].Address
                != CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
            || !accesses[0].ValueCaptured
            || accesses[0].Value != CounterGate.CounterBefore
            || accesses[1].RawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset
            || accesses[1].Kind != CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            || accesses[1].Address
                != CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
            || !accesses[1].ValueCaptured
            || accesses[1].Value != CounterGate.CounterAfter
            || CounterGate.ReachedInstructions.SequenceEqual(
                ExpectedReachedInstructions()) == false)
        {
            return false;
        }

        return CounterGate.Outcome switch
        {
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                Outcome
                    == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .CounterDecrementedNonzeroTo9441
                && CounterGate.JnzTaken == true
                && CounterGate.SelectedDestinationRawFileOffset == 0x012C9441
                && CounterGate.StopAtRawFileOffsetExclusive == 0x012C9441
                && !CounterGate.MovEcxReached
                && CounterGate.EcxValue is null
                && !DestinationEntered,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                Outcome
                    == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .CounterDecrementedToZeroBeforeCall943C
                && CounterGate.JnzTaken == false
                && CounterGate.SelectedDestinationRawFileOffset == 0x012C9437
                && CounterGate.StopAtRawFileOffsetExclusive == 0x012C943C
                && CounterGate.MovEcxReached
                && CounterGate.EcxValue
                    == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                && !DestinationEntered,
            _ => false
        };
    }

    private uint[] ExpectedReachedInstructions() =>
        CounterGate.JnzTaken == true
            ? new uint[]
            {
                CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset
            }
            : new uint[]
            {
                CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateProjection.MovEcxRawFileOffset
            };
}

/// <summary>
/// Composes the exact common-node self-ring child with the independent cleanup-counter gate.
/// </summary>
public static class CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.EntryRawFileOffset;
    public const uint InternalCleanupEntryRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CounterVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress;

    public static CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeRingCleanupLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan parentChild =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(
                    capture.EntryEDI,
                    capture.LateEntryEBP,
                    capture.EntryESI,
                    capture.LateFreshGlobalValue));
        bool exactInternalEntry =
            parentChild.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
            && parentChild.ExactZeroChildHandoff
            && parentChild.ExactChildCompletion
            && parentChild.ExactParentWrites
            && parentChild.ExactOrderedMemoryEffects
            && parentChild.ChildORReached
            && parentChild.ChildORFlags is { Carry: false }
            && parentChild.Child?.Writes.Length == 5
            && parentChild.ChildJumpReached
            && !parentChild.ChildJumpTargetEntered
            && parentChild.ChildJumpTargetRawFileOffset == InternalCleanupEntryRawFileOffset
            && parentChild.StopAtRawFileOffsetExclusive == InternalCleanupEntryRawFileOffset;

        CapacityFinalCalleeCleanupCounterGatePlan counterGate =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: exactInternalEntry,
                    FreshCounterDword: capture.LateFreshCleanupCounterDword,
                    IncomingCarryFlag: parentChild.ChildORFlags?.Carry));
        CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome outcome =
            MapOutcome(parentChild, counterGate, exactInternalEntry);
        return new(capture, outcome)
        {
            ParentChild = parentChild,
            CounterGate = counterGate,
            OrderedMemoryEffects = BuildOrderedMemoryEffects(parentChild, counterGate)
        };
    }

    internal static ImmutableArray<CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffect>
        BuildOrderedMemoryEffects(
            CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan parentChild,
            CapacityFinalCalleeCleanupCounterGatePlan counterGate)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect effect
            in parentChild.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentFrameWrite =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                            .ParentFrameWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentNodeWrite =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                            .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .IndependentFreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                            .IndependentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildGlobalWrite =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                            .ChildGlobalWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                            .ChildPointerStore,
                    _ => throw new ArgumentOutOfRangeException(nameof(parentChild))
                },
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                effect.ValueCaptured,
                effect.Committed,
                effect.ForwardedFromPriorWrite));
        }

        foreach (CapacityFinalCalleeCleanupCounterGateCounterAccess access
            in counterGate.CounterAccesses)
        {
            effects.Add(new(
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
                    ? CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead
                    : CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite,
                access.RawFileOffset,
                access.Address,
                access.Value,
                access.ValueCaptured,
                access.Kind == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
                    && access.ValueCaptured,
                ForwardedFromPriorWrite: false));
        }

        return effects.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome MapOutcome(
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan parentChild,
        CapacityFinalCalleeCleanupCounterGatePlan counterGate,
        bool exactInternalEntry)
    {
        if (!exactInternalEntry)
        {
            return parentChild.Outcome switch
            {
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryEDIAt9354 =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .MissingEntryEDIAt9354,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateEntryEBPAt9374 =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .MissingLateEntryEBPAt9374,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryESIAt9384 =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .MissingEntryESIAt9384,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.ParentNotCleanAt93A1 =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .ParentNotCleanAt93A1,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .MissingFreshGlobalValueAt93A1 =>
                        CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                            .MissingFreshGlobalValueAt93A1,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .NonzeroTestResultBypassesChild =>
                        parentChild.ExactNonzeroParentBypass
                            ? CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                                .NonzeroTestResultBypassesCleanupAt93C0
                            : CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                                .InvalidFreshMaskHandoff,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidParentHandoff =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .InvalidParentHandoff,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidNodeHandoff =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .InvalidNodeHandoff,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidFreshMaskHandoff =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .InvalidFreshMaskHandoff,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidZeroChildHandoff =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .InvalidZeroChildHandoff,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingBothLatePointers =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .MissingBothLatePointers,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateEBX =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingLateEBX,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateESI =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingLateESI,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed =>
                    CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                        .InvalidInternal942FEntry,
                _ => CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .InvalidParentHandoff
            };
        }

        return counterGate.Outcome switch
        {
            CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword =>
                CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .MissingFreshCounterDwordAt942F,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero =>
                CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedToZeroBeforeCall943C,
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero =>
                CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzeroTo9441,
            _ => CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                .InvalidInternal942FEntry
        };
    }
}
