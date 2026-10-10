namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer63To68Payload;
public sealed record MatrixWordSamplesPayload(uint MessageCode, IReadOnlyList<ushort> Words) : MatrixServer63To68Payload;
public sealed record MatrixLfgMatchFoundPayload(
    ulong MatchId, uint LfgCategoryId, ulong QwordAt38, ReadOnlyMemory<byte> LeaderNameBytes,
    IReadOnlyList<ReadOnlyMemory<byte>> TextListAt44) : MatrixServer63To68Payload;
public sealed record MatrixLfgLeaderChangePayload(
    ulong MatchId, ulong QwordAt30, ReadOnlyMemory<byte> LeaderNameBytes) : MatrixServer63To68Payload;
public sealed record MatrixZoneChatPayload(
    ulong AuthorId, ReadOnlyMemory<byte> AuthorNameBytes,
    ReadOnlyMemory<byte> TextBytes, uint RawFlags) : MatrixServer63To68Payload;
public sealed record MatrixMetadataMapsPayload(MatrixEnterZoneMetadata Metadata) : MatrixServer63To68Payload;

public enum MatrixServer63To68ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer63To68ReadResult(
    MatrixServer63To68ReadOutcome Outcome, MatrixServer63To68Payload? Payload, int BytesConsumed);

public static class MatrixServerMessages63To68Wire
{
    public const uint Message63ReaderFileOffset = 0x34B570;
    public const uint Message64ReaderFileOffset = 0x34B470;
    public const uint Message65ReaderFileOffset = 0x34B420;
    public const uint Message65TextListReaderFileOffset = 0x34B1F0;
    public const uint Message66ReaderFileOffset = 0x34AF70;
    public const uint Message67ReaderFileOffset = 0x34AE90;

    public static MatrixServer63To68ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer63To68ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer63To68ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not uint code || code is < 63 or > 68)
            return new(MatrixServer63To68ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixServer63To68Payload payload;
        if (code is 63 or 64)
        {
            var words = new ushort[code == 63 ? 2 : 7];
            for (int i = 0; i < words.Length; i++)
                if (!reader.TryReadUInt16(out words[i]))
                    return Truncated();
            payload = new MatrixWordSamplesPayload(code, Array.AsReadOnly(words));
        }
        else if (code == 65)
        {
            if (!reader.TryReadUInt64(out var match) || !reader.TryReadUInt32(out var category)
                || !reader.TryReadUInt64(out var qword38) || !reader.TryReadNullTerminatedBytes(out var leader)
                || !MatrixChunkedListWire.TryRead<ReadOnlyMemory<byte>>(ref reader, ReadText, out var texts))
                return Truncated();
            payload = new MatrixLfgMatchFoundPayload(match, category, qword38, leader, texts);
        }
        else if (code == 66)
        {
            if (!reader.TryReadUInt64(out var match) || !reader.TryReadUInt64(out var qword30)
                || !reader.TryReadNullTerminatedBytes(out var leader))
                return Truncated();
            payload = new MatrixLfgLeaderChangePayload(match, qword30, leader);
        }
        else if (code == 67)
        {
            if (!reader.TryReadUInt64(out var qword) || !reader.TryReadNullTerminatedBytes(out var first)
                || !reader.TryReadNullTerminatedBytes(out var second) || !reader.TryReadUInt32(out var word))
                return Truncated();
            payload = new MatrixZoneChatPayload(qword, first, second, word);
        }
        else
        {
            if (!MatrixEnterZoneMetadataWire.TryRead(ref reader, out var metadata) || metadata is null)
                return Truncated();
            payload = new MatrixMetadataMapsPayload(metadata);
        }
        return new(MatrixServer63To68ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static bool ReadText(ref MatrixMessageSpanReader reader, out ReadOnlyMemory<byte> value) =>
        reader.TryReadNullTerminatedBytes(out value);

    private static MatrixServer63To68ReadResult Truncated() =>
        new(MatrixServer63To68ReadOutcome.TruncatedPayload, null, 0);
}
