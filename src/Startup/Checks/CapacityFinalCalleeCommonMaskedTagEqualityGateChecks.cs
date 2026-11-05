using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonMaskedTagEqualityGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        var evidence =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.ProductionInstructionEvidence;
        check("MOV/mask/shift/CMP/JZ evidence matches the bounded byte window",
            evidence.Length == 7
            && evidence[0] is
            {
                Mnemonic: "MOV ECX, EAX",
                RawFileOffset: 0x012C93D4,
                FlatIdaAddress: 0x012C93D4,
                PEVirtualAddress: 0x016C9FD4
            }
            && HasBytes(evidence[0], 0x8B, 0xC8)
            && evidence[1] is
            {
                Mnemonic: "MOV EBX, EDI",
                RawFileOffset: 0x012C93D6
            }
            && HasBytes(evidence[1], 0x8B, 0xDF)
            && evidence[2] is
            {
                Mnemonic: "MOV EAX, [EDX+4]",
                RawFileOffset: 0x012C93D8
            }
            && HasBytes(evidence[2], 0x8B, 0x42, 0x04)
            && evidence[3] is
            {
                Mnemonic: "AND EAX, FFFFFFF8",
                RawFileOffset: 0x012C93DB
            }
            && HasBytes(evidence[3], 0x83, 0xE0, 0xF8)
            && evidence[4] is
            {
                Mnemonic: "SHL EBX, CL",
                RawFileOffset: 0x012C93DE
            }
            && HasBytes(evidence[4], 0xD3, 0xE3)
            && evidence[5] is
            {
                Mnemonic: "CMP EAX, EDI",
                RawFileOffset: 0x012C93E0
            }
            && HasBytes(evidence[5], 0x3B, 0xC7)
            && evidence[6] is
            {
                Mnemonic: "JZ 012C9403",
                RawFileOffset: 0x012C93E2,
                BranchTargetRawFileOffset: 0x012C9403,
                BranchTargetPEVirtualAddress: 0x016CA003
            }
            && HasBytes(evidence[6], 0x74, 0x1F)
            && Normalize(CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.CapturedWindowBytes)
                == "8BC88BDF8B420483E0F8D3E33BC7741F"
            && EvidenceLookupMatches());

        var destinations =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.BoundedDestinations;
        check("JZ and fall-through destinations stop before either target",
            destinations.Length == 2
            && destinations[0] is
            {
                RawFileOffset: 0x012C9403,
                PEVirtualAddress: 0x016CA003,
                BeforeTargetInstruction: true
            }
            && destinations[1] is
            {
                RawFileOffset: 0x012C93E4,
                PEVirtualAddress: 0x016C9FE4,
                BeforeTargetInstruction: true
            }
            && CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .RawFileOffsetToVirtualAddress(uint.MaxValue) == 0x00400BFF);

        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan missingEAX =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: null, EntryEDI: 0x44, EntryEDX: 0x100, FreshEDXPlus4: 0));
        check("A missing first MOV source stops before consuming later captures",
            missingEAX.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEAX
            && missingEAX.OrderedEvents.Length == 1
            && !missingEAX.OrderedEvents[0].Completed
            && missingEAX.EntryEDICaptureProvidedButUnused
            && missingEAX.EntryEDXCaptureProvidedButUnused
            && missingEAX.FreshEDXPlus4CaptureProvidedButUnused
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C93D4);

        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan missingEDI =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 1, EntryEDI: null, EntryEDX: 0x100, FreshEDXPlus4: 0));
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan missingEDX =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0x44, EntryEDX: null, FreshEDXPlus4: 0xFFFFFFFF));
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan missingFreshRead =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 2, EntryEDI: 0x44, EntryEDX: 0xFFFFFFFE, FreshEDXPlus4: null));
        check("Missing sources stop at their MOV while retaining completed register values",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI
            && missingEDI.ECXValue == 1
            && missingEDI.EAXValue == 1
            && missingEDI.EntryEDXCaptureProvidedButUnused
            && missingEDI.FreshEDXPlus4CaptureProvidedButUnused
            && missingEDI.OrderedEvents.Length == 2
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C93D6
            && missingEDX.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDX
            && missingEDX.ECXValue == 0
            && missingEDX.EAXValue == 0
            && missingEDX.EBXValue == 0x44
            && missingEDX.EntryEDXOperandConsumed == false
            && missingEDX.FreshEDXPlus4CaptureProvidedButUnused
            && missingEDX.ReadAttempts is
            [
                {
                    RawFileOffset: 0x012C93D8,
                    AbsoluteAddress: null,
                    ReadReached: false,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && missingFreshRead.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4
            && missingFreshRead.FreshReadAddress == 2
            && missingFreshRead.ReadAttempts[0].ReadReached
            && !missingFreshRead.ReadAttempts[0].ValueCaptured
            && missingFreshRead.EAXValue == 2
            && missingFreshRead.EBXValue == 0x44
            && missingFreshRead.StopAtRawFileOffsetExclusive == 0x012C93D8);

        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan freshZero =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0, EntryEDX: 0xFFFFFFFE, FreshEDXPlus4: 0));
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan maskedTags =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0xFEDCBAF8, EntryEDX: 0x100, FreshEDXPlus4: 0xFEDCBAFF));
        check("Fresh [EDX+4] data is read independently, wraps its address, and masks tag bits",
            freshZero.FreshReadAddress == 2
            && freshZero.ReadAttempts[0].Value == 0
            && freshZero.EAXValue == 0
            && freshZero.MaskResult == 0
            && freshZero.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403
            && freshZero.StopAtRawFileOffsetExclusive == 0x012C9403
            && maskedTags.FreshReadAddress == 0x104
            && maskedTags.ReadAttempts[0].Value == 0xFEDCBAFF
            && maskedTags.MaskResult == 0xFEDCBAF8
            && maskedTags.JzTaken == true
            && !maskedTags.MemoryWritePerformed);

        (uint CountSource, byte ExpectedCount, uint ExpectedShift)[] countCases =
        [
            (0, 0, 0x80000003),
            (1, 1, 0x00000006),
            (31, 31, 0x80000000),
            (32, 0, 0x80000003)
        ];
        foreach ((uint countSource, byte expectedCount, uint expectedShift) in countCases)
        {
            CapacityFinalCalleeCommonMaskedTagEqualityGatePlan plan =
                CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                    new(
                        EntryEAX: countSource,
                        EntryEDI: 0x80000003,
                        EntryEDX: 0x100,
                        FreshEDXPlus4: 0x1234));
            bool? expectedCarry = expectedCount == 0
                ? false
                : expectedCount == 1 || expectedCount == 31;
            bool? expectedOverflow = expectedCount == 0
                ? false
                : expectedCount == 1 ? true : null;
            var expectedFlags = expectedCount == 0
                ? new CapacityFinalCalleeCommonMaskedTagEqualityGateFlags(
                    false, EvenParity(0x30), null, false, false, false)
                : new CapacityFinalCalleeCommonMaskedTagEqualityGateFlags(
                    expectedCarry,
                    EvenParity((byte)expectedShift),
                    null,
                    expectedShift == 0,
                    (expectedShift & 0x80000000u) != 0,
                    expectedOverflow);

            check($"ECX {countSource:X8} follows masked SHL semantics and flag preservation",
                plan.ShiftCountSource == countSource
                && plan.ShiftCountLowByte == (byte)countSource
                && plan.ShiftCountMasked == expectedCount
                && plan.ShiftInputEBX == 0x80000003
                && plan.ShiftResult == expectedShift
                && plan.ShiftFlags == expectedFlags
                && plan.FlagsAtStop == plan.CompareFlags
                && plan.StopAtRawFileOffsetExclusive == 0x012C93E4);
        }

        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan equal =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0x12345678, EntryEDX: 0x100, FreshEDXPlus4: 0x1234567F));
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan unsignedBelow =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0x80000001, EntryEDX: 0x100, FreshEDXPlus4: 0x80000007));
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan signedOverflow =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(
                new(EntryEAX: 0, EntryEDI: 0x7FFFFFFF, EntryEDX: 0x100, FreshEDXPlus4: 0x80000007));
        check("CMP flags distinguish equality and unsigned differences before the bounded branch",
            equal.CompareSubtractionResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.JzTaken == true
            && equal.JzSelectedDestinationRawFileOffset == 0x012C9403
            && unsignedBelow.CompareSubtractionResult == 0xFFFFFFFF
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedBelow.JzTaken == false
            && unsignedBelow.JzSelectedDestinationRawFileOffset == 0x012C93E4
            && signedOverflow.CompareSubtractionResult == 1
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.StopAtRawFileOffsetExclusive == 0x012C93E4
            && equal.OrderedEvents.Length == 7
            && equal.OrderedEvents[6].Stage
                == CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.Jz);
    }

    private static bool EvidenceLookupMatches()
    {
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.ProductionInstructionEvidence)
        {
            if (instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.PEVirtualAddress
                    != CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || !CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.TryGetInstructionEvidence(
                    instruction.RawFileOffset, out var found)
                || found != instruction)
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvenParity(byte value)
    {
        int setBits = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            setBits += (value >> bit) & 1;
        }

        return (setBits & 1) == 0;
    }

    private static bool HasBytes(
        CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionEvidence instruction,
        params byte[] expected)
    {
        if (instruction.Bytes.Length != expected.Length)
        {
            return false;
        }

        for (int index = 0; index < expected.Length; index++)
        {
            if (instruction.Bytes[index] != expected[index])
            {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
