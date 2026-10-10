namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ReportProjectileHit, metadata ID 122.
/// Unaligned positional fields are preserved without assigning semantics.
/// </summary>
public readonly record struct ReportProjectileHit(
    ulong Target,
    ushort Value28,
    ushort Value2A,
    byte Value2C,
    ushort Value2D,
    byte Value2F,
    ushort Value30,
    ushort Value32)
{
    public const ushort MessageId = 122;
    public const int Value28Offset = 0x28;
    public const int Value2AOffset = 0x2A;
    public const int Value2COffset = 0x2C;
    public const int Value2DOffset = 0x2D;
    public const int Value2FOffset = 0x2F;
    public const int Value30Offset = 0x30;
    public const int Value32Offset = 0x32;
}
