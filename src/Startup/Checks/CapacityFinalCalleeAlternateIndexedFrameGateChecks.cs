using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateIndexedFrameGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete bounded indexed frame window and branch targets match",
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryRawFileOffset
                == 0x012C913D
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryVirtualAddress
                == 0x016C9D3D
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9151
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.EqualDestinationRawFileOffset
                == 0x012C9160
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.UnequalDestinationRawFileOffset
                == 0x012C9151
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryRawFileOffset)
                == 0x016C9D3D
            && Normalize(CapacityFinalCalleeAlternateIndexedFrameGateProjection.CapturedWindowBytes)
                == "8B45FC8B4DF08D04C5F0ADED018945F43BC8740F"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateIndexedFrameGatePlan missingEbp =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus4: 0,
                FreshFrameMinus10: 0));
        check("Missing EBP gates both independent frame reads and records later captures",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingEntryEBP
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FrameMinus4ReadReached
            && missingEbp.FrameMinus4Address is null
            && !missingEbp.FrameMinus4Captured
            && !missingEbp.FrameMinus10ReadReached
            && missingEbp.FrameMinus10Address is null
            && !missingEbp.IndexedAddressCalculationReached
            && !missingEbp.StackStoreReached
            && !missingEbp.CompareReached
            && !missingEbp.JeReached
            && missingEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingEbp.UnreachedFreshFrameMinus10CaptureProvided
            && missingEbp.LateFreshFrameMinus4CaptureProvided
            && missingEbp.LateFreshFrameMinus10CaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingEbp.Writes.IsEmpty
            && !missingEbp.ExactCleanRouteAdmitted);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan missingFrameMinus4 =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus4: null,
                FreshFrameMinus10: 0));
        check("Missing first fresh frame value blocks the second load and marks it late",
            missingFrameMinus4.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus4
            && missingFrameMinus4.EntryEBPCaptured
            && missingFrameMinus4.FrameMinus4ReadReached
            && missingFrameMinus4.FrameMinus4Address == 0xFC
            && !missingFrameMinus4.FrameMinus4Captured
            && !missingFrameMinus4.FrameMinus10ReadReached
            && missingFrameMinus4.FrameMinus10Address is null
            && !missingFrameMinus4.IndexedAddressCalculationReached
            && !missingFrameMinus4.StackStoreReached
            && !missingFrameMinus4.CompareReached
            && missingFrameMinus4.UnreachedFreshFrameMinus10CaptureProvided
            && missingFrameMinus4.LateFreshFrameMinus10CaptureProvided
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingFrameMinus4.Writes.IsEmpty);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan missingFrameMinus10 =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus4: 0,
                FreshFrameMinus10: null));
        check("Missing second fresh frame value reaches only its own load",
            missingFrameMinus10.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus10
            && missingFrameMinus10.EntryEBPCaptured
            && missingFrameMinus10.FrameMinus4ReadReached
            && missingFrameMinus10.FrameMinus4Address == 0x1C
            && missingFrameMinus10.FrameMinus4Captured
            && missingFrameMinus10.FrameMinus4 == 0
            && missingFrameMinus10.FrameMinus10ReadReached
            && missingFrameMinus10.FrameMinus10Address == 0x10
            && !missingFrameMinus10.FrameMinus10Captured
            && !missingFrameMinus10.IndexedAddressCalculationReached
            && !missingFrameMinus10.StackStoreReached
            && !missingFrameMinus10.CompareReached
            && !missingFrameMinus10.LateFreshFrameMinus10CaptureProvided
            && missingFrameMinus10.StopAtRawFileOffsetExclusive == 0x012C9140
            && missingFrameMinus10.Writes.IsEmpty);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan zeroIndexedValue =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinus4: 0,
                FreshFrameMinus10: 0x01EDADF0));
        check("A fresh zero frame value produces the indexed base, stores it, and takes JE",
            zeroIndexedValue.FrameMinus4 == 0
            && zeroIndexedValue.FrameMinus10 == 0x01EDADF0
            && zeroIndexedValue.IndexedAddressInput == 0
            && zeroIndexedValue.IndexedAddress == 0x01EDADF0
            && zeroIndexedValue.StackStoreReached
            && zeroIndexedValue.StackStoreAddress == 0xFFFFFFF4
            && zeroIndexedValue.StackStoreValue == 0x01EDADF0
            && zeroIndexedValue.Writes.Length == 1
            && zeroIndexedValue.Writes[0]
                == new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                    0x012C914A, 0xFFFFFFF4, 0x01EDADF0)
            && zeroIndexedValue.CompareResult == 0
            && zeroIndexedValue.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroIndexedValue.JeTaken == true
            && zeroIndexedValue.JeSelectedDestinationRawFileOffset == 0x012C9160
            && zeroIndexedValue.ExactCleanRouteAdmitted
            && zeroIndexedValue.StopAtRawFileOffsetExclusive == 0x012C9160);

        uint wrappedIndexedAddress =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.CalculateIndexedAddress(
                0xFFFFFFFF);
        CapacityFinalCalleeAlternateIndexedFrameGatePlan wrapped =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 4,
                FreshFrameMinus4: 0xFFFFFFFF,
                FreshFrameMinus10: wrappedIndexedAddress));
        check("Indexed LEA, local store address, and stored value use DWORD wraparound",
            wrappedIndexedAddress == 0x01EDADE8
            && wrapped.FrameMinus4Address == 0
            && wrapped.FrameMinus10Address == 0xFFFFFFF4
            && wrapped.IndexedAddress == 0x01EDADE8
            && wrapped.StackStoreAddress == 0xFFFFFFF8
            && wrapped.StackStoreValue == 0x01EDADE8
            && wrapped.Writes.Length == 1
            && wrapped.Writes[0].Address == 0xFFFFFFF8
            && wrapped.Writes[0].Value == 0x01EDADE8
            && wrapped.JeTaken == true
            && wrapped.JeSelectedDestinationRawFileOffset == 0x012C9160
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan signedOverflow =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinus4: 0x0FC24A42,
                FreshFrameMinus10: 0x7FFFFFFF));
        check("Unequal CMP models signed overflow and preserves its pre-CMP local store",
            signedOverflow.IndexedAddress == 0x80000000
            && signedOverflow.StackStoreValue == 0x80000000
            && signedOverflow.Writes.Length == 1
            && signedOverflow.CompareLeft == 0x7FFFFFFF
            && signedOverflow.CompareRight == 0x80000000
            && signedOverflow.CompareResult == 0xFFFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && signedOverflow.JeTaken == false
            && signedOverflow.JeSelectedDestinationRawFileOffset == 0x012C9151
            && signedOverflow.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough
            && signedOverflow.ExactCleanRouteAdmitted
            && signedOverflow.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan oddParityAndAuxiliary =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0x200,
                FreshFrameMinus4: 0x1FC24A43,
                FreshFrameMinus10: 0));
        check("Unequal wrapped subtraction models odd parity and auxiliary borrow",
            oddParityAndAuxiliary.IndexedAddress == 8
            && oddParityAndAuxiliary.CompareResult == 0xFFFFFFF8
            && oddParityAndAuxiliary.CompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && oddParityAndAuxiliary.JeTaken == false
            && oddParityAndAuxiliary.JeSelectedDestinationRawFileOffset == 0x012C9151
            && oddParityAndAuxiliary.ExactCleanRouteAdmitted
            && oddParityAndAuxiliary.Writes.Length == 1
            && oddParityAndAuxiliary.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeAlternateIndexedFrameGatePlan independentReloads =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(new(
                EntryEBP: 0x1234,
                FreshFrameMinus4: 0x11223344,
                FreshFrameMinus10: 0x55667788));
        check("The two fresh frame captures are independent and feed their own operands",
            independentReloads.FrameMinus4Address == 0x1230
            && independentReloads.FrameMinus10Address == 0x1224
            && independentReloads.FrameMinus4 == 0x11223344
            && independentReloads.FrameMinus10 == 0x55667788
            && independentReloads.IndexedAddressInput == 0x11223344
            && independentReloads.CompareLeft == 0x55667788
            && independentReloads.CompareRight
                == CapacityFinalCalleeAlternateIndexedFrameGateProjection
                    .CalculateIndexedAddress(0x11223344)
            && independentReloads.StackStoreValue == independentReloads.CompareRight);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C913D, "8B 45 FC", null, null),
            (0x012C9140, "8B 4D F0", null, null),
            (0x012C9143, "8D 04 C5 F0 AD ED 01", null, null),
            (0x012C914A, "89 45 F4", null, null),
            (0x012C914D, "3B C8", null, null),
            (0x012C914F, "74 0F", 0x012C9160, 0x016C9D60)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateIndexedFrameGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateIndexedFrameGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateIndexedFrameGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateIndexedFrameGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateIndexedFrameGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateIndexedFrameGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateIndexedFrameGateProjection.Instructions
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
