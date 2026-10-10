namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FireInputIgnored, metadata ID 117.
/// Positional fields are preserved without assigning gameplay semantics.
/// </summary>
public readonly record struct FireInputIgnored(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 117;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
