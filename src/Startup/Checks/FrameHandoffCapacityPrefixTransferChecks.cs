using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityPrefixTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityPrefixTransferPlan zero =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x1000, 0x2000, 0, 0, 0x48),
                    transferEdi: 0x12));
        check("The zero-sum success join admits transfer with EDI derived from the prefix shift",
            zero.Outcome == FrameHandoffCapacityPrefixTransferOutcome.ProjectionComplete
            && zero.PrefixCallPlan.SuccessAdmission
            && zero.PrefixCallPlan.PrefixPlan.Outcome
                == FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA
            && !zero.PrefixCallPlan.CallReached
            && zero.TransferEntryEdiIdentityValidated
            && zero.TransferEntryEdiMatches == true
            && zero.TransferPlan?.Capture.EntryEdi == 0x12
            && zero.TransferSuccessAdmission
            && zero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferProjection.NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacityPrefixTransferPlan direct =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x3000, 0x4000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0x4000, true, 7, null),
                    transferEdi: 0x12345678));
        FrameHandoffCapacityPrefixTransferPlan guard =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x5000, 0x6000, 0x40000000, 0, 0x84),
                    call: new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4,
                        0x6000, null, null, true),
                    transferEdi: 0x87654321));
        check("Direct and guard routes accept independent post-call EDI captures",
            direct.Outcome == FrameHandoffCapacityPrefixTransferOutcome.ProjectionComplete
            && direct.PrefixCallPlan.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.DirectCallB5
            && direct.PrefixCallPlan.SuccessAdmission
            && direct.TransferPlan?.Capture.EntryEdi == 0x12345678
            && !direct.TransferEntryEdiIdentityValidated
            && direct.TransferEntryEdiMatches is null
            && direct.TransferSuccessAdmission
            && guard.Outcome == FrameHandoffCapacityPrefixTransferOutcome.ProjectionComplete
            && guard.PrefixCallPlan.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && guard.PrefixCallPlan.SuccessAdmission
            && guard.TransferPlan?.Capture.EntryEdi == 0x87654321
            && !guard.TransferEntryEdiIdentityValidated
            && guard.TransferEntryEdiMatches is null
            && guard.TransferSuccessAdmission);

        FrameHandoffCapacityPrefixTransferCapture unreachedCallCapture =
            CompleteCapture(
                prefix: new(0x7000, 0x8000, 0, 0, 4),
                call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                    0x8000, true, 1, true));
        FrameHandoffCapacityPrefixTransferPlan unreachedCall =
            FrameHandoffCapacityPrefixTransferProjection.Project(unreachedCallCapture);
        check("A zero-route call capture blocks transfer and preserves every supplied downstream flag",
            unreachedCall.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.PrefixCallBlocked
            && unreachedCall.PrefixCallPlan
                .Outcome == FrameHandoffCapacityPrefixCallOutcome.PrefixZeroRejectedUnreachedCallCaptures
            && unreachedCall.PrefixCallPlan.UnreachedCallEntryRouteCaptureProvided
            && unreachedCall.PrefixCallPlan.UnreachedDirectCallReturnCaptureProvided
            && unreachedCall.PrefixCallPlan.UnreachedDirectCallReturnEaxCaptureProvided
            && unreachedCall.PrefixCallPlan.UnreachedGuardIndirectCallReturnCaptureProvided
            && unreachedCall.TransferPlan is null
            && unreachedCall.UnreachedTransferReceiver4CaptureProvided
            && unreachedCall.UnreachedTransferReceiver8CaptureProvided
            && unreachedCall.UnreachedTransferFrameMinus4CaptureProvided
            && unreachedCall.UnreachedTransferFrameMinusCCaptureProvided
            && unreachedCall.UnreachedTransferNormalCallCaptureProvided
            && !unreachedCall.TransferAdmissionChecked
            && !unreachedCall.TransferSuccessAdmission);

        FrameHandoffCapacityPrefixTransferPlan ebxMismatch =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x9000, 0xA000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0xA000, true, 1, null),
                    transferEbx: 0x9001));
        FrameHandoffCapacityPrefixTransferPlan ebpMismatch =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0xB000, 0xC000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0xC000, true, 1, null),
                    transferEbp: 0xC001));
        check("Transfer EBX or EBP identity mismatch blocks before any transfer read or frame write",
            ebxMismatch.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.TransferEntryIdentityMismatch
            && ebxMismatch.TransferEntryIdentitiesValidated
            && !ebxMismatch.TransferEntryEbxMatches
            && ebxMismatch.TransferEntryEbpMatches
            && ebxMismatch.TransferPlan is null
            && ebxMismatch.UnreachedTransferReceiver4CaptureProvided
            && ebxMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferProjection.EntryRawFileOffset
            && ebpMismatch.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.TransferEntryIdentityMismatch
            && ebpMismatch.TransferEntryIdentitiesValidated
            && ebpMismatch.TransferEntryEbxMatches
            && !ebpMismatch.TransferEntryEbpMatches
            && ebpMismatch.TransferPlan is null
            && ebpMismatch.UnreachedTransferReceiver4CaptureProvided);

        FrameHandoffCapacityPrefixTransferPlan zeroEdiMismatch =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0xD000, 0xE000, 0, 0, 0x48),
                    transferEdi: 0x13));
        check("The zero route rejects a mismatched derived EDI before transfer projection",
            zeroEdiMismatch.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.ZeroRouteEntryEdiMismatch
            && zeroEdiMismatch.TransferEntryEdiIdentityValidated
            && zeroEdiMismatch.TransferEntryEdiMatches == false
            && zeroEdiMismatch.TransferPlan is null
            && zeroEdiMismatch.UnreachedTransferReceiver4CaptureProvided
            && !zeroEdiMismatch.TransferAdmissionChecked);

        FrameHandoffCapacityPrefixTransferPlan independentReads =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0xF000, 0x10000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0x10000, true, 1, null),
                    transferEdi: 0xA,
                    frameMinus4: 0x11111111,
                    frameMinusC: 0x22222222));
        check("Transfer frame reads remain independent captures rather than reusing the prior store",
            independentReads.TransferSuccessAdmission
            && independentReads.TransferPlan?.FrameWrites.Single()
                == new FrameHandoffCapacityTransferFrameWrite(
                    FrameHandoffCapacityTransferProjection.FrameMinusCWriteRawFileOffset,
                    0xFFF4,
                    0x28)
            && independentReads.TransferPlan?.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferRead(0x00BE6BCA, 0xF004, 5),
                new FrameHandoffCapacityTransferRead(0x00BE6BDA, 0xF008, 8),
                new FrameHandoffCapacityTransferRead(0x00BE6BEE, 0xFFFC, 0x11111111),
                new FrameHandoffCapacityTransferRead(0x00BE6BF1, 0xFFF4, 0x22222222)
            }) == true
            && independentReads.TransferPlan?.FrameWrites[0].Value != 0x22222222);

        FrameHandoffCapacityPrefixTransferPlan partialTransfer =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x11000, 0x12000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0x12000, true, 1, null),
                    transferEdi: 3,
                    receiver8: null,
                    frameMinus4: 0x20,
                    frameMinusC: 0x30,
                    helperReturnedNormally: true));
        FrameHandoffCapacityPrefixTransferPlan nonreturningTransfer =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(0x13000, 0x14000, 1, 0, 0x40),
                    call: new(FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                        0x14000, true, 1, null),
                    transferEdi: 3,
                    helperReturnedNormally: false));
        check("Partial and nonreturning transfers retain writes while blocking first-transfer admission",
            partialTransfer.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.TransferHasUnreachedCaptures
            && partialTransfer.TransferPlan?.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingReceiver8Capture
            && partialTransfer.TransferPlan?.FrameWrites.Length == 1
            && partialTransfer.TransferPlan?.Reads.Length == 2
            && partialTransfer.UnreachedTransferFrameMinus4CaptureProvided
            && partialTransfer.UnreachedTransferFrameMinusCCaptureProvided
            && partialTransfer.UnreachedTransferNormalCallCaptureProvided
            && !partialTransfer.TransferSuccessAdmission
            && nonreturningTransfer.Outcome
                == FrameHandoffCapacityPrefixTransferOutcome.TransferBlocked
            && nonreturningTransfer.TransferPlan?.Outcome
                == FrameHandoffCapacityTransferOutcome.HelperDidNotReturnNormally
            && nonreturningTransfer.TransferPlan?.FrameWrites.Length == 1
            && nonreturningTransfer.TransferPlan?.IndirectCallReached == true
            && nonreturningTransfer.TransferPlan?.HelperReturnedNormally == false
            && !nonreturningTransfer.TransferSuccessAdmission);

        FrameHandoffCapacityPrefixTransferPlan wrapped =
            FrameHandoffCapacityPrefixTransferProjection.Project(
                CompleteCapture(
                    prefix: new(uint.MaxValue, 3, 0, 0, 0x48),
                    transferEdi: 0x12,
                    frameMinus4: 0xABCDEF01,
                    frameMinusC: 0x10203040));
        check("Wrapped prefix and transfer identities preserve wrapped read and write addresses",
            wrapped.Outcome == FrameHandoffCapacityPrefixTransferOutcome.ProjectionComplete
            && wrapped.TransferEntryEbxMatches
            && wrapped.TransferEntryEbpMatches
            && wrapped.TransferPlan?.Reads[0].Address == 3
            && wrapped.TransferPlan?.Reads[1].Address == 7
            && wrapped.TransferPlan?.Reads[3].Address == 0xFFFFFFF7
            && wrapped.TransferPlan?.Reads[2].Address == 0xFFFFFFFF
            && wrapped.TransferPlan?.FrameWrites[0].Address == 0xFFFFFFF7
            && wrapped.TransferPlan?.Reads[2].Value == 0xABCDEF01
            && wrapped.TransferPlan?.Reads[3].Value == 0x10203040
            && wrapped.TransferSuccessAdmission);
    }

    private static FrameHandoffCapacityPrefixTransferCapture CompleteCapture(
        FrameHandoffCapacityPrefixCapture prefix,
        FrameHandoffCapacityCallCapture? call = null,
        uint? transferEbx = null,
        uint? transferEbp = null,
        uint transferEdi = 3,
        uint? receiver4 = 5,
        uint? receiver8 = 8,
        uint? frameMinus4 = 0x20,
        uint? frameMinusC = 0x30,
        bool? helperReturnedNormally = true) =>
        new(
            new(prefix, call),
            new(
                EntryEbx: transferEbx ?? prefix.EntryEbx,
                EntryEbp: transferEbp ?? prefix.EntryEbp,
                EntryEdi: transferEdi,
                Receiver4: receiver4,
                Receiver8: receiver8,
                FrameMinus4: frameMinus4,
                FrameMinusC: frameMinusC,
                HelperReturnedNormally: helperReturnedNormally));
}
