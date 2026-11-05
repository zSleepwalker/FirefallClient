using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveSlotGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The requested gate bytes, sample identity, raw/VA mapping, and exact bounds are pinned",
            ReliableReceiveSlotGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveSlotGateProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && ReliableReceiveSlotGateProjection.GateStartRawFileOffset == 0x00BE5303
            && ReliableReceiveSlotGateProjection.GateStartVirtualAddress == 0x00FE5F03
            && ReliableReceiveSlotGateProjection.RawFileOffsetToVirtualAddress(0x00BE5303) == 0x00FE5F03
            && ReliableReceiveSlotGateProjection.ZeroBranchRawFileOffset == 0x00BE532D
            && ReliableReceiveSlotGateProjection.ZeroBranchVirtualAddress == 0x00FE5F2D
            && ReliableReceiveSlotGateProjection.ZeroBranchDestinationRawFileOffset == 0x00BE538D
            && ReliableReceiveSlotGateProjection.ZeroBranchDestinationVirtualAddress == 0x00FE5F8D
            && ReliableReceiveSlotGateProjection.NonzeroFallthroughRawFileOffset == 0x00BE532F
            && ReliableReceiveSlotGateProjection.GateEndRawFileOffsetExclusive == 0x00BE532F
            && ReliableReceiveSlotGateProjection.CapturedGateBytes
                == "8B 8E 78 04 00 00 8B C3 03 C8 81 E1 FF 00 00 80 79 08 49 "
                    + "81 C9 00 FF FF FF 41 0F B7 C1 83 C0 1E 8D 04 86 89 45 08 "
                    + "8B 00 85 C0 74 5E"
            && ReliableReceiveSlotGateProjection.CapturedGateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2C
            && RawBytesMatchEvidence());

        check("The predecessor admits only unsigned WORD deltas below 0x100",
            ReliableReceiveSlotGateProjection.DeltaUpperBoundExclusive == 0x100
            && ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, 0, out var zeroDelta)
            && zeroDelta.IsValidated
            && zeroDelta.Delta == 0
            && ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, 0xFF, out var maxDelta)
            && maxDelta.IsValidated
            && maxDelta.Delta == 0xFF
            && ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, 0x80, out var middleDelta)
            && middleDelta.IsValidated
            && middleDelta.Delta == 0x80
            && !ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, 0x100, out var entryDelta)
            && !entryDelta.IsValidated
            && !ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, 0x180, out _)
            && !ReliableReceiveSlotGateValidatedInput.TryCreate(0x12345678, 0x87654321, ushort.MaxValue, out _));

        var invalidInputPlan = ReliableReceiveSlotGateProjection.Project(
            default,
            new ReliableReceiveSlotGateCapture(0, 0));
        check("An unvalidated or default input rejects both DWORD captures without advancing",
            invalidInputPlan.Outcome == ReliableReceiveSlotGateOutcome.InvalidOrUnvalidatedDelta
            && !invalidInputPlan.ReceiverDwordRead
            && invalidInputPlan.UnreachedSlotDwordCaptureProvided
            && !invalidInputPlan.FrameArgumentWriteReached
            && !invalidInputPlan.SlotDwordRead
            && invalidInputPlan.SelectedDestinationRawFileOffset is null);

        ReliableReceiveSlotGateValidatedInput input = CreateInput(
            receiverBaseAddress: 0x12345678,
            frameBaseAddress: 0x87654321,
            delta: 0);
        ReliableReceiveSlotGatePlan missingFirst = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(null, null));
        ReliableReceiveSlotGatePlan missingFirstWithLaterSlot = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(null, 0));
        check("The receiver DWORD is a strict first-read gate, including for a captured native zero",
            missingFirst.Outcome == ReliableReceiveSlotGateOutcome.MissingReceiverDwordCapture
            && !missingFirst.ReceiverDwordRead
            && missingFirst.ReceiverDwordAddress == 0x12345AF0
            && !missingFirst.FrameArgumentWriteReached
            && !missingFirst.SlotDwordRead
            && missingFirst.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotGateProjection.ReceiverDwordReadRawFileOffset
            && missingFirstWithLaterSlot.Outcome == ReliableReceiveSlotGateOutcome.UnreachedSlotDwordCapture
            && missingFirstWithLaterSlot.UnreachedSlotDwordCaptureProvided
            && !missingFirstWithLaterSlot.ReceiverDwordRead
            && !missingFirstWithLaterSlot.SlotDwordRead
            && missingFirstWithLaterSlot.SelectedDestinationRawFileOffset is null
            && VerifyZeroReceiverValue(input));

        ReliableReceiveSlotGatePlan missingSlot = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(0, null));
        check("A missing slot DWORD preserves the earlier frame+8 write and computed slot address",
            missingSlot.Outcome == ReliableReceiveSlotGateOutcome.MissingSlotDwordCapture
            && missingSlot.ReceiverDwordRead
            && missingSlot.AddedDeltaDword == 0
            && missingSlot.MaskedDword == 0
            && missingSlot.MaskedSignBitSet == false
            && missingSlot.NormalizedDword == 0
            && missingSlot.IndexWord == 0
            && missingSlot.ZeroExtendedIndexDword == 0
            && missingSlot.SlotArrayOffsetDword == 0x1E
            && missingSlot.SlotByteOffsetDword == 0x78
            && missingSlot.ComputedSlotAddress == 0x123456F0
            && missingSlot.FrameArgumentWriteReached
            && missingSlot.FrameArgumentAddress == 0x87654329
            && missingSlot.FrameArgumentValue == 0x123456F0
            && !missingSlot.SlotDwordRead
            && missingSlot.SlotDwordAddress == missingSlot.FrameArgumentValue
            && missingSlot.SelectedDestinationRawFileOffset is null
            && missingSlot.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotGateProjection.SlotDwordReadRawFileOffset);

        check("The signed mask and DEC/OR/INC sequence preserve every DWORD wrap stage",
            VerifyArithmetic(input, 0x00000000, 0, expectedSum: 0, expectedMask: 0,
                expectedDecrement: null, expectedOr: null, expectedIncrement: null, expectedNormalized: 0)
            && VerifyArithmetic(input, 0x7FFFFFFF, 0, expectedSum: 0x7FFFFFFF, expectedMask: 0xFF,
                expectedDecrement: null, expectedOr: null, expectedIncrement: null, expectedNormalized: 0xFF)
            && VerifyArithmetic(input, 0x80000000, 0, expectedSum: 0x80000000, expectedMask: 0x80000000,
                expectedDecrement: 0x7FFFFFFF, expectedOr: 0xFFFFFFFF, expectedIncrement: 0,
                expectedNormalized: 0)
            && VerifyArithmetic(input, 0xFFFFFFFF, 0, expectedSum: 0xFFFFFFFF, expectedMask: 0x800000FF,
                expectedDecrement: 0x800000FE, expectedOr: 0xFFFFFFFE, expectedIncrement: 0xFFFFFFFF,
                expectedNormalized: 0xFFFFFFFF)
            && VerifyArithmetic(
                CreateInput(input.ReceiverBaseAddress, input.FrameBaseAddress, 0xFF),
                0xFFFFFFFF,
                0xFF,
                expectedSum: 0x000000FE,
                expectedMask: 0xFE,
                expectedDecrement: null,
                expectedOr: null,
                expectedIncrement: null,
                expectedNormalized: 0xFE)
            && VerifyArithmetic(
                CreateInput(input.ReceiverBaseAddress, input.FrameBaseAddress, 0xFF),
                0x7FFFFFFF,
                0xFF,
                expectedSum: 0x800000FE,
                expectedMask: 0x800000FE,
                expectedDecrement: 0x800000FD,
                expectedOr: 0xFFFFFFFD,
                expectedIncrement: 0xFFFFFFFE,
                expectedNormalized: 0xFFFFFFFE));

        check("The zero-extended index and receiver-scaled address wrap as DWORDs",
            VerifyAddressWrap(
                CreateInput(0xFFFFFF90, 0xFFFFFFFC, 0),
                receiverDword: 0,
                slotDword: 1,
                expectedIndex: 0,
                expectedSlotArrayOffset: 0x1E,
                expectedByteOffset: 0x78,
                expectedSlotAddress: 0x08,
                expectedFrameArgumentAddress: 0x00000004));

        ReliableReceiveSlotGatePlan zeroSlot = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(0, 0));
        ReliableReceiveSlotGatePlan nonzeroSlot = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(0, 0xFFFFFFFF));
        check("Only a captured slot DWORD selects the zero-branch or nonzero fallthrough",
            zeroSlot.Outcome == ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected
            && zeroSlot.SlotDwordRead
            && zeroSlot.SlotDwordAddress == zeroSlot.ComputedSlotAddress
            && zeroSlot.ZeroBranchTaken == true
            && zeroSlot.SelectedDestinationRawFileOffset == 0x00BE538D
            && zeroSlot.StopAtRawFileOffsetExclusive == 0x00BE532F
            && nonzeroSlot.Outcome == ReliableReceiveSlotGateOutcome.NonzeroSlotFallthroughSelected
            && nonzeroSlot.SlotDwordRead
            && nonzeroSlot.Capture.SlotDword == 0xFFFFFFFF
            && nonzeroSlot.ZeroBranchTaken == false
            && nonzeroSlot.SelectedDestinationRawFileOffset == 0x00BE532F
            && nonzeroSlot.StopAtRawFileOffsetExclusive == 0x00BE532F);
    }

    private static ReliableReceiveSlotGateValidatedInput CreateInput(
        uint receiverBaseAddress,
        uint frameBaseAddress,
        ushort delta)
    {
        if (!ReliableReceiveSlotGateValidatedInput.TryCreate(
                receiverBaseAddress,
                frameBaseAddress,
                delta,
                out ReliableReceiveSlotGateValidatedInput input))
        {
            throw new InvalidOperationException("The check fixture must satisfy the delta precondition.");
        }

        return input;
    }

    private static bool VerifyZeroReceiverValue(ReliableReceiveSlotGateValidatedInput input)
    {
        ReliableReceiveSlotGatePlan plan = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(0, 0));
        return plan.Outcome == ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected
            && plan.ReceiverDwordRead
            && plan.Capture.ReceiverDword == 0
            && plan.AddedDeltaDword == 0
            && plan.SlotDwordRead
            && plan.SlotDwordAddress == 0x123456F0
            && plan.SelectedDestinationRawFileOffset == 0x00BE538D;
    }

    private static bool VerifyArithmetic(
        ReliableReceiveSlotGateValidatedInput input,
        uint receiverDword,
        ushort delta,
        uint expectedSum,
        uint expectedMask,
        uint? expectedDecrement,
        uint? expectedOr,
        uint? expectedIncrement,
        uint expectedNormalized)
    {
        ReliableReceiveSlotGateValidatedInput actualInput = delta == input.Delta
            ? input
            : CreateInput(input.ReceiverBaseAddress, input.FrameBaseAddress, delta);
        ReliableReceiveSlotGatePlan plan = ReliableReceiveSlotGateProjection.Project(
            actualInput,
            new ReliableReceiveSlotGateCapture(receiverDword, 0));
        return plan.ReceiverDwordRead
            && plan.AddedDeltaDword == expectedSum
            && plan.MaskedDword == expectedMask
            && plan.MaskedSignBitSet == ((expectedMask & 0x80000000) != 0)
            && plan.DecrementedMaskedDword == expectedDecrement
            && plan.OrExtendedDword == expectedOr
            && plan.IncrementedOrExtendedDword == expectedIncrement
            && plan.NormalizedDword == expectedNormalized
            && plan.IndexWord == unchecked((ushort)expectedNormalized)
            && plan.ZeroExtendedIndexDword == unchecked((ushort)expectedNormalized)
            && plan.SlotDwordRead;
    }

    private static bool VerifyAddressWrap(
        ReliableReceiveSlotGateValidatedInput input,
        uint receiverDword,
        uint slotDword,
        ushort expectedIndex,
        uint expectedSlotArrayOffset,
        uint expectedByteOffset,
        uint expectedSlotAddress,
        uint expectedFrameArgumentAddress)
    {
        ReliableReceiveSlotGatePlan plan = ReliableReceiveSlotGateProjection.Project(
            input,
            new ReliableReceiveSlotGateCapture(receiverDword, slotDword));
        return plan.ReceiverDwordAddress == 0x00000408
            && plan.IndexWord == expectedIndex
            && plan.ZeroExtendedIndexDword == expectedIndex
            && plan.SlotArrayOffsetDword == expectedSlotArrayOffset
            && plan.SlotByteOffsetDword == expectedByteOffset
            && plan.ComputedSlotAddress == expectedSlotAddress
            && plan.FrameArgumentWriteReached
            && plan.FrameArgumentAddress == expectedFrameArgumentAddress
            && plan.FrameArgumentValue == expectedSlotAddress
            && plan.SlotDwordRead
            && plan.SlotDwordAddress == expectedSlotAddress;
    }

    private static bool RawBytesMatchEvidence()
    {
        var expected = new (uint Raw, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5303, "8B 8E 78 04 00 00", null, null),
            (0x00BE530B, "03 C8", null, null),
            (0x00BE530D, "81 E1 FF 00 00 80 79 08", 0x00BE531D, 0x00FE5F1D),
            (0x00BE5315, "49 81 C9 00 FF FF FF 41", null, null),
            (0x00BE531D, "0F B7 C1 83 C0 1E", null, null),
            (0x00BE5323, "8D 04 86", null, null),
            (0x00BE5326, "89 45 08", null, null),
            (0x00BE5329, "8B 00 85 C0 74 5E", 0x00BE538D, 0x00FE5F8D)
        };
        bool evidenceMatches = ReliableReceiveSlotGateProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);

        return evidenceMatches
            && ShortBranchTarget(0x00BE5313, 0x08) == 0x00BE531D
            && ShortBranchTarget(0x00BE532D, 0x5E) == 0x00BE538D
            && ReliableReceiveSlotGateProjection.RawFileOffsetToVirtualAddress(0x00BE532D)
                == ReliableReceiveSlotGateProjection.ZeroBranchVirtualAddress;
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
