using System.Collections.ObjectModel;

namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::UiQueryResponse, metadata ID 59.
/// The copied map stores opaque DWORD keys and values.
/// </summary>
public sealed class UiQueryResponse
{
    public const ushort MessageId = 59;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
    public const int ValuesOffset = 0x34;

    public UiQueryResponse(
        ulong target,
        uint value28,
        uint value2C,
        uint value30,
        IEnumerable<KeyValuePair<uint, uint>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;

        var copiedValues = new SortedDictionary<uint, uint>();
        foreach (var (key, value) in values)
            copiedValues.Add(key, value);
        Values = new ReadOnlyDictionary<uint, uint>(copiedValues);
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint Value30 { get; }
    public IReadOnlyDictionary<uint, uint> Values { get; }
}
