namespace Firefall.Reconstruction.Startup;

/// <summary>Optional RNG-only advance after an emitted candidate; appends no bytes.</summary>
public static class NativeGeneratedCandidateRngAdvance
{
    public const uint MethodFileOffset = 0x1286780;

    public static uint Count(byte controlByteAt18, ReadOnlySpan<byte> lastCandidate)
    {
        if (controlByteAt18 != 0)
            return 0;
        uint sum = 0;
        foreach (byte value in lastCandidate)
            sum = unchecked(sum + value);
        return sum % 5;
    }

    public static uint Consume(byte controlByteAt18, ReadOnlySpan<byte> lastCandidate, Func<uint> nextUInt32)
    {
        ArgumentNullException.ThrowIfNull(nextUInt32);
        uint count = Count(controlByteAt18, lastCandidate);
        for (uint i = 0; i < count; i++)
            nextUInt32();
        return count;
    }
}
