using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveForwardingReturnChecks
{
    private const uint FixedTailPayloadPointer = 0xCAFEBABE;
    private const uint FixedCallerReceiverDwordAtOffset8 = 0x76543210;

    public static void Run(Action<string, bool> check)
    {
        check("Both routes compose cleanup and return using the caller's reloaded EAX",
            VerifyRoute(ReliableReceiveForwardingCallerRoute.HighDelta,
                callerReceiver: 0x1000,
                callerPayload: 0x11112222)
            && VerifyRoute(ReliableReceiveForwardingCallerRoute.ExistingRecord,
                callerReceiver: 0x2000,
                callerPayload: 0x33334444));

        ReliableReceiveForwardingReturnPlan mismatchedRoute = Project(
            ReliableReceiveForwardingCallerRoute.HighDelta,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            tailRoute: ReliableReceiveRecycleReturnRoute.ExistingRecord,
            helperReturnedNormally: true,
            helperReturnEax: null);
        ReliableReceiveForwardingReturnPlan mismatchedReceiver = Project(
            ReliableReceiveForwardingCallerRoute.ExistingRecord,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            tailReceiverEax: FixedCallerReceiverDwordAtOffset8 + 1,
            helperReturnedNormally: true,
            helperReturnEax: null);
        check("A route or reloaded-EAX mismatch rejects the tail without completing it",
            mismatchedRoute.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailRouteMismatch
            && mismatchedRoute.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && mismatchedRoute.RecycleReturnPlan is null
            && mismatchedReceiver.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailReceiverPointerMismatch
            && mismatchedReceiver.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && mismatchedReceiver.RecycleReturnPlan is null);

        ReliableReceiveForwardingReturnPlan prematureTail = Project(
            ReliableReceiveForwardingCallerRoute.HighDelta,
            ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset8Capture,
            helperReturnedNormally: true,
            helperReturnEax: null);
        ReliableReceiveForwardingReturnPlan missingTail = Project(
            ReliableReceiveForwardingCallerRoute.ExistingRecord,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            includeTail: false);
        check("An incomplete caller blocks a supplied tail and a completed caller requires one",
            prematureTail.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailCaptureProvidedBeforeCallerCleanup
            && prematureTail.CallerPlan.Outcome
                == ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset8Capture
            && prematureTail.CompletedStages == ReliableReceiveForwardingReturnCompletedStages.None
            && prematureTail.RecycleReturnPlan is null
            && missingTail.StopReason
                == ReliableReceiveForwardingReturnStopReason.MissingTailCapture
            && missingTail.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && missingTail.RecycleReturnPlan is null);

        ReliableReceiveForwardingReturnPlan invalidTailReturn = Project(
            ReliableReceiveForwardingCallerRoute.HighDelta,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            helperReturnedNormally: false,
            helperReturnEax: null);
        ReliableReceiveForwardingReturnPlan unreachedTailEax = Project(
            ReliableReceiveForwardingCallerRoute.ExistingRecord,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            helperReturnedNormally: null,
            helperReturnEax: 0xAABBCCDD);
        ReliableReceiveForwardingReturnPlan alOnlyKnownEax = Project(
            ReliableReceiveForwardingCallerRoute.ExistingRecord,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            helperReturnedNormally: true,
            helperReturnEax: null);
        check("Only a completed tail return completes the composition; AL-only return evidence is sufficient",
            invalidTailReturn.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailDidNotCompleteReturn
            && invalidTailReturn.RecycleReturnPlan?.Outcome
                == ReliableReceiveRecycleReturnOutcome.HelperDidNotReturnNormally
            && invalidTailReturn.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && unreachedTailEax.StopReason
                == ReliableReceiveForwardingReturnStopReason.TailDidNotCompleteReturn
            && unreachedTailEax.RecycleReturnPlan?.Outcome
                == ReliableReceiveRecycleReturnOutcome.UnreachedHelperReturnEaxCapture
            && unreachedTailEax.CompletedStages
                == ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
            && alOnlyKnownEax.StopReason
                == ReliableReceiveForwardingReturnStopReason.ReturnCompleted
            && alOnlyKnownEax.RecycleReturnPlan?.AlValue == 1
            && alOnlyKnownEax.RecycleReturnPlan?.EaxAfterAlWrite is null
            && alOnlyKnownEax.CompletedStages
                == (ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
                    | ReliableReceiveForwardingReturnCompletedStages.RecycleReturn));
    }

    private static bool VerifyRoute(
        ReliableReceiveForwardingCallerRoute route,
        uint callerReceiver,
        uint callerPayload)
    {
        ReliableReceiveForwardingReturnPlan plan = Project(
            route,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            callerReceiver: callerReceiver,
            callerPayload: callerPayload,
            helperReturnedNormally: true,
            helperReturnEax: null);

        return plan.StopReason == ReliableReceiveForwardingReturnStopReason.ReturnCompleted
            && plan.CallerPlan.Outcome == ReliableReceiveForwardingCallerOutcome.CleanupCompleted
            && plan.CallerPlan.Capture.Route == route
            && plan.CallerPlan.Capture.ReceiverBaseAddress == callerReceiver
            && plan.CallerPlan.ReceiverOffset8DwordLoadedIntoEax == FixedCallerReceiverDwordAtOffset8
            && plan.CallerPlan.ReceiverOffset8DwordLoadedIntoEax != callerReceiver
            && plan.RecycleReturnPlan is ReliableReceiveRecycleReturnPlan tail
            && tail.Outcome == ReliableReceiveRecycleReturnOutcome.ReturnCompleted
            && tail.Capture.Route == (ReliableReceiveRecycleReturnRoute)route
            && tail.Capture.ReceiverPointerInEax == FixedCallerReceiverDwordAtOffset8
            && tail.Capture.PayloadPointerInEdi == FixedTailPayloadPointer
            && tail.Capture.PayloadPointerInEdi != callerPayload
            && tail.HelperCallReached
            && tail.AlWriteReached
            && tail.AlValue == 1
            && tail.EaxAfterAlWrite is null
            && tail.EpilogueReached
            && tail.RegistersPopped
            && tail.Ret4Reached
            && plan.CompletedStages
                == (ReliableReceiveForwardingReturnCompletedStages.CallerCleanup
                    | ReliableReceiveForwardingReturnCompletedStages.RecycleReturn)
            && plan.StopAtRawFileOffsetExclusive == tail.StopAtRawFileOffsetExclusive;
    }

    private static ReliableReceiveForwardingReturnPlan Project(
        ReliableReceiveForwardingCallerRoute route,
        ReliableReceiveForwardingCallerOutcome callerOutcome,
        uint callerReceiver = 0x1000,
        uint callerPayload = 0x12345678,
        ReliableReceiveRecycleReturnRoute? tailRoute = null,
        uint? tailReceiverEax = null,
        bool includeTail = true,
        bool? helperReturnedNormally = true,
        uint? helperReturnEax = null)
    {
        ReliableReceiveForwardingCallerCapture callerCapture = callerOutcome switch
        {
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted =>
                new(route, callerReceiver, 0, 0, true, FixedCallerReceiverDwordAtOffset8),
            ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset8Capture =>
                new(route, callerReceiver, 0, 0, true, null),
            _ => throw new ArgumentOutOfRangeException(nameof(callerOutcome))
        };

        ReliableReceiveRecycleReturnCapture? tailCapture = includeTail
            ? new ReliableReceiveRecycleReturnCapture(
                tailRoute ?? (ReliableReceiveRecycleReturnRoute)route,
                tailReceiverEax ?? FixedCallerReceiverDwordAtOffset8,
                FixedTailPayloadPointer,
                OldCounterDword: 0,
                GlobalDword: 0,
                HelperReturnedNormally: helperReturnedNormally,
                HelperReturnEax: helperReturnEax)
            : null;

        return ReliableReceiveForwardingReturnProjection.Project(callerCapture, tailCapture);
    }
}
