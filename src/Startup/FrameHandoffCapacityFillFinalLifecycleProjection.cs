namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFillFinalLifecycleCapture(
    FrameHandoffCapacityFillSharedRepeatCapture Fill,
    FrameHandoffCapacityFinalLifecycleCapture Final);

public enum FrameHandoffCapacityFillFinalLifecycleOutcome
{
    FillRouteNotAdmitted,
    FinalGateEntryEbpMismatch,
    FinalGateBlocked,
    FinalGateHasUnreachedCaptures,
    FinalBypassEbxMismatch,
    FinalIdentityMismatch,
    FinalBlocked,
    FinalHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityFillFinalLifecyclePlan(
    FrameHandoffCapacityFillFinalLifecycleCapture Capture,
    FrameHandoffCapacityFillFinalLifecycleOutcome Outcome,
    FrameHandoffCapacityFillSharedRepeatPlan FillPlan,
    FrameHandoffCapacityFinalLifecyclePlan? FinalLifecyclePlan,
    FrameHandoffCapacityFinalCallGatePlan? FinalGatePlan,
    bool FillOutcomeClean,
    bool FillAdmissionSatisfied,
    bool FinalGateEntryEbpValidated,
    bool FinalGateEntryEbpMatches,
    bool DeriveFinalGateCaptureValidatedJoin,
    bool FinalBypassEbxValidated,
    bool FinalBypassEbxMatches,
    bool DeriveFinalEntryEbxFromFillJoin,
    bool FinalLifecycleOutcomeClean,
    bool UnreachedFinalFrameMinus14CaptureProvided,
    bool UnreachedFinalReceiver4CaptureProvided,
    bool UnreachedFinalCallStatusCaptureProvided,
    bool UnreachedFinalReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the fill/shared-repeat route with the final gate and return lifecycle.
/// </summary>
public static class FrameHandoffCapacityFillFinalLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityFillFinalLifecyclePlan Project(
        FrameHandoffCapacityFillFinalLifecycleCapture capture)
    {
        FrameHandoffCapacityFillSharedRepeatPlan fillPlan =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(capture.Fill);
        bool fillOutcomeClean = IsExactCleanFillRoute(fillPlan);
        if (!fillOutcomeClean)
        {
            return StopBeforeFinalGate(
                capture,
                fillPlan,
                FrameHandoffCapacityFillFinalLifecycleOutcome.FillRouteNotAdmitted,
                fillOutcomeClean: false,
                fillAdmissionSatisfied: false,
                stopAt: fillPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.Final.Gate.EntryEbp != capture.Fill.FillJoinCapture.EntryEbp)
        {
            return StopBeforeFinalGate(
                capture,
                fillPlan,
                FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateEntryEbpMismatch,
                fillOutcomeClean: true,
                fillAdmissionSatisfied: true,
                finalGateEntryEbpValidated: true,
                finalGateEntryEbpMatches: false,
                stopAt: FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityFinalLifecycleCapture derivedFinalCapture =
            capture.Final with
            {
                Gate = capture.Final.Gate with { CaptureValidatedJoin = true }
            };
        FrameHandoffCapacityFinalCallGatePlan gatePlan =
            FrameHandoffCapacityFinalCallGateProjection.Project(derivedFinalCapture.Gate);
        bool exactZeroFinalGate =
            gatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && gatePlan.NormalBoundaryAdmitted
            && !gatePlan.UnreachedFrameMinus14CaptureProvided
            && !gatePlan.UnreachedReceiver4CaptureProvided
            && !gatePlan.UnreachedNormalCallStatusProvided;
        uint? fillFrameMinus4 = fillPlan.FillJoinPlan.EbxAfterFrameMinus4Read;
        if (exactZeroFinalGate
            && fillFrameMinus4 is uint derivedEbx
            && derivedFinalCapture.Final.EntryEbx != derivedEbx)
        {
            return StopBeforeFinalWrites(
                capture,
                fillPlan,
                gatePlan,
                finalGateEntryEbpMatches: true,
                finalBypassEbxValidated: true,
                finalBypassEbxMatches: false);
        }

        bool deriveFinalEntryEbxFromFillJoin = exactZeroFinalGate;
        if (deriveFinalEntryEbxFromFillJoin && fillFrameMinus4 is uint entryEbx)
        {
            derivedFinalCapture = derivedFinalCapture with
            {
                Final = derivedFinalCapture.Final with { EntryEbx = entryEbx }
            };
        }

        FrameHandoffCapacityFinalLifecyclePlan finalLifecyclePlan =
            FrameHandoffCapacityFinalLifecycleProjection.Project(derivedFinalCapture);
        FrameHandoffCapacityFillFinalLifecycleOutcome outcome =
            finalLifecyclePlan.Outcome switch
            {
                FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete =>
                    FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete,
                FrameHandoffCapacityFinalLifecycleOutcome.GateBlocked =>
                    FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateBlocked,
                FrameHandoffCapacityFinalLifecycleOutcome.GateHasUnreachedCaptures =>
                    FrameHandoffCapacityFillFinalLifecycleOutcome.FinalGateHasUnreachedCaptures,
                FrameHandoffCapacityFinalLifecycleOutcome.FinalEntryEbpMismatch
                    or FrameHandoffCapacityFinalLifecycleOutcome.ZeroBypassEdiMismatch =>
                    FrameHandoffCapacityFillFinalLifecycleOutcome.FinalIdentityMismatch,
                FrameHandoffCapacityFinalLifecycleOutcome.FinalHasUnreachedCaptures =>
                    FrameHandoffCapacityFillFinalLifecycleOutcome.FinalHasUnreachedCaptures,
                _ => FrameHandoffCapacityFillFinalLifecycleOutcome.FinalBlocked
            };

        return new(
            capture,
            outcome,
            fillPlan,
            finalLifecyclePlan,
            finalLifecyclePlan.GatePlan,
            FillOutcomeClean: true,
            FillAdmissionSatisfied: true,
            FinalGateEntryEbpValidated: true,
            FinalGateEntryEbpMatches: true,
            DeriveFinalGateCaptureValidatedJoin: true,
            FinalBypassEbxValidated: exactZeroFinalGate,
            FinalBypassEbxMatches: exactZeroFinalGate,
            DeriveFinalEntryEbxFromFillJoin: deriveFinalEntryEbxFromFillJoin,
            FinalLifecycleOutcomeClean:
                finalLifecyclePlan.Outcome
                    == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete,
            UnreachedFinalFrameMinus14CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedFrameMinus14CaptureProvided,
            UnreachedFinalReceiver4CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedReceiver4CaptureProvided,
            UnreachedFinalCallStatusCaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedNormalCallStatusProvided,
            UnreachedFinalReceiver8OldCaptureProvided:
                finalLifecyclePlan.UnreachedFinalReceiver8OldCaptureProvided,
            StopAtRawFileOffsetExclusive:
                finalLifecyclePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanFillRoute(
        FrameHandoffCapacityFillSharedRepeatPlan plan)
    {
        FrameHandoffCapacityFillJoinPlan joinPlan = plan.FillJoinPlan;
        bool frameMinus4ReadClean =
            joinPlan.Capture.CaptureValidatedJoin
            && joinPlan.FrameMinus4ReadReached
            && joinPlan.FrameMinus4ReadCaptured
            && joinPlan.EbxAfterFrameMinus4Read.HasValue
            && !joinPlan.UnreachedFrameMinus4ReadCaptureProvided;
        bool noUnreachedRepeatCaptures =
            !plan.UnreachedExpectedRepeatEntryEcxCaptureProvided
            && !plan.UnreachedExpectedRepeatEntryEdiCaptureProvided
            && !plan.UnreachedDirectionFlagCaptureProvided
            && !plan.UnreachedRepCompletionCaptureProvided;

        if (plan.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && joinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && frameMinus4ReadClean
            && joinPlan.EntryEdiTestReached
            && joinPlan.EntryEdiZeroFlag == true
            && joinPlan.ZeroBranchReached
            && joinPlan.ZeroBranchTaken == true
            && joinPlan.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && !joinPlan.EcxEntryEdiMoveReached
            && !joinPlan.EdiLoadedEbxMoveReached
            && !joinPlan.JumpReached
            && !plan.RepeatEntryIdentityValidationReached
            && !plan.RepeatEntryValidatedNonzero
            && plan.RepeatPlan is null
            && noUnreachedRepeatCaptures
            && plan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset)
        {
            return true;
        }

        if (plan.Outcome
                != FrameHandoffCapacityFillSharedRepeatOutcome.Completed
            || joinPlan.Outcome
                != FrameHandoffCapacityFillJoinOutcome
                    .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            || !frameMinus4ReadClean
            || !joinPlan.EntryEdiTestReached
            || joinPlan.EntryEdiZeroFlag != false
            || !joinPlan.ZeroBranchReached
            || joinPlan.ZeroBranchTaken != false
            || joinPlan.BranchTargetRawFileOffset is not null
            || !joinPlan.EcxEntryEdiMoveReached
            || joinPlan.EcxAfterMove is not uint repeatEntryEcx
            || repeatEntryEcx == 0
            || !joinPlan.EdiLoadedEbxMoveReached
            || joinPlan.EdiAfterMove != joinPlan.EbxAfterFrameMinus4Read
            || !joinPlan.JumpReached
            || joinPlan.JumpTargetRawFileOffset
                != FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
            || !plan.RepeatEntryIdentityValidationReached
            || !plan.RepeatEntryValidatedNonzero
            || plan.RepeatEntryEcx != repeatEntryEcx
            || plan.RepeatEntryEdi != joinPlan.EdiAfterMove
            || plan.RepeatPlan is not FrameHandoffCapacitySharedRepeatPlan repeatPlan
            || repeatPlan.Outcome != FrameHandoffCapacitySharedRepeatOutcome.Completed
            || repeatPlan.Capture.EntryEcx != repeatEntryEcx
            || repeatPlan.Capture.EntryEdi != joinPlan.EdiAfterMove
            || !repeatPlan.EntryValidationAccepted
            || !repeatPlan.XorEaxReached
            || repeatPlan.EaxValue != 0
            || !repeatPlan.RepInstructionReached
            || repeatPlan.DirectionFlagSet is null
            || repeatPlan.Capture.RepCompletedNormally != true
            || !repeatPlan.AllStoresCompleted
            || !repeatPlan.StoreDescriptor.HasValue
            || repeatPlan.EcxAfterRep != 0
            || repeatPlan.EdiAfterRep is null
            || repeatPlan.UnreachedDirectionFlagCaptureProvided
            || repeatPlan.UnreachedRepCompletionCaptureProvided
            || !noUnreachedRepeatCaptures
            || plan.StopAtRawFileOffsetExclusive
                != FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive)
        {
            return false;
        }

        return true;
    }

    private static FrameHandoffCapacityFillFinalLifecyclePlan StopBeforeFinalGate(
        FrameHandoffCapacityFillFinalLifecycleCapture capture,
        FrameHandoffCapacityFillSharedRepeatPlan fillPlan,
        FrameHandoffCapacityFillFinalLifecycleOutcome outcome,
        bool fillOutcomeClean,
        bool fillAdmissionSatisfied,
        bool finalGateEntryEbpValidated = false,
        bool finalGateEntryEbpMatches = false,
        uint stopAt = LifecycleStartRawFileOffset)
    {
        FrameHandoffCapacityFinalLifecycleCapture unvalidatedFinalCapture =
            capture.Final with
            {
                Gate = capture.Final.Gate with { CaptureValidatedJoin = false }
            };
        FrameHandoffCapacityFinalLifecyclePlan finalLifecyclePlan =
            FrameHandoffCapacityFinalLifecycleProjection.Project(unvalidatedFinalCapture);
        return new(
            capture,
            outcome,
            fillPlan,
            finalLifecyclePlan,
            finalLifecyclePlan.GatePlan,
            fillOutcomeClean,
            fillAdmissionSatisfied,
            finalGateEntryEbpValidated,
            finalGateEntryEbpMatches,
            DeriveFinalGateCaptureValidatedJoin: false,
            FinalBypassEbxValidated: false,
            FinalBypassEbxMatches: false,
            DeriveFinalEntryEbxFromFillJoin: false,
            FinalLifecycleOutcomeClean: false,
            UnreachedFinalFrameMinus14CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedFrameMinus14CaptureProvided,
            UnreachedFinalReceiver4CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedReceiver4CaptureProvided,
            UnreachedFinalCallStatusCaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedNormalCallStatusProvided,
            UnreachedFinalReceiver8OldCaptureProvided:
                finalLifecyclePlan.UnreachedFinalReceiver8OldCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
    }

    private static FrameHandoffCapacityFillFinalLifecyclePlan StopBeforeFinalWrites(
        FrameHandoffCapacityFillFinalLifecycleCapture capture,
        FrameHandoffCapacityFillSharedRepeatPlan fillPlan,
        FrameHandoffCapacityFinalCallGatePlan gatePlan,
        bool finalGateEntryEbpMatches,
        bool finalBypassEbxValidated,
        bool finalBypassEbxMatches)
    {
        return new(
            capture,
            FrameHandoffCapacityFillFinalLifecycleOutcome.FinalBypassEbxMismatch,
            fillPlan,
            null,
            gatePlan,
            FillOutcomeClean: true,
            FillAdmissionSatisfied: true,
            FinalGateEntryEbpValidated: true,
            FinalGateEntryEbpMatches: finalGateEntryEbpMatches,
            DeriveFinalGateCaptureValidatedJoin: true,
            FinalBypassEbxValidated: finalBypassEbxValidated,
            FinalBypassEbxMatches: finalBypassEbxMatches,
            DeriveFinalEntryEbxFromFillJoin: false,
            FinalLifecycleOutcomeClean: false,
            UnreachedFinalFrameMinus14CaptureProvided:
                gatePlan.UnreachedFrameMinus14CaptureProvided,
            UnreachedFinalReceiver4CaptureProvided:
                gatePlan.UnreachedReceiver4CaptureProvided,
            UnreachedFinalCallStatusCaptureProvided:
                gatePlan.UnreachedNormalCallStatusProvided,
            UnreachedFinalReceiver8OldCaptureProvided:
                capture.Final.Final.Receiver8Old.HasValue,
            StopAtRawFileOffsetExclusive:
                FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);
    }
}
