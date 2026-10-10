namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::TargetAbility, metadata ID 136.
/// The byte field's gameplay meaning is unknown.
/// </summary>
public readonly record struct TargetAbility(ulong Target, byte Value)
{
    public const ushort MessageId = 136;
    public const int ValueOffset = 0x28;
}
