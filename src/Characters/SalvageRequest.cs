namespace Firefall.Characters;

/// <summary>
/// One 16-byte record copied by the native SalvageRequest constructor at
/// 0x539B60 and written by 0x510FA0. The business meaning is unknown.
/// </summary>
public readonly record struct SalvageRecord(ulong Identifier, uint First, uint Second);

/// <summary>
/// Native Character::Commands::SalvageRequest, ID 202. The sender at
/// 0x4D5EA0 accepts between 1 and 200 records; the vector at +0x28 is a
/// deep copy of the caller's 16-byte records.
/// </summary>
public sealed class SalvageRequest
{
    public const ushort MessageId = 202;
    public const int RecordsOffset = 0x28;
    public const int MaxCallerRecords = 200;

    public SalvageRequest(ulong target, IEnumerable<SalvageRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        Target = target;
        Records = Array.AsReadOnly(records.ToArray());
        if (Records.Count == 0 || Records.Count > MaxCallerRecords)
            throw new ArgumentOutOfRangeException(nameof(records), "The traced sender accepts 1..200 records.");
    }

    public ulong Target { get; }
    public IReadOnlyList<SalvageRecord> Records { get; }

    /// <summary>Native vector serializer 0x510FA0; no target or message ID.</summary>
    public void WritePayload(BinaryWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        CharacterCommandWire.WriteChunkedSequence(writer, Records, static (w, record) =>
        {
            w.Write(record.Identifier);
            w.Write(record.First);
            w.Write(record.Second);
        });
    }

    /// <summary>Body order proven at 0x508CA0: target, MessageID, payload. Transport framing is excluded.</summary>
    public byte[] EncodeBody()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(Target);
        CharacterCommandWire.WriteMessageId(writer, MessageId);
        WritePayload(writer);
        return stream.ToArray();
    }
}
