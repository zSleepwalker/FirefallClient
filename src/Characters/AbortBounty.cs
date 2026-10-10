namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AbortBounty, metadata ID 156.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct AbortBounty(ulong Target, uint Value)
{
    public const ushort MessageId = 156;
    public const int ValueOffset = 0x28;
}
