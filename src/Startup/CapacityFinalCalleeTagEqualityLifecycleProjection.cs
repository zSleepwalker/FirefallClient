namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeTagEqualityLifecycleCapture(
    CapacityFinalCalleeAlignmentTagLifecycleCapture AlignmentTagCapture,
    uint? InitialGlobalDword,
    uint? FreshTagDword,
    uint? CapacityDword,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeTagEqualityLifecycleOutcome
{
    TagPlanNotAdmitted,
    TagRouteCompletedWithoutEqualityBoundary,
    NonEqualityRouteContaminatedByContinuationCaptures,
    ContinuationIncomplete,
    CapacityExitAt9441,
    DirectCallReturnedNormallyAtF71,
    DirectCallReturnStatusMissing,
    DirectCallDidNotReturnNormally
}

public readonly record struct CapacityFinalCalleeTagEqualityLifecyclePlan(
    CapacityFinalCalleeTagEqualityLifecycleCapture Capture,
    CapacityFinalCalleeAlignmentTagLifecyclePlan AlignmentTag,
    CapacityFinalCalleeTagEqualityContinuationPlan? Continuation,
    CapacityFinalCalleeTagEqualityLifecycleOutcome Outcome,
    bool EqualityBoundaryAdmitted,
    bool ContinuationReached,
    uint? ContinuationESI,
    uint? ContinuationEDI,
    uint? ContinuationESP,
    bool UnreachedInitialGlobalDwordCaptureProvided,
    bool UnreachedFreshTagDwordCaptureProvided,
    bool UnreachedCapacityDwordCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins only the clean equality boundary of the admitted alignment/tag lifecycle
/// to the standalone continuation. Continuation registers are derived from the
/// admitted tag ESI, aligned dispatch EDI, and original entry ESP; each continuation
/// global remains an independent read capture.
/// </summary>
public static class CapacityFinalCalleeTagEqualityLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeAlignmentTagLifecycleProjection.EntryRawFileOffset;
    public const uint EqualityBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeAlignmentTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive;
    public const uint CapacityExitRawFileOffset =
        CapacityFinalCalleeTagEqualityContinuationProjection.CapacityBranchDestinationRawFileOffset;
    public const uint DirectCallBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeTagEqualityContinuationProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeTagEqualityLifecyclePlan Project(
        CapacityFinalCalleeTagEqualityLifecycleCapture capture)
    {
        CapacityFinalCalleeAlignmentTagLifecyclePlan alignmentTag =
            CapacityFinalCalleeAlignmentTagLifecycleProjection.Project(
                capture.AlignmentTagCapture);

        if (!IsCleanEqualityBoundary(alignmentTag))
        {
            bool equalityCandidate =
                alignmentTag.Outcome
                    == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                        .TagEntryEdxEqualsGlobalDwordBoundary;
            bool continuationCaptureProvided =
                capture.InitialGlobalDword.HasValue
                || capture.FreshTagDword.HasValue
                || capture.CapacityDword.HasValue
                || capture.CallReturnedNormally.HasValue;
            if (alignmentTag.Admitted && !equalityCandidate)
            {
                return Blocked(
                    capture,
                    alignmentTag,
                    continuationCaptureProvided
                        ? CapacityFinalCalleeTagEqualityLifecycleOutcome
                            .NonEqualityRouteContaminatedByContinuationCaptures
                        : CapacityFinalCalleeTagEqualityLifecycleOutcome
                            .TagRouteCompletedWithoutEqualityBoundary,
                    admitted: !continuationCaptureProvided);
            }

            return Blocked(
                capture,
                alignmentTag,
                CapacityFinalCalleeTagEqualityLifecycleOutcome.TagPlanNotAdmitted);
        }

        uint continuationEsi = alignmentTag.TagEntryESI!.Value;
        uint continuationEdi = alignmentTag.AlignmentDispatch.EDI!.Value;
        uint continuationEsp =
            capture.AlignmentTagCapture.AlignmentDispatchCapture.EntryESP;
        CapacityFinalCalleeTagEqualityContinuationPlan continuation =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(
                new(
                    CaptureValidatedEntry: true,
                    EntryESI: continuationEsi,
                    EntryEDI: continuationEdi,
                    EntryESP: continuationEsp,
                    InitialGlobalDword: capture.InitialGlobalDword,
                    FreshTagDword: capture.FreshTagDword,
                    CapacityDword: capture.CapacityDword,
                    CallReturnedNormally: capture.CallReturnedNormally));

        bool continuationCapturesClean =
            !continuation.UnreachedInitialGlobalDwordCaptureProvided
            && !continuation.UnreachedFreshTagDwordCaptureProvided
            && !continuation.UnreachedCapacityDwordCaptureProvided
            && !continuation.UnreachedCallReturnStatusProvided;
        bool capacityExit =
            continuation.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome
                    .CapacityUnsignedBelowOrEqual
            && continuation.CapacityJbeReached
            && continuation.CapacityJbeTaken == true
            && continuation.CapacityBranchDestinationRawFileOffset
                == CapacityExitRawFileOffset
            && continuation.StopAtRawFileOffsetExclusive == CapacityExitRawFileOffset
            && !continuation.CallReached;
        bool directCallReturned =
            continuation.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.CallReturnedNormally
            && continuation.CallReached
            && continuation.CallReturnedNormally == true
            && continuation.CapacityJbeReached
            && continuation.CapacityJbeTaken == false
            && continuation.CapacityBranchDestinationRawFileOffset is null
            && continuation.StopAtRawFileOffsetExclusive
                == DirectCallBoundaryRawFileOffsetExclusive;
        bool admitted = continuationCapturesClean && (capacityExit || directCallReturned);

        CapacityFinalCalleeTagEqualityLifecycleOutcome outcome =
            continuation.Outcome switch
            {
                CapacityFinalCalleeTagEqualityContinuationOutcome.MissingCallReturnStatus =>
                    CapacityFinalCalleeTagEqualityLifecycleOutcome.DirectCallReturnStatusMissing,
                CapacityFinalCalleeTagEqualityContinuationOutcome.CallDidNotReturnNormally =>
                    CapacityFinalCalleeTagEqualityLifecycleOutcome.DirectCallDidNotReturnNormally,
                _ when admitted && capacityExit =>
                    CapacityFinalCalleeTagEqualityLifecycleOutcome.CapacityExitAt9441,
                _ when admitted && directCallReturned =>
                    CapacityFinalCalleeTagEqualityLifecycleOutcome.DirectCallReturnedNormallyAtF71,
                _ => CapacityFinalCalleeTagEqualityLifecycleOutcome.ContinuationIncomplete
            };

        return new(
            capture,
            alignmentTag,
            continuation,
            outcome,
            EqualityBoundaryAdmitted: true,
            ContinuationReached: true,
            continuationEsi,
            continuationEdi,
            continuationEsp,
            continuation.UnreachedInitialGlobalDwordCaptureProvided,
            continuation.UnreachedFreshTagDwordCaptureProvided,
            continuation.UnreachedCapacityDwordCaptureProvided,
            continuation.UnreachedCallReturnStatusProvided,
            admitted,
            continuation.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeTagEqualityLifecyclePlan Blocked(
        CapacityFinalCalleeTagEqualityLifecycleCapture capture,
        CapacityFinalCalleeAlignmentTagLifecyclePlan alignmentTag,
        CapacityFinalCalleeTagEqualityLifecycleOutcome outcome,
        bool admitted = false) =>
        new(
            capture,
            alignmentTag,
            Continuation: null,
            outcome,
            EqualityBoundaryAdmitted: false,
            ContinuationReached: false,
            ContinuationESI: null,
            ContinuationEDI: null,
            ContinuationESP: null,
            UnreachedInitialGlobalDwordCaptureProvided:
                capture.InitialGlobalDword.HasValue,
            UnreachedFreshTagDwordCaptureProvided: capture.FreshTagDword.HasValue,
            UnreachedCapacityDwordCaptureProvided: capture.CapacityDword.HasValue,
            UnreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
            Admitted: admitted,
            StopAtRawFileOffsetExclusive: alignmentTag.StopAtRawFileOffsetExclusive);

    private static bool IsCleanEqualityBoundary(
        CapacityFinalCalleeAlignmentTagLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeAlignmentTagLifecycleOutcome
                .TagEntryEdxEqualsGlobalDwordBoundary
        && plan.TagValidationReached
        && plan.TagValidation is
        {
            Outcome: CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword,
            NotEqualBranchReached: true,
            NotEqualBranchTaken: false,
            SelectedDestinationRawFileOffset:
                CapacityFinalCalleeAlignmentTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeAlignmentTagLifecycleProjection.TagBoundaryRawFileOffsetExclusive,
            UnreachedEdxPlusFourDwordCaptureProvided: false,
            UnreachedGlobalDwordCaptureProvided: false
        }
        && plan.TagEntryESI.HasValue
        && plan.TagEntryEDX.HasValue
        && plan.AlignmentDispatch.Outcome
            == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
        && plan.AlignmentDispatch.TestAlBranchReached
        && plan.AlignmentDispatch.TestAlBranchTaken == true
        && plan.AlignmentDispatch.TestAlSelectedDestinationRawFileOffset
            == CapacityFinalCalleeAlignmentTagLifecycleProjection.TagRouteRawFileOffset
        && plan.AlignmentDispatch.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeAlignmentTagLifecycleProjection.TagRouteRawFileOffset
        && plan.AlignmentDispatch.EDI.HasValue
        && plan.AlignmentDispatch.ESI.HasValue
        && plan.AlignmentDispatch.EDX == plan.TagEntryEDX
        && !plan.AlignmentDispatch.CallReached
        && plan.AlignmentDispatch.Reads.IsEmpty
        && !plan.UnreachedCapturedTagEntryESIProvided
        && !plan.UnreachedCapturedTagEntryEDXProvided
        && !plan.UnreachedTagEdxPlusFourDwordProvided
        && !plan.UnreachedTagGlobalDwordProvided
        && !plan.AlignmentDispatch.UnreachedEntryEsiDwordCaptureProvided
        && !plan.AlignmentDispatch.UnreachedGlobalDispatchDwordCaptureProvided
        && !plan.AlignmentDispatch.UnreachedCallCellTargetCaptureProvided
        && !plan.AlignmentDispatch.UnreachedCallReturnStatusProvided;
}
