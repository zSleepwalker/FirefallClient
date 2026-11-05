using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP,
    uint? EntryESI,
    uint? LateFreshGlobalValue,
    uint? LateFreshCleanupCounterDword,
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

public enum CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
{
    ParentNotAdmittedToChild,
    ParentDecrementedToZeroBeforeCall,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    ChildProjectionStarted
}

public readonly record struct CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan(
    CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan ParentPlan { get; init; } =
        null!;
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactNonzeroEntryHandoff =>
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
            .IsExactNonzeroEntryHandoff(ParentPlan);
    public bool ParentExactZeroBypass =>
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
            .IsExactZeroBypass(ParentPlan);
    public bool ParentExactNonzeroTestBypass =>
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
            .IsExactNonzeroTestBypass(ParentPlan);
    public bool ChildEntryPending =>
        ParentExactNonzeroEntryHandoff && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleFlags? ParentFlagsAtChildEntry =>
        ParentPlan.FlagsAtStop is { } flags
            ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
            : null;
    public CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleFlags? FlagsAtStop =>
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
    public bool ChildBoundedExitReached =>
        ParentExactNonzeroEntryHandoff
        && EpiloguePlan is
        {
            ReturnReached: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
        };
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
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
    public bool ExactCleanCompletion =>
        (ParentExactZeroBypass || ParentExactNonzeroTestBypass)
            && !AnyChildCaptureProvided
        || ChildExactCleanExitAdmitted && ParentExactNonzeroEntryHandoff;
    public bool ExactBoundedExitAdmitted =>
        ParentExactZeroBypass
        || ParentExactNonzeroTestBypass
        || ChildBoundedExitReached;

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
/// Admits the 9441 TEST/epilogue only from the exact node cleanup DEC/JNZ handoff.
/// </summary>
public static class CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint ZeroCounterStopRawFileOffset =
        CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(new(
                capture.EntryEDI,
                capture.LateEntryEBP,
                capture.EntryESI,
                capture.LateFreshGlobalValue,
                capture.LateFreshCleanupCounterDword));

        if (IsExactNonzeroEntryHandoff(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
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
                CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .ChildProjectionStarted,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome outcome =
            IsExactZeroBypass(parent)
                ? CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .ParentDecrementedToZeroBeforeCall
                : CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome
                    .ParentNotAdmittedToChild;
        return Finish(
            capture,
            parent,
            epilogue: null,
            outcome,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactNonzeroEntryHandoff(
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parent) =>
        parent.ExactCleanExitAdmitted
        && parent.ExactCleanupCounterGateCompletion
        && parent.Outcome
            == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                .CounterDecrementedNonzeroTo9441
        && parent.ExactParentZeroChildHandoff
        && parent.ExactParentChildCompletion
        && parent.ExactParentWrites
        && parent.ExactFiveChildWrites
        && parent.ExactOrderedMemoryEffects
        && parent.Internal942FEntryReached
        && parent.CleanupCounterReadAttempted
        && parent.CleanupCounterValueCaptured
        && parent.CleanupCounterDecrementExecuted
        && parent.CleanupCounterWriteCommitted
        && parent.JnzTaken == true
        && parent.SelectedDestinationRawFileOffset == ChildEntryRawFileOffset
        && parent.CounterGate.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset
        && !parent.CleanupJnzTargetEntered
        && !parent.FollowingCallReached
        && !parent.CalleeEntered
        && !parent.DestinationEntered
        && parent.StopAtRawFileOffsetExclusive == ChildEntryRawFileOffset;

    public static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parent) =>
        parent.ExactCleanExitAdmitted
        && parent.ExactCleanupCounterGateCompletion
        && parent.Outcome
            == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                .CounterDecrementedToZeroBeforeCall943C
        && parent.ExactParentZeroChildHandoff
        && parent.ExactParentChildCompletion
        && parent.ExactFiveChildWrites
        && parent.ExactOrderedMemoryEffects
        && parent.Internal942FEntryReached
        && parent.CleanupCounterReadAttempted
        && parent.CleanupCounterValueCaptured
        && parent.CleanupCounterDecrementExecuted
        && parent.CleanupCounterWriteCommitted
        && parent.JnzTaken == false
        && parent.SelectedDestinationRawFileOffset
            == CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
        && parent.CounterGate.StopAtRawFileOffsetExclusive == ZeroCounterStopRawFileOffset
        && parent.CounterGate.MovEcxReached
        && !parent.FollowingCallReached
        && !parent.CalleeEntered
        && !parent.DestinationEntered
        && parent.StopAtRawFileOffsetExclusive == ZeroCounterStopRawFileOffset;

    public static bool IsExactNonzeroTestBypass(
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parent) =>
        parent.ExactCleanExitAdmitted
        && parent.ExactBoundedExitAdmitted
        && parent.ExactParentNonzeroBypass
        && parent.Outcome
            == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                .NonzeroTestResultBypassesCleanupAt93C0
        && parent.ParentChild.ExactNonzeroParentBypass
        && !parent.ChildReached
        && !parent.Internal942FEntryReached
        && !parent.CleanupCounterReadAttempted
        && !parent.LateFreshCleanupCounterDwordProvidedButUnused
        && parent.ParentChild.Parent.FreshMaskGate is
        {
            TestReached: true,
            JnzReached: true,
            JnzTaken: true,
            JnzSelectedDestinationRawFileOffset: 0x012C93C0,
            StopAtRawFileOffsetExclusive: 0x012C93C0
        } gate
        && gate.TestResult is uint testResult
        && testResult != 0
        && !parent.DestinationEntered
        && parent.StopAtRawFileOffsetExclusive == 0x012C93C0;

    private static CapacityFinalCalleeCommonNodeCleanupEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeCommonNodeCleanupEpilogueLifecycleOutcome outcome,
        uint stopAt) =>
        new(capture, outcome)
        {
            ParentPlan = parent,
            EpiloguePlan = epilogue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
