using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargePairedFieldTestLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The paired window joins the first bounded gate to the second-field gate",
            CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.EntryRawFileOffset
                == 0x012C91D6
            && CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.EntryVirtualAddress
                == 0x016C9DD6
            && CapacityFinalCalleeLargePairedFieldTestLifecycleProjection
                .FirstFieldNonzeroDestinationRawFileOffset == 0x012C91F0
            && CapacityFinalCalleeLargePairedFieldTestLifecycleProjection
                .SecondFieldZeroDestinationRawFileOffset == 0x012C9226
            && CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.CapturedWindowBytes)
                == "8B42148D4A14894DFC85C0750D8B42108D4A10894DFC85C07436"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan missingEntryEDX =
            Project(null, 0x800, 0x12, 0x34);
        check("Missing parent EDX stops at the first read and contaminates exit with unused captures",
            missingEntryEDX.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingEntryEDXForFirstRead
            && missingEntryEDX.FirstFieldTest.Reads.Length == 1
            && missingEntryEDX.FirstFieldTest.Reads[0] is
            {
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && missingEntryEDX.ReachedInstructions.SequenceEqual(new[] { 0x012C91D6u })
            && missingEntryEDX.Writes.IsEmpty
            && !missingEntryEDX.SecondFieldTestReached
            && missingEntryEDX.SecondFieldTest is null
            && !missingEntryEDX.FirstFieldCleanExitAdmitted
            && missingEntryEDX.UnreachedSecondFreshEAXCaptureProvided
            && !missingEntryEDX.ExactCleanExitAdmitted
            && missingEntryEDX.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan missingFirstFresh =
            Project(0x100, 0x800, null, 0x34);
        check("Missing first fresh EAX stops before parent store and leaves second capture unused",
            missingFirstFresh.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingFirstFreshEAX
            && missingFirstFresh.FirstFieldTest.Reads.Length == 1
            && missingFirstFresh.FirstFieldTest.Reads[0] is
            {
                Address: 0x114,
                ValueCaptured: false,
                Value: null
            }
            && missingFirstFresh.Writes.IsEmpty
            && !missingFirstFresh.FirstFieldTest.TestEAXReached
            && !missingFirstFresh.SecondFieldTestReached
            && missingFirstFresh.UnreachedSecondFreshEAXCaptureProvided
            && !missingFirstFresh.ExactCleanExitAdmitted
            && missingFirstFresh.StopAtRawFileOffsetExclusive == 0x012C91D6);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan missingParentEBP =
            Project(0x100, null, 0, 0x34);
        check("Missing parent EBP stops at the first store without reaching either test or child",
            missingParentEBP.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingEntryEBPForFirstStore
            && missingParentEBP.FirstFieldTest.LEAECXValue == 0x114
            && missingParentEBP.FirstFieldTest.Writes.Length == 1
            && !missingParentEBP.FirstFieldTest.Writes[0].Completed
            && !missingParentEBP.FirstFieldTest.TestEAXReached
            && !missingParentEBP.SecondFieldTestReached
            && missingParentEBP.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu
            })
            && missingParentEBP.UnreachedSecondFreshEAXCaptureProvided
            && !missingParentEBP.ExactCleanExitAdmitted
            && missingParentEBP.StopAtRawFileOffsetExclusive == 0x012C91DC);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan parentNonzero =
            Project(0x100, 0x800, 1, 0x34);
        check("The first nonzero branch stops at 91F0 and rejects the unused second capture",
            parentNonzero.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.FirstFreshEAXNonzeroToTarget
            && parentNonzero.FirstFieldTest.NonzeroBranchTaken == true
            && parentNonzero.FirstFieldTest.NonzeroBranchSelectedDestinationRawFileOffset
                == 0x012C91F0
            && parentNonzero.FirstFieldTest.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && !parentNonzero.SecondFieldTestReached
            && parentNonzero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu,
                0x012C91DFu,
                0x012C91E1u
            })
            && parentNonzero.UnreachedSecondFreshEAXCaptureProvided
            && !parentNonzero.ExactCleanExitAdmitted
            && parentNonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan boundedParentNonzero =
            Project(0x100, 0x800, 1, null);
        check("First-field nonzero is a clean bounded exit at 91F0 when no child capture is supplied",
            boundedParentNonzero.FirstFieldCleanExitAdmitted
            && !boundedParentNonzero.SecondFieldTestReached
            && !boundedParentNonzero.UnreachedSecondFreshEAXCaptureProvided
            && boundedParentNonzero.ExactCleanExitAdmitted
            && boundedParentNonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan missingSecondFresh =
            Project(0x100, 0x800, 0, null);
        check("The clean zero parent reaches the second read and retains first TEST flags on missing data",
            missingSecondFresh.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingSecondFreshEAX
            && missingSecondFresh.FirstFieldCleanExitAdmitted
            && missingSecondFresh.SecondFieldTestReached
            && missingSecondFresh.SecondFieldEntryEDXRetained
            && missingSecondFresh.SecondFieldEntryEDX == 0x100
            && missingSecondFresh.SecondFieldTest!.Capture.EntryEDX == 0x100
            && missingSecondFresh.SecondFieldTest.Capture.EntryEBP is null
            && missingSecondFresh.FirstFieldTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && missingSecondFresh.SecondFieldTest.TestEAXFlags is null
            && missingSecondFresh.FlagsAtStop is { Zero: true, AuxiliaryCarry: null }
            && missingSecondFresh.Reads.Length == 2
            && missingSecondFresh.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargePairedFieldTestLifecycleStage.SecondField,
                RawFileOffset: 0x012C91E3,
                Address: 0x110,
                ValueCaptured: false,
                Value: null
            }
            && missingSecondFresh.Writes.Length == 1
            && missingSecondFresh.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu,
                0x012C91DFu,
                0x012C91E1u,
                0x012C91E3u
            })
            && !missingSecondFresh.SecondFieldEntryEBPRetained
            && !missingSecondFresh.ExactCleanExitAdmitted
            && missingSecondFresh.StopAtRawFileOffsetExclusive == 0x012C91E3);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan secondZero =
            Project(0xFFFFFFEC, 2, 0, 0);
        check("Two clean stores repeat EBP-4 with ordered distinct LEA values and zero reaches 9226",
            secondZero.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.SecondFreshEAXZeroToTarget
            && secondZero.FirstFieldCleanExitAdmitted
            && secondZero.SecondFieldTest!.Outcome
                == CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXZeroToTarget
            && secondZero.FirstFieldTest.FreshEDXPlus14Address == 0
            && secondZero.SecondFieldTest.FreshEDXPlus10Address == 0xFFFFFFFC
            && secondZero.Reads.Length == 2
            && secondZero.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField,
                Address: 0,
                ValueCaptured: true,
                Value: 0
            }
            && secondZero.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargePairedFieldTestLifecycleStage.SecondField,
                Address: 0xFFFFFFFC,
                ValueCaptured: true,
                Value: 0
            }
            && secondZero.Writes.Length == 2
            && secondZero.Writes[0] is
            {
                Stage: CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField,
                Address: 0xFFFFFFFE,
                Value: 0,
                Completed: true
            }
            && secondZero.Writes[1] is
            {
                Stage: CapacityFinalCalleeLargePairedFieldTestLifecycleStage.SecondField,
                Address: 0xFFFFFFFE,
                Value: 0xFFFFFFFC,
                Completed: true
            }
            && secondZero.Writes[0].Address == secondZero.Writes[1].Address
            && secondZero.Writes[0].Value != secondZero.Writes[1].Value
            && secondZero.FirstFieldTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondZero.SecondFieldTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondZero.SecondFieldTest.ZeroBranchTaken == true
            && secondZero.SecondFieldTest.ZeroBranchSelectedDestinationRawFileOffset == 0x012C9226
            && secondZero.SecondFieldEntryEBPRetained
            && secondZero.SecondFieldEntryEBP == 2
            && secondZero.ExactCleanExitAdmitted
            && secondZero.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan secondNonzero =
            Project(0x200, 0x300, 0, 0x80000001);
        check("Both reached TESTs retain their flags and second nonzero falls through to 91F0",
            secondNonzero.Outcome
                == CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome
                    .SecondFreshEAXNonzeroToFallthrough
            && secondNonzero.FirstFieldTest.TestEAXFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondNonzero.SecondFieldTest!.TestEAXFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: true,
                Overflow: false,
                AuxiliaryCarry: null
            }
            && secondNonzero.SecondFieldTest.ZeroBranchTaken == false
            && secondNonzero.SecondFieldTest.ZeroBranchSelectedDestinationRawFileOffset
                == 0x012C91F0
            && secondNonzero.SecondFieldTest.StoreAddress == 0x2FC
            && secondNonzero.SecondFieldTest.StoreValue == 0x210
            && secondNonzero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C91D6u,
                0x012C91D9u,
                0x012C91DCu,
                0x012C91DFu,
                0x012C91E1u,
                0x012C91E3u,
                0x012C91E6u,
                0x012C91E9u,
                0x012C91ECu,
                0x012C91EEu
            })
            && secondNonzero.ExactCleanExitAdmitted
            && secondNonzero.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan freshAlias =
            Project(0x100, 0x114, 0, 0xDEADBEEF);
        check("Second fresh read remains independent when it aliases the first store address",
            freshAlias.Reads.Length == 2
            && freshAlias.Writes.Length == 2
            && freshAlias.Reads[0].Value == 0
            && freshAlias.Reads[1].Address == freshAlias.Writes[0].Address
            && freshAlias.Reads[1].Value == 0xDEADBEEF
            && freshAlias.Writes[0].Value == 0x114
            && freshAlias.SecondFieldTest!.FreshEAX == 0xDEADBEEF
            && freshAlias.SecondFieldTest.LEAECXValue == 0x110
            && freshAlias.Writes[1].Address == freshAlias.Writes[0].Address
            && freshAlias.Writes[1].Value == 0x110
            && freshAlias.Writes[1].Value != freshAlias.Reads[1].Value
            && freshAlias.ExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeLargePairedFieldTestLifecyclePlan Project(
        uint? entryEDX,
        uint? entryEBP,
        uint? firstFreshEAX,
        uint? secondFreshEAX) =>
        CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.Project(new(
            entryEDX,
            entryEBP,
            firstFreshEAX,
            secondFreshEAX));

    private static bool InstructionEvidenceMatches()
    {
        uint nextRawOffset =
            CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargePairedFieldTestLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargePairedFieldTestLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargePairedFieldTestLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset == 0x012C91F0
            && bytes == Normalize(
                CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(new (
                    uint Raw,
                    string Bytes,
                    uint? BranchTargetRaw,
                    uint? BranchTargetVirtual)[] {
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
                });
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
