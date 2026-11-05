using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonIndexedAddressGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded LEA/MOV/CMP/JNB/CALL bytes and addresses match",
            CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset
                == 0x012C92F6
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryVirtualAddress
                == 0x016C9EF6
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C930A
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.OpaqueCallRawFileOffset
                == 0x012C9304
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.BranchTargetRawFileOffset
                == 0x012C930A
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.FallthroughRawFileOffset
                == 0x012C9304
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset)
                == 0x016C9EF6
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.LEABaseVirtualAddress
                == 0x01EDADF0
            && Normalize(CapacityFinalCalleeCommonIndexedAddressGateProjection.CapturedWindowBytes)
                == "8D14CDF0ADED018BC283FF107306FF15244AAC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonIndexedAddressGatePlan allMissing =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: null,
                EntryEDI: null));
        check("Missing ECX stops at LEA and does not consume EDI",
            allMissing.Outcome
                == CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryECX
            && !allMissing.EntryECXCaptured
            && !allMissing.LEAReached
            && !allMissing.MoveEAXReached
            && !allMissing.EntryEDICaptured
            && !allMissing.CompareReached
            && !allMissing.JnbReached
            && !allMissing.ExactCleanBoundedEdge
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonIndexedAddressGatePlan missingECXWithEDI =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: null,
                EntryEDI: 0x10));
        check("A supplied EDI remains unconsumed when ECX is missing",
            !missingECXWithEDI.EntryEDICaptured
            && !missingECXWithEDI.CompareReached
            && missingECXWithEDI.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonIndexedAddressGatePlan missingEDI =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 2,
                EntryEDI: null));
        check("Missing EDI retains the derived EDX/EAX address and stops at CMP",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryEDI
            && missingEDI.EntryECXCaptured
            && missingEDI.LEAReached
            && missingEDI.LEAIndex == 2
            && missingEDI.LEAScaledIndex == 16
            && missingEDI.DerivedEDX == 0x01EDAE00
            && missingEDI.MoveEAXReached
            && missingEDI.DerivedEAX == missingEDI.DerivedEDX
            && !missingEDI.EntryEDICaptured
            && !missingEDI.CompareReached
            && !missingEDI.JnbReached
            && !missingEDI.ExactCleanBoundedEdge
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C92FF);

        CapacityFinalCalleeCommonIndexedAddressGatePlan zeroIndex =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 0,
                EntryEDI: 0));
        check("Zero index produces the LEA base while zero EDI selects the bounded call edge",
            zeroIndex.DerivedEDX == 0x01EDADF0
            && zeroIndex.DerivedEAX == 0x01EDADF0
            && zeroIndex.CompareResult == 0xFFFFFFF0
            && zeroIndex.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroIndex.JnbTaken == false
            && zeroIndex.JnbSelectedDestinationRawFileOffset == 0x012C9304
            && zeroIndex.ExactCleanBoundedEdge
            && zeroIndex.OpaqueCallInstructionAtStop
            && !zeroIndex.OpaqueCallEntered
            && !zeroIndex.OpaqueCallReturnObserved
            && !zeroIndex.CalleeIdentityKnown);

        CapacityFinalCalleeCommonIndexedAddressGatePlan wrappedIndex =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 0x20000000,
                EntryEDI: 0x10));
        CapacityFinalCalleeCommonIndexedAddressGatePlan wrappedAddress =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 0xFFFFFFFF,
                EntryEDI: 0x10));
        check("LEA multiplication and addition wrap as unchecked DWORD address arithmetic",
            wrappedIndex.LEAScaledIndex == 0
            && wrappedIndex.DerivedEDX == 0x01EDADF0
            && wrappedAddress.LEAScaledIndex == 0xFFFFFFF8
            && wrappedAddress.DerivedEDX == 0x01EDADE8
            && wrappedAddress.DerivedEAX == 0x01EDADE8);

        CapacityFinalCalleeCommonIndexedAddressGatePlan belowThreshold =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 1,
                EntryEDI: 0xF));
        check("EDI F remains below the unsigned threshold with complete CMP flags",
            belowThreshold.CompareResult == 0xFFFFFFFF
            && belowThreshold.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowThreshold.JnbTaken == false
            && belowThreshold.JnbSelectedDestinationRawFileOffset == 0x012C9304
            && belowThreshold.ExactCleanBoundedEdge
            && belowThreshold.OpaqueCallInstructionAtStop);

        CapacityFinalCalleeCommonIndexedAddressGatePlan threshold =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 1,
                EntryEDI: 0x10));
        check("EDI 10 takes JNB on unsigned equality with complete CMP flags",
            threshold.CompareResult == 0
            && threshold.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && threshold.JnbTaken == true
            && threshold.JnbSelectedDestinationRawFileOffset == 0x012C930A
            && threshold.ExactCleanBoundedEdge
            && threshold.StopAtRawFileOffsetExclusive == 0x012C930A);

        CapacityFinalCalleeCommonIndexedAddressGatePlan allBitsSet =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: 0,
                EntryEDI: 0xFFFFFFFF));
        check("EDI FFFFFFFF takes the unsigned branch with all six CMP flags",
            allBitsSet.CompareResult == 0xFFFFFFEF
            && allBitsSet.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allBitsSet.JnbTaken == true
            && allBitsSet.JnbSelectedDestinationRawFileOffset == 0x012C930A
            && allBitsSet.ExactCleanBoundedEdge);

        check("Both outcomes stop at clean bounded edges without invoking the opaque call",
            new[] { zeroIndex, belowThreshold, threshold, allBitsSet }.All(plan =>
                plan.ExactCleanBoundedEdge
                && !plan.OpaqueCallEntered
                && !plan.OpaqueCallReturnObserved
                && !plan.CalleeIdentityKnown
                && plan.Writes.IsEmpty)
            && zeroIndex.StopAtRawFileOffsetExclusive == 0x012C9304
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x012C9304
            && threshold.StopAtRawFileOffsetExclusive == 0x012C930A
            && allBitsSet.StopAtRawFileOffsetExclusive == 0x012C930A);

        check("LEA computes an address only and performs no memory read",
            new[] { missingEDI, zeroIndex, wrappedIndex, wrappedAddress }.All(plan =>
                plan.AddressOnlyLEA
                && !plan.MemoryReadPerformed
                && plan.DerivedEDX.HasValue));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92F6, "8D 14 CD F0 AD ED 01", null, null),
            (0x012C92FD, "8B C2", null, null),
            (0x012C92FF, "83 FF 10", null, null),
            (0x012C9302, "73 06", 0x012C930A, 0x016C9F0A),
            (0x012C9304, "FF 15 24 4A AC 01", null, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonIndexedAddressGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonIndexedAddressGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonIndexedAddressGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonIndexedAddressGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonIndexedAddressGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonIndexedAddressGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonIndexedAddressGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonIndexedAddressGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonIndexedAddressGateProjection.Instructions
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
