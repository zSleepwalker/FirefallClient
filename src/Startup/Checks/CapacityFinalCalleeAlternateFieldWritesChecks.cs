using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateFieldWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The write window, absolute operand, jump target, and instruction bytes match",
            CapacityFinalCalleeAlternateFieldWritesProjection.EntryRawFileOffset
                == 0x012C90B0
            && CapacityFinalCalleeAlternateFieldWritesProjection.EntryVirtualAddress
                == 0x016C9CB0
            && CapacityFinalCalleeAlternateFieldWritesProjection.GlobalWritePeVirtualAddress
                == 0x01EDADD0
            && CapacityFinalCalleeAlternateFieldWritesProjection.AndEdxPlus4RawFileOffset
                == 0x012C90B8
            && CapacityFinalCalleeAlternateFieldWritesProjection.EsiPlus4WriteRawFileOffset
                == 0x012C90BF
            && CapacityFinalCalleeAlternateFieldWritesProjection.EsiPlusEdiWriteRawFileOffset
                == 0x012C90C2
            && CapacityFinalCalleeAlternateFieldWritesProjection.JumpRawFileOffset
                == 0x012C90C5
            && CapacityFinalCalleeAlternateFieldWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C90CA
            && CapacityFinalCalleeAlternateFieldWritesProjection.JumpTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeAlternateFieldWritesProjection.JumpTargetVirtualAddress
                == 0x016CA041
            && CapacityFinalCalleeAlternateFieldWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFieldWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateFieldWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeAlternateFieldWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFieldWritesProjection.JumpTargetRawFileOffset)
                == CapacityFinalCalleeAlternateFieldWritesProjection.JumpTargetVirtualAddress
            && Normalize(
                CapacityFinalCalleeAlternateFieldWritesProjection.CapturedWindowBytes)
                == "893DD0ADED018BC7836204FE83C801894604893C3EE977030000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateFieldWritesPlan missingEdi =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: null,
                entryEdx: 0x100,
                freshEdxPlus4: 0,
                entryEsi: 0x200));
        check("Missing EDI stops before the global write and leaves later captures unused",
            missingEdi.Outcome
                == CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryEDI
            && !missingEdi.GlobalWriteReached
            && !missingEdi.EAXLoadedFromEDI
            && !missingEdi.FreshEdxPlus4ReadReached
            && !missingEdi.AndInstructionReached
            && !missingEdi.EsiPlus4WriteReached
            && !missingEdi.JumpReached
            && missingEdi.UnreachedEntryEDXCaptureProvided
            && missingEdi.UnreachedFreshEdxPlus4CaptureProvided
            && missingEdi.UnreachedEntryESICaptureProvided
            && !missingEdi.ExactCleanExitAdmitted
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C90B0
            && missingEdi.Writes.IsEmpty);

        CapacityFinalCalleeAlternateFieldWritesPlan missingEdx =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: 0x10,
                entryEdx: null,
                freshEdxPlus4: 0,
                entryEsi: 0x200));
        check("The absolute write and EAX load precede the missing EDX address gate",
            missingEdx.Outcome
                == CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryEDX
            && missingEdx.GlobalWriteReached
            && missingEdx.GlobalWriteAddress == 0x01EDADD0
            && missingEdx.GlobalWriteValue == 0x10
            && missingEdx.EAXLoadedFromEDI
            && missingEdx.EAXRetained == 0x10
            && !missingEdx.EntryEDXCaptured
            && missingEdx.FreshEdxPlus4Address is null
            && !missingEdx.FreshEdxPlus4ReadReached
            && !missingEdx.AndInstructionReached
            && missingEdx.UnreachedFreshEdxPlus4CaptureProvided
            && missingEdx.UnreachedEntryESICaptureProvided
            && missingEdx.Writes.Length == 1
            && missingEdx.Writes[0]
                == new CapacityFinalCalleeAlternateFieldWritesWrite(
                    0x012C90B0, 0x01EDADD0, 0x10)
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C90B8);

        CapacityFinalCalleeAlternateFieldWritesPlan missingFresh =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: 0,
                entryEdx: 0xFFFFFFFC,
                freshEdxPlus4: null,
                entryEsi: 0));
        check("A missing fresh EDX+4 DWORD preserves the earlier global write and zero EAX",
            missingFresh.Outcome
                == CapacityFinalCalleeAlternateFieldWritesOutcome.MissingFreshEdxPlus4
            && missingFresh.GlobalWriteReached
            && missingFresh.GlobalWriteValue == 0
            && missingFresh.EAXRetained == 0
            && missingFresh.FreshEdxPlus4Address == 0
            && missingFresh.FreshEdxPlus4ReadReached
            && !missingFresh.FreshEdxPlus4Captured
            && missingFresh.AndInstructionReached
            && missingFresh.AndResult is null
            && !missingFresh.AndWriteReached
            && !missingFresh.OrInstructionReached
            && missingFresh.UnreachedEntryESICaptureProvided
            && missingFresh.Writes.Length == 1
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C90B8);

        CapacityFinalCalleeAlternateFieldWritesPlan missingEsi =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: 0x20,
                entryEdx: 0x30,
                freshEdxPlus4: 0xFFFFFFFF,
                entryEsi: null));
        check("A missing ESI blocks its stores only after the read-modify-write and OR",
            missingEsi.Outcome
                == CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryESI
            && missingEsi.GlobalWriteReached
            && missingEsi.FreshEdxPlus4Captured
            && missingEsi.AndResult == 0xFFFFFFFE
            && missingEsi.AndWriteReached
            && missingEsi.AndWriteAddress == 0x34
            && missingEsi.AndWriteValue == 0xFFFFFFFE
            && missingEsi.AndFlags is
                { Carry: false, Overflow: false, Sign: true, Zero: false }
            && missingEsi.OrInstructionReached
            && missingEsi.EAXAfterOr == 0x21
            && missingEsi.OrFlags is
                { Carry: false, Overflow: false, Sign: false, Zero: false }
            && !missingEsi.EntryESICaptured
            && !missingEsi.EsiPlus4WriteReached
            && !missingEsi.EsiPlusEdiWriteReached
            && !missingEsi.JumpReached
            && !missingEsi.ExactCleanExitAdmitted
            && missingEsi.Writes.Length == 2
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C90BF);

        CapacityFinalCalleeAlternateFieldWritesPlan zeros =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: 0,
                entryEdx: 0,
                freshEdxPlus4: 1,
                entryEsi: 0));
        check("Captured zero registers and values remain distinct from missing inputs",
            zeros.Outcome == CapacityFinalCalleeAlternateFieldWritesOutcome.Completed
            && zeros.GlobalWriteReached
            && zeros.GlobalWriteAddress == 0x01EDADD0
            && zeros.GlobalWriteValue == 0
            && zeros.EAXRetained == 0
            && zeros.FreshEdxPlus4Address == 4
            && zeros.FreshEdxPlus4ReadReached
            && zeros.FreshEdxPlus4Captured
            && zeros.FreshEdxPlus4 == 1
            && zeros.AndResult == 0
            && zeros.AndFlags is
                { Carry: false, Overflow: false, Sign: false, Zero: true, Parity: true }
            && zeros.EAXAfterOr == 1
            && zeros.OrFlags is
                { Carry: false, Overflow: false, Sign: false, Zero: false }
            && zeros.EsiPlus4WriteAddress == 4
            && zeros.EsiPlus4WriteValue == 1
            && zeros.EsiPlusEdiWriteAddress == 0
            && zeros.EsiPlusEdiWriteValue == 0
            && zeros.JumpReached
            && zeros.JumpTargetRawFileOffset == 0x012C9441
            && zeros.JumpTargetVirtualAddress == 0x016CA041
            && zeros.ExactCleanExitAdmitted
            && zeros.Writes.Length == 4
            && zeros.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeAlternateFieldWritesPlan aliasedAndWrapped =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(Capture(
                entryEdi: 4,
                entryEdx: 0x01EDADCC,
                freshEdxPlus4: 0,
                entryEsi: 0xFFFFFFFC));
        check("Address arithmetic wraps and aliased writes remain ordered and independent",
            aliasedAndWrapped.FreshEdxPlus4Address == 0x01EDADD0
            && aliasedAndWrapped.GlobalWriteAddress
                == aliasedAndWrapped.FreshEdxPlus4Address
            && aliasedAndWrapped.GlobalWriteValue == 4
            && aliasedAndWrapped.FreshEdxPlus4Captured
            && aliasedAndWrapped.FreshEdxPlus4 == 0
            && aliasedAndWrapped.AndWriteAddress == 0x01EDADD0
            && aliasedAndWrapped.AndWriteValue == 0
            && aliasedAndWrapped.EsiPlus4WriteAddress == 0
            && aliasedAndWrapped.EsiPlus4WriteValue == 5
            && aliasedAndWrapped.EsiPlusEdiWriteAddress == 0
            && aliasedAndWrapped.EsiPlusEdiWriteValue == 4
            && aliasedAndWrapped.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C90B0, 0x012C90B8, 0x012C90BF, 0x012C90C2
                })
            && aliasedAndWrapped.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[]
                {
                    0x01EDADD0, 0x01EDADD0, 0, 0
                })
            && aliasedAndWrapped.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 4, 0, 5, 4 })
            && aliasedAndWrapped.JumpReached
            && aliasedAndWrapped.ExactCleanExitAdmitted
            && CapacityFinalCalleeAlternateFieldWritesProjection.DeriveEdxPlus4Address(
                0xFFFFFFFC) == 0
            && CapacityFinalCalleeAlternateFieldWritesProjection.DeriveEsiPlus4Address(
                0xFFFFFFFC) == 0
            && CapacityFinalCalleeAlternateFieldWritesProjection.DeriveEsiPlusEdiAddress(
                0xFFFFFFFC, 4) == 0);
    }

    private static CapacityFinalCalleeAlternateFieldWritesCapture Capture(
        uint? entryEdi = 0x10,
        uint? entryEdx = 0x20,
        uint? freshEdxPlus4 = 0x30,
        uint? entryEsi = 0x40) =>
        new(entryEdi, entryEdx, freshEdxPlus4, entryEsi);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw) []
        {
            (0x012C90B0, "89 3D D0 AD ED 01", 0x01EDADD0, null),
            (0x012C90B6, "8B C7", null, null),
            (0x012C90B8, "83 62 04 FE", null, null),
            (0x012C90BC, "83 C8 01", null, null),
            (0x012C90BF, "89 46 04", null, null),
            (0x012C90C2, "89 3C 3E", null, null),
            (0x012C90C5, "E9 77 03 00 00", null, 0x012C9441)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateFieldWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateFieldWritesInstructionEvidence instruction
            in CapacityFinalCalleeAlternateFieldWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateFieldWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetVirtualAddress
                    != (instruction.BranchTargetRawFileOffset is uint targetRaw
                        ? CapacityFinalCalleeAlternateFieldWritesProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                        : null))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateFieldWritesProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateFieldWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateFieldWritesProjection.Instructions
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
