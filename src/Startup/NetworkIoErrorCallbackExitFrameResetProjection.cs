using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackExitFrameResetInputs(
    NetworkIoErrorCallbackExitEbxRestoreInputs ParentExitEbxRestoreInputs,
    uint? FreshPostCallEBP,
    uint? FreshSavedEBPDWORD);

public enum NetworkIoErrorCallbackExitFrameResetOutcome
{
    ParentNotAtMoveESPToEBPBoundary,
    ParentNotCleanAtMoveESPToEBPBoundary,
    MissingFreshPostCallEBP,
    MissingFreshSavedEBPDWORD,
    PopEBPCompleted
}

public enum NetworkIoErrorCallbackExitFrameResetCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackExitFrameResetCaptureStates(
    NetworkIoErrorCallbackExitFrameResetCaptureState FreshPostCallEBP,
    NetworkIoErrorCallbackExitFrameResetCaptureState FreshSavedEBPDWORD);

public enum NetworkIoErrorCallbackExitFrameResetEBPSource
{
    Unknown,
    FreshPostCallEBP,
    FreshSavedEBPDWORD
}

public readonly record struct NetworkIoErrorCallbackExitFrameResetRead(
    uint RawFileOffset,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public enum NetworkIoErrorCallbackExitFrameResetInstructionKind
{
    MoveESPFromEBP,
    PopEBP
}

public readonly record struct NetworkIoErrorCallbackExitFrameResetInstruction(
    NetworkIoErrorCallbackExitFrameResetInstructionKind Kind,
    uint RawFileOffset,
    uint? ESPBefore,
    uint? ESPAfter,
    uint? EBPBefore,
    uint? EBPAfter,
    uint? Value,
    bool Completed);

public readonly record struct NetworkIoErrorCallbackExitFrameResetOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackExitEbxRestoreOrderedEffect? ParentExitEbxRestoreEffect,
    NetworkIoErrorCallbackExitFrameResetRead? Read,
    NetworkIoErrorCallbackExitFrameResetInstruction? Instruction);

public sealed record NetworkIoErrorCallbackExitFrameResetPlan(
    NetworkIoErrorCallbackExitEbxRestorePlan ParentExitEbxRestorePlan,
    NetworkIoErrorCallbackExitFrameResetInputs Capture,
    NetworkIoErrorCallbackExitFrameResetOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackExitFrameResetRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackExitFrameResetInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackExitFrameResetOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackExitFrameResetCaptureStates CaptureStates,
    uint? EBP,
    NetworkIoErrorCallbackExitFrameResetEBPSource EBPSourceAtStop,
    uint? RestoredEBP,
    uint? EBX,
    uint? ESP,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool MoveESPToEBPCompleted,
    bool PopEBPCompleted,
    bool ChildBoundedCompletionBoundaryReached,
    bool ExactChildCompletion,
    bool BoundedCompletionBoundaryReached,
    bool ParentExactBoundedCompletion,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset)
{
    public bool NativeInstructionsExecuted => false;

    public bool NativeReadsExecuted => false;

    public bool MemoryWriteExecuted => false;

    public bool NativeCallExecuted => false;

    public bool NextInstructionExecuted => false;
}

/// <summary>
/// Reprojects the clean POP EBX boundary through MOV ESP, EBP and POP EBP,
/// stopping before RET.
/// </summary>
public static class NetworkIoErrorCallbackExitFrameResetProjection
{
    public const uint MoveESPToEBPRawFileOffset = 0x00BBC9E4;
    public const uint PopEBPRawFileOffset = 0x00BBC9E6;
    public const uint RetRawFileOffset = 0x00BBC9E7;
    public const byte MoveESPToEBPOpcode = 0x8B;
    public const byte PopEBPOpcode = 0x5D;
    public const byte DwordWidth = 4;
    public const uint StackPopIncrement = 4;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool ExecutesNextInstruction => false;

    public static bool ExecutesCalls => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(0x8B, 0xE5, 0x5D, 0xC3);

    public static NetworkIoErrorCallbackExitFrameResetPlan Project(
        NetworkIoErrorCallbackExitFrameResetInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentExitEbxRestoreInputs);

        NetworkIoErrorCallbackExitEbxRestorePlan parent =
            NetworkIoErrorCallbackExitEbxRestoreProjection.Project(
                inputs.ParentExitEbxRestoreInputs);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitFrameResetRead>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitFrameResetInstruction>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitFrameResetOrderedEffect>();

        foreach (NetworkIoErrorCallbackExitEbxRestoreOrderedEffect effect
            in parent.OrderedEffects)
        {
            ordered.Add(new(ordered.Count, effect, null, null));
        }

        bool parentAtBoundary =
            parent.Outcome == NetworkIoErrorCallbackExitEbxRestoreOutcome.PopEBXCompleted
            && parent.StopBeforeRawFileOffset == MoveESPToEBPRawFileOffset
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackExitEbxRestoreProjection.PopEBXRawFileOffset
            && parent.ChildBoundedCompletionBoundaryReached
            && parent.ExactChildCompletion;
        bool cleanParent = parentAtBoundary
            && parent.ChildGateAdmitted
            && parent.PopEBXCompleted
            && parent.CaptureStates.FreshPostCallESP
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured
            && parent.CaptureStates.FreshSavedEBXDWORD
                == NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.FlagsAtStop is null;

        NetworkIoErrorCallbackExitFrameResetCaptureState ebpState =
            BeforeAdmission(inputs.FreshPostCallEBP.HasValue);
        NetworkIoErrorCallbackExitFrameResetCaptureState savedEbpState =
            BeforeAdmission(inputs.FreshSavedEBPDWORD.HasValue);
        NetworkIoErrorCallbackExitFrameResetOutcome outcome;
        uint? ebp = null;
        uint? restoredEBP = null;
        NetworkIoErrorCallbackExitFrameResetEBPSource ebpSource =
            NetworkIoErrorCallbackExitFrameResetEBPSource.Unknown;
        uint? ebx = parent.EBX;
        uint? esp = parent.ESP;
        NetworkIoErrorCallbackLoopGateFlags? flags = parent.FlagsAtStop;
        bool moveCompleted = false;
        bool popCompleted = false;
        bool childComplete = false;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!parentAtBoundary)
        {
            outcome =
                NetworkIoErrorCallbackExitFrameResetOutcome
                    .ParentNotAtMoveESPToEBPBoundary;
        }
        else if (!cleanParent)
        {
            outcome =
                NetworkIoErrorCallbackExitFrameResetOutcome
                    .ParentNotCleanAtMoveESPToEBPBoundary;
        }
        else
        {
            ebpState = inputs.FreshPostCallEBP.HasValue
                ? NetworkIoErrorCallbackExitFrameResetCaptureState.Captured
                : NetworkIoErrorCallbackExitFrameResetCaptureState.Missing;
            lastReached = MoveESPToEBPRawFileOffset;
            stopBefore = MoveESPToEBPRawFileOffset;

            if (inputs.FreshPostCallEBP is not uint freshEBP)
            {
                savedEbpState = UnreachedOrUnused(inputs.FreshSavedEBPDWORD.HasValue);
                outcome =
                    NetworkIoErrorCallbackExitFrameResetOutcome.MissingFreshPostCallEBP;
            }
            else
            {
                uint? parentESP = esp;
                esp = freshEBP;
                ebp = freshEBP;
                ebpSource =
                    NetworkIoErrorCallbackExitFrameResetEBPSource.FreshPostCallEBP;
                moveCompleted = true;
                lastReached = MoveESPToEBPRawFileOffset;
                stopBefore = PopEBPRawFileOffset;
                var move = new NetworkIoErrorCallbackExitFrameResetInstruction(
                    NetworkIoErrorCallbackExitFrameResetInstructionKind.MoveESPFromEBP,
                    MoveESPToEBPRawFileOffset,
                    ESPBefore: parentESP,
                    ESPAfter: freshEBP,
                    EBPBefore: freshEBP,
                    EBPAfter: freshEBP,
                    Value: freshEBP,
                    Completed: true);
                instructions.Add(move);
                ordered.Add(new(ordered.Count, null, null, move));

                savedEbpState = inputs.FreshSavedEBPDWORD.HasValue
                    ? NetworkIoErrorCallbackExitFrameResetCaptureState.Captured
                    : NetworkIoErrorCallbackExitFrameResetCaptureState.Missing;
                var read = new NetworkIoErrorCallbackExitFrameResetRead(
                    PopEBPRawFileOffset,
                    freshEBP,
                    DwordWidth,
                    Attempted: true,
                    Captured: inputs.FreshSavedEBPDWORD.HasValue,
                    Value: inputs.FreshSavedEBPDWORD,
                    NativeReadExecuted: false);
                reads.Add(read);

                NetworkIoErrorCallbackExitFrameResetInstruction pop = new(
                    NetworkIoErrorCallbackExitFrameResetInstructionKind.PopEBP,
                    PopEBPRawFileOffset,
                    ESPBefore: freshEBP,
                    ESPAfter: inputs.FreshSavedEBPDWORD.HasValue
                        ? unchecked(freshEBP + StackPopIncrement)
                        : freshEBP,
                    EBPBefore: freshEBP,
                    EBPAfter: inputs.FreshSavedEBPDWORD ?? freshEBP,
                    Value: inputs.FreshSavedEBPDWORD,
                    Completed: inputs.FreshSavedEBPDWORD.HasValue);
                instructions.Add(pop);
                ordered.Add(new(ordered.Count, null, read, pop));
                lastReached = PopEBPRawFileOffset;

                if (!inputs.FreshSavedEBPDWORD.HasValue)
                {
                    outcome =
                        NetworkIoErrorCallbackExitFrameResetOutcome
                            .MissingFreshSavedEBPDWORD;
                    stopBefore = PopEBPRawFileOffset;
                }
                else
                {
                    ebp = inputs.FreshSavedEBPDWORD.Value;
                    restoredEBP = inputs.FreshSavedEBPDWORD.Value;
                    ebpSource =
                        NetworkIoErrorCallbackExitFrameResetEBPSource
                            .FreshSavedEBPDWORD;
                    esp = unchecked(freshEBP + StackPopIncrement);
                    flags = parent.FlagsAtStop;
                    popCompleted = true;
                    childComplete = true;
                    outcome =
                        NetworkIoErrorCallbackExitFrameResetOutcome.PopEBPCompleted;
                    stopBefore = RetRawFileOffset;
                }
            }
        }

        if (!cleanParent && inputs.FreshPostCallEBP.HasValue)
        {
            ebpState =
                NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused;
        }
        if (!cleanParent && inputs.FreshSavedEBPDWORD.HasValue)
        {
            savedEbpState =
                NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused;
        }

        bool boundedCompletion = cleanParent
            ? childComplete
            : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion = cleanParent
            ? childComplete
            : parent.ExactBoundedCompletion;
        bool exactCleanCompletion = cleanParent
            && childComplete
            && moveCompleted
            && popCompleted
            && ebpState == NetworkIoErrorCallbackExitFrameResetCaptureState.Captured
            && savedEbpState
                == NetworkIoErrorCallbackExitFrameResetCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            instructions.ToImmutable(),
            ordered.ToImmutable(),
            new(ebpState, savedEbpState),
            ebp,
            ebpSource,
            restoredEBP,
            ebx,
            esp,
            flags,
            cleanParent,
            moveCompleted,
            popCompleted,
            childComplete,
            childComplete,
            boundedCompletion,
            parent.ExactBoundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackExitFrameResetCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackExitFrameResetCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackExitFrameResetCaptureState.Unreached;

    private static NetworkIoErrorCallbackExitFrameResetCaptureState UnreachedOrUnused(
        bool supplied) => BeforeAdmission(supplied);
}
