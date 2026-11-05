using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeClearIndexedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The clean field-clear boundary joins the indexed window at exact FFE",
            CapacityFinalCalleeClearIndexedLifecycleProjection.ClearTailEntryRawFileOffset
                == 0x012C8FDD
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedRootMaskEntryRawFileOffset == 0x012C8FFE
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedRootMaskBoundaryRawFileOffsetExclusive == 0x012C903B
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .ClearTailBelowDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedZeroExitRawFileOffset == 0x012C8EF8
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedUnequalExitRawFileOffset == 0x012C903B
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedNonzeroExitRawFileOffset == 0x012C9058
            && CapacityFinalCalleeClearIndexedLifecycleProjection
                .IndexedMaskContinuationRawFileOffset == 0x012C8EF2);

        CapacityFinalCalleeClearIndexedLifecyclePlan continuation = Project();
        check("The normal route carries EBP, retains the clear writes, reloads each indexed value independently, and reaches EF2",
            continuation.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContinuation
            && continuation.ClearTailPlan.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
            && continuation.ClearTailPlan.StopAtRawFileOffsetExclusive == 0x012C8FFE
            && continuation.ClearTailPlan.Capture.ValidatedEntryEBP == 0x2000
            && continuation.ClearTailPlan.TailGlobalValue == 0x12345678
            && continuation.ClearTailPlan.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x30),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0x3000, 0)
            })
            && continuation.IndexedRootMask is
                CapacityFinalCalleeIndexedRootMaskPlan root
            && root.Capture.EntryEBP == 0x2000
            && root.Capture.EntryESI == 0xFFFFFFF0
            && root.Capture.EntryEAX == 0
            && root.FreshFrameMinus14Address == 0x1FEC
            && root.FreshFrameMinus14 == 1
            && root.FirstIndexAddress == 0x0000000C
            && root.FirstIndexedTableAddress
                == CapacityFinalCalleeIndexedRootMaskProjection
                    .DeriveIndexedTableAddress(0xFFFFFFFF)
            && root.FirstIndexedTableValue == 0xFFFFFFF0
            && root.FreshFrameMinusCAddress == 0x1FF4
            && root.SecondIndexAddress == 0x0000000C
            && root.FreshSecondIndex == 5
            && root.TableWriteAddress
                == CapacityFinalCalleeIndexedRootMaskProjection
                    .DeriveIndexedTableAddress(5)
            && root.TableWriteValue == 0
            && root.FreshFrameMinus8Address == 0x1FF8
            && root.FreshThirdIndex == 32
            && root.ShiftCount == 0
            && root.GlobalAddress == 0x01EDADCC
            && root.GlobalValueBefore == 0xFFFFFFFF
            && root.GlobalValueAfter == 0x7FFFFFFE
            && continuation.IndexedRootMaskReached
            && continuation.Admitted
            && !continuation.UnreachedIndexedRootMaskCaptureProvided
            && continuation.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleeClearIndexedLifecyclePlan zeroExit = Project(
            indexedEntryESI: null,
            indexedEntryEAX: null,
            freshFrameMinus14: 0,
            freshFirstIndex: null,
            firstIndexedTableValue: null,
            freshFrameMinusC: null,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("A captured zero is an exact EF8 exit and leaves optional caller live-ins and later reads unconsumed",
            zeroExit.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskZeroExit
            && zeroExit.IndexedRootMask?.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.FreshFrameMinus14ZeroExit
            && zeroExit.IndexedRootMask?.ZeroBranchDestinationRawFileOffset == 0x012C8EF8
            && zeroExit.IndexedRootMask?.FreshFrameMinus14Captured == true
            && zeroExit.IndexedRootMask?.FreshFrameMinus14 == 0
            && zeroExit.IndexedRootMask?.FirstIndexReadReached == false
            && zeroExit.IndexedRootMask?.GlobalWriteReached == false
            && zeroExit.Admitted
            && zeroExit.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeClearIndexedLifecyclePlan zeroWithUnusedEsi = Project(
            indexedEntryESI: 0x2000,
            indexedEntryEAX: null,
            freshFrameMinus14: 0,
            freshFirstIndex: null,
            firstIndexedTableValue: null,
            freshFrameMinusC: null,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("A supplied ESI before its first indexed consumer contaminates but preserves the EF8 route",
            zeroWithUnusedEsi.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContaminated
            && zeroWithUnusedEsi.IndexedRootMask?.ZeroBranchTaken == true
            && zeroWithUnusedEsi.UnreachedIndexedRootEntryESICaptureProvided
            && zeroWithUnusedEsi.UnreachedIndexedRootMaskCaptureProvided
            && !zeroWithUnusedEsi.Admitted
            && zeroWithUnusedEsi.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeClearIndexedLifecyclePlan missingFrame14 = Project(
            freshFrameMinus14: null,
            freshFirstIndex: 1,
            firstIndexedTableValue: 1);
        check("A missing fresh EBP-minus-14 read remains distinct from zero and rejects supplied later reads",
            missingFrame14.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContaminated
            && missingFrame14.IndexedRootMask?.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus14
            && missingFrame14.IndexedRootMask?.FreshFrameMinus14Address == 0x1FEC
            && missingFrame14.IndexedRootMask?.FreshFrameMinus14ReadReached == true
            && missingFrame14.IndexedRootMask?.FreshFrameMinus14Captured == false
            && missingFrame14.IndexedRootMask?.StopAtRawFileOffsetExclusive
                == 0x012C8FFE
            && missingFrame14.UnreachedIndexedRootMaskCaptureProvided
            && !missingFrame14.Admitted);

        CapacityFinalCalleeClearIndexedLifecyclePlan missingEsi = Project(
            indexedEntryESI: null,
            indexedEntryEAX: null,
            freshFrameMinus14: 1,
            freshFirstIndex: null,
            firstIndexedTableValue: null,
            freshFrameMinusC: null,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("Missing ESI is demanded only after a nonzero fresh frame read and stops before its index load",
            missingEsi.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskIncomplete
            && missingEsi.IndexedRootMask?.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryESI
            && missingEsi.IndexedRootMask?.ZeroBranchTaken == false
            && missingEsi.IndexedRootMask?.FirstIndexReadReached == false
            && missingEsi.IndexedRootMask?.StopAtRawFileOffsetExclusive == 0x012C9009
            && !missingEsi.UnreachedIndexedRootMaskCaptureProvided
            && !missingEsi.Admitted);

        CapacityFinalCalleeClearIndexedLifecyclePlan unequalExit = Project(
            indexedEntryESI: 10,
            indexedEntryEAX: null,
            freshFrameMinus14: 1,
            freshFirstIndex: 0xFFFFFFFF,
            firstIndexedTableValue: 11,
            freshFrameMinusC: 0xDEADBEEF,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("The fresh EBP-minus-C reload preserves CMP flags and takes the exact 903B unequal exit",
            unequalExit.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskUnequalExit
            && unequalExit.IndexedRootMask?.FirstCompareResult == 0xFFFFFFFF
            && unequalExit.IndexedRootMask?.FirstCompareFlags is
                { Carry: true, Zero: false }
            && unequalExit.IndexedRootMask?.FreshFrameMinusC == 0xDEADBEEF
            && unequalExit.IndexedRootMask?.UnequalBranchTaken == true
            && unequalExit.IndexedRootMask?.UnequalBranchDestinationRawFileOffset
                == 0x012C903B
            && unequalExit.IndexedRootMask?.SecondIndexReadReached == false
            && unequalExit.Admitted
            && unequalExit.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeClearIndexedLifecyclePlan unequalWithUnusedEax = Project(
            indexedEntryESI: 10,
            indexedEntryEAX: 0xDEADBEEF,
            freshFrameMinus14: 1,
            freshFirstIndex: 1,
            firstIndexedTableValue: 11,
            freshFrameMinusC: 0,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("EAX remains an independent live-in and an unused value contaminates the unequal route",
            unequalWithUnusedEax.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContaminated
            && unequalWithUnusedEax.IndexedRootMask?.UnequalBranchTaken == true
            && unequalWithUnusedEax.UnreachedIndexedRootEntryEAXCaptureProvided
            && !unequalWithUnusedEax.Admitted
            && unequalWithUnusedEax.StopAtRawFileOffsetExclusive == 0x012C903B);

        CapacityFinalCalleeClearIndexedLifecyclePlan missingEax = Project(
            indexedEntryESI: 10,
            indexedEntryEAX: null,
            freshFrameMinus14: 1,
            freshFirstIndex: 1,
            firstIndexedTableValue: 10,
            freshFrameMinusC: 0,
            freshSecondIndex: 2,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("EAX is demanded at the table store only after equality and the fresh second index",
            missingEax.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskIncomplete
            && missingEax.IndexedRootMask?.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryEAX
            && missingEax.IndexedRootMask?.FreshSecondIndex == 2
            && missingEax.IndexedRootMask?.TableWriteReached == false
            && missingEax.IndexedRootMask?.StopAtRawFileOffsetExclusive == 0x012C901B
            && !missingEax.UnreachedIndexedRootEntryEAXCaptureProvided
            && !missingEax.Admitted);

        CapacityFinalCalleeClearIndexedLifecyclePlan nonzeroExit = Project(
            indexedEntryESI: 0x2000,
            indexedEntryEAX: 0xAABBCCDD,
            freshFrameMinus14: 1,
            freshFirstIndex: 7,
            firstIndexedTableValue: 0x2000,
            freshFrameMinusC: 7,
            freshSecondIndex: 9,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("A nonzero caller EAX is written at its separately reloaded index before the exact 9058 exit",
            nonzeroExit.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskNonzeroTableWriteExit
            && nonzeroExit.IndexedRootMask?.TableWriteAddress
                == CapacityFinalCalleeIndexedRootMaskProjection
                    .DeriveIndexedTableAddress(9)
            && nonzeroExit.IndexedRootMask?.TableWriteValue == 0xAABBCCDD
            && nonzeroExit.IndexedRootMask?.NonzeroBranchTaken == true
            && nonzeroExit.IndexedRootMask?.NonzeroBranchDestinationRawFileOffset
                == 0x012C9058
            && nonzeroExit.IndexedRootMask?.FreshFrameMinus8ReadReached == false
            && nonzeroExit.Admitted
            && nonzeroExit.StopAtRawFileOffsetExclusive == 0x012C9058);

        CapacityFinalCalleeClearIndexedLifecyclePlan missingIndexedRead = Project(
            indexedEntryESI: 0x2000,
            indexedEntryEAX: 0,
            freshFrameMinus14: 1,
            freshFirstIndex: 1,
            firstIndexedTableValue: 0x2000,
            freshFrameMinusC: 0,
            freshSecondIndex: 2,
            freshFrameMinus8: null,
            freshThirdIndex: 4,
            freshIndexedGlobalValue: 0xFFFF);
        check("A missing fresh EBP-minus-8 read preserves the prior zero table write and rejects unreached later captures",
            missingIndexedRead.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContaminated
            && missingIndexedRead.IndexedRootMask?.Outcome
                == CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus8
            && missingIndexedRead.IndexedRootMask?.TableWriteReached == true
            && missingIndexedRead.IndexedRootMask?.TableWriteValue == 0
            && missingIndexedRead.IndexedRootMask?.FreshFrameMinus8Address == 0x1FF8
            && missingIndexedRead.IndexedRootMask?.FreshFrameMinus8Captured == false
            && missingIndexedRead.IndexedRootMask?.ThirdIndexReadReached == false
            && missingIndexedRead.UnreachedIndexedRootMaskCaptureProvided
            && !missingIndexedRead.Admitted);

        CapacityFinalCalleeClearIndexedLifecyclePlan incompleteTail = Project(
            clearTailCapture: TailCapture(freshGlobalForTailLoad: null),
            indexedEntryESI: 0x2000,
            freshFrameMinus14: 1);
        check("A missing tail-global capture preserves the two earlier stores and rejects every indexed capture",
            incompleteTail.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .ClearTailContaminated
            && incompleteTail.ClearTailPlan.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.MissingFreshGlobalForTailLoad
            && incompleteTail.ClearTailPlan.Writes.Length == 2
            && incompleteTail.ClearTailPlan.TailGlobalReadReached
            && !incompleteTail.ClearTailPlan.TailGlobalCaptured
            && incompleteTail.IndexedRootMask is null
            && !incompleteTail.IndexedRootMaskReached
            && incompleteTail.UnreachedIndexedRootEntryESICaptureProvided
            && incompleteTail.UnreachedIndexedRootMaskCaptureProvided
            && !incompleteTail.Admitted
            && incompleteTail.StopAtRawFileOffsetExclusive == 0x012C8FF8);

        CapacityFinalCalleeClearIndexedLifecyclePlan missingTailCompare =
            Project(clearTailCapture: TailCapture(
                validatedEntryEBP: null,
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null),
                indexedEntryESI: null,
                indexedEntryEAX: null,
                freshFrameMinus14: null,
                freshFirstIndex: null,
                firstIndexedTableValue: null,
                freshFrameMinusC: null,
                freshSecondIndex: null,
                freshFrameMinus8: null,
                freshThirdIndex: null,
                freshIndexedGlobalValue: null);
        check("A missing first clear-tail read is incomplete rather than a zero-valued branch",
            missingTailCompare.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome.ClearTailIncomplete
            && missingTailCompare.ClearTailPlan.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome
                    .MissingFreshGlobalForEntryCompare
            && missingTailCompare.ClearTailPlan.Reads.Length == 1
            && missingTailCompare.ClearTailPlan.Writes.IsEmpty
            && missingTailCompare.IndexedRootMask is null
            && missingTailCompare.StopAtRawFileOffsetExclusive == 0x012C8FDD);

        CapacityFinalCalleeClearIndexedLifecyclePlan clearBelow = Project(
            clearTailCapture: TailCapture(
                validatedEntryEDX: 0x7FFFFFFF,
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: 0xFFFFFFFF,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null),
            indexedEntryESI: null,
            indexedEntryEAX: null,
            freshFrameMinus14: null,
            freshFirstIndex: null,
            firstIndexedTableValue: null,
            freshFrameMinusC: null,
            freshSecondIndex: null,
            freshFrameMinus8: null,
            freshThirdIndex: null,
            freshIndexedGlobalValue: null);
        check("The clean E76 branch remains a bounded clear-tail destination and never reaches indexed code",
            clearBelow.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .ClearTailBelowDestination
            && clearBelow.ClearTailPlan.BranchTaken == true
            && clearBelow.ClearTailPlan.SelectedBranchDestinationRawFileOffset
                == 0x012C8E76
            && clearBelow.ClearTailPlan.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x7FFFFFFF)
            })
            && clearBelow.IndexedRootMask is null
            && clearBelow.Admitted
            && clearBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeClearIndexedLifecyclePlan contaminatedBoundary =
            Project(clearTailCapture: TailCapture(
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: 0x1234));
        check("An unreached tail-global capture prevents an FFE join without discarding earlier clear effects",
            contaminatedBoundary.Outcome
                == CapacityFinalCalleeClearIndexedLifecycleOutcome.ClearTailContaminated
            && contaminatedBoundary.ClearTailPlan.BranchTaken == false
            && contaminatedBoundary.ClearTailPlan.Writes.Length == 1
            && contaminatedBoundary.ClearTailPlan
                .UnreachedFreshGlobalForTailLoadCaptureProvided
            && contaminatedBoundary.IndexedRootMask is null
            && !contaminatedBoundary.Admitted
            && contaminatedBoundary.StopAtRawFileOffsetExclusive == 0x012C8FEF);
    }

    private static CapacityFinalCalleeClearIndexedLifecyclePlan Project(
        CapacityFinalCalleeFieldClearTailCapture? clearTailCapture = null,
        uint? indexedEntryESI = 0xFFFFFFF0,
        uint? indexedEntryEAX = 0,
        uint? freshFrameMinus14 = 1,
        uint? freshFirstIndex = 0xFFFFFFFF,
        uint? firstIndexedTableValue = 0xFFFFFFF0,
        uint? freshFrameMinusC = 0,
        uint? freshSecondIndex = 5,
        uint? freshFrameMinus8 = 0x80000001,
        uint? freshThirdIndex = 32,
        uint? freshIndexedGlobalValue = 0xFFFFFFFF) =>
        CapacityFinalCalleeClearIndexedLifecycleProjection.Project(new(
            clearTailCapture ?? TailCapture(),
            indexedEntryESI,
            indexedEntryEAX,
            freshFrameMinus14,
            freshFirstIndex,
            firstIndexedTableValue,
            freshFrameMinusC,
            freshSecondIndex,
            freshFrameMinus8,
            freshThirdIndex,
            freshIndexedGlobalValue));

    private static CapacityFinalCalleeFieldClearTailCapture TailCapture(
        uint? validatedEntryEDX = 0x30,
        uint? validatedEntryEBP = 0x2000,
        uint? freshGlobalForEntryCompare = 0,
        uint? freshFrameMinus12 = 0x11111111,
        uint? freshFrameMinus4ForZeroStoreAddress = 0x3000,
        uint? freshGlobalForTailLoad = 0x12345678) =>
        new(
            validatedEntryEDX,
            validatedEntryEBP,
            freshGlobalForEntryCompare,
            freshFrameMinus12,
            freshFrameMinus4ForZeroStoreAddress,
            freshGlobalForTailLoad);
}
