namespace Firefall.Reconstruction.Startup;

public enum UnreliableReceiveCallbackRecycleJoinOutcome
{
    MissingPresenceCapture,
    PresenceDidNotAdmitCallback,
    DirectCallbackIncomplete,
    DirectCallbackDidNotReturnNormally,
    MissingRecycleCapture,
    RecyclePayloadEbxMismatch,
    RecyclePriorCallbackMissing,
    RecyclePriorCallbackDidNotReturnNormally,
    RecycleIncomplete,
    RecycleHelperDidNotReturnNormally,
    RecycleHelperReturnedNormally
}

public readonly record struct UnreliableReceiveCallbackRecycleJoinPlan(
    UnreliableReceivePresenceCallbackPlan? PresenceCallbackPlan,
    UnreliableReceiveCallbackRecyclePlan? RecyclePlan,
    UnreliableReceiveCallbackRecycleJoinOutcome Outcome,
    bool UnreachedDirectCaptureProvided,
    bool UnreachedRecycleCaptureProvided,
    bool? RecyclePayloadEbxMatchesDirect,
    bool? RecyclePriorCallbackReturnedNormally,
    bool RecycleFrameEbpExplicitlyCaptured,
    bool UnreachedRecycleGlobalDwordCaptureProvided,
    bool UnreachedRecycleCallbackEaxCaptureProvided,
    bool UnreachedRecycleHelperReturnCaptureProvided,
    bool RecycleAdmittedByNormalDirectCallback,
    bool CompositionComplete,
    uint? SelectedDestinationRawFileOffset,
    string? CompleteRawWindowBytes,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the receiver-presence callback, direct callback, and recycle continuation.
/// </summary>
public static class UnreliableReceiveCallbackRecycleJoinProjection
{
    public const uint EntryRawFileOffset =
        UnreliableReceivePresenceCallbackProjection.EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveCallbackRecycleProjection.EndRawFileOffsetExclusive;
    public const uint EndVirtualAddress = 0x00FE6778;

    public const string CompleteRawWindowBytes =
        UnreliableReceivePresenceCallbackProjection.CompleteRawWindowBytes
        + " "
        + UnreliableReceiveCallbackRecycleProjection.CapturedWindowBytes;

    public static UnreliableReceiveCallbackRecycleJoinPlan Project(
        UnreliableReceiveCallbackPresenceCapture? presenceCapture,
        UnreliableReceiveDirectCallbackCapture? directCapture,
        UnreliableReceiveCallbackRecycleCapture? recycleCapture)
    {
        if (presenceCapture is not UnreliableReceiveCallbackPresenceCapture presence)
        {
            return Stop(
                UnreliableReceiveCallbackRecycleJoinOutcome.MissingPresenceCapture,
                unreachedDirectCaptureProvided: directCapture.HasValue,
                unreachedRecycleCaptureProvided: recycleCapture.HasValue,
                unreachedRecycleGlobalDwordCaptureProvided:
                    recycleCapture?.GlobalDwordAtFirstRead.HasValue == true,
                unreachedRecycleCallbackEaxCaptureProvided:
                    recycleCapture?.CallbackEaxAtCopy.HasValue == true,
                unreachedRecycleHelperReturnCaptureProvided:
                    recycleCapture?.RecycleHelperReturnedNormally.HasValue == true,
                recyclePriorCallbackReturnedNormally:
                    recycleCapture?.PriorCallbackReturnedNormally,
                recycleFrameEbpExplicitlyCaptured: recycleCapture.HasValue,
                stopAt: EntryRawFileOffset);
        }

        UnreliableReceivePresenceCallbackPlan presenceCallbackPlan =
            UnreliableReceivePresenceCallbackProjection.Project(presence, directCapture);
        bool directReturnedNormally =
            presenceCallbackPlan.CompositionComplete
            && presenceCallbackPlan.DirectCallbackPlan?.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally;
        if (!directReturnedNormally)
        {
            return Stop(
                GetDirectFailureOutcome(presenceCallbackPlan),
                presenceCallbackPlan,
                unreachedDirectCaptureProvided:
                    presenceCallbackPlan.UnreachedCallbackCaptureProvided,
                unreachedRecycleCaptureProvided: recycleCapture.HasValue,
                unreachedRecycleGlobalDwordCaptureProvided:
                    recycleCapture?.GlobalDwordAtFirstRead.HasValue == true,
                unreachedRecycleCallbackEaxCaptureProvided:
                    recycleCapture?.CallbackEaxAtCopy.HasValue == true,
                unreachedRecycleHelperReturnCaptureProvided:
                    recycleCapture?.RecycleHelperReturnedNormally.HasValue == true,
                recyclePriorCallbackReturnedNormally:
                    recycleCapture?.PriorCallbackReturnedNormally,
                recycleFrameEbpExplicitlyCaptured: recycleCapture.HasValue,
                stopAt: presenceCallbackPlan.StopAtRawFileOffsetExclusive);
        }

        UnreliableReceiveDirectCallbackPlan directPlan =
            presenceCallbackPlan.DirectCallbackPlan!.Value;
        if (recycleCapture is not UnreliableReceiveCallbackRecycleCapture recycle)
        {
            return Stop(
                UnreliableReceiveCallbackRecycleJoinOutcome.MissingRecycleCapture,
                presenceCallbackPlan,
                recycleAdmittedByNormalDirectCallback: true,
                stopAt: UnreliableReceiveCallbackRecycleProjection.EntryRawFileOffset);
        }

        if (recycle.PayloadEbx != directPlan.Capture.PayloadEbx)
        {
            return Stop(
                UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePayloadEbxMismatch,
                presenceCallbackPlan,
                unreachedRecycleGlobalDwordCaptureProvided:
                    recycle.GlobalDwordAtFirstRead.HasValue,
                unreachedRecycleCallbackEaxCaptureProvided:
                    recycle.CallbackEaxAtCopy.HasValue,
                unreachedRecycleHelperReturnCaptureProvided:
                    recycle.RecycleHelperReturnedNormally.HasValue,
                recyclePayloadEbxMatchesDirect: false,
                recyclePriorCallbackReturnedNormally:
                    recycle.PriorCallbackReturnedNormally,
                recycleFrameEbpExplicitlyCaptured: true,
                recycleAdmittedByNormalDirectCallback: true,
                stopAt: UnreliableReceiveCallbackRecycleProjection.GlobalDwordReadRawFileOffset);
        }

        UnreliableReceiveCallbackRecyclePlan recyclePlan =
            UnreliableReceiveCallbackRecycleProjection.Project(recycle);
        return Stop(
            GetRecycleOutcome(recyclePlan),
            presenceCallbackPlan,
            recyclePlan,
            recyclePayloadEbxMatchesDirect: true,
            recyclePriorCallbackReturnedNormally:
                recyclePlan.PriorCallbackReturnedNormally,
            recycleFrameEbpExplicitlyCaptured: true,
            recycleAdmittedByNormalDirectCallback: true,
            compositionComplete:
                recyclePlan.Outcome
                    == UnreliableReceiveCallbackRecycleOutcome.RecycleHelperReturnedNormally,
            selectedDestinationRawFileOffset:
                recyclePlan.SelectedDestinationRawFileOffset,
            completeRawWindowBytes:
                recyclePlan.NormalCallBoundaryReached ? CompleteRawWindowBytes : null,
            stopAt: recyclePlan.StopAtRawFileOffsetExclusive);
    }

    private static UnreliableReceiveCallbackRecycleJoinOutcome GetDirectFailureOutcome(
        UnreliableReceivePresenceCallbackPlan plan)
    {
        if (plan.Outcome
            == UnreliableReceivePresenceCallbackOutcome.PresenceDidNotAdmitCallback)
        {
            return UnreliableReceiveCallbackRecycleJoinOutcome.PresenceDidNotAdmitCallback;
        }

        return plan.DirectCallbackPlan?.Outcome switch
        {
            UnreliableReceiveDirectCallbackOutcome.IndirectCallDidNotReturnNormally =>
                UnreliableReceiveCallbackRecycleJoinOutcome.DirectCallbackDidNotReturnNormally,
            _ => UnreliableReceiveCallbackRecycleJoinOutcome.DirectCallbackIncomplete
        };
    }

    private static UnreliableReceiveCallbackRecycleJoinOutcome GetRecycleOutcome(
        UnreliableReceiveCallbackRecyclePlan plan) =>
        plan.PriorCallbackReturnedNormally switch
        {
            null => UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePriorCallbackMissing,
            false =>
                UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePriorCallbackDidNotReturnNormally,
            true => plan.Outcome switch
            {
                UnreliableReceiveCallbackRecycleOutcome.RecycleHelperDidNotReturnNormally =>
                    UnreliableReceiveCallbackRecycleJoinOutcome.RecycleHelperDidNotReturnNormally,
                UnreliableReceiveCallbackRecycleOutcome.RecycleHelperReturnedNormally =>
                    UnreliableReceiveCallbackRecycleJoinOutcome.RecycleHelperReturnedNormally,
                _ => UnreliableReceiveCallbackRecycleJoinOutcome.RecycleIncomplete
            }
        };

    private static UnreliableReceiveCallbackRecycleJoinPlan Stop(
        UnreliableReceiveCallbackRecycleJoinOutcome outcome,
        UnreliableReceivePresenceCallbackPlan? presenceCallbackPlan = null,
        UnreliableReceiveCallbackRecyclePlan? recyclePlan = null,
        bool unreachedDirectCaptureProvided = false,
        bool unreachedRecycleCaptureProvided = false,
        bool? recyclePayloadEbxMatchesDirect = null,
        bool? recyclePriorCallbackReturnedNormally = null,
        bool recycleFrameEbpExplicitlyCaptured = false,
        bool unreachedRecycleGlobalDwordCaptureProvided = false,
        bool unreachedRecycleCallbackEaxCaptureProvided = false,
        bool unreachedRecycleHelperReturnCaptureProvided = false,
        bool recycleAdmittedByNormalDirectCallback = false,
        bool compositionComplete = false,
        uint? selectedDestinationRawFileOffset = null,
        string? completeRawWindowBytes = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            presenceCallbackPlan,
            recyclePlan,
            outcome,
            unreachedDirectCaptureProvided,
            unreachedRecycleCaptureProvided,
            recyclePayloadEbxMatchesDirect,
            recyclePriorCallbackReturnedNormally,
            recycleFrameEbpExplicitlyCaptured,
            unreachedRecycleGlobalDwordCaptureProvided,
            unreachedRecycleCallbackEaxCaptureProvided,
            unreachedRecycleHelperReturnCaptureProvided,
            recycleAdmittedByNormalDirectCallback,
            compositionComplete,
            selectedDestinationRawFileOffset,
            completeRawWindowBytes,
            stopAt);
}
