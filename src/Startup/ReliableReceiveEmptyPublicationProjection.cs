namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveEmptyPublicationOutcome
{
    SlotGateDidNotSelectZeroBranch,
    MissingCallbackCapture,
    CallbackPayloadEdiMismatch,
    CallbackUnreachedCaptureProvided,
    CallbackDidNotTakeHeaderBranch,
    CallbackReturnedNormallyAtResultBoundary,
    MissingPublicationCapture,
    PublicationFrameArgument8CaptureMissing,
    PublicationFrameArgument8CaptureAvailable
}

public readonly record struct ReliableReceiveEmptyPublicationPlan(
    ReliableReceiveSlotGatePlan SlotGatePlan,
    ReliableReceiveEmptySlotCallbackPlan? CallbackPlan,
    ReliableReceiveSlotPublicationPlan? PublicationPlan,
    ReliableReceiveEmptyPublicationOutcome Outcome,
    uint PayloadEdi,
    uint? CallbackPayloadEdi,
    bool UnreachedCallbackCaptureProvided,
    bool UnreachedPublicationCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool PublicationProjectionReached => PublicationPlan.HasValue;

    public bool PublicationFrameArgument8CaptureAvailable =>
        Outcome == ReliableReceiveEmptyPublicationOutcome.PublicationFrameArgument8CaptureAvailable;
}

/// <summary>
/// Composes the zero-slot gate, clean empty-slot header branch, and publication projection.
/// A projected publication write does not guarantee that the native memory access succeeds.
/// </summary>
public static class ReliableReceiveEmptyPublicationProjection
{
    public static ReliableReceiveEmptyPublicationPlan Project(
        ReliableReceiveSlotGateValidatedInput slotInput,
        ReliableReceiveSlotGateCapture slotCapture,
        uint payloadEdi,
        ReliableReceiveEmptySlotCallbackCapture? callbackCapture,
        ReliableReceiveSlotPublicationCapture? publicationCapture)
    {
        ReliableReceiveSlotGatePlan slotPlan =
            ReliableReceiveSlotGateProjection.Project(slotInput, slotCapture);

        if (slotPlan.Outcome != ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected)
        {
            return Stop(
                slotPlan,
                callbackPlan: null,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.SlotGateDidNotSelectZeroBranch,
                payloadEdi,
                callbackPayloadEdi: callbackCapture?.PayloadEdi,
                unreachedCallbackCaptureProvided: callbackCapture.HasValue,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                slotPlan.StopAtRawFileOffsetExclusive);
        }

        if (callbackCapture is not ReliableReceiveEmptySlotCallbackCapture callback)
        {
            return Stop(
                slotPlan,
                callbackPlan: null,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.MissingCallbackCapture,
                payloadEdi,
                callbackPayloadEdi: null,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                ReliableReceiveEmptySlotCallbackProjection.EntryRawFileOffset);
        }

        if (callback.PayloadEdi != payloadEdi)
        {
            return Stop(
                slotPlan,
                callbackPlan: null,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.CallbackPayloadEdiMismatch,
                payloadEdi,
                callbackPayloadEdi: callback.PayloadEdi,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                ReliableReceiveEmptySlotCallbackProjection.EntryRawFileOffset);
        }

        ReliableReceiveEmptySlotCallbackPlan callbackPlan =
            ReliableReceiveEmptySlotCallbackProjection.Project(callback);

        if (callbackPlan.Outcome
            == ReliableReceiveEmptySlotCallbackOutcome.IndirectCallReturnedNormally)
        {
            return Stop(
                slotPlan,
                callbackPlan,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.CallbackReturnedNormallyAtResultBoundary,
                payloadEdi,
                callbackPayloadEdi: callback.PayloadEdi,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                ReliableReceiveEmptySlotCallbackProjection.NormalReturnRawFileOffset);
        }

        if (callbackPlan.Outcome
            == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided)
        {
            return Stop(
                slotPlan,
                callbackPlan,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.CallbackUnreachedCaptureProvided,
                payloadEdi,
                callbackPayloadEdi: callback.PayloadEdi,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                callbackPlan.StopAtRawFileOffsetExclusive);
        }

        if (callbackPlan.Outcome
            != ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken)
        {
            return Stop(
                slotPlan,
                callbackPlan,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.CallbackDidNotTakeHeaderBranch,
                payloadEdi,
                callbackPayloadEdi: callback.PayloadEdi,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: publicationCapture.HasValue,
                callbackPlan.StopAtRawFileOffsetExclusive);
        }

        if (publicationCapture is not ReliableReceiveSlotPublicationCapture publication)
        {
            return Stop(
                slotPlan,
                callbackPlan,
                publicationPlan: null,
                ReliableReceiveEmptyPublicationOutcome.MissingPublicationCapture,
                payloadEdi,
                callbackPayloadEdi: callback.PayloadEdi,
                unreachedCallbackCaptureProvided: false,
                unreachedPublicationCaptureProvided: false,
                ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset);
        }

        ReliableReceiveSlotPublicationPlan publicationPlan =
            ReliableReceiveSlotPublicationProjection.Project(
                slotInput.FrameBaseAddress,
                payloadEdi,
                publication);

        return Stop(
            slotPlan,
            callbackPlan,
            publicationPlan,
            publicationPlan.Outcome
                == ReliableReceiveSlotPublicationOutcome.FrameArgument8CaptureAvailable
                ? ReliableReceiveEmptyPublicationOutcome.PublicationFrameArgument8CaptureAvailable
                : ReliableReceiveEmptyPublicationOutcome.PublicationFrameArgument8CaptureMissing,
            payloadEdi,
            callbackPayloadEdi: callback.PayloadEdi,
            unreachedCallbackCaptureProvided: false,
            unreachedPublicationCaptureProvided: false,
            publicationPlan.StopAtRawFileOffsetExclusive);
    }

    private static ReliableReceiveEmptyPublicationPlan Stop(
        ReliableReceiveSlotGatePlan slotPlan,
        ReliableReceiveEmptySlotCallbackPlan? callbackPlan,
        ReliableReceiveSlotPublicationPlan? publicationPlan,
        ReliableReceiveEmptyPublicationOutcome outcome,
        uint payloadEdi,
        uint? callbackPayloadEdi,
        bool unreachedCallbackCaptureProvided,
        bool unreachedPublicationCaptureProvided,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            slotPlan,
            callbackPlan,
            publicationPlan,
            outcome,
            payloadEdi,
            callbackPayloadEdi,
            unreachedCallbackCaptureProvided,
            unreachedPublicationCaptureProvided,
            stopAtRawFileOffsetExclusive);
}
