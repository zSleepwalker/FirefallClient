using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveSelectorCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition covers only the pinned raw window through the direct normal-return boundary",
            UnreliableReceiveSelectorCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveSelectorCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveSelectorCallbackProjection.StartRawFileOffset == 0x00BE5AF0
            && UnreliableReceiveSelectorCallbackProjection.EndRawFileOffsetExclusive
                == 0x00BE5B64
            && UnreliableReceiveSelectorCallbackProjection.EndVirtualAddressExclusive
                == 0x00FE6764
            && UnreliableReceiveSelectorCallbackProjection.CompleteRawWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x74
            && UnreliableReceiveSelectorCallbackProjection.CompleteRawWindowBytes
                .StartsWith(
                    UnreliableReceiveJoinedHeaderProjection.CapturedInstructionBytes,
                    StringComparison.Ordinal)
            && UnreliableReceiveSelectorCallbackProjection.CompleteRawWindowBytes
                .EndsWith(
                    UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes,
                    StringComparison.Ordinal));

        UnreliableReceiveSelectorCallbackPlan zeroSelector =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                PresenceCapture(0x2000, 0x1000, 3));
        check("A zero selector skips marker and scalar captures and completes only at the direct return",
            zeroSelector.JoinedHeaderPlan.Outcome
                == UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken
            && zeroSelector.MarkerScalarPlan is null
            && zeroSelector.PresenceCallbackPlan?.Outcome
                == UnreliableReceivePresenceCallbackOutcome.DirectCallbackReturnedNormally
            && zeroSelector.PresenceStagePlan?.StackCleanupBytes == 0x10
            && zeroSelector.PresenceCallbackPlan?.DirectCallbackPlan?.CapturedCallbackTarget
                == 0xDEADBEEF
            && zeroSelector.PresenceCallbackPlan?.PresencePlan.CapturedReceiverDwordAt14 == 0
            && zeroSelector.CompositionComplete
            && zeroSelector.SelectedDestinationRawFileOffset == 0x00BE5B64
            && zeroSelector.StopAtRawFileOffsetExclusive == 0x00BE5B64
            && zeroSelector.CompleteRawWindowBytes
                == UnreliableReceiveSelectorCallbackProjection.CompleteRawWindowBytes);

        UnreliableReceiveSelectorCallbackPlan nonzeroSelector =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                MarkerScalarCapture(0x1000, 1, 3, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        check("A nonzero selector requires its normal marker/scalar path and permits an independent frame-byte reload",
            nonzeroSelector.JoinedHeaderPlan.Outcome
                == UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough
            && nonzeroSelector.MarkerScalarPlan?.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperReturnedNormally
            && nonzeroSelector.MarkerScalarPlan?.MarkerCallbackPlan.Capture.PayloadEbx == 0x1000
            && nonzeroSelector.MarkerScalarPlan?.MarkerCallbackPlan.Capture.ValidatedMarker == 1
            && nonzeroSelector.MarkerScalarPlan?.MarkerCallbackPlan.Capture.RetainedSi == 3
            && nonzeroSelector.MarkerScalarPlan?.ScalarPlan?.CapturedCl == 0xA7
            && nonzeroSelector.MarkerScalarPlan?.ScalarPlan?.CapturedCl
                != nonzeroSelector.MarkerScalarPlan?.MarkerCallbackPlan.Capture.ValidatedMarker
            && nonzeroSelector.MarkerPayloadIdentityValidated
            && nonzeroSelector.MarkerValueIdentityValidated
            && nonzeroSelector.MarkerRetainedSiIdentityValidated
            && nonzeroSelector.CompositionComplete
            && nonzeroSelector.SelectedDestinationRawFileOffset == 0x00BE5B64);

        UnreliableReceiveSelectorCallbackPlan zeroWithForbiddenMarkerCaptures =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                MarkerScalarCapture(0x1000, 1, 3, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        check("Marker or scalar captures are unreached on the zero-selector branch and cannot advance",
            zeroWithForbiddenMarkerCaptures.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.UnreachedMarkerScalarCaptureProvided
            && zeroWithForbiddenMarkerCaptures.UnreachedMarkerScalarCaptureProvided
            && zeroWithForbiddenMarkerCaptures.UnreachedMarkerCallbackContinuationCaptureProvided
            && zeroWithForbiddenMarkerCaptures.UnreachedScalarCaptureProvided
            && zeroWithForbiddenMarkerCaptures.UnreachedPresenceCallbackCaptureProvided
            && zeroWithForbiddenMarkerCaptures.PresenceCallbackPlan is null
            && !zeroWithForbiddenMarkerCaptures.CompositionComplete
            && zeroWithForbiddenMarkerCaptures.SelectedDestinationRawFileOffset is null
            && zeroWithForbiddenMarkerCaptures.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        UnreliableReceiveSelectorCallbackPlan nonzeroWithoutMarker =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                null,
                PresenceCapture(0x2000, 0x1000, 3));
        UnreliableReceiveSelectorCallbackPlan missingScalar =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                new(MarkerCallback(0x1000, 1, 3), null),
                PresenceCapture(0x2000, 0x1000, 3));
        check("Nonzero branches require a marker capture and normal scalar helper completion before presence",
            nonzeroWithoutMarker.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.MissingMarkerScalarCapture
            && nonzeroWithoutMarker.PresenceCallbackPlan is null
            && nonzeroWithoutMarker.UnreachedPresenceCallbackCaptureProvided
            && nonzeroWithoutMarker.StopAtRawFileOffsetExclusive == 0x00BE5B16
            && missingScalar.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.MarkerScalarDidNotCompleteNormally
            && missingScalar.MarkerScalarPlan?.Outcome
                == UnreliableReceiveMarkerScalarOutcome.MissingScalarCapture
            && missingScalar.MarkerScalarPlan?.MarkerCallbackPlan.NormalCallBoundaryReached == true
            && missingScalar.PresenceCallbackPlan is null
            && missingScalar.UnreachedPresenceCallbackCaptureProvided
            && missingScalar.StopAtRawFileOffsetExclusive
                == UnreliableReceiveCallbackScalarProjection.EntryRawFileOffset);

        UnreliableReceiveSelectorCallbackPlan badMarkerPayload =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                MarkerScalarCapture(0x1001, 1, 3, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        UnreliableReceiveSelectorCallbackPlan badMarkerValue =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                MarkerScalarCapture(0x1000, 2, 3, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        UnreliableReceiveSelectorCallbackPlan badMarkerLength =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 0x1007),
                MarkerScalarCapture(0x1000, 1, 4, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        check("Marker payload, selector, and retained length identities are checked before marker projection",
            badMarkerPayload.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.MarkerPayloadEbxMismatch
            && badMarkerPayload.MarkerScalarPlan is null
            && badMarkerPayload.UnreachedMarkerCallbackContinuationCaptureProvided
            && badMarkerPayload.UnreachedScalarCaptureProvided
            && badMarkerPayload.PresenceCallbackPlan is null
            && badMarkerValue.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.MarkerValueMismatch
            && badMarkerValue.MarkerScalarPlan is null
            && badMarkerLength.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.MarkerRetainedSiMismatch
            && badMarkerLength.MarkerScalarPlan is null
            && !badMarkerLength.CompositionComplete);

        UnreliableReceiveSelectorCallbackPlan unreachedJoinedHeader =
            UnreliableReceiveSelectorCallbackProjection.Project(
                new(0x1000, 0x2000, 0x3000, null, 0x1007),
                MarkerScalarCapture(0x1000, 1, 3, 0xA7),
                PresenceCapture(0x2000, 0x1000, 3));
        check("A failed joined-header prerequisite blocks matching later destinations and preserves unreached captures",
            unreachedJoinedHeader.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.JoinedHeaderDidNotReachSelector
            && unreachedJoinedHeader.JoinedHeaderPlan.Outcome
                == UnreliableReceiveJoinedHeaderOutcome
                    .MissingPayloadWordAtOffset2WithUnreachedHeaderCapture
            && unreachedJoinedHeader.UnreachedMarkerScalarCaptureProvided
            && unreachedJoinedHeader.UnreachedMarkerCallbackContinuationCaptureProvided
            && unreachedJoinedHeader.UnreachedScalarCaptureProvided
            && unreachedJoinedHeader.UnreachedPresenceCallbackCaptureProvided
            && unreachedJoinedHeader.PresenceCallbackPlan is null
            && unreachedJoinedHeader.SelectedDestinationRawFileOffset is null);

        UnreliableReceiveSelectorCallbackPlan presenceReceiverMismatch =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                PresenceCapture(0x2001, 0x1000, 3));
        check("Presence receiver identity is validated before projecting the receiver gate",
            presenceReceiverMismatch.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.PresenceReceiverEdiMismatch
            && presenceReceiverMismatch.PresenceStagePlan is null
            && presenceReceiverMismatch.PresenceCallbackPlan is null
            && presenceReceiverMismatch.UnreachedDirectCallbackCaptureProvided
            && presenceReceiverMismatch.SelectedDestinationRawFileOffset is null);

        UnreliableReceiveSelectorCallbackPlan directPayloadMismatch =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                PresenceCapture(0x2000, 0x1001, 3));
        UnreliableReceiveSelectorCallbackPlan directLengthMismatch =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                PresenceCapture(0x2000, 0x1000, 4));
        UnreliableReceiveSelectorCallbackPlan directReceiverMismatch =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                PresenceCapture(0x2000, 0x1000, 3, directReceiver: 0x2001));
        check("Direct callback EBX, EDI, and retained SI identities block projection before its target reload",
            directPayloadMismatch.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.DirectCallbackPayloadEbxMismatch
            && directPayloadMismatch.PresenceStagePlan?.StackCleanupBytes == 0x10
            && directPayloadMismatch.PresenceCallbackPlan is null
            && directPayloadMismatch.SelectedDestinationRawFileOffset is null
            && directLengthMismatch.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.DirectCallbackRetainedSiMismatch
            && directLengthMismatch.PresenceStagePlan?.StackCleanupBytes == 0x10
            && directLengthMismatch.PresenceCallbackPlan is null
            && directReceiverMismatch.Outcome
                == UnreliableReceiveSelectorCallbackOutcome.DirectCallbackReceiverEdiMismatch
            && directReceiverMismatch.PresenceStagePlan?.StackCleanupBytes == 0x10
            && directReceiverMismatch.PresenceCallbackPlan is null);

        UnreliableReceiveSelectorCallbackPlan falsePresenceBoundary =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                new(
                    new(0x2000, false, 1, null),
                    DirectCallback(0x2000, 0x1000, 3)));
        UnreliableReceiveSelectorCallbackPlan falseDirectBoundary =
            UnreliableReceiveSelectorCallbackProjection.Project(
                JoinedCapture(0x1000, 0x2000, 7),
                null,
                new(
                    new(0x2000, true, 1, null),
                    new(0x2000, 0x1000, 3, false, null, null, null)));
        check("False presence or direct entry boundaries cannot complete on matching destination captures",
            falsePresenceBoundary.Outcome
                == UnreliableReceiveSelectorCallbackOutcome
                    .PresenceCallbackDidNotCompleteNormally
            && falsePresenceBoundary.PresenceCallbackPlan?.PresencePlan.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.EntryBoundaryNotValidated
            && !falsePresenceBoundary.CompositionComplete
            && falsePresenceBoundary.SelectedDestinationRawFileOffset is null
            && falseDirectBoundary.Outcome
                == UnreliableReceiveSelectorCallbackOutcome
                    .PresenceCallbackDidNotCompleteNormally
            && falseDirectBoundary.PresenceCallbackPlan?.Outcome
                == UnreliableReceivePresenceCallbackOutcome
                    .CallbackEntryBoundaryNotValidated
            && !falseDirectBoundary.CompositionComplete
            && falseDirectBoundary.SelectedDestinationRawFileOffset is null);
    }

    private static UnreliableReceiveJoinedHeaderCapture JoinedCapture(
        uint payload,
        uint receiver,
        ushort header) =>
        new(payload, receiver, 0x3000, 0xA55A, header);

    private static UnreliableReceiveSelectorMarkerScalarCapture MarkerScalarCapture(
        uint payload,
        byte marker,
        ushort retainedSi,
        byte frameByte) =>
        new(
            MarkerCallback(payload, marker, retainedSi),
            new(0x4000, true, 0xAABBCCDD, frameByte, 0x12345678, true));

    private static UnreliableReceiveMarkerCallbackCapture MarkerCallback(
        uint payload,
        byte marker,
        ushort retainedSi) =>
        new(payload, retainedSi, marker, 0xA1B2C3D4, 0x11223344, 0x55667788, true);

    private static UnreliableReceiveSelectorPresenceCallbackCapture PresenceCapture(
        uint receiver,
        uint payload,
        ushort retainedSi,
        uint? directReceiver = null) =>
        new(
            new(receiver, true, 0, true),
            DirectCallback(directReceiver ?? receiver, payload, retainedSi));

    private static UnreliableReceiveDirectCallbackCapture DirectCallback(
        uint receiver,
        uint payload,
        ushort retainedSi) =>
        new(receiver, payload, retainedSi, true, 0x01020304, 0xDEADBEEF, true);
}
