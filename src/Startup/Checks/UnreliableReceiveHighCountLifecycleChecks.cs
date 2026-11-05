using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The lifecycle joins the exact result and return windows at the shared boundary",
            UnreliableReceiveHighCountLifecycleProjection.SamplePath
                == UnreliableReceiveHighCountResultProjection.SamplePath
            && UnreliableReceiveHighCountLifecycleProjection.SampleSha256
                == UnreliableReceiveHighCountReturnProjection.SampleSha256
            && UnreliableReceiveHighCountLifecycleProjection.EntryRawFileOffset
                == 0x00BE5BAC
            && UnreliableReceiveHighCountLifecycleProjection.EntryVirtualAddress
                == 0x00FE67AC
            && UnreliableReceiveHighCountLifecycleProjection.EndRawFileOffsetExclusive
                == 0x00BE5C26
            && UnreliableReceiveHighCountLifecycleProjection.EndVirtualAddressExclusive
                == 0x00FE6826
            && UnreliableReceiveHighCountLifecycleProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BAC) == 0x00FE67AC
            && UnreliableReceiveHighCountLifecycleProjection.RawFileOffsetToVirtualAddress(
                0x00BE5C26) == 0x00FE6826
            && UnreliableReceiveHighCountLifecycleProjection.CapturedWindowBytes
                == UnreliableReceiveHighCountResultProjection.CapturedWindowBytes
                    + " "
                    + UnreliableReceiveHighCountReturnProjection.CapturedWindowBytes
            && UnreliableReceiveHighCountLifecycleProjection.Instructions.Length == 41
            && UnreliableReceiveHighCountLifecycleProjection.Instructions[18].RawFileOffset
                == 0x00BE5BEC
            && UnreliableReceiveHighCountLifecycleProjection.Instructions[19].RawFileOffset
                == 0x00BE5BF1
            && UnreliableReceiveHighCountLifecycleProjection.Instructions[^1].RawFileOffset
                == 0x00BE5C23
            && UnreliableReceiveHighCountLifecycleProjection.Instructions.All(instruction =>
                instruction.VirtualAddress
                    == UnreliableReceiveHighCountLifecycleProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)));

        UnreliableReceiveHighCountLifecyclePlan zeroObjectCallback =
            UnreliableReceiveHighCountLifecycleProjection.Project(CreateCapture(
                callbackHelperEax: 0,
                callbackFirstLoadedDword: 0,
                callbackSecondLoadedDword: null,
                callbackIndirectTargetDword: null,
                callbackNormal: null,
                counter4C: 1));
        check("A zero-object callback joins the nonzero result path and returns with AL set only",
            zeroObjectCallback.Outcome
                == UnreliableReceiveHighCountLifecycleOutcome.ReturnCompletedNormally
            && zeroObjectCallback.ResultForwardCompletedNormally
            && zeroObjectCallback.ResultPlan.Outcome
                == UnreliableReceiveHighCountResultOutcome.ForwardCompletedNormally
            && zeroObjectCallback.ResultPlan.HighCountForwardPlan.SecondCallReached
            && zeroObjectCallback.ResultPlan.HighCountForwardPlan.SecondHelperReturnedNormally
                == true
            && zeroObjectCallback.CallbackEntryAdmitted
            && zeroObjectCallback.ReturnPlan.SuffixPlan.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.ZeroObjectBranch
            && zeroObjectCallback.CounterEntryAdmitted
            && zeroObjectCallback.ReturnPlan.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected
                == true
            && zeroObjectCallback.ReturnPlan.HandoffPlan?.AlSetToOne == true
            && zeroObjectCallback.ReturnPlan.HandoffPlan?.EaxAfterAlSet is null
            && zeroObjectCallback.Complete
            && zeroObjectCallback.StopAtRawFileOffsetExclusive == 0x00BE5C26);

        UnreliableReceiveHighCountLifecyclePlan indirectCallback =
            UnreliableReceiveHighCountLifecycleProjection.Project(CreateCapture(
                callbackHelperEax: 0x11112222,
                callbackFirstLoadedDword: 0x2000,
                callbackSecondLoadedDword: 0x3000,
                callbackIndirectTargetDword: 0x4000,
                callbackNormal: true,
                counter4C: 2,
                counter48: 5,
                helperReturnEax: 0xAABBCCDD));
        check("The indirect callback joins independently captured input EAX and counter updates",
            indirectCallback.Outcome
                == UnreliableReceiveHighCountLifecycleOutcome.ReturnCompletedNormally
            && indirectCallback.CallbackEntryAdmitted
            && indirectCallback.ReturnPlan.SuffixPlan.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.CallbackReturnedNormally
            && indirectCallback.ReturnPlan.SuffixPlan.CallbackPlan.FirstReadAddress
                == 0x11112222
            && indirectCallback.ReturnPlan.SuffixPlan.CallbackPlan.FirstReadAddress
                != indirectCallback.ResultPlan.HighCountForwardPlan.LoadedObjectPointerEax
            && indirectCallback.ReturnPlan.SuffixPlan.CounterPlan?.Counter4CNewValue == 1
            && indirectCallback.ReturnPlan.SuffixPlan.CounterPlan?.Counter48NewValue == 6
            && indirectCallback.ReturnPlan.HandoffPlan?.CapturedHelperReturnEax
                == 0xAABBCCDD
            && indirectCallback.Complete);

        UnreliableReceiveHighCountLifecycleCapture thresholdBelowCapture =
            CreateCapture(
                resultCounter4C: 0xFFFF,
                callbackHelperEax: 0x1111,
                callbackFirstLoadedDword: 0,
                counter4C: 1);
        UnreliableReceiveHighCountLifecyclePlan thresholdBelow =
            UnreliableReceiveHighCountLifecycleProjection.Project(thresholdBelowCapture);
        check("Below threshold retains its result stores and classifies supplied suffix captures as unreached",
            thresholdBelow.Outcome
                == UnreliableReceiveHighCountLifecycleOutcome.ResultDidNotCompleteNormally
            && thresholdBelow.ResultPlan.Outcome
                == UnreliableReceiveHighCountResultOutcome
                    .ResultStoresDidNotReachHighCountBoundary
            && thresholdBelow.ResultPlan.ResultStoresPlan.Receiver4Stored
            && thresholdBelow.ResultPlan.ResultStoresPlan.FrameMinus4LowWordStored
            && !thresholdBelow.ResultForwardCompletedNormally
            && !thresholdBelow.CallbackEntryAdmitted
            && thresholdBelow.ReturnPlan.SuffixPlan.CounterPlan is null
            && thresholdBelow.ReturnPlan.SuffixPlan.UnreachedCounterEntryValidationProvided
            && thresholdBelow.ReturnPlan.SuffixPlan.UnreachedIndependentCounter4CReloadCaptureProvided
            && thresholdBelow.ReturnPlan.SuffixPlan.CallbackPlan
                .UnreachedHelperReturnEaxCaptureProvided
            && thresholdBelow.StopAtRawFileOffsetExclusive == 0x00BE5C0F);

        UnreliableReceiveHighCountLifecyclePlan partialResult =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture() with
                {
                    Result = new(
                        new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, null, null),
                        new(true, 0x1000, 0x11223344, 0x55667788, true, 0x4000, 9, true))
                });
        check("A partial result-store plan is retained and cannot admit callback reads",
            partialResult.ResultPlan.ResultStoresPlan.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingFrameMinus4
            && partialResult.ResultPlan.ResultStoresPlan.Receiver4Read
            && !partialResult.ResultPlan.ResultStoresPlan.Receiver4Stored
            && !partialResult.ResultForwardCompletedNormally
            && !partialResult.ReturnPlan.SuffixPlan.CallbackPlan.FirstLoadedDwordRead
            && partialResult.ReturnPlan.SuffixPlan.CallbackPlan
                .UnreachedHelperReturnEaxCaptureProvided
            && !partialResult.Complete);

        UnreliableReceiveHighCountLifecyclePlan firstCallDidNotReturn =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture() with
                {
                    Result = new(
                        new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                        new(true, 0x1000, 0x11223344, 0x55667788, false, 0x4000, 9, true))
                });
        UnreliableReceiveHighCountLifecyclePlan secondCallDidNotReturn =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture() with
                {
                    Result = new(
                        new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                        new(true, 0x1000, 0x11223344, 0x55667788, true, 0x4000, 9, false))
                });
        check("Neither nonreturning forwarding call admits the callback suffix",
            firstCallDidNotReturn.ResultPlan.HighCountForwardPlan.ForwardCallReached
            && firstCallDidNotReturn.ResultPlan.HighCountForwardPlan.ForwardHelperReturnedNormally
                == false
            && firstCallDidNotReturn.ResultPlan.HighCountForwardPlan
                .UnreachedSecondHelperReturnCaptureProvided
            && !firstCallDidNotReturn.CallbackEntryAdmitted
            && !firstCallDidNotReturn.ReturnPlan.SuffixPlan.CallbackPlan.FirstReadAddressCaptured
            && secondCallDidNotReturn.ResultPlan.HighCountForwardPlan.SecondCallReached
            && secondCallDidNotReturn.ResultPlan.HighCountForwardPlan.SecondHelperReturnedNormally
                == false
            && !secondCallDidNotReturn.ResultPlan.HighCountForwardPlan.NormalCallBoundaryReached
            && !secondCallDidNotReturn.CallbackEntryAdmitted
            && !secondCallDidNotReturn.ReturnPlan.SuffixPlan.CounterPlan.HasValue
            && !firstCallDidNotReturn.Complete
            && !secondCallDidNotReturn.Complete);

        UnreliableReceiveHighCountLifecyclePlan missingCallbackInput =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(callbackHelperEax: null, callbackFirstLoadedDword: null));
        UnreliableReceiveHighCountLifecyclePlan invalidCounterEntry =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(counterValidatedEntry: false, counter4C: 1));
        check("Callback input EAX and the counter entry are independently gated before reads and writes",
            missingCallbackInput.ResultForwardCompletedNormally
            && missingCallbackInput.ReturnPlan.SuffixPlan.CallbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.MissingHelperReturnEax
            && !missingCallbackInput.ReturnPlan.SuffixPlan.CallbackPlan.FirstReadAddressCaptured
            && missingCallbackInput.ReturnPlan.SuffixPlan.CounterPlan is null
            && missingCallbackInput.ReturnPlan.SuffixPlan.UnreachedCounterEntryValidationProvided
            && invalidCounterEntry.CallbackEntryAdmitted
            && invalidCounterEntry.ReturnPlan.SuffixPlan.CounterPlan?.Outcome
                == UnreliableReceiveHighCountCounterOutcome.InvalidEntry
            && invalidCounterEntry.ReturnPlan.SuffixPlan.CounterPlan?.Counter4CWriteProjected
                == false
            && invalidCounterEntry.ReturnPlan.SuffixPlan.CounterPlan?.Counter48WriteProjected
                == false
            && !invalidCounterEntry.CounterEntryAdmitted);

        UnreliableReceiveHighCountLifecyclePlan receiverMismatch =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(counterReceiverEdi: 0x1004, counter4C: 1));
        check("A mismatched counter receiver is rejected before either counter write",
            receiverMismatch.ResultForwardCompletedNormally
            && receiverMismatch.CallbackEntryAdmitted
            && !receiverMismatch.CounterReceiverIdentityMatches
            && receiverMismatch.CounterIdentityMismatchBlockedEntry
            && !receiverMismatch.CounterEntryAdmitted
            && receiverMismatch.Outcome
                == UnreliableReceiveHighCountLifecycleOutcome.CounterReceiverIdentityMismatch
            && receiverMismatch.ReturnPlan.SuffixPlan.CounterPlan?.Outcome
                == UnreliableReceiveHighCountCounterOutcome.InvalidEntry
            && receiverMismatch.ReturnPlan.SuffixPlan.CounterPlan?.Counter4CWriteProjected == false
            && receiverMismatch.ReturnPlan.SuffixPlan.CounterPlan?.Counter48WriteProjected == false
            && receiverMismatch.ReturnPlan.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected
                == false
            && receiverMismatch.ReturnPlan.StopAtRawFileOffsetExclusive == 0x00BE5BFE);

        UnreliableReceiveHighCountLifecyclePlan flaggedZeroCallback =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(callbackSecondLoadedDword: 0x2000));
        check("A later read capture on the zero callback branch cannot admit counter entry",
            !flaggedZeroCallback.CallbackEntryAdmitted
            && flaggedZeroCallback.ReturnPlan.SuffixPlan.CallbackPlan
                .UnreachedSecondLoadedDwordCaptureProvided
            && flaggedZeroCallback.ReturnPlan.SuffixPlan.CounterPlan is null
            && !flaggedZeroCallback.Complete);

        UnreliableReceiveHighCountLifecyclePlan partialCounter =
            UnreliableReceiveHighCountLifecycleProjection.Project(CreateCapture(
                callbackHelperEax: 0x1234,
                callbackFirstLoadedDword: 0x2000,
                callbackSecondLoadedDword: 0x3000,
                callbackIndirectTargetDword: 0x4000,
                callbackNormal: true,
                counter4C: 2,
                counter48: null));
        UnreliableReceiveHighCountLifecyclePlan missingFrame =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(counter4C: 1) with
                {
                    Return = CreateCapture(counter4C: 1).Return with { FrameEbp = null }
                });
        UnreliableReceiveHighCountLifecyclePlan missingHelperReturn =
            UnreliableReceiveHighCountLifecycleProjection.Project(
                CreateCapture(counter4C: 1) with
                {
                    Return = CreateCapture(counter4C: 1).Return with
                    {
                        HelperReturnedNormally = null,
                        HelperReturnEax = 0x12345678
                    }
                });
        check("Partial counter, frame, and helper evidence preserves earlier writes and stops",
            partialCounter.ReturnPlan.SuffixPlan.CounterPlan?.Counter4CWriteProjected == true
            && partialCounter.ReturnPlan.SuffixPlan.CounterPlan?.Counter48Read == false
            && partialCounter.ReturnPlan.HandoffPlan is null
            && partialCounter.StopAtRawFileOffsetExclusive == 0x00BE5C0C
            && missingFrame.ReturnPlan.Outcome
                == UnreliableReceiveHighCountReturnOutcome.MissingFrameEbpCapture
            && missingFrame.ReturnPlan.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected == true
            && missingFrame.ReturnPlan.HandoffPlan is null
            && missingFrame.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && missingHelperReturn.ReturnPlan.HandoffPlan?.Outcome
                == UnreliableReceiveFrameHandoffOutcome.MissingHelperReturnCapture
            && missingHelperReturn.ReturnPlan.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected
                == true
            && !missingHelperReturn.Complete
            && missingHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE5C16);
    }

    private static UnreliableReceiveHighCountLifecycleCapture CreateCapture(
        uint resultCounter4C = 0x00010000,
        uint? callbackHelperEax = 0x1234,
        uint? callbackFirstLoadedDword = 0,
        uint? callbackSecondLoadedDword = null,
        uint? callbackIndirectTargetDword = null,
        bool? callbackNormal = null,
        uint counterReceiverEdi = 0x1000,
        bool counterValidatedEntry = true,
        uint counter4C = 1,
        uint? counter48 = null,
        uint? frameEbp = 0xA000,
        bool? helperReturnedNormally = true,
        uint? helperReturnEax = null)
    {
        var result = new UnreliableReceiveHighCountResultCapture(
            new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, resultCounter4C),
            new(true, 0x1000, 0x11223344, 0x55667788, true, 0x4000, 9, true));
        var returnCapture = new UnreliableReceiveHighCountReturnCapture(
            new(
                new(
                    PriorBE6440Normal: false,
                    HelperReturnEax: callbackHelperEax,
                    FirstLoadedDword: callbackFirstLoadedDword,
                    SecondLoadedDword: callbackSecondLoadedDword,
                    IndirectTargetDword: callbackIndirectTargetDword,
                    CallbackNormal: callbackNormal),
                new(counterValidatedEntry, counterReceiverEdi, counter4C, counter48)),
            frameEbp,
            helperReturnedNormally,
            helperReturnEax);
        return new(result, returnCapture);
    }
}
