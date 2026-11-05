using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeFrameRootGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window and its VA mapping match",
            CapacityFinalCalleeLargeFrameRootGateProjection.EntryRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeLargeFrameRootGateProjection.EntryVirtualAddress
                == 0x016C9D9B
            && CapacityFinalCalleeLargeFrameRootGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91A8
            && CapacityFinalCalleeLargeFrameRootGateProjection.EqualDestinationRawFileOffset
                == 0x012C91D6
            && CapacityFinalCalleeLargeFrameRootGateProjection.UnequalDestinationRawFileOffset
                == 0x012C91A8
            && CapacityFinalCalleeLargeFrameRootGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeFrameRootGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeFrameRootGateProjection.EntryVirtualAddress
            && Normalize(
                CapacityFinalCalleeLargeFrameRootGateProjection.CapturedWindowBytes)
                == "8B420C8B4A18894DF03BC2742E"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeFrameRootGatePlan allMissing =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(new(
                EntryEDX: null,
                EntryEBP: null,
                FreshEdxPlusC: null,
                FreshEdxPlus18: null));
        check("Missing EDX stops at the first unaddressed read without reaching later operations",
            allMissing.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX
            && !allMissing.EntryEDXCaptured
            && !allMissing.EntryEBPCaptured
            && allMissing.Reads.Length == 1
            && allMissing.Reads[0] is
            {
                RawFileOffset: 0x012C919B,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && !allMissing.StoreReached
            && allMissing.Writes.IsEmpty
            && allMissing.Events.Select(item => item.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Read
                })
            && !allMissing.CompareReached
            && !allMissing.BranchReached
            && allMissing.UnreachedEntryEBPCaptureProvided == false
            && !allMissing.UnreachedFreshEdxPlusCCaptureProvided
            && !allMissing.UnreachedFreshEdxPlus18CaptureProvided
            && !allMissing.ExactCleanAdmission
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeFrameRootGatePlan missingEdxWithLateCaptures =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: null,
                entryEbp: 0x20,
                freshEdxPlusC: 0,
                freshEdxPlus18: 0x55));
        check("Captures after missing EDX are retained only as unreached inputs",
            missingEdxWithLateCaptures.Reads.Length == 1
            && missingEdxWithLateCaptures.Reads[0].Address is null
            && !missingEdxWithLateCaptures.StoreReached
            && missingEdxWithLateCaptures.Writes.IsEmpty
            && missingEdxWithLateCaptures.UnreachedEntryEBPCaptureProvided
            && missingEdxWithLateCaptures.UnreachedFreshEdxPlusCCaptureProvided
            && missingEdxWithLateCaptures.UnreachedFreshEdxPlus18CaptureProvided
            && !missingEdxWithLateCaptures.FreshEdxPlusCCaptured
            && !missingEdxWithLateCaptures.FreshEdxPlus18Captured);

        CapacityFinalCalleeLargeFrameRootGatePlan equal =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0,
                entryEbp: 0x10,
                freshEdxPlusC: 0,
                freshEdxPlus18: 0));
        check("Both fresh reads precede the EBP store and equal CMP selects 91D6",
            equal.Reads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C919B, 0x012C919E })
            && equal.Events.Select(item => item.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Read,
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Read,
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Write,
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Compare,
                    CapacityFinalCalleeLargeFrameRootGateEventKind.Branch
                })
            && equal.Events.Select(item => item.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C919B, 0x012C919E, 0x012C91A1, 0x012C91A4, 0x012C91A6
                })
            && equal.StoreAddress == 0
            && equal.StoreValueCaptured
            && equal.StoreValue == 0
            && equal.Writes[0].Committed
            && equal.CompareLeft == 0
            && equal.CompareRight == 0
            && equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.BranchTaken == true
            && equal.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && equal.Outcome == CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget
            && equal.ExactCleanAdmission
            && equal.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeFrameRootGatePlan unequal =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 1,
                entryEbp: 0x20,
                freshEdxPlusC: 0,
                freshEdxPlus18: 0xAABBCCDD));
        check("Unequal CMP selects the fallthrough 91A8 clean route",
            unequal.CompareResult == 0xFFFFFFFF
            && unequal.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unequal.BranchTaken == false
            && unequal.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && unequal.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough
            && unequal.StoreAddress == 0x10
            && unequal.StoreValue == 0xAABBCCDD
            && unequal.ExactCleanAdmission
            && unequal.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeFrameRootGatePlan missingFirst =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: 0x200,
                freshEdxPlusC: null,
                freshEdxPlus18: 0x12345678));
        check("A missing first fresh value prevents the second load and marks later captures unreached",
            missingFirst.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlusC
            && missingFirst.Reads.Length == 1
            && missingFirst.Reads[0] is
            {
                Address: 0x10C,
                ValueCaptured: false,
                Value: null
            }
            && !missingFirst.StoreReached
            && missingFirst.Writes.IsEmpty
            && missingFirst.Events.Length == 1
            && !missingFirst.CompareReached
            && !missingFirst.BranchReached
            && missingFirst.UnreachedEntryEBPCaptureProvided
            && missingFirst.UnreachedFreshEdxPlus18CaptureProvided
            && !missingFirst.ExactCleanAdmission
            && missingFirst.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeFrameRootGatePlan missingSecond =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: 0x200,
                freshEdxPlusC: 0x100,
                freshEdxPlus18: null));
        check("A missing second fresh value retains the first read but prevents store and compare",
            missingSecond.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlus18
            && missingSecond.Reads.Length == 2
            && missingSecond.Reads[0].ValueCaptured
            && missingSecond.Reads[0].Value == 0x100
            && missingSecond.Reads[1].Address == 0x118
            && !missingSecond.Reads[1].ValueCaptured
            && !missingSecond.StoreReached
            && missingSecond.Writes.IsEmpty
            && !missingSecond.CompareReached
            && !missingSecond.BranchReached
            && missingSecond.UnreachedEntryEBPCaptureProvided
            && !missingSecond.UnreachedFreshEdxPlus18CaptureProvided
            && !missingSecond.ExactCleanAdmission
            && missingSecond.Events.Length == 2
            && missingSecond.StopAtRawFileOffsetExclusive == 0x012C919E);

        CapacityFinalCalleeLargeFrameRootGatePlan missingEbp =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: null,
                freshEdxPlusC: 0x101,
                freshEdxPlus18: 0x22222222));
        check("Missing EBP stops at the store after retaining both fresh reads",
            missingEbp.Reads.Length == 2
            && missingEbp.Reads.All(read => read.Address.HasValue && read.ValueCaptured)
            && !missingEbp.StoreReached
            && missingEbp.StoreAddress is null
            && missingEbp.FreshEdxPlus18 == 0x22222222
            && missingEbp.Writes.IsEmpty
            && !missingEbp.CompareReached
            && !missingEbp.BranchReached
            && missingEbp.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEBPForStore
            && missingEbp.UnreachedEntryEBPCaptureProvided == false
            && !missingEbp.ExactCleanAdmission
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C91A1);

        CapacityFinalCalleeLargeFrameRootGatePlan aliasedStore =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: 0x128,
                freshEdxPlusC: 0x100,
                freshEdxPlus18: 0xDEADBEEF));
        check("The delayed store may alias the second load without replacing its fresh value",
            aliasedStore.FreshEdxPlus18Address == 0x118
            && aliasedStore.StoreAddress == 0x118
            && aliasedStore.Reads[1].Value == 0xDEADBEEF
            && aliasedStore.Writes[0].Value == 0xDEADBEEF
            && aliasedStore.StoreValue == 0xDEADBEEF
            && aliasedStore.CompareLeft == 0x100
            && aliasedStore.CompareRight == 0x100
            && aliasedStore.CompareResult == 0
            && aliasedStore.BranchTaken == true);

        CapacityFinalCalleeLargeFrameRootGatePlan wrappedZero =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0xFFFFFFF4,
                entryEbp: 0x10,
                freshEdxPlusC: 0,
                freshEdxPlus18: 0));
        check("DWORD source and destination addresses wrap and captured zeros stay present",
            wrappedZero.FreshEdxPlusCAddress == 0
            && wrappedZero.FreshEdxPlus18Address == 0xC
            && wrappedZero.StoreAddress == 0
            && wrappedZero.FreshEdxPlusCCaptured
            && wrappedZero.FreshEdxPlusC == 0
            && wrappedZero.FreshEdxPlus18Captured
            && wrappedZero.FreshEdxPlus18 == 0
            && CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEdxPlusCAddress(
                0xFFFFFFF4) == 0
            && CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEdxPlus18Address(
                0xFFFFFFF4) == 0xC
            && CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEbpMinus10Address(
                0x10) == 0
            && wrappedZero.CompareResult == 0xC);

        CapacityFinalCalleeLargeFrameRootGatePlan signedOverflow =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 1,
                entryEbp: 0x40,
                freshEdxPlusC: 0x80000000,
                freshEdxPlus18: 0));
        check("CMP calculates all six flags for signed overflow",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            });

        CapacityFinalCalleeLargeFrameRootGatePlan lowByteParity =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(Capture(
                entryEdx: 0,
                entryEbp: 0x40,
                freshEdxPlusC: 0x01000001,
                freshEdxPlus18: 0));
        check("CMP parity is derived only from the low byte of the DWORD result",
            lowByteParity.CompareResult == 0x01000001
            && lowByteParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            });
    }

    private static CapacityFinalCalleeLargeFrameRootGateCapture Capture(
        uint? entryEdx = 0x1000,
        uint? entryEbp = 0x2000,
        uint? freshEdxPlusC = 0x3000,
        uint? freshEdxPlus18 = 0x4000) =>
        new(entryEdx, entryEbp, freshEdxPlusC, freshEdxPlus18);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw)[] {
            (0x012C919B, "8B 42 0C", null),
            (0x012C919E, "8B 4A 18", null),
            (0x012C91A1, "89 4D F0", null),
            (0x012C91A4, "3B C2", null),
            (0x012C91A6, "74 2E", 0x012C91D6)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeFrameRootGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeFrameRootGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameRootGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeFrameRootGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeFrameRootGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeFrameRootGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeFrameRootGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeFrameRootGateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
