namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer48To50Payload;

public sealed record MatrixChallengeInvitationCancelPayload(ulong ChallengeId) : MatrixServer48To50Payload;

public sealed record MatrixChallengeInvitationResponsePayload(
    ulong ChallengeId, ReadOnlyMemory<byte> NameBytes, byte StatusByte) : MatrixServer48To50Payload;

public sealed record MatrixChallengeKickedPayload(
    ulong ChallengeId, ReadOnlyMemory<byte> KickerNameBytes, ulong KickeeId, ReadOnlyMemory<byte> KickeeNameBytes)
    : MatrixServer48To50Payload;

public enum MatrixServer48To50ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer48To50ReadResult(
    MatrixServer48To50ReadOutcome Outcome, MatrixServer48To50Payload? Payload, int BytesConsumed);

public static class MatrixServerMessages48To50Wire
{
    public const uint Message48FactoryCaseFileOffset = 0x33DA5B;
    public const uint Message49BodyReaderFileOffset = 0x350D80;
    public const uint Message50BodyReaderFileOffset = 0x350CA0;
    public const int Message48PacketSize = 9;
    public const int Message49MinimumPacketSize = 11;
    public const int Message50MinimumPacketSize = 19;

    public static MatrixServer48To50ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer48To50ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer48To50ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not uint messageCode || messageCode is not (48 or 49 or 50))
            return new(MatrixServer48To50ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        if (!reader.TryReadUInt64(out var challengeId))
            return Truncated();
        MatrixServer48To50Payload payload;
        if (messageCode == 48)
            payload = new MatrixChallengeInvitationCancelPayload(challengeId);
        else if (messageCode == 49)
        {
            if (!reader.TryReadNullTerminatedBytes(out var name) || !reader.TryReadByte(out var status))
                return Truncated();
            payload = new MatrixChallengeInvitationResponsePayload(challengeId, name, status);
        }
        else
        {
            if (!reader.TryReadNullTerminatedBytes(out var kicker) || !reader.TryReadUInt64(out var kickeeId)
                || !reader.TryReadNullTerminatedBytes(out var kickee))
                return Truncated();
            payload = new MatrixChallengeKickedPayload(challengeId, kicker, kickeeId, kickee);
        }
        return new(MatrixServer48To50ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer48To50ReadResult Truncated() =>
        new(MatrixServer48To50ReadOutcome.TruncatedPayload, null, 0);
}
