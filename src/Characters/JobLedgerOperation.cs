namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::JobLedgerOperation, metadata ID 215.
/// Shared text and vector elements are retained as opaque native values.
/// </summary>
public sealed class JobLedgerOperation
{
    public const ushort MessageId = 215;
    public const int TargetOffset = 0x18;
    public const int SharedTextReferenceOffset = 0x28;
    public const int EntriesOffset = 0x2C;
    public const int EntrySize = 4;

    public JobLedgerOperation(
        ulong target,
        uint sharedTextReference,
        IEnumerable<uint> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        SharedTextReference = sharedTextReference;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ulong Target { get; }
    public uint SharedTextReference { get; }
    public IReadOnlyList<uint> Entries { get; }
}
