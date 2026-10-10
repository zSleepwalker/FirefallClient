namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SelectFireMode, metadata ID 123.
/// The positional fields' gameplay meanings are unknown.
/// </summary>
public readonly record struct SelectFireMode(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 123;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
