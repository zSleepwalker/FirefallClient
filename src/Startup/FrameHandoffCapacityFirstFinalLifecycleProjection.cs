namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFirstFinalLifecycleCapture(
    FrameHandoffCapacityRepeatFillJoinCapture RepeatFillJoin,
    FrameHandoffCapacityFinalLifecycleCapture Final,
    uint? ExpectedSharedRepeatEntryEcx = null,
    uint? ExpectedSharedRepeatEntryEdi = null,
    bool? SharedRepeatDirectionFlagSet = null,
    bool? SharedRepeatCompletedNormally = null);

public enum FrameHandoffCapacityFirstFinalLifecycleOutcome
{
    RepeatFillJoinNotAdmitted,
    RepeatFillJoinHasUnreachedCaptures,
    FillFinalRouteNotAdmitted,
    FinalGateEntryEbpMismatch,
    FinalGateBlocked,
    FinalGateHasUnreachedCaptures,
    FinalBypassEbxMismatch,
    FinalIdentityMismatch,
    FinalBlocked,
    FinalHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityFirstFinalLifecyclePlan(
    FrameHandoffCapacityFirstFinalLifecycleCapture Capture,
    FrameHandoffCapacityFirstFinalLifecycleOutcome Outcome,
    FrameHandoffCapacityRepeatFillJoinPlan RepeatFillJoinPlan,
    FrameHandoffCapacityFillFinalLifecyclePlan? FillFinalLifecyclePlan,
    FrameHandoffCapacityFillJoinCapture? DerivedFillFinalFillJoinCapture,
    bool RepeatFillJoinAdmissionSatisfied,
    bool FillFinalProjectionReached,
    bool UnreachedSharedRepeatEntryEcxCaptureProvided,
    bool UnreachedSharedRepeatEntryEdiCaptureProvided,
    bool UnreachedSharedRepeatDirectionFlagCaptureProvided,
    bool UnreachedSharedRepeatCompletionCaptureProvided,
    bool UnreachedFinalFrameMinus14CaptureProvided,
    bool UnreachedFinalReceiver4CaptureProvided,
    bool UnreachedFinalCallStatusCaptureProvided,
    bool UnreachedFinalReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Routes the exact capacity REP/fill-join outcomes into the shared-repeat and final lifecycle projections.
/// The nested plans describe sequential projections of captures, not repeated native execution.
/// </summary>
public static class FrameHandoffCapacityFirstFinalLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityGateRepeatProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityFirstFinalLifecyclePlan Project(
        FrameHandoffCapacityFirstFinalLifecycleCapture capture)
    {
        FrameHandoffCapacityRepeatFillJoinPlan repeatFillJoinPlan =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(capture.RepeatFillJoin);

        if (!IsExactCleanRepeatFillJoin(repeatFillJoinPlan))
        {
            bool unreachedRepeatCaptures = HasUnreachedCaptures(
                capture,
                repeatFillJoinPlan);
            return StopBeforeFillFinal(
                capture,
                repeatFillJoinPlan,
                unreachedRepeatCaptures
                    ? FrameHandoffCapacityFirstFinalLifecycleOutcome
                        .RepeatFillJoinHasUnreachedCaptures
                    : FrameHandoffCapacityFirstFinalLifecycleOutcome
                        .RepeatFillJoinNotAdmitted,
                stopAt: repeatFillJoinPlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffCapacityFillJoinCapture derivedFillJoinCapture =
            repeatFillJoinPlan.FillJoinPlan!.Value.Capture;
        var fillFinalCapture = new FrameHandoffCapacityFillFinalLifecycleCapture(
            Fill: new FrameHandoffCapacityFillSharedRepeatCapture(
                FillJoinCapture: derivedFillJoinCapture,
                ExpectedRepeatEntryEcx: capture.ExpectedSharedRepeatEntryEcx,
                ExpectedRepeatEntryEdi: capture.ExpectedSharedRepeatEntryEdi,
                DirectionFlagSet: capture.SharedRepeatDirectionFlagSet,
                RepCompletedNormally: capture.SharedRepeatCompletedNormally),
            Final: capture.Final);
        FrameHandoffCapacityFillFinalLifecyclePlan fillFinalPlan =
            FrameHandoffCapacityFillFinalLifecycleProjection.Project(fillFinalCapture);

        return new(
            capture,
            MapOutcome(fillFinalPlan.Outcome),
            repeatFillJoinPlan,
            fillFinalPlan,
            derivedFillJoinCapture,
            RepeatFillJoinAdmissionSatisfied: true,
            FillFinalProjectionReached: true,
            UnreachedSharedRepeatEntryEcxCaptureProvided:
                fillFinalPlan.FillPlan.UnreachedExpectedRepeatEntryEcxCaptureProvided,
            UnreachedSharedRepeatEntryEdiCaptureProvided:
                fillFinalPlan.FillPlan.UnreachedExpectedRepeatEntryEdiCaptureProvided,
            UnreachedSharedRepeatDirectionFlagCaptureProvided:
                fillFinalPlan.FillPlan.UnreachedDirectionFlagCaptureProvided,
            UnreachedSharedRepeatCompletionCaptureProvided:
                fillFinalPlan.FillPlan.UnreachedRepCompletionCaptureProvided,
            UnreachedFinalFrameMinus14CaptureProvided:
                fillFinalPlan.UnreachedFinalFrameMinus14CaptureProvided,
            UnreachedFinalReceiver4CaptureProvided:
                fillFinalPlan.UnreachedFinalReceiver4CaptureProvided,
            UnreachedFinalCallStatusCaptureProvided:
                fillFinalPlan.UnreachedFinalCallStatusCaptureProvided,
            UnreachedFinalReceiver8OldCaptureProvided:
                fillFinalPlan.UnreachedFinalReceiver8OldCaptureProvided,
            fillFinalPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanRepeatFillJoin(
        FrameHandoffCapacityRepeatFillJoinPlan plan)
    {
        if (!plan.ExactFillJoinOutcome
            || !plan.NoUnreachedFillJoinCaptures
            || !plan.GateRepeatAdmissionValidated
            || !plan.GateRepeatPlan.GateUnreachedCapturesClear
            || !plan.GateRepeatPlan.GateAdmissionChecked
            || plan.GateRepeatPlan.UnreachedRepeatEntryEcxProvided
            || plan.GateRepeatPlan.UnreachedRepeatEntryEdxProvided
            || plan.GateRepeatPlan.UnreachedRepeatEntryEbpProvided
            || plan.GateRepeatPlan.UnreachedDirectionFlagCaptureProvided
            || plan.GateRepeatPlan.UnreachedRepCompletionCaptureProvided
            || plan.GateRepeatPlan.UnreachedFreshFrameMinus8CaptureProvided
            || plan.UnreachedFillJoinEntryEdiCaptureProvided
            || plan.UnreachedFillJoinFrameMinus4ReadCaptureProvided
            || !plan.FillJoinAdmissionValidated
            || plan.FillJoinPlan is not FrameHandoffCapacityFillJoinPlan fillJoinPlan
            || fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided
            || !fillJoinPlan.Capture.CaptureValidatedJoin
            || !fillJoinPlan.FrameMinus4ReadReached
            || !fillJoinPlan.FrameMinus4ReadCaptured
            || fillJoinPlan.EbxAfterFrameMinus4Read is null)
        {
            return false;
        }

        bool cleanFirstGateZeroBypass =
            plan.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && plan.GateRepeatPlan.ZeroJoinAdmission
            && !plan.GateRepeatPlan.NonzeroGateAdmission
            && plan.GateRepeatPlan.RepeatPlan is null;
        bool cleanFirstGateCompletedRepeat =
            plan.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && plan.GateRepeatPlan.NonzeroGateAdmission
            && !plan.GateRepeatPlan.ZeroJoinAdmission
            && plan.GateRepeatPlan.RepeatPlan is FrameHandoffCapacityRepeatStorePlan firstRepeat
            && firstRepeat.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && firstRepeat.EntryValidationAccepted
            && firstRepeat.AllStoresCompleted
            && firstRepeat.DirectionFlagSet is not null
            && firstRepeat.EcxAfterRep == 0
            && firstRepeat.FreshFrameMinus8ReadReached
            && firstRepeat.FreshFrameMinus8Value.HasValue
            && !firstRepeat.UnreachedDirectionFlagCaptureProvided
            && !firstRepeat.UnreachedRepCompletionCaptureProvided
            && !firstRepeat.UnreachedFreshFrameMinus8CaptureProvided;

        if (!cleanFirstGateZeroBypass && !cleanFirstGateCompletedRepeat)
        {
            return false;
        }

        if (plan.Outcome
            == FrameHandoffCapacityRepeatFillJoinOutcome
                .ZeroEntryEdiBranchToRawFileOffsetBE6CA2)
        {
            return (cleanFirstGateZeroBypass || cleanFirstGateCompletedRepeat)
                && plan.ZeroBranchBypassAdmission != plan.NonzeroRepeatAdmission
                && fillJoinPlan.Outcome
                    == FrameHandoffCapacityFillJoinOutcome
                        .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
                && fillJoinPlan.EntryEdiTestReached
                && fillJoinPlan.EntryEdiZeroFlag == true
                && fillJoinPlan.ZeroBranchReached
                && fillJoinPlan.ZeroBranchTaken == true
                && fillJoinPlan.BranchTargetRawFileOffset
                    == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
                && !fillJoinPlan.EcxEntryEdiMoveReached
                && !fillJoinPlan.EdiLoadedEbxMoveReached
                && !fillJoinPlan.JumpReached;
        }

        if (plan.Outcome
            != FrameHandoffCapacityRepeatFillJoinOutcome
                .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E)
        {
            return false;
        }

        return (cleanFirstGateZeroBypass || cleanFirstGateCompletedRepeat)
            && plan.NonzeroRepeatAdmission != plan.ZeroBranchBypassAdmission
            && fillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome
                    .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && fillJoinPlan.EntryEdiTestReached
            && fillJoinPlan.EntryEdiZeroFlag == false
            && fillJoinPlan.ZeroBranchReached
            && fillJoinPlan.ZeroBranchTaken == false
            && fillJoinPlan.BranchTargetRawFileOffset is null
            && fillJoinPlan.EcxAfterMove == plan.DerivedFillJoinEntryEdi
            && fillJoinPlan.EcxEntryEdiMoveReached
            && fillJoinPlan.EdiAfterMove == fillJoinPlan.EbxAfterFrameMinus4Read
            && fillJoinPlan.EdiLoadedEbxMoveReached
            && fillJoinPlan.JumpReached
            && fillJoinPlan.JumpTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset;
    }

    private static FrameHandoffCapacityFirstFinalLifecycleOutcome MapOutcome(
        FrameHandoffCapacityFillFinalLifecycleOutcome outcome) =>
        outcome switch
        {
            FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FillFinalRouteNotAdmitted,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateEntryEbpMismatch =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalGateEntryEbpMismatch,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateBlocked =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalGateBlocked,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateHasUnreachedCaptures =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalGateHasUnreachedCaptures,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalBypassEbxMismatch =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalBypassEbxMismatch,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalIdentityMismatch =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalIdentityMismatch,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalBlocked =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalBlocked,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalHasUnreachedCaptures =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalHasUnreachedCaptures,
            FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete =>
                FrameHandoffCapacityFirstFinalLifecycleOutcome.ProjectionComplete,
            _ => FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalBlocked
        };

    private static bool HasUnreachedCaptures(
        FrameHandoffCapacityFirstFinalLifecycleCapture capture,
        FrameHandoffCapacityRepeatFillJoinPlan plan) =>
        capture.ExpectedSharedRepeatEntryEcx.HasValue
        || capture.ExpectedSharedRepeatEntryEdi.HasValue
        || capture.SharedRepeatDirectionFlagSet.HasValue
        || capture.SharedRepeatCompletedNormally.HasValue
        || plan.UnreachedFillJoinEntryEdiCaptureProvided
        || plan.UnreachedFillJoinFrameMinus4ReadCaptureProvided
        || plan.GateRepeatPlan.UnreachedRepeatEntryEcxProvided
        || plan.GateRepeatPlan.UnreachedRepeatEntryEdxProvided
        || plan.GateRepeatPlan.UnreachedRepeatEntryEbpProvided
        || plan.GateRepeatPlan.UnreachedDirectionFlagCaptureProvided
        || plan.GateRepeatPlan.UnreachedRepCompletionCaptureProvided
        || plan.GateRepeatPlan.UnreachedFreshFrameMinus8CaptureProvided
        || (plan.GateRepeatPlan.RepeatPlan is FrameHandoffCapacityRepeatStorePlan repeatPlan
            && (repeatPlan.UnreachedDirectionFlagCaptureProvided
                || repeatPlan.UnreachedRepCompletionCaptureProvided
                || repeatPlan.UnreachedFreshFrameMinus8CaptureProvided))
        || (plan.FillJoinPlan is FrameHandoffCapacityFillJoinPlan fillJoinPlan
            && fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided);

    private static FrameHandoffCapacityFirstFinalLifecyclePlan StopBeforeFillFinal(
        FrameHandoffCapacityFirstFinalLifecycleCapture capture,
        FrameHandoffCapacityRepeatFillJoinPlan repeatFillJoinPlan,
        FrameHandoffCapacityFirstFinalLifecycleOutcome outcome,
        uint stopAt) =>
        new(
            capture,
            outcome,
            repeatFillJoinPlan,
            FillFinalLifecyclePlan: null,
            DerivedFillFinalFillJoinCapture: null,
            RepeatFillJoinAdmissionSatisfied: false,
            FillFinalProjectionReached: false,
            UnreachedSharedRepeatEntryEcxCaptureProvided:
                capture.ExpectedSharedRepeatEntryEcx.HasValue,
            UnreachedSharedRepeatEntryEdiCaptureProvided:
                capture.ExpectedSharedRepeatEntryEdi.HasValue,
            UnreachedSharedRepeatDirectionFlagCaptureProvided:
                capture.SharedRepeatDirectionFlagSet.HasValue,
            UnreachedSharedRepeatCompletionCaptureProvided:
                capture.SharedRepeatCompletedNormally.HasValue,
            UnreachedFinalFrameMinus14CaptureProvided:
                capture.Final.Gate.FrameMinus14.HasValue,
            UnreachedFinalReceiver4CaptureProvided:
                capture.Final.Gate.Receiver4.HasValue,
            UnreachedFinalCallStatusCaptureProvided:
                capture.Final.Gate.NormalCallReturned.HasValue,
            UnreachedFinalReceiver8OldCaptureProvided:
                capture.Final.Final.Receiver8Old.HasValue,
            stopAt);
}
