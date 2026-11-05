namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSuccessorWritesLifecycleCapture(
    CapacityFinalCalleeAlternateSuccessorGateCapture GateCapture,
    uint? DownstreamEntryECX);

public enum CapacityFinalCalleeSuccessorWritesLifecycleOutcome
{
    GateIncomplete,
    GateIncompleteWithDownstreamECXCapture,
    OpaqueExitAt918A,
    OpaqueExitAt918AWithDownstreamECXCapture,
    OpaqueExitAt918AWithUnreachedGateCaptures,
    OpaqueExitAt918AWithUnreachedGateCapturesAndDownstreamECXCapture,
    CleanGateRejected,
    CleanGateRejectedWithDownstreamECXCapture,
    SuccessorWritesIncomplete,
    CompletedAt92BD
}

public sealed record CapacityFinalCalleeSuccessorWritesLifecyclePlan(
    CapacityFinalCalleeSuccessorWritesLifecycleCapture Capture,
    CapacityFinalCalleeAlternateSuccessorGatePlan Gate,
    CapacityFinalCalleeAlternateSuccessorWritesPlan? SuccessorWrites,
    bool SuccessorWritesReached,
    bool ExactCleanHandoffAdmitted,
    bool DownstreamEntryEAXDerived,
    uint? DownstreamEntryEAX,
    bool UnreachedDownstreamEntryECXCaptureProvided,
    CapacityFinalCalleeSuccessorWritesLifecycleOutcome Outcome,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins either clean 9190 gate route to the successor stores. The gate's CMP
/// leaves entry EAX available; caller ECX is an independent downstream capture.
/// The opaque 918A routes remain bounded and never enter the stores.
/// </summary>
public static class CapacityFinalCalleeSuccessorWritesLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeAlternateSuccessorGateProjection.EntryRawFileOffset;
    public const uint SuccessorWritesEntryRawFileOffset =
        CapacityFinalCalleeAlternateSuccessorWritesProjection.EntryRawFileOffset;
    public const uint OpaqueCallBoundaryRawFileOffset =
        CapacityFinalCalleeAlternateSuccessorGateProjection.OpaqueCallBoundaryRawFileOffset;
    public const uint SuccessorWritesTargetRawFileOffset =
        CapacityFinalCalleeAlternateSuccessorWritesProjection.JumpTargetRawFileOffset;

    public static CapacityFinalCalleeSuccessorWritesLifecyclePlan Project(
        CapacityFinalCalleeSuccessorWritesLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateSuccessorGatePlan gate =
            CapacityFinalCalleeAlternateSuccessorGateProjection.Project(
                capture.GateCapture);
        bool downstreamECXProvided = capture.DownstreamEntryECX.HasValue;

        if (IsOpaqueExit(gate))
        {
            bool unreachedGateCaptures = !gate.ExactCleanExitAdmitted;
            return new(
                capture,
                gate,
                SuccessorWrites: null,
                SuccessorWritesReached: false,
                ExactCleanHandoffAdmitted: false,
                DownstreamEntryEAXDerived: false,
                DownstreamEntryEAX: null,
                UnreachedDownstreamEntryECXCaptureProvided:
                    downstreamECXProvided,
                Outcome: (unreachedGateCaptures, downstreamECXProvided) switch
                {
                    (false, false) =>
                        CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .OpaqueExitAt918A,
                    (false, true) =>
                        CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .OpaqueExitAt918AWithDownstreamECXCapture,
                    (true, false) =>
                        CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .OpaqueExitAt918AWithUnreachedGateCaptures,
                    (true, true) =>
                        CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .OpaqueExitAt918AWithUnreachedGateCapturesAndDownstreamECXCapture
                },
                Admitted: gate.ExactCleanExitAdmitted
                    && !downstreamECXProvided,
                StopAtRawFileOffsetExclusive:
                    CapacityFinalCalleeAlternateSuccessorGateProjection
                        .OpaqueCallBoundaryRawFileOffset);
        }

        if (Is9190GateExit(gate))
        {
            if (!IsExactCleanGateExit(gate))
            {
                return new(
                    capture,
                    gate,
                    SuccessorWrites: null,
                    SuccessorWritesReached: false,
                    ExactCleanHandoffAdmitted: false,
                    DownstreamEntryEAXDerived: false,
                    DownstreamEntryEAX: null,
                    UnreachedDownstreamEntryECXCaptureProvided:
                        downstreamECXProvided,
                    Outcome: downstreamECXProvided
                        ? CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .CleanGateRejectedWithDownstreamECXCapture
                        : CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                            .CleanGateRejected,
                    Admitted: false,
                    StopAtRawFileOffsetExclusive:
                        CapacityFinalCalleeAlternateSuccessorGateProjection
                            .BoundaryRawFileOffsetExclusive);
            }

            uint entryEax = capture.GateCapture.EntryEAX!.Value;
            CapacityFinalCalleeAlternateSuccessorWritesPlan writes =
                CapacityFinalCalleeAlternateSuccessorWritesProjection.Project(new(
                    EntryECX: capture.DownstreamEntryECX,
                    EntryEAX: entryEax));
            bool completed = writes.Outcome
                == CapacityFinalCalleeAlternateSuccessorWritesOutcome.Completed
                && writes.ExactCleanExitAdmitted
                && writes.StopAtRawFileOffsetExclusive
                    == SuccessorWritesTargetRawFileOffset;
            return new(
                capture,
                gate,
                writes,
                SuccessorWritesReached: true,
                ExactCleanHandoffAdmitted: true,
                DownstreamEntryEAXDerived: true,
                DownstreamEntryEAX: entryEax,
                UnreachedDownstreamEntryECXCaptureProvided: false,
                Outcome: completed
                    ? CapacityFinalCalleeSuccessorWritesLifecycleOutcome.CompletedAt92BD
                    : CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .SuccessorWritesIncomplete,
                Admitted: completed,
                StopAtRawFileOffsetExclusive:
                    writes.StopAtRawFileOffsetExclusive);
        }

        bool gateIncomplete = !IsExactCleanGateExit(gate);
        return new(
            capture,
            gate,
            SuccessorWrites: null,
            SuccessorWritesReached: false,
            ExactCleanHandoffAdmitted: false,
            DownstreamEntryEAXDerived: false,
            DownstreamEntryEAX: null,
            UnreachedDownstreamEntryECXCaptureProvided: downstreamECXProvided,
            Outcome: gateIncomplete
                ? downstreamECXProvided
                    ? CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .GateIncompleteWithDownstreamECXCapture
                    : CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .GateIncomplete
                : downstreamECXProvided
                    ? CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .CleanGateRejectedWithDownstreamECXCapture
                    : CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .CleanGateRejected,
            Admitted: false,
            StopAtRawFileOffsetExclusive: gate.StopAtRawFileOffsetExclusive);
    }

    private static bool IsOpaqueExit(
        CapacityFinalCalleeAlternateSuccessorGatePlan gate) =>
        gate.StopAtRawFileOffsetExclusive == OpaqueCallBoundaryRawFileOffset
        && (gate.Outcome is
            CapacityFinalCalleeAlternateSuccessorGateOutcome
                .BelowToOpaqueCallBoundary
            or CapacityFinalCalleeAlternateSuccessorGateOutcome
                .UnequalFreshFieldToOpaqueCallBoundary);

    private static bool Is9190GateExit(
        CapacityFinalCalleeAlternateSuccessorGatePlan gate) =>
        (gate.Outcome is
            CapacityFinalCalleeAlternateSuccessorGateOutcome
                .EqualFrameValueToEntryEAX
            or CapacityFinalCalleeAlternateSuccessorGateOutcome
                .EqualFreshFieldToEntryEDX)
        && gate.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeAlternateSuccessorGateProjection
                .BoundaryRawFileOffsetExclusive;

    private static bool IsExactCleanGateExit(
        CapacityFinalCalleeAlternateSuccessorGatePlan gate) =>
        gate.ExactCleanExitAdmitted
        && gate.ExactCleanBypassAdmitted
        && !gate.OpaqueCallReached
        && !gate.UnusedEntryEBXCaptureProvided
        && !gate.UnusedFreshFieldCaptureProvided
        && !gate.UnusedEntryEDXCaptureProvided
        && ((gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome.EqualFrameValueToEntryEAX
                && gate.FrameJzReached && gate.FrameJzTaken == true
                && gate.FrameJzSelectedDestinationRawFileOffset == SuccessorWritesEntryRawFileOffset)
            || (gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome.EqualFreshFieldToEntryEDX
                && gate.FieldJzReached && gate.FieldJzTaken == true
                && gate.FieldJzSelectedDestinationRawFileOffset == SuccessorWritesEntryRawFileOffset))
        && gate.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeAlternateSuccessorGateProjection
                .BoundaryRawFileOffsetExclusive;
}
