namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveSelectorMarkerScalarCapture(
    UnreliableReceiveMarkerCallbackCapture? MarkerCallback,
    UnreliableReceiveCallbackScalarCapture? Scalar)
{
    public bool HasAnyCapture => MarkerCallback.HasValue || Scalar.HasValue;
}

public readonly record struct UnreliableReceiveSelectorPresenceCallbackCapture(
    UnreliableReceiveCallbackPresenceCapture? Presence,
    UnreliableReceiveDirectCallbackCapture? DirectCallback)
{
    public bool HasAnyCapture => Presence.HasValue || DirectCallback.HasValue;
}

public enum UnreliableReceiveSelectorCallbackOutcome
{
    JoinedHeaderDidNotReachSelector,
    UnreachedMarkerScalarCaptureProvided,
    MissingMarkerScalarCapture,
    MissingMarkerCallbackCapture,
    MarkerPayloadEbxMismatch,
    MarkerValueMismatch,
    MarkerRetainedSiMismatch,
    MarkerScalarDidNotCompleteNormally,
    MissingPresenceCallbackCapture,
    PresenceReceiverEdiMismatch,
    PresenceCallbackDidNotCompleteNormally,
    DirectCallbackReceiverEdiMismatch,
    DirectCallbackPayloadEbxMismatch,
    DirectCallbackRetainedSiMismatch,
    DirectCallbackReturnedNormally
}

public readonly record struct UnreliableReceiveSelectorCallbackPlan(
    UnreliableReceiveJoinedHeaderPlan JoinedHeaderPlan,
    UnreliableReceiveMarkerScalarPlan? MarkerScalarPlan,
    UnreliableReceiveCallbackPresencePlan? PresenceStagePlan,
    UnreliableReceivePresenceCallbackPlan? PresenceCallbackPlan,
    UnreliableReceiveSelectorCallbackOutcome Outcome,
    bool UnreachedMarkerScalarCaptureProvided,
    bool UnreachedMarkerCallbackContinuationCaptureProvided,
    bool UnreachedScalarCaptureProvided,
    bool UnreachedPresenceCallbackCaptureProvided,
    bool UnreachedDirectCallbackCaptureProvided,
    bool MarkerPayloadIdentityValidated,
    bool MarkerValueIdentityValidated,
    bool MarkerRetainedSiIdentityValidated,
    bool PresenceReceiverIdentityValidated,
    bool DirectReceiverIdentityValidated,
    bool DirectPayloadIdentityValidated,
    bool DirectRetainedSiIdentityValidated,
    bool CompositionComplete,
    uint? SelectedDestinationRawFileOffset,
    string? CompleteRawWindowBytes,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the joined selector branch, its optional nonzero-marker scalar path,
/// and the receiver-presence/direct-callback path without inferring callback targets.
/// </summary>
public static class UnreliableReceiveSelectorCallbackProjection
{
    public const string SamplePath = UnreliableReceiveJoinedHeaderProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveJoinedHeaderProjection.SampleSha256;

    public const uint StartRawFileOffset = UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryRawFileOffset;
    public const uint EndVirtualAddressExclusive =
        UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryVirtualAddress;

    public const string CompleteRawWindowBytes =
        UnreliableReceiveJoinedHeaderProjection.CapturedInstructionBytes
        + " "
        + UnreliableReceiveMarkerCallbackProjection.CapturedCallbackBytes
        + " "
        + UnreliableReceiveCallbackScalarProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveCallbackPresenceProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes;

    public static UnreliableReceiveSelectorCallbackPlan Project(
        UnreliableReceiveJoinedHeaderCapture joinedHeaderCapture,
        UnreliableReceiveSelectorMarkerScalarCapture? markerScalarCapture,
        UnreliableReceiveSelectorPresenceCallbackCapture? presenceCallbackCapture)
    {
        UnreliableReceiveJoinedHeaderPlan joinedPlan =
            UnreliableReceiveJoinedHeaderProjection.Project(joinedHeaderCapture);
        bool zeroSelectorBranch =
            joinedPlan.Outcome == UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken;
        bool nonzeroSelectorBranch =
            joinedPlan.Outcome == UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough;

        if ((!zeroSelectorBranch && !nonzeroSelectorBranch)
            || joinedPlan.UnreachedPayloadHeaderCaptureProvided)
        {
            return Stop(
                joinedPlan,
                UnreliableReceiveSelectorCallbackOutcome.JoinedHeaderDidNotReachSelector,
                unreachedMarkerScalarCaptureProvided:
                    markerScalarCapture is { HasAnyCapture: true },
                unreachedMarkerCallbackContinuationCaptureProvided:
                    HasMarkerContinuationCaptures(markerScalarCapture),
                unreachedScalarCaptureProvided:
                    markerScalarCapture?.Scalar.HasValue == true,
                unreachedPresenceCallbackCaptureProvided:
                    presenceCallbackCapture is { HasAnyCapture: true },
                unreachedDirectCallbackCaptureProvided:
                    presenceCallbackCapture?.DirectCallback.HasValue == true,
                stopAt: joinedPlan.StopAtRawFileOffsetExclusive);
        }

        UnreliableReceiveMarkerScalarPlan? markerScalarPlan = null;
        if (zeroSelectorBranch)
        {
            if (markerScalarCapture is { HasAnyCapture: true })
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome
                        .UnreachedMarkerScalarCaptureProvided,
                    unreachedMarkerScalarCaptureProvided: true,
                    unreachedMarkerCallbackContinuationCaptureProvided:
                        HasMarkerContinuationCaptures(markerScalarCapture),
                    unreachedScalarCaptureProvided:
                        markerScalarCapture.Value.Scalar.HasValue,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveJoinedHeaderProjection.ZeroBranchTargetRawFileOffset);
            }
        }
        else
        {
            if (markerScalarCapture is not UnreliableReceiveSelectorMarkerScalarCapture markerGroup)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MissingMarkerScalarCapture,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset);
            }

            if (markerGroup.MarkerCallback is not UnreliableReceiveMarkerCallbackCapture marker)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MissingMarkerCallbackCapture,
                    unreachedScalarCaptureProvided: markerGroup.Scalar.HasValue,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset);
            }

            uint joinedSelector = joinedPlan.EcxFrameArgumentDword!.Value;
            ushort joinedLength = unchecked((ushort)joinedPlan.EsiFrameLocalDword!.Value);
            bool markerPayloadMatches =
                marker.PayloadEbx == joinedHeaderCapture.PayloadBaseAddress;
            bool markerValueMatches = marker.ValidatedMarker == joinedSelector;
            bool markerLengthMatches = marker.RetainedSi == joinedLength;

            if (!markerPayloadMatches)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MarkerPayloadEbxMismatch,
                    unreachedMarkerCallbackContinuationCaptureProvided:
                        HasMarkerContinuationCaptures(markerGroup.MarkerCallback),
                    unreachedScalarCaptureProvided: markerGroup.Scalar.HasValue,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset);
            }

            if (!markerValueMatches)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MarkerValueMismatch,
                    unreachedMarkerCallbackContinuationCaptureProvided:
                        HasMarkerContinuationCaptures(markerGroup.MarkerCallback),
                    unreachedScalarCaptureProvided: markerGroup.Scalar.HasValue,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset);
            }

            if (!markerLengthMatches)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MarkerRetainedSiMismatch,
                    unreachedMarkerCallbackContinuationCaptureProvided:
                        HasMarkerContinuationCaptures(markerGroup.MarkerCallback),
                    unreachedScalarCaptureProvided: markerGroup.Scalar.HasValue,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset);
            }

            UnreliableReceiveMarkerScalarPlan projectedMarkerScalarPlan =
                UnreliableReceiveMarkerScalarProjection.Project(
                    marker,
                    markerGroup.Scalar);
            markerScalarPlan = projectedMarkerScalarPlan;
            if (projectedMarkerScalarPlan.Outcome
                != UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperReturnedNormally)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.MarkerScalarDidNotCompleteNormally,
                    markerScalarPlan: projectedMarkerScalarPlan,
                    markerPayloadIdentityValidated: true,
                    markerValueIdentityValidated: true,
                    markerRetainedSiIdentityValidated: true,
                    unreachedPresenceCallbackCaptureProvided:
                        presenceCallbackCapture is { HasAnyCapture: true },
                    unreachedDirectCallbackCaptureProvided:
                        presenceCallbackCapture?.DirectCallback.HasValue == true,
                    stopAt: projectedMarkerScalarPlan.StopAtRawFileOffsetExclusive);
            }
        }

        if (presenceCallbackCapture is not UnreliableReceiveSelectorPresenceCallbackCapture presenceGroup
            || presenceGroup.Presence is not UnreliableReceiveCallbackPresenceCapture presence)
        {
            return Stop(
                joinedPlan,
                UnreliableReceiveSelectorCallbackOutcome.MissingPresenceCallbackCapture,
                markerScalarPlan: markerScalarPlan,
                markerPayloadIdentityValidated: nonzeroSelectorBranch,
                markerValueIdentityValidated: nonzeroSelectorBranch,
                markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
                unreachedDirectCallbackCaptureProvided:
                    presenceCallbackCapture?.DirectCallback.HasValue == true,
                stopAt: UnreliableReceiveCallbackPresenceProjection.EntryRawFileOffset);
        }

        if (presence.EntryBoundaryValidated
            && presence.ReceiverEdi != joinedHeaderCapture.ReceiverBaseAddress)
        {
            return Stop(
                joinedPlan,
                UnreliableReceiveSelectorCallbackOutcome.PresenceReceiverEdiMismatch,
                markerScalarPlan: markerScalarPlan,
                markerPayloadIdentityValidated: nonzeroSelectorBranch,
                markerValueIdentityValidated: nonzeroSelectorBranch,
                markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
                unreachedDirectCallbackCaptureProvided: presenceGroup.DirectCallback.HasValue,
                stopAt: UnreliableReceiveCallbackPresenceProjection.EntryRawFileOffset);
        }

        UnreliableReceiveCallbackPresencePlan presenceStagePlan =
            UnreliableReceiveCallbackPresenceProjection.Project(presence);
        bool presenceAdmitsDirectCallback =
            presenceStagePlan.Outcome
                is UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword
                    or UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached;

        if (presenceAdmitsDirectCallback
            && presenceGroup.DirectCallback is UnreliableReceiveDirectCallbackCapture direct
            && direct.EntryBoundaryValidated)
        {
            if (direct.ReceiverEdi != joinedHeaderCapture.ReceiverBaseAddress)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.DirectCallbackReceiverEdiMismatch,
                    markerScalarPlan: markerScalarPlan,
                    presenceStagePlan: presenceStagePlan,
                    markerPayloadIdentityValidated: nonzeroSelectorBranch,
                    markerValueIdentityValidated: nonzeroSelectorBranch,
                    markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
                    presenceReceiverIdentityValidated: true,
                    stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
            }

            if (direct.PayloadEbx != joinedHeaderCapture.PayloadBaseAddress)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.DirectCallbackPayloadEbxMismatch,
                    markerScalarPlan: markerScalarPlan,
                    presenceStagePlan: presenceStagePlan,
                    markerPayloadIdentityValidated: nonzeroSelectorBranch,
                    markerValueIdentityValidated: nonzeroSelectorBranch,
                    markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
                    presenceReceiverIdentityValidated: true,
                    directReceiverIdentityValidated: true,
                    stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
            }

            ushort joinedLength =
                unchecked((ushort)joinedPlan.EsiFrameLocalDword!.Value);
            if (direct.RetainedSi != joinedLength)
            {
                return Stop(
                    joinedPlan,
                    UnreliableReceiveSelectorCallbackOutcome.DirectCallbackRetainedSiMismatch,
                    markerScalarPlan: markerScalarPlan,
                    presenceStagePlan: presenceStagePlan,
                    markerPayloadIdentityValidated: nonzeroSelectorBranch,
                    markerValueIdentityValidated: nonzeroSelectorBranch,
                    markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
                    presenceReceiverIdentityValidated: true,
                    directReceiverIdentityValidated: true,
                    directPayloadIdentityValidated: true,
                    stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
            }
        }

        UnreliableReceivePresenceCallbackPlan presenceCallbackPlan =
            UnreliableReceivePresenceCallbackProjection.Project(
                presence,
                presenceGroup.DirectCallback);
        bool complete =
            presenceCallbackPlan.Outcome
                == UnreliableReceivePresenceCallbackOutcome.DirectCallbackReturnedNormally;
        return Stop(
            joinedPlan,
            complete
                ? UnreliableReceiveSelectorCallbackOutcome.DirectCallbackReturnedNormally
                : UnreliableReceiveSelectorCallbackOutcome
                    .PresenceCallbackDidNotCompleteNormally,
            markerScalarPlan: markerScalarPlan,
            presenceStagePlan: presenceCallbackPlan.PresencePlan,
            presenceCallbackPlan: presenceCallbackPlan,
            markerPayloadIdentityValidated: nonzeroSelectorBranch,
            markerValueIdentityValidated: nonzeroSelectorBranch,
            markerRetainedSiIdentityValidated: nonzeroSelectorBranch,
            presenceReceiverIdentityValidated: presence.EntryBoundaryValidated,
            directReceiverIdentityValidated:
                complete || DirectIdentityWasChecked(
                    presenceAdmitsDirectCallback,
                    presenceGroup.DirectCallback),
            directPayloadIdentityValidated:
                complete || DirectIdentityWasChecked(
                    presenceAdmitsDirectCallback,
                    presenceGroup.DirectCallback),
            directRetainedSiIdentityValidated:
                complete || DirectIdentityWasChecked(
                    presenceAdmitsDirectCallback,
                    presenceGroup.DirectCallback),
            unreachedDirectCallbackCaptureProvided:
                !presenceAdmitsDirectCallback && presenceGroup.DirectCallback.HasValue,
            compositionComplete: complete,
            selectedDestinationRawFileOffset: complete
                ? UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryRawFileOffset
                : null,
            completeRawWindowBytes: complete ? CompleteRawWindowBytes : null,
            stopAt: presenceCallbackPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool DirectIdentityWasChecked(
        bool presenceAdmitsDirectCallback,
        UnreliableReceiveDirectCallbackCapture? directCallback) =>
        presenceAdmitsDirectCallback
        && directCallback is { EntryBoundaryValidated: true };

    private static bool HasMarkerContinuationCaptures(
        UnreliableReceiveSelectorMarkerScalarCapture? capture) =>
        capture?.MarkerCallback is UnreliableReceiveMarkerCallbackCapture marker
        && HasMarkerContinuationCaptures(marker);

    private static bool HasMarkerContinuationCaptures(
        UnreliableReceiveMarkerCallbackCapture? capture) =>
        capture is UnreliableReceiveMarkerCallbackCapture marker
        && (marker.PayloadDwordAt8.HasValue
            || marker.PayloadDwordAt4.HasValue
            || marker.IndirectTargetAtPayloadDword8Plus4.HasValue
            || marker.IndirectCallReturnedNormally.HasValue);

    private static UnreliableReceiveSelectorCallbackPlan Stop(
        UnreliableReceiveJoinedHeaderPlan joinedHeaderPlan,
        UnreliableReceiveSelectorCallbackOutcome outcome,
        UnreliableReceiveMarkerScalarPlan? markerScalarPlan = null,
        UnreliableReceiveCallbackPresencePlan? presenceStagePlan = null,
        UnreliableReceivePresenceCallbackPlan? presenceCallbackPlan = null,
        bool unreachedMarkerScalarCaptureProvided = false,
        bool unreachedMarkerCallbackContinuationCaptureProvided = false,
        bool unreachedScalarCaptureProvided = false,
        bool unreachedPresenceCallbackCaptureProvided = false,
        bool unreachedDirectCallbackCaptureProvided = false,
        bool markerPayloadIdentityValidated = false,
        bool markerValueIdentityValidated = false,
        bool markerRetainedSiIdentityValidated = false,
        bool presenceReceiverIdentityValidated = false,
        bool directReceiverIdentityValidated = false,
        bool directPayloadIdentityValidated = false,
        bool directRetainedSiIdentityValidated = false,
        bool compositionComplete = false,
        uint? selectedDestinationRawFileOffset = null,
        string? completeRawWindowBytes = null,
        uint? stopAt = null) =>
        new(
            joinedHeaderPlan,
            markerScalarPlan,
            presenceStagePlan,
            presenceCallbackPlan,
            outcome,
            unreachedMarkerScalarCaptureProvided,
            unreachedMarkerCallbackContinuationCaptureProvided,
            unreachedScalarCaptureProvided,
            unreachedPresenceCallbackCaptureProvided,
            unreachedDirectCallbackCaptureProvided,
            markerPayloadIdentityValidated,
            markerValueIdentityValidated,
            markerRetainedSiIdentityValidated,
            presenceReceiverIdentityValidated,
            directReceiverIdentityValidated,
            directPayloadIdentityValidated,
            directRetainedSiIdentityValidated,
            compositionComplete,
            selectedDestinationRawFileOffset,
            completeRawWindowBytes,
            stopAt ?? UnreliableReceiveJoinedHeaderProjection.StartRawFileOffset);
}
