using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFillSharedRepeatChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The fill join's nonzero jump is exactly the shared-repeat entry on the same pinned sample",
            FrameHandoffCapacityFillJoinProjection.SamplePath
                == FrameHandoffCapacitySharedRepeatProjection.SamplePath
            && FrameHandoffCapacityFillJoinProjection.SampleSha256
                == FrameHandoffCapacitySharedRepeatProjection.SampleSha256
            && FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
                == FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset
            && FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
                == FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive
            && FrameHandoffCapacitySharedRepeatProjection.XorEaxRawFileOffset
                == FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset
            && FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset
                == FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset + 2
            && FrameHandoffCapacityFillSharedRepeatProjection.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive);

        FrameHandoffCapacityFillSharedRepeatPlan zeroRoute =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x1000, 0, 0xAABBCCDD),
                ExpectedRepeatEntryEcx: 9,
                ExpectedRepeatEntryEdi: 0x7000,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        check("The exact zero-entry-EDI branch reaches CA2 and rejects every supplied repeat capture",
            zeroRoute.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroRoute.FillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroRoute.FillJoinPlan.ZeroBranchTaken == true
            && zeroRoute.FillJoinPlan.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && !zeroRoute.RepeatEntryIdentityValidationReached
            && zeroRoute.RepeatEntryEcx is null
            && zeroRoute.RepeatEntryEdi is null
            && zeroRoute.ExpectedRepeatEntryEcxMatched is null
            && zeroRoute.ExpectedRepeatEntryEdiMatched is null
            && !zeroRoute.RepeatEntryValidatedNonzero
            && zeroRoute.RepeatPlan is null
            && zeroRoute.UnreachedExpectedRepeatEntryEcxCaptureProvided
            && zeroRoute.UnreachedExpectedRepeatEntryEdiCaptureProvided
            && zeroRoute.UnreachedDirectionFlagCaptureProvided
            && zeroRoute.UnreachedRepCompletionCaptureProvided
            && zeroRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset);

        FrameHandoffCapacityFillSharedRepeatPlan forward =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x2000, 0x1000, 0x3000),
                ExpectedRepeatEntryEcx: 0x1000,
                ExpectedRepeatEntryEdi: 0x3000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityFillSharedRepeatPlan backward =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x4000, 0x1001, 0x5000),
                ExpectedRepeatEntryEcx: 0x1001,
                ExpectedRepeatEntryEdi: 0x5000,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        check("The exact nonzero join supplies ECX from entry EDI and EDI from this frame's [EBP-4] for either DF",
            IsComplete(forward)
            && forward.FillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome
                    .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && forward.FillJoinPlan.ZeroBranchTaken == false
            && forward.RepeatEntryValidatedNonzero
            && forward.RepeatEntryIdentityValidationReached
            && forward.RepeatEntryEcx == 0x1000
            && forward.RepeatEntryEdi == 0x3000
            && forward.ExpectedRepeatEntryEcxMatched == true
            && forward.ExpectedRepeatEntryEdiMatched == true
            && forward.RepeatPlan?.Capture.EntryEcx == forward.FillJoinPlan.EcxAfterMove
            && forward.RepeatPlan?.Capture.EntryEdi == forward.FillJoinPlan.EdiAfterMove
            && forward.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(0x1000, 0x3000, 4, 0)
            && forward.RepeatPlan?.EdiAfterRep == 0x7000
            && IsComplete(backward)
            && backward.RepeatEntryEcx == 0x1001
            && backward.RepeatEntryEdi == 0x5000
            && backward.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(0x1001, 0x5000, -4, 0)
            && backward.RepeatPlan?.EdiAfterRep == 0x0FFC);

        FrameHandoffCapacityFillSharedRepeatPlan missingFrameRead =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x6004, 0x2000, null),
                ExpectedRepeatEntryEcx: 0x2000,
                ExpectedRepeatEntryEdi: 0xA000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("A missing fresh frame-minus-four read preserves the partial join and leaves all repeat captures unreached",
            missingFrameRead.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.MissingFrameMinus4Read
            && missingFrameRead.FillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome.MissingFrameMinus4Read
            && missingFrameRead.FillJoinPlan.FrameMinus4ReadAddress == 0x6000
            && missingFrameRead.FillJoinPlan.FrameMinus4ReadReached
            && !missingFrameRead.FillJoinPlan.FrameMinus4ReadCaptured
            && !missingFrameRead.FillJoinPlan.EntryEdiTestReached
            && !missingFrameRead.RepeatEntryIdentityValidationReached
            && missingFrameRead.RepeatPlan is null
            && missingFrameRead.UnreachedExpectedRepeatEntryEcxCaptureProvided
            && missingFrameRead.UnreachedExpectedRepeatEntryEdiCaptureProvided
            && missingFrameRead.UnreachedDirectionFlagCaptureProvided
            && missingFrameRead.UnreachedRepCompletionCaptureProvided
            && missingFrameRead.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillSharedRepeatPlan unvalidatedJoin =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(false, 0x6800, 0x2000, 0xA000),
                ExpectedRepeatEntryEcx: 0x2000,
                ExpectedRepeatEntryEdi: 0xA000,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        check("An unvalidated join rejects its supplied frame read and every later repeat capture",
            unvalidatedJoin.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.FillJoinNotValidated
            && unvalidatedJoin.FillJoinPlan.Outcome
                == FrameHandoffCapacityFillJoinOutcome.JoinNotValidated
            && !unvalidatedJoin.FillJoinPlan.FrameMinus4ReadReached
            && !unvalidatedJoin.FillJoinPlan.FrameMinus4ReadCaptured
            && unvalidatedJoin.FillJoinPlan.UnreachedFrameMinus4ReadCaptureProvided
            && !unvalidatedJoin.RepeatEntryIdentityValidationReached
            && unvalidatedJoin.RepeatPlan is null
            && unvalidatedJoin.UnreachedExpectedRepeatEntryEcxCaptureProvided
            && unvalidatedJoin.UnreachedExpectedRepeatEntryEdiCaptureProvided
            && unvalidatedJoin.UnreachedDirectionFlagCaptureProvided
            && unvalidatedJoin.UnreachedRepCompletionCaptureProvided
            && unvalidatedJoin.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillSharedRepeatPlan identityMismatch =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x7000, 5, 0x1234),
                ExpectedRepeatEntryEcx: 6,
                ExpectedRepeatEntryEdi: 0x1234,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Optional repeat identities are compared against join-derived registers before XOR",
            identityMismatch.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.RepeatEntryIdentityMismatch
            && identityMismatch.RepeatEntryIdentityValidationReached
            && identityMismatch.RepeatEntryEcx == 5
            && identityMismatch.RepeatEntryEdi == 0x1234
            && identityMismatch.ExpectedRepeatEntryEcxMatched == false
            && identityMismatch.ExpectedRepeatEntryEdiMatched == true
            && !identityMismatch.RepeatEntryValidatedNonzero
            && identityMismatch.RepeatPlan is null
            && identityMismatch.UnreachedDirectionFlagCaptureProvided
            && identityMismatch.UnreachedRepCompletionCaptureProvided
            && identityMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillSharedRepeatPlan zeroStartAddress =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x8000, 1, 0),
                ExpectedRepeatEntryEdi: 0,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("A zero fresh frame DWORD is accepted as the repeat start address on the nonzero-count route",
            IsComplete(zeroStartAddress)
            && zeroStartAddress.RepeatEntryEcx == 1
            && zeroStartAddress.RepeatEntryEdi == 0
            && zeroStartAddress.ExpectedRepeatEntryEcxMatched is null
            && zeroStartAddress.ExpectedRepeatEntryEdiMatched == true
            && zeroStartAddress.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(1, 0, 4, 0)
            && zeroStartAddress.RepeatPlan?.EdiAfterRep == 4);

        FrameHandoffCapacityFillSharedRepeatPlan missingDirection =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0x9000, 4, 0x1000),
                DirectionFlagSet: null,
                RepCompletedNormally: true));
        FrameHandoffCapacityFillSharedRepeatPlan missingCompletion =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0xA000, 7, 0x2000),
                DirectionFlagSet: true,
                RepCompletedNormally: null));
        FrameHandoffCapacityFillSharedRepeatPlan partialRep =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0xB000, 9, 0x3000),
                DirectionFlagSet: false,
                RepCompletedNormally: false));
        check("Missing DF, missing completion, and partial REP preserve only their reached effects",
            missingDirection.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.MissingDirectionFlagCapture
            && missingDirection.RepeatEntryValidatedNonzero
            && missingDirection.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan noDf
            && noDf.XorEaxReached
            && noDf.EaxValue == 0
            && noDf.RepInstructionReached
            && noDf.DirectionFlagSet is null
            && !noDf.AllStoresCompleted
            && noDf.StoreDescriptor is null
            && noDf.UnreachedRepCompletionCaptureProvided
            && missingDirection.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset
            && missingCompletion.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.MissingRepCompletionCapture
            && missingCompletion.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan noCompletion
            && noCompletion.XorEaxReached
            && noCompletion.DirectionFlagSet == true
            && !noCompletion.AllStoresCompleted
            && noCompletion.StoreDescriptor is null
            && partialRep.Outcome
                == FrameHandoffCapacityFillSharedRepeatOutcome.RepDidNotCompleteNormally
            && partialRep.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan incomplete
            && incomplete.XorEaxReached
            && incomplete.DirectionFlagSet == false
            && !incomplete.AllStoresCompleted
            && incomplete.StoreDescriptor is null
            && incomplete.EcxAfterRep is null
            && incomplete.EdiAfterRep is null
            && partialRep.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset);

        FrameHandoffCapacityFillSharedRepeatPlan optionalIdentities =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0xC000, 0xF0000000, 0xFFFFFFFE),
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacityFillSharedRepeatPlan backwardWrap =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 0xD000, 1, 2),
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        FrameHandoffCapacityFillSharedRepeatPlan wrappedFrameAddress =
            FrameHandoffCapacityFillSharedRepeatProjection.Project(new(
                new(true, 2, 1, uint.MaxValue),
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        check("Optional identities may be absent and composed repeat arithmetic plus the fresh frame address wrap as DWORDs",
            IsComplete(optionalIdentities)
            && optionalIdentities.ExpectedRepeatEntryEcxMatched is null
            && optionalIdentities.ExpectedRepeatEntryEdiMatched is null
            && optionalIdentities.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(
                    0xF0000000, 0xFFFFFFFE, 4, 0)
            && optionalIdentities.RepeatPlan?.EdiAfterRep == 0xBFFFFFFE
            && IsComplete(backwardWrap)
            && backwardWrap.RepeatPlan?.EdiAfterRep == 0xFFFFFFFE
            && IsComplete(wrappedFrameAddress)
            && wrappedFrameAddress.FillJoinPlan.FrameMinus4ReadAddress
                == 0xFFFFFFFE
            && wrappedFrameAddress.RepeatEntryEcx == 1
            && wrappedFrameAddress.RepeatEntryEdi == uint.MaxValue
            && wrappedFrameAddress.RepeatPlan?.EdiAfterRep == 3);
    }

    private static bool IsComplete(FrameHandoffCapacityFillSharedRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacityFillSharedRepeatOutcome.Completed
        && plan.RepeatEntryIdentityValidationReached
        && plan.RepeatEntryValidatedNonzero
        && plan.RepeatPlan is FrameHandoffCapacitySharedRepeatPlan repeatPlan
        && repeatPlan.Outcome == FrameHandoffCapacitySharedRepeatOutcome.Completed
        && repeatPlan.EntryValidationAccepted
        && repeatPlan.XorEaxReached
        && repeatPlan.EaxValue == 0
        && repeatPlan.RepInstructionReached
        && repeatPlan.AllStoresCompleted
        && repeatPlan.StoreDescriptor.HasValue
        && repeatPlan.EcxAfterRep == 0
        && !plan.UnreachedExpectedRepeatEntryEcxCaptureProvided
        && !plan.UnreachedExpectedRepeatEntryEdiCaptureProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive;
}
