namespace Firefall.Reconstruction.Startup;

public sealed record MatrixMessage35Payload(
    ulong ValueAt28, ReadOnlyMemory<byte> BlobAt30, ReadOnlyMemory<byte> BlobAt40);

public enum MatrixMessage35ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixMessage35ReadResult(
    MatrixMessage35ReadOutcome Outcome, MatrixMessage35Payload? Payload, int BytesConsumed);

/// <summary>
/// Normal-buffer msg-35 application decoding only. Transport framing, alternate
/// header buffers and native reference-counted storage are not implemented.
/// </summary>
public static class MatrixMessage35Wire
{
    public const uint ReadBodyMethodFileOffset = 0x3570B0;
    public const uint ReadRawPairMethodFileOffset = 0x356F80;
    public const uint ReadLengthPrefixedBlobMethodFileOffset = 0x12EE170;
    public const int MinimumPacketSize = 13;

    public static MatrixMessage35ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixMessage35ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixMessage35ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value != MatrixMessage35NativeLayout.DecodedMessageId)
            return new(MatrixMessage35ReadOutcome.UnexpectedMessageCode, null, 0);

        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        if (!reader.TryReadUInt64(out var value)
            || !reader.TryReadWordLengthBlob(out var firstBlob)
            || !reader.TryReadWordLengthBlob(out var secondBlob))
            return new(MatrixMessage35ReadOutcome.TruncatedPayload, null, 0);
        return new(MatrixMessage35ReadOutcome.Accepted, new(value, firstBlob, secondBlob),
            header.BytesConsumed + reader.Position);
    }
}
