using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateMaskTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The mask-tail byte window, raw/VA mapping, and exact successor match",
            CapacityFinalCalleeAlternateMaskTailProjection.EntryRawFileOffset
                == 0x012C9167
            && CapacityFinalCalleeAlternateMaskTailProjection.EntryVirtualAddress
                == 0x016C9D67
            && CapacityFinalCalleeAlternateMaskTailProjection.BoundaryRawFileOffsetExclusive
                == 0x012C917C
            && CapacityFinalCalleeAlternateMaskTailProjection.SuccessorRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeAlternateMaskTailProjection.GlobalVirtualAddress
                == 0x01EDADC8
            && CapacityFinalCalleeAlternateMaskTailProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateMaskTailProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateMaskTailProjection.EntryVirtualAddress
            && CapacityFinalCalleeAlternateMaskTailProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateMaskTailProjection.SuccessorRawFileOffset)
                == 0x016C9EBD
            && Normalize(CapacityFinalCalleeAlternateMaskTailProjection.CapturedWindowBytes)
                == "8B45F88B4DFCD3E0F7D02105C8ADED01E941010000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateMaskTailPlan allMissing =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus8: 0,
                FreshFrameMinus4: 0,
                FreshGlobalValue: 0,
                EntryFlags: KnownFlags));
        check("Missing EBP gates every memory operation and preserves supplied captures as unreached",
            allMissing.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingEntryEBP
            && !allMissing.EntryEBPCaptured
            && allMissing.FrameMinus8Address is null
            && !allMissing.FrameMinus8ReadReached
            && !allMissing.FrameMinus4ReadReached
            && !allMissing.ShiftReached
            && !allMissing.NotReached
            && !allMissing.GlobalReadReached
            && !allMissing.GlobalWriteReached
            && !allMissing.FinalJumpReached
            && allMissing.UnreachedFreshFrameMinus8CaptureProvided
            && allMissing.UnreachedFreshFrameMinus4CaptureProvided
            && allMissing.UnreachedFreshGlobalCaptureProvided
            && allMissing.UnreachedEntryFlagsCaptureProvided
            && !allMissing.UnusedEntryFlagsCaptureProvided
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9167);

        CapacityFinalCalleeAlternateMaskTailPlan missingFrameMinus8 =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 4,
                FreshFrameMinus8: null,
                FreshFrameMinus4: 3,
                FreshGlobalValue: 0xFFFFFFFF));
        check("A missing fresh EBP-8 read stops before EBP-4 and retains late operands",
            missingFrameMinus8.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus8
            && missingFrameMinus8.FrameMinus8Address == 0xFFFFFFFC
            && missingFrameMinus8.FrameMinus8ReadReached
            && !missingFrameMinus8.FrameMinus8Captured
            && !missingFrameMinus8.FrameMinus4ReadReached
            && !missingFrameMinus8.ShiftReached
            && !missingFrameMinus8.GlobalReadReached
            && missingFrameMinus8.UnreachedFreshFrameMinus4CaptureProvided
            && missingFrameMinus8.UnreachedFreshGlobalCaptureProvided
            && missingFrameMinus8.StopAtRawFileOffsetExclusive == 0x012C9167);

        CapacityFinalCalleeAlternateMaskTailPlan missingFrameMinus4 =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus8: 0x80000001,
                FreshFrameMinus4: null,
                FreshGlobalValue: 0));
        check("A missing independent EBP-4 read preserves the earlier EBP-8 value",
            missingFrameMinus4.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus4
            && missingFrameMinus4.FrameMinus8Address == 0xFFFFFFF8
            && missingFrameMinus4.FrameMinus8Captured
            && missingFrameMinus4.FrameMinus8 == 0x80000001
            && missingFrameMinus4.FrameMinus4Address == 0xFFFFFFFC
            && missingFrameMinus4.FrameMinus4ReadReached
            && !missingFrameMinus4.FrameMinus4Captured
            && !missingFrameMinus4.ShiftReached
            && !missingFrameMinus4.NotReached
            && !missingFrameMinus4.GlobalReadReached
            && missingFrameMinus4.UnreachedFreshGlobalCaptureProvided
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x012C916A);

        CapacityFinalCalleeAlternateMaskTailPlan missingGlobal =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus8: 0x40000001,
                FreshFrameMinus4: 0x12345600,
                FreshGlobalValue: null));
        check("Missing global data retains both loads, masked shift, and NOT without writing",
            missingGlobal.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshGlobalValue
            && missingGlobal.FrameMinus8Address == 0xF8
            && missingGlobal.FrameMinus4Address == 0xFC
            && missingGlobal.FrameMinus8 == 0x40000001
            && missingGlobal.FrameMinus4 == 0x12345600
            && missingGlobal.EffectiveShiftCount == 0
            && missingGlobal.ShiftResult == 0x40000001
            && missingGlobal.ShiftFlags == UnknownFlags
            && missingGlobal.EntryFlagsConsumed == false
            && missingGlobal.NotReached
            && missingGlobal.FlagsAfterNot == UnknownFlags
            && missingGlobal.NotInput == 0x40000001
            && missingGlobal.NotResult == 0xBFFFFFFE
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalCaptured
            && !missingGlobal.GlobalAndReached
            && !missingGlobal.GlobalWriteReached
            && missingGlobal.GlobalWriteValue is null
            && !missingGlobal.FinalJumpReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9171);

        CapacityFinalCalleeAlternateMaskTailPlan countZero =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinus8: 0x80000001,
                FreshFrameMinus4: 0xFFFFFFE0,
                FreshGlobalValue: 0xFFFFFFFF,
                EntryFlags: KnownFlags));
        check("A masked zero shift preserves input flags through NOT and AND uses the fresh global",
            countZero.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor
            && countZero.FrameMinus8Address == 8
            && countZero.FrameMinus4Address == 0xC
            && countZero.ShiftCountByte == 0xE0
            && countZero.EffectiveShiftCount == 0
            && countZero.ShiftResult == 0x80000001
            && countZero.ShiftFlags == KnownFlags
            && countZero.EntryFlagsConsumed
            && !countZero.UnusedEntryFlagsCaptureProvided
            && countZero.FlagsAfterNot == KnownFlags
            && countZero.NotResult == 0x7FFFFFFE
            && countZero.GlobalAddress == 0x01EDADC8
            && countZero.GlobalBefore == 0xFFFFFFFF
            && countZero.GlobalWriteReached
            && countZero.GlobalWriteAddress == 0x01EDADC8
            && countZero.GlobalWriteValue == 0x7FFFFFFE
            && countZero.FinalFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && countZero.FinalJumpReached
            && countZero.ExactCleanExitAdmitted
            && countZero.FinalJumpTaken == true
            && countZero.FinalJumpSelectedDestinationRawFileOffset == 0x012C92BD
            && countZero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeAlternateMaskTailPlan countOne =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus8: 0x40000000,
                FreshFrameMinus4: 0xA1,
                FreshGlobalValue: 0xFFFFFFFF,
                EntryFlags: KnownFlags));
        check("Count one computes CF and the defined OF while ignoring captured entry flags",
            countOne.ShiftCountByte == 0xA1
            && countOne.EffectiveShiftCount == 1
            && countOne.ShiftResult == 0x80000000
            && countOne.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && !countOne.EntryFlagsConsumed
            && countOne.UnusedEntryFlagsCaptureProvided
            && !countOne.ExactCleanExitAdmitted
            && countOne.GlobalWriteValue == 0x7FFFFFFF
            && countOne.FinalFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && countOne.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeAlternateMaskTailPlan countThirtyOne =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus8: 3,
                FreshFrameMinus4: 0xDF,
                FreshGlobalValue: 0xFFFFFFFF));
        check("Count 31 masks correctly and leaves multi-bit OF undefined",
            countThirtyOne.EffectiveShiftCount == 31
            && countThirtyOne.ShiftResult == 0x80000000
            && countThirtyOne.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: null
            }
            && !countThirtyOne.EntryFlagsConsumed
            && !countThirtyOne.UnusedEntryFlagsCaptureProvided
            && countThirtyOne.GlobalWriteValue == 0x7FFFFFFF);

        CapacityFinalCalleeAlternateMaskTailPlan countThirtyTwo =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus8: 0x2468ACE0,
                FreshFrameMinus4: 0x20,
                FreshGlobalValue: 0xFFFFFFFF));
        check("Count 32 aliases zero without entry flags and does not block scalar execution",
            countThirtyTwo.EffectiveShiftCount == 0
            && countThirtyTwo.ShiftResult == 0x2468ACE0
            && countThirtyTwo.ShiftFlags == UnknownFlags
            && !countThirtyTwo.EntryFlagsConsumed
            && !countThirtyTwo.UnusedEntryFlagsCaptureProvided
            && countThirtyTwo.GlobalWriteValue == 0xDB97531F
            && countThirtyTwo.FinalJumpSelectedDestinationRawFileOffset == 0x012C92BD);

        CapacityFinalCalleeAlternateMaskTailPlan countThirtyThree =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus8: 0x80000001,
                FreshFrameMinus4: 0x21,
                FreshGlobalValue: 0));
        check("Count 33 aliases count one and zero global still performs the final write",
            countThirtyThree.EffectiveShiftCount == 1
            && countThirtyThree.ShiftResult == 2
            && countThirtyThree.ShiftFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && countThirtyThree.NotResult == 0xFFFFFFFD
            && countThirtyThree.GlobalBefore == 0
            && countThirtyThree.GlobalWriteReached
            && countThirtyThree.GlobalWriteValue == 0
            && countThirtyThree.FinalFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && countThirtyThree.FinalJumpSelectedDestinationRawFileOffset == 0x012C92BD);

        CapacityFinalCalleeAlternateMaskTailPlan independentReads =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus8: 0x12345678,
                FreshFrameMinus4: 0xFFFFFFE1,
                FreshGlobalValue: 0x55555555));
        check("The fresh frame values stay independent and signed CL aliases count one",
            independentReads.FrameMinus8Address == 0x18
            && independentReads.FrameMinus4Address == 0x1C
            && independentReads.FrameMinus8 == 0x12345678
            && independentReads.FrameMinus4 == 0xFFFFFFE1
            && independentReads.ShiftInput == 0x12345678
            && independentReads.ShiftCountByte == 0xE1
            && independentReads.EffectiveShiftCount == 1
            && independentReads.ShiftResult == 0x2468ACF0
            && independentReads.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        check("Frame addresses and the public shift helper use wrapping DWORD semantics",
            CapacityFinalCalleeAlternateMaskTailProjection.DeriveEbpMinus8Address(0)
                == 0xFFFFFFF8
            && CapacityFinalCalleeAlternateMaskTailProjection.DeriveEbpMinus4Address(0)
                == 0xFFFFFFFC
            && CapacityFinalCalleeAlternateMaskTailProjection.ShiftLeft(0x80000001, 33) == 2
            && CapacityFinalCalleeAlternateMaskTailProjection.Complement(0) == 0xFFFFFFFF);
    }

    private static readonly CapacityFinalCalleeAlternateMaskTailFlags KnownFlags =
        new(true, false, true, false, true, false);

    private static readonly CapacityFinalCalleeAlternateMaskTailFlags UnknownFlags =
        new(null, null, null, null, null, null);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual) []
        {
            (0x012C9167, "8B 45 F8", null, null),
            (0x012C916A, "8B 4D FC", null, null),
            (0x012C916D, "D3 E0", null, null),
            (0x012C916F, "F7 D0", null, null),
            (0x012C9171, "21 05 C8 AD ED 01", null, null),
            (0x012C9177, "E9 41 01 00 00", 0x012C92BD, 0x016C9EBD)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateMaskTailProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateMaskTailInstructionEvidence instruction
            in CapacityFinalCalleeAlternateMaskTailProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateMaskTailProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateMaskTailProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlternateMaskTailProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateMaskTailProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateMaskTailProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateMaskTailProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
