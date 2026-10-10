namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LootDistributionSetState, metadata ID 218.
/// The byte field is retained without assigning it a domain meaning.
/// </summary>
public readonly record struct LootDistributionSetState(
    ulong Target,
    uint Value28,
    byte Value2C)
{
    public const ushort MessageId = 218;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
