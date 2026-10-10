namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetNoSpreadFlag, metadata ID 139.
/// Its two payload bytes are preserved positionally.
/// </summary>
public readonly record struct SetNoSpreadFlag(ulong Target, byte Value28, byte Value29)
{
    public const ushort MessageId = 139;
    public const int Value28Offset = 0x28;
    public const int Value29Offset = 0x29;
}
