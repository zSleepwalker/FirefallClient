using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeBitScanProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonHighSizeBitScanPlan allMissing =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(null, null, null));
        CapacityFinalCalleeCommonHighSizeBitScanPlan missingEAXButLateValuesPresent =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(null, 0x1000, uint.MaxValue));
        check("The exact raw bytes map contiguously through the bounded pre-store endpoint",
            CapacityFinalCalleeCommonHighSizeBitScanProjection.EntryRawFileOffset
                == 0x012C936F
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.FollowingStoreRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.VirtualAddress))
                .SequenceEqual(new[]
                {
                    (0x012C936Fu, "0F BD C0", 0x016C9F6Fu),
                    (0x012C9372u, "8B D0", 0x016C9F72u),
                    (0x012C9374u, "89 45 EC", 0x016C9F74u),
                    (0x012C9377u, "8B C7", 0x016C9F77u),
                    (0x012C9379u, "8D 4A 07", 0x016C9F79u),
                    (0x012C937Cu, "D3 E8", 0x016C9F7Cu),
                    (0x012C937Eu, "83 E0 01", 0x016C9F7Eu),
                    (0x012C9381u, "8D 0C 50", 0x016C9F81u)
                })
            && BytesAreContiguous()
            && allMissing.Outcome
                == CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingEntryEAXAt936F
            && missingEAXButLateValuesPresent.Outcome
                == CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingEntryEAXAt936F
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C936F
            && missingEAXButLateValuesPresent.StopAtRawFileOffsetExclusive == 0x012C936F
            && allMissing.Events.IsEmpty
            && missingEAXButLateValuesPresent.Events.IsEmpty
            && !allMissing.StoreAttempted
            && !missingEAXButLateValuesPresent.StoreAttempted
            && allMissing.Capture.LateEntryEBP is null
            && missingEAXButLateValuesPresent.Capture.LateEntryEDI == uint.MaxValue);

        CapacityFinalCalleeCommonHighSizeBitScanPlan zero =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(0, 0x1000, uint.MaxValue));
        check("Zero BSR has undefined destination and no successor write or progression",
            zero.Outcome
                == CapacityFinalCalleeCommonHighSizeBitScanOutcome.UndefinedZeroBsrResult
            && zero.BSRReached
            && zero.BSRInputEAX == 0
            && zero.BSRResultEAX is null
            && zero.BSRDestinationDefined == false
            && zero.BSRFlags is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: true,
                Sign: null,
                Overflow: null
            }
            && zero.FlagsAtStop == zero.BSRFlags
            && zero.StopAtRawFileOffsetExclusive == 0x012C9372
            && zero.Events.Length == 1
            && zero.Events[0].RawFileOffset == 0x012C936F
            && zero.Capture.LateEntryEBP == 0x1000
            && zero.Capture.LateEntryEDI == uint.MaxValue
            && !zero.MoveEDXReached
            && !zero.StoreAttempted
            && !zero.MoveEDIToEAXReached
            && !zero.FinalLeaReached
            && zero.Writes.IsEmpty);

        CapacityFinalCalleeCommonHighSizeBitScanPlan missingEBP =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(0x20, null, 0x12345678));
        check("Missing late EBP preserves the derived BSR value but cannot commit the store",
            missingEBP.Outcome
                == CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEBPAt9374
            && missingEBP.BSRResultEAX == 5
            && missingEBP.EDXResult == 5
            && missingEBP.StoreAttempted
            && missingEBP.StoreAddress is null
            && missingEBP.StoreValue == 5
            && !missingEBP.StoreCommitted
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374
            && missingEBP.Events[^1].Kind
                == CapacityFinalCalleeCommonHighSizeBitScanEventKind.StoreEDXToFrame
            && missingEBP.Events[^1].Address is null
            && missingEBP.Events[^1].Input == 5
            && missingEBP.FlagsAtStop == missingEBP.BSRFlags
            && missingEBP.Writes.IsEmpty
            && missingEBP.Capture.LateEntryEDI == 0x12345678);

        CapacityFinalCalleeCommonHighSizeBitScanPlan missingEDI =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(1, 0x20, null));
        CapacityFinalCalleeCommonHighSizeBitScanPlan wrappedStore =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(1, 0x00000003, null));
        check("Known EBP commits the wrapping frame store before a missing EDI stop",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEDIAt9377
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C9377
            && missingEDI.BSRResultEAX == 0
            && missingEDI.StoreAddress == 0x0000000C
            && missingEDI.StoreValue == 0
            && missingEDI.StoreCommitted
            && missingEDI.Writes.SequenceEqual(new uint[] { 0x0000000C })
            && missingEDI.Events[^1].RawFileOffset == 0x012C9374
            && missingEDI.FlagsAtStop == missingEDI.BSRFlags
            && !missingEDI.MoveEDIToEAXReached
            && wrappedStore.StoreAddress == 0xFFFFFFEF
            && wrappedStore.StoreCommitted
            && wrappedStore.Capture.LateEntryEBP == 3);

        CapacityFinalCalleeCommonHighSizeBitScanPlan bit0 =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(1, 0x2000, 0));
        CapacityFinalCalleeCommonHighSizeBitScanPlan bit1 =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(2, 0x2000, 2));
        CapacityFinalCalleeCommonHighSizeBitScanPlan highBit =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(0x80000000, 0x2000, 0x80000000));
        CapacityFinalCalleeCommonHighSizeBitScanPlan uintMax =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(uint.MaxValue, 0x2000, uint.MaxValue));
        check("Nonzero BSR captures bit zero, bit one, the high bit, and uint maximum",
            bit0.BSRResultEAX == 0
            && bit0.BSRFlags is { Zero: false }
            && bit0.FinalIndexECX == 0
            && bit1.BSRResultEAX == 1
            && bit1.FinalIndexECX == 2
            && highBit.BSRResultEAX == 31
            && highBit.FinalIndexECX == 62
            && uintMax.BSRResultEAX == 31
            && uintMax.FinalIndexECX == 63
            && new[] { bit0, bit1, highBit, uintMax }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeBitScanOutcome
                        .IndexedResultAt9384BeforeNextStore
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && plan.StoreCommitted
                && plan.FinalLeaReached
                && plan.Events.Select(item => item.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.BitScanReverse,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEAXToEDX,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.StoreEDXToFrame,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEDIToEAX,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaShiftCount,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.ShiftRight,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.AndOne,
                    CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaIndexedResult
                })));

        CapacityFinalCalleeCommonHighSizeBitScanPlan maskedCountZero =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(1u << 25, 0x100, 1));
        CapacityFinalCalleeCommonHighSizeBitScanPlan count31 =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(1u << 24, 0x100, 1));
        CapacityFinalCalleeCommonHighSizeBitScanPlan rawCount32 =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(1u << 25, 0x100, 1));
        CapacityFinalCalleeCommonHighSizeBitScanPlan rawCount38 =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(new(uint.MaxValue, 0x100, 1));
        check("LEA count is masked to five bits, including zero and counts above 31",
            maskedCountZero.UnmaskedShiftCount == 32
            && maskedCountZero.ShiftCount == 0
            && maskedCountZero.ShiftResultEAX == 1
            && maskedCountZero.ShiftFlags == maskedCountZero.BSRFlags
            && bit0.UnmaskedShiftCount == 7
            && bit0.ShiftCount == 7
            && count31.UnmaskedShiftCount == 31
            && count31.ShiftCount == 31
            && count31.ShiftResultEAX == 0
            && rawCount32.UnmaskedShiftCount == 32
            && rawCount32.ShiftCount == 0
            && rawCount38.UnmaskedShiftCount == 38
            && rawCount38.ShiftCount == 6
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.EffectiveShiftCount(25) == 0
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.EffectiveShiftCount(24) == 31
            && CapacityFinalCalleeCommonHighSizeBitScanProjection.EffectiveShiftCount(31) == 6);

        CapacityFinalCalleeCommonHighSizeBitScanPlan shiftCountOne =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(1u << 26, 0x100, 0x80000001));
        CapacityFinalCalleeCommonHighSizeBitScanPlan shiftCountTwo =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(1u << 27, 0x100, 0xC0000007));
        check("SHR flags follow x86 count rules and AND/LEA determine final flags",
            shiftCountOne.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && shiftCountTwo.UnmaskedShiftCount == 34
            && shiftCountTwo.ShiftCount == 2
            && shiftCountTwo.ShiftFlags is
            {
                Carry: true,
                Auxiliary: null,
                Overflow: null
            }
            && shiftCountOne.AndResultEAX == 0
            && shiftCountOne.AndFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && shiftCountTwo.AndResultEAX == 1
            && shiftCountTwo.AndFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && shiftCountTwo.FlagsAtStop == shiftCountTwo.AndFlags
            && shiftCountTwo.Events[^1].Kind
                == CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaIndexedResult
            && shiftCountTwo.Events[^1].Flags == shiftCountTwo.AndFlags
            && shiftCountTwo.FinalIndexECX == 55);
    }

    private static bool BytesAreContiguous()
    {
        uint nextRawOffset =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonHighSizeBitScanInstructionEvidence instruction
            in CapacityFinalCalleeCommonHighSizeBitScanProjection.Instructions)
        {
            string normalized = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonHighSizeBitScanProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonHighSizeBitScanProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == string.Concat(
                CapacityFinalCalleeCommonHighSizeBitScanProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
