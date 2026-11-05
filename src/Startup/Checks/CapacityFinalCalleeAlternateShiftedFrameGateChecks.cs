using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateShiftedFrameGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The shifted-frame gate window, mapping, and exact branches match",
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset
                == 0x012C912D
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryVirtualAddress
                == 0x016C9D2D
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9137
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.OpaqueCallRawFileOffset
                == 0x012C9137
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.OpaqueCallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.EqualDestinationRawFileOffset
                == 0x012C913D
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.UnequalDestinationRawFileOffset
                == 0x012C9137
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeAlternateShiftedFrameGateProjection.CapturedWindowBytes)
                == "8B45FCC1E0033BC87406"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateShiftedFrameGatePlan missingEbp =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus4: 0x20000000,
                EntryECX: 0));
        check("Missing EBP gates the fresh read and marks supplied operands late",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryEBP
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FrameMinus4ReadReached
            && missingEbp.FrameMinus4Address is null
            && !missingEbp.FrameMinus4Captured
            && !missingEbp.ShiftReached
            && missingEbp.ShiftResult is null
            && !missingEbp.CompareReached
            && !missingEbp.JzReached
            && missingEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingEbp.UnreachedEntryECXCaptureProvided
            && missingEbp.LateFreshFrameMinus4CaptureProvided
            && missingEbp.LateEntryECXCaptureProvided
            && !missingEbp.ExactCleanExitAdmitted
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C912D
            && missingEbp.Writes.IsEmpty
            && !missingEbp.OpaqueCallReached);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan missingFrame =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 2,
                FreshFrameMinus4: null,
                EntryECX: 0x100));
        check("Missing fresh EBP-4 data stops at its read and marks ECX late",
            missingFrame.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingFreshFrameMinus4
            && missingFrame.EntryEBPCaptured
            && missingFrame.FrameMinus4ReadReached
            && missingFrame.FrameMinus4Address == 0xFFFFFFFE
            && !missingFrame.FrameMinus4Captured
            && missingFrame.FrameMinus4 is null
            && !missingFrame.ShiftReached
            && missingFrame.ShiftResult is null
            && !missingFrame.EntryECXCaptured
            && !missingFrame.CompareReached
            && !missingFrame.JzReached
            && !missingFrame.UnreachedFreshFrameMinus4CaptureProvided
            && missingFrame.UnreachedEntryECXCaptureProvided
            && missingFrame.LateEntryECXCaptureProvided
            && !missingFrame.ExactCleanExitAdmitted
            && missingFrame.StopAtRawFileOffsetExclusive == 0x012C912D);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan missingEcx =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0x104,
                FreshFrameMinus4: 0x20000000,
                EntryECX: null));
        check("Missing ECX stops at CMP after the independent fresh value is shifted",
            missingEcx.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryECX
            && missingEcx.EntryEBPCaptured
            && missingEcx.FrameMinus4ReadReached
            && missingEcx.FrameMinus4Address == 0x100
            && missingEcx.FrameMinus4Captured
            && missingEcx.FrameMinus4 == 0x20000000
            && missingEcx.ShiftReached
            && missingEcx.ShiftInput == 0x20000000
            && missingEcx.ShiftResult == 0
            && missingEcx.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && !missingEcx.EntryECXCaptured
            && !missingEcx.CompareReached
            && missingEcx.CompareResult is null
            && !missingEcx.JzReached
            && !missingEcx.LateEntryECXCaptureProvided
            && !missingEcx.ExactCleanExitAdmitted
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9133);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan zeroEquality =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus4: 0,
                EntryECX: 0));
        check("Zero shifted value compares equal and takes JZ with defined shift flags",
            zeroEquality.FrameMinus4Address == 0xFFFFFFFC
            && zeroEquality.ShiftResult == 0
            && zeroEquality.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && zeroEquality.CompareLeft == 0
            && zeroEquality.CompareRight == 0
            && zeroEquality.CompareResult == 0
            && zeroEquality.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroEquality.JzReached
            && zeroEquality.JzTaken == true
            && zeroEquality.JzSelectedDestinationRawFileOffset == 0x012C913D
            && zeroEquality.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.EqualToBranchTarget
            && zeroEquality.ExactCleanExitAdmitted
            && zeroEquality.StopAtRawFileOffsetExclusive == 0x012C913D
            && zeroEquality.Writes.IsEmpty
            && !zeroEquality.OpaqueCallReached);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan shiftedWrapEquality =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0x40,
                FreshFrameMinus4: 0xE0000001,
                EntryECX: 8));
        check("SHL wraps at 32 bits and CF is the original bit 29",
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.ShiftLeftByThree(
                0xE0000001) == 8
            && shiftedWrapEquality.ShiftResult == 8
            && shiftedWrapEquality.ShiftFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && shiftedWrapEquality.CompareResult == 0
            && shiftedWrapEquality.JzTaken == true
            && shiftedWrapEquality.JzSelectedDestinationRawFileOffset == 0x012C913D
            && shiftedWrapEquality.ExactCleanExitAdmitted);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan unequal =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus4: 1,
                EntryECX: 0));
        check("Unequal CMP flags select the opaque call boundary without entering it",
            unequal.ShiftResult == 8
            && unequal.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && unequal.CompareResult == 0xFFFFFFF8
            && unequal.CompareFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unequal.JzTaken == false
            && unequal.JzSelectedDestinationRawFileOffset == 0x012C9137
            && unequal.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.UnequalToOpaqueCallBoundary
            && unequal.ExactCleanExitAdmitted
            && unequal.StopAtRawFileOffsetExclusive == 0x012C9137
            && unequal.Writes.IsEmpty
            && !unequal.OpaqueCallReached);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan signAndOverflow =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus4: 0xF0000000,
                EntryECX: 0x7FFFFFFF));
        check("Shift sign and CMP signed overflow are modeled while undefined shift flags stay null",
            signAndOverflow.ShiftResult == 0x80000000
            && signAndOverflow.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: null
            }
            && signAndOverflow.CompareResult == 0xFFFFFFFF
            && signAndOverflow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && signAndOverflow.JzTaken == false
            && signAndOverflow.JzSelectedDestinationRawFileOffset == 0x012C9137);

        CapacityFinalCalleeAlternateShiftedFrameGatePlan independentFreshValue =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.Project(new(
                EntryEBP: 0x200,
                FreshFrameMinus4: 0x12345678,
                EntryECX: 0x91A2B3C0));
        check("The CMP uses the separately captured fresh EBP-4 value",
            independentFreshValue.FrameMinus4Address == 0x1FC
            && independentFreshValue.FrameMinus4 == 0x12345678
            && independentFreshValue.ShiftResult == 0x91A2B3C0
            && independentFreshValue.CompareLeft == 0x91A2B3C0
            && independentFreshValue.CompareRight == 0x91A2B3C0
            && independentFreshValue.CompareResult == 0
            && independentFreshValue.JzSelectedDestinationRawFileOffset == 0x012C913D);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual) []
        {
            (0x012C912D, "8B 45 FC", null, null),
            (0x012C9130, "C1 E0 03", null, null),
            (0x012C9133, "3B C8", null, null),
            (0x012C9135, "74 06", 0x012C913D, 0x016C9D3D)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateShiftedFrameGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateShiftedFrameGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateShiftedFrameGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateShiftedFrameGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateShiftedFrameGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlternateShiftedFrameGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateShiftedFrameGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateShiftedFrameGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateShiftedFrameGateProjection.Instructions
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
