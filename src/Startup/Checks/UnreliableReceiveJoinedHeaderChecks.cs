using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveJoinedHeaderChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The sample, raw-to-VA range, captured bytes, and branch destinations are pinned",
            UnreliableReceiveJoinedHeaderProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveJoinedHeaderProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveJoinedHeaderProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset == 0x00BE5AF0
            && UnreliableReceiveJoinedHeaderProjection.StartVirtualAddress == 0x00FE66F0
            && UnreliableReceiveJoinedHeaderProjection.BranchRawFileOffset == 0x00BE5B14
            && UnreliableReceiveJoinedHeaderProjection.EndRawFileOffsetExclusive == 0x00BE5B16
            && UnreliableReceiveJoinedHeaderProjection.EndVirtualAddressExclusive == 0x00FE6716
            && UnreliableReceiveJoinedHeaderProjection.ZeroBranchTargetRawFileOffset == 0x00BE5B32
            && UnreliableReceiveJoinedHeaderProjection.RawFileOffsetToVirtualAddress(0x00BE5AF0)
                == 0x00FE66F0
            && UnreliableReceiveJoinedHeaderProjection.RawFileOffsetToVirtualAddress(0x00BE5B16)
                == 0x00FE6716
            && UnreliableReceiveJoinedHeaderProjection.RawFileOffsetToVirtualAddress(0x00BE5B32)
                == 0x00FE6732
            && UnreliableReceiveJoinedHeaderProjection.CapturedInstructionBytes
                == "66 8B 43 02 66 89 47 1E 0F B7 0B 8B C1 C1 E9 0C "
                    + "25 FF 07 00 00 83 E8 04 80 E1 03 0F B7 F0 89 75 FC "
                    + "89 4D 08 74 1C"
            && UnreliableReceiveJoinedHeaderProjection.CapturedInstructionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x26
            && InstructionEvidenceMatches());

        check("Both WORD captures remain independent, including zero and the early write",
            VerifyIndependentWordCaptures());

        check("All 65,536 headers match an independent branch, DWORD subtraction, and ESI truncation oracle",
            VerifyAllHeaders());

        UnreliableReceiveJoinedHeaderPlan missingWord =
            UnreliableReceiveJoinedHeaderProjection.Project(new(0xFFFFFFFE, 0xFFFFFFE0, 0, null, 0));
        UnreliableReceiveJoinedHeaderPlan missingWordWithoutLaterCapture =
            UnreliableReceiveJoinedHeaderProjection.Project(new(0, 0, 0, null, null));
        UnreliableReceiveJoinedHeaderPlan missingHeader =
            UnreliableReceiveJoinedHeaderProjection.Project(new(0x1000, 0x2000, 0x3000, 0, null));
        check("Missing first and second WORD captures stop at their reads without erasing prior writes",
            missingWord.Outcome
                == UnreliableReceiveJoinedHeaderOutcome.MissingPayloadWordAtOffset2WithUnreachedHeaderCapture
            && !missingWord.PayloadWordAtOffset2Read
            && !missingWord.ReceiverWordAtOffset1EWritten
            && missingWord.UnreachedPayloadHeaderCaptureProvided
            && !missingWord.PayloadHeaderRead
            && missingWord.SelectedDestinationRawFileOffset is null
            && missingWord.StopAtRawFileOffsetExclusive == 0x00BE5AF0
            && missingWordWithoutLaterCapture.Outcome
                == UnreliableReceiveJoinedHeaderOutcome.MissingPayloadWordAtOffset2
            && !missingWordWithoutLaterCapture.UnreachedPayloadHeaderCaptureProvided
            && missingHeader.Outcome == UnreliableReceiveJoinedHeaderOutcome.MissingPayloadHeader
            && missingHeader.PayloadWordAtOffset2Read
            && missingHeader.ReceiverWordAtOffset1EWritten
            && missingHeader.PayloadWordAtOffset2 == 0
            && !missingHeader.PayloadHeaderRead
            && missingHeader.SubtractedLength32 is null
            && missingHeader.EsiFrameLocalDword is null
            && missingHeader.EcxFrameArgumentDword is null
            && missingHeader.SelectedDestinationRawFileOffset is null
            && missingHeader.StopAtRawFileOffsetExclusive == 0x00BE5AF8);

        UnreliableReceiveJoinedHeaderPlan zeroHeader =
            UnreliableReceiveJoinedHeaderProjection.Project(new(0x1000, 0x2000, 0x3000, 0, 0));
        check("Header zero is a valid capture with wrapped subtraction and the zero JZ target",
            zeroHeader.Outcome == UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken
            && zeroHeader.PayloadHeaderRead
            && zeroHeader.PayloadHeader == 0
            && zeroHeader.MaskedHeader == 0
            && zeroHeader.SubtractedLength32 == 0xFFFFFFFC
            && zeroHeader.EsiFrameLocalDword == 0x0000FFFC
            && zeroHeader.EcxFrameArgumentDword == 0
            && zeroHeader.ZeroBranchTaken == true
            && zeroHeader.SelectedDestinationRawFileOffset == 0x00BE5B32
            && zeroHeader.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        check("Payload, receiver, and frame effective addresses wrap as DWORD addresses",
            missingWord.PayloadWordAtOffset2Address == 0
            && missingWord.ReceiverWordAtOffset1EAddress == 0xFFFFFFFE
            && missingWord.PayloadHeaderAddress == 0xFFFFFFFE
            && missingWord.EsiFrameLocalAddress == 0xFFFFFFFC
            && missingWord.EcxFrameArgumentAddress == 8
            && UnreliableReceiveJoinedHeaderProjection.PayloadAddress(0xFFFFFFFF, 2) == 1
            && UnreliableReceiveJoinedHeaderProjection.ReceiverWordAtOffset1EAddress(0xFFFFFFE2) == 0
            && UnreliableReceiveJoinedHeaderProjection.EsiFrameLocalAddress(3) == 0xFFFFFFFF
            && UnreliableReceiveJoinedHeaderProjection.EcxFrameArgumentAddress(0xFFFFFFF8) == 0);
    }

    private static bool VerifyIndependentWordCaptures()
    {
        const ushort payloadWordAtOffset2 = 0xA55A;
        const ushort payloadHeader = 0xF123;
        UnreliableReceiveJoinedHeaderPlan plan =
            UnreliableReceiveJoinedHeaderProjection.Project(
                new(0x1000, 0x2000, 0x3000, payloadWordAtOffset2, payloadHeader));
        return plan.PayloadWordAtOffset2Read
            && plan.ReceiverWordAtOffset1EWritten
            && plan.PayloadWordAtOffset2 == payloadWordAtOffset2
            && plan.PayloadHeaderRead
            && plan.PayloadHeader == payloadHeader
            && plan.Outcome == UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough
            && plan.EcxFrameArgumentDword == 3;
    }

    private static bool VerifyAllHeaders()
    {
        int zeroBranches = 0;
        int nonzeroBranches = 0;
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort header = (ushort)value;
            ushort copiedWord = unchecked((ushort)(header ^ 0xA55A));
            UnreliableReceiveJoinedHeaderPlan plan =
                UnreliableReceiveJoinedHeaderProjection.Project(
                    new(0xFFFFFFFE, 0xFFFFFFE2, 0x00000003, copiedWord, header));

            uint expectedMaskedHeader = (uint)header & 0x07FFu;
            uint expectedLength32 = unchecked(expectedMaskedHeader - 4u);
            uint expectedEsi = (ushort)expectedLength32;
            uint expectedEcx = ((uint)header >> 12) & 3u;
            bool expectedZeroBranch = expectedEcx == 0;
            uint expectedDestination = expectedZeroBranch ? 0x00BE5B32u : 0x00BE5B16u;
            UnreliableReceiveJoinedHeaderOutcome expectedOutcome = expectedZeroBranch
                ? UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken
                : UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough;

            if (!plan.PayloadWordAtOffset2Read
                || !plan.ReceiverWordAtOffset1EWritten
                || plan.PayloadWordAtOffset2 != copiedWord
                || !plan.PayloadHeaderRead
                || plan.PayloadHeader != header
                || plan.MaskedHeader != expectedMaskedHeader
                || plan.SubtractedLength32 != expectedLength32
                || plan.EsiFrameLocalDword != expectedEsi
                || plan.EcxFrameArgumentDword != expectedEcx
                || plan.ZeroBranchTaken != expectedZeroBranch
                || plan.SelectedDestinationRawFileOffset != expectedDestination
                || plan.StopAtRawFileOffsetExclusive != expectedDestination
                || plan.Outcome != expectedOutcome
                || plan.PayloadWordAtOffset2Address != 0
                || plan.ReceiverWordAtOffset1EAddress != 0
                || plan.EsiFrameLocalAddress != 0xFFFFFFFF
                || plan.EcxFrameArgumentAddress != 0x0000000B)
            {
                return false;
            }

            if (expectedZeroBranch)
                zeroBranches++;
            else
                nonzeroBranches++;
        }

        return zeroBranches == 0x4000 && nonzeroBranches == 0xC000;
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5AF0, 0x00FE66F0, "66 8B 43 02", null, null),
            (0x00BE5AF4, 0x00FE66F4, "66 89 47 1E", null, null),
            (0x00BE5AF8, 0x00FE66F8, "0F B7 0B", null, null),
            (0x00BE5AFB, 0x00FE66FB, "8B C1 C1 E9 0C", null, null),
            (0x00BE5B00, 0x00FE6700, "25 FF 07 00 00 83 E8 04", null, null),
            (0x00BE5B08, 0x00FE6708, "80 E1 03", null, null),
            (0x00BE5B0B, 0x00FE670B, "0F B7 F0 89 75 FC", null, null),
            (0x00BE5B11, 0x00FE6711, "89 4D 08 74 1C", 0x00BE5B32, 0x00FE6732)
        };
        return UnreliableReceiveJoinedHeaderProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }
}
