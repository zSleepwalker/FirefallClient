using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeRootThresholdLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed root-to-threshold window retains its raw and VA mapping",
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.EntryRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.EntryVirtualAddress
                == 0x016C9D9B
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.ThresholdEntryRawFileOffset
                == 0x012C91A8
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.RootEqualDestinationRawFileOffset
                == 0x012C91D6
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.ThresholdBranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.ThresholdFallthroughRawFileOffset
                == 0x012C91B9
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeRootThresholdLifecycleProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeRootThresholdLifecycleProjection.EntryVirtualAddress
            && Normalize(
                CapacityFinalCalleeLargeRootThresholdLifecycleProjection.CapturedWindowBytes)
                == "8B420C8B4A18894DF03BC2742E8B4A083BCB894DEC8B4DF00F82BDFCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan allMissing =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(new(
                EntryEDX: null,
                EntryEBP: null,
                FreshEdxPlusC: null,
                FreshEdxPlus18: null,
                EntryEBX: null,
                FreshEdxPlus8: null,
                FreshEbpMinus10: null));
        check("All missing root evidence stops at its first read without creating a threshold child",
            allMissing.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingRootEntryEDX
            && allMissing.RootGate.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX
            && allMissing.Threshold is null
            && !allMissing.ThresholdReached
            && allMissing.Reads is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                    RawFileOffset: 0x012C919B,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && allMissing.Writes.IsEmpty
            && allMissing.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Read
            })
            && !allMissing.BranchReached
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan rootMissingWithLateCaptures =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(new(
                EntryEDX: null,
                EntryEBP: 0x200,
                FreshEdxPlusC: 0x100,
                FreshEdxPlus18: 0x300,
                EntryEBX: 0x20,
                FreshEdxPlus8: 0x20,
                FreshEbpMinus10: 0x400));
        check("Unreached root and threshold captures remain bounded when root EDX is missing",
            rootMissingWithLateCaptures.Threshold is null
            && rootMissingWithLateCaptures.RootGate.UnreachedEntryEBPCaptureProvided
            && rootMissingWithLateCaptures.RootGate.UnreachedFreshEdxPlusCCaptureProvided
            && rootMissingWithLateCaptures.RootGate.UnreachedFreshEdxPlus18CaptureProvided
            && rootMissingWithLateCaptures.UnreachedEntryEBXCaptureProvided
            && rootMissingWithLateCaptures.UnreachedFreshEdxPlus8CaptureProvided
            && rootMissingWithLateCaptures.UnreachedFreshEbpMinus10CaptureProvided
            && rootMissingWithLateCaptures.Writes.IsEmpty
            && !rootMissingWithLateCaptures.ExactCleanExitAdmitted
            && rootMissingWithLateCaptures.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootThresholdLifecycleCapture rootEqualCapture = new(
            EntryEDX: 0x100,
            EntryEBP: 0x200,
            FreshEdxPlusC: 0x100,
            FreshEdxPlus18: 0xAABBCCDD,
            EntryEBX: null,
            FreshEdxPlus8: null,
            FreshEbpMinus10: null);
        CapacityFinalCalleeLargeRootThresholdLifecyclePlan rootEqual =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(rootEqualCapture);
        check("Clean equal root exits at 91D6 without projecting threshold work",
            rootEqual.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.RootEqualToTarget
            && rootEqual.RootGate.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget
            && rootEqual.RootGate.BranchReached
            && rootEqual.RootGate.BranchTaken == true
            && rootEqual.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && rootEqual.RootGate.StopAtRawFileOffsetExclusive == 0x012C91D6
            && rootEqual.RootGate.ExactCleanAdmission
            && rootEqual.BranchReached
            && rootEqual.BranchTaken == true
            && rootEqual.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && rootEqual.FlagsAtStop is { Carry: false, Zero: true }
            && rootEqual.BranchFlags == rootEqual.FlagsAtStop
            && rootEqual.Threshold is null
            && !rootEqual.ThresholdReached
            && rootEqual.RootCleanExitAdmitted
            && rootEqual.ExactCleanExitAdmitted
            && !rootEqual.RootGate.UnreachedEntryEBPCaptureProvided
            && !rootEqual.RootGate.UnreachedFreshEdxPlusCCaptureProvided
            && !rootEqual.RootGate.UnreachedFreshEdxPlus18CaptureProvided
            && !rootEqual.UnreachedEntryEBXCaptureProvided
            && !rootEqual.UnreachedFreshEdxPlus8CaptureProvided
            && !rootEqual.UnreachedFreshEbpMinus10CaptureProvided
            && rootEqual.Writes is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                    RawFileOffset: 0x012C91A1,
                    Address: 0x1F0,
                    Value: 0xAABBCCDD,
                    Committed: true
                }
            ]
            && rootEqual.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan contaminatedRootEqual =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(
                rootEqualCapture with
                {
                    EntryEBX = 0x20,
                    FreshEdxPlus8 = 0x20,
                    FreshEbpMinus10 = 0x400
                });
        check("Root equality rejects later threshold captures and never creates a child",
            contaminatedRootEqual.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .RootEqualWithRejectedThresholdCaptures
            && contaminatedRootEqual.RootGate.BranchTaken == true
            && contaminatedRootEqual.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && contaminatedRootEqual.Threshold is null
            && !contaminatedRootEqual.RootCleanExitAdmitted
            && !contaminatedRootEqual.ExactCleanExitAdmitted
            && contaminatedRootEqual.UnreachedEntryEBXCaptureProvided
            && contaminatedRootEqual.UnreachedFreshEdxPlus8CaptureProvided
            && contaminatedRootEqual.UnreachedFreshEbpMinus10CaptureProvided
            && contaminatedRootEqual.Writes.Length == 1
            && contaminatedRootEqual.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan missingField =
            ProjectUnequal(
                entryEBX: 0x20,
                freshEdxPlus8: null,
                freshEbpMinus10: 0x44444444);
        check("A missing threshold field retains the root store and blocks later threshold captures",
            missingField.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .MissingThresholdFreshEdxPlus8
            && missingField.ThresholdReached
            && missingField.EntryEDXRetainedForThreshold
            && missingField.ThresholdEntryEDX == 0x100
            && missingField.Threshold is
            {
                Outcome: CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEdxPlus8,
                UnreachedEntryEBPCaptureProvided: false,
                UnreachedEntryEBXCaptureProvided: true,
                UnreachedFreshEbpMinus10CaptureProvided: true
            }
            && !missingField.EntryEBPRetainedForThreshold
            && missingField.ThresholdEntryEBP is null
            && missingField.Reads.Length == 3
            && missingField.Writes is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                    RawFileOffset: 0x012C91A1,
                    Value: 0x33333333,
                    Committed: true
                }
            ]
            && !missingField.ExactCleanExitAdmitted
            && missingField.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan missingEBX =
            ProjectUnequal(
                entryEBX: null,
                freshEdxPlus8: 0x20,
                freshEbpMinus10: 0x44444444);
        check("Missing caller EBX retains the root store and stops before CMP or the threshold store",
            missingEBX.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome.MissingThresholdEntryEBX
            && missingEBX.Threshold is
            {
                Outcome: CapacityFinalCalleeLargeFrameThresholdOutcome.MissingEntryEBX,
                FreshEdxPlus8Captured: true,
                CompareReached: false,
                StoreReached: false,
                UnreachedEntryEBPCaptureProvided: false,
                UnreachedFreshEbpMinus10CaptureProvided: true
            }
            && !missingEBX.EntryEBPRetainedForThreshold
            && missingEBX.ThresholdEntryEBP is null
            && missingEBX.Reads.Length == 3
            && missingEBX.Writes is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                    RawFileOffset: 0x012C91A1,
                    Committed: true
                }
            ]
            && missingEBX.UnreachedFreshEbpMinus10CaptureProvided
            && !missingEBX.ExactCleanExitAdmitted
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C91AB);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan missingFrame =
            ProjectUnequal(
                entryEBX: 0x20,
                freshEdxPlus8: 0x20,
                freshEbpMinus10: null);
        check("A missing fresh frame read preserves CMP flags and both ordered writes",
            missingFrame.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .MissingThresholdFreshEbpMinus10
            && missingFrame.Threshold is
            {
                Outcome: CapacityFinalCalleeLargeFrameThresholdOutcome.MissingFreshEbpMinus10,
                CompareReached: true,
                StoreReached: true,
                FreshEbpMinus10ReadReached: true,
                BranchReached: false,
                EntryEBPCaptured: true,
                UnreachedEntryEBPCaptureProvided: false,
                CompareFlags: { Zero: true, Carry: false }
            }
            && missingFrame.EntryEBPRetainedForThreshold
            && missingFrame.ThresholdEntryEBP == 0x220
            && missingFrame.FlagsAtStop is { Zero: true, Carry: false }
            && missingFrame.RootGate.CompareFlags is { Zero: false, Carry: false }
            && missingFrame.BranchFlags is { Zero: false, Carry: false }
            && missingFrame.FlagsAtStop != missingFrame.BranchFlags
            && missingFrame.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C91A1, 0x012C91AD })
            && missingFrame.Writes.Select(write => write.Stage).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.RootGate,
                CapacityFinalCalleeLargeRootThresholdLifecycleStage.Threshold
            })
            && missingFrame.Writes.All(write => write.Committed)
            && missingFrame.Threshold!.BranchFlags is null
            && !missingFrame.ExactCleanExitAdmitted
            && missingFrame.StopAtRawFileOffsetExclusive == 0x012C91B0);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan wrappedBelow =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(new(
                EntryEDX: 0xFFFFFFF8,
                EntryEBP: 0x10,
                FreshEdxPlusC: 0,
                FreshEdxPlus18: 0x11111111,
                EntryEBX: 1,
                FreshEdxPlus8: 0,
                FreshEbpMinus10: 0x22222222));
        CapacityFinalCalleeLargeRootThresholdLifecycleEvent belowBranch =
            wrappedBelow.Events[^1];
        check("The unequal root composes into unsigned-below while wraparound aliasing reloads fresh frame data",
            wrappedBelow.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .ThresholdUnsignedBelowBranchTarget
            && wrappedBelow.RootGate.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough
            && wrappedBelow.RootGate.BranchReached
            && wrappedBelow.RootGate.BranchTaken == false
            && wrappedBelow.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && wrappedBelow.RootGate.StopAtRawFileOffsetExclusive == 0x012C91A8
            && wrappedBelow.RootGate.ExactCleanAdmission
            && wrappedBelow.Threshold is
            {
                Capture.EntryEDX: 0xFFFFFFF8,
                Capture.EntryEBP: 0x10,
                FreshEdxPlus8Address: 0,
                StoreAddress: 0xFFFFFFFC,
                FreshEbpMinus10Address: 0,
                FreshEbpMinus10: 0x22222222,
                EntryEBPCaptured: true,
                UnreachedEntryEBPCaptureProvided: false,
                UnreachedEntryEBXCaptureProvided: false,
                UnreachedFreshEdxPlus8CaptureProvided: false,
                UnreachedFreshEbpMinus10CaptureProvided: false,
                CompareFlags: { Carry: true },
                BranchFlags: { Carry: true },
                BranchTaken: true,
                BranchSelectedDestinationRawFileOffset: 0x012C8E76,
                ExactCleanAdmission: true
            }
            && wrappedBelow.ThresholdEntryEDX == 0xFFFFFFF8
            && wrappedBelow.ThresholdEntryEBP == 0x10
            && wrappedBelow.RootGate.StoreAddress == 0
            && wrappedBelow.RootGate.StoreValue == 0x11111111
            && wrappedBelow.Threshold!.FreshEbpMinus10Address
                == wrappedBelow.RootGate.StoreAddress
            && wrappedBelow.Threshold.FreshEbpMinus10
                != wrappedBelow.RootGate.StoreValue
            && wrappedBelow.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C91A1, 0x012C91AD })
            && wrappedBelow.Writes.Select(write => write.Value)
                .SequenceEqual(new uint?[] { 0x11111111, 0 })
            && wrappedBelow.Events.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C919B, 0x012C919E, 0x012C91A1, 0x012C91A4, 0x012C91A6,
                0x012C91A8, 0x012C91AB, 0x012C91AD, 0x012C91B0, 0x012C91B3
            })
            && wrappedBelow.BranchReached
            && wrappedBelow.BranchTaken == true
            && wrappedBelow.BranchFlags?.Carry == true
            && wrappedBelow.FlagsAtStop?.Carry == true
            && !wrappedBelow.RootGate.UnreachedEntryEBPCaptureProvided
            && !wrappedBelow.RootGate.UnreachedFreshEdxPlusCCaptureProvided
            && !wrappedBelow.RootGate.UnreachedFreshEdxPlus18CaptureProvided
            && !wrappedBelow.UnreachedEntryEBXCaptureProvided
            && !wrappedBelow.UnreachedFreshEdxPlus8CaptureProvided
            && !wrappedBelow.UnreachedFreshEbpMinus10CaptureProvided
            && belowBranch.Kind
                == CapacityFinalCalleeLargeRootThresholdLifecycleEventKind.Branch
            && belowBranch.Flags == wrappedBelow.BranchFlags
            && belowBranch.BranchTaken == true
            && wrappedBelow.RootCleanExitAdmitted
            && wrappedBelow.ThresholdCleanExitAdmitted
            && wrappedBelow.ExactCleanExitAdmitted
            && wrappedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeRootThresholdLifecyclePlan notBelow =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(new(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshEdxPlusC: 0x101,
                FreshEdxPlus18: 0x33333333,
                EntryEBX: 1,
                FreshEdxPlus8: 2,
                FreshEbpMinus10: 0x44444444));
        check("The other threshold branch is an exact bounded clean fallthrough with retained CMP flags",
            notBelow.Outcome
                == CapacityFinalCalleeLargeRootThresholdLifecycleOutcome
                    .ThresholdUnsignedNotBelowFallthrough
            && notBelow.Threshold is
            {
                EntryEBPCaptured: true,
                UnreachedEntryEBPCaptureProvided: false,
                UnreachedEntryEBXCaptureProvided: false,
                UnreachedFreshEdxPlus8CaptureProvided: false,
                UnreachedFreshEbpMinus10CaptureProvided: false,
                CompareLeft: 2,
                CompareRight: 1,
                CompareFlags: { Carry: false, Zero: false },
                BranchFlags: { Carry: false, Zero: false },
                BranchTaken: false,
                BranchSelectedDestinationRawFileOffset: 0x012C91B9,
                ExactCleanAdmission: true
            }
            && notBelow.Threshold!.BranchFlags == notBelow.Threshold.CompareFlags
            && notBelow.Events[^1].Flags == notBelow.BranchFlags
            && notBelow.FlagsAtStop == notBelow.BranchFlags
            && !notBelow.RootGate.UnreachedEntryEBPCaptureProvided
            && !notBelow.RootGate.UnreachedFreshEdxPlusCCaptureProvided
            && !notBelow.RootGate.UnreachedFreshEdxPlus18CaptureProvided
            && !notBelow.UnreachedEntryEBXCaptureProvided
            && !notBelow.UnreachedFreshEdxPlus8CaptureProvided
            && !notBelow.UnreachedFreshEbpMinus10CaptureProvided
            && notBelow.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C91A1, 0x012C91AD })
            && notBelow.ExactCleanExitAdmitted
            && notBelow.StopAtRawFileOffsetExclusive == 0x012C91B9);
    }

    private static CapacityFinalCalleeLargeRootThresholdLifecyclePlan ProjectUnequal(
        uint? entryEBX,
        uint? freshEdxPlus8,
        uint? freshEbpMinus10) =>
        CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Project(new(
            EntryEDX: 0x100,
            EntryEBP: 0x220,
            FreshEdxPlusC: 0x101,
            FreshEdxPlus18: 0x33333333,
            EntryEBX: entryEBX,
            FreshEdxPlus8: freshEdxPlus8,
            FreshEbpMinus10: freshEbpMinus10));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C919B, "8B 42 0C", null, null),
            (0x012C919E, "8B 4A 18", null, null),
            (0x012C91A1, "89 4D F0", null, null),
            (0x012C91A4, "3B C2", null, null),
            (0x012C91A6, "74 2E", 0x012C91D6, 0x016C9DD6),
            (0x012C91A8, "8B 4A 08", null, null),
            (0x012C91AB, "3B CB", null, null),
            (0x012C91AD, "89 4D EC", null, null),
            (0x012C91B0, "8B 4D F0", null, null),
            (0x012C91B3, "0F 82 BD FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeRootThresholdLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRootThresholdLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeRootThresholdLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeRootThresholdLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeRootThresholdLifecycleProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeRootThresholdLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeRootThresholdLifecycleProjection.Instructions
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
