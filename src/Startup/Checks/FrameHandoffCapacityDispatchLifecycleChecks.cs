using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityDispatchLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityDispatchLifecyclePlan above =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    returnEntryEdi: 0x1200,
                    returnEntryEsi: 0x20,
                    helperReturnEax: 0x30,
                    returnReceiver4: 0x77,
                    frameMinus8: 0x21,
                    aboveBranch: AboveBranchCapture(
                        aboveEntryEsi: 0xBAD0,
                        aboveEntryEcx: 0xBAD1,
                        aboveEntryEdx: 0xBAD2)));
        FrameHandoffCapacityDispatchLifecyclePlan atBoundary =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    returnEntryEsi: 0,
                    returnReceiver4: 0,
                    frameMinus8: 0,
                    secondBranch: SecondBranchCapture()));
        check("The admitted unsigned routes dispatch only their successor using fresh return outputs",
            above.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.ProjectionComplete
            && above.SelectedRoute == FrameHandoffCapacityDispatchRoute.AboveCapacity
            && above.AboveBranchPlan?.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome.ProjectionComplete
            && above.DerivedAboveBranchCapture?.AboveLifecycle.AboveTransfer
                is FrameHandoffCapacityAboveTransferCapture derivedAbove
            && derivedAbove.ValidatedBranchEntry
            && derivedAbove.EntryEsi == 0x20
            && derivedAbove.EntryEcx == 0x77
            && derivedAbove.EntryEdx == 0x1230
            && above.DerivedAboveBranchCapture?.AboveLifecycle.AboveTransfer
                != above.Capture.AboveBranch?.AboveLifecycle.AboveTransfer
            && above.SecondBranchPlan is null
            && atBoundary.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.ProjectionComplete
            && atBoundary.SelectedRoute
                == FrameHandoffCapacityDispatchRoute.AtOrBelowCapacity
            && atBoundary.FirstTransferPlan.ReturnPlan?.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedLessThanOrEqualToEntryEsi
            && atBoundary.DerivedSecondBranchCapture?.SecondTransfer.Transfer
                is FrameHandoffCapacitySecondTransferCapture derivedSecond
            && derivedSecond.EntryEcx == 0
            && derivedSecond.EntryEdi == 0
            && derivedSecond.EntryEsi == 0
            && derivedSecond.EntryEbp == 0x4000
            && atBoundary.SecondBranchPlan?.Outcome
                == FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete
            && atBoundary.AboveBranchPlan is null);

        FrameHandoffCapacityDispatchLifecyclePlan wrapped =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    entryEbx: uint.MaxValue,
                    entryEbp: 3,
                    transferEntryEdi: 0x12,
                    returnEntryEdi: uint.MaxValue,
                    returnEntryEsi: 4,
                    helperReturnEax: 2,
                    returnReceiver4: 0,
                    frameMinus8: 5,
                    aboveBranch: AboveBranchCapture(aboveEntryEsi: 4)));
        check("DWORD wraparound is preserved across fresh reads and derived successor registers",
            wrapped.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.ProjectionComplete
            && wrapped.SelectedRoute == FrameHandoffCapacityDispatchRoute.AboveCapacity
            && wrapped.FirstTransferPlan.TransferPlan.Reads[0].Address == 3
            && wrapped.FirstTransferPlan.ReturnPlan?.Reads[0].Address == 3
            && wrapped.FirstTransferPlan.ReturnPlan?.Reads[1].Address == 0xFFFFFFFB
            && wrapped.FirstTransferPlan.ReturnPlan?.EdxAfterLea == 1
            && wrapped.DerivedAboveBranchCapture?.AboveLifecycle.AboveTransfer.EntryEcx == 0
            && wrapped.DerivedAboveBranchCapture?.AboveLifecycle.AboveTransfer.EntryEdx == 1
            && wrapped.DerivedAboveBranchCapture?.AboveLifecycle.AboveTransfer.EntryEsi == 4);

        FrameHandoffCapacityDispatchLifecycleCapture blockedCapture =
            CreateDispatchCapture(
                firstHelperReturnedNormally: false,
                aboveBranch: AboveBranchCapture(),
                secondBranch: SecondBranchCapture());
        FrameHandoffCapacityDispatchLifecyclePlan blocked =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(blockedCapture);
        FrameHandoffCapacityDispatchLifecyclePlan missingFirstRead =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    firstReceiver4: null,
                    aboveBranch: AboveBranchCapture()));
        FrameHandoffCapacityDispatchLifecyclePlan missingReturnRead =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    returnReceiver4: null,
                    aboveBranch: AboveBranchCapture()));
        FrameHandoffCapacityDispatchLifecyclePlan returnIdentityMismatch =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    returnEntryEbx: 0x3001,
                    secondBranch: SecondBranchCapture()));
        FrameHandoffCapacityDispatchLifecycleCapture forgedValidationCapture =
            CreateDispatchCapture(
                firstHelperReturnedNormally: false,
                aboveBranch: AboveBranchCapture(
                    firstEntryValidated: true,
                    aboveEntryEsi: 0xBAD0,
                    aboveEntryEcx: 0xBAD1,
                    aboveEntryEdx: 0xBAD2));
        forgedValidationCapture = forgedValidationCapture with
        {
            FirstTransfer = forgedValidationCapture.FirstTransfer with
            {
                Return = forgedValidationCapture.FirstTransfer.Return with
                {
                    ValidatedNormalEntry = true,
                    HelperReturnEax = null,
                    Receiver4 = null,
                    FrameMinus8 = null
                }
            }
        };
        FrameHandoffCapacityDispatchLifecyclePlan forgedValidation =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                forgedValidationCapture);
        check("Blocked, incomplete, or mismatched first routes never execute a downstream branch",
            blocked.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome
                    .FirstTransferHasUnreachedCaptures
            && blocked.FirstTransferPlan.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.TransferBlocked
            && blocked.FirstTransferPlan.TransferPlan.FrameWrites.Length == 1
            && blocked.FirstTransferPlan.TransferPlan.ScalarPushes.Length == 3
            && blocked.FirstTransferPlan.TransferPlan.HelperReturnedNormally == false
            && blocked.FirstTransferPlan.UnreachedReturnReceiver4CaptureProvided
            && blocked.FirstTransferPlan.UnreachedReturnHelperReturnEaxCaptureProvided
            && blocked.FirstTransferPlan.UnreachedReturnFrameMinus8CaptureProvided
            && blocked.AboveBranchPlan is null
            && blocked.SecondBranchPlan is null
            && blocked.UnreachedAboveBranchCaptureProvided
            && blocked.UnreachedSecondBranchCaptureProvided
            && missingFirstRead.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome
                    .FirstTransferHasUnreachedCaptures
            && missingFirstRead.FirstTransferPlan.TransferPlan.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingReceiver4Capture
            && missingFirstRead.FirstTransferPlan.TransferPlan.UnreachedReceiver8CaptureProvided
            && missingFirstRead.FirstTransferPlan.TransferPlan.UnreachedFrameMinus4CaptureProvided
            && missingFirstRead.AboveBranchPlan is null
            && missingReturnRead.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome
                    .FirstReturnHasUnreachedCaptures
            && missingReturnRead.FirstTransferPlan.ReturnPlan?.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.MissingReceiver4Capture
            && missingReturnRead.FirstTransferPlan.UnreachedReturnHelperReturnEaxCaptureProvided
            && missingReturnRead.FirstTransferPlan.UnreachedReturnFrameMinus8CaptureProvided
            && missingReturnRead.AboveBranchPlan is null
            && returnIdentityMismatch.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome
                    .FirstReturnEntryIdentityMismatch
            && returnIdentityMismatch.SecondBranchPlan is null
            && forgedValidation.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.FirstTransferBlocked
            && forgedValidation.FirstTransferPlan.ReturnPlan is null
            && !forgedValidation.FirstTransferPlan.UnreachedReturnReceiver4CaptureProvided
            && !forgedValidation.FirstTransferPlan.UnreachedReturnHelperReturnEaxCaptureProvided
            && !forgedValidation.FirstTransferPlan.UnreachedReturnFrameMinus8CaptureProvided
            && forgedValidation.AboveBranchPlan is null
            && forgedValidation.UnreachedAboveBranchCaptureProvided);

        FrameHandoffCapacityDispatchLifecyclePlan missingSelectedBranch =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    aboveBranch: null,
                    secondBranch: SecondBranchCapture()));
        FrameHandoffCapacityDispatchLifecyclePlan unusedBranch =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    aboveBranch: AboveBranchCapture(),
                    secondBranch: SecondBranchCapture()));
        check("A missing selected capture is distinct from an unselected supplied capture",
            missingSelectedBranch.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.SelectedBranchCaptureMissing
            && missingSelectedBranch.SelectedRoute
                == FrameHandoffCapacityDispatchRoute.AboveCapacity
            && !missingSelectedBranch.SelectedBranchCaptureProvided
            && missingSelectedBranch.UnselectedBranchCaptureProvided
            && missingSelectedBranch.UnreachedSecondBranchCaptureProvided
            && missingSelectedBranch.AboveBranchPlan is null
            && unusedBranch.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome
                    .UnselectedBranchCaptureProvided
            && unusedBranch.SelectedBranchCaptureProvided
            && unusedBranch.UnselectedBranchCaptureProvided
            && unusedBranch.UnreachedSecondBranchCaptureProvided
            && unusedBranch.AboveBranchPlan?.Outcome
                == FrameHandoffCapacityAboveBranchLifecycleOutcome.ProjectionComplete
            && unusedBranch.SecondBranchPlan is null);

        FrameHandoffCapacityDispatchLifecyclePlan failedDownstream =
            FrameHandoffCapacityDispatchLifecycleProjection.Project(
                CreateDispatchCapture(
                    aboveBranch: AboveBranchCapture(firstCallReturned: false)));
        check("A failed selected successor retains its reached pushes and blocks its later tail",
            failedDownstream.Outcome
                == FrameHandoffCapacityDispatchLifecycleOutcome.AboveBranchHasUnreachedCaptures
            && failedDownstream.FirstRouteAdmissionValidated
            && failedDownstream.AboveBranchPlan?.AboveLifecyclePlan.AboveTransferPlan
                .CallReached == true
            && failedDownstream.AboveBranchPlan?.AboveLifecyclePlan.AboveTransferPlan
                .CallReturnedNormally == false
            && failedDownstream.AboveBranchPlan?.AboveLifecyclePlan.AboveTransferPlan
                .Pushes.Length == 3
            && failedDownstream.AboveBranchPlan?.AboveFinalLifecyclePlan is null
            && failedDownstream.SecondBranchPlan is null);
    }

    private static FrameHandoffCapacityDispatchLifecycleCapture CreateDispatchCapture(
        uint entryEbx = 0x3000,
        uint entryEbp = 0x4000,
        uint transferEntryEdi = 3,
        bool? firstHelperReturnedNormally = true,
        uint? returnEntryEbx = null,
        uint? returnEntryEbp = null,
        uint returnEntryEdi = 0x10,
        uint returnEntryEsi = 0x20,
        uint? helperReturnEax = 0x20,
        uint? returnReceiver4 = 0x77,
        uint? frameMinus8 = 0x21,
        uint? firstReceiver4 = 5,
        FrameHandoffCapacityAboveBranchLifecycleCapture? aboveBranch = null,
        FrameHandoffCapacitySecondBranchLifecycleCapture? secondBranch = null) =>
        new(
            FirstTransfer: new(
                Transfer: new(
                    EntryEbx: entryEbx,
                    EntryEbp: entryEbp,
                    EntryEdi: transferEntryEdi,
                    Receiver4: firstReceiver4,
                    Receiver8: 8,
                    FrameMinus4: 0x20,
                    FrameMinusC: 0x30,
                    HelperReturnedNormally: firstHelperReturnedNormally),
                Return: new(
                    ValidatedNormalEntry: false,
                    EntryEbx: returnEntryEbx ?? entryEbx,
                    EntryEbp: returnEntryEbp ?? entryEbp,
                    EntryEdi: returnEntryEdi,
                    EntryEsi: returnEntryEsi,
                    HelperReturnEax: helperReturnEax,
                    Receiver4: returnReceiver4,
                    FrameMinus8: frameMinus8)),
            AboveBranch: aboveBranch,
            SecondBranch: secondBranch);

    private static FrameHandoffCapacityAboveBranchLifecycleCapture AboveBranchCapture(
        bool firstEntryValidated = false,
        uint aboveEntryEsi = 0x20,
        uint aboveEntryEcx = 0xBAD1,
        uint aboveEntryEdx = 0xBAD2,
        bool? firstCallReturned = true,
        uint secondEntryEbp = 0x4000,
        uint? finalGateReceiver4 = 0,
        bool? finalCallReturned = null,
        uint? receiver8Old = 0xFFFFFFFC) =>
        new(
            AboveLifecycle: new(
                AboveTransfer: new(
                    ValidatedBranchEntry: firstEntryValidated,
                    EntryEsi: aboveEntryEsi,
                    EntryEcx: aboveEntryEcx,
                    EntryEdx: aboveEntryEdx,
                    CallCellDword: 0x1000,
                    CallReturnedNormally: firstCallReturned),
                AboveSecondTransfer: new(
                    ValidatedNormalEntry: false,
                    EntryEbx: 0x3000,
                    EntryEdi: 0x4000,
                    EntryEbp: secondEntryEbp,
                    Receiver4Dword: 1,
                    FrameMinusCDword: 0x80000000,
                    FrameMinus4Dword: 0x11111111,
                    CallReturnedNormally: true),
                AboveReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: secondEntryEbp,
                    EntryEbx: 0x9000,
                    EntryEsi: aboveEntryEsi,
                    HelperReturnEax: 0x100,
                    FrameMinus4: 0x22222222)),
            AboveRepeat: new(
                AboveReturnGate: new(
                    ValidatedNormalEntry: false,
                    EntryEbp: 0xDEAD0000,
                    EntryEbx: 0xBAD00000,
                    EntryEsi: 0xBAD00001,
                    HelperReturnEax: null,
                    FrameMinus4: null),
                RepeatEntryEcx: aboveEntryEsi == 0 ? null : aboveEntryEsi,
                RepeatEntryEdi: aboveEntryEsi == 0 ? null : 0x9100,
                DirectionFlagSet: aboveEntryEsi == 0 ? null : false,
                RepCompletedNormally: aboveEntryEsi == 0 ? null : true),
            FinalGate: new(
                CaptureValidatedJoin: false,
                EntryEbp: 0x4000,
                FrameMinus14: 0x5000,
                Receiver4: finalGateReceiver4,
                NormalCallReturned: finalCallReturned),
            FinalReturn: new(
                ValidatedNormalTail: false,
                EntryEdi: finalGateReceiver4 == 0 ? 0x5000u : 0xD000u,
                EntryEsi: aboveEntryEsi,
                EntryEbx: 0x22222222,
                EntryEbp: 0x4000,
                Receiver8Old: receiver8Old));

    private static FrameHandoffCapacitySecondBranchLifecycleCapture SecondBranchCapture(
        uint entryEbp = 0x4000,
        uint frameMinusC = 0,
        uint postCallEbx = 0x9000,
        uint helperReturnEax = 0x100,
        bool nonzeroFillRoute = false,
        uint finalReceiver4 = 0,
        bool? callReturnedNormally = true,
        bool? repReturnedNormally = true)
    {
        uint? freshFrameMinus8 = nonzeroFillRoute ? 0x5000u : 0u;
        uint? fillEntryEdi = frameMinusC == 0
            ? nonzeroFillRoute ? 0x5000u : 0u
            : freshFrameMinus8;
        uint postCallEaxAddressSum = unchecked(postCallEbx + helperReturnEax);
        bool hasSharedRepeat = fillEntryEdi is uint nonzeroEntryEdi && nonzeroEntryEdi != 0;

        FrameHandoffCapacitySecondTransferLifecycleCapture secondTransfer = new(
            Transfer: new(
                EntryEdi: 3,
                EntryEsi: 0x20,
                EntryEcx: 0x99,
                EntryEbp: entryEbp,
                CallCellDword: 0x1234,
                FrameMinus10Dword: 0x5678,
                CallReturnedNormally: callReturnedNormally),
            ReturnGate: new(
                ValidatedNormalEntry: false,
                EntryEbp: entryEbp,
                EntryEbx: postCallEbx,
                FrameMinusC: frameMinusC,
                HelperReturnEax: helperReturnEax));

        FrameHandoffCapacityRepeatFillJoinCapture repeatFillJoin = new(
            GateRepeatCapture: new(
                GatePlan: null,
                RepeatEntryEcx: frameMinusC == 0 ? null : frameMinusC,
                RepeatEntryEdx: frameMinusC == 0 ? null : postCallEaxAddressSum,
                RepeatEntryEbp: frameMinusC == 0 ? null : entryEbp,
                DirectionFlagSet: frameMinusC == 0 ? null : false,
                RepReturnedNormally: frameMinusC == 0 ? null : repReturnedNormally,
                FreshFrameMinus8Value: frameMinusC == 0 ? null : freshFrameMinus8),
            FillJoinEntryEbp: entryEbp,
            FillJoinEntryEdi: fillEntryEdi,
            FrameMinus4: 0x6000);

        FrameHandoffCapacityFinalLifecycleCapture final = new(
            Gate: new(
                CaptureValidatedJoin: false,
                EntryEbp: entryEbp,
                FrameMinus14: 0x7000,
                Receiver4: finalReceiver4,
                NormalCallReturned: finalReceiver4 == 0 ? null : true),
            Final: new(
                ValidatedNormalTail: false,
                EntryEdi: finalReceiver4 == 0 ? 0x7000u : 0x7777u,
                EntryEsi: 0xCAFEBABE,
                EntryEbx: 0x6000,
                EntryEbp: entryEbp,
                Receiver8Old: 0xD000));

        return new(
            secondTransfer,
            new(
                RepeatFillJoin: repeatFillJoin,
                Final: final,
                ExpectedSharedRepeatEntryEcx: hasSharedRepeat ? fillEntryEdi : null,
                ExpectedSharedRepeatEntryEdi: hasSharedRepeat ? 0x6000u : null,
                SharedRepeatDirectionFlagSet: hasSharedRepeat ? false : null,
                SharedRepeatCompletedNormally: hasSharedRepeat ? true : null));
    }
}
