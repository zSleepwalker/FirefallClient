namespace Firefall.Reconstruction.Startup;

public enum UnreliableReceiveMarkerScalarOutcome
{
    MarkerCallbackDidNotReturnNormally,
    UnreachedScalarCaptureProvided,
    MissingScalarCapture,
    ScalarOutcomeMissingPriorIndirectCallReturn,
    ScalarOutcomeUnreachedCaptureProvided,
    ScalarOutcomePriorIndirectCallDidNotReturnNormally,
    ScalarOutcomeMissingFrameByteAt8,
    ScalarOutcomeMissingPriorCallbackEax,
    ScalarOutcomeMissingHelperReturn,
    ScalarOutcomeHelperDidNotReturnNormally,
    ScalarOutcomeHelperReturnedNormally
}

public readonly record struct UnreliableReceiveMarkerScalarPlan(
    UnreliableReceiveMarkerCallbackPlan MarkerCallbackPlan,
    UnreliableReceiveCallbackScalarPlan? ScalarPlan,
    UnreliableReceiveMarkerScalarOutcome Outcome,
    bool UnreachedScalarCaptureProvided,
    bool ScalarProjectionReached,
    bool ScalarOutcomeHelperReturnedNormally,
    bool NormalCallBoundaryReached,
    byte? StackCleanupBytes,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the validated marker callback with the later scalar helper handoff.
/// </summary>
public static class UnreliableReceiveMarkerScalarProjection
{
    public static UnreliableReceiveMarkerScalarPlan Project(
        UnreliableReceiveMarkerCallbackCapture markerCallbackCapture,
        UnreliableReceiveCallbackScalarCapture? scalarCapture)
    {
        UnreliableReceiveMarkerCallbackPlan markerPlan =
            UnreliableReceiveMarkerCallbackProjection.Project(markerCallbackCapture);

        if (markerPlan.Outcome
            != UnreliableReceiveMarkerCallbackOutcome.IndirectCallReturnedNormally)
        {
            return Stop(
                markerPlan,
                outcome: scalarCapture.HasValue
                    ? UnreliableReceiveMarkerScalarOutcome.UnreachedScalarCaptureProvided
                    : UnreliableReceiveMarkerScalarOutcome.MarkerCallbackDidNotReturnNormally,
                unreachedScalarCaptureProvided: scalarCapture.HasValue,
                stopAt: markerPlan.StopAtRawFileOffsetExclusive);
        }

        if (scalarCapture is not UnreliableReceiveCallbackScalarCapture capturedScalar)
        {
            return Stop(
                markerPlan,
                outcome: UnreliableReceiveMarkerScalarOutcome.MissingScalarCapture,
                stopAt: UnreliableReceiveCallbackScalarProjection.EntryRawFileOffset);
        }

        UnreliableReceiveCallbackScalarPlan scalarPlan =
            UnreliableReceiveCallbackScalarProjection.Project(capturedScalar);

        UnreliableReceiveMarkerScalarOutcome outcome = scalarPlan.Outcome switch
        {
            UnreliableReceiveCallbackScalarOutcome.MissingPriorIndirectCallReturn =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeMissingPriorIndirectCallReturn,
            UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeUnreachedCaptureProvided,
            UnreliableReceiveCallbackScalarOutcome.PriorIndirectCallDidNotReturnNormally =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomePriorIndirectCallDidNotReturnNormally,
            UnreliableReceiveCallbackScalarOutcome.MissingFrameByteAt8 =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeMissingFrameByteAt8,
            UnreliableReceiveCallbackScalarOutcome.MissingPriorCallbackEax =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeMissingPriorCallbackEax,
            UnreliableReceiveCallbackScalarOutcome.MissingHelperReturn =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeMissingHelperReturn,
            UnreliableReceiveCallbackScalarOutcome.HelperDidNotReturnNormally =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperDidNotReturnNormally,
            UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally =>
                UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperReturnedNormally,
            _ => throw new ArgumentOutOfRangeException(
                nameof(scalarPlan),
                scalarPlan.Outcome,
                "The scalar projection returned an unsupported outcome.")
        };

        bool completed =
            outcome == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperReturnedNormally;
        return Stop(
            markerPlan,
            scalarPlan,
            outcome,
            scalarProjectionReached: true,
            scalarOutcomeHelperReturnedNormally: completed,
            normalCallBoundaryReached: completed && scalarPlan.NormalCallBoundaryReached,
            stackCleanupBytes: completed ? scalarPlan.StackCleanupBytes : null,
            selectedDestinationRawFileOffset:
                completed ? scalarPlan.SelectedDestinationRawFileOffset : null,
            stopAt: scalarPlan.StopAtRawFileOffsetExclusive);
    }

    private static UnreliableReceiveMarkerScalarPlan Stop(
        UnreliableReceiveMarkerCallbackPlan markerCallbackPlan,
        UnreliableReceiveCallbackScalarPlan? scalarPlan = null,
        UnreliableReceiveMarkerScalarOutcome outcome =
            UnreliableReceiveMarkerScalarOutcome.MarkerCallbackDidNotReturnNormally,
        bool unreachedScalarCaptureProvided = false,
        bool scalarProjectionReached = false,
        bool scalarOutcomeHelperReturnedNormally = false,
        bool normalCallBoundaryReached = false,
        byte? stackCleanupBytes = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset) =>
        new(
            markerCallbackPlan,
            scalarPlan,
            outcome,
            unreachedScalarCaptureProvided,
            scalarProjectionReached,
            scalarOutcomeHelperReturnedNormally,
            normalCallBoundaryReached,
            stackCleanupBytes,
            selectedDestinationRawFileOffset,
            stopAt);
}
