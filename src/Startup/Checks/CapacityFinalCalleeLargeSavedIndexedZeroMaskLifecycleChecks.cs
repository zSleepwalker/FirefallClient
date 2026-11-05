using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact zero gate falls through to the bounded saved-mask entry",
            CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleProjection
                .ParentZeroDestinationRawFileOffset == 0x012C9251
            && CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleProjection
                .ParentNonzeroDestinationRawFileOffset == 0x012C927C
            && CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleProjection
                .BoundedStopRawFileOffsetExclusive == 0x012C925B
            && CapacityFinalCalleeLargeSavedMaskShiftProjection
                .BoundedStopRawFileOffsetExclusive == 0x012C925B);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan aliasedZeroCount =
            Project(
                entryEDX: 0x100,
                parentIndex: 4,
                parentEAX: 0,
                childIndex: 32,
                childEBP: 0x01EDAF10,
                childFrameValue: 0xA5);
        check("The zero edge keeps original EDX and uses independent aliased index and frame reads",
            aliasedZeroCount.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ChildCompletedTo925B
            && aliasedZeroCount.ExactCleanHandoffToChild
            && aliasedZeroCount.ChildEntryEDX == 0x100
            && aliasedZeroCount.ChildEntryEDXSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEDXSource
                    .ParentOriginalEntryEDXAtReachedZeroGate
            && aliasedZeroCount.ParentPlan.FreshIndex == 4
            && aliasedZeroCount.ParentPlan.StoreValue == 0
            && aliasedZeroCount.ParentPlan.StoreAddress == 0x01EDAF08
            && aliasedZeroCount.ChildPlan?.FreshEDXPlus1CAddress == 0x11C
            && aliasedZeroCount.ChildFreshEDXPlus1C == 32
            && aliasedZeroCount.ChildFreshIndexSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshIndexSource
                    .IndependentChildEDXPlus1CRead
            && aliasedZeroCount.ChildEntryEBP == 0x01EDAF10
            && aliasedZeroCount.ChildEntryEBPSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleChildEBPSource
                    .ExplicitIndependentCaptureAtReachedConsumer
            && aliasedZeroCount.ChildPlan?.FreshEBPMinus8Address == 0x01EDAF08
            && aliasedZeroCount.ChildFreshEBPMinus8 == 0xA5
            && aliasedZeroCount.ChildFreshFrameSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFreshFrameSource
                    .IndependentChildEBPMinus8Read
            && aliasedZeroCount.OrderedReads.Length == 3
            && aliasedZeroCount.OrderedReads[0] is
            {
                Stage: CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                    .SavedIndexedStoreGate,
                RawFileOffset: 0x012C9243,
                Value: 4
            }
            && aliasedZeroCount.OrderedReads[1] is
            {
                Stage: CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                    .SavedMaskShift,
                RawFileOffset: 0x012C9251,
                Address: 0x11C,
                Value: 32
            }
            && aliasedZeroCount.OrderedReads[2] is
            {
                Stage: CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                    .SavedMaskShift,
                RawFileOffset: 0x012C9254,
                Address: 0x01EDAF08,
                Value: 0xA5
            }
            && aliasedZeroCount.OrderedWrites is
            [{
                Stage: CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                    .SavedIndexedStoreGate,
                RawFileOffset: 0x012C9246,
                Address: 0x01EDAF08,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0,
                Committed: true
            }]
            && aliasedZeroCount.ChildPlan?.ShiftCountMasked == 0
            && aliasedZeroCount.ChildPlan?.ShiftInputEAX == 0xA5
            && aliasedZeroCount.ChildPlan?.NotResult == ~0xA5u
            && aliasedZeroCount.ParentFlagsRestoredForZeroCount
            && !aliasedZeroCount.ChildShiftFlagsReplaceParentTestFlags
            && aliasedZeroCount.ChildShiftFlags
                == new CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags(
                    null, null, null, null, null, null)
            && aliasedZeroCount.FinalFlagsAtStop
                == aliasedZeroCount.ParentTestFlags
            && aliasedZeroCount.FlagsAtStopSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource.ParentTest
            && !aliasedZeroCount.AnyUnreachedChildCaptureProvided
            && aliasedZeroCount.ReachedInstructions.SequenceEqual(
                new uint[]
                {
                    0x012C9243, 0x012C9246, 0x012C924D, 0x012C924F,
                    0x012C9251, 0x012C9254, 0x012C9257, 0x012C9259
                })
            && aliasedZeroCount.StopAtRawFileOffsetExclusive == 0x012C925B
            && aliasedZeroCount.ExactCleanExitAdmitted
            && !aliasedZeroCount.GlobalAndReached);

        (uint CountSource, byte ExpectedCount)[] countCases =
        [
            (0, 0),
            (32, 0),
            (1, 1),
            (31, 31),
            (uint.MaxValue, 31)
        ];
        foreach ((uint countSource, byte expectedCount) in countCases)
        {
            CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan plan =
                Project(
                    entryEDX: 0x100,
                    parentIndex: 5,
                    parentEAX: 0,
                    childIndex: countSource,
                    childEBP: 0x400,
                    childFrameValue: 0x80000000);
            uint shifted = 0x80000000u << expectedCount;
            CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? expectedStopFlags =
                expectedCount == 0
                    ? plan.ParentTestFlags
                    : ConvertFlags(plan.ChildPlan!.ShiftFlags);

            check($"Fresh child count {countSource:X8} masks to {expectedCount} with correct flag source",
                plan.Outcome
                    == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                        .ChildCompletedTo925B
                && plan.ParentPlan.FreshIndex == 5
                && plan.ChildFreshEDXPlus1C == countSource
                && plan.ChildPlan?.ShiftCountSource == countSource
                && plan.ChildPlan?.ShiftCountLowByte == (byte)countSource
                && plan.ChildPlan?.ShiftCountMasked == expectedCount
                && plan.ChildPlan?.ShiftResult == shifted
                && plan.ChildPlan?.NotResult == ~shifted
                && plan.FinalFlagsAtStop == expectedStopFlags
                && plan.ParentFlagsRestoredForZeroCount == (expectedCount == 0)
                && plan.ChildShiftFlagsReplaceParentTestFlags == (expectedCount != 0)
                && plan.FlagsAtStopSource
                    == (expectedCount == 0
                        ? CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
                            .ParentTest
                        : CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource
                            .ChildShift)
                && plan.StopAtRawFileOffsetExclusive == 0x012C925B
                && plan.ExactCleanExitAdmitted);
        }

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan nonzeroCount =
            Project(
                entryEDX: 0x100,
                parentIndex: 5,
                parentEAX: 0,
                childIndex: 1,
                childEBP: 8,
                childFrameValue: 0x80000000);
        check("A nonzero child SHL replaces TEST flags and NOT preserves those flags",
            nonzeroCount.ChildPlan?.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: true
            }
            && nonzeroCount.ChildPlan?.NotResult == uint.MaxValue
            && nonzeroCount.ChildShiftFlagsReplaceParentTestFlags
            && !nonzeroCount.ParentFlagsRestoredForZeroCount
            && nonzeroCount.FinalFlagsAtStop == ConvertFlags(nonzeroCount.ChildPlan!.ShiftFlags)
            && nonzeroCount.FlagsAtStopSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource.ChildShift);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan missingParentEDX =
            Project(
                entryEDX: null,
                parentIndex: 4,
                parentEAX: 0,
                childIndex: 9,
                childEBP: 0x200,
                childFrameValue: 0x55);
        check("An incomplete parent gate never reaches the child and reports all later captures unused",
            missingParentEDX.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ParentGateIncomplete
            && !missingParentEDX.ChildReached
            && missingParentEDX.UnreachedChildEntryEBPCaptureProvided
            && missingParentEDX.UnreachedChildFreshEDXPlus1CCaptureProvided
            && missingParentEDX.UnreachedChildFreshEBPMinus8CaptureProvided
            && missingParentEDX.OrderedReads.Length == 1
            && missingParentEDX.OrderedReads[0].Stage
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleStage
                    .SavedIndexedStoreGate
            && missingParentEDX.OrderedWrites.IsEmpty
            && !missingParentEDX.ExactCleanExitAdmitted
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan missingParentEAX =
            Project(
                entryEDX: 0x100,
                parentIndex: 4,
                parentEAX: null,
                childIndex: 9,
                childEBP: 0x200,
                childFrameValue: 0x55);
        check("A reached parent store with missing EAX is retained and blocks the child",
            missingParentEAX.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ParentGateIncomplete
            && !missingParentEAX.ChildReached
            && missingParentEAX.ParentPlan.StoreInstructionReached
            && missingParentEAX.OrderedWrites is
            [{
                RawFileOffset: 0x012C9246,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null,
                Committed: false
            }]
            && !missingParentEAX.ParentPlan.TestReached
            && missingParentEAX.UnreachedChildFreshEDXPlus1CCaptureProvided
            && missingParentEAX.StopAtRawFileOffsetExclusive == 0x012C9246);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan parentNonzero =
            Project(
                entryEDX: 0x100,
                parentIndex: 4,
                parentEAX: 1,
                childIndex: 9,
                childEBP: 0x200,
                childFrameValue: 0x55);
        check("Parent nonzero branches to 927C without entering the child or consuming its captures",
            parentNonzero.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ParentNonzeroTo927CWithUnusedChildCaptures
            && parentNonzero.ParentPlan.JnzTaken == true
            && parentNonzero.ParentPlan.JnzSelectedDestinationRawFileOffset == 0x012C927C
            && !parentNonzero.ChildReached
            && parentNonzero.UnreachedChildEntryEBPCaptureProvided
            && parentNonzero.UnreachedChildFreshEDXPlus1CCaptureProvided
            && parentNonzero.UnreachedChildFreshEBPMinus8CaptureProvided
            && parentNonzero.OrderedReads.Length == 1
            && parentNonzero.OrderedWrites.Length == 1
            && parentNonzero.FinalFlagsAtStop == parentNonzero.ParentTestFlags
            && parentNonzero.FlagsAtStopSource
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlagsSource.ParentTest
            && parentNonzero.StopAtRawFileOffsetExclusive == 0x012C927C
            && !parentNonzero.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan cleanParentNonzero =
            Project(
                entryEDX: 0x100,
                parentIndex: 4,
                parentEAX: 1,
                childIndex: null,
                childEBP: null,
                childFrameValue: null);
        check("An uncontaminated parent nonzero branch is a clean bounded exit",
            cleanParentNonzero.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ParentNonzeroTo927C
            && !cleanParentNonzero.ChildReached
            && cleanParentNonzero.ExactCleanExitAdmitted
            && cleanParentNonzero.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan missingChildIndex =
            Project(
                entryEDX: 0x100,
                parentIndex: 5,
                parentEAX: 0,
                childIndex: null,
                childEBP: 0x300,
                childFrameValue: 0x66);
        check("A missing fresh child count retains the 9251 read and withholds EBP prerequisites",
            missingChildIndex.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ChildMissingFreshEDXPlus1C
            && missingChildIndex.ChildEntryEDX == 0x100
            && missingChildIndex.ChildPlan?.FreshEDXPlus1CReadReached == true
            && missingChildIndex.ChildPlan?.FreshEDXPlus1CAddress == 0x11C
            && missingChildIndex.ChildPlan?.EntryEBPConsumerReached == false
            && missingChildIndex.UnreachedChildEntryEBPCaptureProvided
            && missingChildIndex.UnreachedChildFreshEBPMinus8CaptureProvided
            && missingChildIndex.OrderedReads.Length == 2
            && missingChildIndex.OrderedReads[1] is
            {
                RawFileOffset: 0x012C9251,
                Address: 0x11C,
                ValueCaptured: false
            }
            && missingChildIndex.FinalFlagsAtStop == missingChildIndex.ParentTestFlags
            && missingChildIndex.StopAtRawFileOffsetExclusive == 0x012C9251
            && !missingChildIndex.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan missingChildEBP =
            Project(
                entryEDX: 0x100,
                parentIndex: 5,
                parentEAX: 0,
                childIndex: 1,
                childEBP: null,
                childFrameValue: 0x66);
        check("Missing EBP is discovered only after the child count read and leaves the frame capture unused",
            missingChildEBP.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ChildMissingEntryEBP
            && missingChildEBP.ChildPlan?.FreshEDXPlus1CCaptured == true
            && missingChildEBP.ChildPlan?.EntryEBPConsumerReached == true
            && missingChildEBP.ChildPlan?.FreshEBPMinus8ReadReached == false
            && missingChildEBP.ChildFreshEDXPlus1C == 1
            && missingChildEBP.UnreachedChildFreshEBPMinus8CaptureProvided
            && missingChildEBP.OrderedReads.Length == 2
            && missingChildEBP.OrderedReads[1].RawFileOffset == 0x012C9251
            && missingChildEBP.ReachedInstructions.SequenceEqual(
                new uint[]
                {
                    0x012C9243, 0x012C9246, 0x012C924D, 0x012C924F,
                    0x012C9251, 0x012C9254
                })
            && missingChildEBP.FinalFlagsAtStop == missingChildEBP.ParentTestFlags
            && missingChildEBP.StopAtRawFileOffsetExclusive == 0x012C9254
            && !missingChildEBP.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan missingChildFrame =
            Project(
                entryEDX: 0x100,
                parentIndex: 5,
                parentEAX: 0,
                childIndex: 31,
                childEBP: 4,
                childFrameValue: null);
        check("A missing fresh frame value retains both reads and the loaded count without shifting",
            missingChildFrame.Outcome
                == CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleOutcome
                    .ChildMissingFreshEBPMinus8
            && missingChildFrame.ChildPlan?.ShiftCountSource == 31
            && missingChildFrame.ChildPlan?.ShiftCountMasked == 31
            && missingChildFrame.ChildPlan?.FreshEBPMinus8ReadReached == true
            && missingChildFrame.ChildPlan?.FreshEBPMinus8Address == 0xFFFFFFFC
            && missingChildFrame.ChildPlan?.ShiftResult is null
            && missingChildFrame.OrderedReads.Length == 3
            && missingChildFrame.OrderedReads[2] is
            {
                RawFileOffset: 0x012C9254,
                Address: 0xFFFFFFFC,
                ValueCaptured: false
            }
            && missingChildFrame.FinalFlagsAtStop == missingChildFrame.ParentTestFlags
            && missingChildFrame.StopAtRawFileOffsetExclusive == 0x012C9254
            && !missingChildFrame.ExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecyclePlan Project(
        uint? entryEDX,
        uint? parentIndex,
        uint? parentEAX,
        uint? childIndex,
        uint? childEBP,
        uint? childFrameValue) =>
        CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleProjection.Project(new(
            new(
                EntryEDX: entryEDX,
                FreshEDXPlus1C: parentIndex,
                EntryEAX: parentEAX),
            ChildEntryEBP: childEBP,
            ChildFreshEDXPlus1C: childIndex,
            ChildFreshEBPMinus8: childFrameValue));

    private static CapacityFinalCalleeLargeSavedIndexedZeroMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeLargeSavedMaskShiftFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedMaskShiftFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
