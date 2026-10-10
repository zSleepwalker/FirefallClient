namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer34And36Payload;
public sealed record MatrixBufferSnapshotPayload(
    ReadOnlyMemory<byte> BlobBytesAt28, byte RawFlagAt38) : MatrixServer34And36Payload;
public sealed record MatrixAnnouncementPayload(
    ReadOnlyMemory<byte> TypeBytesAt28, ReadOnlyMemory<byte> DataBytesAt2C) : MatrixServer34And36Payload;

public enum MatrixServer34And36ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer34And36ReadResult(
    MatrixServer34And36ReadOutcome Outcome, MatrixServer34And36Payload? Payload, int BytesConsumed);

public static class MatrixServerMessages34And36Wire
{
    public const uint Message34FactoryFileOffset = 0x33D4F4;
    public const string Message34NativeTypeName = "matrix_fury::SigscanData";
    public const uint Message34ConstructorFileOffset = 0x339950;
    public const uint Message34BodyReaderFileOffset = 0x357180;
    public const uint Message36FactoryFileOffset = 0x33D5BA;
    public const uint Message36ConstructorFileOffset = 0x339C80;
    public const int Message34MinimumPacketSize = 4;
    public const int Message36MinimumPacketSize = 3;

    public static MatrixServer34And36ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer34And36ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer34And36ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not (34 or 36))
            return new(MatrixServer34And36ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixServer34And36Payload payload;
        if (header.Value == 34)
        {
            if (!reader.TryReadWordLengthBlob(out var blob) || !reader.TryReadByte(out var flag))
                return Truncated();
            payload = new MatrixBufferSnapshotPayload(blob, flag);
        }
        else
        {
            if (!reader.TryReadNullTerminatedBytes(out var type) || !reader.TryReadNullTerminatedBytes(out var data))
                return Truncated();
            payload = new MatrixAnnouncementPayload(type, data);
        }
        return new(MatrixServer34And36ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer34And36ReadResult Truncated() =>
        new(MatrixServer34And36ReadOutcome.TruncatedPayload, null, 0);
}
