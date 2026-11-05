namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePairedTagLifecycleCapture(
    CapacityFinalCalleePairedFieldWritesCapture PairedFieldWritesCapture,
    uint? TagEntryESI,
    uint? TagEntryEDX,
    uint? TagEdxPlusFourDword,
    uint? TagGlobalDword);

public enum CapacityFinalCalleePairedTagLifecycleOutcome
{
    PairedFieldWritesIncomplete,
    PairedFieldWritesContaminated,
    TagEntryRegistersMissing,
    TagValidationIncomplete,
    TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
    TagClBitZeroClearExit,
    TagClBitOneSetExit,
    TagEntryEdxNotEqualGlobalDwordExit,
    TagEntryEdxEqualsGlobalDwordBoundary
}

public readonly record struct CapacityFinalCalleePairedTagLifecyclePlan(
    CapacityFinalCalleePairedTagLifecycleCapture Capture,
    CapacityFinalCalleePairedFieldWritesPlan PairedFieldWrites,
    CapacityFinalCalleeTagValidationPlan? TagValidation,
    CapacityFinalCalleePairedTagLifecycleOutcome Outcome,
    bool TagValidationReached,
    bool UnreachedTagEntryESIProvided,
    bool UnreachedTagEntryEDXProvided,
    bool UnreachedTagEdxPlusFourDwordProvided,
    bool UnreachedTagGlobalDwordProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the two writes and fresh global reload to tag validation at the exact
/// EF8 boundary. Tag-entry registers and tag memory reads remain independent captures.
/// </summary>
public static class CapacityFinalCalleePairedTagLifecycleProjection
{
    public const uint TagEntryRawFileOffset =
        CapacityFinalCalleeTagValidationProjection.EntryRawFileOffset;
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

    public static CapacityFinalCalleePairedTagLifecyclePlan Project(
        CapacityFinalCalleePairedTagLifecycleCapture capture)
    {
        CapacityFinalCalleePairedFieldWritesPlan pairedFieldWrites =
            CapacityFinalCalleePairedFieldWritesProjection.Project(
                capture.PairedFieldWritesCapture);

        if (!IsExactPairedFieldWritesCommit(pairedFieldWrites))
        {
            bool contaminated = HasUnreachedPairedFieldWritesCaptures(pairedFieldWrites)
                || HasTagCaptures(capture);
            return BuildPlan(
                capture,
                pairedFieldWrites,
                tagValidation: null,
                outcome: contaminated
                    ? CapacityFinalCalleePairedTagLifecycleOutcome.PairedFieldWritesContaminated
                    : CapacityFinalCalleePairedTagLifecycleOutcome.PairedFieldWritesIncomplete,
                tagValidationReached: false,
                unreachedTagEntryESIProvided: capture.TagEntryESI.HasValue,
                unreachedTagEntryEDXProvided: capture.TagEntryEDX.HasValue,
                unreachedTagEdxPlusFourDwordProvided:
                    capture.TagEdxPlusFourDword.HasValue,
                unreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
                admitted: false,
                stopAt: pairedFieldWrites.StopAtRawFileOffsetExclusive);
        }

        if (capture.TagEntryESI is not uint tagEntryEsi
            || capture.TagEntryEDX is not uint tagEntryEdx)
        {
            return BuildPlan(
                capture,
                pairedFieldWrites,
                tagValidation: null,
                outcome:
                    CapacityFinalCalleePairedTagLifecycleOutcome.TagEntryRegistersMissing,
                tagValidationReached: false,
                unreachedTagEntryESIProvided: capture.TagEntryESI.HasValue,
                unreachedTagEntryEDXProvided: capture.TagEntryEDX.HasValue,
                unreachedTagEdxPlusFourDwordProvided:
                    capture.TagEdxPlusFourDword.HasValue,
                unreachedTagGlobalDwordProvided: capture.TagGlobalDword.HasValue,
                admitted: false,
                stopAt: TagEntryRawFileOffset);
        }

        CapacityFinalCalleeTagValidationPlan tagValidation =
            CapacityFinalCalleeTagValidationProjection.Project(
                new(
                    CaptureValidatedEntry: true,
                    EntryESI: tagEntryEsi,
                    EntryEDX: tagEntryEdx,
                    EdxPlusFourDword: capture.TagEdxPlusFourDword,
                    GlobalDword: capture.TagGlobalDword));

        bool admitted = IsExactBoundedTagOutcome(tagValidation);
        return BuildPlan(
            capture,
            pairedFieldWrites,
            tagValidation,
            admitted
                ? BoundedOutcome(tagValidation.Outcome)
                : CapacityFinalCalleePairedTagLifecycleOutcome.TagValidationIncomplete,
            tagValidationReached: true,
            unreachedTagEntryESIProvided: false,
            unreachedTagEntryEDXProvided: false,
            unreachedTagEdxPlusFourDwordProvided:
                tagValidation.UnreachedEdxPlusFourDwordCaptureProvided,
            unreachedTagGlobalDwordProvided:
                tagValidation.UnreachedGlobalDwordCaptureProvided,
            admitted: admitted,
            stopAt: tagValidation.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleePairedTagLifecyclePlan BuildPlan(
        CapacityFinalCalleePairedTagLifecycleCapture capture,
        CapacityFinalCalleePairedFieldWritesPlan pairedFieldWrites,
        CapacityFinalCalleeTagValidationPlan? tagValidation,
        CapacityFinalCalleePairedTagLifecycleOutcome outcome,
        bool tagValidationReached,
        bool unreachedTagEntryESIProvided,
        bool unreachedTagEntryEDXProvided,
        bool unreachedTagEdxPlusFourDwordProvided,
        bool unreachedTagGlobalDwordProvided,
        bool admitted,
        uint stopAt) =>
        new(
            capture,
            pairedFieldWrites,
            tagValidation,
            outcome,
            tagValidationReached,
            unreachedTagEntryESIProvided,
            unreachedTagEntryEDXProvided,
            unreachedTagEdxPlusFourDwordProvided,
            unreachedTagGlobalDwordProvided,
            admitted,
            stopAt);

    private static bool IsExactPairedFieldWritesCommit(
        CapacityFinalCalleePairedFieldWritesPlan plan) =>
        plan.Outcome == CapacityFinalCalleePairedFieldWritesOutcome.Completed
        && plan.FreshFrameMinus4ReadReached
        && plan.FreshFrameMinus4Captured
        && plan.ECXLoaded.HasValue
        && plan.EAXRetained.HasValue
        && plan.FirstWriteReached
        && plan.FirstWriteAddress.HasValue
        && plan.FirstWriteValue.HasValue
        && plan.SecondWriteReached
        && plan.SecondWriteAddress.HasValue
        && plan.SecondWriteValue.HasValue
        && plan.FreshGlobalReadReached
        && plan.FreshGlobalValueCaptured
        && plan.EBXFresh.HasValue
        && plan.StopAtRawFileOffsetExclusive == TagEntryRawFileOffset
        && !HasUnreachedPairedFieldWritesCaptures(plan);

    private static bool HasUnreachedPairedFieldWritesCaptures(
        CapacityFinalCalleePairedFieldWritesPlan plan) =>
        plan.UnreachedFreshFrameMinus4CaptureProvided
        || plan.UnreachedValidatedEntryEAXCaptureProvided
        || plan.UnreachedFreshGlobalValueCaptureProvided;

    private static bool HasTagCaptures(
        CapacityFinalCalleePairedTagLifecycleCapture capture) =>
        capture.TagEntryESI.HasValue
        || capture.TagEntryEDX.HasValue
        || capture.TagEdxPlusFourDword.HasValue
        || capture.TagGlobalDword.HasValue;

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
                tagValidation.CompareEsiEdxReached
                && tagValidation.UnsignedBelowBranchReached
                && tagValidation.EntryEsiUnsignedBelowEntryEdx == false
                && tagValidation.Reads.IsEmpty
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagUnsignedExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagUnsignedExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear =>
                tagValidation.EdxPlusFourReadReached
                && tagValidation.EdxPlusFourDwordCaptured
                && tagValidation.TestClOneReached
                && tagValidation.ClZeroBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagClBitZeroClearExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagClBitZeroClearExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.ClBitOneSet =>
                tagValidation.TestClOneReached
                && tagValidation.ClZeroBranchTaken == false
                && tagValidation.TestClTwoReached
                && tagValidation.ClOneBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagClBitOneSetExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagClBitOneSetExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword =>
                tagValidation.GlobalReadReached
                && tagValidation.GlobalDwordCaptured
                && tagValidation.CompareEdxGlobalReached
                && tagValidation.NotEqualBranchTaken == true
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagNotEqualExitRawFileOffset
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagNotEqualExitRawFileOffset,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword =>
                tagValidation.GlobalReadReached
                && tagValidation.GlobalDwordCaptured
                && tagValidation.CompareEdxGlobalReached
                && tagValidation.NotEqualBranchTaken == false
                && tagValidation.SelectedDestinationRawFileOffset
                    == TagBoundaryRawFileOffsetExclusive
                && tagValidation.StopAtRawFileOffsetExclusive
                    == TagBoundaryRawFileOffsetExclusive,
            _ => false
        };
    }

    private static CapacityFinalCalleePairedTagLifecycleOutcome BoundedOutcome(
        CapacityFinalCalleeTagValidationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx =>
                CapacityFinalCalleePairedTagLifecycleOutcome
                    .TagEntryEsiUnsignedGreaterOrEqualEntryEdxExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear =>
                CapacityFinalCalleePairedTagLifecycleOutcome.TagClBitZeroClearExit,
            CapacityFinalCalleeTagValidationOutcome.ClBitOneSet =>
                CapacityFinalCalleePairedTagLifecycleOutcome.TagClBitOneSetExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword =>
                CapacityFinalCalleePairedTagLifecycleOutcome
                    .TagEntryEdxNotEqualGlobalDwordExit,
            CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword =>
                CapacityFinalCalleePairedTagLifecycleOutcome
                    .TagEntryEdxEqualsGlobalDwordBoundary,
            _ => CapacityFinalCalleePairedTagLifecycleOutcome.TagValidationIncomplete
        };
}
