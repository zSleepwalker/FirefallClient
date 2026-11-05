namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFillSharedRepeatCapture(
    FrameHandoffCapacityFillJoinCapture FillJoinCapture,
    uint? ExpectedRepeatEntryEcx = null,
    uint? ExpectedRepeatEntryEdi = null,
    bool? DirectionFlagSet = null,
    bool? RepCompletedNormally = null);

public enum FrameHandoffCapacityFillSharedRepeatOutcome
{
    FillJoinNotValidated,
    MissingFrameMinus4Read,
    ZeroEntryEdiBranchToRawFileOffsetBE6CA2,
    FillJoinOutcomeNotAccepted,
    RepeatEntryIdentityMismatch,
    MissingDirectionFlagCapture,
    MissingRepCompletionCapture,
    RepDidNotCompleteNormally,
    Completed
}

public readonly record struct FrameHandoffCapacityFillSharedRepeatPlan(
    FrameHandoffCapacityFillSharedRepeatCapture Capture,
    FrameHandoffCapacityFillJoinPlan FillJoinPlan,
    FrameHandoffCapacityFillSharedRepeatOutcome Outcome,
    bool RepeatEntryIdentityValidationReached,
    uint? RepeatEntryEcx,
    uint? RepeatEntryEdi,
    bool? ExpectedRepeatEntryEcxMatched,
    bool? ExpectedRepeatEntryEdiMatched,
    bool RepeatEntryValidatedNonzero,
    FrameHandoffCapacitySharedRepeatPlan? RepeatPlan,
    bool UnreachedExpectedRepeatEntryEcxCaptureProvided,
    bool UnreachedExpectedRepeatEntryEdiCaptureProvided,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the validated fill join with its shared XOR EAX / REP STOSD route.
/// </summary>
public static class FrameHandoffCapacityFillSharedRepeatProjection
{
    public const uint StopAtRawFileOffsetExclusive =
        FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive;

    public static FrameHandoffCapacityFillSharedRepeatPlan Project(
        FrameHandoffCapacityFillSharedRepeatCapture capture)
    {
        FrameHandoffCapacityFillJoinPlan fillJoinPlan =
            FrameHandoffCapacityFillJoinProjection.Project(capture.FillJoinCapture);

        if (fillJoinPlan.Outcome
            == FrameHandoffCapacityFillJoinOutcome.JoinNotValidated)
        {
            return Stop(
                capture,
                fillJoinPlan,
                FrameHandoffCapacityFillSharedRepeatOutcome.FillJoinNotValidated,
                unreachedExpectedRepeatEntryEcx:
                    capture.ExpectedRepeatEntryEcx.HasValue,
                unreachedExpectedRepeatEntryEdi:
                    capture.ExpectedRepeatEntryEdi.HasValue,
                unreachedDirectionFlag:
                    capture.DirectionFlagSet.HasValue,
                unreachedRepCompletion:
                    capture.RepCompletedNormally.HasValue,
                stopAt: fillJoinPlan.StopAtRawFileOffsetExclusive);
        }

        if (fillJoinPlan.Outcome
            == FrameHandoffCapacityFillJoinOutcome.MissingFrameMinus4Read)
        {
            return Stop(
                capture,
                fillJoinPlan,
                FrameHandoffCapacityFillSharedRepeatOutcome.MissingFrameMinus4Read,
                unreachedExpectedRepeatEntryEcx:
                    capture.ExpectedRepeatEntryEcx.HasValue,
                unreachedExpectedRepeatEntryEdi:
                    capture.ExpectedRepeatEntryEdi.HasValue,
                unreachedDirectionFlag:
                    capture.DirectionFlagSet.HasValue,
                unreachedRepCompletion:
                    capture.RepCompletedNormally.HasValue,
                stopAt: fillJoinPlan.StopAtRawFileOffsetExclusive);
        }

        if (fillJoinPlan.Outcome
            == FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2)
        {
            return Stop(
                capture,
                fillJoinPlan,
                FrameHandoffCapacityFillSharedRepeatOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2,
                unreachedExpectedRepeatEntryEcx:
                    capture.ExpectedRepeatEntryEcx.HasValue,
                unreachedExpectedRepeatEntryEdi:
                    capture.ExpectedRepeatEntryEdi.HasValue,
                unreachedDirectionFlag:
                    capture.DirectionFlagSet.HasValue,
                unreachedRepCompletion:
                    capture.RepCompletedNormally.HasValue,
                stopAt: FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset);
        }

        if (fillJoinPlan.Outcome
                != FrameHandoffCapacityFillJoinOutcome
                    .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            || !fillJoinPlan.EntryEdiTestReached
            || fillJoinPlan.EntryEdiZeroFlag != false
            || fillJoinPlan.ZeroBranchTaken != false
            || fillJoinPlan.BranchTargetRawFileOffset is not null
            || !fillJoinPlan.EcxEntryEdiMoveReached
            || fillJoinPlan.EcxAfterMove is not uint repeatEntryEcx
            || repeatEntryEcx == 0
            || !fillJoinPlan.EdiLoadedEbxMoveReached
            || fillJoinPlan.EdiAfterMove is not uint repeatEntryEdi
            || !fillJoinPlan.JumpReached
            || fillJoinPlan.JumpTargetRawFileOffset
                != FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset)
        {
            return Stop(
                capture,
                fillJoinPlan,
                FrameHandoffCapacityFillSharedRepeatOutcome.FillJoinOutcomeNotAccepted,
                unreachedExpectedRepeatEntryEcx:
                    capture.ExpectedRepeatEntryEcx.HasValue,
                unreachedExpectedRepeatEntryEdi:
                    capture.ExpectedRepeatEntryEdi.HasValue,
                unreachedDirectionFlag:
                    capture.DirectionFlagSet.HasValue,
                unreachedRepCompletion:
                    capture.RepCompletedNormally.HasValue,
                stopAt: fillJoinPlan.StopAtRawFileOffsetExclusive);
        }

        bool? expectedRepeatEntryEcxMatched =
            capture.ExpectedRepeatEntryEcx is uint expectedEcx
                ? repeatEntryEcx == expectedEcx
                : null;
        bool? expectedRepeatEntryEdiMatched =
            capture.ExpectedRepeatEntryEdi is uint expectedEdi
                ? repeatEntryEdi == expectedEdi
                : null;

        if (expectedRepeatEntryEcxMatched == false
            || expectedRepeatEntryEdiMatched == false)
        {
            return Stop(
                capture,
                fillJoinPlan,
                FrameHandoffCapacityFillSharedRepeatOutcome.RepeatEntryIdentityMismatch,
                repeatEntryIdentityValidationReached: true,
                repeatEntryEcx: repeatEntryEcx,
                repeatEntryEdi: repeatEntryEdi,
                expectedRepeatEntryEcxMatched: expectedRepeatEntryEcxMatched,
                expectedRepeatEntryEdiMatched: expectedRepeatEntryEdiMatched,
                unreachedDirectionFlag:
                    capture.DirectionFlagSet.HasValue,
                unreachedRepCompletion:
                    capture.RepCompletedNormally.HasValue,
                stopAt: FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacitySharedRepeatPlan repeatPlan =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                EntryValidatedNonzero: true,
                EntryEcx: repeatEntryEcx,
                EntryEdi: repeatEntryEdi,
                DirectionFlagSet: capture.DirectionFlagSet,
                RepCompletedNormally: capture.RepCompletedNormally));

        FrameHandoffCapacityFillSharedRepeatOutcome outcome = repeatPlan.Outcome switch
        {
            FrameHandoffCapacitySharedRepeatOutcome.MissingDirectionFlagCapture =>
                FrameHandoffCapacityFillSharedRepeatOutcome.MissingDirectionFlagCapture,
            FrameHandoffCapacitySharedRepeatOutcome.MissingRepCompletionCapture =>
                FrameHandoffCapacityFillSharedRepeatOutcome.MissingRepCompletionCapture,
            FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally =>
                FrameHandoffCapacityFillSharedRepeatOutcome.RepDidNotCompleteNormally,
            FrameHandoffCapacitySharedRepeatOutcome.Completed =>
                FrameHandoffCapacityFillSharedRepeatOutcome.Completed,
            _ => FrameHandoffCapacityFillSharedRepeatOutcome.FillJoinOutcomeNotAccepted
        };

        return Stop(
            capture,
            fillJoinPlan,
            outcome,
            repeatEntryIdentityValidationReached: true,
            repeatEntryEcx: repeatEntryEcx,
            repeatEntryEdi: repeatEntryEdi,
            expectedRepeatEntryEcxMatched: expectedRepeatEntryEcxMatched,
            expectedRepeatEntryEdiMatched: expectedRepeatEntryEdiMatched,
            repeatEntryValidatedNonzero: true,
            repeatPlan: repeatPlan,
            unreachedDirectionFlag:
                repeatPlan.UnreachedDirectionFlagCaptureProvided,
            unreachedRepCompletion:
                repeatPlan.UnreachedRepCompletionCaptureProvided,
            stopAt: repeatPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityFillSharedRepeatPlan Stop(
        FrameHandoffCapacityFillSharedRepeatCapture capture,
        FrameHandoffCapacityFillJoinPlan fillJoinPlan,
        FrameHandoffCapacityFillSharedRepeatOutcome outcome,
        bool repeatEntryIdentityValidationReached = false,
        uint? repeatEntryEcx = null,
        uint? repeatEntryEdi = null,
        bool? expectedRepeatEntryEcxMatched = null,
        bool? expectedRepeatEntryEdiMatched = null,
        bool repeatEntryValidatedNonzero = false,
        FrameHandoffCapacitySharedRepeatPlan? repeatPlan = null,
        bool unreachedExpectedRepeatEntryEcx = false,
        bool unreachedExpectedRepeatEntryEdi = false,
        bool unreachedDirectionFlag = false,
        bool unreachedRepCompletion = false,
        uint stopAt = StopAtRawFileOffsetExclusive) =>
        new(
            capture,
            fillJoinPlan,
            outcome,
            repeatEntryIdentityValidationReached,
            repeatEntryEcx,
            repeatEntryEdi,
            expectedRepeatEntryEcxMatched,
            expectedRepeatEntryEdiMatched,
            repeatEntryValidatedNonzero,
            repeatPlan,
            unreachedExpectedRepeatEntryEcx,
            unreachedExpectedRepeatEntryEdi,
            unreachedDirectionFlag,
            unreachedRepCompletion,
            stopAt);
}
