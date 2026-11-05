using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedMaskShiftChecks
{
    public static void Run(Action<string, bool> check)
    {
        var evidence =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.ProductionInstructionEvidence;
        bool mappedInstructions = evidence.Length == 4
            && evidence[0].RawFileOffset
                == CapacityFinalCalleeLargeSavedMaskShiftProjection.EntryRawFileOffset
            && evidence[0].FlatAddress == 0x012C9251
            && evidence[0].PEVirtualAddress == 0x016C9E51
            && HasBytes(evidence[0], 0x8B, 0x4A, 0x1C)
            && evidence[1].RawFileOffset
                == CapacityFinalCalleeLargeSavedMaskShiftProjection.EBPMinus8ReadRawFileOffset
            && evidence[1].FlatAddress == 0x012C9254
            && evidence[1].PEVirtualAddress == 0x016C9E54
            && HasBytes(evidence[1], 0x8B, 0x45, 0xF8)
            && evidence[2].RawFileOffset
                == CapacityFinalCalleeLargeSavedMaskShiftProjection.ShiftRawFileOffset
            && evidence[2].FlatAddress == 0x012C9257
            && evidence[2].PEVirtualAddress == 0x016C9E57
            && HasBytes(evidence[2], 0xD3, 0xE0)
            && evidence[3].RawFileOffset
                == CapacityFinalCalleeLargeSavedMaskShiftProjection.NotRawFileOffset
            && evidence[3].FlatAddress == 0x012C9259
            && evidence[3].PEVirtualAddress == 0x016C9E59
            && HasBytes(evidence[3], 0xF7, 0xD0)
            && CapacityFinalCalleeLargeSavedMaskShiftProjection.TryGetInstructionEvidence(
                evidence[0].RawFileOffset, out var mapped0)
            && mapped0 == evidence[0]
            && CapacityFinalCalleeLargeSavedMaskShiftProjection.TryGetInstructionEvidence(
                evidence[1].RawFileOffset, out var mapped1)
            && mapped1 == evidence[1]
            && CapacityFinalCalleeLargeSavedMaskShiftProjection.TryGetInstructionEvidence(
                evidence[2].RawFileOffset, out var mapped2)
            && mapped2 == evidence[2]
            && CapacityFinalCalleeLargeSavedMaskShiftProjection.TryGetInstructionEvidence(
                evidence[3].RawFileOffset, out var mapped3)
            && mapped3 == evidence[3];

        check("Production instruction evidence maps raw offsets to flat and PE addresses",
            mappedInstructions);
        var finalInstruction = evidence[evidence.Length - 1];
        check("The bounded stop follows the evidenced NOT instruction and precedes the global AND",
            finalInstruction.RawFileOffset + (uint)finalInstruction.Bytes.Length
                == CapacityFinalCalleeLargeSavedMaskShiftProjection
                    .BoundedStopRawFileOffsetExclusive
            && finalInstruction.FlatAddress + (uint)finalInstruction.Bytes.Length
                == 0x012C925B
            && finalInstruction.PEVirtualAddress + (uint)finalInstruction.Bytes.Length
                == 0x016C9E5B
            && !CapacityFinalCalleeLargeSavedMaskShiftProjection.TryGetInstructionEvidence(
                CapacityFinalCalleeLargeSavedMaskShiftProjection
                    .BoundedStopRawFileOffsetExclusive,
                out _));

        CapacityFinalCalleeLargeSavedMaskShiftPlan missingEDX =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: null,
                FreshEDXPlus1C: 1,
                EntryEBP: 0x200,
                FreshEBPMinus8: 0xFFFFFFFF));
        check("Missing EDX stops before 9251 and flags every supplied later capture unused",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEDX
            && missingEDX.OrderedReads.IsEmpty
            && !missingEDX.FreshEDXPlus1CReadReached
            && !missingEDX.EntryEBPConsumerReached
            && missingEDX.UnreachedFreshEDXPlus1CCaptureProvided
            && missingEDX.UnreachedEntryEBPCaptureProvided
            && missingEDX.UnreachedFreshEBPMinus8CaptureProvided
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C9251
            && !missingEDX.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedMaskShiftPlan missingFirstValue =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                FreshEDXPlus1C: null,
                EntryEBP: 0x200,
                FreshEBPMinus8: 0xFFFFFFFF));
        check("The first fresh read wraps its address and missing data waits at 9251",
            missingFirstValue.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEDXPlus1C
            && missingFirstValue.FreshEDXPlus1CReadReached
            && missingFirstValue.FreshEDXPlus1CAddress == 0x0000000C
            && missingFirstValue.OrderedReads is
            [
                {
                    Stage: CapacityFinalCalleeLargeSavedMaskShiftReadStage.FreshEDXPlus1C,
                    RawFileOffset: 0x012C9251,
                    Address: 0x0000000C,
                    ReadReached: true,
                    ValueCaptured: false
                }
            ]
            && !missingFirstValue.EntryEBPConsumerReached
            && missingFirstValue.UnreachedEntryEBPCaptureProvided
            && missingFirstValue.UnreachedFreshEBPMinus8CaptureProvided
            && missingFirstValue.StopAtRawFileOffsetExclusive == 0x012C9251);

        CapacityFinalCalleeLargeSavedMaskShiftPlan missingEBP =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 0,
                EntryEBP: null,
                FreshEBPMinus8: 0x80000000));
        check("EBP is consumed only after the first fresh read; its absent base stops at 9254",
            missingEBP.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEBP
            && missingEBP.FreshEDXPlus1CCaptured
            && missingEBP.EntryEBPConsumerReached
            && missingEBP.ShiftCountSource == 0
            && missingEBP.ShiftCountLowByte == 0
            && missingEBP.ShiftCountMasked == 0
            && !missingEBP.FreshEBPMinus8ReadReached
            && missingEBP.UnreachedFreshEBPMinus8CaptureProvided
            && missingEBP.OrderedReads.Length == 1
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9254);

        CapacityFinalCalleeLargeSavedMaskShiftPlan missingSecondValue =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 31,
                EntryEBP: 4,
                FreshEBPMinus8: null));
        check("The second fresh read wraps EBP-8 and missing data waits at 9254",
            missingSecondValue.Outcome
                == CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEBPMinus8
            && missingSecondValue.OrderedReads.Length == 2
            && missingSecondValue.OrderedReads[0].Stage
                == CapacityFinalCalleeLargeSavedMaskShiftReadStage.FreshEDXPlus1C
            && missingSecondValue.OrderedReads[1] is
            {
                Stage: CapacityFinalCalleeLargeSavedMaskShiftReadStage.FreshEBPMinus8,
                RawFileOffset: 0x012C9254,
                Address: 0xFFFFFFFC,
                ReadReached: true,
                ValueCaptured: false
            }
            && missingSecondValue.FreshEBPMinus8Address == 0xFFFFFFFC
            && missingSecondValue.ShiftCountSource == 31
            && missingSecondValue.ShiftCountLowByte == 31
            && missingSecondValue.ShiftCountMasked == 31
            && !missingSecondValue.ShiftResult.HasValue
            && missingSecondValue.StopAtRawFileOffsetExclusive == 0x012C9254);

        CapacityFinalCalleeLargeSavedMaskShiftPlan aliased =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: 0xFFFFFFE4,
                FreshEDXPlus1C: 1,
                EntryEBP: 8,
                FreshEBPMinus8: 0x80000001));
        check("Aliased addresses retain two ordered fresh captures without substituting values",
            aliased.Outcome == CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd
            && aliased.OrderedReads.Length == 2
            && aliased.OrderedReads[0].Address == 0
            && aliased.OrderedReads[0].Value == 1
            && aliased.OrderedReads[1].Address == 0
            && aliased.OrderedReads[1].Value == 0x80000001
            && aliased.ShiftInputEAX == 0x80000001
            && aliased.ShiftResult == 2
            && aliased.NotResult == 0xFFFFFFFD
            && aliased.FlagsAtStop == aliased.ShiftFlags
            && !aliased.GlobalAndReached
            && aliased.StopAtRawFileOffsetExclusive == 0x012C925B);

        (uint CountSource, byte ExpectedCount)[] countCases =
        [
            (0, 0),
            (1, 1),
            (31, 31),
            (32, 0),
            (0xFFFFFFFF, 31)
        ];
        uint[] values = [0, 1, 0x80000000, 0xFFFFFFFF];
        foreach ((uint countSource, byte expectedCount) in countCases)
        {
            foreach (uint value in values)
            {
                CapacityFinalCalleeLargeSavedMaskShiftPlan plan =
                    CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                        EntryEDX: 0,
                        FreshEDXPlus1C: countSource,
                        EntryEBP: 8,
                        FreshEBPMinus8: value));

                uint expectedShift = value << expectedCount;
                bool? expectedCarry = expectedCount == 0
                    ? null
                    : ((value >> (32 - expectedCount)) & 1u) != 0;
                bool? expectedOverflow = expectedCount == 1
                    ? ((expectedShift & 0x80000000u) != 0) ^ expectedCarry!.Value
                    : null;
                CapacityFinalCalleeLargeSavedMaskShiftFlags? expectedFlags =
                    expectedCount == 0
                        ? new(null, null, null, null, null, null)
                        : new(
                            expectedCarry,
                            EvenParity((byte)expectedShift),
                            Auxiliary: null,
                            Zero: expectedShift == 0,
                            Sign: (expectedShift & 0x80000000u) != 0,
                            Overflow: expectedOverflow);

                check($"Shift count {countSource:X8} and EAX {value:X8} use the x86 count and flag rules",
                    plan.Outcome
                        == CapacityFinalCalleeLargeSavedMaskShiftOutcome.BoundedBeforeGlobalAnd
                    && plan.ShiftCountSource == countSource
                    && plan.ShiftCountLowByte == (byte)countSource
                    && plan.ShiftCountMasked == expectedCount
                    && plan.ShiftInputEAX == value
                    && plan.ShiftResult == expectedShift
                    && plan.NotResult == ~expectedShift
                    && plan.ShiftFlags == expectedFlags
                    && plan.FlagsAtStop == expectedFlags
                    && plan.NotReached
                    && !plan.GlobalAndReached
                    && plan.ExactCleanExitAdmitted
                    && plan.StopAtRawFileOffsetExclusive == 0x012C925B);
            }
        }

        CapacityFinalCalleeLargeSavedMaskShiftPlan countOne =
            CapacityFinalCalleeLargeSavedMaskShiftProjection.Project(new(
                EntryEDX: 0,
                FreshEDXPlus1C: 1,
                EntryEBP: 8,
                FreshEBPMinus8: 0x80000000));
        check("Count-one OF is defined from result sign XOR carry",
            countOne.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: true
            }
            && countOne.NotResult == 0xFFFFFFFF
            && countOne.FlagsAtStop == countOne.ShiftFlags);
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
        CapacityFinalCalleeLargeSavedMaskShiftInstructionEvidence instruction,
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
}
