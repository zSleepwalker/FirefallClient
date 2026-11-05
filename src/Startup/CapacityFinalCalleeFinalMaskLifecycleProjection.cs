namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFinalMaskLifecycleCapture(
    CapacityFinalCalleeAlternateFinalFrameGateCapture UpstreamFinalFrameCapture,
    uint? FreshFrameMinus8,
    uint? FreshFrameMinus4,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeFinalMaskLifecycleOutcome
{
    UpstreamMissingEntryEBP,
    UpstreamMissingFreshFrameMinus14,
    UpstreamMissingEntryECX,
    UpstreamUnequalTo917C,
    UpstreamRouteNotAdmitted,
    MaskMissingFreshFrameMinus8,
    MaskMissingFreshFrameMinus4,
    MaskMissingFreshGlobalValue,
    Reached92BD
}

public sealed record CapacityFinalCalleeFinalMaskLifecyclePlan(
    CapacityFinalCalleeFinalMaskLifecycleCapture Capture,
    CapacityFinalCalleeAlternateFinalFrameGatePlan UpstreamFinalFrameGate,
    CapacityFinalCalleeAlternateMaskTailPlan? MaskTail,
    CapacityFinalCalleeFinalMaskLifecycleOutcome Outcome)
{
    public bool MaskTailReached { get; init; }
    public bool ExactCleanHandoffToMaskTail { get; init; }
    public bool EntryEBPCarriedToMaskTail { get; init; }
    public uint? EntryEBPCarried { get; init; }
    public CapacityFinalCalleeAlternateMaskTailFlags? DerivedEntryFlagsForMaskHelper
        { get; init; }
    public bool UpstreamCompareFlagsDerivedForMaskHelper { get; init; }
    public bool UnreachedFreshFrameMinus8CaptureProvided { get; init; }
    public bool UnreachedFreshFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedFreshGlobalCaptureProvided { get; init; }
    public bool MaskHelperExactCleanExitAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean equal route at 9167 to the bounded mask helper ending at
/// 92BD, while admitting a clean unequal stop at 917C without entering it.
/// EBP is carried only on the equal route; the helper's frame and global
/// operands remain independent fresh captures.
/// </summary>
public static class CapacityFinalCalleeFinalMaskLifecycleProjection
{
    public const uint UpstreamEntryRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EntryRawFileOffset;
    public const uint MaskTailEntryRawFileOffset =
        CapacityFinalCalleeAlternateMaskTailProjection.EntryRawFileOffset;
    public const uint UpstreamEqualDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.EqualDestinationRawFileOffset;
    public const uint UpstreamUnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFinalFrameGateProjection.UnequalDestinationRawFileOffset;
    public const uint MaskTailSuccessorRawFileOffset =
        CapacityFinalCalleeAlternateMaskTailProjection.SuccessorRawFileOffset;

    public static CapacityFinalCalleeFinalMaskLifecyclePlan Project(
        CapacityFinalCalleeFinalMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateFinalFrameGatePlan upstream =
            CapacityFinalCalleeAlternateFinalFrameGateProjection.Project(
                capture.UpstreamFinalFrameCapture);

        if (IsExactCleanUnequalRoute(upstream))
        {
            bool maskCapturesProvided =
                capture.FreshFrameMinus8.HasValue
                || capture.FreshFrameMinus4.HasValue
                || capture.FreshGlobalValue.HasValue;
            return new(
                capture,
                upstream,
                null,
                CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamUnequalTo917C)
            {
                UnreachedFreshFrameMinus8CaptureProvided =
                    capture.FreshFrameMinus8.HasValue,
                UnreachedFreshFrameMinus4CaptureProvided =
                    capture.FreshFrameMinus4.HasValue,
                UnreachedFreshGlobalCaptureProvided =
                    capture.FreshGlobalValue.HasValue,
                ExactCleanExitAdmitted = !maskCapturesProvided,
                StopAtRawFileOffsetExclusive =
                    upstream.StopAtRawFileOffsetExclusive
            };
        }

        if (!IsExactCleanEqualRoute(upstream))
        {
            return new(
                capture,
                upstream,
                null,
                GetUnadmittedUpstreamOutcome(upstream))
            {
                UnreachedFreshFrameMinus8CaptureProvided =
                    capture.FreshFrameMinus8.HasValue,
                UnreachedFreshFrameMinus4CaptureProvided =
                    capture.FreshFrameMinus4.HasValue,
                UnreachedFreshGlobalCaptureProvided =
                    capture.FreshGlobalValue.HasValue,
                StopAtRawFileOffsetExclusive =
                    upstream.StopAtRawFileOffsetExclusive
            };
        }

        uint entryEbp = capture.UpstreamFinalFrameCapture.EntryEBP!.Value;
        uint freshFrameMinus4 = capture.FreshFrameMinus4.GetValueOrDefault();
        bool shiftWillBeReached =
            capture.FreshFrameMinus8.HasValue && capture.FreshFrameMinus4.HasValue;
        int effectiveShiftCount = (int)((byte)freshFrameMinus4 & 31);
        bool zeroCountShiftReached = shiftWillBeReached && effectiveShiftCount == 0;
        CapacityFinalCalleeAlternateMaskTailFlags? derivedEntryFlags =
            zeroCountShiftReached
                ? ConvertFlags(upstream.CompareFlags!.Value)
                : null;

        CapacityFinalCalleeAlternateMaskTailPlan maskTail =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(new(
                EntryEBP: entryEbp,
                FreshFrameMinus8: capture.FreshFrameMinus8,
                FreshFrameMinus4: capture.FreshFrameMinus4,
                FreshGlobalValue: capture.FreshGlobalValue,
                EntryFlags: derivedEntryFlags));

        CapacityFinalCalleeFinalMaskLifecycleOutcome outcome = maskTail.Outcome switch
        {
            CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus8 =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshFrameMinus8,
            CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshFrameMinus4,
            CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshGlobalValue,
            CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.Reached92BD,
            _ => CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamRouteNotAdmitted
        };

        bool maskHelperExactCleanExitAdmitted =
            IsExactCleanMaskHelperExit(maskTail, derivedEntryFlags);

        return new(
            capture,
            upstream,
            maskTail,
            outcome)
        {
            MaskTailReached = true,
            ExactCleanHandoffToMaskTail = true,
            EntryEBPCarriedToMaskTail =
                maskTail.EntryEBPCaptured
                && maskTail.Capture.EntryEBP == entryEbp
                && maskTail.FrameMinus8Address
                    == CapacityFinalCalleeAlternateMaskTailProjection
                        .DeriveEbpMinus8Address(entryEbp),
            EntryEBPCarried = maskTail.EntryEBPCaptured
                ? maskTail.Capture.EntryEBP
                : null,
            DerivedEntryFlagsForMaskHelper = derivedEntryFlags,
            UpstreamCompareFlagsDerivedForMaskHelper =
                zeroCountShiftReached && derivedEntryFlags.HasValue,
            UnreachedFreshFrameMinus8CaptureProvided =
                maskTail.UnreachedFreshFrameMinus8CaptureProvided,
            UnreachedFreshFrameMinus4CaptureProvided =
                maskTail.UnreachedFreshFrameMinus4CaptureProvided,
            UnreachedFreshGlobalCaptureProvided =
                maskTail.UnreachedFreshGlobalCaptureProvided,
            MaskHelperExactCleanExitAdmitted = maskHelperExactCleanExitAdmitted,
            ExactCleanExitAdmitted =
                maskHelperExactCleanExitAdmitted
                && IsExactCleanEqualRoute(upstream)
                && maskTail.FinalJumpSelectedDestinationRawFileOffset
                    == MaskTailSuccessorRawFileOffset,
            StopAtRawFileOffsetExclusive =
                maskTail.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactCleanEqualRoute(
        CapacityFinalCalleeAlternateFinalFrameGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough
        && plan.EntryEBPCaptured
        && plan.FrameMinus14Captured
        && plan.EntryECXCaptured
        && plan.CompareReached
        && plan.CompareFlags is { Zero: true }
        && plan.JnzReached
        && plan.JnzTaken == false
        && plan.JnzSelectedDestinationRawFileOffset
            == UpstreamEqualDestinationRawFileOffset
        && plan.ExactCleanExitAdmitted
        && !plan.UnreachedFreshFrameMinus14CaptureProvided
        && !plan.UnreachedEntryECXCaptureProvided
        && !plan.LateFreshFrameMinus14CaptureProvided
        && !plan.LateEntryECXCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == UpstreamEqualDestinationRawFileOffset;

    private static bool IsExactCleanUnequalRoute(
        CapacityFinalCalleeAlternateFinalFrameGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget
        && plan.EntryEBPCaptured
        && plan.FrameMinus14Captured
        && plan.EntryECXCaptured
        && plan.CompareReached
        && plan.CompareFlags is { Zero: false }
        && plan.JnzReached
        && plan.JnzTaken == true
        && plan.JnzSelectedDestinationRawFileOffset
            == UpstreamUnequalDestinationRawFileOffset
        && plan.ExactCleanExitAdmitted
        && !plan.UnreachedFreshFrameMinus14CaptureProvided
        && !plan.UnreachedEntryECXCaptureProvided
        && !plan.LateFreshFrameMinus14CaptureProvided
        && !plan.LateEntryECXCaptureProvided
        && plan.Writes.IsEmpty
        && plan.StopAtRawFileOffsetExclusive
            == UpstreamUnequalDestinationRawFileOffset;

    private static bool IsExactCleanMaskHelperExit(
        CapacityFinalCalleeAlternateMaskTailPlan plan,
        CapacityFinalCalleeAlternateMaskTailFlags? derivedEntryFlags)
    {
        bool zeroCountShiftReached =
            plan.ShiftReached && plan.EffectiveShiftCount == 0;
        bool validEntryFlags =
            zeroCountShiftReached
                ? derivedEntryFlags.HasValue
                    && plan.Capture.EntryFlags == derivedEntryFlags
                    && plan.EntryFlagsConsumed
                : !plan.Capture.EntryFlags.HasValue
                    && !plan.EntryFlagsConsumed
                    && !plan.UnusedEntryFlagsCaptureProvided;

        return plan.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor
            && plan.ExactCleanExitAdmitted
            && plan.EntryEBPCaptured
            && plan.FrameMinus8Captured
            && plan.FrameMinus4Captured
            && plan.ShiftReached
            && plan.NotReached
            && validEntryFlags
            && !plan.UnusedEntryFlagsCaptureProvided
            && !plan.UnreachedEntryFlagsCaptureProvided
            && !plan.UnreachedFreshFrameMinus8CaptureProvided
            && !plan.UnreachedFreshFrameMinus4CaptureProvided
            && !plan.UnreachedFreshGlobalCaptureProvided
            && plan.GlobalCaptured
            && plan.GlobalAndReached
            && plan.GlobalWriteReached
            && plan.GlobalWriteAddress
                == CapacityFinalCalleeAlternateMaskTailProjection.GlobalVirtualAddress
            && plan.Capture.FreshGlobalValue is uint freshGlobal
            && plan.NotResult is uint notResult
            && plan.GlobalWriteValue == (freshGlobal & notResult)
            && plan.FinalJumpReached
            && plan.FinalJumpTaken == true
            && plan.FinalJumpSelectedDestinationRawFileOffset
                == MaskTailSuccessorRawFileOffset
            && plan.StopAtRawFileOffsetExclusive == MaskTailSuccessorRawFileOffset;
    }

    private static CapacityFinalCalleeFinalMaskLifecycleOutcome
        GetUnadmittedUpstreamOutcome(
            CapacityFinalCalleeAlternateFinalFrameGatePlan plan) =>
        plan.Outcome switch
        {
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryEBP =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamMissingEntryEBP,
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome
                    .UpstreamMissingFreshFrameMinus14,
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryECX =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamMissingEntryECX,
            CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget =>
                CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamUnequalTo917C,
            _ => CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamRouteNotAdmitted
        };

    private static CapacityFinalCalleeAlternateMaskTailFlags ConvertFlags(
        CapacityFinalCalleeAlternateFinalFrameGateFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}
