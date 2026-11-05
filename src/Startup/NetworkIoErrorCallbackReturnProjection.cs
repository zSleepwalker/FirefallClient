using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackReturnInputs(
    NetworkIoErrorCallbackPreparationInputs ParentPreparationInputs,
    uint? FreshSlot20TargetDWORD,
    bool? OpaqueCallReturnedNormally);

public enum NetworkIoErrorCallbackReturnOutcome
{
    ParentPreparationNotAtSlot20Boundary,
    ParentPreparationNotClean,
    MissingFreshSlot20TargetDWORD,
    MissingOpaqueCallReturnObservation,
    OpaqueCallDidNotReturnNormally,
    OpaqueCallReturnedNormally
}

public enum NetworkIoErrorCallbackReturnCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackReturnCaptureStates(
    NetworkIoErrorCallbackReturnCaptureState FreshSlot20TargetDWORD,
    NetworkIoErrorCallbackReturnCaptureState OpaqueCallReturnedNormally);

public readonly record struct NetworkIoErrorCallbackReturnTargetRead(
    uint RawFileOffset,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool NativeReadExecuted);

public readonly record struct NetworkIoErrorCallbackReturnCallBoundary(
    uint RawFileOffset,
    uint? TargetPointer,
    uint? DispatchKeyPointerECX,
    uint? FailedCompletionErrorEAX,
    uint? PreCallStackPointer,
    bool LogicalCallAttempted,
    bool NativeCallExecuted);

public enum NetworkIoErrorCallbackReturnStepKind
{
    ReadFreshSlot20TargetDWORD,
    OpaqueCallBoundary,
    OpaqueCallReturnObservation,
    ReturnJump
}

public readonly record struct NetworkIoErrorCallbackReturnStep(
    NetworkIoErrorCallbackReturnStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct NetworkIoErrorCallbackReturnOrderedEffect(
    int Sequence,
    NetworkIoErrorCallbackPreparationOrderedEffect? ParentPreparationEffect,
    NetworkIoErrorCallbackReturnTargetRead? TargetRead,
    NetworkIoErrorCallbackReturnCallBoundary? CallBoundary,
    bool? OpaqueCallReturnedNormally,
    uint? JumpRawFileOffset,
    uint? JumpTargetRawFileOffset);

public sealed record NetworkIoErrorCallbackReturnPlan(
    NetworkIoErrorCallbackPreparationPlan ParentPreparationPlan,
    NetworkIoErrorCallbackReturnInputs Capture,
    NetworkIoErrorCallbackReturnOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackReturnTargetRead> TargetReads,
    ImmutableArray<NetworkIoErrorCallbackReturnCallBoundary> CallBoundaries,
    ImmutableArray<NetworkIoErrorCallbackReturnStep> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackReturnOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackReturnCaptureStates CaptureStates,
    uint? DispatchKeyPointerECX,
    uint? FailedCompletionErrorEAX,
    uint? PreCallStackPointer,
    bool? OpaqueCallReturnedNormally,
    NetworkIoErrorCallbackPreparationCompareFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool TargetReadAttempted,
    bool TargetReadCaptured,
    bool LogicalCallAttempted,
    bool CallReturnObservationCaptured,
    bool ChildBoundedCompletionBoundaryReached,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset)
{
    public bool NativeInstructionsExecuted => false;

    public bool NativeTargetReadExecuted => false;

    public bool NativeCallExecuted => false;
    public bool ReturnJumpTargetEntered => false;
}

/// <summary>
/// Reprojects the prepared slot-20 boundary and records only independent
/// target and opaque normal-return observations; it never executes native code.
/// </summary>
public static class NetworkIoErrorCallbackReturnProjection
{
    public const uint Slot20CallRawFileOffset =
        NetworkIoErrorCallbackPreparationProjection.Slot20CallRawFileOffset;
    public const uint ReturnJumpRawFileOffset = 0x00BBC986;
    public const uint ReturnJumpNextInstructionRawFileOffset = 0x00BBC988;
    public const byte ReturnJumpDisplacement = 0x33;
    public const uint ReturnJumpTargetRawFileOffset =
        ReturnJumpNextInstructionRawFileOffset + ReturnJumpDisplacement;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint ReturnJumpTargetVirtualAddress =
        ReturnJumpTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const byte DwordWidth = 4;

    public static bool ExecutesNativeInstructions => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0xFF, 0x52, 0x20,
            0xEB, 0x33);

    public static NetworkIoErrorCallbackReturnPlan Project(
        NetworkIoErrorCallbackReturnInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(inputs.ParentPreparationInputs);

        NetworkIoErrorCallbackPreparationPlan parent =
            NetworkIoErrorCallbackPreparationProjection.Project(
                inputs.ParentPreparationInputs);
        var targetReads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackReturnTargetRead>();
        var callBoundaries =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackReturnCallBoundary>();
        var steps =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackReturnStep>();
        var ordered =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackReturnOrderedEffect>();

        foreach (NetworkIoErrorCallbackPreparationOrderedEffect effect in parent.OrderedEffects)
        {
            ordered.Add(new(
                ordered.Count,
                effect,
                null,
                null,
                null,
                null,
                null));
        }

        bool expectedBoundary =
            parent.Outcome == NetworkIoErrorCallbackPreparationOutcome.Slot20CallBoundary
            && parent.StopBeforeRawFileOffset == Slot20CallRawFileOffset;
        bool cleanParent = expectedBoundary
            && parent.ChildGateAdmitted
            && parent.ExactCleanCompletion
            && parent.CaptureStates.FreshEBP
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && parent.CaptureStates.FreshDispatchKeyDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && parent.CaptureStates.FreshESP
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && parent.CaptureStates.FreshVptrDWORD
                == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && parent.Reads.All(read => read.Captured)
            && parent.Pushes.All(push => push.Committed);

        NetworkIoErrorCallbackReturnCaptureState targetState =
            UnreachedOrUnused(inputs.FreshSlot20TargetDWORD.HasValue);
        NetworkIoErrorCallbackReturnCaptureState returnState =
            UnreachedOrUnused(inputs.OpaqueCallReturnedNormally.HasValue);
        NetworkIoErrorCallbackReturnOutcome outcome;
        NetworkIoErrorCallbackReturnTargetRead? targetRead = null;
        NetworkIoErrorCallbackReturnCallBoundary? callBoundary = null;
        bool targetReadAttempted = false;
        bool targetReadCaptured = false;
        bool logicalCallAttempted = false;
        bool returnObservationCaptured = false;
        bool childBoundedCompletion = false;
        bool? returnedNormally = null;
        NetworkIoErrorCallbackPreparationCompareFlags? flagsAtStop = parent.FlagsAtStop;
        uint? lastReached = parent.LastReachedRawFileOffset;
        uint? stopBefore = parent.StopBeforeRawFileOffset;
        uint? dispatchKey = null;
        uint? failedError = null;
        uint? preCallStackPointer = null;

        if (parent.Outcome
            == NetworkIoErrorCallbackPreparationOutcome.ParentPassBoundaryNotClean)
        {
            outcome = NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotClean;
        }
        else if (!expectedBoundary)
        {
            outcome =
                NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotAtSlot20Boundary;
        }
        else if (!cleanParent)
        {
            outcome = NetworkIoErrorCallbackReturnOutcome.ParentPreparationNotClean;
        }
        else
        {
            targetState = inputs.FreshSlot20TargetDWORD.HasValue
                ? NetworkIoErrorCallbackReturnCaptureState.Captured
                : NetworkIoErrorCallbackReturnCaptureState.Missing;
            targetReadAttempted = true;
            targetReadCaptured = inputs.FreshSlot20TargetDWORD.HasValue;
            targetRead = new(
                Slot20CallRawFileOffset,
                parent.Slot20Address,
                DwordWidth,
                Attempted: true,
                Captured: targetReadCaptured,
                Value: targetReadCaptured ? inputs.FreshSlot20TargetDWORD : null,
                NativeReadExecuted: false);
            targetReads.Add(targetRead.Value);
            steps.Add(new(
                NetworkIoErrorCallbackReturnStepKind.ReadFreshSlot20TargetDWORD,
                Slot20CallRawFileOffset,
                parent.Slot20Address,
                targetReadCaptured ? inputs.FreshSlot20TargetDWORD : null));
            ordered.Add(new(
                ordered.Count,
                null,
                targetRead,
                null,
                null,
                null,
                null));
            stopBefore = Slot20CallRawFileOffset;
            lastReached = Slot20CallRawFileOffset;

            if (!targetReadCaptured)
            {
                outcome =
                    NetworkIoErrorCallbackReturnOutcome.MissingFreshSlot20TargetDWORD;
            }
            else
            {
                dispatchKey = parent.DispatchKeyPointerECX;
                failedError = parent.FailedCompletionErrorEAX;
                preCallStackPointer = parent.StackPointerAtStop;
                logicalCallAttempted = true;
                callBoundary = new(
                    Slot20CallRawFileOffset,
                    inputs.FreshSlot20TargetDWORD,
                    dispatchKey,
                    failedError,
                    preCallStackPointer,
                    LogicalCallAttempted: true,
                    NativeCallExecuted: false);
                callBoundaries.Add(callBoundary.Value);
                steps.Add(new(
                    NetworkIoErrorCallbackReturnStepKind.OpaqueCallBoundary,
                    Slot20CallRawFileOffset,
                    inputs.FreshSlot20TargetDWORD));
                ordered.Add(new(
                    ordered.Count,
                    null,
                    null,
                    callBoundary,
                    null,
                    null,
                    null));

                if (inputs.OpaqueCallReturnedNormally is not bool callReturnedNormally)
                {
                    returnState = NetworkIoErrorCallbackReturnCaptureState.Missing;
                    outcome =
                        NetworkIoErrorCallbackReturnOutcome.MissingOpaqueCallReturnObservation;
                }
                else
                {
                    returnState = NetworkIoErrorCallbackReturnCaptureState.Captured;
                    returnObservationCaptured = true;
                    returnedNormally = callReturnedNormally;
                    steps.Add(new(
                        NetworkIoErrorCallbackReturnStepKind.OpaqueCallReturnObservation,
                        Slot20CallRawFileOffset,
                        Value: callReturnedNormally ? 1u : 0u));
                    ordered.Add(new(
                        ordered.Count,
                        null,
                        null,
                        null,
                        callReturnedNormally,
                        null,
                        null));

                    if (!callReturnedNormally)
                    {
                        outcome =
                            NetworkIoErrorCallbackReturnOutcome.OpaqueCallDidNotReturnNormally;
                    }
                    else
                    {
                        outcome =
                            NetworkIoErrorCallbackReturnOutcome.OpaqueCallReturnedNormally;
                        childBoundedCompletion = true;
                        lastReached = ReturnJumpRawFileOffset;
                        stopBefore = ReturnJumpTargetRawFileOffset;
                        flagsAtStop = null;
                        steps.Add(new(
                            NetworkIoErrorCallbackReturnStepKind.ReturnJump,
                            ReturnJumpRawFileOffset,
                            BranchTargetRawFileOffset: ReturnJumpTargetRawFileOffset));
                        ordered.Add(new(
                            ordered.Count,
                            null,
                            null,
                            null,
                            null,
                            ReturnJumpRawFileOffset,
                            ReturnJumpTargetRawFileOffset));
                    }
                }
            }
        }

        if (!cleanParent && inputs.FreshSlot20TargetDWORD.HasValue)
        {
            targetState = NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused;
        }
        if (!logicalCallAttempted && inputs.OpaqueCallReturnedNormally.HasValue)
        {
            returnState = NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused;
        }

        bool boundedCompletion =
            cleanParent ? childBoundedCompletion : parent.BoundedCompletionBoundaryReached;
        bool exactBoundedCompletion =
            cleanParent ? childBoundedCompletion : parent.ExactBoundedCompletion;
        bool exactCleanCompletion = cleanParent
            && targetReadCaptured
            && logicalCallAttempted
            && returnObservationCaptured
            && returnedNormally == true
            && targetState == NetworkIoErrorCallbackReturnCaptureState.Captured
            && returnState == NetworkIoErrorCallbackReturnCaptureState.Captured;

        return new(
            parent,
            inputs,
            outcome,
            targetReads.ToImmutable(),
            callBoundaries.ToImmutable(),
            steps.ToImmutable(),
            ordered.ToImmutable(),
            new(targetState, returnState),
            dispatchKey,
            failedError,
            preCallStackPointer,
            returnedNormally,
            flagsAtStop,
            cleanParent,
            targetReadAttempted,
            targetReadCaptured,
            logicalCallAttempted,
            returnObservationCaptured,
            childBoundedCompletion,
            boundedCompletion,
            exactBoundedCompletion,
            exactCleanCompletion,
            lastReached,
            stopBefore);
    }

    private static NetworkIoErrorCallbackReturnCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackReturnCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackReturnCaptureState.Unreached;
}
