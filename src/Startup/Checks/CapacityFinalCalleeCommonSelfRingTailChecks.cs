using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonSelfRingTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The instruction bytes, offsets, global address, and jump destination match",
            CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset
                == 0x012C93AC
            && CapacityFinalCalleeCommonSelfRingTailProjection.EntryVirtualAddress
                == 0x016C9FAC
            && CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStorePEVirtualAddress
                == 0x01EDADCC
            && CapacityFinalCalleeCommonSelfRingTailProjection.BoundaryRawFileOffsetExclusive
                == 0x012C93C0
            && CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetRawFileOffset
                == 0x012C942F
            && CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetVirtualAddress
                == 0x016CA02F
            && CapacityFinalCalleeCommonSelfRingTailProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset)
                == 0x016C9FAC
            && Normalize(
                CapacityFinalCalleeCommonSelfRingTailProjection.CapturedWindowBytes)
                == "0BC2A3CCADED018933895E1889760C897608EB6F"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonSelfRingTailPlan missingBoth =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(null, null, 0x10, 0x20));
        check("Missing OR operands stop before the global store and retain supplied values",
            missingBoth.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothEntryOperands
            && !missingBoth.EntryEAXCaptured
            && !missingBoth.EntryEDXCaptured
            && !missingBoth.OrReached
            && missingBoth.OrEAX is null
            && missingBoth.OrEDX is null
            && missingBoth.OrResult is null
            && !missingBoth.GlobalStoreReached
            && missingBoth.Writes.IsEmpty
            && missingBoth.UnreachedLateEBXCaptureProvided
            && missingBoth.UnreachedLateESICaptureProvided
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C93AC);

        CapacityFinalCalleeCommonSelfRingTailPlan missingEAX =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(null, 0x40, 0, null));
        check("A missing EAX does not discard captured EDX or consume later pointers",
            missingEAX.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingEntryEAX
            && !missingEAX.EntryEAXCaptured
            && missingEAX.EntryEDXCaptured
            && missingEAX.OrEAX is null
            && missingEAX.OrEDX == 0x40
            && !missingEAX.OrReached
            && !missingEAX.LateEBXCaptured
            && !missingEAX.LateESICaptured
            && missingEAX.UnreachedLateEBXCaptureProvided
            && !missingEAX.UnreachedLateESICaptureProvided
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C93AC);

        CapacityFinalCalleeCommonSelfRingTailPlan missingEDX =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(0x80, null, null, 0));
        check("A missing EDX independently retains EAX and the unconsumed ESI capture",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingEntryEDX
            && missingEDX.EntryEAXCaptured
            && !missingEDX.EntryEDXCaptured
            && missingEDX.OrEAX == 0x80
            && missingEDX.OrEDX is null
            && !missingEDX.OrReached
            && !missingEDX.LateEBXCaptured
            && !missingEDX.LateESICaptured
            && !missingEDX.UnreachedLateEBXCaptureProvided
            && missingEDX.UnreachedLateESICaptureProvided
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C93AC);

        CapacityFinalCalleeCommonSelfRingTailPlan missingBothLate =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(0x100, 0x20, null, null));
        check("The global OR store commits before missing late pointers stop at the first ring store",
            missingBothLate.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothLatePointers
            && missingBothLate.OrResult == 0x120
            && missingBothLate.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingBothLate.GlobalStoreReached
            && missingBothLate.GlobalStoreAddress == 0x01EDADCC
            && missingBothLate.GlobalStoreValue == 0x120
            && missingBothLate.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonSelfRingTailWrite(
                    0x012C93AE,
                    0x01EDADCC,
                    0x120)
            })
            && !missingBothLate.FirstLocalStoreReached
            && missingBothLate.FirstLocalStoreAttempted
            && !missingBothLate.FirstLocalStoreCommitted
            && missingBothLate.FlagsPreserved
            && missingBothLate.FlagsAtStop == missingBothLate.OrFlags
            && !missingBothLate.ExactBoundedExitAdmitted
            && missingBothLate.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonSelfRingTailPlan missingEBX =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(1, 2, null, 0x30));
        check("First-store value remains independently captured when its EBX address is missing",
            missingEBX.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateEBX
            && missingEBX.GlobalStoreReached
            && missingEBX.LateESICaptured
            && !missingEBX.LateEBXCaptured
            && missingEBX.FirstLocalStoreAddress is null
            && missingEBX.FirstLocalStoreValue == 0x30
            && !missingEBX.FirstLocalStoreReached
            && missingEBX.Writes.Length == 1
            && missingEBX.FlagsAtStop == missingEBX.OrFlags
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonSelfRingTailPlan missingESI =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(1, 2, 0x40, null));
        check("First-store EBX address remains independently captured when its value is missing",
            missingESI.Outcome
                == CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateESI
            && missingESI.GlobalStoreReached
            && missingESI.LateEBXCaptured
            && !missingESI.LateESICaptured
            && missingESI.FirstLocalStoreAddress == 0x40
            && missingESI.FirstLocalStoreValue is null
            && !missingESI.FirstLocalStoreReached
            && missingESI.Writes.Length == 1
            && missingESI.FlagsAtStop == missingESI.OrFlags
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonSelfRingTailPlan zeroPointers =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(new(0, 0, 0, 0));
        check("Captured zero pointers are valid and all zero-ring writes remain explicit",
            zeroPointers.Outcome == CapacityFinalCalleeCommonSelfRingTailOutcome.Completed
            && zeroPointers.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroPointers.FirstLocalStoreReached
            && zeroPointers.FirstLocalStoreCommitted
            && zeroPointers.FirstLocalStoreAddress == 0
            && zeroPointers.FirstLocalStoreValue == 0
            && zeroPointers.SecondLocalStoreAddress == 0x18
            && zeroPointers.ThirdLocalStoreAddress == 0x0C
            && zeroPointers.FourthLocalStoreAddress == 8
            && zeroPointers.Writes.Length == 5
            && zeroPointers.JumpReached
            && !zeroPointers.JumpTargetEntered
            && zeroPointers.ExactBoundedExitAdmitted
            && zeroPointers.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonSelfRingTailPlan highBits =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(0x80000000, 1, 0x10, 0x80000000));
        check("The high result bit and low-byte odd parity are reflected in exact OR flags",
            highBits.OrResult == 0x80000001
            && highBits.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBits.FlagsAtStop == highBits.OrFlags
            && highBits.FlagsPreserved
            && highBits.JumpReached
            && highBits.JumpTargetRawFileOffset == 0x012C942F
            && highBits.JumpTargetVirtualAddress == 0x016CA02F);

        CapacityFinalCalleeCommonSelfRingTailPlan aliasedWrapped =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(3, 0, 8, 0xFFFFFFF0));
        check("Aliased DWORD-wrapped stores remain separate and in instruction order",
            aliasedWrapped.SecondLocalStoreAddress == 8
            && aliasedWrapped.ThirdLocalStoreAddress == 0xFFFFFFFC
            && aliasedWrapped.FourthLocalStoreAddress == 0xFFFFFFF8
            && aliasedWrapped.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C93AEu, 0x01EDADCCu, 3u),
                    (0x012C93B3u, 8u, 0xFFFFFFF0u),
                    (0x012C93B5u, 8u, 8u),
                    (0x012C93B8u, 0xFFFFFFFCu, 0xFFFFFFF0u),
                    (0x012C93BBu, 0xFFFFFFF8u, 0xFFFFFFF0u)
                })
            && aliasedWrapped.Writes.Length == 5
            && aliasedWrapped.FlagsPreserved
            && aliasedWrapped.JumpReached);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? MemoryOperandPEVA,
            uint? BranchTargetRaw,
            uint? BranchTargetVA)[]
        {
            (0x012C93AC, "0B C2", null, null, null),
            (0x012C93AE, "A3 CC AD ED 01", 0x01EDADCC, null, null),
            (0x012C93B3, "89 33", null, null, null),
            (0x012C93B5, "89 5E 18", null, null, null),
            (0x012C93B8, "89 76 0C", null, null, null),
            (0x012C93BB, "89 76 08", null, null, null),
            (0x012C93BE, "EB 6F", null, 0x012C942F, 0x016CA02F)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonSelfRingTailInstructionEvidence instruction
            in CapacityFinalCalleeCommonSelfRingTailProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonSelfRingTailProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonSelfRingTailProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonSelfRingTailProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonSelfRingTailProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonSelfRingTailProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonSelfRingTailProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.MemoryOperandPEVirtualAddress,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
