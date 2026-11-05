using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoCompletionPassChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pass anchors preserve the raw-to-PE mapping and native loop bounds",
            NetworkIoCompletionPassProjection.PumpThreadEntryRawFileOffset == 0xBBC860
            && NetworkIoCompletionPassProjection.PumpThreadEntryVirtualAddress == 0xFBD460
            && NetworkIoCompletionPassProjection.DequeueCallRawFileOffset == 0xBBC911
            && NetworkIoCompletionPassProjection.PostDequeueGetLastErrorCallRawFileOffset
                == 0xBBC916
            && NetworkIoCompletionPassProjection.FailedCompletionErrorCallRawFileOffset
                == 0xBBC973
            && NetworkIoCompletionPassProjection.GetQueuedCompletionStatusImportRawFileOffset
                == 0x16C2F2C
            && NetworkIoCompletionPassProjection.GetQueuedCompletionStatusImportVirtualAddress
                == 0x1AC432C
            && NetworkIoCompletionPassProjection.IatVirtualAddressDeltaFromRawFileOffset
                == 0x00401400
            && NetworkIoCompletionPassProjection.GetLastErrorAfterDequeueImportVirtualAddress
                == 0x1AC4378
            && NetworkIoCompletionPassProjection.GetLastErrorAfterDequeueImportRawFileOffset
                == 0x16C2F78
            && NetworkIoCompletionPassProjection.GetLastErrorAfterDequeueImportRawFileOffset
                + NetworkIoCompletionPassProjection.IatVirtualAddressDeltaFromRawFileOffset
                == NetworkIoCompletionPassProjection.GetLastErrorAfterDequeueImportVirtualAddress
            && NetworkIoCompletionPassProjection.GetLastErrorAfterDequeueImportModule
                == "KERNEL32.dll"
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportVirtualAddress
                == 0x1AC4DA0
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportRawFileOffset
                == 0x16C39A0
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportRawFileOffset
                + NetworkIoCompletionPassProjection.IatVirtualAddressDeltaFromRawFileOffset
                == NetworkIoCompletionPassProjection.FailedCompletionErrorImportVirtualAddress
            && NetworkIoCompletionPassProjection.GetQueuedCompletionStatusImportRawFileOffset
                + NetworkIoCompletionPassProjection.IatVirtualAddressDeltaFromRawFileOffset
                == NetworkIoCompletionPassProjection.GetQueuedCompletionStatusImportVirtualAddress
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportModule
                == "WSOCK32.dll"
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportOrdinal == 111
            && NetworkIoCompletionPassProjection.FailedCompletionErrorImportOrdinalThunk
                == 0x8000006F
            && NetworkIoCompletionPassProjection.MaximumAttempts == 100
            && NetworkIoCompletionPassProjection.InitialTimeoutMilliseconds == 0
            && NetworkIoCompletionPassProjection.FallbackTimeoutMilliseconds == 500);

        NetworkIoCompletionPassPlan missingEntry = Project(null, null, null);
        NetworkIoCompletionPassPlan stoppedAtEntry = Project(
            true,
            ImmutableArray.Create(Receive()),
            true);
        NetworkIoCompletionPassPlan cleanStopAtEntry = Project(true, null, null);
        check("Entry stop capture distinguishes unknown, unentered, and unused later captures",
            !missingEntry.PassEntered
            && missingEntry.Outcome == NetworkIoCompletionPassOutcome.MissingInitialStopFlag
            && missingEntry.StopFlagAtEntryState == NetworkIoCompletionCaptureState.Missing
            && missingEntry.LastReachedRawFileOffset == 0xBBC878
            && missingEntry.StopBeforeRawFileOffset == 0xBBC878
            && missingEntry.AttemptCapturesState == NetworkIoCompletionCaptureState.NotReached
            && !missingEntry.StopFlagAfterPassReadReached
            && stoppedAtEntry.Outcome == NetworkIoCompletionPassOutcome.EntryStopRequested
            && stoppedAtEntry.AttemptCapturesState
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && stoppedAtEntry.StopFlagAfterPassState
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && stoppedAtEntry.SuppliedStopFlagAfterPass == true
            && stoppedAtEntry.UnusedAttemptCaptures.Length == 1
            && stoppedAtEntry.Disposition
                == NetworkIoCompletionPassDisposition.BoundedBeforeInnerPass
            && cleanStopAtEntry.Disposition
                == NetworkIoCompletionPassDisposition.ExactCleanCompletion);

        NetworkIoCompletionPassPlan hundred = Project(
            false,
            Enumerable.Repeat(Receive(), 100).ToImmutableArray(),
            true);
        check("The pass counts one hundred normally returned dequeues then reaches its outer stop read",
            hundred.Outcome == NetworkIoCompletionPassOutcome.AttemptLimitReached
            && hundred.AttemptCount == 100
            && hundred.CompletedPassIterations == 100
            && hundred.TimeoutSequence.Length == 100
            && hundred.TimeoutSequence.All(timeout => timeout == 0)
            && hundred.StopFlagAfterPassReadReached
            && hundred.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Captured
            && hundred.SuppliedStopFlagAfterPass == true
            && hundred.Attempts.All(attempt =>
                attempt.CaptureStates.GetLastErrorAfterDequeue
                    == NetworkIoCompletionCaptureState.Captured)
            && hundred.Attempts[99].LastReachedRawFileOffset == 0xBBC9C2
            && hundred.LastReachedRawFileOffset == 0xBBC9CE
            && hundred.StopBeforeRawFileOffset == 0xBBC9D5
            && hundred.Disposition
                == NetworkIoCompletionPassDisposition.ExactCleanCompletion
            && hundred.Attempts.All(attempt =>
                !HasUnusedCaptures(attempt.CaptureStates)));

        NetworkIoCompletionPassPlan excessAttempt = Project(
            false,
            Enumerable.Repeat(Receive(), 101).ToImmutableArray(),
            false);
        check("Captures beyond the one-pass bound remain visible as unused",
            excessAttempt.AttemptCount == 100
            && excessAttempt.CompletedPassIterations == 100
            && excessAttempt.UnusedAttemptCaptures.Length == 1
            && excessAttempt.UnusedAttemptCaptures[0] == Receive()
            && excessAttempt.Disposition
                == NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion
            && excessAttempt.StopFlagAfterPassReadReached);

        NetworkIoCompletionPassPlan missingFinalStop = Project(
            false,
            Enumerable.Repeat(Receive(), 100).ToImmutableArray(),
            null);
        NetworkIoCompletionPassPlan extraSuccessOverlap = Project(
            false,
            Enumerable.Repeat(Receive(), 100)
                .ToImmutableArray()
                .SetItem(0, Success(overlappedPointerFromDequeue: 0x1234)),
            true);
        check("Missing final stop and unconsumed success-overlap data prevent exact clean completion",
            missingFinalStop.StopFlagAfterPassReadReached
            && missingFinalStop.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Missing
            && missingFinalStop.Disposition
                == NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion
            && extraSuccessOverlap.Attempts[0].CaptureStates.OverlappedPointerFromDequeue
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && extraSuccessOverlap.Disposition
                == NetworkIoCompletionPassDisposition.BoundedInnerPassCompletion);

        NetworkIoCompletionPassPlan ninetyNine = Project(
            false,
            Enumerable.Repeat(Receive(), 99).ToImmutableArray(),
            false);
        check("Ninety-nine records do not become a completed bound and leave the next outcome pending",
            ninetyNine.Outcome == NetworkIoCompletionPassOutcome.MissingAttemptCapture
            && ninetyNine.AttemptCount == 100
            && ninetyNine.CompletedPassIterations == 99
            && !ninetyNine.Attempts[99].HasFreshOutcomeCapture
            && ninetyNine.Attempts[99].CaptureStates.GetLastErrorAfterDequeue
                == NetworkIoCompletionCaptureState.Missing
            && ninetyNine.Attempts[99].CaptureStates.DequeueSucceeded
                == NetworkIoCompletionCaptureState.NotReached
            && !ninetyNine.Attempts[99].DequeueBooleanBranchReached
            && ninetyNine.StopFlagAfterPassState
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && ninetyNine.SuppliedStopFlagAfterPass == false
            && !ninetyNine.StopFlagAfterPassReadReached);

        NetworkIoCompletionPassPlan emptyExit = Project(
            false,
            Captures(
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError),
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError)),
            false);
        check("One 500 ms fallback is followed by early exit on the next empty timeout",
            emptyExit.Outcome
                == NetworkIoCompletionPassOutcome.EmptyFailureAfterFallbackConsumed
            && emptyExit.TimeoutSequence.SequenceEqual(new[] { 0, 500 })
            && emptyExit.Attempts[0].FallbackTimeoutSelected
            && !emptyExit.Attempts[1].FallbackTimeoutSelected
            && emptyExit.CompletedPassIterations == 1
            && emptyExit.StopFlagAfterPassReadReached
            && emptyExit.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Captured
            && emptyExit.SuppliedStopFlagAfterPass == false
            && emptyExit.Attempts[1].LastReachedRawFileOffset == 0xBBC94B
            && emptyExit.LastReachedRawFileOffset == 0xBBC9CE
            && emptyExit.StopBeforeRawFileOffset == 0xBBC9D5
            && emptyExit.Disposition
                == NetworkIoCompletionPassDisposition.ExactCleanCompletion);

        NetworkIoCompletionPassPlan fallbackReset = Project(
            false,
            Captures(
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError),
                Receive(),
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError)),
            null);
        check("A dispatch resets the timeout to zero without rearming the single fallback",
            fallbackReset.Outcome
                == NetworkIoCompletionPassOutcome.EmptyFailureAfterFallbackConsumed
            && fallbackReset.TimeoutSequence.SequenceEqual(new[] { 0, 500, 0 })
            && fallbackReset.Attempts[1].CallbackDecision
                == NetworkIoCompletionCallbackDecision.Receive
            && !fallbackReset.Attempts[2].FallbackTimeoutSelected
            && fallbackReset.StopFlagAfterPassReadReached
            && fallbackReset.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Missing
            && fallbackReset.SuppliedStopFlagAfterPass is null);

        NetworkIoCompletionPassPlan receiveFromFreshOverlapped = Project(
            false,
            Captures(Success(
                overlappedPointerFromDequeue: 0,
                completionKeyPointerFromDequeue: 0x5678,
                diagnosticReturnedNormally: true,
                completionKeyPointerAfterDiagnostic: 0,
                overlappedPointerAtDispatch: 0x9999)),
            null);
        NetworkIoCompletionPassPlan closeFromFreshOverlapped = Project(
            false,
            Captures(Success(
                overlappedPointerFromDequeue: 0x9999,
                completionKeyPointerFromDequeue: 0x5678,
                overlappedPointerAtDispatch: 0)),
            null);
        check("Success dispatch uses its fresh key and OVERLAPPED reads, not earlier pointer values",
            receiveFromFreshOverlapped.Attempts[0].DispatchSlotOffset == 0x18
            && receiveFromFreshOverlapped.Attempts[0].CallbackDecision
                == NetworkIoCompletionCallbackDecision.Receive
            && receiveFromFreshOverlapped.Attempts[0].CaptureStates.OverlappedPointerFromDequeue
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && receiveFromFreshOverlapped.Attempts[0].CaptureStates.DiagnosticReturnedNormally
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && receiveFromFreshOverlapped.Attempts[0].CaptureStates.CompletionKeyPointerAfterDiagnostic
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && receiveFromFreshOverlapped.Attempts[0].CaptureStates.OverlappedPointerAtSuccessDispatch
                == NetworkIoCompletionCaptureState.Captured
            && closeFromFreshOverlapped.Attempts[0].DispatchSlotOffset == 0x1C
            && closeFromFreshOverlapped.Attempts[0].CallbackDecision
                == NetworkIoCompletionCallbackDecision.Close);

        NetworkIoCompletionPassPlan refreshedSuccessKey = Project(
            false,
            Captures(Success(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                completionKeyPointerAfterDiagnostic: 0x8888,
                overlappedPointerAtDispatch: 0x9999)),
            null);
        NetworkIoCompletionPassPlan invalidSuccessKey = Project(
            false,
            Captures(Success(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                completionKeyPointerAfterDiagnostic: 0,
                overlappedPointerAtDispatch: 0x9999)),
            null);
        NetworkIoCompletionPassPlan invalidSuccessKeyMissingOverlap = Project(
            false,
            Captures(Success(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                completionKeyPointerAfterDiagnostic: 0,
                overlappedPointerAtDispatch: null)),
            null);
        check("A successful null-key diagnostic requires and uses a fresh post-diagnostic key",
            refreshedSuccessKey.Attempts[0].DiagnosticDecision
                == NetworkIoCompletionDiagnosticDecision.NullCompletionKey
            && refreshedSuccessKey.Attempts[0].CaptureStates.CompletionKeyPointerAfterDiagnostic
                == NetworkIoCompletionCaptureState.Captured
            && refreshedSuccessKey.Attempts[0].DispatchSlotOffset == 0x18
            && invalidSuccessKey.Outcome
                == NetworkIoCompletionPassOutcome.InvalidCompletionKeyPointerAfterDiagnostic
            && invalidSuccessKey.Disposition
                == NetworkIoCompletionPassDisposition.BoundedAtInvalidCompletionKey
            && invalidSuccessKey.LastReachedRawFileOffset == 0xBBC9AB
            && invalidSuccessKey.StopBeforeRawFileOffset == 0xBBC9AF
            && invalidSuccessKey.Attempts[0].DispatchSlotOffset is null
            && invalidSuccessKey.Attempts[0].CallbackDecision
                == NetworkIoCompletionCallbackDecision.NotEvaluated
            && invalidSuccessKey.Attempts[0].CaptureStates.OverlappedPointerAtSuccessDispatch
                == NetworkIoCompletionCaptureState.Captured
            && invalidSuccessKeyMissingOverlap.Outcome
                == NetworkIoCompletionPassOutcome.MissingOverlappedPointerAtSuccessDispatch
            && invalidSuccessKeyMissingOverlap.StopBeforeRawFileOffset == 0xBBC9AB
            && invalidSuccessKeyMissingOverlap.Attempts[0].CaptureStates.OverlappedPointerAtSuccessDispatch
                == NetworkIoCompletionCaptureState.Missing);

        NetworkIoCompletionPassPlan errorCallback = Project(
            false,
            Captures(FailedCompletion(
                completionKeyPointerFromDequeue: 0x5678,
                failedCompletionError: 10054,
                completionKeyPointerAtErrorDispatch: 0x8888,
                callbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan invalidErrorKey = Project(
            false,
            Captures(FailedCompletion(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                failedCompletionError: 10054,
                completionKeyPointerAtErrorDispatch: 0,
                callbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan abortedCompletion = Project(
            false,
            Captures(FailedCompletion(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                failedCompletionError:
                    NetworkIoCompletionPassProjection.OperationAbortedError,
                completionKeyPointerAtErrorDispatch: 0x8888,
                callbackReturnedNormally: null)),
            null);
        check("Failed completions use an opaque error capture, refresh the key, and bound null dereferences",
            errorCallback.Attempts[0].Capture.FailedCompletionError == 10054
            && errorCallback.Attempts[0].CaptureStates.FailedCompletionError
                == NetworkIoCompletionCaptureState.Captured
            && errorCallback.Attempts[0].CaptureStates.GetLastErrorAfterDequeue
                == NetworkIoCompletionCaptureState.Captured
            && errorCallback.Attempts[0].CaptureStates.CompletionKeyPointerAtErrorDispatch
                == NetworkIoCompletionCaptureState.Captured
            && errorCallback.Attempts[0].DispatchSlotOffset == 0x20
            && invalidErrorKey.Outcome
                == NetworkIoCompletionPassOutcome.InvalidCompletionKeyPointerAtErrorDispatch
            && invalidErrorKey.LastReachedRawFileOffset == 0xBBC97D
            && invalidErrorKey.StopBeforeRawFileOffset == 0xBBC981
            && invalidErrorKey.Attempts[0].DispatchSlotOffset is null
            && invalidErrorKey.Attempts[0].CallbackDecision
                == NetworkIoCompletionCallbackDecision.NotEvaluated
            && abortedCompletion.Attempts[0].CallbackDecision
                == NetworkIoCompletionCallbackDecision.OperationAbortedSkipped
            && abortedCompletion.Attempts[0].CaptureStates.CompletionKeyPointerAtErrorDispatch
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && abortedCompletion.Attempts[0].CaptureStates.CallbackReturnedNormally
                == NetworkIoCompletionCaptureState.NotReached);

        NetworkIoCompletionPassPlan missingPostDequeueError = Project(
            false,
            Captures(Success(
                overlappedPointerFromDequeue: 0x1234,
                getLastErrorAfterDequeue: null,
                callbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan missingPostDequeueErrorOnFailure = Project(
            false,
            Captures(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: false,
                OverlappedPointerFromDequeue: 0x1234,
                CompletionKeyPointerFromDequeue: 0x5678,
                GetLastErrorAfterDequeue: null,
                DiagnosticReturnedNormally: null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: null,
                FailedCompletionError: 10054,
                CompletionKeyPointerAtErrorDispatch: 0x8888,
                CallbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan missingQueueBoolean = Project(
            false,
            Captures(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: null,
                OverlappedPointerFromDequeue: 0,
                CompletionKeyPointerFromDequeue: 0x5678,
                GetLastErrorAfterDequeue: 0,
                DiagnosticReturnedNormally: null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: 0x9999,
                FailedCompletionError: null,
                CompletionKeyPointerAtErrorDispatch: null,
                CallbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan missingSuccessOverlap = Project(
            false,
            Captures(Success(overlappedPointerAtDispatch: null)),
            null);
        NetworkIoCompletionPassPlan missingFailedError = Project(
            false,
            Captures(FailedCompletion(
                completionKeyPointerFromDequeue: 0x5678,
                failedCompletionError: null,
                completionKeyPointerAtErrorDispatch: 0x8888,
                callbackReturnedNormally: true)),
            null);
        check("Missing captures stop at their native reads and later supplied fields stay unused",
            missingPostDequeueError.Outcome
                == NetworkIoCompletionPassOutcome.MissingGetLastErrorAfterDequeue
            && missingPostDequeueError.Attempts[0].CaptureStates.GetLastErrorAfterDequeue
                == NetworkIoCompletionCaptureState.Missing
            && missingPostDequeueError.Attempts[0].CaptureStates.DequeueSucceeded
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && missingPostDequeueError.Attempts[0].CaptureStates.OverlappedPointerFromDequeue
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && !missingPostDequeueError.Attempts[0].DequeueBooleanBranchReached
            && missingPostDequeueErrorOnFailure.Outcome
                == NetworkIoCompletionPassOutcome.MissingGetLastErrorAfterDequeue
            && missingPostDequeueErrorOnFailure.Attempts[0].CaptureStates.FailedCompletionError
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && missingPostDequeueErrorOnFailure.Attempts[0].CaptureStates.DequeueSucceeded
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && missingQueueBoolean.Outcome
                == NetworkIoCompletionPassOutcome.MissingDequeueBoolean
            && missingQueueBoolean.Attempts[0].CaptureStates.GetLastErrorAfterDequeue
                == NetworkIoCompletionCaptureState.Captured
            && !missingQueueBoolean.Attempts[0].DequeueBooleanBranchReached
            && missingQueueBoolean.Attempts[0].LastReachedRawFileOffset == 0xBBC920
            && missingQueueBoolean.Attempts[0].StopBeforeRawFileOffset == 0xBBC920
            && missingSuccessOverlap.Outcome
                == NetworkIoCompletionPassOutcome.MissingOverlappedPointerAtSuccessDispatch
            && missingFailedError.Outcome
                == NetworkIoCompletionPassOutcome.MissingFailedCompletionError
            && missingFailedError.Attempts[0].CaptureStates.CompletionKeyPointerAtErrorDispatch
                == NetworkIoCompletionCaptureState.SuppliedButUnused);

        NetworkIoCompletionPassPlan missingSuccessKeyReload = Project(
            false,
            Captures(Success(
                completionKeyPointerFromDequeue: 0,
                diagnosticReturnedNormally: true,
                completionKeyPointerAfterDiagnostic: null)),
            null);
        NetworkIoCompletionPassPlan missingErrorKeyReload = Project(
            false,
            Captures(FailedCompletion(
                completionKeyPointerFromDequeue: 0x5678,
                failedCompletionError: 10054,
                completionKeyPointerAtErrorDispatch: null,
                callbackReturnedNormally: true)),
            null);
        NetworkIoCompletionPassPlan missingCallback = Project(
            false,
            Captures(Success(callbackReturnedNormally: null)),
            null);
        check("Diagnostic refreshes and callback observations remain independently required",
            missingSuccessKeyReload.Outcome
                == NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerAfterDiagnostic
            && missingSuccessKeyReload.LastReachedRawFileOffset == 0xBBC9A5
            && missingSuccessKeyReload.StopBeforeRawFileOffset == 0xBBC9A5
            && missingSuccessKeyReload.Attempts[0].CaptureStates.OverlappedPointerAtSuccessDispatch
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && missingErrorKeyReload.Outcome
                == NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerAtErrorDispatch
            && missingErrorKeyReload.LastReachedRawFileOffset == 0xBBC97D
            && missingErrorKeyReload.StopBeforeRawFileOffset == 0xBBC97D
            && missingCallback.Outcome
                == NetworkIoCompletionPassOutcome.MissingCallbackReturnObservation
            && missingCallback.Attempts[0].CaptureStates.CallbackReturnedNormally
                == NetworkIoCompletionCaptureState.Missing
            && missingCallback.Attempts[0].StopBeforeRawFileOffset == 0xBBC9B6);

        NetworkIoCompletionPassPlan emptyErrorWithoutDiagnostic = Project(
            false,
            Captures(EmptyFailure(error: 5, diagnosticReturnedNormally: null)),
            null);
        NetworkIoCompletionPassPlan abnormalDiagnostic = Project(
            false,
            Captures(EmptyFailure(error: 5, diagnosticReturnedNormally: false)),
            null);
        check("Unexpected empty failures expose the native diagnostic decision and its return capture",
            emptyErrorWithoutDiagnostic.Outcome
                == NetworkIoCompletionPassOutcome.MissingDiagnosticReturnObservation
            && emptyErrorWithoutDiagnostic.Attempts[0].DiagnosticDecision
                == NetworkIoCompletionDiagnosticDecision.UnexpectedEmptyFailure
            && abnormalDiagnostic.Outcome
                == NetworkIoCompletionPassOutcome.DiagnosticDidNotReturnNormally
            && abnormalDiagnostic.Attempts[0].CaptureStates.DiagnosticReturnedNormally
                == NetworkIoCompletionCaptureState.Captured);

        NetworkIoCompletionPassPlan unreadEndStop = Project(
            false,
            Captures(
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError),
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError)),
            null);
        NetworkIoCompletionPassPlan zeroEndStop = Project(
            false,
            Captures(
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError),
                EmptyFailure(NetworkIoCompletionPassProjection.WaitTimeoutError)),
            false);
        check("The post-pass stop read distinguishes unreached, reached-but-unread, and captured zero",
            !stoppedAtEntry.StopFlagAfterPassReadReached
            && unreadEndStop.StopFlagAfterPassReadReached
            && unreadEndStop.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Missing
            && zeroEndStop.StopFlagAfterPassReadReached
            && zeroEndStop.StopFlagAfterPassState == NetworkIoCompletionCaptureState.Captured
            && zeroEndStop.SuppliedStopFlagAfterPass == false);
    }

    private static NetworkIoCompletionPassPlan Project(
        bool? stopFlagAtEntry,
        ImmutableArray<NetworkIoCompletionAttemptCapture>? attempts,
        bool? stopFlagAfterPass) =>
        NetworkIoCompletionPassProjection.Project(
            new NetworkIoCompletionPassCapture(
                stopFlagAtEntry,
                attempts,
                stopFlagAfterPass));

    private static ImmutableArray<NetworkIoCompletionAttemptCapture> Captures(
        params NetworkIoCompletionAttemptCapture[] attempts) =>
        ImmutableArray.CreateRange(attempts);

    private static NetworkIoCompletionAttemptCapture Receive(
        bool? callbackReturnedNormally = true) =>
        Success(callbackReturnedNormally: callbackReturnedNormally);

    private static NetworkIoCompletionAttemptCapture Success(
        uint? overlappedPointerFromDequeue = null,
        uint? completionKeyPointerFromDequeue = 0x5678,
        uint? getLastErrorAfterDequeue = 0,
        bool? diagnosticReturnedNormally = null,
        uint? completionKeyPointerAfterDiagnostic = null,
        uint? overlappedPointerAtDispatch = 0x9999,
        bool? callbackReturnedNormally = true) =>
        new(
            DequeueSucceeded: true,
            OverlappedPointerFromDequeue: overlappedPointerFromDequeue,
            CompletionKeyPointerFromDequeue: completionKeyPointerFromDequeue,
            GetLastErrorAfterDequeue: getLastErrorAfterDequeue,
            DiagnosticReturnedNormally: diagnosticReturnedNormally,
            CompletionKeyPointerAfterDiagnostic: completionKeyPointerAfterDiagnostic,
            OverlappedPointerAtSuccessDispatch: overlappedPointerAtDispatch,
            FailedCompletionError: null,
            CompletionKeyPointerAtErrorDispatch: null,
            CallbackReturnedNormally: callbackReturnedNormally);

    private static bool HasUnusedCaptures(
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

    private static NetworkIoCompletionAttemptCapture EmptyFailure(
        uint error,
        bool? diagnosticReturnedNormally = null) =>
        new(
            DequeueSucceeded: false,
            OverlappedPointerFromDequeue: 0,
            CompletionKeyPointerFromDequeue: null,
            GetLastErrorAfterDequeue: error,
            DiagnosticReturnedNormally: diagnosticReturnedNormally,
            CompletionKeyPointerAfterDiagnostic: null,
            OverlappedPointerAtSuccessDispatch: null,
            FailedCompletionError: null,
            CompletionKeyPointerAtErrorDispatch: null,
            CallbackReturnedNormally: null);

    private static NetworkIoCompletionAttemptCapture FailedCompletion(
        uint? completionKeyPointerFromDequeue,
        uint? failedCompletionError,
        uint? completionKeyPointerAtErrorDispatch,
        bool? diagnosticReturnedNormally = null,
        bool? callbackReturnedNormally = true) =>
        new(
            DequeueSucceeded: false,
            OverlappedPointerFromDequeue: 0x1234,
            CompletionKeyPointerFromDequeue: completionKeyPointerFromDequeue,
            GetLastErrorAfterDequeue: 0,
            DiagnosticReturnedNormally: diagnosticReturnedNormally,
            CompletionKeyPointerAfterDiagnostic: null,
            OverlappedPointerAtSuccessDispatch: null,
            FailedCompletionError: failedCompletionError,
            CompletionKeyPointerAtErrorDispatch: completionKeyPointerAtErrorDispatch,
            CallbackReturnedNormally: callbackReturnedNormally);
}
