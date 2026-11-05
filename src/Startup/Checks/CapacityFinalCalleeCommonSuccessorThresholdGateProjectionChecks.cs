using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonSuccessorThresholdGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw range, VA mapping, bytes, global operand, and branch target match",
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryRawFileOffset
                == 0x012C9334
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryVirtualAddress
                == 0x016C9F34
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FreshGlobalVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9343
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FallthroughRawFileOffset
                == 0x012C9343
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BranchTargetRawFileOffset)
                == 0x016C9A76
            && Normalize(CapacityFinalCalleeCommonSuccessorThresholdGateProjection.CapturedWindowBytes)
                == "8B42083B05D8ADED010F8233FBFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan allMissing =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: null,
                FreshEdxPlus8: null,
                FreshGlobal1EDADD8: null));
        check("All missing captures stop before the first load",
            allMissing.Outcome
                == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingEntryEDX
            && allMissing.Reads.IsEmpty
            && allMissing.Events.IsEmpty
            && !allMissing.FreshEdxPlus8ReadReached
            && !allMissing.FreshGlobalReadReached
            && !allMissing.CompareReached
            && !allMissing.JBReached
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9334
            && allMissing.Writes.IsEmpty
            && !allMissing.TargetEntered);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan missingEntryWithUnreachedGlobal =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: null,
                FreshEdxPlus8: 0,
                FreshGlobal1EDADD8: 0));
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan missingFieldWithUnreachedGlobal =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEdxPlus8: null,
                FreshGlobal1EDADD8: 0));
        check("The global capture remains unreached until the first fresh field load completes",
            missingEntryWithUnreachedGlobal.UnreachedFreshGlobalCaptureProvided
            && !missingEntryWithUnreachedGlobal.FreshGlobalReadReached
            && missingEntryWithUnreachedGlobal.Reads.IsEmpty
            && missingFieldWithUnreachedGlobal.UnreachedFreshGlobalCaptureProvided
            && missingFieldWithUnreachedGlobal.FreshEdxPlus8ReadReached
            && missingFieldWithUnreachedGlobal.Reads is [{ ValueCaptured: false }]
            && !missingFieldWithUnreachedGlobal.FreshGlobalReadReached
            && !missingFieldWithUnreachedGlobal.CompareReached
            && missingFieldWithUnreachedGlobal.StopAtRawFileOffsetExclusive == 0x012C9334);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan missingField =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEdxPlus8: null,
                FreshGlobal1EDADD8: null));
        CapacityFinalCalleeCommonSuccessorThresholdGatePlan missingGlobal =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEdxPlus8: 0x20,
                FreshGlobal1EDADD8: null));
        check("Each missing read stops at its first dependent instruction and keeps prior reads",
            missingField.Outcome
                == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingFreshEdxPlus8
            && missingField.FreshEdxPlus8Address == 0x108
            && missingField.Reads.Length == 1
            && !missingField.CompareReached
            && missingField.StopAtRawFileOffsetExclusive == 0x012C9334
            && missingGlobal.Outcome
                == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .MissingFreshGlobal1EDADD8
            && missingGlobal.Reads.Length == 2
            && missingGlobal.Reads[0].ValueCaptured
            && missingGlobal.Reads[0].Value == 0x20
            && missingGlobal.Reads[1].RawFileOffset == 0x012C9337
            && missingGlobal.Reads[1].Address == 0x01EDADD8
            && !missingGlobal.Reads[1].ValueCaptured
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.CompareReached
            && !missingGlobal.JBReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9337);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan zero =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0,
                FreshEdxPlus8: 0,
                FreshGlobal1EDADD8: 0));
        check("Zero equals zero, retains all CMP flags, and JB falls through",
            zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.JBTaken == false
            && zero.JBSelectedDestinationRawFileOffset == 0x012C9343
            && zero.Outcome
                == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedNotBelowFallthrough
            && zero.ExactCleanAdmission
            && zero.StopAtRawFileOffsetExclusive == 0x012C9343
            && zero.Writes.IsEmpty
            && !zero.TargetEntered);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan subtractionUnderflow =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0,
                FreshEdxPlus8: 0,
                FreshGlobal1EDADD8: 1));
        check("Unsigned subtraction wraps and JB follows CMP carry",
            subtractionUnderflow.CompareResult == 0xFFFFFFFF
            && subtractionUnderflow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && subtractionUnderflow.JBTaken == true
            && subtractionUnderflow.JBSelectedDestinationRawFileOffset == 0x012C8E76
            && subtractionUnderflow.Outcome
                == CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedBelowBranchTarget
            && subtractionUnderflow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan unsignedNotBelow =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEdxPlus8: 1,
                FreshGlobal1EDADD8: 0));
        check("A non-borrowing unsigned compare does not take JB",
            unsignedNotBelow.CompareResult == 1
            && unsignedNotBelow.CompareFlags?.Carry == false
            && unsignedNotBelow.JBTaken == false
            && unsignedNotBelow.JBSelectedDestinationRawFileOffset == 0x012C9343
            && unsignedNotBelow.ExactCleanAdmission);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan signedOverflow =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEdxPlus8: 0x80000000,
                FreshGlobal1EDADD8: 1));
        check("CMP records signed overflow independently from the unsigned JB condition",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.JBTaken == false
            && signedOverflow.JBSelectedDestinationRawFileOffset == 0x012C9343);

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan wrappedAddress =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Project(new(
                EntryEDX: 0xFFFFFFF8,
                FreshEdxPlus8: 7,
                FreshGlobal1EDADD8: 8));
        check("The entry EDX+8 effective address wraps as a DWORD",
            wrappedAddress.FreshEdxPlus8Address == 0
            && wrappedAddress.Reads[0].Address == 0
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection
                .DeriveEdxPlus8Address(0xFFFFFFF8) == 0
            && wrappedAddress.CompareLeft == 7
            && wrappedAddress.CompareRight == 8
            && wrappedAddress.JBTaken == true);

        check("The bounded projection preserves ordered reads and makes no future writes",
            zero.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Read,
                CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Read,
                CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Compare,
                CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Branch
            })
            && zero.Events[0].RawFileOffset == 0x012C9334
            && zero.Events[1].RawFileOffset == 0x012C9337
            && zero.Events[1].Address == 0x01EDADD8
            && zero.Events[^1].RawFileOffset == 0x012C933D
            && new[]
            {
                zero, subtractionUnderflow, unsignedNotBelow, signedOverflow, wrappedAddress
            }
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
            (0x012C9334, "8B 42 08", null, null),
            (0x012C9337, "3B 05 D8 AD ED 01", null, null),
            (0x012C933D, "0F 82 33 FB FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonSuccessorThresholdGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonSuccessorThresholdGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonSuccessorThresholdGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonSuccessorThresholdGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonSuccessorThresholdGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonSuccessorThresholdGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonSuccessorThresholdGateProjection.Instructions
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
