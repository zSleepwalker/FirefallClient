using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddBranchCapture(
    GameSocketParserDiagnosticCleanupSecondXaddCapture ParentCapture);

public enum GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
{
    ParentNotAdmitted,
    JnzNotTakenTo34DB,
    JnzTakenTo34E2
}

public enum GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
{
    DecrementEBXAt34D8,
    JnzAt34D9
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddBranchInstruction(
    GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointer,
    uint? EAX,
    uint? EBXBefore,
    uint? EBXAfter,
    uint? ECX,
    uint? ESI,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool? Taken,
    uint? TargetRawFileOffset,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticCleanupSecondXaddBranchPlan
{
    private readonly System.Lazy<bool> _orderedSemanticPrefixPreserved;
    private readonly System.Lazy<bool> _exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondXaddBranchPlan(
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture capture,
        GameSocketParserDiagnosticCleanupSecondXaddPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondXaddBranchInstruction> instructions,
        GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome outcome,
        bool parentAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? eaxValue,
        uint? ebxValue,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Outcome = outcome;
        ParentAdmitted = parentAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        EAXValue = eaxValue;
        EBXValue = ebxValue;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        _orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved);
        _exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondXaddBranchCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondXaddBranchInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome Outcome { get; }
    public bool ParentAdmitted { get; }
    public bool ParentExactCompletion => ParentPlan.ExactChildCompletion;
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool ExactBoundedCompletion =>
        ParentAdmitted
            ? ChildBoundedCompletionBoundaryReached
                && (Outcome is GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                        .JnzNotTakenTo34DB
                    or GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                        .JnzTakenTo34E2)
                && Instructions.Length == 2
                && Instructions[0].Kind
                    == GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                        .DecrementEBXAt34D8
                && Instructions[0].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondXaddBranchProjection
                        .DecRawFileOffset
                && Instructions[1].Kind
                    == GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                        .JnzAt34D9
                && Instructions[1].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondXaddBranchProjection
                        .JnzRawFileOffset
                && Instructions.All(instruction =>
                    instruction.Attempted && instruction.Reached && instruction.Committed)
                && StopBeforeRawFileOffset == BranchTargetRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactCleanCompletion => _exactCleanCompletion.Value;

    private bool ComputeExactCleanCompletion() =>
        ParentAdmitted
        && ParentPlan.ExactCleanCompletion
        && (Outcome is GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                .JnzNotTakenTo34DB
            or GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2)
        && Instructions.Length == 2
        && Instructions[0].Kind
            == GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                .DecrementEBXAt34D8
        && Instructions[0].Encoding
            == GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.DecEncoding
        && Instructions[1].Kind
            == GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind.JnzAt34D9
        && Instructions[1].Encoding
            == GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.JnzEncoding
        && ChildBoundedCompletionBoundaryReached
        && ExactBoundedCompletion
        && FlagsPropagated
        && RegistersPreserved
        && OrderedSemanticPrefixPreserved
        && !BranchTargetInstructionExecuted
        && !FollowingTargetInstructionExecuted
        && !NativeAtomicSemanticsModeled
        && !ConcurrencyModeled
        && !LinearizabilityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ECXValue => null;
    public uint? ESIValue { get; }
    public uint? ESPValue => null;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(StopBeforeRawFileOffset.Value)
            : null;
    public uint? BranchTargetRawFileOffset =>
        ParentAdmitted && Instructions.Length == 2
            ? Instructions[1].TargetRawFileOffset
            : null;
    public uint? BranchTargetCodeVirtualAddress =>
        BranchTargetRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(BranchTargetRawFileOffset.Value)
            : null;
    public uint? FallThroughRawFileOffset =>
        ParentAdmitted
            ? GameSocketParserDiagnosticCleanupSecondXaddBranchProjection
                .FallThroughRawFileOffset
            : null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool DecrementAttempted =>
        Instructions.Length == 2 && Instructions[0].Attempted;
    public bool DecrementReached =>
        Instructions.Length == 2 && Instructions[0].Reached;
    public bool DecrementCommitted =>
        Instructions.Length == 2 && Instructions[0].Committed;
    public bool JnzAttempted =>
        Instructions.Length == 2 && Instructions[1].Attempted;
    public bool JnzReached =>
        Instructions.Length == 2 && Instructions[1].Reached;
    public bool JnzCommitted =>
        Instructions.Length == 2 && Instructions[1].Committed;
    public bool JnzTaken =>
        Instructions.Length == 2 && Instructions[1].Taken == true;
    public bool CarryPreservedByDecrement =>
        ParentAdmitted
        && Instructions.Length == 2
        && Instructions[0].FlagsBefore?.Carry == Instructions[0].FlagsAfter?.Carry
        && Instructions[0].FlagsAfter?.Carry == ParentPlan.FlagsAtStop?.Carry;
    public bool JnzFlagsPreserved =>
        ParentAdmitted
        && Instructions.Length == 2
        && Instructions[1].FlagsBefore == Instructions[1].FlagsAfter
        && Instructions[1].FlagsAfter == FlagsAtStop;
    public bool FlagsPropagated =>
        ParentAdmitted
        && Instructions.Length == 2
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == Instructions[1].FlagsBefore
        && Instructions[1].FlagsAfter == FlagsAtStop
        && CarryPreservedByDecrement
        && JnzFlagsPreserved;
    public bool RegistersPreserved =>
        ParentAdmitted
        && EAXValue == ParentPlan.EAXValue
        && ESIValue == ParentPlan.ESIValue
        && ECXValue is null
        && ESPValue is null
        && Instructions.Length == 2
        && Instructions.All(instruction =>
            instruction.EAX == EAXValue
            && instruction.EBXAfter == EBXValue
            && instruction.ECX is null
            && instruction.ESI == ESIValue
            && instruction.StackPointer is null);
    public bool OrderedSemanticPrefixPreserved =>
        _orderedSemanticPrefixPreserved.Value;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    public bool BranchTargetInstructionExecuted => false;
    public bool FollowingTargetInstructionExecuted => false;
    public bool NativeAtomicSemanticsModeled => false;
    public bool ConcurrencyModeled => false;
    public bool LinearizabilityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondXaddPlan reprojected =
            GameSocketParserDiagnosticCleanupSecondXaddProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentAdmitted == ParentPlan.ParentAdmitted
            && reprojected.ChildBoundedCompletionBoundaryReached
                == ParentPlan.ChildBoundedCompletionBoundaryReached
            && reprojected.FreshPostCallEBXCaptureState
                == ParentPlan.FreshPostCallEBXCaptureState
            && reprojected.FreshDWORDCaptureState == ParentPlan.FreshDWORDCaptureState
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.MemoryWrite == ParentPlan.MemoryWrite
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    }
}

/// <summary>Projects DEC EBX and its immediately following JNZ without entering either target.</summary>
public static class GameSocketParserDiagnosticCleanupSecondXaddBranchProjection
{
    public const uint DecRawFileOffset = 0x00BD34D8;
    public const uint JnzRawFileOffset = 0x00BD34D9;
    public const uint FallThroughRawFileOffset = 0x00BD34DB;
    public const uint JnzTargetRawFileOffset = 0x00BD34E2;
    public const string DecEncoding = "4B";
    public const string JnzEncoding = "75 07";
    public const string CoordinatorProvidedBytes =
        "4B 75 07 8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupSecondXaddBranchPlan Project(
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondXaddPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondXaddProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondXaddBranchInstruction>();
        bool parentAdmitted =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentAdmitted
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddOutcome.BoundedBeforeDecAt34D8
            && parentPlan.StopBeforeRawFileOffset == DecRawFileOffset
            && parentPlan.EAXValue.HasValue
            && parentPlan.EBXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
            && parentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
            && parentPlan.RmwWriteCommitted
            && HasKnownFlags(parentPlan.FlagsAtStop)
            && parentPlan.ECXValue is null
            && parentPlan.ESPValue is null;

        if (!parentAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.ParentNotAdmitted,
                parentAdmitted: false,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                esiValue: parentPlan.ESIValue,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: parentPlan.LastReachedRawFileOffset,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint eax = parentPlan.EAXValue!.Value;
        uint oldEBX = parentPlan.EBXValue!.Value;
        uint esi = parentPlan.ESIValue!.Value;
        GameSocketParserStoredByteSelectorGateFlags flagsBefore =
            parentPlan.FlagsAtStop!.Value;
        uint decrementedEBX = unchecked(oldEBX - 1);
        GameSocketParserStoredByteSelectorGateFlags decrementFlags =
            FlagsForDecrement(oldEBX, flagsBefore);
        bool taken = decrementFlags.Zero == false;
        uint target = taken ? JnzTargetRawFileOffset : FallThroughRawFileOffset;

        instructions.Add(new(
            GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                .DecrementEBXAt34D8,
            DecRawFileOffset,
            ToCodeVA(DecRawFileOffset),
            DecEncoding,
            StackPointer: null,
            EAX: eax,
            EBXBefore: oldEBX,
            EBXAfter: decrementedEBX,
            ECX: null,
            ESI: esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            Taken: null,
            TargetRawFileOffset: null,
            FlagsBefore: flagsBefore,
            FlagsAfter: decrementFlags));
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind.JnzAt34D9,
            JnzRawFileOffset,
            ToCodeVA(JnzRawFileOffset),
            JnzEncoding,
            StackPointer: null,
            EAX: eax,
            EBXBefore: decrementedEBX,
            EBXAfter: decrementedEBX,
            ECX: null,
            ESI: esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            Taken: taken,
            TargetRawFileOffset: target,
            FlagsBefore: decrementFlags,
            FlagsAfter: decrementFlags));

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            taken
                ? GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
                : GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzNotTakenTo34DB,
            parentAdmitted: true,
            childBoundedCompletionBoundaryReached: true,
            eaxValue: eax,
            ebxValue: decrementedEBX,
            esiValue: esi,
            flagsAtStop: decrementFlags,
            lastReachedRawFileOffset: JnzRawFileOffset,
            stopBeforeRawFileOffset: target);
    }

    private static bool HasKnownFlags(
        GameSocketParserStoredByteSelectorGateFlags? flags) =>
        flags is {
            Carry: not null,
            Parity: not null,
            AuxiliaryCarry: not null,
            Zero: not null,
            Sign: not null,
            Overflow: not null
        };

    private static GameSocketParserStoredByteSelectorGateFlags FlagsForDecrement(
        uint oldValue,
        GameSocketParserStoredByteSelectorGateFlags priorFlags)
    {
        uint result = unchecked(oldValue - 1);
        bool evenParity = (System.Numerics.BitOperations.PopCount((byte)result) & 1) == 0;
        return new(
            Carry: priorFlags.Carry,
            Parity: evenParity,
            AuxiliaryCarry: (oldValue & 0xF) == 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: oldValue == 0x80000000);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
