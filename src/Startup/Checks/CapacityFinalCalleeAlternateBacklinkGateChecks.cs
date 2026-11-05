using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateBacklinkGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete parent-fresh window and both branch destinations match",
            CapacityFinalCalleeAlternateBacklinkGateProjection.EntryRawFileOffset
                == 0x012C9151
            && CapacityFinalCalleeAlternateBacklinkGateProjection.EntryVirtualAddress
                == 0x016C9D51
            && CapacityFinalCalleeAlternateBacklinkGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C915A
            && CapacityFinalCalleeAlternateBacklinkGateProjection.OpaqueCallRawFileOffset
                == 0x012C915A
            && CapacityFinalCalleeAlternateBacklinkGateProjection.OpaqueCallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeAlternateBacklinkGateProjection.BelowDestinationRawFileOffset
                == 0x012C915A
            && CapacityFinalCalleeAlternateBacklinkGateProjection.UnequalDestinationRawFileOffset
                == 0x012C915A
            && CapacityFinalCalleeAlternateBacklinkGateProjection.EqualDestinationRawFileOffset
                == 0x012C9160
            && Normalize(CapacityFinalCalleeAlternateBacklinkGateProjection.CapturedWindowBytes)
                == "3BCB720539510C7406"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateBacklinkGatePlan belowClean =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0x10,
                EntryEBX: 0x20,
                EntryEDX: null,
                FreshFieldAtEcxPlusC: null));
        check("Below takes JB cleanly without demanding later operands or entering the call",
            belowClean.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.BelowToOpaqueCallBoundary
            && belowClean.FirstCompareLeft == 0x10
            && belowClean.FirstCompareRight == 0x20
            && belowClean.FirstCompareResult == 0xFFFFFFF0
            && belowClean.FirstCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowClean.JbReached
            && belowClean.JbTaken == true
            && belowClean.JbSelectedDestinationRawFileOffset == 0x012C915A
            && !belowClean.EntryEDXReadReached
            && !belowClean.FreshFieldReadReached
            && !belowClean.SecondCompareReached
            && !belowClean.JzReached
            && belowClean.ExactCleanExitAdmitted
            && !belowClean.ExactCleanBypassAdmitted
            && belowClean.StopAtRawFileOffsetExclusive == 0x012C915A
            && belowClean.Writes.IsEmpty
            && !belowClean.OpaqueCallReached);

        CapacityFinalCalleeAlternateBacklinkGatePlan belowContaminated =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0,
                EntryEBX: 1,
                EntryEDX: 0x1234,
                FreshFieldAtEcxPlusC: 0x5678));
        check("Below flags and rejects supplied-but-unused EDX and fresh-field captures",
            belowContaminated.JbTaken == true
            && belowContaminated.UnusedEntryEDXCaptureProvided
            && belowContaminated.UnusedFreshFieldCaptureProvided
            && !belowContaminated.EntryEDXReadReached
            && !belowContaminated.FreshFieldReadReached
            && !belowContaminated.ExactCleanExitAdmitted
            && belowContaminated.StopAtRawFileOffsetExclusive == 0x012C915A
            && belowContaminated.Writes.IsEmpty
            && !belowContaminated.OpaqueCallReached);

        CapacityFinalCalleeAlternateBacklinkGatePlan equalFirstAndSecond =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 7,
                EntryEBX: 7,
                EntryEDX: 0xAABBCCDD,
                FreshFieldAtEcxPlusC: 0xAABBCCDD));
        check("Equal first compare proceeds and equal fresh comparison bypasses exactly",
            equalFirstAndSecond.FirstCompareResult == 0
            && equalFirstAndSecond.FirstCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalFirstAndSecond.JbTaken == false
            && equalFirstAndSecond.JbSelectedDestinationRawFileOffset == 0x012C9155
            && equalFirstAndSecond.EntryEDXReadReached
            && equalFirstAndSecond.FreshFieldReadReached
            && equalFirstAndSecond.FreshFieldAddress == 0x13
            && equalFirstAndSecond.SecondCompareLeft == 0xAABBCCDD
            && equalFirstAndSecond.SecondCompareRight == 0xAABBCCDD
            && equalFirstAndSecond.SecondCompareResult == 0
            && equalFirstAndSecond.SecondCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalFirstAndSecond.JzReached
            && equalFirstAndSecond.JzTaken == true
            && equalFirstAndSecond.JzSelectedDestinationRawFileOffset == 0x012C9160
            && equalFirstAndSecond.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget
            && equalFirstAndSecond.ExactCleanExitAdmitted
            && equalFirstAndSecond.ExactCleanBypassAdmitted
            && equalFirstAndSecond.StopAtRawFileOffsetExclusive == 0x012C9160
            && equalFirstAndSecond.Writes.IsEmpty
            && !equalFirstAndSecond.OpaqueCallReached);

        CapacityFinalCalleeAlternateBacklinkGatePlan aboveFirstAndUnequal =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 8,
                EntryEBX: 7,
                EntryEDX: 3,
                FreshFieldAtEcxPlusC: 2));
        check("Above first compare reaches unequal fresh CMP and stops at call boundary",
            aboveFirstAndUnequal.JbTaken == false
            && aboveFirstAndUnequal.FirstCompareFlags is { Carry: false, Zero: false }
            && aboveFirstAndUnequal.SecondCompareLeft == 2
            && aboveFirstAndUnequal.SecondCompareRight == 3
            && aboveFirstAndUnequal.SecondCompareResult == 0xFFFFFFFF
            && aboveFirstAndUnequal.SecondCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && aboveFirstAndUnequal.JzTaken == false
            && aboveFirstAndUnequal.JzSelectedDestinationRawFileOffset == 0x012C915A
            && aboveFirstAndUnequal.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.UnequalToOpaqueCallBoundary
            && aboveFirstAndUnequal.ExactCleanExitAdmitted
            && !aboveFirstAndUnequal.ExactCleanBypassAdmitted
            && aboveFirstAndUnequal.StopAtRawFileOffsetExclusive == 0x012C915A
            && aboveFirstAndUnequal.Writes.IsEmpty
            && !aboveFirstAndUnequal.OpaqueCallReached);

        CapacityFinalCalleeAlternateBacklinkGatePlan missingEcx =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: null,
                EntryEBX: 1,
                EntryEDX: 2,
                FreshFieldAtEcxPlusC: 3));
        check("Missing ECX gates the first CMP and rejects later supplied captures",
            missingEcx.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryECX
            && !missingEcx.FirstCompareReached
            && missingEcx.UnreachedEntryEBXCaptureProvided
            && missingEcx.LateEntryEDXCaptureProvided
            && missingEcx.LateFreshFieldCaptureProvided
            && !missingEcx.ExactCleanExitAdmitted
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeAlternateBacklinkGatePlan missingEbx =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 1,
                EntryEBX: null,
                EntryEDX: 2,
                FreshFieldAtEcxPlusC: 3));
        check("Missing EBX gates the first CMP and leaves later values unreached",
            missingEbx.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEBX
            && missingEbx.EntryECXCaptured
            && !missingEbx.FirstCompareReached
            && missingEbx.LateEntryEDXCaptureProvided
            && missingEbx.LateFreshFieldCaptureProvided
            && !missingEbx.ExactCleanExitAdmitted
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeAlternateBacklinkGatePlan missingEdx =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0x100,
                EntryEBX: 0x100,
                EntryEDX: null,
                FreshFieldAtEcxPlusC: 4));
        check("Missing EDX prevents the second CMP while retaining the reached fresh read",
            missingEdx.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEDX
            && missingEdx.FirstCompareReached
            && missingEdx.FirstCompareFlags is { Zero: true, Carry: false }
            && !missingEdx.EntryEDXCaptured
            && missingEdx.FreshFieldReadReached
            && missingEdx.FreshFieldAddress == 0x10C
            && missingEdx.FreshFieldCaptured
            && !missingEdx.SecondCompareReached
            && !missingEdx.JzReached
            && !missingEdx.ExactCleanExitAdmitted
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C9155);

        CapacityFinalCalleeAlternateBacklinkGatePlan missingFresh =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0x100,
                EntryEBX: 0x100,
                EntryEDX: 4,
                FreshFieldAtEcxPlusC: null));
        check("Missing fresh field retains the earlier CMP flags and address",
            missingFresh.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingFreshFieldAtEcxPlusC
            && missingFresh.FirstCompareReached
            && missingFresh.FirstCompareResult == 0
            && missingFresh.FirstCompareFlags is { Zero: true, Carry: false }
            && missingFresh.EntryEDXCaptured
            && missingFresh.FreshFieldReadReached
            && missingFresh.FreshFieldAddress == 0x10C
            && !missingFresh.FreshFieldCaptured
            && missingFresh.FirstCompareFlags.HasValue
            && !missingFresh.SecondCompareReached
            && !missingFresh.JzReached
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9155);

        CapacityFinalCalleeAlternateBacklinkGatePlan wrappedZeroAddress =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0xFFFFFFF4,
                EntryEBX: 0,
                EntryEDX: 0x80000000,
                FreshFieldAtEcxPlusC: 0x7FFFFFFF));
        check("ECX+C wraps to zero and the second CMP models signed overflow and all flags",
            CapacityFinalCalleeAlternateBacklinkGateProjection.DeriveEcxPlusCAddress(
                0xFFFFFFF4) == 0
            && wrappedZeroAddress.FreshFieldAddress == 0
            && wrappedZeroAddress.SecondCompareResult == 0xFFFFFFFF
            && wrappedZeroAddress.SecondCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && wrappedZeroAddress.JzTaken == false);

        CapacityFinalCalleeAlternateBacklinkGatePlan signedOverflow =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: 0x80000000,
                EntryEBX: 0x7FFFFFFF,
                EntryEDX: 0xFFFFFFFF,
                FreshFieldAtEcxPlusC: 0x7FFFFFFF));
        check("First CMP models signed overflow without confusing JB's carry condition",
            signedOverflow.FirstCompareResult == 1
            && signedOverflow.FirstCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.JbTaken == false
            && signedOverflow.SecondCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && signedOverflow.JzTaken == false
            && signedOverflow.ExactCleanExitAdmitted);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9151, "3B CB", null, null),
            (0x012C9153, "72 05", 0x012C915A, 0x016C9D5A),
            (0x012C9155, "39 51 0C", null, null),
            (0x012C9158, "74 06", 0x012C9160, 0x016C9D60)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateBacklinkGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateBacklinkGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateBacklinkGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateBacklinkGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateBacklinkGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateBacklinkGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateBacklinkGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateBacklinkGateProjection.Instructions
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
