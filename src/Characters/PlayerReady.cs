namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::PlayerReady, metadata ID 88.
/// The optional vector contains opaque 8-byte entries.
/// </summary>
public sealed class PlayerReady
{
    public const ushort MessageId = 88;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value29Offset = 0x29;
    public const int HasEntriesOffset = 0x2C;
    public const int EntriesOffset = 0x30;
    public const int EntrySize = 8;

    public PlayerReady(
        ulong target,
        byte value28,
        byte value29,
        bool hasEntries,
        IEnumerable<ulong> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        Value28 = value28;
        Value29 = value29;
        HasEntries = hasEntries;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ulong Target { get; }
    public byte Value28 { get; }
    public byte Value29 { get; }
    public bool HasEntries { get; }
    public IReadOnlyList<ulong> Entries { get; }
}
