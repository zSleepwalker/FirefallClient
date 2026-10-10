namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeByteVectorPartition(int FirstIndex, int EndIndex);

/// <summary>Managed observation counters, not fields serialized or stored by the native sorter.</summary>
public readonly record struct NativeByteVectorSortTrace(
    int PartitionCount, int MedianOfThreeCount, int NintherCount,
    int InsertionRangeCount, int HeapFallbackRangeCount, int MaxRecursionDepth);

/// <summary>
/// Native introsort/insertion content projection using the prefix-only byte comparator.
/// The regular wrapper starts with an element-count budget; the unchecked entry also accepts an explicit budget.
/// </summary>
public static class NativeByteVectorSort
{
    public const uint SortUncheckedMethodFileOffset = 0x127E8A0;
    public const uint SortMethodFileOffset = 0x127E4F0;
    public const uint ForwardingSortMethodFileOffset = 0x127E6D0;
    public const uint PartitionMethodFileOffset = 0x127EB60;
    public const uint SelectMedianMethodFileOffset = 0x127F3D0;
    public const uint InsertionSortMethodFileOffset = 0x127F6F0;
    public const uint OrderThreeMethodFileOffset = 0x127F8F0;
    public const int InsertionThreshold = 32;
    public const int NintherMinimumCount = 42;

    public static NativeByteVectorSortTrace Sort(Span<ReadOnlyMemory<byte>> buffers) =>
        SortUnchecked(buffers, buffers.Length);

    public static NativeByteVectorSortTrace SortUnchecked(Span<ReadOnlyMemory<byte>> buffers, int ideal)
    {
        var trace = new Trace();
        SortRange(buffers, ideal, 1, ref trace);
        return new(trace.Partitions, trace.Medians, trace.Ninthers,
            trace.Insertions, trace.HeapFallbacks, trace.MaxDepth);
    }

    public static NativeByteVectorPartition PartitionEqualRange(Span<ReadOnlyMemory<byte>> buffers)
    {
        if (buffers.IsEmpty)
            throw new ArgumentException("Native partition requires a pivot buffer.", nameof(buffers));
        var trace = new Trace();
        return PartitionCore(buffers, ref trace);
    }

    public static void InsertionSort(Span<ReadOnlyMemory<byte>> buffers)
    {
        for (int next = 1; next < buffers.Length; next++)
        {
            byte[] saved = buffers[next].ToArray();
            int hole = next;
            if (Less(saved, buffers[0].Span))
            {
                // The native first-element branch shifts the entire prefix, not just greater predecessors.
                while (hole > 0)
                {
                    buffers[hole] = buffers[hole - 1].ToArray();
                    hole--;
                }
            }
            else
            {
                while (Less(saved, buffers[hole - 1].Span))
                {
                    buffers[hole] = buffers[hole - 1].ToArray();
                    hole--;
                    if (hole == 0)
                        throw new InvalidOperationException("Native insertion sentinel was crossed.");
                }
            }
            buffers[hole] = saved;
        }
    }

    private static void SortRange(Span<ReadOnlyMemory<byte>> buffers, int ideal, int depth, ref Trace trace)
    {
        trace.MaxDepth = Math.Max(trace.MaxDepth, depth);
        while (buffers.Length > InsertionThreshold)
        {
            if (ideal <= 0)
            {
                trace.HeapFallbacks++;
                NativeByteVectorHeap.MakeHeap(buffers);
                NativeByteVectorHeap.SortHeap(buffers);
                return;
            }
            var partition = PartitionCore(buffers, ref trace);
            ideal = ideal / 2 + ideal / 2 / 2;
            int leftCount = partition.FirstIndex;
            int rightCount = buffers.Length - partition.EndIndex;
            if (leftCount >= rightCount)
            {
                SortRange(buffers[partition.EndIndex..], ideal, depth + 1, ref trace);
                buffers = buffers[..partition.FirstIndex];
            }
            else
            {
                SortRange(buffers[..partition.FirstIndex], ideal, depth + 1, ref trace);
                buffers = buffers[partition.EndIndex..];
            }
        }
        if (buffers.Length > 1)
        {
            trace.Insertions++;
            InsertionSort(buffers);
        }
    }

    private static NativeByteVectorPartition PartitionCore(Span<ReadOnlyMemory<byte>> buffers, ref Trace trace)
    {
        trace.Partitions++;
        int first = buffers.Length / 2;
        SelectMedianPivot(buffers, first, ref trace);
        int end = first + 1;
        while (first > 0 && Equivalent(buffers[first - 1].Span, buffers[first].Span))
            first--;
        while (end < buffers.Length && Equivalent(buffers[end].Span, buffers[first].Span))
            end++;
        int left = first, right = end;
        for (;;)
        {
            // Read the current first pivot buffer: absorbing an unequal prefix may change its representative.
            while (right < buffers.Length)
            {
                if (Less(buffers[first].Span, buffers[right].Span))
                    right++;
                else if (Less(buffers[right].Span, buffers[first].Span))
                    break;
                else
                {
                    if (end != right)
                        Swap(buffers, end, right);
                    end++;
                    right++;
                }
            }
            while (left > 0)
            {
                if (Less(buffers[left - 1].Span, buffers[first].Span))
                    left--;
                else if (Less(buffers[first].Span, buffers[left - 1].Span))
                    break;
                else
                {
                    first--;
                    if (first != left - 1)
                        Swap(buffers, first, left - 1);
                    left--;
                }
            }
            if (left == 0 && right == buffers.Length)
                return new(first, end);
            if (left == 0)
            {
                if (end != right)
                    Swap(buffers, first, end);
                end++;
                Swap(buffers, first++, right++);
            }
            else if (right == buffers.Length)
            {
                left--;
                first--;
                if (left != first)
                    Swap(buffers, left, first);
                end--;
                Swap(buffers, first, end);
            }
            else
                Swap(buffers, --left, right++);
        }
    }

    private static void SelectMedianPivot(Span<ReadOnlyMemory<byte>> buffers, int middle, ref Trace trace)
    {
        int last = buffers.Length - 1;
        if (buffers.Length < NintherMinimumCount)
        {
            trace.Medians++;
            OrderThree(buffers, 0, middle, last);
        }
        else
        {
            trace.Ninthers++;
            int step = buffers.Length / 8;
            OrderThree(buffers, 0, step, 2 * step);
            OrderThree(buffers, middle - step, middle, middle + step);
            OrderThree(buffers, last - 2 * step, last - step, last);
            OrderThree(buffers, step, middle, last - step);
        }
    }

    private static void OrderThree(Span<ReadOnlyMemory<byte>> buffers, int first, int middle, int last)
    {
        if (Less(buffers[middle].Span, buffers[first].Span))
            Swap(buffers, middle, first);
        if (Less(buffers[last].Span, buffers[middle].Span))
        {
            Swap(buffers, last, middle);
            if (Less(buffers[middle].Span, buffers[first].Span))
                Swap(buffers, middle, first);
        }
    }

    private static bool Less(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) =>
        NativeByteVectorHeap.LessByCommonPrefix(left, right);

    private static bool Equivalent(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) =>
        !Less(left, right) && !Less(right, left);

    private static void Swap(Span<ReadOnlyMemory<byte>> buffers, int first, int second) =>
        NativeByteVectorHeap.SwapByCopy(buffers, first, second);

    private struct Trace
    {
        public int Partitions, Medians, Ninthers, Insertions, HeapFallbacks, MaxDepth;
    }
}
