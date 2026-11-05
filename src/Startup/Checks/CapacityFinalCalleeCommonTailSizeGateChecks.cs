using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTailSizeGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded MOV/SHR/CMP/JNB bytes, addresses, and destinations match",
            CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset
                == 0x012C92EC
            && CapacityFinalCalleeCommonTailSizeGateProjection.EntryVirtualAddress
                == 0x016C9EEC
            && CapacityFinalCalleeCommonTailSizeGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C92F6
            && CapacityFinalCalleeCommonTailSizeGateProjection.BranchTargetRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonTailSizeGateProjection.FallthroughRawFileOffset
                == 0x012C92F6
            && CapacityFinalCalleeCommonTailSizeGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonTailSizeGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonTailSizeGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonTailSizeGateProjection.BranchTargetRawFileOffset)
                == 0x016C9F54
            && Normalize(CapacityFinalCalleeCommonTailSizeGateProjection.CapturedWindowBytes)
                == "8BCFC1E90383F920735E"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonTailSizeGatePlan missingEntry =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(EntryEDI: null));
        check("Missing entry EDI stops at MOV without evaluating or entering a destination",
            missingEntry.Outcome
                == CapacityFinalCalleeCommonTailSizeGateOutcome.MissingEntryEDI
            && !missingEntry.EntryEDICaptured
            && !missingEntry.MoveReached
            && !missingEntry.ShiftReached
            && !missingEntry.CompareReached
            && !missingEntry.JnbReached
            && !missingEntry.ExactCleanAdmission
            && !missingEntry.TargetEntered
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonTailSizeGatePlan zero =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(EntryEDI: 0));
        check("EDI zero shifts to zero with defined SHR flags and takes the below-threshold path",
            zero.ShiftInput == 0
            && zero.ShiftCount == 3
            && zero.ShiftResult == 0
            && zero.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && zero.CompareResult == 0xFFFFFFE0
            && zero.CompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zero.JnbTaken == false
            && zero.JnbSelectedDestinationRawFileOffset == 0x012C92F6
            && zero.ExactCleanAdmission
            && zero.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonTailSizeGatePlan belowThreshold =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(EntryEDI: 0xFF));
        check("EDI FF shifts to 1F and remains below the unsigned threshold",
            belowThreshold.ShiftResult == 0x1F
            && belowThreshold.ShiftFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && belowThreshold.CompareResult == 0xFFFFFFFF
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
            && belowThreshold.JnbSelectedDestinationRawFileOffset == 0x012C92F6
            && belowThreshold.ExactCleanAdmission);

        CapacityFinalCalleeCommonTailSizeGatePlan threshold =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(new(EntryEDI: 0x100));
        check("EDI 100 shifts exactly to 20h and branches on unsigned equality",
            threshold.ShiftResult == 0x20
            && threshold.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && threshold.CompareResult == 0
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
            && threshold.JnbSelectedDestinationRawFileOffset == 0x012C9354
            && threshold.ExactCleanAdmission
            && threshold.StopAtRawFileOffsetExclusive == 0x012C9354);

        CapacityFinalCalleeCommonTailSizeGatePlan signedPositiveBoundary =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(
                new(EntryEDI: 0x7FFFFFFF));
        CapacityFinalCalleeCommonTailSizeGatePlan signedNegativeBoundary =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(
                new(EntryEDI: 0x80000000));
        check("Signed-boundary values use unsigned CMP/JNB semantics",
            signedPositiveBoundary.ShiftResult == 0x0FFFFFFF
            && signedPositiveBoundary.CompareResult == 0x0FFFFFDF
            && signedPositiveBoundary.CompareFlags?.Carry == false
            && signedPositiveBoundary.JnbTaken == true
            && signedPositiveBoundary.JnbSelectedDestinationRawFileOffset == 0x012C9354
            && signedNegativeBoundary.ShiftResult == 0x10000000
            && signedNegativeBoundary.CompareResult == 0x0FFFFFE0
            && signedNegativeBoundary.CompareFlags?.Carry == false
            && signedNegativeBoundary.JnbTaken == true
            && signedNegativeBoundary.JnbSelectedDestinationRawFileOffset == 0x012C9354);

        CapacityFinalCalleeCommonTailSizeGatePlan allBitsSet =
            CapacityFinalCalleeCommonTailSizeGateProjection.Project(
                new(EntryEDI: 0xFFFFFFFF));
        check("EDI FFFFFFFF preserves SHR carry while CMP independently selects the unsigned branch",
            allBitsSet.ShiftResult == 0x1FFFFFFF
            && allBitsSet.ShiftFlags?.Carry == true
            && allBitsSet.CompareResult == 0x1FFFFFDF
            && allBitsSet.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && allBitsSet.JnbTaken == true
            && allBitsSet.JnbSelectedDestinationRawFileOffset == 0x012C9354
            && allBitsSet.ExactCleanAdmission);

        check("The projection stops cleanly at either selected edge without writes or target entry",
            new[] { zero, belowThreshold }.All(plan =>
                plan.ExactCleanAdmission
                && !plan.TargetEntered
                && plan.Writes.IsEmpty
                && plan.StopAtRawFileOffsetExclusive == 0x012C92F6)
            && new[] { threshold, signedPositiveBoundary, signedNegativeBoundary, allBitsSet }
                .All(plan =>
                    plan.ExactCleanAdmission
                    && !plan.TargetEntered
                    && plan.Writes.IsEmpty
                    && plan.StopAtRawFileOffsetExclusive == 0x012C9354));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92EC, "8B CF", null, null),
            (0x012C92EE, "C1 E9 03", null, null),
            (0x012C92F1, "83 F9 20", null, null),
            (0x012C92F4, "73 5E", 0x012C9354, 0x016C9F54)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonTailSizeGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonTailSizeGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonTailSizeGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonTailSizeGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonTailSizeGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonTailSizeGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonTailSizeGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonTailSizeGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonTailSizeGateProjection.Instructions
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
