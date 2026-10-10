namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ListPreviousBounties, metadata ID 163.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct ListPreviousBounties(ulong Target, uint Value)
{
    public const ushort MessageId = 163;
    public const int ValueOffset = 0x28;
}
