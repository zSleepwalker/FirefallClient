using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupRegisterPreparationCapture(
    GameSocketParserDiagnosticCleanupZeroBranchCapture ParentCapture);

public enum GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
{
    ParentBranchNotAdmitted,
    BoundedBeforeLockXaddAt34C5
}

public enum GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
{
    OrEBXWithMinusOneAt34BD,
    LeaEAXFromESIPlusFourAt34C0,
    MoveECXFromEBXAt34C3
}

public readonly record struct GameSocketParserDiagnosticCleanupRegisterPreparationInstruction(
    GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    string DestinationRegister,
    uint? DestinationValue,
    string? SourceRegister,
    uint? SourceValue,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticCleanupRegisterPreparationPlan
{
    internal GameSocketParserDiagnosticCleanupRegisterPreparationPlan(
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture capture,
        GameSocketParserDiagnosticCleanupZeroBranchPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupRegisterPreparationInstruction> instructions,
        GameSocketParserDiagnosticCleanupRegisterPreparationOutcome outcome,
        bool parentBranchAdmitted,
        uint? resultESP,
        uint? esiValue,
        uint? eaxValue,
        uint? ebxValue,
        uint? ecxValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Outcome = outcome;
        ParentBranchAdmitted = parentBranchAdmitted;
        ResultESP = resultESP;
        ESIValue = esiValue;
        EAXValue = eaxValue;
        EBXValue = ebxValue;
        ECXValue = ecxValue;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupRegisterPreparationCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupZeroBranchPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupRegisterPreparationInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupRegisterPreparationOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public uint? ResultESP { get; }
    public uint? ESIValue { get; }
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ECXValue { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? LockXaddRawFileOffset =>
        ParentBranchAdmitted ? GameSocketParserDiagnosticCleanupRegisterPreparationProjection
            .LockXaddRawFileOffset : null;
    public bool PreviousEBXValueCaptured => false;
    public bool PreviousEBXCaptureRequired => false;
    public bool LEAAddressDereferenced => false;
    public bool LockXaddAttempted => false;
    public bool LockXaddReached => false;
    public bool LockXaddCommitted => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeAtomicSemanticsModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool ESPUnchanged =>
        ParentBranchAdmitted
        && ResultESP.HasValue
        && Instructions.All(instruction =>
            instruction.StackPointerBefore == ResultESP
            && instruction.StackPointerAfter == ResultESP);
    public bool ESIInherited =>
        ParentBranchAdmitted && ESIValue == ParentPlan.ESIValue;
    public bool FlagsPropagated =>
        ParentBranchAdmitted
        && Instructions.Length == 3
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == OrFlags
        && Instructions[1].FlagsBefore == OrFlags
        && Instructions[1].FlagsAfter == OrFlags
        && Instructions[2].FlagsBefore == OrFlags
        && Instructions[2].FlagsAfter == OrFlags
        && FlagsAtStop == OrFlags;
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentPrefixMatches()
        && ParentReadsAndEffectsPreserved();
    public bool ExactBoundedCompletion =>
        ParentBranchAdmitted
            ? Outcome
                    == GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                        .BoundedBeforeLockXaddAt34C5
                && Instructions.Length == 3
                && Instructions[0].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                        .OrRawFileOffset
                && Instructions[1].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                        .LeaRawFileOffset
                && Instructions[2].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                        .MoveRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                        .LockXaddRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentBranchAdmitted
        && ParentPlan.IsCleanCompletion
        && ExactBoundedCompletion
        && Instructions.All(instruction =>
            instruction.Attempted && instruction.Reached && instruction.Committed)
        && EAXValue.HasValue
        && EBXValue == uint.MaxValue
        && ECXValue == uint.MaxValue
        && ESPUnchanged
        && ESIInherited
        && FlagsPropagated
        && OrderedSemanticPrefixPreserved
        && !LockXaddAttempted
        && !LockXaddReached
        && !LockXaddCommitted;

    private static GameSocketParserStoredByteSelectorGateFlags OrFlags =>
        new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: null,
            Zero: false,
            Sign: true,
            Overflow: false);

    private bool ReprojectedParentPrefixMatches()
    {
        GameSocketParserDiagnosticCleanupZeroBranchPlan reprojected =
            GameSocketParserDiagnosticCleanupZeroBranchProjection.Project(
                Capture.ParentCapture);
        return reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentPrefixAdmitted == ParentPlan.ParentPrefixAdmitted
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion;
    }

    private bool ParentReadsAndEffectsPreserved() =>
        Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
}

/// <summary>Projects the three register operations after the captured nonzero cleanup branch.</summary>
public static class GameSocketParserDiagnosticCleanupRegisterPreparationProjection
{
    public const uint OrRawFileOffset = 0x00BD34BD;
    public const uint LeaRawFileOffset = 0x00BD34C0;
    public const uint MoveRawFileOffset = 0x00BD34C3;
    public const uint LockXaddRawFileOffset = 0x00BD34C5;
    public const string OrEncoding = "83 CB FF";
    public const string LeaEncoding = "8D 46 04";
    public const string MoveEncoding = "8B CB";
    public const string CoordinatorProvidedBytes =
        "C7 44 24 50 FF FF FF FF 8B 74 24 3C 85 F6 74 25 83 CB FF 8D 46 04 8B CB F0 0F C1 08 75 17 8B 06";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupRegisterPreparationPlan Project(
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture capture)
    {
        GameSocketParserDiagnosticCleanupZeroBranchPlan parentPlan =
            GameSocketParserDiagnosticCleanupZeroBranchProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupRegisterPreparationInstruction>();
        bool parentBranchAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzNotTakenTo34BD
            && parentPlan.StopBeforeRawFileOffset == OrRawFileOffset
            && parentPlan.ESIValue is > 0
            && parentPlan.ResultESP.HasValue
            && parentPlan.FlagsAtStop is { Zero: false };

        if (!parentBranchAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                    .ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                resultESP: parentPlan.ResultESP,
                esiValue: parentPlan.ESIValue,
                eaxValue: null,
                ebxValue: null,
                ecxValue: null,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint esp = parentPlan.ResultESP!.Value;
        uint esi = parentPlan.ESIValue!.Value;
        GameSocketParserStoredByteSelectorGateFlags? incomingFlags = parentPlan.FlagsAtStop;
        GameSocketParserStoredByteSelectorGateFlags orFlags = new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: null,
            Zero: false,
            Sign: true,
            Overflow: false);
        uint eax = unchecked(esi + 4);

        instructions.Add(new(
            GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                .OrEBXWithMinusOneAt34BD,
            OrRawFileOffset,
            ToCodeVA(OrRawFileOffset),
            OrEncoding,
            esp,
            esp,
            "EBX",
            uint.MaxValue,
            "EBX",
            null,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: incomingFlags,
            FlagsAfter: orFlags));
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                .LeaEAXFromESIPlusFourAt34C0,
            LeaRawFileOffset,
            ToCodeVA(LeaRawFileOffset),
            LeaEncoding,
            esp,
            esp,
            "EAX",
            eax,
            "ESI + 4",
            esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: orFlags,
            FlagsAfter: orFlags));
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                .MoveECXFromEBXAt34C3,
            MoveRawFileOffset,
            ToCodeVA(MoveRawFileOffset),
            MoveEncoding,
            esp,
            esp,
            "ECX",
            uint.MaxValue,
            "EBX",
            uint.MaxValue,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: orFlags,
            FlagsAfter: orFlags));

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                .BoundedBeforeLockXaddAt34C5,
            parentBranchAdmitted: true,
            resultESP: esp,
            esiValue: esi,
            eaxValue: eax,
            ebxValue: uint.MaxValue,
            ecxValue: uint.MaxValue,
            flagsAtStop: orFlags,
            stopBeforeRawFileOffset: LockXaddRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
