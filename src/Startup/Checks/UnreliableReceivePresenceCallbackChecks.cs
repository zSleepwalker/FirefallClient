using Firefall.Reconstruction.Startup;

internal static class UnreliableReceivePresenceCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete composed raw window spans the presence gate through the direct callback return",
            UnreliableReceivePresenceCallbackProjection.EntryRawFileOffset == 0x00BE5B32
            && UnreliableReceivePresenceCallbackProjection.EndRawFileOffsetExclusive == 0x00BE5B64
            && UnreliableReceivePresenceCallbackProjection.EndVirtualAddress == 0x00FE6764
            && UnreliableReceivePresenceCallbackProjection.CompleteRawWindowBytes
                == UnreliableReceiveCallbackPresenceProjection.CapturedWindowBytes
                    + " "
                    + UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes
            && UnreliableReceivePresenceCallbackProjection.CompleteRawWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 50);

        UnreliableReceivePresenceCallbackPlan nonzeroThenZeroTarget =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0, true));
        check("A nonzero first read admits the callback, whose independent reload may be zero",
            nonzeroThenZeroTarget.PresencePlan.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword
            && nonzeroThenZeroTarget.PresencePlan.CapturedReceiverDwordAt14 == 1
            && nonzeroThenZeroTarget.DirectCallbackPlan?.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally
            && nonzeroThenZeroTarget.DirectCallbackPlan?.CapturedCallbackTarget == 0
            && nonzeroThenZeroTarget.CallbackEntryBoundaryValidated
            && nonzeroThenZeroTarget.SameReceiverEdi
            && nonzeroThenZeroTarget.CompositionComplete
            && nonzeroThenZeroTarget.CompleteRawWindowBytes
                == UnreliableReceivePresenceCallbackProjection.CompleteRawWindowBytes
            && nonzeroThenZeroTarget.SelectedDestinationRawFileOffset == 0x00BE5B64
            && nonzeroThenZeroTarget.StopAtRawFileOffsetExclusive == 0x00BE5B64);

        UnreliableReceivePresenceCallbackPlan diagnosticThenDifferentTarget =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 0, true),
                new(0x10203040, 0x20304050, 0x1234, true, 0xAABBCCDD, 0x55667788, true));
        check("A normal diagnostic return admits a fresh callback target independent of the zero first read",
            diagnosticThenDifferentTarget.PresencePlan.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached
            && diagnosticThenDifferentTarget.PresencePlan.CapturedReceiverDwordAt14 == 0
            && diagnosticThenDifferentTarget.DirectCallbackPlan?.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally
            && diagnosticThenDifferentTarget.DirectCallbackPlan?.CapturedCallbackTarget
                == 0x55667788
            && diagnosticThenDifferentTarget.CompositionComplete);

        UnreliableReceivePresenceCallbackPlan forbiddenUnreachedReturn =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, false),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0, true));
        check("An unreached diagnostic return capture blocks composition despite the matching destination",
            forbiddenUnreachedReturn.PresencePlan.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.UnreachedCaptureProvided
            && forbiddenUnreachedReturn.PresencePlan.SelectedDestinationRawFileOffset == 0x00BE5B52
            && forbiddenUnreachedReturn.Outcome
                == UnreliableReceivePresenceCallbackOutcome.PresenceDidNotAdmitCallback
            && forbiddenUnreachedReturn.UnreachedCallbackCaptureProvided
            && forbiddenUnreachedReturn.DirectCallbackPlan is null
            && !forbiddenUnreachedReturn.CompositionComplete
            && forbiddenUnreachedReturn.CompleteRawWindowBytes is null
            && forbiddenUnreachedReturn.SelectedDestinationRawFileOffset is null);

        UnreliableReceivePresenceCallbackPlan missingCallback =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                null);
        UnreliableReceivePresenceCallbackPlan invalidPresenceEntry =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, false, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0, true));
        UnreliableReceivePresenceCallbackPlan invalidCallbackEntry =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, false, null, null, null));
        UnreliableReceivePresenceCallbackPlan receiverMismatch =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203041, 0x20304050, 7, true, 0xAABBCCDD, 0, true));
        check("Missing or invalid entry and receiver identity prevent callback composition",
            missingCallback.Outcome
                == UnreliableReceivePresenceCallbackOutcome.MissingCallbackCapture
            && missingCallback.DirectCallbackPlan is null
            && !missingCallback.CompositionComplete
            && invalidPresenceEntry.PresencePlan.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.EntryBoundaryNotValidated
            && invalidPresenceEntry.DirectCallbackPlan is null
            && !invalidPresenceEntry.CompositionComplete
            && invalidCallbackEntry.Outcome
                == UnreliableReceivePresenceCallbackOutcome.CallbackEntryBoundaryNotValidated
            && invalidCallbackEntry.DirectCallbackPlan is null
            && !invalidCallbackEntry.CompositionComplete
            && receiverMismatch.Outcome
                == UnreliableReceivePresenceCallbackOutcome.ReceiverEdiMismatch
            && receiverMismatch.DirectCallbackPlan is null
            && !receiverMismatch.SameReceiverEdi
            && !receiverMismatch.CompositionComplete);

        UnreliableReceivePresenceCallbackPlan missingPayloadDword =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, null, null, null));
        UnreliableReceivePresenceCallbackPlan missingCallbackTarget =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, null, null));
        UnreliableReceivePresenceCallbackPlan missingCallbackReturn =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0, null));
        UnreliableReceivePresenceCallbackPlan nonReturningCallback =
            UnreliableReceivePresenceCallbackProjection.Project(
                new(0x10203040, true, 1, null),
                new(0x10203040, 0x20304050, 7, true, 0xAABBCCDD, 0, false));
        uint[] expectedPushes = { 0, 7, 0xAABBCCDD, 0x20304058 };
        check("Callback failures preserve direct-projection outcomes and pushes without completing the raw window",
            missingPayloadDword.Outcome
                == UnreliableReceivePresenceCallbackOutcome.MissingPayloadDwordAt4
            && missingPayloadDword.DirectCallbackPlan?.StackPushesInInstructionOrder
                .SequenceEqual(new uint[] { 0, 7 }) == true
            && missingCallbackTarget.Outcome
                == UnreliableReceivePresenceCallbackOutcome.MissingReceiverDwordAt14
            && missingCallbackTarget.DirectCallbackPlan?.StackPushesInInstructionOrder
                .SequenceEqual(expectedPushes) == true
            && missingCallbackReturn.Outcome
                == UnreliableReceivePresenceCallbackOutcome.MissingNormalReturn
            && missingCallbackReturn.DirectCallbackPlan?.StackPushesInInstructionOrder
                .SequenceEqual(expectedPushes) == true
            && nonReturningCallback.Outcome
                == UnreliableReceivePresenceCallbackOutcome.DirectCallbackDidNotReturnNormally
            && nonReturningCallback.DirectCallbackPlan?.StackPushesInInstructionOrder
                .SequenceEqual(expectedPushes) == true
            && !nonReturningCallback.CompositionComplete
            && nonReturningCallback.CompleteRawWindowBytes is null
            && nonReturningCallback.SelectedDestinationRawFileOffset is null);
    }
}
