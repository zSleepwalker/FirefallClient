using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture(
    GameSocketParserDiagnosticCleanupSecondXaddBranchCapture ParentCapture,
    uint? FreshOperationVptrDWORDAtESI);

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
{
    ParentBranchNotAdmitted,
    MissingFreshOperationVptrDWORDAt34DB,
    BoundedBeforeSlot4CallAt34DF
}

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
{
    MoveEAXFromESIAt34DB,
    MoveECXFromESIAt34DD
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstruction(
    GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    string DestinationRegister,
    uint? DestinationValue,
    string SourceRegister,
    uint? SourceValue,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured);

public sealed class GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondXaddBranchPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan(
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture capture,
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstruction>
            instructions,
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState captureState,
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome outcome,
        bool parentBranchAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? eaxValue,
        uint? ebxValue,
        uint? ecxValue,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        MemoryRead = memoryRead;
        FreshOperationVptrCaptureState = captureState;
        Outcome = outcome;
        ParentBranchAdmitted = parentBranchAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        EAXValue = eaxValue;
        EBXValue = ebxValue;
        ECXValue = ecxValue;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                Capture.ParentCapture));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddBranchPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
        FreshOperationVptrCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ECXValue { get; }
    public uint? ESIValue { get; }
    public uint? ESPValue => null;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                StopBeforeRawFileOffset.Value)
            : null;
    public uint? Slot4CallRawFileOffset =>
        ParentBranchAdmitted
            ? GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .Slot4CallRawFileOffset
            : null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool FreshOperationVptrReadAttempted => MemoryRead is { Attempted: true };
    public bool FreshOperationVptrReadCaptured => MemoryRead is { Captured: true };
    public bool MoveEAXAttempted => Instructions.Length >= 1 && Instructions[0].Attempted;
    public bool MoveEAXReached => Instructions.Length >= 1 && Instructions[0].Reached;
    public bool MoveEAXCommitted => Instructions.Length >= 1 && Instructions[0].Committed;
    public bool MoveECXAttempted => Instructions.Length == 2 && Instructions[1].Attempted;
    public bool MoveECXReached => Instructions.Length == 2 && Instructions[1].Reached;
    public bool MoveECXCommitted => Instructions.Length == 2 && Instructions[1].Committed;
    public bool ESPRemainsUnknown =>
        ParentBranchAdmitted
        && ESPValue is null
        && Instructions.All(instruction =>
            instruction.StackPointerBefore is null
            && instruction.StackPointerAfter is null);
    public bool EBXUnchanged => ParentBranchAdmitted && EBXValue == ParentPlan.EBXValue;
    public bool ESIUnchanged => ParentBranchAdmitted && ESIValue == ParentPlan.ESIValue;
    public bool FlagsPropagated =>
        ParentBranchAdmitted
        && Instructions.All(instruction =>
            instruction.FlagsBefore == ParentPlan.FlagsAtStop
            && instruction.FlagsAfter == ParentPlan.FlagsAtStop)
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool RegistersPreserved =>
        ParentBranchAdmitted
        && EBXValue == 0
        && ESIUnchanged
        && ESPRemainsUnknown
        && Instructions.All(instruction =>
            instruction.StackPointerBefore is null
            && instruction.StackPointerAfter is null);
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion =>
        ParentBranchAdmitted
            ? ChildBoundedCompletionBoundaryReached
                && ExactAdmittedInstructionPrefix()
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    public bool Slot4CallAttempted => false;
    public bool Slot4CallReached => false;
    public bool Slot4CallCommitted => false;
    public bool Slot4TargetRead => false;
    public bool PointerValidityModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);

    private bool ComputeExactCleanCompletion() =>
        ParentBranchAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                .BoundedBeforeSlot4CallAt34DF
        && FreshOperationVptrCaptureState
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                .Captured
        && MemoryRead is {
            InstructionRawFileOffset: GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .VptrReadRawFileOffset,
            Address: not null,
            Value: not null,
            Attempted: true,
            Captured: true
        }
        && MemoryRead.Value.Address == ESIValue
        && MemoryRead.Value.Value == Capture.FreshOperationVptrDWORDAtESI
        && EAXValue == Capture.FreshOperationVptrDWORDAtESI
        && ECXValue == ESIValue
        && Instructions.Length == 2
        && Instructions[0].Kind
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                .MoveEAXFromESIAt34DB
        && Instructions[0].Encoding
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .VptrReadEncoding
        && Instructions[1].Kind
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                .MoveECXFromESIAt34DD
        && Instructions[1].Encoding
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .MoveECXEncoding
        && Instructions.All(instruction =>
            instruction.Attempted && instruction.Reached && instruction.Committed)
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .Slot4CallRawFileOffset
        && ExactBoundedCompletion
        && RegistersPreserved
        && FlagsPropagated
        && OrderedSemanticPrefixPreserved
        && !Slot4CallAttempted
        && !Slot4CallReached
        && !Slot4CallCommitted
        && !Slot4TargetRead
        && !PointerValidityModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !NativeMemoryEffectsModeled
        && !ParserExecutionModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    private bool ExactAdmittedInstructionPrefix() =>
        Outcome
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                .BoundedBeforeSlot4CallAt34DF
        && FreshOperationVptrCaptureState
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                .Captured
        && Instructions.Length == 2
        && Instructions.All(instruction =>
            instruction.Attempted && instruction.Reached && instruction.Committed)
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .Slot4CallRawFileOffset;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentAdmitted == ParentPlan.ParentAdmitted
            && reprojected.ChildBoundedCompletionBoundaryReached
                == ParentPlan.ChildBoundedCompletionBoundaryReached
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    }
}

/// <summary>Projects the operation-vptr load and register move before the slot-four call.</summary>
public static class GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
{
    public const uint VptrReadRawFileOffset = 0x00BD34DB;
    public const uint MoveECXRawFileOffset = 0x00BD34DD;
    public const uint Slot4CallRawFileOffset = 0x00BD34DF;
    public const string VptrReadEncoding = "8B 06";
    public const string MoveECXEncoding = "8B CE";
    public const string CoordinatorProvidedBytes =
        "4B 75 07 8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan Project(
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture capture,
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted XADD-branch plan must match the captured parent.");

        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstruction>();
        bool parentBranchAdmitted =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentAdmitted
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzNotTakenTo34DB
            && parentPlan.StopBeforeRawFileOffset == VptrReadRawFileOffset
            && parentPlan.BranchTargetRawFileOffset == VptrReadRawFileOffset
            && parentPlan.EBXValue == 0
            && parentPlan.EAXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.ParentPlan.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
            && parentPlan.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
            && parentPlan.Capture.ParentCapture.ParentCapture.FreshPostCallESI
                == parentPlan.ESIValue
            && parentPlan.ParentPlan.EBXValue == 1
            && parentPlan.ParentPlan.MemoryRead is {
                Value: 1,
                Attempted: true,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && parentPlan.ParentPlan.MemoryWrite is {
                OldValue: 1,
                Attempted: true,
                Committed: true
            }
            && parentPlan.FlagsAtStop is {
                Carry: not null,
                Parity: not null,
                AuxiliaryCarry: not null,
                Zero: true,
                Sign: not null,
                Overflow: not null
            }
            && parentPlan.ECXValue is null
            && parentPlan.ESPValue is null;

        if (!parentBranchAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead: null,
                captureState: capture.FreshOperationVptrDWORDAtESI.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                        .SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                        .NotReached,
                outcome: GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                    .ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                ecxValue: parentPlan.ECXValue,
                esiValue: parentPlan.ESIValue,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: parentPlan.LastReachedRawFileOffset,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint esi = parentPlan.ESIValue!.Value;
        uint? freshVptr = capture.FreshOperationVptrDWORDAtESI;
        var memoryRead =
            new GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationMemoryRead(
                VptrReadRawFileOffset,
                esi,
                freshVptr,
                Attempted: true,
                Captured: freshVptr.HasValue);

        if (!freshVptr.HasValue)
        {
            instructions.Add(new(
                GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                    .MoveEAXFromESIAt34DB,
                VptrReadRawFileOffset,
                ToCodeVA(VptrReadRawFileOffset),
                VptrReadEncoding,
                StackPointerBefore: null,
                StackPointerAfter: null,
                DestinationRegister: "EAX",
                DestinationValue: parentPlan.EAXValue,
                SourceRegister: "[ESI]",
                SourceValue: esi,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead,
                GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                    .Missing,
                GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                    .MissingFreshOperationVptrDWORDAt34DB,
                parentBranchAdmitted: true,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                ecxValue: null,
                esiValue: esi,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: VptrReadRawFileOffset,
                stopBeforeRawFileOffset: VptrReadRawFileOffset);
        }

        instructions.Add(new(
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                .MoveEAXFromESIAt34DB,
            VptrReadRawFileOffset,
            ToCodeVA(VptrReadRawFileOffset),
            VptrReadEncoding,
            StackPointerBefore: null,
            StackPointerAfter: null,
            DestinationRegister: "EAX",
            DestinationValue: freshVptr.Value,
            SourceRegister: "[ESI]",
            SourceValue: esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: parentPlan.FlagsAtStop));
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                .MoveECXFromESIAt34DD,
            MoveECXRawFileOffset,
            ToCodeVA(MoveECXRawFileOffset),
            MoveECXEncoding,
            StackPointerBefore: null,
            StackPointerAfter: null,
            DestinationRegister: "ECX",
            DestinationValue: esi,
            SourceRegister: "ESI",
            SourceValue: esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: parentPlan.FlagsAtStop));

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            memoryRead,
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                .Captured,
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                .BoundedBeforeSlot4CallAt34DF,
            parentBranchAdmitted: true,
            childBoundedCompletionBoundaryReached: true,
            eaxValue: freshVptr.Value,
            ebxValue: parentPlan.EBXValue,
            ecxValue: esi,
            esiValue: esi,
            flagsAtStop: parentPlan.FlagsAtStop,
            lastReachedRawFileOffset: MoveECXRawFileOffset,
            stopBeforeRawFileOffset: Slot4CallRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
