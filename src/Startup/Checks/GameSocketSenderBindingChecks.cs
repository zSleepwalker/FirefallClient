using Firefall.Reconstruction.Startup;

internal static class GameSocketSenderBindingChecks
{
    public static void Run(Action<string, bool> check)
    {
        static GameSocketSenderBindingProjectionResult Project(
            byte? flag,
            uint? socket = 0x1000,
            uint? sender = 0x2000,
            bool? receiverHelperReturn = true,
            bool? predicateHelperReturn = true,
            byte? al = 0xFF,
            bool? fallbackReturn = null,
            bool? queueReturn = true,
            GameSocketPacketAdmissionOutcome admission =
                GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext) =>
            GameSocketSenderBindingProjection.Project(
                admission, flag, socket, sender, receiverHelperReturn,
                predicateHelperReturn, al, fallbackReturn, queueReturn);

        var rejected = Project(
            1,
            admission: GameSocketPacketAdmissionOutcome.InvalidLength);
        check("Game-socket sender binding begins only after accepted outer packet admission",
            rejected.Stop == GameSocketSenderBindingStop.AdmissionRejected
            && rejected.StopAtRaw is null
            && rejected.Steps.SequenceEqual(
                [GameSocketSenderBindingStep.AdmissionRejected])
            && rejected.SuppliedButUnusedInputs.Contains(GameSocketSenderBindingInput.BindingFlag)
            && !rejected.FlagReadAttempted
            && !rejected.ReachedCommonTail);

        var missingSocket = Project(0, socket: null);
        check("Game-socket inherited ESI is required before the binding-flag CMP",
            missingSocket.Stop == GameSocketSenderBindingStop.AwaitingSocketEsiBeforeBindingCompare
            && missingSocket.StopAtRaw == GameSocketSenderBindingProjection.BindingFlagCompareFileOffset
            && !missingSocket.FlagReadAttempted
            && missingSocket.FlagReadAddress is null
            && missingSocket.SuppliedButUnusedInputs.Contains(GameSocketSenderBindingInput.BindingFlag)
            && missingSocket.InstructionOrder.IsEmpty);

        var missingFlag = Project(null, socket: 0xFFFF_FFF0);
        check("Missing flag capture records the attempted CMP at wrapped ESI plus 25A",
            missingFlag.Stop == GameSocketSenderBindingStop.AwaitingBindingFlagAtCompare
            && missingFlag.StopAtRaw == GameSocketSenderBindingProjection.BindingFlagCompareFileOffset
            && missingFlag.FlagReadAttempted
            && missingFlag.FlagReadAddress == 0x24A
            && missingFlag.FlagReadValue is null
            && !missingFlag.FlagReadCommitted
            && !missingFlag.FlagCompareCompleted
            && missingFlag.FlagsAfterBindingFlagCompare is null
            && !missingFlag.SuppliedButUnusedInputs.Contains(GameSocketSenderBindingInput.SocketEsi)
            && missingFlag.InstructionOrder.Single().FileOffset
                == GameSocketSenderBindingProjection.BindingFlagCompareFileOffset);

        var disabledClean = GameSocketSenderBindingProjection.Project(
            GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext,
            bindingFlag: 0,
            socketEsi: 0x1000,
            senderEbx: null,
            receiverArgumentHelperReturnedNormally: null,
            predicateValueHelperReturnedNormally: null,
            predicateValueHelperAl: null,
            fallbackHelperReturnedNormally: null,
            queueAppendReturnedNormally: null);
        check("Disabled flag reads and compares after ESI, then branches directly to the common tail",
            disabledClean.Stop == GameSocketSenderBindingStop.BindingDisabledAtCommonTail
            && disabledClean.StopAtRaw == GameSocketSenderBindingProjection.CommonTailFileOffset
            && disabledClean.ReachedCommonTail
            && disabledClean.IsCleanCompletion
            && disabledClean.FlagReadAttempted
            && disabledClean.FlagReadAddress == 0x125A
            && disabledClean.FlagReadValue == 0
            && !disabledClean.FlagReadCommitted
            && disabledClean.FlagCompareCompleted
            && disabledClean.FlagsAfterBindingFlagCompare is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && disabledClean.FlagsAtStop == disabledClean.FlagsAfterBindingFlagCompare
            && !disabledClean.ReceiverArgumentHelperCallAttempted
            && !disabledClean.PredicateValueHelperCallAttempted
            && !disabledClean.FallbackHelperCallAttempted
            && !disabledClean.QueueAppendCallAttempted
            && !disabledClean.FlagClearAttempted
            && !disabledClean.FlagClearCommitted
            && disabledClean.InstructionOrder.Length == 2
            && disabledClean.InstructionOrder[1].BranchTaken == true
            && disabledClean.InstructionOrder[1].TargetFileOffset
                == GameSocketSenderBindingProjection.CommonTailFileOffset);

        var missingDisabledSocket = Project(0, socket: null);
        check("Zero flag cannot bypass the CMP when ESI was not captured",
            missingDisabledSocket.Stop
                == GameSocketSenderBindingStop.AwaitingSocketEsiBeforeBindingCompare
            && !missingDisabledSocket.ReachedCommonTail
            && !missingDisabledSocket.FlagReadAttempted);

        var missingSender = Project(1, sender: null);
        check("Missing EBX stops before PUSH at BD3084 while a captured zero sender is valid",
            missingSender.Stop == GameSocketSenderBindingStop.AwaitingSenderEbxBeforePush
            && missingSender.StopAtRaw == GameSocketSenderBindingProjection.SenderEbxPushFileOffset
            && missingSender.ReceiverAddress is null
            && !missingSender.ReceiverArgumentHelperCallAttempted
            && missingSender.InstructionOrder.All(i =>
                i.Kind != GameSocketSenderBindingInstructionKind.PushSenderEbx)
            && Project(1, sender: 0).ReceiverArgumentHelperCallAttempted);

        var zeroAl = Project(
            0x80,
            socket: 0xFFFF_FFF0,
            al: 0,
            fallbackReturn: true,
            queueReturn: true);
        check("Zero AL selects fallback then pump-close token 2 with wrapped addresses and committed clear",
            zeroAl.Stop == GameSocketSenderBindingStop.CompletedAtCommonTail
            && zeroAl.StopAtRaw == GameSocketSenderBindingProjection.CommonTailFileOffset
            && zeroAl.QueueToken == GameSocketReceivedQueueEvent.PumpCloseRequest
            && zeroAl.FlagsAfterAlTest is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroAl.FlagsAtStop is null
            && zeroAl.FlagReadAddress == 0x24A
            && zeroAl.ReceiverAddress == 0x220
            && zeroAl.QueueAddress == 0x250
            && zeroAl.FallbackHelperArgument == 10
            && zeroAl.FlagClearAttempted
            && zeroAl.FlagClearAddress == 0x24A
            && zeroAl.FlagClearValue == 0
            && zeroAl.FlagClearCommitted
            && zeroAl.ReachedCommonTail);

        var nonzeroAl = Project(1, al: 0xFF);
        check("Nonzero AL selects sender-binding token 3, not fallback, and preserves TEST flags as history",
            nonzeroAl.Stop == GameSocketSenderBindingStop.CompletedAtCommonTail
            && nonzeroAl.StopAtRaw == GameSocketSenderBindingProjection.CommonTailFileOffset
            && nonzeroAl.QueueToken == GameSocketReceivedQueueEvent.SenderBindingSucceeded
            && nonzeroAl.FlagsAfterAlTest is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroAl.FlagsAtStop is null
            && !nonzeroAl.FallbackHelperCallAttempted
            && !nonzeroAl.SuppliedButUnusedInputs.Contains(
                GameSocketSenderBindingInput.FallbackHelperReturnedNormally)
            && nonzeroAl.IsCleanCompletion
            && nonzeroAl.FlagClearCommitted);

        var expectedZeroOrder = new[]
        {
            (GameSocketSenderBindingInstructionKind.CompareBindingFlag, 0xBD307Bu),
            (GameSocketSenderBindingInstructionKind.BranchIfBindingDisabled, 0xBD3082u),
            (GameSocketSenderBindingInstructionKind.PushSenderEbx, 0xBD3084u),
            (GameSocketSenderBindingInstructionKind.LoadReceiverArgumentAddress, 0xBD3085u),
            (GameSocketSenderBindingInstructionKind.CallReceiverArgumentHelper, 0xBD308Bu),
            (GameSocketSenderBindingInstructionKind.MoveEcxFromEsi, 0xBD3090u),
            (GameSocketSenderBindingInstructionKind.CallPredicateValueHelper, 0xBD3092u),
            (GameSocketSenderBindingInstructionKind.TestAl, 0xBD3097u),
            (GameSocketSenderBindingInstructionKind.BranchIfAlZero, 0xBD3099u),
            (GameSocketSenderBindingInstructionKind.PushFallbackHelperArgument, 0xBD309Fu),
            (GameSocketSenderBindingInstructionKind.MoveEcxFromEsi, 0xBD30A1u),
            (GameSocketSenderBindingInstructionKind.CallFallbackHelper, 0xBD30A3u),
            (GameSocketSenderBindingInstructionKind.PushPumpCloseRequestToken, 0xBD30A8u),
            (GameSocketSenderBindingInstructionKind.LoadQueueAddress, 0xBD30AAu),
            (GameSocketSenderBindingInstructionKind.CallQueueAppend, 0xBD30B0u),
            (GameSocketSenderBindingInstructionKind.ClearBindingFlag, 0xBD30B5u)
        };
        check("Game-socket bounded trace records native raw instruction order and addresses",
            zeroAl.InstructionOrder.Select(i => (i.Kind, i.FileOffset))
                .SequenceEqual(expectedZeroOrder)
            && zeroAl.SuppliedButUnusedInputs.IsEmpty);

        var missingReceiverHelperReturn = Project(1, receiverHelperReturn: null);
        var nonreturningReceiverHelper = Project(1, receiverHelperReturn: false);
        var missingPredicateHelperReturn = Project(1, predicateHelperReturn: null);
        var nonreturningPredicateHelper = Project(1, predicateHelperReturn: false);
        var missingAl = Project(1, al: null);
        check("Opaque helper return and pending TEST stops retain raw addresses and flag history",
            missingReceiverHelperReturn.Stop
                == GameSocketSenderBindingStop.AwaitingReceiverArgumentHelperReturn
            && missingReceiverHelperReturn.StopAtRaw == 0xBD3090
            && missingReceiverHelperReturn.FlagsAtStop
                == missingReceiverHelperReturn.FlagsAfterBindingFlagCompare
            && nonreturningReceiverHelper.StopAtRaw == 0xBD3090
            && nonreturningReceiverHelper.FlagsAtStop
                == nonreturningReceiverHelper.FlagsAfterBindingFlagCompare
            && missingPredicateHelperReturn.Stop
                == GameSocketSenderBindingStop.AwaitingPredicateValueHelperReturn
            && missingPredicateHelperReturn.StopAtRaw == 0xBD3097
            && missingPredicateHelperReturn.FlagsAtStop is null
            && nonreturningPredicateHelper.StopAtRaw == 0xBD3097
            && nonreturningPredicateHelper.FlagsAtStop is null
            && missingAl.Stop == GameSocketSenderBindingStop.AwaitingAlForTest
            && missingAl.StopAtRaw == GameSocketSenderBindingProjection.AlTestFileOffset
            && missingAl.AlTestAttempted
            && missingAl.FlagsAfterAlTest is null
            && missingAl.FlagsAtStop is null);

        var missingFallback = Project(1, al: 0, fallbackReturn: null);
        var nonreturningFallback = Project(1, al: 0, fallbackReturn: false);
        var normalFallbackMissingQueue = Project(1, al: 0, fallbackReturn: true, queueReturn: null);
        var missingQueue = Project(1, queueReturn: null);
        var nonreturningQueue = Project(1, queueReturn: false);
        check("Pending and nonreturning opaque calls preserve TEST flags; normal returns invalidate them",
            missingFallback.Stop == GameSocketSenderBindingStop.AwaitingFallbackHelperReturn
            && missingFallback.StopAtRaw == 0xBD30A8
            && missingFallback.FlagsAtStop == missingFallback.FlagsAfterAlTest
            && nonreturningFallback.Stop == GameSocketSenderBindingStop.FallbackHelperDidNotReturn
            && nonreturningFallback.FlagsAtStop == nonreturningFallback.FlagsAfterAlTest
            && normalFallbackMissingQueue.Stop
                == GameSocketSenderBindingStop.AwaitingQueueAppendReturn
            && normalFallbackMissingQueue.FlagsAtStop is null
            && missingQueue.Stop == GameSocketSenderBindingStop.AwaitingQueueAppendReturn
            && missingQueue.StopAtRaw == 0xBD30B5
            && missingQueue.FlagsAtStop == missingQueue.FlagsAfterAlTest
            && nonreturningQueue.Stop == GameSocketSenderBindingStop.QueueAppendDidNotReturn
            && nonreturningQueue.FlagsAtStop == nonreturningQueue.FlagsAfterAlTest
            && !missingQueue.FlagClearAttempted
            && !nonreturningQueue.FlagClearCommitted);

        var dirtyCompleted = Project(1, al: 0xFF, fallbackReturn: true);
        check("Supplied unused branch spoils clean completion without changing the bounded tail",
            dirtyCompleted.ReachedCommonTail
            && dirtyCompleted.StopAtRaw == GameSocketSenderBindingProjection.CommonTailFileOffset
            && dirtyCompleted.HasSuppliedButUnusedInputs
            && !dirtyCompleted.IsCleanCompletion
            && dirtyCompleted.SuppliedButUnusedInputs.Contains(
                GameSocketSenderBindingInput.FallbackHelperReturnedNormally));

        check("Game-socket projection records call attempts without executing opaque helpers",
            !zeroAl.OpaqueHelpersExecuted
            && zeroAl.ReceiverArgumentHelperCallAttempted
            && zeroAl.PredicateValueHelperCallAttempted
            && zeroAl.FallbackHelperCallAttempted
            && zeroAl.QueueAppendCallAttempted);

        check("Game-socket sender-binding projection pins native offsets and existing queue tokens",
            GameSocketSenderBindingProjection.BindingFlagCompareFileOffset == 0xBD307B
            && GameSocketSenderBindingProjection.BindingFlagBranchFileOffset == 0xBD3082
            && GameSocketSenderBindingProjection.SenderEbxPushFileOffset == 0xBD3084
            && GameSocketSenderBindingProjection.ReceiverArgumentHelperCallFileOffset == 0xBD308B
            && GameSocketSenderBindingProjection.PredicateHelperCallFileOffset == 0xBD3092
            && GameSocketSenderBindingProjection.AlTestFileOffset == 0xBD3097
            && GameSocketSenderBindingProjection.FallbackHelperCallFileOffset == 0xBD30A3
            && GameSocketSenderBindingProjection.QueueAppendHelperCallFileOffset == 0xBD30B0
            && GameSocketSenderBindingProjection.BindingFlagClearFileOffset == 0xBD30B5
            && GameSocketSenderBindingProjection.CommonTailFileOffset == 0xBD30BC
            && GameSocketSenderBindingProjection.BindingFlagOffset == 0x25A
            && GameSocketSenderBindingProjection.ReceiverOffset == 0x230
            && GameSocketSenderBindingProjection.QueueOffset
                == GameSocketQueuedReceiveNativeLayout.ReceivedQueueOffset
            && (uint)GameSocketReceivedQueueEvent.PumpCloseRequest == 2
            && (uint)GameSocketReceivedQueueEvent.SenderBindingSucceeded == 3);
    }
}
