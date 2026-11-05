using System;
using System.Linq;
using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightFieldThresholdLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightFieldThresholdLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed instruction evidence preserves both bounded gate windows",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan missingEntry =
            Projection.Project(new(
                EntryEDX: null,
                FreshEDXPlus14: 0x12345678,
                FreshGlobalValue: 0x10));
        check("Missing entry EDX preserves the unreached parent read and contaminates late input",
            missingEntry.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingParentPrerequisiteWithUnusedFreshGlobalValue
            && missingEntry.RightFieldGate.Outcome
                == CapacityFinalCalleeLargeRightFieldGateOutcome.MissingEntryEDXForFreshRead
            && missingEntry.RightFieldGate.FreshReadInstructionReached
            && !missingEntry.RightFieldGate.FreshReadAttempted
            && missingEntry.RightFieldGate.ReadAttempts.IsEmpty
            && missingEntry.ThresholdGate is null
            && missingEntry.UnusedFreshGlobalValueCaptureProvided
            && missingEntry.Reads.IsEmpty
            && missingEntry.FlagsAtStop is null
            && !missingEntry.ExactCleanExitAdmitted
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan missingParentRead =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: null,
                FreshGlobalValue: null));
        check("A missing fresh parent value retains its attempted address and stops before TEST",
            missingParentRead.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingFreshEDXPlus14
            && missingParentRead.RightFieldGate.ReadAttempts.Length == 1
            && missingParentRead.RightFieldGate.ReadAttempts[0] is
            {
                Address: 0x214,
                AddressCaptured: true,
                Value: null,
                ValueCaptured: false,
                Attempted: true
            }
            && missingParentRead.Reads.Length == 1
            && missingParentRead.Reads[0].Stage
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate
            && !missingParentRead.RightFieldGate.TestReached
            && missingParentRead.FlagsAtStop is null
            && missingParentRead.ThresholdGate is null
            && !missingParentRead.ExactCleanExitAdmitted
            && missingParentRead.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan zero =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null));
        check("A zero TEST result bypasses the threshold and admits the exact 92BD exit",
            zero.Outcome == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD
            && zero.RightFieldGate.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.RightFieldGate.JzReached
            && zero.RightFieldGate.JzTaken == true
            && zero.RightFieldGate.JzSelectedDestinationRawFileOffset == 0x012C92BD
            && zero.ThresholdGate is null
            && zero.Reads.Length == 1
            && !zero.ThresholdGateReached
            && zero.BranchReached
            && zero.BranchTaken == true
            && zero.BranchSelectedDestinationRawFileOffset == 0x012C92BD
            && !zero.RightFieldBranchTargetEntered
            && !zero.RightFieldFallthroughEntered
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan zeroWithUnusedGlobal =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: 0x20));
        check("A supplied global value on the zero bypass is explicitly unused and contaminating",
            zeroWithUnusedGlobal.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .ZeroTo92BDWithUnusedFreshGlobalValue
            && zeroWithUnusedGlobal.UnusedFreshGlobalValueCaptureProvided
            && zeroWithUnusedGlobal.ThresholdGate is null
            && zeroWithUnusedGlobal.RightFieldGate.ExactCleanCompletion
            && !zeroWithUnusedGlobal.ExactCleanExitAdmitted
            && zeroWithUnusedGlobal.FlagsAtStop
                == zeroWithUnusedGlobal.ParentTestFlags
            && zeroWithUnusedGlobal.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan missingGlobal =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0x80000001,
                FreshGlobalValue: null));
        check("A missing independent global read preserves parent TEST flags at the child stop",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.ExactCleanBranchToThresholdGate
            && missingGlobal.ThresholdGateReached
            && missingGlobal.ThresholdGate?.Capture.EntryECX == 0x80000001
            && missingGlobal.ThresholdGate?.GlobalReadReached == true
            && missingGlobal.ThresholdGate?.GlobalAddress == 0x01EDADD8
            && missingGlobal.ThresholdGate?.CompareReached == false
            && missingGlobal.ParentTestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && missingGlobal.ThresholdCompareFlags is null
            && missingGlobal.ParentTestFlagsPreservedAtStop
            && missingGlobal.BranchReached
            && missingGlobal.BranchTaken == false
            && missingGlobal.BranchSelectedDestinationRawFileOffset == 0x012C92AB
            && missingGlobal.Reads.Length == 2
            && missingGlobal.Reads[0].Stage
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate
            && missingGlobal.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                RawFileOffset: 0x012C92AB,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C92AB);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan below =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 1,
                FreshGlobalValue: 2));
        check("A nonzero handoff compares the fresh ECX against the independent global value",
            below.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && below.ThresholdGate?.Capture.EntryECX == below.RightFieldGate.FreshECX
            && below.ThresholdGate?.Capture.FreshGlobalValue == 2
            && below.ThresholdGate?.CompareLeft == 1
            && below.ThresholdGate?.CompareRight == 2
            && below.ThresholdGate?.CompareResult == 0xFFFFFFFF
            && below.ExactCleanBranchToThresholdGate
            && below.RightFieldFallthroughEntered
            && below.ExactCleanExitAdmitted
            && below.BranchReached
            && below.BranchTaken == true
            && below.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !below.ThresholdBranchTargetEntered
            && !below.ThresholdFallthroughEntered
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan notBelow =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0x80000001,
                FreshGlobalValue: 0x80000001));
        check("The child CMP replaces TEST flags only after both operands are captured",
            notBelow.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && notBelow.ParentTestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && notBelow.ThresholdCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && notBelow.FlagsAtStop == notBelow.ThresholdCompareFlags
            && !notBelow.ParentTestFlagsPreservedAtStop
            && notBelow.BranchFlags == notBelow.ThresholdCompareFlags
            && notBelow.BranchTaken == false
            && notBelow.BranchSelectedDestinationRawFileOffset == 0x012C92B7
            && notBelow.ExactCleanExitAdmitted
            && !notBelow.ThresholdBranchTargetEntered
            && !notBelow.ThresholdFallthroughEntered
            && notBelow.StopAtRawFileOffsetExclusive == 0x012C92B7);

        const uint aliasedAddress = 0x01EDADD8;
        uint entryEDX = unchecked(aliasedAddress - 0x14);
        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan aliasedDistinctValues =
            Projection.Project(new(
                EntryEDX: entryEDX,
                FreshEDXPlus14: 0x40,
                FreshGlobalValue: 0x20));
        check("Aliased addresses retain ordered independent read observations without forwarding",
            aliasedDistinctValues.RightFieldGate.FreshReadAddress == aliasedAddress
            && aliasedDistinctValues.ThresholdGate?.GlobalAddress == aliasedAddress
            && aliasedDistinctValues.Reads.Length == 2
            && aliasedDistinctValues.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                Address: aliasedAddress,
                Value: 0x40,
                ValueCaptured: true
            }
            && aliasedDistinctValues.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                Address: aliasedAddress,
                Value: 0x20,
                ValueCaptured: true
            }
            && aliasedDistinctValues.ThresholdGate?.Capture.EntryECX == 0x40
            && aliasedDistinctValues.ThresholdGate?.Capture.FreshGlobalValue == 0x20
            && aliasedDistinctValues.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && aliasedDistinctValues.BranchSelectedDestinationRawFileOffset == 0x012C92B7);

        CapacityFinalCalleeLargeRightFieldThresholdLifecyclePlan aliasedReversedValues =
            Projection.Project(new(
                EntryEDX: entryEDX,
                FreshEDXPlus14: 0x20,
                FreshGlobalValue: 0x40));
        check("Aliased fresh reads can independently select the opposite unsigned threshold path",
            aliasedReversedValues.Reads.Length == 2
            && aliasedReversedValues.Reads[0].Address
                == aliasedReversedValues.Reads[1].Address
            && aliasedReversedValues.Reads[0].Value == 0x20
            && aliasedReversedValues.Reads[1].Value == 0x40
            && aliasedReversedValues.ThresholdGate?.CompareLeft == 0x20
            && aliasedReversedValues.ThresholdGate?.CompareRight == 0x40
            && aliasedReversedValues.Outcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && aliasedReversedValues.BranchSelectedDestinationRawFileOffset == 0x012C8E76);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92A4, "8B 4A 14", null, null),
            (0x012C92A7, "85 C9", null, null),
            (0x012C92A9, "74 12", 0x012C92BD, 0x016C9EBD),
            (0x012C92AB, "3B 0D D8 AD ED 01", null, null),
            (0x012C92B1, "0F 82 BF FB FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightFieldThresholdLifecycleInstructionEvidence instruction
            in Projection.Instructions)
        {
            byte[] instructionBytes = Convert.FromHexString(Normalize(instruction.Bytes));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != Projection.RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != Projection.RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint branchTarget)
            {
                int displacement = instructionBytes.Length == 2
                    ? unchecked((sbyte)instructionBytes[1])
                    : BinaryPrimitives.ReadInt32LittleEndian(
                        instructionBytes.AsSpan(instructionBytes.Length - 4, 4));
                if ((long)instruction.RawFileOffset + instructionBytes.Length + displacement
                    != branchTarget)
                {
                    return false;
                }
            }

            nextRawOffset += (uint)instructionBytes.Length;
            bytes += Convert.ToHexString(instructionBytes);
        }

        return Projection.EntryRawFileOffset == 0x012C92A4
            && Projection.EntryVirtualAddress == 0x016C9EA4
            && Projection.ThresholdEntryRawFileOffset == 0x012C92AB
            && Projection.RightFieldZeroDestinationRawFileOffset == 0x012C92BD
            && Projection.ThresholdBranchTargetRawFileOffset == 0x012C8E76
            && Projection.ThresholdFallthroughRawFileOffset == 0x012C92B7
            && Projection.FreshGlobalValueVirtualAddress == 0x01EDADD8
            && Projection.RawFileOffsetToVirtualAddress(Projection.EntryRawFileOffset)
                == Projection.EntryVirtualAddress
            && Projection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && nextRawOffset == Projection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
