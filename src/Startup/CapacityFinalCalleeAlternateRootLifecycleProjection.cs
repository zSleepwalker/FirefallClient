using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateRootLifecycleCapture(
    uint? GateEntryEDX,
    uint? GateFreshGlobal1EDADDC,
    uint? DownstreamFreshGlobal1EDADD0,
    uint? DownstreamEntryEDI,
    uint? DownstreamEntryESI);

public enum CapacityFinalCalleeAlternateRootLifecycleOutcome
{
    MissingGateEntryEDX,
    MissingGateFreshGlobal,
    GateUnequalToAlternateTarget,
    UnequalGateWithDownstreamCaptures,
    MissingDownstreamFreshGlobal,
    MissingDownstreamEntryEDI,
    MissingDownstreamEntryESI,
    Completed
}

public sealed record CapacityFinalCalleeAlternateRootLifecyclePlan(
    CapacityFinalCalleeAlternateRootLifecycleCapture Capture,
    CapacityFinalCalleeAlternateRootLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeAlternateRootGatePlan GatePlan { get; init; } = null!;
    public CapacityFinalCalleeAlternateRootWritesPlan? WritesPlan { get; init; }
    public bool GateExactCleanAdmission { get; init; }
    public bool GateEqualFallthroughSelected { get; init; }
    public bool DownstreamWritesReached { get; init; }
    public bool DownstreamCapturesRejected { get; init; }
    public bool UnreachedDownstreamFreshGlobalCaptureProvided { get; init; }
    public bool UnreachedDownstreamEntryEDICaptureProvided { get; init; }
    public bool UnreachedDownstreamEntryESICaptureProvided { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint? FinalJumpTargetRawFileOffset { get; init; }
    public uint? FinalJumpTargetVirtualAddress { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the clean equal root gate with the independent fresh-global root writes.
/// </summary>
public static class CapacityFinalCalleeAlternateRootLifecycleProjection
{
    public const uint GateFallthroughRawFileOffset = 0x012C90D2;
    public const uint GateAlternateRawFileOffset = 0x012C90F6;
    public const uint FinalJumpTargetRawFileOffset = 0x012C9441;
    public const uint FinalJumpTargetVirtualAddress = 0x016CA041;

    public static CapacityFinalCalleeAlternateRootLifecyclePlan Project(
        CapacityFinalCalleeAlternateRootLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateRootGatePlan gate =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                capture.GateEntryEDX,
                capture.GateFreshGlobal1EDADDC));

        bool gateEqualFallthroughSelected =
            gate.ExactCleanAdmission
            && gate.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && gate.JnzReached
            && gate.JnzTaken == false
            && gate.JnzSelectedDestinationRawFileOffset == GateFallthroughRawFileOffset
            && gate.StopAtRawFileOffsetExclusive == GateFallthroughRawFileOffset;
        bool gateUnequalAlternateSelected =
            gate.ExactCleanAdmission
            && gate.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
            && gate.JnzReached
            && gate.JnzTaken == true
            && gate.JnzSelectedDestinationRawFileOffset == GateAlternateRawFileOffset
            && gate.StopAtRawFileOffsetExclusive == GateAlternateRawFileOffset;

        bool downstreamFreshGlobalProvided =
            capture.DownstreamFreshGlobal1EDADD0.HasValue;
        bool downstreamEdiProvided = capture.DownstreamEntryEDI.HasValue;
        bool downstreamEsiProvided = capture.DownstreamEntryESI.HasValue;

        CapacityFinalCalleeAlternateRootLifecyclePlan Finish(
            CapacityFinalCalleeAlternateRootLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateRootWritesPlan? writesPlan = null,
            bool downstreamWritesReached = false,
            bool downstreamCapturesRejected = false,
            bool unreachedDownstreamFreshGlobal = false,
            bool unreachedDownstreamEdi = false,
            bool unreachedDownstreamEsi = false) =>
            new(capture, outcome)
            {
                GatePlan = gate,
                WritesPlan = writesPlan,
                GateExactCleanAdmission = gate.ExactCleanAdmission,
                GateEqualFallthroughSelected = gateEqualFallthroughSelected,
                DownstreamWritesReached = downstreamWritesReached,
                DownstreamCapturesRejected = downstreamCapturesRejected,
                UnreachedDownstreamFreshGlobalCaptureProvided =
                    unreachedDownstreamFreshGlobal,
                UnreachedDownstreamEntryEDICaptureProvided =
                    unreachedDownstreamEdi
                    || writesPlan?.UnreachedEntryEDICaptureProvided == true,
                UnreachedDownstreamEntryESICaptureProvided =
                    unreachedDownstreamEsi
                    || writesPlan?.UnreachedEntryESICaptureProvided == true,
                Writes = writesPlan?.Writes
                    ?? ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite>.Empty,
                ExactCleanExitAdmitted =
                    (gateUnequalAlternateSelected
                        && !downstreamCapturesRejected
                        && !unreachedDownstreamFreshGlobal
                        && !unreachedDownstreamEdi
                        && !unreachedDownstreamEsi)
                    || (gateEqualFallthroughSelected
                        && writesPlan?.ExactCleanExitAdmitted == true
                        && writesPlan.JumpTargetRawFileOffset == FinalJumpTargetRawFileOffset
                        && stopAt == FinalJumpTargetRawFileOffset),
                FinalJumpTargetRawFileOffset =
                    gateEqualFallthroughSelected ? writesPlan?.JumpTargetRawFileOffset : null,
                FinalJumpTargetVirtualAddress =
                    gateEqualFallthroughSelected ? writesPlan?.JumpTargetVirtualAddress : null,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (gate.Outcome
            == CapacityFinalCalleeAlternateRootGateOutcome.MissingEntryEDX)
        {
            return Finish(
                CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingGateEntryEDX,
                gate.StopAtRawFileOffsetExclusive,
                unreachedDownstreamFreshGlobal: downstreamFreshGlobalProvided,
                unreachedDownstreamEdi: downstreamEdiProvided,
                unreachedDownstreamEsi: downstreamEsiProvided);
        }

        if (gate.Outcome
            == CapacityFinalCalleeAlternateRootGateOutcome.MissingFreshDWORD)
        {
            return Finish(
                CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingGateFreshGlobal,
                gate.StopAtRawFileOffsetExclusive,
                unreachedDownstreamFreshGlobal: downstreamFreshGlobalProvided,
                unreachedDownstreamEdi: downstreamEdiProvided,
                unreachedDownstreamEsi: downstreamEsiProvided);
        }

        if (gateUnequalAlternateSelected)
        {
            bool downstreamCapturesProvided =
                downstreamFreshGlobalProvided || downstreamEdiProvided || downstreamEsiProvided;
            return Finish(
                downstreamCapturesProvided
                    ? CapacityFinalCalleeAlternateRootLifecycleOutcome
                        .UnequalGateWithDownstreamCaptures
                    : CapacityFinalCalleeAlternateRootLifecycleOutcome
                        .GateUnequalToAlternateTarget,
                GateAlternateRawFileOffset,
                downstreamCapturesRejected: downstreamCapturesProvided,
                unreachedDownstreamFreshGlobal: downstreamFreshGlobalProvided,
                unreachedDownstreamEdi: downstreamEdiProvided,
                unreachedDownstreamEsi: downstreamEsiProvided);
        }

        if (!gateEqualFallthroughSelected)
        {
            throw new System.InvalidOperationException(
                "The alternate-root gate did not select either exact branch destination.");
        }

        CapacityFinalCalleeAlternateRootWritesPlan writes =
            CapacityFinalCalleeAlternateRootWritesProjection.Project(new(
                capture.DownstreamFreshGlobal1EDADD0,
                capture.DownstreamEntryEDI,
                capture.DownstreamEntryESI));
        CapacityFinalCalleeAlternateRootLifecycleOutcome outcome =
            writes.Outcome switch
            {
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingFreshGlobal =>
                    CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamFreshGlobal,
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamEntryEDI,
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryESI =>
                    CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamEntryESI,
                CapacityFinalCalleeAlternateRootWritesOutcome.Completed =>
                    CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed,
                _ => throw new System.InvalidOperationException(
                    "The alternate-root write projection returned an unknown outcome.")
            };

        return Finish(
            outcome,
            writes.StopAtRawFileOffsetExclusive,
            writesPlan: writes,
            downstreamWritesReached: true);
    }
}
