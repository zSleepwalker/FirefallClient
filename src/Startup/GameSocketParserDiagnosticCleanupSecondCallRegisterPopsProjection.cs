using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture(
    GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture ParentCapture,
    uint? FreshPopECX,
    uint? FreshPopEDI,
    uint? FreshPopESI,
    uint? FreshPopEBX);

public enum GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
{
    ParentContinuationNotAdmitted,
    MissingFreshPopECXAt34ED,
    MissingFreshPopEDIAt34EE,
    MissingFreshPopESIAt34EF,
    MissingFreshPopEBXAt34F0,
    CompletedBeforeMoveESPFromEBPAt34F1
}

public enum GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind
{
    PopECXAt34ED,
    PopEDIAt34EE,
    PopESIAt34EF,
    PopEBXAt34F0
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallRegisterPopsRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallRegisterPop(
    GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind Kind,
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

public readonly record struct GameSocketParserDiagnosticCleanupSecondCallRegisterPopsBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public sealed class GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan(
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallRegisterPopsRead> reads,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallRegisterPop> pops,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsBoundary? continuationBoundary,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState ecxCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState ediCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState esiCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState ebxCaptureState,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome outcome,
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
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactBoundedCompletion = new(ComputeExactBoundedCompletion);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallRegisterPopsRead> Reads { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondCallRegisterPop> Pops { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsBoundary? ContinuationBoundary
        { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        FreshPopECXCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        FreshPopEDICaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        FreshPopESICaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        FreshPopEBXCaptureState { get; }
    public GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome Outcome { get; }
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
    public bool ESPAdvancedForCommittedPops =>
        SuffixAdmitted
        && Pops.All(pop => pop.Committed
            ? pop.StackPointerAfter == unchecked(pop.StackPointerBefore!.Value
                + GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.DwordWidth)
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
            GameSocketParserDiagnosticCleanupSecondCallRegisterPop pop = Pops[index];
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsRead read = Reads[index];
            bool committed = read.Captured;
            if (pop.Kind != descriptor.Kind
                || pop.InstructionRawFileOffset != descriptor.RawFileOffset
                || pop.Encoding != descriptor.Encoding
                || pop.DestinationRegister != descriptor.Register
                || read.InstructionRawFileOffset != descriptor.RawFileOffset
                || read.Width != GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                    .ReadWidth
                || read.Address != pop.StackPointerBefore
                || read.Value != pop.Value
                || !read.Attempted
                || read.Captured != pop.Committed
                || read.NativeReadExecuted
                || !pop.Attempted
                || !pop.Reached
                || pop.Committed != committed
                || pop.FlagsBefore is not null
                || pop.FlagsAfter is not null)
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
                    + GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.DwordWidth)
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
                    GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                        .StopBeforeMoveEspEbpRawFileOffset,
                NextInstructionEncoding:
                    GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                        .StopBeforeMoveEspEbpEncoding,
                Reached: true,
                NextInstructionExecuted: false
            }
            && ChildBoundedCompletionBoundaryReached
            && LastReachedRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                    .PopEBXRawFileOffset
            && StopBeforeRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                    .StopBeforeMoveEspEbpRawFileOffset;
    }

    private bool ComputeExactCleanCompletion() =>
        SuffixAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
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
        && FlagsAtStop is null
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeReadsExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed
        && !NativeABIOrProtocolSemanticsModeled;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && ParentPlan.Reads.SequenceEqual(reprojectedParentPlan.Value.Reads)
        && ParentPlan.MemoryEffects.SequenceEqual(reprojectedParentPlan.Value.MemoryEffects)
        && Reads.Length == Pops.Length
        && Pops.Select((pop, index) =>
            pop.InstructionRawFileOffset == PopDescriptors[index].RawFileOffset
            && Reads[index].InstructionRawFileOffset == pop.InstructionRawFileOffset)
            .All(matches => matches)
        && (Pops.IsEmpty
            || InitialESP == ParentPlan.FreshPostCallESP
                && Pops[0].StackPointerBefore == InitialESP);

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentContinuationReached == ParentPlan.ParentContinuationReached
            && reprojected.SuffixAdmitted == ParentPlan.SuffixAdmitted
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.FreshPostCallESP == ParentPlan.FreshPostCallESP
            && reprojected.FreshPostCallESPCaptureState
                == ParentPlan.FreshPostCallESPCaptureState
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
        GameSocketParserDiagnosticCleanupSecondCallRegisterPop? pop =
            Pops.LastOrDefault(candidate =>
                candidate.DestinationRegister == register && candidate.Committed);
        return pop?.Value;
    }

    private static readonly PopDescriptor[] PopDescriptors =
    {
        new(GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .PopECXRawFileOffset,
            "59", "ECX",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopECXAt34ED),
        new(GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .PopEDIRawFileOffset,
            "5F", "EDI",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopEDIAt34EE),
        new(GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .PopESIRawFileOffset,
            "5E", "ESI",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopESIAt34EF),
        new(GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .PopEBXRawFileOffset,
            "5B", "EBX",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopEBXAt34F0)
    };

    private readonly record struct PopDescriptor(
        uint RawFileOffset,
        string Encoding,
        string Register,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind Kind);
}

/// <summary>Projects the four independently captured stack pops, stopping before MOV ESP,EBP.</summary>
public static class GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
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
        "8B 4C 24 48 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D C2 08 00 CC";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x4C, 0x24, 0x48, 0x64, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
            0x59, 0x5F, 0x5E, 0x5B, 0x8B, 0xE5, 0x5D, 0xC2, 0x08, 0x00, 0xCC);

    public static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan Project(
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture capture,
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted FS-restore plan must match the captured parent.");

        bool parentContinuationReached =
            parentPlan.ExactCleanCompletion
            && parentPlan.ParentContinuationReached
            && parentPlan.StopBeforeRawFileOffset == PopECXRawFileOffset;
        bool suffixAdmitted =
            parentContinuationReached
            && parentPlan.ExactBoundedCompletion
            && parentPlan.FlagsAtStop is null;
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondCallRegisterPopsRead>();
        var pops =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondCallRegisterPop>();
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsBoundary? boundary = null;
        var states = new GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState[4];
        uint? initialESP = suffixAdmitted ? parentPlan.FreshPostCallESP : null;
        uint? esp = initialESP;
        uint? lastReached = parentPlan.LastReachedRawFileOffset;
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome outcome =
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
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
            PopDescriptor[] descriptors = Descriptors;
            for (int index = 0; index < descriptors.Length; index++)
            {
                PopDescriptor descriptor = descriptors[index];
                uint? value = values[index];
                states[index] = value.HasValue
                    ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured
                    : GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Missing;
                uint? address = esp;
                bool captured = value.HasValue;
                reads.Add(new(
                    descriptor.RawFileOffset,
                    address,
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
                    FlagsBefore: null,
                    FlagsAfter: null));
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

                if (index == descriptors.Length - 1)
                {
                    boundary = new(
                        StopBeforeMoveEspEbpRawFileOffset,
                        StopBeforeMoveEspEbpEncoding,
                        Reached: true,
                        NextInstructionExecuted: false);
                    childComplete = true;
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
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

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        BeforeAdmission(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        AfterFailedRead(bool supplied) =>
            supplied
                ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                    .SuppliedButUnused
                : GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome MissingOutcome(
        int index) =>
        index switch
        {
            0 => GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopECXAt34ED,
            1 => GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopEDIAt34EE,
            2 => GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopESIAt34EF,
            _ => GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopEBXAt34F0
        };

    private static readonly PopDescriptor[] Descriptors =
    {
        new(PopECXRawFileOffset, PopECXEncoding, "ECX",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopECXAt34ED),
        new(PopEDIRawFileOffset, PopEDIEncoding, "EDI",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopEDIAt34EE),
        new(PopESIRawFileOffset, PopESIEncoding, "ESI",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopESIAt34EF),
        new(PopEBXRawFileOffset, PopEBXEncoding, "EBX",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind.PopEBXAt34F0)
    };

    private readonly record struct PopDescriptor(
        uint RawFileOffset,
        string Encoding,
        string Register,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsKind Kind);
}
