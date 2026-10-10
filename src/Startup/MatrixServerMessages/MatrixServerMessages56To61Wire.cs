namespace Firefall.Reconstruction.Startup;

public abstract record MatrixServer56To61Payload;
public sealed record MatrixForceUnqueuePayload(
    byte ReasonCode, IReadOnlyList<ulong> InvolvedIds, uint WordAt38) : MatrixServer56To61Payload;
public sealed record MatrixRemainingBufferPayload(ReadOnlyMemory<byte> Bytes) : MatrixServer56To61Payload;
public sealed record MatrixClockUpdatePayload(MatrixClockParameters Parameters) : MatrixServer56To61Payload;
public sealed record MatrixSuperPingPayload(
    IReadOnlyList<ulong> Ticks, uint WordAt34) : MatrixServer56To61Payload;
public readonly record struct MatrixWordTextEntry(ushort WordAt0, ReadOnlyMemory<byte> TextBytesAt4);
public sealed record MatrixWordTextListsPayload(
    IReadOnlyList<MatrixWordTextEntry> ListAt28,
    IReadOnlyList<MatrixWordTextEntry> ListAt34) : MatrixServer56To61Payload;
public sealed record MatrixNestedWordNode(
    ushort WordAt0, IReadOnlyList<ushort> WordsAt10To1E, IReadOnlyList<MatrixNestedWordNode> Children);
public readonly record struct MatrixNestedWordRoot(ushort WordAt0, IReadOnlyList<MatrixNestedWordNode> Children);
public sealed record MatrixNestedWordRootsPayload(
    IReadOnlyList<MatrixNestedWordRoot> Roots) : MatrixServer56To61Payload;

public enum MatrixServer56To61ReadOutcome
{
    Accepted,
    TruncatedHeader,
    NonCanonicalHeader,
    UnexpectedMessageCode,
    TruncatedPayload
}

public sealed record MatrixServer56To61ReadResult(
    MatrixServer56To61ReadOutcome Outcome, MatrixServer56To61Payload? Payload, int BytesConsumed);

public static class MatrixServerMessages56To61Wire
{
    public const uint Message56ReaderFileOffset = 0x34FAA0;
    public const uint Message57ReaderFileOffset = 0x34F380;
    public const uint Message58ReaderFileOffset = 0x356B70;
    public const uint Message59ReaderFileOffset = 0x34F0D0;
    public const uint Message60ListReaderFileOffset = 0x34E700;
    public const uint Message61RootReaderFileOffset = 0x34D6D0;
    public const uint Message61NodeReaderFileOffset = 0x34BE80;
    public const int Message58PacketSize = 42;
    public const int Message61NativeRootStride = 16;
    public const int Message61NativeNodeStride = 32;

    /// <summary>Message57 consumes the entire supplied normal-buffer remainder, not a length-prefixed blob.</summary>
    public static MatrixServer56To61ReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var header = CompactPositiveUInt16Codec.Decode(bytes);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(MatrixServer56To61ReadOutcome.TruncatedHeader, null, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(MatrixServer56To61ReadOutcome.NonCanonicalHeader, null, 0);
        if (header.Value is not uint code || code is < 56 or > 61)
            return new(MatrixServer56To61ReadOutcome.UnexpectedMessageCode, null, 0);
        if (code == 57)
            return new(MatrixServer56To61ReadOutcome.Accepted,
                new MatrixRemainingBufferPayload(bytes[header.BytesConsumed..].ToArray()), bytes.Length);
        var reader = new MatrixMessageSpanReader(bytes[header.BytesConsumed..]);
        MatrixServer56To61Payload payload;
        switch (code)
        {
            case 56:
                if (!reader.TryReadByte(out var reason)
                    || !MatrixChunkedListWire.TryRead<ulong>(ref reader, ReadQword, out var ids)
                    || !reader.TryReadUInt32(out var word38))
                    return Truncated();
                payload = new MatrixForceUnqueuePayload(reason, ids, word38);
                break;
            case 58:
                if (!reader.TryReadUInt64(out var initial) || !reader.TryReadUInt64(out var reference)
                    || !reader.TryReadUInt64(out var scale) || !reader.TryReadUInt64(out var upper)
                    || !reader.TryReadUInt64(out var offset) || !reader.TryReadByte(out var hasUpper))
                    return Truncated();
                payload = new MatrixClockUpdatePayload(new(initial, reference, scale, upper, offset, hasUpper != 0));
                break;
            case 59:
                if (!MatrixChunkedListWire.TryRead<ulong>(ref reader, ReadQword, out var ticks)
                    || !reader.TryReadUInt32(out var word34))
                    return Truncated();
                payload = new MatrixSuperPingPayload(ticks, word34);
                break;
            case 60:
                if (!MatrixChunkedListWire.TryRead<MatrixWordTextEntry>(ref reader, ReadWordText, out var first)
                    || !MatrixChunkedListWire.TryRead<MatrixWordTextEntry>(ref reader, ReadWordText, out var second))
                    return Truncated();
                payload = new MatrixWordTextListsPayload(first, second);
                break;
            default:
                if (!MatrixChunkedListWire.TryRead<MatrixNestedWordRoot>(ref reader, ReadRoot, out var roots))
                    return Truncated();
                payload = new MatrixNestedWordRootsPayload(roots);
                break;
        }
        return new(MatrixServer56To61ReadOutcome.Accepted, payload, header.BytesConsumed + reader.Position);
    }

    private static MatrixServer56To61ReadResult Truncated() =>
        new(MatrixServer56To61ReadOutcome.TruncatedPayload, null, 0);

    private static bool ReadQword(ref MatrixMessageSpanReader reader, out ulong value) => reader.TryReadUInt64(out value);

    private static bool ReadWordText(ref MatrixMessageSpanReader reader, out MatrixWordTextEntry value)
    {
        value = default;
        if (!reader.TryReadUInt16(out var word) || !reader.TryReadNullTerminatedBytes(out var text))
            return false;
        value = new(word, text);
        return true;
    }

    private static bool ReadRoot(ref MatrixMessageSpanReader reader, out MatrixNestedWordRoot value)
    {
        value = default;
        if (!reader.TryReadUInt16(out var word) || !ReadNodes(ref reader, out var children))
            return false;
        value = new(word, children);
        return true;
    }

    private sealed class NodeListFrame
    {
        public ushort WordAt0 { get; init; }
        public IReadOnlyList<ushort> Words { get; init; } = Array.Empty<ushort>();
        public List<MatrixNestedWordNode> Children { get; } = new();
        public bool NeedsInitialCount { get; set; } = true;
        public byte Chunk { get; set; }
        public int Remaining { get; set; }
    }

    private static bool ReadNodes(ref MatrixMessageSpanReader reader, out IReadOnlyList<MatrixNestedWordNode> result)
    {
        result = Array.Empty<MatrixNestedWordNode>();
        // Native records recurse; an explicit stack avoids imposing a CLR call-stack depth limit.
        var stack = new Stack<NodeListFrame>();
        stack.Push(new());
        while (stack.Count != 0)
        {
            var frame = stack.Peek();
            if (frame.Remaining == 0)
            {
                if (frame.NeedsInitialCount || frame.Chunk == byte.MaxValue)
                {
                    if (!reader.TryReadByte(out var count))
                        return false;
                    frame.NeedsInitialCount = false;
                    frame.Chunk = count;
                    frame.Remaining = count;
                    if (count != 0)
                        continue;
                }
                stack.Pop();
                var children = frame.Children.AsReadOnly();
                if (stack.Count == 0)
                    result = children;
                else
                    stack.Peek().Children.Add(new(frame.WordAt0, frame.Words, children));
                continue;
            }
            if (!reader.TryReadUInt16(out var word))
                return false;
            var words = new ushort[8];
            for (int i = 0; i < words.Length; i++)
                if (!reader.TryReadUInt16(out words[i]))
                    return false;
            frame.Remaining--;
            stack.Push(new() { WordAt0 = word, Words = Array.AsReadOnly(words) });
        }
        return true;
    }
}
