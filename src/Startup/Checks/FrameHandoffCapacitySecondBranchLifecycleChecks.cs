using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySecondBranchLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacitySecondReturnGatePlan suppliedForgedZeroPlan =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteSecondTransfer(frameMinusC: 0))
                .ReturnGatePlan!.Value;
        FrameHandoffCapacitySecondBranchLifecycleCapture missingCallCellCapture =
            CompleteCapture(callCell: null);
        missingCallCellCapture = WithSuppliedGatePlan(
            missingCallCellCapture,
            suppliedForgedZeroPlan);
        FrameHandoffCapacitySecondBranchLifecyclePlan missingCallCell =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                missingCallCellCapture);
        check("An upstream missing transfer read blocks the forged downstream gate and flags later captures",
            missingCallCell.Outcome
                    == FrameHandoffCapacitySecondBranchLifecycleOutcome
                        .SecondTransferHasUnreachedCaptures
            && missingCallCell.SecondTransferPlan.TransferPlan.FrameMinusCWriteReached
            && missingCallCell.SecondTransferPlan.TransferPlan.CallCellReadReached
            && !missingCallCell.SecondTransferPlan.TransferPlan.CallCellCaptured
            && missingCallCell.ForwardedReturnGatePlan is null
            && missingCallCell.DerivedFirstFinalCapture.RepeatFillJoin.GateRepeatCapture.GatePlan
                is null
            && missingCallCell.UnreachedFillJoinEntryEdiCaptureProvided
            && missingCallCell.UnreachedFillJoinFrameMinus4CaptureProvided
            && missingCallCell.UnreachedFinalFrameMinus14CaptureProvided
            && missingCallCell.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySecondTransferProjection.CallCellReadRawFileOffset);

        FrameHandoffCapacitySecondBranchLifecyclePlan nonreturning =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(
                    callReturnedNormally: false,
                    nonzeroFillRoute: true,
                    captureUnreachedRepeatInputs: true));
        check("A nonreturning second transfer preserves its write and pushes and blocks the final chain",
            nonreturning.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .SecondTransferHasUnreachedCaptures
            && nonreturning.SecondTransferPlan.TransferPlan.FrameMinusCWriteReached
            && nonreturning.SecondTransferPlan.TransferPlan.Pushes.Length == 3
            && nonreturning.SecondTransferPlan.TransferPlan.CallReturnedNormally == false
            && nonreturning.ForwardedReturnGatePlan is null
            && nonreturning.UnreachedRepeatEntryEcxCaptureProvided
            && nonreturning.UnreachedFillJoinFrameMinus4CaptureProvided
            && nonreturning.UnreachedSharedRepeatEntryEcxCaptureProvided
            && nonreturning.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacitySecondBranchLifecyclePlan frameMismatch =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(
                    returnEbp: 0x4001,
                    captureUnreachedRepeatInputs: true));
        check("Return-frame identity mismatch prevents any downstream admission",
            frameMismatch.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome
                    .SecondReturnEntryIdentityMismatch
            && frameMismatch.SecondTransferPlan.TransferPlan.FrameMinusCWriteReached
            && frameMismatch.SecondTransferPlan.TransferPlan.Pushes.Length == 3
            && frameMismatch.SecondTransferPlan.ReturnGatePlan is null
            && !frameMismatch.FirstFinalAdmissionValidated
            && frameMismatch.UnreachedRepeatEntryEcxCaptureProvided
            && frameMismatch.UnreachedFinalFrameMinus14CaptureProvided);

        FrameHandoffCapacitySecondBranchLifecycleCapture nonzeroReturnCapture =
            CompleteCapture(
                frameMinusC: 0x12345678,
                postCallEbx: 0x9000,
                helperReturnEax: 0x100,
                nonzeroFillRoute: true);
        nonzeroReturnCapture = WithSuppliedGatePlan(
            nonzeroReturnCapture,
            suppliedForgedZeroPlan);
        FrameHandoffCapacitySecondBranchLifecyclePlan nonzeroReturn =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                nonzeroReturnCapture);
        check("The exact successful nonzero return plan replaces a supplied forged plan",
            nonzeroReturn.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            && nonzeroReturn.SecondReturnBranchAdmissionValidated
            && nonzeroReturn.ForwardedReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough
            && nonzeroReturn.FirstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan.Capture.GatePlan
                == nonzeroReturn.SecondTransferPlan.ReturnGatePlan
            && nonzeroReturn.DerivedFirstFinalCapture.RepeatFillJoin.GateRepeatCapture.GatePlan
                == nonzeroReturn.SecondTransferPlan.ReturnGatePlan
            && nonzeroReturn.ExactReturnGatePlanForwarded
            && nonzeroReturn.SecondTransferPlan.ReturnGatePlan?.Capture.EntryEbx == 0x9000
            && nonzeroReturn.SecondTransferPlan.ReturnGatePlan?.EdxAfterLea == 0x9100
            && nonzeroReturn.SecondTransferPlan.TransferPlan.EbxAfterLea != 0x9000
            && nonzeroReturn.FirstFinalPlan.FillFinalLifecyclePlan?.FinalLifecyclePlan
                ?.FinalPlan?.Capture.EntryEsi == 0xCAFEBABE
            && nonzeroReturn.Capture.SecondTransfer.Transfer.EntryEsi == 0x20);

        FrameHandoffCapacitySecondBranchLifecyclePlan zeroReturn =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(frameMinusC: 0, nonzeroFillRoute: false));
        check("The exact clean zero return branch admits the independent fill zero route",
            zeroReturn.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            && zeroReturn.SecondReturnBranchAdmissionValidated
            && zeroReturn.ForwardedReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
            && zeroReturn.FirstFinalPlan.RepeatFillJoinPlan.ZeroBranchBypassAdmission
            && zeroReturn.FirstFinalPlan.RepeatFillJoinPlan.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome
                    .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroReturn.FirstFinalPlan.FillFinalLifecyclePlan?.Outcome
                == FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete);

        bool allEightRoutesComplete = true;
        foreach (bool zeroReturnBranch in new[] { false, true })
        foreach (bool zeroFillJoin in new[] { false, true })
        foreach (bool zeroFinalCall in new[] { false, true })
        {
            FrameHandoffCapacitySecondBranchLifecyclePlan route =
                FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                    CompleteCapture(
                        frameMinusC: zeroReturnBranch ? 0u : 0x100u,
                        nonzeroFillRoute: !zeroFillJoin,
                        finalReceiver4: zeroFinalCall ? 0u : 0xA000u));
            allEightRoutesComplete &=
                route.Outcome
                    == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
                && route.FirstFinalAdmissionValidated
                && route.FirstFinalOutcomeClean
                && route.FirstFinalPlan.RepeatFillJoinPlan.ExactFillJoinOutcome
                && route.ForwardedReturnGatePlan?.Outcome
                    == (zeroReturnBranch
                        ? FrameHandoffCapacitySecondReturnGateOutcome
                            .FrameMinusCZeroBranchTaken
                        : FrameHandoffCapacitySecondReturnGateOutcome
                            .FrameMinusCNonzeroFallThrough)
                && route.FirstFinalPlan.RepeatFillJoinPlan.Outcome
                    == (zeroFillJoin
                        ? FrameHandoffCapacityRepeatFillJoinOutcome
                            .ZeroEntryEdiBranchToRawFileOffsetBE6CA2
                        : FrameHandoffCapacityRepeatFillJoinOutcome
                            .NonzeroEntryEdiJumpToRawFileOffsetBE6C9E)
                && route.FirstFinalPlan.FillFinalLifecyclePlan?.Outcome
                    == FrameHandoffCapacityFillFinalLifecycleOutcome.ProjectionComplete
                && route.FirstFinalPlan.FillFinalLifecyclePlan?.FinalGatePlan?.Outcome
                    == (zeroFinalCall
                        ? FrameHandoffCapacityFinalCallGateOutcome
                            .ZeroReceiver4CallSkipped
                        : FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned)
                && route.ExactReturnGatePlanForwarded;
        }
        check("Both return branches compose through both fill routes and both final-gate routes",
            allEightRoutesComplete);

        FrameHandoffCapacitySecondBranchLifecyclePlan partialRep =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 0x20,
                    nonzeroFillRoute: true,
                    repReturnedNormally: false));
        check("A partial REP preserves upstream effects and marks fill, shared REP, and final captures unreached",
            partialRep.Outcome
                != FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            && partialRep.SecondTransferPlan.TransferPlan.FrameMinusCWriteReached
            && partialRep.SecondTransferPlan.TransferPlan.Pushes.Length == 3
            && partialRep.SecondTransferPlan.ReturnGatePlan?.StackCleanupReached == true
            && partialRep.FirstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                ?.Outcome == FrameHandoffCapacityRepeatStoreOutcome.RepDidNotReturnNormally
            && partialRep.UnreachedFillJoinEntryEdiCaptureProvided
            && partialRep.UnreachedFillJoinFrameMinus4CaptureProvided
            && partialRep.UnreachedSharedRepeatEntryEcxCaptureProvided
            && partialRep.UnreachedSharedRepeatEntryEdiCaptureProvided
            && partialRep.UnreachedFinalFrameMinus14CaptureProvided
            && partialRep.UnreachedFinalReceiver8OldCaptureProvided);

        FrameHandoffCapacitySecondBranchLifecyclePlan missingFreshFrame8 =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(
                    frameMinusC: 0x20,
                    nonzeroFillRoute: true,
                    captureFreshFrameMinus8: false,
                    independentFillJoinEntryEdi: 0x5000));
        check("Missing the post-REP frame-8 reload retains the REP result and blocks later stages",
            missingFreshFrame8.FirstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                ?.RepInstructionReached == true
            && missingFreshFrame8.FirstFinalPlan.RepeatFillJoinPlan.GateRepeatPlan.RepeatPlan
                ?.AllStoresCompleted == true
            && missingFreshFrame8.UnreachedFillJoinEntryEdiCaptureProvided
            && missingFreshFrame8.UnreachedSharedRepeatEntryEcxCaptureProvided
            && missingFreshFrame8.UnreachedFinalFrameMinus14CaptureProvided);

        FrameHandoffCapacitySecondBranchLifecyclePlan wrapped =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                CompleteCapture(
                    entryEbp: 3,
                    frameMinusC: 0,
                    postCallEbx: uint.MaxValue,
                    helperReturnEax: 2,
                    nonzeroFillRoute: false,
                    frameMinus4: uint.MaxValue,
                    finalEntryEsi: 2,
                    finalReceiver8Old: uint.MaxValue));
        check("DWORD wrapping remains visible from the second return LEA through the RET4 update",
            wrapped.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            && wrapped.SecondTransferPlan.ReturnGatePlan?.EdxAfterLea == 1
            && wrapped.FirstFinalPlan.RepeatFillJoinPlan.FillJoinPlan?.FrameMinus4ReadAddress
                == uint.MaxValue
            && wrapped.FirstFinalPlan.FillFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan
                ?.Receiver8ValueAfterAdd == 1
            && wrapped.FirstFinalPlan.FillFinalLifecyclePlan?.FinalLifecyclePlan?.FinalPlan
                ?.ReturnImmediateBytes == 4);
    }

    private static FrameHandoffCapacitySecondBranchLifecycleCapture CompleteCapture(
        uint entryEbp = 0x4000,
        uint frameMinusC = 0,
        uint postCallEbx = 0x9000,
        uint helperReturnEax = 0x100,
        bool nonzeroFillRoute = false,
        uint finalReceiver4 = 0,
        uint? callCell = 0x1234,
        uint? frameMinus10 = 0x5678,
        bool? callReturnedNormally = true,
        uint? returnEbp = null,
        bool? repReturnedNormally = true,
        bool captureFreshFrameMinus8 = true,
        bool captureUnreachedRepeatInputs = false,
        uint? independentFillJoinEntryEdi = null,
        uint frameMinus4 = 0x6000,
        uint finalEntryEsi = 0xCAFEBABE,
        uint finalReceiver8Old = 0xD000,
        uint? finalFrameMinus14 = 0x7000,
        uint? finalReceiver8OldCapture = null,
        uint? finalFrameMinus4 = null)
    {
        uint? freshFrameMinus8 = captureFreshFrameMinus8
            ? nonzeroFillRoute ? 0x5000u : 0u
            : null;
        uint? fillEntryEdi = independentFillJoinEntryEdi
            ?? (frameMinusC == 0
                ? nonzeroFillRoute ? 0x5000u : 0u
                : freshFrameMinus8);
        uint postCallEaxAddressSum = unchecked(postCallEbx + helperReturnEax);
        bool hasSharedRepeat = fillEntryEdi is uint nonzeroEntryEdi
            && nonzeroEntryEdi != 0;
        FrameHandoffCapacitySecondTransferLifecycleCapture secondTransfer =
            new(
                Transfer: new(
                    EntryEdi: 3,
                    EntryEsi: 0x20,
                    EntryEcx: 0x99,
                    EntryEbp: entryEbp,
                    CallCellDword: callCell,
                    FrameMinus10Dword: frameMinus10,
                    CallReturnedNormally: callReturnedNormally),
                ReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: returnEbp ?? entryEbp,
                    EntryEbx: postCallEbx,
                    FrameMinusC: frameMinusC,
                    HelperReturnEax: helperReturnEax));

        FrameHandoffCapacityGateRepeatCapture gateRepeat =
            frameMinusC == 0 && !captureUnreachedRepeatInputs
                ? new(
                    GatePlan: null,
                    RepeatEntryEcx: null,
                    RepeatEntryEdx: null,
                    RepeatEntryEbp: null,
                    DirectionFlagSet: null,
                    RepReturnedNormally: null,
                    FreshFrameMinus8Value: null)
                : new(
                    GatePlan: null,
                    RepeatEntryEcx: frameMinusC == 0 ? 0x20u : frameMinusC,
                    RepeatEntryEdx: frameMinusC == 0
                        ? 0x9100u
                        : postCallEaxAddressSum,
                    RepeatEntryEbp: entryEbp,
                    DirectionFlagSet: false,
                    RepReturnedNormally: repReturnedNormally,
                    FreshFrameMinus8Value: freshFrameMinus8);

        FrameHandoffCapacityRepeatFillJoinCapture repeatFillJoin =
            new(
                GateRepeatCapture: gateRepeat,
                FillJoinEntryEbp: entryEbp,
                FillJoinEntryEdi: fillEntryEdi,
                FrameMinus4: frameMinus4);

        FrameHandoffCapacityFinalLifecycleCapture final =
            new(
                Gate: new(
                    CaptureValidatedJoin: false,
                    EntryEbp: entryEbp,
                    FrameMinus14: finalFrameMinus14,
                    Receiver4: finalReceiver4,
                    NormalCallReturned: finalReceiver4 == 0 ? null : true),
                Final: new(
                    ValidatedNormalTail: false,
                    EntryEdi: finalReceiver4 == 0
                        ? finalFrameMinus14 ?? 0u
                        : 0x7777u,
                    EntryEsi: finalEntryEsi,
                    EntryEbx: finalFrameMinus4 ?? frameMinus4,
                    EntryEbp: entryEbp,
                    Receiver8Old: finalReceiver8OldCapture ?? finalReceiver8Old));

        return new(
            secondTransfer,
            new(
                RepeatFillJoin: repeatFillJoin,
                Final: final,
                ExpectedSharedRepeatEntryEcx: hasSharedRepeat ? fillEntryEdi : null,
                ExpectedSharedRepeatEntryEdi: hasSharedRepeat ? frameMinus4 : null,
                SharedRepeatDirectionFlagSet: hasSharedRepeat ? false : null,
                SharedRepeatCompletedNormally: hasSharedRepeat ? true : null));
    }

    private static FrameHandoffCapacitySecondTransferLifecycleCapture CompleteSecondTransfer(
        uint entryEbp = 0x4000,
        uint frameMinusC = 0,
        uint postCallEbx = 0x9000,
        uint helperReturnEax = 0x100) =>
        new(
            Transfer: new(
                EntryEdi: 3,
                EntryEsi: 0x20,
                EntryEcx: 0x99,
                EntryEbp: entryEbp,
                CallCellDword: 0x1234,
                FrameMinus10Dword: 0x5678,
                CallReturnedNormally: true),
            ReturnGate: new(
                ValidatedNormalEntry: false,
                EntryEbp: entryEbp,
                EntryEbx: postCallEbx,
                FrameMinusC: frameMinusC,
                HelperReturnEax: helperReturnEax));

    private static FrameHandoffCapacitySecondBranchLifecycleCapture WithSuppliedGatePlan(
        FrameHandoffCapacitySecondBranchLifecycleCapture capture,
        FrameHandoffCapacitySecondReturnGatePlan gatePlan) =>
        capture with
        {
            FirstFinal = capture.FirstFinal with
            {
                RepeatFillJoin = capture.FirstFinal.RepeatFillJoin with
                {
                    GateRepeatCapture =
                        capture.FirstFinal.RepeatFillJoin.GateRepeatCapture
                            with { GatePlan = gatePlan }
                }
            }
        };
}
