using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The callback window and instruction anchors match the bounded native bytes",
            !NetworkIoErrorCallbackPreparationProjection.ExecutesNativeInstructions
            && NetworkIoErrorCallbackPreparationProjection.FailedCompletionErrorCallRawFileOffset
                == 0xBBC973
            && NetworkIoErrorCallbackPreparationProjection.FailedCompletionErrorCompareRawFileOffset
                == 0xBBC976
            && NetworkIoErrorCallbackPreparationProjection.DispatchKeyReadRawFileOffset
                == 0xBBC97D
            && NetworkIoErrorCallbackPreparationProjection.PushFailedCompletionErrorRawFileOffset
                == 0xBBC980
            && NetworkIoErrorCallbackPreparationProjection.FreshVptrReadRawFileOffset
                == 0xBBC981
            && NetworkIoErrorCallbackPreparationProjection.Slot20CallRawFileOffset
                == 0xBBC983
            && NetworkIoErrorCallbackPreparationProjection.OperationAbortedError == 995
            && NetworkIoErrorCallbackPreparationProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0xFF, 0x55, 0xE0,
                    0x3D, 0xE3, 0x03, 0x00, 0x00,
                    0x74, 0x3E,
                    0x8B, 0x4D, 0xFC,
                    0x50,
                    0x8B, 0x11,
                    0xFF, 0x52, 0x20,
                    0xEB, 0x33
                }));

        NetworkIoErrorCallbackPreparationInputs completeInputs = Inputs(
            CleanMissingKeyParent(),
            freshEbp: 0x1000,
            freshDispatchKey: 0,
            freshEsp: 0x2000,
            freshVptr: 0);
        NetworkIoErrorCallbackPreparationPlan complete =
            NetworkIoErrorCallbackPreparationProjection.Project(completeInputs);
        check("A clean missing-key prefix reaches only the pre-call slot-20 boundary",
            complete.ChildGateAdmitted
            && complete.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.Slot20CallBoundary
            && complete.BoundedCompletionBoundaryReached
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && complete.LastReachedRawFileOffset == 0xBBC981
            && complete.StopBeforeRawFileOffset == 0xBBC983
            && complete.FailedCompletionErrorEAX == 0x1111
            && complete.DispatchKeyPointerECX == 0
            && complete.FreshVptrEDX == 0
            && complete.StackPointerAtStop == 0x1FFC
            && complete.Slot20Address == 0x20
            && complete.ParentPassPlan.Attempts[0].Capture.GetLastErrorAfterDequeue
                == 0x2222
            && complete.ParentPassPlan.Attempts[0].Capture.FailedCompletionError == 0x1111
            && complete.Reads.Length == 2
            && complete.Reads[0].RawFileOffset == 0xBBC97D
            && complete.Reads[0].Address == 0xFFC
            && complete.Reads[0].Value == 0
            && complete.Reads[1].RawFileOffset == 0xBBC981
            && complete.Reads[1].Address == 0
            && complete.Reads[1].Value == 0
            && complete.Pushes.Length == 1
            && complete.Pushes[0].StackPointerBefore == 0x2000
            && complete.Pushes[0].StackPointerAfter == 0x1FFC
            && complete.Pushes[0].Value == 0x1111
            && complete.Pushes[0].Committed
            && complete.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[] { 0xBBC97D, 0xBBC980, 0xBBC981 })
            && complete.OrderedEffects.Length == 4
            && complete.OrderedEffects[0].ParentPassDecision == complete.ParentPassPlan
            && complete.OrderedEffects[1].Read == complete.Reads[0]
            && complete.OrderedEffects[2].Push == complete.Pushes[0]
            && complete.OrderedEffects[3].Read == complete.Reads[1]
            && !complete.Slot20TargetReadAttempted
            && !complete.Slot20CallAttempted
            && !complete.CallbackReturnObserved
            && complete.FlagsAtStop == new NetworkIoErrorCallbackPreparationCompareFlags(
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false));

        NetworkIoErrorCallbackPreparationPlan missingEbp =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: null,
                freshDispatchKey: 0x44,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("Missing EBP attempts the unknown 97D read and leaves the supplied key unused",
            missingEbp.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.MissingFreshEBP
            && missingEbp.Reads.Length == 1
            && missingEbp.Reads[0].Attempted
            && missingEbp.Reads[0].Address is null
            && !missingEbp.Reads[0].Captured
            && missingEbp.Reads[0].Value is null
            && missingEbp.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.Missing
            && missingEbp.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingEbp.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingEbp.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingEbp.FailedCompletionErrorEAX == 0x1111
            && missingEbp.FlagsAtStop == complete.FlagsAtStop
            && missingEbp.Pushes.IsEmpty
            && !missingEbp.BoundedCompletionBoundaryReached
            && !missingEbp.ExactBoundedCompletion
            && missingEbp.StopBeforeRawFileOffset == 0xBBC97D);

        NetworkIoErrorCallbackPreparationPlan missingEbpAndKey =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: null,
                freshDispatchKey: null,
                freshEsp: null,
                freshVptr: null));
        check("An absent key capture is unreached when EBP leaves its address unknown",
            missingEbpAndKey.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.Missing
            && missingEbpAndKey.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Unreached
            && missingEbpAndKey.Reads[0].Address is null
            && !missingEbpAndKey.Reads[0].Captured
            && missingEbpAndKey.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.MissingFreshEBP);

        NetworkIoErrorCallbackPreparationPlan missingKey =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 3,
                freshDispatchKey: null,
                freshEsp: 2,
                freshVptr: 0xFFFFFFF0));
        check("A missing fresh dispatch-key DWORD stops at 97D without inheriting the parent boundary",
            missingKey.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.MissingFreshDispatchKeyDWORD
            && missingKey.Reads[0].Address == uint.MaxValue
            && missingKey.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Missing
            && missingKey.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingKey.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingKey.Pushes.IsEmpty
            && !missingKey.BoundedCompletionBoundaryReached
            && !missingKey.ExactBoundedCompletion
            && !missingKey.ExactCleanCompletion
            && missingKey.StopBeforeRawFileOffset == 0xBBC97D);

        NetworkIoErrorCallbackPreparationPlan missingEsp =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: null,
                freshVptr: 0x80));
        check("The error push at 980 requires fresh ESP and does not consume the later vptr",
            missingEsp.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.MissingFreshESP
            && missingEsp.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && missingEsp.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && missingEsp.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.Missing
            && missingEsp.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && missingEsp.Pushes.Length == 1
            && missingEsp.Pushes[0].Attempted
            && !missingEsp.Pushes[0].Committed
            && missingEsp.Pushes[0].Value == 0x1111
            && missingEsp.StopBeforeRawFileOffset == 0xBBC980
            && missingEsp.FlagsAtStop == complete.FlagsAtStop
            && !missingEsp.ExactBoundedCompletion);

        NetworkIoErrorCallbackPreparationPlan missingVptr =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: null));
        check("The fresh vptr DWORD is required at 981 after the error push",
            missingVptr.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.MissingFreshVptrDWORD
            && missingVptr.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Missing
            && missingVptr.Reads.Length == 2
            && missingVptr.Reads[1].Address == 0x40
            && missingVptr.Reads[1].Attempted
            && !missingVptr.Reads[1].Captured
            && missingVptr.Pushes[0].Committed
            && missingVptr.StopBeforeRawFileOffset == 0xBBC981
            && missingVptr.FlagsAtStop == complete.FlagsAtStop
            && !missingVptr.ExactBoundedCompletion);

        NetworkIoErrorCallbackPreparationPlan wrappedAddresses =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 3,
                freshDispatchKey: 0xDEAD,
                freshEsp: 2,
                freshVptr: 0xFFFFFFF0));
        check("Fresh stack and slot address arithmetic wraps independently",
            wrappedAddresses.Reads[0].Address == uint.MaxValue
            && wrappedAddresses.Reads[1].Address == 0xDEAD
            && wrappedAddresses.Pushes[0].StackPointerAfter == 0xFFFFFFFE
            && wrappedAddresses.Slot20Address == 0x10
            && wrappedAddresses.ParentPassPlan.Attempts[0].Capture
                .CompletionKeyPointerFromDequeue == 0x88
            && wrappedAddresses.DispatchKeyPointerECX == 0xDEAD);

        NetworkIoErrorCallbackPreparationPlan aliasedAddressValues =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1004,
                freshDispatchKey: 0x1FFC,
                freshEsp: 0x2000,
                freshVptr: 0x77));
        check("Aliased stack-write and fresh-vptr addresses retain independent captured values",
            aliasedAddressValues.Reads[0].Address == 0x1000
            && aliasedAddressValues.DispatchKeyPointerECX == 0x1FFC
            && aliasedAddressValues.Pushes[0].StackPointerAfter == 0x1FFC
            && aliasedAddressValues.Pushes[0].Value == 0x1111
            && aliasedAddressValues.Reads[1].Address == 0x1FFC
            && aliasedAddressValues.Reads[1].Value == 0x77
            && aliasedAddressValues.FreshVptrEDX == 0x77
            && aliasedAddressValues.FreshVptrEDX != aliasedAddressValues.Pushes[0].Value);

        check("All captured failed-error values derive and preserve full CMP flags through PUSH",
            HasFlagsAndPush(0,
                new(true, true, true, false, true, false))
            && HasFlagsAndPush(1,
                new(true, true, true, false, true, false))
            && HasFlagsAndPush(994,
                new(true, true, true, false, true, false))
            && HasFlagsAndPush(996,
                new(false, false, false, false, false, false))
            && HasFlagsAndPush(uint.MaxValue,
                new(false, false, false, false, true, false))
            && HasFlagsAndPush(0x80000000,
                new(false, true, true, false, false, true)));

        NetworkIoErrorCallbackPreparationPlan dirtyParent =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                ParentWithUnusedCallbackCapture(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("Parent supplied-but-unused callback data rejects admission without projecting child effects",
            dirtyParent.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.ParentPassBoundaryNotClean
            && !dirtyParent.ChildGateAdmitted
            && dirtyParent.ParentPassPlan.Attempts[0].CaptureStates.CallbackReturnedNormally
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.Pushes.IsEmpty
            && dirtyParent.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && dirtyParent.FailedCompletionErrorEAX is null
            && dirtyParent.FlagsAtStop == complete.FlagsAtStop
            && !dirtyParent.ExactBoundedCompletion);

        NetworkIoErrorCallbackPreparationPlan extraParentAttempt =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                ParentWithExtraAttemptCapture(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("An invented attempt after the actual missing-key prefix remains unused and rejects admission",
            extraParentAttempt.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.ParentPassBoundaryNotClean
            && extraParentAttempt.ParentPassPlan.Attempts.Length == 1
            && extraParentAttempt.ParentPassPlan.UnusedAttemptCaptures.Length == 1
            && extraParentAttempt.ParentPassPlan.AttemptCapturesState
                == NetworkIoCompletionCaptureState.Captured
            && extraParentAttempt.FlagsAtStop == complete.FlagsAtStop
            && extraParentAttempt.Reads.IsEmpty
            && extraParentAttempt.Pushes.IsEmpty
            && extraParentAttempt.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused);

        NetworkIoErrorCallbackPreparationPlan abortedParent =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(failedError: 995),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        NetworkIoErrorCallbackPreparationPlan callbackAlreadyReturned =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                ParentWithDispatchKeyAndReturnedCallback(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("The parent 995 and already-returned-callback paths are rejected with all child inputs unused",
            !abortedParent.ChildGateAdmitted
            && abortedParent.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.ParentPassNotAtMissingDispatchKeyBoundary
            && abortedParent.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && abortedParent.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && abortedParent.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && abortedParent.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && abortedParent.Reads.IsEmpty
            && abortedParent.Pushes.IsEmpty
            && abortedParent.FailedCompletionErrorEAX is null
            && !callbackAlreadyReturned.ChildGateAdmitted
            && callbackAlreadyReturned.ParentPassPlan.Attempts[0].Capture.CallbackReturnedNormally
                == true
            && callbackAlreadyReturned.Reads.IsEmpty
            && callbackAlreadyReturned.Pushes.IsEmpty
            && callbackAlreadyReturned.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && callbackAlreadyReturned.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && callbackAlreadyReturned.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && callbackAlreadyReturned.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused);

        NetworkIoErrorCallbackPreparationPlan parentBound =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                new NetworkIoCompletionPassCapture(true, null, null),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("Only a new 983 endpoint or an already completed parent bound is exact-bounded",
            !parentBound.ChildGateAdmitted
            && parentBound.ParentPassPlan.Disposition
                == NetworkIoCompletionPassDisposition.ExactCleanCompletion
            && parentBound.ExactBoundedCompletion
            && !parentBound.BoundedCompletionBoundaryReached
            && !parentBound.ExactCleanCompletion
            && parentBound.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused);

        NetworkIoErrorCallbackPreparationPlan diagnosticParent =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(completionKeyFromDequeue: 0),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        check("A normally returned null-key diagnostic remains part of the clean parent prefix",
            diagnosticParent.ChildGateAdmitted
            && diagnosticParent.ParentPassPlan.Attempts[0].CaptureStates
                .DiagnosticReturnedNormally == NetworkIoCompletionCaptureState.Captured
            && diagnosticParent.ParentPassPlan.Attempts[0].Capture
                .DiagnosticReturnedNormally == true
            && diagnosticParent.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.Slot20CallBoundary);

        NetworkIoErrorCallbackPreparationPlan reprojection =
            NetworkIoErrorCallbackPreparationProjection.Project(completeInputs);
        check("Immutable parent-attempt inputs reproject deterministically",
            reprojection.Outcome == complete.Outcome
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.Reads.SequenceEqual(complete.Reads)
            && reprojection.Pushes.SequenceEqual(complete.Pushes)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.OrderedEffects.Length == complete.OrderedEffects.Length
            && reprojection.OrderedEffects[0].ParentPassDecision?.Attempts.SequenceEqual(
                complete.ParentPassPlan.Attempts) == true
            && reprojection.OrderedEffects.Skip(1).SequenceEqual(
                complete.OrderedEffects.Skip(1))
            && reprojection.ParentPassPlan.Attempts.SequenceEqual(
                complete.ParentPassPlan.Attempts)
            && complete.Capture.ParentPassCapture.AttemptCaptures!.Value.Length == 1
            && complete.Capture.ParentPassCapture.AttemptCaptures.Value[0]
                == complete.ParentPassPlan.Attempts[0].Capture);
    }

    private static NetworkIoErrorCallbackPreparationInputs Inputs(
        NetworkIoCompletionPassCapture parent,
        uint? freshEbp,
        uint? freshDispatchKey,
        uint? freshEsp,
        uint? freshVptr) =>
        new(parent, freshEbp, freshDispatchKey, freshEsp, freshVptr);

    private static NetworkIoCompletionPassCapture CleanMissingKeyParent(
        uint failedError = 0x1111,
        uint completionKeyFromDequeue = 0x88) =>
        new(
            StopFlagAtEntry: false,
            AttemptCaptures: ImmutableArray.Create(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: false,
                OverlappedPointerFromDequeue: 0x1234,
                CompletionKeyPointerFromDequeue: completionKeyFromDequeue,
                GetLastErrorAfterDequeue: 0x2222,
                DiagnosticReturnedNormally: completionKeyFromDequeue == 0 ? true : null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: null,
                FailedCompletionError: failedError,
                CompletionKeyPointerAtErrorDispatch: null,
                CallbackReturnedNormally: null)),
            StopFlagAfterPass: null);

    private static NetworkIoCompletionPassCapture ParentWithUnusedCallbackCapture() =>
        new(
            StopFlagAtEntry: false,
            AttemptCaptures: ImmutableArray.Create(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: false,
                OverlappedPointerFromDequeue: 0x1234,
                CompletionKeyPointerFromDequeue: 0x88,
                GetLastErrorAfterDequeue: 0x2222,
                DiagnosticReturnedNormally: null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: null,
                FailedCompletionError: 0x1111,
                CompletionKeyPointerAtErrorDispatch: null,
                CallbackReturnedNormally: true)),
            StopFlagAfterPass: null);

    private static NetworkIoCompletionPassCapture ParentWithExtraAttemptCapture()
    {
        NetworkIoCompletionAttemptCapture actual =
            CleanMissingKeyParent().AttemptCaptures!.Value[0];
        return new(
            StopFlagAtEntry: false,
            AttemptCaptures: ImmutableArray.Create(actual, default),
            StopFlagAfterPass: null);
    }

    private static NetworkIoCompletionPassCapture ParentWithDispatchKeyAndReturnedCallback() =>
        new(
            StopFlagAtEntry: false,
            AttemptCaptures: ImmutableArray.Create(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: false,
                OverlappedPointerFromDequeue: 0x1234,
                CompletionKeyPointerFromDequeue: 0x88,
                GetLastErrorAfterDequeue: 0x2222,
                DiagnosticReturnedNormally: null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: null,
                FailedCompletionError: 0x1111,
                CompletionKeyPointerAtErrorDispatch: 0x40,
                CallbackReturnedNormally: true)),
            StopFlagAfterPass: null);

    private static bool HasFlagsAndPush(
        uint failedError,
        NetworkIoErrorCallbackPreparationCompareFlags expectedFlags)
    {
        NetworkIoErrorCallbackPreparationPlan plan =
            NetworkIoErrorCallbackPreparationProjection.Project(Inputs(
                CleanMissingKeyParent(failedError),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0x80));
        return plan.FlagsAtStop == expectedFlags
            && plan.Outcome == NetworkIoErrorCallbackPreparationOutcome.Slot20CallBoundary
            && plan.Pushes.Length == 1
            && plan.Pushes[0].Committed
            && plan.Pushes[0].Value == failedError
            && plan.FailedCompletionErrorEAX == failedError
            && plan.Slot20TargetReadAttempted == false
            && plan.Slot20CallAttempted == false;
    }
}
