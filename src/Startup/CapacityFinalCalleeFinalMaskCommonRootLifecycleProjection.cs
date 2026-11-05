using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFinalMaskCommonRootLifecycleCapture(
    CapacityFinalCalleeFinalMaskCommonLifecycleCapture FinalMaskCommonCapture,
    uint? FreshGlobalDWORD);

public enum CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
{
    FinalMaskCommonIncomplete,
    FinalMaskCommonContaminatedAtBoundary,
    FinalMaskCommonCleanUnequalTo917C,
    CommonTailMissingEntryEDI,
    CommonTailMissingEntryESI,
    FinalMaskCommonHandoffRejected,
    RootGateMissingFreshGlobalDWORD,
    RootGateEqualToFallthrough92D0,
    RootGateUnequalToBranch92EC
}

public enum CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind
{
    MaskGlobal,
    CommonTailFirstStore,
    CommonTailSecondStore
}

public readonly record struct CapacityFinalCalleeFinalMaskCommonRootLifecycleWrite(
    CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan(
    CapacityFinalCalleeFinalMaskCommonRootLifecycleCapture Capture,
    CapacityFinalCalleeFinalMaskCommonLifecyclePlan FinalMaskCommon,
    CapacityFinalCalleeCommonTailRootGatePlan? CommonTailRootGate,
    CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome Outcome)
{
    public bool RootGateReached => CommonTailRootGate is not null;
    public bool ChildEntryESIRetained => RootGateReached;
    public uint? ChildEntryESI => CommonTailRootGate?.Capture.EntryESI;
    public bool FreshGlobalReadReached =>
        CommonTailRootGate?.FreshGlobalReadReached == true;
    public uint? FreshGlobalAddress => CommonTailRootGate?.FreshGlobalAddress;
    public bool FreshGlobalCaptured =>
        CommonTailRootGate?.FreshGlobalCaptured == true;
    public uint? FreshGlobalDWORD => CommonTailRootGate?.FreshGlobalDWORD;
    public bool UnreachedFreshGlobalCaptureProvided =>
        !RootGateReached && Capture.FreshGlobalDWORD.HasValue;
    public bool UnreachedCallerEntryEDICaptureProvided =>
        FinalMaskCommon.UnreachedCallerEntryEDICaptureProvided;
    public bool UnreachedCallerEntryESICaptureProvided =>
        FinalMaskCommon.UnreachedCallerEntryESICaptureProvided;
    public ImmutableArray<CapacityFinalCalleeFinalMaskCommonRootLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeFinalMaskCommonRootLifecycleWrite>.Empty;
    public CapacityFinalCalleeCommonTailRootGateFlags? CompareFlags =>
        CommonTailRootGate?.CompareFlags;
    public CapacityFinalCalleeAlternateMaskTailFlags? FlagsAtStop { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean 92C8 final-mask/common-tail completion to its root gate.
/// The retained caller ESI and the fresh global DWORD are independent inputs;
/// neither value is derived from the preceding writes.
/// </summary>
public static class CapacityFinalCalleeFinalMaskCommonRootLifecycleProjection
{
    public const uint CommonTailRootGateEntryRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.EntryRawFileOffset;
    public const uint FallthroughRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.FallthroughRawFileOffset;
    public const uint BranchTargetRawFileOffset =
        CapacityFinalCalleeCommonTailRootGateProjection.BranchTargetRawFileOffset;
    public const uint FreshGlobalDWORDVirtualAddress =
        CapacityFinalCalleeCommonTailRootGateProjection.FreshGlobalDWORDVirtualAddress;
    public const uint CleanUnequalStopRawFileOffsetExclusive =
        CapacityFinalCalleeFinalMaskLifecycleProjection
            .UpstreamUnequalDestinationRawFileOffset;

    public static CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan Project(
        CapacityFinalCalleeFinalMaskCommonRootLifecycleCapture capture)
    {
        CapacityFinalCalleeFinalMaskCommonLifecyclePlan finalMaskCommon =
            CapacityFinalCalleeFinalMaskCommonLifecycleProjection.Project(
                capture.FinalMaskCommonCapture);
        var writes =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeFinalMaskCommonRootLifecycleWrite>();

        foreach (CapacityFinalCalleeFinalMaskCommonLifecycleWrite write
            in finalMaskCommon.OrderedWrites)
        {
            writes.Add(new(
                write.Kind switch
                {
                    CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal =>
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind.MaskGlobal,
                    CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailFirstStore =>
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind
                            .CommonTailFirstStore,
                    CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailSecondStore =>
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind
                            .CommonTailSecondStore,
                    _ => throw new InvalidOperationException(
                        "The final-mask/common lifecycle returned an unknown write kind.")
                },
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        if (IsCleanUnequalAt917C(finalMaskCommon))
        {
            return new(
                capture,
                finalMaskCommon,
                null,
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonCleanUnequalTo917C)
            {
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = finalMaskCommon.FlagsAtStop,
                ExactCleanAdmission = !capture.FreshGlobalDWORD.HasValue,
                StopAtRawFileOffsetExclusive =
                    CleanUnequalStopRawFileOffsetExclusive
            };
        }

        if (finalMaskCommon.Outcome
            != CapacityFinalCalleeFinalMaskCommonLifecycleOutcome.Completed)
        {
            return new(
                capture,
                finalMaskCommon,
                null,
                MapUnreachedOutcome(finalMaskCommon.Outcome))
            {
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = finalMaskCommon.FlagsAtStop,
                StopAtRawFileOffsetExclusive =
                    finalMaskCommon.StopAtRawFileOffsetExclusive
            };
        }

        if (!IsExactCleanCommonTailCompletion(finalMaskCommon, out uint retainedEntryEsi))
        {
            return new(
                capture,
                finalMaskCommon,
                null,
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonHandoffRejected)
            {
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = finalMaskCommon.FlagsAtStop,
                StopAtRawFileOffsetExclusive =
                    finalMaskCommon.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailRootGatePlan rootGate =
            CapacityFinalCalleeCommonTailRootGateProjection.Project(new(
                EntryESI: retainedEntryEsi,
                FreshGlobalDWORD: capture.FreshGlobalDWORD));
        CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome outcome =
            rootGate.Outcome switch
            {
                CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD =>
                    CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                        .RootGateMissingFreshGlobalDWORD,
                CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough =>
                    CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                        .RootGateEqualToFallthrough92D0,
                CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget =>
                    CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                        .RootGateUnequalToBranch92EC,
                _ => throw new InvalidOperationException(
                    "The common-tail root gate returned an unexpected outcome.")
            };
        bool rootGateCompleted = rootGate.ExactCleanAdmission
            && rootGate.CompareReached
            && rootGate.JnzReached
            && rootGate.Writes.IsEmpty
            && rootGate.JnzSelectedDestinationRawFileOffset is
                FallthroughRawFileOffset or BranchTargetRawFileOffset
            && rootGate.StopAtRawFileOffsetExclusive
                == rootGate.JnzSelectedDestinationRawFileOffset;

        return new(capture, finalMaskCommon, rootGate, outcome)
        {
            OrderedWrites = writes.ToImmutable(),
            FlagsAtStop = GetFlagsAtStop(rootGate, finalMaskCommon),
            ExactCleanAdmission =
                finalMaskCommon.ExactCleanExitAdmitted && rootGateCompleted,
            StopAtRawFileOffsetExclusive =
                rootGate.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
        MapUnreachedOutcome(
            CapacityFinalCalleeFinalMaskCommonLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeFinalMaskCommonLifecycleOutcome.FinalMaskIncomplete =>
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonIncomplete,
            CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                .FinalMaskContaminatedAtBoundary =>
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonContaminatedAtBoundary,
            CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                .CommonTailMissingEntryEDI =>
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .CommonTailMissingEntryEDI,
            CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                .CommonTailMissingEntryESI =>
                CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .CommonTailMissingEntryESI,
            _ => CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                .FinalMaskCommonHandoffRejected
        };

    private static bool IsCleanUnequalAt917C(
        CapacityFinalCalleeFinalMaskCommonLifecyclePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                .FinalMaskCleanUnequalTo917C
        && plan.FinalMask.ExactCleanExitAdmitted
        && plan.ExactCleanExitAdmitted
        && !plan.CommonTailReached
        && plan.CommonTail is null
        && plan.OrderedWrites.IsEmpty
        && plan.StopAtRawFileOffsetExclusive
            == CleanUnequalStopRawFileOffsetExclusive;

    private static bool IsExactCleanCommonTailCompletion(
        CapacityFinalCalleeFinalMaskCommonLifecyclePlan plan,
        out uint retainedEntryEsi)
    {
        retainedEntryEsi = 0;
        if (!plan.ExactCleanHandoffToCommonTail
            || !plan.CommonTailReached
            || !plan.ExactCleanExitAdmitted
            || plan.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleeCommonTailTagWritesProjection
                    .BoundaryRawFileOffsetExclusive
            || plan.CommonTail is not
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeCommonTailTagWritesProjection
                        .BoundaryRawFileOffsetExclusive,
                Writes.Length: 2,
                Capture.EntryESI: uint entryEsi
            }
            || plan.OrderedWrites.Length != 3
            || plan.OrderedWrites[0].Kind
                != CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal
            || plan.OrderedWrites[1].Kind
                != CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailFirstStore
            || plan.OrderedWrites[2].Kind
                != CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailSecondStore)
        {
            return false;
        }

        retainedEntryEsi = entryEsi;
        return true;
    }

    private static CapacityFinalCalleeAlternateMaskTailFlags? GetFlagsAtStop(
        CapacityFinalCalleeCommonTailRootGatePlan rootGate,
        CapacityFinalCalleeFinalMaskCommonLifecyclePlan finalMaskCommon)
    {
        if (rootGate.CompareFlags is CapacityFinalCalleeCommonTailRootGateFlags flags)
        {
            return new(
                flags.Carry,
                flags.Parity,
                flags.Auxiliary,
                flags.Zero,
                flags.Sign,
                flags.Overflow);
        }

        return finalMaskCommon.FlagsAtStop;
    }
}
