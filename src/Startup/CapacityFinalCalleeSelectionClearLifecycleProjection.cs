namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSelectionClearLifecycleCapture(
    CapacityFinalCalleeSelectionDescentLifecycleCapture SelectionDescentCapture,
    uint? FreshGlobalForEntryCompare,
    uint? FreshFrameMinus12,
    uint? FreshFrameMinus4ForZeroStoreAddress,
    uint? FreshGlobalForTailLoad);

public enum CapacityFinalCalleeSelectionClearLifecycleOutcome
{
    SelectionDescentBlocked,
    SelectionFfeBypass,
    SelectionFfeBypassContaminated,
    SelectionFc5Loopback,
    SelectionFc5LoopbackContaminated,
    TerminalDescentBlocked,
    TerminalDescentContaminated,
    ClearTailIncomplete,
    ClearTailContaminated,
    ClearBelowDestination,
    ClearTailCompleted
}

public readonly record struct CapacityFinalCalleeSelectionClearLifecyclePlan(
    CapacityFinalCalleeSelectionClearLifecycleCapture Capture,
    CapacityFinalCalleeSelectionDescentLifecyclePlan SelectionDescentPlan,
    CapacityFinalCalleeDescentClearLifecyclePlan? DescentClearPlan,
    CapacityFinalCalleeSelectionClearLifecycleOutcome Outcome,
    bool ExactTerminalSelectionDescent,
    bool ClearTailCaptureSupplied,
    bool UnreachedClearTailCaptureProvided,
    bool ClearTailReached,
    bool Admitted,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins an exact alternate selection and one clean terminal descent at FDD
/// to the field-clear tail. FFE bypasses and FC5 loopbacks remain bounded.
/// </summary>
public static class CapacityFinalCalleeSelectionClearLifecycleProjection
{
    public const uint SelectionEntryRawFileOffset =
        CapacityFinalCalleeAlternateFieldSelectionProjection.EntryRawFileOffset;
    public const uint DescentEntryRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.EntryRawFileOffset;
    public const uint LoopbackRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.LoopbackRawFileOffset;
    public const uint TerminalRawFileOffset =
        CapacityFinalCalleeFieldDescentStepProjection.TerminalRawFileOffset;
    public const uint FfeTargetRawFileOffset =
        CapacityFinalCalleeAlternateFieldSelectionProjection
            .SecondZeroDestinationRawFileOffset;
    public const uint BelowDestinationRawFileOffset =
        CapacityFinalCalleeFieldClearTailProjection.BelowDestinationRawFileOffset;
    public const uint ClearTailCompletionRawFileOffsetExclusive =
        CapacityFinalCalleeFieldClearTailProjection.TailCompletionRawFileOffset;

    public static CapacityFinalCalleeSelectionClearLifecyclePlan Project(
        CapacityFinalCalleeSelectionClearLifecycleCapture capture)
    {
        CapacityFinalCalleeSelectionDescentLifecyclePlan selectionDescent =
            CapacityFinalCalleeSelectionDescentLifecycleProjection.Project(
                capture.SelectionDescentCapture);
        bool clearTailCaptureSupplied = HasClearTailCapture(capture);
        CapacityFinalCalleeFieldDescentStepPlan? descent =
            selectionDescent.FieldDescentStepPlan;
        uint? selectedDestination = SelectedDestination(selectionDescent);

        if (selectionDescent.ExactFfeRouteSelected)
        {
            bool cleanBypass = selectionDescent.AlternateFieldSelectionClean
                && selectionDescent.Outcome
                    == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                        .AlternateSecondFreshEsiPlus10ZeroTarget
                && selectionDescent.Admitted
                && selectionDescent.StopAtRawFileOffsetExclusive
                    == FfeTargetRawFileOffset;
            bool admitted = cleanBypass && !clearTailCaptureSupplied;
            return BuildPlan(
                capture,
                selectionDescent,
                descentClear: null,
                outcome: admitted
                    ? CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .SelectionFfeBypass
                    : CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .SelectionFfeBypassContaminated,
                exactTerminalSelectionDescent: false,
                clearTailCaptureSupplied,
                unreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                clearTailReached: false,
                admitted,
                selectedDestination: FfeTargetRawFileOffset,
                stopAt: FfeTargetRawFileOffset);
        }

        bool exactFc2 = selectionDescent.ExactFC2RouteSelected;
        if (!exactFc2
            || descent is not CapacityFinalCalleeFieldDescentStepPlan descentStep)
        {
            return BuildPlan(
                capture,
                selectionDescent,
                descentClear: null,
                outcome: CapacityFinalCalleeSelectionClearLifecycleOutcome
                    .SelectionDescentBlocked,
                exactTerminalSelectionDescent: false,
                clearTailCaptureSupplied,
                unreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                clearTailReached: false,
                admitted: false,
                selectedDestination,
                stopAt: selectionDescent.StopAtRawFileOffsetExclusive);
        }

        if (descentStep.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            || descentStep.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.LoopbackHasUnreachedCaptures)
        {
            bool cleanLoopback =
                selectionDescent.Outcome
                    == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                        .FieldDescentReachedLoopback
                && selectionDescent.Admitted
                && selectionDescent.StopAtRawFileOffsetExclusive
                    == LoopbackRawFileOffset
                && !selectionDescent.AnyUnreachedDescentCaptureProvided
                && descentStep.LoopbackReached
                && !descentStep.AnyUnreachedCaptureProvided;
            bool admitted = cleanLoopback && !clearTailCaptureSupplied;
            return BuildPlan(
                capture,
                selectionDescent,
                descentClear: null,
                outcome: admitted
                    ? CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .SelectionFc5Loopback
                    : CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .SelectionFc5LoopbackContaminated,
                exactTerminalSelectionDescent: false,
                clearTailCaptureSupplied,
                unreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                clearTailReached: false,
                admitted,
                selectedDestination: LoopbackRawFileOffset,
                stopAt: LoopbackRawFileOffset);
        }

        if (!IsExactCleanTerminal(selectionDescent, descentStep))
        {
            bool terminalReached = descentStep.TerminalReached
                && descentStep.StopAtRawFileOffsetExclusive == TerminalRawFileOffset;
            bool contaminated = terminalReached
                && (selectionDescent.Outcome
                        == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                            .ContaminatedFieldDescentCaptures
                    || descentStep.AnyUnreachedCaptureProvided);
            return BuildPlan(
                capture,
                selectionDescent,
                descentClear: null,
                outcome: contaminated
                    ? CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .TerminalDescentContaminated
                    : CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .TerminalDescentBlocked,
                exactTerminalSelectionDescent: false,
                clearTailCaptureSupplied,
                unreachedClearTailCaptureProvided: clearTailCaptureSupplied,
                clearTailReached: false,
                admitted: false,
                selectedDestination: terminalReached
                    ? TerminalRawFileOffset
                    : selectedDestination,
                stopAt: selectionDescent.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeDescentClearLifecyclePlan descentClear =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(new(
                new(
                    ValidatedEntryEBP: selectionDescent.DerivedDescentEntryEBP,
                    ValidatedEntryEAX: selectionDescent.DerivedDescentEntryEAX,
                    FreshFrameMinus4:
                        capture.SelectionDescentCapture.FreshDescentFrameMinus4,
                    FreshEaxPlus14:
                        capture.SelectionDescentCapture.FreshDescentEaxPlus14,
                    FreshEaxPlus10:
                        capture.SelectionDescentCapture.FreshDescentEaxPlus10,
                    FreshEcxValue:
                        capture.SelectionDescentCapture.FreshDescentEcxValue),
                capture.FreshGlobalForEntryCompare,
                capture.FreshFrameMinus12,
                capture.FreshFrameMinus4ForZeroStoreAddress,
                capture.FreshGlobalForTailLoad));

        CapacityFinalCalleeSelectionClearLifecycleOutcome finalOutcome =
            descentClear.Outcome switch
            {
                CapacityFinalCalleeDescentClearLifecycleOutcome.ClearBelowDestination =>
                    CapacityFinalCalleeSelectionClearLifecycleOutcome
                        .ClearBelowDestination,
                CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailCompleted =>
                    CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailCompleted,
                CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailContaminated =>
                    CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailContaminated,
                _ => CapacityFinalCalleeSelectionClearLifecycleOutcome.ClearTailIncomplete
            };

        uint? clearDestination =
            descentClear.ClearTailPlan?.SelectedBranchDestinationRawFileOffset;
        if (descentClear.Outcome
            == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearBelowDestination)
        {
            clearDestination = BelowDestinationRawFileOffset;
        }
        else if (descentClear.ClearTailPlan is
                 {
                     BranchTaken: false,
                     StopAtRawFileOffsetExclusive:
                         ClearTailCompletionRawFileOffsetExclusive
                 })
        {
            clearDestination = ClearTailCompletionRawFileOffsetExclusive;
        }

        return BuildPlan(
            capture,
            selectionDescent,
            descentClear,
            finalOutcome,
            exactTerminalSelectionDescent: true,
            clearTailCaptureSupplied,
            unreachedClearTailCaptureProvided:
                descentClear.UnreachedClearTailCaptureProvided,
            clearTailReached: descentClear.ClearTailReached,
            descentClear.Admitted,
            selectedDestination: clearDestination,
            stopAt: descentClear.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanTerminal(
        CapacityFinalCalleeSelectionDescentLifecyclePlan selectionDescent,
        CapacityFinalCalleeFieldDescentStepPlan descent) =>
        selectionDescent.ExactFC2RouteSelected
        && selectionDescent.Outcome
            == CapacityFinalCalleeSelectionDescentLifecycleOutcome
                .FieldDescentReachedTerminal
        && selectionDescent.Admitted
        && selectionDescent.DerivedDescentEntryEAX.HasValue
        && selectionDescent.DerivedDescentEntryEBP.HasValue
        && !selectionDescent.AnyUnreachedDescentCaptureProvided
        && descent.Outcome == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
        && descent.TerminalReached
        && descent.StopAtRawFileOffsetExclusive == TerminalRawFileOffset
        && descent.EDXLoadedFromFrameMinus4.HasValue
        && !descent.AnyUnreachedCaptureProvided
        && !descent.UnreachedValidatedEntryEAXCaptureProvided
        && !descent.UnreachedFreshFrameMinus4CaptureProvided
        && !descent.UnreachedFreshEaxPlus14CaptureProvided
        && !descent.UnreachedFreshEaxPlus10CaptureProvided
        && !descent.UnreachedFreshEcxCaptureProvided;

    private static uint? SelectedDestination(
        CapacityFinalCalleeSelectionDescentLifecyclePlan selectionDescent)
    {
        if (selectionDescent.SelectedDestinationRawFileOffset is uint selected)
        {
            return selected;
        }

        CapacityFinalCalleeFieldDescentStepPlan? descent =
            selectionDescent.FieldDescentStepPlan;
        if (descent is CapacityFinalCalleeFieldDescentStepPlan descentStep
            && descentStep.TerminalReached
            && descentStep.StopAtRawFileOffsetExclusive == TerminalRawFileOffset)
        {
            return TerminalRawFileOffset;
        }

        if (descent is CapacityFinalCalleeFieldDescentStepPlan loopbackStep
            && loopbackStep.LoopbackReached
            && loopbackStep.StopAtRawFileOffsetExclusive == LoopbackRawFileOffset)
        {
            return LoopbackRawFileOffset;
        }

        return null;
    }

    private static bool HasClearTailCapture(
        CapacityFinalCalleeSelectionClearLifecycleCapture capture) =>
        capture.FreshGlobalForEntryCompare.HasValue
        || capture.FreshFrameMinus12.HasValue
        || capture.FreshFrameMinus4ForZeroStoreAddress.HasValue
        || capture.FreshGlobalForTailLoad.HasValue;

    private static CapacityFinalCalleeSelectionClearLifecyclePlan BuildPlan(
        CapacityFinalCalleeSelectionClearLifecycleCapture capture,
        CapacityFinalCalleeSelectionDescentLifecyclePlan selectionDescentPlan,
        CapacityFinalCalleeDescentClearLifecyclePlan? descentClear,
        CapacityFinalCalleeSelectionClearLifecycleOutcome outcome,
        bool exactTerminalSelectionDescent,
        bool clearTailCaptureSupplied,
        bool unreachedClearTailCaptureProvided,
        bool clearTailReached,
        bool admitted,
        uint? selectedDestination,
        uint stopAt) =>
        new(
            capture,
            selectionDescentPlan,
            descentClear,
            outcome,
            exactTerminalSelectionDescent,
            clearTailCaptureSupplied,
            unreachedClearTailCaptureProvided,
            clearTailReached,
            admitted,
            selectedDestination,
            stopAt);
}
