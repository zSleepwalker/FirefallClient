using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePrefixLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition stops at the zero RET or the exclusive prefix-call boundary",
            CapacityFinalCalleePrefixLifecycleProjection.LifecycleStartRawFileOffset
                == 0x012C8D60
            && CapacityFinalCalleePrefixLifecycleProjection.NonzeroPrefixBoundaryRawFileOffsetExclusive
                == 0x012C8D78
            && CapacityFinalCalleePrefixLifecycleProjection.PrefixCallBoundaryRawFileOffsetExclusive
                == 0x012C8D88
            && CapacityFinalCalleePrefixLifecycleProjection.ZeroReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C);

        CapacityFinalCalleePrefixLifecyclePlan zeroPath =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xDEADC0DE,
                prefixValidatedNonzeroEntry: false));
        check("A clean zero entry admits only the captured zero epilogue and has no prefix-call plan",
            zeroPath.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathReturned
            && zeroPath.EntryPlan.Outcome
                == CapacityFinalCalleeEntryOutcome.ZeroPathReturned
            && zeroPath.EntryOutcomeClean
            && zeroPath.ZeroEpilogueAdmitted
            && !zeroPath.DeriveValidatedNonzeroEntry
            && !zeroPath.PrefixCallProjectionReached
            && zeroPath.PrefixCallPlan is null
            && zeroPath.AuthoritativeEntryECX == 0
            && zeroPath.PrefixEntryEcxMatchesAuthoritative == true
            && zeroPath.EntryPlan.TestedBitIsSet is null
            && zeroPath.UnreachedPrefixEntryEcxCaptureProvided
            && !zeroPath.UnreachedPrefixPriorTestMaskCaptureProvided
            && !zeroPath.UnreachedPrefixCallCellTargetCaptureProvided
            && !zeroPath.UnreachedPrefixCallReturnStatusProvided
            && zeroPath.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleePrefixLifecyclePlan zeroWithExtraCallCaptures =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xAABBCCDD,
                prefixValidatedNonzeroEntry: true,
                priorTestMaskSet: false,
                callTarget: 0,
                callReturnedNormally: true));
        check("An extra zero-path TEST/call capture or asserted nonzero validation never admits a prefix call",
            zeroWithExtraCallCaptures.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathHasUnreachedCaptures
            && zeroWithExtraCallCaptures.EntryPlan.ReturnReached
            && !zeroWithExtraCallCaptures.ZeroEpilogueAdmitted
            && zeroWithExtraCallCaptures.PrefixCallPlan is null
            && !zeroWithExtraCallCaptures.PrefixCallProjectionReached
            && !zeroWithExtraCallCaptures.PrefixValidatedNonzeroFlagMatchesDerived
            && zeroWithExtraCallCaptures.UnreachedPrefixPriorTestMaskCaptureProvided
            && zeroWithExtraCallCaptures.UnreachedPrefixCallCellTargetCaptureProvided
            && zeroWithExtraCallCaptures.UnreachedPrefixCallReturnStatusProvided
            && zeroWithExtraCallCaptures.UnreachedPrefixNonzeroValidationAsserted
            && zeroWithExtraCallCaptures.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleePrefixLifecyclePlan maskClear =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 4,
                testedByte: 0,
                prefixValidatedNonzeroEntry: false,
                priorTestMaskSet: true));
        CapacityFinalCalleePrefixLifecyclePlan maskSet =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 0x12345678,
                testedByte: 0x02,
                prefixValidatedNonzeroEntry: false,
                priorTestMaskSet: false,
                callTarget: 0,
                callReturnedNormally: true));
        check("A clean nonzero entry supplies validation, ECX, and prior TEST flags to either prefix route",
            maskClear.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ProjectionComplete
            && maskClear.DeriveValidatedNonzeroEntry
            && maskClear.PrefixCallPlan?.Capture.CaptureValidatedNonzeroEntry == true
            && maskClear.PrefixCallPlan?.Capture.EntryECX == 4
            && maskClear.PrefixCallPlan?.Capture.CapturedPriorTestMaskSet == false
            && maskClear.SuppliedPriorTestMaskMatchesDerived == false
            && maskClear.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallSkipped
            && maskClear.PrefixCallPlan?.OutgoingESI == 0xFFFFFFFC
            && maskClear.PrefixCallPlan?.CallReached == false
            && maskClear.PrefixCallPlan?.StopAtRawFileOffsetExclusive == 0x012C8D88
            && maskSet.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ProjectionComplete
            && maskSet.PrefixCallPlan?.Capture.CaptureValidatedNonzeroEntry == true
            && maskSet.PrefixCallPlan?.Capture.EntryECX == 0x12345678
            && maskSet.PrefixCallPlan?.Capture.CapturedPriorTestMaskSet == true
            && maskSet.SuppliedPriorTestMaskMatchesDerived == false
            && maskSet.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally
            && maskSet.PrefixCallPlan?.CallCellTargetVirtualAddress == 0
            && maskSet.PrefixCallPlan?.OutgoingESI is null
            && maskSet.StopAtRawFileOffsetExclusive == 0x012C8D88);

        CapacityFinalCalleePrefixLifecyclePlan missingEntryByte =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEsp: 0x20,
                entryEcx: 0x40,
                testedByte: null,
                prefixValidatedNonzeroEntry: true,
                priorTestMaskSet: true,
                callTarget: 0x1000,
                callReturnedNormally: true));
        check("A missing nonzero-path byte retains the entry frame write and marks all later captures unreached",
            missingEntryByte.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.EntryBlocked
            && missingEntryByte.EntryPlan.Outcome
                == CapacityFinalCalleeEntryOutcome.MissingTestedByte
            && missingEntryByte.EntryPlan.StackStores.Length == 1
            && missingEntryByte.EntryPlan.StackStores[0].Kind
                == CapacityFinalCalleeEntryStackStoreKind.SavedEbp
            && missingEntryByte.EntryPlan.StackStores[0].Address == 0x1C
            && missingEntryByte.EntryPlan.FrameEsp == 0xFFFFFFEC
            && !missingEntryByte.DeriveValidatedNonzeroEntry
            && missingEntryByte.PrefixCallPlan is null
            && missingEntryByte.UnreachedPrefixEntryEcxCaptureProvided
            && missingEntryByte.UnreachedPrefixPriorTestMaskCaptureProvided
            && missingEntryByte.UnreachedPrefixCallCellTargetCaptureProvided
            && missingEntryByte.UnreachedPrefixCallReturnStatusProvided
            && missingEntryByte.UnreachedPrefixNonzeroValidationAsserted
            && missingEntryByte.StopAtRawFileOffsetExclusive == 0x012C8D6E);

        CapacityFinalCalleePrefixLifecyclePlan missingCallTarget =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 8,
                testedByte: 0x02,
                callTarget: null,
                callReturnedNormally: true));
        check("A partial prefix call preserves its reached push and stops at CALL",
            missingCallTarget.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.PrefixCallHasUnreachedCaptures
            && missingCallTarget.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.MissingCallCellTarget
            && missingCallTarget.PrefixCallPlan?.PushImmediateReached == true
            && missingCallTarget.PrefixCallPlan?.CallCellReadReached == true
            && missingCallTarget.PrefixCallPlan?.CallReached == false
            && missingCallTarget.UnreachedPrefixCallReturnStatusProvided
            && missingCallTarget.StopAtRawFileOffsetExclusive == 0x012C8D82);

        CapacityFinalCalleePrefixLifecyclePlan wrapped =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEsp: 0,
                entryEcx: uint.MaxValue,
                testedByte: 0x02,
                callTarget: 0,
                callReturnedNormally: true));
        check("Entry-derived stack addresses and the LEA ESI result wrap as DWORDs",
            wrapped.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ProjectionComplete
            && wrapped.EntryPlan.StackStores.Select(store => store.Address).SequenceEqual(new[]
            {
                0xFFFFFFFCu,
                0xFFFFFFC8u,
                0xFFFFFFC4u,
                0xFFFFFFC0u
            })
            && wrapped.PrefixCallPlan?.DerivedPrecallESI == 0xFFFFFFF7
            && wrapped.PrefixCallPlan?.CallReturnedNormally == true
            && wrapped.PrefixCallPlan?.OutgoingESI is null
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8D88);

        CapacityFinalCalleePrefixLifecyclePlan duplicateEcxMismatch =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryEcx: 0x100,
                testedByte: 0,
                prefixEntryEcx: 0x101,
                priorTestMaskSet: true));
        check("The duplicate prefix ECX must match the validated entry value before LEA",
            duplicateEcxMismatch.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.PrefixEntryEcxMismatch
            && duplicateEcxMismatch.AuthoritativeEntryECX == 0x100
            && duplicateEcxMismatch.PrefixEntryEcxMatchesAuthoritative == false
            && !duplicateEcxMismatch.PrefixCallProjectionReached
            && duplicateEcxMismatch.PrefixCallPlan is null
            && duplicateEcxMismatch.UnreachedPrefixPriorTestMaskCaptureProvided
            && duplicateEcxMismatch.StopAtRawFileOffsetExclusive == 0x012C8D78);

        CapacityFinalCalleePrefixLifecyclePlan unvalidatedEntryWithAssertedPrefix =
            CapacityFinalCalleePrefixLifecycleProjection.Project(Capture(
                entryValidated: false,
                entryEcx: 0x100,
                testedByte: 0x02,
                prefixValidatedNonzeroEntry: true,
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: true));
        check("A prefix validation assertion cannot bypass an unvalidated entry",
            unvalidatedEntryWithAssertedPrefix.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.EntryHasUnreachedCaptures
            && unvalidatedEntryWithAssertedPrefix.EntryPlan.Outcome
                == CapacityFinalCalleeEntryOutcome.EntryNotValidated
            && unvalidatedEntryWithAssertedPrefix.EntryPlan.StackStores.IsEmpty
            && !unvalidatedEntryWithAssertedPrefix.DeriveValidatedNonzeroEntry
            && !unvalidatedEntryWithAssertedPrefix.PrefixCallProjectionReached
            && unvalidatedEntryWithAssertedPrefix.PrefixCallPlan is null
            && !unvalidatedEntryWithAssertedPrefix.PrefixValidatedNonzeroFlagMatchesDerived
            && unvalidatedEntryWithAssertedPrefix.UnreachedPrefixPriorTestMaskCaptureProvided
            && unvalidatedEntryWithAssertedPrefix.UnreachedPrefixCallCellTargetCaptureProvided
            && unvalidatedEntryWithAssertedPrefix.UnreachedPrefixCallReturnStatusProvided
            && unvalidatedEntryWithAssertedPrefix.UnreachedPrefixNonzeroValidationAsserted
            && unvalidatedEntryWithAssertedPrefix.StopAtRawFileOffsetExclusive
                == 0x012C8D60);
    }

    private static CapacityFinalCalleePrefixLifecycleCapture Capture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x2000,
        uint entryEcx = 1,
        uint entryEbx = 0x11111111,
        uint entryEsi = 0x22222222,
        uint entryEdi = 0x33333333,
        byte? testedByte = 0,
        uint? zeroPathPoppedEbp = null,
        bool prefixValidatedNonzeroEntry = true,
        uint? prefixEntryEcx = null,
        bool? priorTestMaskSet = null,
        uint? callTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            Entry: new(
                entryValidated,
                entryEsp,
                entryEbp,
                entryEcx,
                entryEbx,
                entryEsi,
                entryEdi,
                testedByte,
                zeroPathPoppedEbp),
            PrefixCall: new(
                prefixValidatedNonzeroEntry,
                prefixEntryEcx ?? entryEcx,
                priorTestMaskSet,
                callTarget,
                callReturnedNormally));
}
