using Firefall.Reconstruction.Startup;

internal static class GameSocketReceivePacketCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceivePacketCallPreparationProjectionResult Project(
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
            uint? freshCount = 0x5566_7788) =>
            GameSocketReceivePacketCallPreparationProjection.Project(new(
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
                freshCount));

        var direct = Project(
            receiver: 0xABCD_1234,
            initial: 5,
            reload: 0x8765_4321,
            helperEax: 0,
            postHelperEsp: 0x1000,
            freshCount: 0x5566_7788);
        check("Normal sender-helper return pushes independent EAX and fresh count before the bounded fixed call",
            direct.ParentProjection.IsCleanCompletion
            && direct.ParentProjection.StopAtRawFileOffset == 0xBD2F19
            && direct.ParentHandoffAdmitted
            && direct.Stop
                == GameSocketReceivePacketCallPreparationStop.FixedCallBoundaryAtF25
            && direct.StopAtRawFileOffset == 0xBD2F25
            && direct.ReachedBoundedEndpoint
            && direct.IsCleanCompletion
            && direct.SenderResultStackAddress == 0xFFC
            && direct.FreshCountReadAddress == 0x1010
            && direct.FreshCountStackAddress == 0xFF8
            && direct.StackPointerAtFixedCall == 0xFF8
            && direct.InheritedReceiver == 0xABCD_1234
            && direct.FixedCallStackArguments.SequenceEqual(
                new uint?[] { 0x5566_7788, 0 })
            && direct.InstructionOrder.Select(instruction => instruction.Kind)
                .SequenceEqual(new[]
                {
                    GameSocketReceivePacketCallPreparationInstructionKind.PushSenderHelperResult,
                    GameSocketReceivePacketCallPreparationInstructionKind.PushFreshCount,
                    GameSocketReceivePacketCallPreparationInstructionKind.MoveInheritedReceiverToEcx,
                    GameSocketReceivePacketCallPreparationInstructionKind.CallFixedTarget
                })
            && direct.InstructionOrder[^1] is
            {
                RawFileOffset: 0xBD2F25,
                TargetRawFileOffset: 0xBD3030,
                TargetAddress: 0xFD3C30,
                OperandValue: 0xABCD1234
            }
            && direct.Calls.Single(call =>
                call.Kind == GameSocketReceivePacketCallPreparationCallKind.SenderHelper) is
            {
                CallSiteRawFileOffset: 0xBD2F19,
                TargetRawFileOffset: 0xBE3A00,
                TargetAddress: 0xFE4600,
                Attempted: true,
                ReturnedNormally: true,
                ReturnedEax: 0,
                PostCallEsp: 0x1000
            }
            && direct.Calls.Last() is
            {
                Kind: GameSocketReceivePacketCallPreparationCallKind.FixedTargetBoundary,
                CallSiteRawFileOffset: 0xBD2F25,
                TargetRawFileOffset: 0xBD3030,
                TargetAddress: 0xFD3C30,
                CallSiteReached: true,
                Attempted: false,
                EcxArgument: 0xABCD1234
            }
            && direct.MemoryEffects[0] is
            {
                Kind: GameSocketReceivePacketCallPreparationMemoryEffectKind.SenderHelperResultStackWrite,
                RawFileOffset: 0xBD2F1E,
                Address: 0xFFC,
                Value: 0,
                Committed: true
            }
            && direct.MemoryEffects[1] is
            {
                Kind: GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountRead,
                RawFileOffset: 0xBD2F1F,
                Address: 0x1010,
                Value: 0x55667788,
                Attempted: true,
                Captured: true,
                Committed: false
            }
            && direct.MemoryEffects[2] is
            {
                Kind: GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountStackWrite,
                RawFileOffset: 0xBD2F1F,
                Address: 0xFF8,
                Value: 0x55667788,
                Committed: true
            }
            && direct.AnyMemoryWriteCommitted
            && direct.FlagsAtStop is null
            && !direct.PacketReceiveCallExecuted
            && !direct.PacketRepostModeled
            && !direct.ReturnInstructionModeled
            && !direct.OpaqueCallsExecuted);

        var helperPending = Project(helperReturned: null);
        var helperDidNotReturn = Project(helperReturned: false);
        check("Missing or nonnormal helper status stops at F19 and preserves the parent's ADD flags",
            helperPending.Stop
                == GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperReturnAtF19
            && helperPending.StopAtRawFileOffset == 0xBD2F19
            && helperPending.FlagsAtStop == helperPending.ParentProjection.FlagsAtStop
            && helperPending.CaptureStates.SenderHelperReturnedNormally
                == GameSocketReceivePacketCallPreparationCaptureState.Missing
            && helperPending.CaptureStates.SenderHelperReturnedEax
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && helperPending.CaptureStates.FreshPostHelperEsp
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && helperPending.CaptureStates.FreshCount
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && helperPending.MemoryEffects.IsEmpty
            && helperPending.Calls.Last().ReturnedNormally is null
            && helperPending.Calls.Last().ReturnedEax is null
            && helperPending.Calls.Last().PostCallEsp is null
            && helperDidNotReturn.Stop
                == GameSocketReceivePacketCallPreparationStop.SenderHelperDidNotReturnNormally
            && helperDidNotReturn.StopAtRawFileOffset == 0xBD2F19
            && helperDidNotReturn.FlagsAtStop
                == helperDidNotReturn.ParentProjection.FlagsAtStop
            && helperDidNotReturn.CaptureStates.SenderHelperReturnedNormally
                == GameSocketReceivePacketCallPreparationCaptureState.Captured
            && helperDidNotReturn.CaptureStates.SenderHelperReturnedEax
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && helperDidNotReturn.MemoryEffects.IsEmpty);

        var missingEax = Project(helperEax: null);
        var missingEsp = Project(postHelperEsp: null);
        check("A normal opaque return invalidates flags even when EAX or post-helper ESP is missing at F1E",
            missingEax.Stop
                == GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperEaxAtF1E
            && missingEax.StopAtRawFileOffset == 0xBD2F1E
            && missingEax.FlagsAtStop is null
            && missingEax.CaptureStates.SenderHelperReturnedEax
                == GameSocketReceivePacketCallPreparationCaptureState.Missing
            && missingEax.CaptureStates.FreshPostHelperEsp
                == GameSocketReceivePacketCallPreparationCaptureState.Captured
            && missingEax.MemoryEffects.Single() is
            {
                Kind: GameSocketReceivePacketCallPreparationMemoryEffectKind.SenderHelperResultStackWrite,
                Address: 0x1FFC,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingEsp.Stop
                == GameSocketReceivePacketCallPreparationStop.AwaitingPostHelperEspAtF1E
            && missingEsp.StopAtRawFileOffset == 0xBD2F1E
            && missingEsp.FlagsAtStop is null
            && missingEsp.CaptureStates.SenderHelperReturnedEax
                == GameSocketReceivePacketCallPreparationCaptureState.Captured
            && missingEsp.CaptureStates.FreshPostHelperEsp
                == GameSocketReceivePacketCallPreparationCaptureState.Missing
            && missingEsp.MemoryEffects.Single() is
            {
                Address: null,
                Value: 0x11223344,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && !missingEax.FreshCountReadAttempted
            && !missingEsp.FreshCountReadAttempted);

        var missingCount = Project(freshCount: null);
        check("A missing fresh count stops at F1F after committing the first PUSH and attempting its independent read",
            missingCount.Stop
                == GameSocketReceivePacketCallPreparationStop.AwaitingFreshCountAtF1F
            && missingCount.StopAtRawFileOffset == 0xBD2F1F
            && missingCount.SenderResultStackAddress == 0x1FFC
            && missingCount.FreshCountReadAddress == 0x2010
            && missingCount.FreshCountReadAttempted
            && missingCount.MemoryEffects[0].Committed
            && missingCount.MemoryEffects[1] is
            {
                Kind: GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountRead,
                Address: 0x2010,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingCount.CaptureStates.FreshCount
                == GameSocketReceivePacketCallPreparationCaptureState.Missing
            && !missingCount.FixedCallSiteReached
            && !missingCount.IsCleanCompletion);

        var wrappedCountAddress = Project(
            helperEax: 0xAABB_CCDD,
            postHelperEsp: 0xFFFF_FFF8,
            freshCount: 0);
        check("Fresh count uses wrapped post-ESP plus ten and zero remains a valid captured value",
            wrappedCountAddress.Stop
                == GameSocketReceivePacketCallPreparationStop.FixedCallBoundaryAtF25
            && wrappedCountAddress.SenderResultStackAddress == 0xFFFF_FFF4
            && wrappedCountAddress.FreshCountReadAddress == 8
            && wrappedCountAddress.FreshCountStackAddress == 0xFFFF_FFF0
            && wrappedCountAddress.MemoryEffects[1].Value == 0
            && wrappedCountAddress.CaptureStates.FreshCount
                == GameSocketReceivePacketCallPreparationCaptureState.Captured);

        var dirtyParent = Project(initial: 5, diagnosticReturned: true, postDiagnosticEsp: 0x7000);
        var parentError996 = Project(
            parentResult: 0,
            accessorReturned: true,
            error: 996);
        var missingParentResult = Project(parentResult: null);
        check("Dirty success, error 996 and missing parent results keep their own bounds without child reads or calls",
            dirtyParent.Stop
                == GameSocketReceivePacketCallPreparationStop.ParentSenderHelperBoundaryWasDirty
            && dirtyParent.StopAtRawFileOffset == 0xBD2F19
            && dirtyParent.ParentProjection.ReachedBoundedEndpoint
            && !dirtyParent.ParentHandoffAdmitted
            && dirtyParent.MemoryEffects.IsEmpty
            && dirtyParent.Calls.All(call =>
                call.Kind != GameSocketReceivePacketCallPreparationCallKind.FixedTargetBoundary)
            && dirtyParent.Calls.Single(call =>
                call.Kind == GameSocketReceivePacketCallPreparationCallKind.SenderHelper)
                .Attempted == false
            && dirtyParent.CaptureStates.SenderHelperReturnedNormally
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && parentError996.Stop
                == GameSocketReceivePacketCallPreparationStop.ParentDidNotReachSenderHelper
            && parentError996.StopAtRawFileOffset == 0xBD3001
            && parentError996.ExactBoundedCompletion
            && parentError996.FlagsAtStop == parentError996.ParentProjection.FlagsAtStop
            && parentError996.MemoryEffects.IsEmpty
            && !parentError996.FreshCountReadAttempted
            && parentError996.CaptureStates.FreshCount
                == GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused
            && missingParentResult.Stop
                == GameSocketReceivePacketCallPreparationStop.ParentDidNotReachSenderHelper
            && missingParentResult.StopAtRawFileOffset == 0xBD2ED7
            && missingParentResult.FlagsAtStop == missingParentResult.ParentProjection.FlagsAtStop
            && !missingParentResult.ExactBoundedCompletion
            && missingParentResult.MemoryEffects.IsEmpty
            && missingParentResult.Calls.IsEmpty);

        check("The ordered trace retains parent effects before child effects and anchors the verified window",
            direct.OrderedMemoryEffects.Take(direct.ParentProjection.OrderedMemoryEffects.Length)
                .Select(step =>
                    (step.ResultGateMemoryEffect, step.SenderPreparationMemoryEffect))
                .SequenceEqual(direct.ParentProjection.OrderedMemoryEffects.Select(step =>
                    (step.ParentMemoryEffect, step.PreparationMemoryEffect)))
            && direct.OrderedMemoryEffects.TakeLast(direct.MemoryEffects.Length)
                .Select(step => step.PacketCallPreparationMemoryEffect)
                .SequenceEqual(direct.MemoryEffects.Select(effect =>
                    (GameSocketReceivePacketCallPreparationMemoryEffect?)effect))
            && GameSocketReceivePacketCallPreparationProjection.MethodRawFileOffset == 0xBD2F19
            && GameSocketReceivePacketCallPreparationProjection.SenderResultPushRawFileOffset
                == 0xBD2F1E
            && GameSocketReceivePacketCallPreparationProjection.FreshCountPushRawFileOffset
                == 0xBD2F1F
            && GameSocketReceivePacketCallPreparationProjection.InheritedReceiverMoveRawFileOffset
                == 0xBD2F23
            && GameSocketReceivePacketCallPreparationProjection.FixedCallRawFileOffset == 0xBD2F25
            && GameSocketReceivePacketCallPreparationProjection.FixedCallTargetRawFileOffset
                == 0xBD3030
            && GameSocketReceivePacketCallPreparationProjection.FixedCallTargetVirtualAddress
                == 0xFD3C30
            && GameSocketReceivePacketCallPreparationProjection.SenderHelperTargetRawFileOffset
                == 0xBE3A00
            && GameSocketReceivePacketCallPreparationProjection.SenderHelperTargetVirtualAddress
                == 0xFE4600
            && GameSocketReceivePacketCallPreparationProjection.SenderHelperCallBytes
                .SequenceEqual(new byte[] { 0xE8, 0xE2, 0x0A, 0x01, 0x00 })
            && GameSocketReceivePacketCallPreparationProjection.SenderResultPushBytes
                .SequenceEqual(new byte[] { 0x50 })
            && GameSocketReceivePacketCallPreparationProjection.FreshCountPushBytes
                .SequenceEqual(new byte[] { 0xFF, 0x74, 0x24, 0x14 })
            && GameSocketReceivePacketCallPreparationProjection.InheritedReceiverMoveBytes
                .SequenceEqual(new byte[] { 0x8B, 0xCE })
            && GameSocketReceivePacketCallPreparationProjection.FixedCallBytes
                .SequenceEqual(new byte[] { 0xE8, 0x06, 0x01, 0x00, 0x00 }));
    }
}
