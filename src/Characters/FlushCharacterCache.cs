namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FlushCharacterCache, metadata ID 231.
/// The five subclass bytes remain positional because their meanings are unknown.
/// </summary>
public readonly record struct FlushCharacterCache(
    ulong Target,
    byte Value28,
    byte Value29,
    byte Value2A,
    byte Value2B,
    byte Value2C)
{
    public const ushort MessageId = 231;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value29Offset = 0x29;
    public const int Value2AOffset = 0x2A;
    public const int Value2BOffset = 0x2B;
    public const int Value2COffset = 0x2C;
}
