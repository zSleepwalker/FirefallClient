using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlignedFrameLifecycleCapture(
    bool EntryValidated,
    uint? EntryECX,
    uint? EntryEDI,
    uint? EntryEBP,
    uint? EntryEDX,
    uint? FreshEdxPlus8,
    uint? FreshEdxPlusC);

public readonly record struct CapacityFinalCalleeAlignedFrameLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public enum CapacityFinalCalleeAlignedFrameLifecycleOutcome
{
    EntryNotValidated,
    MissingEntryECX,
    MissingEntryEDI,
    MissingEntryEBPForSizeStore,
    SizeAtOrAboveThresholdBranchTarget,
    SizeGateNotClean,
    MissingFrameEntryEDX,
    MissingFreshEdxPlus8,
    MissingDerivedEBPForFirstStore,
    MissingFreshEdxPlusC,
    CompareEqualToOpaqueCallBoundary,
    CompareUnequalToBranchTarget
}

public sealed record CapacityFinalCalleeAlignedFrameLifecyclePlan(
    CapacityFinalCalleeAlignedFrameLifecycleCapture Capture,
    CapacityFinalCalleeAlignedFrameLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeAlternateAlignedSizePlan SizePlan { get; init; } = null!;
    public CapacityFinalCalleeAlternateFrameLoadsPlan? FrameLoadsPlan { get; init; }
    public bool FrameLoadsEntered { get; init; }
    public bool DownstreamFrameCapturesRejected { get; init; }
    public bool DerivedEBPCaptured { get; init; }
    public uint? DerivedEBP { get; init; }
    public bool UnreachedFrameEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus8CaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlusCCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the validated aligned-size gate with its below-threshold frame
/// loads. The size-store address supplies EBP only when the first frame store
/// is reached; EDX and both fresh loads remain independent captures.
/// </summary>
public static class CapacityFinalCalleeAlignedFrameLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeAlternateAlignedSizeProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeAlternateAlignedSizeProjection.EntryVirtualAddress;
    public const uint BelowThresholdRawFileOffset =
        CapacityFinalCalleeAlternateAlignedSizeProjection.FallthroughRawFileOffset;
    public const uint AtOrAboveThresholdRawFileOffset =
        CapacityFinalCalleeAlternateAlignedSizeProjection.BranchTargetRawFileOffset;
    public const uint OpaqueCallRawFileOffset =
        CapacityFinalCalleeAlternateFrameLoadsProjection.OpaqueCallRawFileOffset;
    public const uint UnequalDestinationRawFileOffset =
        CapacityFinalCalleeAlternateFrameLoadsProjection.UnequalDestinationRawFileOffset;

    public static ImmutableArray<CapacityFinalCalleeAlignedFrameLifecycleInstructionEvidence>
        Instructions { get; } =
        CapacityFinalCalleeAlternateAlignedSizeProjection.Instructions
            .Select(instruction =>
                new CapacityFinalCalleeAlignedFrameLifecycleInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.FlatIdaAddress,
                    instruction.VirtualAddress,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
            .Concat(CapacityFinalCalleeAlternateFrameLoadsProjection.Instructions
                .Select(instruction =>
                    new CapacityFinalCalleeAlignedFrameLifecycleInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.FlatIdaAddress,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress)))
            .ToImmutableArray();

    public static string CapturedWindowBytes =>
        CapacityFinalCalleeAlternateAlignedSizeProjection.CapturedWindowBytes
        + CapacityFinalCalleeAlternateFrameLoadsProjection.CapturedWindowBytes;

    public static CapacityFinalCalleeAlignedFrameLifecyclePlan Project(
        CapacityFinalCalleeAlignedFrameLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateAlignedSizePlan sizePlan =
            CapacityFinalCalleeAlternateAlignedSizeProjection.Project(new(
                capture.EntryValidated,
                capture.EntryECX,
                capture.EntryEDI,
                capture.EntryEBP));

        CapacityFinalCalleeAlignedFrameLifecyclePlan Finish(
            CapacityFinalCalleeAlignedFrameLifecycleOutcome outcome,
            bool frameLoadsEntered,
            bool downstreamFrameCapturesRejected,
            CapacityFinalCalleeAlternateFrameLoadsPlan? frameLoadsPlan,
            bool derivedEbpCaptured,
            uint? derivedEbp,
            bool exactCleanExitAdmitted,
            uint stopAt) =>
            new(capture, outcome)
            {
                SizePlan = sizePlan,
                FrameLoadsPlan = frameLoadsPlan,
                FrameLoadsEntered = frameLoadsEntered,
                DownstreamFrameCapturesRejected = downstreamFrameCapturesRejected,
                DerivedEBPCaptured = derivedEbpCaptured,
                DerivedEBP = derivedEbpCaptured ? derivedEbp : null,
                UnreachedFrameEntryEDXCaptureProvided =
                    !frameLoadsEntered && capture.EntryEDX.HasValue,
                UnreachedFreshEdxPlus8CaptureProvided =
                    (!frameLoadsEntered && capture.FreshEdxPlus8.HasValue)
                    || frameLoadsPlan?.UnreachedFreshEdxPlus8CaptureProvided == true,
                UnreachedFreshEdxPlusCCaptureProvided =
                    (!frameLoadsEntered && capture.FreshEdxPlusC.HasValue)
                    || frameLoadsPlan?.UnreachedFreshEdxPlusCCaptureProvided == true,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (sizePlan.Outcome is
            CapacityFinalCalleeAlternateAlignedSizeOutcome.EntryNotValidated
            or CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX
            or CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI
            or CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEBP)
        {
            CapacityFinalCalleeAlignedFrameLifecycleOutcome outcome =
                sizePlan.Outcome switch
                {
                    CapacityFinalCalleeAlternateAlignedSizeOutcome.EntryNotValidated =>
                        CapacityFinalCalleeAlignedFrameLifecycleOutcome.EntryNotValidated,
                    CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX =>
                        CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingEntryECX,
                    CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI =>
                        CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingEntryEDI,
                    _ =>
                        CapacityFinalCalleeAlignedFrameLifecycleOutcome
                            .MissingEntryEBPForSizeStore
                };
            return Finish(
                outcome,
                frameLoadsEntered: false,
                downstreamFrameCapturesRejected: false,
                frameLoadsPlan: null,
                derivedEbpCaptured: false,
                derivedEbp: null,
                exactCleanExitAdmitted: false,
                stopAt: sizePlan.StopAtRawFileOffsetExclusive);
        }

        if (sizePlan.Outcome
            == CapacityFinalCalleeAlternateAlignedSizeOutcome
                .AtOrAboveThresholdBranchTarget)
        {
            bool downstreamCapturesProvided =
                capture.EntryEDX.HasValue
                || capture.FreshEdxPlus8.HasValue
                || capture.FreshEdxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlignedFrameLifecycleOutcome
                    .SizeAtOrAboveThresholdBranchTarget,
                frameLoadsEntered: false,
                downstreamFrameCapturesRejected: downstreamCapturesProvided,
                frameLoadsPlan: null,
                derivedEbpCaptured: false,
                derivedEbp: null,
                exactCleanExitAdmitted:
                    sizePlan.ExactCleanAdmission && !downstreamCapturesProvided,
                stopAt: sizePlan.StopAtRawFileOffsetExclusive);
        }

        bool cleanBelowThresholdHandoff =
            sizePlan.ExactCleanAdmission
            && sizePlan.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome
                    .BelowThresholdFallthrough
            && sizePlan.JnbReached
            && sizePlan.JnbTaken == false
            && sizePlan.StoreReached
            && sizePlan.Store.HasValue
            && sizePlan.CompareReached
            && sizePlan.SelectedDestinationRawFileOffset
                == BelowThresholdRawFileOffset
            && sizePlan.StopAtRawFileOffsetExclusive
                == BelowThresholdRawFileOffset
            && sizePlan.EntryECXCaptured
            && sizePlan.EntryEDICaptured
            && sizePlan.EntryEBPCaptured
            && !sizePlan.UnreachedEntryECXCaptureProvided
            && !sizePlan.UnreachedEntryEDICaptureProvided
            && !sizePlan.UnreachedEntryEBPCaptureProvided;
        if (!cleanBelowThresholdHandoff)
        {
            return Finish(
                CapacityFinalCalleeAlignedFrameLifecycleOutcome.SizeGateNotClean,
                frameLoadsEntered: false,
                downstreamFrameCapturesRejected: false,
                frameLoadsPlan: null,
                derivedEbpCaptured: false,
                derivedEbp: null,
                exactCleanExitAdmitted: false,
                stopAt: sizePlan.StopAtRawFileOffsetExclusive);
        }

        uint? derivedEbp = null;
        if (capture.EntryEDX.HasValue && capture.FreshEdxPlus8.HasValue
            && sizePlan.Store
                is CapacityFinalCalleeAlternateAlignedSizeStore sizeStore)
        {
            derivedEbp = unchecked(
                sizeStore.Address
                + CapacityFinalCalleeAlternateAlignedSizeProjection
                    .LocalStoreDisplacement);
        }

        CapacityFinalCalleeAlternateFrameLoadsPlan framePlan =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(new(
                capture.EntryEDX,
                derivedEbp,
                capture.FreshEdxPlus8,
                capture.FreshEdxPlusC));
        bool derivedEbpCaptured = framePlan.FirstWriteReached;

        CapacityFinalCalleeAlignedFrameLifecycleOutcome frameOutcome =
            framePlan.Outcome switch
            {
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEDX =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .MissingFrameEntryEDX,
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlus8 =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .MissingFreshEdxPlus8,
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEBPForFirstWrite =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .MissingDerivedEBPForFirstStore,
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .MissingFreshEdxPlusC,
                CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .CompareEqualToOpaqueCallBoundary =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .CompareEqualToOpaqueCallBoundary,
                _ =>
                    CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .CompareUnequalToBranchTarget
            };

        return Finish(
            frameOutcome,
            frameLoadsEntered: true,
            downstreamFrameCapturesRejected: false,
            frameLoadsPlan: framePlan,
            derivedEbpCaptured: derivedEbpCaptured,
            derivedEbp: derivedEbp,
            exactCleanExitAdmitted: framePlan.ExactCleanExitAdmitted,
            stopAt: framePlan.StopAtRawFileOffsetExclusive);
    }
}
