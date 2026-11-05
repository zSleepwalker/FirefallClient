using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRootSizeLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The root and size path bytes and raw-to-virtual mapping match",
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.EntryRawFileOffset
                == 0x012C92C8
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.EntryVirtualAddress
                == 0x016C9EC8
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.ParentBoundaryRawFileOffsetExclusive
                == 0x012C92D0
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.ChildEntryRawFileOffset
                == 0x012C92EC
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.ChildBoundaryRawFileOffsetExclusive
                == 0x012C92F6
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.EqualFallthroughRawFileOffset
                == 0x012C92D0
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.UnequalBranchTargetRawFileOffset
                == 0x012C92EC
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.SizeFallthroughRawFileOffset
                == 0x012C92F6
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.SizeBranchTargetRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.FreshGlobalDWORDVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRootSizeLifecycleProjection.EntryRawFileOffset)
                == 0x016C9EC8
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRootSizeLifecycleProjection.ChildEntryRawFileOffset)
                == 0x016C9EEC
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRootSizeLifecycleProjection.SizeFallthroughRawFileOffset)
                == 0x016C9EF6
            && CapacityFinalCalleeCommonRootSizeLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRootSizeLifecycleProjection.SizeBranchTargetRawFileOffset)
                == 0x016C9F54
            && Normalize(CapacityFinalCalleeCommonRootSizeLifecycleProjection.CapturedWindowBytes)
                == "3B35DCADED01751C8BCFC1E90383F920735E"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonRootSizeLifecyclePlan allMissing =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: null,
                CallerEDI: null));
        check("All missing captures preserve the independent global read and stop at 92C8",
            allMissing.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome
                    .MissingEntryESIAndFreshGlobalDWORD
            && allMissing.FreshGlobalReadReached
            && allMissing.FreshGlobalAddress == 0x01EDADDC
            && !allMissing.FreshGlobalCaptured
            && allMissing.FreshGlobalDWORD is null
            && allMissing.Parent.CompareFlags is null
            && allMissing.Child is null
            && !allMissing.CallerEDICaptureReached
            && !allMissing.CallerEDICaptureProvidedButUnreached
            && !allMissing.ExactCleanParentAdmission
            && !allMissing.ExactCleanBranchToSizeGate
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.FlagsAtStop is null
            && allMissing.Writes.IsEmpty
            && !allMissing.TargetEntered
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan missingESIWithCapturedZeroGlobal =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: 0,
                CallerEDI: 0));
        check("A captured zero global read remains independent when ESI is missing and cannot be bypassed",
            missingESIWithCapturedZeroGlobal.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .MissingParentWithUnusedCallerEDI
            && missingESIWithCapturedZeroGlobal.Parent.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
            && !missingESIWithCapturedZeroGlobal.Parent.EntryESICaptured
            && missingESIWithCapturedZeroGlobal.FreshGlobalReadReached
            && missingESIWithCapturedZeroGlobal.FreshGlobalCaptured
            && missingESIWithCapturedZeroGlobal.FreshGlobalDWORD == 0
            && missingESIWithCapturedZeroGlobal.Child is null
            && missingESIWithCapturedZeroGlobal.CallerEDICaptureProvidedButUnreached
            && !missingESIWithCapturedZeroGlobal.ExactCleanExitAdmitted
            && missingESIWithCapturedZeroGlobal.StopAtRawFileOffsetExclusive
                == 0x012C92C8);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan missingGlobalWithCallerEDI =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: null,
                CallerEDI: 0x100));
        check("Caller EDI cannot bypass a missing fresh global read",
            missingGlobalWithCallerEDI.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .MissingParentWithUnusedCallerEDI
            && missingGlobalWithCallerEDI.Parent.EntryESICaptured
            && missingGlobalWithCallerEDI.Parent.FreshGlobalReadReached
            && !missingGlobalWithCallerEDI.Parent.FreshGlobalCaptured
            && !missingGlobalWithCallerEDI.Parent.CompareReached
            && missingGlobalWithCallerEDI.Child is null
            && missingGlobalWithCallerEDI.CallerEDICaptureProvidedButUnreached
            && !missingGlobalWithCallerEDI.ExactCleanBranchToSizeGate
            && !missingGlobalWithCallerEDI.ExactCleanExitAdmitted
            && missingGlobalWithCallerEDI.StopAtRawFileOffsetExclusive
                == 0x012C92C8);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan cleanEqual =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0x42,
                FreshGlobalDWORD: 0x42,
                CallerEDI: null));
        check("A clean equal root comparison is bounded at 92D0 without entering the size gate",
            cleanEqual.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome.EqualToFallthrough92D0
            && cleanEqual.ExactCleanParentAdmission
            && !cleanEqual.ExactCleanBranchToSizeGate
            && cleanEqual.Parent.JnzTaken == false
            && cleanEqual.Parent.JnzSelectedDestinationRawFileOffset == 0x012C92D0
            && cleanEqual.Parent.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && cleanEqual.Child is null
            && !cleanEqual.CallerEDICaptureReached
            && cleanEqual.ExactCleanExitAdmitted
            && cleanEqual.ParentCompareFlagsPreservedAtStop
            && cleanEqual.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan equalWithUnusedEDI =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 7,
                FreshGlobalDWORD: 7,
                CallerEDI: 0));
        check("Captured unused EDI contaminates equal admission while parent CMP flags remain intact",
            equalWithUnusedEDI.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .EqualWithUnusedCallerEDI
            && equalWithUnusedEDI.ExactCleanParentAdmission
            && equalWithUnusedEDI.CallerEDICaptureProvidedButUnreached
            && !equalWithUnusedEDI.CallerEDICaptureReached
            && equalWithUnusedEDI.Child is null
            && equalWithUnusedEDI.Parent.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalWithUnusedEDI.FlagsAtStop
                == new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    false, true, false, true, false, false)
            && equalWithUnusedEDI.ParentCompareFlagsPreservedAtStop
            && !equalWithUnusedEDI.ExactCleanExitAdmitted
            && equalWithUnusedEDI.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan unequalMissingEDI =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1,
                CallerEDI: null));
        check("A clean unequal parent enters only the bounded child MOV and preserves parent flags if EDI is absent",
            unequalMissingEDI.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .UnequalMissingCallerEDI
            && unequalMissingEDI.ExactCleanParentAdmission
            && unequalMissingEDI.ExactCleanBranchToSizeGate
            && unequalMissingEDI.Parent.JnzTaken == true
            && unequalMissingEDI.Parent.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && unequalMissingEDI.Child?.Outcome
                == CapacityFinalCalleeCommonTailSizeGateOutcome.MissingEntryEDI
            && !unequalMissingEDI.Child!.MoveReached
            && !unequalMissingEDI.Child!.ShiftReached
            && !unequalMissingEDI.Child!.CompareReached
            && !unequalMissingEDI.Child!.JnbReached
            && unequalMissingEDI.FlagsAtStop
                == new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    true, true, true, false, true, false)
            && unequalMissingEDI.ParentCompareFlagsPreservedAtStop
            && !unequalMissingEDI.ExactCleanExitAdmitted
            && unequalMissingEDI.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan belowUnsigned =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1,
                CallerEDI: 0xFF));
        check("Unequal root path retains captured EDI through SHR and takes unsigned-below 92F6",
            belowUnsigned.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .BelowUnsignedThresholdToFallthrough92F6
            && belowUnsigned.ExactCleanBranchToSizeGate
            && belowUnsigned.CallerEDICaptureReached
            && belowUnsigned.CallerEDIRetained
            && belowUnsigned.RetainedCallerEDI == 0xFF
            && belowUnsigned.Child?.ShiftResult == 0x1F
            && belowUnsigned.ChildShiftFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && belowUnsigned.ChildCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowUnsigned.FlagsAtStop == belowUnsigned.ChildCompareFlags
            && !belowUnsigned.ParentCompareFlagsPreservedAtStop
            && belowUnsigned.Child?.JnbTaken == false
            && belowUnsigned.Child?.JnbSelectedDestinationRawFileOffset == 0x012C92F6
            && belowUnsigned.ExactCleanExitAdmitted
            && !belowUnsigned.TargetEntered
            && belowUnsigned.Writes.IsEmpty
            && belowUnsigned.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan capturedZeroEDI =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0,
                FreshGlobalDWORD: 1,
                CallerEDI: 0));
        check("Captured zero EDI is distinct from missing and takes unsigned-below 92F6",
            capturedZeroEDI.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .BelowUnsignedThresholdToFallthrough92F6
            && capturedZeroEDI.CallerEDICaptureReached
            && capturedZeroEDI.CallerEDIRetained
            && capturedZeroEDI.RetainedCallerEDI == 0
            && capturedZeroEDI.Child?.EntryEDICaptured == true
            && capturedZeroEDI.Child?.ShiftInput == 0
            && capturedZeroEDI.Child?.ShiftResult == 0
            && capturedZeroEDI.ExactCleanExitAdmitted
            && capturedZeroEDI.StopAtRawFileOffsetExclusive == 0x012C92F6);

        CapacityFinalCalleeCommonRootSizeLifecyclePlan atOrAboveUnsigned =
            CapacityFinalCalleeCommonRootSizeLifecycleProjection.Project(new(
                EntryESI: 0x80000000,
                FreshGlobalDWORD: 1,
                CallerEDI: 0x100));
        check("Unequal root path uses unsigned threshold equality and stops at 9354",
            atOrAboveUnsigned.Outcome
                == CapacityFinalCalleeCommonRootSizeLifecycleOutcome
                    .AtOrAboveUnsignedThresholdToBranch9354
            && atOrAboveUnsigned.Parent.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && atOrAboveUnsigned.CallerEDICaptureReached
            && atOrAboveUnsigned.RetainedCallerEDI == 0x100
            && atOrAboveUnsigned.Child?.ShiftResult == 0x20
            && atOrAboveUnsigned.ChildShiftFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && atOrAboveUnsigned.ChildCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && atOrAboveUnsigned.FlagsAtStop == atOrAboveUnsigned.ChildCompareFlags
            && atOrAboveUnsigned.Child?.JnbTaken == true
            && atOrAboveUnsigned.Child?.JnbSelectedDestinationRawFileOffset == 0x012C9354
            && atOrAboveUnsigned.ExactCleanExitAdmitted
            && !atOrAboveUnsigned.TargetEntered
            && atOrAboveUnsigned.Writes.IsEmpty
            && atOrAboveUnsigned.StopAtRawFileOffsetExclusive == 0x012C9354);

        check("Parent CMP and child SHR/CMP expose every defined flag and branch boundary",
            unequalMissingEDI.Parent.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowUnsigned.ChildShiftFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && belowUnsigned.ChildCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowUnsigned.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonRootSizeLifecycleProjection
                    .ChildBoundaryRawFileOffsetExclusive
            && atOrAboveUnsigned.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonRootSizeLifecycleProjection
                    .SizeBranchTargetRawFileOffset
            && new[] { belowUnsigned, atOrAboveUnsigned }.All(plan =>
                plan.ExactCleanExitAdmitted
                && !plan.TargetEntered
                && plan.Writes.IsEmpty));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92C8, "3B 35 DC AD ED 01", null, null),
            (0x012C92CE, "75 1C", 0x012C92EC, 0x016C9EEC),
            (0x012C92EC, "8B CF", null, null),
            (0x012C92EE, "C1 E9 03", null, null),
            (0x012C92F1, "83 F9 20", null, null),
            (0x012C92F4, "73 5E", 0x012C9354, 0x016C9F54)
        };

        ImmutableArray<CapacityFinalCalleeCommonRootSizeLifecycleInstructionEvidence>
            instructions =
                CapacityFinalCalleeCommonRootSizeLifecycleProjection.Instructions;
        if (instructions.Length != expected.Length)
        {
            return false;
        }

        for (int index = 0; index < instructions.Length; index++)
        {
            CapacityFinalCalleeCommonRootSizeLifecycleInstructionEvidence instruction =
                instructions[index];
            var expectedInstruction = expected[index];
            uint? expectedTargetVirtual = expectedInstruction.BranchTargetRaw is uint targetRaw
                ? CapacityFinalCalleeCommonRootSizeLifecycleProjection
                    .RawFileOffsetToVirtualAddress(targetRaw)
                : null;
            if (instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != expectedInstruction.Raw
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonRootSizeLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || Normalize(instruction.Bytes) != Normalize(expectedInstruction.Bytes)
                || instruction.BranchTargetRawFileOffset
                    != expectedInstruction.BranchTargetRaw
                || instruction.BranchTargetVirtualAddress != expectedTargetVirtual
                || instruction.BranchTargetVirtualAddress
                    != expectedInstruction.BranchTargetVirtual)
            {
                return false;
            }
        }

        return string.Concat(instructions.Select(instruction =>
                Normalize(instruction.Bytes)))
            == Normalize(
                CapacityFinalCalleeCommonRootSizeLifecycleProjection
                    .CapturedWindowBytes);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
