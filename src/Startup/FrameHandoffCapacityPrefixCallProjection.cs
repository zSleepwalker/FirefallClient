namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityPrefixCallCapture(
    FrameHandoffCapacityPrefixCapture PrefixCapture,
    FrameHandoffCapacityCallCapture? CallCapture);

public enum FrameHandoffCapacityPrefixCallOutcome
{
    MissingPrefixReceiverCRead,
    PrefixZeroBranchToSuccessJoin,
    PrefixZeroRejectedUnreachedCallCaptures,
    CallCaptureMissing,
    InvalidSuppliedCallRoute,
    SuppliedCallRouteMismatch,
    FrameEbpMismatch,
    CallProjected
}

public readonly record struct FrameHandoffCapacityPrefixCallPlan(
    FrameHandoffCapacityPrefixCallCapture Capture,
    FrameHandoffCapacityPrefixCallOutcome Outcome,
    FrameHandoffCapacityPrefixPlan PrefixPlan,
    FrameHandoffCapacityCallPlan? CallPlan,
    FrameHandoffCapacityCallEntryRoute? ExpectedCallRoute,
    bool CallRouteDerivedFromPrefix,
    uint? EsiAfterSubtract,
    uint? EdiAfterShift,
    bool FrameEbpIdentityValidated,
    bool? FrameEbpIdentityMatches,
    bool CallReached,
    bool UnreachedCallEntryRouteCaptureProvided,
    bool UnreachedDirectCallReturnCaptureProvided,
    bool UnreachedDirectCallReturnEaxCaptureProvided,
    bool UnreachedGuardIndirectCallReturnCaptureProvided,
    bool SuccessJoinReached,
    bool SuccessAdmission,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the capacity prefix and call, deriving the only legal call route from the prefix branch.
/// </summary>
public static class FrameHandoffCapacityPrefixCallProjection
{
    public const uint SuccessJoinRawFileOffset = 0x00BE6BCA;

    public static FrameHandoffCapacityPrefixCallPlan Project(
        FrameHandoffCapacityPrefixCallCapture capture)
    {
        FrameHandoffCapacityPrefixPlan prefixPlan =
            FrameHandoffCapacityPrefixProjection.Project(capture.PrefixCapture);
        FrameHandoffCapacityCallCapture? suppliedCall = capture.CallCapture;

        if (prefixPlan.Outcome == FrameHandoffCapacityPrefixOutcome.MissingReceiverCRead)
        {
            return StopWithoutCall(
                capture,
                FrameHandoffCapacityPrefixCallOutcome.MissingPrefixReceiverCRead,
                prefixPlan,
                expectedRoute: null,
                successJoinReached: false,
                stopAt: prefixPlan.StopAtRawFileOffsetExclusive);
        }

        if (prefixPlan.Outcome
            == FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA)
        {
            bool routeCapture = suppliedCall?.EntryRoute.HasValue == true;
            bool directStatusCapture = suppliedCall?.DirectCallReturnedNormally.HasValue == true;
            bool directEaxCapture = suppliedCall?.DirectCallReturnEax.HasValue == true;
            bool guardStatusCapture =
                suppliedCall?.GuardIndirectCallReturnedNormally.HasValue == true;
            bool unexpectedCallCapture = routeCapture
                || directStatusCapture
                || directEaxCapture
                || guardStatusCapture;

            return new(
                capture,
                unexpectedCallCapture
                    ? FrameHandoffCapacityPrefixCallOutcome.PrefixZeroRejectedUnreachedCallCaptures
                    : FrameHandoffCapacityPrefixCallOutcome.PrefixZeroBranchToSuccessJoin,
                prefixPlan,
                null,
                null,
                false,
                prefixPlan.EsiAfterSubtract,
                prefixPlan.EdiAfterShift,
                false,
                null,
                false,
                routeCapture,
                directStatusCapture,
                directEaxCapture,
                guardStatusCapture,
                !unexpectedCallCapture,
                !unexpectedCallCapture,
                SuccessJoinRawFileOffset);
        }

        FrameHandoffCapacityCallEntryRoute expectedRoute =
            prefixPlan.Outcome switch
            {
                FrameHandoffCapacityPrefixOutcome.AboveGuardBranchToRawFileOffsetBC4 =>
                    FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4,
                FrameHandoffCapacityPrefixOutcome.ScaledPushBeforeCall =>
                    FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                _ => throw new InvalidOperationException(
                    $"Unexpected capacity-prefix outcome: {prefixPlan.Outcome}")
            };

        uint callEntryRawFileOffset = expectedRoute
            == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            ? FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset
            : FrameHandoffCapacityCallProjection.DirectCallRawFileOffset;

        if (suppliedCall is not FrameHandoffCapacityCallCapture callCapture)
        {
            return StopWithoutCall(
                capture,
                FrameHandoffCapacityPrefixCallOutcome.CallCaptureMissing,
                prefixPlan,
                expectedRoute,
                successJoinReached: false,
                stopAt: callEntryRawFileOffset);
        }

        if (callCapture.EntryRoute is FrameHandoffCapacityCallEntryRoute suppliedRoute)
        {
            if (suppliedRoute is not (FrameHandoffCapacityCallEntryRoute.DirectCallB5
                or FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4))
            {
                return StopWithoutCall(
                    capture,
                    FrameHandoffCapacityPrefixCallOutcome.InvalidSuppliedCallRoute,
                    prefixPlan,
                    expectedRoute,
                    successJoinReached: false,
                    stopAt: callEntryRawFileOffset);
            }

            if (suppliedRoute != expectedRoute)
            {
                return StopWithoutCall(
                    capture,
                    FrameHandoffCapacityPrefixCallOutcome.SuppliedCallRouteMismatch,
                    prefixPlan,
                    expectedRoute,
                    successJoinReached: false,
                    stopAt: callEntryRawFileOffset);
            }
        }

        bool frameEbpMatches = callCapture.FrameEbp == capture.PrefixCapture.EntryEbp;
        if (!frameEbpMatches)
        {
            return StopWithoutCall(
                capture,
                FrameHandoffCapacityPrefixCallOutcome.FrameEbpMismatch,
                prefixPlan,
                expectedRoute,
                successJoinReached: false,
                stopAt: callEntryRawFileOffset,
                frameEbpIdentityValidated: true,
                frameEbpIdentityMatches: false);
        }

        FrameHandoffCapacityCallCapture derivedCallCapture =
            callCapture with { EntryRoute = expectedRoute };
        FrameHandoffCapacityCallPlan callPlan =
            FrameHandoffCapacityCallProjection.Project(derivedCallCapture);
        bool successJoinReached = callPlan.NormalBoundaryReached;
        bool successAdmission = successJoinReached
            && callPlan.SuccessAdmission
            && callPlan.Capture.EntryRoute == expectedRoute;

        return new(
            capture,
            FrameHandoffCapacityPrefixCallOutcome.CallProjected,
            prefixPlan,
            callPlan,
            expectedRoute,
            true,
            prefixPlan.EsiAfterSubtract,
            prefixPlan.EdiAfterShift,
            true,
            true,
            true,
            false,
            callPlan.UnreachedDirectCallReturnCaptureProvided,
            callPlan.UnreachedDirectCallReturnEaxCaptureProvided,
            callPlan.UnreachedGuardIndirectCallReturnCaptureProvided,
            successJoinReached,
            successAdmission,
            callPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityPrefixCallPlan StopWithoutCall(
        FrameHandoffCapacityPrefixCallCapture capture,
        FrameHandoffCapacityPrefixCallOutcome outcome,
        FrameHandoffCapacityPrefixPlan prefixPlan,
        FrameHandoffCapacityCallEntryRoute? expectedRoute,
        bool successJoinReached,
        uint stopAt,
        bool frameEbpIdentityValidated = false,
        bool? frameEbpIdentityMatches = null)
    {
        FrameHandoffCapacityCallCapture? callCapture = capture.CallCapture;

        return new(
            capture,
            outcome,
            prefixPlan,
            null,
            expectedRoute,
            expectedRoute.HasValue,
            prefixPlan.EsiAfterSubtract,
            prefixPlan.EdiAfterShift,
            frameEbpIdentityValidated,
            frameEbpIdentityMatches,
            false,
            callCapture?.EntryRoute.HasValue == true,
            callCapture?.DirectCallReturnedNormally.HasValue == true,
            callCapture?.DirectCallReturnEax.HasValue == true,
            callCapture?.GuardIndirectCallReturnedNormally.HasValue == true,
            successJoinReached,
            false,
            stopAt);
    }
}
