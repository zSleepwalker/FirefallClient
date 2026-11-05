using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleCapture(
    uint? ParentEntryECX,
    uint? FreshGlobalValue,
    uint? LateEntryESI,
    uint? LateEntryEDX,
    uint? FreshCounterDword,
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

public enum CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZero,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan(
    CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroBypass =>
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
            .IsExactZeroBypass(ParentPlan);
    public bool ChildEntryPending =>
        ParentExactNonzeroEntryHandoff && EpiloguePlan is null;
    public bool ChildEntryValidated =>
        EpiloguePlan is { } epilogue && epilogue.Capture.EntryValidated;
    public bool ChildEntryTargetEntered => EpiloguePlan is { } epilogue
        && epilogue.TestReadReached;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan is { } epilogue ? epilogue.Outcome : null;
    public CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleFlags?
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
    public CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleFlags? FlagsAtStop =>
        EpiloguePlan is
        {
            CallReached: true,
            CallReturnedNormally: true
        }
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
        EpiloguePlan is not { TestedByteCaptured: true }
        && FlagsAtStop is { } stopFlags
        && ParentFlagsAtChildEntry is { } parentFlags
        && stopFlags.Carry == parentFlags.Carry
        && stopFlags.Parity == parentFlags.Parity
        && stopFlags.AuxiliaryCarry == parentFlags.AuxiliaryCarry
        && stopFlags.Zero == parentFlags.Zero
        && stopFlags.Sign == parentFlags.Sign
        && stopFlags.Overflow == parentFlags.Overflow;
    public bool ChildTestReadReached =>
        EpiloguePlan is { } epilogue && epilogue.TestReadReached;
    public bool Internal9441Entered => ChildTestReadReached;
    public bool DestinationEntered => false;
    public bool ChildCallReached =>
        EpiloguePlan is { } epilogue && epilogue.CallReached;
    public bool OpaqueCallTargetCaptured =>
        EpiloguePlan is { } epilogue && epilogue.CallCellTargetCaptured;
    public bool? OpaqueCallReturnedNormally =>
        EpiloguePlan is { } epilogue ? epilogue.CallReturnedNormally : null;
    public bool OpaqueCallExecuted => false;
    public bool ChildReturnReached =>
        EpiloguePlan is { } epilogue && epilogue.ReturnReached;
    public uint ChildReturnInstructionRawFileOffset =>
        CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset;
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { } epilogue
        && epilogue.ExactCleanExitAdmitted
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan is { } epilogue
            ? epilogue.StackAccesses
            : ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotCleanupMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.OrderedMemoryEffects;
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        ParentExactNonzeroEntryHandoff
        && ChildTestReadReached
        && ParentPlan.AllFourStoresCommitted
        && ParentPlan.CounterReadOccursAfterAllFourStores
        && ParentPlan.CounterWriteOccursAfterCounterRead
        && ParentPlan.OrderedMemoryEffects.Length == 7;
    public bool NonzeroBranchTargetEntered => false;
    public bool ZeroPathFallthroughEntered => ParentPlan.ZeroPathFallthroughEntered;
    public bool DirectCallReached => ParentPlan.DirectCallReached;
    public bool CalleeEntered => ParentPlan.CalleeEntered;
    public bool LateChildEspProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEbp.HasValue;
    public bool UnreachedTestedByteCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedTestedByteCaptureProvided
            : Capture.TestedByte.HasValue;
    public bool UnreachedStackPopEdiCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedStackPopEdiCaptureProvided
            : Capture.StackPopEdiValue.HasValue;
    public bool UnreachedStackPopEsiCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedStackPopEsiCaptureProvided
            : Capture.StackPopEsiValue.HasValue;
    public bool UnreachedStackPopEbxCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedStackPopEbxCaptureProvided
            : Capture.StackPopEbxValue.HasValue;
    public bool UnreachedCallCellTargetCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedCallCellTargetCaptureProvided
            : Capture.CallCellTargetVirtualAddress.HasValue;
    public bool UnreachedCallReturnStatusProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedCallReturnStatusProvided
            : Capture.CallReturnedNormally.HasValue;
    public bool UnreachedPostCallEbpCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedPostCallEbpCaptureProvided
            : Capture.PostCallEbp.HasValue;
    public bool UnreachedStackPopEbpCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedStackPopEbpCaptureProvided
            : Capture.StackPopEbpValue.HasValue;
    public bool UnreachedReturnTargetCaptureProvided =>
        EpiloguePlan is { } epilogue
            ? epilogue.UnreachedReturnTargetCaptureProvided
            : Capture.ReturnTargetVirtualAddress.HasValue;
    public bool ExactCleanCompletion =>
        ParentExactZeroBypass && !AnyChildCaptureProvided
        || ParentExactNonzeroEntryHandoff && ChildExactCleanExitAdmitted;

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
/// Admits the 9441 epilogue only from the exact four-store zero-slot cleanup's nonzero
/// counter branch. The zero result remains bounded before the call at 943C.
/// </summary>
public static class CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonZeroSlotCleanupLifecycleProjection.Project(new(
                capture.ParentEntryECX,
                capture.FreshGlobalValue,
                capture.LateEntryESI,
                capture.LateEntryEDX,
                capture.FreshCounterDword));

        if (IsExactNonzeroEntryHandoff(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEbp;
                return Finish(
                    capture,
                    parent,
                    epilogue: null,
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
                CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .ChildProjectionStarted,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome outcome =
            IsExactZeroBypass(parent)
                ? CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZero
                : CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild;
        return Finish(
            capture,
            parent,
            epilogue: null,
            outcome,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan parent)
    {
        if (!parent.ExactBoundedCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .DecrementedNonzeroBranchTarget
            || !parent.CleanupCounterGateEntryReached
            || !parent.AllFourStoresCommitted
            || !parent.StoreJumpTargetStoppedBeforeEntry
            || !parent.CounterReadOccursAfterAllFourStores
            || !parent.CounterReadValueCaptured
            || !parent.CounterWriteCommitted
            || !parent.CounterWriteOccursAfterCounterRead
            || !parent.CounterValueIsIndependentFreshCapture
            || parent.FreshCounterDwordCaptureUnused
            || parent.CleanupCounterGate.UnreachedFreshCounterDwordCaptureProvided
            || parent.CleanupCounterGate.UnreachedIncomingCarryFlagCaptureProvided
            || parent.CleanupCounterGate.Capture.EntryValidated != true
            || parent.CleanupCounterGate.Outcome
                != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            || !parent.CleanupCounterGate.JnzReached
            || parent.CleanupCounterGate.JnzTaken != true
            || parent.CleanupCounterGate.SelectedDestinationRawFileOffset
                != ChildEntryRawFileOffset
            || parent.CleanupCounterGate.StopAtRawFileOffsetExclusive
                != ChildEntryRawFileOffset
            || parent.NonzeroBranchTargetEntered
            || parent.ZeroPathFallthroughEntered
            || parent.DirectCallReached
            || parent.CalleeEntered
            || parent.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || parent.OrderedMemoryEffects.Length != 7)
        {
            return false;
        }

        return true;
    }

    public static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan parent)
    {
        if (!parent.ExactBoundedCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonZeroSlotCleanupLifecycleOutcome
                    .DecrementedToZeroBeforeCall
            || !parent.CleanupCounterGateEntryReached
            || !parent.AllFourStoresCommitted
            || !parent.StoreJumpTargetStoppedBeforeEntry
            || !parent.CounterReadOccursAfterAllFourStores
            || !parent.CounterReadValueCaptured
            || !parent.CounterWriteCommitted
            || !parent.CounterWriteOccursAfterCounterRead
            || !parent.CounterValueIsIndependentFreshCapture
            || parent.FreshCounterDwordCaptureUnused
            || parent.CleanupCounterGate.UnreachedFreshCounterDwordCaptureProvided
            || parent.CleanupCounterGate.UnreachedIncomingCarryFlagCaptureProvided
            || parent.CleanupCounterGate.Capture.EntryValidated != true
            || parent.CleanupCounterGate.Outcome
                != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
            || !parent.CleanupCounterGate.JnzReached
            || parent.CleanupCounterGate.JnzTaken != false
            || parent.CleanupCounterGate.SelectedDestinationRawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            || !parent.ZeroPathFallthroughEntered
            || parent.CleanupCounterGate.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
                    .ZeroCounterStopRawFileOffset
            || parent.DirectCallReached
            || parent.CalleeEntered
            || parent.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleProjection
                    .ZeroCounterStopRawFileOffset
            || parent.OrderedMemoryEffects.Length != 7)
        {
            return false;
        }

        return true;
    }

    private static CapacityFinalCalleeCommonZeroCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonZeroSlotCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonZeroCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
