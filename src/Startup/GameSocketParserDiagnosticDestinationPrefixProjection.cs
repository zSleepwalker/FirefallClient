using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticDestinationPrefixCapture(
    GameSocketParserSelectorDiagnosticCounterContinuationCapture ParentCapture,
    uint? FreshPostCallEBP,
    uint? FreshDWORDAtEBPPlusC,
    uint? FreshDWORDAtESPPlus14);

public enum GameSocketParserDiagnosticDestinationPrefixCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticDestinationPrefixCaptureKind
{
    FreshPostCallEBP,
    FreshDWORDAtEBPPlusC,
    FreshDWORDAtESPPlus14
}

public enum GameSocketParserDiagnosticDestinationPrefixOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshPostCallEBP,
    MissingFreshDWORDAtEBPPlusC,
    MissingFreshDWORDAtESPPlus14,
    BoundedBeforeJaeAt3480
}

public enum GameSocketParserDiagnosticDestinationPrefixInstructionKind
{
    CompareDWORDAtEBPPlusCWith2At3473,
    MoveEDIImmediateAt3477,
    MoveEDXFromESPPlus14At347C
}

public enum GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind
{
    ParentPrefix,
    FreshDWORDAtEBPPlusCRead,
    FreshDWORDAtESPPlus14Read
}

public readonly record struct GameSocketParserDiagnosticDestinationPrefixInstruction(
    GameSocketParserDiagnosticDestinationPrefixInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? MemoryAddress,
    uint? MemoryValue,
    string? DestinationRegister,
    uint? DestinationValue,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticDestinationPrefixRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserDiagnosticDestinationPrefixMemoryEffect(
    GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind? ParentKind);

public sealed class GameSocketParserDiagnosticDestinationPrefixPlan
{
    internal GameSocketParserDiagnosticDestinationPrefixPlan(
        GameSocketParserDiagnosticDestinationPrefixCapture capture,
        GameSocketParserSelectorDiagnosticCounterContinuationPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixInstruction> instructions,
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixRead> reads,
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixCaptureKind> unusedCaptures,
        GameSocketParserDiagnosticDestinationPrefixCaptureState ebpCaptureState,
        GameSocketParserDiagnosticDestinationPrefixCaptureState comparisonCaptureState,
        GameSocketParserDiagnosticDestinationPrefixCaptureState stackDWORDCaptureState,
        GameSocketParserDiagnosticDestinationPrefixOutcome outcome,
        bool parentContinuationAdmitted,
        uint? resultESP,
        uint? comparisonAddress,
        uint? stackDWORDAddress,
        uint? ediValue,
        uint? edxValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Reads = reads;
        MemoryEffects = memoryEffects;
        UnusedCaptures = unusedCaptures;
        FreshPostCallEBPCaptureState = ebpCaptureState;
        DWORDAtEBPPlusCCaptureState = comparisonCaptureState;
        DWORDAtESPPlus14CaptureState = stackDWORDCaptureState;
        Outcome = outcome;
        ParentContinuationAdmitted = parentContinuationAdmitted;
        ResultESP = resultESP;
        ComparisonAddress = comparisonAddress;
        StackDWORDAddress = stackDWORDAddress;
        EDIValue = ediValue;
        EDXValue = edxValue;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticDestinationPrefixCapture Capture { get; }
    public GameSocketParserSelectorDiagnosticCounterContinuationPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixInstruction>
        Instructions { get; }
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixRead> Reads { get; }
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixMemoryEffect>
        MemoryEffects { get; }
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixCaptureKind>
        UnusedCaptures { get; }
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        FreshPostCallEBPCaptureState { get; }
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        DWORDAtEBPPlusCCaptureState { get; }
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        DWORDAtESPPlus14CaptureState { get; }
    public GameSocketParserDiagnosticDestinationPrefixOutcome Outcome { get; }
    public bool ParentContinuationAdmitted { get; }
    public bool ParentPreparationCompleted => ParentPlan.ParentPreparationCompleted;
    public bool ParentExactBoundedCompletion => ParentPlan.ParentExactBoundedCompletion;
    public bool? NormalReturnObserved => ParentPlan.NormalReturnObserved;
    public bool? ResultNormalReturnObserved => ParentPlan.ResultNormalReturnObserved;
    public uint? ParentESP => ParentPlan.ParentESP;
    public uint? FreshPostCallESP => ParentPlan.FreshPostCallESP;
    public uint? FreshPostCallEBP => Capture.FreshPostCallEBP;
    public uint? ResultESP { get; }
    public uint? FinalESP => ResultESP;
    public uint? ComparisonAddress { get; }
    public uint? StackDWORDAddress { get; }
    public uint? EDIValue { get; }
    public uint? EDXValue { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool JaeAttempted => false;
    public bool JaeReached => false;
    public bool JaeCommitted => false;
    public bool JaeDestinationModeled => false;
    public bool BranchTargetInstructionExecuted => false;
    public bool EBPIdentitySemanticsModeled => false;
    public bool AliasedReadsReconciled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool LogicalMemoryEffectsOnly => true;
    public bool OrderedMemoryPrefixPreserved
    {
        get
        {
            ImmutableArray<GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect>
                parentEffects = ParentPlan.MemoryEffects;
            if (MemoryEffects.Length < parentEffects.Length)
            {
                return false;
            }

            for (int index = 0; index < parentEffects.Length; index++)
            {
                GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect parent =
                    parentEffects[index];
                GameSocketParserDiagnosticDestinationPrefixMemoryEffect child =
                    MemoryEffects[index];
                if (child.Kind
                        != GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind.ParentPrefix
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
                == GameSocketParserDiagnosticDestinationPrefixOutcome.BoundedBeforeJaeAt3480
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticDestinationPrefixProjection.JaeRawFileOffset
            : ParentPlan.ReachedBoundedEndpoint
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactBoundedCompletion =>
        ParentContinuationAdmitted
            ? Outcome
                == GameSocketParserDiagnosticDestinationPrefixOutcome.BoundedBeforeJaeAt3480
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticDestinationPrefixProjection.JaeRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool IsCleanCompletion =>
        ParentContinuationAdmitted
        && ParentPlan.IsCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticDestinationPrefixOutcome.BoundedBeforeJaeAt3480
        && ExactBoundedCompletion
        && UnusedCaptures.IsEmpty;
}

/// <summary>
/// Reprojects the observed prefix at 3473 and stops before the JAE instruction at 3480.
/// </summary>
public static class GameSocketParserDiagnosticDestinationPrefixProjection
{
    public const uint CompareRawFileOffset = 0x00BD3473;
    public const uint MoveEDIRawFileOffset = 0x00BD3477;
    public const uint MoveEDXRawFileOffset = 0x00BD347C;
    public const uint JaeRawFileOffset = 0x00BD3480;
    public const uint EBPDisplacement = 0x0000000C;
    public const uint ESPDisplacement = 0x00000014;
    public const uint EDIImmediate = 0x0000056E;
    public const uint CompareImmediate = 2;
    public const string CompareEncoding = "83 7D 0C 02";
    public const string MoveEDIEncoding = "BF 6E 05 00 00";
    public const string MoveEDXEncoding = "8B 54 24 14";
    public const string CoordinatorProvidedBytes =
        "83 7D 0C 02 BF 6E 05 00 00 8B 54 24 14 0F 83 72 FD FF FF EB 25";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticDestinationPrefixPlan Project(
        GameSocketParserDiagnosticDestinationPrefixCapture capture)
    {
        GameSocketParserSelectorDiagnosticCounterContinuationPlan parentPlan =
            GameSocketParserSelectorDiagnosticCounterContinuationProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticDestinationPrefixInstruction>();
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticDestinationPrefixRead>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticDestinationPrefixMemoryEffect>();
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticDestinationPrefixCaptureKind>();
        foreach (GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffect parentEffect
            in parentPlan.MemoryEffects)
        {
            effects.Add(new(
                GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind.ParentPrefix,
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
                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .BoundedBeforeBranchTarget
            && parentPlan.StopBeforeRawFileOffset == CompareRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.ResultNormalReturnObserved == true
            && parentPlan.FlagsAtStop.HasValue;
        GameSocketParserDiagnosticDestinationPrefixCaptureState ebpState =
            parentContinuationAdmitted
                ? GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached
                : UnreachedState(capture.FreshPostCallEBP);
        GameSocketParserDiagnosticDestinationPrefixCaptureState comparisonState =
            parentContinuationAdmitted
                ? GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached
                : UnreachedState(capture.FreshDWORDAtEBPPlusC);
        GameSocketParserDiagnosticDestinationPrefixCaptureState stackDWORDState =
            parentContinuationAdmitted
                ? GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached
                : UnreachedState(capture.FreshDWORDAtESPPlus14);
        GameSocketParserDiagnosticDestinationPrefixOutcome outcome;
        uint? resultESP = parentPlan.ResultESP;
        uint? comparisonAddress = null;
        uint? stackDWORDAddress = null;
        uint? ediValue = null;
        uint? edxValue = null;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        uint stopBefore = parentPlan.StopBeforeRawFileOffset;

        if (!parentContinuationAdmitted)
        {
            outcome =
                GameSocketParserDiagnosticDestinationPrefixOutcome.ParentContinuationNotAdmitted;
            AddUnusedIfSupplied(
                unused,
                ebpState,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshPostCallEBP);
            AddUnusedIfSupplied(
                unused,
                comparisonState,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshDWORDAtEBPPlusC);
            AddUnusedIfSupplied(
                unused,
                stackDWORDState,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshDWORDAtESPPlus14);
            return CreatePlan();
        }

        stopBefore = CompareRawFileOffset;
        ebpState = CaptureState(capture.FreshPostCallEBP);
        comparisonState =
            GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached;
        stackDWORDState =
            GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached;
        if (capture.FreshPostCallEBP is not uint freshEBP)
        {
            MarkUnusedIfSupplied(
                unused,
                ref comparisonState,
                capture.FreshDWORDAtEBPPlusC,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshDWORDAtEBPPlusC);
            MarkUnusedIfSupplied(
                unused,
                ref stackDWORDState,
                capture.FreshDWORDAtESPPlus14,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshDWORDAtESPPlus14);
            instructions.Add(new(
                GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .CompareDWORDAtEBPPlusCWith2At3473,
                CompareRawFileOffset,
                ToCodeVA(CompareRawFileOffset),
                CompareEncoding,
                resultESP,
                resultESP,
                MemoryAddress: null,
                MemoryValue: null,
                DestinationRegister: null,
                DestinationValue: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            outcome =
                GameSocketParserDiagnosticDestinationPrefixOutcome.MissingFreshPostCallEBP;
            return CreatePlan();
        }

        comparisonAddress = unchecked(freshEBP + EBPDisplacement);
        comparisonState = CaptureState(capture.FreshDWORDAtEBPPlusC);
        reads.Add(new(
            CompareRawFileOffset,
            comparisonAddress,
            capture.FreshDWORDAtEBPPlusC,
            Attempted: true,
            Captured: capture.FreshDWORDAtEBPPlusC.HasValue,
            Committed: false));
        effects.Add(new(
            GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind
                .FreshDWORDAtEBPPlusCRead,
            CompareRawFileOffset,
            comparisonAddress,
            capture.FreshDWORDAtEBPPlusC,
            Attempted: true,
            Captured: capture.FreshDWORDAtEBPPlusC.HasValue,
            Committed: false,
            ParentKind: null));

        if (capture.FreshDWORDAtEBPPlusC is not uint comparisonValue)
        {
            MarkUnusedIfSupplied(
                unused,
                ref stackDWORDState,
                capture.FreshDWORDAtESPPlus14,
                GameSocketParserDiagnosticDestinationPrefixCaptureKind.FreshDWORDAtESPPlus14);
            instructions.Add(new(
                GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .CompareDWORDAtEBPPlusCWith2At3473,
                CompareRawFileOffset,
                ToCodeVA(CompareRawFileOffset),
                CompareEncoding,
                resultESP,
                resultESP,
                comparisonAddress,
                MemoryValue: null,
                DestinationRegister: null,
                DestinationValue: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            outcome =
                GameSocketParserDiagnosticDestinationPrefixOutcome.MissingFreshDWORDAtEBPPlusC;
            return CreatePlan();
        }

        GameSocketParserStoredByteSelectorGateFlags comparisonFlags =
            CompareFlags32(comparisonValue, CompareImmediate);
        instructions.Add(new(
            GameSocketParserDiagnosticDestinationPrefixInstructionKind
                .CompareDWORDAtEBPPlusCWith2At3473,
            CompareRawFileOffset,
            ToCodeVA(CompareRawFileOffset),
            CompareEncoding,
            resultESP,
            resultESP,
            comparisonAddress,
            comparisonValue,
            DestinationRegister: null,
            DestinationValue: null,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: comparisonFlags));

        ediValue = EDIImmediate;
        instructions.Add(new(
            GameSocketParserDiagnosticDestinationPrefixInstructionKind.MoveEDIImmediateAt3477,
            MoveEDIRawFileOffset,
            ToCodeVA(MoveEDIRawFileOffset),
            MoveEDIEncoding,
            resultESP,
            resultESP,
            MemoryAddress: null,
            MemoryValue: null,
            DestinationRegister: "EDI",
            DestinationValue: ediValue,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: comparisonFlags,
            FlagsAfter: comparisonFlags));

        stackDWORDAddress = unchecked(resultESP!.Value + ESPDisplacement);
        stackDWORDState = CaptureState(capture.FreshDWORDAtESPPlus14);
        reads.Add(new(
            MoveEDXRawFileOffset,
            stackDWORDAddress,
            capture.FreshDWORDAtESPPlus14,
            Attempted: true,
            Captured: capture.FreshDWORDAtESPPlus14.HasValue,
            Committed: false));
        effects.Add(new(
            GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind
                .FreshDWORDAtESPPlus14Read,
            MoveEDXRawFileOffset,
            stackDWORDAddress,
            capture.FreshDWORDAtESPPlus14,
            Attempted: true,
            Captured: capture.FreshDWORDAtESPPlus14.HasValue,
            Committed: false,
            ParentKind: null));

        if (capture.FreshDWORDAtESPPlus14 is not uint stackValue)
        {
            instructions.Add(new(
                GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .MoveEDXFromESPPlus14At347C,
                MoveEDXRawFileOffset,
                ToCodeVA(MoveEDXRawFileOffset),
                MoveEDXEncoding,
                resultESP,
                resultESP,
                stackDWORDAddress,
                MemoryValue: null,
                DestinationRegister: "EDX",
                DestinationValue: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: comparisonFlags,
                FlagsAfter: comparisonFlags));
            outcome =
                GameSocketParserDiagnosticDestinationPrefixOutcome.MissingFreshDWORDAtESPPlus14;
            stopBefore = MoveEDXRawFileOffset;
            flagsAtStop = comparisonFlags;
            return CreatePlan();
        }

        edxValue = stackValue;
        instructions.Add(new(
            GameSocketParserDiagnosticDestinationPrefixInstructionKind
                .MoveEDXFromESPPlus14At347C,
            MoveEDXRawFileOffset,
            ToCodeVA(MoveEDXRawFileOffset),
            MoveEDXEncoding,
            resultESP,
            resultESP,
            stackDWORDAddress,
            stackValue,
            DestinationRegister: "EDX",
            DestinationValue: edxValue,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: comparisonFlags,
            FlagsAfter: comparisonFlags));
        outcome =
            GameSocketParserDiagnosticDestinationPrefixOutcome.BoundedBeforeJaeAt3480;
        stopBefore = JaeRawFileOffset;
        flagsAtStop = comparisonFlags;
        return CreatePlan();

        GameSocketParserDiagnosticDestinationPrefixPlan CreatePlan() =>
            new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                reads.ToImmutable(),
                effects.ToImmutable(),
                unused.ToImmutable(),
                ebpState,
                comparisonState,
                stackDWORDState,
                outcome,
                parentContinuationAdmitted,
                resultESP,
                comparisonAddress,
                stackDWORDAddress,
                ediValue,
                edxValue,
                flagsAtStop,
                stopBefore);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);

    private static GameSocketParserStoredByteSelectorGateFlags CompareFlags32(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static GameSocketParserDiagnosticDestinationPrefixCaptureState CaptureState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserDiagnosticDestinationPrefixCaptureState.Captured
            : GameSocketParserDiagnosticDestinationPrefixCaptureState.Missing;

    private static GameSocketParserDiagnosticDestinationPrefixCaptureState UnreachedState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            : GameSocketParserDiagnosticDestinationPrefixCaptureState.NotReached;

    private static void MarkUnusedIfSupplied(
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixCaptureKind>.Builder unused,
        ref GameSocketParserDiagnosticDestinationPrefixCaptureState state,
        uint? value,
        GameSocketParserDiagnosticDestinationPrefixCaptureKind kind)
    {
        state = UnreachedState(value);
        AddUnusedIfSupplied(unused, state, kind);
    }

    private static void AddUnusedIfSupplied(
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixCaptureKind>.Builder unused,
        GameSocketParserDiagnosticDestinationPrefixCaptureState state,
        GameSocketParserDiagnosticDestinationPrefixCaptureKind kind)
    {
        if (state
            == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused)
        {
            unused.Add(kind);
        }
    }
}
