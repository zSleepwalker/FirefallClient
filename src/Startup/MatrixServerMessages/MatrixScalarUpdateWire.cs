namespace Firefall.Reconstruction.Startup;

public abstract record MatrixScalarUpdatePayload;

public sealed record MatrixMessage38Payload(
    ulong QwordAt28, uint WordBitsAt30, uint WordBitsAt34) : MatrixScalarUpdatePayload;

public sealed record MatrixMessage39Payload(byte ByteAt28) : MatrixScalarUpdatePayload;

public enum MatrixScalarUpdateReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixScalarUpdateReadResult(
    MatrixScalarUpdateReadOutcome Outcome, MatrixScalarUpdatePayload? Payload, int BytesConsumed);

/// <summary>
/// Normal-buffer bodies for msg-38, msg-39 and queue-status msg-62. Field widths are recovered;
/// semantic names and the interpretation of msg-38's 32-bit values are not.
/// </summary>
public static class MatrixScalarUpdateWire
{
    public const uint Message38Code = 38;
    public const uint Message39Code = 39;
    public const uint Message38FactoryCaseFileOffset = 0x33D68F;
    public const uint Message39FactoryCaseFileOffset = 0x33D6F2;
    public const uint Message38ConstructorFileOffset = 0x33A340;
    public const uint Message39ConstructorFileOffset = 0x33A420;
    public const uint Message38BodyReaderFileOffset = 0x3545D0;
    public const uint Message39ByteReaderFileOffset = 0x337180;
    public const int Message38DescriptorSize = 0x38;
    public const int Message39DescriptorSize = 0x30;
    public const int Message38PacketSize = 17;
    public const int Message39PacketSize = 2;
    public const uint Message38DefaultWordBitsAt30 = 0x3F800000;

    public static MatrixScalarUpdateReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixScalarUpdateReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixScalarUpdateReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not (Message38Code or Message39Code or MatrixQueueStatusNativeLayout.MessageCode))
            return new(MatrixScalarUpdateReadOutcome.UnexpectedMessageCode, null, 0);

        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixScalarUpdatePayload payload;
        if (header.Value == Message38Code)
        {
            if (!reader.TryReadUInt64(out var qword)
                || !reader.TryReadUInt32(out var word0) || !reader.TryReadUInt32(out var word1))
                return new(MatrixScalarUpdateReadOutcome.TruncatedPayload, null, 0);
            payload = new MatrixMessage38Payload(qword, word0, word1);
        }
        else if (header.Value == Message39Code)
        {
            if (!reader.TryReadByte(out var value))
                return new(MatrixScalarUpdateReadOutcome.TruncatedPayload, null, 0);
            payload = new MatrixMessage39Payload(value);
        }
        else
        {
            if (!reader.TryReadUInt32(out var position) || !reader.TryReadUInt32(out var length))
                return new(MatrixScalarUpdateReadOutcome.TruncatedPayload, null, 0);
            payload = new MatrixQueueStatusPayload(position, length);
        }
        return new(MatrixScalarUpdateReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }
}

public readonly record struct MatrixMessage39ObservedUpdate(byte SubsystemInputByte, byte ConnectionByteAt8A0);

public static class MatrixScalarUpdateNativeLayout
{
    public const uint Message38HandlerFileOffset = 0x491380;
    public const uint Message39HandlerFileOffset = 0x4913A0;
    public const int Message38DestinationBlockOffset = 0x8D0;
    public const int Message38CopiedByteCount = 16;
    public const int Message39DestinationByteOffset = 0x8A0;

    public static MatrixMessage39ObservedUpdate CaptureMessage39Update(byte value, bool hasGlobalSubsystem)
    {
        if (!hasGlobalSubsystem)
            throw new ArgumentException("The native msg-39 handler requires the global subsystem at +C64.",
                nameof(hasGlobalSubsystem));
        return new(value, value);
    }
}
