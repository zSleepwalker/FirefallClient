namespace Firefall.Reconstruction.Startup;

internal static class MatrixChunkedListWire
{
    internal delegate bool ReadEntry<T>(ref MatrixMessageSpanReader reader, out T entry) where T : struct;

    public static bool TryRead<T>(ref MatrixMessageSpanReader reader, ReadEntry<T> readEntry,
        out IReadOnlyList<T> entries) where T : struct
    {
        entries = Array.Empty<T>();
        var list = new List<T>();
        byte count;
        do
        {
            if (!reader.TryReadByte(out count))
                return false;
            for (int i = 0; i < count; i++)
            {
                if (!readEntry(ref reader, out var entry))
                    return false;
                list.Add(entry);
            }
        }
        while (count == byte.MaxValue);
        entries = list.AsReadOnly();
        return true;
    }
}
