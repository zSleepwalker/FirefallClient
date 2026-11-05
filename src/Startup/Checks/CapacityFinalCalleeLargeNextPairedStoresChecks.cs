using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeLargeNextPairedStoresProjection;

internal static class CapacityFinalCalleeLargeNextPairedStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat, and mapped virtual evidence matches both MOV stores",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeNextPairedStoresPlan missingBoth =
            Projection.Project(new(EntryEAX: null, EntryECX: null));
        check("Missing both first-store operands stops at 929E without either write",
            missingBoth.Outcome
                == CapacityFinalCalleeLargeNextPairedStoresOutcome
                    .MissingBothFirstStoreOperands
            && missingBoth.FirstStoreReached
            && missingBoth.FirstStoreAddressAttempted
            && missingBoth.FirstStoreValueAttempted
            && missingBoth.FirstStoreAddress is null
            && missingBoth.FirstStoreValue is null
            && !missingBoth.FirstStoreCommitted
            && !missingBoth.SecondStoreReached
            && !missingBoth.SecondStoreCommitted
            && missingBoth.Writes.IsEmpty
            && missingBoth.FlagsUnaffected
            && !missingBoth.ExactCleanCompletion
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C929E);

        CapacityFinalCalleeLargeNextPairedStoresPlan missingEAX =
            Projection.Project(new(EntryEAX: null, EntryECX: 0x12345678));
        CapacityFinalCalleeLargeNextPairedStoresPlan missingECX =
            Projection.Project(new(EntryEAX: 0x87654321, EntryECX: null));
        check("The first store independently retains its known value or address",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreAddress
            && !missingEAX.EntryEAXCaptured
            && missingEAX.EntryECXCaptured
            && missingEAX.FirstStoreAddress is null
            && missingEAX.FirstStoreValue == 0x12345678
            && missingEAX.FirstStoreAddressAttempted
            && missingEAX.FirstStoreValueAttempted
            && !missingEAX.FirstStoreCommitted
            && !missingEAX.SecondStoreReached
            && missingEAX.Writes.IsEmpty
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C929E
            && missingECX.Outcome
                == CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreValue
            && missingECX.EntryEAXCaptured
            && !missingECX.EntryECXCaptured
            && missingECX.FirstStoreAddress == 0x87654331
            && missingECX.FirstStoreValue is null
            && missingECX.FirstStoreAddressAttempted
            && missingECX.FirstStoreValueAttempted
            && !missingECX.FirstStoreCommitted
            && !missingECX.SecondStoreReached
            && missingECX.Writes.IsEmpty
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C929E);

        CapacityFinalCalleeLargeNextPairedStoresPlan zero =
            Projection.Project(new(EntryEAX: 0, EntryECX: 0));
        check("Zero registers produce two ordered writes and stop exactly at 92A4",
            zero.Outcome == CapacityFinalCalleeLargeNextPairedStoresOutcome.Completed
            && zero.FirstStoreAddress == 0x10
            && zero.FirstStoreValue == 0
            && zero.FirstStoreCommitted
            && zero.SecondStoreReached
            && zero.SecondStoreAddress == 0x18
            && zero.SecondStoreValue == 0
            && zero.SecondStoreCommitted
            && zero.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C929E, 0x10, 0),
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C92A1, 0x18, 0)
            })
            && zero.FlagsUnaffected
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C92A4
            && !zero.BoundaryInstructionExecuted
            && !zero.FreshReadReached);

        CapacityFinalCalleeLargeNextPairedStoresPlan maximum =
            Projection.Project(new(EntryEAX: uint.MaxValue, EntryECX: uint.MaxValue));
        check("Maximum register values retain DWORD-width addresses and both values",
            maximum.FirstStoreAddress == 0x0F
            && maximum.FirstStoreValue == uint.MaxValue
            && maximum.SecondStoreAddress == 0x17
            && maximum.SecondStoreValue == uint.MaxValue
            && maximum.Writes.Length == 2
            && maximum.Writes[0].Address == 0x0F
            && maximum.Writes[1].Address == 0x17
            && maximum.ExactCleanCompletion
            && maximum.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeNextPairedStoresPlan wrapped =
            Projection.Project(new(EntryEAX: 0xFFFFFFF8, EntryECX: 0xFFFFFFF0));
        check("Both effective addresses use unsigned DWORD wraparound",
            wrapped.FirstStoreAddress == 0x08
            && wrapped.SecondStoreAddress == 0x08
            && wrapped.Writes.Length == 2
            && wrapped.Writes[0].Value == 0xFFFFFFF0
            && wrapped.Writes[1].Value == 0xFFFFFFF8
            && wrapped.Writes[0].RawFileOffset == 0x012C929E
            && wrapped.Writes[1].RawFileOffset == 0x012C92A1);

        CapacityFinalCalleeLargeNextPairedStoresPlan alias =
            Projection.Project(new(EntryEAX: 0x100, EntryECX: 0xF8));
        check("Aliased effective addresses preserve both distinct values in instruction order",
            alias.FirstStoreAddress == 0x110
            && alias.SecondStoreAddress == 0x110
            && alias.FirstStoreValue == 0xF8
            && alias.SecondStoreValue == 0x100
            && alias.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C929E, 0x110, 0xF8),
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C92A1, 0x110, 0x100)
            })
            && alias.Writes[0].Address == alias.Writes[1].Address
            && alias.Writes[0].Value != alias.Writes[1].Value
            && alias.FlagsUnaffected
            && alias.ExactCleanCompletion);

        CapacityFinalCalleeLargeNextPairedStoresPlan distinct =
            Projection.Project(new(EntryEAX: 0x100, EntryECX: 0x200));
        check("Non-aliased effective addresses and stored values are independently derived",
            distinct.FirstStoreAddress == 0x110
            && distinct.FirstStoreValue == 0x200
            && distinct.SecondStoreAddress == 0x218
            && distinct.SecondStoreValue == 0x100
            && distinct.Writes.Length == 2
            && distinct.Writes[0].Address != distinct.Writes[1].Address
            && distinct.Writes[0].Value != distinct.Writes[1].Value);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x012C929E, "894810"),
            (0x012C92A1, "894118")
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeNextPairedStoresInstructionEvidence instruction
            in Projection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != Projection.RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return Projection.EntryRawFileOffset == 0x012C929E
            && Projection.EntryVirtualAddress == 0x016C9E9E
            && Projection.RawFileOffsetToVirtualAddress(Projection.EntryRawFileOffset)
                == Projection.EntryVirtualAddress
            && Projection.FirstStoreRawFileOffset == 0x012C929E
            && Projection.SecondStoreRawFileOffset == 0x012C92A1
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C92A4
            && Projection.FreshReadRawFileOffset == 0x012C92A4
            && nextRawOffset == Projection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, Normalize(instruction.Bytes)))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
