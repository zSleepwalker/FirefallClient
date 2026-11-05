using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The callback return window records the exact bounded bytes and branch target",
            !NetworkIoErrorCallbackReturnProjection.ExecutesNativeInstructions
            && NetworkIoErrorCallbackReturnProjection.Slot20CallRawFileOffset == 0xBBC983
            && NetworkIoErrorCallbackReturnProjection.ReturnJumpRawFileOffset == 0xBBC986
            && NetworkIoErrorCallbackReturnProjection.ReturnJumpNextInstructionRawFileOffset
                == 0xBBC988
            && NetworkIoErrorCallbackReturnProjection.ReturnJumpDisplacement == 0x33
            && NetworkIoErrorCallbackReturnProjection.ReturnJumpTargetRawFileOffset == 0xBBC9BB
            && NetworkIoErrorCallbackReturnProjection.ReturnJumpTargetVirtualAddress == 0xFBD5BB
            && NetworkIoErrorCallbackReturnProjection.WindowBytes.SequenceEqual(
                new byte[] { 0xFF, 0x52, 0x20, 0xEB, 0x33 }));

        NetworkIoErrorCallbackReturnInputs zeroTargetInputs = Inputs(
            CleanMissingKeyParent(),
            freshSlot20Target: 0,
            opaqueCallReturnedNormally: true);
        NetworkIoErrorCallbackReturnPlan zeroTarget =
            NetworkIoErrorCallbackReturnProjection.Project(zeroTargetInputs);
        check("A captured zero target is distinct from a missing target and remains opaque",
            zeroTarget.ChildGateAdmitted
            && zeroTarget.Outcome
                == NetworkIoErrorCallbackReturnOutcome.OpaqueCallReturnedNormally
            && zeroTarget.CaptureStates.FreshSlot20TargetDWORD
                == NetworkIoErrorCallbackReturnCaptureState.Captured
            && zeroTarget.TargetReadAttempted
            && zeroTarget.TargetReadCaptured
            && zeroTarget.TargetReads.Length == 1
            && zeroTarget.TargetReads[0].Address == 0xA0
            && zeroTarget.TargetReads[0].Width == 4
            && zeroTarget.TargetReads[0].Value == 0
            && !zeroTarget.TargetReads[0].NativeReadExecuted
            && zeroTarget.LogicalCallAttempted
            && zeroTarget.CallBoundaries.Length == 1
            && zeroTarget.CallBoundaries[0].TargetPointer == 0
            && zeroTarget.CallBoundaries[0].DispatchKeyPointerECX == 0x40
            && zeroTarget.CallBoundaries[0].FailedCompletionErrorEAX == 0x1111
            && zeroTarget.CallBoundaries[0].PreCallStackPointer == 0x1FFC
            && zeroTarget.CallBoundaries[0].NativeCallExecuted == false
            && zeroTarget.OpaqueCallReturnedNormally == true
            && zeroTarget.FlagsAtStop is null
            && zeroTarget.ChildBoundedCompletionBoundaryReached
            && zeroTarget.LastReachedRawFileOffset == 0xBBC986
            && zeroTarget.StopBeforeRawFileOffset == 0xBBC9BB
            && zeroTarget.ExactBoundedCompletion
            && zeroTarget.ExactCleanCompletion
            && !zeroTarget.NativeInstructionsExecuted
            && !zeroTarget.NativeTargetReadExecuted
            && !zeroTarget.NativeCallExecuted);

        NetworkIoErrorCallbackReturnPlan maximumTarget =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshSlot20Target: uint.MaxValue,
                opaqueCallReturnedNormally: true));
        check("The maximum captured target remains an opaque DWORD with no identity claim",
            maximumTarget.TargetReads[0].Value == uint.MaxValue
            && maximumTarget.CallBoundaries[0].TargetPointer == uint.MaxValue
            && maximumTarget.ExactCleanCompletion
            && maximumTarget.CallBoundaries[0].NativeCallExecuted == false);

        NetworkIoErrorCallbackReturnPlan wrappedSlot =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0x2000,
                freshVptr: 0xFFFFFFF0,
                freshSlot20Target: 0x12345678,
                opaqueCallReturnedNormally: false));
        check("The slot address wraps as a DWORD and the target read stays distinct from the error push",
            wrappedSlot.ParentPreparationPlan.Slot20Address == 0x10
            && wrappedSlot.TargetReads[0].Address == 0x10
            && wrappedSlot.TargetReads[0].Value == 0x12345678
            && wrappedSlot.ParentPreparationPlan.Pushes[0].StackPointerAfter == 0x1FFC
            && wrappedSlot.TargetReads[0].Address
                != wrappedSlot.ParentPreparationPlan.Pushes[0].StackPointerAfter
            && wrappedSlot.CallBoundaries[0].TargetPointer == 0x12345678
            && wrappedSlot.OpaqueCallReturnedNormally == false
            && wrappedSlot.FlagsAtStop
                == wrappedSlot.ParentPreparationPlan.FlagsAtStop
            && !wrappedSlot.ChildBoundedCompletionBoundaryReached
            && !wrappedSlot.ExactCleanCompletion
            && wrappedSlot.StopBeforeRawFileOffset == 0xBBC983);

        NetworkIoErrorCallbackReturnPlan missingTarget =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshSlot20Target: null,
                opaqueCallReturnedNormally: true));
        check("A missing target read stops before the call and leaves its supplied return status unused",
            missingTarget.Outcome
                == NetworkIoErrorCallbackReturnOutcome.MissingFreshSlot20TargetDWORD
            && missingTarget.TargetReadAttempted
            && !missingTarget.TargetReadCaptured
            && missingTarget.TargetReads.Length == 1
            && missingTarget.TargetReads[0].Address == 0xA0
            && missingTarget.TargetReads[0].Value is null
            && !missingTarget.LogicalCallAttempted
            && missingTarget.CallBoundaries.IsEmpty
            && missingTarget.CaptureStates.FreshSlot20TargetDWORD
                == NetworkIoErrorCallbackReturnCaptureState.Missing
            && missingTarget.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && missingTarget.OpaqueCallReturnedNormally is null
            && missingTarget.FlagsAtStop
                == missingTarget.ParentPreparationPlan.FlagsAtStop
            && missingTarget.StopBeforeRawFileOffset == 0xBBC983
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.BoundedCompletionBoundaryReached
            && !missingTarget.ExactCleanCompletion);

        NetworkIoErrorCallbackReturnPlan pendingReturn =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshSlot20Target: 0xBEEF,
                opaqueCallReturnedNormally: null));
        NetworkIoErrorCallbackReturnPlan falseReturn =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshSlot20Target: 0xBEEF,
                opaqueCallReturnedNormally: false));
        check("Pending or false normal-return observations leave flags and the stop at the parent call boundary",
            pendingReturn.Outcome
                == NetworkIoErrorCallbackReturnOutcome.MissingOpaqueCallReturnObservation
            && pendingReturn.TargetReadCaptured
            && pendingReturn.LogicalCallAttempted
            && pendingReturn.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.Missing
            && !pendingReturn.CallReturnObservationCaptured
            && pendingReturn.OpaqueCallReturnedNormally is null
            && pendingReturn.FlagsAtStop == pendingReturn.ParentPreparationPlan.FlagsAtStop
            && pendingReturn.StopBeforeRawFileOffset == 0xBBC983
            && !pendingReturn.ChildBoundedCompletionBoundaryReached
            && !pendingReturn.ExactBoundedCompletion
            && !pendingReturn.BoundedCompletionBoundaryReached
            && !pendingReturn.ExactCleanCompletion
            && falseReturn.Outcome
                == NetworkIoErrorCallbackReturnOutcome.OpaqueCallDidNotReturnNormally
            && falseReturn.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.Captured
            && falseReturn.CallReturnObservationCaptured
            && falseReturn.FlagsAtStop == falseReturn.ParentPreparationPlan.FlagsAtStop
            && falseReturn.StopBeforeRawFileOffset == 0xBBC983
            && !falseReturn.ChildBoundedCompletionBoundaryReached
            && !falseReturn.ExactBoundedCompletion
            && !falseReturn.BoundedCompletionBoundaryReached
            && !falseReturn.ExactCleanCompletion);

        NetworkIoErrorCallbackReturnPlan dirtyParent =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                ParentWithUnusedCallbackCapture(),
                freshSlot20Target: 0x80,
                opaqueCallReturnedNormally: true));
        NetworkIoErrorCallbackReturnPlan missingParentESP =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: null,
                freshVptr: 0x80,
                freshSlot20Target: 0x80,
                opaqueCallReturnedNormally: true));
        check("Rejected dirty or incomplete parents retain their own bound and do not invent child effects",
            dirtyParent.Outcome
                == NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotClean
            && !dirtyParent.ChildGateAdmitted
            && dirtyParent.TargetReads.IsEmpty
            && dirtyParent.CallBoundaries.IsEmpty
            && dirtyParent.CaptureStates.FreshSlot20TargetDWORD
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && dirtyParent.ParentPreparationPlan.StopBeforeRawFileOffset == 0xBBC97D
            && dirtyParent.LastReachedRawFileOffset
                == dirtyParent.ParentPreparationPlan.LastReachedRawFileOffset
            && dirtyParent.StopBeforeRawFileOffset
                == dirtyParent.ParentPreparationPlan.StopBeforeRawFileOffset
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentPreparationPlan.ExactBoundedCompletion
            && !dirtyParent.ExactCleanCompletion
            && missingParentESP.Outcome
                == NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotAtSlot20Boundary
            && missingParentESP.TargetReads.IsEmpty
            && missingParentESP.CallBoundaries.IsEmpty
            && missingParentESP.CaptureStates.FreshSlot20TargetDWORD
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && missingParentESP.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && missingParentESP.ExactBoundedCompletion
                == missingParentESP.ParentPreparationPlan.ExactBoundedCompletion
            && !missingParentESP.ExactCleanCompletion);

        NetworkIoErrorCallbackReturnPlan missingParentPointer =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: null,
                freshEsp: 0x2000,
                freshVptr: 0x80,
                freshSlot20Target: 0x80,
                opaqueCallReturnedNormally: true));
        check("A parent stopped at the missing dispatch-key pointer preserves its real bounded endpoint",
            missingParentPointer.Outcome
                == NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotAtSlot20Boundary
            && missingParentPointer.TargetReads.IsEmpty
            && missingParentPointer.CallBoundaries.IsEmpty
            && missingParentPointer.ParentPreparationPlan.StopBeforeRawFileOffset == 0xBBC97D
            && missingParentPointer.ExactBoundedCompletion
                == missingParentPointer.ParentPreparationPlan.ExactBoundedCompletion
            && !missingParentPointer.ExactCleanCompletion);

        NetworkIoErrorCallbackReturnPlan allNewCapturesUnused =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                new NetworkIoCompletionPassCapture(true, null, null),
                freshSlot20Target: 0x80,
                opaqueCallReturnedNormally: true));
        check("A parent that never enters the pass leaves every new capture supplied but unused",
            !allNewCapturesUnused.ChildGateAdmitted
            && allNewCapturesUnused.TargetReads.IsEmpty
            && allNewCapturesUnused.CallBoundaries.IsEmpty
            && allNewCapturesUnused.CaptureStates.FreshSlot20TargetDWORD
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && allNewCapturesUnused.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            && allNewCapturesUnused.ExactBoundedCompletion
                == allNewCapturesUnused.ParentPreparationPlan.ExactBoundedCompletion
            && !allNewCapturesUnused.ExactCleanCompletion);

        NetworkIoErrorCallbackReturnPlan callWithoutFreshStatus =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshSlot20Target: 0x80,
                opaqueCallReturnedNormally: null));
        check("An attempted opaque call with no return observation leaves that capture missing",
            callWithoutFreshStatus.LogicalCallAttempted
            && callWithoutFreshStatus.CallBoundaries.Length == 1
            && callWithoutFreshStatus.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackReturnCaptureState.Missing
            && !callWithoutFreshStatus.CallReturnObservationCaptured
            && callWithoutFreshStatus.OpaqueCallReturnedNormally is null);

        NetworkIoErrorCallbackReturnPlan aliasedRead =
            NetworkIoErrorCallbackReturnProjection.Project(Inputs(
                CleanMissingKeyParent(),
                freshEbp: 0x1000,
                freshDispatchKey: 0x40,
                freshEsp: 0xA4,
                freshVptr: 0x80,
                freshSlot20Target: 0xDEADBEEF,
                opaqueCallReturnedNormally: true));
        check("A slot target read may alias the parent's pushed-error address without inheriting its value",
            aliasedRead.ParentPreparationPlan.Pushes[0].StackPointerAfter == 0xA0
            && aliasedRead.TargetReads[0].Address == 0xA0
            && aliasedRead.TargetReads[0].Value == 0xDEADBEEF
            && aliasedRead.TargetReads[0].Value
                != aliasedRead.ParentPreparationPlan.Pushes[0].Value
            && aliasedRead.CallBoundaries[0].TargetPointer == 0xDEADBEEF);

        NetworkIoErrorCallbackReturnPlan complete =
            NetworkIoErrorCallbackReturnProjection.Project(zeroTargetInputs);
        int parentEffectCount = complete.ParentPreparationPlan.OrderedEffects.Length;
        check("The parent ordered trace remains an unchanged prefix before target, call, and return effects",
            complete.OrderedEffects.Length == parentEffectCount + 4
            && complete.OrderedEffects.Take(parentEffectCount)
                .Select(effect => effect.ParentPreparationEffect)
                .SequenceEqual(complete.ParentPreparationPlan.OrderedEffects
                    .Select(effect => (NetworkIoErrorCallbackPreparationOrderedEffect?)effect))
            && complete.OrderedEffects[parentEffectCount].TargetRead == complete.TargetReads[0]
            && complete.OrderedEffects[parentEffectCount + 1].CallBoundary
                == complete.CallBoundaries[0]
            && complete.OrderedEffects[parentEffectCount + 2].OpaqueCallReturnedNormally == true
            && complete.OrderedEffects[parentEffectCount + 3].JumpRawFileOffset == 0xBBC986
            && complete.OrderedEffects[parentEffectCount + 3].JumpTargetRawFileOffset
                == 0xBBC9BB
            && !complete.ReturnJumpTargetEntered
            && complete.InstructionOrder.Select(step => step.Kind).SequenceEqual(
                new[]
                {
                    NetworkIoErrorCallbackReturnStepKind.ReadFreshSlot20TargetDWORD,
                    NetworkIoErrorCallbackReturnStepKind.OpaqueCallBoundary,
                    NetworkIoErrorCallbackReturnStepKind.OpaqueCallReturnObservation,
                    NetworkIoErrorCallbackReturnStepKind.ReturnJump
                }));

        NetworkIoErrorCallbackReturnPlan reprojection =
            NetworkIoErrorCallbackReturnProjection.Project(zeroTargetInputs);
        check("Immutable return inputs reproject the same parent prefix and observations",
            reprojection.Outcome == complete.Outcome
            && reprojection.Capture == complete.Capture
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.TargetReads.SequenceEqual(complete.TargetReads)
            && reprojection.CallBoundaries.SequenceEqual(complete.CallBoundaries)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.OrderedEffects.Skip(parentEffectCount).SequenceEqual(
                complete.OrderedEffects.Skip(parentEffectCount))
            && reprojection.ParentPreparationPlan.OrderedEffects
                .Select(effect => (effect.Sequence, effect.Read, effect.Push))
                .SequenceEqual(complete.ParentPreparationPlan.OrderedEffects
                    .Select(effect => (effect.Sequence, effect.Read, effect.Push)))
            && reprojection.ParentPreparationPlan.ParentPassPlan.Outcome
                == complete.ParentPreparationPlan.ParentPassPlan.Outcome
            && reprojection.ParentPreparationPlan.FlagsAtStop
                == complete.ParentPreparationPlan.FlagsAtStop);
    }

    private static NetworkIoErrorCallbackReturnInputs Inputs(
        NetworkIoCompletionPassCapture parent,
        uint? freshSlot20Target,
        bool? opaqueCallReturnedNormally) =>
        new(PreparationInputs(parent), freshSlot20Target, opaqueCallReturnedNormally);

    private static NetworkIoErrorCallbackReturnInputs Inputs(
        NetworkIoCompletionPassCapture parent,
        uint? freshEbp,
        uint? freshDispatchKey,
        uint? freshEsp,
        uint? freshVptr,
        uint? freshSlot20Target,
        bool? opaqueCallReturnedNormally) =>
        new(
            PreparationInputs(parent, freshEbp, freshDispatchKey, freshEsp, freshVptr),
            freshSlot20Target,
            opaqueCallReturnedNormally);

    private static NetworkIoErrorCallbackPreparationInputs PreparationInputs(
        NetworkIoCompletionPassCapture parent,
        uint? freshEbp = 0x1000,
        uint? freshDispatchKey = 0x40,
        uint? freshEsp = 0x2000,
        uint? freshVptr = 0x80) =>
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
}
