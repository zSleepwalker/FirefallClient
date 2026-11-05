using Firefall.Reconstruction.Startup;

internal static class FrameHandoffSlotCallPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffSlotCallPublicationPlan direct =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 0x12345678, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    receiver: 0x7000,
                    ebx: 4,
                    edi: 4,
                    storeBase: 0x20,
                    freshBase: 0x30,
                    slotDword: 0xFFFFFFFE)));
        check("The zero-slot direct nonzero call routes EAX through ECX and composes native slot registers",
            direct.Outcome == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && direct.SlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && direct.SlotPlan.SelectedDestinationRawFileOffset == 0x00BE64F2
            && direct.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
            && direct.PublicationPlan?.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && direct.NativeEsiReceiver == 0x7000
            && direct.NativeEbxSlotByteOffset == 4
            && direct.NativeEdiWrappingReceiverPlus0C == 4
            && direct.KnownEcxAfterDirectCall == 0x12345678
            && direct.PublicationEcx == 0x12345678
            && direct.PublicationPlan?.StoreWriteProjected == true
            && direct.PublicationPlan?.StoreWriteAddress == 0x24
            && direct.PublicationPlan?.StoreWriteValue == 0x12345678
            && direct.PublicationPlan?.FreshBaseReadValue == 0x30
            && direct.PublicationPlan?.SlotReadAddress == 0x34
            && direct.PublicationPlan?.ComputedEcxBeforeTest == 0xFFFFFFFE
            && direct.PublicationReceiverIdentityMatches
            && direct.PublicationEbxIdentityMatches
            && direct.PublicationEdiIdentityMatches
            && NoUnreachedCaptures(direct));

        FrameHandoffSlotCallPublicationPlan zeroCall =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 0, true, 0xAABBCCDD),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    receiver: 0x7000,
                    ebx: 4,
                    edi: 4,
                    storeBase: 0x40,
                    freshBase: 0x50,
                    slotDword: 0)));
        check("The zero-EAX indirect-call route publishes using captured post-call ECX",
            zeroCall.Outcome == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && zeroCall.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.ZeroRouteReturnedNormally
            && zeroCall.CapturedZeroRoutePostCallEcx == 0xAABBCCDD
            && zeroCall.PublicationEcx == 0xAABBCCDD
            && zeroCall.PublicationPlan?.StoreWriteValue == 0xAABBCCDD
            && zeroCall.PublicationPlan?.StoreWriteAddress == 0x44
            && NoUnreachedCaptures(zeroCall));

        FrameHandoffSlotCallPublicationPlan skippedNonzero =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0x80000000),
                Call: new(false, null, null, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    receiver: 0x7000,
                    ebx: 4,
                    edi: 4,
                    storeBase: null,
                    freshBase: 0x60,
                    slotDword: 0x1234)));
        check("The 650E nonzero-slot route bypasses the call and store but continues with native identities",
            skippedNonzero.Outcome == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && skippedNonzero.SlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches
            && skippedNonzero.SlotPlan.SelectedDestinationRawFileOffset == 0x00BE650E
            && skippedNonzero.CallPlan is null
            && skippedNonzero.PublicationPlan?.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && skippedNonzero.PublicationPlan?.StoreInstructionReached == false
            && skippedNonzero.PublicationPlan?.StoreWriteProjected == false
            && skippedNonzero.PublicationPlan?.StoreWriteAddress is null
            && skippedNonzero.PublicationPlan?.StoreWriteValue is null
            && skippedNonzero.NativeEbxSlotByteOffset == 4
            && skippedNonzero.NativeEdiWrappingReceiverPlus0C == 4
            && skippedNonzero.PublicationPlan?.FreshBaseReadValue == 0x60
            && skippedNonzero.PublicationPlan?.SlotReadAddress == 0x64
            && NoUnreachedCaptures(skippedNonzero));

        FrameHandoffSlotCallPublicationPlan badNonzeroCaptures =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 1),
                Call: new(true, true, 9, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    receiver: 0x7000,
                    ebx: 4,
                    edi: 4,
                    storeBase: 0x20,
                    freshBase: 0x60,
                    slotDword: 0x1234) with { IncomingEcx = 9 }));
        check("Unreached nonzero-route call and store captures are rejected before publication reads",
            badNonzeroCaptures.Outcome
                == FrameHandoffSlotCallPublicationOutcome
                    .NonzeroSlotRouteRejectedUnreachedCaptures
            && badNonzeroCaptures.CallPlan is null
            && badNonzeroCaptures.PublicationPlan is null
            && badNonzeroCaptures.UnreachedCallEntryValidationProvided
            && badNonzeroCaptures.UnreachedDirectCallReturnCaptureProvided
            && badNonzeroCaptures.UnreachedDirectCallReturnEaxCaptureProvided
            && badNonzeroCaptures.UnreachedPublicationIncomingEcxCaptureProvided
            && badNonzeroCaptures.UnreachedPublicationStoreBase6508CaptureProvided
            && badNonzeroCaptures.UnreachedPublicationFreshBase650EProvided
            && badNonzeroCaptures.UnreachedPublicationSlotDwordProvided
            && badNonzeroCaptures.StopAtRawFileOffsetExclusive == 0x00BE650E);

        FrameHandoffSlotCallPublicationPlan missingCall =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, null, null, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 7)));
        FrameHandoffSlotCallPublicationPlan nonreturningDirectCall =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, false, 0x88, true, 0x99),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 7)));
        FrameHandoffSlotCallPublicationPlan nonreturningZeroRouteCall =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 0, false, 0x99),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 7)));
        check("Missing and nonreturning calls preserve their partial plans and block publication",
            missingCall.Outcome == FrameHandoffSlotCallPublicationOutcome.CallProjectionBlocked
            && missingCall.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnCapture
            && missingCall.PublicationPlan is null
            && missingCall.UnreachedPublicationStoreBase6508CaptureProvided
            && nonreturningDirectCall.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectCallDidNotReturnNormally
            && nonreturningDirectCall.PublicationPlan is null
            && nonreturningDirectCall.CallPlan?.DirectCallReturnedNormally == false
            && nonreturningZeroRouteCall.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.ZeroRouteIndirectCallDidNotReturnNormally
            && nonreturningZeroRouteCall.PublicationPlan is null
            && nonreturningZeroRouteCall.CallPlan?.ZeroRouteIndirectCallReturnedNormally == false);

        FrameHandoffSlotCallPublicationPlan partialSlot =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: new(0x7000, 2, 1, 0xA5, 3, 0x9000, null),
                Call: new(true, true, 5, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 7) with { IncomingEcx = 5 }));
        check("An incomplete slot projection is retained and prevents both later projections",
            partialSlot.Outcome == FrameHandoffSlotCallPublicationOutcome.SlotProjectionBlocked
            && partialSlot.SlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.MissingFirstSlotDwordCapture
            && partialSlot.CallPlan is null
            && partialSlot.PublicationPlan is null
            && partialSlot.UnreachedDirectCallReturnCaptureProvided
            && partialSlot.UnreachedDirectCallReturnEaxCaptureProvided
            && partialSlot.UnreachedPublicationIncomingEcxCaptureProvided
            && partialSlot.UnreachedPublicationStoreBase6508CaptureProvided
            && partialSlot.UnreachedPublicationFreshBase650EProvided
            && partialSlot.UnreachedPublicationSlotDwordProvided
            && partialSlot.StopAtRawFileOffsetExclusive == 0x00BE64EC);

        FrameHandoffSlotCallPublicationPlan unreachedCallFlag =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 5, true, 6),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 7)));
        check("A nominally successful direct call with supplied zero-route captures is blocked by its unreached flags",
            unreachedCallFlag.Outcome
                == FrameHandoffSlotCallPublicationOutcome.CallHasUnreachedCaptures
            && unreachedCallFlag.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
            && unreachedCallFlag.CallPlan?.UnreachedZeroRouteIndirectCallReturnCaptureProvided == true
            && unreachedCallFlag.CallPlan?.UnreachedZeroRoutePostCallEcxCaptureProvided == true
            && unreachedCallFlag.PublicationPlan is null
            && unreachedCallFlag.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            && unreachedCallFlag.UnreachedZeroRoutePostCallEcxCaptureProvided);

        FrameHandoffSlotCallPublicationPlan receiverMismatch =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 5, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7001, 4, 4, 0x20, 0x30, 7)));
        FrameHandoffSlotCallPublicationPlan ebxMismatch =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 5, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 0x9004, 4, 0x20, 0x30, 7)));
        FrameHandoffSlotCallPublicationPlan ediMismatch =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 5, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 5, 0x20, 0x30, 7)));
        check("Receiver, EBX, and EDI identities are validated before any publication write is projected",
            receiverMismatch.Outcome
                == FrameHandoffSlotCallPublicationOutcome.PublicationReceiverIdentityMismatch
            && !receiverMismatch.PublicationReceiverIdentityMatches
            && receiverMismatch.PublicationPlan is null
            && receiverMismatch.UnreachedPublicationStoreBase6508CaptureProvided
            && ebxMismatch.Outcome
                == FrameHandoffSlotCallPublicationOutcome.PublicationEbxIdentityMismatch
            && ebxMismatch.PublicationReceiverIdentityMatches
            && !ebxMismatch.PublicationEbxIdentityMatches
            && ebxMismatch.PublicationPlan is null
            && ediMismatch.Outcome
                == FrameHandoffSlotCallPublicationOutcome.PublicationEdiIdentityMismatch
            && ediMismatch.PublicationReceiverIdentityMatches
            && ediMismatch.PublicationEbxIdentityMatches
            && !ediMismatch.PublicationEdiIdentityMatches
            && ediMismatch.PublicationPlan is null);

        FrameHandoffSlotCallPublicationPlan partialPublication =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0),
                Call: new(true, true, 0xCAFEBABE, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, null, 0x55)));
        check("A publication store remains in the partial plan when the independent fresh reload is missing",
            partialPublication.Outcome
                == FrameHandoffSlotCallPublicationOutcome.PublicationProjectionBlocked
            && partialPublication.PublicationPlan?.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture
            && partialPublication.PublicationPlan?.StoreWriteProjected == true
            && partialPublication.PublicationPlan?.StoreWriteAddress == 0x24
            && partialPublication.PublicationPlan?.StoreWriteValue == 0xCAFEBABE
            && partialPublication.PublicationPlan?.FreshBaseReadReached == true
            && partialPublication.PublicationPlan?.FreshBaseCaptured == false
            && partialPublication.UnreachedPublicationSlotDwordProvided);

        FrameHandoffSlotCallPublicationPlan independentReloads =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: Slot(firstSlotDword: 0, receiverPlus04: 0x9000),
                Call: new(true, true, 1, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    0x7000, 4, 4, 0x20, 0x30, 0x1234)));
        check("The helper's receiver+4 value is not equated with either independent publication reload",
            independentReloads.Outcome
                == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && independentReloads.SlotPlan.ReceiverPlus04ReadValue == 0x9000
            && independentReloads.PublicationPlan?.StoreBaseReadValue == 0x20
            && independentReloads.PublicationPlan?.FreshBaseReadValue == 0x30
            && independentReloads.PublicationPlan?.StoreWriteAddress == 0x24
            && independentReloads.PublicationPlan?.SlotReadAddress == 0x34);

        FrameHandoffSlotCallPublicationPlan wrapping =
            FrameHandoffSlotCallPublicationProjection.Project(new(
                Slot: new(
                    ReceiverBaseEsi: uint.MaxValue,
                    ReceiverPlus08: 0,
                    ReceiverPlus10: uint.MaxValue,
                    ReceiverPlus0COld: 0,
                    ReceiverPlus0CFresh: 0,
                    ReceiverPlus04: 5,
                    FirstSlotDword: 0),
                Call: new(true, true, uint.MaxValue, null, null),
                Publication: Publication(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    uint.MaxValue,
                    0xFFFFFFFC,
                    uint.MaxValue,
                    8,
                    12,
                    uint.MaxValue)));
        check("The derived EBX byte offset, EDI handoff, and later publication address math wrap independently",
            wrapping.Outcome == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && wrapping.SlotPlan.FirstSlotIndex == 0x3FFFFFFF
            && wrapping.SlotPlan.FirstSlotByteOffset == 0xFFFFFFFC
            && wrapping.NativeEbxSlotByteOffset == 0xFFFFFFFC
            && wrapping.NativeEdiWrappingReceiverPlus0C == uint.MaxValue
            && wrapping.PublicationEcx == uint.MaxValue
            && wrapping.PublicationPlan?.StoreWriteAddress == 4
            && wrapping.PublicationPlan?.SlotReadAddress == 8
            && wrapping.PublicationPlan?.ComputedEcxBeforeTest == 11);
    }

    private static FrameHandoffHelperSlotCapture Slot(
        uint firstSlotDword,
        uint receiverPlus04 = 0x9000) =>
        new(
            ReceiverBaseEsi: 0x7000,
            ReceiverPlus08: 2,
            ReceiverPlus10: 1,
            ReceiverPlus0COld: 0xA5,
            ReceiverPlus0CFresh: 3,
            ReceiverPlus04: receiverPlus04,
            FirstSlotDword: firstSlotDword);

    private static FrameHandoffSlotPublicationCapture Publication(
        FrameHandoffSlotPublicationEntry entry,
        uint receiver,
        uint ebx,
        uint edi,
        uint? storeBase,
        uint? freshBase,
        uint? slotDword) =>
        new(
            Entry: entry,
            ReceiverBaseEsi: receiver,
            SlotArrayBaseEbx: ebx,
            IncomingEdi: edi,
            IncomingEcx: null,
            ReceiverPlus04At6508: storeBase,
            ReceiverPlus04At650E: freshBase,
            SlotDwordAtEbxPlusFreshBase: slotDword);

    private static bool NoUnreachedCaptures(
        FrameHandoffSlotCallPublicationPlan plan) =>
        !plan.UnreachedCallEntryValidationProvided
        && !plan.UnreachedDirectCallReturnCaptureProvided
        && !plan.UnreachedDirectCallReturnEaxCaptureProvided
        && !plan.UnreachedZeroRouteIndirectCallReturnCaptureProvided
        && !plan.UnreachedZeroRoutePostCallEcxCaptureProvided
        && !plan.UnreachedPublicationIncomingEcxCaptureProvided
        && !plan.UnreachedPublicationStoreBase6508CaptureProvided
        && !plan.UnreachedPublicationFreshBase650EProvided
        && !plan.UnreachedPublicationSlotDwordProvided;
}
