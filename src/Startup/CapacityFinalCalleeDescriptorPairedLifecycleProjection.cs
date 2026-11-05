namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescriptorPairedLifecycleCapture(
    CapacityFinalCalleeDescriptorComparisonCallCapture DescriptorCapture,
    uint? OpaqueCallPairedEntryEAX = null,
    uint? OpaqueCallPairedEntryEBP = null,
    uint? FreshFrameMinus4 = null,
    uint? FreshGlobalValue = null);

public enum CapacityFinalCalleeDescriptorPairedLifecycleOutcome
{
    DescriptorEntryNotValidated,
    DescriptorDidNotReachBoundary,
    ContaminatedDescriptorBoundary,
    MissingPairedEntryRegisters,
    MissingFreshFrameMinus4,
    MissingValidatedEntryEAX,
    MissingFreshGlobalValue,
    Completed
}

public readonly record struct CapacityFinalCalleeDescriptorPairedLifecyclePlan(
    CapacityFinalCalleeDescriptorPairedLifecycleCapture Capture,
    CapacityFinalCalleeDescriptorComparisonCallPlan DescriptorPlan,
    CapacityFinalCalleeDescriptorPairedLifecycleOutcome Outcome,
    bool DescriptorBoundaryReached,
    bool PairedFieldWritesReached,
    bool PairedEntryEAXCaptured,
    uint? PairedEntryEAX,
    bool PairedEntryEBPCaptured,
    uint? PairedEntryEBP,
    CapacityFinalCalleePairedFieldWritesPlan? PairedFieldWritesPlan,
    bool UnreachedOpaqueCallPairedEntryEAXCaptureProvided,
    bool UnreachedOpaqueCallPairedEntryEBPCaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins descriptor comparison to the paired writes only at the clean 0x012C8EE9
/// boundary. Equality bypass carries entry EAX/EBP; a returning opaque call requires
/// separately captured post-call EAX/EBP.
/// </summary>
public static class CapacityFinalCalleeDescriptorPairedLifecycleProjection
{
    public const uint DescriptorBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeDescriptorComparisonCallProjection.BoundaryRawFileOffsetExclusive;
    public const uint PairedFieldWritesEntryRawFileOffset =
        CapacityFinalCalleePairedFieldWritesProjection.EntryRawFileOffset;
    public const uint PairedFieldWritesBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleePairedFieldWritesProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeDescriptorPairedLifecyclePlan Project(
        CapacityFinalCalleeDescriptorPairedLifecycleCapture capture)
    {
        CapacityFinalCalleeDescriptorComparisonCallPlan descriptorPlan =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(
                capture.DescriptorCapture);

        if (descriptorPlan.Outcome
            == CapacityFinalCalleeDescriptorComparisonCallOutcome.EntryNotValidated)
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome.DescriptorEntryNotValidated,
                unreachedOpaqueCallPairedEntryEAXCaptureProvided:
                    capture.OpaqueCallPairedEntryEAX.HasValue,
                unreachedOpaqueCallPairedEntryEBPCaptureProvided:
                    capture.OpaqueCallPairedEntryEBP.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
        }

        bool firstEqualityBypass =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .FirstComparisonEqualCallSkipped;
        bool memoryEqualityBypass =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .MemoryComparisonEqualCallSkipped;
        bool callSkipped = firstEqualityBypass || memoryEqualityBypass;
        bool callReturnedNormally =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallReturnedNormally;
        bool reachedExactBoundary =
            descriptorPlan.StopAtRawFileOffsetExclusive
                == DescriptorBoundaryRawFileOffsetExclusive
            && (callSkipped || callReturnedNormally);
        if (!reachedExactBoundary)
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .DescriptorDidNotReachBoundary,
                unreachedOpaqueCallPairedEntryEAXCaptureProvided:
                    capture.OpaqueCallPairedEntryEAX.HasValue,
                unreachedOpaqueCallPairedEntryEBPCaptureProvided:
                    capture.OpaqueCallPairedEntryEBP.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
        }

        bool contaminatedDescriptorBoundary =
            descriptorPlan.UnreachedFreshEaxPlus8CaptureProvided
            || descriptorPlan.UnreachedCallCellTargetCaptureProvided
            || descriptorPlan.UnreachedCallReturnStatusProvided
            || callSkipped
                && (capture.OpaqueCallPairedEntryEAX.HasValue
                    || capture.OpaqueCallPairedEntryEBP.HasValue);
        if (contaminatedDescriptorBoundary)
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .ContaminatedDescriptorBoundary,
                descriptorBoundaryReached: true,
                unreachedOpaqueCallPairedEntryEAXCaptureProvided:
                    callSkipped && capture.OpaqueCallPairedEntryEAX.HasValue,
                unreachedOpaqueCallPairedEntryEBPCaptureProvided:
                    callSkipped && capture.OpaqueCallPairedEntryEBP.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: PairedFieldWritesEntryRawFileOffset);
        }

        uint? pairedEntryEAX = callSkipped
            ? capture.DescriptorCapture.EntryEAX
            : capture.OpaqueCallPairedEntryEAX;
        uint? pairedEntryEBP = callSkipped
            ? capture.DescriptorCapture.EntryEBP
            : capture.OpaqueCallPairedEntryEBP;
        if (pairedEntryEAX is not uint entryEAX
            || pairedEntryEBP is not uint entryEBP)
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingPairedEntryRegisters,
                descriptorBoundaryReached: true,
                pairedEntryEAXCaptured: pairedEntryEAX.HasValue,
                pairedEntryEAX: pairedEntryEAX,
                pairedEntryEBPCaptured: pairedEntryEBP.HasValue,
                pairedEntryEBP: pairedEntryEBP,
                unreachedOpaqueCallPairedEntryEAXCaptureProvided:
                    callSkipped && capture.OpaqueCallPairedEntryEAX.HasValue,
                unreachedOpaqueCallPairedEntryEBPCaptureProvided:
                    callSkipped && capture.OpaqueCallPairedEntryEBP.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: PairedFieldWritesEntryRawFileOffset);
        }

        CapacityFinalCalleePairedFieldWritesPlan pairedPlan =
            CapacityFinalCalleePairedFieldWritesProjection.Project(
                new CapacityFinalCalleePairedFieldWritesCapture(
                    ValidatedEntryEBP: entryEBP,
                    ValidatedEntryEAX: entryEAX,
                    FreshFrameMinus4: capture.FreshFrameMinus4,
                    FreshGlobalValue: capture.FreshGlobalValue));

        return BuildPlan(
            capture,
            descriptorPlan,
            MapOutcome(pairedPlan.Outcome),
            descriptorBoundaryReached: true,
            pairedFieldWritesReached: true,
            pairedEntryEAXCaptured: true,
            pairedEntryEAX: entryEAX,
            pairedEntryEBPCaptured: true,
            pairedEntryEBP: entryEBP,
            pairedFieldWritesPlan: pairedPlan,
            unreachedOpaqueCallPairedEntryEAXCaptureProvided:
                callSkipped && capture.OpaqueCallPairedEntryEAX.HasValue,
            unreachedOpaqueCallPairedEntryEBPCaptureProvided:
                callSkipped && capture.OpaqueCallPairedEntryEBP.HasValue,
            unreachedFreshFrameMinus4CaptureProvided:
                pairedPlan.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshGlobalValueCaptureProvided:
                pairedPlan.UnreachedFreshGlobalValueCaptureProvided,
            stopAt: pairedPlan.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeDescriptorPairedLifecycleOutcome MapOutcome(
        CapacityFinalCalleePairedFieldWritesOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEBP
                or CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEAX =>
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingPairedEntryRegisters,
            CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome.MissingFreshFrameMinus4,
            CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome.MissingFreshGlobalValue,
            CapacityFinalCalleePairedFieldWritesOutcome.Completed =>
                CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };

    private static CapacityFinalCalleeDescriptorPairedLifecyclePlan BuildPlan(
        CapacityFinalCalleeDescriptorPairedLifecycleCapture capture,
        CapacityFinalCalleeDescriptorComparisonCallPlan descriptorPlan,
        CapacityFinalCalleeDescriptorPairedLifecycleOutcome outcome,
        bool descriptorBoundaryReached = false,
        bool pairedFieldWritesReached = false,
        bool pairedEntryEAXCaptured = false,
        uint? pairedEntryEAX = null,
        bool pairedEntryEBPCaptured = false,
        uint? pairedEntryEBP = null,
        CapacityFinalCalleePairedFieldWritesPlan? pairedFieldWritesPlan = null,
        bool unreachedOpaqueCallPairedEntryEAXCaptureProvided = false,
        bool unreachedOpaqueCallPairedEntryEBPCaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshGlobalValueCaptureProvided = false,
        uint stopAt = DescriptorBoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            descriptorPlan,
            outcome,
            descriptorBoundaryReached,
            pairedFieldWritesReached,
            pairedEntryEAXCaptured,
            pairedEntryEAXCaptured ? pairedEntryEAX : null,
            pairedEntryEBPCaptured,
            pairedEntryEBPCaptured ? pairedEntryEBP : null,
            pairedFieldWritesReached ? pairedFieldWritesPlan : null,
            unreachedOpaqueCallPairedEntryEAXCaptureProvided
                || !descriptorBoundaryReached
                    && capture.OpaqueCallPairedEntryEAX.HasValue,
            unreachedOpaqueCallPairedEntryEBPCaptureProvided
                || !descriptorBoundaryReached
                    && capture.OpaqueCallPairedEntryEBP.HasValue,
            (!pairedFieldWritesReached && capture.FreshFrameMinus4.HasValue)
                || unreachedFreshFrameMinus4CaptureProvided,
            (!pairedFieldWritesReached && capture.FreshGlobalValue.HasValue)
                || unreachedFreshGlobalValueCaptureProvided,
            stopAt);
}
