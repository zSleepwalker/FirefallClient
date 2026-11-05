namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeClearIndexedLifecycleCapture(
    CapacityFinalCalleeFieldClearTailCapture ClearTailCapture,
    uint? IndexedEntryESI,
    uint? IndexedEntryEAX,
    uint? FreshFrameMinus14,
    uint? FreshFirstIndex,
    uint? FirstIndexedTableValue,
    uint? FreshFrameMinusC,
    uint? FreshSecondIndex,
    uint? FreshFrameMinus8,
    uint? FreshThirdIndex,
    uint? FreshIndexedGlobalValue);

public enum CapacityFinalCalleeClearIndexedLifecycleOutcome
{
    ClearTailIncomplete,
    ClearTailContaminated,
    ClearTailBelowDestination,
    IndexedRootMaskIncomplete,
    IndexedRootMaskContaminated,
    IndexedRootMaskZeroExit,
    IndexedRootMaskUnequalExit,
    IndexedRootMaskNonzeroTableWriteExit,
    IndexedRootMaskContinuation
}

public readonly record struct CapacityFinalCalleeClearIndexedLifecyclePlan(
    CapacityFinalCalleeClearIndexedLifecycleCapture Capture,
    CapacityFinalCalleeFieldClearTailPlan ClearTailPlan,
    CapacityFinalCalleeIndexedRootMaskPlan? IndexedRootMask,
    CapacityFinalCalleeClearIndexedLifecycleOutcome Outcome,
    bool IndexedRootMaskReached,
    bool UnreachedIndexedRootEntryESICaptureProvided,
    bool UnreachedIndexedRootEntryEAXCaptureProvided,
    bool UnreachedIndexedRootMaskCaptureProvided,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the field-clear tail's exact FFE load to the indexed root-mask
/// window. Entry EBP is carried through the clear-tail capture; ESI and EAX
/// remain separate caller live-ins and are only consumed on their native
/// branches.
/// </summary>
public static class CapacityFinalCalleeClearIndexedLifecycleProjection
{
    public const uint ClearTailEntryRawFileOffset =
        CapacityFinalCalleeFieldClearTailProjection.EntryRawFileOffset;
    public const uint IndexedRootMaskEntryRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.EntryRawFileOffset;
    public const uint IndexedRootMaskBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeIndexedRootMaskProjection.WindowBoundaryRawFileOffsetExclusive;
    public const uint ClearTailBelowDestinationRawFileOffset =
        CapacityFinalCalleeFieldClearTailProjection.BelowDestinationRawFileOffset;
    public const uint IndexedZeroExitRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.ZeroExitRawFileOffset;
    public const uint IndexedUnequalExitRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.UnequalExitRawFileOffset;
    public const uint IndexedNonzeroExitRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.NonzeroExitRawFileOffset;
    public const uint IndexedMaskContinuationRawFileOffset =
        CapacityFinalCalleeIndexedRootMaskProjection.MaskContinuationRawFileOffset;

    public static CapacityFinalCalleeClearIndexedLifecyclePlan Project(
        CapacityFinalCalleeClearIndexedLifecycleCapture capture)
    {
        CapacityFinalCalleeFieldClearTailPlan clearTail =
            CapacityFinalCalleeFieldClearTailProjection.Project(
                capture.ClearTailCapture);
        bool indexedCapturesProvided = HasIndexedCaptures(capture);

        if (IsCleanBelowDestination(clearTail))
        {
            bool contaminated = indexedCapturesProvided;
            return BuildPlan(
                capture,
                clearTail,
                indexedRootMask: null,
                contaminated
                    ? CapacityFinalCalleeClearIndexedLifecycleOutcome
                        .ClearTailContaminated
                    : CapacityFinalCalleeClearIndexedLifecycleOutcome
                        .ClearTailBelowDestination,
                indexedRootMaskReached: false,
                unreachedIndexedRootEntryESICaptureProvided:
                    capture.IndexedEntryESI.HasValue,
                unreachedIndexedRootEntryEAXCaptureProvided:
                    capture.IndexedEntryEAX.HasValue,
                unreachedIndexedRootMaskCaptureProvided: indexedCapturesProvided,
                admitted: !contaminated,
                stopAt: ClearTailBelowDestinationRawFileOffset);
        }

        if (!IsCleanTailCompletion(clearTail))
        {
            bool contaminated =
                HasUnreachedClearTailCapture(clearTail) || indexedCapturesProvided;
            return BuildPlan(
                capture,
                clearTail,
                indexedRootMask: null,
                contaminated
                    ? CapacityFinalCalleeClearIndexedLifecycleOutcome
                        .ClearTailContaminated
                    : CapacityFinalCalleeClearIndexedLifecycleOutcome
                        .ClearTailIncomplete,
                indexedRootMaskReached: false,
                unreachedIndexedRootEntryESICaptureProvided:
                    capture.IndexedEntryESI.HasValue,
                unreachedIndexedRootEntryEAXCaptureProvided:
                    capture.IndexedEntryEAX.HasValue,
                unreachedIndexedRootMaskCaptureProvided: indexedCapturesProvided,
                admitted: false,
                stopAt: clearTail.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeIndexedRootMaskPlan rootMask =
            CapacityFinalCalleeIndexedRootMaskProjection.Project(new(
                CaptureValidatedEntryRegisters: true,
                EntryEBP: capture.ClearTailCapture.ValidatedEntryEBP,
                EntryESI: capture.IndexedEntryESI,
                EntryEAX: capture.IndexedEntryEAX,
                FreshFrameMinus14: capture.FreshFrameMinus14,
                FreshFirstIndex: capture.FreshFirstIndex,
                FirstIndexedTableValue: capture.FirstIndexedTableValue,
                FreshFrameMinusC: capture.FreshFrameMinusC,
                FreshSecondIndex: capture.FreshSecondIndex,
                FreshFrameMinus8: capture.FreshFrameMinus8,
                FreshThirdIndex: capture.FreshThirdIndex,
                FreshGlobalValue: capture.FreshIndexedGlobalValue));

        bool unreachedEsi =
            capture.IndexedEntryESI.HasValue && !rootMask.FirstIndexReadReached;
        bool unreachedEax =
            capture.IndexedEntryEAX.HasValue && !rootMask.SecondIndexCaptured;
        bool unreachedRootCapture =
            unreachedEsi || unreachedEax || HasUnreachedRootCapture(rootMask);

        if (IsExactZeroExit(rootMask)
            && !unreachedEsi
            && !unreachedEax)
        {
            return BuildPlan(
                capture,
                clearTail,
                rootMask,
                CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskZeroExit,
                indexedRootMaskReached: true,
                admitted: true,
                stopAt: IndexedZeroExitRawFileOffset);
        }

        if (IsExactUnequalExit(rootMask)
            && !unreachedEsi
            && !unreachedEax)
        {
            return BuildPlan(
                capture,
                clearTail,
                rootMask,
                CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskUnequalExit,
                indexedRootMaskReached: true,
                admitted: true,
                stopAt: IndexedUnequalExitRawFileOffset);
        }

        if (IsExactNonzeroExit(rootMask)
            && !unreachedEsi
            && !unreachedEax)
        {
            return BuildPlan(
                capture,
                clearTail,
                rootMask,
                CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskNonzeroTableWriteExit,
                indexedRootMaskReached: true,
                admitted: true,
                stopAt: IndexedNonzeroExitRawFileOffset);
        }

        if (IsExactMaskContinuation(rootMask)
            && !unreachedEsi
            && !unreachedEax)
        {
            return BuildPlan(
                capture,
                clearTail,
                rootMask,
                CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContinuation,
                indexedRootMaskReached: true,
                admitted: true,
                stopAt: IndexedMaskContinuationRawFileOffset);
        }

        return BuildPlan(
            capture,
            clearTail,
            rootMask,
            unreachedRootCapture
                ? CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskContaminated
                : CapacityFinalCalleeClearIndexedLifecycleOutcome
                    .IndexedRootMaskIncomplete,
            indexedRootMaskReached: true,
            unreachedIndexedRootEntryESICaptureProvided: unreachedEsi,
            unreachedIndexedRootEntryEAXCaptureProvided: unreachedEax,
            unreachedIndexedRootMaskCaptureProvided: unreachedRootCapture,
            admitted: false,
            stopAt: rootMask.StopAtRawFileOffsetExclusive);
    }

    private static bool IsCleanBelowDestination(
        CapacityFinalCalleeFieldClearTailPlan clearTail) =>
        clearTail.Outcome
            == CapacityFinalCalleeFieldClearTailOutcome.EntryEDXUnsignedBelowGlobal
        && clearTail.Capture.ValidatedEntryEDX is uint entryEdx
        && clearTail.CompareGlobalValue is uint compareGlobal
        && clearTail.CompareResult == unchecked(entryEdx - compareGlobal)
        && clearTail.CompareFlags is { } compareFlags
        && compareFlags.Carry == (entryEdx < compareGlobal)
        && compareFlags.Zero == (clearTail.CompareResult == 0)
        && clearTail.StopAtRawFileOffsetExclusive
            == ClearTailBelowDestinationRawFileOffset
        && clearTail.BranchReached
        && clearTail.BranchTaken == true
        && clearTail.SelectedBranchDestinationRawFileOffset
            == ClearTailBelowDestinationRawFileOffset
        && clearTail.CompareFlags is { Carry: true }
        && clearTail.FrameMinus12ReadReached
        && clearTail.FrameMinus12Captured
        && !HasUnreachedClearTailCapture(clearTail);

    private static bool IsCleanTailCompletion(
        CapacityFinalCalleeFieldClearTailPlan clearTail) =>
        clearTail.Outcome == CapacityFinalCalleeFieldClearTailOutcome.TailGlobalLoaded
        && clearTail.Capture.ValidatedEntryEDX is uint entryEdx
        && clearTail.Capture.ValidatedEntryEBP is uint entryEbp
        && clearTail.CompareGlobalValue is uint compareGlobal
        && clearTail.CompareResult == unchecked(entryEdx - compareGlobal)
        && clearTail.CompareFlags is { } compareFlags
        && compareFlags.Carry == (entryEdx < compareGlobal)
        && compareFlags.Zero == (clearTail.CompareResult == 0)
        && clearTail.StopAtRawFileOffsetExclusive
            == IndexedRootMaskEntryRawFileOffset
        && clearTail.CompareReached
        && clearTail.CompareFlags is { Carry: false }
        && clearTail.BranchReached
        && clearTail.BranchTaken == false
        && clearTail.SelectedBranchDestinationRawFileOffset
            == CapacityFinalCalleeFieldClearTailProjection
                .TailFrameMinus4LoadRawFileOffset
        && clearTail.FrameMinus12ReadReached
        && clearTail.FrameMinus12Captured
        && clearTail.FrameMinus4ReadReached
        && clearTail.FrameMinus4Captured
        && clearTail.TailGlobalReadReached
        && clearTail.TailGlobalCaptured
        && clearTail.Writes.Length == 2
        && clearTail.Writes[0].RawFileOffset
            == CapacityFinalCalleeFieldClearTailProjection.EntryFrameStoreRawFileOffset
        && clearTail.Writes[0].Address
            == CapacityFinalCalleeFieldClearTailProjection.DeriveFrameAddress(
                entryEbp,
                CapacityFinalCalleeFieldClearTailProjection.FrameMinus4Displacement)
        && clearTail.Writes[0].Value == entryEdx
        && clearTail.Writes[1].RawFileOffset
            == CapacityFinalCalleeFieldClearTailProjection.ZeroStoreRawFileOffset
        && clearTail.Writes[1].Address
            == clearTail.Capture.FreshFrameMinus4ForZeroStoreAddress
        && clearTail.Writes[1].Value == 0
        && !HasUnreachedClearTailCapture(clearTail);

    private static bool IsExactZeroExit(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome
            == CapacityFinalCalleeIndexedRootMaskOutcome.FreshFrameMinus14ZeroExit
        && root.Capture.EntryEBP.HasValue
        && root.FreshFrameMinus14Address
            == CapacityFinalCalleeIndexedRootMaskProjection
                .DeriveFrameMinus14Address(root.Capture.EntryEBP.Value)
        && root.FreshFrameMinus14ReadReached
        && root.FreshFrameMinus14Captured
        && root.FreshFrameMinus14 == 0
        && root.ZeroTestReached
        && root.ZeroTestResult == true
        && root.ZeroBranchReached
        && root.ZeroBranchTaken == true
        && root.ZeroBranchDestinationRawFileOffset == IndexedZeroExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive == IndexedZeroExitRawFileOffset
        && !root.FirstIndexReadReached
        && !root.FirstCompareReached
        && !root.TableWriteReached
        && !root.GlobalWriteReached
        && !HasUnreachedRootCapture(root);

    private static bool IsExactUnequalExit(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome
            == CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal
        && IsCleanRootCompare(root, equal: false)
        && root.UnequalBranchReached
        && root.UnequalBranchTaken == true
        && root.UnequalBranchDestinationRawFileOffset
            == IndexedUnequalExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive == IndexedUnequalExitRawFileOffset
        && !root.SecondIndexReadReached
        && !root.TableWriteReached
        && !root.GlobalWriteReached
        && !HasUnreachedRootCapture(root);

    private static bool IsExactNonzeroExit(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome
            == CapacityFinalCalleeIndexedRootMaskOutcome.StoredEntryEAXNonzeroExit
        && root.Capture.EntryEAX is uint eax and not 0
        && IsCleanRootCompare(root, equal: true)
        && root.SecondIndexReadReached
        && root.SecondIndexCaptured
        && root.SecondIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI!.Value)
        && root.TableWriteReached
        && root.TableWriteAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshSecondIndex!.Value)
        && root.TableWriteValue == eax
        && root.StoredValueTestReached
        && root.StoredValueIsZero == false
        && root.NonzeroBranchReached
        && root.NonzeroBranchTaken == true
        && root.NonzeroBranchDestinationRawFileOffset
            == IndexedNonzeroExitRawFileOffset
        && root.StopAtRawFileOffsetExclusive == IndexedNonzeroExitRawFileOffset
        && !root.FreshFrameMinus8ReadReached
        && !root.GlobalReadReached
        && !HasUnreachedRootCapture(root);

    private static bool IsExactMaskContinuation(
        CapacityFinalCalleeIndexedRootMaskPlan root) =>
        root.Outcome == CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated
        && root.Capture.EntryEAX == 0
        && IsCleanRootCompare(root, equal: true)
        && root.SecondIndexReadReached
        && root.SecondIndexCaptured
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
                root.Capture.EntryEBP!.Value)
        && root.ThirdIndexReadReached
        && root.ThirdIndexCaptured
        && root.ThirdIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI!.Value)
        && root.GlobalReadReached
        && root.GlobalValueCaptured
        && root.GlobalAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.GlobalMaskVirtualAddress
        && root.GlobalWriteReached
        && root.GlobalValueAfter == (root.GlobalValueBefore & root.NotMask)
        && root.StopAtRawFileOffsetExclusive == IndexedMaskContinuationRawFileOffset
        && !HasUnreachedRootCapture(root);

    private static bool IsCleanRootCompare(
        CapacityFinalCalleeIndexedRootMaskPlan root,
        bool equal) =>
        root.Capture.CaptureValidatedEntryRegisters
        && root.Capture.EntryEBP.HasValue
        && root.Capture.EntryESI.HasValue
        && root.FreshFrameMinus14ReadReached
        && root.FreshFrameMinus14Captured
        && root.FreshFrameMinus14 is not null and not 0
        && root.ZeroTestReached
        && root.ZeroTestResult == false
        && root.ZeroBranchReached
        && root.ZeroBranchTaken == false
        && root.FirstIndexReadReached
        && root.FirstIndexCaptured
        && root.FirstIndexAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveEsiIndexAddress(
                root.Capture.EntryESI.Value)
        && root.FirstIndexedTableReadReached
        && root.FirstIndexedTableValueCaptured
        && root.FirstIndexedTableAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveIndexedTableAddress(
                root.FreshFirstIndex!.Value)
        && root.FirstCompareReached
        && root.FirstIndexedTableValue is uint firstTableValue
        && root.FirstCompareResult is uint compareResult
        && compareResult
            == unchecked(root.Capture.EntryESI.Value - firstTableValue)
        && root.FirstCompareFlags.HasValue
        && root.FirstCompareFlags.Value.Zero == (compareResult == 0)
        && root.FirstCompareFlags.Value.Zero == equal
        && root.FirstCompareEqual == equal
        && root.FreshFrameMinusCReadReached
        && root.FreshFrameMinusCCaptured
        && root.FreshFrameMinusCAddress
            == CapacityFinalCalleeIndexedRootMaskProjection.DeriveFrameMinusCAddress(
                root.Capture.EntryEBP.Value)
        && root.UnequalBranchReached
        && root.UnequalBranchTaken == !equal
        && root.UnequalBranchDestinationRawFileOffset
            == (equal ? null : IndexedUnequalExitRawFileOffset);

    private static bool HasUnreachedClearTailCapture(
        CapacityFinalCalleeFieldClearTailPlan clearTail) =>
        clearTail.UnreachedValidatedEntryEBPCaptureProvided
        || clearTail.UnreachedFreshGlobalForEntryCompareCaptureProvided
        || clearTail.UnreachedFreshFrameMinus12CaptureProvided
        || clearTail.UnreachedFreshFrameMinus4CaptureProvided
        || clearTail.UnreachedFreshGlobalForTailLoadCaptureProvided;

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

    private static bool HasIndexedCaptures(
        CapacityFinalCalleeClearIndexedLifecycleCapture capture) =>
        capture.IndexedEntryESI.HasValue
        || capture.IndexedEntryEAX.HasValue
        || capture.FreshFrameMinus14.HasValue
        || capture.FreshFirstIndex.HasValue
        || capture.FirstIndexedTableValue.HasValue
        || capture.FreshFrameMinusC.HasValue
        || capture.FreshSecondIndex.HasValue
        || capture.FreshFrameMinus8.HasValue
        || capture.FreshThirdIndex.HasValue
        || capture.FreshIndexedGlobalValue.HasValue;

    private static CapacityFinalCalleeClearIndexedLifecyclePlan BuildPlan(
        CapacityFinalCalleeClearIndexedLifecycleCapture capture,
        CapacityFinalCalleeFieldClearTailPlan clearTail,
        CapacityFinalCalleeIndexedRootMaskPlan? indexedRootMask,
        CapacityFinalCalleeClearIndexedLifecycleOutcome outcome,
        bool indexedRootMaskReached,
        bool unreachedIndexedRootEntryESICaptureProvided = false,
        bool unreachedIndexedRootEntryEAXCaptureProvided = false,
        bool unreachedIndexedRootMaskCaptureProvided = false,
        bool admitted = false,
        uint? stopAt = null) =>
        new(
            capture,
            clearTail,
            indexedRootMask,
            outcome,
            indexedRootMaskReached,
            unreachedIndexedRootEntryESICaptureProvided,
            unreachedIndexedRootEntryEAXCaptureProvided,
            unreachedIndexedRootMaskCaptureProvided,
            admitted,
            stopAt ?? indexedRootMask?.StopAtRawFileOffsetExclusive
                ?? clearTail.StopAtRawFileOffsetExclusive);
}
