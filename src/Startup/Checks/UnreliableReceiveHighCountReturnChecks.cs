using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed range preserves the pinned raw mapping and exact instruction bytes",
            UnreliableReceiveHighCountReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveHighCountReturnProjection.EntryRawFileOffset == 0x00BE5BF1
            && UnreliableReceiveHighCountReturnProjection.EntryVirtualAddress == 0x00FE67F1
            && UnreliableReceiveHighCountReturnProjection.FrameHandoffEntryRawFileOffset
                == 0x00BE5C0F
            && UnreliableReceiveHighCountReturnProjection.EndRawFileOffsetExclusive == 0x00BE5C26
            && UnreliableReceiveHighCountReturnProjection.EndVirtualAddressExclusive == 0x00FE6826
            && UnreliableReceiveHighCountReturnProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BF1) == 0x00FE67F1
            && UnreliableReceiveHighCountReturnProjection.RawFileOffsetToVirtualAddress(
                0x00BE5C26) == 0x00FE6826
            && UnreliableReceiveHighCountReturnProjection.CapturedWindowBytes
                == UnreliableReceiveHighCountSuffixProjection.CapturedWindowBytes
                    + " "
                    + UnreliableReceiveFrameHandoffProjection.CapturedWindowBytes
            && UnreliableReceiveHighCountReturnProjection.CapturedWindowBytes
                .Replace(" ", string.Empty, StringComparison.Ordinal).Length == 0x6A
            && UnreliableReceiveHighCountReturnProjection.Instructions.Length == 22
            && UnreliableReceiveHighCountReturnProjection.Instructions[0].RawFileOffset
                == 0x00BE5BF1
            && UnreliableReceiveHighCountReturnProjection.Instructions[^1].RawFileOffset
                == 0x00BE5C23
            && UnreliableReceiveHighCountReturnProjection.Instructions.All(instruction =>
                instruction.VirtualAddress
                    == UnreliableReceiveHighCountReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)));

        UnreliableReceiveHighCountReturnPlan zeroedCounter =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0, 0, null, null, null),
                        new(true, 0x9000, 1, null)),
                    FrameEbp: 0xA000,
                    HelperReturnedNormally: true,
                    HelperReturnEax: null));
        check("A zeroed counter admits the handoff and sets AL without requiring a captured full EAX",
            zeroedCounter.Outcome == UnreliableReceiveHighCountReturnOutcome.ReturnReached
            && zeroedCounter.SuffixPlan.Outcome
                == UnreliableReceiveHighCountSuffixOutcome
                    .Counter4CDecrementedAndCounter48Zeroed
            && zeroedCounter.SuffixPlan.Complete
            && zeroedCounter.SuffixPlan.CounterJoinAdmitted
            && zeroedCounter.HandoffEntryAdmitted
            && zeroedCounter.HandoffCapture?.ValidatedEntry == true
            && zeroedCounter.HandoffCapture?.FrameEbp == 0xA000
            && zeroedCounter.HandoffCapture?.ReceiverEdi == 0x9000
            && zeroedCounter.HandoffPlan?.Outcome
                == UnreliableReceiveFrameHandoffOutcome.ReturnReached
            && zeroedCounter.HandoffPlan?.FramePlusEightAddress == 0xA008
            && zeroedCounter.HandoffPlan?.ReceiverPlus3CAddressPassedInEcx == 0x903C
            && zeroedCounter.HandoffPlan?.AlSetToOne == true
            && zeroedCounter.HandoffPlan?.EaxAfterAlSet is null
            && zeroedCounter.HandoffPlan?.RetInstructionReached == true
            && zeroedCounter.Complete
            && zeroedCounter.StopAtRawFileOffsetExclusive == 0x00BE5C26
            && !zeroedCounter.UnreachedHelperReturnEaxCaptureProvided);

        UnreliableReceiveHighCountReturnPlan incrementedCounter =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0x1000, 0x2000, 0x3000, 0x4000, true),
                        new(true, 0x9000, 2, 5)),
                    FrameEbp: 0xA000,
                    HelperReturnedNormally: true,
                    HelperReturnEax: 0xAABBCCDD));
        check("An incremented counter uses its own receiver identity through the normal return",
            incrementedCounter.Outcome == UnreliableReceiveHighCountReturnOutcome.ReturnReached
            && incrementedCounter.SuffixPlan.Outcome
                == UnreliableReceiveHighCountSuffixOutcome
                    .Counter4CDecrementedAndCounter48Incremented
            && incrementedCounter.SuffixPlan.CounterPlan?.Counter4CNewValue == 1
            && incrementedCounter.SuffixPlan.CounterPlan?.Counter48NewValue == 6
            && incrementedCounter.HandoffCapture?.ReceiverEdi == 0x9000
            && incrementedCounter.HandoffCapture?.FrameEbp == 0xA000
            && incrementedCounter.HandoffPlan?.CapturedHelperReturnEax == 0xAABBCCDD
            && incrementedCounter.HandoffPlan?.EaxAfterAlSet == 0xAABBCC01
            && incrementedCounter.Complete);

        UnreliableReceiveHighCountReturnPlan failedCounterAtJoin =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0, 0, null, null, null),
                        new(true, 0x9000, 1, 0x1234)),
                    FrameEbp: 0xA000,
                    HelperReturnedNormally: true,
                    HelperReturnEax: 0x12345678));
        check("A failed counter is not skipped even when its stop offset equals the shared join",
            failedCounterAtJoin.Outcome
                == UnreliableReceiveHighCountReturnOutcome.SuffixDidNotComplete
            && failedCounterAtJoin.SuffixPlan.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && failedCounterAtJoin.SuffixPlan.CounterPlan?.Counter4CWriteProjected == true
            && failedCounterAtJoin.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected == true
            && !failedCounterAtJoin.SuffixPlan.CounterJoinAdmitted
            && !failedCounterAtJoin.HandoffEntryAdmitted
            && failedCounterAtJoin.HandoffCapture is null
            && failedCounterAtJoin.HandoffPlan is null
            && failedCounterAtJoin.UnreachedFrameEbpCaptureProvided
            && failedCounterAtJoin.UnreachedHelperReturnedNormallyCaptureProvided
            && failedCounterAtJoin.UnreachedHelperReturnEaxCaptureProvided);

        UnreliableReceiveHighCountReturnPlan missingCounter48 =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0x1000, 0x2000, 0x3000, 0x4000, true),
                        new(true, 0x9000, 2, null)),
                    FrameEbp: 0xA000,
                    HelperReturnedNormally: true,
                    HelperReturnEax: null));
        check("A partial counter decrement remains visible when the nonzero branch lacks its 48 capture",
            missingCounter48.Outcome
                == UnreliableReceiveHighCountReturnOutcome.SuffixDidNotComplete
            && missingCounter48.SuffixPlan.CounterPlan?.Counter4CNewValue == 1
            && missingCounter48.SuffixPlan.CounterPlan?.Counter4CWriteProjected == true
            && missingCounter48.SuffixPlan.StopAtRawFileOffsetExclusive == 0x00BE5C0C
            && !missingCounter48.HandoffEntryAdmitted
            && missingCounter48.HandoffPlan is null
            && missingCounter48.UnreachedFrameEbpCaptureProvided
            && missingCounter48.UnreachedHelperReturnedNormallyCaptureProvided
            && !missingCounter48.UnreachedHelperReturnEaxCaptureProvided);

        UnreliableReceiveHighCountReturnPlan missingFrame =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0, 0, null, null, null),
                        new(true, 0x9000, 1, null)),
                    FrameEbp: null,
                    HelperReturnedNormally: true,
                    HelperReturnEax: 0x12345678));
        UnreliableReceiveHighCountReturnPlan missingHelperReturn =
            UnreliableReceiveHighCountReturnProjection.Project(
                new(
                    new(
                        new(true, 0, 0, null, null, null),
                        new(true, 0x9000, 1, null)),
                    FrameEbp: 0xA000,
                    HelperReturnedNormally: null,
                    HelperReturnEax: 0x12345678));
        check("Frame validation precedes LEA; missing helper evidence preserves prior counter writes",
            missingFrame.Outcome
                == UnreliableReceiveHighCountReturnOutcome.MissingFrameEbpCapture
            && missingFrame.HandoffEntryAdmitted
            && missingFrame.HandoffPlan is null
            && missingFrame.UnreachedHelperReturnedNormallyCaptureProvided
            && missingFrame.UnreachedHelperReturnEaxCaptureProvided
            && missingFrame.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && missingHelperReturn.Outcome
                == UnreliableReceiveHighCountReturnOutcome.HandoffDidNotReturnNormally
            && missingHelperReturn.SuffixPlan.CounterPlan?.Counter4CWriteProjected == true
            && missingHelperReturn.SuffixPlan.CounterPlan?.Counter48ZeroStoreProjected == true
            && missingHelperReturn.HandoffCapture?.ReceiverEdi == 0x9000
            && missingHelperReturn.HandoffPlan?.Outcome
                == UnreliableReceiveFrameHandoffOutcome.MissingHelperReturnCapture
            && missingHelperReturn.HandoffPlan?.FramePlusEightAddress == 0xA008
            && missingHelperReturn.HandoffPlan?.EdiPopped == false
            && !missingHelperReturn.Complete
            && missingHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE5C16);
    }
}
