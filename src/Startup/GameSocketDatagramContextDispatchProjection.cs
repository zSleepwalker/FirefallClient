using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketDatagramContextDispatchStop
{
    ParentDidNotReachExactCleanCommonTail,
    AwaitingFreshSocketEsiBeforeContextCompare,
    AwaitingFreshContextWordAtCompare,
    AwaitingOriginalPacketCountBeforeLength,
    AwaitingZeroParserReturn,
    ZeroParserDidNotReturnNormally,
    ZeroParserReturnedAtBoundedD7,
    AwaitingNonzeroParserReturn,
    NonzeroParserDidNotReturnNormally,
    AwaitingFreshHighWaterAtCompare,
    HighWaterAtLeastPacketCountAtBounded3130,
    HighWaterBelowPacketCountAtBoundedF2
}

public enum GameSocketDatagramContextDispatchInput
{
    OriginalPacketCount,
    FreshContextWord,
    ZeroParserReturnedNormally,
    NonzeroParserReturnedNormally,
    FreshHighWaterDword
}

public enum GameSocketDatagramContextDispatchInstructionKind
{
    CompareFreshContextWord,
    ComputeParserLength,
    PushParserLength,
    LoadParserBufferAddress,
    MoveEcxFromSocketEsi,
    PushParserBufferAddress,
    BranchIfFreshContextNonzero,
    CallZeroContextParser,
    CallNonzeroContextParser,
    CompareFreshHighWater,
    BranchIfHighWaterAtLeastPacketCount,
    StoreFreshHighWater
}

public enum GameSocketDatagramContextDispatchMemoryEffectKind
{
    ParentBindingFlagRead,
    ParentBindingFlagClear,
    FreshContextWordRead,
    FreshHighWaterRead,
    FreshHighWaterWrite
}

public readonly record struct GameSocketDatagramContextDispatchFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct GameSocketDatagramContextDispatchInstruction(
    GameSocketDatagramContextDispatchInstructionKind Kind,
    uint FileOffset,
    uint? TargetFileOffset = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null);

public readonly record struct GameSocketDatagramContextDispatchMemoryEffect(
    GameSocketDatagramContextDispatchMemoryEffectKind Kind,
    uint FileOffset,
    uint? Address,
    uint? Value,
    bool? ReadCommitted,
    bool? WriteCommitted);

/// <summary>
/// Bounded offline projection of the datagram-context dispatch reached only
/// from the sender-binding projection's exact clean common tail.
/// </summary>
public static class GameSocketDatagramContextDispatchProjection
{
    public const uint MethodFileOffset = 0xBD30BC;
    public const uint FreshContextCompareFileOffset = 0xBD30BC;
    public const uint ParserLengthLoadFileOffset = 0xBD30C3;
    public const uint ParserLengthPushFileOffset = 0xBD30C6;
    public const uint ParserBufferAddressLoadFileOffset = 0xBD30C7;
    public const uint SocketEcxLoadFileOffset = 0xBD30CD;
    public const uint ParserBufferPushFileOffset = 0xBD30CF;
    public const uint NonzeroContextBranchFileOffset = 0xBD30D0;
    public const uint ZeroContextParserCallFileOffset = 0xBD30D2;
    public const uint ZeroParserReturnFileOffset = 0xBD30D7;
    public const uint NonzeroContextParserCallFileOffset = 0xBD30DF;
    public const uint FreshHighWaterCompareFileOffset = 0xBD30E4;
    public const uint HighWaterBranchFileOffset = 0xBD30EA;
    public const uint FreshHighWaterStoreFileOffset = 0xBD30EC;
    public const uint BelowHighWaterBoundedFileOffset = 0xBD30F2;
    public const uint AtOrAboveHighWaterBoundedFileOffset = 0xBD3130;
    public const uint ZeroContextParserFileOffset = 0xBD39D0;
    public const uint NonzeroContextParserFileOffset = 0xBD3140;
    public const int FreshContextWordOffset = 0x114;
    public const int ParserBufferOffset = 0x27C;
    public const int FreshHighWaterOffset = 0xD88;

    /// <summary>
    /// Reprojects the parent from its immutable captures; a caller cannot supply
    /// a forged parent result to enter this internal tail.
    /// </summary>
    public static GameSocketDatagramContextDispatchProjectionResult Project(
        GameSocketSenderBindingProjectionInputs parentInputs,
        uint? originalPacketCount,
        uint? freshContextWord,
        bool? zeroParserReturnedNormally,
        bool? nonzeroParserReturnedNormally,
        uint? freshHighWaterDword)
    {
        var inputs = new GameSocketDatagramContextDispatchProjectionInputs(
            parentInputs,
            originalPacketCount,
            freshContextWord,
            zeroParserReturnedNormally,
            nonzeroParserReturnedNormally,
            freshHighWaterDword);
        var parent = GameSocketSenderBindingProjection.Project(
            parentInputs.Admission,
            parentInputs.BindingFlag,
            parentInputs.SocketEsi,
            parentInputs.SenderEbx,
            parentInputs.ReceiverArgumentHelperReturnedNormally,
            parentInputs.PredicateValueHelperReturnedNormally,
            parentInputs.PredicateValueHelperAl,
            parentInputs.FallbackHelperReturnedNormally,
            parentInputs.QueueAppendReturnedNormally);
        bool enteredExactTail = parent.IsCleanCompletion
            && parent.ReachedCommonTail
            && parent.StopAtRaw == GameSocketSenderBindingProjection.CommonTailFileOffset;
        var instructions = ImmutableArray.CreateBuilder<GameSocketDatagramContextDispatchInstruction>();
        var consumed = new HashSet<GameSocketDatagramContextDispatchInput>();
        uint? contextAddress = null;
        uint? contextReadValue = null;
        uint? parserLength = null;
        uint? parserBufferAddress = null;
        uint? parserTarget = null;
        uint? highWaterAddress = null;
        uint? highWaterValue = null;
        uint? highWaterStoreValue = null;
        GameSocketDatagramContextDispatchFlags? contextFlags = null;
        GameSocketDatagramContextDispatchFlags? highWaterFlags = null;
        GameSocketDatagramContextDispatchFlags? flagsAtStop =
            parent.FlagsAtStop is { } parentFlags
                ? new(parentFlags.Carry, parentFlags.Parity, parentFlags.AuxiliaryCarry,
                    parentFlags.Zero, parentFlags.Sign, parentFlags.Overflow)
                : null;
        bool contextReadAttempted = false;
        bool parserCallAttempted = false;
        bool? parserReturnedNormally = null;
        bool highWaterReadAttempted = false;
        bool highWaterStoreAttempted = false;
        bool highWaterStoreCommitted = false;

        GameSocketDatagramContextDispatchProjectionResult Finish(
            GameSocketDatagramContextDispatchStop stop,
            uint? stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused = ImmutableArray.CreateBuilder<GameSocketDatagramContextDispatchInput>();
            var memoryEffects = ImmutableArray.CreateBuilder<GameSocketDatagramContextDispatchMemoryEffect>();
            foreach (var input in Enum.GetValues<GameSocketDatagramContextDispatchInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            foreach (var instruction in parent.InstructionOrder)
            {
                if (instruction.Kind == GameSocketSenderBindingInstructionKind.CompareBindingFlag)
                {
                    memoryEffects.Add(new(
                        GameSocketDatagramContextDispatchMemoryEffectKind.ParentBindingFlagRead,
                        instruction.FileOffset,
                        parent.FlagReadAddress,
                        parent.FlagReadValue,
                        parent.FlagReadCommitted,
                        null));
                }
                else if (instruction.Kind == GameSocketSenderBindingInstructionKind.ClearBindingFlag)
                {
                    memoryEffects.Add(new(
                        GameSocketDatagramContextDispatchMemoryEffectKind.ParentBindingFlagClear,
                        instruction.FileOffset,
                        parent.FlagClearAddress,
                        parent.FlagClearValue,
                        null,
                        parent.FlagClearCommitted));
                }
            }

            if (contextReadAttempted)
            {
                memoryEffects.Add(new(
                    GameSocketDatagramContextDispatchMemoryEffectKind.FreshContextWordRead,
                    FreshContextCompareFileOffset,
                    contextAddress,
                    contextReadValue,
                    false,
                    null));
            }

            if (highWaterReadAttempted)
            {
                memoryEffects.Add(new(
                    GameSocketDatagramContextDispatchMemoryEffectKind.FreshHighWaterRead,
                    FreshHighWaterCompareFileOffset,
                    highWaterAddress,
                    highWaterValue,
                    false,
                    null));
            }

            if (highWaterStoreAttempted)
            {
                memoryEffects.Add(new(
                    GameSocketDatagramContextDispatchMemoryEffectKind.FreshHighWaterWrite,
                    FreshHighWaterStoreFileOffset,
                    highWaterAddress,
                    highWaterStoreValue,
                    null,
                    highWaterStoreCommitted));
            }

            return new GameSocketDatagramContextDispatchProjectionResult(
                inputs,
                parent,
                enteredExactTail,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                unused.ToImmutable(),
                memoryEffects.ToImmutable(),
                contextReadAttempted,
                contextAddress,
                contextReadValue,
                contextFlags,
                parserLength,
                parserBufferAddress,
                parserCallAttempted,
                parserTarget,
                parserReturnedNormally,
                flagsAtStop,
                highWaterReadAttempted,
                highWaterAddress,
                highWaterValue,
                highWaterFlags,
                highWaterStoreAttempted,
                highWaterStoreCommitted,
                highWaterStoreValue,
                reachedBoundedEndpoint);
        }

        if (!enteredExactTail)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.ParentDidNotReachExactCleanCommonTail,
                parent.StopAtRaw);
        }

        if (parentInputs.SocketEsi is not uint socket)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.AwaitingFreshSocketEsiBeforeContextCompare,
                FreshContextCompareFileOffset);
        }

        contextAddress = unchecked(socket + (uint)FreshContextWordOffset);
        contextReadAttempted = true;
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.CompareFreshContextWord,
            FreshContextCompareFileOffset,
            EffectiveAddress: contextAddress));
        if (inputs.FreshContextWord is not uint contextWord)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.AwaitingFreshContextWordAtCompare,
                FreshContextCompareFileOffset);
        }

        consumed.Add(GameSocketDatagramContextDispatchInput.FreshContextWord);
        contextReadValue = contextWord;
        contextFlags = GetCompareFlags(contextWord);
        flagsAtStop = contextFlags;

        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.ComputeParserLength,
            ParserLengthLoadFileOffset,
            OperandValue: inputs.OriginalPacketCount));
        if (inputs.OriginalPacketCount is not uint originalCount)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.AwaitingOriginalPacketCountBeforeLength,
                ParserLengthLoadFileOffset);
        }

        consumed.Add(GameSocketDatagramContextDispatchInput.OriginalPacketCount);
        parserLength = unchecked(originalCount - sizeof(uint));
        instructions[^1] = instructions[^1] with { OperandValue = originalCount };
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.PushParserLength,
            ParserLengthPushFileOffset,
            OperandValue: parserLength));
        parserBufferAddress = unchecked(socket + (uint)ParserBufferOffset);
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.LoadParserBufferAddress,
            ParserBufferAddressLoadFileOffset,
            EffectiveAddress: parserBufferAddress));
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.MoveEcxFromSocketEsi,
            SocketEcxLoadFileOffset,
            OperandValue: socket));
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.PushParserBufferAddress,
            ParserBufferPushFileOffset,
            OperandValue: parserBufferAddress));

        bool isNonzeroContext = contextWord != 0;
        parserTarget = isNonzeroContext
            ? NonzeroContextParserFileOffset
            : ZeroContextParserFileOffset;
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.BranchIfFreshContextNonzero,
            NonzeroContextBranchFileOffset,
            NonzeroContextParserCallFileOffset,
            BranchTaken: isNonzeroContext));
        parserCallAttempted = true;
        bool? parserReturnCapture = isNonzeroContext
            ? inputs.NonzeroParserReturnedNormally
            : inputs.ZeroParserReturnedNormally;
        uint parserCallOffset = isNonzeroContext
            ? NonzeroContextParserCallFileOffset
            : ZeroContextParserCallFileOffset;
        instructions.Add(new(
            isNonzeroContext
                ? GameSocketDatagramContextDispatchInstructionKind.CallNonzeroContextParser
                : GameSocketDatagramContextDispatchInstructionKind.CallZeroContextParser,
            parserCallOffset,
            TargetFileOffset: parserTarget));

        if (parserReturnCapture is not bool returnedNormally)
        {
            return Finish(
                isNonzeroContext
                    ? GameSocketDatagramContextDispatchStop.AwaitingNonzeroParserReturn
                    : GameSocketDatagramContextDispatchStop.AwaitingZeroParserReturn,
                parserCallOffset);
        }

        consumed.Add(isNonzeroContext
            ? GameSocketDatagramContextDispatchInput.NonzeroParserReturnedNormally
            : GameSocketDatagramContextDispatchInput.ZeroParserReturnedNormally);
        parserReturnedNormally = returnedNormally;
        if (!returnedNormally)
        {
            return Finish(
                isNonzeroContext
                    ? GameSocketDatagramContextDispatchStop.NonzeroParserDidNotReturnNormally
                    : GameSocketDatagramContextDispatchStop.ZeroParserDidNotReturnNormally,
                parserCallOffset);
        }

        flagsAtStop = null;
        if (!isNonzeroContext)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.ZeroParserReturnedAtBoundedD7,
                ZeroParserReturnFileOffset,
                true);
        }

        highWaterAddress = unchecked(socket + (uint)FreshHighWaterOffset);
        highWaterReadAttempted = true;
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.CompareFreshHighWater,
            FreshHighWaterCompareFileOffset,
            EffectiveAddress: highWaterAddress,
            OperandValue: originalCount));
        if (inputs.FreshHighWaterDword is not uint observedHighWater)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.AwaitingFreshHighWaterAtCompare,
                FreshHighWaterCompareFileOffset);
        }

        consumed.Add(GameSocketDatagramContextDispatchInput.FreshHighWaterDword);
        highWaterValue = observedHighWater;
        highWaterFlags = GetCompareFlags(observedHighWater, originalCount);
        flagsAtStop = highWaterFlags;
        bool highWaterAtLeastPacketCount = observedHighWater >= originalCount;
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.BranchIfHighWaterAtLeastPacketCount,
            HighWaterBranchFileOffset,
            AtOrAboveHighWaterBoundedFileOffset,
            BranchTaken: highWaterAtLeastPacketCount));
        if (highWaterAtLeastPacketCount)
        {
            return Finish(
                GameSocketDatagramContextDispatchStop.HighWaterAtLeastPacketCountAtBounded3130,
                AtOrAboveHighWaterBoundedFileOffset,
                true);
        }

        highWaterStoreAttempted = true;
        highWaterStoreCommitted = true;
        highWaterStoreValue = originalCount;
        instructions.Add(new(
            GameSocketDatagramContextDispatchInstructionKind.StoreFreshHighWater,
            FreshHighWaterStoreFileOffset,
            EffectiveAddress: highWaterAddress,
            OperandValue: originalCount));
        return Finish(
            GameSocketDatagramContextDispatchStop.HighWaterBelowPacketCountAtBoundedF2,
            BelowHighWaterBoundedFileOffset,
            true);
    }

    public static GameSocketDatagramContextDispatchFlags GetCompareFlags(uint left, uint right = 0)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (System.Numerics.BitOperations.PopCount(result & 0xFF) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x8000_0000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x8000_0000) != 0);
    }

    private static bool IsSupplied(
        GameSocketDatagramContextDispatchProjectionInputs inputs,
        GameSocketDatagramContextDispatchInput input) => input switch
    {
        GameSocketDatagramContextDispatchInput.OriginalPacketCount =>
            inputs.OriginalPacketCount.HasValue,
        GameSocketDatagramContextDispatchInput.FreshContextWord =>
            inputs.FreshContextWord.HasValue,
        GameSocketDatagramContextDispatchInput.ZeroParserReturnedNormally =>
            inputs.ZeroParserReturnedNormally.HasValue,
        GameSocketDatagramContextDispatchInput.NonzeroParserReturnedNormally =>
            inputs.NonzeroParserReturnedNormally.HasValue,
        GameSocketDatagramContextDispatchInput.FreshHighWaterDword =>
            inputs.FreshHighWaterDword.HasValue,
        _ => false
    };
}

public sealed record GameSocketDatagramContextDispatchProjectionInputs(
    GameSocketSenderBindingProjectionInputs ParentInputs,
    uint? OriginalPacketCount,
    uint? FreshContextWord,
    bool? ZeroParserReturnedNormally,
    bool? NonzeroParserReturnedNormally,
    uint? FreshHighWaterDword);

public sealed record GameSocketDatagramContextDispatchProjectionResult(
    GameSocketDatagramContextDispatchProjectionInputs Inputs,
    GameSocketSenderBindingProjectionResult ParentProjection,
    bool ParentExactCleanCommonTailEntered,
    GameSocketDatagramContextDispatchStop Stop,
    uint? StopAtRaw,
    ImmutableArray<GameSocketDatagramContextDispatchInstruction> InstructionOrder,
    ImmutableArray<GameSocketDatagramContextDispatchInput> SuppliedButUnusedInputs,
    ImmutableArray<GameSocketDatagramContextDispatchMemoryEffect> OrderedMemoryEffects,
    bool FreshContextReadAttempted,
    uint? FreshContextReadAddress,
    uint? FreshContextReadValue,
    GameSocketDatagramContextDispatchFlags? FlagsAfterFreshContextCompare,
    uint? ParserLength,
    uint? ParserBufferAddress,
    bool ParserCallAttempted,
    uint? ParserTargetFileOffset,
    bool? ParserReturnedNormally,
    GameSocketDatagramContextDispatchFlags? FlagsAtStop,
    bool FreshHighWaterReadAttempted,
    uint? FreshHighWaterReadAddress,
    uint? FreshHighWaterReadValue,
    GameSocketDatagramContextDispatchFlags? FlagsAfterFreshHighWaterCompare,
    bool FreshHighWaterStoreAttempted,
    bool FreshHighWaterStoreCommitted,
    uint? FreshHighWaterStoreValue,
    bool ReachedBoundedEndpoint)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool IsCleanCompletion => ReachedBoundedEndpoint && !HasSuppliedButUnusedInputs;

    public bool ExactBoundedCompletion => ParentExactCleanCommonTailEntered
        ? ReachedBoundedEndpoint
        : ParentProjection.ReachedCommonTail;

    public bool FreshContextReadCommitted => false;

    public bool FreshHighWaterReadCommitted => false;

    /// <summary>This offline projection never enters a native bounded target or models RET.</summary>
    public bool BoundedTargetEntered => false;

    public bool ReturnInstructionModeled => false;

    /// <summary>This offline projection never executes an opaque native parser.</summary>
    public bool OpaqueParsersExecuted => false;
}
