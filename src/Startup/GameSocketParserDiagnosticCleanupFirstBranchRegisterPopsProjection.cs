using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCapture(
    GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCapture ParentCapture,
    uint? FreshPopECX,
    uint? FreshPopEDI,
    uint? FreshPopESI,
    uint? FreshPopEBX);

public enum GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshPopECXAt34ED,
    MissingFreshPopEDIAt34EE,
    MissingFreshPopESIAt34EF,
    MissingFreshPopEBXAt34F0,
    CompletedBeforeMoveESPFromEBPAt34F1
}

public enum GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind
{
    PopECXAt34ED,
    PopEDIAt34EE,
    PopESIAt34EF,
    PopEBXAt34F0
}

public readonly record struct GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupFirstBranchRegisterPop(
    GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind Kind,
    uint InstructionRawFileOffset,
    string Encoding,
    string DestinationRegister,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public sealed class GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan(
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCapture capture,
        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsRead> reads,
        ImmutableArray<GameSocketParserDiagnosticCleanupFirstBranchRegisterPop> pops,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsBoundary? continuationBoundary,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState ecxCaptureState,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState ediCaptureState,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState esiCaptureState,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState ebxCaptureState,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome outcome,
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
        Reads = reads;
        Pops = pops;
        ContinuationBoundary = continuationBoundary;
        FreshPopECXCaptureState = ecxCaptureState;
        FreshPopEDICaptureState = ediCaptureState;
        FreshPopESICaptureState = esiCaptureState;
        FreshPopEBXCaptureState = ebxCaptureState;
        Outcome = outcome;
        ParentContinuationReached = parentContinuationReached;
        SuffixAdmitted = suffixAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        InitialESP = initialESP;
        ResultESP = resultESP;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        reprojectedParentPlan = new(
            () => GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection.Project(
                Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsRead> Reads
        { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupFirstBranchRegisterPop> Pops { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsBoundary?
        ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        FreshPopECXCaptureState { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        FreshPopEDICaptureState { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        FreshPopESICaptureState { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        FreshPopEBXCaptureState { get; }
    public GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome Outcome { get; }
    public bool ParentContinuationReached { get; }
    public bool SuffixAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public uint? InitialESP { get; }
    public uint? ResultESP { get; }
    public uint? ECXAfterPop => CommittedValue("ECX");
    public uint? EDIAfterPop => CommittedValue("EDI");
    public uint? ESIAfterPop => CommittedValue("ESI");
    public uint? EBXAfterPop => CommittedValue("EBX");
    public uint? EAXAfterPop => null;
    public uint? EBPAfterPop => null;
    public bool EAXCaptured => false;
    public bool EBPCaptured => false;
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
    public bool FirstXaddAddFlagsPreserved =>
        SuffixAdmitted
        && ParentPlan.FlagsAtStop.HasValue
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && Pops.All(pop =>
            pop.FlagsBefore == ParentPlan.FlagsAtStop
            && pop.FlagsAfter == ParentPlan.FlagsAtStop);
    public bool ESPAdvancedForCommittedPops =>
        SuffixAdmitted
        && Pops.All(pop => pop.Committed
            ? pop.StackPointerAfter == unchecked(pop.StackPointerBefore!.Value
                + GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.DwordWidth)
            : pop.StackPointerAfter == pop.StackPointerBefore)
        && (Pops.Length == 0
            ? ResultESP == InitialESP
            : ResultESP == Pops[^1].StackPointerAfter);
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
                && Pops.IsEmpty
                && Reads.IsEmpty;
        }

        if (Pops.IsEmpty || Pops.Length > PopDescriptors.Length || Reads.Length != Pops.Length)
        {
            return false;
        }

        for (int index = 0; index < Pops.Length; index++)
        {
            PopDescriptor descriptor = PopDescriptors[index];
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPop pop = Pops[index];
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsRead read = Reads[index];
            bool committed = read.Captured;
            if (pop.Kind != descriptor.Kind
                || pop.InstructionRawFileOffset != descriptor.RawFileOffset
                || pop.Encoding != descriptor.Encoding
                || pop.DestinationRegister != descriptor.Register
                || read.InstructionRawFileOffset != descriptor.RawFileOffset
                || read.Width != GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                    .ReadWidth
                || read.Address != pop.StackPointerBefore
                || read.Value != pop.Value
                || !read.Attempted
                || read.Captured != pop.Committed
                || read.NativeReadExecuted
                || !pop.Attempted
                || !pop.Reached
                || pop.Committed != committed
                || pop.FlagsBefore != ParentPlan.FlagsAtStop
                || pop.FlagsAfter != ParentPlan.FlagsAtStop)
            {
                return false;
            }

            if (index > 0
                && pop.StackPointerBefore != Pops[index - 1].StackPointerAfter)
            {
                return false;
            }

            uint? expectedESP = committed
                ? unchecked(pop.StackPointerBefore!.Value
                    + GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                        .DwordWidth)
                : pop.StackPointerBefore;
            if (pop.StackPointerAfter != expectedESP)
            {
                return false;
            }

            if (!committed)
            {
                return index == Pops.Length - 1
                    && StopBeforeRawFileOffset == descriptor.RawFileOffset
                    && LastReachedRawFileOffset == descriptor.RawFileOffset
                    && ContinuationBoundary is null;
            }
        }

        if (Pops.Length < PopDescriptors.Length)
        {
            return false;
        }

        return ContinuationBoundary is {
                InstructionRawFileOffset:
                    GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                        .StopBeforeMoveEspEbpRawFileOffset,
                NextInstructionEncoding:
                    GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                        .StopBeforeMoveEspEbpEncoding,
                Reached: true,
                NextInstructionExecuted: false
            }
            && ChildBoundedCompletionBoundaryReached
            && LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                    .PopEBXRawFileOffset
            && StopBeforeRawFileOffset
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                    .StopBeforeMoveEspEbpRawFileOffset;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .CompletedBeforeMoveESPFromEBPAt34F1
        && ExactBoundedCompletion
        && Pops.Length == PopDescriptors.Length
        && Pops.All(pop => pop.Committed)
        && Reads.Length == PopDescriptors.Length
        && Reads.All(read => read.Captured)
        && ESPAdvancedForCommittedPops
        && ECXAfterPop == Capture.FreshPopECX
        && EDIAfterPop == Capture.FreshPopEDI
        && ESIAfterPop == Capture.FreshPopESI
        && EBXAfterPop == Capture.FreshPopEBX
        && EAXAfterPop is null
        && EBPAfterPop is null
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && FirstXaddAddFlagsPreserved
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadsExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !NativeABIOrProtocolSemanticsModeled;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ReprojectedParentMatches()
        && ParentPlan.Reads.SequenceEqual(reprojectedParentPlan.Value.Reads)
        && ParentPlan.MemoryEffects.SequenceEqual(reprojectedParentPlan.Value.MemoryEffects)
        && Reads.Length == Pops.Length
        && Pops.Select((pop, index) =>
            pop.InstructionRawFileOffset == PopDescriptors[index].RawFileOffset
            && Reads[index].InstructionRawFileOffset == pop.InstructionRawFileOffset)
            .All(matches => matches)
        && (Pops.IsEmpty
            || InitialESP == ParentPlan.FreshESPAt34E2
                && Pops[0].StackPointerBefore == InitialESP);

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentContinuationReached == ParentPlan.ParentContinuationReached
            && reprojected.SuffixAdmitted == ParentPlan.SuffixAdmitted
            && reprojected.FreshESPAt34E2 == ParentPlan.FreshESPAt34E2
            && reprojected.FreshESPCaptureState == ParentPlan.FreshESPCaptureState
            && reprojected.FreshDWORDCaptureState == ParentPlan.FreshDWORDCaptureState
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.RegisterMove == ParentPlan.RegisterMove
            && reprojected.MemoryWrite == ParentPlan.MemoryWrite
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.InstructionOrder.SequenceEqual(ParentPlan.InstructionOrder)
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    }

    private uint? CommittedValue(string register)
    {
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPop? pop =
            Pops.LastOrDefault(candidate =>
                candidate.DestinationRegister == register && candidate.Committed);
        return pop?.Value;
    }

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        AfterFailedRead(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.NotReached;

    private static readonly ImmutableArray<PopDescriptor> PopDescriptors =
        ImmutableArray.Create(
            new PopDescriptor(PopECXRawFileOffset, PopECXEncoding, "ECX",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopECXAt34ED),
            new PopDescriptor(PopEDIRawFileOffset, PopEDIEncoding, "EDI",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopEDIAt34EE),
            new PopDescriptor(PopESIRawFileOffset, PopESIEncoding, "ESI",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopESIAt34EF),
            new PopDescriptor(PopEBXRawFileOffset, PopEBXEncoding, "EBX",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopEBXAt34F0));

    private readonly record struct PopDescriptor(
        uint RawFileOffset,
        string Encoding,
        string Register,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind Kind);

    private static uint PopECXRawFileOffset =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopECXRawFileOffset;
    private static uint PopEDIRawFileOffset =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopEDIRawFileOffset;
    private static uint PopESIRawFileOffset =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopESIRawFileOffset;
    private static uint PopEBXRawFileOffset =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopEBXRawFileOffset;
    private static string PopECXEncoding =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopECXEncoding;
    private static string PopEDIEncoding =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopEDIEncoding;
    private static string PopESIEncoding =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopESIEncoding;
    private static string PopEBXEncoding =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.PopEBXEncoding;
}

/// <summary>Projects four fresh stack pops on the taken first-XADD branch, stopping before MOV ESP,EBP.</summary>
public static class GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
{
    public const uint PopECXRawFileOffset = 0x00BD34ED;
    public const uint PopEDIRawFileOffset = 0x00BD34EE;
    public const uint PopESIRawFileOffset = 0x00BD34EF;
    public const uint PopEBXRawFileOffset = 0x00BD34F0;
    public const uint StopBeforeMoveEspEbpRawFileOffset = 0x00BD34F1;
    public const uint DwordWidth = 4;
    public const byte ReadWidth = 4;
    public const string PopECXEncoding = "59";
    public const string PopEDIEncoding = "5F";
    public const string PopESIEncoding = "5E";
    public const string PopEBXEncoding = "5B";
    public const string StopBeforeMoveEspEbpEncoding = "8B E5";
    public const string CoordinatorProvidedBytes =
        "59 5F 5E 5B 8B E5 5D C2 08 00 CC";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x59, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00, 0xCC);

    public static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan Project(
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCapture capture)
    {
        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan parentPlan =
            GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection.Project(
                capture.ParentCapture);
        bool parentContinuationReached =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentContinuationReached
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .CompletedBeforePopAt34ED
            && parentPlan.StopBeforeRawFileOffset == PopECXRawFileOffset;
        bool suffixAdmitted =
            parentContinuationReached
            && parentPlan.ExactBoundedCompletion
            && HasKnownFlags(parentPlan.FlagsAtStop);
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsRead>();
        var pops =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupFirstBranchRegisterPop>();
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsBoundary? boundary = null;
        var states =
            new GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState[4];
        uint? initialESP = suffixAdmitted ? parentPlan.FreshESPAt34E2 : null;
        uint? esp = initialESP;
        uint? lastReached = parentPlan.LastReachedRawFileOffset;
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome outcome =
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .ParentContinuationNotAdmitted;
        bool childComplete = false;
        uint?[] values =
        {
            capture.FreshPopECX,
            capture.FreshPopEDI,
            capture.FreshPopESI,
            capture.FreshPopEBX
        };

        if (!suffixAdmitted)
        {
            for (int index = 0; index < values.Length; index++)
            {
                states[index] = BeforeAdmission(values[index].HasValue);
            }
        }
        else
        {
            for (int index = 0; index < PopDescriptors.Length; index++)
            {
                PopDescriptor descriptor = PopDescriptors[index];
                uint? value = values[index];
                states[index] = value.HasValue
                    ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                        .Captured
                    : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                        .Missing;
                bool captured = value.HasValue;
                reads.Add(new(
                    descriptor.RawFileOffset,
                    esp,
                    ReadWidth,
                    value,
                    Attempted: true,
                    Captured: captured,
                    NativeReadExecuted: false));
                uint? nextESP = captured
                    ? unchecked(esp!.Value + DwordWidth)
                    : esp;
                pops.Add(new(
                    descriptor.Kind,
                    descriptor.RawFileOffset,
                    descriptor.Encoding,
                    descriptor.Register,
                    esp,
                    nextESP,
                    value,
                    Attempted: true,
                    Reached: true,
                    Committed: captured,
                    FlagsBefore: flagsAtStop,
                    FlagsAfter: flagsAtStop));
                lastReached = descriptor.RawFileOffset;
                stopBefore = descriptor.RawFileOffset;
                esp = nextESP;

                if (!captured)
                {
                    for (int later = index + 1; later < values.Length; later++)
                    {
                        states[later] = AfterFailedRead(values[later].HasValue);
                    }

                    outcome = MissingOutcome(index);
                    break;
                }

                if (index == PopDescriptors.Length - 1)
                {
                    boundary = new(
                        StopBeforeMoveEspEbpRawFileOffset,
                        StopBeforeMoveEspEbpEncoding,
                        Reached: true,
                        NextInstructionExecuted: false);
                    childComplete = true;
                    outcome =
                        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                            .CompletedBeforeMoveESPFromEBPAt34F1;
                    lastReached = PopEBXRawFileOffset;
                    stopBefore = StopBeforeMoveEspEbpRawFileOffset;
                }
            }
        }

        return new(
            capture,
            parentPlan,
            reads.ToImmutable(),
            pops.ToImmutable(),
            boundary,
            states[0],
            states[1],
            states[2],
            states[3],
            outcome,
            parentContinuationReached,
            suffixAdmitted,
            childComplete,
            initialESP,
            esp,
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

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        AfterFailedRead(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome MissingOutcome(
        int index) =>
        index switch
        {
            0 => GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopECXAt34ED,
            1 => GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopEDIAt34EE,
            2 => GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopESIAt34EF,
            _ => GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopEBXAt34F0
        };

    private static readonly ImmutableArray<PopDescriptor> PopDescriptors =
        ImmutableArray.Create(
            new PopDescriptor(PopECXRawFileOffset, PopECXEncoding, "ECX",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopECXAt34ED),
            new PopDescriptor(PopEDIRawFileOffset, PopEDIEncoding, "EDI",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopEDIAt34EE),
            new PopDescriptor(PopESIRawFileOffset, PopESIEncoding, "ESI",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopESIAt34EF),
            new PopDescriptor(PopEBXRawFileOffset, PopEBXEncoding, "EBX",
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind.PopEBXAt34F0));

    private readonly record struct PopDescriptor(
        uint RawFileOffset,
        string Encoding,
        string Register,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsKind Kind);
}
