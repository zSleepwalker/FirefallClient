namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ListItemForSale, metadata ID 83.
/// The positional names intentionally make no claims about commerce semantics.
/// </summary>
public sealed class ListItemForSale
{
    public const ushort MessageId = 83;
    public const int TargetOffset = 0x18;
    public const int Value28AOffset = 0x28;
    public const int Value28BOffset = 0x2C;
    public const int Value30AOffset = 0x30;
    public const int Value30BOffset = 0x34;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
    public const int SharedText40Offset = 0x40;
    public const int Value48Offset = 0x48;
    public const int Value4COffset = 0x4C;
    public const int SharedText50Offset = 0x50;
    public const int Flag54Offset = 0x54;

    public ListItemForSale(
        ulong target,
        uint value28A,
        uint value28B,
        uint value30A,
        uint value30B,
        uint value38,
        uint value3C,
        uint sharedText40,
        uint value48,
        uint value4C,
        uint sharedText50,
        byte flag54)
    {
        Target = target;
        Value28A = value28A;
        Value28B = value28B;
        Value30A = value30A;
        Value30B = value30B;
        Value38 = value38;
        Value3C = value3C;
        SharedText40 = sharedText40;
        Value48 = value48;
        Value4C = value4C;
        SharedText50 = sharedText50;
        Flag54 = flag54;
    }

    public ulong Target { get; }
    public uint Value28A { get; }
    public uint Value28B { get; }
    public uint Value30A { get; }
    public uint Value30B { get; }
    public uint Value38 { get; }
    public uint Value3C { get; }
    public uint SharedText40 { get; }
    public uint Value48 { get; }
    public uint Value4C { get; }
    public uint SharedText50 { get; }
    public byte Flag54 { get; }
}
