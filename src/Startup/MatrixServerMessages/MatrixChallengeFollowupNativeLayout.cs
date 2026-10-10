namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixChallengeInvitationSnapshot(ReadOnlyMemory<byte> NameBytes, ulong ChallengeId);

public readonly record struct MatrixChallengeResponseNotification(
    uint RuntimeMessageId, ReadOnlyMemory<byte> NameBytes, bool Accepted, string Reason);

/// <summary>Snapshot selections only; no native vector mutation, callback reentrancy or object release.</summary>
public static class MatrixChallengeFollowupNativeLayout
{
    public const uint CancelHandlerFileOffset = 0x491C60;
    public const uint CancelConsumerFileOffset = 0x8FE1A0;
    public const uint ResponseHandlerFileOffset = 0x491C80;
    public const uint ResponseConsumerFileOffset = 0x8FE3E0;
    public const uint KickedHandlerFileOffset = 0x491CF0;
    public const uint KickedConsumerFileOffset = 0x8FABA0;
    public const uint ResponseReasonTableVirtualAddress = 0x1C6FCC0;
    public const uint ResponseReasonTableFileOffset = 0x186E8C0;
    public const uint CancelRuntimeMessageId = 271;
    public const uint ResponseRuntimeMessageId = 272;
    public const uint KickedRuntimeMessageId = 274;
    public const uint LocalKickedRuntimeMessageId = 275;
    public const int ActiveChallengeModeMemberOffset = MatrixChallengeNativeLayout.CurrentModeSpecificComponentOffset
        + MatrixChallengeNativeLayout.ActiveChallengePointerOffset;
    public const string ResponseAcceptedPropertyName = "accepted";
    public const string ResponseReasonPropertyName = "reason";
    public const string KickerPropertyName = "kicker";
    public const string KickeePropertyName = "kickee";

    public static IReadOnlyList<string> ResponseReasons { get; } = Array.AsReadOnly(new[]
    {
        "Accepted", "Declined", "DoesNotExist", "Offline", "TeamFull", "MatchStarting",
        "TargetNotChallengeable", "SquadTeamFull"
    });

    public static MatrixChallengeResponseNotification CaptureResponse(ReadOnlyMemory<byte> name, byte statusByte) =>
        new(ResponseRuntimeMessageId, name, statusByte == 0,
            statusByte < ResponseReasons.Count ? ResponseReasons[statusByte] : "UNKNOWN");

    public static IReadOnlyList<int> SelectCancellationMatches(
        IReadOnlyList<MatrixChallengeInvitationSnapshot> invitations, ulong challengeId)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        return Array.AsReadOnly(Enumerable.Range(0, invitations.Count)
            .Where(index => invitations[index].ChallengeId == challengeId).ToArray());
    }

    public static bool SelectKickedHandler(bool hasCurrentModeObject, ulong? activeChallengeId, ulong incomingChallengeId) =>
        hasCurrentModeObject && activeChallengeId is ulong id && id == incomingChallengeId;

    /// <summary>The native consumer explicitly uses zero when the local-client getter returns null.</summary>
    public static bool SelectLocalKickedBranch(ulong kickeeId, ulong? localClientId) =>
        kickeeId == (localClientId ?? 0);
}
