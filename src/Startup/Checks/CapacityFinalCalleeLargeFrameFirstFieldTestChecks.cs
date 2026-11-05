using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeFrameFirstFieldTestChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The fresh-load window, raw/VA mapping, and nonzero branch evidence match",
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryRawFileOffset
                == 0x012C91D6
            && CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryVirtualAddress
                == 0x016C9DD6
            && CapacityFinalCalleeLargeFrameFirstFieldTestProjection.NonzeroDestinationRawFileOffset
                == 0x012C91F0
            && CapacityFinalCalleeLargeFrameFirstFieldTestProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91E3
            && CapacityFinalCalleeLargeFrameFirstFieldTestProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeFrameFirstFieldTestProjection.CapturedWindowBytes)
                == "8B42148D4A14894DFC85C0750D"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan allMissing =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: null,
                EntryEBP: null,
                FreshEAX: null));
        check("All missing captures stop at the first load without a read value or later instructions",
            allMissing.Outcome
                == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEDXForFreshRead
            && allMissing.FreshEAXReadReached
            && !allMissing.EntryEDXCaptured
            && allMissing.Reads.Length == 1
            && allMissing.Reads[0] is
            {
                RawFileOffset: 0x012C91D6,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && !allMissing.LEAECXReached
            && !allMissing.StoreReached
            && !allMissing.TestEAXReached
            && !allMissing.NonzeroBranchReached
            && allMissing.ReachedInstructions.SequenceEqual(new[] { 0x012C91D6u })
            && !allMissing.UnreachedEntryEBPCaptureProvided
            && !allMissing.UnreachedFreshEAXCaptureProvided
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan missingEDXWithLaterCaptures =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: null,
                EntryEBP: 0x400,
                FreshEAX: 0));
        check("Missing first-use EDX stops at the load and marks the field and EBP captures unreached",
            missingEDXWithLaterCaptures.UnreachedEntryEBPCaptureProvided
            && missingEDXWithLaterCaptures.UnreachedFreshEAXCaptureProvided
            && !missingEDXWithLaterCaptures.LEAECXReached
            && !missingEDXWithLaterCaptures.StoreReached
            && !missingEDXWithLaterCaptures.ExactCleanExitAdmitted
            && missingEDXWithLaterCaptures.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan missingFresh =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshEAX: null));
        check("A missing fresh field stops at its load and cannot use the supplied later EBP",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingFreshEAX
            && missingFresh.EntryEDXCaptured
            && missingFresh.FreshEDXPlus14Address == 0x114
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x114,
                ValueCaptured: false,
                Value: null
            }
            && missingFresh.UnreachedEntryEBPCaptureProvided
            && !missingFresh.FreshEAXCaptured
            && !missingFresh.LEAECXReached
            && !missingFresh.StoreReached
            && !missingFresh.TestEAXReached
            && missingFresh.ReachedInstructions.SequenceEqual(new[] { 0x012C91D6u })
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan missingEBP =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: 0x300,
                EntryEBP: null,
                FreshEAX: 0x1234));
        check("Missing late EBP stops at the store after the independent load and address-only LEA",
            missingEBP.Outcome
                == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEBPForStore
            && missingEBP.Reads[0] is
            {
                Address: 0x314,
                ValueCaptured: true,
                Value: 0x1234
            }
            && missingEBP.LEAECXReached
            && missingEBP.LEAECXValue == 0x314
            && missingEBP.StoreReached
            && !missingEBP.EntryEBPCaptured
            && missingEBP.Writes.Length == 1
            && missingEBP.Writes[0] is
            {
                RawFileOffset: 0x012C91DC,
                Address: null,
                Value: 0x314,
                Completed: false
            }
            && !missingEBP.TestEAXReached
            && missingEBP.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu
            })
            && !missingEBP.ExactCleanExitAdmitted
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C91DC);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan zero =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: 0xFFFFFFEC,
                EntryEBP: 0x2,
                FreshEAX: 0));
        check("Captured zero preserves independent load and LEA values, wraps the store, and falls through cleanly",
            zero.Outcome
                == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXZeroToFallthrough
            && zero.FreshEDXPlus14Address == 0
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
            && zero.NonzeroBranchReached
            && zero.NonzeroBranchTaken == false
            && zero.NonzeroBranchSelectedDestinationRawFileOffset == 0x012C91E3
            && zero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu,
                0x012C91DFu,
                0x012C91E1u
            })
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan nonzero =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                EntryEBP: 0x10,
                FreshEAX: 0x80000001));
        check("Captured nonzero branches to 91F0 with TEST flags derived from the fresh EAX value",
            nonzero.Outcome
                == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXNonzeroToTarget
            && nonzero.FreshEDXPlus14Address == 4
            && nonzero.LEAECXValue == 4
            && nonzero.StoreAddress == 0xC
            && nonzero.StoreValue == 4
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
            && nonzero.NonzeroBranchTaken == true
            && nonzero.NonzeroBranchSelectedDestinationRawFileOffset == 0x012C91F0
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan independentFreshValue =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                EntryEDX: 0x200,
                EntryEBP: 0x800,
                FreshEAX: 0xDEADBEEF));
        check("The field load remains independent of the LEA-derived store value",
            independentFreshValue.FreshEDXPlus14Address == 0x214
            && independentFreshValue.Reads[0].Value == 0xDEADBEEF
            && independentFreshValue.LEAECXValue == 0x214
            && independentFreshValue.StoreValue == 0x214
            && independentFreshValue.StoreValue != independentFreshValue.FreshEAX
            && independentFreshValue.Writes[0] is
            {
                Address: 0x7FC,
                Value: 0x214,
                Completed: true
            });
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C91D6, "8B 42 14", null, null),
            (0x012C91D9, "8D 4A 14", null, null),
            (0x012C91DC, "89 4D FC", null, null),
            (0x012C91DF, "85 C0", null, null),
            (0x012C91E1, "75 0D", 0x012C91F0, 0x016C9DF0)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeFrameFirstFieldTestInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeFrameFirstFieldTestProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeFrameFirstFieldTestProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeFrameFirstFieldTestProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeFrameFirstFieldTestProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Instructions
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
