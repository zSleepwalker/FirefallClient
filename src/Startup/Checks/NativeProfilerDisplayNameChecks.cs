using Firefall.Reconstruction.Startup;

internal static class NativeProfilerDisplayNameChecks
{
    public static void Run(Action<string, bool> check)
    {
        var names = MatrixServerProfilerNames60NativeLayout.Apply(true,
            MatrixServerProfilerNames60NativeLayout.Empty(0),
            new([new(1, "root-low"u8.ToArray()), new(0x101, "root-high"u8.ToArray()), new(65535, ReadOnlyMemory<byte>.Empty)],
                [new(1, "node"u8.ToArray()), new(65535, ReadOnlyMemory<byte>.Empty)]), 0).State;
        check("Profiler display names select distinct root/node maps by complete WORD key",
            NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Root, 0x101, names)
                .LogicalTextBytes.Span.SequenceEqual("root-high"u8)
            && NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 1, names)
                .LogicalTextBytes.Span.SequenceEqual("node"u8));
        check("Profiler known-empty names remain empty rather than using the missing-key placeholder",
            NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Root, 65535, names)
                is { Source: NativeProfilerDisplayNameSource.RootNameTable, LogicalTextBytes.IsEmpty: true }
            && NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 65535, names)
                is { Source: NativeProfilerDisplayNameSource.NodeNameTable, LogicalTextBytes.IsEmpty: true });
        var waiting = NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 2, names);
        var exclusive = NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Exclusive, 1, names);
        var error = NativeProfilerDisplayNames.Resolve((NativeProfilerTreeKind)0, 65535, names);
        check("Profiler exact native exclusive, waiting and error labels are separate outcomes",
            waiting.Source == NativeProfilerDisplayNameSource.WaitingForServer
            && waiting.LogicalTextBytes.Span.SequenceEqual("<Waiting For Server>"u8)
            && exclusive.Source == NativeProfilerDisplayNameSource.ExclusiveLabel
            && exclusive.LogicalTextBytes.Span.SequenceEqual("[Exclusive]"u8)
            && error.Source == NativeProfilerDisplayNameSource.ErrorLabel
            && error.LogicalTextBytes.Span.SequenceEqual("<Error>"u8));
        byte[] cache = "saved-name"u8.ToArray();
        var cached = NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Exclusive, 1, names, cache);
        cache[0] = (byte)'X';
        check("Profiler nonempty cached logical text overrides kind/table selection and returned bytes are independent",
            cached.Source == NativeProfilerDisplayNameSource.CachedOverride
            && cached.LogicalTextBytes.Span.SequenceEqual("saved-name"u8)
            && NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 1, names, ReadOnlyMemory<byte>.Empty)
                .Source == NativeProfilerDisplayNameSource.NodeNameTable);
        var replaced = MatrixServerProfilerNames60NativeLayout.Apply(true, names,
            new([], [new(1, "replacement"u8.ToArray())]), 1).State;
        check("Profiler uncached getter follows name-map updates while sampled nonempty cache remains authoritative",
            NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 1, replaced)
                .LogicalTextBytes.Span.SequenceEqual("replacement"u8)
            && NativeProfilerDisplayNames.Resolve(NativeProfilerTreeKind.Node, 1, replaced, "cached"u8.ToArray())
                .LogicalTextBytes.Span.SequenceEqual("cached"u8));
    }
}
