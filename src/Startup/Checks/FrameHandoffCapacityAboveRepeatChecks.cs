using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveRepeatChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityAboveRepeatPlan zeroBranch =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                AboveReturnGate: Gate(entryEsi: 0, entryEbx: 0xFFFFFFFE, helperEax: 3),
                RepeatEntryEcx: null,
                RepeatEntryEdi: null,
                DirectionFlagSet: null,
                RepCompletedNormally: null));
        check("The exact zero branch bypasses REP and reaches CA2 without repeat captures",
            zeroBranch.Outcome == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
            && zeroBranch.GateSuccessAdmission
            && zeroBranch.GateUnreachedCapturesClear
            && zeroBranch.ZeroBranchBypassedRepeat
            && zeroBranch.SharedRepeatPlan is null
            && !zeroBranch.SharedRepeatNormalEntryDerived
            && !zeroBranch.UnreachedRepeatEntryEcxCaptureProvided
            && !zeroBranch.UnreachedRepeatEntryEdiCaptureProvided
            && !zeroBranch.UnreachedDirectionFlagCaptureProvided
            && !zeroBranch.UnreachedRepCompletionCaptureProvided
            && zeroBranch.StopAtRawFileOffsetExclusive == 0x00BE6CA2);

        FrameHandoffCapacityAboveRepeatPlan zeroBranchWithRepeatOnlyCaptures =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 0, entryEbx: 0x1000, helperEax: 0),
                RepeatEntryEcx: 5,
                RepeatEntryEdi: 0x2000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Zero-path repeat-only captures are rejected and marked unreached",
            zeroBranchWithRepeatOnlyCaptures.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchHasRepeatCaptures
            && zeroBranchWithRepeatOnlyCaptures.ZeroBranchBypassedRepeat
            && zeroBranchWithRepeatOnlyCaptures.SharedRepeatPlan is null
            && zeroBranchWithRepeatOnlyCaptures.UnreachedRepeatEntryEcxCaptureProvided
            && zeroBranchWithRepeatOnlyCaptures.UnreachedRepeatEntryEdiCaptureProvided
            && zeroBranchWithRepeatOnlyCaptures.UnreachedDirectionFlagCaptureProvided
            && zeroBranchWithRepeatOnlyCaptures.UnreachedRepCompletionCaptureProvided
            && zeroBranchWithRepeatOnlyCaptures.StopAtRawFileOffsetExclusive == 0x00BE6CA2);

        FrameHandoffCapacityAboveRepeatPlan forward =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 3, entryEbx: 0x1000, helperEax: 0),
                RepeatEntryEcx: 3,
                RepeatEntryEdi: 0x1000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan backward =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 3, entryEbx: 0x1000, helperEax: 0),
                RepeatEntryEcx: 3,
                RepeatEntryEdi: 0x1000,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan withoutOptionalIdentities =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 3, entryEbx: 0x1000, helperEax: 0),
                RepeatEntryEcx: null,
                RepeatEntryEdi: null,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Only the exact nonzero gate derives REP ECX and EDI, preserving either DF",
            IsComplete(forward)
            && forward.SharedRepeatPlan!.Value.Capture.EntryEcx == 3
            && forward.SharedRepeatPlan.Value.Capture.EntryEdi == 0x1000
            && forward.SharedRepeatPlan.Value.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, 4, 0)
            && forward.SharedRepeatPlan.Value.EdiAfterRep == 0x100C
            && backward.Outcome == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
            && backward.SharedRepeatPlan!.Value.DirectionFlagSet == true
            && backward.SharedRepeatPlan.Value.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, -4, 0)
            && backward.SharedRepeatPlan.Value.EdiAfterRep == 0x0FF4
            && IsComplete(withoutOptionalIdentities)
            && withoutOptionalIdentities.RepeatEcxIdentityMatches == true
            && withoutOptionalIdentities.RepeatEdiIdentityMatches == true
            && withoutOptionalIdentities.SharedRepeatPlan!.Value.Capture.EntryEcx == 3
            && withoutOptionalIdentities.SharedRepeatPlan.Value.Capture.EntryEdi == 0x1000);

        FrameHandoffCapacityAboveRepeatPlan zeroStart =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 1, entryEbx: 0, helperEax: 0),
                RepeatEntryEcx: 1,
                RepeatEntryEdi: 0,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan forwardWrap =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 1, entryEbx: 0xFFFFFFFE, helperEax: 0),
                RepeatEntryEcx: 1,
                RepeatEntryEdi: 0xFFFFFFFE,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan backwardWrap =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 1, entryEbx: 2, helperEax: 0),
                RepeatEntryEcx: 1,
                RepeatEntryEdi: 2,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan maximumCount =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: uint.MaxValue, entryEbx: 2, helperEax: 0),
                RepeatEntryEcx: uint.MaxValue,
                RepeatEntryEdi: 2,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Zero start and DWORD-wrapped repeat arithmetic preserve gate identities",
            IsComplete(zeroStart)
            && zeroStart.SharedRepeatPlan!.Value.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(1, 0, 4, 0)
            && zeroStart.SharedRepeatPlan.Value.EdiAfterRep == 4
            && IsComplete(forwardWrap)
            && forwardWrap.SharedRepeatPlan!.Value.EdiAfterRep == 2
            && IsComplete(backwardWrap)
            && backwardWrap.SharedRepeatPlan!.Value.EdiAfterRep == uint.MaxValue - 1
            && IsComplete(maximumCount)
            && maximumCount.SharedRepeatPlan!.Value.EcxAfterRep == 0
            && maximumCount.SharedRepeatPlan.Value.EdiAfterRep == 0xFFFFFFFE);

        FrameHandoffCapacityAboveRepeatPlan missingDirection =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 5, entryEbx: 0x4000, helperEax: 0),
                RepeatEntryEcx: 5,
                RepeatEntryEdi: 0x4000,
                DirectionFlagSet: null,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan missingCompletion =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 7, entryEbx: 0x5000, helperEax: 0),
                RepeatEntryEcx: 7,
                RepeatEntryEdi: 0x5000,
                DirectionFlagSet: false,
                RepCompletedNormally: null));
        FrameHandoffCapacityAboveRepeatPlan incompleteRep =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 9, entryEbx: 0x6000, helperEax: 0),
                RepeatEntryEcx: 9,
                RepeatEntryEdi: 0x6000,
                DirectionFlagSet: true,
                RepCompletedNormally: false));
        check("Partial REP plans remain available but cannot admit the CA2 completion boundary",
            missingDirection.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.SharedRepeatHasUnreachedCaptures
            && missingDirection.SharedRepeatPlan.HasValue
            && missingDirection.SharedRepeatPlan.Value.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.MissingDirectionFlagCapture
            && missingDirection.SharedRepeatPlan.Value.EaxValue == 0
            && !missingDirection.SharedRepeatCompletionAdmission
            && missingDirection.UnreachedRepCompletionCaptureProvided
            && missingDirection.StopAtRawFileOffsetExclusive == 0x00BE6CA0
            && missingCompletion.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.SharedRepeatIncomplete
            && missingCompletion.SharedRepeatPlan.HasValue
            && missingCompletion.SharedRepeatPlan.Value.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.MissingRepCompletionCapture
            && !missingCompletion.SharedRepeatCompletionAdmission
            && missingCompletion.StopAtRawFileOffsetExclusive == 0x00BE6CA0
            && incompleteRep.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.SharedRepeatIncomplete
            && incompleteRep.SharedRepeatPlan.HasValue
            && incompleteRep.SharedRepeatPlan.Value.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally
            && !incompleteRep.SharedRepeatPlan.Value.AllStoresCompleted
            && !incompleteRep.SharedRepeatCompletionAdmission);

        FrameHandoffCapacityAboveRepeatPlan missingCallEax =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                AboveReturnGate: new(
                    ValidatedNormalEntry: true,
                    EntryEbp: 0x20,
                    EntryEbx: 0x1000,
                    EntryEsi: 4,
                    HelperReturnEax: null,
                    FrameMinus4: 0xAABBCCDD),
                RepeatEntryEcx: 4,
                RepeatEntryEdi: 0x1000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityAboveRepeatPlan missingOldRead =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                AboveReturnGate: new(
                    ValidatedNormalEntry: true,
                    EntryEbp: 0x20,
                    EntryEbx: 0x1000,
                    EntryEsi: 4,
                    HelperReturnEax: 0,
                    FrameMinus4: null),
                RepeatEntryEcx: 4,
                RepeatEntryEdi: 0x1000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Missing helper EAX or old frame read blocks REP and marks every later capture unreached",
            missingCallEax.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.GateHasUnreachedCaptures
            && missingCallEax.AboveReturnGatePlan.StackCleanupReached
            && !missingCallEax.AboveReturnGatePlan.EdiLeaReached
            && missingCallEax.SharedRepeatPlan is null
            && missingCallEax.UnreachedGateFrameMinus4CaptureProvided
            && missingCallEax.UnreachedRepeatEntryEcxCaptureProvided
            && missingCallEax.UnreachedRepeatEntryEdiCaptureProvided
            && missingCallEax.UnreachedDirectionFlagCaptureProvided
            && missingCallEax.UnreachedRepCompletionCaptureProvided
            && missingCallEax.StopAtRawFileOffsetExclusive == 0x00BE6C92
            && missingOldRead.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.GateHasUnreachedCaptures
            && missingOldRead.AboveReturnGatePlan.EdiLeaReached
            && missingOldRead.AboveReturnGatePlan.EdiAfterLea == 0x1000
            && !missingOldRead.AboveReturnGatePlan.TestReached
            && missingOldRead.SharedRepeatPlan is null
            && missingOldRead.UnreachedRepeatEntryEcxCaptureProvided
            && missingOldRead.UnreachedRepeatEntryEdiCaptureProvided
            && missingOldRead.UnreachedDirectionFlagCaptureProvided
            && missingOldRead.UnreachedRepCompletionCaptureProvided
            && missingOldRead.StopAtRawFileOffsetExclusive == 0x00BE6C95);

        FrameHandoffCapacityAboveRepeatPlan identityMismatch =
            FrameHandoffCapacityAboveRepeatProjection.Project(new(
                Gate(entryEsi: 3, entryEbx: 0x1000, helperEax: 0),
                RepeatEntryEcx: 4,
                RepeatEntryEdi: null,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        check("Mismatched repeat identities stop before XOR and leave DF/completion captures unreached",
            identityMismatch.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.RepeatEntryIdentityMismatch
            && identityMismatch.RepeatEntryIdentityChecked
            && identityMismatch.RepeatEcxIdentityMatches == false
            && identityMismatch.RepeatEdiIdentityMatches == true
            && identityMismatch.SharedRepeatPlan is null
            && !identityMismatch.SharedRepeatNormalEntryDerived
            && identityMismatch.UnreachedDirectionFlagCaptureProvided
            && identityMismatch.UnreachedRepCompletionCaptureProvided
            && identityMismatch.StopAtRawFileOffsetExclusive == 0x00BE6C9E);
    }

    private static FrameHandoffCapacityAboveReturnGateCapture Gate(
        uint entryEsi,
        uint entryEbx,
        uint helperEax) =>
        new(
            ValidatedNormalEntry: true,
            EntryEbp: 0x20,
            EntryEbx: entryEbx,
            EntryEsi: entryEsi,
            HelperReturnEax: helperEax,
            FrameMinus4: 0xAABBCCDD);

    private static bool IsComplete(FrameHandoffCapacityAboveRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
        && plan.GateSuccessAdmission
        && plan.GateUnreachedCapturesClear
        && plan.RepeatEntryIdentityChecked
        && plan.RepeatEcxIdentityMatches == true
        && plan.RepeatEdiIdentityMatches == true
        && plan.SharedRepeatNormalEntryDerived
        && plan.SharedRepeatCompletionAdmission
        && plan.SharedRepeatUnreachedCapturesClear
        && plan.SharedRepeatPlan is { } repeatPlan
        && repeatPlan.Outcome == FrameHandoffCapacitySharedRepeatOutcome.Completed
        && repeatPlan.AllStoresCompleted
        && repeatPlan.StoreDescriptor.HasValue
        && !repeatPlan.UnreachedDirectionFlagCaptureProvided
        && !repeatPlan.UnreachedRepCompletionCaptureProvided
        && !plan.UnreachedRepeatEntryEcxCaptureProvided
        && !plan.UnreachedRepeatEntryEdiCaptureProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveRepeatProjection.EndRawFileOffsetExclusive;
}
