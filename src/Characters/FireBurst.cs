namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FireBurst, metadata ID 118.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct FireBurst(ulong Target, uint Value)
{
    public const ushort MessageId = 118;
    public const int ValueOffset = 0x28;
}
