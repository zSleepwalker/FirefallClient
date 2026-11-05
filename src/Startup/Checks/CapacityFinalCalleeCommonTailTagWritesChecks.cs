using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTailTagWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded instruction window and raw-to-VA mapping match",
            CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeCommonTailTagWritesProjection.EntryVirtualAddress
                == 0x016C9EBD
            && CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset
                == 0x012C92C2
            && CapacityFinalCalleeCommonTailTagWritesProjection.SecondStoreRawFileOffset
                == 0x012C92C5
            && CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C92C8
            && CapacityFinalCalleeCommonTailTagWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonTailTagWritesProjection.EntryVirtualAddress
            && Normalize(
                CapacityFinalCalleeCommonTailTagWritesProjection.CapturedWindowBytes)
                == "8BC783C801894604893C37"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonTailTagWritesPlan allMissing =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(null, null));
        check("Missing EDI stops before MOV and leaves the tail unentered",
            allMissing.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && !allMissing.EntryEDICaptured
            && !allMissing.EAXLoadedFromEDI
            && !allMissing.OrInstructionReached
            && allMissing.OrFlags is null
            && !allMissing.EntryESICaptured
            && !allMissing.FirstStoreReached
            && !allMissing.SecondStoreReached
            && allMissing.Writes.IsEmpty
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeCommonTailTagWritesPlan missingEdiWithLaterEsi =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(null, 0x20));
        check("A supplied ESI remains unreached when the first EDI use is missing",
            missingEdiWithLaterEsi.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && missingEdiWithLaterEsi.UnreachedEntryESICaptureProvided
            && !missingEdiWithLaterEsi.ExactCleanExitAdmitted
            && missingEdiWithLaterEsi.Writes.IsEmpty);

        CapacityFinalCalleeCommonTailTagWritesPlan missingEsi =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(0, null));
        check("Missing ESI retains the completed MOV and OR result and flags",
            missingEsi.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI
            && missingEsi.EntryEDICaptured
            && missingEsi.EAXLoadedFromEDI
            && missingEsi.EAXRetained == 0
            && missingEsi.OrInstructionReached
            && missingEsi.EAXAfterOr == 1
            && missingEsi.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEsi.FlagsAtStop == missingEsi.OrFlags
            && !missingEsi.EntryESICaptured
            && !missingEsi.FirstStoreReached
            && !missingEsi.SecondStoreReached
            && missingEsi.Writes.IsEmpty
            && !missingEsi.ExactCleanExitAdmitted
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeCommonTailTagWritesPlan aliased =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(4, 0xFFFFFFFC));
        check("Aliased DWORD-wrapped stores remain ordered with different values and OR flags",
            aliased.Outcome == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed
            && aliased.EAXAfterOr == 5
            && aliased.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && aliased.FirstStoreAddress == 0
            && aliased.FirstStoreValue == 5
            && aliased.SecondStoreAddress == 0
            && aliased.SecondStoreValue == 4
            && aliased.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C92C2u, 0u, 5u),
                    (0x012C92C5u, 0u, 4u)
                })
            && aliased.FlagsAtStop == aliased.OrFlags
            && aliased.ExactCleanExitAdmitted
            && aliased.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonTailTagWritesPlan zeroAndSign =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                0xFFFFFFFE, 0xFFFFFFFF));
        check("OR flags derive from the DWORD result, while store addresses wrap",
            zeroAndSign.EAXAfterOr == 0xFFFFFFFF
            && zeroAndSign.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroAndSign.FirstStoreAddress == 3
            && zeroAndSign.SecondStoreAddress == 0xFFFFFFFD
            && zeroAndSign.FlagsAtStop == zeroAndSign.OrFlags);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new[]
        {
            (0x012C92BDu, "8B C7"),
            (0x012C92BFu, "83 C8 01"),
            (0x012C92C2u, "89 46 04"),
            (0x012C92C5u, "89 3C 37")
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonTailTagWritesInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailTagWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonTailTagWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonTailTagWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonTailTagWritesProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
