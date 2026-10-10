namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer44To47Payload;

public sealed record MatrixFoundMatchUpdatePayload(
    uint MatchmakerId, ulong MatchId, byte StateByte,
    ushort WordAt3A, ushort WordAt3C, ushort WordAt3E, ushort WordAt40) : MatrixServer44To47Payload;

public readonly record struct MatrixChallengeJoinEntry(
    uint WordAt0, ReadOnlyMemory<byte> TextBytesAt4, ReadOnlyMemory<byte> TextBytesAt8,
    byte ByteAtC, byte ByteAtD, byte ByteAtE);

public sealed record MatrixChallengeJoinResponsePayload(
    ulong ChallengeId, byte JoinSucceededByte, byte CreatedByte, byte SquadReasonByte, uint WordAt34,
    IReadOnlyList<MatrixChallengeJoinEntry> Entries, uint WordAt44) : MatrixServer44To47Payload;

public sealed record MatrixChallengeInvitationPayload(
    uint MessageCode, ulong QwordAt28, ulong QwordAt30, ReadOnlyMemory<byte> NameBytes) : MatrixServer44To47Payload;

public enum MatrixServer44To47ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer44To47ReadResult(
    MatrixServer44To47ReadOutcome Outcome, MatrixServer44To47Payload? Payload, int BytesConsumed);

/// <summary>Normal-buffer decoding; native layout padding is not serialized.</summary>
public static class MatrixServerMessages44To47Wire
{
    public const uint Message44BodyReaderFileOffset = 0x352190;
    public const uint Message45BodyReaderFileOffset = 0x352130;
    public const uint Message45EntryReaderFileOffset = 0x351C10;
    public const uint Message46BodyReaderFileOffset = 0x350F70;
    public const uint Message47BodyReaderFileOffset = 0x350EA0;
    public const int Message44PacketSize = 22;
    public const int Message45MinimumPacketSize = 21;
    public const int Message46And47MinimumPacketSize = 18;

    public static MatrixServer44To47ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer44To47ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer44To47ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not uint messageCode || messageCode is not (44 or 45 or 46 or 47))
            return new(MatrixServer44To47ReadOutcome.UnexpectedMessageCode, null, 0);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixServer44To47Payload payload;
        if (messageCode == 44)
        {
            if (!reader.TryReadUInt32(out var matchmaker) || !reader.TryReadUInt64(out var matchId)
                || !reader.TryReadByte(out var state) || !reader.TryReadUInt16(out var w3A)
                || !reader.TryReadUInt16(out var w3C) || !reader.TryReadUInt16(out var w3E)
                || !reader.TryReadUInt16(out var w40))
                return Truncated();
            payload = new MatrixFoundMatchUpdatePayload(matchmaker, matchId, state, w3A, w3C, w3E, w40);
        }
        else if (messageCode == 45)
        {
            if (!reader.TryReadUInt64(out var challengeId) || !reader.TryReadByte(out var success)
                || !reader.TryReadByte(out var created) || !reader.TryReadByte(out var squad)
                || !reader.TryReadUInt32(out var w34)
                || !MatrixChunkedListWire.TryRead<MatrixChallengeJoinEntry>(ref reader, ReadJoinEntry, out var entries)
                || !reader.TryReadUInt32(out var w44))
                return Truncated();
            payload = new MatrixChallengeJoinResponsePayload(challengeId, success, created, squad, w34, entries, w44);
        }
        else
        {
            if (!reader.TryReadUInt64(out var q28) || !reader.TryReadUInt64(out var q30)
                || !reader.TryReadNullTerminatedBytes(out var name))
                return Truncated();
            payload = new MatrixChallengeInvitationPayload(messageCode, q28, q30, name);
        }
        return new(MatrixServer44To47ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer44To47ReadResult Truncated() =>
        new(MatrixServer44To47ReadOutcome.TruncatedPayload, null, 0);

    private static bool ReadJoinEntry(ref MatrixMessageSpanReader reader, out MatrixChallengeJoinEntry entry)
    {
        entry = default;
        if (!reader.TryReadUInt32(out var word) || !reader.TryReadNullTerminatedBytes(out var text4)
            || !reader.TryReadNullTerminatedBytes(out var text8) || !reader.TryReadByte(out var bC)
            || !reader.TryReadByte(out var bD) || !reader.TryReadByte(out var bE))
            return false;
        entry = new(word, text4, text8, bC, bD, bE);
        return true;
    }
}
