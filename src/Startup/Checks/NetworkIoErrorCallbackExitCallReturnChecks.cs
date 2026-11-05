using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackExitCallReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The call-return window reads the fixed call cell and stops before POP EBX",
            !NetworkIoErrorCallbackExitCallReturnProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackExitCallReturnProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackExitCallReturnProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackExitCallReturnProjection.ExecutesCalls
            && !NetworkIoErrorCallbackExitCallReturnProjection.EntersOpaqueCallTarget
            && !NetworkIoErrorCallbackExitCallReturnProjection.ExecutesContinuationInstruction
            && NetworkIoErrorCallbackExitCallReturnProjection.OpaqueCallRawFileOffset
                == 0xBBC9DD
            && NetworkIoErrorCallbackExitCallReturnProjection.ContinuationRawFileOffset
                == 0xBBC9E3
            && NetworkIoErrorCallbackExitCallReturnProjection.OpaqueCallCellVirtualAddress
                == 0x01DF9760
            && NetworkIoErrorCallbackExitCallReturnProjection.PopEBXOpcode == 0x5B
            && NetworkIoErrorCallbackExitCallReturnProjection.WindowBytes.SequenceEqual(
                new byte[] { 0xFF, 0x15, 0x60, 0x97, 0xDF, 0x01, 0x5B }));

        NetworkIoErrorCallbackExitCallReturnPlan zeroTarget =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0,
                returnedNormally: true));
        check("A zero target DWORD is an observed scalar, not a target identity claim",
            zeroTarget.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.OpaqueCallReturnedNormally
            && zeroTarget.TargetReadAttempted
            && zeroTarget.TargetReadCaptured
            && zeroTarget.TargetReads.Length == 1
            && zeroTarget.TargetReads[0].Address == 0x01DF9760
            && zeroTarget.TargetReads[0].Width == 4
            && zeroTarget.TargetReads[0].Value == 0
            && zeroTarget.CallBoundaries.Length == 1
            && zeroTarget.CallBoundaries[0].TargetDWORD == 0
            && zeroTarget.LogicalCallAttempted
            && !zeroTarget.OpaqueCallTargetEntered
            && !zeroTarget.NativeCallExecuted
            && zeroTarget.ExactCleanCompletion);

        NetworkIoErrorCallbackExitCallReturnPlan maximumTarget =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: uint.MaxValue,
                returnedNormally: true));
        check("A maximum target DWORD remains a scalar with no native execution",
            maximumTarget.TargetReads[0].Value == uint.MaxValue
            && maximumTarget.CallBoundaries[0].TargetDWORD == uint.MaxValue
            && maximumTarget.LogicalCallAttempted
            && !maximumTarget.NativeTargetReadExecuted
            && !maximumTarget.NativeCallExecuted
            && !maximumTarget.OpaqueCallTargetEntered
            && maximumTarget.ExactCleanCompletion);

        NetworkIoErrorCallbackExitCallReturnPlan missingTarget =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: null,
                returnedNormally: true));
        check("A missing target read stops at DD and leaves the supplied return observation unused",
            missingTarget.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome
                    .MissingFreshOpaqueCallTargetDWORD
            && missingTarget.ParentExitPopPlan.ExactCleanCompletion
            && missingTarget.TargetReadAttempted
            && !missingTarget.TargetReadCaptured
            && missingTarget.TargetReads.Length == 1
            && missingTarget.TargetReads[0].Attempted
            && !missingTarget.TargetReads[0].Captured
            && missingTarget.TargetReads[0].Value is null
            && missingTarget.TargetReads[0].Address == 0x01DF9760
            && !missingTarget.LogicalCallAttempted
            && missingTarget.CallBoundaries.IsEmpty
            && missingTarget.OpaqueCallReturnedNormally is null
            && missingTarget.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            && missingTarget.FlagsAtStop == missingTarget.ParentExitPopPlan.FlagsAtStop
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && !missingTarget.ExactChildCompletion
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion
            && missingTarget.LastReachedRawFileOffset
                == missingTarget.ParentExitPopPlan.LastReachedRawFileOffset
            && missingTarget.StopBeforeRawFileOffset == 0xBBC9DD);

        NetworkIoErrorCallbackExitCallReturnPlan pendingReturn =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0x12345678,
                returnedNormally: null));
        check("A captured target with pending return metadata stops before the call completes",
            pendingReturn.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome
                    .MissingOpaqueCallReturnObservation
            && pendingReturn.TargetReadCaptured
            && pendingReturn.LogicalCallAttempted
            && pendingReturn.CallBoundaries[0].LogicalCallAttempted
            && !pendingReturn.CallBoundaries[0].NativeCallExecuted
            && pendingReturn.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.Missing
            && pendingReturn.OpaqueCallReturnedNormally is null
            && pendingReturn.FlagsAtStop == pendingReturn.ParentExitPopPlan.FlagsAtStop
            && !pendingReturn.ChildBoundedCompletionBoundaryReached
            && !pendingReturn.ExactChildCompletion
            && !pendingReturn.ExactBoundedCompletion
            && !pendingReturn.ExactCleanCompletion
            && pendingReturn.LastReachedRawFileOffset
                == pendingReturn.ParentExitPopPlan.LastReachedRawFileOffset
            && pendingReturn.StopBeforeRawFileOffset == 0xBBC9DD);

        NetworkIoErrorCallbackExitCallReturnPlan didNotReturn =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0x12345678,
                returnedNormally: false));
        check("An explicit nonnormal observation retains parent flags and stops at DD",
            didNotReturn.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.OpaqueCallDidNotReturnNormally
            && didNotReturn.CallReturnObservationCaptured
            && didNotReturn.OpaqueCallReturnedNormally == false
            && didNotReturn.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.Captured
            && didNotReturn.FlagsAtStop == didNotReturn.ParentExitPopPlan.FlagsAtStop
            && !didNotReturn.ChildBoundedCompletionBoundaryReached
            && !didNotReturn.ExactChildCompletion
            && !didNotReturn.ExactBoundedCompletion
            && !didNotReturn.ExactCleanCompletion
            && didNotReturn.LastReachedRawFileOffset
                == didNotReturn.ParentExitPopPlan.LastReachedRawFileOffset
            && didNotReturn.StopBeforeRawFileOffset == 0xBBC9DD);

        NetworkIoErrorCallbackExitCallReturnPlan returnedNormally =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0x12345678,
                returnedNormally: true));
        NetworkIoErrorCallbackLoopGateFlags knownParentFlags =
            new(false, false, false, false, false, false);
        check("A normal return reaches E3 before POP EBX and makes all parent flags unknown",
            returnedNormally.ParentExitPopPlan.FlagsAtStop == knownParentFlags
            && returnedNormally.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.OpaqueCallReturnedNormally
            && returnedNormally.OpaqueCallReturnedNormally == true
            && returnedNormally.CallReturnObservationCaptured
            && returnedNormally.ContinuationBoundary
                == new NetworkIoErrorCallbackExitCallReturnContinuationBoundary(
                    0xBBC9E3,
                    0x5B,
                    Reached: true,
                    NextInstructionExecuted: false)
            && returnedNormally.LastReachedRawFileOffset == 0xBBC9DD
            && returnedNormally.StopBeforeRawFileOffset == 0xBBC9E3
            && returnedNormally.FlagsAtStop is null
            && returnedNormally.ChildBoundedCompletionBoundaryReached
            && returnedNormally.ExactChildCompletion
            && returnedNormally.BoundedCompletionBoundaryReached
            && returnedNormally.ExactBoundedCompletion
            && returnedNormally.ExactCleanCompletion
            && !returnedNormally.ContinuationInstructionExecuted
            && !returnedNormally.NativeInstructionsExecuted
            && !returnedNormally.NativeCallExecuted);

        NetworkIoErrorCallbackExitCallReturnPlan aliasingStack =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0xAABBCCDD,
                returnedNormally: true,
                freshESP: 0x01DF9758));
        check("The fixed call-cell read is independent when its address aliases post-pop ESP",
            aliasingStack.PreCallESP == 0x01DF9760
            && aliasingStack.TargetReads[0].Address == 0x01DF9760
            && aliasingStack.ParentExitPopPlan.Reads[0].Address == 0x01DF9758
            && aliasingStack.ParentExitPopPlan.Reads[1].Address == 0x01DF975C
            && aliasingStack.TargetReads[0].Address
                == aliasingStack.CallBoundaries[0].CallCellVirtualAddress
            && aliasingStack.TargetReads[0].Value == 0xAABBCCDD
            && aliasingStack.ExactCleanCompletion);

        NetworkIoErrorCallbackExitCallReturnPlan rejectedParent =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0x55,
                returnedNormally: true,
                comparedByte: 0));
        NetworkIoErrorCallbackExitCallReturnPlan dirtyParent =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(Inputs(
                freshTarget: 0x55,
                returnedNormally: true,
                callbackReturnedNormally: true));
        check("Rejected and dirty parents preserve their exact bound and leave child captures unused",
            rejectedParent.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.ParentNotAtOpaqueCallBoundary
            && rejectedParent.ParentExitPopPlan.ParentPostLoopByteGatePlan.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
            && !rejectedParent.ChildGateAdmitted
            && rejectedParent.TargetReads.IsEmpty
            && rejectedParent.OpaqueCallReturnedNormally is null
            && rejectedParent.CaptureStates.FreshOpaqueCallTargetDWORD
                == NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            && rejectedParent.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            && rejectedParent.ParentExactBoundedCompletion
                == rejectedParent.ParentExitPopPlan.ExactBoundedCompletion
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentExitPopPlan.ExactBoundedCompletion
            && rejectedParent.LastReachedRawFileOffset
                == rejectedParent.ParentExitPopPlan.LastReachedRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentExitPopPlan.StopBeforeRawFileOffset
            && dirtyParent.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.ParentNotAtOpaqueCallBoundary
            && dirtyParent.ParentExitPopPlan.ParentPostLoopByteGatePlan.ParentLoopGatePlan
                .ParentReturnPlan.ParentPreparationPlan.ParentPassPlan.Attempts[0]
                .Capture.CallbackReturnedNormally
                == true
            && !dirtyParent.ChildGateAdmitted
            && dirtyParent.TargetReads.IsEmpty
            && dirtyParent.OpaqueCallReturnedNormally is null
            && dirtyParent.CaptureStates.FreshOpaqueCallTargetDWORD
                == NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            && dirtyParent.ParentExactBoundedCompletion
                == dirtyParent.ParentExitPopPlan.ExactBoundedCompletion
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentExitPopPlan.ExactBoundedCompletion
            && dirtyParent.LastReachedRawFileOffset
                == dirtyParent.ParentExitPopPlan.LastReachedRawFileOffset
            && dirtyParent.StopBeforeRawFileOffset
                == dirtyParent.ParentExitPopPlan.StopBeforeRawFileOffset);

        NetworkIoErrorCallbackExitCallReturnPlan reprojection =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(
                returnedNormally.Capture with { });
        check("The immutable parent trace remains a semantic ordered prefix on reprojection",
            reprojection.Outcome == returnedNormally.Outcome
            && reprojection.Capture.FreshOpaqueCallTargetDWORD
                == returnedNormally.Capture.FreshOpaqueCallTargetDWORD
            && reprojection.Capture.OpaqueCallReturnedNormally
                == returnedNormally.Capture.OpaqueCallReturnedNormally
            && reprojection.CaptureStates == returnedNormally.CaptureStates
            && reprojection.TargetReads.SequenceEqual(returnedNormally.TargetReads)
            && reprojection.CallBoundaries.SequenceEqual(returnedNormally.CallBoundaries)
            && reprojection.InstructionOrder.SequenceEqual(returnedNormally.InstructionOrder)
            && reprojection.ParentExitPopPlan.Outcome
                == returnedNormally.ParentExitPopPlan.Outcome
            && reprojection.ParentExitPopPlan.Reads
                .Select(read => (read.RawFileOffset, read.Kind, read.Address,
                    read.Width, read.Attempted, read.Captured, read.Value))
                .SequenceEqual(returnedNormally.ParentExitPopPlan.Reads
                    .Select(read => (read.RawFileOffset, read.Kind, read.Address,
                        read.Width, read.Attempted, read.Captured, read.Value)))
            && reprojection.ParentExitPopPlan.InstructionOrder
                .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                    instruction.ESPBefore, instruction.ESPAfter,
                    instruction.Value, instruction.Completed))
                .SequenceEqual(returnedNormally.ParentExitPopPlan.InstructionOrder
                    .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                        instruction.ESPBefore, instruction.ESPAfter,
                        instruction.Value, instruction.Completed)))
            && reprojection.OrderedEffects.Take(
                    returnedNormally.ParentExitPopPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitPopEffect?.Sequence)
                .SequenceEqual(returnedNormally.ParentExitPopPlan.OrderedEffects
                    .Select(effect => (int?)effect.Sequence))
            && reprojection.OrderedEffects.Take(
                    returnedNormally.ParentExitPopPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitPopEffect?.Read)
                .SequenceEqual(returnedNormally.ParentExitPopPlan.OrderedEffects
                    .Select(effect => effect.Read))
            && reprojection.OrderedEffects.Take(
                    returnedNormally.ParentExitPopPlan.OrderedEffects.Length)
                .Select(effect => effect.ParentExitPopEffect?.Instruction)
                .SequenceEqual(returnedNormally.ParentExitPopPlan.OrderedEffects
                    .Select(effect => effect.Instruction))
            && reprojection.OrderedEffects.Length
                == returnedNormally.OrderedEffects.Length);
    }

    private static NetworkIoErrorCallbackExitCallReturnInputs Inputs(
        uint? freshTarget,
        bool? returnedNormally,
        byte? comparedByte = 1,
        uint? freshESP = 0x2000,
        bool? callbackReturnedNormally = null) =>
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
                freshESP,
                FreshPopEDIDWORD: 0x3333,
                FreshPopESIDWORD: 0x4444),
            freshTarget,
            returnedNormally);

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
