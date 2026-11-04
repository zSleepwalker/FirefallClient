using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveDeltaGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The independent PE sample, raw bytes, raw-to-VA mapping, and branch destinations are pinned",
            ReliableReceiveDeltaGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveDeltaGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveDeltaGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveDeltaGateProjection.GateStartRawFileOffset == 0x00BE5227
            && ReliableReceiveDeltaGateProjection.GateStartVirtualAddress == 0x00FE5E27
            && ReliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(0x00BE5227)
                == 0x00FE5E27
            && ReliableReceiveDeltaGateProjection.BranchRawFileOffset == 0x00BE524D
            && ReliableReceiveDeltaGateProjection.BranchVirtualAddress == 0x00FE5E4D
            && ReliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset == 0x00BE528F
            && ReliableReceiveDeltaGateProjection.BranchTakenDestinationVirtualAddress == 0x00FE5E8F
            && ReliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset == 0x00BE524F
            && ReliableReceiveDeltaGateProjection.FallthroughDestinationVirtualAddress == 0x00FE5E4F
            && ReliableReceiveDeltaGateProjection.GateEndRawFileOffsetExclusive == 0x00BE524F
            && ReliableReceiveDeltaGateProjection.CapturedGateBytes
                == "8B 7D 08 0F B7 57 02 8D 4F 02 0F B7 5E 2A 8B C2 2B C3 "
                    + "C7 45 08 FF 7E 00 00 0F B7 D8 8D 83 00 FF FF FF "
                    + "66 3B 45 08 77 40"
            && ReliableReceiveDeltaGateProjection.CapturedGateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x28
            && InstructionEvidenceMatches());

        check("The projected operand offsets, stack overwrite, modular comparison, and terminal branch agree with the captured instructions",
            ReliableReceiveDeltaGateProjection.PayloadWordOffset == 2
            && ReliableReceiveDeltaGateProjection.ReceiverWordOffset == 0x2A
            && ReliableReceiveDeltaGateProjection.ArgumentStackSlotOffset == 8
            && ReliableReceiveDeltaGateProjection.CompareImmediate == 0x7EFF
            && ReliableReceiveDeltaGateProjection.CompareSubtrahend == 0x100
            && ReliableReceiveDeltaGateProjection.PayloadWordAddress(uint.MaxValue - 1) == 0
            && ReliableReceiveDeltaGateProjection.PayloadWordAddress(uint.MaxValue) == 1
            && ReliableReceiveDeltaGateProjection.ReceiverWordAddress(uint.MaxValue - 0x29) == 0
            && ReliableReceiveDeltaGateProjection.ReceiverWordAddress(uint.MaxValue) == 0x29
            && ShortBranchTarget(
                ReliableReceiveDeltaGateProjection.BranchRawFileOffset,
                0x40) == ReliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
            && ReliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset)
                == ReliableReceiveDeltaGateProjection.BranchTakenDestinationVirtualAddress
            && ReliableReceiveDeltaGateProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset)
                == ReliableReceiveDeltaGateProjection.FallthroughDestinationVirtualAddress);

        ReliableReceiveDeltaGatePlan missingPayload =
            ReliableReceiveDeltaGateProjection.Project(new(0x1000, null, 0x2000, null));
        ReliableReceiveDeltaGatePlan unreachedPayload =
            ReliableReceiveDeltaGateProjection.Project(new(0x1000, null, 0x2000, 0x1234));
        ReliableReceiveDeltaGatePlan missingReceiver =
            ReliableReceiveDeltaGateProjection.Project(new(0x1000, 0x2345, 0x2000, null));
        check("Nullable word captures stop in instruction order and mark later supplied data unreached",
            missingPayload.Outcome == ReliableReceiveDeltaGateOutcome.MissingPayloadWordCapture
            && !missingPayload.PayloadWordRead
            && !missingPayload.ReceiverWordRead
            && missingPayload.PayloadWordAddress is null
            && missingPayload.StopAtRawFileOffsetExclusive
                == ReliableReceiveDeltaGateProjection.PayloadWordReadRawFileOffset
            && unreachedPayload.Outcome == ReliableReceiveDeltaGateOutcome.UnreachedReceiverWordCapture
            && unreachedPayload.UnreachedReceiverWordCaptureProvided
            && !unreachedPayload.ReceiverWordRead
            && missingReceiver.Outcome == ReliableReceiveDeltaGateOutcome.MissingReceiverWordCapture
            && missingReceiver.PayloadWordRead
            && missingReceiver.PayloadWordAddress == 0x1002
            && !missingReceiver.ReceiverWordRead
            && missingReceiver.ReceiverWordAddress == 0x202A
            && missingReceiver.StopAtRawFileOffsetExclusive
                == ReliableReceiveDeltaGateProjection.ReceiverWordReadRawFileOffset);

        bool allDifferencesMatchUnsignedGate = Enumerable.Range(ushort.MinValue, ushort.MaxValue + 1)
            .All(value =>
            {
                ushort difference = (ushort)value;
                const ushort receiverWord = 0xA55A;
                ushort payloadWord = unchecked((ushort)(receiverWord + difference));
                ReliableReceiveDeltaGatePlan plan = ReliableReceiveDeltaGateProjection.Project(
                    new(0xFFFFFFFE, payloadWord, 0xFFFFFFD6, receiverWord));
                uint difference32 = unchecked((uint)payloadWord - receiverWord);
                ushort differenceLow16 = unchecked((ushort)difference32);
                uint compareIntermediate32 = unchecked((uint)differenceLow16 - 0x100);
                ushort compareLow16 = unchecked((ushort)compareIntermediate32);
                bool branchTaken = compareLow16 > 0x7EFF;
                return plan.Outcome == (branchTaken
                        ? ReliableReceiveDeltaGateOutcome.BranchTaken
                        : ReliableReceiveDeltaGateOutcome.FallthroughSelected)
                    && plan.PayloadWordRead
                    && plan.ReceiverWordRead
                    && plan.PayloadWordAddress == 0
                    && plan.ReceiverWordAddress == 0
                    && plan.Difference32 == difference32
                    && plan.DifferenceLow16 == differenceLow16
                    && plan.CompareIntermediate32 == compareIntermediate32
                    && plan.CompareLow16 == compareLow16
                    && plan.BranchTaken == branchTaken
                    && plan.BranchTaken == (difference < 0x100 || difference > 0x7FFF)
                    && plan.SelectedDestinationRawFileOffset
                        == (branchTaken
                            ? ReliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
                            : ReliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset)
                    && plan.StopAtRawFileOffsetExclusive
                        == ReliableReceiveDeltaGateProjection.GateEndRawFileOffsetExclusive;
            });
        check("All 65536 modular word differences match the exact 32-bit subtraction, low-WORD compare, and unsigned JA",
            allDifferencesMatchUnsignedGate);

        check("The explicit modular boundaries select the observed branch destinations",
            VerifyDifference(0x00FF, 0x000000FF, 0xFFFF, branchTaken: true)
            && VerifyDifference(0x0100, 0x00000100, 0x0000, branchTaken: false)
            && VerifyDifference(0x7FFF, 0xFFFF7FFF, 0x7EFF, branchTaken: false)
            && VerifyDifference(0x8000, 0xFFFF8000, 0x7F00, branchTaken: true)
            && VerifyDifference(0xFFFF, 0xFFFFFFFF, 0xFEFF, branchTaken: true));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5227, 0x00FE5E27, "8B 7D 08", null, null),
            (0x00BE522A, 0x00FE5E2A, "0F B7 57 02", null, null),
            (0x00BE522E, 0x00FE5E2E, "8D 4F 02", null, null),
            (0x00BE5231, 0x00FE5E31, "0F B7 5E 2A", null, null),
            (0x00BE5235, 0x00FE5E35, "8B C2 2B C3", null, null),
            (0x00BE5239, 0x00FE5E39, "C7 45 08 FF 7E 00 00", null, null),
            (0x00BE5240, 0x00FE5E40, "0F B7 D8", null, null),
            (0x00BE5243, 0x00FE5E43, "8D 83 00 FF FF FF", null, null),
            (0x00BE5249, 0x00FE5E49, "66 3B 45 08", null, null),
            (0x00BE524D, 0x00FE5E4D, "77 40", 0x00BE528F, 0x00FE5E8F)
        };
        return ReliableReceiveDeltaGateProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static bool VerifyDifference(
        ushort difference,
        uint expectedDifference32,
        ushort expectedCompareLow16,
        bool branchTaken)
    {
        const ushort receiverWord = 0xA55A;
        ushort payloadWord = unchecked((ushort)(receiverWord + difference));
        ReliableReceiveDeltaGatePlan plan = ReliableReceiveDeltaGateProjection.Project(
            new(0xFFFFFFFE, payloadWord, 0xFFFFFFD6, receiverWord));
        uint expectedCompareIntermediate32 = unchecked((uint)difference - 0x100);
        return plan.Difference32 == expectedDifference32
            && plan.DifferenceLow16 == difference
            && plan.CompareIntermediate32 == expectedCompareIntermediate32
            && plan.CompareLow16 == expectedCompareLow16
            && plan.BranchTaken == branchTaken
            && plan.SelectedDestinationRawFileOffset
                == (branchTaken
                    ? ReliableReceiveDeltaGateProjection.BranchTakenDestinationRawFileOffset
                    : ReliableReceiveDeltaGateProjection.FallthroughDestinationRawFileOffset);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
