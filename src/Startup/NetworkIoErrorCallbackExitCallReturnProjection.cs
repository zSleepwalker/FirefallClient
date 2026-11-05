using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackExitCallReturnInputs(
    NetworkIoErrorCallbackExitPopInputs ParentExitPopInputs,
    uint? FreshOpaqueCallTargetDWORD,
    bool? OpaqueCallReturnedNormally);

public enum NetworkIoErrorCallbackExitCallReturnOutcome
{
    ParentNotAtOpaqueCallBoundary,
    ParentNotClean,
    MissingFreshOpaqueCallTargetDWORD,
    MissingOpaqueCallReturnObservation,
    OpaqueCallDidNotReturnNormally,
    OpaqueCallReturnedNormally
}

public enum NetworkIoErrorCallbackExitCallReturnCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackExitCallReturnCaptureStates(
    NetworkIoErrorCallbackExitCallReturnCaptureState FreshOpaqueCallTargetDWORD,
    NetworkIoErrorCallbackExitCallReturnCaptureState OpaqueCallReturnedNormally);

public readonly record struct NetworkIoErrorCallbackExitCallReturnTargetRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public readonly record struct NetworkIoErrorCallbackExitCallReturnCallBoundary(
    uint RawFileOffset,
    uint CallCellVirtualAddress,
    uint TargetDWORD,
    uint? PreCallEBP,
    uint? PreCallEAX,
    uint? PreCallEDI,
    uint? PreCallESI,
    uint? PreCallESP,
    bool LogicalCallAttempted,
    bool NativeCallExecuted);

public readonly record struct NetworkIoErrorCallbackExitCallReturnContinuationBoundary(
    uint RawFileOffset,
    byte NextInstructionByte,
    bool Reached,
    bool NextInstructionExecuted);

public enum NetworkIoErrorCallbackExitCallReturnStepKind
{
    ReadFreshOpaqueCallTargetDWORD,
    OpaqueCallBoundary,
    OpaqueCallReturnObservation,
    ContinuationBoundary
}

public readonly record struct NetworkIoErrorCallbackExitCallReturnStep(
    NetworkIoErrorCallbackExitCallReturnStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public readonly record struct NetworkIoErrorCallbackExitCallReturnOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackExitPopOrderedEffect? ParentExitPopEffect,
    NetworkIoErrorCallbackExitCallReturnTargetRead? TargetRead,
    NetworkIoErrorCallbackExitCallReturnCallBoundary? CallBoundary,
    bool? OpaqueCallReturnedNormally,
    NetworkIoErrorCallbackExitCallReturnContinuationBoundary? ContinuationBoundary);

public sealed record NetworkIoErrorCallbackExitCallReturnPlan(
    NetworkIoErrorCallbackExitPopPlan ParentExitPopPlan,
    NetworkIoErrorCallbackExitCallReturnInputs Capture,
    NetworkIoErrorCallbackExitCallReturnOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackExitCallReturnTargetRead> TargetReads,
    ImmutableArray<NetworkIoErrorCallbackExitCallReturnCallBoundary> CallBoundaries,
    ImmutableArray<NetworkIoErrorCallbackExitCallReturnStep> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackExitCallReturnOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackExitCallReturnCaptureStates CaptureStates,
    uint? PreCallEBP,
    uint? PreCallEAX,
    uint? PreCallEDI,
    uint? PreCallESI,
    uint? PreCallESP,
    bool? OpaqueCallReturnedNormally,
    NetworkIoErrorCallbackLoopGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool TargetReadAttempted,
    bool TargetReadCaptured,
    bool LogicalCallAttempted,
    bool CallReturnObservationCaptured,
    NetworkIoErrorCallbackExitCallReturnContinuationBoundary? ContinuationBoundary,
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

    public bool NativeTargetReadExecuted => false;

    public bool NativeCallExecuted => false;

    public bool OpaqueCallTargetEntered => false;

    public bool ContinuationInstructionExecuted => false;
}

/// <summary>
/// Reprojects the clean call-cell boundary, recording an independent target
/// scalar and explicit opaque normal-return observation without executing code.
/// </summary>
public static class NetworkIoErrorCallbackExitCallReturnProjection
{
    public const uint OpaqueCallRawFileOffset =
        NetworkIoErrorCallbackExitPopProjection.OpaqueCallCellRawFileOffset;
    public const uint ContinuationRawFileOffset = 0x00BBC9E3;
    public const uint OpaqueCallCellVirtualAddress =
        NetworkIoErrorCallbackExitPopProjection.OpaqueCallCellVirtualAddress;
    public const byte PopEBXOpcode = 0x5B;
    public const byte DwordWidth = 4;

    public static bool ExecutesNativeInstructions => false;

    public static bool ExecutesNativeReads => false;

    public static bool ExecutesMemoryWrites => false;

    public static bool ExecutesCalls => false;

    public static bool EntersOpaqueCallTarget => false;

    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0xFF, 0x15, 0x60, 0x97, 0xDF, 0x01, PopEBXOpcode);

    public static NetworkIoErrorCallbackExitCallReturnPlan Project(
        NetworkIoErrorCallbackExitCallReturnInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentExitPopInputs);

        NetworkIoErrorCallbackExitPopPlan parent =
            NetworkIoErrorCallbackExitPopProjection.Project(inputs.ParentExitPopInputs);
        var targetReads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitCallReturnTargetRead>();
        var callBoundaries =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitCallReturnCallBoundary>();
        var steps =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitCallReturnStep>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackExitCallReturnOrderedEffect>();

        foreach (NetworkIoErrorCallbackExitPopOrderedEffect effect in parent.OrderedEffects)
        {
            ordered.Add(new(
                ordered.Count,
                effect,
                null,
                null,
                null,
                null));
        }

        bool parentAtBoundary =
            parent.Outcome == NetworkIoErrorCallbackExitPopOutcome.OpaqueCallCellBoundary
            && parent.StopBeforeRawFileOffset == OpaqueCallRawFileOffset
            && parent.CallCellBoundary is
            {
                BoundaryReached: true,
                TargetReadAttempted: false,
                NativeCallExecuted: false
            };
        bool cleanParent = parentAtBoundary
            && parent.ChildGateAdmitted
            && parent.PopEDICompleted
            && parent.PopESICompleted
            && parent.ChildBoundedCompletionBoundaryReached
            && parent.ExactChildCompletion
            && parent.ExactBoundedCompletion
            && parent.ExactCleanCompletion
            && parent.CaptureStates.FreshESP
                == NetworkIoErrorCallbackExitPopCaptureState.Captured
            && parent.CaptureStates.FreshPopEDIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.Captured
            && parent.CaptureStates.FreshPopESIDWORD
                == NetworkIoErrorCallbackExitPopCaptureState.Captured
            && parent.Reads.All(read => read.Captured)
            && parent.InstructionOrder.All(instruction => instruction.Completed)
            && parent.EBP.HasValue
            && parent.EAX.HasValue
            && parent.EDI.HasValue
            && parent.ESI.HasValue
            && parent.ESP.HasValue
            && parent.LastReachedRawFileOffset
                == NetworkIoErrorCallbackExitPopProjection.PopESIRawFileOffset
            && parent.StopBeforeRawFileOffset == OpaqueCallRawFileOffset;

        NetworkIoErrorCallbackExitCallReturnCaptureState targetState =
            BeforeAdmission(inputs.FreshOpaqueCallTargetDWORD.HasValue);
        NetworkIoErrorCallbackExitCallReturnCaptureState returnState =
            BeforeAdmission(inputs.OpaqueCallReturnedNormally.HasValue);
        NetworkIoErrorCallbackExitCallReturnOutcome outcome;
        NetworkIoErrorCallbackExitCallReturnTargetRead? targetRead = null;
        NetworkIoErrorCallbackExitCallReturnCallBoundary? callBoundary = null;
        NetworkIoErrorCallbackExitCallReturnContinuationBoundary? continuationBoundary = null;
        bool targetReadAttempted = false;
        bool targetReadCaptured = false;
        bool logicalCallAttempted = false;
        bool returnObservationCaptured = false;
        bool childComplete = false;
        bool? returnedNormally = null;
        NetworkIoErrorCallbackLoopGateFlags? flagsAtStop = parent.FlagsAtStop;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;

        if (!parentAtBoundary)
        {
            outcome =
                NetworkIoErrorCallbackExitCallReturnOutcome.ParentNotAtOpaqueCallBoundary;
        }
        else if (!cleanParent)
        {
            outcome = NetworkIoErrorCallbackExitCallReturnOutcome.ParentNotClean;
        }
        else
        {
            targetState = inputs.FreshOpaqueCallTargetDWORD.HasValue
                ? NetworkIoErrorCallbackExitCallReturnCaptureState.Captured
                : NetworkIoErrorCallbackExitCallReturnCaptureState.Missing;
            targetReadAttempted = true;
            targetReadCaptured = inputs.FreshOpaqueCallTargetDWORD.HasValue;
            targetRead = new(
                OpaqueCallRawFileOffset,
                OpaqueCallCellVirtualAddress,
                DwordWidth,
                Attempted: true,
                Captured: targetReadCaptured,
                Value: targetReadCaptured ? inputs.FreshOpaqueCallTargetDWORD : null,
                NativeReadExecuted: false);
            targetReads.Add(targetRead.Value);
            steps.Add(new(
                NetworkIoErrorCallbackExitCallReturnStepKind.ReadFreshOpaqueCallTargetDWORD,
                OpaqueCallRawFileOffset,
                OpaqueCallCellVirtualAddress,
                targetReadCaptured ? inputs.FreshOpaqueCallTargetDWORD : null));
            ordered.Add(new(
                ordered.Count,
                null,
                targetRead,
                null,
                null,
                null));
            lastReached = parent.LastReachedRawFileOffset;
            stopBefore = OpaqueCallRawFileOffset;

            if (!targetReadCaptured)
            {
                returnState = UnreachedOrUnused(inputs.OpaqueCallReturnedNormally.HasValue);
                outcome =
                    NetworkIoErrorCallbackExitCallReturnOutcome
                        .MissingFreshOpaqueCallTargetDWORD;
            }
            else
            {
                logicalCallAttempted = true;
                callBoundary = new(
                    OpaqueCallRawFileOffset,
                    OpaqueCallCellVirtualAddress,
                    inputs.FreshOpaqueCallTargetDWORD!.Value,
                    parent.EBP,
                    parent.EAX,
                    parent.EDI,
                    parent.ESI,
                    parent.ESP,
                    LogicalCallAttempted: true,
                    NativeCallExecuted: false);
                callBoundaries.Add(callBoundary.Value);
                steps.Add(new(
                    NetworkIoErrorCallbackExitCallReturnStepKind.OpaqueCallBoundary,
                    OpaqueCallRawFileOffset,
                    OpaqueCallCellVirtualAddress,
                    inputs.FreshOpaqueCallTargetDWORD));
                ordered.Add(new(
                    ordered.Count,
                    null,
                    null,
                    callBoundary,
                    null,
                    null));

                if (inputs.OpaqueCallReturnedNormally is not bool callReturnedNormally)
                {
                    returnState = NetworkIoErrorCallbackExitCallReturnCaptureState.Missing;
                    outcome =
                        NetworkIoErrorCallbackExitCallReturnOutcome
                            .MissingOpaqueCallReturnObservation;
                }
                else
                {
                    returnState = NetworkIoErrorCallbackExitCallReturnCaptureState.Captured;
                    returnObservationCaptured = true;
                    returnedNormally = callReturnedNormally;
                    steps.Add(new(
                        NetworkIoErrorCallbackExitCallReturnStepKind.OpaqueCallReturnObservation,
                        OpaqueCallRawFileOffset,
                        Value: callReturnedNormally ? 1u : 0u));
                    ordered.Add(new(
                        ordered.Count,
                        null,
                        null,
                        null,
                        callReturnedNormally,
                        null));

                    if (!callReturnedNormally)
                    {
                        outcome =
                            NetworkIoErrorCallbackExitCallReturnOutcome
                                .OpaqueCallDidNotReturnNormally;
                    }
                    else
                    {
                        outcome =
                            NetworkIoErrorCallbackExitCallReturnOutcome
                                .OpaqueCallReturnedNormally;
                        continuationBoundary = new(
                            ContinuationRawFileOffset,
                            PopEBXOpcode,
                            Reached: true,
                            NextInstructionExecuted: false);
                        ordered.Add(new(
                            ordered.Count,
                            null,
                            null,
                            null,
                            null,
                            continuationBoundary));
                        steps.Add(new(
                            NetworkIoErrorCallbackExitCallReturnStepKind.ContinuationBoundary,
                            ContinuationRawFileOffset,
                            Value: PopEBXOpcode));
                        childComplete = true;
                        lastReached = OpaqueCallRawFileOffset;
                        stopBefore = ContinuationRawFileOffset;
                        flagsAtStop = null;
                    }
                }
            }
        }

        if (!cleanParent && inputs.FreshOpaqueCallTargetDWORD.HasValue)
        {
            targetState =
                NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused;
        }
        if (!logicalCallAttempted && inputs.OpaqueCallReturnedNormally.HasValue)
        {
            returnState =
                NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused;
        }

        bool boundedCompletion = cleanParent
            ? childComplete
            : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion = cleanParent
            ? childComplete
            : parent.ExactBoundedCompletion;
        bool exactCleanCompletion = cleanParent
            && targetReadCaptured
            && logicalCallAttempted
            && returnObservationCaptured
            && returnedNormally == true
            && targetState == NetworkIoErrorCallbackExitCallReturnCaptureState.Captured
            && returnState == NetworkIoErrorCallbackExitCallReturnCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            targetReads.ToImmutable(),
            callBoundaries.ToImmutable(),
            steps.ToImmutable(),
            ordered.ToImmutable(),
            new(targetState, returnState),
            cleanParent ? parent.EBP : null,
            cleanParent ? parent.EAX : null,
            cleanParent ? parent.EDI : null,
            cleanParent ? parent.ESI : null,
            cleanParent ? parent.ESP : null,
            returnedNormally,
            flagsAtStop,
            cleanParent,
            targetReadAttempted,
            targetReadCaptured,
            logicalCallAttempted,
            returnObservationCaptured,
            continuationBoundary,
            childComplete,
            childComplete,
            boundedCompletion,
            parent.ExactBoundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackExitCallReturnCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackExitCallReturnCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackExitCallReturnCaptureState.Unreached;

    private static NetworkIoErrorCallbackExitCallReturnCaptureState UnreachedOrUnused(
        bool supplied) => BeforeAdmission(supplied);
}
