using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCapture(
    GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCapture ParentCapture,
    uint? FreshFrameResetEBP,
    uint? FreshPopEBPStackDWORD,
    uint? FreshReturnTargetDWORDAtResetESPPlus4);

public enum GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshFrameResetEBPAt34F1,
    MissingFreshPopEBPStackDWORDAt34F3,
    MissingFreshReturnTargetDWORDAt34F4,
    LogicalRet8CompletedAt34F4
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters(
    uint? EBP,
    uint? ECX,
    uint? EDI,
    uint? ESI,
    uint? EBX);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnFrameReset(
    uint InstructionRawFileOffset,
    string Encoding,
    uint? EBPValue,
    uint? ESPBefore,
    uint? ESPAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter,
    bool FlagsWritten);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPop(
    uint InstructionRawFileOffset,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? EBPBefore,
    uint? Value,
    uint? EBPMovedTo,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter,
    bool FlagsWritten);

public readonly record struct GameSocketParserDiagnosticCleanupSecondBranchFrameReturnInstruction(
    uint InstructionRawFileOffset,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? TargetVirtualAddress,
    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters RegistersBefore,
    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters RegistersAfter,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    bool FlagsWritten,
    bool TargetExecuted,
    bool CallerContinuationIdentified);

public sealed class GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan(
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan parentPlan,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnFrameReset? frameReset,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? savedEBPRead,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPop? savedEBPPop,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? returnTargetRead,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnInstruction? returnInstruction,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState frameResetCaptureState,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState savedEBPCaptureState,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState returnTargetCaptureState,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome outcome,
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
        FrameReset = frameReset;
        SavedEBPRead = savedEBPRead;
        SavedEBPPop = savedEBPPop;
        ReturnTargetRead = returnTargetRead;
        ReturnInstruction = returnInstruction;
        FreshFrameResetEBPCaptureState = frameResetCaptureState;
        FreshPopEBPStackCaptureState = savedEBPCaptureState;
        FreshReturnTargetCaptureState = returnTargetCaptureState;
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
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan ParentPlan { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnFrameReset? FrameReset
        { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? SavedEBPRead { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPop? SavedEBPPop { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? ReturnTargetRead
        { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnInstruction?
        ReturnInstruction { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
        FreshFrameResetEBPCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
        FreshPopEBPStackCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
        FreshReturnTargetCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome Outcome { get; }
    public bool ParentContinuationReached { get; }
    public bool SuffixAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public uint? InitialESP { get; }
    public uint? ResultESP { get; }
    public uint? FreshFrameResetEBP => Capture.FreshFrameResetEBP;
    public uint? FreshPopEBPStackDWORD => Capture.FreshPopEBPStackDWORD;
    public uint? FreshReturnTargetDWORDAtResetESPPlus4 =>
        Capture.FreshReturnTargetDWORDAtResetESPPlus4;
    public uint? FinalESPFromResetEBP =>
        Capture.FreshFrameResetEBP.HasValue
            ? unchecked(Capture.FreshFrameResetEBP.Value
                + GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .TotalStackAdvanceFromResetEBP)
            : null;
    public uint? EBPAfterPop =>
        SavedEBPPop is { Committed: true } pop ? pop.EBPMovedTo : null;
    public uint? ReturnTargetVirtualAddress =>
        ReturnInstruction is { Committed: true } instruction
            ? instruction.TargetVirtualAddress
            : null;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                StopBeforeRawFileOffset.Value)
            : null;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters?
        RestoredRegisters =>
            ReturnInstruction is { } instruction ? instruction.RegistersAfter : null;
    public bool ESPResetFromFreshEBP =>
        FrameReset is { Committed: true }
        && FrameReset.Value.ESPAfter == Capture.FreshFrameResetEBP;
    public bool ESPAdvancedForCommittedEffects =>
        ResultESP == ExpectedESPAfterLastCommittedEffect();
    public bool DECFlagsPreserved =>
        (ParentPlan.DECFlagsPreserved
            || ParentPlan.ParentPlan.ParentPlan.FlagsPropagated)
        && ParentPlan.ParentPlan.FlagsAtStop
            == ParentPlan.ParentPlan.ParentPlan.FlagsAtStop
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && (!FrameReset.HasValue
            || FrameReset.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && FrameReset.Value.FlagsAfter == ParentPlan.FlagsAtStop
                && !FrameReset.Value.FlagsWritten)
        && (!SavedEBPPop.HasValue
            || SavedEBPPop.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && SavedEBPPop.Value.FlagsAfter == ParentPlan.FlagsAtStop
                && !SavedEBPPop.Value.FlagsWritten)
        && (!ReturnInstruction.HasValue
            || ReturnInstruction.Value.FlagsBefore == ParentPlan.FlagsAtStop
                && ReturnInstruction.Value.FlagsAfter == ParentPlan.FlagsAtStop
                && !ReturnInstruction.Value.FlagsWritten);
    public GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters?
        RegistersBeforeRet =>
            SavedEBPPop is { } pop && pop.Committed
                ? RegistersAfterSavedEBPPop()
                : null;
    public bool RegistersPreservedByRet =>
        ReturnInstruction is { } instruction
        && RegistersBeforeRet.HasValue
        && instruction.RegistersBefore == RegistersBeforeRet.Value
        && instruction.RegistersAfter == instruction.RegistersBefore;
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
                && FrameReset is null
                && SavedEBPRead is null
                && SavedEBPPop is null
                && ReturnTargetRead is null
                && ReturnInstruction is null
                && !ChildBoundedCompletionBoundaryReached;
        }

        if (FrameReset is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .MoveEspEbpRawFileOffset,
                Encoding: GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .MoveEspEbpEncoding,
                Attempted: true,
                Reached: true,
                FlagsWritten: false
            }
            || FrameReset.Value.ESPBefore != InitialESP
            || FrameReset.Value.EBPValue != Capture.FreshFrameResetEBP
            || FrameReset.Value.Committed != Capture.FreshFrameResetEBP.HasValue
            || FrameReset.Value.FlagsBefore != ParentPlan.FlagsAtStop
            || FrameReset.Value.FlagsAfter != FrameReset.Value.FlagsBefore)
        {
            return false;
        }

        if (!Capture.FreshFrameResetEBP.HasValue)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                        .MissingFreshFrameResetEBPAt34F1
                && FrameReset.Value.ESPAfter == InitialESP
                && SavedEBPRead is null
                && SavedEBPPop is null
                && ReturnTargetRead is null
                && ReturnInstruction is null
                && ResultESP == InitialESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .MoveEspEbpRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .MoveEspEbpRawFileOffset
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint resetESP = Capture.FreshFrameResetEBP.Value;
        if (FrameReset.Value.ESPAfter != resetESP
            || SavedEBPRead is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .PopEBPRawFileOffset,
                Address: var savedEBPAddress,
                Width: GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .ReadWidth,
                Value: var savedEBPValue,
                Captured: var savedEBPCaptured,
                Attempted: true,
                NativeReadExecuted: false
            }
            || savedEBPAddress != resetESP
            || savedEBPValue != Capture.FreshPopEBPStackDWORD
            || savedEBPCaptured != Capture.FreshPopEBPStackDWORD.HasValue)
        {
            return false;
        }

        if (SavedEBPPop is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .PopEBPRawFileOffset,
                Encoding: GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .PopEBPEncoding,
                StackPointerBefore: var savedPopESPBefore,
                StackPointerAfter: var savedPopESPAfter,
                EBPBefore: var savedPopEBPBefore,
                Value: var savedPopValue,
                EBPMovedTo: var movedEBP,
                Attempted: true,
                Reached: true,
                FlagsWritten: false
            }
            || savedPopESPBefore != resetESP
            || savedPopEBPBefore != Capture.FreshFrameResetEBP
            || savedPopValue != Capture.FreshPopEBPStackDWORD
            || movedEBP != Capture.FreshPopEBPStackDWORD
            || SavedEBPPop.Value.Committed != Capture.FreshPopEBPStackDWORD.HasValue
            || SavedEBPPop.Value.FlagsBefore != FrameReset.Value.FlagsAfter
            || SavedEBPPop.Value.FlagsAfter != SavedEBPPop.Value.FlagsBefore)
        {
            return false;
        }

        if (!Capture.FreshPopEBPStackDWORD.HasValue)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                        .MissingFreshPopEBPStackDWORDAt34F3
                && savedPopESPAfter == resetESP
                && ReturnTargetRead is null
                && ReturnInstruction is null
                && ResultESP == resetESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .PopEBPRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .PopEBPRawFileOffset
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint retESP = unchecked(resetESP
            + GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection.DwordWidth);
        if (savedPopESPAfter != retESP
            || ReturnTargetRead is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .Ret8RawFileOffset,
                Address: var targetAddress,
                Width: GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .ReadWidth,
                Value: var targetValue,
                Captured: var targetCaptured,
                Attempted: true,
                NativeReadExecuted: false
            }
            || targetAddress != retESP
            || targetValue != Capture.FreshReturnTargetDWORDAtResetESPPlus4
            || targetCaptured != Capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue
            || ReturnInstruction is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .Ret8RawFileOffset,
                Encoding:
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .Ret8Encoding,
                StackPointerBefore: var returnESPBefore,
                Attempted: true,
                Reached: true,
                FlagsWritten: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false
            }
            || returnESPBefore != retESP
            || ReturnInstruction.Value.FlagsBefore != ParentPlan.FlagsAtStop
            || ReturnInstruction.Value.FlagsAfter != ReturnInstruction.Value.FlagsBefore
            || !RegistersBeforeRet.HasValue
            || ReturnInstruction.Value.RegistersBefore != RegistersBeforeRet.Value
            || ReturnInstruction.Value.RegistersAfter != ReturnInstruction.Value.RegistersBefore)
        {
            return false;
        }

        if (!Capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                        .MissingFreshReturnTargetDWORDAt34F4
                && FreshReturnTargetCaptureState
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                        .Missing
                && !ReturnInstruction.Value.Committed
                && ReturnInstruction.Value.TargetVirtualAddress is null
                && ReturnInstruction.Value.StackPointerAfter == retESP
                && ResultESP == retESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .Ret8RawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                        .Ret8RawFileOffset
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint resultESP = unchecked(retESP
            + GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .Ret8StackAdvance);
        return Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                    .LogicalRet8CompletedAt34F4
            && FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .Captured
            && ReturnInstruction.Value.Committed
            && ReturnInstruction.Value.TargetVirtualAddress
                == Capture.FreshReturnTargetDWORDAtResetESPPlus4
            && ReturnInstruction.Value.StackPointerAfter == resultESP
            && resultESP == FinalESPFromResetEBP
            && ResultESP == resultESP
            && LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .Ret8RawFileOffset
            && StopBeforeRawFileOffset is null
            && ChildBoundedCompletionBoundaryReached;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                .LogicalRet8CompletedAt34F4
        && ExactBoundedCompletion
        && FreshFrameResetEBPCaptureState
            == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Captured
        && FreshPopEBPStackCaptureState
            == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Captured
        && FreshReturnTargetCaptureState
            == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Captured
        && ESPResetFromFreshEBP
        && ESPAdvancedForCommittedEffects
        && EBPAfterPop == Capture.FreshPopEBPStackDWORD
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && DECFlagsPreserved
        && RegistersPreservedByRet
        && ReturnTargetVirtualAddress == Capture.FreshReturnTargetDWORDAtResetESPPlus4
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
        && (FrameReset is null
            || FrameReset.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .MoveEspEbpRawFileOffset)
        && (SavedEBPRead is null
            || SavedEBPRead.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .PopEBPRawFileOffset)
        && (SavedEBPPop is null
            || SavedEBPPop.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .PopEBPRawFileOffset)
        && (ReturnTargetRead is null
            || ReturnTargetRead.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .Ret8RawFileOffset)
        && (ReturnInstruction is null
            || ReturnInstruction.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                    .Ret8RawFileOffset)
        && (!SuffixAdmitted
            || InitialESP == ParentPlan.ResultESP
                && FrameReset is { Attempted: true, Reached: true });

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentContinuationReached == ParentPlan.ParentContinuationReached
            && reprojected.SuffixAdmitted == ParentPlan.SuffixAdmitted
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.InitialESP == ParentPlan.InitialESP
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.ECXAfterPop == ParentPlan.ECXAfterPop
            && reprojected.EDIAfterPop == ParentPlan.EDIAfterPop
            && reprojected.ESIAfterPop == ParentPlan.ESIAfterPop
            && reprojected.EBXAfterPop == ParentPlan.EBXAfterPop
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.DECFlagsPreserved == ParentPlan.DECFlagsPreserved
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.Pops.SequenceEqual(ParentPlan.Pops)
            && reprojected.ParentPlan.Outcome == ParentPlan.ParentPlan.Outcome
            && reprojected.ParentPlan.ExactBoundedCompletion
                == ParentPlan.ParentPlan.ExactBoundedCompletion
            && reprojected.ParentPlan.ExactCleanCompletion
                == ParentPlan.ParentPlan.ExactCleanCompletion
            && reprojected.ParentPlan.FlagsAtStop == ParentPlan.ParentPlan.FlagsAtStop
            && reprojected.ParentPlan.LastReachedRawFileOffset
                == ParentPlan.ParentPlan.LastReachedRawFileOffset
            && reprojected.ParentPlan.StopBeforeRawFileOffset
                == ParentPlan.ParentPlan.StopBeforeRawFileOffset
            && reprojected.ParentPlan.Reads.SequenceEqual(ParentPlan.ParentPlan.Reads)
            && reprojected.ParentPlan.MemoryEffects.SequenceEqual(
                ParentPlan.ParentPlan.MemoryEffects)
            && reprojected.ParentPlan.ParentPlan.Outcome
                == ParentPlan.ParentPlan.ParentPlan.Outcome
            && reprojected.ParentPlan.ParentPlan.FlagsAtStop
                == ParentPlan.ParentPlan.ParentPlan.FlagsAtStop
            && reprojected.ParentPlan.ParentPlan.Instructions.SequenceEqual(
                ParentPlan.ParentPlan.ParentPlan.Instructions)
            && reprojected.ContinuationBoundary == ParentPlan.ContinuationBoundary;
    }

    private GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters
        RegistersAfterSavedEBPPop() =>
            new(
                Capture.FreshPopEBPStackDWORD,
                ParentPlan.ECXAfterPop,
                ParentPlan.EDIAfterPop,
                ParentPlan.ESIAfterPop,
                ParentPlan.EBXAfterPop);

    private uint? ExpectedESPAfterLastCommittedEffect()
    {
        if (ReturnInstruction is { Committed: true } completedReturn)
        {
            return completedReturn.StackPointerAfter;
        }

        if (SavedEBPPop is { Committed: true } committedPop)
        {
            return committedPop.StackPointerAfter;
        }

        if (FrameReset is { Committed: true } committedReset)
        {
            return committedReset.ESPAfter;
        }

        return InitialESP;
    }
}

/// <summary>
/// Projects the taken second-branch frame reset and RETN 8 effects without executing the target.
/// </summary>
public static class GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
{
    public const uint MoveEspEbpRawFileOffset = 0x00BD34F1;
    public const uint PopEBPRawFileOffset = 0x00BD34F3;
    public const uint Ret8RawFileOffset = 0x00BD34F4;
    public const uint DwordWidth = 4;
    public const byte ReadWidth = 4;
    public const uint Ret8StackAdvance = 12;
    public const uint TotalStackAdvanceFromResetEBP = 16;
    public const string MoveEspEbpEncoding = "8B E5";
    public const string PopEBPEncoding = "5D";
    public const string Ret8Encoding = "C2 08 00";
    public const string CoordinatorProvidedBytes = "8B E5 5D C2 08 00";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesReturnTarget => false;
    public static bool IdentifiesCallerContinuation => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00);

    public static GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan Project(
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted register-pops plan must match the captured parent.");

        bool parentContinuationReached =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentContinuationReached
            && parentPlan.StopBeforeRawFileOffset == MoveEspEbpRawFileOffset;
        bool suffixAdmitted =
            parentContinuationReached
            && parentPlan.ExactBoundedCompletion
            && parentPlan.DECFlagsPreserved;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnFrameReset? frameReset = null;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? savedEBPRead = null;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPop? savedEBPPop = null;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRead? returnTargetRead = null;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnInstruction? ret = null;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState frameResetState;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState savedEBPState;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState returnTargetState;
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome outcome =
            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                .ParentContinuationNotAdmitted;
        uint? initialESP = suffixAdmitted ? parentPlan.ResultESP : null;
        uint? resultESP = initialESP;
        uint? lastReached = parentPlan.LastReachedRawFileOffset;
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        bool childComplete = false;

        if (!suffixAdmitted)
        {
            frameResetState = BeforeAdmission(capture.FreshFrameResetEBP.HasValue);
            savedEBPState = BeforeAdmission(capture.FreshPopEBPStackDWORD.HasValue);
            returnTargetState =
                BeforeAdmission(capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue);
        }
        else
        {
            frameResetState = capture.FreshFrameResetEBP.HasValue
                ? GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Missing;
            frameReset = new(
                MoveEspEbpRawFileOffset,
                MoveEspEbpEncoding,
                capture.FreshFrameResetEBP,
                initialESP,
                capture.FreshFrameResetEBP ?? initialESP,
                Attempted: true,
                Reached: true,
                Committed: capture.FreshFrameResetEBP.HasValue,
                FlagsBefore: flagsAtStop,
                FlagsAfter: flagsAtStop,
                FlagsWritten: false);
            lastReached = MoveEspEbpRawFileOffset;
            stopBefore = MoveEspEbpRawFileOffset;

            if (!capture.FreshFrameResetEBP.HasValue)
            {
                savedEBPState = AfterFailedRead(capture.FreshPopEBPStackDWORD.HasValue);
                returnTargetState = AfterFailedRead(
                    capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue);
                outcome =
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                        .MissingFreshFrameResetEBPAt34F1;
            }
            else
            {
                uint resetESP = capture.FreshFrameResetEBP.Value;
                resultESP = resetESP;
                savedEBPState = capture.FreshPopEBPStackDWORD.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                        .Captured
                    : GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                        .Missing;
                savedEBPRead = new(
                    PopEBPRawFileOffset,
                    resetESP,
                    ReadWidth,
                    capture.FreshPopEBPStackDWORD,
                    Attempted: true,
                    Captured: capture.FreshPopEBPStackDWORD.HasValue,
                    NativeReadExecuted: false);
                savedEBPPop = new(
                    PopEBPRawFileOffset,
                    PopEBPEncoding,
                    resetESP,
                    capture.FreshPopEBPStackDWORD.HasValue
                        ? unchecked(resetESP + DwordWidth)
                        : resetESP,
                    resetESP,
                    capture.FreshPopEBPStackDWORD,
                    capture.FreshPopEBPStackDWORD,
                    Attempted: true,
                    Reached: true,
                    Committed: capture.FreshPopEBPStackDWORD.HasValue,
                    FlagsBefore: flagsAtStop,
                    FlagsAfter: flagsAtStop,
                    FlagsWritten: false);
                lastReached = PopEBPRawFileOffset;
                stopBefore = PopEBPRawFileOffset;

                if (!capture.FreshPopEBPStackDWORD.HasValue)
                {
                    returnTargetState = AfterFailedRead(
                        capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue);
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                            .MissingFreshPopEBPStackDWORDAt34F3;
                }
                else
                {
                    uint retESP = unchecked(resetESP + DwordWidth);
                    resultESP = retESP;
                    returnTargetState =
                        capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue
                            ? GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                                .Captured
                            : GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                                .Missing;
                    returnTargetRead = new(
                        Ret8RawFileOffset,
                        retESP,
                        ReadWidth,
                        capture.FreshReturnTargetDWORDAtResetESPPlus4,
                        Attempted: true,
                        Captured: capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue,
                        NativeReadExecuted: false);
                    lastReached = Ret8RawFileOffset;
                    stopBefore = Ret8RawFileOffset;
                    GameSocketParserDiagnosticCleanupSecondBranchFrameReturnRegisters registers =
                        new(
                            capture.FreshPopEBPStackDWORD,
                            parentPlan.ECXAfterPop,
                            parentPlan.EDIAfterPop,
                            parentPlan.ESIAfterPop,
                            parentPlan.EBXAfterPop);
                    ret = new(
                        Ret8RawFileOffset,
                        Ret8Encoding,
                        retESP,
                        capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue
                            ? unchecked(retESP + Ret8StackAdvance)
                            : retESP,
                        capture.FreshReturnTargetDWORDAtResetESPPlus4,
                        registers,
                        registers,
                        flagsAtStop,
                        flagsAtStop,
                        Attempted: true,
                        Reached: true,
                        Committed: capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue,
                        FlagsWritten: false,
                        TargetExecuted: false,
                        CallerContinuationIdentified: false);

                    if (capture.FreshReturnTargetDWORDAtResetESPPlus4.HasValue)
                    {
                        resultESP = unchecked(retESP + Ret8StackAdvance);
                        stopBefore = null;
                        childComplete = true;
                        outcome =
                            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                                .LogicalRet8CompletedAt34F4;
                    }
                    else
                    {
                        outcome =
                            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                                .MissingFreshReturnTargetDWORDAt34F4;
                    }
                }
            }
        }

        return new(
            capture,
            parentPlan,
            frameReset,
            savedEBPRead,
            savedEBPPop,
            returnTargetRead,
            ret,
            frameResetState,
            savedEBPState,
            returnTargetState,
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

    private static GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
        AfterFailedRead(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.NotReached;
}
