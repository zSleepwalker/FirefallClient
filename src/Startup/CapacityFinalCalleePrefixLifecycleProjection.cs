namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePrefixLifecycleCapture(
    CapacityFinalCalleeEntryCapture Entry,
    CapacityFinalCalleePrefixCallCapture PrefixCall);

public enum CapacityFinalCalleePrefixLifecycleOutcome
{
    EntryBlocked,
    EntryHasUnreachedCaptures,
    PrefixEntryEcxMismatch,
    ZeroPathReturned,
    ZeroPathHasUnreachedCaptures,
    PrefixCallBlocked,
    PrefixCallHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct CapacityFinalCalleePrefixLifecyclePlan(
    CapacityFinalCalleePrefixLifecycleCapture Capture,
    CapacityFinalCalleePrefixLifecycleOutcome Outcome,
    CapacityFinalCalleeEntryPlan EntryPlan,
    CapacityFinalCalleePrefixCallPlan? PrefixCallPlan,
    bool EntryOutcomeClean,
    bool EntryUnreachedCapturesClear,
    bool DeriveValidatedNonzeroEntry,
    bool ZeroEpilogueAdmitted,
    bool PrefixCallProjectionReached,
    uint? AuthoritativeEntryECX,
    bool? PrefixEntryEcxMatchesAuthoritative,
    bool PrefixValidatedNonzeroFlagMatchesDerived,
    bool? SuppliedPriorTestMaskMatchesDerived,
    bool? DerivedPriorTestMaskSet,
    bool PrefixCallOutcomeClean,
    bool UnreachedPrefixEntryEcxCaptureProvided,
    bool UnreachedPrefixPriorTestMaskCaptureProvided,
    bool UnreachedPrefixCallCellTargetCaptureProvided,
    bool UnreachedPrefixCallReturnStatusProvided,
    bool UnreachedPrefixNonzeroValidationAsserted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the captured callee-entry prefix with its bounded LEA/JZ/call window.
/// The entry capture is authoritative; this projection makes no claim after either
/// the zero-path RET or the prefix-call window.
/// </summary>
public static class CapacityFinalCalleePrefixLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        CapacityFinalCalleeEntryProjection.EntryRawFileOffset;
    public const uint NonzeroPrefixBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeEntryProjection.NonzeroPrefixBoundaryRawFileOffsetExclusive;
    public const uint PrefixCallBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleePrefixCallProjection.BoundaryRawFileOffsetExclusive;
    public const uint ZeroReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleePrefixLifecyclePlan Project(
        CapacityFinalCalleePrefixLifecycleCapture capture)
    {
        CapacityFinalCalleeEntryPlan entryPlan =
            CapacityFinalCalleeEntryProjection.Project(capture.Entry);
        bool entryUnreachedCapturesClear =
            !entryPlan.UnreachedTestedByteCaptureProvided
            && !entryPlan.UnreachedZeroPathPoppedEbpCaptureProvided;
        bool exactCleanNonzeroPrefixCompleted =
            IsExactCleanNonzeroPrefixCompleted(entryPlan);
        bool cleanZeroPathReturned =
            IsCleanZeroPathReturned(entryPlan);
        uint? authoritativeEntryEcx =
            capture.Entry.EntryValidated ? capture.Entry.EntryEcx : null;
        bool? prefixEntryEcxMatches =
            authoritativeEntryEcx is uint expectedEntryEcx
                ? capture.PrefixCall.EntryECX == expectedEntryEcx
                : null;
        bool prefixValidatedFlagMatchesDerived =
            capture.PrefixCall.CaptureValidatedNonzeroEntry
                == exactCleanNonzeroPrefixCompleted;
        bool? suppliedPriorTestMaskMatchesDerived =
            exactCleanNonzeroPrefixCompleted
                && capture.PrefixCall.CapturedPriorTestMaskSet is bool suppliedMask
                    ? suppliedMask == entryPlan.TestedBitIsSet
                    : null;

        if (cleanZeroPathReturned)
        {
            bool prefixEntryEcxMatchesEntry =
                prefixEntryEcxMatches == true;
            bool prefixOptionalCapturesUnreached =
                !capture.PrefixCall.CapturedPriorTestMaskSet.HasValue
                && !capture.PrefixCall.CallCellTargetVirtualAddress.HasValue
                && !capture.PrefixCall.CallReturnedNormally.HasValue
                && !capture.PrefixCall.CaptureValidatedNonzeroEntry;
            bool zeroPathClean =
                entryUnreachedCapturesClear
                && prefixEntryEcxMatchesEntry
                && prefixOptionalCapturesUnreached;

            return BuildPlan(
                capture,
                zeroPathClean
                    ? CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathReturned
                    : CapacityFinalCalleePrefixLifecycleOutcome.ZeroPathHasUnreachedCaptures,
                entryPlan,
                prefixCallPlan: null,
                entryOutcomeClean: true,
                entryUnreachedCapturesClear,
                deriveValidatedNonzeroEntry: false,
                zeroEpilogueAdmitted: zeroPathClean,
                prefixCallProjectionReached: false,
                authoritativeEntryEcx,
                prefixEntryEcxMatches,
                prefixValidatedFlagMatchesDerived,
                suppliedPriorTestMaskMatchesDerived: null,
                derivedPriorTestMaskSet: null,
                prefixCallOutcomeClean: false,
                unreachedPrefixEntryEcxCaptureProvided: true,
                unreachedPrefixPriorTestMaskCaptureProvided:
                    capture.PrefixCall.CapturedPriorTestMaskSet.HasValue,
                unreachedPrefixCallCellTargetCaptureProvided:
                    capture.PrefixCall.CallCellTargetVirtualAddress.HasValue,
                unreachedPrefixCallReturnStatusProvided:
                    capture.PrefixCall.CallReturnedNormally.HasValue,
                unreachedPrefixNonzeroValidationAsserted:
                    capture.PrefixCall.CaptureValidatedNonzeroEntry,
                stopAt: ZeroReturnBoundaryRawFileOffsetExclusive);
        }

        if (!exactCleanNonzeroPrefixCompleted)
        {
            bool entryHasUnreachedCaptures =
                entryPlan.UnreachedTestedByteCaptureProvided
                || entryPlan.UnreachedZeroPathPoppedEbpCaptureProvided;
            return BuildPlan(
                capture,
                entryHasUnreachedCaptures
                    ? CapacityFinalCalleePrefixLifecycleOutcome.EntryHasUnreachedCaptures
                    : CapacityFinalCalleePrefixLifecycleOutcome.EntryBlocked,
                entryPlan,
                prefixCallPlan: null,
                entryOutcomeClean: false,
                entryUnreachedCapturesClear,
                deriveValidatedNonzeroEntry: false,
                zeroEpilogueAdmitted: false,
                prefixCallProjectionReached: false,
                authoritativeEntryEcx,
                prefixEntryEcxMatches,
                prefixValidatedFlagMatchesDerived,
                suppliedPriorTestMaskMatchesDerived: null,
                derivedPriorTestMaskSet: null,
                prefixCallOutcomeClean: false,
                unreachedPrefixEntryEcxCaptureProvided: true,
                unreachedPrefixPriorTestMaskCaptureProvided:
                    capture.PrefixCall.CapturedPriorTestMaskSet.HasValue,
                unreachedPrefixCallCellTargetCaptureProvided:
                    capture.PrefixCall.CallCellTargetVirtualAddress.HasValue,
                unreachedPrefixCallReturnStatusProvided:
                    capture.PrefixCall.CallReturnedNormally.HasValue,
                unreachedPrefixNonzeroValidationAsserted:
                    capture.PrefixCall.CaptureValidatedNonzeroEntry,
                stopAt: entryPlan.StopAtRawFileOffsetExclusive);
        }

        if (prefixEntryEcxMatches != true)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixLifecycleOutcome.PrefixEntryEcxMismatch,
                entryPlan,
                prefixCallPlan: null,
                entryOutcomeClean: true,
                entryUnreachedCapturesClear: true,
                deriveValidatedNonzeroEntry: false,
                zeroEpilogueAdmitted: false,
                prefixCallProjectionReached: false,
                authoritativeEntryEcx,
                prefixEntryEcxMatches,
                prefixValidatedFlagMatchesDerived,
                suppliedPriorTestMaskMatchesDerived,
                derivedPriorTestMaskSet: entryPlan.TestedBitIsSet,
                prefixCallOutcomeClean: false,
                unreachedPrefixEntryEcxCaptureProvided: true,
                unreachedPrefixPriorTestMaskCaptureProvided:
                    capture.PrefixCall.CapturedPriorTestMaskSet.HasValue,
                unreachedPrefixCallCellTargetCaptureProvided:
                    capture.PrefixCall.CallCellTargetVirtualAddress.HasValue,
                unreachedPrefixCallReturnStatusProvided:
                    capture.PrefixCall.CallReturnedNormally.HasValue,
                unreachedPrefixNonzeroValidationAsserted:
                    capture.PrefixCall.CaptureValidatedNonzeroEntry,
                stopAt: CapacityFinalCalleePrefixCallProjection.EntryRawFileOffset);
        }

        CapacityFinalCalleePrefixCallCapture derivedPrefixCallCapture =
            capture.PrefixCall with
            {
                CaptureValidatedNonzeroEntry = true,
                EntryECX = capture.Entry.EntryEcx,
                CapturedPriorTestMaskSet = entryPlan.TestedBitIsSet
            };
        CapacityFinalCalleePrefixCallPlan prefixCallPlan =
            CapacityFinalCalleePrefixCallProjection.Project(derivedPrefixCallCapture);
        bool prefixCallOutcomeClean =
            (prefixCallPlan.Outcome is
                CapacityFinalCalleePrefixCallOutcome.CallSkipped
                or CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally)
            && !prefixCallPlan.UnreachedPriorTestMaskCaptureProvided
            && !prefixCallPlan.UnreachedCallCellTargetCaptureProvided
            && !prefixCallPlan.UnreachedCallReturnStatusProvided;
        bool prefixCallHasUnreachedCaptures =
            prefixCallPlan.UnreachedPriorTestMaskCaptureProvided
            || prefixCallPlan.UnreachedCallCellTargetCaptureProvided
            || prefixCallPlan.UnreachedCallReturnStatusProvided;

        return BuildPlan(
            capture,
            prefixCallOutcomeClean
                ? CapacityFinalCalleePrefixLifecycleOutcome.ProjectionComplete
                : prefixCallHasUnreachedCaptures
                    ? CapacityFinalCalleePrefixLifecycleOutcome.PrefixCallHasUnreachedCaptures
                    : CapacityFinalCalleePrefixLifecycleOutcome.PrefixCallBlocked,
            entryPlan,
            prefixCallPlan,
            entryOutcomeClean: true,
            entryUnreachedCapturesClear: true,
            deriveValidatedNonzeroEntry: true,
            zeroEpilogueAdmitted: false,
            prefixCallProjectionReached: true,
            authoritativeEntryEcx,
            prefixEntryEcxMatches,
            prefixValidatedFlagMatchesDerived,
            suppliedPriorTestMaskMatchesDerived,
            derivedPriorTestMaskSet: entryPlan.TestedBitIsSet,
            prefixCallOutcomeClean,
            unreachedPrefixEntryEcxCaptureProvided: false,
            unreachedPrefixPriorTestMaskCaptureProvided:
                prefixCallPlan.UnreachedPriorTestMaskCaptureProvided,
            unreachedPrefixCallCellTargetCaptureProvided:
                prefixCallPlan.UnreachedCallCellTargetCaptureProvided,
            unreachedPrefixCallReturnStatusProvided:
                prefixCallPlan.UnreachedCallReturnStatusProvided,
            unreachedPrefixNonzeroValidationAsserted: false,
            stopAt: prefixCallPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanNonzeroPrefixCompleted(
        CapacityFinalCalleeEntryPlan plan) =>
        plan.Outcome == CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted
        && plan.Capture.EntryValidated
        && plan.Capture.EntryEcx != 0
        && plan.TestByteReadReached
        && plan.TestedByteCaptured
        && plan.TestedBitIsSet.HasValue
        && plan.PushEbxReached
        && plan.PushEsiReached
        && plan.PushEdiReached
        && plan.StackPointerAfterNonzeroSaves.HasValue
        && !plan.UnreachedTestedByteCaptureProvided
        && !plan.UnreachedZeroPathPoppedEbpCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == NonzeroPrefixBoundaryRawFileOffsetExclusive;

    private static bool IsCleanZeroPathReturned(
        CapacityFinalCalleeEntryPlan plan) =>
        plan.Outcome == CapacityFinalCalleeEntryOutcome.ZeroPathReturned
        && plan.Capture.EntryValidated
        && plan.Capture.EntryEcx == 0
        && plan.ZeroPathPopEbpReached
        && plan.ReturnReached
        && plan.TestedBitIsSet is null
        && !plan.UnreachedZeroPathPoppedEbpCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == ZeroReturnBoundaryRawFileOffsetExclusive;

    private static CapacityFinalCalleePrefixLifecyclePlan BuildPlan(
        CapacityFinalCalleePrefixLifecycleCapture capture,
        CapacityFinalCalleePrefixLifecycleOutcome outcome,
        CapacityFinalCalleeEntryPlan entryPlan,
        CapacityFinalCalleePrefixCallPlan? prefixCallPlan,
        bool entryOutcomeClean,
        bool entryUnreachedCapturesClear,
        bool deriveValidatedNonzeroEntry,
        bool zeroEpilogueAdmitted,
        bool prefixCallProjectionReached,
        uint? authoritativeEntryEcx,
        bool? prefixEntryEcxMatches,
        bool prefixValidatedFlagMatchesDerived,
        bool? suppliedPriorTestMaskMatchesDerived,
        bool? derivedPriorTestMaskSet,
        bool prefixCallOutcomeClean,
        bool unreachedPrefixEntryEcxCaptureProvided,
        bool unreachedPrefixPriorTestMaskCaptureProvided,
        bool unreachedPrefixCallCellTargetCaptureProvided,
        bool unreachedPrefixCallReturnStatusProvided,
        bool unreachedPrefixNonzeroValidationAsserted,
        uint stopAt) =>
        new(
            capture,
            outcome,
            entryPlan,
            prefixCallPlan,
            entryOutcomeClean,
            entryUnreachedCapturesClear,
            deriveValidatedNonzeroEntry,
            zeroEpilogueAdmitted,
            prefixCallProjectionReached,
            authoritativeEntryEcx,
            prefixEntryEcxMatches,
            prefixValidatedFlagMatchesDerived,
            suppliedPriorTestMaskMatchesDerived,
            derivedPriorTestMaskSet,
            prefixCallOutcomeClean,
            unreachedPrefixEntryEcxCaptureProvided,
            unreachedPrefixPriorTestMaskCaptureProvided,
            unreachedPrefixCallCellTargetCaptureProvided,
            unreachedPrefixCallReturnStatusProvided,
            unreachedPrefixNonzeroValidationAsserted,
            stopAt);
}
