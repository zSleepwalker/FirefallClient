namespace Firefall.Reconstruction.Startup;

public sealed record MatrixEnterZoneMetadataEntry<T>(ReadOnlyMemory<byte> KeyBytes, T ValueBits)
    where T : unmanaged;

public sealed record MatrixEnterZoneMetadata(
    IReadOnlyList<MatrixEnterZoneMetadataEntry<ushort>> Word16Entries,
    IReadOnlyList<MatrixEnterZoneMetadataEntry<uint>> Word32Entries);

/// <summary>
/// Two chunk-counted, byte-keyed maps. Value widths are proven; signedness and
/// semantic types are not. Entries are sorted by unsigned key bytes, last value wins.
/// </summary>
internal static class MatrixEnterZoneMetadataWire
{
    private delegate bool ReadValue<T>(ref MatrixMessageSpanReader reader, out T value)
        where T : unmanaged;

    private static readonly IComparer<ReadOnlyMemory<byte>> KeyComparer =
        Comparer<ReadOnlyMemory<byte>>.Create((left, right) => left.Span.SequenceCompareTo(right.Span));

    public static bool TryRead(ref MatrixMessageSpanReader reader, out MatrixEnterZoneMetadata? metadata)
    {
        metadata = null;
        if (!TryReadMap<ushort>(ref reader, ReadWord16, out var word16Entries)
            || !TryReadMap<uint>(ref reader, ReadWord32, out var word32Entries))
            return false;
        metadata = new(word16Entries, word32Entries);
        return true;
    }

    private static bool ReadWord16(ref MatrixMessageSpanReader reader, out ushort value) =>
        reader.TryReadUInt16(out value);

    private static bool ReadWord32(ref MatrixMessageSpanReader reader, out uint value) =>
        reader.TryReadUInt32(out value);

    private static bool TryReadMap<T>(ref MatrixMessageSpanReader reader, ReadValue<T> readValue,
        out IReadOnlyList<MatrixEnterZoneMetadataEntry<T>> entries) where T : unmanaged
    {
        entries = Array.Empty<MatrixEnterZoneMetadataEntry<T>>();
        var map = new SortedDictionary<ReadOnlyMemory<byte>, T>(KeyComparer);
        byte chunkCount;
        do
        {
            if (!reader.TryReadByte(out chunkCount))
                return false;
            for (int i = 0; i < chunkCount; i++)
            {
                if (!reader.TryReadNullTerminatedBytes(out var key)
                    || !readValue(ref reader, out var value))
                    return false;
                map[key] = value;
            }
        }
        while (chunkCount == byte.MaxValue);

        entries = Array.AsReadOnly(map.Select(pair =>
            new MatrixEnterZoneMetadataEntry<T>(pair.Key, pair.Value)).ToArray());
        return true;
    }
}
