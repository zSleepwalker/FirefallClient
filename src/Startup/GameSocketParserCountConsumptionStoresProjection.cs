using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserCountConsumptionStoresCapture(
    GameSocketParserCallerCountGateCapture ParentCapture,
    uint? FreshStackDwordAtESPPlus14,
    byte? FreshStackByteAtESPPlus13);

public enum GameSocketParserCountConsumptionStoresCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserCountConsumptionStoresCaptureKind
{
    FreshStackDwordAtESPPlus14,
    FreshStackByteAtESPPlus13
}

public enum GameSocketParserCountConsumptionStoresOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshStackDwordAtESPPlus14,
    MissingInheritedEDX,
    MissingFreshStackByteAtESPPlus13,
    MissingCallerEBP,
    BoundedBeforeJZ
}

public enum GameSocketParserCountConsumptionStoresReadKind
{
    FreshStackDwordAtESPPlus14,
    FreshStackByteAtESPPlus13
}

public enum GameSocketParserCountConsumptionStoresStoreKind
{
    StackDwordReadModifyWriteAtESPPlus14,
    StackDwordAtESPPlus20,
    CallerEBPPlusC
}

public enum GameSocketParserCountConsumptionStoresMemoryEffectKind
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
    ParentFreshCallerCountDwordRead,
    FreshStackDwordAtESPPlus14Read,
    StackDwordReadModifyWriteAtESPPlus14,
    StackDwordAtESPPlus20,
    FreshStackByteAtESPPlus13Read,
    CallerEBPPlusC
}

public readonly record struct GameSocketParserCountConsumptionStoresRead(
    GameSocketParserCountConsumptionStoresReadKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserCountConsumptionStoresStore(
    GameSocketParserCountConsumptionStoresStoreKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserCountConsumptionStoresMemoryEffect(
    GameSocketParserCountConsumptionStoresMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserCountConsumptionStoresFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed class GameSocketParserCountConsumptionStoresPlan
{
    internal GameSocketParserCountConsumptionStoresPlan(
        GameSocketParserCountConsumptionStoresCapture capture,
        GameSocketParserCallerCountGatePlan parentPlan,
        GameSocketParserCountConsumptionStoresCaptureState stackDwordCaptureState,
        GameSocketParserCountConsumptionStoresCaptureState stackByteCaptureState,
        ImmutableArray<GameSocketParserCountConsumptionStoresCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserCountConsumptionStoresRead> reads,
        ImmutableArray<GameSocketParserCountConsumptionStoresStore> stores,
        ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserCountConsumptionStoresOutcome outcome,
        bool parentHandoffAdmitted,
        uint? resultEDI,
        uint? resultEAX,
        uint? resultEDX,
        uint? resultESP,
        uint? resultEBP,
        uint? freshStackDwordReadAddress,
        uint? freshStackByteReadAddress,
        GameSocketParserCountConsumptionStoresFlags? flagsAfterAddStackDword,
        GameSocketParserCountConsumptionStoresFlags? flagsAfterAddEDX,
        GameSocketParserCountConsumptionStoresFlags? flagsAfterSubEAX,
        GameSocketParserCountConsumptionStoresFlags? flagsAfterCompareStackByte,
        GameSocketParserCountConsumptionStoresFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        StackDwordCaptureState = stackDwordCaptureState;
        StackByteCaptureState = stackByteCaptureState;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        Stores = stores;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        ResultEDI = resultEDI;
        ResultEAX = resultEAX;
        ResultEDX = resultEDX;
        ResultESP = resultESP;
        ResultEBP = resultEBP;
        FreshStackDwordReadAddress = freshStackDwordReadAddress;
        FreshStackByteReadAddress = freshStackByteReadAddress;
        FlagsAfterAddStackDword = flagsAfterAddStackDword;
        FlagsAfterAddEDX = flagsAfterAddEDX;
        FlagsAfterSubEAX = flagsAfterSubEAX;
        FlagsAfterCompareStackByte = flagsAfterCompareStackByte;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserCountConsumptionStoresCapture Capture { get; }
    public GameSocketParserCallerCountGatePlan ParentPlan { get; }
    public GameSocketParserCountConsumptionStoresCaptureState StackDwordCaptureState { get; }
    public GameSocketParserCountConsumptionStoresCaptureState StackByteCaptureState { get; }
    public ImmutableArray<GameSocketParserCountConsumptionStoresCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserCountConsumptionStoresRead> Reads { get; }
    public ImmutableArray<GameSocketParserCountConsumptionStoresStore> Stores { get; }
    public ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserCountConsumptionStoresOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? ResultEDI { get; }
    public uint? ResultEAX { get; }
    public uint? ResultEDX { get; }
    public uint? ResultESP { get; }
    public uint? ResultEBP { get; }
    public uint? InheritedEDX => ParentPlan.InheritedEDX;
    public uint? InheritedESP => ParentPlan.InheritedESP;
    public uint? FreshStackDwordReadAddress { get; }
    public uint? FreshStackByteReadAddress { get; }
    public GameSocketParserCountConsumptionStoresFlags? FlagsAfterAddStackDword { get; }
    public GameSocketParserCountConsumptionStoresFlags? FlagsAfterAddEDX { get; }
    public GameSocketParserCountConsumptionStoresFlags? FlagsAfterSubEAX { get; }
    public GameSocketParserCountConsumptionStoresFlags? FlagsAfterCompareStackByte { get; }
    public GameSocketParserCountConsumptionStoresFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserCountConsumptionStoresOutcome.BoundedBeforeJZ
        && StopBeforeRawFileOffset
            == GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
        || (!ParentHandoffAdmitted && ParentPlan.ReachedBoundedEndpoint);
    public bool ExactBoundedCompletion =>
        Outcome == GameSocketParserCountConsumptionStoresOutcome.BoundedBeforeJZ
        && StopBeforeRawFileOffset
            == GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
        || (!ParentHandoffAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion && ParentPlan.IsCleanCompletion && UnusedCaptures.IsEmpty;
    public bool BranchTakenIsUnmodeled => true;
    public bool JumpZeroModeled => false;
    public bool JZTargetEntered => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
    public bool ExactlyThreeChildWritesCommitted =>
        Stores.Length == 3 && Stores.All(store => store.Committed);
    public bool ChildWritesEmpty => !Stores.Any(store => store.Committed);
}

/// <summary>
/// Reprojects the caller-count gate's fallthrough, then records the three ordered
/// stores and the intervening fresh stack reads without entering the following JZ.
/// </summary>
public static class GameSocketParserCountConsumptionStoresProjection
{
    public const uint AddStackDwordAtESPPlus14RawFileOffset = 0x00BD3249;
    public const uint AddEDX2RawFileOffset = 0x00BD324D;
    public const uint SubEAXEDI32RawFileOffset = 0x00BD3250;
    public const uint StoreEDXAtESPPlus20RawFileOffset = 0x00BD3252;
    public const uint CompareFreshStackByteRawFileOffset = 0x00BD3256;
    public const uint StoreEAXAtEBPPlusCRawFileOffset = 0x00BD325B;
    public const uint JZRawFileOffset = 0x00BD325E;

    public const uint FreshStackDwordDisplacement = 0x14;
    public const uint StackDwordStoreDisplacement = 0x20;
    public const uint FreshStackByteDisplacement = 0x13;
    public const uint CallerEBPStoreDisplacement = 0x0C;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "01 7C 24 14 83 C2 02 2B C7 89 54 24 20 80 7C 24 13 00 89 45 0C";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserCountConsumptionStoresPlan Project(
        GameSocketParserCountConsumptionStoresCapture capture)
    {
        GameSocketParserCallerCountGatePlan parentPlan =
            GameSocketParserCallerCountGateProjection.Project(capture.ParentCapture);
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserCountConsumptionStoresRead>();
        var stores =
            ImmutableArray.CreateBuilder<GameSocketParserCountConsumptionStoresStore>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserCountConsumptionStoresMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();

        foreach (GameSocketParserCallerCountGateMemoryEffect parentEffect
            in parentPlan.OrderedMemoryEffects)
        {
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                Kind: MapParentEffect(parentEffect.Kind),
                InstructionRawFileOffset: parentEffect.InstructionRawFileOffset,
                Address: parentEffect.Address,
                Value: parentEffect.Value,
                Attempted: parentEffect.Attempted,
                Captured: parentEffect.Captured,
                Committed: parentEffect.Committed));
        }

        bool parentHandoffAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.ChildGateReachedEndpoint
            && parentPlan.BranchTaken == false
            && parentPlan.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.FallthroughDestinationRawFileOffset
            && parentPlan.ResultEDI.HasValue
            && parentPlan.ResultEAX.HasValue;

        var stackDwordState =
            GameSocketParserCountConsumptionStoresCaptureState.NotReached;
        var stackByteState =
            GameSocketParserCountConsumptionStoresCaptureState.NotReached;
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserCountConsumptionStoresCaptureKind>();
        uint? resultEDI = null;
        uint? resultEAX = null;
        uint? resultEDX = null;
        uint? resultESP = null;
        uint? resultEBP = null;
        uint? stackDwordAddress = null;
        uint? stackByteAddress = null;
        GameSocketParserCountConsumptionStoresFlags? flagsAfterAddStack = null;
        GameSocketParserCountConsumptionStoresFlags? flagsAfterAddEDX = null;
        GameSocketParserCountConsumptionStoresFlags? flagsAfterSubEAX = null;
        GameSocketParserCountConsumptionStoresFlags? flagsAfterCompareStackByte = null;
        GameSocketParserCountConsumptionStoresFlags? flagsAtStop =
            ToFlags(parentPlan.FlagsAtStop);
        GameSocketParserCountConsumptionStoresOutcome outcome;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome =
                GameSocketParserCountConsumptionStoresOutcome.ParentHandoffNotAdmitted;
            stackDwordState = StateWhenUnreached(capture.FreshStackDwordAtESPPlus14);
            stackByteState = StateWhenUnreached(capture.FreshStackByteAtESPPlus13);
            stopBefore = parentPlan.StopBeforeRawFileOffset;
            AddUnusedIfSupplied(
                unused,
                stackDwordState,
                GameSocketParserCountConsumptionStoresCaptureKind.FreshStackDwordAtESPPlus14);
            AddUnusedIfSupplied(
                unused,
                stackByteState,
                GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13);
        }
        else
        {
            reached.Add(AddStackDwordAtESPPlus14RawFileOffset);
            resultEDI = parentPlan.ResultEDI;
            resultEAX = parentPlan.ResultEAX;
            resultEDX = parentPlan.InheritedEDX;
            resultESP = parentPlan.InheritedESP;
            resultEBP = capture.ParentCapture.FreshCallerEBP;

            if (resultESP is not uint stackPointer)
            {
                stackDwordState = StateWhenUnreached(capture.FreshStackDwordAtESPPlus14);
                stackByteState = StateWhenUnreached(capture.FreshStackByteAtESPPlus13);
                reads.Add(new GameSocketParserCountConsumptionStoresRead(
                    GameSocketParserCountConsumptionStoresReadKind.FreshStackDwordAtESPPlus14,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    null,
                    null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                    GameSocketParserCountConsumptionStoresMemoryEffectKind.FreshStackDwordAtESPPlus14Read,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    null,
                    null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                stores.Add(new GameSocketParserCountConsumptionStoresStore(
                    GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    null,
                    null,
                    Attempted: true,
                    Committed: false));
                effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                    GameSocketParserCountConsumptionStoresMemoryEffectKind.StackDwordReadModifyWriteAtESPPlus14,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    null,
                    null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                outcome =
                    GameSocketParserCountConsumptionStoresOutcome.MissingFreshStackDwordAtESPPlus14;
                stopBefore = AddStackDwordAtESPPlus14RawFileOffset;
                flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
                AddUnusedIfSupplied(
                    unused,
                    stackDwordState,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackDwordAtESPPlus14);
                AddUnusedIfSupplied(
                    unused,
                    stackByteState,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13);
                return CreatePlan();
            }

            stackDwordAddress =
                unchecked(stackPointer + FreshStackDwordDisplacement);
            bool stackDwordCaptured = capture.FreshStackDwordAtESPPlus14.HasValue;
            uint? stackDwordValue = capture.FreshStackDwordAtESPPlus14;
            reads.Add(new GameSocketParserCountConsumptionStoresRead(
                GameSocketParserCountConsumptionStoresReadKind.FreshStackDwordAtESPPlus14,
                AddStackDwordAtESPPlus14RawFileOffset,
                stackDwordAddress,
                stackDwordValue,
                Attempted: true,
                Captured: stackDwordCaptured,
                Committed: false));
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                GameSocketParserCountConsumptionStoresMemoryEffectKind.FreshStackDwordAtESPPlus14Read,
                AddStackDwordAtESPPlus14RawFileOffset,
                stackDwordAddress,
                stackDwordValue,
                Attempted: true,
                Captured: stackDwordCaptured,
                Committed: false));

            if (!stackDwordCaptured)
            {
                stackDwordState =
                    GameSocketParserCountConsumptionStoresCaptureState.Missing;
                stackByteState = StateWhenUnreached(capture.FreshStackByteAtESPPlus13);
                stores.Add(new GameSocketParserCountConsumptionStoresStore(
                    GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    stackDwordAddress,
                    null,
                    Attempted: true,
                    Committed: false));
                effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                    GameSocketParserCountConsumptionStoresMemoryEffectKind.StackDwordReadModifyWriteAtESPPlus14,
                    AddStackDwordAtESPPlus14RawFileOffset,
                    stackDwordAddress,
                    null,
                    Attempted: true,
                    Captured: false,
                    Committed: false));
                outcome =
                    GameSocketParserCountConsumptionStoresOutcome.MissingFreshStackDwordAtESPPlus14;
                stopBefore = AddStackDwordAtESPPlus14RawFileOffset;
                AddUnusedIfSupplied(
                    unused,
                    stackByteState,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13);
                return CreatePlan();
            }

            stackDwordState = GameSocketParserCountConsumptionStoresCaptureState.Captured;
            uint updatedStackDword = unchecked(stackDwordValue!.Value + resultEDI!.Value);
            flagsAfterAddStack = AddFlags32(stackDwordValue.Value, resultEDI.Value);
            flagsAtStop = flagsAfterAddStack;
            stores.Add(new GameSocketParserCountConsumptionStoresStore(
                GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14,
                AddStackDwordAtESPPlus14RawFileOffset,
                stackDwordAddress,
                updatedStackDword,
                Attempted: true,
                Committed: true));
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                GameSocketParserCountConsumptionStoresMemoryEffectKind.StackDwordReadModifyWriteAtESPPlus14,
                AddStackDwordAtESPPlus14RawFileOffset,
                stackDwordAddress,
                updatedStackDword,
                Attempted: true,
                Captured: true,
                Committed: true));
            reached.Add(AddEDX2RawFileOffset);

            if (resultEDX is not uint inheritedEDX)
            {
                stackByteState = StateWhenUnreached(capture.FreshStackByteAtESPPlus13);
                outcome = GameSocketParserCountConsumptionStoresOutcome.MissingInheritedEDX;
                stopBefore = AddEDX2RawFileOffset;
                AddUnusedIfSupplied(
                    unused,
                    stackByteState,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13);
                return CreatePlan();
            }

            resultEDX = unchecked(inheritedEDX + 2);
            flagsAfterAddEDX = AddFlags32(inheritedEDX, 2);
            flagsAtStop = flagsAfterAddEDX;
            reached.Add(SubEAXEDI32RawFileOffset);
            uint countBeforeConsumption = resultEAX!.Value;
            resultEAX = unchecked(countBeforeConsumption - resultEDI.Value);
            flagsAfterSubEAX = SubFlags32(countBeforeConsumption, resultEDI.Value);
            flagsAtStop = flagsAfterSubEAX;

            reached.Add(StoreEDXAtESPPlus20RawFileOffset);
            uint stackDwordStoreAddress =
                unchecked(stackPointer + StackDwordStoreDisplacement);
            stores.Add(new GameSocketParserCountConsumptionStoresStore(
                GameSocketParserCountConsumptionStoresStoreKind.StackDwordAtESPPlus20,
                StoreEDXAtESPPlus20RawFileOffset,
                stackDwordStoreAddress,
                resultEDX,
                Attempted: true,
                Committed: true));
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                GameSocketParserCountConsumptionStoresMemoryEffectKind.StackDwordAtESPPlus20,
                StoreEDXAtESPPlus20RawFileOffset,
                stackDwordStoreAddress,
                resultEDX,
                Attempted: true,
                Captured: true,
                Committed: true));

            reached.Add(CompareFreshStackByteRawFileOffset);
            stackByteAddress =
                unchecked(stackPointer + FreshStackByteDisplacement);
            bool stackByteCaptured = capture.FreshStackByteAtESPPlus13.HasValue;
            byte? stackByteValue = capture.FreshStackByteAtESPPlus13;
            reads.Add(new GameSocketParserCountConsumptionStoresRead(
                GameSocketParserCountConsumptionStoresReadKind.FreshStackByteAtESPPlus13,
                CompareFreshStackByteRawFileOffset,
                stackByteAddress,
                stackByteValue.HasValue ? stackByteValue.Value : null,
                Attempted: true,
                Captured: stackByteCaptured,
                Committed: false));
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                GameSocketParserCountConsumptionStoresMemoryEffectKind.FreshStackByteAtESPPlus13Read,
                CompareFreshStackByteRawFileOffset,
                stackByteAddress,
                stackByteValue.HasValue ? stackByteValue.Value : null,
                Attempted: true,
                Captured: stackByteCaptured,
                Committed: false));

            if (!stackByteCaptured)
            {
                stackByteState =
                    GameSocketParserCountConsumptionStoresCaptureState.Missing;
                outcome =
                    GameSocketParserCountConsumptionStoresOutcome.MissingFreshStackByteAtESPPlus13;
                stopBefore = CompareFreshStackByteRawFileOffset;
                AddUnusedIfSupplied(
                    unused,
                    GameSocketParserCountConsumptionStoresCaptureState.Captured,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackDwordAtESPPlus14);
                return CreatePlan();
            }

            stackByteState = GameSocketParserCountConsumptionStoresCaptureState.Captured;
            flagsAfterCompareStackByte = CompareFlags8(stackByteValue!.Value);
            flagsAtStop = flagsAfterCompareStackByte;
            reached.Add(StoreEAXAtEBPPlusCRawFileOffset);
            if (resultEBP is not uint callerEBP)
            {
                outcome = GameSocketParserCountConsumptionStoresOutcome.MissingCallerEBP;
                stopBefore = StoreEAXAtEBPPlusCRawFileOffset;
                AddUnusedIfSupplied(
                    unused,
                    GameSocketParserCountConsumptionStoresCaptureState.Captured,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackDwordAtESPPlus14);
                AddUnusedIfSupplied(
                    unused,
                    GameSocketParserCountConsumptionStoresCaptureState.Captured,
                    GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13);
                return CreatePlan();
            }

            uint callerStoreAddress =
                unchecked(callerEBP + CallerEBPStoreDisplacement);
            stores.Add(new GameSocketParserCountConsumptionStoresStore(
                GameSocketParserCountConsumptionStoresStoreKind.CallerEBPPlusC,
                StoreEAXAtEBPPlusCRawFileOffset,
                callerStoreAddress,
                resultEAX,
                Attempted: true,
                Committed: true));
            effects.Add(new GameSocketParserCountConsumptionStoresMemoryEffect(
                GameSocketParserCountConsumptionStoresMemoryEffectKind.CallerEBPPlusC,
                StoreEAXAtEBPPlusCRawFileOffset,
                callerStoreAddress,
                resultEAX,
                Attempted: true,
                Captured: true,
                Committed: true));
            outcome = GameSocketParserCountConsumptionStoresOutcome.BoundedBeforeJZ;
            stopBefore = JZRawFileOffset;
        }

        return CreatePlan();

        GameSocketParserCountConsumptionStoresPlan CreatePlan() =>
            new(
                capture,
                parentPlan,
                stackDwordState,
                stackByteState,
                unused.ToImmutable(),
                reads.ToImmutable(),
                stores.ToImmutable(),
                effects.ToImmutable(),
                reached.ToImmutable(),
                outcome,
                parentHandoffAdmitted,
                resultEDI,
                resultEAX,
                resultEDX,
                resultESP,
                resultEBP,
                stackDwordAddress,
                stackByteAddress,
                flagsAfterAddStack,
                flagsAfterAddEDX,
                flagsAfterSubEAX,
                flagsAfterCompareStackByte,
                flagsAtStop,
                stopBefore);
    }

    private static GameSocketParserCountConsumptionStoresCaptureState StateWhenUnreached(
        uint? value) =>
        value.HasValue
            ? GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused
            : GameSocketParserCountConsumptionStoresCaptureState.NotReached;

    private static GameSocketParserCountConsumptionStoresCaptureState StateWhenUnreached(
        byte? value) =>
        value.HasValue
            ? GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused
            : GameSocketParserCountConsumptionStoresCaptureState.NotReached;

    private static void AddUnusedIfSupplied(
        ImmutableArray<GameSocketParserCountConsumptionStoresCaptureKind>.Builder unused,
        GameSocketParserCountConsumptionStoresCaptureState state,
        GameSocketParserCountConsumptionStoresCaptureKind kind)
    {
        if (state == GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused)
        {
            unused.Add(kind);
        }
    }

    private static GameSocketParserCountConsumptionStoresMemoryEffectKind MapParentEffect(
        GameSocketParserCallerCountGateMemoryEffectKind kind) =>
        kind switch
        {
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshLocalVtableRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshLocalVtableRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshSlotCIndirectTargetRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentStackPush =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentStackPush,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshField38DwordRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshField38DwordRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshFieldPlus4DwordRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshFieldPlus4DwordRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentStackStore =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentStackStore,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshHeaderByteRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshHeaderByteRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentFreshSecondHeaderByteRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshSecondHeaderByteRead,
            GameSocketParserCallerCountGateMemoryEffectKind.ParentStackScalarStore =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentStackScalarStore,
            GameSocketParserCallerCountGateMemoryEffectKind.FreshCallerCountDwordRead =>
                GameSocketParserCountConsumptionStoresMemoryEffectKind.ParentFreshCallerCountDwordRead,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    private static GameSocketParserCountConsumptionStoresFlags? ToFlags(
        GameSocketParserCallerCountGateFlags? flags) =>
        flags is GameSocketParserCallerCountGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserCountConsumptionStoresFlags AddFlags32(
        uint left,
        uint right)
    {
        ulong wideResult = (ulong)left + right;
        uint result = unchecked(left + right);
        return new(
            Carry: wideResult > uint.MaxValue,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (~(left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static GameSocketParserCountConsumptionStoresFlags SubFlags32(
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

    private static GameSocketParserCountConsumptionStoresFlags CompareFlags8(byte value)
    {
        byte result = value;
        return new(
            Carry: false,
            Parity: HasEvenParity(result),
            AuxiliaryCarry: false,
            Zero: result == 0,
            Sign: (result & 0x80) != 0,
            Overflow: false);
    }

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;
}
