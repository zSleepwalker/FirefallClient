namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSizeDescriptorLifecycleCapture(
    CapacityFinalCalleeSizeComparisonCallCapture SizeCapture,
    uint? OpaqueCallDescriptorEntryESI = null,
    uint? DescriptorEntryEBP = null,
    uint? DescriptorEntryEBX = null,
    uint? InitialFrameMinus10 = null,
    uint? FreshFrameMinus4 = null,
    uint? FreshFrameMinus10 = null,
    uint? FreshEaxPlusC = null);

public enum CapacityFinalCalleeSizeDescriptorLifecycleOutcome
{
    SizeEntryNotValidated,
    SizeDidNotReachBoundary,
    ContaminatedSizeBoundary,
    ContaminatedDescriptorBoundary,
    MissingDescriptorEntryRegisters,
    MissingInitialFrameMinus10,
    MissingFreshFrameMinus4,
    MissingFreshFrameMinus10,
    DescriptorFirstCompareEqualExit,
    DescriptorEntryEaxUnsignedBelowExit,
    MissingFreshEaxPlusC,
    DescriptorFreshEaxPlusCEqualExit,
    DescriptorFellThroughToBoundary
}

public readonly record struct CapacityFinalCalleeSizeDescriptorLifecyclePlan(
    CapacityFinalCalleeSizeDescriptorLifecycleCapture Capture,
    CapacityFinalCalleeSizeComparisonCallPlan SizePlan,
    CapacityFinalCalleeSizeDescriptorLifecycleOutcome Outcome,
    bool SizeBoundaryReached,
    bool DescriptorReloadReached,
    bool DescriptorEntryEBPCaptured,
    uint? DescriptorEntryEBP,
    bool DescriptorEntryEBXCaptured,
    uint? DescriptorEntryEBX,
    bool DescriptorEntryESICaptured,
    uint? DescriptorEntryESI,
    CapacityFinalCalleeDescriptorReloadPlan? DescriptorPlan,
    bool UnreachedOpaqueCallDescriptorEntryESICaptureProvided,
    bool UnreachedDescriptorEntryEBPCaptureProvided,
    bool UnreachedDescriptorEntryEBXCaptureProvided,
    bool UnreachedInitialFrameMinus10CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshFrameMinus10CaptureProvided,
    bool UnreachedFreshEaxPlusCCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the standalone size-call window to descriptor reload only at its clean
/// 0x012C8E9A boundary. The skipped-call route carries only entry ESI forward;
/// EBP and EBX are always independent captures, and call outputs are not assumed
/// to preserve any register.
/// </summary>
public static class CapacityFinalCalleeSizeDescriptorLifecycleProjection
{
    public const uint SizeBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeSizeComparisonCallProjection.BoundaryRawFileOffsetExclusive;
    public const uint DescriptorEntryRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset;
    public const uint DescriptorBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeDescriptorReloadProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeSizeDescriptorLifecyclePlan Project(
        CapacityFinalCalleeSizeDescriptorLifecycleCapture capture)
    {
        CapacityFinalCalleeSizeComparisonCallPlan sizePlan =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(capture.SizeCapture);

        if (sizePlan.Outcome
            == CapacityFinalCalleeSizeComparisonCallOutcome.EntryNotValidated)
        {
            return BuildPlan(
                capture,
                sizePlan,
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.SizeEntryNotValidated,
                unreachedOpaqueCallDescriptorEntryESICaptureProvided:
                    capture.OpaqueCallDescriptorEntryESI.HasValue,
                unreachedDescriptorEntryEBPCaptureProvided:
                    capture.DescriptorEntryEBP.HasValue,
                unreachedDescriptorEntryEBXCaptureProvided:
                    capture.DescriptorEntryEBX.HasValue,
                unreachedInitialFrameMinus10CaptureProvided:
                    capture.InitialFrameMinus10.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: sizePlan.StopAtRawFileOffsetExclusive);
        }

        bool callSkipped =
            sizePlan.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped;
        bool callReturnedNormally =
            sizePlan.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally;
        bool reachedExactBoundary =
            sizePlan.StopAtRawFileOffsetExclusive == SizeBoundaryRawFileOffsetExclusive
            && (callSkipped || callReturnedNormally);
        if (!reachedExactBoundary)
        {
            return BuildPlan(
                capture,
                sizePlan,
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.SizeDidNotReachBoundary,
                unreachedOpaqueCallDescriptorEntryESICaptureProvided:
                    capture.OpaqueCallDescriptorEntryESI.HasValue,
                unreachedDescriptorEntryEBPCaptureProvided:
                    capture.DescriptorEntryEBP.HasValue,
                unreachedDescriptorEntryEBXCaptureProvided:
                    capture.DescriptorEntryEBX.HasValue,
                unreachedInitialFrameMinus10CaptureProvided:
                    capture.InitialFrameMinus10.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: sizePlan.StopAtRawFileOffsetExclusive);
        }

        bool contaminatedSizeBoundary =
            sizePlan.UnreachedFreshEsiPlus4CaptureProvided
            || sizePlan.UnreachedCallCellTargetCaptureProvided
            || sizePlan.UnreachedCallReturnStatusProvided
            || callSkipped && capture.OpaqueCallDescriptorEntryESI.HasValue;
        if (contaminatedSizeBoundary)
        {
            return BuildPlan(
                capture,
                sizePlan,
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedSizeBoundary,
                sizeBoundaryReached: true,
                unreachedOpaqueCallDescriptorEntryESICaptureProvided:
                    callSkipped && capture.OpaqueCallDescriptorEntryESI.HasValue,
                unreachedDescriptorEntryEBPCaptureProvided:
                    capture.DescriptorEntryEBP.HasValue,
                unreachedDescriptorEntryEBXCaptureProvided:
                    capture.DescriptorEntryEBX.HasValue,
                unreachedInitialFrameMinus10CaptureProvided:
                    capture.InitialFrameMinus10.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: DescriptorEntryRawFileOffset);
        }

        uint? descriptorEntryEsi = callSkipped
            ? capture.SizeCapture.EntryESI
            : capture.OpaqueCallDescriptorEntryESI;
        if (descriptorEntryEsi is null
            || capture.DescriptorEntryEBP is null
            || capture.DescriptorEntryEBX is null)
        {
            return BuildPlan(
                capture,
                sizePlan,
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingDescriptorEntryRegisters,
                sizeBoundaryReached: true,
                descriptorEntryEBPCaptured: capture.DescriptorEntryEBP.HasValue,
                descriptorEntryEBP: capture.DescriptorEntryEBP,
                descriptorEntryEBXCaptured: capture.DescriptorEntryEBX.HasValue,
                descriptorEntryEBX: capture.DescriptorEntryEBX,
                descriptorEntryESICaptured: descriptorEntryEsi.HasValue,
                descriptorEntryESI: descriptorEntryEsi,
                unreachedOpaqueCallDescriptorEntryESICaptureProvided:
                    callSkipped && capture.OpaqueCallDescriptorEntryESI.HasValue,
                unreachedInitialFrameMinus10CaptureProvided:
                    capture.InitialFrameMinus10.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: DescriptorEntryRawFileOffset);
        }

        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan =
            CapacityFinalCalleeDescriptorReloadProjection.Project(
                new CapacityFinalCalleeDescriptorReloadCapture(
                    CaptureValidatedEntry: true,
                    EntryEBP: capture.DescriptorEntryEBP.Value,
                    EntryEBX: capture.DescriptorEntryEBX.Value,
                    EntryESI: descriptorEntryEsi.Value,
                    InitialFrameMinus10: capture.InitialFrameMinus10,
                    FreshFrameMinus4: capture.FreshFrameMinus4,
                    FreshFrameMinus10: capture.FreshFrameMinus10,
                    FreshEaxPlusC: capture.FreshEaxPlusC));

        CapacityFinalCalleeSizeDescriptorLifecycleOutcome descriptorOutcome =
            MapDescriptorOutcome(descriptorPlan.Outcome);
        bool successfulDescriptorOutcome =
            descriptorPlan.Outcome is
                CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
                or CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit
                or CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
                or CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary;
        bool contaminatedDescriptorBoundary =
            descriptorPlan.UnreachedInitialFrameMinus10CaptureProvided
            || descriptorPlan.UnreachedFreshFrameMinus4CaptureProvided
            || descriptorPlan.UnreachedFreshFrameMinus10CaptureProvided
            || descriptorPlan.UnreachedFreshEaxPlusCCaptureProvided;
        if (successfulDescriptorOutcome && contaminatedDescriptorBoundary)
        {
            descriptorOutcome =
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedDescriptorBoundary;
        }

        return BuildPlan(
            capture,
            sizePlan,
            descriptorOutcome,
            sizeBoundaryReached: true,
            descriptorReloadReached: true,
            descriptorEntryEBPCaptured: true,
            descriptorEntryEBP: capture.DescriptorEntryEBP,
            descriptorEntryEBXCaptured: true,
            descriptorEntryEBX: capture.DescriptorEntryEBX,
            descriptorEntryESICaptured: true,
            descriptorEntryESI: descriptorEntryEsi,
            descriptorPlan: descriptorPlan,
            unreachedOpaqueCallDescriptorEntryESICaptureProvided:
                callSkipped && capture.OpaqueCallDescriptorEntryESI.HasValue,
            unreachedInitialFrameMinus10CaptureProvided:
                descriptorPlan.UnreachedInitialFrameMinus10CaptureProvided,
            unreachedFreshFrameMinus4CaptureProvided:
                descriptorPlan.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshFrameMinus10CaptureProvided:
                descriptorPlan.UnreachedFreshFrameMinus10CaptureProvided,
            unreachedFreshEaxPlusCCaptureProvided:
                descriptorPlan.UnreachedFreshEaxPlusCCaptureProvided,
            stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeSizeDescriptorLifecycleOutcome MapDescriptorOutcome(
        CapacityFinalCalleeDescriptorReloadOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10 =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingInitialFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingFreshFrameMinus4,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus10 =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingFreshFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFirstCompareEqualExit,
            CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorEntryEaxUnsignedBelowExit,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshEaxPlusC =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingFreshEaxPlusC,
            CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFreshEaxPlusCEqualExit,
            CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary =>
                CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFellThroughToBoundary,
            _ => throw new InvalidOperationException(
                $"Unexpected descriptor-reload outcome: {outcome}.")
        };

    private static CapacityFinalCalleeSizeDescriptorLifecyclePlan BuildPlan(
        CapacityFinalCalleeSizeDescriptorLifecycleCapture capture,
        CapacityFinalCalleeSizeComparisonCallPlan sizePlan,
        CapacityFinalCalleeSizeDescriptorLifecycleOutcome outcome,
        bool sizeBoundaryReached = false,
        bool descriptorReloadReached = false,
        bool descriptorEntryEBPCaptured = false,
        uint? descriptorEntryEBP = null,
        bool descriptorEntryEBXCaptured = false,
        uint? descriptorEntryEBX = null,
        bool descriptorEntryESICaptured = false,
        uint? descriptorEntryESI = null,
        CapacityFinalCalleeDescriptorReloadPlan? descriptorPlan = null,
        bool unreachedOpaqueCallDescriptorEntryESICaptureProvided = false,
        bool unreachedDescriptorEntryEBPCaptureProvided = false,
        bool unreachedDescriptorEntryEBXCaptureProvided = false,
        bool unreachedInitialFrameMinus10CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshFrameMinus10CaptureProvided = false,
        bool unreachedFreshEaxPlusCCaptureProvided = false,
        uint? stopAt = null) =>
        new(
            capture,
            sizePlan,
            outcome,
            sizeBoundaryReached,
            descriptorReloadReached,
            descriptorEntryEBPCaptured,
            descriptorEntryEBP,
            descriptorEntryEBXCaptured,
            descriptorEntryEBX,
            descriptorEntryESICaptured,
            descriptorEntryESI,
            descriptorPlan,
            unreachedOpaqueCallDescriptorEntryESICaptureProvided,
            unreachedDescriptorEntryEBPCaptureProvided,
            unreachedDescriptorEntryEBXCaptureProvided,
            unreachedInitialFrameMinus10CaptureProvided,
            unreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshFrameMinus10CaptureProvided,
            unreachedFreshEaxPlusCCaptureProvided,
            stopAt ?? sizePlan.StopAtRawFileOffsetExclusive);
}
