using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackLoopGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The loop-gate window pins the two local loads, increment, compare, and signed branch",
            !NetworkIoErrorCallbackLoopGateProjection.ExecutesNativeInstructions
            && !NetworkIoErrorCallbackLoopGateProjection.ExecutesNativeReads
            && !NetworkIoErrorCallbackLoopGateProjection.ExecutesMemoryWrites
            && !NetworkIoErrorCallbackLoopGateProjection.EntersSelectedBranchTarget
            && NetworkIoErrorCallbackLoopGateProjection.EntryRawFileOffset == 0xBBC9BB
            && NetworkIoErrorCallbackLoopGateProjection.MoveESIRawFileOffset == 0xBBC9BE
            && NetworkIoErrorCallbackLoopGateProjection.IncrementEDIRawFileOffset == 0xBBC9C1
            && NetworkIoErrorCallbackLoopGateProjection.CompareEDIRawFileOffset == 0xBBC9C2
            && NetworkIoErrorCallbackLoopGateProjection.SignedBranchRawFileOffset == 0xBBC9C5
            && NetworkIoErrorCallbackLoopGateProjection.BranchTargetRawFileOffset == 0xBBC8F0
            && NetworkIoErrorCallbackLoopGateProjection.FallThroughRawFileOffset == 0xBBC9CB
            && NetworkIoErrorCallbackLoopGateProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0x8B, 0x45, 0xF8,
                    0x8B, 0x75, 0xF0,
                    0x47,
                    0x83, 0xFF, 0x64,
                    0x0F, 0x8C, 0x25, 0xFF, 0xFF, 0xFF
                }));

        NetworkIoErrorCallbackLoopGatePlan lower =
            Project(freshPostCallEDI: 98);
        check("EDI 98 increments to 99 and takes signed JL with exact CMP flags",
            lower.ChildGateAdmitted
            && lower.Outcome == NetworkIoErrorCallbackLoopGateOutcome.BranchTo8F0
            && lower.EAX == 0
            && lower.ESI == 0
            && lower.EDI == 99
            && lower.IncrementExecuted
            && lower.CompareExecuted
            && lower.BranchExecuted
            && lower.BranchTaken
            && lower.FlagsAtStop == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false)
            && lower.FlagsAfterIncrement == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: null,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false)
            && lower.FlagsAfterCompare == lower.FlagsAtStop
            && lower.LastReachedRawFileOffset == 0xBBC9C5
            && lower.StopBeforeRawFileOffset == 0xBBC8F0
            && lower.BoundedCompletionBoundaryReached
            && lower.ExactBoundedCompletion
            && lower.ExactCleanCompletion
            && !lower.BranchTargetEntered
            && !lower.MemoryWriteExecuted);

        NetworkIoErrorCallbackLoopGatePlan equal =
            Project(freshPostCallEDI: 99);
        check("EDI 99 increments to the 100 threshold and falls through on signed equality",
            equal.Outcome == NetworkIoErrorCallbackLoopGateOutcome.FallThroughTo9CB
            && equal.EDI == 100
            && !equal.BranchTaken
            && equal.FlagsAtStop == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false)
            && equal.FlagsAfterIncrement == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: null,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false)
            && equal.StopBeforeRawFileOffset == 0xBBC9CB
            && !equal.BranchTargetEntered);

        NetworkIoErrorCallbackLoopGatePlan wrapToZero =
            Project(freshPostCallEDI: uint.MaxValue);
        NetworkIoErrorCallbackLoopGatePlan incrementOverflow =
            Project(freshPostCallEDI: 0x7FFFFFFF);
        NetworkIoErrorCallbackLoopGatePlan signedMinimum =
            Project(freshPostCallEDI: 0x80000000);
        NetworkIoErrorCallbackLoopGatePlan wrapToSignedMinusOne =
            Project(freshPostCallEDI: 0xFFFFFFFE);
        check("INC wraps at DWORD width and preserves unknown CF while computing its other flags",
            wrapToZero.EDI == 0
            && wrapToZero.Outcome == NetworkIoErrorCallbackLoopGateOutcome.BranchTo8F0
            && wrapToZero.FlagsAfterIncrement == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: null,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: true,
                Sign: false,
                Overflow: false)
            && wrapToZero.FlagsAtStop == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false)
            && incrementOverflow.EDI == 0x80000000
            && incrementOverflow.BranchTaken
            && incrementOverflow.FlagsAfterIncrement
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: null,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: true,
                    Overflow: true)
            && incrementOverflow.FlagsAtStop == new NetworkIoErrorCallbackLoopGateFlags(
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true)
            && signedMinimum.EDI == 0x80000001
            && signedMinimum.BranchTaken
            && signedMinimum.FlagsAfterIncrement
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: null,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false)
            && signedMinimum.FlagsAtStop?.Overflow == true
            && wrapToSignedMinusOne.EDI == uint.MaxValue
            && wrapToSignedMinusOne.BranchTaken
            && wrapToSignedMinusOne.FlagsAfterIncrement
                == new NetworkIoErrorCallbackLoopGateFlags(
                    Carry: null,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false)
            && wrapToSignedMinusOne.FlagsAtStop?.Overflow == false);

        NetworkIoErrorCallbackLoopGatePlan missingEBP =
            Project(freshPostCallEBP: null);
        NetworkIoErrorCallbackLoopGatePlan missingLocal8 =
            Project(freshLocalMinus8DWORD: null);
        NetworkIoErrorCallbackLoopGatePlan missingLocal10 =
            Project(freshLocalMinus10DWORD: null);
        NetworkIoErrorCallbackLoopGatePlan missingEDI =
            Project(freshPostCallEDI: null);
        check("Missing EBP leaves both local captures unreached and supplied locals unused",
            missingEBP.ChildGateAdmitted
            && missingEBP.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.MissingFreshPostCallEBP
            && missingEBP.Reads.IsEmpty
            && missingEBP.InstructionOrder.IsEmpty
            && missingEBP.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackLoopGateCaptureState.Missing
            && missingEBP.CaptureStates.FreshLocalMinus8DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && missingEBP.CaptureStates.FreshLocalMinus10DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && missingEBP.CaptureStates.FreshPostCallEDI
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && missingEBP.StopBeforeRawFileOffset == 0xBBC9BB
            && missingEBP.ParentReturnPlan.ExactBoundedCompletion
            && !missingEBP.BoundedCompletionBoundaryReached
            && !missingEBP.ExactBoundedCompletion
            && !missingEBP.ExactCleanCompletion);
        check("A missing first local has no EAX value and does not attempt the second load",
            missingLocal8.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.MissingFreshLocalMinus8DWORD
            && missingLocal8.Reads.Length == 1
            && missingLocal8.Reads[0].Register
                == NetworkIoErrorCallbackLoopGateRegister.EAX
            && missingLocal8.Reads[0].Address == 0xFF8
            && missingLocal8.Reads[0].Attempted
            && !missingLocal8.Reads[0].Captured
            && missingLocal8.EAX is null
            && missingLocal8.FlagsAfterMoveEAX is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingLocal8.CaptureStates.FreshLocalMinus10DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && missingLocal8.InstructionOrder.Length == 1
            && missingLocal8.ParentReturnPlan.ExactBoundedCompletion
            && !missingLocal8.ExactBoundedCompletion
            && !missingLocal8.ExactCleanCompletion
            && !missingLocal8.CompareExecuted);
        check("A missing second local preserves EAX and emits no EDI or CMP metadata",
            missingLocal10.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.MissingFreshLocalMinus10DWORD
            && missingLocal10.Reads.Length == 2
            && missingLocal10.EAX == 0
            && missingLocal10.ESI is null
            && missingLocal10.FlagsAfterMoveEAX is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingLocal10.FlagsAfterMoveESI is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingLocal10.CaptureStates.FreshPostCallEDI
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && missingLocal10.InstructionOrder.Select(step => step.Kind).SequenceEqual(
                new[]
                {
                    NetworkIoErrorCallbackLoopGateInstructionKind.MoveEAXFromEBPMinus8,
                    NetworkIoErrorCallbackLoopGateInstructionKind.MoveESIFromEBPMinus10
                })
            && missingLocal10.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingLocal10.ParentReturnPlan.ExactBoundedCompletion
            && !missingLocal10.BoundedCompletionBoundaryReached
            && !missingLocal10.ExactBoundedCompletion
            && !missingLocal10.ExactCleanCompletion
            && !missingLocal10.CompareExecuted);
        check("A missing EDI preserves both fresh locals and the unknown post-call flags",
            missingEDI.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.MissingFreshPostCallEDI
            && missingEDI.EAX == 0
            && missingEDI.ESI == 0
            && missingEDI.EDI is null
            && missingEDI.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                AuxiliaryCarry: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !missingEDI.IncrementExecuted
            && !missingEDI.CompareExecuted
            && !missingEDI.BranchExecuted
            && missingEDI.ParentReturnPlan.ExactBoundedCompletion
            && !missingEDI.BoundedCompletionBoundaryReached
            && !missingEDI.ExactBoundedCompletion
            && !missingEDI.ExactCleanCompletion);

        NetworkIoErrorCallbackLoopGatePlan wrappedEBP =
            Project(freshPostCallEBP: 0);
        NetworkIoErrorCallbackLoopGatePlan zeroScalars =
            Project(freshLocalMinus8DWORD: 0, freshLocalMinus10DWORD: 0);
        NetworkIoErrorCallbackLoopGatePlan aliasedParentWrite =
            Project(freshPostCallEBP: 0x2004, freshLocalMinus8DWORD: 0);
        check("Fresh local addresses use wrapping DWORD EBP arithmetic and zero remains a captured scalar",
            wrappedEBP.Reads[0].Address == 0xFFFFFFF8
            && wrappedEBP.Reads[1].Address == 0xFFFFFFF0
            && wrappedEBP.Reads[0].Address != wrappedEBP.Reads[1].Address
            && zeroScalars.EAX == 0
            && zeroScalars.ESI == 0
            && zeroScalars.CaptureStates.FreshLocalMinus8DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.Captured
            && zeroScalars.CaptureStates.FreshLocalMinus10DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.Captured
            && aliasedParentWrite.ParentReturnPlan.ParentPreparationPlan.Pushes[0]
                .StackPointerAfter == 0x1FFC
            && aliasedParentWrite.Reads[0].Address == 0x1FFC
            && aliasedParentWrite.Reads[1].Address == 0x1FF4
            && aliasedParentWrite.EAX == 0
            && aliasedParentWrite.EAX
                != aliasedParentWrite.ParentReturnPlan.FailedCompletionErrorEAX);

        NetworkIoErrorCallbackLoopGatePlan falseParent =
            Project(opaqueCallReturnedNormally: false);
        NetworkIoErrorCallbackLoopGatePlan missingParent =
            Project(freshParentEBP: null);
        NetworkIoErrorCallbackLoopGatePlan dirtyParent =
            Project(parentPassCallbackReturnedNormally: true);
        NetworkIoErrorCallbackLoopGatePlan pendingNormalReturn =
            Project(opaqueCallReturnedNormally: null);
        check("A false or incomplete parent preserves its endpoint and never admits the loop gate",
            !falseParent.ChildGateAdmitted
            && falseParent.Outcome
                == NetworkIoErrorCallbackLoopGateOutcome.ParentReturnNotAtExact9BB
            && falseParent.LastReachedRawFileOffset
                == falseParent.ParentReturnPlan.LastReachedRawFileOffset
            && falseParent.StopBeforeRawFileOffset
                == falseParent.ParentReturnPlan.StopBeforeRawFileOffset
            && falseParent.StopBeforeRawFileOffset != 0xBBC9BB
            && falseParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && !falseParent.BoundedCompletionBoundaryReached
            && !missingParent.ChildGateAdmitted
            && missingParent.LastReachedRawFileOffset
                == missingParent.ParentReturnPlan.LastReachedRawFileOffset
            && missingParent.StopBeforeRawFileOffset
                == missingParent.ParentReturnPlan.StopBeforeRawFileOffset
            && missingParent.StopBeforeRawFileOffset != 0xBBC9BB
            && missingParent.ExactBoundedCompletion
                == missingParent.ParentReturnPlan.ExactBoundedCompletion
            && !missingParent.ExactBoundedCompletion);
        check("A dirty parent at 97D forwards only its actual bound and flags without consuming child captures",
            !dirtyParent.ChildGateAdmitted
            && dirtyParent.ParentReturnPlan.ParentPreparationPlan.Outcome
                == NetworkIoErrorCallbackPreparationOutcome.ParentPassBoundaryNotClean
            && dirtyParent.ParentReturnPlan.ParentPreparationPlan.ParentPassPlan.Attempts[0]
                .CaptureStates.CallbackReturnedNormally
                == NetworkIoCompletionCaptureState.SuppliedButUnused
            && dirtyParent.ParentReturnPlan.LastReachedRawFileOffset == 0xBBC97D
            && dirtyParent.ParentReturnPlan.StopBeforeRawFileOffset == 0xBBC97D
            && dirtyParent.LastReachedRawFileOffset == 0xBBC97D
            && dirtyParent.StopBeforeRawFileOffset == 0xBBC97D
            && dirtyParent.BoundedCompletionBoundaryReached
                == dirtyParent.ParentReturnPlan.BoundedCompletionBoundaryReached
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentReturnPlan.ExactBoundedCompletion
            && dirtyParent.FlagsAtStop is NetworkIoErrorCallbackLoopGateFlags
                dirtyFlags
            && dirtyFlags.Carry
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.Carry
            && dirtyFlags.Parity
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.Parity
            && dirtyFlags.AuxiliaryCarry
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.AuxiliaryCarry
            && dirtyFlags.Zero
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.Zero
            && dirtyFlags.Sign
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.Sign
            && dirtyFlags.Overflow
                == dirtyParent.ParentReturnPlan.FlagsAtStop?.Overflow
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.InstructionOrder.IsEmpty
            && dirtyParent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshLocalMinus8DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshLocalMinus10DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && dirtyParent.CaptureStates.FreshPostCallEDI
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused);
        check("A pending normal-return observation is rejected without replacing the parent endpoint",
            !pendingNormalReturn.ChildGateAdmitted
            && pendingNormalReturn.ParentReturnPlan.Outcome
                == NetworkIoErrorCallbackReturnOutcome.MissingOpaqueCallReturnObservation
            && pendingNormalReturn.ParentReturnPlan.LastReachedRawFileOffset == 0xBBC983
            && pendingNormalReturn.ParentReturnPlan.StopBeforeRawFileOffset == 0xBBC983
            && pendingNormalReturn.LastReachedRawFileOffset == 0xBBC983
            && pendingNormalReturn.StopBeforeRawFileOffset == 0xBBC983
            && pendingNormalReturn.BoundedCompletionBoundaryReached
                == pendingNormalReturn.ParentReturnPlan.BoundedCompletionBoundaryReached
            && pendingNormalReturn.ExactBoundedCompletion
                == pendingNormalReturn.ParentReturnPlan.ExactBoundedCompletion
            && pendingNormalReturn.Reads.IsEmpty
            && pendingNormalReturn.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && pendingNormalReturn.CaptureStates.FreshLocalMinus8DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && pendingNormalReturn.CaptureStates.FreshLocalMinus10DWORD
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused
            && pendingNormalReturn.CaptureStates.FreshPostCallEDI
                == NetworkIoErrorCallbackLoopGateCaptureState.SuppliedButUnused);

        int parentEffectCount = lower.ParentReturnPlan.OrderedEffects.Length;
        check("The parent trace is an unchanged ordered prefix followed by two reads and the full branch trace",
            lower.OrderedEffects.Length == parentEffectCount + 5
            && lower.OrderedEffects.Take(parentEffectCount)
                .Select(effect => effect.ParentReturnEffect)
                .SequenceEqual(lower.ParentReturnPlan.OrderedEffects
                    .Select(effect => (NetworkIoErrorCallbackReturnOrderedEffect?)effect))
            && lower.OrderedEffects.Skip(parentEffectCount)
                .Select(effect => effect.Instruction?.Kind)
                .SequenceEqual(new NetworkIoErrorCallbackLoopGateInstructionKind?[]
                {
                    NetworkIoErrorCallbackLoopGateInstructionKind.MoveEAXFromEBPMinus8,
                    NetworkIoErrorCallbackLoopGateInstructionKind.MoveESIFromEBPMinus10,
                    NetworkIoErrorCallbackLoopGateInstructionKind.IncrementEDI,
                    NetworkIoErrorCallbackLoopGateInstructionKind.CompareEDIWith64,
                    NetworkIoErrorCallbackLoopGateInstructionKind.SignedLessThanBranch
                })
            && lower.OrderedEffects[parentEffectCount].Read == lower.Reads[0]
            && lower.OrderedEffects[parentEffectCount + 1].Read == lower.Reads[1]
            && lower.InstructionOrder[^1].BranchTargetRawFileOffset == 0xBBC8F0
            && lower.InstructionOrder[^1].BranchTaken == true
            && !lower.BranchTargetEntered
            && !lower.MemoryWriteExecuted
            && !lower.NativeInstructionsExecuted);

        NetworkIoErrorCallbackLoopGatePlan reprojection =
            Project(freshPostCallEDI: 98);
        check("Immutable captures reproject semantically without nested immutable-array reference equality",
            reprojection.Outcome == lower.Outcome
            && reprojection.Capture.FreshPostCallEBP == lower.Capture.FreshPostCallEBP
            && reprojection.Capture.FreshLocalMinus8DWORD
                == lower.Capture.FreshLocalMinus8DWORD
            && reprojection.Capture.FreshLocalMinus10DWORD
                == lower.Capture.FreshLocalMinus10DWORD
            && reprojection.Capture.FreshPostCallEDI == lower.Capture.FreshPostCallEDI
            && reprojection.CaptureStates == lower.CaptureStates
            && reprojection.Reads.SequenceEqual(lower.Reads)
            && reprojection.InstructionOrder.SequenceEqual(lower.InstructionOrder)
            && reprojection.OrderedEffects.Skip(parentEffectCount)
                .SequenceEqual(lower.OrderedEffects.Skip(parentEffectCount))
            && reprojection.ParentReturnPlan.Outcome == lower.ParentReturnPlan.Outcome
            && reprojection.ParentReturnPlan.OrderedEffects
                .Select(effect => (effect.Sequence, effect.TargetRead, effect.CallBoundary,
                    effect.OpaqueCallReturnedNormally, effect.JumpRawFileOffset,
                    effect.JumpTargetRawFileOffset))
                .SequenceEqual(lower.ParentReturnPlan.OrderedEffects
                    .Select(effect => (effect.Sequence, effect.TargetRead, effect.CallBoundary,
                        effect.OpaqueCallReturnedNormally, effect.JumpRawFileOffset,
                        effect.JumpTargetRawFileOffset))));
    }

    private static NetworkIoErrorCallbackLoopGatePlan Project(
        uint? freshPostCallEBP = 0x1000,
        uint? freshLocalMinus8DWORD = 0,
        uint? freshLocalMinus10DWORD = 0,
        uint? freshPostCallEDI = 98,
        bool? opaqueCallReturnedNormally = true,
        uint? freshParentEBP = 0x1000,
        bool? parentPassCallbackReturnedNormally = null) =>
        NetworkIoErrorCallbackLoopGateProjection.Project(new(
            new(
                new(
                    ParentPassCapture: CleanMissingKeyParent(
                        parentPassCallbackReturnedNormally),
                    FreshEBP: freshParentEBP,
                    FreshDispatchKeyDWORD: 0,
                    FreshESP: 0x2000,
                    FreshVptrDWORD: 0),
                FreshSlot20TargetDWORD: 0,
                OpaqueCallReturnedNormally: opaqueCallReturnedNormally),
            freshPostCallEBP,
            freshLocalMinus8DWORD,
            freshLocalMinus10DWORD,
            freshPostCallEDI));

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
