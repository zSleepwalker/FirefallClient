using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedFinalLifecycleCapture(
    CapacityFinalCalleeAlternateIndexedFrameGateCapture IndexedFrameGateCapture,
    uint? FreshFrameMinus14);

public enum CapacityFinalCalleeIndexedFinalLifecycleOutcome
{
    UpstreamMissingEntryEBP,
    UpstreamMissingFreshFrameMinus4,
    UpstreamMissingFreshFrameMinus10,
    UpstreamUnequalTo9151,
    UpstreamUnequalTo9151Contaminated,
    MissingFreshFrameMinus14,
    ChildEqualToFallthrough,
    ChildUnequalToBranchTarget,
    UpstreamIndexedGateBlocked,
    ChildFinalFrameGateBlocked
}

public enum CapacityFinalCalleeIndexedFinalLifecycleChildECXSource
{
    None,
    UpstreamFreshFrameMinus10
}

public readonly record struct CapacityFinalCalleeIndexedFinalLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeIndexedFinalLifecyclePlan(
    CapacityFinalCalleeIndexedFinalLifecycleCapture Capture,
    CapacityFinalCalleeAlternateIndexedFrameGatePlan IndexedFrameGatePlan,
    CapacityFinalCalleeAlternateFinalFrameGatePlan? FinalFrameGatePlan,
    CapacityFinalCalleeIndexedFinalLifecycleOutcome Outcome)
{
    public bool FinalFrameGateReached { get; init; }
    public uint? ChildEntryEBP { get; init; }
    public uint? ChildEntryECX { get; init; }
    public CapacityFinalCalleeIndexedFinalLifecycleChildECXSource ChildEntryECXSource
        { get; init; }
    public bool UnreachedFreshFrameMinus14CaptureProvided { get; init; }
    public bool LateFreshFrameMinus14CaptureProvided { get; init; }
    public bool UnreachedChildEntryECXCaptureProvided { get; init; }
    public bool LateChildEntryECXCaptureProvided { get; init; }
    public bool UpstreamCleanRouteAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleeIndexedFinalLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeIndexedFinalLifecycleWrite>.Empty;
}

/// <summary>
/// Joins the clean equal indexed-frame route at 9160 to the final-frame gate.
/// The indexed gate's EBP and fresh EBP-10 value become the child's EBP and
/// CMP operand only on that route; the child's EBP-14 read remains independent.
/// </summary>
public static class CapacityFinalCalleeIndexedFinalLifecycleProjection
{
    public const uint FinalFrameEntryRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset;
    public const uint UpstreamEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint UpstreamUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint FinalEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint FinalUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset;

    public static CapacityFinalCalleeIndexedFinalLifecyclePlan Project(
        CapacityFinalCalleeIndexedFinalLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateIndexedFrameGatePlan indexed =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(
                capture.IndexedFrameGateCapture);

        ImmutableArray<CapacityFinalCalleeIndexedFinalLifecycleWrite> RetainedWrites()
        {
            var writes =
                ImmutableArray.CreateBuilder<CapacityFinalCalleeIndexedFinalLifecycleWrite>();
            foreach (CapacityFinalCalleeAlternateIndexedFrameGateWrite write in indexed.Writes)
            {
                writes.Add(new(write.RawFileOffset, write.Address, write.Value));
            }

            return writes.ToImmutable();
        }

        CapacityFinalCalleeIndexedFinalLifecyclePlan FinishWithoutChild(
            CapacityFinalCalleeIndexedFinalLifecycleOutcome outcome,
            bool unreachedFreshFrame14,
            bool cleanExitAdmitted) =>
            new(capture, indexed, null, outcome)
            {
                UnreachedFreshFrameMinus14CaptureProvided = unreachedFreshFrame14,
                LateFreshFrameMinus14CaptureProvided = unreachedFreshFrame14,
                UpstreamCleanRouteAdmitted = indexed.ExactCleanRouteAdmitted,
                ExactCleanExitAdmitted = cleanExitAdmitted,
                StopAtRawFileOffsetExclusive = indexed.StopAtRawFileOffsetExclusive,
                Writes = RetainedWrites()
            };

        switch (indexed.Outcome)
        {
            case CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingEntryEBP:
                return FinishWithoutChild(
                    CapacityFinalCalleeIndexedFinalLifecycleOutcome.UpstreamMissingEntryEBP,
                    capture.FreshFrameMinus14.HasValue,
                    cleanExitAdmitted: false);

            case CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus4:
                return FinishWithoutChild(
                    CapacityFinalCalleeIndexedFinalLifecycleOutcome
                        .UpstreamMissingFreshFrameMinus4,
                    capture.FreshFrameMinus14.HasValue,
                    cleanExitAdmitted: false);

            case CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus10:
                return FinishWithoutChild(
                    CapacityFinalCalleeIndexedFinalLifecycleOutcome
                        .UpstreamMissingFreshFrameMinus10,
                    capture.FreshFrameMinus14.HasValue,
                    cleanExitAdmitted: false);

            case CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough:
            {
                bool cleanUpstreamRoute =
                    indexed.ExactCleanRouteAdmitted
                    && indexed.JeReached
                    && indexed.JeTaken == false
                    && indexed.JeSelectedDestinationRawFileOffset
                        == UpstreamUnequalDestinationRawFileOffset
                    && indexed.StopAtRawFileOffsetExclusive
                        == UpstreamUnequalDestinationRawFileOffset;
                bool contaminated = capture.FreshFrameMinus14.HasValue;
                return FinishWithoutChild(
                    contaminated
                        ? CapacityFinalCalleeIndexedFinalLifecycleOutcome
                            .UpstreamUnequalTo9151Contaminated
                        : CapacityFinalCalleeIndexedFinalLifecycleOutcome
                            .UpstreamUnequalTo9151,
                    unreachedFreshFrame14: contaminated,
                    cleanExitAdmitted: cleanUpstreamRoute && !contaminated);
            }

            case CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget:
                break;

            default:
                return FinishWithoutChild(
                    CapacityFinalCalleeIndexedFinalLifecycleOutcome
                        .UpstreamIndexedGateBlocked,
                    capture.FreshFrameMinus14.HasValue,
                    cleanExitAdmitted: false);
        }

        if (!IsCleanEqualIndexedRoute(indexed)
            || indexed.Capture.EntryEBP is not uint childEbp
            || indexed.FrameMinus10 is not uint childEcx)
        {
            return FinishWithoutChild(
                CapacityFinalCalleeIndexedFinalLifecycleOutcome
                    .UpstreamIndexedGateBlocked,
                capture.FreshFrameMinus14.HasValue,
                cleanExitAdmitted: false);
        }

        CapacityFinalCalleeAlternateFinalFrameGatePlan final =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(
                new(
                    EntryEBP: childEbp,
                    FreshFrameMinus14: capture.FreshFrameMinus14,
                    EntryECX: capture.FreshFrameMinus14.HasValue ? childEcx : null));

        CapacityFinalCalleeIndexedFinalLifecycleChildECXSource ecxSource =
            final.CompareReached
                ? CapacityFinalCalleeIndexedFinalLifecycleChildECXSource
                    .UpstreamFreshFrameMinus10
                : CapacityFinalCalleeIndexedFinalLifecycleChildECXSource.None;

        CapacityFinalCalleeIndexedFinalLifecycleOutcome outcome = final.Outcome switch
        {
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeIndexedFinalLifecycleOutcome.MissingFreshFrameMinus14,
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough =>
                CapacityFinalCalleeIndexedFinalLifecycleOutcome.ChildEqualToFallthrough,
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget =>
                CapacityFinalCalleeIndexedFinalLifecycleOutcome.ChildUnequalToBranchTarget,
            _ => CapacityFinalCalleeIndexedFinalLifecycleOutcome.ChildFinalFrameGateBlocked
        };

        return new(capture, indexed, final, outcome)
        {
            FinalFrameGateReached = true,
            ChildEntryEBP = childEbp,
            ChildEntryECX = final.CompareReached ? childEcx : null,
            ChildEntryECXSource = ecxSource,
            UnreachedFreshFrameMinus14CaptureProvided =
                final.UnreachedFreshFrameMinus14CaptureProvided,
            LateFreshFrameMinus14CaptureProvided =
                final.LateFreshFrameMinus14CaptureProvided,
            UnreachedChildEntryECXCaptureProvided =
                final.UnreachedEntryECXCaptureProvided,
            LateChildEntryECXCaptureProvided =
                final.LateEntryECXCaptureProvided,
            UpstreamCleanRouteAdmitted = true,
            ExactCleanExitAdmitted =
                final.ExactCleanExitAdmitted
                && (final.Outcome is
                    CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough
                    or CapacityFinalCalleeAlternateFinalFrameGateOutcome
                        .UnequalToBranchTarget),
            StopAtRawFileOffsetExclusive = final.StopAtRawFileOffsetExclusive,
            Writes = RetainedWrites()
        };
    }

    private static bool IsCleanEqualIndexedRoute(
        CapacityFinalCalleeAlternateIndexedFrameGatePlan indexed) =>
        indexed.ExactCleanRouteAdmitted
        && indexed.Outcome
            == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget
        && indexed.JeReached
        && indexed.JeTaken == true
        && indexed.JeSelectedDestinationRawFileOffset
            == UpstreamEqualDestinationRawFileOffset
        && indexed.StopAtRawFileOffsetExclusive == FinalFrameEntryRawFileOffset
        && indexed.FrameMinus10Captured
        && indexed.Writes.Length == 1
        && !indexed.UnreachedFreshFrameMinus4CaptureProvided
        && !indexed.UnreachedFreshFrameMinus10CaptureProvided
        && !indexed.LateFreshFrameMinus4CaptureProvided
        && !indexed.LateFreshFrameMinus10CaptureProvided;
}
