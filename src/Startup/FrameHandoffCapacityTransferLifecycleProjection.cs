namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityTransferLifecycleCapture(
    FrameHandoffCapacityTransferCapture Transfer,
    FrameHandoffCapacityTransferReturnCapture Return);

public enum FrameHandoffCapacityTransferLifecycleOutcome
{
    TransferBlocked,
    TransferHasUnreachedCaptures,
    ReturnEntryIdentityMismatch,
    ReturnBlocked,
    ReturnHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityTransferLifecyclePlan(
    FrameHandoffCapacityTransferLifecycleCapture Capture,
    FrameHandoffCapacityTransferLifecycleOutcome Outcome,
    FrameHandoffCapacityTransferPlan TransferPlan,
    FrameHandoffCapacityTransferReturnPlan? ReturnPlan,
    bool TransferAdmissionChecked,
    bool TransferSuccessAdmission,
    bool TransferUnreachedCapturesClear,
    bool ReturnEntryIdentitiesValidated,
    bool ReturnEntryEbxMatches,
    bool ReturnEntryEbpMatches,
    bool ReturnBranchAdmitted,
    bool UnreachedReturnReceiver4CaptureProvided,
    bool UnreachedReturnHelperReturnEaxCaptureProvided,
    bool UnreachedReturnFrameMinus8CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the first capacity transfer with the freshly captured helper return state.
/// </summary>
public static class FrameHandoffCapacityTransferLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityTransferProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityTransferReturnProjection.NormalBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityTransferLifecyclePlan Project(
        FrameHandoffCapacityTransferLifecycleCapture capture)
    {
        FrameHandoffCapacityTransferPlan transferPlan =
            FrameHandoffCapacityTransferProjection.Project(capture.Transfer);
        bool transferSuccessAdmission = transferPlan.SuccessAdmission;
        bool transferUnreachedCapturesClear =
            !transferPlan.UnreachedReceiver8CaptureProvided
            && !transferPlan.UnreachedFrameMinus4CaptureProvided
            && !transferPlan.UnreachedFrameMinusCCaptureProvided
            && !transferPlan.UnreachedHelperReturnCaptureProvided;

        if (!transferSuccessAdmission || !transferUnreachedCapturesClear)
        {
            return StopBeforeReturn(
                capture,
                transferPlan,
                transferUnreachedCapturesClear
                    ? FrameHandoffCapacityTransferLifecycleOutcome.TransferBlocked
                    : FrameHandoffCapacityTransferLifecycleOutcome.TransferHasUnreachedCaptures,
                transferSuccessAdmission,
                transferUnreachedCapturesClear,
                stopAt: transferPlan.StopAtRawFileOffsetExclusive);
        }

        bool entryEbxMatches = capture.Return.EntryEbx == capture.Transfer.EntryEbx;
        bool entryEbpMatches = capture.Return.EntryEbp == capture.Transfer.EntryEbp;
        if (!entryEbxMatches || !entryEbpMatches)
        {
            return StopBeforeReturn(
                capture,
                transferPlan,
                FrameHandoffCapacityTransferLifecycleOutcome.ReturnEntryIdentityMismatch,
                transferSuccessAdmission,
                transferUnreachedCapturesClear,
                returnEntryIdentitiesValidated: true,
                returnEntryEbxMatches: entryEbxMatches,
                returnEntryEbpMatches: entryEbpMatches,
                stopAt: FrameHandoffCapacityTransferReturnProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityTransferReturnCapture derivedReturnCapture =
            capture.Return with { ValidatedNormalEntry = transferSuccessAdmission };
        FrameHandoffCapacityTransferReturnPlan returnPlan =
            FrameHandoffCapacityTransferReturnProjection.Project(derivedReturnCapture);
        bool returnBranchAdmitted = IsBranchOutcome(returnPlan.Outcome)
            && !returnPlan.UnreachedReceiver4CaptureProvided
            && !returnPlan.UnreachedHelperReturnEaxCaptureProvided
            && !returnPlan.UnreachedFrameMinus8CaptureProvided;
        bool returnBlocked = !IsBranchOutcome(returnPlan.Outcome);

        FrameHandoffCapacityTransferLifecycleOutcome outcome =
            returnBranchAdmitted
                ? FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
                : returnBlocked
                    ? FrameHandoffCapacityTransferLifecycleOutcome.ReturnBlocked
                    : FrameHandoffCapacityTransferLifecycleOutcome.ReturnHasUnreachedCaptures;

        return new(
            capture,
            outcome,
            transferPlan,
            returnPlan,
            TransferAdmissionChecked: true,
            transferSuccessAdmission,
            transferUnreachedCapturesClear,
            ReturnEntryIdentitiesValidated: true,
            entryEbxMatches,
            entryEbpMatches,
            returnBranchAdmitted,
            returnPlan.UnreachedReceiver4CaptureProvided,
            returnPlan.UnreachedHelperReturnEaxCaptureProvided,
            returnPlan.UnreachedFrameMinus8CaptureProvided,
            returnPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsBranchOutcome(FrameHandoffCapacityTransferReturnOutcome outcome) =>
        outcome is
            FrameHandoffCapacityTransferReturnOutcome.FrameMinus8UnsignedGreaterThanEntryEsi
            or FrameHandoffCapacityTransferReturnOutcome
                .FrameMinus8UnsignedLessThanOrEqualToEntryEsi;

    private static FrameHandoffCapacityTransferLifecyclePlan StopBeforeReturn(
        FrameHandoffCapacityTransferLifecycleCapture capture,
        FrameHandoffCapacityTransferPlan transferPlan,
        FrameHandoffCapacityTransferLifecycleOutcome outcome,
        bool transferSuccessAdmission,
        bool transferUnreachedCapturesClear,
        bool returnEntryIdentitiesValidated = false,
        bool returnEntryEbxMatches = false,
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
            returnEntryIdentitiesValidated,
            returnEntryEbxMatches,
            returnEntryEbpMatches,
            ReturnBranchAdmitted: false,
            capture.Return.Receiver4.HasValue,
            capture.Return.HelperReturnEax.HasValue,
            capture.Return.FrameMinus8.HasValue,
            stopAt);
}
