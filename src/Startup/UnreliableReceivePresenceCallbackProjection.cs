namespace Firefall.Reconstruction.Startup;

public enum UnreliableReceivePresenceCallbackOutcome
{
    PresenceDidNotAdmitCallback,
    MissingCallbackCapture,
    CallbackEntryBoundaryNotValidated,
    ReceiverEdiMismatch,
    DirectCallbackUnreachedCaptureProvided,
    MissingPayloadDwordAt4,
    MissingReceiverDwordAt14,
    MissingNormalReturn,
    DirectCallbackDidNotReturnNormally,
    DirectCallbackReturnedNormally
}

public readonly record struct UnreliableReceivePresenceCallbackPlan(
    UnreliableReceiveCallbackPresencePlan PresencePlan,
    UnreliableReceiveDirectCallbackPlan? DirectCallbackPlan,
    UnreliableReceivePresenceCallbackOutcome Outcome,
    bool UnreachedCallbackCaptureProvided,
    bool CallbackEntryBoundaryValidated,
    bool SameReceiverEdi,
    bool CompositionComplete,
    uint? SelectedDestinationRawFileOffset,
    string? CompleteRawWindowBytes,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the receiver-presence route with the callback's independent receiver-target reload.
/// </summary>
public static class UnreliableReceivePresenceCallbackProjection
{
    public const uint EntryRawFileOffset =
        UnreliableReceiveCallbackPresenceProjection.EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryRawFileOffset;
    public const uint EndVirtualAddress =
        UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryVirtualAddress;

    public const string CompleteRawWindowBytes =
        UnreliableReceiveCallbackPresenceProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes;

    public static UnreliableReceivePresenceCallbackPlan Project(
        UnreliableReceiveCallbackPresenceCapture presenceCapture,
        UnreliableReceiveDirectCallbackCapture? callbackCapture)
    {
        UnreliableReceiveCallbackPresencePlan presencePlan =
            UnreliableReceiveCallbackPresenceProjection.Project(presenceCapture);

        if (presencePlan.Outcome
            is not (UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword
                or UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached))
        {
            return Stop(
                presencePlan,
                UnreliableReceivePresenceCallbackOutcome.PresenceDidNotAdmitCallback,
                unreachedCallbackCaptureProvided: callbackCapture.HasValue,
                stopAt: presencePlan.StopAtRawFileOffsetExclusive);
        }

        if (callbackCapture is not UnreliableReceiveDirectCallbackCapture callback)
        {
            return Stop(
                presencePlan,
                UnreliableReceivePresenceCallbackOutcome.MissingCallbackCapture,
                stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
        }

        if (!callback.EntryBoundaryValidated)
        {
            return Stop(
                presencePlan,
                UnreliableReceivePresenceCallbackOutcome.CallbackEntryBoundaryNotValidated,
                callbackEntryBoundaryValidated: false,
                stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
        }

        if (callback.ReceiverEdi != presenceCapture.ReceiverEdi)
        {
            return Stop(
                presencePlan,
                UnreliableReceivePresenceCallbackOutcome.ReceiverEdiMismatch,
                callbackEntryBoundaryValidated: true,
                stopAt: UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset);
        }

        UnreliableReceiveDirectCallbackPlan directPlan =
            UnreliableReceiveDirectCallbackProjection.Project(callback);
        UnreliableReceivePresenceCallbackOutcome outcome = directPlan.Outcome switch
        {
            UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided =>
                UnreliableReceivePresenceCallbackOutcome.DirectCallbackUnreachedCaptureProvided,
            UnreliableReceiveDirectCallbackOutcome.MissingPayloadDwordAt4 =>
                UnreliableReceivePresenceCallbackOutcome.MissingPayloadDwordAt4,
            UnreliableReceiveDirectCallbackOutcome.MissingReceiverDwordAt14 =>
                UnreliableReceivePresenceCallbackOutcome.MissingReceiverDwordAt14,
            UnreliableReceiveDirectCallbackOutcome.MissingNormalReturn =>
                UnreliableReceivePresenceCallbackOutcome.MissingNormalReturn,
            UnreliableReceiveDirectCallbackOutcome.IndirectCallDidNotReturnNormally =>
                UnreliableReceivePresenceCallbackOutcome.DirectCallbackDidNotReturnNormally,
            UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally =>
                UnreliableReceivePresenceCallbackOutcome.DirectCallbackReturnedNormally,
            UnreliableReceiveDirectCallbackOutcome.EntryBoundaryNotValidated =>
                UnreliableReceivePresenceCallbackOutcome.CallbackEntryBoundaryNotValidated,
            _ => throw new ArgumentOutOfRangeException(nameof(directPlan))
        };
        bool complete =
            directPlan.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally;

        return Stop(
            presencePlan,
            outcome,
            directCallbackPlan: directPlan,
            callbackEntryBoundaryValidated: true,
            sameReceiverEdi: true,
            compositionComplete: complete,
            selectedDestinationRawFileOffset: complete
                ? directPlan.SelectedDestinationRawFileOffset
                : null,
            completeRawWindowBytes: complete ? CompleteRawWindowBytes : null,
            stopAt: directPlan.StopAtRawFileOffsetExclusive);
    }

    private static UnreliableReceivePresenceCallbackPlan Stop(
        UnreliableReceiveCallbackPresencePlan presencePlan,
        UnreliableReceivePresenceCallbackOutcome outcome,
        UnreliableReceiveDirectCallbackPlan? directCallbackPlan = null,
        bool unreachedCallbackCaptureProvided = false,
        bool callbackEntryBoundaryValidated = false,
        bool sameReceiverEdi = false,
        bool compositionComplete = false,
        uint? selectedDestinationRawFileOffset = null,
        string? completeRawWindowBytes = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            presencePlan,
            directCallbackPlan,
            outcome,
            unreachedCallbackCaptureProvided,
            callbackEntryBoundaryValidated,
            sameReceiverEdi,
            compositionComplete,
            selectedDestinationRawFileOffset,
            completeRawWindowBytes,
            stopAt);
}
