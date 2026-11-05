using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFinalPairedTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete bounded window, mapping, global operand, and branch destinations match",
            CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset
                == 0x012C908B
            && CapacityFinalCalleeFinalPairedTailProjection.EntryVirtualAddress
                == 0x016C9C8B
            && CapacityFinalCalleeFinalPairedTailProjection.ValidationDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeFinalPairedTailProjection.FinalJumpDestinationRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleeFinalPairedTailProjection.BoundaryRawFileOffsetExclusive
                == 0x012C90A2
            && CapacityFinalCalleeFinalPairedTailProjection.GlobalValueVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleeFinalPairedTailProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFinalPairedTailProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeFinalPairedTailProjection.CapturedWindowBytes)
                == "3B0DD8ADED010F82DFFDFFFF894814894118E950FEFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeFinalPairedTailPlan missingEcx =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: null,
                freshGlobalForCompare: 0,
                entryEax: 0));
        check("Missing first-use ECX stops at CMP and rejects later supplied captures",
            missingEcx.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryECXForCompare
            && !missingEcx.EntryECXCaptured
            && !missingEcx.CompareGlobalReadReached
            && missingEcx.Reads.IsEmpty
            && !missingEcx.CompareReached
            && !missingEcx.GuardBranchReached
            && !missingEcx.FirstStoreReached
            && !missingEcx.SecondStoreReached
            && !missingEcx.FinalJumpReached
            && missingEcx.UnreachedFreshGlobalForCompareCaptureProvided
            && missingEcx.UnreachedEntryEAXCaptureProvided
            && missingEcx.Writes.IsEmpty
            && !missingEcx.ExactCleanExitAdmitted
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeFinalPairedTailPlan missingGlobal =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                freshGlobalForCompare: null));
        check("Missing global data is a reached CMP read, not a captured zero or later store",
            missingGlobal.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare
            && missingGlobal.EntryECXCaptured
            && missingGlobal.CompareGlobalReadReached
            && !missingGlobal.CompareGlobalCaptured
            && missingGlobal.Reads.Length == 1
            && missingGlobal.Reads[0] is
            {
                RawFileOffset: 0x012C908B,
                Address: 0x01EDADD8,
                ValueCaptured: false,
                Value: null
            }
            && !missingGlobal.CompareReached
            && missingGlobal.CompareResult is null
            && !missingGlobal.GuardBranchReached
            && missingGlobal.UnreachedEntryEAXCaptureProvided
            && missingGlobal.Writes.IsEmpty
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C908B);

        CapacityFinalCalleeFinalPairedTailPlan unsignedBelow =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 1,
                freshGlobalForCompare: 0xF0000000,
                entryEax: null));
        check("JB uses unsigned carry and branches before EAX is consumed",
            unsignedBelow.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome
                    .EntryECXUnsignedBelowGlobalToValidation
            && unsignedBelow.CompareResult == 0x10000001
            && unsignedBelow.CompareFlags is
            {
                Carry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedBelow.EntryECXUnsignedBelowGlobal == true
            && unsignedBelow.GuardBranchReached
            && unsignedBelow.GuardBranchTaken == true
            && unsignedBelow.GuardBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !unsignedBelow.UnreachedEntryEAXCaptureProvided
            && unsignedBelow.Writes.IsEmpty
            && !unsignedBelow.FirstStoreReached
            && !unsignedBelow.SecondStoreReached
            && !unsignedBelow.FinalJumpReached
            && unsignedBelow.ExactCleanExitAdmitted
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeFinalPairedTailPlan contaminatedBelow =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 1,
                freshGlobalForCompare: 2,
                entryEax: 0x10));
        check("A supplied EAX on the guard-below exit is unused and rejects clean admission",
            contaminatedBelow.GuardBranchTaken == true
            && contaminatedBelow.UnreachedEntryEAXCaptureProvided
            && !contaminatedBelow.ExactCleanExitAdmitted
            && contaminatedBelow.Writes.IsEmpty);

        CapacityFinalCalleeFinalPairedTailPlan equal =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 0xF0000000,
                freshGlobalForCompare: 0xF0000000));
        check("Unsigned equality sets ZF but does not take JB",
            equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.EntryECXUnsignedBelowGlobal == false
            && equal.GuardBranchTaken == false
            && equal.GuardBranchSelectedDestinationRawFileOffset == 0x012C9097
            && equal.FirstStoreReached);

        CapacityFinalCalleeFinalPairedTailPlan highUnsigned =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 0xF0000000,
                freshGlobalForCompare: 0x10000000));
        check("A high-bit ECX above a smaller global is not unsigned-below",
            highUnsigned.CompareResult == 0xE0000000
            && highUnsigned.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highUnsigned.EntryECXUnsignedBelowGlobal == false
            && highUnsigned.GuardBranchTaken == false);

        CapacityFinalCalleeFinalPairedTailPlan overflow =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 0x80000000,
                freshGlobalForCompare: 1));
        check("CMP flags retain signed overflow, auxiliary carry, and low-byte parity",
            overflow.CompareResult == 0x7FFFFFFF
            && overflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            });

        CapacityFinalCalleeFinalPairedTailPlan missingEax =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(entryEax: null));
        check("Missing EAX stops at the first store before either ordered write",
            missingEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore
            && missingEax.CompareReached
            && missingEax.EntryECXUnsignedBelowGlobal == false
            && missingEax.GuardBranchTaken == false
            && !missingEax.EntryEAXCaptured
            && !missingEax.FirstStoreReached
            && !missingEax.SecondStoreReached
            && missingEax.Writes.IsEmpty
            && !missingEax.FinalJumpReached
            && !missingEax.ExactCleanExitAdmitted
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C9097);

        CapacityFinalCalleeFinalPairedTailPlan aliases =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 0xFFFFFFE8,
                freshGlobalForCompare: 0,
                entryEax: 0xFFFFFFEC));
        check("Wrapping paired stores preserve their order and may alias the same address",
            aliases.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget
            && aliases.FirstStoreAddress == 0
            && aliases.FirstStoreValue == 0xFFFFFFE8
            && aliases.SecondStoreAddress == 0
            && aliases.SecondStoreValue == 0xFFFFFFEC
            && aliases.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9097u, 0u, 0xFFFFFFE8u),
                    (0x012C909Au, 0u, 0xFFFFFFECu)
                })
            && CapacityFinalCalleeFinalPairedTailProjection.DeriveFirstStoreAddress(
                0xFFFFFFEC) == 0
            && CapacityFinalCalleeFinalPairedTailProjection.DeriveSecondStoreAddress(
                0xFFFFFFE8) == 0);

        CapacityFinalCalleeFinalPairedTailPlan exactExit =
            CapacityFinalCalleeFinalPairedTailProjection.Project(Capture(
                entryEcx: 0x200,
                freshGlobalForCompare: 0x100,
                entryEax: 0x300));
        check("The two writes are followed only by the unconditional jump to 8EF2",
            exactExit.FirstStoreReached
            && exactExit.SecondStoreReached
            && exactExit.Writes.Length == 2
            && exactExit.Writes[0]
                == new CapacityFinalCalleeFinalPairedTailWrite(
                    0x012C9097, 0x314, 0x200)
            && exactExit.Writes[1]
                == new CapacityFinalCalleeFinalPairedTailWrite(
                    0x012C909A, 0x218, 0x300)
            && exactExit.FinalJumpReached
            && exactExit.FinalJumpSelectedDestinationRawFileOffset == 0x012C8EF2
            && exactExit.Reads.Length == 1
            && exactExit.Reads[0].RawFileOffset == 0x012C908B
            && exactExit.ExactCleanExitAdmitted
            && exactExit.StopAtRawFileOffsetExclusive == 0x012C8EF2);
    }

    private static CapacityFinalCalleeFinalPairedTailCapture Capture(
        uint? entryEcx = 0x100,
        uint? freshGlobalForCompare = 0x80,
        uint? entryEax = 0x200) =>
        new(entryEcx, freshGlobalForCompare, entryEax);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C908B, "3B 0D D8 AD ED 01", 0x01EDADD8, null, null),
            (0x012C9091, "0F 82 DF FD FF FF", null, 0x012C8E76, 0x016C9A76),
            (0x012C9097, "89 48 14", null, null, null),
            (0x012C909A, "89 41 18", null, null, null),
            (0x012C909D, "E9 50 FE FF FF", null, 0x012C8EF2, 0x016C9AF2)
        };
        uint nextRawOffset =
            CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFinalPairedTailInstructionEvidence instruction
            in CapacityFinalCalleeFinalPairedTailProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFinalPairedTailProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.PeVirtualAddressOperand is not null
                    && instruction.PeVirtualAddressOperand != 0x01EDADD8)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeFinalPairedTailProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFinalPairedTailProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFinalPairedTailProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFinalPairedTailProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
