using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeConditionalFieldStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The conditional-store bytes, raw/VA mapping, and branch targets match",
            CapacityFinalCalleeConditionalFieldStoreProjection.EntryRawFileOffset
                == 0x012C903B
            && CapacityFinalCalleeConditionalFieldStoreProjection.EntryVirtualAddress
                == 0x016C9C3B
            && CapacityFinalCalleeConditionalFieldStoreProjection.FirstBranchDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeConditionalFieldStoreProjection.ZeroBranchDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeConditionalFieldStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9058
            && CapacityFinalCalleeConditionalFieldStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeConditionalFieldStoreProjection.EntryRawFileOffset)
                == CapacityFinalCalleeConditionalFieldStoreProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeConditionalFieldStoreProjection.CapturedWindowBytes)
                == "3BCB0F8233FEFFFF3971107505894110EB0389411485C00F849AFEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeConditionalFieldStorePlan missingEcx =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: null,
                entryEbx: 0,
                freshEcxPlus10: 0,
                entryEsi: 0,
                entryEax: 0));
        check("Missing first-use ECX blocks the CMP and marks all later captures unreached",
            missingEcx.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryECX
            && !missingEcx.EntryECXCaptured
            && !missingEcx.EntryEBXCaptured
            && missingEcx.UnreachedEntryEBXCaptureProvided
            && missingEcx.UnreachedFreshEcxPlus10CaptureProvided
            && missingEcx.UnreachedEntryESICaptureProvided
            && missingEcx.UnreachedEntryEAXCaptureProvided
            && missingEcx.FirstCompareResult is null
            && !missingEcx.FirstBranchReached
            && !missingEcx.FreshEcxPlus10ReadReached
            && !missingEcx.StoreReached
            && !missingEcx.TestEAXReached
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeConditionalFieldStorePlan missingEbx =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0,
                entryEbx: null,
                freshEcxPlus10: 0,
                entryEsi: 0,
                entryEax: 0));
        check("Missing second CMP input retains ECX but reaches no comparison or later read",
            missingEbx.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryEBX
            && missingEbx.EntryECXCaptured
            && !missingEbx.EntryEBXCaptured
            && missingEbx.FirstCompareResult is null
            && missingEbx.UnreachedFreshEcxPlus10CaptureProvided
            && missingEbx.UnreachedEntryESICaptureProvided
            && missingEbx.UnreachedEntryEAXCaptureProvided
            && !missingEbx.FirstBranchReached
            && !missingEbx.FreshEcxPlus10ReadReached
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeConditionalFieldStorePlan unsignedBelow =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x10000000,
                entryEbx: 0xF0000000,
                freshEcxPlus10: 0x22,
                entryEsi: 0x22,
                entryEax: 1));
        check("The first CMP uses unsigned below and routes directly to the shared validation call",
            unsignedBelow.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .EntryECXUnsignedBelowEntryEBXToValidation
            && unsignedBelow.FirstCompareResult == 0x20000000
            && unsignedBelow.FirstCompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedBelow.EntryECXUnsignedBelowEntryEBX == true
            && unsignedBelow.FirstBranchReached
            && unsignedBelow.FirstBranchTaken == true
            && unsignedBelow.FirstBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.UnreachedFreshEcxPlus10CaptureProvided
            && unsignedBelow.UnreachedEntryESICaptureProvided
            && unsignedBelow.UnreachedEntryEAXCaptureProvided
            && !unsignedBelow.FreshEcxPlus10ReadReached
            && !unsignedBelow.StoreReached
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeConditionalFieldStorePlan highUnsignedNotBelow =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0xF0000000,
                entryEbx: 0x10000000,
                freshEcxPlus10: 0,
                entryEsi: 0,
                entryEax: 0));
        check("A high-bit ECX is not treated as unsigned-below when EBX is smaller",
            highUnsignedNotBelow.FirstCompareResult == 0xE0000000
            && highUnsignedNotBelow.FirstCompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highUnsignedNotBelow.EntryECXUnsignedBelowEntryEBX == false
            && highUnsignedNotBelow.FirstBranchTaken == false
            && highUnsignedNotBelow.FirstBranchSelectedDestinationRawFileOffset
                == 0x012C9043
            && highUnsignedNotBelow.FreshEcxPlus10Address == 0xF0000010
            && highUnsignedNotBelow.FreshEcxPlus10Captured
            && highUnsignedNotBelow.FreshEcxPlus10EqualEntryESI == true);

        CapacityFinalCalleeConditionalFieldStorePlan missingFresh =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0xFFFFFFF8,
                entryEbx: 0xFFFFFFF8,
                freshEcxPlus10: null,
                entryEsi: 0,
                entryEax: 0));
        check("Missing fresh ECX+10 stops at its load after the first compare falls through",
            missingFresh.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingFreshEcxPlus10
            && missingFresh.FirstCompareResult == 0
            && missingFresh.FirstCompareFlags?.Zero == true
            && missingFresh.FirstBranchTaken == false
            && missingFresh.FreshEcxPlus10Address == 8
            && missingFresh.FreshEcxPlus10ReadReached
            && !missingFresh.FreshEcxPlus10Captured
            && missingFresh.UnreachedEntryESICaptureProvided
            && missingFresh.UnreachedEntryEAXCaptureProvided
            && !missingFresh.SecondBranchReached
            && !missingFresh.StoreReached
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9043);

        CapacityFinalCalleeConditionalFieldStorePlan missingEsi =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x200,
                entryEbx: 0x100,
                freshEcxPlus10: 0x1234,
                entryEsi: null,
                entryEax: 0));
        check("The fresh memory capture is retained when the independent ESI operand is absent",
            missingEsi.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryESI
            && missingEsi.FreshEcxPlus10Address == 0x210
            && missingEsi.FreshEcxPlus10ReadReached
            && missingEsi.FreshEcxPlus10Captured
            && missingEsi.FreshEcxPlus10 == 0x1234
            && !missingEsi.EntryESICaptured
            && missingEsi.SecondCompareResult is null
            && missingEsi.UnreachedEntryEAXCaptureProvided
            && !missingEsi.SecondBranchReached
            && !missingEsi.StoreReached
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9043);

        CapacityFinalCalleeConditionalFieldStorePlan missingEaxEqual =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x100,
                entryEbx: 0x100,
                freshEcxPlus10: 0xABC,
                entryEsi: 0xABC,
                entryEax: null));
        check("Missing EAX after an equal fresh comparison retains flags but performs no store",
            missingEaxEqual.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .MissingEntryEAXForPlus10Store
            && missingEaxEqual.SecondCompareResult == 0
            && missingEaxEqual.SecondCompareFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingEaxEqual.FreshEcxPlus10EqualEntryESI == true
            && missingEaxEqual.SecondBranchReached
            && missingEaxEqual.SecondBranchTaken == false
            && missingEaxEqual.SecondBranchSelectedDestinationRawFileOffset == 0x012C9048
            && !missingEaxEqual.EntryEAXCaptured
            && !missingEaxEqual.StoreReached
            && !missingEaxEqual.TestEAXReached
            && missingEaxEqual.StopAtRawFileOffsetExclusive == 0x012C9048);

        CapacityFinalCalleeConditionalFieldStorePlan missingEaxUnequal =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x100,
                entryEbx: 0x100,
                freshEcxPlus10: 0x101,
                entryEsi: 0x100,
                entryEax: null));
        check("Missing EAX after an unequal fresh comparison selects the plus-14 store but stops there",
            missingEaxUnequal.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .MissingEntryEAXForPlus14Store
            && missingEaxUnequal.SecondCompareResult == 1
            && missingEaxUnequal.SecondCompareFlags?.Carry == false
            && missingEaxUnequal.FreshEcxPlus10EqualEntryESI == false
            && missingEaxUnequal.SecondBranchTaken == true
            && missingEaxUnequal.SecondBranchSelectedDestinationRawFileOffset == 0x012C904D
            && !missingEaxUnequal.StoreReached
            && !missingEaxUnequal.TestEAXReached
            && missingEaxUnequal.StopAtRawFileOffsetExclusive == 0x012C904D);

        CapacityFinalCalleeConditionalFieldStorePlan equalZero =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x100,
                entryEbx: 0x100,
                freshEcxPlus10: 0,
                entryEsi: 0,
                entryEax: 0));
        check("Equal fresh data selects the plus-10 store, jumps to TEST, and branches on captured zero",
            equalZero.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome.Plus10StoredThenEaxZeroToTarget
            && equalZero.FreshEcxPlus10 == 0
            && equalZero.SecondCompareResult == 0
            && equalZero.SecondCompareFlags?.Zero == true
            && equalZero.SecondBranchTaken == false
            && equalZero.SecondBranchSelectedDestinationRawFileOffset == 0x012C9048
            && equalZero.StoreReached
            && equalZero.StoreAddress == 0x110
            && equalZero.StoreDisplacement == 0x10
            && equalZero.StoreValue == 0
            && equalZero.JumpToTestReached
            && equalZero.JumpToTestDestinationRawFileOffset == 0x012C9050
            && equalZero.TestEAXReached
            && equalZero.TestEAXValue == 0
            && equalZero.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.ZeroBranchTaken == true
            && equalZero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C8EF2
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeConditionalFieldStorePlan unequalNonzero =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x200,
                entryEbx: 0x100,
                freshEcxPlus10: 0x80000000,
                entryEsi: 0x7FFFFFFF,
                entryEax: 0x80000001));
        check("Unequal fresh data selects plus-14 and nonzero TEST falls through to the bounded edge",
            unequalNonzero.Outcome
                == CapacityFinalCalleeConditionalFieldStoreOutcome
                    .Plus14StoredThenEaxNonzeroToBoundary
            && unequalNonzero.SecondCompareResult == 1
            && unequalNonzero.SecondCompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && unequalNonzero.SecondBranchTaken == true
            && unequalNonzero.SecondBranchSelectedDestinationRawFileOffset == 0x012C904D
            && unequalNonzero.StoreAddress == 0x214
            && unequalNonzero.StoreDisplacement == 0x14
            && unequalNonzero.StoreValue == 0x80000001
            && !unequalNonzero.JumpToTestReached
            && unequalNonzero.TestEAXValue == 0x80000001
            && unequalNonzero.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unequalNonzero.ZeroBranchTaken == false
            && unequalNonzero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9058
            && unequalNonzero.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeConditionalFieldStorePlan wrappedReadAndStores =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0xFFFFFFF0,
                entryEbx: 0,
                freshEcxPlus10: 0x55,
                entryEsi: 0x56,
                entryEax: 0xAABBCCDD));
        check("Fresh reads and selected write addresses use wrapping DWORD arithmetic",
            wrappedReadAndStores.FreshEcxPlus10Address == 0
            && wrappedReadAndStores.FreshEcxPlus10 == 0x55
            && wrappedReadAndStores.SecondCompareResult == 0xFFFFFFFF
            && wrappedReadAndStores.SecondCompareFlags?.Carry == true
            && wrappedReadAndStores.StoreAddress == 4
            && wrappedReadAndStores.StoreDisplacement == 0x14
            && wrappedReadAndStores.StoreValue == 0xAABBCCDD
            && CapacityFinalCalleeConditionalFieldStoreProjection.DeriveFreshEcxPlus10Address(
                0xFFFFFFF0) == 0
            && CapacityFinalCalleeConditionalFieldStoreProjection.DeriveStoreAddress(
                0xFFFFFFF0, 0x14) == 4);

        CapacityFinalCalleeConditionalFieldStorePlan independentRead =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(Capture(
                entryEcx: 0x300,
                entryEbx: 0x200,
                freshEcxPlus10: 0x12345678,
                entryEsi: 0x12345678,
                entryEax: 0xCAFEBABE));
        check("The fresh ECX+10 value remains an independent read and can select a store despite entry values",
            independentRead.FreshEcxPlus10Address == 0x310
            && independentRead.FreshEcxPlus10Captured
            && independentRead.FreshEcxPlus10 == 0x12345678
            && independentRead.EntryESICaptured
            && independentRead.SecondCompareResult == 0
            && independentRead.StoreAddress == 0x310
            && independentRead.StoreValue == 0xCAFEBABE
            && independentRead.TestEAXReached
            && independentRead.TestEAXValue == 0xCAFEBABE);
    }

    private static CapacityFinalCalleeConditionalFieldStoreCapture Capture(
        uint? entryEcx = 0x100,
        uint? entryEbx = 0x100,
        uint? freshEcxPlus10 = 0x20,
        uint? entryEsi = 0x20,
        uint? entryEax = 0x21) =>
        new(entryEcx, entryEbx, freshEcxPlus10, entryEsi, entryEax);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C903B, "3B CB", null, null),
            (0x012C903D, "0F 82 33 FE FF FF", 0x012C8E76, 0x016C9A76),
            (0x012C9043, "39 71 10", null, null),
            (0x012C9046, "75 05", 0x012C904D, 0x016C9C4D),
            (0x012C9048, "89 41 10", null, null),
            (0x012C904B, "EB 03", 0x012C9050, 0x016C9C50),
            (0x012C904D, "89 41 14", null, null),
            (0x012C9050, "85 C0", null, null),
            (0x012C9052, "0F 84 9A FE FF FF", 0x012C8EF2, 0x016C9AF2)
        };
        uint nextRawOffset =
            CapacityFinalCalleeConditionalFieldStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeConditionalFieldStoreInstructionEvidence instruction
            in CapacityFinalCalleeConditionalFieldStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeConditionalFieldStoreProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeConditionalFieldStoreProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeConditionalFieldStoreProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeConditionalFieldStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeConditionalFieldStoreProjection.Instructions
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
