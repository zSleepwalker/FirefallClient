namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Runtime field descriptor at raw 0x1310600, with its compared words at
/// offsets +4, +12 and +16. The null-bit index is read by the row converter
/// at offset +8 but is not part of the fast-path layout comparison.
/// </summary>
public readonly record struct SdbRuntimeFieldLayout(
    uint NameHash,
    byte NullBitIndex,
    uint ValueOffset,
    uint TypeId);

/// <summary>
/// Runtime table fields used by the identical-layout fast path. Descriptor
/// count is at +20 and the array pointer at +24 in the native object.
/// </summary>
public sealed record SdbRuntimeTableLayout(
    uint NameHash,
    uint RowStride,
    IReadOnlyList<SdbRuntimeFieldLayout> Fields,
    uint NullBitmapOffset = 0)
{
    public static SdbRuntimeTableLayout FromSdbTable(SdbTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(table.Fields);

        var fields = new SdbRuntimeFieldLayout[table.Fields.Count];
        for (int i = 0; i < fields.Length; i++)
        {
            var field = table.Fields[i];
            fields[i] = new SdbRuntimeFieldLayout(
                field.NameHash, field.NullBit, field.ValueOffset,
                field.Type < 27 ? field.Type : 0u);
        }

        return new SdbRuntimeTableLayout(table.NameHash, table.RowStride, fields, table.NullBitmapOffset);
    }

    public static int[] MapSourceFieldsByHash(SdbRuntimeTableLayout source, SdbRuntimeTableLayout destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(source.Fields);
        ArgumentNullException.ThrowIfNull(destination.Fields);

        var indexes = new int[destination.Fields.Count];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = -1;
            for (int j = 0; j < source.Fields.Count; j++)
            {
                if (source.Fields[j].NameHash != destination.Fields[i].NameHash)
                    continue;
                indexes[i] = j;
                break;
            }
        }

        return indexes;
    }

    public static bool IsSourceFieldNull(ReadOnlySpan<byte> row, uint nullBitmapOffset, byte nullBitIndex)
    {
        int byteOffset = NullByteOffset(row.Length, nullBitmapOffset, nullBitIndex);
        if (byteOffset < 0)
            return false;
        return (row[byteOffset] & (1 << (nullBitIndex & 7))) != 0;
    }

    public static void MarkDestinationFieldNull(Span<byte> row, uint nullBitmapOffset, byte nullBitIndex)
    {
        int byteOffset = NullByteOffset(row.Length, nullBitmapOffset, nullBitIndex);
        if (byteOffset >= 0)
            row[byteOffset] |= (byte)(1 << (nullBitIndex & 7));
    }

    private static int NullByteOffset(int rowLength, uint nullBitmapOffset, byte nullBitIndex)
    {
        if (nullBitIndex == 0xFF)
            return -1;
        ulong byteOffset = (ulong)nullBitmapOffset + (uint)(nullBitIndex >> 3);
        if (byteOffset >= (ulong)rowLength)
            throw new ArgumentException("The SDB row's null bitmap is truncated.");
        return (int)byteOffset;
    }

    public static bool HasIdenticalStorageLayout(SdbRuntimeTableLayout left, SdbRuntimeTableLayout right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(left.Fields);
        ArgumentNullException.ThrowIfNull(right.Fields);

        if (left.NameHash != right.NameHash
            || left.RowStride != right.RowStride
            || left.Fields.Count != right.Fields.Count)
            return false;

        for (int i = 0; i < left.Fields.Count; i++)
        {
            var source = left.Fields[i];
            var target = right.Fields[i];
            if (source.NameHash != target.NameHash
                || source.TypeId != target.TypeId
                || source.ValueOffset != target.ValueOffset)
                return false;
        }

        return true;
    }
}
