using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateFieldSelectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/VA window, branch targets, and instruction bytes match",
            CapacityFinalCalleeAlternateFieldSelectionProjection.EntryRawFileOffset
                == 0x012C8FA8
            && CapacityFinalCalleeAlternateFieldSelectionProjection.EntryVirtualAddress
                == 0x016C9BA8
            && CapacityFinalCalleeAlternateFieldSelectionProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8FC2
            && CapacityFinalCalleeAlternateFieldSelectionProjection.FirstNonzeroDestinationRawFileOffset
                == 0x012C8FC2
            && CapacityFinalCalleeAlternateFieldSelectionProjection.SecondNonzeroDestinationRawFileOffset
                == 0x012C8FC2
            && CapacityFinalCalleeAlternateFieldSelectionProjection.SecondZeroDestinationRawFileOffset
                == 0x012C8FFE
            && CapacityFinalCalleeAlternateFieldSelectionProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFieldSelectionProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateFieldSelectionProjection.EntryVirtualAddress
            && Normalize(
                CapacityFinalCalleeAlternateFieldSelectionProjection.CapturedWindowBytes)
                == "8B46148D4E14894DFC85C0750D8B46108D4E10894DFC85C0743C"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateFieldSelectionPlan missingEsi =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: null,
                entryEbp: 0,
                freshEsiPlus14: 0,
                freshEsiPlus10: 0));
        check("Missing first-needed ESI leaves both reads and all writes unreached",
            missingEsi.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingEntryESI
            && !missingEsi.EntryESICaptured
            && missingEsi.FreshEsiPlus14Address is null
            && !missingEsi.FreshEsiPlus14ReadReached
            && !missingEsi.FreshEsiPlus14Captured
            && missingEsi.UnreachedFreshEsiPlus14CaptureProvided
            && missingEsi.UnreachedEntryEBPCaptureProvided
            && missingEsi.UnreachedFreshEsiPlus10CaptureProvided
            && !missingEsi.FirstLEAReached
            && !missingEsi.FirstWriteReached
            && !missingEsi.FirstTestReached
            && !missingEsi.FirstBranchReached
            && !missingEsi.FreshEsiPlus10ReadReached
            && !missingEsi.SecondWriteReached
            && !missingEsi.SecondBranchReached
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C8FA8);

        CapacityFinalCalleeAlternateFieldSelectionPlan missingFirstRead =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0xFFFFFFEC,
                entryEbp: 0x100,
                freshEsiPlus14: null,
                freshEsiPlus10: 0));
        check("A missing fresh ESI+14 DWORD stops before LEA and leaves EBP and the second read unreached",
            missingFirstRead.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingFreshEsiPlus14
            && missingFirstRead.EntryESICaptured
            && missingFirstRead.FreshEsiPlus14Address == 0
            && missingFirstRead.FreshEsiPlus14ReadReached
            && !missingFirstRead.FreshEsiPlus14Captured
            && !missingFirstRead.FirstLEAReached
            && !missingFirstRead.FirstWriteReached
            && !missingFirstRead.FirstTestReached
            && missingFirstRead.UnreachedEntryEBPCaptureProvided
            && missingFirstRead.UnreachedFreshEsiPlus10CaptureProvided
            && missingFirstRead.StopAtRawFileOffsetExclusive == 0x012C8FA8);

        CapacityFinalCalleeAlternateFieldSelectionPlan missingEbp =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0xFFFFFFEC,
                entryEbp: null,
                freshEsiPlus14: 0x25,
                freshEsiPlus10: 0));
        check("A missing EBP blocks the first store after preserving the fresh EAX and LEA result",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingEntryEBP
            && missingEbp.EntryESICaptured
            && missingEbp.FreshEsiPlus14Captured
            && missingEbp.FirstEAX == 0x25
            && missingEbp.FirstLEAReached
            && missingEbp.FirstECX == 0
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FirstWriteReached
            && missingEbp.FirstWriteAddress is null
            && !missingEbp.FirstTestReached
            && !missingEbp.FreshEsiPlus10ReadReached
            && missingEbp.UnreachedFreshEsiPlus10CaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C8FAE);

        CapacityFinalCalleeAlternateFieldSelectionPlan firstNonzero =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0x100,
                entryEbp: 0x200,
                freshEsiPlus14: 1,
                freshEsiPlus10: 0));
        check("A nonzero first fresh EAX takes the exact FC2 route without following it",
            firstNonzero.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .FirstFreshEsiPlus14NonzeroToBoundary
            && firstNonzero.FreshEsiPlus14Address == 0x114
            && firstNonzero.FreshEsiPlus14Captured
            && firstNonzero.FirstEAX == 1
            && firstNonzero.FirstECX == 0x114
            && firstNonzero.FirstWriteReached
            && firstNonzero.FirstWriteAddress == 0x1FC
            && firstNonzero.FirstWriteValue == 0x114
            && firstNonzero.FirstTestReached
            && firstNonzero.FirstTestIsZero == false
            && firstNonzero.FirstBranchReached
            && firstNonzero.FirstBranchTaken == true
            && firstNonzero.FirstBranchSelectedDestinationRawFileOffset == 0x012C8FC2
            && !firstNonzero.FreshEsiPlus10ReadReached
            && firstNonzero.UnreachedFreshEsiPlus10CaptureProvided
            && !firstNonzero.SecondLEAReached
            && !firstNonzero.SecondWriteReached
            && !firstNonzero.SecondBranchReached
            && firstNonzero.StopAtRawFileOffsetExclusive == 0x012C8FC2);

        CapacityFinalCalleeAlternateFieldSelectionPlan missingSecondRead =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0x100,
                entryEbp: 0x200,
                freshEsiPlus14: 0,
                freshEsiPlus10: null));
        check("A missing second fresh read retains the completed first write and zero-test route",
            missingSecondRead.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingFreshEsiPlus10
            && missingSecondRead.FirstWriteReached
            && missingSecondRead.FirstWriteAddress == 0x1FC
            && missingSecondRead.FirstWriteValue == 0x114
            && missingSecondRead.FirstTestReached
            && missingSecondRead.FirstTestIsZero == true
            && missingSecondRead.FirstBranchReached
            && missingSecondRead.FirstBranchTaken == false
            && missingSecondRead.FirstBranchSelectedDestinationRawFileOffset == 0x012C8FB5
            && missingSecondRead.FreshEsiPlus10Address == 0x110
            && missingSecondRead.FreshEsiPlus10ReadReached
            && !missingSecondRead.FreshEsiPlus10Captured
            && !missingSecondRead.SecondLEAReached
            && !missingSecondRead.SecondWriteReached
            && !missingSecondRead.SecondTestReached
            && missingSecondRead.StopAtRawFileOffsetExclusive == 0x012C8FB5);

        CapacityFinalCalleeAlternateFieldSelectionPlan secondZero =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0x100,
                entryEbp: 0x200,
                freshEsiPlus14: 0,
                freshEsiPlus10: 0));
        check("A captured second zero stores ESI+10 and branches to exact FFE",
            secondZero.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10ZeroToTarget
            && secondZero.FreshEsiPlus10Address == 0x110
            && secondZero.FreshEsiPlus10ReadReached
            && secondZero.FreshEsiPlus10Captured
            && secondZero.SecondEAX == 0
            && secondZero.SecondLEAReached
            && secondZero.SecondECX == 0x110
            && secondZero.SecondWriteReached
            && secondZero.SecondWriteAddress == 0x1FC
            && secondZero.SecondWriteValue == 0x110
            && secondZero.SecondTestReached
            && secondZero.SecondTestIsZero == true
            && secondZero.SecondBranchReached
            && secondZero.SecondBranchTaken == true
            && secondZero.SecondBranchSelectedDestinationRawFileOffset == 0x012C8FFE
            && secondZero.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeAlternateFieldSelectionPlan secondNonzero =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0x100,
                entryEbp: 0x200,
                freshEsiPlus14: 0,
                freshEsiPlus10: 0xFFFFFFFF));
        check("A captured second nonzero falls through to exact FC2",
            secondNonzero.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10NonzeroToBoundary
            && secondNonzero.SecondEAX == 0xFFFFFFFF
            && secondNonzero.SecondTestReached
            && secondNonzero.SecondTestIsZero == false
            && secondNonzero.SecondBranchReached
            && secondNonzero.SecondBranchTaken == false
            && secondNonzero.SecondBranchSelectedDestinationRawFileOffset == 0x012C8FC2
            && secondNonzero.StopAtRawFileOffsetExclusive == 0x012C8FC2);

        CapacityFinalCalleeAlternateFieldSelectionPlan zeros =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(Capture(
                entryEsi: 0,
                entryEbp: 0,
                freshEsiPlus14: 0,
                freshEsiPlus10: 0));
        check("Captured zero values remain distinct from missing registers and reads",
            zeros.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome
                    .SecondFreshEsiPlus10ZeroToTarget
            && zeros.EntryESICaptured
            && zeros.FreshEsiPlus14Address == 0x14
            && zeros.FreshEsiPlus14Captured
            && zeros.FirstEAX == 0
            && zeros.FirstECX == 0x14
            && zeros.EntryEBPCaptured
            && zeros.FirstWriteAddress == 0xFFFFFFFC
            && zeros.FirstWriteValue == 0x14
            && zeros.FreshEsiPlus10Address == 0x10
            && zeros.FreshEsiPlus10Captured
            && zeros.SecondEAX == 0
            && zeros.SecondECX == 0x10
            && zeros.SecondWriteAddress == 0xFFFFFFFC
            && zeros.SecondWriteValue == 0x10
            && zeros.SecondBranchTaken == true
            && zeros.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        check("LEA offsets and EBP-4 use wrapping DWORD arithmetic",
            CapacityFinalCalleeAlternateFieldSelectionProjection.DeriveEsiPlus14Address(
                0xFFFFFFEC) == 0
            && CapacityFinalCalleeAlternateFieldSelectionProjection.DeriveEsiPlus10Address(
                0xFFFFFFF0) == 0
            && CapacityFinalCalleeAlternateFieldSelectionProjection.DeriveEbpMinus4Address(
                0) == 0xFFFFFFFC);
    }

    private static CapacityFinalCalleeAlternateFieldSelectionCapture Capture(
        uint? entryEsi = 0x1000,
        uint? entryEbp = 0x2000,
        uint? freshEsiPlus14 = 1,
        uint? freshEsiPlus10 = 2) =>
        new(entryEsi, entryEbp, freshEsiPlus14, freshEsiPlus10);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8FA8, "8B 46 14", null, null),
            (0x012C8FAB, "8D 4E 14", null, null),
            (0x012C8FAE, "89 4D FC", null, null),
            (0x012C8FB1, "85 C0", null, null),
            (0x012C8FB3, "75 0D", 0x012C8FC2, 0x016C9BC2),
            (0x012C8FB5, "8B 46 10", null, null),
            (0x012C8FB8, "8D 4E 10", null, null),
            (0x012C8FBB, "89 4D FC", null, null),
            (0x012C8FBE, "85 C0", null, null),
            (0x012C8FC0, "74 3C", 0x012C8FFE, 0x016C9BFE)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateFieldSelectionProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateFieldSelectionInstructionEvidence instruction
            in CapacityFinalCalleeAlternateFieldSelectionProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateFieldSelectionProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateFieldSelectionProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateFieldSelectionProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateFieldSelectionProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateFieldSelectionProjection.Instructions
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
