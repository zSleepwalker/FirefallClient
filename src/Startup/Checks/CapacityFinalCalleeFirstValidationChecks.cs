using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFirstValidationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, instruction order, and branch destinations match",
            CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset == 0x012C8D88
            && CapacityFinalCalleeFirstValidationProjection.EntryVirtualAddress == 0x016C9988
            && CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8DA7
            && CapacityFinalCalleeFirstValidationProjection.BoundaryVirtualAddressExclusive
                == 0x016C99A7
            && CapacityFinalCalleeFirstValidationProjection.GlobalDwordVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeFirstValidationProjection.EsiUnsignedBelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeFirstValidationProjection.EsiUnsignedBelowDestinationVirtualAddress
                == 0x016C9A76
            && CapacityFinalCalleeFirstValidationProjection.MaskEqualsOneDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeFirstValidationProjection.MaskEqualsOneDestinationVirtualAddress
                == 0x016C9A76
            && CapacityFinalCalleeFirstValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFirstValidationProjection.EntryVirtualAddress
            && CapacityFinalCalleeFirstValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeFirstValidationProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeFirstValidationProjection.CapturedWindowBytes)
                == "8B1DD8ADED013BF30F82E00000008B46048BC883E10383F9010F84CF000000"
            && CapacityFinalCalleeFirstValidationProjection.Instructions.Sum(
                instruction => instruction.Bytes.Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries).Length) == 31
            && InstructionEvidenceMatches()
            && ReadDescriptorsMatch());

        CapacityFinalCalleeFirstValidationPlan invalidEntry =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                captureValidatedEntry: false,
                globalDword: 0,
                esi4Dword: 0));
        check("An unvalidated entry performs no reads and preserves supplied captures as unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && !invalidEntry.GlobalReadReached
            && !invalidEntry.CompareEsiEbxReached
            && !invalidEntry.Esi4ReadReached
            && invalidEntry.UnreachedGlobalDwordCaptureProvided
            && invalidEntry.UnreachedEsi4DwordCaptureProvided
            && invalidEntry.Capture.EntryESI == 0x12345678
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8D88);

        CapacityFinalCalleeFirstValidationPlan missingGlobal =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                globalDword: null,
                esi4Dword: 0xAABBCCDD));
        check("A missing global read stops at entry and leaves the later ESI+4 capture unreached",
            missingGlobal.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MissingGlobalDword
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalDwordCaptured
            && missingGlobal.GlobalDword is null
            && missingGlobal.Reads.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFirstValidationRead(
                    0x012C8D88,
                    CapacityFinalCalleeFirstValidationReadKind.GlobalDword,
                    0x01EDADD8,
                    false,
                    null)
            })
            && !missingGlobal.CompareEsiEbxReached
            && !missingGlobal.Esi4ReadReached
            && missingGlobal.UnreachedEsi4DwordCaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8D88);

        CapacityFinalCalleeFirstValidationPlan unsignedBelow =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                entryEsi: 0x7FFFFFFF,
                globalDword: 0x80000000,
                esi4Dword: 0));
        CapacityFinalCalleeFirstValidationPlan highBitUnsignedAbove =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                entryEsi: 0x80000000,
                globalDword: 0x7FFFFFFF,
                esi4Dword: 0));
        CapacityFinalCalleeFirstValidationPlan equality =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                entryEsi: 0x80000000,
                globalDword: 0x80000000,
                esi4Dword: 0));
        check("The first compare is unsigned, branches below only, and treats equality as fallthrough",
            unsignedBelow.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
            && unsignedBelow.CompareEsiEbxFlags is
            {
                Carry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.UnsignedBelowBranchReached
            && unsignedBelow.EntryEsiUnsignedBelowGlobal == true
            && unsignedBelow.FirstSelectedDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.FirstSelectedDestinationVirtualAddress == 0x016C9A76
            && !unsignedBelow.Esi4ReadReached
            && unsignedBelow.UnreachedEsi4DwordCaptureProvided
            && unsignedBelow.Reads.Length == 1
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && highBitUnsignedAbove.CompareEsiEbxFlags is
            {
                Carry: false,
                Zero: false
            }
            && highBitUnsignedAbove.EntryEsiUnsignedBelowGlobal == false
            && highBitUnsignedAbove.Esi4ReadReached
            && equality.CompareEsiEbxFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equality.EntryEsiUnsignedBelowGlobal == false
            && equality.FirstSelectedDestinationRawFileOffset == 0x012C8D96
            && equality.Esi4ReadReached);

        CapacityFinalCalleeFirstValidationPlan missingEsi4 =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                entryEsi: 0x1000,
                globalDword: 0x1000,
                esi4Dword: null));
        check("A missing ESI+4 dword stops after the unsigned comparison at the read instruction",
            missingEsi4.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MissingEsi4Dword
            && missingEsi4.CompareEsiEbxFlags?.Zero == true
            && missingEsi4.FirstSelectedDestinationRawFileOffset == 0x012C8D96
            && missingEsi4.Esi4ReadReached
            && missingEsi4.Esi4Address == 0x1004
            && !missingEsi4.Esi4DwordCaptured
            && missingEsi4.Esi4Dword is null
            && missingEsi4.Reads.Length == 2
            && missingEsi4.Reads[1].Address == 0x1004
            && !missingEsi4.MovEcxFromEaxReached
            && missingEsi4.StopAtRawFileOffsetExclusive == 0x012C8D96);

        CapacityFinalCalleeFirstValidationPlan maskZero =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                globalDword: 0,
                esi4Dword: 0x12345678));
        CapacityFinalCalleeFirstValidationPlan maskOne =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                globalDword: 0,
                esi4Dword: 0x12345679));
        CapacityFinalCalleeFirstValidationPlan maskTwo =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                globalDword: 0,
                esi4Dword: 0x1234567A));
        CapacityFinalCalleeFirstValidationPlan maskThree =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                globalDword: 0,
                esi4Dword: 0x1234567B));
        check("Mask results 0, 1, 2, and 3 preserve EAX and branch only for exactly 1",
            maskZero.Outcome == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && maskZero.EcxAfterMove == 0x12345678
            && maskZero.EcxAfterMask == 0
            && maskZero.AndEcxFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && maskZero.CompareEcxOneFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maskZero.EqualOneBranchTaken == false
            && maskZero.FinalSelectedDestinationRawFileOffset == 0x012C8DA7
            && maskZero.StopAtRawFileOffsetExclusive == 0x012C8DA7
            && maskOne.Outcome == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && maskOne.EcxAfterMask == 1
            && maskOne.AndEcxFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && maskOne.CompareEcxOneFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && maskOne.EqualOneBranchTaken == true
            && maskOne.FinalSelectedDestinationRawFileOffset == 0x012C8E76
            && maskTwo.Outcome == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && maskTwo.EcxAfterMask == 2
            && maskTwo.AndEcxFlags?.Parity == false
            && maskTwo.CompareEcxOneFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && maskTwo.EqualOneBranchTaken == false
            && maskTwo.FinalSelectedDestinationRawFileOffset == 0x012C8DA7
            && maskThree.Outcome == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && maskThree.EcxAfterMask == 3
            && maskThree.AndEcxFlags?.Parity == true
            && maskThree.CompareEcxOneFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && maskThree.EqualOneBranchTaken == false
            && maskThree.FinalSelectedDestinationRawFileOffset == 0x012C8DA7
            && new[]
            {
                maskZero,
                maskOne,
                maskTwo,
                maskThree
            }.All(plan =>
                plan.EaxAfterLoad == plan.Capture.Esi4Dword
                && plan.EaxAfterValidation == plan.EaxAfterLoad
                && plan.EcxAfterMove == plan.EaxAfterLoad
                && plan.MovEcxFromEaxReached
                && plan.AndEcxThreeReached
                && plan.CompareEcxOneReached
                && plan.EqualOneBranchReached
                && plan.Reads.Length == 2));

        CapacityFinalCalleeFirstValidationPlan zeroDword =
            CapacityFinalCalleeFirstValidationProjection.Project(Capture(
                entryEsi: 0xFFFFFFFE,
                globalDword: 0,
                esi4Dword: 0));
        check("A captured zero is distinct from a missing ESI+4 read and ESI+4 wraps",
            CapacityFinalCalleeFirstValidationProjection.EsiPlusFourAddress(uint.MaxValue)
                == 3
            && zeroDword.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && zeroDword.Esi4ReadReached
            && zeroDword.Esi4Address == 2
            && zeroDword.Esi4DwordCaptured
            && zeroDword.Esi4Dword == 0
            && zeroDword.Reads[1].ValueCaptured
            && zeroDword.Reads[1].Value == 0
            && zeroDword.EaxAfterLoad == 0
            && zeroDword.EaxAfterValidation == 0
            && zeroDword.EcxAfterMask == 0);
    }

    private static CapacityFinalCalleeFirstValidationCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEsi = 0x12345678,
        uint? globalDword = 0,
        uint? esi4Dword = 0x12345679) =>
        new(captureValidatedEntry, entryEsi, globalDword, esi4Dword);

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
            (0x012C8D88, "8B 1D D8 AD ED 01", 0x01EDADD8, null, null, null),
            (0x012C8D8E, "3B F3", null, null, null, null),
            (0x012C8D90, "0F 82 E0 00 00 00", null, null, 0x012C8E76, 0x016C9A76),
            (0x012C8D96, "8B 46 04", null, null, null, null),
            (0x012C8D99, "8B C8", null, null, null, null),
            (0x012C8D9B, "83 E1 03", null, 3, null, null),
            (0x012C8D9E, "83 F9 01", null, 1, null, null),
            (0x012C8DA1, "0F 84 CF 00 00 00", null, null, 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset = CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFirstValidationInstructionEvidence instruction
            in CapacityFinalCalleeFirstValidationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFirstValidationProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeFirstValidationProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFirstValidationProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFirstValidationProjection.Instructions
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
            new CapacityFinalCalleeFirstValidationReadDescriptor(
                0x012C8D88,
                CapacityFinalCalleeFirstValidationReadKind.GlobalDword,
                0x01EDADD8,
                null,
                0),
            new CapacityFinalCalleeFirstValidationReadDescriptor(
                0x012C8D96,
                CapacityFinalCalleeFirstValidationReadKind.EsiPlusFourDword,
                null,
                "ESI",
                4)
        };
        return CapacityFinalCalleeFirstValidationProjection.OrderedReadDescriptors
            .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
