namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeProfilerSample(
    ushort DivisorAt0, uint ScaledAt4, uint ScaledAt8, uint ScaledAt0C,
    ushort RawWordAt10, ushort RawWordAt12, uint ScaledAt14, uint ScaledAt18);

public enum NativeProfilerMetricOutcome
{
    Accepted,
    InvalidWordCount,
    UnsupportedNonFiniteHalf
}

public sealed record NativeProfilerMetricResult(
    NativeProfilerMetricOutcome Outcome, NativeProfilerSample? Sample, int? UnsupportedWordIndex);

public readonly record struct NativeProfilerNodeSamples(NativeProfilerSample Inclusive, NativeProfilerSample Exclusive);

/// <summary>Finite-half metric expansion and numeric sample projections; metric units and native UI objects are not modeled.</summary>
public static class NativeProfilerSamples
{
    public const uint NodeDecoderFileOffset = 0x1303020;
    public const uint HalfLookupVirtualAddress = 0x1C165B8;
    public const uint ScaleLiteralVirtualAddress = 0x1C821D4;
    public const int NativeSampleSize = 28;

    public static bool TryExpandFiniteHalf(ushort word, out uint value)
    {
        float decoded = (float)BitConverter.UInt16BitsToHalf(word);
        if (!float.IsFinite(decoded))
        {
            value = 0;
            return false;
        }
        float scaled = decoded * 65536.0f;
        value = unchecked((uint)(long)scaled);
        return true;
    }

    public static NativeProfilerMetricResult Decode(IReadOnlyList<ushort> wordsAt10To1E)
    {
        ArgumentNullException.ThrowIfNull(wordsAt10To1E);
        if (wordsAt10To1E.Count != 8)
            return new(NativeProfilerMetricOutcome.InvalidWordCount, null, null);
        Span<uint> values = stackalloc uint[5];
        ReadOnlySpan<int> indices = [1, 2, 3, 6, 7];
        for (int i = 0; i < indices.Length; i++)
            if (!TryExpandFiniteHalf(wordsAt10To1E[indices[i]], out values[i]))
                return new(NativeProfilerMetricOutcome.UnsupportedNonFiniteHalf, null, indices[i]);
        return new(NativeProfilerMetricOutcome.Accepted,
            new NativeProfilerSample(wordsAt10To1E[0], values[0], values[1], values[2],
                wordsAt10To1E[4], wordsAt10To1E[5], values[3], values[4]), null);
    }

    public static NativeProfilerNodeSamples ProjectNode(
        NativeProfilerSample decoded, IReadOnlyList<NativeProfilerSample> processedChildren)
    {
        ArgumentNullException.ThrowIfNull(processedChildren);
        uint exclusive = decoded.ScaledAt4;
        ushort word10 = decoded.RawWordAt10, word12 = decoded.RawWordAt12;
        uint scaled14 = decoded.ScaledAt14, scaled18 = decoded.ScaledAt18;
        foreach (var child in processedChildren)
        {
            exclusive = child.ScaledAt4 <= exclusive ? exclusive - child.ScaledAt4 : 0;
            word10 = unchecked((ushort)(word10 + child.RawWordAt10));
            word12 = unchecked((ushort)(word12 + child.RawWordAt12));
            scaled14 = unchecked(scaled14 + child.ScaledAt14);
            scaled18 = unchecked(scaled18 + child.ScaledAt18);
        }
        uint average = decoded.DivisorAt0 != 0 ? exclusive / decoded.DivisorAt0 : exclusive;
        return new(decoded with { RawWordAt10 = word10, RawWordAt12 = word12, ScaledAt14 = scaled14, ScaledAt18 = scaled18 },
            decoded with { ScaledAt4 = exclusive, ScaledAt8 = average, ScaledAt0C = average });
    }

    public static NativeProfilerSample AggregateRoot(IReadOnlyList<NativeProfilerSample> processedChildren)
    {
        ArgumentNullException.ThrowIfNull(processedChildren);
        uint total = 0, scaled14 = 0, scaled18 = 0;
        ushort word10 = 0, word12 = 0;
        foreach (var child in processedChildren)
        {
            total = unchecked(total + child.ScaledAt4);
            word10 = unchecked((ushort)(word10 + child.RawWordAt10));
            word12 = unchecked((ushort)(word12 + child.RawWordAt12));
            scaled14 = unchecked(scaled14 + child.ScaledAt14);
            scaled18 = unchecked(scaled18 + child.ScaledAt18);
        }
        return new(0, total, total, total, word10, word12, scaled14, scaled18);
    }
}

/// <summary>Native history fills all80 slots from a template; Latest also returns the template before the first append.</summary>
public sealed class NativeProfilerHistory80
{
    public const int SlotCount = 80;
    public const uint InitializerFileOffset = 0xE8E090;
    public const int NativeFirstSampleOffset = 0x14;
    public const int NativeNextSlotOffset = 0x8D4;

    private readonly NativeProfilerSample[] _samples = new NativeProfilerSample[SlotCount];
    public int NextSlot { get; private set; }
    public NativeProfilerSample Latest => _samples[(NextSlot + SlotCount - 1) % SlotCount];

    public NativeProfilerHistory80(NativeProfilerSample template) => Array.Fill(_samples, template);

    public NativeProfilerSample GetPhysicalSlot(int slot) => _samples[slot];

    public void Append(NativeProfilerSample sample)
    {
        _samples[NextSlot] = sample;
        NextSlot = (NextSlot + 1) % SlotCount;
    }
}
