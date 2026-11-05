namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveZeroForwardReturnCapture(
    UnreliableReceiveZeroResultCapture ZeroResult,
    UnreliableReceiveZeroReturnCapture ZeroReturn);

public enum UnreliableReceiveZeroForwardReturnOutcome
{
    ZeroResultDidNotReachExclusiveBoundary,
    MissingPriorForwardCallReturnCapture,
    PriorForwardCallDidNotReturnNormally,
    ZeroReturnTailReached
}

public readonly record struct UnreliableReceiveZeroForwardReturnPlan(
    UnreliableReceiveZeroForwardReturnCapture Capture,
    UnreliableReceiveZeroResultPlan ZeroResultPlan,
    UnreliableReceiveZeroReturnPlan? ZeroReturnPlan,
    UnreliableReceiveZeroForwardReturnOutcome Outcome,
    bool ZeroReturnTailAdmitted,
    bool UnreachedTailCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the zero-result handoff and its normal-return tail through raw 0xBE5BAC.
/// </summary>
public static class UnreliableReceiveZeroForwardReturnProjection
{
    public const string SamplePath = UnreliableReceiveZeroResultProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveZeroResultProjection.SampleSha256;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        UnreliableReceiveZeroResultProjection.CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint StartRawFileOffset =
        UnreliableReceiveZeroResultProjection.HandoffStartRawFileOffset;
    public const uint StartVirtualAddress =
        UnreliableReceiveZeroResultProjection.HandoffStartVirtualAddress;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveZeroReturnProjection.TailEndRawFileOffsetExclusive;
    public const uint EndVirtualAddressExclusive =
        UnreliableReceiveZeroReturnProjection.TailEndVirtualAddressExclusive;

    public const string CapturedBytes =
        UnreliableReceiveZeroResultProjection.CapturedHandoffBytes
        + " "
        + UnreliableReceiveZeroReturnProjection.CapturedTailBytes;

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        UnreliableReceiveZeroResultProjection.RawFileOffsetToVirtualAddress(rawFileOffset);

    public static UnreliableReceiveZeroForwardReturnPlan Project(
        UnreliableReceiveZeroForwardReturnCapture capture)
    {
        UnreliableReceiveZeroResultPlan zeroResultPlan =
            UnreliableReceiveZeroResultProjection.Project(capture.ZeroResult);

        bool tailAdmitted =
            zeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.HandoffReturnedToExclusiveBoundary
            && zeroResultPlan.HandoffReturnBoundaryReached == true;

        if (!tailAdmitted)
        {
            bool unreachedTailCaptureProvided =
                capture.ZeroReturn.PriorForwardCallReturnedNormally.HasValue
                || capture.ZeroReturn.HelperReturnEax.HasValue;

            return new(
                capture,
                zeroResultPlan,
                ZeroReturnPlan: null,
                Outcome:
                    UnreliableReceiveZeroForwardReturnOutcome.ZeroResultDidNotReachExclusiveBoundary,
                ZeroReturnTailAdmitted: false,
                UnreachedTailCaptureProvided: unreachedTailCaptureProvided,
                StopAtRawFileOffsetExclusive: zeroResultPlan.StopAtRawFileOffsetExclusive);
        }

        UnreliableReceiveZeroReturnPlan zeroReturnPlan =
            UnreliableReceiveZeroReturnProjection.Project(capture.ZeroReturn);

        UnreliableReceiveZeroForwardReturnOutcome outcome =
            zeroReturnPlan.Outcome switch
            {
                UnreliableReceiveZeroReturnOutcome.MissingPriorForwardCallReturnCapture =>
                    UnreliableReceiveZeroForwardReturnOutcome.MissingPriorForwardCallReturnCapture,
                UnreliableReceiveZeroReturnOutcome.PriorForwardCallDidNotReturnNormally =>
                    UnreliableReceiveZeroForwardReturnOutcome.PriorForwardCallDidNotReturnNormally,
                UnreliableReceiveZeroReturnOutcome.ZeroReturnTailReached =>
                    UnreliableReceiveZeroForwardReturnOutcome.ZeroReturnTailReached,
                _ => throw new System.Diagnostics.UnreachableException()
            };

        return new(
            capture,
            zeroResultPlan,
            zeroReturnPlan,
            Outcome: outcome,
            ZeroReturnTailAdmitted: true,
            UnreachedTailCaptureProvided: false,
            StopAtRawFileOffsetExclusive: zeroReturnPlan.StopAtRawFileOffsetExclusive);
    }
}
