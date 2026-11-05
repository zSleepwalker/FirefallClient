using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddCapture(
    GameSocketParserDiagnosticCleanupPostCallAddressCapture ParentCapture,
    uint? FreshPostCallEBX,
    uint? FreshDWORDObservedAtLockedRmw);

public enum GameSocketParserDiagnosticCleanupSecondXaddCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondXaddOutcome
{
    ParentNotAdmitted,
    MissingFreshPostCallEBXAt34D4,
    MissingFreshDWORDAt34D4,
    BoundedBeforeDecAt34D8
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddInstruction(
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

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool ObservedAtLockedRmw);

public readonly record struct GameSocketParserDiagnosticCleanupSecondXaddMemoryWrite(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? OldValue,
    uint? NewValue,
    bool Attempted,
    bool Committed);

public sealed class GameSocketParserDiagnosticCleanupSecondXaddPlan
{
    private readonly System.Lazy<bool> _orderedSemanticPrefixPreserved;
    private readonly System.Lazy<bool> _exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondXaddPlan(
        GameSocketParserDiagnosticCleanupSecondXaddCapture capture,
        GameSocketParserDiagnosticCleanupPostCallAddressPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondXaddInstruction> instructions,
        GameSocketParserDiagnosticCleanupSecondXaddMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupSecondXaddMemoryWrite? memoryWrite,
        GameSocketParserDiagnosticCleanupSecondXaddCaptureState ebxCaptureState,
        GameSocketParserDiagnosticCleanupSecondXaddCaptureState dwordCaptureState,
        GameSocketParserDiagnosticCleanupSecondXaddOutcome outcome,
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
        MemoryRead = memoryRead;
        MemoryWrite = memoryWrite;
        FreshPostCallEBXCaptureState = ebxCaptureState;
        FreshDWORDCaptureState = dwordCaptureState;
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

    public GameSocketParserDiagnosticCleanupSecondXaddCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupPostCallAddressPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondXaddInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddMemoryWrite? MemoryWrite { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddCaptureState
        FreshPostCallEBXCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddCaptureState
        FreshDWORDCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddOutcome Outcome { get; }
    public bool ParentAdmitted { get; }
    public bool ParentExactCompletion => ParentPlan.ExactChildCompletion;
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool ExactBoundedCompletion =>
        !ParentAdmitted
            ? ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            : ChildBoundedCompletionBoundaryReached
                && Outcome
                    == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                        .BoundedBeforeDecAt34D8
                && Instructions.Length == 1
                && Instructions[0].InstructionRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondXaddProjection
                        .LockedXaddRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondXaddProjection
                        .ExclusiveStopRawFileOffset
                && RmwWriteCommitted;
    public uint? EAXValue { get; }
    public uint? EBXValue { get; }
    public uint? ESIValue { get; }
    public uint? ECXValue => null;
    public uint? ESPValue => null;
    public bool PostCallECXCaptured => false;
    public bool PostCallESPCaptured => false;
    public bool PostCallEBXCaptured =>
        FreshPostCallEBXCaptureState
            == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(StopBeforeRawFileOffset.Value)
            : null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool FreshDWORDReadAttempted => MemoryRead is { Attempted: true };
    public bool FreshDWORDCaptured => MemoryRead is { Captured: true };
    public bool RmwWriteAttempted => MemoryWrite is { Attempted: true };
    public bool RmwWriteCommitted => MemoryWrite is { Committed: true };
    public bool EBXReceivedOldMemoryValue =>
        RmwWriteCommitted
        && MemoryRead is { Captured: true, Value: not null }
        && EBXValue == MemoryRead.Value.Value;
    public bool WriteIsOldMemoryPlusFreshEBX =>
        RmwWriteCommitted
        && MemoryRead is { Captured: true, Value: not null }
        && Capture.FreshPostCallEBX.HasValue
        && MemoryWrite is { NewValue: not null }
        && MemoryWrite.Value.NewValue
            == unchecked(MemoryRead.Value.Value + Capture.FreshPostCallEBX.Value);
    public bool EAXUnchanged =>
        ParentAdmitted && EAXValue == ParentPlan.EAXValue;
    public bool ESIUnchanged =>
        ParentAdmitted && ESIValue == ParentPlan.ESIValue;
    public bool ESPLineageCaptured => false;
    public bool ECXLineageCaptured => false;
    public bool FlagsPropagated =>
        ParentAdmitted
        && RmwWriteCommitted
        && Instructions.Length == 1
        && Instructions[0].FlagsBefore == ParentPlan.FlagsAtStop
        && Instructions[0].FlagsAfter == FlagsAtStop
        && FlagsAtStop == GameSocketParserDiagnosticCleanupSecondXaddProjection
            .FlagsForAdd(
                Capture.FreshDWORDObservedAtLockedRmw!.Value,
                Capture.FreshPostCallEBX!.Value);
    public bool OrderedSemanticPrefixPreserved =>
        _orderedSemanticPrefixPreserved.Value;
    public bool ExactCleanCompletion => _exactCleanCompletion.Value;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);

    private bool ComputeExactCleanCompletion() =>
        ParentAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                .BoundedBeforeDecAt34D8
        && FreshPostCallEBXCaptureState
            == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
        && FreshDWORDCaptureState
            == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
        && Instructions.Length == 1
        && Instructions[0] is {
            Attempted: true,
            Reached: true,
            Committed: true
        }
        && EAXValue == ParentPlan.EAXValue
        && ESIValue == ParentPlan.ESIValue
        && EBXReceivedOldMemoryValue
        && WriteIsOldMemoryPlusFreshEBX
        && ECXValue is null
        && ESPValue is null
        && ChildBoundedCompletionBoundaryReached
        && ExactBoundedCompletion
        && FlagsPropagated
        && OrderedSemanticPrefixPreserved
        && LockPrefixRecorded
        && !NativeAtomicSemanticsModeled
        && !ConcurrencyModeled
        && !LinearizabilityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;
    public bool LockPrefixRecorded =>
        Instructions.Length == 1 && Instructions[0].LockPrefixPresent;
    public bool NativeAtomicSemanticsModeled => false;
    public bool ConcurrencyModeled => false;
    public bool LinearizabilityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupPostCallAddressPlan reprojected =
            GameSocketParserDiagnosticCleanupPostCallAddressProjection.Project(
                Capture.ParentCapture);
        return reprojected.Outcome == ParentPlan.Outcome
            && reprojected.FreshPostCallESICaptureState
                == ParentPlan.FreshPostCallESICaptureState
            && reprojected.ParentAdmitted == ParentPlan.ParentAdmitted
            && reprojected.ChildBoundedCompletionBoundaryReached
                == ParentPlan.ChildBoundedCompletionBoundaryReached
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset
                == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset
                == ParentPlan.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
            && reprojected.ExactChildCompletion == ParentPlan.ExactChildCompletion
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.OrderedSemanticPrefixPreserved
                == ParentPlan.OrderedSemanticPrefixPreserved;
    }
}

public static class GameSocketParserDiagnosticCleanupSecondXaddProjection
{
    public const uint LockedXaddRawFileOffset = 0x00BD34D4;
    public const uint LockedXaddLength = 4;
    public const uint ExclusiveStopRawFileOffset = 0x00BD34D8;
    public const uint FollowingDecRawFileOffset = 0x00BD34D8;
    public const string LockedXaddEncoding = "F0 0F C1 18";
    public const string FollowingDecEncoding = "4B";
    public const string CoordinatorProvidedBytes =
        "8D 46 08 F0 0F C1 18 4B 75 07 8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D";
    public const bool BytesIndependentlyVerified = false;

    public static GameSocketParserDiagnosticCleanupSecondXaddPlan Project(
        GameSocketParserDiagnosticCleanupSecondXaddCapture capture)
    {
        GameSocketParserDiagnosticCleanupPostCallAddressPlan parentPlan =
            GameSocketParserDiagnosticCleanupPostCallAddressProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondXaddInstruction>();

        bool parentAdmitted =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentAdmitted
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupPostCallAddressOutcome.ProjectedLEAAt34D1
            && parentPlan.StopBeforeRawFileOffset
                == LockedXaddRawFileOffset
            && parentPlan.EAXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && !parentPlan.PostCallEBXCaptured
            && !parentPlan.PostCallECXCaptured
            && !parentPlan.PostCallESPCaptured
            && !parentPlan.FlagsAtStop.HasValue;

        if (!parentAdmitted)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                memoryRead: null,
                memoryWrite: null,
                ebxCaptureState: capture.FreshPostCallEBX.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondXaddCaptureState.NotReached,
                dwordCaptureState: capture.FreshDWORDObservedAtLockedRmw.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondXaddCaptureState.NotReached,
                outcome: GameSocketParserDiagnosticCleanupSecondXaddOutcome.ParentNotAdmitted,
                parentAdmitted: false,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: parentPlan.EAXValue,
                ebxValue: null,
                esiValue: parentPlan.ESIValue,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: parentPlan.LastReachedRawFileOffset,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint eax = parentPlan.EAXValue!.Value;
        uint esi = parentPlan.ESIValue!.Value;
        uint? freshEBX = capture.FreshPostCallEBX;
        if (!freshEBX.HasValue)
        {
            instructions.Add(new(
                LockedXaddRawFileOffset,
                ToCodeVA(LockedXaddRawFileOffset),
                LockedXaddEncoding,
                true,
                null,
                eax,
                eax,
                eax,
                null,
                null,
                null,
                null,
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
                memoryRead: null,
                new(
                    LockedXaddRawFileOffset,
                    eax,
                    OldValue: null,
                    NewValue: null,
                    Attempted: false,
                    Committed: false),
                GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing,
                capture.FreshDWORDObservedAtLockedRmw.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondXaddCaptureState.NotReached,
                GameSocketParserDiagnosticCleanupSecondXaddOutcome
                    .MissingFreshPostCallEBXAt34D4,
                parentAdmitted: true,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: eax,
                ebxValue: null,
                esiValue: esi,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: LockedXaddRawFileOffset,
                stopBeforeRawFileOffset: LockedXaddRawFileOffset);
        }

        uint ebx = freshEBX.Value;
        uint? oldValue = capture.FreshDWORDObservedAtLockedRmw;
        var memoryRead = new GameSocketParserDiagnosticCleanupSecondXaddMemoryRead(
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
                null,
                eax,
                eax,
                eax,
                ebx,
                ebx,
                null,
                null,
                esi,
                esi,
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
                new(
                    LockedXaddRawFileOffset,
                    eax,
                    OldValue: null,
                    NewValue: null,
                    Attempted: false,
                    Committed: false),
                GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured,
                GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing,
                GameSocketParserDiagnosticCleanupSecondXaddOutcome.MissingFreshDWORDAt34D4,
                parentAdmitted: true,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: eax,
                ebxValue: ebx,
                esiValue: esi,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: LockedXaddRawFileOffset,
                stopBeforeRawFileOffset: LockedXaddRawFileOffset);
        }

        uint result = unchecked(oldValue.Value + ebx);
        GameSocketParserStoredByteSelectorGateFlags flagsAfter =
            FlagsForAdd(oldValue.Value, ebx);
        instructions.Add(new(
            LockedXaddRawFileOffset,
            ToCodeVA(LockedXaddRawFileOffset),
            LockedXaddEncoding,
            true,
            null,
            eax,
            eax,
            eax,
            ebx,
            oldValue.Value,
            null,
            null,
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
                result,
                Attempted: true,
                Committed: true),
            GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured,
            GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured,
            GameSocketParserDiagnosticCleanupSecondXaddOutcome.BoundedBeforeDecAt34D8,
            parentAdmitted: true,
            childBoundedCompletionBoundaryReached: true,
            eaxValue: eax,
            ebxValue: oldValue.Value,
            esiValue: esi,
            flagsAtStop: flagsAfter,
            lastReachedRawFileOffset: LockedXaddRawFileOffset,
            stopBeforeRawFileOffset: ExclusiveStopRawFileOffset);
    }

    internal static GameSocketParserStoredByteSelectorGateFlags FlagsForAdd(
        uint left,
        uint right)
    {
        uint result = unchecked(left + right);
        bool evenParity = (BitOperations.PopCount((byte)result) & 1) == 0;
        bool auxiliaryCarry = ((left ^ right ^ result) & 0x10) != 0;
        bool overflow = (~(left ^ right) & (left ^ result) & 0x80000000) != 0;
        return new(
            Carry: (ulong)left + right > uint.MaxValue,
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
