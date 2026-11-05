namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySecondTransferLifecycleCapture(
    FrameHandoffCapacitySecondTransferCapture Transfer,
    FrameHandoffCapacitySecondReturnGateCapture ReturnGate);

public enum FrameHandoffCapacitySecondTransferLifecycleOutcome
{
    TransferBlocked,
    TransferHasUnreachedCaptures,
    ReturnEntryIdentityMismatch,
    ReturnBlocked,
    ReturnHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacitySecondTransferLifecyclePlan(
    FrameHandoffCapacitySecondTransferLifecycleCapture Capture,
    FrameHandoffCapacitySecondTransferLifecycleOutcome Outcome,
    FrameHandoffCapacitySecondTransferPlan TransferPlan,
    FrameHandoffCapacitySecondReturnGatePlan? ReturnGatePlan,
    bool TransferAdmissionChecked,
    bool TransferSuccessAdmission,
    bool TransferUnreachedCapturesClear,
    bool ReturnEntryIdentityValidated,
    bool ReturnEntryEbpMatches,
    bool ReturnBranchAdmitted,
    bool UnreachedReturnFrameMinusCCaptureProvided,
    bool UnreachedReturnHelperReturnEaxCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the second capacity transfer with its independently captured return gate.
/// </summary>
public static class FrameHandoffCapacitySecondTransferLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacitySecondTransferProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacitySecondTransferLifecyclePlan Project(
        FrameHandoffCapacitySecondTransferLifecycleCapture capture)
    {
        FrameHandoffCapacitySecondTransferPlan transferPlan =
            FrameHandoffCapacitySecondTransferProjection.Project(capture.Transfer);
        bool transferSuccessAdmission =
            transferPlan.Outcome == FrameHandoffCapacitySecondTransferOutcome.ReturnedNormally
            && transferPlan.NormalBoundaryReached;
        bool transferUnreachedCapturesClear =
            !transferPlan.UnreachedFrameMinus10CaptureProvided
            && !transferPlan.UnreachedCallReturnCaptureProvided;

        if (!transferSuccessAdmission || !transferUnreachedCapturesClear)
        {
            return StopBeforeReturn(
                capture,
                transferPlan,
                transferUnreachedCapturesClear
                    ? FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferBlocked
                    : FrameHandoffCapacitySecondTransferLifecycleOutcome
                        .TransferHasUnreachedCaptures,
                transferSuccessAdmission,
                transferUnreachedCapturesClear,
                stopAt: transferPlan.StopAtRawFileOffsetExclusive);
        }

        bool entryEbpMatches = capture.ReturnGate.EntryEbp == capture.Transfer.EntryEbp;
        if (!entryEbpMatches)
        {
            return StopBeforeReturn(
                capture,
                transferPlan,
                FrameHandoffCapacitySecondTransferLifecycleOutcome
                    .ReturnEntryIdentityMismatch,
                transferSuccessAdmission,
                transferUnreachedCapturesClear,
                returnEntryIdentityValidated: true,
                returnEntryEbpMatches: false,
                stopAt: FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacitySecondReturnGateCapture derivedReturnCapture =
            capture.ReturnGate with { ValidatedNormalEntry = transferSuccessAdmission };
        FrameHandoffCapacitySecondReturnGatePlan returnGatePlan =
            FrameHandoffCapacitySecondReturnGateProjection.Project(derivedReturnCapture);
        bool returnBranchOutcome = IsBranchOutcome(returnGatePlan.Outcome);
        bool returnBranchAdmitted = returnBranchOutcome
            && !returnGatePlan.UnreachedFrameMinusCCaptureProvided
            && !returnGatePlan.UnreachedHelperReturnEaxCaptureProvided;
        bool returnBlocked = !returnBranchOutcome;

        FrameHandoffCapacitySecondTransferLifecycleOutcome outcome =
            returnBranchAdmitted
                ? FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
                : returnBlocked
                    ? FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnBlocked
                    : FrameHandoffCapacitySecondTransferLifecycleOutcome
                        .ReturnHasUnreachedCaptures;

        return new(
            capture,
            outcome,
            transferPlan,
            returnGatePlan,
            TransferAdmissionChecked: true,
            transferSuccessAdmission,
            transferUnreachedCapturesClear,
            ReturnEntryIdentityValidated: true,
            entryEbpMatches,
            returnBranchAdmitted,
            returnGatePlan.UnreachedFrameMinusCCaptureProvided,
            returnGatePlan.UnreachedHelperReturnEaxCaptureProvided,
            returnGatePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsBranchOutcome(
        FrameHandoffCapacitySecondReturnGateOutcome outcome) =>
        outcome is
            FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
            or FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough;

    private static FrameHandoffCapacitySecondTransferLifecyclePlan StopBeforeReturn(
        FrameHandoffCapacitySecondTransferLifecycleCapture capture,
        FrameHandoffCapacitySecondTransferPlan transferPlan,
        FrameHandoffCapacitySecondTransferLifecycleOutcome outcome,
        bool transferSuccessAdmission,
        bool transferUnreachedCapturesClear,
        bool returnEntryIdentityValidated = false,
        bool returnEntryEbpMatches = false,
        uint stopAt = LifecycleStartRawFileOffset) =>
        new(
            capture,
            outcome,
            transferPlan,
            null,
            TransferAdmissionChecked: true,
            transferSuccessAdmission,
            transferUnreachedCapturesClear,
            returnEntryIdentityValidated,
            returnEntryEbpMatches,
            ReturnBranchAdmitted: false,
            capture.ReturnGate.FrameMinusC.HasValue,
            capture.ReturnGate.HelperReturnEax.HasValue,
            stopAt);
}
