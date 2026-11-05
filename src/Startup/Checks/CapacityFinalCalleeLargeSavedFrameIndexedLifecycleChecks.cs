using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The saved-frame nonzero edge joins the indexed gate at 9231",
            CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
                .IndexedGateEntryRawFileOffset == 0x012C9231
            && CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
                .ParentZeroDestinationRawFileOffset == 0x012C92BD
            && CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
                .ParentNonzeroDestinationRawFileOffset == 0x012C9231
            && CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
                .ChildEqualDestinationRawFileOffset == 0x012C9243
            && CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection
                .ChildUnequalDestinationRawFileOffset == 0x012C9263);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingParent =
            Project(entryEBP: null, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x100, freshGlobalValue: 7);
        check("Missing parent EBP blocks child reads and records every unreached child capture",
            missingParent.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .LargeSavedFrameGateIncomplete
            && missingParent.LargeSavedIndexGate is null
            && missingParent.UnreachedChildEntryEDXCaptureProvided
            && missingParent.UnreachedFreshEDXPlus1CCaptureProvided
            && missingParent.UnreachedFreshIndexedDwordCaptureProvided
            && missingParent.UnreachedFreshGlobalValueCaptureProvided
            && missingParent.OrderedReads.Length == 1
            && missingParent.OrderedReads[0].Stage
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage
                    .LargeSavedFrameGate
            && !missingParent.ExactCleanExitAdmitted
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C9226
            && !missingParent.FutureStoresReached
            && !missingParent.EpilogueReached);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingParentValue =
            Project(entryEBP: 0x100, freshFrameMinus10: null,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x100, freshGlobalValue: 7);
        check("Missing fresh parent data cannot admit the indexed child",
            missingParentValue.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingFreshFrameMinus10
            && missingParentValue.LargeSavedIndexGate is null
            && missingParentValue.UnreachedChildEntryEDXCaptureProvided
            && !missingParentValue.ExactCleanExitAdmitted
            && missingParentValue.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan zeroParent =
            Project(entryEBP: 0x100, freshFrameMinus10: 0,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x100, freshGlobalValue: 7);
        check("The zero parent stops at 92BD and treats all child captures as contamination",
            zeroParent.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .ParentZeroTo92BDContaminated
            && zeroParent.LargeSavedFrameGate.JzTaken == true
            && zeroParent.LargeSavedFrameGate.JzSelectedDestinationRawFileOffset
                == 0x012C92BD
            && zeroParent.LargeSavedIndexGate is null
            && zeroParent.UnreachedChildEntryEDXCaptureProvided
            && zeroParent.UnreachedFreshEDXPlus1CCaptureProvided
            && zeroParent.UnreachedFreshIndexedDwordCaptureProvided
            && zeroParent.UnreachedFreshGlobalValueCaptureProvided
            && zeroParent.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !zeroParent.ExactCleanExitAdmitted
            && zeroParent.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan cleanZeroParent =
            Project(entryEBP: 0x100, freshFrameMinus10: 0,
                childEntryEDX: null, freshEDXPlus1C: null,
                freshIndexedDword: null, freshGlobalValue: null);
        check("An uncontaminated zero parent is clean only to its bounded 92BD stop",
            cleanZeroParent.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .ParentZeroTo92BD
            && cleanZeroParent.LargeSavedIndexGate is null
            && cleanZeroParent.ExactCleanExitAdmitted
            && cleanZeroParent.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingChildEDX =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: null, freshEDXPlus1C: 5,
                freshIndexedDword: 0x100, freshGlobalValue: 7);
        check("Child entry EDX is explicitly captured and never inferred from parent ECX",
            missingChildEDX.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .MissingChildEntryEDX
            && missingChildEDX.ExactCleanHandoffToIndexedGate
            && missingChildEDX.LargeSavedIndexGate?.Capture.EntryEDX is null
            && missingChildEDX.ChildEntryEDX is null
            && !missingChildEDX.ChildEntryEDXCaptured
            && missingChildEDX.ChildEntryEDXSource
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource.None
            && missingChildEDX.LargeSavedIndexGate?.FreshEDXPlus1CAddress is null
            && missingChildEDX.LargeSavedIndexGate?.FreshGlobalReadReached == false
            && missingChildEDX.UnreachedFreshIndexedDwordCaptureProvided
            && missingChildEDX.UnreachedFreshGlobalValueCaptureProvided
            && missingChildEDX.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildEDX.OrderedReads.Length == 2
            && missingChildEDX.OrderedReads[1].Stage
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleReadStage
                    .LargeSavedIndexGate
            && missingChildEDX.OrderedReads[1].AddressCaptured == false
            && !missingChildEDX.ExactCleanExitAdmitted
            && missingChildEDX.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingChildValue =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: null,
                freshIndexedDword: 0x100, freshGlobalValue: 7);
        check("A missing fresh EDX+1C stops at 9231 without substituting parent ECX",
            missingChildValue.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .MissingChildFreshEDXPlus1C
            && missingChildValue.ChildEntryEDX == 0x100
            && missingChildValue.ChildEntryEDXSource
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource
                    .ExplicitIndependentCapture
            && missingChildValue.LargeSavedIndexGate?.FreshEDXPlus1CCaptured == false
            && missingChildValue.UnreachedFreshIndexedDwordCaptureProvided
            && missingChildValue.UnreachedFreshGlobalValueCaptureProvided
            && missingChildValue.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingIndex =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: null, freshGlobalValue: 7);
        check("Missing indexed data preserves parent TEST flags with undefined AF",
            missingIndex.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .MissingChildFreshIndexedDword
            && missingIndex.LargeSavedIndexGate?.FreshIndexedReadReached == true
            && missingIndex.LargeSavedIndexGate?.CompareReached == false
            && missingIndex.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingIndex.StopAtRawFileOffsetExclusive == 0x012C9234);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan missingGlobal =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x101, freshGlobalValue: null);
        check("Missing global reload retains the defined child CMP flags instead of parent TEST flags",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .MissingChildFreshGlobalValue
            && missingGlobal.LargeSavedIndexGate?.CompareReached == true
            && missingGlobal.LargeSavedIndexGate?.FreshGlobalReadReached == true
            && missingGlobal.LargeSavedIndexGate?.FreshGlobalValueCaptured == false
            && missingGlobal.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C923B
            && !missingGlobal.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan equal =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x100, freshGlobalValue: 0xABCD);
        check("An equal child CMP preserves its flags through the independent global reload and stops at 9243",
            equal.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .ChildEqualToFallthrough9243
            && equal.ChildEntryEDX == 0x100
            && equal.ChildEntryEDXSource
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleChildEDXSource
                    .ExplicitIndependentCapture
            && equal.LargeSavedIndexGate?.CompareLeft == 0x100
            && equal.LargeSavedIndexGate?.CompareRight == 0x100
            && equal.LargeSavedIndexGate?.EBXAfterFreshGlobalReload == 0xABCD
            && equal.LargeSavedIndexGate?.JnzTaken == false
            && equal.LargeSavedIndexGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C9243
            && equal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.ExactCleanExitAdmitted
            && equal.StopAtRawFileOffsetExclusive == 0x012C9243
            && !equal.FutureStoresReached
            && !equal.EpilogueReached);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan unequal =
            Project(entryEBP: 0x100, freshFrameMinus10: 1,
                childEntryEDX: 0x100, freshEDXPlus1C: 5,
                freshIndexedDword: 0x101, freshGlobalValue: 0x100);
        check("An unequal child CMP uses its own flags for the 9263 branch despite the matching reload",
            unequal.Outcome
                == CapacityFinalCalleeLargeSavedFrameIndexedLifecycleOutcome
                    .ChildUnequalToBranch9263
            && unequal.LargeSavedIndexGate?.CompareRight == 0x101
            && unequal.LargeSavedIndexGate?.EBXAfterFreshGlobalReload == 0x100
            && unequal.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unequal.LargeSavedIndexGate?.JnzTaken == true
            && unequal.LargeSavedIndexGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C9263
            && unequal.ExactCleanExitAdmitted
            && unequal.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan aliasedFreshReads =
            Project(entryEBP: 0x100, freshFrameMinus10: 0xCAFEBABE,
                childEntryEDX: 0xD4, freshEDXPlus1C: 5,
                freshIndexedDword: 0xD4, freshGlobalValue: 0x44444444);
        check("Aliased parent and child addresses retain their separately captured fresh values",
            aliasedFreshReads.LargeSavedFrameGate.FreshFrameMinus10Address == 0xF0
            && aliasedFreshReads.LargeSavedFrameGate.FreshFrameMinus10 == 0xCAFEBABE
            && aliasedFreshReads.LargeSavedIndexGate?.FreshEDXPlus1CAddress == 0xF0
            && aliasedFreshReads.LargeSavedIndexGate?.Capture.FreshEDXPlus1C == 5
            && aliasedFreshReads.LargeSavedIndexGate?.FreshIndexedAddress == 0x01EDAF0C
            && aliasedFreshReads.LargeSavedIndexGate?.FreshIndexedDword == 0xD4
            && aliasedFreshReads.OrderedReads.Length == 4
            && aliasedFreshReads.OrderedReads[0].Address
                == aliasedFreshReads.OrderedReads[1].Address
            && aliasedFreshReads.OrderedReads[0].Value
                != aliasedFreshReads.OrderedReads[1].Value
            && aliasedFreshReads.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan wrapped =
            Project(entryEBP: 0x20, freshFrameMinus10: 1,
                childEntryEDX: 0xFFFFFFF0, freshEDXPlus1C: 0xFFFFFFFF,
                freshIndexedDword: 0xFFFFFFEF, freshGlobalValue: 0);
        check("The child EDX+1C and scaled indexed addresses use DWORD wrapping",
            wrapped.LargeSavedIndexGate?.FreshEDXPlus1CAddress == 0xC
            && wrapped.LargeSavedIndexGate?.FreshIndexedAddress == 0x01EDAEF4
            && wrapped.LargeSavedIndexGate?.Reads[0].Address == 0xC
            && wrapped.LargeSavedIndexGate?.Reads[1].Address == 0x01EDAEF4
            && wrapped.LargeSavedIndexGate?.CompareFlags?.Zero == false
            && wrapped.LargeSavedIndexGate?.JnzTaken == true
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C9263);
    }

    private static CapacityFinalCalleeLargeSavedFrameIndexedLifecyclePlan Project(
        uint? entryEBP,
        uint? freshFrameMinus10,
        uint? childEntryEDX,
        uint? freshEDXPlus1C,
        uint? freshIndexedDword,
        uint? freshGlobalValue) =>
        CapacityFinalCalleeLargeSavedFrameIndexedLifecycleProjection.Project(new(
            new(entryEBP, freshFrameMinus10),
            childEntryEDX,
            freshEDXPlus1C,
            freshIndexedDword,
            freshGlobalValue));
}
