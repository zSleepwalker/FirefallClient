using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketChunkHeader(
    byte Channel, byte PayloadMarker, bool MoreFragments, ushort TotalLength);

public enum GameSocketChunkHeaderOutcome
{
    Accepted,
    TruncatedHeader,
    InvalidLength,
    TruncatedChunk,
    InvalidContinuation,
    UnsupportedControlMarker
}

public sealed record GameSocketChunkHeaderResult(
    GameSocketChunkHeaderOutcome Outcome, GameSocketChunkHeader? Header);

/// <summary>
/// Inspects only a chunk's two-byte network-order header. It does not read the
/// subsequent data sequence, execute controls, or emulate native queue ownership.
/// </summary>
public static class GameSocketChunkHeaderDecoder
{
    public const uint MethodFileOffset = 0xBD3140;
    public const int HeaderSize = 2;
    public const int MaximumChunkSize = 0x570;

    public static GameSocketChunkHeaderResult Inspect(ReadOnlySpan<byte> remainingBody)
    {
        if (remainingBody.Length < HeaderSize)
            return new(GameSocketChunkHeaderOutcome.TruncatedHeader, null);
        ushort word = BinaryPrimitives.ReadUInt16BigEndian(remainingBody);
        var header = new GameSocketChunkHeader(
            (byte)(word >> 14), (byte)((word >> 12) & 3),
            (word & 0x0800) != 0, (ushort)(word & 0x07FF));
        var outcome = header.TotalLength is < HeaderSize or > MaximumChunkSize
            ? GameSocketChunkHeaderOutcome.InvalidLength
            : header.TotalLength > remainingBody.Length
                ? GameSocketChunkHeaderOutcome.TruncatedChunk
                : header.MoreFragments && header.Channel is not (1 or 2)
                    ? GameSocketChunkHeaderOutcome.InvalidContinuation
                    : header.Channel == 0 && header.PayloadMarker != 0
                        ? GameSocketChunkHeaderOutcome.UnsupportedControlMarker
                        : GameSocketChunkHeaderOutcome.Accepted;
        return new(outcome, header);
    }
}
