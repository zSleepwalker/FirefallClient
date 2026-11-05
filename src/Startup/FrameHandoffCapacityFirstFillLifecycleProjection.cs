namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFirstFillLifecycleCapture(
    FrameHandoffCapacitySecondTransferLifecycleCapture SecondTransferLifecycleCapture,
    FrameHandoffCapacityRepeatFillJoinCapture RepeatFillJoinCapture);

public enum FrameHandoffCapacityFirstFillLifecycleOutcome
{
    SecondTransferBlocked,
    SecondTransferHasUnreachedCaptures,
    SecondReturnEntryIdentityMismatch,
    SecondReturnBlocked,
    SecondReturnHasUnreachedCaptures,
    SecondTransferLifecycleNotAdmitted,
    RepeatFillJoinNotAdmitted,
    RepeatFillJoinHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityFirstFillLifecyclePlan(
    FrameHandoffCapacityFirstFillLifecycleCapture Capture,
    FrameHandoffCapacityFirstFillLifecycleOutcome Outcome,
    FrameHandoffCapacitySecondTransferLifecyclePlan SecondTransferLifecyclePlan,
    FrameHandoffCapacityRepeatFillJoinPlan? RepeatFillJoinPlan,
    bool SecondTransferLifecycleSuccessAdmission,
    bool SecondTransferLifecycleUnreachedCapturesClear,
    bool DerivedRepeatFillJoinGatePlan,
    bool RepeatFillJoinAdmissionValidated,
    bool RepeatFillJoinOutcomeClean,
    bool UnreachedRepeatEntryEcxCaptureProvided,
    bool UnreachedRepeatEntryEdxCaptureProvided,
    bool UnreachedRepeatEntryEbpCaptureProvided,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFillJoinEntryEbpCaptureProvided,
    bool UnreachedFillJoinEntryEdiCaptureProvided,
    bool UnreachedFillJoinFrameMinus4CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the second-transfer return lifecycle with its validated fill/REP join.
/// </summary>
public static class FrameHandoffCapacityFirstFillLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacitySecondTransferLifecycleProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFillJoinProjection.EndRawFileOffsetExclusive;

    public static FrameHandoffCapacityFirstFillLifecyclePlan Project(
        FrameHandoffCapacityFirstFillLifecycleCapture capture)
    {
        FrameHandoffCapacitySecondTransferLifecyclePlan lifecyclePlan =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                capture.SecondTransferLifecycleCapture);
        bool lifecycleUnreachedCapturesClear = HasNoUnreachedLifecycleCaptures(lifecyclePlan);

        if (!IsExactSuccessfulLifecycle(lifecyclePlan))
        {
            return StopBeforeFill(
                capture,
                lifecyclePlan,
                MapLifecycleFailure(lifecyclePlan, lifecycleUnreachedCapturesClear),
                lifecycleUnreachedCapturesClear,
                lifecyclePlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffCapacitySecondReturnGatePlan returnGatePlan =
            lifecyclePlan.ReturnGatePlan!.Value;
        FrameHandoffCapacityRepeatFillJoinCapture derivedFillCapture =
            capture.RepeatFillJoinCapture with
            {
                GateRepeatCapture =
                    capture.RepeatFillJoinCapture.GateRepeatCapture with
                    {
                        GatePlan = returnGatePlan
                    }
            };
        FrameHandoffCapacityRepeatFillJoinPlan fillPlan =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(derivedFillCapture);
        bool fillAdmissionValidated = IsExactRepeatFillJoin(fillPlan);
        FrameHandoffCapacityFirstFillLifecycleOutcome outcome =
            fillAdmissionValidated
                ? FrameHandoffCapacityFirstFillLifecycleOutcome.ProjectionComplete
                : HasUnreachedRepeatFillCaptures(fillPlan)
                    ? FrameHandoffCapacityFirstFillLifecycleOutcome
                        .RepeatFillJoinHasUnreachedCaptures
                    : FrameHandoffCapacityFirstFillLifecycleOutcome
                        .RepeatFillJoinNotAdmitted;

        return new(
            capture,
            outcome,
            lifecyclePlan,
            fillPlan,
            SecondTransferLifecycleSuccessAdmission: true,
            SecondTransferLifecycleUnreachedCapturesClear: true,
            DerivedRepeatFillJoinGatePlan: true,
            RepeatFillJoinAdmissionValidated: fillAdmissionValidated,
            RepeatFillJoinOutcomeClean: fillAdmissionValidated,
            fillPlan.GateRepeatPlan.UnreachedRepeatEntryEcxProvided,
            fillPlan.GateRepeatPlan.UnreachedRepeatEntryEdxProvided,
            fillPlan.GateRepeatPlan.UnreachedRepeatEntryEbpProvided,
            fillPlan.GateRepeatPlan.UnreachedDirectionFlagCaptureProvided,
            fillPlan.GateRepeatPlan.UnreachedRepCompletionCaptureProvided,
            fillPlan.GateRepeatPlan.UnreachedFreshFrameMinus8CaptureProvided,
            !fillPlan.GateRepeatAdmissionValidated,
            fillPlan.UnreachedFillJoinEntryEdiCaptureProvided,
            fillPlan.UnreachedFillJoinFrameMinus4ReadCaptureProvided,
            fillPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactSuccessfulLifecycle(
        FrameHandoffCapacitySecondTransferLifecyclePlan plan)
    {
        if (plan.Outcome
                != FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            || !plan.TransferAdmissionChecked
            || !plan.TransferSuccessAdmission
            || !plan.TransferUnreachedCapturesClear
            || !plan.ReturnEntryIdentityValidated
            || !plan.ReturnEntryEbpMatches
            || !plan.ReturnBranchAdmitted
            || !HasNoUnreachedLifecycleCaptures(plan)
            || plan.TransferPlan.Outcome
                != FrameHandoffCapacitySecondTransferOutcome.ReturnedNormally
            || !plan.TransferPlan.NormalBoundaryReached
            || plan.TransferPlan.Capture.CallReturnedNormally != true
            || !plan.TransferPlan.EaxLeaReached
            || !plan.TransferPlan.EdxMoveReached
            || !plan.TransferPlan.ArithmeticShiftRightReached
            || !plan.TransferPlan.EdxSubtractReached
            || !plan.TransferPlan.FrameMinusCWriteReached
            || !plan.TransferPlan.EbxLeaReached
            || !plan.TransferPlan.CallCellReadReached
            || !plan.TransferPlan.CallCellCaptured
            || !plan.TransferPlan.EbxPushReached
            || !plan.TransferPlan.EcxPushReached
            || !plan.TransferPlan.FrameMinus10ReadReached
            || !plan.TransferPlan.FrameMinus10Captured
            || !plan.TransferPlan.FrameMinus10PushReached
            || !plan.TransferPlan.CallReached
            || plan.TransferPlan.CallReturnedNormally != true
            || plan.TransferPlan.Pushes.Length != 3
            || plan.TransferPlan.StopAtRawFileOffsetExclusive
                != FrameHandoffCapacitySecondTransferProjection
                    .NormalBoundaryRawFileOffsetExclusive
            || plan.ReturnGatePlan is not FrameHandoffCapacitySecondReturnGatePlan returnPlan
            || !returnPlan.Capture.ValidatedNormalEntry
            || returnPlan.Capture.EntryEbp != plan.TransferPlan.Capture.EntryEbp
            || !returnPlan.FrameMinusCReadReached
            || !returnPlan.FrameMinusCCaptured
            || !returnPlan.StackCleanupReached
            || !returnPlan.EdxLeaReached
            || !returnPlan.HelperReturnEaxConsumed
            || !returnPlan.TestReached
            || !returnPlan.BranchReached
            || returnPlan.BranchTaken is null)
        {
            return false;
        }

        FrameHandoffCapacitySecondTransferPlan transferPlan = plan.TransferPlan;
        FrameHandoffCapacitySecondTransferCapture transferCapture = transferPlan.Capture;
        if (transferPlan.Reads.Length != 2
            || transferPlan.Reads[0]
                != new FrameHandoffCapacitySecondTransferRead(
                    FrameHandoffCapacitySecondTransferProjection.CallCellReadRawFileOffset,
                    FrameHandoffCapacitySecondTransferProjection.CallCellAddress,
                    transferCapture.CallCellDword)
            || transferPlan.Reads[1]
                != new FrameHandoffCapacitySecondTransferRead(
                    FrameHandoffCapacitySecondTransferProjection.FrameMinus10PushRawFileOffset,
                    FrameHandoffCapacitySecondTransferProjection.FrameMinus10Address(
                        transferCapture.EntryEbp),
                    transferCapture.FrameMinus10Dword)
            || transferPlan.FrameMinusCWriteAddress
                != FrameHandoffCapacitySecondTransferProjection.FrameMinusCAddress(
                    transferCapture.EntryEbp)
            || transferPlan.FrameMinusCWriteValue
                != unchecked(transferCapture.EntryEsi - transferCapture.EntryEdi)
            || transferPlan.CallTargetAddress != transferCapture.CallCellDword
            || transferPlan.Pushes[0]
                != new FrameHandoffCapacitySecondTransferPush(
                    FrameHandoffCapacitySecondTransferProjection.PushEbxRawFileOffset,
                    "EBX",
                    transferPlan.EbxAfterLea!.Value)
            || transferPlan.Pushes[1]
                != new FrameHandoffCapacitySecondTransferPush(
                    FrameHandoffCapacitySecondTransferProjection.PushEcxRawFileOffset,
                    "ECX",
                    transferCapture.EntryEcx)
            || transferPlan.Pushes[2]
                != new FrameHandoffCapacitySecondTransferPush(
                    FrameHandoffCapacitySecondTransferProjection.FrameMinus10PushRawFileOffset,
                    "[EBP-0x10]",
                    transferCapture.FrameMinus10Dword!.Value)
            || returnPlan.Reads.Length != 1
            || returnPlan.Reads[0]
                != new FrameHandoffCapacitySecondReturnGateRead(
                    FrameHandoffCapacitySecondReturnGateProjection
                        .FrameMinusCReadRawFileOffset,
                    FrameHandoffCapacitySecondReturnGateProjection.FrameMinusCAddress(
                        returnPlan.Capture.EntryEbp),
                    returnPlan.Capture.FrameMinusC)
            || returnPlan.EdxAfterLea
                != FrameHandoffCapacitySecondReturnGateProjection
                    .AddEntryEbxAndHelperReturnEax(
                        returnPlan.Capture.EntryEbx,
                        returnPlan.Capture.HelperReturnEax!.Value))
        {
            return false;
        }

        return returnPlan.Outcome switch
        {
            FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken =>
                returnPlan.Capture.FrameMinusC == 0
                && returnPlan.FrameMinusCIsZero == true
                && returnPlan.BranchTaken == true
                && returnPlan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && returnPlan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset,
            FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough =>
                returnPlan.Capture.FrameMinusC is uint frameMinusC
                && frameMinusC != 0
                && returnPlan.FrameMinusCIsZero == false
                && returnPlan.BranchTaken == false
                && returnPlan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && returnPlan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacitySecondReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive,
            _ => false
        };
    }

    private static bool HasNoUnreachedLifecycleCaptures(
        FrameHandoffCapacitySecondTransferLifecyclePlan plan) =>
        plan.TransferUnreachedCapturesClear
        && !plan.TransferPlan.UnreachedFrameMinus10CaptureProvided
        && !plan.TransferPlan.UnreachedCallReturnCaptureProvided
        && !plan.UnreachedReturnFrameMinusCCaptureProvided
        && !plan.UnreachedReturnHelperReturnEaxCaptureProvided
        && (plan.ReturnGatePlan is not FrameHandoffCapacitySecondReturnGatePlan returnPlan
            || (!returnPlan.UnreachedFrameMinusCCaptureProvided
                && !returnPlan.UnreachedHelperReturnEaxCaptureProvided));

    private static bool IsExactRepeatFillJoin(
        FrameHandoffCapacityRepeatFillJoinPlan plan)
    {
        if (!plan.GateRepeatAdmissionValidated
            || !plan.FillJoinAdmissionValidated
            || !plan.ExactFillJoinOutcome
            || !plan.NoUnreachedFillJoinCaptures
            || plan.GateRepeatPlan.Capture.GatePlan
                is not FrameHandoffCapacitySecondReturnGatePlan
            || !plan.GateRepeatPlan.GateUnreachedCapturesClear
            || plan.UnreachedFillJoinEntryEdiCaptureProvided
            || plan.UnreachedFillJoinFrameMinus4ReadCaptureProvided)
        {
            return false;
        }

        if (plan.FillJoinPlan is not FrameHandoffCapacityFillJoinPlan fillJoinPlan)
        {
            return false;
        }

        bool repeatBypassed =
            plan.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && plan.GateRepeatPlan.ZeroJoinAdmission
            && !plan.GateRepeatPlan.NonzeroGateAdmission
            && plan.GateRepeatPlan.RepeatPlan is null
            && GateRepeatCapturesClear(plan.GateRepeatPlan);
        bool repeatCompleted =
            plan.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && plan.GateRepeatPlan.NonzeroGateAdmission
            && !plan.GateRepeatPlan.ZeroJoinAdmission
            && plan.GateRepeatPlan.RepeatPlan is FrameHandoffCapacityRepeatStorePlan repeatPlan
            && repeatPlan.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && repeatPlan.Capture.RepReturnedNormally == true
            && repeatPlan.AllStoresCompleted
            && repeatPlan.FreshFrameMinus8Value.HasValue
            && repeatPlan.EcxAfterRep == 0
            && GateRepeatCapturesClear(plan.GateRepeatPlan);
        bool exactFillBranch =
            fillJoinPlan.FrameMinus4ReadReached
            && fillJoinPlan.FrameMinus4ReadCaptured
            && fillJoinPlan.EbxAfterFrameMinus4Read.HasValue
            && fillJoinPlan.EntryEdiTestReached
            && fillJoinPlan.ZeroBranchReached
            && !fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided
            && (fillJoinPlan.Outcome
                    == FrameHandoffCapacityFillJoinOutcome
                        .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
                ? fillJoinPlan.EntryEdiZeroFlag == true
                    && fillJoinPlan.ZeroBranchTaken == true
                    && fillJoinPlan.BranchTargetRawFileOffset
                        == FrameHandoffCapacityFillJoinProjection
                            .ZeroBranchTargetRawFileOffset
                    && !fillJoinPlan.EcxEntryEdiMoveReached
                    && !fillJoinPlan.EdiLoadedEbxMoveReached
                    && !fillJoinPlan.JumpReached
                    && fillJoinPlan.StopAtRawFileOffsetExclusive
                        == FrameHandoffCapacityFillJoinProjection
                            .ZeroBranchTargetRawFileOffset
                : fillJoinPlan.Outcome
                    == FrameHandoffCapacityFillJoinOutcome
                        .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
                    && fillJoinPlan.EntryEdiZeroFlag == false
                    && fillJoinPlan.ZeroBranchTaken == false
                    && fillJoinPlan.BranchTargetRawFileOffset is null
                    && fillJoinPlan.EcxEntryEdiMoveReached
                    && fillJoinPlan.EcxAfterMove == fillJoinPlan.Capture.EntryEdi
                    && fillJoinPlan.EdiLoadedEbxMoveReached
                    && fillJoinPlan.EdiAfterMove
                        == fillJoinPlan.EbxAfterFrameMinus4Read
                    && fillJoinPlan.JumpReached
                    && fillJoinPlan.JumpTargetRawFileOffset
                        == FrameHandoffCapacityFillJoinProjection
                            .NonzeroJumpTargetRawFileOffset
                    && fillJoinPlan.StopAtRawFileOffsetExclusive
                        == FrameHandoffCapacityFillJoinProjection
                            .NonzeroJumpTargetRawFileOffset);

        return (repeatBypassed || repeatCompleted) && exactFillBranch;
    }

    private static bool GateRepeatCapturesClear(
        FrameHandoffCapacityGateRepeatPlan plan) =>
        !plan.UnreachedRepeatEntryEcxProvided
        && !plan.UnreachedRepeatEntryEdxProvided
        && !plan.UnreachedRepeatEntryEbpProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && !plan.UnreachedFreshFrameMinus8CaptureProvided;

    private static bool HasUnreachedRepeatFillCaptures(
        FrameHandoffCapacityRepeatFillJoinPlan plan) =>
        !plan.GateRepeatPlan.GateUnreachedCapturesClear
        || !GateRepeatCapturesClear(plan.GateRepeatPlan)
        || !plan.GateRepeatAdmissionValidated
        || plan.UnreachedFillJoinEntryEdiCaptureProvided
        || plan.UnreachedFillJoinFrameMinus4ReadCaptureProvided;

    private static FrameHandoffCapacityFirstFillLifecycleOutcome MapLifecycleFailure(
        FrameHandoffCapacitySecondTransferLifecyclePlan plan,
        bool unreachedCapturesClear) =>
        plan.Outcome switch
        {
            FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferBlocked =>
                FrameHandoffCapacityFirstFillLifecycleOutcome.SecondTransferBlocked,
            FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferHasUnreachedCaptures =>
                FrameHandoffCapacityFirstFillLifecycleOutcome.SecondTransferHasUnreachedCaptures,
            FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnEntryIdentityMismatch =>
                FrameHandoffCapacityFirstFillLifecycleOutcome
                    .SecondReturnEntryIdentityMismatch,
            FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnBlocked =>
                FrameHandoffCapacityFirstFillLifecycleOutcome.SecondReturnBlocked,
            FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnHasUnreachedCaptures =>
                FrameHandoffCapacityFirstFillLifecycleOutcome
                    .SecondReturnHasUnreachedCaptures,
            _ when !unreachedCapturesClear =>
                FrameHandoffCapacityFirstFillLifecycleOutcome
                    .SecondTransferHasUnreachedCaptures,
            _ => FrameHandoffCapacityFirstFillLifecycleOutcome
                .SecondTransferLifecycleNotAdmitted
        };

    private static FrameHandoffCapacityFirstFillLifecyclePlan StopBeforeFill(
        FrameHandoffCapacityFirstFillLifecycleCapture capture,
        FrameHandoffCapacitySecondTransferLifecyclePlan lifecyclePlan,
        FrameHandoffCapacityFirstFillLifecycleOutcome outcome,
        bool lifecycleUnreachedCapturesClear,
        uint stopAt) =>
        new(
            capture,
            outcome,
            lifecyclePlan,
            null,
            SecondTransferLifecycleSuccessAdmission: false,
            lifecycleUnreachedCapturesClear,
            DerivedRepeatFillJoinGatePlan: false,
            RepeatFillJoinAdmissionValidated: false,
            RepeatFillJoinOutcomeClean: false,
            capture.RepeatFillJoinCapture.GateRepeatCapture.RepeatEntryEcx.HasValue,
            capture.RepeatFillJoinCapture.GateRepeatCapture.RepeatEntryEdx.HasValue,
            capture.RepeatFillJoinCapture.GateRepeatCapture.RepeatEntryEbp.HasValue,
            capture.RepeatFillJoinCapture.GateRepeatCapture.DirectionFlagSet.HasValue,
            capture.RepeatFillJoinCapture.GateRepeatCapture.RepReturnedNormally.HasValue,
            capture.RepeatFillJoinCapture.GateRepeatCapture.FreshFrameMinus8Value.HasValue,
            UnreachedFillJoinEntryEbpCaptureProvided: true,
            capture.RepeatFillJoinCapture.FillJoinEntryEdi.HasValue,
            capture.RepeatFillJoinCapture.FrameMinus4.HasValue,
            stopAt);
}
