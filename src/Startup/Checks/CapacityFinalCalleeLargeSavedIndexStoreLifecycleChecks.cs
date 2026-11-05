using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedIndexStoreLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact parent equality edge joins the child store gate at 9243",
            CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
                .StoreGateEntryRawFileOffset == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
                .ParentUnequalDestinationRawFileOffset == 0x012C9263
            && CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
                .ParentEqualDestinationRawFileOffset == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
                .ChildZeroDestinationRawFileOffset == 0x012C9251
            && CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection
                .ChildNonzeroDestinationRawFileOffset == 0x012C927C);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan missingParent =
            Project(entryEDX: null, parentFreshIndex: 5, parentIndexedDword: 0,
                parentGlobalValue: 9, childFreshIndex: 11, childEntryEAX: 12);
        check("Missing parent EDX blocks the child and retains supplied child captures as unused",
            missingParent.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .ParentGateIncomplete
            && missingParent.SavedIndexGate.Outcome
                == CapacityFinalCalleeLargeSavedIndexGateOutcome
                    .MissingEntryEDXForFreshRead
            && !missingParent.StoreGateReached
            && missingParent.UnreachedChildFreshEDXPlus1CCaptureProvided
            && missingParent.UnreachedChildEntryEAXCaptureProvided
            && !missingParent.ChildEntryEAXCaptured
            && missingParent.OrderedReads.Length == 1
            && missingParent.OrderedReads[0].Stage
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage.SavedIndexGate
            && missingParent.OrderedWrites.IsEmpty
            && missingParent.FlagsAtStop is null
            && !missingParent.ExactCleanExitAdmitted
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan parentUnequal =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x101,
                parentGlobalValue: 0x100, childFreshIndex: 7, childEntryEAX: 8);
        check("The unequal parent stops bounded at 9263 and rejects unused child inputs",
            parentUnequal.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .ParentUnequalTo9263Contaminated
            && parentUnequal.SavedIndexGate.JnzTaken == true
            && parentUnequal.SavedIndexGate.JnzSelectedDestinationRawFileOffset
                == 0x012C9263
            && !parentUnequal.StoreGateReached
            && parentUnequal.UnreachedChildFreshEDXPlus1CCaptureProvided
            && parentUnequal.UnreachedChildEntryEAXCaptureProvided
            && parentUnequal.ChildEntryEAXCaptureRejectedAtParentExit
            && parentUnequal.OrderedReads.Length == 3
            && parentUnequal.OrderedWrites.IsEmpty
            && parentUnequal.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && !parentUnequal.ExactCleanExitAdmitted
            && parentUnequal.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan cleanParentUnequal =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x101,
                parentGlobalValue: 0x100, childFreshIndex: null, childEntryEAX: null);
        check("An uncontaminated unequal parent admits only its bounded 9263 exit",
            cleanParentUnequal.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .ParentUnequalTo9263
            && !cleanParentUnequal.StoreGateReached
            && cleanParentUnequal.ExactCleanExitAdmitted
            && cleanParentUnequal.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan equalWithDistinctChildIndex =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x100,
                parentGlobalValue: 0xABCDEF01, childFreshIndex: 9, childEntryEAX: 0x1234);
        check("Equality passes original parent EDX while the aliased child load supplies a new index",
            equalWithDistinctChildIndex.ExactCleanHandoffToStoreGate
            && equalWithDistinctChildIndex.StoreGateReached
            && equalWithDistinctChildIndex.ChildEntryEDX == 0x100
            && equalWithDistinctChildIndex.ChildEntryEDXSource
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEDXSource
                    .ParentOriginalEntryEDXFromReachedFirstLoad
            && equalWithDistinctChildIndex.SavedIndexGate.FreshEDXPlus1CAddress == 0x11C
            && equalWithDistinctChildIndex.SavedIndexGate.EBXAfterFreshEDXPlus1C == 5
            && equalWithDistinctChildIndex.SavedIndexGate.EBXAfterFreshGlobalReload
                == 0xABCDEF01
            && equalWithDistinctChildIndex.SavedIndexedStoreGate?.FreshIndexReadAddress
                == 0x11C
            && equalWithDistinctChildIndex.SavedIndexedStoreGate?.FreshIndex == 9
            && equalWithDistinctChildIndex.ChildFreshEDXPlus1C == 9
            && equalWithDistinctChildIndex.ChildFreshIndexSource
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildFreshIndexSource
                    .IndependentChildEDXPlus1CRead
            && equalWithDistinctChildIndex.ChildEntryEAXSource
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleChildEAXSource
                    .ExplicitIndependentCaptureAtReachedStore
            && equalWithDistinctChildIndex.SavedIndexedStoreGate?.StoreAddress
                == 0x01EDAF1C
            && equalWithDistinctChildIndex.SavedIndexedStoreGate?.StoreValue == 0x1234
            && equalWithDistinctChildIndex.ChildEntryEAXCaptured
            && equalWithDistinctChildIndex.OrderedReads.Length == 4
            && equalWithDistinctChildIndex.OrderedReads[0].Address
                == equalWithDistinctChildIndex.OrderedReads[3].Address
            && equalWithDistinctChildIndex.OrderedReads[0].Value == 5
            && equalWithDistinctChildIndex.OrderedReads[3].Value == 9
            && equalWithDistinctChildIndex.OrderedWrites is
                [{
                    Stage: CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
                        .SavedIndexedStoreGate,
                    RawFileOffset: 0x012C9246,
                    Address: 0x01EDAF1C,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: 0x1234,
                    Committed: true
                }]
            && equalWithDistinctChildIndex.ReachedInstructions.SequenceEqual(
                new uint[]
                {
                    0x012C9231, 0x012C9234, 0x012C923B, 0x012C9241,
                    0x012C9243, 0x012C9246, 0x012C924D, 0x012C924F
                })
            && equalWithDistinctChildIndex.ExactCleanExitAdmitted
            && equalWithDistinctChildIndex.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan missingChildIndex =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x100,
                parentGlobalValue: 8, childFreshIndex: null, childEntryEAX: 0x55);
        check("A missing child index stops at its load and keeps EAX and parent CMP flags",
            missingChildIndex.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .MissingChildFreshEDXPlus1C
            && missingChildIndex.SavedIndexedStoreGate?.FreshIndexReadReached == true
            && missingChildIndex.SavedIndexedStoreGate?.FreshIndexReadAddress == 0x11C
            && missingChildIndex.SavedIndexedStoreGate?.FreshEDXPlus1CCaptured == false
            && missingChildIndex.UnreachedChildEntryEAXCaptureProvided
            && !missingChildIndex.ChildEntryEAXCaptured
            && missingChildIndex.OrderedReads.Length == 4
            && missingChildIndex.OrderedWrites.IsEmpty
            && missingChildIndex.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingChildIndex.ExactCleanExitAdmitted
            && missingChildIndex.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan missingChildEAX =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x100,
                parentGlobalValue: 8, childFreshIndex: 3, childEntryEAX: null);
        check("A missing child EAX retains the reached 9246 store attempt with unknown value",
            missingChildEAX.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .MissingChildEntryEAXForStore
            && missingChildEAX.SavedIndexedStoreGate?.StoreInstructionReached == true
            && missingChildEAX.SavedIndexedStoreGate?.StoreAddress == 0x01EDAF04
            && missingChildEAX.OrderedReads.Length == 4
            && missingChildEAX.OrderedWrites is
                [{
                    Stage: CapacityFinalCalleeLargeSavedIndexStoreLifecycleStage
                        .SavedIndexedStoreGate,
                    RawFileOffset: 0x012C9246,
                    Address: 0x01EDAF04,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && missingChildEAX.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingChildEAX.SavedIndexedStoreGate!.TestReached
            && !missingChildEAX.ExactCleanExitAdmitted
            && missingChildEAX.StopAtRawFileOffsetExclusive == 0x012C9246);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan zeroEAX =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x100,
                parentGlobalValue: 6, childFreshIndex: 0, childEntryEAX: 0);
        check("Zero EAX is stored before TEST and falls through only to 9251",
            zeroEAX.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .ChildZeroToFallthrough9251
            && zeroEAX.SavedIndexedStoreGate?.StoreValue == 0
            && zeroEAX.SavedIndexedStoreGate?.TestResult == 0
            && zeroEAX.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroEAX.SavedIndexedStoreGate?.JnzTaken == false
            && zeroEAX.StopAtRawFileOffsetExclusive == 0x012C9251
            && zeroEAX.ExactCleanExitAdmitted
            && !zeroEAX.FuturePathsReached);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan nonzeroEAX =
            Project(entryEDX: 0x100, parentFreshIndex: 5, parentIndexedDword: 0x100,
                parentGlobalValue: 6, childFreshIndex: 4, childEntryEAX: 0x80000000);
        check("Nonzero EAX uses child TEST flags with undefined AF and stops at 927C",
            nonzeroEAX.Outcome
                == CapacityFinalCalleeLargeSavedIndexStoreLifecycleOutcome
                    .ChildNonzeroTo927C
            && nonzeroEAX.SavedIndexedStoreGate?.StoreValue == 0x80000000
            && nonzeroEAX.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroEAX.SavedIndexedStoreGate?.JnzTaken == true
            && nonzeroEAX.SavedIndexedStoreGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C927C
            && nonzeroEAX.StopAtRawFileOffsetExclusive == 0x012C927C
            && nonzeroEAX.ExactCleanExitAdmitted
            && !nonzeroEAX.FuturePathsReached);

        CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan wrapped =
            Project(entryEDX: 0xFFFFFFF0, parentFreshIndex: 0xFFFFFFFF,
                parentIndexedDword: 0xFFFFFFF0, parentGlobalValue: 0x12345678,
                childFreshIndex: uint.MaxValue, childEntryEAX: uint.MaxValue);
        check("The child reload and maximum scaled index wrap as DWORDs independently",
            wrapped.SavedIndexGate.FreshEDXPlus1CAddress == 0xC
            && wrapped.SavedIndexedStoreGate?.FreshIndexReadAddress == 0xC
            && wrapped.SavedIndexGate.EBXAfterFreshEDXPlus1C == uint.MaxValue
            && wrapped.SavedIndexGate.EBXAfterFreshGlobalReload == 0x12345678
            && wrapped.SavedIndexedStoreGate?.FreshIndex == uint.MaxValue
            && wrapped.SavedIndexedStoreGate?.StoreAddress == 0x01EDAEF4
            && wrapped.SavedIndexedStoreGate?.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C927C);
    }

    private static CapacityFinalCalleeLargeSavedIndexStoreLifecyclePlan Project(
        uint? entryEDX,
        uint? parentFreshIndex,
        uint? parentIndexedDword,
        uint? parentGlobalValue,
        uint? childFreshIndex,
        uint? childEntryEAX) =>
        CapacityFinalCalleeLargeSavedIndexStoreLifecycleProjection.Project(new(
            new(
                EntryEDX: entryEDX,
                FreshEDXPlus1C: parentFreshIndex,
                FreshIndexedDword: parentIndexedDword,
                FreshGlobalValue: parentGlobalValue),
            ChildFreshEDXPlus1C: childFreshIndex,
            ChildEntryEAX: childEntryEAX));
}
