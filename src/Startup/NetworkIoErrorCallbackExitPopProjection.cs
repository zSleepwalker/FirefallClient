using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackExitPopInputs(
    NetworkIoErrorCallbackPostLoopByteGateInputs ParentPostLoopByteGateInputs,
    uint? FreshESP,
    uint? FreshPopEDIDWORD,
    uint? FreshPopESIDWORD);

public enum NetworkIoErrorCallbackExitPopOutcome
{
    ParentNotAtCleanFallThrough9DB,
    MissingFreshESP,
    MissingFreshPopEDIDWORD,
    MissingFreshPopESIDWORD,
    OpaqueCallCellBoundary
}

public enum NetworkIoErrorCallbackExitPopCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackExitPopCaptureStates(
    NetworkIoErrorCallbackExitPopCaptureState FreshESP,
    NetworkIoErrorCallbackExitPopCaptureState FreshPopEDIDWORD,
    NetworkIoErrorCallbackExitPopCaptureState FreshPopESIDWORD);

public enum NetworkIoErrorCallbackExitPopReadKind
{
    FreshPopEDIDWORD,
    FreshPopESIDWORD
}

public readonly record struct NetworkIoErrorCallbackExitPopRead(
    uint RawFileOffset,
    NetworkIoErrorCallbackExitPopReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public enum NetworkIoErrorCallbackExitPopInstructionKind
{
    PopEDI,
    PopESI
}

public readonly record struct NetworkIoErrorCallbackExitPopInstruction(
    NetworkIoErrorCallbackExitPopInstructionKind Kind,
    uint RawFileOffset,
    uint? ESPBefore,
    uint? ESPAfter,
    uint? Value,
    bool Completed);

public readonly record struct NetworkIoErrorCallbackExitPopCallCellBoundary(
    uint RawFileOffset,
    uint CallCellVirtualAddress,
    bool BoundaryReached,
    bool TargetReadAttempted,
    bool NativeCallExecuted);

public readonly record struct NetworkIoErrorCallbackExitPopOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackPostLoopByteGateOrderedEffect? ParentPostLoopEffect,
    NetworkIoErrorCallbackExitPopRead? Read,
    NetworkIoErrorCallbackExitPopInstruction? Instruction,
    NetworkIoErrorCallbackExitPopCallCellBoundary? CallCellBoundary);

public sealed record NetworkIoErrorCallbackExitPopPlan(
    NetworkIoErrorCallbackPostLoopByteGatePlan ParentPostLoopByteGatePlan,
    NetworkIoErrorCallbackExitPopInputs Capture,
    NetworkIoErrorCallbackExitPopOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackExitPopRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackExitPopInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackExitPopOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackExitPopCaptureStates CaptureStates,
    uint? EBP,
    uint? EAX,
    uint? EDI,
    uint? ESI,
    uint? ESP,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    NetworkIoErrorCallbackExitPopCallCellBoundary? CallCellBoundary,
    bool ChildGateAdmitted,
    bool PopEDICompleted,
    bool PopESICompleted,
    bool ChildBoundedCompletionBoundaryReached,
    bool ExactChildCompletion,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    bool NativeInstructionsExecuted,
    bool NativeReadsExecuted,
    bool MemoryWriteExecuted,
    bool NativeCallExecuted,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset);

/// <summary>
/// Reprojects the clean 9DB fallthrough through two modeled stack pops and
/// stops at the opaque absolute call-cell boundary without dereferencing it.
/// </summary>
public static class NetworkIoErrorCallbackExitPopProjection
{
    public const uint EntryRawFileOffset =
        NetworkIoErrorCallbackPostLoopByteGateProjection.FallThroughRawFileOffset;
    public const uint PopEDIRawFileOffset = 0x00BBC9DB;
    public const uint PopESIRawFileOffset = 0x00BBC9DC;
    public const uint OpaqueCallCellRawFileOffset = 0x00BBC9DD;
    public const uint OpaqueCallCellVirtualAddress = 0x01DF9760;
    public const uint DwordWidth = 4;
    public const uint StackPopIncrement = 4;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool ExecutesCalls => false;

    public static bool EntersOpaqueCallCell => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x5F, 0x5E,
            0xFF, 0x15, 0x60, 0x97, 0xDF, 0x01);

    public static NetworkIoErrorCallbackExitPopPlan Project(
        NetworkIoErrorCallbackExitPopInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentPostLoopByteGateInputs);

        NetworkIoErrorCallbackPostLoopByteGatePlan parent =
            NetworkIoErrorCallbackPostLoopByteGateProjection.Project(
                inputs.ParentPostLoopByteGateInputs);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitPopRead>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitPopInstruction>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitPopOrderedEffect>();

        foreach (NetworkIoErrorCallbackPostLoopByteGateOrderedEffect effect
            in parent.OrderedEffects)
        {
            ordered.Add(new(ordered.Count, effect, null, null, null));
        }

        bool cleanParent =
            parent.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.FallThroughTo9DB
            && parent.ChildGateAdmitted
            && parent.ExactChildCompletion
            && parent.ExactCleanCompletion
            && parent.BranchExecuted
            && !parent.BranchTaken
            && !parent.BranchTargetEntered
            && !parent.FallThroughEntered
            && parent.EBP.HasValue
            && parent.EAX.HasValue
            && parent.FlagsAtStop is { Zero: false }
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackPostLoopByteGateProjection.ZeroBranchRawFileOffset
            && parent.StopBeforeRawFileOffset == EntryRawFileOffset;

        NetworkIoErrorCallbackExitPopCaptureState espState =
            BeforeAdmission(inputs.FreshESP.HasValue);
        NetworkIoErrorCallbackExitPopCaptureState ediState =
            BeforeAdmission(inputs.FreshPopEDIDWORD.HasValue);
        NetworkIoErrorCallbackExitPopCaptureState esiState =
            BeforeAdmission(inputs.FreshPopESIDWORD.HasValue);
        NetworkIoErrorCallbackExitPopOutcome outcome;
        uint? ebp = parent.EBP;
        uint? eax = parent.EAX;
        uint? edi = parent.ParentLoopGatePlan.EDI;
        uint? esi = parent.ParentLoopGatePlan.ESI;
        uint? esp = null;
        NetworkIoErrorCallbackLoopGateFlags? flags = parent.FlagsAtStop;
        NetworkIoErrorCallbackExitPopCallCellBoundary? callBoundary = null;
        bool popEdiCompleted = false;
        bool popEsiCompleted = false;
        bool childComplete = false;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!cleanParent)
        {
            outcome =
                NetworkIoErrorCallbackExitPopOutcome.ParentNotAtCleanFallThrough9DB;
        }
        else
        {
            espState = inputs.FreshESP.HasValue
                ? NetworkIoErrorCallbackExitPopCaptureState.Captured
                : NetworkIoErrorCallbackExitPopCaptureState.Missing;
            lastReached = EntryRawFileOffset;
            stopBefore = PopEDIRawFileOffset;

            if (inputs.FreshESP is not uint entryESP)
            {
                ediState = UnreachedOrUnused(inputs.FreshPopEDIDWORD.HasValue);
                esiState = UnreachedOrUnused(inputs.FreshPopESIDWORD.HasValue);
                outcome = NetworkIoErrorCallbackExitPopOutcome.MissingFreshESP;
            }
            else
            {
                esp = entryESP;
                ediState = inputs.FreshPopEDIDWORD.HasValue
                    ? NetworkIoErrorCallbackExitPopCaptureState.Captured
                    : NetworkIoErrorCallbackExitPopCaptureState.Missing;
                NetworkIoErrorCallbackExitPopRead ediRead = AddPop(
                    reads,
                    instructions,
                    ordered,
                    NetworkIoErrorCallbackExitPopReadKind.FreshPopEDIDWORD,
                    NetworkIoErrorCallbackExitPopInstructionKind.PopEDI,
                    PopEDIRawFileOffset,
                    esp.Value,
                    inputs.FreshPopEDIDWORD);

                if (!ediRead.Captured)
                {
                    esiState = UnreachedOrUnused(inputs.FreshPopESIDWORD.HasValue);
                    outcome =
                        NetworkIoErrorCallbackExitPopOutcome.MissingFreshPopEDIDWORD;
                }
                else
                {
                    popEdiCompleted = true;
                    edi = ediRead.Value;
                    esp = unchecked(esp.Value + StackPopIncrement);
                    lastReached = PopEDIRawFileOffset;
                    stopBefore = PopESIRawFileOffset;
                    esiState = inputs.FreshPopESIDWORD.HasValue
                        ? NetworkIoErrorCallbackExitPopCaptureState.Captured
                        : NetworkIoErrorCallbackExitPopCaptureState.Missing;
                    NetworkIoErrorCallbackExitPopRead esiRead = AddPop(
                        reads,
                        instructions,
                        ordered,
                        NetworkIoErrorCallbackExitPopReadKind.FreshPopESIDWORD,
                        NetworkIoErrorCallbackExitPopInstructionKind.PopESI,
                        PopESIRawFileOffset,
                        esp.Value,
                        inputs.FreshPopESIDWORD);

                    if (!esiRead.Captured)
                    {
                        outcome =
                            NetworkIoErrorCallbackExitPopOutcome.MissingFreshPopESIDWORD;
                    }
                    else
                    {
                        popEsiCompleted = true;
                        esi = esiRead.Value;
                        esp = unchecked(esp.Value + StackPopIncrement);
                        lastReached = PopESIRawFileOffset;
                        stopBefore = OpaqueCallCellRawFileOffset;
                        callBoundary = new(
                            OpaqueCallCellRawFileOffset,
                            OpaqueCallCellVirtualAddress,
                            BoundaryReached: true,
                            TargetReadAttempted: false,
                            NativeCallExecuted: false);
                        ordered.Add(new(
                            ordered.Count,
                            null,
                            null,
                            null,
                            callBoundary));
                        outcome =
                            NetworkIoErrorCallbackExitPopOutcome.OpaqueCallCellBoundary;
                        childComplete = true;
                    }
                }
            }
        }

        bool boundedCompletion = cleanParent
            ? childComplete
            : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion = cleanParent
            ? childComplete
            : parent.ExactBoundedCompletion;
        bool exactCleanCompletion = cleanParent
            && childComplete
            && popEdiCompleted
            && popEsiCompleted
            && ediState == NetworkIoErrorCallbackExitPopCaptureState.Captured
            && esiState == NetworkIoErrorCallbackExitPopCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            instructions.ToImmutable(),
            ordered.ToImmutable(),
            new(espState, ediState, esiState),
            ebp,
            eax,
            edi,
            esi,
            esp,
            flags,
            callBoundary,
            cleanParent,
            popEdiCompleted,
            popEsiCompleted,
            childComplete,
            childComplete,
            boundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            NativeInstructionsExecuted: false,
            NativeReadsExecuted: false,
            MemoryWriteExecuted: false,
            NativeCallExecuted: false,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackExitPopRead AddPop(
        ImmutableArray<NetworkIoErrorCallbackExitPopRead>.Builder reads,
        ImmutableArray<NetworkIoErrorCallbackExitPopInstruction>.Builder instructions,
        ImmutableArray<NetworkIoErrorCallbackExitPopOrderedEffect>.Builder ordered,
        NetworkIoErrorCallbackExitPopReadKind readKind,
        NetworkIoErrorCallbackExitPopInstructionKind instructionKind,
        uint rawFileOffset,
        uint address,
        uint? value)
    {
        var read = new NetworkIoErrorCallbackExitPopRead(
            rawFileOffset,
            readKind,
            address,
            Width: (byte)DwordWidth,
            Attempted: true,
            Captured: value.HasValue,
            Value: value,
            NativeReadExecuted: false);
        NetworkIoErrorCallbackExitPopInstruction instruction = new(
            instructionKind,
            rawFileOffset,
            ESPBefore: address,
            ESPAfter: value.HasValue
                ? unchecked(address + StackPopIncrement)
                : address,
            Value: value,
            Completed: value.HasValue);
        instructions.Add(instruction);

        reads.Add(read);
        ordered.Add(new(ordered.Count, null, read, instruction, null));
        return read;
    }

    private static NetworkIoErrorCallbackExitPopCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackExitPopCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackExitPopCaptureState.Unreached;

    private static NetworkIoErrorCallbackExitPopCaptureState UnreachedOrUnused(
        bool supplied) => BeforeAdmission(supplied);
}
