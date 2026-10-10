using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public sealed record MatrixRequestSigscan32Payload(uint CacheKeyAt28, byte RawFlagAt2C);

public enum MatrixRequestSigscan32ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixRequestSigscan32ReadResult(
    MatrixRequestSigscan32ReadOutcome Outcome, MatrixRequestSigscan32Payload? Payload, int BytesConsumed);

/// <summary>Offline normal application-buffer codec; does not send requests or perform signature scanning.</summary>
public static class MatrixRequestSigscan32Wire
{
    public const byte MessageCode = 32;
    public const int PacketSize = 6;
    public const int ApplicationBufferChannel = 1;
    public const uint EncoderFileOffset = 0x31E7B0;
    public const uint BodyReaderFileOffset = 0x345D20;
    public const uint SenderFileOffset = 0x31D350;
    public const uint NativeVtableVirtualAddress = 0x1C62384;
    public const string NativeTypeName = "matrix_fury::RequestSigscan";

    public static MatrixRequestSigscan32Payload FromRequestArguments(int firstArgument, uint requestedCacheKey) =>
        new(requestedCacheKey, firstArgument == 0 ? (byte)1 : (byte)0);

    public static byte[] Encode(MatrixRequestSigscan32Payload payload)
    {
        var bytes = new byte[PacketSize];
        bytes[0] = MessageCode;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(1), payload.CacheKeyAt28);
        bytes[5] = payload.RawFlagAt2C;
        return bytes;
    }

    public static MatrixRequestSigscan32ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixRequestSigscan32ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixRequestSigscan32ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value != MessageCode)
            return new(MatrixRequestSigscan32ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        if (!reader.TryReadUInt32(out uint key) || !reader.TryReadByte(out byte flag))
            return new(MatrixRequestSigscan32ReadOutcome.TruncatedPayload, null, 0);
        return new(MatrixRequestSigscan32ReadOutcome.Accepted,
            new(key, flag), header.BytesConsumed + reader.Position);
    }
}
