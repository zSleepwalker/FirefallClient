using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackExitPopChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exit-pop window stops before the absolute call cell",
            !NetworkIoErrorCallbackExitPopProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackExitPopProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackExitPopProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackExitPopProjection.ExecutesCalls
            && !NetworkIoErrorCallbackExitPopProjection.EntersOpaqueCallCell
            && NetworkIoErrorCallbackExitPopProjection.EntryRawFileOffset == 0xBBC9DB
            && NetworkIoErrorCallbackExitPopProjection.PopEDIRawFileOffset == 0xBBC9DB
            && NetworkIoErrorCallbackExitPopProjection.PopESIRawFileOffset == 0xBBC9DC
            && NetworkIoErrorCallbackExitPopProjection.OpaqueCallCellRawFileOffset == 0xBBC9DD
            && NetworkIoErrorCallbackExitPopProjection.OpaqueCallCellVirtualAddress
                == 0x01DF9760
            && NetworkIoErrorCallbackExitPopProjection.WindowBytes.SequenceEqual(
                new byte[] { 0x5F, 0x5E, 0xFF, 0x15, 0x60, 0x97, 0xDF, 0x01 }));

        NetworkIoErrorCallbackExitPopPlan complete =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x1111,
                freshComparedByte: 1,
                freshLocalMinus28: 0x2222,
                freshESP: 0x1000,
                freshPopEDI: 0x3333,
                freshPopESI: 0x4444));
        check("A clean 9DB fallthrough captures independent stack DWORDs and stops before the call",
            complete.ChildGateAdmitted
            && complete.ParentPostLoopByteGatePlan.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.FallThroughTo9DB
            && complete.ParentPostLoopByteGatePlan.EAX == 0x2222
            && complete.EAX == 0x2222
            && complete.EBP == complete.ParentPostLoopByteGatePlan.EBP
            && complete.EDI == 0x3333
            && complete.ESI == 0x4444
            && complete.FlagsAtStop
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false)
            && complete.FlagsAtStop
                == complete.ParentPostLoopByteGatePlan.FlagsAtStop
            && complete.CaptureStates
                == new NetworkIoErrorCallbackExitPopCaptureStates(
                    NetworkIoErrorCallbackExitPopCaptureState.Captured,
                    NetworkIoErrorCallbackExitPopCaptureState.Captured,
                    NetworkIoErrorCallbackExitPopCaptureState.Captured)
            && complete.Reads.Length == 2
            && complete.Reads[0].RawFileOffset == 0xBBC9DB
            && complete.Reads[0].Address == 0x1000
            && complete.Reads[0].Width == 4
            && complete.Reads[0].Value == 0x3333
            && complete.Reads[0].Captured
            && !complete.Reads[0].NativeReadExecuted
            && complete.Reads[1].RawFileOffset == 0xBBC9DC
            && complete.Reads[1].Address == 0x1004
            && complete.Reads[1].Width == 4
            && complete.Reads[1].Value == 0x4444
            && complete.Reads[1].Captured
            && !complete.Reads[1].NativeReadExecuted
            && complete.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[] { 0xBBC9DB, 0xBBC9DC })
            && complete.InstructionOrder[0].Kind
                == NetworkIoErrorCallbackExitPopInstructionKind.PopEDI
            && complete.InstructionOrder[0].ESPBefore == 0x1000
            && complete.InstructionOrder[0].ESPAfter == 0x1004
            && complete.InstructionOrder[0].Value == 0x3333
            && complete.InstructionOrder[0].Completed
            && complete.InstructionOrder[1].Kind
                == NetworkIoErrorCallbackExitPopInstructionKind.PopESI
            && complete.InstructionOrder[1].ESPBefore == 0x1004
            && complete.InstructionOrder[1].ESPAfter == 0x1008
            && complete.InstructionOrder[1].Value == 0x4444
            && complete.InstructionOrder[1].Completed
            && complete.ESP == 0x1008
            && complete.PopEDICompleted
            && complete.PopESICompleted
            && complete.CallCellBoundary
                == new NetworkIoErrorCallbackExitPopCallCellBoundary(
                    0xBBC9DD,
                    0x01DF9760,
                    BoundaryReached: true,
                    TargetReadAttempted: false,
                    NativeCallExecuted: false)
            && complete.LastReachedRawFileOffset == 0xBBC9DC
            && complete.StopBeforeRawFileOffset == 0xBBC9DD
            && complete.ChildBoundedCompletionBoundaryReached
            && complete.ExactChildCompletion
            && complete.BoundedCompletionBoundaryReached
            && complete.ExactBoundedCompletion
            && complete.ExactCleanCompletion
            && !complete.NativeInstructionsExecuted
            && !complete.NativeReadsExecuted
            && !complete.MemoryWriteExecuted
            && !complete.NativeCallExecuted);

        NetworkIoErrorCallbackExitPopPlan zeroValues =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0,
                freshComparedByte: 1,
                freshLocalMinus28: 0,
                freshESP: 0,
                freshPopEDI: 0,
                freshPopESI: 0));
        check("Zero ESP and zero pop values are captured values and stack arithmetic wraps",
            zeroValues.Reads[0].Captured
            && zeroValues.Reads[0].Address == 0
            && zeroValues.Reads[0].Value == 0
            && zeroValues.Reads[1].Captured
            && zeroValues.Reads[1].Address == 4
            && zeroValues.Reads[1].Value == 0
            && zeroValues.ESP == 8
            && zeroValues.EAX == 0
            && zeroValues.ExactCleanCompletion);

        NetworkIoErrorCallbackExitPopPlan wrappedStack =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 0x80,
                freshLocalMinus28: 0xABCDEF01,
                freshESP: 0xFFFFFFFC,
                freshPopEDI: 0x10203040,
                freshPopESI: 0x50607080));
        check("The second stack address and final ESP wrap independently while parent EAX is retained",
            wrappedStack.Reads[0].Address == 0xFFFFFFFC
            && wrappedStack.Reads[1].Address == 0
            && wrappedStack.ESP == 4
            && wrappedStack.EAX == 0xABCDEF01
            && wrappedStack.InstructionOrder[0].ESPAfter == 0
            && wrappedStack.InstructionOrder[1].ESPAfter == 4
            && wrappedStack.FlagsAtStop
                == wrappedStack.ParentPostLoopByteGatePlan.FlagsAfterComparedByte);

        NetworkIoErrorCallbackExitPopPlan missingESP =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 1,
                freshLocalMinus28: 0x12345678,
                freshESP: null,
                freshPopEDI: 0,
                freshPopESI: 0x55));
        check("Missing fresh ESP performs no pop read and preserves the parent EAX and flags",
            missingESP.Outcome == NetworkIoErrorCallbackExitPopOutcome.MissingFreshESP
            && missingESP.ChildGateAdmitted
            && missingESP.Reads.IsEmpty
            && missingESP.InstructionOrder.IsEmpty
            && missingESP.ESP is null
            && missingESP.EAX == 0x12345678
            && missingESP.FlagsAtStop
                == missingESP.ParentPostLoopByteGatePlan.FlagsAtStop
            && missingESP.CaptureStates.FreshESP
                == NetworkIoErrorCallbackExitPopCaptureState.Missing
            && missingESP.CaptureStates.FreshPopEDIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && missingESP.CaptureStates.FreshPopESIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && !missingESP.ChildBoundedCompletionBoundaryReached
            && !missingESP.ExactChildCompletion
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion
            && missingESP.LastReachedRawFileOffset == 0xBBC9DB
            && missingESP.StopBeforeRawFileOffset == 0xBBC9DB);

        NetworkIoErrorCallbackExitPopPlan missingEDI =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 1,
                freshLocalMinus28: 0x12345678,
                freshESP: 0x7000,
                freshPopEDI: null,
                freshPopESI: 0x55));
        check("Missing EDI at DB does not advance ESP or overwrite parent EAX",
            missingEDI.Outcome
                == NetworkIoErrorCallbackExitPopOutcome.MissingFreshPopEDIDWORD
            && missingEDI.Reads.Length == 1
            && missingEDI.Reads[0].Address == 0x7000
            && missingEDI.Reads[0].Attempted
            && !missingEDI.Reads[0].Captured
            && missingEDI.ESP == 0x7000
            && missingEDI.EAX == 0x12345678
            && missingEDI.EDI == 100
            && missingEDI.ESI == 0
            && missingEDI.InstructionOrder.Length == 1
            && missingEDI.InstructionOrder[0].RawFileOffset == 0xBBC9DB
            && missingEDI.InstructionOrder[0].ESPBefore == 0x7000
            && missingEDI.InstructionOrder[0].ESPAfter == 0x7000
            && missingEDI.InstructionOrder[0].Value is null
            && !missingEDI.InstructionOrder[0].Completed
            && missingEDI.CaptureStates.FreshPopEDIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.Missing
            && missingEDI.CaptureStates.FreshPopESIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && !missingEDI.ExactChildCompletion
            && !missingEDI.ExactBoundedCompletion
            && !missingEDI.ExactCleanCompletion
            && missingEDI.LastReachedRawFileOffset == 0xBBC9DB
            && missingEDI.StopBeforeRawFileOffset == 0xBBC9DB);

        NetworkIoErrorCallbackExitPopPlan missingESI =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 1,
                freshLocalMinus28: 0x12345678,
                freshESP: 0x7000,
                freshPopEDI: 0x88,
                freshPopESI: null));
        check("Missing ESI at DC retains EDI and the single completed stack increment",
            missingESI.Outcome
                == NetworkIoErrorCallbackExitPopOutcome.MissingFreshPopESIDWORD
            && missingESI.Reads.Length == 2
            && missingESI.Reads[1].Address == 0x7004
            && missingESI.Reads[1].Attempted
            && !missingESI.Reads[1].Captured
            && missingESI.InstructionOrder.Length == 2
            && missingESI.InstructionOrder[0].Kind
                == NetworkIoErrorCallbackExitPopInstructionKind.PopEDI
            && missingESI.InstructionOrder[0].Value == 0x88
            && missingESI.InstructionOrder[0].Completed
            && missingESI.InstructionOrder[1].Kind
                == NetworkIoErrorCallbackExitPopInstructionKind.PopESI
            && missingESI.InstructionOrder[1].ESPBefore == 0x7004
            && missingESI.InstructionOrder[1].ESPAfter == 0x7004
            && !missingESI.InstructionOrder[1].Completed
            && missingESI.ESP == 0x7004
            && missingESI.EAX == 0x12345678
            && missingESI.EDI == 0x88
            && missingESI.ESI == 0
            && missingESI.FlagsAtStop
                == missingESI.ParentPostLoopByteGatePlan.FlagsAtStop
            && missingESI.CaptureStates.FreshPopESIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.Missing
            && !missingESI.PopESICompleted
            && !missingESI.ExactChildCompletion
            && !missingESI.ExactBoundedCompletion
            && !missingESI.ExactCleanCompletion
            && missingESI.LastReachedRawFileOffset == 0xBBC9DB
            && missingESI.StopBeforeRawFileOffset == 0xBBC9DC);

        NetworkIoErrorCallbackExitPopPlan takenParent =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 0,
                freshLocalMinus28: 0x12345678,
                freshESP: 0x7000,
                freshPopEDI: 1,
                freshPopESI: 2));
        NetworkIoErrorCallbackExitPopPlan dirtyParent =
            NetworkIoErrorCallbackExitPopProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 1,
                freshLocalMinus28: 0x12345678,
                freshESP: 0x7000,
                freshPopEDI: 1,
                freshPopESI: 2,
                parentCallbackReturnedNormally: true));
        check("Taken or dirty parents preserve their exact endpoint and do not admit child captures",
            !takenParent.ChildGateAdmitted
            && takenParent.ParentPostLoopByteGatePlan.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
            && takenParent.Reads.IsEmpty
            && takenParent.LastReachedRawFileOffset
                == takenParent.ParentPostLoopByteGatePlan.LastReachedRawFileOffset
            && takenParent.StopBeforeRawFileOffset
                == takenParent.ParentPostLoopByteGatePlan.StopBeforeRawFileOffset
            && takenParent.FlagsAtStop
                == takenParent.ParentPostLoopByteGatePlan.FlagsAtStop
            && takenParent.CaptureStates.FreshESP
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && takenParent.CaptureStates.FreshPopEDIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && takenParent.CaptureStates.FreshPopESIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            && !takenParent.ExactChildCompletion
            && takenParent.BoundedCompletionBoundaryReached
                == takenParent.ParentPostLoopByteGatePlan
                    .BoundedCompletionBoundaryReached
            && takenParent.ExactBoundedCompletion
                == takenParent.ParentPostLoopByteGatePlan.ExactBoundedCompletion
            && !takenParent.ExactCleanCompletion
            && !dirtyParent.ChildGateAdmitted
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.LastReachedRawFileOffset
                == dirtyParent.ParentPostLoopByteGatePlan.LastReachedRawFileOffset
            && dirtyParent.StopBeforeRawFileOffset
                == dirtyParent.ParentPostLoopByteGatePlan.StopBeforeRawFileOffset
            && dirtyParent.FlagsAtStop
                == dirtyParent.ParentPostLoopByteGatePlan.FlagsAtStop
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentPostLoopByteGatePlan.ExactBoundedCompletion
            && !dirtyParent.ExactChildCompletion
            && !dirtyParent.ExactCleanCompletion);

        NetworkIoErrorCallbackExitPopPlan reprojection =
            NetworkIoErrorCallbackExitPopProjection.Project(complete.Capture with { });
        check("The immutable parent trace remains a semantic prefix on reprojection",
            reprojection.Outcome == complete.Outcome
            && reprojection.Capture.FreshESP == complete.Capture.FreshESP
            && reprojection.Capture.FreshPopEDIDWORD
                == complete.Capture.FreshPopEDIDWORD
            && reprojection.Capture.FreshPopESIDWORD
                == complete.Capture.FreshPopESIDWORD
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.Reads.SequenceEqual(complete.Reads)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.ParentPostLoopByteGatePlan.Outcome
                == complete.ParentPostLoopByteGatePlan.Outcome
            && reprojection.ParentPostLoopByteGatePlan.Reads
                .Select(read => (read.RawFileOffset, read.Kind, read.Address,
                    read.Width, read.Attempted, read.Captured, read.Value))
                .SequenceEqual(complete.ParentPostLoopByteGatePlan.Reads
                    .Select(read => (read.RawFileOffset, read.Kind, read.Address,
                        read.Width, read.Attempted, read.Captured, read.Value)))
            && reprojection.ParentPostLoopByteGatePlan.InstructionOrder
                .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                    instruction.Address, instruction.Value,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTaken))
                .SequenceEqual(complete.ParentPostLoopByteGatePlan.InstructionOrder
                    .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                        instruction.Address, instruction.Value,
                        instruction.BranchTargetRawFileOffset, instruction.BranchTaken)))
            && reprojection.OrderedEffects.Take(
                    complete.ParentPostLoopByteGatePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentPostLoopEffect?.Sequence)
                .SequenceEqual(complete.ParentPostLoopByteGatePlan.OrderedEffects
                    .Select(effect => (int?)effect.Sequence))
            && reprojection.OrderedEffects.Take(
                    complete.ParentPostLoopByteGatePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentPostLoopEffect?.Instruction)
                .SequenceEqual(complete.ParentPostLoopByteGatePlan.OrderedEffects
                    .Select(effect => effect.Instruction))
            && reprojection.OrderedEffects.Take(
                    complete.ParentPostLoopByteGatePlan.OrderedEffects.Length)
                .Select(effect => effect.ParentPostLoopEffect?.Read)
                .SequenceEqual(complete.ParentPostLoopByteGatePlan.OrderedEffects
                    .Select(effect => effect.Read)));
    }

    private static NetworkIoErrorCallbackExitPopInputs Inputs(
        uint? freshLocalMinus24,
        byte? freshComparedByte,
        uint? freshLocalMinus28,
        uint? freshESP,
        uint? freshPopEDI,
        uint? freshPopESI,
        uint? parentLoopEDI = 99,
        uint? freshParentEBP = 0x1000,
        uint? freshPostCallEBP = 0x1000,
        bool? parentCallbackReturnedNormally = null) =>
        new(
            new(
                new(
                    new(
                        new(
                            ParentPassCapture: CleanMissingKeyParent(
                                parentCallbackReturnedNormally),
                            FreshEBP: freshParentEBP,
                            FreshDispatchKeyDWORD: 0,
                            FreshESP: 0x2000,
                            FreshVptrDWORD: 0),
                        FreshSlot20TargetDWORD: 0,
                        OpaqueCallReturnedNormally: true),
                    FreshPostCallEBP: freshPostCallEBP,
                    FreshLocalMinus8DWORD: 0,
                    FreshLocalMinus10DWORD: 0,
                    FreshPostCallEDI: parentLoopEDI),
                freshLocalMinus24,
                freshComparedByte,
                freshLocalMinus28),
            freshESP,
            freshPopEDI,
            freshPopESI);

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
