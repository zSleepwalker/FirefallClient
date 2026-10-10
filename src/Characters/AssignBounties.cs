namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AssignBounties, metadata ID 155.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct AssignBounties(ulong Target, uint Value)
{
    public const ushort MessageId = 155;
    public const int ValueOffset = 0x28;
}
