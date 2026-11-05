using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSecondFieldTestChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The standalone 9080 window, PE mapping, and zero branch evidence match",
            CapacityFinalCalleeSecondFieldTestProjection.EntryRawFileOffset
                == 0x012C9080
            && CapacityFinalCalleeSecondFieldTestProjection.EntryVirtualAddress
                == 0x016C9C80
            && CapacityFinalCalleeSecondFieldTestProjection.ZeroDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeSecondFieldTestProjection.BoundaryRawFileOffsetExclusive
                == 0x012C908B
            && CapacityFinalCalleeSecondFieldTestProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeSecondFieldTestProjection.EntryRawFileOffset)
                == CapacityFinalCalleeSecondFieldTestProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeSecondFieldTestProjection.CapturedWindowBytes)
                == "8B4E1485C90F8467FEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeSecondFieldTestPlan missingEsi =
            CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                EntryESI: null,
                FreshECX: 0));
        check("Missing entry ESI stops at 9080 and explicitly marks the supplied fresh value unreached",
            missingEsi.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.MissingEntryESIForRead
            && !missingEsi.EntryESICaptured
            && missingEsi.FreshECXReadReached
            && missingEsi.FreshEsiPlus14Address is null
            && !missingEsi.FreshECXCaptured
            && missingEsi.FreshECX is null
            && missingEsi.Reads.Length == 1
            && missingEsi.Reads[0] is
            {
                RawFileOffset: 0x012C9080,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingEsi.UnreachedFreshECXCaptureProvided
            && !missingEsi.TestECXReached
            && !missingEsi.ZeroBranchReached
            && !missingEsi.ExactCleanExitAdmitted
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSecondFieldTestPlan missingFresh =
            CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                EntryESI: 0x300,
                FreshECX: null));
        check("A missing fresh ECX is distinct from zero and stops at its reached load",
            missingFresh.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.MissingFreshECX
            && missingFresh.EntryESICaptured
            && missingFresh.FreshECXReadReached
            && missingFresh.FreshEsiPlus14Address == 0x314
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x314,
                ValueCaptured: false,
                Value: null
            }
            && !missingFresh.FreshECXCaptured
            && missingFresh.FreshECX is null
            && !missingFresh.UnreachedFreshECXCaptureProvided
            && !missingFresh.TestECXReached
            && !missingFresh.ZeroBranchReached
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSecondFieldTestPlan zero =
            CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                EntryESI: 0xFFFFFFEC,
                FreshECX: 0));
        check("Captured zero follows TEST/JZ, wraps ESI+14, and exposes TEST-defined flags",
            zero.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.FreshECXZeroToTarget
            && zero.FreshEsiPlus14Address == 0
            && zero.FreshECXCaptured
            && zero.FreshECX == 0
            && zero.Reads.Length == 1
            && zero.Reads[0] is
            {
                Address: 0,
                ValueCaptured: true,
                Value: 0
            }
            && zero.TestECXReached
            && zero.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.ZeroBranchReached
            && zero.ZeroBranchTaken == true
            && zero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C8EF2
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeSecondFieldTestPlan nonzero =
            CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                EntryESI: 0xFFFFFFF0,
                FreshECX: 0x80000001));
        check("Nonzero fresh ECX falls through to 908B with low-byte odd parity",
            nonzero.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.FreshECXNonzeroToBoundary
            && nonzero.FreshEsiPlus14Address == 4
            && nonzero.FreshECX == 0x80000001
            && nonzero.TestECXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzero.ZeroBranchReached
            && nonzero.ZeroBranchTaken == false
            && nonzero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C908B
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeSecondFieldTestPlan evenParity =
            CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                EntryESI: 0x100,
                FreshECX: 0x80000003));
        check("TEST parity uses only the low byte and is even for an even population count",
            evenParity.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && evenParity.ZeroBranchTaken == false
            && evenParity.ZeroBranchSelectedDestinationRawFileOffset == 0x012C908B
            && evenParity.ExactCleanExitAdmitted);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9080, "8B 4E 14", null, null),
            (0x012C9083, "85 C9", null, null),
            (0x012C9085, "0F 84 67 FE FF FF", 0x012C8EF2, 0x016C9AF2)
        };
        uint nextRawOffset =
            CapacityFinalCalleeSecondFieldTestProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeSecondFieldTestInstructionEvidence instruction
            in CapacityFinalCalleeSecondFieldTestProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeSecondFieldTestProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeSecondFieldTestProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeSecondFieldTestProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeSecondFieldTestProjection.CapturedWindowBytes)
            && CapacityFinalCalleeSecondFieldTestProjection.Instructions
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
