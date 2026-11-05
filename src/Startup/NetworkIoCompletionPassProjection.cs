using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// FailedCompletionError is the raw return observation from the WSOCK32
/// ordinal-111 import slot; no named API semantics are assigned to it.
/// </summary>
public readonly record struct NetworkIoCompletionAttemptCapture(
    bool? DequeueSucceeded,
    uint? OverlappedPointerFromDequeue,
    uint? CompletionKeyPointerFromDequeue,
    uint? GetLastErrorAfterDequeue,
    bool? DiagnosticReturnedNormally,
    uint? CompletionKeyPointerAfterDiagnostic,
    uint? OverlappedPointerAtSuccessDispatch,
    uint? FailedCompletionError,
    uint? CompletionKeyPointerAtErrorDispatch,
    bool? CallbackReturnedNormally);

public readonly record struct NetworkIoCompletionPassCapture(
    bool? StopFlagAtEntry,
    ImmutableArray<NetworkIoCompletionAttemptCapture>? AttemptCaptures,
    bool? StopFlagAfterPass);

public enum NetworkIoCompletionCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoCompletionAttemptCaptureStates(
    NetworkIoCompletionCaptureState DequeueSucceeded,
    NetworkIoCompletionCaptureState OverlappedPointerFromDequeue,
    NetworkIoCompletionCaptureState CompletionKeyPointerFromDequeue,
    NetworkIoCompletionCaptureState GetLastErrorAfterDequeue,
    NetworkIoCompletionCaptureState DiagnosticReturnedNormally,
    NetworkIoCompletionCaptureState CompletionKeyPointerAfterDiagnostic,
    NetworkIoCompletionCaptureState OverlappedPointerAtSuccessDispatch,
    NetworkIoCompletionCaptureState FailedCompletionError,
    NetworkIoCompletionCaptureState CompletionKeyPointerAtErrorDispatch,
    NetworkIoCompletionCaptureState CallbackReturnedNormally);

public enum NetworkIoCompletionPassOutcome
{
    MissingInitialStopFlag,
    EntryStopRequested,
    MissingAttemptCapture,
    MissingGetLastErrorAfterDequeue,
    MissingDequeueBoolean,
    MissingOverlappedPointerFromDequeue,
    MissingCompletionKeyPointerFromDequeue,
    MissingDiagnosticReturnObservation,
    DiagnosticDidNotReturnNormally,
    MissingCompletionKeyPointerAfterDiagnostic,
    InvalidCompletionKeyPointerAfterDiagnostic,
    MissingOverlappedPointerAtSuccessDispatch,
    MissingFailedCompletionError,
    MissingCompletionKeyPointerAtErrorDispatch,
    InvalidCompletionKeyPointerAtErrorDispatch,
    MissingCallbackReturnObservation,
    CallbackDidNotReturnNormally,
    EmptyFailureAfterFallbackConsumed,
    AttemptLimitReached
}

public enum NetworkIoCompletionPassDisposition
{
    ExactCleanCompletion,
    BoundedInnerPassCompletion,
    BoundedBeforeInnerPass,
    BoundedAtCapture,
    BoundedAtInvalidCompletionKey
}

public enum NetworkIoCompletionDiagnosticDecision
{
    NotEvaluated,
    NotRequired,
    UnexpectedEmptyFailure,
    NullCompletionKey
}

public enum NetworkIoCompletionCallbackDecision
{
    NotEvaluated,
    NotInvoked,
    Receive,
    Close,
    Error,
    OperationAbortedSkipped
}

public readonly record struct NetworkIoCompletionAttemptPlan(
    int AttemptNumber,
    int RequestedTimeoutMilliseconds,
    bool HasFreshOutcomeCapture,
    NetworkIoCompletionAttemptCapture Capture,
    NetworkIoCompletionAttemptCaptureStates CaptureStates,
    bool DequeueBooleanBranchReached,
    NetworkIoCompletionDiagnosticDecision DiagnosticDecision,
    NetworkIoCompletionCallbackDecision CallbackDecision,
    int? DispatchSlotOffset,
    bool CountedTowardAttemptLimit,
    bool FallbackTimeoutSelected,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset);

public sealed class NetworkIoCompletionPassPlan
{
    internal NetworkIoCompletionPassPlan(
        bool passEntered,
        NetworkIoCompletionCaptureState stopFlagAtEntryState,
        NetworkIoCompletionCaptureState attemptCapturesState,
        NetworkIoCompletionCaptureState stopFlagAfterPassState,
        bool stopFlagAfterPassReadReached,
        bool? suppliedStopFlagAfterPass,
        NetworkIoCompletionPassOutcome outcome,
        NetworkIoCompletionPassDisposition disposition,
        int completedPassIterations,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        ImmutableArray<NetworkIoCompletionAttemptPlan> attempts,
        ImmutableArray<int> timeoutSequence,
        ImmutableArray<NetworkIoCompletionAttemptCapture> unusedAttemptCaptures)
    {
        PassEntered = passEntered;
        StopFlagAtEntryState = stopFlagAtEntryState;
        AttemptCapturesState = attemptCapturesState;
        StopFlagAfterPassState = stopFlagAfterPassState;
        StopFlagAfterPassReadReached = stopFlagAfterPassReadReached;
        SuppliedStopFlagAfterPass = suppliedStopFlagAfterPass;
        Outcome = outcome;
        Disposition = disposition;
        CompletedPassIterations = completedPassIterations;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        Attempts = attempts;
        TimeoutSequence = timeoutSequence;
        UnusedAttemptCaptures = unusedAttemptCaptures;
    }

    public bool PassEntered { get; }
    public NetworkIoCompletionCaptureState StopFlagAtEntryState { get; }
    public NetworkIoCompletionCaptureState AttemptCapturesState { get; }
    public NetworkIoCompletionCaptureState StopFlagAfterPassState { get; }
    public bool StopFlagAfterPassReadReached { get; }
    public bool? SuppliedStopFlagAfterPass { get; }
    public NetworkIoCompletionPassOutcome Outcome { get; }
    public NetworkIoCompletionPassDisposition Disposition { get; }
    public int CompletedPassIterations { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public ImmutableArray<NetworkIoCompletionAttemptPlan> Attempts { get; }
    public int AttemptCount => Attempts.Length;
    public ImmutableArray<int> TimeoutSequence { get; }
    public ImmutableArray<NetworkIoCompletionAttemptCapture> UnusedAttemptCaptures { get; }
}

/// <summary>
/// Projects one bounded PumpThread dequeue pass. Native diagnostic and callback
/// calls are represented only by captured normal-return observations.
/// </summary>
public static class NetworkIoCompletionPassProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint IatVirtualAddressDeltaFromRawFileOffset = 0x00401400;
    public const uint PumpThreadEntryRawFileOffset =
        NetworkIoCompletionNativeLayout.PumpThreadMethodFileOffset;
    public const uint PumpThreadEntryVirtualAddress =
        PumpThreadEntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DequeueCallRawFileOffset =
        NetworkIoCompletionNativeLayout.DequeueCompletionCallFileOffset;
    public const uint PostDequeueGetLastErrorCallRawFileOffset = 0xBBC916;
    public const uint FailedCompletionErrorCallRawFileOffset = 0xBBC973;
    public const uint InitialStopFlagReadRawFileOffset = 0xBBC878;
    public const uint StopFlagAfterPassReadRawFileOffset = 0xBBC9CE;
    public const uint GetQueuedCompletionStatusImportVirtualAddress =
        NetworkIoCompletionNativeLayout.GetQueuedCompletionStatusImportVirtualAddress;
    public const uint GetQueuedCompletionStatusImportRawFileOffset = 0x16C2F2C;
    public const uint GetLastErrorAfterDequeueImportVirtualAddress = 0x1AC4378;
    public const uint GetLastErrorAfterDequeueImportRawFileOffset = 0x16C2F78;
    public const string GetLastErrorAfterDequeueImportModule = "KERNEL32.dll";
    public const uint FailedCompletionErrorImportVirtualAddress = 0x1AC4DA0;
    public const uint FailedCompletionErrorImportRawFileOffset = 0x16C39A0;
    public const string FailedCompletionErrorImportModule = "WSOCK32.dll";
    public const ushort FailedCompletionErrorImportOrdinal = 111;
    public const uint FailedCompletionErrorImportOrdinalThunk = 0x8000006F;

    public const int MaximumAttempts = NetworkIoCompletionNativeLayout.DrainIterationLimit;
    public const int InitialTimeoutMilliseconds = 0;
    public const int FallbackTimeoutMilliseconds =
        NetworkIoCompletionNativeLayout.IdleWaitMilliseconds;
    public const int WaitTimeoutError = 258;
    public const int OperationAbortedError =
        NetworkIoCompletionNativeLayout.OperationAbortedError;

    public const int ReceiveCallbackSlotOffset =
        NetworkIoCompletionNativeLayout.ReceiveCallbackVirtualOffset;
    public const int CloseCallbackSlotOffset =
        NetworkIoCompletionNativeLayout.CloseCallbackVirtualOffset;
    public const int ErrorCallbackSlotOffset =
        NetworkIoCompletionNativeLayout.ErrorCallbackVirtualOffset;

    private const uint DequeueReturnRawFileOffset = 0xBBC914;
    private const uint DequeueBooleanTestRawFileOffset = 0xBBC920;
    private const uint DequeueResultBranchRawFileOffset = 0xBBC922;
    private const uint FailedCompletionOverlapCompareRawFileOffset = 0xBBC924;
    private const uint EmptyFailureDiagnosticCallRawFileOffset = 0xBBC940;
    private const uint EmptyFailureDiagnosticReturnRawFileOffset = 0xBBC946;
    private const uint FailedCompletionKeyReadRawFileOffset = 0xBBC953;
    private const uint FailedCompletionKeyBranchRawFileOffset = 0xBBC957;
    private const uint FailedCompletionDiagnosticCallRawFileOffset = 0xBBC96A;
    private const uint FailedCompletionDiagnosticReturnRawFileOffset = 0xBBC970;
    private const uint FailedCompletionErrorTestRawFileOffset = 0xBBC976;
    private const uint FailedCompletionKeyReloadRawFileOffset = 0xBBC97D;
    private const uint FailedCompletionKeyDereferenceRawFileOffset = 0xBBC981;
    private const uint ErrorCallbackCallRawFileOffset = 0xBBC983;
    private const uint ErrorCallbackReturnRawFileOffset = 0xBBC986;
    private const uint SuccessCompletionKeyReadRawFileOffset = 0xBBC988;
    private const uint SuccessCompletionKeyBranchRawFileOffset = 0xBBC98D;
    private const uint SuccessCompletionKeyTestRawFileOffset = 0xBBC98B;
    private const uint SuccessCompletionDiagnosticCallRawFileOffset = 0xBBC99F;
    private const uint SuccessCompletionKeyReloadRawFileOffset = 0xBBC9A5;
    private const uint SuccessOverlappedTestRawFileOffset = 0xBBC9AB;
    private const uint SuccessCompletionKeyDereferenceRawFileOffset = 0xBBC9AF;
    private const uint ReceiveCallbackCallRawFileOffset = 0xBBC9B3;
    private const uint ReceiveCallbackReturnRawFileOffset = 0xBBC9B6;
    private const uint CloseCallbackCallRawFileOffset = 0xBBC9B8;
    private const uint CloseCallbackReturnRawFileOffset = 0xBBC9BB;
    private const uint CompletedAttemptIncrementRawFileOffset = 0xBBC9C1;
    private const uint AttemptLimitCompareRawFileOffset = 0xBBC9C2;
    private const uint AfterPassStopBranchRawFileOffset = 0xBBC9D5;
    private const uint InvalidStopFlagExitRawFileOffset = 0xBBC9DD;
    private const uint EmptyFailureFallbackBranchRawFileOffset = 0xBBC949;
    private const uint EmptyFailureStopBranchRawFileOffset = 0xBBC94B;

    public static NetworkIoCompletionPassPlan Project(
        NetworkIoCompletionPassCapture capture)
    {
        ImmutableArray<NetworkIoCompletionAttemptCapture> suppliedCaptures =
            capture.AttemptCaptures.GetValueOrDefault();
        bool hasCaptureList = capture.AttemptCaptures.HasValue
            && !capture.AttemptCaptures.Value.IsDefault;
        var attempts = ImmutableArray.CreateBuilder<NetworkIoCompletionAttemptPlan>();
        var timeouts = ImmutableArray.CreateBuilder<int>();
        int consumedCaptureCount = 0;
        bool passEntered = capture.StopFlagAtEntry == false;

        NetworkIoCompletionPassPlan Finish(
            NetworkIoCompletionPassOutcome outcome,
            NetworkIoCompletionPassDisposition disposition,
            int completedPassIterations,
            bool stopFlagAfterPassReadReached,
            uint? lastReachedRawFileOffset,
            uint? stopBeforeRawFileOffset)
        {
            if (stopFlagAfterPassReadReached)
            {
                lastReachedRawFileOffset = StopFlagAfterPassReadRawFileOffset;
                stopBeforeRawFileOffset = capture.StopFlagAfterPass.HasValue
                    ? AfterPassStopBranchRawFileOffset
                    : StopFlagAfterPassReadRawFileOffset;
            }

            ImmutableArray<NetworkIoCompletionAttemptCapture> unusedCaptures =
                suppliedCaptures.IsDefaultOrEmpty
                    ? ImmutableArray<NetworkIoCompletionAttemptCapture>.Empty
                    : suppliedCaptures
                        .Skip(consumedCaptureCount)
                        .ToImmutableArray();
            NetworkIoCompletionCaptureState stopAfterState =
                stopFlagAfterPassReadReached
                    ? State(capture.StopFlagAfterPass, reached: true)
                    : State(capture.StopFlagAfterPass, reached: false);
            NetworkIoCompletionCaptureState attemptListState =
                passEntered
                    ? hasCaptureList
                        ? NetworkIoCompletionCaptureState.Captured
                        : NetworkIoCompletionCaptureState.Missing
                    : hasCaptureList
                        ? NetworkIoCompletionCaptureState.SuppliedButUnused
                        : NetworkIoCompletionCaptureState.NotReached;

            ImmutableArray<NetworkIoCompletionAttemptPlan> attemptSnapshot =
                attempts.ToImmutable();
            bool hasUnusedCapture =
                unusedCaptures.Length != 0
                || attemptListState == NetworkIoCompletionCaptureState.SuppliedButUnused
                || stopAfterState == NetworkIoCompletionCaptureState.SuppliedButUnused
                || attemptSnapshot.Any(attempt =>
                    HasSuppliedButUnusedCapture(attempt.CaptureStates));
            bool exactCleanCompletion =
                !hasUnusedCapture
                && ((outcome == NetworkIoCompletionPassOutcome.EntryStopRequested
                    && disposition == NetworkIoCompletionPassDisposition.BoundedBeforeInnerPass)
                    || (disposition
                            == NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion
                        && stopFlagAfterPassReadReached
                        && capture.StopFlagAfterPass.HasValue));
            if (exactCleanCompletion)
                disposition = NetworkIoCompletionPassDisposition.ExactCleanCompletion;

            return new NetworkIoCompletionPassPlan(
                passEntered,
                State(capture.StopFlagAtEntry, reached: true),
                attemptListState,
                stopAfterState,
                stopFlagAfterPassReadReached,
                capture.StopFlagAfterPass,
                outcome,
                disposition,
                completedPassIterations,
                lastReachedRawFileOffset,
                stopBeforeRawFileOffset,
                attemptSnapshot,
                timeouts.ToImmutable(),
                unusedCaptures);
        }

        if (capture.StopFlagAtEntry is null)
            return Finish(
                NetworkIoCompletionPassOutcome.MissingInitialStopFlag,
                NetworkIoCompletionPassDisposition.BoundedAtCapture,
                0,
                false,
                InitialStopFlagReadRawFileOffset,
                InitialStopFlagReadRawFileOffset);

        if (capture.StopFlagAtEntry.Value)
            return Finish(
                NetworkIoCompletionPassOutcome.EntryStopRequested,
                NetworkIoCompletionPassDisposition.BoundedBeforeInnerPass,
                0,
                false,
                InitialStopFlagReadRawFileOffset,
                InvalidStopFlagExitRawFileOffset);

        int timeout = InitialTimeoutMilliseconds;
        int completedIterations = 0;
        bool fallbackAvailable = true;

        for (int attemptIndex = 0; attemptIndex < MaximumAttempts; attemptIndex++)
        {
            timeouts.Add(timeout);
            bool hasCapture = hasCaptureList && attemptIndex < suppliedCaptures.Length;
            NetworkIoCompletionAttemptCapture attemptCapture = hasCapture
                ? suppliedCaptures[attemptIndex]
                : default;
            if (hasCapture)
                consumedCaptureCount++;

            var reached = new AttemptReach();
            var attempt = new AttemptBuilder(
                attemptIndex + 1,
                timeout,
                hasCapture,
                attemptCapture);

            attempt.LastReachedRawFileOffset = DequeueCallRawFileOffset;
            attempt.StopBeforeRawFileOffset = DequeueReturnRawFileOffset;
            reached.GetLastErrorAfterDequeue = true;
            attempt.LastReachedRawFileOffset = PostDequeueGetLastErrorCallRawFileOffset;
            attempt.StopBeforeRawFileOffset = DequeueBooleanTestRawFileOffset;
            if (attemptCapture.GetLastErrorAfterDequeue is null)
            {
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    hasCapture
                        ? NetworkIoCompletionPassOutcome.MissingGetLastErrorAfterDequeue
                        : NetworkIoCompletionPassOutcome.MissingAttemptCapture,
                    NetworkIoCompletionPassDisposition.BoundedAtCapture,
                    completedIterations,
                    false,
                    attempt.LastReachedRawFileOffset,
                    attempt.StopBeforeRawFileOffset);
            }

            if (attemptCapture.DequeueSucceeded is null)
            {
                reached.DequeueSucceeded = true;
                attempt.LastReachedRawFileOffset = DequeueBooleanTestRawFileOffset;
                attempt.StopBeforeRawFileOffset = DequeueBooleanTestRawFileOffset;
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    NetworkIoCompletionPassOutcome.MissingDequeueBoolean,
                    NetworkIoCompletionPassDisposition.BoundedAtCapture,
                    completedIterations,
                    false,
                    DequeueBooleanTestRawFileOffset,
                    DequeueBooleanTestRawFileOffset);
            }

            reached.DequeueSucceeded = true;
            attempt.DequeueBooleanBranchReached = true;
            if (attemptCapture.DequeueSucceeded.Value)
            {
                reached.CompletionKeyPointerFromDequeue = true;
                attempt.LastReachedRawFileOffset = SuccessCompletionKeyReadRawFileOffset;
                attempt.StopBeforeRawFileOffset = SuccessCompletionKeyReadRawFileOffset;
                if (attemptCapture.CompletionKeyPointerFromDequeue is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerFromDequeue,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                uint completionKey = attemptCapture.CompletionKeyPointerFromDequeue.Value;
                bool invalidCompletionKeyAtDispatch = false;
                attempt.LastReachedRawFileOffset = SuccessCompletionKeyTestRawFileOffset;
                if (completionKey == 0)
                {
                    attempt.DiagnosticDecision =
                        NetworkIoCompletionDiagnosticDecision.NullCompletionKey;
                    reached.DiagnosticReturnedNormally = true;
                    attempt.LastReachedRawFileOffset =
                        SuccessCompletionDiagnosticCallRawFileOffset;
                    attempt.StopBeforeRawFileOffset =
                        SuccessCompletionKeyReloadRawFileOffset;
                    if (attemptCapture.DiagnosticReturnedNormally is null)
                    {
                        attempts.Add(attempt.ToPlan(reached));
                        return Finish(
                            NetworkIoCompletionPassOutcome.MissingDiagnosticReturnObservation,
                            NetworkIoCompletionPassDisposition.BoundedAtCapture,
                            completedIterations,
                            false,
                            attempt.LastReachedRawFileOffset,
                            attempt.StopBeforeRawFileOffset);
                    }

                    if (!attemptCapture.DiagnosticReturnedNormally.Value)
                    {
                        attempts.Add(attempt.ToPlan(reached));
                        return Finish(
                            NetworkIoCompletionPassOutcome.DiagnosticDidNotReturnNormally,
                            NetworkIoCompletionPassDisposition.BoundedAtCapture,
                            completedIterations,
                            false,
                            attempt.LastReachedRawFileOffset,
                            attempt.StopBeforeRawFileOffset);
                    }

                    reached.CompletionKeyPointerAfterDiagnostic = true;
                    attempt.LastReachedRawFileOffset =
                        SuccessCompletionKeyReloadRawFileOffset;
                    attempt.StopBeforeRawFileOffset =
                        SuccessCompletionKeyReloadRawFileOffset;
                    if (attemptCapture.CompletionKeyPointerAfterDiagnostic is null)
                    {
                        attempts.Add(attempt.ToPlan(reached));
                        return Finish(
                            NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerAfterDiagnostic,
                            NetworkIoCompletionPassDisposition.BoundedAtCapture,
                            completedIterations,
                            false,
                            attempt.LastReachedRawFileOffset,
                            attempt.StopBeforeRawFileOffset);
                    }

                    invalidCompletionKeyAtDispatch =
                        attemptCapture.CompletionKeyPointerAfterDiagnostic.Value == 0;
                }
                else
                {
                    attempt.LastReachedRawFileOffset = SuccessCompletionKeyBranchRawFileOffset;
                    attempt.DiagnosticDecision =
                        NetworkIoCompletionDiagnosticDecision.NotRequired;
                }

                reached.OverlappedPointerAtSuccessDispatch = true;
                attempt.LastReachedRawFileOffset = SuccessOverlappedTestRawFileOffset;
                attempt.StopBeforeRawFileOffset = SuccessOverlappedTestRawFileOffset;
                if (attemptCapture.OverlappedPointerAtSuccessDispatch is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingOverlappedPointerAtSuccessDispatch,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                bool hasOverlappedAtDispatch =
                    attemptCapture.OverlappedPointerAtSuccessDispatch.Value != 0;
                if (invalidCompletionKeyAtDispatch)
                {
                    attempt.LastReachedRawFileOffset = SuccessOverlappedTestRawFileOffset;
                    attempt.StopBeforeRawFileOffset = SuccessCompletionKeyDereferenceRawFileOffset;
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.InvalidCompletionKeyPointerAfterDiagnostic,
                        NetworkIoCompletionPassDisposition.BoundedAtInvalidCompletionKey,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                attempt.CallbackDecision = hasOverlappedAtDispatch
                    ? NetworkIoCompletionCallbackDecision.Receive
                    : NetworkIoCompletionCallbackDecision.Close;
                attempt.DispatchSlotOffset = hasOverlappedAtDispatch
                    ? ReceiveCallbackSlotOffset
                    : CloseCallbackSlotOffset;
                reached.CallbackReturnedNormally = true;
                attempt.LastReachedRawFileOffset = hasOverlappedAtDispatch
                    ? ReceiveCallbackCallRawFileOffset
                    : CloseCallbackCallRawFileOffset;
                attempt.StopBeforeRawFileOffset = hasOverlappedAtDispatch
                    ? ReceiveCallbackReturnRawFileOffset
                    : CloseCallbackReturnRawFileOffset;
                if (attemptCapture.CallbackReturnedNormally is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingCallbackReturnObservation,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                if (!attemptCapture.CallbackReturnedNormally.Value)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.CallbackDidNotReturnNormally,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                completedIterations++;
                attempt.CountedTowardAttemptLimit = true;
                attempt.LastReachedRawFileOffset = AttemptLimitCompareRawFileOffset;
                if (completedIterations == MaximumAttempts)
                {
                    attempt.StopBeforeRawFileOffset = StopFlagAfterPassReadRawFileOffset;
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.AttemptLimitReached,
                        NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion,
                        completedIterations,
                        true,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                attempt.StopBeforeRawFileOffset = DequeueCallRawFileOffset;
                attempts.Add(attempt.ToPlan(reached));
                timeout = InitialTimeoutMilliseconds;
                continue;
            }

            attempt.LastReachedRawFileOffset = DequeueResultBranchRawFileOffset;
            attempt.StopBeforeRawFileOffset = FailedCompletionOverlapCompareRawFileOffset;
            reached.OverlappedPointerFromDequeue = true;
            if (attemptCapture.OverlappedPointerFromDequeue is null)
            {
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    NetworkIoCompletionPassOutcome.MissingOverlappedPointerFromDequeue,
                    NetworkIoCompletionPassDisposition.BoundedAtCapture,
                    completedIterations,
                    false,
                    attempt.LastReachedRawFileOffset,
                    attempt.StopBeforeRawFileOffset);
            }

            if (attemptCapture.OverlappedPointerFromDequeue.Value == 0)
            {
                uint error = attemptCapture.GetLastErrorAfterDequeue.Value;
                if (error != WaitTimeoutError)
                {
                    attempt.DiagnosticDecision =
                        NetworkIoCompletionDiagnosticDecision.UnexpectedEmptyFailure;
                    reached.DiagnosticReturnedNormally = true;
                    attempt.LastReachedRawFileOffset =
                        EmptyFailureDiagnosticCallRawFileOffset;
                    attempt.StopBeforeRawFileOffset = EmptyFailureDiagnosticReturnRawFileOffset;
                    if (attemptCapture.DiagnosticReturnedNormally is null)
                    {
                        attempts.Add(attempt.ToPlan(reached));
                        return Finish(
                            NetworkIoCompletionPassOutcome.MissingDiagnosticReturnObservation,
                            NetworkIoCompletionPassDisposition.BoundedAtCapture,
                            completedIterations,
                            false,
                            attempt.LastReachedRawFileOffset,
                            attempt.StopBeforeRawFileOffset);
                    }

                    if (!attemptCapture.DiagnosticReturnedNormally.Value)
                    {
                        attempts.Add(attempt.ToPlan(reached));
                        return Finish(
                            NetworkIoCompletionPassOutcome.DiagnosticDidNotReturnNormally,
                            NetworkIoCompletionPassDisposition.BoundedAtCapture,
                            completedIterations,
                            false,
                            attempt.LastReachedRawFileOffset,
                            attempt.StopBeforeRawFileOffset);
                    }
                }
                else
                {
                    attempt.DiagnosticDecision =
                        NetworkIoCompletionDiagnosticDecision.NotRequired;
                }

                if (!fallbackAvailable)
                {
                    attempt.CallbackDecision =
                        NetworkIoCompletionCallbackDecision.NotInvoked;
                    attempt.LastReachedRawFileOffset = EmptyFailureStopBranchRawFileOffset;
                    attempt.StopBeforeRawFileOffset = StopFlagAfterPassReadRawFileOffset;
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.EmptyFailureAfterFallbackConsumed,
                        NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion,
                        completedIterations,
                        true,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                fallbackAvailable = false;
                attempt.CallbackDecision = NetworkIoCompletionCallbackDecision.NotInvoked;
                attempt.CountedTowardAttemptLimit = true;
                attempt.FallbackTimeoutSelected = true;
                completedIterations++;
                attempt.LastReachedRawFileOffset = CompletedAttemptIncrementRawFileOffset;
                if (completedIterations == MaximumAttempts)
                {
                    attempt.LastReachedRawFileOffset = AttemptLimitCompareRawFileOffset;
                    attempt.StopBeforeRawFileOffset = StopFlagAfterPassReadRawFileOffset;
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.AttemptLimitReached,
                        NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion,
                        completedIterations,
                        true,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                attempt.StopBeforeRawFileOffset = DequeueCallRawFileOffset;
                attempts.Add(attempt.ToPlan(reached));
                timeout = FallbackTimeoutMilliseconds;
                continue;
            }

            reached.CompletionKeyPointerFromDequeue = true;
            attempt.LastReachedRawFileOffset = FailedCompletionKeyReadRawFileOffset;
            attempt.StopBeforeRawFileOffset = FailedCompletionKeyReadRawFileOffset;
            if (attemptCapture.CompletionKeyPointerFromDequeue is null)
            {
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerFromDequeue,
                    NetworkIoCompletionPassDisposition.BoundedAtCapture,
                    completedIterations,
                    false,
                    attempt.LastReachedRawFileOffset,
                    attempt.StopBeforeRawFileOffset);
            }

            if (attemptCapture.CompletionKeyPointerFromDequeue.Value == 0)
            {
                attempt.DiagnosticDecision =
                    NetworkIoCompletionDiagnosticDecision.NullCompletionKey;
                reached.DiagnosticReturnedNormally = true;
                attempt.LastReachedRawFileOffset =
                    FailedCompletionDiagnosticCallRawFileOffset;
                attempt.StopBeforeRawFileOffset = FailedCompletionDiagnosticReturnRawFileOffset;
                if (attemptCapture.DiagnosticReturnedNormally is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingDiagnosticReturnObservation,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                if (!attemptCapture.DiagnosticReturnedNormally.Value)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.DiagnosticDidNotReturnNormally,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }
            }
            else
            {
                attempt.LastReachedRawFileOffset = FailedCompletionKeyBranchRawFileOffset;
                attempt.DiagnosticDecision =
                    NetworkIoCompletionDiagnosticDecision.NotRequired;
            }

            attempt.LastReachedRawFileOffset = FailedCompletionErrorCallRawFileOffset;
            attempt.StopBeforeRawFileOffset = FailedCompletionErrorTestRawFileOffset;
            reached.FailedCompletionError = true;
            if (attemptCapture.FailedCompletionError is null)
            {
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    NetworkIoCompletionPassOutcome.MissingFailedCompletionError,
                    NetworkIoCompletionPassDisposition.BoundedAtCapture,
                    completedIterations,
                    false,
                    attempt.LastReachedRawFileOffset,
                    attempt.StopBeforeRawFileOffset);
            }

            if (attemptCapture.FailedCompletionError.Value != OperationAbortedError)
            {
                reached.CompletionKeyPointerAtErrorDispatch = true;
                attempt.LastReachedRawFileOffset = FailedCompletionKeyReloadRawFileOffset;
                attempt.StopBeforeRawFileOffset = FailedCompletionKeyReloadRawFileOffset;
                if (attemptCapture.CompletionKeyPointerAtErrorDispatch is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerAtErrorDispatch,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                if (attemptCapture.CompletionKeyPointerAtErrorDispatch.Value == 0)
                {
                    attempt.StopBeforeRawFileOffset = FailedCompletionKeyDereferenceRawFileOffset;
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.InvalidCompletionKeyPointerAtErrorDispatch,
                        NetworkIoCompletionPassDisposition.BoundedAtInvalidCompletionKey,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                attempt.CallbackDecision = NetworkIoCompletionCallbackDecision.Error;
                attempt.DispatchSlotOffset = ErrorCallbackSlotOffset;
                reached.CallbackReturnedNormally = true;
                attempt.LastReachedRawFileOffset = ErrorCallbackCallRawFileOffset;
                attempt.StopBeforeRawFileOffset = ErrorCallbackReturnRawFileOffset;
                if (attemptCapture.CallbackReturnedNormally is null)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.MissingCallbackReturnObservation,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }

                if (!attemptCapture.CallbackReturnedNormally.Value)
                {
                    attempts.Add(attempt.ToPlan(reached));
                    return Finish(
                        NetworkIoCompletionPassOutcome.CallbackDidNotReturnNormally,
                        NetworkIoCompletionPassDisposition.BoundedAtCapture,
                        completedIterations,
                        false,
                        attempt.LastReachedRawFileOffset,
                        attempt.StopBeforeRawFileOffset);
                }
            }
            else
            {
                attempt.CallbackDecision =
                    NetworkIoCompletionCallbackDecision.OperationAbortedSkipped;
                attempt.LastReachedRawFileOffset = FailedCompletionErrorTestRawFileOffset;
                attempt.StopBeforeRawFileOffset = CompletedAttemptIncrementRawFileOffset;
            }

            completedIterations++;
            attempt.CountedTowardAttemptLimit = true;
            attempt.LastReachedRawFileOffset = AttemptLimitCompareRawFileOffset;
            if (completedIterations == MaximumAttempts)
            {
                attempt.StopBeforeRawFileOffset = StopFlagAfterPassReadRawFileOffset;
                attempts.Add(attempt.ToPlan(reached));
                return Finish(
                    NetworkIoCompletionPassOutcome.AttemptLimitReached,
                    NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion,
                    completedIterations,
                    true,
                    attempt.LastReachedRawFileOffset,
                    attempt.StopBeforeRawFileOffset);
            }

            attempt.StopBeforeRawFileOffset = DequeueCallRawFileOffset;
            attempts.Add(attempt.ToPlan(reached));
            timeout = InitialTimeoutMilliseconds;
        }

        return Finish(
            NetworkIoCompletionPassOutcome.AttemptLimitReached,
            NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion,
            completedIterations,
            true,
            AttemptLimitCompareRawFileOffset,
            StopFlagAfterPassReadRawFileOffset);
    }

    private static NetworkIoCompletionCaptureState State(bool? value, bool reached) =>
        reached
            ? value.HasValue
                ? NetworkIoCompletionCaptureState.Captured
                : NetworkIoCompletionCaptureState.Missing
            : value.HasValue
                ? NetworkIoCompletionCaptureState.SuppliedButUnused
                : NetworkIoCompletionCaptureState.NotReached;

    private static bool HasSuppliedButUnusedCapture(
        NetworkIoCompletionAttemptCaptureStates states) =>
        states.DequeueSucceeded == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.OverlappedPointerFromDequeue
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.CompletionKeyPointerFromDequeue
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.GetLastErrorAfterDequeue
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.DiagnosticReturnedNormally
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.CompletionKeyPointerAfterDiagnostic
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.OverlappedPointerAtSuccessDispatch
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.FailedCompletionError
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.CompletionKeyPointerAtErrorDispatch
            == NetworkIoCompletionCaptureState.SuppliedButUnused
        || states.CallbackReturnedNormally
            == NetworkIoCompletionCaptureState.SuppliedButUnused;

    private static NetworkIoCompletionCaptureState State(uint? value, bool reached) =>
        reached
            ? value.HasValue
                ? NetworkIoCompletionCaptureState.Captured
                : NetworkIoCompletionCaptureState.Missing
            : value.HasValue
                ? NetworkIoCompletionCaptureState.SuppliedButUnused
                : NetworkIoCompletionCaptureState.NotReached;

    private sealed class AttemptBuilder
    {
        public AttemptBuilder(
            int attemptNumber,
            int requestedTimeoutMilliseconds,
            bool hasFreshOutcomeCapture,
            NetworkIoCompletionAttemptCapture capture)
        {
            AttemptNumber = attemptNumber;
            RequestedTimeoutMilliseconds = requestedTimeoutMilliseconds;
            HasFreshOutcomeCapture = hasFreshOutcomeCapture;
            Capture = capture;
        }

        public int AttemptNumber { get; }
        public int RequestedTimeoutMilliseconds { get; }
        public bool HasFreshOutcomeCapture { get; }
        public NetworkIoCompletionAttemptCapture Capture { get; }
        public bool DequeueBooleanBranchReached { get; set; }
        public NetworkIoCompletionDiagnosticDecision DiagnosticDecision { get; set; } =
            NetworkIoCompletionDiagnosticDecision.NotEvaluated;
        public NetworkIoCompletionCallbackDecision CallbackDecision { get; set; } =
            NetworkIoCompletionCallbackDecision.NotEvaluated;
        public int? DispatchSlotOffset { get; set; }
        public bool CountedTowardAttemptLimit { get; set; }
        public bool FallbackTimeoutSelected { get; set; }
        public uint? LastReachedRawFileOffset { get; set; }
        public uint? StopBeforeRawFileOffset { get; set; }

        public NetworkIoCompletionAttemptPlan ToPlan(AttemptReach reached) =>
            new(
                AttemptNumber,
                RequestedTimeoutMilliseconds,
                HasFreshOutcomeCapture,
                Capture,
                new NetworkIoCompletionAttemptCaptureStates(
                    State(Capture.DequeueSucceeded, reached.DequeueSucceeded),
                    State(Capture.OverlappedPointerFromDequeue, reached.OverlappedPointerFromDequeue),
                    State(Capture.CompletionKeyPointerFromDequeue, reached.CompletionKeyPointerFromDequeue),
                    State(Capture.GetLastErrorAfterDequeue, reached.GetLastErrorAfterDequeue),
                    State(Capture.DiagnosticReturnedNormally, reached.DiagnosticReturnedNormally),
                    State(Capture.CompletionKeyPointerAfterDiagnostic, reached.CompletionKeyPointerAfterDiagnostic),
                    State(Capture.OverlappedPointerAtSuccessDispatch, reached.OverlappedPointerAtSuccessDispatch),
                    State(Capture.FailedCompletionError, reached.FailedCompletionError),
                    State(Capture.CompletionKeyPointerAtErrorDispatch, reached.CompletionKeyPointerAtErrorDispatch),
                    State(Capture.CallbackReturnedNormally, reached.CallbackReturnedNormally)),
                DequeueBooleanBranchReached,
                DiagnosticDecision,
                CallbackDecision,
                DispatchSlotOffset,
                CountedTowardAttemptLimit,
                FallbackTimeoutSelected,
                LastReachedRawFileOffset,
                StopBeforeRawFileOffset);
    }

    private sealed class AttemptReach
    {
        public bool DequeueSucceeded { get; set; }
        public bool OverlappedPointerFromDequeue { get; set; }
        public bool CompletionKeyPointerFromDequeue { get; set; }
        public bool GetLastErrorAfterDequeue { get; set; }
        public bool DiagnosticReturnedNormally { get; set; }
        public bool CompletionKeyPointerAfterDiagnostic { get; set; }
        public bool OverlappedPointerAtSuccessDispatch { get; set; }
        public bool FailedCompletionError { get; set; }
        public bool CompletionKeyPointerAtErrorDispatch { get; set; }
        public bool CallbackReturnedNormally { get; set; }
    }
}
