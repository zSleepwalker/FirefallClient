namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolReplenishmentContinuationCapture(
    UiShaderSourcePoolReplenishmentCapture Request,
    uint AllocatorResult,
    uint PoolPointer,
    bool NullPathDiagnosticCallsReturnedNormally,
    bool? FirstPushCallReturnedNormally,
    UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture? AfterFirstPush);

public readonly record struct UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture(
    uint AlignmentDivisor,
    bool? AlignmentDiagnosticCallReturnedNormally,
    uint? InitialLoopBound20,
    UiShaderSourcePoolReplenishmentContinuationIterationCapture? FirstIteration);

public readonly record struct UiShaderSourcePoolReplenishmentContinuationIterationCapture(
    uint Field14Value,
    uint Field1CValue,
    uint PoolDivisor,
    bool DiagnosticCallReturnedNormally,
    bool PushCallReturnedNormally,
    uint? LaterBaseCapture,
    uint? FreshLoopBound20);

public enum UiShaderSourcePoolReplenishmentContinuationOutcome
{
    NullPathDiagnosticDidNotReturnNormally,
    FirstPushDidNotReturnNormally,
    AlignmentDivideFault,
    AlignmentDiagnosticDidNotReturnNormally,
    InitialLoopGateNotEntered,
    IterationDivideFault,
    IterationDiagnosticDidNotReturnNormally,
    IterationPushDidNotReturnNormally,
    RepeatDecisionReached
}

public readonly record struct UiShaderSourcePoolReplenishmentContinuationProjection(
    UiShaderSourcePoolReplenishmentContinuationCapture Capture,
    UiShaderSourcePoolReplenishmentProjection Request,
    UiShaderSourcePoolReplenishmentResultProjection Result,
    UiShaderSourcePoolReplenishmentAlignmentProjection? Alignment,
    bool? ReachedInitialLoopGate,
    bool? EnteredFirstIteration,
    UiShaderSourcePoolReplenishmentIterationProjection? FirstIteration,
    UiShaderSourcePoolReplenishmentContinuationOutcome Outcome);

/// <summary>
/// Composes the captured replenishment stages through the initial loop gate and
/// at most one already-entered iteration's repeat decision. Each stage retains
/// its own non-atomic captures; calls continue only across explicit normal-return
/// captures. No helper effects or later iterations are modeled.
/// </summary>
public static class UiShaderSourcePoolReplenishmentContinuationProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint EntryRawFileOffset =
        UiShaderSourcePoolReplenishmentProjectionRules.EntryRawFileOffset;

    public const uint RepeatDecisionEndRawFileOffsetExclusive =
        UiShaderSourcePoolReplenishmentIterationProjectionRules.IterationEndRawFileOffsetExclusive;

    public static UiShaderSourcePoolReplenishmentContinuationProjection Project(
        UiShaderSourcePoolReplenishmentContinuationCapture capture)
    {
        UiShaderSourcePoolReplenishmentProjection request =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(capture.Request);
        UiShaderSourcePoolReplenishmentResultProjection result =
            UiShaderSourcePoolReplenishmentResultProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentResultCapture(
                    capture.AllocatorResult,
                    request.RequestBytes,
                    capture.PoolPointer,
                    capture.NullPathDiagnosticCallsReturnedNormally));

        bool firstPushCallReached = result.ReachesPushHelperCall;
        if (capture.FirstPushCallReturnedNormally.HasValue != firstPushCallReached)
        {
            throw new ArgumentException(
                firstPushCallReached
                    ? "The reached first-push call requires its normal-return capture."
                    : "A first-push return capture is invalid when the call is not reached.",
                nameof(capture));
        }

        bool reachesFirstPushReturn = capture.FirstPushCallReturnedNormally == true;
        if (capture.AfterFirstPush.HasValue != reachesFirstPushReturn)
        {
            throw new ArgumentException(
                reachesFirstPushReturn
                    ? "The normal first-push return requires the subsequent alignment capture."
                    : "Alignment captures are invalid when the first push has not returned normally.",
                nameof(capture));
        }

        if (!result.ReachesPushHelperCall)
        {
            return new UiShaderSourcePoolReplenishmentContinuationProjection(
                capture,
                request,
                result,
                null,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentContinuationOutcome
                    .NullPathDiagnosticDidNotReturnNormally);
        }

        if (capture.FirstPushCallReturnedNormally == false)
        {
            return new UiShaderSourcePoolReplenishmentContinuationProjection(
                capture,
                request,
                result,
                null,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentContinuationOutcome.FirstPushDidNotReturnNormally);
        }

        UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture afterFirstPush =
            capture.AfterFirstPush!.Value;
        UiShaderSourcePoolReplenishmentAlignmentProjection alignment =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    capture.AllocatorResult,
                    afterFirstPush.AlignmentDivisor));

        if (alignment.Outcome
            == UiShaderSourcePoolReplenishmentAlignmentOutcome.DivideFaultAtFirstDivision)
        {
            if (afterFirstPush.AlignmentDiagnosticCallReturnedNormally.HasValue)
            {
                throw new ArgumentException(
                    "The alignment diagnostic return must not be captured when division faults first.",
                    nameof(capture));
            }

            ValidateAlignmentContinuationCaptures(afterFirstPush, reachedLoopGate: false);
            return new UiShaderSourcePoolReplenishmentContinuationProjection(
                capture,
                request,
                result,
                alignment,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentContinuationOutcome.AlignmentDivideFault);
        }

        bool alignmentDiagnosticReached =
            alignment.Outcome == UiShaderSourcePoolReplenishmentAlignmentOutcome
                .DiagnosticCallReached;
        if (alignmentDiagnosticReached
            != afterFirstPush.AlignmentDiagnosticCallReturnedNormally.HasValue)
        {
            throw new ArgumentException(
                alignmentDiagnosticReached
                    ? "A reached alignment diagnostic requires its normal-return capture."
                    : "An alignment diagnostic return must not be captured when that call is not reached.",
                nameof(capture));
        }

        bool reachesLoopGate = !alignmentDiagnosticReached
            || afterFirstPush.AlignmentDiagnosticCallReturnedNormally!.Value;
        ValidateAlignmentContinuationCaptures(afterFirstPush, reachesLoopGate);
        if (!reachesLoopGate)
        {
            return new UiShaderSourcePoolReplenishmentContinuationProjection(
                capture,
                request,
                result,
                alignment,
                false,
                null,
                null,
                UiShaderSourcePoolReplenishmentContinuationOutcome
                    .AlignmentDiagnosticDidNotReturnNormally);
        }

        uint initialLoopBound20 = afterFirstPush.InitialLoopBound20!.Value;
        bool entersFirstIteration =
            UiShaderSourcePoolReplenishmentIterationProjectionRules.ShouldEnterIteration(
                initialLoopBound20);
        if (!entersFirstIteration)
        {
            return new UiShaderSourcePoolReplenishmentContinuationProjection(
                capture,
                request,
                result,
                alignment,
                true,
                false,
                null,
                UiShaderSourcePoolReplenishmentContinuationOutcome.InitialLoopGateNotEntered);
        }

        UiShaderSourcePoolReplenishmentContinuationIterationCapture firstIterationCapture =
            afterFirstPush.FirstIteration!.Value;
        UiShaderSourcePoolReplenishmentIterationProjection firstIteration =
            UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentIterationCapture(
                    Index: 0,
                    CurrentBase: alignment.Candidate!.Value,
                    firstIterationCapture.Field14Value,
                    firstIterationCapture.Field1CValue,
                    firstIterationCapture.PoolDivisor,
                    firstIterationCapture.LaterBaseCapture,
                    firstIterationCapture.FreshLoopBound20,
                    firstIterationCapture.DiagnosticCallReturnedNormally,
                    firstIterationCapture.PushCallReturnedNormally));

        UiShaderSourcePoolReplenishmentContinuationOutcome outcome =
            firstIteration.Outcome switch
            {
                UiShaderSourcePoolReplenishmentIterationOutcome.DivideByZeroFault =>
                    UiShaderSourcePoolReplenishmentContinuationOutcome.IterationDivideFault,
                UiShaderSourcePoolReplenishmentIterationOutcome.DiagnosticDidNotReturnNormally =>
                    UiShaderSourcePoolReplenishmentContinuationOutcome
                        .IterationDiagnosticDidNotReturnNormally,
                UiShaderSourcePoolReplenishmentIterationOutcome.PushDidNotReturnNormally =>
                    UiShaderSourcePoolReplenishmentContinuationOutcome
                        .IterationPushDidNotReturnNormally,
                UiShaderSourcePoolReplenishmentIterationOutcome.IterationCompleted =>
                    UiShaderSourcePoolReplenishmentContinuationOutcome.RepeatDecisionReached,
                _ => throw new ArgumentOutOfRangeException(nameof(firstIteration))
            };

        return new UiShaderSourcePoolReplenishmentContinuationProjection(
            capture,
            request,
            result,
            alignment,
            true,
            true,
            firstIteration,
            outcome);
    }

    private static void ValidateAlignmentContinuationCaptures(
        UiShaderSourcePoolReplenishmentContinuationAfterFirstPushCapture capture,
        bool reachedLoopGate)
    {
        bool hasLoopBound = capture.InitialLoopBound20.HasValue;
        if (hasLoopBound != reachedLoopGate)
        {
            throw new ArgumentException(
                reachedLoopGate
                    ? "Reaching the initial loop gate requires its fresh bound capture."
                    : "The initial loop bound must not be captured before the gate is reached.",
                nameof(capture));
        }

        bool entersFirstIteration =
            reachedLoopGate && capture.InitialLoopBound20!.Value != 0;
        if (capture.FirstIteration.HasValue != entersFirstIteration)
        {
            throw new ArgumentException(
                entersFirstIteration
                    ? "A nonzero initial bound requires the one entered-iteration capture."
                    : "An iteration capture is invalid when the initial gate does not enter it.",
                nameof(capture));
        }
    }
}
