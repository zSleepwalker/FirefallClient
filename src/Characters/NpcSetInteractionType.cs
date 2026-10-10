namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::NPCSetInteractionType, metadata ID 141.
/// The vector entries are eight-byte positional values; their meaning is unknown.
/// </summary>
public sealed class NpcSetInteractionType
{
    public const ushort MessageId = 141;
    public const int Value28Offset = 0x28;
    public const int EntriesOffset = 0x30;
    public const int InlineStorageOffset = 0x40;

    public NpcSetInteractionType(ulong target, byte value28, IEnumerable<ulong> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        Value28 = value28;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ulong Target { get; }
    public byte Value28 { get; }
    public IReadOnlyList<ulong> Entries { get; }
}
