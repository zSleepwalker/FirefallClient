namespace Firefall.Reconstruction.Startup;

public sealed record MatrixServerMessageNativeType(
    uint MessageId, string ClassName, uint FactoryCaseFileOffset, uint ConstructorFileOffset,
    uint VtableStoreFileOffset, uint VtableVirtualAddress, uint CompleteObjectLocatorVirtualAddress,
    uint TypeDescriptorVirtualAddress, uint RttiNameFileOffset)
{
    public string QualifiedName => $"matrix_fury::{ClassName}";
    public string RttiText => $".?AV{ClassName}@matrix_fury@@";
}

/// <summary>Original factory/constructor/RTTI identities. Names do not establish scalar field semantics.</summary>
public static class MatrixServerMessageNativeTypes
{
    public const uint FactoryFileOffset = 0x33D4B0;
    public const uint FactoryJumpTableFileOffset = 0x33E250;

    public static IReadOnlyList<MatrixServerMessageNativeType> Entries { get; } = Array.AsReadOnly(
        new MatrixServerMessageNativeType[]
        {
            new(34, "SigscanData", 0x33D4F4, 0x339950, 0x339977, 0x1C62354, 0x1C971C8, 0x1E04DE0, 0x1A031E8),
            new(35, "WelcomeToTheMatrix", 0x33D557, 0x339AB0, 0x339AD7, 0x1C62344, 0x1C971A0, 0x1E04D74, 0x1A0317C),
            new(36, "Announce", 0x33D5BA, 0x339C80, 0x339CA7, 0x1C62334, 0x1C97178, 0x1E04D0C, 0x1A03114),
            new(37, "EnterZone", 0x33D629, 0x339EA0, 0x339EC7, 0x1C62324, 0x1C97150, 0x1E04CB0, 0x1A030B8),
            new(38, "UpdateZoneTimeSync", 0x33D68F, 0x33A340, 0x33A367, 0x1C62314, 0x1C97128, 0x1E04C44, 0x1A0304C),
            new(39, "HotfixLevelChanged", 0x33D6F2, 0x33A420, 0x33A447, 0x1C62304, 0x1C97100, 0x1E04BCC, 0x1A02FD4),
            new(40, "ExitZone", 0x33D757, 0x33A4E0, 0x33A507, 0x1C622F4, 0x1C970D8, 0x1E04B60, 0x1A02F68),
            new(41, "MatrixStatus", 0x33D7A6, 0x33A560, 0x33A587, 0x1C622E4, 0x1C970B0, 0x1E04B00, 0x1A02F08),
            new(42, "MatchQueueResponse", 0x33D809, 0x33A7E0, 0x33A807, 0x1C622D4, 0x1C97088, 0x1E04A94, 0x1A02E9C),
            new(43, "MatchQueueUpdate", 0x33D86C, 0x33A930, 0x33A959, 0x1C622C4, 0x1C97060, 0x1E04A20, 0x1A02E28),
            new(44, "FoundMatchUpdate", 0x33D8CF, 0x33AA10, 0x33AA37, 0x1C622B4, 0x1C97038, 0x1E049B0, 0x1A02DB8),
            new(45, "ChallengeJoinResponse", 0x33D932, 0x33AB30, 0x33AB57, 0x1C622A4, 0x1C97010, 0x1E04940, 0x1A02D48),
            new(46, "ChallengeInvitation", 0x33D995, 0x33ACD0, 0x33ACF7, 0x1C62294, 0x1C96FE8, 0x1E048C4, 0x1A02CCC),
            new(47, "ChallengeInvitationSquadInfoAck", 0x33D9F8, 0x33AEB0, 0x33AED7, 0x1C62284, 0x1C96FC0, 0x1E04840, 0x1A02C48),
            new(48, "ChallengeInvitationCancel", 0x33DA5B, 0x33B090, 0x33B0B7, 0x1C62274, 0x1C96F98, 0x1E047BC, 0x1A02BC4),
            new(49, "ChallengeInvitationResponse", 0x33DAC1, 0x33B170, 0x33B197, 0x1C62264, 0x1C96F70, 0x1E04734, 0x1A02B3C),
            new(50, "ChallengeKicked", 0x33DB24, 0x33B330, 0x33B357, 0x1C62254, 0x1C96F48, 0x1E046BC, 0x1A02AC4),
            new(51, "ChallengeLeave", 0x33DB87, 0x33B590, 0x33B5B7, 0x1C62244, 0x1C96F20, 0x1E04650, 0x1A02A58),
            new(52, "ChallengeRosterUpdate", 0x33DBCA, 0x33B670, 0x33B697, 0x1C62234, 0x1C96EF8, 0x1E045E0, 0x1A029E8),
            new(53, "ChallengeReadyCheck", 0x33DC2D, 0x33B820, 0x33B847, 0x1C62224, 0x1C96ED0, 0x1E04564, 0x1A0296C),
            new(54, "ChallengeMatchParametersUpdate", 0x33DC70, 0x33B900, 0x33B927, 0x1C62214, 0x1C96EA8, 0x1E044E0, 0x1A028E8),
            new(55, "ChallengeMatchStarting", 0x33DCD3, 0x33B9E0, 0x33BA07, 0x1C62204, 0x1C96E80, 0x1E04458, 0x1A02860),
            new(56, "ForceUnqueue", 0x33DD16, 0x33BAC0, 0x33BAE7, 0x1C621F4, 0x1C96E58, 0x1E043E8, 0x1A027F0),
            new(57, "SynchronizationRequest", 0x33DD79, 0x33BC40, 0x33BC67, 0x1C621E4, 0x1C96E30, 0x1E04378, 0x1A02780),
            new(58, "GamePaused", 0x33DDDC, 0x33BD90, 0x33BDBC, 0x1C621D4, 0x1C96E08, 0x1E04308, 0x1A02710),
            new(59, "SuperPong", 0x33DE40, 0x33BE80, 0x33BEA7, 0x1C621C4, 0x1C96DE0, 0x1E042A8, 0x1A026B0),
            new(60, "ServerProfiler_SendNames", 0x33DEA3, 0x33BFE0, 0x33C007, 0x1C621B4, 0x1C96DB8, 0x1E04238, 0x1A02640),
            new(61, "ServerProfiler_SendFrame", 0x33DF12, 0x33C180, 0x33C1A7, 0x1C621A4, 0x1C96D90, 0x1E041B8, 0x1A025C0),
            new(62, "ZoneQueueUpdate", 0x33DF77, 0x33C310, 0x33C337, 0x1C62194, 0x1C96D68, 0x1E04140, 0x1A02548),
            new(63, "DebugLagSampleSim", 0x33DFDA, 0x33C3F0, 0x33C419, 0x1C62184, 0x1C96D40, 0x1E040D4, 0x1A024DC),
            new(64, "DebugLagSampleClient", 0x33E03D, 0x33C4C0, 0x33C4E9, 0x1C62174, 0x1C96D18, 0x1E0405C, 0x1A02464),
            new(65, "LFGMatchFound", 0x33E0A0, 0x33C5C0, 0x33C5E7, 0x1C62164, 0x1C96CF0, 0x1E03FF0, 0x1A023F8),
            new(66, "LFGLeaderChange", 0x33E103, 0x33C810, 0x33C837, 0x1C62154, 0x1C96CC8, 0x1E03F84, 0x1A0238C),
            new(67, "ReceiveEmergencyChat", 0x33E166, 0x33C9F0, 0x33CA17, 0x1C62144, 0x1C96CA0, 0x1E03F14, 0x1A0231C),
            new(68, "UpdateDevZoneInfo", 0x33E1C9, 0x33CC40, 0x33CC94, 0x1C62134, 0x1C96C78, 0x1E03EA4, 0x1A022AC)
        });

    public static MatrixServerMessageNativeType Get(uint decodedMessageId)
    {
        if (decodedMessageId is < MatrixServerMessageCatalog.FirstMessageId or > MatrixServerMessageCatalog.LastMessageId)
            throw new ArgumentOutOfRangeException(nameof(decodedMessageId));
        return Entries[(int)(decodedMessageId - MatrixServerMessageCatalog.FirstMessageId)];
    }
}
