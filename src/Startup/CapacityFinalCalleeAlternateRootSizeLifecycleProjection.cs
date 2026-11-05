namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateRootSizeLifecycleCapture(
    uint? GateEntryEDX,
    uint? GateFreshGlobal1EDADDC,
    uint? AlignedEntryECX,
    uint? AlignedEntryEDI,
    uint? AlignedEntryEBP);

public enum CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
{
    MissingGateEntryEDX,
    MissingGateEntryEDXWithAlignedSizeCaptures,
    MissingGateFreshGlobal,
    MissingGateFreshGlobalWithAlignedSizeCaptures,
    GateEqualFallthrough,
    GateEqualFallthroughWithAlignedSizeCaptures,
    MissingAlignedEntryECX,
    MissingAlignedEntryEDI,
    MissingAlignedEntryEBP,
    AlignedSizeBelowThresholdFallthrough,
    AlignedSizeAtOrAboveThresholdBranchTarget
}

public sealed record CapacityFinalCalleeAlternateRootSizeLifecyclePlan(
    CapacityFinalCalleeAlternateRootSizeLifecycleCapture Capture,
    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeAlternateRootGatePlan GatePlan { get; init; } = null!;
    public CapacityFinalCalleeAlternateAlignedSizePlan? AlignedSizePlan { get; init; }
    public bool GateExactCleanAdmission { get; init; }
    public bool GateUnequalAlternateSelected { get; init; }
    public bool GateEqualFallthroughSelected { get; init; }
    public bool AlignedSizeHandoffReached { get; init; }
    public bool AlignedSizeEntryValidatedAtHandoff { get; init; }
    public bool AlignedSizeCapturesRejected { get; init; }
    public bool UnreachedAlignedEntryECXCaptureProvided { get; init; }
    public bool UnreachedAlignedEntryEDICaptureProvided { get; init; }
    public bool UnreachedAlignedEntryEBPCaptureProvided { get; init; }
    public bool AlignedSizeExactCleanAdmission { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Follows only the clean unequal root-gate route into the aligned-size window.
/// </summary>
public static class CapacityFinalCalleeAlternateRootSizeLifecycleProjection
{
    public const uint GateFallthroughRawFileOffset = 0x012C90D2;
    public const uint GateAlternateRawFileOffset = 0x012C90F6;
    public const uint AlignedSizeFallthroughRawFileOffset = 0x012C910C;
    public const uint AlignedSizeBranchTargetRawFileOffset = 0x012C919B;

    public static CapacityFinalCalleeAlternateRootSizeLifecyclePlan Project(
        CapacityFinalCalleeAlternateRootSizeLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateRootGatePlan gate =
            CapacityFinalCalleeAlternateRootGateProjection.Project(new(
                capture.GateEntryEDX,
                capture.GateFreshGlobal1EDADDC));

        bool gateUnequalSelected =
            gate.ExactCleanAdmission
            && gate.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
            && gate.JnzReached
            && gate.JnzTaken == true
            && gate.JnzSelectedDestinationRawFileOffset == GateAlternateRawFileOffset
            && gate.StopAtRawFileOffsetExclusive == GateAlternateRawFileOffset;
        bool gateEqualSelected =
            gate.ExactCleanAdmission
            && gate.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && gate.JnzReached
            && gate.JnzTaken == false
            && gate.JnzSelectedDestinationRawFileOffset == GateFallthroughRawFileOffset
            && gate.StopAtRawFileOffsetExclusive == GateFallthroughRawFileOffset;

        bool alignedECXProvided = capture.AlignedEntryECX.HasValue;
        bool alignedEDIProvided = capture.AlignedEntryEDI.HasValue;
        bool alignedEBPProvided = capture.AlignedEntryEBP.HasValue;
        bool anyAlignedCaptureProvided =
            alignedECXProvided || alignedEDIProvided || alignedEBPProvided;

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan Finish(
            CapacityFinalCalleeAlternateRootSizeLifecycleOutcome outcome,
            uint stopAt,
            CapacityFinalCalleeAlternateAlignedSizePlan? alignedSize = null,
            bool alignedSizeHandoffReached = false,
            bool alignedSizeEntryValidatedAtHandoff = false,
            bool alignedSizeCapturesRejected = false,
            bool unreachedAlignedECX = false,
            bool unreachedAlignedEDI = false,
            bool unreachedAlignedEBP = false,
            bool alignedSizeExactCleanAdmission = false,
            uint? selectedDestination = null) =>
            new(capture, outcome)
            {
                GatePlan = gate,
                AlignedSizePlan = alignedSize,
                GateExactCleanAdmission = gate.ExactCleanAdmission,
                GateUnequalAlternateSelected = gateUnequalSelected,
                GateEqualFallthroughSelected = gateEqualSelected,
                AlignedSizeHandoffReached = alignedSizeHandoffReached,
                AlignedSizeEntryValidatedAtHandoff =
                    alignedSizeEntryValidatedAtHandoff,
                AlignedSizeCapturesRejected = alignedSizeCapturesRejected,
                UnreachedAlignedEntryECXCaptureProvided = unreachedAlignedECX,
                UnreachedAlignedEntryEDICaptureProvided = unreachedAlignedEDI,
                UnreachedAlignedEntryEBPCaptureProvided = unreachedAlignedEBP,
                AlignedSizeExactCleanAdmission = alignedSizeExactCleanAdmission,
                ExactCleanAdmission =
                    (gateUnequalSelected && alignedSizeExactCleanAdmission)
                    || (gateEqualSelected && !alignedSizeCapturesRejected),
                SelectedDestinationRawFileOffset = selectedDestination,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (gate.Outcome
            == CapacityFinalCalleeAlternateRootGateOutcome.MissingEntryEDX)
        {
            return Finish(
                anyAlignedCaptureProvided
                    ? CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingGateEntryEDXWithAlignedSizeCaptures
                    : CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingGateEntryEDX,
                gate.StopAtRawFileOffsetExclusive,
                alignedSizeCapturesRejected: anyAlignedCaptureProvided,
                unreachedAlignedECX: alignedECXProvided,
                unreachedAlignedEDI: alignedEDIProvided,
                unreachedAlignedEBP: alignedEBPProvided);
        }

        if (gate.Outcome
            == CapacityFinalCalleeAlternateRootGateOutcome.MissingFreshDWORD)
        {
            return Finish(
                anyAlignedCaptureProvided
                    ? CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingGateFreshGlobalWithAlignedSizeCaptures
                    : CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingGateFreshGlobal,
                gate.StopAtRawFileOffsetExclusive,
                alignedSizeCapturesRejected: anyAlignedCaptureProvided,
                unreachedAlignedECX: alignedECXProvided,
                unreachedAlignedEDI: alignedEDIProvided,
                unreachedAlignedEBP: alignedEBPProvided);
        }

        if (gateEqualSelected)
        {
            return Finish(
                anyAlignedCaptureProvided
                    ? CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .GateEqualFallthroughWithAlignedSizeCaptures
                    : CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .GateEqualFallthrough,
                GateFallthroughRawFileOffset,
                alignedSizeCapturesRejected: anyAlignedCaptureProvided,
                unreachedAlignedECX: alignedECXProvided,
                unreachedAlignedEDI: alignedEDIProvided,
                unreachedAlignedEBP: alignedEBPProvided);
        }

        if (!gateUnequalSelected)
        {
            throw new System.InvalidOperationException(
                "The alternate-root gate did not select either exact branch destination.");
        }

        CapacityFinalCalleeAlternateAlignedSizePlan alignedSizePlan =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                EntryValidated: true,
                EntryECX: capture.AlignedEntryECX,
                EntryEDI: capture.AlignedEntryEDI,
                EntryEBP: capture.AlignedEntryEBP));

        bool alignedSizeCleanAdmission =
            alignedSizePlan.ExactCleanAdmission
            && IsCleanAlignedSizeExit(alignedSizePlan);

        CapacityFinalCalleeAlternateRootSizeLifecycleOutcome outcome =
            alignedSizePlan.Outcome switch
            {
                CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX =>
                    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingAlignedEntryECX,
                CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingAlignedEntryEDI,
                CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEBP =>
                    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .MissingAlignedEntryEBP,
                CapacityFinalCalleeAlternateAlignedSizeOutcome.BelowThresholdFallthrough =>
                    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .AlignedSizeBelowThresholdFallthrough,
                CapacityFinalCalleeAlternateAlignedSizeOutcome.AtOrAboveThresholdBranchTarget =>
                    CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                        .AlignedSizeAtOrAboveThresholdBranchTarget,
                _ => throw new System.InvalidOperationException(
                    "The alternate aligned-size projection returned an unexpected outcome.")
            };

        return Finish(
            outcome,
            alignedSizePlan.StopAtRawFileOffsetExclusive,
            alignedSize: alignedSizePlan,
            alignedSizeHandoffReached: true,
            alignedSizeEntryValidatedAtHandoff:
                alignedSizePlan.Capture.EntryValidated,
            unreachedAlignedECX:
                alignedSizePlan.UnreachedEntryECXCaptureProvided,
            unreachedAlignedEDI:
                alignedSizePlan.UnreachedEntryEDICaptureProvided,
            unreachedAlignedEBP:
                alignedSizePlan.UnreachedEntryEBPCaptureProvided,
            alignedSizeExactCleanAdmission: alignedSizeCleanAdmission,
            selectedDestination:
                alignedSizePlan.SelectedDestinationRawFileOffset);
    }

    private static bool IsCleanAlignedSizeExit(
        CapacityFinalCalleeAlternateAlignedSizePlan plan)
    {
        if (!plan.StoreReached
            || plan.Store is not
            {
                RawFileOffset: CapacityFinalCalleeAlternateAlignedSizeProjection
                    .StoreRawFileOffset
            }
            || !plan.CompareReached
            || plan.CompareFlags is null
            || !plan.JnbReached
            || plan.JnbTaken is not bool jnbTaken
            || plan.SelectedDestinationRawFileOffset is not uint destination)
        {
            return false;
        }

        uint expectedDestination = jnbTaken
            ? AlignedSizeBranchTargetRawFileOffset
            : AlignedSizeFallthroughRawFileOffset;
        return destination == expectedDestination
            && plan.StopAtRawFileOffsetExclusive == expectedDestination
            && !plan.UnreachedEntryECXCaptureProvided
            && !plan.UnreachedEntryEDICaptureProvided
            && !plan.UnreachedEntryEBPCaptureProvided;
    }
}
