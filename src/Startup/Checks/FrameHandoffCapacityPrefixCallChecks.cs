using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityPrefixCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityPrefixCallPlan zero =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x1000, 0x2000, 0, 0, 0x1234),
                null));
        check("The zero-sum prefix skips every call and reaches the clean success join",
            zero.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.PrefixZeroBranchToSuccessJoin
            && zero.PrefixPlan.Outcome
                == FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA
            && zero.PrefixPlan.FrameStores.Length == 2
            && zero.PrefixPlan.FrameStores[0].Address == 0x1FF8
            && zero.PrefixPlan.FrameStores[1].Address == 0x1FFC
            && zero.CallPlan is null
            && !zero.CallReached
            && zero.SuccessJoinReached
            && zero.SuccessAdmission
            && zero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixCallProjection.SuccessJoinRawFileOffset);

        FrameHandoffCapacityPrefixCallPlan zeroWithCallCaptures =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x1100, 0x2200, 0, 0, 4),
                new FrameHandoffCapacityCallCapture(
                    FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x2200, true, 9, true)));
        check("The zero-sum branch rejects every supplied route, call-status, and EAX capture before admission",
            zeroWithCallCaptures.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.PrefixZeroRejectedUnreachedCallCaptures
            && zeroWithCallCaptures.CallPlan is null
            && !zeroWithCallCaptures.CallReached
            && zeroWithCallCaptures.UnreachedCallEntryRouteCaptureProvided
            && zeroWithCallCaptures.UnreachedDirectCallReturnCaptureProvided
            && zeroWithCallCaptures.UnreachedDirectCallReturnEaxCaptureProvided
            && zeroWithCallCaptures.UnreachedGuardIndirectCallReturnCaptureProvided
            && !zeroWithCallCaptures.SuccessJoinReached
            && !zeroWithCallCaptures.SuccessAdmission
            && zeroWithCallCaptures.PrefixPlan.FrameStores.Length == 2);

        FrameHandoffCapacityPrefixCallPlan guardRoute =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x3000, 0x4000, 0x40000000, 0, 8),
                new FrameHandoffCapacityCallCapture(
                    FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4,
                    0x4000, null, null, true)));
        check("The above-guard prefix selects the BC4 indirect route and admits only its normal return",
            guardRoute.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && guardRoute.CallRouteDerivedFromPrefix
            && guardRoute.CallPlan?.Capture.EntryRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && guardRoute.CallPlan?.GuardIndirectCallReached == true
            && guardRoute.CallPlan?.GuardIndirectCallReturnedNormally == true
            && guardRoute.CallPlan?.NormalBoundaryReached == true
            && guardRoute.SuccessJoinReached
            && guardRoute.SuccessAdmission
            && guardRoute.PrefixPlan.PushReached == false
            && guardRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixCallProjection.SuccessJoinRawFileOffset);

        FrameHandoffCapacityPrefixCallPlan directRoute =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x5000, 0x6000, 3, 0, 0x40),
                new FrameHandoffCapacityCallCapture(
                    FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                    0x6000, true, 7, null)));
        check("The scaled-push prefix selects B5 and carries its subtract and shifted-EDI values",
            directRoute.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.DirectCallB5
            && directRoute.CallPlan?.DirectCallReached == true
            && directRoute.CallPlan?.FrameWriteValue == 7
            && directRoute.CallPlan?.FrameWriteAddress == 0x5FFC
            && directRoute.PrefixPlan.EsiAfterSubtract == 3
            && directRoute.PrefixPlan.EdiAfterShift == 0x10
            && directRoute.EsiAfterSubtract == directRoute.PrefixPlan.EsiAfterSubtract
            && directRoute.EdiAfterShift == directRoute.PrefixPlan.EdiAfterShift
            && directRoute.PrefixPlan.PushedValue == 12
            && directRoute.SuccessJoinReached
            && directRoute.SuccessAdmission);

        FrameHandoffCapacityPrefixCallPlan missingPrefix =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x7000, 0x8000, 3, 5, null),
                new FrameHandoffCapacityCallCapture(
                    FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                    0x8000, true, 1, true)));
        check("A partial prefix capture blocks all calls and records the supplied captures as unreached",
            missingPrefix.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.MissingPrefixReceiverCRead
            && missingPrefix.PrefixPlan.FrameStores.IsEmpty
            && missingPrefix.CallPlan is null
            && !missingPrefix.CallReached
            && missingPrefix.UnreachedCallEntryRouteCaptureProvided
            && missingPrefix.UnreachedDirectCallReturnCaptureProvided
            && missingPrefix.UnreachedDirectCallReturnEaxCaptureProvided
            && missingPrefix.UnreachedGuardIndirectCallReturnCaptureProvided
            && !missingPrefix.SuccessJoinReached
            && !missingPrefix.SuccessAdmission
            && missingPrefix.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset);

        FrameHandoffCapacityPrefixCallPlan missingCallCapture =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x9000, 0xA000, 3, 0, 4),
                null));
        FrameHandoffCapacityPrefixCallPlan missingCallStatus =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x9000, 0xA000, 3, 0, 4),
                new FrameHandoffCapacityCallCapture(null, 0xA000, null, 9, null)));
        FrameHandoffCapacityPrefixCallPlan directDidNotReturn =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x9000, 0xA000, 3, 0, 4),
                new FrameHandoffCapacityCallCapture(null, 0xA000, false, 9, true)));
        check("Missing or nonreturning calls preserve the completed prefix stores and push without reaching the join",
            missingCallCapture.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.CallCaptureMissing
            && missingCallCapture.CallPlan is null
            && missingCallCapture.PrefixPlan.PushReached
            && missingCallCapture.PrefixPlan.FrameStores.Length == 2
            && missingCallCapture.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.DirectCallRawFileOffset
            && missingCallStatus.CallPlan?.Outcome
                == FrameHandoffCapacityCallOutcome.MissingDirectCallReturnCapture
            && missingCallStatus.CallPlan?.DirectCallReached == true
            && missingCallStatus.PrefixPlan.PushReached
            && !missingCallStatus.SuccessJoinReached
            && !missingCallStatus.SuccessAdmission
            && directDidNotReturn.CallPlan?.Outcome
                == FrameHandoffCapacityCallOutcome.DirectCallDidNotReturnNormally
            && directDidNotReturn.CallPlan?.DirectCallReturnedNormally == false
            && directDidNotReturn.PrefixPlan.PushReached
            && directDidNotReturn.PrefixPlan.FrameStores.Length == 2
            && !directDidNotReturn.SuccessJoinReached);

        FrameHandoffCapacityPrefixCallPlan guardDidNotReturn =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0xB000, 0xC000, 0x40000000, 0, 0),
                new FrameHandoffCapacityCallCapture(null, 0xC000, null, null, false)));
        check("A nonreturning BC4 call retains the above-guard prefix stores and stops at the indirect call",
            guardDidNotReturn.CallPlan?.Outcome
                == FrameHandoffCapacityCallOutcome.GuardIndirectCallDidNotReturnNormally
            && guardDidNotReturn.PrefixPlan.FrameStores.Length == 2
            && !guardDidNotReturn.PrefixPlan.PushReached
            && !guardDidNotReturn.SuccessJoinReached
            && guardDidNotReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset);

        FrameHandoffCapacityPrefixCallPlan invalidRoute =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0xD000, 0xE000, 0x40000000, 0, 1),
                new FrameHandoffCapacityCallCapture(
                    (FrameHandoffCapacityCallEntryRoute)99, 0xE000, null, null, true)));
        FrameHandoffCapacityPrefixCallPlan bypassAttempt =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0xD000, 0xE000, 0x40000000, 0, 1),
                new FrameHandoffCapacityCallCapture(
                    FrameHandoffCapacityCallEntryRoute.DirectCallB5,
                    0xE000, true, 1, true)));
        check("Invalid and mismatched route captures cannot override the prefix-selected BC4 guard",
            invalidRoute.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.InvalidSuppliedCallRoute
            && invalidRoute.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && invalidRoute.CallPlan is null
            && !invalidRoute.CallReached
            && bypassAttempt.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.SuppliedCallRouteMismatch
            && bypassAttempt.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && bypassAttempt.CallPlan is null
            && !bypassAttempt.SuccessAdmission
            && bypassAttempt.PrefixPlan.PushReached == false);

        FrameHandoffCapacityPrefixCallPlan frameMismatch =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0xF000, 0x1000, 3, 0, 1),
                new FrameHandoffCapacityCallCapture(null, 0x1001, true, 1, null)));
        check("A mismatched frame identity is rejected before the call can project its frame write",
            frameMismatch.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.FrameEbpMismatch
            && frameMismatch.FrameEbpIdentityValidated
            && frameMismatch.FrameEbpIdentityMatches == false
            && frameMismatch.CallPlan is null
            && !frameMismatch.CallReached
            && frameMismatch.PrefixPlan.PushReached
            && frameMismatch.PrefixPlan.FrameStores.Length == 2
            && !frameMismatch.SuccessJoinReached);

        FrameHandoffCapacityPrefixCallPlan unexpectedIndirectCapture =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x11000, 0x12000, 3, 0, 1),
                new FrameHandoffCapacityCallCapture(null, 0x12000, true, 7, true)));
        check("A nonzero direct return with an unreachable indirect status reaches but cannot admit the join",
            unexpectedIndirectCapture.CallPlan?.Outcome
                == FrameHandoffCapacityCallOutcome.DirectNonzeroReturnedNormally
            && unexpectedIndirectCapture.CallPlan?.NormalBoundaryReached == true
            && unexpectedIndirectCapture.CallPlan?.UnreachedGuardIndirectCallReturnCaptureProvided == true
            && unexpectedIndirectCapture.SuccessJoinReached
            && !unexpectedIndirectCapture.SuccessAdmission);

        FrameHandoffCapacityPrefixCallPlan wrappedFrame =
            FrameHandoffCapacityPrefixCallProjection.Project(new(
                new(0x13000, 0, 3, 0, 0x40),
                new FrameHandoffCapacityCallCapture(null, 0, true, 0, true)));
        check("Prefix and call frame destinations both preserve wrapping EBP arithmetic after identity validation",
            wrappedFrame.FrameEbpIdentityValidated
            && wrappedFrame.FrameEbpIdentityMatches == true
            && wrappedFrame.PrefixPlan.FrameStores[0].Address == 0xFFFFFFF8
            && wrappedFrame.PrefixPlan.FrameStores[1].Address == 0xFFFFFFFC
            && wrappedFrame.CallPlan?.FrameWriteAddress == 0xFFFFFFFC
            && wrappedFrame.CallPlan?.FrameWriteValue == 0
            && wrappedFrame.SuccessAdmission);
    }
}
