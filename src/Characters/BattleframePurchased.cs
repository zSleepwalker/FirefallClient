namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::BattleframePurchased, metadata ID 110.
/// The observed constructor initializes no subclass fields.
/// </summary>
public readonly record struct BattleframePurchased(ulong Target)
{
    public const ushort MessageId = 110;
    public const int TargetOffset = 0x18;
}
