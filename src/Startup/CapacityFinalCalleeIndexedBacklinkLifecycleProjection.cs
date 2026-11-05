namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedBacklinkLifecycleCapture(
    CapacityFinalCalleeAlternateIndexedFrameGateCapture UpstreamCapture,
    uint? CallerEBX,
    uint? CallerEDX,
    uint? FreshFieldAtEcxPlusC);

public enum CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
{
    UpstreamIncomplete,
    UpstreamIncompleteWithDownstreamCapture,
    UpstreamEqualTo9160,
    UpstreamEqualTo9160WithDownstreamCapture,
    MissingBacklinkEntryEBX,
    BelowTo915A,
    BelowTo915AWithUnusedCapture,
    MissingBacklinkEntryEDX,
    MissingFreshFieldAtEcxPlusC,
    EqualTo9160,
    UnequalTo915A
}

public sealed record CapacityFinalCalleeIndexedBacklinkLifecyclePlan(
    CapacityFinalCalleeIndexedBacklinkLifecycleCapture Capture,
    CapacityFinalCalleeAlternateIndexedFrameGatePlan Upstream,
    CapacityFinalCalleeAlternateBacklinkGatePlan? BacklinkGate,
    CapacityFinalCalleeIndexedBacklinkLifecycleOutcome Outcome,
    bool BacklinkGateReached,
    bool ExactCleanHandoffToBacklinkGate,
    uint? DerivedBacklinkEntryECX,
    bool UnreachedCallerEBXCaptureProvided,
    bool UnreachedCallerEDXCaptureProvided,
    bool UnreachedFreshFieldCaptureProvided,
    bool LateCallerEDXCaptureProvided,
    bool LateFreshFieldCaptureProvided,
    bool UnusedCallerEDXCaptureProvided,
    bool UnusedFreshFieldCaptureProvided,
    bool ExactCleanExitAdmitted,
    bool ExactCleanBypassAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactCleanBacklinkExitAdmitted => ExactCleanExitAdmitted;
    public bool OpaqueCallReached => false;
}

/// <summary>
/// Composes the indexed-frame gate's clean unequal handoff at 9151 with the
/// bounded backlink gate. The equal 9160 destination remains a bounded stop.
/// </summary>
public static class CapacityFinalCalleeIndexedBacklinkLifecycleProjection
{
    public const uint IndexedGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EntryRawFileOffset;
    public const uint BacklinkGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.EntryRawFileOffset;
    public const uint IndexedEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint IndexedUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint BacklinkEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.EqualDestinationRawFileOffset;
    public const uint BacklinkUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.UnequalDestinationRawFileOffset;
    public const uint BacklinkBelowDestinationRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.BelowDestinationRawFileOffset;

    public static CapacityFinalCalleeIndexedBacklinkLifecyclePlan Project(
        CapacityFinalCalleeIndexedBacklinkLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateIndexedFrameGatePlan upstream =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.Project(
                capture.UpstreamCapture);
        bool callerEbxProvided = capture.CallerEBX.HasValue;
        bool callerEdxProvided = capture.CallerEDX.HasValue;
        bool freshFieldProvided = capture.FreshFieldAtEcxPlusC.HasValue;
        bool anyDownstreamCapture = callerEbxProvided
            || callerEdxProvided
            || freshFieldProvided;

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan Finish(
            CapacityFinalCalleeIndexedBacklinkLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateBacklinkGatePlan? backlink = null,
            bool backlinkReached = false,
            bool exactHandoff = false,
            uint? derivedEcx = null,
            bool unreachedEbx = false,
            bool unreachedEdx = false,
            bool unreachedFreshField = false,
            bool lateEdx = false,
            bool lateFreshField = false,
            bool unusedEdx = false,
            bool unusedFreshField = false,
            bool exactCleanExitAdmitted = false,
            bool exactCleanBypassAdmitted = false) =>
            new(
                capture,
                upstream,
                backlink,
                outcome,
                backlinkReached,
                exactHandoff,
                derivedEcx,
                unreachedEbx,
                unreachedEdx,
                unreachedFreshField,
                lateEdx,
                lateFreshField,
                unusedEdx,
                unusedFreshField,
                exactCleanExitAdmitted,
                exactCleanBypassAdmitted,
                stopAt);

        if (IsExactCleanEqualBoundary(upstream, capture.UpstreamCapture))
        {
            return Finish(
                anyDownstreamCapture
                    ? CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .UpstreamEqualTo9160WithDownstreamCapture
                    : CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .UpstreamEqualTo9160,
                upstream.StopAtRawFileOffsetExclusive,
                unreachedEbx: callerEbxProvided,
                unreachedEdx: callerEdxProvided,
                unreachedFreshField: freshFieldProvided,
                exactCleanExitAdmitted: !anyDownstreamCapture,
                exactCleanBypassAdmitted: !anyDownstreamCapture);
        }

        if (!IsExactCleanUnequalHandoff(upstream, capture.UpstreamCapture))
        {
            return Finish(
                anyDownstreamCapture
                    ? CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .UpstreamIncompleteWithDownstreamCapture
                    : CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .UpstreamIncomplete,
                upstream.StopAtRawFileOffsetExclusive,
                unreachedEbx: callerEbxProvided,
                unreachedEdx: callerEdxProvided,
                unreachedFreshField: freshFieldProvided);
        }

        uint derivedEcx = capture.UpstreamCapture.FreshFrameMinus10!.Value;
        CapacityFinalCalleeAlternateBacklinkGatePlan firstCompare =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: derivedEcx,
                EntryEBX: capture.CallerEBX,
                EntryEDX: null,
                FreshFieldAtEcxPlusC: null));

        if (!firstCompare.FirstCompareReached)
        {
            return Finish(
                CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .MissingBacklinkEntryEBX,
                firstCompare.StopAtRawFileOffsetExclusive,
                firstCompare,
                backlinkReached: true,
                exactHandoff: true,
                derivedEcx: derivedEcx,
                lateEdx: callerEdxProvided,
                lateFreshField: freshFieldProvided);
        }

        if (firstCompare.JbTaken == true)
        {
            bool unusedEdx = callerEdxProvided;
            bool unusedFreshField = freshFieldProvided;
            bool cleanExit = !unusedEdx
                && !unusedFreshField
                && firstCompare.ExactCleanExitAdmitted;
            return Finish(
                unusedEdx || unusedFreshField
                    ? CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .BelowTo915AWithUnusedCapture
                    : CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .BelowTo915A,
                firstCompare.StopAtRawFileOffsetExclusive,
                firstCompare,
                backlinkReached: true,
                exactHandoff: true,
                derivedEcx: derivedEcx,
                unusedEdx: unusedEdx,
                unusedFreshField: unusedFreshField,
                exactCleanExitAdmitted: cleanExit);
        }

        CapacityFinalCalleeAlternateBacklinkGatePlan backlinkGate =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(new(
                EntryECX: derivedEcx,
                EntryEBX: capture.CallerEBX,
                EntryEDX: capture.CallerEDX,
                FreshFieldAtEcxPlusC: capture.FreshFieldAtEcxPlusC));

        if (backlinkGate.Outcome
            == CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEDX)
        {
            return Finish(
                CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .MissingBacklinkEntryEDX,
                backlinkGate.StopAtRawFileOffsetExclusive,
                backlinkGate,
                backlinkReached: true,
                exactHandoff: true,
                derivedEcx: derivedEcx);
        }

        if (backlinkGate.Outcome
            == CapacityFinalCalleeAlternateBacklinkGateOutcome
                .MissingFreshFieldAtEcxPlusC)
        {
            return Finish(
                CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .MissingFreshFieldAtEcxPlusC,
                backlinkGate.StopAtRawFileOffsetExclusive,
                backlinkGate,
                backlinkReached: true,
                exactHandoff: true,
                derivedEcx: derivedEcx);
        }

        bool equal = backlinkGate.Outcome
            == CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget;
        return Finish(
            equal
                ? CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.EqualTo9160
                : CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UnequalTo915A,
            backlinkGate.StopAtRawFileOffsetExclusive,
            backlinkGate,
            backlinkReached: true,
            exactHandoff: true,
            derivedEcx: derivedEcx,
            exactCleanExitAdmitted: backlinkGate.ExactCleanExitAdmitted,
            exactCleanBypassAdmitted: equal
                && backlinkGate.ExactCleanBypassAdmitted);
    }

    private static bool IsExactCleanEqualBoundary(
        CapacityFinalCalleeAlternateIndexedFrameGatePlan plan,
        CapacityFinalCalleeAlternateIndexedFrameGateCapture capture) =>
        IsExactCleanUpstreamBranch(
            plan,
            capture,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget,
            branchTaken: true,
            destination: IndexedEqualDestinationRawFileOffset);

    private static bool IsExactCleanUnequalHandoff(
        CapacityFinalCalleeAlternateIndexedFrameGatePlan plan,
        CapacityFinalCalleeAlternateIndexedFrameGateCapture capture) =>
        IsExactCleanUpstreamBranch(
            plan,
            capture,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough,
            branchTaken: false,
            destination: IndexedUnequalDestinationRawFileOffset);

    private static bool IsExactCleanUpstreamBranch(
        CapacityFinalCalleeAlternateIndexedFrameGatePlan plan,
        CapacityFinalCalleeAlternateIndexedFrameGateCapture capture,
        CapacityFinalCalleeAlternateIndexedFrameGateOutcome expectedOutcome,
        bool branchTaken,
        uint destination)
    {
        if (capture.EntryEBP is not uint entryEbp
            || capture.FreshFrameMinus4 is not uint freshFrameMinus4
            || capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return false;
        }

        uint indexedAddress =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection
                .CalculateIndexedAddress(freshFrameMinus4);
        return plan.Outcome == expectedOutcome
            && plan.EntryEBPCaptured
            && plan.FrameMinus4Captured
            && plan.FrameMinus4 == freshFrameMinus4
            && plan.FrameMinus10Captured
            && plan.FrameMinus10 == freshFrameMinus10
            && plan.StackStoreReached
            && plan.StackStoreAddress
                == CapacityFinalCalleeAlternateIndexedFrameGateProjection
                    .DeriveEbpMinusCAddress(entryEbp)
            && plan.StackStoreValue == indexedAddress
            && plan.CompareReached
            && plan.CompareLeft == freshFrameMinus10
            && plan.CompareRight == indexedAddress
            && plan.JeReached
            && plan.JeTaken == branchTaken
            && plan.JeSelectedDestinationRawFileOffset == destination
            && plan.StopAtRawFileOffsetExclusive == destination
            && plan.Writes.Length == 1
            && !plan.UnreachedFreshFrameMinus4CaptureProvided
            && !plan.UnreachedFreshFrameMinus10CaptureProvided
            && !plan.LateFreshFrameMinus4CaptureProvided
            && !plan.LateFreshFrameMinus10CaptureProvided
            && plan.ExactCleanRouteAdmitted;
    }
}
