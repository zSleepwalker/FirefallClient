using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedRootMaskChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, raw/virtual addresses, operands, and four bounded destinations match",
            CapacityFinalCalleeIndexedRootMaskProjection.EntryRawFileOffset
                == 0x012C8FFE
            && CapacityFinalCalleeIndexedRootMaskProjection.EntryVirtualAddress
                == 0x016C9BFE
            && CapacityFinalCalleeIndexedRootMaskProjection.FirstIndexLoadRawFileOffset
                == 0x012C9009
            && CapacityFinalCalleeIndexedRootMaskProjection.FirstTableCompareRawFileOffset
                == 0x012C900C
            && CapacityFinalCalleeIndexedRootMaskProjection.FreshFrameMinusCLoadRawFileOffset
                == 0x012C9013
            && CapacityFinalCalleeIndexedRootMaskProjection.SecondIndexLoadRawFileOffset
                == 0x012C9018
            && CapacityFinalCalleeIndexedRootMaskProjection.TableStoreRawFileOffset
                == 0x012C901B
            && CapacityFinalCalleeIndexedRootMaskProjection.ThirdIndexLoadRawFileOffset
                == 0x012C9029
            && CapacityFinalCalleeIndexedRootMaskProjection.GlobalAndRawFileOffset
                == 0x012C9030
            && CapacityFinalCalleeIndexedRootMaskProjection.WindowBoundaryRawFileOffsetExclusive
                == 0x012C903B
            && CapacityFinalCalleeIndexedRootMaskProjection.ZeroExitRawFileOffset
                == 0x012C8EF8
            && CapacityFinalCalleeIndexedRootMaskProjection.UnequalExitRawFileOffset
                == 0x012C903B
            && CapacityFinalCalleeIndexedRootMaskProjection.NonzeroExitRawFileOffset
                == 0x012C9058
            && CapacityFinalCalleeIndexedRootMaskProjection.MaskContinuationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeIndexedRootMaskProjection.IndexedTablePeVirtualAddress
                == 0x01EDAEF8
            && CapacityFinalCalleeIndexedRootMaskProjection.GlobalMaskVirtualAddress
                == 0x01EDADCC
            && CapacityFinalCalleeIndexedRootMaskProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeIndexedRootMaskProjection.EntryRawFileOffset)
                == CapacityFinalCalleeIndexedRootMaskProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeIndexedRootMaskProjection.CapturedWindowBytes)
                == "8B4DEC85C90F84EFFEFFFF8B561C3B3495F8AEED018B55F475238B5E1C89049DF8AEED0185C075328B45F88B4E1CD3E0F7D02105CCADED01E9B7FEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeIndexedRootMaskPlan entryNotValidated =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                captureValidatedEntryRegisters: false,
                entryEbp: 0x1000,
                entryEsi: 0x2000,
                entryEax: 0));
        check("Unvalidated entry registers stop before any read and are reported as unreached",
            entryNotValidated.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.EntryNotValidated
            && !entryNotValidated.FreshFrameMinus14ReadReached
            && entryNotValidated.FreshFrameMinus14Address is null
            && entryNotValidated.UnreachedEntryRegisterCaptureProvided
            && entryNotValidated.UnreachedFreshFrameMinus14CaptureProvided
            && entryNotValidated.UnreachedFreshFirstIndexCaptureProvided
            && entryNotValidated.UnreachedFirstIndexedTableValueCaptureProvided
            && entryNotValidated.UnreachedFreshFrameMinusCCaptureProvided
            && entryNotValidated.UnreachedFreshSecondIndexCaptureProvided
            && entryNotValidated.UnreachedFreshFrameMinus8CaptureProvided
            && entryNotValidated.UnreachedFreshThirdIndexCaptureProvided
            && entryNotValidated.UnreachedFreshGlobalValueCaptureProvided
            && entryNotValidated.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeIndexedRootMaskPlan zeroExit =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                freshFrameMinus14: 0));
        check("A captured zero at EBP-14 takes only the EF8 exit and leaves all later captures unreached",
            zeroExit.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.FreshFrameMinus14ZeroExit
            && zeroExit.FreshFrameMinus14Address == 0xFEC
            && zeroExit.ZeroTestReached
            && zeroExit.ZeroTestResult == true
            && zeroExit.ZeroBranchReached
            && zeroExit.ZeroBranchTaken == true
            && zeroExit.ZeroBranchDestinationRawFileOffset == 0x012C8EF8
            && !zeroExit.FirstIndexReadReached
            && !zeroExit.FirstCompareReached
            && !zeroExit.TableWriteReached
            && !zeroExit.GlobalWriteReached
            && zeroExit.UnreachedFreshFirstIndexCaptureProvided
            && zeroExit.UnreachedFreshFrameMinusCCaptureProvided
            && zeroExit.UnreachedFreshGlobalValueCaptureProvided
            && zeroExit.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeIndexedRootMaskPlan missingMinus14 =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEbp: 0x10,
                freshFrameMinus14: null));
        check("A missing fresh EBP-minus-14 read is distinct from a captured zero and wraps its address",
            missingMinus14.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus14
            && missingMinus14.FreshFrameMinus14Address == 0xFFFFFFFC
            && missingMinus14.FreshFrameMinus14ReadReached
            && !missingMinus14.FreshFrameMinus14Captured
            && missingMinus14.FreshFrameMinus14 is null
            && !missingMinus14.ZeroTestReached
            && missingMinus14.UnreachedFreshFirstIndexCaptureProvided
            && missingMinus14.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeIndexedRootMaskPlan missingEntryEsi =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: null));
        check("A missing entry ESI blocks the first fresh indexed read",
            missingEntryEsi.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryESI
            && missingEntryEsi.ZeroBranchReached
            && missingEntryEsi.ZeroBranchTaken == false
            && !missingEntryEsi.FirstIndexReadReached
            && missingEntryEsi.FirstIndexAddress is null
            && missingEntryEsi.UnreachedFreshFirstIndexCaptureProvided
            && missingEntryEsi.StopAtRawFileOffsetExclusive == 0x012C9009);

        CapacityFinalCalleeIndexedRootMaskPlan missingFirstIndex =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                freshFirstIndex: null));
        check("A missing fresh first index stops at its load before the indexed table operand",
            missingFirstIndex.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFirstIndex
            && missingFirstIndex.FirstIndexAddress == 0x201C
            && missingFirstIndex.FirstIndexReadReached
            && !missingFirstIndex.FirstIndexCaptured
            && !missingFirstIndex.FirstIndexedTableReadReached
            && missingFirstIndex.UnreachedFirstIndexedTableValueCaptureProvided
            && missingFirstIndex.StopAtRawFileOffsetExclusive == 0x012C9009);

        CapacityFinalCalleeIndexedRootMaskPlan missingTableValue =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0xFFFFFFF0,
                freshFirstIndex: 0xFFFFFFFF,
                firstIndexedTableValue: null));
        check("The first indexed table read uses its own index and wraps address arithmetic",
            missingTableValue.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFirstIndexedTableValue
            && missingTableValue.FirstIndexAddress == 0x0000000C
            && missingTableValue.FirstIndexCaptured
            && missingTableValue.FirstIndexedTableAddress == 0x01EDAEF4
            && missingTableValue.FirstIndexedTableReadReached
            && !missingTableValue.FirstIndexedTableValueCaptured
            && !missingTableValue.FirstCompareReached
            && missingTableValue.UnreachedFreshFrameMinusCCaptureProvided
            && missingTableValue.StopAtRawFileOffsetExclusive == 0x012C900C);

        CapacityFinalCalleeIndexedRootMaskPlan missingFrameMinusC =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x80000000,
                freshFirstIndex: 0xFFFFFFFF,
                firstIndexedTableValue: 1,
                freshFrameMinusC: null));
        check("The fresh EBP-minus-C read is mandatory before JNZ even though CMP flags survive the MOV",
            missingFrameMinusC.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinusC
            && missingFrameMinusC.FreshFrameMinusCAddress == 0xFF4
            && missingFrameMinusC.FirstCompareResult == 0x7FFFFFFF
            && missingFrameMinusC.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && missingFrameMinusC.FirstCompareEqual == false
            && missingFrameMinusC.FreshFrameMinusCReadReached
            && !missingFrameMinusC.FreshFrameMinusCCaptured
            && !missingFrameMinusC.UnequalBranchReached
            && !missingFrameMinusC.SecondIndexReadReached
            && missingFrameMinusC.UnreachedFreshSecondIndexCaptureProvided
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x012C9013);

        CapacityFinalCalleeIndexedRootMaskPlan unequalExit =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x80000000,
                freshFirstIndex: 0xFFFFFFFF,
                firstIndexedTableValue: 1,
                freshFrameMinusC: 0xDEADBEEF));
        check("The retained unequal CMP flags take the 903B route after the independent EDX reload",
            unequalExit.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal
            && unequalExit.FreshFrameMinusC == 0xDEADBEEF
            && unequalExit.FirstCompareFlags
                == missingFrameMinusC.FirstCompareFlags
            && unequalExit.UnequalBranchReached
            && unequalExit.UnequalBranchTaken == true
            && unequalExit.UnequalBranchDestinationRawFileOffset == 0x012C903B
            && !unequalExit.SecondIndexReadReached
            && !unequalExit.TableWriteReached
            && unequalExit.UnreachedFreshSecondIndexCaptureProvided
            && unequalExit.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeIndexedRootMaskPlan missingEntryEax =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x1234,
                entryEax: null,
                freshFirstIndex: 11,
                firstIndexedTableValue: 0x1234,
                freshFrameMinusC: 7,
                freshSecondIndex: 22));
        check("The second independent index is read before a missing store-value register blocks the write",
            missingEntryEax.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryEAX
            && missingEntryEax.FreshFirstIndex == 11
            && missingEntryEax.FreshSecondIndex == 22
            && missingEntryEax.FirstCompareEqual == true
            && missingEntryEax.SecondIndexReadReached
            && missingEntryEax.SecondIndexCaptured
            && !missingEntryEax.TableWriteReached
            && missingEntryEax.UnreachedFreshFrameMinus8CaptureProvided
            && missingEntryEax.StopAtRawFileOffsetExclusive == 0x012C901B);

        CapacityFinalCalleeIndexedRootMaskPlan missingSecondIndex =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x1234,
                freshFirstIndex: 11,
                firstIndexedTableValue: 0x1234,
                freshFrameMinusC: 7,
                freshSecondIndex: null));
        check("A missing fresh second index stops before the store and does not reuse the first index",
            missingSecondIndex.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshSecondIndex
            && missingSecondIndex.FreshFirstIndex == 11
            && missingSecondIndex.SecondIndexAddress == 0x1250
            && missingSecondIndex.SecondIndexReadReached
            && !missingSecondIndex.SecondIndexCaptured
            && !missingSecondIndex.TableWriteReached
            && missingSecondIndex.UnreachedFreshFrameMinus8CaptureProvided
            && missingSecondIndex.StopAtRawFileOffsetExclusive == 0x012C9018);

        CapacityFinalCalleeIndexedRootMaskPlan nonzeroExit =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x1234,
                entryEax: 0xAABBCCDD,
                freshFirstIndex: 11,
                firstIndexedTableValue: 0x1234,
                freshFrameMinusC: 0,
                freshSecondIndex: 22));
        check("A nonzero EAX is first written at its independently reloaded index, then exits to 9058",
            nonzeroExit.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.StoredEntryEAXNonzeroExit
            && nonzeroExit.FirstIndexedTableAddress
                == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(11)
            && nonzeroExit.TableWriteAddress
                == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(22)
            && nonzeroExit.FreshFirstIndex == 11
            && nonzeroExit.FreshSecondIndex == 22
            && nonzeroExit.TableWriteReached
            && nonzeroExit.TableWriteValue == 0xAABBCCDD
            && nonzeroExit.StoredValueTestReached
            && nonzeroExit.StoredValueIsZero == false
            && nonzeroExit.NonzeroBranchTaken == true
            && nonzeroExit.NonzeroBranchDestinationRawFileOffset == 0x012C9058
            && !nonzeroExit.FreshFrameMinus8ReadReached
            && nonzeroExit.UnreachedFreshFrameMinus8CaptureProvided
            && nonzeroExit.UnreachedFreshThirdIndexCaptureProvided
            && nonzeroExit.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeIndexedRootMaskPlan missingFrameMinus8 =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                freshFrameMinus8: null));
        check("A missing fresh EBP-minus-8 read stops at its load after preserving the indexed table write",
            missingFrameMinus8.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus8
            && missingFrameMinus8.FreshFrameMinus8Address == 0xFF8
            && missingFrameMinus8.FreshFrameMinus8ReadReached
            && !missingFrameMinus8.FreshFrameMinus8Captured
            && missingFrameMinus8.TableWriteReached
            && missingFrameMinus8.TableWriteValue == 0
            && missingFrameMinus8.StoredValueIsZero == true
            && !missingFrameMinus8.ThirdIndexReadReached
            && missingFrameMinus8.UnreachedFreshThirdIndexCaptureProvided
            && missingFrameMinus8.StopAtRawFileOffsetExclusive == 0x012C9026);

        CapacityFinalCalleeIndexedRootMaskPlan missingThirdIndex =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x2000,
                entryEax: 0,
                freshFirstIndex: 1,
                firstIndexedTableValue: 0x2000,
                freshFrameMinusC: 2,
                freshSecondIndex: 3,
                freshFrameMinus8: 0x80000001,
                freshThirdIndex: null));
        check("A missing third index cannot reuse either prior fresh index and preserves the earlier table write",
            missingThirdIndex.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshThirdIndex
            && missingThirdIndex.FreshFirstIndex == 1
            && missingThirdIndex.FreshSecondIndex == 3
            && missingThirdIndex.FreshThirdIndex is null
            && missingThirdIndex.ThirdIndexReadReached
            && missingThirdIndex.TableWriteReached
            && missingThirdIndex.TableWriteAddress
                == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(3)
            && missingThirdIndex.TableWriteValue == 0
            && !missingThirdIndex.GlobalReadReached
            && missingThirdIndex.StopAtRawFileOffsetExclusive == 0x012C9029);

        uint[] rawCounts = [0, 31, 32, 63, 255];
        uint[] maskedCounts = [0, 31, 0, 31, 31];
        uint[] shiftedValues = [
            0x80000001,
            0x80000000,
            0x80000001,
            0x80000000,
            0x80000000
        ];
        bool shiftCountsMatch = true;
        for (int i = 0; i < rawCounts.Length; i++)
        {
            CapacityFinalCalleeIndexedRootMaskPlan shifted =
                CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                    entryEsi: 0x4000,
                    entryEax: 0,
                    freshFirstIndex: 7,
                    firstIndexedTableValue: 0x4000,
                    freshFrameMinusC: 0,
                    freshSecondIndex: 8,
                    freshFrameMinus8: 0x80000001,
                    freshThirdIndex: rawCounts[i],
                    freshGlobalValue: 0xFFFFFFFF));
            shiftCountsMatch &= shifted.Outcome
                    == CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated
                && shifted.FreshFirstIndex == 7
                && shifted.FreshSecondIndex == 8
                && shifted.FreshThirdIndex == rawCounts[i]
                && shifted.ShiftCount == maskedCounts[i]
                && shifted.ShiftedValue == shiftedValues[i]
                && shifted.NotMask == ~shiftedValues[i]
                && shifted.GlobalValueAfter == ~shiftedValues[i];
        }

        check("The three fresh ESI indices stay independent and SHL masks CL to five bits",
            shiftCountsMatch);

        CapacityFinalCalleeIndexedRootMaskPlan zeroShiftedMask =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x4000,
                entryEax: 0,
                freshFirstIndex: 7,
                firstIndexedTableValue: 0x4000,
                freshFrameMinusC: 0,
                freshSecondIndex: 8,
                freshFrameMinus8: 0,
                freshThirdIndex: 32,
                freshGlobalValue: 0x12345678));
        check("A zero shifted value yields an all-ones NOT mask and leaves a captured global value unchanged",
            zeroShiftedMask.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated
            && zeroShiftedMask.ShiftCount == 0
            && zeroShiftedMask.ShiftedValue == 0
            && zeroShiftedMask.NotMask == 0xFFFFFFFF
            && zeroShiftedMask.GlobalValueBefore == 0x12345678
            && zeroShiftedMask.GlobalWriteReached
            && zeroShiftedMask.GlobalValueAfter == 0x12345678
            && zeroShiftedMask.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeIndexedRootMaskPlan missingGlobal =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0x4000,
                entryEax: 0,
                freshFirstIndex: 7,
                firstIndexedTableValue: 0x4000,
                freshFrameMinusC: 0,
                freshSecondIndex: 8,
                freshFrameMinus8: 0xFFFFFFFF,
                freshThirdIndex: 31,
                freshGlobalValue: null));
        check("A missing global capture stops before AND without rolling back the indexed table write",
            missingGlobal.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshGlobalValue
            && missingGlobal.TableWriteReached
            && missingGlobal.TableWriteValue == 0
            && missingGlobal.ShiftCount == 31
            && missingGlobal.ShiftedValue == 0x80000000
            && missingGlobal.NotMask == 0x7FFFFFFF
            && missingGlobal.GlobalAddress == 0x01EDADCC
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalValueCaptured
            && !missingGlobal.GlobalWriteReached
            && missingGlobal.GlobalValueAfter is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9030);

        CapacityFinalCalleeIndexedRootMaskPlan allZeroValues =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(Capture(
                entryEsi: 0,
                entryEax: 0,
                freshFrameMinus14: 1,
                freshFirstIndex: 0,
                firstIndexedTableValue: 0,
                freshFrameMinusC: 0,
                freshSecondIndex: 0,
                freshFrameMinus8: 0,
                freshThirdIndex: 0,
                freshGlobalValue: 0));
        check("Captured zero inputs remain distinct from missing inputs while the indexed and global writes execute",
            allZeroValues.FreshFrameMinus14Captured
            && allZeroValues.FirstIndexCaptured
            && allZeroValues.FirstIndexedTableValueCaptured
            && allZeroValues.FreshFrameMinusCCaptured
            && allZeroValues.SecondIndexCaptured
            && allZeroValues.TableWriteReached
            && allZeroValues.TableWriteValue == 0
            && allZeroValues.FreshFrameMinus8Captured
            && allZeroValues.ThirdIndexCaptured
            && allZeroValues.GlobalValueCaptured
            && allZeroValues.GlobalWriteReached
            && allZeroValues.GlobalValueAfter == 0
            && allZeroValues.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated);
    }

    private static CapacityFinalCalleeIndexedRootMaskCapture Capture(
        bool captureValidatedEntryRegisters = true,
        uint? entryEbp = 0x1000,
        uint? entryEsi = 0x2000,
        uint? entryEax = 0,
        uint? freshFrameMinus14 = 1,
        uint? freshFirstIndex = 3,
        uint? firstIndexedTableValue = 0x2000,
        uint? freshFrameMinusC = 0,
        uint? freshSecondIndex = 4,
        uint? freshFrameMinus8 = 0x80000001,
        uint? freshThirdIndex = 5,
        uint? freshGlobalValue = 0xFFFFFFFF) =>
        new(
            captureValidatedEntryRegisters,
            entryEbp,
            entryEsi,
            entryEax,
            freshFrameMinus14,
            freshFirstIndex,
            firstIndexedTableValue,
            freshFrameMinusC,
            freshSecondIndex,
            freshFrameMinus8,
            freshThirdIndex,
            freshGlobalValue);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw)[]
        {
            (0x012C8FFE, "8B 4D EC", null, null),
            (0x012C9001, "85 C9", null, null),
            (0x012C9003, "0F 84 EF FE FF FF", null, 0x012C8EF8),
            (0x012C9009, "8B 56 1C", null, null),
            (0x012C900C, "3B 34 95 F8 AE ED 01", 0x01EDAEF8, null),
            (0x012C9013, "8B 55 F4", null, null),
            (0x012C9016, "75 23", null, 0x012C903B),
            (0x012C9018, "8B 5E 1C", null, null),
            (0x012C901B, "89 04 9D F8 AE ED 01", 0x01EDAEF8, null),
            (0x012C9022, "85 C0", null, null),
            (0x012C9024, "75 32", null, 0x012C9058),
            (0x012C9026, "8B 45 F8", null, null),
            (0x012C9029, "8B 4E 1C", null, null),
            (0x012C902C, "D3 E0", null, null),
            (0x012C902E, "F7 D0", null, null),
            (0x012C9030, "21 05 CC AD ED 01", 0x01EDADCC, null),
            (0x012C9036, "E9 B7 FE FF FF", null, 0x012C8EF2)
        };
        uint nextRawOffset =
            CapacityFinalCalleeIndexedRootMaskProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeIndexedRootMaskInstructionEvidence instruction
            in CapacityFinalCalleeIndexedRootMaskProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeIndexedRootMaskProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeIndexedRootMaskProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeIndexedRootMaskProjection.WindowBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeIndexedRootMaskProjection.CapturedWindowBytes)
            && CapacityFinalCalleeIndexedRootMaskProjection.Instructions
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
