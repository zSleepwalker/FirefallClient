using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture(
    GameSocketParserDiagnosticCleanupSecondXaddBranchCapture ParentCapture,
    uint? FreshESPAt34E2,
    uint? FreshDWORDAtWrappedESPPlus48);

public enum GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
{
    ParentContinuationNotAdmitted,
    BranchNotTakenTo34DB,
    MissingFreshESPAt34E2,
    MissingFreshDWORDAt34E2,
    CompletedBeforePopAt34ED
}

public enum GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
{
    ReadFreshDWORDAtESPPlus48,
    MoveECXFromESPPlus48,
    StoreECXAtFSSegmentOffsetZero
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreRegisterMove(
    uint InstructionRawFileOffset,
    string DestinationRegister,
    uint? SourceAddress,
    uint? Value,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryWrite(
    uint InstructionRawFileOffset,
    string SegmentRegister,
    uint SegmentOffset,
    byte Width,
    uint Value,
    bool SegmentRelative,
    uint? LinearAddress,
    bool Attempted,
    bool Committed,
    bool NativeWriteExecuted,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStep(
    GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore = null,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter = null);

public sealed class GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondXaddBranchPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan(
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture capture,
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStep>
            instructionOrder,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreRegisterMove? registerMove,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryWrite? memoryWrite,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreBoundary? continuationBoundary,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState espCaptureState,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState dwordCaptureState,
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome outcome,
        bool parentContinuationReached,
        bool suffixAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? freshESP,
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
        FreshESPCaptureState = espCaptureState;
        FreshDWORDCaptureState = dwordCaptureState;
        Outcome = outcome;
        ParentContinuationReached = parentContinuationReached;
        SuffixAdmitted = suffixAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        FreshESPAt34E2 = freshESP;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                Capture.ParentCapture));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondXaddBranchPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStep>
        InstructionOrder { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreRegisterMove? RegisterMove
        { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryWrite? MemoryWrite
        { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreBoundary?
        ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
        FreshESPCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
        FreshDWORDCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome Outcome { get; }
    public bool ParentContinuationReached { get; }
    public bool SuffixAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool ExactBoundedCompletion =>
        SuffixAdmitted
            ? ChildBoundedCompletionBoundaryReached
                && Outcome
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                        .CompletedBeforePopAt34ED
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                        .FsStoreRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                        .StopBeforePopRawFileOffset
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public uint? FreshESPAt34E2 { get; }
    public uint? FreshDWORDAtWrappedESPPlus48 =>
        MemoryRead is { Captured: true } ? MemoryRead.Value.Value : null;
    public uint? ECXAfterMove =>
        RegisterMove is { Committed: true } ? RegisterMove.Value.Value : null;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public bool DECFlagsPreserved =>
        ParentContinuationReached
        && ParentPlan.FlagsAtStop.HasValue
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && (RegisterMove is null
            || RegisterMove.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && RegisterMove.Value.FlagsAfter == ParentPlan.FlagsAtStop)
        && (MemoryWrite is null
            || MemoryWrite.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && MemoryWrite.Value.FlagsAfter == ParentPlan.FlagsAtStop);
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
    public bool OpaqueCallAfterBranchObserved => false;
    public bool LinearFSAddressModeled => false;
    public bool PointerValidityModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                .CompletedBeforePopAt34ED
        && FreshESPCaptureState
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Captured
        && FreshDWORDCaptureState
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Captured
        && FreshESPAt34E2 == Capture.FreshESPAt34E2
        && MemoryRead is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.MoveRawFileOffset,
            Address: not null,
            Width: GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.DwordWidth,
            Value: not null,
            Attempted: true,
            Captured: true,
            NativeReadExecuted: false
        }
        && MemoryRead.Value.Address == WrappedReadAddress(FreshESPAt34E2)
        && MemoryRead.Value.Value == Capture.FreshDWORDAtWrappedESPPlus48
        && RegisterMove is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.MoveRawFileOffset,
            DestinationRegister: "ECX",
            SourceAddress: not null,
            Value: not null,
            Attempted: true,
            Reached: true,
            Committed: true
        }
        && RegisterMove.Value.SourceAddress == MemoryRead.Value.Address
        && RegisterMove.Value.Value == MemoryRead.Value.Value
        && RegisterMove.Value.FlagsBefore == ParentPlan.FlagsAtStop
        && RegisterMove.Value.FlagsAfter == ParentPlan.FlagsAtStop
        && MemoryWrite is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                    .FsStoreRawFileOffset,
            SegmentRegister: "FS",
            SegmentOffset: 0,
            Width: GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.DwordWidth,
            SegmentRelative: true,
            LinearAddress: null,
            Attempted: true,
            Committed: true,
            NativeWriteExecuted: false
        }
        && MemoryWrite.Value.Value == RegisterMove.Value.Value
        && MemoryWrite.Value.FlagsBefore == ParentPlan.FlagsAtStop
        && MemoryWrite.Value.FlagsAfter == ParentPlan.FlagsAtStop
        && InstructionOrder.Length == 3
        && InstructionOrder[0].Kind
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                .ReadFreshDWORDAtESPPlus48
        && InstructionOrder[1].Kind
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                .MoveECXFromESPPlus48
        && InstructionOrder[2].Kind
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                .StoreECXAtFSSegmentOffsetZero
        && InstructionOrder.All(step =>
            step.FlagsBefore == ParentPlan.FlagsAtStop
            && step.FlagsAfter == ParentPlan.FlagsAtStop)
        && MemoryWrite.Value.InstructionRawFileOffset
            > MemoryRead.Value.InstructionRawFileOffset
        && ContinuationBoundary is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                    .StopBeforePopRawFileOffset,
            NextInstructionEncoding:
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                    .NextInstructionEncoding,
            Reached: true,
            NextInstructionExecuted: false
        }
        && ChildBoundedCompletionBoundaryReached
        && ExactBoundedCompletion
        && LastReachedRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .FsStoreRawFileOffset
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .StopBeforePopRawFileOffset
        && DECFlagsPreserved
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadExecuted
        && !NativeWriteExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !OpaqueCallAfterBranchObserved
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
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                    .MoveRawFileOffset)
        && (MemoryWrite is null
            || MemoryRead is { Captured: true }
                && RegisterMove is { Committed: true }
                && MemoryWrite.Value.InstructionRawFileOffset
                    > RegisterMove.Value.InstructionRawFileOffset
                && MemoryWrite.Value.Value == RegisterMove.Value.Value
                && DECFlagsPreserved);

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

    private static uint? WrappedReadAddress(uint? esp) =>
        esp.HasValue
            ? unchecked(esp.Value
                + GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.StackOffset)
            : null;
}

/// <summary>Projects the taken second-XADD branch through FS:[0], stopping before POP ECX.</summary>
public static class GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
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
        "8B 4C 24 48 64 89 0D 00 00 00 00 59";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesNativeWrites => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x4C, 0x24, 0x48, 0x64, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
            0x59);

    public static GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan Project(
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture capture,
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted XADD-branch plan must match the captured parent.");

        bool parentContinuationReached =
            parentPlan.ExactCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && parentPlan.StopBeforeRawFileOffset == MoveRawFileOffset
            && HasKnownFlags(parentPlan.FlagsAtStop);
        bool branchNotTaken =
            parentPlan.ExactCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                    .JnzNotTakenTo34DB
            && parentPlan.StopBeforeRawFileOffset == 0x00BD34DB;

        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState espState =
            BeforeAdmission(capture.FreshESPAt34E2.HasValue);
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState dwordState =
            BeforeAdmission(capture.FreshDWORDAtWrappedESPPlus48.HasValue);
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome outcome =
            parentContinuationReached
                ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .MissingFreshESPAt34E2
                : branchNotTaken
                    ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                        .BranchNotTakenTo34DB
                    : GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                        .ParentContinuationNotAdmitted;

        var steps =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStep>();
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryRead? memoryRead = null;
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreRegisterMove? registerMove = null;
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreMemoryWrite? memoryWrite = null;
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreBoundary? boundary = null;
        uint? freshESP = null;
        bool suffixAdmitted = parentContinuationReached;
        bool childComplete = false;
        uint? lastReached = parentPlan.LastReachedRawFileOffset;
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;

        if (suffixAdmitted)
        {
            espState = capture.FreshESPAt34E2.HasValue
                ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Missing;
            if (!capture.FreshESPAt34E2.HasValue)
            {
                dwordState = capture.FreshDWORDAtWrappedESPPlus48.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                        .SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                        .NotReached;
                outcome =
                    GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                        .MissingFreshESPAt34E2;
                stopBefore = MoveRawFileOffset;
            }
            else
            {
                freshESP = capture.FreshESPAt34E2;
                uint address = unchecked(freshESP.Value + StackOffset);
                bool captured = capture.FreshDWORDAtWrappedESPPlus48.HasValue;
                dwordState = captured
                    ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Captured
                    : GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Missing;
                memoryRead = new(
                    MoveRawFileOffset,
                    address,
                    DwordWidth,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    Attempted: true,
                    Captured: captured,
                    NativeReadExecuted: false);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                        .ReadFreshDWORDAtESPPlus48,
                    MoveRawFileOffset,
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    flagsAtStop,
                    flagsAtStop));
                registerMove = new(
                    MoveRawFileOffset,
                    "ECX",
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    Attempted: true,
                    Reached: true,
                    Committed: captured,
                    FlagsBefore: flagsAtStop,
                    FlagsAfter: flagsAtStop);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                        .MoveECXFromESPPlus48,
                    MoveRawFileOffset,
                    address,
                    capture.FreshDWORDAtWrappedESPPlus48,
                    flagsAtStop,
                    flagsAtStop));
                lastReached = MoveRawFileOffset;

                if (!captured)
                {
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                            .MissingFreshDWORDAt34E2;
                    stopBefore = MoveRawFileOffset;
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
                        NativeWriteExecuted: false,
                        FlagsBefore: flagsAtStop,
                        FlagsAfter: flagsAtStop);
                    steps.Add(new(
                        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreStepKind
                            .StoreECXAtFSSegmentOffsetZero,
                        FsStoreRawFileOffset,
                        FsSegmentOffset,
                        capture.FreshDWORDAtWrappedESPPlus48,
                        flagsAtStop,
                        flagsAtStop));
                    boundary = new(
                        StopBeforePopRawFileOffset,
                        NextInstructionEncoding,
                        Reached: true,
                        NextInstructionExecuted: false);
                    childComplete = true;
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
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

    private static bool HasKnownFlags(GameSocketParserStoredByteSelectorGateFlags? flags) =>
        flags is {
            Carry: not null,
            Parity: not null,
            AuxiliaryCarry: not null,
            Zero: not null,
            Sign: not null,
            Overflow: not null
        };

    private static GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.NotReached;
}
