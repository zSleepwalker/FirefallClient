namespace Firefall.Reconstruction.Shared;

internal static class EngineByteHash32
{
    public static uint Compute(ReadOnlySpan<byte> bytes)
    {
        uint hash = ComputeFnv1a(bytes);
        hash = unchecked(hash * 8193);
        hash ^= hash >> 7;
        hash = unchecked(hash * 9);
        hash ^= hash >> 17;
        return unchecked(hash * 33);
    }

    public static uint ComputeFnv1a(ReadOnlySpan<byte> bytes)
    {
        uint hash = 2166136261;
        foreach (byte value in bytes)
            hash = unchecked((hash ^ value) * 16777619);
        return hash;
    }
}
