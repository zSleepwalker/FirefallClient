using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateTagGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded window, PE mapping, inbound JZ, and byte sequence match",
            CapacityFinalCalleeAlternateTagGateProjection.EntryRawFileOffset
                == 0x012C90A2
            && CapacityFinalCalleeAlternateTagGateProjection.EntryVirtualAddress
                == 0x016C9CA2
            && CapacityFinalCalleeAlternateTagGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C90B0
            && CapacityFinalCalleeAlternateTagGateProjection.AlternateDestinationRawFileOffset
                == 0x012C8EF8
            && CapacityFinalCalleeAlternateTagGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateTagGateProjection.EntryRawFileOffset)
                == 0x016C9CA2
            && CapacityFinalCalleeAlternateTagGateProjection.InboundZeroBranch
                is
                {
                    RawFileOffset: 0x012C8E51,
                    Bytes: "0F 84 4B 02 00 00",
                    BranchTargetRawFileOffset: 0x012C90A2,
                    BranchTargetVirtualAddress: 0x016C9CA2
                }
            && CapacityFinalCalleeAlternateTagGateProjection.AdjacentNonFallthroughJumpRawFileOffset
                == 0x012C909D
            && Normalize(CapacityFinalCalleeAlternateTagGateProjection.CapturedWindowBytes)
                == "8B420483E0033C030F8548FEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateTagGatePlan missingEdx =
            CapacityFinalCalleeAlternateTagGateProjection.Project(new(
                EntryEDX: null,
                FreshDWORD: 3));
        check("Missing EDX stops at the load and marks a supplied DWORD unreached",
            missingEdx.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome.MissingEntryEDXForRead
            && !missingEdx.EntryEDXCaptured
            && missingEdx.FreshDWORDReadReached
            && missingEdx.FreshDWORDAddress is null
            && !missingEdx.FreshDWORDCaptured
            && missingEdx.Reads.Length == 1
            && missingEdx.Reads[0] is
            {
                RawFileOffset: 0x012C90A2,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingEdx.UnreachedFreshDWORDCaptureProvided
            && !missingEdx.AndReached
            && !missingEdx.ByteCompareReached
            && !missingEdx.JnzReached
            && !missingEdx.ExactCleanExitAdmitted
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C90A2);

        CapacityFinalCalleeAlternateTagGatePlan missingFresh =
            CapacityFinalCalleeAlternateTagGateProjection.Project(new(
                EntryEDX: 0x300,
                FreshDWORD: null));
        check("Missing fresh data is distinct from a captured zero and stops at its load",
            missingFresh.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome.MissingFreshDWORD
            && missingFresh.EntryEDXCaptured
            && missingFresh.FreshDWORDReadReached
            && missingFresh.FreshDWORDAddress == 0x304
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x304,
                ValueCaptured: false,
                Value: null
            }
            && !missingFresh.FreshDWORDCaptured
            && !missingFresh.AndReached
            && !missingFresh.ByteCompareReached
            && !missingFresh.JnzReached
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C90A2);

        var expectedFlags = new[]
        {
            new CapacityFinalCalleeAlternateTagGateCompareFlags(
                Carry: true, Parity: false, AuxiliaryCarry: true,
                Zero: false, Sign: true, Overflow: false),
            new CapacityFinalCalleeAlternateTagGateCompareFlags(
                Carry: true, Parity: false, AuxiliaryCarry: true,
                Zero: false, Sign: true, Overflow: false),
            new CapacityFinalCalleeAlternateTagGateCompareFlags(
                Carry: true, Parity: true, AuxiliaryCarry: true,
                Zero: false, Sign: true, Overflow: false),
            new CapacityFinalCalleeAlternateTagGateCompareFlags(
                Carry: false, Parity: true, AuxiliaryCarry: false,
                Zero: true, Sign: false, Overflow: false)
        };
        bool allMaskedTagsMatch = true;
        for (uint tag = 0; tag <= 3; tag++)
        {
            CapacityFinalCalleeAlternateTagGatePlan plan =
                CapacityFinalCalleeAlternateTagGateProjection.Project(new(
                    EntryEDX: 0x100,
                    FreshDWORD: 0xA5A50000 | tag));
            bool notThree = tag != 3;
            allMaskedTagsMatch &= plan.FreshDWORD == (0xA5A50000 | tag)
                && plan.AndReached
                && plan.AndResult == tag
                && plan.ByteCompareReached
                && plan.ByteCompareResult == unchecked((byte)(tag - 3))
                && plan.ByteCompareFlags == expectedFlags[tag]
                && plan.JnzReached
                && plan.JnzTaken == notThree
                && plan.JnzSelectedDestinationRawFileOffset
                    == (notThree
                        ? 0x012C8EF8u
                        : 0x012C90B0u)
                && plan.Outcome == (notThree
                    ? CapacityFinalCalleeAlternateTagGateOutcome
                        .MaskedTagNotThreeToAlternateTarget
                    : CapacityFinalCalleeAlternateTagGateOutcome
                        .MaskedTagThreeToBoundary)
                && plan.ExactCleanExitAdmitted
                && plan.StopAtRawFileOffsetExclusive
                    == plan.JnzSelectedDestinationRawFileOffset;
        }

        check("All four masked tags select the exact byte-CMP/JNZ path",
            allMaskedTagsMatch);

        CapacityFinalCalleeAlternateTagGatePlan highBits =
            CapacityFinalCalleeAlternateTagGateProjection.Project(new(
                EntryEDX: 0xFFFFFFFC,
                FreshDWORD: 0xFFFFFFFE));
        check("High DWORD bits are masked away and the EDX+4 read wraps as a DWORD",
            highBits.FreshDWORDAddress == 0
            && highBits.Reads.Length == 1
            && highBits.Reads[0].Address == 0
            && highBits.AndResult == 2
            && highBits.ByteCompareResult == 0xFF
            && highBits.ByteCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBits.JnzTaken == true
            && highBits.JnzSelectedDestinationRawFileOffset == 0x012C8EF8
            && CapacityFinalCalleeAlternateTagGateProjection.DeriveFreshEdxPlus4Address(
                0xFFFFFFFC) == 0);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C90A2, "8B 42 04", null, null),
            (0x012C90A5, "83 E0 03", null, null),
            (0x012C90A8, "3C 03", null, null),
            (0x012C90AA, "0F 85 48 FE FF FF", 0x012C8EF8, 0x016C9AF8)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateTagGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateTagGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateTagGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateTagGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateTagGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateTagGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateTagGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateTagGateProjection.Instructions
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
