namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Content projection of heaps over native 12-byte owning byte-buffer headers.
/// Common-prefix equality has no length tie-break; arbitrary unequal lengths do not define a strict weak order.
/// </summary>
public static class NativeByteVectorHeap
{
    public const uint LessMethodFileOffset = 0x127C8F0;
    public const uint MakeHeapMethodFileOffset = 0x127F550;
    public const uint SortHeapMethodFileOffset = 0x127F610;
    public const uint SwapMethodFileOffset = 0x127F9F0;
    public const uint AdjustHoleMethodFileOffset = 0x127FA80;
    public const uint PopHeapMethodFileOffset = 0x127FB10;
    public const uint PushSavedValueMethodFileOffset = 0x127FBD0;
    public const uint DuplicatePopHeapMethodFileOffset = 0x127FC50;
    public const int NativeHeaderStride = 12;

    public static bool LessByCommonPrefix(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        int commonLength = Math.Min(left.Length, right.Length);
        return left[..commonLength].SequenceCompareTo(right[..commonLength]) < 0;
    }

    public static void SwapByCopy(Span<ReadOnlyMemory<byte>> buffers, int first, int second)
    {
        if (first < 0 || first >= buffers.Length)
            throw new ArgumentOutOfRangeException(nameof(first));
        if (second < 0 || second >= buffers.Length)
            throw new ArgumentOutOfRangeException(nameof(second));
        byte[] saved = buffers[first].ToArray();
        buffers[first] = buffers[second].ToArray();
        buffers[second] = saved;
    }

    public static void MakeHeap(Span<ReadOnlyMemory<byte>> buffers)
    {
        for (int hole = buffers.Length / 2 - 1; hole >= 0; hole--)
            AdjustHole(buffers, hole, buffers[hole].Span);
    }

    /// <summary>Repeatedly pops the supplied heap; this does not first heapify an arbitrary input.</summary>
    public static void SortHeap(Span<ReadOnlyMemory<byte>> heap)
    {
        for (int length = heap.Length; length > 1; length--)
            PopHeap(heap[..length]);
    }

    public static void PopHeap(Span<ReadOnlyMemory<byte>> heap)
    {
        if (heap.IsEmpty)
            throw new ArgumentException("Native pop-heap requires a last buffer.", nameof(heap));

        byte[] saved = heap[^1].ToArray();
        heap[^1] = heap[0].ToArray();
        if (heap.Length == 1)
            heap[0] = saved;
        else
            AdjustOwnedHole(heap[..^1], 0, saved);
    }

    public static void AdjustHole(Span<ReadOnlyMemory<byte>> heap, int hole, ReadOnlySpan<byte> savedValue)
    {
        ValidateHole(heap.Length, hole, hole);
        AdjustOwnedHole(heap, hole, savedValue.ToArray());
    }

    public static void PushSavedValue(Span<ReadOnlyMemory<byte>> heap, int hole, int top,
        ReadOnlySpan<byte> savedValue)
    {
        ValidateHole(heap.Length, hole, top);
        PushOwnedSavedValue(heap, hole, top, savedValue.ToArray());
    }

    private static void AdjustOwnedHole(Span<ReadOnlyMemory<byte>> heap, int hole,
        ReadOnlySpan<byte> savedValue)
    {
        int top = hole;
        long right = 2L * hole + 2;
        while (right < heap.Length)
        {
            int child = (int)right;
            // Equivalent children select the right child, including unequal matching prefixes.
            if (LessByCommonPrefix(heap[child].Span, heap[child - 1].Span))
                child--;
            heap[hole] = heap[child].ToArray();
            hole = child;
            right = 2L * hole + 2;
        }
        if (right == heap.Length)
        {
            heap[hole] = heap[(int)right - 1].ToArray();
            hole = (int)right - 1;
        }
        PushOwnedSavedValue(heap, hole, top, savedValue);
    }

    private static void PushOwnedSavedValue(Span<ReadOnlyMemory<byte>> heap, int hole, int top,
        ReadOnlySpan<byte> savedValue)
    {
        while (top < hole)
        {
            int parent = (hole - 1) / 2;
            if (!LessByCommonPrefix(heap[parent].Span, savedValue))
                break;
            heap[hole] = heap[parent].ToArray();
            hole = parent;
        }
        heap[hole] = savedValue.ToArray();
    }

    private static void ValidateHole(int length, int hole, int top)
    {
        if (hole < 0 || hole >= length)
            throw new ArgumentOutOfRangeException(nameof(hole), "Hole must be inside the supplied heap.");
        if (top < 0 || top > hole)
            throw new ArgumentOutOfRangeException(nameof(top), "Top must be between zero and the hole.");
    }
}
