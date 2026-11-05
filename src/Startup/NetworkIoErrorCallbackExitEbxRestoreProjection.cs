using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackExitEbxRestoreInputs(
    NetworkIoErrorCallbackExitCallReturnInputs ParentExitCallReturnInputs,
    uint? FreshPostCallESP,
    uint? FreshSavedEBXDWORD);

public enum NetworkIoErrorCallbackExitEbxRestoreOutcome
{
    ParentNotAtPopEBXBoundary,
    ParentNotCleanAtPopEBXBoundary,
    MissingFreshPostCallESP,
    MissingFreshSavedEBXDWORD,
    PopEBXCompleted
}

public enum NetworkIoErrorCallbackExitEbxRestoreCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackExitEbxRestoreCaptureStates(
    NetworkIoErrorCallbackExitEbxRestoreCaptureState FreshPostCallESP,
    NetworkIoErrorCallbackExitEbxRestoreCaptureState FreshSavedEBXDWORD);

public readonly record struct NetworkIoErrorCallbackExitEbxRestoreRead(
    uint RawFileOffset,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public readonly record struct NetworkIoErrorCallbackExitEbxRestoreInstruction(
    uint RawFileOffset,
    uint? ESPBefore,
    uint? ESPAfter,
    uint? Value,
    bool Completed);

public readonly record struct NetworkIoErrorCallbackExitEbxRestoreOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackExitCallReturnOrderedEffect? ParentExitCallReturnEffect,
    NetworkIoErrorCallbackExitEbxRestoreRead? Read,
    NetworkIoErrorCallbackExitEbxRestoreInstruction? Instruction);

public sealed record NetworkIoErrorCallbackExitEbxRestorePlan(
    NetworkIoErrorCallbackExitCallReturnPlan ParentExitCallReturnPlan,
    NetworkIoErrorCallbackExitEbxRestoreInputs Capture,
    NetworkIoErrorCallbackExitEbxRestoreOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackExitEbxRestoreRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackExitEbxRestoreInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackExitEbxRestoreOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackExitEbxRestoreCaptureStates CaptureStates,
    uint? PostCallEBP,
    uint? EBX,
    uint? ESP,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool PopEBXCompleted,
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
/// Reprojects the clean normal-return boundary through POP EBX and stops
/// before the following MOV ESP, EBP instruction.
/// </summary>
public static class NetworkIoErrorCallbackExitEbxRestoreProjection
{
    public const uint PopEBXRawFileOffset = 0x00BBC9E3;
    public const uint NextInstructionRawFileOffset = 0x00BBC9E4;
    public const byte PopEBXOpcode = 0x5B;
    public const byte DwordWidth = 4;
    public const uint StackPopIncrement = 4;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool ExecutesNextInstruction => false;

    public static bool ExecutesCalls => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(0x5B, 0x8B, 0xE5, 0x5D, 0xC3);

    public static NetworkIoErrorCallbackExitEbxRestorePlan Project(
        NetworkIoErrorCallbackExitEbxRestoreInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentExitCallReturnInputs);

        NetworkIoErrorCallbackExitCallReturnPlan parent =
            NetworkIoErrorCallbackExitCallReturnProjection.Project(
                inputs.ParentExitCallReturnInputs);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitEbxRestoreRead>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitEbxRestoreInstruction>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitEbxRestoreOrderedEffect>();

        foreach (NetworkIoErrorCallbackExitCallReturnOrderedEffect effect
            in parent.OrderedEffects)
        {
            ordered.Add(new(ordered.Count, effect, null, null));
        }

        bool parentAtBoundary =
            parent.Outcome
                == NetworkIoErrorCallbackExitCallReturnOutcome.OpaqueCallReturnedNormally
            && parent.ContinuationBoundary is
            {
                RawFileOffset: PopEBXRawFileOffset,
                NextInstructionByte: PopEBXOpcode,
                Reached: true,
                NextInstructionExecuted: false
            }
            && parent.StopBeforeRawFileOffset == PopEBXRawFileOffset
            && parent.ChildBoundedCompletionBoundaryReached
            && parent.ExactChildCompletion;
        bool cleanParent = parentAtBoundary
            && parent.ChildGateAdmitted
            && parent.TargetReadCaptured
            && parent.LogicalCallAttempted
            && parent.CallReturnObservationCaptured
            && parent.OpaqueCallReturnedNormally == true
            && parent.CaptureStates.FreshOpaqueCallTargetDWORD
                == NetworkIoErrorCallbackExitCallReturnCaptureState.Captured
            && parent.CaptureStates.OpaqueCallReturnedNormally
                == NetworkIoErrorCallbackExitCallReturnCaptureState.Captured
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.FlagsAtStop is null;

        NetworkIoErrorCallbackExitEbxRestoreCaptureState espState =
            BeforeAdmission(inputs.FreshPostCallESP.HasValue);
        NetworkIoErrorCallbackExitEbxRestoreCaptureState ebxState =
            BeforeAdmission(inputs.FreshSavedEBXDWORD.HasValue);
        NetworkIoErrorCallbackExitEbxRestoreOutcome outcome;
        uint? ebx = null;
        uint? esp = null;
        NetworkIoErrorCallbackLoopGateFlags? flags = parent.FlagsAtStop;
        bool popCompleted = false;
        bool childComplete = false;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!parentAtBoundary)
        {
            outcome =
                NetworkIoErrorCallbackExitEbxRestoreOutcome.ParentNotAtPopEBXBoundary;
        }
        else if (!cleanParent)
        {
            outcome =
                NetworkIoErrorCallbackExitEbxRestoreOutcome.ParentNotCleanAtPopEBXBoundary;
        }
        else
        {
            espState = inputs.FreshPostCallESP.HasValue
                ? NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured
                : NetworkIoErrorCallbackExitEbxRestoreCaptureState.Missing;
            lastReached = PopEBXRawFileOffset;
            stopBefore = PopEBXRawFileOffset;

            if (inputs.FreshPostCallESP is not uint entryESP)
            {
                ebxState = UnreachedOrUnused(inputs.FreshSavedEBXDWORD.HasValue);
                outcome =
                    NetworkIoErrorCallbackExitEbxRestoreOutcome.MissingFreshPostCallESP;
            }
            else
            {
                esp = entryESP;
                ebxState = inputs.FreshSavedEBXDWORD.HasValue
                    ? NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured
                    : NetworkIoErrorCallbackExitEbxRestoreCaptureState.Missing;
                var read = new NetworkIoErrorCallbackExitEbxRestoreRead(
                    PopEBXRawFileOffset,
                    entryESP,
                    DwordWidth,
                    Attempted: true,
                    Captured: inputs.FreshSavedEBXDWORD.HasValue,
                    Value: inputs.FreshSavedEBXDWORD,
                    NativeReadExecuted: false);
                reads.Add(read);

                NetworkIoErrorCallbackExitEbxRestoreInstruction instruction = new(
                    PopEBXRawFileOffset,
                    ESPBefore: entryESP,
                    ESPAfter: inputs.FreshSavedEBXDWORD.HasValue
                        ? unchecked(entryESP + StackPopIncrement)
                        : entryESP,
                    Value: inputs.FreshSavedEBXDWORD,
                    Completed: inputs.FreshSavedEBXDWORD.HasValue);
                instructions.Add(instruction);
                ordered.Add(new(ordered.Count, null, read, instruction));

                if (!inputs.FreshSavedEBXDWORD.HasValue)
                {
                    outcome =
                        NetworkIoErrorCallbackExitEbxRestoreOutcome
                            .MissingFreshSavedEBXDWORD;
                }
                else
                {
                    ebx = inputs.FreshSavedEBXDWORD.Value;
                    esp = unchecked(entryESP + StackPopIncrement);
                    flags = parent.FlagsAtStop;
                    popCompleted = true;
                    childComplete = true;
                    outcome = NetworkIoErrorCallbackExitEbxRestoreOutcome.PopEBXCompleted;
                    stopBefore = NextInstructionRawFileOffset;
                }
            }
        }

        if (!cleanParent && inputs.FreshPostCallESP.HasValue)
        {
            espState =
                NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused;
        }
        if (!cleanParent && inputs.FreshSavedEBXDWORD.HasValue)
        {
            ebxState =
                NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused;
        }

        bool boundedCompletion = cleanParent
            ? childComplete
            : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion = cleanParent
            ? childComplete
            : parent.ExactBoundedCompletion;
        bool exactCleanCompletion = cleanParent
            && childComplete
            && popCompleted
            && espState == NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured
            && ebxState == NetworkIoErrorCallbackExitEbxRestoreCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            instructions.ToImmutable(),
            ordered.ToImmutable(),
            new(espState, ebxState),
            null,
            ebx,
            esp,
            flags,
            cleanParent,
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

    private static NetworkIoErrorCallbackExitEbxRestoreCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackExitEbxRestoreCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackExitEbxRestoreCaptureState.Unreached;

    private static NetworkIoErrorCallbackExitEbxRestoreCaptureState UnreachedOrUnused(
        bool supplied) => BeforeAdmission(supplied);
}
