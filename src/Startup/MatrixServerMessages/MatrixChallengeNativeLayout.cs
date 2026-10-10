namespace Firefall.Reconstruction.Startup;

public enum MatrixChallengeJoinSelection
{
    IgnoreUnexpectedResponse,
    ReleaseActiveChallenge,
    ApplyJoinedChallenge
}

public readonly record struct MatrixChallengeJoinNotificationSelection(
    MatrixChallengeJoinSelection Selection, bool EmitRuntime269, string? Runtime270Reason, bool EmitRuntime277);

/// <summary>Confirmed routing and join-response gates; this does not execute invitation/party management.</summary>
public static class MatrixChallengeNativeLayout
{
    public const uint JoinHandlerFileOffset = 0x491A40;
    public const uint JoinConsumerFileOffset = 0x8FD6B0;
    public const uint Invitation46HandlerFileOffset = 0x491A90;
    public const uint Invitation47HandlerFileOffset = 0x491BA0;
    public const uint InvitationConsumerFileOffset = 0x8FD960;
    public const int CurrentModeSpecificComponentOffset = 0x80;
    public const int ActiveChallengePointerOffset = 0x64;
    public const uint JoinCreatedRuntimeMessageId = 269;
    public const uint JoinedRuntimeMessageId = 270;
    public const uint JoinedFollowupRuntimeMessageId = 277;
    public const uint InvitationRuntimeMessageId = 271;
    public const string CreatedPropertyName = "successful";
    public const string JoinedReasonPropertyName = "reason";

    public static MatrixChallengeJoinNotificationSelection SelectJoinResponse(ulong? storedChallengeId,
        ulong incomingChallengeId, byte joinSucceededByte, byte createdByte, byte squadReasonByte)
    {
        bool created = createdByte != 0;
        if (storedChallengeId is not ulong id
            || (IsValidStoredId(id) ? id != incomingChallengeId : !created))
            return new(MatrixChallengeJoinSelection.IgnoreUnexpectedResponse, false, null, false);
        if (joinSucceededByte == 0)
            return new(MatrixChallengeJoinSelection.ReleaseActiveChallenge, false, null, false);
        string reason = squadReasonByte != 0 ? "Squad" : created ? "Created" : "Accepted";
        return new(MatrixChallengeJoinSelection.ApplyJoinedChallenge, created, reason, true);
    }

    public static bool SelectInvitationVariant(uint messageCode) => messageCode switch
    {
        46 => false,
        47 => true,
        _ => throw new ArgumentOutOfRangeException(nameof(messageCode), "Only server messages46/47 use this variant selection.")
    };

    internal static bool IsValidStoredId(ulong id) =>
        id != 0 && (id & 0xFFFFFFFFFFFFFF00UL) != 0xFFFFFFFFFFFFFF00UL;
}
