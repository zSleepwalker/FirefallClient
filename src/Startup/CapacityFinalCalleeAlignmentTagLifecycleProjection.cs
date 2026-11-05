namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlignmentTagLifecycleCapture(
    CapacityFinalCalleeAlignmentDispatchCapture AlignmentDispatchCapture,
    uint? CapturedTagEntryESI = null,
    uint? CapturedTagEntryEDX = null,
    uint? TagEdxPlusFourDword = null,
    uint? TagGlobalDword = null);

public enum CapacityFinalCalleeAlignmentTagLifecycleOutcome
{
    EntryNotValidated,
    AlignmentRouteNotSelected,
    AlignmentRouteContaminated,
    TagValidationIncomplete,
    TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
    TagClBitZeroClearExit,
    TagClBitOneSetExit,
    TagEntryEdxNotEqualGlobalDwordExit,
    TagEntryEdxEqualsGlobalDwordBoundary
}

public readonly record struct CapacityFinalCalleeAlignmentTagLifecyclePlan(
    CapacityFinalCalleeAlignmentTagLifecycleCapture Capture,
    CapacityFinalCalleeAlignmentDispatchPlan AlignmentDispatch,
    CapacityFinalCalleeTagValidationPlan? TagValidation,
    CapacityFinalCalleeAlignmentTagLifecycleOutcome Outcome,
    bool TagValidationReached,
    uint? TagEntryESI,
    uint? TagEntryEDX,
    bool? CapturedTagEntryESIMatchesDerivedValue,
    bool? CapturedTagEntryEDXMatchesDerivedValue,
    bool UnreachedCapturedTagEntryESIProvided,
    bool UnreachedCapturedTagEntryEDXProvided,
    bool UnreachedTagEdxPlusFourDwordProvided,
    bool UnreachedTagGlobalDwordProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the exact AL-bit-set alignment route with the bounded tag-validation window.
/// The tag entry registers are derived from the dispatch entry ESI and aligned EDI plus
/// that entry ESI; dispatch subtraction results and opaque-call outputs are not used.
/// </summary>
public static class CapacityFinalCalleeAlignmentTagLifecycleProjection
{
    public const uint EntryRawFileOffset = CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset;
    public const uint TagRouteRawFileOffset =
        CapacityFinalCalleeAlignmentDispatchProjection.EntryAlBranchDestinationRawFileOffset;
    public const uint TagBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeTagValidationProjection.BoundaryRawFileOffsetExclusive;
    public const uint TagUnsignedExitRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.UnsignedBelowDestinationRawFileOffset;
    public const uint TagClBitZeroClearExitRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.ClZeroDestinationRawFileOffset;
    public const uint TagClBitOneSetExitRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.ClOneDestinationRawFileOffset;
    public const uint TagNotEqualExitRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.NotEqualDestinationRawFileOffset;

    public static CapacityFinalCalleeAlignmentTagLifecyclePlan Project(
        CapacityFinalCalleeAlignmentTagLifecycleCapture capture)
    {
        CapacityFinalCalleeAlignmentDispatchPlan alignment =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(
                capture.AlignmentDispatchCapture);

        if (alignment.Outcome
            == CapacityFinalCalleeAlignmentDispatchOutcome.EntryNotValidated)
        {
            return Blocked(
                capture,
                alignment,
                CapacityFinalCalleeAlignmentTagLifecycleOutcome.EntryNotValidated);
        }

        bool selectedTagRoute =
            alignment.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && alignment.TestAlBranchReached
            && alignment.TestAlBranchTaken == true
            && alignment.TestAlSelectedDestinationRawFileOffset == TagRouteRawFileOffset
            && alignment.StopAtRawFileOffsetExclusive == TagRouteRawFileOffset;
        if (!selectedTagRoute)
        {
            return Blocked(
                capture,
                alignment,
                CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteNotSelected);
        }

        if (alignment.UnreachedEntryEsiDwordCaptureProvided
            || alignment.UnreachedGlobalDispatchDwordCaptureProvided
            || alignment.UnreachedCallCellTargetCaptureProvided
            || alignment.UnreachedCallReturnStatusProvided
            || alignment.CallReached
            || alignment.Reads.Length != 0
            || alignment.ESI is not uint
            || alignment.EDX is not uint
            || alignment.EDI is not uint)
        {
            return Blocked(
                capture,
                alignment,
                CapacityFinalCalleeAlignmentTagLifecycleOutcome.AlignmentRouteContaminated);
        }

        uint tagEntryEsi = capture.AlignmentDispatchCapture.EntryESI;
        uint tagEntryEdx = alignment.EDX.Value;
        CapacityFinalCalleeTagValidationPlan tagValidation =
            CapacityFinalCalleeTagValidationProjection.Project(
                new(
                    CaptureValidatedEntry: true,
                    EntryESI: tagEntryEsi,
                    EntryEDX: tagEntryEdx,
                    EdxPlusFourDword: capture.TagEdxPlusFourDword,
                    GlobalDword: capture.TagGlobalDword));

        bool? capturedEsiMatches =
            capture.CapturedTagEntryESI is uint capturedEsi
                ? capturedEsi == tagEntryEsi
                : null;
        bool? capturedEdxMatches =
            capture.CapturedTagEntryEDX is uint capturedEdx
                ? capturedEdx == tagEntryEdx
                : null;
        bool registerCapturesConsistent =
            capturedEsiMatches != false && capturedEdxMatches != false;

        bool tagReadsClean =
            !tagValidation.UnreachedEdxPlusFourDwordCaptureProvided
            && !tagValidation.UnreachedGlobalDwordCaptureProvided;
        bool admitted = tagReadsClean
            && registerCapturesConsistent
            && IsExactBoundedTagOutcome(tagValidation);
        CapacityFinalCalleeAlignmentTagLifecycleOutcome outcome =
            !registerCapturesConsistent
                ? CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete
                : admitted
                    ? BoundedOutcome(tagValidation.Outcome)
                    : CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete;

        return new(
            capture,
            alignment,
            tagValidation,
            outcome,
            TagValidationReached: true,
            tagEntryEsi,
            tagEntryEdx,
            capturedEsiMatches,
            capturedEdxMatches,
            UnreachedCapturedTagEntryESIProvided: false,
            UnreachedCapturedTagEntryEDXProvided: false,
            UnreachedTagEdxPlusFourDwordProvided:
                tagValidation.UnreachedEdxPlusFourDwordCaptureProvided,
            UnreachedTagGlobalDwordProvided:
                tagValidation.UnreachedGlobalDwordCaptureProvided,
            admitted,
            tagValidation.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeAlignmentTagLifecyclePlan Blocked(
        CapacityFinalCalleeAlignmentTagLifecycleCapture capture,
        CapacityFinalCalleeAlignmentDispatchPlan alignment,
        CapacityFinalCalleeAlignmentTagLifecycleOutcome outcome) =>
        new(
            capture,
            alignment,
            TagValidation: null,
            outcome,
            TagValidationReached: false,
            TagEntryESI: null,
            TagEntryEDX: null,
            CapturedTagEntryESIMatchesDerivedValue: null,
            CapturedTagEntryEDXMatchesDerivedValue: null,
            UnreachedCapturedTagEntryESIProvided: capture.CapturedTagEntryESI.HasValue,
            UnreachedCapturedTagEntryEDXProvided: capture.CapturedTagEntryEDX.HasValue,
            UnreachedTagEdxPlusFourDwordProvided: capture.TagEdxPlusFourDword.HasValue,
            UnreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
            Admitted: false,
            StopAtRawFileOffsetExclusive: alignment.StopAtRawFileOffsetExclusive);

    private static bool IsExactBoundedTagOutcome(
        CapacityFinalCalleeTagValidationPlan tagValidation)
    {
        if (tagValidation.UnreachedEdxPlusFourDwordCaptureProvided
            || tagValidation.UnreachedGlobalDwordCaptureProvided)
        {
            return false;
        }

        return tagValidation.Outcome switch
        {
            CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx =>
                tagValidation.SelectedDestinationRawFileOffset == TagUnsignedExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive == TagUnsignedExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear =>
                tagValidation.ClZeroBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagClBitZeroClearExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagClBitZeroClearExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.ClBitOneSet =>
                tagValidation.ClOneBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagClBitOneSetExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagClBitOneSetExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword =>
                tagValidation.NotEqualBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagNotEqualExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive == TagNotEqualExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword =>
                tagValidation.NotEqualBranchTaken == false
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagBoundaryRawFileOffsetExclusive
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagBoundaryRawFileOffsetExclusive,
            _ => false
        };
    }

    private static CapacityFinalCalleeAlignmentTagLifecycleOutcome BoundedOutcome(
        CapacityFinalCalleeTagValidationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx =>
                CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear =>
                CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitZeroClearExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitOneSet =>
                CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagClBitOneSetExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword =>
                CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEdxNotEqualGlobalDwordExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword =>
                CapacityFinalCalleeAlignmentTagLifecycleOutcome
                    .TagEntryEdxEqualsGlobalDwordBoundary,
            _ => CapacityFinalCalleeAlignmentTagLifecycleOutcome.TagValidationIncomplete
        };
}
