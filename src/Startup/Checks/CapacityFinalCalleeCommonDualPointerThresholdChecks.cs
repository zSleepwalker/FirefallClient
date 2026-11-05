using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonDualPointerThresholdChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw range, VA mapping, bytes, global operand, and both branch targets match",
            CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryRawFileOffset
                == 0x012C9403
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryVirtualAddress
                == 0x016CA003
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.FreshGlobalVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.FreshGlobalReadRawFileOffset
                == 0x012C9403
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.FreshEDXPlus8ReadRawFileOffset
                == 0x012C9409
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.FirstCompareRawFileOffset
                == 0x012C940C
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.FirstJBRawFileOffset
                == 0x012C940E
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.SecondCompareRawFileOffset
                == 0x012C9414
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.SecondJBRawFileOffset
                == 0x012C9416
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.BoundaryRawFileOffsetExclusive
                == 0x012C941C
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonDualPointerThresholdProjection.BranchTargetRawFileOffset)
                == 0x016C9A76
            && Normalize(CapacityFinalCalleeCommonDualPointerThresholdProjection.CapturedWindowBytes)
                == "8B0DD8ADED018B42083BD10F8262FAFFFF3BC10F825AFAFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonDualPointerThresholdPlan missingGlobal =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: null,
                EntryEDX: 0x100,
                FreshEDXPlus8: 0x200));
        check("The global read is first and missing its value stops before later captures are used",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshGlobalValue
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.FreshGlobalCaptured
            && missingGlobal.Reads is [{ RawFileOffset: 0x012C9403, ValueCaptured: false }]
            && missingGlobal.UnreachedEntryEDXCaptureProvided
            && missingGlobal.UnreachedFreshEDXPlus8CaptureProvided
            && !missingGlobal.FreshEDXPlus8ReadReached
            && !missingGlobal.FirstCompareReached
            && !missingGlobal.SecondCompareReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9403
            && missingGlobal.Writes.IsEmpty
            && !missingGlobal.TargetEntered);

        CapacityFinalCalleeCommonDualPointerThresholdPlan missingEntryEDX =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 0x80,
                EntryEDX: null,
                FreshEDXPlus8: 0x20));
        check("A known global is retained when the later entry pointer is missing",
            missingEntryEDX.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingEntryEDX
            && missingEntryEDX.FreshGlobalCaptured
            && missingEntryEDX.FreshGlobal == 0x80
            && missingEntryEDX.Reads is [{ RawFileOffset: 0x012C9403, Value: 0x80 }]
            && missingEntryEDX.UnreachedFreshEDXPlus8CaptureProvided
            && !missingEntryEDX.FreshEDXPlus8ReadReached
            && !missingEntryEDX.FirstCompareReached
            && missingEntryEDX.StopAtRawFileOffsetExclusive == 0x012C9409);

        CapacityFinalCalleeCommonDualPointerThresholdPlan missingFreshEDXPlus8 =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 0x80,
                EntryEDX: 0x100,
                FreshEDXPlus8: null));
        check("The fresh field read is attempted before either comparison",
            missingFreshEDXPlus8.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome.MissingFreshEDXPlus8
            && missingFreshEDXPlus8.FreshGlobal == 0x80
            && missingFreshEDXPlus8.EntryEDXCaptured
            && missingFreshEDXPlus8.FreshEDXPlus8ReadReached
            && missingFreshEDXPlus8.FreshEDXPlus8Address == 0x108
            && missingFreshEDXPlus8.Reads.Length == 2
            && missingFreshEDXPlus8.Reads[1] is
            {
                RawFileOffset: 0x012C9409,
                Address: 0x108,
                ValueCaptured: false
            }
            && !missingFreshEDXPlus8.FirstCompareReached
            && missingFreshEDXPlus8.StopAtRawFileOffsetExclusive == 0x012C9409);

        CapacityFinalCalleeCommonDualPointerThresholdPlan zero =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 0,
                EntryEDX: 0,
                FreshEDXPlus8: 0));
        check("Known zero is distinct from a missing capture and both zero comparisons fall through",
            zero.FreshGlobalCaptured
            && zero.FreshEDXPlus8Captured
            && zero.Reads.All(read => read.ValueCaptured && read.Value == 0)
            && zero.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.SecondCompareFlags == zero.FirstCompareFlags
            && zero.FirstJBTaken == false
            && zero.SecondJBTaken == false
            && zero.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .BothUnsignedNotBelowFallthrough
            && zero.StopAtRawFileOffsetExclusive == 0x012C941C
            && zero.ExactCleanAdmission);

        CapacityFinalCalleeCommonDualPointerThresholdPlan firstBelow =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 0x80000000,
                EntryEDX: 0x7FFFFFFF,
                FreshEDXPlus8: 0));
        check("The first JB takes its target on unsigned borrow and CMP flags remain at the stop",
            firstBelow.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .FirstUnsignedBelowBranchTarget
            && firstBelow.FirstCompareLeft == 0x7FFFFFFF
            && firstBelow.FirstCompareRight == 0x80000000
            && firstBelow.FirstCompareResult == 0xFFFFFFFF
            && firstBelow.FirstCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && firstBelow.FlagsAtStop == firstBelow.FirstCompareFlags
            && firstBelow.FirstJBTaken == true
            && firstBelow.FirstJBSelectedDestinationRawFileOffset == 0x012C8E76
            && !firstBelow.SecondCompareReached
            && !firstBelow.SecondJBReached
            && firstBelow.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Compare,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Branch
            })
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonDualPointerThresholdPlan independentFreshField =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 0x180,
                EntryEDX: 0x200,
                FreshEDXPlus8: 0x100));
        check("The EDX+8 observation is independent and can take only the second threshold",
            independentFreshField.FreshEDXPlus8Address == 0x208
            && independentFreshField.FirstCompareLeft == 0x200
            && independentFreshField.FirstCompareRight == 0x180
            && independentFreshField.FirstCompareFlags?.Carry == false
            && independentFreshField.FirstJBTaken == false
            && independentFreshField.SecondCompareLeft == 0x100
            && independentFreshField.SecondCompareRight == 0x180
            && independentFreshField.SecondCompareFlags?.Carry == true
            && independentFreshField.SecondJBTaken == true
            && independentFreshField.SecondJBSelectedDestinationRawFileOffset == 0x012C8E76
            && independentFreshField.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .SecondUnsignedBelowBranchTarget);

        CapacityFinalCalleeCommonDualPointerThresholdPlan signedOverflowWithoutBorrow =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 1,
                EntryEDX: 0x80000000,
                FreshEDXPlus8: 0x80000000));
        check("Signed overflow does not substitute for JB's unsigned carry condition",
            signedOverflowWithoutBorrow.FirstCompareFlags?.Overflow == true
            && signedOverflowWithoutBorrow.FirstCompareFlags?.Carry == false
            && signedOverflowWithoutBorrow.FirstJBTaken == false
            && signedOverflowWithoutBorrow.SecondCompareResult == 0x7FFFFFFF
            && signedOverflowWithoutBorrow.SecondCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflowWithoutBorrow.SecondJBTaken == false
            && signedOverflowWithoutBorrow.FlagsAtStop
                == signedOverflowWithoutBorrow.SecondCompareFlags
            && signedOverflowWithoutBorrow.Outcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .BothUnsignedNotBelowFallthrough);

        CapacityFinalCalleeCommonDualPointerThresholdPlan wrappedAddress =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                FreshGlobalValue: 8,
                EntryEDX: 0xFFFFFFF8,
                FreshEDXPlus8: 7));
        check("EDX+8 address calculation wraps as a DWORD without deriving the loaded value",
            wrappedAddress.FreshEDXPlus8Address == 0
            && wrappedAddress.Reads[1].Address == 0
            && CapacityFinalCalleeCommonDualPointerThresholdProjection
                .DeriveEdxPlus8Address(0xFFFFFFF8) == 0
            && wrappedAddress.FirstCompareLeft == 0xFFFFFFF8
            && wrappedAddress.SecondCompareLeft == 7
            && wrappedAddress.SecondJBTaken == true);

        check("Both fresh reads precede CMP1, CMP1 flags survive its branch, and CMP2 overwrites them",
            zero.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Read,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Compare,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Branch,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Compare,
                CapacityFinalCalleeCommonDualPointerThresholdEventKind.Branch
            })
            && zero.Events[0].RawFileOffset == 0x012C9403
            && zero.Events[0].Address == 0x01EDADD8
            && zero.Events[1].RawFileOffset == 0x012C9409
            && zero.Events[1].Address == 8
            && zero.Events[2].RawFileOffset == 0x012C940C
            && zero.Events[3].RawFileOffset == 0x012C940E
            && independentFreshField.Events[4].RawFileOffset == 0x012C9414
            && independentFreshField.Events[2].Flags
                == independentFreshField.FirstCompareFlags
            && independentFreshField.Events[4].Flags
                == independentFreshField.SecondCompareFlags
            && independentFreshField.Events[4].Flags != independentFreshField.Events[2].Flags
            && independentFreshField.FlagsAtStop == independentFreshField.SecondCompareFlags
            && independentFreshField.Events[^1].RawFileOffset == 0x012C9416
            && new[] { zero, firstBelow, independentFreshField, signedOverflowWithoutBorrow }
                .All(plan => plan.Writes.IsEmpty
                    && !plan.TargetEntered
                    && plan.ExactCleanAdmission));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9403, "8B 0D D8 AD ED 01", null, null),
            (0x012C9409, "8B 42 08", null, null),
            (0x012C940C, "3B D1", null, null),
            (0x012C940E, "0F 82 62 FA FF FF", 0x012C8E76, 0x016C9A76),
            (0x012C9414, "3B C1", null, null),
            (0x012C9416, "0F 82 5A FA FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence instruction
            in CapacityFinalCalleeCommonDualPointerThresholdProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonDualPointerThresholdProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonDualPointerThresholdProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonDualPointerThresholdProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonDualPointerThresholdProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonDualPointerThresholdProjection.Instructions
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
