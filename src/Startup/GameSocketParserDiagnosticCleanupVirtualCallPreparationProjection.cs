using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture(
    GameSocketParserDiagnosticCleanupXaddBranchCapture ParentCapture,
    uint? FreshVptrDWORDAtESI);

public enum GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
{
    ParentBranchNotAdmitted,
    MissingFreshVptrDWORDAt34CB,
    BoundedBeforeIndirectCallAt34CF
}

public enum GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
{
    MoveEAXFromESIAt34CB,
    MoveECXFromESIAt34CD
}

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallPreparationInstruction(
    GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind Kind,
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

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallPreparationMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured);

public sealed class GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan
{
    internal GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan(
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture capture,
        GameSocketParserDiagnosticCleanupXaddBranchPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupVirtualCallPreparationInstruction> instructions,
        GameSocketParserDiagnosticCleanupVirtualCallPreparationMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState captureState,
        GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome outcome,
        bool parentBranchAdmitted,
        uint? resultESP,
        uint? eaxValue,
        uint? ebxValue,
        uint? ecxValue,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        MemoryRead = memoryRead;
        FreshVptrCaptureState = captureState;
        Outcome = outcome;
        ParentBranchAdmitted = parentBranchAdmitted;
        ResultESP = resultESP;
        EAXValue = eaxValue;
        EBXValue = ebxValue;
        ECXValue = ecxValue;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupXaddBranchPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupVirtualCallPreparationInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallPreparationMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState FreshVptrCaptureState { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public uint? ResultESP { get; }
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ECXValue { get; }
    public uint? ESIValue { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? IndirectCallRawFileOffset =>
        ParentBranchAdmitted
            ? GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .IndirectCallRawFileOffset
            : null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool FreshVptrReadAttempted => MemoryRead is { Attempted: true };
    public bool FreshVptrReadCaptured => MemoryRead is { Captured: true };
    public bool ESPUnchanged =>
        ParentBranchAdmitted
        && ResultESP.HasValue
        && Instructions.All(instruction =>
            instruction.StackPointerBefore == ResultESP
            && instruction.StackPointerAfter == ResultESP);
    public bool EBXUnchanged => ParentBranchAdmitted && EBXValue == ParentPlan.EBXValue;
    public bool ESIUnchanged => ParentBranchAdmitted && ESIValue == ParentPlan.ESIValue;
    public bool FlagsPropagated =>
        ParentBranchAdmitted
        && Instructions.All(instruction =>
            instruction.FlagsBefore == ParentPlan.FlagsAtStop
            && instruction.FlagsAfter == ParentPlan.FlagsAtStop)
        && FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && ReadsAndEffectsPreserved();
    public bool ExactBoundedCompletion =>
        ParentBranchAdmitted
            ? Outcome
                    == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                        .BoundedBeforeIndirectCallAt34CF
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                        .IndirectCallRawFileOffset
                && Instructions.Length == 2
                && Instructions.All(instruction =>
                    instruction.Attempted && instruction.Reached && instruction.Committed)
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentBranchAdmitted
        && ParentPlan.IsCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                .BoundedBeforeIndirectCallAt34CF
        && ExactBoundedCompletion
        && Instructions.Length == 2
        && Instructions.All(instruction =>
            instruction.Attempted && instruction.Reached && instruction.Committed)
        && FreshVptrReadAttempted
        && FreshVptrReadCaptured
        && EAXValue == Capture.FreshVptrDWORDAtESI
        && ECXValue == ESIValue
        && EBXUnchanged
        && ESIUnchanged
        && ESPUnchanged
        && FlagsPropagated
        && OrderedSemanticPrefixPreserved
        && !IndirectCallAttempted
        && !IndirectCallReached
        && !IndirectCallCommitted
        && !VtableSlotRead
        && !CleanupReleaseIdentityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !ParserExecutionModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    public bool IndirectCallAttempted => false;
    public bool IndirectCallReached => false;
    public bool IndirectCallCommitted => false;
    public bool VtableSlotRead => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupXaddBranchPlan reprojected =
            GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentBranchAdmitted == ParentPlan.ParentBranchAdmitted
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ECXValue == ParentPlan.ECXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion
            && reprojected.OrderedSemanticPrefixPreserved
                == ParentPlan.OrderedSemanticPrefixPreserved;
    }

    private bool ReadsAndEffectsPreserved() =>
        Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
}

/// <summary>Projects the two register moves after the clean 34CB branch endpoint.</summary>
public static class GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
{
    public const uint VptrReadRawFileOffset = 0x00BD34CB;
    public const uint MoveECXRawFileOffset = 0x00BD34CD;
    public const uint IndirectCallRawFileOffset = 0x00BD34CF;
    public const string VptrReadEncoding = "8B 06";
    public const string MoveECXEncoding = "8B CE";
    public const string CoordinatorProvidedBytes =
        "F0 0F C1 08 75 17 8B 06 8B CE FF 10 8D 46 08 F0";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan Project(
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture capture)
    {
        GameSocketParserDiagnosticCleanupXaddBranchPlan parentPlan =
            GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupVirtualCallPreparationInstruction>();
        bool parentBranchAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzNotTakenTo34CB
            && parentPlan.StopBeforeRawFileOffset == VptrReadRawFileOffset
            && parentPlan.FlagsAtStop?.Zero == true
            && parentPlan.ParentPlan.MemoryRead is {
                Value: 1,
                Attempted: true,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && parentPlan.ParentPlan.MemoryWrite is {
                OldValue: 1,
                NewValue: 0,
                Attempted: true,
                Committed: true
            }
            && parentPlan.ResultESP.HasValue
            && parentPlan.EAXValue.HasValue
            && parentPlan.EBXValue.HasValue
            && parentPlan.ECXValue.HasValue
            && parentPlan.ESIValue.HasValue;

        if (!parentBranchAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead: null,
                captureState: capture.FreshVptrDWORDAtESI.HasValue
                    ? GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                        .SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                        .NotReached,
                outcome: GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                resultESP: parentPlan.ResultESP,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                ecxValue: parentPlan.ECXValue,
                esiValue: parentPlan.ESIValue,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint esp = parentPlan.ResultESP!.Value;
        uint esi = parentPlan.ESIValue!.Value;
        uint? freshVptr = capture.FreshVptrDWORDAtESI;
        var memoryRead = new GameSocketParserDiagnosticCleanupVirtualCallPreparationMemoryRead(
            VptrReadRawFileOffset,
            esi,
            freshVptr,
            Attempted: true,
            Captured: freshVptr.HasValue);

        if (!freshVptr.HasValue)
        {
            instructions.Add(new(
                GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
                    .MoveEAXFromESIAt34CB,
                VptrReadRawFileOffset,
                ToCodeVA(VptrReadRawFileOffset),
                VptrReadEncoding,
                esp,
                esp,
                "EAX",
                parentPlan.EAXValue,
                "[ESI]",
                esi,
                Attempted: false,
                Reached: true,
                Committed: false,
                FlagsBefore: parentPlan.FlagsAtStop,
                FlagsAfter: parentPlan.FlagsAtStop));
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead,
                GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState.Missing,
                GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .MissingFreshVptrDWORDAt34CB,
                parentBranchAdmitted: true,
                resultESP: esp,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                ecxValue: parentPlan.ECXValue,
                esiValue: esi,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: VptrReadRawFileOffset);
        }

        instructions.Add(new(
            GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
                .MoveEAXFromESIAt34CB,
            VptrReadRawFileOffset,
            ToCodeVA(VptrReadRawFileOffset),
            VptrReadEncoding,
            esp,
            esp,
            "EAX",
            freshVptr.Value,
            "[ESI]",
            esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: parentPlan.FlagsAtStop));
        instructions.Add(new(
            GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
                .MoveECXFromESIAt34CD,
            MoveECXRawFileOffset,
            ToCodeVA(MoveECXRawFileOffset),
            MoveECXEncoding,
            esp,
            esp,
            "ECX",
            esi,
            "ESI",
            esi,
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
            GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState.Captured,
            GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                .BoundedBeforeIndirectCallAt34CF,
            parentBranchAdmitted: true,
            resultESP: esp,
            eaxValue: freshVptr.Value,
            ebxValue: parentPlan.EBXValue,
            ecxValue: esi,
            esiValue: esi,
            flagsAtStop: parentPlan.FlagsAtStop,
            stopBeforeRawFileOffset: IndirectCallRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
