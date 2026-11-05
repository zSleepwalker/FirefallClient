using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateSuccessorGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The byte window, raw/IDA mapping, call cell, and branch destinations match",
            CapacityFinalCalleeAlternateSuccessorGateProjection.EntryRawFileOffset
                == 0x012C917C
            && CapacityFinalCalleeAlternateSuccessorGateProjection.EntryVirtualAddress
                == 0x016C9D7C
            && CapacityFinalCalleeAlternateSuccessorGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9190
            && CapacityFinalCalleeAlternateSuccessorGateProjection.OpaqueCallRawFileOffset
                == 0x012C918A
            && CapacityFinalCalleeAlternateSuccessorGateProjection.OpaqueCallCellVirtualAddress
                == 0x01AC4A24
            && CapacityFinalCalleeAlternateSuccessorGateProjection.FrameEqualDestinationRawFileOffset
                == 0x012C9190
            && CapacityFinalCalleeAlternateSuccessorGateProjection.BelowDestinationRawFileOffset
                == 0x012C918A
            && CapacityFinalCalleeAlternateSuccessorGateProjection.FieldEqualDestinationRawFileOffset
                == 0x012C9190
            && CapacityFinalCalleeAlternateSuccessorGateProjection.FieldUnequalDestinationRawFileOffset
                == 0x012C918A
            && Normalize(CapacityFinalCalleeAlternateSuccessorGateProjection.CapturedWindowBytes)
                == "3B45F4740F3BC372053950087406FF15244AAC01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateSuccessorGatePlan frameEqual =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinusC: 0x55,
                EntryEAX: 0x55,
                EntryEBX: null,
                FreshFieldAtEaxPlus8: null,
                EntryEDX: null));
        check("First equality exits cleanly at 9190 without consuming later captures",
            frameEqual.FrameMinusCAddress == 0xF4
            && frameEqual.FrameMinusC == 0x55
            && frameEqual.FrameCompareLeft == 0x55
            && frameEqual.FrameCompareRight == 0x55
            && frameEqual.FrameCompareResult == 0
            && frameEqual.FrameCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && frameEqual.FrameJzReached
            && frameEqual.FrameJzTaken == true
            && frameEqual.FrameJzSelectedDestinationRawFileOffset == 0x012C9190
            && !frameEqual.RegisterCompareReached
            && !frameEqual.JbReached
            && !frameEqual.FreshFieldReadReached
            && !frameEqual.FieldCompareReached
            && !frameEqual.FieldJzReached
            && frameEqual.ExactCleanExitAdmitted
            && frameEqual.ExactCleanBypassAdmitted
            && frameEqual.StopAtRawFileOffsetExclusive == 0x012C9190
            && frameEqual.Writes.IsEmpty
            && !frameEqual.OpaqueCallReached);

        CapacityFinalCalleeAlternateSuccessorGatePlan below =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 1,
                EntryEAX: 0,
                EntryEBX: 1,
                FreshFieldAtEaxPlus8: null,
                EntryEDX: null));
        check("Unsigned-below branch exits cleanly at the opaque-call boundary",
            below.FrameCompareResult == 0xFFFFFFFF
            && below.FrameCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && below.FrameJzTaken == false
            && below.FrameJzSelectedDestinationRawFileOffset == 0x012C9181
            && below.RegisterCompareLeft == 0
            && below.RegisterCompareRight == 1
            && below.RegisterCompareResult == 0xFFFFFFFF
            && below.RegisterCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && below.JbReached
            && below.JbTaken == true
            && below.JbSelectedDestinationRawFileOffset == 0x012C918A
            && !below.FreshFieldReadReached
            && !below.FieldCompareReached
            && !below.FieldJzReached
            && below.ExactCleanExitAdmitted
            && !below.ExactCleanBypassAdmitted
            && below.StopAtRawFileOffsetExclusive == 0x012C918A
            && below.Writes.IsEmpty
            && !below.OpaqueCallReached);

        CapacityFinalCalleeAlternateSuccessorGatePlan fieldEqual =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinusC: 9,
                EntryEAX: 8,
                EntryEBX: 7,
                FreshFieldAtEaxPlus8: 0xAABBCCDD,
                EntryEDX: 0xAABBCCDD));
        check("Fresh-field equality independently exits cleanly at 9190",
            fieldEqual.FrameCompareResult == 0xFFFFFFFF
            && fieldEqual.FrameCompareFlags is { Carry: true, Zero: false }
            && fieldEqual.FrameJzTaken == false
            && fieldEqual.RegisterCompareResult == 1
            && fieldEqual.RegisterCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && fieldEqual.JbReached
            && fieldEqual.JbTaken == false
            && fieldEqual.JbSelectedDestinationRawFileOffset == 0x012C9185
            && fieldEqual.FreshFieldReadReached
            && fieldEqual.FreshFieldAddress == 0x10
            && fieldEqual.FreshFieldCaptured
            && fieldEqual.FieldCompareLeft == 0xAABBCCDD
            && fieldEqual.FieldCompareRight == 0xAABBCCDD
            && fieldEqual.FieldCompareResult == 0
            && fieldEqual.FieldCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && fieldEqual.FieldJzTaken == true
            && fieldEqual.FieldJzSelectedDestinationRawFileOffset == 0x012C9190
            && fieldEqual.ExactCleanExitAdmitted
            && fieldEqual.ExactCleanBypassAdmitted
            && fieldEqual.StopAtRawFileOffsetExclusive == 0x012C9190
            && fieldEqual.Writes.IsEmpty
            && !fieldEqual.OpaqueCallReached);

        CapacityFinalCalleeAlternateSuccessorGatePlan fieldUnequal =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x100,
                FreshFrameMinusC: 9,
                EntryEAX: 8,
                EntryEBX: 7,
                FreshFieldAtEaxPlus8: 2,
                EntryEDX: 3));
        check("Unequal fresh-field comparison falls through to the call boundary only",
            fieldUnequal.FieldCompareLeft == 2
            && fieldUnequal.FieldCompareRight == 3
            && fieldUnequal.FieldCompareResult == 0xFFFFFFFF
            && fieldUnequal.FieldCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && fieldUnequal.FieldJzReached
            && fieldUnequal.FieldJzTaken == false
            && fieldUnequal.FieldJzSelectedDestinationRawFileOffset == 0x012C918A
            && fieldUnequal.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .UnequalFreshFieldToOpaqueCallBoundary
            && fieldUnequal.ExactCleanExitAdmitted
            && !fieldUnequal.ExactCleanBypassAdmitted
            && fieldUnequal.StopAtRawFileOffsetExclusive == 0x012C918A
            && fieldUnequal.Writes.IsEmpty
            && !fieldUnequal.OpaqueCallReached);

        CapacityFinalCalleeAlternateSuccessorGatePlan frameEqualContaminated =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 4,
                EntryEAX: 4,
                EntryEBX: 5,
                FreshFieldAtEaxPlus8: 6,
                EntryEDX: 7));
        check("First equality rejects every supplied later-branch capture",
            frameEqualContaminated.FrameJzTaken == true
            && frameEqualContaminated.UnusedEntryEBXCaptureProvided
            && frameEqualContaminated.UnusedFreshFieldCaptureProvided
            && frameEqualContaminated.UnusedEntryEDXCaptureProvided
            && !frameEqualContaminated.RegisterCompareReached
            && !frameEqualContaminated.FreshFieldReadReached
            && !frameEqualContaminated.ExactCleanExitAdmitted
            && !frameEqualContaminated.ExactCleanBypassAdmitted
            && frameEqualContaminated.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeAlternateSuccessorGatePlan belowContaminated =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 1,
                EntryEAX: 0,
                EntryEBX: 1,
                FreshFieldAtEaxPlus8: 2,
                EntryEDX: 3));
        check("Unsigned-below rejects the supplied fresh-field and EDX captures",
            belowContaminated.JbTaken == true
            && belowContaminated.UnusedFreshFieldCaptureProvided
            && belowContaminated.UnusedEntryEDXCaptureProvided
            && !belowContaminated.FreshFieldReadReached
            && !belowContaminated.FieldCompareReached
            && !belowContaminated.ExactCleanExitAdmitted
            && belowContaminated.StopAtRawFileOffsetExclusive == 0x012C918A);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingEbp =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinusC: 0,
                EntryEAX: 0,
                EntryEBX: 0,
                FreshFieldAtEaxPlus8: 0,
                EntryEDX: 0));
        check("Missing EBP gates all reads and comparisons despite later zero captures",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingEntryEBP
            && !missingEbp.FrameMinusCReadReached
            && !missingEbp.FrameMinusCCaptured
            && !missingEbp.FrameCompareReached
            && !missingEbp.RegisterCompareReached
            && !missingEbp.FreshFieldReadReached
            && !missingEbp.FieldCompareReached
            && missingEbp.UnreachedFreshFrameMinusCCaptureProvided
            && missingEbp.UnreachedEntryEAXCaptureProvided
            && missingEbp.UnreachedEntryEBXCaptureProvided
            && missingEbp.UnreachedFreshFieldCaptureProvided
            && missingEbp.UnreachedEntryEDXCaptureProvided
            && missingEbp.LateEntryEDXCaptureProvided
            && !missingEbp.ExactCleanExitAdmitted
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingFrame =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinusC: null,
                EntryEAX: 0,
                EntryEBX: 0,
                FreshFieldAtEaxPlus8: 0,
                EntryEDX: 0));
        check("Missing frame-C read retains its address but blocks supplied later operands",
            missingFrame.FrameMinusCReadReached
            && missingFrame.FrameMinusCAddress == 4
            && !missingFrame.FrameMinusCCaptured
            && !missingFrame.FrameCompareReached
            && missingFrame.LateEntryEAXCaptureProvided
            && missingFrame.LateEntryEBXCaptureProvided
            && missingFrame.LateFreshFieldCaptureProvided
            && missingFrame.LateEntryEDXCaptureProvided
            && !missingFrame.RegisterCompareReached
            && !missingFrame.FieldCompareReached
            && !missingFrame.ExactCleanExitAdmitted
            && missingFrame.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingEax =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinusC: 1,
                EntryEAX: null,
                EntryEBX: 2,
                FreshFieldAtEaxPlus8: 3,
                EntryEDX: 4));
        check("Missing EAX preserves the fresh frame read and stops before its CMP",
            missingEax.FrameMinusCAddress == 4
            && missingEax.FrameMinusC == 1
            && !missingEax.EntryEAXCaptured
            && !missingEax.FrameCompareReached
            && missingEax.UnreachedEntryEBXCaptureProvided
            && missingEax.UnreachedFreshFieldCaptureProvided
            && missingEax.UnreachedEntryEDXCaptureProvided
            && !missingEax.RegisterCompareReached
            && !missingEax.ExactCleanExitAdmitted
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingEbx =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 1,
                EntryEAX: 0,
                EntryEBX: null,
                FreshFieldAtEaxPlus8: 2,
                EntryEDX: 3));
        check("Missing EBX retains the first CMP and rejects later field operands",
            missingEbx.FrameCompareReached
            && missingEbx.FrameCompareFlags is { Zero: false }
            && missingEbx.FrameJzTaken == false
            && !missingEbx.RegisterCompareReached
            && missingEbx.LateFreshFieldCaptureProvided
            && missingEbx.LateEntryEDXCaptureProvided
            && !missingEbx.FreshFieldReadReached
            && !missingEbx.ExactCleanExitAdmitted
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C9181);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingField =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 9,
                EntryEAX: 8,
                EntryEBX: 7,
                FreshFieldAtEaxPlus8: null,
                EntryEDX: 3));
        check("Missing EAX+8 data retains both prior comparisons and blocks the final CMP",
            missingField.FrameCompareReached
            && missingField.FrameCompareFlags is { Zero: false }
            && missingField.RegisterCompareReached
            && missingField.RegisterCompareFlags is { Carry: false }
            && missingField.JbTaken == false
            && missingField.FreshFieldReadReached
            && missingField.FreshFieldAddress == 0x10
            && !missingField.FreshFieldCaptured
            && !missingField.FieldCompareReached
            && missingField.LateEntryEDXCaptureProvided
            && !missingField.ExactCleanExitAdmitted
            && missingField.StopAtRawFileOffsetExclusive == 0x012C9185);

        CapacityFinalCalleeAlternateSuccessorGatePlan missingEdx =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 9,
                EntryEAX: 8,
                EntryEBX: 7,
                FreshFieldAtEaxPlus8: 4,
                EntryEDX: null));
        check("Missing EDX preserves the independent fresh field read and earlier flags",
            missingEdx.FrameCompareReached
            && missingEdx.RegisterCompareReached
            && missingEdx.FreshFieldReadReached
            && missingEdx.FreshFieldCaptured
            && missingEdx.FreshFieldAddress == 0x10
            && !missingEdx.EntryEDXCaptured
            && !missingEdx.FieldCompareReached
            && !missingEdx.FieldJzReached
            && !missingEdx.ExactCleanExitAdmitted
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C9185);

        CapacityFinalCalleeAlternateSuccessorGatePlan wrappedAddresses =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 4,
                FreshFrameMinusC: 1,
                EntryEAX: 0xFFFFFFF8,
                EntryEBX: 0,
                FreshFieldAtEaxPlus8: 0x80000000,
                EntryEDX: 0x7FFFFFFF));
        check("Both effective addresses wrap as DWORDs and later compare flags are retained",
            CapacityFinalCalleeAlternateSuccessorGateProjection.DeriveEbpMinusCAddress(4)
                == 0xFFFFFFF8
            && CapacityFinalCalleeAlternateSuccessorGateProjection.DeriveEaxPlus8Address(
                0xFFFFFFF8) == 0
            && wrappedAddresses.FrameMinusCAddress == 0xFFFFFFF8
            && wrappedAddresses.FreshFieldAddress == 0
            && wrappedAddresses.JbTaken == false
            && wrappedAddresses.FieldCompareResult == 1
            && wrappedAddresses.FieldCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && wrappedAddresses.FieldJzTaken == false
            && wrappedAddresses.StopAtRawFileOffsetExclusive == 0x012C918A);

        CapacityFinalCalleeAlternateSuccessorGatePlan aliasedIndependentReads =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0x100C,
                FreshFrameMinusC: 0x1001,
                EntryEAX: 0xFF8,
                EntryEBX: 0xFF8,
                FreshFieldAtEaxPlus8: 0xABCDEF01,
                EntryEDX: 0x12345678));
        check("Aliased effective addresses still use independent fresh captures",
            aliasedIndependentReads.FrameMinusCAddress == 0x1000
            && aliasedIndependentReads.FreshFieldAddress == 0x1000
            && aliasedIndependentReads.FrameMinusC != aliasedIndependentReads.FreshFieldValue
            && aliasedIndependentReads.FrameCompareLeft == 0xFF8
            && aliasedIndependentReads.FrameCompareRight == 0x1001
            && aliasedIndependentReads.RegisterCompareReached
            && aliasedIndependentReads.JbTaken == false
            && aliasedIndependentReads.FieldCompareLeft == 0xABCDEF01
            && aliasedIndependentReads.FieldCompareRight == 0x12345678
            && aliasedIndependentReads.FieldJzTaken == false);

        CapacityFinalCalleeAlternateSuccessorGatePlan signedOverflow =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(new(
                EntryEBP: 0,
                FreshFrameMinusC: 0x7FFFFFFF,
                EntryEAX: 0x80000000,
                EntryEBX: 0x80000001,
                FreshFieldAtEaxPlus8: 1,
                EntryEDX: 0));
        check("CMP records signed overflow while JB follows unsigned carry",
            signedOverflow.FrameCompareResult == 1
            && signedOverflow.FrameCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.FrameJzTaken == false
            && signedOverflow.RegisterCompareResult == 0xFFFFFFFF
            && signedOverflow.RegisterCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && signedOverflow.JbTaken == true
            && signedOverflow.StopAtRawFileOffsetExclusive == 0x012C918A);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C917C, "3B 45 F4", null, null),
            (0x012C917F, "74 0F", 0x012C9190, 0x016C9D90),
            (0x012C9181, "3B C3", null, null),
            (0x012C9183, "72 05", 0x012C918A, 0x016C9D8A),
            (0x012C9185, "39 50 08", null, null),
            (0x012C9188, "74 06", 0x012C9190, 0x016C9D90),
            (0x012C918A, "FF 15 24 4A AC 01", null, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateSuccessorGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateSuccessorGateInstructionEvidence instruction
            in CapacityFinalCalleeAlternateSuccessorGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateSuccessorGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateSuccessorGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlternateSuccessorGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateSuccessorGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateSuccessorGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateSuccessorGateProjection.Instructions
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
