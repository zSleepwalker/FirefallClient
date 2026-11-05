using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityTransferLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityTransferLifecyclePlan above =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(
                    transferEdi: 0x11,
                    returnEdi: 0xE0,
                    returnEsi: 1,
                    helperReturnEax: 0x20,
                    receiver4: 0xAABBCCDD,
                    frameMinus8: 0x80000000));
        FrameHandoffCapacityTransferLifecyclePlan notAbove =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(
                    transferEdi: 0x22,
                    returnEdi: 0xF0,
                    returnEsi: 0x100,
                    helperReturnEax: 0x30,
                    receiver4: 0x12345678,
                    frameMinus8: 0x50));
        check("Only the two clean unsigned return branches complete after exact transfer admission",
            above.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
            && above.TransferPlan.SuccessAdmission
            && above.ReturnPlan?.Capture.ValidatedNormalEntry == true
            && above.ReturnPlan?.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedGreaterThanEntryEsi
            && above.ReturnBranchAdmitted
            && !above.UnreachedReturnReceiver4CaptureProvided
            && !above.UnreachedReturnHelperReturnEaxCaptureProvided
            && !above.UnreachedReturnFrameMinus8CaptureProvided
            && notAbove.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
            && notAbove.ReturnPlan?.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedLessThanOrEqualToEntryEsi
            && notAbove.ReturnBranchAdmitted
                == (notAbove.ReturnPlan?.Outcome
                    == FrameHandoffCapacityTransferReturnOutcome
                        .FrameMinus8UnsignedLessThanOrEqualToEntryEsi)
            && above.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferReturnProjection
                    .AboveCapacityDestinationRawFileOffset
            && notAbove.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferReturnProjection
                    .NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacityTransferLifecyclePlan nonreturn =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(helperReturnedNormally: false));
        FrameHandoffCapacityTransferLifecyclePlan missingCallReturn =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(helperReturnedNormally: null));
        check("A nonreturning or unobserved first opaque call blocks every return-stage capture",
            nonreturn.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.TransferBlocked
            && nonreturn.TransferPlan.Outcome
                == FrameHandoffCapacityTransferOutcome.HelperDidNotReturnNormally
            && nonreturn.ReturnPlan is null
            && nonreturn.UnreachedReturnReceiver4CaptureProvided
            && nonreturn.UnreachedReturnHelperReturnEaxCaptureProvided
            && nonreturn.UnreachedReturnFrameMinus8CaptureProvided
            && missingCallReturn.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.TransferBlocked
            && missingCallReturn.TransferPlan.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingHelperNormalCapture
            && missingCallReturn.TransferPlan.IndirectCallReached
            && missingCallReturn.ReturnPlan is null
            && missingCallReturn.UnreachedReturnReceiver4CaptureProvided
            && missingCallReturn.UnreachedReturnHelperReturnEaxCaptureProvided
            && missingCallReturn.UnreachedReturnFrameMinus8CaptureProvided);

        FrameHandoffCapacityTransferLifecycleCapture firstTransferFlags =
            CompleteCapture() with
            {
                Transfer = CompleteCapture().Transfer with
                {
                    Receiver4 = null
                }
            };
        FrameHandoffCapacityTransferLifecyclePlan transferFlags =
            FrameHandoffCapacityTransferLifecycleProjection.Project(firstTransferFlags);
        check("Supplied captures after a blocked first-transfer read prevent return admission",
            transferFlags.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.TransferHasUnreachedCaptures
            && !transferFlags.TransferSuccessAdmission
            && !transferFlags.TransferUnreachedCapturesClear
            && transferFlags.TransferPlan.UnreachedReceiver8CaptureProvided
            && transferFlags.TransferPlan.UnreachedFrameMinus4CaptureProvided
            && transferFlags.TransferPlan.UnreachedFrameMinusCCaptureProvided
            && transferFlags.TransferPlan.UnreachedHelperReturnCaptureProvided
            && transferFlags.ReturnPlan is null
            && transferFlags.UnreachedReturnReceiver4CaptureProvided
            && transferFlags.UnreachedReturnHelperReturnEaxCaptureProvided
            && transferFlags.UnreachedReturnFrameMinus8CaptureProvided);

        FrameHandoffCapacityTransferLifecyclePlan ebxMismatch =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(returnEbx: 0x3001));
        FrameHandoffCapacityTransferLifecyclePlan ebpMismatch =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(returnEbp: 0x4001));
        check("Return EBX and EBP identity mismatches block reads and cleanup before projection",
            ebxMismatch.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ReturnEntryIdentityMismatch
            && ebxMismatch.ReturnEntryIdentitiesValidated
            && !ebxMismatch.ReturnEntryEbxMatches
            && ebxMismatch.ReturnEntryEbpMatches
            && ebxMismatch.ReturnPlan is null
            && ebxMismatch.UnreachedReturnReceiver4CaptureProvided
            && ebxMismatch.UnreachedReturnHelperReturnEaxCaptureProvided
            && ebxMismatch.UnreachedReturnFrameMinus8CaptureProvided
            && ebpMismatch.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ReturnEntryIdentityMismatch
            && ebpMismatch.ReturnEntryIdentitiesValidated
            && ebpMismatch.ReturnEntryEbxMatches
            && !ebpMismatch.ReturnEntryEbpMatches
            && ebpMismatch.ReturnPlan is null
            && ebpMismatch.UnreachedReturnReceiver4CaptureProvided
            && ebpMismatch.UnreachedReturnHelperReturnEaxCaptureProvided
            && ebpMismatch.UnreachedReturnFrameMinus8CaptureProvided);

        check("Post-call EDI and ESI are independent captures and return reads reload independently",
            above.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
            && above.Capture.Transfer.EntryEdi == 0x11
            && above.Capture.Return.EntryEdi == 0xE0
            && above.ReturnPlan?.Capture.EntryEdi == 0xE0
            && above.ReturnPlan?.Capture.EntryEsi == 1
            && above.ReturnPlan?.EdxAfterLea == 0x100
            && above.ReturnPlan?.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferReturnRead(0x00BE6BFD, 0x3004, 0xAABBCCDD),
                new FrameHandoffCapacityTransferReturnRead(0x00BE6C06, 0x3FF8, 0x80000000)
            }) == true
            && above.ReturnPlan?.Receiver4ReadReached == true
            && above.ReturnPlan?.FrameMinus8ReadReached == true
            && above.ReturnPlan?.FrameMinus10StoreReached == true
            && notAbove.ReturnPlan?.Capture.EntryEsi == 0x100
            && notAbove.ReturnPlan?.FrameMinus8UnsignedGreaterThanEntryEsi == false);

        FrameHandoffCapacityTransferLifecyclePlan missingReturnEax =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(helperReturnEax: null));
        check("Missing opaque helper EAX blocks after the fresh receiver read and cleanup",
            missingReturnEax.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ReturnBlocked
            && missingReturnEax.ReturnPlan?.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.MissingHelperReturnEaxCapture
            && missingReturnEax.ReturnPlan?.Receiver4ReadReached == true
            && missingReturnEax.ReturnPlan?.Receiver4Captured == true
            && missingReturnEax.ReturnPlan?.StackCleanupReached == true
            && missingReturnEax.ReturnPlan?.EdxLeaReached == false
            && missingReturnEax.ReturnPlan?.FrameMinus8ReadReached == false
            && !missingReturnEax.UnreachedReturnReceiver4CaptureProvided
            && !missingReturnEax.UnreachedReturnHelperReturnEaxCaptureProvided
            && missingReturnEax.UnreachedReturnFrameMinus8CaptureProvided
            && !missingReturnEax.ReturnBranchAdmitted);

        FrameHandoffCapacityTransferLifecyclePlan wrapped =
            FrameHandoffCapacityTransferLifecycleProjection.Project(
                CompleteCapture(
                    entryEbx: uint.MaxValue,
                    entryEbp: 3,
                    transferEdi: 0x12,
                    returnEdi: uint.MaxValue,
                    returnEsi: 0,
                    helperReturnEax: 2,
                    receiver4: 0,
                    frameMinus8: 0));
        check("DWORD-wrapped identities, fresh-read addresses, LEA, and frame store remain exact",
            wrapped.Outcome
                == FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
            && wrapped.TransferPlan.Reads[0].Address == 3
            && wrapped.ReturnPlan?.Reads[0].Address == 3
            && wrapped.ReturnPlan?.Reads[1].Address == 0xFFFFFFFB
            && wrapped.ReturnPlan?.EdxAfterLea == 1
            && wrapped.ReturnPlan?.FrameMinus10StoreAddress == 0xFFFFFFF3
            && wrapped.ReturnPlan?.FrameMinus10StoreValue == 1
            && wrapped.ReturnPlan?.FrameMinus8UnsignedGreaterThanEntryEsi == false
            && wrapped.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferReturnProjection.NormalBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityTransferLifecycleCapture CompleteCapture(
        uint entryEbx = 0x3000,
        uint entryEbp = 0x4000,
        uint transferEdi = 3,
        bool? helperReturnedNormally = true,
        uint? returnEbx = null,
        uint? returnEbp = null,
        uint returnEdi = 0x10,
        uint returnEsi = 0x20,
        uint? helperReturnEax = 0x20,
        uint? receiver4 = 0,
        uint? frameMinus8 = 0x10) =>
        new(
            Transfer: new(
                EntryEbx: entryEbx,
                EntryEbp: entryEbp,
                EntryEdi: transferEdi,
                Receiver4: 5,
                Receiver8: 8,
                FrameMinus4: 0x20,
                FrameMinusC: 0x30,
                HelperReturnedNormally: helperReturnedNormally),
            Return: new(
                ValidatedNormalEntry: false,
                EntryEbx: returnEbx ?? entryEbx,
                EntryEbp: returnEbp ?? entryEbp,
                EntryEdi: returnEdi,
                EntryEsi: returnEsi,
                HelperReturnEax: helperReturnEax,
                Receiver4: receiver4,
                FrameMinus8: frameMinus8));
}
