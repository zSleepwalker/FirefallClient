namespace Firefall.Reconstruction.Startup;

public enum NativeProfilerSelectionOutcome
{
    RetainedVisitedObject,
    RecoveredFirstIdentityMatch,
    SelectedFirstVisibleRow,
    NoVisibleRow
}

public sealed record NativeProfilerTreeSelectionResult(
    IReadOnlyList<NativeProfilerTreeNode> MatchedRoots,
    NativeProfilerTreeNode? Selection,
    NativeProfilerSelectionOutcome Outcome,
    bool ContainerExpanded);

public static class NativeProfilerTreeSelection
{
    public const uint IdentitySearchFileOffset = 0xE8D3F0;
    public const uint VisibleRowSearchFileOffset = 0xE72480;
    public const int NativeExpansionByteOffset = 0x10;
}
