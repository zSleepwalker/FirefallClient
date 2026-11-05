using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeLeafClearStoreProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat IDA, PE VA, bytes, and exact two-instruction boundary match",
            CapacityFinalCalleeLargeLeafClearStoreProjection.EntryRawFileOffset
                == 0x012C921D
            && CapacityFinalCalleeLargeLeafClearStoreProjection.EntryFlatIdaAddress
                == 0x012C921D
            && CapacityFinalCalleeLargeLeafClearStoreProjection.EntryVirtualAddress
                == 0x016C9E1D
            && CapacityFinalCalleeLargeLeafClearStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeLeafClearStoreProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeLeafClearStoreProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeLeafClearStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9226
            && Normalize(
                CapacityFinalCalleeLargeLeafClearStoreProjection.CapturedWindowBytes)
                == "8B4DFCC70100000000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeLeafClearStorePlan allMissing =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus4: null));
        check("All missing captures stop before the fresh read and store",
            allMissing.Outcome
                == CapacityFinalCalleeLargeLeafClearStoreOutcome.MissingEntryEBP
            && !allMissing.EntryEBPCaptured
            && !allMissing.FreshFrameReadReached
            && !allMissing.StoreReached
            && allMissing.Reads.IsEmpty
            && allMissing.Writes.IsEmpty
            && allMissing.Events.IsEmpty
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeLeafClearStorePlan unreachedFresh =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus4: 0x12345678));
        check("A supplied fresh value is rejected when missing EBP prevents its read",
            unreachedFresh.UnreachedFreshFrameCaptureProvided
            && !unreachedFresh.FreshFrameReadReached
            && !unreachedFresh.StoreReached
            && unreachedFresh.Reads.IsEmpty
            && unreachedFresh.Writes.IsEmpty
            && !unreachedFresh.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeLeafClearStorePlan missingFresh =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus4: null));
        check("A reached read with missing data stops before the future store",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeLeafClearStoreOutcome.MissingFreshFrameMinus4
            && missingFresh.FreshFrameReadReached
            && missingFresh.FreshFrameAddress == 0xFC
            && !missingFresh.FreshFrameCaptured
            && missingFresh.FreshFrameValue is null
            && missingFresh.Reads is
                [{ RawFileOffset: 0x012C921D, Address: 0xFC, ValueCaptured: false }]
            && missingFresh.Writes.IsEmpty
            && missingFresh.Events is
                [{ Kind: CapacityFinalCalleeLargeLeafClearStoreEventKind.Read }]
            && !missingFresh.StoreReached
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeLeafClearStorePlan zero =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus4: 0));
        check("A captured zero is a valid store address and receives the literal zero",
            zero.Outcome == CapacityFinalCalleeLargeLeafClearStoreOutcome.StoredZero
            && zero.FreshFrameCaptured
            && zero.FreshFrameValue == 0
            && zero.StoreReached
            && zero.StoreAddress == 0
            && zero.StoreValue == 0
            && zero.Writes is
                [{ RawFileOffset: 0x012C9220, Address: 0, Value: 0, Committed: true }]
            && zero.ExactCleanExitAdmitted
            && !zero.DestinationEntered
            && zero.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeLeafClearStorePlan maximum =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: uint.MaxValue,
                FreshFrameMinus4: uint.MaxValue));
        check("Maximum DWORD EBP subtraction wraps and a maximum fresh pointer is stored to",
            maximum.FreshFrameAddress == 0xFFFFFFFB
            && maximum.FreshFrameValue == uint.MaxValue
            && maximum.StoreAddress == uint.MaxValue
            && maximum.StoreValue == 0
            && maximum.Writes is
                [{ Address: uint.MaxValue, Value: 0, Committed: true }]
            && CapacityFinalCalleeLargeLeafClearStoreProjection
                .DeriveEbpMinus4Address(uint.MaxValue) == 0xFFFFFFFB);

        CapacityFinalCalleeLargeLeafClearStorePlan wrapped =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus4: 0xFEDCBA98));
        check("EBP minus four wraps as a DWORD address without altering the fresh pointer",
            wrapped.FreshFrameAddress == 0xFFFFFFFC
            && wrapped.FreshFrameValue == 0xFEDCBA98
            && wrapped.StoreAddress == 0xFEDCBA98
            && wrapped.Writes is [{ Address: 0xFEDCBA98, Value: 0 }]
            && CapacityFinalCalleeLargeLeafClearStoreProjection
                .DeriveEbpMinus4Address(0) == 0xFFFFFFFC);

        CapacityFinalCalleeLargeLeafClearStorePlan aliased =
            CapacityFinalCalleeLargeLeafClearStoreProjection.Project(new(
                EntryEBP: 4,
                FreshFrameMinus4: 0));
        check("Aliased read and write addresses remain distinct ordered instructions",
            aliased.FreshFrameAddress == 0
            && aliased.StoreAddress == 0
            && aliased.Reads.Length == 1
            && aliased.Writes.Length == 1
            && aliased.Events.Select(e => (e.Kind, e.RawFileOffset, e.Address))
                .SequenceEqual(new (CapacityFinalCalleeLargeLeafClearStoreEventKind, uint, uint?)[]
                {
                    (CapacityFinalCalleeLargeLeafClearStoreEventKind.Read,
                        0x012C921Du, 0u),
                    (CapacityFinalCalleeLargeLeafClearStoreEventKind.Write,
                        0x012C9220u, 0u)
                })
            && aliased.Reads[0].RawFileOffset != aliased.Writes[0].RawFileOffset
            && aliased.FlagsUnaffected
            && aliased.ExactCleanExitAdmitted);

        check("MOV instructions leave incoming flags untouched without fabricating their values",
            allMissing.FlagsUnaffected
            && missingFresh.FlagsUnaffected
            && zero.FlagsUnaffected
            && maximum.FlagsUnaffected
            && wrapped.FlagsUnaffected
            && aliased.FlagsUnaffected);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x012C921D, "8B 4D FC"),
            (0x012C9220, "C7 01 00 00 00 00")
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeLeafClearStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeLeafClearStoreInstructionEvidence instruction
            in CapacityFinalCalleeLargeLeafClearStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeLeafClearStoreProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeLeafClearStoreProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeLeafClearStoreProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeLeafClearStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeLeafClearStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
