using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveCallbackRecycleJoinChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The join spans the complete pinned raw window and its mapped virtual-address range",
            UnreliableReceiveCallbackRecycleJoinProjection.EntryRawFileOffset == 0x00BE5B32
            && UnreliableReceiveCallbackRecycleJoinProjection.EndRawFileOffsetExclusive
                == 0x00BE5B78
            && UnreliableReceiveCallbackRecycleJoinProjection.EndVirtualAddress == 0x00FE6778
            && UnreliableReceiveCallbackRecycleJoinProjection.CompleteRawWindowBytes
                == UnreliableReceivePresenceCallbackProjection.CompleteRawWindowBytes
                    + " "
                    + UnreliableReceiveCallbackRecycleProjection.CapturedWindowBytes
            && UnreliableReceiveCallbackRecycleJoinProjection.CompleteRawWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 70);

        UnreliableReceiveCallbackRecycleJoinPlan complete =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 0xCAFED00D, 0x11223344, true));
        check("Only a normal direct return joins matching payload captures through recycle return",
            complete.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.RecycleHelperReturnedNormally
            && complete.PresenceCallbackPlan?.Outcome
                == UnreliableReceivePresenceCallbackOutcome.DirectCallbackReturnedNormally
            && complete.PresenceCallbackPlan?.PresencePlan.CapturedReceiverDwordAt14 == 1
            && complete.PresenceCallbackPlan?.DirectCallbackPlan?.CapturedCallbackTarget
                == 0x55667788
            && complete.RecyclePayloadEbxMatchesDirect == true
            && complete.RecyclePriorCallbackReturnedNormally == true
            && complete.RecycleFrameEbpExplicitlyCaptured
            && complete.RecycleAdmittedByNormalDirectCallback
            && complete.RecyclePlan?.CapturedGlobalDword == 0xCAFED00D
            && complete.RecyclePlan?.CapturedCallbackEax == 0x11223344
            && complete.RecyclePlan?.FrameDwordWritten == 0x11223344
            && complete.RecyclePlan?.FrameDwordAddress == 0x30405068
            && complete.RecyclePlan?.CallEcx == 0xCAFED00D
            && complete.CompositionComplete
            && complete.SelectedDestinationRawFileOffset == 0x00BE5B78
            && complete.CompleteRawWindowBytes
                == UnreliableReceiveCallbackRecycleJoinProjection.CompleteRawWindowBytes
            && complete.StopAtRawFileOffsetExclusive == 0x00BE5B78);

        UnreliableReceiveCallbackRecycleJoinPlan directDidNotReturn =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, false),
                new(true, 0x30405060, 0x20304050, 0xCAFED00D, 0x11223344, true));
        UnreliableReceiveCallbackRecycleJoinPlan failedPresence =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 0, false),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 0xCAFED00D, 0x11223344, true));
        check("A failed callback or presence path cannot advance merely because later boundary captures exist",
            directDidNotReturn.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.DirectCallbackDidNotReturnNormally
            && directDidNotReturn.UnreachedRecycleCaptureProvided
            && directDidNotReturn.UnreachedRecycleGlobalDwordCaptureProvided
            && directDidNotReturn.UnreachedRecycleCallbackEaxCaptureProvided
            && directDidNotReturn.UnreachedRecycleHelperReturnCaptureProvided
            && directDidNotReturn.PresenceCallbackPlan?.StopAtRawFileOffsetExclusive
                == 0x00BE5B62
            && directDidNotReturn.RecyclePlan is null
            && !directDidNotReturn.RecycleAdmittedByNormalDirectCallback
            && !directDidNotReturn.CompositionComplete
            && failedPresence.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.PresenceDidNotAdmitCallback
            && failedPresence.UnreachedDirectCaptureProvided
            && failedPresence.UnreachedRecycleCaptureProvided
            && failedPresence.PresenceCallbackPlan?.StopAtRawFileOffsetExclusive
                == 0x00BE5B49
            && failedPresence.RecyclePlan is null
            && !failedPresence.CompositionComplete);

        UnreliableReceiveCallbackRecycleJoinPlan missingPresence =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                null,
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 1, 2, true));
        UnreliableReceiveCallbackRecycleJoinPlan invalidDirect =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, false, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 1, 2, true));
        check("Missing or invalid earlier captures retain explicit unreached-capture state",
            missingPresence.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.MissingPresenceCapture
            && missingPresence.UnreachedDirectCaptureProvided
            && missingPresence.UnreachedRecycleCaptureProvided
            && missingPresence.UnreachedRecycleGlobalDwordCaptureProvided
            && missingPresence.UnreachedRecycleCallbackEaxCaptureProvided
            && missingPresence.UnreachedRecycleHelperReturnCaptureProvided
            && missingPresence.StopAtRawFileOffsetExclusive == 0x00BE5B32
            && invalidDirect.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.DirectCallbackIncomplete
            && invalidDirect.PresenceCallbackPlan?.DirectCallbackPlan is null
            && invalidDirect.PresenceCallbackPlan?.Outcome
                == UnreliableReceivePresenceCallbackOutcome.CallbackEntryBoundaryNotValidated
            && invalidDirect.UnreachedRecycleCaptureProvided
            && invalidDirect.RecyclePlan is null
            && invalidDirect.StopAtRawFileOffsetExclusive == 0x00BE5B52);

        UnreliableReceiveCallbackRecycleJoinPlan payloadMismatch =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304051, 0xCAFED00D, 0x11223344, true));
        check("A recycle payload mismatch blocks global read, cleanup, frame write, push, and helper call",
            payloadMismatch.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePayloadEbxMismatch
            && payloadMismatch.RecyclePayloadEbxMatchesDirect == false
            && payloadMismatch.RecyclePriorCallbackReturnedNormally == true
            && payloadMismatch.RecycleFrameEbpExplicitlyCaptured
            && payloadMismatch.UnreachedRecycleGlobalDwordCaptureProvided
            && payloadMismatch.UnreachedRecycleCallbackEaxCaptureProvided
            && payloadMismatch.UnreachedRecycleHelperReturnCaptureProvided
            && payloadMismatch.RecyclePlan is null
            && payloadMismatch.RecycleAdmittedByNormalDirectCallback
            && payloadMismatch.StopAtRawFileOffsetExclusive == 0x00BE5B64
            && !payloadMismatch.CompositionComplete);

        UnreliableReceiveCallbackRecycleJoinPlan missingPriorReturn =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(null, 0x30405060, 0x20304050, 0xCAFED00D, 0x11223344, true));
        UnreliableReceiveCallbackRecycleJoinPlan priorDidNotReturn =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(false, 0x30405060, 0x20304050, 0xCAFED00D, 0x11223344, true));
        check("Recycle prior-return evidence must be explicitly true before any continuation side effect",
            missingPriorReturn.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePriorCallbackMissing
            && missingPriorReturn.RecyclePayloadEbxMatchesDirect == true
            && missingPriorReturn.RecyclePlan?.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
            && !missingPriorReturn.RecyclePlan!.Value.GlobalDwordRead
            && !missingPriorReturn.RecyclePlan.Value.StackCleanupReached
            && missingPriorReturn.RecycleFrameEbpExplicitlyCaptured
            && priorDidNotReturn.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.RecyclePriorCallbackDidNotReturnNormally
            && !priorDidNotReturn.RecyclePlan!.Value.GlobalDwordRead
            && !priorDidNotReturn.RecyclePlan.Value.StackCleanupReached
            && !priorDidNotReturn.RecyclePlan.Value.FrameDwordWriteReached
            && priorDidNotReturn.RecyclePlan.Value.UnreachedGlobalDwordCaptureProvided
            && priorDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE5B64);

        UnreliableReceiveCallbackRecycleJoinPlan missingCallbackEax =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 0xCAFED00D, null, null));
        check("The recycle EAX is an independent capture and missing EAX preserves cleanup without inventing a value",
            missingCallbackEax.Outcome
                == UnreliableReceiveCallbackRecycleJoinOutcome.RecycleIncomplete
            && missingCallbackEax.PresenceCallbackPlan?.DirectCallbackPlan?.CapturedCallbackTarget
                == 0x55667788
            && missingCallbackEax.RecyclePlan?.Outcome
                == UnreliableReceiveCallbackRecycleOutcome.MissingCallbackEaxAtCopy
            && missingCallbackEax.RecyclePlan?.CapturedGlobalDword == 0xCAFED00D
            && missingCallbackEax.RecyclePlan?.StackCleanupReached == true
            && missingCallbackEax.RecyclePlan?.StackCleanupBytes == 0x10
            && !missingCallbackEax.RecyclePlan!.Value.CallbackEaxCopied
            && missingCallbackEax.RecyclePlan.Value.CapturedCallbackEax is null
            && !missingCallbackEax.RecyclePlan.Value.FrameDwordWriteReached
            && !missingCallbackEax.RecyclePlan.Value.RecycleHelperCallReached
            && missingCallbackEax.StopAtRawFileOffsetExclusive == 0x00BE5B6D);

        UnreliableReceiveCallbackRecycleJoinPlan independentValues =
            UnreliableReceiveCallbackRecycleJoinProjection.Project(
                new(0x10203040, true, 0x1234, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0x55667788, true),
                new(true, 0x30405060, 0x20304050, 0x01020304, 0xA1B2C3D4, true));
        check("Presence selector, callback target, callback return EAX, and global DWORD stay independent",
            independentValues.PresenceCallbackPlan?.PresencePlan.CapturedReceiverDwordAt14
                == 0x1234
            && independentValues.PresenceCallbackPlan?.DirectCallbackPlan?.CapturedCallbackTarget
                == 0x55667788
            && independentValues.RecyclePlan?.CapturedCallbackEax == 0xA1B2C3D4
            && independentValues.RecyclePlan?.CapturedCallbackEax
                != independentValues.PresenceCallbackPlan?.DirectCallbackPlan?.CapturedCallbackTarget
            && independentValues.RecyclePlan?.CapturedGlobalDword == 0x01020304
            && independentValues.RecyclePlan?.CapturedGlobalDword
                != independentValues.RecyclePlan?.CapturedCallbackEax);
    }
}
