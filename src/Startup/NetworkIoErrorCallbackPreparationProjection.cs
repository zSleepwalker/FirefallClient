using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkIoErrorCallbackPreparationInputs(
    NetworkIoCompletionPassCapture ParentPassCapture,
    uint? FreshEBP,
    uint? FreshDispatchKeyDWORD,
    uint? FreshESP,
    uint? FreshVptrDWORD);

public enum NetworkIoErrorCallbackPreparationOutcome
{
    ParentPassNotAtMissingDispatchKeyBoundary,
    ParentPassBoundaryNotClean,
    MissingFreshEBP,
    MissingFreshDispatchKeyDWORD,
    MissingFreshESP,
    MissingFreshVptrDWORD,
    Slot20CallBoundary
}

public enum NetworkIoErrorCallbackPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkIoErrorCallbackPreparationCaptureStates(
    NetworkIoErrorCallbackPreparationCaptureState FreshEBP,
    NetworkIoErrorCallbackPreparationCaptureState FreshDispatchKeyDWORD,
    NetworkIoErrorCallbackPreparationCaptureState FreshESP,
    NetworkIoErrorCallbackPreparationCaptureState FreshVptrDWORD);

public enum NetworkIoErrorCallbackPreparationReadKind
{
    FreshDispatchKeyDWORD,
    FreshVptrDWORD
}

public enum NetworkIoErrorCallbackPreparationPushKind
{
    FailedCompletionErrorEAX
}

public enum NetworkIoErrorCallbackPreparationInstructionKind
{
    ReadFreshDispatchKeyDWORD,
    PushFailedCompletionErrorEAX,
    ReadFreshVptrDWORD
}

public readonly record struct NetworkIoErrorCallbackPreparationRead(
    uint RawFileOffset,
    NetworkIoErrorCallbackPreparationReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkIoErrorCallbackPreparationPush(
    uint RawFileOffset,
    NetworkIoErrorCallbackPreparationPushKind Kind,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkIoErrorCallbackPreparationInstruction(
    NetworkIoErrorCallbackPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public readonly record struct NetworkIoErrorCallbackPreparationOrderedEffect(
    int Sequence,
    NetworkIoCompletionPassPlan? ParentPassDecision,
    NetworkIoErrorCallbackPreparationRead? Read,
    NetworkIoErrorCallbackPreparationPush? Push);

public readonly record struct NetworkIoErrorCallbackPreparationCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public sealed record NetworkIoErrorCallbackPreparationPlan(
    NetworkIoCompletionPassPlan ParentPassPlan,
    NetworkIoErrorCallbackPreparationInputs Capture,
    NetworkIoErrorCallbackPreparationOutcome Outcome,
    ImmutableArray<NetworkIoErrorCallbackPreparationRead> Reads,
    ImmutableArray<NetworkIoErrorCallbackPreparationPush> Pushes,
    ImmutableArray<NetworkIoErrorCallbackPreparationInstruction> InstructionOrder,
    ImmutableArray<NetworkIoErrorCallbackPreparationOrderedEffect> OrderedEffects,
    NetworkIoErrorCallbackPreparationCaptureStates CaptureStates,
    uint? FailedCompletionErrorEAX,
    uint? DispatchKeyPointerECX,
    uint? FreshVptrEDX,
    uint? StackPointerAtStop,
    uint? Slot20Address,
    NetworkIoErrorCallbackPreparationCompareFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint? LastReachedRawFileOffset,
    uint? StopBeforeRawFileOffset)
{
    public bool Slot20TargetReadAttempted => false;

    public bool Slot20CallAttempted => false;

    public bool CallbackReturnObserved => false;
}

/// <summary>
/// Reprojects the clean missing-dispatch-key prefix and models only the
/// subsequent key load, error push, vptr load, and pre-call boundary.
/// </summary>
public static class NetworkIoErrorCallbackPreparationProjection
{
    public const uint FailedCompletionErrorCallRawFileOffset = 0x00BBC973;
    public const uint FailedCompletionErrorCompareRawFileOffset = 0x00BBC976;
    public const uint DispatchKeyReadRawFileOffset = 0x00BBC97D;
    public const uint PushFailedCompletionErrorRawFileOffset = 0x00BBC980;
    public const uint FreshVptrReadRawFileOffset = 0x00BBC981;
    public const uint Slot20CallRawFileOffset = 0x00BBC983;
    public const uint OperationAbortedError = 995;
    public const uint DispatchKeyStackOffset = 4;
    public const uint VtableSlotOffset = 0x20;
    public const byte DwordWidth = 4;

    public static bool ExecutesNativeInstructions => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0xFF, 0x55, 0xE0,
            0x3D, 0xE3, 0x03, 0x00, 0x00,
            0x74, 0x3E,
            0x8B, 0x4D, 0xFC,
            0x50,
            0x8B, 0x11,
            0xFF, 0x52, 0x20,
            0xEB, 0x33);

    public static NetworkIoErrorCallbackPreparationPlan Project(
        NetworkIoErrorCallbackPreparationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        NetworkIoCompletionPassPlan parent =
            NetworkIoCompletionPassProjection.Project(inputs.ParentPassCapture);
        var reads =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPreparationRead>();
        var pushes =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPreparationPush>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPreparationInstruction>();
        var ownEffects =
            ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPreparationOrderedEffect>();

        bool expectedParentBoundary = IsMissingDispatchKeyBoundary(parent);
        bool cleanParentPrefix = expectedParentBoundary
            && IsCleanParentPrefix(parent, inputs.ParentPassCapture);
        bool childGateAdmitted = cleanParentPrefix;

        NetworkIoErrorCallbackPreparationOutcome? parentRejection = null;
        if (!expectedParentBoundary)
        {
            parentRejection =
                NetworkIoErrorCallbackPreparationOutcome.ParentPassNotAtMissingDispatchKeyBoundary;
        }
        else if (!cleanParentPrefix)
        {
            parentRejection =
                NetworkIoErrorCallbackPreparationOutcome.ParentPassBoundaryNotClean;
        }

        NetworkIoCompletionAttemptPlan? parentAttempt =
            parent.Attempts.Length == 1 ? parent.Attempts[0] : null;
        uint? errorEax = childGateAdmitted
            ? parentAttempt!.Value.Capture.FailedCompletionError
            : null;
        NetworkIoErrorCallbackPreparationCompareFlags? compareFlags = null;
        if (expectedParentBoundary
            && parentAttempt is NetworkIoCompletionAttemptPlan boundaryAttempt
            && boundaryAttempt.Capture.FailedCompletionError is uint error
            && boundaryAttempt.CaptureStates.FailedCompletionError
                == NetworkIoCompletionCaptureState.Captured)
        {
            compareFlags = CompareFlags(error, OperationAbortedError);
        }

        NetworkIoErrorCallbackPreparationCaptureState ebpState =
            UnreachedOrUnused(inputs.FreshEBP.HasValue);
        NetworkIoErrorCallbackPreparationCaptureState keyState =
            UnreachedOrUnused(inputs.FreshDispatchKeyDWORD.HasValue);
        NetworkIoErrorCallbackPreparationCaptureState espState =
            UnreachedOrUnused(inputs.FreshESP.HasValue);
        NetworkIoErrorCallbackPreparationCaptureState vptrState =
            UnreachedOrUnused(inputs.FreshVptrDWORD.HasValue);

        uint? ecx = null;
        uint? edx = null;
        uint? stackPointer = null;
        uint? slot20Address = null;
        uint? lastReached = parentRejection is null
            ? null
            : parent.LastReachedRawFileOffset;
        uint? stopBefore = parentRejection is null
            ? null
            : parent.StopBeforeRawFileOffset;
        NetworkIoErrorCallbackPreparationOutcome outcome;
        bool boundedBoundary = false;

        if (parentRejection is NetworkIoErrorCallbackPreparationOutcome rejection)
        {
            outcome = rejection;
        }
        else
        {
            uint? keyAddress = inputs.FreshEBP is uint ebp
                ? unchecked(ebp - DispatchKeyStackOffset)
                : null;
            ebpState = CaptureState(inputs.FreshEBP, consumed: true, required: true);
            keyState = CaptureState(
                inputs.FreshDispatchKeyDWORD,
                consumed: inputs.FreshEBP.HasValue,
                required: inputs.FreshEBP.HasValue);
            AddRead(
                DispatchKeyReadRawFileOffset,
                NetworkIoErrorCallbackPreparationReadKind.FreshDispatchKeyDWORD,
                keyAddress,
                inputs.FreshDispatchKeyDWORD,
                captured: inputs.FreshEBP.HasValue
                    && inputs.FreshDispatchKeyDWORD.HasValue);
            instructions.Add(new(
                NetworkIoErrorCallbackPreparationInstructionKind.ReadFreshDispatchKeyDWORD,
                DispatchKeyReadRawFileOffset,
                keyAddress,
                inputs.FreshEBP.HasValue ? inputs.FreshDispatchKeyDWORD : null));
            lastReached = DispatchKeyReadRawFileOffset;
            stopBefore = DispatchKeyReadRawFileOffset;

            if (inputs.FreshEBP is null)
            {
                outcome = NetworkIoErrorCallbackPreparationOutcome.MissingFreshEBP;
            }
            else if (inputs.FreshDispatchKeyDWORD is not uint dispatchKey)
            {
                outcome =
                    NetworkIoErrorCallbackPreparationOutcome.MissingFreshDispatchKeyDWORD;
            }
            else
            {
                ecx = dispatchKey;
                espState = CaptureState(
                    inputs.FreshESP,
                    consumed: inputs.FreshESP.HasValue,
                    required: true);
                uint? pushedEsp = inputs.FreshESP is uint esp
                    ? unchecked(esp - DwordWidth)
                    : null;
                var push = new NetworkIoErrorCallbackPreparationPush(
                    PushFailedCompletionErrorRawFileOffset,
                    NetworkIoErrorCallbackPreparationPushKind.FailedCompletionErrorEAX,
                    inputs.FreshESP,
                    pushedEsp,
                    errorEax,
                    Attempted: true,
                    Committed: inputs.FreshESP.HasValue);
                pushes.Add(push);
                ownEffects.Add(new(ownEffects.Count, null, null, push));
                instructions.Add(new(
                    NetworkIoErrorCallbackPreparationInstructionKind.PushFailedCompletionErrorEAX,
                    PushFailedCompletionErrorRawFileOffset,
                    pushedEsp,
                    errorEax));
                lastReached = PushFailedCompletionErrorRawFileOffset;
                stopBefore = PushFailedCompletionErrorRawFileOffset;

                if (inputs.FreshESP is null)
                {
                    outcome = NetworkIoErrorCallbackPreparationOutcome.MissingFreshESP;
                }
                else
                {
                    stackPointer = pushedEsp;
                    vptrState = CaptureState(
                        inputs.FreshVptrDWORD,
                        consumed: inputs.FreshVptrDWORD.HasValue,
                        required: true);
                    var read = new NetworkIoErrorCallbackPreparationRead(
                        FreshVptrReadRawFileOffset,
                        NetworkIoErrorCallbackPreparationReadKind.FreshVptrDWORD,
                        ecx,
                        DwordWidth,
                        Attempted: true,
                        Captured: inputs.FreshVptrDWORD.HasValue,
                        Value: inputs.FreshVptrDWORD,
                        Committed: false);
                    reads.Add(read);
                    ownEffects.Add(new(ownEffects.Count, null, read, null));
                    instructions.Add(new(
                        NetworkIoErrorCallbackPreparationInstructionKind.ReadFreshVptrDWORD,
                        FreshVptrReadRawFileOffset,
                        ecx,
                        inputs.FreshVptrDWORD));
                    lastReached = FreshVptrReadRawFileOffset;
                    stopBefore = FreshVptrReadRawFileOffset;

                    if (inputs.FreshVptrDWORD is not uint vptr)
                    {
                        outcome =
                            NetworkIoErrorCallbackPreparationOutcome.MissingFreshVptrDWORD;
                    }
                    else
                    {
                        edx = vptr;
                        slot20Address = unchecked(vptr + VtableSlotOffset);
                        outcome = NetworkIoErrorCallbackPreparationOutcome.Slot20CallBoundary;
                        boundedBoundary = true;
                        stopBefore = Slot20CallRawFileOffset;
                    }
                }
            }
        }

        var captureStates = new NetworkIoErrorCallbackPreparationCaptureStates(
            ebpState,
            keyState,
            espState,
            vptrState);
        var ordered = ImmutableArray.CreateBuilder<NetworkIoErrorCallbackPreparationOrderedEffect>();
        ordered.Add(new(ordered.Count, parent, null, null));

        foreach (NetworkIoErrorCallbackPreparationOrderedEffect effect in ownEffects)
        {
            ordered.Add(effect with { Sequence = ordered.Count });
        }

        bool noUnusedChildCapture =
            ebpState != NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && keyState != NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && espState != NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            && vptrState != NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused;
        bool exactCleanCompletion =
            boundedBoundary
            && cleanParentPrefix
            && ebpState == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && keyState == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && espState == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && vptrState == NetworkIoErrorCallbackPreparationCaptureState.Captured
            && noUnusedChildCapture;

        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            instructions.ToImmutable(),
            ordered.ToImmutable(),
            captureStates,
            errorEax,
            ecx,
            edx,
            stackPointer,
            slot20Address,
            compareFlags,
            childGateAdmitted,
            boundedBoundary,
            boundedBoundary || (!childGateAdmitted && HasParentActualBound(parent)),
            exactCleanCompletion,
            lastReached,
            stopBefore);

        void AddRead(
            uint rawFileOffset,
            NetworkIoErrorCallbackPreparationReadKind kind,
            uint? address,
            uint? value,
            bool captured)
        {
            var read = new NetworkIoErrorCallbackPreparationRead(
                rawFileOffset,
                kind,
                address,
                DwordWidth,
                Attempted: true,
                captured,
                captured ? value : null,
                Committed: false);
            reads.Add(read);
            ownEffects.Add(new(ownEffects.Count, null, read, null));
        }
    }

    private static bool IsMissingDispatchKeyBoundary(NetworkIoCompletionPassPlan parent) =>
        parent.Outcome
            == NetworkIoCompletionPassOutcome.MissingCompletionKeyPointerAtErrorDispatch
        && parent.Disposition == NetworkIoCompletionPassDisposition.BoundedAtCapture
        && parent.LastReachedRawFileOffset == DispatchKeyReadRawFileOffset
        && parent.StopBeforeRawFileOffset == DispatchKeyReadRawFileOffset;

    private static bool IsCleanParentPrefix(
        NetworkIoCompletionPassPlan parent,
        NetworkIoCompletionPassCapture parentCapture)
    {
        if (!parent.PassEntered
            || parent.StopFlagAtEntryState != NetworkIoCompletionCaptureState.Captured
            || parentCapture.StopFlagAtEntry != false
            || parent.AttemptCapturesState != NetworkIoCompletionCaptureState.Captured
            || parent.Attempts.Length != 1
            || !parent.UnusedAttemptCaptures.IsEmpty
            || parent.StopFlagAfterPassReadReached
            || parent.StopFlagAfterPassState != NetworkIoCompletionCaptureState.NotReached
            || parentCapture.StopFlagAfterPass.HasValue)
        {
            return false;
        }

        NetworkIoCompletionAttemptPlan attempt = parent.Attempts[0];
        NetworkIoCompletionAttemptCapture capture = attempt.Capture;
        NetworkIoCompletionAttemptCaptureStates states = attempt.CaptureStates;
        bool nullKeyRequired = capture.CompletionKeyPointerFromDequeue == 0;
        return attempt.AttemptNumber == 1
            && attempt.RequestedTimeoutMilliseconds
                == NetworkIoCompletionPassProjection.InitialTimeoutMilliseconds
            && attempt.HasFreshOutcomeCapture
            && attempt.DequeueBooleanBranchReached
            && attempt.LastReachedRawFileOffset == DispatchKeyReadRawFileOffset
            && attempt.StopBeforeRawFileOffset == DispatchKeyReadRawFileOffset
            && !attempt.CountedTowardAttemptLimit
            && states.GetLastErrorAfterDequeue == NetworkIoCompletionCaptureState.Captured
            && capture.GetLastErrorAfterDequeue.HasValue
            && states.DequeueSucceeded == NetworkIoCompletionCaptureState.Captured
            && capture.DequeueSucceeded == false
            && states.OverlappedPointerFromDequeue
                == NetworkIoCompletionCaptureState.Captured
            && capture.OverlappedPointerFromDequeue is uint overlapped
            && overlapped != 0
            && states.CompletionKeyPointerFromDequeue
                == NetworkIoCompletionCaptureState.Captured
            && capture.CompletionKeyPointerFromDequeue.HasValue
            && (nullKeyRequired
                ? states.DiagnosticReturnedNormally == NetworkIoCompletionCaptureState.Captured
                    && capture.DiagnosticReturnedNormally == true
                    && attempt.DiagnosticDecision
                        == NetworkIoCompletionDiagnosticDecision.NullCompletionKey
                : states.DiagnosticReturnedNormally == NetworkIoCompletionCaptureState.NotReached
                    && capture.DiagnosticReturnedNormally is null
                    && attempt.DiagnosticDecision
                        == NetworkIoCompletionDiagnosticDecision.NotRequired)
            && states.CompletionKeyPointerAfterDiagnostic
                == NetworkIoCompletionCaptureState.NotReached
            && capture.CompletionKeyPointerAfterDiagnostic is null
            && states.OverlappedPointerAtSuccessDispatch
                == NetworkIoCompletionCaptureState.NotReached
            && capture.OverlappedPointerAtSuccessDispatch is null
            && states.FailedCompletionError == NetworkIoCompletionCaptureState.Captured
            && capture.FailedCompletionError is uint failedError
            && failedError != OperationAbortedError
            && states.CompletionKeyPointerAtErrorDispatch
                == NetworkIoCompletionCaptureState.Missing
            && capture.CompletionKeyPointerAtErrorDispatch is null
            && states.CallbackReturnedNormally == NetworkIoCompletionCaptureState.NotReached
            && capture.CallbackReturnedNormally is null
            && attempt.CallbackDecision == NetworkIoCompletionCallbackDecision.NotEvaluated
            && attempt.DispatchSlotOffset is null;
    }

    private static bool HasParentActualBound(NetworkIoCompletionPassPlan parent) =>
        parent.Disposition != NetworkIoCompletionPassDisposition.BoundedAtCapture;

    private static NetworkIoErrorCallbackPreparationCaptureState CaptureState(
        uint? value,
        bool consumed,
        bool required) =>
        consumed && value.HasValue
            ? NetworkIoErrorCallbackPreparationCaptureState.Captured
            : value.HasValue
                ? NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
                : required
                    ? NetworkIoErrorCallbackPreparationCaptureState.Missing
                    : NetworkIoErrorCallbackPreparationCaptureState.Unreached;

    private static NetworkIoErrorCallbackPreparationCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkIoErrorCallbackPreparationCaptureState.SuppliedButUnused
            : NetworkIoErrorCallbackPreparationCaptureState.Unreached;

    private static NetworkIoErrorCallbackPreparationCompareFlags CompareFlags(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        bool auxiliaryCarry = ((left ^ right ^ result) & 0x10) != 0;
        bool parity = (System.Numerics.BitOperations.PopCount(result & 0xFF) & 1) == 0;
        bool overflow = (((left ^ right) & (left ^ result) & 0x80000000) != 0);
        return new(
            Carry: left < right,
            Parity: parity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: overflow);
    }
}
