namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveOccupiedSlotOutcome
{
    EmptySlotBranchSelected,
    SlotGateDidNotSelectNonzeroFallthrough,
    MissingRecordWordCapture,
    RecordPointerMismatch,
    RecordWordDidNotPermitForwarding,
    MissingForwardingCallerCapture,
    ForwardingCallerRouteMismatch,
    ForwardingCallerReceiverBaseMismatch,
    TailPayloadPointerMismatch,
    ForwardingReturnDidNotComplete,
    OccupiedBranchCompleted
}

public readonly record struct ReliableReceiveOccupiedSlotPlan(
    ReliableReceiveSlotGatePlan SlotGatePlan,
    ReliableReceiveRecordWordPlan? RecordWordPlan,
    ReliableReceiveForwardingReturnPlan? ForwardingReturnPlan,
    ReliableReceiveOccupiedSlotOutcome Outcome,
    bool UnreachedRecordCaptureProvided,
    bool UnreachedCallerCaptureProvided,
    bool UnreachedRecycleReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool OccupiedBranchCompleted =>
        Outcome == ReliableReceiveOccupiedSlotOutcome.OccupiedBranchCompleted;
}

/// <summary>
/// Composes the occupied-slot gate, record-word decision, and forwarding return.
/// </summary>
public static class ReliableReceiveOccupiedSlotProjection
{
    public static ReliableReceiveOccupiedSlotPlan Project(
        ReliableReceiveSlotGateValidatedInput slotInput,
        ReliableReceiveSlotGateCapture slotCapture,
        ReliableReceiveRecordWordCapture? recordWordCapture,
        ReliableReceiveForwardingCallerCapture? forwardingCallerCapture,
        ReliableReceiveRecycleReturnCapture? recycleReturnCapture)
    {
        ReliableReceiveSlotGatePlan slotPlan =
            ReliableReceiveSlotGateProjection.Project(slotInput, slotCapture);

        if (slotPlan.Outcome != ReliableReceiveSlotGateOutcome.NonzeroSlotFallthroughSelected)
        {
            return new(
                slotPlan,
                null,
                null,
                slotPlan.Outcome == ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected
                    ? ReliableReceiveOccupiedSlotOutcome.EmptySlotBranchSelected
                    : ReliableReceiveOccupiedSlotOutcome.SlotGateDidNotSelectNonzeroFallthrough,
                recordWordCapture.HasValue,
                forwardingCallerCapture.HasValue,
                recycleReturnCapture.HasValue,
                slotPlan.StopAtRawFileOffsetExclusive);
        }

        if (recordWordCapture is not ReliableReceiveRecordWordCapture recordCapture)
        {
            return Stop(
                slotPlan,
                null,
                null,
                ReliableReceiveOccupiedSlotOutcome.MissingRecordWordCapture,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: forwardingCallerCapture.HasValue,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                ReliableReceiveRecordWordProjection.RecordWordReadRawFileOffset);
        }

        if (recordCapture.CapturedSlotRecordPointer != slotCapture.SlotDword)
        {
            return Stop(
                slotPlan,
                null,
                null,
                ReliableReceiveOccupiedSlotOutcome.RecordPointerMismatch,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: forwardingCallerCapture.HasValue,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                ReliableReceiveRecordWordProjection.RecordWordReadRawFileOffset);
        }

        ReliableReceiveRecordWordPlan recordPlan =
            ReliableReceiveRecordWordProjection.Project(recordCapture);
        if (recordPlan.Outcome is not (
                ReliableReceiveRecordWordOutcome.DiagnosticSkippedForEqualWords
                or ReliableReceiveRecordWordOutcome.DiagnosticReturnedNormally))
        {
            return Stop(
                slotPlan,
                recordPlan,
                null,
                ReliableReceiveOccupiedSlotOutcome.RecordWordDidNotPermitForwarding,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: forwardingCallerCapture.HasValue,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                recordPlan.StopAtRawFileOffsetExclusive);
        }

        if (forwardingCallerCapture is not ReliableReceiveForwardingCallerCapture callerCapture)
        {
            return Stop(
                slotPlan,
                recordPlan,
                null,
                ReliableReceiveOccupiedSlotOutcome.MissingForwardingCallerCapture,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: false,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                ReliableReceiveForwardingCallerProjection.StartRawFileOffset(
                    ReliableReceiveForwardingCallerRoute.ExistingRecord));
        }

        uint callerStart = ReliableReceiveForwardingCallerProjection.StartRawFileOffset(
            ReliableReceiveForwardingCallerRoute.ExistingRecord);
        if (callerCapture.Route != ReliableReceiveForwardingCallerRoute.ExistingRecord)
        {
            return Stop(
                slotPlan,
                recordPlan,
                null,
                ReliableReceiveOccupiedSlotOutcome.ForwardingCallerRouteMismatch,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: false,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                callerStart);
        }

        if (callerCapture.ReceiverBaseAddress != slotInput.ReceiverBaseAddress)
        {
            return Stop(
                slotPlan,
                recordPlan,
                null,
                ReliableReceiveOccupiedSlotOutcome.ForwardingCallerReceiverBaseMismatch,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: false,
                unreachedRecycleReturnCaptureProvided: recycleReturnCapture.HasValue,
                callerStart);
        }

        if (recycleReturnCapture is ReliableReceiveRecycleReturnCapture tailCapture
            && tailCapture.PayloadPointerInEdi != recordCapture.PayloadPointer)
        {
            return Stop(
                slotPlan,
                recordPlan,
                null,
                ReliableReceiveOccupiedSlotOutcome.TailPayloadPointerMismatch,
                unreachedRecordCaptureProvided: false,
                unreachedCallerCaptureProvided: false,
                unreachedRecycleReturnCaptureProvided: true,
                callerStart);
        }

        ReliableReceiveForwardingReturnPlan forwardingPlan =
            ReliableReceiveForwardingReturnProjection.Project(callerCapture, recycleReturnCapture);
        bool returnCompleted =
            forwardingPlan.StopReason == ReliableReceiveForwardingReturnStopReason.ReturnCompleted
            && forwardingPlan.CompletedStages
                == (ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
                    | ReliableReceiveForwardingReturnCompletedStages.RecycleReturn);
        return new(
            slotPlan,
            recordPlan,
            forwardingPlan,
            returnCompleted
                ? ReliableReceiveOccupiedSlotOutcome.OccupiedBranchCompleted
                : ReliableReceiveOccupiedSlotOutcome.ForwardingReturnDidNotComplete,
            false,
            false,
            false,
            forwardingPlan.StopAtRawFileOffsetExclusive);
    }

    private static ReliableReceiveOccupiedSlotPlan Stop(
        ReliableReceiveSlotGatePlan slotPlan,
        ReliableReceiveRecordWordPlan? recordPlan,
        ReliableReceiveForwardingReturnPlan? forwardingPlan,
        ReliableReceiveOccupiedSlotOutcome outcome,
        bool unreachedRecordCaptureProvided,
        bool unreachedCallerCaptureProvided,
        bool unreachedRecycleReturnCaptureProvided,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            slotPlan,
            recordPlan,
            forwardingPlan,
            outcome,
            unreachedRecordCaptureProvided,
            unreachedCallerCaptureProvided,
            unreachedRecycleReturnCaptureProvided,
            stopAtRawFileOffsetExclusive);
}
