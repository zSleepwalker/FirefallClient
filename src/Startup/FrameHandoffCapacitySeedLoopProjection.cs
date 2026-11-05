namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySeedLoopCapture(
    FrameHandoffCapacitySeedCapture SeedCapture,
    uint? ExpectedLoopEsi = null,
    uint? ExpectedLoopEdx = null);

public enum FrameHandoffCapacitySeedLoopOutcome
{
    MissingReceiverPlus08Capture,
    LoopIdentityCaptureMismatch,
    OneLoopIterationProjected
}

public readonly record struct FrameHandoffCapacitySeedLoopPlan(
    FrameHandoffCapacitySeedLoopCapture Capture,
    FrameHandoffCapacitySeedPlan SeedPlan,
    FrameHandoffCapacitySeedLoopOutcome Outcome,
    bool LoopIdentityCapturesReached,
    uint? ActualLoopEsi,
    uint? ActualLoopEdx,
    bool? ExpectedLoopEsiMatched,
    bool? ExpectedLoopEdxMatched,
    FrameHandoffCapacityLoopPlan? LoopPlan,
    uint StopAtRawFileOffsetExclusive)
{
    public bool UnreachedExpectedLoopEsiCaptureProvided =>
        !LoopIdentityCapturesReached && Capture.ExpectedLoopEsi.HasValue;
    public bool UnreachedExpectedLoopEdxCaptureProvided =>
        !LoopIdentityCapturesReached && Capture.ExpectedLoopEdx.HasValue;
}

/// <summary>
/// Composes the capacity seed with at most one capacity-loop iteration.
/// </summary>
public static class FrameHandoffCapacitySeedLoopProjection
{
    public static FrameHandoffCapacitySeedLoopPlan Project(
        FrameHandoffCapacitySeedLoopCapture capture)
    {
        FrameHandoffCapacitySeedPlan seedPlan =
            FrameHandoffCapacitySeedProjection.Project(capture.SeedCapture);

        if (seedPlan.ReceiverPlus08ReadValue is not uint actualLoopEdx
            || seedPlan.EsiAtJoin is not uint actualLoopEsi)
        {
            return new(
                capture,
                seedPlan,
                FrameHandoffCapacitySeedLoopOutcome.MissingReceiverPlus08Capture,
                false,
                null,
                null,
                null,
                null,
                null,
                seedPlan.StopAtRawFileOffsetExclusive);
        }

        bool? expectedLoopEsiMatched = capture.ExpectedLoopEsi is uint expectedEsi
            ? actualLoopEsi == expectedEsi
            : null;
        bool? expectedLoopEdxMatched = capture.ExpectedLoopEdx is uint expectedEdx
            ? actualLoopEdx == expectedEdx
            : null;

        if (expectedLoopEsiMatched == false || expectedLoopEdxMatched == false)
        {
            return new(
                capture,
                seedPlan,
                FrameHandoffCapacitySeedLoopOutcome.LoopIdentityCaptureMismatch,
                true,
                actualLoopEsi,
                actualLoopEdx,
                expectedLoopEsiMatched,
                expectedLoopEdxMatched,
                null,
                FrameHandoffCapacityLoopProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityLoopPlan loopPlan =
            FrameHandoffCapacityLoopProjection.Project(
                new(actualLoopEsi, actualLoopEdx));

        return new(
            capture,
            seedPlan,
            FrameHandoffCapacitySeedLoopOutcome.OneLoopIterationProjected,
            true,
            actualLoopEsi,
            actualLoopEdx,
            expectedLoopEsiMatched,
            expectedLoopEdxMatched,
            loopPlan,
            loopPlan.StopAtRawFileOffsetExclusive);
    }
}
