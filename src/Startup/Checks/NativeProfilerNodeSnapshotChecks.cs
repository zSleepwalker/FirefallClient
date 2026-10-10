using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class NativeProfilerNodeSnapshotChecks
{
    public static void Run(Action<string, bool> check)
    {
        var sample = new NativeProfilerSample(0x1234, 0x11223344, 2, 3, 0x5678, 0x9ABC, 4, uint.MaxValue);
        var slots = Enumerable.Range(0, 80).Select(i => sample with { ScaledAt8 = (uint)i }).ToArray();
        var leaf = new NativeProfilerNodeSnapshot(0, [], 0xA5, slots, uint.MaxValue, "name"u8.ToArray());
        byte[] encoded = NativeProfilerNodeSnapshotCodec.Encode(leaf);
        check("Profiler snapshot leaf packs eighty26-byte samples without native two-byte padding and preserves raw flags/index",
            encoded.Length == NativeProfilerNodeSnapshotCodec.MinimumLeafSize + 4
            && encoded.AsSpan(0, 7).SequenceEqual(new byte[] { 0, 0, 0, 0, 0xA5, 0x34, 0x12 })
            && BinaryPrimitives.ReadUInt32LittleEndian(encoded.AsSpan(7)) == 0x11223344
            && BinaryPrimitives.ReadUInt16LittleEndian(encoded.AsSpan(19)) == 0x5678
            && BinaryPrimitives.ReadUInt32LittleEndian(encoded.AsSpan(2085)) == uint.MaxValue
            && encoded.AsSpan(2089).SequenceEqual("name\0"u8));
        var decoded = NativeProfilerNodeSnapshotCodec.Decode(encoded);
        check("Profiler snapshot restores physical slot order/raw UInt32 next index, not chronological or normalized history",
            decoded is { Outcome: NativeProfilerSnapshotReadOutcome.Accepted, Snapshot.RawNextSlot: uint.MaxValue }
            && decoded.BytesConsumed == encoded.Length && decoded.Snapshot.ExpansionByte == 0xA5
            && decoded.Snapshot.PhysicalHistorySlots.SequenceEqual(slots));
        bool truncated = true;
        for (int length = 0; length < encoded.Length; length++)
            truncated &= NativeProfilerNodeSnapshotCodec.Decode(encoded.AsSpan(0, length))
                is { Outcome: NativeProfilerSnapshotReadOutcome.TruncatedSnapshot, Snapshot: null };
        check("Profiler snapshot every truncated leaf prefix fails explicitly including missing name terminator", truncated);
        encoded[2089] = (byte)'X';
        check("Profiler decoded snapshot text owns independent bytes",
            decoded.Snapshot!.CachedNameBytes.Span.SequenceEqual("name"u8));
        var withTrailing = NativeProfilerNodeSnapshotCodec.Decode(encoded.Concat(new byte[] { 0xFF }).ToArray());
        check("Profiler snapshot reports exact consumption and leaves following framing bytes alone",
            withTrailing.Outcome == NativeProfilerSnapshotReadOutcome.Accepted && withTrailing.BytesConsumed == encoded.Length);
        var subtree = new NativeProfilerNodeSnapshot(2, [leaf, leaf], 1, slots, 79, new byte[] { (byte)'a', 0, (byte)'b' });
        byte[] treeBytes = NativeProfilerNodeSnapshotCodec.Encode(subtree);
        var treeResult = NativeProfilerNodeSnapshotCodec.Decode(treeBytes);
        check("Profiler snapshot child bodies precede parent flags/history/name, and snapshot CString encoding truncates embedded NUL",
            BinaryPrimitives.ReadUInt32LittleEndian(treeBytes) == 2
            && treeResult.Snapshot is { Children.Count: 2, ExpansionByte: 1, RawNextSlot: 79 }
            && treeResult.Snapshot.CachedNameBytes.Span.SequenceEqual("a"u8)
            && NativeProfilerNodeSnapshotCodec.Encode(treeResult.Snapshot).SequenceEqual(treeBytes));
        byte[] negative = NativeProfilerNodeSnapshotCodec.Encode(leaf);
        BinaryPrimitives.WriteInt32LittleEndian(negative, -1);
        var negativeResult = NativeProfilerNodeSnapshotCodec.Decode(negative);
        check("Profiler signed negative child count is read as no children, while writer re-counts actual children as zero",
            negativeResult.Snapshot is { RawReadChildCount: -1, Children.Count: 0 }
            && BinaryPrimitives.ReadUInt32LittleEndian(NativeProfilerNodeSnapshotCodec.Encode(negativeResult.Snapshot)) == 0);
        NativeProfilerNodeSnapshot deep = leaf;
        for (int i = 0; i < 600; i++)
            deep = new(1, [deep], 0, slots, 0, ReadOnlyMemory<byte>.Empty);
        var deepResult = NativeProfilerNodeSnapshotCodec.Decode(NativeProfilerNodeSnapshotCodec.Encode(deep));
        int depth = 0;
        for (var cursor = deepResult.Snapshot; cursor?.Children.Count > 0; cursor = cursor.Children[0])
            depth++;
        check("Profiler snapshot encoder/decoder traverse600 nested child bodies without a managed recursion cap",
            deepResult.Outcome == NativeProfilerSnapshotReadOutcome.Accepted && depth == 600);
        bool badSlots = false, cycle = false;
        try { NativeProfilerNodeSnapshotCodec.Encode(leaf with { PhysicalHistorySlots = [] }); }
        catch (ArgumentException) { badSlots = true; }
        var children = new List<NativeProfilerNodeSnapshot>();
        var cyclic = leaf with { Children = children };
        children.Add(cyclic);
        try { NativeProfilerNodeSnapshotCodec.Encode(cyclic); }
        catch (InvalidOperationException) { cycle = true; }
        check("Profiler snapshot invalid slot count and cyclic managed input are explicit errors", badSlots && cycle);
    }
}
