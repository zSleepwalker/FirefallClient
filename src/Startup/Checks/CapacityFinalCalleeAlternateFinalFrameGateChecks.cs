using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateFinalFrameGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The seven-byte window, mapping, and exact branch destinations match",
            CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset
                == 0x012C9160
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryVirtualAddress
                == 0x016C9D60
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9167
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.EqualDestinationRawFileOffset
                == 0x012C9167
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset
                == 0x012C917C
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset)
                == 0x016C9D60
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset)
                == 0x016C9D7C
            && Normalize(CapacityFinalCalleeAlternateFinalFrameGateProjection.CapturedWindowBytes)
                == "8B45EC3BC17515"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateFinalFrameGatePlan missingEbp =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus14: 0x20000000,
                EntryECX: 0));
        check("Missing EBP gates the fresh read and rejects later frame and ECX captures",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryEBP
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FrameMinus14ReadReached
            && missingEbp.FrameMinus14Address is null
            && !missingEbp.FrameMinus14Captured
            && !missingEbp.EntryECXCaptured
            && !missingEbp.CompareReached
            && !missingEbp.JnzReached
            && missingEbp.UnreachedFreshFrameMinus14CaptureProvided
            && missingEbp.UnreachedEntryECXCaptureProvided
            && missingEbp.LateFreshFrameMinus14CaptureProvided
            && missingEbp.LateEntryECXCaptureProvided
            && !missingEbp.ExactCleanExitAdmitted
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C9160
            && missingEbp.Writes.IsEmpty);

        CapacityFinalCalleeAlternateFinalFrameGatePlan missingFresh =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x14,
                FreshFrameMinus14: null,
                EntryECX: 0x100));
        check("Missing fresh EBP-14 data stops at its load and cannot be bypassed by late ECX",
            missingFresh.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14
            && missingFresh.EntryEBPCaptured
            && missingFresh.FrameMinus14ReadReached
            && missingFresh.FrameMinus14Address == 0
            && !missingFresh.FrameMinus14Captured
            && missingFresh.FrameMinus14 is null
            && !missingFresh.EntryECXCaptured
            && !missingFresh.CompareReached
            && !missingFresh.JnzReached
            && !missingFresh.UnreachedFreshFrameMinus14CaptureProvided
            && missingFresh.UnreachedEntryECXCaptureProvided
            && missingFresh.LateEntryECXCaptureProvided
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeAlternateFinalFrameGatePlan missingEcx =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus14: 0x20000000,
                EntryECX: null));
        check("Missing ECX stops at CMP after the independent fresh load",
            missingEcx.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryECX
            && missingEcx.EntryEBPCaptured
            && missingEcx.FrameMinus14ReadReached
            && missingEcx.FrameMinus14Address == 0xEC
            && missingEcx.FrameMinus14Captured
            && missingEcx.FrameMinus14 == 0x20000000
            && !missingEcx.EntryECXCaptured
            && !missingEcx.CompareReached
            && missingEcx.CompareResult is null
            && !missingEcx.JnzReached
            && !missingEcx.LateEntryECXCaptureProvided
            && !missingEcx.ExactCleanExitAdmitted
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9163);

        CapacityFinalCalleeAlternateFinalFrameGatePlan equalZero =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus14: 0,
                EntryECX: 0));
        check("Equal zero falls through exactly after the bounded window",
            equalZero.FrameMinus14Address == 0xFFFFFFEC
            && equalZero.FrameMinus14Captured
            && equalZero.CompareLeft == 0
            && equalZero.CompareRight == 0
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
            && equalZero.JnzSelectedDestinationRawFileOffset == 0x012C9167
            && equalZero.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough
            && equalZero.ExactCleanExitAdmitted
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C9167
            && equalZero.Writes.IsEmpty);

        CapacityFinalCalleeAlternateFinalFrameGatePlan equalNonzero =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus14: 0x81234567,
                EntryECX: 0x81234567));
        check("Equal nonzero also selects only the fallthrough destination",
            equalNonzero.CompareResult == 0
            && equalNonzero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalNonzero.JnzTaken == false
            && equalNonzero.JnzSelectedDestinationRawFileOffset == 0x012C9167
            && equalNonzero.ExactCleanExitAdmitted
            && equalNonzero.StopAtRawFileOffsetExclusive == 0x012C9167);

        CapacityFinalCalleeAlternateFinalFrameGatePlan unsignedBorrow =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x14,
                FreshFrameMinus14: 0,
                EntryECX: 1));
        check("Wrapped subtraction reports unsigned borrow and all six CMP flags",
            unsignedBorrow.FrameMinus14Address == 0
            && unsignedBorrow.CompareResult == 0xFFFFFFFF
            && unsignedBorrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedBorrow.JnzTaken == true
            && unsignedBorrow.JnzSelectedDestinationRawFileOffset == 0x012C917C
            && unsignedBorrow.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget
            && unsignedBorrow.ExactCleanExitAdmitted
            && unsignedBorrow.StopAtRawFileOffsetExclusive == 0x012C917C
            && unsignedBorrow.Writes.IsEmpty);

        CapacityFinalCalleeAlternateFinalFrameGatePlan overflowAndOddParity =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x80000000,
                EntryECX: 0x7FFFFFFF));
        check("Signed overflow, auxiliary carry, and odd low-byte parity are modeled",
            overflowAndOddParity.CompareResult == 1
            && overflowAndOddParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflowAndOddParity.JnzTaken == true
            && overflowAndOddParity.JnzSelectedDestinationRawFileOffset == 0x012C917C);

        CapacityFinalCalleeAlternateFinalFrameGatePlan independentFreshFrame =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: 0x12345678,
                FreshFrameMinus14: 0x87654321,
                EntryECX: 0x12345678));
        check("CMP uses the fresh EBP-14 capture rather than a prior store",
            independentFreshFrame.FrameMinus14Address == 0x12345664
            && independentFreshFrame.FrameMinus14 == 0x87654321
            && independentFreshFrame.CompareLeft == 0x87654321
            && independentFreshFrame.CompareRight == 0x12345678
            && independentFreshFrame.CompareResult
                == unchecked(0x87654321u - 0x12345678u)
            && independentFreshFrame.JnzSelectedDestinationRawFileOffset == 0x012C917C);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual) []
        {
            (0x012C9160, "8B 45 EC", null, null),
            (0x012C9163, "3B C1", null, null),
            (0x012C9165, "75 15", 0x012C917C, 0x016C9D7C)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateFinalFrameGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateFinalFrameGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateFinalFrameGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateFinalFrameGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlternateFinalFrameGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateFinalFrameGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateFinalFrameGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateFinalFrameGateProjection.Instructions
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
