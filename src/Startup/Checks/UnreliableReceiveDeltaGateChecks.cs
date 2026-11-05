using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveDeltaGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample identity, raw bytes, raw-to-VA mapping, and destinations match independent literals",
            UnreliableReceiveDeltaGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveDeltaGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveDeltaGateProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && UnreliableReceiveDeltaGateProjection.GateStartRawFileOffset == 0x00BE5A6B
            && UnreliableReceiveDeltaGateProjection.GateStartVirtualAddress == 0x00FE666B
            && UnreliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(0x00BE5A6B) == 0x00FE666B
            && UnreliableReceiveDeltaGateProjection.CapturedGateBytes
                == "8B 5D 08 0F B7 57 1E 8B C2 0F B7 4B 02 2B C1 "
                    + "25 FF FF 00 00 3D 00 80 00 00 7D 3C"
            && UnreliableReceiveDeltaGateProjection.CapturedGateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x1B
            && UnreliableReceiveDeltaGateProjection.BranchRawFileOffset == 0x00BE5A84
            && UnreliableReceiveDeltaGateProjection.BranchVirtualAddress == 0x00FE6684
            && UnreliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset == 0x00BE5AC2
            && UnreliableReceiveDeltaGateProjection.BranchTakenDestinationVirtualAddress == 0x00FE66C2
            && UnreliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset == 0x00BE5A86
            && UnreliableReceiveDeltaGateProjection.FallthroughDestinationVirtualAddress == 0x00FE6686
            && UnreliableReceiveDeltaGateProjection.GateEndRawFileOffsetExclusive == 0x00BE5A86
            && InstructionEvidenceMatches());

        check("The receiver EDI WORD is read before the payload EBX WORD and both effective addresses wrap",
            UnreliableReceiveDeltaGateProjection.ReceiverWordOffset == 0x1E
            && UnreliableReceiveDeltaGateProjection.PayloadWordOffset == 2
            && UnreliableReceiveDeltaGateProjection.ReceiverWordAddress(0xFFFFFFE2) == 0
            && UnreliableReceiveDeltaGateProjection.PayloadWordAddress(uint.MaxValue - 1) == 0
            && ShortBranchTarget(
                UnreliableReceiveDeltaGateProjection.BranchRawFileOffset,
                0x3C) == UnreliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
            && UnreliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(
                UnreliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset)
                == UnreliableReceiveDeltaGateProjection.BranchTakenDestinationVirtualAddress
            && UnreliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(
                UnreliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset)
                == UnreliableReceiveDeltaGateProjection.FallthroughDestinationVirtualAddress);

        UnreliableReceiveDeltaGatePlan missingReceiver =
            UnreliableReceiveDeltaGateProjection.Project(new(0xFFFFFFFE, null, 0xFFFFFFE2, null));
        UnreliableReceiveDeltaGatePlan unreachedPayload =
            UnreliableReceiveDeltaGateProjection.Project(new(0xFFFFFFFE, 0x1234, 0xFFFFFFE2, null));
        UnreliableReceiveDeltaGatePlan missingPayload =
            UnreliableReceiveDeltaGateProjection.Project(new(0xFFFFFFFE, null, 0xFFFFFFE2, 0xA55A));
        check("Nullable captures are consumed strictly in receiver-then-payload memory-read order",
            missingReceiver.Outcome == UnreliableReceiveDeltaGateOutcome.MissingReceiverWordCapture
            && !missingReceiver.ReceiverWordRead
            && missingReceiver.ReceiverWordAddress == 0
            && !missingReceiver.PayloadWordRead
            && missingReceiver.PayloadWordAddress is null
            && missingReceiver.StopAtRawFileOffsetExclusive
                == UnreliableReceiveDeltaGateProjection.ReceiverWordReadRawFileOffset
            && unreachedPayload.Outcome == UnreliableReceiveDeltaGateOutcome.UnreachedPayloadWordCapture
            && unreachedPayload.UnreachedPayloadWordCaptureProvided
            && !unreachedPayload.PayloadWordRead
            && unreachedPayload.PayloadWordAddress is null
            && unreachedPayload.StopAtRawFileOffsetExclusive
                == UnreliableReceiveDeltaGateProjection.ReceiverWordReadRawFileOffset
            && missingPayload.Outcome == UnreliableReceiveDeltaGateOutcome.MissingPayloadWordCapture
            && missingPayload.ReceiverWordRead
            && missingPayload.PayloadWordAddress == 0
            && !missingPayload.PayloadWordRead
            && missingPayload.StopAtRawFileOffsetExclusive
                == UnreliableReceiveDeltaGateProjection.PayloadWordReadRawFileOffset
            && missingPayload.Difference32 is null
            && missingPayload.SelectedDestinationRawFileOffset is null);

        const ushort representativeReceiverWord = 0xA55A;
        bool allResiduesMatchSubtractionMaskAndSignedBranch = Enumerable.Range(0, 0x10000)
            .All(value =>
            {
                ushort residue = (ushort)value;
                ushort payloadWord = unchecked((ushort)(representativeReceiverWord - residue));
                UnreliableReceiveDeltaGatePlan plan =
                    UnreliableReceiveDeltaGateProjection.Project(
                        new(0xFFFFFFFE, payloadWord, 0xFFFFFFE2, representativeReceiverWord));
                uint difference32 = unchecked((uint)representativeReceiverWord - payloadWord);
                uint residue32 = difference32 & 0xFFFF;
                bool signedGreaterThanOrEqual = (int)residue32 >= 0x8000;
                return plan.Outcome == (signedGreaterThanOrEqual
                        ? UnreliableReceiveDeltaGateOutcome.BranchTaken
                        : UnreliableReceiveDeltaGateOutcome.FallthroughSelected)
                    && plan.ReceiverWordRead
                    && plan.ReceiverWordAddress == 0
                    && plan.PayloadWordRead
                    && plan.PayloadWordAddress == 0
                    && plan.Difference32 == difference32
                    && plan.Residue32 == residue32
                    && plan.SignedGreaterThanOrEqual == signedGreaterThanOrEqual
                    && plan.BranchTaken == signedGreaterThanOrEqual
                    && plan.BranchTaken == (residue >= 0x8000)
                    && plan.SelectedDestinationRawFileOffset
                        == (signedGreaterThanOrEqual
                            ? UnreliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
                            : UnreliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset)
                    && plan.StopAtRawFileOffsetExclusive
                        == UnreliableReceiveDeltaGateProjection.GateEndRawFileOffsetExclusive;
            });
        check("All 65536 WORD residues preserve the DWORD subtraction, low-WORD mask, and signed JGE condition",
            allResiduesMatchSubtractionMaskAndSignedBranch);

        check("Residues 0, 0x7FFF, 0x8000, and 0xFFFF select the exact signed-comparison destinations",
            VerifyResidue(0x0000, 0x00000000, branchTaken: false)
            && VerifyResidue(0x7FFF, 0x00007FFF, branchTaken: false)
            && VerifyResidue(0x8000, 0x00008000, branchTaken: true)
            && VerifyResidue(0xFFFF, 0xFFFFFFFF, branchTaken: true));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5A6B, 0x00FE666B, "8B 5D 08", null, null),
            (0x00BE5A6E, 0x00FE666E, "0F B7 57 1E", null, null),
            (0x00BE5A72, 0x00FE6672, "8B C2", null, null),
            (0x00BE5A74, 0x00FE6674, "0F B7 4B 02", null, null),
            (0x00BE5A78, 0x00FE6678, "2B C1", null, null),
            (0x00BE5A7A, 0x00FE667A, "25 FF FF 00 00", null, null),
            (0x00BE5A7F, 0x00FE667F, "3D 00 80 00 00", null, null),
            (0x00BE5A84, 0x00FE6684, "7D 3C", 0x00BE5AC2, 0x00FE66C2)
        };
        return UnreliableReceiveDeltaGateProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static bool VerifyResidue(ushort residue, uint expectedDifference32, bool branchTaken)
    {
        const ushort receiverWord = 0xA55A;
        ushort payloadWord = unchecked((ushort)(receiverWord - residue));
        UnreliableReceiveDeltaGatePlan plan =
            UnreliableReceiveDeltaGateProjection.Project(
                new(0xFFFFFFFE, payloadWord, 0xFFFFFFE2, receiverWord));
        uint expectedResidue32 = expectedDifference32 & 0xFFFF;
        return plan.Difference32 == expectedDifference32
            && plan.Residue32 == expectedResidue32
            && plan.SignedGreaterThanOrEqual == branchTaken
            && plan.BranchTaken == branchTaken
            && plan.SelectedDestinationRawFileOffset
                == (branchTaken
                    ? UnreliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
                    : UnreliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
