using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeTagValidationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The captured 41-byte window and all instruction destinations match",
            CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset == 0x012C8EF8
            && CapacityFinalCalleeTagValidationProjection.EntryVirtualAddress == 0x016C9AF8
            && CapacityFinalCalleeTagValidationProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8F21
            && CapacityFinalCalleeTagValidationProjection.BoundaryVirtualAddressExclusive
                == 0x016C9B21
            && CapacityFinalCalleeTagValidationProjection.GlobalDwordVirtualAddress
                == 0x01EDADE0
            && CapacityFinalCalleeTagValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset)
                == CapacityFinalCalleeTagValidationProjection.EntryVirtualAddress
            && CapacityFinalCalleeTagValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeTagValidationProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeTagValidationProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeTagValidationProjection.CapturedWindowBytes)
                == "3BF20F8376FFFFFF8B4A04F6C1010F846AFFFFFFF6C1020F85C60300003B15E0ADED010F85A9010000"
            && CapacityFinalCalleeTagValidationProjection.Instructions.Sum(
                instruction => instruction.Bytes.Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries).Length) == 41
            && InstructionEvidenceMatches()
            && ReadDescriptorsMatch());

        CapacityFinalCalleeTagValidationPlan invalidEntry =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                captureValidatedEntry: false,
                edxPlusFourDword: 0,
                globalDword: 0));
        check("An unvalidated entry reads nothing and marks both supplied memory captures unreached",
            invalidEntry.Outcome == CapacityFinalCalleeTagValidationOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && !invalidEntry.CompareEsiEdxReached
            && !invalidEntry.EdxPlusFourReadReached
            && !invalidEntry.GlobalReadReached
            && invalidEntry.UnreachedEdxPlusFourDwordCaptureProvided
            && invalidEntry.UnreachedGlobalDwordCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeTagValidationPlan firstBranch =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0x80000000,
                entryEdx: 0x7FFFFFFF,
                edxPlusFourDword: 3,
                globalDword: 0));
        CapacityFinalCalleeTagValidationPlan firstEquality =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0x80000000,
                entryEdx: 0x80000000,
                edxPlusFourDword: 3,
                globalDword: 0));
        CapacityFinalCalleeTagValidationPlan firstFallthrough =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0x7FFFFFFF,
                entryEdx: 0x80000000,
                edxPlusFourDword: null));
        check("The initial CMP/JNB is unsigned, with equality taking the JNB exit",
            firstBranch.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx
            && firstBranch.CompareEsiEdxFlags is
            {
                Carry: false,
                Zero: false
            }
            && firstBranch.UnsignedBelowBranchReached
            && firstBranch.EntryEsiUnsignedBelowEntryEdx == false
            && firstBranch.SelectedDestinationRawFileOffset == 0x012C8E76
            && firstBranch.Reads.IsEmpty
            && firstBranch.UnreachedEdxPlusFourDwordCaptureProvided
            && firstBranch.UnreachedGlobalDwordCaptureProvided
            && firstBranch.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstEquality.CompareEsiEdxFlags is
            {
                Carry: false,
                Zero: true
            }
            && firstEquality.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx
            && firstEquality.Reads.IsEmpty
            && firstEquality.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstFallthrough.CompareEsiEdxFlags is
            {
                Carry: true,
                Zero: false
            }
            && firstFallthrough.EntryEsiUnsignedBelowEntryEdx == true
            && firstFallthrough.EdxPlusFourReadReached
            && firstFallthrough.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingEdxPlusFourDword);

        CapacityFinalCalleeTagValidationPlan clZero =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 1,
                edxPlusFourDword: 0,
                globalDword: 0xABCDEF01));
        CapacityFinalCalleeTagValidationPlan clOne =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 1,
                edxPlusFourDword: 0x12345679,
                globalDword: 0xABCDEF01));
        CapacityFinalCalleeTagValidationPlan clTwo =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 1,
                edxPlusFourDword: 3,
                globalDword: 0xABCDEF01));
        check("TEST CL,1 exits for clear bit 0; TEST CL,2 exits for set bit 1",
            clZero.Outcome == CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear
            && clZero.TestClOneFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && clZero.EdxPlusFourDwordCaptured
            && clZero.EdxPlusFourDword == 0
            && clZero.Reads[0].ValueCaptured
            && clZero.Reads[0].Value == 0
            && clZero.ClZeroBranchTaken == true
            && !clZero.TestClTwoReached
            && clZero.SelectedDestinationRawFileOffset == 0x012C8E76
            && clZero.UnreachedGlobalDwordCaptureProvided
            && clZero.StopAtRawFileOffsetExclusive == 0x012C8E76
            && clOne.Outcome == CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword
            && clOne.TestClOneFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && clOne.ClZeroBranchTaken == false
            && clOne.TestClTwoReached
            && clOne.TestClTwoFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && clOne.ClOneBranchTaken == false
            && clOne.SelectedDestinationRawFileOffset == 0x012C90CA
            && clTwo.Outcome == CapacityFinalCalleeTagValidationOutcome.ClBitOneSet
            && clTwo.TestClTwoFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && clTwo.ClOneBranchTaken == true
            && clTwo.SelectedDestinationRawFileOffset == 0x012C92DB
            && clTwo.UnreachedGlobalDwordCaptureProvided
            && clTwo.StopAtRawFileOffsetExclusive == 0x012C92DB);

        CapacityFinalCalleeTagValidationPlan missingGlobal =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 0xFFFFFFFC,
                edxPlusFourDword: 1,
                globalDword: null));
        CapacityFinalCalleeTagValidationPlan unequalGlobal =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 0x80000000,
                edxPlusFourDword: 1,
                globalDword: 0x7FFFFFFF));
        CapacityFinalCalleeTagValidationPlan capturedZeroGlobal =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 1,
                edxPlusFourDword: 1,
                globalDword: 0));
        CapacityFinalCalleeTagValidationPlan equalZero =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: 1,
                edxPlusFourDword: 1,
                globalDword: 1));
        check("The final CMP/JNZ distinguishes missing and captured-zero reads, inequality, and equality",
            missingGlobal.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingGlobalDword
            && missingGlobal.EdxPlusFourAddress == 0
            && missingGlobal.EdxPlusFourDwordCaptured
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalDwordCaptured
            && missingGlobal.Reads.Length == 2
            && missingGlobal.Reads[0].Address == 0
            && missingGlobal.Reads[1].Address == 0x01EDADE0
            && !missingGlobal.Reads[1].ValueCaptured
            && !missingGlobal.CompareEdxGlobalReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8F15
            && unequalGlobal.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword
            && unequalGlobal.CompareEdxGlobalFlags is
            {
                Carry: false,
                Zero: false
            }
            && unequalGlobal.NotEqualBranchTaken == true
            && unequalGlobal.SelectedDestinationRawFileOffset == 0x012C90CA
            && capturedZeroGlobal.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword
            && capturedZeroGlobal.GlobalDwordCaptured
            && capturedZeroGlobal.GlobalDword == 0
            && capturedZeroGlobal.Reads[1].ValueCaptured
            && capturedZeroGlobal.Reads[1].Value == 0
            && capturedZeroGlobal.NotEqualBranchTaken == true
            && equalZero.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && equalZero.GlobalDwordCaptured
            && equalZero.GlobalDword == 1
            && equalZero.Reads[1].ValueCaptured
            && equalZero.Reads[1].Value == 1
            && equalZero.CompareEdxGlobalFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.NotEqualBranchTaken == false
            && equalZero.SelectedDestinationRawFileOffset == 0x012C8F21
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C8F21);

        CapacityFinalCalleeTagValidationPlan wrapped =
            CapacityFinalCalleeTagValidationProjection.Project(Capture(
                entryEsi: 0,
                entryEdx: uint.MaxValue,
                edxPlusFourDword: 1,
                globalDword: uint.MaxValue));
        check("EDX+4 wraps as a 32-bit address and the equality route ends at the exclusive boundary",
            CapacityFinalCalleeTagValidationProjection.EdxPlusFourAddress(uint.MaxValue) == 3
            && wrapped.EdxPlusFourAddress == 3
            && wrapped.Reads[0].Address == 3
            && wrapped.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8F21);
    }

    private static CapacityFinalCalleeTagValidationCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEsi = 0,
        uint entryEdx = 1,
        uint? edxPlusFourDword = 1,
        uint? globalDword = 1) =>
        new(captureValidatedEntry, entryEsi, entryEdx, edxPlusFourDword, globalDword);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            byte? ImmediateValue,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8EF8, "3B F2", null, null, null, null),
            (0x012C8EFA, "0F 83 76 FF FF FF", null, null, 0x012C8E76, 0x016C9A76),
            (0x012C8F00, "8B 4A 04", null, null, null, null),
            (0x012C8F03, "F6 C1 01", null, 1, null, null),
            (0x012C8F06, "0F 84 6A FF FF FF", null, null, 0x012C8E76, 0x016C9A76),
            (0x012C8F0C, "F6 C1 02", null, 2, null, null),
            (0x012C8F0F, "0F 85 C6 03 00 00", null, null, 0x012C92DB, 0x016C9EDB),
            (0x012C8F15, "3B 15 E0 AD ED 01", 0x01EDADE0, null, null, null),
            (0x012C8F1B, "0F 85 A9 01 00 00", null, null, 0x012C90CA, 0x016C9CCA)
        };
        uint nextRawOffset = CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeTagValidationInstructionEvidence instruction
            in CapacityFinalCalleeTagValidationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeTagValidationProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeTagValidationProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeTagValidationProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeTagValidationProjection.CapturedWindowBytes)
            && CapacityFinalCalleeTagValidationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.ImmediateValue,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static bool ReadDescriptorsMatch()
    {
        var expected = new[]
        {
            new CapacityFinalCalleeTagValidationReadDescriptor(
                0x012C8F00,
                CapacityFinalCalleeTagValidationReadKind.EdxPlusFourDword,
                null,
                "EDX",
                4),
            new CapacityFinalCalleeTagValidationReadDescriptor(
                0x012C8F15,
                CapacityFinalCalleeTagValidationReadKind.GlobalDword,
                0x01EDADE0,
                null,
                0)
        };
        return CapacityFinalCalleeTagValidationProjection.OrderedReadDescriptors
            .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
