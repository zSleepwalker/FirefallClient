using Firefall.Reconstruction.Startup;

internal static class MatrixServerProfilerNamesChecks
{
    public static void Run(Action<string, bool> check)
    {
        var empty = MatrixServerProfilerNames60NativeLayout.Empty(1234);
        byte[] source = "first"u8.ToArray();
        var initial = new MatrixWordTextListsPayload(
            [new(1, source), new(257, "high"u8.ToArray())], [new(1, "second"u8.ToArray())]);
        var applied = MatrixServerProfilerNames60NativeLayout.Apply(true, empty, initial, 7000);
        check("Profiler names60 keeps full WORD keys and independent first/second tables",
            applied.Outcome == MatrixProfilerNamesOutcome.Applied
            && applied.State.FirstNames.Count == 2 && applied.State.SecondNames.Count == 1
            && applied.State.FirstNames[1].Span.SequenceEqual("first"u8)
            && applied.State.FirstNames[257].Span.SequenceEqual("high"u8)
            && applied.State.SecondNames[1].Span.SequenceEqual("second"u8)
            && applied.State.TickMarkerAt18C == 2000);
        var next = MatrixServerProfilerNames60NativeLayout.Apply(true, applied.State,
            new([new(1, "old"u8.ToArray()), new(1, ReadOnlyMemory<byte>.Empty), new(65535, "max"u8.ToArray())], []), 100);
        check("Profiler names60 last duplicate wins, empty replaces text and unmentioned entries persist",
            next.State.FirstNames[1].IsEmpty && next.State.FirstNames.Count == 3
            && next.State.FirstNames[257].Span.SequenceEqual("high"u8)
            && next.State.FirstNames[65535].Span.SequenceEqual("max"u8)
            && next.State.SecondNames[1].Span.SequenceEqual("second"u8)
            && next.State.TickMarkerAt18C == unchecked(100U - 5000));
        source[0] = 0;
        check("Profiler names60 projection copies source bytes and does not mutate earlier sampled states",
            empty.FirstNames.Count == 0 && empty.TickMarkerAt18C == 1234
            && applied.State.FirstNames[1].Span.SequenceEqual("first"u8));
        var unchanged = MatrixServerProfilerNames60NativeLayout.Apply(false, next.State, initial, 5000);
        var emptyUpdate = MatrixServerProfilerNames60NativeLayout.Apply(true, next.State, new([], []), 5000);
        check("Profiler names60 missing consumer preserves state; empty lists preserve tables but still update tick",
            unchanged.Outcome == MatrixProfilerNamesOutcome.ConsumerAbsent && ReferenceEquals(unchanged.State, next.State)
            && emptyUpdate.State.FirstNames.Count == 3 && emptyUpdate.State.SecondNames.Count == 1
            && emptyUpdate.State.TickMarkerAt18C == 0);
        var wire = MatrixServerMessages56To61Wire.Decode(
            new byte[] { 60, 1, 0x34, 0x12, (byte)'x', 0, 1, 0x34, 0x12, (byte)'y', 0 });
        bool decodedApplied = false;
        if (wire is { BytesConsumed: 11, Payload: MatrixWordTextListsPayload decoded })
        {
            var projected = MatrixServerProfilerNames60NativeLayout.Apply(true, empty, decoded, 9000);
            decodedApplied = projected.State.FirstNames[0x1234].Span.SequenceEqual("x"u8)
                && projected.State.SecondNames[0x1234].Span.SequenceEqual("y"u8)
                && projected.State.TickMarkerAt18C == 4000;
        }
        check("Profiler names60 decoded wire lists feed independent persistent state without losing high key byte",
            decodedApplied);
    }
}
