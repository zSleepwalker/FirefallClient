namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::TrackBounty, metadata ID 165.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct TrackBounty(ulong Target, uint Value)
{
    public const ushort MessageId = 165;
    public const int ValueOffset = 0x28;
}
