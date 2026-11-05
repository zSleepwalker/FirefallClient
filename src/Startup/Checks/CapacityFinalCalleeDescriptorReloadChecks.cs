using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescriptorReloadChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The descriptor-reload instruction window and raw/VA anchors match",
            CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset
                == 0x012C8E9A
            && CapacityFinalCalleeDescriptorReloadProjection.EntryVirtualAddress
                == 0x016C9A9A
            && CapacityFinalCalleeDescriptorReloadProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8EB8
            && CapacityFinalCalleeDescriptorReloadProjection.BelowDestinationRawFileOffset
                == 0x012C8EB8
            && CapacityFinalCalleeDescriptorReloadProjection.EqualityDestinationRawFileOffset
                == 0x012C8EBE
            && CapacityFinalCalleeDescriptorReloadProjection.BelowDestinationVirtualAddress
                == 0x016C9AB8
            && CapacityFinalCalleeDescriptorReloadProjection.EqualityDestinationVirtualAddress
                == 0x016C9ABE
            && CapacityFinalCalleeDescriptorReloadProjection.EncodedDescriptorBasePeVirtualAddress
                == 0x01EDADF0
            && CapacityFinalCalleeDescriptorReloadProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset)
                == CapacityFinalCalleeDescriptorReloadProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeDescriptorReloadProjection.CapturedWindowBytes)
                == "8B4DF08D04CDF0ADED018945F08B45FC3B45F0740F3BC3720539700C7406"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeDescriptorReloadPlan invalidEntry =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                captureValidatedEntry: false,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("An unvalidated entry leaves all reads, computations, writes, and branches unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.EntryNotValidated
            && !invalidEntry.InitialFrameMinus10ReadReached
            && !invalidEntry.DescriptorAddressComputationReached
            && !invalidEntry.DescriptorAddressWriteReached
            && !invalidEntry.FreshFrameMinus4ReadReached
            && !invalidEntry.FreshFrameMinus10ReadReached
            && !invalidEntry.FirstCompareReached
            && !invalidEntry.EntryEbxCompareReached
            && !invalidEntry.FreshEaxPlusCReadReached
            && !invalidEntry.FinalCompareReached
            && !invalidEntry.FirstBranchReached
            && !invalidEntry.SecondBranchReached
            && !invalidEntry.FinalBranchReached
            && invalidEntry.UnreachedInitialFrameMinus10CaptureProvided
            && invalidEntry.UnreachedFreshFrameMinus4CaptureProvided
            && invalidEntry.UnreachedFreshFrameMinus10CaptureProvided
            && invalidEntry.UnreachedFreshEaxPlusCCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeDescriptorReloadPlan missingInitialRead =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                entryEbp: 8,
                initialFrameMinus10: null,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("A missing first read performs no descriptor-address computation or write",
            missingInitialRead.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10
            && missingInitialRead.InitialFrameMinus10Address == 0xFFFFFFF8
            && missingInitialRead.InitialFrameMinus10ReadReached
            && !missingInitialRead.InitialFrameMinus10Captured
            && !missingInitialRead.DescriptorAddressComputationReached
            && !missingInitialRead.DescriptorAddressWriteReached
            && missingInitialRead.WrittenFrameMinus10 is null
            && missingInitialRead.UnreachedFreshFrameMinus4CaptureProvided
            && missingInitialRead.UnreachedFreshFrameMinus10CaptureProvided
            && missingInitialRead.UnreachedFreshEaxPlusCCaptureProvided
            && missingInitialRead.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeDescriptorReloadPlan missingFrameMinus4 =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                entryEbp: 0,
                initialFrameMinus10: 1,
                freshFrameMinus4: null,
                freshFrameMinus10: 0x100,
                freshEaxPlusC: 0x200));
        check("A missing EBP-4 read preserves the earlier wrapped-address write",
            missingFrameMinus4.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus4
            && missingFrameMinus4.InitialFrameMinus10Captured
            && missingFrameMinus4.ComputedDescriptorAddress == 0x01EDADF8
            && missingFrameMinus4.DescriptorAddressWriteReached
            && missingFrameMinus4.WrittenFrameMinus10 == 0x01EDADF8
            && missingFrameMinus4.FreshFrameMinus4Address == 0xFFFFFFFC
            && missingFrameMinus4.FreshFrameMinus4ReadReached
            && !missingFrameMinus4.FreshFrameMinus4Captured
            && !missingFrameMinus4.FreshFrameMinus10ReadReached
            && missingFrameMinus4.UnreachedFreshFrameMinus10CaptureProvided
            && missingFrameMinus4.UnreachedFreshEaxPlusCCaptureProvided
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x012C8EA7);

        CapacityFinalCalleeDescriptorReloadPlan missingReload =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                initialFrameMinus10: 1,
                freshFrameMinus4: 0x40,
                freshFrameMinus10: null,
                freshEaxPlusC: 0x200));
        check("A missing reloaded EBP-10 read preserves the prior write and stops before CMP",
            missingReload.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus10
            && missingReload.DescriptorAddressWriteReached
            && missingReload.WrittenFrameMinus10 == 0x01EDADF8
            && missingReload.FreshFrameMinus4Captured
            && missingReload.FreshFrameMinus10Address
                == CapacityFinalCalleeDescriptorReloadProjection.DeriveFrameMinus10Address(
                    0x12345678)
            && missingReload.FreshFrameMinus10ReadReached
            && !missingReload.FreshFrameMinus10Captured
            && !missingReload.FirstCompareReached
            && missingReload.UnreachedFreshEaxPlusCCaptureProvided
            && missingReload.StopAtRawFileOffsetExclusive == 0x012C8EAA);

        CapacityFinalCalleeDescriptorReloadPlan firstEquality =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                initialFrameMinus10: 1,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0x30));
        check("The fresh reload, not the preceding write, controls the first equality exit",
            firstEquality.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && firstEquality.WrittenFrameMinus10 == 0x01EDADF8
            && firstEquality.FreshFrameMinus4Captured
            && firstEquality.FreshFrameMinus4 == 0
            && firstEquality.FreshFrameMinus10Captured
            && firstEquality.FreshFrameMinus10 == 0
            && firstEquality.WrittenFrameMinus10 != firstEquality.FreshFrameMinus10
            && firstEquality.FirstCompareResult == 0
            && firstEquality.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && firstEquality.FirstCompareEqual == true
            && firstEquality.FirstBranchReached
            && firstEquality.FirstBranchTaken == true
            && firstEquality.FirstSelectedDestinationRawFileOffset == 0x012C8EBE
            && !firstEquality.EntryEbxCompareReached
            && !firstEquality.FreshEaxPlusCReadReached
            && firstEquality.UnreachedFreshEaxPlusCCaptureProvided
            && firstEquality.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorReloadPlan unsignedBelow =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x7FFFFFFF,
                freshFrameMinus10: 0x80000000,
                entryEbx: 0x80000000,
                freshEaxPlusC: 0));
        check("JB uses unsigned CMP carry at the high-bit boundary and skips the later read",
            unsignedBelow.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit
            && unsignedBelow.FirstCompareResult == 0xFFFFFFFF
            && unsignedBelow.FirstCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.FirstCompareEqual == false
            && unsignedBelow.EntryEbxCompareResult == 0xFFFFFFFF
            && unsignedBelow.EntryEbxCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.EntryEaxUnsignedBelowEntryEbx == true
            && unsignedBelow.SecondBranchTaken == true
            && unsignedBelow.SecondSelectedDestinationRawFileOffset == 0x012C8EB8
            && !unsignedBelow.FreshEaxPlusCReadReached
            && unsignedBelow.UnreachedFreshEaxPlusCCaptureProvided
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorReloadPlan finalEquality =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x400,
                freshFrameMinus10: 0x100,
                entryEbx: 0x300,
                entryEsi: 0,
                freshEaxPlusC: 0));
        check("The second equality route follows EAX+0C and compares the fresh value with entry ESI",
            finalEquality.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
            && finalEquality.InitialFrameMinus10Captured
            && finalEquality.InitialFrameMinus10 == 0
            && finalEquality.FirstCompareResult == 0x300
            && finalEquality.FirstCompareEqual == false
            && finalEquality.EntryEbxCompareResult == 0x100
            && finalEquality.EntryEaxUnsignedBelowEntryEbx == false
            && finalEquality.SecondBranchTaken == false
            && finalEquality.FreshEaxPlusCAddress == 0x40C
            && finalEquality.FreshEaxPlusCReadReached
            && finalEquality.FreshEaxPlusCCaptured
            && finalEquality.FreshEaxPlusC == 0
            && finalEquality.FinalCompareResult == 0
            && finalEquality.FinalCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && finalEquality.FinalBranchReached
            && finalEquality.FinalBranchTaken == true
            && finalEquality.FinalSelectedDestinationRawFileOffset == 0x012C8EBE
            && finalEquality.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeDescriptorReloadPlan fallThrough =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                entryEbp: 0x20,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0xFFFFFFF8,
                freshFrameMinus10: 0x10,
                entryEbx: 0xFFFFFFF0,
                entryEsi: 0x7FFFFFFF,
                freshEaxPlusC: 0x80000000));
        check("A non-equal final CMP falls through to EB8 with exact flags and wrapping EAX+0C",
            fallThrough.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary
            && fallThrough.InitialFrameMinus10Address == 0x10
            && fallThrough.FreshEaxPlusCAddress == 4
            && fallThrough.FinalCompareResult == 1
            && fallThrough.FinalCompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && fallThrough.FinalCompareEqual == false
            && fallThrough.FinalBranchReached
            && fallThrough.FinalBranchTaken == false
            && fallThrough.FinalSelectedDestinationRawFileOffset is null
            && fallThrough.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeDescriptorReloadPlan missingEaxPlusC =
            CapacityFinalCalleeDescriptorReloadProjection.Project(Capture(
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x400,
                freshFrameMinus10: 0x100,
                entryEbx: 0x300,
                freshEaxPlusC: null));
        check("A missing EAX+0C read preserves the write and stops before the final CMP",
            missingEaxPlusC.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshEaxPlusC
            && missingEaxPlusC.DescriptorAddressWriteReached
            && missingEaxPlusC.WrittenFrameMinus10
                == CapacityFinalCalleeDescriptorReloadProjection.EncodedDescriptorBasePeVirtualAddress
            && missingEaxPlusC.FirstCompareReached
            && missingEaxPlusC.EntryEbxCompareReached
            && missingEaxPlusC.FreshEaxPlusCAddress == 0x40C
            && missingEaxPlusC.FreshEaxPlusCReadReached
            && !missingEaxPlusC.FreshEaxPlusCCaptured
            && !missingEaxPlusC.FinalCompareReached
            && missingEaxPlusC.StopAtRawFileOffsetExclusive == 0x012C8EB3);

        check("Scaled descriptor and effective addresses wrap as 32-bit DWORD arithmetic",
            CapacityFinalCalleeDescriptorReloadProjection.DeriveDescriptorAddress(
                0xFFFFFFFF) == 0x01EDADE8
            && CapacityFinalCalleeDescriptorReloadProjection.DeriveFrameMinus10Address(8)
                == 0xFFFFFFF8
            && CapacityFinalCalleeDescriptorReloadProjection.DeriveEaxPlusCAddress(
                0xFFFFFFF8) == 4);
    }

    private static CapacityFinalCalleeDescriptorReloadCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEbp = 0x12345678,
        uint entryEbx = 0x300,
        uint entryEsi = 0x55,
        uint? initialFrameMinus10 = 1,
        uint? freshFrameMinus4 = 0x400,
        uint? freshFrameMinus10 = 0x100,
        uint? freshEaxPlusC = 0x200) =>
        new(
            captureValidatedEntry,
            entryEbp,
            entryEbx,
            entryEsi,
            initialFrameMinus10,
            freshFrameMinus4,
            freshFrameMinus10,
            freshEaxPlusC);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8E9A, "8B 4D F0", null, null, null),
            (0x012C8E9D, "8D 04 CD F0 AD ED 01", 0x01EDADF0, null, null),
            (0x012C8EA4, "89 45 F0", null, null, null),
            (0x012C8EA7, "8B 45 FC", null, null, null),
            (0x012C8EAA, "3B 45 F0", null, null, null),
            (0x012C8EAD, "74 0F", null, 0x012C8EBE, 0x016C9ABE),
            (0x012C8EAF, "3B C3", null, null, null),
            (0x012C8EB1, "72 05", null, 0x012C8EB8, 0x016C9AB8),
            (0x012C8EB3, "39 70 0C", null, null, null),
            (0x012C8EB6, "74 06", null, 0x012C8EBE, 0x016C9ABE)
        };
        uint nextRawOffset =
            CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeDescriptorReloadInstructionEvidence instruction
            in CapacityFinalCalleeDescriptorReloadProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeDescriptorReloadProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeDescriptorReloadProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeDescriptorReloadProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeDescriptorReloadProjection.CapturedWindowBytes)
            && CapacityFinalCalleeDescriptorReloadProjection.Instructions
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
