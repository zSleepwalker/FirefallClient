namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LeaveEncounterParty, metadata ID 212.
/// The two subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct LeaveEncounterParty(ulong Target, uint Value28, uint Value2C)
{
    public const ushort MessageId = 212;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
