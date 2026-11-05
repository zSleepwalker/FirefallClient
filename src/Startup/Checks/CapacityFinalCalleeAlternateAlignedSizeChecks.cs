using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateAlignedSizeChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The aligned-size instruction window and JNB destinations match",
            CapacityFinalCalleeAlternateAlignedSizeProjection.EntryRawFileOffset
                == 0x012C90F6
            && CapacityFinalCalleeAlternateAlignedSizeProjection.EntryVirtualAddress
                == 0x016C9CF6
            && CapacityFinalCalleeAlternateAlignedSizeProjection.BoundaryRawFileOffsetExclusive
                == 0x012C910C
            && CapacityFinalCalleeAlternateAlignedSizeProjection.BranchTargetRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeAlternateAlignedSizeProjection.FallthroughRawFileOffset
                == 0x012C910C
            && CapacityFinalCalleeAlternateAlignedSizeProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateAlignedSizeProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateAlignedSizeProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeAlternateAlignedSizeProjection.CapturedWindowBytes)
                == "83E1F88BC103F9C1E8038945FC83F8200F838F000000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateAlignedSizePlan invalidEntry =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: false,
                EntryECX: 0,
                EntryEDI: 0,
                EntryEBP: 0));
        check("An unvalidated entry leaves all later registers and instructions unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.EntryNotValidated
            && !invalidEntry.AndReached
            && !invalidEntry.MoveEAXReached
            && !invalidEntry.AddReached
            && !invalidEntry.ShiftReached
            && !invalidEntry.StoreReached
            && !invalidEntry.CompareReached
            && !invalidEntry.JnbReached
            && !invalidEntry.ExactCleanAdmission
            && invalidEntry.UnreachedEntryECXCaptureProvided
            && invalidEntry.UnreachedEntryEDICaptureProvided
            && invalidEntry.UnreachedEntryEBPCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlternateAlignedSizePlan missingECX =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: null,
                EntryEDI: 0x100,
                EntryEBP: 0x200));
        check("ECX is required at AND and later supplied registers remain unreached",
            missingECX.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX
            && !missingECX.AndReached
            && !missingECX.EntryECXCaptured
            && missingECX.AlignedECX is null
            && missingECX.AndFlags is null
            && !missingECX.MoveEAXReached
            && !missingECX.AddReached
            && !missingECX.StoreReached
            && !missingECX.ExactCleanAdmission
            && missingECX.UnreachedEntryEDICaptureProvided
            && missingECX.UnreachedEntryEBPCaptureProvided
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlternateAlignedSizePlan missingEDI =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0x1F,
                EntryEDI: null,
                EntryEBP: 0x200));
        check("A missing EDI stops at ADD while MOV preserves AND flags",
            missingEDI.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI
            && missingEDI.AndReached
            && missingEDI.EntryECXCaptured
            && missingEDI.AlignedECX == 0x18
            && missingEDI.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEDI.MoveEAXReached
            && missingEDI.EAXAfterMove == 0x18
            && missingEDI.FlagsAfterMove == missingEDI.AndFlags
            && !missingEDI.AddReached
            && missingEDI.AddFlags is null
            && !missingEDI.ShiftReached
            && !missingEDI.StoreReached
            && !missingEDI.CompareReached
            && !missingEDI.ExactCleanAdmission
            && missingEDI.UnreachedEntryEBPCaptureProvided
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C90FB);

        CapacityFinalCalleeAlternateAlignedSizePlan missingEBP =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0xF8,
                EntryEDI: 0x7FFFFFF8,
                EntryEBP: null));
        check("A missing EBP stops at the local store and retains the prior ADD and SHR flags",
            missingEBP.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEBP
            && missingEBP.AndReached
            && missingEBP.MoveEAXReached
            && missingEBP.AddReached
            && missingEBP.EDIAfterAdd == 0x800000F0
            && missingEBP.AddFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && missingEBP.ShiftReached
            && missingEBP.AlignedSize == 0x1F
            && missingEBP.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && !missingEBP.StoreReached
            && missingEBP.Store is null
            && !missingEBP.EntryEBPCaptured
            && !missingEBP.CompareReached
            && !missingEBP.JnbReached
            && !missingEBP.ExactCleanAdmission
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9100);

        CapacityFinalCalleeAlternateAlignedSizePlan belowThreshold =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0xF8,
                EntryEDI: 0xFFFFFFFC,
                EntryEBP: 2));
        check("The aligned size below 0x20 takes JNB fallthrough with exact arithmetic flags",
            belowThreshold.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.BelowThresholdFallthrough
            && belowThreshold.AlignedECX == 0xF8
            && belowThreshold.EAXAfterMove == 0xF8
            && belowThreshold.EDIAfterAdd == 0xF4
            && belowThreshold.AlignedSize == 0x1F
            && belowThreshold.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0xFFFFFFFE,
                Value: 0x1F
            }
            && belowThreshold.FlagsAfterStore == belowThreshold.ShiftFlags
            && belowThreshold.CompareReached
            && belowThreshold.CompareResult == 0xFFFFFFFF
            && belowThreshold.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowThreshold.JnbReached
            && belowThreshold.JnbTaken == false
            && belowThreshold.SelectedDestinationRawFileOffset == 0x012C910C
            && belowThreshold.ExactCleanAdmission
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleeAlternateAlignedSizePlan equalThreshold =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0x100,
                EntryEDI: 0xFFFFFF00,
                EntryEBP: 3));
        check("An aligned size equal to 0x20 takes the JNB target and preserves wrapped stores",
            equalThreshold.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.AtOrAboveThresholdBranchTarget
            && equalThreshold.AlignedECX == 0x100
            && equalThreshold.EDIAfterAdd == 0
            && equalThreshold.AlignedSize == 0x20
            && equalThreshold.Store is
            {
                Address: 0xFFFFFFFF,
                Value: 0x20
            }
            && equalThreshold.CompareResult == 0
            && equalThreshold.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalThreshold.JnbTaken == true
            && equalThreshold.SelectedDestinationRawFileOffset == 0x012C919B
            && equalThreshold.ExactCleanAdmission
            && equalThreshold.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeAlternateAlignedSizePlan aboveThreshold =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0x108,
                EntryEDI: 0x7FFFFFF8,
                EntryEBP: 0x80000000));
        check("An aligned size above 0x20 takes the exact JNB target",
            aboveThreshold.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.AtOrAboveThresholdBranchTarget
            && aboveThreshold.AlignedECX == 0x108
            && aboveThreshold.AlignedSize == 0x21
            && aboveThreshold.Store is
            {
                Address: 0x7FFFFFFC,
                Value: 0x21
            }
            && aboveThreshold.CompareResult == 1
            && aboveThreshold.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && aboveThreshold.JnbTaken == true
            && aboveThreshold.SelectedDestinationRawFileOffset == 0x012C919B
            && aboveThreshold.ExactCleanAdmission);

        CapacityFinalCalleeAlternateAlignedSizePlan zero =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0,
                EntryEDI: 0xFFFFFFFF,
                EntryEBP: 0));
        check("Zero aligned size keeps SHR undefined flags absent and takes the below-threshold path",
            zero.AlignedECX == 0
            && zero.EDIAfterAdd == 0xFFFFFFFF
            && zero.AlignedSize == 0
            && zero.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.AddFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zero.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && zero.CompareResult == 0xFFFFFFE0
            && zero.CompareFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zero.JnbTaken == false
            && zero.SelectedDestinationRawFileOffset == 0x012C910C
            && zero.ExactCleanAdmission);

        CapacityFinalCalleeAlternateAlignedSizePlan arithmeticFlags =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: 0xFFFFFFFF,
                EntryEDI: 0x7FFFFFF8,
                EntryEBP: 0));
        check("AND, ADD, SHR, and CMP expose their defined flags and retain undefined flags as null",
            arithmeticFlags.AndFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && arithmeticFlags.FlagsAfterMove == arithmeticFlags.AndFlags
            && arithmeticFlags.AddFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && arithmeticFlags.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && arithmeticFlags.FlagsAfterStore == arithmeticFlags.ShiftFlags
            && arithmeticFlags.CompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && arithmeticFlags.JnbTaken == true);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? BranchTarget)[]
        {
            (0x012C90F6, "83 E1 F8", null),
            (0x012C90F9, "8B C1", null),
            (0x012C90FB, "03 F9", null),
            (0x012C90FD, "C1 E8 03", null),
            (0x012C9100, "89 45 FC", null),
            (0x012C9103, "83 F8 20", null),
            (0x012C9106, "0F 83 8F 00 00 00", 0x012C919B)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateAlignedSizeProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateAlignedSizeInstructionEvidence instruction
            in CapacityFinalCalleeAlternateAlignedSizeProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateAlignedSizeProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || instruction.BranchTargetRawFileOffset
                    != expected.First(item => item.Raw == instruction.RawFileOffset).BranchTarget
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateAlignedSizeProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlternateAlignedSizeProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateAlignedSizeProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateAlignedSizeProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateAlignedSizeProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected.Select(item => (item.Raw, item.Bytes)));
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
