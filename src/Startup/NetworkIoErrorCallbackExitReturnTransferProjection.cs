using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackExitReturnTransferInputs(
    NetworkIoErrorCallbackExitFrameResetInputs ParentExitFrameResetInputs,
    uint? FreshReturnTargetDWORD);

public enum NetworkIoErrorCallbackExitReturnTransferOutcome
{
    ParentNotAtRETBoundary,
    ParentNotCleanAtRETBoundary,
    MissingFreshReturnTargetDWORD,
    LogicalRETTransferCompleted
}

public enum NetworkIoErrorCallbackExitReturnTransferCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkIoErrorCallbackExitReturnTransferDisposition
{
    BoundedAtParent,
    BoundedBeforeRET,
    BoundedAfterLogicalRETTransfer
}

public enum NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind
{
    ParentFrameResetEffect,
    ReturnTargetDWORDRead,
    LogicalRETTransfer
}

public readonly record struct NetworkIoErrorCallbackExitReturnTransferRead(
    uint InstructionRawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkIoErrorCallbackLogicalRETTransfer(
    uint TargetVirtualAddress,
    uint ESPBeforeRET,
    uint ESPAfterRET,
    uint ReturnAddressBytes,
    uint InstructionRawFileOffset);

public readonly record struct NetworkIoErrorCallbackExitReturnTransferOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackExitFrameResetOrderedEffect? ParentEffect,
    NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind Kind,
    NetworkIoErrorCallbackExitReturnTransferRead? Read,
    NetworkIoErrorCallbackLogicalRETTransfer? Transfer);

public sealed record NetworkIoErrorCallbackExitReturnTransferPlan(
    NetworkIoErrorCallbackExitFrameResetPlan ParentPlan,
    NetworkIoErrorCallbackExitReturnTransferInputs Capture,
    NetworkIoErrorCallbackExitReturnTransferOutcome Outcome,
    NetworkIoErrorCallbackExitReturnTransferDisposition Disposition,
    NetworkIoErrorCallbackExitReturnTransferCaptureState ReturnTargetCaptureState,
    ImmutableArray<NetworkIoErrorCallbackExitReturnTransferRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackExitReturnTransferOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkIoErrorCallbackLogicalRETTransfer? Transfer,
    uint EntryRawFileOffset,
    uint? LastReachedRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? SelectedDestinationVirtualAddress,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ReturnTransferBoundaryReached,
    bool ChildBoundedCompletionBoundaryReached,
    bool ExactChildCompletion,
    bool BoundedCompletionBoundaryReached,
    bool ParentExactBoundedCompletion,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool NativeInstructionsExecuted => false;

    public bool NativeReadsExecuted => false;

    public bool NativeRETExecuted => false;

    public bool ReturnTargetEntered => false;

    public bool ReturnTargetIsRawFileOffset => false;

    public bool NativeMemoryWritesCommitted => false;

    public bool LogicalRETStackAdjustmentProjected => Transfer.HasValue;

    public uint? ReturnTargetReadAddress => Reads.IsEmpty ? null : Reads[0].Address;

    public uint? ESPBeforeRET => Transfer?.ESPBeforeRET
        ?? (ProjectionEntered ? ParentPlan.ESP : null);

    public uint? ESPAfterRET => Transfer?.ESPAfterRET;
}

/// <summary>
/// Reprojects the callback-exit near RET from the completed frame-reset boundary.
/// The captured destination is a runtime virtual address scalar, not a file offset.
/// </summary>
public static class NetworkIoErrorCallbackExitReturnTransferProjection
{
    public const uint EntryRawFileOffset = 0x00BBC9E7;
    public const uint RETRawFileOffset = 0x00BBC9E7;
    public const byte RETOpcode = 0xC3;
    public const bool RETIsNear = true;
    public const byte RETImmediateByteCount = 0;
    public const byte RETInstructionByteLength = 1;
    public const byte DwordWidth = 4;
    public const uint ReturnAddressBytes = 4;
    public const uint StackPopIncrement = 4;
    public const int ReturnTargetStackOffset = 0;
    public const string CapturedWindowBytes = "C3";

    public static bool ExecutesNativeInstructions => false;

    public static ImmutableArray<byte> RETInstructionBytes { get; } =
        ImmutableArray.Create<byte>(RETOpcode);

    public static NetworkIoErrorCallbackExitReturnTransferPlan Project(
        NetworkIoErrorCallbackExitReturnTransferInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentExitFrameResetInputs);

        NetworkIoErrorCallbackExitFrameResetPlan parent =
            NetworkIoErrorCallbackExitFrameResetProjection.Project(
                inputs.ParentExitFrameResetInputs);
        var effects =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitReturnTransferOrderedEffect>();
        foreach (NetworkIoErrorCallbackExitFrameResetOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind.ParentFrameResetEffect,
                null,
                null));
        }

        bool parentAtBoundary =
            parent.Outcome == NetworkIoErrorCallbackExitFrameResetOutcome.PopEBPCompleted
            && parent.StopBeforeRawFileOffset
                == NetworkIoErrorCallbackExitFrameResetProjection.RetRawFileOffset
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackExitFrameResetProjection.PopEBPRawFileOffset
            && parent.ChildBoundedCompletionBoundaryReached
            && parent.ExactChildCompletion;
        bool cleanParent = parentAtBoundary
            && parent.ChildGateAdmitted
            && parent.MoveESPToEBPCompleted
            && parent.PopEBPCompleted
            && parent.CaptureStates.FreshPostCallEBP
                == NetworkIoErrorCallbackExitFrameResetCaptureState.Captured
            && parent.CaptureStates.FreshSavedEBPDWORD
                == NetworkIoErrorCallbackExitFrameResetCaptureState.Captured
            && parent.Reads.All(read => read.Captured)
            && parent.InstructionOrder.All(instruction => instruction.Completed)
            && parent.EBP.HasValue
            && parent.RestoredEBP.HasValue
            && parent.EBX.HasValue
            && parent.ESP.HasValue
            && parent.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.FreshSavedEBPDWORD
            && parent.FlagsAtStop is null
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && !parent.NativeInstructionsExecuted
            && !parent.NativeReadsExecuted
            && !parent.MemoryWriteExecuted
            && !parent.NativeCallExecuted
            && !parent.NextInstructionExecuted;

        NetworkIoErrorCallbackExitReturnTransferCaptureState captureState =
            !cleanParent
                ? BeforeAdmission(inputs.FreshReturnTargetDWORD.HasValue)
                : inputs.FreshReturnTargetDWORD.HasValue
                    ? NetworkIoErrorCallbackExitReturnTransferCaptureState.Captured
                    : NetworkIoErrorCallbackExitReturnTransferCaptureState.Missing;
        uint entry = cleanParent
            ? EntryRawFileOffset
            : parent.StopBeforeRawFileOffset ?? EntryRawFileOffset;
        uint? stopAt = parent.StopBeforeRawFileOffset;
        uint? lastReached = parent.LastReachedRawFileOffset;
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitReturnTransferRead>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        NetworkIoErrorCallbackExitReturnTransferOutcome outcome;
        NetworkIoErrorCallbackExitReturnTransferDisposition disposition;
        NetworkIoErrorCallbackLogicalRETTransfer? transfer = null;
        uint? selectedDestination = null;
        bool transferBoundaryReached = false;

        if (!parentAtBoundary)
        {
            outcome =
                NetworkIoErrorCallbackExitReturnTransferOutcome.ParentNotAtRETBoundary;
            disposition =
                NetworkIoErrorCallbackExitReturnTransferDisposition.BoundedAtParent;
        }
        else if (!cleanParent)
        {
            outcome =
                NetworkIoErrorCallbackExitReturnTransferOutcome.ParentNotCleanAtRETBoundary;
            disposition =
                NetworkIoErrorCallbackExitReturnTransferDisposition.BoundedAtParent;
        }
        else
        {
            uint stackAddress = parent.ESP!.Value;
            uint? target = inputs.FreshReturnTargetDWORD;
            var read = new NetworkIoErrorCallbackExitReturnTransferRead(
                RETRawFileOffset,
                stackAddress,
                DwordWidth,
                Attempted: true,
                Captured: target.HasValue,
                Value: target,
                Committed: false);
            reads.Add(read);
            effects.Add(new(
                effects.Count,
                null,
                NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind.ReturnTargetDWORDRead,
                read,
                null));

            if (target is not uint capturedTarget)
            {
                captureState =
                    NetworkIoErrorCallbackExitReturnTransferCaptureState.Missing;
                outcome =
                    NetworkIoErrorCallbackExitReturnTransferOutcome
                        .MissingFreshReturnTargetDWORD;
                disposition =
                    NetworkIoErrorCallbackExitReturnTransferDisposition.BoundedBeforeRET;
                stopAt = RETRawFileOffset;
            }
            else
            {
                transfer = new(
                    capturedTarget,
                    stackAddress,
                    unchecked(stackAddress + ReturnAddressBytes),
                    ReturnAddressBytes,
                    RETRawFileOffset);
                selectedDestination = capturedTarget;
                reached.Add(RETRawFileOffset);
                lastReached = RETRawFileOffset;
                stopAt = null;
                transferBoundaryReached = true;
                outcome =
                    NetworkIoErrorCallbackExitReturnTransferOutcome
                        .LogicalRETTransferCompleted;
                disposition =
                    NetworkIoErrorCallbackExitReturnTransferDisposition
                        .BoundedAfterLogicalRETTransfer;
                effects.Add(new(
                    effects.Count,
                    null,
                    NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind
                        .LogicalRETTransfer,
                    null,
                    transfer));
            }
        }

        ImmutableArray<NetworkIoErrorCallbackExitReturnTransferOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        bool boundedCompletionBoundaryReached = cleanParent
            ? transferBoundaryReached
            : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion =
            transferBoundaryReached
            || (!cleanParent
                && parent.ExactBoundedCompletion
                && stopAt == parent.StopBeforeRawFileOffset);
        bool exactCleanCompletion =
            transferBoundaryReached
            && cleanParent
            && captureState
                == NetworkIoErrorCallbackExitReturnTransferCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            disposition,
            captureState,
            reads.ToImmutable(),
            ordered,
            reached.ToImmutable(),
            transfer,
            entry,
            lastReached,
            stopAt,
            selectedDestination,
            parent.FlagsAtStop,
            transferBoundaryReached,
            transferBoundaryReached,
            transferBoundaryReached,
            boundedCompletionBoundaryReached,
            parent.ExactBoundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            cleanParent);
    }

    private static NetworkIoErrorCallbackExitReturnTransferCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackExitReturnTransferCaptureState.SuppliedUnused
            : NetworkIoErrorCallbackExitReturnTransferCaptureState.Unreached;
}
