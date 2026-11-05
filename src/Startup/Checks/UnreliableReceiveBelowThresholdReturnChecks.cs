using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveBelowThresholdReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Only the successful unsigned-below-threshold stores outcome enters the validated frame handoff",
            UnreliableReceiveBelowThresholdReturnProjection.CounterAtOrAboveThresholdStopRawFileOffsetExclusive
                == 0x00BE5BC4
            && UnreliableReceiveBelowThresholdReturnProjection.FrameHandoffEntryRawFileOffset
                == 0x00BE5C0F
            && BelowThresholdEntryUsesMatchingBases());

        UnreliableReceiveBelowThresholdReturnPlan atThreshold =
            Project(0x00010000, helperReturnedNormally: true, helperReturnEax: 0xAABBCCDD);
        UnreliableReceiveBelowThresholdReturnPlan aboveThreshold =
            Project(uint.MaxValue, helperReturnedNormally: false, helperReturnEax: 0x11223344);
        check("At or above threshold stops at BE5BC4 and keeps both supplied handoff captures unreached",
            atThreshold.Outcome
                == UnreliableReceiveBelowThresholdReturnOutcome.CounterAtOrAboveThresholdHandoffUnreached
            && atThreshold.Stores.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold
            && atThreshold.Stores.Receiver4StoredValue == 0xAABBCCDD
            && atThreshold.Stores.FrameMinus4LowWordStoredValue == 0x7788
            && atThreshold.FrameHandoffCapture is null
            && atThreshold.FrameHandoff is null
            && atThreshold.UnreachedHelperReturnedNormallyCaptureProvided
            && atThreshold.UnreachedHelperReturnEaxCaptureProvided
            && atThreshold.StopAtRawFileOffsetExclusive == 0x00BE5BC4
            && aboveThreshold.Outcome
                == UnreliableReceiveBelowThresholdReturnOutcome.CounterAtOrAboveThresholdHandoffUnreached
            && aboveThreshold.Stores.CounterBelowThreshold == false
            && aboveThreshold.FrameHandoff is null
            && aboveThreshold.UnreachedHelperReturnedNormallyCaptureProvided
            && aboveThreshold.UnreachedHelperReturnEaxCaptureProvided
            && aboveThreshold.StopAtRawFileOffsetExclusive == 0x00BE5BC4);

        UnreliableReceiveBelowThresholdReturnPlan missingCounter =
            Project(counter: null, helperReturnedNormally: true, helperReturnEax: 0xDEADBEEF);
        UnreliableReceiveBelowThresholdReturnPlan missingReceiver =
            UnreliableReceiveBelowThresholdReturnProjection.Project(
                new(
                    new(0, 0x2000, 0x3000, null, null, null),
                    HelperReturnedNormally: true,
                    HelperReturnEax: 0xDEADBEEF));
        check("A blocked store stage preserves completed stores and marks later handoff captures unreached",
            missingCounter.Outcome
                == UnreliableReceiveBelowThresholdReturnOutcome.StoresDidNotReachCounterBelowThreshold
            && missingCounter.Stores.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingReceiver4C
            && missingCounter.Stores.Receiver4Stored
            && missingCounter.Stores.Receiver4StoredValue == 0xAABBCCDD
            && missingCounter.Stores.FrameMinus4LowWordStored
            && missingCounter.Stores.FrameMinus4LowWordStoredValue == 0x7788
            && missingCounter.FrameHandoff is null
            && missingCounter.UnreachedHelperReturnedNormallyCaptureProvided
            && missingCounter.UnreachedHelperReturnEaxCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x00BE5BBB
            && missingReceiver.Stores.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingReceiver4
            && !missingReceiver.Stores.Receiver4Stored
            && missingReceiver.FrameHandoff is null
            && missingReceiver.UnreachedHelperReturnEaxCaptureProvided);

        UnreliableReceiveBelowThresholdReturnPlan missingHelperStatus =
            Project(0, helperReturnedNormally: null, helperReturnEax: 0xAABBCCDD);
        UnreliableReceiveBelowThresholdReturnPlan helperDidNotReturn =
            Project(0, helperReturnedNormally: false, helperReturnEax: 0xAABBCCDD);
        check("The frame handoff validates the original receiver/frame before LEA, push, or helper call",
            missingHelperStatus.Outcome
                == UnreliableReceiveBelowThresholdReturnOutcome.MissingHelperReturnCapture
            && missingHelperStatus.FrameHandoffCapture is
                { ValidatedEntry: true, ReceiverEdi: 0x1000, FrameEbp: 0x3000 }
            && missingHelperStatus.FrameHandoff is { HelperCallReached: true }
            && missingHelperStatus.FrameHandoff.Value.FramePlusEightAddress == 0x3008
            && missingHelperStatus.FrameHandoff.Value.FrameAddressPushedToHelperStack == 0x3008
            && missingHelperStatus.FrameHandoff.Value.ReceiverPlus3CAddressPassedInEcx == 0x103C
            && missingHelperStatus.FrameHandoff.Value.StopAtRawFileOffsetExclusive == 0x00BE5C16
            && missingHelperStatus.UnreachedHelperReturnEaxCaptureProvided
            && helperDidNotReturn.Outcome
                == UnreliableReceiveBelowThresholdReturnOutcome.HelperDidNotReturnNormally
            && helperDidNotReturn.FrameHandoff is
                { HelperCallReached: true, HelperReturnedNormally: false, RetInstructionReached: false }
            && helperDidNotReturn.FrameHandoff.Value.StopAtRawFileOffsetExclusive == 0x00BE5C16);

        UnreliableReceiveBelowThresholdReturnPlan returnedWithEax =
            Project(0, helperReturnedNormally: true, helperReturnEax: 0xAABBCCDD);
        UnreliableReceiveBelowThresholdReturnPlan returnedWithoutEax =
            Project(0, helperReturnedNormally: true, helperReturnEax: null);
        check("A normal return sets AL to one; upper EAX stays unknown unless independently captured",
            returnedWithEax.Outcome == UnreliableReceiveBelowThresholdReturnOutcome.ReturnReached
            && returnedWithEax.FrameHandoff is
                { Outcome: UnreliableReceiveFrameHandoffOutcome.ReturnReached, AlSetToOne: true }
            && returnedWithEax.FrameHandoff.Value.CapturedHelperReturnEax == 0xAABBCCDD
            && returnedWithEax.FrameHandoff.Value.EaxAfterAlSet == 0xAABBCC01
            && returnedWithEax.FrameHandoff.Value.RetInstructionReached
            && returnedWithoutEax.Outcome == UnreliableReceiveBelowThresholdReturnOutcome.ReturnReached
            && returnedWithoutEax.FrameHandoff is
                { AlSetToOne: true, EaxAfterAlSet: null, CapturedHelperReturnEax: null }
            && returnedWithoutEax.Capture.HelperReturnEax is null
            && returnedWithoutEax.FrameHandoffCapture!.Value.HelperReturnEax is null);

        UnreliableReceiveBelowThresholdReturnPlan zeroBases =
            UnreliableReceiveBelowThresholdReturnProjection.Project(
                new(
                    new(0, 0x2000, 0, 0x12345678, 0x87654321, 0),
                    HelperReturnedNormally: true,
                    HelperReturnEax: 0));
        check("Zero receiver and frame bases remain valid and reload values stay independent",
            zeroBases.Outcome == UnreliableReceiveBelowThresholdReturnOutcome.ReturnReached
            && zeroBases.Stores.Receiver4Address == 4
            && zeroBases.Stores.CapturedReceiver4 == 0x12345678
            && zeroBases.Stores.FrameMinus4Address == uint.MaxValue - 3
            && zeroBases.Stores.CapturedFrameMinus4 == 0x87654321
            && zeroBases.Stores.Receiver4CAddress == 0x4C
            && zeroBases.FrameHandoffCapture is
                { ValidatedEntry: true, ReceiverEdi: 0, FrameEbp: 0 }
            && zeroBases.FrameHandoff!.Value.FramePlusEightAddress == 8
            && zeroBases.FrameHandoff.Value.FrameAddressPushedToHelperStack == 8
            && zeroBases.FrameHandoff.Value.ReceiverPlus3CAddressPassedInEcx == 0x3C
            && zeroBases.FrameHandoff.Value.EaxAfterAlSet == 1
            && zeroBases.FrameHandoff.Value.CapturedHelperReturnEax == 0);
    }

    private static UnreliableReceiveBelowThresholdReturnPlan Project(
        uint counter,
        bool? helperReturnedNormally,
        uint? helperReturnEax) =>
        Project((uint?)counter, helperReturnedNormally, helperReturnEax);

    private static UnreliableReceiveBelowThresholdReturnPlan Project(
        uint? counter,
        bool? helperReturnedNormally,
        uint? helperReturnEax) =>
        UnreliableReceiveBelowThresholdReturnProjection.Project(
            new(
                new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12347788, counter),
                helperReturnedNormally,
                helperReturnEax));

    private static bool BelowThresholdEntryUsesMatchingBases()
    {
        UnreliableReceiveBelowThresholdReturnPlan plan =
            Project(0xFFFF, helperReturnedNormally: null, helperReturnEax: null);
        return plan.Stores.Outcome == UnreliableReceiveResultStoresOutcome.CounterBelowThreshold
            && plan.FrameHandoffCapture is
                { ValidatedEntry: true, ReceiverEdi: 0x1000, FrameEbp: 0x3000 }
            && plan.FrameHandoff is { HelperCallReached: true }
            && plan.FrameHandoff.Value.StopAtRawFileOffsetExclusive == 0x00BE5C16;
    }
}
