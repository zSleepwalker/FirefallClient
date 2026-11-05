using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackExitFrameResetChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The frame-reset window models MOV ESP, EBP and POP EBP, then stops before RET",
            !NetworkIoErrorCallbackExitFrameResetProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackExitFrameResetProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackExitFrameResetProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackExitFrameResetProjection.ExecutesNextInstruction
            && !NetworkIoErrorCallbackExitFrameResetProjection.ExecutesCalls
            && NetworkIoErrorCallbackExitFrameResetProjection.MoveESPToEBPRawFileOffset
                == 0xBBC9E4
            && NetworkIoErrorCallbackExitFrameResetProjection.PopEBPRawFileOffset
                == 0xBBC9E6
            && NetworkIoErrorCallbackExitFrameResetProjection.RetRawFileOffset
                == 0xBBC9E7
            && NetworkIoErrorCallbackExitFrameResetProjection.MoveESPToEBPOpcode
                == 0x8B
            && NetworkIoErrorCallbackExitFrameResetProjection.PopEBPOpcode == 0x5D
            && NetworkIoErrorCallbackExitFrameResetProjection.WindowBytes.SequenceEqual(
                new byte[] { 0x8B, 0xE5, 0x5D, 0xC3 }));

        NetworkIoErrorCallbackExitFrameResetPlan complete =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0xA000,
                freshSavedEBP: 0xAABBCCDD));
        check("Fresh post-call EBP resets ESP and POP EBP restores its independent DWORD",
            complete.ParentExitEbxRestorePlan.Outcome
                == NetworkIoErrorCallbackExitEbxRestoreOutcome.PopEBXCompleted
            && complete.ParentExitEbxRestorePlan.StopBeforeRawFileOffset == 0xBBC9E4
            && complete.ChildGateAdmitted
            && complete.Reads.Length == 1
            && complete.Reads[0].RawFileOffset == 0xBBC9E6
            && complete.Reads[0].Address == 0xA000
            && complete.Reads[0].Width == 4
            && complete.Reads[0].Attempted
            && complete.Reads[0].Captured
            && complete.Reads[0].Value == 0xAABBCCDD
            && !complete.Reads[0].NativeReadExecuted
            && complete.InstructionOrder.SequenceEqual(new[]
            {
                new NetworkIoErrorCallbackExitFrameResetInstruction(
                    NetworkIoErrorCallbackExitFrameResetInstructionKind.MoveESPFromEBP,
                    0xBBC9E4, 0x2004, 0xA000, 0xA000, 0xA000, 0xA000, true),
                new NetworkIoErrorCallbackExitFrameResetInstruction(
                    NetworkIoErrorCallbackExitFrameResetInstructionKind.PopEBP,
                    0xBBC9E6, 0xA000, 0xA004, 0xA000, 0xAABBCCDD,
                    0xAABBCCDD, true)
            })
            && complete.EBP == 0xAABBCCDD
            && complete.RestoredEBP == 0xAABBCCDD
            && complete.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.FreshSavedEBPDWORD
            && complete.EBX == complete.ParentExitEbxRestorePlan.EBX
            && complete.ESP == 0xA004
            && complete.FlagsAtStop is null
            && complete.MoveESPToEBPCompleted
            && complete.PopEBPCompleted
            && complete.ChildBoundedCompletionBoundaryReached
            && complete.ExactChildCompletion
            && complete.BoundedCompletionBoundaryReached
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && complete.LastReachedRawFileOffset == 0xBBC9E6
            && complete.StopBeforeRawFileOffset == 0xBBC9E7
            && !complete.NativeInstructionsExecuted
            && !complete.NativeReadsExecuted
            && !complete.MemoryWriteExecuted
            && !complete.NativeCallExecuted
            && !complete.NextInstructionExecuted);

        NetworkIoErrorCallbackExitFrameResetPlan zeroFrame =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0,
                freshSavedEBP: 0));
        NetworkIoErrorCallbackExitFrameResetPlan maximumFrame =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: uint.MaxValue,
                freshSavedEBP: uint.MaxValue));
        NetworkIoErrorCallbackExitFrameResetPlan wrappedFrame =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0xFFFFFFFC,
                freshSavedEBP: 0x12345678));
        check("Zero, maximum, and wrapping frame values are captured as DWORDs",
            zeroFrame.Reads[0].Address == 0
            && zeroFrame.Reads[0].Value == 0
            && zeroFrame.EBP == 0
            && zeroFrame.RestoredEBP == 0
            && zeroFrame.ESP == 4
            && zeroFrame.ExactCleanCompletion
            && maximumFrame.Reads[0].Address == uint.MaxValue
            && maximumFrame.Reads[0].Value == uint.MaxValue
            && maximumFrame.EBP == uint.MaxValue
            && maximumFrame.RestoredEBP == uint.MaxValue
            && maximumFrame.ESP == 3
            && maximumFrame.InstructionOrder[1].ESPAfter == 3
            && maximumFrame.ExactCleanCompletion
            && wrappedFrame.Reads[0].Address == 0xFFFFFFFC
            && wrappedFrame.Reads[0].Value == 0x12345678
            && wrappedFrame.EBP == 0x12345678
            && wrappedFrame.RestoredEBP == 0x12345678
            && wrappedFrame.EBP != wrappedFrame.Capture.FreshPostCallEBP
            && wrappedFrame.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.FreshSavedEBPDWORD
            && wrappedFrame.ESP == 0
            && wrappedFrame.ExactCleanCompletion);

        NetworkIoErrorCallbackExitFrameResetPlan aliasingFrame =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x2000,
                freshSavedEBP: 0xDEADBEEF));
        check("The fresh saved-EBP DWORD is independent when its address aliases the prior POP read",
            aliasingFrame.Reads[0].Address
                == aliasingFrame.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                    .ParentExitPopPlan.Reads[0].Address
            && aliasingFrame.Reads[0].Value == 0xDEADBEEF
            && aliasingFrame.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                .ParentExitPopPlan.Reads[0].Value == 0x3333
            && aliasingFrame.Reads[0].Value
                != aliasingFrame.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                    .ParentExitPopPlan.Reads[0].Value
            && aliasingFrame.EBP == 0xDEADBEEF
            && aliasingFrame.ExactCleanCompletion);

        NetworkIoErrorCallbackExitFrameResetPlan missingEBP =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: null,
                freshSavedEBP: 0x55));
        check("Missing fresh post-call EBP stops at E4 without resetting ESP or using the saved DWORD",
            missingEBP.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome.MissingFreshPostCallEBP
            && missingEBP.ChildGateAdmitted
            && missingEBP.Reads.IsEmpty
            && missingEBP.InstructionOrder.IsEmpty
            && missingEBP.EBP is null
            && missingEBP.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.Unknown
            && missingEBP.ESP == missingEBP.ParentExitEbxRestorePlan.ESP
            && missingEBP.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.Missing
            && missingEBP.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && !missingEBP.MoveESPToEBPCompleted
            && !missingEBP.PopEBPCompleted
            && !missingEBP.ExactChildCompletion
            && !missingEBP.ExactBoundedCompletion
            && !missingEBP.ExactCleanCompletion
            && missingEBP.LastReachedRawFileOffset == 0xBBC9E4
            && missingEBP.StopBeforeRawFileOffset == 0xBBC9E4);

        NetworkIoErrorCallbackExitFrameResetPlan missingSavedEBP =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x7000,
                freshSavedEBP: null));
        check("A missing saved DWORD after MOV ESP, EBP leaves ESP reset and EBP source explicit",
            missingSavedEBP.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome.MissingFreshSavedEBPDWORD
            && missingSavedEBP.Reads.Length == 1
            && missingSavedEBP.Reads[0].Address == 0x7000
            && missingSavedEBP.Reads[0].Attempted
            && !missingSavedEBP.Reads[0].Captured
            && missingSavedEBP.Reads[0].Value is null
            && missingSavedEBP.ESP == 0x7000
            && missingSavedEBP.EBP == 0x7000
            && missingSavedEBP.RestoredEBP is null
            && missingSavedEBP.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.FreshPostCallEBP
            && missingSavedEBP.InstructionOrder[0].ESPAfter == 0x7000
            && missingSavedEBP.InstructionOrder[1].ESPAfter == 0x7000
            && missingSavedEBP.InstructionOrder[1].EBPAfter == 0x7000
            && !missingSavedEBP.InstructionOrder[1].Completed
            && missingSavedEBP.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.Missing
            && !missingSavedEBP.PopEBPCompleted
            && !missingSavedEBP.ExactChildCompletion
            && !missingSavedEBP.ExactBoundedCompletion
            && !missingSavedEBP.ExactCleanCompletion
            && missingSavedEBP.LastReachedRawFileOffset == 0xBBC9E6
            && missingSavedEBP.StopBeforeRawFileOffset == 0xBBC9E6);

        NetworkIoErrorCallbackExitFrameResetPlan pendingParent =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x8000,
                freshSavedEBP: 0x66,
                returnedNormally: null));
        NetworkIoErrorCallbackExitFrameResetPlan falseParent =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x8000,
                freshSavedEBP: 0x66,
                returnedNormally: false));
        NetworkIoErrorCallbackExitFrameResetPlan rejectedParent =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x8000,
                freshSavedEBP: 0x66,
                comparedByte: 0));
        NetworkIoErrorCallbackExitFrameResetPlan dirtyParent =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0x8000,
                freshSavedEBP: 0x66,
                callbackReturnedNormally: true));
        check("Pending, false, rejected, and dirty parents retain their bounds and ignore captures",
            !pendingParent.ChildGateAdmitted
            && !falseParent.ChildGateAdmitted
            && !rejectedParent.ChildGateAdmitted
            && !dirtyParent.ChildGateAdmitted
            && pendingParent.ParentExitEbxRestorePlan.Outcome
                == NetworkIoErrorCallbackExitEbxRestoreOutcome
                    .ParentNotAtPopEBXBoundary
            && falseParent.ParentExitEbxRestorePlan.Outcome
                == NetworkIoErrorCallbackExitEbxRestoreOutcome
                    .ParentNotAtPopEBXBoundary
            && pendingParent.Reads.IsEmpty
            && falseParent.Reads.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && dirtyParent.Reads.IsEmpty
            && pendingParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && pendingParent.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && falseParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && falseParent.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && rejectedParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && rejectedParent.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            && falseParent.BoundedCompletionBoundaryReached
                == falseParent.ParentExitEbxRestorePlan.BoundedCompletionBoundaryReached
            && falseParent.StopBeforeRawFileOffset
                == falseParent.ParentExitEbxRestorePlan.StopBeforeRawFileOffset
            && pendingParent.LastReachedRawFileOffset
                == pendingParent.ParentExitEbxRestorePlan.LastReachedRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentExitEbxRestorePlan.StopBeforeRawFileOffset
            && dirtyParent.LastReachedRawFileOffset
                == dirtyParent.ParentExitEbxRestorePlan.LastReachedRawFileOffset
            && pendingParent.BoundedCompletionBoundaryReached
                == pendingParent.ParentExitEbxRestorePlan.BoundedCompletionBoundaryReached
            && pendingParent.ExactBoundedCompletion
                == pendingParent.ParentExitEbxRestorePlan.ExactBoundedCompletion
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentExitEbxRestorePlan.ExactBoundedCompletion
            && falseParent.ExactBoundedCompletion
                == falseParent.ParentExitEbxRestorePlan.ExactBoundedCompletion
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentExitEbxRestorePlan.ExactBoundedCompletion);

        NetworkIoErrorCallbackExitFrameResetPlan distinctFreshEBP =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(Inputs(
                freshEBP: 0xA000,
                freshSavedEBP: 0xB000));
        check("The fresh post-call EBP is not inferred from the older pre-call EBP",
            distinctFreshEBP.ParentExitEbxRestorePlan.ParentExitCallReturnPlan.PreCallEBP
                == 0x1000
            && distinctFreshEBP.Capture.FreshPostCallEBP == 0xA000
            && distinctFreshEBP.InstructionOrder[0].EBPBefore == 0xA000
            && distinctFreshEBP.EBP == 0xB000);

        NetworkIoErrorCallbackExitFrameResetPlan reprojection =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(
                complete.Capture with { });
        check("Reprojection preserves the immutable capture semantics and ordered parent prefix",
            reprojection.Outcome == complete.Outcome
            && reprojection.Capture.FreshPostCallEBP == complete.Capture.FreshPostCallEBP
            && reprojection.Capture.FreshSavedEBPDWORD
                == complete.Capture.FreshSavedEBPDWORD
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.Reads.SequenceEqual(complete.Reads)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.ParentExitEbxRestorePlan.Outcome
                == complete.ParentExitEbxRestorePlan.Outcome
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitEbxRestorePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitEbxRestoreEffect?.Sequence)
                .SequenceEqual(complete.ParentExitEbxRestorePlan.OrderedEffects
                    .Select(effect => (int?)effect.Sequence))
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitEbxRestorePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitEbxRestoreEffect?.Read)
                .SequenceEqual(complete.ParentExitEbxRestorePlan.OrderedEffects
                    .Select(effect => effect.Read))
            && reprojection.OrderedEffects.Take(
                    complete.ParentExitEbxRestorePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitEbxRestoreEffect?.Instruction)
                .SequenceEqual(complete.ParentExitEbxRestorePlan.OrderedEffects
                    .Select(effect => effect.Instruction))
            && reprojection.OrderedEffects.Length == complete.OrderedEffects.Length);
    }

    private static NetworkIoErrorCallbackExitFrameResetInputs Inputs(
        uint? freshEBP,
        uint? freshSavedEBP,
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
            FreshPostCallESP: 0x2000,
            FreshSavedEBXDWORD: 0x5678),
            freshEBP,
            freshSavedEBP);

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
