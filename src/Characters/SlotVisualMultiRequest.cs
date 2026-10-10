namespace Firefall.Characters;

/// <summary>
/// The 28-byte elements of the inline visual list in SlotVisualMultiRequest.
/// The last 12 bytes of each native element are a vector of DWORDs.
/// </summary>
public sealed class VisualDescriptor
{
    public VisualDescriptor(uint first, byte flag, uint second, uint third, IEnumerable<uint> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        First = first;
        Flag = flag;
        Second = second;
        Third = third;
        Values = Array.AsReadOnly(values.ToArray());
    }

    public uint First { get; }
    public byte Flag { get; }
    public uint Second { get; }
    public uint Third { get; }
    public IReadOnlyList<uint> Values { get; }
}

/// <summary>
/// The 28-byte elements of the second vector. Both text fields are native
/// shared-text references serialized as NUL-terminated byte strings.
/// The string encoding and the field meanings are not established.
/// </summary>
public sealed class NamedVisualDescriptor
{
    public NamedVisualDescriptor(
        uint first, ReadOnlyMemory<byte> firstText, uint second, uint third,
        ReadOnlyMemory<byte> secondText, uint fourth, uint fifth)
    {
        if (firstText.Span.Contains((byte)0) || secondText.Span.Contains((byte)0))
            throw new ArgumentException("Native NUL-terminated text cannot contain an embedded NUL.");
        First = first;
        FirstText = firstText.ToArray();
        Second = second;
        Third = third;
        SecondText = secondText.ToArray();
        Fourth = fourth;
        Fifth = fifth;
    }

    public uint First { get; }
    public ReadOnlyMemory<byte> FirstText { get; }
    public uint Second { get; }
    public uint Third { get; }
    public ReadOnlyMemory<byte> SecondText { get; }
    public uint Fourth { get; }
    public uint Fifth { get; }
}

/// <summary>
/// Native command 208. Constructors at 0x53A620/0x53A760 initialize an
/// inline-capacity visual list (+0x30; 16 slots of 28 bytes) followed by a
/// heap vector of named descriptors (+0x20C). Only the payload serializer
/// at 0x5B8340 is reconstructed; transport framing is not claimed.
/// </summary>
public sealed class SlotVisualMultiRequest
{
    public const ushort MessageId = 208;
    public const int SelectorOffset = 0x28;
    public const int SelectorKindOffset = 0x2C;
    public const int VisualsOffset = 0x30;
    public const int NamedVisualsOffset = 0x20C;
    public const int NativeSize = 0x218;
    public const int InlineVisualCapacity = 16;

    public SlotVisualMultiRequest(
        ulong target, uint selector, uint selectorKind,
        IEnumerable<VisualDescriptor> visuals, IEnumerable<NamedVisualDescriptor> namedVisuals)
    {
        ArgumentNullException.ThrowIfNull(visuals);
        ArgumentNullException.ThrowIfNull(namedVisuals);
        Target = target;
        Selector = selector;
        SelectorKind = selectorKind;
        Visuals = Array.AsReadOnly(visuals.ToArray());
        NamedVisuals = Array.AsReadOnly(namedVisuals.ToArray());
        if (Visuals.Any(x => x is null) || NamedVisuals.Any(x => x is null))
            throw new ArgumentException("Descriptor sequences cannot contain null entries.");
    }

    public ulong Target { get; }
    public uint Selector { get; }
    public uint SelectorKind { get; }
    public IReadOnlyList<VisualDescriptor> Visuals { get; }
    public IReadOnlyList<NamedVisualDescriptor> NamedVisuals { get; }

    /// <summary>Write just the virtual serializer's payload (raw 0x5B8340).</summary>
    public void WritePayload(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.Write(Selector);
        writer.Write(SelectorKind);
        CharacterCommandWire.WriteChunkedSequence(writer, Visuals, static (w, item) =>
        {
            w.Write(item.First);
            w.Write(item.Flag);
            w.Write(item.Second);
            w.Write(item.Third);
            CharacterCommandWire.WriteChunkedSequence(w, item.Values, static (nested, value) => nested.Write(value));
        });
        CharacterCommandWire.WriteChunkedSequence(writer, NamedVisuals, static (w, item) =>
        {
            w.Write(item.First);
            w.Write(item.FirstText.Span);
            w.Write((byte)0);
            w.Write(item.Second);
            w.Write(item.Third);
            w.Write(item.SecondText.Span);
            w.Write((byte)0);
            w.Write(item.Fourth);
            w.Write(item.Fifth);
        });
    }
}
