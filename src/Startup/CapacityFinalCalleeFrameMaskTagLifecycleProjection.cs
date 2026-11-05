namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFrameMaskTagLifecycleCapture(
    CapacityFinalCalleeFrameMaskUpdateCapture FrameMaskCapture,
    uint? TagEntryESI,
    uint? TagEntryEDX,
    uint? TagEdxPlusFourDword,
    uint? TagGlobalDword);

public enum CapacityFinalCalleeFrameMaskTagLifecycleOutcome
{
    FrameMaskIncomplete,
    FrameMaskContaminated,
    FrameMismatchBoundedExit,
    FrameMismatchTagCaptureContaminated,
    TagEntryRegistersMissing,
    TagValidationIncomplete,
    TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
    TagClBitZeroClearExit,
    TagClBitOneSetExit,
    TagEntryEdxNotEqualGlobalDwordExit,
    TagEntryEdxEqualsGlobalDwordBoundary
}

public readonly record struct CapacityFinalCalleeFrameMaskTagLifecyclePlan(
    CapacityFinalCalleeFrameMaskTagLifecycleCapture Capture,
    CapacityFinalCalleeFrameMaskUpdatePlan FrameMaskUpdate,
    CapacityFinalCalleeTagValidationPlan? TagValidation,
    CapacityFinalCalleeFrameMaskTagLifecycleOutcome Outcome,
    bool TagValidationReached,
    bool UnreachedTagEntryESIProvided,
    bool UnreachedTagEntryEDXProvided,
    bool UnreachedTagEdxPlusFourDwordProvided,
    bool UnreachedTagGlobalDwordProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the clean frame-mask update continuation to tag validation using independent,
/// opaque entry ESI/EDX captures. A clean frame mismatch remains a separate bounded exit.
/// </summary>
public static class CapacityFinalCalleeFrameMaskTagLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset;
    public const uint TagEntryRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset;
    public const uint FrameMismatchDestinationRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.MismatchDestinationRawFileOffset;
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

    public static CapacityFinalCalleeFrameMaskTagLifecyclePlan Project(
        CapacityFinalCalleeFrameMaskTagLifecycleCapture capture)
    {
        CapacityFinalCalleeFrameMaskUpdatePlan frameMask =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(
                capture.FrameMaskCapture);

        if (frameMask.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch)
        {
            if (!IsExactFrameMismatchDestination(frameMask))
            {
                return Blocked(
                    capture,
                    frameMask,
                    CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskIncomplete);
            }

            if (HasUnreachedFrameMaskCaptures(frameMask))
            {
                return Blocked(
                    capture,
                    frameMask,
                    CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskContaminated);
            }

            bool tagCapturesProvided = capture.TagEntryESI.HasValue
                || capture.TagEntryEDX.HasValue
                || capture.TagEdxPlusFourDword.HasValue
                || capture.TagGlobalDword.HasValue;
            return new(
                capture,
                frameMask,
                TagValidation: null,
                tagCapturesProvided
                    ? CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                        .FrameMismatchTagCaptureContaminated
                    : CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMismatchBoundedExit,
                TagValidationReached: false,
                UnreachedTagEntryESIProvided: capture.TagEntryESI.HasValue,
                UnreachedTagEntryEDXProvided: capture.TagEntryEDX.HasValue,
                UnreachedTagEdxPlusFourDwordProvided:
                    capture.TagEdxPlusFourDword.HasValue,
                UnreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
                Admitted: !tagCapturesProvided,
                StopAtRawFileOffsetExclusive: FrameMismatchDestinationRawFileOffset);
        }

        bool frameMaskUpdated = IsCleanGlobalUpdate(frameMask);
        if (!frameMaskUpdated)
        {
            bool contaminated = HasUnreachedFrameMaskCaptures(frameMask);
            return Blocked(
                capture,
                frameMask,
                contaminated
                    ? CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskContaminated
                    : CapacityFinalCalleeFrameMaskTagLifecycleOutcome.FrameMaskIncomplete);
        }

        if (capture.TagEntryESI is not uint tagEntryEsi
            || capture.TagEntryEDX is not uint tagEntryEdx)
        {
            return new(
                capture,
                frameMask,
                TagValidation: null,
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagEntryRegistersMissing,
                TagValidationReached: false,
                UnreachedTagEntryESIProvided: capture.TagEntryESI.HasValue,
                UnreachedTagEntryEDXProvided: capture.TagEntryEDX.HasValue,
                UnreachedTagEdxPlusFourDwordProvided:
                    capture.TagEdxPlusFourDword.HasValue,
                UnreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
                Admitted: false,
                StopAtRawFileOffsetExclusive: TagEntryRawFileOffset);
        }

        CapacityFinalCalleeTagValidationPlan tagValidation =
            CapacityFinalCalleeTagValidationProjection.Project(
                new(
                    CaptureValidatedEntry: true,
                    EntryESI: tagEntryEsi,
                    EntryEDX: tagEntryEdx,
                    EdxPlusFourDword: capture.TagEdxPlusFourDword,
                    GlobalDword: capture.TagGlobalDword));

        bool cleanTagReads =
            !tagValidation.UnreachedEdxPlusFourDwordCaptureProvided
            && !tagValidation.UnreachedGlobalDwordCaptureProvided;
        bool admitted = cleanTagReads && IsExactBoundedTagOutcome(tagValidation);
        return new(
            capture,
            frameMask,
            tagValidation,
            admitted
                ? BoundedOutcome(tagValidation.Outcome)
                : CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagValidationIncomplete,
            TagValidationReached: true,
            UnreachedTagEntryESIProvided: false,
            UnreachedTagEntryEDXProvided: false,
            UnreachedTagEdxPlusFourDwordProvided:
                tagValidation.UnreachedEdxPlusFourDwordCaptureProvided,
            UnreachedTagGlobalDwordProvided:
                tagValidation.UnreachedGlobalDwordCaptureProvided,
            admitted,
            tagValidation.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeFrameMaskTagLifecyclePlan Blocked(
        CapacityFinalCalleeFrameMaskTagLifecycleCapture capture,
        CapacityFinalCalleeFrameMaskUpdatePlan frameMask,
        CapacityFinalCalleeFrameMaskTagLifecycleOutcome outcome) =>
        new(
            capture,
            frameMask,
            TagValidation: null,
            outcome,
            TagValidationReached: false,
            UnreachedTagEntryESIProvided: capture.TagEntryESI.HasValue,
            UnreachedTagEntryEDXProvided: capture.TagEntryEDX.HasValue,
            UnreachedTagEdxPlusFourDwordProvided:
                capture.TagEdxPlusFourDword.HasValue,
            UnreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
            Admitted: false,
            StopAtRawFileOffsetExclusive: frameMask.StopAtRawFileOffsetExclusive);

    private static bool IsCleanGlobalUpdate(
        CapacityFinalCalleeFrameMaskUpdatePlan frameMask) =>
        frameMask.Outcome
            == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
        && frameMask.CompareReached
        && frameMask.Equal == true
        && frameMask.BranchReached
        && frameMask.BranchTaken == false
        && frameMask.SelectedBranchDestinationRawFileOffset is null
        && frameMask.ShiftReached
        && frameMask.GlobalReadReached
        && frameMask.GlobalValueCaptured
        && frameMask.GlobalWriteReached
        && frameMask.StopAtRawFileOffsetExclusive
            == TagEntryRawFileOffset
        && !HasUnreachedFrameMaskCaptures(frameMask);

    private static bool IsExactFrameMismatchDestination(
        CapacityFinalCalleeFrameMaskUpdatePlan frameMask) =>
        frameMask.CompareReached
        && frameMask.Equal == false
        && frameMask.BranchReached
        && frameMask.BranchTaken == true
        && frameMask.SelectedBranchDestinationRawFileOffset
            == FrameMismatchDestinationRawFileOffset
        && frameMask.StopAtRawFileOffsetExclusive
            == FrameMismatchDestinationRawFileOffset;

    private static bool HasUnreachedFrameMaskCaptures(
        CapacityFinalCalleeFrameMaskUpdatePlan frameMask) =>
        frameMask.UnreachedEntryECXCaptureProvided
        || frameMask.UnreachedFreshFrameMinus14CaptureProvided
        || frameMask.UnreachedFreshFrameMinus4CaptureProvided
        || frameMask.UnreachedFreshFrameMinus8CaptureProvided
        || frameMask.UnreachedFreshGlobalValueCaptureProvided;

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

    private static CapacityFinalCalleeFrameMaskTagLifecycleOutcome BoundedOutcome(
        CapacityFinalCalleeTagValidationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx =>
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear =>
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagClBitZeroClearExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitOneSet =>
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagClBitOneSetExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword =>
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEdxNotEqualGlobalDwordExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword =>
                CapacityFinalCalleeFrameMaskTagLifecycleOutcome
                    .TagEntryEdxEqualsGlobalDwordBoundary,
            _ => CapacityFinalCalleeFrameMaskTagLifecycleOutcome.TagValidationIncomplete
        };
}
