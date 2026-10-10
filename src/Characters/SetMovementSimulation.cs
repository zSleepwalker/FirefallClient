namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetMovementSimulation, metadata ID 197.
/// The constructor stores a single u16 whose exact meaning is unknown.
/// </summary>
public readonly record struct SetMovementSimulation(ulong Target, ushort Value)
{
    public const ushort MessageId = 197;
    public const int ValueOffset = 0x28;
}
