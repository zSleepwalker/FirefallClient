using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePrefixCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded raw window, branch destination, and indirect-call operands match",
            CapacityFinalCalleePrefixCallProjection.EntryRawFileOffset == 0x012C8D78
            && CapacityFinalCalleePrefixCallProjection.EntryVirtualAddress == 0x016C9978
            && CapacityFinalCalleePrefixCallProjection.BranchRawFileOffset == 0x012C8D7B
            && CapacityFinalCalleePrefixCallProjection.PushImmediateRawFileOffset
                == 0x012C8D7D
            && CapacityFinalCalleePrefixCallProjection.CallRawFileOffset == 0x012C8D82
            && CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8D88
            && CapacityFinalCalleePrefixCallProjection.BoundaryVirtualAddressExclusive
                == 0x016C9988
            && CapacityFinalCalleePrefixCallProjection.BranchDestinationRawFileOffset
                == 0x012C8D88
            && CapacityFinalCalleePrefixCallProjection.PushedImmediateVirtualAddress
                == 0x01EDAF88
            && CapacityFinalCalleePrefixCallProjection.CallCellVirtualAddress
                == 0x01AC4278
            && CapacityFinalCalleePrefixCallProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleePrefixCallProjection.EntryRawFileOffset)
                == CapacityFinalCalleePrefixCallProjection.EntryVirtualAddress
            && CapacityFinalCalleePrefixCallProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleePrefixCallProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleePrefixCallProjection.CapturedWindowBytes)
                == "8D71F8740B6888AFED01FF157842AC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleePrefixCallPlan invalidEntry =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                captureValidatedNonzeroEntry: false,
                entryEcx: 0x1234,
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: true));
        check("An unvalidated entry derives no ESI and marks later captures unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleePrefixCallOutcome.EntryNotValidated
            && !invalidEntry.LeaEsiReached
            && invalidEntry.DerivedPrecallESI is null
            && !invalidEntry.BranchReached
            && !invalidEntry.PushImmediateReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedPriorTestMaskCaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8D78);

        CapacityFinalCalleePrefixCallPlan zeroEntryEcx =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                entryEcx: 0,
                priorTestMaskSet: false,
                callTarget: 0x1000,
                callReturnedNormally: true));
        check("A zero ECX is rejected as an invalid nonzero entry before LEA",
            zeroEntryEcx.Outcome == CapacityFinalCalleePrefixCallOutcome.ZeroEntryEcx
            && !zeroEntryEcx.LeaEsiReached
            && zeroEntryEcx.DerivedPrecallESI is null
            && !zeroEntryEcx.BranchReached
            && zeroEntryEcx.UnreachedPriorTestMaskCaptureProvided
            && zeroEntryEcx.UnreachedCallCellTargetCaptureProvided
            && zeroEntryEcx.UnreachedCallReturnStatusProvided
            && zeroEntryEcx.StopAtRawFileOffsetExclusive == 0x012C8D78);

        CapacityFinalCalleePrefixCallPlan missingPriorFlags =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                entryEcx: 2,
                priorTestMaskSet: null,
                callTarget: 0x1000,
                callReturnedNormally: true));
        check("LEA derives wrapping pre-call ESI before a missing prior-TEST-flags stop",
            missingPriorFlags.Outcome
                == CapacityFinalCalleePrefixCallOutcome.MissingPriorTestMaskCapture
            && missingPriorFlags.LeaEsiReached
            && missingPriorFlags.DerivedPrecallESI == 0xFFFFFFFA
            && !missingPriorFlags.BranchReached
            && !missingPriorFlags.CallCellReadReached
            && missingPriorFlags.UnreachedCallCellTargetCaptureProvided
            && missingPriorFlags.UnreachedCallReturnStatusProvided
            && missingPriorFlags.StopAtRawFileOffsetExclusive == 0x012C8D7B);

        CapacityFinalCalleePrefixCallPlan maskClear =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                entryEcx: 7,
                priorTestMaskSet: false,
                callTarget: 0x2000,
                callReturnedNormally: true));
        CapacityFinalCalleePrefixCallPlan maskSet =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                entryEcx: 0x12345678,
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: true));
        check("The captured prior TEST mask selects the bypass or call path without reloading flags",
            maskClear.Outcome == CapacityFinalCalleePrefixCallOutcome.CallSkipped
            && maskClear.DerivedPrecallESI == 0xFFFFFFFF
            && maskClear.BranchReached
            && maskClear.BranchTaken == true
            && maskClear.SelectedDestinationRawFileOffset == 0x012C8D88
            && maskClear.OutgoingESI == 0xFFFFFFFF
            && !maskClear.PushImmediateReached
            && !maskClear.CallCellReadReached
            && !maskClear.CallReached
            && maskClear.UnreachedCallCellTargetCaptureProvided
            && maskClear.UnreachedCallReturnStatusProvided
            && maskClear.StopAtRawFileOffsetExclusive == 0x012C8D88
            && maskSet.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally
            && maskSet.DerivedPrecallESI == 0x12345670
            && maskSet.BranchReached
            && maskSet.BranchTaken == false
            && maskSet.SelectedDestinationRawFileOffset is null
            && maskSet.PushImmediateReached
            && maskSet.PushedImmediateVirtualAddress == 0x01EDAF88
            && maskSet.CallCellReadReached
            && maskSet.CallCellTargetCaptured
            && maskSet.CallCellTargetVirtualAddress == 0
            && maskSet.CallReached
            && maskSet.CallReturnedNormally == true
            && maskSet.OutgoingESI is null
            && maskSet.DerivedPrecallESI != maskSet.OutgoingESI
            && maskSet.StopAtRawFileOffsetExclusive == 0x012C8D88);

        CapacityFinalCalleePrefixCallPlan missingTarget =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: null,
                callReturnedNormally: true));
        check("The immediate is pushed before a missing call-cell target stops at CALL",
            missingTarget.Outcome
                == CapacityFinalCalleePrefixCallOutcome.MissingCallCellTarget
            && missingTarget.DerivedPrecallESI == 0x12345670
            && missingTarget.BranchTaken == false
            && missingTarget.PushImmediateReached
            && missingTarget.PushedImmediateVirtualAddress == 0x01EDAF88
            && missingTarget.CallCellReadReached
            && !missingTarget.CallCellTargetCaptured
            && !missingTarget.CallReached
            && missingTarget.UnreachedCallReturnStatusProvided
            && missingTarget.StopAtRawFileOffsetExclusive == 0x012C8D82);

        CapacityFinalCalleePrefixCallPlan missingReturnStatus =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: 0x1000,
                callReturnedNormally: null));
        CapacityFinalCalleePrefixCallPlan nonreturning =
            CapacityFinalCalleePrefixCallProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: false));
        check("A captured target, including zero, does not imply a normal return",
            missingReturnStatus.Outcome
                == CapacityFinalCalleePrefixCallOutcome.MissingCallReturnStatus
            && missingReturnStatus.CallCellTargetCaptured
            && missingReturnStatus.CallCellTargetVirtualAddress == 0x1000
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C8D82
            && nonreturning.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallDidNotReturnNormally
            && nonreturning.CallCellTargetVirtualAddress == 0
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.OutgoingESI is null
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C8D82);
    }

    private static CapacityFinalCalleePrefixCallCapture Capture(
        bool captureValidatedNonzeroEntry = true,
        uint entryEcx = 0x12345678,
        bool? priorTestMaskSet = false,
        uint? callTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedNonzeroEntry,
            entryEcx,
            priorTestMaskSet,
            callTarget,
            callReturnedNormally);

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
            (0x012C8D78, "8D 71 F8", null, null, null, null),
            (0x012C8D7B, "74 0B", null, null, 0x012C8D88, 0x016C9988),
            (0x012C8D7D, "68 88 AF ED 01", 0x01EDAF88, null, null, null),
            (0x012C8D82, "FF 15 78 42 AC 01", 0x01AC4278, null, null, null)
        };
        uint nextRawOffset = CapacityFinalCalleePrefixCallProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleePrefixCallInstructionEvidence instruction
            in CapacityFinalCalleePrefixCallProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleePrefixCallProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleePrefixCallProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(CapacityFinalCalleePrefixCallProjection.CapturedWindowBytes)
            && CapacityFinalCalleePrefixCallProjection.Instructions
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
