namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClearBounties, metadata ID 161.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct ClearBounties(ulong Target, uint Value)
{
    public const ushort MessageId = 161;
    public const int ValueOffset = 0x28;
}
