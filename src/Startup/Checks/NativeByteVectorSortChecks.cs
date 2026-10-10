using Firefall.Reconstruction.Startup;

internal static class NativeByteVectorSortChecks
{
    public static void Run(Action<string, bool> check)
    {
        ReadOnlyMemory<byte>[] Ordered(int count) =>
            Enumerable.Range(0, count).Select(i => (ReadOnlyMemory<byte>)new byte[] { (byte)i }).ToArray();
        var threshold32 = NativeByteVectorSort.SortUnchecked(Ordered(32), 0);
        var threshold33 = NativeByteVectorSort.SortUnchecked(Ordered(33), 0);
        var negative = NativeByteVectorSort.SortUnchecked(Ordered(33), -1);
        check("Native byte-vector sort checks the32-record insertion threshold before the exhausted ideal budget",
            threshold32 is { PartitionCount: 0, InsertionRangeCount: 1, HeapFallbackRangeCount: 0 }
            && threshold33 is { PartitionCount: 0, InsertionRangeCount: 0, HeapFallbackRangeCount: 1 }
            && negative.HeapFallbackRangeCount == 1);
        var median41 = NativeByteVectorSort.SortUnchecked(Ordered(41), 1);
        var ninther42 = NativeByteVectorSort.SortUnchecked(Ordered(42), 1);
        var decayed = NativeByteVectorSort.SortUnchecked(Ordered(97), 1);
        check("Native pivot sampling switches from median-of-three to ninther at42 records, not40 or41",
            median41 is { PartitionCount: 1, MedianOfThreeCount: 1, NintherCount: 0 }
            && ninther42 is { PartitionCount: 1, MedianOfThreeCount: 0, NintherCount: 1 });
        check("Native ideal budget1 decays to zero after partition and falls back separately on both48-record sides",
            decayed is { PartitionCount: 1, HeapFallbackRangeCount: 2, MaxRecursionDepth: 2 });

        ReadOnlyMemory<byte>[] firstBranch = [new byte[] { 2 }, ReadOnlyMemory<byte>.Empty, new byte[] { 1 }];
        NativeByteVectorSort.InsertionSort(firstBranch);
        check("Native insertion first-element branch shifts an entire prefix even through an equivalent empty buffer",
            Keys(firstBranch).SequenceEqual(new[] { "01", "02", "" }));
        ReadOnlyMemory<byte>[] representative =
            [ReadOnlyMemory<byte>.Empty, new byte[] { 1, 2 }, new byte[] { 1, 3 }];
        var absorbed = NativeByteVectorSort.PartitionEqualRange(representative);
        check("Native partition reads its moved pivot representative; absorbing empty prefix can classify differing buffers as equivalent",
            absorbed == new NativeByteVectorPartition(0, 3)
            && Keys(representative).SequenceEqual(new[] { "", "0102", "0103" }));
        ReadOnlyMemory<byte>[] rotation =
            [new byte[] { 2 }, new byte[] { 0 }, new byte[] { 2 }, new byte[] { 1 }, new byte[] { 2 }];
        var rotated = NativeByteVectorSort.PartitionEqualRange(rotation);
        check("Native partition absorbs left equivalents and rotates the pivot block when only the right side remains",
            rotated == new NativeByteVectorPartition(2, 5)
            && Keys(rotation).SequenceEqual(new[] { "00", "01", "02", "02", "02" }));
        ReadOnlyMemory<byte>[] leftRotation =
            [new byte[] { 2 }, new byte[] { 3 }, new byte[] { 2 }, new byte[] { 4 }, new byte[] { 2 }];
        var leftRotated = NativeByteVectorSort.PartitionEqualRange(leftRotation);
        ReadOnlyMemory<byte>[] cross =
            [new byte[] { 2 }, new byte[] { 4 }, new byte[] { 2 }, new byte[] { 0 }, new byte[] { 2 }];
        var crossed = NativeByteVectorSort.PartitionEqualRange(cross);
        check("Native partition preserves the complementary left-only rotation and two-sided cross swap",
            leftRotated == new NativeByteVectorPartition(0, 3)
            && Keys(leftRotation).SequenceEqual(new[] { "02", "02", "02", "03", "04" })
            && crossed == new NativeByteVectorPartition(1, 4)
            && Keys(cross).SequenceEqual(new[] { "00", "02", "02", "02", "04" }));

        bool partitions = true, contents = true, sorting = true, shallow = true;
        uint state = 0x31415926;
        foreach (int count in new[] { 0, 1, 2, 3, 31, 32, 33, 40, 41, 42, 63, 97, 128, 255, 512, 2048 })
        {
            foreach (int pattern in Enumerable.Range(0, 5))
            {
                ReadOnlyMemory<byte>[] input = new ReadOnlyMemory<byte>[count];
                for (int i = 0; i < count; i++)
                {
                    state = unchecked(state * 1664525 + 1013904223);
                    ushort value = pattern switch
                    {
                        0 => (ushort)(state >> 16),
                        1 => (ushort)i,
                        2 => (ushort)(count - i),
                        3 => 7,
                        _ => (ushort)((i % 9) * 17)
                    };
                    input[i] = new byte[] { (byte)(value >> 8), (byte)value };
                }
                string[] expected = Keys(input).Order(StringComparer.Ordinal).ToArray();
                if (count > 0)
                {
                    ReadOnlyMemory<byte>[] partitionInput = input.ToArray();
                    var pair = NativeByteVectorSort.PartitionEqualRange(partitionInput);
                    partitions &= pair.FirstIndex >= 0 && pair.EndIndex > pair.FirstIndex && pair.EndIndex <= count;
                    var pivot = partitionInput[pair.FirstIndex];
                    for (int i = 0; i < pair.FirstIndex; i++)
                        partitions &= NativeByteVectorHeap.LessByCommonPrefix(partitionInput[i].Span, pivot.Span);
                    for (int i = pair.FirstIndex; i < pair.EndIndex; i++)
                        partitions &= partitionInput[i].Span.SequenceEqual(pivot.Span);
                    for (int i = pair.EndIndex; i < count; i++)
                        partitions &= NativeByteVectorHeap.LessByCommonPrefix(pivot.Span, partitionInput[i].Span);
                    contents &= Keys(partitionInput).Order(StringComparer.Ordinal).SequenceEqual(expected);
                }
                foreach (int ideal in new[] { 0, 1, count })
                {
                    ReadOnlyMemory<byte>[] actual = input.ToArray();
                    var trace = ideal == count
                        ? NativeByteVectorSort.Sort(actual)
                        : NativeByteVectorSort.SortUnchecked(actual, ideal);
                    sorting &= Keys(actual).SequenceEqual(expected);
                    contents &= Keys(actual).Order(StringComparer.Ordinal).SequenceEqual(expected);
                    shallow &= trace.MaxRecursionDepth <= 2 + (int)Math.Log2(Math.Max(1, count));
                }
            }
        }
        check("Native partition returns a nonempty exact equal range with strict fixed-width left/right partitions", partitions);
        check("Native partition/introsort preserve content multiplicities across randomized, sorted, reversed and duplicate-heavy inputs",
            contents);
        check("Native introsort matches independent fixed-width ordinal ordering across32/42 thresholds and multiple budget paths", sorting);
        check("Native smaller-side recursion and larger-side iteration keep stack depth logarithmic", shallow);
        var empty = NativeByteVectorSort.SortUnchecked(Span<ReadOnlyMemory<byte>>.Empty, int.MaxValue);
        bool emptyPartitionRejected = false;
        try { NativeByteVectorSort.PartitionEqualRange(Span<ReadOnlyMemory<byte>>.Empty); }
        catch (ArgumentException) { emptyPartitionRejected = true; }
        check("Native empty sort is a no-op while invalid empty partition is explicitly rejected",
            empty.PartitionCount == 0 && empty.InsertionRangeCount == 0
            && empty.HeapFallbackRangeCount == 0 && emptyPartitionRejected);
    }

    private static IEnumerable<string> Keys(IEnumerable<ReadOnlyMemory<byte>> buffers) =>
        buffers.Select(buffer => Convert.ToHexString(buffer.Span));
}
