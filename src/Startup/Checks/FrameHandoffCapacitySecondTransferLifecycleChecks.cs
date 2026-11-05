using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySecondTransferLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacitySecondTransferLifecyclePlan zeroBranch =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(frameMinusC: 0));
        FrameHandoffCapacitySecondTransferLifecyclePlan nonzeroBranch =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(
                    postCallEbx: 0x9000,
                    frameMinusC: 0x12345678,
                    helperReturnEax: 0x100));
        check("Only the two clean return-gate outcomes complete after exact transfer admission",
            zeroBranch.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            && zeroBranch.TransferSuccessAdmission
            && zeroBranch.ReturnGatePlan?.Capture.ValidatedNormalEntry == true
            && zeroBranch.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
            && zeroBranch.ReturnBranchAdmitted
            && !zeroBranch.UnreachedReturnFrameMinusCCaptureProvided
            && !zeroBranch.UnreachedReturnHelperReturnEaxCaptureProvided
            && nonzeroBranch.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            && nonzeroBranch.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough
            && nonzeroBranch.ReturnBranchAdmitted
            && zeroBranch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySecondReturnGateProjection.ZeroBranchDestinationRawFileOffset
            && nonzeroBranch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacitySecondTransferLifecyclePlan nonreturning =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(callReturnedNormally: false));
        FrameHandoffCapacitySecondTransferLifecyclePlan missingCallReturn =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(callReturnedNormally: null));
        check("A nonreturning or unobserved first call blocks all return-gate captures",
            nonreturning.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferBlocked
            && nonreturning.TransferPlan.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.CallDidNotReturnNormally
            && nonreturning.ReturnGatePlan is null
            && nonreturning.UnreachedReturnFrameMinusCCaptureProvided
            && nonreturning.UnreachedReturnHelperReturnEaxCaptureProvided
            && missingCallReturn.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferBlocked
            && missingCallReturn.TransferPlan.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.MissingCallReturnCapture
            && missingCallReturn.TransferPlan.CallReached
            && missingCallReturn.ReturnGatePlan is null
            && missingCallReturn.UnreachedReturnFrameMinusCCaptureProvided
            && missingCallReturn.UnreachedReturnHelperReturnEaxCaptureProvided);

        FrameHandoffCapacitySecondTransferLifecycleCapture firstReadFlags =
            CompleteCapture() with
            {
                Transfer = CompleteCapture().Transfer with
                {
                    CallCellDword = null
                }
            };
        FrameHandoffCapacitySecondTransferLifecyclePlan firstReadBlocked =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(firstReadFlags);
        check("Captures supplied after a missing first transfer read are flagged and not admitted",
            firstReadBlocked.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.TransferHasUnreachedCaptures
            && !firstReadBlocked.TransferSuccessAdmission
            && !firstReadBlocked.TransferUnreachedCapturesClear
            && firstReadBlocked.TransferPlan.UnreachedFrameMinus10CaptureProvided
            && firstReadBlocked.TransferPlan.UnreachedCallReturnCaptureProvided
            && firstReadBlocked.ReturnGatePlan is null
            && firstReadBlocked.UnreachedReturnFrameMinusCCaptureProvided
            && firstReadBlocked.UnreachedReturnHelperReturnEaxCaptureProvided);

        FrameHandoffCapacitySecondTransferLifecyclePlan frameIdentityMismatch =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(returnEbp: 0x4001));
        check("Return EBP identity is checked before any fresh frame-C read",
            frameIdentityMismatch.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome
                    .ReturnEntryIdentityMismatch
            && frameIdentityMismatch.ReturnEntryIdentityValidated
            && !frameIdentityMismatch.ReturnEntryEbpMatches
            && frameIdentityMismatch.ReturnGatePlan is null
            && frameIdentityMismatch.UnreachedReturnFrameMinusCCaptureProvided
            && frameIdentityMismatch.UnreachedReturnHelperReturnEaxCaptureProvided
            && frameIdentityMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset);

        check("Post-call EBX is independent and the return gate reloads frame-C independently",
            nonzeroBranch.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            && nonzeroBranch.Capture.Transfer.EntryEbp
                == nonzeroBranch.Capture.ReturnGate.EntryEbp
            && nonzeroBranch.Capture.ReturnGate.EntryEbx == 0x9000
            && nonzeroBranch.ReturnGatePlan?.Capture.EntryEbx == 0x9000
            && nonzeroBranch.TransferPlan.FrameMinusCWriteValue
                != nonzeroBranch.ReturnGatePlan?.Reads[0].Value
            && nonzeroBranch.TransferPlan.FrameMinusCWriteAddress
                == nonzeroBranch.ReturnGatePlan?.Reads[0].Address
            && nonzeroBranch.ReturnGatePlan?.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondReturnGateRead(0x00BE6C34, 0x3FF4, 0x12345678)
            }) == true
            && nonzeroBranch.ReturnGatePlan?.EdxAfterLea == 0x9100);

        FrameHandoffCapacitySecondTransferLifecyclePlan missingReturnEax =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(helperReturnEax: null));
        check("Missing return EAX blocks after the fresh frame-C read and stack cleanup",
            missingReturnEax.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnBlocked
            && missingReturnEax.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.MissingHelperReturnEaxCapture
            && missingReturnEax.ReturnGatePlan?.FrameMinusCReadReached == true
            && missingReturnEax.ReturnGatePlan?.FrameMinusCCaptured == true
            && missingReturnEax.ReturnGatePlan?.StackCleanupReached == true
            && missingReturnEax.ReturnGatePlan?.EdxLeaReached == false
            && missingReturnEax.ReturnGatePlan?.TestReached == false
            && missingReturnEax.ReturnGatePlan?.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondReturnGateRead(0x00BE6C34, 0x3FF4, 0x10)
            }) == true
            && !missingReturnEax.UnreachedReturnFrameMinusCCaptureProvided
            && !missingReturnEax.UnreachedReturnHelperReturnEaxCaptureProvided
            && !missingReturnEax.ReturnBranchAdmitted);

        FrameHandoffCapacitySecondTransferLifecyclePlan returnReadFlags =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(frameMinusC: null, helperReturnEax: 0x777));
        check("An EAX capture supplied after a missing return frame-C read blocks completion",
            returnReadFlags.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ReturnBlocked
            && returnReadFlags.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.MissingFrameMinusCCapture
            && returnReadFlags.ReturnGatePlan?.FrameMinusCReadReached == true
            && returnReadFlags.ReturnGatePlan?.UnreachedHelperReturnEaxCaptureProvided == true
            && returnReadFlags.UnreachedReturnHelperReturnEaxCaptureProvided
            && !returnReadFlags.ReturnBranchAdmitted);

        FrameHandoffCapacitySecondTransferLifecyclePlan wrapped =
            FrameHandoffCapacitySecondTransferLifecycleProjection.Project(
                CompleteCapture(
                    entryEbp: 3,
                    postCallEbx: uint.MaxValue,
                    entryEdi: 0x20000000,
                    entryEsi: 1,
                    frameMinusC: 0,
                    helperReturnEax: 2));
        check("DWORD-wrapped transfer writes and post-call return LEA remain independent",
            wrapped.Outcome
                == FrameHandoffCapacitySecondTransferLifecycleOutcome.ProjectionComplete
            && wrapped.TransferPlan.FrameMinusCWriteAddress == 0xFFFFFFF7
            && wrapped.ReturnGatePlan?.Reads[0].Address == 0xFFFFFFF7
            && wrapped.ReturnGatePlan?.EdxAfterLea == 1
            && wrapped.ReturnGatePlan?.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
            && wrapped.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySecondReturnGateProjection.ZeroBranchDestinationRawFileOffset);
    }

    private static FrameHandoffCapacitySecondTransferLifecycleCapture CompleteCapture(
        uint entryEbp = 0x4000,
        uint postCallEbx = 0x3000,
        uint entryEdi = 3,
        uint entryEsi = 0x20,
        bool? callReturnedNormally = true,
        uint? callCellDword = 0x1234,
        uint? frameMinus10Dword = 0x5678,
        uint? frameMinusC = 0x10,
        uint? helperReturnEax = 0x20,
        uint? returnEbp = null) =>
        new(
            Transfer: new(
                EntryEdi: entryEdi,
                EntryEsi: entryEsi,
                EntryEcx: 0x99,
                EntryEbp: entryEbp,
                CallCellDword: callCellDword,
                FrameMinus10Dword: frameMinus10Dword,
                CallReturnedNormally: callReturnedNormally),
            ReturnGate: new(
                ValidatedNormalEntry: false,
                EntryEbp: returnEbp ?? entryEbp,
                EntryEbx: postCallEbx,
                FrameMinusC: frameMinusC,
                HelperReturnEax: helperReturnEax));
}
