namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MatchQueue, metadata ID 90.
/// Vector contents and scalar values remain opaque/positional.
/// </summary>
public sealed class MatchQueue
{
    public const ushort MessageId = 90;
    public const int TargetOffset = 0x18;
    public const int FirstVectorOffset = 0x28;
    public const int FirstVectorElementSize = 8;
    public const int UnwrittenGapOffset = 0x34;
    public const int UnwrittenGapSize = 4;
    public const int Value38Offset = 0x38;
    public const int Value40Offset = 0x40;
    public const int Value44Offset = 0x44;
    public const int SecondVectorOffset = 0x48;
    public const int SecondVectorElementSize = 4;
    public const int Value54Offset = 0x54;
    public const int Value58Offset = 0x58;
    public const int Value5COffset = 0x5C;
    public const int Value60Offset = 0x60;
    public const int Value64Offset = 0x64;
    public const int SharedText68Offset = 0x68;
    public const int Value6COffset = 0x6C;
    public const int SharedText70Offset = 0x70;

    public MatchQueue(
        ulong target,
        IEnumerable<ulong> firstVector,
        ulong value38,
        uint value40,
        uint value44,
        IEnumerable<uint> secondVector,
        uint value54,
        uint value58,
        uint value5C,
        uint value60,
        uint value64,
        uint sharedText68,
        byte value6C,
        uint sharedText70)
    {
        ArgumentNullException.ThrowIfNull(firstVector);
        ArgumentNullException.ThrowIfNull(secondVector);
        Target = target;
        FirstVector = Array.AsReadOnly(firstVector.ToArray());
        Value38 = value38;
        Value40 = value40;
        Value44 = value44;
        SecondVector = Array.AsReadOnly(secondVector.ToArray());
        Value54 = value54;
        Value58 = value58;
        Value5C = value5C;
        Value60 = value60;
        Value64 = value64;
        SharedText68 = sharedText68;
        Value6C = value6C;
        SharedText70 = sharedText70;
    }

    public ulong Target { get; }
    public IReadOnlyList<ulong> FirstVector { get; }
    public ulong Value38 { get; }
    public uint Value40 { get; }
    public uint Value44 { get; }
    public IReadOnlyList<uint> SecondVector { get; }
    public uint Value54 { get; }
    public uint Value58 { get; }
    public uint Value5C { get; }
    public uint Value60 { get; }
    public uint Value64 { get; }
    public uint SharedText68 { get; }
    public byte Value6C { get; }
    public uint SharedText70 { get; }
}
