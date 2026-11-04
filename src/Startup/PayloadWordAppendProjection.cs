namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordAppendCapture(
    PayloadWordGrowthGateCapture? GrowthGate,
    PayloadWordSlotIndexCapture? SlotIndex,
    PayloadWordSlotAllocationCapture? SlotAllocation,
    PayloadWordCopyTailCapture? CopyTail);

public enum PayloadWordAppendOutcome
{
    MissingGrowthGateStage,
    UnreachedLaterStageCaptureProvided,
    GrowthGateInvalid,
    MissingSlotIndexStage,
    SlotIndexInvalid,
    MissingSlotAllocationStage,
    SlotAllocationInvalid,
    AllocationCaptureForNonzeroSlot,
    MissingCopyTailStage,
    CopyTailInvalid,
    CompletedWithExistingSlot,
    CompletedAfterSlotAllocation,
    ReceiverPointerMismatch,
    RetainedEdiMismatch,
    RetainedEbxMismatch
}

public readonly record struct PayloadWordAppendPlan(
    PayloadWordAppendCapture Capture,
    PayloadWordAppendOutcome Outcome,
    PayloadWordGrowthGatePlan? GrowthGatePlan,
    PayloadWordSlotIndexPlan? SlotIndexPlan,
    PayloadWordSlotAllocationPlan? SlotAllocationPlan,
    PayloadWordCopyTailPlan? CopyTailPlan);

/// <summary>
/// Composes the bounded payload-word gate, slot-index, allocation, and copy-tail
/// projections without adding memory-access or arithmetic semantics.
/// </summary>
public static class PayloadWordAppendProjection
{
    public static PayloadWordAppendPlan Project(PayloadWordAppendCapture capture)
    {
        if (capture.GrowthGate is not { } growthGate)
        {
            return Stop(capture, PayloadWordAppendOutcome.MissingGrowthGateStage);
        }

        PayloadWordGrowthGatePlan gate =
            PayloadWordGrowthGateProjection.Project(growthGate);
        PayloadWordGrowthGatePlan? gatePlan = gate;
        if (HasReceiverPointerMismatch(capture, growthGate.ReceiverPointer))
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.ReceiverPointerMismatch,
                gatePlan);
        }

        bool gateLowBitsBranch = IsGateOutcome(
            gate,
            PayloadWordGrowthGateOutcome.LowBitsBranchToBoundary,
            PayloadWordGrowthGateProjection.LowBitsBranchTargetRawFileOffset);
        bool gateReceiverBoundBranch = IsGateOutcome(
            gate,
            PayloadWordGrowthGateOutcome.ReceiverBoundBranchesToBoundary,
            PayloadWordGrowthGateProjection.ReceiverBoundBranchTargetRawFileOffset);
        bool gateCallReturnedNormally = IsGateOutcome(
            gate,
            PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally,
            PayloadWordGrowthGateProjection.HandoffCallReturnBoundaryRawFileOffset);
        if (!gateLowBitsBranch && !gateReceiverBoundBranch && !gateCallReturnedNormally)
        {
            return Stop(capture, PayloadWordAppendOutcome.GrowthGateInvalid, gatePlan);
        }

        if (capture.SlotIndex is not { } slotIndex)
        {
            return Stop(
                capture,
                HasCaptureAfterGate(capture)
                    ? PayloadWordAppendOutcome.UnreachedLaterStageCaptureProvided
                    : PayloadWordAppendOutcome.MissingSlotIndexStage,
                gatePlan);
        }

        PayloadWordSlotIndexPlan index =
            PayloadWordSlotIndexProjection.Project(slotIndex);
        PayloadWordSlotIndexPlan? indexPlan = index;
        if (slotIndex.ReceiverPointer != growthGate.ReceiverPointer)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.ReceiverPointerMismatch,
                gatePlan,
                indexPlan);
        }

        bool indexHasNonzeroOutcome = index.Outcome
            == PayloadWordSlotIndexOutcome.SlotDwordNonzeroBranchesToBoundary
            && index.RouteRawFileOffset
                == PayloadWordSlotIndexProjection.BranchTargetRawFileOffset
            && index.StopAtRawFileOffsetExclusive
                == PayloadWordSlotIndexProjection.BranchTargetRawFileOffset
            && index.SlotDwordIsNonzero == true;
        bool indexHasZeroOutcome = index.Outcome
            == PayloadWordSlotIndexOutcome.SlotDwordZeroFallsThroughToBoundary
            && index.RouteRawFileOffset
                == PayloadWordSlotIndexProjection.GateEndRawFileOffsetExclusive
            && index.StopAtRawFileOffsetExclusive
                == PayloadWordSlotIndexProjection.GateEndRawFileOffsetExclusive
            && index.SlotDwordIsNonzero == false;
        if (!indexHasNonzeroOutcome && !indexHasZeroOutcome)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.SlotIndexInvalid,
                gatePlan,
                indexPlan);
        }

        PayloadWordSlotAllocationPlan? allocationPlan = null;
        if (indexHasNonzeroOutcome && capture.SlotAllocation is not null)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.AllocationCaptureForNonzeroSlot,
                gatePlan,
                indexPlan);
        }

        if (indexHasZeroOutcome && capture.SlotAllocation is null)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.MissingSlotAllocationStage,
                gatePlan,
                indexPlan);
        }

        if (indexHasZeroOutcome)
        {
            PayloadWordSlotAllocationCapture slotAllocation =
                capture.SlotAllocation!.Value;
            if (slotAllocation.ReceiverPointer != growthGate.ReceiverPointer)
            {
                return Stop(
                    capture,
                    PayloadWordAppendOutcome.ReceiverPointerMismatch,
                    gatePlan,
                    indexPlan);
            }

            PayloadWordSlotAllocationPlan allocation =
                PayloadWordSlotAllocationProjection.Project(slotAllocation);
            allocationPlan = allocation;
            if (index.ScaledOffset is not uint allocationOffset
                || allocation.Capture.RetainedScaledOffset != allocationOffset)
            {
                return Stop(
                    capture,
                    PayloadWordAppendOutcome.RetainedEbxMismatch,
                    gatePlan,
                    indexPlan,
                    allocationPlan);
            }

            if (allocation.Outcome != PayloadWordSlotAllocationOutcome.SlotDwordWritten
                || !allocation.SlotDwordWritten
                || allocation.StopAtRawFileOffsetExclusive
                    != PayloadWordSlotAllocationProjection.GateEndRawFileOffsetExclusive)
            {
                return Stop(
                    capture,
                    PayloadWordAppendOutcome.SlotAllocationInvalid,
                    gatePlan,
                    indexPlan,
                    allocationPlan);
            }
        }

        if (capture.CopyTail is not { } copyTail)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.MissingCopyTailStage,
                gatePlan,
                indexPlan,
                allocationPlan);
        }

        if (copyTail.ReceiverPointer != growthGate.ReceiverPointer)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.ReceiverPointerMismatch,
                gatePlan,
                indexPlan,
                allocationPlan);
        }

        PayloadWordCopyTailPlan tail = PayloadWordCopyTailProjection.Project(copyTail);
        PayloadWordCopyTailPlan? tailPlan = tail;
        if (index.AddedDword is not uint addedDword
            || tail.Capture.RetainedEdi != addedDword)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.RetainedEdiMismatch,
                gatePlan,
                indexPlan,
                allocationPlan,
                tailPlan);
        }

        if (index.ScaledOffset is not uint scaledOffset
            || tail.Capture.RetainedEbx != scaledOffset)
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.RetainedEbxMismatch,
                gatePlan,
                indexPlan,
                allocationPlan,
                tailPlan);
        }

        if (!IsTailComplete(tail))
        {
            return Stop(
                capture,
                PayloadWordAppendOutcome.CopyTailInvalid,
                gatePlan,
                indexPlan,
                allocationPlan,
                tailPlan);
        }

        return Stop(
            capture,
            indexHasNonzeroOutcome
                ? PayloadWordAppendOutcome.CompletedWithExistingSlot
                : PayloadWordAppendOutcome.CompletedAfterSlotAllocation,
            gatePlan,
            indexPlan,
            allocationPlan,
            tailPlan);
    }

    private static bool IsGateOutcome(
        PayloadWordGrowthGatePlan plan,
        PayloadWordGrowthGateOutcome outcome,
        uint route) =>
        plan.Outcome == outcome
        && plan.RouteRawFileOffset == route
        && plan.StopAtRawFileOffsetExclusive
            == PayloadWordGrowthGateProjection.GateEndRawFileOffsetExclusive;

    private static bool IsTailComplete(PayloadWordCopyTailPlan plan) =>
        (plan.Outcome is PayloadWordCopyTailOutcome.CandidateZeroSkippedWordCopy
            or PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented)
        && plan.Ret4BoundaryRawFileOffsetExclusive
            == PayloadWordCopyTailProjection.Ret4BoundaryRawFileOffsetExclusive
        && plan.StopAtRawFileOffsetExclusive
            == PayloadWordCopyTailProjection.Ret4BoundaryRawFileOffsetExclusive;

    private static bool HasReceiverPointerMismatch(
        PayloadWordAppendCapture capture,
        uint receiverPointer) =>
        capture.SlotIndex is { } index
            && index.ReceiverPointer != receiverPointer
        || capture.SlotAllocation is { } allocation
            && allocation.ReceiverPointer != receiverPointer
        || capture.CopyTail is { } tail
            && tail.ReceiverPointer != receiverPointer;

    private static bool HasCaptureAfterGate(PayloadWordAppendCapture capture) =>
        capture.SlotAllocation is not null || capture.CopyTail is not null;

    private static PayloadWordAppendPlan Stop(
        PayloadWordAppendCapture capture,
        PayloadWordAppendOutcome outcome,
        PayloadWordGrowthGatePlan? growthGatePlan = null,
        PayloadWordSlotIndexPlan? slotIndexPlan = null,
        PayloadWordSlotAllocationPlan? allocationPlan = null,
        PayloadWordCopyTailPlan? copyTailPlan = null) =>
        new(
            capture,
            outcome,
            growthGatePlan,
            slotIndexPlan,
            allocationPlan,
            copyTailPlan);
}
