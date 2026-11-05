using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveZeroResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample hash, raw-to-VA mapping, exact bytes, and branch/call targets are pinned",
            UnreliableReceiveZeroResultProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveZeroResultProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveZeroResultProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveZeroResultProjection.HandoffStartRawFileOffset == 0x00BE5B78
            && UnreliableReceiveZeroResultProjection.HandoffStartVirtualAddress == 0x00FE6778
            && UnreliableReceiveZeroResultProjection.HandoffEndRawFileOffsetExclusive == 0x00BE5B9E
            && UnreliableReceiveZeroResultProjection.HandoffEndVirtualAddressExclusive == 0x00FE679E
            && UnreliableReceiveZeroResultProjection.RawFileOffsetToVirtualAddress(0x00BE5B78)
                == 0x00FE6778
            && UnreliableReceiveZeroResultProjection.RawFileOffsetToVirtualAddress(0x00BE5B9E)
                == 0x00FE679E
            && UnreliableReceiveZeroResultProjection.CapturedHandoffBytes
                == "85 F6 75 30 8B 07 B9 68 EA C5 01 FF 77 04 8B 40 04 "
                    + "85 C0 0F 45 C8 51 68 7C 8E BB 01 6A 04 FF 77 18 "
                    + "E8 B2 D3 FF FF"
            && UnreliableReceiveZeroResultProjection.CapturedHandoffBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x26
            && InstructionEvidenceMatches());

        check("The prior normal-return gate and ESI test prevent later captures from being consumed",
            VerifyGateAndBranchPaths());

        check("The zero pointer is still dereferenced at offset four and ECX retains its literal for a zero slot",
            VerifyZeroSlotAndPushOrder());

        check("A nonzero loaded-slot value replaces ECX and all five pushes retain instruction order",
            VerifyNonzeroSlotAndCompleteHandoff());

        check("Missing captures stop at their read or push, preserving only pushes already executed",
            VerifyMissingCaptureStops());

        check("Receiver and loaded-slot effective addresses wrap as 32-bit DWORD addresses",
            UnreliableReceiveZeroResultProjection.ReceiverDwordAtOffset00Address(uint.MaxValue) == uint.MaxValue
            && UnreliableReceiveZeroResultProjection.ReceiverDwordAtOffset04Address(0xFFFFFFFC) == 0
            && UnreliableReceiveZeroResultProjection.ReceiverDwordAtOffset18Address(0xFFFFFFF0) == 8
            && UnreliableReceiveZeroResultProjection.LoadedSlotDwordAtOffset04Address(uint.MaxValue) == 3);
    }

    private static bool VerifyGateAndBranchPaths()
    {
        var missingPriorGate = UnreliableReceiveZeroResultProjection.Project(
            new(1, 0x1000, null, 0x2000, 0x3000, 0x4000, 0x5000, true));
        var priorDidNotReturn = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, false, 0x2000, 0x3000, 0x4000, 0x5000, true));
        var nonzeroResult = UnreliableReceiveZeroResultProjection.Project(
            new(1, 0x1000, true, 0, 0, 0, 0, true));
        var zeroResult = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, null, null, null, null, null));

        return missingPriorGate.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingPriorRecycleHelperReturnCapture
            && missingPriorGate.EsiIsZero is null
            && missingPriorGate.UnreachedReceiverDwordAtOffset00CaptureProvided
            && missingPriorGate.UnreachedReceiverDwordAtOffset04CaptureProvided
            && missingPriorGate.UnreachedLoadedSlotDwordAtOffset04CaptureProvided
            && missingPriorGate.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingPriorGate.UnreachedHandoffReturnCaptureProvided
            && missingPriorGate.StopAtRawFileOffsetExclusive == 0x00BE5B78
            && priorDidNotReturn.Outcome
                == UnreliableReceiveZeroResultOutcome.PriorRecycleHelperDidNotReturnNormally
            && !priorDidNotReturn.HandoffCallReached
            && priorDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE5B78
            && nonzeroResult.Outcome == UnreliableReceiveZeroResultOutcome.NonzeroEsiBranch
            && nonzeroResult.EsiIsZero == false
            && nonzeroResult.NonzeroEsiBranchTaken == true
            && nonzeroResult.SelectedDestinationRawFileOffset == 0x00BE5BAC
            && nonzeroResult.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && !nonzeroResult.ReceiverDwordAtOffset00Read
            && nonzeroResult.UnreachedReceiverDwordAtOffset00CaptureProvided
            && nonzeroResult.UnreachedReceiverDwordAtOffset04CaptureProvided
            && nonzeroResult.UnreachedLoadedSlotDwordAtOffset04CaptureProvided
            && nonzeroResult.UnreachedReceiverDwordAtOffset18CaptureProvided
            && nonzeroResult.UnreachedHandoffReturnCaptureProvided
            && nonzeroResult.ArgumentsPushedInInstructionOrder.IsEmpty
            && !nonzeroResult.CanComposeCompletedHandoff
            && zeroResult.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingReceiverOffset00Capture
            && zeroResult.EsiIsZero == true
            && zeroResult.NonzeroEsiBranchTaken == false
            && zeroResult.StopAtRawFileOffsetExclusive == 0x00BE5B7C;
    }

    private static bool VerifyZeroSlotAndPushOrder()
    {
        var plan = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0xFFFFFFFC, true, 0, 0x11223344, 0, 0x55667788, true));

        return plan.Outcome == UnreliableReceiveZeroResultOutcome.HandoffReturnedToExclusiveBoundary
            && plan.ReceiverDwordAtOffset00Address == 0xFFFFFFFC
            && plan.ReceiverDwordAtOffset04Address == 0
            && plan.LoadedSlotDwordAtOffset04Address == 4
            && plan.SelectedEcxValue == 0x01C5EA68
            && plan.ReceiverDwordAtOffset00Read
            && plan.ReceiverDwordAtOffset04Read
            && plan.LoadedSlotDwordAtOffset04Read
            && plan.ReceiverDwordAtOffset18Read
            && plan.ArgumentsPushedInInstructionOrder.SequenceEqual(
                new uint[] { 0x11223344, 0x01C5EA68, 0x01BB8E7C, 4, 0x55667788 })
            && UnreliableReceiveZeroResultProjection.PushKindsInInstructionOrder.SequenceEqual(
                new[]
                {
                    UnreliableReceiveZeroResultPushKind.ReceiverDwordAtOffset04,
                    UnreliableReceiveZeroResultPushKind.SelectedEcx,
                    UnreliableReceiveZeroResultPushKind.LiteralArgument,
                    UnreliableReceiveZeroResultPushKind.FourArgument,
                    UnreliableReceiveZeroResultPushKind.ReceiverDwordAtOffset18
                })
            && plan.HandoffCallReached
            && plan.HandoffHelperReturnedNormally == true
            && plan.HandoffReturnBoundaryReached == true
            && plan.CanComposeCompletedHandoff
            && plan.SelectedDestinationRawFileOffset is null
            && plan.StopAtRawFileOffsetExclusive == 0x00BE5B9E;
    }

    private static bool VerifyNonzeroSlotAndCompleteHandoff()
    {
        var plan = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, 0xAABBCCDD, 0x10203040, 0x55667788, true));
        var nonreturning = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, 0xAABBCCDD, 0x10203040, 0x55667788, false));

        return plan.Outcome == UnreliableReceiveZeroResultOutcome.HandoffReturnedToExclusiveBoundary
            && plan.LoadedSlotDwordAtOffset04Address == 0x2004
            && plan.SelectedEcxValue == 0x10203040
            && plan.ArgumentsPushedInInstructionOrder.SequenceEqual(
                new uint[] { 0xAABBCCDD, 0x10203040, 0x01BB8E7C, 4, 0x55667788 })
            && plan.HandoffCallReached
            && plan.HandoffHelperReturnedNormally == true
            && plan.HandoffReturnBoundaryReached == true
            && plan.CanComposeCompletedHandoff
            && nonreturning.Outcome
                == UnreliableReceiveZeroResultOutcome.HandoffHelperDidNotReturnNormally
            && nonreturning.HandoffCallReached
            && nonreturning.HandoffHelperReturnedNormally == false
            && nonreturning.HandoffReturnBoundaryReached == false
            && !nonreturning.CanComposeCompletedHandoff
            && nonreturning.ArgumentsPushedInInstructionOrder.SequenceEqual(
                plan.ArgumentsPushedInInstructionOrder)
            && nonreturning.StopAtRawFileOffsetExclusive == 0x00BE5B99
            && UnreliableReceiveZeroResultProjection.RelativeCallTarget(
                0x00BE5B99,
                unchecked((int)0xFFFFD3B2)) == 0x00BE2F50
            && UnreliableReceiveZeroResultProjection.RawFileOffsetToVirtualAddress(0x00BE2F50)
                == 0x00FE3B50
            && ShortBranchTarget(0x00BE5B7A, 0x30) == 0x00BE5BAC;
    }

    private static bool VerifyMissingCaptureStops()
    {
        var missingReceiver00 = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, null, 0x22, 0x33, 0x44, true));
        var missingReceiver04 = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, null, 0x33, 0x44, true));
        var missingLoadedSlot = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, 0x11, null, 0x44, true));
        var missingReceiver18 = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, 0x11, 0, null, true));
        var missingReturn = UnreliableReceiveZeroResultProjection.Project(
            new(0, 0x1000, true, 0x2000, 0x11, 0x22, 0x33, null));

        return missingReceiver00.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingReceiverOffset00CaptureWithUnreachedLaterCaptures
            && !missingReceiver00.ReceiverDwordAtOffset00Read
            && missingReceiver00.ArgumentsPushedInInstructionOrder.IsEmpty
            && missingReceiver00.UnreachedReceiverDwordAtOffset04CaptureProvided
            && missingReceiver00.UnreachedLoadedSlotDwordAtOffset04CaptureProvided
            && missingReceiver00.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingReceiver00.UnreachedHandoffReturnCaptureProvided
            && missingReceiver00.StopAtRawFileOffsetExclusive == 0x00BE5B7C
            && missingReceiver04.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingReceiverOffset04CaptureWithUnreachedLaterCaptures
            && missingReceiver04.ReceiverDwordAtOffset00Read
            && !missingReceiver04.ReceiverDwordAtOffset04Read
            && missingReceiver04.ArgumentsPushedInInstructionOrder.IsEmpty
            && missingReceiver04.UnreachedLoadedSlotDwordAtOffset04CaptureProvided
            && missingReceiver04.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingReceiver04.UnreachedHandoffReturnCaptureProvided
            && missingReceiver04.StopAtRawFileOffsetExclusive == 0x00BE5B83
            && missingLoadedSlot.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingLoadedSlotOffset04CaptureWithUnreachedLaterCaptures
            && missingLoadedSlot.ReceiverDwordAtOffset04Read
            && !missingLoadedSlot.LoadedSlotDwordAtOffset04Read
            && missingLoadedSlot.ArgumentsPushedInInstructionOrder.SequenceEqual(new uint[] { 0x11 })
            && missingLoadedSlot.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingLoadedSlot.UnreachedHandoffReturnCaptureProvided
            && missingLoadedSlot.StopAtRawFileOffsetExclusive == 0x00BE5B86
            && missingReceiver18.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingReceiverOffset18CaptureWithUnreachedHandoffReturnCapture
            && missingReceiver18.SelectedEcxValue == 0x01C5EA68
            && missingReceiver18.ArgumentsPushedInInstructionOrder.SequenceEqual(
                new uint[] { 0x11, 0x01C5EA68, 0x01BB8E7C, 4 })
            && missingReceiver18.UnreachedHandoffReturnCaptureProvided
            && missingReceiver18.StopAtRawFileOffsetExclusive == 0x00BE5B96
            && missingReturn.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingHandoffHelperReturnCapture
            && missingReturn.HandoffCallReached
            && missingReturn.HandoffHelperReturnedNormally is null
            && missingReturn.HandoffReturnBoundaryReached is null
            && missingReturn.ArgumentsPushedInInstructionOrder.SequenceEqual(
                new uint[] { 0x11, 0x22, 0x01BB8E7C, 4, 0x33 })
            && !missingReturn.CanComposeCompletedHandoff
            && missingReturn.StopAtRawFileOffsetExclusive == 0x00BE5B99;
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5B78, 0x00FE6778, "85 F6 75 30", 0x00BE5BAC, 0x00FE67AC),
            (0x00BE5B7C, 0x00FE677C, "8B 07", null, null),
            (0x00BE5B7E, 0x00FE677E, "B9 68 EA C5 01 FF 77 04", null, null),
            (0x00BE5B86, 0x00FE6786, "8B 40 04 85 C0 0F 45 C8", null, null),
            (0x00BE5B8E, 0x00FE678E, "51 68 7C 8E BB 01 6A 04 FF 77 18", null, null),
            (0x00BE5B99, 0x00FE6799, "E8 B2 D3 FF FF", 0x00BE2F50, 0x00FE3B50)
        };
        return UnreliableReceiveZeroResultProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
