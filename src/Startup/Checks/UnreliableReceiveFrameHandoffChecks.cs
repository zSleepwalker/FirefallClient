using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveFrameHandoffChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE sample, raw bytes, and authoritative raw-to-VA endpoints match",
            UnreliableReceiveFrameHandoffProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveFrameHandoffProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveFrameHandoffProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveFrameHandoffProjection.EntryRawFileOffset == 0x00BE5C0F
            && UnreliableReceiveFrameHandoffProjection.EntryVirtualAddress == 0x00FE680F
            && UnreliableReceiveFrameHandoffProjection.HelperCallRawFileOffset == 0x00BE5C16
            && UnreliableReceiveFrameHandoffProjection.HelperReturnRawFileOffset == 0x00BE5C1B
            && UnreliableReceiveFrameHandoffProjection.EndRawFileOffsetExclusive == 0x00BE5C26
            && UnreliableReceiveFrameHandoffProjection.EndVirtualAddressExclusive == 0x00FE6826
            && UnreliableReceiveFrameHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE5C0F)
                == 0x00FE680F
            && UnreliableReceiveFrameHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE5C26)
                == 0x00FE6826
            && UnreliableReceiveFrameHandoffProjection.CapturedWindowBytes
                == "8D4508508D4F3CE8850800005F5EB0015B8BE55DC20400"
            && UnreliableReceiveFrameHandoffProjection.CapturedWindowBytes.Length == 0x2E
            && InstructionEvidenceMatches());

        check("The validated entry is the shared join gate and call targets match the raw bytes",
            UnreliableReceiveFrameHandoffProjection.RelativeCallTarget(
                UnreliableReceiveFrameHandoffProjection.HelperCallRawFileOffset,
                0x885) == 0x00BE64A0
            && UnreliableReceiveFrameHandoffProjection.HelperCallTargetRawFileOffset == 0x00BE64A0
            && UnreliableReceiveFrameHandoffProjection.HelperCallTargetVirtualAddress == 0x00FE70A0
            && UnreliableReceiveFrameHandoffProjection.InstructionsInOrder.SequenceEqual(
                new[]
                {
                    UnreliableReceiveFrameHandoffInstruction.LeaFramePlusEightIntoEax,
                    UnreliableReceiveFrameHandoffInstruction.PushFramePlusEightAddress,
                    UnreliableReceiveFrameHandoffInstruction.LeaReceiverPlus3CIntoEcx,
                    UnreliableReceiveFrameHandoffInstruction.CallHelper,
                    UnreliableReceiveFrameHandoffInstruction.PopEdi,
                    UnreliableReceiveFrameHandoffInstruction.PopEsi,
                    UnreliableReceiveFrameHandoffInstruction.SetAlToOne,
                    UnreliableReceiveFrameHandoffInstruction.PopEbx,
                    UnreliableReceiveFrameHandoffInstruction.MoveEspFromEbp,
                    UnreliableReceiveFrameHandoffInstruction.PopEbp,
                    UnreliableReceiveFrameHandoffInstruction.RetFour
                }));

        UnreliableReceiveFrameHandoffPlan unvalidated =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(false, 0x1000, 0x2000, true, 0x12345678));
        check("The gate must be validated before either shared-join path reaches the handoff",
            unvalidated.Outcome == UnreliableReceiveFrameHandoffOutcome.EntryNotValidated
            && !unvalidated.HelperCallReached
            && unvalidated.FramePlusEightAddress is null
            && unvalidated.FrameAddressPushedToHelperStack is null
            && unvalidated.ReceiverPlus3CAddressPassedInEcx is null
            && unvalidated.UnreachedHelperReturnEaxCaptureProvided
            && unvalidated.InstructionsExecuted.IsEmpty
            && unvalidated.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && !unvalidated.EdiPopped
            && !unvalidated.EsiPopped
            && !unvalidated.AlSetToOne
            && !unvalidated.EbxPopped
            && !unvalidated.RetInstructionReached);

        UnreliableReceiveFrameHandoffPlan noReturnCapture =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(true, 0, 0, null, 0xAABBCCDD));
        UnreliableReceiveFrameHandoffPlan helperDidNotReturn =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(true, 0, 0, false, 0xAABBCCDD));
        check("Missing or false helper-return evidence stops before all pops, AL, and RET",
            noReturnCapture.Outcome
                == UnreliableReceiveFrameHandoffOutcome.MissingHelperReturnCapture
            && noReturnCapture.HelperCallReached
            && noReturnCapture.HelperReturnedNormally is null
            && noReturnCapture.UnreachedHelperReturnEaxCaptureProvided
            && !noReturnCapture.EdiPopped
            && !noReturnCapture.EsiPopped
            && !noReturnCapture.AlSetToOne
            && !noReturnCapture.EbxPopped
            && !noReturnCapture.RetInstructionReached
            && noReturnCapture.StopAtRawFileOffsetExclusive == 0x00BE5C16
            && helperDidNotReturn.Outcome
                == UnreliableReceiveFrameHandoffOutcome.HelperDidNotReturnNormally
            && helperDidNotReturn.HelperReturnedNormally == false
            && helperDidNotReturn.UnreachedHelperReturnEaxCaptureProvided
            && helperDidNotReturn.CapturedHelperReturnEax is null
            && !helperDidNotReturn.EdiPopped
            && !helperDidNotReturn.EsiPopped
            && !helperDidNotReturn.AlSetToOne
            && !helperDidNotReturn.EbxPopped
            && !helperDidNotReturn.EspRestoredFromEbp
            && !helperDidNotReturn.EbpPopped
            && !helperDidNotReturn.RetInstructionReached
            && helperDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE5C16);

        UnreliableReceiveFrameHandoffPlan completeWithEax =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(true, 0, 0, true, 0xAABBCCDD));
        UnreliableReceiveFrameHandoffPlan completeWithoutEax =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(true, 0, 0, true, null));
        check("Normal return executes the exact epilogue; AL becomes one while upper EAX bits are preserved",
            completeWithEax.Outcome == UnreliableReceiveFrameHandoffOutcome.ReturnReached
            && completeWithEax.FramePlusEightAddress == 8
            && completeWithEax.FrameAddressPushedToHelperStack == 8
            && completeWithEax.ReceiverPlus3CAddressPassedInEcx == 0x3C
            && completeWithEax.HelperCallReached
            && completeWithEax.HelperReturnedNormally == true
            && completeWithEax.CapturedHelperReturnEax == 0xAABBCCDD
            && completeWithEax.EdiPopped
            && completeWithEax.EsiPopped
            && completeWithEax.AlSetToOne
            && completeWithEax.EaxAfterAlSet == 0xAABBCC01
            && completeWithEax.EbxPopped
            && completeWithEax.EspRestoredFromEbp
            && completeWithEax.EbpPopped
            && completeWithEax.RetInstructionReached
            && completeWithEax.RetImmediateBytes == 4
            && completeWithEax.InstructionsExecuted
                .SequenceEqual(UnreliableReceiveFrameHandoffProjection.InstructionsInOrder)
            && completeWithEax.StopAtRawFileOffsetExclusive == 0x00BE5C26
            && completeWithoutEax.Outcome == UnreliableReceiveFrameHandoffOutcome.ReturnReached
            && completeWithoutEax.AlSetToOne
            && completeWithoutEax.EaxAfterAlSet is null
            && completeWithoutEax.RetInstructionReached
            && completeWithoutEax.RetImmediateBytes == 4
            && !completeWithoutEax.UnreachedHelperReturnEaxCaptureProvided);

        UnreliableReceiveFrameHandoffPlan wrappedAddresses =
            UnreliableReceiveFrameHandoffProjection.Project(
                new(true, uint.MaxValue - 7, uint.MaxValue - 0x3B, true, 0));
        check("LEA address arithmetic wraps at DWORD width, including valid zero bases",
            UnreliableReceiveFrameHandoffProjection.FramePlusEightAddress(0) == 8
            && UnreliableReceiveFrameHandoffProjection.ReceiverPlus3CAddress(0) == 0x3C
            && wrappedAddresses.FramePlusEightAddress == 0
            && wrappedAddresses.FrameAddressPushedToHelperStack == 0
            && wrappedAddresses.ReceiverPlus3CAddressPassedInEcx == 0
            && wrappedAddresses.EaxAfterAlSet == 1);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5C0F, 0x00FE680F, "8D4508", null, null),
            (0x00BE5C12, 0x00FE6812, "50", null, null),
            (0x00BE5C13, 0x00FE6813, "8D4F3C", null, null),
            (0x00BE5C16, 0x00FE6816, "E885080000", 0x00BE64A0, 0x00FE70A0),
            (0x00BE5C1B, 0x00FE681B, "5F", null, null),
            (0x00BE5C1C, 0x00FE681C, "5E", null, null),
            (0x00BE5C1D, 0x00FE681D, "B001", null, null),
            (0x00BE5C1F, 0x00FE681F, "5B", null, null),
            (0x00BE5C20, 0x00FE6820, "8BE5", null, null),
            (0x00BE5C22, 0x00FE6822, "5D", null, null),
            (0x00BE5C23, 0x00FE6823, "C20400", null, null)
        };
        return UnreliableReceiveFrameHandoffProjection.Evidence
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }
}
