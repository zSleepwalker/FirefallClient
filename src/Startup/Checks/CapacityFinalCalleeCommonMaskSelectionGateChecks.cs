using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonMaskSelectionGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The mask selection MOV/SHL/MOV/TEST/JNZ bytes and mapped addresses match",
            CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset
                == 0x012C930A
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryVirtualAddress
                == 0x016C9F0A
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.ShiftRawFileOffset
                == 0x012C930F
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalMoveRawFileOffset
                == 0x012C9311
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.TestRawFileOffset
                == 0x012C9317
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.JnzRawFileOffset
                == 0x012C9319
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.ZeroDestinationRawFileOffset
                == 0x012C931B
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.NonzeroDestinationRawFileOffset
                == 0x012C9334
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalVirtualAddress
                == 0x01EDADC8
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset)
                == 0x016C9F0A
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.RawFileOffsetToVirtualAddress(
                uint.MaxValue) == 0x00400BFF
            && Normalize(CapacityFinalCalleeCommonMaskSelectionGateProjection.CapturedWindowBytes)
                == "BF01000000D3E78B0DC8ADED0185F97519"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonMaskSelectionGatePlan allMissing =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(new(null, null));
        check("Missing ECX stops before SHL and leaves the absolute global read unreached",
            allMissing.Outcome
                == CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingEntryECX
            && allMissing.MoveEDIReached
            && allMissing.DestinationValueAfterMove == 1
            && !allMissing.ShiftReached
            && allMissing.DestinationValueAfterShift is null
            && allMissing.ShiftFlags is null
            && !allMissing.FreshGlobalReadReached
            && !allMissing.TestReached
            && !allMissing.JnzReached
            && !allMissing.ExactCleanBoundedDestination
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C930F);

        CapacityFinalCalleeCommonMaskSelectionGatePlan unusedGlobal =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: null, FreshGlobalValue: 0xFFFFFFFF));
        check("A supplied global value is not consumed when the preceding shift is unevidenced",
            unusedGlobal.UnreachedFreshGlobalCaptureProvided
            && !unusedGlobal.FreshGlobalReadReached
            && !unusedGlobal.FreshGlobalValueCaptured
            && !unusedGlobal.TestReached
            && unusedGlobal.StopAtRawFileOffsetExclusive == 0x012C930F);

        CapacityFinalCalleeCommonMaskSelectionGatePlan missingGlobal =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 2, FreshGlobalValue: null));
        check("A missing global read stops at 9311 but retains the reached shifted destination and flags",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingFreshGlobalValue
            && missingGlobal.ShiftReached
            && missingGlobal.ShiftCount == 2
            && missingGlobal.DestinationValueAfterShift == 4
            && missingGlobal.FreshGlobalReadReached
            && missingGlobal.FreshGlobalReadAddress == 0x01EDADC8
            && !missingGlobal.FreshGlobalValueCaptured
            && !missingGlobal.TestReached
            && missingGlobal.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && missingGlobal.FlagsAtStop == missingGlobal.ShiftFlags
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9311);

        CapacityFinalCalleeCommonMaskSelectionGatePlan zeroCount =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 0, FreshGlobalValue: null));
        check("A zero-count SHL preserves unknown incoming flags while keeping EDI equal to one",
            zeroCount.ShiftCount == 0
            && zeroCount.DestinationValueAfterShift == 1
            && zeroCount.ShiftFlags is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && zeroCount.FlagsAtStop == zeroCount.ShiftFlags);

        CapacityFinalCalleeCommonMaskSelectionGatePlan oneCount =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 1, FreshGlobalValue: null));
        check("A one-bit shift defines CF, OF, and P/S/ZF while leaving AF undefined",
            oneCount.ShiftCount == 1
            && oneCount.DestinationValueAfterShift == 2
            && oneCount.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeCommonMaskSelectionGatePlan thirtyOneCount =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 31, FreshGlobalValue: null));
        check("A 31-bit shift yields 80000000h with undefined multi-bit OF and defined other shift flags",
            thirtyOneCount.DestinationValueAfterShift == 0x80000000
            && thirtyOneCount.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: null
            });

        CapacityFinalCalleeCommonMaskSelectionGatePlan maskedThirtyTwo =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 32, FreshGlobalValue: null));
        CapacityFinalCalleeCommonMaskSelectionGatePlan maskedAllBits =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: uint.MaxValue, FreshGlobalValue: null));
        check("SHL masks ECX counts to five bits, including 32 to zero and FFFFFFFF to 31",
            maskedThirtyTwo.ShiftCount == 0
            && maskedThirtyTwo.DestinationValueAfterShift == 1
            && maskedThirtyTwo.ShiftFlags == zeroCount.ShiftFlags
            && maskedAllBits.ShiftCount == 31
            && maskedAllBits.DestinationValueAfterShift == 0x80000000
            && maskedAllBits.ShiftFlags == thirtyOneCount.ShiftFlags);

        CapacityFinalCalleeCommonMaskSelectionGatePlan zeroTest =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 1, FreshGlobalValue: 0));
        check("A zero TEST result branches to 931B with TEST flags superseding shift flags",
            zeroTest.Outcome
                == CapacityFinalCalleeCommonMaskSelectionGateOutcome.ZeroTestResultToBounded931B
            && zeroTest.TestResult == 0
            && zeroTest.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroTest.FlagsAtStop == zeroTest.TestFlags
            && zeroTest.JnzTaken == false
            && zeroTest.JnzSelectedDestinationRawFileOffset == 0x012C931B
            && zeroTest.ExactCleanBoundedDestination
            && zeroTest.StopAtRawFileOffsetExclusive == 0x012C931B);

        CapacityFinalCalleeCommonMaskSelectionGatePlan nonzeroTest =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 1, FreshGlobalValue: 2));
        check("A nonzero TEST result branches to 9334 with CF/OF clear and correct result flags",
            nonzeroTest.Outcome
                == CapacityFinalCalleeCommonMaskSelectionGateOutcome
                    .NonzeroTestResultToBounded9334
            && nonzeroTest.TestResult == 2
            && nonzeroTest.TestFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && nonzeroTest.JnzTaken == true
            && nonzeroTest.JnzSelectedDestinationRawFileOffset == 0x012C9334
            && nonzeroTest.ExactCleanBoundedDestination
            && nonzeroTest.StopAtRawFileOffsetExclusive == 0x012C9334);

        CapacityFinalCalleeCommonMaskSelectionGatePlan highBitSelection =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 31, FreshGlobalValue: 0x80000000));
        CapacityFinalCalleeCommonMaskSelectionGatePlan highBitUnselected =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.Project(
                new(EntryECX: 31, FreshGlobalValue: 0x7FFFFFFF));
        check("The high shifted mask selects only the matching fresh-global bit",
            highBitSelection.TestResult == 0x80000000
            && highBitSelection.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBitSelection.JnzTaken == true
            && highBitUnselected.TestResult == 0
            && highBitUnselected.JnzTaken == false);

        check("Both bounded destinations stop before any later OR or store",
            new[] { zeroTest, nonzeroTest, highBitSelection, highBitUnselected }
                .All(plan => plan.ExactCleanBoundedDestination
                    && plan.JnzReached
                    && !plan.LaterOrOrStoreReached
                    && !plan.MemoryWritePerformed
                    && plan.Writes.IsEmpty)
            && zeroTest.StopAtRawFileOffsetExclusive == 0x012C931B
            && nonzeroTest.StopAtRawFileOffsetExclusive == 0x012C9334);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C930A, "BF 01 00 00 00", null, null),
            (0x012C930F, "D3 E7", null, null),
            (0x012C9311, "8B 0D C8 AD ED 01", null, null),
            (0x012C9317, "85 F9", null, null),
            (0x012C9319, "75 19", 0x012C9334, 0x016C9F34)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonMaskSelectionGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonMaskSelectionGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonMaskSelectionGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonMaskSelectionGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonMaskSelectionGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonMaskSelectionGateProjection.JnzRawFileOffset + 2
            && bytes == Normalize(
                CapacityFinalCalleeCommonMaskSelectionGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonMaskSelectionGateProjection.Instructions
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
