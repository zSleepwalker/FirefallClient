namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSizeAlternateLifecycleCapture(
    CapacityFinalCalleeLargeSizeFieldsCapture LargeSizeCapture,
    uint? FreshAlternateEsiPlus14,
    uint? FreshAlternateEsiPlus10);

public enum CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
{
    LargeSizeFieldsIncomplete,
    ContaminatedLargeSizeFields,
    ContaminatedUnselectedAlternateCaptures,
    LargeSizeEsiPlus8UnsignedBelowExit,
    LargeSizeEcxPlusCMismatchExit,
    LargeSizeEaxPlus8MismatchExit,
    LargeSizePairedWritesExit,
    AlternateFieldSelectionIncomplete,
    ContaminatedAlternateFieldSelection,
    AlternateFirstFreshEsiPlus14NonzeroBoundary,
    AlternateSecondFreshEsiPlus10ZeroTarget,
    AlternateSecondFreshEsiPlus10NonzeroBoundary
}

public readonly record struct CapacityFinalCalleeLargeSizeAlternateLifecyclePlan(
    CapacityFinalCalleeLargeSizeAlternateLifecycleCapture Capture,
    CapacityFinalCalleeLargeSizeFieldsPlan LargeSizeFieldsPlan,
    CapacityFinalCalleeAlternateFieldSelectionPlan? AlternateFieldSelectionPlan,
    CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome Outcome,
    bool LargeSizeBoundaryReached,
    bool AlternateFieldSelectionReached,
    bool UnreachedAlternateFreshEsiPlus14CaptureProvided,
    bool UnreachedAlternateFreshEsiPlus10CaptureProvided,
    bool Admitted,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the clean large-size ESI+ C equality route to alternate field selection.
/// The no-call continuation uses the large-size entry ESI/EBP captures directly;
/// other clean large-size exits remain bounded without entering the alternate window.
/// </summary>
public static class CapacityFinalCalleeLargeSizeAlternateLifecycleProjection
{
    public const uint AlternateEntryRawFileOffset =
        CapacityFinalCalleeAlternateFieldSelectionProjection.EntryRawFileOffset;
    public const uint LargeSizeAlternateEntryRawFileOffset =
        CapacityFinalCalleeLargeSizeFieldsProjection.EsiPlusCEqualsExitRawFileOffset;
    public const uint LargeSizeBranchExitRawFileOffset =
        CapacityFinalCalleeLargeSizeFieldsProjection.BranchExitRawFileOffset;
    public const uint LargeSizePairedWritesExitRawFileOffset =
        CapacityFinalCalleeLargeSizeFieldsProjection.PairedWritesExitRawFileOffset;
    public const uint AlternateBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateFieldSelectionProjection.BoundaryRawFileOffsetExclusive;
    public const uint AlternateFirstNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFieldSelectionProjection
            .FirstNonzeroDestinationRawFileOffset;
    public const uint AlternateSecondZeroDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFieldSelectionProjection.SecondZeroDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeSizeAlternateLifecyclePlan Project(
        CapacityFinalCalleeLargeSizeAlternateLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSizeFieldsPlan largeSizePlan =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(capture.LargeSizeCapture);

        if (!TryGetExactLargeSizeDestination(largeSizePlan, out uint destination))
        {
            bool contaminatedLargeSize = HasUnreachedLargeSizeCaptures(largeSizePlan);
            return BuildPlan(
                capture,
                largeSizePlan,
                alternateFieldSelectionPlan: null,
                outcome: contaminatedLargeSize
                    ? CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                        .ContaminatedLargeSizeFields
                    : CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                        .LargeSizeFieldsIncomplete,
                unreachedAlternateFreshEsiPlus14CaptureProvided:
                    capture.FreshAlternateEsiPlus14.HasValue,
                unreachedAlternateFreshEsiPlus10CaptureProvided:
                    capture.FreshAlternateEsiPlus10.HasValue,
                stopAt: largeSizePlan.StopAtRawFileOffsetExclusive);
        }

        if (largeSizePlan.Outcome
            != CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI)
        {
            bool hasAlternateCaptures = capture.FreshAlternateEsiPlus14.HasValue
                || capture.FreshAlternateEsiPlus10.HasValue;
            bool clean = !HasUnreachedLargeSizeCaptures(largeSizePlan)
                && !hasAlternateCaptures;
            return BuildPlan(
                capture,
                largeSizePlan,
                alternateFieldSelectionPlan: null,
                outcome: !clean && hasAlternateCaptures
                    ? CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                        .ContaminatedUnselectedAlternateCaptures
                    : !clean
                        ? CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                            .ContaminatedLargeSizeFields
                        : BoundedLargeSizeOutcome(largeSizePlan.Outcome),
                largeSizeBoundaryReached: true,
                unreachedAlternateFreshEsiPlus14CaptureProvided:
                    capture.FreshAlternateEsiPlus14.HasValue,
                unreachedAlternateFreshEsiPlus10CaptureProvided:
                    capture.FreshAlternateEsiPlus10.HasValue,
                admitted: clean,
                selectedDestinationRawFileOffset: destination,
                stopAt: destination);
        }

        if (HasUnreachedLargeSizeCaptures(largeSizePlan))
        {
            return BuildPlan(
                capture,
                largeSizePlan,
                alternateFieldSelectionPlan: null,
                outcome: CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .ContaminatedLargeSizeFields,
                largeSizeBoundaryReached: true,
                unreachedAlternateFreshEsiPlus14CaptureProvided:
                    capture.FreshAlternateEsiPlus14.HasValue,
                unreachedAlternateFreshEsiPlus10CaptureProvided:
                    capture.FreshAlternateEsiPlus10.HasValue,
                selectedDestinationRawFileOffset: destination,
                stopAt: destination);
        }

        CapacityFinalCalleeAlternateFieldSelectionPlan alternatePlan =
            CapacityFinalCalleeAlternateFieldSelectionProjection.Project(
                new(
                    EntryESI: capture.LargeSizeCapture.ValidatedEntryESI,
                    EntryEBP: capture.LargeSizeCapture.ValidatedEntryEBP,
                    FreshEsiPlus14: capture.FreshAlternateEsiPlus14,
                    FreshEsiPlus10: capture.FreshAlternateEsiPlus10));
        bool alternateAdmitted = TryGetExactAlternateDestination(
            alternatePlan,
            out uint alternateDestination);
        bool alternateContaminated = HasUnreachedAlternateCaptures(alternatePlan);
        CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome alternateOutcome =
            alternateAdmitted
                ? BoundedAlternateOutcome(alternatePlan.Outcome)
                : alternateContaminated
                    ? CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                        .ContaminatedAlternateFieldSelection
                    : CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                        .AlternateFieldSelectionIncomplete;

        return BuildPlan(
            capture,
            largeSizePlan,
            alternatePlan,
            alternateOutcome,
            largeSizeBoundaryReached: true,
            alternateFieldSelectionReached: true,
            unreachedAlternateFreshEsiPlus14CaptureProvided:
                alternatePlan.UnreachedFreshEsiPlus14CaptureProvided,
            unreachedAlternateFreshEsiPlus10CaptureProvided:
                alternatePlan.UnreachedFreshEsiPlus10CaptureProvided,
            admitted: alternateAdmitted && !alternateContaminated,
            selectedDestinationRawFileOffset: alternateAdmitted
                ? alternateDestination
                : null,
            stopAt: alternatePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool TryGetExactLargeSizeDestination(
        CapacityFinalCalleeLargeSizeFieldsPlan plan,
        out uint destination)
    {
        destination = plan.StopAtRawFileOffsetExclusive;
        if (!plan.FreshEsiPlus18ReadReached
            || !plan.FreshEsiPlus18Captured
            || !plan.FrameWriteReached
            || plan.FrameWriteAddress is null
            || plan.FrameWriteValue is null)
        {
            return false;
        }

        switch (plan.Outcome)
        {
            case CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI:
                return plan.FreshEsiPlusCReadReached
                    && plan.FreshEsiPlusCCaptured
                    && plan.EsiPlusCCompareReached
                    && plan.EsiPlusCEqualsEntryESI == true
                    && plan.StopAtRawFileOffsetExclusive
                        == LargeSizeAlternateEntryRawFileOffset;
            case CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlus8UnsignedBelowEntryEBX:
                return plan.FreshEsiPlusCReadReached
                    && plan.FreshEsiPlusCCaptured
                    && plan.EsiPlusCCompareReached
                    && plan.EsiPlusCEqualsEntryESI == false
                    && plan.FreshEsiPlus8ReadReached
                    && plan.FreshEsiPlus8Captured
                    && plan.EcxEntryEbxCompareReached
                    && plan.EsiPlus8UnsignedBelowEntryEBX == true
                    && plan.StopAtRawFileOffsetExclusive
                        == LargeSizeBranchExitRawFileOffset;
            case CapacityFinalCalleeLargeSizeFieldsOutcome.EcxPlusCNotEqualEntryESI:
                return HasLargeSizeEcxPlusCComparison(plan)
                    && plan.EcxPlusCEqualsEntryESI == false
                    && plan.StopAtRawFileOffsetExclusive
                        == LargeSizeBranchExitRawFileOffset;
            case CapacityFinalCalleeLargeSizeFieldsOutcome.EaxPlus8NotEqualEntryESI:
                return HasLargeSizeEcxPlusCComparison(plan)
                    && plan.EcxPlusCEqualsEntryESI == true
                    && plan.FreshEaxPlus8ReadReached
                    && plan.FreshEaxPlus8Captured
                    && plan.EaxPlus8EntryEsiCompareReached
                    && plan.EaxPlus8EqualsEntryESI == false
                    && plan.StopAtRawFileOffsetExclusive
                        == LargeSizeBranchExitRawFileOffset;
            case CapacityFinalCalleeLargeSizeFieldsOutcome.Completed:
                return HasLargeSizeEcxPlusCComparison(plan)
                    && plan.EcxPlusCEqualsEntryESI == true
                    && plan.FreshEaxPlus8ReadReached
                    && plan.FreshEaxPlus8Captured
                    && plan.EaxPlus8EntryEsiCompareReached
                    && plan.EaxPlus8EqualsEntryESI == true
                    && plan.FirstPairedWriteReached
                    && plan.FirstPairedWriteAddress.HasValue
                    && plan.FirstPairedWriteValue.HasValue
                    && plan.SecondPairedWriteReached
                    && plan.SecondPairedWriteAddress.HasValue
                    && plan.SecondPairedWriteValue.HasValue
                    && plan.StopAtRawFileOffsetExclusive
                        == LargeSizePairedWritesExitRawFileOffset;
            default:
                return false;
        }
    }

    private static bool HasLargeSizeEcxPlusCComparison(
        CapacityFinalCalleeLargeSizeFieldsPlan plan) =>
        plan.FreshEsiPlusCReadReached
        && plan.FreshEsiPlusCCaptured
        && plan.EsiPlusCCompareReached
        && plan.EsiPlusCEqualsEntryESI == false
        && plan.FreshEsiPlus8ReadReached
        && plan.FreshEsiPlus8Captured
        && plan.EcxEntryEbxCompareReached
        && plan.EsiPlus8UnsignedBelowEntryEBX == false
        && plan.FreshEcxPlusCReadReached
        && plan.FreshEcxPlusCCaptured
        && plan.EcxPlusCEntryEsiCompareReached;

    private static bool HasUnreachedLargeSizeCaptures(
        CapacityFinalCalleeLargeSizeFieldsPlan plan) =>
        plan.UnreachedValidatedEntryEBPCaptureProvided
        || plan.UnreachedValidatedEntryEBXCaptureProvided
        || plan.UnreachedFreshEsiPlus18CaptureProvided
        || plan.UnreachedFreshEsiPlusCCaptureProvided
        || plan.UnreachedFreshEsiPlus8CaptureProvided
        || plan.UnreachedFreshEcxPlusCCaptureProvided
        || plan.UnreachedFreshEaxPlus8CaptureProvided;

    private static bool TryGetExactAlternateDestination(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan,
        out uint destination)
    {
        destination = plan.StopAtRawFileOffsetExclusive;
        if (HasUnreachedAlternateCaptures(plan))
        {
            return false;
        }

        switch (plan.Outcome)
        {
            case CapacityFinalCalleeAlternateFieldSelectionOutcome
                .FirstFreshEsiPlus14NonzeroToBoundary:
                return plan.FreshEsiPlus14ReadReached
                    && plan.FreshEsiPlus14Captured
                    && plan.FirstLEAReached
                    && plan.FirstWriteReached
                    && plan.FirstWriteAddress.HasValue
                    && plan.FirstWriteValue.HasValue
                    && plan.FirstTestReached
                    && plan.FirstTestIsZero == false
                    && plan.FirstBranchReached
                    && plan.FirstBranchTaken == true
                    && plan.FirstBranchSelectedDestinationRawFileOffset
                        == AlternateFirstNonzeroDestinationRawFileOffset
                    && plan.StopAtRawFileOffsetExclusive
                        == AlternateFirstNonzeroDestinationRawFileOffset;
            case CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10ZeroToTarget:
                return plan.FreshEsiPlus14ReadReached
                    && plan.FreshEsiPlus14Captured
                    && plan.FirstLEAReached
                    && plan.FirstWriteReached
                    && plan.FirstWriteAddress.HasValue
                    && plan.FirstWriteValue.HasValue
                    && plan.FirstTestReached
                    && plan.FirstTestIsZero == true
                    && plan.FirstBranchReached
                    && plan.FirstBranchTaken == false
                    && plan.FreshEsiPlus10ReadReached
                    && plan.FreshEsiPlus10Captured
                    && plan.SecondLEAReached
                    && plan.SecondWriteReached
                    && plan.SecondWriteAddress.HasValue
                    && plan.SecondWriteValue.HasValue
                    && plan.SecondTestReached
                    && plan.SecondTestIsZero == true
                    && plan.SecondBranchReached
                    && plan.SecondBranchTaken == true
                    && plan.SecondBranchSelectedDestinationRawFileOffset
                        == AlternateSecondZeroDestinationRawFileOffset
                    && plan.StopAtRawFileOffsetExclusive
                        == AlternateSecondZeroDestinationRawFileOffset;
            case CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10NonzeroToBoundary:
                return plan.FreshEsiPlus14ReadReached
                    && plan.FreshEsiPlus14Captured
                    && plan.FirstLEAReached
                    && plan.FirstWriteReached
                    && plan.FirstWriteAddress.HasValue
                    && plan.FirstWriteValue.HasValue
                    && plan.FirstTestReached
                    && plan.FirstTestIsZero == true
                    && plan.FirstBranchReached
                    && plan.FirstBranchTaken == false
                    && plan.FreshEsiPlus10ReadReached
                    && plan.FreshEsiPlus10Captured
                    && plan.SecondLEAReached
                    && plan.SecondWriteReached
                    && plan.SecondWriteAddress.HasValue
                    && plan.SecondWriteValue.HasValue
                    && plan.SecondTestReached
                    && plan.SecondTestIsZero == false
                    && plan.SecondBranchReached
                    && plan.SecondBranchTaken == false
                    && plan.SecondBranchSelectedDestinationRawFileOffset
                        == AlternateFirstNonzeroDestinationRawFileOffset
                    && plan.StopAtRawFileOffsetExclusive
                        == AlternateFirstNonzeroDestinationRawFileOffset;
            default:
                return false;
        }
    }

    private static bool HasUnreachedAlternateCaptures(
        CapacityFinalCalleeAlternateFieldSelectionPlan plan) =>
        plan.UnreachedFreshEsiPlus14CaptureProvided
        || plan.UnreachedEntryEBPCaptureProvided
        || plan.UnreachedFreshEsiPlus10CaptureProvided;

    private static CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
        BoundedLargeSizeOutcome(CapacityFinalCalleeLargeSizeFieldsOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlus8UnsignedBelowEntryEBX =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEsiPlus8UnsignedBelowExit,
            CapacityFinalCalleeLargeSizeFieldsOutcome.EcxPlusCNotEqualEntryESI =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEcxPlusCMismatchExit,
            CapacityFinalCalleeLargeSizeFieldsOutcome.EaxPlus8NotEqualEntryESI =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizeEaxPlus8MismatchExit,
            CapacityFinalCalleeLargeSizeFieldsOutcome.Completed =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .LargeSizePairedWritesExit,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };

    private static CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
        BoundedAlternateOutcome(
            CapacityFinalCalleeAlternateFieldSelectionOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeAlternateFieldSelectionOutcome
                .FirstFreshEsiPlus14NonzeroToBoundary =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateFirstFreshEsiPlus14NonzeroBoundary,
            CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10ZeroToTarget =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10ZeroTarget,
            CapacityFinalCalleeAlternateFieldSelectionOutcome
                .SecondFreshEsiPlus10NonzeroToBoundary =>
                CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome
                    .AlternateSecondFreshEsiPlus10NonzeroBoundary,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };

    private static CapacityFinalCalleeLargeSizeAlternateLifecyclePlan BuildPlan(
        CapacityFinalCalleeLargeSizeAlternateLifecycleCapture capture,
        CapacityFinalCalleeLargeSizeFieldsPlan largeSizeFieldsPlan,
        CapacityFinalCalleeAlternateFieldSelectionPlan? alternateFieldSelectionPlan,
        CapacityFinalCalleeLargeSizeAlternateLifecycleOutcome outcome,
        bool largeSizeBoundaryReached = false,
        bool alternateFieldSelectionReached = false,
        bool unreachedAlternateFreshEsiPlus14CaptureProvided = false,
        bool unreachedAlternateFreshEsiPlus10CaptureProvided = false,
        bool admitted = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = LargeSizeAlternateEntryRawFileOffset) =>
        new(
            capture,
            largeSizeFieldsPlan,
            alternateFieldSelectionPlan,
            outcome,
            largeSizeBoundaryReached,
            alternateFieldSelectionReached,
            unreachedAlternateFreshEsiPlus14CaptureProvided
                || !alternateFieldSelectionReached
                    && capture.FreshAlternateEsiPlus14.HasValue,
            unreachedAlternateFreshEsiPlus10CaptureProvided
                || !alternateFieldSelectionReached
                    && capture.FreshAlternateEsiPlus10.HasValue,
            admitted,
            selectedDestinationRawFileOffset,
            stopAt);
}
