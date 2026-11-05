namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescriptorMaskLifecycleCapture(
    CapacityFinalCalleeDescriptorReloadCapture DescriptorCapture,
    uint? FreshFrameMinus14,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus8,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeDescriptorMaskLifecycleOutcome
{
    EntryNotValidated,
    MissingInitialFrameMinus10,
    MissingDescriptorFreshFrameMinus4,
    MissingDescriptorFreshFrameMinus10,
    DescriptorEntryEaxUnsignedBelowExit,
    MissingFreshEaxPlusC,
    DescriptorFinalCompareUnequalAtBoundary,
    ContaminatedDescriptorRoute,
    MissingFreshFrameMinus14,
    MissingFreshFrameMinus4,
    FrameMismatch,
    ContaminatedFrameMaskRoute,
    FrameMaskIncomplete,
    MissingFreshFrameMinus8,
    MissingFreshGlobalValue,
    GlobalValueUpdated
}

public readonly record struct CapacityFinalCalleeDescriptorMaskLifecyclePlan(
    CapacityFinalCalleeDescriptorMaskLifecycleCapture Capture,
    CapacityFinalCalleeDescriptorReloadPlan DescriptorPlan,
    CapacityFinalCalleeDescriptorMaskLifecycleOutcome Outcome,
    bool DescriptorEqualityDestinationReached,
    bool FrameMaskUpdateReached,
    bool FrameMaskRouteAdmitted,
    uint? FrameMaskValidatedEntryEBP,
    uint? FrameMaskValidatedEntryECX,
    CapacityFinalCalleeFrameMaskUpdatePlan? FrameMaskPlan,
    bool UnreachedFreshFrameMinus14CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins descriptor reload to the local frame-mask window only at a clean
/// equality destination. The initial [EBP-10] value remains ECX across this
/// no-call path; all later frame and global reads remain independent captures.
/// </summary>
public static class CapacityFinalCalleeDescriptorMaskLifecycleProjection
{
    public const uint DescriptorEntryRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset;
    public const uint DescriptorEqualityDestinationRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.EqualityDestinationRawFileOffset;
    public const uint UnselectedDescriptorDestinationRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.BelowDestinationRawFileOffset;
    public const uint FrameMaskEntryRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset;
    public const uint FrameMaskMismatchDestinationRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.MismatchDestinationRawFileOffset;
    public const uint FrameMaskContinuationRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.ContinuationRawFileOffset;

    public static CapacityFinalCalleeDescriptorMaskLifecyclePlan Project(
        CapacityFinalCalleeDescriptorMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan =
            CapacityFinalCalleeDescriptorReloadProjection.Project(
                capture.DescriptorCapture);

        bool firstEqualityDestination =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && descriptorPlan.FirstBranchReached
            && descriptorPlan.FirstBranchTaken == true
            && descriptorPlan.FirstSelectedDestinationRawFileOffset
                == DescriptorEqualityDestinationRawFileOffset
            && descriptorPlan.StopAtRawFileOffsetExclusive
                == DescriptorEqualityDestinationRawFileOffset;
        bool finalEqualityDestination =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
            && descriptorPlan.FinalBranchTaken == true
            && descriptorPlan.FinalSelectedDestinationRawFileOffset
                == DescriptorEqualityDestinationRawFileOffset
            && descriptorPlan.StopAtRawFileOffsetExclusive
                == DescriptorEqualityDestinationRawFileOffset;
        bool equalityDestinationReached =
            firstEqualityDestination || finalEqualityDestination;
        if (!equalityDestinationReached)
        {
            bool maskCaptureProvided = HasFrameMaskCapture(capture);
            bool descriptorCaptureUnreached =
                descriptorPlan.UnreachedInitialFrameMinus10CaptureProvided
                || descriptorPlan.UnreachedFreshFrameMinus4CaptureProvided
                || descriptorPlan.UnreachedFreshFrameMinus10CaptureProvided
                || descriptorPlan.UnreachedFreshEaxPlusCCaptureProvided;
            if (maskCaptureProvided || descriptorCaptureUnreached)
            {
                return BuildPlan(
                    capture,
                    descriptorPlan,
                    CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                        .ContaminatedDescriptorRoute,
                    unreachedFreshFrameMinus14CaptureProvided:
                        capture.FreshFrameMinus14.HasValue,
                    unreachedFreshFrameMinus4CaptureProvided:
                        capture.FreshFrameMinus4.HasValue,
                    unreachedFreshFrameMinus8CaptureProvided:
                        capture.FreshFrameMinus8.HasValue,
                    unreachedFreshGlobalValueCaptureProvided:
                        capture.FreshGlobalValue.HasValue,
                    stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
            }

            return BuildPlan(
                capture,
                descriptorPlan,
                MapDescriptorOutcome(descriptorPlan.Outcome),
                stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
        }

        bool descriptorRouteClean =
            !descriptorPlan.UnreachedInitialFrameMinus10CaptureProvided
            && !descriptorPlan.UnreachedFreshFrameMinus4CaptureProvided
            && !descriptorPlan.UnreachedFreshFrameMinus10CaptureProvided
            && !descriptorPlan.UnreachedFreshEaxPlusCCaptureProvided;
        if (!descriptorRouteClean
            || !descriptorPlan.InitialFrameMinus10Captured
            || descriptorPlan.InitialFrameMinus10 is not uint initialFrameMinus10
            || !capture.DescriptorCapture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .ContaminatedDescriptorRoute,
                descriptorEqualityDestinationReached: true,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: DescriptorEqualityDestinationRawFileOffset);
        }

        bool frameMaskShiftReached =
            capture.FreshFrameMinus14 is uint freshFrameMinus14
            && capture.FreshFrameMinus4 is uint freshFrameMinus4
            && freshFrameMinus14 == freshFrameMinus4
            && capture.FreshFrameMinus8.HasValue;
        uint? frameMaskEntryEcx =
            frameMaskShiftReached ? initialFrameMinus10 : null;

        CapacityFinalCalleeFrameMaskUpdatePlan frameMaskPlan =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(
                new CapacityFinalCalleeFrameMaskUpdateCapture(
                    ValidatedEntryEBP: capture.DescriptorCapture.EntryEBP,
                    ValidatedEntryECX: frameMaskEntryEcx,
                    FreshFrameMinus14: capture.FreshFrameMinus14,
                    FreshFrameMinus4: capture.FreshFrameMinus4,
                    FreshFrameMinus8: capture.FreshFrameMinus8,
                    FreshGlobalValue: capture.FreshGlobalValue));

        bool cleanMismatch = IsExactCleanFrameMismatch(frameMaskPlan);
        bool cleanGlobalUpdate = IsExactCleanGlobalUpdate(frameMaskPlan);
        CapacityFinalCalleeDescriptorMaskLifecycleOutcome frameMaskOutcome =
            MapFrameMaskOutcome(frameMaskPlan.Outcome);
        if (frameMaskPlan.Outcome
                == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
            && !cleanMismatch)
        {
            frameMaskOutcome = HasUnreachedFrameMaskCaptures(frameMaskPlan)
                ? CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute
                : CapacityFinalCalleeDescriptorMaskLifecycleOutcome.FrameMaskIncomplete;
        }
        else if (frameMaskPlan.Outcome
                    == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
                 && !cleanGlobalUpdate)
        {
            frameMaskOutcome = HasUnreachedFrameMaskCaptures(frameMaskPlan)
                ? CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute
                : CapacityFinalCalleeDescriptorMaskLifecycleOutcome.FrameMaskIncomplete;
        }

        return BuildPlan(
            capture,
            descriptorPlan,
            frameMaskOutcome,
            descriptorEqualityDestinationReached: true,
            frameMaskUpdateReached: true,
            frameMaskRouteAdmitted: cleanMismatch || cleanGlobalUpdate,
            frameMaskValidatedEntryEBP: capture.DescriptorCapture.EntryEBP,
            frameMaskValidatedEntryECX: frameMaskEntryEcx,
            frameMaskPlan: frameMaskPlan,
            unreachedFreshFrameMinus14CaptureProvided:
                frameMaskPlan.UnreachedFreshFrameMinus14CaptureProvided,
            unreachedFreshFrameMinus4CaptureProvided:
                frameMaskPlan.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshFrameMinus8CaptureProvided:
                frameMaskPlan.UnreachedFreshFrameMinus8CaptureProvided,
            unreachedFreshGlobalValueCaptureProvided:
                frameMaskPlan.UnreachedFreshGlobalValueCaptureProvided,
            stopAt: frameMaskPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool HasFrameMaskCapture(
        CapacityFinalCalleeDescriptorMaskLifecycleCapture capture) =>
        capture.FreshFrameMinus14.HasValue
        || capture.FreshFrameMinus4.HasValue
        || capture.FreshFrameMinus8.HasValue
        || capture.FreshGlobalValue.HasValue;

    private static CapacityFinalCalleeDescriptorMaskLifecycleOutcome MapDescriptorOutcome(
        CapacityFinalCalleeDescriptorReloadOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeDescriptorReloadOutcome.EntryNotValidated =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.EntryNotValidated,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingInitialFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .MissingDescriptorFreshFrameMinus4,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus10 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .MissingDescriptorFreshFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .DescriptorEntryEaxUnsignedBelowExit,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshEaxPlusC =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshEaxPlusC,
            CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome
                    .DescriptorFinalCompareUnequalAtBoundary,
            CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
                or CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute,
            _ => throw new InvalidOperationException(
                $"Unexpected descriptor-reload outcome: {outcome}.")
        };

    private static CapacityFinalCalleeDescriptorMaskLifecycleOutcome MapFrameMaskOutcome(
        CapacityFinalCalleeFrameMaskUpdateOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryECX =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.ContaminatedDescriptorRoute,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshFrameMinus14,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshFrameMinus4,
            CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.FrameMismatch,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus8 =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshFrameMinus8,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.MissingFreshGlobalValue,
            CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated =>
                CapacityFinalCalleeDescriptorMaskLifecycleOutcome.GlobalValueUpdated,
            _ => throw new InvalidOperationException(
                $"Unexpected frame-mask outcome: {outcome}.")
        };

    private static bool IsExactCleanFrameMismatch(
        CapacityFinalCalleeFrameMaskUpdatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch
        && plan.FreshFrameMinus14ReadReached
        && plan.FreshFrameMinus14Captured
        && plan.FreshFrameMinus4ReadReached
        && plan.FreshFrameMinus4Captured
        && plan.CompareReached
        && plan.CompareFlags is { Zero: false }
        && plan.Equal == false
        && plan.BranchReached
        && plan.BranchTaken == true
        && plan.SelectedBranchDestinationRawFileOffset
            == FrameMaskMismatchDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == FrameMaskMismatchDestinationRawFileOffset
        && !HasUnreachedFrameMaskCaptures(plan);

    private static bool IsExactCleanGlobalUpdate(
        CapacityFinalCalleeFrameMaskUpdatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated
        && plan.FreshFrameMinus14ReadReached
        && plan.FreshFrameMinus14Captured
        && plan.FreshFrameMinus4ReadReached
        && plan.FreshFrameMinus4Captured
        && plan.CompareReached
        && plan.CompareFlags is { Zero: true }
        && plan.Equal == true
        && plan.BranchReached
        && plan.BranchTaken == false
        && plan.SelectedBranchDestinationRawFileOffset is null
        && plan.ShiftReached
        && plan.FreshFrameMinus8ReadReached
        && plan.FreshFrameMinus8Captured
        && plan.GlobalReadReached
        && plan.GlobalValueCaptured
        && plan.GlobalValueBefore is not null
        && plan.GlobalWriteReached
        && plan.GlobalValueAfter is not null
        && plan.StopAtRawFileOffsetExclusive == FrameMaskContinuationRawFileOffset
        && !HasUnreachedFrameMaskCaptures(plan);

    private static bool HasUnreachedFrameMaskCaptures(
        CapacityFinalCalleeFrameMaskUpdatePlan plan) =>
        plan.UnreachedEntryECXCaptureProvided
        || plan.UnreachedFreshFrameMinus14CaptureProvided
        || plan.UnreachedFreshFrameMinus4CaptureProvided
        || plan.UnreachedFreshFrameMinus8CaptureProvided
        || plan.UnreachedFreshGlobalValueCaptureProvided;

    private static CapacityFinalCalleeDescriptorMaskLifecyclePlan BuildPlan(
        CapacityFinalCalleeDescriptorMaskLifecycleCapture capture,
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan,
        CapacityFinalCalleeDescriptorMaskLifecycleOutcome outcome,
        bool descriptorEqualityDestinationReached = false,
        bool frameMaskUpdateReached = false,
        bool frameMaskRouteAdmitted = false,
        uint? frameMaskValidatedEntryEBP = null,
        uint? frameMaskValidatedEntryECX = null,
        CapacityFinalCalleeFrameMaskUpdatePlan? frameMaskPlan = null,
        bool unreachedFreshFrameMinus14CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshFrameMinus8CaptureProvided = false,
        bool unreachedFreshGlobalValueCaptureProvided = false,
        uint? stopAt = null) =>
        new(
            capture,
            descriptorPlan,
            outcome,
            descriptorEqualityDestinationReached,
            frameMaskUpdateReached,
            frameMaskRouteAdmitted,
            frameMaskUpdateReached ? frameMaskValidatedEntryEBP : null,
            frameMaskUpdateReached ? frameMaskValidatedEntryECX : null,
            frameMaskUpdateReached ? frameMaskPlan : null,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus14CaptureProvided
                : unreachedFreshFrameMinus14CaptureProvided
                    || capture.FreshFrameMinus14.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus4CaptureProvided
                : unreachedFreshFrameMinus4CaptureProvided
                    || capture.FreshFrameMinus4.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus8CaptureProvided
                : unreachedFreshFrameMinus8CaptureProvided
                    || capture.FreshFrameMinus8.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshGlobalValueCaptureProvided
                : unreachedFreshGlobalValueCaptureProvided
                    || capture.FreshGlobalValue.HasValue,
            stopAt ?? descriptorPlan.StopAtRawFileOffsetExclusive);
}
