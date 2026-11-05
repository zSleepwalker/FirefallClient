using Firefall.Reconstruction.Startup;

internal static class GameSocketReceiveSenderPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceiveSenderPreparationProjectionResult Project(
            uint? parentResult = 1,
            uint? parentReceiver = null,
            bool? accessorReturned = null,
            uint? error = null,
            uint? receiver = 0x1000,
            uint? initial = 1,
            uint? reload = 0x20,
            bool? diagnosticReturned = null,
            uint? postDiagnosticEsp = null,
            uint? entryEsp = 0x10000) =>
            GameSocketReceiveSenderPreparationProjection.Project(new(
                new(
                    parentResult,
                    parentReceiver,
                    accessorReturned,
                    error,
                    null,
                    null,
                    null),
                receiver,
                initial,
                reload,
                diagnosticReturned,
                postDiagnosticEsp,
                entryEsp));

        var direct = Project(
            parentResult: 0x8000_0000,
            receiver: 0xFFFF_F000,
            initial: 1,
            reload: 0xFFFF_FFF8,
            entryEsp: 0xFFFF_FFF0);
        check("Any nonzero parent BOOL composes only from a clean EEB endpoint and uses a separate fresh receiver",
            direct.ParentProjection.IsCleanCompletion
            && direct.ParentProjection.StopAtRawFileOffset == 0xBD2EEB
            && direct.Stop
                == GameSocketReceiveSenderPreparationStop.SenderHelperCallBoundaryAtF19
            && direct.StopAtRawFileOffset == 0xBD2F19
            && direct.ReachedBoundedEndpoint
            && direct.ParentHandoffAdmitted
            && direct.PreparationBoundedCallReached
            && direct.ExactBoundedCompletion
            && direct.IsCleanCompletion
            && direct.InitialOperationReadAddress
                == unchecked(0xFFFF_F000u + 0xDF4u)
            && direct.FreshReloadReadAddress
                == unchecked(0xFFFF_F000u + 0xDF4u)
            && direct.LocalBufferPointer == 0x24
            && direct.SenderArgumentStackAddress == 0xFFFF_FFEC
            && direct.StackPointerAtSenderCall == 0xFFFF_FFEC
            && direct.FlagsAfterInitialCompare is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && direct.FlagsAfterSenderOffsetAdd is
                {
                    Carry: true,
                    Parity: false,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && direct.InstructionOrder[^1] is
                {
                    Kind: GameSocketReceiveSenderPreparationInstructionKind.CallSenderHelper,
                    RawFileOffset: 0xBD2F19,
                    TargetRawFileOffset: 0xBE3A00,
                    TargetAddress: 0xFE4600,
                    OperandValue: 0x10,
                    ResultValue: 0x24
                }
            && direct.OpaqueCalls.Last() is
                {
                    Kind: GameSocketReceiveSenderPreparationOpaqueCallKind.SenderHelper,
                    CallSiteReached: true,
                    Attempted: false,
                    EaxArgument: 0x24,
                    EcxArgument: 0x10
                }
            && direct.MemoryEffects
                .Where(effect =>
                    effect.Kind is GameSocketReceiveSenderPreparationMemoryEffectKind.InitialOperationRead
                        or GameSocketReceiveSenderPreparationMemoryEffectKind.FreshReloadedOperationRead)
                .All(effect => !effect.Committed)
            && direct.AnyMemoryWriteCommitted
            && !direct.PacketReceiveCallModeled
            && !direct.ReturnInstructionModeled
            && !direct.SenderHelperCallExecuted
            && !direct.OpaqueNativeCallsExecuted);

        var diagnostic = Project(
            parentResult: 2,
            receiver: 0x2000,
            initial: 0,
            reload: 0xFFFF_FFF0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0xFFFF_FFF8,
            entryEsp: 8);
        check("Zero initial operation still takes diagnostic path, cleans ESP, then independently reloads",
            diagnostic.Stop
                == GameSocketReceiveSenderPreparationStop.SenderHelperCallBoundaryAtF19
            && diagnostic.IsCleanCompletion
            && diagnostic.DiagnosticBranchTaken == true
            && diagnostic.DiagnosticCallSiteReached
            && diagnostic.OpaqueCalls[0] is
                {
                    Kind: GameSocketReceiveSenderPreparationOpaqueCallKind.DiagnosticImport,
                    CallSiteRawFileOffset: 0xBD2F02,
                    ImportCellVirtualAddress: 0x01C15EA0,
                    CallSiteReached: true,
                    Attempted: true,
                    ReturnedNormally: true
                }
            && diagnostic.MemoryEffects
                .Where(effect =>
                    effect.Kind == GameSocketReceiveSenderPreparationMemoryEffectKind.DiagnosticStackWrite)
                .Select(effect => (effect.RawFileOffset, effect.Address, effect.Value))
                .SequenceEqual(new[]
                {
                    (0xBD2EF4u, (uint?)4, (uint?)0),
                    (0xBD2EF6u, (uint?)0, (uint?)0x5B),
                    (0xBD2EF8u, (uint?)0xFFFF_FFFCu, (uint?)0x01C5_EA58),
                    (0xBD2EFDu, (uint?)0xFFFF_FFF8, (uint?)0x01C5_EA58)
                })
            && diagnostic.FlagsAfterDiagnosticStackCleanup is
                {
                    Carry: true,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && diagnostic.InstructionOrder.Single(instruction =>
                instruction.Kind == GameSocketReceiveSenderPreparationInstructionKind.AddEsp10)
                is
                {
                    RawFileOffset: 0xBD2F08,
                    EffectiveAddress: 0xFFFF_FFF8,
                    OperandValue: 0x10,
                    ResultValue: 8
                }
            && diagnostic.LocalBufferPointer == 0x3C
            && diagnostic.SenderArgumentStackAddress == 4
            && diagnostic.FlagsAfterSenderOffsetAdd is
                {
                    Carry: true,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && diagnostic.ParentProjection.Inputs.ReceiverEsi is null
            && diagnostic.CaptureStates.ReceiverEsi
                == GameSocketReceiveSenderPreparationCaptureState.Captured);

        var diagnosticPending = Project(
            initial: 0,
            diagnosticReturned: null,
            postDiagnosticEsp: 0x3000);
        var diagnosticDidNotReturn = Project(
            initial: 0,
            diagnosticReturned: false,
            postDiagnosticEsp: 0x3000);
        check("Missing and nonnormal diagnostic returns stop at F02 without cleanup or the sender reload",
            diagnosticPending.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingDiagnosticReturnAtF02
            && diagnosticPending.StopAtRawFileOffset == 0xBD2F02
            && diagnosticPending.OpaqueCalls.Single().Attempted
            && diagnosticPending.FlagsAtStop == diagnosticPending.FlagsAfterInitialCompare
            && !diagnosticPending.FreshReloadReadAttempted
            && diagnosticPending.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && diagnosticDidNotReturn.Stop
                == GameSocketReceiveSenderPreparationStop.DiagnosticDidNotReturnNormally
            && diagnosticDidNotReturn.StopAtRawFileOffset == 0xBD2F02
            && !diagnosticDidNotReturn.InstructionOrder.Any(instruction =>
                instruction.Kind
                    == GameSocketReceiveSenderPreparationInstructionKind.AddEsp10)
            && !diagnosticDidNotReturn.FreshReloadReadAttempted);

        var missingPostDiagnosticEsp = Project(
            initial: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: null,
            reload: 0x80);
        check("A normal diagnostic return invalidates prior flags and requires fresh post-call ESP before ADD",
            missingPostDiagnosticEsp.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingPostDiagnosticEspAtF08
            && missingPostDiagnosticEsp.StopAtRawFileOffset == 0xBD2F08
            && missingPostDiagnosticEsp.FlagsAtStop is null
            && missingPostDiagnosticEsp.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveSenderPreparationCaptureState.Missing
            && !missingPostDiagnosticEsp.FreshReloadReadAttempted
            && missingPostDiagnosticEsp.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused);

        var missingReload = Project(
            receiver: 0xFFFF_FFF0,
            initial: 1,
            reload: null);
        check("The second operation read is attempted independently and missing reload preserves CMP flags",
            missingReload.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingFreshReloadAtF0B
            && missingReload.FreshReloadReadAttempted
            && missingReload.FreshReloadReadAddress == 0xDE4
            && missingReload.MemoryEffects.Last() is
                {
                    Kind: GameSocketReceiveSenderPreparationMemoryEffectKind.FreshReloadedOperationRead,
                    RawFileOffset: 0xBD2F0B,
                    Address: 0xDE4,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }
            && missingReload.FlagsAtStop == missingReload.FlagsAfterInitialCompare
            && missingReload.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveSenderPreparationCaptureState.Missing
            && !missingReload.SenderHelperCallSiteReached);

        var freshZeroDoesNotChangeBranch = Project(
            initial: 7,
            reload: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0x7777);
        check("The fresh reload controls ECX but cannot retroactively select the initial zero-operation branch",
            freshZeroDoesNotChangeBranch.DiagnosticBranchTaken == false
            && !freshZeroDoesNotChangeBranch.DiagnosticCallSiteReached
            && freshZeroDoesNotChangeBranch.FlagsAfterSenderOffsetAdd is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && freshZeroDoesNotChangeBranch.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && freshZeroDoesNotChangeBranch.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && !freshZeroDoesNotChangeBranch.IsCleanCompletion);

        var missingReceiver = Project(receiver: null, initial: 1);
        var missingInitialOperation = Project(initial: null);
        check("Missing ESI performs no child read, while a missing initial dword attempts its read and preserves parent TEST flags",
            missingReceiver.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingReceiverEsiAtEEB
            && missingReceiver.StopAtRawFileOffset == 0xBD2EEB
            && !missingReceiver.InitialOperationReadAttempted
            && missingReceiver.MemoryEffects.IsEmpty
            && missingReceiver.FlagsAtStop
                == missingReceiver.ParentProjection.FlagsAfterResultTest
            && missingReceiver.CaptureStates.InitialOperation
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && missingInitialOperation.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingInitialOperationAtEEB
            && missingInitialOperation.InitialOperationReadAttempted
            && missingInitialOperation.InitialOperationReadAddress == 0x1DF4
            && missingInitialOperation.FlagsAtStop
                == missingInitialOperation.ParentProjection.FlagsAfterResultTest
            && missingInitialOperation.CaptureStates.InitialOperation
                == GameSocketReceiveSenderPreparationCaptureState.Missing);

        var missingEntryEsp = Project(entryEsp: null, initial: 1, reload: 4);
        check("Direct path reaches F11 only after fresh reload and stops there without entry ESP",
            missingEntryEsp.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtF11
            && missingEntryEsp.StopAtRawFileOffset == 0xBD2F11
            && missingEntryEsp.FreshReloadReadAttempted
            && missingEntryEsp.CaptureStates.EntryEsp
                == GameSocketReceiveSenderPreparationCaptureState.Missing
            && missingEntryEsp.InstructionOrder[^1] is
                {
                    Kind: GameSocketReceiveSenderPreparationInstructionKind.LeaLocalBuffer,
                    RawFileOffset: 0xBD2F11,
                    EffectiveAddress: null,
                    OperandValue: 0x34,
                    ResultValue: null
                }
            && !missingEntryEsp.LocalBufferPointer.HasValue
            && !missingEntryEsp.SenderHelperCallSiteReached);

        var missingDiagnosticEntryEsp = Project(
            initial: 0,
            entryEsp: null,
            diagnosticReturned: true,
            postDiagnosticEsp: 0x5000,
            reload: 7);
        check("Zero-operation route requires entry ESP before the first diagnostic PUSH",
            missingDiagnosticEntryEsp.Stop
                == GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtDiagnosticPush
            && missingDiagnosticEntryEsp.StopAtRawFileOffset == 0xBD2EF4
            && missingDiagnosticEntryEsp.CaptureStates.EntryEsp
                == GameSocketReceiveSenderPreparationCaptureState.Missing
            && missingDiagnosticEntryEsp.InstructionOrder.All(instruction =>
                instruction.RawFileOffset < 0xBD2EF4)
            && missingDiagnosticEntryEsp.MemoryEffects.All(effect =>
                effect.Kind != GameSocketReceiveSenderPreparationMemoryEffectKind.DiagnosticStackWrite)
            && missingDiagnosticEntryEsp.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && missingDiagnosticEntryEsp.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && missingDiagnosticEntryEsp.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && !missingDiagnosticEntryEsp.IsCleanCompletion);

        var parentError996 = Project(
            parentResult: 0,
            accessorReturned: true,
            error: 996,
            receiver: 0x9999,
            initial: 0x10);
        var dirtyParentError996 = Project(
            parentResult: 0,
            parentReceiver: 0x1234,
            accessorReturned: true,
            error: 996,
            receiver: 0x9999,
            initial: 0x10);
        var dirtyParentSuccess = Project(
            parentResult: 2,
            parentReceiver: 0x1234,
            receiver: 0x9999,
            initial: 0x10);
        var missingParentResult = Project(
            parentResult: null,
            receiver: 0x9999,
            initial: 0x10);
        check("Error, dirty-success, and missing parent routes retain their own bounds and never start child reads",
            parentError996.Stop
                == GameSocketReceiveSenderPreparationStop.ParentDidNotReachNonzeroEndpoint
            && parentError996.StopAtRawFileOffset == 0xBD3001
            && parentError996.FlagsAtStop == parentError996.ParentProjection.FlagsAtStop
            && parentError996.MemoryEffects.IsEmpty
            && !parentError996.ParentHandoffAdmitted
            && !parentError996.PreparationBoundedCallReached
            && parentError996.ExactBoundedCompletion
            && !parentError996.IsCleanCompletion
            && parentError996.CaptureStates.ReceiverEsi
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && dirtyParentError996.Stop
                == GameSocketReceiveSenderPreparationStop.ParentDidNotReachNonzeroEndpoint
            && dirtyParentError996.StopAtRawFileOffset == 0xBD3001
            && dirtyParentError996.FlagsAtStop
                == dirtyParentError996.ParentProjection.FlagsAtStop
            && dirtyParentError996.ParentProjection.CaptureStates.ReceiverEsi
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused
            && dirtyParentError996.ExactBoundedCompletion
            && dirtyParentError996.MemoryEffects.IsEmpty
            && dirtyParentSuccess.Stop
                == GameSocketReceiveSenderPreparationStop.ParentNonzeroEndpointWasDirty
            && dirtyParentSuccess.StopAtRawFileOffset == 0xBD2EEB
            && dirtyParentSuccess.ParentProjection.CaptureStates.ReceiverEsi
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused
            && dirtyParentSuccess.CaptureStates.ReceiverEsi
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && dirtyParentSuccess.ExactBoundedCompletion
            && !dirtyParentSuccess.ParentHandoffAdmitted
            && !dirtyParentSuccess.PreparationBoundedCallReached
            && dirtyParentSuccess.MemoryEffects.IsEmpty
            && !dirtyParentSuccess.InitialOperationReadAttempted
            && missingParentResult.Stop
                == GameSocketReceiveSenderPreparationStop.ParentDidNotReachNonzeroEndpoint
            && missingParentResult.StopAtRawFileOffset == 0xBD2ED7
            && missingParentResult.FlagsAtStop
                == missingParentResult.ParentProjection.FlagsAtStop
            && !missingParentResult.ExactBoundedCompletion
            && missingParentResult.MemoryEffects.IsEmpty);

        var dirtyBoundedCall = Project(
            initial: 5,
            reload: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0x4444);
        check("Unreached supplied diagnostic captures dirty an otherwise bounded F19 projection",
            dirtyBoundedCall.ReachedBoundedEndpoint
            && dirtyBoundedCall.ParentHandoffAdmitted
            && dirtyBoundedCall.PreparationBoundedCallReached
            && dirtyBoundedCall.ExactBoundedCompletion
            && dirtyBoundedCall.StopAtRawFileOffset == 0xBD2F19
            && dirtyBoundedCall.HasSuppliedButUnusedInputs
            && !dirtyBoundedCall.IsCleanCompletion
            && dirtyBoundedCall.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && dirtyBoundedCall.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused
            && dirtyBoundedCall.OpaqueCalls.Last().Attempted == false);

        var signedOverflow = Project(reload: 0x7FFF_FFF8);
        check("ADD ECX,18 projects signed overflow, sign, auxiliary carry, and parity independently",
            signedOverflow.FlagsAfterSenderOffsetAdd is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            });

        check("Instruction and import anchors retain the verified raw targets and stop before packet or return code",
            GameSocketReceiveSenderPreparationProjection.InitialOperationCompareRawFileOffset
                == 0xBD2EEB
            && GameSocketReceiveSenderPreparationProjection.InitialOperationBranchRawFileOffset
                == 0xBD2EF2
            && GameSocketReceiveSenderPreparationProjection.DiagnosticCallRawFileOffset
                == 0xBD2F02
            && GameSocketReceiveSenderPreparationProjection.DiagnosticImportCellVirtualAddress
                == 0x01C15EA0
            && GameSocketReceiveSenderPreparationProjection.AddEspRawFileOffset == 0xBD2F08
            && GameSocketReceiveSenderPreparationProjection.FreshReloadRawFileOffset == 0xBD2F0B
            && GameSocketReceiveSenderPreparationProjection.LocalBufferLeaRawFileOffset
                == 0xBD2F11
            && GameSocketReceiveSenderPreparationProjection.AddEcxRawFileOffset == 0xBD2F16
            && GameSocketReceiveSenderPreparationProjection.SenderHelperCallRawFileOffset
                == 0xBD2F19
            && GameSocketReceiveSenderPreparationProjection.BoundedSenderHelperTargetRawFileOffset
                == 0xBE3A00
            && GameSocketReceiveSenderPreparationProjection.BoundedSenderHelperTargetVirtualAddress
                == 0xFE4600
            && !direct.PacketReceiveCallModeled
            && !direct.ReturnInstructionModeled);
    }
}
