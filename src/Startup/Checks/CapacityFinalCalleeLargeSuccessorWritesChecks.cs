using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSuccessorWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/IDA/VA addresses, exact bytes, and both bounded branch targets match",
            CapacityFinalCalleeLargeSuccessorWritesProjection.EntryRawFileOffset
                == 0x012C91C5
            && CapacityFinalCalleeLargeSuccessorWritesProjection.EntryVirtualAddress
                == 0x016C9DC5
            && CapacityFinalCalleeLargeSuccessorWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91D6
            && CapacityFinalCalleeLargeSuccessorWritesProjection.UnequalDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeSuccessorWritesProjection.EqualDestinationRawFileOffset
                == 0x012C9229
            && CapacityFinalCalleeLargeSuccessorWritesProjection.EqualDestinationVirtualAddress
                == 0x016C9E29
            && CapacityFinalCalleeLargeSuccessorWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSuccessorWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeSuccessorWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeSuccessorWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSuccessorWritesProjection.UnequalDestinationRawFileOffset)
                == 0x016C9A76
            && CapacityFinalCalleeLargeSuccessorWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSuccessorWritesProjection.EqualDestinationRawFileOffset)
                == CapacityFinalCalleeLargeSuccessorWritesProjection.EqualDestinationVirtualAddress
            && Normalize(
                CapacityFinalCalleeLargeSuccessorWritesProjection.CapturedWindowBytes)
                == "3950080F85A8FCFFFF89430C895808EB53"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeSuccessorWritesPlan missingAll =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: null,
                FreshEaxPlus8: null,
                EntryEDX: null,
                LateEntryEBX: null));
        check("All missing captures stop before the fresh read and comparison",
            missingAll.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEAX
            && !missingAll.EntryEAXCaptured
            && !missingAll.FreshFieldReadReached
            && !missingAll.EntryEDXCaptured
            && !missingAll.CompareReached
            && !missingAll.JnzReached
            && !missingAll.FirstStoreReached
            && !missingAll.SecondStoreReached
            && !missingAll.JumpReached
            && missingAll.Reads.IsEmpty
            && missingAll.Writes.IsEmpty
            && !missingAll.ExactCleanExitAdmitted
            && missingAll.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeSuccessorWritesPlan missingEaxWithFutureData =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: null,
                FreshEaxPlus8: 0x10,
                EntryEDX: 0x20,
                LateEntryEBX: 0x30));
        check("Fresh field, EDX, and late EBX captures are rejected when EAX cannot form the read address",
            missingEaxWithFutureData.UnreachedFreshFieldCaptureProvided
            && missingEaxWithFutureData.UnreachedEntryEDXCaptureProvided
            && !missingEaxWithFutureData.EntryEDXCaptured
            && missingEaxWithFutureData.UnreachedLateEntryEBXCaptureProvided
            && !missingEaxWithFutureData.FreshFieldReadReached
            && !missingEaxWithFutureData.CompareReached
            && !missingEaxWithFutureData.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSuccessorWritesPlan missingFresh =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: null,
                EntryEDX: 0x20,
                LateEntryEBX: 0x30));
        check("Fresh [EAX+8] is recorded at the CMP even when its value is missing",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingFreshField
            && missingFresh.FreshFieldReadReached
            && missingFresh.FreshFieldAddress == 0x108
            && !missingFresh.FreshFieldCaptured
            && missingFresh.EntryEDXCaptured
            && !missingFresh.CompareReached
            && !missingFresh.JnzReached
            && missingFresh.UnreachedLateEntryEBXCaptureProvided
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                RawFileOffset: 0x012C91C5,
                Address: 0x108,
                ValueCaptured: false,
                Value: null
            }
            && missingFresh.Writes.IsEmpty
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeSuccessorWritesPlan missingFreshAndEdx =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: null,
                EntryEDX: null,
                LateEntryEBX: 0x30));
        check("Missing memory data and EDX remain distinct while late EBX is unreached",
            missingFreshAndEdx.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome
                    .MissingFreshFieldAndEntryEDX
            && missingFreshAndEdx.FreshFieldReadReached
            && !missingFreshAndEdx.FreshFieldCaptured
            && !missingFreshAndEdx.EntryEDXCaptured
            && missingFreshAndEdx.UnreachedLateEntryEBXCaptureProvided
            && !missingFreshAndEdx.CompareReached
            && missingFreshAndEdx.Reads.Length == 1);

        CapacityFinalCalleeLargeSuccessorWritesPlan missingEdx =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 0x200,
                EntryEDX: null,
                LateEntryEBX: 0x30));
        check("The memory operand is independently captured even when entry EDX is missing",
            missingEdx.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEDX
            && missingEdx.FreshFieldReadReached
            && missingEdx.FreshFieldAddress == 0x108
            && missingEdx.FreshFieldCaptured
            && missingEdx.FreshFieldValue == 0x200
            && missingEdx.Reads[0].Value == 0x200
            && !missingEdx.CompareReached
            && missingEdx.UnreachedLateEntryEBXCaptureProvided
            && missingEdx.Writes.IsEmpty);

        CapacityFinalCalleeLargeSuccessorWritesPlan missingLateEbx =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 0x12345678,
                EntryEDX: 0x12345678,
                LateEntryEBX: null));
        check("An equal CMP reaches the first store but cannot project writes without late EBX",
            missingLateEbx.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome
                    .MissingLateEntryEBXForEqualStores
            && missingLateEbx.CompareReached
            && missingLateEbx.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingLateEbx.JnzReached
            && missingLateEbx.JnzTaken == false
            && missingLateEbx.JnzSelectedDestinationRawFileOffset == 0x012C91CE
            && missingLateEbx.FirstStoreReached
            && missingLateEbx.FirstStoreAddress is null
            && missingLateEbx.FlagsAtStop == missingLateEbx.CompareFlags
            && !missingLateEbx.SecondStoreReached
            && !missingLateEbx.JumpReached
            && missingLateEbx.Writes.IsEmpty
            && !missingLateEbx.ExactCleanExitAdmitted
            && missingLateEbx.StopAtRawFileOffsetExclusive == 0x012C91CE);

        CapacityFinalCalleeLargeSuccessorWritesPlan equal =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0,
                FreshEaxPlus8: 0,
                EntryEDX: 0,
                LateEntryEBX: 0));
        check("A zero equality retains all six CMP flags through both ordered stores and stops at 9229",
            equal.Outcome == CapacityFinalCalleeLargeSuccessorWritesOutcome.EqualTo9229
            && equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.JnzTaken == false
            && equal.FirstStoreReached
            && equal.FirstStoreAddress == 0x0C
            && equal.FirstStoreValue == 0
            && equal.SecondStoreReached
            && equal.SecondStoreAddress == 0x08
            && equal.SecondStoreValue == 0
            && equal.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C91CEu, 0x0Cu, 0u),
                    (0x012C91D1u, 0x08u, 0u)
                })
            && equal.JumpReached
            && equal.JumpTargetRawFileOffset == 0x012C9229
            && equal.JumpTargetVirtualAddress == 0x016C9E29
            && !equal.DestinationEntered
            && equal.FlagsPreservedAcrossStores
            && equal.ExactCleanExitAdmitted
            && equal.StopAtRawFileOffsetExclusive == 0x012C9229);

        CapacityFinalCalleeLargeSuccessorWritesPlan signedOverflow =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 0x80000000,
                EntryEDX: 1,
                LateEntryEBX: null));
        check("CMP exposes correct signed overflow, auxiliary carry, parity, sign, and unequal branch flags",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.JnzTaken == true
            && signedOverflow.JnzSelectedDestinationRawFileOffset == 0x012C8E76
            && signedOverflow.Outcome
                == CapacityFinalCalleeLargeSuccessorWritesOutcome.UnequalTo8E76);

        CapacityFinalCalleeLargeSuccessorWritesPlan carryAndSign =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 1,
                EntryEDX: 2,
                LateEntryEBX: null));
        check("Unsigned borrow and negative subtraction retain the remaining CMP flag cases",
            carryAndSign.CompareResult == 0xFFFFFFFF
            && carryAndSign.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && carryAndSign.JnzTaken == true);

        CapacityFinalCalleeLargeSuccessorWritesPlan oddParity =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 2,
                EntryEDX: 1,
                LateEntryEBX: null));
        check("Parity reflects odd population count in the low result byte",
            oddParity.CompareResult == 1
            && oddParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && oddParity.JnzTaken == true);

        CapacityFinalCalleeLargeSuccessorWritesPlan unequal =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 0x10,
                EntryEDX: 0x20,
                LateEntryEBX: null));
        check("Clean unequal comparison takes 8E76 without entering its destination",
            unequal.Outcome == CapacityFinalCalleeLargeSuccessorWritesOutcome.UnequalTo8E76
            && unequal.CompareResult == 0xFFFFFFF0
            && unequal.JnzReached
            && unequal.JnzTaken == true
            && unequal.JnzSelectedDestinationRawFileOffset == 0x012C8E76
            && !unequal.FirstStoreReached
            && !unequal.SecondStoreReached
            && !unequal.JumpReached
            && !unequal.DestinationEntered
            && unequal.ExactCleanExitAdmitted
            && unequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeSuccessorWritesPlan contaminatedUnequal =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0x100,
                FreshEaxPlus8: 0x10,
                EntryEDX: 0x20,
                LateEntryEBX: 0x30));
        check("Unequal branch rejects the supplied but unused late EBX capture",
            contaminatedUnequal.JnzTaken == true
            && contaminatedUnequal.UnusedLateEntryEBXCaptureProvided
            && !contaminatedUnequal.UnreachedLateEntryEBXCaptureProvided
            && !contaminatedUnequal.ExactCleanExitAdmitted
            && contaminatedUnequal.Writes.IsEmpty
            && contaminatedUnequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeSuccessorWritesPlan aliasedWrappedStores =
            CapacityFinalCalleeLargeSuccessorWritesProjection.Project(new(
                EntryEAX: 0xFFFFFFF8,
                FreshEaxPlus8: 0x55,
                EntryEDX: 0x55,
                LateEntryEBX: 0xFFFFFFF4));
        check("Equal-path store addresses wrap at DWORD width and preserve both aliased writes in order",
            aliasedWrappedStores.FirstStoreAddress == 0
            && aliasedWrappedStores.SecondStoreAddress == 0
            && aliasedWrappedStores.FirstStoreValue == 0xFFFFFFF8
            && aliasedWrappedStores.SecondStoreValue == 0xFFFFFFF4
            && aliasedWrappedStores.Writes.Length == 2
            && aliasedWrappedStores.Writes[0]
                == new CapacityFinalCalleeLargeSuccessorWritesWrite(
                    0x012C91CE, 0, 0xFFFFFFF8)
            && aliasedWrappedStores.Writes[1]
                == new CapacityFinalCalleeLargeSuccessorWritesWrite(
                    0x012C91D1, 0, 0xFFFFFFF4)
            && aliasedWrappedStores.Writes[0].RawFileOffset
                != aliasedWrappedStores.Writes[1].RawFileOffset
            && aliasedWrappedStores.Writes[0].Address
                == aliasedWrappedStores.Writes[1].Address
            && aliasedWrappedStores.FlagsPreservedAcrossStores
            && aliasedWrappedStores.ExactCleanExitAdmitted
            && CapacityFinalCalleeLargeSuccessorWritesProjection
                .DeriveFirstStoreAddress(0xFFFFFFF4) == 0
            && CapacityFinalCalleeLargeSuccessorWritesProjection
                .DeriveSecondStoreAddress(0xFFFFFFF8) == 0
            && CapacityFinalCalleeLargeSuccessorWritesProjection
                .DeriveFreshFieldAddress(0xFFFFFFF8) == 0);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw) []
        {
            (0x012C91C5, "39 50 08", null),
            (0x012C91C8, "0F 85 A8 FC FF FF", 0x012C8E76),
            (0x012C91CE, "89 43 0C", null),
            (0x012C91D1, "89 58 08", null),
            (0x012C91D4, "EB 53", 0x012C9229)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeSuccessorWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeSuccessorWritesInstructionEvidence instruction
            in CapacityFinalCalleeLargeSuccessorWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeSuccessorWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetVirtualAddress
                    != (instruction.BranchTargetRawFileOffset is uint targetRaw
                        ? CapacityFinalCalleeLargeSuccessorWritesProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                        : null))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeSuccessorWritesProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeSuccessorWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeSuccessorWritesProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
