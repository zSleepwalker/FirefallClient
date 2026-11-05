using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherReturnTransferCapture(
    NetworkDispatcherFrameResetCapture ParentCapture,
    uint? FreshReturnTargetDWORD);

public enum NetworkDispatcherReturnTransferCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherReturnTransferOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshReturnTargetDWORD,
    BoundedAfterRET4Transfer
}

public enum NetworkDispatcherReturnTransferDisposition
{
    BoundedAtParent,
    BoundedBeforeRET4,
    BoundedAfterRET4Transfer
}

public enum NetworkDispatcherReturnTransferOrderedEffectKind
{
    ParentFrameResetEffect,
    ReturnTargetDWORDRead,
    LogicalRET4Transfer
}

public readonly record struct NetworkDispatcherReturnTransferRead(
    uint InstructionRawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherLogicalRET4Transfer(
    uint TargetVirtualAddress,
    uint ESPBeforeRET,
    uint ESPAfterRET,
    uint ReturnAddressBytes,
    uint ArgumentBytes,
    uint InstructionRawFileOffset);

public readonly record struct NetworkDispatcherReturnTransferOrderedEffect(
    int Sequence,
    NetworkDispatcherFrameResetOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherReturnTransferOrderedEffectKind Kind,
    NetworkDispatcherReturnTransferRead? Read,
    NetworkDispatcherLogicalRET4Transfer? Transfer);

public sealed record NetworkDispatcherReturnTransferPlan(
    NetworkDispatcherFrameResetPlan ParentPlan,
    NetworkDispatcherReturnTransferCapture Capture,
    NetworkDispatcherReturnTransferOutcome Outcome,
    NetworkDispatcherReturnTransferDisposition Disposition,
    NetworkDispatcherReturnTransferCaptureState ReturnTargetCaptureState,
    ImmutableArray<NetworkDispatcherReturnTransferRead> Reads,
    ImmutableArray<NetworkDispatcherReturnTransferOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkDispatcherLogicalRET4Transfer? Transfer,
    uint EntryRawFileOffset,
    uint? LastReachedRawFileOffset,
    uint? StopAtRawFileOffsetExclusive,
    uint? SelectedDestinationVirtualAddress,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    bool ReturnTransferBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool ProjectionEntered)
{
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesCommitted => false;
    public bool NativeRET4Executed => false;
    public bool ReturnTargetEntered => false;
    public bool ReturnTargetIsRawFileOffset => false;
    public bool EAXReturnValueInferred => false;
    public bool LogicalRET4StackAdjustmentProjected => Transfer.HasValue;
    public uint? ReturnTargetReadAddress => Reads.IsEmpty ? null : Reads[0].Address;
    public uint? ESPBeforeRET => Transfer?.ESPBeforeRET
        ?? (ProjectionEntered ? ParentPlan.ESPAtStop : null);
    public uint? ESPAfterRET => Transfer?.ESPAfterRET;
}

/// <summary>
/// Reprojects the dispatcher RET 4 transfer from the frame-reset boundary.
/// The selected destination remains a runtime virtual address, not a file offset.
/// </summary>
public static class NetworkDispatcherReturnTransferProjection
{
    public const uint EntryRawFileOffset = 0x00BBDEC7;
    public const uint RET4RawFileOffset = 0x00BBDEC7;
    public const byte DwordWidth = 4;
    public const uint ReturnAddressBytes = 4;
    public const uint ArgumentBytes = 4;
    public const uint ESPIncrement = 4;
    public const int ReturnTargetStackOffset = 0;
    public const int RET4InstructionByteLength = 3;
    public const string CapturedWindowBytes = "C20400";

    public static bool ExecutesNativeInstructions => false;

    public static ImmutableArray<byte> RET4InstructionBytes { get; } =
        ImmutableArray.Create<byte>(0xC2, 0x04, 0x00);

    public static NetworkDispatcherReturnTransferPlan Project(
        NetworkDispatcherReturnTransferCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherFrameResetPlan parent =
            NetworkDispatcherFrameResetProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherReturnTransferOrderedEffect>();
        foreach (NetworkDispatcherFrameResetOrderedEffect parentEffect in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherReturnTransferOrderedEffectKind.ParentFrameResetEffect,
                null,
                null));
        }

        bool admitted =
            parent.ExactCleanCompletion
            && parent.BoundedCompletionBoundaryReached
            && parent.Outcome == NetworkDispatcherFrameResetOutcome.BoundedBeforeRET4
            && parent.StopAtRawFileOffsetExclusive == RET4RawFileOffset
            && parent.ESPAtStop.HasValue
            && parent.FreshSavedEBPDWORDState == NetworkDispatcherFrameResetCaptureState.Captured
            && parent.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !parent.NativeInstructionsExecuted
            && !parent.RET4InstructionEntered
            && !parent.RET4StackAdjustmentProjected;

        NetworkDispatcherReturnTransferCaptureState captureState =
            admitted
                ? State(capture.FreshReturnTargetDWORD)
                : capture.FreshReturnTargetDWORD.HasValue
                    ? NetworkDispatcherReturnTransferCaptureState.SuppliedUnused
                    : NetworkDispatcherReturnTransferCaptureState.Unreached;
        uint entry = admitted ? EntryRawFileOffset : parent.StopAtRawFileOffsetExclusive;
        uint? stopAt = parent.StopAtRawFileOffsetExclusive;
        uint? lastReached = parent.ReachedInstructionRawFileOffsets
            .Select(offset => (uint?)offset).LastOrDefault();
        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherReturnTransferRead>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        NetworkDispatcherReturnTransferOutcome outcome;
        NetworkDispatcherReturnTransferDisposition disposition;
        NetworkDispatcherLogicalRET4Transfer? transfer = null;
        uint? selectedDestination = null;
        bool transferBoundaryReached = false;

        if (!admitted)
        {
            outcome = NetworkDispatcherReturnTransferOutcome.ParentHandoffNotAdmitted;
            disposition = NetworkDispatcherReturnTransferDisposition.BoundedAtParent;
        }
        else
        {
            uint stackAddress = parent.ESPAtStop!.Value;
            uint? target = capture.FreshReturnTargetDWORD;
            var read = new NetworkDispatcherReturnTransferRead(
                RET4RawFileOffset,
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
                RET4RawFileOffset,
                NetworkDispatcherReturnTransferOrderedEffectKind.ReturnTargetDWORDRead,
                read,
                null));

            if (target is not uint capturedTarget)
            {
                captureState = NetworkDispatcherReturnTransferCaptureState.Missing;
                outcome = NetworkDispatcherReturnTransferOutcome.MissingFreshReturnTargetDWORD;
                disposition = NetworkDispatcherReturnTransferDisposition.BoundedBeforeRET4;
                stopAt = RET4RawFileOffset;
            }
            else
            {
                transfer = new(
                    capturedTarget,
                    stackAddress,
                    unchecked(stackAddress + ReturnAddressBytes + ArgumentBytes),
                    ReturnAddressBytes,
                    ArgumentBytes,
                    RET4RawFileOffset);
                selectedDestination = capturedTarget;
                reached.Add(RET4RawFileOffset);
                lastReached = RET4RawFileOffset;
                stopAt = null;
                transferBoundaryReached = true;
                outcome = NetworkDispatcherReturnTransferOutcome.BoundedAfterRET4Transfer;
                disposition = NetworkDispatcherReturnTransferDisposition.BoundedAfterRET4Transfer;
                effects.Add(new(
                    effects.Count,
                    null,
                    RET4RawFileOffset,
                    NetworkDispatcherReturnTransferOrderedEffectKind.LogicalRET4Transfer,
                    null,
                    transfer));
            }
        }

        ImmutableArray<NetworkDispatcherReturnTransferOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        bool exactBoundedCompletion =
            transferBoundaryReached
            || (!admitted
                && parent.ExactBoundedCompletion
                && stopAt == parent.StopAtRawFileOffsetExclusive);
        bool exactCleanCompletion =
            transferBoundaryReached
            && parent.ExactCleanCompletion
            && captureState == NetworkDispatcherReturnTransferCaptureState.Captured;

        return new(
            parent,
            capture,
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
            exactBoundedCompletion,
            exactCleanCompletion,
            admitted);
    }

    private static NetworkDispatcherReturnTransferCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherReturnTransferCaptureState.Captured
            : NetworkDispatcherReturnTransferCaptureState.Missing;
}
