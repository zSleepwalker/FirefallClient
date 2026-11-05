using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityAboveLifecyclePlan zeroBranch =
            FrameHandoffCapacityAboveLifecycleProjection.Project(CompleteCapture(
                returnEsi: 0,
                returnFrameMinus4: 0x22222222));
        FrameHandoffCapacityAboveLifecyclePlan nonzeroBranch =
            FrameHandoffCapacityAboveLifecycleProjection.Project(CompleteCapture(
                returnEbx: 0x9000,
                returnEsi: 0x55,
                secondEntryEbx: 0x7000,
                secondEntryEdi: 0x8000,
                returnFrameMinus4: 0x33333333,
                secondFrameMinus4: 0x11111111));

        check("Both exact return-gate outcomes complete only after normal boundaries and clear captures",
            zeroBranch.Outcome == FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
            && zeroBranch.AboveTransferSuccessAdmission
            && zeroBranch.AboveTransferPlan.NormalBoundaryReached
            && zeroBranch.AboveSecondTransferSuccessAdmission
            && zeroBranch.AboveSecondTransferPlan?.NormalBoundaryReached == true
            && zeroBranch.ReturnGateSuccessAdmission
            && zeroBranch.AboveReturnGatePlan?.Outcome
                == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
            && zeroBranch.AboveTransferUnreachedCapturesClear
            && zeroBranch.AboveSecondTransferUnreachedCapturesClear
            && zeroBranch.ReturnGateUnreachedCapturesClear
            && zeroBranch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection.ZeroBranchDestinationRawFileOffset
            && nonzeroBranch.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
            && nonzeroBranch.AboveReturnGatePlan?.Outcome
                == FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough
            && nonzeroBranch.AboveReturnGatePlan?.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection
                    .NormalBoundaryRawFileOffsetExclusive
            && nonzeroBranch.ReturnGateSuccessAdmission
            && nonzeroBranch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection
                    .NormalBoundaryRawFileOffsetExclusive);

        check("Normal-entry validity is derived and post-opaque EBX/EDI/ESI captures remain independent",
            zeroBranch.AboveSecondTransferNormalEntryDerived
            && zeroBranch.AboveSecondTransferPlan?.Capture.ValidatedNormalEntry == true
            && zeroBranch.ReturnGateNormalEntryDerived
            && zeroBranch.AboveReturnGatePlan?.Capture.ValidatedNormalEntry == true
            && nonzeroBranch.Capture.AboveSecondTransfer.EntryEbx == 0x7000
            && nonzeroBranch.Capture.AboveSecondTransfer.EntryEdi == 0x8000
            && nonzeroBranch.Capture.AboveSecondTransfer.EntryEbx
                != nonzeroBranch.Capture.AboveReturnGate.EntryEbx
            && nonzeroBranch.Capture.AboveReturnGate.EntryEbx == 0x9000
            && nonzeroBranch.Capture.AboveReturnGate.EntryEsi == 0x55
            && nonzeroBranch.Capture.AboveReturnGate.EntryEsi
                != nonzeroBranch.Capture.AboveTransfer.EntryEsi
            && nonzeroBranch.AboveReturnGatePlan?.EdiAfterLea
                == unchecked(0x9000 + nonzeroBranch.Capture.AboveReturnGate.HelperReturnEax!.Value)
            && nonzeroBranch.AboveReturnGatePlan?.EcxAfterMove == 0x55
            && nonzeroBranch.AboveReturnGatePlan?.EdiAfterLea
                != nonzeroBranch.Capture.AboveSecondTransfer.EntryEdi);

        FrameHandoffCapacityAboveLifecyclePlan firstNonreturn =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(firstCallReturned: false));
        FrameHandoffCapacityAboveLifecyclePlan firstMissingCell =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(firstCallCell: null));
        FrameHandoffCapacityAboveLifecyclePlan unvalidatedFirstEntry =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(
                    firstEntryValidated: false,
                    secondEntryInitiallyValidated: true,
                    returnEntryInitiallyValidated: true));
        check("A nonreturning first call blocks later stages; a missing first read preserves its push and flags supplied captures",
            firstNonreturn.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveTransferBlocked
            && firstNonreturn.AboveTransferPlan.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.CallDidNotReturnNormally
            && firstNonreturn.AboveTransferPlan.CallReached
            && firstNonreturn.AboveSecondTransferPlan is null
            && firstNonreturn.AboveReturnGatePlan is null
            && firstNonreturn.UnreachedAboveSecondTransferReceiver4CaptureProvided
            && firstNonreturn.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
            && firstMissingCell.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveTransferHasUnreachedCaptures
            && firstMissingCell.AboveTransferPlan.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.MissingCallCellCapture
            && firstMissingCell.AboveTransferPlan.Pushes.Length == 1
            && firstMissingCell.AboveTransferPlan.Pushes[0].Source == "EAX"
            && firstMissingCell.AboveTransferPlan.UnreachedCallReturnCaptureProvided
            && firstMissingCell.AboveSecondTransferPlan is null
            && firstMissingCell.UnreachedAboveSecondTransferFrameMinusCCaptureProvided
            && firstMissingCell.UnreachedAboveSecondTransferCallReturnCaptureProvided
            && firstMissingCell.UnreachedAboveReturnGateFrameMinus4CaptureProvided
            && unvalidatedFirstEntry.AboveTransferPlan.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.EntryNotValidated
            && unvalidatedFirstEntry.AboveSecondTransferPlan is null
            && unvalidatedFirstEntry.AboveReturnGatePlan is null
            && !unvalidatedFirstEntry.AboveSecondTransferNormalEntryDerived
            && !unvalidatedFirstEntry.ReturnGateNormalEntryDerived
            && unvalidatedFirstEntry.UnreachedAboveSecondTransferReceiver4CaptureProvided
            && unvalidatedFirstEntry.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided);

        FrameHandoffCapacityAboveLifecyclePlan secondNonreturn =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(secondCallReturned: false));
        FrameHandoffCapacityAboveLifecyclePlan secondMissingRead =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(
                    secondReceiver4: null,
                    secondFrameMinusC: 0x44,
                    secondFrameMinus4: 0x55,
                    secondCallReturned: true));
        FrameHandoffCapacityAboveLifecyclePlan secondMissingFrameMinus4 =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(secondFrameMinus4: null));
        check("A nonreturning second call and a missing second-transfer read block return cleanup with later captures explicit",
            secondNonreturn.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveSecondTransferBlocked
            && secondNonreturn.AboveSecondTransferPlan?.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.CallDidNotReturnNormally
            && secondNonreturn.AboveSecondTransferPlan?.CallReached == true
            && secondNonreturn.AboveReturnGatePlan is null
            && secondNonreturn.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
            && secondNonreturn.UnreachedAboveReturnGateFrameMinus4CaptureProvided
            && secondMissingRead.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveSecondTransferHasUnreachedCaptures
            && secondMissingRead.AboveSecondTransferPlan?.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingReceiver4Capture
            && secondMissingRead.AboveSecondTransferPlan?.Pushes.IsEmpty == true
            && secondMissingRead.UnreachedAboveSecondTransferFrameMinusCCaptureProvided
            && secondMissingRead.UnreachedAboveSecondTransferFrameMinus4CaptureProvided
            && secondMissingRead.UnreachedAboveSecondTransferCallReturnCaptureProvided
            && secondMissingRead.AboveReturnGatePlan is null
            && secondMissingRead.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
            && secondMissingFrameMinus4.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveSecondTransferHasUnreachedCaptures
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinus4Capture
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.Reads.Length == 3
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.Pushes.Length == 2
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.Pushes[0].Source == "EBX"
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.Pushes[1].Source == "ECX"
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.FrameMinus4ReadReached == true
            && secondMissingFrameMinus4.AboveSecondTransferPlan?.FrameMinus4PushReached == false
            && secondMissingFrameMinus4.AboveReturnGatePlan is null
            && secondMissingFrameMinus4.UnreachedAboveReturnGateFrameMinus4CaptureProvided);

        FrameHandoffCapacityAboveLifecyclePlan frameMismatch =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(returnEbp: 0x4001));
        check("The shared EBP identity is checked before the return gate's 24-byte cleanup",
            frameMismatch.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.ReturnGateFrameIdentityMismatch
            && frameMismatch.AboveSecondTransferSuccessAdmission
            && frameMismatch.ReturnGateFrameEbpIdentityValidated
            && !frameMismatch.ReturnGateFrameEbpMatches
            && frameMismatch.AboveReturnGatePlan is null
            && frameMismatch.UnreachedAboveReturnGateHelperReturnEaxCaptureProvided
            && frameMismatch.UnreachedAboveReturnGateFrameMinus4CaptureProvided
            && frameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset);

        FrameHandoffCapacityAboveLifecyclePlan independentLoads =
            FrameHandoffCapacityAboveLifecycleProjection.Project(CompleteCapture(
                secondEntryEbp: 0xC,
                secondFrameMinus4: 0x11111111,
                returnEbp: 0xC,
                returnFrameMinus4: 0x22222222));
        check("Second-transfer and return-gate memory reads are independent fresh captures",
            independentLoads.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
            && independentLoads.AboveSecondTransferPlan?.Reads[2].Address == 8
            && independentLoads.AboveSecondTransferPlan?.Reads[2].Value == 0x11111111
            && independentLoads.AboveReturnGatePlan?.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveReturnGateRead(0x00BE6C95, 8, 0x22222222)
            }) == true
            && independentLoads.AboveSecondTransferPlan?.Reads[2].Value
                != independentLoads.AboveReturnGatePlan?.Reads[0].Value);

        FrameHandoffCapacityAboveLifecyclePlan missingReturnEax =
            FrameHandoffCapacityAboveLifecycleProjection.Project(
                CompleteCapture(returnHelperEax: null));
        check("Missing helper-return EAX retains the cleanup and stops before the LEA",
            missingReturnEax.Outcome
                == FrameHandoffCapacityAboveLifecycleOutcome.AboveReturnGateHasUnreachedCaptures
            && missingReturnEax.AboveReturnGatePlan?.StackCleanupReached == true
            && missingReturnEax.AboveReturnGatePlan?.EdiLeaReached == false
            && missingReturnEax.AboveReturnGatePlan?.HelperReturnEaxConsumed == false
            && missingReturnEax.AboveReturnGatePlan?.FrameMinus4ReadReached == false
            && missingReturnEax.UnreachedAboveReturnGateFrameMinus4CaptureProvided
            && missingReturnEax.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection.EdiLeaRawFileOffset);

        FrameHandoffCapacityAboveLifecyclePlan wrapped =
            FrameHandoffCapacityAboveLifecycleProjection.Project(CompleteCapture(
                entryEsi: 0x40000001,
                secondEntryEbx: uint.MaxValue,
                secondEntryEbp: 3,
                returnEbp: 3,
                returnEbx: uint.MaxValue,
                returnHelperEax: 2,
                returnEsi: 0,
                secondReceiver4: 0,
                secondFrameMinusC: 0x80000000,
                secondFrameMinus4: 1,
                returnFrameMinus4: 2));
        check("DWORD wrapping is preserved across stage addresses, arithmetic, and return LEA",
            wrapped.Outcome == FrameHandoffCapacityAboveLifecycleOutcome.ProjectionComplete
            && wrapped.AboveTransferPlan.EdiAfterLea == 4
            && wrapped.AboveSecondTransferPlan?.Receiver4ReadAddress == 3
            && wrapped.AboveSecondTransferPlan?.FrameMinusCReadAddress == 0xFFFFFFF7
            && wrapped.AboveSecondTransferPlan?.FrameMinus4ReadAddress == 0xFFFFFFFF
            && wrapped.AboveReturnGatePlan?.Reads[0].Address == 0xFFFFFFFF
            && wrapped.AboveReturnGatePlan?.EdiAfterLea == 1
            && wrapped.AboveReturnGatePlan?.Outcome
                == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken);
    }

    private static FrameHandoffCapacityAboveLifecycleCapture CompleteCapture(
        bool firstEntryValidated = true,
        uint entryEsi = 3,
        uint firstEntryEcx = 0x11,
        uint firstEntryEdx = 0x22,
        uint? firstCallCell = 0x1000,
        bool? firstCallReturned = true,
        bool secondEntryInitiallyValidated = false,
        uint secondEntryEbx = 0x1234,
        uint secondEntryEdi = 0x5678,
        uint secondEntryEbp = 0x4000,
        uint? secondReceiver4 = 1,
        uint? secondFrameMinusC = 0x80000000,
        uint? secondFrameMinus4 = 0x11111111,
        bool? secondCallReturned = true,
        bool returnEntryInitiallyValidated = false,
        uint? returnEbp = null,
        uint returnEbx = 0x9000,
        uint returnEsi = 0x55,
        uint? returnHelperEax = 0x100,
        uint? returnFrameMinus4 = 0x22222222) =>
        new(
            AboveTransfer: new(
                ValidatedBranchEntry: firstEntryValidated,
                EntryEsi: entryEsi,
                EntryEcx: firstEntryEcx,
                EntryEdx: firstEntryEdx,
                CallCellDword: firstCallCell,
                CallReturnedNormally: firstCallReturned),
            AboveSecondTransfer: new(
                ValidatedNormalEntry: secondEntryInitiallyValidated,
                EntryEbx: secondEntryEbx,
                EntryEdi: secondEntryEdi,
                EntryEbp: secondEntryEbp,
                Receiver4Dword: secondReceiver4,
                FrameMinusCDword: secondFrameMinusC,
                FrameMinus4Dword: secondFrameMinus4,
                CallReturnedNormally: secondCallReturned),
            AboveReturnGate: new(
                ValidatedNormalEntry: returnEntryInitiallyValidated,
                EntryEbp: returnEbp ?? secondEntryEbp,
                EntryEbx: returnEbx,
                EntryEsi: returnEsi,
                HelperReturnEax: returnHelperEax,
                FrameMinus4: returnFrameMinus4));
}
