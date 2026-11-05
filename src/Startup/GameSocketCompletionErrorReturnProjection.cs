using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketCompletionErrorReturnCaptures(
    GameSocketCompletionErrorEpilogueCaptures ParentEpilogue,
    uint? ReturnTarget);

public enum GameSocketCompletionErrorReturnCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketCompletionErrorReturnOutcome
{
    ParentBoundary,
    MissingReturnTarget,
    BoundedAfterNativeRet
}

public enum GameSocketCompletionErrorReturnDisposition
{
    BoundedAtParent,
    BoundedBeforeNativeRet,
    BoundedAfterNativeRet
}

public enum GameSocketCompletionErrorReturnMemoryEffectKind
{
    ReturnTargetRead
}

public enum GameSocketCompletionErrorReturnTraceKind
{
    ParentEpilogue,
    ReturnTargetRead,
    LogicalTransfer
}

public readonly record struct GameSocketCompletionErrorReturnMemoryEffect(
    GameSocketCompletionErrorReturnMemoryEffectKind Kind,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool? Committed,
    uint InstructionRawFileOffset);

public readonly record struct GameSocketCompletionErrorReturnTransfer(
    uint TargetVirtualAddress,
    uint EspBeforeRet,
    uint EspAfterRet,
    uint ReturnAddressBytes,
    uint ArgumentBytes,
    uint InstructionRawFileOffset);

public readonly record struct GameSocketCompletionErrorReturnTraceStep(
    int Sequence,
    GameSocketCompletionErrorReturnTraceKind Kind,
    GameSocketCompletionErrorEpilogueTraceStep? ParentEpilogueTraceStep,
    GameSocketCompletionErrorReturnMemoryEffect? ReturnMemoryEffect,
    GameSocketCompletionErrorReturnTransfer? Transfer);

public sealed class GameSocketCompletionErrorReturnPlan
{
    internal GameSocketCompletionErrorReturnPlan(
        GameSocketCompletionErrorReturnCaptures captures,
        GameSocketCompletionErrorEpiloguePlan parentEpilogue,
        GameSocketCompletionErrorReturnCaptureState returnTargetCaptureState,
        ImmutableArray<GameSocketCompletionErrorReturnMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketCompletionErrorReturnTraceStep> orderedTrace,
        GameSocketCompletionErrorReturnTransfer? transfer,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketCompletionErrorReturnOutcome outcome,
        GameSocketCompletionErrorReturnDisposition disposition,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        bool projectionEntered)
    {
        Captures = captures;
        ParentEpilogue = parentEpilogue;
        ReturnTargetCaptureState = returnTargetCaptureState;
        MemoryEffects = memoryEffects;
        OrderedTrace = orderedTrace;
        Transfer = transfer;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        Disposition = disposition;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        FlagsAtStop = flagsAtStop;
        ProjectionEntered = projectionEntered;
    }

    public GameSocketCompletionErrorReturnCaptures Captures { get; }
    public GameSocketCompletionErrorEpiloguePlan ParentEpilogue { get; }
    public GameSocketCompletionErrorReturnCaptureState ReturnTargetCaptureState { get; }
    public ImmutableArray<GameSocketCompletionErrorReturnMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketCompletionErrorReturnTraceStep> OrderedTrace { get; }
    public GameSocketCompletionErrorReturnTransfer? Transfer { get; }
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketCompletionErrorReturnOutcome Outcome { get; }
    public GameSocketCompletionErrorReturnDisposition Disposition { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public GameSocketCompletionErrorStateFlags? FlagsAtStop { get; }
    public bool ProjectionEntered { get; }
    public bool NativeInstructionsExecuted => false;
    public bool NativeRetExecuted => false;
    public bool TargetEntryExecuted => false;
    public bool CallerReturnAddressMappingKnown => false;
    public bool NativeRetTargetCaptured =>
        ReturnTargetCaptureState == GameSocketCompletionErrorReturnCaptureState.Captured;
    public bool ExactBoundedCompletion =>
        Outcome == GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet
        || (!ProjectionEntered && ParentEpilogue.ExactBoundedCompletion);
    public bool IsCleanReturn =>
        Outcome == GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet
        && ParentEpilogue.IsCleanEpilogue
        && ReturnTargetCaptureState == GameSocketCompletionErrorReturnCaptureState.Captured;

    public uint? EspBeforeRet =>
        ProjectionEntered ? ParentEpilogue.EspBeforeNativeRet : null;

    public uint? ReturnTargetReadAddress =>
        MemoryEffects.FirstOrDefault(effect =>
            effect.Kind == GameSocketCompletionErrorReturnMemoryEffectKind.ReturnTargetRead).Address;

    public uint? TargetVirtualAddress =>
        ReturnTargetCaptureState == GameSocketCompletionErrorReturnCaptureState.Captured
            ? Captures.ReturnTarget
            : null;

    public uint? EspAfterRet =>
        Outcome == GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet
        && EspBeforeRet.HasValue
            ? unchecked(EspBeforeRet.Value + ReturnAddressBytes + ArgumentBytes)
            : null;

    public const uint NativeRetRawFileOffset = 0x00BD2E29;
    public const uint AfterNativeRetRawFileOffset = 0x00BD2E2C;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint NativeRetVirtualAddress =
        NativeRetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint ReturnAddressBytes = 4;
    public const uint ArgumentBytes = 4;
    public const int ReturnTargetStackOffset = 4;
    public const int NativeRetInstructionByteLength = 3;

    public static ImmutableArray<byte> NativeRetInstructionBytes { get; } =
        ImmutableArray.Create<byte>(0xC2, 0x04, 0x00);
}

/// <summary>
/// Projects the bounded logical effect of the error epilogue's RET 4.
/// Captured values are reprojected without reading memory or entering the target.
/// </summary>
public static class GameSocketCompletionErrorReturnProjection
{
    public const uint NativeRetRawFileOffset =
        GameSocketCompletionErrorReturnPlan.NativeRetRawFileOffset;
    public const uint AfterNativeRetRawFileOffset =
        GameSocketCompletionErrorReturnPlan.AfterNativeRetRawFileOffset;
    public const uint NativeRetVirtualAddress =
        GameSocketCompletionErrorReturnPlan.NativeRetVirtualAddress;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        GameSocketCompletionErrorReturnPlan.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint ReturnAddressBytes = GameSocketCompletionErrorReturnPlan.ReturnAddressBytes;
    public const uint ArgumentBytes = GameSocketCompletionErrorReturnPlan.ArgumentBytes;
    public const int ReturnTargetStackOffset =
        GameSocketCompletionErrorReturnPlan.ReturnTargetStackOffset;
    public const int NativeRetInstructionByteLength =
        GameSocketCompletionErrorReturnPlan.NativeRetInstructionByteLength;

    public static ImmutableArray<byte> NativeRetInstructionBytes { get; } =
        GameSocketCompletionErrorReturnPlan.NativeRetInstructionBytes;

    private static readonly GameSocketCompletionErrorStateFlags UnknownFlags =
        new(null, null, null, null, null, null);

    public static GameSocketCompletionErrorReturnPlan Project(
        GameSocketCompletionErrorReturnCaptures captures)
    {
        GameSocketCompletionErrorEpiloguePlan parentEpilogue =
            GameSocketCompletionErrorEpilogueProjection.Project(captures.ParentEpilogue);

        if (!IsAdmissibleParent(parentEpilogue))
        {
            return Finish(
                captures,
                parentEpilogue,
                InitialCaptureState(captures.ReturnTarget),
                ImmutableArray<GameSocketCompletionErrorReturnMemoryEffect>.Empty,
                ImmutableArray<uint>.Empty,
                GameSocketCompletionErrorReturnOutcome.ParentBoundary,
                GameSocketCompletionErrorReturnDisposition.BoundedAtParent,
                parentEpilogue.LastReachedRawFileOffset,
                parentEpilogue.StopBeforeRawFileOffset,
                parentEpilogue.FlagsAtStop,
                false);
        }

        uint? returnTargetAddress = parentEpilogue.EspBeforeNativeRet;
        bool captured = returnTargetAddress.HasValue && captures.ReturnTarget.HasValue;
        var memoryEffect = new GameSocketCompletionErrorReturnMemoryEffect(
            GameSocketCompletionErrorReturnMemoryEffectKind.ReturnTargetRead,
            returnTargetAddress,
            returnTargetAddress.HasValue ? captures.ReturnTarget : null,
            true,
            captured,
            false,
            NativeRetRawFileOffset);
        var effects = ImmutableArray.Create(memoryEffect);

        if (!captured)
        {
            return Finish(
                captures,
                parentEpilogue,
                returnTargetAddress.HasValue
                    ? GameSocketCompletionErrorReturnCaptureState.Missing
                    : GameSocketCompletionErrorReturnCaptureState.NotReached,
                effects,
                ImmutableArray<uint>.Empty,
                GameSocketCompletionErrorReturnOutcome.MissingReturnTarget,
                GameSocketCompletionErrorReturnDisposition.BoundedBeforeNativeRet,
                parentEpilogue.LastReachedRawFileOffset,
                NativeRetRawFileOffset,
                UnknownFlags,
                true);
        }

        var transfer = new GameSocketCompletionErrorReturnTransfer(
            captures.ReturnTarget!.Value,
            parentEpilogue.EspBeforeNativeRet!.Value,
            unchecked(parentEpilogue.EspBeforeNativeRet.Value + ReturnAddressBytes + ArgumentBytes),
            ReturnAddressBytes,
            ArgumentBytes,
            NativeRetRawFileOffset);

        return Finish(
            captures,
            parentEpilogue,
            GameSocketCompletionErrorReturnCaptureState.Captured,
            effects,
            ImmutableArray.Create(NativeRetRawFileOffset),
            GameSocketCompletionErrorReturnOutcome.BoundedAfterNativeRet,
            GameSocketCompletionErrorReturnDisposition.BoundedAfterNativeRet,
            NativeRetRawFileOffset,
            AfterNativeRetRawFileOffset,
            UnknownFlags,
            true,
            transfer);
    }

    private static bool IsAdmissibleParent(GameSocketCompletionErrorEpiloguePlan plan) =>
        plan.IsCleanEpilogue
        && plan.ReachedBeforeNativeRet
        && plan.Outcome == GameSocketCompletionErrorEpilogueOutcome.BoundedBeforeNativeRet
        && plan.StopBeforeRawFileOffset == NativeRetRawFileOffset
        && plan.LastReachedRawFileOffset
            == GameSocketCompletionErrorEpiloguePlan.PopSavedEbpRawFileOffset;

    private static GameSocketCompletionErrorReturnCaptureState InitialCaptureState(uint? value) =>
        value.HasValue
            ? GameSocketCompletionErrorReturnCaptureState.SuppliedButUnused
            : GameSocketCompletionErrorReturnCaptureState.NotReached;

    private static GameSocketCompletionErrorReturnPlan Finish(
        GameSocketCompletionErrorReturnCaptures captures,
        GameSocketCompletionErrorEpiloguePlan parentEpilogue,
        GameSocketCompletionErrorReturnCaptureState captureState,
        ImmutableArray<GameSocketCompletionErrorReturnMemoryEffect> effects,
        ImmutableArray<uint> reached,
        GameSocketCompletionErrorReturnOutcome outcome,
        GameSocketCompletionErrorReturnDisposition disposition,
        uint? lastReached,
        uint? stopBefore,
        GameSocketCompletionErrorStateFlags? flagsAtStop,
        bool projectionEntered,
        GameSocketCompletionErrorReturnTransfer? transfer = null)
    {
        var trace = ImmutableArray.CreateBuilder<GameSocketCompletionErrorReturnTraceStep>(
            parentEpilogue.OrderedTrace.Length + effects.Length + (transfer.HasValue ? 1 : 0));
        foreach (GameSocketCompletionErrorEpilogueTraceStep parentStep in parentEpilogue.OrderedTrace)
        {
            trace.Add(new(
                trace.Count,
                GameSocketCompletionErrorReturnTraceKind.ParentEpilogue,
                parentStep,
                null,
                null));
        }

        foreach (GameSocketCompletionErrorReturnMemoryEffect effect in effects)
        {
            trace.Add(new(
                trace.Count,
                GameSocketCompletionErrorReturnTraceKind.ReturnTargetRead,
                null,
                effect,
                null));
        }

        if (transfer.HasValue)
        {
            trace.Add(new(
                trace.Count,
                GameSocketCompletionErrorReturnTraceKind.LogicalTransfer,
                null,
                null,
                transfer));
        }

        return new(
            captures,
            parentEpilogue,
            captureState,
            effects,
            trace.ToImmutable(),
            transfer,
            reached,
            outcome,
            disposition,
            lastReached,
            stopBefore,
            flagsAtStop,
            projectionEntered);
    }
}
