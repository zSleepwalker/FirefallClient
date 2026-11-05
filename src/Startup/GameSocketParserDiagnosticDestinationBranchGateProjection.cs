using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticDestinationBranchGateCapture(
    GameSocketParserDiagnosticDestinationPrefixCapture ParentCapture);

public enum GameSocketParserDiagnosticDestinationBranchGateOutcome
{
    ParentPrefixNotAdmitted,
    JaeTakenTo31F8,
    JaeFallsThroughJmpTo34AD
}

public enum GameSocketParserDiagnosticDestinationBranchGateInstructionKind
{
    JaeAt3480,
    JmpAt3486
}

public readonly record struct GameSocketParserDiagnosticDestinationBranchGateInstruction(
    GameSocketParserDiagnosticDestinationBranchGateInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    bool Attempted,
    bool Reached,
    bool Committed,
    uint? BranchTargetRawFileOffset,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticDestinationBranchGatePlan
{
    internal GameSocketParserDiagnosticDestinationBranchGatePlan(
        GameSocketParserDiagnosticDestinationBranchGateCapture capture,
        GameSocketParserDiagnosticDestinationPrefixPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticDestinationBranchGateInstruction> instructions,
        GameSocketParserDiagnosticDestinationBranchGateOutcome outcome,
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

    public GameSocketParserDiagnosticDestinationBranchGateCapture Capture { get; }
    public GameSocketParserDiagnosticDestinationPrefixPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticDestinationBranchGateInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticDestinationBranchGateOutcome Outcome { get; }
    public bool ParentPrefixAdmitted { get; }
    public bool ParentPreparationCompleted => ParentPlan.ParentPreparationCompleted;
    public bool ParentExactBoundedCompletion => ParentPlan.ParentExactBoundedCompletion;
    public bool ParentPrefixExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool? NormalReturnObserved => ParentPlan.NormalReturnObserved;
    public bool? ResultNormalReturnObserved => ParentPlan.ResultNormalReturnObserved;
    public uint? ParentESP => ParentPlan.ParentESP;
    public uint? ResultESP => ParentPlan.ResultESP;
    public uint? FreshPostCallEBP => ParentPlan.FreshPostCallEBP;
    public uint? ComparisonAddress => ParentPlan.ComparisonAddress;
    public uint? StackDWORDAddress => ParentPlan.StackDWORDAddress;
    public uint? EDIValue => ParentPlan.EDIValue;
    public uint? EDXValue => ParentPlan.EDXValue;
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        FreshPostCallEBPCaptureState => ParentPlan.FreshPostCallEBPCaptureState;
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        DWORDAtEBPPlusCCaptureState => ParentPlan.DWORDAtEBPPlusCCaptureState;
    public GameSocketParserDiagnosticDestinationPrefixCaptureState
        DWORDAtESPPlus14CaptureState => ParentPlan.DWORDAtESPPlus14CaptureState;
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticDestinationPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop => ParentPlan.FlagsAtStop;
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? BranchTargetRawFileOffset =>
        ParentPrefixAdmitted ? StopBeforeRawFileOffset : null;
    public bool JaeAttempted => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JaeAt3480
            && instruction.Attempted);
    public bool JaeReached => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JaeAt3480
            && instruction.Reached);
    public bool JaeCommitted => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JaeAt3480
            && instruction.Committed);
    public bool JmpAttempted => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JmpAt3486
            && instruction.Attempted);
    public bool JmpReached => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JmpAt3486
            && instruction.Reached);
    public bool JmpCommitted => Instructions.Any(
        instruction => instruction.Kind
            == GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JmpAt3486
            && instruction.Committed);
    public bool BranchTargetInstructionExecuted => false;
    public bool EDIAndEDXPreserved =>
        ParentPrefixAdmitted
        && EDIValue == ParentPlan.EDIValue
        && EDXValue == ParentPlan.EDXValue;
    public bool FlagsPreserved =>
        ParentPrefixAdmitted
        && Instructions.All(instruction =>
            instruction.FlagsBefore == ParentPlan.FlagsAtStop
            && instruction.FlagsAfter == ParentPlan.FlagsAtStop);
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedMemoryPrefixPreserved
        && ReprojectedParentPrefixMatches();
    public bool ExactBoundedCompletion =>
        ParentPrefixAdmitted
            ? Instructions.Length > 0
                && (Outcome == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
                    ? StopBeforeRawFileOffset == GameSocketParserDiagnosticDestinationBranchGateProjection.JaeTargetRawFileOffset
                    : Outcome == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeFallsThroughJmpTo34AD
                        && StopBeforeRawFileOffset == GameSocketParserDiagnosticDestinationBranchGateProjection.JmpTargetRawFileOffset)
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
        && EDIAndEDXPreserved
        && OrderedSemanticPrefixPreserved;
    public bool JaeDestinationModeled => JaeCommitted;
    public bool JmpDestinationModeled => JmpCommitted;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentPrefixMatches()
    {
        GameSocketParserDiagnosticDestinationPrefixPlan reprojected =
            GameSocketParserDiagnosticDestinationPrefixProjection.Project(
                Capture.ParentCapture);
        return reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FreshPostCallEBP == ParentPlan.FreshPostCallEBP
            && reprojected.EDIValue == ParentPlan.EDIValue
            && reprojected.EDXValue == ParentPlan.EDXValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    }
}

/// <summary>Reprojects the conditional branch and fall-through jump after the clean prefix at 3480.</summary>
public static class GameSocketParserDiagnosticDestinationBranchGateProjection
{
    public const uint JaeRawFileOffset = 0x00BD3480;
    public const uint JmpRawFileOffset = 0x00BD3486;
    public const uint JaeTargetRawFileOffset = 0x00BD31F8;
    public const uint JmpTargetRawFileOffset = 0x00BD34AD;
    public const string JaeEncoding = "0F 83 72 FD FF FF";
    public const string JmpEncoding = "EB 25";
    public const string CoordinatorProvidedBytes = "0F 83 72 FD FF FF EB 25";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticDestinationBranchGatePlan Project(
        GameSocketParserDiagnosticDestinationBranchGateCapture capture)
    {
        GameSocketParserDiagnosticDestinationPrefixPlan parentPlan =
            GameSocketParserDiagnosticDestinationPrefixProjection.Project(capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticDestinationBranchGateInstruction>();
        bool parentPrefixAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticDestinationPrefixOutcome.BoundedBeforeJaeAt3480
            && parentPlan.StopBeforeRawFileOffset
                == GameSocketParserDiagnosticDestinationPrefixProjection.JaeRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.FreshPostCallEBP.HasValue
            && parentPlan.ComparisonAddress.HasValue
            && parentPlan.StackDWORDAddress.HasValue
            && parentPlan.EDIValue.HasValue
            && parentPlan.EDXValue.HasValue
            && parentPlan.FlagsAtStop.HasValue
            && parentPlan.FlagsAtStop.Value.Carry.HasValue;

        if (!parentPrefixAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticDestinationBranchGateOutcome.ParentPrefixNotAdmitted,
                parentPrefixAdmitted: false,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        GameSocketParserStoredByteSelectorGateFlags flags = parentPlan.FlagsAtStop!.Value;
        instructions.Add(new(
            GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JaeAt3480,
            JaeRawFileOffset,
            ToCodeVA(JaeRawFileOffset),
            JaeEncoding,
            Attempted: true,
            Reached: true,
            Committed: true,
            BranchTargetRawFileOffset: flags.Carry == false ? JaeTargetRawFileOffset : null,
            FlagsBefore: flags,
            FlagsAfter: flags));

        bool jaeTaken = flags.Carry == false;
        if (jaeTaken)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8,
                parentPrefixAdmitted: true,
                stopBeforeRawFileOffset: JaeTargetRawFileOffset);
        }

        instructions.Add(new(
            GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JmpAt3486,
            JmpRawFileOffset,
            ToCodeVA(JmpRawFileOffset),
            JmpEncoding,
            Attempted: true,
            Reached: true,
            Committed: true,
            BranchTargetRawFileOffset: JmpTargetRawFileOffset,
            FlagsBefore: flags,
            FlagsAfter: flags));
        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeFallsThroughJmpTo34AD,
            parentPrefixAdmitted: true,
            stopBeforeRawFileOffset: JmpTargetRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
