using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture(
    GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture ParentCapture,
    uint? FreshPostCallESP,
    uint? FreshDWORDAtWrappedESPPlus48);

public enum GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
{
    ParentContinuationNotAdmitted,
    ParentNotCleanAt34E2,
    MissingFreshPostCallESPAt34E2,
    MissingFreshDWORDAt34E2,
    CompletedBeforePopAt34ED
}

public enum GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind
{
    ReadFreshDWORDAtESPPlus48,
    MoveECXFromESPPlus48,
    StoreECXAtFSSegmentOffsetZero
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreRegisterMove(
    uint InstructionRawFileOffset,
    string DestinationRegister,
    uint? SourceAddress,
    uint? Value,
    bool Attempted,
    bool Reached,
    bool Committed);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryWrite(
    uint InstructionRawFileOffset,
    string SegmentRegister,
    uint SegmentOffset,
    byte Width,
    uint Value,
    bool SegmentRelative,
    uint? LinearAddress,
    bool Attempted,
    bool Committed,
    bool NativeWriteExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFsRestoreStep(
    GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public sealed class GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan(
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture capture,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallFsRestoreStep> instructionOrder,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreRegisterMove? registerMove,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryWrite? memoryWrite,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreBoundary? continuationBoundary,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState espCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState dwordCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome outcome,
        bool parentContinuationReached,
        bool suffixAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? freshPostCallESP,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        InstructionOrder = instructionOrder;
        MemoryRead = memoryRead;
        RegisterMove = registerMove;
        MemoryWrite = memoryWrite;
        ContinuationBoundary = continuationBoundary;
        FreshPostCallESPCaptureState = espCaptureState;
        FreshDWORDCaptureState = dwordCaptureState;
        Outcome = outcome;
        ParentContinuationReached = parentContinuationReached;
        SuffixAdmitted = suffixAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        FreshPostCallESP = freshPostCallESP;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallFsRestoreStep> InstructionOrder
        { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreRegisterMove? RegisterMove { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryWrite? MemoryWrite { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreBoundary? ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
        FreshPostCallESPCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState FreshDWORDCaptureState
        { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome Outcome { get; }
    public bool ParentContinuationReached { get; }
    public bool SuffixAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool ExactBoundedCompletion =>
        SuffixAdmitted
            ? ChildBoundedCompletionBoundaryReached
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public uint? FreshPostCallESP { get; }
    public uint? FreshDWORDAtWrappedESPPlus48 =>
        MemoryRead is { Captured: true } ? MemoryRead.Value.Value : null;
    public uint? ECXAfterMove =>
        RegisterMove is { Committed: true } ? RegisterMove.Value.Value : null;
    public uint? EAXAfterCall => null;
    public uint? EBXAfterCall => null;
    public uint? ESIAfterCall => null;
    public uint? EBPAfterCall => null;
    public bool PostCallRegistersCaptured => false;
    public bool ESPUnchangedClaimed => false;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                StopBeforeRawFileOffset.Value)
            : null;

    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;

    public bool NativeInstructionsExecuted => false;
    public bool NativeReadExecuted => false;
    public bool NativeWriteExecuted => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool LinearFSAddressModeled => false;
    public bool PointerValidityModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome.CompletedBeforePopAt34ED
        && FreshPostCallESPCaptureState
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Captured
        && FreshDWORDCaptureState
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Captured
        && FreshPostCallESP == Capture.FreshPostCallESP
        && MemoryRead is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.MoveRawFileOffset,
            Address: not null,
            Width: GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.DwordWidth,
            Value: not null,
            Attempted: true,
            Captured: true,
            NativeReadExecuted: false
        }
        && MemoryRead.Value.Address == WrappedReadAddress(FreshPostCallESP)
        && MemoryRead.Value.Value == Capture.FreshDWORDAtWrappedESPPlus48
        && RegisterMove is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.MoveRawFileOffset,
            DestinationRegister: "ECX",
            SourceAddress: not null,
            Value: not null,
            Attempted: true,
            Reached: true,
            Committed: true
        }
        && RegisterMove.Value.SourceAddress == MemoryRead.Value.Address
        && RegisterMove.Value.Value == MemoryRead.Value.Value
        && MemoryWrite is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.FsStoreRawFileOffset,
            SegmentRegister: "FS",
            SegmentOffset: 0,
            Width: GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.DwordWidth,
            SegmentRelative: true,
            LinearAddress: null,
            Attempted: true,
            Committed: true,
            NativeWriteExecuted: false
        }
        && MemoryWrite.Value.Value == RegisterMove.Value.Value
        && InstructionOrder.Length == 3
        && InstructionOrder[0].Kind
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind.ReadFreshDWORDAtESPPlus48
        && InstructionOrder[1].Kind
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind.MoveECXFromESPPlus48
        && InstructionOrder[2].Kind
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind
                .StoreECXAtFSSegmentOffsetZero
        && MemoryWrite.Value.InstructionRawFileOffset
            > MemoryRead.Value.InstructionRawFileOffset
        && ContinuationBoundary is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                    .StopBeforePopRawFileOffset,
            NextInstructionEncoding:
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                    .NextInstructionEncoding,
            Reached: true,
            NextInstructionExecuted: false
        }
        && ChildBoundedCompletionBoundaryReached
        && LastReachedRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.FsStoreRawFileOffset
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .StopBeforePopRawFileOffset
        && FlagsAtStop is null
        && !PostCallRegistersCaptured
        && !ESPUnchangedClaimed
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadExecuted
        && !NativeWriteExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !LinearFSAddressModeled
        && !PointerValidityModeled
        && !NativeABIOrProtocolSemanticsModeled;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects)
        && (MemoryRead is null
            || MemoryRead.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                    .MoveRawFileOffset)
        && (MemoryWrite is null
            || MemoryRead is { Captured: true }
                && RegisterMove is { Committed: true }
                && MemoryWrite.Value.InstructionRawFileOffset
                    > RegisterMove.Value.InstructionRawFileOffset
                && MemoryWrite.Value.Value == RegisterMove.Value.Value);

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentCallBoundaryAdmitted == ParentPlan.ParentCallBoundaryAdmitted
            && reprojected.ChildBoundedCompletionBoundaryReached
                == ParentPlan.ChildBoundedCompletionBoundaryReached
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.TargetCaptureState == ParentPlan.TargetCaptureState
            && reprojected.ReturnCaptureState == ParentPlan.ReturnCaptureState
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.CallBoundary == ParentPlan.CallBoundary
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.InstructionOrder.SequenceEqual(ParentPlan.InstructionOrder)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    }

    private static uint? WrappedReadAddress(uint? esp) =>
        esp.HasValue
            ? unchecked(esp.Value
                + GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.StackOffset)
            : null;
}

/// <summary>Projects the fresh post-call stack load and FS:[0] write, stopping before POP ECX.</summary>
public static class GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
{
    public const uint MoveRawFileOffset = 0x00BD34E2;
    public const uint FsStoreRawFileOffset = 0x00BD34E6;
    public const uint StopBeforePopRawFileOffset = 0x00BD34ED;
    public const uint StackOffset = 0x48;
    public const uint FsSegmentOffset = 0;
    public const byte DwordWidth = 4;
    public const string MoveEncoding = "8B 4C 24 48";
    public const string FsStoreEncoding = "64 89 0D 00 00 00 00";
    public const string NextInstructionEncoding = "59";
    public const string CoordinatorProvidedBytes =
        "8B 4C 24 48 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D C2 08 00 CC";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesNativeWrites => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x4C, 0x24, 0x48, 0x64, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
            0x59, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00, 0xCC);

    public static GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan Project(
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture capture,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted second-call plan must match the captured parent.");

        bool parentContinuationReached =
            parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .OpaqueCallReturnedNormally
            && parentPlan.ExactBoundedCompletion
            && parentPlan.StopBeforeRawFileOffset == MoveRawFileOffset;
        bool suffixAdmitted =
            parentContinuationReached
            && parentPlan.ExactCleanCompletion
            && parentPlan.FlagsAtStop is null;

        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState espState =
            BeforeAdmission(capture.FreshPostCallESP.HasValue);
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState dwordState =
            BeforeAdmission(capture.FreshDWORDAtWrappedESPPlus48.HasValue);
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome outcome =
            !parentContinuationReached
                ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                    .ParentContinuationNotAdmitted
                : !suffixAdmitted
                    ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                        .ParentNotCleanAt34E2
                    : GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                        .MissingFreshPostCallESPAt34E2;

        var steps =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondCallFsRestoreStep>();
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryRead? memoryRead = null;
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreRegisterMove? registerMove = null;
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreMemoryWrite? memoryWrite = null;
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreBoundary? boundary = null;
        uint? freshESP = null;
        bool childComplete = false;
        uint? lastReached = ParentLastReached(parentPlan);
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;

        if (suffixAdmitted)
        {
            espState = capture.FreshPostCallESP.HasValue
                ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Missing;
            if (!capture.FreshPostCallESP.HasValue)
            {
                dwordState = capture.FreshDWORDAtWrappedESPPlus48.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
                        .SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.NotReached;
                outcome =
                    GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                        .MissingFreshPostCallESPAt34E2;
                stopBefore = MoveRawFileOffset;
            }
            else
            {
                freshESP = capture.FreshPostCallESP;
                uint address = unchecked(freshESP.Value + StackOffset);
                bool captured = capture.FreshDWORDAtWrappedESPPlus48.HasValue;
                dwordState = captured
                    ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Captured
                    : GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Missing;
                memoryRead = new(
                    MoveRawFileOffset,
                    address,
                    DwordWidth,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    Attempted: true,
                    Captured: captured,
                    NativeReadExecuted: false);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind
                        .ReadFreshDWORDAtESPPlus48,
                    MoveRawFileOffset,
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48));
                registerMove = new(
                    MoveRawFileOffset,
                    "ECX",
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    Attempted: true,
                    Reached: true,
                    Committed: captured);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind
                        .MoveECXFromESPPlus48,
                    MoveRawFileOffset,
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48));
                lastReached = MoveRawFileOffset;
                stopBefore = MoveRawFileOffset;

                if (!captured)
                {
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                            .MissingFreshDWORDAt34E2;
                }
                else
                {
                    memoryWrite = new(
                        FsStoreRawFileOffset,
                        "FS",
                        FsSegmentOffset,
                        DwordWidth,
                        capture.FreshDWORDAtWrappedESPPlus48!.Value,
                        SegmentRelative: true,
                        LinearAddress: null,
                        Attempted: true,
                        Committed: true,
                        NativeWriteExecuted: false);
                    steps.Add(new(
                        GameSocketParserDiagnosticCleanupSecondCallFsRestoreStepKind
                            .StoreECXAtFSSegmentOffsetZero,
                        FsStoreRawFileOffset,
                        FsSegmentOffset,
                        capture.FreshDWORDAtWrappedESPPlus48));
                    boundary = new(
                        StopBeforePopRawFileOffset,
                        NextInstructionEncoding,
                        Reached: true,
                        NextInstructionExecuted: false);
                    childComplete = true;
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                            .CompletedBeforePopAt34ED;
                    lastReached = FsStoreRawFileOffset;
                    stopBefore = StopBeforePopRawFileOffset;
                }
            }
        }

        return new(
            capture,
            parentPlan,
            steps.ToImmutable(),
            memoryRead,
            registerMove,
            memoryWrite,
            boundary,
            espState,
            dwordState,
            outcome,
            parentContinuationReached,
            suffixAdmitted,
            childComplete,
            freshESP,
            flagsAtStop,
            lastReached,
            stopBefore);
    }

    private static uint? ParentLastReached(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan parentPlan) =>
        parentPlan.LastReachedRawFileOffset;

    private static GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.NotReached;
}
