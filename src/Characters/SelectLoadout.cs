namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SelectLoadout, metadata ID 150.
/// The DWORD and byte are retained positionally.
/// </summary>
public readonly record struct SelectLoadout(ulong Target, uint Value, byte Flag)
{
    public const ushort MessageId = 150;
    public const int ValueOffset = 0x28;
    public const int FlagOffset = 0x2C;
}
