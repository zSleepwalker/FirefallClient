namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeValidationLifecycleCapture(
    CapacityFinalCalleePrefixCallCapture Prefix,
    CapacityFinalCalleeFirstValidationCapture Validation,
    uint? ExpectedValidationEntryESI = null);

public enum CapacityFinalCalleeValidationLifecycleOutcome
{
    PrefixNotAdmitted,
    ValidationEntryESIMismatch,
    ValidationBlocked,
    ProjectionComplete
}

public readonly record struct CapacityFinalCalleeValidationLifecyclePlan(
    CapacityFinalCalleeValidationLifecycleCapture Capture,
    CapacityFinalCalleeValidationLifecycleOutcome Outcome,
    CapacityFinalCalleePrefixCallPlan PrefixPlan,
    CapacityFinalCalleeFirstValidationPlan ValidationPlan,
    bool PrefixOutcomeClean,
    bool DeriveValidatedEntry,
    bool DeriveValidationEntryESIFromPrefixOutgoing,
    bool ExpectedValidationEntryESIValidated,
    bool? ExpectedValidationEntryESIMatches,
    bool ValidationOutcomeClean,
    bool UnreachedGlobalDwordCaptureProvided,
    bool UnreachedEsi4DwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the bounded final-callee prefix call with its first validation window. A clean
/// bypass supplies ESI from the prefix LEA; after an opaque normal return, validation ESI is
/// independently captured.
/// </summary>
public static class CapacityFinalCalleeValidationLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        CapacityFinalCalleePrefixCallProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        CapacityFinalCalleeFirstValidationProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeValidationLifecyclePlan Project(
        CapacityFinalCalleeValidationLifecycleCapture capture)
    {
        CapacityFinalCalleePrefixCallPlan prefixPlan =
            CapacityFinalCalleePrefixCallProjection.Project(capture.Prefix);
        bool cleanSkippedCall = IsExactCleanSkippedCall(prefixPlan);
        bool cleanReturnedCall = IsExactCleanReturnedCall(prefixPlan);

        if (!cleanSkippedCall && !cleanReturnedCall)
        {
            return StopBeforeValidation(
                capture,
                prefixPlan,
                CapacityFinalCalleeValidationLifecycleOutcome.PrefixNotAdmitted,
                prefixOutcomeClean: false,
                stopAt: prefixPlan.StopAtRawFileOffsetExclusive);
        }

        bool deriveValidationEntryESIFromPrefixOutgoing = cleanSkippedCall;
        uint validationEntryESI = cleanSkippedCall
            ? prefixPlan.OutgoingESI!.Value
            : capture.Validation.EntryESI;
        bool? expectedEntryESIMatches =
            capture.ExpectedValidationEntryESI is uint expectedEntryESI
                ? validationEntryESI == expectedEntryESI
                : null;
        if (expectedEntryESIMatches == false)
        {
            return StopBeforeValidation(
                capture,
                prefixPlan,
                CapacityFinalCalleeValidationLifecycleOutcome.ValidationEntryESIMismatch,
                prefixOutcomeClean: true,
                deriveValidationEntryESIFromPrefixOutgoing:
                    deriveValidationEntryESIFromPrefixOutgoing,
                expectedValidationEntryESIValidated: true,
                expectedValidationEntryESIMatches: false,
                validationEntryESI: validationEntryESI,
                stopAt: CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset);
        }

        CapacityFinalCalleeFirstValidationCapture validationCapture =
            capture.Validation with
            {
                CaptureValidatedEntry = true,
                EntryESI = validationEntryESI
            };
        CapacityFinalCalleeFirstValidationPlan validationPlan =
            CapacityFinalCalleeFirstValidationProjection.Project(validationCapture);
        bool validationOutcomeClean = IsCleanTerminalValidationOutcome(validationPlan);

        return new(
            capture,
            validationOutcomeClean
                ? CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
                : CapacityFinalCalleeValidationLifecycleOutcome.ValidationBlocked,
            prefixPlan,
            validationPlan,
            PrefixOutcomeClean: true,
            DeriveValidatedEntry: true,
            DeriveValidationEntryESIFromPrefixOutgoing:
                deriveValidationEntryESIFromPrefixOutgoing,
            ExpectedValidationEntryESIValidated:
                capture.ExpectedValidationEntryESI.HasValue,
            ExpectedValidationEntryESIMatches: expectedEntryESIMatches,
            ValidationOutcomeClean: validationOutcomeClean,
            UnreachedGlobalDwordCaptureProvided:
                validationPlan.UnreachedGlobalDwordCaptureProvided,
            UnreachedEsi4DwordCaptureProvided:
                validationPlan.UnreachedEsi4DwordCaptureProvided,
            StopAtRawFileOffsetExclusive: validationPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanSkippedCall(
        CapacityFinalCalleePrefixCallPlan plan) =>
        plan.Outcome == CapacityFinalCalleePrefixCallOutcome.CallSkipped
        && plan.Capture.CaptureValidatedNonzeroEntry
        && plan.Capture.EntryECX != 0
        && plan.LeaEsiReached
        && plan.DerivedPrecallESI.HasValue
        && plan.BranchReached
        && plan.BranchTaken == true
        && plan.SelectedDestinationRawFileOffset
            == CapacityFinalCalleePrefixCallProjection.BranchDestinationRawFileOffset
        && plan.OutgoingESI == plan.DerivedPrecallESI
        && !plan.UnreachedPriorTestMaskCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.PushImmediateReached
        && !plan.CallCellReadReached
        && !plan.CallReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive;

    private static bool IsExactCleanReturnedCall(
        CapacityFinalCalleePrefixCallPlan plan) =>
        plan.Outcome == CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally
        && plan.Capture.CaptureValidatedNonzeroEntry
        && plan.Capture.EntryECX != 0
        && plan.LeaEsiReached
        && plan.DerivedPrecallESI.HasValue
        && plan.BranchReached
        && plan.BranchTaken == false
        && plan.SelectedDestinationRawFileOffset is null
        && plan.PushImmediateReached
        && plan.PushedImmediateVirtualAddress
            == CapacityFinalCalleePrefixCallProjection.PushedImmediateVirtualAddress
        && plan.CallCellReadReached
        && plan.CallCellTargetCaptured
        && plan.CallCellTargetVirtualAddress
            == plan.Capture.CallCellTargetVirtualAddress
        && plan.CallReached
        && plan.CallReturnedNormally == true
        && plan.OutgoingESI is null
        && !plan.UnreachedPriorTestMaskCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive;

    private static bool IsCleanTerminalValidationOutcome(
        CapacityFinalCalleeFirstValidationPlan plan) =>
        (plan.Outcome is CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
            or CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
            or CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne)
        && !plan.UnreachedGlobalDwordCaptureProvided
        && !plan.UnreachedEsi4DwordCaptureProvided;

    private static CapacityFinalCalleeValidationLifecyclePlan StopBeforeValidation(
        CapacityFinalCalleeValidationLifecycleCapture capture,
        CapacityFinalCalleePrefixCallPlan prefixPlan,
        CapacityFinalCalleeValidationLifecycleOutcome outcome,
        bool prefixOutcomeClean,
        bool deriveValidationEntryESIFromPrefixOutgoing = false,
        bool expectedValidationEntryESIValidated = false,
        bool? expectedValidationEntryESIMatches = null,
        uint? validationEntryESI = null,
        uint stopAt = LifecycleStartRawFileOffset)
    {
        CapacityFinalCalleeFirstValidationCapture unvalidatedCapture =
            capture.Validation with
            {
                CaptureValidatedEntry = false,
                EntryESI = validationEntryESI ?? capture.Validation.EntryESI
            };
        CapacityFinalCalleeFirstValidationPlan validationPlan =
            CapacityFinalCalleeFirstValidationProjection.Project(unvalidatedCapture);
        return new(
            capture,
            outcome,
            prefixPlan,
            validationPlan,
            PrefixOutcomeClean: prefixOutcomeClean,
            DeriveValidatedEntry: false,
            DeriveValidationEntryESIFromPrefixOutgoing:
                deriveValidationEntryESIFromPrefixOutgoing,
            ExpectedValidationEntryESIValidated:
                expectedValidationEntryESIValidated,
            ExpectedValidationEntryESIMatches: expectedValidationEntryESIMatches,
            ValidationOutcomeClean: false,
            UnreachedGlobalDwordCaptureProvided:
                validationPlan.UnreachedGlobalDwordCaptureProvided,
            UnreachedEsi4DwordCaptureProvided:
                validationPlan.UnreachedEsi4DwordCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
    }
}
