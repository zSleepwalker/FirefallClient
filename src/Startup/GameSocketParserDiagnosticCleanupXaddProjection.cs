using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupXaddCapture(
    GameSocketParserDiagnosticCleanupRegisterPreparationCapture ParentCapture,
    uint? FreshDWORDObservedAtLockedRmw);

public enum GameSocketParserDiagnosticCleanupXaddCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupXaddOutcome
{
    ParentBranchNotAdmitted,
    MissingFreshDWORDAt34C5,
    BoundedBeforeJnzAt34C9
}

public readonly record struct GameSocketParserDiagnosticCleanupXaddInstruction(
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    bool LockPrefixPresent,
    uint? StackPointer,
    uint? MemoryAddress,
    uint? EAXBefore,
    uint? EAXAfter,
    uint? EBXBefore,
    uint? EBXAfter,
    uint? ECXBefore,
    uint? ECXAfter,
    uint? ESIBefore,
    uint? ESIAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticCleanupXaddMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool ObservedAtLockedRmw);

public readonly record struct GameSocketParserDiagnosticCleanupXaddMemoryWrite(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? OldValue,
    uint? NewValue,
    bool Attempted,
    bool Committed);

public sealed class GameSocketParserDiagnosticCleanupXaddPlan
{
    internal GameSocketParserDiagnosticCleanupXaddPlan(
        GameSocketParserDiagnosticCleanupXaddCapture capture,
        GameSocketParserDiagnosticCleanupRegisterPreparationPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupXaddInstruction> instructions,
        GameSocketParserDiagnosticCleanupXaddMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupXaddMemoryWrite? memoryWrite,
        GameSocketParserDiagnosticCleanupXaddCaptureState captureState,
        GameSocketParserDiagnosticCleanupXaddOutcome outcome,
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
        MemoryRead = memoryRead;
        MemoryWrite = memoryWrite;
        FreshDWORDCaptureState = captureState;
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

    public GameSocketParserDiagnosticCleanupXaddCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupRegisterPreparationPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupXaddInstruction> Instructions { get; }
    public GameSocketParserDiagnosticCleanupXaddMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupXaddMemoryWrite? MemoryWrite { get; }
    public GameSocketParserDiagnosticCleanupXaddCaptureState FreshDWORDCaptureState { get; }
    public GameSocketParserDiagnosticCleanupXaddOutcome Outcome { get; }
    public bool ParentBranchAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
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
    public uint? LockedXaddRawFileOffset =>
        ParentBranchAdmitted ? GameSocketParserDiagnosticCleanupXaddProjection.LockedXaddRawFileOffset : null;
    public bool FreshDWORDReadAttempted => MemoryRead is { Attempted: true };
    public bool FreshDWORDCaptured => MemoryRead is { Captured: true };
    public bool RmwWriteAttempted => MemoryWrite is { Attempted: true };
    public bool RmwWriteCommitted => MemoryWrite is { Committed: true };
    public bool ECXReceivedOldMemoryValue =>
        RmwWriteCommitted
        && MemoryRead is { Captured: true, Value: not null }
        && ECXValue == MemoryRead.Value.Value;
    public bool EAXUnchanged =>
        ParentBranchAdmitted && EAXValue == ParentPlan.EAXValue;
    public bool EBXUnchanged =>
        ParentBranchAdmitted && EBXValue == ParentPlan.EBXValue;
    public bool ESIUnchanged =>
        ParentBranchAdmitted && ESIValue == ParentPlan.ESIValue;
    public bool ESPUnchanged =>
        ParentBranchAdmitted
        && ResultESP.HasValue
        && Instructions.All(instruction =>
            instruction.StackPointer == ResultESP);
    public bool FlagsPropagated =>
        ParentBranchAdmitted
        && MemoryWrite is { Committed: true }
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == FlagsAtStop
        && FlagsAtStop == GameSocketParserDiagnosticCleanupXaddProjection.FlagsForAdd(
            Capture.FreshDWORDObservedAtLockedRmw!.Value,
            MemoryWrite.Value.NewValue!.Value);
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && ReadsAndEffectsPreserved();
    public bool ExactBoundedCompletion =>
        !ParentBranchAdmitted
            ? ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            : ParentBranchAdmitted
        && Outcome == GameSocketParserDiagnosticCleanupXaddOutcome.BoundedBeforeJnzAt34C9
        && Instructions.Length == 1
        && Instructions[0].InstructionRawFileOffset
            == GameSocketParserDiagnosticCleanupXaddProjection.LockedXaddRawFileOffset
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupXaddProjection.JnzRawFileOffset
        && RmwWriteCommitted;
    public bool ReachedBoundedEndpoint => ExactBoundedCompletion;
    public bool IsCleanCompletion =>
        ParentBranchAdmitted
        && ParentPlan.IsCleanCompletion
        && ParentExactBoundedCompletion
        && ExactBoundedCompletion
        && Instructions.Length == 1
        && Instructions[0].Attempted
        && Instructions[0].Reached
        && Instructions[0].Committed
        && FreshDWORDReadAttempted
        && FreshDWORDCaptured
        && RmwWriteAttempted
        && RmwWriteCommitted
        && ECXReceivedOldMemoryValue
        && EAXUnchanged
        && EBXUnchanged
        && ESIUnchanged
        && ESPUnchanged
        && FlagsPropagated
        && OrderedSemanticPrefixPreserved
        && LockPrefixRecorded
        && !NativeAtomicSemanticsModeled
        && !ConcurrencyModeled
        && !LinearizabilityModeled
        && !CleanupReleaseIdentityModeled;

    public bool LockPrefixRecorded =>
        Instructions.Length == 1 && Instructions[0].LockPrefixPresent;
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
        GameSocketParserDiagnosticCleanupRegisterPreparationPlan reprojected =
            GameSocketParserDiagnosticCleanupRegisterPreparationProjection.Project(
                Capture.ParentCapture);
        return reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentBranchAdmitted == ParentPlan.ParentBranchAdmitted
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ECXValue == ParentPlan.ECXValue
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion;
    }

    private bool ReadsAndEffectsPreserved() =>
        ParentPlan.Reads.SequenceEqual(ParentPlan.ParentPlan.Reads)
        && ParentPlan.MemoryEffects.SequenceEqual(ParentPlan.ParentPlan.MemoryEffects);
}

/// <summary>Projects the captured locked decrement through the branch immediately after it.</summary>
public static class GameSocketParserDiagnosticCleanupXaddProjection
{
    public const uint LockedXaddRawFileOffset = 0x00BD34C5;
    public const uint JnzRawFileOffset = 0x00BD34C9;
    public const string LockedXaddEncoding = "F0 0F C1 08";
    public const string CoordinatorProvidedBytes =
        "F0 0F C1 08 75 17 8B 06 8B CE FF 10 8D 46 08 F0";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupXaddPlan Project(
        GameSocketParserDiagnosticCleanupXaddCapture capture)
    {
        GameSocketParserDiagnosticCleanupRegisterPreparationPlan parentPlan =
            GameSocketParserDiagnosticCleanupRegisterPreparationProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupXaddInstruction>();
        bool parentBranchAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                    .BoundedBeforeLockXaddAt34C5
            && parentPlan.StopBeforeRawFileOffset == LockedXaddRawFileOffset
            && parentPlan.EAXValue.HasValue
            && parentPlan.EBXValue == uint.MaxValue
            && parentPlan.ECXValue == uint.MaxValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.ResultESP.HasValue
            && parentPlan.FlagsAtStop.HasValue;

        if (!parentBranchAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead: null,
                memoryWrite: null,
                captureState: capture.FreshDWORDObservedAtLockedRmw.HasValue
                    ? GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupXaddCaptureState.NotReached,
                outcome: GameSocketParserDiagnosticCleanupXaddOutcome.ParentBranchNotAdmitted,
                parentBranchAdmitted: false,
                resultESP: parentPlan.ResultESP,
                esiValue: parentPlan.ESIValue,
                eaxValue: parentPlan.EAXValue,
                ebxValue: parentPlan.EBXValue,
                ecxValue: parentPlan.ECXValue,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint esp = parentPlan.ResultESP!.Value;
        uint esi = parentPlan.ESIValue!.Value;
        uint eax = parentPlan.EAXValue!.Value;
        uint ebx = parentPlan.EBXValue!.Value;
        uint ecx = parentPlan.ECXValue!.Value;
        uint? oldValue = capture.FreshDWORDObservedAtLockedRmw;
        var memoryRead = new GameSocketParserDiagnosticCleanupXaddMemoryRead(
            LockedXaddRawFileOffset,
            eax,
            oldValue,
            Attempted: true,
            Captured: oldValue.HasValue,
            ObservedAtLockedRmw: true);

        if (!oldValue.HasValue)
        {
            instructions.Add(new(
                LockedXaddRawFileOffset,
                ToCodeVA(LockedXaddRawFileOffset),
                LockedXaddEncoding,
                true,
                esp,
                eax,
                eax,
                eax,
                ebx,
                ebx,
                ecx,
                ecx,
                esi,
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
                new(
                    LockedXaddRawFileOffset,
                    eax,
                    OldValue: null,
                    NewValue: null,
                    Attempted: false,
                    Committed: false),
                GameSocketParserDiagnosticCleanupXaddCaptureState.Missing,
                GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5,
                parentBranchAdmitted: true,
                resultESP: esp,
                esiValue: esi,
                eaxValue: eax,
                ebxValue: ebx,
                ecxValue: ecx,
                flagsAtStop: parentPlan.FlagsAtStop,
                stopBeforeRawFileOffset: LockedXaddRawFileOffset);
        }

        uint newValue = unchecked(oldValue.Value + uint.MaxValue);
        GameSocketParserStoredByteSelectorGateFlags flagsAfter =
            FlagsForAdd(oldValue.Value, newValue);
        instructions.Add(new(
            LockedXaddRawFileOffset,
            ToCodeVA(LockedXaddRawFileOffset),
            LockedXaddEncoding,
            true,
            esp,
            eax,
            eax,
            eax,
            ebx,
            ebx,
            ecx,
            oldValue.Value,
            esi,
            esi,
            Attempted: true,
            Reached: true,
            Committed: true,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: flagsAfter));

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            memoryRead,
            new(
                LockedXaddRawFileOffset,
                eax,
                oldValue.Value,
                newValue,
                Attempted: true,
                Committed: true),
            GameSocketParserDiagnosticCleanupXaddCaptureState.Captured,
            GameSocketParserDiagnosticCleanupXaddOutcome.BoundedBeforeJnzAt34C9,
            parentBranchAdmitted: true,
            resultESP: esp,
            esiValue: esi,
            eaxValue: eax,
            ebxValue: ebx,
            ecxValue: oldValue.Value,
            flagsAtStop: flagsAfter,
            stopBeforeRawFileOffset: JnzRawFileOffset);
    }

    internal static GameSocketParserStoredByteSelectorGateFlags FlagsForAdd(
        uint left,
        uint result)
    {
        byte lowByte = (byte)result;
        bool evenParity = (BitOperations.PopCount(lowByte) & 1) == 0;
        bool auxiliaryCarry = (left & 0xF) != 0;
        bool overflow = ((left ^ result) & 0x80000000) != 0
            && (left & 0x80000000) != 0;
        return new(
            Carry: left != 0,
            Parity: evenParity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: overflow);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
