using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSuccessorPairedStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The entry, operand, branch targets, ordered stores, and raw/VA mapping match",
            CapacityFinalCalleeSuccessorPairedStoresProjection.EntryRawFileOffset
                == 0x012C906E
            && CapacityFinalCalleeSuccessorPairedStoresProjection.EntryVirtualAddress
                == 0x016C9C6E
            && CapacityFinalCalleeSuccessorPairedStoresProjection.ValidationDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeSuccessorPairedStoresProjection.ZeroDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeSuccessorPairedStoresProjection.BoundaryRawFileOffsetExclusive
                == 0x012C908B
            && CapacityFinalCalleeSuccessorPairedStoresProjection.GlobalValueVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeSuccessorPairedStoresProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeSuccessorPairedStoresProjection.EntryRawFileOffset)
                == CapacityFinalCalleeSuccessorPairedStoresProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeSuccessorPairedStoresProjection.CapturedWindowBytes)
                == "3B0DD8ADED010F82FCFDFFFF8948108941188B4E1485C90F8467FEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeSuccessorPairedStoresPlan missingEcx =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: null,
                freshGlobalValue: 0,
                entryEax: 0,
                entryEsi: 0,
                freshEsiPlus14: 0));
        check("Missing entry ECX leaves the CMP and every later effect unreached",
            missingEcx.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryECX
            && !missingEcx.EntryECXCaptured
            && !missingEcx.CompareGlobalReadReached
            && !missingEcx.CompareReached
            && missingEcx.CompareResult is null
            && missingEcx.UnreachedFreshGlobalValueCaptureProvided
            && missingEcx.UnreachedEntryEAXCaptureProvided
            && missingEcx.UnreachedEntryESICaptureProvided
            && missingEcx.UnreachedFreshEsiPlus14CaptureProvided
            && missingEcx.Writes.IsEmpty
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C906E);

        CapacityFinalCalleeSuccessorPairedStoresPlan missingGlobal =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x100,
                freshGlobalValue: null,
                entryEax: 0,
                entryEsi: 0,
                freshEsiPlus14: 0));
        check("Missing fresh global data retains ECX but reaches no comparison or stores",
            missingGlobal.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue
            && missingGlobal.EntryECXCaptured
            && missingGlobal.CompareGlobalReadReached
            && !missingGlobal.CompareGlobalValueCaptured
            && !missingGlobal.CompareReached
            && missingGlobal.CompareFlags is null
            && missingGlobal.Reads.Length == 1
            && missingGlobal.Reads[0] is { ValueCaptured: false, Value: null }
            && missingGlobal.UnreachedEntryEAXCaptureProvided
            && missingGlobal.UnreachedEntryESICaptureProvided
            && missingGlobal.UnreachedFreshEsiPlus14CaptureProvided
            && missingGlobal.Writes.IsEmpty
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C906E);

        CapacityFinalCalleeSuccessorPairedStoresPlan unsignedBelow =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x10000000,
                freshGlobalValue: 0xF0000000,
                entryEax: null,
                entryEsi: null,
                freshEsiPlus14: null));
        check("Unsigned-below takes the validation edge and bypasses both stores and the fresh field",
            unsignedBelow.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome
                    .EntryECXUnsignedBelowGlobalToValidation
            && unsignedBelow.CompareResult == 0x20000000
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedBelow.EntryECXUnsignedBelowGlobal == true
            && unsignedBelow.GuardBranchReached
            && unsignedBelow.GuardBranchTaken == true
            && unsignedBelow.GuardBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !unsignedBelow.FirstStoreReached
            && !unsignedBelow.SecondStoreReached
            && !unsignedBelow.FreshEsiPlus14ReadReached
            && !unsignedBelow.UnreachedEntryEAXCaptureProvided
            && !unsignedBelow.UnreachedEntryESICaptureProvided
            && !unsignedBelow.UnreachedFreshEsiPlus14CaptureProvided
            && unsignedBelow.ExactCleanExitAdmitted
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeSuccessorPairedStoresPlan contaminatedBelow =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 1,
                freshGlobalValue: 2,
                entryEax: 0x10,
                entryEsi: null,
                freshEsiPlus14: 0));
        check("A taken guard remains unadmitted when unreachable later captures are supplied",
            contaminatedBelow.GuardBranchTaken == true
            && contaminatedBelow.UnreachedEntryEAXCaptureProvided
            && contaminatedBelow.UnreachedFreshEsiPlus14CaptureProvided
            && !contaminatedBelow.UnreachedEntryESICaptureProvided
            && !contaminatedBelow.ExactCleanExitAdmitted
            && contaminatedBelow.Writes.IsEmpty);

        CapacityFinalCalleeSuccessorPairedStoresPlan highUnsignedNotBelow =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0xF0000000,
                freshGlobalValue: 0x10000000,
                entryEax: 0,
                entryEsi: 0,
                freshEsiPlus14: 0));
        check("High-bit ECX is not unsigned-below a smaller global and reaches the first store",
            highUnsignedNotBelow.CompareResult == 0xE0000000
            && highUnsignedNotBelow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highUnsignedNotBelow.EntryECXUnsignedBelowGlobal == false
            && highUnsignedNotBelow.GuardBranchTaken == false
            && highUnsignedNotBelow.GuardBranchSelectedDestinationRawFileOffset == 0x012C907A
            && highUnsignedNotBelow.FirstStoreReached
            && highUnsignedNotBelow.FirstStoreAddress == 0x10);

        CapacityFinalCalleeSuccessorPairedStoresPlan missingEax =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x100,
                freshGlobalValue: 0x100,
                entryEax: null,
                entryEsi: 0x200,
                freshEsiPlus14: 0x99));
        check("Missing EAX stops at the first store after the guard, before either write",
            missingEax.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryEAXForFirstStore
            && missingEax.CompareResult == 0
            && missingEax.CompareFlags?.Zero == true
            && missingEax.GuardBranchTaken == false
            && !missingEax.EntryEAXCaptured
            && !missingEax.FirstStoreReached
            && !missingEax.SecondStoreReached
            && missingEax.UnreachedEntryESICaptureProvided
            && missingEax.UnreachedFreshEsiPlus14CaptureProvided
            && missingEax.Writes.IsEmpty
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C907A);

        CapacityFinalCalleeSuccessorPairedStoresPlan missingEsi =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x200,
                freshGlobalValue: 0x100,
                entryEax: 0x300,
                entryEsi: null,
                freshEsiPlus14: 0));
        check("Missing ESI preserves both ordered writes and records the reached fresh-load instruction",
            missingEsi.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryESIForFreshRead
            && missingEsi.FirstStoreReached
            && missingEsi.FirstStoreAddress == 0x310
            && missingEsi.FirstStoreValue == 0x200
            && missingEsi.SecondStoreReached
            && missingEsi.SecondStoreAddress == 0x218
            && missingEsi.SecondStoreValue == 0x300
            && missingEsi.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C907Au, 0x310u, 0x200u),
                    (0x012C907Du, 0x218u, 0x300u)
                })
            && missingEsi.FreshEsiPlus14ReadReached
            && !missingEsi.EntryESICaptured
            && missingEsi.FreshEsiPlus14Address is null
            && missingEsi.UnreachedFreshEsiPlus14CaptureProvided
            && !missingEsi.TestFreshEcxReached
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSuccessorPairedStoresPlan missingFresh =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x200,
                freshGlobalValue: 0x100,
                entryEax: 0x300,
                entryEsi: 0xFFFFFFEC,
                freshEsiPlus14: null));
        check("Missing fresh ESI+14 data retains writes and the wrapped read address",
            missingFresh.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshEsiPlus14
            && missingFresh.FirstStoreReached
            && missingFresh.SecondStoreReached
            && missingFresh.FreshEsiPlus14Address == 0
            && missingFresh.FreshEsiPlus14ReadReached
            && !missingFresh.FreshEsiPlus14Captured
            && missingFresh.Reads.Length == 2
            && missingFresh.Reads[1] is { Address: 0, ValueCaptured: false, Value: null }
            && !missingFresh.TestFreshEcxReached
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeSuccessorPairedStoresPlan zero =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0,
                freshGlobalValue: 0,
                entryEax: 0xFFFFFFF0,
                entryEsi: 0xFFFFFFEC,
                freshEsiPlus14: 0));
        check("Captured zero remains distinct and selects the exact zero destination after both writes",
            zero.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14ZeroToTarget
            && zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.FirstStoreAddress == 0
            && zero.FirstStoreValue == 0
            && zero.SecondStoreAddress == 0x18
            && zero.SecondStoreValue == 0xFFFFFFF0
            && zero.FreshEsiPlus14Address == 0
            && zero.FreshEsiPlus14Captured
            && zero.FreshEsiPlus14 == 0
            && zero.TestFreshEcxFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.ZeroBranchReached
            && zero.ZeroBranchTaken == true
            && zero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C8EF2
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeSuccessorPairedStoresPlan nonzero =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x200,
                freshGlobalValue: 0x100,
                entryEax: 0x300,
                entryEsi: 0x400,
                freshEsiPlus14: 0x80000000));
        check("A nonzero fresh value falls through to the bounded 908B edge",
            nonzero.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome
                    .FreshEsiPlus14NonzeroToBoundary
            && nonzero.FreshEsiPlus14 == 0x80000000
            && nonzero.TestFreshEcxFlags is
            {
                Carry: false,
                Parity: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzero.ZeroBranchTaken == false
            && nonzero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C908B
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeSuccessorPairedStoresPlan aliases =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x100,
                freshGlobalValue: 0x80,
                entryEax: 0xFFFFFFF0,
                entryEsi: 0xFFFFFFEC,
                freshEsiPlus14: 0x12345678));
        check("Wrapping stores may alias the fresh read address without changing its independent capture",
            aliases.FirstStoreAddress == 0
            && aliases.SecondStoreAddress == 0x118
            && aliases.FreshEsiPlus14Address == aliases.FirstStoreAddress
            && aliases.FirstStoreValue == 0x100
            && aliases.FreshEsiPlus14 == 0x12345678
            && aliases.FreshEsiPlus14Captured
            && aliases.Writes.Length == 2
            && aliases.Writes[0].RawFileOffset == 0x012C907A
            && aliases.Writes[1].RawFileOffset == 0x012C907D
            && CapacityFinalCalleeSuccessorPairedStoresProjection.DeriveFirstStoreAddress(
                0xFFFFFFF0) == 0
            && CapacityFinalCalleeSuccessorPairedStoresProjection.DeriveSecondStoreAddress(
                0x100) == 0x118
            && CapacityFinalCalleeSuccessorPairedStoresProjection.DeriveFreshEsiPlus14Address(
                0xFFFFFFEC) == 0);

        CapacityFinalCalleeSuccessorPairedStoresPlan globalAliasFirstStore =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(Capture(
                entryEcx: 0x01EDADD8,
                freshGlobalValue: 0x01EDADD8,
                entryEax: 0x01EDADC8,
                entryEsi: 0x500,
                freshEsiPlus14: 0));
        check("The global comparison read remains independent when the first store aliases it",
            globalAliasFirstStore.FirstStoreAddress
                == CapacityFinalCalleeSuccessorPairedStoresProjection.GlobalValueVirtualAddress
            && globalAliasFirstStore.SecondStoreAddress == 0x01EDADF0
            && globalAliasFirstStore.FirstStoreValue == 0x01EDADD8
            && globalAliasFirstStore.SecondStoreValue == 0x01EDADC8
            && globalAliasFirstStore.Reads[0] is
            {
                Address: 0x01EDADD8,
                ValueCaptured: true,
                Value: 0x01EDADD8
            }
            && globalAliasFirstStore.CompareResult == 0
            && globalAliasFirstStore.GuardBranchTaken == false
            && globalAliasFirstStore.FirstStoreReached);
    }

    private static CapacityFinalCalleeSuccessorPairedStoresCapture Capture(
        uint? entryEcx = 0x100,
        uint? freshGlobalValue = 0x80,
        uint? entryEax = 0x200,
        uint? entryEsi = 0x300,
        uint? freshEsiPlus14 = 0x400) =>
        new(entryEcx, freshGlobalValue, entryEax, entryEsi, freshEsiPlus14);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw)[] {
            (0x012C906E, "3B 0D D8 AD ED 01", 0x01EDADD8, null),
            (0x012C9074, "0F 82 FC FD FF FF", null, 0x012C8E76),
            (0x012C907A, "89 48 10", null, null),
            (0x012C907D, "89 41 18", null, null),
            (0x012C9080, "8B 4E 14", null, null),
            (0x012C9083, "85 C9", null, null),
            (0x012C9085, "0F 84 67 FE FF FF", null, 0x012C8EF2)
        };
        uint nextRawOffset =
            CapacityFinalCalleeSuccessorPairedStoresProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeSuccessorPairedStoresInstructionEvidence instruction
            in CapacityFinalCalleeSuccessorPairedStoresProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeSuccessorPairedStoresProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeSuccessorPairedStoresProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeSuccessorPairedStoresProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeSuccessorPairedStoresProjection.CapturedWindowBytes)
            && CapacityFinalCalleeSuccessorPairedStoresProjection.Instructions
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
