namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClearPreviousBounties, metadata ID 162.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct ClearPreviousBounties(ulong Target, uint Value)
{
    public const ushort MessageId = 162;
    public const int ValueOffset = 0x28;
}
