using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallReturnCapture(
    GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture ParentCapture,
    uint? FreshVtableSlotDWORDAtEAX,
    bool? OpaqueCallReturnedNormally);

public enum GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
{
    ParentBranchNotAdmitted,
    ParentNotCleanAt34CF,
    MissingVtableSlotDWORDAt34CF,
    MissingOpaqueCallReturnObservation,
    OpaqueCallDidNotReturnNormally,
    OpaqueCallReturnedNormally
}

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallReturnMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallReturnCallBoundary(
    uint InstructionRawFileOffset,
    uint? VtableSlotAddress,
    uint TargetDWORD,
    uint? PreCallEAX,
    uint? PreCallECX,
    uint? PreCallESI,
    uint? PreCallESP,
    bool LogicalCallAttempted,
    bool NativeCallExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallReturnContinuationBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public enum GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind
{
    ReadFreshVtableSlotDWORD,
    OpaqueCallBoundary,
    OpaqueCallReturnObservation,
    ContinuationBoundary
}

public readonly record struct GameSocketParserDiagnosticCleanupVirtualCallReturnStep(
    GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public sealed class GameSocketParserDiagnosticCleanupVirtualCallReturnPlan
{
    internal GameSocketParserDiagnosticCleanupVirtualCallReturnPlan(
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture capture,
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupVirtualCallReturnStep> instructionOrder,
        GameSocketParserDiagnosticCleanupVirtualCallReturnMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupVirtualCallReturnCallBoundary? callBoundary,
        GameSocketParserDiagnosticCleanupVirtualCallReturnContinuationBoundary?
            continuationBoundary,
        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState targetCaptureState,
        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState returnCaptureState,
        GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome outcome,
        bool parentCallBoundaryAdmitted,
        bool targetReadAttempted,
        bool targetReadCaptured,
        bool logicalCallAttempted,
        bool returnObservationCaptured,
        bool? opaqueCallReturnedNormally,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        bool childBoundedCompletionBoundaryReached,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        InstructionOrder = instructionOrder;
        MemoryRead = memoryRead;
        CallBoundary = callBoundary;
        ContinuationBoundary = continuationBoundary;
        TargetCaptureState = targetCaptureState;
        ReturnCaptureState = returnCaptureState;
        Outcome = outcome;
        ParentCallBoundaryAdmitted = parentCallBoundaryAdmitted;
        TargetReadAttempted = targetReadAttempted;
        TargetReadCaptured = targetReadCaptured;
        LogicalCallAttempted = logicalCallAttempted;
        ReturnObservationCaptured = returnObservationCaptured;
        OpaqueCallReturnedNormally = opaqueCallReturnedNormally;
        FlagsAtStop = flagsAtStop;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserDiagnosticCleanupVirtualCallReturnCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupVirtualCallReturnStep>
        InstructionOrder { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnCallBoundary? CallBoundary { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnContinuationBoundary?
        ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState TargetCaptureState { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState ReturnCaptureState { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome Outcome { get; }
    public bool ParentCallBoundaryAdmitted { get; }
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool TargetReadAttempted { get; }
    public bool TargetReadCaptured { get; }
    public bool LogicalCallAttempted { get; }
    public bool ReturnObservationCaptured { get; }
    public bool? OpaqueCallReturnedNormally { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool BoundedCompletionBoundaryReached =>
        ParentCallBoundaryAdmitted
            ? ChildBoundedCompletionBoundaryReached
            : ParentPlan.ExactBoundedCompletion;
    public bool ExactBoundedCompletion => BoundedCompletionBoundaryReached;
    public bool ExactCleanCompletion =>
        ParentCallBoundaryAdmitted
        && ParentPlan.IsCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                .OpaqueCallReturnedNormally
        && TargetCaptureState
            == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Captured
        && ReturnCaptureState
            == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Captured
        && TargetReadAttempted
        && TargetReadCaptured
        && MemoryRead is { Attempted: true, Captured: true, NativeReadExecuted: false }
        && LogicalCallAttempted
        && CallBoundary is {
            LogicalCallAttempted: true,
            NativeCallExecuted: false
        }
        && ReturnObservationCaptured
        && OpaqueCallReturnedNormally == true
        && ContinuationBoundary is {
            InstructionRawFileOffset: GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ContinuationRawFileOffset,
            Reached: true,
            NextInstructionExecuted: false
        }
        && ChildBoundedCompletionBoundaryReached
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ContinuationRawFileOffset
        && FlagsAtStop is null
        && PostCallRegistersCaptured == false
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeCallExecuted
        && !OpaqueCallTargetEntered
        && !VtableSlotReadExecuted
        && !CleanupReleaseIdentityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !ParserExecutionModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(StopBeforeRawFileOffset.Value)
            : null;
    public uint? PreCallEAX => CallBoundary?.PreCallEAX;
    public uint? PreCallECX => CallBoundary?.PreCallECX;
    public uint? PreCallESI => CallBoundary?.PreCallESI;
    public uint? PreCallESP => CallBoundary?.PreCallESP;
    public bool PostCallRegistersCaptured => false;
    public uint? PostCallEAX => null;
    public uint? PostCallECX => null;
    public uint? PostCallESI => null;
    public uint? PostCallESP => null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;
    public bool OrderedSemanticPrefixPreserved =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);

    public bool NativeInstructionsExecuted => false;
    public bool NativeCallExecuted => false;
    public bool OpaqueCallTargetEntered => false;
    public bool VtableSlotReadExecuted => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan reprojected =
            GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentBranchAdmitted == ParentPlan.ParentBranchAdmitted
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.FreshVptrCaptureState == ParentPlan.FreshVptrCaptureState
            && reprojected.ResultESP == ParentPlan.ResultESP
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ECXValue == ParentPlan.ECXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.ParentPlan.ParentPlan.MemoryRead
                == ParentPlan.ParentPlan.ParentPlan.MemoryRead
            && reprojected.ParentPlan.ParentPlan.MemoryWrite
                == ParentPlan.ParentPlan.ParentPlan.MemoryWrite
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.IsCleanCompletion == ParentPlan.IsCleanCompletion
            && reprojected.OrderedSemanticPrefixPreserved
                == ParentPlan.OrderedSemanticPrefixPreserved;
    }
}

/// <summary>Projects the opaque call return and stops before the following LEA.</summary>
public static class GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
{
    public const uint OpaqueCallRawFileOffset = 0x00BD34CF;
    public const uint ContinuationRawFileOffset = 0x00BD34D1;
    public const string OpaqueCallEncoding = "FF 10";
    public const string ContinuationEncoding = "8D 46 08";
    public const byte DwordWidth = 4;
    public const string CoordinatorProvidedBytes =
        "F0 0F C1 08 75 17 8B 06 8B CE FF 10 8D 46 08 F0";
    public const bool BytesIndependentlyVerified = false;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesMemoryWrites => false;
    public static bool ExecutesCalls => false;
    public static bool EntersOpaqueCallTarget => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0xF0, 0x0F, 0xC1, 0x08, 0x75, 0x17, 0x8B, 0x06,
            0x8B, 0xCE, 0xFF, 0x10, 0x8D, 0x46, 0x08, 0xF0);

    public static GameSocketParserDiagnosticCleanupVirtualCallReturnPlan Project(
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture capture)
    {
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan parentPlan =
            GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection.Project(
                capture.ParentCapture);
        var steps =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupVirtualCallReturnStep>();

        bool parentAtCallBoundary =
            parentPlan.ParentBranchAdmitted
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .BoundedBeforeIndirectCallAt34CF
            && parentPlan.StopBeforeRawFileOffset == OpaqueCallRawFileOffset;
        bool cleanParent = parentAtCallBoundary
            && parentPlan.IsCleanCompletion
            && parentPlan.FreshVptrReadCaptured
            && parentPlan.EAXValue.HasValue
            && parentPlan.ECXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.ResultESP.HasValue
            && parentPlan.FlagsAtStop.HasValue
            && parentPlan.Instructions.Length == 2
            && parentPlan.Instructions.All(instruction =>
                instruction.Attempted && instruction.Reached && instruction.Committed);

        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState targetState =
            BeforeAdmission(capture.FreshVtableSlotDWORDAtEAX.HasValue);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState returnState =
            BeforeAdmission(capture.OpaqueCallReturnedNormally.HasValue);
        GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome outcome;
        GameSocketParserDiagnosticCleanupVirtualCallReturnMemoryRead? memoryRead = null;
        GameSocketParserDiagnosticCleanupVirtualCallReturnCallBoundary? callBoundary = null;
        GameSocketParserDiagnosticCleanupVirtualCallReturnContinuationBoundary?
            continuationBoundary = null;
        bool targetReadAttempted = false;
        bool targetReadCaptured = false;
        bool logicalCallAttempted = false;
        bool returnObservationCaptured = false;
        bool childComplete = false;
        bool? returnedNormally = null;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        uint? lastReached = ParentLastReached(parentPlan);
        uint? stopBefore = parentPlan.StopBeforeRawFileOffset;

        if (!parentAtCallBoundary)
        {
            outcome =
                GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted;
        }
        else if (!cleanParent)
        {
            outcome =
                GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentNotCleanAt34CF;
        }
        else
        {
            targetState = capture.FreshVtableSlotDWORDAtEAX.HasValue
                ? GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Captured
                : GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Missing;
            targetReadAttempted = true;
            targetReadCaptured = capture.FreshVtableSlotDWORDAtEAX.HasValue;
            memoryRead = new(
                OpaqueCallRawFileOffset,
                parentPlan.EAXValue,
                DwordWidth,
                capture.FreshVtableSlotDWORDAtEAX,
                Attempted: true,
                Captured: targetReadCaptured,
                NativeReadExecuted: false);
            steps.Add(new(
                GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind
                    .ReadFreshVtableSlotDWORD,
                OpaqueCallRawFileOffset,
                parentPlan.EAXValue,
                capture.FreshVtableSlotDWORDAtEAX));
            stopBefore = OpaqueCallRawFileOffset;

            if (!targetReadCaptured)
            {
                returnState = UnreachedOrUnused(
                    capture.OpaqueCallReturnedNormally.HasValue);
                outcome =
                    GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                        .MissingVtableSlotDWORDAt34CF;
            }
            else
            {
                logicalCallAttempted = true;
                callBoundary = new(
                    OpaqueCallRawFileOffset,
                    parentPlan.EAXValue,
                    capture.FreshVtableSlotDWORDAtEAX!.Value,
                    parentPlan.EAXValue,
                    parentPlan.ECXValue,
                    parentPlan.ESIValue,
                    parentPlan.ResultESP,
                    LogicalCallAttempted: true,
                    NativeCallExecuted: false);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind
                        .OpaqueCallBoundary,
                    OpaqueCallRawFileOffset,
                    parentPlan.EAXValue,
                    capture.FreshVtableSlotDWORDAtEAX));

                if (capture.OpaqueCallReturnedNormally is not bool callReturnedNormally)
                {
                    returnState =
                        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Missing;
                    outcome =
                        GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                            .MissingOpaqueCallReturnObservation;
                }
                else
                {
                    returnState =
                        GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Captured;
                    returnObservationCaptured = true;
                    returnedNormally = callReturnedNormally;
                    steps.Add(new(
                        GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind
                            .OpaqueCallReturnObservation,
                        OpaqueCallRawFileOffset,
                        Value: callReturnedNormally ? 1u : 0u));

                    if (!callReturnedNormally)
                    {
                        outcome =
                            GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                                .OpaqueCallDidNotReturnNormally;
                    }
                    else
                    {
                        outcome =
                            GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                                .OpaqueCallReturnedNormally;
                        continuationBoundary = new(
                            ContinuationRawFileOffset,
                            ContinuationEncoding,
                            Reached: true,
                            NextInstructionExecuted: false);
                        steps.Add(new(
                            GameSocketParserDiagnosticCleanupVirtualCallReturnStepKind
                                .ContinuationBoundary,
                            ContinuationRawFileOffset));
                        childComplete = true;
                        lastReached = OpaqueCallRawFileOffset;
                        stopBefore = ContinuationRawFileOffset;
                        flagsAtStop = null;
                    }
                }
            }
        }

        if (!cleanParent && capture.FreshVtableSlotDWORDAtEAX.HasValue)
        {
            targetState =
                GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused;
        }
        if (!logicalCallAttempted && capture.OpaqueCallReturnedNormally.HasValue)
        {
            returnState =
                GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused;
        }

        return new(
            capture,
            parentPlan,
            steps.ToImmutable(),
            memoryRead,
            callBoundary,
            continuationBoundary,
            targetState,
            returnState,
            outcome,
            cleanParent,
            targetReadAttempted,
            targetReadCaptured,
            logicalCallAttempted,
            returnObservationCaptured,
            returnedNormally,
            flagsAtStop,
            childComplete,
            lastReached,
            stopBefore);
    }

    private static uint? ParentLastReached(
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan parentPlan)
    {
        foreach (GameSocketParserDiagnosticCleanupVirtualCallPreparationInstruction instruction
            in parentPlan.Instructions.Reverse())
        {
            if (instruction.Reached)
            {
                return instruction.InstructionRawFileOffset;
            }
        }

        return null;
    }

    private static GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState BeforeAdmission(
        bool supplied) =>
        supplied
            ? GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.SuppliedButUnused
            : GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
        UnreachedOrUnused(bool supplied) => BeforeAdmission(supplied);
}
