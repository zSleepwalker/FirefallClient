namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffContinuationLifecycleCapture(
    FrameHandoffSlotCallPublicationCapture SlotCallPublication,
    FrameHandoffFinalWriteReturnCapture Final);

public enum FrameHandoffContinuationLifecycleOutcome
{
    SlotCallPublicationBlocked,
    SlotCallPublicationHasUnreachedCaptures,
    ReturnProjectionBlocked,
    ProjectionComplete
}

public readonly record struct FrameHandoffContinuationLifecyclePlan(
    FrameHandoffContinuationLifecycleCapture Capture,
    FrameHandoffContinuationLifecycleOutcome Outcome,
    FrameHandoffSlotCallPublicationPlan SlotCallPublicationPlan,
    FrameHandoffPublicationReturnPlan? ReturnPlan,
    bool UnreachedFinalFrame8PointerCaptureProvided,
    bool UnreachedFinalPointeeDwordCaptureProvided,
    bool UnreachedFinalReceiver10OldValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the helper continuation at raw 0xBE64C5 through the shared return at 0xBE652F.
/// The existing slot/call and publication/return projections remain authoritative for their
/// respective slices; this composition adds no entry gate or allocation behavior.
/// </summary>
public static class FrameHandoffContinuationLifecycleProjection
{
    public const uint ContinuationStartRawFileOffset = 0x00BE64C5;
    public const uint ReturnBoundaryRawFileOffsetExclusive = 0x00BE652F;

    public static FrameHandoffContinuationLifecyclePlan Project(
        FrameHandoffContinuationLifecycleCapture capture)
    {
        FrameHandoffSlotCallPublicationPlan firstPlan =
            FrameHandoffSlotCallPublicationProjection.Project(
                capture.SlotCallPublication);

        if (firstPlan.Outcome != FrameHandoffSlotCallPublicationOutcome.ProjectionComplete)
        {
            return Stop(
                capture,
                FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked,
                firstPlan,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: firstPlan.StopAtRawFileOffsetExclusive);
        }

        if (HasUnreachedSlotCallPublicationCaptures(firstPlan))
        {
            return Stop(
                capture,
                FrameHandoffContinuationLifecycleOutcome
                    .SlotCallPublicationHasUnreachedCaptures,
                firstPlan,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: firstPlan.StopAtRawFileOffsetExclusive);
        }

        if (firstPlan.PublicationPlan is not
            { Outcome: FrameHandoffSlotPublicationOutcome.ProjectionComplete } effectivePlan)
        {
            return Stop(
                capture,
                FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked,
                firstPlan,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: firstPlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffPublicationReturnPlan returnPlan =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: effectivePlan.Capture,
                Final: capture.Final));

        bool complete =
            returnPlan.Outcome == FrameHandoffPublicationReturnOutcome.ProjectionComplete;
        return Stop(
            capture,
            complete
                ? FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
                : FrameHandoffContinuationLifecycleOutcome.ReturnProjectionBlocked,
            firstPlan,
            returnPlan,
            returnPlan.UnreachedFinalFrame8PointerCaptureProvided,
            returnPlan.UnreachedFinalPointeeDwordCaptureProvided,
            returnPlan.UnreachedFinalReceiver10OldValueCaptureProvided,
            returnPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool HasUnreachedSlotCallPublicationCaptures(
        FrameHandoffSlotCallPublicationPlan plan) =>
        plan.UnreachedCallEntryValidationProvided
        || plan.UnreachedDirectCallReturnCaptureProvided
        || plan.UnreachedDirectCallReturnEaxCaptureProvided
        || plan.UnreachedZeroRouteIndirectCallReturnCaptureProvided
        || plan.UnreachedZeroRoutePostCallEcxCaptureProvided
        || plan.UnreachedPublicationIncomingEcxCaptureProvided
        || plan.UnreachedPublicationStoreBase6508CaptureProvided
        || plan.UnreachedPublicationFreshBase650EProvided
        || plan.UnreachedPublicationSlotDwordProvided;

    private static FrameHandoffContinuationLifecyclePlan Stop(
        FrameHandoffContinuationLifecycleCapture capture,
        FrameHandoffContinuationLifecycleOutcome outcome,
        FrameHandoffSlotCallPublicationPlan slotCallPublicationPlan,
        FrameHandoffPublicationReturnPlan? returnPlan = null,
        bool unreachedFinalFrame8Pointer = false,
        bool unreachedFinalPointeeDword = false,
        bool unreachedFinalReceiver10OldValue = false,
        uint stopAt = ReturnBoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            slotCallPublicationPlan,
            returnPlan,
            unreachedFinalFrame8Pointer,
            unreachedFinalPointeeDword,
            unreachedFinalReceiver10OldValue,
            stopAt);
}
