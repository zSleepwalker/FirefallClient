namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FindNearbyResourceAreas, metadata ID 183.
/// The observed constructor has no subclass payload fields.
/// </summary>
public readonly record struct FindNearbyResourceAreas(ulong Target)
{
    public const ushort MessageId = 183;
}
