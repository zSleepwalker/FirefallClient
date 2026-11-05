namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedConditionalLifecycleCapture(
    CapacityFinalCalleeIndexedRootMaskCapture IndexedRootMaskCapture,
    uint? ConditionalEntryEBX,
    uint? ConditionalFreshEcxPlus10,
    uint? ConditionalEntryEAX);

public enum CapacityFinalCalleeIndexedConditionalLifecycleOutcome
{
    IndexedRootMaskIncomplete,
    IndexedRootMaskContaminated,
    IndexedRootMaskZeroExit,
    IndexedRootMaskNonzeroTableWriteExit,
    IndexedRootMaskContinuation,
    ConditionalFieldStoreIncomplete,
    ConditionalFieldStoreContaminated,
    ConditionalUnsignedBelowToValidation,
    ConditionalPlus10StoredThenEaxZeroToMaskContinuation,
    ConditionalPlus10StoredThenEaxNonzeroToBoundary,
    ConditionalPlus14StoredThenEaxZeroToMaskContinuation,
    ConditionalPlus14StoredThenEaxNonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeIndexedConditionalLifecyclePlan(
    CapacityFinalCalleeIndexedConditionalLifecycleCapture Capture,
    CapacityFinalCalleeIndexedRootMaskPlan IndexedRootMask,
    CapacityFinalCalleeConditionalFieldStorePlan? ConditionalFieldStore,
    uint? DerivedConditionalEntryECX,
    uint? PreservedConditionalEntryESI,
    CapacityFinalCalleeIndexedConditionalLifecycleOutcome Outcome,
    bool ConditionalFieldStoreReached,
    bool UnreachedConditionalEntryEBXCaptureProvided,
    bool UnreachedConditionalFreshEcxPlus10CaptureProvided,
    bool UnreachedConditionalEntryEAXCaptureProvided,
    bool UnreachedIndexedRootEntryESICaptureProvided,
    bool UnreachedIndexedRootEntryEAXCaptureProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the indexed-root unequal comparison route to the conditional stores.
/// ECX is retained from the fresh EBP-14 load; EBX, EAX, and ECX+10 are separate
/// captures because the unequal root route does not establish those values.
/// </summary>
public static class CapacityFinalCalleeIndexedConditionalLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.EntryRawFileOffset;
    public const uint ConditionalStoreEntryRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection.EntryRawFileOffset;
    public const uint ConditionalValidationDestinationRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection
            .FirstBranchDestinationRawFileOffset;
    public const uint ConditionalZeroDestinationRawFileOffset =
        CapacityFinalCalleeConditionalFieldStoreProjection
            .ZeroBranchDestinationRawFileOffset;
    public const uint ConditionalBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeConditionalFieldStoreProjection
            .BoundaryRawFileOffsetExclusive;
    public const uint ZeroExitRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.ZeroExitRawFileOffset;
    public const uint NonzeroExitRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.NonzeroExitRawFileOffset;
    public const uint MaskContinuationRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.MaskContinuationRawFileOffset;

    public static CapacityFinalCalleeIndexedConditionalLifecyclePlan Project(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture)
    {
        CapacityFinalCalleeIndexedRootMaskPlan root =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(
                capture.IndexedRootMaskCapture);

        if (IsExactRootUnequalJoin(root))
        {
            if (root.Capture.EntryEAX.HasValue
                || HasUnreachedRootCapture(root))
            {
                return RootBlocked(
                    capture,
                    root,
                    CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                        .IndexedRootMaskContaminated,
                    unreachedIndexedRootEntryEAXCaptureProvided:
                        root.Capture.EntryEAX.HasValue);
            }

            CapacityFinalCalleeConditionalFieldStoreCapture fieldStoreCapture =
                new(
                    EntryECX: root.FreshFrameMinus14,
                    EntryEBX: capture.ConditionalEntryEBX,
                    FreshEcxPlus10: capture.ConditionalFreshEcxPlus10,
                    EntryESI: null,
                    EntryEAX: capture.ConditionalEntryEAX);
            CapacityFinalCalleeConditionalFieldStorePlan fieldStore =
                CapacityFinalCalleeConditionalFieldStoreProjection.Project(
                    fieldStoreCapture);

            if (fieldStore.FirstBranchTaken == false
                && fieldStore.FreshEcxPlus10ReadReached
                && fieldStore.FreshEcxPlus10Captured)
            {
                fieldStore = CapacityFinalCalleeConditionalFieldStoreProjection.Project(
                    fieldStoreCapture with { EntryESI = root.Capture.EntryESI });
            }

            return JoinConditionalStore(capture, root, fieldStore);
        }

        if (IsExactRootZeroExit(root))
        {
            bool unreachedEsi = root.Capture.EntryESI.HasValue;
            bool unreachedEax = root.Capture.EntryEAX.HasValue;
            bool lateConditionalCaptures = HasConditionalCaptures(capture);
            if (unreachedEsi || unreachedEax || lateConditionalCaptures)
            {
                return RootBlocked(
                    capture,
                    root,
                    CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                        .IndexedRootMaskContaminated,
                    unreachedConditionalEntryEBXCaptureProvided:
                        capture.ConditionalEntryEBX.HasValue,
                    unreachedConditionalFreshEcxPlus10CaptureProvided:
                        capture.ConditionalFreshEcxPlus10.HasValue,
                    unreachedConditionalEntryEAXCaptureProvided:
                        capture.ConditionalEntryEAX.HasValue,
                    unreachedIndexedRootEntryESICaptureProvided: unreachedEsi,
                    unreachedIndexedRootEntryEAXCaptureProvided: unreachedEax,
                    stopAt: ZeroExitRawFileOffset);
            }

            return RootBounded(
                capture,
                root,
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskZeroExit,
                ZeroExitRawFileOffset);
        }

        if (IsExactRootNonzeroExit(root))
        {
            if (HasConditionalCaptures(capture))
            {
                return RootBlocked(
                    capture,
                    root,
                    CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                        .IndexedRootMaskContaminated,
                    unreachedConditionalEntryEBXCaptureProvided:
                        capture.ConditionalEntryEBX.HasValue,
                    unreachedConditionalFreshEcxPlus10CaptureProvided:
                        capture.ConditionalFreshEcxPlus10.HasValue,
                    unreachedConditionalEntryEAXCaptureProvided:
                        capture.ConditionalEntryEAX.HasValue,
                    stopAt: NonzeroExitRawFileOffset);
            }

            return RootBounded(
                capture,
                root,
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskNonzeroTableWriteExit,
                NonzeroExitRawFileOffset);
        }

        if (IsExactRootMaskContinuation(root))
        {
            if (HasConditionalCaptures(capture))
            {
                return RootBlocked(
                    capture,
                    root,
                    CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                        .IndexedRootMaskContaminated,
                    unreachedConditionalEntryEBXCaptureProvided:
                        capture.ConditionalEntryEBX.HasValue,
                    unreachedConditionalFreshEcxPlus10CaptureProvided:
                        capture.ConditionalFreshEcxPlus10.HasValue,
                    unreachedConditionalEntryEAXCaptureProvided:
                        capture.ConditionalEntryEAX.HasValue,
                    stopAt: MaskContinuationRawFileOffset);
            }

            return RootBounded(
                capture,
                root,
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContinuation,
                MaskContinuationRawFileOffset);
        }

        bool rootContaminated = HasUnreachedRootCapture(root)
            || HasConditionalCaptures(capture)
            || (!root.FirstIndexReadReached && root.Capture.EntryESI.HasValue)
            || (!root.TableWriteReached && root.Capture.EntryEAX.HasValue);
        return RootBlocked(
            capture,
            root,
            rootContaminated
                ? CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskContaminated
                : CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .IndexedRootMaskIncomplete,
            unreachedConditionalEntryEBXCaptureProvided:
                capture.ConditionalEntryEBX.HasValue,
            unreachedConditionalFreshEcxPlus10CaptureProvided:
                capture.ConditionalFreshEcxPlus10.HasValue,
            unreachedConditionalEntryEAXCaptureProvided:
                capture.ConditionalEntryEAX.HasValue,
            unreachedIndexedRootEntryESICaptureProvided:
                !root.FirstIndexReadReached && root.Capture.EntryESI.HasValue,
            unreachedIndexedRootEntryEAXCaptureProvided:
                !root.TableWriteReached && root.Capture.EntryEAX.HasValue);
    }

    private static CapacityFinalCalleeIndexedConditionalLifecyclePlan JoinConditionalStore(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture,
        CapacityFinalCalleeIndexedRootMaskPlan root,
        CapacityFinalCalleeConditionalFieldStorePlan fieldStore)
    {
        if (IsExactConditionalValidationBoundary(fieldStore))
        {
            return Joined(
                capture,
                root,
                fieldStore,
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalUnsignedBelowToValidation,
                admitted: true,
                ConditionalValidationDestinationRawFileOffset);
        }

        if (IsExactConditionalStoreOutcome(fieldStore))
        {
            return Joined(
                capture,
                root,
                fieldStore,
                MapConditionalStoreOutcome(fieldStore.Outcome),
                admitted: true,
                fieldStore.StopAtRawFileOffsetExclusive);
        }

        bool contaminated =
            fieldStore.UnreachedFreshEcxPlus10CaptureProvided
            || fieldStore.UnreachedEntryEAXCaptureProvided;
        return Joined(
            capture,
            root,
            fieldStore,
            contaminated
                ? CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreContaminated
                : CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalFieldStoreIncomplete,
            admitted: false,
            fieldStore.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeIndexedConditionalLifecycleOutcome
        MapConditionalStoreOutcome(
            CapacityFinalCalleeConditionalFieldStoreOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxZeroToTarget =>
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus10StoredThenEaxZeroToMaskContinuation,
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxNonzeroToBoundary =>
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus10StoredThenEaxNonzeroToBoundary,
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxZeroToTarget =>
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxZeroToMaskContinuation,
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxNonzeroToBoundary =>
                CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                    .ConditionalPlus14StoredThenEaxNonzeroToBoundary,
            _ => CapacityFinalCalleeIndexedConditionalLifecycleOutcome
                .ConditionalFieldStoreIncomplete
        };

    private static bool IsExactRootUnequalJoin(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome == CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal
        && root.Capture.CaptureValidatedEntryRegisters
        && root.Capture.EntryEBP.HasValue
        && root.Capture.EntryESI.HasValue
        && root.FreshFrameMinus14ReadReached
        && root.FreshFrameMinus14Address
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinus14Address(
                root.Capture.EntryEBP.Value)
        && root.FreshFrameMinus14Captured
        && root.FreshFrameMinus14 is not null and not 0
        && root.ZeroTestReached
        && root.ZeroTestResult == false
        && root.ZeroBranchReached
        && root.ZeroBranchTaken == false
        && root.FirstIndexReadReached
        && root.FirstIndexCaptured
        && root.FreshFirstIndex.HasValue
        && root.FirstIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI.Value)
        && root.FirstIndexedTableReadReached
        && root.FirstIndexedTableValueCaptured
        && root.FirstIndexedTableValue.HasValue
        && root.FirstIndexedTableAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshFirstIndex.Value)
        && root.FirstCompareReached
        && root.FirstCompareResult
            == unchecked(root.Capture.EntryESI.Value
                - root.FirstIndexedTableValue.Value)
        && root.FirstCompareResult != 0
        && root.FirstCompareFlags.HasValue
        && root.FirstCompareEqual == false
        && root.FirstCompareFlags.Value.Zero
            == (root.FirstCompareResult == 0)
        && root.FirstCompareFlags.Value.Zero == false
        && root.FreshFrameMinusCReadReached
        && root.FreshFrameMinusCCaptured
        && root.FreshFrameMinusCAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinusCAddress(
                root.Capture.EntryEBP.Value)
        && root.UnequalBranchReached
        && root.UnequalBranchTaken == true
        && root.UnequalBranchTaken == !root.FirstCompareFlags.Value.Zero
        && root.UnequalBranchDestinationRawFileOffset
            == CapacityFinalCalleeIndexedRootMaskProjection.UnequalExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeIndexedRootMaskProjection.UnequalExitRawFileOffset
        && !root.SecondIndexReadReached
        && !root.TableWriteReached
        && !root.StoredValueTestReached
        && !root.FreshFrameMinus8ReadReached
        && !root.ThirdIndexReadReached
        && !root.GlobalReadReached
        && !root.GlobalWriteReached;

    private static bool IsExactRootZeroExit(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome
            == CapacityFinalCalleeIndexedRootMaskOutcome.FreshFrameMinus14ZeroExit
        && root.Capture.CaptureValidatedEntryRegisters
        && root.Capture.EntryEBP.HasValue
        && root.FreshFrameMinus14ReadReached
        && root.FreshFrameMinus14Address
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinus14Address(
                root.Capture.EntryEBP.Value)
        && root.FreshFrameMinus14Captured
        && root.FreshFrameMinus14 == 0
        && root.ZeroTestReached
        && root.ZeroTestResult == true
        && root.ZeroBranchReached
        && root.ZeroBranchTaken == true
        && root.ZeroBranchDestinationRawFileOffset == ZeroExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive == ZeroExitRawFileOffset
        && !root.FirstIndexReadReached
        && !root.FirstCompareReached
        && !root.TableWriteReached
        && !root.GlobalWriteReached
        && !HasUnreachedRootCapture(root);

    private static bool IsExactRootNonzeroExit(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome
            == CapacityFinalCalleeIndexedRootMaskOutcome.StoredEntryEAXNonzeroExit
        && root.Capture.CaptureValidatedEntryRegisters
        && root.Capture.EntryEBP.HasValue
        && root.Capture.EntryESI.HasValue
        && root.Capture.EntryEAX is uint entryEax
        && entryEax != 0
        && IsCleanRootEqualityCompare(root)
        && root.SecondIndexReadReached
        && root.SecondIndexCaptured
        && root.FreshSecondIndex.HasValue
        && root.TableWriteReached
        && root.TableWriteAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshSecondIndex!.Value)
        && root.TableWriteValue == entryEax
        && root.StoredValueTestReached
        && root.StoredValueIsZero == false
        && root.NonzeroBranchReached
        && root.NonzeroBranchTaken == true
        && root.NonzeroBranchDestinationRawFileOffset == NonzeroExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive == NonzeroExitRawFileOffset
        && !root.FreshFrameMinus8ReadReached
        && !root.ThirdIndexReadReached
        && !root.GlobalReadReached
        && !HasUnreachedRootCapture(root);

    private static bool IsExactRootMaskContinuation(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome == CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated
        && root.Capture.CaptureValidatedEntryRegisters
        && root.Capture.EntryEBP.HasValue
        && root.Capture.EntryESI.HasValue
        && root.Capture.EntryEAX == 0
        && IsCleanRootEqualityCompare(root)
        && root.SecondIndexReadReached
        && root.SecondIndexCaptured
        && root.FreshSecondIndex.HasValue
        && root.TableWriteReached
        && root.TableWriteAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshSecondIndex!.Value)
        && root.TableWriteValue == 0
        && root.StoredValueTestReached
        && root.StoredValueIsZero == true
        && root.NonzeroBranchReached
        && root.NonzeroBranchTaken == false
        && root.FreshFrameMinus8ReadReached
        && root.FreshFrameMinus8Captured
        && root.FreshFrameMinus8Address
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinus8Address(
                root.Capture.EntryEBP.Value)
        && root.ThirdIndexReadReached
        && root.ThirdIndexCaptured
        && root.FreshThirdIndex.HasValue
        && root.ThirdIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI.Value)
        && root.ShiftCount
            == (root.FreshThirdIndex.Value
                & (uint)CapacityFinalCalleeIndexedRootMaskProjection.ShiftCountMask)
        && root.ShiftedValue
            == unchecked(root.FreshFrameMinus8!.Value << (int)root.ShiftCount!.Value)
        && root.NotMask == ~root.ShiftedValue
        && root.GlobalReadReached
        && root.GlobalValueCaptured
        && root.GlobalAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.GlobalMaskVirtualAddress
        && root.GlobalValueBefore.HasValue
        && root.NotMask.HasValue
        && root.GlobalValueAfter.HasValue
        && root.GlobalWriteReached
        && root.GlobalValueAfter == (root.GlobalValueBefore & root.NotMask)
        && root.StopAtRawFileOffsetExclusive == MaskContinuationRawFileOffset
        && !HasUnreachedRootCapture(root);

    private static bool IsCleanRootEqualityCompare(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.FreshFrameMinus14ReadReached
        && root.FreshFrameMinus14Captured
        && root.FreshFrameMinus14 is not null and not 0
        && root.ZeroTestReached
        && root.ZeroTestResult == false
        && root.ZeroBranchReached
        && root.ZeroBranchTaken == false
        && root.FirstIndexReadReached
        && root.FirstIndexCaptured
        && root.FreshFirstIndex.HasValue
        && root.FirstIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI!.Value)
        && root.FirstIndexedTableReadReached
        && root.FirstIndexedTableValueCaptured
        && root.FirstIndexedTableAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshFirstIndex.Value)
        && root.FirstCompareReached
        && root.FirstCompareResult
            == unchecked(root.Capture.EntryESI!.Value
                - root.FirstIndexedTableValue!.Value)
        && root.FirstCompareResult == 0
        && root.FirstCompareFlags is { Zero: true }
        && root.FirstCompareEqual == true
        && root.FreshFrameMinusCReadReached
        && root.FreshFrameMinusCCaptured
        && root.FreshFrameMinusCAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinusCAddress(
                root.Capture.EntryEBP!.Value)
        && root.UnequalBranchReached
        && root.UnequalBranchTaken == false
        && root.SecondIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI!.Value);

    private static bool IsExactConditionalValidationBoundary(
        CapacityFinalCalleeConditionalFieldStorePlan fieldStore) =>
        fieldStore.Outcome
            == CapacityFinalCalleeConditionalFieldStoreOutcome
                .EntryECXUnsignedBelowEntryEBXToValidation
        && fieldStore.EntryECXCaptured
        && fieldStore.EntryEBXCaptured
        && fieldStore.FirstCompareResult
            == unchecked(fieldStore.Capture.EntryECX!.Value
                - fieldStore.Capture.EntryEBX!.Value)
        && fieldStore.FirstCompareFlags is
        {
            Carry: true,
            Zero: false
        }
        && fieldStore.FirstCompareFlags.Value.Carry
            == (fieldStore.Capture.EntryECX!.Value
                < fieldStore.Capture.EntryEBX!.Value)
        && fieldStore.EntryECXUnsignedBelowEntryEBX == true
        && fieldStore.FirstBranchReached
        && fieldStore.FirstBranchTaken == true
        && fieldStore.FirstBranchTaken == fieldStore.FirstCompareFlags.Value.Carry
        && fieldStore.FirstBranchSelectedDestinationRawFileOffset
            == ConditionalValidationDestinationRawFileOffset
        && !fieldStore.FreshEcxPlus10ReadReached
        && !fieldStore.StoreReached
        && !fieldStore.UnreachedEntryESICaptureProvided
        && !fieldStore.UnreachedEntryEBXCaptureProvided
        && fieldStore.StopAtRawFileOffsetExclusive
            == ConditionalValidationDestinationRawFileOffset
        && !fieldStore.UnreachedFreshEcxPlus10CaptureProvided
        && !fieldStore.UnreachedEntryEAXCaptureProvided;

    private static bool IsExactConditionalStoreOutcome(
        CapacityFinalCalleeConditionalFieldStorePlan fieldStore) =>
        fieldStore.Outcome is
            CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxZeroToTarget
            or CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus10StoredThenEaxNonzeroToBoundary
            or CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxZeroToTarget
            or CapacityFinalCalleeConditionalFieldStoreOutcome
                .Plus14StoredThenEaxNonzeroToBoundary
        && fieldStore.EntryECXCaptured
        && fieldStore.EntryEBXCaptured
        && fieldStore.FirstCompareResult
            == unchecked(fieldStore.Capture.EntryECX!.Value
                - fieldStore.Capture.EntryEBX!.Value)
        && fieldStore.FirstCompareFlags is
        {
            Carry: false
        }
        && fieldStore.FirstCompareFlags.Value.Carry
            == (fieldStore.Capture.EntryECX!.Value
                < fieldStore.Capture.EntryEBX!.Value)
        && fieldStore.FirstCompareFlags.Value.Zero
            == (fieldStore.FirstCompareResult == 0)
        && fieldStore.FirstBranchReached
        && fieldStore.EntryECXUnsignedBelowEntryEBX == false
        && !fieldStore.FirstBranchTaken.GetValueOrDefault()
        && fieldStore.FirstBranchTaken == fieldStore.FirstCompareFlags.Value.Carry
        && fieldStore.FirstBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeConditionalFieldStoreProjection
                .FreshCompareDestinationRawFileOffset
        && fieldStore.FreshEcxPlus10ReadReached
        && fieldStore.FreshEcxPlus10Captured
        && fieldStore.EntryESICaptured
        && fieldStore.SecondCompareResult.HasValue
        && fieldStore.SecondCompareFlags.HasValue
        && fieldStore.SecondCompareResult
            == unchecked(fieldStore.Capture.FreshEcxPlus10!.Value
                - fieldStore.Capture.EntryESI!.Value)
        && fieldStore.SecondCompareFlags.Value.Zero
            == (fieldStore.SecondCompareResult == 0)
        && fieldStore.FreshEcxPlus10EqualEntryESI
            == fieldStore.SecondCompareFlags.Value.Zero
        && fieldStore.SecondBranchReached
        && fieldStore.SecondBranchTaken
            == (fieldStore.FreshEcxPlus10EqualEntryESI == false)
        && fieldStore.SecondBranchSelectedDestinationRawFileOffset
            == (fieldStore.FreshEcxPlus10EqualEntryESI == true
                ? CapacityFinalCalleeConditionalFieldStoreProjection
                    .Plus10StoreDestinationRawFileOffset
                : CapacityFinalCalleeConditionalFieldStoreProjection
                    .Plus14StoreDestinationRawFileOffset)
        && fieldStore.EntryEAXCaptured
        && fieldStore.StoreReached
        && fieldStore.StoreAddress.HasValue
        && fieldStore.StoreDisplacement.HasValue
        && fieldStore.StoreDisplacement
            == (fieldStore.FreshEcxPlus10EqualEntryESI == true
                ? CapacityFinalCalleeConditionalFieldStoreProjection
                    .Plus10StoreDisplacement
                : CapacityFinalCalleeConditionalFieldStoreProjection
                    .Plus14StoreDisplacement)
        && fieldStore.StoreAddress
            == CapacityFinalCalleeConditionalFieldStoreProjection.DeriveStoreAddress(
                fieldStore.Capture.EntryECX!.Value,
                fieldStore.StoreDisplacement!.Value)
        && fieldStore.StoreValue == fieldStore.Capture.EntryEAX
        && fieldStore.JumpToTestReached
            == (fieldStore.FreshEcxPlus10EqualEntryESI == true)
        && fieldStore.JumpToTestDestinationRawFileOffset
            == (fieldStore.JumpToTestReached
                ? CapacityFinalCalleeConditionalFieldStoreProjection
                    .TestEaxDestinationRawFileOffset
                : null)
        && fieldStore.TestEAXReached
        && fieldStore.TestEAXValue == fieldStore.Capture.EntryEAX
        && fieldStore.TestEAXFlags.HasValue
        && fieldStore.TestEAXFlags.Value.Zero
            == (fieldStore.Capture.EntryEAX == 0)
        && fieldStore.ZeroBranchReached
        && fieldStore.ZeroBranchTaken == fieldStore.TestEAXFlags?.Zero
        && fieldStore.ZeroBranchSelectedDestinationRawFileOffset
            == (fieldStore.ZeroBranchTaken == true
                ? ConditionalZeroDestinationRawFileOffset
                : ConditionalBoundaryRawFileOffsetExclusive)
        && fieldStore.StopAtRawFileOffsetExclusive
            == (fieldStore.ZeroBranchTaken == true
                ? ConditionalZeroDestinationRawFileOffset
                : ConditionalBoundaryRawFileOffsetExclusive)
        && !fieldStore.UnreachedEntryEBXCaptureProvided
        && !fieldStore.UnreachedFreshEcxPlus10CaptureProvided
        && !fieldStore.UnreachedEntryESICaptureProvided
        && !fieldStore.UnreachedEntryEAXCaptureProvided;

    private static bool HasUnreachedRootCapture(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.UnreachedEntryRegisterCaptureProvided
        || root.UnreachedFreshFrameMinus14CaptureProvided
        || root.UnreachedFreshFirstIndexCaptureProvided
        || root.UnreachedFirstIndexedTableValueCaptureProvided
        || root.UnreachedFreshFrameMinusCCaptureProvided
        || root.UnreachedFreshSecondIndexCaptureProvided
        || root.UnreachedFreshFrameMinus8CaptureProvided
        || root.UnreachedFreshThirdIndexCaptureProvided
        || root.UnreachedFreshGlobalValueCaptureProvided;

    private static bool HasConditionalCaptures(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture) =>
        capture.ConditionalEntryEBX.HasValue
        || capture.ConditionalFreshEcxPlus10.HasValue
        || capture.ConditionalEntryEAX.HasValue;

    private static CapacityFinalCalleeIndexedConditionalLifecyclePlan RootBounded(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture,
        CapacityFinalCalleeIndexedRootMaskPlan root,
        CapacityFinalCalleeIndexedConditionalLifecycleOutcome outcome,
        uint stopAt) =>
        RootBlocked(
            capture,
            root,
            outcome,
            admitted: true,
            stopAt: stopAt);

    private static CapacityFinalCalleeIndexedConditionalLifecyclePlan RootBlocked(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture,
        CapacityFinalCalleeIndexedRootMaskPlan root,
        CapacityFinalCalleeIndexedConditionalLifecycleOutcome outcome,
        bool admitted = false,
        bool? unreachedConditionalEntryEBXCaptureProvided = null,
        bool? unreachedConditionalFreshEcxPlus10CaptureProvided = null,
        bool? unreachedConditionalEntryEAXCaptureProvided = null,
        bool? unreachedIndexedRootEntryESICaptureProvided = null,
        bool? unreachedIndexedRootEntryEAXCaptureProvided = null,
        uint? stopAt = null) =>
        new(
            capture,
            root,
            ConditionalFieldStore: null,
            DerivedConditionalEntryECX: null,
            PreservedConditionalEntryESI: null,
            Outcome: outcome,
            ConditionalFieldStoreReached: false,
            UnreachedConditionalEntryEBXCaptureProvided:
                unreachedConditionalEntryEBXCaptureProvided
                    ?? capture.ConditionalEntryEBX.HasValue,
            UnreachedConditionalFreshEcxPlus10CaptureProvided:
                unreachedConditionalFreshEcxPlus10CaptureProvided
                    ?? capture.ConditionalFreshEcxPlus10.HasValue,
            UnreachedConditionalEntryEAXCaptureProvided:
                unreachedConditionalEntryEAXCaptureProvided
                    ?? capture.ConditionalEntryEAX.HasValue,
            UnreachedIndexedRootEntryESICaptureProvided:
                unreachedIndexedRootEntryESICaptureProvided
                    ?? (!root.FirstIndexReadReached && root.Capture.EntryESI.HasValue),
            UnreachedIndexedRootEntryEAXCaptureProvided:
                unreachedIndexedRootEntryEAXCaptureProvided
                    ?? (!root.TableWriteReached && root.Capture.EntryEAX.HasValue),
            Admitted: admitted,
            StopAtRawFileOffsetExclusive:
                stopAt ?? root.StopAtRawFileOffsetExclusive);

    private static CapacityFinalCalleeIndexedConditionalLifecyclePlan Joined(
        CapacityFinalCalleeIndexedConditionalLifecycleCapture capture,
        CapacityFinalCalleeIndexedRootMaskPlan root,
        CapacityFinalCalleeConditionalFieldStorePlan fieldStore,
        CapacityFinalCalleeIndexedConditionalLifecycleOutcome outcome,
        bool admitted,
        uint stopAt) =>
        new(
            capture,
            root,
            fieldStore,
            DerivedConditionalEntryECX: root.FreshFrameMinus14,
            PreservedConditionalEntryESI: fieldStore.EntryESICaptured
                ? root.Capture.EntryESI
                : null,
            Outcome: outcome,
            ConditionalFieldStoreReached: true,
            UnreachedConditionalEntryEBXCaptureProvided: false,
            UnreachedConditionalFreshEcxPlus10CaptureProvided:
                fieldStore.UnreachedFreshEcxPlus10CaptureProvided,
            UnreachedConditionalEntryEAXCaptureProvided:
                fieldStore.UnreachedEntryEAXCaptureProvided,
            UnreachedIndexedRootEntryESICaptureProvided: false,
            UnreachedIndexedRootEntryEAXCaptureProvided: false,
            Admitted: admitted,
            StopAtRawFileOffsetExclusive: stopAt);
}
