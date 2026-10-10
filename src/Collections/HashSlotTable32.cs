using System.Runtime.InteropServices;

namespace Firefall.Collections;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
public struct HashSlot32
{
    public uint Key;
    public uint Value;
}

public interface IHashSlotAllocator32
{
    HashSlot32[] AllocateSlots(int byteCount);
    void ReleaseSlots(HashSlot32[]? slots, int byteCount);
}

public sealed class ManagedHashSlotAllocator32 : IHashSlotAllocator32
{
    public HashSlot32[] AllocateSlots(int byteCount) => new HashSlot32[byteCount / 8];
    public void ReleaseSlots(HashSlot32[]? slots, int byteCount) { }
}

/// <summary>
/// Behavioral model of the 32-bit open-addressed table, not a native pointer-compatible header.
/// The native header contains a slot pointer, a tagged count, and a slot-index mask.
/// </summary>
public sealed class HashSlotTable32
{
    public const uint EmptyKey = uint.MaxValue;
    private const uint BorrowedFlag = 0x80000000;
    private const uint CountMask = 0x7fffffff;
    private const uint HashMultiplier = 0x9e3779b1;

    private HashSlot32[]? _slots;
    private uint _taggedCount;
    private int _mask = -1;

    public int Count => (int)(_taggedCount & CountMask);
    public bool IsBorrowed => (_taggedCount & BorrowedFlag) != 0;
    public int Mask => _mask;
    public int SlotCount => _mask + 1;

    public HashSlotTable32() { }

    // Models an externally supplied native slot pointer whose high count bit forbids release.
    public HashSlotTable32(HashSlot32[] borrowedSlots, int count)
    {
        ArgumentNullException.ThrowIfNull(borrowedSlots);
        ValidateSlotCount(borrowedSlots.Length);
        if ((uint)count > (uint)borrowedSlots.Length)
            throw new ArgumentOutOfRangeException(nameof(count));
        _slots = borrowedSlots;
        _taggedCount = (uint)count | BorrowedFlag;
        _mask = borrowedSlots.Length - 1;
    }

    // 0x101f0: misses return mask+1, including zero for an unallocated table.
    public int FindIndex(uint key)
    {
        if (_mask <= 0)
            return _mask + 1;

        int index = InitialIndex(key);
        while (_slots![index].Key != EmptyKey)
        {
            if (_slots[index].Key == key)
                return index;
            index = (index + 1) & _mask;
        }
        return _mask + 1;
    }

    // 0x10230: a miss returns the caller's value, not a fixed zero.
    public uint GetOrDefault(uint key, uint fallback)
    {
        int index = FindIndex(key);
        return index == _mask + 1 ? fallback : _slots![index].Value;
    }

    // 0x3790: returns one for a new key, zero for an existing key.
    public bool InsertOrUpdate(IHashSlotAllocator32 allocator, uint key, uint value)
    {
        ArgumentNullException.ThrowIfNull(allocator);
        if (key == EmptyKey)
            throw new ArgumentOutOfRangeException(nameof(key), "0xffffffff is the empty-slot sentinel.");

        if (2L * Count > _mask)
            RebuildAndReinsert(allocator, checked(2 * (_mask + 1)));

        int index = InitialIndex(key);
        while (_slots![index].Key != EmptyKey)
        {
            if (_slots[index].Key == key)
            {
                _slots[index].Value = value;
                return false;
            }
            index = (index + 1) & _mask;
        }

        _taggedCount++;
        _slots[index].Key = key;
        _slots[index].Value = value;
        return true;
    }

    // 0x2f540: unlike 0x3790, a hit leaves the old value unchanged and returns its index.
    public int InsertIfAbsent(IHashSlotAllocator32 allocator, uint key, uint value)
    {
        ArgumentNullException.ThrowIfNull(allocator);
        if (key == EmptyKey)
            throw new ArgumentOutOfRangeException(nameof(key));

        if (2L * Count > _mask)
            RebuildAndReinsert(allocator, checked(2 * (_mask + 1)));

        int index = InitialIndex(key);
        while (_slots![index].Key != EmptyKey)
        {
            if (_slots[index].Key == key)
                return index;
            index = (index + 1) & _mask;
        }

        _slots[index].Key = key;
        _slots[index].Value = value;
        _taggedCount++;
        return index;
    }

    // 0x36a0: only keys are cleared; payloads remain physically untouched.
    public void Clear()
    {
        if (_slots is not null)
            for (int i = 0; i <= _mask; i++)
                _slots[i].Key = EmptyKey;
        _taggedCount &= BorrowedFlag;
    }

    // 0x3860: selects the first power of two >= max(8, 2 * requestedEntries).
    public void ReserveForEntries(IHashSlotAllocator32 allocator, int requestedEntries)
    {
        if (requestedEntries < 0)
            throw new ArgumentOutOfRangeException(nameof(requestedEntries));
        int slots = 8;
        while (slots < 2L * requestedEntries)
            slots = checked(slots * 2);
        RebuildAndReinsert(allocator, slots);
    }

    // 0x36d0: allocate first, reinsert occupied old slots, then conditionally release old storage.
    public void RebuildAndReinsert(IHashSlotAllocator32 allocator, int requestedSlots)
    {
        ArgumentNullException.ThrowIfNull(allocator);
        int newCount = Math.Max(8, requestedSlots);
        ValidateSlotCount(newCount);
        HashSlot32[]? oldSlots = _slots;
        int oldSlotCount = _mask + 1;
        bool oldWasBorrowed = IsBorrowed;

        HashSlot32[] newSlots = allocator.AllocateSlots(checked(newCount * 8));
        if (newSlots.Length != newCount)
            throw new InvalidOperationException("The allocator returned a different slot count.");
        for (int i = 0; i < newCount; i++)
            newSlots[i].Key = EmptyKey;

        _slots = newSlots;
        _mask = newCount - 1;
        _taggedCount = 0;
        for (int i = 0; i < oldSlotCount; i++)
            if (oldSlots![i].Key != EmptyKey)
                InsertOrUpdate(allocator, oldSlots[i].Key, oldSlots[i].Value);

        if (!oldWasBorrowed)
            allocator.ReleaseSlots(oldSlots, checked(oldSlotCount * 8));
    }

    // 0x3810: clear keys before releasing, and reset the header even for borrowed storage.
    public void ResetAndReleaseStorage(IHashSlotAllocator32 allocator)
    {
        ArgumentNullException.ThrowIfNull(allocator);
        Clear();
        if (!IsBorrowed)
            allocator.ReleaseSlots(_slots, checked((_mask + 1) * 8));
        _slots = null;
        _taggedCount = 0;
        _mask = -1;
    }

    private int InitialIndex(uint key) =>
        (int)(unchecked((key >> 4) * HashMultiplier) & (uint)_mask);

    private static void ValidateSlotCount(int slots)
    {
        if (slots < 8 || (slots & (slots - 1)) != 0 || slots > int.MaxValue / 8)
            throw new ArgumentOutOfRangeException(nameof(slots), "Slot count must be a supported power of two >= 8.");
    }
}
