namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityGateRepeatCapture(
    FrameHandoffCapacitySecondReturnGatePlan? GatePlan,
    uint? RepeatEntryEcx,
    uint? RepeatEntryEdx,
    uint? RepeatEntryEbp,
    bool? DirectionFlagSet,
    bool? RepReturnedNormally,
    uint? FreshFrameMinus8Value);

public enum FrameHandoffCapacityGateRepeatOutcome
{
    MissingGatePlan,
    GateBlocked,
    GateHasUnreachedCaptures,
    ZeroBranchBypassedRepeat,
    ZeroBranchHasUnreachedCaptures,
    RepeatEntryIdentityMismatch,
    RepeatBlocked,
    RepeatHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityGateRepeatPlan(
    FrameHandoffCapacityGateRepeatCapture Capture,
    FrameHandoffCapacityGateRepeatOutcome Outcome,
    bool GateAdmissionChecked,
    bool NonzeroGateAdmission,
    bool ZeroJoinAdmission,
    bool GateUnreachedCapturesClear,
    FrameHandoffCapacityRepeatStorePlan? RepeatPlan,
    uint? DerivedEntryEcx,
    uint? DerivedEntryEdx,
    uint? DerivedEntryEbp,
    bool RepeatEntryIdentitiesValidated,
    bool? RepeatEntryEcxMatches,
    bool? RepeatEntryEdxMatches,
    bool? RepeatEntryEbpMatches,
    bool UnreachedRepeatEntryEcxProvided,
    bool UnreachedRepeatEntryEdxProvided,
    bool UnreachedRepeatEntryEbpProvided,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Routes the second-return gate's nonzero fallthrough into REP STOSD, or preserves its zero join.
/// </summary>
public static class FrameHandoffCapacityGateRepeatProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset;
    public const uint RepeatEntryRawFileOffset =
        FrameHandoffCapacityRepeatStoreProjection.EntryRawFileOffset;
    public const uint ZeroJoinRawFileOffset =
        FrameHandoffCapacitySecondReturnGateProjection.ZeroBranchDestinationRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityRepeatStoreProjection.EndRawFileOffsetExclusive;

    public static FrameHandoffCapacityGateRepeatPlan Project(
        FrameHandoffCapacityGateRepeatCapture capture)
    {
        if (capture.GatePlan is not FrameHandoffCapacitySecondReturnGatePlan gatePlan)
        {
            return Stop(
                capture,
                FrameHandoffCapacityGateRepeatOutcome.MissingGatePlan,
                gateAdmissionChecked: true,
                gateUnreachedCapturesClear: false,
                unreachedRepeatEntryEcxProvided: capture.RepeatEntryEcx.HasValue,
                unreachedRepeatEntryEdxProvided: capture.RepeatEntryEdx.HasValue,
                unreachedRepeatEntryEbpProvided: capture.RepeatEntryEbp.HasValue,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: LifecycleStartRawFileOffset);
        }

        bool gateUnreachedCapturesClear =
            !gatePlan.UnreachedFrameMinusCCaptureProvided
            && !gatePlan.UnreachedHelperReturnEaxCaptureProvided;
        if (gatePlan.Outcome
            == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken)
        {
            bool exactZeroBranch =
                gatePlan.Capture.ValidatedNormalEntry
                && gatePlan.FrameMinusCIsZero == true
                && gatePlan.BranchReached
                && gatePlan.BranchTaken == true
                && gatePlan.SelectedDestinationRawFileOffset == ZeroJoinRawFileOffset
                && gatePlan.StopAtRawFileOffsetExclusive == ZeroJoinRawFileOffset
                && gatePlan.Capture.FrameMinusC == 0
                && gatePlan.Capture.HelperReturnEax.HasValue
                && gatePlan.FrameMinusCCaptured
                && gatePlan.StackCleanupReached
                && gatePlan.EdxLeaReached
                && gatePlan.EdxAfterLea.HasValue
                && gatePlan.HelperReturnEaxConsumed
                && gatePlan.TestReached;
            if (!exactZeroBranch)
            {
                return Stop(
                    capture,
                    FrameHandoffCapacityGateRepeatOutcome.GateBlocked,
                    gateAdmissionChecked: true,
                    gateUnreachedCapturesClear: gateUnreachedCapturesClear,
                    unreachedRepeatEntryEcxProvided: capture.RepeatEntryEcx.HasValue,
                    unreachedRepeatEntryEdxProvided: capture.RepeatEntryEdx.HasValue,
                    unreachedRepeatEntryEbpProvided: capture.RepeatEntryEbp.HasValue,
                    unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                    unreachedRepCompletionCaptureProvided:
                        capture.RepReturnedNormally.HasValue,
                    unreachedFreshFrameMinus8CaptureProvided:
                        capture.FreshFrameMinus8Value.HasValue,
                    stopAt: gatePlan.StopAtRawFileOffsetExclusive);
            }

            bool repeatCapturesClear =
                !capture.RepeatEntryEcx.HasValue
                && !capture.RepeatEntryEdx.HasValue
                && !capture.RepeatEntryEbp.HasValue
                && !capture.DirectionFlagSet.HasValue
                && !capture.RepReturnedNormally.HasValue
                && !capture.FreshFrameMinus8Value.HasValue;
            bool zeroJoinAdmission = gateUnreachedCapturesClear && repeatCapturesClear;
            return Stop(
                capture,
                zeroJoinAdmission
                    ? FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
                    : gateUnreachedCapturesClear
                        ? FrameHandoffCapacityGateRepeatOutcome.ZeroBranchHasUnreachedCaptures
                        : FrameHandoffCapacityGateRepeatOutcome.GateHasUnreachedCaptures,
                gateAdmissionChecked: true,
                gateUnreachedCapturesClear: gateUnreachedCapturesClear,
                zeroJoinAdmission: zeroJoinAdmission,
                unreachedRepeatEntryEcxProvided: capture.RepeatEntryEcx.HasValue,
                unreachedRepeatEntryEdxProvided: capture.RepeatEntryEdx.HasValue,
                unreachedRepeatEntryEbpProvided: capture.RepeatEntryEbp.HasValue,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: ZeroJoinRawFileOffset);
        }

        bool exactNonzeroBranch =
            gatePlan.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough
            && gatePlan.Capture.ValidatedNormalEntry
            && gatePlan.Capture.FrameMinusC is uint frameMinusC
            && frameMinusC != 0
            && gatePlan.FrameMinusCIsZero == false
            && gatePlan.BranchReached
            && gatePlan.BranchTaken == false
            && gatePlan.SelectedDestinationRawFileOffset == RepeatEntryRawFileOffset
            && gatePlan.StopAtRawFileOffsetExclusive == RepeatEntryRawFileOffset
            && gatePlan.Capture.HelperReturnEax.HasValue
            && gatePlan.FrameMinusCCaptured
            && gatePlan.StackCleanupReached
            && gatePlan.EdxLeaReached
            && gatePlan.EdxAfterLea.HasValue
            && gatePlan.HelperReturnEaxConsumed
            && gatePlan.TestReached;
        if (!exactNonzeroBranch || !gateUnreachedCapturesClear)
        {
            return Stop(
                capture,
                gateUnreachedCapturesClear
                    ? FrameHandoffCapacityGateRepeatOutcome.GateBlocked
                    : FrameHandoffCapacityGateRepeatOutcome.GateHasUnreachedCaptures,
                gateAdmissionChecked: true,
                gateUnreachedCapturesClear: gateUnreachedCapturesClear,
                unreachedRepeatEntryEcxProvided: capture.RepeatEntryEcx.HasValue,
                unreachedRepeatEntryEdxProvided: capture.RepeatEntryEdx.HasValue,
                unreachedRepeatEntryEbpProvided: capture.RepeatEntryEbp.HasValue,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: gatePlan.StopAtRawFileOffsetExclusive);
        }

        uint entryEcx = gatePlan.Capture.FrameMinusC!.Value;
        uint entryEdx = gatePlan.EdxAfterLea!.Value;
        uint entryEbp = gatePlan.Capture.EntryEbp;
        bool? entryEcxMatches = capture.RepeatEntryEcx is uint repeatEntryEcx
            ? repeatEntryEcx == entryEcx
            : null;
        bool? entryEdxMatches = capture.RepeatEntryEdx is uint repeatEntryEdx
            ? repeatEntryEdx == entryEdx
            : null;
        bool? entryEbpMatches = capture.RepeatEntryEbp is uint repeatEntryEbp
            ? repeatEntryEbp == entryEbp
            : null;

        if (entryEcxMatches == false || entryEdxMatches == false || entryEbpMatches == false)
        {
            return Stop(
                capture,
                FrameHandoffCapacityGateRepeatOutcome.RepeatEntryIdentityMismatch,
                gateAdmissionChecked: true,
                gateUnreachedCapturesClear: true,
                nonzeroGateAdmission: true,
                derivedEntryEcx: entryEcx,
                derivedEntryEdx: entryEdx,
                derivedEntryEbp: entryEbp,
                repeatEntryIdentitiesValidated: true,
                repeatEntryEcxMatches: entryEcxMatches,
                repeatEntryEdxMatches: entryEdxMatches,
                repeatEntryEbpMatches: entryEbpMatches,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: RepeatEntryRawFileOffset);
        }

        var repeatCapture = new FrameHandoffCapacityRepeatStoreCapture(
            EntryEcxValidatedNonzero: true,
            EntryEdx: entryEdx,
            EntryEcx: entryEcx,
            EntryEbp: entryEbp,
            DirectionFlagSet: capture.DirectionFlagSet,
            RepReturnedNormally: capture.RepReturnedNormally,
            FreshFrameMinus8Value: capture.FreshFrameMinus8Value);
        FrameHandoffCapacityRepeatStorePlan repeatPlan =
            FrameHandoffCapacityRepeatStoreProjection.Project(repeatCapture);
        bool repeatUnreachedCapturesClear =
            !repeatPlan.UnreachedDirectionFlagCaptureProvided
            && !repeatPlan.UnreachedRepCompletionCaptureProvided
            && !repeatPlan.UnreachedFreshFrameMinus8CaptureProvided;
        bool repeatCompleted =
            repeatPlan.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && repeatUnreachedCapturesClear;
        bool repeatBlocked =
            repeatPlan.Outcome != FrameHandoffCapacityRepeatStoreOutcome.Completed;
        FrameHandoffCapacityGateRepeatOutcome outcome =
            repeatCompleted
                ? FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
                : repeatBlocked
                    ? FrameHandoffCapacityGateRepeatOutcome.RepeatBlocked
                    : FrameHandoffCapacityGateRepeatOutcome.RepeatHasUnreachedCaptures;

        return new(
            capture,
            outcome,
            GateAdmissionChecked: true,
            NonzeroGateAdmission: true,
            ZeroJoinAdmission: false,
            GateUnreachedCapturesClear: true,
            repeatPlan,
            entryEcx,
            entryEdx,
            entryEbp,
            RepeatEntryIdentitiesValidated: true,
            RepeatEntryEcxMatches: entryEcxMatches,
            RepeatEntryEdxMatches: entryEdxMatches,
            RepeatEntryEbpMatches: entryEbpMatches,
            UnreachedRepeatEntryEcxProvided: false,
            UnreachedRepeatEntryEdxProvided: false,
            UnreachedRepeatEntryEbpProvided: false,
            repeatPlan.UnreachedDirectionFlagCaptureProvided,
            repeatPlan.UnreachedRepCompletionCaptureProvided,
            repeatPlan.UnreachedFreshFrameMinus8CaptureProvided,
            repeatPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityGateRepeatPlan Stop(
        FrameHandoffCapacityGateRepeatCapture capture,
        FrameHandoffCapacityGateRepeatOutcome outcome,
        bool gateAdmissionChecked = false,
        bool nonzeroGateAdmission = false,
        bool zeroJoinAdmission = false,
        bool gateUnreachedCapturesClear = true,
        uint? derivedEntryEcx = null,
        uint? derivedEntryEdx = null,
        uint? derivedEntryEbp = null,
        bool repeatEntryIdentitiesValidated = false,
        bool? repeatEntryEcxMatches = null,
        bool? repeatEntryEdxMatches = null,
        bool? repeatEntryEbpMatches = null,
        bool unreachedRepeatEntryEcxProvided = false,
        bool unreachedRepeatEntryEdxProvided = false,
        bool unreachedRepeatEntryEbpProvided = false,
        bool unreachedDirectionFlagCaptureProvided = false,
        bool unreachedRepCompletionCaptureProvided = false,
        bool unreachedFreshFrameMinus8CaptureProvided = false,
        uint stopAt = LifecycleStartRawFileOffset) =>
        new(
            capture,
            outcome,
            gateAdmissionChecked,
            nonzeroGateAdmission,
            zeroJoinAdmission,
            gateUnreachedCapturesClear,
            RepeatPlan: null,
            derivedEntryEcx,
            derivedEntryEdx,
            derivedEntryEbp,
            repeatEntryIdentitiesValidated,
            repeatEntryEcxMatches,
            repeatEntryEdxMatches,
            repeatEntryEbpMatches,
            unreachedRepeatEntryEcxProvided,
            unreachedRepeatEntryEdxProvided,
            unreachedRepeatEntryEbpProvided,
            unreachedDirectionFlagCaptureProvided,
            unreachedRepCompletionCaptureProvided,
            unreachedFreshFrameMinus8CaptureProvided,
            stopAt);
}
