using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeShiftedIndexedLifecycleCapture(
    CapacityFinalCalleeAlternateShiftedFrameGateCapture ShiftedFrameGateCapture,
    uint? FreshIndexedFrameMinus4,
    uint? FreshIndexedFrameMinus10);

public enum CapacityFinalCalleeShiftedIndexedLifecycleOutcome
{
    ShiftedFrameGateBlocked,
    ShiftedUnequalTo9137,
    ShiftedUnequalTo9137Contaminated,
    IndexedFrameGateBlocked,
    IndexedEqualTo9160,
    IndexedUnequalTo9151
}

public sealed record CapacityFinalCalleeShiftedIndexedLifecyclePlan(
    CapacityFinalCalleeShiftedIndexedLifecycleCapture Capture,
    CapacityFinalCalleeAlternateShiftedFrameGatePlan ShiftedFrameGatePlan,
    CapacityFinalCalleeAlternateIndexedFrameGatePlan? IndexedFrameGatePlan,
    CapacityFinalCalleeShiftedIndexedLifecycleOutcome Outcome)
{
    public bool IndexedFrameGateReached { get; init; }
    public uint? RetainedUpstreamEBP { get; init; }
    public bool UnreachedFreshIndexedFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedFreshIndexedFrameMinus10CaptureProvided { get; init; }
    public bool LateFreshIndexedFrameMinus4CaptureProvided { get; init; }
    public bool LateFreshIndexedFrameMinus10CaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite>
        OrderedIndexedWrites =>
        IndexedFrameGatePlan?.Writes
        ?? ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public bool OpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins only the clean equal 0x913D edge of the shifted-frame gate to the
/// indexed-frame gate. The latter receives its own independent fresh frame
/// captures; the shifted gate's EBP-4 value is never reused.
/// </summary>
public static class CapacityFinalCalleeShiftedIndexedLifecycleProjection
{
    public const uint ShiftedFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset;
    public const uint ShiftedUnequalOpaqueCallBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint IndexedFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryRawFileOffset;
    public const uint IndexedUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint IndexedEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EqualDestinationRawFileOffset;

    public static CapacityFinalCalleeShiftedIndexedLifecyclePlan Project(
        CapacityFinalCalleeShiftedIndexedLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateShiftedFrameGatePlan shifted =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(
                capture.ShiftedFrameGateCapture);
        bool indexedFrame4Provided = capture.FreshIndexedFrameMinus4.HasValue;
        bool indexedFrame10Provided = capture.FreshIndexedFrameMinus10.HasValue;

        if (!IsCleanShiftedGateExit(shifted))
        {
            return new(
                capture,
                shifted,
                IndexedFrameGatePlan: null,
                CapacityFinalCalleeShiftedIndexedLifecycleOutcome.ShiftedFrameGateBlocked)
            {
                UnreachedFreshIndexedFrameMinus4CaptureProvided = indexedFrame4Provided,
                UnreachedFreshIndexedFrameMinus10CaptureProvided = indexedFrame10Provided,
                LateFreshIndexedFrameMinus4CaptureProvided = indexedFrame4Provided,
                LateFreshIndexedFrameMinus10CaptureProvided = indexedFrame10Provided,
                StopAtRawFileOffsetExclusive = shifted.StopAtRawFileOffsetExclusive
            };
        }

        if (shifted.JzSelectedDestinationRawFileOffset
            == ShiftedUnequalOpaqueCallBoundaryRawFileOffset)
        {
            bool contaminated = indexedFrame4Provided || indexedFrame10Provided;
            return new(
                capture,
                shifted,
                IndexedFrameGatePlan: null,
                contaminated
                    ? CapacityFinalCalleeShiftedIndexedLifecycleOutcome
                        .ShiftedUnequalTo9137Contaminated
                    : CapacityFinalCalleeShiftedIndexedLifecycleOutcome
                        .ShiftedUnequalTo9137)
            {
                UnreachedFreshIndexedFrameMinus4CaptureProvided = indexedFrame4Provided,
                UnreachedFreshIndexedFrameMinus10CaptureProvided = indexedFrame10Provided,
                LateFreshIndexedFrameMinus4CaptureProvided = indexedFrame4Provided,
                LateFreshIndexedFrameMinus10CaptureProvided = indexedFrame10Provided,
                ExactCleanExitAdmitted = !contaminated,
                StopAtRawFileOffsetExclusive =
                    ShiftedUnequalOpaqueCallBoundaryRawFileOffset
            };
        }

        uint retainedEbp = capture.ShiftedFrameGateCapture.EntryEBP!.Value;
        CapacityFinalCalleeAlternateIndexedFrameGatePlan indexed =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(
                new CapacityFinalCalleeAlternateIndexedFrameGateCapture(
                    EntryEBP: retainedEbp,
                    FreshFrameMinus4: capture.FreshIndexedFrameMinus4,
                    FreshFrameMinus10: capture.FreshIndexedFrameMinus10));

        bool frame4ReadReached = indexed.FrameMinus4ReadReached;
        return new(
            capture,
            shifted,
            indexed,
            MapIndexedOutcome(indexed.Outcome))
        {
            IndexedFrameGateReached = frame4ReadReached,
            RetainedUpstreamEBP = frame4ReadReached ? retainedEbp : null,
            UnreachedFreshIndexedFrameMinus4CaptureProvided =
                indexed.UnreachedFreshFrameMinus4CaptureProvided,
            UnreachedFreshIndexedFrameMinus10CaptureProvided =
                indexed.UnreachedFreshFrameMinus10CaptureProvided,
            LateFreshIndexedFrameMinus4CaptureProvided =
                indexed.LateFreshFrameMinus4CaptureProvided,
            LateFreshIndexedFrameMinus10CaptureProvided =
                indexed.LateFreshFrameMinus10CaptureProvided,
            ExactCleanExitAdmitted = IsCleanIndexedGateExit(indexed),
            StopAtRawFileOffsetExclusive = indexed.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsCleanShiftedGateExit(
        CapacityFinalCalleeAlternateShiftedFrameGatePlan plan)
    {
        if (!plan.ExactCleanExitAdmitted
            || !plan.JzReached
            || plan.JzSelectedDestinationRawFileOffset
                != (plan.JzTaken == true
                    ? IndexedFrameGateEntryRawFileOffset
                    : ShiftedUnequalOpaqueCallBoundaryRawFileOffset)
            || plan.StopAtRawFileOffsetExclusive
                != plan.JzSelectedDestinationRawFileOffset
            || plan.UnreachedFreshFrameMinus4CaptureProvided
            || plan.UnreachedEntryECXCaptureProvided
            || plan.LateFreshFrameMinus4CaptureProvided
            || plan.LateEntryECXCaptureProvided
            || plan.OpaqueCallReached)
        {
            return false;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome.EqualToBranchTarget =>
                plan.JzTaken == true
                && plan.JzSelectedDestinationRawFileOffset
                    == IndexedFrameGateEntryRawFileOffset,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .UnequalToOpaqueCallBoundary =>
                plan.JzTaken == false
                && plan.JzSelectedDestinationRawFileOffset
                    == ShiftedUnequalOpaqueCallBoundaryRawFileOffset,
            _ => false
        };
    }

    private static bool IsCleanIndexedGateExit(
        CapacityFinalCalleeAlternateIndexedFrameGatePlan plan)
    {
        if (!plan.ExactCleanRouteAdmitted
            || !plan.JeReached
            || plan.JeSelectedDestinationRawFileOffset
                != (plan.JeTaken == true
                    ? IndexedEqualDestinationRawFileOffset
                    : IndexedUnequalDestinationRawFileOffset)
            || plan.StopAtRawFileOffsetExclusive
                != plan.JeSelectedDestinationRawFileOffset
            || plan.UnreachedFreshFrameMinus4CaptureProvided
            || plan.UnreachedFreshFrameMinus10CaptureProvided
            || plan.LateFreshFrameMinus4CaptureProvided
            || plan.LateFreshFrameMinus10CaptureProvided)
        {
            return false;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget =>
                plan.JeTaken == true
                && plan.JeSelectedDestinationRawFileOffset
                    == IndexedEqualDestinationRawFileOffset,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough =>
                plan.JeTaken == false
                && plan.JeSelectedDestinationRawFileOffset
                    == IndexedUnequalDestinationRawFileOffset,
            _ => false
        };
    }

    private static CapacityFinalCalleeShiftedIndexedLifecycleOutcome MapIndexedOutcome(
        CapacityFinalCalleeAlternateIndexedFrameGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingEntryEBP
                or CapacityFinalCalleeAlternateIndexedFrameGateOutcome
                    .MissingFreshFrameMinus4
                or CapacityFinalCalleeAlternateIndexedFrameGateOutcome
                    .MissingFreshFrameMinus10 =>
                CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedFrameGateBlocked,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget =>
                CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedEqualTo9160,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough =>
                CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedUnequalTo9151,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
