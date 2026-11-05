using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackPostLoopByteGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The byte-gate window and instruction anchors match the bounded native bytes",
            !NetworkIoErrorCallbackPostLoopByteGateProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackPostLoopByteGateProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackPostLoopByteGateProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackPostLoopByteGateProjection.ExecutesCalls
            && !NetworkIoErrorCallbackPostLoopByteGateProjection.ExecutesPops
            && !NetworkIoErrorCallbackPostLoopByteGateProjection.EntersSelectedBranchDestination
            && NetworkIoErrorCallbackPostLoopByteGateProjection.EntryRawFileOffset
                == 0xBBC9CB
            && NetworkIoErrorCallbackPostLoopByteGateProjection.MoveEAXFromEBPMinus24RawFileOffset
                == 0xBBC9CB
            && NetworkIoErrorCallbackPostLoopByteGateProjection.CompareByteAtEAXPlus4RawFileOffset
                == 0xBBC9CE
            && NetworkIoErrorCallbackPostLoopByteGateProjection.MoveEAXFromEBPMinus28RawFileOffset
                == 0xBBC9D2
            && NetworkIoErrorCallbackPostLoopByteGateProjection.ZeroBranchRawFileOffset
                == 0xBBC9D5
            && NetworkIoErrorCallbackPostLoopByteGateProjection.ZeroBranchNextInstructionRawFileOffset
                == 0xBBC9DB
            && NetworkIoErrorCallbackPostLoopByteGateProjection.ZeroBranchTargetRawFileOffset
                == 0xBBC8B0
            && NetworkIoErrorCallbackPostLoopByteGateProjection.FallThroughRawFileOffset
                == 0xBBC9DB
            && NetworkIoErrorCallbackPostLoopByteGateProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0x8B, 0x45, 0xDC,
                    0x80, 0x78, 0x04, 0x00,
                    0x8B, 0x45, 0xD8,
                    0x0F, 0x84, 0xD5, 0xFE, 0xFF, 0xFF
                }));

        NetworkIoErrorCallbackPostLoopByteGatePlan complete =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 1,
                freshLocalMinus28: 0x4000));
        check("A clean not-taken parent branch admits the three fresh captures in order",
            complete.ChildGateAdmitted
            && complete.ParentLoopGatePlan.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.FallThroughTo9CB
            && complete.ParentLoopGatePlan.LastReachedRawFileOffset == 0xBBC9C5
            && complete.ParentLoopGatePlan.StopBeforeRawFileOffset == 0xBBC9CB
            && complete.ParentLoopGatePlan.BranchExecuted
            && !complete.ParentLoopGatePlan.BranchTaken
            && complete.ParentLoopGatePlan.ExactCleanCompletion
            && complete.EBP == 0x1000
            && complete.EAX == 0x4000
            && complete.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.FallThroughTo9DB
            && complete.Reads.Length == 3
            && complete.Reads[0].RawFileOffset == 0xBBC9CB
            && complete.Reads[0].Address == 0xFDC
            && complete.Reads[0].Width == 4
            && complete.Reads[0].Value == 0x2000
            && complete.Reads[1].RawFileOffset == 0xBBC9CE
            && complete.Reads[1].Address == 0x2004
            && complete.Reads[1].Width == 1
            && complete.Reads[1].Value == 1
            && complete.Reads[2].RawFileOffset == 0xBBC9D2
            && complete.Reads[2].Address == 0xFD8
            && complete.Reads[2].Width == 4
            && complete.Reads[2].Value == 0x4000
            && complete.CaptureStates
                == new NetworkIoErrorCallbackPostLoopByteGateCaptureStates(
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured,
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured,
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Captured)
            && complete.FlagsAfterComparedByte
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false)
            && complete.FlagsAtStop == complete.FlagsAfterComparedByte
            && complete.BranchExecuted
            && !complete.BranchTaken
            && complete.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[] { 0xBBC9CB, 0xBBC9CE, 0xBBC9D2, 0xBBC9D5 })
            && complete.InstructionOrder[^1].BranchTargetRawFileOffset == 0xBBC9DB
            && complete.InstructionOrder[^1].BranchTaken == false
            && complete.ChildBoundedCompletionBoundaryReached
            && complete.ExactChildCompletion
            && complete.ExactCleanCompletion
            && complete.LastReachedRawFileOffset == 0xBBC9D5
            && complete.StopBeforeRawFileOffset == 0xBBC9DB
            && !complete.BranchTargetEntered
            && !complete.FallThroughEntered
            && !complete.MemoryWriteExecuted
            && !complete.CallExecuted
            && !complete.PopExecuted
            && !complete.NativeInstructionsExecuted);

        NetworkIoErrorCallbackPostLoopByteGatePlan missingFirst =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: null,
                freshComparedByte: 0,
                freshLocalMinus28: 0x44));
        check("A missing 9CB DWORD has no EAX and leaves later captures unused",
            missingFirst.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.MissingFreshLocalMinus24DWORD
            && missingFirst.Reads.Length == 1
            && missingFirst.Reads[0].Attempted
            && !missingFirst.Reads[0].Captured
            && missingFirst.Reads[0].Value is null
            && missingFirst.EAX is null
            && missingFirst.FlagsAtStop
                == missingFirst.ParentLoopGatePlan.FlagsAtStop
            && missingFirst.CaptureStates.FreshLocalMinus24DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing
            && missingFirst.CaptureStates.FreshComparedBYTE
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && missingFirst.CaptureStates.FreshLocalMinus28DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && !missingFirst.ChildBoundedCompletionBoundaryReached
            && !missingFirst.ExactChildCompletion
            && !missingFirst.ExactCleanCompletion
            && missingFirst.LastReachedRawFileOffset == 0xBBC9CB
            && missingFirst.StopBeforeRawFileOffset == 0xBBC9CB);

        NetworkIoErrorCallbackPostLoopByteGatePlan allMissing =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: null,
                freshComparedByte: null,
                freshLocalMinus28: null));
        check("All missing fresh captures leave exact child completion false",
            allMissing.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.MissingFreshLocalMinus24DWORD
            && allMissing.CaptureStates
                == new NetworkIoErrorCallbackPostLoopByteGateCaptureStates(
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing,
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Unreached,
                    NetworkIoErrorCallbackPostLoopByteGateCaptureState.Unreached)
            && !allMissing.ExactChildCompletion
            && !allMissing.ExactCleanCompletion);

        NetworkIoErrorCallbackPostLoopByteGatePlan missingByte =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0xFFFFFFFC,
                freshComparedByte: null,
                freshLocalMinus28: 0x1234));
        check("A missing CE byte retains the first EAX and parent CMP flags",
            missingByte.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.MissingFreshComparedByte
            && missingByte.EAX == 0xFFFFFFFC
            && missingByte.Reads.Length == 2
            && missingByte.Reads[1].Address == 0
            && missingByte.Reads[1].Width == 1
            && missingByte.Reads[1].Attempted
            && !missingByte.Reads[1].Captured
            && missingByte.FlagsAfterComparedByte is null
            && missingByte.FlagsAtStop
                == missingByte.ParentLoopGatePlan.FlagsAtStop
            && missingByte.CaptureStates.FreshComparedBYTE
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.Missing
            && missingByte.CaptureStates.FreshLocalMinus28DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && missingByte.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[] { 0xBBC9CB, 0xBBC9CE })
            && !missingByte.BranchExecuted
            && !missingByte.ExactChildCompletion
            && missingByte.LastReachedRawFileOffset == 0xBBC9CB
            && missingByte.StopBeforeRawFileOffset == 0xBBC9CE);

        NetworkIoErrorCallbackPostLoopByteGatePlan missingLast =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 0x80,
                freshLocalMinus28: null));
        check("A missing D2 DWORD retains first EAX and CMP8 flags without reaching JZ",
            missingLast.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.MissingFreshLocalMinus28DWORD
            && missingLast.EAX == 0x2000
            && missingLast.Reads.Length == 3
            && missingLast.Reads[2].Address == 0xFD8
            && missingLast.Reads[2].Attempted
            && !missingLast.Reads[2].Captured
            && missingLast.FlagsAfterComparedByte
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false)
            && missingLast.FlagsAtStop == missingLast.FlagsAfterComparedByte
            && missingLast.InstructionOrder.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(new uint[] { 0xBBC9CB, 0xBBC9CE, 0xBBC9D2 })
            && !missingLast.BranchExecuted
            && !missingLast.ExactChildCompletion
            && missingLast.LastReachedRawFileOffset == 0xBBC9CE
            && missingLast.StopBeforeRawFileOffset == 0xBBC9D2);

        NetworkIoErrorCallbackPostLoopByteGatePlan zeroByte =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x2000,
                freshComparedByte: 0,
                freshLocalMinus28: 0x3333));
        check("CMP8 zero selects the 8B0 JZ target without entering it",
            zeroByte.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
            && zeroByte.EAX == 0x3333
            && zeroByte.FlagsAfterComparedByte
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false)
            && zeroByte.BranchExecuted
            && zeroByte.BranchTaken
            && zeroByte.InstructionOrder[^1].BranchTargetRawFileOffset == 0xBBC8B0
            && zeroByte.StopBeforeRawFileOffset == 0xBBC8B0
            && !zeroByte.BranchTargetEntered
            && !zeroByte.FallThroughEntered
            && zeroByte.ExactChildCompletion);

        NetworkIoErrorCallbackPostLoopByteGatePlan zeroPointer =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0,
                freshComparedByte: 0,
                freshLocalMinus28: 0));
        check("Captured zero pointer and second-local values are not mistaken for missing inputs",
            zeroPointer.Reads[0].Captured
            && zeroPointer.Reads[0].Value == 0
            && zeroPointer.Reads[1].Address == 4
            && zeroPointer.Reads[1].Captured
            && zeroPointer.Reads[2].Captured
            && zeroPointer.EAX == 0
            && zeroPointer.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0);

        check("Every compared byte selects the exact 8-bit CMP flags and JZ result",
            Enumerable.Range(byte.MinValue, byte.MaxValue + 1).All(value =>
            {
                byte compared = (byte)value;
                NetworkIoErrorCallbackPostLoopByteGatePlan plan =
                    NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                        freshLocalMinus24: 0xFFFFFFFC,
                        freshComparedByte: compared,
                        freshLocalMinus28: 0x12345678));
                NetworkIoErrorCallbackLoopGateFlags expected =
                    ExpectedFlags(compared);
                bool zero = compared == 0;
                return plan.FlagsAfterComparedByte == expected
                    && plan.FlagsAtStop == expected
                    && plan.EAX == 0x12345678
                    && plan.BranchTaken == zero
                    && plan.Outcome == (zero
                        ? NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
                        : NetworkIoErrorCallbackPostLoopByteGateOutcome.FallThroughTo9DB)
                    && plan.StopBeforeRawFileOffset
                        == (zero ? 0xBBC8B0u : 0xBBC9DBu)
                    && !plan.BranchTargetEntered
                    && !plan.FallThroughEntered;
            }));

        NetworkIoErrorCallbackPostLoopByteGatePlan takenParent =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x22,
                freshComparedByte: 0,
                freshLocalMinus28: 0x33,
                parentLoopEDI: 98));
        NetworkIoErrorCallbackPostLoopByteGatePlan dirtyParent =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x22,
                freshComparedByte: 0,
                freshLocalMinus28: 0x33,
                parentCallbackReturnedNormally: true));
        check("Taken and dirty parent endpoints reject the child and preserve their flags and bounds",
            !takenParent.ChildGateAdmitted
            && takenParent.ParentLoopGatePlan.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.BranchTo8F0
            && takenParent.LastReachedRawFileOffset
                == takenParent.ParentLoopGatePlan.LastReachedRawFileOffset
            && takenParent.StopBeforeRawFileOffset
                == takenParent.ParentLoopGatePlan.StopBeforeRawFileOffset
            && takenParent.StopBeforeRawFileOffset == 0xBBC8F0
            && takenParent.FlagsAtStop == takenParent.ParentLoopGatePlan.FlagsAtStop
            && takenParent.CaptureStates.FreshLocalMinus24DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && takenParent.CaptureStates.FreshComparedBYTE
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && takenParent.CaptureStates.FreshLocalMinus28DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && takenParent.Reads.IsEmpty
            && !takenParent.ExactChildCompletion
            && !dirtyParent.ChildGateAdmitted
            && dirtyParent.ParentLoopGatePlan.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.ParentReturnNotAtExact9BB
            && dirtyParent.LastReachedRawFileOffset == 0xBBC97D
            && dirtyParent.StopBeforeRawFileOffset == 0xBBC97D
            && dirtyParent.FlagsAtStop == dirtyParent.ParentLoopGatePlan.FlagsAtStop
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.CaptureStates.FreshLocalMinus24DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshComparedBYTE
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshLocalMinus28DWORD
                == NetworkIoErrorCallbackPostLoopByteGateCaptureState.SuppliedButUnused
            && !dirtyParent.ExactChildCompletion);

        NetworkIoErrorCallbackPostLoopByteGatePlan wrappedAddresses =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0xFFFFFFFC,
                freshComparedByte: 0,
                freshLocalMinus28: 0x55,
                freshPostCallEBP: 0));
        check("All EBP and EAX address arithmetic wraps independently, including address zero",
            wrappedAddresses.Reads[0].Address == 0xFFFFFFDC
            && wrappedAddresses.Reads[1].Address == 0
            && wrappedAddresses.Reads[2].Address == 0xFFFFFFD8
            && wrappedAddresses.EBP == 0
            && wrappedAddresses.EAX == 0x55);

        NetworkIoErrorCallbackPostLoopByteGatePlan aliasedCapture =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(Inputs(
                freshLocalMinus24: 0x1FF8,
                freshComparedByte: 0x5A,
                freshLocalMinus28: 0xAABBCCDD,
                freshPostCallEBP: 0x2024));
        check("The later local is independently captured even when it aliases the compared byte and prior push",
            aliasedCapture.ParentLoopGatePlan.ParentReturnPlan.ParentPreparationPlan.Pushes[0]
                .StackPointerAfter == 0x1FFC
            && aliasedCapture.Reads[1].Address == 0x1FFC
            && aliasedCapture.Reads[1].Value == 0x5A
            && aliasedCapture.Reads[2].Address == 0x1FFC
            && aliasedCapture.Reads[2].Value == 0xAABBCCDD
            && aliasedCapture.EAX == 0xAABBCCDD
            && aliasedCapture.FlagsAtStop == aliasedCapture.FlagsAfterComparedByte);

        NetworkIoErrorCallbackPostLoopByteGatePlan reprojection =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(complete.Capture with { });
        int parentEffectCount = complete.ParentLoopGatePlan.OrderedEffects.Length;
        check("The parent trace is an unchanged prefix and immutable inputs reproject semantically",
            reprojection.Outcome == complete.Outcome
            && reprojection.Capture.FreshLocalMinus24DWORD
                == complete.Capture.FreshLocalMinus24DWORD
            && reprojection.Capture.FreshComparedBYTE
                == complete.Capture.FreshComparedBYTE
            && reprojection.Capture.FreshLocalMinus28DWORD
                == complete.Capture.FreshLocalMinus28DWORD
            && reprojection.CaptureStates == complete.CaptureStates
            && reprojection.Reads.SequenceEqual(complete.Reads)
            && reprojection.InstructionOrder.SequenceEqual(complete.InstructionOrder)
            && reprojection.OrderedEffects.Length == complete.OrderedEffects.Length
            && reprojection.OrderedEffects.Take(parentEffectCount)
                .Select(effect => effect.ParentLoopGateEffect)
                .SequenceEqual(reprojection.ParentLoopGatePlan.OrderedEffects
                    .Select(effect =>
                        (NetworkIoErrorCallbackLoopGateOrderedEffect?)effect))
            && reprojection.OrderedEffects.Skip(parentEffectCount)
                .SequenceEqual(complete.OrderedEffects.Skip(parentEffectCount))
            && reprojection.ParentLoopGatePlan.Outcome
                == complete.ParentLoopGatePlan.Outcome
            && reprojection.ParentLoopGatePlan.OrderedEffects
                .Select(effect => (effect.Sequence, effect.Instruction, effect.Read))
                .SequenceEqual(complete.ParentLoopGatePlan.OrderedEffects
                    .Select(effect => (effect.Sequence, effect.Instruction, effect.Read))));
    }

    private static NetworkIoErrorCallbackPostLoopByteGateInputs Inputs(
        uint? freshLocalMinus24,
        byte? freshComparedByte,
        uint? freshLocalMinus28,
        uint? parentLoopEDI = 99,
        uint? freshParentEBP = 0x1000,
        uint? freshPostCallEBP = 0x1000,
        bool? parentCallbackReturnedNormally = null) =>
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
            freshLocalMinus28);

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

    private static NetworkIoErrorCallbackLoopGateFlags ExpectedFlags(byte value)
    {
        byte folded = value;
        folded ^= (byte)(folded >> 4);
        folded &= 0x0F;
        return new(
            Carry: false,
            Parity: ((0x6996 >> folded) & 1) == 0,
            AuxiliaryCarry: false,
            Zero: value == 0,
            Sign: (value & 0x80) != 0,
            Overflow: false);
    }
}
