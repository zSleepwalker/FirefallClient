namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::LootDistributionSetVotes, metadata ID 219.
/// Vector entries are copied opaque 64-bit values.
/// </summary>
public sealed class LootDistributionSetVotes
{
    public const ushort MessageId = 219;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int EntriesOffset = 0x2C;
    public const int EntrySize = 8;

    public LootDistributionSetVotes(ulong target, uint value28, IEnumerable<ulong> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        Value28 = value28;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public IReadOnlyList<ulong> Entries { get; }
}
