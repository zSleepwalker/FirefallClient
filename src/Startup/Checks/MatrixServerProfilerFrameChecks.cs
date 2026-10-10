using Firefall.Reconstruction.Startup;

internal static class MatrixServerProfilerFrameChecks
{
    public static void Run(Action<string, bool> check)
    {
        var names = MatrixServerProfilerNames60NativeLayout.Apply(true,
            MatrixServerProfilerNames60NativeLayout.Empty(0),
            new([new(1, ReadOnlyMemory<byte>.Empty)], [new(10, ReadOnlyMemory<byte>.Empty)]), 5000).State;
        MatrixNestedWordNode Node(ushort id, params MatrixNestedWordNode[] children) => new(id, new ushort[8], children);
        var frame = new MatrixNestedWordRootsPayload(
        [
            new(1, [Node(2, Node(3)), Node(10, Node(2))]),
            new(257, [Node(3)]),
            new(257, [])
        ]);
        var missing = MatrixServerProfilerFrame61NativeLayout.CollectMissingNames(names, frame);
        check("Profiler frame61 missing IDs preserve duplicates/root order/node preorder and treat present-empty names as known",
            missing.RootIds.SequenceEqual(new ushort[] { 257, 257 })
            && missing.NodeIds.SequenceEqual(new ushort[] { 2, 3, 2, 3 }));
        check("Profiler frame61 request threshold is unsigned elapsed strictly greater than5000",
            MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 6000, 1000, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.Throttled, TickMarkerAfter: 1000 }
            && MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 6001, 1000, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.CallbackSelected, TickMarkerAfter: 6001 }
            && MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 4000, uint.MaxValue - 1000, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.CallbackSelected, TickMarkerAfter: 4000 });
        check("Profiler frame61 advances marker before absent endpoint and leaves marker unchanged when no names are missing",
            MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 6001, 1000, false)
                is { Outcome: MatrixProfilerNameRequestOutcome.EndpointAbsent, TickMarkerAfter: 6001 }
            && MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(new([], []), 6001, 1000, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.NoMissingNames, TickMarkerAfter: 1000 });
        var namesReply = MatrixServerProfilerNames60NativeLayout.Apply(true, names, new([], []), 10000).State;
        check("Profiler names60 tick-minus5000 makes same-tick frame request throttled and next-tick request selectable",
            MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 10000, namesReply.TickMarkerAt18C, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.Throttled }
            && MatrixServerProfilerFrame61NativeLayout.SelectMissingNameRequest(missing, 10001, namesReply.TickMarkerAt18C, true)
                is { Outcome: MatrixProfilerNameRequestOutcome.CallbackSelected });
        MatrixNestedWordNode deep = Node(65535);
        for (int i = 0; i < 600; i++)
            deep = Node(0, deep);
        var deepMissing = MatrixServerProfilerFrame61NativeLayout.CollectMissingNames(names,
            new([new(65535, [deep])]));
        check("Profiler frame61 iteratively handles600 levels, zero/maximum IDs and independent opaque-refresh suppression",
            deepMissing.RootIds.SequenceEqual(new ushort[] { 65535 })
            && deepMissing.NodeIds.Count == 601 && deepMissing.NodeIds[0] == 0 && deepMissing.NodeIds[^1] == 65535
            && MatrixServerProfilerFrame61NativeLayout.SelectOpaqueFrameRefresh(0)
            && !MatrixServerProfilerFrame61NativeLayout.SelectOpaqueFrameRefresh(1)
            && !MatrixServerProfilerFrame61NativeLayout.SelectOpaqueFrameRefresh(0x80));
    }
}
