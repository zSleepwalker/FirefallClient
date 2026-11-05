namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffHelperEntrySlotCapture(
    FrameHandoffHelperEntryCapture Entry,
    FrameHandoffHelperSlotCapture Slot);

public enum FrameHandoffHelperEntrySlotOutcome
{
    EntryDidNotAdmitContinuation,
    EntryUnreachedCaptureWasProvided,
    ReceiverIdentityMismatch,
    SlotProjected
}

public readonly record struct FrameHandoffHelperEntrySlotPlan(
    FrameHandoffHelperEntrySlotCapture Capture,
    FrameHandoffHelperEntryPlan EntryPlan,
    FrameHandoffHelperEntrySlotOutcome Outcome,
    bool EntryRouteAdmitted,
    bool EntryUnreachedCapturesClear,
    bool ReceiverIdentityChecked,
    bool ReceiverIdentityMatches,
    FrameHandoffHelperSlotPlan? SlotPlan,
    bool UnreachedReceiverBaseEsiCapture,
    bool UnreachedReceiverPlus08CaptureProvided,
    bool UnreachedReceiverPlus10CaptureProvided,
    bool UnreachedReceiverPlus0COldCaptureProvided,
    bool UnreachedReceiverPlus0CFreshCaptureProvided,
    bool UnreachedReceiverPlus04CaptureProvided,
    bool UnreachedFirstSlotDwordCaptureProvided);

/// <summary>
/// Composes the entry gates at BE64A0 with the slot continuation at BE64C5.
/// </summary>
public static class FrameHandoffHelperEntrySlotProjection
{
    public static FrameHandoffHelperEntrySlotPlan Project(
        FrameHandoffHelperEntrySlotCapture capture)
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
            return Stop(
                capture,
                entryPlan,
                FrameHandoffHelperEntrySlotOutcome.EntryDidNotAdmitContinuation,
                entryRouteAdmitted,
                entryUnreachedCapturesClear);
        }

        if (!entryUnreachedCapturesClear)
        {
            return Stop(
                capture,
                entryPlan,
                FrameHandoffHelperEntrySlotOutcome.EntryUnreachedCaptureWasProvided,
                entryRouteAdmitted,
                entryUnreachedCapturesClear);
        }

        bool receiverIdentityMatches =
            capture.Entry.EntryReceiverEcx == capture.Slot.ReceiverBaseEsi;
        if (!receiverIdentityMatches)
        {
            return Stop(
                capture,
                entryPlan,
                FrameHandoffHelperEntrySlotOutcome.ReceiverIdentityMismatch,
                entryRouteAdmitted,
                entryUnreachedCapturesClear,
                receiverIdentityChecked: true,
                receiverIdentityMatches: false,
                unreachedReceiverBaseEsiCapture: false);
        }

        FrameHandoffHelperSlotPlan slotPlan =
            FrameHandoffHelperSlotProjection.Project(capture.Slot);
        return new(
            capture,
            entryPlan,
            FrameHandoffHelperEntrySlotOutcome.SlotProjected,
            entryRouteAdmitted,
            entryUnreachedCapturesClear,
            true,
            true,
            slotPlan,
            false,
            false,
            slotPlan.UnreachedReceiverPlus10CaptureProvided,
            slotPlan.UnreachedReceiverPlus0COldCaptureProvided,
            slotPlan.UnreachedReceiverPlus0CFreshCaptureProvided,
            slotPlan.UnreachedReceiverPlus04CaptureProvided,
            slotPlan.UnreachedFirstSlotDwordCaptureProvided);
    }

    private static bool IsAdmittingRoute(FrameHandoffHelperEntryOutcome outcome) =>
        outcome is FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            or FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength
            or FrameHandoffHelperEntryOutcome.HelperReturnedNormally;

    private static FrameHandoffHelperEntrySlotPlan Stop(
        FrameHandoffHelperEntrySlotCapture capture,
        FrameHandoffHelperEntryPlan entryPlan,
        FrameHandoffHelperEntrySlotOutcome outcome,
        bool entryRouteAdmitted,
        bool entryUnreachedCapturesClear,
        bool receiverIdentityChecked = false,
        bool receiverIdentityMatches = false,
        bool unreachedReceiverBaseEsiCapture = true) =>
        new(
            capture,
            entryPlan,
            outcome,
            entryRouteAdmitted,
            entryUnreachedCapturesClear,
            receiverIdentityChecked,
            receiverIdentityMatches,
            null,
            unreachedReceiverBaseEsiCapture,
            capture.Slot.ReceiverPlus08.HasValue,
            capture.Slot.ReceiverPlus10.HasValue,
            capture.Slot.ReceiverPlus0COld.HasValue,
            capture.Slot.ReceiverPlus0CFresh.HasValue,
            capture.Slot.ReceiverPlus04.HasValue,
            capture.Slot.FirstSlotDword.HasValue);
}
