using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeValidationLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeValidationLifecyclePlan skipped =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                validationCaptureValidatedEntry: false,
                validationEntryEsi: 0xDEADBEEF,
                globalDword: 0,
                esi4Dword: 0x12345679));
        check("A clean skipped call derives validation ESI and the entry-validation bool",
            skipped.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
            && skipped.PrefixPlan.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallSkipped
            && skipped.PrefixOutcomeClean
            && skipped.DeriveValidatedEntry
            && skipped.DeriveValidationEntryESIFromPrefixOutgoing
            && skipped.ValidationPlan.Capture.CaptureValidatedEntry
            && skipped.ValidationPlan.Capture.EntryESI
                == skipped.PrefixPlan.OutgoingESI
            && skipped.ValidationPlan.Capture.EntryESI == 0x12345670
            && skipped.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && skipped.ValidationPlan.Esi4Address == 0x12345674
            && skipped.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == 0x012C8E76
            && skipped.ValidationOutcomeClean
            && !skipped.UnreachedGlobalDwordCaptureProvided
            && !skipped.UnreachedEsi4DwordCaptureProvided
            && skipped.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.MaskEqualsOneDestinationRawFileOffset);

        CapacityFinalCalleeValidationLifecyclePlan returned =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                entryEcx: 0x12345678,
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: true,
                validationCaptureValidatedEntry: false,
                validationEntryEsi: 0x80000000,
                expectedValidationEntryESI: 0x80000000,
                globalDword: 0x7FFFFFFF,
                esi4Dword: 0xCAFEBABE));
        check("A clean returned call retains independent validation ESI despite opaque call outputs",
            returned.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
            && returned.PrefixPlan.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally
            && returned.PrefixPlan.CallCellTargetVirtualAddress == 0
            && returned.PrefixPlan.OutgoingESI is null
            && returned.DeriveValidatedEntry
            && !returned.DeriveValidationEntryESIFromPrefixOutgoing
            && returned.ExpectedValidationEntryESIValidated
            && returned.ExpectedValidationEntryESIMatches == true
            && returned.ValidationPlan.Capture.CaptureValidatedEntry
            && returned.ValidationPlan.Capture.EntryESI == 0x80000000
            && returned.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && returned.ValidationPlan.FirstSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.EsiUnsignedFallthroughRawFileOffset
            && returned.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.MaskNotEqualOneFallthroughRawFileOffset
            && returned.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeValidationLifecyclePlan bypassMismatch =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                validationCaptureValidatedEntry: true,
                validationEntryEsi: 0x12345670,
                expectedValidationEntryESI: 0x12345671,
                globalDword: 0,
                esi4Dword: 0));
        check("An optional bypass-entry mismatch blocks before the global read",
            bypassMismatch.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ValidationEntryESIMismatch
            && bypassMismatch.PrefixOutcomeClean
            && bypassMismatch.DeriveValidationEntryESIFromPrefixOutgoing
            && bypassMismatch.ExpectedValidationEntryESIValidated
            && bypassMismatch.ExpectedValidationEntryESIMatches == false
            && !bypassMismatch.DeriveValidatedEntry
            && !bypassMismatch.ValidationPlan.Capture.CaptureValidatedEntry
            && bypassMismatch.ValidationPlan.Capture.EntryESI == 0x12345670
            && bypassMismatch.ValidationPlan.Reads.IsEmpty
            && !bypassMismatch.ValidationPlan.GlobalReadReached
            && !bypassMismatch.ValidationPlan.Esi4ReadReached
            && bypassMismatch.UnreachedGlobalDwordCaptureProvided
            && bypassMismatch.UnreachedEsi4DwordCaptureProvided
            && bypassMismatch.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset);

        CapacityFinalCalleeValidationLifecyclePlan missingPriorFlags =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: null,
                callTarget: 0x1000,
                callReturnedNormally: true,
                validationCaptureValidatedEntry: true));
        CapacityFinalCalleeValidationLifecyclePlan missingCallCell =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: null,
                callReturnedNormally: true,
                validationCaptureValidatedEntry: true));
        CapacityFinalCalleeValidationLifecyclePlan missingReturnStatus =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: 0x1000,
                callReturnedNormally: null,
                validationCaptureValidatedEntry: true));
        CapacityFinalCalleeValidationLifecyclePlan nonreturning =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: true,
                callTarget: 0,
                callReturnedNormally: false,
                validationCaptureValidatedEntry: true));
        check("Partial, missing-flag, and nonreturning prefixes retain their reached effects and leave validation reads unreached",
            missingPriorFlags.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.PrefixNotAdmitted
            && missingPriorFlags.PrefixPlan.LeaEsiReached
            && missingPriorFlags.PrefixPlan.DerivedPrecallESI == 0x12345670
            && !missingPriorFlags.PrefixPlan.BranchReached
            && missingPriorFlags.PrefixPlan.UnreachedCallCellTargetCaptureProvided
            && missingPriorFlags.PrefixPlan.UnreachedCallReturnStatusProvided
            && missingCallCell.PrefixPlan.PushImmediateReached
            && missingCallCell.PrefixPlan.CallCellReadReached
            && !missingCallCell.PrefixPlan.CallCellTargetCaptured
            && missingCallCell.PrefixPlan.UnreachedCallReturnStatusProvided
            && missingReturnStatus.PrefixPlan.PushImmediateReached
            && missingReturnStatus.PrefixPlan.CallCellTargetCaptured
            && missingReturnStatus.PrefixPlan.CallReached
            && nonreturning.PrefixPlan.CallReached
            && nonreturning.PrefixPlan.CallReturnedNormally == false
            && new[]
            {
                missingPriorFlags,
                missingCallCell,
                missingReturnStatus,
                nonreturning
            }.All(plan =>
                plan.Outcome == CapacityFinalCalleeValidationLifecycleOutcome.PrefixNotAdmitted
                && !plan.DeriveValidatedEntry
                && plan.ValidationPlan.Outcome
                    == CapacityFinalCalleeFirstValidationOutcome.EntryNotValidated
                && plan.ValidationPlan.Reads.IsEmpty
                && !plan.ValidationPlan.GlobalReadReached
                && !plan.ValidationPlan.Esi4ReadReached
                && plan.UnreachedGlobalDwordCaptureProvided
                && plan.UnreachedEsi4DwordCaptureProvided));

        CapacityFinalCalleeValidationLifecyclePlan unvalidatedPrefix =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                prefixValidated: false,
                priorTestMaskSet: false,
                validationCaptureValidatedEntry: true,
                globalDword: 0,
                esi4Dword: 0));
        CapacityFinalCalleeValidationLifecyclePlan flaggedBypass =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                callTarget: 0x1000,
                callReturnedNormally: true,
                validationCaptureValidatedEntry: true));
        check("Supplied validation admission cannot bypass an unvalidated or capture-contaminated prefix",
            unvalidatedPrefix.PrefixPlan.Outcome
                == CapacityFinalCalleePrefixCallOutcome.EntryNotValidated
            && !unvalidatedPrefix.PrefixPlan.LeaEsiReached
            && unvalidatedPrefix.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryNotValidated
            && !unvalidatedPrefix.DeriveValidatedEntry
            && unvalidatedPrefix.UnreachedGlobalDwordCaptureProvided
            && unvalidatedPrefix.UnreachedEsi4DwordCaptureProvided
            && flaggedBypass.PrefixPlan.Outcome
                == CapacityFinalCalleePrefixCallOutcome.CallSkipped
            && flaggedBypass.PrefixPlan.UnreachedCallCellTargetCaptureProvided
            && flaggedBypass.PrefixPlan.UnreachedCallReturnStatusProvided
            && !flaggedBypass.PrefixOutcomeClean
            && !flaggedBypass.DeriveValidatedEntry
            && flaggedBypass.ValidationPlan.Reads.IsEmpty
            && flaggedBypass.UnreachedGlobalDwordCaptureProvided
            && flaggedBypass.UnreachedEsi4DwordCaptureProvided);

        CapacityFinalCalleeValidationLifecyclePlan unsignedBelow =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                globalDword: 0x80000000,
                esi4Dword: null));
        CapacityFinalCalleeValidationLifecyclePlan unsignedBelowWithUnreachedEsi4 =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                globalDword: 0x80000000,
                esi4Dword: 0x12345679));
        CapacityFinalCalleeValidationLifecyclePlan tagBranch =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                globalDword: 0,
                esi4Dword: 0x12345679));
        CapacityFinalCalleeValidationLifecyclePlan tagFallthrough =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                globalDword: 0,
                esi4Dword: 0x12345678));
        check("Unsigned-below, tag-equals-one branch, and tag fallthrough remain distinct",
            unsignedBelow.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
            && unsignedBelow.ValidationPlan.EntryEsiUnsignedBelowGlobal == true
            && unsignedBelow.ValidationPlan.FirstSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.EsiUnsignedBelowDestinationRawFileOffset
            && unsignedBelow.ValidationPlan.Esi4ReadReached == false
            && unsignedBelow.ValidationOutcomeClean
            && unsignedBelow.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
            && unsignedBelowWithUnreachedEsi4.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ValidationBlocked
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.Reads.Length == 1
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.Reads[0].Kind
                == CapacityFinalCalleeFirstValidationReadKind.GlobalDword
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.GlobalReadReached
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.UnsignedBelowBranchReached
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.FirstSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.EsiUnsignedBelowDestinationRawFileOffset
            && !unsignedBelowWithUnreachedEsi4.ValidationPlan.Esi4ReadReached
            && unsignedBelowWithUnreachedEsi4.ValidationPlan.UnreachedEsi4DwordCaptureProvided
            && !unsignedBelowWithUnreachedEsi4.ValidationOutcomeClean
            && unsignedBelowWithUnreachedEsi4.UnreachedEsi4DwordCaptureProvided
            && tagBranch.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            && tagBranch.ValidationPlan.EqualOneBranchTaken == true
            && tagBranch.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.MaskEqualsOneDestinationRawFileOffset
            && tagBranch.ValidationOutcomeClean
            && tagFallthrough.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
            && tagFallthrough.ValidationPlan.EqualOneBranchTaken == false
            && tagFallthrough.ValidationPlan.FinalSelectedDestinationRawFileOffset
                == CapacityFinalCalleeFirstValidationProjection.MaskNotEqualOneFallthroughRawFileOffset
            && tagFallthrough.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeValidationLifecyclePlan wrapped =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                entryEcx: 5,
                priorTestMaskSet: false,
                globalDword: 0,
                esi4Dword: 1));
        check("Prefix LEA and validation ESI-plus-four arithmetic wrap as DWORDs",
            wrapped.PrefixPlan.DerivedPrecallESI == 0xFFFFFFFD
            && wrapped.ValidationPlan.Capture.EntryESI == 0xFFFFFFFD
            && wrapped.ValidationPlan.Esi4Address == 1
            && wrapped.ValidationPlan.Esi4DwordCaptured
            && wrapped.ValidationPlan.Esi4Dword == 1
            && wrapped.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne);

        CapacityFinalCalleeValidationLifecyclePlan missingGlobal =
            CapacityFinalCalleeValidationLifecycleProjection.Project(Capture(
                priorTestMaskSet: false,
                globalDword: null,
                esi4Dword: 0));
        check("A reached missing global read blocks while preserving the later ESI-plus-four capture as unreached",
            missingGlobal.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ValidationBlocked
            && missingGlobal.DeriveValidatedEntry
            && missingGlobal.ValidationPlan.Outcome
                == CapacityFinalCalleeFirstValidationOutcome.MissingGlobalDword
            && missingGlobal.ValidationPlan.GlobalReadReached
            && !missingGlobal.ValidationPlan.GlobalDwordCaptured
            && !missingGlobal.ValidationPlan.Esi4ReadReached
            && missingGlobal.UnreachedEsi4DwordCaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset);
    }

    private static CapacityFinalCalleeValidationLifecycleCapture Capture(
        bool prefixValidated = true,
        uint entryEcx = 0x12345678,
        bool? priorTestMaskSet = false,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        bool validationCaptureValidatedEntry = false,
        uint validationEntryEsi = 0xDEADBEEF,
        uint? globalDword = 0,
        uint? esi4Dword = 0x12345679,
        uint? expectedValidationEntryESI = null) =>
        new(
            Prefix: new(
                CaptureValidatedNonzeroEntry: prefixValidated,
                EntryECX: entryEcx,
                CapturedPriorTestMaskSet: priorTestMaskSet,
                CallCellTargetVirtualAddress: callTarget,
                CallReturnedNormally: callReturnedNormally),
            Validation: new(
                CaptureValidatedEntry: validationCaptureValidatedEntry,
                EntryESI: validationEntryEsi,
                GlobalDword: globalDword,
                Esi4Dword: esi4Dword),
            ExpectedValidationEntryESI: expectedValidationEntryESI);
}
