using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleCapture(
    uint? FreshGlobalValue,
    uint? ParentEntryEDX,
    uint? FreshEDXPlus8,
    uint? LateEntryESI,
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

public enum CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZero,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan(
    CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonRingCleanupLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCommonRingCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroBypass =>
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
            .IsExactZeroBypass(ParentPlan);
    public bool ChildEntryPending =>
        ParentExactNonzeroEntryHandoff && EpiloguePlan is null;
    public bool ChildEntryValidated =>
        EpiloguePlan?.Capture.EntryValidated == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonRingCleanupLifecycleFlags? ParentFlagsAtChildEntry =>
        ParentPlan.FlagsAtStop is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null;
    public CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleFlags? FlagsAtStop =>
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
            : ParentFlagsAtChildEntry is { } parentFlags
                ? new(
                    parentFlags.Carry,
                    parentFlags.Parity,
                    parentFlags.Auxiliary,
                    parentFlags.Zero,
                    parentFlags.Sign,
                    parentFlags.Overflow)
                : null;
    public bool ParentFlagsPreservedUntilChildTest =>
        EpiloguePlan?.TestedByteCaptured != true
        && FlagsAtStop is { } stopFlags
        && ParentFlagsAtChildEntry is { } parentFlags
        && stopFlags.Carry == parentFlags.Carry
        && stopFlags.Parity == parentFlags.Parity
        && stopFlags.Auxiliary == parentFlags.Auxiliary
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
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public bool LateChildEspProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEbp.HasValue;
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
    public bool ExactCleanCompletion =>
        ParentExactZeroBypass
            && !AnyChildCaptureProvided
        || ChildExactCleanExitAdmitted
            && ParentExactNonzeroEntryHandoff;

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
/// Admits the 9441 epilogue only from the exact nonzero cleanup-counter branch.
/// ESP, EBP, TEST, stack, and opaque-call observations remain independent captures.
/// </summary>
public static class CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonRingCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCommonRingCleanupLifecycleProjection.CleanupZeroPathStopRawFileOffset;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonRingCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonRingCleanupLifecycleProjection.Project(new(
                capture.FreshGlobalValue,
                capture.ParentEntryEDX,
                capture.FreshEDXPlus8,
                capture.LateEntryESI,
                capture.FreshCounterDword));

        if (IsExactNonzeroEntryHandoff(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
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
                CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .ChildProjectionStarted,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome outcome =
            IsExactZeroBypass(parent)
                ? CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZero
                : CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild;
        return Finish(
            capture,
            parent,
            epilogue: null,
            outcome,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonRingCleanupLifecyclePlan parent)
    {
        if (!parent.ExactCleanCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedNonzero
            || !parent.ParentRingBoundaryReached
            || parent.ParentRingBoundaryInstructionEntered
            || !parent.CleanupCounterInstructionEntered
            || parent.LateEntryESIProvidedButUnused
            || parent.LateCounterProvidedButUnused
            || !CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactRingBoundaryHandoff(parent.ParentPlan)
            || parent.CleanupCounterPlan is not { } cleanup
            || !CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactCleanupCompletion(parent.ParentPlan, cleanup)
            || cleanup.Outcome != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            || cleanup.Capture.EntryValidated != true
            || cleanup.JnzReached != true
            || cleanup.JnzTaken != true
            || cleanup.SelectedDestinationRawFileOffset != ChildEntryRawFileOffset
            || cleanup.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || cleanup.CallReached
            || cleanup.CalleeEntered
            || parent.StopAtRawFileOffsetExclusive != ChildEntryRawFileOffset
            || parent.OrderedMemoryEffects.Count(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind.RingWrite
                && effect.Committed) != 5)
        {
            return false;
        }

        return true;
    }

    public static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonRingCleanupLifecyclePlan parent)
    {
        if (!parent.ExactCleanCompletion
            || parent.Outcome
                != CapacityFinalCalleeCommonRingCleanupLifecycleOutcome.DecrementedToZero
            || !parent.ParentRingBoundaryReached
            || parent.ParentRingBoundaryInstructionEntered
            || !parent.CleanupCounterInstructionEntered
            || parent.LateEntryESIProvidedButUnused
            || parent.LateCounterProvidedButUnused
            || !CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactRingBoundaryHandoff(parent.ParentPlan)
            || parent.CleanupCounterPlan is not { } cleanup
            || !CapacityFinalCalleeCommonRingCleanupLifecycleProjection
                .IsExactCleanupCompletion(parent.ParentPlan, cleanup)
            || cleanup.Outcome != CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
            || cleanup.Capture.EntryValidated != true
            || cleanup.JnzReached != true
            || cleanup.JnzTaken != false
            || cleanup.SelectedDestinationRawFileOffset
                != CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
            || cleanup.StopAtRawFileOffsetExclusive != ZeroCounterStopRawFileOffset
            || cleanup.CallReached
            || cleanup.CalleeEntered
            || parent.StopAtRawFileOffsetExclusive != ZeroCounterStopRawFileOffset
            || parent.OrderedMemoryEffects.Count(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonRingCleanupLifecycleMemoryEffectKind.RingWrite
                && effect.Committed) != 5)
        {
            return false;
        }

        return true;
    }

    private static CapacityFinalCalleeCommonRingCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonRingCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonRingCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
