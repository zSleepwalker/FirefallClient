using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSizeAlternateLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan equalityJoin =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 0));
        check("Only the clean ESI+ C equality joins FA8 with preserved entry ESI and EBP",
            equalityJoin.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10ZeroTarget
            && equalityJoin.LargeSizeBoundaryReached
            && equalityJoin.LargeSizeFieldsPlan.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI
            && equalityJoin.LargeSizeFieldsPlan.StopAtRawFileOffsetExclusive
                == 0x012C8FA8
            && equalityJoin.AlternateFieldSelectionReached
            && equalityJoin.AlternateFieldSelectionPlan is
                CapacityFinalCalleeAlternateFieldSelectionPlan alternate
            && alternate.Capture.EntryESI == 0x1000
            && alternate.Capture.EntryEBP == 0x2000
            && equalityJoin.Admitted
            && equalityJoin.SelectedDestinationRawFileOffset == 0x012C8FFE
            && equalityJoin.StopAtRawFileOffsetExclusive == 0x012C8FFE
            && !equalityJoin.UnreachedAlternateFreshEsiPlus14CaptureProvided
            && !equalityJoin.UnreachedAlternateFreshEsiPlus10CaptureProvided);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan alternateFirstNonzero =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(),
                    FreshAlternateEsiPlus14: 1,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan alternateSecondNonzero =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 1));
        check("Alternate nonzero branches are admitted only at their exact FC2 boundary",
            alternateFirstNonzero.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateFirstFreshEsiPlus14NonzeroBoundary
            && alternateFirstNonzero.Admitted
            && alternateFirstNonzero.SelectedDestinationRawFileOffset == 0x012C8FC2
            && alternateFirstNonzero.StopAtRawFileOffsetExclusive == 0x012C8FC2
            && alternateFirstNonzero.AlternateFieldSelectionPlan?.FreshEsiPlus10ReadReached
                == false
            && alternateSecondNonzero.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10NonzeroBoundary
            && alternateSecondNonzero.Admitted
            && alternateSecondNonzero.SelectedDestinationRawFileOffset == 0x012C8FC2
            && alternateSecondNonzero.StopAtRawFileOffsetExclusive == 0x012C8FC2);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan largeSizeBelow =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(freshEsiPlus8: 0xF,
                        freshEcxPlusC: null, freshEaxPlus8: null),
                    FreshAlternateEsiPlus14: null,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan ecxMismatch =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(freshEcxPlusC: 0x1001, freshEaxPlus8: null),
                    FreshAlternateEsiPlus14: null,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan eaxMismatch =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(freshEaxPlus8: 0x1001),
                    FreshAlternateEsiPlus14: null,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan largeSizePairedWrites =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(),
                    FreshAlternateEsiPlus14: null,
                    FreshAlternateEsiPlus10: null));
        check("Clean E76 exits and the completed FF8 paired-write route stay bounded successes",
            largeSizeBelow.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEsiPlus8UnsignedBelowExit
            && largeSizeBelow.Admitted
            && largeSizeBelow.SelectedDestinationRawFileOffset == 0x012C8E76
            && largeSizeBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && largeSizeBelow.AlternateFieldSelectionPlan is null
            && largeSizeBelow.LargeSizeFieldsPlan.FrameWriteReached
            && ecxMismatch.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEcxPlusCMismatchExit
            && ecxMismatch.Admitted
            && ecxMismatch.SelectedDestinationRawFileOffset == 0x012C8E76
            && eaxMismatch.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEaxPlus8MismatchExit
            && eaxMismatch.Admitted
            && eaxMismatch.SelectedDestinationRawFileOffset == 0x012C8E76
            && largeSizePairedWrites.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizePairedWritesExit
            && largeSizePairedWrites.Admitted
            && largeSizePairedWrites.SelectedDestinationRawFileOffset == 0x012C8FF8
            && largeSizePairedWrites.StopAtRawFileOffsetExclusive == 0x012C8FF8
            && largeSizePairedWrites.LargeSizeFieldsPlan.FirstPairedWriteReached
            && largeSizePairedWrites.LargeSizeFieldsPlan.SecondPairedWriteReached);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan contaminatedLargeEquality =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(validatedEntryEBX: 0x10),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 0));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan unselectedLargeSizeCapture =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan unselectedIncompleteCapture =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(validatedEntryESI: null),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 0));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan contaminatedAlternate =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(),
                    FreshAlternateEsiPlus14: 1,
                    FreshAlternateEsiPlus10: 0));
        check("Unselected and nested supplied captures prevent admission without losing the source boundary",
            contaminatedLargeEquality.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .ContaminatedLargeSizeFields
            && contaminatedLargeEquality.LargeSizeBoundaryReached
            && !contaminatedLargeEquality.AlternateFieldSelectionReached
            && !contaminatedLargeEquality.Admitted
            && contaminatedLargeEquality.LargeSizeFieldsPlan
                .UnreachedValidatedEntryEBXCaptureProvided
            && unselectedLargeSizeCapture.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .ContaminatedUnselectedAlternateCaptures
            && !unselectedLargeSizeCapture.Admitted
            && unselectedLargeSizeCapture.UnreachedAlternateFreshEsiPlus14CaptureProvided
            && unselectedIncompleteCapture.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .ContaminatedLargeSizeFields
            && !unselectedIncompleteCapture.LargeSizeBoundaryReached
            && unselectedIncompleteCapture.UnreachedAlternateFreshEsiPlus14CaptureProvided
            && unselectedIncompleteCapture.UnreachedAlternateFreshEsiPlus10CaptureProvided
            && contaminatedAlternate.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .ContaminatedAlternateFieldSelection
            && !contaminatedAlternate.Admitted
            && contaminatedAlternate.AlternateFieldSelectionPlan?
                .UnreachedFreshEsiPlus10CaptureProvided == true);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan missingLargeRead =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(
                        validatedEntryEBX: null,
                        freshEsiPlusC: null,
                        freshEsiPlus8: null,
                        freshEcxPlusC: null,
                        freshEaxPlus8: null),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: null));
        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan missingAlternateSecondRead =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: null));
        check("Missing reads remain distinct and preserve earlier frame and selection writes",
            missingLargeRead.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeFieldsIncomplete
            && !missingLargeRead.Admitted
            && missingLargeRead.LargeSizeFieldsPlan.FrameWriteReached
            && missingLargeRead.LargeSizeFieldsPlan.FrameWriteAddress == 0x1FEC
            && missingLargeRead.LargeSizeFieldsPlan.FrameWriteValue == 0x3000
            && !missingLargeRead.AlternateFieldSelectionReached
            && missingAlternateSecondRead.Outcome
                == CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateFieldSelectionIncomplete
            && !missingAlternateSecondRead.Admitted
            && missingAlternateSecondRead.LargeSizeFieldsPlan.FrameWriteReached
            && missingAlternateSecondRead.LargeSizeFieldsPlan.FrameWriteAddress == 0x1FEC
            && missingAlternateSecondRead.LargeSizeFieldsPlan.FrameWriteValue == 0x3000
            && missingAlternateSecondRead.AlternateFieldSelectionPlan is
                CapacityFinalCalleeAlternateFieldSelectionPlan missingSecond
            && missingSecond.Outcome
                == CapacityFinalCalleeAlternateFieldSelectionOutcome.MissingFreshEsiPlus10
            && missingSecond.FirstWriteReached
            && missingSecond.FirstWriteAddress == 0x1FFC
            && missingSecond.FirstWriteValue == 0x1014
            && missingAlternateSecondRead.StopAtRawFileOffsetExclusive == 0x012C8FB5);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan aliasedIndependentReads =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeEqualityCapture(validatedEntryEBP: 0x1028),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 0));
        check("The alternate ESI+14 read stays independent when it aliases the prior frame write",
            aliasedIndependentReads.Admitted
            && aliasedIndependentReads.LargeSizeFieldsPlan.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI
            && aliasedIndependentReads.LargeSizeFieldsPlan.FrameWriteReached
            && aliasedIndependentReads.LargeSizeFieldsPlan.FrameWriteAddress == 0x1014
            && aliasedIndependentReads.LargeSizeFieldsPlan.FrameWriteValue == 0x3000
            && !aliasedIndependentReads.LargeSizeFieldsPlan.FirstPairedWriteReached
            && !aliasedIndependentReads.LargeSizeFieldsPlan.SecondPairedWriteReached
            && aliasedIndependentReads.AlternateFieldSelectionPlan is
                CapacityFinalCalleeAlternateFieldSelectionPlan aliasedAlternate
            && aliasedAlternate.FreshEsiPlus14Address == 0x1014
            && aliasedAlternate.FirstEAX == 0
            && aliasedAlternate.FreshEsiPlus10Address == 0x1010
            && aliasedAlternate.SecondEAX == 0
            && aliasedAlternate.FirstWriteAddress == 0x1024
            && aliasedAlternate.FirstWriteValue == 0x1014
            && aliasedAlternate.SecondWriteAddress == 0x1024
            && aliasedAlternate.SecondWriteValue == 0x1010
            && aliasedIndependentReads.SelectedDestinationRawFileOffset == 0x012C8FFE);

        CapacityFinalCalleeLargeSizeAlternateLifecyclePlan wrapped =
            CapacityFinalCalleeLargeSizeAlternateLifecycleProjection.Project(
                new(
                    LargeSizeCapture(
                        validatedEntryESI: 0xFFFFFFF0,
                        validatedEntryEBP: 0x10,
                        validatedEntryEBX: null,
                        freshEsiPlus18: 0,
                        freshEsiPlusC: 0xFFFFFFF0,
                        freshEsiPlus8: null,
                        freshEcxPlusC: null,
                        freshEaxPlus8: null),
                    FreshAlternateEsiPlus14: 0,
                    FreshAlternateEsiPlus10: 0));
        check("Large-size and alternate address arithmetic wraps as unsigned DWORDs",
            wrapped.Admitted
            && wrapped.LargeSizeFieldsPlan.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI
            && wrapped.LargeSizeFieldsPlan.FrameWriteAddress == 0xFFFFFFFC
            && wrapped.LargeSizeFieldsPlan.FrameWriteValue == 0
            && wrapped.LargeSizeFieldsPlan.FreshEsiPlusCReadReached
            && wrapped.LargeSizeFieldsPlan.FreshEsiPlusCCaptured
            && wrapped.LargeSizeFieldsPlan.FreshEsiPlusCAddress == 0xFFFFFFFC
            && wrapped.LargeSizeFieldsPlan.EaxLoadedFromEsiPlusC == 0xFFFFFFF0
            && wrapped.LargeSizeFieldsPlan.EsiPlusCEqualsEntryESI == true
            && !wrapped.LargeSizeFieldsPlan.FirstPairedWriteReached
            && !wrapped.LargeSizeFieldsPlan.SecondPairedWriteReached
            && wrapped.AlternateFieldSelectionPlan is
                CapacityFinalCalleeAlternateFieldSelectionPlan wrappedAlternate
            && wrappedAlternate.FreshEsiPlus14Address == 4
            && wrappedAlternate.FreshEsiPlus10Address == 0
            && wrappedAlternate.FirstWriteAddress == 0xC
            && wrappedAlternate.SecondWriteAddress == 0xC
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8FFE);
    }

    private static CapacityFinalCalleeLargeSizeFieldsCapture LargeSizeEqualityCapture(
        uint? validatedEntryEBP = 0x2000,
        uint? validatedEntryEBX = null) =>
        new(
            ValidatedEntryESI: 0x1000,
            ValidatedEntryEBP: validatedEntryEBP,
            ValidatedEntryEBX: validatedEntryEBX,
            FreshEsiPlus18: 0x3000,
            FreshEsiPlusC: 0x1000,
            FreshEsiPlus8: null,
            FreshEcxPlusC: null,
            FreshEaxPlus8: null);

    private static CapacityFinalCalleeLargeSizeFieldsCapture LargeSizeCapture(
        uint? validatedEntryESI = 0x1000,
        uint? validatedEntryEBP = 0x2000,
        uint? validatedEntryEBX = 0x10,
        uint? freshEsiPlus18 = 0x3000,
        uint? freshEsiPlusC = 0x1001,
        uint? freshEsiPlus8 = 0x10,
        uint? freshEcxPlusC = 0x1000,
        uint? freshEaxPlus8 = 0x1000) =>
        new(
            validatedEntryESI,
            validatedEntryEBP,
            validatedEntryEBX,
            freshEsiPlus18,
            freshEsiPlusC,
            freshEsiPlus8,
            freshEcxPlusC,
            freshEaxPlus8);
}
