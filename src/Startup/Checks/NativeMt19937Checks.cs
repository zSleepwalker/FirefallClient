using Firefall.Reconstruction.Startup;

internal static class NativeMt19937Checks
{
    public static void Run(Action<string, bool> check)
    {
        var canonical = new NativeMt19937(5489);
        uint[] first =
        [
            3499211612, 581869302, 3890346734, 3586334585, 545404204,
            4161255391, 3922919429, 949333985, 2715962298, 1323567403
        ];
        check("Native MT19937 matches the canonical seed5489 first ten outputs",
            first.All(expected => canonical.NextUInt32() == expected));
        foreach (uint seed in new uint[] { 0, 1, 5489, uint.MaxValue })
        {
            var generator = new NativeMt19937(seed);
            var reference = NativeSplitLoopSequence(seed, 2000);
            check($"Native MT19937 seed{seed} matches native split-loop projection across multiple624-word twists",
                reference.All(expected => generator.NextUInt32() == expected));
            generator.Reseed(seed);
            check($"Native MT19937 reseeding{seed} restarts the complete deterministic sequence",
                reference.All(expected => generator.NextUInt32() == expected));
        }
        var arrayCanonical = new NativeMt19937(new uint[] { 0x123, 0x234, 0x345, 0x456 });
        uint[] arrayFirst = [1067595299, 955945823, 477289528, 4107218783, 4228976476,
            3344332714, 3355579695, 227628506, 810200273, 2591290167];
        check("Native MT19937 array initialization matches canonical four-word seed vector",
            arrayFirst.All(expected => arrayCanonical.NextUInt32() == expected));
        int[] positions = [0, 1, 2, 623, 624, 1247, 1248, 1999];
        // Independent Python random.Random integer seeding uses these little-endian seed words via init_by_array.
        (int Count, uint[] Expected)[] goldens =
        [
            (1, [1561712898, 1396601857, 563176776, 2229829662, 1129355166, 804677776, 3541026647, 246070998]),
            (623, [1680255654, 535620588, 2093468309, 3540907689, 1774763594, 2984507545, 2586672434, 1526992477]),
            (624, [4136389835, 10543802, 300768514, 4140860119, 3795744337, 2130892483, 828699006, 3003269433]),
            (625, [1774280004, 2815230205, 3366784656, 90042752, 3496762975, 881000561, 2474878855, 3295731559]),
            (1000, [1590832226, 1873972030, 700858973, 451920439, 3276871954, 2671447546, 1539071846, 14812374])
        ];
        foreach (var golden in goldens)
        {
            var seeds = Enumerable.Range(1, golden.Count).Select(i => unchecked((uint)i * 2654435761U)).ToArray();
            var generator = new NativeMt19937(seeds);
            uint[] actual = new uint[positions.Length];
            int selected = 0;
            for (int i = 0; i < 2000; i++)
            {
                uint value = generator.NextUInt32();
                if (i == positions[selected])
                {
                    actual[selected++] = value;
                    if (selected == positions.Length)
                        break;
                }
            }
            check($"Native MT19937 array{golden.Count} matches independent standard-library goldens across multiple twists",
                actual.SequenceEqual(golden.Expected));
            generator.Reseed(seeds);
            seeds[0] ^= uint.MaxValue;
            check($"Native MT19937 array{golden.Count} reseed resets output and retains no caller seed-array dependency",
                generator.NextUInt32() == golden.Expected[0]);
        }
        bool emptyRejected = false;
        try { arrayCanonical.Reseed(ReadOnlySpan<uint>.Empty); }
        catch (ArgumentException) { emptyRejected = true; }
        check("Native MT19937 empty seed array is an explicit unsupported input rather than an invented default seed", emptyRejected);

        int draws = 0;
        uint Draw() { draws++; return uint.MaxValue; }
        var invalid = NativeRandomInclusive32.Draw(Draw, 5, 4);
        var full = NativeRandomInclusive32.Draw(Draw, 0, uint.MaxValue);
        var singleton = NativeRandomInclusive32.Draw(Draw, 7, 7);
        check("Native inclusive UInt32 range consumes one draw before invalid bounds/full-range faults and even singleton ranges",
            draws == 3 && invalid is { Outcome: NativeRandomInclusive32Outcome.InvalidBoundsException,
                Value: null, ConsumedSample: uint.MaxValue, NativeDiagnosticCode: 0x1C271921 }
            && full is { Outcome: NativeRandomInclusive32Outcome.WrappedDivisorZero, Value: null }
            && singleton is { Outcome: NativeRandomInclusive32Outcome.Accepted, Value: 7 });
        check("Native inclusive range uses unsigned modulo without rejection/bias correction or full-range shortcut",
            NativeRandomInclusive32.ProjectConsumedSample(5, 10, 12).Value == 12
            && NativeRandomInclusive32.ProjectConsumedSample(uint.MaxValue, 1, uint.MaxValue).Value == 1
            && NativeRandomInclusive32.ProjectConsumedSample(uint.MaxValue, uint.MaxValue - 1, uint.MaxValue).Value == uint.MaxValue);
        var consumed = new NativeMt19937(5489);
        consumed.NextUInt32Inclusive(9, 8);
        check("Native MT inclusive wrapper advances generator state before rejecting bounds",
            consumed.NextUInt32() == first[1]);
    }

    private static IEnumerable<uint> NativeSplitLoopSequence(uint seed, int count)
    {
        var state = new uint[624];
        state[0] = seed;
        for (uint i = 1; i < 624; i++)
            state[i] = unchecked(i + 1812433253U * (state[i - 1] ^ (state[i - 1] >> 30)));
        for (int produced = 0; produced < count; produced++)
        {
            if (produced % 624 == 0)
            {
                for (int i = 0; i < 227; i++)
                    state[i] = Combine(state[i], state[i + 1], state[i + 397]);
                for (int i = 227; i < 623; i++)
                    state[i] = Combine(state[i], state[i + 1], state[i - 227]);
                state[623] = Combine(state[623], state[0], state[396]);
            }
            uint value = state[produced % 624];
            value = (value >> 11) ^ value;
            value = ((value & 0xFF3A58AD) << 7) ^ value;
            value = ((value & 0xFFFFDF8C) << 15) ^ value;
            yield return value ^ (value >> 18);
        }
    }

    private static uint Combine(uint current, uint next, uint distant)
    {
        uint joined = current ^ ((current ^ next) & 0x7FFFFFFF);
        return distant ^ (joined >> 1) ^ ((joined & 1) != 0 ? 0x9908B0DFU : 0U);
    }
}
