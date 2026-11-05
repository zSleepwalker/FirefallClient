using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeLargeRightPairedStoresProjection;

internal static class CapacityFinalCalleeLargeRightPairedStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat, and mapped virtual evidence matches both MOV stores",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightPairedStoresPlan missingBoth =
            Projection.Project(new(EntryEAX: null, EntryECX: null, IncomingEFlags: 0x246));
        check("Missing both first-store operands retains flags and stops without either write",
            missingBoth.Outcome
                == CapacityFinalCalleeLargeRightPairedStoresOutcome
                    .MissingBothFirstStoreOperands
            && missingBoth.FirstStoreReached
            && missingBoth.FirstStoreAddressAttempted
            && missingBoth.FirstStoreValueAttempted
            && missingBoth.FirstStoreAddress is null
            && missingBoth.FirstStoreValue is null
            && !missingBoth.FirstStoreCommitted
            && !missingBoth.SecondStoreReached
            && !missingBoth.SecondStoreAddressAttempted
            && !missingBoth.SecondStoreValueAttempted
            && !missingBoth.SecondStoreCommitted
            && missingBoth.Writes.IsEmpty
            && missingBoth.IncomingEFlagsCaptured
            && missingBoth.IncomingEFlags == 0x246
            && missingBoth.EFlagsAtStop == 0x246
            && missingBoth.FlagsUnaffected
            && !missingBoth.ExactCleanCompletion
            && missingBoth.StopAtRawFileOffsetExclusive == Projection.EntryRawFileOffset);

        CapacityFinalCalleeLargeRightPairedStoresPlan missingEAX =
            Projection.Project(new(EntryEAX: null, EntryECX: 0x12345678));
        CapacityFinalCalleeLargeRightPairedStoresPlan missingECX =
            Projection.Project(new(EntryEAX: 0x87654321, EntryECX: null));
        check("A missing address or value retains the known first-store operand and blocks store two",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingFirstStoreAddress
            && !missingEAX.EntryEAXCaptured
            && missingEAX.EntryECXCaptured
            && missingEAX.FirstStoreAddress is null
            && missingEAX.FirstStoreValue == 0x12345678
            && missingEAX.FirstStoreAddressAttempted
            && missingEAX.FirstStoreValueAttempted
            && !missingEAX.FirstStoreCommitted
            && !missingEAX.SecondStoreReached
            && !missingEAX.SecondStoreAddressAttempted
            && !missingEAX.SecondStoreValueAttempted
            && !missingEAX.SecondStoreCommitted
            && missingEAX.Writes.IsEmpty
            && missingEAX.StopAtRawFileOffsetExclusive == Projection.EntryRawFileOffset
            && missingECX.Outcome
                == CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingFirstStoreValue
            && missingECX.EntryEAXCaptured
            && !missingECX.EntryECXCaptured
            && missingECX.FirstStoreAddress == 0x87654335
            && missingECX.FirstStoreValue is null
            && missingECX.FirstStoreAddressAttempted
            && missingECX.FirstStoreValueAttempted
            && !missingECX.FirstStoreCommitted
            && !missingECX.SecondStoreReached
            && !missingECX.SecondStoreAddressAttempted
            && !missingECX.SecondStoreValueAttempted
            && !missingECX.SecondStoreCommitted
            && missingECX.Writes.IsEmpty
            && missingECX.StopAtRawFileOffsetExclusive == Projection.EntryRawFileOffset);

        CapacityFinalCalleeLargeRightPairedStoresPlan zero =
            Projection.Project(new(EntryEAX: 0, EntryECX: 0));
        check("Zero registers produce two ordered stores and stop exactly at 92BD",
            zero.Outcome == CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed
            && zero.FirstStoreAddress == 0x14
            && zero.FirstStoreValue == 0
            && zero.FirstStoreCommitted
            && zero.SecondStoreReached
            && zero.SecondStoreAddressAttempted
            && zero.SecondStoreValueAttempted
            && zero.SecondStoreAddress == 0x18
            && zero.SecondStoreValue == 0
            && zero.SecondStoreCommitted
            && zero.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92B7, 0x14, 0),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92BA, 0x18, 0)
            })
            && zero.FlagsUnaffected
            && !zero.IncomingEFlagsCaptured
            && zero.EFlagsAtStop is null
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeRightPairedStoresProjection
                    .BoundaryRawFileOffsetExclusive
            && !zero.BoundaryInstructionExecuted);

        CapacityFinalCalleeLargeRightPairedStoresPlan wrapped =
            Projection.Project(new(EntryEAX: 0xFFFFFFEC, EntryECX: 0xFFFFFFF0));
        check("Effective addresses wrap as unsigned DWORDs without changing stored values",
            wrapped.FirstStoreAddress == 0
            && wrapped.FirstStoreValue == 0xFFFFFFF0
            && wrapped.SecondStoreAddress == 8
            && wrapped.SecondStoreValue == 0xFFFFFFEC
            && wrapped.Writes.Length == 2
            && wrapped.Writes[0].Address == 0
            && wrapped.Writes[1].Address == 8
            && wrapped.Writes[0].Value == 0xFFFFFFF0
            && wrapped.Writes[1].Value == 0xFFFFFFEC
            && wrapped.ExactCleanCompletion
            && wrapped.StopAtRawFileOffsetExclusive == Projection.BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeLargeRightPairedStoresPlan alias =
            Projection.Project(new(EntryEAX: 0x100, EntryECX: 0xFC, IncomingEFlags: 0xA97));
        check("Aliased destinations retain distinct values in instruction order and preserve flags",
            alias.FirstStoreAddress == 0x114
            && alias.SecondStoreAddress == 0x114
            && alias.FirstStoreValue == 0xFC
            && alias.SecondStoreValue == 0x100
            && alias.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92B7, 0x114, 0xFC),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92BA, 0x114, 0x100)
            })
            && alias.Writes[0].Address == alias.Writes[1].Address
            && alias.Writes[0].Value != alias.Writes[1].Value
            && alias.IncomingEFlagsCaptured
            && alias.IncomingEFlags == 0xA97
            && alias.EFlagsAtStop == 0xA97
            && alias.FlagsUnaffected
            && alias.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightPairedStoresPlan distinct =
            Projection.Project(new(EntryEAX: 0x100, EntryECX: 0x200));
        check("Distinct destinations and values are independently derived",
            distinct.FirstStoreAddress == 0x114
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
            (0x012C92B7, "894814"),
            (0x012C92BA, "894118")
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightPairedStoresInstructionEvidence instruction
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

        return Projection.EntryRawFileOffset == 0x012C92B7
            && Projection.EntryVirtualAddress == 0x016C9EB7
            && Projection.RawFileOffsetToVirtualAddress(Projection.EntryRawFileOffset)
                == Projection.EntryVirtualAddress
            && Projection.FirstStoreRawFileOffset == 0x012C92B7
            && Projection.SecondStoreRawFileOffset == 0x012C92BA
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C92BD
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
