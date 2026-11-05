using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserSelectorDiagnosticCounterContinuationCapture(
    GameSocketParserSelectorDiagnosticReturnCleanupCapture ParentCapture,
    uint? FreshPostCallESI,
    uint? FreshCounterDWORD);

public enum GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind
{
    FreshPostCallESI,
    FreshCounterDWORD
}

public enum GameSocketParserSelectorDiagnosticCounterContinuationOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshPostCallESI,
    MissingFreshCounterDWORD,
    BoundedBeforeBranchTarget
}

public enum GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
{
    IncrementDWORDAt3289,
    JumpTo3473At328F
}

public enum GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
{
    ParentPrefix,
    FreshCounterDWORDRead,
    CounterDWORDIncrement
}

public readonly record struct GameSocketParserSelectorDiagnosticCounterContinuationInstruction(
    GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? MemoryAddress,
    uint? ValueBefore,
    uint? ValueAfter,
    uint? TargetRawFileOffset,
    uint? TargetCodeVirtualAddress,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserSelectorDiagnosticCounterContinuationRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect(
    GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffectKind? ParentKind);

public sealed class GameSocketParserSelectorDiagnosticCounterContinuationPlan
{
    internal GameSocketParserSelectorDiagnosticCounterContinuationPlan(
        GameSocketParserSelectorDiagnosticCounterContinuationCapture capture,
        GameSocketParserSelectorDiagnosticReturnCleanupPlan parentPlan,
        ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationInstruction> instructions,
        ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationRead> reads,
        ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind> unusedCaptures,
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureState esiCaptureState,
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureState counterCaptureState,
        GameSocketParserSelectorDiagnosticCounterContinuationOutcome outcome,
        bool parentContinuationAdmitted,
        uint? resultESP,
        uint? counterAddress,
        uint? counterValueBefore,
        uint? counterValueAfter,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Reads = reads;
        MemoryEffects = memoryEffects;
        UnusedCaptures = unusedCaptures;
        FreshESICaptureState = esiCaptureState;
        CounterDWORDCaptureState = counterCaptureState;
        Outcome = outcome;
        ParentContinuationAdmitted = parentContinuationAdmitted;
        ResultESP = resultESP;
        CounterAddress = counterAddress;
        CounterValueBefore = counterValueBefore;
        CounterValueAfter = counterValueAfter;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserSelectorDiagnosticCounterContinuationCapture Capture { get; }
    public GameSocketParserSelectorDiagnosticReturnCleanupPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationInstruction>
        Instructions { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationRead> Reads { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect>
        MemoryEffects { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind>
        UnusedCaptures { get; }
    public GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
        FreshESICaptureState { get; }
    public GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
        CounterDWORDCaptureState { get; }
    public GameSocketParserSelectorDiagnosticCounterContinuationOutcome Outcome { get; }
    public bool ParentContinuationAdmitted { get; }
    public bool ParentPreparationCompleted => ParentPlan.ParentPreparationCompleted;
    public bool ParentExactBoundedCompletion => ParentPlan.ParentExactBoundedCompletion;
    public bool? NormalReturnObserved => ParentPlan.NormalReturnObserved;
    public bool? ResultNormalReturnObserved => ParentPlan.ResultNormalReturnObserved;
    public uint? ParentESP => ParentPlan.ParentESP;
    public uint? FreshPostCallESP => ParentPlan.FreshPostCallESP;
    public uint? FreshPostCallESI => Capture.FreshPostCallESI;
    public uint? FreshCounterDWORD => Capture.FreshCounterDWORD;
    public uint? CounterAddress { get; }
    public uint? CounterValueBefore { get; }
    public uint? CounterValueAfter { get; }
    public uint? ResultESP { get; }
    public uint? FinalESP => ResultESP;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? BranchTargetRawFileOffset =>
        GameSocketParserSelectorDiagnosticCounterContinuationProjection.BranchTargetRawFileOffset;
    public uint? BranchTargetCodeVirtualAddress =>
        GameSocketParserSelectorDiagnosticCounterContinuationProjection
            .BranchTargetCodeVirtualAddress;
    public bool BranchAttempted =>
        Instructions.Any(instruction =>
            instruction.Kind
                == GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .JumpTo3473At328F
            && instruction.Attempted);
    public bool BranchReached =>
        Instructions.Any(instruction =>
            instruction.Kind
                == GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .JumpTo3473At328F
            && instruction.Reached);
    public bool BranchCommitted =>
        Instructions.Any(instruction =>
            instruction.Kind
                == GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .JumpTo3473At328F
            && instruction.Committed);
    public bool BranchTargetInstructionExecuted => false;
    public bool CallTargetEntryObserved => false;
    public bool OpaqueCallEffectsModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool CounterIdentitySemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool LogicalMemoryEffectsOnly => true;
    public bool OrderedMemoryPrefixPreserved
    {
        get
        {
            ImmutableArray<GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect>
                parentEffects = ParentPlan.MemoryEffects;
            if (MemoryEffects.Length < parentEffects.Length)
            {
                return false;
            }

            for (int index = 0; index < parentEffects.Length; index++)
            {
                GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect parent =
                    parentEffects[index];
                GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect child =
                    MemoryEffects[index];
                if (child.Kind
                        != GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
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
        ParentContinuationAdmitted
            ? Outcome
                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .BoundedBeforeBranchTarget
                && StopBeforeRawFileOffset
                    == GameSocketParserSelectorDiagnosticCounterContinuationProjection
                        .BranchTargetRawFileOffset
            : ParentPlan.ReachedBoundedEndpoint
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactBoundedCompletion =>
        ParentContinuationAdmitted
            ? Outcome
                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .BoundedBeforeBranchTarget
                && StopBeforeRawFileOffset
                    == GameSocketParserSelectorDiagnosticCounterContinuationProjection
                        .BranchTargetRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool IsCleanCompletion =>
        ParentContinuationAdmitted
        && ParentPlan.IsCleanCompletion
        && Outcome
            == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                .BoundedBeforeBranchTarget
        && ExactBoundedCompletion
        && UnusedCaptures.IsEmpty;
}

/// <summary>
/// Reprojects the supplied suffix at 3289 through its unconditional branch target at 3473.
/// </summary>
public static class GameSocketParserSelectorDiagnosticCounterContinuationProjection
{
    public const uint IncrementRawFileOffset = 0x00BD3289;
    public const uint BranchRawFileOffset = 0x00BD328F;
    public const uint BranchTargetRawFileOffset = 0x00BD3473;
    public const uint CounterDisplacement = 0x000000EC;
    public const uint BranchDisplacement = 0x000001DF;
    public const string IncrementEncoding = "FF 86 EC 00 00 00";
    public const string BranchEncoding = "E9 DF 01 00 00";
    public const string CoordinatorProvidedBytes =
        "FF 86 EC 00 00 00 E9 DF 01 00 00";
    public const bool BytesIndependentlyVerified = false;

    public const uint BranchTargetCodeVirtualAddress =
        BranchTargetRawFileOffset
        + GameSocketParserCountConsumptionStoresProjection.RawToCodeVirtualAddressDelta;

    public static GameSocketParserSelectorDiagnosticCounterContinuationPlan Project(
        GameSocketParserSelectorDiagnosticCounterContinuationCapture capture)
    {
        GameSocketParserSelectorDiagnosticReturnCleanupPlan parentPlan =
            GameSocketParserSelectorDiagnosticReturnCleanupProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticCounterContinuationInstruction>();
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticCounterContinuationRead>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect>();
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind>();
        foreach (GameSocketParserSelectorDiagnosticReturnCleanupMemoryEffect parentEffect
            in parentPlan.MemoryEffects)
        {
            effects.Add(new(
                GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind.ParentPrefix,
                parentEffect.InstructionRawFileOffset,
                parentEffect.Address,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Captured,
                parentEffect.Committed,
                parentEffect.Kind));
        }

        bool parentContinuationAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome
                    .BoundedBeforeNextInstruction
            && parentPlan.StopBeforeRawFileOffset == IncrementRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.ResultNormalReturnObserved == true
            && parentPlan.FlagsAtStop.HasValue;
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureState esiState =
            parentContinuationAdmitted
                ? GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.NotReached
                : UnreachedState(capture.FreshPostCallESI);
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureState counterState =
            parentContinuationAdmitted
                ? GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.NotReached
                : UnreachedState(capture.FreshCounterDWORD);
        GameSocketParserSelectorDiagnosticCounterContinuationOutcome outcome;
        uint? resultESP = parentPlan.ResultESP;
        uint? counterAddress = null;
        uint? counterValueBefore = null;
        uint? counterValueAfter = null;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        uint stopBefore = parentPlan.StopBeforeRawFileOffset;

        if (!parentContinuationAdmitted)
        {
            outcome =
                GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .ParentContinuationNotAdmitted;
            AddUnusedIfSupplied(unused, esiState,
                GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind
                    .FreshPostCallESI);
            AddUnusedIfSupplied(unused, counterState,
                GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind
                    .FreshCounterDWORD);
            return CreatePlan();
        }

        stopBefore = IncrementRawFileOffset;
        esiState = CaptureState(capture.FreshPostCallESI);
        counterState =
            GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.NotReached;
        if (capture.FreshPostCallESI is not uint freshESI)
        {
            if (capture.FreshCounterDWORD.HasValue)
            {
                counterState =
                    GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                        .SuppliedButUnused;
                unused.Add(
                    GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind
                        .FreshCounterDWORD);
            }

            instructions.Add(new(
                GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .IncrementDWORDAt3289,
                IncrementRawFileOffset,
                ToCodeVA(IncrementRawFileOffset),
                IncrementEncoding,
                parentPlan.ResultESP,
                parentPlan.ResultESP,
                MemoryAddress: null,
                ValueBefore: null,
                ValueAfter: null,
                TargetRawFileOffset: null,
                TargetCodeVirtualAddress: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            outcome =
                GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .MissingFreshPostCallESI;
            return CreatePlan();
        }

        counterAddress = unchecked(freshESI + CounterDisplacement);
        counterState = CaptureState(capture.FreshCounterDWORD);
        reads.Add(new(
            IncrementRawFileOffset,
            counterAddress,
            capture.FreshCounterDWORD,
            Attempted: true,
            Captured: capture.FreshCounterDWORD.HasValue,
            Committed: false));
        effects.Add(new(
            GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                .FreshCounterDWORDRead,
            IncrementRawFileOffset,
            counterAddress,
            capture.FreshCounterDWORD,
            Attempted: true,
            Captured: capture.FreshCounterDWORD.HasValue,
            Committed: false,
            ParentKind: null));

        if (capture.FreshCounterDWORD is not uint oldValue)
        {
            instructions.Add(new(
                GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .IncrementDWORDAt3289,
                IncrementRawFileOffset,
                ToCodeVA(IncrementRawFileOffset),
                IncrementEncoding,
                parentPlan.ResultESP,
                parentPlan.ResultESP,
                counterAddress,
                ValueBefore: null,
                ValueAfter: null,
                TargetRawFileOffset: null,
                TargetCodeVirtualAddress: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            outcome =
                GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .MissingFreshCounterDWORD;
            return CreatePlan();
        }

        uint incrementedValue = unchecked(oldValue + 1);
        GameSocketParserStoredByteSelectorGateFlags incrementedFlags =
            IncrementFlags32(oldValue, incrementedValue, parentPlan.FlagsAtStop!.Value);
        counterValueBefore = oldValue;
        counterValueAfter = incrementedValue;
        instructions.Add(new(
            GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                .IncrementDWORDAt3289,
            IncrementRawFileOffset,
            ToCodeVA(IncrementRawFileOffset),
            IncrementEncoding,
            parentPlan.ResultESP,
            parentPlan.ResultESP,
            counterAddress,
            oldValue,
            incrementedValue,
            TargetRawFileOffset: null,
            TargetCodeVirtualAddress: null,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: incrementedFlags));
        effects.Add(new(
            GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                .CounterDWORDIncrement,
            IncrementRawFileOffset,
            counterAddress,
            incrementedValue,
            Attempted: true,
            Captured: true,
            Committed: true,
            ParentKind: null));

        instructions.Add(new(
            GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                .JumpTo3473At328F,
            BranchRawFileOffset,
            ToCodeVA(BranchRawFileOffset),
            BranchEncoding,
            parentPlan.ResultESP,
            parentPlan.ResultESP,
            MemoryAddress: null,
            ValueBefore: null,
            ValueAfter: null,
            TargetRawFileOffset: BranchTargetRawFileOffset,
            TargetCodeVirtualAddress: BranchTargetCodeVirtualAddress,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: incrementedFlags,
            FlagsAfter: incrementedFlags));
        flagsAtStop = incrementedFlags;
        outcome =
            GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                .BoundedBeforeBranchTarget;
        stopBefore = BranchTargetRawFileOffset;
        return CreatePlan();

        GameSocketParserSelectorDiagnosticCounterContinuationPlan CreatePlan() =>
            new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                reads.ToImmutable(),
                effects.ToImmutable(),
                unused.ToImmutable(),
                esiState,
                counterState,
                outcome,
                parentContinuationAdmitted,
                resultESP,
                counterAddress,
                counterValueBefore,
                counterValueAfter,
                flagsAtStop,
                stopBefore);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);

    private static GameSocketParserSelectorDiagnosticCounterContinuationCaptureState CaptureState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.Captured
            : GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.Missing;

    private static GameSocketParserSelectorDiagnosticCounterContinuationCaptureState UnreachedState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.SuppliedButUnused
            : GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.NotReached;

    private static void AddUnusedIfSupplied(
        ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind>.Builder
            unused,
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureState state,
        GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind kind)
    {
        if (state
            == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.SuppliedButUnused)
        {
            unused.Add(kind);
        }
    }

    private static GameSocketParserStoredByteSelectorGateFlags IncrementFlags32(
        uint oldValue,
        uint incrementedValue,
        GameSocketParserStoredByteSelectorGateFlags flagsBefore) =>
        new(
            Carry: flagsBefore.Carry,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)incrementedValue) & 1) == 0,
            AuxiliaryCarry: (oldValue & 0x0F) == 0x0F,
            Zero: incrementedValue == 0,
            Sign: (incrementedValue & 0x80000000) != 0,
            Overflow: oldValue == 0x7FFFFFFF);
}
