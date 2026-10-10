using Firefall.Reconstruction.Startup;

internal static class NativeByteVectorHeapChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool Less(byte[] a, byte[] b) => NativeByteVectorHeap.LessByCommonPrefix(a, b);
        check("Native byte-vector comparison ignores length after an equal common prefix, including empty buffers",
            !Less([], [1]) && !Less([1], [])
            && !Less([1], [1, 2]) && !Less([1, 2], [1]));
        check("Native byte-vector comparison uses unsigned bytes rather than signed bytes or little-endian DWORD order",
            Less([0x7F], [0x80]) && !Less([0xFF], [0])
            && Less([1, 0xFF, 0, 0], [2, 0, 0, 0]));
        bool boundaries = true;
        for (int length = 0; length <= 17; length++)
        {
            byte[] prefix = Enumerable.Repeat((byte)0xA5, length).ToArray();
            byte[] longer = prefix.Concat(new byte[] { 0xFF }).ToArray();
            boundaries &= !Less(prefix, longer) && !Less(longer, prefix);
            for (int differing = 0; differing < length; differing++)
            {
                byte[] lower = prefix.ToArray();
                lower[differing]--;
                boundaries &= Less(lower, prefix) && !Less(prefix, lower);
            }
        }
        check("Native byte-vector comparison preserves prefix and first-difference behavior across all four-byte boundaries",
            boundaries);
        check("Native unequal-length prefix equivalence is nontransitive and must not be treated as ordinary ordered equality",
            !Less([1], [1, 2]) && !Less([1, 2], [1])
            && !Less([1], [1, 3]) && !Less([1, 3], [1])
            && Less([1, 2], [1, 3]));

        ReadOnlyMemory<byte>[] ties = [new byte[] { 9 }, new byte[] { 1, 2 }, new byte[] { 1 }];
        byte[] saved = [0];
        NativeByteVectorHeap.AdjustHole(ties, 0, saved);
        saved[0] = 0xFF;
        check("Native heap adjust selects the right child on prefix-equivalent children and owns the saved bytes",
            ties[0].Span.SequenceEqual(new byte[] { 1 })
            && ties[1].Span.SequenceEqual(new byte[] { 1, 2 })
            && ties[2].Span.SequenceEqual(new byte[] { 0 }));

        ReadOnlyMemory<byte>[] upward =
            [new byte[] { 9 }, new byte[] { 5 }, new byte[] { 4 }, new byte[] { 2 }];
        NativeByteVectorHeap.PushSavedValue(upward, 3, 1, new byte[] { 10 });
        check("Native heap push respects its top boundary and shifts parent content down",
            Values(upward).SequenceEqual(new[] { "09", "0A", "04", "05" }));
        ReadOnlyMemory<byte>[] singleton = [new byte[] { 7 }];
        NativeByteVectorHeap.PopHeap(singleton);
        check("Native singleton pop retains content; empty make/sort ranges are no-ops",
            singleton[0].Span.SequenceEqual(new byte[] { 7 }));
        NativeByteVectorHeap.MakeHeap(Span<ReadOnlyMemory<byte>>.Empty);
        NativeByteVectorHeap.SortHeap(Span<ReadOnlyMemory<byte>>.Empty);

        bool heaps = true, sorting = true, preservation = true;
        uint state = 0x13579BDF;
        for (int count = 0; count <= 80; count++)
        {
            ReadOnlyMemory<byte>[] values = new ReadOnlyMemory<byte>[count];
            for (int i = 0; i < count; i++)
            {
                state = unchecked(state * 1664525 + 1013904223);
                values[i] = new byte[] { (byte)(state >> 24), (byte)(state >> 16), (byte)(state >> 8) };
            }
            string[] expected = Values(values).Order(StringComparer.Ordinal).ToArray();
            NativeByteVectorHeap.MakeHeap(values);
            for (int length = count; length > 1; length--)
            {
                heaps &= IsHeap(values.AsSpan(0, length));
                string maximum = Values(values.Take(length)).Max(StringComparer.Ordinal)!;
                NativeByteVectorHeap.PopHeap(values.AsSpan(0, length));
                heaps &= Convert.ToHexString(values[length - 1].Span) == maximum;
            }
            sorting &= Values(values).SequenceEqual(expected);
            preservation &= Values(values).Order(StringComparer.Ordinal).SequenceEqual(expected);
            Array.Reverse(values);
            NativeByteVectorHeap.MakeHeap(values);
            NativeByteVectorHeap.SortHeap(values);
            sorting &= Values(values).SequenceEqual(expected);
        }
        check("Native make/pop heaps retain the maximum and valid parent-child relations for fixed-width byte records", heaps);
        check("Native make/sort heap matches independent ordinal hexadecimal ordering for fixed-width records", sorting);
        check("Native heap operations preserve every input buffer's contents and duplicate multiplicity", preservation);
        ReadOnlyMemory<byte>[] duplicates =
            [new byte[] { 2 }, new byte[] { 1 }, new byte[] { 2 }, new byte[] { 1 }, new byte[] { 2 }];
        NativeByteVectorHeap.MakeHeap(duplicates);
        NativeByteVectorHeap.SortHeap(duplicates);
        check("Native heap sorting retains exact duplicate multiplicities without deduplication",
            Values(duplicates).SequenceEqual(new[] { "01", "01", "02", "02", "02" }));

        byte[] original = [9];
        ReadOnlyMemory<byte>[] ownership = [original, new byte[] { 5 }, new byte[] { 1 }];
        NativeByteVectorHeap.PopHeap(ownership);
        original[0] = 0;
        check("Native pop copies root contents to the tail rather than leaving a caller-owned byte-array alias",
            ownership[^1].Span.SequenceEqual(new byte[] { 9 }));
        bool emptyRejected = false, holeRejected = false, topRejected = false;
        try { NativeByteVectorHeap.PopHeap(Span<ReadOnlyMemory<byte>>.Empty); }
        catch (ArgumentException) { emptyRejected = true; }
        try { NativeByteVectorHeap.AdjustHole(singleton, 1, []); }
        catch (ArgumentOutOfRangeException) { holeRejected = true; }
        try { NativeByteVectorHeap.PushSavedValue(singleton, 0, 1, []); }
        catch (ArgumentOutOfRangeException) { topRejected = true; }
        check("Native byte-vector heap rejects unmanaged out-of-range accesses explicitly",
            emptyRejected && holeRejected && topRejected);
    }

    private static IEnumerable<string> Values(IEnumerable<ReadOnlyMemory<byte>> values) =>
        values.Select(value => Convert.ToHexString(value.Span));

    private static bool IsHeap(ReadOnlySpan<ReadOnlyMemory<byte>> values)
    {
        for (int i = 1; i < values.Length; i++)
            if (NativeByteVectorHeap.LessByCommonPrefix(values[(i - 1) / 2].Span, values[i].Span))
                return false;
        return true;
    }
}
