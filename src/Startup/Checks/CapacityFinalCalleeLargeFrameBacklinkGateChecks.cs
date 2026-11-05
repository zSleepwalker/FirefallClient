using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeFrameBacklinkGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window and its VA mapping match",
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryRawFileOffset
                == 0x012C91B9
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryVirtualAddress
                == 0x016C9DB9
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91C5
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.NonzeroDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.ZeroDestinationRawFileOffset
                == 0x012C91C5
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeFrameBacklinkGateProjection.CapturedWindowBytes)
                == "8B5DEC39530C0F85B1FCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeFrameBacklinkGatePlan missingEbp =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: null,
                frameMinus14: 0x100,
                entryEdx: 0x200,
                freshBacklink: 0x300));
        check("Missing EBP stops at the frame read and leaves all later captures unreached",
            missingEbp.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEBP
            && missingEbp.FrameMinus14ReadReached
            && !missingEbp.EntryEBPCaptured
            && missingEbp.Reads.Length == 1
            && missingEbp.Reads[0] is
            {
                RawFileOffset: 0x012C91B9,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null
            }
            && missingEbp.UnreachedFreshFrameMinus14CaptureProvided
            && missingEbp.UnreachedEntryEDXCaptureProvided
            && missingEbp.UnreachedFreshBacklinkCaptureProvided
            && !missingEbp.BacklinkReadReached
            && !missingEbp.CompareReached
            && !missingEbp.JnzReached
            && !missingEbp.ExactCleanExitAdmitted
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C91B9);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan missingFrameValue =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x20,
                frameMinus14: null,
                entryEdx: 0x200,
                freshBacklink: 0x300));
        check("Missing frame-minus-14 value stops before EDX and backlink consumption",
            missingFrameValue.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshFrameMinus14
            && missingFrameValue.FrameMinus14Address == 0x0C
            && missingFrameValue.Reads.Length == 1
            && missingFrameValue.Reads[0] is
            {
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingFrameValue.UnreachedEntryEDXCaptureProvided
            && missingFrameValue.UnreachedFreshBacklinkCaptureProvided
            && !missingFrameValue.EntryEDXCaptured
            && !missingFrameValue.BacklinkReadReached
            && !missingFrameValue.CompareReached
            && !missingFrameValue.JnzReached
            && missingFrameValue.StopAtRawFileOffsetExclusive == 0x012C91B9);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan missingEdx =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x20,
                frameMinus14: 0x100,
                entryEdx: null,
                freshBacklink: 0x300));
        check("Missing EDX stops at CMP while retaining its independent memory operand",
            missingEdx.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEDX
            && missingEdx.FrameMinus14Captured
            && missingEdx.FrameMinus14 == 0x100
            && !missingEdx.EntryEDXCaptured
            && missingEdx.BacklinkReadReached
            && missingEdx.BacklinkAddress == 0x10C
            && missingEdx.BacklinkCaptured
            && missingEdx.BacklinkValue == 0x300
            && !missingEdx.UnreachedFreshBacklinkCaptureProvided
            && missingEdx.Reads.Length == 2
            && !missingEdx.CompareReached
            && !missingEdx.JnzReached
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan missingBacklink =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x20,
                frameMinus14: 0x100,
                entryEdx: 0x200,
                freshBacklink: null));
        check("Missing backlink retains the CMP memory address but prevents flags and JNZ",
            missingBacklink.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshBacklink
            && missingBacklink.EntryEDXCaptured
            && missingBacklink.BacklinkReadReached
            && missingBacklink.BacklinkAddress == 0x10C
            && missingBacklink.Reads.Length == 2
            && missingBacklink.Reads[1] is
            {
                RawFileOffset: 0x012C91BC,
                Address: 0x10C,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingBacklink.UnreachedFreshBacklinkCaptureProvided
            && !missingBacklink.CompareReached
            && !missingBacklink.JnzReached
            && missingBacklink.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan equalZero =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x20,
                frameMinus14: 0,
                entryEdx: 0,
                freshBacklink: 0));
        check("Zero backlink equality calculates all flags and falls through 91C5",
            equalZero.Reads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C91B9, 0x012C91BC })
            && equalZero.Events.Select(item => item.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read,
                    CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read,
                    CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Compare,
                    CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Branch
                })
            && equalZero.Events.Select(item => item.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C91B9, 0x012C91BC, 0x012C91BC, 0x012C91BF
                })
            && equalZero.FrameMinus14Address == 0x0C
            && equalZero.BacklinkAddress == 0x0C
            && equalZero.CompareLeft == 0
            && equalZero.CompareRight == 0
            && equalZero.CompareResult == 0
            && equalZero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.JnzReached
            && equalZero.JnzTaken == false
            && equalZero.JnzSelectedDestinationRawFileOffset == 0x012C91C5
            && equalZero.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough
            && equalZero.ExactCleanExitAdmitted
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan unequal =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x40,
                frameMinus14: 0x100,
                entryEdx: 1,
                freshBacklink: 0x80000000));
        check("Unequal backlink calculates all flags and takes JNZ to 8E76",
            unequal.CompareLeft == 0x80000000
            && unequal.CompareRight == 1
            && unequal.CompareResult == 0x7FFFFFFF
            && unequal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && unequal.JnzReached
            && unequal.JnzTaken == true
            && unequal.JnzSelectedDestinationRawFileOffset == 0x012C8E76
            && unequal.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.UnequalToTarget
            && unequal.ExactCleanExitAdmitted
            && unequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan wrappedZero =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x10,
                frameMinus14: 0xFFFFFFF4,
                entryEdx: 0,
                freshBacklink: 0));
        check("Frame and backlink addresses wrap as DWORDs while zero remains captured",
            wrappedZero.FrameMinus14Address == 0xFFFFFFFC
            && wrappedZero.BacklinkAddress == 0
            && wrappedZero.FrameMinus14Captured
            && wrappedZero.FrameMinus14 == 0xFFFFFFF4
            && wrappedZero.BacklinkCaptured
            && wrappedZero.BacklinkValue == 0
            && wrappedZero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection
                .DeriveFrameMinus14Address(0x10) == 0xFFFFFFFC
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection
                .DeriveBacklinkAddress(0xFFFFFFF4) == 0
            && wrappedZero.JnzTaken == false);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan aliasIndependent =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(Capture(
                entryEbp: 0x20,
                frameMinus14: 0,
                entryEdx: 0,
                freshBacklink: 0));
        check("Aliased read addresses retain independent captured zero values",
            aliasIndependent.FrameMinus14Address == aliasIndependent.BacklinkAddress
            && aliasIndependent.Reads[0].Address == aliasIndependent.Reads[1].Address
            && aliasIndependent.Reads[0].ValueCaptured
            && aliasIndependent.Reads[1].ValueCaptured
            && aliasIndependent.Reads[0].Value == 0
            && aliasIndependent.Reads[1].Value == 0
            && aliasIndependent.CompareLeft == 0
            && aliasIndependent.CompareRight == 0
            && aliasIndependent.JnzTaken == false);
    }

    private static CapacityFinalCalleeLargeFrameBacklinkGateCapture Capture(
        uint? entryEbp = 0x1000,
        uint? frameMinus14 = 0x2000,
        uint? entryEdx = 0x3000,
        uint? freshBacklink = 0x4000) =>
        new(entryEbp, frameMinus14, entryEdx, freshBacklink);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C91B9, "8B 5D EC", null, null),
            (0x012C91BC, "39 53 0C", null, null),
            (0x012C91BF, "0F 85 B1 FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeFrameBacklinkGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameBacklinkGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeFrameBacklinkGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeFrameBacklinkGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeFrameBacklinkGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeFrameBacklinkGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeFrameBacklinkGateProjection.Instructions
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
