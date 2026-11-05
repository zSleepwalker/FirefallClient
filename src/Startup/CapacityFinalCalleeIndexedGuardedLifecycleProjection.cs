namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedGuardedLifecycleCapture(
    CapacityFinalCalleeIndexedConditionalLifecycleCapture IndexedConditionalCapture,
    uint? GuardedFreshGlobalForCompare,
    uint? GuardedFreshEsiPlus10);

public enum CapacityFinalCalleeIndexedGuardedLifecycleOutcome
{
    IndexedConditionalIncomplete,
    IndexedConditionalContaminated,
    IndexedRootZeroExit,
    IndexedRootNonzeroTableWriteExit,
    IndexedRootMaskContinuation,
    ConditionalUnsignedBelowToValidation,
    ConditionalEaxZeroExit,
    DownstreamGuardCaptureOnBypass,
    GuardedIncomplete,
    GuardedContaminated,
    GuardedUnsignedBelowToValidation,
    GuardedFreshEsiPlus10ZeroToTarget,
    GuardedFreshEsiPlus10NonzeroToBoundary
}

public sealed record CapacityFinalCalleeIndexedGuardedLifecyclePlan(
    CapacityFinalCalleeIndexedGuardedLifecycleCapture Capture,
    CapacityFinalCalleeIndexedConditionalLifecyclePlan IndexedConditionalLifecycle,
    CapacityFinalCalleeGuardedFieldStorePlan? GuardedFieldStore,
    CapacityFinalCalleeIndexedGuardedLifecycleOutcome Outcome,
    bool GuardedFieldStoreReached,
    uint? GuardedEntryEAX,
    uint? GuardedEntryECX,
    uint? GuardedEntryESI,
    bool GuardedEntryECXDerived,
    bool GuardedEntryESIDerived,
    bool UnreachedGuardedFreshGlobalForCompareCaptureProvided,
    bool UnreachedGuardedFreshEsiPlus10CaptureProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins only the exact nonzero conditional-store boundary at 9058 to the
/// independent guarded store. The indexed root's separate nonzero table-write
/// exit at the same address remains a bounded root outcome.
/// </summary>
public static class CapacityFinalCalleeIndexedGuardedLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeIndexedConditionalLifecycleProjection.EntryRawFileOffset;
    public const uint ConditionalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeIndexedConditionalLifecycleProjection
            .ConditionalBoundaryRawFileOffsetExclusive;
    public const uint GuardedValidationDestinationRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection
            .ValidationDestinationRawFileOffset;
    public const uint GuardedZeroDestinationRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection.ZeroDestinationRawFileOffset;
    public const uint GuardedBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGuardedFieldStoreProjection
            .BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeIndexedGuardedLifecyclePlan Project(
        CapacityFinalCalleeIndexedGuardedLifecycleCapture capture)
    {
        CapacityFinalCalleeIndexedConditionalLifecyclePlan conditional =
            CapacityFinalCalleeIndexedConditionalLifecycleProjection.Project(
                capture.IndexedConditionalCapture);

        if (!IsExactConditionalNonzeroBoundary(conditional))
        {
            bool unreachedGlobal = capture.GuardedFreshGlobalForCompare.HasValue;
            bool unreachedFresh = capture.GuardedFreshEsiPlus10.HasValue;
            bool downstreamCaptureProvided = unreachedGlobal || unreachedFresh;
            CapacityFinalCalleeIndexedGuardedLifecycleOutcome outcome =
                downstreamCaptureProvided
                    ? CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                        .DownstreamGuardCaptureOnBypass
                    : MapBoundedConditionalOutcome(conditional);
            return new(
                capture,
                conditional,
                GuardedFieldStore: null,
                outcome,
                GuardedFieldStoreReached: false,
                GuardedEntryEAX: null,
                GuardedEntryECX: null,
                GuardedEntryESI: null,
                GuardedEntryECXDerived: false,
                GuardedEntryESIDerived: false,
                unreachedGlobal,
                unreachedFresh,
                Admitted: !downstreamCaptureProvided && conditional.Admitted,
                StopAtRawFileOffsetExclusive:
                    conditional.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeConditionalFieldStorePlan conditionalStore =
            conditional.ConditionalFieldStore!.Value;
        CapacityFinalCalleeConditionalGuardedLifecyclePlan guarded =
            CapacityFinalCalleeConditionalGuardedLifecycleProjection.Project(new(
                conditionalStore.Capture,
                capture.GuardedFreshGlobalForCompare,
                IndependentlySuppliedGuardedEntryESI: null,
                capture.GuardedFreshEsiPlus10));
        uint? guardedEntryEcx = guarded.GuardedEntryECX;
        bool guardedEntryEsiDerived =
            guarded.GuardedEntryESIDerivedFromConditionalCapture
            && guarded.GuardedEntryESI.HasValue;
        return new(
            capture,
            conditional,
            guarded.GuardedFieldStore,
            MapGuardedOutcome(guarded.Outcome),
            guarded.GuardedFieldStoreReached,
            guarded.GuardedEntryEAX,
            guardedEntryEcx,
            guarded.GuardedEntryESI,
            GuardedEntryECXDerived: guardedEntryEcx.HasValue,
            GuardedEntryESIDerived: guardedEntryEsiDerived,
            guarded.UnreachedGuardedFreshGlobalForCompareCaptureProvided,
            guarded.UnreachedGuardedFreshEsiPlus10CaptureProvided,
            guarded.Admitted,
            guarded.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeIndexedGuardedLifecycleOutcome MapGuardedOutcome(
        CapacityFinalCalleeConditionalGuardedLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                .GuardedUnsignedBelowToValidation =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedUnsignedBelowToValidation,
            CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                .GuardedFreshEsiPlus10ZeroToTarget =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10ZeroToTarget,
            CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                .GuardedFreshEsiPlus10NonzeroToBoundary =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedFreshEsiPlus10NonzeroToBoundary,
            CapacityFinalCalleeConditionalGuardedLifecycleOutcome
                .GuardedFieldStoreContaminated =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedContaminated,
            _ =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .GuardedIncomplete
        };

    private static bool IsExactConditionalNonzeroBoundary(
        CapacityFinalCalleeIndexedConditionalLifecyclePlan conditional)
    {
        CapacityFinalCalleeConditionalFieldStorePlan? conditionalStore =
            conditional.ConditionalFieldStore;
        if (!conditional.Admitted
            || conditional.StopAtRawFileOffsetExclusive
                != ConditionalBoundaryRawFileOffsetExclusive
            || !conditional.ConditionalFieldStoreReached
            || conditionalStore is null
            || conditionalStore.Value.Capture.EntryEAX is not uint entryEax
            || entryEax == 0
            || !conditionalStore.Value.EntryEAXCaptured
            || !conditionalStore.Value.TestEAXReached
            || conditionalStore.Value.TestEAXValue != entryEax
            || conditionalStore.Value.TestEAXFlags is not { Zero: false }
            || conditionalStore.Value.ZeroBranchReached != true
            || conditionalStore.Value.ZeroBranchTaken != false
            || conditionalStore.Value.ZeroBranchSelectedDestinationRawFileOffset
                != ConditionalBoundaryRawFileOffsetExclusive
            || conditionalStore.Value.StopAtRawFileOffsetExclusive
                != ConditionalBoundaryRawFileOffsetExclusive
            || conditionalStore.Value.UnreachedEntryEBXCaptureProvided
            || conditionalStore.Value.UnreachedFreshEcxPlus10CaptureProvided
            || conditionalStore.Value.UnreachedEntryESICaptureProvided
            || conditionalStore.Value.UnreachedEntryEAXCaptureProvided
            || conditional.UnreachedConditionalEntryEBXCaptureProvided
            || conditional.UnreachedConditionalFreshEcxPlus10CaptureProvided
            || conditional.UnreachedConditionalEntryEAXCaptureProvided
            || conditional.UnreachedIndexedRootEntryESICaptureProvided
            || conditional.UnreachedIndexedRootEntryEAXCaptureProvided
            || conditional.DerivedConditionalEntryECX is null
            || conditional.PreservedConditionalEntryESI is null)
        {
            return false;
        }

        return conditional.Outcome is
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalPlus10StoredThenEaxNonzeroToBoundary
            or CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalPlus14StoredThenEaxNonzeroToBoundary;
    }

    private static CapacityFinalCalleeIndexedGuardedLifecycleOutcome
        MapBoundedConditionalOutcome(
            CapacityFinalCalleeIndexedConditionalLifecyclePlan conditional) =>
        conditional.Outcome switch
        {
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .IndexedRootMaskZeroExit =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedRootZeroExit,
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .IndexedRootMaskNonzeroTableWriteExit =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedRootNonzeroTableWriteExit,
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .IndexedRootMaskContinuation =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedRootMaskContinuation,
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalUnsignedBelowToValidation =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation,
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalPlus10StoredThenEaxZeroToMaskContinuation
            or CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalPlus14StoredThenEaxZeroToMaskContinuation =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .ConditionalEaxZeroExit,
            CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .IndexedRootMaskContaminated
            or CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalFieldStoreContaminated =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedConditionalContaminated,
            _ =>
                CapacityFinalCalleeIndexedGuardedLifecycleOutcome
                    .IndexedConditionalIncomplete
        };

}
