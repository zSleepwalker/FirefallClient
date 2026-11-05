using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeZeroExitLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The zero branch, native SUB/JMP, and exit entry retain exact raw and VA evidence",
            CapacityFinalCalleeZeroExitLifecycleProjection.EntryRawFileOffset
                == 0x012C8E3A
            && CapacityFinalCalleeZeroExitLifecycleProjection.SubtractRawFileOffset
                == 0x012C8E3A
            && CapacityFinalCalleeZeroExitLifecycleProjection.JumpRawFileOffset
                == 0x012C8E40
            && CapacityFinalCalleeZeroExitLifecycleProjection.SubtractWindowBoundaryRawFileOffsetExclusive
                == 0x012C8E45
            && CapacityFinalCalleeZeroExitLifecycleProjection.ExitEntryRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeZeroExitLifecycleProjection.BoundaryRawFileOffsetExclusive
                == 0x012C945C
            && CapacityFinalCalleeZeroExitLifecycleProjection.NativeGlobalDwordVirtualAddress
                == 0x01EDAF78
            && CapacityFinalCalleeZeroExitLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeZeroExitLifecycleProjection.EntryRawFileOffset)
                == 0x016C9A3A
            && CapacityFinalCalleeZeroExitLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeZeroExitLifecycleProjection.ExitEntryRawFileOffset)
                == CapacityFinalCalleeExitProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeZeroExitLifecycleProjection.CapturedWindowBytes)
                == "290D78AFED01E9FC050000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlignmentDispatchPlan cleanDispatch =
            ComputedZeroDispatch(
                entryEax: 8,
                entryEsi: 1,
                entryEbp: 4,
                entryEsp: 0,
                entryEsiDword: 0xFFFFFFE8);
        CapacityFinalCalleeZeroExitLifecyclePlan maskedClear =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(
                    testedByte: 0,
                    popEdi: 0x11111111,
                    popEsi: 0x22222222,
                    popEbx: 0x33333333,
                    popEbp: 0x44444444)));
        check("Only the clean computed-zero plan admits ECX zero and still performs the global read and write",
            maskedClear.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.ReturnedNormally
            && maskedClear.DispatchComputedZeroAdmitted
            && maskedClear.SubtractionEcxDerived
            && maskedClear.SubtractionEcx == 0
            && cleanDispatch.ECX == 0
            && cleanDispatch.TestComputedEcxFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && cleanDispatch.ESI == 0x19
            && cleanDispatch.EDX == 9
            && cleanDispatch.FrameWrites[^1]
                == new CapacityFinalCalleeAlignmentDispatchWrite(
                    0x012C8DCE,
                    0xFFFFFFF4,
                    0)
            && maskedClear.GlobalAccess == new CapacityFinalCalleeZeroExitGlobalAccess(
                0x012C8E3A,
                0x01EDAF78,
                ReadReached: true,
                ValueCaptured: true,
                ValueBefore: 0,
                WriteReached: true,
                ValueAfter: 0,
                Subtrahend: 0)
            && maskedClear.ExitEntryAdmitted
            && maskedClear.ExitPlan.Capture.EntryValidated
            && maskedClear.ExitPlan.Outcome == CapacityFinalCalleeExitOutcome.CallSkipped
            && maskedClear.ExitPlan.TestedByte == 0
            && maskedClear.ExitPlan.PoppedEdi == 0x11111111
            && maskedClear.ExitPlan.PoppedEsi == 0x22222222
            && maskedClear.ExitPlan.PoppedEbx == 0x33333333
            && maskedClear.ExitPlan.PoppedEbp == 0x44444444
            && maskedClear.ExitPlan.ReturnReached
            && maskedClear.ExitReturnedNormally
            && maskedClear.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeZeroExitLifecyclePlan contaminatedSkippedExit =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0xA5A5A5A5,
                Exit(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 0x1234,
                    callReturnedNormally: true,
                    outputEbx: 4,
                    outputEdi: 5,
                    outputEsi: 6,
                    popEbp: 7)));
        check("Skipped-call contamination blocks wrapper success without erasing the preceding global write",
            contaminatedSkippedExit.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.ExitBlocked
            && contaminatedSkippedExit.GlobalAccess.ReadReached
            && contaminatedSkippedExit.GlobalAccess.WriteReached
            && contaminatedSkippedExit.GlobalAccess.ValueBefore == 0xA5A5A5A5
            && contaminatedSkippedExit.GlobalAccess.ValueAfter == 0xA5A5A5A5
            && contaminatedSkippedExit.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.CallSkipped
            && contaminatedSkippedExit.ExitPlan.ReturnReached
            && contaminatedSkippedExit.ExitPlan.StopAtRawFileOffsetExclusive == 0x012C945C
            && contaminatedSkippedExit.ExitPlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedSkippedExit.ExitPlan.UnreachedCallReturnStatusProvided
            && contaminatedSkippedExit.ExitPlan.UnreachedCalleeOutputEbxCaptureProvided
            && contaminatedSkippedExit.ExitPlan.UnreachedCalleeOutputEdiCaptureProvided
            && contaminatedSkippedExit.ExitPlan.UnreachedCalleeOutputEsiCaptureProvided
            && !contaminatedSkippedExit.ExitReturnedNormally
            && contaminatedSkippedExit.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeZeroExitLifecyclePlan missingGlobal =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                null,
                Exit(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    outputEbx: 5,
                    popEbp: 6)));
        CapacityFinalCalleeZeroExitLifecyclePlan capturedZeroGlobal =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    popEbp: 4)));
        check("A missing source read blocks the exit, while a captured zero is read, written, and distinct",
            missingGlobal.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.MissingNativeGlobalDword
            && missingGlobal.GlobalAccess.ReadReached
            && !missingGlobal.GlobalAccess.ValueCaptured
            && missingGlobal.GlobalAccess.ValueBefore is null
            && !missingGlobal.GlobalAccess.WriteReached
            && missingGlobal.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.EntryNotValidated
            && !missingGlobal.ExitEntryAdmitted
            && missingGlobal.ExitPlan.UnreachedTestedByteCaptureProvided
            && missingGlobal.ExitPlan.UnreachedStackPopEdiCaptureProvided
            && missingGlobal.ExitPlan.UnreachedCallCellTargetCaptureProvided
            && missingGlobal.ExitPlan.UnreachedCallReturnStatusProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8E3A
            && capturedZeroGlobal.GlobalAccess.ValueCaptured
            && capturedZeroGlobal.GlobalAccess.ValueBefore == 0
            && capturedZeroGlobal.GlobalAccess.WriteReached
            && capturedZeroGlobal.GlobalAccess.ValueAfter == 0
            && capturedZeroGlobal.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.CallSkipped
            && capturedZeroGlobal.ExitPlan.ReturnReached);

        CapacityFinalCalleeZeroExitLifecyclePlan maskedSet =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                uint.MaxValue,
                Exit(
                    entryValidated: false,
                    testedByte: 2,
                    popEdi: 0xA,
                    popEsi: 0xB,
                    popEbx: 0xC,
                    callTarget: 0x12345678,
                    callReturnedNormally: true,
                    outputEbx: 0xDEADBEEF,
                    outputEdi: 0xD1D1D1D1,
                    outputEsi: 0x51515151,
                    popEbp: 0xEEEEEEEE)));
        CapacityFinalCalleeZeroExitLifecyclePlan nonreturning =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0x80000000,
                Exit(
                    testedByte: 0xFF,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 0x1000,
                    callReturnedNormally: false,
                    outputEbx: 8,
                    popEbp: 4)));
        check("The exit retains independent POP and opaque-call captures and requires normal call return",
            maskedSet.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.ReturnedNormally
            && maskedSet.GlobalAccess.ValueBefore == uint.MaxValue
            && maskedSet.GlobalAccess.ValueAfter == uint.MaxValue
            && maskedSet.GlobalAccess.WriteReached
            && maskedSet.ExitPlan.Outcome == CapacityFinalCalleeExitOutcome.ReturnedNormally
            && maskedSet.ExitPlan.CallCellTargetVirtualAddress == 0x12345678
            && maskedSet.ExitPlan.PoppedEbx == 0xC
            && maskedSet.ExitPlan.CalleeOutputEbx == 0xDEADBEEF
            && maskedSet.ExitPlan.PoppedEdi == 0xA
            && maskedSet.ExitPlan.CalleeOutputEdi == 0xD1D1D1D1
            && maskedSet.ExitPlan.PoppedEsi == 0xB
            && maskedSet.ExitPlan.CalleeOutputEsi == 0x51515151
            && maskedSet.ExitPlan.PoppedEbp == 0xEEEEEEEE
            && nonreturning.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.ExitBlocked
            && nonreturning.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.CallDidNotReturnNormally
            && nonreturning.ExitPlan.CallReturnedNormally == false
            && nonreturning.ExitPlan.UnreachedCalleeOutputEbxCaptureProvided
            && !nonreturning.ExitPlan.MovEspFromEbpReached
            && !nonreturning.ExitReturnedNormally
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C9452);

        CapacityFinalCalleeZeroExitLifecyclePlan missingTestedByte =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0xFFFFFFFF,
                Exit(
                    testedByte: null,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    popEbp: 4)));
        CapacityFinalCalleeZeroExitLifecyclePlan missingPopEdi =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(testedByte: 2, popEdi: null, popEsi: 2, popEbx: 3, popEbp: 4)));
        CapacityFinalCalleeZeroExitLifecyclePlan missingPopEsi =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(testedByte: 2, popEdi: 1, popEsi: null, popEbx: 3, popEbp: 4)));
        CapacityFinalCalleeZeroExitLifecyclePlan missingPopEbx =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(testedByte: 2, popEdi: 1, popEsi: 2, popEbx: null, popEbp: 4)));
        CapacityFinalCalleeZeroExitLifecyclePlan missingPopEbp =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                cleanDispatch,
                0,
                Exit(testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3, popEbp: null)));
        check("Missing exit byte and restored-register captures stay blocked after the global write",
            missingTestedByte.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.ExitBlocked
            && missingTestedByte.GlobalAccess.WriteReached
            && missingTestedByte.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.MissingTestedByte
            && missingPopEdi.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEdiValue
            && missingPopEsi.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEsiValue
            && missingPopEbx.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEbxValue
            && missingPopEbp.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEbpValue
            && missingPopEbp.ExitPlan.MovEspFromEbpReached
            && !missingPopEbp.ExitPlan.ReturnReached
            && missingPopEbp.ExitPlan.PoppedEbp is null
            && !missingPopEbp.ExitReturnedNormally);

        CapacityFinalCalleeAlignmentDispatchPlan blockedBranch =
            Dispatch(
                captureValidatedEntry: true,
                entryEax: 0x19,
                entryEsi: 0x100,
                entryEsiDword: 0,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeZeroExitLifecyclePlan blocked =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                blockedBranch,
                0,
                Exit(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    popEbp: 5)));
        CapacityFinalCalleeAlignmentDispatchPlan contaminatedComputedZero =
            Dispatch(
                entryEax: 8,
                entryEsi: 1,
                entryEsiDword: 0xFFFFFFE8,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeZeroExitLifecyclePlan contaminated =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                contaminatedComputedZero,
                0,
                Exit(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    popEbp: 5)));
        check("Earlier dispatch branches and downstream captures on computed zero cannot bypass admission",
            blockedBranch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && blockedBranch.UnreachedGlobalDispatchDwordCaptureProvided
            && blockedBranch.UnreachedCallCellTargetCaptureProvided
            && blocked.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.DispatchNotAdmitted
            && blocked.UnreachedNativeGlobalDwordCaptureProvided
            && !blocked.GlobalAccess.ReadReached
            && blocked.ExitPlan.Outcome
                == CapacityFinalCalleeExitOutcome.EntryNotValidated
            && blocked.ExitPlan.UnreachedTestedByteCaptureProvided
            && blocked.ExitPlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedComputedZero.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
            && contaminatedComputedZero.UnreachedGlobalDispatchDwordCaptureProvided
            && contaminatedComputedZero.UnreachedCallCellTargetCaptureProvided
            && contaminated.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.DispatchNotAdmitted
            && !contaminated.GlobalAccess.ReadReached
            && contaminated.UnreachedNativeGlobalDwordCaptureProvided
            && contaminated.ExitPlan.UnreachedTestedByteCaptureProvided
            && contaminated.ExitPlan.UnreachedCallReturnStatusProvided
            && contaminated.StopAtRawFileOffsetExclusive == 0x012C8E3A);

        CapacityFinalCalleeAlignmentDispatchPlan entryEcxBranch =
            Dispatch(
                entryEax: 0x18,
                entryEsi: 0x100,
                entryEcx: 1,
                entryEsiDword: 0,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeAlignmentDispatchPlan computedNonzero =
            Dispatch(
                entryEax: 0x18,
                entryEsi: 0x100,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true);
        CapacityFinalCalleeZeroExitLifecyclePlan entryEcxBlocked =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                entryEcxBranch,
                0,
                Exit(testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3, popEbp: 4)));
        CapacityFinalCalleeZeroExitLifecyclePlan computedNonzeroBlocked =
            CapacityFinalCalleeZeroExitLifecycleProjection.Project(new(
                computedNonzero,
                0,
                Exit(testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3, popEbp: 4)));
        check("Entry-ECX and computed-ECX branches preserve their upstream stop and unreached captures",
            entryEcxBranch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
            && entryEcxBranch.UnreachedGlobalDispatchDwordCaptureProvided
            && entryEcxBlocked.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.DispatchNotAdmitted
            && entryEcxBlocked.StopAtRawFileOffsetExclusive == 0x012C8E45
            && entryEcxBlocked.ExitPlan.UnreachedStackPopEdiCaptureProvided
            && computedNonzero.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
            && computedNonzeroBlocked.Outcome
                == CapacityFinalCalleeZeroExitLifecycleOutcome.DispatchNotAdmitted
            && !computedNonzeroBlocked.GlobalAccess.ReadReached
            && computedNonzeroBlocked.UnreachedNativeGlobalDwordCaptureProvided
            && computedNonzeroBlocked.ExitPlan.UnreachedStackPopEbpCaptureProvided
            && computedNonzeroBlocked.StopAtRawFileOffsetExclusive
                == computedNonzero.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeAlignmentDispatchPlan ComputedZeroDispatch(
        uint entryEax,
        uint entryEsi,
        uint entryEbp,
        uint entryEsp,
        uint entryEsiDword) =>
        Dispatch(
            entryEax: entryEax,
            entryEsi: entryEsi,
            entryEcx: 0,
            entryEbp: entryEbp,
            entryEsp: entryEsp,
            entryEsiDword: entryEsiDword,
            globalDispatchDword: null,
            callCellTarget: null,
            callReturnedNormally: null);

    private static CapacityFinalCalleeAlignmentDispatchPlan Dispatch(
        uint entryEax = 0x18,
        uint entryEsi = 0x1000,
        uint entryEcx = 0,
        uint entryEbp = 0x2000,
        uint entryEsp = 0x3000,
        uint? entryEsiDword = 0x20,
        uint? globalDispatchDword = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null,
        bool captureValidatedEntry = true) =>
        CapacityFinalCalleeAlignmentDispatchProjection.Project(new(
            captureValidatedEntry,
            entryEax,
            entryEsi,
            entryEcx,
            entryEbp,
            entryEsp,
            entryEsiDword,
            globalDispatchDword,
            callCellTarget,
            callReturnedNormally));

    private static CapacityFinalCalleeExitCapture Exit(
        bool entryValidated = false,
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
        var expected = new[]
        {
            (0x012C8E3Au, "29 0D 78 AF ED 01", (uint?)0x01EDAF78, (uint?)null),
            (0x012C8E40u, "E9 FC 05 00 00", (uint?)null, (uint?)0x012C9441)
        };
        uint nextRawOffset =
            CapacityFinalCalleeZeroExitLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeZeroExitInstructionEvidence instruction
            in CapacityFinalCalleeZeroExitLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeZeroExitLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeZeroExitLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeZeroExitLifecycleProjection
                    .SubtractWindowBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeZeroExitLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeZeroExitLifecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
