using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveResidueJoinChecks
{
    public static void Run(Action<string, bool> check)
    {
        UnreliableReceiveResidueHandoffEntry.TryCreate(0, 1, out var earlyEntry);
        UnreliableReceiveResidueJoinPlan earlyJoin =
            UnreliableReceiveResidueJoinProjection.Project(
                earlyEntry,
                new(0x1000, null, null, null),
                counterCapture: null,
                joinedCapture: new(0x2000, 0x1000, 0x3000, 0xBEEF, 0));
        check("Entry residue 0xFFFF reaches the joined header without projecting the counter",
            earlyJoin.Outcome == UnreliableReceiveResidueJoinOutcome.JoinedZeroBranchTaken
            && earlyJoin.Handoff.HandoffResidue == 1
            && earlyJoin.EarlyJbeTaken == true
            && earlyJoin.HandoffCompleted
            && !earlyJoin.CounterCompleted
            && earlyJoin.Counter is null
            && earlyJoin.JoinedCompleted
            && earlyJoin.JoinedZeroBranchTaken == true
            && earlyJoin.JoinedNonzeroBranchFallthrough == false
            && earlyJoin.IsTerminalSuccess
            && earlyJoin.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        UnreliableReceiveResidueHandoffEntry.TryCreate(0xA55A, 0x255A, out var normalEntry);
        var normalHandoff = new UnreliableReceiveResidueHandoffCapture(
            0x4000,
            0x11111111,
            0x22222222,
            true);
        var normalCounter = new UnreliableReceiveResidueCounterCapture(
            true,
            0x4000,
            0x8000,
            0x5000,
            7);
        var independentJoinedRead = new UnreliableReceiveJoinedHeaderCapture(
            0x6000,
            0x4000,
            0x7000,
            0x1234,
            0x1000);
        UnreliableReceiveResidueJoinPlan normalJoin =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter,
                independentJoinedRead);
        check("Normal residue 0x8000 requires the matching EDI and retained ESI before joining",
            normalJoin.Outcome
                == UnreliableReceiveResidueJoinOutcome.JoinedNonzeroBranchFallthrough
            && normalJoin.Handoff.HandoffResidue == 0x8000
            && normalJoin.HandoffCompleted
            && normalJoin.Counter?.Outcome
                == UnreliableReceiveResidueCounterOutcome.CounterDwordAdded
            && normalJoin.CounterCompleted
            && normalJoin.Joined?.PayloadWordAtOffset2 == 0x1234
            && normalJoin.Joined?.PayloadWordAtOffset2 != normalEntry.PayloadWord
            && normalJoin.JoinedCompleted
            && normalJoin.JoinedZeroBranchTaken == false
            && normalJoin.JoinedNonzeroBranchFallthrough == true
            && normalJoin.IsTerminalSuccess
            && normalJoin.StopAtRawFileOffsetExclusive == 0x00BE5B16);

        UnreliableReceiveResidueJoinPlan wrongRetainedEsi =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter with { RetainedEsi = 0x7FFF },
                independentJoinedRead);
        check("A retained ESI value other than the handoff residue rejects both later stages",
            wrongRetainedEsi.Outcome
                == UnreliableReceiveResidueJoinOutcome.CounterRetainedEsiDoesNotMatchHandoffResidue
            && wrongRetainedEsi.HandoffCompleted
            && !wrongRetainedEsi.CounterCompleted
            && wrongRetainedEsi.Counter is null
            && wrongRetainedEsi.UnreachedJoinedCaptureProvided
            && wrongRetainedEsi.Joined is null
            && wrongRetainedEsi.StopAtRawFileOffsetExclusive
                == UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset);

        UnreliableReceiveResidueJoinPlan missingHandoffCapture =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                new(0x4000, null, 0x22222222, true),
                normalCounter,
                independentJoinedRead);
        UnreliableReceiveResidueJoinPlan handoffWithoutLaterValues =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                new(0x4000, null, null, null),
                counterCapture: null,
                joinedCapture: null);
        check("Incomplete handoff and missing later captures stop before projecting downstream stages",
            missingHandoffCapture.Outcome
                == UnreliableReceiveResidueJoinOutcome.HandoffDidNotReturnToExclusiveBoundary
            && missingHandoffCapture.Counter is null
            && missingHandoffCapture.Joined is null
            && missingHandoffCapture.UnreachedCounterCaptureProvided
            && missingHandoffCapture.UnreachedJoinedCaptureProvided
            && !missingHandoffCapture.HandoffCompleted
            && missingHandoffCapture.StopAtRawFileOffsetExclusive
                == UnreliableReceiveResidueHandoffProjection.ReceiverDwordAtOffset04PushRawFileOffset
            && handoffWithoutLaterValues.Outcome
                == UnreliableReceiveResidueJoinOutcome.HandoffDidNotReturnToExclusiveBoundary
            && handoffWithoutLaterValues.Counter is null
            && handoffWithoutLaterValues.Joined is null
            && handoffWithoutLaterValues.StopAtRawFileOffsetExclusive
                == UnreliableReceiveResidueHandoffProjection.ReceiverDwordAtOffset04PushRawFileOffset);

        UnreliableReceiveResidueJoinPlan missingCounterCapture =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                counterCapture: null,
                joinedCapture: independentJoinedRead);
        UnreliableReceiveResidueJoinPlan missingCounterWord =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter with { CounterDwordAtLoadedPointerOffset1C = null },
                independentJoinedRead);
        check("Counter and joined captures remain unreached until each prior stage completes",
            missingCounterCapture.Outcome == UnreliableReceiveResidueJoinOutcome.MissingCounterCapture
            && missingCounterCapture.HandoffCompleted
            && !missingCounterCapture.CounterCompleted
            && missingCounterCapture.UnreachedJoinedCaptureProvided
            && missingCounterCapture.Joined is null
            && missingCounterCapture.StopAtRawFileOffsetExclusive
                == UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset
            && missingCounterWord.Outcome == UnreliableReceiveResidueJoinOutcome.CounterDidNotComplete
            && missingCounterWord.Counter?.Outcome
                == UnreliableReceiveResidueCounterOutcome.MissingCounterDwordAtLoadedPointerOffset1C
            && missingCounterWord.UnreachedJoinedCaptureProvided
            && missingCounterWord.Joined is null
            && missingCounterWord.StopAtRawFileOffsetExclusive
                == UnreliableReceiveResidueCounterProjection.CounterDwordAddRawFileOffset);

        UnreliableReceiveResidueJoinPlan missingJoinedCapture =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter,
                joinedCapture: null);
        check("The joined stage is not completed without its capture after the counter succeeds",
            missingJoinedCapture.Outcome == UnreliableReceiveResidueJoinOutcome.MissingJoinedCapture
            && missingJoinedCapture.HandoffCompleted
            && missingJoinedCapture.CounterCompleted
            && !missingJoinedCapture.JoinedCompleted
            && missingJoinedCapture.Joined is null
            && missingJoinedCapture.StopAtRawFileOffsetExclusive
                == UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset);

        UnreliableReceiveResidueJoinPlan earlyWithUnreachedHandoffCapture =
            UnreliableReceiveResidueJoinProjection.Project(
                earlyEntry,
                new(0x1000, 0, null, null),
                counterCapture: null,
                joinedCapture: new(0x2000, 0x1000, 0x3000, 0xBEEF, 0));
        UnreliableReceiveResidueJoinPlan earlyWithCounterCapture =
            UnreliableReceiveResidueJoinProjection.Project(
                earlyEntry,
                new(0x1000, null, null, null),
                normalCounter,
                new(0x2000, 0x1000, 0x3000, 0xBEEF, 0));
        check("Early JBE rejects supplied unreached handoff and counter captures",
            earlyWithUnreachedHandoffCapture.Outcome
                == UnreliableReceiveResidueJoinOutcome.EarlyJbeHasUnreachedHandoffCaptures
            && earlyWithUnreachedHandoffCapture.EarlyJbeTaken == true
            && !earlyWithUnreachedHandoffCapture.HandoffCompleted
            && earlyWithUnreachedHandoffCapture.Joined is null
            && earlyWithCounterCapture.Outcome
                == UnreliableReceiveResidueJoinOutcome.EarlyJbeHasCounterCapture
            && earlyWithCounterCapture.Counter is null
            && earlyWithCounterCapture.Joined is null);

        UnreliableReceiveResidueJoinPlan joinedReceiverMismatch =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter,
                independentJoinedRead with { ReceiverBaseAddress = 0x4001 });
        UnreliableReceiveResidueJoinPlan missingJoinedWord =
            UnreliableReceiveResidueJoinProjection.Project(
                normalEntry,
                normalHandoff,
                normalCounter,
                independentJoinedRead with
                {
                    PayloadWordAtOffset2 = null,
                    PayloadHeader = 0
                });
        check("Joined receiver identity is enforced and a missing first reload stops the header read",
            joinedReceiverMismatch.Outcome
                == UnreliableReceiveResidueJoinOutcome.JoinedReceiverDoesNotMatchHandoff
            && joinedReceiverMismatch.CounterCompleted
            && joinedReceiverMismatch.Joined is null
            && joinedReceiverMismatch.StopAtRawFileOffsetExclusive
                == UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset
            && missingJoinedWord.Outcome
                == UnreliableReceiveResidueJoinOutcome.JoinedHeaderDidNotComplete
            && missingJoinedWord.Joined?.Outcome
                == UnreliableReceiveJoinedHeaderOutcome.MissingPayloadWordAtOffset2WithUnreachedHeaderCapture
            && missingJoinedWord.JoinedCompleted == false
            && missingJoinedWord.UnreachedJoinedCaptureProvided == false
            && missingJoinedWord.StopAtRawFileOffsetExclusive
                == UnreliableReceiveJoinedHeaderProjection.PayloadWordAtOffset2ReadRawFileOffset);
    }
}
