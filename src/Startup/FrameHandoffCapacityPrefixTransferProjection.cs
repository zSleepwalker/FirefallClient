namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityPrefixTransferCapture(
    FrameHandoffCapacityPrefixCallCapture PrefixCall,
    FrameHandoffCapacityTransferCapture Transfer);

public enum FrameHandoffCapacityPrefixTransferOutcome
{
    PrefixCallBlocked,
    TransferEntryIdentityMismatch,
    ZeroRouteEntryEdiMismatch,
    TransferBlocked,
    TransferHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityPrefixTransferPlan(
    FrameHandoffCapacityPrefixTransferCapture Capture,
    FrameHandoffCapacityPrefixTransferOutcome Outcome,
    FrameHandoffCapacityPrefixCallPlan PrefixCallPlan,
    FrameHandoffCapacityTransferPlan? TransferPlan,
    bool PrefixCallAdmissionChecked,
    bool PrefixCallSuccessAdmission,
    bool TransferEntryIdentitiesValidated,
    bool TransferEntryEbxMatches,
    bool TransferEntryEbpMatches,
    bool TransferEntryEdiIdentityValidated,
    bool? TransferEntryEdiMatches,
    bool TransferAdmissionChecked,
    bool TransferSuccessAdmission,
    bool TransferUnreachedCapturesClear,
    bool UnreachedTransferReceiver4CaptureProvided,
    bool UnreachedTransferReceiver8CaptureProvided,
    bool UnreachedTransferFrameMinus4CaptureProvided,
    bool UnreachedTransferFrameMinusCCaptureProvided,
    bool UnreachedTransferNormalCallCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the exact prefix-call admission with the first opaque frame-handoff transfer.
/// </summary>
public static class FrameHandoffCapacityPrefixTransferProjection
{
    public const uint ProjectionStartRawFileOffset =
        FrameHandoffCapacityPrefixProjection.EntryRawFileOffset;
    public const uint TransferEntryRawFileOffset =
        FrameHandoffCapacityTransferProjection.EntryRawFileOffset;
    public const uint ProjectionEndRawFileOffsetExclusive =
        FrameHandoffCapacityTransferProjection.NormalBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityPrefixTransferPlan Project(
        FrameHandoffCapacityPrefixTransferCapture capture)
    {
        FrameHandoffCapacityPrefixCallPlan prefixCallPlan =
            FrameHandoffCapacityPrefixCallProjection.Project(capture.PrefixCall);

        if (!prefixCallPlan.SuccessAdmission)
        {
            return StopBeforeTransfer(
                capture,
                prefixCallPlan,
                FrameHandoffCapacityPrefixTransferOutcome.PrefixCallBlocked,
                prefixCallSuccessAdmission: false,
                stopAt: prefixCallPlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffCapacityPrefixPlan prefixPlan = prefixCallPlan.PrefixPlan;
        uint prefixEntryEbx = capture.PrefixCall.PrefixCapture.EntryEbx;
        uint prefixEntryEbp = capture.PrefixCall.PrefixCapture.EntryEbp;
        bool entryEbxMatches = capture.Transfer.EntryEbx == prefixEntryEbx;
        bool entryEbpMatches = capture.Transfer.EntryEbp == prefixEntryEbp;
        bool zeroRoute = prefixPlan.Outcome
            == FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA;
        bool ediIdentityValidated = zeroRoute;
        bool? ediIdentityMatches = zeroRoute
            ? capture.Transfer.EntryEdi == prefixPlan.EdiAfterShift
            : null;

        if (!entryEbxMatches || !entryEbpMatches)
        {
            return StopBeforeTransfer(
                capture,
                prefixCallPlan,
                FrameHandoffCapacityPrefixTransferOutcome.TransferEntryIdentityMismatch,
                prefixCallSuccessAdmission: true,
                transferEntryIdentitiesValidated: true,
                transferEntryEbxMatches: entryEbxMatches,
                transferEntryEbpMatches: entryEbpMatches,
                transferEntryEdiIdentityValidated: ediIdentityValidated,
                transferEntryEdiMatches: ediIdentityMatches,
                stopAt: TransferEntryRawFileOffset);
        }

        if (zeroRoute && ediIdentityMatches != true)
        {
            return StopBeforeTransfer(
                capture,
                prefixCallPlan,
                FrameHandoffCapacityPrefixTransferOutcome.ZeroRouteEntryEdiMismatch,
                prefixCallSuccessAdmission: true,
                transferEntryIdentitiesValidated: true,
                transferEntryEbxMatches: true,
                transferEntryEbpMatches: true,
                transferEntryEdiIdentityValidated: true,
                transferEntryEdiMatches: false,
                stopAt: TransferEntryRawFileOffset);
        }

        FrameHandoffCapacityTransferCapture transferCapture = zeroRoute
            ? capture.Transfer with { EntryEdi = prefixPlan.EdiAfterShift!.Value }
            : capture.Transfer;
        FrameHandoffCapacityTransferPlan transferPlan =
            FrameHandoffCapacityTransferProjection.Project(transferCapture);
        bool transferSuccessAdmission = transferPlan.SuccessAdmission;
        bool transferUnreachedCapturesClear =
            !transferPlan.UnreachedReceiver8CaptureProvided
            && !transferPlan.UnreachedFrameMinus4CaptureProvided
            && !transferPlan.UnreachedFrameMinusCCaptureProvided
            && !transferPlan.UnreachedHelperReturnCaptureProvided;

        FrameHandoffCapacityPrefixTransferOutcome outcome =
            transferSuccessAdmission
                ? FrameHandoffCapacityPrefixTransferOutcome.ProjectionComplete
                : transferUnreachedCapturesClear
                    ? FrameHandoffCapacityPrefixTransferOutcome.TransferBlocked
                    : FrameHandoffCapacityPrefixTransferOutcome.TransferHasUnreachedCaptures;

        return new(
            capture,
            outcome,
            prefixCallPlan,
            transferPlan,
            PrefixCallAdmissionChecked: true,
            PrefixCallSuccessAdmission: true,
            TransferEntryIdentitiesValidated: true,
            TransferEntryEbxMatches: true,
            TransferEntryEbpMatches: true,
            TransferEntryEdiIdentityValidated: ediIdentityValidated,
            TransferEntryEdiMatches: ediIdentityMatches,
            TransferAdmissionChecked: true,
            TransferSuccessAdmission: transferSuccessAdmission,
            TransferUnreachedCapturesClear: transferUnreachedCapturesClear,
            UnreachedTransferReceiver4CaptureProvided: false,
            UnreachedTransferReceiver8CaptureProvided:
                transferPlan.UnreachedReceiver8CaptureProvided,
            UnreachedTransferFrameMinus4CaptureProvided:
                transferPlan.UnreachedFrameMinus4CaptureProvided,
            UnreachedTransferFrameMinusCCaptureProvided:
                transferPlan.UnreachedFrameMinusCCaptureProvided,
            UnreachedTransferNormalCallCaptureProvided:
                transferPlan.UnreachedHelperReturnCaptureProvided,
            StopAtRawFileOffsetExclusive: transferPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityPrefixTransferPlan StopBeforeTransfer(
        FrameHandoffCapacityPrefixTransferCapture capture,
        FrameHandoffCapacityPrefixCallPlan prefixCallPlan,
        FrameHandoffCapacityPrefixTransferOutcome outcome,
        bool prefixCallSuccessAdmission,
        bool transferEntryIdentitiesValidated = false,
        bool transferEntryEbxMatches = false,
        bool transferEntryEbpMatches = false,
        bool transferEntryEdiIdentityValidated = false,
        bool? transferEntryEdiMatches = null,
        uint stopAt = ProjectionStartRawFileOffset) =>
        new(
            capture,
            outcome,
            prefixCallPlan,
            null,
            PrefixCallAdmissionChecked: true,
            PrefixCallSuccessAdmission: prefixCallSuccessAdmission,
            TransferEntryIdentitiesValidated: transferEntryIdentitiesValidated,
            TransferEntryEbxMatches: transferEntryEbxMatches,
            TransferEntryEbpMatches: transferEntryEbpMatches,
            TransferEntryEdiIdentityValidated: transferEntryEdiIdentityValidated,
            TransferEntryEdiMatches: transferEntryEdiMatches,
            TransferAdmissionChecked: false,
            TransferSuccessAdmission: false,
            TransferUnreachedCapturesClear:
                !capture.Transfer.Receiver4.HasValue
                && !capture.Transfer.Receiver8.HasValue
                && !capture.Transfer.FrameMinus4.HasValue
                && !capture.Transfer.FrameMinusC.HasValue
                && !capture.Transfer.HelperReturnedNormally.HasValue,
            UnreachedTransferReceiver4CaptureProvided: capture.Transfer.Receiver4.HasValue,
            UnreachedTransferReceiver8CaptureProvided: capture.Transfer.Receiver8.HasValue,
            UnreachedTransferFrameMinus4CaptureProvided:
                capture.Transfer.FrameMinus4.HasValue,
            UnreachedTransferFrameMinusCCaptureProvided:
                capture.Transfer.FrameMinusC.HasValue,
            UnreachedTransferNormalCallCaptureProvided:
                capture.Transfer.HelperReturnedNormally.HasValue,
            StopAtRawFileOffsetExclusive: stopAt);
}
