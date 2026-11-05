using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePairedFrameGateLifecycleCapture(
    CapacityFinalCalleeAlternateFrameLoadsCapture AlternateFrameLoadsCapture,
    uint? FreshFrameMinus10);

public enum CapacityFinalCalleePairedFrameGateLifecycleOutcome
{
    AlternateFrameLoadsIncomplete,
    AlternateFrameLoadsIncompleteWithDownstreamCapture,
    AlternateFrameLoadsEqualToOpaqueCallBoundary,
    AlternateFrameLoadsEqualWithDownstreamCapture,
    MissingSecondGateFreshFrameMinus10,
    SecondGateEqualToOpaqueCallBoundary,
    SecondGateUnequalTo912D
}

public sealed record CapacityFinalCalleePairedFrameGateLifecyclePlan(
    CapacityFinalCalleePairedFrameGateLifecycleCapture Capture,
    CapacityFinalCalleeAlternateFrameLoadsPlan AlternateFrameLoads,
    CapacityFinalCalleeAlternateSecondFrameGatePlan? AlternateSecondFrameGate,
    bool AlternateSecondFrameGateReached,
    bool ExactCleanHandoffToAlternateSecondFrameGate,
    uint? DerivedSecondGateEntryEDX,
    uint? DerivedSecondGateEntryEBP,
    bool UnreachedFreshFrameMinus10CaptureProvided,
    CapacityFinalCalleePairedFrameGateLifecycleOutcome Outcome,
    bool ExactCleanExitAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsWrite> OrderedUpstreamWrites =>
        AlternateFrameLoads.Writes;

    public bool FirstOpaqueCallReached => false;
    public bool SecondOpaqueCallReached => false;
}

public static class CapacityFinalCalleePairedFrameGateLifecycleProjection
{
    public const uint FirstOpaqueCallBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateFrameLoadsProjection.OpaqueCallRawFileOffset;
    public const uint AlternateSecondFrameGateEntryRawFileOffset =
        CapacityFinalCalleeAlternateSecondFrameGateProjection.EntryRawFileOffset;
    public const uint EqualSecondGateBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateSecondFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint UnequalSecondGateBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateSecondFrameGateProjection.UnequalDestinationRawFileOffset;

    public static CapacityFinalCalleePairedFrameGateLifecyclePlan Project(
        CapacityFinalCalleePairedFrameGateLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateFrameLoadsPlan alternateFrameLoads =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(
                capture.AlternateFrameLoadsCapture);
        bool downstreamFreshProvided = capture.FreshFrameMinus10.HasValue;

        CapacityFinalCalleePairedFrameGateLifecyclePlan Finish(
            CapacityFinalCalleePairedFrameGateLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateSecondFrameGatePlan? secondGate = null,
            bool secondGateReached = false,
            bool exactHandoff = false,
            uint? derivedEdx = null,
            uint? derivedEbp = null,
            bool unreachedFresh = false,
            bool exactCleanExitAdmitted = false) =>
            new(
                capture,
                alternateFrameLoads,
                secondGate,
                secondGateReached,
                exactHandoff,
                derivedEdx,
                derivedEbp,
                unreachedFresh,
                outcome,
                exactCleanExitAdmitted,
                stopAt);

        if (IsExactCleanEqualBoundary(alternateFrameLoads, capture.AlternateFrameLoadsCapture))
        {
            bool contaminated = downstreamFreshProvided;
            return Finish(
                contaminated
                    ? CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .AlternateFrameLoadsEqualWithDownstreamCapture
                    : CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .AlternateFrameLoadsEqualToOpaqueCallBoundary,
                FirstOpaqueCallBoundaryRawFileOffsetExclusive,
                unreachedFresh: contaminated,
                exactCleanExitAdmitted: !contaminated);
        }

        if (!IsExactCleanUnequalHandoff(
                alternateFrameLoads,
                capture.AlternateFrameLoadsCapture)
            || capture.AlternateFrameLoadsCapture.EntryEDX is not uint entryEdx
            || capture.AlternateFrameLoadsCapture.EntryEBP is not uint entryEbp)
        {
            return Finish(
                downstreamFreshProvided
                    ? CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .AlternateFrameLoadsIncompleteWithDownstreamCapture
                    : CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .AlternateFrameLoadsIncomplete,
                alternateFrameLoads.StopAtRawFileOffsetExclusive,
                unreachedFresh: downstreamFreshProvided);
        }

        CapacityFinalCalleeAlternateSecondFrameGatePlan secondGate =
            CapacityFinalCalleeAlternateSecondFrameGateProjection.Project(new(
                EntryEDX: entryEdx,
                EntryEBP: entryEbp,
                FreshFrameMinus10: capture.FreshFrameMinus10));

        if (secondGate.Outcome
            == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingFreshFrameMinus10)
        {
            return Finish(
                CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .MissingSecondGateFreshFrameMinus10,
                secondGate.StopAtRawFileOffsetExclusive,
                secondGate,
                secondGateReached: true,
                exactHandoff: true,
                derivedEdx: entryEdx,
                derivedEbp: entryEbp);
        }

        if (capture.FreshFrameMinus10 is uint freshFrameMinus10
            && (IsExactSecondGateEqualBoundary(
                    secondGate, entryEdx, entryEbp, freshFrameMinus10)
                || IsExactSecondGateUnequalBoundary(
                    secondGate, entryEdx, entryEbp, freshFrameMinus10)))
        {
            bool equalBoundary = IsExactSecondGateEqualBoundary(
                secondGate, entryEdx, entryEbp, freshFrameMinus10);
            return Finish(
                equalBoundary
                    ? CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .SecondGateEqualToOpaqueCallBoundary
                    : CapacityFinalCalleePairedFrameGateLifecycleOutcome
                        .SecondGateUnequalTo912D,
                secondGate.StopAtRawFileOffsetExclusive,
                secondGate,
                secondGateReached: true,
                exactHandoff: true,
                derivedEdx: entryEdx,
                derivedEbp: entryEbp,
                exactCleanExitAdmitted: true);
        }

        return Finish(
            CapacityFinalCalleePairedFrameGateLifecycleOutcome
                .AlternateFrameLoadsIncomplete,
            secondGate.StopAtRawFileOffsetExclusive,
            secondGate,
            secondGateReached: true,
            exactHandoff: true,
            derivedEdx: entryEdx,
            derivedEbp: entryEbp);
    }

    private static bool IsExactCleanEqualBoundary(
        CapacityFinalCalleeAlternateFrameLoadsPlan plan,
        CapacityFinalCalleeAlternateFrameLoadsCapture capture) =>
        IsExactCleanBranch(
            plan,
            capture,
            CapacityFinalCalleeAlternateFrameLoadsOutcome
                .CompareEqualToOpaqueCallBoundary,
            branchTaken: false,
            destination: CapacityFinalCalleeAlternateFrameLoadsProjection
                .OpaqueCallRawFileOffset);

    private static bool IsExactCleanUnequalHandoff(
        CapacityFinalCalleeAlternateFrameLoadsPlan plan,
        CapacityFinalCalleeAlternateFrameLoadsCapture capture) =>
        IsExactCleanBranch(
            plan,
            capture,
            CapacityFinalCalleeAlternateFrameLoadsOutcome
                .CompareUnequalToBranchTarget,
            branchTaken: true,
            destination: CapacityFinalCalleeAlternateFrameLoadsProjection
                .UnequalDestinationRawFileOffset);

    private static bool IsExactCleanBranch(
        CapacityFinalCalleeAlternateFrameLoadsPlan plan,
        CapacityFinalCalleeAlternateFrameLoadsCapture capture,
        CapacityFinalCalleeAlternateFrameLoadsOutcome expectedOutcome,
        bool branchTaken,
        uint destination)
    {
        if (capture.EntryEDX is not uint entryEdx
            || capture.EntryEBP is not uint entryEbp
            || capture.FreshEdxPlus8 is not uint freshEdxPlus8
            || capture.FreshEdxPlusC is not uint freshEdxPlusC)
        {
            return false;
        }

        return plan.Outcome == expectedOutcome
            && plan.EntryEDXCaptured
            && plan.FreshEdxPlus8Captured
            && plan.EntryEBPCaptured
            && plan.FreshEdxPlusCCaptured
            && plan.FirstWriteReached
            && plan.FirstWriteAddress
                == CapacityFinalCalleeAlternateFrameLoadsProjection
                    .DeriveEbpMinus10Address(entryEbp)
            && plan.FirstWriteValue == freshEdxPlus8
            && plan.SecondWriteReached
            && plan.SecondWriteAddress
                == CapacityFinalCalleeAlternateFrameLoadsProjection
                    .DeriveEbpMinus14Address(entryEbp)
            && plan.SecondWriteValue == freshEdxPlusC
            && plan.CompareReached
            && plan.CompareLeft == entryEdx
            && plan.CompareRight == freshEdxPlusC
            && plan.CompareResult == unchecked(entryEdx - freshEdxPlusC)
            && plan.CompareFlags is { } flags
            && flags.Zero == !branchTaken
            && plan.BranchReached
            && plan.BranchTaken == branchTaken
            && plan.BranchSelectedDestinationRawFileOffset == destination
            && plan.Writes.SequenceEqual(ImmutableArray.Create(
                new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    CapacityFinalCalleeAlternateFrameLoadsProjection
                        .FirstWriteRawFileOffset,
                    CapacityFinalCalleeAlternateFrameLoadsProjection
                        .DeriveEbpMinus10Address(entryEbp),
                    freshEdxPlus8),
                new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    CapacityFinalCalleeAlternateFrameLoadsProjection
                        .SecondWriteRawFileOffset,
                    CapacityFinalCalleeAlternateFrameLoadsProjection
                        .DeriveEbpMinus14Address(entryEbp),
                    freshEdxPlusC)))
            && !plan.UnreachedEntryEBPCaptureProvided
            && !plan.UnreachedFreshEdxPlus8CaptureProvided
            && !plan.UnreachedFreshEdxPlusCCaptureProvided
            && plan.ExactCleanExitAdmitted
            && !plan.OpaqueCallReached
            && plan.StopAtRawFileOffsetExclusive == destination;
    }

    private static bool IsExactSecondGateEqualBoundary(
        CapacityFinalCalleeAlternateSecondFrameGatePlan plan,
        uint entryEdx,
        uint entryEbp,
        uint freshFrameMinus10) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateSecondFrameGateOutcome
                .EqualToOpaqueCallBoundary
        && IsExactSecondGateCompare(plan, entryEdx, entryEbp, freshFrameMinus10)
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == EqualSecondGateBoundaryRawFileOffsetExclusive
        && plan.ExactCleanExitAdmitted
        && !plan.OpaqueCallReached
        && plan.StopAtRawFileOffsetExclusive
            == EqualSecondGateBoundaryRawFileOffsetExclusive
        && !plan.UnreachedEntryEBPCaptureProvided
        && !plan.LateFreshFrameMinus10CaptureProvided;

    private static bool IsExactSecondGateUnequalBoundary(
        CapacityFinalCalleeAlternateSecondFrameGatePlan plan,
        uint entryEdx,
        uint entryEbp,
        uint freshFrameMinus10) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateSecondFrameGateOutcome
                .UnequalToBranchTarget
        && IsExactSecondGateCompare(plan, entryEdx, entryEbp, freshFrameMinus10)
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == UnequalSecondGateBoundaryRawFileOffsetExclusive
        && plan.ExactCleanExitAdmitted
        && !plan.OpaqueCallReached
        && plan.StopAtRawFileOffsetExclusive
            == UnequalSecondGateBoundaryRawFileOffsetExclusive
        && !plan.UnreachedEntryEBPCaptureProvided
        && !plan.LateFreshFrameMinus10CaptureProvided;

    private static bool IsExactSecondGateCompare(
        CapacityFinalCalleeAlternateSecondFrameGatePlan plan,
        uint entryEdx,
        uint entryEbp,
        uint freshFrameMinus10)
    {
        uint result = unchecked(entryEdx - freshFrameMinus10);
        return plan.EntryEDXCaptured
            && plan.Capture.EntryEDX == entryEdx
            && plan.EntryEBPCaptured
            && plan.Capture.EntryEBP == entryEbp
            && plan.FrameMinus10ReadReached
            && plan.FrameMinus10Address
                == CapacityFinalCalleeAlternateSecondFrameGateProjection
                    .DeriveEbpMinus10Address(entryEbp)
            && plan.FrameMinus10Captured
            && plan.FrameMinus10 == freshFrameMinus10
            && plan.CompareReached
            && plan.CompareLeft == entryEdx
            && plan.CompareRight == freshFrameMinus10
            && plan.CompareResult == result
            && plan.CompareFlags == CalculateCompareFlags(
                entryEdx, freshFrameMinus10, result);
    }

    private static CapacityFinalCalleeAlternateSecondFrameGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
}
