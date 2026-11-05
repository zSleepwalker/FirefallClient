using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlignmentTagLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeAlignmentTagLifecyclePlan invalidEntry =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(captureValidatedEntry: false),
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        CapacityFinalCalleeAlignmentTagLifecyclePlan missingEntryRead =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(entryEax: 0x1000, entryEsiDword: null),
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 1));
        check("Unvalidated and missing upstream inputs block the tag window without fabricating a result",
            invalidEntry.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.EntryNotValidated
            && invalidEntry.AlignmentDispatch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryNotValidated
            && !invalidEntry.TagValidationReached
            && invalidEntry.TagValidation is null
            && !invalidEntry.Admitted
            && invalidEntry.UnreachedTagEdxPlusFourDwordProvided
            && invalidEntry.UnreachedTagGlobalDwordProvided
            && invalidEntry.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset
            && missingEntryRead.AlignmentDispatch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingEntryEsiDword
            && !missingEntryRead.TagValidationReached
            && missingEntryRead.TagValidation is null
            && !missingEntryRead.Admitted
            && missingEntryRead.UnreachedTagEdxPlusFourDwordProvided
            && missingEntryRead.UnreachedTagGlobalDwordProvided
            && missingEntryRead.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection.EntryEsiReadRawFileOffset);

        CapacityFinalCalleeAlignmentTagLifecyclePlan entryEcxRoute =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(
                    entryEax: 0x1000,
                    entryEsi: 0x20,
                    entryEcx: 1,
                    entryEsiDword: 0)));
        CapacityFinalCalleeAlignmentTagLifecyclePlan computedZeroRoute =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(
                    entryEax: 0xFFFFFFF0,
                    entryEsiDword: 0)));
        check("The entry-ECX and computed-zero routes do not feed tag validation",
            entryEcxRoute.AlignmentDispatch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
            && entryEcxRoute.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteNotSelected
            && !entryEcxRoute.TagValidationReached
            && entryEcxRoute.TagValidation is null
            && !entryEcxRoute.Admitted
            && computedZeroRoute.AlignmentDispatch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
            && computedZeroRoute.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteNotSelected
            && !computedZeroRoute.TagValidationReached
            && computedZeroRoute.TagValidation is null
            && !computedZeroRoute.Admitted);

        CapacityFinalCalleeAlignmentTagLifecyclePlan contaminatedAlRoute =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(
                    entryEax: 0x1001,
                    entryEsi: 7,
                    entryEsiDword: 0,
                    globalDispatchDword: 0,
                    callCellTarget: 0,
                    callReturnedNormally: true),
                capturedTagEntryEsi: 7,
                capturedTagEntryEdx: 15,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 15));
        check("The exact AL branch is rejected when upstream unreached captures contaminate it",
            contaminatedAlRoute.AlignmentDispatch.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && contaminatedAlRoute.AlignmentDispatch.TestAlSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentTagLifecycleProjection.TagRouteRawFileOffset
            && contaminatedAlRoute.AlignmentDispatch.UnreachedEntryEsiDwordCaptureProvided
            && contaminatedAlRoute.AlignmentDispatch.UnreachedGlobalDispatchDwordCaptureProvided
            && contaminatedAlRoute.AlignmentDispatch.UnreachedCallCellTargetCaptureProvided
            && contaminatedAlRoute.AlignmentDispatch.UnreachedCallReturnStatusProvided
            && contaminatedAlRoute.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteContaminated
            && contaminatedAlRoute.AlignmentDispatch.FrameWrites.Length == 2
            && !contaminatedAlRoute.AlignmentDispatch.CallReached
            && !contaminatedAlRoute.TagValidationReached
            && contaminatedAlRoute.TagValidation is null
            && contaminatedAlRoute.UnreachedCapturedTagEntryESIProvided
            && contaminatedAlRoute.UnreachedCapturedTagEntryEDXProvided
            && contaminatedAlRoute.UnreachedTagEdxPlusFourDwordProvided
            && contaminatedAlRoute.UnreachedTagGlobalDwordProvided
            && !contaminatedAlRoute.Admitted);

        CapacityFinalCalleeAlignmentTagLifecyclePlan unsignedExit =
            ProjectTag(entryEax: 1, entryEsi: 0x20);
        CapacityFinalCalleeAlignmentTagLifecyclePlan clZeroExit =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 0);
        CapacityFinalCalleeAlignmentTagLifecyclePlan clOneExit =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 3);
        CapacityFinalCalleeAlignmentTagLifecyclePlan notEqualExit =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 1, global: 0);
        CapacityFinalCalleeAlignmentTagLifecyclePlan equalBoundary =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 1, global: 15);
        CapacityFinalCalleeAlignmentTagLifecyclePlan unsignedExitWithUnreachedRead =
            ProjectTag(entryEax: 1, entryEsi: 0x20, edxPlusFour: 0);
        CapacityFinalCalleeAlignmentTagLifecyclePlan clZeroWithUnreachedGlobal =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 0, global: 15);
        check("Each clean tag exit and the equality boundary is a separate admitted bounded outcome",
            unsignedExit.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit
            && unsignedExit.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx
            && unsignedExit.TagEntryESI == 0x20
            && unsignedExit.TagEntryEDX == 0x20
            && unsignedExit.TagValidation?.SelectedDestinationRawFileOffset
                == CapacityFinalCalleeTagValidationProjection.UnsignedBelowDestinationRawFileOffset
            && unsignedExit.Admitted
            && clZeroExit.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitZeroClearExit
            && clZeroExit.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear
            && clZeroExit.Admitted
            && clOneExit.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitOneSetExit
            && clOneExit.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.ClBitOneSet
            && clOneExit.Admitted
            && notEqualExit.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagEntryEdxNotEqualGlobalDwordExit
            && notEqualExit.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword
            && notEqualExit.Admitted
            && equalBoundary.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagEntryEdxEqualsGlobalDwordBoundary
            && equalBoundary.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && equalBoundary.TagValidation?.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive
            && equalBoundary.Admitted
            && AllUseDerivedRegistersAndPreserveDispatchWrites(
                unsignedExit, clZeroExit, clOneExit, notEqualExit, equalBoundary));

        check("Reached tag plans propagate captures for reads their selected branches leave unreached",
            (unsignedExitWithUnreachedRead.TagValidation?.UnreachedEdxPlusFourDwordCaptureProvided
                == true)
            && unsignedExitWithUnreachedRead.UnreachedTagEdxPlusFourDwordProvided
            && !unsignedExitWithUnreachedRead.UnreachedTagGlobalDwordProvided
            && !unsignedExitWithUnreachedRead.Admitted
            && (clZeroWithUnreachedGlobal.TagValidation?.UnreachedGlobalDwordCaptureProvided
                == true)
            && clZeroWithUnreachedGlobal.UnreachedTagGlobalDwordProvided
            && !clZeroWithUnreachedGlobal.UnreachedTagEdxPlusFourDwordProvided
            && !clZeroWithUnreachedGlobal.Admitted);

        CapacityFinalCalleeAlignmentTagLifecyclePlan missingEdxPlusFour =
            ProjectTag(entryEax: 9, entryEsi: 7, global: 15);
        CapacityFinalCalleeAlignmentTagLifecyclePlan missingGlobal =
            ProjectTag(entryEax: 9, entryEsi: 7, edxPlusFour: 1);
        check("A missing reached tag read blocks admission and remains distinct from captured zero",
            missingEdxPlusFour.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingEdxPlusFourDword
            && missingEdxPlusFour.TagValidation?.EdxPlusFourReadReached == true
            && missingEdxPlusFour.TagValidation?.EdxPlusFourDwordCaptured == false
            && missingEdxPlusFour.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete
            && !missingEdxPlusFour.Admitted
            && missingGlobal.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.MissingGlobalDword
            && missingGlobal.TagValidation?.GlobalReadReached == true
            && missingGlobal.TagValidation?.GlobalDwordCaptured == false
            && missingGlobal.Outcome
                == CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete
            && !missingGlobal.Admitted
            && clZeroExit.TagValidation?.EdxPlusFourDwordCaptured == true
            && clZeroExit.TagValidation?.EdxPlusFourDword == 0
            && notEqualExit.TagValidation?.GlobalDwordCaptured == true
            && notEqualExit.TagValidation?.GlobalDword == 0);

        CapacityFinalCalleeAlignmentTagLifecyclePlan wrappedEdx =
            ProjectTag(
                entryEax: 0xFFFFFFF9,
                entryEsi: 7,
                edxPlusFour: 1,
                global: uint.MaxValue);
        check("The derived EDX wraps as a DWORD before the independent [EDX+4] read",
            wrappedEdx.TagEntryESI == 7
            && wrappedEdx.TagEntryEDX == uint.MaxValue
            && wrappedEdx.TagValidation?.EdxPlusFourAddress == 3
            && wrappedEdx.TagValidation?.Reads[0].Address == 3
            && wrappedEdx.TagValidation?.Outcome
                == CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
            && wrappedEdx.TagValidation?.GlobalDword == uint.MaxValue
            && wrappedEdx.Admitted);

        CapacityFinalCalleeAlignmentTagLifecyclePlan inconsistentDuplicate =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
                alignment: Dispatch(entryEax: 9, entryEsi: 7),
                capturedTagEntryEsi: 8,
                capturedTagEntryEdx: 15,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: 15));
        check("Optional duplicate register captures are checked but never replace derived tag registers",
            inconsistentDuplicate.TagEntryESI == 7
            && inconsistentDuplicate.TagEntryEDX == 15
            && inconsistentDuplicate.CapturedTagEntryESIMatchesDerivedValue == false
            && inconsistentDuplicate.CapturedTagEntryEDXMatchesDerivedValue == true
            && inconsistentDuplicate.TagValidation?.Capture.EntryESI == 7
            && inconsistentDuplicate.TagValidation?.Capture.EntryEDX == 15
            && !inconsistentDuplicate.Admitted);
    }

    private static CapacityFinalCalleeAlignmentTagLifecyclePlan ProjectTag(
        uint entryEax,
        uint entryEsi,
        uint? edxPlusFour = null,
        uint? global = null,
        uint? capturedTagEntryEsi = null,
        uint? capturedTagEntryEdx = null) =>
        CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(Capture(
            alignment: Dispatch(entryEax: entryEax, entryEsi: entryEsi),
            capturedTagEntryEsi,
            capturedTagEntryEdx,
            edxPlusFour,
            global));

    private static CapacityFinalCalleeAlignmentTagLifecycleCapture Capture(
        CapacityFinalCalleeAlignmentDispatchCapture alignment,
        uint? capturedTagEntryEsi = null,
        uint? capturedTagEntryEdx = null,
        uint? tagEdxPlusFourDword = null,
        uint? tagGlobalDword = null) =>
        new(
            alignment,
            capturedTagEntryEsi,
            capturedTagEntryEdx,
            tagEdxPlusFourDword,
            tagGlobalDword);

    private static CapacityFinalCalleeAlignmentDispatchCapture Dispatch(
        bool captureValidatedEntry = true,
        uint entryEax = 0x1001,
        uint entryEsi = 7,
        uint entryEcx = 0,
        uint entryEbp = 0x10000,
        uint entryEsp = 0x20000,
        uint? entryEsiDword = null,
        uint? globalDispatchDword = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedEntry,
            entryEax,
            entryEsi,
            entryEcx,
            entryEbp,
            entryEsp,
            entryEsiDword,
            globalDispatchDword,
            callCellTarget,
            callReturnedNormally);

    private static bool AllUseDerivedRegistersAndPreserveDispatchWrites(
        params CapacityFinalCalleeAlignmentTagLifecyclePlan[] plans) =>
        plans.All(plan =>
            plan.TagValidationReached
            && plan.TagEntryESI == plan.Capture.AlignmentDispatchCapture.EntryESI
            && plan.TagEntryEDX
                == unchecked(
                    (plan.Capture.AlignmentDispatchCapture.EntryEAX & 0xFFFFFFF8)
                    + plan.Capture.AlignmentDispatchCapture.EntryESI)
            && plan.AlignmentDispatch.FrameWrites.Length == 2
            && plan.AlignmentDispatch.EDX == plan.TagEntryEDX
            && plan.AlignmentDispatch.EDI
                == (plan.Capture.AlignmentDispatchCapture.EntryEAX & 0xFFFFFFF8)
            && !plan.AlignmentDispatch.CallReached
            && plan.AlignmentDispatch.Reads.IsEmpty);
}
