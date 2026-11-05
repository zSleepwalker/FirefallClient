namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSelectionDescentLifecycleCapture(
    CapacityFinalCalleeAlternateFieldSelectionCapture AlternateFieldSelectionCapture,
    uint? FreshDescentFrameMinus4,
    uint? FreshDescentEaxPlus14,
    uint? FreshDescentEaxPlus10,
    uint? FreshDescentEcxValue);

public enum CapacityFinalCalleeSelectionDescentLifecycleOutcome
{
    AlternateFieldSelectionIncomplete,
    ContaminatedAlternateFieldSelection,
    UnreachedDescentCaptures,
    AlternateSecondFreshEsiPlus10ZeroTarget,
    MissingFieldDescentCapture,
    ContaminatedFieldDescentCaptures,
    FieldDescentReachedLoopback,
    FieldDescentReachedTerminal
}

public readonly record struct CapacityFinalCalleeSelectionDescentLifecyclePlan(
    CapacityFinalCalleeSelectionDescentLifecycleCapture Capture,
    CapacityFinalCalleeAlternateFieldSelectionPlan AlternateFieldSelectionPlan,
    CapacityFinalCalleeFieldDescentStepPlan? FieldDescentStepPlan,
    CapacityFinalCalleeSelectionDescentLifecycleOutcome Outcome,
    bool AlternateFieldSelectionClean,
    bool ExactFC2RouteSelected,
    bool ExactFfeRouteSelected,
    uint? DerivedDescentEntryEAX,
    uint? DerivedDescentEntryEBP,
    bool UnreachedDescentFrameMinus4CaptureProvided,
    bool UnreachedDescentEaxPlus14CaptureProvided,
    bool UnreachedDescentEaxPlus10CaptureProvided,
    bool UnreachedDescentEcxCaptureProvided,
    bool AnyUnreachedDescentCaptureProvided,
    bool Admitted,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins one clean alternate selection to one field-descent step. EAX is the
/// selected fresh field value; EBP comes from selection entry, while the
/// descent's frame-minus-four value remains an independent fresh read.
/// </summary>
public static class CapacityFinalCalleeSelectionDescentLifecycleProjection
{
    public const uint AlternateEntryRawFileOffset =
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

    public static CapacityFinalCalleeSelectionDescentLifecyclePlan Project(
        CapacityFinalCalleeSelectionDescentLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateFieldSelectionPlan selectionPlan =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(
                capture.AlternateFieldSelectionCapture);
        bool anyDescentCaptureProvided = AnyDescentCaptureProvided(capture);
        bool selectionClean = HasNoUnreachedSelectionCaptures(selectionPlan);

        bool firstRoute = IsExactFirstFC2Route(selectionPlan);
        bool secondRoute = IsExactSecondFC2Route(selectionPlan);
        bool ffeRoute = IsExactFFERoute(selectionPlan);

        if (!firstRoute && !secondRoute && !ffeRoute)
        {
            return BuildPlan(
                capture,
                selectionPlan,
                fieldDescentStepPlan: null,
                outcome: anyDescentCaptureProvided
                    ? CapacityFinalCalleeSelectionDescentLifecycleOutcome
                        .UnreachedDescentCaptures
                    : selectionClean
                        ? CapacityFinalCalleeSelectionDescentLifecycleOutcome
                            .AlternateFieldSelectionIncomplete
                        : CapacityFinalCalleeSelectionDescentLifecycleOutcome
                            .ContaminatedAlternateFieldSelection,
                alternateFieldSelectionClean: selectionClean,
                unreachedDescentFrameMinus4CaptureProvided:
                    capture.FreshDescentFrameMinus4.HasValue,
                unreachedDescentEaxPlus14CaptureProvided:
                    capture.FreshDescentEaxPlus14.HasValue,
                unreachedDescentEaxPlus10CaptureProvided:
                    capture.FreshDescentEaxPlus10.HasValue,
                unreachedDescentEcxCaptureProvided:
                    capture.FreshDescentEcxValue.HasValue,
                stopAt: selectionPlan.StopAtRawFileOffsetExclusive);
        }

        if (ffeRoute)
        {
            if (anyDescentCaptureProvided)
            {
                return BuildPlan(
                    capture,
                    selectionPlan,
                    fieldDescentStepPlan: null,
                    outcome: CapacityFinalCalleeSelectionDescentLifecycleOutcome
                        .UnreachedDescentCaptures,
                    alternateFieldSelectionClean: true,
                    exactFfeRouteSelected: true,
                    selectedDestinationRawFileOffset: FfeTargetRawFileOffset,
                    unreachedDescentFrameMinus4CaptureProvided:
                        capture.FreshDescentFrameMinus4.HasValue,
                    unreachedDescentEaxPlus14CaptureProvided:
                        capture.FreshDescentEaxPlus14.HasValue,
                    unreachedDescentEaxPlus10CaptureProvided:
                        capture.FreshDescentEaxPlus10.HasValue,
                    unreachedDescentEcxCaptureProvided:
                        capture.FreshDescentEcxValue.HasValue,
                    stopAt: FfeTargetRawFileOffset);
            }

            return BuildPlan(
                capture,
                selectionPlan,
                fieldDescentStepPlan: null,
                outcome: CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10ZeroTarget,
                alternateFieldSelectionClean: true,
                exactFfeRouteSelected: true,
                admitted: true,
                selectedDestinationRawFileOffset: FfeTargetRawFileOffset,
                stopAt: FfeTargetRawFileOffset);
        }

        uint selectedEax = firstRoute
            ? selectionPlan.FirstEAX!.Value
            : selectionPlan.SecondEAX!.Value;
        uint entryEbp = selectionPlan.Capture.EntryEBP!.Value;
        CapacityFinalCalleeFieldDescentStepPlan descentPlan =
            CapacityFinalCalleeFieldDescentStepProjection.Project(new(
                ValidatedEntryEBP: entryEbp,
                ValidatedEntryEAX: selectedEax,
                FreshFrameMinus4: capture.FreshDescentFrameMinus4,
                FreshEaxPlus14: capture.FreshDescentEaxPlus14,
                FreshEaxPlus10: capture.FreshDescentEaxPlus10,
                FreshEcxValue: capture.FreshDescentEcxValue));
        bool descentClean = !descentPlan.AnyUnreachedCaptureProvided;
        bool exactLoopback = descentClean
            && descentPlan.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && descentPlan.LoopbackReached
            && descentPlan.StopAtRawFileOffsetExclusive == LoopbackRawFileOffset;
        bool exactTerminal = descentClean
            && descentPlan.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
            && descentPlan.TerminalReached
            && descentPlan.StopAtRawFileOffsetExclusive == TerminalRawFileOffset;
        bool missingDescentCapture = descentPlan.Outcome
            is CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEBP
                or CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshFrameMinus4
                or CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEAX
                or CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus14
                or CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus10
                or CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEcxValue;

        CapacityFinalCalleeSelectionDescentLifecycleOutcome outcome =
            exactLoopback
                ? CapacityFinalCalleeSelectionDescentLifecycleOutcome
                    .FieldDescentReachedLoopback
                : exactTerminal
                    ? CapacityFinalCalleeSelectionDescentLifecycleOutcome
                        .FieldDescentReachedTerminal
                    : missingDescentCapture
                        ? CapacityFinalCalleeSelectionDescentLifecycleOutcome
                            .MissingFieldDescentCapture
                        : CapacityFinalCalleeSelectionDescentLifecycleOutcome
                            .ContaminatedFieldDescentCaptures;

        return BuildPlan(
            capture,
            selectionPlan,
            descentPlan,
            outcome,
            alternateFieldSelectionClean: true,
            exactFc2RouteSelected: true,
            derivedDescentEntryEAX: selectedEax,
            derivedDescentEntryEBP: entryEbp,
            unreachedDescentFrameMinus4CaptureProvided:
                descentPlan.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedDescentEaxPlus14CaptureProvided:
                descentPlan.UnreachedFreshEaxPlus14CaptureProvided,
            unreachedDescentEaxPlus10CaptureProvided:
                descentPlan.UnreachedFreshEaxPlus10CaptureProvided,
            unreachedDescentEcxCaptureProvided:
                descentPlan.UnreachedFreshEcxCaptureProvided,
            admitted: exactLoopback || exactTerminal,
            selectedDestinationRawFileOffset: exactLoopback
                ? LoopbackRawFileOffset
                : exactTerminal
                    ? TerminalRawFileOffset
                    : null,
            stopAt: descentPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactFirstFC2Route(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan) =>
        HasNoUnreachedSelectionCaptures(plan)
        && plan.Outcome
            == CapacityFinalCalleeAlternateFieldSelectionOutcome
                .FirstFreshEsiPlus14NonzeroToBoundary
        && plan.Capture.EntryESI is uint entryEsi
        && plan.Capture.EntryEBP is uint entryEbp
        && plan.EntryESICaptured
        && plan.FreshEsiPlus14ReadReached
        && plan.FreshEsiPlus14Captured
        && plan.FreshEsiPlus14Address
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEsiPlus14Address(entryEsi)
        && plan.FirstEAX is uint firstEax
        && firstEax != 0
        && plan.FirstLEAReached
        && plan.FirstECX == plan.FreshEsiPlus14Address
        && plan.EntryEBPCaptured
        && plan.FirstWriteReached
        && plan.FirstWriteAddress
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEbpMinus4Address(entryEbp)
        && plan.FirstWriteValue == plan.FirstECX
        && plan.FirstTestReached
        && plan.FirstTestIsZero == false
        && plan.FirstBranchReached
        && plan.FirstBranchTaken == true
        && plan.FirstBranchSelectedDestinationRawFileOffset
            == DescentEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == DescentEntryRawFileOffset;

    private static bool IsExactSecondFC2Route(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan) =>
        HasNoUnreachedSelectionCaptures(plan)
        && plan.Outcome
            == CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10NonzeroToBoundary
        && plan.Capture.EntryESI is uint entryEsi
        && plan.Capture.EntryEBP is uint entryEbp
        && plan.EntryESICaptured
        && plan.FreshEsiPlus14ReadReached
        && plan.FreshEsiPlus14Captured
        && plan.FreshEsiPlus14Address
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEsiPlus14Address(entryEsi)
        && plan.FirstEAX == 0
        && plan.FirstLEAReached
        && plan.FirstECX == plan.FreshEsiPlus14Address
        && plan.EntryEBPCaptured
        && plan.FirstWriteReached
        && plan.FirstWriteAddress
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEbpMinus4Address(entryEbp)
        && plan.FirstWriteValue == plan.FirstECX
        && plan.FirstTestReached
        && plan.FirstTestIsZero == true
        && plan.FirstBranchReached
        && plan.FirstBranchTaken == false
        && plan.FirstBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .SecondReadRawFileOffset
        && plan.FreshEsiPlus10ReadReached
        && plan.FreshEsiPlus10Captured
        && plan.FreshEsiPlus10Address
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEsiPlus10Address(entryEsi)
        && plan.SecondEAX is uint secondEax
        && secondEax != 0
        && plan.SecondLEAReached
        && plan.SecondECX == plan.FreshEsiPlus10Address
        && plan.SecondWriteReached
        && plan.SecondWriteAddress == plan.FirstWriteAddress
        && plan.SecondWriteValue == plan.SecondECX
        && plan.SecondTestReached
        && plan.SecondTestIsZero == false
        && plan.SecondBranchReached
        && plan.SecondBranchTaken == false
        && plan.SecondBranchSelectedDestinationRawFileOffset
            == DescentEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == DescentEntryRawFileOffset;

    private static bool IsExactFFERoute(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan) =>
        HasNoUnreachedSelectionCaptures(plan)
        && plan.Outcome
            == CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10ZeroToTarget
        && plan.Capture.EntryESI is uint entryEsi
        && plan.Capture.EntryEBP is uint entryEbp
        && plan.EntryESICaptured
        && plan.FreshEsiPlus14ReadReached
        && plan.FreshEsiPlus14Captured
        && plan.FreshEsiPlus14Address
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEsiPlus14Address(entryEsi)
        && plan.FirstEAX == 0
        && plan.FirstLEAReached
        && plan.FirstECX == plan.FreshEsiPlus14Address
        && plan.EntryEBPCaptured
        && plan.FirstWriteReached
        && plan.FirstWriteAddress
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEbpMinus4Address(entryEbp)
        && plan.FirstWriteValue == plan.FirstECX
        && plan.FirstTestReached
        && plan.FirstTestIsZero == true
        && plan.FirstBranchReached
        && plan.FirstBranchTaken == false
        && plan.FirstBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .SecondReadRawFileOffset
        && plan.FreshEsiPlus10ReadReached
        && plan.FreshEsiPlus10Captured
        && plan.FreshEsiPlus10Address
            == CapacityFinalCalleeAlternateFieldSelectionProjection
                .DeriveEsiPlus10Address(entryEsi)
        && plan.SecondEAX == 0
        && plan.SecondLEAReached
        && plan.SecondECX == plan.FreshEsiPlus10Address
        && plan.SecondWriteReached
        && plan.SecondWriteAddress == plan.FirstWriteAddress
        && plan.SecondWriteValue == plan.SecondECX
        && plan.SecondTestReached
        && plan.SecondTestIsZero == true
        && plan.SecondBranchReached
        && plan.SecondBranchTaken == true
        && plan.SecondBranchSelectedDestinationRawFileOffset == FfeTargetRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == FfeTargetRawFileOffset;

    private static bool HasNoUnreachedSelectionCaptures(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan) =>
        !plan.UnreachedFreshEsiPlus14CaptureProvided
        && !plan.UnreachedEntryEBPCaptureProvided
        && !plan.UnreachedFreshEsiPlus10CaptureProvided;

    private static bool AnyDescentCaptureProvided(
        CapacityFinalCalleeSelectionDescentLifecycleCapture capture) =>
        capture.FreshDescentFrameMinus4.HasValue
        || capture.FreshDescentEaxPlus14.HasValue
        || capture.FreshDescentEaxPlus10.HasValue
        || capture.FreshDescentEcxValue.HasValue;

    private static CapacityFinalCalleeSelectionDescentLifecyclePlan BuildPlan(
        CapacityFinalCalleeSelectionDescentLifecycleCapture capture,
        CapacityFinalCalleeAlternateFieldSelectionPlan selectionPlan,
        CapacityFinalCalleeFieldDescentStepPlan? fieldDescentStepPlan,
        CapacityFinalCalleeSelectionDescentLifecycleOutcome outcome,
        bool alternateFieldSelectionClean = false,
        bool exactFc2RouteSelected = false,
        bool exactFfeRouteSelected = false,
        uint? derivedDescentEntryEAX = null,
        uint? derivedDescentEntryEBP = null,
        bool unreachedDescentFrameMinus4CaptureProvided = false,
        bool unreachedDescentEaxPlus14CaptureProvided = false,
        bool unreachedDescentEaxPlus10CaptureProvided = false,
        bool unreachedDescentEcxCaptureProvided = false,
        bool admitted = false,
        uint? selectedDestinationRawFileOffset = null,
        uint? stopAt = null) =>
        new(
            capture,
            selectionPlan,
            fieldDescentStepPlan,
            outcome,
            alternateFieldSelectionClean,
            exactFc2RouteSelected,
            exactFfeRouteSelected,
            derivedDescentEntryEAX,
            derivedDescentEntryEBP,
            unreachedDescentFrameMinus4CaptureProvided,
            unreachedDescentEaxPlus14CaptureProvided,
            unreachedDescentEaxPlus10CaptureProvided,
            unreachedDescentEcxCaptureProvided,
            unreachedDescentFrameMinus4CaptureProvided
                || unreachedDescentEaxPlus14CaptureProvided
                || unreachedDescentEaxPlus10CaptureProvided
                || unreachedDescentEcxCaptureProvided,
            admitted,
            selectedDestinationRawFileOffset,
            stopAt ?? selectionPlan.StopAtRawFileOffsetExclusive);
}
