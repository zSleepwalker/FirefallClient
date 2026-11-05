using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeGatedEpilogueChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The byte window, branch target, and PE virtual-address operands match the observation",
            CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset == 0x012C9441
            && CapacityFinalCalleeGatedEpilogueProjection.EntryVirtualAddress == 0x016CA041
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C
            && CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryVirtualAddressExclusive
                == 0x016CA05C
            && CapacityFinalCalleeGatedEpilogueProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset)
                == CapacityFinalCalleeGatedEpilogueProjection.EntryVirtualAddress
            && CapacityFinalCalleeGatedEpilogueProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryVirtualAddressExclusive
            && CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress == 0x01EDAF84
            && CapacityFinalCalleeGatedEpilogueProjection.PushedImmediateVirtualAddress
                == 0x01EDAF88
            && CapacityFinalCalleeGatedEpilogueProjection.CallCellVirtualAddress == 0x01AC427C
            && Normalize(CapacityFinalCalleeGatedEpilogueProjection.CapturedWindowBytes)
                == "F60584AFED01025F5E5B740B6888AFED01FF157C42AC018BE55DC3"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeGatedEpiloguePlan invalidEntry =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryValidated: false,
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 4,
                callReturnedNormally: true,
                postCallEbp: 5,
                popEbp: 6,
                returnTarget: 7));
        check("An unvalidated entry stops before TEST and rejects every supplied later capture",
            invalidEntry.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.EntryNotValidated
            && !invalidEntry.TestReadReached
            && invalidEntry.TestedByte is null
            && !invalidEntry.StackPopEdiReached
            && !invalidEntry.BranchReached
            && !invalidEntry.PushImmediateReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.MovEspFromEbpReached
            && !invalidEntry.StackPopEbpReached
            && !invalidEntry.ReturnReached
            && !invalidEntry.ExactCleanExitAdmitted
            && invalidEntry.UnreachedTestedByteCaptureProvided
            && invalidEntry.UnreachedStackPopEdiCaptureProvided
            && invalidEntry.UnreachedStackPopEsiCaptureProvided
            && invalidEntry.UnreachedStackPopEbxCaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.UnreachedPostCallEbpCaptureProvided
            && invalidEntry.UnreachedStackPopEbpCaptureProvided
            && invalidEntry.UnreachedReturnTargetCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeGatedEpiloguePlan bypass =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEsp: 0xFFFFFFFC,
                entryEbp: 0xFFFFFFFC,
                testedByte: 0xFD,
                popEdi: 0xA1A1A1A1,
                popEsi: 0xB2B2B2B2,
                popEbx: 0xC3C3C3C3,
                callTarget: 0,
                callReturnedNormally: true,
                postCallEbp: 0x11111111,
                popEbp: 0xD4D4D4D4,
                returnTarget: 0x87654321));
        CapacityFinalCalleeGatedEpiloguePlan cleanBypass =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEsp: 0x1000,
                entryEbp: 0x2000,
                testedByte: 0,
                popEdi: 0xA1A1A1A1,
                popEsi: 0xB2B2B2B2,
                popEbx: 0xC3C3C3C3,
                popEbp: 0xD4D4D4D4,
                returnTarget: 0x87654321));
        check("A clear tested bit preserves flags across independent POP reads and bypasses the opaque call",
            bypass.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && bypass.TestedByte == 0xFD
            && bypass.TestResult == 0
            && bypass.TestFlags.HasValue
            && !bypass.TestFlags.Value.Carry
            && bypass.TestFlags.Value.Parity
            && bypass.TestFlags.Value.AuxiliaryCarry is null
            && bypass.TestFlags.Value.Zero
            && !bypass.TestFlags.Value.Sign
            && !bypass.TestFlags.Value.Overflow
            && bypass.TestedBitIsSet == false
            && bypass.BranchTaken == bypass.TestFlags.Value.Zero
            && bypass.StackPopEdiAddress == 0xFFFFFFFC
            && bypass.StackPopEsiAddress == 0
            && bypass.StackPopEbxAddress == 4
            && bypass.PoppedEdi == 0xA1A1A1A1
            && bypass.PoppedEsi == 0xB2B2B2B2
            && bypass.PoppedEbx == 0xC3C3C3C3
            && bypass.BranchTaken == true
            && bypass.MovEspFromEbpReached
            && bypass.MovEspSourceEbp == 0xFFFFFFFC
            && bypass.EpilogueEsp == 0xFFFFFFFC
            && !bypass.PushImmediateReached
            && !bypass.CallCellReadReached
            && !bypass.CallReached
            && bypass.UnreachedCallCellTargetCaptureProvided
            && bypass.UnreachedCallReturnStatusProvided
            && bypass.UnreachedPostCallEbpCaptureProvided
            && bypass.StackPopEbpAddress == 0xFFFFFFFC
            && bypass.PoppedEbp == 0xD4D4D4D4
            && bypass.ReturnTargetStackAddress == 0
            && bypass.ReturnTargetVirtualAddress == 0x87654321
            && bypass.EspAfterReturn == 4
            && bypass.ReturnReached
            && !bypass.ExactCleanExitAdmitted
            && bypass.StackAccesses.Select(access => access.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget
                })
            && bypass.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanBypass.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && cleanBypass.ReturnReached
            && cleanBypass.ExactCleanExitAdmitted
            && !cleanBypass.UnreachedCallCellTargetCaptureProvided
            && !cleanBypass.UnreachedCallReturnStatusProvided
            && !cleanBypass.UnreachedPostCallEbpCaptureProvided
            && !cleanBypass.UnreachedTestedByteCaptureProvided
            && !cleanBypass.UnreachedStackPopEdiCaptureProvided
            && !cleanBypass.UnreachedStackPopEsiCaptureProvided
            && !cleanBypass.UnreachedStackPopEbxCaptureProvided
            && !cleanBypass.UnreachedStackPopEbpCaptureProvided
            && !cleanBypass.UnreachedReturnTargetCaptureProvided);

        CapacityFinalCalleeGatedEpiloguePlan callPath =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEsp: 0xFFFFFFFC,
                entryEbp: 0x12345678,
                testedByte: 2,
                popEdi: 0x11111111,
                popEsi: 0x22222222,
                popEbx: 0x33333333,
                callTarget: 0,
                callReturnedNormally: true,
                postCallEbp: 0xFFFFFFFC,
                popEbp: 0x44444444,
                returnTarget: 0xDEADBEEF));
        check("The set-bit fallthrough pushes before the opaque call and requires independent post-call EBP",
            callPath.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
            && callPath.TestResult == 2
            && callPath.TestFlags.HasValue
            && !callPath.TestFlags.Value.Carry
            && !callPath.TestFlags.Value.Parity
            && callPath.TestFlags.Value.AuxiliaryCarry is null
            && !callPath.TestFlags.Value.Zero
            && !callPath.TestFlags.Value.Sign
            && !callPath.TestFlags.Value.Overflow
            && callPath.TestedBitIsSet == true
            && callPath.BranchTaken == false
            && callPath.BranchTaken == callPath.TestFlags.Value.Zero
            && callPath.PushImmediateReached
            && callPath.PushImmediateAddress == 4
            && callPath.PushedImmediateVirtualAddress == 0x01EDAF88
            && callPath.StackPopEbxAddress == 4
            && callPath.StackAccesses[3].Kind
                == CapacityFinalCalleeGatedEpilogueStackAccessKind.PushImmediate
            && callPath.StackAccesses[3].Address == callPath.StackPopEbxAddress
            && callPath.CallCellReadReached
            && callPath.CallCellTargetCaptured
            && callPath.CallCellTargetVirtualAddress == 0
            && callPath.CallReached
            && callPath.CallReturnedNormally == true
            && callPath.PostCallEbpCaptureReached
            && callPath.PostCallEbpCaptured
            && callPath.PostCallEbp == 0xFFFFFFFC
            && callPath.MovEspSourceEbp == 0xFFFFFFFC
            && callPath.EpilogueEsp == 0xFFFFFFFC
            && callPath.StackPopEbpAddress == 0xFFFFFFFC
            && callPath.PoppedEbp == 0x44444444
            && callPath.ReturnTargetStackAddress == 0
            && callPath.ReturnTargetVirtualAddress == 0xDEADBEEF
            && callPath.EspAfterReturn == 4
            && callPath.ReturnReached
            && callPath.ExactCleanExitAdmitted
            && !callPath.UnreachedPostCallEbpCaptureProvided
            && callPath.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeGatedEpiloguePlan missingByte =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: null,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 4,
                callReturnedNormally: true,
                postCallEbp: 5,
                popEbp: 6,
                returnTarget: 7));
        CapacityFinalCalleeGatedEpiloguePlan missingEdi =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: null,
                popEsi: 2,
                popEbx: 3));
        CapacityFinalCalleeGatedEpiloguePlan missingEsi =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: null,
                popEbx: 3));
        CapacityFinalCalleeGatedEpiloguePlan missingEbx =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: null,
                callTarget: 0));
        check("Missing TEST and POP reads stop at their own instruction and preserve earlier stack effects",
            missingByte.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingByte.TestReadReached
            && !missingByte.TestedByteCaptured
            && missingByte.UnreachedStackPopEdiCaptureProvided
            && missingByte.UnreachedPostCallEbpCaptureProvided
            && missingByte.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingEdi.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue
            && missingEdi.TestedBitIsSet == true
            && missingEdi.StackPopEdiReached
            && missingEdi.StackPopEdiAddress == 0x1000
            && missingEdi.StackAccesses.Length == 1
            && missingEdi.StackAccesses[0].Value is null
            && !missingEdi.StackPopEsiReached
            && missingEdi.UnreachedStackPopEsiCaptureProvided
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingEsi.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingEsi.PoppedEdi == 1
            && missingEsi.StackPopEsiAddress == 0x1004
            && missingEsi.UnreachedStackPopEbxCaptureProvided
            && missingEsi.StackAccesses.Length == 2
            && missingEsi.StackAccesses[1].Value is null
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9449
            && missingEbx.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && missingEbx.PoppedEdi == 1
            && missingEbx.PoppedEsi == 2
            && missingEbx.StackPopEbxAddress == 0x1008
            && !missingEbx.BranchReached
            && missingEbx.StackAccesses.Length == 3
            && missingEbx.StackAccesses[2].Value is null
            && missingEbx.UnreachedCallCellTargetCaptureProvided
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeGatedEpiloguePlan missingCell =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: null,
                callReturnedNormally: true,
                postCallEbp: 5,
                popEbp: 6,
                returnTarget: 7));
        CapacityFinalCalleeGatedEpiloguePlan missingCallStatus =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0xCAFEBABE,
                callReturnedNormally: null,
                postCallEbp: 5,
                popEbp: 6,
                returnTarget: 7));
        CapacityFinalCalleeGatedEpiloguePlan nonreturning =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0xCAFEBABE,
                callReturnedNormally: false,
                postCallEbp: 5,
                popEbp: 6,
                returnTarget: 7));
        check("The call gate stops at CALL when its target or return is unavailable, preserving the push and POPs",
            missingCell.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCell.PushImmediateAddress == 0x1008
            && missingCell.StackAccesses.Length == 4
            && missingCell.CallCellReadReached
            && !missingCell.CallCellTargetCaptured
            && !missingCell.CallReached
            && missingCell.UnreachedCallReturnStatusProvided
            && missingCell.UnreachedPostCallEbpCaptureProvided
            && !missingCell.ExactCleanExitAdmitted
            && missingCell.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingCallStatus.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus
            && missingCallStatus.CallCellTargetVirtualAddress == 0xCAFEBABE
            && missingCallStatus.CallReached
            && missingCallStatus.CallReturnedNormally is null
            && missingCallStatus.UnreachedPostCallEbpCaptureProvided
            && !missingCallStatus.ExactCleanExitAdmitted
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x012C9452
            && nonreturning.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && nonreturning.CallReturnedNormally == false
            && nonreturning.StackAccesses.Length == 4
            && !nonreturning.PostCallEbpCaptureReached
            && !nonreturning.MovEspFromEbpReached
            && !nonreturning.StackPopEbpReached
            && nonreturning.UnreachedPostCallEbpCaptureProvided
            && nonreturning.UnreachedStackPopEbpCaptureProvided
            && nonreturning.UnreachedReturnTargetCaptureProvided
            && !nonreturning.ExactCleanExitAdmitted
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C9452);

        CapacityFinalCalleeGatedEpiloguePlan missingPostCallEbp =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEbp: 0x1000,
                testedByte: 2,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                callTarget: 0x1234,
                callReturnedNormally: true,
                postCallEbp: null,
                popEbp: 4,
                returnTarget: 5));
        CapacityFinalCalleeGatedEpiloguePlan missingFinalPop =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEbp: 0x1000,
                testedByte: 0,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                popEbp: null,
                returnTarget: 5));
        CapacityFinalCalleeGatedEpiloguePlan missingReturnTarget =
            CapacityFinalCalleeGatedEpilogueProjection.Project(Capture(
                entryEbp: 0x1000,
                testedByte: 0,
                popEdi: 1,
                popEsi: 2,
                popEbx: 3,
                popEbp: 4,
                returnTarget: null));
        check("Post-call EBP, final POP EBP, and RET target are independent fresh captures",
            missingPostCallEbp.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && missingPostCallEbp.CallReturnedNormally == true
            && missingPostCallEbp.PostCallEbpCaptureReached
            && !missingPostCallEbp.PostCallEbpCaptured
            && missingPostCallEbp.MovEspFromEbpReached
            && missingPostCallEbp.MovEspSourceEbp is null
            && !missingPostCallEbp.ExactCleanExitAdmitted
            && missingPostCallEbp.StopAtRawFileOffsetExclusive == 0x012C9458
            && missingFinalPop.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue
            && missingFinalPop.MovEspSourceEbp == 0x1000
            && missingFinalPop.StackPopEbpReached
            && missingFinalPop.StackPopEbpAddress == 0x1000
            && missingFinalPop.StackAccesses.Length == 4
            && missingFinalPop.StackAccesses[3].Value is null
            && !missingFinalPop.ExactCleanExitAdmitted
            && !missingFinalPop.ReturnTargetReadReached
            && missingFinalPop.UnreachedReturnTargetCaptureProvided
            && missingFinalPop.StopAtRawFileOffsetExclusive == 0x012C945A
            && missingReturnTarget.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingReturnTarget
            && missingReturnTarget.PoppedEbp == 4
            && missingReturnTarget.ReturnTargetReadReached
            && missingReturnTarget.ReturnTargetStackAddress == 0x1004
            && missingReturnTarget.StackAccesses.Length == 5
            && missingReturnTarget.StackAccesses[4].Value is null
            && !missingReturnTarget.ReturnTargetCaptured
            && !missingReturnTarget.ReturnReached
            && !missingReturnTarget.ExactCleanExitAdmitted
            && missingReturnTarget.StopAtRawFileOffsetExclusive == 0x012C945B);
    }

    private static CapacityFinalCalleeGatedEpilogueCapture Capture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x4000,
        byte? testedByte = 0,
        uint? popEdi = 0x1111,
        uint? popEsi = 0x2222,
        uint? popEbx = 0x3333,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? popEbp = 0x4444,
        uint? returnTarget = 0x5555) =>
        new(
            entryValidated,
            entryEsp,
            entryEbp,
            testedByte,
            popEdi,
            popEsi,
            popEbx,
            callTarget,
            callReturnedNormally,
            postCallEbp,
            popEbp,
            returnTarget);

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
        uint nextRawOffset = CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeGatedEpilogueInstructionEvidence instruction
            in CapacityFinalCalleeGatedEpilogueProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeGatedEpilogueProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeGatedEpilogueProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeGatedEpilogueProjection.CapturedWindowBytes)
            && CapacityFinalCalleeGatedEpilogueProjection.Instructions
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
