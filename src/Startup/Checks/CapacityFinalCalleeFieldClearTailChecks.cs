using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFieldClearTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, PE mapping, operands, and unsigned branch target match",
            CapacityFinalCalleeFieldClearTailProjection.EntryRawFileOffset
                == 0x012C8FDD
            && CapacityFinalCalleeFieldClearTailProjection.EntryVirtualAddress
                == 0x016C9BDD
            && CapacityFinalCalleeFieldClearTailProjection.WindowBoundaryRawFileOffsetExclusive
                == 0x012C8FFE
            && CapacityFinalCalleeFieldClearTailProjection.BelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeFieldClearTailProjection.GlobalValueVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeFieldClearTailProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFieldClearTailProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFieldClearTailProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeFieldClearTailProjection.CapturedWindowBytes)
                == "3B15D8ADED018955FC8B55F40F8287FEFFFF8B4DFCC701000000008B1DD8ADED01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeFieldClearTailPlan missingEntryEDX =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: null));
        check("A missing entry EDX blocks the first compare and leaves supplied later captures unreached",
            missingEntryEDX.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.MissingValidatedEntryEDX
            && missingEntryEDX.Reads.IsEmpty
            && missingEntryEDX.Writes.IsEmpty
            && !missingEntryEDX.CompareReached
            && !missingEntryEDX.BranchReached
            && missingEntryEDX.UnreachedValidatedEntryEBPCaptureProvided
            && missingEntryEDX.UnreachedFreshGlobalForEntryCompareCaptureProvided
            && missingEntryEDX.UnreachedFreshFrameMinus12CaptureProvided
            && missingEntryEDX.UnreachedFreshFrameMinus4CaptureProvided
            && missingEntryEDX.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingEntryEDX.StopAtRawFileOffsetExclusive == 0x012C8FDD);

        CapacityFinalCalleeFieldClearTailPlan missingCompareGlobal =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                freshGlobalForEntryCompare: null));
        check("A missing first global read is distinct from a captured zero and precedes every write",
            missingCompareGlobal.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshGlobalForEntryCompare
            && missingCompareGlobal.CompareGlobalReadReached
            && !missingCompareGlobal.CompareGlobalCaptured
            && missingCompareGlobal.CompareGlobalValue is null
            && missingCompareGlobal.Reads.Length == 1
            && missingCompareGlobal.Reads[0].Address == 0x01EDADD8
            && !missingCompareGlobal.Reads[0].ValueCaptured
            && missingCompareGlobal.Writes.IsEmpty
            && missingCompareGlobal.UnreachedFreshFrameMinus12CaptureProvided
            && missingCompareGlobal.UnreachedFreshFrameMinus4CaptureProvided
            && missingCompareGlobal.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingCompareGlobal.StopAtRawFileOffsetExclusive == 0x012C8FDD);

        CapacityFinalCalleeFieldClearTailPlan missingEBP =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0xFFFFFFFF,
                validatedEntryEBP: null,
                freshGlobalForEntryCompare: 0));
        check("CMP completes before the missing EBP is needed by the first frame write",
            missingEBP.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.MissingValidatedEntryEBP
            && missingEBP.CompareGlobalCaptured
            && missingEBP.CompareGlobalValue == 0
            && missingEBP.CompareReached
            && missingEBP.CompareResult == 0xFFFFFFFF
            && missingEBP.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && missingEBP.Writes.IsEmpty
            && missingEBP.UnreachedFreshFrameMinus12CaptureProvided
            && missingEBP.UnreachedFreshFrameMinus4CaptureProvided
            && missingEBP.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C8FE3);

        CapacityFinalCalleeFieldClearTailPlan missingFrameMinus12 =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0x7FFFFFFF,
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: null));
        check("The original EDX is stored before the independent EBP-minus-12 reload can fail",
            missingFrameMinus12.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.MissingFreshFrameMinus12
            && missingFrameMinus12.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && missingFrameMinus12.FrameMinus12Address == 0x0FF4
            && missingFrameMinus12.FrameMinus12ReadReached
            && !missingFrameMinus12.FrameMinus12Captured
            && !missingFrameMinus12.BranchReached
            && missingFrameMinus12.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x0FFC, 0x7FFFFFFF)
            })
            && missingFrameMinus12.UnreachedFreshFrameMinus4CaptureProvided
            && missingFrameMinus12.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingFrameMinus12.StopAtRawFileOffsetExclusive == 0x012C8FE6);

        CapacityFinalCalleeFieldClearTailPlan unsignedBelow =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0x7FFFFFFF,
                validatedEntryEBP: 0x1000,
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: 0xFFFFFFFF));
        check("JB uses the original unsigned CMP operands and retains the preceding store and reload",
            unsignedBelow.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.EntryEDXUnsignedBelowGlobal
            && unsignedBelow.CompareResult == 0xFFFFFFFF
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.BranchReached
            && unsignedBelow.BranchTaken == true
            && unsignedBelow.SelectedBranchDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.FrameMinus12Captured
            && unsignedBelow.FrameMinus12 == 0xFFFFFFFF
            && unsignedBelow.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x0FFC, 0x7FFFFFFF)
            })
            && !unsignedBelow.FrameMinus4ReadReached
            && !unsignedBelow.TailGlobalReadReached
            && unsignedBelow.UnreachedFreshFrameMinus4CaptureProvided
            && unsignedBelow.UnreachedFreshGlobalForTailLoadCaptureProvided
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeFieldClearTailPlan unsignedEqual =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0x80000000,
                freshGlobalForEntryCompare: 0x80000000));
        check("Unsigned equality is not below and preserves the exact CMP flags through MOVs",
            unsignedEqual.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
            && unsignedEqual.CompareResult == 0
            && unsignedEqual.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && unsignedEqual.BranchReached
            && unsignedEqual.BranchTaken == false
            && unsignedEqual.SelectedBranchDestinationRawFileOffset == 0x012C8FEF
            && unsignedEqual.FrameMinus12 == 0x1234
            && unsignedEqual.Writes.Length == 2);

        CapacityFinalCalleeFieldClearTailPlan unsignedHighBit =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0x80000000,
                validatedEntryEBP: 8,
                freshGlobalForEntryCompare: 0x7FFFFFFF,
                freshFrameMinus12: 0x12345678,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("A high-bit entry EDX above the global value falls through with wrapped frame addresses",
            unsignedHighBit.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
            && unsignedHighBit.CompareResult == 1
            && unsignedHighBit.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && unsignedHighBit.BranchTaken == false
            && unsignedHighBit.FrameMinus12Address == 0xFFFFFFFC
            && unsignedHighBit.FrameMinus4Address == 4
            && unsignedHighBit.FrameMinus4ForZeroStoreAddress == 0
            && unsignedHighBit.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 4, 0x80000000),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0, 0)
            })
            && unsignedHighBit.TailGlobalReadReached
            && unsignedHighBit.TailGlobalCaptured
            && unsignedHighBit.TailGlobalValue == 0
            && unsignedHighBit.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeFieldClearTailPlan missingFrameMinus4 =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 7,
                freshGlobalForEntryCompare: 7,
                freshFrameMinus12: 0xDEADBEEF,
                freshFrameMinus4ForZeroStoreAddress: null));
        check("The fresh EBP-minus-4 read is independent of its prior write and can stop after that write",
            missingFrameMinus4.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshFrameMinus4ForZeroStoreAddress
            && missingFrameMinus4.FrameMinus4Address == 0x0FFC
            && missingFrameMinus4.FrameMinus4ReadReached
            && !missingFrameMinus4.FrameMinus4Captured
            && missingFrameMinus4.FrameMinus4ForZeroStoreAddress is null
            && missingFrameMinus4.FrameMinus12 == 0xDEADBEEF
            && missingFrameMinus4.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x0FFC, 7)
            })
            && missingFrameMinus4.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x012C8FEF);

        CapacityFinalCalleeFieldClearTailPlan missingTailGlobal =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 9,
                freshGlobalForEntryCompare: 9,
                freshFrameMinus4ForZeroStoreAddress: 0x01EDADD8,
                freshGlobalForTailLoad: null));
        check("A missing tail global reload retains both ordered writes and the fresh frame value",
            missingTailGlobal.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshGlobalForTailLoad
            && missingTailGlobal.FrameMinus4Captured
            && missingTailGlobal.FrameMinus4ForZeroStoreAddress == 0x01EDADD8
            && missingTailGlobal.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x0FFC, 9),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0x01EDADD8, 0)
            })
            && missingTailGlobal.TailGlobalReadReached
            && !missingTailGlobal.TailGlobalCaptured
            && missingTailGlobal.TailGlobalValue is null
            && missingTailGlobal.Reads[^1].RawFileOffset == 0x012C8FF8
            && missingTailGlobal.Reads[^1].Address == 0x01EDADD8
            && !missingTailGlobal.Reads[^1].ValueCaptured
            && missingTailGlobal.StopAtRawFileOffsetExclusive == 0x012C8FF8);

        CapacityFinalCalleeFieldClearTailPlan zeroAliasTailRead =
            CapacityFinalCalleeFieldClearTailProjection.Project(Capture(
                validatedEntryEDX: 0xFFFFFFFF,
                validatedEntryEBP: 0,
                freshGlobalForEntryCompare: 0xFFFFFFFF,
                freshFrameMinus12: 0,
                freshFrameMinus4ForZeroStoreAddress: 0x01EDADD8,
                freshGlobalForTailLoad: 0));
        check("The post-store global read is still fresh and reached when the zero store aliases it",
            zeroAliasTailRead.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
            && zeroAliasTailRead.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0xFFFFFFFC, 0xFFFFFFFF),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0x01EDADD8, 0)
            })
            && zeroAliasTailRead.Reads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C8FDD, 0x012C8FE6, 0x012C8FEF, 0x012C8FF8
                })
            && zeroAliasTailRead.Reads[^1].Address == 0x01EDADD8
            && zeroAliasTailRead.Reads[^1].ValueCaptured
            && zeroAliasTailRead.Reads[^1].Value == 0
            && zeroAliasTailRead.TailGlobalReadReached
            && zeroAliasTailRead.TailGlobalCaptured
            && zeroAliasTailRead.TailGlobalValue == 0);
    }

    private static CapacityFinalCalleeFieldClearTailCapture Capture(
        uint? validatedEntryEDX = 0x12345678,
        uint? validatedEntryEBP = 0x1000,
        uint? freshGlobalForEntryCompare = 0,
        uint? freshFrameMinus12 = 0x1234,
        uint? freshFrameMinus4ForZeroStoreAddress = 0,
        uint? freshGlobalForTailLoad = 0xFFFFFFFF) =>
        new(
            validatedEntryEDX,
            validatedEntryEBP,
            freshGlobalForEntryCompare,
            freshFrameMinus12,
            freshFrameMinus4ForZeroStoreAddress,
            freshGlobalForTailLoad);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw)[]
        {
            (0x012C8FDD, "3B 15 D8 AD ED 01", 0x01EDADD8, null),
            (0x012C8FE3, "89 55 FC", null, null),
            (0x012C8FE6, "8B 55 F4", null, null),
            (0x012C8FE9, "0F 82 87 FE FF FF", null, 0x012C8E76),
            (0x012C8FEF, "8B 4D FC", null, null),
            (0x012C8FF2, "C7 01 00 00 00 00", null, null),
            (0x012C8FF8, "8B 1D D8 AD ED 01", 0x01EDADD8, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeFieldClearTailProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFieldClearTailInstructionEvidence instruction
            in CapacityFinalCalleeFieldClearTailProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFieldClearTailProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeFieldClearTailProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFieldClearTailProjection
                    .WindowBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFieldClearTailProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFieldClearTailProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
