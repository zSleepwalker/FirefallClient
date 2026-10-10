namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FillBuyOrder, metadata ID 85.
/// Positional fields are preserved without inferring marketplace semantics.
/// </summary>
public sealed class FillBuyOrder
{
    public const ushort MessageId = 85;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int Value34Offset = 0x34;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
    public const int Value40Offset = 0x40;
    public const int Value44Offset = 0x44;

    public FillBuyOrder(
        ulong target,
        uint value28,
        uint value2C,
        uint value30,
        uint value34,
        uint value38,
        uint value3C,
        uint value40,
        uint value44)
    {
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;
        Value34 = value34;
        Value38 = value38;
        Value3C = value3C;
        Value40 = value40;
        Value44 = value44;
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint Value30 { get; }
    public uint Value34 { get; }
    public uint Value38 { get; }
    public uint Value3C { get; }
    public uint Value40 { get; }
    public uint Value44 { get; }
}
