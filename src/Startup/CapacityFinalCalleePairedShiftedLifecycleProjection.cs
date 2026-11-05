using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePairedShiftedLifecycleCapture(
    CapacityFinalCalleePairedFrameGateLifecycleCapture PairedFrameGateCapture,
    uint? FreshFrameMinus4,
    uint? ShiftedEntryECX);

public enum CapacityFinalCalleePairedShiftedLifecycleOutcome
{
    UpstreamGateBlocked,
    EqualTo911COpaqueCallBoundary,
    EqualTo911COpaqueCallBoundaryContaminated,
    EqualTo9127OpaqueCallBoundary,
    EqualTo9127OpaqueCallBoundaryContaminated,
    ShiftedFrameGateBlocked,
    ShiftedValueEqualTo913D,
    ShiftedValueUnequalTo9137
}

public sealed record CapacityFinalCalleePairedShiftedLifecyclePlan(
    CapacityFinalCalleePairedShiftedLifecycleCapture Capture,
    CapacityFinalCalleePairedFrameGateLifecyclePlan PairedFrameGatePlan,
    CapacityFinalCalleeAlternateShiftedFrameGatePlan? ShiftedFrameGatePlan,
    CapacityFinalCalleePairedShiftedLifecycleOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsWrite> OrderedUpstreamWrites =>
        PairedFrameGatePlan.OrderedUpstreamWrites;

    public bool ShiftedFrameGateReached { get; init; }
    public uint? DerivedShiftedEntryEBP { get; init; }
    public bool UnreachedFreshFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedShiftedEntryECXCaptureProvided { get; init; }
    public bool LateFreshFrameMinus4CaptureProvided { get; init; }
    public bool LateShiftedEntryECXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool FirstOpaqueCallReached => false;
    public bool SecondOpaqueCallReached => false;
    public bool ShiftedOpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean unequal 0x912D edge of the paired frame-gate lifecycle to
/// the existing shifted-frame gate. The fresh EBP-4 value is independently
/// captured, and all opaque-call boundaries remain bounded exits.
/// </summary>
public static class CapacityFinalCalleePairedShiftedLifecycleProjection
{
    public const uint FirstOpaqueCallBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleePairedFrameGateLifecycleProjection
            .FirstOpaqueCallBoundaryRawFileOffsetExclusive;
    public const uint EqualSecondGateBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleePairedFrameGateLifecycleProjection
            .EqualSecondGateBoundaryRawFileOffsetExclusive;
    public const uint ShiftedFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset;
    public const uint ShiftedUnequalOpaqueCallBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection
            .UnequalDestinationRawFileOffset;
    public const uint ShiftedEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateShiftedFrameGateProjection
            .EqualDestinationRawFileOffset;

    public static CapacityFinalCalleePairedShiftedLifecyclePlan Project(
        CapacityFinalCalleePairedShiftedLifecycleCapture capture)
    {
        CapacityFinalCalleePairedFrameGateLifecyclePlan paired =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                capture.PairedFrameGateCapture);
        bool freshFrameProvided = capture.FreshFrameMinus4.HasValue;
        bool ecxProvided = capture.ShiftedEntryECX.HasValue;

        CapacityFinalCalleePairedShiftedLifecyclePlan Finish(
            CapacityFinalCalleePairedShiftedLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateShiftedFrameGatePlan? shifted = null,
            bool shiftedReached = false,
            uint? derivedEbp = null,
            bool unreachedFreshFrame = false,
            bool unreachedEcx = false,
            bool lateFreshFrame = false,
            bool lateEcx = false,
            bool exactCleanExitAdmitted = false) =>
            new(capture, paired, shifted, outcome)
            {
                ShiftedFrameGateReached = shiftedReached,
                DerivedShiftedEntryEBP = derivedEbp,
                UnreachedFreshFrameMinus4CaptureProvided = unreachedFreshFrame,
                UnreachedShiftedEntryECXCaptureProvided = unreachedEcx,
                LateFreshFrameMinus4CaptureProvided = lateFreshFrame,
                LateShiftedEntryECXCaptureProvided = lateEcx,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (IsFirstOpaqueBoundary(paired))
        {
            bool contaminated =
                paired.Outcome
                    == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .AlternateFrameLoadsEqualWithDownstreamCapture
                || freshFrameProvided
                || ecxProvided;
            return Finish(
                contaminated
                    ? CapacityFinalCalleePairedShiftedLifecycleOutcome
                        .EqualTo911COpaqueCallBoundaryContaminated
                    : CapacityFinalCalleePairedShiftedLifecycleOutcome
                        .EqualTo911COpaqueCallBoundary,
                FirstOpaqueCallBoundaryRawFileOffsetExclusive,
                unreachedFreshFrame: freshFrameProvided,
                unreachedEcx: ecxProvided,
                lateFreshFrame: freshFrameProvided,
                lateEcx: ecxProvided,
                exactCleanExitAdmitted: !contaminated
                    && paired.ExactCleanExitAdmitted);
        }

        if (IsSecondOpaqueBoundary(paired))
        {
            bool contaminated = freshFrameProvided || ecxProvided;
            return Finish(
                contaminated
                    ? CapacityFinalCalleePairedShiftedLifecycleOutcome
                        .EqualTo9127OpaqueCallBoundaryContaminated
                    : CapacityFinalCalleePairedShiftedLifecycleOutcome
                        .EqualTo9127OpaqueCallBoundary,
                EqualSecondGateBoundaryRawFileOffsetExclusive,
                unreachedFreshFrame: freshFrameProvided,
                unreachedEcx: ecxProvided,
                lateFreshFrame: freshFrameProvided,
                lateEcx: ecxProvided,
                exactCleanExitAdmitted: !contaminated
                    && paired.ExactCleanExitAdmitted);
        }

        if (!IsCleanUnequalSecondGateHandoff(paired)
            || paired.DerivedSecondGateEntryEBP is not uint retainedEbp)
        {
            return Finish(
                CapacityFinalCalleePairedShiftedLifecycleOutcome.UpstreamGateBlocked,
                paired.StopAtRawFileOffsetExclusive,
                unreachedFreshFrame: freshFrameProvided,
                unreachedEcx: ecxProvided,
                lateFreshFrame: freshFrameProvided,
                lateEcx: ecxProvided);
        }

        CapacityFinalCalleeAlternateShiftedFrameGatePlan shifted =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(
                new(
                    EntryEBP: retainedEbp,
                    FreshFrameMinus4: capture.FreshFrameMinus4,
                    EntryECX: capture.ShiftedEntryECX));

        bool frameReadReached = shifted.FrameMinus4ReadReached;
        return Finish(
            MapShiftedOutcome(shifted.Outcome),
            shifted.StopAtRawFileOffsetExclusive,
            shifted,
            shiftedReached: frameReadReached,
            derivedEbp: frameReadReached ? retainedEbp : null,
            unreachedFreshFrame:
                shifted.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedEcx:
                shifted.UnreachedEntryECXCaptureProvided,
            lateFreshFrame:
                shifted.LateFreshFrameMinus4CaptureProvided,
            lateEcx:
                shifted.LateEntryECXCaptureProvided,
            exactCleanExitAdmitted: IsCleanShiftedGateExit(shifted));
    }

    private static bool IsFirstOpaqueBoundary(
        CapacityFinalCalleePairedFrameGateLifecyclePlan plan) =>
        plan.AlternateFrameLoads.Outcome
            == CapacityFinalCalleeAlternateFrameLoadsOutcome
                .CompareEqualToOpaqueCallBoundary
        && !plan.AlternateSecondFrameGateReached
        && plan.AlternateSecondFrameGate is null
        && plan.StopAtRawFileOffsetExclusive
            == FirstOpaqueCallBoundaryRawFileOffsetExclusive;

    private static bool IsSecondOpaqueBoundary(
        CapacityFinalCalleePairedFrameGateLifecyclePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                .SecondGateEqualToOpaqueCallBoundary
        && plan.AlternateSecondFrameGateReached
        && plan.AlternateSecondFrameGate is
        {
            Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                .EqualToOpaqueCallBoundary,
            JnzReached: true,
            JnzTaken: false,
            JnzSelectedDestinationRawFileOffset:
                EqualSecondGateBoundaryRawFileOffsetExclusive,
            ExactCleanExitAdmitted: true,
            OpaqueCallReached: false,
            StopAtRawFileOffsetExclusive:
                EqualSecondGateBoundaryRawFileOffsetExclusive
        }
        && plan.ExactCleanExitAdmitted
        && plan.StopAtRawFileOffsetExclusive
            == EqualSecondGateBoundaryRawFileOffsetExclusive;

    private static bool IsCleanUnequalSecondGateHandoff(
        CapacityFinalCalleePairedFrameGateLifecyclePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                .SecondGateUnequalTo912D
        && plan.ExactCleanExitAdmitted
        && plan.ExactCleanHandoffToAlternateSecondFrameGate
        && plan.AlternateSecondFrameGateReached
        && plan.AlternateSecondFrameGate is
        {
            Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                .UnequalToBranchTarget,
            EntryEDXCaptured: true,
            EntryEBPCaptured: true,
            FrameMinus10ReadReached: true,
            FrameMinus10Captured: true,
            JnzReached: true,
            JnzTaken: true,
            JnzSelectedDestinationRawFileOffset:
                CapacityFinalCalleePairedFrameGateLifecycleProjection
                    .UnequalSecondGateBoundaryRawFileOffsetExclusive,
            ExactCleanExitAdmitted: true,
            OpaqueCallReached: false,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleePairedFrameGateLifecycleProjection
                    .UnequalSecondGateBoundaryRawFileOffsetExclusive
        }
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleePairedFrameGateLifecycleProjection
                .UnequalSecondGateBoundaryRawFileOffsetExclusive
        && plan.OrderedUpstreamWrites.Length == 2
        && !plan.UnreachedFreshFrameMinus10CaptureProvided;

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

    private static CapacityFinalCalleePairedShiftedLifecycleOutcome MapShiftedOutcome(
        CapacityFinalCalleeAlternateShiftedFrameGateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryEBP
                or CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .MissingFreshFrameMinus4
                or CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryECX =>
                CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedFrameGateBlocked,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome.EqualToBranchTarget =>
                CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedValueEqualTo913D,
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                .UnequalToOpaqueCallBoundary =>
                CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedValueUnequalTo9137,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
