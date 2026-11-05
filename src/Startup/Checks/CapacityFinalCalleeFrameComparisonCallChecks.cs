using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFrameComparisonCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The standalone raw window and its encoded call-cell operand match",
            CapacityFinalCalleeFrameComparisonCallProjection.EntryRawFileOffset
                == 0x012C8E7C
            && CapacityFinalCalleeFrameComparisonCallProjection.EntryVirtualAddress
                == 0x016C9A7C
            && CapacityFinalCalleeFrameComparisonCallProjection.BranchRawFileOffset
                == 0x012C8E7F
            && CapacityFinalCalleeFrameComparisonCallProjection.CallRawFileOffset
                == 0x012C8E81
            && CapacityFinalCalleeFrameComparisonCallProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8E87
            && CapacityFinalCalleeFrameComparisonCallProjection.BranchDestinationRawFileOffset
                == 0x012C8E87
            && CapacityFinalCalleeFrameComparisonCallProjection.BranchDestinationVirtualAddress
                == 0x016C9A87
            && CapacityFinalCalleeFrameComparisonCallProjection.CallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeFrameComparisonCallProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFrameComparisonCallProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFrameComparisonCallProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeFrameComparisonCallProjection.CapturedWindowBytes)
                == "3B75FC7506FF15244AAC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeFrameComparisonCallPlan invalidEntry =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                captureValidatedEntry: false,
                freshFrameMinus4: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("An unvalidated entry performs no fresh read and treats all captures as unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.EntryNotValidated
            && invalidEntry.FreshFrameMinus4Address is null
            && !invalidEntry.FreshFrameMinus4ReadReached
            && !invalidEntry.FreshFrameMinus4Captured
            && invalidEntry.CompareFlags is null
            && !invalidEntry.BranchReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedFreshFrameMinus4CaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8E7C);

        CapacityFinalCalleeFrameComparisonCallPlan missingFreshRead =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                entryEbp: 0,
                freshFrameMinus4: null,
                callCellTarget: 0x1000,
                callReturnedNormally: true));
        check("The fresh DWORD read is required before CMP can produce flags or branch",
            missingFreshRead.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.MissingFreshFrameMinus4
            && missingFreshRead.FreshFrameMinus4Address == 0xFFFFFFFC
            && missingFreshRead.FreshFrameMinus4ReadReached
            && !missingFreshRead.FreshFrameMinus4Captured
            && missingFreshRead.CompareResult is null
            && missingFreshRead.CompareFlags is null
            && !missingFreshRead.BranchReached
            && !missingFreshRead.CallCellReadReached
            && missingFreshRead.UnreachedCallCellTargetCaptureProvided
            && missingFreshRead.UnreachedCallReturnStatusProvided
            && missingFreshRead.StopAtRawFileOffsetExclusive == 0x012C8E7C);

        CapacityFinalCalleeFrameComparisonCallPlan mismatch =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                entryEsi: 0x80000000,
                entryEbp: 4,
                entryEsp: 0x7000,
                freshFrameMinus4: 0x7FFFFFFF,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("A DWORD mismatch takes JNZ with exact unsigned and signed CMP relations",
            mismatch.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && mismatch.FreshFrameMinus4Address == 0
            && mismatch.FreshFrameMinus4 == 0x7FFFFFFF
            && mismatch.CompareResult == 1
            && mismatch.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && mismatch.Equal == false
            && mismatch.UnsignedBelow == false
            && mismatch.SignedLess == true
            && mismatch.BranchReached
            && mismatch.BranchTaken == true
            && mismatch.SelectedDestinationRawFileOffset == 0x012C8E87
            && !mismatch.CallCellReadReached
            && !mismatch.CallReached
            && mismatch.UnreachedCallCellTargetCaptureProvided
            && mismatch.UnreachedCallReturnStatusProvided
            && mismatch.StopAtRawFileOffsetExclusive == 0x012C8E87);

        CapacityFinalCalleeFrameComparisonCallPlan unsignedBelow =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                entryEsi: 0x7FFFFFFF,
                freshFrameMinus4: 0x80000000));
        check("Unsigned-below and signed-not-less are independently reflected by CMP flags",
            unsignedBelow.CompareResult == 0xFFFFFFFF
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.UnsignedBelow == true
            && unsignedBelow.SignedLess == false
            && unsignedBelow.BranchTaken == true);

        CapacityFinalCalleeFrameComparisonCallPlan equal =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                entryEsi: 0,
                entryEbp: 3,
                entryEsp: 0xABCDEF00,
                freshFrameMinus4: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Equality falls through to a zero-target call without inventing post-call registers",
            equal.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.CallReturnedNormally
            && equal.FreshFrameMinus4Address == 0xFFFFFFFF
            && equal.FreshFrameMinus4Captured
            && equal.FreshFrameMinus4 == 0
            && equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.Equal == true
            && equal.UnsignedBelow == false
            && equal.SignedLess == false
            && equal.BranchReached
            && equal.BranchTaken == false
            && equal.SelectedDestinationRawFileOffset is null
            && equal.CallCellReadReached
            && equal.CallCellTargetCaptured
            && equal.CallCellTargetVirtualAddress == 0
            && equal.CallReached
            && equal.CallReturnedNormally == true
            && equal.PreCallESI == 0
            && equal.PreCallEBP == 3
            && equal.PreCallESP == 0xABCDEF00
            && equal.CallOutputsOpaque
            && equal.StopAtRawFileOffsetExclusive == 0x012C8E87);

        CapacityFinalCalleeFrameComparisonCallPlan missingCallCell =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                callCellTarget: null,
                callReturnedNormally: true));
        CapacityFinalCalleeFrameComparisonCallPlan missingReturnStatus =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                callCellTarget: 0x1000,
                callReturnedNormally: null));
        CapacityFinalCalleeFrameComparisonCallPlan nonreturning =
            CapacityFinalCalleeFrameComparisonCallProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                callCellTarget: 0,
                callReturnedNormally: false));
        check("Equal frames require a cell and captured return status; neither target nor nonreturn is inferred",
            missingCallCell.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.MissingCallCellTarget
            && missingCallCell.CallCellReadReached
            && !missingCallCell.CallCellTargetCaptured
            && !missingCallCell.CallReached
            && missingCallCell.UnreachedCallReturnStatusProvided
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C8E81
            && missingReturnStatus.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.MissingCallReturnStatus
            && missingReturnStatus.CallCellTargetCaptured
            && missingReturnStatus.CallCellTargetVirtualAddress == 0x1000
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.CallOutputsOpaque
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C8E81
            && nonreturning.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.CallDidNotReturnNormally
            && nonreturning.CallCellTargetCaptured
            && nonreturning.CallCellTargetVirtualAddress == 0
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.CallOutputsOpaque
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C8E81);
    }

    private static CapacityFinalCalleeFrameComparisonCallCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEsi = 0x12345678,
        uint entryEbp = 0x1234567C,
        uint entryEsp = 0x7000,
        uint? freshFrameMinus4 = 0x12345678,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedEntry,
            entryEsi,
            entryEbp,
            entryEsp,
            freshFrameMinus4,
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
            (0x012C8E7C, "3B 75 FC", null, null, null),
            (0x012C8E7F, "75 06", null, 0x012C8E87, 0x016C9A87),
            (0x012C8E81, "FF 15 24 4A AC 01", 0x01AC4A24, null, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeFrameComparisonCallProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFrameComparisonCallInstructionEvidence instruction
            in CapacityFinalCalleeFrameComparisonCallProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFrameComparisonCallProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeFrameComparisonCallProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFrameComparisonCallProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFrameComparisonCallProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFrameComparisonCallProjection.Instructions
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
