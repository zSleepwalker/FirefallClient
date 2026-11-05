namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeEntryValidationCapture(
    CapacityFinalCalleePrefixLifecycleCapture InputPrefixLifecycleCapture,
    CapacityFinalCalleeFirstValidationCapture Validation,
    uint? ExpectedValidationEntryESI = null);

public enum CapacityFinalCalleeEntryValidationOutcome
{
    UpstreamBlocked,
    ZeroPathReturned,
    ZeroPathHasUnreachedValidationCaptures,
    ValidationEntryESIMismatch,
    ValidationBlocked,
    ProjectionComplete
}

public readonly record struct CapacityFinalCalleeEntryValidationPlan(
    CapacityFinalCalleeEntryValidationCapture Capture,
    CapacityFinalCalleeEntryValidationOutcome Outcome,
    CapacityFinalCalleePrefixLifecyclePlan PrefixLifecyclePlan,
    CapacityFinalCalleeValidationLifecyclePlan? ValidationLifecyclePlan,
    bool PrefixLifecycleOutcomeClean,
    bool ZeroEntryReturned,
    bool ValidationLifecycleReached,
    bool DeriveValidationLifecyclePrefix,
    bool ValidationOutcomeClean,
    bool UnreachedValidationEntryAssertionProvided,
    bool UnreachedGlobalDwordCaptureProvided,
    bool UnreachedEsi4DwordCaptureProvided,
    bool UnreachedExpectedValidationEntryESIProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the captured callee entry and prefix lifecycle with the first validation window.
/// The zero-entry return remains bounded at its RET; nonzero validation starts only from the
/// exact prefix-call capture produced by a clean prefix lifecycle.
/// </summary>
public static class CapacityFinalCalleeEntryValidationProjection
{
    public const uint LifecycleStartRawFileOffset =
        CapacityFinalCalleePrefixLifecycleProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive;
    public const uint ZeroReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleePrefixLifecycleProjection.ZeroReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeEntryValidationPlan Project(
        CapacityFinalCalleeEntryValidationCapture capture)
    {
        CapacityFinalCalleePrefixLifecyclePlan prefixLifecyclePlan =
            CapacityFinalCalleePrefixLifecycleProjection.Project(
                capture.InputPrefixLifecycleCapture);
        bool zeroEntryReturned = IsZeroEntryReturned(prefixLifecyclePlan);

        if (zeroEntryReturned)
        {
            bool cleanZeroPrefix =
                prefixLifecyclePlan.Outcome
                    == CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathReturned
                && prefixLifecyclePlan.ZeroEpilogueAdmitted
                && !prefixLifecyclePlan.UnreachedPrefixPriorTestMaskCaptureProvided
                && !prefixLifecyclePlan.UnreachedPrefixCallCellTargetCaptureProvided
                && !prefixLifecyclePlan.UnreachedPrefixCallReturnStatusProvided
                && !prefixLifecyclePlan.UnreachedPrefixNonzeroValidationAsserted;
            bool unreachedEntryAssertion = capture.Validation.CaptureValidatedEntry;
            bool unreachedGlobal = capture.Validation.GlobalDword.HasValue;
            bool unreachedEsi4 = capture.Validation.Esi4Dword.HasValue;
            bool unreachedExpectedEsi = capture.ExpectedValidationEntryESI.HasValue;
            bool noValidationCaptures =
                !unreachedEntryAssertion && !unreachedGlobal && !unreachedEsi4
                && !unreachedExpectedEsi;
            bool clean = cleanZeroPrefix && noValidationCaptures;

            return new(
                capture,
                clean
                    ? CapacityFinalCalleeEntryValidationOutcome.ZeroPathReturned
                    : cleanZeroPrefix
                        ? CapacityFinalCalleeEntryValidationOutcome
                            .ZeroPathHasUnreachedValidationCaptures
                        : CapacityFinalCalleeEntryValidationOutcome.UpstreamBlocked,
                prefixLifecyclePlan,
                ValidationLifecyclePlan: null,
                PrefixLifecycleOutcomeClean: cleanZeroPrefix,
                ZeroEntryReturned: true,
                ValidationLifecycleReached: false,
                DeriveValidationLifecyclePrefix: false,
                ValidationOutcomeClean: false,
                UnreachedValidationEntryAssertionProvided: unreachedEntryAssertion,
                UnreachedGlobalDwordCaptureProvided: unreachedGlobal,
                UnreachedEsi4DwordCaptureProvided: unreachedEsi4,
                UnreachedExpectedValidationEntryESIProvided: unreachedExpectedEsi,
                StopAtRawFileOffsetExclusive: ZeroReturnBoundaryRawFileOffsetExclusive);
        }

        if (!IsExactCleanNonzeroPrefixLifecycle(prefixLifecyclePlan))
        {
            return BlockBeforeValidation(capture, prefixLifecyclePlan);
        }

        CapacityFinalCalleePrefixCallCapture exactPrefixCallCapture =
            prefixLifecyclePlan.PrefixCallPlan!.Value.Capture;
        CapacityFinalCalleeValidationLifecycleCapture validationCapture = new(
            Prefix: exactPrefixCallCapture,
            Validation: capture.Validation,
            ExpectedValidationEntryESI: capture.ExpectedValidationEntryESI);
        CapacityFinalCalleeValidationLifecyclePlan validationLifecyclePlan =
            CapacityFinalCalleeValidationLifecycleProjection.Project(validationCapture);
        bool validationOutcomeClean =
            validationLifecyclePlan.Outcome
                == CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
            && validationLifecyclePlan.PrefixOutcomeClean
            && validationLifecyclePlan.ValidationOutcomeClean
            && validationLifecyclePlan.DeriveValidatedEntry
            && validationLifecyclePlan.ValidationPlan.Capture.CaptureValidatedEntry
            && !validationLifecyclePlan.UnreachedGlobalDwordCaptureProvided
            && !validationLifecyclePlan.UnreachedEsi4DwordCaptureProvided
            && validationLifecyclePlan.ExpectedValidationEntryESIMatches != false;
        CapacityFinalCalleeEntryValidationOutcome outcome =
            validationOutcomeClean
                ? CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
                : validationLifecyclePlan.Outcome
                    == CapacityFinalCalleeValidationLifecycleOutcome
                        .ValidationEntryESIMismatch
                    ? CapacityFinalCalleeEntryValidationOutcome.ValidationEntryESIMismatch
                    : CapacityFinalCalleeEntryValidationOutcome.ValidationBlocked;

        return new(
            capture,
            outcome,
            prefixLifecyclePlan,
            validationLifecyclePlan,
            PrefixLifecycleOutcomeClean: true,
            ZeroEntryReturned: false,
            ValidationLifecycleReached: true,
            DeriveValidationLifecyclePrefix: true,
            ValidationOutcomeClean: validationOutcomeClean,
            UnreachedValidationEntryAssertionProvided: false,
            UnreachedGlobalDwordCaptureProvided:
                validationLifecyclePlan.UnreachedGlobalDwordCaptureProvided,
            UnreachedEsi4DwordCaptureProvided:
                validationLifecyclePlan.UnreachedEsi4DwordCaptureProvided,
            UnreachedExpectedValidationEntryESIProvided: false,
            StopAtRawFileOffsetExclusive:
                validationLifecyclePlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanNonzeroPrefixLifecycle(
        CapacityFinalCalleePrefixLifecyclePlan plan)
    {
        if (plan.Outcome != CapacityFinalCalleePrefixLifecycleOutcome.ProjectionComplete
            || !plan.EntryOutcomeClean
            || !plan.EntryUnreachedCapturesClear
            || !plan.DeriveValidatedNonzeroEntry
            || plan.ZeroEpilogueAdmitted
            || !plan.PrefixCallProjectionReached
            || !plan.PrefixCallOutcomeClean
            || plan.UnreachedPrefixEntryEcxCaptureProvided
            || plan.UnreachedPrefixPriorTestMaskCaptureProvided
            || plan.UnreachedPrefixCallCellTargetCaptureProvided
            || plan.UnreachedPrefixCallReturnStatusProvided
            || plan.UnreachedPrefixNonzeroValidationAsserted
            || plan.PrefixCallPlan is not CapacityFinalCalleePrefixCallPlan prefixPlan
            || plan.EntryPlan.Outcome
                != CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted
            || !plan.EntryPlan.Capture.EntryValidated
            || plan.EntryPlan.Capture.EntryEcx == 0
            || plan.EntryPlan.UnreachedTestedByteCaptureProvided
            || plan.EntryPlan.UnreachedZeroPathPoppedEbpCaptureProvided
            || prefixPlan.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive)
        {
            return false;
        }

        return (prefixPlan.Outcome is
                CapacityFinalCalleePrefixCallOutcome.CallSkipped
                or CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally)
            && prefixPlan.Capture.CaptureValidatedNonzeroEntry
            && prefixPlan.Capture.EntryECX == plan.EntryPlan.Capture.EntryEcx
            && prefixPlan.Capture.CapturedPriorTestMaskSet
                == plan.EntryPlan.TestedBitIsSet
            && prefixPlan.UnreachedPriorTestMaskCaptureProvided == false
            && prefixPlan.UnreachedCallCellTargetCaptureProvided == false
            && prefixPlan.UnreachedCallReturnStatusProvided == false;
    }

    private static bool IsZeroEntryReturned(
        CapacityFinalCalleePrefixLifecyclePlan plan) =>
        plan.EntryPlan.Outcome == CapacityFinalCalleeEntryOutcome.ZeroPathReturned
        && plan.EntryPlan.ReturnReached
        && plan.EntryPlan.Capture.EntryValidated
        && plan.EntryPlan.Capture.EntryEcx == 0
        && plan.EntryPlan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryRawFileOffsetExclusive
        && plan.StopAtRawFileOffsetExclusive
            == ZeroReturnBoundaryRawFileOffsetExclusive;

    private static CapacityFinalCalleeEntryValidationPlan BlockBeforeValidation(
        CapacityFinalCalleeEntryValidationCapture capture,
        CapacityFinalCalleePrefixLifecyclePlan prefixLifecyclePlan) =>
        new(
            capture,
            CapacityFinalCalleeEntryValidationOutcome.UpstreamBlocked,
            prefixLifecyclePlan,
            ValidationLifecyclePlan: null,
            PrefixLifecycleOutcomeClean: false,
            ZeroEntryReturned: false,
            ValidationLifecycleReached: false,
            DeriveValidationLifecyclePrefix: false,
            ValidationOutcomeClean: false,
            UnreachedValidationEntryAssertionProvided:
                capture.Validation.CaptureValidatedEntry,
            UnreachedGlobalDwordCaptureProvided:
                capture.Validation.GlobalDword.HasValue,
            UnreachedEsi4DwordCaptureProvided:
                capture.Validation.Esi4Dword.HasValue,
            UnreachedExpectedValidationEntryESIProvided:
                capture.ExpectedValidationEntryESI.HasValue,
            StopAtRawFileOffsetExclusive:
                prefixLifecyclePlan.StopAtRawFileOffsetExclusive);
}
