namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::CancelReload, metadata ID 127.
/// The sole subclass DWORD is retained positionally.
/// </summary>
public readonly record struct CancelReload(ulong Target, uint Value)
{
    public const ushort MessageId = 127;
    public const int ValueOffset = 0x28;
}
