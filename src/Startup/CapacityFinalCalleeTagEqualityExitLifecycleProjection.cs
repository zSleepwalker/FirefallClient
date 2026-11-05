namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeTagEqualityExitLifecycleCapture(
    CapacityFinalCalleeTagEqualityLifecycleCapture TagEqualityCapture,
    CapacityFinalCalleeExitCapture? ExitCapture);

public enum CapacityFinalCalleeTagEqualityExitLifecycleOutcome
{
    TagPlanNotAdmitted,
    TagRouteCompletedWithoutCapacityExit,
    NonCapacityRouteContaminatedByExitCaptures,
    TagEqualityContinuationIncomplete,
    CapacityExitBlocked,
    CapacityExitReturnedNormally,
    DirectCallReturnedNormallyAtF71,
    DirectCallReturnStatusMissing,
    DirectCallDidNotReturnNormally
}

public readonly record struct CapacityFinalCalleeTagEqualityExitLifecyclePlan(
    CapacityFinalCalleeTagEqualityExitLifecycleCapture Capture,
    CapacityFinalCalleeTagEqualityLifecyclePlan TagEquality,
    CapacityFinalCalleeExitPlan? ExitPlan,
    CapacityFinalCalleeTagEqualityExitLifecycleOutcome Outcome,
    bool ExitCaptureSupplied,
    bool UnreachedExitCaptureProvided,
    bool ExitEntryAdmitted,
    bool ExitReturnedNormally,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins only the exact capacity branch at 9441 to the independent callee-exit
/// captures. Other admitted tag routes remain bounded at their own destinations.
/// </summary>
public static class CapacityFinalCalleeTagEqualityExitLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeTagEqualityLifecycleProjection.EntryRawFileOffset;
    public const uint CapacityExitRawFileOffset =
        CapacityFinalCalleeTagEqualityLifecycleProjection.CapacityExitRawFileOffset;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeTagEqualityExitLifecyclePlan Project(
        CapacityFinalCalleeTagEqualityExitLifecycleCapture capture)
    {
        CapacityFinalCalleeTagEqualityLifecyclePlan tagEquality =
            CapacityFinalCalleeTagEqualityLifecycleProjection.Project(
                capture.TagEqualityCapture);
        bool exitCaptureSupplied = capture.ExitCapture.HasValue;

        if (IsCleanCapacityExit(tagEquality))
        {
            CapacityFinalCalleeExitCapture reachedCapture =
                capture.ExitCapture is CapacityFinalCalleeExitCapture supplied
                    ? supplied with { EntryValidated = true }
                    : new(
                        EntryValidated: true,
                        TestedByte: null,
                        StackPopEdiValue: null,
                        StackPopEsiValue: null,
                        StackPopEbxValue: null,
                        CallCellTargetVirtualAddress: null,
                        CallReturnedNormally: null,
                        CalleeOutputEbx: null,
                        CalleeOutputEdi: null,
                        CalleeOutputEsi: null,
                        StackPopEbpValue: null);
            CapacityFinalCalleeExitPlan exitPlan =
                CapacityFinalCalleeExitProjection.Project(reachedCapture);
            bool exitReturnedNormally = IsCleanReturnedExit(exitPlan);
            return new(
                capture,
                tagEquality,
                exitPlan,
                exitReturnedNormally
                    ? CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .CapacityExitReturnedNormally
                    : CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .CapacityExitBlocked,
                exitCaptureSupplied,
                UnreachedExitCaptureProvided: false,
                ExitEntryAdmitted: true,
                exitReturnedNormally,
                exitReturnedNormally,
                exitReturnedNormally
                    ? ReturnBoundaryRawFileOffsetExclusive
                    : exitPlan.StopAtRawFileOffsetExclusive);
        }

        if (tagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallReturnedNormallyAtF71
            && tagEquality.Admitted)
        {
            return NonCapacityRoute(
                capture,
                tagEquality,
                exitCaptureSupplied,
                exitCaptureSupplied,
                exitCaptureSupplied
                    ? CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .NonCapacityRouteContaminatedByExitCaptures
                    : CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .DirectCallReturnedNormallyAtF71);
        }

        if (tagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .TagRouteCompletedWithoutEqualityBoundary
            && tagEquality.Admitted)
        {
            return NonCapacityRoute(
                capture,
                tagEquality,
                exitCaptureSupplied,
                exitCaptureSupplied,
                exitCaptureSupplied
                    ? CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .NonCapacityRouteContaminatedByExitCaptures
                    : CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .TagRouteCompletedWithoutCapacityExit);
        }

        CapacityFinalCalleeTagEqualityExitLifecycleOutcome outcome =
            tagEquality.Outcome switch
            {
                CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallReturnStatusMissing =>
                    CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .DirectCallReturnStatusMissing,
                CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallDidNotReturnNormally =>
                    CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .DirectCallDidNotReturnNormally,
                CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .ContinuationIncomplete =>
                    CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                        .TagEqualityContinuationIncomplete,
                _ => CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .TagPlanNotAdmitted
            };
        return new(
            capture,
            tagEquality,
            ExitPlan: null,
            outcome,
            exitCaptureSupplied,
            exitCaptureSupplied,
            ExitEntryAdmitted: false,
            ExitReturnedNormally: false,
            Admitted: false,
            tagEquality.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeTagEqualityExitLifecyclePlan NonCapacityRoute(
        CapacityFinalCalleeTagEqualityExitLifecycleCapture capture,
        CapacityFinalCalleeTagEqualityLifecyclePlan tagEquality,
        bool exitCaptureSupplied,
        bool unreachedExitCaptureProvided,
        CapacityFinalCalleeTagEqualityExitLifecycleOutcome outcome)
    {
        bool admitted = !unreachedExitCaptureProvided;
        return new(
            capture,
            tagEquality,
            ExitPlan: null,
            outcome,
            exitCaptureSupplied,
            unreachedExitCaptureProvided,
            ExitEntryAdmitted: false,
            ExitReturnedNormally: false,
            admitted,
            tagEquality.StopAtRawFileOffsetExclusive);
    }

    private static bool IsCleanCapacityExit(
        CapacityFinalCalleeTagEqualityLifecyclePlan plan) =>
        plan.Admitted
        && plan.EqualityBoundaryAdmitted
        && plan.ContinuationReached
        && plan.Outcome
            == CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441
        && plan.StopAtRawFileOffsetExclusive == CapacityExitRawFileOffset
        && plan.Continuation is CapacityFinalCalleeTagEqualityContinuationPlan continuation
        && continuation.Outcome
            == CapacityFinalCalleeTagEqualityContinuationOutcome.CapacityUnsignedBelowOrEqual
        && continuation.CapacityJbeReached
        && continuation.CapacityJbeTaken == true
        && continuation.CapacityBranchDestinationRawFileOffset
            == CapacityExitRawFileOffset
        && continuation.StopAtRawFileOffsetExclusive == CapacityExitRawFileOffset
        && !continuation.CallReached
        && !continuation.UnreachedInitialGlobalDwordCaptureProvided
        && !continuation.UnreachedFreshTagDwordCaptureProvided
        && !continuation.UnreachedCapacityDwordCaptureProvided
        && !continuation.UnreachedCallReturnStatusProvided;

    private static bool IsCleanReturnedExit(CapacityFinalCalleeExitPlan plan) =>
        (plan.Outcome is CapacityFinalCalleeExitOutcome.CallSkipped
            or CapacityFinalCalleeExitOutcome.ReturnedNormally)
        && plan.ReturnReached
        && plan.StopAtRawFileOffsetExclusive
            == ReturnBoundaryRawFileOffsetExclusive
        && !plan.UnreachedTestedByteCaptureProvided
        && !plan.UnreachedStackPopEdiCaptureProvided
        && !plan.UnreachedStackPopEsiCaptureProvided
        && !plan.UnreachedStackPopEbxCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.UnreachedCalleeOutputEbxCaptureProvided
        && !plan.UnreachedCalleeOutputEdiCaptureProvided
        && !plan.UnreachedCalleeOutputEsiCaptureProvided
        && !plan.UnreachedStackPopEbpCaptureProvided;
}
