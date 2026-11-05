using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSuccessorCommonLifecycleCapture(
    uint? EntryECX,
    uint? EntryEAX,
    uint? CallerEDI,
    uint? CallerESI);

public enum CapacityFinalCalleeSuccessorCommonLifecycleOutcome
{
    MissingEntryECX,
    MissingEntryEAX,
    MissingEntryECXAndEAX,
    MissingCallerEDI,
    MissingCallerESI,
    Completed
}

public readonly record struct CapacityFinalCalleeSuccessorCommonLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeSuccessorCommonLifecyclePlan(
    CapacityFinalCalleeSuccessorCommonLifecycleCapture Capture,
    CapacityFinalCalleeSuccessorCommonLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeAlternateSuccessorWritesPlan Upstream { get; init; } =
        null!;
    public CapacityFinalCalleeCommonTailTagWritesPlan? CommonTail { get; init; }
    public bool CallerEDICaptureReached { get; init; }
    public bool CallerESICaptureReached { get; init; }
    public bool CallerEDICaptureProvidedButUnreached { get; init; }
    public bool CallerESICaptureProvidedButUnreached { get; init; }
    public bool FlagsPreservedThroughUpstream { get; init; }
    public CapacityFinalCalleeCommonTailTagWritesFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeSuccessorCommonLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeSuccessorCommonLifecycleWrite>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the exact clean 92BD successor exit to the bounded common tail ending at 92C8.
/// </summary>
public static class CapacityFinalCalleeSuccessorCommonLifecycleProjection
{
    public const uint UpstreamEntryRawFileOffset =
        CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryRawFileOffset;
    public const uint CommonTailEntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeSuccessorCommonLifecyclePlan Project(
        CapacityFinalCalleeSuccessorCommonLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateSuccessorWritesPlan upstream =
            CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                capture.EntryECX,
                capture.EntryEAX));

        if (upstream.Outcome
            != CapacityFinalCalleeAlternateSuccessorWritesOutcome.Completed)
        {
            CapacityFinalCalleeSuccessorCommonLifecycleOutcome outcome =
                upstream.Outcome switch
                {
                    CapacityFinalCalleeAlternateSuccessorWritesOutcome.MissingEntryECX =>
                        CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingEntryECX,
                    CapacityFinalCalleeAlternateSuccessorWritesOutcome.MissingEntryEAX =>
                        CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingEntryEAX,
                    CapacityFinalCalleeAlternateSuccessorWritesOutcome
                        .MissingEntryECXAndEAX =>
                        CapacityFinalCalleeSuccessorCommonLifecycleOutcome
                            .MissingEntryECXAndEAX,
                    _ => throw new InvalidOperationException()
                };
            return new(capture, outcome)
            {
                Upstream = upstream,
                CallerEDICaptureProvidedButUnreached = capture.CallerEDI.HasValue,
                CallerESICaptureProvidedButUnreached = capture.CallerESI.HasValue,
                StopAtRawFileOffsetExclusive =
                    upstream.StopAtRawFileOffsetExclusive
            };
        }

        bool exactHandoff =
            upstream.FirstStoreReached
            && upstream.SecondStoreReached
            && upstream.JumpReached
            && upstream.JumpTargetRawFileOffset == CommonTailEntryRawFileOffset
            && upstream.JumpTargetVirtualAddress
                == CapacityFinalCalleeCommonTailTagWritesProjection.EntryVirtualAddress
            && upstream.ExactCleanExitAdmitted
            && upstream.FlagsPreserved
            && upstream.StopAtRawFileOffsetExclusive == CommonTailEntryRawFileOffset;
        if (!exactHandoff)
        {
            throw new InvalidOperationException(
                "The successor projection did not admit its exact clean 92BD exit.");
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                capture.CallerEDI,
                capture.CallerESI));
        ImmutableArray<CapacityFinalCalleeSuccessorCommonLifecycleWrite> writes =
            upstream.Writes
                .Select(write =>
                    new CapacityFinalCalleeSuccessorCommonLifecycleWrite(
                        write.RawFileOffset,
                        write.Address,
                        write.Value))
                .Concat(commonTail.Writes.Select(write =>
                    new CapacityFinalCalleeSuccessorCommonLifecycleWrite(
                        write.RawFileOffset,
                        write.Address,
                        write.Value)))
                .ToImmutableArray();

        CapacityFinalCalleeSuccessorCommonLifecycleOutcome completedOutcome =
            commonTail.Outcome switch
            {
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingCallerEDI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingCallerESI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.Completed =>
                    CapacityFinalCalleeSuccessorCommonLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException()
            };

        return new(capture, completedOutcome)
        {
            Upstream = upstream,
            CommonTail = commonTail,
            CallerEDICaptureReached = true,
            CallerESICaptureReached = commonTail.EntryEDICaptured,
            CallerESICaptureProvidedButUnreached =
                commonTail.UnreachedEntryESICaptureProvided,
            FlagsPreservedThroughUpstream = true,
            FlagsAtStop = commonTail.FlagsAtStop,
            Writes = writes,
            ExactCleanExitAdmitted =
                commonTail.Outcome
                    == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed
                && commonTail.ExactCleanExitAdmitted
                && commonTail.StopAtRawFileOffsetExclusive
                    == BoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive =
                commonTail.StopAtRawFileOffsetExclusive
        };
    }
}
