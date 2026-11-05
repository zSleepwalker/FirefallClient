using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeCommonZeroSlotThresholdGateProjection;

internal static class CapacityFinalCalleeCommonZeroSlotThresholdGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat, and mapped virtual evidence matches the bounded CMP/JB window",
            InstructionEvidenceMatches());

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan allMissing =
            Projection.Project(new(
                EntryECX: null,
                FreshGlobalValue: null));
        check("Both missing CMP operands retain the absolute read attempt and stop without flags",
            allMissing.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .MissingBothCompareOperands
            && allMissing.CompareInstructionReached
            && allMissing.GlobalReadReached
            && allMissing.GlobalAddress == 0x01EDADD8
            && allMissing.Reads.Length == 1
            && allMissing.Reads[0] is
            {
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !allMissing.EntryECXCaptured
            && !allMissing.FreshGlobalValueCaptured
            && !allMissing.CompareReached
            && allMissing.CompareLeft is null
            && allMissing.CompareRight is null
            && allMissing.FlagsAtStop is null
            && !allMissing.BranchReached
            && allMissing.BranchFlags is null
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan missingEntryECX =
            Projection.Project(new(
                EntryECX: null,
                FreshGlobalValue: 0x11223344));
        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan missingGlobal =
            Projection.Project(new(
                EntryECX: 0x11223344,
                FreshGlobalValue: null));
        check("Each known same-CMP operand is retained when the other operand is missing",
            missingEntryECX.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX
            && missingEntryECX.CompareLeft is null
            && missingEntryECX.CompareRight == 0x11223344
            && missingEntryECX.Reads[0].ValueCaptured
            && missingEntryECX.Reads[0].Value == 0x11223344
            && missingEntryECX.FlagsAtStop is null
            && missingEntryECX.StopAtRawFileOffsetExclusive == 0x012C945C
            && missingGlobal.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingFreshGlobalValue
            && missingGlobal.CompareLeft == 0x11223344
            && missingGlobal.CompareRight is null
            && missingGlobal.GlobalAddress == 0x01EDADD8
            && missingGlobal.Reads[0].AddressCaptured
            && !missingGlobal.Reads[0].ValueCaptured
            && missingGlobal.FlagsAtStop is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan zero =
            Projection.Project(new(
                EntryECX: 0,
                FreshGlobalValue: 0));
        check("Captured zero operands compare equal and select the 9468 fallthrough",
            zero.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && zero.CompareReached
            && zero.CompareLeft == 0
            && zero.CompareRight == 0
            && zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.BranchReached
            && zero.BranchTaken == false
            && zero.BranchFlags == zero.CompareFlags
            && zero.BranchSelectedDestinationRawFileOffset == 0x012C9468
            && zero.StopAtRawFileOffsetExclusive == 0x012C9468
            && zero.ExactCleanCompletion
            && !zero.TargetEntered
            && !zero.MemoryWriteReached);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan equal =
            Projection.Project(new(
                EntryECX: 0x13579BDF,
                FreshGlobalValue: 0x13579BDF));
        check("Equal nonzero operands preserve zero CMP flags and do not take unsigned JB",
            equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.BranchTaken == false
            && equal.BranchSelectedDestinationRawFileOffset == 0x012C9468);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan borrow =
            Projection.Project(new(
                EntryECX: 0,
                FreshGlobalValue: 1));
        check("Unsigned borrow takes JB and exposes the six subtraction flags",
            borrow.Outcome
                == CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedBelowBranchTarget
            && borrow.CompareResult == 0xFFFFFFFF
            && borrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && borrow.BranchTaken == true
            && !borrow.TargetEntered
            && borrow.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && borrow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan signedOverflow =
            Projection.Project(new(
                EntryECX: 0x80000000,
                FreshGlobalValue: 1));
        check("Signed overflow does not change unsigned JB selection",
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
            && signedOverflow.BranchTaken == false
            && signedOverflow.BranchSelectedDestinationRawFileOffset == 0x012C9468);

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan maximum =
            Projection.Project(new(
                EntryECX: uint.MaxValue,
                FreshGlobalValue: 0));
        check("Maximum DWORD comparison wraps as x86 subtraction and uses unsigned ordering",
            maximum.CompareResult == uint.MaxValue
            && maximum.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maximum.BranchTaken == false
            && maximum.BranchSelectedDestinationRawFileOffset == 0x012C9468);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C945C, "3B 0D D8 AD ED 01", null, null),
            (0x012C9462, "0F 82 0E FA FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonZeroSlotThresholdGateInstructionEvidence instruction
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
                int displacement = BinaryPrimitives.ReadInt32LittleEndian(
                    instructionBytes.AsSpan(2, 4));
                if ((long)instruction.RawFileOffset + instructionBytes.Length + displacement
                    != branchTarget)
                {
                    return false;
                }
            }

            nextRawOffset += (uint)instructionBytes.Length;
            bytes += Convert.ToHexString(instructionBytes);
        }

        return Projection.EntryRawFileOffset == 0x012C945C
            && Projection.EntryVirtualAddress == 0x016CA05C
            && Projection.RawFileOffsetToVirtualAddress(Projection.EntryRawFileOffset)
                == Projection.EntryVirtualAddress
            && Projection.CompareRawFileOffset == 0x012C945C
            && Projection.BranchRawFileOffset == 0x012C9462
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C9468
            && Projection.BranchTargetRawFileOffset == 0x012C8E76
            && Projection.GlobalValueVirtualAddress == 0x01EDADD8
            && nextRawOffset == Projection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.Instructions
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
