using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserSecondByteScalarGateCapture(
    GameSocketParserFirstByteFieldsCapture ParentCapture,
    byte? FreshSecondHeaderByte);

public enum GameSocketParserSecondByteScalarGateCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserSecondByteScalarGateCaptureKind
{
    FreshSecondHeaderByte
}

public enum GameSocketParserSecondByteScalarGateOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshSecondHeaderByte,
    BoundedBeforeEitherGateDestination
}

public enum GameSocketParserSecondByteScalarGateReadKind
{
    FreshSecondHeaderByte
}

public enum GameSocketParserSecondByteScalarGateMemoryEffectKind
{
    ParentFreshLocalVtableRead,
    ParentFreshSlotCIndirectTargetRead,
    ParentStackPush,
    ParentFreshField38DwordRead,
    ParentFreshFieldPlus4DwordRead,
    ParentStackStore,
    ParentFreshHeaderByteRead,
    FreshSecondHeaderByteRead,
    StackStore
}

public enum GameSocketParserSecondByteScalarGateStoreKind
{
    CombinedScalar
}

public readonly record struct GameSocketParserSecondByteScalarGateRead(
    GameSocketParserSecondByteScalarGateReadKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    byte? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserSecondByteScalarGateStore(
    GameSocketParserSecondByteScalarGateStoreKind Kind,
    uint InstructionRawFileOffset,
    uint StackAddress,
    uint Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserSecondByteScalarGateMemoryEffect(
    GameSocketParserSecondByteScalarGateMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserSecondByteScalarGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed class GameSocketParserSecondByteScalarGatePlan
{
    internal GameSocketParserSecondByteScalarGatePlan(
        GameSocketParserSecondByteScalarGateCapture capture,
        GameSocketParserFirstByteFieldsPlan parentPlan,
        GameSocketParserSecondByteScalarGateCaptureState captureState,
        ImmutableArray<GameSocketParserSecondByteScalarGateCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserSecondByteScalarGateRead> reads,
        ImmutableArray<GameSocketParserSecondByteScalarGateStore> stores,
        ImmutableArray<GameSocketParserSecondByteScalarGateMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserSecondByteScalarGateOutcome outcome,
        bool parentHandoffAdmitted,
        uint? secondHeaderByteReadAddress,
        uint? stackStoreAddress,
        ushort? combinedScalar,
        uint? resultEAX,
        uint? resultECX,
        ushort? comparedCX,
        ushort? comparedDI,
        GameSocketParserSecondByteScalarGateFlags? flagsAfterShiftCX8,
        GameSocketParserSecondByteScalarGateFlags? flagsAfterOrCXAX,
        GameSocketParserSecondByteScalarGateFlags? flagsAtStop,
        bool? branchTaken,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        CaptureState = captureState;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        Stores = stores;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        SecondHeaderByteReadAddress = secondHeaderByteReadAddress;
        StackStoreAddress = stackStoreAddress;
        CombinedScalar = combinedScalar;
        ResultEAX = resultEAX;
        ResultECX = resultECX;
        ComparedCX = comparedCX;
        ComparedDI = comparedDI;
        FlagsAfterShiftCX8 = flagsAfterShiftCX8;
        FlagsAfterOrCXAX = flagsAfterOrCXAX;
        FlagsAtStop = flagsAtStop;
        BranchTaken = branchTaken;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserSecondByteScalarGateCapture Capture { get; }
    public GameSocketParserFirstByteFieldsPlan ParentPlan { get; }
    public GameSocketParserSecondByteScalarGateCaptureState CaptureState { get; }
    public ImmutableArray<GameSocketParserSecondByteScalarGateCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserSecondByteScalarGateRead> Reads { get; }
    public ImmutableArray<GameSocketParserSecondByteScalarGateStore> Stores { get; }
    public ImmutableArray<GameSocketParserSecondByteScalarGateMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserSecondByteScalarGateMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserSecondByteScalarGateOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? ParentResultEDX => ParentPlan.ParentResultEDX;
    public uint? InheritedFreshPostResizeESP => ParentPlan.InheritedFreshPostResizeESP;
    public uint? InheritedDI => ParentPlan.ParentPlan.ResultEDI;
    public uint? SecondHeaderByteReadAddress { get; }
    public uint? StackStoreAddress { get; }
    public ushort? CombinedScalar { get; }
    public uint? ResultEAX { get; }
    public uint? ResultECX { get; }
    public ushort? ComparedCX { get; }
    public ushort? ComparedDI { get; }
    public GameSocketParserSecondByteScalarGateFlags? FlagsAfterShiftCX8 { get; }
    public GameSocketParserSecondByteScalarGateFlags? FlagsAfterOrCXAX { get; }
    public GameSocketParserSecondByteScalarGateFlags? FlagsAtStop { get; }
    public bool? BranchTaken { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserSecondByteScalarGateProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserSecondByteScalarGateOutcome.BoundedBeforeEitherGateDestination
        && BranchTaken is bool branchTaken
        && StopBeforeRawFileOffset
            == (branchTaken
                ? GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset
                : GameSocketParserSecondByteScalarGateProjection.FallthroughDestinationRawFileOffset);
    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentHandoffAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion && ParentPlan.IsCleanCompletion && UnusedCaptures.IsEmpty;
    public bool SecondHeaderByteReadAttempted => Reads.Length != 0 && Reads[0].Attempted;
    public bool BranchTargetEntered => false;
    public bool FallthroughDestinationEntered => false;
    public bool TargetExecutionModeled => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
}

/// <summary>
/// Reprojects the preceding first-byte scalar window, then records the independent
/// second-byte read, 11-bit scalar construction, and 16-bit gate without entering either
/// branch destination or modeling parser semantics.
/// </summary>
public static class GameSocketParserSecondByteScalarGateProjection
{
    public const uint SecondHeaderByteLoadRawFileOffset = 0x00BD321D;
    public const uint ShiftCX8RawFileOffset = 0x00BD3221;
    public const uint OrCXAXRawFileOffset = 0x00BD3225;
    public const uint MoveZXEAXFromCXRawFileOffset = 0x00BD3228;
    public const uint CombinedScalarStackStoreRawFileOffset = 0x00BD322B;
    public const uint LeaECXMinusTwoRawFileOffset = 0x00BD322F;
    public const uint CompareCXDI16RawFileOffset = 0x00BD3232;
    public const uint UnsignedAboveBranchRawFileOffset = 0x00BD3235;
    public const uint FallthroughDestinationRawFileOffset = 0x00BD323B;
    public const uint BranchTargetRawFileOffset = 0x00BD3488;

    public const int CombinedScalarStackStoreDisplacement = 0x18;
    public const ushort InheritedDIValue = 0x056E;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "0F B6 42 01 66 C1 E1 08 66 0B C8 0F B7 C1 "
        + "89 44 24 18 8D 48 FE 66 3B CF 0F 87 4D 02 00 00";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserSecondByteScalarGatePlan Project(
        GameSocketParserSecondByteScalarGateCapture capture)
    {
        GameSocketParserFirstByteFieldsPlan parentPlan =
            GameSocketParserFirstByteFieldsProjection.Project(capture.ParentCapture);
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserSecondByteScalarGateRead>();
        var stores =
            ImmutableArray.CreateBuilder<GameSocketParserSecondByteScalarGateStore>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserSecondByteScalarGateMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();

        foreach (GameSocketParserFirstByteFieldsMemoryEffect parentEffect
            in parentPlan.MemoryEffects)
        {
            effects.Add(new GameSocketParserSecondByteScalarGateMemoryEffect(
                Kind: parentEffect.Kind switch
                {
                    GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshLocalVtableRead =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshLocalVtableRead,
                    GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshSlotCIndirectTargetRead =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
                    GameSocketParserFirstByteFieldsMemoryEffectKind.ParentStackPush =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentStackPush,
                    GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshField38DwordRead =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshField38DwordRead,
                    GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshFieldPlus4DwordRead =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshFieldPlus4DwordRead,
                    GameSocketParserFirstByteFieldsMemoryEffectKind.FreshHeaderByteRead =>
                        GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentFreshHeaderByteRead,
                    _ => GameSocketParserSecondByteScalarGateMemoryEffectKind.ParentStackStore
                },
                InstructionRawFileOffset: parentEffect.InstructionRawFileOffset,
                Address: parentEffect.Address,
                Value: parentEffect.Value,
                Attempted: parentEffect.Attempted,
                Captured: parentEffect.Captured,
                Committed: parentEffect.Committed));
        }

        bool parentHandoffAdmitted = parentPlan.IsCleanCompletion
            && parentPlan.ReachedBoundedEndpoint
            && parentPlan.ParentResultEDX.HasValue
            && parentPlan.InheritedFreshPostResizeESP.HasValue
            && parentPlan.ResultECX.HasValue
            && parentPlan.ParentPlan.ResultEDI == InheritedDIValue;
        GameSocketParserSecondByteScalarGateCaptureState captureState;
        GameSocketParserSecondByteScalarGateOutcome outcome;
        uint? secondByteAddress = null;
        uint? stackStoreAddress = null;
        ushort? combinedScalar = null;
        uint? resultEAX = null;
        uint? resultECX = null;
        ushort? comparedCX = null;
        ushort? comparedDI = null;
        GameSocketParserSecondByteScalarGateFlags? flagsAfterShiftCX8 = null;
        GameSocketParserSecondByteScalarGateFlags? flagsAfterOrCXAX = null;
        GameSocketParserSecondByteScalarGateFlags? flagsAtStop;
        bool? branchTaken = null;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome =
                GameSocketParserSecondByteScalarGateOutcome.ParentHandoffNotAdmitted;
            captureState = capture.FreshSecondHeaderByte.HasValue
                ? GameSocketParserSecondByteScalarGateCaptureState.SuppliedButUnused
                : GameSocketParserSecondByteScalarGateCaptureState.NotReached;
            flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
            stopBefore = parentPlan.StopBeforeRawFileOffset;
        }
        else
        {
            uint address = unchecked(parentPlan.ParentResultEDX!.Value + 1);
            secondByteAddress = address;
            reached.Add(SecondHeaderByteLoadRawFileOffset);
            reads.Add(new GameSocketParserSecondByteScalarGateRead(
                Kind: GameSocketParserSecondByteScalarGateReadKind.FreshSecondHeaderByte,
                InstructionRawFileOffset: SecondHeaderByteLoadRawFileOffset,
                Address: address,
                Value: capture.FreshSecondHeaderByte,
                Attempted: true,
                Captured: capture.FreshSecondHeaderByte.HasValue,
                Committed: false));
            effects.Add(new GameSocketParserSecondByteScalarGateMemoryEffect(
                Kind:
                    GameSocketParserSecondByteScalarGateMemoryEffectKind.FreshSecondHeaderByteRead,
                InstructionRawFileOffset: SecondHeaderByteLoadRawFileOffset,
                Address: address,
                Value: capture.FreshSecondHeaderByte is byte freshByte ? freshByte : null,
                Attempted: true,
                Captured: capture.FreshSecondHeaderByte.HasValue,
                Committed: false));

            if (capture.FreshSecondHeaderByte is not byte secondByte)
            {
                captureState = GameSocketParserSecondByteScalarGateCaptureState.Missing;
                outcome =
                    GameSocketParserSecondByteScalarGateOutcome.MissingFreshSecondHeaderByte;
                flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
                stopBefore = SecondHeaderByteLoadRawFileOffset;
            }
            else
            {
                captureState = GameSocketParserSecondByteScalarGateCaptureState.Captured;
                byte firstLowThree = (byte)parentPlan.ResultECX!.Value;
                ushort shiftedFirst = (ushort)(firstLowThree << 8);
                reached.Add(ShiftCX8RawFileOffset);
                flagsAfterShiftCX8 = ShiftFlags16(
                    shiftedFirst,
                    carry: false,
                    overflow: null);

                reached.Add(OrCXAXRawFileOffset);
                ushort scalar = (ushort)(shiftedFirst | secondByte);
                flagsAfterOrCXAX = LogicFlags16(scalar);
                combinedScalar = scalar;

                reached.Add(MoveZXEAXFromCXRawFileOffset);
                resultEAX = scalar;

                reached.Add(CombinedScalarStackStoreRawFileOffset);
                uint esp = parentPlan.InheritedFreshPostResizeESP!.Value;
                stackStoreAddress =
                    unchecked(esp + (uint)CombinedScalarStackStoreDisplacement);
                stores.Add(new GameSocketParserSecondByteScalarGateStore(
                    Kind: GameSocketParserSecondByteScalarGateStoreKind.CombinedScalar,
                    InstructionRawFileOffset: CombinedScalarStackStoreRawFileOffset,
                    StackAddress: stackStoreAddress.Value,
                    Value: scalar,
                    Attempted: true,
                    Committed: true));
                effects.Add(new GameSocketParserSecondByteScalarGateMemoryEffect(
                    Kind: GameSocketParserSecondByteScalarGateMemoryEffectKind.StackStore,
                    InstructionRawFileOffset: CombinedScalarStackStoreRawFileOffset,
                    Address: stackStoreAddress.Value,
                    Value: scalar,
                    Attempted: true,
                    Captured: true,
                    Committed: true));

                reached.Add(LeaECXMinusTwoRawFileOffset);
                uint leaResult = unchecked((uint)scalar - 2);
                resultECX = leaResult;

                reached.Add(CompareCXDI16RawFileOffset);
                ushort compareLeft = (ushort)leaResult;
                ushort compareRight = (ushort)parentPlan.ParentPlan.ResultEDI!.Value;
                comparedCX = compareLeft;
                comparedDI = compareRight;
                flagsAtStop = CompareFlags16(compareLeft, compareRight);

                reached.Add(UnsignedAboveBranchRawFileOffset);
                bool isAbove = compareLeft > compareRight;
                branchTaken = isAbove;
                outcome =
                    GameSocketParserSecondByteScalarGateOutcome.BoundedBeforeEitherGateDestination;
                stopBefore = isAbove
                    ? BranchTargetRawFileOffset
                    : FallthroughDestinationRawFileOffset;
            }
        }

        ImmutableArray<GameSocketParserSecondByteScalarGateCaptureKind> unusedCaptures =
            captureState == GameSocketParserSecondByteScalarGateCaptureState.SuppliedButUnused
                ? [GameSocketParserSecondByteScalarGateCaptureKind.FreshSecondHeaderByte]
                : [];
        return new GameSocketParserSecondByteScalarGatePlan(
            capture,
            parentPlan,
            captureState,
            unusedCaptures,
            reads.ToImmutable(),
            stores.ToImmutable(),
            effects.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            parentHandoffAdmitted,
            secondByteAddress,
            stackStoreAddress,
            combinedScalar,
            resultEAX,
            resultECX,
            comparedCX,
            comparedDI,
            flagsAfterShiftCX8,
            flagsAfterOrCXAX,
            flagsAtStop,
            branchTaken,
            stopBefore);
    }

    private static GameSocketParserSecondByteScalarGateFlags? ToFlags(
        GameSocketParserFirstByteFieldsFlags? flags) =>
        flags is GameSocketParserFirstByteFieldsFlags value
            ? new GameSocketParserSecondByteScalarGateFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserSecondByteScalarGateFlags ShiftFlags16(
        ushort result,
        bool carry,
        bool? overflow) =>
        new(
            Carry: carry,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x8000) != 0,
            Overflow: overflow);

    private static GameSocketParserSecondByteScalarGateFlags LogicFlags16(ushort result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x8000) != 0,
            Overflow: false);

    private static GameSocketParserSecondByteScalarGateFlags CompareFlags16(
        ushort left,
        ushort right)
    {
        ushort result = unchecked((ushort)(left - right));
        return new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x8000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x8000) != 0);
    }

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;
}
