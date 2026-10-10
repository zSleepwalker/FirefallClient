namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FireCancel, metadata ID 120.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct FireCancel(ulong Target, uint Value)
{
    public const ushort MessageId = 120;
    public const int ValueOffset = 0x28;
}
