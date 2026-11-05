using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateRootWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded window bytes, instruction offsets, and exact jump destination match",
            CapacityFinalCalleeAlternateRootWritesProjection.EntryRawFileOffset
                == 0x012C90D2
            && CapacityFinalCalleeAlternateRootWritesProjection.EntryVirtualAddress
                == 0x016C9CD2
            && CapacityFinalCalleeAlternateRootWritesProjection.FreshGlobalPeVirtualAddress
                == 0x01EDADD0
            && CapacityFinalCalleeAlternateRootWritesProjection.FirstStorePeVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeAlternateRootWritesProjection.JumpRawFileOffset
                == 0x012C90F1
            && CapacityFinalCalleeAlternateRootWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C90F6
            && CapacityFinalCalleeAlternateRootWritesProjection.JumpTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeAlternateRootWritesProjection.JumpTargetVirtualAddress
                == 0x016CA041
            && CapacityFinalCalleeAlternateRootWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateRootWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateRootWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeAlternateRootWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateRootWritesProjection.JumpTargetRawFileOffset)
                == CapacityFinalCalleeAlternateRootWritesProjection.JumpTargetVirtualAddress
            && Normalize(CapacityFinalCalleeAlternateRootWritesProjection.CapturedWindowBytes)
                == "8B0DD0ADED0103CF8935DCADED018BC1890DD0ADED0183C801894604890C31E94B030000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateRootWritesPlan missingFreshGlobal =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(Capture(
                freshGlobal: null,
                entryEdi: 0,
                entryEsi: 0));
        check("A missing fresh global stops at its load and leaves later captures unused",
            missingFreshGlobal.Outcome
                == CapacityFinalCalleeAlternateRootWritesOutcome.MissingFreshGlobal
            && missingFreshGlobal.FreshGlobalReadReached
            && !missingFreshGlobal.FreshGlobalCaptured
            && !missingFreshGlobal.AddInstructionReached
            && !missingFreshGlobal.FirstStoreReached
            && !missingFreshGlobal.JumpReached
            && missingFreshGlobal.UnreachedEntryEDICaptureProvided
            && missingFreshGlobal.UnreachedEntryESICaptureProvided
            && !missingFreshGlobal.ExactCleanExitAdmitted
            && missingFreshGlobal.Writes.IsEmpty
            && missingFreshGlobal.StopAtRawFileOffsetExclusive == 0x012C90D2);

        CapacityFinalCalleeAlternateRootWritesPlan missingEdi =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(Capture(
                freshGlobal: 0xFFFFFFFE,
                entryEdi: null,
                entryEsi: 0x100));
        check("A missing EDI stops at ADD after capturing the fresh global",
            missingEdi.Outcome
                == CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryEDI
            && missingEdi.FreshGlobalCaptured
            && missingEdi.FreshGlobal == 0xFFFFFFFE
            && !missingEdi.EntryEDICaptured
            && !missingEdi.AddInstructionReached
            && !missingEdi.FirstStoreReached
            && missingEdi.UnreachedEntryESICaptureProvided
            && missingEdi.Writes.IsEmpty
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C90D8);

        CapacityFinalCalleeAlternateRootWritesPlan missingEsi =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(Capture(
                freshGlobal: 0xFFFFFFFE,
                entryEdi: 3,
                entryEsi: null));
        check("A missing ESI stops at the first store without losing the completed ADD",
            missingEsi.Outcome
                == CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryESI
            && missingEsi.AddInstructionReached
            && missingEsi.AddResult == 1
            && missingEsi.AddFlags is
                {
                    Carry: true,
                    Parity: false,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && !missingEsi.EntryESICaptured
            && !missingEsi.FirstStoreReached
            && !missingEsi.GlobalStoreReached
            && !missingEsi.OrInstructionReached
            && missingEsi.Writes.IsEmpty
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C90DA);

        CapacityFinalCalleeAlternateRootWritesPlan zeroValues =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(Capture(
                freshGlobal: 0,
                entryEdi: 0,
                entryEsi: 0));
        check("Present zero inputs remain distinct from missing inputs",
            zeroValues.Outcome == CapacityFinalCalleeAlternateRootWritesOutcome.Completed
            && zeroValues.FreshGlobalCaptured
            && zeroValues.FreshGlobal == 0
            && zeroValues.AddResult == 0
            && zeroValues.AddFlags is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            && zeroValues.FirstStoreAddress == 0x01EDADDC
            && zeroValues.FirstStoreValue == 0
            && zeroValues.GlobalStoreAddress == 0x01EDADD0
            && zeroValues.GlobalStoreValue == 0
            && zeroValues.EAXAfterOr == 1
            && zeroValues.OrFlags is
                { Carry: false, Parity: false, Zero: false, Sign: false, Overflow: false }
            && zeroValues.EsiPlus4StoreAddress == 4
            && zeroValues.EsiPlus4StoreValue == 1
            && zeroValues.EsiPlusEcxStoreAddress == 0
            && zeroValues.EsiPlusEcxStoreValue == 0
            && zeroValues.JumpReached
            && zeroValues.JumpTargetRawFileOffset == 0x012C9441
            && zeroValues.JumpTargetVirtualAddress == 0x016CA041
            && zeroValues.ExactCleanExitAdmitted
            && zeroValues.Writes.Length == 4
            && zeroValues.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeAlternateRootWritesPlan wrapped =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(Capture(
                freshGlobal: 0xFFFFFFFF,
                entryEdi: 1,
                entryEsi: 0xFFFFFFFC));
        check("ADD flags and wrapped indexed addresses preserve ordered aliased writes",
            wrapped.AddResult == 0
            && wrapped.AddFlags is
                {
                    Carry: true,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            && wrapped.FirstStoreAddress == 0x01EDADDC
            && wrapped.FirstStoreValue == 0xFFFFFFFC
            && wrapped.GlobalStoreValue == 0
            && wrapped.EsiPlus4StoreAddress == 0
            && wrapped.EsiPlus4StoreValue == 1
            && wrapped.EsiPlusEcxStoreAddress == 0xFFFFFFFC
            && wrapped.EsiPlusEcxStoreValue == 0
            && wrapped.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C90DA, 0x012C90E2, 0x012C90EB, 0x012C90EE })
            && wrapped.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x01EDADDC, 0x01EDADD0, 0, 0xFFFFFFFC })
            && wrapped.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 0xFFFFFFFC, 0, 1, 0 })
            && wrapped.JumpReached
            && wrapped.ExactCleanExitAdmitted
            && CapacityFinalCalleeAlternateRootWritesProjection.ApplyAdd(
                0xFFFFFFFF, 1) == 0
            && CapacityFinalCalleeAlternateRootWritesProjection.DeriveEsiPlus4Address(
                0xFFFFFFFC) == 0
            && CapacityFinalCalleeAlternateRootWritesProjection.DeriveEsiPlusEcxAddress(
                0xFFFFFFFC, 4) == 0);
    }

    private static CapacityFinalCalleeAlternateRootWritesCapture Capture(
        uint? freshGlobal = 0x10,
        uint? entryEdi = 0x20,
        uint? entryEsi = 0x30) =>
        new(freshGlobal, entryEdi, entryEsi);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw) []
        {
            (0x012C90D2, "8B 0D D0 AD ED 01", 0x01EDADD0, null),
            (0x012C90D8, "03 CF", null, null),
            (0x012C90DA, "89 35 DC AD ED 01", 0x01EDADDC, null),
            (0x012C90E0, "8B C1", null, null),
            (0x012C90E2, "89 0D D0 AD ED 01", 0x01EDADD0, null),
            (0x012C90E8, "83 C8 01", null, null),
            (0x012C90EB, "89 46 04", null, null),
            (0x012C90EE, "89 0C 31", null, null),
            (0x012C90F1, "E9 4B 03 00 00", null, 0x012C9441)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateRootWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateRootWritesInstructionEvidence instruction
            in CapacityFinalCalleeAlternateRootWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateRootWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetVirtualAddress
                    != (instruction.BranchTargetRawFileOffset is uint targetRaw
                        ? CapacityFinalCalleeAlternateRootWritesProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                        : null))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateRootWritesProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateRootWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateRootWritesProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
