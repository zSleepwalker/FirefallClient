using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeGuardedFieldStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, PE mapping, global operand, and branch targets match",
            CapacityFinalCalleeGuardedFieldStoreProjection.EntryRawFileOffset
                == 0x012C9058
            && CapacityFinalCalleeGuardedFieldStoreProjection.EntryVirtualAddress
                == 0x016C9C58
            && CapacityFinalCalleeGuardedFieldStoreProjection.ValidationDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeGuardedFieldStoreProjection.ZeroDestinationRawFileOffset
                == 0x012C9080
            && CapacityFinalCalleeGuardedFieldStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C906E
            && CapacityFinalCalleeGuardedFieldStoreProjection.GlobalValueVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeGuardedFieldStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeGuardedFieldStoreProjection.EntryRawFileOffset)
                == CapacityFinalCalleeGuardedFieldStoreProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeGuardedFieldStoreProjection.CapturedWindowBytes)
                == "3B05D8ADED010F8212FEFFFF8948188B4E1085C97412"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeGuardedFieldStorePlan missingEax =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: null));
        check("Missing first-use EAX stops at CMP and rejects later supplied captures",
            missingEax.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryEAXForCompare
            && !missingEax.EntryEAXCaptured
            && !missingEax.CompareGlobalReadReached
            && missingEax.Reads.IsEmpty
            && !missingEax.CompareReached
            && !missingEax.BranchReached
            && !missingEax.StoreReached
            && !missingEax.FreshEsiPlus10ReadReached
            && missingEax.UnreachedFreshGlobalForCompareCaptureProvided
            && missingEax.UnreachedEntryECXCaptureProvided
            && missingEax.UnreachedEntryESICaptureProvided
            && missingEax.UnreachedFreshEsiPlus10CaptureProvided
            && !missingEax.ExactCleanExitAdmitted
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeGuardedFieldStorePlan missingGlobal =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                freshGlobalForCompare: null));
        check("Missing global data is a reached CMP read, not a captured zero or later store",
            missingGlobal.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingFreshGlobalForCompare
            && missingGlobal.EntryEAXCaptured
            && missingGlobal.CompareGlobalReadReached
            && !missingGlobal.CompareGlobalCaptured
            && missingGlobal.Reads.Length == 1
            && missingGlobal.Reads[0].Address == 0x01EDADD8
            && !missingGlobal.Reads[0].ValueCaptured
            && missingGlobal.Reads[0].Value is null
            && !missingGlobal.CompareReached
            && missingGlobal.CompareResult is null
            && missingGlobal.UnreachedEntryECXCaptureProvided
            && missingGlobal.UnreachedEntryESICaptureProvided
            && missingGlobal.UnreachedFreshEsiPlus10CaptureProvided
            && missingGlobal.Writes.IsEmpty
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeGuardedFieldStorePlan unsignedBelow =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: 1,
                freshGlobalForCompare: 0xF0000000));
        check("CMP/JB uses unsigned carry and branches before all later fields are consumed",
            unsignedBelow.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .EntryEAXUnsignedBelowGlobalToValidation
            && unsignedBelow.CompareResult == 0x10000001
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedBelow.EntryEAXUnsignedBelowGlobal == true
            && unsignedBelow.BranchReached
            && unsignedBelow.BranchTaken == true
            && unsignedBelow.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.Writes.IsEmpty
            && !unsignedBelow.FreshEsiPlus10ReadReached
            && unsignedBelow.UnreachedEntryECXCaptureProvided
            && unsignedBelow.UnreachedEntryESICaptureProvided
            && unsignedBelow.UnreachedFreshEsiPlus10CaptureProvided
            && !unsignedBelow.ExactCleanExitAdmitted
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedFieldStorePlan cleanBelow =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(new(
                EntryEAX: 0,
                FreshGlobalForCompare: 1,
                EntryECX: null,
                EntryESI: null,
                FreshEsiPlus10: null));
        check("The unsigned-below route is admitted only with its exact two captures",
            cleanBelow.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .EntryEAXUnsignedBelowGlobalToValidation
            && !cleanBelow.UnreachedEntryECXCaptureProvided
            && !cleanBelow.UnreachedEntryESICaptureProvided
            && !cleanBelow.UnreachedFreshEsiPlus10CaptureProvided
            && cleanBelow.ExactCleanExitAdmitted
            && cleanBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeGuardedFieldStorePlan unsignedEqual =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: 0xF0000000,
                freshGlobalForCompare: 0xF0000000));
        check("Unsigned equality falls through with ZF set and no below branch",
            unsignedEqual.CompareResult == 0
            && unsignedEqual.CompareFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && unsignedEqual.EntryEAXUnsignedBelowGlobal == false
            && unsignedEqual.BranchTaken == false
            && unsignedEqual.BranchSelectedDestinationRawFileOffset == 0x012C9064
            && unsignedEqual.StoreReached
            && unsignedEqual.ExactCleanExitAdmitted);

        CapacityFinalCalleeGuardedFieldStorePlan highUnsigned =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: 0xF0000000,
                freshGlobalForCompare: 0x10000000));
        check("A high-bit EAX greater than the global is not treated as unsigned-below",
            highUnsigned.CompareResult == 0xE0000000
            && highUnsigned.CompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highUnsigned.EntryEAXUnsignedBelowGlobal == false
            && highUnsigned.BranchTaken == false
            && highUnsigned.StoreReached);

        CapacityFinalCalleeGuardedFieldStorePlan missingEcx =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEcx: null));
        check("Missing ECX stops at the guarded store after CMP/JB and before any write",
            missingEcx.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryECXForStore
            && missingEcx.CompareReached
            && missingEcx.EntryEAXUnsignedBelowGlobal == false
            && missingEcx.BranchReached
            && missingEcx.BranchTaken == false
            && !missingEcx.EntryECXCaptured
            && !missingEcx.StoreReached
            && missingEcx.Writes.IsEmpty
            && missingEcx.UnreachedEntryESICaptureProvided
            && missingEcx.UnreachedFreshEsiPlus10CaptureProvided
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9064);

        CapacityFinalCalleeGuardedFieldStorePlan missingEsi =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEsi: null));
        check("Missing ESI preserves the earlier ECX store and stops at the fresh load",
            missingEsi.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryESIForRead
            && missingEsi.StoreReached
            && missingEsi.Writes.Length == 1
            && missingEsi.Writes[0]
                == new CapacityFinalCalleeGuardedFieldStoreWrite(
                    0x012C9064, 0x118, 0xCAFEBABE)
            && missingEsi.FreshEsiPlus10ReadReached
            && !missingEsi.EntryESICaptured
            && missingEsi.FreshEsiPlus10Address is null
            && missingEsi.Reads.Length == 2
            && missingEsi.Reads[1].RawFileOffset == 0x012C9067
            && missingEsi.Reads[1].Address is null
            && !missingEsi.Reads[1].ValueCaptured
            && missingEsi.UnreachedFreshEsiPlus10CaptureProvided
            && !missingEsi.TestECXReached
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C9067);

        CapacityFinalCalleeGuardedFieldStorePlan missingFresh =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEsi: 0xFFFFFFF8,
                freshEsiPlus10: null));
        check("A missing fresh ESI+10 value retains the prior write and its wrapped read address",
            missingFresh.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.MissingFreshEsiPlus10
            && missingFresh.Writes.Length == 1
            && missingFresh.StoreAddress == 0x118
            && missingFresh.StoreValue == 0xCAFEBABE
            && missingFresh.FreshEsiPlus10ReadReached
            && missingFresh.EntryESICaptured
            && missingFresh.FreshEsiPlus10Address == 8
            && !missingFresh.FreshEsiPlus10Captured
            && missingFresh.Reads.Length == 2
            && missingFresh.Reads[1].Address == 8
            && !missingFresh.Reads[1].ValueCaptured
            && !missingFresh.TestECXReached
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9067);

        CapacityFinalCalleeGuardedFieldStorePlan zeroAlias =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: 0xFFFFFFF0,
                freshGlobalForCompare: 0xFFFFFFF0,
                entryEcx: 0xCAFED00D,
                entryEsi: 0xFFFFFFF8,
                freshEsiPlus10: 0));
        check("The wrapping store precedes an independent aliasing fresh read and zero branch",
            zeroAlias.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome.FreshEsiPlus10ZeroToTarget
            && zeroAlias.CompareFlags?.Zero == true
            && zeroAlias.BranchTaken == false
            && zeroAlias.StoreAddress == 8
            && zeroAlias.StoreValue == 0xCAFED00D
            && zeroAlias.StoreDisplacement == 0x18
            && zeroAlias.Writes.Length == 1
            && zeroAlias.FreshEsiPlus10Address == 8
            && zeroAlias.FreshEsiPlus10 == 0
            && zeroAlias.Reads.Length == 2
            && zeroAlias.Reads[1].RawFileOffset == 0x012C9067
            && zeroAlias.Reads[1].Value == 0
            && zeroAlias.TestECXReached
            && zeroAlias.TestECXValue == 0
            && zeroAlias.TestECXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroAlias.ZeroBranchReached
            && zeroAlias.ZeroBranchTaken == true
            && zeroAlias.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9080
            && zeroAlias.ExactCleanExitAdmitted
            && zeroAlias.StopAtRawFileOffsetExclusive == 0x012C9080);

        CapacityFinalCalleeGuardedFieldStorePlan nonzeroBoundary =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(Capture(
                entryEax: 0x80000000,
                freshGlobalForCompare: 0x7FFFFFFF,
                entryEcx: 0x80000001,
                entryEsi: 0xFFFFFFF8,
                freshEsiPlus10: 0x80000001));
        check("A nonzero fresh load falls through to the bounded edge with its own TEST flags",
            nonzeroBoundary.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10NonzeroToBoundary
            && nonzeroBoundary.CompareResult == 1
            && nonzeroBoundary.CompareFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && nonzeroBoundary.StoreAddress == 0x80000018
            && nonzeroBoundary.StoreValue == 0x80000001
            && nonzeroBoundary.FreshEsiPlus10Address == 8
            && nonzeroBoundary.FreshEsiPlus10 == 0x80000001
            && nonzeroBoundary.TestECXValue == 0x80000001
            && nonzeroBoundary.TestECXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroBoundary.ZeroBranchTaken == false
            && nonzeroBoundary.ZeroBranchSelectedDestinationRawFileOffset == 0x012C906E
            && nonzeroBoundary.ExactCleanExitAdmitted
            && nonzeroBoundary.StopAtRawFileOffsetExclusive == 0x012C906E);
    }

    private static CapacityFinalCalleeGuardedFieldStoreCapture Capture(
        uint? entryEax = 0x100,
        uint? freshGlobalForCompare = 0x100,
        uint? entryEcx = 0xCAFEBABE,
        uint? entryEsi = 0x200,
        uint? freshEsiPlus10 = 0x20) =>
        new(entryEax, freshGlobalForCompare, entryEcx, entryEsi, freshEsiPlus10);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddress,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9058, "3B 05 D8 AD ED 01", 0x01EDADD8, null, null),
            (0x012C905E, "0F 82 12 FE FF FF", null, 0x012C8E76, 0x016C9A76),
            (0x012C9064, "89 48 18", null, null, null),
            (0x012C9067, "8B 4E 10", null, null, null),
            (0x012C906A, "85 C9", null, null, null),
            (0x012C906C, "74 12", null, 0x012C9080, 0x016C9C80)
        };
        uint nextRawOffset =
            CapacityFinalCalleeGuardedFieldStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeGuardedFieldStoreInstructionEvidence instruction
            in CapacityFinalCalleeGuardedFieldStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeGuardedFieldStoreProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.PeVirtualAddressOperand is not null
                    && instruction.PeVirtualAddressOperand != 0x01EDADD8)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeGuardedFieldStoreProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeGuardedFieldStoreProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeGuardedFieldStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeGuardedFieldStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
