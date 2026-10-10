using Firefall.Reconstruction.Startup;

internal static class NativeProfilerTreeChecks
{
    public static void Run(Action<string, bool> check)
    {
        MatrixNestedWordNode Node(ushort id, params MatrixNestedWordNode[] children) => new(id, new ushort[8], children);
        var tree = new NativeProfilerTree();
        var first = tree.MergeStructure(new([new(10, [Node(20, Node(30))])]));
        var root = first[0];
        var node = root.Children[0];
        check("Profiler structure materializes root/node kinds and same-ID exclusive first children with parent links",
            root.Kind == NativeProfilerTreeKind.Root && root.Parent is null
            && node.Kind == NativeProfilerTreeKind.Node && ReferenceEquals(node.Parent, root)
            && node.Children[0] is { Kind: NativeProfilerTreeKind.Exclusive, Id: 20 }
            && node.Children[1].Id == 30 && node.Children[1].Children[0].Id == 30);
        tree.MergeStructure(new([new(10, [Node(40)])]));
        var afterEmpty = tree.MergeStructure(new([]));
        check("Profiler structure retains stale roots/children and inserts new unmatched peers before remaining stale siblings",
            afterEmpty.Count == 0 && tree.Roots.Count == 1 && ReferenceEquals(tree.Roots[0], root)
            && root.Children.Select(child => child.Id).SequenceEqual(new ushort[] { 40, 20 })
            && ReferenceEquals(root.Children[1], node) && node.Children.Count == 2);
        var reordered = new NativeProfilerTree();
        var old = reordered.MergeStructure(new([new(1, []), new(2, [])]));
        var next = reordered.MergeStructure(new([new(2, []), new(1, [])]));
        check("Profiler forward-only cursor preserves skipped old nodes and creates duplicate-ID nodes on reorder",
            reordered.Roots.Select(item => item.Id).SequenceEqual(new ushort[] { 1, 2, 1 })
            && ReferenceEquals(next[0], old[1]) && !ReferenceEquals(next[1], old[0]));
        var duplicates = new NativeProfilerTree();
        var duplicateRoots = duplicates.MergeStructure(new([new(5, [Node(7), Node(7)]), new(5, [])]));
        check("Profiler duplicate source IDs remain separate tree objects rather than dictionary-deduplicated keys",
            duplicateRoots.Count == 2 && !ReferenceEquals(duplicateRoots[0], duplicateRoots[1])
            && duplicateRoots[0].Children.Count == 2
            && !ReferenceEquals(duplicateRoots[0].Children[0], duplicateRoots[0].Children[1]));
        MatrixNestedWordNode deep = Node(65535);
        for (int i = 0; i < 600; i++)
            deep = Node(0, deep);
        var deepTree = new NativeProfilerTree();
        var cursor = deepTree.MergeStructure(new([new(65535, [deep])]))[0].Children[0];
        int depth = 0;
        while (cursor.Children.Count > 1)
        {
            cursor = cursor.Children[1];
            depth++;
        }
        check("Profiler structure iteratively merges600 nested levels without a managed recursion limit",
            depth == 600 && cursor.Id == 65535 && cursor.Children.Count == 1);

        check("Profiler new roots, regular nodes and exclusive nodes begin collapsed",
            !root.IsExpanded && !node.IsExpanded && !node.Children[0].IsExpanded);
        check("Profiler identity lookup ignores collapsed flags and distinguishes regular/exclusive kinds",
            ReferenceEquals(tree.FindFirstByIdentity(NativeProfilerTreeKind.Node, 20), node)
            && ReferenceEquals(tree.FindFirstByIdentity(NativeProfilerTreeKind.Exclusive, 20), node.Children[0])
            && tree.FindFirstByIdentity(NativeProfilerTreeKind.Node, 10) is null
            && deepTree.FindFirstByIdentity(NativeProfilerTreeKind.Exclusive, 65535) is not null);
        var staleSelection = reordered.MergeWithSelection(new([new(2, [])]), old[0], containerExpanded: false);
        check("Profiler unvisited selection searches persistent preorder and can recover an old stale object before a new same-ID object",
            staleSelection.Outcome == NativeProfilerSelectionOutcome.RecoveredFirstIdentityMatch
            && ReferenceEquals(staleSelection.Selection, old[0]) && staleSelection.ContainerExpanded
            && !old[0].IsExpanded);
        var retained = tree.MergeWithSelection(new([new(10, [Node(20)])]), node.Children[0],
            containerExpanded: false);
        check("Profiler exclusive selection is visited even for an empty incoming child list and retains collapsed ancestors",
            retained.Outcome == NativeProfilerSelectionOutcome.RetainedVisitedObject
            && ReferenceEquals(retained.Selection, node.Children[0]) && !retained.ContainerExpanded
            && !root.IsExpanded && !node.IsExpanded);
        var recoveredLeaf = tree.MergeWithSelection(new([new(10, [])]), node.Children[1],
            containerExpanded: false);
        check("Profiler recovered stale leaf expands only ancestors, including the hidden container",
            recoveredLeaf.Outcome == NativeProfilerSelectionOutcome.RecoveredFirstIdentityMatch
            && ReferenceEquals(recoveredLeaf.Selection, node.Children[1])
            && root.IsExpanded && node.IsExpanded && !node.Children[1].IsExpanded
            && recoveredLeaf.ContainerExpanded);
        check("Profiler visible rows are expanded-child preorder including exclusive nodes and reject closed container/out-of-range index",
            ReferenceEquals(tree.FindVisibleRow(0, true), root)
            && tree.FindVisibleRow(1, true)?.Id == 40
            && ReferenceEquals(tree.FindVisibleRow(2, true), node)
            && ReferenceEquals(tree.FindVisibleRow(3, true), node.Children[0])
            && ReferenceEquals(tree.FindVisibleRow(4, true), node.Children[1])
            && tree.FindVisibleRow(5, true) is null && tree.FindVisibleRow(uint.MaxValue, true) is null
            && tree.FindVisibleRow(0, false) is null);
        var missingSelection = tree.MergeWithSelection(new([]), cursor, containerExpanded: true);
        var closedFallback = tree.MergeWithSelection(new([]), null, containerExpanded: false);
        var emptyFallback = new NativeProfilerTree().MergeWithSelection(new([]), null, containerExpanded: true);
        check("Profiler absent/null selection falls back to the first visible row, not allocation or unconditional selection",
            missingSelection.Outcome == NativeProfilerSelectionOutcome.SelectedFirstVisibleRow
            && ReferenceEquals(missingSelection.Selection, root)
            && closedFallback.Outcome == NativeProfilerSelectionOutcome.NoVisibleRow
            && closedFallback.Selection is null && !closedFallback.ContainerExpanded
            && emptyFallback.Outcome == NativeProfilerSelectionOutcome.NoVisibleRow
            && emptyFallback.Selection is null);
    }
}
