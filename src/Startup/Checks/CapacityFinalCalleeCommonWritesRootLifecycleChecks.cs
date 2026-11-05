using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonWritesRootLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The common writes join the root gate at 92C8 and retain exact branch roots",
            CapacityFinalCalleeCommonWritesRootLifecycleProjection.EntryRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection.EntryVirtualAddress
                == 0x016C9EBD
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .CommonTailBoundaryRawFileOffsetExclusive == 0x012C92C8
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection.RootCompareRawFileOffset
                == 0x012C92C8
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .BoundaryRawFileOffsetExclusive == 0x012C92D0
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .FallthroughRawFileOffset == 0x012C92D0
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .BranchTargetRawFileOffset == 0x012C92EC
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .FreshGlobalDWORDVirtualAddress == 0x01EDADDC
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .RawFileOffsetToVirtualAddress(
                    CapacityFinalCalleeCommonWritesRootLifecycleProjection.EntryRawFileOffset)
                == 0x016C9EBD
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonWritesRootLifecyclePlan missingEDI =
            Project(entryEDI: null, entryESI: 0x20, freshGlobalDWORD: 7);
        check("Missing tag EDI blocks the child and marks the supplied fresh global unreached",
            missingEDI.CommonTailTagWrites.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && missingEDI.CommonTailTagWrites.UnreachedEntryESICaptureProvided
            && !missingEDI.RootGateReached
            && !missingEDI.ChildEntryESIRetained
            && missingEDI.UnreachedFreshGlobalCaptureProvided
            && !missingEDI.FreshGlobalReadReached
            && missingEDI.OrderedWrites.IsEmpty
            && missingEDI.OrFlags is null
            && !missingEDI.ExactCleanAdmission
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeCommonWritesRootLifecyclePlan missingESI =
            Project(entryEDI: 0, entryESI: null, freshGlobalDWORD: 0);
        check("Missing tag ESI preserves OR flags but blocks both stores and the child",
            missingESI.CommonTailTagWrites.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI
            && missingESI.CommonTailTagWrites.EAXAfterOr == 1
            && missingESI.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && !missingESI.CommonTailTagWrites.FirstStoreReached
            && !missingESI.CommonTailTagWrites.SecondStoreReached
            && missingESI.OrderedWrites.IsEmpty
            && !missingESI.RootGateReached
            && missingESI.UnreachedFreshGlobalCaptureProvided
            && !missingESI.FreshGlobalReadReached
            && !missingESI.ExactCleanAdmission
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeCommonWritesRootLifecyclePlan missingGlobal =
            Project(entryEDI: 0x10, entryESI: 0x20, freshGlobalDWORD: null);
        check("A missing fresh global read keeps the OR flags and both ordered writes",
            missingGlobal.CommonTailTagWrites.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed
            && missingGlobal.RootGateReached
            && missingGlobal.ChildEntryESIRetained
            && missingGlobal.ChildEntryESIDerived
            && missingGlobal.ChildEntryESI == 0x20
            && missingGlobal.CommonTailRootGate?.Capture.EntryESI == 0x20
            && missingGlobal.CommonTailRootGate?.FreshGlobalDWORD is null
            && missingGlobal.FreshGlobalReadReached
            && missingGlobal.FreshGlobalAddress == 0x01EDADDC
            && !missingGlobal.FreshGlobalCaptured
            && missingGlobal.CommonTailRootGate?.CompareReached == false
            && missingGlobal.OrFlags == missingGlobal.CommonTailTagWrites.OrFlags
            && missingGlobal.FlagsAtStop == missingGlobal.OrFlags
            && missingGlobal.OrderedWrites.Select(write =>
                (write.Kind, write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (CapacityFinalCalleeCommonWritesRootLifecycleWriteKind
                        .CommonTailFirstStore, 0x012C92C2u, 0x24u, 0x11u),
                    (CapacityFinalCalleeCommonWritesRootLifecycleWriteKind
                        .CommonTailSecondStore, 0x012C92C5u, 0x30u, 0x10u)
                })
            && !missingGlobal.ExactCleanAdmission
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeCommonWritesRootLifecyclePlan capturedZero =
            Project(entryEDI: 0, entryESI: 0, freshGlobalDWORD: 0);
        check("Captured zero is distinct from a missing global and takes the exact 92D0 route",
            capturedZero.FreshGlobalReadReached
            && capturedZero.FreshGlobalCaptured
            && capturedZero.FreshGlobalDWORD == 0
            && capturedZero.CommonTailRootGate?.CompareReached == true
            && capturedZero.RootCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && capturedZero.Outcome
                == CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                    .RootGateEqualToFallthrough92D0
            && capturedZero.CommonTailRootGate?.JnzTaken == false
            && capturedZero.CommonTailRootGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C92D0
            && capturedZero.ExactCleanAdmission
            && capturedZero.FlagsAtStop is { Zero: true, AuxiliaryCarry: false }
            && capturedZero.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeCommonWritesRootLifecyclePlan unsignedUnderflow =
            Project(entryEDI: 0, entryESI: 0, freshGlobalDWORD: 1);
        check("All six CMP flags are preserved and the unequal route stops exactly at 92EC",
            unsignedUnderflow.RootCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedUnderflow.Outcome
                == CapacityFinalCalleeCommonWritesRootLifecycleOutcome
                    .RootGateUnequalToBranch92EC
            && unsignedUnderflow.CommonTailRootGate?.JnzTaken == true
            && unsignedUnderflow.CommonTailRootGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C92EC
            && unsignedUnderflow.ExactCleanAdmission
            && unsignedUnderflow.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonWritesRootLifecyclePlan wrapped =
            Project(entryEDI: 0xFFFFFFFE, entryESI: 0xFFFFFFFE,
                freshGlobalDWORD: 0xFFFFFFFC);
        check("Both store addresses use unchecked DWORD wrapping",
            wrapped.OrderedWrites.Select(write => (write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (2u, 0xFFFFFFFFu),
                    (0xFFFFFFFCu, 0xFFFFFFFEu)
                })
            && wrapped.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrapped.CommonTailRootGate?.Capture.EntryESI == 0xFFFFFFFE
            && wrapped.CommonTailRootGate?.FreshGlobalDWORD == 0xFFFFFFFC
            && wrapped.CommonTailRootGate?.CompareResult == 2);

        uint aliasedGlobalAddress =
            CapacityFinalCalleeCommonWritesRootLifecycleProjection
                .FreshGlobalDWORDVirtualAddress;
        uint aliasingEntryESI = unchecked(aliasedGlobalAddress - 4);
        CapacityFinalCalleeCommonWritesRootLifecyclePlan aliasedStores =
            Project(entryEDI: 4, entryESI: aliasingEntryESI,
                freshGlobalDWORD: 0xDEADBEEF);
        check("Aliased stores remain ordered while 1EDADDC is read independently afterward",
            aliasedStores.OrderedWrites.Select(write =>
                (write.Address, write.Value)).SequenceEqual(new[]
                {
                    (aliasedGlobalAddress, 5u),
                    (aliasedGlobalAddress, 4u)
                })
            && aliasedStores.FreshGlobalReadReached
            && aliasedStores.FreshGlobalAddress == aliasedGlobalAddress
            && aliasedStores.FreshGlobalCaptured
            && aliasedStores.FreshGlobalDWORD == 0xDEADBEEF
            && aliasedStores.CommonTailRootGate?.Capture.EntryESI == aliasingEntryESI
            && aliasedStores.CommonTailRootGate?.Capture.FreshGlobalDWORD == 0xDEADBEEF
            && aliasedStores.CommonTailRootGate?.CompareResult
                == unchecked(aliasingEntryESI - 0xDEADBEEF)
            && aliasedStores.CommonTailRootGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C92EC);
    }

    private static CapacityFinalCalleeCommonWritesRootLifecyclePlan Project(
        uint? entryEDI,
        uint? entryESI,
        uint? freshGlobalDWORD) =>
        CapacityFinalCalleeCommonWritesRootLifecycleProjection.Project(new(
            entryEDI,
            entryESI,
            freshGlobalDWORD));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new[]
        {
            (0x012C92BDu, "8B C7", (uint?)null),
            (0x012C92BFu, "83 C8 01", (uint?)null),
            (0x012C92C2u, "89 46 04", (uint?)null),
            (0x012C92C5u, "89 3C 37", (uint?)null),
            (0x012C92C8u, "3B 35 DC AD ED 01", (uint?)null),
            (0x012C92CEu, "75 1C", (uint?)0x012C92EC)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonWritesRootLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonWritesRootLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeCommonWritesRootLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonWritesRootLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonWritesRootLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonWritesRootLifecycleProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonWritesRootLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonWritesRootLifecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
