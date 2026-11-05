namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFinalLifecycleCapture(
    FrameHandoffCapacityFinalCallGateCapture Gate,
    FrameHandoffCapacityFinalReturnCapture Final);

public enum FrameHandoffCapacityFinalLifecycleOutcome
{
    GateBlocked,
    GateHasUnreachedCaptures,
    FinalEntryEbpMismatch,
    ZeroBypassEdiMismatch,
    FinalBlocked,
    FinalHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityFinalLifecyclePlan(
    FrameHandoffCapacityFinalLifecycleCapture Capture,
    FrameHandoffCapacityFinalLifecycleOutcome Outcome,
    FrameHandoffCapacityFinalCallGatePlan GatePlan,
    FrameHandoffCapacityFinalReturnPlan? FinalPlan,
    bool RequireNormalBoundaryAdmission,
    bool NormalBoundaryAdmissionSatisfied,
    bool GateOutcomeClean,
    bool GateUnreachedCapturesClear,
    bool FinalEntryEbpValidated,
    bool FinalEntryEbpMatches,
    bool ZeroBypassEdiValidated,
    bool ZeroBypassEdiMatches,
    bool DeriveValidatedNormalTail,
    bool FinalOutcomeClean,
    bool UnreachedFinalReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the final call gate with the receiver update and RET 4 tail.
/// </summary>
public static class FrameHandoffCapacityFinalLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityFinalLifecyclePlan Project(
        FrameHandoffCapacityFinalLifecycleCapture capture)
    {
        FrameHandoffCapacityFinalCallGatePlan gatePlan =
            FrameHandoffCapacityFinalCallGateProjection.Project(capture.Gate);
        bool gateUnreachedCapturesClear =
            !gatePlan.UnreachedFrameMinus14CaptureProvided
            && !gatePlan.UnreachedReceiver4CaptureProvided
            && !gatePlan.UnreachedNormalCallStatusProvided;
        bool gateOutcomeClean = gatePlan.Outcome is
            FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            or FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned;
        bool normalBoundaryAdmissionSatisfied =
            gatePlan.NormalBoundaryAdmitted && gateOutcomeClean;

        if (!normalBoundaryAdmissionSatisfied || !gateUnreachedCapturesClear)
        {
            return StopBeforeFinal(
                capture,
                gatePlan,
                gateUnreachedCapturesClear
                    ? FrameHandoffCapacityFinalLifecycleOutcome.GateBlocked
                    : FrameHandoffCapacityFinalLifecycleOutcome.GateHasUnreachedCaptures,
                gateOutcomeClean,
                gateUnreachedCapturesClear,
                normalBoundaryAdmissionSatisfied,
                stopAt: gatePlan.StopAtRawFileOffsetExclusive);
        }

        bool finalEntryEbpMatches = capture.Final.EntryEbp == capture.Gate.EntryEbp;
        if (!finalEntryEbpMatches)
        {
            return StopBeforeFinal(
                capture,
                gatePlan,
                FrameHandoffCapacityFinalLifecycleOutcome.FinalEntryEbpMismatch,
                gateOutcomeClean,
                gateUnreachedCapturesClear,
                normalBoundaryAdmissionSatisfied,
                finalEntryEbpValidated: true,
                finalEntryEbpMatches: false,
                stopAt: FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);
        }

        bool zeroBypass = gatePlan.Outcome
            == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped;
        bool zeroBypassEdiMatches =
            !zeroBypass || gatePlan.OutgoingEdi == capture.Final.EntryEdi;
        if (!zeroBypassEdiMatches)
        {
            return StopBeforeFinal(
                capture,
                gatePlan,
                FrameHandoffCapacityFinalLifecycleOutcome.ZeroBypassEdiMismatch,
                gateOutcomeClean,
                gateUnreachedCapturesClear,
                normalBoundaryAdmissionSatisfied,
                finalEntryEbpValidated: true,
                finalEntryEbpMatches: true,
                zeroBypassEdiValidated: true,
                zeroBypassEdiMatches: false,
                stopAt: FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityFinalReturnCapture derivedFinalCapture =
            capture.Final with { ValidatedNormalTail = true };
        FrameHandoffCapacityFinalReturnPlan finalPlan =
            FrameHandoffCapacityFinalReturnProjection.Project(derivedFinalCapture);
        bool finalUnreachedCaptureProvided =
            finalPlan.UnreachedReceiver8OldCaptureProvided;
        bool finalOutcomeClean =
            finalPlan.Outcome == FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally
            && finalPlan.ReturnReached
            && !finalUnreachedCaptureProvided;
        FrameHandoffCapacityFinalLifecycleOutcome outcome =
            finalOutcomeClean
                ? FrameHandoffCapacityFinalLifecycleOutcome.ProjectionComplete
                : finalUnreachedCaptureProvided
                    ? FrameHandoffCapacityFinalLifecycleOutcome.FinalHasUnreachedCaptures
                    : FrameHandoffCapacityFinalLifecycleOutcome.FinalBlocked;

        return new(
            capture,
            outcome,
            gatePlan,
            finalPlan,
            RequireNormalBoundaryAdmission: true,
            normalBoundaryAdmissionSatisfied,
            gateOutcomeClean,
            gateUnreachedCapturesClear,
            FinalEntryEbpValidated: true,
            FinalEntryEbpMatches: true,
            ZeroBypassEdiValidated: zeroBypass,
            zeroBypassEdiMatches,
            DeriveValidatedNormalTail: true,
            finalOutcomeClean,
            UnreachedFinalReceiver8OldCaptureProvided: false,
            finalPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityFinalLifecyclePlan StopBeforeFinal(
        FrameHandoffCapacityFinalLifecycleCapture capture,
        FrameHandoffCapacityFinalCallGatePlan gatePlan,
        FrameHandoffCapacityFinalLifecycleOutcome outcome,
        bool gateOutcomeClean,
        bool gateUnreachedCapturesClear,
        bool normalBoundaryAdmissionSatisfied,
        bool finalEntryEbpValidated = false,
        bool finalEntryEbpMatches = false,
        bool zeroBypassEdiValidated = false,
        bool zeroBypassEdiMatches = false,
        uint stopAt = LifecycleStartRawFileOffset) =>
        new(
            capture,
            outcome,
            gatePlan,
            null,
            RequireNormalBoundaryAdmission: true,
            normalBoundaryAdmissionSatisfied,
            gateOutcomeClean,
            gateUnreachedCapturesClear,
            finalEntryEbpValidated,
            finalEntryEbpMatches,
            zeroBypassEdiValidated,
            zeroBypassEdiMatches,
            DeriveValidatedNormalTail: false,
            FinalOutcomeClean: false,
            UnreachedFinalReceiver8OldCaptureProvided: capture.Final.Receiver8Old.HasValue,
            stopAt);
}
