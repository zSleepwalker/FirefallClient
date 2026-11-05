namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSecondShiftedLifecycleCapture(
    CapacityFinalCalleeAlternateSecondFrameGateCapture SecondFrameGateCapture,
    uint? FreshShiftedFrameMinus4,
    uint? ShiftedEntryECX);

public enum CapacityFinalCalleeSecondShiftedLifecycleOutcome
{
    SecondFrameGateBlocked,
    EqualTo9127OpaqueCallBoundary,
    EqualTo9127OpaqueCallBoundaryContaminated,
    ShiftedFrameGateBlocked,
    ShiftedValueEqualTo913D,
    ShiftedValueUnequalTo9137
}

public sealed record CapacityFinalCalleeSecondShiftedLifecyclePlan(
    CapacityFinalCalleeSecondShiftedLifecycleCapture Capture,
    CapacityFinalCalleeAlternateSecondFrameGatePlan SecondFrameGatePlan,
    CapacityFinalCalleeAlternateShiftedFrameGatePlan? ShiftedFrameGatePlan,
    CapacityFinalCalleeSecondShiftedLifecycleOutcome Outcome)
{
    public bool ShiftedFrameGateReached { get; init; }
    public uint? DerivedShiftedEntryEBP { get; init; }
    public bool UnreachedFreshShiftedFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedShiftedEntryECXCaptureProvided { get; init; }
    public bool LateFreshShiftedFrameMinus4CaptureProvided { get; init; }
    public bool LateShiftedEntryECXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool OpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean unequal 0x912D edge of the second frame gate to the
/// shifted-frame gate. The equal 0x9127 edge remains an opaque call boundary.
/// </summary>
public static class CapacityFinalCalleeSecondShiftedLifecycleProjection
{
    public const uint SecondFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryRawFileOffset;
    public const uint ShiftedFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset;
    public const uint EqualOpaqueCallBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateSecondFrameGateProjection.OpaqueCallRawFileOffset;
    public const uint ShiftedUnequalOpaqueCallBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint ShiftedEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.EqualDestinationRawFileOffset;

    public static CapacityFinalCalleeSecondShiftedLifecyclePlan Project(
        CapacityFinalCalleeSecondShiftedLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateSecondFrameGatePlan secondGate =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(
                capture.SecondFrameGateCapture);

        bool cleanUnequalJoin =
            secondGate.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.UnequalToBranchTarget
            && IsCleanUnequalSecondGateExit(secondGate);

        if (!cleanUnequalJoin)
        {
            bool shiftedFrameProvided = capture.FreshShiftedFrameMinus4.HasValue;
            bool shiftedECXProvided = capture.ShiftedEntryECX.HasValue;
            bool isEqualBoundary = IsEqualOpaqueCallBoundary(secondGate);
            bool contaminated = isEqualBoundary
                && (shiftedFrameProvided || shiftedECXProvided);

            return new(
                capture,
                secondGate,
                ShiftedFrameGatePlan: null,
                isEqualBoundary
                    ? contaminated
                        ? CapacityFinalCalleeSecondShiftedLifecycleOutcome
                            .EqualTo9127OpaqueCallBoundaryContaminated
                        : CapacityFinalCalleeSecondShiftedLifecycleOutcome
                            .EqualTo9127OpaqueCallBoundary
                    : CapacityFinalCalleeSecondShiftedLifecycleOutcome
                        .SecondFrameGateBlocked)
            {
                UnreachedFreshShiftedFrameMinus4CaptureProvided = shiftedFrameProvided,
                UnreachedShiftedEntryECXCaptureProvided = shiftedECXProvided,
                LateFreshShiftedFrameMinus4CaptureProvided = shiftedFrameProvided,
                LateShiftedEntryECXCaptureProvided = shiftedECXProvided,
                ExactCleanExitAdmitted = isEqualBoundary && !contaminated,
                StopAtRawFileOffsetExclusive =
                    secondGate.StopAtRawFileOffsetExclusive
            };
        }

        uint entryEbp = capture.SecondFrameGateCapture.EntryEBP!.Value;
        CapacityFinalCalleeAlternateShiftedFrameGatePlan shiftedGate =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(
                new CapacityFinalCalleeAlternateShiftedFrameGateCapture(
                    EntryEBP: entryEbp,
                    FreshFrameMinus4: capture.FreshShiftedFrameMinus4,
                    EntryECX: capture.ShiftedEntryECX));

        bool frameConsumerReached = shiftedGate.FrameMinus4ReadReached;
        return new(
            capture,
            secondGate,
            shiftedGate,
            MapShiftedOutcome(shiftedGate.Outcome))
        {
            ShiftedFrameGateReached = frameConsumerReached,
            DerivedShiftedEntryEBP = frameConsumerReached ? entryEbp : null,
            UnreachedFreshShiftedFrameMinus4CaptureProvided =
                shiftedGate.UnreachedFreshFrameMinus4CaptureProvided,
            UnreachedShiftedEntryECXCaptureProvided =
                shiftedGate.UnreachedEntryECXCaptureProvided,
            LateFreshShiftedFrameMinus4CaptureProvided =
                shiftedGate.LateFreshFrameMinus4CaptureProvided,
            LateShiftedEntryECXCaptureProvided =
                shiftedGate.LateEntryECXCaptureProvided,
            ExactCleanExitAdmitted = IsCleanShiftedGateExit(shiftedGate),
            StopAtRawFileOffsetExclusive =
                shiftedGate.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsCleanUnequalSecondGateExit(
        CapacityFinalCalleeAlternateSecondFrameGatePlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.CompareReached
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == ShiftedFrameGateEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ShiftedFrameGateEntryRawFileOffset
        && !plan.UnreachedEntryEBPCaptureProvided
        && !plan.LateFreshFrameMinus10CaptureProvided
        && !plan.OpaqueCallReached;

    private static bool IsEqualOpaqueCallBoundary(
        CapacityFinalCalleeAlternateSecondFrameGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateSecondFrameGateOutcome
                .EqualToOpaqueCallBoundary
        && plan.ExactCleanExitAdmitted
        && plan.CompareReached
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == EqualOpaqueCallBoundaryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == EqualOpaqueCallBoundaryRawFileOffset
        && !plan.UnreachedEntryEBPCaptureProvided
        && !plan.LateFreshFrameMinus10CaptureProvided
        && !plan.OpaqueCallReached;

    private static bool IsCleanShiftedGateExit(
        CapacityFinalCalleeAlternateShiftedFrameGatePlan plan)
    {
        if (!plan.ExactCleanExitAdmitted
            || !plan.JzReached
            || plan.JzSelectedDestinationRawFileOffset
                != (plan.JzTaken == true
                    ? ShiftedEqualDestinationRawFileOffset
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
                    == ShiftedEqualDestinationRawFileOffset,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .UnequalToOpaqueCallBoundary =>
                plan.JzTaken == false
                && plan.JzSelectedDestinationRawFileOffset
                    == ShiftedUnequalOpaqueCallBoundaryRawFileOffset,
            _ => false
        };
    }

    private static CapacityFinalCalleeSecondShiftedLifecycleOutcome MapShiftedOutcome(
        CapacityFinalCalleeAlternateShiftedFrameGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryEBP
                or CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .MissingFreshFrameMinus4
                or CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedFrameGateBlocked,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome.EqualToBranchTarget =>
                CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedValueEqualTo913D,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                .UnequalToOpaqueCallBoundary =>
                CapacityFinalCalleeSecondShiftedLifecycleOutcome
                    .ShiftedValueUnequalTo9137,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
