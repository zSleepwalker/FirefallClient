using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherFinalReturnSehStoreCapture(
    NetworkDispatcherFinalCallPreparationCapture ParentCapture,
    uint? FreshIndirectTargetDWORD,
    bool? OpaqueCallReturnedNormally,
    uint? FreshPostCallEBP,
    uint? FreshLocalMinusCDWORD);

public enum NetworkDispatcherFinalReturnSehStoreCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused,
    Pending
}

public readonly record struct NetworkDispatcherFinalReturnSehStoreCaptureStates(
    NetworkDispatcherFinalReturnSehStoreCaptureState FreshIndirectTargetDWORD,
    NetworkDispatcherFinalReturnSehStoreCaptureState OpaqueCallReturnedNormally,
    NetworkDispatcherFinalReturnSehStoreCaptureState FreshPostCallEBP,
    NetworkDispatcherFinalReturnSehStoreCaptureState FreshLocalMinusCDWORD);

public enum NetworkDispatcherFinalReturnSehStoreOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshIndirectTargetDWORD,
    OpaqueCallReturnPending,
    OpaqueCallDidNotReturnNormally,
    MissingFreshPostCallEBP,
    MissingFreshLocalMinusCDWORD,
    BoundedBeforePop
}

public enum NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
{
    ParentFinalCallPreparationEffect,
    FreshIndirectTargetDWORDRead,
    OpaqueCall,
    OpaqueCallReturnObservation,
    FreshLocalMinusCDWORDRead,
    FsSegmentDWORDStore
}

public enum NetworkDispatcherFinalReturnSehStoreInstructionKind
{
    IndirectCallBoundary,
    LoadFreshLocalIntoECX,
    StoreECXThroughFS
}

public enum NetworkDispatcherFinalReturnSehStoreSegment
{
    FS
}

public readonly record struct NetworkDispatcherFinalReturnSehStoreRead(
    uint RawFileOffset,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherFinalReturnSehStoreCall(
    uint RawFileOffset,
    uint? IndirectMemoryOperandAddress,
    uint? TargetValue,
    bool CallSiteReached,
    bool TargetReadAttempted,
    bool TargetCaptured,
    bool CallAttempted,
    bool? ReturnedNormally);

public readonly record struct NetworkDispatcherFinalReturnSehStoreReturnObservation(
    uint RawFileOffset,
    bool? ReturnedNormally,
    bool Captured);

public readonly record struct NetworkDispatcherFinalReturnSehStore(
    uint RawFileOffset,
    NetworkDispatcherFinalReturnSehStoreSegment Segment,
    uint SegmentOffset,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherFinalReturnSehStoreInstruction(
    NetworkDispatcherFinalReturnSehStoreInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public readonly record struct NetworkDispatcherFinalReturnSehStoreOrderedEffect(
    int Sequence,
    NetworkDispatcherFinalCallPreparationOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind Kind,
    NetworkDispatcherFinalReturnSehStoreRead? Read,
    NetworkDispatcherFinalReturnSehStoreCall? Call,
    NetworkDispatcherFinalReturnSehStoreReturnObservation? ReturnObservation,
    NetworkDispatcherFinalReturnSehStore? Store);

public sealed record NetworkDispatcherFinalReturnSehStorePlan(
    NetworkDispatcherFinalCallPreparationPlan ParentPlan,
    NetworkDispatcherFinalReturnSehStoreCapture Capture,
    NetworkDispatcherFinalReturnSehStoreOutcome Outcome,
    ImmutableArray<NetworkDispatcherFinalReturnSehStoreRead> Reads,
    ImmutableArray<NetworkDispatcherFinalReturnSehStoreCall> Calls,
    ImmutableArray<NetworkDispatcherFinalReturnSehStore> Stores,
    ImmutableArray<NetworkDispatcherFinalReturnSehStoreInstruction> InstructionOrder,
    ImmutableArray<NetworkDispatcherFinalReturnSehStoreOrderedEffect> OrderedEffects,
    NetworkDispatcherFinalReturnSehStoreCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    uint EntryRawFileOffset,
    uint? IndirectTargetValue,
    uint? PostCallEBP,
    uint? LocalAddress,
    uint? LocalValue,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint StopAtRawFileOffsetExclusive)
{
    public bool NativeInstructionsExecuted => false;
    public bool CalleeIdentityInferred => false;
    public bool EAXInferred => false;
    public bool AbiInferred => false;
    public bool PopInstructionEntered => false;
    public uint? FsLinearAddress => null;
    public uint? StackPointerAtStop => null;

    public bool CallAttempted => Calls.Any(call => call.CallAttempted);
}

/// <summary>
/// Reprojects the final opaque call return and FS:[0] store without executing
/// native instructions or inferring the opaque callee's identity or ABI.
/// </summary>
public static class NetworkDispatcherFinalReturnSehStoreProjection
{
    public const uint EntryRawFileOffset = 0x00BBDEB0;
    public const uint IndirectCallRawFileOffset = 0x00BBDEB0;
    public const uint PostCallLocalReadRawFileOffset = 0x00BBDEB6;
    public const uint FsStoreRawFileOffset = 0x00BBDEB9;
    public const uint PopRawFileOffset = 0x00BBDEC0;
    public const uint IndirectMemoryOperandAddress = 0x01AC427C;
    public const uint LocalDisplacement = 0x0C;
    public const uint FsSegmentOffset = 0;
    public const byte DwordWidth = 4;
    public const string CapturedWindowBytes =
        "FF157C42AC018B4DF464890D00000000";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherFinalReturnSehStorePlan Project(
        NetworkDispatcherFinalReturnSehStoreCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherFinalCallPreparationPlan parent =
            NetworkDispatcherFinalCallPreparationProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalReturnSehStoreOrderedEffect>();
        foreach (NetworkDispatcherFinalCallPreparationOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                    .ParentFinalCallPreparationEffect,
                null,
                null,
                null,
                null));
        }

        bool admitted =
            parent.ExactCleanCompletion
            && parent.BoundedCompletionBoundaryReached
            && parent.Outcome
                == NetworkDispatcherFinalCallPreparationOutcome.BoundedBeforeIndirectCall
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.CallBoundaryReached
            && !parent.NativeInstructionsExecuted
            && !parent.IndirectTargetReadAttempted
            && !parent.IndirectCallAttempted;

        var reads =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalReturnSehStoreRead>();
        var calls =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalReturnSehStoreCall>();
        var stores =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalReturnSehStore>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkDispatcherFinalReturnSehStoreInstruction>();
        NetworkDispatcherFinalReturnSehStoreCaptureState targetState =
            UnreachedOrUnused(capture.FreshIndirectTargetDWORD.HasValue);
        NetworkDispatcherFinalReturnSehStoreCaptureState returnState =
            UnreachedOrUnused(capture.OpaqueCallReturnedNormally.HasValue);
        NetworkDispatcherFinalReturnSehStoreCaptureState ebpState =
            UnreachedOrUnused(capture.FreshPostCallEBP.HasValue);
        NetworkDispatcherFinalReturnSehStoreCaptureState localState =
            UnreachedOrUnused(capture.FreshLocalMinusCDWORD.HasValue);

        uint entry = admitted ? EntryRawFileOffset : parent.StopAtRawFileOffsetExclusive;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        uint? targetValue = null;
        uint? postCallEbp = null;
        uint? localAddress = null;
        uint? localValue = null;
        NetworkDispatcherFencedPublicationFlags? flags = parent.FlagsAtStop;
        NetworkDispatcherFinalReturnSehStoreOutcome outcome;
        bool boundaryReached = false;

        if (!admitted)
        {
            outcome =
                NetworkDispatcherFinalReturnSehStoreOutcome.ParentHandoffNotAdmitted;
        }
        else
        {
            targetState = State(capture.FreshIndirectTargetDWORD);
            var targetRead = new NetworkDispatcherFinalReturnSehStoreRead(
                IndirectCallRawFileOffset,
                IndirectMemoryOperandAddress,
                DwordWidth,
                Attempted: true,
                Captured: capture.FreshIndirectTargetDWORD.HasValue,
                Value: capture.FreshIndirectTargetDWORD,
                Committed: false);
            reads.Add(targetRead);
            effects.Add(new(
                effects.Count,
                null,
                IndirectCallRawFileOffset,
                NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                    .FreshIndirectTargetDWORDRead,
                targetRead,
                null,
                null,
                null));

            if (capture.FreshIndirectTargetDWORD is not uint capturedTarget)
            {
                returnState = UnreachedOrUnused(
                    capture.OpaqueCallReturnedNormally.HasValue);
                ebpState = UnreachedOrUnused(capture.FreshPostCallEBP.HasValue);
                localState = UnreachedOrUnused(capture.FreshLocalMinusCDWORD.HasValue);
                outcome =
                    NetworkDispatcherFinalReturnSehStoreOutcome
                        .MissingFreshIndirectTargetDWORD;
            }
            else
            {
                targetValue = capturedTarget;
                returnState = capture.OpaqueCallReturnedNormally.HasValue
                    ? NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
                    : NetworkDispatcherFinalReturnSehStoreCaptureState.Pending;
                var call = new NetworkDispatcherFinalReturnSehStoreCall(
                    IndirectCallRawFileOffset,
                    IndirectMemoryOperandAddress,
                    capturedTarget,
                    CallSiteReached: true,
                    TargetReadAttempted: true,
                    TargetCaptured: true,
                    CallAttempted: true,
                    ReturnedNormally: capture.OpaqueCallReturnedNormally);
                calls.Add(call);
                effects.Add(new(
                    effects.Count,
                    null,
                    IndirectCallRawFileOffset,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind.OpaqueCall,
                    null,
                    call,
                    null,
                    null));
                var returnObservation =
                    new NetworkDispatcherFinalReturnSehStoreReturnObservation(
                        IndirectCallRawFileOffset,
                        capture.OpaqueCallReturnedNormally,
                        capture.OpaqueCallReturnedNormally.HasValue);
                effects.Add(new(
                    effects.Count,
                    null,
                    IndirectCallRawFileOffset,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                        .OpaqueCallReturnObservation,
                    null,
                    null,
                    returnObservation,
                    null));
                instructions.Add(new(
                    NetworkDispatcherFinalReturnSehStoreInstructionKind
                        .IndirectCallBoundary,
                    IndirectCallRawFileOffset,
                    IndirectMemoryOperandAddress,
                    capturedTarget));

                if (capture.OpaqueCallReturnedNormally is not true)
                {
                    ebpState = UnreachedOrUnused(capture.FreshPostCallEBP.HasValue);
                    localState = UnreachedOrUnused(capture.FreshLocalMinusCDWORD.HasValue);
                    outcome = capture.OpaqueCallReturnedNormally is false
                        ? NetworkDispatcherFinalReturnSehStoreOutcome
                            .OpaqueCallDidNotReturnNormally
                        : NetworkDispatcherFinalReturnSehStoreOutcome
                            .OpaqueCallReturnPending;
                }
                else
                {
                    flags = UnknownFlags;
                    ebpState = State(capture.FreshPostCallEBP);
                    if (capture.FreshPostCallEBP is not uint capturedEbp)
                    {
                        localState = UnreachedOrUnused(
                            capture.FreshLocalMinusCDWORD.HasValue);
                        outcome =
                            NetworkDispatcherFinalReturnSehStoreOutcome
                                .MissingFreshPostCallEBP;
                        stopAt = PostCallLocalReadRawFileOffset;
                    }
                    else
                    {
                        postCallEbp = capturedEbp;
                        localAddress = unchecked(capturedEbp - LocalDisplacement);
                        instructions.Add(new(
                            NetworkDispatcherFinalReturnSehStoreInstructionKind
                                .LoadFreshLocalIntoECX,
                            PostCallLocalReadRawFileOffset,
                            localAddress));
                        localState = State(capture.FreshLocalMinusCDWORD);
                        var localRead = new NetworkDispatcherFinalReturnSehStoreRead(
                            PostCallLocalReadRawFileOffset,
                            localAddress,
                            DwordWidth,
                            Attempted: true,
                            Captured: capture.FreshLocalMinusCDWORD.HasValue,
                            Value: capture.FreshLocalMinusCDWORD,
                            Committed: false);
                        reads.Add(localRead);
                        effects.Add(new(
                            effects.Count,
                            null,
                            PostCallLocalReadRawFileOffset,
                            NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                                .FreshLocalMinusCDWORDRead,
                            localRead,
                            null,
                            null,
                            null));

                        if (capture.FreshLocalMinusCDWORD is not uint capturedLocal)
                        {
                            outcome =
                                NetworkDispatcherFinalReturnSehStoreOutcome
                                    .MissingFreshLocalMinusCDWORD;
                            stopAt = PostCallLocalReadRawFileOffset;
                        }
                        else
                        {
                            localValue = capturedLocal;
                            var store = new NetworkDispatcherFinalReturnSehStore(
                                FsStoreRawFileOffset,
                                NetworkDispatcherFinalReturnSehStoreSegment.FS,
                                FsSegmentOffset,
                                DwordWidth,
                                capturedLocal,
                                Attempted: true,
                                Committed: true);
                            stores.Add(store);
                            effects.Add(new(
                                effects.Count,
                                null,
                                FsStoreRawFileOffset,
                                NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                                    .FsSegmentDWORDStore,
                                null,
                                null,
                                null,
                                store));
                            instructions.Add(new(
                                NetworkDispatcherFinalReturnSehStoreInstructionKind
                                    .StoreECXThroughFS,
                                FsStoreRawFileOffset,
                                FsSegmentOffset,
                                capturedLocal));
                            outcome =
                                NetworkDispatcherFinalReturnSehStoreOutcome.BoundedBeforePop;
                            boundaryReached = true;
                            stopAt = PopRawFileOffset;
                        }
                    }
                }
            }
        }

        ImmutableArray<NetworkDispatcherFinalReturnSehStoreOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        var states = new NetworkDispatcherFinalReturnSehStoreCaptureStates(
            targetState,
            returnState,
            ebpState,
            localState);
        bool exactCleanCompletion =
            boundaryReached
            && parent.ExactCleanCompletion
            && targetState == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && returnState == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && ebpState == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && localState == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && stores.Count == 1
            && stores[0].Committed;
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
            calls.ToImmutable(),
            stores.ToImmutable(),
            instructions.ToImmutable(),
            ordered,
            states,
            admitted,
            entry,
            targetValue,
            postCallEbp,
            localAddress,
            localValue,
            flags,
            boundaryReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            stopAt);
    }

    private static NetworkDispatcherFencedPublicationFlags UnknownFlags =>
        new(null, null, null, null, null, null);

    private static NetworkDispatcherFinalReturnSehStoreCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            : NetworkDispatcherFinalReturnSehStoreCaptureState.Missing;

    private static NetworkDispatcherFinalReturnSehStoreCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            : NetworkDispatcherFinalReturnSehStoreCaptureState.Unreached;
}
