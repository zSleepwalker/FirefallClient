using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedIndexGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/VA anchors, instruction bytes, and branch target match",
            CapacityFinalCalleeLargeSavedIndexGateProjection.EntryRawFileOffset
                == 0x012C9231
            && CapacityFinalCalleeLargeSavedIndexGateProjection.EntryVirtualAddress
                == 0x016C9E31
            && CapacityFinalCalleeLargeSavedIndexGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexGateProjection.JnzDestinationRawFileOffset
                == 0x012C9263
            && CapacityFinalCalleeLargeSavedIndexGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSavedIndexGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeSavedIndexGateProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeSavedIndexGateProjection.CapturedWindowBytes)
                == "8B5A1C3B149DF8AEED018B1DD8ADED017520"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeSavedIndexGatePlan missingEDX =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: null,
                FreshEDXPlus1C: 0x11111111,
                FreshIndexedDword: 0x22222222,
                FreshGlobalValue: 0x33333333));
        check("Missing first-use EDX stops at the first MOV and leaves later captures unused",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingEntryEDXForFreshRead
            && missingEDX.FreshEDXPlus1CReadReached
            && missingEDX.Reads.Length == 1
            && missingEDX.Reads[0] is
            {
                RawFileOffset: 0x012C9231,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null
            }
            && missingEDX.UnreachedFreshIndexedDwordCaptureProvided
            && missingEDX.UnreachedFreshGlobalValueCaptureProvided
            && !missingEDX.CompareReached
            && !missingEDX.FreshGlobalReadReached
            && !missingEDX.JnzReached
            && !missingEDX.IncomingFlagsPreservedAfterCompare
            && !missingEDX.ExactCleanCompletion
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedIndexGatePlan missingFirstValue =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: null,
                FreshIndexedDword: 0x22222222,
                FreshGlobalValue: 0x33333333));
        check("Missing fresh EDX+1C stops before the indexed operand and absolute reload",
            missingFirstValue.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshEDXPlus1C
            && missingFirstValue.Reads.Length == 1
            && missingFirstValue.Reads[0] is
            {
                Address: 0x11C,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingFirstValue.UnreachedFreshIndexedDwordCaptureProvided
            && missingFirstValue.UnreachedFreshGlobalValueCaptureProvided
            && !missingFirstValue.FreshIndexedReadReached
            && !missingFirstValue.CompareReached
            && !missingFirstValue.FreshGlobalReadReached
            && !missingFirstValue.JnzReached
            && missingFirstValue.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedIndexGatePlan missingIndexed =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 3,
                FreshIndexedDword: null,
                FreshGlobalValue: 0x44444444));
        check("A known indexed address is retained for its missing value and stops at CMP",
            missingIndexed.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshIndexedDword
            && missingIndexed.FreshIndexedAddress == 0x01EDAF04
            && missingIndexed.Reads.Length == 2
            && missingIndexed.Reads[1] is
            {
                RawFileOffset: 0x012C9234,
                Address: 0x01EDAF04,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingIndexed.UnreachedFreshGlobalValueCaptureProvided
            && !missingIndexed.CompareReached
            && !missingIndexed.FreshGlobalReadReached
            && !missingIndexed.JnzReached
            && missingIndexed.StopAtRawFileOffsetExclusive == 0x012C9234);

        CapacityFinalCalleeLargeSavedIndexGatePlan missingGlobal =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 3,
                FreshIndexedDword: 0x100,
                FreshGlobalValue: null));
        check("Missing absolute reload stops before JNZ while preserving the completed CMP flags",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshGlobalValue
            && missingGlobal.CompareReached
            && missingGlobal.CompareResult == 0
            && missingGlobal.CompareFlags?.Zero == true
            && missingGlobal.FreshGlobalReadReached
            && missingGlobal.FreshGlobalAddress == 0x01EDADD8
            && missingGlobal.Reads.Length == 3
            && missingGlobal.Reads[2] is
            {
                RawFileOffset: 0x012C923B,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingGlobal.IncomingFlagsPreservedAfterCompare
            && !missingGlobal.JnzReached
            && !missingGlobal.ExactCleanCompletion
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C923B);

        CapacityFinalCalleeLargeSavedIndexGatePlan zero =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0,
                FreshEDXPlus1C: 0,
                FreshIndexedDword: 0,
                FreshGlobalValue: 0));
        check("Captured zero values are valid and equal CMP flags select the 9243 fallthrough",
            zero.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome.EqualToFallthrough9243
            && zero.FreshEDXPlus1CAddress == 0x1C
            && zero.EBXAfterFreshEDXPlus1C == 0
            && zero.FreshIndexedAddress == 0x01EDAEF8
            && zero.CompareLeft == 0
            && zero.CompareRight == 0
            && zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.EBXAfterFreshGlobalReload == 0
            && zero.JnzReached
            && zero.JnzTaken == false
            && zero.JnzSelectedDestinationRawFileOffset == 0x012C9243
            && zero.StopAtRawFileOffsetExclusive == 0x012C9243
            && zero.IncomingFlagsPreservedAfterCompare
            && zero.ExactCleanCompletion
            && !zero.MemoryWriteReached);

        CapacityFinalCalleeLargeSavedIndexGatePlan equalWithReloadMismatch =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 5,
                FreshIndexedDword: 0x100,
                FreshGlobalValue: 0x101));
        CapacityFinalCalleeLargeSavedIndexGatePlan unequalWithReloadMatch =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshEDXPlus1C: 5,
                FreshIndexedDword: 0x101,
                FreshGlobalValue: 0x100));
        check("JNZ uses the preserved earlier CMP rather than comparing the reloaded EBX",
            equalWithReloadMismatch.EBXAfterFreshGlobalReload == 0x101
            && equalWithReloadMismatch.CompareRight == 0x100
            && equalWithReloadMismatch.CompareFlags?.Zero == true
            && equalWithReloadMismatch.JnzTaken == false
            && equalWithReloadMismatch.JnzSelectedDestinationRawFileOffset == 0x012C9243
            && unequalWithReloadMatch.EBXAfterFreshGlobalReload == 0x100
            && unequalWithReloadMatch.CompareRight == 0x101
            && unequalWithReloadMatch.CompareFlags?.Zero == false
            && unequalWithReloadMatch.JnzTaken == true
            && unequalWithReloadMatch.JnzSelectedDestinationRawFileOffset == 0x012C9263);

        CapacityFinalCalleeLargeSavedIndexGatePlan wrappedAddresses =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                FreshEDXPlus1C: 0xFFFFFFFF,
                FreshIndexedDword: 0xFFFFFFF0,
                FreshGlobalValue: 0xFFFFFFF0));
        check("The EDX+1C and scaled indexed addresses use unsigned 32-bit wraparound",
            wrappedAddresses.FreshEDXPlus1CAddress == 0xC
            && wrappedAddresses.Reads[0].Address == 0xC
            && wrappedAddresses.FreshIndexedAddress == 0x01EDAEF4
            && wrappedAddresses.Reads[1].Address == 0x01EDAEF4
            && wrappedAddresses.CompareFlags?.Zero == true
            && wrappedAddresses.JnzTaken == false);

        CapacityFinalCalleeLargeSavedIndexGatePlan subtractFlags =
            CapacityFinalCalleeLargeSavedIndexGateProjection.Project(new(
                EntryEDX: 0x80000000,
                FreshEDXPlus1C: 0,
                FreshIndexedDword: 1,
                FreshGlobalValue: 0));
        check("CMP exposes subtraction carry, parity, auxiliary, zero, sign, and overflow flags",
            subtractFlags.CompareResult == 0x7FFFFFFF
            && subtractFlags.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && subtractFlags.JnzTaken == true
            && subtractFlags.JnzSelectedDestinationRawFileOffset == 0x012C9263);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9231, "8B 5A 1C", null, null),
            (0x012C9234, "3B 14 9D F8 AE ED 01", null, null),
            (0x012C923B, "8B 1D D8 AD ED 01", null, null),
            (0x012C9241, "75 20", 0x012C9263, 0x016C9E63)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeSavedIndexGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeSavedIndexGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeSavedIndexGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeSavedIndexGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeSavedIndexGateProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeSavedIndexGateProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeSavedIndexGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeSavedIndexGateProjection.Instructions
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
