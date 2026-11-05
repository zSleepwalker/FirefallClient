using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupZeroBranchCapture(
    GameSocketParserDiagnosticCleanupStackPrefixCapture ParentCapture);

public enum GameSocketParserDiagnosticCleanupZeroBranchOutcome
{
    ParentPrefixNotAdmitted,
    JzTakenTo34E2,
    JzNotTakenTo34BD
}

public enum GameSocketParserDiagnosticCleanupZeroBranchInstructionKind
{
    JzAt34BB
}

public readonly record struct GameSocketParserDiagnosticCleanupZeroBranchInstruction(
    GameSocketParserDiagnosticCleanupZeroBranchInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    bool Attempted,
    bool Reached,
    bool Committed,
    uint? BranchTargetRawFileOffset,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticCleanupZeroBranchPlan
{
    internal GameSocketParserDiagnosticCleanupZeroBranchPlan(
        GameSocketParserDiagnosticCleanupZeroBranchCapture capture,
        GameSocketParserDiagnosticCleanupStackPrefixPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupZeroBranchInstruction> instructions,
        GameSocketParserDiagnosticCleanupZeroBranchOutcome outcome,
        bool parentPrefixAdmitted,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Outcome = outcome;
        ParentPrefixAdmitted = parentPrefixAdmitted;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupZeroBranchCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupStackPrefixPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupZeroBranchInstruction> Instructions { get; }
    public GameSocketParserDiagnosticCleanupZeroBranchOutcome Outcome { get; }
    public bool ParentPrefixAdmitted { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public uint? ResultESP => ParentPlan.ResultESP;
    public uint? ESIValue => ParentPlan.ESIValue;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop =>
        Instructions.IsEmpty ? ParentPlan.FlagsAtStop : Instructions[^1].FlagsAfter;
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? BranchTargetRawFileOffset =>
        ParentPrefixAdmitted ? StopBeforeRawFileOffset : null;
    public bool JzAttempted => Instructions.Any(instruction => instruction.Attempted);
    public bool JzReached => Instructions.Any(instruction => instruction.Reached);
    public bool JzCommitted => Instructions.Any(instruction => instruction.Committed);
    public bool JzTaken =>
        Outcome == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2;
    public bool BranchTargetInstructionExecuted => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeAtomicSemanticsModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool FlagsPreserved =>
        ParentPrefixAdmitted
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == ParentPlan.FlagsAtStop
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool ESIAndESPInherited =>
        ParentPrefixAdmitted
        && ESIValue == ParentPlan.ESIValue
        && ResultESP == ParentPlan.ResultESP;
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentPrefixMatches();
    public bool ExactBoundedCompletion =>
        ParentPrefixAdmitted
            ? Instructions.Length == 1
                && Instructions[0].Attempted
                && Instructions[0].Reached
                && Instructions[0].Committed
                && (Outcome == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
                    ? StopBeforeRawFileOffset
                        == GameSocketParserDiagnosticCleanupZeroBranchProjection.JzTargetRawFileOffset
                        && Instructions[0].BranchTargetRawFileOffset
                            == GameSocketParserDiagnosticCleanupZeroBranchProjection.JzTargetRawFileOffset
                    : Outcome == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzNotTakenTo34BD
                        && StopBeforeRawFileOffset
                            == GameSocketParserDiagnosticCleanupZeroBranchProjection.FallThroughRawFileOffset
                        && Instructions[0].BranchTargetRawFileOffset
                            == GameSocketParserDiagnosticCleanupZeroBranchProjection.FallThroughRawFileOffset)
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentPrefixAdmitted
        && ParentPlan.IsCleanCompletion
        && ExactBoundedCompletion
        && BranchTargetRawFileOffset.HasValue
        && !BranchTargetInstructionExecuted
        && FlagsPreserved
        && ESIAndESPInherited
        && OrderedSemanticPrefixPreserved;
    public bool JzTargetModeled => JzCommitted;

    private bool ReprojectedParentPrefixMatches()
    {
        GameSocketParserDiagnosticCleanupStackPrefixPlan reprojected =
            GameSocketParserDiagnosticCleanupStackPrefixProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.OrderedSemanticPrefixPreserved
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion;
    }
}

/// <summary>Reprojects only the conditional zero branch following the captured cleanup prefix.</summary>
public static class GameSocketParserDiagnosticCleanupZeroBranchProjection
{
    public const uint JzRawFileOffset = 0x00BD34BB;
    public const uint FallThroughRawFileOffset = 0x00BD34BD;
    public const uint JzTargetRawFileOffset = 0x00BD34E2;
    public const string JzEncoding = "74 25";
    public const string CoordinatorProvidedBytes =
        "C7 44 24 50 FF FF FF FF 8B 74 24 3C 85 F6 74 25 83 CB FF 8D 46 04 8B CB F0 0F C1 08 75 17 8B 06";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupZeroBranchPlan Project(
        GameSocketParserDiagnosticCleanupZeroBranchCapture capture)
    {
        GameSocketParserDiagnosticCleanupStackPrefixPlan parentPlan =
            GameSocketParserDiagnosticCleanupStackPrefixProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupZeroBranchInstruction>();
        bool parentPrefixAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupStackPrefixOutcome.BoundedBeforeJzAt34BB
            && parentPlan.StopBeforeRawFileOffset == JzRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.FlagsAtStop is { Zero: not null };

        if (!parentPrefixAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticCleanupZeroBranchOutcome.ParentPrefixNotAdmitted,
                parentPrefixAdmitted: false,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        GameSocketParserStoredByteSelectorGateFlags flags = parentPlan.FlagsAtStop!.Value;
        bool taken = flags.Zero == true;
        uint stopBeforeRawFileOffset =
            taken ? JzTargetRawFileOffset : FallThroughRawFileOffset;
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupZeroBranchInstructionKind.JzAt34BB,
            JzRawFileOffset,
            ToCodeVA(JzRawFileOffset),
            JzEncoding,
            Attempted: true,
            Reached: true,
            Committed: true,
            BranchTargetRawFileOffset: stopBeforeRawFileOffset,
            FlagsBefore: flags,
            FlagsAfter: flags));
        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            taken
                ? GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
                : GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzNotTakenTo34BD,
            parentPrefixAdmitted: true,
            stopBeforeRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
