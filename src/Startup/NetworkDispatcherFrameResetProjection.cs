using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherFrameResetCapture(
    NetworkDispatcherRegisterRestoreCapture ParentCapture,
    uint? FreshSavedEBPDWORD);

public enum NetworkDispatcherFrameResetCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherFrameResetOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshSavedEBPDWORD,
    BoundedBeforeRET4
}

public enum NetworkDispatcherFrameResetOrderedEffectKind
{
    ParentRegisterRestoreEffect,
    LogicalMoveESPToEBPProjection,
    SavedEBPDWORDRead,
    LogicalPopEBPAndESPProjection
}

public readonly record struct NetworkDispatcherFrameResetRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherFrameResetESPReset(
    uint RawFileOffset,
    uint? ESPBefore,
    uint? EBPSource,
    uint? ESPAfter,
    bool Attempted,
    bool ESPDerivedFromParentPostCallEBP,
    bool ESPResetInProjection);

public readonly record struct NetworkDispatcherFrameResetPop(
    uint RawFileOffset,
    uint? StackAddress,
    uint? StackValue,
    uint? EBPBefore,
    uint? EBPAfter,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    bool Attempted,
    bool StackReadCaptured,
    bool EBPDerivedFromRead,
    bool ESPAdvancedInProjection);

public readonly record struct NetworkDispatcherFrameResetOrderedEffect(
    int Sequence,
    NetworkDispatcherRegisterRestoreOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherFrameResetOrderedEffectKind Kind,
    NetworkDispatcherFrameResetESPReset? ESPReset,
    NetworkDispatcherFrameResetRead? Read,
    NetworkDispatcherFrameResetPop? Pop);

public sealed record NetworkDispatcherFrameResetPlan(
    NetworkDispatcherRegisterRestorePlan ParentPlan,
    NetworkDispatcherFrameResetCapture Capture,
    NetworkDispatcherFrameResetOutcome Outcome,
    ImmutableArray<NetworkDispatcherFrameResetRead> Reads,
    ImmutableArray<NetworkDispatcherFrameResetPop> Pops,
    ImmutableArray<NetworkDispatcherFrameResetOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkDispatcherFrameResetCaptureState FreshSavedEBPDWORDState,
    bool ChildGateAdmitted,
    uint EntryRawFileOffset,
    uint? ParentESPAtStop,
    uint? EBPSourceForReset,
    uint? ESPAfterReset,
    uint? ESPAtStop,
    uint? EBPAtStop,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint StopAtRawFileOffsetExclusive)
{
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesCommitted => false;
    public bool PopStackReadsAreMemoryWrites => false;
    public bool RET4InstructionEntered => false;
    public bool EAXReturnValueInferred => false;
    public bool RET4StackAdjustmentProjected => false;
}

/// <summary>
/// Reprojects the frame reset and saved-EBP pop after the dispatcher register restores.
/// </summary>
public static class NetworkDispatcherFrameResetProjection
{
    public const uint EntryRawFileOffset = 0x00BBDEC4;
    public const uint MoveESPToEBPRawFileOffset = 0x00BBDEC4;
    public const uint PopEBPRawFileOffset = 0x00BBDEC6;
    public const uint RET4RawFileOffset = 0x00BBDEC7;
    public const byte DwordWidth = 4;
    public const uint ESPIncrement = 4;
    public const string CapturedWindowBytes = "8BE55DC20400";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherFrameResetPlan Project(
        NetworkDispatcherFrameResetCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherRegisterRestorePlan parent =
            NetworkDispatcherRegisterRestoreProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherFrameResetOrderedEffect>();
        foreach (NetworkDispatcherRegisterRestoreOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherFrameResetOrderedEffectKind.ParentRegisterRestoreEffect,
                null,
                null,
                null));
        }

        bool admitted =
            parent.ExactCleanCompletion
            && parent.BoundedCompletionBoundaryReached
            && parent.Outcome
                == NetworkDispatcherRegisterRestoreOutcome.BoundedBeforeMoveESPToEBP
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.ParentPlan.PostCallEBP.HasValue
            && parent.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !parent.NativeInstructionsExecuted
            && !parent.MoveESPToEBPInstructionEntered;

        NetworkDispatcherFrameResetCaptureState captureState =
            admitted
                ? State(capture.FreshSavedEBPDWORD)
                : capture.FreshSavedEBPDWORD.HasValue
                    ? NetworkDispatcherFrameResetCaptureState.SuppliedUnused
                    : NetworkDispatcherFrameResetCaptureState.Unreached;
        uint entry = admitted ? EntryRawFileOffset : parent.StopAtRawFileOffsetExclusive;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        uint? ebpSource = parent.ParentPlan.PostCallEBP;
        uint? espAfterReset = null;
        uint? espAtStop = parent.CurrentESPAtStop;
        uint? ebpAtStop = ebpSource;
        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherFrameResetRead>();
        var pops = ImmutableArray.CreateBuilder<NetworkDispatcherFrameResetPop>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        NetworkDispatcherFrameResetOutcome outcome;
        bool boundaryReached = false;

        if (!admitted)
        {
            outcome = NetworkDispatcherFrameResetOutcome.ParentHandoffNotAdmitted;
        }
        else
        {
            uint resetESP = ebpSource!.Value;
            espAfterReset = resetESP;
            espAtStop = resetESP;
            reached.Add(MoveESPToEBPRawFileOffset);
            var reset = new NetworkDispatcherFrameResetESPReset(
                MoveESPToEBPRawFileOffset,
                parent.CurrentESPAtStop,
                ebpSource,
                resetESP,
                Attempted: true,
                ESPDerivedFromParentPostCallEBP: true,
                ESPResetInProjection: true);
            effects.Add(new(
                effects.Count,
                null,
                MoveESPToEBPRawFileOffset,
                NetworkDispatcherFrameResetOrderedEffectKind.LogicalMoveESPToEBPProjection,
                reset,
                null,
                null));

            reached.Add(PopEBPRawFileOffset);
            uint? savedEBP = capture.FreshSavedEBPDWORD;
            var read = new NetworkDispatcherFrameResetRead(
                PopEBPRawFileOffset,
                resetESP,
                DwordWidth,
                Attempted: true,
                Captured: savedEBP.HasValue,
                Value: savedEBP,
                Committed: false);
            reads.Add(read);
            effects.Add(new(
                effects.Count,
                null,
                PopEBPRawFileOffset,
                NetworkDispatcherFrameResetOrderedEffectKind.SavedEBPDWORDRead,
                null,
                read,
                null));

            if (savedEBP is not uint restoredEBP)
            {
                var missingPop = new NetworkDispatcherFrameResetPop(
                    PopEBPRawFileOffset,
                    resetESP,
                    null,
                    ebpSource,
                    ebpSource,
                    resetESP,
                    resetESP,
                    Attempted: true,
                    StackReadCaptured: false,
                    EBPDerivedFromRead: false,
                    ESPAdvancedInProjection: false);
                pops.Add(missingPop);
                effects.Add(new(
                    effects.Count,
                    null,
                    PopEBPRawFileOffset,
                    NetworkDispatcherFrameResetOrderedEffectKind
                        .LogicalPopEBPAndESPProjection,
                    null,
                    null,
                    missingPop));
                captureState = NetworkDispatcherFrameResetCaptureState.Missing;
                outcome = NetworkDispatcherFrameResetOutcome.MissingFreshSavedEBPDWORD;
                stopAt = PopEBPRawFileOffset;
            }
            else
            {
                uint nextESP = unchecked(resetESP + ESPIncrement);
                var pop = new NetworkDispatcherFrameResetPop(
                    PopEBPRawFileOffset,
                    resetESP,
                    restoredEBP,
                    ebpSource,
                    restoredEBP,
                    resetESP,
                    nextESP,
                    Attempted: true,
                    StackReadCaptured: true,
                    EBPDerivedFromRead: true,
                    ESPAdvancedInProjection: true);
                pops.Add(pop);
                effects.Add(new(
                    effects.Count,
                    null,
                    PopEBPRawFileOffset,
                    NetworkDispatcherFrameResetOrderedEffectKind
                        .LogicalPopEBPAndESPProjection,
                    null,
                    null,
                    pop));
                espAtStop = nextESP;
                ebpAtStop = restoredEBP;
                boundaryReached = true;
                outcome = NetworkDispatcherFrameResetOutcome.BoundedBeforeRET4;
                stopAt = RET4RawFileOffset;
            }
        }

        ImmutableArray<NetworkDispatcherFrameResetOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        bool exactBoundedCompletion =
            boundaryReached
            || (!admitted
                && parent.ExactBoundedCompletion
                && stopAt == parent.StopAtRawFileOffsetExclusive);
        bool exactCleanCompletion =
            boundaryReached
            && parent.ExactCleanCompletion
            && captureState == NetworkDispatcherFrameResetCaptureState.Captured;

        return new(
            parent,
            capture,
            outcome,
            reads.ToImmutable(),
            pops.ToImmutable(),
            ordered,
            reached.ToImmutable(),
            captureState,
            admitted,
            entry,
            parent.CurrentESPAtStop,
            ebpSource,
            espAfterReset,
            espAtStop,
            ebpAtStop,
            parent.FlagsAtStop,
            boundaryReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            stopAt);
    }

    private static NetworkDispatcherFrameResetCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherFrameResetCaptureState.Captured
            : NetworkDispatcherFrameResetCaptureState.Missing;
}
