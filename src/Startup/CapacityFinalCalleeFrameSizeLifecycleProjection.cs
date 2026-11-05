namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFrameSizeLifecycleCapture(
    CapacityFinalCalleeFrameComparisonCallCapture FrameCapture,
    uint? OpaqueCallSizeEntryESI = null,
    uint? OpaqueCallSizeEntryESP = null,
    uint? SizeEntryEAX = null,
    uint? FreshEsiPlus4 = null,
    uint? SizeCallCellTargetVirtualAddress = null,
    bool? SizeCallReturnedNormally = null);

public enum CapacityFinalCalleeFrameSizeLifecycleOutcome
{
    FrameEntryNotValidated,
    FrameDidNotReachBoundary,
    ContaminatedFrameBoundary,
    ContaminatedSizeBoundary,
    MissingSizeEntryRegisters,
    MissingFreshEsiPlus4,
    EqualSizesCallSkipped,
    MissingSizeCallCellTarget,
    MissingSizeCallReturnStatus,
    SizeCallDidNotReturnNormally,
    SizeCallReturnedNormally
}

public readonly record struct CapacityFinalCalleeFrameSizeLifecyclePlan(
    CapacityFinalCalleeFrameSizeLifecycleCapture Capture,
    CapacityFinalCalleeFrameComparisonCallPlan FramePlan,
    CapacityFinalCalleeFrameSizeLifecycleOutcome Outcome,
    bool SizeBoundaryReached,
    bool SizeEntryESICaptured,
    uint? SizeEntryESI,
    bool SizeEntryEAXCaptured,
    uint? SizeEntryEAX,
    bool SizeEntryESPCaptured,
    uint? SizeEntryESP,
    bool SizeComparisonReached,
    CapacityFinalCalleeSizeComparisonCallPlan? SizePlan,
    bool UnreachedOpaqueCallSizeEntryESICaptureProvided,
    bool UnreachedOpaqueCallSizeEntryESPCaptureProvided,
    bool UnreachedSizeEntryEAXCaptureProvided,
    bool UnreachedFreshEsiPlus4CaptureProvided,
    bool UnreachedSizeCallCellTargetCaptureProvided,
    bool UnreachedSizeCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the frame-comparison and size-comparison windows only at the exact,
/// clean 0x012C8E87 boundary. Registers after the opaque frame call are independent
/// captures; only the call-skipped route carries frame-entry ESI and ESP forward.
/// </summary>
public static class CapacityFinalCalleeFrameSizeLifecycleProjection
{
    public const uint FrameBoundaryRawFileOffset =
        CapacityFinalCalleeFrameComparisonCallProjection.BoundaryRawFileOffsetExclusive;
    public const uint SizeEntryRawFileOffset =
        CapacityFinalCalleeSizeComparisonCallProjection.EntryRawFileOffset;
    public const uint SizeBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeSizeComparisonCallProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeFrameSizeLifecyclePlan Project(
        CapacityFinalCalleeFrameSizeLifecycleCapture capture)
    {
        CapacityFinalCalleeFrameComparisonCallPlan framePlan =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(capture.FrameCapture);

        if (framePlan.Outcome
            == CapacityFinalCalleeFrameComparisonCallOutcome.EntryNotValidated)
        {
            return BuildPlan(
                capture,
                framePlan,
                CapacityFinalCalleeFrameSizeLifecycleOutcome.FrameEntryNotValidated,
                unreachedOpaqueCallSizeEntryESICaptureProvided:
                    capture.OpaqueCallSizeEntryESI.HasValue,
                unreachedOpaqueCallSizeEntryESPCaptureProvided:
                    capture.OpaqueCallSizeEntryESP.HasValue,
                stopAt: framePlan.StopAtRawFileOffsetExclusive);
        }

        bool mismatchSkip =
            framePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped;
        bool normalFrameCall =
            framePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.CallReturnedNormally;
        bool reachedExactBoundary =
            framePlan.StopAtRawFileOffsetExclusive == FrameBoundaryRawFileOffset
            && (mismatchSkip || normalFrameCall);
        if (!reachedExactBoundary)
        {
            return BuildPlan(
                capture,
                framePlan,
                CapacityFinalCalleeFrameSizeLifecycleOutcome.FrameDidNotReachBoundary,
                unreachedOpaqueCallSizeEntryESICaptureProvided:
                    capture.OpaqueCallSizeEntryESI.HasValue,
                unreachedOpaqueCallSizeEntryESPCaptureProvided:
                    capture.OpaqueCallSizeEntryESP.HasValue,
                stopAt: framePlan.StopAtRawFileOffsetExclusive);
        }

        bool contaminatedFrame =
            framePlan.UnreachedFreshFrameMinus4CaptureProvided
            || framePlan.UnreachedCallCellTargetCaptureProvided
            || framePlan.UnreachedCallReturnStatusProvided;
        if (contaminatedFrame)
        {
            return BuildPlan(
                capture,
                framePlan,
                CapacityFinalCalleeFrameSizeLifecycleOutcome.ContaminatedFrameBoundary,
                unreachedOpaqueCallSizeEntryESICaptureProvided:
                    capture.OpaqueCallSizeEntryESI.HasValue,
                unreachedOpaqueCallSizeEntryESPCaptureProvided:
                    capture.OpaqueCallSizeEntryESP.HasValue,
                stopAt: SizeEntryRawFileOffset);
        }

        uint? sizeEntryEsi = mismatchSkip
            ? capture.FrameCapture.EntryESI
            : capture.OpaqueCallSizeEntryESI;
        uint? sizeEntryEsp = mismatchSkip
            ? capture.FrameCapture.EntryESP
            : capture.OpaqueCallSizeEntryESP;

        if (sizeEntryEsi is null
            || capture.SizeEntryEAX is null
            || sizeEntryEsp is null)
        {
            return BuildPlan(
                capture,
                framePlan,
                CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeEntryRegisters,
                sizeBoundaryReached: true,
                sizeEntryESICaptured: sizeEntryEsi.HasValue,
                sizeEntryESI: sizeEntryEsi,
                sizeEntryEAXCaptured: capture.SizeEntryEAX.HasValue,
                sizeEntryEAX: capture.SizeEntryEAX,
                sizeEntryESPCaptured: sizeEntryEsp.HasValue,
                sizeEntryESP: sizeEntryEsp,
                unreachedOpaqueCallSizeEntryESICaptureProvided:
                    mismatchSkip && capture.OpaqueCallSizeEntryESI.HasValue,
                unreachedOpaqueCallSizeEntryESPCaptureProvided:
                    mismatchSkip && capture.OpaqueCallSizeEntryESP.HasValue,
                unreachedFreshEsiPlus4CaptureProvided: capture.FreshEsiPlus4.HasValue,
                unreachedSizeCallCellTargetCaptureProvided:
                    capture.SizeCallCellTargetVirtualAddress.HasValue,
                unreachedSizeCallReturnStatusProvided:
                    capture.SizeCallReturnedNormally.HasValue,
                stopAt: SizeEntryRawFileOffset);
        }

        CapacityFinalCalleeSizeComparisonCallPlan sizePlan =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(
                new CapacityFinalCalleeSizeComparisonCallCapture(
                    CaptureValidatedEntry: true,
                    EntryESI: sizeEntryEsi.Value,
                    EntryEAX: capture.SizeEntryEAX.Value,
                    EntryESP: sizeEntryEsp.Value,
                    FreshEsiPlus4: capture.FreshEsiPlus4,
                    CallCellTargetVirtualAddress:
                        capture.SizeCallCellTargetVirtualAddress,
                    CallReturnedNormally: capture.SizeCallReturnedNormally));

        return BuildPlan(
            capture,
            framePlan,
            MapOutcome(sizePlan, mismatchSkip, capture),
            sizeBoundaryReached: true,
            sizeEntryESICaptured: true,
            sizeEntryESI: sizeEntryEsi,
            sizeEntryEAXCaptured: true,
            sizeEntryEAX: capture.SizeEntryEAX,
            sizeEntryESPCaptured: true,
            sizeEntryESP: sizeEntryEsp,
            sizeComparisonReached: true,
            sizePlan: sizePlan,
            unreachedOpaqueCallSizeEntryESICaptureProvided:
                mismatchSkip && capture.OpaqueCallSizeEntryESI.HasValue,
            unreachedOpaqueCallSizeEntryESPCaptureProvided:
                mismatchSkip && capture.OpaqueCallSizeEntryESP.HasValue,
            unreachedFreshEsiPlus4CaptureProvided:
                sizePlan.UnreachedFreshEsiPlus4CaptureProvided,
            unreachedSizeCallCellTargetCaptureProvided:
                sizePlan.UnreachedCallCellTargetCaptureProvided,
            unreachedSizeCallReturnStatusProvided:
                sizePlan.UnreachedCallReturnStatusProvided,
            stopAt: sizePlan.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeFrameSizeLifecycleOutcome MapOutcome(
        CapacityFinalCalleeSizeComparisonCallPlan sizePlan,
        bool mismatchSkip,
        CapacityFinalCalleeFrameSizeLifecycleCapture capture)
    {
        bool successfulOutcome =
            sizePlan.Outcome
                is CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
                or CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally;
        bool reachedExactBoundary =
            sizePlan.StopAtRawFileOffsetExclusive == SizeBoundaryRawFileOffsetExclusive;
        bool hasUnreachedSizeCaptures =
            sizePlan.UnreachedFreshEsiPlus4CaptureProvided
            || sizePlan.UnreachedCallCellTargetCaptureProvided
            || sizePlan.UnreachedCallReturnStatusProvided
            || mismatchSkip && capture.OpaqueCallSizeEntryESI.HasValue
            || mismatchSkip && capture.OpaqueCallSizeEntryESP.HasValue;
        if (successfulOutcome && (!reachedExactBoundary || hasUnreachedSizeCaptures))
        {
            return CapacityFinalCalleeFrameSizeLifecycleOutcome.ContaminatedSizeBoundary;
        }

        return sizePlan.Outcome switch
        {
            CapacityFinalCalleeSizeComparisonCallOutcome.MissingFreshEsiPlus4 =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingFreshEsiPlus4,
            CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.EqualSizesCallSkipped,
            CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallCellTarget =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeCallCellTarget,
            CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallReturnStatus =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeCallReturnStatus,
            CapacityFinalCalleeSizeComparisonCallOutcome.CallDidNotReturnNormally =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.SizeCallDidNotReturnNormally,
            CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally =>
                CapacityFinalCalleeFrameSizeLifecycleOutcome.SizeCallReturnedNormally,
            _ => throw new ArgumentOutOfRangeException(nameof(sizePlan), sizePlan, null)
        };
    }

    private static CapacityFinalCalleeFrameSizeLifecyclePlan BuildPlan(
        CapacityFinalCalleeFrameSizeLifecycleCapture capture,
        CapacityFinalCalleeFrameComparisonCallPlan framePlan,
        CapacityFinalCalleeFrameSizeLifecycleOutcome outcome,
        bool sizeBoundaryReached = false,
        bool sizeEntryESICaptured = false,
        uint? sizeEntryESI = null,
        bool sizeEntryEAXCaptured = false,
        uint? sizeEntryEAX = null,
        bool sizeEntryESPCaptured = false,
        uint? sizeEntryESP = null,
        bool sizeComparisonReached = false,
        CapacityFinalCalleeSizeComparisonCallPlan? sizePlan = null,
        bool unreachedOpaqueCallSizeEntryESICaptureProvided = false,
        bool unreachedOpaqueCallSizeEntryESPCaptureProvided = false,
        bool unreachedSizeEntryEAXCaptureProvided = false,
        bool unreachedFreshEsiPlus4CaptureProvided = false,
        bool unreachedSizeCallCellTargetCaptureProvided = false,
        bool unreachedSizeCallReturnStatusProvided = false,
        uint stopAt = SizeEntryRawFileOffset) =>
        new(
            capture,
            framePlan,
            outcome,
            sizeBoundaryReached,
            sizeEntryESICaptured,
            sizeEntryESICaptured ? sizeEntryESI : null,
            sizeEntryEAXCaptured,
            sizeEntryEAXCaptured ? sizeEntryEAX : null,
            sizeEntryESPCaptured,
            sizeEntryESPCaptured ? sizeEntryESP : null,
            sizeComparisonReached,
            sizeComparisonReached ? sizePlan : null,
            unreachedOpaqueCallSizeEntryESICaptureProvided,
            unreachedOpaqueCallSizeEntryESPCaptureProvided,
            !sizeBoundaryReached && capture.SizeEntryEAX.HasValue
                || unreachedSizeEntryEAXCaptureProvided,
            (!sizeComparisonReached && capture.FreshEsiPlus4.HasValue)
                || unreachedFreshEsiPlus4CaptureProvided,
            (!sizeComparisonReached
                && capture.SizeCallCellTargetVirtualAddress.HasValue)
                || unreachedSizeCallCellTargetCaptureProvided,
            (!sizeComparisonReached && capture.SizeCallReturnedNormally.HasValue)
                || unreachedSizeCallReturnStatusProvided,
            stopAt);
}
