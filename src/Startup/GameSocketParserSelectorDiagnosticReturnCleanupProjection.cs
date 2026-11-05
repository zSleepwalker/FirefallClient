using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserSelectorDiagnosticReturnCleanupCapture(
    GameSocketParserSelectorDiagnosticPreparationCapture ParentCapture,
    bool? NormalReturnObserved,
    uint? FreshPostCallESP);

public enum GameSocketParserSelectorDiagnosticReturnCleanupCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind
{
    FreshPostCallESP
}

public enum GameSocketParserSelectorDiagnosticReturnCleanupOutcome
{
    ParentHandoffNotAdmitted,
    CallPending,
    NonNormalReturn,
    MissingFreshPostCallESP,
    BoundedBeforeNextInstruction
}

public enum GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind
{
    OpaqueDirectCallAt3281,
    AddESP14At3286
}

public enum GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffectKind
{
    ParentPrefix
}

public readonly record struct GameSocketParserSelectorDiagnosticReturnCleanupInstruction(
    GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool? NormalReturnObserved,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect(
    GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind? ParentKind);

public sealed class GameSocketParserSelectorDiagnosticReturnCleanupPlan
{
    internal GameSocketParserSelectorDiagnosticReturnCleanupPlan(
        GameSocketParserSelectorDiagnosticReturnCleanupCapture capture,
        GameSocketParserSelectorDiagnosticPreparationPlan parentPlan,
        ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupInstruction> instructions,
        ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind> unusedCaptures,
        GameSocketParserSelectorDiagnosticReturnCleanupCaptureState postCallESPCaptureState,
        GameSocketParserSelectorDiagnosticReturnCleanupOutcome outcome,
        bool parentHandoffAdmitted,
        bool? resultNormalReturnObserved,
        uint? resultESP,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        MemoryEffects = memoryEffects;
        UnusedCaptures = unusedCaptures;
        PostCallESPCaptureState = postCallESPCaptureState;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        ResultNormalReturnObserved = resultNormalReturnObserved;
        ResultESP = resultESP;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserSelectorDiagnosticReturnCleanupCapture Capture { get; }
    public GameSocketParserSelectorDiagnosticPreparationPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupInstruction> Instructions { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind> UnusedCaptures { get; }
    public GameSocketParserSelectorDiagnosticReturnCleanupCaptureState PostCallESPCaptureState { get; }
    public GameSocketParserSelectorDiagnosticReturnCleanupOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public bool ParentPreparationCompleted => ParentPlan.IsCleanCompletion;
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool? NormalReturnObserved => Capture.NormalReturnObserved;
    public bool? ResultNormalReturnObserved { get; }
    public uint? ParentESP => ParentPlan.ResultESP;
    public uint? FreshPostCallESP => Capture.FreshPostCallESP;
    public uint? ResultESP { get; }
    public uint? FinalESP => ResultESP;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? CallTargetRawFileOffset =>
        GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallTargetRawFileOffset;
    public uint? CallTargetCodeVirtualAddress =>
        GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallTargetCodeVirtualAddress;
    public uint? AddResultESP => ResultESP;
    public bool CallAttempted =>
        Instructions.Any(instruction =>
            instruction.Kind
                == GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind
                    .OpaqueDirectCallAt3281
            && instruction.Attempted);
    public bool CallReturnedNormally => ResultNormalReturnObserved == true;
    public bool NormalReturnObservationSuppliedButUnused =>
        !ParentHandoffAdmitted && Capture.NormalReturnObserved.HasValue;
    public bool CallTargetEntryObserved => false;
    public bool OpaqueCallEffectsModeled => false;
    public bool NativeCallStackWriteModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool LogicalMemoryEffectsOnly => true;
    public bool OrderedMemoryPrefixPreserved
    {
        get
        {
            ImmutableArray<GameSocketParserSelectorDiagnosticPreparationMemoryEffect> parentEffects =
                ParentPlan.OrderedMemoryEffects;
            if (MemoryEffects.Length != parentEffects.Length)
            {
                return false;
            }

            for (int index = 0; index < parentEffects.Length; index++)
            {
                GameSocketParserSelectorDiagnosticPreparationMemoryEffect parent =
                    parentEffects[index];
                GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect child =
                    MemoryEffects[index];
                if (child.Kind
                        != GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffectKind
                            .ParentPrefix
                    || child.ParentKind != parent.Kind
                    || child.InstructionRawFileOffset != parent.InstructionRawFileOffset
                    || child.Address != parent.Address
                    || child.Value != parent.Value
                    || child.Attempted != parent.Attempted
                    || child.Captured != parent.Captured
                    || child.Committed != parent.Committed)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public bool ReachedBoundedEndpoint =>
        ParentHandoffAdmitted
            ? Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome
                    .BoundedBeforeNextInstruction
                && StopBeforeRawFileOffset
                    == GameSocketParserSelectorDiagnosticReturnCleanupProjection
                        .AfterAddRawFileOffset
            : ParentPlan.ReachedBoundedEndpoint
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactBoundedCompletion =>
        ParentHandoffAdmitted
            ? Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome
                    .BoundedBeforeNextInstruction
                && StopBeforeRawFileOffset
                    == GameSocketParserSelectorDiagnosticReturnCleanupProjection
                        .AfterAddRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool IsCleanCompletion =>
        ParentHandoffAdmitted
        && ParentPlan.IsCleanCompletion
        && Outcome
            == GameSocketParserSelectorDiagnosticReturnCleanupOutcome
                .BoundedBeforeNextInstruction
        && ExactBoundedCompletion
        && UnusedCaptures.IsEmpty;
}

/// <summary>
/// Records the opaque diagnostic call's observed return state and models only the
/// following ADD ESP,14h when a fresh post-call ESP is supplied.
/// </summary>
public static class GameSocketParserSelectorDiagnosticReturnCleanupProjection
{
    public const uint CallRawFileOffset = 0x00BD3281;
    public const uint CallReturnRawFileOffset = 0x00BD3286;
    public const uint AddESP14RawFileOffset = 0x00BD3286;
    public const uint AfterAddRawFileOffset = 0x00BD3289;
    public const uint CallTargetRawFileOffset = 0x00BE2F50;
    public const uint CallDisplacement = 0x0000FCCA;
    public const uint ESPAdjustment = 0x00000014;
    public const string CallEncoding = "E8 CA FC 00 00";
    public const string AddEncoding = "83 C4 14";
    public const uint CallTargetCodeVirtualAddress =
        CallTargetRawFileOffset
        + GameSocketParserCountConsumptionStoresProjection.RawToCodeVirtualAddressDelta;

    public static GameSocketParserSelectorDiagnosticReturnCleanupPlan Project(
        GameSocketParserSelectorDiagnosticReturnCleanupCapture capture)
    {
        GameSocketParserSelectorDiagnosticPreparationPlan parentPlan =
            GameSocketParserSelectorDiagnosticPreparationProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticReturnCleanupInstruction>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect>();
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind>();
        foreach (GameSocketParserSelectorDiagnosticPreparationMemoryEffect parentEffect
            in parentPlan.OrderedMemoryEffects)
        {
            effects.Add(new(
                GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffectKind.ParentPrefix,
                parentEffect.InstructionRawFileOffset,
                parentEffect.Address,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Captured,
                parentEffect.Committed,
                parentEffect.Kind));
        }

        bool parentHandoffAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall
            && parentPlan.StopBeforeRawFileOffset == CallRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.ExactlyFivePushesCommitted;
        GameSocketParserSelectorDiagnosticReturnCleanupCaptureState espState =
            parentHandoffAdmitted
                ? GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.NotReached
                : UnreachedState(capture.FreshPostCallESP);
        GameSocketParserSelectorDiagnosticReturnCleanupOutcome outcome;
        bool? resultNormalReturnObserved = null;
        uint? resultESP = null;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome =
                GameSocketParserSelectorDiagnosticReturnCleanupOutcome.ParentHandoffNotAdmitted;
            resultESP = parentPlan.ResultESP;
            stopBefore = parentPlan.StopBeforeRawFileOffset;
            AddUnusedIfSupplied(unused, espState);
            return CreatePlan();
        }

        resultNormalReturnObserved = capture.NormalReturnObserved;
        resultESP = null;
        instructions.Add(new(
            GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind
                .OpaqueDirectCallAt3281,
            CallRawFileOffset,
            ToCodeVA(CallRawFileOffset),
            CallEncoding,
            parentPlan.ResultESP,
            StackPointerAfter: null,
            Attempted: true,
            Reached: true,
            Committed: capture.NormalReturnObserved == true,
            NormalReturnObserved: capture.NormalReturnObserved,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: capture.NormalReturnObserved == true ? null : parentPlan.FlagsAtStop));

        if (capture.NormalReturnObserved is null)
        {
            outcome = GameSocketParserSelectorDiagnosticReturnCleanupOutcome.CallPending;
            stopBefore = CallRawFileOffset;
            espState = UnreachedState(capture.FreshPostCallESP);
            AddUnusedIfSupplied(unused, espState);
            return CreatePlan();
        }

        if (capture.NormalReturnObserved == false)
        {
            outcome = GameSocketParserSelectorDiagnosticReturnCleanupOutcome.NonNormalReturn;
            stopBefore = CallRawFileOffset;
            espState = UnreachedState(capture.FreshPostCallESP);
            AddUnusedIfSupplied(unused, espState);
            return CreatePlan();
        }

        flagsAtStop = null;
        espState = CaptureState(capture.FreshPostCallESP);
        if (capture.FreshPostCallESP is not uint postCallESP)
        {
            instructions.Add(new(
                GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind.AddESP14At3286,
                AddESP14RawFileOffset,
                ToCodeVA(AddESP14RawFileOffset),
                AddEncoding,
                StackPointerBefore: null,
                StackPointerAfter: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                NormalReturnObserved: true,
                FlagsBefore: null,
                FlagsAfter: null));
            outcome =
                GameSocketParserSelectorDiagnosticReturnCleanupOutcome.MissingFreshPostCallESP;
            stopBefore = AddESP14RawFileOffset;
            return CreatePlan();
        }

        uint adjustedESP = unchecked(postCallESP + ESPAdjustment);
        GameSocketParserStoredByteSelectorGateFlags adjustedFlags =
            AddFlags32(postCallESP, ESPAdjustment, adjustedESP);
        instructions.Add(new(
            GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind.AddESP14At3286,
            AddESP14RawFileOffset,
            ToCodeVA(AddESP14RawFileOffset),
            AddEncoding,
            postCallESP,
            adjustedESP,
            Attempted: true,
            Reached: true,
            Committed: true,
            NormalReturnObserved: true,
            FlagsBefore: null,
            FlagsAfter: adjustedFlags));
        resultESP = adjustedESP;
        flagsAtStop = adjustedFlags;
        outcome =
            GameSocketParserSelectorDiagnosticReturnCleanupOutcome.BoundedBeforeNextInstruction;
        stopBefore = AfterAddRawFileOffset;
        return CreatePlan();

        GameSocketParserSelectorDiagnosticReturnCleanupPlan CreatePlan() =>
            new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                effects.ToImmutable(),
                unused.ToImmutable(),
                espState,
                outcome,
                parentHandoffAdmitted,
                resultNormalReturnObserved,
                resultESP,
                flagsAtStop,
                stopBefore);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);

    private static GameSocketParserSelectorDiagnosticReturnCleanupCaptureState CaptureState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.Captured
            : GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.Missing;

    private static GameSocketParserSelectorDiagnosticReturnCleanupCaptureState UnreachedState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused
            : GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.NotReached;

    private static void AddUnusedIfSupplied(
        ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind>.Builder unused,
        GameSocketParserSelectorDiagnosticReturnCleanupCaptureState state)
    {
        if (state
            == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused)
        {
            unused.Add(
                GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind.FreshPostCallESP);
        }
    }

    private static GameSocketParserStoredByteSelectorGateFlags AddFlags32(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: (ulong)left + right > uint.MaxValue,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((~(left ^ right)) & (left ^ result) & 0x80000000) != 0);
}
