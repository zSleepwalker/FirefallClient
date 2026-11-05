using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeFrameSecondFieldTestChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The fresh-load window, raw/VA mapping, and zero branch evidence match",
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryRawFileOffset
                == 0x012C91E3
            && CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryVirtualAddress
                == 0x016C9DE3
            && CapacityFinalCalleeLargeFrameSecondFieldTestProjection.ZeroDestinationRawFileOffset
                == 0x012C9226
            && CapacityFinalCalleeLargeFrameSecondFieldTestProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91F0
            && CapacityFinalCalleeLargeFrameSecondFieldTestProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeFrameSecondFieldTestProjection.CapturedWindowBytes)
                == "8B42108D4A10894DFC85C07436"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan allMissing =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: null,
                EntryEBP: null,
                FreshEAX: null));
        check("All missing captures stop at the first load without a read value or later instructions",
            allMissing.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingEntryEDXForFreshRead
            && allMissing.FreshEAXReadReached
            && !allMissing.EntryEDXCaptured
            && allMissing.Reads.Length == 1
            && allMissing.Reads[0] is
            {
                RawFileOffset: 0x012C91E3,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && !allMissing.LEAECXReached
            && !allMissing.StoreReached
            && !allMissing.TestEAXReached
            && !allMissing.ZeroBranchReached
            && allMissing.ReachedInstructions.SequenceEqual(new[] { 0x012C91E3u })
            && !allMissing.UnreachedEntryEBPCaptureProvided
            && !allMissing.UnreachedFreshEAXCaptureProvided
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan missingEDXWithLaterCaptures =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: null,
                EntryEBP: 0x400,
                FreshEAX: 0));
        check("Missing first-use EDX stops at the load and marks later captures unreached",
            missingEDXWithLaterCaptures.UnreachedEntryEBPCaptureProvided
            && missingEDXWithLaterCaptures.UnreachedFreshEAXCaptureProvided
            && !missingEDXWithLaterCaptures.LEAECXReached
            && !missingEDXWithLaterCaptures.StoreReached
            && !missingEDXWithLaterCaptures.ExactCleanExitAdmitted
            && missingEDXWithLaterCaptures.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan missingFresh =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshEAX: null));
        check("A missing fresh EAX stops at its load and cannot use the supplied later EBP",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingFreshEAX
            && missingFresh.EntryEDXCaptured
            && missingFresh.FreshEDXPlus10Address == 0x110
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x110,
                ValueCaptured: false,
                Value: null
            }
            && missingFresh.UnreachedEntryEBPCaptureProvided
            && !missingFresh.FreshEAXCaptured
            && !missingFresh.LEAECXReached
            && !missingFresh.StoreReached
            && !missingFresh.TestEAXReached
            && !missingFresh.ZeroBranchReached
            && missingFresh.ReachedInstructions.SequenceEqual(new[] { 0x012C91E3u })
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan missingEBP =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: 0x300,
                EntryEBP: null,
                FreshEAX: 0x1234));
        check("Missing late EBP stops at the store after the independent load and address-only LEA",
            missingEBP.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingEntryEBPForStore
            && missingEBP.Reads[0] is
            {
                Address: 0x310,
                ValueCaptured: true,
                Value: 0x1234
            }
            && missingEBP.LEAECXReached
            && missingEBP.LEAECXValue == 0x310
            && missingEBP.StoreReached
            && !missingEBP.EntryEBPCaptured
            && missingEBP.Writes.Length == 1
            && missingEBP.Writes[0] is
            {
                RawFileOffset: 0x012C91E9,
                Address: null,
                Value: 0x310,
                Completed: false
            }
            && !missingEBP.TestEAXReached
            && !missingEBP.ZeroBranchReached
            && missingEBP.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91E3u,
                0x012C91E6u,
                0x012C91E9u
            })
            && !missingEBP.ExactCleanExitAdmitted
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C91E9);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan zero =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                EntryEBP: 0x2,
                FreshEAX: 0));
        check("Captured zero preserves independent load and LEA values, wraps addresses, and takes JZ",
            zero.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXZeroToTarget
            && zero.FreshEDXPlus10Address == 0
            && zero.Reads.Length == 1
            && zero.Reads[0] is
            {
                Address: 0,
                ValueCaptured: true,
                Value: 0
            }
            && zero.LEAECXValue == 0
            && zero.StoreAddress == 0xFFFFFFFE
            && zero.StoreValue == 0
            && zero.Writes.Length == 1
            && zero.Writes[0] is
            {
                Address: 0xFFFFFFFE,
                Value: 0,
                Completed: true
            }
            && zero.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && zero.ZeroBranchReached
            && zero.ZeroBranchTaken == true
            && zero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9226
            && zero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91E3u,
                0x012C91E6u,
                0x012C91E9u,
                0x012C91ECu,
                0x012C91EEu
            })
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan nonzero =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                EntryEBP: 0x10,
                FreshEAX: 0x80000001));
        check("Captured nonzero falls through to 91F0 with TEST flags from the fresh EAX value",
            nonzero.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXNonzeroToFallthrough
            && nonzero.FreshEDXPlus10Address == 0
            && nonzero.LEAECXValue == 0
            && nonzero.StoreAddress == 0xC
            && nonzero.StoreValue == 0
            && nonzero.Reads[0].Value == 0x80000001
            && nonzero.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && nonzero.ZeroBranchTaken == false
            && nonzero.ZeroBranchSelectedDestinationRawFileOffset == 0x012C91F0
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan wrappedAlias =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: 0xFFFFFFF4,
                EntryEBP: 8,
                FreshEAX: 0xDEADBEEF));
        check("Wrapped read and store addresses may alias while fresh field and LEA values remain independent",
            wrappedAlias.FreshEDXPlus10Address == 4
            && wrappedAlias.Reads[0].Address == 4
            && wrappedAlias.Reads[0].Value == 0xDEADBEEF
            && wrappedAlias.LEAECXValue == 4
            && wrappedAlias.StoreAddress == 4
            && wrappedAlias.StoreValue == 4
            && wrappedAlias.StoreAddress == wrappedAlias.Reads[0].Address
            && wrappedAlias.StoreValue != wrappedAlias.FreshEAX
            && wrappedAlias.Writes[0] is
            {
                Address: 4,
                Value: 4,
                Completed: true
            }
            && wrappedAlias.ZeroBranchTaken == false
            && wrappedAlias.ZeroBranchSelectedDestinationRawFileOffset == 0x012C91F0);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C91E3, "8B 42 10", null, null),
            (0x012C91E6, "8D 4A 10", null, null),
            (0x012C91E9, "89 4D FC", null, null),
            (0x012C91EC, "85 C0", null, null),
            (0x012C91EE, "74 36", 0x012C9226, 0x016C9E26)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeFrameSecondFieldTestInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeFrameSecondFieldTestProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeFrameSecondFieldTestProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeFrameSecondFieldTestProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeFrameSecondFieldTestProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Instructions
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
