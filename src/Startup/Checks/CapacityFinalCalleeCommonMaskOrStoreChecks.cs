using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonMaskOrStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The OR/store window bytes, raw offsets, VA mapping, and global address match",
            CapacityFinalCalleeCommonMaskOrStoreProjection.EntryRawFileOffset
                == 0x012C931B
            && CapacityFinalCalleeCommonMaskOrStoreProjection.EntryVirtualAddress
                == 0x016C9F1B
            && CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalStoreRawFileOffset
                == 0x012C931D
            && CapacityFinalCalleeCommonMaskOrStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9323
            && CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress
                == 0x01EDADC8
            && CapacityFinalCalleeCommonMaskOrStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonMaskOrStoreProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonMaskOrStoreProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeCommonMaskOrStoreProjection.CapturedWindowBytes)
                == "0BCF890DC8ADED01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonMaskOrStorePlan zero =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(new(0, 0));
        check("Zero operands write zero and produce the exact logical-OR flags",
            zero.Outcome == CapacityFinalCalleeCommonMaskOrStoreOutcome.Completed
            && zero.EntryECXCaptured
            && zero.EntryEDICaptured
            && zero.OrReached
            && zero.OrECX == 0
            && zero.OrEDI == 0
            && zero.OrResult == 0
            && zero.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.GlobalWriteReached
            && zero.GlobalWriteAddress == 0x01EDADC8
            && zero.GlobalWriteValue == 0
            && zero.Writes.Length == 1
            && zero.Writes[0].RawFileOffset == 0x012C931D
            && zero.Writes[0].Address == 0x01EDADC8
            && zero.Writes[0].Value == 0
            && !zero.GlobalReadReached
            && zero.FlagsAtStop == zero.OrFlags
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonMaskOrStorePlan highBit =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(
                new(0x80000000, 1));
        check("The high bit is preserved and parity is computed from the low result byte",
            highBit.OrECX == 0x80000000
            && highBit.OrEDI == 1
            && highBit.OrResult == 0x80000001
            && highBit.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBit.GlobalWriteAddress == 0x01EDADC8
            && highBit.GlobalWriteValue == 0x80000001
            && !highBit.GlobalReadReached
            && highBit.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonMaskOrStorePlan allOnes =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(
                new(0xFFFFFFFF, 0x80000000));
        check("All-one OR result has even low-byte parity and is stored unchanged",
            allOnes.OrResult == 0xFFFFFFFF
            && allOnes.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allOnes.GlobalWriteReached
            && allOnes.GlobalWriteValue == 0xFFFFFFFF
            && allOnes.Writes.Length == 1
            && allOnes.Writes[0].Value == 0xFFFFFFFF
            && allOnes.ExactCleanExitAdmitted
            && allOnes.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonMaskOrStorePlan missingECX =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(new(null, 0x10));
        check("Missing ECX independently retains EDI capture and stops before the OR",
            missingECX.Outcome
                == CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryECX
            && !missingECX.EntryECXCaptured
            && missingECX.EntryEDICaptured
            && !missingECX.OrReached
            && missingECX.OrResult is null
            && missingECX.OrFlags is null
            && !missingECX.GlobalReadReached
            && !missingECX.GlobalWriteReached
            && missingECX.Writes.IsEmpty
            && !missingECX.ExactCleanExitAdmitted
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C931B);

        CapacityFinalCalleeCommonMaskOrStorePlan missingEDI =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(new(0x10, null));
        check("Missing EDI independently retains ECX capture and stops before the OR",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryEDI
            && missingEDI.EntryECXCaptured
            && !missingEDI.EntryEDICaptured
            && !missingEDI.OrReached
            && missingEDI.OrResult is null
            && missingEDI.OrFlags is null
            && !missingEDI.GlobalReadReached
            && !missingEDI.GlobalWriteReached
            && missingEDI.Writes.IsEmpty
            && !missingEDI.ExactCleanExitAdmitted
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C931B);

        CapacityFinalCalleeCommonMaskOrStorePlan missingBoth =
            CapacityFinalCalleeCommonMaskOrStoreProjection.Project(new(null, null));
        check("Missing both OR operands stops at entry without a global access or write",
            missingBoth.Outcome
                == CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingBothEntryOperands
            && !missingBoth.EntryECXCaptured
            && !missingBoth.EntryEDICaptured
            && !missingBoth.OrReached
            && missingBoth.OrECX is null
            && missingBoth.OrEDI is null
            && missingBoth.OrResult is null
            && missingBoth.OrFlags is null
            && !missingBoth.GlobalReadReached
            && !missingBoth.GlobalWriteReached
            && missingBoth.GlobalWriteAddress is null
            && missingBoth.Writes.IsEmpty
            && !missingBoth.ExactCleanExitAdmitted
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C931B);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes)[]
        {
            (0x012C931B, "0B CF"),
            (0x012C931D, "89 0D C8 AD ED 01")
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonMaskOrStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonMaskOrStoreInstructionEvidence instruction
            in CapacityFinalCalleeCommonMaskOrStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonMaskOrStoreProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonMaskOrStoreProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonMaskOrStoreProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonMaskOrStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonMaskOrStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
