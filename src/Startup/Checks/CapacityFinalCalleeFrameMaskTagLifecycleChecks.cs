using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFrameMaskTagLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeFrameMaskTagLifecyclePlan cleanJoin =
            ProjectTag(entryEsi: 0, entryEdx: 0);
        check("A clean global-mask update joins at 12C8EF8 using independent zero ESI/EDX captures",
            cleanJoin.FrameMaskUpdate.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
            && cleanJoin.FrameMaskUpdate.GlobalWriteReached
            && cleanJoin.FrameMaskUpdate.GlobalValueAfter == 0x7FFFFFFF
            && !cleanJoin.FrameMaskUpdate.UnreachedEntryECXCaptureProvided
            && !cleanJoin.FrameMaskUpdate.UnreachedFreshFrameMinus14CaptureProvided
            && !cleanJoin.FrameMaskUpdate.UnreachedFreshFrameMinus4CaptureProvided
            && !cleanJoin.FrameMaskUpdate.UnreachedFreshFrameMinus8CaptureProvided
            && !cleanJoin.FrameMaskUpdate.UnreachedFreshGlobalValueCaptureProvided
            && cleanJoin.FrameMaskUpdate.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFrameMaskTagLifecycleProjection.TagEntryRawFileOffset
            && cleanJoin.TagValidationReached
            && cleanJoin.TagValidation?.Capture.EntryESI == 0
            && cleanJoin.TagValidation?.Capture.EntryEDX == 0
            && cleanJoin.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit
            && cleanJoin.TagValidation?.SelectedDestinationRawFileOffset
                == CapacityFinalCalleeTagValidationProjection.UnsignedBelowDestinationRawFileOffset
            && cleanJoin.Admitted);

        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingTagEntry =
            ProjectTag(
                entryEsi: null,
                entryEdx: 0,
                tagEdxPlusFour: 1,
                tagGlobal: 1);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingTagEdx =
            ProjectTag(entryEsi: 0, entryEdx: null);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan bothTagEntriesMissing =
            ProjectTag(entryEsi: null, entryEdx: null);
        check("Missing independent tag registers block only the later stage and preserve the mask write",
            missingTagEntry.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagEntryRegistersMissing
            && !missingTagEntry.TagValidationReached
            && missingTagEntry.TagValidation is null
            && missingTagEntry.UnreachedTagEntryEDXProvided
            && missingTagEntry.FrameMaskUpdate.GlobalWriteReached
            && missingTagEntry.FrameMaskUpdate.GlobalValueAfter == 0x7FFFFFFF
            && missingTagEntry.UnreachedTagEdxPlusFourDwordProvided
            && missingTagEntry.UnreachedTagGlobalDwordProvided
            && !missingTagEntry.Admitted
            && missingTagEdx.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagEntryRegistersMissing
            && missingTagEdx.FrameMaskUpdate.GlobalWriteReached
            && missingTagEdx.FrameMaskUpdate.GlobalValueAfter == 0x7FFFFFFF
            && !missingTagEdx.Admitted
            && bothTagEntriesMissing.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagEntryRegistersMissing
            && !bothTagEntriesMissing.TagValidationReached
            && bothTagEntriesMissing.FrameMaskUpdate.GlobalWriteReached
            && !bothTagEntriesMissing.Admitted);

        CapacityFinalCalleeFrameMaskTagLifecyclePlan cleanMismatch =
            Project(
                frameMask: FrameMask(
                    frameMinus14: 8,
                    frameMinus4: 4,
                    entryEcx: null,
                    frameMinus8: null,
                    global: null),
                entryEsi: null,
                entryEdx: null);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan contaminatedMismatch =
            Project(
                frameMask: FrameMask(
                    frameMinus14: 8,
                    frameMinus4: 4,
                    entryEcx: 0,
                    frameMinus8: 0,
                    global: 0),
                entryEsi: null,
                entryEdx: null);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan mismatchWithTagCapture =
            Project(
                frameMask: FrameMask(
                    frameMinus14: 8,
                    frameMinus4: 4,
                    entryEcx: null,
                    frameMinus8: null,
                    global: null),
                entryEsi: 0,
                entryEdx: null);
        check("Only a clean mismatch reaches its bounded destination; frame-mask or tag-only contamination blocks it",
            cleanMismatch.FrameMaskUpdate.SelectedBranchDestinationRawFileOffset
                == CapacityFinalCalleeFrameMaskTagLifecycleProjection
                    .FrameMismatchDestinationRawFileOffset
            && cleanMismatch.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMismatchBoundedExit
            && !cleanMismatch.TagValidationReached
            && !cleanMismatch.FrameMaskUpdate.UnreachedEntryECXCaptureProvided
            && !cleanMismatch.FrameMaskUpdate.UnreachedFreshFrameMinus8CaptureProvided
            && !cleanMismatch.FrameMaskUpdate.UnreachedFreshGlobalValueCaptureProvided
            && cleanMismatch.Admitted
            && cleanMismatch.StopAtRawFileOffsetExclusive == 0x012C8ED5
            && contaminatedMismatch.FrameMaskUpdate.UnreachedEntryECXCaptureProvided
            && contaminatedMismatch.FrameMaskUpdate
                .UnreachedFreshFrameMinus8CaptureProvided
            && contaminatedMismatch.FrameMaskUpdate
                .UnreachedFreshGlobalValueCaptureProvided
            && contaminatedMismatch.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskContaminated
            && !contaminatedMismatch.TagValidationReached
            && !contaminatedMismatch.Admitted
            && mismatchWithTagCapture.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .FrameMismatchTagCaptureContaminated
            && mismatchWithTagCapture.UnreachedTagEntryESIProvided
            && !mismatchWithTagCapture.TagValidationReached
            && !mismatchWithTagCapture.Admitted);

        CapacityFinalCalleeFrameMaskTagLifecyclePlan unvalidated =
            Project(
                frameMask: FrameMask(validatedEntryEbp: null),
                entryEsi: 0,
                entryEdx: 1,
                tagEdxPlusFour: 1,
                tagGlobal: 1);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingFrame =
            Project(
                frameMask: FrameMask(
                    frameMinus14: null,
                    entryEcx: null,
                    frameMinus4: null,
                    frameMinus8: null,
                    global: null),
                entryEsi: 0,
                entryEdx: 1,
                tagEdxPlusFour: 1,
                tagGlobal: 1);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingGlobal =
            Project(
                frameMask: FrameMask(global: null),
                entryEsi: 0,
                entryEdx: 1,
                tagEdxPlusFour: 1,
                tagGlobal: 1);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingEntryEcx =
            Project(
                frameMask: FrameMask(entryEcx: null, global: null),
                entryEsi: 0,
                entryEdx: 1,
                tagEdxPlusFour: 1,
                tagGlobal: 1);
        check("Unvalidated EBP and missing frame/global reads prevent the tag stage",
            unvalidated.FrameMaskUpdate.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP
            && unvalidated.FrameMaskUpdate.UnreachedFreshFrameMinus14CaptureProvided
            && unvalidated.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskContaminated
            && unvalidated.UnreachedTagEntryESIProvided
            && unvalidated.UnreachedTagEdxPlusFourDwordProvided
            && !unvalidated.TagValidationReached
            && missingFrame.FrameMaskUpdate.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus14
            && missingFrame.FrameMaskUpdate.FreshFrameMinus14ReadReached
            && missingFrame.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskIncomplete
            && !missingFrame.TagValidationReached
            && missingGlobal.FrameMaskUpdate.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshGlobalValue
            && missingGlobal.FrameMaskUpdate.GlobalReadReached
            && !missingGlobal.FrameMaskUpdate.GlobalWriteReached
            && missingGlobal.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskIncomplete
            && !missingGlobal.TagValidationReached
            && missingEntryEcx.FrameMaskUpdate.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryECX
            && missingEntryEcx.FrameMaskUpdate.FreshFrameMinus8Captured
            && !missingEntryEcx.FrameMaskUpdate.ShiftReached
            && !missingEntryEcx.FrameMaskUpdate.GlobalReadReached
            && missingEntryEcx.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskIncomplete
            && !missingEntryEcx.TagValidationReached);

        CapacityFinalCalleeFrameMaskTagLifecyclePlan unsignedExit =
            ProjectTag(entryEsi: 0x20, entryEdx: 0x20);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan clZeroExit =
            ProjectTag(entryEsi: 7, entryEdx: 15, tagEdxPlusFour: 0);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan clOneExit =
            ProjectTag(entryEsi: 7, entryEdx: 15, tagEdxPlusFour: 3);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan notEqualExit =
            ProjectTag(entryEsi: 7, entryEdx: 15, tagEdxPlusFour: 1, tagGlobal: 0);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan equalBoundary =
            ProjectTag(entryEsi: 7, entryEdx: 15, tagEdxPlusFour: 1, tagGlobal: 15);
        check("The five clean tag routes remain distinct, including the equality boundary",
            IsAdmittedRoute(
                unsignedExit,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
                CapacityFinalCalleeTagValidationOutcome
                    .EntryEsiUnsignedGreaterOrEqualEntryEdx,
                CapacityFinalCalleeTagValidationProjection.UnsignedBelowDestinationRawFileOffset)
            && IsAdmittedRoute(
                clZeroExit,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagClBitZeroClearExit,
                CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear,
                CapacityFinalCalleeTagValidationProjection.ClZeroDestinationRawFileOffset)
            && IsAdmittedRoute(
                clOneExit,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagClBitOneSetExit,
                CapacityFinalCalleeTagValidationOutcome.ClBitOneSet,
                CapacityFinalCalleeTagValidationProjection.ClOneDestinationRawFileOffset)
            && IsAdmittedRoute(
                notEqualExit,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEdxNotEqualGlobalDwordExit,
                CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword,
                CapacityFinalCalleeTagValidationProjection.NotEqualDestinationRawFileOffset)
            && IsAdmittedRoute(
                equalBoundary,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEdxEqualsGlobalDwordBoundary,
                CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword,
                CapacityFinalCalleeTagValidationProjection.BoundaryRawFileOffsetExclusive));

        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingTagEdxRead =
            ProjectTag(entryEsi: 0, entryEdx: 1, tagEdxPlusFour: null, tagGlobal: 1);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan missingTagGlobalRead =
            ProjectTag(entryEsi: 0, entryEdx: 1, tagEdxPlusFour: 1, tagGlobal: null);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan unreachedTagEdxRead =
            ProjectTag(entryEsi: 1, entryEdx: 1, tagEdxPlusFour: 0);
        CapacityFinalCalleeFrameMaskTagLifecyclePlan unreachedTagGlobalRead =
            ProjectTag(entryEsi: 0, entryEdx: 1, tagEdxPlusFour: 0, tagGlobal: 1);
        check("Missing reached reads and supplied unreached reads reject tag admission without erasing the frame-mask write",
            missingTagEdxRead.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingEdxPlusFourDword
            && missingTagEdxRead.TagValidation?.EdxPlusFourReadReached == true
            && missingTagEdxRead.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagValidationIncomplete
            && missingTagEdxRead.FrameMaskUpdate.GlobalWriteReached
            && missingTagGlobalRead.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingGlobalDword
            && missingTagGlobalRead.TagValidation?.GlobalReadReached == true
            && missingTagGlobalRead.Outcome
                == CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagValidationIncomplete
            && missingTagGlobalRead.FrameMaskUpdate.GlobalWriteReached
            && unreachedTagEdxRead.TagValidation?.UnreachedEdxPlusFourDwordCaptureProvided
                == true
            && unreachedTagEdxRead.UnreachedTagEdxPlusFourDwordProvided
            && !unreachedTagEdxRead.Admitted
            && unreachedTagGlobalRead.TagValidation?.UnreachedGlobalDwordCaptureProvided
                == true
            && unreachedTagGlobalRead.UnreachedTagGlobalDwordProvided
            && !unreachedTagGlobalRead.Admitted);

        CapacityFinalCalleeFrameMaskTagLifecyclePlan wrappedTagRead =
            ProjectTag(
                entryEsi: 0,
                entryEdx: uint.MaxValue,
                tagEdxPlusFour: 1,
                tagGlobal: uint.MaxValue);
        check("Tag reads retain DWORD address wrapping after the independent register captures",
            wrappedTagRead.TagValidation?.EdxPlusFourAddress == 3
            && wrappedTagRead.TagValidation?.Reads[0].Address == 3
            && wrappedTagRead.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && wrappedTagRead.TagValidation?.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFrameMaskTagLifecycleProjection
                    .TagBoundaryRawFileOffsetExclusive
            && wrappedTagRead.Admitted
            && wrappedTagRead.FrameMaskUpdate.GlobalWriteReached);
    }

    private static bool IsAdmittedRoute(
        CapacityFinalCalleeFrameMaskTagLifecyclePlan plan,
        CapacityFinalCalleeFrameMaskTagLifecycleOutcome expectedOutcome,
        CapacityFinalCalleeTagValidationOutcome expectedTagOutcome,
        uint expectedDestination) =>
        plan.Outcome == expectedOutcome
        && plan.TagValidationReached
        && plan.TagValidation?.Outcome == expectedTagOutcome
        && plan.TagValidation?.SelectedDestinationRawFileOffset == expectedDestination
        && plan.TagValidation?.StopAtRawFileOffsetExclusive == expectedDestination
        && plan.Admitted
        && plan.FrameMaskUpdate.GlobalWriteReached;

    private static CapacityFinalCalleeFrameMaskTagLifecyclePlan ProjectTag(
        uint? entryEsi,
        uint? entryEdx,
        uint? tagEdxPlusFour = null,
        uint? tagGlobal = null) =>
        Project(
            FrameMask(),
            entryEsi,
            entryEdx,
            tagEdxPlusFour,
            tagGlobal);

    private static CapacityFinalCalleeFrameMaskTagLifecyclePlan Project(
        CapacityFinalCalleeFrameMaskUpdateCapture frameMask,
        uint? entryEsi,
        uint? entryEdx,
        uint? tagEdxPlusFour = null,
        uint? tagGlobal = null) =>
        CapacityFinalCalleeFrameMaskTagLifecycleProjection.Project(
            new(
                frameMask,
                entryEsi,
                entryEdx,
                tagEdxPlusFour,
                tagGlobal));

    private static CapacityFinalCalleeFrameMaskUpdateCapture FrameMask(
        uint? validatedEntryEbp = 0x1000,
        uint? entryEcx = 0,
        uint? frameMinus14 = 0x1234,
        uint? frameMinus4 = 0x1234,
        uint? frameMinus8 = 0x80000000,
        uint? global = 0xFFFFFFFF) =>
        new(
            validatedEntryEbp,
            entryEcx,
            frameMinus14,
            frameMinus4,
            frameMinus8,
            global);
}
