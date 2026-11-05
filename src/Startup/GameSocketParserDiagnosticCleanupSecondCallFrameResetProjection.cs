using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture(
    GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture ParentCapture,
    uint? FreshFrameResetEBP,
    uint? FreshPopEBPStackDWORD);

public enum GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshFrameResetEBPAt34F1,
    MissingFreshPopEBPStackDWORDAt34F3,
    CompletedBeforeRet8At34F4
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFrameResetMove(
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

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFrameResetRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFrameResetPop(
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

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallFrameResetBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public sealed class GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan(
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan parentPlan,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetMove? frameReset,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetRead? stackRead,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPop? pop,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetBoundary? continuationBoundary,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState frameResetCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState popStackCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome outcome,
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
        StackRead = stackRead;
        Pop = pop;
        ContinuationBoundary = continuationBoundary;
        FreshFrameResetEBPCaptureState = frameResetCaptureState;
        FreshPopEBPStackCaptureState = popStackCaptureState;
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
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan ParentPlan { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetMove? FrameReset { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetRead? StackRead { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetPop? Pop { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetBoundary?
        ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
        FreshFrameResetEBPCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
        FreshPopEBPStackCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome Outcome { get; }
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
    public uint? EBPAfterPop => Pop is { Committed: true } ? Pop.Value.EBPMovedTo : null;
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
    public bool ESPResetFromFreshEBP =>
        FrameReset is { Committed: true }
        && FrameReset.Value.ESPAfter == Capture.FreshFrameResetEBP;
    public bool ESPAdvancedForCommittedPop =>
        Pop is { Committed: true }
            ? Pop.Value.StackPointerAfter
                == unchecked(Pop.Value.StackPointerBefore!.Value
                    + GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.DwordWidth)
                && ResultESP == Pop.Value.StackPointerAfter
            : Pop is { Reached: true }
                ? Pop.Value.StackPointerAfter == Pop.Value.StackPointerBefore
                    && ResultESP == Pop.Value.StackPointerBefore
                : ResultESP == InitialESP
                    || FrameReset is { Committed: true }
                        && ResultESP == FrameReset.Value.ESPAfter;
    public bool FlagsPreserved =>
        (!FrameReset.HasValue
            || FrameReset.Value.FlagsBefore == FrameReset.Value.FlagsAfter
                && !FrameReset.Value.FlagsWritten)
        && (!Pop.HasValue
            || Pop.Value.FlagsBefore == Pop.Value.FlagsAfter
                && !Pop.Value.FlagsWritten);
    public bool NativeInstructionsExecuted => false;
    public bool NativeReadsExecuted => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;

    private bool ComputeExactBoundedCompletion()
    {
        if (!SuffixAdmitted)
        {
            return ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
                && LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
                && FrameReset is null
                && StackRead is null
                && Pop is null
                && ContinuationBoundary is null;
        }

        if (FrameReset is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .MoveEspEbpRawFileOffset,
                Encoding: GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
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

        if (!FrameReset.Value.Committed)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                        .MissingFreshFrameResetEBPAt34F1
                && FrameReset.Value.ESPAfter == InitialESP
                && StackRead is null
                && Pop is null
                && ContinuationBoundary is null
                && ResultESP == InitialESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .MoveEspEbpRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .MoveEspEbpRawFileOffset
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint resetESP = Capture.FreshFrameResetEBP!.Value;
        if (FrameReset.Value.ESPAfter != resetESP
            || StackRead is not {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .PopEBPRawFileOffset,
                Address: var address,
                Width: GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.ReadWidth,
                Value: var value,
                Captured: var captured,
                Attempted: true,
                NativeReadExecuted: false
            }
            || address != resetESP
            || value != Capture.FreshPopEBPStackDWORD
            || captured != Capture.FreshPopEBPStackDWORD.HasValue)
        {
            return false;
        }

        if (!Capture.FreshPopEBPStackDWORD.HasValue)
        {
            return Outcome
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                        .MissingFreshPopEBPStackDWORDAt34F3
                && Pop is {
                    InstructionRawFileOffset:
                        GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                            .PopEBPRawFileOffset,
                    Encoding: GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .PopEBPEncoding,
                    StackPointerBefore: var missingBefore,
                    StackPointerAfter: var missingAfter,
                    EBPBefore: var missingEBPBefore,
                    Value: null,
                    EBPMovedTo: null,
                    Attempted: true,
                    Reached: true,
                    Committed: false,
                    FlagsWritten: false
                }
                && missingBefore == resetESP
                && missingAfter == resetESP
                && missingEBPBefore == Capture.FreshFrameResetEBP
                && Pop.Value.FlagsBefore == FrameReset.Value.FlagsAfter
                && Pop.Value.FlagsAfter == Pop.Value.FlagsBefore
                && ResultESP == resetESP
                && LastReachedRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .PopEBPRawFileOffset
                && StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .PopEBPRawFileOffset
                && ContinuationBoundary is null
                && !ChildBoundedCompletionBoundaryReached;
        }

        uint resultESP = unchecked(resetESP
            + GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.DwordWidth);
        return Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .CompletedBeforeRet8At34F4
            && Pop is {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .PopEBPRawFileOffset,
                Encoding: GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .PopEBPEncoding,
                StackPointerBefore: var before,
                StackPointerAfter: var after,
                EBPBefore: var ebpBefore,
                Value: var popValue,
                EBPMovedTo: var movedTo,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false
            }
            && before == resetESP
            && after == resultESP
            && ebpBefore == Capture.FreshFrameResetEBP
            && popValue == Capture.FreshPopEBPStackDWORD
            && movedTo == Capture.FreshPopEBPStackDWORD
            && Pop.Value.FlagsBefore == FrameReset.Value.FlagsAfter
            && Pop.Value.FlagsAfter == Pop.Value.FlagsBefore
            && ResultESP == resultESP
            && ContinuationBoundary is {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .StopBeforeRet8RawFileOffset,
                NextInstructionEncoding:
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                        .Ret8Encoding,
                Reached: true,
                NextInstructionExecuted: false
            }
            && ChildBoundedCompletionBoundaryReached
            && LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.PopEBPRawFileOffset
            && StopBeforeRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .StopBeforeRet8RawFileOffset;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                .CompletedBeforeRet8At34F4
        && ExactBoundedCompletion
        && FreshFrameResetEBPCaptureState
            == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Captured
        && FreshPopEBPStackCaptureState
            == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Captured
        && ESPResetFromFreshEBP
        && ESPAdvancedForCommittedPop
        && EBPAfterPop == Capture.FreshPopEBPStackDWORD
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && FlagsPreserved
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadsExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !NativeABIOrProtocolSemanticsModeled;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && (FrameReset is null
            || FrameReset.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .MoveEspEbpRawFileOffset)
        && (StackRead is null
            || StackRead.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .PopEBPRawFileOffset)
        && (Pop is null
            || Pop.Value.InstructionRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                    .PopEBPRawFileOffset)
        && (!SuffixAdmitted
            || InitialESP == ParentPlan.ResultESP
                && FrameReset is { Attempted: true, Reached: true });

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan reprojected =
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
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.FreshPopECXCaptureState == ParentPlan.FreshPopECXCaptureState
            && reprojected.FreshPopEDICaptureState == ParentPlan.FreshPopEDICaptureState
            && reprojected.FreshPopESICaptureState == ParentPlan.FreshPopESICaptureState
            && reprojected.FreshPopEBXCaptureState == ParentPlan.FreshPopEBXCaptureState
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.Pops.SequenceEqual(ParentPlan.Pops)
            && reprojected.ParentPlan.Outcome == ParentPlan.ParentPlan.Outcome
            && reprojected.ParentPlan.ExactBoundedCompletion
                == ParentPlan.ParentPlan.ExactBoundedCompletion
            && reprojected.ParentPlan.ExactCleanCompletion
                == ParentPlan.ParentPlan.ExactCleanCompletion
            && reprojected.ParentPlan.FreshPostCallESP
                == ParentPlan.ParentPlan.FreshPostCallESP
            && reprojected.ParentPlan.FlagsAtStop == ParentPlan.ParentPlan.FlagsAtStop
            && reprojected.ParentPlan.LastReachedRawFileOffset
                == ParentPlan.ParentPlan.LastReachedRawFileOffset
            && reprojected.ParentPlan.StopBeforeRawFileOffset
                == ParentPlan.ParentPlan.StopBeforeRawFileOffset
            && reprojected.ParentPlan.InstructionOrder.SequenceEqual(
                ParentPlan.ParentPlan.InstructionOrder)
            && reprojected.ParentPlan.Reads.SequenceEqual(ParentPlan.ParentPlan.Reads)
            && reprojected.ParentPlan.MemoryEffects.SequenceEqual(
                ParentPlan.ParentPlan.MemoryEffects)
            && reprojected.ContinuationBoundary == ParentPlan.ContinuationBoundary;
    }
}

/// <summary>Projects MOV ESP,EBP and POP EBP, stopping before RETN 8.</summary>
public static class GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
{
    public const uint MoveEspEbpRawFileOffset = 0x00BD34F1;
    public const uint PopEBPRawFileOffset = 0x00BD34F3;
    public const uint StopBeforeRet8RawFileOffset = 0x00BD34F4;
    public const uint DwordWidth = 4;
    public const byte ReadWidth = 4;
    public const string MoveEspEbpEncoding = "8B E5";
    public const string PopEBPEncoding = "5D";
    public const string Ret8Encoding = "C2 08 00";
    public const string CoordinatorProvidedBytes = "8B E5 5D C2 08 00";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00);

    public static GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan Project(
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan parentPlan)
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
            && parentPlan.FlagsAtStop is null;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetMove? frameReset = null;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetRead? stackRead = null;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPop? pop = null;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetBoundary? boundary = null;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState frameResetState;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState popStackState;
        GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome outcome =
            GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
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
            popStackState = BeforeAdmission(capture.FreshPopEBPStackDWORD.HasValue);
        }
        else
        {
            frameResetState = capture.FreshFrameResetEBP.HasValue
                ? GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Missing;
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
                popStackState = capture.FreshPopEBPStackDWORD.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                        .SuppliedButUnused
                    : GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                        .NotReached;
                outcome =
                    GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                        .MissingFreshFrameResetEBPAt34F1;
            }
            else
            {
                uint resetESP = capture.FreshFrameResetEBP.Value;
                resultESP = resetESP;
                lastReached = PopEBPRawFileOffset;
                stopBefore = PopEBPRawFileOffset;
                popStackState = capture.FreshPopEBPStackDWORD.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Captured
                    : GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Missing;
                stackRead = new(
                    PopEBPRawFileOffset,
                    resetESP,
                    ReadWidth,
                    capture.FreshPopEBPStackDWORD,
                    Attempted: true,
                    Captured: capture.FreshPopEBPStackDWORD.HasValue,
                    NativeReadExecuted: false);
                pop = new(
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

                if (!capture.FreshPopEBPStackDWORD.HasValue)
                {
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                            .MissingFreshPopEBPStackDWORDAt34F3;
                }
                else
                {
                    resultESP = unchecked(resetESP + DwordWidth);
                    boundary = new(
                        StopBeforeRet8RawFileOffset,
                        Ret8Encoding,
                        Reached: true,
                        NextInstructionExecuted: false);
                    childComplete = true;
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                            .CompletedBeforeRet8At34F4;
                    lastReached = PopEBPRawFileOffset;
                    stopBefore = StopBeforeRet8RawFileOffset;
                }
            }
        }

        return new(
            capture,
            parentPlan,
            frameReset,
            stackRead,
            pop,
            boundary,
            frameResetState,
            popStackState,
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

    private static GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.NotReached;
}
