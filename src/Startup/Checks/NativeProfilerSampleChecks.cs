using Firefall.Reconstruction.Startup;

internal static class NativeProfilerSampleChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool golden = true;
        foreach (var (word, expected) in new (ushort, uint)[]
        {
            (0, 0), (1, 0), (0x100, 1), (0x400, 4), (0x3C00, 65536),
            (0x7BFF, 0xFFE00000), (0x8000, 0), (0xBC00, 0xFFFF0000), (0xFBFF, 0x00200000)
        })
            golden &= NativeProfilerSamples.TryExpandFiniteHalf(word, out uint value) && value == expected;
        check("Profiler finite binary16 values scale65536 then truncate signed64 and retain low32, including negative wrap", golden);
        var decoded = NativeProfilerSamples.Decode(new ushort[] { 2, 0x3C00, 0x4000, 0x4200, 4, 5, 0x4400, 0x4500 });
        check("Profiler sample maps all eight WORDs to correct raw/scaled28-byte fields",
            decoded is { Outcome: NativeProfilerMetricOutcome.Accepted,
                Sample: { DivisorAt0: 2, ScaledAt4: 65536, ScaledAt8: 131072, ScaledAt0C: 196608,
                    RawWordAt10: 4, RawWordAt12: 5, ScaledAt14: 262144, ScaledAt18: 327680 } });
        check("Profiler malformed metric count and nonfinite half values are explicit unsupported outcomes",
            NativeProfilerSamples.Decode(new ushort[7]) is { Outcome: NativeProfilerMetricOutcome.InvalidWordCount, Sample: null }
            && NativeProfilerSamples.Decode(new ushort[] { 0, 0, 0, 0, 0, 0, 0x7C00, 0 })
                is { Outcome: NativeProfilerMetricOutcome.UnsupportedNonFiniteHalf, Sample: null, UnsupportedWordIndex: 6 }
            && !NativeProfilerSamples.TryExpandFiniteHalf(0x7E00, out _));
        var own = new NativeProfilerSample(3, 100, 10, 200, 65535, 3, uint.MaxValue, 9);
        var child = new NativeProfilerSample(1, 40, 2, 3, 2, 4, 3, uint.MaxValue);
        var projected = NativeProfilerSamples.ProjectNode(own, [child]);
        check("Profiler node excludes child scaled4 with saturation, averages by divisor and aggregates late fields with wrap",
            projected.Inclusive == own with { RawWordAt10 = 1, RawWordAt12 = 7, ScaledAt14 = 2, ScaledAt18 = 8 }
            && projected.Exclusive == own with { ScaledAt4 = 60, ScaledAt8 = 20, ScaledAt0C = 20 }
            && NativeProfilerSamples.ProjectNode(own, [child with { ScaledAt4 = uint.MaxValue }, child])
                .Exclusive.ScaledAt4 == 0);
        check("Profiler zero divisor avoids division and root sums preserve native unsigned wrapping",
            NativeProfilerSamples.ProjectNode(own with { DivisorAt0 = 0 }, [child]).Exclusive.ScaledAt8 == 60
            && NativeProfilerSamples.AggregateRoot([]) == default
            && NativeProfilerSamples.AggregateRoot([own, child])
                == new NativeProfilerSample(0, 140, 140, 140, 1, 7, 2, 8));
        var history = new NativeProfilerHistory80(own);
        bool historyMatches = history.NextSlot == 0 && history.Latest == own
            && Enumerable.Range(0, 80).All(i => history.GetPhysicalSlot(i) == own);
        for (uint i = 1; i <= 161; i++)
            history.Append(own with { ScaledAt4 = i });
        check("Profiler history starts with80 template samples and wraps after80/160 appends",
            historyMatches && history.NextSlot == 1 && history.Latest.ScaledAt4 == 161
            && history.GetPhysicalSlot(0).ScaledAt4 == 161 && history.GetPhysicalSlot(79).ScaledAt4 == 160);
    }
}
