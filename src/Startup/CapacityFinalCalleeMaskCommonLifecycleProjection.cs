using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeMaskCommonLifecycleCapture(
    CapacityFinalCalleeAlternateMaskTailCapture MaskTailCapture,
    uint? CallerEntryEDI,
    uint? CallerEntryESI);

public enum CapacityFinalCalleeMaskCommonLifecycleOutcome
{
    MaskTailIncomplete,
    MaskTailContaminatedAtSuccessor,
    CommonTailMissingEntryEDI,
    CommonTailMissingEntryESI,
    Completed
}

public enum CapacityFinalCalleeMaskCommonLifecycleWriteKind
{
    MaskGlobal,
    CommonTailFirstStore,
    CommonTailSecondStore
}

public readonly record struct CapacityFinalCalleeMaskCommonLifecycleWrite(
    CapacityFinalCalleeMaskCommonLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeMaskCommonLifecyclePlan(
    CapacityFinalCalleeMaskCommonLifecycleCapture Capture,
    CapacityFinalCalleeAlternateMaskTailPlan MaskTail,
    CapacityFinalCalleeCommonTailTagWritesPlan? CommonTail,
    CapacityFinalCalleeMaskCommonLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToCommonTail { get; init; }
    public bool CommonTailReached { get; init; }
    public bool CallerEntryEDICaptured { get; init; }
    public bool CallerEntryESICaptured { get; init; }
    public bool UnreachedCallerEntryEDICaptureProvided { get; init; }
    public bool UnreachedCallerEntryESICaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeMaskCommonLifecycleWrite> OrderedWrites { get; init; } =
        ImmutableArray<CapacityFinalCalleeMaskCommonLifecycleWrite>.Empty;
    public CapacityFinalCalleeAlternateMaskTailFlags? FlagsAtStop { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the alternate mask tail's clean 92BD handoff with the bounded common tail.
/// Caller registers and the mask tail's fresh global read remain independent captures.
/// </summary>
public static class CapacityFinalCalleeMaskCommonLifecycleProjection
{
    public const uint CommonTailEntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint CompletedBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint MaskGlobalWriteRawFileOffset =
        CapacityFinalCalleeAlternateMaskTailProjection.GlobalAndRawFileOffset;
    public const uint MaskGlobalVirtualAddress =
        CapacityFinalCalleeAlternateMaskTailProjection.GlobalVirtualAddress;

    public static CapacityFinalCalleeMaskCommonLifecyclePlan Project(
        CapacityFinalCalleeMaskCommonLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateMaskTailPlan maskTail =
            CapacityFinalCalleeAlternateMaskTailProjection.Project(capture.MaskTailCapture);
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeMaskCommonLifecycleWrite>();

        if (maskTail.GlobalWriteReached
            && maskTail.GlobalWriteAddress is uint globalAddress
            && maskTail.GlobalWriteValue is uint globalValue)
        {
            writes.Add(new(
                CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal,
                MaskGlobalWriteRawFileOffset,
                globalAddress,
                globalValue));
        }

        bool cleanHandoff = IsExactCleanHandoff(maskTail);
        if (!cleanHandoff)
        {
            bool contaminatedAtSuccessor =
                maskTail.FinalJumpReached
                && maskTail.FinalJumpSelectedDestinationRawFileOffset
                    == CommonTailEntryRawFileOffset
                && !maskTail.ExactCleanExitAdmitted;
            return new(
                capture,
                maskTail,
                null,
                contaminatedAtSuccessor
                    ? CapacityFinalCalleeMaskCommonLifecycleOutcome
                        .MaskTailContaminatedAtSuccessor
                    : CapacityFinalCalleeMaskCommonLifecycleOutcome.MaskTailIncomplete)
            {
                UnreachedCallerEntryEDICaptureProvided = capture.CallerEntryEDI.HasValue,
                UnreachedCallerEntryESICaptureProvided = capture.CallerEntryESI.HasValue,
                OrderedWrites = writes.ToImmutable(),
                FlagsAtStop = maskTail.FinalFlags ?? maskTail.FlagsAfterNot ?? maskTail.ShiftFlags,
                StopAtRawFileOffsetExclusive = maskTail.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                capture.CallerEntryEDI,
                capture.CallerEntryESI));
        bool ediCaptured = commonTail.EntryEDICaptured;
        bool esiCaptured = commonTail.EntryESICaptured;
        foreach (CapacityFinalCalleeCommonTailTagWritesWrite write in commonTail.Writes)
        {
            writes.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset
                    ? CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailFirstStore
                    : CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        CapacityFinalCalleeMaskCommonLifecycleOutcome outcome =
            commonTail.Outcome switch
            {
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeMaskCommonLifecycleOutcome.CommonTailMissingEntryEDI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeMaskCommonLifecycleOutcome.CommonTailMissingEntryESI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.Completed =>
                    CapacityFinalCalleeMaskCommonLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException(
                    "The common tail returned an unknown outcome.")
            };

        return new(
            capture,
            maskTail,
            commonTail,
            outcome)
        {
            ExactCleanHandoffToCommonTail = true,
            CommonTailReached = true,
            CallerEntryEDICaptured = ediCaptured,
            CallerEntryESICaptured = esiCaptured,
            UnreachedCallerEntryESICaptureProvided =
                commonTail.UnreachedEntryESICaptureProvided,
            OrderedWrites = writes.ToImmutable(),
            FlagsAtStop = commonTail.OrFlags is { } orFlags
                ? new(
                    orFlags.Carry,
                    orFlags.Parity,
                    orFlags.AuxiliaryCarry,
                    orFlags.Zero,
                    orFlags.Sign,
                    orFlags.Overflow)
                : maskTail.FinalFlags,
            ExactCleanExitAdmitted =
                commonTail.ExactCleanExitAdmitted
                && commonTail.StopAtRawFileOffsetExclusive
                    == CompletedBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive = commonTail.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactCleanHandoff(
        CapacityFinalCalleeAlternateMaskTailPlan plan) =>
        plan.Outcome == CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor
        && plan.FinalJumpReached
        && plan.FinalJumpTaken == true
        && plan.FinalJumpSelectedDestinationRawFileOffset == CommonTailEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == CommonTailEntryRawFileOffset
        && plan.GlobalWriteReached
        && plan.GlobalWriteAddress == MaskGlobalVirtualAddress
        && plan.GlobalWriteValue.HasValue
        && plan.ExactCleanExitAdmitted;
}
