using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserPostResizePointerCapture(
    GameSocketParserLocalSizeDispatchCapture ParentCapture,
    bool? OpaqueResizeReturnedNormally,
    uint? FreshPostResizeESP,
    uint? FreshField38Dword,
    uint? FreshFieldPlus4Dword);

public enum GameSocketParserPostResizePointerCaptureKind
{
    OpaqueResizeReturnedNormally,
    FreshPostResizeESP,
    FreshField38Dword,
    FreshFieldPlus4Dword
}

public enum GameSocketParserPostResizePointerCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketParserPostResizePointerCaptureStates(
    GameSocketParserPostResizePointerCaptureState OpaqueResizeReturnedNormally,
    GameSocketParserPostResizePointerCaptureState FreshPostResizeESP,
    GameSocketParserPostResizePointerCaptureState FreshField38Dword,
    GameSocketParserPostResizePointerCaptureState FreshFieldPlus4Dword);

public enum GameSocketParserPostResizePointerOutcome
{
    ParentDidNotReachResizeCallBoundary,
    ParentResizeHandoffNotAdmitted,
    MissingOpaqueResizeReturnStatus,
    OpaqueResizeDidNotReturnNormally,
    MissingPostResizeESP,
    MissingFreshField38Dword,
    MissingFreshFieldPlus4Dword,
    BoundedBeforeHeaderByteRead
}

public enum GameSocketParserPostResizePointerReadKind
{
    FreshField38Dword,
    FreshFieldPlus4Dword
}

public enum GameSocketParserPostResizePointerMemoryEffectKind
{
    ParentFreshLocalVtableRead,
    ParentFreshSlotCIndirectTargetRead,
    ParentStackPush,
    FreshField38DwordRead,
    FreshFieldPlus4DwordRead,
    StackStore
}

public readonly record struct GameSocketParserPostResizePointerRead(
    GameSocketParserPostResizePointerReadKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserPostResizePointerMemoryEffect(
    GameSocketParserPostResizePointerMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserPostResizePointerStore(
    uint InstructionRawFileOffset,
    uint StackAddress,
    uint Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserPostResizePointerAddresses(
    uint? FreshField38ReadAddress,
    uint? FreshFieldPlus4ReadAddress,
    uint? FirstStackStoreAddress,
    uint? SecondStackStoreAddress);

public readonly record struct GameSocketParserPostResizePointerFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow)
{
    public static GameSocketParserPostResizePointerFlags Unknown => new(
        Carry: null,
        Parity: null,
        AuxiliaryCarry: null,
        Zero: null,
        Sign: null,
        Overflow: null);
}

public sealed class GameSocketParserPostResizePointerPlan
{
    internal GameSocketParserPostResizePointerPlan(
        GameSocketParserPostResizePointerCapture capture,
        GameSocketParserLocalSizeDispatchPlan parentPlan,
        GameSocketParserPostResizePointerCaptureStates captureStates,
        ImmutableArray<GameSocketParserPostResizePointerCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserPostResizePointerRead> reads,
        ImmutableArray<GameSocketParserPostResizePointerStore> stores,
        ImmutableArray<GameSocketParserPostResizePointerMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserPostResizePointerOutcome outcome,
        bool parentResizeHandoffAdmitted,
        GameSocketParserPostResizePointerAddresses addresses,
        uint? resultEDX,
        uint? resultEDI,
        GameSocketParserPostResizePointerFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        CaptureStates = captureStates;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        Stores = stores;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        ParentResizeHandoffAdmitted = parentResizeHandoffAdmitted;
        Addresses = addresses;
        ResultEDX = resultEDX;
        ResultEDI = resultEDI;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserPostResizePointerCapture Capture { get; }
    public GameSocketParserLocalSizeDispatchPlan ParentPlan { get; }
    public GameSocketParserPostResizePointerCaptureStates CaptureStates { get; }
    public ImmutableArray<GameSocketParserPostResizePointerCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserPostResizePointerRead> Reads { get; }
    public ImmutableArray<GameSocketParserPostResizePointerStore> Stores { get; }
    public ImmutableArray<GameSocketParserPostResizePointerMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserPostResizePointerMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserPostResizePointerOutcome Outcome { get; }
    public bool ParentResizeHandoffAdmitted { get; }
    public GameSocketParserPostResizePointerAddresses Addresses { get; }
    public uint? ResultEDX { get; }
    public uint? ResultEDI { get; }
    public GameSocketParserPostResizePointerFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserPostResizePointerProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserPostResizePointerOutcome.BoundedBeforeHeaderByteRead
        && StopBeforeRawFileOffset
            == GameSocketParserPostResizePointerProjection.HeaderByteLoadRawFileOffset;
    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentResizeHandoffAdmitted && ParentPlan.ReachedBoundedEndpoint);
    public bool IsCleanCompletion => ExactBoundedCompletion
        && ParentPlan.IsCleanCompletion
        && UnusedCaptures.IsEmpty;
    public uint? HeaderByteReadAddress =>
        ReachedBoundedEndpoint && ResultEDX is uint resultEDX ? resultEDX : null;
    public bool HeaderByteReadAttempted => false;
    public bool OpaqueResizeTargetExecutionModeled => false;
    public bool AllocationSemanticsModeled => false;
    public bool ParserExecutionModeled => false;
}

/// <summary>
/// Reprojects the preceding local-size dispatch from its immutable capture, then follows
/// the caller's post-resize scalar loads and stack stores. This is a bounded observation:
/// field values are untyped DWORDs, and neither the resize target nor parser is emulated.
/// </summary>
public static class GameSocketParserPostResizePointerProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;

    public const uint ResizeCallRawFileOffset =
        GameSocketParserLocalSizeDispatchProjection.ResizeCallRawFileOffset;
    public const uint FreshField38LoadRawFileOffset = 0x00BD31DC;
    public const uint FieldTestRawFileOffset = 0x00BD31E0;
    public const uint FieldZeroBranchRawFileOffset = 0x00BD31E2;
    public const uint FreshFieldPlus4LoadRawFileOffset = 0x00BD31E4;
    public const uint NonNullBranchRawFileOffset = 0x00BD31E7;
    public const uint NullResultClearRawFileOffset = 0x00BD31E9;
    public const uint FirstStackStoreRawFileOffset = 0x00BD31EB;
    public const uint SizeImmediateLoadRawFileOffset = 0x00BD31EF;
    public const uint SecondStackStoreRawFileOffset = 0x00BD31F4;
    public const uint HeaderByteLoadRawFileOffset = 0x00BD31F8;

    public const int FreshField38Displacement = 0x38;
    public const int FreshFieldPlus4Displacement = 4;
    public const int FirstStackStoreDisplacement = 0x14;
    public const int SecondStackStoreDisplacement = 0x24;
    public const uint ResultEDIValue = 0x0000056E;
    public const bool OpaqueResizeTargetExecutionModeled = false;
    public const bool AllocationSemanticsModeled = false;
    public const bool ParserExecutionModeled = false;

    public const string CapturedBytes =
        "8B 54 24 38 85 D2 74 05 8B 52 04 EB 02 33 D2 "
        + "89 54 24 14 BF 6E 05 00 00 89 54 24 24 8A 02";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserPostResizePointerPlan Project(
        GameSocketParserPostResizePointerCapture capture)
    {
        GameSocketParserLocalSizeDispatchPlan parentPlan =
            GameSocketParserLocalSizeDispatchProjection.Project(capture.ParentCapture);
        var reads = ImmutableArray.CreateBuilder<GameSocketParserPostResizePointerRead>();
        var stores = ImmutableArray.CreateBuilder<GameSocketParserPostResizePointerStore>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserPostResizePointerMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        uint? field38Address = null;
        uint? fieldPlus4Address = null;
        uint? firstStoreAddress = null;
        uint? secondStoreAddress = null;
        uint? resultEDX = null;
        uint? resultEDI = null;
        GameSocketParserPostResizePointerOutcome outcome;
        GameSocketParserPostResizePointerFlags? flagsAtStop;
        uint stopBefore;

        foreach (GameSocketParserLocalSizeDispatchMemoryEffect parentEffect
            in parentPlan.MemoryEffects)
        {
            effects.Add(new GameSocketParserPostResizePointerMemoryEffect(
                Kind: parentEffect.Kind switch
                {
                    GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshLocalVtableRead =>
                        GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshLocalVtableRead,
                    GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshSlotCIndirectTargetRead =>
                        GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
                    _ => GameSocketParserPostResizePointerMemoryEffectKind.ParentStackPush
                },
                InstructionRawFileOffset: parentEffect.InstructionRawFileOffset,
                Address: parentEffect.Address,
                Value: parentEffect.Value,
                Attempted: parentEffect.Attempted,
                Captured: parentEffect.Captured,
                Committed: parentEffect.Committed));
        }

        bool parentReachedResizeCall = parentPlan.ReachedBoundedEndpoint;
        bool parentResizeHandoffAdmitted =
            parentPlan.IsCleanCompletion && parentReachedResizeCall;
        if (!parentReachedResizeCall)
        {
            outcome =
                GameSocketParserPostResizePointerOutcome.ParentDidNotReachResizeCallBoundary;
            stopBefore = parentPlan.StopBeforeRawFileOffset;
            flagsAtStop = ToPostResizeFlags(parentPlan.FlagsAtStop);
        }
        else if (!parentResizeHandoffAdmitted)
        {
            outcome =
                GameSocketParserPostResizePointerOutcome.ParentResizeHandoffNotAdmitted;
            stopBefore = parentPlan.StopBeforeRawFileOffset;
            flagsAtStop = ToPostResizeFlags(parentPlan.FlagsAtStop);
        }
        else if (capture.OpaqueResizeReturnedNormally is null)
        {
            outcome =
                GameSocketParserPostResizePointerOutcome.MissingOpaqueResizeReturnStatus;
            stopBefore = ResizeCallRawFileOffset;
            flagsAtStop = ToPostResizeFlags(parentPlan.FlagsAtStop);
        }
        else if (!capture.OpaqueResizeReturnedNormally.Value)
        {
            outcome =
                GameSocketParserPostResizePointerOutcome.OpaqueResizeDidNotReturnNormally;
            stopBefore = ResizeCallRawFileOffset;
            flagsAtStop = ToPostResizeFlags(parentPlan.FlagsAtStop);
        }
        else
        {
            flagsAtStop = GameSocketParserPostResizePointerFlags.Unknown;
            if (capture.FreshPostResizeESP is not uint postResizeEsp)
            {
                outcome = GameSocketParserPostResizePointerOutcome.MissingPostResizeESP;
                stopBefore = FreshField38LoadRawFileOffset;
            }
            else
            {
                field38Address = unchecked(
                    postResizeEsp + (uint)FreshField38Displacement);
                reached.Add(FreshField38LoadRawFileOffset);
                reads.Add(new GameSocketParserPostResizePointerRead(
                    Kind: GameSocketParserPostResizePointerReadKind.FreshField38Dword,
                    InstructionRawFileOffset: FreshField38LoadRawFileOffset,
                    Address: field38Address.Value,
                    Value: capture.FreshField38Dword,
                    Attempted: true,
                    Captured: capture.FreshField38Dword.HasValue,
                    Committed: false));
                effects.Add(new GameSocketParserPostResizePointerMemoryEffect(
                    Kind: GameSocketParserPostResizePointerMemoryEffectKind.FreshField38DwordRead,
                    InstructionRawFileOffset: FreshField38LoadRawFileOffset,
                    Address: field38Address.Value,
                    Value: capture.FreshField38Dword,
                    Attempted: true,
                    Captured: capture.FreshField38Dword.HasValue,
                    Committed: false));

                if (capture.FreshField38Dword is not uint field38)
                {
                    outcome =
                        GameSocketParserPostResizePointerOutcome.MissingFreshField38Dword;
                    stopBefore = FreshField38LoadRawFileOffset;
                }
                else
                {
                    reached.Add(FieldTestRawFileOffset);
                    reached.Add(FieldZeroBranchRawFileOffset);
                    flagsAtStop = TestFlags(field38);
                    if (field38 == 0)
                    {
                        reached.Add(NullResultClearRawFileOffset);
                        resultEDX = 0;
                        flagsAtStop = XorZeroFlags();
                    }
                    else
                    {
                        fieldPlus4Address = unchecked(
                            field38 + (uint)FreshFieldPlus4Displacement);
                        reached.Add(FreshFieldPlus4LoadRawFileOffset);
                        reads.Add(new GameSocketParserPostResizePointerRead(
                            Kind: GameSocketParserPostResizePointerReadKind.FreshFieldPlus4Dword,
                            InstructionRawFileOffset: FreshFieldPlus4LoadRawFileOffset,
                            Address: fieldPlus4Address.Value,
                            Value: capture.FreshFieldPlus4Dword,
                            Attempted: true,
                            Captured: capture.FreshFieldPlus4Dword.HasValue,
                            Committed: false));
                        effects.Add(new GameSocketParserPostResizePointerMemoryEffect(
                            Kind:
                                GameSocketParserPostResizePointerMemoryEffectKind.FreshFieldPlus4DwordRead,
                            InstructionRawFileOffset: FreshFieldPlus4LoadRawFileOffset,
                            Address: fieldPlus4Address.Value,
                            Value: capture.FreshFieldPlus4Dword,
                            Attempted: true,
                            Captured: capture.FreshFieldPlus4Dword.HasValue,
                            Committed: false));

                        if (capture.FreshFieldPlus4Dword is not uint fieldPlus4)
                        {
                            outcome =
                                GameSocketParserPostResizePointerOutcome.MissingFreshFieldPlus4Dword;
                            stopBefore = FreshFieldPlus4LoadRawFileOffset;
                            goto Finish;
                        }

                        resultEDX = fieldPlus4;
                        reached.Add(NonNullBranchRawFileOffset);
                    }

                    reached.Add(FirstStackStoreRawFileOffset);
                    firstStoreAddress = unchecked(
                        postResizeEsp + (uint)FirstStackStoreDisplacement);
                    stores.Add(new GameSocketParserPostResizePointerStore(
                        InstructionRawFileOffset: FirstStackStoreRawFileOffset,
                        StackAddress: firstStoreAddress.Value,
                        Value: resultEDX.Value,
                        Attempted: true,
                        Committed: true));
                    effects.Add(new GameSocketParserPostResizePointerMemoryEffect(
                        Kind: GameSocketParserPostResizePointerMemoryEffectKind.StackStore,
                        InstructionRawFileOffset: FirstStackStoreRawFileOffset,
                        Address: firstStoreAddress.Value,
                        Value: resultEDX.Value,
                        Attempted: true,
                        Captured: true,
                        Committed: true));

                    reached.Add(SizeImmediateLoadRawFileOffset);
                    resultEDI = ResultEDIValue;
                    reached.Add(SecondStackStoreRawFileOffset);
                    secondStoreAddress = unchecked(
                        postResizeEsp + (uint)SecondStackStoreDisplacement);
                    stores.Add(new GameSocketParserPostResizePointerStore(
                        InstructionRawFileOffset: SecondStackStoreRawFileOffset,
                        StackAddress: secondStoreAddress.Value,
                        Value: resultEDX.Value,
                        Attempted: true,
                        Committed: true));
                    effects.Add(new GameSocketParserPostResizePointerMemoryEffect(
                        Kind: GameSocketParserPostResizePointerMemoryEffectKind.StackStore,
                        InstructionRawFileOffset: SecondStackStoreRawFileOffset,
                        Address: secondStoreAddress.Value,
                        Value: resultEDX.Value,
                        Attempted: true,
                        Captured: true,
                        Committed: true));

                    outcome =
                        GameSocketParserPostResizePointerOutcome.BoundedBeforeHeaderByteRead;
                    stopBefore = HeaderByteLoadRawFileOffset;
                }
            }
        }

    Finish:
        GameSocketParserPostResizePointerCaptureStates captureStates =
            GetCaptureStates(capture, parentResizeHandoffAdmitted);
        ImmutableArray<GameSocketParserPostResizePointerCaptureKind> unusedCaptures =
            GetUnusedCaptures(captureStates);
        return new GameSocketParserPostResizePointerPlan(
            capture,
            parentPlan,
            captureStates,
            unusedCaptures,
            reads.ToImmutable(),
            stores.ToImmutable(),
            effects.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            parentResizeHandoffAdmitted,
            new GameSocketParserPostResizePointerAddresses(
                field38Address,
                fieldPlus4Address,
                firstStoreAddress,
                secondStoreAddress),
            resultEDX,
            resultEDI,
            flagsAtStop,
            stopBefore);
    }

    private static GameSocketParserPostResizePointerCaptureStates GetCaptureStates(
        GameSocketParserPostResizePointerCapture capture,
        bool parentResizeHandoffAdmitted)
    {
        bool resizeReturnedNormally =
            parentResizeHandoffAdmitted && capture.OpaqueResizeReturnedNormally == true;
        bool postResizeEspReached = resizeReturnedNormally;
        bool field38Reached = postResizeEspReached && capture.FreshPostResizeESP.HasValue;
        bool fieldPlus4Reached =
            field38Reached && capture.FreshField38Dword is > 0;

        return new GameSocketParserPostResizePointerCaptureStates(
            State(
                capture.OpaqueResizeReturnedNormally.HasValue,
                parentResizeHandoffAdmitted),
            State(capture.FreshPostResizeESP.HasValue, postResizeEspReached),
            State(capture.FreshField38Dword.HasValue, field38Reached),
            State(capture.FreshFieldPlus4Dword.HasValue, fieldPlus4Reached));
    }

    private static GameSocketParserPostResizePointerCaptureState State(
        bool hasValue,
        bool reached) =>
        hasValue
            ? reached
                ? GameSocketParserPostResizePointerCaptureState.Captured
                : GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            : reached
                ? GameSocketParserPostResizePointerCaptureState.Missing
                : GameSocketParserPostResizePointerCaptureState.NotReached;

    private static ImmutableArray<GameSocketParserPostResizePointerCaptureKind>
        GetUnusedCaptures(GameSocketParserPostResizePointerCaptureStates states)
    {
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserPostResizePointerCaptureKind>();
        AddIfUnused(
            states.OpaqueResizeReturnedNormally,
            GameSocketParserPostResizePointerCaptureKind.OpaqueResizeReturnedNormally,
            unused);
        AddIfUnused(
            states.FreshPostResizeESP,
            GameSocketParserPostResizePointerCaptureKind.FreshPostResizeESP,
            unused);
        AddIfUnused(
            states.FreshField38Dword,
            GameSocketParserPostResizePointerCaptureKind.FreshField38Dword,
            unused);
        AddIfUnused(
            states.FreshFieldPlus4Dword,
            GameSocketParserPostResizePointerCaptureKind.FreshFieldPlus4Dword,
            unused);
        return unused.ToImmutable();
    }

    private static void AddIfUnused(
        GameSocketParserPostResizePointerCaptureState state,
        GameSocketParserPostResizePointerCaptureKind kind,
        ImmutableArray<GameSocketParserPostResizePointerCaptureKind>.Builder unused)
    {
        if (state == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused)
            unused.Add(kind);
    }

    private static GameSocketParserPostResizePointerFlags? ToPostResizeFlags(
        GameSocketParserLocalSizeDispatchFlags? flags) =>
        flags is GameSocketParserLocalSizeDispatchFlags value
            ? new GameSocketParserPostResizePointerFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserPostResizePointerFlags TestFlags(uint value) => new(
        Carry: false,
        Parity: HasEvenParity((byte)value),
        AuxiliaryCarry: null,
        Zero: value == 0,
        Sign: (value & 0x80000000) != 0,
        Overflow: false);

    private static GameSocketParserPostResizePointerFlags XorZeroFlags() => new(
        Carry: false,
        Parity: true,
        AuxiliaryCarry: null,
        Zero: true,
        Sign: false,
        Overflow: false);

    private static bool HasEvenParity(byte value)
    {
        int setBits = 0;
        while (value != 0)
        {
            setBits += (int)(value & 1);
            value >>= 1;
        }

        return (setBits & 1) == 0;
    }
}
