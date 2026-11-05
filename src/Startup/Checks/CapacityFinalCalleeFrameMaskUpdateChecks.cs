using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFrameMaskUpdateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window and its raw/virtual addresses match",
            CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset
                == 0x012C8EBE
            && CapacityFinalCalleeFrameMaskUpdateProjection.EntryVirtualAddress
                == 0x016C9ABE
            && CapacityFinalCalleeFrameMaskUpdateProjection.CompareRawFileOffset
                == 0x012C8EC1
            && CapacityFinalCalleeFrameMaskUpdateProjection.BranchRawFileOffset
                == 0x012C8EC4
            && CapacityFinalCalleeFrameMaskUpdateProjection.FrameMinus8LoadRawFileOffset
                == 0x012C8EC6
            && CapacityFinalCalleeFrameMaskUpdateProjection.ShiftRawFileOffset
                == 0x012C8EC9
            && CapacityFinalCalleeFrameMaskUpdateProjection.NotRawFileOffset
                == 0x012C8ECB
            && CapacityFinalCalleeFrameMaskUpdateProjection.GlobalAndRawFileOffset
                == 0x012C8ECD
            && CapacityFinalCalleeFrameMaskUpdateProjection.JumpRawFileOffset
                == 0x012C8ED3
            && CapacityFinalCalleeFrameMaskUpdateProjection.WindowBoundaryRawFileOffsetExclusive
                == 0x012C8ED5
            && CapacityFinalCalleeFrameMaskUpdateProjection.MismatchDestinationRawFileOffset
                == 0x012C8ED5
            && CapacityFinalCalleeFrameMaskUpdateProjection.ContinuationRawFileOffset
                == 0x012C8EF8
            && CapacityFinalCalleeFrameMaskUpdateProjection.GlobalMaskVirtualAddress
                == 0x01DADAC8
            && CapacityFinalCalleeFrameMaskUpdateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFrameMaskUpdateProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeFrameMaskUpdateProjection.CapturedWindowBytes)
                == "8B45EC3B45FC750F8B45F8D3E0F7D02105C8ADDA01EB23"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeFrameMaskUpdatePlan missingEbp =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: null,
                validatedEntryEcx: 0,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("An unvalidated EBP blocks every memory read without confusing zero captures",
            missingEbp.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP
            && !missingEbp.FreshFrameMinus14ReadReached
            && !missingEbp.FreshFrameMinus14Captured
            && !missingEbp.FreshFrameMinus4ReadReached
            && !missingEbp.CompareReached
            && !missingEbp.BranchReached
            && !missingEbp.FreshFrameMinus8ReadReached
            && !missingEbp.GlobalReadReached
            && !missingEbp.GlobalWriteReached
            && missingEbp.UnreachedEntryECXCaptureProvided
            && missingEbp.UnreachedFreshFrameMinus14CaptureProvided
            && missingEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingEbp.UnreachedFreshFrameMinus8CaptureProvided
            && missingEbp.UnreachedFreshGlobalValueCaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeFrameMaskUpdatePlan missingMinus14 =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 8,
                freshFrameMinus14: null,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("The first fresh read blocks CMP while wrapped addresses and downstream captures stay distinct",
            missingMinus14.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus14
            && missingMinus14.FreshFrameMinus14Address == 0xFFFFFFF4
            && missingMinus14.FreshFrameMinus14ReadReached
            && !missingMinus14.FreshFrameMinus14Captured
            && missingMinus14.FreshFrameMinus14 is null
            && missingMinus14.FreshFrameMinus4Address is null
            && !missingMinus14.CompareReached
            && missingMinus14.CompareFlags is null
            && !missingMinus14.BranchReached
            && missingMinus14.UnreachedEntryECXCaptureProvided
            && missingMinus14.UnreachedFreshFrameMinus4CaptureProvided
            && missingMinus14.UnreachedFreshFrameMinus8CaptureProvided
            && missingMinus14.UnreachedFreshGlobalValueCaptureProvided);

        CapacityFinalCalleeFrameMaskUpdatePlan missingMinus4 =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 8,
                validatedEntryEcx: 255,
                freshFrameMinus14: 0,
                freshFrameMinus4: null,
                freshFrameMinus8: 0,
                freshGlobalValue: 0));
        check("The second fresh read blocks CMP and reports a captured zero separately from a missing read",
            missingMinus4.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus4
            && missingMinus4.FreshFrameMinus14Address == 0xFFFFFFF4
            && missingMinus4.FreshFrameMinus14Captured
            && missingMinus4.FreshFrameMinus14 == 0
            && missingMinus4.FreshFrameMinus4Address == 4
            && missingMinus4.FreshFrameMinus4ReadReached
            && !missingMinus4.FreshFrameMinus4Captured
            && missingMinus4.CompareResult is null
            && missingMinus4.CompareFlags is null
            && !missingMinus4.BranchReached
            && missingMinus4.UnreachedFreshFrameMinus8CaptureProvided
            && missingMinus4.UnreachedFreshGlobalValueCaptureProvided);

        CapacityFinalCalleeFrameMaskUpdatePlan mismatch =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 0x80000008,
                validatedEntryEcx: 1,
                freshFrameMinus14: 0x80000000,
                freshFrameMinus4: 0x7FFFFFFF,
                freshFrameMinus8: 0xFFFFFFFF,
                freshGlobalValue: 0xFFFFFFFF));
        check("CMP flags are exact and mismatch branches before frame-minus-8 or global access",
            mismatch.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && mismatch.FreshFrameMinus14Address == 0x7FFFFFF4
            && mismatch.FreshFrameMinus4Address == 0x80000004
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
            && mismatch.BranchReached
            && mismatch.BranchTaken == true
            && mismatch.SelectedBranchDestinationRawFileOffset == 0x012C8ED5
            && !mismatch.FreshFrameMinus8ReadReached
            && !mismatch.ShiftReached
            && !mismatch.GlobalReadReached
            && !mismatch.GlobalWriteReached
            && mismatch.UnreachedEntryECXCaptureProvided
            && mismatch.UnreachedFreshFrameMinus8CaptureProvided
            && mismatch.UnreachedFreshGlobalValueCaptureProvided
            && mismatch.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeFrameMaskUpdatePlan missingMinus8 =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 8,
                validatedEntryEcx: 0,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: null,
                freshGlobalValue: 0));
        check("Equality reaches the independent EBP-minus-8 read before requiring later inputs",
            missingMinus8.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus8
            && missingMinus8.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingMinus8.BranchTaken == false
            && missingMinus8.FreshFrameMinus8Address == 0
            && missingMinus8.FreshFrameMinus8ReadReached
            && !missingMinus8.FreshFrameMinus8Captured
            && !missingMinus8.ShiftReached
            && !missingMinus8.GlobalReadReached
            && missingMinus8.UnreachedFreshGlobalValueCaptureProvided);

        CapacityFinalCalleeFrameMaskUpdatePlan missingEcx =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 8,
                validatedEntryEcx: null,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 1,
                freshGlobalValue: 0));
        check("ECX is an independent validated register required only at the shift",
            missingEcx.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryECX
            && missingEcx.FreshFrameMinus8Captured
            && missingEcx.FreshFrameMinus8 == 1
            && !missingEcx.ShiftReached
            && missingEcx.ShiftCount is null
            && !missingEcx.GlobalReadReached
            && !missingEcx.GlobalWriteReached
            && missingEcx.UnreachedFreshGlobalValueCaptureProvided);

        CapacityFinalCalleeFrameMaskUpdatePlan missingGlobal =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 0,
                validatedEntryEcx: 32,
                freshFrameMinus14: 0,
                freshFrameMinus4: 0,
                freshFrameMinus8: 0xFFFFFFFF,
                freshGlobalValue: null));
        check("A zero NOT-mask still requires the fresh global read and cannot invent a write",
            missingGlobal.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshGlobalValue
            && missingGlobal.ShiftCount == 0
            && missingGlobal.ShiftedValue == 0xFFFFFFFF
            && missingGlobal.NotMask == 0
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalValueCaptured
            && missingGlobal.GlobalValueBefore is null
            && !missingGlobal.GlobalWriteReached
            && missingGlobal.GlobalValueAfter is null);

        uint[] inputCounts = [0, 1, 31, 32, 255];
        uint[] expectedCounts = [0, 1, 31, 0, 31];
        uint[] expectedShifted = [
            0x80000001,
            0x00000002,
            0x80000000,
            0x80000001,
            0x80000000
        ];
        bool shiftCountsMatch = true;
        for (int i = 0; i < inputCounts.Length; i++)
        {
            CapacityFinalCalleeFrameMaskUpdatePlan shifted =
                CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                    validatedEntryEbp: 0x1000,
                    validatedEntryEcx: inputCounts[i],
                    freshFrameMinus14: 0x1234,
                    freshFrameMinus4: 0x1234,
                    freshFrameMinus8: 0x80000001,
                    freshGlobalValue: 0xA5A5A5A5));
            shiftCountsMatch &= shifted.Outcome
                    == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
                && shifted.ShiftCount == expectedCounts[i]
                && shifted.ShiftedValue == expectedShifted[i]
                && shifted.NotMask == ~expectedShifted[i]
                && shifted.GlobalValueAfter
                    == (0xA5A5A5A5 & ~expectedShifted[i]);
        }

        check("SHL truncates CL to five bits and wraps its result to a DWORD",
            shiftCountsMatch);

        CapacityFinalCalleeFrameMaskUpdatePlan independentGlobal =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 0x1000,
                validatedEntryEcx: 0,
                freshFrameMinus14: 0x55,
                freshFrameMinus4: 0x55,
                freshFrameMinus8: 0x80000000,
                freshGlobalValue: 0x00000000));
        check("Captured zero global data is independent from the calculated mask",
            independentGlobal.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
            && independentGlobal.NotMask == 0x7FFFFFFF
            && independentGlobal.GlobalReadReached
            && independentGlobal.GlobalValueCaptured
            && independentGlobal.GlobalValueBefore == 0
            && independentGlobal.GlobalWriteReached
            && independentGlobal.GlobalValueAfter == 0
            && independentGlobal.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeFrameMaskUpdatePlan globalWrite =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(Capture(
                validatedEntryEbp: 0x1000,
                validatedEntryEcx: 1,
                freshFrameMinus14: 0x55,
                freshFrameMinus4: 0x55,
                freshFrameMinus8: 0x40000001,
                freshGlobalValue: 0xFFFFFFFF));
        check("The global operation writes the fresh DWORD AND the inverted shifted DWORD",
            globalWrite.ShiftedValue == 0x80000002
            && globalWrite.NotMask == 0x7FFFFFFD
            && globalWrite.GlobalValueBefore == 0xFFFFFFFF
            && globalWrite.GlobalWriteReached
            && globalWrite.GlobalValueAfter == 0x7FFFFFFD
            && globalWrite.StopAtRawFileOffsetExclusive == 0x012C8EF8);
    }

    private static CapacityFinalCalleeFrameMaskUpdateCapture Capture(
        uint? validatedEntryEbp = 0x1000,
        uint? validatedEntryEcx = 1,
        uint? freshFrameMinus14 = 0x1234,
        uint? freshFrameMinus4 = 0x1234,
        uint? freshFrameMinus8 = 0x80000001,
        uint? freshGlobalValue = 0xFFFFFFFF) =>
        new(
            validatedEntryEbp,
            validatedEntryEcx,
            freshFrameMinus14,
            freshFrameMinus4,
            freshFrameMinus8,
            freshGlobalValue);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw)[]
        {
            (0x012C8EBE, "8B 45 EC", null, null),
            (0x012C8EC1, "3B 45 FC", null, null),
            (0x012C8EC4, "75 0F", null, 0x012C8ED5),
            (0x012C8EC6, "8B 45 F8", null, null),
            (0x012C8EC9, "D3 E0", null, null),
            (0x012C8ECB, "F7 D0", null, null),
            (0x012C8ECD, "21 05 C8 AD DA 01", 0x01DADAC8, null),
            (0x012C8ED3, "EB 23", null, 0x012C8EF8)
        };
        uint nextRawOffset =
            CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFrameMaskUpdateInstructionEvidence instruction
            in CapacityFinalCalleeFrameMaskUpdateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFrameMaskUpdateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeFrameMaskUpdateProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFrameMaskUpdateProjection.WindowBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFrameMaskUpdateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFrameMaskUpdateProjection.Instructions
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
