namespace Firefall.Reconstruction.Startup;

public sealed record MatrixProfilerMissingNames(IReadOnlyList<ushort> RootIds, IReadOnlyList<ushort> NodeIds);

public enum MatrixProfilerNameRequestOutcome
{
    NoMissingNames,
    Throttled,
    EndpointAbsent,
    CallbackSelected
}

public sealed record MatrixProfilerNameRequestSelection(MatrixProfilerNameRequestOutcome Outcome, uint TickMarkerAfter);

/// <summary>Missing-name traversal and sampled gates before opaque native frame refresh; no callback or UI operation is executed.</summary>
public static class MatrixServerProfilerFrame61NativeLayout
{
    public const uint ConsumerFileOffset = 0xE65290;
    public const uint MissingRootCollectorFileOffset = 0xE669F0;
    public const uint MissingNodeCollectorFileOffset = 0xE66B90;
    public const uint NameLookupFileOffset = 0xE67260;
    public const uint OpaqueRefreshFirstFileOffset = 0xE8D440;
    public const uint OpaqueRefreshSecondFileOffset = 0xE8DB00;
    public const uint RefreshSuppressionByteVirtualAddress = 0x1E49A5A;
    public const int RequestContextOwnerOffset = 0x1D0;
    public const uint RequestIntervalTicks = 5000;

    public static MatrixProfilerMissingNames CollectMissingNames(
        MatrixProfilerNamesState names, MatrixNestedWordRootsPayload frame)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(frame);
        var roots = new List<ushort>();
        var nodes = new List<ushort>();
        var stack = new Stack<MatrixNestedWordNode>();
        foreach (var root in frame.Roots)
        {
            if (!names.RootNames.ContainsKey(root.WordAt0))
                roots.Add(root.WordAt0);
            PushReverse(root.Children, stack);
            while (stack.TryPop(out var node))
            {
                if (!names.NodeNames.ContainsKey(node.WordAt0))
                    nodes.Add(node.WordAt0);
                PushReverse(node.Children, stack);
            }
        }
        return new(roots.AsReadOnly(), nodes.AsReadOnly());
    }

    public static MatrixProfilerNameRequestSelection SelectMissingNameRequest(
        MatrixProfilerMissingNames missing, uint sampledNow, uint sampledMarker, bool sampledEndpointAvailable)
    {
        ArgumentNullException.ThrowIfNull(missing);
        if (missing.RootIds.Count == 0 && missing.NodeIds.Count == 0)
            return new(MatrixProfilerNameRequestOutcome.NoMissingNames, sampledMarker);
        if (unchecked(sampledNow - sampledMarker) <= RequestIntervalTicks)
            return new(MatrixProfilerNameRequestOutcome.Throttled, sampledMarker);
        return new(sampledEndpointAvailable ? MatrixProfilerNameRequestOutcome.CallbackSelected
            : MatrixProfilerNameRequestOutcome.EndpointAbsent, sampledNow);
    }

    public static bool SelectOpaqueFrameRefresh(byte sampledSuppressionByte) => sampledSuppressionByte == 0;

    private static void PushReverse(IReadOnlyList<MatrixNestedWordNode> children, Stack<MatrixNestedWordNode> stack)
    {
        for (int i = children.Count - 1; i >= 0; i--)
            stack.Push(children[i]);
    }
}
