using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture(
    GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture ParentCapture,
    uint? FreshSlot4TargetDWORDAtWrappedEAXPlus4,
    bool? OpaqueCallReturnedNormally);

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
{
    ParentBranchNotAdmitted,
    ParentNotCleanAt34DF,
    MissingSlot4TargetDWORDAt34DF,
    MissingOpaqueCallReturnObservation,
    OpaqueCallDidNotReturnNormally,
    OpaqueCallReturnedNormally
}

public enum GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind
{
    ReadFreshSlot4TargetDWORD,
    OpaqueCallBoundary,
    OpaqueCallReturnObservation,
    ContinuationBoundary
}

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallReturnMemoryRead(
    uint InstructionRawFileOffset,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool NativeReadExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCallBoundary(
    uint InstructionRawFileOffset,
    uint? Slot4TargetAddress,
    uint TargetDWORD,
    uint? PreCallEAX,
    uint? PreCallEBX,
    uint? PreCallECX,
    uint? PreCallESI,
    uint? PreCallESP,
    bool LogicalCallAttempted,
    bool NativeCallExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallReturnContinuationBoundary(
    uint InstructionRawFileOffset,
    string NextInstructionEncoding,
    bool Reached,
    bool NextInstructionExecuted);

public readonly record struct GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStep(
    GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public sealed class GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan
{
    private readonly Lazy<GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStep>
            instructionOrder,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnMemoryRead? memoryRead,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCallBoundary? callBoundary,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnContinuationBoundary?
            continuationBoundary,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState targetCaptureState,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState returnCaptureState,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome outcome,
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
        reprojectedParentPlan = new(() =>
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection.ProjectCore(
                Capture.ParentCapture,
                ParentPlan.ParentPlan));
        orderedSemanticPrefixPreserved = new(ComputeOrderedSemanticPrefixPreserved);
        exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStep>
        InstructionOrder { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnMemoryRead? MemoryRead { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCallBoundary? CallBoundary { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnContinuationBoundary?
        ContinuationBoundary { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState TargetCaptureState
        { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState ReturnCaptureState
        { get; }
    public GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome Outcome { get; }
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
    public bool ExactBoundedCompletion =>
        ParentCallBoundaryAdmitted
            ? ChildBoundedCompletionBoundaryReached
            : ParentPlan.ExactBoundedCompletion
                && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;

    public uint? LastReachedRawFileOffset { get; }
    public uint? StopBeforeRawFileOffset { get; }
    public uint? StopBeforeCodeVirtualAddress =>
        StopBeforeRawFileOffset.HasValue
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                StopBeforeRawFileOffset.Value)
            : null;
    public uint? PreCallEAX => CallBoundary?.PreCallEAX;
    public uint? PreCallEBX => CallBoundary?.PreCallEBX;
    public uint? PreCallECX => CallBoundary?.PreCallECX;
    public uint? PreCallESI => CallBoundary?.PreCallESI;
    public uint? PreCallESP => CallBoundary?.PreCallESP;
    public bool PostCallRegistersCaptured => false;
    public uint? PostCallEAX => null;
    public uint? PostCallEBX => null;
    public uint? PostCallECX => null;
    public uint? PostCallESI => null;
    public uint? PostCallESP => null;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixRead> Reads =>
        ParentPlan.Reads;
    public ImmutableArray<GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect> MemoryEffects =>
        ParentPlan.MemoryEffects;

    public bool NativeInstructionsExecuted => false;
    public bool NativeCallExecuted => false;
    public bool OpaqueCallTargetEntered => false;
    public bool VtableSlotReadExecuted => false;
    public bool PointerValidityModeled => false;
    public bool CleanupReleaseIdentityModeled => false;
    public bool NativeMemoryEffectsModeled => false;
    public bool NativeABIOrProtocolSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ComputeExactCleanCompletion() =>
        ParentCallBoundaryAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                .OpaqueCallReturnedNormally
        && TargetCaptureState
            == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Captured
        && ReturnCaptureState
            == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Captured
        && TargetReadAttempted
        && TargetReadCaptured
        && MemoryRead is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                    .Slot4CallRawFileOffset,
            Width: GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.DwordWidth,
            Value: not null,
            Attempted: true,
            Captured: true,
            NativeReadExecuted: false
        }
        && MemoryRead.Value.Address == Slot4TargetAddress(PreCallEAX)
        && MemoryRead.Value.Value == Capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4
        && LogicalCallAttempted
        && CallBoundary is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                    .Slot4CallRawFileOffset,
            LogicalCallAttempted: true,
            NativeCallExecuted: false
        }
        && CallBoundary.Value.Slot4TargetAddress == MemoryRead.Value.Address
        && CallBoundary.Value.TargetDWORD == Capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4
        && ReturnObservationCaptured
        && OpaqueCallReturnedNormally == true
        && ContinuationBoundary is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                    .ContinuationRawFileOffset,
            NextInstructionEncoding:
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                    .ContinuationEncoding,
            Reached: true,
            NextInstructionExecuted: false
        }
        && ChildBoundedCompletionBoundaryReached
        && LastReachedRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .Slot4CallRawFileOffset
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ContinuationRawFileOffset
        && FlagsAtStop is null
        && !PostCallRegistersCaptured
        && OrderedSemanticPrefixPreserved
        && !NativeInstructionsExecuted
        && !NativeCallExecuted
        && !OpaqueCallTargetEntered
        && !VtableSlotReadExecuted
        && !PointerValidityModeled
        && !CleanupReleaseIdentityModeled
        && !NativeMemoryEffectsModeled
        && !NativeABIOrProtocolSemanticsModeled
        && !ParserExecutionModeled
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan reprojected =
            reprojectedParentPlan.Value;
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.ParentBranchAdmitted == ParentPlan.ParentBranchAdmitted
            && reprojected.ChildBoundedCompletionBoundaryReached
                == ParentPlan.ChildBoundedCompletionBoundaryReached
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.EAXValue == ParentPlan.EAXValue
            && reprojected.EBXValue == ParentPlan.EBXValue
            && reprojected.ECXValue == ParentPlan.ECXValue
            && reprojected.ESIValue == ParentPlan.ESIValue
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(ParentPlan.Instructions)
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.Reads.SequenceEqual(ParentPlan.Reads)
            && reprojected.MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);
    }

    private static uint? Slot4TargetAddress(uint? eax) =>
        eax.HasValue ? unchecked(eax.Value + 4u) : null;
}

/// <summary>Projects the second slot-four call return and stops before the next MOV.</summary>
public static class GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
{
    public const uint Slot4CallRawFileOffset = 0x00BD34DF;
    public const uint ContinuationRawFileOffset = 0x00BD34E2;
    public const string Slot4CallEncoding = "FF 50 04";
    public const string ContinuationEncoding = "8B 4C 24 48";
    public const byte DwordWidth = 4;
    public const string CoordinatorProvidedBytes =
        "4B 75 07 8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D";
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;
    public static bool ExecutesNativeReads => false;
    public static bool ExecutesMemoryWrites => false;
    public static bool ExecutesCalls => false;
    public static bool EntersOpaqueCallTarget => false;
    public static bool ExecutesContinuationInstruction => false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x4B, 0x75, 0x07, 0x8B, 0x06, 0x8B, 0xCE, 0xFF, 0x50,
            0x04, 0x8B, 0x4C, 0x24, 0x48, 0x64, 0x89, 0x0D);

    public static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan Project(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture capture)
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan parentPlan =
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection.Project(
                capture.ParentCapture);
        return ProjectCore(capture, parentPlan);
    }

    internal static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan ProjectCore(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture capture,
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan parentPlan)
    {
        ArgumentNullException.ThrowIfNull(parentPlan);
        if (parentPlan.Capture != capture.ParentCapture)
            throw new InvalidOperationException(
                "The trusted call-preparation plan must match the captured parent.");

        var steps =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStep>();

        bool parentAtCallBoundary =
            parentPlan.ParentBranchAdmitted
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                    .BoundedBeforeSlot4CallAt34DF
            && parentPlan.StopBeforeRawFileOffset == Slot4CallRawFileOffset;
        bool cleanParent =
            parentAtCallBoundary
            && parentPlan.ExactCleanCompletion
            && parentPlan.FreshOperationVptrReadCaptured
            && parentPlan.EAXValue.HasValue
            && parentPlan.EBXValue.HasValue
            && parentPlan.ECXValue.HasValue
            && parentPlan.ESIValue.HasValue
            && parentPlan.FlagsAtStop.HasValue
            && parentPlan.Instructions.Length == 2
            && parentPlan.Instructions.All(instruction =>
                instruction.Attempted && instruction.Reached && instruction.Committed);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState targetState =
            BeforeAdmission(capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4.HasValue);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState returnState =
            BeforeAdmission(capture.OpaqueCallReturnedNormally.HasValue);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome outcome;
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnMemoryRead? memoryRead = null;
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCallBoundary? callBoundary = null;
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnContinuationBoundary?
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
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted;
        }
        else if (!cleanParent)
        {
            outcome =
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .ParentNotCleanAt34DF;
        }
        else
        {
            targetState = capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4.HasValue
                ? GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Captured
                : GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Missing;
            uint slot4TargetAddress = unchecked(parentPlan.EAXValue!.Value + 4u);
            targetReadAttempted = true;
            targetReadCaptured = capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4.HasValue;
            memoryRead = new(
                Slot4CallRawFileOffset,
                slot4TargetAddress,
                DwordWidth,
                capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4,
                Attempted: true,
                Captured: targetReadCaptured,
                NativeReadExecuted: false);
            steps.Add(new(
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind
                    .ReadFreshSlot4TargetDWORD,
                Slot4CallRawFileOffset,
                slot4TargetAddress,
                capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4));
            stopBefore = Slot4CallRawFileOffset;
            lastReached = Slot4CallRawFileOffset;

            if (!targetReadCaptured)
            {
                returnState = UnreachedOrUnused(
                    capture.OpaqueCallReturnedNormally.HasValue);
                outcome =
                    GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                        .MissingSlot4TargetDWORDAt34DF;
            }
            else
            {
                logicalCallAttempted = true;
                callBoundary = new(
                    Slot4CallRawFileOffset,
                    slot4TargetAddress,
                    capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4!.Value,
                    parentPlan.EAXValue,
                    parentPlan.EBXValue,
                    parentPlan.ECXValue,
                    parentPlan.ESIValue,
                    parentPlan.ESPValue,
                    LogicalCallAttempted: true,
                    NativeCallExecuted: false);
                steps.Add(new(
                    GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind
                        .OpaqueCallBoundary,
                    Slot4CallRawFileOffset,
                    slot4TargetAddress,
                    capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4));

                if (capture.OpaqueCallReturnedNormally is not bool callReturnedNormally)
                {
                    returnState =
                        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                            .Missing;
                    outcome =
                        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                            .MissingOpaqueCallReturnObservation;
                }
                else
                {
                    returnState =
                        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                            .Captured;
                    returnObservationCaptured = true;
                    returnedNormally = callReturnedNormally;
                    steps.Add(new(
                        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind
                            .OpaqueCallReturnObservation,
                        Slot4CallRawFileOffset,
                        Value: callReturnedNormally ? 1u : 0u));

                    if (!callReturnedNormally)
                    {
                        outcome =
                            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                                .OpaqueCallDidNotReturnNormally;
                    }
                    else
                    {
                        outcome =
                            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                                .OpaqueCallReturnedNormally;
                        continuationBoundary = new(
                            ContinuationRawFileOffset,
                            ContinuationEncoding,
                            Reached: true,
                            NextInstructionExecuted: false);
                        steps.Add(new(
                            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnStepKind
                                .ContinuationBoundary,
                            ContinuationRawFileOffset));
                        childComplete = true;
                        stopBefore = ContinuationRawFileOffset;
                        flagsAtStop = null;
                    }
                }
            }
        }

        if (!cleanParent && capture.FreshSlot4TargetDWORDAtWrappedEAXPlus4.HasValue)
        {
            targetState =
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused;
        }
        if (!logicalCallAttempted && capture.OpaqueCallReturnedNormally.HasValue)
        {
            returnState =
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
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
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan parentPlan)
    {
        foreach (GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstruction instruction
            in parentPlan.Instructions.Reverse())
        {
            if (instruction.Reached)
            {
                return instruction.InstructionRawFileOffset;
            }
        }

        return parentPlan.LastReachedRawFileOffset;
    }

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
        BeforeAdmission(bool supplied) =>
        supplied
            ? GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.SuppliedButUnused
            : GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.NotReached;

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
        UnreachedOrUnused(bool supplied) => BeforeAdmission(supplied);
}
