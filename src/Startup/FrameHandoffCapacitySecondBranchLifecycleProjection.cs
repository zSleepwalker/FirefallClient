namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySecondBranchLifecycleCapture(
    FrameHandoffCapacitySecondTransferLifecycleCapture SecondTransfer,
    FrameHandoffCapacityFirstFinalLifecycleCapture FirstFinal);

public enum FrameHandoffCapacitySecondBranchLifecycleOutcome
{
    SecondTransferBlocked,
    SecondTransferHasUnreachedCaptures,
    SecondReturnEntryIdentityMismatch,
    SecondReturnBlocked,
    SecondReturnHasUnreachedCaptures,
    FirstFinalNotAdmitted,
    FirstFinalHasUnreachedCaptures,
    FirstFinalBlocked,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacitySecondBranchLifecyclePlan(
    FrameHandoffCapacitySecondBranchLifecycleCapture Capture,
    FrameHandoffCapacitySecondBranchLifecycleOutcome Outcome,
    FrameHandoffCapacitySecondTransferLifecyclePlan SecondTransferPlan,
    FrameHandoffCapacityFirstFinalLifecycleCapture DerivedFirstFinalCapture,
    FrameHandoffCapacityFirstFinalLifecyclePlan FirstFinalPlan,
    FrameHandoffCapacitySecondReturnGatePlan? ForwardedReturnGatePlan,
    bool SecondTransferAdmissionValidated,
    bool SecondTransferSuccessAdmission,
    bool SecondReturnBranchAdmissionValidated,
    bool ExactReturnGatePlanForwarded,
    bool FirstFinalAdmissionValidated,
    bool FirstFinalOutcomeClean,
    bool UnreachedRepeatEntryEcxCaptureProvided,
    bool UnreachedRepeatEntryEdxCaptureProvided,
    bool UnreachedRepeatEntryEbpCaptureProvided,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFillJoinEntryEdiCaptureProvided,
    bool UnreachedFillJoinFrameMinus4CaptureProvided,
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
/// Composes the second transfer and return gate with the remaining capacity lifecycle.
/// </summary>
public static class FrameHandoffCapacitySecondBranchLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacitySecondTransferLifecycleProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFirstFinalLifecycleProjection.LifecycleEndRawFileOffsetExclusive;

    public static FrameHandoffCapacitySecondBranchLifecyclePlan Project(
        FrameHandoffCapacitySecondBranchLifecycleCapture capture)
    {
        FrameHandoffCapacitySecondTransferLifecyclePlan secondTransferPlan =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(capture.SecondTransfer);
        bool returnBranchAdmission = IsCleanReturnBranch(secondTransferPlan);
        FrameHandoffCapacitySecondReturnGatePlan? forwardedReturnGatePlan =
            returnBranchAdmission ? secondTransferPlan.ReturnGatePlan : null;

        FrameHandoffCapacityFirstFinalLifecycleCapture derivedFirstFinalCapture =
            capture.FirstFinal with
            {
                RepeatFillJoin = capture.FirstFinal.RepeatFillJoin with
                {
                    GateRepeatCapture =
                        capture.FirstFinal.RepeatFillJoin.GateRepeatCapture with
                        {
                            GatePlan = forwardedReturnGatePlan
                        }
                }
            };

        FrameHandoffCapacityFirstFinalLifecyclePlan firstFinalPlan =
            FrameHandoffCapacityFirstFinalLifecycleProjection.Project(
                derivedFirstFinalCapture);
        bool exactReturnGatePlanForwarded =
            returnBranchAdmission
            && forwardedReturnGatePlan is FrameHandoffCapacitySecondReturnGatePlan sourcePlan
            && derivedFirstFinalCapture.RepeatFillJoin.GateRepeatCapture.GatePlan
                is FrameHandoffCapacitySecondReturnGatePlan derivedPlan
            && sourcePlan == derivedPlan
            && firstFinalPlan.Capture.RepeatFillJoin.GateRepeatCapture.GatePlan
                is FrameHandoffCapacitySecondReturnGatePlan plannedGatePlan
            && sourcePlan == plannedGatePlan
            && firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan.Capture.GatePlan
                is FrameHandoffCapacitySecondReturnGatePlan projectedGatePlan
            && sourcePlan == projectedGatePlan;

        bool firstFinalAdmissionValidated =
            returnBranchAdmission
            && exactReturnGatePlanForwarded
            && firstFinalPlan.RepeatFillJoinAdmissionSatisfied
            && firstFinalPlan.RepeatFillJoinPlan.ExactFillJoinOutcome
            && firstFinalPlan.RepeatFillJoinPlan.NoUnreachedFillJoinCaptures
            && firstFinalPlan.FillFinalProjectionReached
            && firstFinalPlan.FillFinalLifecyclePlan
                is FrameHandoffCapacityFillFinalLifecyclePlan fillFinalPlan
            && fillFinalPlan.FillAdmissionSatisfied
            && fillFinalPlan.FillOutcomeClean
            && fillFinalPlan.FinalGateEntryEbpValidated
            && fillFinalPlan.FinalGateEntryEbpMatches
            && fillFinalPlan.FinalLifecyclePlan
                is FrameHandoffCapacityFinalLifecyclePlan finalLifecyclePlan
            && finalLifecyclePlan.NormalBoundaryAdmissionSatisfied
            && finalLifecyclePlan.GateUnreachedCapturesClear
            && finalLifecyclePlan.FinalEntryEbpValidated
            && finalLifecyclePlan.FinalEntryEbpMatches
            && finalLifecyclePlan.DeriveValidatedNormalTail
            && (!finalLifecyclePlan.ZeroBypassEdiValidated
                || finalLifecyclePlan.ZeroBypassEdiMatches)
            && !firstFinalPlan.UnreachedSharedRepeatEntryEcxCaptureProvided
            && !firstFinalPlan.UnreachedSharedRepeatEntryEdiCaptureProvided
            && !firstFinalPlan.UnreachedSharedRepeatDirectionFlagCaptureProvided
            && !firstFinalPlan.UnreachedSharedRepeatCompletionCaptureProvided
            && !firstFinalPlan.UnreachedFinalFrameMinus14CaptureProvided
            && !firstFinalPlan.UnreachedFinalReceiver4CaptureProvided
            && !firstFinalPlan.UnreachedFinalCallStatusCaptureProvided;
        bool firstFinalOutcomeClean =
            firstFinalAdmissionValidated
            && firstFinalPlan.Outcome
                == FrameHandoffCapacityFirstFinalLifecycleOutcome.ProjectionComplete
            && firstFinalPlan.FillFinalLifecyclePlan
                is FrameHandoffCapacityFillFinalLifecyclePlan completeFillFinalPlan
            && completeFillFinalPlan.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete
            && completeFillFinalPlan.FinalLifecycleOutcomeClean
            && completeFillFinalPlan.FinalLifecyclePlan
                is FrameHandoffCapacityFinalLifecyclePlan completedFinalLifecyclePlan
            && completedFinalLifecyclePlan.FinalOutcomeClean
            && !firstFinalPlan.UnreachedFinalReceiver8OldCaptureProvided;

        bool transferSuccessAdmission = secondTransferPlan.TransferSuccessAdmission
            && secondTransferPlan.TransferAdmissionChecked
            && secondTransferPlan.TransferUnreachedCapturesClear;
        bool transferUnreachedCapturesClear =
            secondTransferPlan.TransferUnreachedCapturesClear;
        bool returnGateUnreachedCapturesClear =
            secondTransferPlan.ReturnGatePlan is FrameHandoffCapacitySecondReturnGatePlan
                returnGatePlan
            && !returnGatePlan.UnreachedFrameMinusCCaptureProvided
            && !returnGatePlan.UnreachedHelperReturnEaxCaptureProvided;

        FrameHandoffCapacitySecondBranchLifecycleOutcome outcome =
            DetermineOutcome(
                secondTransferPlan,
                returnBranchAdmission,
                firstFinalPlan,
                firstFinalOutcomeClean,
                transferUnreachedCapturesClear,
                returnGateUnreachedCapturesClear);
        uint stopAt = outcome == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            || IsFirstFinalOutcome(outcome)
                ? firstFinalPlan.StopAtRawFileOffsetExclusive
                : secondTransferPlan.StopAtRawFileOffsetExclusive;

        return new(
            capture,
            outcome,
            secondTransferPlan,
            derivedFirstFinalCapture,
            firstFinalPlan,
            forwardedReturnGatePlan,
            SecondTransferAdmissionValidated: secondTransferPlan.TransferAdmissionChecked,
            transferSuccessAdmission,
            returnBranchAdmission,
            exactReturnGatePlanForwarded,
            firstFinalAdmissionValidated,
            firstFinalOutcomeClean,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedRepeatEntryEcxProvided,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedRepeatEntryEdxProvided,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedRepeatEntryEbpProvided,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedDirectionFlagCaptureProvided,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedRepCompletionCaptureProvided,
            firstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan
                .UnreachedFreshFrameMinus8CaptureProvided,
            firstFinalPlan.RepeatFillJoinPlan
                .UnreachedFillJoinEntryEdiCaptureProvided,
            firstFinalPlan.RepeatFillJoinPlan
                .UnreachedFillJoinFrameMinus4ReadCaptureProvided,
            firstFinalPlan.UnreachedSharedRepeatEntryEcxCaptureProvided,
            firstFinalPlan.UnreachedSharedRepeatEntryEdiCaptureProvided,
            firstFinalPlan.UnreachedSharedRepeatDirectionFlagCaptureProvided,
            firstFinalPlan.UnreachedSharedRepeatCompletionCaptureProvided,
            firstFinalPlan.UnreachedFinalFrameMinus14CaptureProvided,
            firstFinalPlan.UnreachedFinalReceiver4CaptureProvided,
            firstFinalPlan.UnreachedFinalCallStatusCaptureProvided,
            firstFinalPlan.UnreachedFinalReceiver8OldCaptureProvided,
            stopAt);
    }

    private static bool IsCleanReturnBranch(
        FrameHandoffCapacitySecondTransferLifecyclePlan lifecyclePlan)
    {
        if (lifecyclePlan.Outcome
                != FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            || !lifecyclePlan.TransferAdmissionChecked
            || !lifecyclePlan.TransferSuccessAdmission
            || !lifecyclePlan.TransferUnreachedCapturesClear
            || !lifecyclePlan.ReturnEntryIdentityValidated
            || !lifecyclePlan.ReturnEntryEbpMatches
            || !lifecyclePlan.ReturnBranchAdmitted
            || lifecyclePlan.ReturnGatePlan
                is not FrameHandoffCapacitySecondReturnGatePlan gatePlan
            || !gatePlan.Capture.ValidatedNormalEntry
            || gatePlan.UnreachedFrameMinusCCaptureProvided
            || gatePlan.UnreachedHelperReturnEaxCaptureProvided
            || !gatePlan.FrameMinusCReadReached
            || !gatePlan.FrameMinusCCaptured
            || !gatePlan.StackCleanupReached
            || !gatePlan.EdxLeaReached
            || gatePlan.EdxAfterLea is null
            || !gatePlan.HelperReturnEaxConsumed
            || !gatePlan.TestReached
            || !gatePlan.BranchReached)
        {
            return false;
        }

        return gatePlan.Outcome switch
        {
            FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken =>
                gatePlan.Capture.FrameMinusC == 0
                && gatePlan.FrameMinusCIsZero == true
                && gatePlan.BranchTaken == true
                && gatePlan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && gatePlan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset,
            FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough =>
                gatePlan.Capture.FrameMinusC is uint frameMinusC
                && frameMinusC != 0
                && gatePlan.FrameMinusCIsZero == false
                && gatePlan.BranchTaken == false
                && gatePlan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && gatePlan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive,
            _ => false
        };
    }

    private static FrameHandoffCapacitySecondBranchLifecycleOutcome DetermineOutcome(
        FrameHandoffCapacitySecondTransferLifecyclePlan secondTransferPlan,
        bool returnBranchAdmission,
        FrameHandoffCapacityFirstFinalLifecyclePlan firstFinalPlan,
        bool firstFinalOutcomeClean,
        bool transferUnreachedCapturesClear,
        bool returnGateUnreachedCapturesClear)
    {
        if (secondTransferPlan.Outcome is
            FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferBlocked
            or FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferHasUnreachedCaptures)
        {
            return !transferUnreachedCapturesClear
                || secondTransferPlan.Outcome
                    == FrameHandoffCapacitySecondTransferLifecycleOutcome
                        .TransferHasUnreachedCaptures
                || secondTransferPlan.UnreachedReturnFrameMinusCCaptureProvided
                || secondTransferPlan.UnreachedReturnHelperReturnEaxCaptureProvided
                ? FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .SecondTransferHasUnreachedCaptures
                : FrameHandoffCapacitySecondBranchLifecycleOutcome.SecondTransferBlocked;
        }

        if (secondTransferPlan.Outcome
            == FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnEntryIdentityMismatch)
        {
            return FrameHandoffCapacitySecondBranchLifecycleOutcome
                .SecondReturnEntryIdentityMismatch;
        }

        if (secondTransferPlan.Outcome is
            FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnBlocked
            or FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnHasUnreachedCaptures)
        {
            return !returnGateUnreachedCapturesClear
                || secondTransferPlan.UnreachedReturnFrameMinusCCaptureProvided
                || secondTransferPlan.UnreachedReturnHelperReturnEaxCaptureProvided
                || secondTransferPlan.Outcome
                    == FrameHandoffCapacitySecondTransferLifecycleOutcome
                        .ReturnHasUnreachedCaptures
                ? FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .SecondReturnHasUnreachedCaptures
                : FrameHandoffCapacitySecondBranchLifecycleOutcome.SecondReturnBlocked;
        }

        if (!returnBranchAdmission)
        {
            return !returnGateUnreachedCapturesClear
                || secondTransferPlan.Outcome
                    == FrameHandoffCapacitySecondTransferLifecycleOutcome
                        .ReturnHasUnreachedCaptures
                ? FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .SecondReturnHasUnreachedCaptures
                : FrameHandoffCapacitySecondBranchLifecycleOutcome.SecondReturnBlocked;
        }

        if (firstFinalOutcomeClean)
        {
            return FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete;
        }

        return firstFinalPlan.Outcome is
            FrameHandoffCapacityFirstFinalLifecycleOutcome.RepeatFillJoinHasUnreachedCaptures
            or FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalGateHasUnreachedCaptures
            or FrameHandoffCapacityFirstFinalLifecycleOutcome.FinalHasUnreachedCaptures
                ? FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .FirstFinalHasUnreachedCaptures
                : firstFinalPlan.Outcome is
                    FrameHandoffCapacityFirstFinalLifecycleOutcome.RepeatFillJoinNotAdmitted
                    or FrameHandoffCapacityFirstFinalLifecycleOutcome.FillFinalRouteNotAdmitted
                    ? FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalNotAdmitted
                    : FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalBlocked;
    }

    private static bool IsFirstFinalOutcome(
        FrameHandoffCapacitySecondBranchLifecycleOutcome outcome) =>
        outcome is
            FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalNotAdmitted
            or FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalHasUnreachedCaptures
            or FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalBlocked;
}
