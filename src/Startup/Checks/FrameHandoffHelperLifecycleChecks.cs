using Firefall.Reconstruction.Startup;

internal static class FrameHandoffHelperLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffHelperLifecyclePlan lowTwoRoute =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x1111, 0x7000, 1, 0, null, null),
                Continuation: CompleteContinuation(0x7000)));
        FrameHandoffHelperLifecyclePlan greaterRoute =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x2222, 0x7100, uint.MaxValue, 1, 2, null),
                Continuation: CompleteContinuation(0x7100)));
        FrameHandoffHelperLifecyclePlan helperReturnedRoute =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x3333, 0x7200, 0, 0, 0, true),
                Continuation: CompleteContinuation(0x7200)));
        check("Exactly the three successful entry routes compose through the complete continuation",
            lowTwoRoute.Outcome == FrameHandoffHelperLifecycleOutcome.ProjectionComplete
            && lowTwoRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            && lowTwoRoute.ContinuationPlan?.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && greaterRoute.Outcome == FrameHandoffHelperLifecycleOutcome.ProjectionComplete
            && greaterRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength
            && greaterRoute.ContinuationPlan?.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && helperReturnedRoute.Outcome
                == FrameHandoffHelperLifecycleOutcome.ProjectionComplete
            && helperReturnedRoute.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.HelperReturnedNormally
            && helperReturnedRoute.ContinuationPlan?.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ProjectionComplete
            && NoUnreachedCaptures(lowTwoRoute)
            && NoUnreachedCaptures(greaterRoute)
            && NoUnreachedCaptures(helperReturnedRoute));

        FrameHandoffHelperLifecyclePlan missingEntryRead =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x4444, 0x7300, null, 0, 0, true),
                Continuation: CompleteContinuation(0x7300)));
        FrameHandoffHelperLifecyclePlan missingHelperReturn =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x5555, 0x7400, 0, 0, 0, null),
                Continuation: CompleteContinuation(0x7400)));
        check("Missing entry reads and helper-call results retain the partial entry plan and block continuation projection",
            missingEntryRead.Outcome
                == FrameHandoffHelperLifecycleOutcome.EntryDidNotAdmitContinuation
            && missingEntryRead.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.MissingReceiverPlus0CCapture
            && missingEntryRead.EntryPlan.Capture.ReceiverPlus10 == 0
            && missingEntryRead.ContinuationPlan is null
            && missingEntryRead.UnreachedContinuationReceiverBaseEsiCapture
            && !missingEntryRead.UnreachedContinuationPublicationIncomingEcxCaptureProvided
            && AllOptionalContinuationCapturesUnreached(missingEntryRead)
            && missingHelperReturn.Outcome
                == FrameHandoffHelperLifecycleOutcome.EntryDidNotAdmitContinuation
            && missingHelperReturn.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.MissingHelperReturnCapture
            && missingHelperReturn.EntryPlan.HelperCallReached
            && missingHelperReturn.ContinuationPlan is null
            && missingHelperReturn.UnreachedContinuationCallEntryValidationProvided
            && missingHelperReturn.UnreachedContinuationPublicationStoreBase6508CaptureProvided
            && missingHelperReturn.UnreachedContinuationFinalPointeeDwordCaptureProvided);

        FrameHandoffHelperLifecyclePlan flaggedEntry =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x6666, 0x7500, 1, 0, 0, true),
                Continuation: CompleteContinuation(0x7500)));
        check("Entry admission requires every unreachable-entry capture flag to be clear",
            flaggedEntry.EntryPlan.Outcome
                == FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero
            && flaggedEntry.EntryPlan.UnreachedReceiverPlus08CaptureProvided
            && flaggedEntry.EntryPlan.UnreachedHelperReturnedNormallyCaptureProvided
            && flaggedEntry.Outcome
                == FrameHandoffHelperLifecycleOutcome.EntryUnreachedCaptureWasProvided
            && flaggedEntry.EntryRouteAdmitted
            && !flaggedEntry.EntryUnreachedCapturesClear
            && flaggedEntry.ContinuationPlan is null
            && AllOptionalContinuationCapturesUnreached(flaggedEntry));

        FrameHandoffContinuationLifecycleCapture missingContinuationCallCapture =
            CompleteContinuation(0x7580) with
            {
                SlotCallPublication = CompleteContinuation(0x7580).SlotCallPublication with
                {
                    Call = new(true, null, 0x12345678, null, null)
                }
            };
        FrameHandoffContinuationLifecycleCapture missingContinuationSlotRead =
            CompleteContinuation(0x7590) with
            {
                SlotCallPublication = CompleteContinuation(0x7590).SlotCallPublication with
                {
                    Slot = CompleteContinuation(0x7590).SlotCallPublication.Slot with
                    {
                        ReceiverPlus08 = null
                    }
                }
            };
        FrameHandoffHelperLifecyclePlan continuationCallBlocked =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x6789, 0x7580, 0, 0, 0, true),
                Continuation: missingContinuationCallCapture));
        FrameHandoffHelperLifecyclePlan continuationReadBlocked =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x6790, 0x7590, 0, 0, 0, true),
                Continuation: missingContinuationSlotRead));
        check("Missing continuation call and slot reads retain their partial projections and mark later captures unreached",
            continuationCallBlocked.Outcome
                == FrameHandoffHelperLifecycleOutcome.ContinuationBlocked
            && continuationCallBlocked.ContinuationPlan?.SlotCallPublicationPlan.CallPlan?.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnCapture
            && continuationCallBlocked.ContinuationPlan.Value.SlotCallPublicationPlan
                .PublicationPlan is null
            && continuationCallBlocked.UnreachedContinuationPublicationStoreBase6508CaptureProvided
            && continuationCallBlocked.UnreachedContinuationFinalPointeeDwordCaptureProvided
            && continuationReadBlocked.Outcome
                == FrameHandoffHelperLifecycleOutcome.ContinuationBlocked
            && continuationReadBlocked.ContinuationPlan?.SlotCallPublicationPlan.SlotPlan.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus08Capture
            && continuationReadBlocked.UnreachedContinuationReceiverPlus10CaptureProvided
            && continuationReadBlocked.UnreachedContinuationCallEntryValidationProvided
            && continuationReadBlocked.UnreachedContinuationFinalFrame8PointerCaptureProvided);

        FrameHandoffHelperLifecyclePlan receiverMismatch =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x7777, 0x7600, 0, 0, 0, true),
                Continuation: CompleteContinuation(0x7601)));
        check("The entry receiver identity is checked before any continuation slot write",
            receiverMismatch.Outcome
                == FrameHandoffHelperLifecycleOutcome.ReceiverIdentityMismatch
            && receiverMismatch.ReceiverIdentityChecked
            && !receiverMismatch.ReceiverIdentityMatches
            && receiverMismatch.ContinuationPlan is null
            && !receiverMismatch.UnreachedContinuationReceiverBaseEsiCapture
            && receiverMismatch.UnreachedContinuationReceiverPlus08CaptureProvided
            && receiverMismatch.UnreachedContinuationFirstSlotDwordCaptureProvided
            && AllOptionalContinuationCapturesUnreached(receiverMismatch));

        FrameHandoffHelperLifecyclePlan independentReads =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x8888, 0x7700, 0, 0, 0, true),
                Continuation: CompleteContinuation(
                    0x7700,
                    slotPlus10: 9,
                    slotFreshPlus0C: 0x1234,
                    storeBase: 0x10,
                    freshBase: 0x30)));
        check("Entry, continuation, and publication reloads remain independent captures",
            independentReads.Outcome
                == FrameHandoffHelperLifecycleOutcome.ProjectionComplete
            && independentReads.EntryPlan.Capture.ReceiverPlus10 == 0
            && independentReads.ContinuationPlan?.SlotCallPublicationPlan.SlotPlan
                .ReceiverPlus10ReadValue == 9
            && independentReads.ContinuationPlan?.SlotCallPublicationPlan.SlotPlan
                .ReceiverPlus0CFreshReadValue == 0x1234
            && independentReads.ContinuationPlan?.SlotCallPublicationPlan.NativeEbxSlotByteOffset
                == ExpectedSlotByteOffset(2, unchecked(9u + 0x1234))
            && independentReads.ContinuationPlan?.ReturnPlan?.PublicationPlan.StoreBaseReadValue
                == 0x10
            && independentReads.ContinuationPlan?.ReturnPlan?.PublicationPlan.FreshBaseReadValue
                == 0x30
            && independentReads.ContinuationPlan?.ReturnPlan?.PublicationPlan.ComputedEcxBeforeTest
                == unchecked(0x5004u + ((unchecked(9u + 0x1234) & 3u) * 4u))
            && independentReads.ContinuationPlan?.ReturnPlan?.FinalPlan?.Capture.EntryEcx
                == unchecked(0x5004u + ((unchecked(9u + 0x1234) & 3u) * 4u))
            && NoUnreachedCaptures(independentReads));

        FrameHandoffHelperLifecyclePlan partialStore =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0x9999, 0x7800, 0, 0, 0, true),
                Continuation: CompleteContinuation(
                    0x7800,
                    freshBase: null,
                    slotDword: 0x5555)));
        FrameHandoffHelperLifecyclePlan partialFinalWrite =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0xAAAA, 0x7900, 0, 0, 0, true),
                Continuation: CompleteContinuation(
                    0x7900,
                    finalReceiver10: null)));
        check("Partial continuation stores and final writes are preserved without claiming full completion",
            partialStore.Outcome == FrameHandoffHelperLifecycleOutcome.ContinuationBlocked
            && partialStore.ContinuationPlan?.Outcome
                == FrameHandoffContinuationLifecycleOutcome.SlotCallPublicationBlocked
            && partialStore.ContinuationPlan?.SlotCallPublicationPlan.PublicationPlan is
                { Outcome: FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture,
                    StoreWriteProjected: true }
            && partialStore.ContinuationPlan.Value.ReturnPlan is null
            && partialFinalWrite.Outcome
                == FrameHandoffHelperLifecycleOutcome.ContinuationBlocked
            && partialFinalWrite.ContinuationPlan?.Outcome
                == FrameHandoffContinuationLifecycleOutcome.ReturnProjectionBlocked
            && partialFinalWrite.ContinuationPlan?.ReturnPlan?.FinalPlan is
                { Outcome: FrameHandoffFinalWriteReturnOutcome.MissingReceiver10OldValueCapture,
                    FinalWriteReached: true }
            && partialFinalWrite.ContinuationPlan.Value.ReturnPlan.Value.FinalPlan.Value
                .FinalWriteValue == 0xCAFEBABE);

        FrameHandoffHelperLifecyclePlan counterWrap =
            FrameHandoffHelperLifecycleProjection.Project(new(
                Entry: new(0xBBBB, 0x7A00, 0, 0, 0, true),
                Continuation: CompleteContinuation(0x7A00, finalReceiver10: uint.MaxValue)));
        check("The exact full lifecycle preserves the continuation counter's DWORD wrap",
            counterWrap.Outcome == FrameHandoffHelperLifecycleOutcome.ProjectionComplete
            && counterWrap.ContinuationPlan?.ReturnPlan?.FinalPlan?.CounterValueBeforeIncrement
                == uint.MaxValue
            && counterWrap.ContinuationPlan?.ReturnPlan?.FinalPlan?.CounterValueAfterIncrement == 0
            && counterWrap.ContinuationPlan?.ReturnPlan?.FinalPlan?.ReturnReached == true
            && counterWrap.StopAtRawFileOffsetExclusive
                == FrameHandoffHelperLifecycleProjection.LifecycleEndRawFileOffsetExclusive);
    }

    private static FrameHandoffContinuationLifecycleCapture CompleteContinuation(
        uint receiver,
        uint slotPlus10 = 1,
        uint slotFreshPlus0C = 3,
        uint? storeBase = 0x20,
        uint? freshBase = 0x30,
        uint? slotDword = 0x5004,
        uint? finalReceiver10 = 7)
    {
        const uint receiverPlus08 = 2;
        uint wrappingEdi = unchecked(slotPlus10 + slotFreshPlus0C);
        uint slotIndex = unchecked((receiverPlus08 - 1) & (wrappingEdi >> 2));
        uint slotByteOffset = unchecked(slotIndex * FrameHandoffHelperSlotProjection.SlotScale);
        uint finalEcx = unchecked(
            slotDword.GetValueOrDefault()
            + ((wrappingEdi & 3) * FrameHandoffSlotPublicationProjection.EdiScale));

        return new(
            SlotCallPublication: new(
                Slot: new(
                    ReceiverBaseEsi: receiver,
                    ReceiverPlus08: receiverPlus08,
                    ReceiverPlus10: slotPlus10,
                    ReceiverPlus0COld: 0xFFFFFFFF,
                    ReceiverPlus0CFresh: slotFreshPlus0C,
                    ReceiverPlus04: 0x9000,
                    FirstSlotDword: 0),
                Call: new(true, true, 0x12345678, null, null),
                Publication: new(
                    Entry: FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    ReceiverBaseEsi: receiver,
                    SlotArrayBaseEbx: slotByteOffset,
                    IncomingEdi: wrappingEdi,
                    IncomingEcx: null,
                    ReceiverPlus04At6508: storeBase,
                    ReceiverPlus04At650E: freshBase,
                    SlotDwordAtEbxPlusFreshBase: slotDword)),
            Final: new(
                EntryEcx: finalEcx,
                EntryEsi: receiver,
                EntryEbp: 0x3000,
                Frame8Pointer: 0x6000,
                PointeeDword: 0xCAFEBABE,
                Receiver10OldValue: finalReceiver10));
    }

    private static uint ExpectedSlotByteOffset(uint receiverPlus08, uint wrappingEdi)
    {
        uint slotIndex = unchecked((receiverPlus08 - 1) & (wrappingEdi >> 2));
        return unchecked(slotIndex * FrameHandoffHelperSlotProjection.SlotScale);
    }

    private static bool NoUnreachedCaptures(FrameHandoffHelperLifecyclePlan plan) =>
        !plan.UnreachedContinuationReceiverBaseEsiCapture
        && !plan.UnreachedContinuationReceiverPlus08CaptureProvided
        && !plan.UnreachedContinuationReceiverPlus10CaptureProvided
        && !plan.UnreachedContinuationReceiverPlus0COldCaptureProvided
        && !plan.UnreachedContinuationReceiverPlus0CFreshCaptureProvided
        && !plan.UnreachedContinuationReceiverPlus04CaptureProvided
        && !plan.UnreachedContinuationFirstSlotDwordCaptureProvided
        && !plan.UnreachedContinuationCallEntryValidationProvided
        && !plan.UnreachedContinuationDirectCallReturnCaptureProvided
        && !plan.UnreachedContinuationDirectCallReturnEaxCaptureProvided
        && !plan.UnreachedContinuationZeroRouteIndirectCallReturnCaptureProvided
        && !plan.UnreachedContinuationZeroRoutePostCallEcxCaptureProvided
        && !plan.UnreachedContinuationPublicationBranchEntryCaptureProvided
        && !plan.UnreachedContinuationPublicationIncomingEcxCaptureProvided
        && !plan.UnreachedContinuationPublicationStoreBase6508CaptureProvided
        && !plan.UnreachedContinuationPublicationFreshBase650EProvided
        && !plan.UnreachedContinuationPublicationSlotDwordProvided
        && !plan.UnreachedContinuationFinalFrame8PointerCaptureProvided
        && !plan.UnreachedContinuationFinalPointeeDwordCaptureProvided
        && !plan.UnreachedContinuationFinalReceiver10OldValueCaptureProvided;

    private static bool AllOptionalContinuationCapturesUnreached(
        FrameHandoffHelperLifecyclePlan plan) =>
        plan.UnreachedContinuationReceiverPlus08CaptureProvided
        && plan.UnreachedContinuationReceiverPlus10CaptureProvided
        && plan.UnreachedContinuationReceiverPlus0COldCaptureProvided
        && plan.UnreachedContinuationReceiverPlus0CFreshCaptureProvided
        && plan.UnreachedContinuationReceiverPlus04CaptureProvided
        && plan.UnreachedContinuationFirstSlotDwordCaptureProvided
        && plan.UnreachedContinuationCallEntryValidationProvided
        && plan.UnreachedContinuationDirectCallReturnCaptureProvided
        && plan.UnreachedContinuationDirectCallReturnEaxCaptureProvided
        && plan.UnreachedContinuationPublicationBranchEntryCaptureProvided
        && plan.UnreachedContinuationPublicationStoreBase6508CaptureProvided
        && plan.UnreachedContinuationPublicationFreshBase650EProvided
        && plan.UnreachedContinuationPublicationSlotDwordProvided
        && plan.UnreachedContinuationFinalFrame8PointerCaptureProvided
        && plan.UnreachedContinuationFinalPointeeDwordCaptureProvided
        && plan.UnreachedContinuationFinalReceiver10OldValueCaptureProvided;
}
