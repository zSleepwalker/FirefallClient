using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupStackPrefixCapture(
    GameSocketParserDiagnosticDestinationBranchGateCapture ParentCapture,
    uint? FreshDWORDAtESPPlus3C);

public enum GameSocketParserDiagnosticCleanupStackPrefixCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupStackPrefixOutcome
{
    ParentBranchNotAdmitted,
    MissingFreshDWORDAtESPPlus3C,
    BoundedBeforeJzAt34BB
}

public enum GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
{
    StoreFFFFFFFFAt34AD,
    MoveESIFromESPPlus3CAt34B5,
    TestESIAt34B9
}

public enum GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
{
    ParentPrefix,
    StoreFFFFFFFFAtESPPlus50,
    FreshDWORDAtESPPlus3CRead
}

public readonly record struct GameSocketParserDiagnosticCleanupStackPrefixInstruction(
    GameSocketParserDiagnosticCleanupStackPrefixInstructionKind Kind,
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

public readonly record struct GameSocketParserDiagnosticCleanupStackPrefixRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect(
    GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    GameSocketParserDiagnosticDestinationPrefixMemoryEffect? ParentEffect);

public sealed class GameSocketParserDiagnosticCleanupStackPrefixPlan
{
    internal GameSocketParserDiagnosticCleanupStackPrefixPlan(
        GameSocketParserDiagnosticCleanupStackPrefixCapture capture,
        GameSocketParserDiagnosticDestinationBranchGatePlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixInstruction> instructions,
        ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> reads,
        ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> memoryEffects,
        GameSocketParserDiagnosticCleanupStackPrefixCaptureState readCaptureState,
        GameSocketParserDiagnosticCleanupStackPrefixOutcome outcome,
        bool parentBranchAdmitted,
        uint? resultESP,
        uint? storeAddress,
        uint? readAddress,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Reads = reads;
        MemoryEffects = memoryEffects;
        FreshDWORDAtESPPlus3CCaptureState = readCaptureState;
        Outcome = outcome;
        ParentBranchAdmitted = parentBranchAdmitted;
        ResultESP = resultESP;
        StoreAddress = storeAddress;
        ReadAddress = readAddress;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupStackPrefixCapture Capture { get; }
    public GameSocketParserDiagnosticDestinationBranchGatePlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixInstruction> Instructions { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects { get; }
    public GameSocketParserDiagnosticCleanupStackPrefixCaptureState FreshDWORDAtESPPlus3CCaptureState { get; }
    public GameSocketParserDiagnosticCleanupStackPrefixOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public bool ParentPreparationCompleted => ParentPlan.ParentPreparationCompleted;
    public bool ParentExactBoundedCompletion => ParentPlan.ParentExactBoundedCompletion;
    public bool ParentPrefixExactBoundedCompletion => ParentPlan.ParentPrefixExactBoundedCompletion;
    public bool? NormalReturnObserved => ParentPlan.NormalReturnObserved;
    public bool? ResultNormalReturnObserved => ParentPlan.ResultNormalReturnObserved;
    public uint? ParentESP => ParentPlan.ParentESP;
    public uint? ResultESP { get; }
    public uint? StoreAddress { get; }
    public uint? ReadAddress { get; }
    public uint? ESIValue { get; }
    public GameSocketParserStoredByteSelectorGateFlags? ParentFlagsAtStop =>
        ParentPlan.FlagsAtStop;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? BranchTargetRawFileOffset =>
        ParentBranchAdmitted
            ? GameSocketParserDiagnosticCleanupStackPrefixProjection.StoreRawFileOffset
            : null;
    public bool StoreAttempted => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.StoreFFFFFFFFAt34AD
        && instruction.Attempted);
    public bool StoreReached => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.StoreFFFFFFFFAt34AD
        && instruction.Reached);
    public bool StoreCommitted => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.StoreFFFFFFFFAt34AD
        && instruction.Committed);
    public bool FreshDWORDReadAttempted => Reads.Any(read => read.Attempted);
    public bool FreshDWORDCaptured => Reads.Any(read => read.Captured);
    public bool ESICommitted => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.MoveESIFromESPPlus3CAt34B5
        && instruction.Committed);
    public bool TestAttempted => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.TestESIAt34B9
        && instruction.Attempted);
    public bool TestReached => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.TestESIAt34B9
        && instruction.Reached);
    public bool TestCommitted => Instructions.Any(instruction =>
        instruction.Kind == GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.TestESIAt34B9
        && instruction.Committed);
    public bool JzAttempted => false;
    public bool JzReached => false;
    public bool JzCommitted => false;
    public bool BranchTargetInstructionExecuted => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeAtomicSemanticsModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool AliasedReadsReconciled => false;
    public uint StoreWidthBytes =>
        GameSocketParserDiagnosticCleanupStackPrefixProjection.DWORDWidthBytes;
    public uint FreshReadWidthBytes =>
        GameSocketParserDiagnosticCleanupStackPrefixProjection.DWORDWidthBytes;
    public bool ESPUnchanged =>
        ParentBranchAdmitted
        && ResultESP.HasValue
        && Instructions.All(instruction =>
            instruction.StackPointerBefore == ResultESP
            && instruction.StackPointerAfter == ResultESP);
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentPrefixMatches()
        && ParentEffectsPreserved();
    public bool ExactBoundedCompletion =>
        ParentBranchAdmitted
            ? Outcome == GameSocketParserDiagnosticCleanupStackPrefixOutcome.BoundedBeforeJzAt34BB
                && Instructions.Length == 3
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupStackPrefixProjection.JzRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentBranchAdmitted
        && ParentPlan.IsCleanCompletion
        && ExactBoundedCompletion
        && StoreCommitted
        && FreshDWORDCaptured
        && ESICommitted
        && TestCommitted
        && ESPUnchanged
        && OrderedSemanticPrefixPreserved;

    private bool ParentEffectsPreserved()
    {
        ImmutableArray<GameSocketParserDiagnosticDestinationPrefixMemoryEffect> parentEffects =
            ParentPlan.ParentPlan.MemoryEffects;
        if (MemoryEffects.Length != parentEffects.Length + (ParentBranchAdmitted ? 2 : 0))
        {
            return false;
        }

        for (int index = 0; index < parentEffects.Length; index++)
        {
            GameSocketParserDiagnosticDestinationPrefixMemoryEffect parentEffect =
                parentEffects[index];
            GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect effect =
                MemoryEffects[index];
            if (effect.Kind != GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind.ParentPrefix
                || effect.ParentEffect != parentEffect
                || effect.Address != parentEffect.Address
                || effect.Value != parentEffect.Value
                || effect.Attempted != parentEffect.Attempted
                || effect.Captured != parentEffect.Captured
                || effect.Committed != parentEffect.Committed)
            {
                return false;
            }
        }

        if (!ParentBranchAdmitted)
        {
            return true;
        }

        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect storeEffect =
            MemoryEffects[parentEffects.Length];
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect readEffect =
            MemoryEffects[parentEffects.Length + 1];
        return storeEffect.Kind
                == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                    .StoreFFFFFFFFAtESPPlus50
            && storeEffect.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupStackPrefixProjection.StoreRawFileOffset
            && storeEffect.Address == StoreAddress
            && storeEffect.Value == uint.MaxValue
            && storeEffect.Attempted
            && storeEffect.Captured
            && storeEffect.Committed
            && storeEffect.ParentEffect is null
            && readEffect.Kind
                == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                    .FreshDWORDAtESPPlus3CRead
            && readEffect.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupStackPrefixProjection.MoveESIRawFileOffset
            && readEffect.Address == ReadAddress
            && readEffect.Value == Capture.FreshDWORDAtESPPlus3C
            && readEffect.Attempted
            && readEffect.Captured == Capture.FreshDWORDAtESPPlus3C.HasValue
            && !readEffect.Committed
            && readEffect.ParentEffect is null;
    }

    private bool ReprojectedParentPrefixMatches()
    {
        GameSocketParserDiagnosticDestinationBranchGatePlan reprojected =
            GameSocketParserDiagnosticDestinationBranchGateProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.ParentPrefixAdmitted == ParentPlan.ParentPrefixAdmitted
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.OrderedSemanticPrefixPreserved
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion;
    }
}

/// <summary>Reprojects the diagnostic cleanup store, fresh stack load, and TEST through 34B9.</summary>
public static class GameSocketParserDiagnosticCleanupStackPrefixProjection
{
    public const uint StoreRawFileOffset = 0x00BD34AD;
    public const uint MoveESIRawFileOffset = 0x00BD34B5;
    public const uint TestESIRawFileOffset = 0x00BD34B9;
    public const uint JzRawFileOffset = 0x00BD34BB;
    public const uint ESPStoreDisplacement = 0x50;
    public const uint ESPReadDisplacement = 0x3C;
    public const uint DWORDWidthBytes = 4;
    public const uint StoreImmediate = uint.MaxValue;
    public const string StoreEncoding = "C7 44 24 50 FF FF FF FF";
    public const string MoveESIEncoding = "8B 74 24 3C";
    public const string TestESIEncoding = "85 F6";
    public const string CoordinatorProvidedBytes =
        "C7 44 24 50 FF FF FF FF 8B 74 24 3C 85 F6";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupStackPrefixPlan Project(
        GameSocketParserDiagnosticCleanupStackPrefixCapture capture)
    {
        GameSocketParserDiagnosticDestinationBranchGatePlan parentPlan =
            GameSocketParserDiagnosticDestinationBranchGateProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupStackPrefixInstruction>();
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupStackPrefixRead>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect>();
        foreach (GameSocketParserDiagnosticDestinationPrefixMemoryEffect parentEffect
            in parentPlan.ParentPlan.MemoryEffects)
        {
            effects.Add(new(
                GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind.ParentPrefix,
                parentEffect.InstructionRawFileOffset,
                parentEffect.Address,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Captured,
                parentEffect.Committed,
                parentEffect));
        }

        bool parentBranchAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome
                    .JaeFallsThroughJmpTo34AD
            && parentPlan.StopBeforeRawFileOffset == StoreRawFileOffset
            && parentPlan.BranchTargetRawFileOffset == StoreRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.FlagsAtStop is { Carry: true }
            && (parentPlan.ParentPlan.Capture.FreshDWORDAtEBPPlusC is 0 or 1);
        uint? resultESP = parentBranchAdmitted ? parentPlan.ResultESP : null;
        uint? storeAddress = resultESP is uint espBeforeStore
            ? unchecked(espBeforeStore + ESPStoreDisplacement)
            : null;
        uint? readAddress = resultESP is uint espBeforeRead
            ? unchecked(espBeforeRead + ESPReadDisplacement)
            : null;
        GameSocketParserDiagnosticCleanupStackPrefixCaptureState readState =
            parentBranchAdmitted
                ? capture.FreshDWORDAtESPPlus3C.HasValue
                    ? GameSocketParserDiagnosticCleanupStackPrefixCaptureState.Captured
                    : GameSocketParserDiagnosticCleanupStackPrefixCaptureState.Missing
                : capture.FreshDWORDAtESPPlus3C.HasValue
                    ? GameSocketParserDiagnosticCleanupStackPrefixCaptureState.SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupStackPrefixCaptureState.NotReached;

        if (!parentBranchAdmitted)
        {
            return CreatePlan(
                GameSocketParserDiagnosticCleanupStackPrefixOutcome.ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                resultESP: parentPlan.ResultESP,
                storeAddress: null,
                readAddress: null,
                esiValue: null,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        GameSocketParserStoredByteSelectorGateFlags incomingFlags =
            parentPlan.FlagsAtStop!.Value;
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.StoreFFFFFFFFAt34AD,
            StoreRawFileOffset,
            ToCodeVA(StoreRawFileOffset),
            StoreEncoding,
            resultESP,
            resultESP,
            storeAddress,
            StoreImmediate,
            DestinationRegister: null,
            DestinationValue: null,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: incomingFlags,
            FlagsAfter: incomingFlags));
        effects.Add(new(
            GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind.StoreFFFFFFFFAtESPPlus50,
            StoreRawFileOffset,
            storeAddress,
            StoreImmediate,
            Attempted: true,
            Captured: true,
            Committed: true,
            ParentEffect: null));

        reads.Add(new(
            MoveESIRawFileOffset,
            readAddress,
            capture.FreshDWORDAtESPPlus3C,
            Attempted: true,
            Captured: capture.FreshDWORDAtESPPlus3C.HasValue,
            Committed: false));
        effects.Add(new(
            GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind.FreshDWORDAtESPPlus3CRead,
            MoveESIRawFileOffset,
            readAddress,
            capture.FreshDWORDAtESPPlus3C,
            Attempted: true,
            Captured: capture.FreshDWORDAtESPPlus3C.HasValue,
            Committed: false,
            ParentEffect: null));

        if (capture.FreshDWORDAtESPPlus3C is not uint esiValue)
        {
            instructions.Add(new(
                GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
                    .MoveESIFromESPPlus3CAt34B5,
                MoveESIRawFileOffset,
                ToCodeVA(MoveESIRawFileOffset),
                MoveESIEncoding,
                resultESP,
                resultESP,
                readAddress,
                null,
                DestinationRegister: "ESI",
                DestinationValue: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: incomingFlags,
                FlagsAfter: incomingFlags));
            return CreatePlan(
                GameSocketParserDiagnosticCleanupStackPrefixOutcome.MissingFreshDWORDAtESPPlus3C,
                parentBranchAdmitted: true,
                resultESP: resultESP,
                storeAddress: storeAddress,
                readAddress: readAddress,
                esiValue: null,
                flagsAtStop: incomingFlags,
                stopBeforeRawFileOffset: MoveESIRawFileOffset);
        }

        instructions.Add(new(
            GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
                .MoveESIFromESPPlus3CAt34B5,
            MoveESIRawFileOffset,
            ToCodeVA(MoveESIRawFileOffset),
            MoveESIEncoding,
            resultESP,
            resultESP,
            readAddress,
            esiValue,
            DestinationRegister: "ESI",
            DestinationValue: esiValue,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: incomingFlags,
            FlagsAfter: incomingFlags));

        GameSocketParserStoredByteSelectorGateFlags testFlags = new(
            Carry: false,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)esiValue) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: esiValue == 0,
            Sign: (esiValue & 0x80000000) != 0,
            Overflow: false);
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.TestESIAt34B9,
            TestESIRawFileOffset,
            ToCodeVA(TestESIRawFileOffset),
            TestESIEncoding,
            resultESP,
            resultESP,
            MemoryAddress: null,
            MemoryValue: null,
            DestinationRegister: null,
            DestinationValue: null,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: incomingFlags,
            FlagsAfter: testFlags));
        return CreatePlan(
            GameSocketParserDiagnosticCleanupStackPrefixOutcome.BoundedBeforeJzAt34BB,
            parentBranchAdmitted: true,
            resultESP: resultESP,
            storeAddress: storeAddress,
            readAddress: readAddress,
            esiValue: esiValue,
            flagsAtStop: testFlags,
            stopBeforeRawFileOffset: JzRawFileOffset);

        GameSocketParserDiagnosticCleanupStackPrefixPlan CreatePlan(
            GameSocketParserDiagnosticCleanupStackPrefixOutcome outcome,
            bool parentBranchAdmitted,
            uint? resultESP,
            uint? storeAddress,
            uint? readAddress,
            uint? esiValue,
            GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
            uint stopBeforeRawFileOffset) =>
            new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                reads.ToImmutable(),
                effects.ToImmutable(),
                readState,
                outcome,
                parentBranchAdmitted,
                resultESP,
                storeAddress,
                readAddress,
                esiValue,
                flagsAtStop,
                stopBeforeRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
