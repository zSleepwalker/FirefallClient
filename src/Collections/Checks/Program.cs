using Firefall.Collections;

static void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

var allocator = new RecordingAllocator();
var table = new HashSlotTable32();
Check(table.Mask == -1 && table.FindIndex(0x10) == 0, "Unallocated lookup sentinel");
Check(table.GetOrDefault(0x10, 123) == 123, "Unallocated default");

for (uint key = 0x10; key < 0x14; key++)
    Check(table.InsertOrUpdate(allocator, key, key + 100), "Initial insertion");
Check(table.Count == 4 && table.SlotCount == 8, "First allocation and count");
Check(allocator.Releases.Count == 1 && allocator.Releases[0] == 0, "Initial null/zero release");
Check(!table.InsertOrUpdate(allocator, 0x10, 999), "Overwrite on duplicate");
Check(table.SlotCount == 16 && table.Count == 4, "Growth before duplicate check");
Check(table.GetOrDefault(0x10, 0) == 999, "Overwrite payload");
Check(table.FindIndex(0x11) < table.SlotCount, "Collision probe");
Check(table.GetOrDefault(0x24, 77) == 77, "Miss fallback");
Check(table.InsertIfAbsent(allocator, 0x10, 111) == table.FindIndex(0x10), "Existing index");
Check(table.GetOrDefault(0x10, 0) == 999, "Insert-if-absent leaves old payload");
Check(table.InsertIfAbsent(allocator, 0x25, 222) == table.FindIndex(0x25), "New index");
Check(table.Count == 5, "Insert-if-absent count");
table.ReserveForEntries(allocator, 9);
Check(table.SlotCount == 32 && table.Count == 5, "Power-of-two reserve and reinsertion");
for (uint key = 0x10; key < 0x14; key++)
    Check(table.GetOrDefault(key, 0) == (key == 0x10 ? 999 : key + 100), "Rehash preserves entries");
Check(allocator.Releases.SequenceEqual(new[] { 0, 64, 128 }), "Rehash release sizes");
table.Clear();
Check(table.Count == 0 && table.SlotCount == 32 && table.FindIndex(0x10) == 32, "Clear retains capacity");
table.ResetAndReleaseStorage(allocator);
Check(table.Count == 0 && table.Mask == -1 && allocator.Releases[^1] == 256, "Owned reset");

var borrowed = new HashSlot32[8];
for (int i = 0; i < borrowed.Length; i++)
    borrowed[i].Key = HashSlotTable32.EmptyKey;
borrowed[0] = new HashSlot32 { Key = 0x10, Value = 444 };
var borrowedTable = new HashSlotTable32(borrowed, 1);
int releaseCount = allocator.Releases.Count;
borrowedTable.Clear();
Check(borrowedTable.IsBorrowed && borrowed[0].Key == HashSlotTable32.EmptyKey &&
      borrowed[0].Value == 444, "Clear preserves high bit and payload");
borrowedTable.InsertOrUpdate(allocator, 0x20, 555);
borrowedTable.RebuildAndReinsert(allocator, 16);
Check(!borrowedTable.IsBorrowed && allocator.Releases.Count == releaseCount,
    "Borrowed storage not released on rebuild");
borrowedTable.ResetAndReleaseStorage(allocator);
Check(allocator.Releases.Count == releaseCount + 1 && allocator.Releases[^1] == 128,
    "Newly allocated storage is owned");

var borrowedReset = new HashSlotTable32(borrowed, 0);
releaseCount = allocator.Releases.Count;
borrowedReset.ResetAndReleaseStorage(allocator);
Check(allocator.Releases.Count == releaseCount && borrowedReset.Mask == -1, "Borrowed reset");

var populatedBorrowed = new HashSlot32[8];
for (int i = 0; i < populatedBorrowed.Length; i++)
    populatedBorrowed[i].Key = HashSlotTable32.EmptyKey;
var populated = new HashSlotTable32(populatedBorrowed, 0);
for (uint key = 0x30; key < 0x34; key++)
    populated.InsertOrUpdate(allocator, key, key + 1);
releaseCount = allocator.Releases.Count;
populated.InsertOrUpdate(allocator, 0x34, 0x99);
Check(populated.SlotCount == 16 && populated.Count == 5 &&
      !populated.IsBorrowed && allocator.Releases.Count == releaseCount,
    "Growth masks the borrowed count and transfers ownership");
for (uint key = 0x30; key < 0x34; key++)
    Check(populated.GetOrDefault(key, 0) == key + 1, "Borrowed growth reinsertion");
populated.ResetAndReleaseStorage(allocator);
Console.WriteLine("Hash-slot table checks passed.");

sealed class RecordingAllocator : IHashSlotAllocator32
{
    public List<int> Releases { get; } = [];
    public HashSlot32[] AllocateSlots(int byteCount) => new HashSlot32[byteCount / 8];
    public void ReleaseSlots(HashSlot32[]? slots, int byteCount) => Releases.Add(byteCount);
}
