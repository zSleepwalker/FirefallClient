using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeEntryValidationChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeEntryValidationPlan zero =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xDEADC0DE,
                prefixValidatedNonzeroEntry: false,
                validationCaptureValidatedEntry: false,
                globalDword: null,
                esi4Dword: null));
        check("A clean zero entry preserves its return and admits no validation lifecycle",
            zero.Outcome == CapacityFinalCalleeEntryValidationOutcome.ZeroPathReturned
            && zero.PrefixLifecyclePlan.Outcome
                == CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathReturned
            && zero.PrefixLifecyclePlan.EntryPlan.ReturnReached
            && zero.PrefixLifecyclePlan.EntryPlan.ZeroPathPoppedEbp == 0xDEADC0DE
            && zero.PrefixLifecyclePlan.EntryPlan.EntryEcxIsZero == true
            && zero.PrefixLifecyclePlan.EntryPlan.TestedBitIsSet is null
            && zero.PrefixLifecyclePlan.PrefixCallPlan is null
            && zero.ZeroEntryReturned
            && zero.PrefixLifecycleOutcomeClean
            && !zero.ValidationLifecycleReached
            && zero.ValidationLifecyclePlan is null
            && !zero.UnreachedGlobalDwordCaptureProvided
            && !zero.UnreachedEsi4DwordCaptureProvided
            && !zero.UnreachedExpectedValidationEntryESIProvided
            && zero.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeEntryValidationProjection
                    .ZeroReturnBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeEntryValidationPlan zeroWithValidationCaptures =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: 0xAABBCCDD,
                prefixValidatedNonzeroEntry: false,
                validationCaptureValidatedEntry: false,
                globalDword: 0,
                esi4Dword: 0x12345679,
                expectedValidationEntryESI: 0x12345678));
        check("Extra global, ESI+4, or expected-identity captures stay unreached after zero RET",
            zeroWithValidationCaptures.Outcome
                == CapacityFinalCalleeEntryValidationOutcome
                    .ZeroPathHasUnreachedValidationCaptures
            && zeroWithValidationCaptures.ZeroEntryReturned
            && zeroWithValidationCaptures.PrefixLifecyclePlan.EntryPlan.ReturnReached
            && zeroWithValidationCaptures.PrefixLifecyclePlan.EntryPlan
                .ZeroPathPoppedEbp == 0xAABBCCDD
            && !zeroWithValidationCaptures.ValidationLifecycleReached
            && zeroWithValidationCaptures.ValidationLifecyclePlan is null
            && zeroWithValidationCaptures.UnreachedGlobalDwordCaptureProvided
            && zeroWithValidationCaptures.UnreachedEsi4DwordCaptureProvided
            && zeroWithValidationCaptures.UnreachedExpectedValidationEntryESIProvided
            && zeroWithValidationCaptures.PrefixLifecycleOutcomeClean
            && zeroWithValidationCaptures.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeEntryValidationProjection
                    .ZeroReturnBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeEntryValidationPlan maskClear =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0x12345678,
                testedByte: 0,
                validationEntryEsi: 0xDEADBEEF,
                globalDword: 0,
                esi4Dword: 0x12345679));
        CapacityFinalCalleeEntryValidationPlan maskSet =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0x12345678,
                testedByte: 0x02,
                callTarget: 0,
                callReturnedNormally: true,
                validationEntryEsi: 0x80000000,
                expectedValidationEntryESI: 0x80000000,
                globalDword: 0x7FFFFFFF,
                esi4Dword: 0xCAFEBABE));
        check("Both prior TEST mask routes compose, deriving bypass ESI and preserving opaque call ESI",
            maskClear.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            && maskClear.PrefixLifecyclePlan.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallSkipped
            && maskClear.ValidationLifecycleReached
            && maskClear.DeriveValidationLifecyclePrefix
            && maskClear.ValidationLifecyclePlan?.Capture.Prefix
                == maskClear.PrefixLifecyclePlan.PrefixCallPlan?.Capture
            && maskClear.ValidationLifecyclePlan?.DeriveValidationEntryESIFromPrefixOutgoing
                == true
            && maskClear.ValidationLifecyclePlan?.ValidationPlan.Capture.EntryESI
                == 0x12345670
            && maskClear.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && maskClear.ValidationOutcomeClean
            && !maskClear.UnreachedGlobalDwordCaptureProvided
            && !maskClear.UnreachedEsi4DwordCaptureProvided
            && maskSet.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            && maskSet.PrefixLifecyclePlan.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally
            && maskSet.ValidationLifecyclePlan?.Capture.Prefix
                == maskSet.PrefixLifecyclePlan.PrefixCallPlan?.Capture
            && maskSet.ValidationLifecyclePlan?.PrefixPlan.OutgoingESI is null
            && maskSet.ValidationLifecyclePlan?.DeriveValidationEntryESIFromPrefixOutgoing
                == false
            && maskSet.ValidationLifecyclePlan?.ValidationPlan.Capture.EntryESI
                == 0x80000000
            && maskSet.ValidationLifecyclePlan?.ExpectedValidationEntryESIMatches == true
            && maskSet.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && maskSet.ValidationOutcomeClean
            && !maskSet.UnreachedExpectedValidationEntryESIProvided
            && maskSet.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection
                    .BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeEntryValidationPlan expectedMismatch =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 4,
                testedByte: 0,
                validationEntryEsi: 0xDEADBEEF,
                expectedValidationEntryESI: 0xFFFFFFFD,
                globalDword: 0,
                esi4Dword: 0x12345679));
        check("A mismatched expected bypass ESI blocks before any validation read",
            expectedMismatch.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ValidationEntryESIMismatch
            && expectedMismatch.ValidationLifecycleReached
            && expectedMismatch.ValidationLifecyclePlan?.ExpectedValidationEntryESIMatches
                == false
            && expectedMismatch.ValidationLifecyclePlan?.DeriveValidationEntryESIFromPrefixOutgoing
                == true
            && expectedMismatch.ValidationLifecyclePlan?.ValidationPlan.Capture.EntryESI
                == 0xFFFFFFFC
            && expectedMismatch.ValidationLifecyclePlan?.ValidationPlan.Reads.IsEmpty == true
            && expectedMismatch.ValidationLifecyclePlan?.ValidationPlan.GlobalReadReached
                == false
            && expectedMismatch.UnreachedGlobalDwordCaptureProvided
            && expectedMismatch.UnreachedEsi4DwordCaptureProvided
            && !expectedMismatch.ValidationOutcomeClean
            && expectedMismatch.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset);

        CapacityFinalCalleeEntryValidationPlan missingEntryByte =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0x40,
                testedByte: null,
                validationCaptureValidatedEntry: true,
                globalDword: 0,
                esi4Dword: 0x12345679,
                expectedValidationEntryESI: 0x38));
        CapacityFinalCalleeEntryValidationPlan nonreturningCall =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 8,
                testedByte: 0x02,
                callTarget: 0,
                callReturnedNormally: false,
                validationCaptureValidatedEntry: true,
                globalDword: 0,
                esi4Dword: 0x12345679,
                expectedValidationEntryESI: 0));
        check("Missing entry bytes and nonreturning calls preserve upstream effects and reject later captures",
            missingEntryByte.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.UpstreamBlocked
            && missingEntryByte.PrefixLifecyclePlan.EntryPlan.Outcome
                == CapacityFinalCalleeEntryOutcome.MissingTestedByte
            && missingEntryByte.PrefixLifecyclePlan.EntryPlan.StackStores.Length == 1
            && missingEntryByte.PrefixLifecyclePlan.EntryPlan.StackStores[0].Kind
                == CapacityFinalCalleeEntryStackStoreKind.SavedEbp
            && !missingEntryByte.ValidationLifecycleReached
            && missingEntryByte.UnreachedValidationEntryAssertionProvided
            && missingEntryByte.UnreachedGlobalDwordCaptureProvided
            && missingEntryByte.UnreachedEsi4DwordCaptureProvided
            && missingEntryByte.UnreachedExpectedValidationEntryESIProvided
            && missingEntryByte.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeEntryProjection.TestByteRawFileOffset
            && nonreturningCall.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.UpstreamBlocked
            && nonreturningCall.PrefixLifecyclePlan.PrefixCallPlan?.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallDidNotReturnNormally
            && nonreturningCall.PrefixLifecyclePlan.EntryPlan.StackStores.Length == 4
            && nonreturningCall.PrefixLifecyclePlan.EntryPlan.PushEbxReached
            && nonreturningCall.PrefixLifecyclePlan.EntryPlan.PushEsiReached
            && nonreturningCall.PrefixLifecyclePlan.EntryPlan.PushEdiReached
            && nonreturningCall.PrefixLifecyclePlan.PrefixCallPlan?.PushImmediateReached
                == true
            && nonreturningCall.PrefixLifecyclePlan.PrefixCallPlan?.CallCellReadReached
                == true
            && nonreturningCall.PrefixLifecyclePlan.PrefixCallPlan?.CallReached == true
            && nonreturningCall.PrefixLifecyclePlan.PrefixCallPlan?.CallReturnedNormally
                == false
            && !nonreturningCall.ValidationLifecycleReached
            && nonreturningCall.UnreachedGlobalDwordCaptureProvided
            && nonreturningCall.UnreachedEsi4DwordCaptureProvided
            && nonreturningCall.UnreachedExpectedValidationEntryESIProvided
            && nonreturningCall.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleePrefixCallProjection.CallRawFileOffset);

        CapacityFinalCalleeEntryValidationPlan tagBranch =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0x12345678,
                testedByte: 0,
                globalDword: 0,
                esi4Dword: 0x12345679));
        CapacityFinalCalleeEntryValidationPlan tagFallthrough =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 0x12345678,
                testedByte: 0,
                globalDword: 0,
                esi4Dword: 0x12345678));
        CapacityFinalCalleeEntryValidationPlan unsignedBelow =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 8,
                testedByte: 0,
                globalDword: 1,
                esi4Dword: null));
        CapacityFinalCalleeEntryValidationPlan wrapped =
            CapacityFinalCalleeEntryValidationProjection.Project(Capture(
                entryEcx: 5,
                testedByte: 0,
                globalDword: 0,
                esi4Dword: 1));
        check("Tag branch, tag fallthrough, unsigned-below, and wrapped bypass ESI remain distinct",
            tagBranch.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && tagBranch.ValidationLifecyclePlan?.ValidationPlan.EqualOneBranchTaken == true
            && tagBranch.ValidationLifecyclePlan?.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection
                    .MaskEqualsOneDestinationRawFileOffset
            && tagFallthrough.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && tagFallthrough.ValidationLifecyclePlan?.ValidationPlan.EqualOneBranchTaken
                == false
            && tagFallthrough.ValidationLifecyclePlan?.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection
                    .MaskNotEqualOneFallthroughRawFileOffset
            && unsignedBelow.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
            && unsignedBelow.ValidationLifecyclePlan?.ValidationPlan.EntryEsiUnsignedBelowGlobal
                == true
            && unsignedBelow.ValidationLifecyclePlan?.ValidationPlan.Esi4ReadReached == false
            && unsignedBelow.ValidationOutcomeClean
            && wrapped.ValidationLifecyclePlan?.ValidationPlan.Capture.EntryESI
                == 0xFFFFFFFD
            && wrapped.ValidationLifecyclePlan?.ValidationPlan.Esi4Address == 1
            && wrapped.ValidationLifecyclePlan?.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && tagBranch.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            && tagFallthrough.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            && unsignedBelow.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            && wrapped.Outcome
                == CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete);
    }

    private static CapacityFinalCalleeEntryValidationCapture Capture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x2000,
        uint entryEcx = 0x12345678,
        uint entryEbx = 0x11111111,
        uint entryEsi = 0x22222222,
        uint entryEdi = 0x33333333,
        byte? testedByte = 0,
        uint? zeroPathPoppedEbp = null,
        bool prefixValidatedNonzeroEntry = true,
        uint? prefixEntryEcx = null,
        bool? priorTestMaskSet = null,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        bool validationCaptureValidatedEntry = false,
        uint validationEntryEsi = 0xDEADBEEF,
        uint? globalDword = 0,
        uint? esi4Dword = 0x12345679,
        uint? expectedValidationEntryESI = null) =>
        new(
            InputPrefixLifecycleCapture: new(
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
                    callReturnedNormally)),
            Validation: new(
                validationCaptureValidatedEntry,
                validationEntryEsi,
                globalDword,
                esi4Dword),
            ExpectedValidationEntryESI: expectedValidationEntryESI);
}
