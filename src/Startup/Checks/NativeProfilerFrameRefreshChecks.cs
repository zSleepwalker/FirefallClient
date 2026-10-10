using Firefall.Reconstruction.Startup;

internal static class NativeProfilerFrameRefreshChecks
{
    public static void Run(Action<string, bool> check)
    {
        MatrixNestedWordNode Node(ushort id, ushort half, params MatrixNestedWordNode[] children) =>
            new(id, new ushort[] { 1, half, half, half, 1, 2, half, half }, children);
        var tree = new NativeProfilerTree();
        MatrixNestedWordRootsPayload first = new([new(10, [Node(20, 0x4200, Node(30, 0x3C00)), Node(40, 0x4000)])]);
        var root = tree.MergeStructure(first)[0];
        var node20 = root.Children[0];
        var node30 = node20.Children[1];
        var node40 = root.Children[1];
        check("Profiler structural merge initializes all history slots to zero without advancing them",
            root.History.NextSlot == 0 && node20.History.NextSlot == 0
            && Enumerable.Range(0, 80).All(i => node20.History.GetPhysicalSlot(i) == default));
        var result = NativeProfilerFrameRefresh.Apply(tree, first);
        check("Profiler graph refresh updates children before inclusive/exclusive histories and aggregates only processed root children",
            result is { Outcome: NativeProfilerMetricOutcome.Accepted, UpdatedRoots: 1, UpdatedNodes: 3,
                SkippedNestedChildren: 0, SkippedRootChildren: 0, PositionalIdentityMismatches: 0 }
            && root.History.Latest == new NativeProfilerSample(0, 327680, 327680, 327680, 3, 6, 393216, 393216)
            && node20.History.Latest.ScaledAt4 == 196608 && node20.History.Latest.ScaledAt14 == 262144
            && node20.Children[0].History.Latest.ScaledAt4 == 131072
            && node20.Children[0].History.Latest.ScaledAt14 == 196608
            && node30.History.NextSlot == 1 && node30.Children[0].History.NextSlot == 1);

        MatrixNestedWordRootsPayload subset = new([new(10, [Node(40, 0x4400)])]);
        var matched = tree.MergeStructure(subset);
        result = NativeProfilerFrameRefresh.Apply(tree, subset);
        check("Profiler root refresh deliberately pairs by position: a skipped stale sibling receives another incoming ID's sample",
            ReferenceEquals(matched[0].Children[0], node20) && matched[0].Children[1] == node40
            && result is { Outcome: NativeProfilerMetricOutcome.Accepted, UpdatedNodes: 1, PositionalIdentityMismatches: 1 }
            && node20.History.Latest.ScaledAt4 == 262144 && node20.History.NextSlot == 2
            && node40.History.Latest.ScaledAt4 == 131072 && node40.History.NextSlot == 1
            && node30.History.NextSlot == 1 && root.History.Latest.ScaledAt4 == 262144
            && node20.Children[0].History.Latest.ScaledAt4 == 262144);

        var poison = Node(50, 0x7C00);
        MatrixNestedWordRootsPayload missing = new([new(10, [Node(40, 0x4500, poison)])]);
        tree.MergeStructure(missing);
        result = NativeProfilerFrameRefresh.Apply(tree, missing);
        check("Profiler deeper refresh skips missing forward IDs without decoding or aggregating their nonfinite samples",
            result is { Outcome: NativeProfilerMetricOutcome.Accepted, UpdatedNodes: 1,
                SkippedNestedChildren: 1, PositionalIdentityMismatches: 1 }
            && node20.History.Latest.ScaledAt4 == 327680 && node30.History.NextSlot == 1
            && node40.Children[1].History.NextSlot == 0 && root.History.NextSlot == 3);
        result = NativeProfilerFrameRefresh.Apply(tree, new([new(10, [])]));
        var noRoots = NativeProfilerFrameRefresh.Apply(tree, new([]));
        check("Profiler empty source children append a zero root sample without refreshing stale children; absent roots do not advance",
            result is { UpdatedRoots: 1, UpdatedNodes: 0 } && root.History.Latest == default
            && node20.History.NextSlot == 3 && noRoots.UpdatedRoots == 0 && root.History.NextSlot == 4);

        var partial = new NativeProfilerTree();
        var rejected = Node(2, 0x7E00);
        MatrixNestedWordRootsPayload invalid = new([new(1, [Node(1, 0x3C00), rejected])]);
        var partialRoot = partial.MergeStructure(invalid)[0];
        var unsupported = NativeProfilerFrameRefresh.Apply(partial, invalid);
        check("Profiler unsupported half outcome preserves earlier history writes and reports the exact rejected source/index",
            unsupported is { Outcome: NativeProfilerMetricOutcome.UnsupportedNonFiniteHalf,
                UpdatedRoots: 0, UpdatedNodes: 1, UnsupportedWordIndex: 1 }
            && ReferenceEquals(unsupported.RejectedSourceNode, rejected)
            && partialRoot.History.NextSlot == 0 && partialRoot.Children[0].History.NextSlot == 1
            && partialRoot.Children[0].Children[0].History.NextSlot == 1
            && partialRoot.Children[1].History.NextSlot == 0);
        MatrixNestedWordRootsPayload malformed = new([new(1, [new(1, new ushort[7], [])])]);
        partial.MergeStructure(malformed);
        check("Profiler graph refresh reports malformed metric word count without advancing the rejected node",
            NativeProfilerFrameRefresh.Apply(partial, malformed)
                is { Outcome: NativeProfilerMetricOutcome.InvalidWordCount, UpdatedNodes: 0 }
            && partialRoot.Children[0].History.NextSlot == 1);
        var empty = new NativeProfilerTree();
        empty.MergeStructure(new([new(1, [])]));
        var skipped = NativeProfilerFrameRefresh.Apply(empty, invalid);
        check("Profiler native no-stored-child branch emits zero root without decoding incoming children",
            skipped is { Outcome: NativeProfilerMetricOutcome.Accepted, UpdatedRoots: 1, UpdatedNodes: 0, SkippedRootChildren: 2 }
            && empty.Roots[0].History.Latest == default);

        MatrixNestedWordNode deep = Node(65535, 0x3C00);
        for (int i = 0; i < 600; i++)
            deep = Node(0, 0x3C00, deep);
        MatrixNestedWordRootsPayload deepFrame = new([new(65535, [deep])]);
        var deepTree = new NativeProfilerTree();
        deepTree.MergeStructure(deepFrame);
        check("Profiler graph sample refresh handles601 nested nodes iteratively and root total uses inclusive rather than summed descendants",
            NativeProfilerFrameRefresh.Apply(deepTree, deepFrame) is { UpdatedNodes: 601, UpdatedRoots: 1 }
            && deepTree.Roots[0].History.Latest.ScaledAt4 == 65536
            && deepTree.Roots[0].History.Latest.RawWordAt10 == 601);
        bool missingRootThrows = false;
        try { NativeProfilerFrameRefresh.Apply(new NativeProfilerTree(), first); }
        catch (InvalidOperationException) { missingRootThrows = true; }
        check("Profiler inconsistent missing-root graph is explicit failure rather than a success-shaped fallback", missingRootThrows);
    }
}
