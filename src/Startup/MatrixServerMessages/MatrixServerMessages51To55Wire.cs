namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer51To55Payload;

public sealed record MatrixChallengeIdOnlyPayload(uint MessageCode, ulong ChallengeId) : MatrixServer51To55Payload;

public readonly record struct MatrixChallengeRecordEntry(
    ulong QwordAt0, ReadOnlyMemory<byte> TextBytesAt8, byte ByteAtC, uint WordAt10, byte ByteAt14);

public sealed record MatrixChallengeRecordListPayload(
    ulong ChallengeId, IReadOnlyList<MatrixChallengeRecordEntry> Records) : MatrixServer51To55Payload;

public sealed record MatrixChallengeWordUpdatePayload(ulong ChallengeId, uint WordAt30) : MatrixServer51To55Payload;

public enum MatrixServer51To55ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer51To55ReadResult(
    MatrixServer51To55ReadOutcome Outcome, MatrixServer51To55Payload? Payload, int BytesConsumed);

public static class MatrixServerMessages51To55Wire
{
    public const uint Message52BodyReaderFileOffset = 0x350B60;
    public const uint Message52RecordReaderFileOffset = 0x350820;
    public const uint Message54BodyReaderFileOffset = 0x34FB90;
    public const int IdOnlyPacketSize = 9;
    public const int Message52MinimumPacketSize = 10;
    public const int Message52NativeRecordStride = 24;
    public const int Message54PacketSize = 13;

    public static MatrixServer51To55ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer51To55ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer51To55ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not uint messageCode || messageCode is < 51 or > 55)
            return new(MatrixServer51To55ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        if (!reader.TryReadUInt64(out var challengeId))
            return Truncated();
        MatrixServer51To55Payload payload;
        if (messageCode == 52)
        {
            if (!MatrixChunkedListWire.TryRead<MatrixChallengeRecordEntry>(ref reader, ReadRecord, out var records))
                return Truncated();
            payload = new MatrixChallengeRecordListPayload(challengeId, records);
        }
        else if (messageCode == 54)
        {
            if (!reader.TryReadUInt32(out var word))
                return Truncated();
            payload = new MatrixChallengeWordUpdatePayload(challengeId, word);
        }
        else
            payload = new MatrixChallengeIdOnlyPayload(messageCode, challengeId);
        return new(MatrixServer51To55ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer51To55ReadResult Truncated() =>
        new(MatrixServer51To55ReadOutcome.TruncatedPayload, null, 0);

    private static bool ReadRecord(ref MatrixMessageSpanReader reader, out MatrixChallengeRecordEntry record)
    {
        record = default;
        if (!reader.TryReadUInt64(out var qword) || !reader.TryReadNullTerminatedBytes(out var text)
            || !reader.TryReadByte(out var bC) || !reader.TryReadUInt32(out var word10) || !reader.TryReadByte(out var b14))
            return false;
        record = new(qword, text, bC, word10, b14);
        return true;
    }
}
