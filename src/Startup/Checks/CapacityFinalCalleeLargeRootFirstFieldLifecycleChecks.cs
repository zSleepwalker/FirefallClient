using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeRootFirstFieldLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed route preserves the root JZ target and exact first-field window",
            CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.EntryRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.EntryVirtualAddress
                == 0x016C9D9B
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.RootUnequalDestinationRawFileOffset
                == 0x012C91A8
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.RootEqualDestinationRawFileOffset
                == 0x012C91D6
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.FirstFieldZeroDestinationRawFileOffset
                == 0x012C91E3
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.FirstFieldNonzeroDestinationRawFileOffset
                == 0x012C91F0
            && CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.CapturedWindowBytes)
                == "8B420C8B4A18894DF03BC2742E8B42148D4A14894DFC85C0750D"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan allMissing =
            CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.Project(new(
                EntryEDX: null,
                EntryEBP: null,
                FreshEdxPlusC: null,
                FreshEdxPlus18: null,
                FreshEdxPlus14: null));
        check("All missing captures stop at the root load",
            allMissing.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome.MissingRootEntryEDX
            && allMissing.RootGate.Reads.Length == 1
            && allMissing.Writes.IsEmpty
            && !allMissing.FirstFieldTestReached
            && allMissing.FirstFieldTest is null
            && !allMissing.FirstFieldEntryEDXRetained
            && !allMissing.FirstFieldEntryEBPRetained
            && !allMissing.BranchReached
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan rootMissingWithLateCaptures =
            Project(
                entryEdx: null,
                entryEbp: 0x200,
                freshPlusC: 0x11111111,
                freshPlus18: 0x22222222,
                freshPlus14: 0);
        check("Missing root EDX bounds all later captures, including a captured-zero child value",
            rootMissingWithLateCaptures.RootGate.UnreachedEntryEBPCaptureProvided
            && rootMissingWithLateCaptures.RootGate.UnreachedFreshEdxPlusCCaptureProvided
            && rootMissingWithLateCaptures.RootGate.UnreachedFreshEdxPlus18CaptureProvided
            && rootMissingWithLateCaptures.UnreachedFirstFieldFreshEdxPlus14CaptureProvided
            && !rootMissingWithLateCaptures.FirstFieldTestReached
            && rootMissingWithLateCaptures.Writes.IsEmpty
            && !rootMissingWithLateCaptures.ExactCleanExitAdmitted
            && rootMissingWithLateCaptures.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan unequal =
            Project(
                entryEdx: 1,
                entryEbp: 0x20,
                freshPlusC: 0,
                freshPlus18: 0xAABBCCDD,
                freshPlus14: null);
        check("An unequal root is bounded at 91A8 and does not retain child registers",
            unequal.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                    .RootUnequalToBoundedReject
            && unequal.RootGate.Outcome
                == CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough
            && unequal.RootGate.BranchTaken == false
            && unequal.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && unequal.RootGate.BranchReached
            && unequal.FirstFieldTest is null
            && !unequal.FirstFieldTestReached
            && !unequal.FirstFieldEntryEDXRetained
            && !unequal.FirstFieldEntryEBPRetained
            && unequal.FirstFieldEntryEBP is null
            && unequal.Writes is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.RootGate,
                    RawFileOffset: 0x012C91A1,
                    Value: 0xAABBCCDD,
                    Committed: true
                }
            ]
            && unequal.RootCleanExitAdmitted
            && unequal.ExactCleanExitAdmitted
            && unequal.BranchReached
            && unequal.BranchTaken == false
            && unequal.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && unequal.StopAtRawFileOffsetExclusive
                == unequal.BranchSelectedDestinationRawFileOffset
            && unequal.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unequal.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan unequalWithLateChild =
            Project(
                entryEdx: 1,
                entryEbp: 0x20,
                freshPlusC: 0,
                freshPlus18: 0xAABBCCDD,
                freshPlus14: 0);
        check("An unequal root rejects even a supplied zero child capture without entering 91D6",
            unequalWithLateChild.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                    .RootUnequalWithRejectedFirstFieldCapture
            && unequalWithLateChild.RejectedFirstFieldCaptures
            && unequalWithLateChild.UnreachedFirstFieldFreshEdxPlus14CaptureProvided
            && !unequalWithLateChild.FirstFieldTestReached
            && unequalWithLateChild.Writes.Length == 1
            && !unequalWithLateChild.RootCleanExitAdmitted
            && !unequalWithLateChild.ExactCleanExitAdmitted
            && unequalWithLateChild.BranchReached
            && unequalWithLateChild.BranchTaken == false
            && unequalWithLateChild.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && unequalWithLateChild.StopAtRawFileOffsetExclusive
                == unequalWithLateChild.BranchSelectedDestinationRawFileOffset
            && unequalWithLateChild.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan missingChild =
            Project(
                entryEdx: 0x100,
                entryEbp: 0x200,
                freshPlusC: 0x100,
                freshPlus18: 0x33333333,
                freshPlus14: null);
        check("The equal root retains its committed store when the independent plus-14 read is missing",
            missingChild.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                    .MissingFirstFieldFreshEdxPlus14
            && missingChild.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && missingChild.FirstFieldTestReached
            && missingChild.FirstFieldEntryEDXRetained
            && missingChild.FirstFieldEntryEDX == 0x100
            && !missingChild.FirstFieldEntryEBPRetained
            && missingChild.FirstFieldEntryEBP is null
            && missingChild.FirstFieldTest is
            {
                Outcome: CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingFreshEAX,
                FreshEAXReadReached: true,
                FreshEDXPlus14Address: 0x114,
                FreshEAXCaptured: false,
                UnreachedEntryEBPCaptureProvided: false
            }
            && missingChild.Writes is
            [
                {
                    Stage: CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.RootGate,
                    RawFileOffset: 0x012C91A1,
                    Value: 0x33333333,
                    Committed: true
                }
            ]
            && missingChild.RootCleanExitAdmitted
            && !missingChild.FirstFieldCleanExitAdmitted
            && !missingChild.ExactCleanExitAdmitted
            && missingChild.RootGate.BranchReached
            && missingChild.RootGate.BranchTaken == true
            && missingChild.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91D6
            && missingChild.RootGate.StopAtRawFileOffsetExclusive == 0x012C91D6
            && missingChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingChild.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan zeroChild =
            Project(
                entryEdx: 0x100,
                entryEbp: 0x200,
                freshPlusC: 0x100,
                freshPlus18: 0x22222222,
                freshPlus14: 0);
        check("A captured zero is an independent plus-14 value and takes the exact 91E3 branch",
            zeroChild.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                    .FirstFieldZeroToCleanFallthrough
            && zeroChild.RootGate.BranchTaken == true
            && zeroChild.FirstFieldTest?.FreshEAXCaptured == true
            && zeroChild.FirstFieldTest.FreshEAX == 0
            && zeroChild.FirstFieldTest.FreshEDXPlus14Address == 0x114
            && zeroChild.RootGate.CompareFlags is { Zero: true }
            && zeroChild.FirstFieldTest.TestEAXFlags is { Zero: true, Carry: false }
            && zeroChild.FlagsAtStop == new CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags(
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false)
            && zeroChild.FirstFieldTest.FreshEAX != zeroChild.RootGate.FreshEdxPlusC
            && zeroChild.FirstFieldTest.StoreValue == 0x114
            && zeroChild.Writes.Select(write => write.Stage).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.RootGate,
                CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.FirstFieldTest
            })
            && zeroChild.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C91A1, 0x012C91DC })
            && zeroChild.Writes.All(write => write.Committed)
            && zeroChild.BranchTaken == false
            && zeroChild.BranchSelectedDestinationRawFileOffset == 0x012C91E3
            && zeroChild.RootCleanExitAdmitted
            && zeroChild.FirstFieldCleanExitAdmitted
            && zeroChild.ExactCleanExitAdmitted
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan nonzeroChild =
            Project(
                entryEdx: 0xFFFFFFF0,
                entryEbp: 0x10,
                freshPlusC: 0xFFFFFFF0,
                freshPlus18: 0xDEADBEEF,
                freshPlus14: 0x80000001);
        check("The nonzero child uses original retained roots, wraps aliases, and exits exactly at 91F0",
            nonzeroChild.Outcome
                == CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                    .FirstFieldNonzeroToCleanTarget
            && nonzeroChild.FirstFieldEntryEDX == 0xFFFFFFF0
            && nonzeroChild.FirstFieldEntryEBPRetained
            && nonzeroChild.FirstFieldEntryEBP == 0x10
            && nonzeroChild.FirstFieldTest?.FreshEDXPlus14Address == 4
            && nonzeroChild.FirstFieldTest?.LEAECXValue == 4
            && nonzeroChild.FirstFieldTest?.StoreAddress == 0xC
            && nonzeroChild.FirstFieldTest?.StoreValue == 4
            && nonzeroChild.FirstFieldTest.TestEAXFlags is
            {
                Zero: false,
                Sign: true,
                Carry: false,
                Overflow: false
            }
            && nonzeroChild.FlagsAtStop is
            {
                Carry: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroChild.Writes.Select(write => write.Address)
                .SequenceEqual(new uint?[] { 0, 0xC })
            && nonzeroChild.Writes.Select(write => write.Value)
                .SequenceEqual(new uint?[] { 0xDEADBEEF, 4 })
            && nonzeroChild.BranchTaken == true
            && nonzeroChild.BranchSelectedDestinationRawFileOffset == 0x012C91F0
            && nonzeroChild.FirstFieldCleanExitAdmitted
            && nonzeroChild.ExactCleanExitAdmitted
            && nonzeroChild.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan aliasedFreshReads =
            Project(
                entryEdx: 0x100,
                entryEbp: 0x124,
                freshPlusC: 0x100,
                freshPlus18: 0x33333333,
                freshPlus14: 0x44444444);
        check("The root store may alias the fresh plus-14 address without reusing either prior root value",
            aliasedFreshReads.RootGate.StoreAddress == 0x114
            && aliasedFreshReads.FirstFieldTest?.FreshEDXPlus14Address == 0x114
            && aliasedFreshReads.Writes.Select(write => write.Address)
                .SequenceEqual(new uint?[] { 0x114, 0x120 })
            && aliasedFreshReads.Writes.Select(write => write.Value)
                .SequenceEqual(new uint?[] { 0x33333333, 0x114 })
            && aliasedFreshReads.FirstFieldTest?.FreshEAX == 0x44444444
            && aliasedFreshReads.FirstFieldTest.FreshEAX
                != aliasedFreshReads.RootGate.StoreValue
            && aliasedFreshReads.FirstFieldTest.FreshEAX
                != aliasedFreshReads.RootGate.FreshEdxPlusC
            && aliasedFreshReads.ExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan Project(
        uint? entryEdx,
        uint? entryEbp,
        uint? freshPlusC,
        uint? freshPlus18,
        uint? freshPlus14) =>
        CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.Project(new(
            entryEdx,
            entryEbp,
            freshPlusC,
            freshPlus18,
            freshPlus14));

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
            (0x012C91D6, "8B 42 14", null, null),
            (0x012C91D9, "8D 4A 14", null, null),
            (0x012C91DC, "89 4D FC", null, null),
            (0x012C91DF, "85 C0", null, null),
            (0x012C91E1, "75 0D", 0x012C91F0, 0x016C9DF0)
        };
        var actual = CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection.Instructions;
        return actual.Length == expected.Length
            && actual.Select(instruction =>
                (instruction.RawFileOffset,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && actual.All(instruction =>
                instruction.FlatIdaAddress == instruction.RawFileOffset
                && instruction.VirtualAddress
                    == CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                && (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        == CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : !instruction.BranchTargetVirtualAddress.HasValue));
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
