using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupXaddBranchCapture(
    GameSocketParserDiagnosticCleanupXaddCapture ParentCapture);

public enum GameSocketParserDiagnosticCleanupXaddBranchOutcome
{
    ParentBranchNotAdmitted,
    JnzNotTakenTo34CB,
    JnzTakenTo34E2
}

public readonly record struct GameSocketParserDiagnosticCleanupXaddBranchInstruction(
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointer,
    uint? EAX,
    uint? EBX,
    uint? ECX,
    uint? ESI,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool Taken,
    uint? TargetRawFileOffset,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticCleanupXaddBranchPlan
{
    internal GameSocketParserDiagnosticCleanupXaddBranchPlan(
        GameSocketParserDiagnosticCleanupXaddBranchCapture capture,
        GameSocketParserDiagnosticCleanupXaddPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupXaddBranchInstruction> instructions,
        GameSocketParserDiagnosticCleanupXaddBranchOutcome outcome,
        bool parentBranchAdmitted,
        uint? resultESP,
        uint? eaxValue,
        uint? ebxValue,
        uint? ecxValue,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? branchTargetRawFileOffset,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Outcome = outcome;
        ParentBranchAdmitted = parentBranchAdmitted;
        ResultESP = resultESP;
        EAXValue = eaxValue;
        EBXValue = ebxValue;
        ECXValue = ecxValue;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        BranchTargetRawFileOffset = branchTargetRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupXaddBranchCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupXaddPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupXaddBranchInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupXaddBranchOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public uint? ResultESP { get; }
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ECXValue { get; }
    public uint? ESIValue { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? BranchTargetRawFileOffset { get; }
    public uint? BranchTargetCodeVirtualAddress =>
        BranchTargetRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                BranchTargetRawFileOffset.Value)
            : null;
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? FallThroughRawFileOffset =>
        ParentBranchAdmitted
            ? GameSocketParserDiagnosticCleanupXaddBranchProjection.FallThroughRawFileOffset
            : null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.ParentPlan.MemoryEffects;
    public bool JnzAttempted => Instructions.Length == 1 && Instructions[0].Attempted;
    public bool JnzReached => Instructions.Length == 1 && Instructions[0].Reached;
    public bool JnzCommitted => Instructions.Length == 1 && Instructions[0].Committed;
    public bool JnzTaken =>
        Instructions.Length == 1 && Instructions[0].Taken;
    public bool BranchFlagsPreserved =>
        ParentBranchAdmitted
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == FlagsAtStop
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool RegistersPreserved =>
        ParentBranchAdmitted
        && EAXValue == ParentPlan.EAXValue
        && EBXValue == ParentPlan.EBXValue
        && ECXValue == ParentPlan.ECXValue
        && ESIValue == ParentPlan.ESIValue
        && ResultESP == ParentPlan.ResultESP
        && Instructions.Length == 1
        && Instructions[0].EAX == EAXValue
        && Instructions[0].EBX == EBXValue
        && Instructions[0].ECX == ECXValue
        && Instructions[0].ESI == ESIValue
        && Instructions[0].StackPointer == ResultESP;
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && ReadsAndEffectsPreserved();
    public bool ExactBoundedCompletion =>
        ParentBranchAdmitted
            ? (Outcome is GameSocketParserDiagnosticCleanupXaddBranchOutcome
                    .JnzNotTakenTo34CB
                    or GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzTakenTo34E2)
                && Instructions.Length == 1
                && Instructions[0].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupXaddBranchProjection.JnzRawFileOffset
                && StopBeforeRawFileOffset == BranchTargetRawFileOffset
                && Instructions[0].TargetRawFileOffset == BranchTargetRawFileOffset
                && JnzCommitted
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentBranchAdmitted
        && ParentPlan.IsCleanCompletion
        && ExactBoundedCompletion
        && Instructions.Length == 1
        && Instructions[0].Attempted
        && Instructions[0].Reached
        && Instructions[0].Committed
        && BranchFlagsPreserved
        && RegistersPreserved
        && OrderedSemanticPrefixPreserved
        && !BranchTargetInstructionExecuted
        && !NativeAtomicSemanticsModeled
        && !ConcurrencyModeled
        && !CleanupReleaseIdentityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !ParserExecutionModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    public bool BranchTargetInstructionExecuted => false;
    public bool NativeAtomicSemanticsModeled => false;
    public bool ConcurrencyModeled => false;
    public bool LinearizabilityModeled => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupXaddPlan reprojected =
            GameSocketParserDiagnosticCleanupXaddProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentBranchAdmitted == ParentPlan.ParentBranchAdmitted
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ECXValue == ParentPlan.ECXValue
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.MemoryWrite == ParentPlan.MemoryWrite
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion
            && reprojected.OrderedSemanticPrefixPreserved
                == ParentPlan.OrderedSemanticPrefixPreserved;
    }

    private bool ReadsAndEffectsPreserved() =>
        ParentPlan.ParentPlan.Reads.SequenceEqual(ParentPlan.ParentPlan.ParentPlan.Reads)
        && ParentPlan.ParentPlan.MemoryEffects.SequenceEqual(
            ParentPlan.ParentPlan.ParentPlan.MemoryEffects);
}

/// <summary>Projects the JNZ immediately after the captured locked decrement.</summary>
public static class GameSocketParserDiagnosticCleanupXaddBranchProjection
{
    public const uint JnzRawFileOffset = 0x00BD34C9;
    public const uint FallThroughRawFileOffset = 0x00BD34CB;
    public const uint JnzTargetRawFileOffset = 0x00BD34E2;
    public const string JnzEncoding = "75 17";
    public const string CoordinatorProvidedBytes =
        "F0 0F C1 08 75 17 8B 06 8B CE FF 10 8D 46 08 F0";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupXaddBranchPlan Project(
        GameSocketParserDiagnosticCleanupXaddBranchCapture capture)
    {
        GameSocketParserDiagnosticCleanupXaddPlan parentPlan =
            GameSocketParserDiagnosticCleanupXaddProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupXaddBranchInstruction>();
        bool parentBranchAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.BoundedBeforeJnzAt34C9
            && parentPlan.StopBeforeRawFileOffset == JnzRawFileOffset
            && parentPlan.ResultESP.HasValue
            && parentPlan.EAXValue.HasValue
            && parentPlan.EBXValue == uint.MaxValue
            && parentPlan.ECXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.FlagsAtStop?.Zero.HasValue == true;

        if (!parentBranchAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticCleanupXaddBranchOutcome.ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                parentPlan.ResultESP,
                parentPlan.EAXValue,
                parentPlan.EBXValue,
                parentPlan.ECXValue,
                parentPlan.ESIValue,
                parentPlan.FlagsAtStop,
                branchTargetRawFileOffset: null,
                parentPlan.StopBeforeRawFileOffset);
        }

        bool taken = parentPlan.FlagsAtStop!.Value.Zero == false;
        uint target = taken ? JnzTargetRawFileOffset : FallThroughRawFileOffset;
        instructions.Add(new(
            JnzRawFileOffset,
            GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                JnzRawFileOffset),
            JnzEncoding,
            parentPlan.ResultESP,
            parentPlan.EAXValue,
            parentPlan.EBXValue,
            parentPlan.ECXValue,
            parentPlan.ESIValue,
            Attempted: true,
            Reached: true,
            Committed: true,
            Taken: taken,
            TargetRawFileOffset: target,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: parentPlan.FlagsAtStop));

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            taken
                ? GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzTakenTo34E2
                : GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzNotTakenTo34CB,
            parentBranchAdmitted: true,
            parentPlan.ResultESP,
            parentPlan.EAXValue,
            parentPlan.EBXValue,
            parentPlan.ECXValue,
            parentPlan.ESIValue,
            parentPlan.FlagsAtStop,
            target,
            target);
    }
}
