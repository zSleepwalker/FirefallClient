using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTailRootStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The MOV/JMP window and raw-to-VA mapping match",
            CapacityFinalCalleeCommonTailRootStoreProjection.EntryRawFileOffset
                == 0x012C92D0
            && CapacityFinalCalleeCommonTailRootStoreProjection.EntryVirtualAddress
                == 0x016C9ED0
            && CapacityFinalCalleeCommonTailRootStoreProjection.StoreRawFileOffset
                == 0x012C92D0
            && CapacityFinalCalleeCommonTailRootStoreProjection.JumpRawFileOffset
                == 0x012C92D6
            && CapacityFinalCalleeCommonTailRootStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C92DB
            && CapacityFinalCalleeCommonTailRootStoreProjection.RootStorePEVirtualAddress
                == 0x01EDADD0
            && CapacityFinalCalleeCommonTailRootStoreProjection.JumpTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeCommonTailRootStoreProjection.JumpTargetVirtualAddress
                == 0x016CA041
            && CapacityFinalCalleeCommonTailRootStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailRootStoreProjection.EntryRawFileOffset)
                == 0x016C9ED0
            && CapacityFinalCalleeCommonTailRootStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailRootStoreProjection.JumpTargetRawFileOffset)
                == 0x016CA041
            && Normalize(
                CapacityFinalCalleeCommonTailRootStoreProjection.CapturedWindowBytes)
                == "893DD0ADED01E966010000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonTailRootStorePlan missing =
            CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(null));
        check("Missing EDI stops at the store without reading or writing memory",
            missing.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.MissingEntryEDI
            && !missing.EntryEDICaptured
            && !missing.StoreReached
            && missing.StoreAddress is null
            && missing.StoreValue is null
            && !missing.JumpReached
            && missing.Writes.IsEmpty
            && !missing.MemoryReadReached
            && !missing.StorePreservesFlags
            && !missing.JumpPreservesFlags
            && !missing.FlagsPreserved
            && missing.FlagsAtStop is null
            && !missing.ExactBoundedExitAdmitted
            && !missing.EpilogueEntered
            && missing.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonTailRootStorePlan zero =
            CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(0));
        check("Captured zero EDI is stored at the absolute PE address and jumps to 9441",
            zero.Outcome
                == CapacityFinalCalleeCommonTailRootStoreOutcome.StoredAndJumped
            && zero.EntryEDICaptured
            && zero.StoreReached
            && zero.StoreAddress == 0x01EDADD0
            && zero.StoreValue == 0
            && zero.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonTailRootStoreWrite(
                    0x012C92D0,
                    0x01EDADD0,
                    0)
            })
            && zero.JumpReached
            && zero.JumpTargetRawFileOffset == 0x012C9441
            && zero.JumpTargetVirtualAddress == 0x016CA041
            && zero.ExactBoundedExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C9441
            && !zero.EpilogueEntered);

        CapacityFinalCalleeCommonTailRootStorePlan wrapped =
            CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(0xFFFFFFFF));
        check("The DWORD value is preserved unchanged by the single store",
            wrapped.StoreAddress == 0x01EDADD0
            && wrapped.StoreValue == 0xFFFFFFFF
            && wrapped.Writes.Length == 1
            && wrapped.Writes[0].Value == 0xFFFFFFFF
            && wrapped.Writes[0].Address == 0x01EDADD0
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonTailRootStorePlan flags =
            CapacityFinalCalleeCommonTailRootStoreProjection.Project(new(0x12345678));
        check("MOV and JMP preserve flags without inventing flag values",
            flags.FlagsPreserved
            && flags.StorePreservesFlags
            && flags.JumpPreservesFlags
            && flags.FlagsAtStop is null
            && !flags.MemoryReadReached
            && flags.Writes.Length == 1
            && flags.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? MemoryOperandPEVA,
            uint? BranchTargetRaw,
            uint? BranchTargetVA)[]
        {
            (0x012C92D0, "89 3D D0 AD ED 01", 0x01EDADD0, null, null),
            (0x012C92D6, "E9 66 01 00 00", null, 0x012C9441, 0x016CA041)
        };

        uint nextRawOffset =
            CapacityFinalCalleeCommonTailRootStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonTailRootStoreInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailRootStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonTailRootStoreProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonTailRootStoreProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonTailRootStoreProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonTailRootStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonTailRootStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.MemoryOperandPEVirtualAddress,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
