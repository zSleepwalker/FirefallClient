using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeRootPairedFieldLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed window joins the clean root equality branch to both child tests",
            CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.EntryRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.EntryVirtualAddress
                == 0x016C9D9B
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                .RootUnequalDestinationRawFileOffset == 0x012C91A8
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                .RootEqualDestinationRawFileOffset == 0x012C91D6
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                .FirstChildNonzeroDestinationRawFileOffset == 0x012C91F0
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                .SecondChildZeroDestinationRawFileOffset == 0x012C9226
            && CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.EntryVirtualAddress
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingRootEDX =
            Project(null, 0x800, 0x100, 0x200, 0x300, 0x400);
        check("Missing root EDX preserves the failed root read and never reaches child fields",
            missingRootEDX.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome.MissingRootEntryEDX
            && missingRootEDX.RootGate.Reads.Length == 1
            && missingRootEDX.Reads.Length == 1
            && missingRootEDX.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.RootGate,
                RawFileOffset: 0x012C919B,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingRootEDX.Writes.IsEmpty
            && !missingRootEDX.FirstChildTestReached
            && !missingRootEDX.SecondChildTestReached
            && missingRootEDX.FlagsAtStop is null
            && missingRootEDX.UnreachedFirstChildFreshEAXCaptureProvided
            && missingRootEDX.UnreachedSecondChildFreshEAXCaptureProvided
            && missingRootEDX.ReachedInstructions.SequenceEqual(new[] { 0x012C919Bu })
            && !missingRootEDX.ExactCleanExitAdmitted
            && missingRootEDX.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingRootFirstFresh =
            Project(0x100, 0x800, null, 0x34, 0, 0x12);
        check("Missing root first fresh data preserves the read and rejects all child captures",
            missingRootFirstFresh.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootFreshEdxPlusC
            && missingRootFirstFresh.Reads.Length == 1
            && missingRootFirstFresh.Reads[0] is
            {
                Address: 0x10C,
                ValueCaptured: false,
                Value: null
            }
            && missingRootFirstFresh.Writes.IsEmpty
            && !missingRootFirstFresh.FirstChildTestReached
            && !missingRootFirstFresh.SecondChildTestReached
            && missingRootFirstFresh.UnreachedFirstChildFreshEAXCaptureProvided
            && missingRootFirstFresh.UnreachedSecondChildFreshEAXCaptureProvided
            && missingRootFirstFresh.FlagsAtStop is null
            && missingRootFirstFresh.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingRootSecondFresh =
            Project(0x100, 0x800, 0x100, null, 0, 0x12);
        check("Missing root second fresh data preserves both reads without entering children",
            missingRootSecondFresh.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootFreshEdxPlus18
            && missingRootSecondFresh.Reads.Length == 2
            && missingRootSecondFresh.Reads[1] is
            {
                RawFileOffset: 0x012C919E,
                Address: 0x118,
                ValueCaptured: false,
                Value: null
            }
            && missingRootSecondFresh.Writes.IsEmpty
            && !missingRootSecondFresh.FirstChildTestReached
            && !missingRootSecondFresh.SecondChildTestReached
            && missingRootSecondFresh.FlagsAtStop is null
            && missingRootSecondFresh.UnreachedFirstChildFreshEAXCaptureProvided
            && missingRootSecondFresh.UnreachedSecondChildFreshEAXCaptureProvided
            && missingRootSecondFresh.StopAtRawFileOffsetExclusive == 0x012C919E);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingRootEBP =
            Project(0x100, null, 0x100, 0x34, 0, 0x12);
        check("Missing root EBP stops before the root store and retains no child state",
            missingRootEBP.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootEntryEBPForStore
            && missingRootEBP.Reads.Length == 2
            && missingRootEBP.Writes.IsEmpty
            && !missingRootEBP.RootGate.StoreReached
            && !missingRootEBP.RootGate.CompareReached
            && !missingRootEBP.FirstChildTestReached
            && !missingRootEBP.SecondChildTestReached
            && missingRootEBP.FlagsAtStop is null
            && missingRootEBP.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C919Bu,
                0x012C919Eu
            })
            && missingRootEBP.UnreachedFirstChildFreshEAXCaptureProvided
            && missingRootEBP.UnreachedSecondChildFreshEAXCaptureProvided
            && missingRootEBP.StopAtRawFileOffsetExclusive == 0x012C91A1);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan rootUnequal =
            Project(0x100, 0x800, 0x101, 0x44, 0, 0x12);
        check("Unequal root takes bounded 91A8 and rejects both child fresh captures",
            rootUnequal.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .RootUnequalWithRejectedChildCaptures
            && rootUnequal.RootGate.CompareLeft == 0x101
            && rootUnequal.RootGate.CompareRight == 0x100
            && rootUnequal.RootGate.BranchTaken == false
            && rootUnequal.RootGate.BranchSelectedDestinationRawFileOffset == 0x012C91A8
            && rootUnequal.RootGate.StopAtRawFileOffsetExclusive == 0x012C91A8
            && rootUnequal.RootGate.Writes.Length == 1
            && rootUnequal.RootGate.Writes[0] is
            {
                RawFileOffset: 0x012C91A1,
                Address: 0x7F0,
                Value: 0x44,
                Committed: true
            }
            && rootUnequal.FirstChildTest is null
            && rootUnequal.SecondChildTest is null
            && !rootUnequal.FirstChildTestReached
            && !rootUnequal.SecondChildTestReached
            && rootUnequal.UnreachedFirstChildFreshEAXCaptureProvided
            && rootUnequal.UnreachedSecondChildFreshEAXCaptureProvided
            && rootUnequal.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && rootUnequal.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C919Bu,
                0x012C919Eu,
                0x012C91A1u,
                0x012C91A4u,
                0x012C91A6u
            })
            && !rootUnequal.RootCleanExitAdmitted
            && !rootUnequal.ExactCleanExitAdmitted
            && rootUnequal.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan cleanRootReject =
            Project(0x100, 0x800, 0x101, 0x44, null, null);
        check("A clean bounded unequal root exit is admitted only with no unreached child captures",
            cleanRootReject.RootCleanExitAdmitted
            && cleanRootReject.ExactCleanExitAdmitted
            && !cleanRootReject.UnreachedFirstChildFreshEAXCaptureProvided
            && !cleanRootReject.UnreachedSecondChildFreshEAXCaptureProvided
            && cleanRootReject.StopAtRawFileOffsetExclusive == 0x012C91A8);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingFirstChild =
            Project(0x100, 0x800, 0x100, 0x34, null, 0x12);
        check("Missing first child data keeps root CMP flags and rejects the unobserved second capture",
            missingFirstChild.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingFirstChildFreshEAX
            && missingFirstChild.RootCleanExitAdmitted
            && missingFirstChild.FirstChildTestReached
            && !missingFirstChild.SecondChildTestReached
            && missingFirstChild.FirstChildEntryEDXRetained
            && missingFirstChild.FirstChildEntryEDX == 0x100
            && !missingFirstChild.FirstChildEntryEBPRetained
            && missingFirstChild.FirstChildTest!.Capture.EntryEBP is null
            && missingFirstChild.Reads.Length == 3
            && missingFirstChild.Reads[2] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.FirstChild,
                RawFileOffset: 0x012C91D6,
                Address: 0x114,
                ValueCaptured: false
            }
            && missingFirstChild.Writes.Length == 1
            && missingFirstChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingFirstChild.UnreachedSecondChildFreshEAXCaptureProvided
            && !missingFirstChild.ExactCleanExitAdmitted
            && missingFirstChild.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan firstChildNonzero =
            Project(0x100, 0x800, 0x100, 0x34, 1, 0x12);
        check("First child nonzero exits at 91F0 and rejects the unreached second field",
            firstChildNonzero.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome.FirstChildNonzeroToTarget
            && firstChildNonzero.FirstChildTest!.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && firstChildNonzero.SecondChildTest is null
            && firstChildNonzero.UnreachedSecondChildFreshEAXCaptureProvided
            && !firstChildNonzero.ExactCleanExitAdmitted
            && firstChildNonzero.BranchSelectedDestinationRawFileOffset == 0x012C91F0
            && firstChildNonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan missingSecondChild =
            Project(0x100, 0x800, 0x100, 0x34, 0, null);
        check("A zero first child reaches the second read and preserves first TEST flags if it is missing",
            missingSecondChild.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingSecondChildFreshEAX
            && missingSecondChild.FirstChildTest!.FreshEAX == 0
            && missingSecondChild.FirstChildTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && missingSecondChild.SecondChildTest is
            {
                Capture.EntryEDX: 0x100,
                Capture.EntryEBP: null,
                Outcome: CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingFreshEAX
            }
            && missingSecondChild.SecondChildEntryEDXRetained
            && missingSecondChild.SecondChildEntryEDX == 0x100
            && !missingSecondChild.SecondChildEntryEBPRetained
            && missingSecondChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingSecondChild.Reads.Length == 4
            && missingSecondChild.Reads[3] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.SecondChild,
                RawFileOffset: 0x012C91E3,
                Address: 0x110,
                ValueCaptured: false
            }
            && missingSecondChild.Writes.Length == 2
            && missingSecondChild.Writes[1] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.FirstChild,
                Address: 0x7FC,
                Value: 0x114,
                Committed: true
            }
            && !missingSecondChild.ExactCleanExitAdmitted
            && missingSecondChild.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan secondChildZero =
            Project(0xFFFFFFFC, 2, 0xFFFFFFFC, 0x12345678, 0, 0);
        check("Zero child values retain wraparound addresses and ordered repeated EBP-4 stores",
            secondChildZero.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome.SecondChildZeroToTarget
            && secondChildZero.RootGate.FreshEdxPlusCAddress == 8
            && secondChildZero.RootGate.FreshEdxPlus18Address == 0x14
            && secondChildZero.RootGate.Writes[0].Address == 0xFFFFFFF2
            && secondChildZero.RootGate.Writes[0].Value == 0x12345678
            && secondChildZero.FirstChildTest!.FreshEDXPlus14Address == 0x10
            && secondChildZero.SecondChildTest!.FreshEDXPlus10Address == 0xC
            && secondChildZero.Reads[2] is
            {
                Address: 0x10,
                ValueCaptured: true,
                Value: 0
            }
            && secondChildZero.Reads[3] is
            {
                Address: 0xC,
                ValueCaptured: true,
                Value: 0
            }
            && secondChildZero.Writes.Length == 3
            && secondChildZero.Writes[0] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.RootGate,
                Address: 0xFFFFFFF2,
                Value: 0x12345678,
                Committed: true
            }
            && secondChildZero.Writes[1] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.FirstChild,
                Address: 0xFFFFFFFE,
                Value: 0x10,
                Committed: true
            }
            && secondChildZero.Writes[2] is
            {
                Stage: CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.SecondChild,
                Address: 0xFFFFFFFE,
                Value: 0xC,
                Committed: true
            }
            && secondChildZero.Writes[1].Address == secondChildZero.Writes[2].Address
            && secondChildZero.Writes[1].Value != secondChildZero.Writes[2].Value
            && secondChildZero.SecondChildTest.ZeroBranchTaken == true
            && secondChildZero.BranchSelectedDestinationRawFileOffset == 0x012C9226
            && secondChildZero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && secondChildZero.FirstChildTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondChildZero.ExactCleanExitAdmitted
            && secondChildZero.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan secondChildNonzero =
            Project(0x200, 0x300, 0x200, 0x80000000, 0, 0x80000001);
        check("The reached second TEST supplies all stop flags and a nonzero falls through to 91F0",
            secondChildNonzero.Outcome
                == CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .SecondChildNonzeroToFallthrough
            && secondChildNonzero.FirstChildTest!.TestEAXFlags is { Zero: true }
            && secondChildNonzero.SecondChildTest!.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondChildNonzero.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && secondChildNonzero.SecondChildTest.ZeroBranchTaken == false
            && secondChildNonzero.BranchSelectedDestinationRawFileOffset == 0x012C91F0
            && secondChildNonzero.ExactCleanExitAdmitted
            && secondChildNonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan rootStoreAliasesFirstFresh =
            Project(0x100, 0x124, 0x100, 0xDEADBEEF, 0, 0xCAFE);
        check("First child fresh data remains independent when its address aliases the root store",
            rootStoreAliasesFirstFresh.RootGate.Writes[0].Address == 0x114
            && rootStoreAliasesFirstFresh.Reads[2].Address
                == rootStoreAliasesFirstFresh.RootGate.Writes[0].Address
            && rootStoreAliasesFirstFresh.Reads[2].Value == 0
            && rootStoreAliasesFirstFresh.RootGate.Writes[0].Value == 0xDEADBEEF
            && rootStoreAliasesFirstFresh.FirstChildTest!.FreshEAX == 0
            && rootStoreAliasesFirstFresh.SecondChildTest!.FreshEAX == 0xCAFE
            && rootStoreAliasesFirstFresh.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan rootStoreAliasesSecondFresh =
            Project(0x100, 0x120, 0x100, 0xDEADBEEF, 0, 0xCAFE);
        check("Second child fresh data remains independent when its address aliases the root store",
            rootStoreAliasesSecondFresh.RootGate.Writes[0].Address == 0x110
            && rootStoreAliasesSecondFresh.Reads[3].Address
                == rootStoreAliasesSecondFresh.RootGate.Writes[0].Address
            && rootStoreAliasesSecondFresh.Reads[3].Value == 0xCAFE
            && rootStoreAliasesSecondFresh.RootGate.Writes[0].Value == 0xDEADBEEF
            && rootStoreAliasesSecondFresh.SecondChildTest!.FreshEAX == 0xCAFE
            && rootStoreAliasesSecondFresh.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan comparisonFlags =
            Project(1, 0x100, 0x80000000, 0, null, null);
        check("Root CMP flags preserve carry parity auxiliary sign and overflow semantics",
            comparisonFlags.RootGate.CompareResult == 0x7FFFFFFF
            && comparisonFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && comparisonFlags.RootGate.BranchTaken == false
            && comparisonFlags.StopAtRawFileOffsetExclusive == 0x012C91A8);
    }

    private static CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan Project(
        uint? entryEDX,
        uint? entryEBP,
        uint? freshEdxPlusC,
        uint? freshEdxPlus18,
        uint? firstFreshEdxPlus14,
        uint? secondFreshEdxPlus10) =>
        CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.Project(new(
            entryEDX,
            entryEBP,
            freshEdxPlusC,
            freshEdxPlus18,
            firstFreshEdxPlus14,
            secondFreshEdxPlus10));

    private static bool InstructionEvidenceMatches()
    {
        var instructions =
            CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.Instructions;
        string bytes = string.Concat(instructions.Select(instruction => Normalize(instruction.Bytes)));
        return bytes == Normalize(
                CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection.CapturedWindowBytes)
            && instructions.Select(instruction =>
                (instruction.RawFileOffset,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
                .SequenceEqual(new (
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
                    (0x012C91E1, "75 0D", 0x012C91F0, 0x016C9DF0),
                    (0x012C91E3, "8B 42 10", null, null),
                    (0x012C91E6, "8D 4A 10", null, null),
                    (0x012C91E9, "89 4D FC", null, null),
                    (0x012C91EC, "85 C0", null, null),
                    (0x012C91EE, "74 36", 0x012C9226, 0x016C9E26)
                })
            && instructions.All(instruction =>
                instruction.FlatIdaAddress == instruction.RawFileOffset
                && instruction.VirtualAddress
                    == CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                && (instruction.BranchTargetRawFileOffset is not uint targetRaw
                    || instruction.BranchTargetVirtualAddress
                        == CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)));
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
