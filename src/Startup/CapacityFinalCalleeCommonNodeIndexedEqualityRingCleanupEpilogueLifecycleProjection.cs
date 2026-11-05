using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleCapture ParentCapture,
        uint? LateChildEsp,
        uint? LateChildEbp,
        bool? CleanupCallReturnedNormally,
        byte? TestedByte,
        uint? StackPopEdiValue,
        uint? StackPopEsiValue,
        uint? StackPopEbxValue,
        uint? CallCellTargetVirtualAddress,
        bool? CallCellReturnedNormally,
        uint? PostCallEbp,
        uint? StackPopEbpValue,
        uint? ReturnTargetVirtualAddress);

public enum
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZeroBeforeChild,
    MissingCleanupCallReturnStatus,
    CleanupCallDidNotReturnNormally,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public enum
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
{
    ParentRead,
    ParentWrite,
    RingWrite,
    CleanupCounterRead,
    CleanupCounterWrite,
    CleanupCall,
    TestByteRead,
    StackPopEdi,
    StackPopEsi,
    StackPopEbx,
    TestBranch,
    PushImmediate,
    CallCellRead,
    OpaqueCallReturnStatus,
    PostCallEbpCapture,
    MovEspFromEbp,
    StackPopEbp,
    ReturnTargetRead,
    ReturnInstruction
}

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        uint? Address,
        uint? Value,
        bool? BooleanValue,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome Outcome,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCleanupCounterGatePlan? ParentCleanupCounterPlan =>
        ParentPlan.CleanupCounterPlan;
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroCleanupCallHandoff =>
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
            .IsExactZeroCleanupCallHandoff(ParentPlan);
    public bool ChildEntryPending =>
        (ParentExactNonzeroEntryHandoff
            || ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true)
        && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome => EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags?
        ParentFlagsAtChildEntry =>
            ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true
                ? UnknownFlags
                : ConvertFlags(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags?
        FlagsAtStop =>
            EpiloguePlan is { CallReached: true, CallReturnedNormally: true }
                ? UnknownFlags
                : EpiloguePlan is { TestedByteCaptured: true, TestFlags: { } testFlags }
                    ? ToLifecycleFlags(testFlags)
                    : CleanupCallReturnedNormally == true
                        ? UnknownFlags
                        : ParentFlagsAtChildEntry;
    public bool ParentFlagsPreservedUntilChildTest =>
        EpiloguePlan?.TestedByteCaptured != true
        && CleanupCallReturnedNormally != true
        && FlagsEqual(FlagsAtStop, ParentFlagsAtChildEntry);
    public bool ChildTestReadReached => EpiloguePlan?.TestReadReached == true;
    public bool Internal9441Entered => ChildTestReadReached;
    public bool EpilogueEntered => ChildEntryTargetEntered;
    public bool CleanupCallReached =>
        ParentExactZeroCleanupCallHandoff && ParentPlan.ZeroPathMovEcxReached;
    public bool CleanupCallAttempted => CleanupCallReached;
    public uint? CleanupCallRawFileOffset =>
        CleanupCallReached
            ? CapacityFinalCalleeCleanupCounterGateProjection.CallRawFileOffset
            : null;
    public uint? CleanupCallTargetRawFileOffset =>
        CleanupCallReached
            ? CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetRawFileOffset
            : null;
    public uint? CleanupCallTargetVirtualAddress =>
        CleanupCallReached
            ? CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetVirtualAddress
            : null;
    public bool? CleanupCallReturnedNormally =>
        CleanupCallReached ? Capture.CleanupCallReturnedNormally : null;
    public bool CleanupCallReturnStatusCaptured =>
        CleanupCallReached && Capture.CleanupCallReturnedNormally.HasValue;
    public bool CleanupCallExecuted => false;
    public bool CleanupHelperEntered => false;
    public bool ChildCallReached => EpiloguePlan?.CallReached == true;
    public bool OpaqueCallTargetCaptured => EpiloguePlan?.CallCellTargetCaptured == true;
    public bool? OpaqueCallReturnedNormally => EpiloguePlan?.CallReturnedNormally;
    public bool OpaqueCallExecuted => false;
    public bool ChildReturnReached => EpiloguePlan?.ReturnReached == true;
    public uint ChildReturnInstructionRawFileOffset =>
        CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset;
    public bool ChildBoundedExitReached =>
        EpiloguePlan is
        {
            ReturnReached: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
        };
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleInstruction>
        ParentOrderedInstructions => ParentPlan.OrderedInstructions;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect>
        OrderedEffects =>
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
                .BuildOrderedEffects(this);
    public int ParentMemoryEffectCount => ParentPlan.OrderedMemoryEffects.Length;
    public int ChildEffectCount => OrderedEffects.Length - ParentMemoryEffectCount;
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        (ParentExactNonzeroEntryHandoff || ParentExactZeroCleanupCallHandoff)
        && ChildTestReadReached
        && OrderedEffects.Length > ParentMemoryEffectCount
        && OrderedEffects.Take(ParentMemoryEffectCount)
            .Select(effect => effect.SourceIndex)
            .SequenceEqual(ParentPlan.OrderedMemoryEffects.Select(effect => effect.SourceIndex));
    public bool NonzeroBranchTargetEntered => false;
    public bool ZeroPathFallthroughEntered => ParentPlan.ZeroPathMovEcxReached;
    public bool ZeroPathCallReached => CleanupCallReached;
    public bool DirectCallReached => CleanupCallReached;
    public bool ZeroPathCalleeEntered => false;
    public bool Outer943CCallEntered => false;
    public bool Outer9441Entered => false;
    public bool CalleeEntered => false;
    public bool DestinationEntered => false;
    public bool LateChildEspProvidedButUnused =>
        !ChildEntryValidated && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        !ChildEntryValidated && Capture.LateChildEbp.HasValue;
    public bool UnreachedCleanupCallReturnStatusProvided =>
        !CleanupCallReached && Capture.CleanupCallReturnedNormally.HasValue;
    public bool UnreachedTestedByteCaptureProvided =>
        EpiloguePlan?.UnreachedTestedByteCaptureProvided ?? Capture.TestedByte.HasValue;
    public bool UnreachedStackPopEdiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEdiCaptureProvided ?? Capture.StackPopEdiValue.HasValue;
    public bool UnreachedStackPopEsiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEsiCaptureProvided ?? Capture.StackPopEsiValue.HasValue;
    public bool UnreachedStackPopEbxCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbxCaptureProvided ?? Capture.StackPopEbxValue.HasValue;
    public bool UnreachedCallCellTargetCaptureProvided =>
        EpiloguePlan?.UnreachedCallCellTargetCaptureProvided
        ?? Capture.CallCellTargetVirtualAddress.HasValue;
    public bool UnreachedCallCellReturnStatusProvided =>
        EpiloguePlan?.UnreachedCallReturnStatusProvided
        ?? Capture.CallCellReturnedNormally.HasValue;
    public bool UnreachedPostCallEbpCaptureProvided =>
        EpiloguePlan?.UnreachedPostCallEbpCaptureProvided ?? Capture.PostCallEbp.HasValue;
    public bool UnreachedStackPopEbpCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbpCaptureProvided ?? Capture.StackPopEbpValue.HasValue;
    public bool UnreachedReturnTargetCaptureProvided =>
        EpiloguePlan?.UnreachedReturnTargetCaptureProvided
        ?? Capture.ReturnTargetVirtualAddress.HasValue;
    public bool ExactBoundedCompletion =>
        ParentPlan.ExactBoundedCompletion
        && (ParentExactNonzeroEntryHandoff
            ? ChildBoundedExitReached
            : ParentExactZeroCleanupCallHandoff
                ? CleanupCallReturnedNormally == true
                    ? ChildBoundedExitReached
                    : StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeCleanupCounterGateProjection
                            .BoundaryRawFileOffsetExclusive
                : StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);
    public bool ExactBoundedExitAdmitted => ExactBoundedCompletion;
    public bool ExactCleanCompletion =>
        ParentPlan.ExactCleanCompletion
        && !AnyUnreachedChildCaptureProvided
        && !(CleanupCallReached && CleanupCallReturnedNormally != true)
        && (ParentExactNonzeroEntryHandoff
            || ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true
                ? ChildExactCleanExitAdmitted
                : !ParentExactNonzeroEntryHandoff && !ParentExactZeroCleanupCallHandoff);

    private bool AnyUnreachedChildCaptureProvided =>
        LateChildEspProvidedButUnused
        || LateChildEbpProvidedButUnused
        || UnreachedCleanupCallReturnStatusProvided
        || UnreachedTestedByteCaptureProvided
        || UnreachedStackPopEdiCaptureProvided
        || UnreachedStackPopEsiCaptureProvided
        || UnreachedStackPopEbxCaptureProvided
        || UnreachedCallCellTargetCaptureProvided
        || UnreachedCallCellReturnStatusProvided
        || UnreachedPostCallEbpCaptureProvided
        || UnreachedStackPopEbpCaptureProvided
        || UnreachedReturnTargetCaptureProvided;

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags?
        UnknownFlags => new(null, null, null, null, null, null);

    private static bool FlagsEqual(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags? left,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags? right) =>
        left is { } leftFlags && right is { } rightFlags && leftFlags == rightFlags;

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeGatedEpilogueTestFlags flags) =>
            new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign, flags.Overflow);
}

/// <summary>
/// Joins the exact five-store equality-ring cleanup-counter handoff to the shared 9441 epilogue.
/// Neither the direct helper nor the indirect cell target is executed.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset = CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterCallRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.CallRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint CleanupCallTargetRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetRawFileOffset;
    public const uint CleanupCallTargetVirtualAddress =
        CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetVirtualAddress;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection.Project(
                capture.ParentCapture);
        bool nonzeroHandoff = IsExactNonzeroEntryHandoff(parent);
        bool zeroHandoff = IsExactZeroCleanupCallHandoff(parent);
        bool cleanupCallReached = zeroHandoff && parent.ZeroPathMovEcxReached;

        if (cleanupCallReached && capture.CleanupCallReturnedNormally != true)
        {
            return Finish(
                capture,
                parent,
                null,
                capture.CleanupCallReturnedNormally is null
                    ? CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                        .MissingCleanupCallReturnStatus
                    : CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                        .CleanupCallDidNotReturnNormally,
                ZeroCounterCallRawFileOffset);
        }

        if (!nonzeroHandoff && !zeroHandoff)
        {
            return Finish(
                capture,
                parent,
                null,
                parent.Outcome
                    == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                        .DecrementedToZero
                    ? CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                        .ParentDecrementedToZeroBeforeChild
                    : CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                        .ParentNotAdmittedToChild,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (capture.LateChildEsp is not uint childEsp
            || capture.LateChildEbp is not uint childEbp)
        {
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome pending =
                capture.LateChildEsp is null && capture.LateChildEbp is null
                    ? CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                        .MissingLateChildEspAndEbp
                    : capture.LateChildEsp is null
                        ? CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEsp
                        : CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEbp;
            return Finish(capture, parent, null, pending, ChildEntryRawFileOffset);
        }

        CapacityFinalCalleeGatedEpiloguePlan epilogue =
            CapacityFinalCalleeGatedEpilogueProjection.Project(new(
                EntryValidated: true,
                EntryEsp: childEsp,
                EntryEbp: childEbp,
                TestedByte: capture.TestedByte,
                StackPopEdiValue: capture.StackPopEdiValue,
                StackPopEsiValue: capture.StackPopEsiValue,
                StackPopEbxValue: capture.StackPopEbxValue,
                CallCellTargetVirtualAddress: capture.CallCellTargetVirtualAddress,
                CallReturnedNormally: capture.CallCellReturnedNormally,
                PostCallEbp: capture.PostCallEbp,
                StackPopEbpValue: capture.StackPopEbpValue,
                ReturnTargetVirtualAddress: capture.ReturnTargetVirtualAddress));
        return Finish(
            capture,
            parent,
            epilogue,
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome
                .ChildProjectionStarted,
            epilogue.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                    .DecrementedNonzero
            && gate is
            {
                Capture.EntryValidated: true,
                Outcome: CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero,
                CounterReadReached: true,
                FreshCounterDwordCaptured: true,
                CounterWriteReached: true,
                JnzReached: true,
                JnzTaken: true,
                SelectedDestinationRawFileOffset: ChildEntryRawFileOffset,
                StopAtRawFileOffsetExclusive: ChildEntryRawFileOffset,
                MovEcxReached: false,
                CallReached: false,
                CalleeEntered: false,
                CounterAccesses.Length: 2
            }
            && gate.Capture.FreshCounterDword is uint before
            && gate.CounterBefore == before
            && gate.CounterAfter == unchecked(before - 1)
            && gate.CounterAfter != 0
            && parent.CounterJnzTaken == true
            && parent.CounterJnzDestinationRawFileOffset == ChildEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset;
    }

    public static bool IsExactZeroCleanupCallHandoff(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleOutcome
                    .DecrementedToZero
            && gate is
            {
                Capture.EntryValidated: true,
                Outcome: CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero,
                CounterReadReached: true,
                FreshCounterDwordCaptured: true,
                CounterWriteReached: true,
                JnzReached: true,
                JnzTaken: false,
                SelectedDestinationRawFileOffset:
                    CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset,
                StopAtRawFileOffsetExclusive: ZeroCounterStopRawFileOffset,
                MovEcxReached: true,
                EcxValue: CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue,
                CallReached: false,
                CalleeEntered: false,
                CounterAccesses.Length: 2
            }
            && gate.Capture.FreshCounterDword == 1
            && gate.CounterBefore == 1
            && gate.CounterAfter == 0
            && parent.CounterJnzTaken == false
            && parent.CounterJnzDestinationRawFileOffset
                == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == ZeroCounterStopRawFileOffset;
    }

    private static bool IsExactCompletedParent(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan parent) =>
        parent.ExactBoundedCompletion
        && parent.ExactCleanCompletion
        && parent.ExactParentStoreCompletion
        && parent.ExactCleanupCounterCompletion
        && parent.CleanupCounterGateEntered
        && parent.CounterReadAttempted
        && parent.CounterValueCaptured
        && parent.CounterWriteCommitted
        && parent.CleanupCounterPlan is
        {
            CounterAccesses.Length: 2,
            CounterReadReached: true,
            FreshCounterDwordCaptured: true,
            CounterWriteReached: true,
            JnzReached: true,
            Capture.FreshCounterDword: not null
        }
        && parent.ParentPlan.ThresholdRingPlan?.Writes is { Length: 5 } writes
        && writes.All(write => write.Attempted && write.Committed)
        && parent.OrderedMemoryEffects.Length > 0
        && parent.OrderedMemoryEffects.All(effect => effect.Attempted);

    internal static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect>
        BuildOrderedEffects(
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan plan)
    {
        var effects = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffect effect
            in plan.ParentPlan.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .RingWriteAttempt =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                            .RingWrite,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterWrite,
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                            .ParentWrite,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                            .ParentRead
                },
                effect.SourceIndex,
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                null,
                effect.Attempted,
                effect.ValueCaptured,
                effect.Committed));
        }

        if (plan.CleanupCallReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CleanupCall,
                -1,
                ZeroCounterCallRawFileOffset,
                CleanupCallTargetVirtualAddress,
                CleanupCallTargetRawFileOffset,
                plan.Capture.CleanupCallReturnedNormally,
                Attempted: true,
                ValueCaptured: true,
                Committed: false));
        }

        if (plan.EpiloguePlan is not { } child)
        {
            return effects.ToImmutable();
        }

        if (child.TestReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                true,
                child.TestedByteCaptured));
        }
        AddStackPop(effects, child.StackPopEdiReached,
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress, child.PoppedEdi);
        AddStackPop(effects, child.StackPopEsiReached,
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress, child.PoppedEsi);
        AddStackPop(effects, child.StackPopEbxReached,
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress, child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .TestBranch,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.BranchRawFileOffset,
                null,
                null,
                child.BranchTaken,
                true,
                child.BranchTaken.HasValue,
                false));
        }
        if (child.PushImmediateReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .PushImmediate,
                CapacityFinalCalleeGatedEpilogueProjection.PushImmediateRawFileOffset,
                child.PushImmediateAddress,
                child.PushedImmediateVirtualAddress,
                true,
                child.PushedImmediateVirtualAddress.HasValue,
                true));
        }
        if (child.CallCellReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .CallCellRead,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.CallCellVirtualAddress,
                child.CallCellTargetVirtualAddress,
                true,
                child.CallCellTargetCaptured));
        }
        if (child.CallReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .OpaqueCallReturnStatus,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                null,
                null,
                child.Capture.CallReturnedNormally,
                true,
                child.Capture.CallReturnedNormally.HasValue,
                false));
        }
        if (child.PostCallEbpCaptureReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .PostCallEbpCapture,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.PostCallEbp,
                true,
                child.PostCallEbpCaptured));
        }
        if (child.MovEspFromEbpReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                true,
                child.MovEspSourceEbp.HasValue));
        }
        AddStackPop(effects, child.StackPopEbpReached,
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress, child.PoppedEbp);
        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .ReturnTargetRead,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                true,
                child.ReturnTargetCaptured));
        }
        if (child.ReturnReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind
                    .ReturnInstruction,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                true,
                child.ReturnTargetCaptured));
        }
        return effects.ToImmutable();
    }

    private static void AddStackPop(
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect>.Builder
            effects,
        bool reached,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value)
    {
        if (reached)
        {
            effects.Add(ChildEffect(kind, rawFileOffset, address, value, true, value.HasValue));
        }
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffect
        ChildEffect(
            CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleEffectKind kind,
            uint rawFileOffset,
            uint? address,
            uint? value,
            bool attempted,
            bool valueCaptured,
            bool committed = false) =>
        new(kind, -1, rawFileOffset, address, value, null, attempted, valueCaptured, committed);

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
