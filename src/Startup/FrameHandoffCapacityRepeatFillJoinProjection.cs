namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityRepeatFillJoinCapture(
    FrameHandoffCapacityGateRepeatCapture GateRepeatCapture,
    uint FillJoinEntryEbp,
    uint? FillJoinEntryEdi,
    uint? FrameMinus4);

public enum FrameHandoffCapacityRepeatFillJoinOutcome
{
    GateRepeatBlocked,
    GateRepeatHasUnreachedCaptures,
    GateRepeatNotAdmitted,
    MissingZeroBranchEntryEdi,
    FillJoinEntryEbpMismatch,
    FillJoinEntryEdiMismatch,
    FillJoinBlocked,
    ZeroEntryEdiBranchToRawFileOffsetBE6CA2,
    NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
}

public readonly record struct FrameHandoffCapacityRepeatFillJoinPlan(
    FrameHandoffCapacityRepeatFillJoinCapture Capture,
    FrameHandoffCapacityGateRepeatPlan GateRepeatPlan,
    FrameHandoffCapacityRepeatFillJoinOutcome Outcome,
    bool GateRepeatAdmissionValidated,
    bool NonzeroRepeatAdmission,
    bool ZeroBranchBypassAdmission,
    uint? DerivedFillJoinEntryEbp,
    bool? FillJoinEntryEbpMatches,
    uint? DerivedFillJoinEntryEdi,
    bool? ProvidedFillJoinEntryEdiMatches,
    bool FillJoinAdmissionValidated,
    FrameHandoffCapacityFillJoinPlan? FillJoinPlan,
    bool UnreachedFillJoinEntryEdiCaptureProvided,
    bool UnreachedFillJoinFrameMinus4ReadCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactFillJoinOutcome =>
        Outcome is FrameHandoffCapacityRepeatFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            or FrameHandoffCapacityRepeatFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
        && FillJoinPlan is FrameHandoffCapacityFillJoinPlan fillJoinPlan
        && fillJoinPlan.Outcome is
            FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            or FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E;

    public bool NoUnreachedFillJoinCaptures =>
        !UnreachedFillJoinEntryEdiCaptureProvided
        && !UnreachedFillJoinFrameMinus4ReadCaptureProvided
        && FillJoinPlan is FrameHandoffCapacityFillJoinPlan fillJoinPlan
        && !fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided;
}

/// <summary>
/// Composes the admitted REP path or clean zero bypass with the validated fill join.
/// </summary>
public static class FrameHandoffCapacityRepeatFillJoinProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityGateRepeatProjection.LifecycleStartRawFileOffset;
    public const uint FillJoinEntryRawFileOffset =
        FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityFillJoinProjection.EndRawFileOffsetExclusive;

    public static FrameHandoffCapacityRepeatFillJoinPlan Project(
        FrameHandoffCapacityRepeatFillJoinCapture capture)
    {
        FrameHandoffCapacityGateRepeatPlan gateRepeatPlan =
            FrameHandoffCapacityGateRepeatProjection.Project(capture.GateRepeatCapture);

        bool unreachedEntryEdiProvided =
            capture.FillJoinEntryEdi.HasValue;
        bool unreachedFrameMinus4Provided =
            capture.FrameMinus4.HasValue;

        bool nonzeroRepeatAdmission =
            gateRepeatPlan.Outcome == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && gateRepeatPlan.NonzeroGateAdmission
            && !gateRepeatPlan.ZeroJoinAdmission
            && gateRepeatPlan.GateUnreachedCapturesClear
            && gateRepeatPlan.RepeatPlan is FrameHandoffCapacityRepeatStorePlan repeatPlan
            && repeatPlan.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && repeatPlan.AllStoresCompleted
            && repeatPlan.FreshFrameMinus8Value is uint freshFrameMinus8Value
            && !gateRepeatPlan.UnreachedRepeatEntryEcxProvided
            && !gateRepeatPlan.UnreachedRepeatEntryEdxProvided
            && !gateRepeatPlan.UnreachedRepeatEntryEbpProvided
            && !gateRepeatPlan.UnreachedDirectionFlagCaptureProvided
            && !gateRepeatPlan.UnreachedRepCompletionCaptureProvided
            && !gateRepeatPlan.UnreachedFreshFrameMinus8CaptureProvided;

        bool zeroBranchBypassAdmission =
            gateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && gateRepeatPlan.ZeroJoinAdmission
            && !gateRepeatPlan.NonzeroGateAdmission
            && gateRepeatPlan.GateUnreachedCapturesClear
            && gateRepeatPlan.RepeatPlan is null
            && !gateRepeatPlan.UnreachedRepeatEntryEcxProvided
            && !gateRepeatPlan.UnreachedRepeatEntryEdxProvided
            && !gateRepeatPlan.UnreachedRepeatEntryEbpProvided
            && !gateRepeatPlan.UnreachedDirectionFlagCaptureProvided
            && !gateRepeatPlan.UnreachedRepCompletionCaptureProvided
            && !gateRepeatPlan.UnreachedFreshFrameMinus8CaptureProvided;

        if (!nonzeroRepeatAdmission && !zeroBranchBypassAdmission)
        {
            FrameHandoffCapacityRepeatFillJoinOutcome blockedOutcome =
                gateRepeatPlan.Outcome
                    == FrameHandoffCapacityGateRepeatOutcome.RepeatHasUnreachedCaptures
                    || gateRepeatPlan.Outcome
                        == FrameHandoffCapacityGateRepeatOutcome.GateHasUnreachedCaptures
                    || !gateRepeatPlan.GateUnreachedCapturesClear
                    ? FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatHasUnreachedCaptures
                    : gateRepeatPlan.Outcome
                        == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
                        || gateRepeatPlan.Outcome
                            == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
                        ? FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatNotAdmitted
                        : FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatBlocked;

            return Stop(
                capture,
                gateRepeatPlan,
                blockedOutcome,
                unreachedFillJoinEntryEdiCaptureProvided: unreachedEntryEdiProvided,
                unreachedFillJoinFrameMinus4ReadCaptureProvided:
                    unreachedFrameMinus4Provided,
                stopAt: gateRepeatPlan.StopAtRawFileOffsetExclusive);
        }

        if (gateRepeatPlan.Capture.GatePlan is not FrameHandoffCapacitySecondReturnGatePlan gatePlan)
        {
            return Stop(
                capture,
                gateRepeatPlan,
                FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatNotAdmitted,
                unreachedFillJoinEntryEdiCaptureProvided: unreachedEntryEdiProvided,
                unreachedFillJoinFrameMinus4ReadCaptureProvided:
                    unreachedFrameMinus4Provided,
                stopAt: gateRepeatPlan.StopAtRawFileOffsetExclusive);
        }

        uint derivedEntryEbp = gatePlan.Capture.EntryEbp;
        bool entryEbpMatches = capture.FillJoinEntryEbp == derivedEntryEbp;
        uint? derivedEntryEdi = null;
        bool? providedEntryEdiMatches = null;

        if (nonzeroRepeatAdmission)
        {
            if (gateRepeatPlan.RepeatPlan is not FrameHandoffCapacityRepeatStorePlan availableRepeatPlan
                || availableRepeatPlan.FreshFrameMinus8Value is not uint availableFreshFrameMinus8Value)
            {
                return Stop(
                    capture,
                    gateRepeatPlan,
                    FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatNotAdmitted,
                    unreachedFillJoinEntryEdiCaptureProvided: unreachedEntryEdiProvided,
                    unreachedFillJoinFrameMinus4ReadCaptureProvided:
                        unreachedFrameMinus4Provided,
                    stopAt: gateRepeatPlan.StopAtRawFileOffsetExclusive);
            }

            derivedEntryEdi = availableFreshFrameMinus8Value;
            providedEntryEdiMatches = capture.FillJoinEntryEdi is uint providedEntryEdi
                ? providedEntryEdi == availableFreshFrameMinus8Value
                : null;
        }
        else if (capture.FillJoinEntryEdi is uint zeroBranchEntryEdi)
        {
            derivedEntryEdi = zeroBranchEntryEdi;
        }

        if (!entryEbpMatches)
        {
            return Stop(
                capture,
                gateRepeatPlan,
                FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEbpMismatch,
                gateRepeatAdmissionValidated: true,
                nonzeroRepeatAdmission: nonzeroRepeatAdmission,
                zeroBranchBypassAdmission: zeroBranchBypassAdmission,
                derivedFillJoinEntryEbp: derivedEntryEbp,
                fillJoinEntryEbpMatches: false,
                derivedFillJoinEntryEdi: derivedEntryEdi,
                providedFillJoinEntryEdiMatches: providedEntryEdiMatches,
                unreachedFillJoinEntryEdiCaptureProvided:
                    !nonzeroRepeatAdmission && unreachedEntryEdiProvided,
                unreachedFillJoinFrameMinus4ReadCaptureProvided:
                    unreachedFrameMinus4Provided,
                stopAt: FillJoinEntryRawFileOffset);
        }

        if (!nonzeroRepeatAdmission && derivedEntryEdi is null)
        {
            return Stop(
                capture,
                gateRepeatPlan,
                FrameHandoffCapacityRepeatFillJoinOutcome.MissingZeroBranchEntryEdi,
                gateRepeatAdmissionValidated: true,
                zeroBranchBypassAdmission: true,
                derivedFillJoinEntryEbp: derivedEntryEbp,
                fillJoinEntryEbpMatches: true,
                unreachedFillJoinFrameMinus4ReadCaptureProvided:
                    unreachedFrameMinus4Provided,
                stopAt: FillJoinEntryRawFileOffset);
        }

        if (providedEntryEdiMatches == false)
        {
            return Stop(
                capture,
                gateRepeatPlan,
                FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEdiMismatch,
                gateRepeatAdmissionValidated: true,
                nonzeroRepeatAdmission: true,
                derivedFillJoinEntryEbp: derivedEntryEbp,
                fillJoinEntryEbpMatches: true,
                derivedFillJoinEntryEdi: derivedEntryEdi,
                providedFillJoinEntryEdiMatches: false,
                unreachedFillJoinFrameMinus4ReadCaptureProvided:
                    unreachedFrameMinus4Provided,
                stopAt: FillJoinEntryRawFileOffset);
        }

        var fillJoinCapture = new FrameHandoffCapacityFillJoinCapture(
            CaptureValidatedJoin: true,
            EntryEbp: derivedEntryEbp,
            EntryEdi: derivedEntryEdi!.Value,
            FrameMinus4: capture.FrameMinus4);
        FrameHandoffCapacityFillJoinPlan fillJoinPlan =
            FrameHandoffCapacityFillJoinProjection.Project(fillJoinCapture);
        bool exactZeroRoute =
            fillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && fillJoinPlan.FrameMinus4ReadReached
            && fillJoinPlan.FrameMinus4ReadCaptured
            && fillJoinPlan.EntryEdiZeroFlag == true
            && fillJoinPlan.ZeroBranchReached
            && fillJoinPlan.ZeroBranchTaken == true
            && fillJoinPlan.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && !fillJoinPlan.EcxEntryEdiMoveReached
            && !fillJoinPlan.EdiLoadedEbxMoveReached
            && !fillJoinPlan.JumpReached
            && !fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided;
        bool exactNonzeroRoute =
            fillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && fillJoinPlan.FrameMinus4ReadReached
            && fillJoinPlan.FrameMinus4ReadCaptured
            && fillJoinPlan.EntryEdiZeroFlag == false
            && fillJoinPlan.ZeroBranchReached
            && fillJoinPlan.ZeroBranchTaken == false
            && fillJoinPlan.EcxEntryEdiMoveReached
            && fillJoinPlan.EcxAfterMove == derivedEntryEdi
            && fillJoinPlan.EdiLoadedEbxMoveReached
            && fillJoinPlan.EdiAfterMove == fillJoinPlan.EbxAfterFrameMinus4Read
            && fillJoinPlan.JumpReached
            && fillJoinPlan.JumpTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
            && !fillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided;

        bool exactFillJoin = exactZeroRoute || exactNonzeroRoute;
        return new(
            capture,
            gateRepeatPlan,
            exactFillJoin
                ? exactZeroRoute
                    ? FrameHandoffCapacityRepeatFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
                    : FrameHandoffCapacityRepeatFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
                : FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinBlocked,
            GateRepeatAdmissionValidated: true,
            NonzeroRepeatAdmission: nonzeroRepeatAdmission,
            ZeroBranchBypassAdmission: zeroBranchBypassAdmission,
            DerivedFillJoinEntryEbp: derivedEntryEbp,
            FillJoinEntryEbpMatches: true,
            DerivedFillJoinEntryEdi: derivedEntryEdi,
            ProvidedFillJoinEntryEdiMatches: providedEntryEdiMatches,
            FillJoinAdmissionValidated: true,
            fillJoinPlan,
            UnreachedFillJoinEntryEdiCaptureProvided: false,
            UnreachedFillJoinFrameMinus4ReadCaptureProvided: false,
            fillJoinPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityRepeatFillJoinPlan Stop(
        FrameHandoffCapacityRepeatFillJoinCapture capture,
        FrameHandoffCapacityGateRepeatPlan gateRepeatPlan,
        FrameHandoffCapacityRepeatFillJoinOutcome outcome,
        bool gateRepeatAdmissionValidated = false,
        bool nonzeroRepeatAdmission = false,
        bool zeroBranchBypassAdmission = false,
        uint? derivedFillJoinEntryEbp = null,
        bool? fillJoinEntryEbpMatches = null,
        uint? derivedFillJoinEntryEdi = null,
        bool? providedFillJoinEntryEdiMatches = null,
        bool fillJoinAdmissionValidated = false,
        FrameHandoffCapacityFillJoinPlan? fillJoinPlan = null,
        bool unreachedFillJoinEntryEdiCaptureProvided = false,
        bool unreachedFillJoinFrameMinus4ReadCaptureProvided = false,
        uint stopAt = LifecycleStartRawFileOffset) =>
        new(
            capture,
            gateRepeatPlan,
            outcome,
            gateRepeatAdmissionValidated,
            nonzeroRepeatAdmission,
            zeroBranchBypassAdmission,
            derivedFillJoinEntryEbp,
            fillJoinEntryEbpMatches,
            derivedFillJoinEntryEdi,
            providedFillJoinEntryEdiMatches,
            fillJoinAdmissionValidated,
            fillJoinPlan,
            unreachedFillJoinEntryEdiCaptureProvided,
            unreachedFillJoinFrameMinus4ReadCaptureProvided,
            stopAt);
}
