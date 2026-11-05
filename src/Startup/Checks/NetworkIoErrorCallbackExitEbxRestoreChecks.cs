using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackExitEbxRestoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded exit window stops before MOV ESP, EBP",
            !NetworkIoErrorCallbackExitEbxRestoreProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackExitEbxRestoreProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackExitEbxRestoreProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackExitEbxRestoreProjection.ExecutesNextInstruction
            && !NetworkIoErrorCallbackExitEbxRestoreProjection.ExecutesCalls
            && NetworkIoErrorCallbackExitEbxRestoreProjection.PopEBXRawFileOffset
                == 0xBBC9E3
            && NetworkIoErrorCallbackExitEbxRestoreProjection.NextInstructionRawFileOffset
                == 0xBBC9E4
            && NetworkIoErrorCallbackExitEbxRestoreProjection.PopEBXOpcode == 0x5B
            && NetworkIoErrorCallbackExitEbxRestoreProjection.WindowBytes.SequenceEqual(
                new byte[] { 0x5B, 0x8B, 0xE5, 0x5D, 0xC3 }));

        NetworkIoErrorCallbackExitEbxRestorePlan complete =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x1000,
                freshSavedEBX: 0x12345678));
        check("A normal parent return uses a fresh post-call stack DWORD and stops before E4",
            complete.ParentExitCallReturnPlan.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.OpaqueCallReturnedNormally
            && complete.ParentExitCallReturnPlan.StopBeforeRawFileOffset == 0xBBC9E3
            && complete.ChildGateAdmitted
            && complete.Reads.Length == 1
            && complete.Reads[0].RawFileOffset == 0xBBC9E3
            && complete.Reads[0].Address == 0x1000
            && complete.Reads[0].Width == 4
            && complete.Reads[0].Attempted
            && complete.Reads[0].Captured
            && complete.Reads[0].Value == 0x12345678
            && !complete.Reads[0].NativeReadExecuted
            && complete.InstructionOrder.SequenceEqual(new[]
            {
                new NetworkIoErrorCallbackExitEbxRestoreInstruction(
                    0xBBC9E3, 0x1000, 0x1004, 0x12345678, Completed: true)
            })
            && complete.EBX == 0x12345678
            && complete.ESP == 0x1004
            && complete.PostCallEBP is null
            && complete.ParentExitCallReturnPlan.PreCallEBP == 0x1000
            && complete.FlagsAtStop is null
            && complete.PopEBXCompleted
            && complete.ChildBoundedCompletionBoundaryReached
            && complete.ExactChildCompletion
            && complete.BoundedCompletionBoundaryReached
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && complete.LastReachedRawFileOffset == 0xBBC9E3
            && complete.StopBeforeRawFileOffset == 0xBBC9E4
            && !complete.NativeInstructionsExecuted
            && !complete.NativeReadsExecuted
            && !complete.MemoryWriteExecuted
            && !complete.NativeCallExecuted
            && !complete.NextInstructionExecuted);

        NetworkIoErrorCallbackExitEbxRestorePlan zeroESP =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0,
                freshSavedEBX: 0));
        NetworkIoErrorCallbackExitEbxRestorePlan wrappedESP =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0xFFFFFFFC,
                freshSavedEBX: uint.MaxValue));
        check("Zero values are captured and the POP stack increment wraps",
            zeroESP.Reads[0].Address == 0
            && zeroESP.Reads[0].Value == 0
            && zeroESP.EBX == 0
            && zeroESP.ESP == 4
            && zeroESP.ExactCleanCompletion
            && wrappedESP.Reads[0].Address == 0xFFFFFFFC
            && wrappedESP.Reads[0].Value == uint.MaxValue
            && wrappedESP.EBX == uint.MaxValue
            && wrappedESP.ESP == 0
            && wrappedESP.InstructionOrder[0].ESPAfter == 0
            && wrappedESP.ExactCleanCompletion);

        NetworkIoErrorCallbackExitEbxRestorePlan changedStack =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0xA000,
                freshSavedEBX: 0xAABBCCDD,
                parentPreCallESP: 0x1000));
        check("Post-call ESP is independently captured rather than forwarded from pre-call ESP",
            changedStack.ParentExitCallReturnPlan.PreCallESP == 0x1008
            && changedStack.Capture.FreshPostCallESP == 0xA000
            && changedStack.Reads[0].Address == 0xA000
            && changedStack.ESP == 0xA004
            && changedStack.EBX == 0xAABBCCDD);

        NetworkIoErrorCallbackExitEbxRestorePlan aliasingStack =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x2000,
                freshSavedEBX: 0xDEADBEEF));
        check("The new saved-EBX DWORD is independent when its address aliases the parent POP read",
            aliasingStack.Reads[0].Address
                == aliasingStack.ParentExitCallReturnPlan.ParentExitPopPlan.Reads[0].Address
            && aliasingStack.Reads[0].Value == 0xDEADBEEF
            && aliasingStack.ParentExitCallReturnPlan.ParentExitPopPlan.Reads[0].Value
                == 0x3333
            && aliasingStack.Reads[0].Value
                != aliasingStack.ParentExitCallReturnPlan.ParentExitPopPlan.Reads[0].Value
            && aliasingStack.EBX == 0xDEADBEEF
            && aliasingStack.ExactCleanCompletion);

        NetworkIoErrorCallbackExitEbxRestorePlan missingESP =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: null,
                freshSavedEBX: 0x55));
        check("Missing post-call ESP at E3 performs no read and leaves the saved DWORD unused",
            missingESP.Outcome
                == NetworkIoErrorCallbackExitEbxRestoreOutcome.MissingFreshPostCallESP
            && missingESP.ChildGateAdmitted
            && missingESP.Reads.IsEmpty
            && missingESP.InstructionOrder.IsEmpty
            && missingESP.ESP is null
            && missingESP.EBX is null
            && missingESP.CaptureStates.FreshPostCallESP
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.Missing
            && missingESP.ParentExitCallReturnPlan.ExactCleanCompletion
            && missingESP.CaptureStates.FreshSavedEBXDWORD
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && !missingESP.ExactChildCompletion
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion
            && missingESP.LastReachedRawFileOffset == 0xBBC9E3
            && missingESP.StopBeforeRawFileOffset == 0xBBC9E3);

        NetworkIoErrorCallbackExitEbxRestorePlan missingSavedEBX =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x7000,
                freshSavedEBX: null));
        check("A missing saved DWORD at E3 is attempted without advancing ESP or defining EBX",
            missingSavedEBX.Outcome
                == NetworkIoErrorCallbackExitEbxRestoreOutcome.MissingFreshSavedEBXDWORD
            && missingSavedEBX.Reads.Length == 1
            && missingSavedEBX.Reads[0].Address == 0x7000
            && missingSavedEBX.Reads[0].Attempted
            && !missingSavedEBX.Reads[0].Captured
            && missingSavedEBX.Reads[0].Value is null
            && missingSavedEBX.ESP == 0x7000
            && missingSavedEBX.EBX is null
            && missingSavedEBX.InstructionOrder[0].ESPBefore == 0x7000
            && missingSavedEBX.InstructionOrder[0].ESPAfter == 0x7000
            && !missingSavedEBX.InstructionOrder[0].Completed
            && missingSavedEBX.FlagsAtStop is null
            && !missingSavedEBX.ExactChildCompletion
            && !missingSavedEBX.ExactBoundedCompletion
            && !missingSavedEBX.ExactCleanCompletion
            && missingSavedEBX.LastReachedRawFileOffset == 0xBBC9E3
            && missingSavedEBX.StopBeforeRawFileOffset == 0xBBC9E3);

        NetworkIoErrorCallbackExitEbxRestorePlan pendingParent =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x8000,
                freshSavedEBX: 0x66,
                returnedNormally: null));
        NetworkIoErrorCallbackExitEbxRestorePlan nonnormalParent =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x8000,
                freshSavedEBX: 0x66,
                returnedNormally: false));
        NetworkIoErrorCallbackExitEbxRestorePlan rejectedParent =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x8000,
                freshSavedEBX: 0x66,
                comparedByte: 0));
        NetworkIoErrorCallbackExitEbxRestorePlan dirtyParent =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(Inputs(
                freshESP: 0x8000,
                freshSavedEBX: 0x66,
                callbackReturnedNormally: true));
        check("Pending, nonnormal, rejected, and dirty parents preserve their bounds and ignore child captures",
            !pendingParent.ChildGateAdmitted
            && !nonnormalParent.ChildGateAdmitted
            && !rejectedParent.ChildGateAdmitted
            && !dirtyParent.ChildGateAdmitted
            && pendingParent.ParentExitCallReturnPlan.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome
                    .MissingOpaqueCallReturnObservation
            && nonnormalParent.ParentExitCallReturnPlan.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome
                    .OpaqueCallDidNotReturnNormally
            && rejectedParent.ParentExitCallReturnPlan.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.ParentNotAtOpaqueCallBoundary
            && pendingParent.Reads.IsEmpty
            && nonnormalParent.Reads.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && dirtyParent.Reads.IsEmpty
            && pendingParent.CaptureStates.FreshPostCallESP
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && pendingParent.CaptureStates.FreshSavedEBXDWORD
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && nonnormalParent.LastReachedRawFileOffset
                == nonnormalParent.ParentExitCallReturnPlan.LastReachedRawFileOffset
            && nonnormalParent.StopBeforeRawFileOffset
                == nonnormalParent.ParentExitCallReturnPlan.StopBeforeRawFileOffset
            && pendingParent.StopBeforeRawFileOffset
                == pendingParent.ParentExitCallReturnPlan.StopBeforeRawFileOffset
            && rejectedParent.LastReachedRawFileOffset
                == rejectedParent.ParentExitCallReturnPlan.LastReachedRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentExitCallReturnPlan.StopBeforeRawFileOffset
            && dirtyParent.LastReachedRawFileOffset
                == dirtyParent.ParentExitCallReturnPlan.LastReachedRawFileOffset
            && dirtyParent.StopBeforeRawFileOffset
                == dirtyParent.ParentExitCallReturnPlan.StopBeforeRawFileOffset
            && pendingParent.ExactBoundedCompletion
                == pendingParent.ParentExitCallReturnPlan.ExactBoundedCompletion
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentExitCallReturnPlan.ExactBoundedCompletion
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentExitCallReturnPlan.ExactBoundedCompletion
            && rejectedParent.CaptureStates.FreshPostCallESP
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && rejectedParent.CaptureStates.FreshSavedEBXDWORD
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshPostCallESP
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshSavedEBXDWORD
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused);

        NetworkIoErrorCallbackExitEbxRestorePlan reprojection =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(
                complete.Capture with { });
        check("The immutable parent trace remains a semantic ordered prefix on reprojection",
            reprojection.Outcome == complete.Outcome
            && reprojection.Capture.FreshPostCallESP == complete.Capture.FreshPostCallESP
            && reprojection.Capture.FreshSavedEBXDWORD
                == complete.Capture.FreshSavedEBXDWORD
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.Reads.SequenceEqual(complete.Reads)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.ParentExitCallReturnPlan.Outcome
                == complete.ParentExitCallReturnPlan.Outcome
            && reprojection.ParentExitCallReturnPlan.TargetReads
                .Select(read => (read.RawFileOffset, read.Address, read.Width,
                    read.Attempted, read.Captured, read.Value))
                .SequenceEqual(complete.ParentExitCallReturnPlan.TargetReads
                    .Select(read => (read.RawFileOffset, read.Address, read.Width,
                        read.Attempted, read.Captured, read.Value)))
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitCallReturnPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitCallReturnEffect?.Sequence)
                .SequenceEqual(complete.ParentExitCallReturnPlan.OrderedEffects
                    .Select(effect => (int?)effect.Sequence))
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitCallReturnPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitCallReturnEffect?.TargetRead)
                .SequenceEqual(complete.ParentExitCallReturnPlan.OrderedEffects
                    .Select(effect => effect.TargetRead))
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitCallReturnPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitCallReturnEffect?.CallBoundary)
                .SequenceEqual(complete.ParentExitCallReturnPlan.OrderedEffects
                    .Select(effect => effect.CallBoundary))
            && reprojection.OrderedEffects.Length == complete.OrderedEffects.Length);
    }

    private static NetworkIoErrorCallbackExitEbxRestoreInputs Inputs(
        uint? freshESP,
        uint? freshSavedEBX,
        uint? parentPreCallESP = 0x2000,
        byte? comparedByte = 1,
        bool? returnedNormally = true,
        bool? callbackReturnedNormally = null) =>
        new(
            new(
                new(
                    new(
                        new(
                            new(
                                new(
                                ParentPassCapture: CleanMissingKeyParent(
                                    callbackReturnedNormally),
                                FreshEBP: 0x1000,
                                FreshDispatchKeyDWORD: 0,
                                FreshESP: 0x2000,
                                FreshVptrDWORD: 0),
                            FreshSlot20TargetDWORD: 0,
                            OpaqueCallReturnedNormally: true),
                        FreshPostCallEBP: 0x1000,
                        FreshLocalMinus8DWORD: 0,
                        FreshLocalMinus10DWORD: 0,
                        FreshPostCallEDI: 99),
                    FreshLocalMinus24DWORD: 0x2000,
                    FreshComparedBYTE: comparedByte,
                    FreshLocalMinus28DWORD: 0x2222),
                parentPreCallESP,
                FreshPopEDIDWORD: 0x3333,
                FreshPopESIDWORD: 0x4444),
                FreshOpaqueCallTargetDWORD: 0x1234,
                OpaqueCallReturnedNormally: returnedNormally),
            freshESP,
            freshSavedEBX);

    private static NetworkIoCompletionPassCapture CleanMissingKeyParent(
        bool? callbackReturnedNormally = null) =>
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
                CallbackReturnedNormally: callbackReturnedNormally)),
            StopFlagAfterPass: null);
}
