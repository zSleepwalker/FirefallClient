using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveZeroReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE tail bytes and raw-to-VA mapping match the instructions",
            UnreliableReceiveZeroReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveZeroReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveZeroReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveZeroReturnProjection.TailStartRawFileOffset == 0x00BE5B9E
            && UnreliableReceiveZeroReturnProjection.TailStartVirtualAddress == 0x00FE679E
            && UnreliableReceiveZeroReturnProjection.RawFileOffsetToVirtualAddress(0x00BE5B9E)
                == 0x00FE679E
            && UnreliableReceiveZeroReturnProjection.TailEndRawFileOffsetExclusive == 0x00BE5BAC
            && UnreliableReceiveZeroReturnProjection.TailEndVirtualAddressExclusive == 0x00FE67AC
            && UnreliableReceiveZeroReturnProjection.RawFileOffsetToVirtualAddress(0x00BE5BAC)
                == 0x00FE67AC
            && UnreliableReceiveZeroReturnProjection.CapturedTailBytes
                == "83 C4 14 32 C0 5F 5E 5B 8B E5 5D C2 04 00"
            && InstructionEvidenceMatches());

        UnreliableReceiveZeroReturnPlan missingPriorReturn =
            UnreliableReceiveZeroReturnProjection.Project(
                new(
                    PriorForwardCallReturnedNormally: null,
                    HelperReturnEax: 0xA1B2C3D7));
        UnreliableReceiveZeroReturnPlan nonreturningPriorCall =
            UnreliableReceiveZeroReturnProjection.Project(
                new(
                    PriorForwardCallReturnedNormally: false,
                    HelperReturnEax: 0xA1B2C3D7));
        check("Missing or nonreturning prior calls reject the tail and later EAX captures",
            missingPriorReturn.Outcome
                == UnreliableReceiveZeroReturnOutcome.MissingPriorForwardCallReturnCapture
            && missingPriorReturn.PriorForwardCallNormalGatePassed is null
            && missingPriorReturn.UnreachedHelperReturnEaxCaptureProvided
            && !missingPriorReturn.StackCleanupProjected
            && !missingPriorReturn.AlZeroProjected
            && !missingPriorReturn.EdiPopped
            && !missingPriorReturn.FrameStackPointerReset
            && !missingPriorReturn.EbpPopped
            && !missingPriorReturn.ReturnReached
            && missingPriorReturn.ReturnImmediateBytes is null
            && missingPriorReturn.StopAtRawFileOffsetExclusive == 0x00BE5B9E
            && nonreturningPriorCall.Outcome
                == UnreliableReceiveZeroReturnOutcome.PriorForwardCallDidNotReturnNormally
            && nonreturningPriorCall.PriorForwardCallNormalGatePassed == false
            && nonreturningPriorCall.UnreachedHelperReturnEaxCaptureProvided
            && !nonreturningPriorCall.StackCleanupProjected
            && !nonreturningPriorCall.AlZeroProjected
            && !nonreturningPriorCall.EsiPopped
            && !nonreturningPriorCall.EbxPopped
            && !nonreturningPriorCall.ReturnReached
            && nonreturningPriorCall.StopAtRawFileOffsetExclusive == 0x00BE5B9E);

        UnreliableReceiveZeroReturnPlan missingEax =
            UnreliableReceiveZeroReturnProjection.Project(
                new(
                    PriorForwardCallReturnedNormally: true,
                    HelperReturnEax: null));
        UnreliableReceiveZeroReturnPlan capturedEax =
            UnreliableReceiveZeroReturnProjection.Project(
                new(
                    PriorForwardCallReturnedNormally: true,
                    HelperReturnEax: 0xA1B2C3D7));
        check("A missing helper EAX still reaches cleanup and return; captured EAX keeps its upper 24 bits",
            missingEax.Outcome == UnreliableReceiveZeroReturnOutcome.ZeroReturnTailReached
            && missingEax.PriorForwardCallNormalGatePassed == true
            && !missingEax.UnreachedHelperReturnEaxCaptureProvided
            && missingEax.StackCleanupProjected
            && missingEax.StackCleanupBytes == 20
            && missingEax.AlZeroProjected
            && missingEax.AlAfterXor == 0
            && missingEax.EaxAfterXorAl is null
            && missingEax.EdiPopped
            && missingEax.EsiPopped
            && missingEax.EbxPopped
            && missingEax.FrameStackPointerReset
            && missingEax.EbpPopped
            && missingEax.ReturnReached
            && missingEax.ReturnImmediateBytes == 4
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && capturedEax.Outcome == UnreliableReceiveZeroReturnOutcome.ZeroReturnTailReached
            && capturedEax.AlAfterXor == 0
            && capturedEax.EaxAfterXorAl == 0xA1B2C300
            && capturedEax.StackCleanupBytes == 0x14
            && capturedEax.ReturnImmediateBytes == 4);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B9E, 0x00FE679E, "83 C4 14"),
            (0x00BE5BA1, 0x00FE67A1, "32 C0"),
            (0x00BE5BA3, 0x00FE67A3, "5F"),
            (0x00BE5BA4, 0x00FE67A4, "5E"),
            (0x00BE5BA5, 0x00FE67A5, "5B"),
            (0x00BE5BA6, 0x00FE67A6, "8B E5"),
            (0x00BE5BA8, 0x00FE67A8, "5D"),
            (0x00BE5BA9, 0x00FE67A9, "C2 04 00")
        };
        return UnreliableReceiveZeroReturnProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveZeroReturnProjection.StackCleanupBytes == 0x14
            && UnreliableReceiveZeroReturnProjection.ReturnImmediateBytes == 4;
    }
}
