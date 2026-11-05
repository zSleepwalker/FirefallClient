using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateRootGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded CMP/JNZ window and control-flow evidence match",
            CapacityFinalCalleeAlternateRootGateProjection.EntryRawFileOffset
                == 0x012C90CA
            && CapacityFinalCalleeAlternateRootGateProjection.EntryVirtualAddress
                == 0x016C9CCA
            && CapacityFinalCalleeAlternateRootGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C90D2
            && CapacityFinalCalleeAlternateRootGateProjection.FreshDWORDGlobalVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeAlternateRootGateProjection.AlternateDestinationRawFileOffset
                == 0x012C90F6
            && CapacityFinalCalleeAlternateRootGateProjection.FallthroughDestinationRawFileOffset
                == 0x012C90D2
            && CapacityFinalCalleeAlternateRootGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateRootGateProjection.EntryRawFileOffset)
                == 0x016C9CCA
            && CapacityFinalCalleeAlternateRootGateProjection.InboundXrefRawFileOffset
                == 0x012C8F1B
            && !CapacityFinalCalleeAlternateRootGateProjection.InboundXrefMore
            && CapacityFinalCalleeAlternateRootGateProjection.AdjacentNonFallthroughJumpRawFileOffset
                == 0x012C90C5
            && CapacityFinalCalleeAlternateRootGateProjection.AdjacentNonFallthroughJumpDestinationRawFileOffset
                == 0x012C9441
            && Normalize(CapacityFinalCalleeAlternateRootGateProjection.CapturedWindowBytes)
                == "3B15DCADED017524"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateRootGatePlan missingEntry =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: null,
                FreshDWORD: 0));
        check("Missing entry EDX stops at CMP and flags the supplied global as late",
            missingEntry.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.MissingEntryEDX
            && !missingEntry.EntryEDXCaptured
            && !missingEntry.FreshDWORDReadReached
            && missingEntry.FreshDWORDAddress is null
            && !missingEntry.FreshDWORDCaptured
            && !missingEntry.CompareReached
            && !missingEntry.JnzReached
            && missingEntry.LateFreshDWORDCaptureProvided
            && !missingEntry.ExactCleanAdmission
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C90CA);

        CapacityFinalCalleeAlternateRootGatePlan missingGlobal =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0,
                FreshDWORD: null));
        check("A missing fresh global is distinct from a captured zero",
            missingGlobal.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.MissingFreshDWORD
            && missingGlobal.EntryEDXCaptured
            && missingGlobal.FreshDWORDReadReached
            && missingGlobal.FreshDWORDAddress == 0x01EDADDC
            && !missingGlobal.FreshDWORDCaptured
            && !missingGlobal.CompareReached
            && !missingGlobal.JnzReached
            && !missingGlobal.LateFreshDWORDCaptureProvided
            && !missingGlobal.ExactCleanAdmission
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C90CA);

        CapacityFinalCalleeAlternateRootGatePlan equalZero =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0,
                FreshDWORD: 0));
        check("Equal zero takes the fallthrough clean-admission route with exact flags",
            equalZero.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && equalZero.FreshDWORDCaptured
            && equalZero.FreshDWORD == 0
            && equalZero.CompareReached
            && equalZero.CompareResult == 0
            && equalZero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.JnzReached
            && equalZero.JnzTaken == false
            && equalZero.JnzSelectedDestinationRawFileOffset == 0x012C90D2
            && equalZero.ExactCleanAdmission
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C90D2);

        CapacityFinalCalleeAlternateRootGatePlan equalNonzero =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0x81234567,
                FreshDWORD: 0x81234567));
        check("Equal nonzero also takes clean admission and sets only ZF/PF",
            equalNonzero.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && equalNonzero.CompareResult == 0
            && equalNonzero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalNonzero.JnzTaken == false
            && equalNonzero.JnzSelectedDestinationRawFileOffset == 0x012C90D2
            && equalNonzero.ExactCleanAdmission);

        CapacityFinalCalleeAlternateRootGatePlan unsignedWrap =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0,
                FreshDWORD: 1));
        check("Unsigned subtraction wrap produces the exact DWORD flags and alternate route",
            unsignedWrap.CompareResult == 0xFFFFFFFF
            && unsignedWrap.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedWrap.JnzTaken == true
            && unsignedWrap.JnzSelectedDestinationRawFileOffset == 0x012C90F6
            && unsignedWrap.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
            && unsignedWrap.ExactCleanAdmission
            && unsignedWrap.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlternateRootGatePlan signedOverflow =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0x80000000,
                FreshDWORD: 1));
        check("Signed overflow and even low-byte parity are modeled for DWORD CMP",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
            && signedOverflow.JnzTaken == true
            && signedOverflow.JnzSelectedDestinationRawFileOffset == 0x012C90F6
            && signedOverflow.ExactCleanAdmission);

        CapacityFinalCalleeAlternateRootGatePlan evenParity =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshDWORD: 1));
        check("Parity uses only the low byte of the wrapped DWORD result",
            evenParity.CompareResult == 0xFF
            && evenParity.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && evenParity.JnzTaken == true
            && evenParity.JnzSelectedDestinationRawFileOffset == 0x012C90F6);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C90CA, "3B 15 DC AD ED 01", null, null),
            (0x012C90D0, "75 24", 0x012C90F6, 0x016C9CF6)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateRootGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateRootGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateRootGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateRootGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateRootGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateRootGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateRootGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateRootGateProjection.Instructions
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
