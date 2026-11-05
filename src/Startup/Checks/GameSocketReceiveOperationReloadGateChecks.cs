using Firefall.Reconstruction.Startup;

internal static class GameSocketReceiveOperationReloadGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceiveOperationReloadGateProjectionResult Project(
            uint? parentResult = 1,
            bool? accessorReturned = null,
            uint? error = null,
            uint? postPacketEsi = 0x1000,
            uint? postPacketEsp = 0x2000,
            uint? parentOperation = 1,
            bool? diagnosticReturned = null,
            uint? postDiagnosticEsp = null,
            uint? postDiagnosticEsi = null,
            uint? reloadedOperation = 0)
        {
            var parentInputs = new GameSocketReceivePostPacketStackPreparationProjectionInputs(
                new(
                    new(
                        new(
                            parentResult,
                            null,
                            accessorReturned,
                            error,
                            null,
                            null,
                            null),
                        0x1234,
                        1,
                        0x20,
                        null,
                        null,
                        0x10000),
                    true,
                    0x1122_3344,
                    0x2000,
                    0x5566_7788),
                true,
                postPacketEsi,
                postPacketEsp,
                parentOperation);
            return GameSocketReceiveOperationReloadGateProjection.Project(new(
                parentInputs,
                diagnosticReturned,
                postDiagnosticEsp,
                postDiagnosticEsi,
                reloadedOperation));
        }

        var directReloadsZero = Project(
            postPacketEsi: 0xFFFF_FFF0,
            postPacketEsp: 0xFFFF_FFF8,
            parentOperation: 0x8000_0000,
            reloadedOperation: 0);
        check("The direct F64 route reuses parent registers and the fresh reload independently selects zero",
            directReloadsZero.ParentHandoffAdmitted
            && directReloadsZero.Route
                == GameSocketReceiveOperationReloadGateRoute.DirectNonzero
            && directReloadsZero.Stop
                == GameSocketReceiveOperationReloadGateStop.DiagnosticImportBoundaryAtF7F
            && directReloadsZero.StopAtRawFileOffset == 0xBD2F7F
            && directReloadsZero.ReachedBoundedEndpoint
            && directReloadsZero.IsCleanCompletion
            && directReloadsZero.ParentProjection.StopAtRawFileOffset == 0xBD2F64
            && directReloadsZero.OperationAddress == 0xDE4
            && directReloadsZero.OperationStackAddress == 0x10
            && directReloadsZero.StackPointerAtDiagnosticCall == 0xFFFF_FFE8
            && directReloadsZero.NonzeroBranchTaken == false
            && directReloadsZero.CaptureStates.PostPacketDiagnosticReturnedNormally
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && directReloadsZero.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && directReloadsZero.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && directReloadsZero.MemoryEffects[0] is
            {
                Kind: GameSocketReceiveOperationReloadGateMemoryEffectKind
                    .FreshReloadedOperationRead,
                RawFileOffset: 0xBD2F64,
                Address: 0xDE4,
                Value: 0,
                Attempted: true,
                Captured: true,
                Committed: false
            }
            && directReloadsZero.MemoryEffects[1] is
            {
                Kind: GameSocketReceiveOperationReloadGateMemoryEffectKind.OperationStackWrite,
                Address: 0x10,
                Value: 0,
                Committed: true
            }
            && directReloadsZero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            });

        var zeroParentReloadsNonzero = Project(
            postPacketEsi: 0x3000,
            postPacketEsp: 0x4000,
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0xFFFF_FFF8,
            postDiagnosticEsi: 0xFFFF_FFF0,
            reloadedOperation: 0x8000_0000);
        check("The zero F5B route requires normal return and fresh post-diagnostic registers before the F64 reload",
            zeroParentReloadsNonzero.ParentHandoffAdmitted
            && zeroParentReloadsNonzero.Route
                == GameSocketReceiveOperationReloadGateRoute.ZeroDiagnostic
            && zeroParentReloadsNonzero.ParentProjection.StopAtRawFileOffset == 0xBD2F5B
            && zeroParentReloadsNonzero.Stop
                == GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88
            && zeroParentReloadsNonzero.StopAtRawFileOffset == 0xBD2F88
            && zeroParentReloadsNonzero.ReachedBoundedEndpoint
            && zeroParentReloadsNonzero.IsCleanCompletion
            && zeroParentReloadsNonzero.PostDiagnosticEspAfterCleanup == 8
            && zeroParentReloadsNonzero.OperationAddress == 0xDE4
            && zeroParentReloadsNonzero.OperationStackAddress == 0x20
            && zeroParentReloadsNonzero.NonzeroBranchTaken == true
            && zeroParentReloadsNonzero.CaptureStates.PostPacketDiagnosticReturnedNormally
                == GameSocketReceiveOperationReloadGateCaptureState.Captured
            && zeroParentReloadsNonzero.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveOperationReloadGateCaptureState.Captured
            && zeroParentReloadsNonzero.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.Captured
            && zeroParentReloadsNonzero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && !zeroParentReloadsNonzero.DiagnosticCallSiteReached
            && !zeroParentReloadsNonzero.BranchTargetF88Entered);

        var missingDiagnosticReturn = Project(
            parentOperation: 0,
            postDiagnosticEsp: 0x5000,
            postDiagnosticEsi: 0x6000);
        var nonnormalDiagnosticReturn = Project(
            parentOperation: 0,
            diagnosticReturned: false,
            postDiagnosticEsp: 0x5000,
            postDiagnosticEsi: 0x6000);
        check("A missing or nonnormal F5B diagnostic return stops before cleanup and leaves later captures unused",
            missingDiagnosticReturn.Stop
                == GameSocketReceiveOperationReloadGateStop
                    .AwaitingPostPacketDiagnosticReturnAtF5B
            && missingDiagnosticReturn.StopAtRawFileOffset == 0xBD2F5B
            && missingDiagnosticReturn.Calls.Single() is
            {
                CallSiteReached: true,
                Attempted: true,
                ReturnedNormally: null
            }
            && missingDiagnosticReturn.FlagsAtStop
                == missingDiagnosticReturn.ParentProjection.FlagsAtStop
            && missingDiagnosticReturn.CaptureStates.PostPacketDiagnosticReturnedNormally
                == GameSocketReceiveOperationReloadGateCaptureState.Missing
            && missingDiagnosticReturn.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused
            && missingDiagnosticReturn.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused
            && missingDiagnosticReturn.MemoryEffects.IsEmpty
            && nonnormalDiagnosticReturn.Stop
                == GameSocketReceiveOperationReloadGateStop
                    .PostPacketDiagnosticDidNotReturnNormally
            && nonnormalDiagnosticReturn.StopAtRawFileOffset == 0xBD2F5B
            && nonnormalDiagnosticReturn.Calls.Single().ReturnedNormally == false
            && nonnormalDiagnosticReturn.FlagsAtStop
                == nonnormalDiagnosticReturn.ParentProjection.FlagsAtStop
            && nonnormalDiagnosticReturn.InstructionOrder.IsEmpty
            && nonnormalDiagnosticReturn.MemoryEffects.IsEmpty);

        var missingPostDiagnosticEsp = Project(
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsi: 0x7000);
        var missingPostDiagnosticEsi = Project(
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0x8000);
        check("The zero route requires post-diagnostic ESP before ADD and independently requires ESI at F64",
            missingPostDiagnosticEsp.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEspAtF61
            && missingPostDiagnosticEsp.StopAtRawFileOffset == 0xBD2F61
            && missingPostDiagnosticEsp.InstructionOrder[^1] is
            {
                Kind: GameSocketReceiveOperationReloadGateInstructionKind
                    .Add10ToPostDiagnosticEsp,
                EffectiveAddress: null,
                OperandValue: 0x10
            }
            && missingPostDiagnosticEsp.PostDiagnosticEspAfterCleanup is null
            && missingPostDiagnosticEsp.FlagsAtStop is null
            && missingPostDiagnosticEsp.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveOperationReloadGateCaptureState.Missing
            && missingPostDiagnosticEsp.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused
            && missingPostDiagnosticEsi.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEsiAtF64
            && missingPostDiagnosticEsi.StopAtRawFileOffset == 0xBD2F64
            && missingPostDiagnosticEsi.PostDiagnosticEspAfterCleanup == 0x8010
            && missingPostDiagnosticEsi.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false,
                  Sign: false, Overflow: false }
            && missingPostDiagnosticEsi.InstructionOrder
                .Select(instruction => instruction.Kind)
                .SequenceEqual(new[]
                {
                    GameSocketReceiveOperationReloadGateInstructionKind.Add10ToPostDiagnosticEsp
                })
            && missingPostDiagnosticEsi.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.Missing
            && missingPostDiagnosticEsi.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused);

        var signedAddBoundary = Project(
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0x7FFF_FFF8);
        var wrappedAddBoundary = Project(
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0xFFFF_FFFF);
        check("ADD ESP,10 models signed overflow and unsigned wrap with all six flags known before the F64 ESI capture",
            signedAddBoundary.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEsiAtF64
            && signedAddBoundary.PostDiagnosticEspAfterCleanup == 0x8000_0008
            && signedAddBoundary.FlagsAtStop is
                { Carry: false, Parity: false, AuxiliaryCarry: false, Zero: false,
                  Sign: true, Overflow: true }
            && signedAddBoundary.InstructionOrder.Length == 1
            && signedAddBoundary.InstructionOrder[0].Kind
                == GameSocketReceiveOperationReloadGateInstructionKind.Add10ToPostDiagnosticEsp
            && wrappedAddBoundary.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEsiAtF64
            && wrappedAddBoundary.PostDiagnosticEspAfterCleanup == 0xF
            && wrappedAddBoundary.FlagsAtStop is
                { Carry: true, Parity: true, AuxiliaryCarry: false, Zero: false,
                  Sign: false, Overflow: false }
            && wrappedAddBoundary.InstructionOrder.Length == 1
            && wrappedAddBoundary.InstructionOrder[0].Kind
                == GameSocketReceiveOperationReloadGateInstructionKind.Add10ToPostDiagnosticEsp);

        var missingReload = Project(
            postPacketEsi: 0x9000,
            postPacketEsp: 0xFFFF_FFF8,
            parentOperation: 4,
            reloadedOperation: null);
        check("The independent F64 DWORD reload is an attempted read and missing EAX prevents the stack store and TEST",
            missingReload.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshReloadedOperationAtF64
            && missingReload.StopAtRawFileOffset == 0xBD2F64
            && missingReload.OperationAddress == 0x9DF4
            && missingReload.MemoryEffects.Single() is
            {
                Kind: GameSocketReceiveOperationReloadGateMemoryEffectKind
                    .FreshReloadedOperationRead,
                Address: 0x9DF4,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingReload.OperationStackAddress is null
            && missingReload.FlagsAtStop is
            {
                Carry: false, Parity: false, AuxiliaryCarry: false,
                Zero: false, Sign: false, Overflow: false
            }
            && missingReload.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveOperationReloadGateCaptureState.Missing
            && missingReload.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached);

        var directUnusedDiagnosticCaptures = Project(
            parentOperation: 0x40,
            diagnosticReturned: true,
            postDiagnosticEsp: 0xA000,
            postDiagnosticEsi: 0xB000,
            reloadedOperation: 1);
        check("Direct-route diagnostic captures remain unused and dirty without erasing the F88 endpoint",
            directUnusedDiagnosticCaptures.Stop
                == GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88
            && directUnusedDiagnosticCaptures.ReachedBoundedEndpoint
            && directUnusedDiagnosticCaptures.ExactBoundedCompletion
            && !directUnusedDiagnosticCaptures.IsCleanCompletion
            && directUnusedDiagnosticCaptures.SuppliedButUnusedInputs.SequenceEqual(new[]
            {
                GameSocketReceiveOperationReloadGateInput.PostPacketDiagnosticReturnedNormally,
                GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsp,
                GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsi
            })
            && directUnusedDiagnosticCaptures.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveOperationReloadGateCaptureState.Captured
            && directUnusedDiagnosticCaptures.OperationAddress == 0x1DF4);

        var missingFreshEsi = Project(postPacketEsi: null, parentOperation: 1);
        var parent996 = Project(parentResult: 0, accessorReturned: true, error: 996);
        check("Incomplete parents preserve their exact F2A or 3001 bounds and never admit reloaded captures",
            missingFreshEsi.Stop
                == GameSocketReceiveOperationReloadGateStop.ParentDidNotReachCleanEndpoint
            && missingFreshEsi.StopAtRawFileOffset == 0xBD2F2A
            && !missingFreshEsi.ExactBoundedCompletion
            && !missingFreshEsi.ParentHandoffAdmitted
            && missingFreshEsi.MemoryEffects.IsEmpty
            && missingFreshEsi.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused
            && parent996.Stop
                == GameSocketReceiveOperationReloadGateStop.ParentDidNotReachCleanEndpoint
            && parent996.StopAtRawFileOffset == 0xBD3001
            && parent996.ParentProjection.ExactBoundedCompletion
            && parent996.ExactBoundedCompletion
            && parent996.FlagsAtStop == parent996.ParentProjection.FlagsAtStop
            && !parent996.ParentHandoffAdmitted
            && parent996.MemoryEffects.IsEmpty);

        var dirtyReceiveSuccess = Project(
            parentResult: 1,
            accessorReturned: true,
            error: 996);
        check("A dirty nested receive-success bound is preserved without admitting child captures",
            dirtyReceiveSuccess.Stop
                == GameSocketReceiveOperationReloadGateStop.ParentDidNotReachCleanEndpoint
            && dirtyReceiveSuccess.StopAtRawFileOffset == 0xBD2EEB
            && dirtyReceiveSuccess.ExactBoundedCompletion
            && dirtyReceiveSuccess.ParentProjection.Stop
                == GameSocketReceivePostPacketStackPreparationStop.ParentDidNotReachPacketCall
            && dirtyReceiveSuccess.ParentProjection.ParentProjection.Stop
                == GameSocketReceivePacketCallPreparationStop.ParentDidNotReachSenderHelper
            && dirtyReceiveSuccess.ParentProjection.ParentProjection.ParentProjection.Stop
                == GameSocketReceiveSenderPreparationStop.ParentNonzeroEndpointWasDirty
            && dirtyReceiveSuccess.ParentProjection.ParentProjection.ParentProjection.ParentProjection
                is
                {
                    Stop: GameSocketReceiveResultGateStop.NonzeroResultAtBounded2EEB,
                    StopAtRawFileOffset: 0xBD2EEB,
                    ReachedBoundedEndpoint: true
                }
            && dirtyReceiveSuccess.ParentProjection.FlagsAtStop == dirtyReceiveSuccess.FlagsAtStop
            && !dirtyReceiveSuccess.ParentHandoffAdmitted
            && dirtyReceiveSuccess.CaptureStates.PostPacketDiagnosticReturnedNormally
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && dirtyReceiveSuccess.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && dirtyReceiveSuccess.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveOperationReloadGateCaptureState.Unreached
            && dirtyReceiveSuccess.CaptureStates.FreshReloadedOperation
                == GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused
            && dirtyReceiveSuccess.MemoryEffects.IsEmpty);

        var zero = Project(
            parentOperation: 0,
            diagnosticReturned: true,
            postDiagnosticEsp: 0xFFFF_FFF0,
            postDiagnosticEsi: 0x2000,
            reloadedOperation: 0);
        check("Parent memory is retained before the uncommitted fresh reload, committed stack write, and ordered pushes",
            zero.OrderedMemoryEffects.Take(zero.ParentProjection.OrderedMemoryEffects.Length)
                .Select(step => step.ParentMemoryEffect)
                .SequenceEqual(zero.ParentProjection.OrderedMemoryEffects.Select(step =>
                    (GameSocketReceivePostPacketStackPreparationTraceStep?)step))
            && zero.OrderedMemoryEffects.Skip(zero.ParentProjection.OrderedMemoryEffects.Length)
                .Select(step => step.ReloadGateMemoryEffect?.Kind)
                .SequenceEqual(new GameSocketReceiveOperationReloadGateMemoryEffectKind?[]
                {
                    GameSocketReceiveOperationReloadGateMemoryEffectKind
                        .FreshReloadedOperationRead,
                    GameSocketReceiveOperationReloadGateMemoryEffectKind.OperationStackWrite,
                    GameSocketReceiveOperationReloadGateMemoryEffectKind.PushOperationStackWrite,
                    GameSocketReceiveOperationReloadGateMemoryEffectKind.PushTokenStackWrite,
                    GameSocketReceiveOperationReloadGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite,
                    GameSocketReceiveOperationReloadGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite
                })
            && zero.PostDiagnosticEspAfterCleanup == 0
            && zero.OperationStackAddress == 0x18
            && zero.MemoryEffects.Skip(2).Select(effect => (effect.Address, effect.Value))
                .SequenceEqual(new (uint?, uint?)[]
                {
                (0xFFFF_FFFC, 0),
                (0xFFFF_FFF8, 0x5B),
                (0xFFFF_FFF4, 0x1C5EA58),
                (0xFFFF_FFF0, 0x1C5EA58)
                })
            && zero.StackPointerAtDiagnosticCall == 0xFFFF_FFF0
            && zero.Calls.Last() is
            {
                Kind: GameSocketReceiveOperationReloadGateCallKind.DiagnosticImportBoundary,
                CallSiteRawFileOffset: 0xBD2F7F,
                ImportCellVirtualAddress: 0x1C15EA0,
                TargetCaptured: false,
                CallSiteReached: true,
                Attempted: false,
                StackArguments: var args
            }
            && args.SequenceEqual(new uint?[] { 0x1C5EA58, 0x1C5EA58, 0x5B, 0 })
            && !zero.DiagnosticImportExecuted
            && !zero.RepostApiModeled
            && GameSocketReceiveOperationReloadGateProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0x8B, 0x86, 0xF4, 0x0D, 0x00, 0x00,
                    0x89, 0x44, 0x24, 0x18,
                    0x85, 0xC0, 0x75, 0x16, 0x50, 0x6A, 0x5B,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
                    0x83, 0xC4, 0x10
                })
            && GameSocketReceiveOperationReloadGateProjection.NonzeroBranchTargetRawFileOffset
                == 0xBD2F88
            && GameSocketReceiveOperationReloadGateProjection.DiagnosticImportCallRawFileOffset
                == 0xBD2F7F);
    }
}
