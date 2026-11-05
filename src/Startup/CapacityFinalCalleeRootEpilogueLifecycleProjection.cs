using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeRootEpilogueLifecycleCapture(
    CapacityFinalCalleeAlternateRootLifecycleCapture AlternateRootCapture,
    CapacityFinalCalleeGatedEpilogueCapture GatedEpilogueCapture);

public enum CapacityFinalCalleeRootEpilogueLifecycleOutcome
{
    RootLifecycleIncomplete,
    RootLifecycleIncompleteWithEpilogueCaptures,
    RootGateUnequalAt90F6,
    RootGateUnequalAt90F6WithEpilogueCaptures,
    EpilogueEntryNotValidated,
    EpilogueIncomplete,
    EpilogueContaminated,
    CompletedAt945C
}

public sealed record CapacityFinalCalleeRootEpilogueLifecyclePlan(
    CapacityFinalCalleeRootEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeAlternateRootLifecyclePlan AlternateRoot,
    CapacityFinalCalleeGatedEpiloguePlan? GatedEpilogue,
    bool GatedEpilogueReached,
    bool ExactJmp9441HandoffAdmitted,
    bool UnreachedEntryValidatedClaimProvided,
    bool UnreachedTestedByteCaptureProvided,
    bool UnreachedStackPopEdiCaptureProvided,
    bool UnreachedStackPopEsiCaptureProvided,
    bool UnreachedStackPopEbxCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool UnreachedPostCallEbpCaptureProvided,
    bool UnreachedStackPopEbpCaptureProvided,
    bool UnreachedReturnTargetCaptureProvided,
    CapacityFinalCalleeRootEpilogueLifecycleOutcome Outcome,
    bool ExactCleanExitAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite> OrderedUpstreamWrites =>
        AlternateRoot.Writes;

    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> EpilogueStackAccesses =>
        GatedEpilogue?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
}

/// <summary>
/// Joins the clean alternate-root route through JMP 9441 to the independently captured
/// gated epilogue. Entry ESP and EBP remain independent inputs.
/// </summary>
public static class CapacityFinalCalleeRootEpilogueLifecycleProjection
{
    public const uint AlternateRootEntryRawFileOffset =
        CapacityFinalCalleeAlternateRootGateProjection.EntryRawFileOffset;
    public const uint GatedEpilogueEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint UnequalGateStopRawFileOffsetExclusive =
        CapacityFinalCalleeAlternateRootLifecycleProjection.GateAlternateRawFileOffset;
    public const uint FinalReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;
    public const uint FinalJmp9441TargetVirtualAddress =
        CapacityFinalCalleeAlternateRootLifecycleProjection.FinalJumpTargetVirtualAddress;

    public static CapacityFinalCalleeRootEpilogueLifecyclePlan Project(
        CapacityFinalCalleeRootEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateRootLifecyclePlan alternateRoot =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(
                capture.AlternateRootCapture);
        CapacityFinalCalleeGatedEpilogueCapture epilogueCapture =
            capture.GatedEpilogueCapture;

        if (!IsExactJmp9441Handoff(alternateRoot))
        {
            bool anyEpilogueCaptureProvided =
                epilogueCapture.EntryValidated || HasEpilogueCaptures(epilogueCapture);
            return new(
                capture,
                alternateRoot,
                GatedEpilogue: null,
                GatedEpilogueReached: false,
                ExactJmp9441HandoffAdmitted: false,
                UnreachedEntryValidatedClaimProvided: epilogueCapture.EntryValidated,
                UnreachedTestedByteCaptureProvided: epilogueCapture.TestedByte.HasValue,
                UnreachedStackPopEdiCaptureProvided:
                    epilogueCapture.StackPopEdiValue.HasValue,
                UnreachedStackPopEsiCaptureProvided:
                    epilogueCapture.StackPopEsiValue.HasValue,
                UnreachedStackPopEbxCaptureProvided:
                    epilogueCapture.StackPopEbxValue.HasValue,
                UnreachedCallCellTargetCaptureProvided:
                    epilogueCapture.CallCellTargetVirtualAddress.HasValue,
                UnreachedCallReturnStatusProvided:
                    epilogueCapture.CallReturnedNormally.HasValue,
                UnreachedPostCallEbpCaptureProvided:
                    epilogueCapture.PostCallEbp.HasValue,
                UnreachedStackPopEbpCaptureProvided:
                    epilogueCapture.StackPopEbpValue.HasValue,
                UnreachedReturnTargetCaptureProvided:
                    epilogueCapture.ReturnTargetVirtualAddress.HasValue,
                Outcome: IsCleanUnequalRootExit(alternateRoot)
                    ? anyEpilogueCaptureProvided
                        ? CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .RootGateUnequalAt90F6WithEpilogueCaptures
                        : CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .RootGateUnequalAt90F6
                    : anyEpilogueCaptureProvided
                        ? CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .RootLifecycleIncompleteWithEpilogueCaptures
                        : CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .RootLifecycleIncomplete,
                ExactCleanExitAdmitted:
                    IsCleanUnequalRootExit(alternateRoot) && !anyEpilogueCaptureProvided,
                StopAtRawFileOffsetExclusive:
                    alternateRoot.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeGatedEpiloguePlan gatedEpilogue =
            CapacityFinalCalleeGatedEpilogueProjection.Project(
                epilogueCapture with
                {
                    EntryValidated =
                        epilogueCapture.EntryValidated
                        && IsExactJmp9441Handoff(alternateRoot)
                });
        bool exactCleanExitAdmitted = IsExactCleanExit(gatedEpilogue);
        bool hasUnreachedEpilogueCapture =
            gatedEpilogue.UnreachedTestedByteCaptureProvided
            || gatedEpilogue.UnreachedStackPopEdiCaptureProvided
            || gatedEpilogue.UnreachedStackPopEsiCaptureProvided
            || gatedEpilogue.UnreachedStackPopEbxCaptureProvided
            || gatedEpilogue.UnreachedCallCellTargetCaptureProvided
            || gatedEpilogue.UnreachedCallReturnStatusProvided
            || gatedEpilogue.UnreachedPostCallEbpCaptureProvided
            || gatedEpilogue.UnreachedStackPopEbpCaptureProvided
            || gatedEpilogue.UnreachedReturnTargetCaptureProvided;

        CapacityFinalCalleeRootEpilogueLifecycleOutcome outcome =
            exactCleanExitAdmitted
                ? CapacityFinalCalleeRootEpilogueLifecycleOutcome.CompletedAt945C
                : gatedEpilogue.Outcome
                    == CapacityFinalCalleeGatedEpilogueOutcome.EntryNotValidated
                    ? CapacityFinalCalleeRootEpilogueLifecycleOutcome
                        .EpilogueEntryNotValidated
                    : hasUnreachedEpilogueCapture
                        ? CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .EpilogueContaminated
                        : CapacityFinalCalleeRootEpilogueLifecycleOutcome
                            .EpilogueIncomplete;

        return new(
            capture,
            alternateRoot,
            gatedEpilogue,
            GatedEpilogueReached: true,
            ExactJmp9441HandoffAdmitted: true,
            UnreachedEntryValidatedClaimProvided: false,
            UnreachedTestedByteCaptureProvided:
                gatedEpilogue.UnreachedTestedByteCaptureProvided,
            UnreachedStackPopEdiCaptureProvided:
                gatedEpilogue.UnreachedStackPopEdiCaptureProvided,
            UnreachedStackPopEsiCaptureProvided:
                gatedEpilogue.UnreachedStackPopEsiCaptureProvided,
            UnreachedStackPopEbxCaptureProvided:
                gatedEpilogue.UnreachedStackPopEbxCaptureProvided,
            UnreachedCallCellTargetCaptureProvided:
                gatedEpilogue.UnreachedCallCellTargetCaptureProvided,
            UnreachedCallReturnStatusProvided:
                gatedEpilogue.UnreachedCallReturnStatusProvided,
            UnreachedPostCallEbpCaptureProvided:
                gatedEpilogue.UnreachedPostCallEbpCaptureProvided,
            UnreachedStackPopEbpCaptureProvided:
                gatedEpilogue.UnreachedStackPopEbpCaptureProvided,
            UnreachedReturnTargetCaptureProvided:
                gatedEpilogue.UnreachedReturnTargetCaptureProvided,
            outcome,
            exactCleanExitAdmitted,
            gatedEpilogue.StopAtRawFileOffsetExclusive);
    }

    private static bool HasEpilogueCaptures(
        CapacityFinalCalleeGatedEpilogueCapture capture) =>
        capture.TestedByte.HasValue
        || capture.StackPopEdiValue.HasValue
        || capture.StackPopEsiValue.HasValue
        || capture.StackPopEbxValue.HasValue
        || capture.CallCellTargetVirtualAddress.HasValue
        || capture.CallReturnedNormally.HasValue
        || capture.PostCallEbp.HasValue
        || capture.StackPopEbpValue.HasValue
        || capture.ReturnTargetVirtualAddress.HasValue;

    private static bool IsCleanUnequalRootExit(
        CapacityFinalCalleeAlternateRootLifecyclePlan plan) =>
        plan.Outcome
            == CapacityFinalCalleeAlternateRootLifecycleOutcome.GateUnequalToAlternateTarget
        && plan.GateExactCleanAdmission
        && !plan.GateEqualFallthroughSelected
        && !plan.DownstreamWritesReached
        && !plan.DownstreamCapturesRejected
        && plan.ExactCleanExitAdmitted
        && plan.FinalJumpTargetRawFileOffset is null
        && plan.FinalJumpTargetVirtualAddress is null
        && plan.StopAtRawFileOffsetExclusive == UnequalGateStopRawFileOffsetExclusive;

    private static bool IsExactJmp9441Handoff(
        CapacityFinalCalleeAlternateRootLifecyclePlan plan) =>
        plan.Outcome == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
        && plan.ExactCleanExitAdmitted
        && plan.GateExactCleanAdmission
        && plan.GateEqualFallthroughSelected
        && plan.DownstreamWritesReached
        && !plan.DownstreamCapturesRejected
        && plan.FinalJumpTargetRawFileOffset == GatedEpilogueEntryRawFileOffset
        && plan.FinalJumpTargetVirtualAddress == FinalJmp9441TargetVirtualAddress
        && plan.StopAtRawFileOffsetExclusive == GatedEpilogueEntryRawFileOffset;

    private static bool IsExactCleanExit(
        CapacityFinalCalleeGatedEpiloguePlan plan) =>
        plan.ReturnReached
        && plan.ExactCleanExitAdmitted
        && plan.StopAtRawFileOffsetExclusive
            == FinalReturnBoundaryRawFileOffsetExclusive
        && plan.Outcome is
            CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
                or CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
        && !plan.UnreachedTestedByteCaptureProvided
        && !plan.UnreachedStackPopEdiCaptureProvided
        && !plan.UnreachedStackPopEsiCaptureProvided
        && !plan.UnreachedStackPopEbxCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.UnreachedPostCallEbpCaptureProvided
        && !plan.UnreachedStackPopEbpCaptureProvided
        && !plan.UnreachedReturnTargetCaptureProvided;
}
