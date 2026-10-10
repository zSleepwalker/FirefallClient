namespace Firefall.Reconstruction.Startup;

public sealed record MatrixForceUnqueueNotification(string Reason, bool LocalPlayer, uint TextSourceWord)
{
    public uint RuntimeMessageId => 291;
}

public static class MatrixForceUnqueueNativeLayout
{
    public const uint HandlerFileOffset = 0x496460;
    public const uint ConsumerFileOffset = 0x85DCC0;
    public const uint ReasonTableVirtualAddress = 0x1C6FD10;
    public const int ReasonCount = 20;

    public static string GetReason(byte reason) => reason switch
    {
        0 => "Unknown",
        1 => "DidNotAccept",
        2 => "ServerError",
        3 => "InChallenge",
        4 => "NotEnoughPlayers",
        5 => "TooManyPlayers",
        6 => "InvalidTeamId",
        7 => "LackingAccess",
        8 => "TooManyPlayersPvP",
        9 => "CannotQueueFromThisInstance",
        10 => "InvalidQueues",
        11 => "NotSquadLeader",
        12 => "NotInSquad",
        13 => "SquadDoesNotExist",
        14 => "SquadReadyTimedOut",
        15 => "InvalidLFGCategory",
        16 => "TooManyQuitters",
        17 => "InvalidZoneId",
        18 => "MinimumLevelNotMet",
        19 => "DeserterDebuff",
        _ => "UNKNOWN"
    };

    public static MatrixForceUnqueueNotification CaptureNotification(
        MatrixForceUnqueuePayload payload, ulong? localClientId)
    {
        ulong local = localClientId ?? 0;
        ulong masked = local & 0xFFFFFFFFFFFFFF00;
        bool involved = masked != 0 && payload.InvolvedIds.Any(id => (id & 0xFFFFFFFFFFFFFF00) == masked);
        return new(GetReason(payload.ReasonCode), involved, payload.WordAt38);
    }
}
