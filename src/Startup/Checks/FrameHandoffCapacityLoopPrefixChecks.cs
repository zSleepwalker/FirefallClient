using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityLoopPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition begins at the capacity loop and only admits its exact unsigned exit branch",
            FrameHandoffCapacityLoopProjection.EntryRawFileOffset == 0x00BE6B70
            && FrameHandoffCapacityLoopProjection.ExitRawFileOffset
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset
            && FrameHandoffCapacityLoopPrefixProjection.PrefixEntryRawFileOffset
                == 0x00BE6B91);

        FrameHandoffCapacityLoopPrefixPlan direct =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 7),
                0x1000,
                0x2000,
                4,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x2000, true, 7, null),
                ExpectedPrefixEntryEsi: 8,
                ExpectedPrefixEntryEdx: 7,
                ExpectedPrefixEntryEbx: 0x1000,
                ExpectedPrefixEntryEbp: 0x2000));
        check("A nonzero unsigned difference with ESI at eight derives prefix ESI/EDX and admits the direct call success",
            direct.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.PrefixCallSuccessAdmitted
            && direct.LoopExitAdmitted
            && direct.PartialLoopPlan.CompareEsiWithEightReached
            && direct.PartialLoopPlan.EsiAtLeastEight == true
            && direct.PartialLoopPlan.ExitBranchTaken == true
            && direct.PartialLoopPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityLoopProjection.ExitRawFileOffset
            && direct.DerivedPrefixCapture is FrameHandoffCapacityPrefixCapture directPrefix
            && directPrefix.EntryEsi == 8
            && directPrefix.EntryEdx == 7
            && direct.ExpectedPrefixEntryEsiMatches == true
            && direct.ExpectedPrefixEntryEdxMatches == true
            && direct.ExpectedPrefixEntryEbxMatches == true
            && direct.ExpectedPrefixEntryEbpMatches == true
            && direct.CallEntryEbpIdentityValidated
            && direct.CallEntryEbpIdentityMatches == true
            && !direct.UnreachedCallFrameEbpIdentityCaptureProvided
            && direct.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan directCall
            && directCall.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.DirectCallB5
            && directCall.PrefixPlan.FrameStores.Length == 2
            && directCall.SuccessAdmission
            && direct.SuccessAdmission
            && direct.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixCallProjection.SuccessJoinRawFileOffset);

        FrameHandoffCapacityLoopPrefixPlan doubled =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(7, 6),
                0x3000,
                0x4000,
                8,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x4000, true, 1, true),
                ExpectedPrefixEntryEsi: 7,
                ExpectedPrefixEntryEdx: 6));
        FrameHandoffCapacityLoopPrefixPlan zeroDifferenceAtThreshold =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 8),
                0x3100,
                0x4100,
                0,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x4100, true, 1, null)));
        check("A doubling iteration is blocked even when its stop offset equals the prefix entry, including zero-difference ESI eight",
            doubled.Outcome == FrameHandoffCapacityLoopPrefixOutcome.LoopExitNotAdmitted
            && doubled.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && doubled.PartialLoopPlan.LoopBackReached
            && doubled.PartialLoopPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityLoopProjection.LoopRawFileOffset
            && doubled.PartialLoopPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.ExitRawFileOffset
            && !doubled.LoopExitAdmitted
            && doubled.DerivedPrefixCapture is null
            && doubled.PrefixCallPlan is null
            && !doubled.PrefixCallProjectionReached
            && doubled.UnreachedExpectedPrefixEntryEsiCaptureProvided
            && doubled.UnreachedExpectedPrefixEntryEdxCaptureProvided
            && doubled.UnreachedPrefixReceiverCReadCaptureProvided
            && doubled.UnreachedCallEntryRouteCaptureProvided
            && doubled.UnreachedDirectCallReturnCaptureProvided
            && doubled.UnreachedDirectCallReturnEaxCaptureProvided
            && doubled.UnreachedGuardIndirectCallReturnCaptureProvided
            && !doubled.SuccessAdmission
            && doubled.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.LoopRawFileOffset
            && zeroDifferenceAtThreshold.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.LoopExitNotAdmitted
            && zeroDifferenceAtThreshold.PartialLoopPlan.EntrySubtractResultIsZero == true
            && !zeroDifferenceAtThreshold.PartialLoopPlan.CompareEsiWithEightReached
            && zeroDifferenceAtThreshold.PartialLoopPlan.EsiAfterDouble == 16
            && zeroDifferenceAtThreshold.PrefixCallPlan is null);

        FrameHandoffCapacityLoopPrefixPlan failure =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(0x08000000, 0x08000000),
                0x5000,
                0x6000,
                8,
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0x6000, null, null, true)));
        check("The 0xBE6CC0 failure route preserves its loop plan and never projects the prefix or call",
            failure.Outcome == FrameHandoffCapacityLoopPrefixOutcome.LoopExitNotAdmitted
            && failure.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && failure.PartialLoopPlan.FailureBranchTaken == true
            && failure.PartialLoopPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityLoopProjection.FailureRawFileOffset
            && failure.PrefixCallPlan is null
            && !failure.PrefixCallProjectionReached
            && failure.UnreachedPrefixReceiverCReadCaptureProvided
            && failure.UnreachedCallEntryRouteCaptureProvided
            && failure.UnreachedGuardIndirectCallReturnCaptureProvided
            && !failure.SuccessAdmission
            && failure.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.FailureRawFileOffset);

        FrameHandoffCapacityLoopPrefixPlan identityMismatch =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 7),
                0x7000,
                0x8000,
                4,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x8000, true, 1, null),
                ExpectedPrefixEntryEsi: 9,
                ExpectedPrefixEntryEdx: 7,
                ExpectedPrefixEntryEbx: 0x7001,
                ExpectedPrefixEntryEbp: 0x8000));
        FrameHandoffCapacityLoopPrefixPlan callFrameMismatch =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 7),
                0x7100,
                0x8100,
                4,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x8101, true, 1, true)));
        check("Prefix identity mismatch blocks before stores, while call-frame EBP mismatch preserves earlier prefix effects",
            identityMismatch.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.PrefixEntryIdentityMismatch
            && identityMismatch.DerivedPrefixCapture is FrameHandoffCapacityPrefixCapture mismatchPrefix
            && mismatchPrefix.EntryEsi == 8
            && mismatchPrefix.EntryEdx == 7
            && identityMismatch.PrefixCallPlan is null
            && !identityMismatch.PrefixCallProjectionReached
            && identityMismatch.PrefixEntryIdentityCapturesReached
            && identityMismatch.ExpectedPrefixEntryEsiMatches == false
            && identityMismatch.ExpectedPrefixEntryEdxMatches == true
            && identityMismatch.ExpectedPrefixEntryEbxMatches == false
            && identityMismatch.ExpectedPrefixEntryEbpMatches == true
            && identityMismatch.UnreachedPrefixReceiverCReadCaptureProvided
            && identityMismatch.UnreachedCallEntryRouteCaptureProvided
            && !identityMismatch.SuccessAdmission
            && identityMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset
            && callFrameMismatch.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.PrefixCallBlocked
            && callFrameMismatch.PrefixCallProjectionReached
            && callFrameMismatch.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan frameMismatchCall
            && frameMismatchCall.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.FrameEbpMismatch
            && frameMismatchCall.PrefixPlan.FrameStores.Length == 2
            && frameMismatchCall.PrefixPlan.PushReached
            && frameMismatchCall.CallPlan is null
            && frameMismatchCall.UnreachedDirectCallReturnCaptureProvided
            && frameMismatchCall.UnreachedDirectCallReturnEaxCaptureProvided
            && frameMismatchCall.FrameEbpIdentityValidated
            && frameMismatchCall.FrameEbpIdentityMatches == false
            && callFrameMismatch.CallEntryEbpIdentityValidated
            && callFrameMismatch.CallEntryEbpIdentityMatches == false
            && !callFrameMismatch.UnreachedCallFrameEbpIdentityCaptureProvided
            && !callFrameMismatch.SuccessAdmission);

        FrameHandoffCapacityLoopPrefixPlan missingPrefixRead =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 7),
                0x9000,
                0xA000,
                null,
                new(null, 0xA000, null, 3, true)));
        check("A missing prefix receiver read retains the passed loop, emits no stores, and marks supplied call captures unreached",
            missingPrefixRead.Outcome == FrameHandoffCapacityLoopPrefixOutcome.PrefixCallBlocked
            && missingPrefixRead.PrefixCallProjectionReached
            && missingPrefixRead.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan missingPrefixCall
            && missingPrefixCall.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.MissingPrefixReceiverCRead
            && missingPrefixCall.PrefixPlan.FrameStores.IsEmpty
            && !missingPrefixRead.UnreachedPrefixReceiverCReadCaptureProvided
            && missingPrefixRead.UnreachedCallFrameEbpIdentityCaptureProvided
            && missingPrefixRead.UnreachedDirectCallReturnEaxCaptureProvided
            && missingPrefixRead.UnreachedGuardIndirectCallReturnCaptureProvided
            && !missingPrefixRead.SuccessAdmission
            && missingPrefixRead.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset);

        FrameHandoffCapacityLoopPrefixPlan indirectGuard =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(0x40000000, 0),
                0xB000,
                0xC000,
                8,
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0xC000, null, null, true)));
        FrameHandoffCapacityLoopPrefixPlan zeroPrefixRoute =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(0, 0),
                0xB100,
                0xC100,
                8,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0xC100, true, 1, true)));
        check("The guard indirect route admits its normal return, while the prefix zero route is unreachable after the loop gate",
            indirectGuard.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan guardCall
            && guardCall.ExpectedCallRoute
                == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4
            && guardCall.PrefixPlan.Outcome
                == FrameHandoffCapacityPrefixOutcome.AboveGuardBranchToRawFileOffsetBC4
            && guardCall.CallPlan?.GuardIndirectCallReturnedNormally == true
            && guardCall.SuccessAdmission
            && indirectGuard.SuccessAdmission
            && indirectGuard.CallEntryEbpIdentityValidated
            && indirectGuard.CallEntryEbpIdentityMatches == true
            && !indirectGuard.UnreachedCallFrameEbpIdentityCaptureProvided
            && !guardCall.UnreachedDirectCallReturnCaptureProvided
            && !guardCall.UnreachedGuardIndirectCallReturnCaptureProvided
            && zeroPrefixRoute.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.LoopExitNotAdmitted
            && zeroPrefixRoute.PartialLoopPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityLoopProjection.LoopRawFileOffset
            && zeroPrefixRoute.PrefixCallPlan is null
            && zeroPrefixRoute.UnreachedCallEntryRouteCaptureProvided
            && zeroPrefixRoute.UnreachedCallFrameEbpIdentityCaptureProvided
            && !zeroPrefixRoute.SuccessAdmission);

        FrameHandoffCapacityLoopPrefixPlan partialCall =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(8, 7),
                0xD000,
                0xE000,
                4,
                new(null, 0xE000, null, null, null)));
        check("A partial call capture keeps completed prefix stores and push without claiming call success",
            partialCall.Outcome == FrameHandoffCapacityLoopPrefixOutcome.PrefixCallBlocked
            && partialCall.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan partialCallPlan
            && partialCallPlan.Outcome
                == FrameHandoffCapacityPrefixCallOutcome.CallProjected
            && partialCallPlan.CallPlan?.Outcome
                == FrameHandoffCapacityCallOutcome.MissingDirectCallReturnCapture
            && partialCallPlan.PrefixPlan.FrameStores.Length == 2
            && partialCallPlan.PrefixPlan.PushReached
            && partialCallPlan.CallReached
            && partialCall.CallEntryEbpIdentityValidated
            && partialCall.CallEntryEbpIdentityMatches == true
            && !partialCallPlan.SuccessAdmission
            && !partialCall.SuccessAdmission);

        FrameHandoffCapacityLoopPrefixPlan wrapped =
            FrameHandoffCapacityLoopPrefixProjection.Project(new(
                new(0x3FFFFFFF, 0xFFFFFFFF),
                uint.MaxValue - 11,
                0,
                uint.MaxValue,
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0, true, 1, null),
                ExpectedPrefixEntryEsi: 0x3FFFFFFF,
                ExpectedPrefixEntryEdx: 0xFFFFFFFF,
                ExpectedPrefixEntryEbx: uint.MaxValue - 11,
                ExpectedPrefixEntryEbp: 0));
        check("Loop-derived registers, receiver reads, prefix arithmetic, and frame addresses preserve DWORD wraparound",
            wrapped.Outcome
                == FrameHandoffCapacityLoopPrefixOutcome.PrefixCallSuccessAdmitted
            && wrapped.DerivedPrefixCapture is FrameHandoffCapacityPrefixCapture wrappedPrefix
            && wrappedPrefix.EntryEsi == 0x3FFFFFFF
            && wrappedPrefix.EntryEdx == 0xFFFFFFFF
            && wrapped.PrefixCallPlan is FrameHandoffCapacityPrefixCallPlan wrappedCall
            && wrappedCall.PrefixPlan.ReceiverCReadAddress == 0
            && wrappedCall.PrefixPlan.EsiAfterSubtract == 0x40000000
            && wrappedCall.PrefixPlan.EaxAfterLea == 0x3FFFFFFF
            && wrappedCall.PrefixPlan.EaxAfterScale == 0xFFFFFFFC
            && wrappedCall.PrefixPlan.FrameStores[0].Address == 0xFFFFFFF8
            && wrappedCall.PrefixPlan.FrameStores[1].Address == 0xFFFFFFFC
            && wrappedCall.CallPlan?.FrameWriteAddress == 0xFFFFFFFC
            && wrapped.CallEntryEbpIdentityValidated
            && wrapped.CallEntryEbpIdentityMatches == true
            && wrappedCall.SuccessAdmission
            && wrapped.SuccessAdmission);
    }
}
