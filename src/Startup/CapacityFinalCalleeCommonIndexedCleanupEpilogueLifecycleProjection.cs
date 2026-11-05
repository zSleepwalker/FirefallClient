using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleCapture(
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleCapture ParentCapture,
    uint? LateChildEsp,
    uint? LateChildEbp,
    byte? TestedByte,
    uint? StackPopEdiValue,
    uint? StackPopEsiValue,
    uint? StackPopEbxValue,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally,
    uint? PostCallEbp,
    uint? StackPopEbpValue,
    uint? ReturnTargetVirtualAddress);

public enum CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZeroBeforeCall,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public enum CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
{
    ParentRead,
    ParentStore,
    CleanupCounterRead,
    CleanupCounterWrite,
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

public readonly record struct CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect(
    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind Kind,
    int SourceIndex,
    uint? RawFileOffset,
    uint? Address,
    uint? Value,
    bool? BooleanValue,
    bool Attempted,
    bool ValueCaptured,
    bool Committed);

public sealed record CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan(
    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome Outcome,
    CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroBypass =>
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
            .IsExactZeroBypass(ParentPlan);
    public bool ChildEntryPending =>
        ParentExactNonzeroEntryHandoff && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleFlags?
        ParentFlagsAtChildEntry =>
            ParentPlan.FlagsAtStop is { } flags
                ? new(
                    flags.Carry,
                    flags.Parity,
                    flags.AuxiliaryCarry,
                    flags.Zero,
                    flags.Sign,
                    flags.Overflow)
                : null;
    public CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleFlags? FlagsAtStop =>
        EpiloguePlan is { CallReached: true, CallReturnedNormally: true }
            ? new(null, null, null, null, null, null)
            : EpiloguePlan is
            {
                TestedByteCaptured: true,
                TestFlags: { } testFlags
            }
                ? new(
                    testFlags.Carry,
                    testFlags.Parity,
                    testFlags.AuxiliaryCarry,
                    testFlags.Zero,
                    testFlags.Sign,
                    testFlags.Overflow)
                : ParentFlagsAtChildEntry;
    public bool ParentFlagsPreservedUntilChildTest =>
        EpiloguePlan?.TestedByteCaptured != true
        && FlagsAtStop is { } stopFlags
        && ParentFlagsAtChildEntry is { } parentFlags
        && stopFlags.Carry == parentFlags.Carry
        && stopFlags.Parity == parentFlags.Parity
        && stopFlags.AuxiliaryCarry == parentFlags.AuxiliaryCarry
        && stopFlags.Zero == parentFlags.Zero
        && stopFlags.Sign == parentFlags.Sign
        && stopFlags.Overflow == parentFlags.Overflow;
    public bool ChildTestReadReached => EpiloguePlan?.TestReadReached == true;
    public bool Internal9441Entered => ChildTestReadReached;
    public bool DestinationEntered => false;
    public bool ChildCallReached => EpiloguePlan?.CallReached == true;
    public bool OpaqueCallTargetCaptured => EpiloguePlan?.CallCellTargetCaptured == true;
    public bool? OpaqueCallReturnedNormally => EpiloguePlan?.CallReturnedNormally;
    public bool OpaqueCallExecuted => false;
    public bool ChildReturnReached => EpiloguePlan?.ReturnReached == true;
    public uint ChildReturnInstructionRawFileOffset =>
        CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset;
    public bool ChildBoundedExitReached =>
        ParentExactNonzeroEntryHandoff
        && EpiloguePlan is
        {
            ReturnReached: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
                    .ReturnBoundaryRawFileOffsetExclusive
        };
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public ImmutableArray<
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>
        ParentOrderedInstructions => ParentPlan.OrderedInstructions;
    public ImmutableArray<
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public ImmutableArray<
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect> OrderedEffects =>
            CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
                .BuildOrderedEffects(ParentPlan, EpiloguePlan);
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        ParentExactNonzeroEntryHandoff
        && ChildTestReadReached
        && ParentPlan.FourParentStoresCommitted
        && ParentPlan.CounterReadAttempted
        && ParentPlan.CounterValueCaptured
        && ParentPlan.CounterWriteCommitted
        && ParentPlan.OrderedMemoryEffects.Length == 10
        && OrderedEffects.Length >= 11
        && OrderedEffects.Take(10).Select(effect => effect.Kind)
            .SequenceEqual(ParentEffectKinds);
    public bool NonzeroBranchTargetEntered => false;
    public bool ZeroPathFallthroughEntered =>
        ParentPlan.CleanupCounterPlan?.MovEcxReached == true;
    public bool DirectCallReached =>
        ParentPlan.CleanupCounterPlan?.CallReached == true;
    public bool CalleeEntered =>
        ParentPlan.CleanupCounterPlan?.CalleeEntered == true;
    public bool LateChildEspProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEbp.HasValue;
    public bool UnreachedTestedByteCaptureProvided =>
        EpiloguePlan?.UnreachedTestedByteCaptureProvided ?? Capture.TestedByte.HasValue;
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
    public bool UnreachedCallReturnStatusProvided =>
        EpiloguePlan?.UnreachedCallReturnStatusProvided
        ?? Capture.CallReturnedNormally.HasValue;
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
            : StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);
    public bool ExactBoundedExitAdmitted => ExactBoundedCompletion;
    public bool ExactCleanCompletion =>
        ParentExactNonzeroEntryHandoff
            ? ChildExactCleanExitAdmitted
            : ParentPlan.ExactCleanCompletion && !AnyChildCaptureProvided;

    private bool AnyChildCaptureProvided =>
        Capture.LateChildEsp.HasValue
        || Capture.LateChildEbp.HasValue
        || Capture.TestedByte.HasValue
        || Capture.StackPopEdiValue.HasValue
        || Capture.StackPopEsiValue.HasValue
        || Capture.StackPopEbxValue.HasValue
        || Capture.CallCellTargetVirtualAddress.HasValue
        || Capture.CallReturnedNormally.HasValue
        || Capture.PostCallEbp.HasValue
        || Capture.StackPopEbpValue.HasValue
        || Capture.ReturnTargetVirtualAddress.HasValue;

    private static ImmutableArray<CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind>
        ParentEffectKinds =>
            ImmutableArray.Create(
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CleanupCounterRead,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CleanupCounterWrite);
}

/// <summary>
/// Joins the indexed four-store DEC/JNZ handoff to the bounded 9441 epilogue.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
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

    public static CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleProjection.Project(
                capture.ParentCapture);

        if (IsExactNonzeroEntryHandoff(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
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
                    CallReturnedNormally: capture.CallReturnedNormally,
                    PostCallEbp: capture.PostCallEbp,
                    StackPopEbpValue: capture.StackPopEbpValue,
                    ReturnTargetVirtualAddress: capture.ReturnTargetVirtualAddress));
            return Finish(
                capture,
                parent,
                epilogue,
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .ChildProjectionStarted,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome outcome =
            IsExactZeroBypass(parent)
                ? CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZeroBeforeCall
                : CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild;
        return Finish(
            capture,
            parent,
            null,
            outcome,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        if (!parent.ExactBoundedCompletion
            || !parent.ExactCleanCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .DecrementedNonzero
            || !parent.ExactParentStoreCompletion
            || !parent.ExactCleanupCounterCompletion
            || !parent.CleanupCounterGateEntered
            || !parent.FourParentStoresCommitted
            || !parent.Internal942FEntered
            || !parent.CounterReadAttempted
            || !parent.CounterValueCaptured
            || !parent.CounterWriteCommitted
            || gate is null
            || gate.Capture.EntryValidated != true
            || gate.Capture.FreshCounterDword is null
            || gate.Outcome != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            || !gate.CounterReadReached
            || !gate.FreshCounterDwordCaptured
            || !gate.CounterWriteReached
            || !gate.JnzReached
            || gate.JnzTaken != true
            || gate.SelectedDestinationRawFileOffset != ChildEntryRawFileOffset
            || gate.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || gate.CallReached
            || gate.CalleeEntered
            || gate.MovEcxReached
            || gate.CounterAccesses.Length != 2
            || gate.ReachedInstructions.Length != 2
            || parent.CounterJnzTaken != true
            || parent.CounterJnzDestinationRawFileOffset != ChildEntryRawFileOffset
            || parent.CounterNonzeroTargetEntered
            || parent.ZeroPathCallReached
            || parent.ZeroPathCalleeEntered
            || parent.ZeroPathImmediateWasMemoryRead
            || parent.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || !HasExactParentMemoryEffects(parent)
            || !HasExactInstructionTail(parent.OrderedInstructions, NonzeroParentTailInstructionOffsets)
            || parent.OrderedInstructions.Any(instruction =>
                !instruction.Reached || !instruction.Executed))
        {
            return false;
        }

        return gate.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress,
                ValueCaptured: true
            }
            && gate.CounterAccesses[1] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
                Address: CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress,
                ValueCaptured: true
            }
            && gate.CounterAccesses[0].Value is uint before
            && gate.CounterBefore == before
            && gate.CounterAfter == unchecked(before - 1)
            && gate.CounterAccesses[1].Value == unchecked(before - 1)
            && gate.CounterAccesses[1].Value != 0
            && gate.FlagsAfterDecrement is { Carry: false }
            && parent.OrderedMemoryEffects.All(effect => effect.Attempted
                && (effect.Kind
                        != CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore
                    || effect.ValueCaptured && effect.Committed));
    }

    public static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        if (!parent.ExactBoundedCompletion
            || !parent.ExactCleanCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleOutcome
                    .DecrementedToZero
            || !parent.ExactParentStoreCompletion
            || !parent.ExactCleanupCounterCompletion
            || !parent.FourParentStoresCommitted
            || !parent.Internal942FEntered
            || !parent.CounterReadAttempted
            || !parent.CounterValueCaptured
            || !parent.CounterWriteCommitted
            || gate is null
            || gate.Capture.EntryValidated != true
            || gate.Outcome != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
            || !gate.CounterReadReached
            || !gate.FreshCounterDwordCaptured
            || !gate.CounterWriteReached
            || !gate.JnzReached
            || gate.JnzTaken != false
            || gate.SelectedDestinationRawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            || gate.StopAtRawFileOffsetExclusive != ZeroCounterStopRawFileOffset
            || !gate.MovEcxReached
            || gate.EcxValue != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
            || gate.CallReached
            || gate.CalleeEntered
            || gate.CounterAccesses.Length != 2
            || parent.CounterJnzTaken != false
            || parent.CounterJnzDestinationRawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != ZeroCounterStopRawFileOffset
            || parent.ZeroPathCallReached
            || parent.ZeroPathCalleeEntered
            || !HasExactParentMemoryEffects(parent)
            || !HasExactInstructionTail(parent.OrderedInstructions, ZeroParentTailInstructionOffsets)
            || parent.OrderedInstructions.Any(instruction =>
                !instruction.Reached || !instruction.Executed))
        {
            return false;
        }

        return gate.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress,
                ValueCaptured: true,
                Value: 1
            }
            && gate.CounterAccesses[1] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
                Address: CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress,
                ValueCaptured: true,
                Value: 0
            }
            && gate.CounterBefore == 1
            && gate.CounterAfter == 0
            && gate.FlagsAfterDecrement is { Carry: false, Zero: true };
    }

    public static ImmutableArray<CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect>
        BuildOrderedEffects(
            CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent,
            CapacityFinalCalleeGatedEpiloguePlan? epilogue)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect>();
        foreach (CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            uint? address = null;
            uint? value = null;
            switch (effect.Kind)
            {
                case CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentRead:
                    if (effect.SourceIndex < parent.OrderedReads.Length)
                    {
                        CapacityFinalCalleeCommonIndexedTraversalZeroSlotLifecycleRead read =
                            parent.OrderedReads[effect.SourceIndex];
                        address = read.Address;
                        value = read.Value;
                    }

                    effects.Add(new(
                        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentRead,
                        effect.SourceIndex,
                        effect.RawFileOffset,
                        address,
                        value,
                        null,
                        effect.Attempted,
                        effect.ValueCaptured,
                        effect.Committed));
                    break;

                case CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentStore:
                    if (effect.SourceIndex < parent.StoreWrites.Length)
                    {
                        CapacityFinalCalleeCommonZeroSlotStoreTailWrite write =
                            parent.StoreWrites[effect.SourceIndex];
                        address = write.Address;
                        value = write.Value;
                    }

                    effects.Add(new(
                        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.ParentStore,
                        effect.SourceIndex,
                        effect.RawFileOffset,
                        address,
                        value,
                        null,
                        effect.Attempted,
                        effect.ValueCaptured,
                        effect.Committed));
                    break;

                case CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead:
                case CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterWrite:
                    if (parent.CleanupCounterPlan is { } counter
                        && effect.SourceIndex < counter.CounterAccesses.Length)
                    {
                        CapacityFinalCalleeCleanupCounterGateCounterAccess access =
                            counter.CounterAccesses[effect.SourceIndex];
                        address = access.Address;
                        value = access.Value;
                    }

                    effects.Add(new(
                        effect.Kind
                            == CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind
                                .CleanupCounterRead
                            ? CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterRead
                            : CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterWrite,
                        effect.SourceIndex,
                        effect.RawFileOffset,
                        address,
                        value,
                        null,
                        effect.Attempted,
                        effect.ValueCaptured,
                        effect.Committed));
                    break;
            }
        }

        if (epilogue is not { } child)
        {
            return effects.ToImmutable();
        }

        if (child.TestReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                attempted: true,
                valueCaptured: child.TestedByteCaptured));
        }

        AddStackPop(
            effects,
            child.StackPopEdiReached,
            CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress,
            child.PoppedEdi);
        AddStackPop(
            effects,
            child.StackPopEsiReached,
            CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress,
            child.PoppedEsi);
        AddStackPop(
            effects,
            child.StackPopEbxReached,
            CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress,
            child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.TestBranch,
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
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.PushImmediate,
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
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.CallCellRead,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.CallCellVirtualAddress,
                child.CallCellTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.CallCellTargetCaptured));
        }

        if (child.CallReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                attempted: true,
                valueCaptured: child.MovEspSourceEbp.HasValue));
        }

        AddStackPop(
            effects,
            child.StackPopEbpReached,
            CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind.StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress,
            child.PoppedEbp);

        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind
                    .ReturnInstruction,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.ReturnTargetCaptured));
        }

        return effects.ToImmutable();
    }

    private static void AddStackPop(
        ImmutableArray<CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect>.Builder
            effects,
        bool reached,
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind effectKind,
        uint rawFileOffset,
        uint? address,
        uint? value)
    {
        if (!reached)
        {
            return;
        }

        effects.Add(ChildEffect(
            effectKind,
            rawFileOffset,
            address,
            value,
            attempted: true,
            valueCaptured: value.HasValue));
    }

    private static CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffect ChildEffect(
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value,
        bool attempted,
        bool valueCaptured,
        bool committed = false) =>
        new(
            kind,
            -1,
            rawFileOffset,
            address,
            value,
            null,
            attempted,
            valueCaptured,
            committed);

    private static bool HasExactParentMemoryEffects(
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent)
    {
        if (parent.OrderedMemoryEffects.Length != 10
            || parent.OrderedReads.Length != 4
            || parent.StoreWrites.Length != 4
            || parent.CleanupCounterPlan?.CounterAccesses.Length != 2)
        {
            return false;
        }

        ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind>
            expected = ImmutableArray.Create(
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.CleanupCounterRead,
                CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleMemoryEffectKind.CleanupCounterWrite);
        return parent.OrderedMemoryEffects.Select(effect => effect.Kind).SequenceEqual(expected)
            && parent.OrderedMemoryEffects.Take(4).All(effect =>
                effect.Attempted && effect.ValueCaptured && !effect.Committed)
            && parent.OrderedMemoryEffects.Skip(4).Take(4).All(effect =>
                effect.Attempted && effect.ValueCaptured && effect.Committed)
            && parent.OrderedMemoryEffects[^2] is
            {
                RawFileOffset:
                    CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
                Attempted: true,
                ValueCaptured: true,
                Committed: false
            }
            && parent.OrderedMemoryEffects[^1] is
            {
                RawFileOffset:
                    CapacityFinalCalleeCleanupCounterGateProjection.CounterDecrementRawFileOffset,
                Attempted: true,
                ValueCaptured: true,
                Committed: true
            };
    }

    private static bool HasExactInstructionTail(
        ImmutableArray<CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecycleInstruction>
            instructions,
        ImmutableArray<uint> expectedOffsets) =>
        instructions.Length >= expectedOffsets.Length
        && instructions.TakeLast(expectedOffsets.Length)
            .Select(instruction => instruction.RawFileOffset)
            .SequenceEqual(expectedOffsets);

    private static CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonIndexedTraversalSlotCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonIndexedCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
