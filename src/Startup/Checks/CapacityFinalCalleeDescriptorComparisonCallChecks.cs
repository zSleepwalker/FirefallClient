using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescriptorComparisonCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The descriptor-comparison bytes, branch targets, and call-cell operand match",
            CapacityFinalCalleeDescriptorComparisonCallProjection.EntryRawFileOffset
                == 0x012C8ED5
            && CapacityFinalCalleeDescriptorComparisonCallProjection.EntryVirtualAddress
                == 0x016C9AD5
            && CapacityFinalCalleeDescriptorComparisonCallProjection.FirstEqualityBranchRawFileOffset
                == 0x012C8ED8
            && CapacityFinalCalleeDescriptorComparisonCallProjection.UnsignedCompareRawFileOffset
                == 0x012C8EDA
            && CapacityFinalCalleeDescriptorComparisonCallProjection.UnsignedBelowBranchRawFileOffset
                == 0x012C8EDC
            && CapacityFinalCalleeDescriptorComparisonCallProjection.MemoryCompareRawFileOffset
                == 0x012C8EDE
            && CapacityFinalCalleeDescriptorComparisonCallProjection.MemoryEqualityBranchRawFileOffset
                == 0x012C8EE1
            && CapacityFinalCalleeDescriptorComparisonCallProjection.CallRawFileOffset
                == 0x012C8EE3
            && CapacityFinalCalleeDescriptorComparisonCallProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8EE9
            && CapacityFinalCalleeDescriptorComparisonCallProjection.BoundaryVirtualAddress
                == 0x016C9AE9
            && CapacityFinalCalleeDescriptorComparisonCallProjection.CallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeDescriptorComparisonCallProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeDescriptorComparisonCallProjection.EntryRawFileOffset)
                == CapacityFinalCalleeDescriptorComparisonCallProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeDescriptorComparisonCallProjection.CapturedWindowBytes)
                == "3B45F0740F3BC372053970087406FF15244AAC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeDescriptorComparisonCallPlan invalidEntry =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                captureValidatedEntry: false,
                freshFrameMinus10: 0,
                freshEaxPlus8: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Unvalidated entry blocks the comparisons and marks all later captures unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.EntryNotValidated
            && !invalidEntry.FreshFrameMinus10ReadReached
            && !invalidEntry.FreshEaxPlus8ReadReached
            && !invalidEntry.FirstEqualityBranchReached
            && !invalidEntry.UnsignedBelowBranchReached
            && !invalidEntry.MemoryEqualityBranchReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedFreshFrameMinus10CaptureProvided
            && invalidEntry.UnreachedFreshEaxPlus8CaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorComparisonCallPlan missingFrame =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                freshFrameMinus10: null,
                freshEaxPlus8: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Missing first fresh read blocks every later comparison and call",
            missingFrame.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingFreshFrameMinus10
            && missingFrame.FreshFrameMinus10Address == 0x6FF0
            && missingFrame.FreshFrameMinus10ReadReached
            && !missingFrame.FreshFrameMinus10Captured
            && missingFrame.FirstCompareResult is null
            && !missingFrame.UnsignedBelowBranchReached
            && !missingFrame.FreshEaxPlus8ReadReached
            && !missingFrame.CallCellReadReached
            && missingFrame.UnreachedFreshEaxPlus8CaptureProvided
            && missingFrame.UnreachedCallCellTargetCaptureProvided
            && missingFrame.UnreachedCallReturnStatusProvided
            && missingFrame.StopAtRawFileOffsetExclusive == 0x012C8ED5);

        CapacityFinalCalleeDescriptorComparisonCallPlan firstEqual =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0,
                freshFrameMinus10: 0,
                freshEaxPlus8: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("First CMP/JZ equality skips every later instruction, including the unsigned compare",
            firstEqual.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.FirstComparisonEqualCallSkipped
            && firstEqual.FreshFrameMinus10Captured
            && firstEqual.FreshFrameMinus10 == 0
            && firstEqual.FirstCompareResult == 0
            && firstEqual.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && firstEqual.FirstCompareEqual == true
            && firstEqual.FirstEqualityBranchReached
            && firstEqual.FirstEqualityBranchTaken == true
            && firstEqual.FirstEqualityBranchDestinationRawFileOffset == 0x012C8EE9
            && firstEqual.UnsignedCompareResult is null
            && !firstEqual.UnsignedBelowBranchReached
            && !firstEqual.FreshEaxPlus8ReadReached
            && !firstEqual.CallCellReadReached
            && firstEqual.UnreachedFreshEaxPlus8CaptureProvided
            && firstEqual.UnreachedCallCellTargetCaptureProvided
            && firstEqual.UnreachedCallReturnStatusProvided
            && firstEqual.StopAtRawFileOffsetExclusive == 0x012C8EE9);

        CapacityFinalCalleeDescriptorComparisonCallPlan unsignedBelow =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0x10000000,
                entryEbx: 0xF0000000,
                freshFrameMinus10: 0,
                freshEaxPlus8: 0x1234,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Unsigned-below branch calls directly without reaching the EAX+8 read",
            unsignedBelow.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallReturnedNormally
            && unsignedBelow.FirstCompareEqual == false
            && unsignedBelow.UnsignedCompareResult == 0x20000000
            && unsignedBelow.UnsignedCompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: false
            }
            && unsignedBelow.UnsignedBelow == true
            && unsignedBelow.UnsignedBelowBranchReached
            && unsignedBelow.UnsignedBelowBranchTaken == true
            && unsignedBelow.UnsignedBelowBranchDestinationRawFileOffset == 0x012C8EE3
            && !unsignedBelow.FreshEaxPlus8ReadReached
            && !unsignedBelow.FreshEaxPlus8Captured
            && unsignedBelow.UnreachedFreshEaxPlus8CaptureProvided
            && unsignedBelow.CallReached
            && unsignedBelow.CallReturnedNormally == true
            && unsignedBelow.CallOutputsOpaque
            && unsignedBelow.PreCallEAX == 0x10000000
            && unsignedBelow.PreCallEBX == 0xF0000000
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8EE9);

        CapacityFinalCalleeDescriptorComparisonCallPlan highUnsignedNotBelow =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0xF0000000,
                entryEbx: 0x10000000,
                freshFrameMinus10: 0,
                freshEaxPlus8: 0,
                entryEsi: 1,
                callCellTarget: 0,
                callReturnedNormally: false));
        check("High-bit EAX uses unsigned CMP/JB semantics and captured zero remains valid",
            highUnsignedNotBelow.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallDidNotReturnNormally
            && highUnsignedNotBelow.UnsignedCompareResult == 0xE0000000
            && highUnsignedNotBelow.UnsignedCompareFlags is
            {
                Carry: false,
                Sign: true
            }
            && highUnsignedNotBelow.UnsignedBelow == false
            && highUnsignedNotBelow.UnsignedBelowBranchTaken == false
            && highUnsignedNotBelow.FreshEaxPlus8Address == 0xF0000008
            && highUnsignedNotBelow.FreshEaxPlus8Captured
            && highUnsignedNotBelow.FreshEaxPlus8 == 0
            && highUnsignedNotBelow.MemoryCompareResult == 0xFFFFFFFF
            && highUnsignedNotBelow.MemoryCompareEqual == false
            && highUnsignedNotBelow.CallReached
            && highUnsignedNotBelow.CallReturnedNormally == false
            && highUnsignedNotBelow.StopAtRawFileOffsetExclusive == 0x012C8EE3);

        CapacityFinalCalleeDescriptorComparisonCallPlan missingSecondRead =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0x100,
                entryEbx: 0x100,
                entryEsi: 0x10,
                freshFrameMinus10: 0x200,
                freshEaxPlus8: null,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Missing EAX+8 capture stops at its CMP after the preceding unsigned gate",
            missingSecondRead.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingFreshEaxPlus8
            && missingSecondRead.FirstCompareEqual == false
            && missingSecondRead.UnsignedBelow == false
            && missingSecondRead.UnsignedBelowBranchReached
            && missingSecondRead.UnsignedBelowBranchTaken == false
            && missingSecondRead.FreshEaxPlus8Address == 0x108
            && missingSecondRead.FreshEaxPlus8ReadReached
            && !missingSecondRead.FreshEaxPlus8Captured
            && missingSecondRead.MemoryCompareResult is null
            && !missingSecondRead.MemoryEqualityBranchReached
            && !missingSecondRead.CallCellReadReached
            && missingSecondRead.UnreachedCallCellTargetCaptureProvided
            && missingSecondRead.UnreachedCallReturnStatusProvided
            && missingSecondRead.StopAtRawFileOffsetExclusive == 0x012C8EDE);

        CapacityFinalCalleeDescriptorComparisonCallPlan memoryEqual =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0x100,
                entryEbx: 0x100,
                entryEsi: 0,
                freshFrameMinus10: 0x200,
                freshEaxPlus8: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("CMP [EAX+8],ESI equality takes the final JZ and skips the call",
            memoryEqual.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.MemoryComparisonEqualCallSkipped
            && memoryEqual.FreshEaxPlus8Address == 0x108
            && memoryEqual.FreshEaxPlus8Captured
            && memoryEqual.FreshEaxPlus8 == 0
            && memoryEqual.MemoryCompareResult == 0
            && memoryEqual.MemoryCompareFlags?.Zero == true
            && memoryEqual.MemoryCompareEqual == true
            && memoryEqual.MemoryEqualityBranchReached
            && memoryEqual.MemoryEqualityBranchTaken == true
            && memoryEqual.MemoryEqualityBranchDestinationRawFileOffset == 0x012C8EE9
            && !memoryEqual.CallCellReadReached
            && !memoryEqual.CallReached
            && memoryEqual.UnreachedCallCellTargetCaptureProvided
            && memoryEqual.UnreachedCallReturnStatusProvided
            && memoryEqual.StopAtRawFileOffsetExclusive == 0x012C8EE9);

        CapacityFinalCalleeDescriptorComparisonCallPlan unequalMemory =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                entryEax: 0xFFFFFFFC,
                entryEbx: 0,
                entryEsi: 0xFFFFFFFF,
                entryEbp: 8,
                freshFrameMinus10: 0,
                freshEaxPlus8: 0,
                entryEsp: 0x76543210,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Unequal memory comparison preserves operand order and DWORD address wrapping",
            unequalMemory.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallReturnedNormally
            && unequalMemory.FreshFrameMinus10Address == 0xFFFFFFF8
            && unequalMemory.FreshEaxPlus8Address == 4
            && unequalMemory.UnsignedBelow == false
            && unequalMemory.MemoryCompareResult == 1
            && unequalMemory.MemoryCompareFlags is
            {
                Carry: true,
                Zero: false
            }
            && unequalMemory.MemoryCompareEqual == false
            && unequalMemory.MemoryEqualityBranchTaken == false
            && unequalMemory.CallReached
            && unequalMemory.PreCallEAX == 0xFFFFFFFC
            && unequalMemory.PreCallEBP == 8
            && unequalMemory.PreCallESP == 0x76543210
            && unequalMemory.CallOutputsOpaque
            && unequalMemory.StopAtRawFileOffsetExclusive == 0x012C8EE9);

        CapacityFinalCalleeDescriptorComparisonCallPlan missingCallCell =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                freshEaxPlus8: 0x21,
                callCellTarget: null,
                callReturnedNormally: true));
        CapacityFinalCalleeDescriptorComparisonCallPlan missingReturnStatus =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                freshEaxPlus8: 0x21,
                callCellTarget: 0x1000,
                callReturnedNormally: null));
        CapacityFinalCalleeDescriptorComparisonCallPlan nonreturning =
            CapacityFinalCalleeDescriptorComparisonCallProjection.Project(Capture(
                freshEaxPlus8: 0x21,
                callCellTarget: 0,
                callReturnedNormally: false));
        check("Missing call target, missing status, and nonreturn stop distinctly at the call",
            missingCallCell.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingCallCellTarget
            && missingCallCell.CallCellReadReached
            && !missingCallCell.CallCellTargetCaptured
            && !missingCallCell.CallReached
            && missingCallCell.UnreachedCallReturnStatusProvided
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C8EE3
            && missingReturnStatus.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingCallReturnStatus
            && missingReturnStatus.CallCellTargetCaptured
            && missingReturnStatus.CallCellTargetVirtualAddress == 0x1000
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.CallOutputsOpaque
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C8EE3
            && nonreturning.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallDidNotReturnNormally
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.CallOutputsOpaque
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C8EE3);
    }

    private static CapacityFinalCalleeDescriptorComparisonCallCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEax = 0x10,
        uint entryEbx = 0x10,
        uint entryEsi = 0x10,
        uint entryEbp = 0x7000,
        uint entryEsp = 0x7000,
        uint? freshFrameMinus10 = 0x20,
        uint? freshEaxPlus8 = 0x20,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedEntry,
            entryEax,
            entryEbx,
            entryEsi,
            entryEbp,
            entryEsp,
            freshFrameMinus10,
            freshEaxPlus8,
            callCellTarget,
            callReturnedNormally);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C8ED5, "3B 45 F0", null, null, null),
            (0x012C8ED8, "74 0F", null, 0x012C8EE9, 0x016C9AE9),
            (0x012C8EDA, "3B C3", null, null, null),
            (0x012C8EDC, "72 05", null, 0x012C8EE3, 0x016C9AE3),
            (0x012C8EDE, "39 70 08", null, null, null),
            (0x012C8EE1, "74 06", null, 0x012C8EE9, 0x016C9AE9),
            (0x012C8EE3, "FF 15 24 4A AC 01", 0x01AC4A24, null, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeDescriptorComparisonCallProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeDescriptorComparisonCallInstructionEvidence instruction
            in CapacityFinalCalleeDescriptorComparisonCallProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeDescriptorComparisonCallProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeDescriptorComparisonCallProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeDescriptorComparisonCallProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeDescriptorComparisonCallProjection.CapturedWindowBytes)
            && CapacityFinalCalleeDescriptorComparisonCallProjection.Instructions
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
