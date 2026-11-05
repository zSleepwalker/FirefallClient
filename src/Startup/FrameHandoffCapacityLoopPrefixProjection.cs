namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityLoopPrefixCapture(
    FrameHandoffCapacityLoopCapture LoopCapture,
    uint PrefixEntryEbx,
    uint PrefixEntryEbp,
    uint? ReceiverC,
    FrameHandoffCapacityCallCapture? CallCapture,
    uint? ExpectedPrefixEntryEsi = null,
    uint? ExpectedPrefixEntryEdx = null,
    uint? ExpectedPrefixEntryEbx = null,
    uint? ExpectedPrefixEntryEbp = null);

public enum FrameHandoffCapacityLoopPrefixOutcome
{
    LoopExitNotAdmitted,
    PrefixEntryIdentityMismatch,
    PrefixCallBlocked,
    PrefixCallSuccessAdmitted
}

public readonly record struct FrameHandoffCapacityLoopPrefixPlan(
    FrameHandoffCapacityLoopPrefixCapture Capture,
    FrameHandoffCapacityLoopPlan PartialLoopPlan,
    FrameHandoffCapacityLoopPrefixOutcome Outcome,
    FrameHandoffCapacityPrefixCapture? DerivedPrefixCapture,
    FrameHandoffCapacityPrefixCallPlan? PrefixCallPlan,
    bool LoopExitAdmitted,
    bool PrefixEntryIdentityCapturesReached,
    bool? ExpectedPrefixEntryEsiMatches,
    bool? ExpectedPrefixEntryEdxMatches,
    bool? ExpectedPrefixEntryEbxMatches,
    bool? ExpectedPrefixEntryEbpMatches,
    bool CallEntryEbpIdentityValidated,
    bool? CallEntryEbpIdentityMatches,
    bool PrefixCallProjectionReached,
    bool UnreachedExpectedPrefixEntryEsiCaptureProvided,
    bool UnreachedExpectedPrefixEntryEdxCaptureProvided,
    bool UnreachedExpectedPrefixEntryEbxCaptureProvided,
    bool UnreachedExpectedPrefixEntryEbpCaptureProvided,
    bool UnreachedPrefixEntryEbxCaptureProvided,
    bool UnreachedPrefixEntryEbpCaptureProvided,
    bool UnreachedPrefixReceiverCReadCaptureProvided,
    bool UnreachedCallFrameEbpIdentityCaptureProvided,
    bool UnreachedCallEntryRouteCaptureProvided,
    bool UnreachedDirectCallReturnCaptureProvided,
    bool UnreachedDirectCallReturnEaxCaptureProvided,
    bool UnreachedGuardIndirectCallReturnCaptureProvided,
    bool SuccessAdmission,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes one capacity-loop iteration with the prefix and call, admitting only the exact exit route.
/// </summary>
public static class FrameHandoffCapacityLoopPrefixProjection
{
    public const uint PrefixEntryRawFileOffset =
        FrameHandoffCapacityPrefixProjection.EntryRawFileOffset;

    public static FrameHandoffCapacityLoopPrefixPlan Project(
        FrameHandoffCapacityLoopPrefixCapture capture)
    {
        FrameHandoffCapacityLoopPlan loopPlan =
            FrameHandoffCapacityLoopProjection.Project(capture.LoopCapture);
        bool loopExitAdmitted = IsAdmittedExit(loopPlan);
        if (!loopExitAdmitted)
        {
            return StopBeforePrefix(
                capture,
                loopPlan,
                FrameHandoffCapacityLoopPrefixOutcome.LoopExitNotAdmitted,
                stopAt: loopPlan.SelectedDestinationRawFileOffset);
        }

        FrameHandoffCapacityPrefixCapture prefixCapture = new(
            capture.PrefixEntryEbx,
            capture.PrefixEntryEbp,
            loopPlan.Capture.EntryEsi,
            loopPlan.Capture.EntryEdx,
            capture.ReceiverC);

        bool? expectedEsiMatches = capture.ExpectedPrefixEntryEsi is uint expectedEsi
            ? prefixCapture.EntryEsi == expectedEsi
            : null;
        bool? expectedEdxMatches = capture.ExpectedPrefixEntryEdx is uint expectedEdx
            ? prefixCapture.EntryEdx == expectedEdx
            : null;
        bool? expectedEbxMatches = capture.ExpectedPrefixEntryEbx is uint expectedEbx
            ? prefixCapture.EntryEbx == expectedEbx
            : null;
        bool? expectedEbpMatches = capture.ExpectedPrefixEntryEbp is uint expectedEbp
            ? prefixCapture.EntryEbp == expectedEbp
            : null;
        if (expectedEsiMatches == false
            || expectedEdxMatches == false
            || expectedEbxMatches == false
            || expectedEbpMatches == false)
        {
            return StopBeforePrefix(
                capture,
                loopPlan,
                FrameHandoffCapacityLoopPrefixOutcome.PrefixEntryIdentityMismatch,
                prefixCapture,
                loopExitAdmitted: true,
                prefixEntryIdentityCapturesReached: true,
                expectedPrefixEntryEsiMatches: expectedEsiMatches,
                expectedPrefixEntryEdxMatches: expectedEdxMatches,
                expectedPrefixEntryEbxMatches: expectedEbxMatches,
                expectedPrefixEntryEbpMatches: expectedEbpMatches,
                stopAt: PrefixEntryRawFileOffset);
        }

        FrameHandoffCapacityPrefixCallPlan prefixCallPlan =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                prefixCapture,
                capture.CallCapture));
        bool successAdmission = prefixCallPlan.SuccessAdmission;

        return new(
            Capture: capture,
            PartialLoopPlan: loopPlan,
            Outcome: successAdmission
                ? FrameHandoffCapacityLoopPrefixOutcome.PrefixCallSuccessAdmitted
                : FrameHandoffCapacityLoopPrefixOutcome.PrefixCallBlocked,
            DerivedPrefixCapture: prefixCapture,
            PrefixCallPlan: prefixCallPlan,
            LoopExitAdmitted: true,
            PrefixEntryIdentityCapturesReached: true,
            ExpectedPrefixEntryEsiMatches: expectedEsiMatches,
            ExpectedPrefixEntryEdxMatches: expectedEdxMatches,
            ExpectedPrefixEntryEbxMatches: expectedEbxMatches,
            ExpectedPrefixEntryEbpMatches: expectedEbpMatches,
            CallEntryEbpIdentityValidated: prefixCallPlan.FrameEbpIdentityValidated,
            CallEntryEbpIdentityMatches: prefixCallPlan.FrameEbpIdentityMatches,
            PrefixCallProjectionReached: true,
            UnreachedExpectedPrefixEntryEsiCaptureProvided: false,
            UnreachedExpectedPrefixEntryEdxCaptureProvided: false,
            UnreachedExpectedPrefixEntryEbxCaptureProvided: false,
            UnreachedExpectedPrefixEntryEbpCaptureProvided: false,
            UnreachedPrefixEntryEbxCaptureProvided: false,
            UnreachedPrefixEntryEbpCaptureProvided: false,
            UnreachedPrefixReceiverCReadCaptureProvided: false,
            UnreachedCallFrameEbpIdentityCaptureProvided: capture.CallCapture.HasValue
                && !prefixCallPlan.FrameEbpIdentityValidated,
            UnreachedCallEntryRouteCaptureProvided:
                prefixCallPlan.UnreachedCallEntryRouteCaptureProvided,
            UnreachedDirectCallReturnCaptureProvided:
                prefixCallPlan.UnreachedDirectCallReturnCaptureProvided,
            UnreachedDirectCallReturnEaxCaptureProvided:
                prefixCallPlan.UnreachedDirectCallReturnEaxCaptureProvided,
            UnreachedGuardIndirectCallReturnCaptureProvided:
                prefixCallPlan.UnreachedGuardIndirectCallReturnCaptureProvided,
            SuccessAdmission: successAdmission,
            StopAtRawFileOffsetExclusive: prefixCallPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsAdmittedExit(FrameHandoffCapacityLoopPlan loopPlan) =>
        loopPlan.Outcome == FrameHandoffCapacityLoopOutcome.ExitAtRawFileOffset91
        && loopPlan.CompareEsiWithEightReached
        && loopPlan.EsiAtLeastEight == true
        && loopPlan.ExitBranchTaken == true
        && loopPlan.SelectedDestinationRawFileOffset
            == FrameHandoffCapacityLoopProjection.ExitRawFileOffset;

    private static FrameHandoffCapacityLoopPrefixPlan StopBeforePrefix(
        FrameHandoffCapacityLoopPrefixCapture capture,
        FrameHandoffCapacityLoopPlan loopPlan,
        FrameHandoffCapacityLoopPrefixOutcome outcome,
        FrameHandoffCapacityPrefixCapture? prefixCapture = null,
        bool loopExitAdmitted = false,
        bool prefixEntryIdentityCapturesReached = false,
        bool? expectedPrefixEntryEsiMatches = null,
        bool? expectedPrefixEntryEdxMatches = null,
        bool? expectedPrefixEntryEbxMatches = null,
        bool? expectedPrefixEntryEbpMatches = null,
        uint stopAt = PrefixEntryRawFileOffset) =>
        new(
            Capture: capture,
            PartialLoopPlan: loopPlan,
            Outcome: outcome,
            DerivedPrefixCapture: prefixCapture,
            PrefixCallPlan: null,
            LoopExitAdmitted: loopExitAdmitted,
            PrefixEntryIdentityCapturesReached: prefixEntryIdentityCapturesReached,
            ExpectedPrefixEntryEsiMatches: expectedPrefixEntryEsiMatches,
            ExpectedPrefixEntryEdxMatches: expectedPrefixEntryEdxMatches,
            ExpectedPrefixEntryEbxMatches: expectedPrefixEntryEbxMatches,
            ExpectedPrefixEntryEbpMatches: expectedPrefixEntryEbpMatches,
            CallEntryEbpIdentityValidated: false,
            CallEntryEbpIdentityMatches: null,
            PrefixCallProjectionReached: false,
            UnreachedExpectedPrefixEntryEsiCaptureProvided:
                !prefixEntryIdentityCapturesReached && capture.ExpectedPrefixEntryEsi.HasValue,
            UnreachedExpectedPrefixEntryEdxCaptureProvided:
                !prefixEntryIdentityCapturesReached && capture.ExpectedPrefixEntryEdx.HasValue,
            UnreachedExpectedPrefixEntryEbxCaptureProvided:
                !prefixEntryIdentityCapturesReached && capture.ExpectedPrefixEntryEbx.HasValue,
            UnreachedExpectedPrefixEntryEbpCaptureProvided:
                !prefixEntryIdentityCapturesReached && capture.ExpectedPrefixEntryEbp.HasValue,
            UnreachedPrefixEntryEbxCaptureProvided: !prefixEntryIdentityCapturesReached,
            UnreachedPrefixEntryEbpCaptureProvided: !prefixEntryIdentityCapturesReached,
            UnreachedPrefixReceiverCReadCaptureProvided: capture.ReceiverC.HasValue,
            UnreachedCallFrameEbpIdentityCaptureProvided: capture.CallCapture.HasValue,
            UnreachedCallEntryRouteCaptureProvided:
                capture.CallCapture?.EntryRoute.HasValue == true,
            UnreachedDirectCallReturnCaptureProvided:
                capture.CallCapture?.DirectCallReturnedNormally.HasValue == true,
            UnreachedDirectCallReturnEaxCaptureProvided:
                capture.CallCapture?.DirectCallReturnEax.HasValue == true,
            UnreachedGuardIndirectCallReturnCaptureProvided:
                capture.CallCapture?.GuardIndirectCallReturnedNormally.HasValue == true,
            SuccessAdmission: false,
            StopAtRawFileOffsetExclusive: stopAt);
}
