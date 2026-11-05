using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherFinalCallPreparationCapture(
    NetworkDispatcherFencedPublicationCapture ParentCapture,
    uint? FreshEntryESP,
    uint? FreshArgumentDWORD);

public enum NetworkDispatcherFinalCallPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherFinalCallPreparationCaptureStates(
    NetworkDispatcherFinalCallPreparationCaptureState FreshEntryESP,
    NetworkDispatcherFinalCallPreparationCaptureState FreshArgumentDWORD);

public enum NetworkDispatcherFinalCallPreparationOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshArgumentDWORD,
    MissingFreshEntryESP,
    BoundedBeforeIndirectCall
}

public enum NetworkDispatcherFinalCallPreparationOrderedEffectKind
{
    ParentFencedPublicationEffect,
    FreshArgumentDWORDRead,
    Push
}

public enum NetworkDispatcherFinalCallPreparationInstructionKind
{
    PushArgumentDWORDFromEBPDisplacement,
    IndirectCallBoundary
}

public enum NetworkDispatcherFinalCallPreparationPushKind
{
    ArgumentDWORD
}

public readonly record struct NetworkDispatcherFinalCallPreparationRead(
    uint RawFileOffset,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherFinalCallPreparationPush(
    uint RawFileOffset,
    NetworkDispatcherFinalCallPreparationPushKind Kind,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherFinalCallPreparationInstruction(
    NetworkDispatcherFinalCallPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public readonly record struct NetworkDispatcherFinalCallPreparationCallBoundary(
    uint RawFileOffset,
    uint? IndirectMemoryOperandAddress,
    bool CallSiteReached,
    bool TargetReadAttempted,
    bool TargetCaptured,
    bool CallAttempted);

public readonly record struct NetworkDispatcherFinalCallPreparationOrderedEffect(
    int Sequence,
    NetworkDispatcherFencedPublicationOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherFinalCallPreparationOrderedEffectKind Kind,
    NetworkDispatcherFinalCallPreparationRead? Read,
    NetworkDispatcherFinalCallPreparationPush? Push);

public sealed record NetworkDispatcherFinalCallPreparationPlan(
    NetworkDispatcherFencedPublicationPlan ParentPlan,
    NetworkDispatcherFinalCallPreparationCapture Capture,
    NetworkDispatcherFinalCallPreparationOutcome Outcome,
    ImmutableArray<NetworkDispatcherFinalCallPreparationRead> Reads,
    ImmutableArray<NetworkDispatcherFinalCallPreparationPush> Pushes,
    ImmutableArray<NetworkDispatcherFinalCallPreparationInstruction> InstructionOrder,
    ImmutableArray<NetworkDispatcherFinalCallPreparationCallBoundary> CallBoundaries,
    ImmutableArray<NetworkDispatcherFinalCallPreparationOrderedEffect> OrderedEffects,
    NetworkDispatcherFinalCallPreparationCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    uint EntryRawFileOffset,
    uint? EntryEBP,
    uint? ArgumentAddress,
    uint? ArgumentValue,
    uint? StackPointerAtStop,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint StopAtRawFileOffsetExclusive)
{
    public bool NativeInstructionsExecuted => false;
    public bool IndirectTargetReadAttempted => false;
    public bool IndirectCallAttempted => false;

    public bool CallBoundaryReached =>
        Outcome == NetworkDispatcherFinalCallPreparationOutcome.BoundedBeforeIndirectCall
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherFinalCallPreparationProjection.IndirectCallRawFileOffset;
}

/// <summary>
/// Reprojects the final observed push and stops at the following indirect-call
/// boundary without reading its memory operand or attempting the call.
/// </summary>
public static class NetworkDispatcherFinalCallPreparationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDEAD;
    public const uint ArgumentPushRawFileOffset = 0x00BBDEAD;
    public const uint IndirectCallRawFileOffset = 0x00BBDEB0;
    public const uint ArgumentDisplacement = 0x18;
    public const uint PushWidth = 4;
    public const uint IndirectMemoryOperandAddress = 0x01AC427C;
    public const string CapturedWindowBytes = "FF75E8FF157C42AC01";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherFinalCallPreparationPlan Project(
        NetworkDispatcherFinalCallPreparationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherFencedPublicationPlan parent =
            NetworkDispatcherFencedPublicationProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalCallPreparationOrderedEffect>();
        foreach (NetworkDispatcherFencedPublicationOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherFinalCallPreparationOrderedEffectKind
                    .ParentFencedPublicationEffect,
                null,
                null));
        }

        bool expectedParentBranch = parent.EqualParentBranch.HasValue;
        uint expectedParentEntry = parent.EqualParentBranch == true
            ? NetworkDispatcherFencedPublicationProjection.EqualEntryRawFileOffset
            : NetworkDispatcherFencedPublicationProjection.NotEqualEntryRawFileOffset;
        bool admitted =
            parent.ExactCleanCompletion
            && parent.ChildGateReachedEndpoint
            && parent.Outcome
                == NetworkDispatcherFencedPublicationOutcome.BoundedBeforeContinuation
            && expectedParentBranch
            && parent.EntryRawFileOffset == expectedParentEntry
            && parent.StopAtRawFileOffsetExclusive
                == NetworkDispatcherFencedPublicationProjection.ContinuationRawFileOffset
            && !parent.NativeInstructionsExecuted
            && !parent.OpaqueCallAttempted
            && !parent.ThirdCallTargetEntered;

        var reads =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalCallPreparationRead>();
        var pushes =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalCallPreparationPush>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalCallPreparationInstruction>();
        var callBoundaries =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalCallPreparationCallBoundary>();
        uint? ebp = admitted ? parent.FreshPostCellEBP : null;
        uint? argumentAddress = ebp is uint entryEbp
            ? unchecked(entryEbp - ArgumentDisplacement)
            : null;
        uint? argumentValue = null;
        uint? stackPointer = null;
        NetworkDispatcherFencedPublicationFlags? flags = parent.FlagsAtStop;
        NetworkDispatcherFinalCallPreparationOutcome outcome;
        bool boundaryReached = false;
        uint entry = admitted ? EntryRawFileOffset : parent.StopAtRawFileOffsetExclusive;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;

        NetworkDispatcherFinalCallPreparationCaptureState espState =
            UnreachedOrUnused(capture.FreshEntryESP.HasValue);
        NetworkDispatcherFinalCallPreparationCaptureState argumentState =
            UnreachedOrUnused(capture.FreshArgumentDWORD.HasValue);

        if (!admitted)
        {
            outcome =
                NetworkDispatcherFinalCallPreparationOutcome.ParentHandoffNotAdmitted;
        }
        else
        {
            instructions.Add(new(
                NetworkDispatcherFinalCallPreparationInstructionKind
                    .PushArgumentDWORDFromEBPDisplacement,
                ArgumentPushRawFileOffset,
                argumentAddress));

            argumentState = State(capture.FreshArgumentDWORD);
            bool argumentCaptured = capture.FreshArgumentDWORD.HasValue;
            argumentValue = capture.FreshArgumentDWORD;
            var read = new NetworkDispatcherFinalCallPreparationRead(
                ArgumentPushRawFileOffset,
                argumentAddress,
                Width: 4,
                Attempted: true,
                Captured: argumentCaptured,
                Value: argumentValue,
                Committed: false);
            reads.Add(read);
            effects.Add(new(
                effects.Count,
                null,
                ArgumentPushRawFileOffset,
                NetworkDispatcherFinalCallPreparationOrderedEffectKind
                    .FreshArgumentDWORDRead,
                read,
                null));

            if (!argumentCaptured)
            {
                espState = UnreachedOrUnused(capture.FreshEntryESP.HasValue);
                outcome =
                    NetworkDispatcherFinalCallPreparationOutcome.MissingFreshArgumentDWORD;
                stopAt = ArgumentPushRawFileOffset;
            }
            else
            {
                espState = State(capture.FreshEntryESP);
                if (capture.FreshEntryESP is not uint entryEsp)
                {
                    var missingEspPush = new NetworkDispatcherFinalCallPreparationPush(
                        ArgumentPushRawFileOffset,
                        NetworkDispatcherFinalCallPreparationPushKind.ArgumentDWORD,
                        StackPointerBefore: null,
                        StackPointerAfter: null,
                        Value: argumentValue,
                        Attempted: true,
                        Committed: false);
                    pushes.Add(missingEspPush);
                    effects.Add(new(
                        effects.Count,
                        null,
                        ArgumentPushRawFileOffset,
                        NetworkDispatcherFinalCallPreparationOrderedEffectKind.Push,
                        null,
                        missingEspPush));
                    outcome =
                        NetworkDispatcherFinalCallPreparationOutcome.MissingFreshEntryESP;
                    stopAt = ArgumentPushRawFileOffset;
                }
                else
                {
                    uint after = unchecked(entryEsp - PushWidth);
                    var push = new NetworkDispatcherFinalCallPreparationPush(
                        ArgumentPushRawFileOffset,
                        NetworkDispatcherFinalCallPreparationPushKind.ArgumentDWORD,
                        entryEsp,
                        after,
                        argumentValue,
                        Attempted: true,
                        Committed: true);
                    pushes.Add(push);
                    effects.Add(new(
                        effects.Count,
                        null,
                        ArgumentPushRawFileOffset,
                        NetworkDispatcherFinalCallPreparationOrderedEffectKind.Push,
                        null,
                        push));
                    stackPointer = after;
                    callBoundaries.Add(new(
                        IndirectCallRawFileOffset,
                        IndirectMemoryOperandAddress,
                        CallSiteReached: true,
                        TargetReadAttempted: false,
                        TargetCaptured: false,
                        CallAttempted: false));
                    instructions.Add(new(
                        NetworkDispatcherFinalCallPreparationInstructionKind
                            .IndirectCallBoundary,
                        IndirectCallRawFileOffset,
                        IndirectMemoryOperandAddress));
                    outcome =
                        NetworkDispatcherFinalCallPreparationOutcome.BoundedBeforeIndirectCall;
                    boundaryReached = true;
                    stopAt = IndirectCallRawFileOffset;
                }
            }
        }

        ImmutableArray<NetworkDispatcherFinalCallPreparationOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        var states = new NetworkDispatcherFinalCallPreparationCaptureStates(
            espState,
            argumentState);
        bool exactCleanCompletion =
            boundaryReached
            && parent.ExactCleanCompletion
            && espState == NetworkDispatcherFinalCallPreparationCaptureState.Captured
            && argumentState
                == NetworkDispatcherFinalCallPreparationCaptureState.Captured;
        bool exactBoundedCompletion =
            boundaryReached
            || (!admitted
                && parent.ExactBoundedCompletion
                && stopAt == parent.StopAtRawFileOffsetExclusive);

        return new(
            parent,
            capture,
            outcome,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            instructions.ToImmutable(),
            callBoundaries.ToImmutable(),
            ordered,
            states,
            admitted,
            entry,
            ebp,
            argumentAddress,
            argumentValue,
            stackPointer,
            flags,
            boundaryReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            stopAt);
    }

    private static NetworkDispatcherFinalCallPreparationCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherFinalCallPreparationCaptureState.Captured
            : NetworkDispatcherFinalCallPreparationCaptureState.Missing;

    private static NetworkDispatcherFinalCallPreparationCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherFinalCallPreparationCaptureState.SuppliedUnused
            : NetworkDispatcherFinalCallPreparationCaptureState.Unreached;
}
