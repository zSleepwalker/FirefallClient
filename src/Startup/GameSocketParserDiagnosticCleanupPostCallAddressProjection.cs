using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserDiagnosticCleanupPostCallAddressCapture(
    GameSocketParserDiagnosticCleanupVirtualCallReturnCapture ParentCapture,
    uint? FreshPostCallESI);

public enum GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserDiagnosticCleanupPostCallAddressOutcome
{
    ParentNotAdmitted,
    ParentNotCleanAt34D1,
    MissingFreshPostCallESIAt34D1,
    ProjectedLEAAt34D1
}

public enum GameSocketParserDiagnosticCleanupPostCallAddressInstructionKind
{
    LeaEAXFromESIPlusEightAt34D1
}

public readonly record struct GameSocketParserDiagnosticCleanupPostCallAddressInstruction(
    GameSocketParserDiagnosticCleanupPostCallAddressInstructionKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    string DestinationRegister,
    uint? DestinationValue,
    string SourceRegister,
    uint? SourceValue,
    uint Displacement,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public sealed class GameSocketParserDiagnosticCleanupPostCallAddressPlan
{
    private readonly System.Lazy<bool> _orderedSemanticPrefixPreserved;
    private readonly System.Lazy<bool> _exactCleanCompletion;

    internal GameSocketParserDiagnosticCleanupPostCallAddressPlan(
        GameSocketParserDiagnosticCleanupPostCallAddressCapture capture,
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan parentPlan,
        ImmutableArray<GameSocketParserDiagnosticCleanupPostCallAddressInstruction> instructions,
        GameSocketParserDiagnosticCleanupPostCallAddressCaptureState captureState,
        GameSocketParserDiagnosticCleanupPostCallAddressOutcome outcome,
        bool parentAdmitted,
        bool childBoundedCompletionBoundaryReached,
        uint? eaxValue,
        uint? esiValue,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint? lastReachedRawFileOffset,
        uint? stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        FreshPostCallESICaptureState = captureState;
        Outcome = outcome;
        ParentAdmitted = parentAdmitted;
        ChildBoundedCompletionBoundaryReached = childBoundedCompletionBoundaryReached;
        EAXValue = eaxValue;
        ESIValue = esiValue;
        FlagsAtStop = flagsAtStop;
        LastReachedRawFileOffset = lastReachedRawFileOffset;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
        _orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved);
        _exactCleanCompletion = new(ComputeExactCleanCompletion);
    }

    public GameSocketParserDiagnosticCleanupPostCallAddressCapture Capture { get; }
    public GameSocketParserDiagnosticCleanupVirtualCallReturnPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserDiagnosticCleanupPostCallAddressInstruction>
        Instructions { get; }
    public GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
        FreshPostCallESICaptureState { get; }
    public GameSocketParserDiagnosticCleanupPostCallAddressOutcome Outcome { get; }
    public bool ParentAdmitted { get; }
    public bool ParentExactCompletion => ParentPlan.ExactChildCompletion;
    public bool ParentExactBoundedCompletion => ParentPlan.ExactBoundedCompletion;
    public bool ParentExactCleanCompletion => ParentPlan.ExactCleanCompletion;
    public bool ChildBoundedCompletionBoundaryReached { get; }
    public bool ExactChildCompletion => ChildBoundedCompletionBoundaryReached;
    public bool ExactBoundedCompletion => ChildBoundedCompletionBoundaryReached;
    public bool BoundedCompletionBoundaryReached => ChildBoundedCompletionBoundaryReached;
    public uint? EAXValue { get; }
    public uint? ESIValue { get; }
    public uint? PostCallEAX => EAXValue;
    public uint? PostCallESI => ESIValue;
    public uint? PostCallECX => null;
    public uint? PostCallEBX => null;
    public uint? PostCallESP => null;
    public bool PostCallECXCaptured => false;
    public bool PostCallEBXCaptured => false;
    public bool PostCallESPCaptured => false;
    public bool PostCallRegistersCaptured =>
        PostCallEAX.HasValue && PostCallESI.HasValue
        && PostCallECX.HasValue && PostCallEBX.HasValue && PostCallESP.HasValue;
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
    public bool OrderedSemanticPrefixPreserved =>
        _orderedSemanticPrefixPreserved.Value;

    private bool ComputeOrderedSemanticPrefixPreserved() =>
        ParentPlan.OrderedSemanticPrefixPreserved
        && ReprojectedParentMatches()
        && Reads.SequenceEqual(ParentPlan.Reads)
        && MemoryEffects.SequenceEqual(ParentPlan.MemoryEffects);

    public bool ExactCleanCompletion => _exactCleanCompletion.Value;

    private bool ComputeExactCleanCompletion() =>
        ParentAdmitted
        && ParentPlan.ExactCleanCompletion
        && Outcome
            == GameSocketParserDiagnosticCleanupPostCallAddressOutcome.ProjectedLEAAt34D1
        && FreshPostCallESICaptureState
            == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.Captured
        && Instructions.Length == 1
        && Instructions[0] is {
            InstructionRawFileOffset:
                GameSocketParserDiagnosticCleanupPostCallAddressProjection
                    .LeaRawFileOffset,
            Attempted: true,
            Reached: true,
            Committed: true
        }
        && ESIValue == Capture.FreshPostCallESI
        && EAXValue == unchecked(Capture.FreshPostCallESI!.Value + 8u)
        && ChildBoundedCompletionBoundaryReached
        && StopBeforeRawFileOffset
            == GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .ExclusiveStopRawFileOffset
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && OrderedSemanticPrefixPreserved
        && !PostCallECXCaptured
        && !PostCallEBXCaptured
        && !PostCallESPCaptured
        && !NativeInstructionsExecuted
        && !NativeReadsExecuted
        && !MemoryWritesExecuted
        && !CallsExecuted
        && !LockedRmwExecuted
        && !RuntimeSampleExecuted
        && !ParentInstructionsReplayed;

    public bool NativeInstructionsExecuted => false;
    public bool NativeReadsExecuted => false;
    public bool MemoryWritesExecuted => false;
    public bool CallsExecuted => false;
    public bool LockedRmwExecuted => false;
    public bool RuntimeSampleExecuted => false;
    public bool ParentInstructionsReplayed => false;

    private bool ReprojectedParentMatches()
    {
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan reprojected =
            GameSocketParserDiagnosticCleanupVirtualCallReturnProjection.Project(
                Capture.ParentCapture);
        return reprojected.Capture == ParentPlan.Capture
            && reprojected.Outcome == ParentPlan.Outcome
            && reprojected.TargetCaptureState == ParentPlan.TargetCaptureState
            && reprojected.ReturnCaptureState == ParentPlan.ReturnCaptureState
            && reprojected.ParentCallBoundaryAdmitted
                == ParentPlan.ParentCallBoundaryAdmitted
            && reprojected.TargetReadAttempted == ParentPlan.TargetReadAttempted
            && reprojected.TargetReadCaptured == ParentPlan.TargetReadCaptured
            && reprojected.LogicalCallAttempted == ParentPlan.LogicalCallAttempted
            && reprojected.ReturnObservationCaptured
                == ParentPlan.ReturnObservationCaptured
            && reprojected.OpaqueCallReturnedNormally
                == ParentPlan.OpaqueCallReturnedNormally
            && reprojected.MemoryRead == ParentPlan.MemoryRead
            && reprojected.CallBoundary == ParentPlan.CallBoundary
            && reprojected.ContinuationBoundary == ParentPlan.ContinuationBoundary
            && reprojected.InstructionOrder.SequenceEqual(ParentPlan.InstructionOrder)
            && reprojected.FlagsAtStop == ParentPlan.FlagsAtStop
            && reprojected.LastReachedRawFileOffset
                == ParentPlan.LastReachedRawFileOffset
            && reprojected.StopBeforeRawFileOffset
                == ParentPlan.StopBeforeRawFileOffset
            && reprojected.ExactChildCompletion == ParentPlan.ExactChildCompletion
            && reprojected.ExactBoundedCompletion == ParentPlan.ExactBoundedCompletion
            && reprojected.ExactCleanCompletion == ParentPlan.ExactCleanCompletion
            && reprojected.OrderedSemanticPrefixPreserved
                == ParentPlan.OrderedSemanticPrefixPreserved;
    }
}

/// <summary>Projects only LEA EAX,[ESI+8] after a clean opaque-call return.</summary>
public static class GameSocketParserDiagnosticCleanupPostCallAddressProjection
{
    public const uint LeaRawFileOffset = 0x00BD34D1;
    public const uint ExclusiveStopRawFileOffset = 0x00BD34D4;
    public const uint FollowingLockedRmwRawFileOffset = 0x00BD34D4;
    public const string LeaEncoding = "8D 46 08";
    public const string FollowingLockedRmwEncoding = "F0 0F C1 18";
    public const string CoordinatorProvidedBytes =
        "8D 46 08 F0 0F C1 18 4B 75 07 8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D";
    public const bool BytesIndependentlyVerified = false;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8D, 0x46, 0x08, 0xF0, 0x0F, 0xC1, 0x18, 0x4B,
            0x75, 0x07, 0x8B, 0x06, 0x8B, 0xCE, 0xFF, 0x50,
            0x04, 0x8B, 0x4C, 0x24, 0x48, 0x64, 0x89, 0x0D);

    public static GameSocketParserDiagnosticCleanupPostCallAddressPlan Project(
        GameSocketParserDiagnosticCleanupPostCallAddressCapture capture)
    {
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan parentPlan =
            GameSocketParserDiagnosticCleanupVirtualCallReturnProjection.Project(
                capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserDiagnosticCleanupPostCallAddressInstruction>();
        bool parentAtLEA =
            parentPlan.ExactChildCompletion
            && parentPlan.ExactCleanCompletion
            && parentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .OpaqueCallReturnedNormally
            && parentPlan.ContinuationBoundary is {
                InstructionRawFileOffset: LeaRawFileOffset,
                Reached: true,
                NextInstructionExecuted: false
            }
            && parentPlan.StopBeforeRawFileOffset == LeaRawFileOffset;
        bool cleanParent = parentAtLEA
            && parentPlan.ParentCallBoundaryAdmitted
            && parentPlan.TargetReadCaptured
            && parentPlan.LogicalCallAttempted
            && parentPlan.ReturnObservationCaptured
            && parentPlan.OpaqueCallReturnedNormally == true;

        GameSocketParserDiagnosticCleanupPostCallAddressCaptureState captureState =
            capture.FreshPostCallESI.HasValue
                ? GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.SuppliedButUnused
                : GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.NotReached;

        if (!parentAtLEA)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                captureState,
                GameSocketParserDiagnosticCleanupPostCallAddressOutcome.ParentNotAdmitted,
                parentAdmitted: false,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: null,
                esiValue: null,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: parentPlan.LastReachedRawFileOffset,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        if (!cleanParent)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                captureState,
                GameSocketParserDiagnosticCleanupPostCallAddressOutcome
                    .ParentNotCleanAt34D1,
                parentAdmitted: false,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: null,
                esiValue: null,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: parentPlan.LastReachedRawFileOffset,
                stopBeforeRawFileOffset: parentPlan.StopBeforeRawFileOffset);
        }

        uint? freshESI = capture.FreshPostCallESI;
        var instruction = new GameSocketParserDiagnosticCleanupPostCallAddressInstruction(
            GameSocketParserDiagnosticCleanupPostCallAddressInstructionKind
                .LeaEAXFromESIPlusEightAt34D1,
            LeaRawFileOffset,
            ToCodeVA(LeaRawFileOffset),
            LeaEncoding,
            "EAX",
            freshESI.HasValue ? unchecked(freshESI.Value + 8u) : null,
            "ESI",
            freshESI,
            Displacement: 8,
            Attempted: freshESI.HasValue,
            Reached: true,
            Committed: freshESI.HasValue,
            FlagsBefore: parentPlan.FlagsAtStop,
            FlagsAfter: parentPlan.FlagsAtStop);
        instructions.Add(instruction);

        if (!freshESI.HasValue)
        {
            return new(
                capture,
                parentPlan,
                instructions.ToImmutable(),
                GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.Missing,
                GameSocketParserDiagnosticCleanupPostCallAddressOutcome
                    .MissingFreshPostCallESIAt34D1,
                parentAdmitted: true,
                childBoundedCompletionBoundaryReached: false,
                eaxValue: null,
                esiValue: null,
                flagsAtStop: parentPlan.FlagsAtStop,
                lastReachedRawFileOffset: LeaRawFileOffset,
                stopBeforeRawFileOffset: LeaRawFileOffset);
        }

        return new(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.Captured,
            GameSocketParserDiagnosticCleanupPostCallAddressOutcome.ProjectedLEAAt34D1,
            parentAdmitted: true,
            childBoundedCompletionBoundaryReached: true,
            eaxValue: unchecked(freshESI.Value + 8u),
            esiValue: freshESI.Value,
            flagsAtStop: parentPlan.FlagsAtStop,
            lastReachedRawFileOffset: LeaRawFileOffset,
            stopBeforeRawFileOffset: ExclusiveStopRawFileOffset);
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            rawFileOffset);
}
