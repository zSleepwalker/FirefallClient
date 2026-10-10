namespace Firefall.Characters;

/// <summary>
/// An eight-byte pair copied in each of DuelRequest's two native vectors.
/// The business meaning of either DWORD is unknown.
/// </summary>
public readonly record struct DuelRequestPair(uint First, uint Second);

/// <summary>
/// Opaque subclass descriptor copied by DuelRequest constructors. Offsets in
/// the XML documentation are relative to the descriptor (native object +0x28).
/// Shared-text values are supplied as opaque bytes; their native encoding is
/// not established. Vector element meaning remains unknown.
/// </summary>
public sealed class DuelRequestDescriptor
{
    public DuelRequestDescriptor(
        byte leadingByte,
        ReadOnlyMemory<byte> firstText,
        uint value08,
        uint value0C,
        ReadOnlyMemory<byte> secondText,
        uint value18,
        uint value1C,
        ReadOnlyMemory<byte> thirdText,
        IEnumerable<DuelRequestPair> firstPairs,
        IEnumerable<DuelRequestPair> secondPairs,
        ushort value40,
        ushort value42,
        uint value44,
        byte value48,
        uint value50,
        uint value54)
    {
        ArgumentNullException.ThrowIfNull(firstPairs);
        ArgumentNullException.ThrowIfNull(secondPairs);
        LeadingByte = leadingByte;
        FirstText = firstText.ToArray();
        Value08 = value08;
        Value0C = value0C;
        SecondText = secondText.ToArray();
        Value18 = value18;
        Value1C = value1C;
        ThirdText = thirdText.ToArray();
        FirstPairs = Array.AsReadOnly(firstPairs.ToArray());
        SecondPairs = Array.AsReadOnly(secondPairs.ToArray());
        Value40 = value40;
        Value42 = value42;
        Value44 = value44;
        Value48 = value48;
        Value50 = value50;
        Value54 = value54;
    }

    public byte LeadingByte { get; }
    public ReadOnlyMemory<byte> FirstText { get; }
    public uint Value08 { get; }
    public uint Value0C { get; }
    public ReadOnlyMemory<byte> SecondText { get; }
    public uint Value18 { get; }
    public uint Value1C { get; }
    public ReadOnlyMemory<byte> ThirdText { get; }
    public IReadOnlyList<DuelRequestPair> FirstPairs { get; }
    public IReadOnlyList<DuelRequestPair> SecondPairs { get; }
    public ushort Value40 { get; }
    public ushort Value42 { get; }
    public uint Value44 { get; }
    public byte Value48 { get; }
    public uint Value50 { get; }
    public uint Value54 { get; }
}

/// <summary>
/// Native Character::Commands::DuelRequest, metadata ID 188. Populated
/// constructors deep-copy an 0x58-byte descriptor at +0x28 and store a
/// separate mode byte at +0x80. This is an object-layout model only.
/// </summary>
public sealed class DuelRequest
{
    public const ushort MessageId = 188;
    public const int DescriptorOffset = 0x28;
    public const int ModeOffset = 0x80;
    public const int DescriptorSize = 0x58;

    public DuelRequest(ulong target, DuelRequestDescriptor descriptor, byte mode)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        Target = target;
        Descriptor = descriptor;
        Mode = mode;
    }

    public ulong Target { get; }
    public DuelRequestDescriptor Descriptor { get; }
    public byte Mode { get; }
}
