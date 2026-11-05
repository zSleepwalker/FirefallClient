using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateSuccessorWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The byte window, raw/IDA mapping, store displacements, and jump target match",
            CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryRawFileOffset
                == 0x012C9190
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryVirtualAddress
                == 0x016C9D90
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C919B
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.JumpTargetRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.JumpTargetVirtualAddress
                == 0x016C9EBD
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.FirstStoreDisplacement
                == 0x0C
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.SecondStoreDisplacement
                == 0x08
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateSuccessorWritesProjection.JumpTargetRawFileOffset)
                == CapacityFinalCalleeAlternateSuccessorWritesProjection.JumpTargetVirtualAddress
            && Normalize(
                CapacityFinalCalleeAlternateSuccessorWritesProjection.CapturedWindowBytes)
                == "89410C894808E922010000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateSuccessorWritesPlan missingBoth =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: null,
                EntryEAX: null));
        check("Missing both entry registers gates the first store at 9190",
            missingBoth.Outcome
                == CapacityFinalCalleeAlternateSuccessorWritesOutcome
                    .MissingEntryECXAndEAX
            && !missingBoth.EntryECXCaptured
            && !missingBoth.EntryEAXCaptured
            && !missingBoth.FirstStoreReached
            && !missingBoth.SecondStoreReached
            && !missingBoth.JumpReached
            && missingBoth.Writes.IsEmpty
            && !missingBoth.ExactCleanExitAdmitted
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeAlternateSuccessorWritesPlan missingEcx =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: null,
                EntryEAX: 0));
        check("Missing ECX gates both stores while preserving captured zero EAX",
            missingEcx.Outcome
                == CapacityFinalCalleeAlternateSuccessorWritesOutcome.MissingEntryECX
            && !missingEcx.EntryECXCaptured
            && missingEcx.EntryEAXCaptured
            && !missingEcx.FirstStoreReached
            && !missingEcx.SecondStoreReached
            && missingEcx.Writes.IsEmpty
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeAlternateSuccessorWritesPlan missingEax =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: 0,
                EntryEAX: null));
        check("Missing EAX gates both stores while preserving captured zero ECX",
            missingEax.Outcome
                == CapacityFinalCalleeAlternateSuccessorWritesOutcome.MissingEntryEAX
            && missingEax.EntryECXCaptured
            && !missingEax.EntryEAXCaptured
            && !missingEax.FirstStoreReached
            && !missingEax.SecondStoreReached
            && missingEax.Writes.IsEmpty
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeAlternateSuccessorWritesPlan zeros =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: 0,
                EntryEAX: 0));
        check("Captured zero registers still reach both ordered stores and the exact exit",
            zeros.Outcome
                == CapacityFinalCalleeAlternateSuccessorWritesOutcome.Completed
            && zeros.EntryECXCaptured
            && zeros.EntryEAXCaptured
            && zeros.FirstStoreAddress == 0x0C
            && zeros.FirstStoreValue == 0
            && zeros.SecondStoreAddress == 0x08
            && zeros.SecondStoreValue == 0
            && zeros.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x0Cu, 0u),
                    (0x012C9193u, 0x08u, 0u)
                })
            && zeros.JumpReached
            && zeros.JumpTargetRawFileOffset == 0x012C92BD
            && zeros.JumpTargetVirtualAddress == 0x016C9EBD
            && zeros.ExactCleanExitAdmitted
            && zeros.FlagsPreserved
            && zeros.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeAlternateSuccessorWritesPlan wrapped =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: 0xFFFFFFF8,
                EntryEAX: 0xFFFFFFF0));
        check("Store address arithmetic wraps independently at DWORD width",
            wrapped.FirstStoreAddress == 0x04
            && wrapped.SecondStoreAddress == 0xFFFFFFF8
            && wrapped.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x04, 0xFFFFFFF8 })
            && CapacityFinalCalleeAlternateSuccessorWritesProjection
                .DeriveFirstStoreAddress(0xFFFFFFF8) == 0x04
            && CapacityFinalCalleeAlternateSuccessorWritesProjection
                .DeriveSecondStoreAddress(0xFFFFFFF0) == 0xFFFFFFF8);

        CapacityFinalCalleeAlternateSuccessorWritesPlan aliases =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                EntryECX: 0x100,
                EntryEAX: 0x104));
        check("Aliased store addresses retain both distinct writes in instruction order",
            aliases.FirstStoreAddress == 0x10C
            && aliases.SecondStoreAddress == aliases.FirstStoreAddress
            && aliases.FirstStoreValue == 0x104
            && aliases.SecondStoreValue == 0x100
            && aliases.Writes.Length == 2
            && aliases.Writes[0]
                == new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    0x012C9190, 0x10C, 0x104)
            && aliases.Writes[1]
                == new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    0x012C9193, 0x10C, 0x100)
            && aliases.Writes[0].Address == aliases.Writes[1].Address
            && aliases.Writes[0].Value != aliases.Writes[1].Value
            && aliases.JumpReached
            && aliases.ExactCleanExitAdmitted);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw) []
        {
            (0x012C9190, "89 41 0C", null),
            (0x012C9193, "89 48 08", null),
            (0x012C9196, "E9 22 01 00 00", 0x012C92BD)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateSuccessorWritesInstructionEvidence instruction
            in CapacityFinalCalleeAlternateSuccessorWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateSuccessorWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetVirtualAddress
                    != (instruction.BranchTargetRawFileOffset is uint targetRaw
                        ? CapacityFinalCalleeAlternateSuccessorWritesProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                        : null))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateSuccessorWritesProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateSuccessorWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateSuccessorWritesProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
