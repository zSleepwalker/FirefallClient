using Firefall.Reconstruction.Startup;

internal static class GameSocketReceivePostPacketStackPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceivePostPacketStackPreparationProjectionResult Project(
            uint? parentResult = 1,
            uint? parentReceiver = null,
            bool? accessorReturned = null,
            uint? error = null,
            uint? receiver = 0x1000,
            uint? initial = 1,
            uint? reload = 0x20,
            bool? diagnosticReturned = null,
            uint? postDiagnosticEsp = null,
            uint? entryEsp = 0x10000,
            bool? helperReturned = true,
            uint? helperEax = 0x1122_3344,
            uint? postHelperEsp = 0x2000,
            uint? freshCount = 0x5566_7788,
            bool? packetReturned = true,
            uint? postPacketEsi = 0x3000,
            uint? postPacketEsp = 0x4000,
            uint? operation = 0) =>
            GameSocketReceivePostPacketStackPreparationProjection.Project(new(
                new(
                    new(
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
                        entryEsp),
                    helperReturned,
                    helperEax,
                    postHelperEsp,
                    freshCount),
                packetReturned,
                postPacketEsi,
                postPacketEsp,
                operation));

        var zero = Project(
            receiver: 0xABCD_1234,
            postPacketEsi: 0x7000,
            postPacketEsp: 0x5000,
            operation: 0);
        check("Normal zero operation uses fresh post-packet registers, commits three locals and projects four pushes before the opaque import",
            zero.ParentProjection.IsCleanCompletion
            && zero.ParentProjection.StopAtRawFileOffset == 0xBD2F25
            && zero.ParentHandoffAdmitted
            && zero.Stop
                == GameSocketReceivePostPacketStackPreparationStop.DiagnosticImportBoundaryAtF5B
            && zero.StopAtRawFileOffset == 0xBD2F5B
            && zero.ReachedBoundedEndpoint
            && zero.IsCleanCompletion
            && zero.OperationAddress == 0x7DF4
            && zero.LocalPointer == 0x7278
            && zero.ZeroStackLocalAddress == 0x501C
            && zero.LocalPointerStackAddress == 0x5028
            && zero.SizeStackLocalAddress == 0x5024
            && zero.StackPointerAtDiagnosticCall == 0x4FF0
            && zero.NonzeroBranchTaken == false
            && zero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.MemoryEffects.Select(effect => effect.Kind).SequenceEqual(new[]
            {
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.FreshOperationRead,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.ZeroStackLocalWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.LocalPointerStackWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.SizeStackLocalWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushZeroStackWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushTokenStackWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushDiagnosticArgumentStackWrite,
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushDiagnosticArgumentStackWrite
            })
            && zero.MemoryEffects[0] is
            {
                Address: 0x7DF4,
                Value: 0,
                Attempted: true,
                Captured: true,
                Committed: false
            }
            && zero.MemoryEffects[1] is
            {
                Address: 0x501C,
                Value: 0,
                Committed: true
            }
            && zero.MemoryEffects[2] is
            {
                Address: 0x5028,
                Value: 0x7278,
                Committed: true
            }
            && zero.MemoryEffects[3] is
            {
                Address: 0x5024,
                Value: 0x578,
                Committed: true
            }
            && zero.MemoryEffects.Skip(4).Select(effect => (effect.Address, effect.Value))
                .SequenceEqual(new (uint?, uint?)[]
                {
                    (0x4FFC, 0),
                    (0x4FF8, 0x5B),
                    (0x4FF4, 0x1C5EA58),
                    (0x4FF0, 0x1C5EA58)
                })
            && zero.InstructionOrder.Select(instruction => instruction.Kind).SequenceEqual(new[]
            {
                GameSocketReceivePostPacketStackPreparationInstructionKind.CompareFreshOperationToZero,
                GameSocketReceivePostPacketStackPreparationInstructionKind.LeaLocalPointer,
                GameSocketReceivePostPacketStackPreparationInstructionKind.StoreZeroAtEsp1C,
                GameSocketReceivePostPacketStackPreparationInstructionKind.StoreLocalPointerAtEsp28,
                GameSocketReceivePostPacketStackPreparationInstructionKind.Store578AtEsp24,
                GameSocketReceivePostPacketStackPreparationInstructionKind.BranchIfOperationNonzero,
                GameSocketReceivePostPacketStackPreparationInstructionKind.PushZero,
                GameSocketReceivePostPacketStackPreparationInstructionKind.PushToken5B,
                GameSocketReceivePostPacketStackPreparationInstructionKind.PushDiagnosticArgument,
                GameSocketReceivePostPacketStackPreparationInstructionKind.PushDiagnosticArgument,
                GameSocketReceivePostPacketStackPreparationInstructionKind.CallDiagnosticImport
            })
            && zero.Calls.Single(call =>
                call.Kind == GameSocketReceivePostPacketStackPreparationCallKind.PacketCall) is
            {
                CallSiteRawFileOffset: 0xBD2F25,
                TargetRawFileOffset: 0xBD3030,
                TargetAddress: 0xFD3C30,
                Attempted: true,
                ReturnedNormally: true,
                EcxArgument: 0xABCD1234
            }
            && zero.Calls.Last() is
            {
                Kind: GameSocketReceivePostPacketStackPreparationCallKind.DiagnosticImportBoundary,
                CallSiteRawFileOffset: 0xBD2F5B,
                ImportCellVirtualAddress: 0x1C15EA0,
                TargetRawFileOffset: null,
                TargetAddress: null,
                TargetCaptured: false,
                CallSiteReached: true,
                Attempted: false,
                StackArguments: var args
            }
            && args.SequenceEqual(new uint?[] { 0x1C5EA58, 0x1C5EA58, 0x5B, 0 })
            && zero.CaptureStates.FreshPostPacketEsi
                == GameSocketReceivePostPacketStackPreparationCaptureState.Captured
            && zero.CaptureStates.FreshPostPacketEsp
                == GameSocketReceivePostPacketStackPreparationCaptureState.Captured
            && zero.CaptureStates.FreshOperation
                == GameSocketReceivePostPacketStackPreparationCaptureState.Captured
            && zero.AnyMemoryWriteCommitted
            && !zero.PacketReceiveCallExecuted
            && !zero.DiagnosticImportExecuted
            && !zero.PacketRepostModeled
            && !zero.ReturnInstructionModeled);

        var highBitOperation = Project(
            receiver: 0x1111,
            postPacketEsi: 0x9000,
            postPacketEsp: 0xFFFF_FFF0,
            operation: 0x8000_0000);
        check("Nonzero high-bit operation branches to the unentered F64 target with wrapped fresh ESP stores and signed flags",
            highBitOperation.Stop
                == GameSocketReceivePostPacketStackPreparationStop.NonzeroBranchTargetAtF64
            && highBitOperation.StopAtRawFileOffset == 0xBD2F64
            && highBitOperation.ReachedBoundedEndpoint
            && highBitOperation.IsCleanCompletion
            && highBitOperation.NonzeroBranchTaken == true
            && highBitOperation.OperationAddress == 0x9DF4
            && highBitOperation.LocalPointer == 0x9278
            && highBitOperation.ZeroStackLocalAddress == 0xC
            && highBitOperation.LocalPointerStackAddress == 0x18
            && highBitOperation.SizeStackLocalAddress == 0x14
            && highBitOperation.FlagsAtStop is
            {
                Carry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBitOperation.MemoryEffects.Length == 4
            && !highBitOperation.DiagnosticCallSiteReached
            && highBitOperation.InstructionOrder.All(instruction =>
                instruction.Kind
                    is not GameSocketReceivePostPacketStackPreparationInstructionKind.PushZero
                    and not GameSocketReceivePostPacketStackPreparationInstructionKind.PushToken5B
                    and not GameSocketReceivePostPacketStackPreparationInstructionKind.PushDiagnosticArgument)
            && highBitOperation.ParentProjection.Inputs.ParentInputs.ReceiverEsi == 0x1111
            && highBitOperation.Inputs.FreshPostPacketEsi == 0x9000);

        var missingReturn = Project(packetReturned: null);
        var nonnormalReturn = Project(packetReturned: false);
        check("Missing or nonnormal opaque packet return stops at F25, preserves parent flags and never enters F2A",
            missingReturn.Stop
                == GameSocketReceivePostPacketStackPreparationStop.AwaitingOpaquePacketReturnAtF25
            && missingReturn.StopAtRawFileOffset == 0xBD2F25
            && missingReturn.FlagsAtStop == missingReturn.ParentProjection.FlagsAtStop
            && missingReturn.CaptureStates.OpaquePacketReturnedNormally
                == GameSocketReceivePostPacketStackPreparationCaptureState.Missing
            && missingReturn.CaptureStates.FreshPostPacketEsi
                == GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused
            && missingReturn.MemoryEffects.IsEmpty
            && missingReturn.InstructionOrder.IsEmpty
            && missingReturn.Calls.Single(call =>
                call.Kind == GameSocketReceivePostPacketStackPreparationCallKind.PacketCall) is
            {
                Attempted: true,
                ReturnedNormally: null
            }
            && nonnormalReturn.Stop
                == GameSocketReceivePostPacketStackPreparationStop.OpaquePacketDidNotReturnNormally
            && nonnormalReturn.StopAtRawFileOffset == 0xBD2F25
            && nonnormalReturn.FlagsAtStop == nonnormalReturn.ParentProjection.FlagsAtStop
            && nonnormalReturn.CaptureStates.OpaquePacketReturnedNormally
                == GameSocketReceivePostPacketStackPreparationCaptureState.Captured
            && nonnormalReturn.MemoryEffects.IsEmpty
            && !nonnormalReturn.OperationReadAttempted);

        var missingEsi = Project(postPacketEsi: null);
        var missingOperation = Project(operation: null);
        check("Fresh receiver and operation are independent captures: missing ESI creates no synthetic CMP address, while a missing operation is an attempted read",
            missingEsi.Stop
                == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEsiAtF2A
            && missingEsi.StopAtRawFileOffset == 0xBD2F2A
            && missingEsi.OperationAddress is null
            && !missingEsi.OperationReadAttempted
            && missingEsi.MemoryEffects.IsEmpty
            && missingEsi.ParentProjection.Inputs.ParentInputs.ReceiverEsi == 0x1000
            && missingEsi.CaptureStates.FreshOperation
                == GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused
            && missingOperation.Stop
                == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshOperationAtF2A
            && missingOperation.StopAtRawFileOffset == 0xBD2F2A
            && missingOperation.OperationAddress == 0x3DF4
            && missingOperation.OperationReadAttempted
            && missingOperation.MemoryEffects.Single() is
            {
                Kind: GameSocketReceivePostPacketStackPreparationMemoryEffectKind.FreshOperationRead,
                Address: 0x3DF4,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingOperation.CaptureStates.FreshOperation
                == GameSocketReceivePostPacketStackPreparationCaptureState.Missing
            && missingOperation.CaptureStates.FreshPostPacketEsp
                == GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused
            && missingOperation.FlagsAtStop is null);

        var missingPostPacketEsp = Project(postPacketEsp: null, operation: 0);
        check("Missing fresh post-packet ESP stops at the first F37 store after a derived LEA and preserves CMP flags",
            missingPostPacketEsp.Stop
                == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEspAtF37
            && missingPostPacketEsp.StopAtRawFileOffset == 0xBD2F37
            && missingPostPacketEsp.LocalPointer == 0x3278
            && missingPostPacketEsp.ZeroStackLocalAddress is null
            && missingPostPacketEsp.FlagsAtStop?.Zero == true
            && missingPostPacketEsp.MemoryEffects.Length == 2
            && missingPostPacketEsp.MemoryEffects[0].Kind
                == GameSocketReceivePostPacketStackPreparationMemoryEffectKind.FreshOperationRead
            && missingPostPacketEsp.MemoryEffects[1] is
            {
                Kind: GameSocketReceivePostPacketStackPreparationMemoryEffectKind.ZeroStackLocalWrite,
                Address: null,
                Value: 0,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingPostPacketEsp.CaptureStates.FreshPostPacketEsp
                == GameSocketReceivePostPacketStackPreparationCaptureState.Missing
            && !missingPostPacketEsp.MemoryEffects.Any(effect => effect.Committed));

        var dirtyParentBoundary = Project(
            parentResult: 1,
            accessorReturned: true,
            error: 996);
        var parent996 = Project(
            parentResult: 0,
            accessorReturned: true,
            error: 996);
        check("Dirty and incomplete parent traces preserve their exact nested bounds without admitting child captures",
            dirtyParentBoundary.ParentProjection.Stop
                == GameSocketReceivePacketCallPreparationStop.ParentDidNotReachSenderHelper
            && !dirtyParentBoundary.ParentProjection.IsCleanCompletion
            && dirtyParentBoundary.Stop
                == GameSocketReceivePostPacketStackPreparationStop.ParentDidNotReachPacketCall
            && dirtyParentBoundary.StopAtRawFileOffset == 0xBD2EEB
            && dirtyParentBoundary.ExactBoundedCompletion
            && !dirtyParentBoundary.ParentHandoffAdmitted
            && dirtyParentBoundary.MemoryEffects.IsEmpty
            && dirtyParentBoundary.CaptureStates.OpaquePacketReturnedNormally
                == GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused
            && parent996.Stop
                == GameSocketReceivePostPacketStackPreparationStop.ParentDidNotReachPacketCall
            && parent996.StopAtRawFileOffset == 0xBD3001
            && parent996.ExactBoundedCompletion
            && parent996.FlagsAtStop == parent996.ParentProjection.FlagsAtStop
            && !parent996.ParentHandoffAdmitted
            && parent996.MemoryEffects.IsEmpty
            && parent996.CaptureStates.FreshOperation
                == GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused);

        check("The ordered trace appends post-packet effects after parent effects and matches the supplied raw window",
            zero.OrderedMemoryEffects.Take(zero.ParentProjection.OrderedMemoryEffects.Length)
                .Select(step => step.ParentMemoryEffect)
                .SequenceEqual(zero.ParentProjection.OrderedMemoryEffects.Select(step =>
                    (GameSocketReceivePacketCallPreparationTraceStep?)step))
            && zero.OrderedMemoryEffects.TakeLast(zero.MemoryEffects.Length)
                .Select(step => step.PostPacketMemoryEffect)
                .SequenceEqual(zero.MemoryEffects.Select(effect =>
                    (GameSocketReceivePostPacketStackPreparationMemoryEffect?)effect))
            && GameSocketReceivePostPacketStackPreparationProjection.MethodRawFileOffset == 0xBD2F2A
            && GameSocketReceivePostPacketStackPreparationProjection.PacketReturnBoundaryRawFileOffset
                == 0xBD2F25
            && GameSocketReceivePostPacketStackPreparationProjection.OperationCompareRawFileOffset
                == 0xBD2F2A
            && GameSocketReceivePostPacketStackPreparationProjection.ZeroStackLocalStoreRawFileOffset
                == 0xBD2F37
            && GameSocketReceivePostPacketStackPreparationProjection.OperationBranchRawFileOffset
                == 0xBD2F4B
            && GameSocketReceivePostPacketStackPreparationProjection.NonzeroBranchTargetRawFileOffset
                == 0xBD2F64
            && GameSocketReceivePostPacketStackPreparationProjection.DiagnosticImportCallRawFileOffset
                == 0xBD2F5B
            && GameSocketReceivePostPacketStackPreparationProjection.DiagnosticImportCellVirtualAddress
                == 0x1C15EA0
            && GameSocketReceivePostPacketStackPreparationProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0x83, 0xBE, 0xF4, 0x0D, 0x00, 0x00, 0x00,
                    0x8D, 0x86, 0x78, 0x02, 0x00, 0x00,
                    0xC7, 0x44, 0x24, 0x1C, 0x00, 0x00, 0x00, 0x00,
                    0x89, 0x44, 0x24, 0x28,
                    0xC7, 0x44, 0x24, 0x24, 0x78, 0x05, 0x00, 0x00,
                    0x75, 0x17, 0x6A, 0x00, 0x6A, 0x5B,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
                    0x83, 0xC4, 0x10
                }));
    }
}
