using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketCompletionErrorEpilogueCaptures(
    GameSocketCompletionErrorStateCapture ParentError,
    uint? PostQueueEbp,
    uint? PreviousSeh,
    uint? FirstPopEsp,
    uint? PoppedEcx,
    uint? PoppedEdi,
    uint? PoppedEsi,
    uint? SavedEbp);

public enum GameSocketCompletionErrorEpilogueCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketCompletionErrorEpilogueCaptureStates(
    GameSocketCompletionErrorEpilogueCaptureState PostQueueEbp,
    GameSocketCompletionErrorEpilogueCaptureState PreviousSeh,
    GameSocketCompletionErrorEpilogueCaptureState FirstPopEsp,
    GameSocketCompletionErrorEpilogueCaptureState PoppedEcx,
    GameSocketCompletionErrorEpilogueCaptureState PoppedEdi,
    GameSocketCompletionErrorEpilogueCaptureState PoppedEsi,
    GameSocketCompletionErrorEpilogueCaptureState SavedEbp);

public enum GameSocketCompletionErrorEpilogueCaptureKind
{
    PostQueueEbp,
    PreviousSeh,
    FirstPopEsp,
    PoppedEcx,
    PoppedEdi,
    PoppedEsi,
    SavedEbp
}

public enum GameSocketCompletionErrorEpilogueOutcome
{
    ParentBoundary,
    MissingPostQueueEbp,
    MissingPreviousSeh,
    MissingFirstPopEsp,
    MissingPoppedEcx,
    MissingPoppedEdi,
    MissingPoppedEsi,
    MissingSavedEbp,
    BoundedBeforeNativeRet
}

public enum GameSocketCompletionErrorEpilogueDisposition
{
    BoundedAtParent,
    BoundedAtPostQueueEbpRead,
    BoundedAtPreviousSehRead,
    BoundedAtFirstPop,
    BoundedAtPopEcx,
    BoundedAtPopEdi,
    BoundedAtPopEsi,
    BoundedAtPopEbp,
    BoundedBeforeNativeRet
}

public enum GameSocketCompletionErrorEpilogueMemoryEffectKind
{
    PreviousSehRead,
    SegmentWrite,
    StackRead
}

public readonly record struct GameSocketCompletionErrorEpilogueMemoryEffect(
    GameSocketCompletionErrorEpilogueMemoryEffectKind Kind,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool? Committed,
    uint InstructionRawFileOffset,
    string? Segment,
    uint? SegmentOffset,
    int? Offset);

public readonly record struct GameSocketCompletionErrorEpilogueTraceStep(
    int Sequence,
    GameSocketCompletionErrorStateMemoryEffect? ParentMemoryEffect,
    GameSocketCompletionErrorEpilogueMemoryEffect? EpilogueMemoryEffect);

public sealed class GameSocketCompletionErrorEpiloguePlan
{
    internal GameSocketCompletionErrorEpiloguePlan(
        GameSocketCompletionErrorEpilogueCaptures captures,
        GameSocketCompletionErrorStatePlan parentPlan,
        GameSocketCompletionErrorEpilogueCaptureStates captureStates,
        ImmutableArray<GameSocketCompletionErrorEpilogueCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketCompletionErrorEpilogueMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketCompletionErrorEpilogueTraceStep> orderedTrace,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketCompletionErrorEpilogueOutcome outcome,
        GameSocketCompletionErrorEpilogueDisposition disposition,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        bool projectionEntered)
    {
        Captures = captures;
        ParentPlan = parentPlan;
        CaptureStates = captureStates;
        UnusedCaptures = unusedCaptures;
        MemoryEffects = memoryEffects;
        OrderedTrace = orderedTrace;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        Disposition = disposition;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        FlagsAtStop = flagsAtStop;
        ProjectionEntered = projectionEntered;
    }

    public GameSocketCompletionErrorEpilogueCaptures Captures { get; }
    public GameSocketCompletionErrorStatePlan ParentPlan { get; }
    public GameSocketCompletionErrorEpilogueCaptureStates CaptureStates { get; }
    public ImmutableArray<GameSocketCompletionErrorEpilogueCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketCompletionErrorEpilogueMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketCompletionErrorEpilogueTraceStep> OrderedTrace { get; }
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketCompletionErrorEpilogueOutcome Outcome { get; }
    public GameSocketCompletionErrorEpilogueDisposition Disposition { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }
    public bool ProjectionEntered { get; }
    public bool NativeInstructionsExecuted => false;
    public bool NativeRetExecuted => false;
    public bool NativeRetTargetCaptured => false;
    public bool EaxReturnValueModeled => false;
    public bool ReachedBeforeNativeRet =>
        Outcome == GameSocketCompletionErrorEpilogueOutcome.BoundedBeforeNativeRet;
    public bool ExactBoundedCompletion =>
        ReachedBeforeNativeRet
        || (!ProjectionEntered && ParentPlan.HasActualBoundedCompletion);
    public bool IsCleanEpilogue =>
        ReachedBeforeNativeRet
        && ParentPlan.IsCleanCompletion
        && UnusedCaptures.IsEmpty
        && CaptureStates.PostQueueEbp == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.PreviousSeh == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.FirstPopEsp == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.PoppedEcx == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.PoppedEdi == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.PoppedEsi == GameSocketCompletionErrorEpilogueCaptureState.Captured
        && CaptureStates.SavedEbp == GameSocketCompletionErrorEpilogueCaptureState.Captured;

    public uint? PreviousSehReadAddress =>
        MemoryEffects.FirstOrDefault(effect =>
            effect.Kind == GameSocketCompletionErrorEpilogueMemoryEffectKind.PreviousSehRead)
        .Address;
    public uint? PoppedEcxAddress => StackAddressAt(FirstPopEcxRawFileOffset);
    public uint? PoppedEdiAddress => StackAddressAt(FirstPopEdiRawFileOffset);
    public uint? PoppedEsiAddress => StackAddressAt(FirstPopEsiRawFileOffset);
    public uint? SavedEbpAddress => StackAddressAt(PopSavedEbpRawFileOffset);
    public uint? RestoredEbp =>
        CaptureStates.SavedEbp == GameSocketCompletionErrorEpilogueCaptureState.Captured
            ? Captures.SavedEbp
            : null;
    public uint? EspBeforeNativeRet =>
        ReachedBeforeNativeRet && Captures.PostQueueEbp.HasValue
            ? unchecked(Captures.PostQueueEbp.Value + 4u)
            : null;

    private uint? StackAddressAt(uint instructionRawFileOffset) =>
        MemoryEffects.FirstOrDefault(effect =>
            effect.Kind == GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead
            && effect.InstructionRawFileOffset == instructionRawFileOffset).Address;

    public const uint FirstPopEcxRawFileOffset = 0x00BD2E23;
    public const uint FirstPopEdiRawFileOffset = 0x00BD2E24;
    public const uint FirstPopEsiRawFileOffset = 0x00BD2E25;
    public const uint PopSavedEbpRawFileOffset = 0x00BD2E28;
}

/// <summary>
/// Projects the small error epilogue after the parent queue-append return.
/// It uses captured values only and stops immediately before the native RET.
/// </summary>
public static class GameSocketCompletionErrorEpilogueProjection
{
    public const uint StartRawFileOffset =
        GameSocketCompletionErrorStateProjection.PostQueueAppendContinuationRawFileOffset;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint StartVirtualAddress =
        StartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const int InstructionSequenceByteLength = 19;
    public const int PreviousSehFieldOffset = -0x0C;
    public const uint PreviousSehLoadRawFileOffset = 0x00BD2E19;
    public const uint FsPreviousSehWriteRawFileOffset = 0x00BD2E1C;
    public const uint FirstPopRawFileOffset = 0x00BD2E23;
    public const uint PopEdiRawFileOffset = 0x00BD2E24;
    public const uint PopEsiRawFileOffset = 0x00BD2E25;
    public const uint RestoreEspFromEbpRawFileOffset = 0x00BD2E26;
    public const uint PopSavedEbpRawFileOffset = 0x00BD2E28;
    public const uint NativeRetRawFileOffset = 0x00BD2E29;
    public const uint FsSegmentOffset = 0;
    public const int PopDwordSize = 4;

    public static ImmutableArray<byte> InstructionBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x4D, 0xF4, 0x64, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
            0x59, 0x5F, 0x5E, 0x8B, 0xE5, 0x5D, 0xC2, 0x04, 0x00);

    private static readonly GameSocketCompletionErrorStateFlags UnknownFlags =
        new(null, null, null, null, null, null);

    public static GameSocketCompletionErrorEpiloguePlan Project(
        GameSocketCompletionErrorEpilogueCaptures captures)
    {
        GameSocketCompletionErrorStatePlan parentPlan =
            GameSocketCompletionErrorStateProjection.Project(captures.ParentError);
        var states = InitialCaptureStates(captures);
        var reached = ImmutableArray.CreateBuilder<uint>();
        var effects = ImmutableArray.CreateBuilder<GameSocketCompletionErrorEpilogueMemoryEffect>();

        if (!IsAdmissibleParent(captures.ParentError, parentPlan))
        {
            return Finish(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.ParentBoundary,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtParent,
                parentPlan.LastReachedRawFileOffset,
                parentPlan.StopBeforeRawFileOffset,
                parentPlan.FlagsAtStop,
                false);
        }

        reached.Add(PreviousSehLoadRawFileOffset);
        uint? sehAddress = captures.PostQueueEbp.HasValue
            ? unchecked(captures.PostQueueEbp.Value + (uint)PreviousSehFieldOffset)
            : null;
        effects.Add(new(
            GameSocketCompletionErrorEpilogueMemoryEffectKind.PreviousSehRead,
            sehAddress,
            captures.PostQueueEbp.HasValue ? captures.PreviousSeh : null,
            true,
            captures.PostQueueEbp.HasValue && captures.PreviousSeh.HasValue,
            false,
            PreviousSehLoadRawFileOffset,
            null,
            null,
            PreviousSehFieldOffset));
        states = states with { PostQueueEbp = CaptureState(captures.PostQueueEbp) };
        if (!captures.PostQueueEbp.HasValue)
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.MissingPostQueueEbp,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtPostQueueEbpRead,
                PreviousSehLoadRawFileOffset, PreviousSehLoadRawFileOffset);
        }

        states = states with { PreviousSeh = CaptureState(captures.PreviousSeh) };
        if (!captures.PreviousSeh.HasValue)
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.MissingPreviousSeh,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtPreviousSehRead,
                PreviousSehLoadRawFileOffset, PreviousSehLoadRawFileOffset);
        }

        reached.Add(FsPreviousSehWriteRawFileOffset);
        effects.Add(new(
            GameSocketCompletionErrorEpilogueMemoryEffectKind.SegmentWrite,
            null,
            captures.PreviousSeh.Value,
            true,
            true,
            true,
            FsPreviousSehWriteRawFileOffset,
            "FS",
            FsSegmentOffset,
            null));

        uint? firstPopAddress = captures.FirstPopEsp;
        states = states with { FirstPopEsp = CaptureState(captures.FirstPopEsp) };
        if (firstPopAddress.HasValue)
            states = states with { PoppedEcx = CaptureState(captures.PoppedEcx) };
        reached.Add(FirstPopRawFileOffset);
        if (!TryReadStack(
                captures.PoppedEcx,
                firstPopAddress,
                0,
                FirstPopRawFileOffset,
                effects))
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                captures.FirstPopEsp.HasValue
                    ? GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEcx
                    : GameSocketCompletionErrorEpilogueOutcome.MissingFirstPopEsp,
                captures.FirstPopEsp.HasValue
                    ? GameSocketCompletionErrorEpilogueDisposition.BoundedAtPopEcx
                    : GameSocketCompletionErrorEpilogueDisposition.BoundedAtFirstPop,
                FirstPopRawFileOffset, FirstPopRawFileOffset);
        }

        uint? ediAddress = AddStackOffset(firstPopAddress, PopDwordSize);
        states = states with { PoppedEdi = CaptureState(captures.PoppedEdi) };
        reached.Add(PopEdiRawFileOffset);
        if (!TryReadStack(captures.PoppedEdi, ediAddress, PopDwordSize, PopEdiRawFileOffset, effects))
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEdi,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtPopEdi,
                PopEdiRawFileOffset, PopEdiRawFileOffset);
        }

        uint? esiAddress = AddStackOffset(firstPopAddress, 2 * PopDwordSize);
        states = states with { PoppedEsi = CaptureState(captures.PoppedEsi) };
        reached.Add(PopEsiRawFileOffset);
        if (!TryReadStack(captures.PoppedEsi, esiAddress, 2 * PopDwordSize, PopEsiRawFileOffset, effects))
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.MissingPoppedEsi,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtPopEsi,
                PopEsiRawFileOffset, PopEsiRawFileOffset);
        }

        reached.Add(RestoreEspFromEbpRawFileOffset);

        uint? savedEbpAddress = captures.PostQueueEbp;
        states = states with { SavedEbp = CaptureState(captures.SavedEbp) };
        reached.Add(PopSavedEbpRawFileOffset);
        if (!TryReadStack(
                captures.SavedEbp,
                savedEbpAddress,
                0,
                PopSavedEbpRawFileOffset,
                effects))
        {
            return Missing(
                captures, parentPlan, states, reached, effects,
                GameSocketCompletionErrorEpilogueOutcome.MissingSavedEbp,
                GameSocketCompletionErrorEpilogueDisposition.BoundedAtPopEbp,
                PopSavedEbpRawFileOffset, PopSavedEbpRawFileOffset);
        }

        return Finish(
            captures, parentPlan, states, reached, effects,
            GameSocketCompletionErrorEpilogueOutcome.BoundedBeforeNativeRet,
            GameSocketCompletionErrorEpilogueDisposition.BoundedBeforeNativeRet,
            PopSavedEbpRawFileOffset, NativeRetRawFileOffset, UnknownFlags, true);
    }

    private static bool IsAdmissibleParent(
        GameSocketCompletionErrorStateCapture capture,
        GameSocketCompletionErrorStatePlan plan) =>
        plan.IsCleanCompletion
        && plan.HasActualBoundedCompletion
        && plan.Outcome == GameSocketCompletionErrorStateOutcome.BoundedAtQueueAppendReturn
        && plan.Disposition == GameSocketCompletionErrorStateDisposition.BoundedAfterQueueAppendReturn
        && plan.QueueAppendCallSiteReached
        && plan.QueueAppendReturnedNormally == true
        && capture.QueueAppendReturnedNormally == true
        && plan.CaptureStates.QueueAppendReturnedNormally
            == GameSocketCompletionErrorStateCaptureState.Captured
        && plan.LastReachedRawFileOffset
            == GameSocketCompletionErrorStateProjection.QueueAppendCallRawFileOffset
        && plan.StopBeforeRawFileOffset
            == GameSocketCompletionErrorStateProjection.PostQueueAppendContinuationRawFileOffset;

    private static GameSocketCompletionErrorEpiloguePlan Missing(
        GameSocketCompletionErrorEpilogueCaptures captures,
        GameSocketCompletionErrorStatePlan parentPlan,
        GameSocketCompletionErrorEpilogueCaptureStates states,
        ImmutableArray<uint>.Builder reached,
        ImmutableArray<GameSocketCompletionErrorEpilogueMemoryEffect>.Builder effects,
        GameSocketCompletionErrorEpilogueOutcome outcome,
        GameSocketCompletionErrorEpilogueDisposition disposition,
        uint lastReached,
        uint stopBefore) =>
        Finish(
            captures, parentPlan, states, reached, effects, outcome, disposition,
            lastReached, stopBefore, UnknownFlags, true);

    private static GameSocketCompletionErrorEpiloguePlan Finish(
        GameSocketCompletionErrorEpilogueCaptures captures,
        GameSocketCompletionErrorStatePlan parentPlan,
        GameSocketCompletionErrorEpilogueCaptureStates states,
        ImmutableArray<uint>.Builder reached,
        ImmutableArray<GameSocketCompletionErrorEpilogueMemoryEffect>.Builder effects,
        GameSocketCompletionErrorEpilogueOutcome outcome,
        GameSocketCompletionErrorEpilogueDisposition disposition,
        uint? lastReached,
        uint? stopBefore,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        bool projectionEntered)
    {
        var unused = ImmutableArray.CreateBuilder<GameSocketCompletionErrorEpilogueCaptureKind>();
        var orderedTrace =
            ImmutableArray.CreateBuilder<GameSocketCompletionErrorEpilogueTraceStep>(
                parentPlan.OrderedMemoryEffects.Length + effects.Count);
        foreach (GameSocketCompletionErrorStateMemoryEffect parentEffect
                 in parentPlan.OrderedMemoryEffects)
        {
            orderedTrace.Add(new(orderedTrace.Count, parentEffect, null));
        }

        foreach (GameSocketCompletionErrorEpilogueMemoryEffect epilogueEffect in effects)
        {
            orderedTrace.Add(new(orderedTrace.Count, null, epilogueEffect));
        }

        AddUnused(states.PostQueueEbp, GameSocketCompletionErrorEpilogueCaptureKind.PostQueueEbp, unused);
        AddUnused(states.PreviousSeh, GameSocketCompletionErrorEpilogueCaptureKind.PreviousSeh, unused);
        AddUnused(states.FirstPopEsp, GameSocketCompletionErrorEpilogueCaptureKind.FirstPopEsp, unused);
        AddUnused(states.PoppedEcx, GameSocketCompletionErrorEpilogueCaptureKind.PoppedEcx, unused);
        AddUnused(states.PoppedEdi, GameSocketCompletionErrorEpilogueCaptureKind.PoppedEdi, unused);
        AddUnused(states.PoppedEsi, GameSocketCompletionErrorEpilogueCaptureKind.PoppedEsi, unused);
        AddUnused(states.SavedEbp, GameSocketCompletionErrorEpilogueCaptureKind.SavedEbp, unused);
        return new(
            captures,
            parentPlan,
            states,
            unused.ToImmutable(),
            effects.ToImmutable(),
            orderedTrace.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            disposition,
            lastReached,
            stopBefore,
            flagsAtStop,
            projectionEntered);
    }

    private static bool TryReadStack(
        uint? value,
        uint? address,
        int stackOffset,
        uint instructionRawFileOffset,
        ImmutableArray<GameSocketCompletionErrorEpilogueMemoryEffect>.Builder effects)
    {
        effects.Add(new(
            GameSocketCompletionErrorEpilogueMemoryEffectKind.StackRead,
            address,
            address.HasValue ? value : null,
            true,
            address.HasValue && value.HasValue,
            false,
            instructionRawFileOffset,
            null,
            null,
            stackOffset));
        return address.HasValue && value.HasValue;
    }

    private static uint? AddStackOffset(uint? stackPointer, int offset) =>
        stackPointer.HasValue
            ? unchecked(stackPointer.Value + (uint)offset)
            : null;

    private static GameSocketCompletionErrorEpilogueCaptureStates InitialCaptureStates(
        GameSocketCompletionErrorEpilogueCaptures captures) =>
        new(
            InitialState(captures.PostQueueEbp),
            InitialState(captures.PreviousSeh),
            InitialState(captures.FirstPopEsp),
            InitialState(captures.PoppedEcx),
            InitialState(captures.PoppedEdi),
            InitialState(captures.PoppedEsi),
            InitialState(captures.SavedEbp));

    private static GameSocketCompletionErrorEpilogueCaptureState InitialState(uint? value) =>
        value.HasValue
            ? GameSocketCompletionErrorEpilogueCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorEpilogueCaptureState.NotReached;

    private static GameSocketCompletionErrorEpilogueCaptureState CaptureState(uint? value) =>
        value.HasValue
            ? GameSocketCompletionErrorEpilogueCaptureState.Captured
            : GameSocketCompletionErrorEpilogueCaptureState.Missing;

    private static void AddUnused(
        GameSocketCompletionErrorEpilogueCaptureState state,
        GameSocketCompletionErrorEpilogueCaptureKind kind,
        ImmutableArray<GameSocketCompletionErrorEpilogueCaptureKind>.Builder unused)
    {
        if (state == GameSocketCompletionErrorEpilogueCaptureState.SuppliedButUnused)
            unused.Add(kind);
    }

}
