namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SlotGearRequest, metadata ID 206.
/// Fields remain positional; the 4-byte gap at +0x2C is not initialized by
/// either observed constructor.
/// </summary>
public readonly record struct SlotGearRequest(
    ulong Target,
    uint Value28,
    ulong Value30,
    byte Value38,
    uint Value3C)
{
    public const ushort MessageId = 206;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int UnwrittenGapOffset = 0x2C;
    public const int UnwrittenGapSize = 4;
    public const int Value30Offset = 0x30;
    public const int Value30Size = 8;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
}
