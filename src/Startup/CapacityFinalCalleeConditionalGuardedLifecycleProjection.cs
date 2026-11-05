namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeConditionalGuardedLifecycleCapture(
    CapacityFinalCalleeConditionalFieldStoreCapture ConditionalFieldStoreCapture,
    uint? GuardedFreshGlobalForCompare,
    uint? IndependentlySuppliedGuardedEntryESI,
    uint? GuardedFreshEsiPlus10);

public enum CapacityFinalCalleeConditionalGuardedLifecycleOutcome
{
    ConditionalFieldStoreIncomplete,
    ConditionalFieldStoreContaminated,
    ConditionalUnsignedBelowToValidation,
    ConditionalEaxZeroExit,
    GuardedFieldStoreIncomplete,
    GuardedFieldStoreContaminated,
    GuardedUnsignedBelowToValidation,
    GuardedFreshEsiPlus10ZeroToTarget,
    GuardedFreshEsiPlus10NonzeroToBoundary
}

public sealed record CapacityFinalCalleeConditionalGuardedLifecyclePlan(
    CapacityFinalCalleeConditionalGuardedLifecycleCapture Capture,
    CapacityFinalCalleeConditionalFieldStorePlan ConditionalFieldStore,
    CapacityFinalCalleeGuardedFieldStorePlan? GuardedFieldStore,
    uint? GuardedEntryEAX,
    uint? GuardedEntryECX,
    uint? GuardedEntryESI,
    bool GuardedEntryESIDerivedFromConditionalCapture,
    bool GuardedFieldStoreReached,
    bool UnreachedGuardedFreshGlobalForCompareCaptureProvided,
    bool UnreachedIndependentlySuppliedGuardedEntryESICaptureProvided,
    bool UnreachedGuardedFreshEsiPlus10CaptureProvided,
    bool ExactConditionalBoundaryAdmitted,
    CapacityFinalCalleeConditionalGuardedLifecycleOutcome Outcome,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the conditional stores at 9058 to the guarded store only on the exact
/// nonzero boundary. Retained entry registers are carried only when the prior
/// projection actually captured them; all later memory values remain independent.
/// </summary>
public static class CapacityFinalCalleeConditionalGuardedLifecycleProjection
{
    public const uint ConditionalEntryRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection.EntryRawFileOffset;
    public const uint GuardedEntryRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection.EntryRawFileOffset;
    public const uint ConditionalZeroDestinationRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection.ZeroBranchDestinationRawFileOffset;
    public const uint ValidationDestinationRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection.FirstBranchDestinationRawFileOffset;
    public const uint ConditionalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeConditionalFieldStoreProjection.BoundaryRawFileOffsetExclusive;
    public const uint GuardedZeroDestinationRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection.ZeroDestinationRawFileOffset;
    public const uint GuardedBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGuardedFieldStoreProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeConditionalGuardedLifecyclePlan Project(
        CapacityFinalCalleeConditionalGuardedLifecycleCapture capture)
    {
        CapacityFinalCalleeConditionalFieldStorePlan conditional =
            CapacityFinalCalleeConditionalFieldStoreProjection.Project(
                capture.ConditionalFieldStoreCapture);

        if (IsExactConditionalZeroExit(conditional))
        {
            bool conditionalContamination = HasUnreachedConditionalCaptures(conditional);
            bool guardedContamination = HasGuardedCaptures(capture);
            return Build(
                capture,
                conditional,
                guardedFieldStore: null,
                CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .ConditionalEaxZeroExit,
                guardedFieldStoreReached: false,
                unreachedGuardedFreshGlobalForCompareCaptureProvided:
                    capture.GuardedFreshGlobalForCompare.HasValue,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided:
                    capture.GuardedFreshEsiPlus10.HasValue,
                exactConditionalBoundaryAdmitted: false,
                admitted: !conditionalContamination && !guardedContamination,
                stopAt: conditional.StopAtRawFileOffsetExclusive,
                outcomeOverride: conditionalContamination || guardedContamination
                    ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalFieldStoreContaminated
                    : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalEaxZeroExit);
        }

        if (IsExactConditionalBelowValidation(conditional))
        {
            bool contaminated =
                HasUnreachedConditionalCaptures(conditional)
                || HasGuardedCaptures(capture);
            return Build(
                capture,
                conditional,
                guardedFieldStore: null,
                contaminated
                    ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalFieldStoreContaminated
                    : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalUnsignedBelowToValidation,
                guardedFieldStoreReached: false,
                unreachedGuardedFreshGlobalForCompareCaptureProvided:
                    capture.GuardedFreshGlobalForCompare.HasValue,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided:
                    capture.GuardedFreshEsiPlus10.HasValue,
                exactConditionalBoundaryAdmitted: false,
                admitted: !contaminated,
                stopAt: conditional.StopAtRawFileOffsetExclusive);
        }

        if (!IsExactConditionalNonzeroBoundary(conditional))
        {
            bool contaminated =
                HasUnreachedConditionalCaptures(conditional)
                || HasGuardedCaptures(capture);
            return Build(
                capture,
                conditional,
                guardedFieldStore: null,
                contaminated
                    ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalFieldStoreContaminated
                    : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .ConditionalFieldStoreIncomplete,
                guardedFieldStoreReached: false,
                unreachedGuardedFreshGlobalForCompareCaptureProvided:
                    capture.GuardedFreshGlobalForCompare.HasValue,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided:
                    capture.GuardedFreshEsiPlus10.HasValue,
                exactConditionalBoundaryAdmitted: false,
                admitted: false,
                stopAt: conditional.StopAtRawFileOffsetExclusive);
        }

        uint retainedEax = capture.ConditionalFieldStoreCapture.EntryEAX!.Value;
        CapacityFinalCalleeGuardedFieldStorePlan guardPrefix =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(new(
                EntryEAX: conditional.EntryEAXCaptured ? retainedEax : null,
                FreshGlobalForCompare: capture.GuardedFreshGlobalForCompare,
                EntryECX: null,
                EntryESI: null,
                FreshEsiPlus10: null));

        if (guardPrefix.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .EntryEAXUnsignedBelowGlobalToValidation)
        {
            bool contaminated =
                capture.IndependentlySuppliedGuardedEntryESI.HasValue
                || capture.GuardedFreshEsiPlus10.HasValue;
            bool exactGuardExit =
                guardPrefix.ExactCleanExitAdmitted
                && !contaminated
                && HasAllGuardCompareFlags(guardPrefix);
            return Build(
                capture,
                conditional,
                guardPrefix,
                contaminated
                    ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .GuardedFieldStoreContaminated
                    : exactGuardExit
                        ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                            .GuardedUnsignedBelowToValidation
                        : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                            .GuardedFieldStoreIncomplete,
                guardedFieldStoreReached: true,
                unreachedGuardedFreshGlobalForCompareCaptureProvided: false,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided:
                    capture.GuardedFreshEsiPlus10.HasValue,
                exactConditionalBoundaryAdmitted: true,
                admitted: exactGuardExit && !contaminated,
                stopAt: guardPrefix.StopAtRawFileOffsetExclusive,
                guardedEntryEAX: retainedEax);
        }

        if (guardPrefix.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .MissingFreshGlobalForCompare
            || guardPrefix.Outcome
                == CapacityFinalCalleeGuardedFieldStoreOutcome
                    .MissingEntryEAXForCompare)
        {
            bool contaminated =
                guardPrefix.UnreachedEntryECXCaptureProvided
                || guardPrefix.UnreachedEntryESICaptureProvided
                || guardPrefix.UnreachedFreshEsiPlus10CaptureProvided
                || capture.IndependentlySuppliedGuardedEntryESI.HasValue
                || capture.GuardedFreshEsiPlus10.HasValue;
            return Build(
                capture,
                conditional,
                guardPrefix,
                contaminated
                    ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .GuardedFieldStoreContaminated
                    : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                        .GuardedFieldStoreIncomplete,
                guardedFieldStoreReached: true,
                unreachedGuardedFreshGlobalForCompareCaptureProvided: false,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided:
                    capture.GuardedFreshEsiPlus10.HasValue,
                exactConditionalBoundaryAdmitted: true,
                admitted: false,
                stopAt: guardPrefix.StopAtRawFileOffsetExclusive,
                guardedEntryEAX: guardPrefix.EntryEAXCaptured ? retainedEax : null);
        }

        uint? guardedEcx = conditional.EntryECXCaptured
            ? capture.ConditionalFieldStoreCapture.EntryECX
            : null;
        bool guardedEsiDerived = conditional.EntryESICaptured;
        uint? guardedEsi = guardedEsiDerived
            ? capture.ConditionalFieldStoreCapture.EntryESI
            : capture.IndependentlySuppliedGuardedEntryESI;

        CapacityFinalCalleeGuardedFieldStorePlan guarded =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(new(
                EntryEAX: conditional.EntryEAXCaptured ? retainedEax : null,
                FreshGlobalForCompare: capture.GuardedFreshGlobalForCompare,
                EntryECX: guardedEcx,
                EntryESI: guardedEsi,
                FreshEsiPlus10: capture.GuardedFreshEsiPlus10));
        bool guardContaminated =
            HasUnreachedGuardedCaptures(guarded)
            || (guardedEsiDerived
                && capture.IndependentlySuppliedGuardedEntryESI.HasValue)
            || (!guardedEsiDerived
                && !capture.IndependentlySuppliedGuardedEntryESI.HasValue);
        if (guarded.ExactCleanExitAdmitted
            && HasExactGuardOutcome(guarded)
            && !guardContaminated)
        {
            CapacityFinalCalleeConditionalGuardedLifecycleOutcome outcome =
                guarded.Outcome switch
                {
                    CapacityFinalCalleeGuardedFieldStoreOutcome
                        .FreshEsiPlus10ZeroToTarget =>
                        CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                            .GuardedFreshEsiPlus10ZeroToTarget,
                    CapacityFinalCalleeGuardedFieldStoreOutcome
                        .FreshEsiPlus10NonzeroToBoundary =>
                        CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                            .GuardedFreshEsiPlus10NonzeroToBoundary,
                    _ => throw new InvalidOperationException(
                        "Unexpected exact guarded-store outcome.")
                };
            return Build(
                capture,
                conditional,
                guarded,
                outcome,
                guardedFieldStoreReached: true,
                unreachedGuardedFreshGlobalForCompareCaptureProvided: false,
                unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                    guardedEsiDerived
                        && capture.IndependentlySuppliedGuardedEntryESI.HasValue,
                unreachedGuardedFreshEsiPlus10CaptureProvided: false,
                exactConditionalBoundaryAdmitted: true,
                admitted: true,
                stopAt: guarded.StopAtRawFileOffsetExclusive,
                guardedEntryEAX: retainedEax,
                guardedEntryECX: guarded.EntryECXCaptured ? guardedEcx : null,
                guardedEntryESI: guarded.EntryESICaptured ? guardedEsi : null,
                guardedEntryESIDerivedFromConditionalCapture: guardedEsiDerived);
        }

        bool laterCaptureProvided =
            guarded.UnreachedFreshGlobalForCompareCaptureProvided
            || guarded.UnreachedEntryECXCaptureProvided
            || guarded.UnreachedEntryESICaptureProvided
            || guarded.UnreachedFreshEsiPlus10CaptureProvided
            || capture.IndependentlySuppliedGuardedEntryESI.HasValue;
        bool incompleteOrContaminated = guardContaminated || laterCaptureProvided;
        return Build(
            capture,
            conditional,
            guarded,
            incompleteOrContaminated
                ? CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFieldStoreContaminated
                : CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                    .GuardedFieldStoreIncomplete,
            guardedFieldStoreReached: true,
            unreachedGuardedFreshGlobalForCompareCaptureProvided:
                guarded.UnreachedFreshGlobalForCompareCaptureProvided,
            unreachedIndependentlySuppliedGuardedEntryESICaptureProvided:
                capture.IndependentlySuppliedGuardedEntryESI.HasValue,
            unreachedGuardedFreshEsiPlus10CaptureProvided:
                guarded.UnreachedFreshEsiPlus10CaptureProvided,
            exactConditionalBoundaryAdmitted: true,
            admitted: false,
            stopAt: guarded.StopAtRawFileOffsetExclusive,
            guardedEntryEAX: guarded.EntryEAXCaptured ? retainedEax : null,
            guardedEntryECX: guarded.EntryECXCaptured ? guardedEcx : null,
            guardedEntryESI: guarded.EntryESICaptured ? guardedEsi : null,
            guardedEntryESIDerivedFromConditionalCapture: guardedEsiDerived);
    }

    private static bool IsExactConditionalBelowValidation(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeConditionalFieldStoreOutcome
                .EntryECXUnsignedBelowEntryEBXToValidation
        && plan.EntryECXCaptured
        && plan.EntryEBXCaptured
        && plan.FirstCompareFlags.HasValue
        && plan.FirstBranchReached
        && plan.FirstBranchTaken == true
        && plan.FirstBranchSelectedDestinationRawFileOffset
            == ValidationDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == ValidationDestinationRawFileOffset;

    private static bool IsExactConditionalZeroExit(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.Outcome is
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxZeroToTarget
            or CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxZeroToTarget
        && HasCompleteConditionalStorePrefix(plan)
        && HasAllConditionalFlags(plan)
        && plan.TestEAXFlags?.Zero == true
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == true
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == ConditionalZeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == ConditionalZeroDestinationRawFileOffset
        && HasAllConditionalFlags(plan)
        && !HasUnreachedConditionalCaptures(plan);

    private static bool IsExactConditionalNonzeroBoundary(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.Outcome is
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxNonzeroToBoundary
            or CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxNonzeroToBoundary
        && HasCompleteConditionalStorePrefix(plan)
        && plan.TestEAXFlags?.Zero == false
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == false
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == ConditionalBoundaryRawFileOffsetExclusive
        && plan.StopAtRawFileOffsetExclusive
            == ConditionalBoundaryRawFileOffsetExclusive
        && HasAllConditionalFlags(plan)
        && !HasUnreachedConditionalCaptures(plan);

    private static bool HasCompleteConditionalStorePrefix(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.EntryECXCaptured
        && plan.EntryEBXCaptured
        && plan.FreshEcxPlus10ReadReached
        && plan.FreshEcxPlus10Captured
        && plan.EntryESICaptured
        && plan.SecondBranchReached
        && plan.EntryEAXCaptured
        && plan.StoreReached
        && plan.StoreAddress.HasValue
        && plan.StoreValue.HasValue
        && plan.StoreDisplacement is
            CapacityFinalCalleeConditionalFieldStoreProjection.Plus10StoreDisplacement
            or CapacityFinalCalleeConditionalFieldStoreProjection.Plus14StoreDisplacement;

    private static bool HasAllConditionalFlags(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.FirstCompareFlags.HasValue
        && plan.SecondCompareFlags.HasValue
        && plan.TestEAXFlags.HasValue;

    private static bool HasAllGuardCompareFlags(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.CompareFlags.HasValue && plan.BranchReached;

    private static bool HasExactGuardOutcome(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.Outcome is
            CapacityFinalCalleeGuardedFieldStoreOutcome
                .FreshEsiPlus10ZeroToTarget
            or CapacityFinalCalleeGuardedFieldStoreOutcome
                .FreshEsiPlus10NonzeroToBoundary
        && plan.CompareFlags.HasValue
        && plan.TestECXFlags.HasValue
        && plan.StoreReached
        && plan.FreshEsiPlus10ReadReached
        && plan.FreshEsiPlus10Captured
        && plan.TestECXReached
        && plan.ZeroBranchReached
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == (plan.ZeroBranchTaken == true
                ? GuardedZeroDestinationRawFileOffset
                : GuardedBoundaryRawFileOffsetExclusive)
        && plan.StopAtRawFileOffsetExclusive
            == plan.ZeroBranchSelectedDestinationRawFileOffset;

    private static bool HasUnreachedConditionalCaptures(
        CapacityFinalCalleeConditionalFieldStorePlan plan) =>
        plan.UnreachedEntryEBXCaptureProvided
        || plan.UnreachedFreshEcxPlus10CaptureProvided
        || plan.UnreachedEntryESICaptureProvided
        || plan.UnreachedEntryEAXCaptureProvided;

    private static bool HasGuardedCaptures(
        CapacityFinalCalleeConditionalGuardedLifecycleCapture capture) =>
        capture.GuardedFreshGlobalForCompare.HasValue
        || capture.IndependentlySuppliedGuardedEntryESI.HasValue
        || capture.GuardedFreshEsiPlus10.HasValue;

    private static bool HasUnreachedGuardedCaptures(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.UnreachedFreshGlobalForCompareCaptureProvided
        || plan.UnreachedEntryECXCaptureProvided
        || plan.UnreachedEntryESICaptureProvided
        || plan.UnreachedFreshEsiPlus10CaptureProvided;

    private static CapacityFinalCalleeConditionalGuardedLifecyclePlan Build(
        CapacityFinalCalleeConditionalGuardedLifecycleCapture capture,
        CapacityFinalCalleeConditionalFieldStorePlan conditional,
        CapacityFinalCalleeGuardedFieldStorePlan? guardedFieldStore,
        CapacityFinalCalleeConditionalGuardedLifecycleOutcome outcome,
        bool guardedFieldStoreReached,
        bool unreachedGuardedFreshGlobalForCompareCaptureProvided,
        bool unreachedIndependentlySuppliedGuardedEntryESICaptureProvided,
        bool unreachedGuardedFreshEsiPlus10CaptureProvided,
        bool exactConditionalBoundaryAdmitted,
        bool admitted,
        uint stopAt,
        uint? guardedEntryEAX = null,
        uint? guardedEntryECX = null,
        uint? guardedEntryESI = null,
        bool guardedEntryESIDerivedFromConditionalCapture = false,
        CapacityFinalCalleeConditionalGuardedLifecycleOutcome? outcomeOverride = null) =>
        new(
            capture,
            conditional,
            guardedFieldStore,
            guardedEntryEAX,
            guardedEntryECX,
            guardedEntryESI,
            guardedEntryESIDerivedFromConditionalCapture,
            guardedFieldStoreReached,
            unreachedGuardedFreshGlobalForCompareCaptureProvided,
            unreachedIndependentlySuppliedGuardedEntryESICaptureProvided,
            unreachedGuardedFreshEsiPlus10CaptureProvided,
            exactConditionalBoundaryAdmitted,
            outcomeOverride ?? outcome,
            admitted,
            stopAt);
}
