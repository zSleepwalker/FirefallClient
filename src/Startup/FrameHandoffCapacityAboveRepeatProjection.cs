namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveRepeatCapture(
    FrameHandoffCapacityAboveReturnGateCapture AboveReturnGate,
    uint? RepeatEntryEcx,
    uint? RepeatEntryEdi,
    bool? DirectionFlagSet,
    bool? RepCompletedNormally);

public enum FrameHandoffCapacityAboveRepeatOutcome
{
    GateBlocked,
    GateHasUnreachedCaptures,
    ZeroBranchTaken,
    ZeroBranchHasRepeatCaptures,
    RepeatEntryIdentityMismatch,
    SharedRepeatIncomplete,
    SharedRepeatHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityAboveRepeatPlan(
    FrameHandoffCapacityAboveRepeatCapture Capture,
    FrameHandoffCapacityAboveRepeatOutcome Outcome,
    FrameHandoffCapacityAboveReturnGatePlan AboveReturnGatePlan,
    FrameHandoffCapacitySharedRepeatPlan? SharedRepeatPlan,
    bool GateAdmissionChecked,
    bool GateSuccessAdmission,
    bool GateUnreachedCapturesClear,
    bool UnreachedGateHelperReturnEaxCaptureProvided,
    bool UnreachedGateFrameMinus4CaptureProvided,
    bool ZeroBranchBypassedRepeat,
    bool RepeatEntryIdentityChecked,
    bool? RepeatEcxIdentityMatches,
    bool? RepeatEdiIdentityMatches,
    bool SharedRepeatNormalEntryDerived,
    bool SharedRepeatCompletionAdmission,
    bool SharedRepeatUnreachedCapturesClear,
    bool UnreachedRepeatEntryEcxCaptureProvided,
    bool UnreachedRepeatEntryEdiCaptureProvided,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the return gate with its shared XOR EAX / REP STOSD continuation.
/// </summary>
public static class FrameHandoffCapacityAboveRepeatProjection
{
    public const uint EntryRawFileOffset =
        FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset;
    public const uint SharedRepeatRawFileOffset =
        FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive;
    public const uint ZeroBranchDestinationRawFileOffset =
        FrameHandoffCapacityAboveReturnGateProjection.ZeroBranchDestinationRawFileOffset;

    public static FrameHandoffCapacityAboveRepeatPlan Project(
        FrameHandoffCapacityAboveRepeatCapture capture)
    {
        FrameHandoffCapacityAboveReturnGatePlan gatePlan =
            FrameHandoffCapacityAboveReturnGateProjection.Project(capture.AboveReturnGate);
        bool gateUnreachedCapturesClear =
            !gatePlan.UnreachedHelperReturnEaxCaptureProvided
            && !gatePlan.UnreachedFrameMinus4CaptureProvided;

        if (IsExactZeroBranch(gatePlan))
        {
            bool repeatCapturesClear = !HasRepeatCaptures(capture);
            return BuildPlan(
                capture,
                repeatCapturesClear
                    ? FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
                    : FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchHasRepeatCaptures,
                gatePlan,
                gateSuccessAdmission: true,
                gateUnreachedCapturesClear: gateUnreachedCapturesClear,
                zeroBranchBypassedRepeat: true,
                repeatEntryIdentityChecked: false,
                sharedRepeatUnreachedCapturesClear: repeatCapturesClear,
                unreachedRepeatEntryEcxCaptureProvided: capture.RepeatEntryEcx.HasValue,
                unreachedRepeatEntryEdiCaptureProvided: capture.RepeatEntryEdi.HasValue,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepCompletedNormally.HasValue,
                stopAt: ZeroBranchDestinationRawFileOffset);
        }

        if (!IsExactNonzeroFallThrough(gatePlan) || !gateUnreachedCapturesClear)
        {
            bool repeatCapturesClear = !HasRepeatCaptures(capture);
            return BuildPlan(
                capture,
                gateUnreachedCapturesClear && repeatCapturesClear
                    ? FrameHandoffCapacityAboveRepeatOutcome.GateBlocked
                    : FrameHandoffCapacityAboveRepeatOutcome.GateHasUnreachedCaptures,
                gatePlan,
                gateSuccessAdmission: false,
                gateUnreachedCapturesClear: gateUnreachedCapturesClear,
                sharedRepeatUnreachedCapturesClear: repeatCapturesClear,
                unreachedRepeatEntryEcxCaptureProvided: capture.RepeatEntryEcx.HasValue,
                unreachedRepeatEntryEdiCaptureProvided: capture.RepeatEntryEdi.HasValue,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepCompletedNormally.HasValue,
                stopAt: gatePlan.StopAtRawFileOffsetExclusive);
        }

        bool repeatEcxIdentityMatches =
            capture.RepeatEntryEcx is not uint repeatEntryEcx
            || repeatEntryEcx == gatePlan.EcxAfterMove;
        bool repeatEdiIdentityMatches =
            capture.RepeatEntryEdi is not uint repeatEntryEdi
            || repeatEntryEdi == gatePlan.EdiAfterLea;
        if (!repeatEcxIdentityMatches || !repeatEdiIdentityMatches)
        {
            return StopBeforeSharedRepeat(
                capture,
                gatePlan,
                FrameHandoffCapacityAboveRepeatOutcome.RepeatEntryIdentityMismatch,
                gateUnreachedCapturesClear,
                repeatEntryIdentityChecked: true,
                repeatEcxIdentityMatches: repeatEcxIdentityMatches,
                repeatEdiIdentityMatches: repeatEdiIdentityMatches);
        }

        FrameHandoffCapacitySharedRepeatCapture derivedRepeatCapture = new(
            EntryValidatedNonzero: true,
            EntryEcx: gatePlan.EcxAfterMove!.Value,
            EntryEdi: gatePlan.EdiAfterLea!.Value,
            DirectionFlagSet: capture.DirectionFlagSet,
            RepCompletedNormally: capture.RepCompletedNormally);
        FrameHandoffCapacitySharedRepeatPlan repeatPlan =
            FrameHandoffCapacitySharedRepeatProjection.Project(derivedRepeatCapture);
        bool repeatUnreachedCapturesClear =
            !repeatPlan.UnreachedDirectionFlagCaptureProvided
            && !repeatPlan.UnreachedRepCompletionCaptureProvided;
        bool repeatCompletedExactly =
            repeatPlan.Outcome == FrameHandoffCapacitySharedRepeatOutcome.Completed
            && repeatPlan.EntryValidationAccepted
            && repeatPlan.XorEaxReached
            && repeatPlan.EaxValue == 0
            && repeatPlan.RepInstructionReached
            && repeatPlan.AllStoresCompleted
            && repeatPlan.StoreDescriptor.HasValue
            && repeatPlan.EcxAfterRep == 0
            && repeatPlan.EdiAfterRep.HasValue
            && repeatPlan.StopAtRawFileOffsetExclusive == EndRawFileOffsetExclusive;
        bool repeatCompletionAdmission =
            repeatCompletedExactly && repeatUnreachedCapturesClear;

        return BuildPlan(
            capture,
            repeatCompletionAdmission
                ? FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
                : repeatUnreachedCapturesClear
                    ? FrameHandoffCapacityAboveRepeatOutcome.SharedRepeatIncomplete
                    : FrameHandoffCapacityAboveRepeatOutcome.SharedRepeatHasUnreachedCaptures,
            gatePlan,
            repeatPlan,
            gateSuccessAdmission: true,
            gateUnreachedCapturesClear: gateUnreachedCapturesClear,
            repeatEntryIdentityChecked: true,
            repeatEcxIdentityMatches: true,
            repeatEdiIdentityMatches: true,
            sharedRepeatNormalEntryDerived: true,
            sharedRepeatCompletionAdmission: repeatCompletionAdmission,
            sharedRepeatUnreachedCapturesClear: repeatUnreachedCapturesClear,
            unreachedDirectionFlagCaptureProvided:
                repeatPlan.UnreachedDirectionFlagCaptureProvided,
            unreachedRepCompletionCaptureProvided:
                repeatPlan.UnreachedRepCompletionCaptureProvided,
            stopAt: repeatPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactZeroBranch(
        FrameHandoffCapacityAboveReturnGatePlan plan) =>
        plan.Capture.ValidatedNormalEntry
        && plan.Outcome == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
        && plan.StackCleanupReached
        && plan.EdiLeaReached
        && plan.EdiAfterLea.HasValue
        && plan.HelperReturnEaxConsumed
        && plan.FrameMinus4ReadReached
        && plan.FrameMinus4Captured
        && plan.TestReached
        && plan.EntryEsiIsZero == true
        && plan.BranchReached
        && plan.BranchTaken == true
        && !plan.EcxMoveReached
        && plan.EcxAfterMove is null
        && plan.SelectedDestinationRawFileOffset == ZeroBranchDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == ZeroBranchDestinationRawFileOffset;

    private static bool IsExactNonzeroFallThrough(
        FrameHandoffCapacityAboveReturnGatePlan plan) =>
        plan.Capture.ValidatedNormalEntry
        && plan.Outcome == FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough
        && plan.StackCleanupReached
        && plan.EdiLeaReached
        && plan.EdiAfterLea.HasValue
        && plan.HelperReturnEaxConsumed
        && plan.FrameMinus4ReadReached
        && plan.FrameMinus4Captured
        && plan.TestReached
        && plan.EntryEsiIsZero == false
        && plan.BranchReached
        && plan.BranchTaken == false
        && plan.EcxMoveReached
        && plan.EcxAfterMove.HasValue
        && plan.SelectedDestinationRawFileOffset == SharedRepeatRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == SharedRepeatRawFileOffset;

    private static bool HasRepeatCaptures(FrameHandoffCapacityAboveRepeatCapture capture) =>
        capture.RepeatEntryEcx.HasValue
        || capture.RepeatEntryEdi.HasValue
        || capture.DirectionFlagSet.HasValue
        || capture.RepCompletedNormally.HasValue;

    private static FrameHandoffCapacityAboveRepeatPlan StopBeforeSharedRepeat(
        FrameHandoffCapacityAboveRepeatCapture capture,
        FrameHandoffCapacityAboveReturnGatePlan gatePlan,
        FrameHandoffCapacityAboveRepeatOutcome outcome,
        bool gateUnreachedCapturesClear,
        bool repeatEntryIdentityChecked = false,
        bool? repeatEcxIdentityMatches = null,
        bool? repeatEdiIdentityMatches = null) =>
        BuildPlan(
            capture,
            outcome,
            gatePlan,
            gateSuccessAdmission: true,
            gateUnreachedCapturesClear: gateUnreachedCapturesClear,
            repeatEntryIdentityChecked: repeatEntryIdentityChecked,
            repeatEcxIdentityMatches: repeatEcxIdentityMatches,
            repeatEdiIdentityMatches: repeatEdiIdentityMatches,
            sharedRepeatUnreachedCapturesClear:
                !capture.DirectionFlagSet.HasValue
                && !capture.RepCompletedNormally.HasValue,
            unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
            unreachedRepCompletionCaptureProvided: capture.RepCompletedNormally.HasValue,
            stopAt: SharedRepeatRawFileOffset);

    private static FrameHandoffCapacityAboveRepeatPlan BuildPlan(
        FrameHandoffCapacityAboveRepeatCapture capture,
        FrameHandoffCapacityAboveRepeatOutcome outcome,
        FrameHandoffCapacityAboveReturnGatePlan gatePlan,
        FrameHandoffCapacitySharedRepeatPlan? repeatPlan = null,
        bool gateSuccessAdmission = false,
        bool gateUnreachedCapturesClear = false,
        bool zeroBranchBypassedRepeat = false,
        bool repeatEntryIdentityChecked = false,
        bool? repeatEcxIdentityMatches = null,
        bool? repeatEdiIdentityMatches = null,
        bool sharedRepeatNormalEntryDerived = false,
        bool sharedRepeatCompletionAdmission = false,
        bool sharedRepeatUnreachedCapturesClear = false,
        bool unreachedRepeatEntryEcxCaptureProvided = false,
        bool unreachedRepeatEntryEdiCaptureProvided = false,
        bool unreachedDirectionFlagCaptureProvided = false,
        bool unreachedRepCompletionCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            gatePlan,
            repeatPlan,
            GateAdmissionChecked: true,
            GateSuccessAdmission: gateSuccessAdmission,
            GateUnreachedCapturesClear: gateUnreachedCapturesClear,
            UnreachedGateHelperReturnEaxCaptureProvided:
                gatePlan.UnreachedHelperReturnEaxCaptureProvided,
            UnreachedGateFrameMinus4CaptureProvided:
                gatePlan.UnreachedFrameMinus4CaptureProvided,
            ZeroBranchBypassedRepeat: zeroBranchBypassedRepeat,
            RepeatEntryIdentityChecked: repeatEntryIdentityChecked,
            RepeatEcxIdentityMatches: repeatEcxIdentityMatches,
            RepeatEdiIdentityMatches: repeatEdiIdentityMatches,
            SharedRepeatNormalEntryDerived: sharedRepeatNormalEntryDerived,
            SharedRepeatCompletionAdmission: sharedRepeatCompletionAdmission,
            SharedRepeatUnreachedCapturesClear: sharedRepeatUnreachedCapturesClear,
            UnreachedRepeatEntryEcxCaptureProvided:
                unreachedRepeatEntryEcxCaptureProvided,
            UnreachedRepeatEntryEdiCaptureProvided:
                unreachedRepeatEntryEdiCaptureProvided,
            UnreachedDirectionFlagCaptureProvided:
                unreachedDirectionFlagCaptureProvided,
            UnreachedRepCompletionCaptureProvided:
                unreachedRepCompletionCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
}
