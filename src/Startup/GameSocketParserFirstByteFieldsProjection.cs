using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserFirstByteFieldsCapture(
    GameSocketParserPostResizePointerCapture ParentCapture,
    byte? FreshHeaderByte);

public enum GameSocketParserFirstByteFieldsCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserFirstByteFieldsCaptureKind
{
    FreshHeaderByte
}

public enum GameSocketParserFirstByteFieldsOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshHeaderByte,
    BoundedBeforeSecondHeaderByteLoad
}

public enum GameSocketParserFirstByteFieldsReadKind
{
    FreshHeaderByte
}

public enum GameSocketParserFirstByteFieldsMemoryEffectKind
{
    ParentFreshLocalVtableRead,
    ParentFreshSlotCIndirectTargetRead,
    ParentStackPush,
    ParentFreshField38DwordRead,
    ParentFreshFieldPlus4DwordRead,
    ParentStackStore,
    FreshHeaderByteRead,
    StackStore
}

public enum GameSocketParserFirstByteFieldsStoreKind
{
    Bits5To4,
    Bit3
}

public readonly record struct GameSocketParserFirstByteFieldsRead(
    GameSocketParserFirstByteFieldsReadKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    byte? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserFirstByteFieldsStore(
    GameSocketParserFirstByteFieldsStoreKind Kind,
    uint InstructionRawFileOffset,
    uint StackAddress,
    byte Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserFirstByteFieldsMemoryEffect(
    GameSocketParserFirstByteFieldsMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserFirstByteFieldsFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed class GameSocketParserFirstByteFieldsPlan
{
    internal GameSocketParserFirstByteFieldsPlan(
        GameSocketParserFirstByteFieldsCapture capture,
        GameSocketParserPostResizePointerPlan parentPlan,
        GameSocketParserFirstByteFieldsCaptureState headerByteCaptureState,
        ImmutableArray<GameSocketParserFirstByteFieldsCaptureKind> unusedCaptures,
        ImmutableArray<GameSocketParserFirstByteFieldsRead> reads,
        ImmutableArray<GameSocketParserFirstByteFieldsStore> stores,
        ImmutableArray<GameSocketParserFirstByteFieldsMemoryEffect> memoryEffects,
        ImmutableArray<uint> reachedInstructionRawFileOffsets,
        GameSocketParserFirstByteFieldsOutcome outcome,
        bool parentHandoffAdmitted,
        uint? headerByteReadAddress,
        uint? firstStackStoreAddress,
        uint? secondStackStoreAddress,
        byte? resultAL,
        uint? resultEBX,
        uint? resultECX,
        byte? storedBits5To4,
        byte? storedBit3,
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrCL4,
        GameSocketParserFirstByteFieldsFlags? flagsAfterAndCL3,
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrEBX6,
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrCL3,
        GameSocketParserFirstByteFieldsFlags? flagsAfterAndAL7,
        GameSocketParserFirstByteFieldsFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        HeaderByteCaptureState = headerByteCaptureState;
        UnusedCaptures = unusedCaptures;
        Reads = reads;
        Stores = stores;
        MemoryEffects = memoryEffects;
        ReachedInstructionRawFileOffsets = reachedInstructionRawFileOffsets;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        HeaderByteReadAddress = headerByteReadAddress;
        FirstStackStoreAddress = firstStackStoreAddress;
        SecondStackStoreAddress = secondStackStoreAddress;
        ResultAL = resultAL;
        ResultEBX = resultEBX;
        ResultECX = resultECX;
        StoredBits5To4 = storedBits5To4;
        StoredBit3 = storedBit3;
        FlagsAfterShrCL4 = flagsAfterShrCL4;
        FlagsAfterAndCL3 = flagsAfterAndCL3;
        FlagsAfterShrEBX6 = flagsAfterShrEBX6;
        FlagsAfterShrCL3 = flagsAfterShrCL3;
        FlagsAfterAndAL7 = flagsAfterAndAL7;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserFirstByteFieldsCapture Capture { get; }
    public GameSocketParserPostResizePointerPlan ParentPlan { get; }
    public GameSocketParserFirstByteFieldsCaptureState HeaderByteCaptureState { get; }
    public ImmutableArray<GameSocketParserFirstByteFieldsCaptureKind> UnusedCaptures { get; }
    public ImmutableArray<GameSocketParserFirstByteFieldsRead> Reads { get; }
    public ImmutableArray<GameSocketParserFirstByteFieldsStore> Stores { get; }
    public ImmutableArray<GameSocketParserFirstByteFieldsMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserFirstByteFieldsMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets { get; }
    public GameSocketParserFirstByteFieldsOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? ParentResultEDX => ParentPlan.ResultEDX;
    public uint? InheritedFreshPostResizeESP => ParentPlan.Capture.FreshPostResizeESP;
    public uint? HeaderByteReadAddress { get; }
    public uint? FirstStackStoreAddress { get; }
    public uint? SecondStackStoreAddress { get; }
    public byte? ResultAL { get; }
    public uint? ResultEBX { get; }
    public uint? ResultECX { get; }
    public byte? ResultCL =>
        ResultECX is uint result ? (byte)result : null;
    public uint? ResultEAX => null;
    public byte? StoredBits5To4 { get; }
    public byte? StoredBit3 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAfterShrCL4 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAfterAndCL3 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAfterShrEBX6 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAfterShrCL3 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAfterAndAL7 { get; }
    public GameSocketParserFirstByteFieldsFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserFirstByteFieldsProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserFirstByteFieldsOutcome.BoundedBeforeSecondHeaderByteLoad
        && StopBeforeRawFileOffset
            == GameSocketParserFirstByteFieldsProjection.SecondHeaderByteLoadRawFileOffset;
    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentHandoffAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion && ParentPlan.IsCleanCompletion && UnusedCaptures.IsEmpty;
    public uint? SecondHeaderByteReadAddress =>
        ParentHandoffAdmitted && ParentResultEDX is uint address
            ? unchecked(address + 1)
            : null;
    public bool SecondHeaderByteReadAttempted => false;
    public bool HeaderAddressValidityEstablished => false;
    public bool FullEAXKnown => false;
    public bool TargetExecutionModeled => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
}

/// <summary>
/// Reprojects the clean bounded post-resize pointer capture, then records only the first
/// header-byte load, its scalar bit operations, and two byte stores. No parser semantics
/// or target code execution are modeled.
/// </summary>
public static class GameSocketParserFirstByteFieldsProjection
{
    public const uint HeaderByteLoadRawFileOffset = 0x00BD31F8;
    public const uint MoveCLFromALRawFileOffset = 0x00BD31FA;
    public const uint MoveZXEBXFromALRawFileOffset = 0x00BD31FC;
    public const uint ShrCL4RawFileOffset = 0x00BD31FF;
    public const uint AndCL3RawFileOffset = 0x00BD3202;
    public const uint ShrEBX6RawFileOffset = 0x00BD3205;
    public const uint FirstStackStoreRawFileOffset = 0x00BD3208;
    public const uint ReloadCLFromALRawFileOffset = 0x00BD320C;
    public const uint ShrCL3RawFileOffset = 0x00BD320E;
    public const uint AndAL7RawFileOffset = 0x00BD3211;
    public const uint AndCL1RawFileOffset = 0x00BD3213;
    public const uint SecondStackStoreRawFileOffset = 0x00BD3216;
    public const uint MoveZXECXFromALRawFileOffset = 0x00BD321A;
    public const uint SecondHeaderByteLoadRawFileOffset = 0x00BD321D;

    public const int FirstStackStoreDisplacement = 0x12;
    public const int SecondStackStoreDisplacement = 0x13;
    public const uint RawToCodeVirtualAddressDelta = 0x00400C00;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "8A 02 8A C8 0F B6 D8 C0 E9 04 80 E1 03 C1 EB 06 "
        + "88 4C 24 12 8A C8 C0 E9 03 24 07 80 E1 01 "
        + "88 4C 24 13 0F B6 C8";
    public const string SecondHeaderByteLoadBytes = "0F B6 42 01";

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + RawToCodeVirtualAddressDelta);

    public static uint CodeVirtualAddressToRawFileOffset(uint codeVirtualAddress) =>
        unchecked(codeVirtualAddress - RawToCodeVirtualAddressDelta);

    public static GameSocketParserFirstByteFieldsPlan Project(
        GameSocketParserFirstByteFieldsCapture capture)
    {
        GameSocketParserPostResizePointerPlan parentPlan =
            GameSocketParserPostResizePointerProjection.Project(capture.ParentCapture);
        var reads = ImmutableArray.CreateBuilder<GameSocketParserFirstByteFieldsRead>();
        var stores = ImmutableArray.CreateBuilder<GameSocketParserFirstByteFieldsStore>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserFirstByteFieldsMemoryEffect>();
        var reached = ImmutableArray.CreateBuilder<uint>();

        foreach (GameSocketParserPostResizePointerMemoryEffect parentEffect
            in parentPlan.MemoryEffects)
        {
            effects.Add(new GameSocketParserFirstByteFieldsMemoryEffect(
                Kind: parentEffect.Kind switch
                {
                    GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshLocalVtableRead =>
                        GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshLocalVtableRead,
                    GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshSlotCIndirectTargetRead =>
                        GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
                    GameSocketParserPostResizePointerMemoryEffectKind.ParentStackPush =>
                        GameSocketParserFirstByteFieldsMemoryEffectKind.ParentStackPush,
                    GameSocketParserPostResizePointerMemoryEffectKind.FreshField38DwordRead =>
                        GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshField38DwordRead,
                    GameSocketParserPostResizePointerMemoryEffectKind.FreshFieldPlus4DwordRead =>
                        GameSocketParserFirstByteFieldsMemoryEffectKind.ParentFreshFieldPlus4DwordRead,
                    _ => GameSocketParserFirstByteFieldsMemoryEffectKind.ParentStackStore
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
            && parentPlan.ResultEDX.HasValue
            && parentPlan.Capture.FreshPostResizeESP.HasValue;
        GameSocketParserFirstByteFieldsCaptureState captureState;
        GameSocketParserFirstByteFieldsOutcome outcome;
        uint? headerAddress = null;
        uint? firstStoreAddress = null;
        uint? secondStoreAddress = null;
        byte? resultAL = null;
        uint? resultEBX = null;
        uint? resultECX = null;
        byte? storedBits5To4 = null;
        byte? storedBit3 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrCL4 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAfterAndCL3 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrEBX6 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAfterShrCL3 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAfterAndAL7 = null;
        GameSocketParserFirstByteFieldsFlags? flagsAtStop;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome = GameSocketParserFirstByteFieldsOutcome.ParentHandoffNotAdmitted;
            captureState = capture.FreshHeaderByte.HasValue
                ? GameSocketParserFirstByteFieldsCaptureState.SuppliedButUnused
                : GameSocketParserFirstByteFieldsCaptureState.NotReached;
            flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
            stopBefore = parentPlan.StopBeforeRawFileOffset;
        }
        else
        {
            uint headerAddressValue = parentPlan.ResultEDX!.Value;
            headerAddress = headerAddressValue;
            reached.Add(HeaderByteLoadRawFileOffset);
            reads.Add(new GameSocketParserFirstByteFieldsRead(
                Kind: GameSocketParserFirstByteFieldsReadKind.FreshHeaderByte,
                InstructionRawFileOffset: HeaderByteLoadRawFileOffset,
                Address: headerAddressValue,
                Value: capture.FreshHeaderByte,
                Attempted: true,
                Captured: capture.FreshHeaderByte.HasValue,
                Committed: false));
            effects.Add(new GameSocketParserFirstByteFieldsMemoryEffect(
                Kind: GameSocketParserFirstByteFieldsMemoryEffectKind.FreshHeaderByteRead,
                InstructionRawFileOffset: HeaderByteLoadRawFileOffset,
                Address: headerAddressValue,
                Value: capture.FreshHeaderByte is byte headerByte ? headerByte : null,
                Attempted: true,
                Captured: capture.FreshHeaderByte.HasValue,
                Committed: false));

            if (capture.FreshHeaderByte is not byte value)
            {
                captureState = GameSocketParserFirstByteFieldsCaptureState.Missing;
                outcome = GameSocketParserFirstByteFieldsOutcome.MissingFreshHeaderByte;
                flagsAtStop = ToFlags(parentPlan.FlagsAtStop);
                stopBefore = HeaderByteLoadRawFileOffset;
            }
            else
            {
                captureState = GameSocketParserFirstByteFieldsCaptureState.Captured;
                reached.Add(MoveCLFromALRawFileOffset);
                reached.Add(MoveZXEBXFromALRawFileOffset);
                resultAL = value;
                resultEBX = value;

                reached.Add(ShrCL4RawFileOffset);
                flagsAfterShrCL4 = ShiftFlags(
                    (byte)(value >> 4),
                    (value & 0x08) != 0,
                    overflow: null);
                reached.Add(AndCL3RawFileOffset);
                byte highField = (byte)((value >> 4) & 0x03);
                storedBits5To4 = highField;
                flagsAfterAndCL3 = AndFlags(highField);

                reached.Add(ShrEBX6RawFileOffset);
                uint topField = (uint)(value >> 6);
                resultEBX = topField;
                flagsAfterShrEBX6 = ShiftFlags(
                    (byte)topField,
                    (value & 0x20) != 0,
                    overflow: null);

                reached.Add(FirstStackStoreRawFileOffset);
                uint esp = parentPlan.Capture.FreshPostResizeESP!.Value;
                firstStoreAddress =
                    unchecked(esp + (uint)FirstStackStoreDisplacement);
                stores.Add(new GameSocketParserFirstByteFieldsStore(
                    Kind: GameSocketParserFirstByteFieldsStoreKind.Bits5To4,
                    InstructionRawFileOffset: FirstStackStoreRawFileOffset,
                    StackAddress: firstStoreAddress.Value,
                    Value: highField,
                    Attempted: true,
                    Committed: true));
                effects.Add(new GameSocketParserFirstByteFieldsMemoryEffect(
                    Kind: GameSocketParserFirstByteFieldsMemoryEffectKind.StackStore,
                    InstructionRawFileOffset: FirstStackStoreRawFileOffset,
                    Address: firstStoreAddress.Value,
                    Value: highField,
                    Attempted: true,
                    Captured: true,
                    Committed: true));

                reached.Add(ReloadCLFromALRawFileOffset);
                reached.Add(ShrCL3RawFileOffset);
                flagsAfterShrCL3 = ShiftFlags(
                    (byte)(value >> 3),
                    (value & 0x04) != 0,
                    overflow: null);
                reached.Add(AndAL7RawFileOffset);
                byte lowField = (byte)(value & 0x07);
                resultAL = lowField;
                flagsAfterAndAL7 = AndFlags(lowField);
                reached.Add(AndCL1RawFileOffset);
                byte bit3Field = (byte)((value >> 3) & 0x01);
                storedBit3 = bit3Field;
                flagsAtStop = AndFlags(bit3Field);

                reached.Add(SecondStackStoreRawFileOffset);
                secondStoreAddress =
                    unchecked(esp + (uint)SecondStackStoreDisplacement);
                stores.Add(new GameSocketParserFirstByteFieldsStore(
                    Kind: GameSocketParserFirstByteFieldsStoreKind.Bit3,
                    InstructionRawFileOffset: SecondStackStoreRawFileOffset,
                    StackAddress: secondStoreAddress.Value,
                    Value: bit3Field,
                    Attempted: true,
                    Committed: true));
                effects.Add(new GameSocketParserFirstByteFieldsMemoryEffect(
                    Kind: GameSocketParserFirstByteFieldsMemoryEffectKind.StackStore,
                    InstructionRawFileOffset: SecondStackStoreRawFileOffset,
                    Address: secondStoreAddress.Value,
                    Value: bit3Field,
                    Attempted: true,
                    Captured: true,
                    Committed: true));

                reached.Add(MoveZXECXFromALRawFileOffset);
                resultECX = resultAL.Value;
                outcome =
                    GameSocketParserFirstByteFieldsOutcome.BoundedBeforeSecondHeaderByteLoad;
                stopBefore = SecondHeaderByteLoadRawFileOffset;
            }
        }

        ImmutableArray<GameSocketParserFirstByteFieldsCaptureKind> unusedCaptures =
            captureState == GameSocketParserFirstByteFieldsCaptureState.SuppliedButUnused
                ? [GameSocketParserFirstByteFieldsCaptureKind.FreshHeaderByte]
                : [];
        return new GameSocketParserFirstByteFieldsPlan(
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
            headerAddress,
            firstStoreAddress,
            secondStoreAddress,
            resultAL,
            resultEBX,
            resultECX,
            storedBits5To4,
            storedBit3,
            flagsAfterShrCL4,
            flagsAfterAndCL3,
            flagsAfterShrEBX6,
            flagsAfterShrCL3,
            flagsAfterAndAL7,
            flagsAtStop,
            stopBefore);
    }

    private static GameSocketParserFirstByteFieldsFlags? ToFlags(
        GameSocketParserPostResizePointerFlags? flags) =>
        flags is GameSocketParserPostResizePointerFlags value
            ? new GameSocketParserFirstByteFieldsFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserFirstByteFieldsFlags ShiftFlags(
        byte result,
        bool carry,
        bool? overflow) =>
        new(
            Carry: carry,
            Parity: HasEvenParity(result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80) != 0,
            Overflow: overflow);

    private static GameSocketParserFirstByteFieldsFlags AndFlags(byte result) =>
        new(
            Carry: false,
            Parity: HasEvenParity(result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;
}
