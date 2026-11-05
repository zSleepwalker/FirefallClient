using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeEaxGlobalThresholdGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, instruction addresses, global VA, and branch target are exact",
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryRawFileOffset
                == 0x012C927C
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryVirtualAddress
                == 0x016C9E7C
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.FreshGlobalVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.JBRawFileOffset
                == 0x012C9282
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9288
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.FallthroughRawFileOffset
                == 0x012C9288
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.BranchTargetRawFileOffset)
                == 0x016C9A76
            && Normalize(CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.CapturedWindowBytes)
                == "3B05D8ADED010F82EEFBFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan allMissing =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: null,
                FreshGlobalValue: null));
        check("Both missing operands retain the independent global read and stop before CMP",
            allMissing.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .MissingBothCompareOperands
            && allMissing.Reads is
                [{ RawFileOffset: 0x012C927C, Address: 0x01EDADD8, AddressCaptured: true,
                    ValueCaptured: false, Value: null }]
            && allMissing.Events is
                [{ Kind: CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind.Read,
                    Address: 0x01EDADD8, ValueCaptured: false }]
            && allMissing.FreshGlobalReadReached
            && !allMissing.FreshGlobalCaptured
            && allMissing.FreshGlobalAddress == 0x01EDADD8
            && !allMissing.EntryEAXCaptured
            && !allMissing.CompareReached
            && allMissing.CompareFlags is null
            && allMissing.FlagsAtStop is null
            && !allMissing.JBReached
            && allMissing.JBTaken is null
            && allMissing.JBSelectedDestinationRawFileOffset is null
            && !allMissing.ExactCleanAdmission
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C927C
            && allMissing.Writes.IsEmpty
            && !allMissing.TargetEntered);

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan missingEAX =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: null,
                FreshGlobalValue: 0));
        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan missingGlobal =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 0,
                FreshGlobalValue: null));
        check("Either missing operand stops at CMP while preserving the other capture and no flags",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome.MissingEntryEAX
            && missingEAX.FreshGlobalReadReached
            && missingEAX.FreshGlobalCaptured
            && missingEAX.FreshGlobalValue == 0
            && missingEAX.CompareRight == 0
            && missingEAX.Reads[0].ValueCaptured
            && missingEAX.Reads[0].Value == 0
            && !missingEAX.EntryEAXCaptured
            && !missingEAX.CompareReached
            && missingEAX.CompareFlags is null
            && !missingEAX.JBReached
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C927C
            && missingGlobal.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome.MissingFreshGlobalValue
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.FreshGlobalCaptured
            && missingGlobal.FreshGlobalAddress == 0x01EDADD8
            && missingGlobal.EntryEAXCaptured
            && missingGlobal.CompareLeft == 0
            && !missingGlobal.CompareReached
            && missingGlobal.CompareFlags is null
            && !missingGlobal.JBReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan zeroEqual =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 0,
                FreshGlobalValue: 0));
        check("Zero equals zero, sets CMP parity and zero, and unsigned JB falls through",
            zeroEqual.CompareResult == 0
            && zeroEqual.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroEqual.JBReached
            && zeroEqual.JBTaken == false
            && zeroEqual.JBSelectedDestinationRawFileOffset == 0x012C9288
            && zeroEqual.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && zeroEqual.ExactCleanAdmission
            && zeroEqual.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan borrow =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 0,
                FreshGlobalValue: 1));
        check("Unsigned borrow takes JB to 8E76 and records all six DWORD CMP flags",
            borrow.CompareResult == 0xFFFFFFFF
            && borrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && borrow.JBTaken == true
            && borrow.JBSelectedDestinationRawFileOffset == 0x012C8E76
            && borrow.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedBelowBranchTarget
            && borrow.ExactCleanAdmission
            && borrow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan overflow =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 0x80000000,
                FreshGlobalValue: 1));
        check("Signed overflow is recorded without changing the unsigned JB decision",
            overflow.CompareResult == 0x7FFFFFFF
            && overflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflow.JBTaken == false
            && overflow.JBSelectedDestinationRawFileOffset == 0x012C9288
            && overflow.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan auxiliaryAndEvenParity =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 0x10,
                FreshGlobalValue: 1));
        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan oddParity =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: 2,
                FreshGlobalValue: 1));
        check("AF follows bit four of subtraction and PF follows even parity of the low result byte",
            auxiliaryAndEvenParity.CompareResult == 0x0F
            && auxiliaryAndEvenParity.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && oddParity.CompareResult == 1
            && oddParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan unsignedMaximum =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Project(new(
                EntryEAX: uint.MaxValue,
                FreshGlobalValue: 0));
        check("UINT32_MAX compares as unsigned not-below despite the result sign bit",
            unsignedMaximum.CompareResult == uint.MaxValue
            && unsignedMaximum.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedMaximum.JBTaken == false
            && unsignedMaximum.JBSelectedDestinationRawFileOffset == 0x012C9288
            && unsignedMaximum.Outcome
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && unsignedMaximum.StopAtRawFileOffsetExclusive == 0x012C9288);

        check("The bounded branch records no entry into 8E76 or the 9288 fallthrough",
            new[] { zeroEqual, borrow, overflow, auxiliaryAndEvenParity, oddParity, unsignedMaximum }
                .All(plan => plan.JBReached
                    && plan.Events[^1].Kind
                        == CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind.Branch
                    && BranchAndEventDestinationMatch(plan)
                    && plan.Writes.IsEmpty
                    && !plan.TargetEntered
                    && plan.ExactCleanAdmission));
    }

    private static bool BranchAndEventDestinationMatch(
        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan plan) =>
        plan.Events[^1].RawFileOffset == 0x012C9282
        && plan.Events[^1].BranchTaken == plan.JBTaken
        && plan.Events[^1].SelectedDestinationRawFileOffset
            == plan.JBSelectedDestinationRawFileOffset
        && plan.FlagsAtStop == plan.CompareFlags;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C927C, "3B 05 D8 AD ED 01", null, null),
            (0x012C9282, "0F 82 EE FB FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeEaxGlobalThresholdGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection.Instructions
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
