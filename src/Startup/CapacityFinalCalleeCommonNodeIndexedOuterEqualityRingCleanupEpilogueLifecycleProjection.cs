using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleCapture ParentCapture,
        bool? CleanupCallReturnedNormally,
        uint? LateChildEsp,
        uint? LateChildEbp,
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
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
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
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? Auxiliary,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public enum
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        uint? Address,
        uint? Value,
        bool? BooleanValue,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome Outcome,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCleanupCounterGatePlan? ParentCleanupCounterPlan =>
        ParentPlan.CleanupCounterPlan;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroCleanupCallHandoff =>
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
            .IsExactZeroCleanupCallHandoff(ParentPlan);
    public bool ChildEntryPending =>
        (ParentExactNonzeroEntryHandoff
            || ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true)
        && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome => EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags?
        ParentFlagsAtChildEntry =>
            ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true
                ? UnknownFlags
                : ConvertFlags(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags?
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleInstruction>
        ParentOrderedInstructions => ParentPlan.OrderedInstructions;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect>
        OrderedEffects =>
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
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
                ? CleanupCallReturnedNormally == true && ChildBoundedExitReached
                : StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);
    public bool ParentBoundedExitAt943C =>
        ParentPlan.ExactBoundedCompletion
        && ParentPlan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
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

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags?
        UnknownFlags => new(null, null, null, null, null, null);

    private static bool FlagsEqual(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags? left,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags? right) =>
        left is { } leftFlags && right is { } rightFlags && leftFlags == rightFlags;

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleFlags? flags) =>
            flags is { } value
                ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
                : null;

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeGatedEpilogueTestFlags flags) =>
            new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign, flags.Overflow);
}

/// <summary>
/// Joins the exact outer-equality five-store cleanup-counter handoff to the shared 9441 epilogue.
/// Neither the direct helper nor the indirect cell target is executed.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
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

    public static
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan Project(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection.Project(
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
                    ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                        .MissingCleanupCallReturnStatus
                    : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
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
                    == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
                        .DecrementedToZero
                    ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                        .ParentDecrementedToZeroBeforeChild
                    : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                        .ParentNotAdmittedToChild,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (capture.LateChildEsp is not uint childEsp
            || capture.LateChildEbp is not uint childEbp)
        {
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome pending =
                capture.LateChildEsp is null && capture.LateChildEbp is null
                    ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                        .MissingLateChildEspAndEbp
                    : capture.LateChildEsp is null
                        ? CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEsp
                        : CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
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
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome
                .ChildProjectionStarted,
            epilogue.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleOutcome
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
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan parent) =>
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
        && parent.OrderedMemoryEffects.Length > 0
        && parent.OrderedMemoryEffects.All(effect => effect.Attempted);

    internal static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect>
        BuildOrderedEffects(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan plan)
    {
        var effects = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect>();
        foreach (CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffect effect
            in plan.ParentPlan.OrderedMemoryEffects)
        {
            effects.Add(new(
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .RingWriteAttempt =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                            .RingWrite,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterWrite,
                    CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                            .ParentWrite,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                true,
                child.TestedByteCaptured));
        }
        AddStackPop(effects, child.StackPopEdiReached,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress, child.PoppedEdi);
        AddStackPop(effects, child.StackPopEsiReached,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress, child.PoppedEsi);
        AddStackPop(effects, child.StackPopEbxReached,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress, child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                    .MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                true,
                child.MovEspSourceEbp.HasValue));
        }
        AddStackPop(effects, child.StackPopEbpReached,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
                .StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress, child.PoppedEbp);
        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind
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
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect>.Builder
            effects,
        bool reached,
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value)
    {
        if (reached)
        {
            effects.Add(ChildEffect(kind, rawFileOffset, address, value, true, value.HasValue));
        }
    }

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffect
        ChildEffect(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleEffectKind kind,
            uint rawFileOffset,
            uint? address,
            uint? value,
            bool attempted,
            bool valueCaptured,
            bool committed = false) =>
        new(kind, -1, rawFileOffset, address, value, null, attempted, valueCaptured, committed);

    private static
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecyclePlan Finish(
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleCapture capture,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecyclePlan parent,
            CapacityFinalCalleeGatedEpiloguePlan? epilogue,
            CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleOutcome outcome,
            uint stopAt) =>
        new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
