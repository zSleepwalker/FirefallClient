using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct
    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleCapture(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleCapture ParentCapture,
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

public enum CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
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
    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags(
        bool? Carry,
        bool? Parity,
        bool? AuxiliaryCarry,
        bool? Zero,
        bool? Sign,
        bool? Overflow);

public enum CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
{
    ParentRead,
    ParentStore,
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
    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind Kind,
        int SourceIndex,
        uint? RawFileOffset,
        uint? Address,
        uint? Value,
        bool? BooleanValue,
        bool Attempted,
        bool ValueCaptured,
        bool Committed);

public sealed record
    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleCapture Capture,
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome Outcome,
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCleanupCounterGatePlan? ParentCleanupCounterPlan =>
        ParentPlan.CleanupCounterPlan;
    public CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
        ParentOutcome => ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroCleanupCallHandoff =>
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
            .IsExactZeroCleanupCallHandoff(ParentPlan);
    public bool ChildEntryPending =>
        (ParentExactNonzeroEntryHandoff
            || ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true)
        && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags?
        ParentFlagsAtChildEntry =>
            ParentExactZeroCleanupCallHandoff && CleanupCallReturnedNormally == true
                ? UnknownFlags
                : ParentPlan.FlagsAtStop is { } flags
                    ? ToLifecycleFlags(flags)
                    : null;
    public CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags? FlagsAtStop =>
        EpiloguePlan is { CallReached: true, CallReturnedNormally: true }
            ? UnknownFlags
            : EpiloguePlan is
            {
                TestedByteCaptured: true,
                TestFlags: { } testFlags
            }
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
    public bool DestinationEntered => false;
    public bool CleanupCallReached =>
        ParentExactZeroCleanupCallHandoff
        && ParentCleanupCounterPlan?.MovEcxReached == true;
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
    public bool ParentCleanupCallReached =>
        ParentCleanupCounterPlan?.CallReached == true;
    public bool ParentCleanupMovEcxReached =>
        ParentCleanupCounterPlan?.MovEcxReached == true;
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
            == CapacityFinalCalleeGatedEpilogueProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleInstruction>
        ParentOrderedInstructions => ParentPlan.OrderedInstructions;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public int ParentMemoryEffectCount => ParentPlan.OrderedMemoryEffects.Length;
    public int ChildEffectCount => OrderedEffects.Length - ParentMemoryEffectCount;
    public ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect> OrderedEffects =>
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
                .BuildOrderedEffects(this);
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        (ParentExactNonzeroEntryHandoff || ParentExactZeroCleanupCallHandoff)
        && ChildTestReadReached
        && ParentOrderedMemoryEffects.Length
            == ParentMemoryEffectCount
        && OrderedEffects.Length > ParentMemoryEffectCount
        && OrderedEffects.Take(ParentMemoryEffectCount)
            .Select(effect => effect.Kind)
            .SequenceEqual(ParentEffectKinds(ParentOrderedMemoryEffects));
    public bool NonzeroBranchTargetEntered => false;
    public bool ZeroPathFallthroughEntered => ParentCleanupMovEcxReached;
    public bool DirectCallReached => CleanupCallReached;
    public bool ZeroPathCallReached => CleanupCallReached;
    public bool Outer943CCallEntered => false;
    public bool Outer9441Entered => false;
    public bool CalleeEntered => false;
    public bool LateChildEspProvidedButUnused =>
        !ChildEntryValidated && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        !ChildEntryValidated && Capture.LateChildEbp.HasValue;
    public bool UnreachedCleanupCallReturnStatusProvided =>
        !CleanupCallReached && Capture.CleanupCallReturnedNormally.HasValue;
    public bool UnreachedTestedByteCaptureProvided =>
        EpiloguePlan?.UnreachedTestedByteCaptureProvided
        ?? Capture.TestedByte.HasValue;
    public bool UnreachedStackPopEdiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEdiCaptureProvided
        ?? Capture.StackPopEdiValue.HasValue;
    public bool UnreachedStackPopEsiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEsiCaptureProvided
        ?? Capture.StackPopEsiValue.HasValue;
    public bool UnreachedStackPopEbxCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbxCaptureProvided
        ?? Capture.StackPopEbxValue.HasValue;
    public bool UnreachedCallCellTargetCaptureProvided =>
        EpiloguePlan?.UnreachedCallCellTargetCaptureProvided
        ?? Capture.CallCellTargetVirtualAddress.HasValue;
    public bool UnreachedCallCellReturnStatusProvided =>
        EpiloguePlan?.UnreachedCallReturnStatusProvided
        ?? Capture.CallCellReturnedNormally.HasValue;
    public bool UnreachedPostCallEbpCaptureProvided =>
        EpiloguePlan?.UnreachedPostCallEbpCaptureProvided
        ?? Capture.PostCallEbp.HasValue;
    public bool UnreachedStackPopEbpCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbpCaptureProvided
        ?? Capture.StackPopEbpValue.HasValue;
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
                        == CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
                            .ZeroCounterCallRawFileOffset
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

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags?
        UnknownFlags => new(null, null, null, null, null, null);

    private static bool FlagsEqual(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags? left,
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags? right) =>
        left is { } leftFlags
            && right is { } rightFlags
            && leftFlags == rightFlags;

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleFlags
        ToLifecycleFlags(CapacityFinalCalleeGatedEpilogueTestFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign, flags.Overflow);

    private static ImmutableArray<CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind>
        ParentEffectKinds(
            ImmutableArray<
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect> effects) =>
            effects.Select(effect => effect.Kind switch
            {
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentStore =>
                    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead =>
                    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                        .CleanupCounterRead,
                CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterWrite =>
                    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                        .CleanupCounterWrite,
                _ =>
                    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.ParentRead
            }).ToImmutableArray();
}

/// <summary>
/// Joins the node-indexed cleanup DEC/JNZ handoff, zero-path helper call, and 9441 epilogue.
/// The direct helper target is fixed by its immediate operand; opaque calls are not executed.
/// </summary>
public static class
    CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleProjection.EntryRawFileOffset;
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

    private static readonly ImmutableArray<uint> NonzeroParentTailInstructionOffsets =
        ImmutableArray.Create(
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.FirstStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.SecondStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.ThirdStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.FourthStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset);

    private static readonly ImmutableArray<uint> ZeroParentTailInstructionOffsets =
        ImmutableArray.Create(
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.FirstStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.SecondStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.ThirdStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.FourthStoreRawFileOffset,
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateProjection.MovEcxRawFileOffset);

    public static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleProjection.Project(
                capture.ParentCapture);

        bool nonzeroHandoff = IsExactNonzeroEntryHandoff(parent);
        bool zeroHandoff = IsExactZeroCleanupCallHandoff(parent);
        bool zeroCallReached = zeroHandoff
            && parent.CleanupCounterPlan?.MovEcxReached == true;
        if (zeroCallReached && capture.CleanupCallReturnedNormally != true)
        {
            return Finish(
                capture,
                parent,
                null,
                capture.CleanupCallReturnedNormally is null
                    ? CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                        .MissingCleanupCallReturnStatus
                    : CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
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
                    == CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
                        .DecrementedToZero
                    ? CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                        .ParentDecrementedToZeroBeforeChild
                    : CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                        .ParentNotAdmittedToChild,
                parent.StopAtRawFileOffsetExclusive);
        }

        if (capture.LateChildEsp is not uint childEsp
            || capture.LateChildEbp is not uint childEbp)
        {
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome pendingOutcome =
                capture.LateChildEsp is null && capture.LateChildEbp is null
                    ? CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                        .MissingLateChildEspAndEbp
                    : capture.LateChildEsp is null
                        ? CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEsp
                        : CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEbp;
            return Finish(
                capture,
                parent,
                null,
                pendingOutcome,
                ChildEntryRawFileOffset);
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
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome
                .ChildProjectionStarted,
            epilogue.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
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
            && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
            && parent.OrderedInstructions.TakeLast(NonzeroParentTailInstructionOffsets.Length)
                .Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(NonzeroParentTailInstructionOffsets)
            && parent.OrderedMemoryEffects.All(effect => effect.Attempted
                && (effect.Kind
                        != CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore
                    || effect.ValueCaptured && effect.Committed));
    }

    public static bool IsExactZeroCleanupCallHandoff(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        return IsExactCompletedParent(parent)
            && parent.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleOutcome
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
            && parent.StopAtRawFileOffsetExclusive == ZeroCounterStopRawFileOffset
            && parent.OrderedInstructions.TakeLast(ZeroParentTailInstructionOffsets.Length)
                .Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(ZeroParentTailInstructionOffsets)
            && parent.OrderedMemoryEffects.All(effect => effect.Attempted
                && (effect.Kind
                        != CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore
                    || effect.ValueCaptured && effect.Committed));
    }

    public static ImmutableArray<
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect> BuildOrderedEffects(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan plan)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect>();

        foreach (CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffect
            effect in plan.ParentPlan.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentStore =>
                        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                            .ParentStore,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead =>
                        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterRead,
                    CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterWrite =>
                        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterWrite,
                    _ =>
                        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                            .ParentRead
                };
            effects.Add(new(
                kind,
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
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.CleanupCall,
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
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                attempted: true,
                valueCaptured: child.TestedByteCaptured));
        }

        AddStackPop(effects, child.StackPopEdiReached,
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress, child.PoppedEdi);
        AddStackPop(effects, child.StackPopEsiReached,
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress, child.PoppedEsi);
        AddStackPop(effects, child.StackPopEbxReached,
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress, child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.TestBranch,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.BranchRawFileOffset,
                null,
                null,
                child.BranchTaken,
                Attempted: true,
                ValueCaptured: child.BranchTaken.HasValue,
                Committed: false));
        }

        if (child.PushImmediateReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .PushImmediate,
                CapacityFinalCalleeGatedEpilogueProjection.PushImmediateRawFileOffset,
                child.PushImmediateAddress,
                child.PushedImmediateVirtualAddress,
                attempted: true,
                valueCaptured: child.PushedImmediateVirtualAddress.HasValue,
                committed: true));
        }

        if (child.CallCellReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .CallCellRead,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.CallCellVirtualAddress,
                child.CallCellTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.CallCellTargetCaptured));
        }

        if (child.CallReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .OpaqueCallReturnStatus,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                null,
                null,
                child.Capture.CallReturnedNormally,
                Attempted: true,
                ValueCaptured: child.Capture.CallReturnedNormally.HasValue,
                Committed: false));
        }

        if (child.PostCallEbpCaptureReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .PostCallEbpCapture,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.PostCallEbp,
                attempted: true,
                valueCaptured: child.PostCallEbpCaptured));
        }

        if (child.MovEspFromEbpReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                attempted: true,
                valueCaptured: child.MovEspSourceEbp.HasValue));
        }

        AddStackPop(effects, child.StackPopEbpReached,
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress, child.PoppedEbp);

        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .ReturnTargetRead,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.ReturnTargetCaptured));
        }

        if (child.ReturnReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind
                    .ReturnInstruction,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.ReturnTargetCaptured));
        }

        return effects.ToImmutable();
    }

    private static bool IsExactCompletedParent(
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan parent) =>
        parent.ExactBoundedCompletion
        && parent.ExactCleanCompletion
        && parent.ExactParentStoreCompletion
        && parent.ExactCleanupCounterCompletion
        && parent.CleanupCounterGateEntered
        && parent.ParentCleanupJumpReached
        && !parent.ParentCleanupJumpTargetEntered
        && parent.ParentStorePlan.Writes.Length == 4
        && parent.ParentStorePlan.Writes.All(write => write.Attempted && write.Committed)
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

    private static void AddStackPop(
        ImmutableArray<
            CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect>.Builder effects,
        bool reached,
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value)
    {
        if (reached)
        {
            effects.Add(ChildEffect(
                kind,
                rawFileOffset,
                address,
                value,
                attempted: true,
                valueCaptured: value.HasValue));
        }
    }

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffect ChildEffect(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value,
        bool attempted,
        bool valueCaptured,
        bool committed = false) =>
        new(kind, -1, rawFileOffset, address, value, null, attempted, valueCaptured, committed);

    private static CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeIndexedTraversalSlotCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
