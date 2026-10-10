namespace Firefall.Reconstruction.Startup;

public static class MatrixLfgNativeLayout
{
    public const uint MatchFoundHandlerFileOffset = 0x492400;
    public const uint MatchFoundConsumerFileOffset = 0x6F4A50;
    public const uint LeaderChangeHandlerFileOffset = 0x4924D0;
    public const uint LeaderChangeConsumerFileOffset = 0x6F4C70;
    public const int ModeMatchPresentByteOffset = 0x48;
    public const int ModeMatchIdOffset = 0x50;
    public const int ModeLeaderTextOffset = 0x5C;
    public const int ModeLeaderPairOffset = 0x120;
    public const uint InactiveMatchPairVirtualAddress = 0x1E68530;

    public static bool SelectMatchFound(bool modePresent, bool matchPresent, ulong storedMatchId, ulong inactivePair)
    {
        ulong selected = matchPresent ? storedMatchId : inactivePair;
        return modePresent && (selected == 0 || (selected & 0xFFFFFFFFFFFFFF00) == 0xFFFFFFFFFFFFFF00);
    }

    public static bool SelectLeaderChange(bool modePresent, bool matchPresent,
        ulong storedMatchId, ulong inactivePair, ulong incomingMatchId) =>
        modePresent && (matchPresent ? storedMatchId : inactivePair) == incomingMatchId;
}
