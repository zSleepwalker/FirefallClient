namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveLifecycleCapture(
    FrameHandoffCapacityAboveTransferCapture AboveTransfer,
    FrameHandoffCapacityAboveSecondTransferCapture AboveSecondTransfer,
    FrameHandoffCapacityAboveReturnGateCapture AboveReturnGate);

public enum FrameHandoffCapacityAboveLifecycleOutcome
{
    AboveTransferBlocked,
    AboveTransferHasUnreachedCaptures,
    AboveSecondTransferBlocked,
    AboveSecondTransferHasUnreachedCaptures,
    ReturnGateFrameIdentityMismatch,
    AboveReturnGateBlocked,
    AboveReturnGateHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityAboveLifecyclePlan(
    FrameHandoffCapacityAboveLifecycleCapture Capture,
    FrameHandoffCapacityAboveLifecycleOutcome Outcome,
    FrameHandoffCapacityAboveTransferPlan AboveTransferPlan,
    FrameHandoffCapacityAboveSecondTransferPlan? AboveSecondTransferPlan,
    FrameHandoffCapacityAboveReturnGatePlan? AboveReturnGatePlan,
    bool AboveTransferAdmissionChecked,
    bool AboveTransferSuccessAdmission,
    bool AboveTransferUnreachedCapturesClear,
    bool AboveSecondTransferNormalEntryDerived,
    bool AboveSecondTransferSuccessAdmission,
    bool AboveSecondTransferUnreachedCapturesClear,
    bool ReturnGateNormalEntryDerived,
    bool ReturnGateFrameEbpIdentityValidated,
    bool ReturnGateFrameEbpMatches,
    bool ReturnGateSuccessAdmission,
    bool ReturnGateUnreachedCapturesClear,
    bool UnreachedAboveSecondTransferReceiver4CaptureProvided,
    bool UnreachedAboveSecondTransferFrameMinusCCaptureProvided,
    bool UnreachedAboveSecondTransferFrameMinus4CaptureProvided,
    bool UnreachedAboveSecondTransferCallReturnCaptureProvided,
    bool UnreachedAboveReturnGateHelperReturnEaxCaptureProvided,
    bool UnreachedAboveReturnGateFrameMinus4CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the above-transfer, above-second-transfer, and return-gate projections.
/// </summary>
public static class FrameHandoffCapacityAboveLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityAboveTransferProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryRawFileOffsetExclusive;

    public static FrameHandoffCapacityAboveLifecyclePlan Project(
        FrameHandoffCapacityAboveLifecycleCapture capture)
    {
        FrameHandoffCapacityAboveTransferPlan aboveTransferPlan =
            FrameHandoffCapacityAboveTransferProjection.Project(capture.AboveTransfer);
        bool aboveTransferSuccessAdmission = IsExactAboveTransferSuccess(aboveTransferPlan);
        bool aboveTransferUnreachedCapturesClear =
            !aboveTransferPlan.UnreachedCallCellCaptureProvided
            && !aboveTransferPlan.UnreachedCallReturnCaptureProvided;

        if (!aboveTransferSuccessAdmission || !aboveTransferUnreachedCapturesClear)
        {
            bool skippedAboveSecondTransferCapturesClear =
                !capture.AboveSecondTransfer.Receiver4Dword.HasValue
                && !capture.AboveSecondTransfer.FrameMinusCDword.HasValue
                && !capture.AboveSecondTransfer.FrameMinus4Dword.HasValue
                && !capture.AboveSecondTransfer.CallReturnedNormally.HasValue;
            bool skippedReturnGateCapturesClear =
                !capture.AboveReturnGate.HelperReturnEax.HasValue
                && !capture.AboveReturnGate.FrameMinus4.HasValue;
            return StopBeforeAboveSecondTransfer(
                capture,
                aboveTransferPlan,
                aboveTransferSuccessAdmission,
                aboveTransferUnreachedCapturesClear,
                aboveTransferUnreachedCapturesClear
                    ? FrameHandoffCapacityAboveLifecycleOutcome.AboveTransferBlocked
                    : FrameHandoffCapacityAboveLifecycleOutcome.AboveTransferHasUnreachedCaptures,
                aboveSecondTransferPlan: null,
                aboveSecondTransferSuccessAdmission: false,
                skippedAboveSecondTransferCapturesClear,
                skippedReturnGateCapturesClear,
                stopAt: aboveTransferPlan.StopAtRawFileOffsetExclusive);
        }

        FrameHandoffCapacityAboveSecondTransferCapture derivedAboveSecondTransferCapture =
            capture.AboveSecondTransfer with { ValidatedNormalEntry = true };
        FrameHandoffCapacityAboveSecondTransferPlan aboveSecondTransferPlan =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(
                derivedAboveSecondTransferCapture);
        bool aboveSecondTransferSuccessAdmission =
            IsExactAboveSecondTransferSuccess(aboveSecondTransferPlan);
        bool aboveSecondTransferUnreachedCapturesClear =
            !aboveSecondTransferPlan.UnreachedReceiver4CaptureProvided
            && !aboveSecondTransferPlan.UnreachedFrameMinusCCaptureProvided
            && !aboveSecondTransferPlan.UnreachedFrameMinus4CaptureProvided
            && !aboveSecondTransferPlan.UnreachedCallReturnCaptureProvided;

        if (!aboveSecondTransferSuccessAdmission || !aboveSecondTransferUnreachedCapturesClear)
        {
            bool skippedReturnGateCapturesClear =
                !capture.AboveReturnGate.HelperReturnEax.HasValue
                && !capture.AboveReturnGate.FrameMinus4.HasValue;
            return StopBeforeReturnGate(
                capture,
                aboveTransferPlan,
                aboveSecondTransferPlan,
                aboveTransferSuccessAdmission,
                aboveTransferUnreachedCapturesClear,
                aboveSecondTransferSuccessAdmission,
                aboveSecondTransferUnreachedCapturesClear,
                aboveSecondTransferUnreachedCapturesClear
                    ? FrameHandoffCapacityAboveLifecycleOutcome.AboveSecondTransferBlocked
                    : FrameHandoffCapacityAboveLifecycleOutcome
                        .AboveSecondTransferHasUnreachedCaptures,
                returnGateFrameEbpIdentityValidated: false,
                returnGateFrameEbpMatches: false,
                returnGateSuccessAdmission: false,
                skippedReturnGateCapturesClear,
                aboveReturnGatePlan: null,
                stopAt: aboveSecondTransferPlan.StopAtRawFileOffsetExclusive);
        }

        bool returnGateFrameEbpMatches =
            capture.AboveReturnGate.EntryEbp == capture.AboveSecondTransfer.EntryEbp;
        if (!returnGateFrameEbpMatches)
        {
            bool skippedReturnGateCapturesClear =
                !capture.AboveReturnGate.HelperReturnEax.HasValue
                && !capture.AboveReturnGate.FrameMinus4.HasValue;
            return StopBeforeReturnGate(
                capture,
                aboveTransferPlan,
                aboveSecondTransferPlan,
                aboveTransferSuccessAdmission,
                aboveTransferUnreachedCapturesClear,
                aboveSecondTransferSuccessAdmission,
                aboveSecondTransferUnreachedCapturesClear,
                FrameHandoffCapacityAboveLifecycleOutcome.ReturnGateFrameIdentityMismatch,
                returnGateFrameEbpIdentityValidated: true,
                returnGateFrameEbpMatches: false,
                returnGateSuccessAdmission: false,
                skippedReturnGateCapturesClear,
                aboveReturnGatePlan: null,
                stopAt: FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityAboveReturnGateCapture derivedReturnGateCapture =
            capture.AboveReturnGate with { ValidatedNormalEntry = true };
        FrameHandoffCapacityAboveReturnGatePlan aboveReturnGatePlan =
            FrameHandoffCapacityAboveReturnGateProjection.Project(derivedReturnGateCapture);
        bool returnGateSuccessAdmission = IsExactAboveReturnGateSuccess(aboveReturnGatePlan);
        bool returnGateUnreachedCapturesClear =
            !aboveReturnGatePlan.UnreachedHelperReturnEaxCaptureProvided
            && !aboveReturnGatePlan.UnreachedFrameMinus4CaptureProvided;

        FrameHandoffCapacityAboveLifecycleOutcome outcome =
            returnGateSuccessAdmission && returnGateUnreachedCapturesClear
                ? FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
                : returnGateUnreachedCapturesClear
                    ? FrameHandoffCapacityAboveLifecycleOutcome.AboveReturnGateBlocked
                    : FrameHandoffCapacityAboveLifecycleOutcome.AboveReturnGateHasUnreachedCaptures;

        return BuildPlan(
            capture,
            outcome,
            aboveTransferPlan,
            aboveSecondTransferPlan,
            aboveReturnGatePlan,
            aboveTransferSuccessAdmission,
            aboveTransferUnreachedCapturesClear,
            aboveSecondTransferNormalEntryDerived: true,
            aboveSecondTransferSuccessAdmission,
            aboveSecondTransferUnreachedCapturesClear,
            returnGateNormalEntryDerived: true,
            returnGateFrameEbpIdentityValidated: true,
            returnGateFrameEbpMatches,
            returnGateSuccessAdmission,
            returnGateUnreachedCapturesClear,
            UnreachedAboveSecondTransferReceiver4CaptureProvided:
                aboveSecondTransferPlan.UnreachedReceiver4CaptureProvided,
            UnreachedAboveSecondTransferFrameMinusCCaptureProvided:
                aboveSecondTransferPlan.UnreachedFrameMinusCCaptureProvided,
            UnreachedAboveSecondTransferFrameMinus4CaptureProvided:
                aboveSecondTransferPlan.UnreachedFrameMinus4CaptureProvided,
            UnreachedAboveSecondTransferCallReturnCaptureProvided:
                aboveSecondTransferPlan.UnreachedCallReturnCaptureProvided,
            UnreachedAboveReturnGateHelperReturnEaxCaptureProvided:
                aboveReturnGatePlan.UnreachedHelperReturnEaxCaptureProvided,
            UnreachedAboveReturnGateFrameMinus4CaptureProvided:
                aboveReturnGatePlan.UnreachedFrameMinus4CaptureProvided,
            stopAt: aboveReturnGatePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactAboveTransferSuccess(
        FrameHandoffCapacityAboveTransferPlan plan) =>
        plan.Capture.ValidatedBranchEntry
        && plan.Outcome == FrameHandoffCapacityAboveTransferOutcome.ReturnedNormally
        && plan.CallReached
        && plan.CallReturnedNormally == true
        && plan.NormalBoundaryReached
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveTransferProjection.NormalBoundaryRawFileOffsetExclusive;

    private static bool IsExactAboveSecondTransferSuccess(
        FrameHandoffCapacityAboveSecondTransferPlan plan) =>
        plan.Capture.ValidatedNormalEntry
        && plan.Outcome == FrameHandoffCapacityAboveSecondTransferOutcome.ReturnedNormally
        && plan.CallReached
        && plan.CallReturnedNormally == true
        && plan.NormalBoundaryReached
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveSecondTransferProjection
                .NormalBoundaryRawFileOffsetExclusive;

    private static bool IsExactAboveReturnGateSuccess(
        FrameHandoffCapacityAboveReturnGatePlan plan)
    {
        if (!plan.Capture.ValidatedNormalEntry
            || plan.Capture.HelperReturnEax is not uint helperReturnEax
            || plan.Capture.FrameMinus4 is not uint frameMinus4
            || !plan.StackCleanupReached
            || !plan.EdiLeaReached
            || !plan.HelperReturnEaxConsumed
            || !plan.FrameMinus4ReadReached
            || !plan.FrameMinus4Captured
            || !plan.TestReached
            || !plan.BranchReached
            || plan.Reads.Length != 1
            || plan.Reads[0]
                != new FrameHandoffCapacityAboveReturnGateRead(
                    FrameHandoffCapacityAboveReturnGateProjection.FrameMinus4ReadRawFileOffset,
                    FrameHandoffCapacityAboveReturnGateProjection.FrameMinus4Address(
                        plan.Capture.EntryEbp),
                    frameMinus4))
        {
            return false;
        }

        uint expectedEdi =
            FrameHandoffCapacityAboveReturnGateProjection.AddEntryEbxAndHelperReturnEax(
                plan.Capture.EntryEbx,
                helperReturnEax);
        if (plan.EdiAfterLea != expectedEdi
            || plan.EbxAfterFrameMinus4Read != frameMinus4)
        {
            return false;
        }

        return plan.Outcome switch
        {
            FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken =>
                plan.Capture.EntryEsi == 0
                && plan.EntryEsiIsZero == true
                && plan.BranchTaken == true
                && plan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .ZeroBranchDestinationRawFileOffset
                && !plan.EcxMoveReached
                && plan.EcxAfterMove is null,
            FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough =>
                plan.Capture.EntryEsi != 0
                && plan.EntryEsiIsZero == false
                && plan.BranchTaken == false
                && plan.SelectedDestinationRawFileOffset
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && plan.StopAtRawFileOffsetExclusive
                    == FrameHandoffCapacityAboveReturnGateProjection
                        .NormalBoundaryRawFileOffsetExclusive
                && plan.EcxMoveReached
                && plan.EcxAfterMove == plan.Capture.EntryEsi,
            _ => false
        };
    }

    private static FrameHandoffCapacityAboveLifecyclePlan StopBeforeAboveSecondTransfer(
        FrameHandoffCapacityAboveLifecycleCapture capture,
        FrameHandoffCapacityAboveTransferPlan aboveTransferPlan,
        bool aboveTransferSuccessAdmission,
        bool aboveTransferUnreachedCapturesClear,
        FrameHandoffCapacityAboveLifecycleOutcome outcome,
        FrameHandoffCapacityAboveSecondTransferPlan? aboveSecondTransferPlan,
        bool aboveSecondTransferSuccessAdmission,
        bool aboveSecondTransferUnreachedCapturesClear,
        bool returnGateUnreachedCapturesClear,
        uint stopAt) =>
        BuildPlan(
            capture,
            outcome,
            aboveTransferPlan,
            aboveSecondTransferPlan,
            aboveReturnGatePlan: null,
            aboveTransferSuccessAdmission,
            aboveTransferUnreachedCapturesClear,
            aboveSecondTransferNormalEntryDerived: false,
            aboveSecondTransferSuccessAdmission,
            aboveSecondTransferUnreachedCapturesClear,
            returnGateNormalEntryDerived: false,
            returnGateFrameEbpIdentityValidated: false,
            returnGateFrameEbpMatches: false,
            returnGateSuccessAdmission: false,
            returnGateUnreachedCapturesClear,
            UnreachedAboveSecondTransferReceiver4CaptureProvided:
                capture.AboveSecondTransfer.Receiver4Dword.HasValue,
            UnreachedAboveSecondTransferFrameMinusCCaptureProvided:
                capture.AboveSecondTransfer.FrameMinusCDword.HasValue,
            UnreachedAboveSecondTransferFrameMinus4CaptureProvided:
                capture.AboveSecondTransfer.FrameMinus4Dword.HasValue,
            UnreachedAboveSecondTransferCallReturnCaptureProvided:
                capture.AboveSecondTransfer.CallReturnedNormally.HasValue,
            UnreachedAboveReturnGateHelperReturnEaxCaptureProvided:
                capture.AboveReturnGate.HelperReturnEax.HasValue,
            UnreachedAboveReturnGateFrameMinus4CaptureProvided:
                capture.AboveReturnGate.FrameMinus4.HasValue,
            stopAt);

    private static FrameHandoffCapacityAboveLifecyclePlan StopBeforeReturnGate(
        FrameHandoffCapacityAboveLifecycleCapture capture,
        FrameHandoffCapacityAboveTransferPlan aboveTransferPlan,
        FrameHandoffCapacityAboveSecondTransferPlan aboveSecondTransferPlan,
        bool aboveTransferSuccessAdmission,
        bool aboveTransferUnreachedCapturesClear,
        bool aboveSecondTransferSuccessAdmission,
        bool aboveSecondTransferUnreachedCapturesClear,
        FrameHandoffCapacityAboveLifecycleOutcome outcome,
        bool returnGateFrameEbpIdentityValidated,
        bool returnGateFrameEbpMatches,
        bool returnGateSuccessAdmission,
        bool returnGateUnreachedCapturesClear,
        FrameHandoffCapacityAboveReturnGatePlan? aboveReturnGatePlan,
        uint stopAt) =>
        BuildPlan(
            capture,
            outcome,
            aboveTransferPlan,
            aboveSecondTransferPlan,
            aboveReturnGatePlan,
            aboveTransferSuccessAdmission,
            aboveTransferUnreachedCapturesClear,
            aboveSecondTransferNormalEntryDerived: true,
            aboveSecondTransferSuccessAdmission,
            aboveSecondTransferUnreachedCapturesClear,
            returnGateNormalEntryDerived: false,
            returnGateFrameEbpIdentityValidated,
            returnGateFrameEbpMatches,
            returnGateSuccessAdmission,
            returnGateUnreachedCapturesClear,
            UnreachedAboveSecondTransferReceiver4CaptureProvided:
                aboveSecondTransferPlan.UnreachedReceiver4CaptureProvided,
            UnreachedAboveSecondTransferFrameMinusCCaptureProvided:
                aboveSecondTransferPlan.UnreachedFrameMinusCCaptureProvided,
            UnreachedAboveSecondTransferFrameMinus4CaptureProvided:
                aboveSecondTransferPlan.UnreachedFrameMinus4CaptureProvided,
            UnreachedAboveSecondTransferCallReturnCaptureProvided:
                aboveSecondTransferPlan.UnreachedCallReturnCaptureProvided,
            UnreachedAboveReturnGateHelperReturnEaxCaptureProvided:
                capture.AboveReturnGate.HelperReturnEax.HasValue,
            UnreachedAboveReturnGateFrameMinus4CaptureProvided:
                capture.AboveReturnGate.FrameMinus4.HasValue,
            stopAt);

    private static FrameHandoffCapacityAboveLifecyclePlan BuildPlan(
        FrameHandoffCapacityAboveLifecycleCapture capture,
        FrameHandoffCapacityAboveLifecycleOutcome outcome,
        FrameHandoffCapacityAboveTransferPlan aboveTransferPlan,
        FrameHandoffCapacityAboveSecondTransferPlan? aboveSecondTransferPlan,
        FrameHandoffCapacityAboveReturnGatePlan? aboveReturnGatePlan,
        bool aboveTransferSuccessAdmission,
        bool aboveTransferUnreachedCapturesClear,
        bool aboveSecondTransferNormalEntryDerived,
        bool aboveSecondTransferSuccessAdmission,
        bool aboveSecondTransferUnreachedCapturesClear,
        bool returnGateNormalEntryDerived,
        bool returnGateFrameEbpIdentityValidated,
        bool returnGateFrameEbpMatches,
        bool returnGateSuccessAdmission,
        bool returnGateUnreachedCapturesClear,
        bool UnreachedAboveSecondTransferReceiver4CaptureProvided,
        bool UnreachedAboveSecondTransferFrameMinusCCaptureProvided,
        bool UnreachedAboveSecondTransferFrameMinus4CaptureProvided,
        bool UnreachedAboveSecondTransferCallReturnCaptureProvided,
        bool UnreachedAboveReturnGateHelperReturnEaxCaptureProvided,
        bool UnreachedAboveReturnGateFrameMinus4CaptureProvided,
        uint stopAt) =>
        new(
            capture,
            outcome,
            aboveTransferPlan,
            aboveSecondTransferPlan,
            aboveReturnGatePlan,
            AboveTransferAdmissionChecked: true,
            aboveTransferSuccessAdmission,
            aboveTransferUnreachedCapturesClear,
            aboveSecondTransferNormalEntryDerived,
            aboveSecondTransferSuccessAdmission,
            aboveSecondTransferUnreachedCapturesClear,
            returnGateNormalEntryDerived,
            returnGateFrameEbpIdentityValidated,
            returnGateFrameEbpMatches,
            returnGateSuccessAdmission,
            returnGateUnreachedCapturesClear,
            UnreachedAboveSecondTransferReceiver4CaptureProvided,
            UnreachedAboveSecondTransferFrameMinusCCaptureProvided,
            UnreachedAboveSecondTransferFrameMinus4CaptureProvided,
            UnreachedAboveSecondTransferCallReturnCaptureProvided,
            UnreachedAboveReturnGateHelperReturnEaxCaptureProvided,
            UnreachedAboveReturnGateFrameMinus4CaptureProvided,
            stopAt);
}
