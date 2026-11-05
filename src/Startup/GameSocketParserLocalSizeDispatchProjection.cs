using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserLocalSizeDispatchCapture(
    bool? PostConstructorValidated,
    uint? EntryESPAtInitialLocalLea,
    uint? FreshLocalVtable,
    uint? FreshSlotCIndirectTarget,
    bool? OpaqueGetterReturnedNormally,
    uint? OpaqueGetterReturnedEAX,
    uint? PostGetterESP,
    GameSocketParserLocalSizeDispatchFlags? InheritedEntryFlags);

public readonly record struct GameSocketParserLocalSizeDispatchFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow)
{
    public static GameSocketParserLocalSizeDispatchFlags Unknown => new(
        Carry: null,
        Parity: null,
        AuxiliaryCarry: null,
        Zero: null,
        Sign: null,
        Overflow: null);
}

public enum GameSocketParserLocalSizeDispatchCaptureKind
{
    EntryESPAtInitialLocalLea,
    FreshLocalVtable,
    FreshSlotCIndirectTarget,
    OpaqueGetterReturnedNormally,
    OpaqueGetterReturnedEAX,
    PostGetterESP
}

public enum GameSocketParserLocalSizeDispatchCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketParserLocalSizeDispatchCaptureStates(
    GameSocketParserLocalSizeDispatchCaptureState EntryESPAtInitialLocalLea,
    GameSocketParserLocalSizeDispatchCaptureState FreshLocalVtable,
    GameSocketParserLocalSizeDispatchCaptureState FreshSlotCIndirectTarget,
    GameSocketParserLocalSizeDispatchCaptureState OpaqueGetterReturnedNormally,
    GameSocketParserLocalSizeDispatchCaptureState OpaqueGetterReturnedEAX,
    GameSocketParserLocalSizeDispatchCaptureState PostGetterESP);

public enum GameSocketParserLocalSizeDispatchOutcome
{
    MissingPostConstructorValidation,
    PostConstructorPathNotValidated,
    MissingEntryESPAtInitialLocalLea,
    MissingFreshLocalVtable,
    MissingFreshSlotCIndirectTarget,
    MissingOpaqueGetterReturnStatus,
    OpaqueGetterDidNotReturnNormally,
    MissingPostGetterESPBeforeFirstPush,
    MissingGetterEAXBeforeSecondPush,
    BoundedBeforeResizeCall
}

public enum GameSocketParserLocalSizeDispatchReadKind
{
    FreshLocalVtable,
    FreshSlotCIndirectTarget
}

public enum GameSocketParserLocalSizeDispatchMemoryEffectKind
{
    FreshLocalVtableRead,
    FreshSlotCIndirectTargetRead,
    StackPush
}

public readonly record struct GameSocketParserLocalSizeDispatchRead(
    GameSocketParserLocalSizeDispatchReadKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserLocalSizeDispatchMemoryEffect(
    GameSocketParserLocalSizeDispatchMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserLocalSizeDispatchPush(
    uint InstructionRawFileOffset,
    uint StackAddress,
    uint Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserLocalSizeDispatchIndirectCall(
    uint InstructionRawFileOffset,
    uint SlotAddress,
    uint Target,
    bool CallSiteReached,
    bool TargetExecutionModeled);

public readonly record struct GameSocketParserLocalSizeDispatchAddresses(
    uint? InitialLocalAddress,
    uint? FreshVtableReadAddress,
    uint? FreshSlotCReadAddress,
    uint? PostGetterStackAddress,
    uint? StackAddressAfterFirstPush,
    uint? StackAddressAfterSecondPush,
    uint? ResizeLocalAddress);

public sealed class GameSocketParserLocalSizeDispatchPlan
{
    internal GameSocketParserLocalSizeDispatchPlan(
        GameSocketParserLocalSizeDispatchCapture capture,
        GameSocketParserLocalSizeDispatchCaptureStates captureStates,
        ImmutableArray<GameSocketParserLocalSizeDispatchCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserLocalSizeDispatchRead> reads,
        ImmutableArray<GameSocketParserLocalSizeDispatchPush> pushes,
        ImmutableArray<GameSocketParserLocalSizeDispatchMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserLocalSizeDispatchOutcome outcome,
        GameSocketParserLocalSizeDispatchAddresses addresses,
        GameSocketParserLocalSizeDispatchIndirectCall? getterCall,
        GameSocketParserLocalSizeDispatchFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        CaptureStates = captureStates;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        Pushes = pushes;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        Addresses = addresses;
        GetterCall = getterCall;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserLocalSizeDispatchCapture Capture { get; }
    public GameSocketParserLocalSizeDispatchCaptureStates CaptureStates { get; }
    public ImmutableArray<GameSocketParserLocalSizeDispatchCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserLocalSizeDispatchRead> Reads { get; }
    public ImmutableArray<GameSocketParserLocalSizeDispatchRead> OrderedReads => Reads;
    public ImmutableArray<GameSocketParserLocalSizeDispatchPush> Pushes { get; }
    public ImmutableArray<GameSocketParserLocalSizeDispatchMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserLocalSizeDispatchMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserLocalSizeDispatchOutcome Outcome { get; }
    public GameSocketParserLocalSizeDispatchAddresses Addresses { get; }
    public GameSocketParserLocalSizeDispatchIndirectCall? GetterCall { get; }
    public bool GetterCallSiteReached => GetterCall is not null;
    public bool GetterTargetExecutionModeled => false;
    public GameSocketParserLocalSizeDispatchFlags? FlagsBeforeGetterCall =>
        GetterCallSiteReached ? Capture.InheritedEntryFlags : null;
    public bool? GetterReturnedNormally =>
        CaptureStates.OpaqueGetterReturnedNormally
            == GameSocketParserLocalSizeDispatchCaptureState.Captured
                ? Capture.OpaqueGetterReturnedNormally
                : null;
    public uint? GetterReturnedEAX =>
        CaptureStates.OpaqueGetterReturnedEAX
            == GameSocketParserLocalSizeDispatchCaptureState.Captured
                ? Capture.OpaqueGetterReturnedEAX
                : null;
    public bool? GetterReturnInvalidatedFlags =>
        !GetterCallSiteReached
            || CaptureStates.OpaqueGetterReturnedNormally
                != GameSocketParserLocalSizeDispatchCaptureState.Captured
                    ? null
                    : Capture.OpaqueGetterReturnedNormally;
    public GameSocketParserLocalSizeDispatchFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserLocalSizeDispatchProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ReachedResizeCallBoundary =>
        Outcome == GameSocketParserLocalSizeDispatchOutcome.BoundedBeforeResizeCall;
    public bool ResizeCallInstructionReached => false;
    public bool ResizeCallEntered => false;
    public uint? InitialLocalAddress => Addresses.InitialLocalAddress;
    public uint? FreshVtableReadAddress => Addresses.FreshVtableReadAddress;
    public uint? FreshSlotCReadAddress => Addresses.FreshSlotCReadAddress;
    public uint? ResizeLocalAddress => Addresses.ResizeLocalAddress;
    public bool? FreshVtableMatchesInitializer =>
        CaptureStates.FreshLocalVtable
                == GameSocketParserLocalSizeDispatchCaptureState.Captured
            && Capture.FreshLocalVtable is uint vtable
            ? vtable == UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress
            : null;
    public bool? FreshSlotCTargetMatchesStaticEntry =>
        CaptureStates.FreshSlotCIndirectTarget
                == GameSocketParserLocalSizeDispatchCaptureState.Captured
            && Capture.FreshSlotCIndirectTarget is uint target
            ? target == UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryVirtualAddress
            : null;
    public bool? InitialAndResizeLocalAddressesMatch =>
        Addresses.InitialLocalAddress is uint initial
        && Addresses.ResizeLocalAddress is uint resize
            ? initial == resize
            : null;
    public bool ReachedBoundedEndpoint =>
        ReachedResizeCallBoundary && !ResizeCallInstructionReached && !ResizeCallEntered;
    public bool IsCleanCompletion => ReachedBoundedEndpoint && UnusedCaptures.IsEmpty;
}

/// <summary>
/// Projects the caller's post-constructor path only through the instruction immediately
/// before its resize-helper call. The virtual call remains opaque, and neither its target
/// body nor the resize helper's implementation is modeled. Slot-C values are generic
/// captured scalars; no method-name mapping or target validation is implied. The constructor
/// writes the initializer vtable before its opaque resize-helper call, so that static value
/// is informational only and is never substituted for the caller's fresh vtable read.
/// </summary>
public static class GameSocketParserLocalSizeDispatchProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;
    public const uint WrapperDataRawToVirtualAddressDelta = 0x00401400;

    public const uint CallerEvidenceStartRawFileOffset = 0x00BD31B0;
    public const uint InitialLocalLeaRawFileOffset = 0x00BD31C5;
    public const uint FreshLocalVtableLoadRawFileOffset = 0x00BD31C9;
    public const uint FreshSlotCIndirectCallRawFileOffset = 0x00BD31CD;
    public const uint FirstPushRawFileOffset = 0x00BD31D0;
    public const uint SecondPushRawFileOffset = 0x00BD31D2;
    public const uint ResizeLocalLeaRawFileOffset = 0x00BD31D3;
    public const uint ResizeCallRawFileOffset = 0x00BD31D7;

    public const uint OpaqueConstructorEntryRawFileOffset = 0x0012CB830;
    public const uint ConstructorVtableWriteRawFileOffset = 0x0012CB860;
    public const uint ConstructorOpaqueResizeCallRawFileOffset = 0x0012CB884;
    public const uint OpaqueResizeHelperTargetRawFileOffset =
        UiShaderSourceWrapperNativeLayout.PreserveRawHelperEntryRawFileOffset;
    public const uint ConstructorEvidenceStartRawFileOffset = 0x0012CB854;

    public const int InitialLocalDisplacementFromESP = 0x34;
    public const int SlotCDisplacement = UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCOffset;
    public const uint ResizePreserveArgument = UiShaderSourceWrapperNativeLayout.PreservingResizePreserveArgument;
    public const int PushWidth = sizeof(uint);
    public const int ResizeStackArgumentBytes = 2 * PushWidth;
    public const int ResizeLocalDisplacementFromPostPushESP = 0x3C;
    public const int ResizeLocalDisplacementFromPostGetterESP = 0x34;
    public const bool OpaqueIndirectTargetExecutionModeled = false;

    public const string CapturedCallerBytes =
        "50 FF 75 08 8D 4C 24 3C E8 73 86 6F 00 C7 44 24 50 00 00 00 00 "
        + "8D 4C 24 34 8B 44 24 34 FF 50 0C 6A 01 50 8D 4C 24 3C "
        + "E8 D4 8A 6F 00";
    public const string CapturedConstructorBytes =
        "8B F1 89 75 F0 C7 45 FC 00 00 00 00 C7 06 C4 0C C8 01 "
        + "C7 46 04 00 00 00 00 C7 46 08 00 00 00 00 6A 00 FF 75 0C "
        + "C6 45 FC 01 C7 46 0C 00 00 00 00 E8 27 04 00 00";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserLocalSizeDispatchPlan Project(
        GameSocketParserLocalSizeDispatchCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<GameSocketParserLocalSizeDispatchRead>();
        var pushes = ImmutableArray.CreateBuilder<GameSocketParserLocalSizeDispatchPush>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketParserLocalSizeDispatchMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        uint? initialLocalAddress = null;
        uint? freshVtableReadAddress = null;
        uint? freshSlotCReadAddress = null;
        uint? postGetterStackAddress = null;
        uint? stackAddressAfterFirstPush = null;
        uint? stackAddressAfterSecondPush = null;
        uint? resizeLocalAddress = null;
        GameSocketParserLocalSizeDispatchIndirectCall? getterCall = null;
        GameSocketParserLocalSizeDispatchOutcome outcome;
        uint stopBefore;
        GameSocketParserLocalSizeDispatchFlags? flagsAtStop;

        if (capture.PostConstructorValidated is null)
        {
            outcome = GameSocketParserLocalSizeDispatchOutcome.MissingPostConstructorValidation;
            stopBefore = InitialLocalLeaRawFileOffset;
            flagsAtStop = capture.InheritedEntryFlags;
        }
        else if (!capture.PostConstructorValidated.Value)
        {
            outcome = GameSocketParserLocalSizeDispatchOutcome.PostConstructorPathNotValidated;
            stopBefore = InitialLocalLeaRawFileOffset;
            flagsAtStop = capture.InheritedEntryFlags;
        }
        else if (capture.EntryESPAtInitialLocalLea is not uint entryEsp)
        {
            outcome = GameSocketParserLocalSizeDispatchOutcome.MissingEntryESPAtInitialLocalLea;
            stopBefore = InitialLocalLeaRawFileOffset;
            flagsAtStop = capture.InheritedEntryFlags;
        }
        else
        {
            reached.Add(InitialLocalLeaRawFileOffset);
            initialLocalAddress = unchecked(entryEsp + InitialLocalDisplacementFromESP);
            freshVtableReadAddress = initialLocalAddress;
            reached.Add(FreshLocalVtableLoadRawFileOffset);
            reads.Add(new GameSocketParserLocalSizeDispatchRead(
                Kind: GameSocketParserLocalSizeDispatchReadKind.FreshLocalVtable,
                InstructionRawFileOffset: FreshLocalVtableLoadRawFileOffset,
                Address: freshVtableReadAddress.Value,
                Value: capture.FreshLocalVtable,
                Attempted: true,
                Captured: capture.FreshLocalVtable.HasValue,
                Committed: false));
            memoryEffects.Add(new GameSocketParserLocalSizeDispatchMemoryEffect(
                Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshLocalVtableRead,
                InstructionRawFileOffset: FreshLocalVtableLoadRawFileOffset,
                Address: freshVtableReadAddress.Value,
                Value: capture.FreshLocalVtable,
                Attempted: true,
                Captured: capture.FreshLocalVtable.HasValue,
                Committed: false));

            if (capture.FreshLocalVtable is not uint freshVtable)
            {
                outcome = GameSocketParserLocalSizeDispatchOutcome.MissingFreshLocalVtable;
                stopBefore = FreshLocalVtableLoadRawFileOffset;
                flagsAtStop = capture.InheritedEntryFlags;
            }
            else
            {
                freshSlotCReadAddress = unchecked(
                    freshVtable + (uint)SlotCDisplacement);
                reached.Add(FreshSlotCIndirectCallRawFileOffset);
                reads.Add(new GameSocketParserLocalSizeDispatchRead(
                    Kind: GameSocketParserLocalSizeDispatchReadKind.FreshSlotCIndirectTarget,
                    InstructionRawFileOffset: FreshSlotCIndirectCallRawFileOffset,
                    Address: freshSlotCReadAddress.Value,
                    Value: capture.FreshSlotCIndirectTarget,
                    Attempted: true,
                    Captured: capture.FreshSlotCIndirectTarget.HasValue,
                    Committed: false));
                memoryEffects.Add(new GameSocketParserLocalSizeDispatchMemoryEffect(
                    Kind:
                        GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshSlotCIndirectTargetRead,
                    InstructionRawFileOffset: FreshSlotCIndirectCallRawFileOffset,
                    Address: freshSlotCReadAddress.Value,
                    Value: capture.FreshSlotCIndirectTarget,
                    Attempted: true,
                    Captured: capture.FreshSlotCIndirectTarget.HasValue,
                    Committed: false));

                if (capture.FreshSlotCIndirectTarget is not uint getterTarget)
                {
                    outcome =
                        GameSocketParserLocalSizeDispatchOutcome.MissingFreshSlotCIndirectTarget;
                    stopBefore = FreshSlotCIndirectCallRawFileOffset;
                    flagsAtStop = capture.InheritedEntryFlags;
                }
                else
                {
                    getterCall = new GameSocketParserLocalSizeDispatchIndirectCall(
                        InstructionRawFileOffset: FreshSlotCIndirectCallRawFileOffset,
                        SlotAddress: freshSlotCReadAddress.Value,
                        Target: getterTarget,
                        CallSiteReached: true,
                        TargetExecutionModeled: false);

                    if (capture.OpaqueGetterReturnedNormally is null)
                    {
                        outcome =
                            GameSocketParserLocalSizeDispatchOutcome.MissingOpaqueGetterReturnStatus;
                        stopBefore = FreshSlotCIndirectCallRawFileOffset;
                        flagsAtStop = capture.InheritedEntryFlags;
                    }
                    else if (!capture.OpaqueGetterReturnedNormally.Value)
                    {
                        outcome =
                            GameSocketParserLocalSizeDispatchOutcome.OpaqueGetterDidNotReturnNormally;
                        stopBefore = FreshSlotCIndirectCallRawFileOffset;
                        flagsAtStop = capture.InheritedEntryFlags;
                    }
                    else if (capture.PostGetterESP is not uint postGetterEsp)
                    {
                        outcome =
                            GameSocketParserLocalSizeDispatchOutcome.MissingPostGetterESPBeforeFirstPush;
                        stopBefore = FirstPushRawFileOffset;
                        flagsAtStop = GameSocketParserLocalSizeDispatchFlags.Unknown;
                    }
                    else
                    {
                        postGetterStackAddress = postGetterEsp;
                        reached.Add(FirstPushRawFileOffset);
                        stackAddressAfterFirstPush = unchecked(postGetterEsp - PushWidth);
                        pushes.Add(new GameSocketParserLocalSizeDispatchPush(
                            InstructionRawFileOffset: FirstPushRawFileOffset,
                            StackAddress: stackAddressAfterFirstPush.Value,
                            Value: ResizePreserveArgument,
                            Attempted: true,
                            Committed: true));
                        memoryEffects.Add(new GameSocketParserLocalSizeDispatchMemoryEffect(
                            Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.StackPush,
                            InstructionRawFileOffset: FirstPushRawFileOffset,
                            Address: stackAddressAfterFirstPush.Value,
                            Value: ResizePreserveArgument,
                            Attempted: true,
                            Captured: true,
                            Committed: true));

                        if (capture.OpaqueGetterReturnedEAX is not uint getterEax)
                        {
                            outcome =
                                GameSocketParserLocalSizeDispatchOutcome.MissingGetterEAXBeforeSecondPush;
                            stopBefore = SecondPushRawFileOffset;
                            flagsAtStop = GameSocketParserLocalSizeDispatchFlags.Unknown;
                        }
                        else
                        {
                            reached.Add(SecondPushRawFileOffset);
                            stackAddressAfterSecondPush = unchecked(
                                postGetterEsp - ResizeStackArgumentBytes);
                            pushes.Add(new GameSocketParserLocalSizeDispatchPush(
                                InstructionRawFileOffset: SecondPushRawFileOffset,
                                StackAddress: stackAddressAfterSecondPush.Value,
                                Value: getterEax,
                                Attempted: true,
                                Committed: true));
                            memoryEffects.Add(new GameSocketParserLocalSizeDispatchMemoryEffect(
                                Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.StackPush,
                                InstructionRawFileOffset: SecondPushRawFileOffset,
                                Address: stackAddressAfterSecondPush.Value,
                                Value: getterEax,
                                Attempted: true,
                                Captured: true,
                                Committed: true));

                            reached.Add(ResizeLocalLeaRawFileOffset);
                            resizeLocalAddress = unchecked(
                                stackAddressAfterSecondPush.Value
                                + (uint)ResizeLocalDisplacementFromPostPushESP);
                            outcome = GameSocketParserLocalSizeDispatchOutcome.BoundedBeforeResizeCall;
                            stopBefore = ResizeCallRawFileOffset;
                            flagsAtStop = GameSocketParserLocalSizeDispatchFlags.Unknown;
                        }
                    }
                }
            }
        }

        GameSocketParserLocalSizeDispatchCaptureStates captureStates =
            GetCaptureStates(capture);
        ImmutableArray<GameSocketParserLocalSizeDispatchCaptureKind> unused =
            GetUnusedCaptures(captureStates);
        return new GameSocketParserLocalSizeDispatchPlan(
            capture,
            captureStates,
            unused,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            memoryEffects.ToImmutable(),
            reached.ToImmutable(),
            outcome,
            new GameSocketParserLocalSizeDispatchAddresses(
                initialLocalAddress,
                freshVtableReadAddress,
                freshSlotCReadAddress,
                postGetterStackAddress,
                stackAddressAfterFirstPush,
                stackAddressAfterSecondPush,
                resizeLocalAddress),
            getterCall,
            flagsAtStop,
            stopBefore);
    }

    private static GameSocketParserLocalSizeDispatchCaptureStates GetCaptureStates(
        GameSocketParserLocalSizeDispatchCapture capture)
    {
        bool validated = capture.PostConstructorValidated == true;
        bool initialLeaReached = validated && capture.EntryESPAtInitialLocalLea.HasValue;
        bool vtableReadReached = initialLeaReached;
        bool slotReadReached = vtableReadReached && capture.FreshLocalVtable.HasValue;
        bool getterCallReached = slotReadReached && capture.FreshSlotCIndirectTarget.HasValue;
        bool getterReturnedNormally =
            getterCallReached && capture.OpaqueGetterReturnedNormally == true;

        return new GameSocketParserLocalSizeDispatchCaptureStates(
            State(capture.EntryESPAtInitialLocalLea.HasValue, validated),
            State(capture.FreshLocalVtable.HasValue, vtableReadReached),
            State(capture.FreshSlotCIndirectTarget.HasValue, slotReadReached),
            State(capture.OpaqueGetterReturnedNormally.HasValue, getterCallReached),
            State(capture.OpaqueGetterReturnedEAX.HasValue, getterReturnedNormally),
            State(capture.PostGetterESP.HasValue, getterReturnedNormally));
    }

    private static GameSocketParserLocalSizeDispatchCaptureState State(
        bool hasValue,
        bool reached) =>
        hasValue
            ? reached
                ? GameSocketParserLocalSizeDispatchCaptureState.Captured
                : GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            : reached
                ? GameSocketParserLocalSizeDispatchCaptureState.Missing
                : GameSocketParserLocalSizeDispatchCaptureState.NotReached;

    private static ImmutableArray<GameSocketParserLocalSizeDispatchCaptureKind> GetUnusedCaptures(
        GameSocketParserLocalSizeDispatchCaptureStates states)
    {
        var unused = ImmutableArray.CreateBuilder<GameSocketParserLocalSizeDispatchCaptureKind>();
        AddIfUnused(states.EntryESPAtInitialLocalLea,
            GameSocketParserLocalSizeDispatchCaptureKind.EntryESPAtInitialLocalLea, unused);
        AddIfUnused(states.FreshLocalVtable,
            GameSocketParserLocalSizeDispatchCaptureKind.FreshLocalVtable, unused);
        AddIfUnused(states.FreshSlotCIndirectTarget,
            GameSocketParserLocalSizeDispatchCaptureKind.FreshSlotCIndirectTarget, unused);
        AddIfUnused(states.OpaqueGetterReturnedNormally,
            GameSocketParserLocalSizeDispatchCaptureKind.OpaqueGetterReturnedNormally, unused);
        AddIfUnused(states.OpaqueGetterReturnedEAX,
            GameSocketParserLocalSizeDispatchCaptureKind.OpaqueGetterReturnedEAX, unused);
        AddIfUnused(states.PostGetterESP,
            GameSocketParserLocalSizeDispatchCaptureKind.PostGetterESP, unused);
        return unused.ToImmutable();
    }

    private static void AddIfUnused(
        GameSocketParserLocalSizeDispatchCaptureState state,
        GameSocketParserLocalSizeDispatchCaptureKind kind,
        ImmutableArray<GameSocketParserLocalSizeDispatchCaptureKind>.Builder unused)
    {
        if (state == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused)
            unused.Add(kind);
    }
}
