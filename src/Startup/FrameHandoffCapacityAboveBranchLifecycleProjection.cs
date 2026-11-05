namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveBranchLifecycleCapture(
    FrameHandoffCapacityAboveLifecycleCapture AboveLifecycle,
    FrameHandoffCapacityAboveRepeatCapture AboveRepeat,
    FrameHandoffCapacityFinalCallGateCapture FinalGate,
    FrameHandoffCapacityFinalReturnCapture FinalReturn);

public enum FrameHandoffCapacityAboveBranchLifecycleOutcome
{
    AboveLifecycleBlocked,
    AboveLifecycleHasUnreachedCaptures,
    AboveLifecycleNotClean,
    AboveFinalLifecycleBlocked,
    AboveFinalLifecycleHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityAboveBranchLifecyclePlan(
    FrameHandoffCapacityAboveBranchLifecycleCapture Capture,
    FrameHandoffCapacityAboveBranchLifecycleOutcome Outcome,
    FrameHandoffCapacityAboveLifecyclePlan AboveLifecyclePlan,
    FrameHandoffCapacityAboveFinalLifecycleCapture? DerivedAboveFinalLifecycleCapture,
    FrameHandoffCapacityAboveFinalLifecyclePlan? AboveFinalLifecyclePlan,
    bool AboveLifecycleAdmissionSatisfied,
    bool AboveLifecycleUnreachedCapturesClear,
    bool? SuppliedAboveRepeatReturnGateMatchesDerived,
    bool AboveFinalLifecycleAdmissionSatisfied,
    bool UnreachedAboveRepeatEntryEcxCaptureProvided,
    bool UnreachedAboveRepeatEntryEdiCaptureProvided,
    bool UnreachedAboveRepeatDirectionFlagCaptureProvided,
    bool UnreachedAboveRepeatRepCompletionCaptureProvided,
    bool UnreachedFinalGateFrameMinus14CaptureProvided,
    bool UnreachedFinalGateReceiver4CaptureProvided,
    bool UnreachedFinalGateNormalCallStatusProvided,
    bool UnreachedFinalGateEntryEbpIdentityCaptureProvided,
    bool UnreachedFinalReturnEntryEbpCaptureProvided,
    bool UnreachedFinalReturnEntryEdiCaptureProvided,
    bool UnreachedFinalReturnEntryEbxCaptureProvided,
    bool UnreachedFinalReturnEntryEsiCaptureProvided,
    bool UnreachedFinalReturnReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the above-capacity transfer lifecycle with its REP and final-return tail.
/// The repeated return-gate capture is composition evidence; the admitted above-lifecycle
/// plan supplies the authoritative gate capture to the downstream scalar projection.
/// </summary>
public static class FrameHandoffCapacityAboveBranchLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityAboveLifecycleProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityAboveFinalLifecycleProjection.LifecycleEndRawFileOffsetExclusive;

    public static FrameHandoffCapacityAboveBranchLifecyclePlan Project(
        FrameHandoffCapacityAboveBranchLifecycleCapture capture)
    {
        FrameHandoffCapacityAboveLifecyclePlan aboveLifecyclePlan =
            FrameHandoffCapacityAboveLifecycleProjection.Project(capture.AboveLifecycle);
        bool aboveLifecycleUnreachedCapturesClear =
            AboveLifecycleUnreachedCapturesClear(aboveLifecyclePlan);
        bool aboveLifecycleAdmissionSatisfied =
            IsExactCleanAboveLifecycle(aboveLifecyclePlan)
            && aboveLifecycleUnreachedCapturesClear;

        if (!aboveLifecycleAdmissionSatisfied)
        {
            bool aboveLifecycleHasUnreachedCaptures =
                !aboveLifecycleUnreachedCapturesClear
                || HasUnreachedBranchTailValues(capture);
            FrameHandoffCapacityAboveBranchLifecycleOutcome outcome =
                aboveLifecycleHasUnreachedCaptures
                    ? FrameHandoffCapacityAboveBranchLifecycleOutcome
                        .AboveLifecycleHasUnreachedCaptures
                    : aboveLifecyclePlan.Outcome
                        == FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
                        ? FrameHandoffCapacityAboveBranchLifecycleOutcome.AboveLifecycleNotClean
                        : FrameHandoffCapacityAboveBranchLifecycleOutcome.AboveLifecycleBlocked;

            return BuildPlan(
                capture,
                aboveLifecyclePlan,
                outcome,
                aboveLifecycleUnreachedCapturesClear,
                unreachedAboveRepeatEntryEcxCaptureProvided:
                    capture.AboveRepeat.RepeatEntryEcx.HasValue,
                unreachedAboveRepeatEntryEdiCaptureProvided:
                    capture.AboveRepeat.RepeatEntryEdi.HasValue,
                unreachedAboveRepeatDirectionFlagCaptureProvided:
                    capture.AboveRepeat.DirectionFlagSet.HasValue,
                unreachedAboveRepeatRepCompletionCaptureProvided:
                    capture.AboveRepeat.RepCompletedNormally.HasValue,
                unreachedFinalGateFrameMinus14CaptureProvided:
                    capture.FinalGate.FrameMinus14.HasValue,
                unreachedFinalGateReceiver4CaptureProvided:
                    capture.FinalGate.Receiver4.HasValue,
                unreachedFinalGateNormalCallStatusProvided:
                    capture.FinalGate.NormalCallReturned.HasValue,
                unreachedFinalGateEntryEbpIdentityCaptureProvided: true,
                unreachedFinalReturnEntryEbpCaptureProvided: true,
                unreachedFinalReturnEntryEdiCaptureProvided: true,
                unreachedFinalReturnEntryEbxCaptureProvided: true,
                unreachedFinalReturnEntryEsiCaptureProvided: true,
                unreachedFinalReturnReceiver8OldCaptureProvided:
                    capture.FinalReturn.Receiver8Old.HasValue,
                stopAt: aboveLifecyclePlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffCapacityAboveReturnGateCapture admittedReturnGateCapture =
            aboveLifecyclePlan.AboveReturnGatePlan.Value.Capture;
        bool suppliedGateMatchesDerived =
            capture.AboveRepeat.AboveReturnGate == admittedReturnGateCapture;
        FrameHandoffCapacityAboveRepeatCapture derivedAboveRepeatCapture =
            capture.AboveRepeat with { AboveReturnGate = admittedReturnGateCapture };
        FrameHandoffCapacityAboveFinalLifecycleCapture derivedFinalLifecycleCapture = new(
            derivedAboveRepeatCapture,
            capture.FinalGate,
            capture.FinalReturn);
        FrameHandoffCapacityAboveFinalLifecyclePlan aboveFinalLifecyclePlan =
            FrameHandoffCapacityAboveFinalLifecycleProjection.Project(
                derivedFinalLifecycleCapture);
        bool aboveFinalLifecycleAdmissionSatisfied =
            IsExactCleanAboveFinalLifecycle(aboveFinalLifecyclePlan);

        FrameHandoffCapacityAboveBranchLifecycleOutcome finalOutcome =
            aboveFinalLifecycleAdmissionSatisfied
                ? FrameHandoffCapacityAboveBranchLifecycleOutcome.ProjectionComplete
                : AboveFinalLifecycleUnreachedCapturesClear(aboveFinalLifecyclePlan)
                    ? FrameHandoffCapacityAboveBranchLifecycleOutcome.AboveFinalLifecycleBlocked
                    : FrameHandoffCapacityAboveBranchLifecycleOutcome
                        .AboveFinalLifecycleHasUnreachedCaptures;

        return BuildPlan(
            capture,
            aboveLifecyclePlan,
            finalOutcome,
            aboveLifecycleUnreachedCapturesClear,
            derivedFinalLifecycleCapture: derivedFinalLifecycleCapture,
            aboveFinalLifecyclePlan: aboveFinalLifecyclePlan,
            aboveLifecycleAdmissionSatisfied: aboveLifecycleAdmissionSatisfied,
            suppliedAboveRepeatReturnGateMatchesDerived: suppliedGateMatchesDerived,
            aboveFinalLifecycleAdmissionSatisfied: aboveFinalLifecycleAdmissionSatisfied,
            unreachedAboveRepeatEntryEcxCaptureProvided:
                aboveFinalLifecyclePlan.AboveRepeatPlan
                    .UnreachedRepeatEntryEcxCaptureProvided,
            unreachedAboveRepeatEntryEdiCaptureProvided:
                aboveFinalLifecyclePlan.AboveRepeatPlan
                    .UnreachedRepeatEntryEdiCaptureProvided,
            unreachedAboveRepeatDirectionFlagCaptureProvided:
                aboveFinalLifecyclePlan.AboveRepeatPlan
                    .UnreachedDirectionFlagCaptureProvided,
            unreachedAboveRepeatRepCompletionCaptureProvided:
                aboveFinalLifecyclePlan.AboveRepeatPlan
                    .UnreachedRepCompletionCaptureProvided,
            unreachedFinalGateFrameMinus14CaptureProvided:
                aboveFinalLifecyclePlan.UnreachedFinalGateFrameMinus14CaptureProvided,
            unreachedFinalGateReceiver4CaptureProvided:
                aboveFinalLifecyclePlan.UnreachedFinalGateReceiver4CaptureProvided,
            unreachedFinalGateNormalCallStatusProvided:
                aboveFinalLifecyclePlan.UnreachedFinalGateNormalCallStatusProvided,
            unreachedFinalGateEntryEbpIdentityCaptureProvided:
                !aboveFinalLifecyclePlan.FinalGateEntryEbpValidated,
            unreachedFinalReturnEntryEbpCaptureProvided:
                !aboveFinalLifecyclePlan.FinalLifecyclePlan.HasValue
                || !aboveFinalLifecyclePlan.FinalLifecyclePlan.Value.FinalEntryEbpValidated,
            unreachedFinalReturnEntryEdiCaptureProvided:
                !aboveFinalLifecyclePlan.FinalLifecyclePlan.HasValue
                || !aboveFinalLifecyclePlan.FinalLifecyclePlan.Value.FinalEntryEbpValidated
                || !aboveFinalLifecyclePlan.FinalLifecyclePlan.Value.FinalEntryEbpMatches,
            unreachedFinalReturnEntryEbxCaptureProvided:
                !FinalReturnEntryRegistersReached(aboveFinalLifecyclePlan)
                && !aboveFinalLifecyclePlan.FinalBypassEbxValidated,
            unreachedFinalReturnEntryEsiCaptureProvided:
                !FinalReturnEntryRegistersReached(aboveFinalLifecyclePlan)
                && !aboveFinalLifecyclePlan.FinalBypassEsiValidated,
            unreachedFinalReturnReceiver8OldCaptureProvided:
                aboveFinalLifecyclePlan.UnreachedFinalReturnReceiver8OldCaptureProvided,
            stopAt: aboveFinalLifecyclePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanAboveLifecycle(
        FrameHandoffCapacityAboveLifecyclePlan plan)
    {
        if (plan.Outcome != FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
            || !plan.AboveTransferAdmissionChecked
            || !plan.AboveTransferSuccessAdmission
            || !plan.AboveTransferUnreachedCapturesClear
            || !plan.AboveSecondTransferNormalEntryDerived
            || !plan.AboveSecondTransferSuccessAdmission
            || !plan.AboveSecondTransferUnreachedCapturesClear
            || !plan.ReturnGateNormalEntryDerived
            || !plan.ReturnGateFrameEbpIdentityValidated
            || !plan.ReturnGateFrameEbpMatches
            || !plan.ReturnGateSuccessAdmission
            || !plan.ReturnGateUnreachedCapturesClear
            || plan.AboveSecondTransferPlan is not { } secondPlan
            || plan.AboveReturnGatePlan is not { } gatePlan
            || plan.UnreachedAboveSecondTransferReceiver4CaptureProvided
            || plan.UnreachedAboveSecondTransferFrameMinusCCaptureProvided
            || plan.UnreachedAboveSecondTransferFrameMinus4CaptureProvided
            || plan.UnreachedAboveSecondTransferCallReturnCaptureProvided
            || plan.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
            || plan.UnreachedAboveReturnGateFrameMinus4CaptureProvided)
        {
            return false;
        }

        FrameHandoffCapacityAboveTransferPlan firstPlan = plan.AboveTransferPlan;
        return firstPlan.Capture.ValidatedBranchEntry
            && firstPlan.Outcome == FrameHandoffCapacityAboveTransferOutcome.ReturnedNormally
            && firstPlan.CallReached
            && firstPlan.CallReturnedNormally == true
            && firstPlan.NormalBoundaryReached
            && !firstPlan.UnreachedCallCellCaptureProvided
            && !firstPlan.UnreachedCallReturnCaptureProvided
            && firstPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveTransferProjection
                    .NormalBoundaryRawFileOffsetExclusive
            && secondPlan.Capture.ValidatedNormalEntry
            && secondPlan.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.ReturnedNormally
            && secondPlan.CallReached
            && secondPlan.CallReturnedNormally == true
            && secondPlan.NormalBoundaryReached
            && !secondPlan.UnreachedReceiver4CaptureProvided
            && !secondPlan.UnreachedFrameMinusCCaptureProvided
            && !secondPlan.UnreachedFrameMinus4CaptureProvided
            && !secondPlan.UnreachedCallReturnCaptureProvided
            && secondPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveSecondTransferProjection
                    .NormalBoundaryRawFileOffsetExclusive
            && IsExactReturnGateSuccess(gatePlan);
    }

    private static bool IsExactReturnGateSuccess(
        FrameHandoffCapacityAboveReturnGatePlan plan)
    {
        if (!plan.Capture.ValidatedNormalEntry
            || plan.Capture.HelperReturnEax is not uint helperReturnEax
            || plan.Capture.FrameMinus4 is not uint frameMinus4
            || !plan.StackCleanupReached
            || !plan.EdiLeaReached
            || !plan.HelperReturnEaxConsumed
            || !plan.FrameMinus4ReadReached
            || !plan.FrameMinus4Captured
            || !plan.TestReached
            || !plan.BranchReached
            || plan.Reads.Length != 1
            || plan.Reads[0]
                != new FrameHandoffCapacityAboveReturnGateRead(
                    FrameHandoffCapacityAboveReturnGateProjection.FrameMinus4ReadRawFileOffset,
                    FrameHandoffCapacityAboveReturnGateProjection.FrameMinus4Address(
                        plan.Capture.EntryEbp),
                    frameMinus4)
            || plan.EdiAfterLea
                != FrameHandoffCapacityAboveReturnGateProjection
                    .AddEntryEbxAndHelperReturnEax(plan.Capture.EntryEbx, helperReturnEax)
            || plan.EbxAfterFrameMinus4Read != frameMinus4
            || plan.UnreachedHelperReturnEaxCaptureProvided
            || plan.UnreachedFrameMinus4CaptureProvided)
        {
            return false;
        }

        return plan.Outcome switch
        {
            FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken =>
                plan.Capture.EntryEsi == 0
                && plan.EntryEsiIsZero == true
                && plan.BranchTaken == true
                && plan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && !plan.EcxMoveReached
                && plan.EcxAfterMove is null,
            FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough =>
                plan.Capture.EntryEsi != 0
                && plan.EntryEsiIsZero == false
                && plan.BranchTaken == false
                && plan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && plan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && plan.EcxMoveReached
                && plan.EcxAfterMove == plan.Capture.EntryEsi,
            _ => false
        };
    }

    private static bool AboveLifecycleUnreachedCapturesClear(
        FrameHandoffCapacityAboveLifecyclePlan plan) =>
        plan.AboveTransferUnreachedCapturesClear
        && plan.AboveSecondTransferUnreachedCapturesClear
        && plan.ReturnGateUnreachedCapturesClear
        && !plan.UnreachedAboveSecondTransferReceiver4CaptureProvided
        && !plan.UnreachedAboveSecondTransferFrameMinusCCaptureProvided
        && !plan.UnreachedAboveSecondTransferFrameMinus4CaptureProvided
        && !plan.UnreachedAboveSecondTransferCallReturnCaptureProvided
        && !plan.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
        && !plan.UnreachedAboveReturnGateFrameMinus4CaptureProvided
        && !plan.AboveTransferPlan.UnreachedCallCellCaptureProvided
        && !plan.AboveTransferPlan.UnreachedCallReturnCaptureProvided
        && (plan.AboveSecondTransferPlan is not { } secondPlan
            || !secondPlan.UnreachedReceiver4CaptureProvided
                && !secondPlan.UnreachedFrameMinusCCaptureProvided
                && !secondPlan.UnreachedFrameMinus4CaptureProvided
                && !secondPlan.UnreachedCallReturnCaptureProvided)
        && (plan.AboveReturnGatePlan is not { } gatePlan
            || !gatePlan.UnreachedHelperReturnEaxCaptureProvided
                && !gatePlan.UnreachedFrameMinus4CaptureProvided);

    private static bool HasUnreachedBranchTailValues(
        FrameHandoffCapacityAboveBranchLifecycleCapture capture) =>
        capture.AboveRepeat.RepeatEntryEcx.HasValue
        || capture.AboveRepeat.RepeatEntryEdi.HasValue
        || capture.AboveRepeat.DirectionFlagSet.HasValue
        || capture.AboveRepeat.RepCompletedNormally.HasValue
        || capture.FinalGate.FrameMinus14.HasValue
        || capture.FinalGate.Receiver4.HasValue
        || capture.FinalGate.NormalCallReturned.HasValue
        || capture.FinalReturn.Receiver8Old.HasValue;

    private static bool IsExactCleanAboveFinalLifecycle(
        FrameHandoffCapacityAboveFinalLifecyclePlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveFinalLifecycleOutcome.ProjectionComplete
        && plan.AboveRepeatAdmissionSatisfied
        && plan.FinalGateCaptureValidatedJoinDerived
        && plan.FinalGatePlan is
        {
            Capture.CaptureValidatedJoin: true,
            NormalBoundaryAdmitted: true,
            UnreachedFrameMinus14CaptureProvided: false,
            UnreachedReceiver4CaptureProvided: false,
            UnreachedNormalCallStatusProvided: false
        }
        && plan.FinalLifecyclePlan?.Outcome
            == FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
        && plan.FinalLifecyclePlan?.FinalPlan?.ReturnReached == true
        && plan.FinalOutcomeClean
        && AboveFinalLifecycleUnreachedCapturesClear(plan)
        && !plan.UnreachedFinalGateFrameMinus14CaptureProvided
        && !plan.UnreachedFinalGateReceiver4CaptureProvided
        && !plan.UnreachedFinalGateNormalCallStatusProvided
        && !plan.UnreachedFinalReturnReceiver8OldCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveFinalLifecycleProjection
                .LifecycleEndRawFileOffsetExclusive;

    private static bool AboveFinalLifecycleUnreachedCapturesClear(
        FrameHandoffCapacityAboveFinalLifecyclePlan plan) =>
        !plan.AboveRepeatPlan.UnreachedGateHelperReturnEaxCaptureProvided
        && !plan.AboveRepeatPlan.UnreachedGateFrameMinus4CaptureProvided
        && !plan.AboveRepeatPlan.UnreachedRepeatEntryEcxCaptureProvided
        && !plan.AboveRepeatPlan.UnreachedRepeatEntryEdiCaptureProvided
        && !plan.AboveRepeatPlan.UnreachedDirectionFlagCaptureProvided
        && !plan.AboveRepeatPlan.UnreachedRepCompletionCaptureProvided
        && !plan.UnreachedFinalGateFrameMinus14CaptureProvided
        && !plan.UnreachedFinalGateReceiver4CaptureProvided
        && !plan.UnreachedFinalGateNormalCallStatusProvided
        && !plan.UnreachedFinalReturnReceiver8OldCaptureProvided;

    private static bool FinalReturnEntryRegistersReached(
        FrameHandoffCapacityAboveFinalLifecyclePlan plan) =>
        plan.FinalLifecyclePlan.HasValue
        && plan.FinalLifecyclePlan.Value.FinalPlan.HasValue;

    private static FrameHandoffCapacityAboveBranchLifecyclePlan BuildPlan(
        FrameHandoffCapacityAboveBranchLifecycleCapture capture,
        FrameHandoffCapacityAboveLifecyclePlan aboveLifecyclePlan,
        FrameHandoffCapacityAboveBranchLifecycleOutcome outcome,
        bool aboveLifecycleUnreachedCapturesClear,
        FrameHandoffCapacityAboveFinalLifecycleCapture? derivedFinalLifecycleCapture = null,
        FrameHandoffCapacityAboveFinalLifecyclePlan? aboveFinalLifecyclePlan = null,
        bool aboveLifecycleAdmissionSatisfied = false,
        bool? suppliedAboveRepeatReturnGateMatchesDerived = null,
        bool aboveFinalLifecycleAdmissionSatisfied = false,
        bool unreachedAboveRepeatEntryEcxCaptureProvided = false,
        bool unreachedAboveRepeatEntryEdiCaptureProvided = false,
        bool unreachedAboveRepeatDirectionFlagCaptureProvided = false,
        bool unreachedAboveRepeatRepCompletionCaptureProvided = false,
        bool unreachedFinalGateFrameMinus14CaptureProvided = false,
        bool unreachedFinalGateReceiver4CaptureProvided = false,
        bool unreachedFinalGateNormalCallStatusProvided = false,
        bool unreachedFinalGateEntryEbpIdentityCaptureProvided = false,
        bool unreachedFinalReturnEntryEbpCaptureProvided = false,
        bool unreachedFinalReturnEntryEdiCaptureProvided = false,
        bool unreachedFinalReturnEntryEbxCaptureProvided = false,
        bool unreachedFinalReturnEntryEsiCaptureProvided = false,
        bool unreachedFinalReturnReceiver8OldCaptureProvided = false,
        uint? stopAt = null) =>
        new(
            capture,
            outcome,
            aboveLifecyclePlan,
            derivedFinalLifecycleCapture,
            aboveFinalLifecyclePlan,
            aboveLifecycleAdmissionSatisfied,
            aboveLifecycleUnreachedCapturesClear,
            suppliedAboveRepeatReturnGateMatchesDerived,
            aboveFinalLifecycleAdmissionSatisfied,
            unreachedAboveRepeatEntryEcxCaptureProvided,
            unreachedAboveRepeatEntryEdiCaptureProvided,
            unreachedAboveRepeatDirectionFlagCaptureProvided,
            unreachedAboveRepeatRepCompletionCaptureProvided,
            unreachedFinalGateFrameMinus14CaptureProvided,
            unreachedFinalGateReceiver4CaptureProvided,
            unreachedFinalGateNormalCallStatusProvided,
            unreachedFinalGateEntryEbpIdentityCaptureProvided,
            unreachedFinalReturnEntryEbpCaptureProvided,
            unreachedFinalReturnEntryEdiCaptureProvided,
            unreachedFinalReturnEntryEbxCaptureProvided,
            unreachedFinalReturnEntryEsiCaptureProvided,
            unreachedFinalReturnReceiver8OldCaptureProvided,
            stopAt ?? aboveLifecyclePlan.StopAtRawFileOffsetExclusive);
}
