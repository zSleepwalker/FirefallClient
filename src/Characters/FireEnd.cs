namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FireEnd, metadata ID 119.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct FireEnd(ulong Target, uint Value)
{
    public const ushort MessageId = 119;
    public const int ValueOffset = 0x28;
}
