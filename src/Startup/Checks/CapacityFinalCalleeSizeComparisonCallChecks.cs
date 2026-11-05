using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSizeComparisonCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw size-comparison window, branch target, and call-cell operand match",
            CapacityFinalCalleeSizeComparisonCallProjection.EntryRawFileOffset
                == 0x012C8E87
            && CapacityFinalCalleeSizeComparisonCallProjection.EntryVirtualAddress
                == 0x016C9A87
            && CapacityFinalCalleeSizeComparisonCallProjection.MaskRawFileOffset
                == 0x012C8E8A
            && CapacityFinalCalleeSizeComparisonCallProjection.ShiftRawFileOffset
                == 0x012C8E8D
            && CapacityFinalCalleeSizeComparisonCallProjection.CompareRawFileOffset
                == 0x012C8E90
            && CapacityFinalCalleeSizeComparisonCallProjection.BranchRawFileOffset
                == 0x012C8E92
            && CapacityFinalCalleeSizeComparisonCallProjection.CallRawFileOffset
                == 0x012C8E94
            && CapacityFinalCalleeSizeComparisonCallProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8E9A
            && CapacityFinalCalleeSizeComparisonCallProjection.BranchDestinationRawFileOffset
                == 0x012C8E9A
            && CapacityFinalCalleeSizeComparisonCallProjection.BranchDestinationVirtualAddress
                == 0x016C9A9A
            && CapacityFinalCalleeSizeComparisonCallProjection.CallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeSizeComparisonCallProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeSizeComparisonCallProjection.EntryRawFileOffset)
                == CapacityFinalCalleeSizeComparisonCallProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeSizeComparisonCallProjection.CapturedWindowBytes)
                == "8B4E0483E1F8C1E0033BC87406FF15244AAC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeSizeComparisonCallPlan invalidEntry =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                captureValidatedEntry: false,
                freshEsiPlus4: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("An unvalidated entry does not use or derive any later captures",
            invalidEntry.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EntryNotValidated
            && invalidEntry.FreshEsiPlus4Address is null
            && !invalidEntry.FreshEsiPlus4ReadReached
            && !invalidEntry.FreshEsiPlus4Captured
            && !invalidEntry.MaskReached
            && invalidEntry.MaskedFreshEsiPlus4 is null
            && !invalidEntry.ShiftReached
            && invalidEntry.ShiftedEntryEax is null
            && !invalidEntry.CompareReached
            && invalidEntry.CompareFlags is null
            && !invalidEntry.BranchReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedFreshEsiPlus4CaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8E87);

        CapacityFinalCalleeSizeComparisonCallPlan missingFreshRead =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                entryEsi: 0xFFFFFFFC,
                freshEsiPlus4: null,
                callCellTarget: 0x1000,
                callReturnedNormally: true));
        check("The fresh ESI+4 read is required before masking, shifting, or comparing",
            missingFreshRead.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingFreshEsiPlus4
            && missingFreshRead.FreshEsiPlus4Address == 0
            && missingFreshRead.FreshEsiPlus4ReadReached
            && !missingFreshRead.FreshEsiPlus4Captured
            && !missingFreshRead.MaskReached
            && missingFreshRead.MaskedFreshEsiPlus4 is null
            && !missingFreshRead.ShiftReached
            && missingFreshRead.ShiftedEntryEax is null
            && !missingFreshRead.CompareReached
            && missingFreshRead.CompareFlags is null
            && !missingFreshRead.BranchReached
            && !missingFreshRead.CallCellReadReached
            && !missingFreshRead.CallReached
            && missingFreshRead.UnreachedCallCellTargetCaptureProvided
            && missingFreshRead.UnreachedCallReturnStatusProvided
            && missingFreshRead.StopAtRawFileOffsetExclusive == 0x012C8E87);

        CapacityFinalCalleeSizeComparisonCallPlan capturedZero =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                entryEsi: 0xFFFFFFFC,
                entryEax: 0,
                entryEsp: 0xABCDEF00,
                freshEsiPlus4: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("A captured zero is distinct from a missing read and equal sizes bypass the call",
            capturedZero.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && capturedZero.FreshEsiPlus4Address == 0
            && capturedZero.FreshEsiPlus4ReadReached
            && capturedZero.FreshEsiPlus4Captured
            && capturedZero.FreshEsiPlus4 == 0
            && capturedZero.MaskReached
            && capturedZero.MaskedFreshEsiPlus4 == 0
            && capturedZero.ShiftReached
            && capturedZero.ShiftedEntryEax == 0
            && capturedZero.CompareReached
            && capturedZero.CompareResult == 0
            && capturedZero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && capturedZero.Equal == true
            && capturedZero.UnsignedBelow == false
            && capturedZero.SignedLess == false
            && capturedZero.BranchReached
            && capturedZero.BranchTaken == true
            && capturedZero.SelectedDestinationRawFileOffset == 0x012C8E9A
            && !capturedZero.CallCellReadReached
            && !capturedZero.CallReached
            && capturedZero.UnreachedCallCellTargetCaptureProvided
            && capturedZero.UnreachedCallReturnStatusProvided
            && capturedZero.PreCallESI is null
            && capturedZero.PreCallEAX is null
            && capturedZero.PreCallESP is null
            && capturedZero.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeSizeComparisonCallPlan wrappedShiftAndMask =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                entryEsi: 0x1000,
                entryEax: 0xE0000001,
                freshEsiPlus4: 0x0000000F));
        check("AND clears low bits and SHL independently truncates entry EAX to a DWORD",
            wrappedShiftAndMask.FreshEsiPlus4Address == 0x1004
            && wrappedShiftAndMask.FreshEsiPlus4 == 0x0000000F
            && wrappedShiftAndMask.MaskedFreshEsiPlus4 == 0x00000008
            && wrappedShiftAndMask.ShiftedEntryEax == 0x00000008
            && wrappedShiftAndMask.CompareResult == 0
            && wrappedShiftAndMask.CompareFlags?.Zero == true
            && wrappedShiftAndMask.BranchTaken == true);

        CapacityFinalCalleeSizeComparisonCallPlan callReturned =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                entryEsi: 0x12345678,
                entryEax: 1,
                entryEsp: 0x7000,
                freshEsiPlus4: 0x21,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Unequal sizes preserve exact CMP flags and the call uses captured entry registers",
            callReturned.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally
            && callReturned.FreshEsiPlus4Address == 0x1234567C
            && callReturned.FreshEsiPlus4 == 0x21
            && callReturned.MaskedFreshEsiPlus4 == 0x20
            && callReturned.ShiftedEntryEax == 8
            && callReturned.CompareResult == 0x18
            && callReturned.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && callReturned.Equal == false
            && callReturned.UnsignedBelow == false
            && callReturned.SignedLess == false
            && callReturned.BranchReached
            && callReturned.BranchTaken == false
            && callReturned.SelectedDestinationRawFileOffset is null
            && callReturned.CallCellReadReached
            && callReturned.CallCellTargetCaptured
            && callReturned.CallCellTargetVirtualAddress == 0
            && callReturned.CallReached
            && callReturned.CallReturnedNormally == true
            && callReturned.PreCallESI == 0x12345678
            && callReturned.PreCallEAX == 1
            && callReturned.PreCallESP == 0x7000
            && callReturned.CallOutputsOpaque
            && callReturned.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeSizeComparisonCallPlan missingCallCell =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                freshEsiPlus4: 0x21,
                callCellTarget: null,
                callReturnedNormally: true));
        CapacityFinalCalleeSizeComparisonCallPlan missingReturnStatus =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                freshEsiPlus4: 0x21,
                callCellTarget: 0x1000,
                callReturnedNormally: null));
        CapacityFinalCalleeSizeComparisonCallPlan nonreturning =
            CapacityFinalCalleeSizeComparisonCallProjection.Project(Capture(
                freshEsiPlus4: 0x21,
                callCellTarget: 0,
                callReturnedNormally: false));
        check("Call-cell absence, unknown return status, and nonreturn are distinct",
            missingCallCell.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallCellTarget
            && missingCallCell.FreshEsiPlus4Captured
            && missingCallCell.MaskedFreshEsiPlus4 == 0x20
            && missingCallCell.ShiftedEntryEax == 8
            && missingCallCell.CompareResult == 0x18
            && missingCallCell.CompareFlags is not null
            && missingCallCell.CallCellReadReached
            && !missingCallCell.CallCellTargetCaptured
            && !missingCallCell.CallReached
            && missingCallCell.UnreachedCallReturnStatusProvided
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C8E94
            && missingReturnStatus.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallReturnStatus
            && missingReturnStatus.FreshEsiPlus4Captured
            && missingReturnStatus.MaskedFreshEsiPlus4 == 0x20
            && missingReturnStatus.ShiftedEntryEax == 8
            && missingReturnStatus.CompareResult == 0x18
            && missingReturnStatus.CallCellTargetCaptured
            && missingReturnStatus.CallCellTargetVirtualAddress == 0x1000
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.CallOutputsOpaque
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C8E94
            && nonreturning.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.CallDidNotReturnNormally
            && nonreturning.FreshEsiPlus4Captured
            && nonreturning.MaskedFreshEsiPlus4 == 0x20
            && nonreturning.ShiftedEntryEax == 8
            && nonreturning.CompareResult == 0x18
            && nonreturning.CallCellTargetCaptured
            && nonreturning.CallCellTargetVirtualAddress == 0
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.CallOutputsOpaque
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C8E94);
    }

    private static CapacityFinalCalleeSizeComparisonCallCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEsi = 0x12345678,
        uint entryEax = 1,
        uint entryEsp = 0x7000,
        uint? freshEsiPlus4 = 0x21,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedEntry,
            entryEsi,
            entryEax,
            entryEsp,
            freshEsiPlus4,
            callCellTarget,
            callReturnedNormally);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8E87, "8B 4E 04", null, null, null),
            (0x012C8E8A, "83 E1 F8", null, null, null),
            (0x012C8E8D, "C1 E0 03", null, null, null),
            (0x012C8E90, "3B C8", null, null, null),
            (0x012C8E92, "74 06", null, 0x012C8E9A, 0x016C9A9A),
            (0x012C8E94, "FF 15 24 4A AC 01", 0x01AC4A24, null, null)
        };
        uint nextRawOffset = CapacityFinalCalleeSizeComparisonCallProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeSizeComparisonCallInstructionEvidence instruction
            in CapacityFinalCalleeSizeComparisonCallProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeSizeComparisonCallProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeSizeComparisonCallProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeSizeComparisonCallProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeSizeComparisonCallProjection.CapturedWindowBytes)
            && CapacityFinalCalleeSizeComparisonCallProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
