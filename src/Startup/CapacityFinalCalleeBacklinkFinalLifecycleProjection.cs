using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeBacklinkFinalLifecycleCapture(
    CapacityFinalCalleeAlternateBacklinkGateCapture UpstreamBacklinkCapture,
    uint? CallerEBP,
    uint? FreshCallerFrameMinus14);

public enum CapacityFinalCalleeBacklinkFinalLifecycleOutcome
{
    UpstreamMissingEntryECX,
    UpstreamMissingEntryEBX,
    UpstreamBelowToOpaqueCallBoundary,
    UpstreamMissingEntryEDX,
    UpstreamMissingFreshFieldAtEcxPlusC,
    UpstreamUnequalToOpaqueCallBoundary,
    UpstreamBypassNotAdmitted,
    MissingCallerEBP,
    MissingFreshCallerFrameMinus14,
    MissingDerivedChildECX,
    ChildEqualToFallthrough,
    ChildUnequalToBranchTarget
}

public enum CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
{
    UpstreamFreshFieldAtEcxPlusC,
    ChildFreshFrameMinus14
}

public sealed record CapacityFinalCalleeBacklinkFinalLifecyclePlan(
    CapacityFinalCalleeBacklinkFinalLifecycleCapture Capture,
    CapacityFinalCalleeAlternateBacklinkGatePlan UpstreamBacklinkGate,
    CapacityFinalCalleeAlternateFinalFrameGatePlan? FinalFrameGate,
    bool FinalFrameGateReached,
    bool ExactCleanHandoffToFinalFrameGate,
    uint? UpstreamEntryECX,
    uint? DerivedChildECX,
    bool ChildECXCarriedToCompare,
    bool UnreachedCallerEBPCaptureProvided,
    bool UnreachedFreshCallerFrameMinus14CaptureProvided,
    bool LateCallerEBPCaptureProvided,
    bool LateFreshCallerFrameMinus14CaptureProvided,
    bool UnreachedDerivedChildECXAvailable,
    bool LateDerivedChildECXAvailable,
    CapacityFinalCalleeBacklinkFinalLifecycleOutcome Outcome,
    bool ExactCleanExitAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public ImmutableArray<CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead>
        OrderedReachedMemoryReads
    {
        get
        {
            ImmutableArray<CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead>.Builder reads =
                ImmutableArray.CreateBuilder<
                    CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead>(2);
            if (UpstreamBacklinkGate.FreshFieldReadReached)
            {
                reads.Add(
                    CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                        .UpstreamFreshFieldAtEcxPlusC);
            }

            if (FinalFrameGate?.FrameMinus14ReadReached == true)
            {
                reads.Add(
                    CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                        .ChildFreshFrameMinus14);
            }

            return reads.ToImmutable();
        }
    }

    public CapacityFinalCalleeAlternateBacklinkGateFlags? UpstreamFirstCompareFlags =>
        UpstreamBacklinkGate.FirstCompareFlags;

    public CapacityFinalCalleeAlternateBacklinkGateFlags? UpstreamSecondCompareFlags =>
        UpstreamBacklinkGate.SecondCompareFlags;

    public CapacityFinalCalleeAlternateFinalFrameGateFlags? ChildCompareFlags =>
        FinalFrameGate?.CompareFlags;

    public bool ChildCompareReached => FinalFrameGate?.CompareReached == true;
    public bool ChildJnzReached => FinalFrameGate?.JnzReached == true;
    public bool? ChildJnzTaken => FinalFrameGate?.JnzTaken;
    public uint? ChildJnzSelectedDestinationRawFileOffset =>
        FinalFrameGate?.JnzSelectedDestinationRawFileOffset;

    public bool OpaqueCallReached => false;
}

/// <summary>
/// Joins the clean 9160 equality bypass from the backlink gate to the fresh
/// EBP-14 comparison. Caller EBP and its fresh frame value remain independent
/// captures; upstream ECX is forwarded only when the child CMP is reached.
/// </summary>
public static class CapacityFinalCalleeBacklinkFinalLifecycleProjection
{
    public const uint UpstreamEntryRawFileOffset =
        CapacityFinalCalleeAlternateBacklinkGateProjection.EntryRawFileOffset;
    public const uint FinalFrameEntryRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset;
    public const uint OpaqueCallBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateBacklinkGateProjection.OpaqueCallRawFileOffset;
    public const uint FinalFrameEqualBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint FinalFrameUnequalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset;

    public static CapacityFinalCalleeBacklinkFinalLifecyclePlan Project(
        CapacityFinalCalleeBacklinkFinalLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateBacklinkGatePlan upstream =
            CapacityFinalCalleeAlternateBacklinkGateProjection.Project(
                capture.UpstreamBacklinkCapture);
        bool callerEbpProvided = capture.CallerEBP.HasValue;
        bool freshFrameProvided = capture.FreshCallerFrameMinus14.HasValue;
        uint? upstreamEcx = capture.UpstreamBacklinkCapture.EntryECX;

        CapacityFinalCalleeBacklinkFinalLifecyclePlan Finish(
            CapacityFinalCalleeBacklinkFinalLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateFinalFrameGatePlan? finalFrameGate = null,
            bool finalFrameGateReached = false,
            bool exactHandoff = false,
            uint? derivedChildEcx = null,
            bool childEcxCarried = false,
            bool unreachedCallerEbp = false,
            bool unreachedFreshFrame = false,
            bool lateCallerEbp = false,
            bool lateFreshFrame = false,
            bool unreachedDerivedEcx = false,
            bool lateDerivedEcx = false,
            bool exactCleanExitAdmitted = false) =>
            new(
                capture,
                upstream,
                finalFrameGate,
                finalFrameGateReached,
                exactHandoff,
                upstreamEcx,
                derivedChildEcx,
                childEcxCarried,
                unreachedCallerEbp,
                unreachedFreshFrame,
                lateCallerEbp,
                lateFreshFrame,
                unreachedDerivedEcx,
                lateDerivedEcx,
                outcome,
                exactCleanExitAdmitted,
                stopAt);

        if (!IsExactCleanBypass(upstream, capture.UpstreamBacklinkCapture))
        {
            CapacityFinalCalleeBacklinkFinalLifecycleOutcome upstreamOutcome =
                GetUnadmittedUpstreamOutcome(upstream);
            bool downstreamCapturesProvided = callerEbpProvided || freshFrameProvided;
            bool unreachedDerivedEcx = upstreamEcx.HasValue;
            return Finish(
                upstreamOutcome,
                upstream.StopAtRawFileOffsetExclusive,
                unreachedCallerEbp: callerEbpProvided,
                unreachedFreshFrame: freshFrameProvided,
                lateCallerEbp: callerEbpProvided,
                lateFreshFrame: freshFrameProvided,
                unreachedDerivedEcx: unreachedDerivedEcx,
                lateDerivedEcx: unreachedDerivedEcx,
                exactCleanExitAdmitted:
                    upstream.ExactCleanExitAdmitted && !downstreamCapturesProvided);
        }

        CapacityFinalCalleeAlternateFinalFrameGatePlan finalFrameGate =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(new(
                EntryEBP: capture.CallerEBP,
                FreshFrameMinus14: capture.FreshCallerFrameMinus14,
                EntryECX: callerEbpProvided && freshFrameProvided
                    ? upstreamEcx
                    : null));

        uint? derivedChildEcx = finalFrameGate.CompareReached
            ? upstreamEcx
            : null;
        bool childEcxCarried = finalFrameGate.CompareReached
            && upstreamEcx.HasValue
            && finalFrameGate.EntryECXCaptured
            && finalFrameGate.CompareRight == upstreamEcx;
        bool derivedEcxHeldBack = upstreamEcx.HasValue && !childEcxCarried;

        CapacityFinalCalleeBacklinkFinalLifecycleOutcome outcome =
            finalFrameGate.Outcome switch
            {
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryEBP =>
                    CapacityFinalCalleeBacklinkFinalLifecycleOutcome.MissingCallerEBP,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14 =>
                    CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                        .MissingFreshCallerFrameMinus14,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryECX =>
                    CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                        .MissingDerivedChildECX,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeBacklinkFinalLifecycleOutcome.ChildEqualToFallthrough,
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                        .ChildUnequalToBranchTarget,
                _ => CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamBypassNotAdmitted
            };

        return Finish(
            outcome,
            finalFrameGate.StopAtRawFileOffsetExclusive,
            finalFrameGate,
            finalFrameGateReached: true,
            exactHandoff: true,
            derivedChildEcx: derivedChildEcx,
            childEcxCarried: childEcxCarried,
            unreachedFreshFrame:
                finalFrameGate.UnreachedFreshFrameMinus14CaptureProvided,
            lateFreshFrame:
                finalFrameGate.LateFreshFrameMinus14CaptureProvided,
            unreachedDerivedEcx: derivedEcxHeldBack,
            lateDerivedEcx: derivedEcxHeldBack,
            exactCleanExitAdmitted:
                childEcxCarried && finalFrameGate.ExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeBacklinkFinalLifecycleOutcome
        GetUnadmittedUpstreamOutcome(
            CapacityFinalCalleeAlternateBacklinkGatePlan plan) =>
        plan.Outcome switch
        {
            CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome.UpstreamMissingEntryECX,
            CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEBX =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome.UpstreamMissingEntryEBX,
            CapacityFinalCalleeAlternateBacklinkGateOutcome.BelowToOpaqueCallBoundary =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamBelowToOpaqueCallBoundary,
            CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEDX =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome.UpstreamMissingEntryEDX,
            CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingFreshFieldAtEcxPlusC =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamMissingFreshFieldAtEcxPlusC,
            CapacityFinalCalleeAlternateBacklinkGateOutcome.UnequalToOpaqueCallBoundary =>
                CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamUnequalToOpaqueCallBoundary,
            _ => CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                .UpstreamBypassNotAdmitted
        };

    private static bool IsExactCleanBypass(
        CapacityFinalCalleeAlternateBacklinkGatePlan plan,
        CapacityFinalCalleeAlternateBacklinkGateCapture capture)
    {
        if (capture.EntryECX is not uint entryEcx
            || capture.EntryEBX is not uint entryEbx
            || capture.EntryEDX is not uint entryEdx
            || capture.FreshFieldAtEcxPlusC is not uint freshField)
        {
            return false;
        }

        return plan.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget
            && plan.EntryECXCaptured
            && plan.EntryEBXCaptured
            && plan.FirstCompareReached
            && plan.FirstCompareLeft == entryEcx
            && plan.FirstCompareRight == entryEbx
            && plan.FirstCompareResult == unchecked(entryEcx - entryEbx)
            && plan.FirstCompareFlags is { Carry: false }
            && plan.JbReached
            && plan.JbTaken == false
            && plan.JbSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlternateBacklinkGateProjection
                    .SecondCompareRawFileOffset
            && plan.EntryEDXReadReached
            && plan.EntryEDXCaptured
            && plan.FreshFieldReadReached
            && plan.FreshFieldAddress
                == CapacityFinalCalleeAlternateBacklinkGateProjection
                    .DeriveEcxPlusCAddress(entryEcx)
            && plan.FreshFieldCaptured
            && plan.FreshFieldValue == freshField
            && plan.SecondCompareReached
            && plan.SecondCompareLeft == freshField
            && plan.SecondCompareRight == entryEdx
            && plan.SecondCompareResult == unchecked(freshField - entryEdx)
            && plan.SecondCompareFlags is { Zero: true }
            && plan.JzReached
            && plan.JzTaken == true
            && plan.JzSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlternateBacklinkGateProjection
                    .EqualDestinationRawFileOffset
            && plan.ExactCleanExitAdmitted
            && plan.ExactCleanBypassAdmitted
            && !plan.UnreachedEntryEBXCaptureProvided
            && !plan.LateEntryEDXCaptureProvided
            && !plan.LateFreshFieldCaptureProvided
            && !plan.UnusedEntryEDXCaptureProvided
            && !plan.UnusedFreshFieldCaptureProvided
            && !plan.OpaqueCallReached
            && plan.Writes.IsEmpty
            && plan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlternateBacklinkGateProjection
                    .EqualDestinationRawFileOffset;
    }
}
