namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestAllAchievements, metadata ID 170.
/// The observed constructor has no subclass payload fields.
/// </summary>
public readonly record struct RequestAllAchievements(ulong Target)
{
    public const ushort MessageId = 170;
}
