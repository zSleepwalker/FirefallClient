namespace Firefall.Reconstruction.Startup;

public sealed record NativeProfilerFrameRefreshResult(
    NativeProfilerMetricOutcome Outcome,
    int UpdatedRoots,
    int UpdatedNodes,
    int SkippedNestedChildren,
    int SkippedRootChildren,
    int PositionalIdentityMismatches,
    MatrixNestedWordNode? RejectedSourceNode,
    int? UnsupportedWordIndex);

/// <summary>Sample refresh after structural merge. Root children are paired positionally; deeper children use forward ID search.</summary>
public static class NativeProfilerFrameRefresh
{
    public const uint RootUpdaterFileOffset = 0xE8DB00;
    public const uint NodeUpdaterFileOffset = 0xE8D760;

    public static NativeProfilerFrameRefreshResult Apply(NativeProfilerTree tree, MatrixNestedWordRootsPayload frame)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(frame);
        int rootCursor = 0, updatedRoots = 0, updatedNodes = 0;
        int skippedNested = 0, skippedRoot = 0, mismatches = 0;
        foreach (var sourceRoot in frame.Roots)
        {
            while (rootCursor < tree.Roots.Count && tree.Roots[rootCursor].Id != sourceRoot.WordAt0)
                rootCursor++;
            if (rootCursor == tree.Roots.Count)
                throw new InvalidOperationException("Native profiler sample refresh requires a matching persistent root.");
            var root = tree.Roots[rootCursor++];
            var processed = new List<NativeProfilerSample>(sourceRoot.Children.Count);
            if (root.Children.Count == 0)
                skippedRoot += sourceRoot.Children.Count;
            else
                for (int i = 0; i < sourceRoot.Children.Count; i++)
                {
                    if (i >= root.Children.Count)
                        throw new InvalidOperationException("Native profiler positional root-child refresh exhausted stored siblings.");
                    var node = root.Children[i];
                    var source = sourceRoot.Children[i];
                    if (node.Id != source.WordAt0)
                        mismatches++;
                    var failure = UpdateNode(node, source, ref updatedNodes, ref skippedNested, out var rejected);
                    if (failure is not null)
                        return new(failure.Outcome, updatedRoots, updatedNodes, skippedNested, skippedRoot,
                            mismatches, rejected, failure.UnsupportedWordIndex);
                    processed.Add(node.History.Latest);
                }
            root.History.Append(NativeProfilerSamples.AggregateRoot(processed));
            updatedRoots++;
        }
        return new(NativeProfilerMetricOutcome.Accepted, updatedRoots, updatedNodes, skippedNested, skippedRoot,
            mismatches, null, null);
    }

    private static NativeProfilerMetricResult? UpdateNode(
        NativeProfilerTreeNode node, MatrixNestedWordNode source, ref int updatedNodes, ref int skippedChildren,
        out MatrixNestedWordNode? rejected)
    {
        rejected = null;
        var decoded = DecodeNode(node, source);
        if (decoded.Outcome != NativeProfilerMetricOutcome.Accepted)
        {
            rejected = source;
            return decoded;
        }
        var pending = new Stack<RefreshContext>();
        pending.Push(new(node, source, RequireSample(decoded)));
        while (pending.TryPeek(out var current))
        {
            if (current.NextSourceChild < current.Source.Children.Count)
            {
                var childSource = current.Source.Children[current.NextSourceChild++];
                var children = current.Node.Children;
                while (current.NextStoredChild < children.Count
                    && children[current.NextStoredChild].Id != childSource.WordAt0)
                    current.NextStoredChild++;
                if (current.NextStoredChild == children.Count)
                {
                    skippedChildren++;
                    continue;
                }
                var child = children[current.NextStoredChild++];
                decoded = DecodeNode(child, childSource);
                if (decoded.Outcome != NativeProfilerMetricOutcome.Accepted)
                {
                    rejected = childSource;
                    return decoded;
                }
                pending.Push(new(child, childSource, RequireSample(decoded)));
                continue;
            }
            var samples = NativeProfilerSamples.ProjectNode(current.Decoded, current.ProcessedChildren);
            current.Node.Children[0].History.Append(samples.Exclusive);
            current.Node.History.Append(samples.Inclusive);
            updatedNodes++;
            pending.Pop();
            if (pending.TryPeek(out var parent))
                parent.ProcessedChildren.Add(samples.Inclusive);
        }
        return null;
    }

    private static NativeProfilerMetricResult DecodeNode(NativeProfilerTreeNode node, MatrixNestedWordNode source)
    {
        if (node.Kind != NativeProfilerTreeKind.Node || node.Children.Count == 0
            || node.Children[0].Kind != NativeProfilerTreeKind.Exclusive || node.Children[0].Id != node.Id)
            throw new InvalidOperationException("Native profiler sample refresh requires a same-ID exclusive first child.");
        return NativeProfilerSamples.Decode(source.WordsAt10To1E);
    }

    private static NativeProfilerSample RequireSample(NativeProfilerMetricResult result) =>
        result.Sample ?? throw new InvalidOperationException("Accepted profiler metric decoding must contain a sample.");

    private sealed class RefreshContext(NativeProfilerTreeNode node, MatrixNestedWordNode source, NativeProfilerSample decoded)
    {
        public NativeProfilerTreeNode Node { get; } = node;
        public MatrixNestedWordNode Source { get; } = source;
        public NativeProfilerSample Decoded { get; } = decoded;
        public int NextSourceChild { get; set; }
        public int NextStoredChild { get; set; } = 1;
        public List<NativeProfilerSample> ProcessedChildren { get; } = new(source.Children.Count);
    }
}
