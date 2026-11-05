namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveBelowThresholdReturnCapture(
    UnreliableReceiveResultStoresCapture Stores,
    bool? HelperReturnedNormally,
    uint? HelperReturnEax);

public enum UnreliableReceiveBelowThresholdReturnOutcome
{
    StoresDidNotReachCounterBelowThreshold,
    CounterAtOrAboveThresholdHandoffUnreached,
    FrameHandoffEntryNotValidated,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    ReturnReached
}

public readonly record struct UnreliableReceiveBelowThresholdReturnPlan(
    UnreliableReceiveBelowThresholdReturnCapture Capture,
    UnreliableReceiveBelowThresholdReturnOutcome Outcome,
    UnreliableReceiveResultStoresPlan Stores,
    UnreliableReceiveFrameHandoffCapture? FrameHandoffCapture,
    UnreliableReceiveFrameHandoffPlan? FrameHandoff,
    bool UnreachedHelperReturnedNormallyCaptureProvided,
    bool UnreachedHelperReturnEaxCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the result stores to the frame handoff only when the unsigned counter is below threshold.
/// </summary>
public static class UnreliableReceiveBelowThresholdReturnProjection
{
    public const uint CounterAtOrAboveThresholdStopRawFileOffsetExclusive = 0x00BE5BC4;
    public const uint FrameHandoffEntryRawFileOffset = 0x00BE5C0F;

    public static UnreliableReceiveBelowThresholdReturnPlan Project(
        UnreliableReceiveBelowThresholdReturnCapture capture)
    {
        UnreliableReceiveResultStoresPlan stores =
            UnreliableReceiveResultStoresProjection.Project(capture.Stores);
        if (stores.Outcome != UnreliableReceiveResultStoresOutcome.CounterBelowThreshold)
        {
            bool atOrAboveThreshold =
                stores.Outcome == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold;
            return new UnreliableReceiveBelowThresholdReturnPlan(
                capture,
                atOrAboveThreshold
                    ? UnreliableReceiveBelowThresholdReturnOutcome.CounterAtOrAboveThresholdHandoffUnreached
                    : UnreliableReceiveBelowThresholdReturnOutcome.StoresDidNotReachCounterBelowThreshold,
                stores,
                FrameHandoffCapture: null,
                FrameHandoff: null,
                UnreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                UnreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                StopAtRawFileOffsetExclusive: atOrAboveThreshold
                    ? CounterAtOrAboveThresholdStopRawFileOffsetExclusive
                    : stores.StopAtRawFileOffsetExclusive);
        }

        var handoffCapture = new UnreliableReceiveFrameHandoffCapture(
            ValidatedEntry: true,
            FrameEbp: stores.Capture.FrameEbp,
            ReceiverEdi: stores.Capture.ReceiverEdi,
            HelperReturnedNormally: capture.HelperReturnedNormally,
            HelperReturnEax: capture.HelperReturnEax);
        UnreliableReceiveFrameHandoffPlan handoff =
            UnreliableReceiveFrameHandoffProjection.Project(handoffCapture);

        UnreliableReceiveBelowThresholdReturnOutcome outcome = handoff.Outcome switch
        {
            UnreliableReceiveFrameHandoffOutcome.EntryNotValidated =>
                UnreliableReceiveBelowThresholdReturnOutcome.FrameHandoffEntryNotValidated,
            UnreliableReceiveFrameHandoffOutcome.MissingHelperReturnCapture =>
                UnreliableReceiveBelowThresholdReturnOutcome.MissingHelperReturnCapture,
            UnreliableReceiveFrameHandoffOutcome.HelperDidNotReturnNormally =>
                UnreliableReceiveBelowThresholdReturnOutcome.HelperDidNotReturnNormally,
            UnreliableReceiveFrameHandoffOutcome.ReturnReached =>
                UnreliableReceiveBelowThresholdReturnOutcome.ReturnReached,
            _ => throw new ArgumentOutOfRangeException(nameof(handoff))
        };

        return new UnreliableReceiveBelowThresholdReturnPlan(
            capture,
            outcome,
            stores,
            handoffCapture,
            handoff,
            UnreachedHelperReturnedNormallyCaptureProvided: false,
            UnreachedHelperReturnEaxCaptureProvided:
                handoff.UnreachedHelperReturnEaxCaptureProvided,
            StopAtRawFileOffsetExclusive: handoff.StopAtRawFileOffsetExclusive);
    }
}
