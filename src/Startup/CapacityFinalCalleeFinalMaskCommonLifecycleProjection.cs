using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFinalMaskCommonLifecycleCapture(
    CapacityFinalCalleeFinalMaskLifecycleCapture FinalMaskCapture,
    uint? CallerEntryEDI,
    uint? CallerEntryESI);

public enum CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
{
    FinalMaskIncomplete,
    FinalMaskContaminatedAtBoundary,
    FinalMaskCleanUnequalTo917C,
    CommonTailMissingEntryEDI,
    CommonTailMissingEntryESI,
    Completed
}

public enum CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind
{
    MaskGlobal,
    CommonTailFirstStore,
    CommonTailSecondStore
}

public readonly record struct CapacityFinalCalleeFinalMaskCommonLifecycleWrite(
    CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeFinalMaskCommonLifecyclePlan(
    CapacityFinalCalleeFinalMaskCommonLifecycleCapture Capture,
    CapacityFinalCalleeFinalMaskLifecyclePlan FinalMask,
    CapacityFinalCalleeCommonTailTagWritesPlan? CommonTail,
    CapacityFinalCalleeFinalMaskCommonLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToCommonTail { get; init; }
    public bool CommonTailReached { get; init; }
    public bool CallerEntryEDICaptured { get; init; }
    public bool CallerEntryESICaptured { get; init; }
    public bool UnreachedCallerEntryEDICaptureProvided { get; init; }
    public bool UnreachedCallerEntryESICaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeFinalMaskCommonLifecycleWrite> OrderedWrites
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeFinalMaskCommonLifecycleWrite>.Empty;
    public CapacityFinalCalleeAlternateMaskTailFlags? FlagsAtStop { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean final-mask handoff at 92BD to the bounded common tail.
/// The caller's EDI and ESI are independent captures consumed only by their
/// respective instructions in the common tail.
/// </summary>
public static class CapacityFinalCalleeFinalMaskCommonLifecycleProjection
{
    public const uint CommonTailEntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint CompletedBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint MaskGlobalWriteRawFileOffset =
        CapacityFinalCalleeAlternateMaskTailProjection.GlobalAndRawFileOffset;
    public const uint MaskGlobalVirtualAddress =
        CapacityFinalCalleeAlternateMaskTailProjection.GlobalVirtualAddress;
    public const uint UpstreamUnequalDestinationRawFileOffset =
        CapacityFinalCalleeFinalMaskLifecycleProjection
            .UpstreamUnequalDestinationRawFileOffset;

    public static CapacityFinalCalleeFinalMaskCommonLifecyclePlan Project(
        CapacityFinalCalleeFinalMaskCommonLifecycleCapture capture)
    {
        CapacityFinalCalleeFinalMaskLifecyclePlan finalMask =
            CapacityFinalCalleeFinalMaskLifecycleProjection.Project(
                capture.FinalMaskCapture);
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeFinalMaskCommonLifecycleWrite>();

        if (finalMask.MaskTail is { GlobalWriteReached: true,
                GlobalWriteAddress: uint globalAddress,
                GlobalWriteValue: uint globalValue })
        {
            writes.Add(new(
                CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal,
                MaskGlobalWriteRawFileOffset,
                globalAddress,
                globalValue));
        }

        if (IsExactCleanUnequalAt917C(finalMask, capture))
        {
            return new(
                capture,
                finalMask,
                null,
                CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                    .FinalMaskCleanUnequalTo917C)
            {
                UnreachedCallerEntryEDICaptureProvided =
                    capture.CallerEntryEDI.HasValue,
                UnreachedCallerEntryESICaptureProvided =
                    capture.CallerEntryESI.HasValue,
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = GetLastDefinedMaskFlags(finalMask, null),
                ExactCleanExitAdmitted = true,
                StopAtRawFileOffsetExclusive =
                    UpstreamUnequalDestinationRawFileOffset
            };
        }

        if (!IsExactCleanHandoff(finalMask))
        {
            bool contaminatedAtBoundary = ReachedContaminatedBoundary(finalMask);
            return new(
                capture,
                finalMask,
                null,
                contaminatedAtBoundary
                    ? CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                        .FinalMaskContaminatedAtBoundary
                    : CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                        .FinalMaskIncomplete)
            {
                UnreachedCallerEntryEDICaptureProvided =
                    capture.CallerEntryEDI.HasValue,
                UnreachedCallerEntryESICaptureProvided =
                    capture.CallerEntryESI.HasValue,
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = GetLastDefinedMaskFlags(finalMask, null),
                StopAtRawFileOffsetExclusive =
                    finalMask.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                capture.CallerEntryEDI,
                capture.CallerEntryESI));
        foreach (CapacityFinalCalleeCommonTailTagWritesWrite write in commonTail.Writes)
        {
            writes.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeCommonTailTagWritesProjection
                        .FirstStoreRawFileOffset
                    ? CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind
                        .CommonTailFirstStore
                    : CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind
                        .CommonTailSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        CapacityFinalCalleeFinalMaskCommonLifecycleOutcome outcome =
            commonTail.Outcome switch
            {
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                        .CommonTailMissingEntryEDI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                        .CommonTailMissingEntryESI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.Completed =>
                    CapacityFinalCalleeFinalMaskCommonLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException(
                    "The common tail returned an unknown outcome.")
            };

        return new(capture, finalMask, commonTail, outcome)
        {
            ExactCleanHandoffToCommonTail = true,
            CommonTailReached = true,
            CallerEntryEDICaptured = commonTail.EntryEDICaptured,
            CallerEntryESICaptured = commonTail.EntryESICaptured,
            UnreachedCallerEntryESICaptureProvided =
                commonTail.UnreachedEntryESICaptureProvided,
            OrderedWrites = writes.ToImmutable(),
            FlagsAtStop = GetLastDefinedMaskFlags(finalMask, commonTail),
            ExactCleanExitAdmitted =
                commonTail.ExactCleanExitAdmitted
                && commonTail.StopAtRawFileOffsetExclusive
                    == CompletedBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive =
                commonTail.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactCleanHandoff(
        CapacityFinalCalleeFinalMaskLifecyclePlan plan) =>
        plan.Outcome == CapacityFinalCalleeFinalMaskLifecycleOutcome.Reached92BD
        && plan.MaskTailReached
        && plan.ExactCleanExitAdmitted
        && plan.MaskHelperExactCleanExitAdmitted
        && plan.StopAtRawFileOffsetExclusive == CommonTailEntryRawFileOffset
        && plan.MaskTail is
        {
            Outcome: CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor,
            FinalJumpReached: true,
            FinalJumpTaken: true,
            FinalJumpSelectedDestinationRawFileOffset:
                CapacityFinalCalleeFinalMaskLifecycleProjection.MaskTailSuccessorRawFileOffset,
            ExactCleanExitAdmitted: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeFinalMaskLifecycleProjection.MaskTailSuccessorRawFileOffset
        };

    private static bool IsExactCleanUnequalAt917C(
        CapacityFinalCalleeFinalMaskLifecyclePlan plan,
        CapacityFinalCalleeFinalMaskCommonLifecycleCapture capture) =>
        plan.Outcome
            == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamUnequalTo917C
        && !capture.CallerEntryEDI.HasValue
        && !capture.CallerEntryESI.HasValue
        && plan.ExactCleanExitAdmitted
        && plan.MaskTail is null
        && plan.UpstreamFinalFrameGate.ExactCleanExitAdmitted
        && plan.UpstreamFinalFrameGate.JnzReached
        && plan.UpstreamFinalFrameGate.JnzTaken == true
        && plan.UpstreamFinalFrameGate.JnzSelectedDestinationRawFileOffset
            == UpstreamUnequalDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == UpstreamUnequalDestinationRawFileOffset;

    private static bool ReachedContaminatedBoundary(
        CapacityFinalCalleeFinalMaskLifecyclePlan plan)
    {
        bool reachedUpstreamBranchBoundary =
            plan.UpstreamFinalFrameGate.JnzReached
            && plan.UpstreamFinalFrameGate.JnzSelectedDestinationRawFileOffset is
                uint selectedDestination
            && (selectedDestination
                    == CapacityFinalCalleeFinalMaskLifecycleProjection
                        .UpstreamEqualDestinationRawFileOffset
                || selectedDestination == UpstreamUnequalDestinationRawFileOffset);
        bool reachedMaskBoundary =
            plan.MaskTail is
            {
                FinalJumpReached: true,
                FinalJumpSelectedDestinationRawFileOffset:
                    CapacityFinalCalleeFinalMaskLifecycleProjection.MaskTailSuccessorRawFileOffset
            };
        return reachedUpstreamBranchBoundary || reachedMaskBoundary;
    }

    private static CapacityFinalCalleeAlternateMaskTailFlags? GetLastDefinedMaskFlags(
        CapacityFinalCalleeFinalMaskLifecyclePlan finalMask,
        CapacityFinalCalleeCommonTailTagWritesPlan? commonTail)
    {
        if (commonTail?.OrFlags is CapacityFinalCalleeCommonTailTagWritesFlags orFlags)
        {
            return new(
                orFlags.Carry,
                orFlags.Parity,
                orFlags.AuxiliaryCarry,
                orFlags.Zero,
                orFlags.Sign,
                orFlags.Overflow);
        }

        if (finalMask.MaskTail is CapacityFinalCalleeAlternateMaskTailPlan maskTail)
        {
            if (maskTail.FinalFlags is CapacityFinalCalleeAlternateMaskTailFlags finalFlags)
            {
                return finalFlags;
            }

            if (maskTail.FlagsAfterNot is
                CapacityFinalCalleeAlternateMaskTailFlags flagsAfterNot)
            {
                return flagsAfterNot;
            }

            if (maskTail.ShiftFlags is CapacityFinalCalleeAlternateMaskTailFlags shiftFlags)
            {
                return shiftFlags;
            }
        }

        return finalMask.UpstreamFinalFrameGate.CompareFlags is
            CapacityFinalCalleeAlternateFinalFrameGateFlags compareFlags
                ? new(
                    compareFlags.Carry,
                    compareFlags.Parity,
                    compareFlags.Auxiliary,
                    compareFlags.Zero,
                    compareFlags.Sign,
                    compareFlags.Overflow)
                : null;
    }
}
