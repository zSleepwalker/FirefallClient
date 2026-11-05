using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeExitChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw instruction window and PE-VA operands remain distinct",
            CapacityFinalCalleeExitProjection.EntryRawFileOffset == 0x012C9441
            && CapacityFinalCalleeExitProjection.EntryVirtualAddress == 0x016CA041
            && CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C
            && CapacityFinalCalleeExitProjection.ReturnBoundaryVirtualAddressExclusive
                == 0x016CA05C
            && CapacityFinalCalleeExitProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeExitProjection.EntryRawFileOffset)
                == CapacityFinalCalleeExitProjection.EntryVirtualAddress
            && CapacityFinalCalleeExitProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeExitProjection.ReturnBoundaryVirtualAddressExclusive
            && CapacityFinalCalleeExitProjection.TestByteVirtualAddress == 0x01EDAF84
            && CapacityFinalCalleeExitProjection.PushedImmediateVirtualAddress == 0x01EDAF88
            && CapacityFinalCalleeExitProjection.CallCellVirtualAddress == 0x01AC427C
            && Normalize(CapacityFinalCalleeExitProjection.CapturedWindowBytes)
                == "F60584AFED01025F5E5B740B6888AFED01FF157C42AC018BE55DC3"
            && CapacityFinalCalleeExitProjection.Instructions.Sum(
                instruction => instruction.Bytes.Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries).Length) == 27
            && InstructionEvidenceMatches());

        CapacityFinalCalleeExitPlan invalidEntry =
            CapacityFinalCalleeExitProjection.Project(Capture(
                entryValidated: false,
                testedByte: 2,
                popEdi: 0x10,
                popEsi: 0x20,
                popEbx: 0x30,
                callTarget: 0x1000,
                callReturnedNormally: true,
                outputEbx: 1,
                outputEdi: 2,
                outputEsi: 3,
                popEbp: 0x40));
        check("An unvalidated entry preserves supplied values but marks all later captures unreached",
            invalidEntry.Outcome == CapacityFinalCalleeExitOutcome.EntryNotValidated
            && !invalidEntry.TestReadReached
            && invalidEntry.TestedByte is null
            && !invalidEntry.StackPopEdiReached
            && invalidEntry.PoppedEdi is null
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.ReturnReached
            && invalidEntry.UnreachedTestedByteCaptureProvided
            && invalidEntry.UnreachedStackPopEdiCaptureProvided
            && invalidEntry.UnreachedStackPopEsiCaptureProvided
            && invalidEntry.UnreachedStackPopEbxCaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.UnreachedCalleeOutputEbxCaptureProvided
            && invalidEntry.UnreachedCalleeOutputEdiCaptureProvided
            && invalidEntry.UnreachedCalleeOutputEsiCaptureProvided
            && invalidEntry.UnreachedStackPopEbpCaptureProvided
            && invalidEntry.Capture.StackPopEsiValue == 0x20
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeExitPlan zero =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 0,
                popEdi: 0x1111,
                popEsi: 0x2222,
                popEbx: 0x3333,
                popEbp: 0x4444));
        CapacityFinalCalleeExitPlan bitClearWithCallCapture =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 0xFD,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0,
                callReturnedNormally: true,
                outputEbx: 8,
                popEbp: 4));
        check("Zero and bit-clear TEST values skip the call after preserving each earlier POP",
            zero.Outcome == CapacityFinalCalleeExitOutcome.CallSkipped
            && zero.TestedByte == 0
            && zero.TestedBitIsSet == false
            && zero.PoppedEdi == 0x1111
            && zero.PoppedEsi == 0x2222
            && zero.PoppedEbx == 0x3333
            && zero.BranchReached
            && zero.BranchTaken == true
            && !zero.PushImmediateReached
            && !zero.CallCellReadReached
            && !zero.CallReached
            && !zero.CalleeOutputRegistersReached
            && zero.MovEspFromEbpReached
            && zero.StackPopEbpReached
            && zero.PoppedEbp == 0x4444
            && zero.ReturnReached
            && zero.StopAtRawFileOffsetExclusive == 0x012C945C
            && bitClearWithCallCapture.Outcome
                == CapacityFinalCalleeExitOutcome.CallSkipped
            && bitClearWithCallCapture.TestedBitIsSet == false
            && bitClearWithCallCapture.UnreachedCallCellTargetCaptureProvided
            && bitClearWithCallCapture.UnreachedCallReturnStatusProvided
            && bitClearWithCallCapture.UnreachedCalleeOutputEbxCaptureProvided
            && bitClearWithCallCapture.PoppedEdi == 1
            && bitClearWithCallCapture.PoppedEbx == 3
            && bitClearWithCallCapture.ReturnReached);

        CapacityFinalCalleeExitPlan bitTwo =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 0xA,
                popEsi: 0xB,
                popEbx: 0xC,
                callTarget: 0x12345678,
                callReturnedNormally: true,
                outputEbx: 0xDEADBEEF,
                outputEdi: 0xD1D1D1D1,
                outputEsi: 0x51515151,
                popEbp: 0xEEEEEEEE));
        CapacityFinalCalleeExitPlan allBitsSet =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 0xFF,
                popEdi: 0xA,
                popEsi: 0xB,
                popEbx: 0xC,
                callTarget: 0x87654321,
                callReturnedNormally: true,
                outputEbx: 0xDEADBEEF,
                outputEdi: 0xD1D1D1D1,
                outputEsi: 0x51515151,
                popEbp: 0xEEEEEEEE));
        check("Bit-set values require the call and expose independent opaque output-register captures",
            bitTwo.Outcome == CapacityFinalCalleeExitOutcome.ReturnedNormally
            && bitTwo.TestedByte == 2
            && bitTwo.TestedBitIsSet == true
            && bitTwo.BranchTaken == false
            && bitTwo.PushImmediateReached
            && bitTwo.PushedImmediateVirtualAddress == 0x01EDAF88
            && bitTwo.CallCellReadReached
            && bitTwo.CallCellTargetVirtualAddress == 0x12345678
            && bitTwo.CallReached
            && bitTwo.CallReturnedNormally == true
            && bitTwo.CalleeOutputRegistersReached
            && bitTwo.CalleeOutputEbx == 0xDEADBEEF
            && bitTwo.CalleeOutputEdi == 0xD1D1D1D1
            && bitTwo.CalleeOutputEsi == 0x51515151
            && bitTwo.CalleeOutputEbx != bitTwo.PoppedEbx
            && bitTwo.CalleeOutputEdi != bitTwo.PoppedEdi
            && bitTwo.CalleeOutputEsi != bitTwo.PoppedEsi
            && bitTwo.ReturnReached
            && allBitsSet.TestedByte == 0xFF
            && allBitsSet.TestedBitIsSet == true
            && allBitsSet.CallReturnedNormally == true
            && allBitsSet.ReturnReached);

        CapacityFinalCalleeExitPlan missingByte =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: null,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 4,
                callReturnedNormally: true,
                popEbp: 5));
        CapacityFinalCalleeExitPlan missingFirstPop =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: null,
                popEsi: 2,
                popEbx: 3,
                callTarget: 4,
                callReturnedNormally: true,
                popEbp: 5));
        CapacityFinalCalleeExitPlan missingMiddlePop =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 0x1111,
                popEsi: null,
                popEbx: 3,
                callTarget: 4,
                callReturnedNormally: true,
                popEbp: 5));
        CapacityFinalCalleeExitPlan missingLastPop =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 0x1111,
                popEsi: 0x2222,
                popEbx: null,
                callTarget: 4,
                callReturnedNormally: true,
                popEbp: 5));
        check("Missing byte or POP captures stop in order and preserve prior completed events",
            missingByte.Outcome == CapacityFinalCalleeExitOutcome.MissingTestedByte
            && missingByte.TestReadReached
            && !missingByte.TestedByteCaptured
            && missingByte.UnreachedStackPopEdiCaptureProvided
            && missingByte.UnreachedCallCellTargetCaptureProvided
            && missingByte.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingFirstPop.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEdiValue
            && missingFirstPop.TestedBitIsSet == true
            && missingFirstPop.StackPopEdiReached
            && missingFirstPop.UnreachedStackPopEsiCaptureProvided
            && missingFirstPop.UnreachedCallReturnStatusProvided
            && missingFirstPop.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingMiddlePop.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEsiValue
            && missingMiddlePop.PoppedEdi == 0x1111
            && missingMiddlePop.StackPopEsiReached
            && missingMiddlePop.UnreachedStackPopEbxCaptureProvided
            && missingMiddlePop.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingLastPop.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEbxValue
            && missingLastPop.PoppedEdi == 0x1111
            && missingLastPop.PoppedEsi == 0x2222
            && missingLastPop.StackPopEbxReached
            && missingLastPop.UnreachedCallCellTargetCaptureProvided
            && missingLastPop.UnreachedStackPopEbpCaptureProvided
            && missingLastPop.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeExitPlan missingCell =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: null,
                callReturnedNormally: true,
                outputEbx: 7,
                popEbp: 4));
        CapacityFinalCalleeExitPlan zeroTarget =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0,
                callReturnedNormally: true,
                outputEbx: 0,
                popEbp: 4));
        CapacityFinalCalleeExitPlan missingReturnStatus =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0x1000,
                callReturnedNormally: null,
                popEbp: 4));
        check("The pushed argument precedes the call-cell read; zero target is a captured target",
            missingCell.Outcome == CapacityFinalCalleeExitOutcome.MissingCallCellTarget
            && missingCell.PushImmediateReached
            && missingCell.PushedImmediateVirtualAddress == 0x01EDAF88
            && missingCell.CallCellReadReached
            && !missingCell.CallCellTargetCaptured
            && !missingCell.CallReached
            && missingCell.UnreachedCallReturnStatusProvided
            && missingCell.UnreachedCalleeOutputEbxCaptureProvided
            && !missingCell.MovEspFromEbpReached
            && missingCell.StopAtRawFileOffsetExclusive == 0x012C9452
            && zeroTarget.Outcome == CapacityFinalCalleeExitOutcome.ReturnedNormally
            && zeroTarget.CallCellTargetCaptured
            && zeroTarget.CallCellTargetVirtualAddress == 0
            && zeroTarget.CallReturnedNormally == true
            && zeroTarget.CalleeOutputRegistersReached
            && zeroTarget.CalleeOutputEbx == 0
            && zeroTarget.ReturnReached
            && missingReturnStatus.Outcome
                == CapacityFinalCalleeExitOutcome.MissingCallReturnStatus
            && missingReturnStatus.CallCellTargetVirtualAddress == 0x1000
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && !missingReturnStatus.MovEspFromEbpReached
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C9452);

        CapacityFinalCalleeExitPlan nonreturning =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0x1234,
                callReturnedNormally: false,
                outputEbx: 7,
                outputEdi: 8,
                outputEsi: 9,
                popEbp: 4));
        CapacityFinalCalleeExitPlan missingEbp =
            CapacityFinalCalleeExitProjection.Project(Capture(
                testedByte: 0,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                popEbp: null));
        check("A nonreturning callee blocks the epilogue, while RET requires the final POP capture",
            nonreturning.Outcome
                == CapacityFinalCalleeExitOutcome.CallDidNotReturnNormally
            && nonreturning.CallReturnedNormally == false
            && !nonreturning.CalleeOutputRegistersReached
            && nonreturning.UnreachedCalleeOutputEbxCaptureProvided
            && !nonreturning.MovEspFromEbpReached
            && !nonreturning.ReturnReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingEbp.Outcome == CapacityFinalCalleeExitOutcome.MissingStackPopEbpValue
            && missingEbp.MovEspFromEbpReached
            && missingEbp.StackPopEbpReached
            && missingEbp.PoppedEbp is null
            && !missingEbp.ReturnReached
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C945A);
    }

    private static CapacityFinalCalleeExitCapture Capture(
        bool entryValidated = true,
        byte? testedByte = 0,
        uint? popEdi = 0x1000,
        uint? popEsi = 0x2000,
        uint? popEbx = 0x3000,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? outputEbx = null,
        uint? outputEdi = null,
        uint? outputEsi = null,
        uint? popEbp = 0x4000) =>
        new(
            entryValidated,
            testedByte,
            popEdi,
            popEsi,
            popEbx,
            callTarget,
            callReturnedNormally,
            outputEbx,
            outputEdi,
            outputEsi,
            popEbp);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            byte? ImmediateValue,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9441, "F6 05 84 AF ED 01 02", 0x01EDAF84, 0x02, null, null),
            (0x012C9448, "5F", null, null, null, null),
            (0x012C9449, "5E", null, null, null, null),
            (0x012C944A, "5B", null, null, null, null),
            (0x012C944B, "74 0B", null, null, 0x012C9458, 0x016CA058),
            (0x012C944D, "68 88 AF ED 01", 0x01EDAF88, null, null, null),
            (0x012C9452, "FF 15 7C 42 AC 01", 0x01AC427C, null, null, null),
            (0x012C9458, "8B E5", null, null, null, null),
            (0x012C945A, "5D", null, null, null, null),
            (0x012C945B, "C3", null, null, null, null)
        };
        uint nextRawOffset = CapacityFinalCalleeExitProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeExitInstructionEvidence instruction
            in CapacityFinalCalleeExitProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeExitProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeExitProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive
            && bytes == Normalize(CapacityFinalCalleeExitProjection.CapturedWindowBytes)
            && CapacityFinalCalleeExitProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.ImmediateValue,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
