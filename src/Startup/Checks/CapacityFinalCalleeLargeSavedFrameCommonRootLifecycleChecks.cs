using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition stops at the common root gate destinations",
            CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection.EntryRawFileOffset
                == 0x012C9226
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .CommonTailCompletedBoundaryRawFileOffset == 0x012C92C8
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .RootCompareRawFileOffset == 0x012C92C8
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .RootFallthroughRawFileOffset == 0x012C92D0
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .RootBranchTargetRawFileOffset == 0x012C92EC
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .FreshGlobalDWORDVirtualAddress == 0x01EDADDC
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .NonzeroStopRawFileOffsetExclusive == 0x012C9231
            && CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .BoundaryRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan equalZero =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 0, entryESI: 0,
                freshGlobalDWORD: 0);
        check("Zero saved frame composes both tag stores and an equal root branch",
            equalZero.Parent.ExactCleanHandoffToCommonTail
            && equalZero.ExactCleanParentExit
            && equalZero.RootGateReached
            && equalZero.ChildEntryESIDerived
            && equalZero.ChildEntryESI == 0
            && equalZero.RootGate?.Capture.EntryESI == 0
            && equalZero.FreshGlobalReadReached
            && equalZero.FreshGlobalCaptured
            && equalZero.FreshGlobalDWORD == 0
            && equalZero.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                    .RootGateEqualToFallthrough92D0
            && equalZero.RootGate?.JnzTaken == false
            && equalZero.RootGate?.JnzSelectedDestinationRawFileOffset == 0x012C92D0
            && equalZero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalZero.OrderedWrites.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C92C2u, 4u, 1u),
                    (0x012C92C5u, 0u, 0u)
                })
            && equalZero.OrderedMemoryAccesses.Select(access =>
                (access.Stage, access.Kind, access.RawFileOffset, access.Address,
                    access.ValueCaptured, access.Value)).SequenceEqual(new[]
                {
                    (CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                        .SavedFrameGate,
                        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Read,
                        0x012C9226u, (uint?)0x10, true, (uint?)0),
                    (CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                        .CommonTailTagWrites,
                        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Write,
                        0x012C92C2u, (uint?)4, true, (uint?)1),
                    (CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                        .CommonTailTagWrites,
                        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Write,
                        0x012C92C5u, (uint?)0, true, (uint?)0),
                    (CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                        .CommonTailRootGate,
                        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Read,
                        0x012C92C8u, (uint?)0x01EDADDC, true, (uint?)0)
                })
            && equalZero.ExactCleanExitAdmitted
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C92D0);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan unequal =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 0x10, entryESI: 0x80000000,
                freshGlobalDWORD: 1);
        check("The root CMP flags select the exact bounded unequal destination",
            unequal.RootGate?.CompareResult == 0x7FFFFFFF
            && unequal.RootCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && unequal.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                    .RootGateUnequalToBranch92EC
            && unequal.RootGate?.JnzReached == true
            && unequal.RootGate?.JnzTaken == true
            && unequal.RootGate?.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && unequal.ExactCleanExitAdmitted
            && unequal.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan nonzero =
            Project(entryEBP: 0x20, savedFrame: 1, entryEDI: null, entryESI: null,
                freshGlobalDWORD: null);
        check("A nonzero saved frame remains bounded at 9231 without entering the common tail",
            nonzero.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome.NonzeroTo9231
            && nonzero.Parent.LargeSavedFrameGate.JzTaken == false
            && nonzero.Parent.LargeSavedFrameGate.JzSelectedDestinationRawFileOffset
                == 0x012C9231
            && nonzero.RootGate is null
            && nonzero.OrderedWrites.IsEmpty
            && nonzero.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan nonzeroUnused =
            Project(entryEBP: 0x20, savedFrame: 1, entryEDI: 0, entryESI: 0,
                freshGlobalDWORD: 0);
        check("Unreached caller and global captures block a clean nonzero admission",
            nonzeroUnused.RootGate is null
            && nonzeroUnused.Parent.UnreachedCallerEntryEDICaptureProvided
            && nonzeroUnused.Parent.UnreachedCallerEntryESICaptureProvided
            && nonzeroUnused.UnreachedFreshGlobalCaptureProvided
            && nonzeroUnused.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                    .NonzeroTo9231WithUnreachedCaptures
            && !nonzeroUnused.ExactCleanExitAdmitted
            && nonzeroUnused.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan missingEBP =
            Project(entryEBP: null, savedFrame: null, entryEDI: 0x10, entryESI: 0x20,
                freshGlobalDWORD: 0);
        check("Missing parent EBP preserves its first read and blocks every later stage",
            missingEBP.Parent.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingEntryEBP
            && missingEBP.Parent.LargeSavedFrameGate.Reads.Length == 1
            && missingEBP.Parent.LargeSavedFrameGate.Reads[0].AddressCaptured == false
            && missingEBP.RootGate is null
            && missingEBP.UnreachedFreshGlobalCaptureProvided
            && missingEBP.OrderedWrites.IsEmpty
            && !missingEBP.ExactCleanExitAdmitted
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan missingSavedFrame =
            Project(entryEBP: 0x20, savedFrame: null, entryEDI: null, entryESI: null,
                freshGlobalDWORD: null);
        check("A missing fresh saved-frame value blocks tag writes and the root gate",
            missingSavedFrame.Parent.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingFreshFrameMinus10
            && missingSavedFrame.Parent.LargeSavedFrameGate.FreshFrameMinus10Address == 0x10
            && missingSavedFrame.RootGate is null
            && missingSavedFrame.OrderedWrites.IsEmpty
            && !missingSavedFrame.ExactCleanExitAdmitted
            && missingSavedFrame.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan missingESI =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 0x10, entryESI: null,
                freshGlobalDWORD: 0);
        check("Missing parent ESI retains OR flags but leaves the independent child operand unreached",
            missingESI.Parent.CommonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI,
                OrInstructionReached: true,
                FirstStoreReached: false,
                SecondStoreReached: false
            }
            && missingESI.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingESI.RootGate is null
            && missingESI.UnreachedFreshGlobalCaptureProvided
            && !missingESI.FreshGlobalReadReached
            && missingESI.OrderedWrites.IsEmpty
            && !missingESI.ExactCleanExitAdmitted
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan missingGlobal =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 0x10, entryESI: 0x20,
                freshGlobalDWORD: null);
        check("A missing root global preserves parent writes and stops at 92C8",
            missingGlobal.RootGateReached
            && missingGlobal.FreshGlobalReadReached
            && missingGlobal.FreshGlobalAddress == 0x01EDADDC
            && !missingGlobal.FreshGlobalCaptured
            && missingGlobal.RootGate?.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD
            && missingGlobal.OrderedWrites.Length == 2
            && missingGlobal.RootCompareFlags is null
            && missingGlobal.FlagsAtStop == missingGlobal.ParentFlagsAtStop
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.OrderedMemoryAccesses[^1] is
            {
                Stage: CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleStage
                    .CommonTailRootGate,
                Kind: CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleAccessKind.Read,
                Address: 0x01EDADDC,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan wrapped =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 0xFFFFFFFE,
                entryESI: 0xFFFFFFFE, freshGlobalDWORD: 0xFFFFFFFE);
        check("Parent tag-store addresses wrap as DWORDs before an independent zero-capable compare",
            wrapped.OrderedWrites.Select(write => (write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (2u, 0xFFFFFFFFu),
                    (0xFFFFFFFCu, 0xFFFFFFFEu)
                })
            && wrapped.RootGate?.Capture.EntryESI == 0xFFFFFFFE
            && wrapped.RootGate?.FreshGlobalDWORD == 0xFFFFFFFE
            && wrapped.RootGate?.CompareResult == 0
            && wrapped.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleOutcome
                    .RootGateEqualToFallthrough92D0
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C92D0);

        uint globalAddress =
            CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection
                .FreshGlobalDWORDVirtualAddress;
        uint aliasingESI = unchecked(globalAddress - 4);
        CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan aliased =
            Project(entryEBP: 0x20, savedFrame: 0, entryEDI: 4, entryESI: aliasingESI,
                freshGlobalDWORD: 0xDEADBEEF);
        check("Aliased parent stores do not replace the independently captured global DWORD",
            aliased.OrderedWrites.Select(write => (write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (globalAddress, 5u),
                    (globalAddress, 4u)
                })
            && aliased.FreshGlobalReadReached
            && aliased.FreshGlobalAddress == globalAddress
            && aliased.FreshGlobalCaptured
            && aliased.FreshGlobalDWORD == 0xDEADBEEF
            && aliased.RootGate?.Capture.EntryESI == aliasingESI
            && aliased.RootGate?.Capture.FreshGlobalDWORD == 0xDEADBEEF
            && aliased.RootGate?.CompareResult == unchecked(aliasingESI - 0xDEADBEEF)
            && aliased.RootGate?.JnzSelectedDestinationRawFileOffset == 0x012C92EC
            && aliased.ExactCleanExitAdmitted
            && aliased.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeCommonTailRootGatePlan independentMissingESI =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: null,
                FreshGlobalDWORD: 0));
        check("The root-gate helper retains its independent captured-zero read when ESI is absent",
            independentMissingESI.Outcome
                == CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
            && independentMissingESI.FreshGlobalReadReached
            && independentMissingESI.FreshGlobalCaptured
            && independentMissingESI.FreshGlobalDWORD == 0
            && !independentMissingESI.CompareReached
            && independentMissingESI.StopAtRawFileOffsetExclusive == 0x012C92C8);
    }

    private static CapacityFinalCalleeLargeSavedFrameCommonRootLifecyclePlan Project(
        uint? entryEBP,
        uint? savedFrame,
        uint? entryEDI,
        uint? entryESI,
        uint? freshGlobalDWORD) =>
        CapacityFinalCalleeLargeSavedFrameCommonRootLifecycleProjection.Project(new(
            new(
                new(entryEBP, savedFrame),
                entryEDI,
                entryESI),
            freshGlobalDWORD));
}
