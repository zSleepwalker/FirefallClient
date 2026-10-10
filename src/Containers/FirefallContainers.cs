using System.Runtime.InteropServices;

namespace Firefall.Containers;

public interface IAllocator32
{
    int AllocatorPointer { get; }
    int AllocateArray(ref int requestedBytes);
    int ReallocateArray(int dataPointer, int oldBytes, ref int requestedBytes);
    int AllocateRaw(int bytes);
    int FreeRaw(int basePointer, int bytes);
    int ReadInt32(int address);
    void WriteInt32(int address, int value);
    void WriteByte(int address, byte value);
    void CopyBytes(int destination, int source, int count);
    void CopyCString(int destination, int source);
    int CStringLength(int source);
}

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
public struct DynArray32
{
    public int DataPointer;
    public int Count;
    public int TaggedCapacity;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public struct DynString32
{
    public int TaggedPointer;
}

public static class FirefallContainers
{
    public const int CapacityMask = 0x3fffffff;
    public const int AllocationHeaderSize = 16;

    public static int EnsureCapacity(
        IAllocator32 allocator,
        ref DynArray32 array,
        int desiredCount,
        int elementSize)
    {
        int requestedBytes = unchecked(desiredCount * elementSize);

        if (array.TaggedCapacity < 0)
        {
            int newPointer = allocator.AllocateArray(ref requestedBytes);
            allocator.CopyBytes(
                newPointer,
                array.DataPointer,
                unchecked(array.Count * elementSize));
            array.DataPointer = newPointer;
        }
        else
        {
            int oldBytes = unchecked(
                elementSize * (array.TaggedCapacity & CapacityMask));
            array.DataPointer = allocator.ReallocateArray(
                array.DataPointer,
                oldBytes,
                ref requestedBytes);
        }

        int capacity = requestedBytes / elementSize;
        array.TaggedCapacity = capacity;
        return capacity;
    }

    public static int GrowWithPolicy(
        IAllocator32 allocator,
        ref DynArray32 array,
        int elementSize)
    {
        int targetCount = array.Count == 0
            ? 1
            : unchecked(array.Count * 2);
        int requestedBytes = unchecked(targetCount * elementSize);

        if (array.TaggedCapacity < 0)
        {
            int newPointer = allocator.AllocateArray(ref requestedBytes);
            allocator.CopyBytes(
                newPointer,
                array.DataPointer,
                unchecked(array.Count * elementSize));
            array.DataPointer = newPointer;
        }
        else
        {
            int oldBytes = unchecked(
                elementSize * (array.TaggedCapacity & CapacityMask));
            array.DataPointer = allocator.ReallocateArray(
                array.DataPointer,
                oldBytes,
                ref requestedBytes);
        }

        int capacity = requestedBytes / elementSize;
        array.TaggedCapacity = capacity;
        return capacity;
    }

    public static int ReserveAtLeast(
        IAllocator32 allocator,
        ref DynArray32 array,
        int minimumCount)
    {
        int capacity = array.TaggedCapacity & CapacityMask;
        if (capacity >= minimumCount)
            return capacity;

        int targetCount = unchecked(capacity * 2);
        if (minimumCount >= targetCount)
            targetCount = minimumCount;

        return EnsureCapacity(allocator, ref array, targetCount, 1);
    }

    public static int AllocateTrackedPayload(IAllocator32 allocator, int payloadBytes)
    {
        int totalBytes = unchecked(payloadBytes + AllocationHeaderSize);
        int basePointer = allocator.AllocateRaw(totalBytes);
        allocator.WriteInt32(basePointer, allocator.AllocatorPointer);
        allocator.WriteInt32(unchecked(basePointer + 4), payloadBytes);
        return unchecked(basePointer + AllocationHeaderSize);
    }

    public static int FreeTrackedPayload(IAllocator32 allocator, int dataPointer)
    {
        if (dataPointer == 0)
            return dataPointer;

        int basePointer = unchecked(dataPointer - AllocationHeaderSize);
        int totalBytes = unchecked(
            allocator.ReadInt32(unchecked(basePointer + 4)) + AllocationHeaderSize);
        allocator.WriteInt32(basePointer, 0);
        return allocator.FreeRaw(basePointer, totalBytes);
    }

    public static int DuplicateCString(IAllocator32 allocator, int sourcePointer)
    {
        int payloadBytes = unchecked(allocator.CStringLength(sourcePointer) + 1);
        int destinationPointer = AllocateTrackedPayload(allocator, payloadBytes);
        allocator.CopyCString(destinationPointer, sourcePointer);
        return destinationPointer;
    }

    public static int DuplicateCStringPrefix(
        IAllocator32 allocator,
        int maximumLength,
        int sourcePointer)
    {
        int length = allocator.CStringLength(sourcePointer);
        if (length > maximumLength)
            length = maximumLength;

        int destinationPointer = AllocateTrackedPayload(
            allocator,
            unchecked(length + 1));
        allocator.CopyBytes(destinationPointer, sourcePointer, length);
        allocator.WriteByte(unchecked(destinationPointer + length), 0);
        return destinationPointer;
    }

    public static int AssignString(
        IAllocator32 allocator,
        int maximumLength,
        int sourcePointer,
        ref DynString32 destination)
    {
        int oldTaggedPointer = destination.TaggedPointer;
        int result = oldTaggedPointer;

        if ((oldTaggedPointer & unchecked((int)0xfffffffe)) == sourcePointer)
            return result;

        if ((oldTaggedPointer & 1) != 0)
            result = FreeTrackedPayload(allocator, unchecked(oldTaggedPointer - 1));

        if (sourcePointer == 0)
        {
            destination.TaggedPointer = 0;
            return result;
        }

        int dataPointer = maximumLength == -1
            ? DuplicateCString(allocator, sourcePointer)
            : DuplicateCStringPrefix(allocator, maximumLength, sourcePointer);
        result = unchecked(dataPointer + 1);
        destination.TaggedPointer = result;
        return result;
    }

    public static int ClearString(IAllocator32 allocator, ref DynString32 value)
    {
        return AssignString(allocator, -1, 0, ref value);
    }
}
