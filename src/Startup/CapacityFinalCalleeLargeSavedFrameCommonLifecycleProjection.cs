using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonLifecycleCapture(
    CapacityFinalCalleeLargeSavedFrameGateCapture LargeSavedFrameGateCapture,
    uint? CallerEntryEDI,
    uint? CallerEntryESI);

public enum CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome
{
    LargeSavedFrameGateIncomplete,
    NonzeroTo9231,
    CommonTailMissingEntryEDI,
    CommonTailMissingEntryESI,
    Completed
}

public enum CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind
{
    CommonTailFirstStore,
    CommonTailSecondStore
}

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedFrameCommonLifecycleWrite(
    CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind Kind,
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan(
    CapacityFinalCalleeLargeSavedFrameCommonLifecycleCapture Capture,
    CapacityFinalCalleeLargeSavedFrameGatePlan LargeSavedFrameGate,
    CapacityFinalCalleeCommonTailTagWritesPlan? CommonTail,
    CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome Outcome)
{
    public bool ExactCleanHandoffToCommonTail { get; init; }
    public bool CommonTailReached { get; init; }
    public bool CallerEntryEDICaptured { get; init; }
    public bool CallerEntryESICaptured { get; init; }
    public bool UnreachedCallerEntryEDICaptureProvided { get; init; }
    public bool UnreachedCallerEntryESICaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonLifecycleWrite>
        OrderedWrites { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeSavedFrameCommonLifecycleWrite>.Empty;
    public CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags? FlagsAtStop
        { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean zero branch of the fresh EBP-10 gate to the bounded common tail.
/// Caller EDI and ESI are consumed independently, in instruction order.
/// </summary>
public static class CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection
{
    public const uint CommonTailEntryRawFileOffset =
        CapacityFinalCalleeCommonTailTagWritesProjection.EntryRawFileOffset;
    public const uint CompletedBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonTailTagWritesProjection.BoundaryRawFileOffsetExclusive;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeSavedFrameGateProjection
            .NonzeroDestinationRawFileOffset;

    public static CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan Project(
        CapacityFinalCalleeLargeSavedFrameCommonLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeSavedFrameGatePlan gate =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(
                capture.LargeSavedFrameGateCapture);

        if (IsExactCleanNonzeroExit(gate))
        {
            return new(
                capture,
                gate,
                null,
                CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome.NonzeroTo9231)
            {
                UnreachedCallerEntryEDICaptureProvided =
                    capture.CallerEntryEDI.HasValue,
                UnreachedCallerEntryESICaptureProvided =
                    capture.CallerEntryESI.HasValue,
                FlagsAtStop = ConvertFlags(gate.TestFlags),
                ExactCleanExitAdmitted =
                    !capture.CallerEntryEDI.HasValue
                    && !capture.CallerEntryESI.HasValue,
                StopAtRawFileOffsetExclusive =
                    NonzeroDestinationRawFileOffset
            };
        }

        if (!IsExactCleanZeroHandoff(gate))
        {
            return new(
                capture,
                gate,
                null,
                CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome
                    .LargeSavedFrameGateIncomplete)
            {
                UnreachedCallerEntryEDICaptureProvided =
                    capture.CallerEntryEDI.HasValue,
                UnreachedCallerEntryESICaptureProvided =
                    capture.CallerEntryESI.HasValue,
                FlagsAtStop = ConvertFlags(gate.TestFlags),
                StopAtRawFileOffsetExclusive =
                    gate.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeCommonTailTagWritesPlan commonTail =
            CapacityFinalCalleeCommonTailTagWritesProjection.Project(new(
                capture.CallerEntryEDI,
                capture.CallerEntryEDI.HasValue ? capture.CallerEntryESI : null));
        var writes =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeSavedFrameCommonLifecycleWrite>();
        foreach (CapacityFinalCalleeCommonTailTagWritesWrite write in commonTail.Writes)
        {
            writes.Add(new(
                write.RawFileOffset
                    == CapacityFinalCalleeCommonTailTagWritesProjection
                        .FirstStoreRawFileOffset
                    ? CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind
                        .CommonTailFirstStore
                    : CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind
                        .CommonTailSecondStore,
                write.RawFileOffset,
                write.Address,
                write.Value));
        }

        CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome outcome =
            commonTail.Outcome switch
            {
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome
                        .CommonTailMissingEntryEDI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome
                        .CommonTailMissingEntryESI,
                CapacityFinalCalleeCommonTailTagWritesOutcome.Completed =>
                    CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome.Completed,
                _ => throw new InvalidOperationException(
                    "The common tail returned an unknown outcome.")
            };

        return new(capture, gate, commonTail, outcome)
        {
            ExactCleanHandoffToCommonTail = true,
            CommonTailReached = true,
            CallerEntryEDICaptured = commonTail.EntryEDICaptured,
            CallerEntryESICaptured = commonTail.EntryESICaptured,
            UnreachedCallerEntryESICaptureProvided =
                capture.CallerEntryESI.HasValue
                && !commonTail.EntryESICaptured,
            OrderedWrites = writes.ToImmutable(),
            FlagsAtStop = ConvertFlags(commonTail.OrFlags)
                ?? ConvertFlags(gate.TestFlags),
            ExactCleanExitAdmitted =
                commonTail.ExactCleanExitAdmitted
                && commonTail.StopAtRawFileOffsetExclusive
                    == CompletedBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive =
                commonTail.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactCleanZeroHandoff(
        CapacityFinalCalleeLargeSavedFrameGatePlan plan) =>
        plan.Outcome == CapacityFinalCalleeLargeSavedFrameGateOutcome.ZeroToTarget
        && plan.ExactCleanExitAdmitted
        && plan.FreshReadReached
        && plan.TestReached
        && plan.JzReached
        && plan.JzTaken == true
        && plan.JzSelectedDestinationRawFileOffset
            == CommonTailEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == CommonTailEntryRawFileOffset;

    private static bool IsExactCleanNonzeroExit(
        CapacityFinalCalleeLargeSavedFrameGatePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough
        && plan.ExactCleanExitAdmitted
        && plan.FreshReadReached
        && plan.TestReached
        && plan.JzReached
        && plan.JzTaken == false
        && plan.JzSelectedDestinationRawFileOffset
            == NonzeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == NonzeroDestinationRawFileOffset;

    private static CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeLargeSavedFrameGateFlags? flags) =>
            flags is CapacityFinalCalleeLargeSavedFrameGateFlags value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.Auxiliary,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;

    private static CapacityFinalCalleeLargeSavedFrameCommonLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeCommonTailTagWritesFlags? flags) =>
            flags is CapacityFinalCalleeCommonTailTagWritesFlags value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.AuxiliaryCarry,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}
