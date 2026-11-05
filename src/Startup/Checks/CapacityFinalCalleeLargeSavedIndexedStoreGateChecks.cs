using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedIndexedStoreGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/VA anchors, instruction bytes, boundary, and branch target match",
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryRawFileOffset
                == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryFlatIdaAddress
                == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryVirtualAddress
                == 0x016C9E43
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9251
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.JnzDestinationRawFileOffset
                == 0x012C927C
            && Normalize(CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.CapturedWindowBytes)
                == "8B5A1C89049DF8AEED0185C0752B"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan allMissing =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: null,
                FreshEDXPlus1C: null,
                EntryEAX: null));
        check("All missing captures stop at the fresh load with no store, TEST, or branch",
            allMissing.Outcome
                == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEDXForFreshRead
            && allMissing.FreshIndexReadReached
            && allMissing.Reads is
                [{ RawFileOffset: 0x012C9243, Address: null, AddressCaptured: false }]
            && !allMissing.StoreInstructionReached
            && allMissing.Writes.IsEmpty
            && !allMissing.TestReached
            && !allMissing.JnzReached
            && allMissing.FlagsAtStop is null
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan missingEDX =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: null,
                FreshEDXPlus1C: 0x12345678,
                EntryEAX: 0x87654321));
        check("Missing first-use EDX leaves supplied index and EAX unused",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEDXForFreshRead
            && missingEDX.Reads[0].Address is null
            && !missingEDX.Reads[0].ValueCaptured
            && missingEDX.UnreachedFreshEDXPlus1CCaptureProvided
            && missingEDX.UnreachedEntryEAXCaptureProvided
            && !missingEDX.StoreInstructionReached
            && missingEDX.Writes.IsEmpty
            && !missingEDX.TestReached
            && !missingEDX.JnzReached
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan missingIndex =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: null,
                EntryEAX: 0x87654321));
        check("Missing fresh index retains the known load address and prevents later instructions",
            missingIndex.Outcome
                == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.MissingFreshEDXPlus1C
            && missingIndex.Reads is
                [{
                    RawFileOffset: 0x012C9243,
                    Address: 0x11C,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }]
            && missingIndex.UnreachedEntryEAXCaptureProvided
            && !missingIndex.StoreInstructionReached
            && missingIndex.Writes.IsEmpty
            && !missingIndex.TestReached
            && !missingIndex.JnzReached
            && missingIndex.FlagsAtStop is null
            && missingIndex.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan missingEAX =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 3,
                EntryEAX: null));
        check("Missing EAX records the reached store with a known address and unknown value",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.MissingEntryEAXForStore
            && missingEAX.StoreInstructionReached
            && missingEAX.StoreAddress == 0x01EDAF04
            && !missingEAX.StoreValueCaptured
            && missingEAX.StoreValue is null
            && missingEAX.Writes is
                [{
                    RawFileOffset: 0x012C9246,
                    Address: 0x01EDAF04,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !missingEAX.TestReached
            && !missingEAX.JnzReached
            && missingEAX.FlagsAtStop is null
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C9246);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan zero =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0,
                FreshEDXPlus1C: 0,
                EntryEAX: 0));
        check("A zero EAX is stored before TEST and selects only the bounded 9251 fallthrough",
            zero.Outcome
                == CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.ZeroToFallthrough9251
            && zero.FreshIndexReadAddress == 0x1C
            && zero.FreshIndex == 0
            && zero.StoreAddress == 0x01EDAEF8
            && zero.StoreValueCaptured
            && zero.StoreValue == 0
            && zero.Writes is
                [{ RawFileOffset: 0x012C9246, Address: 0x01EDAEF8, Value: 0, Committed: true }]
            && zero.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C9243, 0x012C9246, 0x012C924D, 0x012C924F })
            && zero.TestReached
            && zero.TestResult == 0
            && zero.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.JnzReached
            && zero.JnzTaken == false
            && zero.JnzSelectedDestinationRawFileOffset == 0x012C9251
            && zero.StopAtRawFileOffsetExclusive == 0x012C9251
            && zero.ExactCleanCompletion
            && !zero.DestinationEntered
            && !zero.FollowingZeroPathReached);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan highBit =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 7,
                EntryEAX: 0x80000000));
        check("TEST derives sign, zero, parity, and defined carry/overflow for high-bit EAX",
            highBit.StoreValue == 0x80000000
            && highBit.TestResult == 0x80000000
            && highBit.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBit.JnzTaken == true
            && highBit.JnzSelectedDestinationRawFileOffset == 0x012C927C
            && highBit.StopAtRawFileOffsetExclusive == 0x012C927C
            && !highBit.DestinationEntered
            && !highBit.FollowingZeroPathReached);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan allOnes =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 0xFFFFFFFF,
                EntryEAX: uint.MaxValue));
        check("All-ones index and EAX retain DWORD semantics and take the nonzero branch",
            allOnes.StoreAddress == 0x01EDAEF4
            && allOnes.StoreValue == uint.MaxValue
            && allOnes.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allOnes.JnzTaken == true
            && allOnes.JnzSelectedDestinationRawFileOffset == 0x012C927C
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                .DeriveFreshIndexedStoreAddress(uint.MaxValue) == 0x01EDAEF4);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan wrapped =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                FreshEDXPlus1C: 0xFFFFFFFF,
                EntryEAX: 1));
        check("Fresh-load and scaled indexed-store addresses use independent DWORD wrapping",
            wrapped.FreshIndexReadAddress == 0xC
            && wrapped.Reads[0].Address == 0xC
            && wrapped.StoreAddress == 0x01EDAEF4
            && wrapped.Writes[0].Address == 0x01EDAEF4
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                .DeriveFreshEDXPlus1CAddress(0xFFFFFFF0) == 0xC
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                .DeriveFreshIndexedStoreAddress(0xFFFFFFFF) == 0x01EDAEF4);

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan aliased =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Project(new(
                EntryEDX: 0x01EDAEF8 - 0x1C,
                FreshEDXPlus1C: 0,
                EntryEAX: 0));
        check("An aliased fresh-load and store address remains two distinct ordered operations",
            aliased.FreshIndexReadAddress == 0x01EDAEF8
            && aliased.StoreAddress == 0x01EDAEF8
            && aliased.Reads.Length == 1
            && aliased.Writes.Length == 1
            && aliased.Reads[0].RawFileOffset == 0x012C9243
            && aliased.Writes[0].RawFileOffset == 0x012C9246
            && aliased.Reads[0].Address == aliased.Writes[0].Address
            && aliased.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C9243, 0x012C9246, 0x012C924D, 0x012C924F }));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9243, "8B 5A 1C", null, null),
            (0x012C9246, "89 04 9D F8 AE ED 01", null, null),
            (0x012C924D, "85 C0", null, null),
            (0x012C924F, "75 2B", 0x012C927C, 0x016C9E7C)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeSavedIndexedStoreGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeSavedIndexedStoreGateProjection.Instructions
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
