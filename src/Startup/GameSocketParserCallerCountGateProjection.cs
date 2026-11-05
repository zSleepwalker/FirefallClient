using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserCallerCountGateCapture(
    GameSocketParserSecondByteScalarGateCapture ParentCapture,
    uint? FreshCallerEBP,
    uint? FreshCallerCount);

public enum GameSocketParserCallerCountGateCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserCallerCountGateCaptureKind
{
    FreshCallerEBP,
    FreshCallerCount
}

public enum GameSocketParserCallerCountGateOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshCallerEBP,
    MissingFreshCallerCount,
    BoundedBeforeEitherGateDestination
}

public enum GameSocketParserCallerCountGateReadKind
{
    FreshCallerCount
}

public enum GameSocketParserCallerCountGateMemoryEffectKind
{
    ParentFreshLocalVtableRead,
    ParentFreshSlotCIndirectTargetRead,
    ParentStackPush,
    ParentFreshField38DwordRead,
    ParentFreshFieldPlus4DwordRead,
    ParentStackStore,
    ParentFreshHeaderByteRead,
    ParentFreshSecondHeaderByteRead,
    ParentStackScalarStore,
    FreshCallerCountDwordRead
}

public readonly record struct GameSocketParserCallerCountGateRead(
    GameSocketParserCallerCountGateReadKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserCallerCountGateMemoryEffect(
    GameSocketParserCallerCountGateMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserCallerCountGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed class GameSocketParserCallerCountGatePlan
{
    internal GameSocketParserCallerCountGatePlan(
        GameSocketParserCallerCountGateCapture capture,
        GameSocketParserSecondByteScalarGatePlan parentPlan,
        GameSocketParserCallerCountGateCaptureState callerEBPCaptureState,
        GameSocketParserCallerCountGateCaptureState callerCountCaptureState,
        ImmutableArray<GameSocketParserCallerCountGateCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserCallerCountGateRead> reads,
        ImmutableArray<GameSocketParserCallerCountGateMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserCallerCountGateOutcome outcome,
        bool parentHandoffAdmitted,
        uint? callerCountReadAddress,
        uint? resultEDI,
        uint? resultEAX,
        uint? comparedEDI,
        uint? comparedEAX,
        GameSocketParserCallerCountGateFlags? flagsAfterMoveZXEDIAX,
        GameSocketParserCallerCountGateFlags? flagsAfterMoveEAXFromCaller,
        GameSocketParserCallerCountGateFlags? flagsAtStop,
        bool? branchTaken,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        CallerEBPCaptureState = callerEBPCaptureState;
        CallerCountCaptureState = callerCountCaptureState;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        CallerCountReadAddress = callerCountReadAddress;
        ResultEDI = resultEDI;
        ResultEAX = resultEAX;
        ComparedEDI = comparedEDI;
        ComparedEAX = comparedEAX;
        FlagsAfterMoveZXEDIAX = flagsAfterMoveZXEDIAX;
        FlagsAfterMoveEAXFromCaller = flagsAfterMoveEAXFromCaller;
        FlagsAtStop = flagsAtStop;
        BranchTaken = branchTaken;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserCallerCountGateCapture Capture { get; }
    public GameSocketParserSecondByteScalarGatePlan ParentPlan { get; }
    public GameSocketParserCallerCountGateCaptureState CallerEBPCaptureState { get; }
    public GameSocketParserCallerCountGateCaptureState CallerCountCaptureState { get; }
    public ImmutableArray<GameSocketParserCallerCountGateCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserCallerCountGateRead> Reads { get; }
    public ImmutableArray<GameSocketParserCallerCountGateMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserCallerCountGateMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserCallerCountGateOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? InheritedEDX => ParentPlan.ParentResultEDX;
    public uint? InheritedESP => ParentPlan.InheritedFreshPostResizeESP;
    public uint? InheritedFreshPostResizeESP => ParentPlan.InheritedFreshPostResizeESP;
    public uint? CallerCountReadAddress { get; }
    public uint? ResultEDI { get; }
    public uint? ResultEAX { get; }
    public uint? ComparedEDI { get; }
    public uint? ComparedEAX { get; }
    public GameSocketParserCallerCountGateFlags? FlagsAfterMoveZXEDIAX { get; }
    public GameSocketParserCallerCountGateFlags? FlagsAfterMoveEAXFromCaller { get; }
    public GameSocketParserCallerCountGateFlags? FlagsAtStop { get; }
    public bool? BranchTaken { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCallerCountGateProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ChildGateReachedEndpoint =>
        Outcome == GameSocketParserCallerCountGateOutcome.BoundedBeforeEitherGateDestination
        && BranchTaken is bool branchTaken
        && StopBeforeRawFileOffset
            == (branchTaken
                ? GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
                : GameSocketParserCallerCountGateProjection.FallthroughDestinationRawFileOffset);
    public bool ReachedBoundedEndpoint =>
        ChildGateReachedEndpoint
        || (!ParentHandoffAdmitted && ParentPlan.ReachedBoundedEndpoint);
    public bool ExactBoundedCompletion =>
        ChildGateReachedEndpoint
        || (!ParentHandoffAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion && ParentPlan.IsCleanCompletion && UnusedCaptures.IsEmpty;
    public bool FreshCallerCountReadAttempted =>
        Reads.Length != 0 && Reads[0].Attempted;
    public bool BranchTargetEntered => false;
    public bool FallthroughDestinationEntered => false;
    public bool TargetExecutionModeled => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
    public bool ChildStoresEmpty =>
        !MemoryEffects.Any(effect =>
            effect.InstructionRawFileOffset >= GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset
            && effect.Committed);
}

/// <summary>
/// Reprojects the preceding scalar gate and the fresh caller-count DWORD read,
/// then stops at the unsigned 32-bit comparison's selected destination.
/// </summary>
public static class GameSocketParserCallerCountGateProjection
{
    public const uint MoveZXEDIAXRawFileOffset = 0x00BD323B;
    public const uint FreshCallerCountLoadRawFileOffset = 0x00BD323E;
    public const uint CompareEDIEAX32RawFileOffset = 0x00BD3241;
    public const uint UnsignedAboveBranchRawFileOffset = 0x00BD3243;
    public const uint FallthroughDestinationRawFileOffset = 0x00BD3249;
    public const uint BranchTargetRawFileOffset = 0x00BD3488;

    public const int FreshCallerCountDisplacementFromEBP = 0x0C;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "0F B7 F8 8B 45 0C 3B F8 0F 87 3F 02 00 00";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserCallerCountGatePlan Project(
        GameSocketParserCallerCountGateCapture capture)
    {
        GameSocketParserSecondByteScalarGatePlan parentPlan =
            GameSocketParserSecondByteScalarGateProjection.Project(capture.ParentCapture);
        var reads = ImmutableArray.CreateBuilder<GameSocketParserCallerCountGateRead>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserCallerCountGateMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();

        foreach (GameSocketParserSecondByteScalarGateMemoryEffect parentEffect
            in parentPlan.OrderedMemoryEffects)
        {
            effects.Add(new GameSocketParserCallerCountGateMemoryEffect(
                Kind: MapParentEffect(parentEffect.Kind),
                InstructionRawFileOffset: parentEffect.InstructionRawFileOffset,
                Address: parentEffect.Address,
                Value: parentEffect.Value,
                Attempted: parentEffect.Attempted,
                Captured: parentEffect.Captured,
                Committed: parentEffect.Committed));
        }

        bool parentHandoffAdmitted = parentPlan.IsCleanCompletion
            && parentPlan.ReachedBoundedEndpoint
            && parentPlan.BranchTaken == false
            && parentPlan.ExactBoundedCompletion
            && parentPlan.StopBeforeRawFileOffset == MoveZXEDIAXRawFileOffset
            && parentPlan.CombinedScalar is ushort combinedScalar
            && parentPlan.ResultEAX is uint parentEAX
            && parentEAX == combinedScalar;
        GameSocketParserCallerCountGateCaptureState ebpState;
        GameSocketParserCallerCountGateCaptureState countState;
        GameSocketParserCallerCountGateOutcome outcome;
        uint? countAddress = null;
        uint? resultEDI = null;
        uint? resultEAX = null;
        uint? comparedEDI = null;
        uint? comparedEAX = null;
        GameSocketParserCallerCountGateFlags? flagsAfterMoveZXEDIAX = null;
        GameSocketParserCallerCountGateFlags? flagsAfterMoveEAXFromCaller = null;
        GameSocketParserCallerCountGateFlags? flagsAtStop;
        bool? branchTaken = null;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome = GameSocketParserCallerCountGateOutcome.ParentHandoffNotAdmitted;
            ebpState = capture.FreshCallerEBP.HasValue
                ? GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
                : GameSocketParserCallerCountGateCaptureState.NotReached;
            countState = capture.FreshCallerCount.HasValue
                ? GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
                : GameSocketParserCallerCountGateCaptureState.NotReached;
            flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
            stopBefore = parentPlan.StopBeforeRawFileOffset;
        }
        else
        {
            reached.Add(MoveZXEDIAXRawFileOffset);
            resultEDI = parentPlan.CombinedScalar!.Value;
            flagsAfterMoveZXEDIAX = ToFlags(parentPlan.FlagsAtStop);
            flagsAtStop = flagsAfterMoveZXEDIAX;

            if (capture.FreshCallerEBP is not uint callerEBP)
            {
                reached.Add(FreshCallerCountLoadRawFileOffset);
                ebpState = GameSocketParserCallerCountGateCaptureState.Missing;
                countState = capture.FreshCallerCount.HasValue
                    ? GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
                    : GameSocketParserCallerCountGateCaptureState.NotReached;
                reads.Add(new GameSocketParserCallerCountGateRead(
                    Kind: GameSocketParserCallerCountGateReadKind.FreshCallerCount,
                    InstructionRawFileOffset: FreshCallerCountLoadRawFileOffset,
                    Address: null,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                effects.Add(new GameSocketParserCallerCountGateMemoryEffect(
                    Kind: GameSocketParserCallerCountGateMemoryEffectKind.FreshCallerCountDwordRead,
                    InstructionRawFileOffset: FreshCallerCountLoadRawFileOffset,
                    Address: null,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                outcome = GameSocketParserCallerCountGateOutcome.MissingFreshCallerEBP;
                stopBefore = FreshCallerCountLoadRawFileOffset;
            }
            else
            {
                ebpState = GameSocketParserCallerCountGateCaptureState.Captured;
                uint address = unchecked(callerEBP + (uint)FreshCallerCountDisplacementFromEBP);
                countAddress = address;
                reached.Add(FreshCallerCountLoadRawFileOffset);
                bool countCaptured = capture.FreshCallerCount is uint;
                uint? countValue = capture.FreshCallerCount;
                reads.Add(new GameSocketParserCallerCountGateRead(
                    Kind: GameSocketParserCallerCountGateReadKind.FreshCallerCount,
                    InstructionRawFileOffset: FreshCallerCountLoadRawFileOffset,
                    Address: address,
                    Value: countValue,
                    Attempted: true,
                    Captured: countCaptured,
                    Committed: false));
                effects.Add(new GameSocketParserCallerCountGateMemoryEffect(
                    Kind: GameSocketParserCallerCountGateMemoryEffectKind.FreshCallerCountDwordRead,
                    InstructionRawFileOffset: FreshCallerCountLoadRawFileOffset,
                    Address: address,
                    Value: countValue,
                    Attempted: true,
                    Captured: countCaptured,
                    Committed: false));

                if (!countCaptured)
                {
                    countState = GameSocketParserCallerCountGateCaptureState.Missing;
                    flagsAtStop = flagsAfterMoveZXEDIAX;
                    outcome = GameSocketParserCallerCountGateOutcome.MissingFreshCallerCount;
                    stopBefore = FreshCallerCountLoadRawFileOffset;
                }
                else
                {
                    countState = GameSocketParserCallerCountGateCaptureState.Captured;
                    uint scalarValue = resultEDI.Value;
                    uint capturedCount = countValue.Value;
                    resultEAX = capturedCount;
                    flagsAfterMoveEAXFromCaller = flagsAfterMoveZXEDIAX;
                    reached.Add(CompareEDIEAX32RawFileOffset);
                    comparedEDI = scalarValue;
                    comparedEAX = capturedCount;
                    flagsAtStop = CompareFlags32(scalarValue, capturedCount);
                    reached.Add(UnsignedAboveBranchRawFileOffset);
                    bool isAbove = scalarValue > capturedCount;
                    branchTaken = isAbove;
                    outcome =
                        GameSocketParserCallerCountGateOutcome.BoundedBeforeEitherGateDestination;
                    stopBefore = isAbove
                        ? BranchTargetRawFileOffset
                        : FallthroughDestinationRawFileOffset;
                }
            }
        }

        var unused = ImmutableArray.CreateBuilder<GameSocketParserCallerCountGateCaptureKind>();
        if (ebpState == GameSocketParserCallerCountGateCaptureState.SuppliedButUnused)
        {
            unused.Add(GameSocketParserCallerCountGateCaptureKind.FreshCallerEBP);
        }

        if (countState == GameSocketParserCallerCountGateCaptureState.SuppliedButUnused)
        {
            unused.Add(GameSocketParserCallerCountGateCaptureKind.FreshCallerCount);
        }

        return new GameSocketParserCallerCountGatePlan(
            capture,
            parentPlan,
            ebpState,
            countState,
            unused.ToImmutable(),
            reads.ToImmutable(),
            effects.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            parentHandoffAdmitted,
            countAddress,
            resultEDI,
            resultEAX,
            comparedEDI,
            comparedEAX,
            flagsAfterMoveZXEDIAX,
            flagsAfterMoveEAXFromCaller,
            flagsAtStop,
            branchTaken,
            stopBefore);
    }

    private static GameSocketParserCallerCountGateMemoryEffectKind MapParentEffect(
        GameSocketParserSecondByteScalarGateMemoryEffectKind kind) =>
        kind switch
        {
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshLocalVtableRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshLocalVtableRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshSlotCIndirectTargetRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentStackPush =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentStackPush,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshField38DwordRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshField38DwordRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshFieldPlus4DwordRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshFieldPlus4DwordRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentStackStore =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentStackStore,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshHeaderByteRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshHeaderByteRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.FreshSecondHeaderByteRead =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshSecondHeaderByteRead,
            GameSocketParserSecondByteScalarGateMemoryEffectKind.StackStore =>
                GameSocketParserCallerCountGateMemoryEffectKind.ParentStackScalarStore,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    private static GameSocketParserCallerCountGateFlags? ToFlags(
        GameSocketParserSecondByteScalarGateFlags? flags) =>
        flags is GameSocketParserSecondByteScalarGateFlags value
            ? new GameSocketParserCallerCountGateFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserCallerCountGateFlags CompareFlags32(
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
