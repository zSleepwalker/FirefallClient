namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescentClearLifecycleCapture(
    CapacityFinalCalleeFieldDescentStepCapture DescentCapture,
    uint? FreshGlobalForEntryCompare,
    uint? FreshFrameMinus12,
    uint? FreshFrameMinus4ForZeroStoreAddress,
    uint? FreshGlobalForTailLoad);

public enum CapacityFinalCalleeDescentClearLifecycleOutcome
{
    DescentBlocked,
    DescentBoundaryContaminated,
    LoopbackBounded,
    LoopbackContaminated,
    ClearTailIncomplete,
    ClearTailContaminated,
    ClearBelowDestination,
    ClearTailCompleted
}

public readonly record struct CapacityFinalCalleeDescentClearLifecyclePlan(
    CapacityFinalCalleeDescentClearLifecycleCapture Capture,
    CapacityFinalCalleeFieldDescentStepPlan DescentPlan,
    CapacityFinalCalleeFieldClearTailPlan? ClearTailPlan,
    CapacityFinalCalleeDescentClearLifecycleOutcome Outcome,
    bool ClearTailCaptureSupplied,
    bool UnreachedClearTailCaptureProvided,
    bool ClearTailReached,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins only a clean terminal descent at FDD to the field-clear tail. A
/// descent loopback remains a single bounded step and is never iterated.
/// </summary>
public static class CapacityFinalCalleeDescentClearLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.EntryRawFileOffset;
    public const uint TerminalRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.TerminalRawFileOffset;
    public const uint LoopbackRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.LoopbackRawFileOffset;
    public const uint BelowDestinationRawFileOffset =
        CapacityFinalCalleeFieldClearTailProjection.BelowDestinationRawFileOffset;
    public const uint ClearTailCompletionRawFileOffsetExclusive =
        CapacityFinalCalleeFieldClearTailProjection.TailCompletionRawFileOffset;

    public static CapacityFinalCalleeDescentClearLifecyclePlan Project(
        CapacityFinalCalleeDescentClearLifecycleCapture capture)
    {
        CapacityFinalCalleeFieldDescentStepPlan descent =
            CapacityFinalCalleeFieldDescentStepProjection.Project(
                capture.DescentCapture);
        bool clearTailCaptureSupplied = HasClearTailCapture(capture);

        if (descent.Outcome
            == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback)
        {
            bool cleanLoopback = IsCleanLoopback(descent);
            return new(
                capture,
                descent,
                ClearTailPlan: null,
                cleanLoopback && !clearTailCaptureSupplied
                    ? CapacityFinalCalleeDescentClearLifecycleOutcome.LoopbackBounded
                    : CapacityFinalCalleeDescentClearLifecycleOutcome.LoopbackContaminated,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                ClearTailReached: false,
                Admitted: cleanLoopback && !clearTailCaptureSupplied,
                descent.StopAtRawFileOffsetExclusive);
        }

        if (descent.Outcome
            == CapacityFinalCalleeFieldDescentStepOutcome.LoopbackHasUnreachedCaptures)
        {
            return new(
                capture,
                descent,
                ClearTailPlan: null,
                CapacityFinalCalleeDescentClearLifecycleOutcome.LoopbackContaminated,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                ClearTailReached: false,
                Admitted: false,
                descent.StopAtRawFileOffsetExclusive);
        }

        if (descent.Outcome
            == CapacityFinalCalleeFieldDescentStepOutcome.TerminalHasUnreachedCaptures)
        {
            return new(
                capture,
                descent,
                ClearTailPlan: null,
                CapacityFinalCalleeDescentClearLifecycleOutcome
                    .DescentBoundaryContaminated,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                ClearTailReached: false,
                Admitted: false,
                descent.StopAtRawFileOffsetExclusive);
        }

        if (!IsCleanTerminal(descent))
        {
            return new(
                capture,
                descent,
                ClearTailPlan: null,
                CapacityFinalCalleeDescentClearLifecycleOutcome.DescentBlocked,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                ClearTailReached: false,
                Admitted: false,
                descent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeFieldClearTailCapture clearTailCapture = new(
            ValidatedEntryEDX: descent.EDXLoadedFromFrameMinus4,
            ValidatedEntryEBP: capture.DescentCapture.ValidatedEntryEBP,
            FreshGlobalForEntryCompare: capture.FreshGlobalForEntryCompare,
            FreshFrameMinus12: capture.FreshFrameMinus12,
            FreshFrameMinus4ForZeroStoreAddress:
                capture.FreshFrameMinus4ForZeroStoreAddress,
            FreshGlobalForTailLoad: capture.FreshGlobalForTailLoad);
        CapacityFinalCalleeFieldClearTailPlan clearTail =
            CapacityFinalCalleeFieldClearTailProjection.Project(clearTailCapture);

        bool clearTailClean = HasNoUnreachedClearTailCaptures(clearTail);
        if (clearTail.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.EntryEDXUnsignedBelowGlobal
            && clearTail.StopAtRawFileOffsetExclusive == BelowDestinationRawFileOffset
            && clearTail.SelectedBranchDestinationRawFileOffset
                == BelowDestinationRawFileOffset
            && clearTail.BranchTaken == true
            && clearTailClean)
        {
            return new(
                capture,
                descent,
                clearTail,
                CapacityFinalCalleeDescentClearLifecycleOutcome.ClearBelowDestination,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: false,
                ClearTailReached: true,
                Admitted: true,
                BelowDestinationRawFileOffset);
        }

        if (clearTail.Outcome
                == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
            && clearTail.StopAtRawFileOffsetExclusive
                == ClearTailCompletionRawFileOffsetExclusive
            && clearTail.BranchTaken == false
            && clearTailClean)
        {
            return new(
                capture,
                descent,
                clearTail,
                CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailCompleted,
                clearTailCaptureSupplied,
                UnreachedClearTailCaptureProvided: false,
                ClearTailReached: true,
                Admitted: true,
                ClearTailCompletionRawFileOffsetExclusive);
        }

        bool hasUnreachedClearTailCapture = !clearTailClean;
        return new(
            capture,
            descent,
            clearTail,
            hasUnreachedClearTailCapture
                ? CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailContaminated
                : CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailIncomplete,
            clearTailCaptureSupplied,
            UnreachedClearTailCaptureProvided: hasUnreachedClearTailCapture,
            ClearTailReached: true,
            Admitted: false,
            clearTail.StopAtRawFileOffsetExclusive);
    }

    private static bool IsCleanTerminal(
        CapacityFinalCalleeFieldDescentStepPlan plan) =>
        plan.Outcome == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
        && plan.TerminalReached
        && plan.StopAtRawFileOffsetExclusive == TerminalRawFileOffset
        && plan.EDXLoadedFromFrameMinus4.HasValue
        && !plan.AnyUnreachedCaptureProvided
        && !plan.UnreachedValidatedEntryEAXCaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshEaxPlus14CaptureProvided
        && !plan.UnreachedFreshEaxPlus10CaptureProvided
        && !plan.UnreachedFreshEcxCaptureProvided;

    private static bool IsCleanLoopback(
        CapacityFinalCalleeFieldDescentStepPlan plan) =>
        plan.LoopbackReached
        && plan.StopAtRawFileOffsetExclusive == LoopbackRawFileOffset
        && !plan.AnyUnreachedCaptureProvided
        && !plan.UnreachedValidatedEntryEAXCaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshEaxPlus14CaptureProvided
        && !plan.UnreachedFreshEaxPlus10CaptureProvided
        && !plan.UnreachedFreshEcxCaptureProvided;

    private static bool HasClearTailCapture(
        CapacityFinalCalleeDescentClearLifecycleCapture capture) =>
        capture.FreshGlobalForEntryCompare.HasValue
        || capture.FreshFrameMinus12.HasValue
        || capture.FreshFrameMinus4ForZeroStoreAddress.HasValue
        || capture.FreshGlobalForTailLoad.HasValue;

    private static bool HasNoUnreachedClearTailCaptures(
        CapacityFinalCalleeFieldClearTailPlan plan) =>
        !plan.UnreachedValidatedEntryEBPCaptureProvided
        && !plan.UnreachedFreshGlobalForEntryCompareCaptureProvided
        && !plan.UnreachedFreshFrameMinus12CaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshGlobalForTailLoadCaptureProvided;
}
