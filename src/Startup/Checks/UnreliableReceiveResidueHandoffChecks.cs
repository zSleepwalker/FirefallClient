using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveResidueHandoffChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample, raw-to-VA mapping, exact window bytes, and branch/call targets are pinned",
            UnreliableReceiveResidueHandoffProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveResidueHandoffProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveResidueHandoffProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && UnreliableReceiveResidueHandoffProjection.HandoffStartRawFileOffset == 0x00BE5AC2
            && UnreliableReceiveResidueHandoffProjection.HandoffStartVirtualAddress == 0x00FE66C2
            && UnreliableReceiveResidueHandoffProjection.HandoffEndRawFileOffsetExclusive == 0x00BE5AE4
            && UnreliableReceiveResidueHandoffProjection.HandoffEndVirtualAddressExclusive == 0x00FE66E4
            && UnreliableReceiveResidueHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE5AC2) == 0x00FE66C2
            && UnreliableReceiveResidueHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE5AE4) == 0x00FE66E4
            && UnreliableReceiveResidueHandoffProjection.CapturedHandoffBytes
                == "2B CA 0F B7 C1 83 F8 01 76 24 FF 77 04 8B F0 8D 46 FF "
                    + "50 68 04 8D BB 01 6A 00 FF 77 18 E8 6C D4 FF FF"
            && UnreliableReceiveResidueHandoffProjection.CapturedHandoffBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x22
            && InstructionEvidenceMatches());

        check("The prior masked entry gate is enforced and JBE can occur only at entry residue 0xFFFF",
            UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0x255A, out var minimumEntry)
            && minimumEntry.EntryResidue == 0x8000
            && minimumEntry.EntryDifference32 == 0x00008000
            && !UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0xA55A, out _)
            && !UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0xA559, out _)
            && !UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0x255B, out _)
            && VerifyAllEntriesAndEarlyBranches());

        check("The five pushes preserve instruction order and reverse into callee stack order",
            UnreliableReceiveResidueHandoffProjection.ArgumentCount == 5
            && UnreliableReceiveResidueHandoffProjection.PushKindsInInstructionOrder
                .SequenceEqual(new[]
                {
                    UnreliableReceiveResidueHandoffPushKind.ReceiverDwordAtOffset04,
                    UnreliableReceiveResidueHandoffPushKind.ResidueMinusOne,
                    UnreliableReceiveResidueHandoffPushKind.LiteralArgument,
                    UnreliableReceiveResidueHandoffPushKind.ZeroArgument,
                    UnreliableReceiveResidueHandoffPushKind.ReceiverDwordAtOffset18
                })
            && UnreliableReceiveResidueHandoffProjection.RelativeCallTarget(
                0x00BE5ADF,
                unchecked((int)0xFFFFD46C)) == 0x00BE2F50
            && UnreliableReceiveResidueHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE2F50)
                == 0x00FE3B50
            && ShortBranchTarget(0x00BE5ACA, 0x24) == 0x00BE5AF0
            && UnreliableReceiveResidueHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE5AF0)
                == 0x00FE66F0);

        UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0x255A, out var callEntry);
        var callCapture = new UnreliableReceiveResidueHandoffCapture(
            0xFFFFFFFC,
            0,
            0x11223344,
            true);
        UnreliableReceiveResidueHandoffPlan completedCall =
            UnreliableReceiveResidueHandoffProjection.Project(callEntry, callCapture);
        UnreliableReceiveResidueHandoffPlan nonReturningCall =
            UnreliableReceiveResidueHandoffProjection.Project(
                callEntry,
                callCapture with { HelperReturnedNormally = false });
        check("The captured zero DWORD remains present and helper status stops at the raw 0xBE5AE4 boundary",
            completedCall.Outcome
                == UnreliableReceiveResidueHandoffOutcome.HelperReturnedToExclusiveBoundary
            && completedCall.ReceiverDwordAtOffset04Address == 0
            && completedCall.ReceiverDwordAtOffset18Address == 0x14
            && completedCall.HandoffDifference32 == 0xFFFF8000
            && completedCall.HandoffResidue == 0x8000
            && completedCall.ResidueMinusOne == 0x7FFF
            && completedCall.ReceiverDwordAtOffset04Read
            && completedCall.ReceiverDwordAtOffset18Read
            && completedCall.ArgumentsPushedInInstructionOrder
                .SequenceEqual(new uint[] { 0, 0x7FFF, 0x01BB8D04, 0, 0x11223344 })
            && completedCall.CalleeStackArguments
                .SequenceEqual(new uint[] { 0x11223344, 0, 0x01BB8D04, 0x7FFF, 0 })
            && completedCall.HelperCallReached
            && completedCall.HelperReturnedNormally == true
            && completedCall.HelperReturnBoundaryReached == true
            && completedCall.SelectedDestinationRawFileOffset is null
            && completedCall.StopAtRawFileOffsetExclusive == 0x00BE5AE4
            && nonReturningCall.Outcome
                == UnreliableReceiveResidueHandoffOutcome.HelperDidNotReturnNormally
            && nonReturningCall.HelperCallReached
            && nonReturningCall.HelperReturnedNormally == false
            && nonReturningCall.HelperReturnBoundaryReached == false
            && nonReturningCall.SelectedDestinationRawFileOffset is null
            && nonReturningCall.StopAtRawFileOffsetExclusive == 0x00BE5AE4);

        UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0xA55B, out var earlyEntry);
        UnreliableReceiveResidueHandoffPlan earlyBranch =
            UnreliableReceiveResidueHandoffProjection.Project(
                earlyEntry,
                new(0xFFFFFFFC, 0, 0, true));
        UnreliableReceiveResidueHandoffPlan missingFirstPushCapture =
            UnreliableReceiveResidueHandoffProjection.Project(
                callEntry,
                new(0x1000, null, 0, true));
        UnreliableReceiveResidueHandoffPlan missingLastPushCapture =
            UnreliableReceiveResidueHandoffProjection.Project(
                callEntry,
                new(0x1000, 0, null, true));
        UnreliableReceiveResidueHandoffPlan missingReturnStatus =
            UnreliableReceiveResidueHandoffProjection.Project(
                callEntry,
                new(0x1000, 0, 0, null));
        check("Missing and not-reached captures stay distinct, including later push/status values",
            earlyBranch.Outcome == UnreliableReceiveResidueHandoffOutcome.EarlyJbeTaken
            && earlyBranch.HandoffResidue == 1
            && earlyBranch.EarlyJbeTaken == true
            && !earlyBranch.ReceiverDwordAtOffset04Read
            && !earlyBranch.ReceiverDwordAtOffset18Read
            && earlyBranch.UnreachedReceiverDwordAtOffset04CaptureProvided
            && !earlyBranch.HelperCallReached
            && earlyBranch.UnreachedReceiverDwordAtOffset18CaptureProvided
            && earlyBranch.UnreachedHelperReturnCaptureProvided
            && earlyBranch.ArgumentsPushedInInstructionOrder.IsEmpty
            && earlyBranch.SelectedDestinationRawFileOffset == 0x00BE5AF0
            && earlyBranch.StopAtRawFileOffsetExclusive == 0x00BE5AF0
            && missingFirstPushCapture.Outcome
                == UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset04CaptureWithUnreachedLaterCaptures
            && !missingFirstPushCapture.ReceiverDwordAtOffset04Read
            && !missingFirstPushCapture.ReceiverDwordAtOffset18Read
            && missingFirstPushCapture.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingFirstPushCapture.UnreachedHelperReturnCaptureProvided
            && missingFirstPushCapture.StopAtRawFileOffsetExclusive == 0x00BE5ACC
            && missingLastPushCapture.Outcome
                == UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset18CaptureWithUnreachedHelperReturnCapture
            && missingLastPushCapture.ArgumentsPushedInInstructionOrder
                .SequenceEqual(new uint[] { 0, 0x7FFF, 0x01BB8D04, 0 })
            && missingLastPushCapture.UnreachedHelperReturnCaptureProvided
            && missingLastPushCapture.StopAtRawFileOffsetExclusive == 0x00BE5ADC
            && missingReturnStatus.Outcome
                == UnreliableReceiveResidueHandoffOutcome.MissingHelperReturnCapture
            && missingReturnStatus.HelperCallReached
            && missingReturnStatus.HelperReturnedNormally is null
            && missingReturnStatus.HelperReturnBoundaryReached is null
            && missingReturnStatus.SelectedDestinationRawFileOffset is null
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x00BE5AE4);

        check("Receiver effective addresses wrap as 32-bit DWORD addresses",
            UnreliableReceiveResidueHandoffProjection.ReceiverDwordAtOffset04Address(0xFFFFFFFC) == 0
            && UnreliableReceiveResidueHandoffProjection.ReceiverDwordAtOffset18Address(0xFFFFFFFC) == 0x14);
    }

    private static bool VerifyAllEntriesAndEarlyBranches()
    {
        const ushort receiverWord = 0xA55A;
        int accepted = 0;
        int earlyBranches = 0;
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort payloadWord = (ushort)value;
            ushort entryResidue = unchecked((ushort)(receiverWord - payloadWord));
            bool expectedValid = entryResidue >= 0x8000;
            bool valid = UnreliableReceiveResidueHandoffEntry.TryCreate(
                receiverWord,
                payloadWord,
                out var entry);
            if (valid != expectedValid)
                return false;
            if (!valid)
                continue;

            accepted++;
            UnreliableReceiveResidueHandoffPlan plan =
                UnreliableReceiveResidueHandoffProjection.Project(
                    entry,
                    new(0, null, null, null));
            bool expectedEarlyBranch = entryResidue == 0xFFFF;
            if (plan.HandoffResidue != unchecked((ushort)(0 - entryResidue))
                || plan.EarlyJbeTaken != expectedEarlyBranch
                || (expectedEarlyBranch
                    ? plan.Outcome != UnreliableReceiveResidueHandoffOutcome.EarlyJbeTaken
                    : plan.Outcome != UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset04Capture))
            {
                return false;
            }

            if (plan.EarlyJbeTaken == true)
                earlyBranches++;
        }

        return accepted == 0x8000 && earlyBranches == 1;
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5AC2, 0x00FE66C2, "2B CA 0F B7 C1", null, null),
            (0x00BE5AC7, 0x00FE66C7, "83 F8 01 76 24", 0x00BE5AF0, 0x00FE66F0),
            (0x00BE5ACC, 0x00FE66CC, "FF 77 04 8B F0 8D 46 FF 50", null, null),
            (0x00BE5AD5, 0x00FE66D5, "68 04 8D BB 01 6A 00 FF 77 18", null, null),
            (0x00BE5ADF, 0x00FE66DF, "E8 6C D4 FF FF", 0x00BE2F50, 0x00FE3B50)
        };
        return UnreliableReceiveResidueHandoffProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
