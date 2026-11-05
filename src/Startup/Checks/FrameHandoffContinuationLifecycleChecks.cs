using Firefall.Reconstruction.Startup;

internal static class FrameHandoffContinuationLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffContinuationLifecyclePlan direct =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0, receiverPlus04: 0x90),
                    Call: new(true, true, 0x12345678, null, null),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        receiver: 0x7000,
                        ebx: 4,
                        edi: 4,
                        storeBase: 0x10,
                        freshBase: 0x30,
                        slotDword: 0x5004)),
                Final: Final(
                    ecx: 0x5004,
                    esi: 0x7000,
                    ebp: 0x3000,
                    frame8Pointer: 0x6000,
                    pointee: 0xCAFEBABE,
                    receiver10: 7)));
        check("The direct nonzero call's effective ECX composes through publication and the complete return",
            direct.Outcome == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && direct.SlotCallPublicationPlan.Outcome
                == FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
            && direct.SlotCallPublicationPlan.Capture.Publication.IncomingEcx is null
            && direct.SlotCallPublicationPlan.PublicationPlan?.Capture.IncomingEcx
                == 0x12345678
            && direct.ReturnPlan?.Capture.Publication.IncomingEcx == 0x12345678
            && direct.ReturnPlan?.Outcome
                == FrameHandoffPublicationReturnOutcome.ProjectionComplete
            && direct.ReturnPlan.Value.FinalPlan is
                { Outcome: FrameHandoffFinalWriteReturnOutcome.ReturnedNormally }
            && direct.ReturnPlan.Value.FinalPlan.Value.FinalWriteAddress == 0x5004
            && direct.ReturnPlan.Value.FinalPlan.Value.FinalWriteValue == 0xCAFEBABE
            && direct.ReturnPlan.Value.FinalPlan.Value.CounterValueAfterIncrement == 8
            && direct.StopAtRawFileOffsetExclusive
                == FrameHandoffContinuationLifecycleProjection
                    .ReturnBoundaryRawFileOffsetExclusive
            && NoUnreachedCaptures(direct));

        FrameHandoffContinuationLifecyclePlan indirect =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0),
                    Call: new(true, true, 0, true, 0xAABBCCDD),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        0x7000, 4, 4, 0x20, 0x40, 0x6000)),
                Final: Final(
                    0x6000, 0x7000, 0x3100, 0x6100, 0xABCD, 1)));
        check("The indirect route uses its opaque post-call ECX rather than the direct call's zero EAX",
            indirect.Outcome == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && indirect.SlotCallPublicationPlan.CallPlan?.CapturedDirectCallReturnEax == 0
            && indirect.SlotCallPublicationPlan.CallPlan?.CapturedZeroRoutePostCallEcx
                == 0xAABBCCDD
            && indirect.SlotCallPublicationPlan.PublicationPlan?.Capture.IncomingEcx
                == 0xAABBCCDD
            && indirect.ReturnPlan?.PublicationPlan.StoreWriteValue == 0xAABBCCDD
            && indirect.ReturnPlan.Value.FinalPlan?.FinalWriteValue == 0xABCD
            && NoUnreachedCaptures(indirect));

        FrameHandoffContinuationLifecyclePlan existingNonzeroSlot =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0x80000000),
                    Call: new(false, null, null, null, null),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                        0x7000, 4, 4, null, 0x40, 0x5004)),
                Final: Final(
                    0x5004, 0x7000, 0x3200, 0x6200, 0x10203040, 3)));
        check("The nonzero slot bypasses both call routes and publication storage but completes the return",
            existingNonzeroSlot.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && existingNonzeroSlot.SlotCallPublicationPlan.SlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches
            && existingNonzeroSlot.SlotCallPublicationPlan.CallPlan is null
            && existingNonzeroSlot.ReturnPlan?.PublicationPlan.StoreWriteProjected == false
            && existingNonzeroSlot.ReturnPlan.Value.FinalPlan?.FinalWriteValue == 0x10203040
            && NoUnreachedCaptures(existingNonzeroSlot));

        FrameHandoffContinuationLifecyclePlan missingCall =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0),
                    Call: new(true, null, null, null, null),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        0x7000, 4, 4, 0x20, 0x30, 0x5000)),
                Final: Final(
                    0x5004, 0x7000, 0x3300, 0x6300, 0x55, 9)));
        FrameHandoffContinuationLifecyclePlan flaggedCall =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0),
                    Call: new(true, true, 5, true, 6),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        0x7000, 4, 4, 0x20, 0x30, 0x5000)),
                Final: Final(
                    0x5004, 0x7000, 0x3400, 0x6400, 0x66, 10)));
        check("Missing calls and successful-but-flagged calls retain the first partial plan and block return composition",
            missingCall.Outcome
                == FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked
            && missingCall.SlotCallPublicationPlan.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnCapture
            && missingCall.SlotCallPublicationPlan.PublicationPlan is null
            && missingCall.ReturnPlan is null
            && missingCall.UnreachedFinalFrame8PointerCaptureProvided
            && missingCall.UnreachedFinalPointeeDwordCaptureProvided
            && missingCall.UnreachedFinalReceiver10OldValueCaptureProvided
            && flaggedCall.Outcome
                == FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked
            && flaggedCall.SlotCallPublicationPlan.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
            && flaggedCall.SlotCallPublicationPlan.UnreachedZeroRoutePostCallEcxCaptureProvided
            && flaggedCall.SlotCallPublicationPlan.PublicationPlan is null
            && flaggedCall.ReturnPlan is null
            && flaggedCall.UnreachedFinalFrame8PointerCaptureProvided
            && flaggedCall.UnreachedFinalPointeeDwordCaptureProvided
            && flaggedCall.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffContinuationLifecyclePlan partialStore =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0),
                    Call: new(true, true, 0xCAFEBABE, null, null),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        0x7000, 4, 4, 0x20, null, 0x5555)),
                Final: Final(
                    0x5559, 0x7000, 0x3500, 0x6500, 0x77, 11)));
        check("A partial publication preserves its completed store and marks every supplied final read capture unreached",
            partialStore.Outcome
                == FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked
            && partialStore.SlotCallPublicationPlan.PublicationPlan?.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture
            && partialStore.SlotCallPublicationPlan.PublicationPlan.Value.StoreWriteProjected
            && partialStore.SlotCallPublicationPlan.PublicationPlan.Value.StoreWriteAddress == 0x24
            && partialStore.SlotCallPublicationPlan.PublicationPlan.Value.StoreWriteValue
                == 0xCAFEBABE
            && partialStore.ReturnPlan is null
            && partialStore.UnreachedFinalFrame8PointerCaptureProvided
            && partialStore.UnreachedFinalPointeeDwordCaptureProvided
            && partialStore.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffContinuationLifecyclePlan independentReloads =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: new(
                    Slot: Slot(firstSlotDword: 0, receiverPlus04: 0x9000),
                    Call: new(true, true, 0x4000, null, null),
                    Publication: Publication(
                        FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                        0x7000, 4, 4, 0x10, 0x30, 0x5004)),
                Final: Final(
                    0x5004, 0x7000, 0x3600, 0x6600, 0x88, 12)));
        check("The helper value and both publication reloads remain independent captures",
            independentReloads.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && independentReloads.SlotCallPublicationPlan.SlotPlan.ReceiverPlus04ReadValue
                == 0x9000
            && independentReloads.ReturnPlan?.PublicationPlan.StoreBaseReadValue == 0x10
            && independentReloads.ReturnPlan?.PublicationPlan.FreshBaseReadValue == 0x30
            && independentReloads.ReturnPlan?.PublicationPlan.StoreWriteAddress == 0x14
            && independentReloads.ReturnPlan?.PublicationPlan.SlotReadAddress == 0x34);

        FrameHandoffContinuationLifecyclePlan ecxMismatch =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: SuccessfulStorePublication(0x8000),
                Final: Final(
                    0x8001, 0x7000, 0x3700, 0x6700, 0x99, 13)));
        FrameHandoffContinuationLifecyclePlan esiMismatch =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: SuccessfulStorePublication(0x8000),
                Final: Final(
                    0x8000, 0x7001, 0x3800, 0x6800, 0xAA, 14)));
        check("Final ECX and ESI mismatches are rejected by publication-return before final reads, write, or increment",
            ecxMismatch.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ReturnProjectionBlocked
            && ecxMismatch.ReturnPlan?.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalEntryEcxMismatch
            && ecxMismatch.ReturnPlan.Value.FinalPlan is null
            && ecxMismatch.ReturnPlan.Value.PublicationPlan.StoreWriteProjected
            && ecxMismatch.UnreachedFinalFrame8PointerCaptureProvided
            && ecxMismatch.UnreachedFinalPointeeDwordCaptureProvided
            && ecxMismatch.UnreachedFinalReceiver10OldValueCaptureProvided
            && esiMismatch.ReturnPlan?.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalEntryEsiMismatch
            && esiMismatch.ReturnPlan.Value.FinalPlan is null
            && esiMismatch.UnreachedFinalFrame8PointerCaptureProvided
            && esiMismatch.UnreachedFinalPointeeDwordCaptureProvided
            && esiMismatch.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffContinuationLifecyclePlan partialFinalWrite =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: SuccessfulStorePublication(0x9004),
                Final: Final(
                    0x9004, 0x7000, 0x3900, 0x6900, 0xBEEF, null)));
        check("A missing counter capture retains the final write while blocking only counter completion",
            partialFinalWrite.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ReturnProjectionBlocked
            && partialFinalWrite.ReturnPlan?.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalProjectionBlocked
            && partialFinalWrite.ReturnPlan.Value.FinalPlan is
                { Outcome: FrameHandoffFinalWriteReturnOutcome.MissingReceiver10OldValueCapture }
            && partialFinalWrite.ReturnPlan.Value.FinalPlan.Value.FinalWriteReached
            && partialFinalWrite.ReturnPlan.Value.FinalPlan.Value.FinalWriteAddress == 0x9004
            && partialFinalWrite.ReturnPlan.Value.FinalPlan.Value.FinalWriteValue == 0xBEEF
            && partialFinalWrite.ReturnPlan.Value.FinalPlan.Value.CounterIncrementReached
            && !partialFinalWrite.UnreachedFinalFrame8PointerCaptureProvided
            && !partialFinalWrite.UnreachedFinalPointeeDwordCaptureProvided
            && !partialFinalWrite.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffContinuationLifecyclePlan counterWrap =
            FrameHandoffContinuationLifecycleProjection.Project(new(
                SlotCallPublication: SuccessfulExistingSlot(0x5004),
                Final: Final(
                    0x5004, 0x7000, 0x3A00, 0x6A00, 0x1234, uint.MaxValue)));
        check("The complete composed return preserves the receiver counter's DWORD wrap",
            counterWrap.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && counterWrap.ReturnPlan?.FinalPlan?.CounterValueBeforeIncrement == uint.MaxValue
            && counterWrap.ReturnPlan?.FinalPlan?.CounterValueAfterIncrement == 0
            && counterWrap.ReturnPlan?.FinalPlan?.ReturnReached == true);
    }

    private static FrameHandoffSlotCallPublicationCapture SuccessfulStorePublication(
        uint finalEcx) =>
        new(
            Slot: Slot(firstSlotDword: 0),
            Call: new(true, true, 0x4000, null, null),
            Publication: Publication(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0x7000, 4, 4, 0x20, 0x30, finalEcx));

    private static FrameHandoffSlotCallPublicationCapture SuccessfulExistingSlot(
        uint finalEcx) =>
        new(
            Slot: Slot(firstSlotDword: 1),
            Call: new(false, null, null, null, null),
            Publication: Publication(
                FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                0x7000, 4, 4, null, 0x30, finalEcx));

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

    private static FrameHandoffFinalWriteReturnCapture Final(
        uint ecx,
        uint esi,
        uint ebp,
        uint? frame8Pointer,
        uint? pointee,
        uint? receiver10) =>
        new(
            EntryEcx: ecx,
            EntryEsi: esi,
            EntryEbp: ebp,
            Frame8Pointer: frame8Pointer,
            PointeeDword: pointee,
            Receiver10OldValue: receiver10);

    private static bool NoUnreachedCaptures(
        FrameHandoffContinuationLifecyclePlan plan) =>
        !plan.SlotCallPublicationPlan.UnreachedCallEntryValidationProvided
        && !plan.SlotCallPublicationPlan.UnreachedDirectCallReturnCaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedDirectCallReturnEaxCaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedZeroRouteIndirectCallReturnCaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedZeroRoutePostCallEcxCaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedPublicationIncomingEcxCaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedPublicationStoreBase6508CaptureProvided
        && !plan.SlotCallPublicationPlan.UnreachedPublicationFreshBase650EProvided
        && !plan.SlotCallPublicationPlan.UnreachedPublicationSlotDwordProvided
        && !plan.UnreachedFinalFrame8PointerCaptureProvided
        && !plan.UnreachedFinalPointeeDwordCaptureProvided
        && !plan.UnreachedFinalReceiver10OldValueCaptureProvided;
}
