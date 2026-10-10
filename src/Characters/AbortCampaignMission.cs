namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AbortCampaignMission, metadata ID 152.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct AbortCampaignMission(ulong Target, uint Value)
{
    public const ushort MessageId = 152;
    public const int ValueOffset = 0x28;
}
