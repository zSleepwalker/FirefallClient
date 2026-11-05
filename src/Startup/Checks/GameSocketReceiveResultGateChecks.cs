using Firefall.Reconstruction.Startup;

internal static class GameSocketReceiveResultGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        static GameSocketReceiveResultGateProjectionResult Project(
            uint? result = 1,
            uint? receiver = null,
            bool? accessorReturned = null,
            uint? error = null,
            uint? vtable = null,
            uint? callbackSlot = null,
            bool? callbackReturned = null) =>
            GameSocketReceiveResultGateProjection.Project(
                result,
                receiver,
                accessorReturned,
                error,
                vtable,
                callbackSlot,
                callbackReturned);

        var success = Project(result: 0x8000_0000, receiver: 0x1234, accessorReturned: true,
            error: 996, vtable: 0x2000, callbackSlot: 0x3000, callbackReturned: true);
        check("Any nonzero BOOL takes the success branch at EEB without entering repost or helper code",
            success.Stop == GameSocketReceiveResultGateStop.NonzeroResultAtBounded2EEB
            && success.StopAtRawFileOffset == 0xBD2EEB
            && success.ReachedBoundedEndpoint
            && !success.IsCleanCompletion
            && success.FlagsAfterResultTest is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && success.SuppliedButUnusedInputs.Length == 6
            && !success.OpaqueCallsExecuted
            && !success.ResultCallReissued
            && !success.AnyMemoryWriteCommitted);

        var missingResult = Project(result: null, accessorReturned: true, error: 996);
        check("Missing result capture stops before branching or repeating the prior result API call",
            missingResult.Stop == GameSocketReceiveResultGateStop.AwaitingResultEaxAtTest
            && missingResult.StopAtRawFileOffset == 0xBD2ED7
            && missingResult.InstructionOrder.Length == 1
            && !missingResult.OpaqueCalls.Any()
            && missingResult.CaptureStates.ResultEax
                == GameSocketReceiveResultGateCaptureState.Missing
            && missingResult.CaptureStates.ErrorAccessorReturnedNormally
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused
            && missingResult.FlagsAtStop is null
            && !missingResult.ResultCallReissued);

        var accessorPending = Project(result: 0, accessorReturned: null, error: 996);
        var accessorFalse = Project(result: 0, accessorReturned: false, error: 996);
        check("Zero BOOL attempts the ordinal-111 accessor once; missing and false returns remain distinct",
            accessorPending.OpaqueCalls.Single() is
            {
                Kind: GameSocketReceiveResultGateOpaqueCallKind.ErrorAccessor,
                TargetAddress: null,
                TargetCaptured: false,
                ImportCellVirtualAddress: 0x01AC4DA0,
                CallSiteReached: true,
                Attempted: true,
                ReturnedNormally: null,
                Status: GameSocketReceiveResultGateOpaqueCallStatus.AwaitingReturnCapture
            }
            && accessorPending.Stop == GameSocketReceiveResultGateStop.AwaitingErrorAccessorReturn
            && accessorPending.StopAtRawFileOffset == 0xBD2EDB
            && accessorPending.FlagsAtStop == accessorPending.FlagsAfterResultTest
            && accessorFalse.Stop == GameSocketReceiveResultGateStop.ErrorAccessorDidNotReturnNormally
            && accessorFalse.OpaqueCalls.Single().Status
                == GameSocketReceiveResultGateOpaqueCallStatus.DidNotReturnNormally
            && accessorFalse.FlagsAtStop == accessorFalse.FlagsAfterResultTest
            && accessorFalse.CaptureStates.ErrorAccessorEax
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused
            && GameSocketReceiveResultGateProjection.ErrorAccessorIatRawFileOffset == 0x016C39A0
            && GameSocketReceiveResultGateProjection.ErrorAccessorImport == "WSOCK32.dll!ordinal 111");

        var missingError = Project(result: 0, accessorReturned: true);
        check("Normal accessor return invalidates TEST flags before a missing EAX capture stops at E1",
            missingError.Stop == GameSocketReceiveResultGateStop.AwaitingErrorAccessorEaxBeforeCompare
            && missingError.StopAtRawFileOffset == 0xBD2EE1
            && missingError.FlagsAfterResultTest is not null
            && missingError.FlagsAtStop is null
            && missingError.CaptureStates.ErrorAccessorEax
                == GameSocketReceiveResultGateCaptureState.Missing
            && missingError.OpaqueCalls.Single().ReturnedNormally == true);

        var error996 = Project(result: 0, accessorReturned: true, error: 996);
        var error997 = Project(result: 0, accessorReturned: true, error: 997,
            receiver: null, vtable: 0x4000);
        check("Only exact error 996 takes the shared JZ cleanup bound; 997 proceeds toward receiver state",
            error996.Stop == GameSocketReceiveResultGateStop.ErrorCode996AtBounded3001
            && error996.StopAtRawFileOffset == 0xBD3001
            && error996.ReachedBoundedEndpoint
            && error996.IsCleanCompletion
            && error996.FlagsAfterErrorCompare is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && error997.Stop == GameSocketReceiveResultGateStop.AwaitingReceiverEsiBeforeVtableRead
            && error997.StopAtRawFileOffset == 0xBD2FF9
            && error997.FlagsAtStop == error997.FlagsAfterErrorCompare
            && error997.CaptureStates.FreshReceiverVtable
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused);

        var missingVtable = Project(result: 0, accessorReturned: true, error: 997,
            receiver: 0x1234);
        check("Non-996 route requires inherited ESI and independently captures a fresh vtable read",
            missingVtable.Stop == GameSocketReceiveResultGateStop.AwaitingFreshReceiverVtableAtFF9
            && missingVtable.FreshReceiverVtableReadAttempted
            && missingVtable.FreshReceiverVtableReadAddress == 0x1234
            && missingVtable.MemoryEffects.Single() is
            {
                Kind: GameSocketReceiveResultGateMemoryEffectKind.FreshReceiverVtableRead,
                RawFileOffset: 0xBD2FF9,
                Address: 0x1234,
                Value: null,
                Attempted: true,
                Captured: false
            }
            && missingVtable.CaptureStates.FreshReceiverVtable
                == GameSocketReceiveResultGateCaptureState.Missing
            && missingVtable.FlagsAtStop == missingVtable.FlagsAfterErrorCompare
            && !missingVtable.ReceiverCallbackCallSiteReached);

        var missingReceiver = Project(result: 0, accessorReturned: true, error: 997,
            vtable: 0x2000, callbackSlot: 0x3000);
        check("Missing ESI stops before vtable access and does not consume later address captures",
            missingReceiver.Stop == GameSocketReceiveResultGateStop.AwaitingReceiverEsiBeforeVtableRead
            && !missingReceiver.FreshReceiverVtableReadAttempted
            && missingReceiver.MemoryEffects.IsEmpty
            && missingReceiver.InstructionOrder[^1].RawFileOffset == 0xBD2FF7
            && missingReceiver.CaptureStates.ReceiverEsi
                == GameSocketReceiveResultGateCaptureState.Missing
            && missingReceiver.CaptureStates.FreshReceiverVtable
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused);

        var missingSlot = Project(result: 0, accessorReturned: true, error: 997,
            receiver: 0xFFFF_FFF0, vtable: 0xFFFF_FFF0);
        check("Missing slot capture wraps vtable plus 20 and stops at FFE after ECX and error push",
            missingSlot.Stop == GameSocketReceiveResultGateStop.AwaitingFreshCallbackSlotAtFFE
            && missingSlot.FreshCallbackSlotReadAddress == 0x10
            && missingSlot.ReceiverCallbackCallSiteReached
            && missingSlot.InstructionOrder[^3].Kind
                == GameSocketReceiveResultGateInstructionKind.MoveEcxFromReceiverEsi
            && missingSlot.InstructionOrder[^2] is
            {
                Kind: GameSocketReceiveResultGateInstructionKind.PushErrorAccessorEax,
                RawFileOffset: 0xBD2FFD,
                OperandValue: 997,
                EffectiveAddress: null
            }
            && missingSlot.InstructionOrder[^1].RawFileOffset == 0xBD2FFE
            && missingSlot.OpaqueCalls.Single(c =>
                c.Kind == GameSocketReceiveResultGateOpaqueCallKind.ReceiverErrorCallback) is
            {
                CallSiteReached: true,
                Attempted: false,
                TargetCaptured: false,
                TargetAddress: null,
                ReturnedNormally: null,
                Status: null,
                EaxArgument: 997,
                EcxArgument: 0xFFFF_FFF0
            }
            && missingSlot.MemoryEffects[^1] is
            {
                RawFileOffset: 0xBD2FFE,
                Address: 0x10,
                Attempted: true,
                Captured: false
            });

        var callbackPending = Project(result: 0, accessorReturned: true, error: 997,
            receiver: 0x1000, vtable: 0, callbackSlot: 0);
        var callbackFalse = Project(result: 0, accessorReturned: true, error: 997,
            receiver: 0x1000, vtable: 0xFFFF_FFF0, callbackSlot: 0x5000,
            callbackReturned: false);
        var callbackNormal = Project(result: 0, accessorReturned: true, error: 997,
            receiver: 0x1000, vtable: 0x2000, callbackSlot: 0x5000,
            callbackReturned: true);
        check("Captured zero and wrapped pointers remain scalar evidence; opaque callback returns are not executed",
            callbackPending.Stop == GameSocketReceiveResultGateStop.AwaitingErrorCallbackReturn
            && callbackPending.FreshCallbackSlotReadAddress == 0x20
            && callbackPending.OpaqueCalls.Last() is
            {
                TargetAddress: 0,
                TargetCaptured: true,
                Attempted: true,
                ReturnedNormally: null,
                Status: GameSocketReceiveResultGateOpaqueCallStatus.AwaitingReturnCapture
            }
            && callbackFalse.Stop == GameSocketReceiveResultGateStop.ErrorCallbackDidNotReturnNormally
            && callbackFalse.FreshCallbackSlotReadAddress == 0x10
            && callbackFalse.FlagsAtStop == callbackFalse.FlagsAfterErrorCompare
            && callbackFalse.OpaqueCalls.Last().Status
                == GameSocketReceiveResultGateOpaqueCallStatus.DidNotReturnNormally
            && callbackNormal.Stop
                == GameSocketReceiveResultGateStop.ErrorCallbackReturnedAtBounded3001
            && callbackNormal.StopAtRawFileOffset == 0xBD3001
            && callbackNormal.ReachedBoundedEndpoint
            && callbackNormal.IsCleanCompletion
            && callbackNormal.FlagsAtStop is null
            && !callbackNormal.OpaqueCallsExecuted
            && !callbackNormal.AnyMemoryWriteCommitted);

        var zeroError = Project(result: 0, accessorReturned: true, error: 0,
            receiver: 0x1000, vtable: 0x2000, callbackSlot: 0x5000,
            callbackReturned: true);
        check("Zero error-accessor value is still a non-996 callback route, not a success or cleanup shortcut",
            zeroError.Stop
                == GameSocketReceiveResultGateStop.ErrorCallbackReturnedAtBounded3001
            && zeroError.OpaqueCalls.Length == 2
            && zeroError.OpaqueCalls.Last().EaxArgument == 0
            && zeroError.ReceiverCallbackCallSiteReached
            && zeroError.FreshReceiverVtableReadAddress == 0x1000
            && zeroError.FlagsAfterErrorCompare is
            {
                Carry: true,
                Zero: false
            });

        var dirty996 = Project(result: 0, receiver: 0x1000, accessorReturned: true,
            error: 996, vtable: 0x2000, callbackSlot: 0x3000, callbackReturned: true);
        check("Unused receiver and callback captures dirty the 996 bound without changing its flags or endpoint",
            dirty996.Stop == GameSocketReceiveResultGateStop.ErrorCode996AtBounded3001
            && dirty996.ReachedBoundedEndpoint
            && !dirty996.IsCleanCompletion
            && dirty996.FlagsAtStop == dirty996.FlagsAfterErrorCompare
            && dirty996.SuppliedButUnusedInputs.Contains(
                GameSocketReceiveResultGateInput.ReceiverEsi)
            && dirty996.SuppliedButUnusedInputs.Contains(
                GameSocketReceiveResultGateInput.FreshReceiverVtable)
            && dirty996.SuppliedButUnusedInputs.Contains(
                GameSocketReceiveResultGateInput.FreshCallbackSlot)
            && dirty996.SuppliedButUnusedInputs.Contains(
                GameSocketReceiveResultGateInput.ErrorCallbackReturnedNormally)
            && !dirty996.FreshReceiverVtableReadAttempted
            && !dirty996.ReceiverCallbackCallSiteReached);

        var dirtySuccess = Project(result: 1, receiver: 0x1000,
            accessorReturned: true, error: 996);
        check("Supplied but unconsumed false-path captures never overwrite successful TEST evidence",
            dirtySuccess.ReachedBoundedEndpoint
            && dirtySuccess.FlagsAtStop == dirtySuccess.FlagsAfterResultTest
            && dirtySuccess.HasSuppliedButUnusedInputs
            && dirtySuccess.CaptureStates.ReceiverEsi
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused
            && dirtySuccess.CaptureStates.ErrorAccessorEax
                == GameSocketReceiveResultGateCaptureState.SuppliedButUnused);

        check("Raw boundaries and import identities match the bounded receive-result gate",
            GameSocketReceiveResultGateProjection.MethodRawFileOffset == 0xBD2ED7
            && GameSocketReceiveResultGateProjection.ResultNonzeroBranchRawFileOffset == 0xBD2ED9
            && GameSocketReceiveResultGateProjection.ErrorAccessorCallRawFileOffset == 0xBD2EDB
            && GameSocketReceiveResultGateProjection.ErrorCodeCompareRawFileOffset == 0xBD2EE1
            && GameSocketReceiveResultGateProjection.SharedGateJumpRawFileOffset == 0xBD2EE6
            && GameSocketReceiveResultGateProjection.SharedErrorGateRawFileOffset == 0xBD2FF7
            && GameSocketReceiveResultGateProjection.ReceiverVtableReadRawFileOffset == 0xBD2FF9
            && GameSocketReceiveResultGateProjection.ReceiverEcxMoveRawFileOffset == 0xBD2FFB
            && GameSocketReceiveResultGateProjection.ErrorEaxPushRawFileOffset == 0xBD2FFD
            && GameSocketReceiveResultGateProjection.ReceiverCallbackCallRawFileOffset == 0xBD2FFE
            && GameSocketReceiveResultGateProjection.BoundedSuccessRawFileOffset == 0xBD2EEB
            && GameSocketReceiveResultGateProjection.BoundedErrorCleanupRawFileOffset == 0xBD3001
            && GameSocketReceiveResultGateProjection.ErrorAccessorIatVirtualAddress == 0x01AC4DA0
            && GameSocketReceiveResultGateProjection.PriorResultCallIatVirtualAddress == 0x01AC4D64
            && GameSocketReceiveResultGateProjection.ReceiverCallbackSlotOffset == 0x20);

        var cleanSuccess = Project(result: 2);
        check("Clean noncanonical BOOL completion and captured callback reads retain their distinct meanings",
            cleanSuccess.IsCleanCompletion
            && cleanSuccess.StopAtRawFileOffset == 0xBD2EEB
            && callbackNormal.MemoryEffects.All(effect =>
                effect.Attempted && effect.Captured && !effect.Committed));
    }
}
