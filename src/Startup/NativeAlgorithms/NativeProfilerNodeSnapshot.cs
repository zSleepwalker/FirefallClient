using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeProfilerNodeSnapshot(
    int RawReadChildCount,
    IReadOnlyList<NativeProfilerNodeSnapshot> Children,
    byte ExpansionByte,
    IReadOnlyList<NativeProfilerSample> PhysicalHistorySlots,
    uint RawNextSlot,
    ReadOnlyMemory<byte> CachedNameBytes);

public enum NativeProfilerSnapshotReadOutcome
{
    Accepted,
    TruncatedSnapshot
}

public sealed record NativeProfilerSnapshotReadResult(
    NativeProfilerSnapshotReadOutcome Outcome, NativeProfilerNodeSnapshot? Snapshot, int BytesConsumed);

/// <summary>Derived profiler node/subtree snapshot only, not server61 wire format or whole-file framing.</summary>
public static class NativeProfilerNodeSnapshotCodec
{
    public const uint WriterFileOffset = 0xE8CE00;
    public const uint ReaderFileOffset = 0xE8CE70;
    public const uint BaseWriterFileOffset = 0xE8DD00;
    public const uint BaseReaderFileOffset = 0xE8DDA0;
    public const int PackedSampleSize = 26;
    public const int MinimumLeafSize = 2090;

    public static NativeProfilerSnapshotReadResult Decode(ReadOnlySpan<byte> bytes)
    {
        var reader = new MatrixMessageSpanReader(bytes);
        if (!reader.TryReadUInt32(out var count))
            return new(NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, null, reader.Position);
        var pending = new Stack<ReadFrame>();
        pending.Push(new(unchecked((int)count)));
        while (pending.TryPeek(out var current))
        {
            if (current.Children.Count < current.RawCount)
            {
                if (!reader.TryReadUInt32(out count))
                    return new(NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, null, reader.Position);
                pending.Push(new(unchecked((int)count)));
                continue;
            }
            if (!reader.TryReadByte(out byte expanded))
                return new(NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, null, reader.Position);
            var slots = new NativeProfilerSample[NativeProfilerHistory80.SlotCount];
            for (int i = 0; i < slots.Length; i++)
                if (!ReadSample(ref reader, out slots[i]))
                    return new(NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, null, reader.Position);
            if (!reader.TryReadUInt32(out uint next) || !reader.TryReadNullTerminatedBytes(out var name))
                return new(NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, null, reader.Position);
            var node = new NativeProfilerNodeSnapshot(current.RawCount, current.Children.AsReadOnly(),
                expanded, Array.AsReadOnly(slots), next, name);
            pending.Pop();
            if (!pending.TryPeek(out var parent))
                return new(NativeProfilerSnapshotReadOutcome.Accepted, node, reader.Position);
            parent.Children.Add(node);
        }
        throw new InvalidOperationException("Profiler snapshot parser lost its root frame.");
    }

    /// <summary>Encodes captured content, not live name lookup. Writes actual child count; negative decoded counts are not re-emitted.</summary>
    public static byte[] Encode(NativeProfilerNodeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        using var output = new MemoryStream();
        var pending = new Stack<(NativeProfilerNodeSnapshot Node, bool Tail)>();
        var active = new HashSet<NativeProfilerNodeSnapshot>(ReferenceEqualityComparer.Instance);
        pending.Push((snapshot, false));
        while (pending.TryPop(out var entry))
        {
            var node = entry.Node;
            if (!entry.Tail)
            {
                if (!active.Add(node))
                    throw new InvalidOperationException("Profiler snapshot input must be an acyclic subtree.");
                if (node.PhysicalHistorySlots.Count != NativeProfilerHistory80.SlotCount)
                    throw new ArgumentException("Profiler snapshots require exactly eighty physical history slots.", nameof(snapshot));
                WriteUInt32(output, (uint)node.Children.Count);
                pending.Push((node, true));
                for (int i = node.Children.Count - 1; i >= 0; i--)
                    pending.Push((node.Children[i], false));
                continue;
            }
            output.WriteByte(node.ExpansionByte);
            foreach (var sample in node.PhysicalHistorySlots)
            {
                WriteUInt16(output, sample.DivisorAt0);
                WriteUInt32(output, sample.ScaledAt4);
                WriteUInt32(output, sample.ScaledAt8);
                WriteUInt32(output, sample.ScaledAt0C);
                WriteUInt16(output, sample.RawWordAt10);
                WriteUInt16(output, sample.RawWordAt12);
                WriteUInt32(output, sample.ScaledAt14);
                WriteUInt32(output, sample.ScaledAt18);
            }
            WriteUInt32(output, node.RawNextSlot);
            var text = node.CachedNameBytes.Span;
            int terminator = text.IndexOf((byte)0);
            output.Write(terminator < 0 ? text : text[..terminator]);
            output.WriteByte(0);
            active.Remove(node);
        }
        return output.ToArray();
    }

    private static bool ReadSample(ref MatrixMessageSpanReader reader, out NativeProfilerSample sample)
    {
        sample = default;
        if (!reader.TryReadUInt16(out ushort divisor) || !reader.TryReadUInt32(out uint scaled4)
            || !reader.TryReadUInt32(out uint scaled8) || !reader.TryReadUInt32(out uint scaledC)
            || !reader.TryReadUInt16(out ushort raw10) || !reader.TryReadUInt16(out ushort raw12)
            || !reader.TryReadUInt32(out uint scaled14) || !reader.TryReadUInt32(out uint scaled18))
            return false;
        sample = new(divisor, scaled4, scaled8, scaledC, raw10, raw12, scaled14, scaled18);
        return true;
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private sealed class ReadFrame(int count)
    {
        public int RawCount { get; } = count;
        public List<NativeProfilerNodeSnapshot> Children { get; } = [];
    }
}
