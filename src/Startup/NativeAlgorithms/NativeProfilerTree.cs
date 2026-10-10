namespace Firefall.Reconstruction.Startup;

public enum NativeProfilerTreeKind
{
    Root = 1,
    Node = 2,
    Exclusive = 3
}

public sealed class NativeProfilerTreeNode
{
    internal List<NativeProfilerTreeNode> MutableChildren { get; } = [];
    public NativeProfilerTreeKind Kind { get; }
    public ushort Id { get; }
    public NativeProfilerTreeNode? Parent { get; }
    public IReadOnlyList<NativeProfilerTreeNode> Children { get; }
    public bool IsExpanded { get; set; }
    public NativeProfilerHistory80 History { get; } = new(default);

    internal NativeProfilerTreeNode(NativeProfilerTreeKind kind, ushort id, NativeProfilerTreeNode? parent)
    {
        Kind = kind;
        Id = id;
        Parent = parent;
        Children = MutableChildren.AsReadOnly();
    }
}

/// <summary>Persistent structural merge and optional offline selection recovery. No metric updates or UI execution.</summary>
public sealed class NativeProfilerTree
{
    public const uint RootMergeFileOffset = 0xE8D440;
    public const uint ChildMergeFileOffset = 0xE8D010;
    public const int NativeKindOffset = 0x8D8;
    public const int NativeIdOffset = 0x8DC;
    public const int NativeParentOffset = 4;
    public const int NativeNextSiblingOffset = 8;
    public const int NativeFirstChildOffset = 0xC;

    private readonly List<NativeProfilerTreeNode> _roots = [];
    public IReadOnlyList<NativeProfilerTreeNode> Roots { get; }

    public NativeProfilerTree() => Roots = _roots.AsReadOnly();

    public IReadOnlyList<NativeProfilerTreeNode> MergeStructure(MatrixNestedWordRootsPayload frame) =>
        MergeStructureCore(frame, null, out _);

    private IReadOnlyList<NativeProfilerTreeNode> MergeStructureCore(
        MatrixNestedWordRootsPayload frame, NativeProfilerTreeNode? selected, out bool selectionVisited)
    {
        ArgumentNullException.ThrowIfNull(frame);
        selectionVisited = false;
        var matchedRoots = new List<NativeProfilerTreeNode>(frame.Roots.Count);
        int cursor = 0, previous = -1;
        foreach (var root in frame.Roots)
        {
            matchedRoots.Add(MatchOrInsert(_roots, NativeProfilerTreeKind.Root, root.WordAt0, null,
                ref cursor, ref previous));
            selectionVisited |= ReferenceEquals(selected, matchedRoots[^1]);
        }
        var pending = new Stack<(NativeProfilerTreeNode Parent, IReadOnlyList<MatrixNestedWordNode> Source)>();
        for (int i = frame.Roots.Count - 1; i >= 0; i--)
            pending.Push((matchedRoots[i], frame.Roots[i].Children));
        while (pending.TryPop(out var level))
        {
            var children = level.Parent.MutableChildren;
            int start = 0;
            if (level.Parent.Kind == NativeProfilerTreeKind.Node)
            {
                if (children.Count == 0)
                    children.Add(new(NativeProfilerTreeKind.Exclusive, level.Parent.Id, level.Parent));
                if (children[0].Kind != NativeProfilerTreeKind.Exclusive || children[0].Id != level.Parent.Id)
                    throw new InvalidOperationException("Native profiler kind2 requires a same-ID kind3 first child.");
                selectionVisited |= ReferenceEquals(selected, children[0]);
                start = 1;
            }
            cursor = start;
            previous = start - 1;
            var matched = new List<NativeProfilerTreeNode>(level.Source.Count);
            foreach (var node in level.Source)
            {
                matched.Add(MatchOrInsert(children, NativeProfilerTreeKind.Node, node.WordAt0, level.Parent,
                    ref cursor, ref previous));
                selectionVisited |= ReferenceEquals(selected, matched[^1]);
            }
            for (int i = level.Source.Count - 1; i >= 0; i--)
                pending.Push((matched[i], level.Source[i].Children));
        }
        return matchedRoots.AsReadOnly();
    }

    public NativeProfilerTreeNode? FindFirstByIdentity(NativeProfilerTreeKind kind, ushort id)
    {
        foreach (var node in EnumeratePreorder(visibleOnly: false))
            if (node.Kind == kind && node.Id == id)
                return node;
        return null;
    }

    /// <summary>The native hidden container is excluded from row numbering; its expansion state is an explicit input.</summary>
    public NativeProfilerTreeNode? FindVisibleRow(uint index, bool containerExpanded)
    {
        if (!containerExpanded)
            return null;
        uint cursor = 0;
        foreach (var node in EnumeratePreorder(visibleOnly: true))
        {
            if (cursor == index)
                return node;
            cursor = unchecked(cursor + 1);
        }
        return null;
    }

    public NativeProfilerTreeSelectionResult MergeWithSelection(
        MatrixNestedWordRootsPayload frame, NativeProfilerTreeNode? previousSelection, bool containerExpanded)
    {
        var matchedRoots = MergeStructureCore(frame, previousSelection, out bool visited);
        if (visited)
            return new(matchedRoots, previousSelection, NativeProfilerSelectionOutcome.RetainedVisitedObject,
                containerExpanded);
        var recovered = previousSelection is null
            ? null : FindFirstByIdentity(previousSelection.Kind, previousSelection.Id);
        if (recovered is not null)
        {
            for (var parent = recovered.Parent; parent is not null; parent = parent.Parent)
                parent.IsExpanded = true;
            return new(matchedRoots, recovered, NativeProfilerSelectionOutcome.RecoveredFirstIdentityMatch,
                ContainerExpanded: true);
        }
        var fallback = FindVisibleRow(0, containerExpanded);
        return new(matchedRoots, fallback, fallback is null
            ? NativeProfilerSelectionOutcome.NoVisibleRow
            : NativeProfilerSelectionOutcome.SelectedFirstVisibleRow, containerExpanded);
    }

    private IEnumerable<NativeProfilerTreeNode> EnumeratePreorder(bool visibleOnly)
    {
        var pending = new Stack<NativeProfilerTreeNode>();
        for (int i = _roots.Count - 1; i >= 0; i--)
            pending.Push(_roots[i]);
        while (pending.TryPop(out var node))
        {
            yield return node;
            if (!visibleOnly || node.IsExpanded)
                for (int i = node.Children.Count - 1; i >= 0; i--)
                    pending.Push(node.Children[i]);
        }
    }

    private static NativeProfilerTreeNode MatchOrInsert(
        List<NativeProfilerTreeNode> siblings, NativeProfilerTreeKind kind, ushort id, NativeProfilerTreeNode? parent,
        ref int cursor, ref int previous)
    {
        int found = cursor;
        while (found < siblings.Count && siblings[found].Id != id)
            found++;
        if (found == siblings.Count)
        {
            found = previous + 1;
            siblings.Insert(found, new(kind, id, parent));
        }
        if (siblings[found].Kind != kind)
            throw new InvalidOperationException("Native profiler sibling kind invariant failed.");
        previous = found;
        cursor = found + 1;
        return siblings[found];
    }
}
