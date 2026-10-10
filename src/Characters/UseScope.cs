namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::UseScope, metadata ID 124.
/// The positional fields' gameplay meanings are unknown.
/// </summary>
public readonly record struct UseScope(ulong Target, uint Value28, byte Value2C)
{
    public const ushort MessageId = 124;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
}
