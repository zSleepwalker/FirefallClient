namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ListAchievements, metadata ID 168.
/// The byte field's gameplay meaning is unknown.
/// </summary>
public readonly record struct ListAchievements(ulong Target, byte Value)
{
    public const ushort MessageId = 168;
    public const int ValueOffset = 0x28;
}
