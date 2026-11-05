using Firefall.Reconstruction.Startup;

internal static class GameSocketReceiveBufferStatusGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceiveBufferStatusGateProjectionResult Project(
            uint? parentOperation = 1,
            uint? reloadedOperation = 1,
            uint? postPacketEsi = 0x1000,
            uint? postPacketEsp = 0x2000,
            bool? parentDiagnosticReturned = null,
            uint? parentPostDiagnosticEsp = null,
            uint? parentPostDiagnosticEsi = null,
            bool? diagnosticReturned = null,
            uint? freshPostDiagnosticEsp = null,
            uint? freshPostDiagnosticEsi = null,
            uint? firstReadBase = 0,
            uint? comparisonValue = 1)
        {
            var parentInputs =
                new GameSocketReceivePostPacketStackPreparationProjectionInputs(
                    new(
                        new(
                            new(
                                1,
                                null,
                                null,
                                null,
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
                        postPacketEsi,
                        postPacketEsp,
                        parentOperation),
                    true,
                    postPacketEsi,
                    postPacketEsp,
                    parentOperation);
            var parentGateInputs =
                new GameSocketReceiveOperationReloadGateProjectionInputs(
                    parentInputs,
                    parentDiagnosticReturned,
                    parentPostDiagnosticEsp,
                    parentPostDiagnosticEsi,
                    reloadedOperation);
            return GameSocketReceiveBufferStatusGateProjection.Project(new(
                parentGateInputs,
                diagnosticReturned,
                freshPostDiagnosticEsp,
                freshPostDiagnosticEsi,
                firstReadBase,
                comparisonValue));
        }

        check("The F88 byte window is the exact 50-byte status gate",
            GameSocketReceiveBufferStatusGateProjection.WindowBytes.Length == 50
            && GameSocketReceiveBufferStatusGateProjection.WindowBytes.SequenceEqual(
                new byte[]
                {
                    0x8B, 0x9E, 0xF4, 0x0D, 0x00, 0x00,
                    0x81, 0xC3, 0x98, 0x00, 0x00, 0x00,
                    0xC7, 0x03, 0x80, 0x00, 0x00, 0x00,
                    0x83, 0xBE, 0xF4, 0x0D, 0x00, 0x00, 0x00,
                    0x75, 0x17,
                    0x6A, 0x00,
                    0x6A, 0x5B,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0x68, 0x58, 0xEA, 0xC5, 0x01,
                    0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
                    0x83, 0xC4, 0x10
                }));

        var priorNonzeroReloadNonzero = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            postPacketEsi: 0x1111_0000,
            postPacketEsp: 0x2222_0000,
            firstReadBase: 0x1234,
            comparisonValue: 1);
        check("A prior nonzero route followed by a nonzero reload admits F88 using packet register lineage",
            priorNonzeroReloadNonzero.ParentProjection.Route
                == GameSocketReceiveOperationReloadGateRoute.DirectNonzero
            && priorNonzeroReloadNonzero.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88
            && priorNonzeroReloadNonzero.Route
                == GameSocketReceiveBufferStatusGateRoute.DirectNonzero
            && priorNonzeroReloadNonzero.EffectiveEsi == 0x1111_0000
            && priorNonzeroReloadNonzero.ParentRouteEsp == 0x2222_0000
            && priorNonzeroReloadNonzero.EffectiveEsp == 0x2222_0000
            && priorNonzeroReloadNonzero.IsCleanCompletion
            && priorNonzeroReloadNonzero.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveBufferStatusGateCaptureState.Unreached);

        var priorZeroReloadNonzero = Project(
            parentOperation: 0,
            reloadedOperation: 1,
            postPacketEsi: 0x1111_0000,
            postPacketEsp: 0x2222_0000,
            parentDiagnosticReturned: true,
            parentPostDiagnosticEsp: 0x5555_0000,
            parentPostDiagnosticEsi: 0x1000,
            firstReadBase: 0x1D5C,
            comparisonValue: 0);
        check("A prior zero route followed by a nonzero reload still admits F88 and uses diagnostic register lineage",
            priorZeroReloadNonzero.ParentProjection.Route
                == GameSocketReceiveOperationReloadGateRoute.ZeroDiagnostic
            && priorZeroReloadNonzero.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88
            && priorZeroReloadNonzero.Route
                == GameSocketReceiveBufferStatusGateRoute.DirectNonzero
            && priorZeroReloadNonzero.EffectiveEsi == 0x1000
            && priorZeroReloadNonzero.ParentRouteEsp == 0x5555_0010
            && priorZeroReloadNonzero.EffectiveEsp == 0x5555_0010
            && priorZeroReloadNonzero.FirstReadAddress == 0x1DF4
            && priorZeroReloadNonzero.EbxAfterAdd == 0x1DF4
            && priorZeroReloadNonzero.ComparisonAddress == 0x1DF4
            && priorZeroReloadNonzero.MemoryEffects[1] is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
                Address: 0x1DF4,
                Value: 0x80,
                Committed: true
            }
            && priorZeroReloadNonzero.ParentProjection.MemoryEffects
                .Single(effect =>
                    effect.Kind
                    == GameSocketReceiveOperationReloadGateMemoryEffectKind
                        .FreshReloadedOperationRead).Value == 1
            && priorZeroReloadNonzero.MemoryEffects[0].Value == 0x1D5C
            && priorZeroReloadNonzero.IsCleanCompletion);

        var priorNonzeroReloadZero = Project(
            parentOperation: 1,
            reloadedOperation: 0,
            postPacketEsi: 0x3333_0000,
            postPacketEsp: 0xFFFF_FFFF,
            diagnosticReturned: true,
            freshPostDiagnosticEsp: 0xFFFF_FFF0,
            freshPostDiagnosticEsi: 0x4444_0000,
            firstReadBase: 0,
            comparisonValue: 0);
        check("A prior nonzero route followed by a zero reload requires independent post-F7F ESP and ESI",
            priorNonzeroReloadZero.ParentProjection.Route
                == GameSocketReceiveOperationReloadGateRoute.DirectNonzero
            && priorNonzeroReloadZero.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.DiagnosticImportBoundaryAtF7F
            && priorNonzeroReloadZero.Route
                == GameSocketReceiveBufferStatusGateRoute.ZeroDiagnostic
            && priorNonzeroReloadZero.EffectiveEsi == 0x4444_0000
            && priorNonzeroReloadZero.ParentRouteEsp is null
            && priorNonzeroReloadZero.EffectiveEsp == 0
            && priorNonzeroReloadZero.PostDiagnosticEspAfterCleanup == 0
            && priorNonzeroReloadZero.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.Captured
            && priorNonzeroReloadZero.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.Captured
            && priorNonzeroReloadZero.EffectiveEsi != 0x3333_0000
            && priorNonzeroReloadZero.EffectiveEsp != 0xFFFF_FFFF
            && priorNonzeroReloadZero.IsCleanCompletion
            && priorNonzeroReloadZero.StackPointerAtDiagnosticCall == 0xFFFF_FFF0
            && priorNonzeroReloadZero.MemoryEffects
                .Where(effect => effect.Kind
                    is GameSocketReceiveBufferStatusGateMemoryEffectKind.PushZeroStackWrite
                    or GameSocketReceiveBufferStatusGateMemoryEffectKind.PushTokenStackWrite
                    or GameSocketReceiveBufferStatusGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite)
                .Select(effect => effect.Address)
                .SequenceEqual(new uint?[]
                {
                    0xFFFF_FFFC,
                    0xFFFF_FFF8,
                    0xFFFF_FFF4,
                    0xFFFF_FFF0
                }));

        var priorZeroReloadZero = Project(
            parentOperation: 0,
            reloadedOperation: 0,
            postPacketEsi: 0x4444_0000,
            postPacketEsp: 0x2222_0000,
            parentDiagnosticReturned: true,
            parentPostDiagnosticEsp: 0xFFFF_FFE8,
            parentPostDiagnosticEsi: 0x7777_0000,
            diagnosticReturned: true,
            freshPostDiagnosticEsp: 0xFFFF_FFEF,
            freshPostDiagnosticEsi: 0x8888_0000,
            firstReadBase: 0xFFFF_FF68,
            comparisonValue: 0);
        check("A prior zero route followed by a zero reload does not forward the earlier F5B ESP/ESI",
            priorZeroReloadZero.ParentProjection.Route
                == GameSocketReceiveOperationReloadGateRoute.ZeroDiagnostic
            && priorZeroReloadZero.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.DiagnosticImportBoundaryAtF7F
            && priorZeroReloadZero.Route
                == GameSocketReceiveBufferStatusGateRoute.ZeroDiagnostic
            && priorZeroReloadZero.EffectiveEsi == 0x8888_0000
            && priorZeroReloadZero.ParentRouteEsp is null
            && priorZeroReloadZero.EffectiveEsp == 0xFFFF_FFFF
            && priorZeroReloadZero.PostDiagnosticEspAfterCleanup == 0xFFFF_FFFF
            && priorZeroReloadZero.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.Captured
            && priorZeroReloadZero.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.Captured
            && priorZeroReloadZero.EffectiveEsi != 0x7777_0000
            && priorZeroReloadZero.EffectiveEsp != priorZeroReloadZero.ParentProjection
                .PostDiagnosticEspAfterCleanup
            && priorZeroReloadZero.EbxAfterAdd == 0
            && priorZeroReloadZero.MemoryEffects[1] is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
                Address: 0,
                Value: 0x80,
                Committed: true
            }
            && priorZeroReloadZero.StackPointerAtDiagnosticCall == 0xFFFF_FFEF
            && priorZeroReloadZero.MemoryEffects
                .Where(effect => effect.Kind
                    is GameSocketReceiveBufferStatusGateMemoryEffectKind.PushZeroStackWrite
                    or GameSocketReceiveBufferStatusGateMemoryEffectKind.PushTokenStackWrite
                    or GameSocketReceiveBufferStatusGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite)
                .Select(effect => effect.Address)
                .SequenceEqual(new uint?[]
                {
                    0xFFFF_FFFB,
                    0xFFFF_FFF7,
                    0xFFFF_FFF3,
                    0xFFFF_FFEF
                })
            && priorZeroReloadZero.IsCleanCompletion);

        check("The ordered trace preserves parent effects before read, store, fresh compare, and pushes",
            priorZeroReloadZero.OrderedMemoryEffects
                .TakeWhile(step => step.ParentMemoryEffect.HasValue).Count()
                == priorZeroReloadZero.ParentProjection.OrderedMemoryEffects.Length
            && priorZeroReloadZero.OrderedMemoryEffects
                .Where(step => step.GateMemoryEffect.HasValue)
                .Select(step => step.GateMemoryEffect!.Value.Kind)
                .SequenceEqual(new[]
                {
                    GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshFirstReadBaseRead,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshComparisonValueRead,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind.PushZeroStackWrite,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind.PushTokenStackWrite,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite,
                    GameSocketReceiveBufferStatusGateMemoryEffectKind
                        .PushDiagnosticArgumentStackWrite
                }));

        var missingReturn = Project(
            parentOperation: 1,
            reloadedOperation: 0,
            diagnosticReturned: null,
            freshPostDiagnosticEsp: 0x7000,
            freshPostDiagnosticEsi: 0x8000,
            firstReadBase: 1,
            comparisonValue: 0);
        var nonnormalReturn = Project(
            parentOperation: 1,
            reloadedOperation: 0,
            diagnosticReturned: false,
            freshPostDiagnosticEsp: 0x7000,
            freshPostDiagnosticEsi: 0x8000,
            firstReadBase: 1,
            comparisonValue: 0);
        check("Pending and nonnormal F7F returns preserve parent flags and do not claim a child endpoint",
            missingReturn.Stop
                == GameSocketReceiveBufferStatusGateStop.AwaitingDiagnosticReturnAtF7F
            && missingReturn.FlagsAtStop == missingReturn.ParentProjection.FlagsAtStop
            && missingReturn.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveBufferStatusGateCaptureState.Missing
            && missingReturn.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && missingReturn.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && missingReturn.EffectiveEsi is null
            && missingReturn.EffectiveEsp is null
            && missingReturn.MemoryEffects.IsEmpty
            && missingReturn.InstructionOrder.IsEmpty
            && !missingReturn.ReachedBoundedEndpoint
            && !missingReturn.ExactBoundedCompletion
            && nonnormalReturn.Stop
                == GameSocketReceiveBufferStatusGateStop.DiagnosticDidNotReturnNormally
            && nonnormalReturn.FlagsAtStop
                == nonnormalReturn.ParentProjection.FlagsAtStop
            && nonnormalReturn.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveBufferStatusGateCaptureState.Captured
            && nonnormalReturn.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && nonnormalReturn.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && nonnormalReturn.EffectiveEsi is null
            && nonnormalReturn.EffectiveEsp is null
            && !nonnormalReturn.ReachedBoundedEndpoint
            && !nonnormalReturn.ExactBoundedCompletion);

        var missingDiagnosticEsp = Project(
            parentOperation: 1,
            reloadedOperation: 0,
            diagnosticReturned: true,
            freshPostDiagnosticEsi: 0x8000,
            firstReadBase: 1,
            comparisonValue: 1);
        var missingDiagnosticEsi = Project(
            parentOperation: 1,
            reloadedOperation: 0,
            diagnosticReturned: true,
            freshPostDiagnosticEsp: 0x7FFF_FFF8,
            firstReadBase: 1,
            comparisonValue: 1);
        check("Normal F7F return requires fresh ESP before ADD and fresh ESI independently at F88",
            missingDiagnosticEsp.Stop
                == GameSocketReceiveBufferStatusGateStop.AwaitingFreshPostDiagnosticEspAtF85
            && missingDiagnosticEsp.EffectiveEsi is null
            && missingDiagnosticEsp.EffectiveEsp is null
            && missingDiagnosticEsp.FlagsAtStop is null
            && missingDiagnosticEsp.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.Missing
            && missingDiagnosticEsp.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && !missingDiagnosticEsp.ExactBoundedCompletion
            && missingDiagnosticEsi.Stop
                == GameSocketReceiveBufferStatusGateStop.AwaitingFreshPostDiagnosticEsiAtF88
            && missingDiagnosticEsi.EffectiveEsi is null
            && missingDiagnosticEsi.EffectiveEsp == 0x8000_0008
            && missingDiagnosticEsi.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.Missing
            && missingDiagnosticEsi.CaptureStates.FreshFirstReadBase
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && missingDiagnosticEsi.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && missingDiagnosticEsi.MemoryEffects.IsEmpty
            && !missingDiagnosticEsi.ExactBoundedCompletion);

        var missingFirstRead = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            postPacketEsi: 0x3000,
            firstReadBase: null,
            comparisonValue: 0);
        var missingComparison = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            postPacketEsi: 0x1000,
            firstReadBase: 0x7FFF_FF98,
            comparisonValue: null);
        check("Missing first reads and comparisons remain pending without an exact child bound, while a missing comparison retains ADD flags",
            missingFirstRead.Stop
                == GameSocketReceiveBufferStatusGateStop.AwaitingFreshFirstReadBaseAtF88
            && missingFirstRead.MemoryEffects.Single() is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshFirstReadBaseRead,
                Address: 0x3DF4,
                Value: null,
                Attempted: true,
                Captured: false
            }
            && missingFirstRead.CaptureStates.FreshFirstReadBase
                == GameSocketReceiveBufferStatusGateCaptureState.Missing
            && !missingFirstRead.ExactBoundedCompletion
            && missingComparison.Stop
                == GameSocketReceiveBufferStatusGateStop.AwaitingFreshComparisonValueAtF9A
            && missingComparison.EbxAfterAdd == 0x8000_0030
            && missingComparison.MemoryEffects[1] is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
                Address: 0x8000_0030,
                Value: 0x80,
                Committed: true
            }
            && missingComparison.MemoryEffects[2] is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshComparisonValueRead,
                Address: 0x1DF4,
                Value: null,
                Captured: false
            }
            && missingComparison.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && !missingComparison.ExactBoundedCompletion);

        var wrappedBase = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            firstReadBase: 0xFFFF_FF68,
            comparisonValue: null);
        check("ADD EBX,98 wraps FFFFFF68 to zero, stores at zero, and retains truthful full-width flags",
            wrappedBase.EbxAfterAdd == 0
            && wrappedBase.MemoryEffects[1] is
            {
                Kind: GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
                Address: 0,
                Value: 0x80,
                Committed: true
            }
            && wrappedBase.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: true,
                Sign: false,
                Overflow: false
            });

        var unusedStatusOnF88 = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            diagnosticReturned: true,
            freshPostDiagnosticEsp: 0x9000,
            freshPostDiagnosticEsi: 0xA000,
            firstReadBase: 0,
            comparisonValue: 1);
        check("An F7F return capture supplied on the F88 route dirties completion without erasing the exact bound",
            unusedStatusOnF88.Route
                == GameSocketReceiveBufferStatusGateRoute.DirectNonzero
            && unusedStatusOnF88.ReachedBoundedEndpoint
            && unusedStatusOnF88.ExactBoundedCompletion
            && !unusedStatusOnF88.IsCleanCompletion
            && unusedStatusOnF88.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && unusedStatusOnF88.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused
            && unusedStatusOnF88.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused);

        var dirtyParentAtEndpoint = Project(
            parentOperation: 1,
            reloadedOperation: 1,
            parentPostDiagnosticEsi: 0xA000,
            firstReadBase: 0,
            comparisonValue: 0);
        var missingDiagnosticLineageEsp = Project(
            parentOperation: 0,
            reloadedOperation: 1,
            parentDiagnosticReturned: true,
            parentPostDiagnosticEsi: 0x7000,
            firstReadBase: 0,
            comparisonValue: 0);
        var missingDiagnosticLineageEsi = Project(
            parentOperation: 0,
            reloadedOperation: 1,
            parentDiagnosticReturned: true,
            parentPostDiagnosticEsp: 0x6000,
            firstReadBase: 0,
            comparisonValue: 0);
        var incompleteParent = Project(
            parentOperation: 1,
            reloadedOperation: null,
            firstReadBase: 0,
            comparisonValue: 0);
        check("Missing zero-route register lineage is rejected by the parent and cannot create an exact child bound",
            missingDiagnosticLineageEsp.Stop
                == GameSocketReceiveBufferStatusGateStop.ParentDidNotReachCleanEndpoint
            && missingDiagnosticLineageEsp.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEspAtF61
            && !missingDiagnosticLineageEsp.ParentHandoffAdmitted
            && !missingDiagnosticLineageEsp.ExactBoundedCompletion
            && missingDiagnosticLineageEsp.InstructionOrder.IsEmpty
            && missingDiagnosticLineageEsi.Stop
                == GameSocketReceiveBufferStatusGateStop.ParentDidNotReachCleanEndpoint
            && missingDiagnosticLineageEsi.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshPostDiagnosticEsiAtF64
            && !missingDiagnosticLineageEsi.ParentHandoffAdmitted
            && !missingDiagnosticLineageEsi.ExactBoundedCompletion
            && missingDiagnosticLineageEsi.InstructionOrder.IsEmpty);
        check("Rejected parents preserve only their actual exact ancestor bounds and execute no child instructions",
            dirtyParentAtEndpoint.Stop
                == GameSocketReceiveBufferStatusGateStop.ParentEndpointWasDirty
            && dirtyParentAtEndpoint.ExactBoundedCompletion
                == dirtyParentAtEndpoint.ParentProjection.ExactBoundedCompletion
            && dirtyParentAtEndpoint.InstructionOrder.IsEmpty
            && dirtyParentAtEndpoint.MemoryEffects.IsEmpty
            && incompleteParent.Stop
                == GameSocketReceiveBufferStatusGateStop.ParentDidNotReachCleanEndpoint
            && incompleteParent.ParentProjection.Stop
                == GameSocketReceiveOperationReloadGateStop.AwaitingFreshReloadedOperationAtF64
            && incompleteParent.ExactBoundedCompletion
                == incompleteParent.ParentProjection.ExactBoundedCompletion
            && !incompleteParent.ExactBoundedCompletion
            && incompleteParent.InstructionOrder.IsEmpty
            && incompleteParent.MemoryEffects.IsEmpty);
    }
}
