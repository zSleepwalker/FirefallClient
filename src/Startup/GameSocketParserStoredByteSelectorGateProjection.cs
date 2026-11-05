using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserStoredByteSelectorGateCapture(
    GameSocketParserCountConsumptionStoresCapture ParentCapture);

public enum GameSocketParserStoredByteSelectorGateOutcome
{
    ParentHandoffNotAdmitted,
    MissingInheritedEBX,
    InvalidInheritedEBX,
    MissingIncomingCompareFlags,
    BoundedBeforeSelectedDestination
}

public enum GameSocketParserStoredByteSelectorGateInstructionKind
{
    JumpZeroFreshStackByte,
    CompareEBX2,
    JumpEqualSelector2,
    CompareEBX1,
    JumpEqualSelector1
}

public enum GameSocketParserStoredByteSelectorGateBranchState
{
    NotReached,
    NotConditional,
    Taken,
    NotTaken
}

public readonly record struct GameSocketParserStoredByteSelectorGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct GameSocketParserStoredByteSelectorGateInstruction(
    GameSocketParserStoredByteSelectorGateInstructionKind Kind,
    uint RawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    bool Attempted,
    bool Reached,
    GameSocketParserStoredByteSelectorGateBranchState BranchState,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter,
    uint? TakenDestinationRawFileOffset,
    uint? FallthroughDestinationRawFileOffset,
    uint? SelectedDestinationRawFileOffset);

public sealed class GameSocketParserStoredByteSelectorGatePlan
{
    internal GameSocketParserStoredByteSelectorGatePlan(
        GameSocketParserStoredByteSelectorGateCapture capture,
        GameSocketParserCountConsumptionStoresPlan parentPlan,
        ImmutableArray<GameSocketParserStoredByteSelectorGateInstruction> instructions,
        GameSocketParserStoredByteSelectorGateOutcome outcome,
        bool parentHandoffAdmitted,
        uint? inheritedEBX,
        GameSocketParserStoredByteSelectorGateFlags? incomingFlags,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        GameSocketParserStoredByteSelectorGateBranchState firstJZState,
        GameSocketParserStoredByteSelectorGateBranchState selector2JEState,
        GameSocketParserStoredByteSelectorGateBranchState selector1JEState,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Instructions = instructions;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        InheritedEBX = inheritedEBX;
        IncomingFlags = incomingFlags;
        FlagsAtStop = flagsAtStop;
        FirstJZState = firstJZState;
        Selector2JEState = selector2JEState;
        Selector1JEState = selector1JEState;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserStoredByteSelectorGateCapture Capture { get; }
    public GameSocketParserCountConsumptionStoresPlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserStoredByteSelectorGateInstruction> Instructions { get; }
    public ImmutableArray<GameSocketParserStoredByteSelectorGateInstruction> OrderedInstructions =>
        Instructions;
    public ImmutableArray<uint> AttemptedInstructionRawFileOffsets =>
        Instructions.Where(instruction => instruction.Attempted)
            .Select(instruction => instruction.RawFileOffset)
            .ToImmutableArray();
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Instructions.Where(instruction => instruction.Reached)
            .Select(instruction => instruction.RawFileOffset)
            .ToImmutableArray();
    public GameSocketParserStoredByteSelectorGateOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? InheritedEBX { get; }
    public uint? ResultEBX => InheritedEBX;
    public bool EBXUnchanged => ResultEBX == InheritedEBX;
    public bool ParentInstructionsReplayed => false;
    public GameSocketParserStoredByteSelectorGateFlags? IncomingFlags { get; }
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public GameSocketParserStoredByteSelectorGateBranchState FirstJZState { get; }
    public GameSocketParserStoredByteSelectorGateBranchState Selector2JEState { get; }
    public GameSocketParserStoredByteSelectorGateBranchState Selector1JEState { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public uint? SelectedDestinationRawFileOffset =>
        Outcome == GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination
            ? StopBeforeRawFileOffset
            : null;
    public uint? SelectedDestinationCodeVirtualAddress =>
        SelectedDestinationRawFileOffset is uint rawFileOffset
            ? GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                rawFileOffset)
            : null;
    public bool DestinationEntered => false;
    public bool BranchTargetEntered => false;
    public bool FallthroughDestinationEntered => false;
    public bool FirstPushReached => false;
    public bool OpaqueEffectsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
    public bool ChildReadsEmpty => true;
    public bool ChildStoresEmpty => true;
    public ImmutableArray<GameSocketParserCountConsumptionStoresRead> Reads =>
        ImmutableArray<GameSocketParserCountConsumptionStoresRead>.Empty;
    public ImmutableArray<GameSocketParserCountConsumptionStoresStore> Stores =>
        ImmutableArray<GameSocketParserCountConsumptionStoresStore>.Empty;
    public ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> MemoryEffects =>
        ParentPlan.OrderedMemoryEffects;
    public ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> OrderedMemoryEffects =>
        ParentPlan.OrderedMemoryEffects;
    public bool OrderedMemoryPrefixPreserved =>
        OrderedMemoryEffects.SequenceEqual(ParentPlan.OrderedMemoryEffects);
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination
        && (StopBeforeRawFileOffset == GameSocketParserStoredByteSelectorGateProjection
                .SelectedDestinationRawFileOffset
            || StopBeforeRawFileOffset == GameSocketParserStoredByteSelectorGateProjection
                .FirstPushRawFileOffset)
        || (!ParentHandoffAdmitted
            && Outcome
                == GameSocketParserStoredByteSelectorGateOutcome.ParentHandoffNotAdmitted
            && ParentPlan.ReachedBoundedEndpoint
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentHandoffAdmitted
            && Outcome
                == GameSocketParserStoredByteSelectorGateOutcome.ParentHandoffNotAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion && ParentPlan.IsCleanCompletion;
}

/// <summary>
/// Reprojects the preceding count-consumption endpoint and models only the ordered
/// byte-driven JZ/CMP/JE selector gates, stopping before either selected destination.
/// </summary>
public static class GameSocketParserStoredByteSelectorGateProjection
{
    public const uint FirstJZRawFileOffset = 0x00BD325E;
    public const uint CompareEBX2RawFileOffset = 0x00BD3260;
    public const uint Selector2JERawFileOffset = 0x00BD3263;
    public const uint CompareEBX1RawFileOffset = 0x00BD3265;
    public const uint Selector1JERawFileOffset = 0x00BD3268;
    public const uint FirstPushRawFileOffset = 0x00BD326A;
    public const uint SelectedDestinationRawFileOffset = 0x00BD3294;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "74 34 83 FB 02 74 2F 83 FB 01 74 2A";

    public static GameSocketParserStoredByteSelectorGatePlan Project(
        GameSocketParserStoredByteSelectorGateCapture capture)
    {
        GameSocketParserCountConsumptionStoresPlan parentPlan =
            GameSocketParserCountConsumptionStoresProjection.Project(capture.ParentCapture);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketParserStoredByteSelectorGateInstruction>();
        uint? inheritedEBX =
            parentPlan.ParentPlan.ParentPlan.ParentPlan.ResultEBX;
        GameSocketParserStoredByteSelectorGateFlags? incomingFlags =
            ToFlags(parentPlan.FlagsAfterCompareStackByte);
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop =
            ToFlags(parentPlan.FlagsAtStop);
        var firstJZState = GameSocketParserStoredByteSelectorGateBranchState.NotReached;
        var selector2JEState = GameSocketParserStoredByteSelectorGateBranchState.NotReached;
        var selector1JEState = GameSocketParserStoredByteSelectorGateBranchState.NotReached;

        bool parentHandoffAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserCountConsumptionStoresOutcome.BoundedBeforeJZ
            && parentPlan.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
            && parentPlan.StackByteCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Captured;

        GameSocketParserStoredByteSelectorGateOutcome outcome;
        uint stopBefore;
        if (!parentHandoffAdmitted)
        {
            outcome =
                GameSocketParserStoredByteSelectorGateOutcome.ParentHandoffNotAdmitted;
            stopBefore = parentPlan.StopBeforeRawFileOffset;
        }
        else if (inheritedEBX is not uint selector)
        {
            outcome = GameSocketParserStoredByteSelectorGateOutcome.MissingInheritedEBX;
            stopBefore = FirstJZRawFileOffset;
        }
        else if (selector > 3)
        {
            outcome = GameSocketParserStoredByteSelectorGateOutcome.InvalidInheritedEBX;
            stopBefore = FirstJZRawFileOffset;
        }
        else if (incomingFlags is not GameSocketParserStoredByteSelectorGateFlags firstFlags
            || firstFlags.Zero is not bool zeroAtFirstJZ)
        {
            outcome =
                GameSocketParserStoredByteSelectorGateOutcome.MissingIncomingCompareFlags;
            stopBefore = FirstJZRawFileOffset;
        }
        else
        {
            AddBranch(
                GameSocketParserStoredByteSelectorGateInstructionKind.JumpZeroFreshStackByte,
                FirstJZRawFileOffset,
                "74 34",
                firstFlags,
                firstFlags,
                zeroAtFirstJZ,
                SelectedDestinationRawFileOffset,
                CompareEBX2RawFileOffset);
            firstJZState = StateFor(zeroAtFirstJZ);

            if (zeroAtFirstJZ)
            {
                outcome =
                    GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination;
                flagsAtStop = firstFlags;
                stopBefore = SelectedDestinationRawFileOffset;
            }
            else
            {
                GameSocketParserStoredByteSelectorGateFlags compare2Flags =
                    CompareFlags32(selector, 2);
                AddInstruction(
                    GameSocketParserStoredByteSelectorGateInstructionKind.CompareEBX2,
                    CompareEBX2RawFileOffset,
                    "83 FB 02",
                    GameSocketParserStoredByteSelectorGateBranchState.NotConditional,
                    firstFlags,
                    compare2Flags);
                flagsAtStop = compare2Flags;

                bool equals2 = compare2Flags.Zero == true;
                AddBranch(
                    GameSocketParserStoredByteSelectorGateInstructionKind.JumpEqualSelector2,
                    Selector2JERawFileOffset,
                    "74 2F",
                    compare2Flags,
                    compare2Flags,
                    equals2,
                    SelectedDestinationRawFileOffset,
                    CompareEBX1RawFileOffset);
                selector2JEState = StateFor(equals2);

                if (equals2)
                {
                    outcome =
                        GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination;
                    stopBefore = SelectedDestinationRawFileOffset;
                }
                else
                {
                    GameSocketParserStoredByteSelectorGateFlags compare1Flags =
                        CompareFlags32(selector, 1);
                    AddInstruction(
                        GameSocketParserStoredByteSelectorGateInstructionKind.CompareEBX1,
                        CompareEBX1RawFileOffset,
                        "83 FB 01",
                        GameSocketParserStoredByteSelectorGateBranchState.NotConditional,
                        compare2Flags,
                        compare1Flags);
                    flagsAtStop = compare1Flags;

                    bool equals1 = compare1Flags.Zero == true;
                    AddBranch(
                        GameSocketParserStoredByteSelectorGateInstructionKind.JumpEqualSelector1,
                        Selector1JERawFileOffset,
                        "74 2A",
                        compare1Flags,
                        compare1Flags,
                        equals1,
                        SelectedDestinationRawFileOffset,
                        FirstPushRawFileOffset);
                    selector1JEState = StateFor(equals1);
                    outcome =
                        GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination;
                    stopBefore = equals1
                        ? SelectedDestinationRawFileOffset
                        : FirstPushRawFileOffset;
                }
            }
        }

        return new GameSocketParserStoredByteSelectorGatePlan(
            capture,
            parentPlan,
            instructions.ToImmutable(),
            outcome,
            parentHandoffAdmitted,
            inheritedEBX,
            incomingFlags,
            flagsAtStop,
            firstJZState,
            selector2JEState,
            selector1JEState,
            stopBefore);

        void AddBranch(
            GameSocketParserStoredByteSelectorGateInstructionKind kind,
            uint rawFileOffset,
            string encoding,
            GameSocketParserStoredByteSelectorGateFlags flagsBefore,
            GameSocketParserStoredByteSelectorGateFlags flagsAfter,
            bool branchTaken,
            uint takenDestination,
            uint fallthroughDestination)
        {
            instructions.Add(new(
                kind,
                rawFileOffset,
                GameSocketParserCountConsumptionStoresProjection
                    .RawFileOffsetToCodeVirtualAddress(rawFileOffset),
                encoding,
                true,
                true,
                StateFor(branchTaken),
                flagsBefore,
                flagsAfter,
                takenDestination,
                fallthroughDestination,
                branchTaken ? takenDestination : fallthroughDestination));
        }

        void AddInstruction(
            GameSocketParserStoredByteSelectorGateInstructionKind kind,
            uint rawFileOffset,
            string encoding,
            GameSocketParserStoredByteSelectorGateBranchState branchState,
            GameSocketParserStoredByteSelectorGateFlags flagsBefore,
            GameSocketParserStoredByteSelectorGateFlags flagsAfter)
        {
            instructions.Add(new(
                kind,
                rawFileOffset,
                GameSocketParserCountConsumptionStoresProjection
                    .RawFileOffsetToCodeVirtualAddress(rawFileOffset),
                encoding,
                true,
                true,
                branchState,
                flagsBefore,
                flagsAfter,
                TakenDestinationRawFileOffset: null,
                FallthroughDestinationRawFileOffset: null,
                SelectedDestinationRawFileOffset: null));
        }
    }

    private static GameSocketParserStoredByteSelectorGateBranchState StateFor(bool branchTaken) =>
        branchTaken
            ? GameSocketParserStoredByteSelectorGateBranchState.Taken
            : GameSocketParserStoredByteSelectorGateBranchState.NotTaken;

    private static GameSocketParserStoredByteSelectorGateFlags? ToFlags(
        GameSocketParserCountConsumptionStoresFlags? flags) =>
        flags is GameSocketParserCountConsumptionStoresFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserStoredByteSelectorGateFlags CompareFlags32(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;
}
