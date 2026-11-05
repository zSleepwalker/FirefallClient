using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeFrameThresholdProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw range, VA mapping, instruction bytes, and relative branch target match",
            CapacityFinalCalleeLargeFrameThresholdProjection.EntryRawFileOffset
                == 0x012C91A8
            && CapacityFinalCalleeLargeFrameThresholdProjection.EntryVirtualAddress
                == 0x016C9DA8
            && CapacityFinalCalleeLargeFrameThresholdProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeFrameThresholdProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeFrameThresholdProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeFrameThresholdProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91B9
            && CapacityFinalCalleeLargeFrameThresholdProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeFrameThresholdProjection.FallthroughRawFileOffset
                == 0x012C91B9
            && Normalize(CapacityFinalCalleeLargeFrameThresholdProjection.CapturedWindowBytes)
                == "8B4A083BCB894DEC8B4DF00F82BDFCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeFrameThresholdPlan allMissing =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: null,
                EntryEBX: null,
                EntryEBP: null,
                FreshEdxPlus8: null,
                FreshEbpMinus10: null));
        CapacityFinalCalleeLargeFrameThresholdPlan missingFreshField =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 0x20,
                EntryEBP: 0x200,
                FreshEdxPlus8: null,
                FreshEbpMinus10: 0x400));
        CapacityFinalCalleeLargeFrameThresholdPlan missingEbx =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: null,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: 0x400));
        CapacityFinalCalleeLargeFrameThresholdPlan missingEbp =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 0x20,
                EntryEBP: null,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: 0x400));
        CapacityFinalCalleeLargeFrameThresholdPlan missingFreshFrame =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 0x20,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: null));
        check("Every missing capture stops at its first dependent instruction",
            allMissing.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEDX
            && allMissing.Reads.IsEmpty
            && allMissing.Events.IsEmpty
            && !allMissing.CompareReached
            && !allMissing.StoreReached
            && !allMissing.BranchReached
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91A8
            && missingFreshField.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEdxPlus8
            && missingFreshField.FreshEdxPlus8ReadReached
            && missingFreshField.FreshEdxPlus8Address == 0x108
            && !missingFreshField.CompareReached
            && missingFreshField.UnreachedEntryEBXCaptureProvided
            && missingFreshField.UnreachedEntryEBPCaptureProvided
            && missingFreshField.UnreachedFreshEbpMinus10CaptureProvided
            && missingFreshField.StopAtRawFileOffsetExclusive == 0x012C91A8
            && missingEbx.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBX
            && missingEbx.FreshEdxPlus8Captured
            && !missingEbx.CompareReached
            && !missingEbx.StoreReached
            && missingEbx.UnreachedEntryEBPCaptureProvided
            && missingEbx.UnreachedFreshEbpMinus10CaptureProvided
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C91AB
            && missingEbp.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBP
            && missingEbp.CompareReached
            && missingEbp.CompareFlags.HasValue
            && !missingEbp.StoreReached
            && missingEbp.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeFrameThresholdEventKind.Read,
                CapacityFinalCalleeLargeFrameThresholdEventKind.Compare
            })
            && missingEbp.UnreachedFreshEbpMinus10CaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C91AD
            && missingFreshFrame.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEbpMinus10
            && missingFreshFrame.StoreReached
            && missingFreshFrame.Writes is [{ Committed: true, Address: 0x1EC, Value: 0x20 }]
            && missingFreshFrame.FreshEbpMinus10ReadReached
            && !missingFreshFrame.FreshEbpMinus10Captured
            && missingFreshFrame.CompareFlags.HasValue
            && missingFreshFrame.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeFrameThresholdEventKind.Read,
                CapacityFinalCalleeLargeFrameThresholdEventKind.Compare,
                CapacityFinalCalleeLargeFrameThresholdEventKind.Write,
                CapacityFinalCalleeLargeFrameThresholdEventKind.Read
            })
            && missingFreshFrame.StopAtRawFileOffsetExclusive == 0x012C91B0);

        CapacityFinalCalleeLargeFrameThresholdPlan wrapped =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0xFFFFFFF8,
                EntryEBX: 0,
                EntryEBP: 0,
                FreshEdxPlus8: 0,
                FreshEbpMinus10: 1));
        check("DWORD effective addresses wrap and both clean branch endpoints are bounded",
            wrapped.FreshEdxPlus8Address == 0
            && wrapped.StoreAddress == 0xFFFFFFEC
            && wrapped.FreshEbpMinus10Address == 0xFFFFFFF0
            && wrapped.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedNotBelowFallthrough
            && !wrapped.BranchTaken.Value
            && wrapped.BranchSelectedDestinationRawFileOffset == 0x012C91B9
            && wrapped.ExactCleanAdmission
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C91B9
            && CapacityFinalCalleeLargeFrameThresholdProjection.DeriveEdxPlus8Address(
                0xFFFFFFF8) == 0
            && CapacityFinalCalleeLargeFrameThresholdProjection.DeriveEbpMinus14Address(0)
                == 0xFFFFFFEC
            && CapacityFinalCalleeLargeFrameThresholdProjection.DeriveEbpMinus10Address(0)
                == 0xFFFFFFF0);

        CapacityFinalCalleeLargeFrameThresholdPlan aliasedFresh =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0xE4,
                EntryEBX: 0xFFFFFFFF,
                EntryEBP: 0x100,
                FreshEdxPlus8: 0xDEADBEEF,
                FreshEbpMinus10: 0x01000000));
        check("Aliased source and store addresses retain separate fresh captures",
            aliasedFresh.FreshEdxPlus8Address == 0xEC
            && aliasedFresh.StoreAddress == aliasedFresh.FreshEdxPlus8Address
            && aliasedFresh.Writes is [{ Value: 0xDEADBEEF }]
            && aliasedFresh.FreshEbpMinus10Address == 0xF0
            && aliasedFresh.FreshEbpMinus10 == 0x01000000
            && aliasedFresh.CompareLeft == 0xDEADBEEF
            && aliasedFresh.CompareRight == 0xFFFFFFFF
            && aliasedFresh.ExactCleanAdmission);

        CapacityFinalCalleeLargeFrameThresholdPlan unsignedBorrow =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 1,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0,
                FreshEbpMinus10: 0xFFFFFFFF));
        CapacityFinalCalleeLargeFrameThresholdPlan signedDisagrees =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0,
                EntryEBX: 0x7FFFFFFF,
                EntryEBP: 0x100,
                FreshEdxPlus8: 0x80000000,
                FreshEbpMinus10: 0x12345678));
        check("JB uses unsigned carry while the full CMP flags retain signed overflow details",
            unsignedBorrow.CompareResult == 0xFFFFFFFF
            && unsignedBorrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedBorrow.BranchTaken == true
            && unsignedBorrow.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedBelowBranchTarget
            && unsignedBorrow.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && unsignedBorrow.ExactCleanAdmission
            && unsignedBorrow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && signedDisagrees.CompareResult == 1
            && signedDisagrees.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedDisagrees.BranchTaken == false
            && signedDisagrees.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.UnsignedNotBelowFallthrough);

        CapacityFinalCalleeLargeFrameThresholdPlan staleCompareFlags =
            CapacityFinalCalleeLargeFrameThresholdProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 1,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0,
                FreshEbpMinus10: 0xFFFFFFFF));
        CapacityFinalCalleeLargeFrameThresholdEvent branch =
            staleCompareFlags.Events[^1];
        check("The second ECX load does not replace CMP flags consumed by JB",
            staleCompareFlags.FreshEbpMinus10 == 0xFFFFFFFF
            && staleCompareFlags.CompareLeft == 0
            && staleCompareFlags.CompareRight == 1
            && staleCompareFlags.CompareFlags is { Carry: true }
            && staleCompareFlags.BranchFlags == staleCompareFlags.CompareFlags
            && branch.Kind == CapacityFinalCalleeLargeFrameThresholdEventKind.Branch
            && branch.Flags == staleCompareFlags.CompareFlags
            && branch.BranchTaken == true
            && branch.SelectedDestinationRawFileOffset == 0x012C8E76);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C91A8, "8B 4A 08", null, null),
            (0x012C91AB, "3B CB", null, null),
            (0x012C91AD, "89 4D EC", null, null),
            (0x012C91B0, "8B 4D F0", null, null),
            (0x012C91B3, "0F 82 BD FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeFrameThresholdProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeFrameThresholdInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameThresholdProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeFrameThresholdProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeFrameThresholdProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeFrameThresholdProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeFrameThresholdProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeFrameThresholdProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeFrameThresholdProjection.Instructions
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
