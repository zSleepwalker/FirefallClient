namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffHelperLifecycleCapture(
    FrameHandoffHelperEntryCapture Entry,
    FrameHandoffContinuationLifecycleCapture Continuation);

public enum FrameHandoffHelperLifecycleOutcome
{
    EntryDidNotAdmitContinuation,
    EntryUnreachedCaptureWasProvided,
    ReceiverIdentityMismatch,
    ContinuationBlocked,
    ContinuationHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffHelperLifecyclePlan(
    FrameHandoffHelperLifecycleCapture Capture,
    FrameHandoffHelperLifecycleOutcome Outcome,
    FrameHandoffHelperEntryPlan EntryPlan,
    bool EntryRouteAdmitted,
    bool EntryUnreachedCapturesClear,
    bool ReceiverIdentityChecked,
    bool ReceiverIdentityMatches,
    FrameHandoffContinuationLifecyclePlan? ContinuationPlan,
    bool UnreachedContinuationReceiverBaseEsiCapture,
    bool UnreachedContinuationReceiverPlus08CaptureProvided,
    bool UnreachedContinuationReceiverPlus10CaptureProvided,
    bool UnreachedContinuationReceiverPlus0COldCaptureProvided,
    bool UnreachedContinuationReceiverPlus0CFreshCaptureProvided,
    bool UnreachedContinuationReceiverPlus04CaptureProvided,
    bool UnreachedContinuationFirstSlotDwordCaptureProvided,
    bool UnreachedContinuationCallEntryValidationProvided,
    bool UnreachedContinuationDirectCallReturnCaptureProvided,
    bool UnreachedContinuationDirectCallReturnEaxCaptureProvided,
    bool UnreachedContinuationZeroRouteIndirectCallReturnCaptureProvided,
    bool UnreachedContinuationZeroRoutePostCallEcxCaptureProvided,
    bool UnreachedContinuationPublicationBranchEntryCaptureProvided,
    bool UnreachedContinuationPublicationIncomingEcxCaptureProvided,
    bool UnreachedContinuationPublicationStoreBase6508CaptureProvided,
    bool UnreachedContinuationPublicationFreshBase650EProvided,
    bool UnreachedContinuationPublicationSlotDwordProvided,
    bool UnreachedContinuationFinalFrame8PointerCaptureProvided,
    bool UnreachedContinuationFinalPointeeDwordCaptureProvided,
    bool UnreachedContinuationFinalReceiver10OldValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the helper entry at raw 0xBE64A0 with its continuation through 0xBE652F.
/// </summary>
public static class FrameHandoffHelperLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset = 0x00BE64A0;
    public const uint LifecycleEndRawFileOffsetExclusive = 0x00BE652F;

    public static FrameHandoffHelperLifecyclePlan Project(
        FrameHandoffHelperLifecycleCapture capture)
    {
        FrameHandoffHelperEntryPlan entryPlan =
            FrameHandoffHelperEntryProjection.Project(capture.Entry);
        bool entryRouteAdmitted = IsAdmittingRoute(entryPlan.Outcome);
        bool entryUnreachedCapturesClear =
            !entryPlan.UnreachedReceiverPlus10CaptureProvided
            && !entryPlan.UnreachedReceiverPlus08CaptureProvided
            && !entryPlan.UnreachedHelperReturnedNormallyCaptureProvided;

        if (!entryRouteAdmitted)
        {
            return StopBeforeContinuation(
                capture,
                entryPlan,
                FrameHandoffHelperLifecycleOutcome.EntryDidNotAdmitContinuation,
                entryRouteAdmitted,
                entryUnreachedCapturesClear);
        }

        if (!entryUnreachedCapturesClear)
        {
            return StopBeforeContinuation(
                capture,
                entryPlan,
                FrameHandoffHelperLifecycleOutcome.EntryUnreachedCaptureWasProvided,
                entryRouteAdmitted,
                entryUnreachedCapturesClear);
        }

        uint continuationReceiver =
            capture.Continuation.SlotCallPublication.Slot.ReceiverBaseEsi;
        bool receiverIdentityMatches =
            capture.Entry.EntryReceiverEcx == continuationReceiver;
        if (!receiverIdentityMatches)
        {
            return StopBeforeContinuation(
                capture,
                entryPlan,
                FrameHandoffHelperLifecycleOutcome.ReceiverIdentityMismatch,
                entryRouteAdmitted,
                entryUnreachedCapturesClear,
                receiverIdentityChecked: true,
                receiverIdentityMatches: false,
                unreachedContinuationReceiverBaseEsi: false);
        }

        FrameHandoffContinuationLifecyclePlan continuationPlan =
            FrameHandoffContinuationLifecycleProjection.Project(capture.Continuation);
        bool continuationCapturesClear =
            !HasUnreachedContinuationCaptures(continuationPlan);
        bool continuationComplete =
            continuationPlan.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete;

        return FromContinuation(
            capture,
            entryPlan,
            continuationPlan,
            continuationComplete && continuationCapturesClear
                ? FrameHandoffHelperLifecycleOutcome.ProjectionComplete
                : continuationComplete
                    ? FrameHandoffHelperLifecycleOutcome.ContinuationHasUnreachedCaptures
                    : FrameHandoffHelperLifecycleOutcome.ContinuationBlocked,
            entryRouteAdmitted,
            entryUnreachedCapturesClear);
    }

    private static bool IsAdmittingRoute(FrameHandoffHelperEntryOutcome outcome) =>
        outcome is FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            or FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength
            or FrameHandoffHelperEntryOutcome.HelperReturnedNormally;

    private static bool HasUnreachedContinuationCaptures(
        FrameHandoffContinuationLifecyclePlan plan)
    {
        FrameHandoffSlotCallPublicationPlan slotCall = plan.SlotCallPublicationPlan;
        FrameHandoffHelperSlotPlan slot = slotCall.SlotPlan;
        FrameHandoffHelperSlotCapture slotCapture = slot.Capture;

        return (slotCapture.ReceiverPlus08.HasValue && !slot.ReceiverPlus08ReadReached)
            || (slotCapture.ReceiverPlus10.HasValue && !slot.ReceiverPlus10ReadReached)
            || (slotCapture.ReceiverPlus0COld.HasValue && !slot.ReceiverPlus0COldReadReached)
            || (slotCapture.ReceiverPlus0CFresh.HasValue && !slot.ReceiverPlus0CFreshReadReached)
            || (slotCapture.ReceiverPlus04.HasValue && !slot.ReceiverPlus04ReadReached)
            || (slotCapture.FirstSlotDword.HasValue && !slot.FirstSlotDwordReadReached)
            || slotCall.UnreachedCallEntryValidationProvided
            || slotCall.UnreachedDirectCallReturnCaptureProvided
            || slotCall.UnreachedDirectCallReturnEaxCaptureProvided
            || slotCall.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            || slotCall.UnreachedZeroRoutePostCallEcxCaptureProvided
            || (slotCall.Capture.Publication.Entry.HasValue
                && !PublicationBranchEntryWasReached(slotCall))
            || slotCall.UnreachedPublicationIncomingEcxCaptureProvided
            || slotCall.UnreachedPublicationStoreBase6508CaptureProvided
            || slotCall.UnreachedPublicationFreshBase650EProvided
            || slotCall.UnreachedPublicationSlotDwordProvided
            || plan.UnreachedFinalFrame8PointerCaptureProvided
            || plan.UnreachedFinalPointeeDwordCaptureProvided
            || plan.UnreachedFinalReceiver10OldValueCaptureProvided;
    }

    private static bool PublicationBranchEntryWasReached(
        FrameHandoffSlotCallPublicationPlan plan) =>
        plan.PublicationPlan is not null
        || plan.Outcome is
            FrameHandoffSlotCallPublicationOutcome.MissingPublicationBranchEntry
            or FrameHandoffSlotCallPublicationOutcome.InvalidPublicationBranchEntry
            or FrameHandoffSlotCallPublicationOutcome.PublicationReceiverIdentityMismatch
            or FrameHandoffSlotCallPublicationOutcome.PublicationEbxIdentityMismatch
            or FrameHandoffSlotCallPublicationOutcome.PublicationEdiIdentityMismatch;

    private static FrameHandoffHelperLifecyclePlan StopBeforeContinuation(
        FrameHandoffHelperLifecycleCapture capture,
        FrameHandoffHelperEntryPlan entryPlan,
        FrameHandoffHelperLifecycleOutcome outcome,
        bool entryRouteAdmitted,
        bool entryUnreachedCapturesClear,
        bool receiverIdentityChecked = false,
        bool receiverIdentityMatches = false,
        bool unreachedContinuationReceiverBaseEsi = true) =>
        new(
            capture,
            outcome,
            entryPlan,
            entryRouteAdmitted,
            entryUnreachedCapturesClear,
            receiverIdentityChecked,
            receiverIdentityMatches,
            null,
            unreachedContinuationReceiverBaseEsi,
            capture.Continuation.SlotCallPublication.Slot.ReceiverPlus08.HasValue,
            capture.Continuation.SlotCallPublication.Slot.ReceiverPlus10.HasValue,
            capture.Continuation.SlotCallPublication.Slot.ReceiverPlus0COld.HasValue,
            capture.Continuation.SlotCallPublication.Slot.ReceiverPlus0CFresh.HasValue,
            capture.Continuation.SlotCallPublication.Slot.ReceiverPlus04.HasValue,
            capture.Continuation.SlotCallPublication.Slot.FirstSlotDword.HasValue,
            capture.Continuation.SlotCallPublication.Call.CaptureValidatedEntry,
            capture.Continuation.SlotCallPublication.Call.DirectCallReturnedNormally.HasValue,
            capture.Continuation.SlotCallPublication.Call.DirectCallReturnEax.HasValue,
            capture.Continuation.SlotCallPublication.Call.ZeroRouteIndirectCallReturnedNormally.HasValue,
            capture.Continuation.SlotCallPublication.Call.ZeroRoutePostCallEcx.HasValue,
            capture.Continuation.SlotCallPublication.Publication.Entry.HasValue,
            capture.Continuation.SlotCallPublication.Publication.IncomingEcx.HasValue,
            capture.Continuation.SlotCallPublication.Publication.ReceiverPlus04At6508.HasValue,
            capture.Continuation.SlotCallPublication.Publication.ReceiverPlus04At650E.HasValue,
            capture.Continuation.SlotCallPublication.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
            capture.Continuation.Final.Frame8Pointer.HasValue,
            capture.Continuation.Final.PointeeDword.HasValue,
            capture.Continuation.Final.Receiver10OldValue.HasValue,
            entryPlan.StopAtRawFileOffsetExclusive);

    private static FrameHandoffHelperLifecyclePlan FromContinuation(
        FrameHandoffHelperLifecycleCapture capture,
        FrameHandoffHelperEntryPlan entryPlan,
        FrameHandoffContinuationLifecyclePlan continuationPlan,
        FrameHandoffHelperLifecycleOutcome outcome,
        bool entryRouteAdmitted,
        bool entryUnreachedCapturesClear)
    {
        FrameHandoffSlotCallPublicationPlan slotCall =
            continuationPlan.SlotCallPublicationPlan;
        FrameHandoffHelperSlotPlan slot = slotCall.SlotPlan;
        FrameHandoffHelperSlotCapture slotCapture = slot.Capture;

        return new(
            capture,
            outcome,
            entryPlan,
            entryRouteAdmitted,
            entryUnreachedCapturesClear,
            true,
            true,
            continuationPlan,
            false,
            slotCapture.ReceiverPlus08.HasValue && !slot.ReceiverPlus08ReadReached,
            slotCapture.ReceiverPlus10.HasValue && !slot.ReceiverPlus10ReadReached,
            slotCapture.ReceiverPlus0COld.HasValue && !slot.ReceiverPlus0COldReadReached,
            slotCapture.ReceiverPlus0CFresh.HasValue && !slot.ReceiverPlus0CFreshReadReached,
            slotCapture.ReceiverPlus04.HasValue && !slot.ReceiverPlus04ReadReached,
            slotCapture.FirstSlotDword.HasValue && !slot.FirstSlotDwordReadReached,
            slotCall.UnreachedCallEntryValidationProvided,
            slotCall.UnreachedDirectCallReturnCaptureProvided,
            slotCall.UnreachedDirectCallReturnEaxCaptureProvided,
            slotCall.UnreachedZeroRouteIndirectCallReturnCaptureProvided,
            slotCall.UnreachedZeroRoutePostCallEcxCaptureProvided,
            slotCall.Capture.Publication.Entry.HasValue
                && !PublicationBranchEntryWasReached(slotCall),
            slotCall.UnreachedPublicationIncomingEcxCaptureProvided,
            slotCall.UnreachedPublicationStoreBase6508CaptureProvided,
            slotCall.UnreachedPublicationFreshBase650EProvided,
            slotCall.UnreachedPublicationSlotDwordProvided,
            continuationPlan.UnreachedFinalFrame8PointerCaptureProvided,
            continuationPlan.UnreachedFinalPointeeDwordCaptureProvided,
            continuationPlan.UnreachedFinalReceiver10OldValueCaptureProvided,
            continuationPlan.StopAtRawFileOffsetExclusive);
    }
}
