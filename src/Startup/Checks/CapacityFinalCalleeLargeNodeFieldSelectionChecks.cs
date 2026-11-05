using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeNodeFieldSelectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw bytes, instruction boundaries, branches, and VA mapping match",
            CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryRawFileOffset
                == 0x012C91F0
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryVirtualAddress
                == 0x016C9DF0
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9205
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.Plus14NonzeroDestinationRawFileOffset
                == 0x012C9205
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.Plus10ZeroDestinationRawFileOffset
                == 0x012C920B
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeNodeFieldSelectionProjection.CapturedWindowBytes)
                == "8B55FC837814008D48147509837810008D48107406"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeNodeFieldSelectionPlan allMissing =
            Project(null, null, null, null, null);
        check("All missing captures stop at the initial frame read",
            allMissing.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEBPForFrameRead
            && allMissing.FrameReadReached
            && !allMissing.EntryEBPCaptured
            && allMissing.Reads is
            [
                {
                    RawFileOffset: 0x012C91F0,
                    Address: null,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && allMissing.ReachedInstructions.SequenceEqual(new[] { 0x012C91F0u })
            && !allMissing.Plus14CompareReached
            && !allMissing.Plus10CompareReached
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan missingEBP =
            Project(null, 0x1234, 0x200, 0, 0);
        check("Missing EBP rejects later captures before the frame read address is known",
            missingEBP.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEBPForFrameRead
            && missingEBP.UnreachedFrameMinus4EDXCaptureProvided
            && missingEBP.UnreachedEntryEAXCaptureProvided
            && missingEBP.UnreachedFreshEAXPlus14CaptureProvided
            && missingEBP.UnreachedFreshEAXPlus10CaptureProvided
            && missingEBP.Reads.Length == 1
            && missingEBP.Reads[0].Address is null
            && !missingEBP.Plus14CompareReached
            && !missingEBP.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan missingFrameValue =
            Project(0x20, null, 0x100, 0, 0);
        check("Missing frame-minus-four EDX stops before either fresh EAX read",
            missingFrameValue.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFrameMinus4EDX
            && missingFrameValue.EntryEBPCaptured
            && missingFrameValue.FrameReadAddress == 0x1C
            && !missingFrameValue.FrameMinus4EDXCaptured
            && missingFrameValue.UnreachedEntryEAXCaptureProvided
            && missingFrameValue.UnreachedFreshEAXPlus14CaptureProvided
            && missingFrameValue.UnreachedFreshEAXPlus10CaptureProvided
            && missingFrameValue.Reads[0] is
            {
                Address: 0x1C,
                ValueCaptured: false,
                Value: null
            }
            && missingFrameValue.ReachedInstructions.SequenceEqual(new[] { 0x012C91F0u })
            && !missingFrameValue.ExactCleanExitAdmitted
            && missingFrameValue.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan missingEAX =
            Project(0x20, 0xFEED, null, 1, 2);
        check("Missing entry EAX stops at the plus-14 compare after retaining loaded frame EDX",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEAXForPlus14Read
            && missingEAX.FrameMinus4EDXCaptured
            && missingEAX.FrameMinus4EDX == 0xFEED
            && !missingEAX.EntryEAXCaptured
            && missingEAX.Reads.Length == 2
            && missingEAX.Reads[1] is
            {
                RawFileOffset: 0x012C91F3,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingEAX.UnreachedFreshEAXPlus14CaptureProvided
            && missingEAX.UnreachedFreshEAXPlus10CaptureProvided
            && !missingEAX.Plus14LeaReached
            && !missingEAX.ExactCleanExitAdmitted
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C91F3);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan missingPlus14 =
            Project(0x20, 0xFEED, 0x100, null, 0);
        check("Missing plus-14 data stops before LEA and leaves plus-10 capture unused",
            missingPlus14.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFreshEAXPlus14
            && missingPlus14.FrameMinus4EDX == 0xFEED
            && missingPlus14.EntryEAXCaptured
            && missingPlus14.FreshEAXPlus14Address == 0x114
            && missingPlus14.Reads[1] is
            {
                Address: 0x114,
                ValueCaptured: false,
                Value: null
            }
            && missingPlus14.UnreachedFreshEAXPlus10CaptureProvided
            && !missingPlus14.Plus14LeaReached
            && !missingPlus14.Plus14BranchReached
            && !missingPlus14.ExactCleanExitAdmitted
            && missingPlus14.StopAtRawFileOffsetExclusive == 0x012C91F3);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan missingPlus10 =
            Project(0x20, 0xFEED, 0x100, 0, null);
        check("Missing plus-10 data stops at its compare and preserves the earlier zero CMP flags",
            missingPlus10.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFreshEAXPlus10
            && missingPlus10.FrameMinus4EDX == 0xFEED
            && missingPlus10.Plus14LeaReached
            && missingPlus10.Plus14LeaECXValue == 0x114
            && missingPlus10.Plus14BranchReached
            && missingPlus10.Plus14BranchTaken == false
            && missingPlus10.Plus10CompareReached
            && missingPlus10.FreshEAXPlus10Address == 0x110
            && missingPlus10.Reads.Length == 3
            && missingPlus10.Reads[2] is
            {
                RawFileOffset: 0x012C91FC,
                Address: 0x110,
                ValueCaptured: false,
                Value: null
            }
            && missingPlus10.Plus14CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingPlus10.FlagsAtStop == missingPlus10.Plus14CompareFlags
            && !missingPlus10.Plus10LeaReached
            && !missingPlus10.Plus10BranchReached
            && !missingPlus10.ExactCleanExitAdmitted
            && missingPlus10.StopAtRawFileOffsetExclusive == 0x012C91FC);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan firstNonzero =
            Project(0x118, 0xDEADBEEF, 0x100, 0x80000001, null);
        check("Nonzero plus-14 branches to 9205 without reading plus-10 and preserves CMP flags",
            firstNonzero.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget
            && firstNonzero.FrameReadAddress == 0x114
            && firstNonzero.FrameMinus4EDX == 0xDEADBEEF
            && firstNonzero.FreshEAXPlus14Address == 0x114
            && firstNonzero.FreshEAXPlus14 == 0x80000001
            && firstNonzero.Plus14LeaECXValue == 0x114
            && firstNonzero.Reads.Length == 2
            && firstNonzero.Reads[0].Address == firstNonzero.Reads[1].Address
            && firstNonzero.Reads[0].Value == 0xDEADBEEF
            && firstNonzero.Reads[1].Value == 0x80000001
            && firstNonzero.Plus14CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && firstNonzero.FlagsAtStop == firstNonzero.Plus14CompareFlags
            && firstNonzero.Plus14BranchReached
            && firstNonzero.Plus14BranchTaken == true
            && firstNonzero.Plus14BranchSelectedDestinationRawFileOffset == 0x012C9205
            && !firstNonzero.Plus10CompareReached
            && !firstNonzero.Plus10LeaReached
            && !firstNonzero.Plus10BranchReached
            && !firstNonzero.UnreachedFreshEAXPlus10CaptureProvided
            && firstNonzero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91F0u, 0x012C91F3u, 0x012C91F7u, 0x012C91FAu
            })
            && firstNonzero.ExactCleanExitAdmitted
            && firstNonzero.StopAtRawFileOffsetExclusive == 0x012C9205);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan nonzeroWithUnusedPlus10 =
            Project(0x100, 0x2222, 1, 1, 0);
        check("A supplied plus-10 capture is unused and blocks clean admission after plus-14 selects 9205",
            nonzeroWithUnusedPlus10.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget
            && nonzeroWithUnusedPlus10.UnreachedFreshEAXPlus10CaptureProvided
            && !nonzeroWithUnusedPlus10.Plus10CompareReached
            && !nonzeroWithUnusedPlus10.ExactCleanExitAdmitted
            && nonzeroWithUnusedPlus10.StopAtRawFileOffsetExclusive == 0x012C9205);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan secondZero =
            Project(2, 0xCAFEBABE, 0xFFFFFFF4, 0, 0);
        check("Captured zero is distinct from missing and takes the second zero branch to 920B",
            secondZero.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldZeroToTarget
            && secondZero.FrameReadAddress == 0xFFFFFFFE
            && secondZero.FrameMinus4EDX == 0xCAFEBABE
            && secondZero.FreshEAXPlus14Address == 8
            && secondZero.FreshEAXPlus10Address == 4
            && secondZero.FreshEAXPlus14Captured
            && secondZero.FreshEAXPlus14 == 0
            && secondZero.FreshEAXPlus10Captured
            && secondZero.FreshEAXPlus10 == 0
            && secondZero.Plus14CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && secondZero.Plus10CompareFlags == secondZero.Plus14CompareFlags
            && secondZero.Plus14LeaECXValue == 8
            && secondZero.Plus10LeaECXValue == 4
            && secondZero.Reads.Select(read => read.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C91F0, 0x012C91F3, 0x012C91FC
            })
            && secondZero.Plus10BranchReached
            && secondZero.Plus10BranchTaken == true
            && secondZero.Plus10BranchSelectedDestinationRawFileOffset == 0x012C920B
            && secondZero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91F0u, 0x012C91F3u, 0x012C91F7u, 0x012C91FAu,
                0x012C91FCu, 0x012C9200u, 0x012C9203u
            })
            && secondZero.ExactCleanExitAdmitted
            && secondZero.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeNodeFieldSelectionPlan secondNonzero =
            Project(0x200, 0x12345678, 0xFFFFFFF4, 0, 0x80000001);
        check("Nonzero plus-10 falls through to 9205 with all CMP flags derived from the fresh value",
            secondNonzero.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldNonzeroToFallthrough
            && secondNonzero.FrameMinus4EDX == 0x12345678
            && secondNonzero.FreshEAXPlus14Address == 8
            && secondNonzero.FreshEAXPlus10Address == 4
            && secondNonzero.Plus10LeaECXValue == 4
            && secondNonzero.Plus10CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && secondNonzero.FlagsAtStop == secondNonzero.Plus10CompareFlags
            && secondNonzero.Plus10BranchTaken == false
            && secondNonzero.Plus10BranchSelectedDestinationRawFileOffset == 0x012C9205
            && secondNonzero.ExactCleanExitAdmitted
            && secondNonzero.StopAtRawFileOffsetExclusive == 0x012C9205);
    }

    private static CapacityFinalCalleeLargeNodeFieldSelectionPlan Project(
        uint? entryEBP,
        uint? frameMinus4EDX,
        uint? entryEAX,
        uint? freshEAXPlus14,
        uint? freshEAXPlus10) =>
        CapacityFinalCalleeLargeNodeFieldSelectionProjection.Project(new(
            entryEBP,
            frameMinus4EDX,
            entryEAX,
            freshEAXPlus14,
            freshEAXPlus10));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C91F0, "8B 55 FC", null, null),
            (0x012C91F3, "83 78 14 00", null, null),
            (0x012C91F7, "8D 48 14", null, null),
            (0x012C91FA, "75 09", 0x012C9205, 0x016C9E05),
            (0x012C91FC, "83 78 10 00", null, null),
            (0x012C9200, "8D 48 10", null, null),
            (0x012C9203, "74 06", 0x012C920B, 0x016C9E0B)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeNodeFieldSelectionProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeNodeFieldSelectionInstructionEvidence instruction
            in CapacityFinalCalleeLargeNodeFieldSelectionProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeNodeFieldSelectionProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeNodeFieldSelectionProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeNodeFieldSelectionProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeNodeFieldSelectionProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeNodeFieldSelectionProjection.Instructions
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
