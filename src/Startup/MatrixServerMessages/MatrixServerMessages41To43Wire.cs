namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer41To43Payload;

public readonly record struct MatrixMessage41Entry(
    ulong QwordAt0, byte ByteAt8, byte ByteAt9, byte ByteAtA, byte ByteAtB);

public sealed record MatrixMessage41Payload(
    uint WordAt28, uint WordAt2C, byte ByteAt30, byte ByteAt31, ushort WordAt32, byte ByteAt34,
    IReadOnlyList<MatrixMessage41Entry>? OptionalEntries, ReadOnlyMemory<byte> BlobAt48) : MatrixServer41To43Payload;

public readonly record struct MatrixPvpQueueRecord(
    uint WordAt0, uint WordAt4, ReadOnlyMemory<byte> TextBytesAt8, ReadOnlyMemory<byte> TextBytesAtC);

public sealed record MatrixPvpQueuePayload(
    IReadOnlyList<MatrixPvpQueueRecord> Records, uint WordAt34) : MatrixServer41To43Payload;

public sealed record MatrixMessage43Payload(uint WordAt28, ushort WordAt2C) : MatrixServer41To43Payload;

public enum MatrixServer41To43ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer41To43ReadResult(
    MatrixServer41To43ReadOutcome Outcome, MatrixServer41To43Payload? Payload, int BytesConsumed);

/// <summary>Normal-buffer wire bodies; text encodings and unnamed scalar semantics are unproven.</summary>
public static class MatrixServerMessages41To43Wire
{
    public const uint Message41BodyReaderFileOffset = 0x3543F0;
    public const uint Message41OptionalReaderFileOffset = 0x354140;
    public const uint Message41EntryReaderFileOffset = 0x3537D0;
    public const uint Message42BodyReaderFileOffset = 0x3532D0;
    public const uint Message42RecordReaderFileOffset = 0x352EC0;
    public const uint Message43BodyReaderFileOffset = 0x352290;
    public const int Message41MinimumPacketSize = 17;
    public const int Message42MinimumPacketSize = 6;
    public const int Message43PacketSize = 7;

    public static MatrixServer41To43ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer41To43ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer41To43ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not (41 or 42 or 43))
            return new(MatrixServer41To43ReadOutcome.UnexpectedMessageCode, null, 0);

        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixServer41To43Payload payload;
        if (header.Value == 41)
        {
            if (!reader.TryReadUInt32(out var word28) || !reader.TryReadUInt32(out var word2C)
                || !reader.TryReadByte(out var byte30) || !reader.TryReadByte(out var byte31)
                || !reader.TryReadUInt16(out var word32) || !reader.TryReadByte(out var byte34)
                || !reader.TryReadByte(out var present))
                return Truncated();
            IReadOnlyList<MatrixMessage41Entry>? entries = null;
            if (present != 0)
            {
                if (!MatrixChunkedListWire.TryRead<MatrixMessage41Entry>(ref reader, ReadMessage41Entry, out var list))
                    return Truncated();
                entries = list;
            }
            if (!reader.TryReadWordLengthBlob(out var blob))
                return Truncated();
            payload = new MatrixMessage41Payload(word28, word2C, byte30, byte31, word32, byte34, entries, blob);
        }
        else if (header.Value == 42)
        {
            if (!MatrixChunkedListWire.TryRead<MatrixPvpQueueRecord>(ref reader, ReadPvpQueueRecord, out var records)
                || !reader.TryReadUInt32(out var word34))
                return Truncated();
            payload = new MatrixPvpQueuePayload(records, word34);
        }
        else
        {
            if (!reader.TryReadUInt32(out var word28) || !reader.TryReadUInt16(out var word2C))
                return Truncated();
            payload = new MatrixMessage43Payload(word28, word2C);
        }
        return new(MatrixServer41To43ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer41To43ReadResult Truncated() =>
        new(MatrixServer41To43ReadOutcome.TruncatedPayload, null, 0);

    private static bool ReadMessage41Entry(ref MatrixMessageSpanReader reader, out MatrixMessage41Entry entry)
    {
        entry = default;
        if (!reader.TryReadUInt64(out var qword) || !reader.TryReadByte(out var b8)
            || !reader.TryReadByte(out var b9) || !reader.TryReadByte(out var bA) || !reader.TryReadByte(out var bB))
            return false;
        entry = new(qword, b8, b9, bA, bB);
        return true;
    }

    private static bool ReadPvpQueueRecord(ref MatrixMessageSpanReader reader, out MatrixPvpQueueRecord entry)
    {
        entry = default;
        if (!reader.TryReadUInt32(out var word0) || !reader.TryReadUInt32(out var word4)
            || !reader.TryReadNullTerminatedBytes(out var text8) || !reader.TryReadNullTerminatedBytes(out var textC))
            return false;
        entry = new(word0, word4, text8, textC);
        return true;
    }

}
