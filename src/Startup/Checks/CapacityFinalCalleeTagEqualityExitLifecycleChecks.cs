using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeTagEqualityExitLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeTagEqualityExitLifecyclePlan maskedClear =
            ProjectCapacityExit(
                Exit(
                    entryValidated: false,
                    testedByte: 0,
                    popEdi: 0x11111111,
                    popEsi: 0x22222222,
                    popEbx: 0x33333333,
                    popEbp: 0x44444444));
        check("Only the clean capacity branch derives a validated 9441 entry and admits the mask-clear return",
            maskedClear.TagEquality.Admitted
            && maskedClear.TagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441
            && maskedClear.ExitEntryAdmitted
            && maskedClear.ExitPlan is CapacityFinalCalleeExitPlan clearExit
            && clearExit.Capture.EntryValidated
            && clearExit.Outcome == CapacityFinalCalleeExitOutcome.CallSkipped
            && clearExit.TestedBitIsSet == false
            && clearExit.PoppedEdi == 0x11111111
            && clearExit.PoppedEsi == 0x22222222
            && clearExit.PoppedEbx == 0x33333333
            && clearExit.PoppedEbp == 0x44444444
            && clearExit.ReturnReached
            && AllExitCapturesReached(clearExit)
            && maskedClear.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .CapacityExitReturnedNormally
            && maskedClear.ExitReturnedNormally
            && maskedClear.Admitted
            && maskedClear.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeTagEqualityExitLifecyclePlan maskedSet =
            ProjectCapacityExit(
                Exit(
                    entryValidated: false,
                    testedByte: 2,
                    popEdi: 0xA,
                    popEsi: 0xB,
                    popEbx: 0xC,
                    callTarget: 0x12345678,
                    callReturnedNormally: true,
                    outputEbx: 0xDEADBEEF,
                    outputEdi: 0xD1D1D1D1,
                    outputEsi: 0x51515151,
                    popEbp: 0xEEEEEEEE));
        check("The mask-set exit preserves independent POP and opaque post-call outputs",
            maskedSet.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .CapacityExitReturnedNormally
            && maskedSet.ExitPlan is CapacityFinalCalleeExitPlan setExit
            && setExit.Capture.EntryValidated
            && setExit.Outcome == CapacityFinalCalleeExitOutcome.ReturnedNormally
            && setExit.CallCellTargetVirtualAddress == 0x12345678
            && setExit.PoppedEbx == 0xC
            && setExit.CalleeOutputEbx == 0xDEADBEEF
            && setExit.PoppedEdi == 0xA
            && setExit.CalleeOutputEdi == 0xD1D1D1D1
            && setExit.PoppedEsi == 0xB
            && setExit.CalleeOutputEsi == 0x51515151
            && setExit.PoppedEbp == 0xEEEEEEEE
            && setExit.ReturnReached
            && AllExitCapturesReached(setExit)
            && maskedSet.ExitReturnedNormally
            && maskedSet.Admitted);

        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingByte =
            ProjectCapacityExit(
                Exit(
                    testedByte: null,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    popEbp: 5));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingEdi =
            ProjectCapacityExit(
                Exit(testedByte: 2, popEdi: null, popEsi: 2, popEbx: 3, popEbp: 4));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingEsi =
            ProjectCapacityExit(
                Exit(testedByte: 2, popEdi: 1, popEsi: null, popEbx: 3, popEbp: 4));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingEbx =
            ProjectCapacityExit(
                Exit(testedByte: 2, popEdi: 1, popEsi: 2, popEbx: null, popEbp: 4));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingEbp =
            ProjectCapacityExit(
                Exit(testedByte: 0, popEdi: 1, popEsi: 2, popEbx: 3, popEbp: null));
        check("Missing exit-byte and POP captures block success at the first missing instruction",
            missingByte.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingTestedByte
            && missingByte.ExitPlan?.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingByte.ExitPlan?.UnreachedStackPopEdiCaptureProvided == true
            && missingEdi.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEdiValue
            && missingEdi.ExitPlan?.StopAtRawFileOffsetExclusive == 0x012C9448
            && missingEsi.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEsiValue
            && missingEsi.ExitPlan?.PoppedEdi == 1
            && missingEbx.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEbxValue
            && missingEbx.ExitPlan?.PoppedEsi == 2
            && missingEbp.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingStackPopEbpValue
            && missingEbp.ExitPlan?.MovEspFromEbpReached == true
            && !missingEbp.ExitPlan!.Value.ReturnReached
            && new[] { missingByte, missingEdi, missingEsi, missingEbx, missingEbp }
                .All(plan => !plan.Admitted && !plan.ExitReturnedNormally
                    && plan.Outcome
                        == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                            .CapacityExitBlocked
                    && plan.TagEquality.Continuation!.Value.Writes.Length > 0));

        CapacityFinalCalleeTagEqualityExitLifecyclePlan missingExitCapture =
            ProjectCapacityExit(exitCapture: null);
        check("An absent selected-exit capture reaches the exit entry but stops on its missing TEST byte",
            !missingExitCapture.ExitCaptureSupplied
            && missingExitCapture.ExitEntryAdmitted
            && missingExitCapture.ExitPlan?.Capture.EntryValidated == true
            && missingExitCapture.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.MissingTestedByte
            && !missingExitCapture.Admitted
            && missingExitCapture.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingExitCapture.TagEquality.Continuation!.Value.Writes.Length > 0);

        CapacityFinalCalleeTagEqualityExitLifecyclePlan nonreturning =
            ProjectCapacityExit(
                Exit(
                    testedByte: 0xFF,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 0x1000,
                    callReturnedNormally: false,
                    outputEbx: 8,
                    popEbp: 4));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan skippedCallContamination =
            ProjectCapacityExit(
                Exit(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 0x1234,
                    callReturnedNormally: true,
                    outputEbx: 4,
                    outputEdi: 5,
                    outputEsi: 6,
                    popEbp: 7));
        check("Partial and nonreturning exits block only the composition and preserve earlier continuation writes",
            nonreturning.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome.CapacityExitBlocked
            && nonreturning.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.CallDidNotReturnNormally
            && nonreturning.ExitPlan?.CallReturnedNormally == false
            && nonreturning.ExitPlan?.UnreachedCalleeOutputEbxCaptureProvided == true
            && nonreturning.ExitPlan?.ReturnReached == false
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C9452
            && skippedCallContamination.ExitPlan?.Outcome
                == CapacityFinalCalleeExitOutcome.CallSkipped
            && skippedCallContamination.ExitPlan?.UnreachedCallCellTargetCaptureProvided == true
            && skippedCallContamination.ExitPlan?.UnreachedCallReturnStatusProvided == true
            && skippedCallContamination.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome.CapacityExitBlocked
            && !skippedCallContamination.Admitted
            && new[] { nonreturning, skippedCallContamination }.All(plan =>
                plan.TagEquality.Continuation!.Value.Writes.Length > 0));

        CapacityFinalCalleeTagEqualityExitLifecyclePlan wrapped =
            Project(
                entryEax: 0xFFFFFFF9,
                entryEsi: 7,
                tagEdxPlusFourDword: 1,
                tagGlobalDword: uint.MaxValue,
                initialGlobalDword: 0x10,
                freshTagDword: 7,
                capacityDword: 8,
                callReturnedNormally: null,
                exitCapture: null);
        check("Wrapped DWORD continuation effects remain committed when the selected exit lacks captures",
            wrapped.TagEquality.AlignmentTag.TagEntryEDX == uint.MaxValue
            && wrapped.TagEquality.Continuation?.ComputedEcx == 8
            && wrapped.TagEquality.Continuation?.EAX == 9
            && wrapped.TagEquality.Continuation?.Writes.Any(write =>
                write.Address == 0x0B && write.Value == 9) == true
            && wrapped.TagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441
            && wrapped.ExitPlan?.Outcome == CapacityFinalCalleeExitOutcome.MissingTestedByte
            && !wrapped.Admitted
            && wrapped.TagEquality.Continuation!.Value.Writes.Length > 0);

        CapacityFinalCalleeTagEqualityExitLifecyclePlan directCall =
            Project(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: true,
                exitCapture: null);
        CapacityFinalCalleeTagEqualityExitLifecyclePlan directCallWithEmptyExit =
            Project(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: true,
                exitCapture: Exit(
                    entryValidated: true,
                    testedByte: null,
                    popEdi: null,
                    popEsi: null,
                    popEbx: null,
                    popEbp: null));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan incompleteWithEmptyExit =
            Project(
                initialGlobalDword: null,
                freshTagDword: 8,
                capacityDword: 0x1010,
                callReturnedNormally: null,
                exitCapture: Exit(
                    entryValidated: true,
                    testedByte: null,
                    popEdi: null,
                    popEsi: null,
                    popEbx: null,
                    popEbp: null));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan contaminatedDirectCall =
            Project(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: true,
                exitCapture: Exit(testedByte: 2));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan directCallMissing =
            Project(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: null,
                exitCapture: null);
        CapacityFinalCalleeTagEqualityExitLifecyclePlan directCallNonreturn =
            Project(
                initialGlobalDword: 1,
                freshTagDword: 8,
                capacityDword: 0,
                callReturnedNormally: false,
                exitCapture: null);
        check("The clean F71 direct-call outcome stays distinct and never implies an exit at 9441",
            directCall.TagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .DirectCallReturnedNormallyAtF71
            && directCall.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .DirectCallReturnedNormallyAtF71
            && directCall.Admitted
            && directCall.StopAtRawFileOffsetExclusive == 0x012C8F71
            && directCall.ExitPlan is null
            && !directCall.ExitEntryAdmitted
            && directCallWithEmptyExit.UnreachedExitCaptureProvided
            && directCallWithEmptyExit.ExitCaptureSupplied
            && directCallWithEmptyExit.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .NonCapacityRouteContaminatedByExitCaptures
            && !directCallWithEmptyExit.Admitted
            && directCallWithEmptyExit.ExitPlan is null
            && contaminatedDirectCall.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .NonCapacityRouteContaminatedByExitCaptures
            && contaminatedDirectCall.UnreachedExitCaptureProvided
            && !contaminatedDirectCall.Admitted
            && contaminatedDirectCall.ExitPlan is null
            && contaminatedDirectCall.TagEquality.Continuation!.Value.Writes.Length > 0
            && incompleteWithEmptyExit.TagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome.ContinuationIncomplete
            && incompleteWithEmptyExit.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .TagEqualityContinuationIncomplete
            && incompleteWithEmptyExit.ExitCaptureSupplied
            && incompleteWithEmptyExit.UnreachedExitCaptureProvided
            && incompleteWithEmptyExit.ExitPlan is null
            && !incompleteWithEmptyExit.Admitted
            && directCallMissing.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .DirectCallReturnStatusMissing
            && !directCallMissing.Admitted
            && directCallNonreturn.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .DirectCallDidNotReturnNormally
            && !directCallNonreturn.Admitted);

        CapacityFinalCalleeTagEqualityExitLifecyclePlan cleanOtherTagRoute =
            Project(
                entryEax: 0xFFFFFFF9,
                entryEsi: 0x10,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null,
                exitCapture: null);
        CapacityFinalCalleeTagEqualityExitLifecyclePlan contaminatedOtherTagRoute =
            Project(
                entryEax: 0xFFFFFFF9,
                entryEsi: 0x10,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null,
                exitCapture: Exit(testedByte: 0));
        CapacityFinalCalleeTagEqualityExitLifecyclePlan emptyExitOtherTagRoute =
            Project(
                entryEax: 0xFFFFFFF9,
                entryEsi: 0x10,
                tagEdxPlusFourDword: null,
                tagGlobalDword: null,
                initialGlobalDword: null,
                freshTagDword: null,
                capacityDword: null,
                callReturnedNormally: null,
                exitCapture: Exit(
                    entryValidated: true,
                    testedByte: null,
                    popEdi: null,
                    popEsi: null,
                    popEbx: null,
                    popEbp: null));
        check("Other clean tag destinations stay bounded only when no exit-only values are supplied",
            cleanOtherTagRoute.TagEquality.Outcome
                == CapacityFinalCalleeTagEqualityLifecycleOutcome
                    .TagRouteCompletedWithoutEqualityBoundary
            && cleanOtherTagRoute.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .TagRouteCompletedWithoutCapacityExit
            && cleanOtherTagRoute.Admitted
            && cleanOtherTagRoute.ExitPlan is null
            && !cleanOtherTagRoute.ExitCaptureSupplied
            && cleanOtherTagRoute.StopAtRawFileOffsetExclusive
                == cleanOtherTagRoute.TagEquality.StopAtRawFileOffsetExclusive
            && contaminatedOtherTagRoute.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .NonCapacityRouteContaminatedByExitCaptures
            && contaminatedOtherTagRoute.UnreachedExitCaptureProvided
            && !contaminatedOtherTagRoute.Admitted
            && contaminatedOtherTagRoute.ExitPlan is null
            && emptyExitOtherTagRoute.ExitCaptureSupplied
            && emptyExitOtherTagRoute.UnreachedExitCaptureProvided
            && emptyExitOtherTagRoute.Outcome
                == CapacityFinalCalleeTagEqualityExitLifecycleOutcome
                    .NonCapacityRouteContaminatedByExitCaptures
            && !emptyExitOtherTagRoute.Admitted
            && emptyExitOtherTagRoute.ExitPlan is null);
    }

    private static CapacityFinalCalleeTagEqualityExitLifecyclePlan ProjectCapacityExit(
        CapacityFinalCalleeExitCapture? exitCapture = null) =>
        Project(
            initialGlobalDword: 0xFFFFFFF0,
            freshTagDword: 7,
            capacityDword: 0x1010,
            callReturnedNormally: null,
            exitCapture: exitCapture);

    private static CapacityFinalCalleeTagEqualityExitLifecyclePlan Project(
        uint entryEax = 0x1001,
        uint entryEsi = 7,
        uint entryEsp = 0x20000,
        uint? entryEsiDword = null,
        uint? globalDispatchDword = null,
        uint? callCellTargetVirtualAddress = null,
        bool? dispatchCallReturnedNormally = null,
        uint? tagEdxPlusFourDword = 1,
        uint? tagGlobalDword = 0x1007,
        uint? capturedTagEntryEdx = null,
        uint? initialGlobalDword = 0,
        uint? freshTagDword = 0,
        uint? capacityDword = 0,
        bool? callReturnedNormally = false,
        CapacityFinalCalleeExitCapture? exitCapture = null)
    {
        var alignmentDispatch = new CapacityFinalCalleeAlignmentDispatchCapture(
            CaptureValidatedEntry: true,
            EntryEAX: entryEax,
            EntryESI: entryEsi,
            EntryECX: 0,
            EntryEBP: 0x10000,
            EntryESP: entryEsp,
            EntryEsiDword: entryEsiDword,
            GlobalDispatchDword: globalDispatchDword,
            CallCellTargetVirtualAddress: callCellTargetVirtualAddress,
            CallReturnedNormally: dispatchCallReturnedNormally);
        var alignmentTag = new CapacityFinalCalleeAlignmentTagLifecycleCapture(
            alignmentDispatch,
            CapturedTagEntryEDX: capturedTagEntryEdx,
            TagEdxPlusFourDword: tagEdxPlusFourDword,
            TagGlobalDword: tagGlobalDword);
        var tagEquality = new CapacityFinalCalleeTagEqualityLifecycleCapture(
            alignmentTag,
            initialGlobalDword,
            freshTagDword,
            capacityDword,
            callReturnedNormally);
        return CapacityFinalCalleeTagEqualityExitLifecycleProjection.Project(
            new(tagEquality, exitCapture));
    }

    private static CapacityFinalCalleeExitCapture Exit(
        bool entryValidated = false,
        byte? testedByte = 0,
        uint? popEdi = 0x1000,
        uint? popEsi = 0x2000,
        uint? popEbx = 0x3000,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? outputEbx = null,
        uint? outputEdi = null,
        uint? outputEsi = null,
        uint? popEbp = 0x4000) =>
        new(
            entryValidated,
            testedByte,
            popEdi,
            popEsi,
            popEbx,
            callTarget,
            callReturnedNormally,
            outputEbx,
            outputEdi,
            outputEsi,
            popEbp);

    private static bool AllExitCapturesReached(CapacityFinalCalleeExitPlan plan) =>
        !plan.UnreachedTestedByteCaptureProvided
        && !plan.UnreachedStackPopEdiCaptureProvided
        && !plan.UnreachedStackPopEsiCaptureProvided
        && !plan.UnreachedStackPopEbxCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.UnreachedCalleeOutputEbxCaptureProvided
        && !plan.UnreachedCalleeOutputEdiCaptureProvided
        && !plan.UnreachedCalleeOutputEsiCaptureProvided
        && !plan.UnreachedStackPopEbpCaptureProvided;
}
