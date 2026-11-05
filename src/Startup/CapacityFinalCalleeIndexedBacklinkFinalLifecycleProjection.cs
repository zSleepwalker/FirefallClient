namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedBacklinkFinalLifecycleCapture(
    CapacityFinalCalleeIndexedBacklinkLifecycleCapture IndexedBacklinkCapture,
    uint? FreshCallerFrameMinus14);

public enum CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
{
    UpstreamIndexedEqualTo9160,
    UpstreamBacklinkNotEqualTo9160,
    UpstreamBacklinkHandoffNotClean,
    MissingFreshFrameMinus14,
    MissingDerivedChildECX,
    ChildEqualToFallthrough,
    ChildUnequalToBranchTarget,
    ChildFinalFrameGateBlocked
}

public enum CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource
{
    None,
    BacklinkEntryECX
}

public sealed record CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan(
    CapacityFinalCalleeIndexedBacklinkFinalLifecycleCapture Capture,
    CapacityFinalCalleeIndexedBacklinkLifecyclePlan IndexedBacklinkLifecycle,
    CapacityFinalCalleeAlternateFinalFrameGatePlan? FinalFrameGate,
    CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome Outcome,
    bool FinalFrameGateReached,
    bool ExactCleanHandoffToFinalFrameGate,
    uint? ChildEntryEBP,
    uint? ChildEntryECX,
    CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource ChildEntryECXSource,
    bool ChildECXCarriedToCompare,
    bool UnreachedFreshFrameMinus14CaptureProvided,
    bool LateFreshFrameMinus14CaptureProvided,
    bool UnreachedDerivedChildECXAvailable,
    bool LateDerivedChildECXAvailable,
    bool ExactCleanExitAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public CapacityFinalCalleeAlternateFinalFrameGatePlan? FinalFrameGatePlan =>
        FinalFrameGate;

    public CapacityFinalCalleeAlternateIndexedFrameGatePlan IndexedFrameGatePlan =>
        IndexedBacklinkLifecycle.Upstream;

    public CapacityFinalCalleeAlternateBacklinkGatePlan? BacklinkGate =>
        IndexedBacklinkLifecycle.BacklinkGate;

    public uint? BacklinkEntryECX =>
        IndexedBacklinkLifecycle.DerivedBacklinkEntryECX;

    public System.Collections.Immutable.ImmutableArray<
        CapacityFinalCalleeAlternateIndexedFrameGateWrite> Writes =>
        IndexedBacklinkLifecycle.Upstream.Writes;

    public bool OpaqueCallReached => false;
}

/// <summary>
/// Joins only the clean downstream backlink equality at 9160 to the final
/// frame gate. An indexed-frame equality at the same address remains a stop.
/// </summary>
public static class CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
{
    public const uint FinalFrameEntryRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset;
    public const uint IndexedEqualBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateIndexedFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint BacklinkEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.EqualDestinationRawFileOffset;
    public const uint BacklinkBelowBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.BelowDestinationRawFileOffset;
    public const uint BacklinkUnequalBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.UnequalDestinationRawFileOffset;
    public const uint FinalFrameEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint FinalFrameUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset;

    public static CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan Project(
        CapacityFinalCalleeIndexedBacklinkFinalLifecycleCapture capture)
    {
        CapacityFinalCalleeIndexedBacklinkLifecyclePlan upstream =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                capture.IndexedBacklinkCapture);
        bool freshFrame14Provided = capture.FreshCallerFrameMinus14.HasValue;
        uint? derivedChildEcx = upstream.DerivedBacklinkEntryECX;

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan FinishWithoutChild(
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome outcome,
            bool exactCleanExitAdmitted) =>
            new(
                capture,
                upstream,
                null,
                outcome,
                FinalFrameGateReached: false,
                ExactCleanHandoffToFinalFrameGate: false,
                ChildEntryEBP: null,
                ChildEntryECX: null,
                ChildEntryECXSource:
                    CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource.None,
                ChildECXCarriedToCompare: false,
                UnreachedFreshFrameMinus14CaptureProvided: freshFrame14Provided,
                LateFreshFrameMinus14CaptureProvided: freshFrame14Provided,
                UnreachedDerivedChildECXAvailable: derivedChildEcx.HasValue,
                LateDerivedChildECXAvailable: derivedChildEcx.HasValue,
                ExactCleanExitAdmitted: exactCleanExitAdmitted && !freshFrame14Provided,
                StopAtRawFileOffsetExclusive: upstream.StopAtRawFileOffsetExclusive);

        if (upstream.Outcome is
            CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UpstreamEqualTo9160
            or CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                .UpstreamEqualTo9160WithDownstreamCapture)
        {
            return FinishWithoutChild(
                CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .UpstreamIndexedEqualTo9160,
                upstream.ExactCleanExitAdmitted);
        }

        if (upstream.Outcome
            != CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.EqualTo9160)
        {
            return FinishWithoutChild(
                CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .UpstreamBacklinkNotEqualTo9160,
                upstream.ExactCleanExitAdmitted);
        }

        if (!IsExactCleanBacklinkHandoff(upstream)
            || capture.IndexedBacklinkCapture.UpstreamCapture.EntryEBP
                is not uint childEbp
            || derivedChildEcx is not uint childEcx)
        {
            return FinishWithoutChild(
                CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .UpstreamBacklinkHandoffNotClean,
                exactCleanExitAdmitted: false);
        }

        CapacityFinalCalleeAlternateFinalFrameGatePlan final =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: childEbp,
                FreshFrameMinus14: capture.FreshCallerFrameMinus14,
                EntryECX: freshFrame14Provided ? childEcx : null));

        bool childEcxCarried = final.CompareReached
            && final.EntryECXCaptured
            && final.CompareRight == childEcx;
        bool derivedEcxHeldBack = derivedChildEcx.HasValue && !childEcxCarried;
        CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome outcome =
            final.Outcome switch
            {
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14 =>
                    CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                        .MissingFreshFrameMinus14,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryECX =>
                    CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                        .MissingDerivedChildECX,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                        .ChildEqualToFallthrough,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                        .ChildUnequalToBranchTarget,
                _ => CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .ChildFinalFrameGateBlocked
            };

        return new(
            capture,
            upstream,
            final,
            outcome,
            FinalFrameGateReached: true,
            ExactCleanHandoffToFinalFrameGate: true,
            ChildEntryEBP: final.EntryEBPCaptured ? childEbp : null,
            ChildEntryECX: childEcxCarried ? childEcx : null,
            ChildEntryECXSource: childEcxCarried
                ? CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource
                    .BacklinkEntryECX
                : CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource.None,
            ChildECXCarriedToCompare: childEcxCarried,
            UnreachedFreshFrameMinus14CaptureProvided:
                final.UnreachedFreshFrameMinus14CaptureProvided,
            LateFreshFrameMinus14CaptureProvided:
                final.LateFreshFrameMinus14CaptureProvided,
            UnreachedDerivedChildECXAvailable: derivedEcxHeldBack,
            LateDerivedChildECXAvailable: derivedEcxHeldBack,
            ExactCleanExitAdmitted:
                childEcxCarried
                && final.ExactCleanExitAdmitted
                && (final.Outcome is
                    CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough
                    or CapacityFinalCalleeAlternateFinalFrameGateOutcome
                        .UnequalToBranchTarget),
            StopAtRawFileOffsetExclusive: final.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanBacklinkHandoff(
        CapacityFinalCalleeIndexedBacklinkLifecyclePlan plan)
    {
        CapacityFinalCalleeAlternateIndexedFrameGatePlan indexed = plan.Upstream;
        CapacityFinalCalleeAlternateBacklinkGatePlan? backlink = plan.BacklinkGate;
        return plan.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.EqualTo9160
            && plan.BacklinkGateReached
            && plan.ExactCleanHandoffToBacklinkGate
            && plan.ExactCleanExitAdmitted
            && plan.ExactCleanBypassAdmitted
            && plan.StopAtRawFileOffsetExclusive == FinalFrameEntryRawFileOffset
            && indexed.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough
            && indexed.ExactCleanRouteAdmitted
            && indexed.JeReached
            && indexed.JeTaken == false
            && indexed.JeSelectedDestinationRawFileOffset
                == CapacityFinalCalleeIndexedBacklinkLifecycleProjection
                    .IndexedUnequalDestinationRawFileOffset
            && indexed.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeIndexedBacklinkLifecycleProjection
                    .IndexedUnequalDestinationRawFileOffset
            && indexed.Writes.Length == 1
            && backlink is
            {
                Outcome: CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget,
                JbReached: true,
                JbTaken: false,
                JzReached: true,
                JzTaken: true,
                ExactCleanExitAdmitted: true,
                ExactCleanBypassAdmitted: true,
                OpaqueCallReached: false
            }
            && backlink.JzSelectedDestinationRawFileOffset
                == FinalFrameEntryRawFileOffset
            && backlink.StopAtRawFileOffsetExclusive == FinalFrameEntryRawFileOffset
            && backlink.Writes.IsEmpty
            && !plan.UnreachedCallerEBXCaptureProvided
            && !plan.UnreachedCallerEDXCaptureProvided
            && !plan.UnreachedFreshFieldCaptureProvided
            && !plan.LateCallerEDXCaptureProvided
            && !plan.LateFreshFieldCaptureProvided
            && !plan.UnusedCallerEDXCaptureProvided
            && !plan.UnusedFreshFieldCaptureProvided
            && plan.DerivedBacklinkEntryECX.HasValue
            && !plan.OpaqueCallReached;
    }
}
