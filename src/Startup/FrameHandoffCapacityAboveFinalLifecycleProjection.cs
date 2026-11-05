namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveFinalLifecycleCapture(
    FrameHandoffCapacityAboveRepeatCapture AboveRepeat,
    FrameHandoffCapacityFinalCallGateCapture FinalGate,
    FrameHandoffCapacityFinalReturnCapture FinalReturn);

public enum FrameHandoffCapacityAboveFinalLifecycleOutcome
{
    AboveRepeatBlocked,
    AboveRepeatHasUnreachedCaptures,
    FinalGateEntryEbpMismatch,
    FinalGateBlocked,
    FinalGateHasUnreachedCaptures,
    FinalBypassEbxMismatch,
    FinalBypassEsiMismatch,
    FinalEntryEbpMismatch,
    FinalBypassEdiMismatch,
    FinalBlocked,
    FinalHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityAboveFinalLifecyclePlan(
    FrameHandoffCapacityAboveFinalLifecycleCapture Capture,
    FrameHandoffCapacityAboveFinalLifecycleOutcome Outcome,
    FrameHandoffCapacityAboveRepeatPlan AboveRepeatPlan,
    FrameHandoffCapacityFinalCallGatePlan? FinalGatePlan,
    FrameHandoffCapacityFinalLifecyclePlan? FinalLifecyclePlan,
    bool AboveRepeatAdmissionSatisfied,
    bool FinalGateCaptureValidatedJoinDerived,
    bool FinalGateEntryEbpValidated,
    bool FinalGateEntryEbpMatches,
    bool FinalBypassEbxValidated,
    bool FinalBypassEbxMatches,
    bool FinalBypassEsiValidated,
    bool FinalBypassEsiMatches,
    bool FinalOutcomeClean,
    bool UnreachedFinalGateFrameMinus14CaptureProvided,
    bool UnreachedFinalGateReceiver4CaptureProvided,
    bool UnreachedFinalGateNormalCallStatusProvided,
    bool UnreachedFinalReturnReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the above-capacity return/REP projection with the final call gate and return.
/// </summary>
public static class FrameHandoffCapacityAboveFinalLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityAboveRepeatProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFinalLifecycleProjection.LifecycleEndRawFileOffsetExclusive;
    public const uint FinalGateEntryRawFileOffset =
        FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset;
    public const uint FinalReturnEntryRawFileOffset =
        FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset;

    public static FrameHandoffCapacityAboveFinalLifecyclePlan Project(
        FrameHandoffCapacityAboveFinalLifecycleCapture capture)
    {
        FrameHandoffCapacityAboveRepeatPlan aboveRepeatPlan =
            FrameHandoffCapacityAboveRepeatProjection.Project(capture.AboveRepeat);
        bool aboveRepeatAdmissionSatisfied =
            IsExactZeroBranch(aboveRepeatPlan)
            || IsExactSharedRepeatCompletion(aboveRepeatPlan);

        if (!aboveRepeatAdmissionSatisfied)
        {
            bool aboveRepeatHasUnreachedCaptures =
                aboveRepeatPlan.UnreachedGateHelperReturnEaxCaptureProvided
                || aboveRepeatPlan.UnreachedGateFrameMinus4CaptureProvided
                || aboveRepeatPlan.UnreachedRepeatEntryEcxCaptureProvided
                || aboveRepeatPlan.UnreachedRepeatEntryEdiCaptureProvided
                || aboveRepeatPlan.UnreachedDirectionFlagCaptureProvided
                || aboveRepeatPlan.UnreachedRepCompletionCaptureProvided;
            return BuildPlan(
                capture,
                aboveRepeatPlan,
                aboveRepeatHasUnreachedCaptures
                    ? FrameHandoffCapacityAboveFinalLifecycleOutcome.AboveRepeatHasUnreachedCaptures
                    : FrameHandoffCapacityAboveFinalLifecycleOutcome.AboveRepeatBlocked,
                unreachedFinalGateFrameMinus14CaptureProvided:
                    capture.FinalGate.FrameMinus14.HasValue,
                unreachedFinalGateReceiver4CaptureProvided:
                    capture.FinalGate.Receiver4.HasValue,
                unreachedFinalGateNormalCallStatusProvided:
                    capture.FinalGate.NormalCallReturned.HasValue,
                unreachedFinalReturnReceiver8OldCaptureProvided:
                    capture.FinalReturn.Receiver8Old.HasValue,
                stopAt: aboveRepeatPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.FinalGate.EntryEbp != capture.AboveRepeat.AboveReturnGate.EntryEbp)
        {
            return BuildPlan(
                capture,
                aboveRepeatPlan,
                FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateEntryEbpMismatch,
                aboveRepeatAdmissionSatisfied: true,
                finalGateEntryEbpValidated: true,
                unreachedFinalGateFrameMinus14CaptureProvided:
                    capture.FinalGate.FrameMinus14.HasValue,
                unreachedFinalGateReceiver4CaptureProvided:
                    capture.FinalGate.Receiver4.HasValue,
                unreachedFinalGateNormalCallStatusProvided:
                    capture.FinalGate.NormalCallReturned.HasValue,
                unreachedFinalReturnReceiver8OldCaptureProvided:
                    capture.FinalReturn.Receiver8Old.HasValue,
                stopAt: FinalGateEntryRawFileOffset);
        }

        FrameHandoffCapacityFinalCallGateCapture derivedGateCapture =
            capture.FinalGate with { CaptureValidatedJoin = true };
        FrameHandoffCapacityFinalCallGatePlan gatePlan =
            FrameHandoffCapacityFinalCallGateProjection.Project(derivedGateCapture);
        bool gateUnreachedCapturesClear =
            !gatePlan.UnreachedFrameMinus14CaptureProvided
            && !gatePlan.UnreachedReceiver4CaptureProvided
            && !gatePlan.UnreachedNormalCallStatusProvided;
        bool gateOutcomeClean = gatePlan.Outcome is
            FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            or FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned;
        bool finalGateAdmissionSatisfied =
            gatePlan.NormalBoundaryAdmitted && gateOutcomeClean && gateUnreachedCapturesClear;

        if (!finalGateAdmissionSatisfied)
        {
            bool gateHasUnreachedCaptures = !gateUnreachedCapturesClear;
            return BuildPlan(
                capture,
                aboveRepeatPlan,
                gateHasUnreachedCaptures
                    ? FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateHasUnreachedCaptures
                    : FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateBlocked,
                finalGatePlan: gatePlan,
                aboveRepeatAdmissionSatisfied: true,
                finalGateCaptureValidatedJoinDerived: true,
                finalGateEntryEbpValidated: true,
                finalGateEntryEbpMatches: true,
                unreachedFinalGateFrameMinus14CaptureProvided:
                    gatePlan.UnreachedFrameMinus14CaptureProvided,
                unreachedFinalGateReceiver4CaptureProvided:
                    gatePlan.UnreachedReceiver4CaptureProvided,
                unreachedFinalGateNormalCallStatusProvided:
                    gatePlan.UnreachedNormalCallStatusProvided,
                unreachedFinalReturnReceiver8OldCaptureProvided:
                    capture.FinalReturn.Receiver8Old.HasValue,
                stopAt: gatePlan.StopAtRawFileOffsetExclusive);
        }

        bool finalZeroBypass =
            gatePlan.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped;
        if (finalZeroBypass)
        {
            uint? expectedEbx = aboveRepeatPlan.AboveReturnGatePlan.EbxAfterFrameMinus4Read;
            bool ebxMatches =
                expectedEbx is uint expected && capture.FinalReturn.EntryEbx == expected;
            if (!ebxMatches)
            {
                return BuildPlan(
                    capture,
                    aboveRepeatPlan,
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEbxMismatch,
                    finalGatePlan: gatePlan,
                    aboveRepeatAdmissionSatisfied: true,
                    finalGateCaptureValidatedJoinDerived: true,
                    finalGateEntryEbpValidated: true,
                    finalGateEntryEbpMatches: true,
                    finalBypassEbxValidated: true,
                    finalBypassEbxMatches: false,
                    unreachedFinalReturnReceiver8OldCaptureProvided:
                        capture.FinalReturn.Receiver8Old.HasValue,
                    stopAt: FinalReturnEntryRawFileOffset);
            }

            bool esiMatches =
                capture.FinalReturn.EntryEsi == capture.AboveRepeat.AboveReturnGate.EntryEsi;
            if (!esiMatches)
            {
                return BuildPlan(
                    capture,
                    aboveRepeatPlan,
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEsiMismatch,
                    finalGatePlan: gatePlan,
                    aboveRepeatAdmissionSatisfied: true,
                    finalGateCaptureValidatedJoinDerived: true,
                    finalGateEntryEbpValidated: true,
                    finalGateEntryEbpMatches: true,
                    finalBypassEbxValidated: true,
                    finalBypassEbxMatches: true,
                    finalBypassEsiValidated: true,
                    finalBypassEsiMatches: false,
                    unreachedFinalReturnReceiver8OldCaptureProvided:
                        capture.FinalReturn.Receiver8Old.HasValue,
                    stopAt: FinalReturnEntryRawFileOffset);
            }
        }

        FrameHandoffCapacityFinalLifecycleCapture finalLifecycleCapture = new(
            Gate: derivedGateCapture,
            Final: capture.FinalReturn);
        FrameHandoffCapacityFinalLifecyclePlan finalLifecyclePlan =
            FrameHandoffCapacityFinalLifecycleProjection.Project(finalLifecycleCapture);
        FrameHandoffCapacityAboveFinalLifecycleOutcome outcome =
            finalLifecyclePlan.Outcome switch
            {
                FrameHandoffCapacityFinalLifecycleOutcome.FinalEntryEbpMismatch =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalEntryEbpMismatch,
                FrameHandoffCapacityFinalLifecycleOutcome.ZeroBypassEdiMismatch =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEdiMismatch,
                FrameHandoffCapacityFinalLifecycleOutcome.FinalHasUnreachedCaptures =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalHasUnreachedCaptures,
                FrameHandoffCapacityFinalLifecycleOutcome.FinalBlocked =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBlocked,
                FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.ProjectionComplete,
                FrameHandoffCapacityFinalLifecycleOutcome.GateHasUnreachedCaptures =>
                    FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateHasUnreachedCaptures,
                _ => FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateBlocked
            };

        return BuildPlan(
            capture,
            aboveRepeatPlan,
            outcome,
            finalLifecyclePlan.GatePlan,
            finalLifecyclePlan,
            aboveRepeatAdmissionSatisfied: true,
            finalGateCaptureValidatedJoinDerived: true,
            finalGateEntryEbpValidated: true,
            finalGateEntryEbpMatches: true,
            finalBypassEbxValidated: finalZeroBypass,
            finalBypassEbxMatches: !finalZeroBypass || capture.FinalReturn.EntryEbx
                == aboveRepeatPlan.AboveReturnGatePlan.EbxAfterFrameMinus4Read,
            finalBypassEsiValidated: finalZeroBypass,
            finalBypassEsiMatches: !finalZeroBypass
                || capture.FinalReturn.EntryEsi == capture.AboveRepeat.AboveReturnGate.EntryEsi,
            finalOutcomeClean:
                finalLifecyclePlan.Outcome
                    == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
                && finalLifecyclePlan.FinalOutcomeClean,
            unreachedFinalGateFrameMinus14CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedFrameMinus14CaptureProvided,
            unreachedFinalGateReceiver4CaptureProvided:
                finalLifecyclePlan.GatePlan.UnreachedReceiver4CaptureProvided,
            unreachedFinalGateNormalCallStatusProvided:
                finalLifecyclePlan.GatePlan.UnreachedNormalCallStatusProvided,
            unreachedFinalReturnReceiver8OldCaptureProvided:
                finalLifecyclePlan.UnreachedFinalReceiver8OldCaptureProvided,
            stopAt: finalLifecyclePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactZeroBranch(FrameHandoffCapacityAboveRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
        && plan.GateSuccessAdmission
        && plan.GateUnreachedCapturesClear
        && plan.ZeroBranchBypassedRepeat
        && plan.SharedRepeatUnreachedCapturesClear
        && plan.SharedRepeatPlan is null
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveRepeatProjection.ZeroBranchDestinationRawFileOffset
        && plan.AboveReturnGatePlan.Outcome
            == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
        && plan.AboveReturnGatePlan.EntryEsiIsZero == true
        && plan.AboveReturnGatePlan.BranchReached
        && plan.AboveReturnGatePlan.BranchTaken == true
        && plan.AboveReturnGatePlan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveRepeatProjection.ZeroBranchDestinationRawFileOffset
        && !plan.UnreachedGateHelperReturnEaxCaptureProvided
        && !plan.UnreachedGateFrameMinus4CaptureProvided
        && !plan.UnreachedRepeatEntryEcxCaptureProvided
        && !plan.UnreachedRepeatEntryEdiCaptureProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided;

    private static bool IsExactSharedRepeatCompletion(
        FrameHandoffCapacityAboveRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
        && plan.GateSuccessAdmission
        && plan.GateUnreachedCapturesClear
        && plan.RepeatEntryIdentityChecked
        && plan.RepeatEcxIdentityMatches == true
        && plan.RepeatEdiIdentityMatches == true
        && plan.SharedRepeatNormalEntryDerived
        && plan.SharedRepeatCompletionAdmission
        && plan.SharedRepeatUnreachedCapturesClear
        && plan.SharedRepeatPlan is
        {
            Outcome: FrameHandoffCapacitySharedRepeatOutcome.Completed,
            Capture.EntryValidatedNonzero: true,
            EntryValidationAccepted: true,
            XorEaxReached: true,
            EaxValue: 0,
            RepInstructionReached: true,
            DirectionFlagSet: not null,
            AllStoresCompleted: true,
            StoreDescriptor: not null,
            EcxAfterRep: 0,
            EdiAfterRep: not null,
            UnreachedDirectionFlagCaptureProvided: false,
            UnreachedRepCompletionCaptureProvided: false,
            StopAtRawFileOffsetExclusive:
                FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive
        }
        && plan.AboveReturnGatePlan.Outcome
            == FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough
        && plan.AboveReturnGatePlan.EntryEsiIsZero == false
        && plan.AboveReturnGatePlan.SelectedDestinationRawFileOffset
            == FrameHandoffCapacityAboveRepeatProjection.SharedRepeatRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveRepeatProjection.EndRawFileOffsetExclusive
        && !plan.UnreachedGateHelperReturnEaxCaptureProvided
        && !plan.UnreachedGateFrameMinus4CaptureProvided
        && !plan.UnreachedRepeatEntryEcxCaptureProvided
        && !plan.UnreachedRepeatEntryEdiCaptureProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided;

    private static FrameHandoffCapacityAboveFinalLifecyclePlan BuildPlan(
        FrameHandoffCapacityAboveFinalLifecycleCapture capture,
        FrameHandoffCapacityAboveRepeatPlan aboveRepeatPlan,
        FrameHandoffCapacityAboveFinalLifecycleOutcome outcome,
        FrameHandoffCapacityFinalCallGatePlan? finalGatePlan = null,
        FrameHandoffCapacityFinalLifecyclePlan? finalLifecyclePlan = null,
        bool aboveRepeatAdmissionSatisfied = false,
        bool finalGateCaptureValidatedJoinDerived = false,
        bool finalGateEntryEbpValidated = false,
        bool finalGateEntryEbpMatches = false,
        bool finalBypassEbxValidated = false,
        bool finalBypassEbxMatches = false,
        bool finalBypassEsiValidated = false,
        bool finalBypassEsiMatches = false,
        bool finalOutcomeClean = false,
        bool unreachedFinalGateFrameMinus14CaptureProvided = false,
        bool unreachedFinalGateReceiver4CaptureProvided = false,
        bool unreachedFinalGateNormalCallStatusProvided = false,
        bool unreachedFinalReturnReceiver8OldCaptureProvided = false,
        uint? stopAt = null) =>
        new(
            capture,
            outcome,
            aboveRepeatPlan,
            finalGatePlan,
            finalLifecyclePlan,
            aboveRepeatAdmissionSatisfied,
            finalGateCaptureValidatedJoinDerived,
            finalGateEntryEbpValidated,
            finalGateEntryEbpMatches,
            finalBypassEbxValidated,
            finalBypassEbxMatches,
            finalBypassEsiValidated,
            finalBypassEsiMatches,
            finalOutcomeClean,
            unreachedFinalGateFrameMinus14CaptureProvided,
            unreachedFinalGateReceiver4CaptureProvided,
            unreachedFinalGateNormalCallStatusProvided,
            unreachedFinalReturnReceiver8OldCaptureProvided,
            stopAt ?? aboveRepeatPlan.StopAtRawFileOffsetExclusive);
}
