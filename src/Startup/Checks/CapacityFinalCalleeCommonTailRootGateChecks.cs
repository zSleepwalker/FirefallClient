using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTailRootGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded CMP/JNZ window and address evidence match",
            CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset
                == 0x012C92C8
            && CapacityFinalCalleeCommonTailRootGateProjection.EntryVirtualAddress
                == 0x016C9EC8
            && CapacityFinalCalleeCommonTailRootGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C92D0
            && CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset
                == 0x012C92EC
            && CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset
                == 0x012C92D0
            && CapacityFinalCalleeCommonTailRootGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset)
                == 0x016C9EC8
            && Normalize(CapacityFinalCalleeCommonTailRootGateProjection.CapturedWindowBytes)
                == "3B35DCADED01751C"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonTailRootGatePlan allMissing =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: null));
        check("Both absent operands remain independent and stop at the CMP",
            allMissing.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
            && !allMissing.EntryESICaptured
            && allMissing.FreshGlobalReadReached
            && allMissing.FreshGlobalAddress == 0x01EDADDC
            && !allMissing.FreshGlobalCaptured
            && allMissing.FreshGlobalDWORD is null
            && !allMissing.CompareReached
            && !allMissing.JnzReached
            && !allMissing.ExactCleanAdmission
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonTailRootGatePlan missingEsi =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: 0));
        check("A captured zero global read is retained when entry ESI is missing",
            missingEsi.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
            && !missingEsi.EntryESICaptured
            && missingEsi.FreshGlobalReadReached
            && missingEsi.FreshGlobalAddress == 0x01EDADDC
            && missingEsi.FreshGlobalCaptured
            && missingEsi.FreshGlobalDWORD == 0
            && !missingEsi.CompareReached
            && !missingEsi.JnzReached
            && !missingEsi.ExactCleanAdmission
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonTailRootGatePlan missingGlobal =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: null));
        check("A missing global read is distinct from a captured zero",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD
            && missingGlobal.EntryESICaptured
            && missingGlobal.FreshGlobalReadReached
            && missingGlobal.FreshGlobalAddress == 0x01EDADDC
            && !missingGlobal.FreshGlobalCaptured
            && !missingGlobal.CompareReached
            && !missingGlobal.JnzReached
            && !missingGlobal.ExactCleanAdmission
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonTailRootGatePlan equalZero =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 0));
        check("Captured zero operands compare equal with exact CMP flags",
            equalZero.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough
            && equalZero.FreshGlobalCaptured
            && equalZero.FreshGlobalDWORD == 0
            && equalZero.CompareReached
            && equalZero.CompareResult == 0
            && equalZero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.JnzReached
            && equalZero.JnzTaken == false
            && equalZero.JnzSelectedDestinationRawFileOffset == 0x012C92D0
            && equalZero.ExactCleanAdmission
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonTailRootGatePlan unsignedUnderflow =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1));
        check("Unsigned underflow retains the complete flags and selects the JNZ target",
            unsignedUnderflow.CompareResult == 0xFFFFFFFF
            && unsignedUnderflow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedUnderflow.JnzTaken == true
            && unsignedUnderflow.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && unsignedUnderflow.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget
            && unsignedUnderflow.ExactCleanAdmission
            && unsignedUnderflow.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonTailRootGatePlan signedOverflow =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: 0x80000000,
                FreshGlobalDWORD: 1));
        check("Signed overflow is represented independently of JNZ's zero test",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.JnzTaken == true
            && signedOverflow.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && signedOverflow.ExactCleanAdmission);

        check("Both branches stop before later captures, writes, or opaque entry",
            equalZero.ExactCleanAdmission
            && unsignedUnderflow.ExactCleanAdmission
            && equalZero.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset
            && unsignedUnderflow.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset
            && equalZero.Writes.IsEmpty
            && unsignedUnderflow.Writes.IsEmpty
            && !equalZero.OpaqueEntryReached
            && !unsignedUnderflow.OpaqueEntryReached);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92C8, "3B 35 DC AD ED 01", null, null),
            (0x012C92CE, "75 1C", 0x012C92EC, 0x016C9EEC)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonTailRootGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailRootGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonTailRootGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonTailRootGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonTailRootGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonTailRootGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonTailRootGateProjection.Instructions
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
