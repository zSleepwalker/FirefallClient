using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleCapture(
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleCapture StoreCleanupCapture,
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

public enum CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZeroBeforeCall,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public enum CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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

public readonly record struct CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect(
    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind Kind,
    int SourceIndex,
    uint? RawFileOffset,
    uint? Address,
    uint? Value,
    bool? BooleanValue,
    bool Attempted,
    bool ValueCaptured,
    bool Committed);

public sealed record CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan(
    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome Outcome,
    CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroBypass =>
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
            .IsExactZeroBypass(ParentPlan);
    public bool ChildEntryPending =>
        ParentExactNonzeroEntryHandoff && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleFlags?
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
    public CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleFlags? FlagsAtStop =>
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
                    .ReturnBoundaryRawFileOffsetExclusive
        };
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>
        ParentOrderedInstructions => ParentPlan.OrderedInstructions;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public ImmutableArray<CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect>
        OrderedEffects =>
            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
                .BuildOrderedEffects(ParentPlan, EpiloguePlan);
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        ParentExactNonzeroEntryHandoff
        && ChildTestReadReached
        && ParentPlan.FourParentStoresCommitted
        && ParentPlan.CounterReadAttempted
        && ParentPlan.CounterValueCaptured
        && ParentPlan.CounterWriteCommitted
        && ParentPlan.OrderedMemoryEffects.Length == 9
        && OrderedEffects.Length >= 10
        && OrderedEffects.Take(9).Select(effect => effect.Kind).SequenceEqual(
            ParentOrderedMemoryEffects.Select(effect => effect.Kind)
                .Select(kind => kind switch
                {
                    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentRead =>
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .ParentRead,
                    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .ParentStore =>
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .ParentStore,
                    CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                        .CleanupCounterRead =>
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterRead,
                    _ =>
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .CleanupCounterWrite
                }))
        && OrderedEffects[9] is
        {
            Kind: CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                .TestByteRead,
            Attempted: true
        };
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
}

/// <summary>
/// Joins the exact traversal slot-store cleanup DEC/JNZ handoff to the bounded 9441 epilogue.
/// </summary>
public static class CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    private static readonly ImmutableArray<uint> NonzeroParentTailInstructionOffsets =
        ImmutableArray.Create(
            0x012C9468u,
            0x012C946Au,
            0x012C946Du,
            0x012C9470u,
            0x012C9473u,
            0x012C942Fu,
            0x012C9435u);

    private static readonly ImmutableArray<uint> ZeroParentTailInstructionOffsets =
        ImmutableArray.Create(
            0x012C9468u,
            0x012C946Au,
            0x012C946Du,
            0x012C9470u,
            0x012C9473u,
            0x012C942Fu,
            0x012C9435u,
            0x012C9437u);

    public static CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection.Project(
                capture.StoreCleanupCapture);

        if (IsExactNonzeroEntryHandoff(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .ChildProjectionStarted,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome outcome =
            IsExactZeroBypass(parent)
                ? CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZeroBeforeCall
                : CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild;
        return Finish(
            capture,
            parent,
            null,
            outcome,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        if (!parent.ExactBoundedCompletion
            || !parent.ExactCleanCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
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
            || gate.Outcome
                != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            || !gate.CounterReadReached
            || !gate.FreshCounterDwordCaptured
            || !gate.CounterWriteReached
            || !gate.JnzReached
            || gate.JnzTaken != true
            || gate.SelectedDestinationRawFileOffset != ChildEntryRawFileOffset
            || gate.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || gate.CallReached
            || gate.CalleeEntered
            || gate.CounterAccesses.Length != 2
            || gate.ReachedInstructions.Length != 2
            || parent.CounterJnzTaken != true
            || parent.CounterJnzDestinationRawFileOffset != ChildEntryRawFileOffset
            || parent.CounterNonzeroTargetEntered
            || gate.MovEcxReached
            || parent.Outer9441Entered
            || parent.Outer943CCallEntered
            || parent.DestinationEntered
            || parent.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || !HasExactParentMemoryEffects(parent)
            || !HasExactInstructionTail(
                parent.OrderedInstructions,
                NonzeroParentTailInstructionOffsets)
            || parent.OrderedInstructions.Any(instruction =>
                !instruction.Reached || !instruction.Executed))
        {
            return false;
        }

        return gate.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                ValueCaptured: true
            }
            && gate.CounterAccesses[1] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
                Address: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                ValueCaptured: true
            }
            && gate.CounterAccesses[0].Value is uint before
            && gate.CounterBefore == before
            && gate.CounterAfter == unchecked(before - 1)
            && gate.CounterAccesses[1].Value == unchecked(before - 1)
            && gate.CounterAccesses[1].Value != 0
            && gate.FlagsAfterDecrement is { Carry: false }
            && parent.OrderedMemoryEffects.All(effect => effect.Attempted
                && (effect.Kind !=
                        CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                            .ParentStore
                    || effect.ValueCaptured && effect.Committed));
    }

    public static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent)
    {
        CapacityFinalCalleeCleanupCounterGatePlan? gate = parent.CleanupCounterPlan;
        if (!parent.ExactBoundedCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleOutcome
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
            || gate.CallReached
            || gate.CalleeEntered
            || gate.CounterAccesses.Length != 2
            || parent.CounterJnzTaken != false
            || parent.CounterJnzDestinationRawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != ZeroCounterStopRawFileOffset
            || parent.Outer9441Entered
            || parent.Outer943CCallEntered
            || parent.DestinationEntered
            || !HasExactParentMemoryEffects(parent)
            || !HasExactInstructionTail(
                parent.OrderedInstructions,
                ZeroParentTailInstructionOffsets)
            || parent.OrderedInstructions.Any(instruction =>
                !instruction.Reached || !instruction.Executed))
        {
            return false;
        }

        return gate.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                ValueCaptured: true
            }
            && gate.CounterAccesses[0].Value == 1
            && gate.CounterAccesses[1] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
                Address: CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleProjection
                    .CleanupCounterVirtualAddress,
                ValueCaptured: true,
                Value: 0
            }
            && gate.CounterBefore == 1
            && gate.CounterAfter == 0
            && gate.FlagsAfterDecrement is { Carry: false, Zero: true };
    }

    public static ImmutableArray<
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect>
        BuildOrderedEffects(
            CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent,
            CapacityFinalCalleeGatedEpiloguePlan? epilogue)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect>();
        foreach (CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            uint? address = null;
            uint? value = null;
            switch (effect.Kind)
            {
                case CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentRead:
                    if (effect.SourceIndex < parent.OrderedReads.Length)
                    {
                        CapacityFinalCalleeCommonTraversalZeroSlotLifecycleRead read =
                            parent.OrderedReads[effect.SourceIndex];
                        address = read.Address;
                        value = read.Value;
                    }

                    effects.Add(new(
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .ParentRead,
                        effect.SourceIndex,
                        effect.RawFileOffset,
                        address,
                        value,
                        null,
                        effect.Attempted,
                        effect.ValueCaptured,
                        effect.Committed));
                    break;

                case CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                    .ParentStore:
                    if (effect.SourceIndex < parent.StoreWrites.Length)
                    {
                        CapacityFinalCalleeCommonZeroSlotStoreTailWrite write =
                            parent.StoreWrites[effect.SourceIndex];
                        address = write.Address;
                        value = write.Value;
                    }

                    effects.Add(new(
                        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                            .ParentStore,
                        effect.SourceIndex,
                        effect.RawFileOffset,
                        address,
                        value,
                        null,
                        effect.Attempted,
                        effect.ValueCaptured,
                        effect.Committed));
                    break;

                case CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead:
                case CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
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
                            == CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind
                                .CleanupCounterRead
                            ? CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                                .CleanupCounterRead
                            : CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                    .TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                attempted: true,
                valueCaptured: child.TestedByteCaptured));
        }

        AddStackPop(
            effects,
            child.StackPopEdiReached,
            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind.StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress,
            child.PoppedEdi);
        AddStackPop(
            effects,
            child.StackPopEsiReached,
            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind.StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress,
            child.PoppedEsi);
        AddStackPop(
            effects,
            child.StackPopEbxReached,
            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind.StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress,
            child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind.TestBranch,
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
                    .MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                attempted: true,
                valueCaptured: child.MovEspSourceEbp.HasValue));
        }

        AddStackPop(
            effects,
            child.StackPopEbpReached,
            CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind.StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress,
            child.PoppedEbp);

        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
                CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind
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
        ImmutableArray<CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect>.Builder
            effects,
        bool reached,
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind effectKind,
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

    private static CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffect ChildEffect(
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleEffectKind kind,
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
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent)
    {
        if (parent.OrderedMemoryEffects.Length != 9)
        {
            return false;
        }

        ImmutableArray<
            CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind> expected =
            ImmutableArray.Create(
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentRead,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.ParentStore,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.CleanupCounterRead,
                CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleMemoryEffectKind.CleanupCounterWrite);
        return parent.OrderedMemoryEffects.Select(effect => effect.Kind)
            .SequenceEqual(expected)
            && parent.OrderedMemoryEffects.Take(3).All(effect =>
                effect.Attempted && effect.ValueCaptured)
            && parent.OrderedMemoryEffects.Skip(3).All(effect => effect.Attempted)
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
        ImmutableArray<CapacityFinalCalleeCommonTraversalSlotCleanupLifecycleInstruction>
            instructions,
        ImmutableArray<uint> expectedOffsets) =>
        instructions.Length >= expectedOffsets.Length
        && instructions.TakeLast(expectedOffsets.Length)
            .Select(instruction => instruction.RawFileOffset)
            .SequenceEqual(expectedOffsets);

    private static CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonTraversalSlotCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonTraversalCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
