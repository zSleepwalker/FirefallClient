using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonIndexedNodeInitializationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The indexed-node initialization bytes, offsets, and VA mapping match",
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.EntryRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.EntryVirtualAddress
                == 0x016C9F84
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.LEARawFileOffset
                == 0x012C9387
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.MoveEDXRawFileOffset
                == 0x012C938E
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.SecondStoreRawFileOffset
                == 0x012C9393
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.ThirdStoreRawFileOffset
                == 0x012C939A
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                .BoundaryRawFileOffsetExclusive == 0x012C93A1
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection.EntryRawFileOffset)
                == 0x016C9F84
            && Normalize(
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection.CapturedWindowBytes)
                == "894E1C8D1C8DF8AEED01BA01000000C7461400000000C7461000000000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan missingBoth =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(new(null, null));
        check("Missing both entry operands stops before the first store",
            missingBoth.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome
                    .MissingBothEntryOperands
            && !missingBoth.EntryESICaptured
            && !missingBoth.EntryECXCaptured
            && missingBoth.FirstStoreAddress is null
            && missingBoth.FirstStoreValue is null
            && !missingBoth.FirstStoreReached
            && missingBoth.FirstStoreAttempted
            && !missingBoth.FirstStoreCommitted
            && !missingBoth.LEAReached
            && !missingBoth.MoveEDXReached
            && missingBoth.Writes.IsEmpty
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan missingESI =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(
                new(EntryESI: null, EntryECX: 0x12345678));
        check("Missing ESI retains the known first-store value but stops before writing",
            missingESI.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryESI
            && !missingESI.EntryESICaptured
            && missingESI.EntryECXCaptured
            && missingESI.FirstStoreAddress is null
            && missingESI.FirstStoreValue == 0x12345678
            && !missingESI.FirstStoreReached
            && !missingESI.LEAReached
            && missingESI.Writes.IsEmpty
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan missingECX =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(
                new(EntryESI: 0x1000, EntryECX: null));
        check("Missing ECX retains the known first-store address but stops before writing",
            missingECX.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryECX
            && missingECX.EntryESICaptured
            && !missingECX.EntryECXCaptured
            && missingECX.FirstStoreAddress == 0x101C
            && missingECX.FirstStoreValue is null
            && !missingECX.FirstStoreReached
            && !missingECX.LEAReached
            && missingECX.Writes.IsEmpty
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan zero =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(new(0, 0));
        check("Zero inputs project the base LEA and ordered zero-initialization writes",
            zero.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.Completed
            && zero.FirstStoreReached
            && zero.FirstStoreCommitted
            && zero.FirstStoreAddress == 0x1C
            && zero.FirstStoreValue == 0
            && zero.LEAReached
            && zero.LEAIndex == 0
            && zero.LEAScaledIndex == 0
            && zero.LEABaseAddress == 0x01EDAEF8
            && zero.DerivedEBX == 0x01EDAEF8
            && zero.AddressOnlyLEA
            && !zero.MemoryReadPerformed
            && zero.MoveEDXReached
            && zero.DerivedEDX == 1
            && zero.SecondStoreReached
            && zero.SecondStoreAddress == 0x14
            && zero.SecondStoreValue == 0
            && zero.ThirdStoreReached
            && zero.ThirdStoreAddress == 0x10
            && zero.ThirdStoreValue == 0
            && WritesAreOrdered(zero)
            && zero.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan scaled =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(
                new(EntryESI: 0x1000, EntryECX: 3));
        check("The address-only LEA scales ECX by four before adding its base",
            scaled.LEAIndex == 3
            && scaled.LEAScaledIndex == 12
            && scaled.DerivedEBX == 0x01EDAF04
            && scaled.MemoryReadPerformed == false);

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan wrapped =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(
                new(EntryESI: 0xFFFFFFF0, EntryECX: 0xFFFFFFFF));
        check("LEA and node-field addresses use wrapping DWORD arithmetic",
            wrapped.LEAScaledIndex == 0xFFFFFFFC
            && wrapped.DerivedEBX == 0x01EDAEF4
            && wrapped.FirstStoreAddress == 0x0C
            && wrapped.SecondStoreAddress == 0x04
            && wrapped.ThirdStoreAddress == 0
            && wrapped.Writes.Length == 3);

        check("MOV and LEA do not modify flags, whose incoming state remains unknown",
            !zero.FlagsModified
            && !zero.FlagsAtStopKnown
            && !zero.InstructionBeyondBoundaryExecuted
            && zero.Writes.Length == 3);
    }

    private static bool WritesAreOrdered(
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan plan) =>
        plan.Writes.Length == 3
        && plan.Writes[0].RawFileOffset == 0x012C9384
        && plan.Writes[0].Address == plan.FirstStoreAddress
        && plan.Writes[0].Value == plan.FirstStoreValue
        && plan.Writes[1].RawFileOffset == 0x012C9393
        && plan.Writes[1].Address == plan.SecondStoreAddress
        && plan.Writes[1].Value == 0
        && plan.Writes[2].RawFileOffset == 0x012C939A
        && plan.Writes[2].Address == plan.ThirdStoreAddress
        && plan.Writes[2].Value == 0;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x012C9384, "89 4E 1C"),
            (0x012C9387, "8D 1C 8D F8 AE ED 01"),
            (0x012C938E, "BA 01 00 00 00"),
            (0x012C9393, "C7 46 14 00 00 00 00"),
            (0x012C939A, "C7 46 10 00 00 00 00")
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonIndexedNodeInitializationInstructionEvidence instruction
            in CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .CapturedWindowBytes)
            && CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
