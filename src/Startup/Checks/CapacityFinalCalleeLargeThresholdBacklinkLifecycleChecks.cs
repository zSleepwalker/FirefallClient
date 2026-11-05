using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeThresholdBacklinkLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed instruction window and both branch destinations are exact",
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.EntryRawFileOffset
                == 0x012C91A8
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.EntryVirtualAddress
                == 0x016C9DA8
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.BacklinkGateEntryRawFileOffset
                == 0x012C91B9
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.ThresholdBelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.BacklinkUnequalDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.BacklinkEqualDestinationRawFileOffset
                == 0x012C91C5
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.BoundaryRawFileOffsetExclusive
                == 0x012C91C5
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.CapturedWindowBytes)
                == "8B4A083BCB894DEC8B4DF00F82BDFCFFFF8B5DEC39530C0F85B1FCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan allMissing =
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Project(new(
                EntryEDX: null,
                EntryEBX: null,
                EntryEBP: null,
                FreshEdxPlus8: null,
                FreshEbpMinus10: null,
                FreshFrameMinus14: null,
                FreshBacklink: null));
        check("Missing upstream evidence creates no child and stops before threshold effects",
            allMissing.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingThresholdEntryEDX
            && allMissing.Threshold.Outcome
                == CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEDX
            && allMissing.BacklinkGate is null
            && !allMissing.BacklinkGateReached
            && !allMissing.EntryEBPRetainedForBacklinkGate
            && !allMissing.EntryEDXRetainedForBacklinkGate
            && allMissing.UnreachedFreshFrameMinus14CaptureProvided == false
            && allMissing.UnreachedFreshBacklinkCaptureProvided == false
            && allMissing.Reads.Length == 0
            && allMissing.Writes.IsEmpty
            && allMissing.Events.IsEmpty
            && !allMissing.ThresholdCleanExitAdmitted
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan missingUpstreamWithLateCaptures =
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: null,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: 0x400,
                FreshFrameMinus14: 0x500,
                FreshBacklink: 0x600));
        check("Unreached threshold and child captures remain rejected without a child",
            missingUpstreamWithLateCaptures.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingThresholdEntryEBX
            && missingUpstreamWithLateCaptures.BacklinkGate is null
            && missingUpstreamWithLateCaptures.Threshold.CompareReached == false
            && missingUpstreamWithLateCaptures.Threshold.StoreReached == false
            && missingUpstreamWithLateCaptures.UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstreamWithLateCaptures.UnreachedFreshBacklinkCaptureProvided
            && missingUpstreamWithLateCaptures.Writes.IsEmpty
            && !missingUpstreamWithLateCaptures.ExactCleanExitAdmitted
            && missingUpstreamWithLateCaptures.StopAtRawFileOffsetExclusive
                == 0x012C91AB);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan missingThresholdFrame =
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 0x20,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: null,
                FreshFrameMinus14: 0x500,
                FreshBacklink: 0x600));
        check("A missing threshold frame read retains its prior store and CMP flags only",
            missingThresholdFrame.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingThresholdFreshEbpMinus10
            && missingThresholdFrame.BacklinkGate is null
            && missingThresholdFrame.Threshold.StoreReached
            && missingThresholdFrame.Threshold.Writes is
                [{ RawFileOffset: 0x012C91AD, Address: 0x1EC, Value: 0x20, Committed: true }]
            && missingThresholdFrame.Threshold.CompareFlags is { Zero: true, Carry: false }
            && missingThresholdFrame.FlagsAtStop is { Zero: true, Carry: false }
            && !missingThresholdFrame.Threshold.BranchReached
            && missingThresholdFrame.Events[^1].Stage
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleStage.Threshold
            && missingThresholdFrame.UnreachedFreshFrameMinus14CaptureProvided
            && missingThresholdFrame.UnreachedFreshBacklinkCaptureProvided
            && !missingThresholdFrame.ExactCleanExitAdmitted
            && missingThresholdFrame.StopAtRawFileOffsetExclusive == 0x012C91B0);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan belowClean =
            ProjectThreshold(
                entryEDX: 0x100,
                entryEBX: 1,
                freshEdxPlus8: 0,
                freshFrameMinus14: null,
                freshBacklink: null);
        check("Clean threshold-below branch ends at 8E76 without fabricating a backlink child",
            belowClean.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .ThresholdUnsignedBelowBranchTarget
            && belowClean.BacklinkGate is null
            && !belowClean.BacklinkGateReached
            && belowClean.Threshold.BranchFlags is { Carry: true }
            && belowClean.BranchFlags is { Carry: true }
            && belowClean.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && belowClean.Threshold.Writes is
                [{ RawFileOffset: 0x012C91AD, Address: 0x1EC, Value: 0, Committed: true }]
            && !belowClean.UnreachedFreshFrameMinus14CaptureProvided
            && !belowClean.UnreachedFreshBacklinkCaptureProvided
            && belowClean.ThresholdCleanExitAdmitted
            && belowClean.ExactCleanExitAdmitted
            && belowClean.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan belowContaminated =
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBX: 1,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0,
                FreshEbpMinus10: 0x444,
                FreshFrameMinus14: 0x555,
                FreshBacklink: 0x666));
        check("Below-branch captures are bounded and rejected while threshold effects survive",
            belowContaminated.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .ThresholdBelowWithRejectedBacklinkCaptures
            && belowContaminated.BacklinkGate is null
            && !belowContaminated.BacklinkGateReached
            && belowContaminated.UnreachedFreshFrameMinus14CaptureProvided
            && belowContaminated.UnreachedFreshBacklinkCaptureProvided
            && belowContaminated.Threshold.Writes is
                [{ RawFileOffset: 0x012C91AD, Address: 0x1EC, Value: 0, Committed: true }]
            && belowContaminated.Threshold.CompareFlags is { Carry: true }
            && belowContaminated.BranchFlags is { Carry: true }
            && belowContaminated.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && belowContaminated.ThresholdCleanExitAdmitted
            && !belowContaminated.ExactCleanExitAdmitted
            && belowContaminated.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan missingChildFrame =
            ProjectThreshold(
                entryEDX: 0x100,
                entryEBX: 0x20,
                freshEdxPlus8: 0x20,
                freshFrameMinus14: null,
                freshBacklink: 0x600);
        check("The threshold fallthrough retains EBP, but an absent fresh frame cannot retain EDX",
            missingChildFrame.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingBacklinkFreshFrameMinus14
            && missingChildFrame.BacklinkGateReached
            && missingChildFrame.BacklinkGate is
            {
                Outcome: CapacityFinalCalleeLargeFrameBacklinkGateOutcome
                    .MissingFreshFrameMinus14,
                Reads.Length: 1,
                UnreachedEntryEDXCaptureProvided: false,
                UnreachedFreshBacklinkCaptureProvided: true,
                CompareReached: false,
                JnzReached: false
            }
            && missingChildFrame.EntryEBPRetainedForBacklinkGate
            && missingChildFrame.BacklinkGateEntryEBP == 0x200
            && !missingChildFrame.EntryEDXRetainedForBacklinkGate
            && missingChildFrame.BacklinkGateEntryEDX is null
            && missingChildFrame.UnreachedFreshBacklinkCaptureProvided
            && missingChildFrame.Threshold.Writes.Length == 1
            && missingChildFrame.Threshold.CompareFlags is { Carry: false }
            && missingChildFrame.BranchFlags is { Carry: false }
            && missingChildFrame.StopAtRawFileOffsetExclusive == 0x012C91B9);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan missingChildBacklink =
            ProjectThreshold(
                entryEDX: 0x100,
                entryEBX: 0x20,
                freshEdxPlus8: 0x20,
                freshFrameMinus14: 0x500,
                freshBacklink: null);
        check("The child EDX is retained only after its fresh frame load and before missing CMP data",
            missingChildBacklink.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .MissingBacklinkFreshBacklink
            && missingChildBacklink.EntryEBPRetainedForBacklinkGate
            && missingChildBacklink.EntryEDXRetainedForBacklinkGate
            && missingChildBacklink.BacklinkGateEntryEDX == 0x100
            && missingChildBacklink.BacklinkGate is
            {
                EntryEDXCaptured: true,
                BacklinkReadReached: true,
                BacklinkAddress: 0x50C,
                BacklinkCaptured: false,
                CompareReached: false,
                JnzReached: false
            }
            && missingChildBacklink.BacklinkGate!.Reads.Length == 2
            && missingChildBacklink.Threshold.Writes.Length == 1
            && missingChildBacklink.Threshold.CompareFlags is { Carry: false }
            && !missingChildBacklink.BacklinkCleanExitAdmitted
            && !missingChildBacklink.ExactCleanExitAdmitted
            && missingChildBacklink.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeFrameBacklinkGatePlan missingEDXOperand =
            CapacityFinalCalleeLargeFrameBacklinkGateProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: null,
                FreshBacklink: 0x300));
        check("The child helper retains its fresh CMP memory operand when EDX is missing",
            missingEDXOperand.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEDX
            && missingEDXOperand.BacklinkReadReached
            && missingEDXOperand.BacklinkCaptured
            && missingEDXOperand.BacklinkAddress == 0x10C
            && missingEDXOperand.BacklinkValue == 0x300
            && !missingEDXOperand.CompareReached
            && !missingEDXOperand.JnzReached
            && missingEDXOperand.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan zeroEqual =
            ProjectThreshold(
                entryEDX: 0,
                entryEBX: 0x20,
                freshEdxPlus8: 0x20,
                freshFrameMinus14: 0,
                freshBacklink: 0);
        check("Independent zero frame and backlink captures take the exact 91C5 fallthrough",
            zeroEqual.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .BacklinkEqualToFallthrough
            && zeroEqual.BacklinkGate is
            {
                FrameMinus14: 0,
                BacklinkAddress: 0x0C,
                BacklinkValue: 0,
                CompareLeft: 0,
                CompareRight: 0,
                CompareFlags: { Zero: true, Carry: false },
                JnzReached: true,
                JnzTaken: false,
                JnzSelectedDestinationRawFileOffset: 0x012C91C5,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C91C5
            }
            && zeroEqual.Threshold.Writes is
                [{ Address: 0x1EC, Value: 0x20, Committed: true }]
            && zeroEqual.BacklinkGate!.FrameMinus14Address == 0x1EC
            && zeroEqual.BacklinkGate.FrameMinus14 != zeroEqual.Threshold.Writes[0].Value
            && zeroEqual.BacklinkGate.Reads[0].Address == zeroEqual.Threshold.Writes[0].Address
            && zeroEqual.BacklinkGate.Reads[0].Value == 0
            && zeroEqual.BacklinkGate.Reads[1].Value == 0
            && zeroEqual.BacklinkCleanExitAdmitted
            && zeroEqual.ExactCleanExitAdmitted
            && zeroEqual.BranchFlags is { Zero: true }
            && zeroEqual.BranchSelectedDestinationRawFileOffset == 0x012C91C5
            && zeroEqual.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan unequal =
            ProjectThreshold(
                entryEDX: 0,
                entryEBX: 0x20,
                freshEdxPlus8: 0x20,
                freshFrameMinus14: 0x1E0,
                freshBacklink: 1);
        check("An unequal fresh backlink takes the exact 8E76 branch with independent aliased reads",
            unequal.Outcome
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleOutcome
                    .BacklinkUnequalToTarget
            && unequal.BacklinkGate is
            {
                FrameMinus14Address: 0x1EC,
                FrameMinus14: 0x1E0,
                BacklinkAddress: 0x1EC,
                BacklinkValue: 1,
                CompareLeft: 1,
                CompareRight: 0,
                JnzReached: true,
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C8E76,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C8E76
            }
            && unequal.BacklinkGate!.Reads[0].Address == unequal.BacklinkGate.Reads[1].Address
            && unequal.BacklinkGate.Reads[0].Value != unequal.BacklinkGate.Reads[1].Value
            && unequal.Threshold.Writes[0].Address == unequal.BacklinkGate.Reads[0].Address
            && unequal.Threshold.Writes[0].Value != unequal.BacklinkGate.Reads[0].Value
            && unequal.Threshold.Writes[0].Value != unequal.BacklinkGate.Reads[1].Value
            && unequal.BacklinkCleanExitAdmitted
            && unequal.ExactCleanExitAdmitted
            && unequal.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && unequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan wrapped =
            ProjectThreshold(
                entryEDX: 0xFFFFFFF8,
                entryEBX: 0,
                entryEBP: 0x14,
                freshEdxPlus8: 0,
                freshEbpMinus10: 0,
                freshFrameMinus14: 0xFFFFFFF4,
                freshBacklink: 0xFFFFFFF8);
        check("DWORD threshold and child addresses wrap without reusing the aliased store value",
            wrapped.BacklinkGate is
            {
                FrameMinus14Address: 0,
                FrameMinus14: 0xFFFFFFF4,
                BacklinkAddress: 0,
                BacklinkValue: 0xFFFFFFF8,
                CompareLeft: 0xFFFFFFF8,
                CompareRight: 0xFFFFFFF8,
                CompareFlags: { Zero: true },
                JnzTaken: false,
                JnzSelectedDestinationRawFileOffset: 0x012C91C5
            }
            && wrapped.Threshold.StoreAddress == 0
            && wrapped.Threshold.StoreValue == 0
            && wrapped.BacklinkGate.Reads[0].Address == wrapped.Threshold.Writes[0].Address
            && wrapped.BacklinkGate.Reads[0].Value != wrapped.Threshold.Writes[0].Value
            && wrapped.BacklinkGate.Reads[1].Address == 0
            && wrapped.BacklinkCleanExitAdmitted
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C91C5);
    }

    private static CapacityFinalCalleeLargeThresholdBacklinkLifecyclePlan ProjectThreshold(
        uint entryEDX,
        uint entryEBX,
        uint freshEdxPlus8,
        uint entryEBP = 0x200,
        uint freshEbpMinus10 = 0x400,
        uint? freshFrameMinus14 = 0x500,
        uint? freshBacklink = 0x600) =>
        CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Project(new(
            entryEDX,
            entryEBX,
            entryEBP,
            freshEdxPlus8,
            freshEbpMinus10,
            freshFrameMinus14,
            freshBacklink));

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
            (0x012C91B3, "0F 82 BD FC FF FF", 0x012C8E76, 0x016C9A76),
            (0x012C91B9, "8B 5D EC", null, null),
            (0x012C91BC, "39 53 0C", null, null),
            (0x012C91BF, "0F 85 B1 FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeThresholdBacklinkLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeThresholdBacklinkLifecycleProjection.Instructions
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
