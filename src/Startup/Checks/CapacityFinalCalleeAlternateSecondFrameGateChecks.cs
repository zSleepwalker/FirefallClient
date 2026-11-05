using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateSecondFrameGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded CMP/JNZ window and both destinations match",
            CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryRawFileOffset
                == 0x012C9122
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryVirtualAddress
                == 0x016C9D22
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9127
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.OpaqueCallRawFileOffset
                == 0x012C9127
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.OpaqueCallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.EqualDestinationRawFileOffset
                == 0x012C9127
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.UnequalDestinationRawFileOffset
                == 0x012C912D
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryRawFileOffset)
                == 0x016C9D22
            && Normalize(CapacityFinalCalleeAlternateSecondFrameGateProjection.CapturedWindowBytes)
                == "3B55F07506"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateSecondFrameGatePlan missingEdx =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: null,
                EntryEBP: 0x100,
                FreshFrameMinus10: 0));
        check("Missing EDX gates this CMP and reports supplied later values",
            missingEdx.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEDX
            && !missingEdx.EntryEDXCaptured
            && !missingEdx.EntryEBPCaptured
            && !missingEdx.FrameMinus10ReadReached
            && missingEdx.FrameMinus10Address is null
            && !missingEdx.CompareReached
            && !missingEdx.JnzReached
            && missingEdx.UnreachedEntryEBPCaptureProvided
            && missingEdx.LateFreshFrameMinus10CaptureProvided
            && !missingEdx.ExactCleanExitAdmitted
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C9122
            && missingEdx.Writes.IsEmpty
            && !missingEdx.OpaqueCallReached);

        CapacityFinalCalleeAlternateSecondFrameGatePlan missingEbp =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBP: null,
                FreshFrameMinus10: 0));
        check("Missing EBP gates the frame operand read and marks its capture late",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEBP
            && missingEbp.EntryEDXCaptured
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FrameMinus10ReadReached
            && missingEbp.FrameMinus10Address is null
            && !missingEbp.FrameMinus10Captured
            && !missingEbp.CompareReached
            && !missingEbp.JnzReached
            && !missingEbp.UnreachedEntryEBPCaptureProvided
            && missingEbp.LateFreshFrameMinus10CaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleeAlternateSecondFrameGatePlan missingFresh =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshFrameMinus10: null));
        check("Missing fresh frame value reaches only its own CMP operand read",
            missingFresh.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingFreshFrameMinus10
            && missingFresh.EntryEDXCaptured
            && missingFresh.EntryEBPCaptured
            && missingFresh.FrameMinus10ReadReached
            && missingFresh.FrameMinus10Address == 0x1F0
            && !missingFresh.FrameMinus10Captured
            && missingFresh.FrameMinus10 is null
            && !missingFresh.CompareReached
            && !missingFresh.JnzReached
            && !missingFresh.LateFreshFrameMinus10CaptureProvided
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleeAlternateSecondFrameGatePlan equalZero =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0,
                EntryEBP: 0,
                FreshFrameMinus10: 0));
        check("Equal zero falls through to the opaque boundary without entering it",
            equalZero.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.EqualToOpaqueCallBoundary
            && equalZero.FrameMinus10Address == 0xFFFFFFF0
            && equalZero.FrameMinus10Captured
            && equalZero.FrameMinus10 == 0
            && equalZero.CompareReached
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
            && equalZero.JnzSelectedDestinationRawFileOffset == 0x012C9127
            && equalZero.ExactCleanExitAdmitted
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C9127
            && equalZero.Writes.IsEmpty
            && !equalZero.OpaqueCallReached);

        CapacityFinalCalleeAlternateSecondFrameGatePlan equalNonzero =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0x81234567,
                EntryEBP: 0x300,
                FreshFrameMinus10: 0x81234567));
        check("Equal nonzero also selects the exact fallthrough destination",
            equalNonzero.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.EqualToOpaqueCallBoundary
            && equalNonzero.CompareResult == 0
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
            && equalNonzero.JnzSelectedDestinationRawFileOffset == 0x012C9127
            && equalNonzero.ExactCleanExitAdmitted
            && equalNonzero.Writes.IsEmpty
            && !equalNonzero.OpaqueCallReached);

        CapacityFinalCalleeAlternateSecondFrameGatePlan wrapped =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0,
                EntryEBP: 0xF,
                FreshFrameMinus10: 1));
        check("Unequal wrapped subtraction preserves all six DWORD CMP flags",
            CapacityFinalCalleeAlternateSecondFrameGateProjection.DeriveEbpMinus10Address(0xF)
                == 0xFFFFFFFF
            && wrapped.FrameMinus10Address == 0xFFFFFFFF
            && wrapped.CompareResult == 0xFFFFFFFF
            && wrapped.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrapped.JnzTaken == true
            && wrapped.JnzSelectedDestinationRawFileOffset == 0x012C912D
            && wrapped.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.UnequalToBranchTarget
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C912D
            && wrapped.Writes.IsEmpty
            && !wrapped.OpaqueCallReached);

        CapacityFinalCalleeAlternateSecondFrameGatePlan overflowAndOddParity =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0x80000000,
                EntryEBP: 0x20,
                FreshFrameMinus10: 0x7FFFFFFF));
        check("Signed overflow, odd low-byte parity, and auxiliary borrow are modeled",
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
            && overflowAndOddParity.JnzSelectedDestinationRawFileOffset == 0x012C912D);

        CapacityFinalCalleeAlternateSecondFrameGatePlan independentFreshFrame =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: 0x12345678,
                EntryEBP: 0x12345678,
                FreshFrameMinus10: 0x87654321));
        check("CMP uses the independent fresh frame capture rather than a prior store",
            independentFreshFrame.FrameMinus10Address == 0x12345668
            && independentFreshFrame.FrameMinus10 == 0x87654321
            && independentFreshFrame.CompareLeft == 0x12345678
            && independentFreshFrame.CompareRight == 0x87654321
            && independentFreshFrame.CompareResult
                == unchecked(0x12345678u - 0x87654321u)
            && independentFreshFrame.JnzSelectedDestinationRawFileOffset == 0x012C912D);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9122, "3B 55 F0", null, null),
            (0x012C9125, "75 06", 0x012C912D, 0x016C9D2D)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateSecondFrameGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateSecondFrameGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateSecondFrameGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateSecondFrameGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateSecondFrameGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateSecondFrameGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateSecondFrameGateProjection.Instructions
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
