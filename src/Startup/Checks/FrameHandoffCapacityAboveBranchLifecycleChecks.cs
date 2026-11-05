using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveBranchLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityAboveBranchLifecyclePlan aboveZeroFinalZero =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(CreateCapture());
        FrameHandoffCapacityAboveBranchLifecyclePlan aboveZeroFinalCall =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    finalReturnEdi: 0xD000,
                    finalReturnEsi: 17,
                    finalReturnEbx: 0x11223344,
                    receiver8Old: 0xFFFFFFFE));
        FrameHandoffCapacityAboveBranchLifecyclePlan aboveNonzeroFinalZero =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(aboveEntryEsi: 3));
        FrameHandoffCapacityAboveBranchLifecyclePlan aboveNonzeroFinalCall =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    aboveEntryEsi: 3,
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    finalReturnEdi: 0xE000,
                    finalReturnEsi: 0xFFFFFFFE,
                    finalReturnEbx: 0x55667788,
                    receiver8Old: 1));
        check("Both admitted above-gate branches compose through final bypass and returned-call paths",
            IsComplete(aboveZeroFinalZero)
            && aboveZeroFinalZero.AboveLifecyclePlan.AboveReturnGatePlan?.Outcome
                == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
            && aboveZeroFinalZero.AboveFinalLifecyclePlan?.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ZeroBranchTaken
            && aboveZeroFinalZero.AboveFinalLifecyclePlan?.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && IsComplete(aboveZeroFinalCall)
            && aboveZeroFinalCall.AboveFinalLifecyclePlan?.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && aboveZeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEdi == 0xD000
            && aboveZeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEsi == 17
            && aboveZeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEbx == 0x11223344
            && aboveZeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Receiver8ValueAfterAdd == 15
            && IsComplete(aboveNonzeroFinalZero)
            && aboveNonzeroFinalZero.AboveFinalLifecyclePlan?.AboveRepeatPlan.Outcome
                == FrameHandoffCapacityAboveRepeatOutcome.ProjectionComplete
            && aboveNonzeroFinalZero.AboveFinalLifecyclePlan?.AboveRepeatPlan
                .SharedRepeatPlan?.AllStoresCompleted == true
            && aboveNonzeroFinalZero.AboveFinalLifecyclePlan?.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && IsComplete(aboveNonzeroFinalCall)
            && aboveNonzeroFinalCall.AboveFinalLifecyclePlan?.FinalGatePlan?.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && aboveNonzeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEsi == 0xFFFFFFFE
            && aboveNonzeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEbx == 0x55667788
            && aboveNonzeroFinalCall.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Receiver8ValueAfterAdd == uint.MaxValue);

        check("The admitted return-gate capture overrides duplicate forged tail evidence",
            !aboveZeroFinalZero.SuppliedAboveRepeatReturnGateMatchesDerived.GetValueOrDefault()
            && aboveZeroFinalZero.DerivedAboveFinalLifecycleCapture?.AboveRepeat
                .AboveReturnGate
                == aboveZeroFinalZero.AboveLifecyclePlan.AboveReturnGatePlan?.Capture
            && aboveNonzeroFinalZero.DerivedAboveFinalLifecycleCapture?.AboveRepeat
                .AboveReturnGate
                == aboveNonzeroFinalZero.AboveLifecyclePlan.AboveReturnGatePlan?.Capture
            && aboveZeroFinalZero.AboveFinalLifecyclePlan?.AboveRepeatPlan.Capture
                .AboveReturnGate.ValidatedNormalEntry == true
            && aboveZeroFinalZero.AboveFinalLifecyclePlan?.AboveRepeatPlan.Capture
                .AboveReturnGate.EntryEbp
                == aboveZeroFinalZero.AboveLifecyclePlan.AboveReturnGatePlan?.Capture.EntryEbp
            && aboveZeroFinalZero.Capture.AboveRepeat.AboveReturnGate.EntryEbp
                == 0xDEAD0000);

        FrameHandoffCapacityAboveBranchLifecyclePlan firstNonreturn =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    firstCallReturned: false,
                    repeatEntryEcx: 0xA,
                    repeatEntryEdi: 0xB,
                    directionFlagSet: true,
                    repCompletedNormally: true,
                    finalGateFrameMinus14: 0xC,
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    receiver8Old: 0xD));
        FrameHandoffCapacityAboveBranchLifecyclePlan firstMissingCell =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    firstCallCell: null,
                    repeatEntryEcx: 0xA,
                    repeatEntryEdi: 0xB,
                    directionFlagSet: true,
                    repCompletedNormally: true));
        FrameHandoffCapacityAboveBranchLifecyclePlan secondNonreturn =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    secondCallReturned: false,
                    repeatEntryEcx: 0xA,
                    repeatEntryEdi: 0xB,
                    directionFlagSet: true,
                    repCompletedNormally: true,
                    finalGateFrameMinus14: 0xC,
                    finalGateReceiver4: 1,
                    finalCallReturned: true,
                    receiver8Old: 0xD));
        FrameHandoffCapacityAboveBranchLifecyclePlan secondMissingReceiver =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(secondReceiver4: null));
        FrameHandoffCapacityAboveBranchLifecyclePlan secondMissingFrameMinus4 =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(secondFrameMinus4: null));
        check("Failed or incomplete above-transfer calls retain earlier pushes and prevent all later stages",
            firstNonreturn.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && firstNonreturn.AboveLifecyclePlan.AboveTransferPlan.CallReached
            && firstNonreturn.AboveLifecyclePlan.AboveTransferPlan.Pushes.Length == 3
            && firstNonreturn.AboveLifecyclePlan.AboveTransferPlan.CallReturnedNormally == false
            && firstNonreturn.AboveLifecyclePlan.AboveSecondTransferPlan is null
            && firstNonreturn.AboveFinalLifecyclePlan is null
            && firstNonreturn.UnreachedAboveRepeatEntryEcxCaptureProvided
            && firstNonreturn.UnreachedAboveRepeatEntryEdiCaptureProvided
            && firstNonreturn.UnreachedAboveRepeatDirectionFlagCaptureProvided
            && firstNonreturn.UnreachedAboveRepeatRepCompletionCaptureProvided
            && firstNonreturn.UnreachedFinalGateReceiver4CaptureProvided
            && firstNonreturn.UnreachedFinalGateNormalCallStatusProvided
            && firstNonreturn.UnreachedFinalGateEntryEbpIdentityCaptureProvided
            && firstNonreturn.UnreachedFinalReturnEntryEbpCaptureProvided
            && firstNonreturn.UnreachedFinalReturnEntryEdiCaptureProvided
            && firstNonreturn.UnreachedFinalReturnEntryEbxCaptureProvided
            && firstNonreturn.UnreachedFinalReturnEntryEsiCaptureProvided
            && firstNonreturn.UnreachedFinalReturnReceiver8OldCaptureProvided
            && firstMissingCell.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && firstMissingCell.AboveLifecyclePlan.AboveTransferPlan.Pushes.Length == 1
            && firstMissingCell.AboveLifecyclePlan.AboveTransferPlan.Pushes[0].Source == "EAX"
            && firstMissingCell.AboveLifecyclePlan.AboveTransferPlan
                .UnreachedCallReturnCaptureProvided
            && firstMissingCell.AboveLifecyclePlan.AboveSecondTransferPlan is null
            && firstMissingCell.UnreachedAboveRepeatRepCompletionCaptureProvided
            && secondNonreturn.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && secondNonreturn.AboveLifecyclePlan.AboveSecondTransferPlan?.CallReached == true
            && secondNonreturn.AboveLifecyclePlan.AboveSecondTransferPlan?.Pushes.Length == 3
            && secondNonreturn.AboveLifecyclePlan.AboveSecondTransferPlan
                ?.CallReturnedNormally == false
            && secondNonreturn.AboveLifecyclePlan.AboveReturnGatePlan is null
            && secondNonreturn.AboveFinalLifecyclePlan is null
            && secondNonreturn.UnreachedFinalGateFrameMinus14CaptureProvided
            && secondMissingReceiver.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && secondMissingReceiver.AboveLifecyclePlan.AboveSecondTransferPlan?.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingReceiver4Capture
            && secondMissingReceiver.AboveLifecyclePlan.AboveSecondTransferPlan?.Pushes.IsEmpty
                == true
            && secondMissingReceiver.AboveLifecyclePlan.AboveReturnGatePlan is null
            && secondMissingFrameMinus4.AboveLifecyclePlan.AboveSecondTransferPlan?.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinus4Capture
            && secondMissingFrameMinus4.AboveLifecyclePlan.AboveSecondTransferPlan?.Pushes
                .Length == 2
            && secondMissingFrameMinus4.AboveLifecyclePlan.AboveSecondTransferPlan
                ?.FrameMinus4ReadReached == true
            && secondMissingFrameMinus4.AboveLifecyclePlan.AboveSecondTransferPlan
                ?.FrameMinus4PushReached == false);

        FrameHandoffCapacityAboveBranchLifecyclePlan missingReturnRead =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(returnFrameMinus4: null));
        FrameHandoffCapacityAboveBranchLifecyclePlan frameMismatch =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(returnEntryEbp: 0x4001));
        check("The return-gate cleanup is retained, but a missing read or frame mismatch cannot enter REP",
            missingReturnRead.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && missingReturnRead.AboveLifecyclePlan.AboveReturnGatePlan
                ?.StackCleanupReached == true
            && missingReturnRead.AboveLifecyclePlan.AboveReturnGatePlan
                ?.FrameMinus4ReadReached == true
            && missingReturnRead.AboveLifecyclePlan.AboveReturnGatePlan
                ?.FrameMinus4Captured == false
            && missingReturnRead.AboveFinalLifecyclePlan is null
            && missingReturnRead.UnreachedFinalGateFrameMinus14CaptureProvided
            && frameMismatch.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveLifecycleHasUnreachedCaptures
            && frameMismatch.AboveLifecyclePlan.AboveSecondTransferSuccessAdmission
            && frameMismatch.AboveLifecyclePlan.ReturnGateFrameEbpIdentityValidated
            && !frameMismatch.AboveLifecyclePlan.ReturnGateFrameEbpMatches
            && frameMismatch.AboveLifecyclePlan.AboveReturnGatePlan is null
            && frameMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && frameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset);

        FrameHandoffCapacityAboveBranchLifecyclePlan partialRep =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(aboveEntryEsi: 3, repCompletedNormally: false,
                    finalCallReturned: true));
        check("An incomplete REP preserves the admitted transfer and REP evidence while blocking final reads",
            partialRep.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveFinalLifecycleHasUnreachedCaptures
            && partialRep.AboveLifecycleAdmissionSatisfied
            && partialRep.AboveFinalLifecyclePlan?.AboveRepeatPlan.SharedRepeatPlan?.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally
            && partialRep.AboveFinalLifecyclePlan?.AboveRepeatPlan.SharedRepeatPlan
                ?.RepInstructionReached == true
            && partialRep.AboveFinalLifecyclePlan?.AboveRepeatPlan.SharedRepeatPlan
                ?.DirectionFlagSet == false
            && partialRep.AboveFinalLifecyclePlan?.FinalGatePlan is null
            && partialRep.UnreachedFinalGateFrameMinus14CaptureProvided
            && partialRep.UnreachedFinalGateReceiver4CaptureProvided
            && partialRep.UnreachedFinalGateNormalCallStatusProvided
            && partialRep.UnreachedFinalGateEntryEbpIdentityCaptureProvided
            && partialRep.UnreachedFinalReturnEntryEbpCaptureProvided
            && partialRep.UnreachedFinalReturnEntryEdiCaptureProvided
            && partialRep.UnreachedFinalReturnEntryEbxCaptureProvided
            && partialRep.UnreachedFinalReturnEntryEsiCaptureProvided
            && partialRep.UnreachedFinalReturnReceiver8OldCaptureProvided
            && partialRep.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset);

        FrameHandoffCapacityAboveBranchLifecyclePlan gateFrameMismatch =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    finalGateEbp: 0x2018,
                    finalGateFrameMinus14: 0x7000,
                    finalGateReceiver4: 0,
                    receiver8Old: 0x1234));
        FrameHandoffCapacityAboveBranchLifecyclePlan finalFrameMismatch =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(finalReturnEbp: 0x9999));
        FrameHandoffCapacityAboveBranchLifecyclePlan finalEbxMismatch =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(finalReturnEbx: 0xAABBCCDE, receiver8Old: 0x1234));
        FrameHandoffCapacityAboveBranchLifecyclePlan finalEsiMismatch =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(finalReturnEsi: 1, receiver8Old: 0x1234));
        check("Frame identity and zero-call EBX/ESI continuity fail before the final receiver8 read",
            gateFrameMismatch.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome
                    .AboveFinalLifecycleHasUnreachedCaptures
            && gateFrameMismatch.AboveFinalLifecyclePlan?.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalGateEntryEbpMismatch
            && gateFrameMismatch.AboveFinalLifecyclePlan?.FinalGatePlan is null
            && gateFrameMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && gateFrameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset
            && finalFrameMismatch.AboveFinalLifecyclePlan?.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalEntryEbpMismatch
            && finalFrameMismatch.AboveFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan is null
            && finalFrameMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && finalFrameMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset
            && finalEbxMismatch.AboveFinalLifecyclePlan?.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEbxMismatch
            && finalEbxMismatch.AboveFinalLifecyclePlan?.FinalGatePlan?.Reads.Length == 2
            && finalEbxMismatch.AboveFinalLifecyclePlan?.FinalLifecyclePlan is null
            && finalEbxMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided
            && finalEsiMismatch.AboveFinalLifecyclePlan?.Outcome
                == FrameHandoffCapacityAboveFinalLifecycleOutcome.FinalBypassEsiMismatch
            && finalEsiMismatch.AboveFinalLifecyclePlan?.FinalLifecyclePlan is null
            && finalEsiMismatch.UnreachedFinalReturnReceiver8OldCaptureProvided);

        FrameHandoffCapacityAboveBranchLifecyclePlan wrapped =
            FrameHandoffCapacityAboveBranchLifecycleProjection.Project(
                CreateCapture(
                    secondEntryEbp: 3,
                    returnEntryEbp: 3,
                    finalGateEbp: 3,
                    aboveEntryEsi: 0,
                    secondEntryEbx: uint.MaxValue,
                    secondEntryEdi: 0x80000000,
                    secondReceiver4: 0,
                    secondFrameMinusC: 0x80000000,
                    secondFrameMinus4: 1,
                    returnEbx: uint.MaxValue,
                    returnHelperEax: 2,
                    returnFrameMinus4: 2,
                    finalReturnEbx: 2,
                    finalReturnEsi: 0,
                    finalReturnEdi: uint.MaxValue - 1,
                    finalReturnEbp: 3,
                    finalGateFrameMinus14: uint.MaxValue - 1));
        check("DWORD wrapping is preserved across derived gate, stack, and final return addresses",
            IsComplete(wrapped)
            && wrapped.AboveLifecyclePlan.AboveSecondTransferPlan?.Receiver4ReadAddress == 3
            && wrapped.AboveLifecyclePlan.AboveSecondTransferPlan?.FrameMinusCReadAddress
                == 0xFFFFFFF7
            && wrapped.AboveLifecyclePlan.AboveSecondTransferPlan?.FrameMinus4ReadAddress
                == 0xFFFFFFFF
            && wrapped.AboveLifecyclePlan.AboveReturnGatePlan?.Reads[0].Address
                == 0xFFFFFFFF
            && wrapped.AboveLifecyclePlan.AboveReturnGatePlan?.EdiAfterLea == 1
            && wrapped.AboveFinalLifecyclePlan?.FinalGatePlan?.Reads[0].Address
                == 0xFFFFFFEF
            && wrapped.AboveFinalLifecyclePlan?.FinalGatePlan?.Reads[1].Address == 2
            && wrapped.AboveFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan
                ?.Receiver8ValueAfterAdd == 0xFFFFFFFC
            && wrapped.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityAboveBranchLifecycleProjection
                    .LifecycleEndRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityAboveBranchLifecycleCapture CreateCapture(
        bool firstEntryValidated = true,
        uint secondEntryEbp = 0x4000,
        uint? returnEntryEbp = null,
        uint aboveEntryEsi = 0,
        uint aboveEntryEcx = 0x11,
        uint aboveEntryEdx = 0x22,
        uint? firstCallCell = 0x1000,
        bool? firstCallReturned = true,
        uint secondEntryEbx = 0x3000,
        uint secondEntryEdi = 0x4000,
        uint? secondReceiver4 = 1,
        uint? secondFrameMinusC = 0x80000000,
        uint? secondFrameMinus4 = 0x11111111,
        bool? secondCallReturned = true,
        uint returnEbx = 0x9000,
        uint? returnHelperEax = 0x100,
        uint? returnFrameMinus4 = 0x22222222,
        uint? repeatEntryEcx = null,
        uint? repeatEntryEdi = null,
        bool? directionFlagSet = null,
        bool? repCompletedNormally = null,
        uint finalGateEbp = 0x4000,
        uint? finalGateFrameMinus14 = 0x5000,
        uint? finalGateReceiver4 = 0,
        bool? finalCallReturned = null,
        uint? finalReturnEdi = null,
        uint? finalReturnEsi = null,
        uint? finalReturnEbx = null,
        uint? finalReturnEbp = null,
        uint? receiver8Old = 0xFFFFFFFC) =>
        new(
            AboveLifecycle: new(
                AboveTransfer: new(
                    ValidatedBranchEntry: firstEntryValidated,
                    EntryEsi: aboveEntryEsi,
                    EntryEcx: aboveEntryEcx,
                    EntryEdx: aboveEntryEdx,
                    CallCellDword: firstCallCell,
                    CallReturnedNormally: firstCallReturned),
                AboveSecondTransfer: new(
                    ValidatedNormalEntry: false,
                    EntryEbx: secondEntryEbx,
                    EntryEdi: secondEntryEdi,
                    EntryEbp: secondEntryEbp,
                    Receiver4Dword: secondReceiver4,
                    FrameMinusCDword: secondFrameMinusC,
                    FrameMinus4Dword: secondFrameMinus4,
                    CallReturnedNormally: secondCallReturned),
                AboveReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: returnEntryEbp ?? secondEntryEbp,
                    EntryEbx: returnEbx,
                    EntryEsi: aboveEntryEsi,
                    HelperReturnEax: returnHelperEax,
                    FrameMinus4: returnFrameMinus4)),
            AboveRepeat: new(
                AboveReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: 0xDEAD0000,
                    EntryEbx: 0xBAD00000,
                    EntryEsi: 0xBAD00001,
                    HelperReturnEax: null,
                    FrameMinus4: null),
                RepeatEntryEcx: repeatEntryEcx
                    ?? (aboveEntryEsi == 0 ? null : aboveEntryEsi),
                RepeatEntryEdi: repeatEntryEdi
                    ?? (aboveEntryEsi == 0 ? null : unchecked(returnEbx
                        + (returnHelperEax ?? 0))),
                DirectionFlagSet: directionFlagSet
                    ?? (aboveEntryEsi == 0 ? null : false),
                RepCompletedNormally: repCompletedNormally
                    ?? (aboveEntryEsi == 0 ? null : true)),
            FinalGate: new(
                CaptureValidatedJoin: false,
                EntryEbp: finalGateEbp,
                FrameMinus14: finalGateFrameMinus14,
                Receiver4: finalGateReceiver4,
                NormalCallReturned: finalCallReturned),
            FinalReturn: new(
                ValidatedNormalTail: false,
                EntryEdi: finalReturnEdi ?? finalGateFrameMinus14 ?? 0,
                EntryEsi: finalReturnEsi ?? aboveEntryEsi,
                EntryEbx: finalReturnEbx ?? returnFrameMinus4 ?? 0,
                EntryEbp: finalReturnEbp ?? finalGateEbp,
                Receiver8Old: receiver8Old));

    private static bool IsComplete(FrameHandoffCapacityAboveBranchLifecyclePlan plan) =>
        plan.Outcome == FrameHandoffCapacityAboveBranchLifecycleOutcome.ProjectionComplete
        && plan.AboveLifecycleAdmissionSatisfied
        && plan.AboveLifecycleUnreachedCapturesClear
        && plan.SuppliedAboveRepeatReturnGateMatchesDerived == false
        && plan.AboveFinalLifecycleAdmissionSatisfied
        && plan.AboveFinalLifecyclePlan?.AboveRepeatAdmissionSatisfied == true
        && plan.AboveFinalLifecyclePlan?.FinalGateCaptureValidatedJoinDerived == true
        && plan.AboveFinalLifecyclePlan?.FinalGatePlan?.NormalBoundaryAdmitted == true
        && plan.AboveFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan?.ReturnReached == true
        && plan.AboveFinalLifecyclePlan?.FinalOutcomeClean == true
        && !plan.UnreachedAboveRepeatEntryEcxCaptureProvided
        && !plan.UnreachedAboveRepeatEntryEdiCaptureProvided
        && !plan.UnreachedAboveRepeatDirectionFlagCaptureProvided
        && !plan.UnreachedAboveRepeatRepCompletionCaptureProvided
        && !plan.UnreachedFinalGateFrameMinus14CaptureProvided
        && !plan.UnreachedFinalGateReceiver4CaptureProvided
        && !plan.UnreachedFinalGateNormalCallStatusProvided
        && !plan.UnreachedFinalGateEntryEbpIdentityCaptureProvided
        && !plan.UnreachedFinalReturnEntryEbpCaptureProvided
        && !plan.UnreachedFinalReturnEntryEdiCaptureProvided
        && !plan.UnreachedFinalReturnEntryEbxCaptureProvided
        && !plan.UnreachedFinalReturnEntryEsiCaptureProvided
        && !plan.UnreachedFinalReturnReceiver8OldCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityAboveBranchLifecycleProjection
                .LifecycleEndRawFileOffsetExclusive;
}
