using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallReturnCapture(
    GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture ParentCapture,
    uint? FreshReturnTargetDWORDAtCurrentESP);

public enum GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
{
    ParentContinuationNotAdmitted,
    ParentNotCleanAt34F4,
    MissingFreshReturnTargetDWORDAt34F4,
    LogicalRet8CompletedAt34F4
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallReturnRegisters(
    uint? EBP,
    uint? ECX,
    uint? EDI,
    uint? ESI,
    uint? EBX);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallReturnRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallReturnInstruction(
    uint InstructionRawFileOffset,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? TargetVirtualAddress,
    GameSocketParserDiagnosticCleanupSecondCallReturnRegisters RegistersBefore,
    GameSocketParserDiagnosticCleanupSecondCallReturnRegisters RegistersAfter,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool FlagsWritten,
    bool TargetExecuted,
    bool CallerContinuationIdentified);

public sealed class GameSocketParserDiagnosticCleanupSecondCallReturnPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondCallReturnPlan(
        GameSocketParserDiagnosticCleanupSecondCallReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan parentPlan,
        GameSocketParserDiagnosticCleanupSecondCallReturnRead? targetRead,
        GameSocketParserDiagnosticCleanupSecondCallReturnInstruction? returnInstruction,
        GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState targetCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallReturnOutcome outcome,
        bool parentContinuationReached,
        bool suffixAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? initialESP,
        uint? resultESP,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        TargetRead = targetRead;
        ReturnInstruction = returnInstruction;
        TargetCaptureState = targetCaptureState;
        Outcome = outcome;
        ParentContinuationReached = parentContinuationReached;
        SuffixAdmitted = suffixAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        InitialESP = initialESP;
        ResultESP = resultESP;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondCallReturnCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan ParentPlan { get; }
    public GameSocketParserDiagnosticCleanupSecondCallReturnRead? TargetRead { get; }
    public GameSocketParserDiagnosticCleanupSecondCallReturnInstruction? ReturnInstruction { get; }
    public GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState TargetCaptureState
        { get; }
    public GameSocketParserDiagnosticCleanupSecondCallReturnOutcome Outcome { get; }
    public bool ParentContinuationReached { get; }
    public bool SuffixAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public uint? InitialESP { get; }
    public uint? ResultESP { get; }
    public uint? FreshReturnTargetDWORD => Capture.FreshReturnTargetDWORDAtCurrentESP;
    public uint? ReturnTargetVirtualAddress =>
        ReturnInstruction is { Committed: true } instruction
            ? instruction.TargetVirtualAddress
            : null;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public GameSocketParserDiagnosticCleanupSecondCallReturnRegisters? RestoredRegisters =>
        ReturnInstruction is { } instruction ? instruction.RegistersAfter : null;
    public bool FlagsPreserved =>
        ReturnInstruction is { } instruction
            ? instruction.FlagsBefore == instruction.FlagsAfter && !instruction.FlagsWritten
            : FlagsAtStop == ParentPlan.FlagsAtStop;
    public bool RegistersPreserved =>
        ReturnInstruction is { } instruction
            && instruction.RegistersBefore == instruction.RegistersAfter
            && instruction.RegistersAfter == ParentRegisters(ParentPlan);
    public bool NativeInstructionsExecuted => false;
    public bool NativeReadsExecuted => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool ReturnTargetExecuted => false;
    public bool CallerContinuationIdentified => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;

    private bool ComputeExactBoundedCompletion()
    {
        if (!SuffixAdmitted)
        {
            return ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
                && LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
                && TargetRead is null
                && ReturnInstruction is null
                && !ChildBoundedCompletionBoundaryReached;
        }

        if (TargetRead is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset,
                Address: var address,
                Width: GameSocketParserDiagnosticCleanupSecondCallReturnProjection.ReadWidth,
                Value: var value,
                Attempted: true,
                NativeReadExecuted: false
            }
            || address != InitialESP
            || value != Capture.FreshReturnTargetDWORDAtCurrentESP
            || TargetRead.Value.Captured != Capture.FreshReturnTargetDWORDAtCurrentESP.HasValue
            || ReturnInstruction is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset,
                Encoding: GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8Encoding,
                StackPointerBefore: var before,
                Attempted: true,
                Reached: true,
                FlagsWritten: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false
            }
            || before != InitialESP
            || ReturnInstruction.Value.FlagsBefore != ParentPlan.FlagsAtStop
            || ReturnInstruction.Value.FlagsAfter != ReturnInstruction.Value.FlagsBefore
            || ReturnInstruction.Value.RegistersBefore != ParentRegisters(ParentPlan)
            || ReturnInstruction.Value.RegistersAfter != ReturnInstruction.Value.RegistersBefore)
        {
            return false;
        }

        if (!Capture.FreshReturnTargetDWORDAtCurrentESP.HasValue)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                        .MissingFreshReturnTargetDWORDAt34F4
                && TargetCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Missing
                && !ReturnInstruction.Value.Committed
                && ReturnInstruction.Value.TargetVirtualAddress is null
                && ReturnInstruction.Value.StackPointerAfter == InitialESP
                && ResultESP == InitialESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint targetESP = unchecked(InitialESP!.Value
            + GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8StackAdvance);
        return Outcome
                == GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                    .LogicalRet8CompletedAt34F4
            && TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Captured
            && ReturnInstruction.Value.Committed
            && ReturnInstruction.Value.TargetVirtualAddress
                == Capture.FreshReturnTargetDWORDAtCurrentESP
            && ReturnInstruction.Value.StackPointerAfter == targetESP
            && TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Captured
            && ResultESP == targetESP
            && ChildBoundedCompletionBoundaryReached
            && LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset
            && StopBeforeRawFileOffset is null;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                .LogicalRet8CompletedAt34F4
        && ExactBoundedCompletion
        && TargetCaptureState
            == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Captured
        && TargetRead is {
            Address: not null,
            Value: not null,
            Attempted: true,
            Captured: true,
            NativeReadExecuted: false
        }
        && TargetRead.Value.Address == InitialESP
        && TargetRead.Value.Value == Capture.FreshReturnTargetDWORDAtCurrentESP
        && ReturnInstruction is { Committed: true, TargetExecuted: false }
        && ReturnTargetVirtualAddress == Capture.FreshReturnTargetDWORDAtCurrentESP
        && ResultESP == unchecked(InitialESP!.Value
            + GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8StackAdvance)
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && FlagsPreserved
        && RegistersPreserved
        && ChildBoundedCompletionBoundaryReached
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadsExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !ReturnTargetExecuted
        && !CallerContinuationIdentified
        && !NativeABIOrProtocolSemanticsModeled;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && (TargetRead is null
            || TargetRead.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset)
        && (ReturnInstruction is null
            || ReturnInstruction.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset)
        && (!SuffixAdmitted
            || InitialESP == ParentPlan.ResultESP
                && ParentPlan.StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Ret8RawFileOffset);

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentContinuationReached == ParentPlan.ParentContinuationReached
            && reprojected.SuffixAdmitted == ParentPlan.SuffixAdmitted
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.InitialESP == ParentPlan.InitialESP
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.EBPAfterPop == ParentPlan.EBPAfterPop
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.FreshFrameResetEBPCaptureState
                == ParentPlan.FreshFrameResetEBPCaptureState
            && reprojected.FreshPopEBPStackCaptureState
                == ParentPlan.FreshPopEBPStackCaptureState
            && reprojected.FrameReset == ParentPlan.FrameReset
            && reprojected.StackRead == ParentPlan.StackRead
            && reprojected.Pop == ParentPlan.Pop
            && reprojected.ContinuationBoundary == ParentPlan.ContinuationBoundary
            && reprojected.ParentPlan.Outcome == ParentPlan.ParentPlan.Outcome
            && reprojected.ParentPlan.ExactBoundedCompletion
                == ParentPlan.ParentPlan.ExactBoundedCompletion
            && reprojected.ParentPlan.ExactCleanCompletion
                == ParentPlan.ParentPlan.ExactCleanCompletion
            && reprojected.ParentPlan.ResultESP == ParentPlan.ParentPlan.ResultESP
            && reprojected.ParentPlan.ECXAfterPop == ParentPlan.ParentPlan.ECXAfterPop
            && reprojected.ParentPlan.EDIAfterPop == ParentPlan.ParentPlan.EDIAfterPop
            && reprojected.ParentPlan.ESIAfterPop == ParentPlan.ParentPlan.ESIAfterPop
            && reprojected.ParentPlan.EBXAfterPop == ParentPlan.ParentPlan.EBXAfterPop
            && reprojected.ParentPlan.FlagsAtStop == ParentPlan.ParentPlan.FlagsAtStop
            && reprojected.ParentPlan.LastReachedRawFileOffset
                == ParentPlan.ParentPlan.LastReachedRawFileOffset
            && reprojected.ParentPlan.StopBeforeRawFileOffset
                == ParentPlan.ParentPlan.StopBeforeRawFileOffset
            && reprojected.ParentPlan.Reads.SequenceEqual(ParentPlan.ParentPlan.Reads)
            && reprojected.ParentPlan.Pops.SequenceEqual(ParentPlan.ParentPlan.Pops)
            && reprojected.ParentPlan.ParentPlan.Outcome
                == ParentPlan.ParentPlan.ParentPlan.Outcome
            && reprojected.ParentPlan.ParentPlan.MemoryEffects.SequenceEqual(
                ParentPlan.ParentPlan.ParentPlan.MemoryEffects);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallReturnRegisters
        ParentRegisters(GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan parentPlan) =>
        new(
            parentPlan.EBPAfterPop,
            parentPlan.ParentPlan.ECXAfterPop,
            parentPlan.ParentPlan.EDIAfterPop,
            parentPlan.ParentPlan.ESIAfterPop,
            parentPlan.ParentPlan.EBXAfterPop);
}

/// <summary>Projects the RETN 8 stack effect and records, but never executes, its target.</summary>
public static class GameSocketParserDiagnosticCleanupSecondCallReturnProjection
{
    public const uint Ret8RawFileOffset = 0x00BD34F4;
    public const uint DwordWidth = 4;
    public const uint ArgumentCleanupBytes = 8;
    public const uint Ret8StackAdvance = DwordWidth + ArgumentCleanupBytes;
    public const byte ReadWidth = 4;
    public const string Ret8Encoding = "C2 08 00";
    public const string CoordinatorProvidedBytes = "C2 08 00";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesReturnTarget => false;
    public static bool IdentifiesCallerContinuation => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(0xC2, 0x08, 0x00);

    public static GameSocketParserDiagnosticCleanupSecondCallReturnPlan Project(
        GameSocketParserDiagnosticCleanupSecondCallReturnCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondCallReturnPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondCallReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted frame-reset plan must match the captured parent.");

        bool parentAtRet8 =
            parentPlan.ExactBoundedCompletion
            && parentPlan.LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .PopEBPRawFileOffset
            && parentPlan.StopBeforeRawFileOffset == Ret8RawFileOffset
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .CompletedBeforeRet8At34F4;
        bool parentContinuationReached =
            parentAtRet8 && parentPlan.ParentContinuationReached;
        bool suffixAdmitted =
            parentContinuationReached
            && parentPlan.ExactCleanCompletion
            && parentPlan.ResultESP.HasValue;
        GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState targetState =
            BeforeAdmission(capture.FreshReturnTargetDWORDAtCurrentESP.HasValue);
        GameSocketParserDiagnosticCleanupSecondCallReturnRead? targetRead = null;
        GameSocketParserDiagnosticCleanupSecondCallReturnInstruction? ret = null;
        GameSocketParserDiagnosticCleanupSecondCallReturnOutcome outcome =
            parentAtRet8
                ? GameSocketParserDiagnosticCleanupSecondCallReturnOutcome.ParentNotCleanAt34F4
                : GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                    .ParentContinuationNotAdmitted;
        uint? initialESP = suffixAdmitted ? parentPlan.ResultESP : null;
        uint? resultESP = initialESP;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        uint? lastReached = parentPlan.LastReachedRawFileOffset;
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        bool childComplete = false;

        if (suffixAdmitted)
        {
            targetState = capture.FreshReturnTargetDWORDAtCurrentESP.HasValue
                ? GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Missing;
            bool targetCaptured = capture.FreshReturnTargetDWORDAtCurrentESP.HasValue;
            targetRead = new(
                Ret8RawFileOffset,
                initialESP,
                ReadWidth,
                capture.FreshReturnTargetDWORDAtCurrentESP,
                Attempted: true,
                Captured: targetCaptured,
                NativeReadExecuted: false);
            GameSocketParserDiagnosticCleanupSecondCallReturnRegisters registers =
                ParentRegisters(parentPlan);
            uint? stackAfter = targetCaptured
                ? unchecked(initialESP!.Value + Ret8StackAdvance)
                : initialESP;
            ret = new(
                Ret8RawFileOffset,
                Ret8Encoding,
                initialESP,
                stackAfter,
                capture.FreshReturnTargetDWORDAtCurrentESP,
                registers,
                registers,
                flagsAtStop,
                flagsAtStop,
                Attempted: true,
                Reached: true,
                Committed: targetCaptured,
                FlagsWritten: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false);
            lastReached = Ret8RawFileOffset;
            stopBefore = targetCaptured ? null : Ret8RawFileOffset;

            if (targetCaptured)
            {
                resultESP = stackAfter;
                childComplete = true;
                outcome =
                    GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                        .LogicalRet8CompletedAt34F4;
            }
            else
            {
                outcome =
                    GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                        .MissingFreshReturnTargetDWORDAt34F4;
            }
        }
        else if (parentAtRet8)
        {
            targetState = BeforeAdmission(capture.FreshReturnTargetDWORDAtCurrentESP.HasValue);
        }

        return new(
            capture,
            parentPlan,
            targetRead,
            ret,
            targetState,
            outcome,
            parentContinuationReached,
            suffixAdmitted,
            childComplete,
            initialESP,
            resultESP,
            flagsAtStop,
            lastReached,
            stopBefore);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.SuppliedButUnused
            : GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupSecondCallReturnRegisters ParentRegisters(
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan parentPlan) =>
        new(
            parentPlan.EBPAfterPop,
            parentPlan.ParentPlan.ECXAfterPop,
            parentPlan.ParentPlan.EDIAfterPop,
            parentPlan.ParentPlan.ESIAfterPop,
            parentPlan.ParentPlan.EBXAfterPop);
}
