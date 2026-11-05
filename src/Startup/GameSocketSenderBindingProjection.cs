using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum GameSocketSenderBindingStop
{
    AdmissionRejected,
    AwaitingSocketEsiBeforeBindingCompare,
    AwaitingBindingFlagAtCompare,
    BindingDisabledAtCommonTail,
    AwaitingSenderEbxBeforePush,
    AwaitingReceiverArgumentHelperReturn,
    ReceiverArgumentHelperDidNotReturn,
    AwaitingPredicateValueHelperReturn,
    PredicateValueHelperDidNotReturn,
    AwaitingAlForTest,
    AwaitingFallbackHelperReturn,
    FallbackHelperDidNotReturn,
    AwaitingQueueAppendReturn,
    QueueAppendDidNotReturn,
    CompletedAtCommonTail
}

public enum GameSocketSenderBindingStep
{
    AdmissionAccepted,
    AdmissionRejected,
    SocketEsiMissing,
    AttemptBindingFlagRead,
    BindingFlagValueMissing,
    BindingFlagCompared,
    BranchBindingDisabled,
    BranchBindingEnabled,
    SenderEbxMissing,
    PushSenderEbx,
    ComputeReceiverAddress,
    CallReceiverArgumentHelper,
    ReceiverArgumentHelperReturnMissing,
    ReceiverArgumentHelperDidNotReturn,
    ReceiverArgumentHelperReturned,
    MoveEcxFromEsiForPredicateHelper,
    CallPredicateValueHelper,
    PredicateValueHelperReturnMissing,
    PredicateValueHelperDidNotReturn,
    PredicateValueHelperReturned,
    AttemptTestAl,
    AlValueMissing,
    SelectSenderBindingSucceededToken,
    SelectFallbackHelperPath,
    PushFallbackHelperArgument,
    CallFallbackHelper,
    FallbackHelperReturnMissing,
    FallbackHelperDidNotReturn,
    FallbackHelperReturned,
    SelectPumpCloseRequestToken,
    ComputeQueueAddress,
    CallQueueAppend,
    QueueAppendReturnMissing,
    QueueAppendDidNotReturn,
    QueueAppendReturned,
    AttemptClearBindingFlag,
    ClearBindingFlagCommitted,
    ReachCommonTail
}

public enum GameSocketSenderBindingInput
{
    Admission,
    BindingFlag,
    SocketEsi,
    SenderEbx,
    ReceiverArgumentHelperReturnedNormally,
    PredicateValueHelperReturnedNormally,
    PredicateValueHelperAl,
    FallbackHelperReturnedNormally,
    QueueAppendReturnedNormally
}

public enum GameSocketSenderBindingInstructionKind
{
    CompareBindingFlag,
    BranchIfBindingDisabled,
    PushSenderEbx,
    LoadReceiverArgumentAddress,
    CallReceiverArgumentHelper,
    MoveEcxFromEsi,
    CallPredicateValueHelper,
    TestAl,
    BranchIfAlZero,
    PushSenderBindingSucceededToken,
    JumpToQueueAppend,
    PushFallbackHelperArgument,
    CallFallbackHelper,
    PushPumpCloseRequestToken,
    LoadQueueAddress,
    CallQueueAppend,
    ClearBindingFlag
}

public readonly record struct GameSocketSenderBindingFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct GameSocketSenderBindingInstruction(
    GameSocketSenderBindingInstructionKind Kind,
    uint FileOffset,
    uint? TargetFileOffset = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null);

/// <summary>
/// Bounded, offline projection of the optional sender-binding branch. It starts
/// after the outer packet-admission classifier, requires the inherited socket
/// pointer before the flag comparison, and makes no claims about opaque helpers'
/// higher-level semantics.
/// </summary>
public static class GameSocketSenderBindingProjection
{
    public const uint MethodFileOffset = 0xBD307B;
    public const uint BindingFlagCompareFileOffset = 0xBD307B;
    public const uint BindingFlagBranchFileOffset = 0xBD3082;
    public const uint SenderEbxPushFileOffset = 0xBD3084;
    public const uint ReceiverAddressLoadFileOffset = 0xBD3085;
    public const uint ReceiverArgumentHelperCallFileOffset = 0xBD308B;
    public const uint PredicateHelperCallFileOffset = 0xBD3092;
    public const uint AlTestFileOffset = 0xBD3097;
    public const uint AlZeroBranchFileOffset = 0xBD3099;
    public const uint SenderBindingTokenPushFileOffset = 0xBD309B;
    public const uint SenderBindingTokenJumpFileOffset = 0xBD309D;
    public const uint FallbackArgumentPushFileOffset = 0xBD309F;
    public const uint FallbackHelperCallFileOffset = 0xBD30A3;
    public const uint PumpCloseTokenPushFileOffset = 0xBD30A8;
    public const uint QueueAddressLoadFileOffset = 0xBD30AA;
    public const uint QueueAppendHelperCallFileOffset = 0xBD30B0;
    public const uint BindingFlagClearFileOffset = 0xBD30B5;
    public const uint CommonTailFileOffset = 0xBD30BC;
    public const uint ReceiverArgumentHelperFileOffset = 0x483640;
    public const uint PredicateHelperFileOffset = 0xBD4A20;
    public const uint FallbackHelperFileOffset = 0xBD49F0;
    public const uint QueueAppendHelperFileOffset = 0x12CE310;
    public const int BindingFlagOffset = 0x25A;
    public const int ReceiverOffset = 0x230;
    public const int QueueOffset = 0x260;
    public const byte FallbackHelperArgument = 10;

    /// <summary>
    /// Projects only the observed bounded branch. Null return observations mean
    /// unavailable; false means the call was observed not to return normally.
    /// No native helper or operating-system behavior is executed.
    /// </summary>
    public static GameSocketSenderBindingProjectionResult Project(
        GameSocketPacketAdmissionOutcome admission,
        byte? bindingFlag,
        uint? socketEsi,
        uint? senderEbx,
        bool? receiverArgumentHelperReturnedNormally,
        bool? predicateValueHelperReturnedNormally,
        byte? predicateValueHelperAl,
        bool? fallbackHelperReturnedNormally,
        bool? queueAppendReturnedNormally)
    {
        var inputs = new GameSocketSenderBindingProjectionInputs(
            admission,
            bindingFlag,
            socketEsi,
            senderEbx,
            receiverArgumentHelperReturnedNormally,
            predicateValueHelperReturnedNormally,
            predicateValueHelperAl,
            fallbackHelperReturnedNormally,
            queueAppendReturnedNormally);
        var steps = ImmutableArray.CreateBuilder<GameSocketSenderBindingStep>();
        var instructions = ImmutableArray.CreateBuilder<GameSocketSenderBindingInstruction>();
        var consumed = new HashSet<GameSocketSenderBindingInput>();
        uint? flagReadAddress = null;
        uint? receiverAddress = null;
        uint? queueAddress = null;
        uint? flagClearAddress = null;
        byte? flagReadValue = null;
        byte? flagClearValue = null;
        GameSocketReceivedQueueEvent? queueToken = null;
        byte? fallbackArgument = null;
        GameSocketSenderBindingFlags? compareFlags = null;
        GameSocketSenderBindingFlags? testAlFlags = null;
        GameSocketSenderBindingFlags? flagsAtStop = null;
        bool flagReadAttempted = false;
        bool flagReadCommitted = false;
        bool flagCompareCompleted = false;
        bool alTestAttempted = false;
        bool receiverArgumentHelperCallAttempted = false;
        bool predicateValueHelperCallAttempted = false;
        bool fallbackHelperCallAttempted = false;
        bool queueAppendCallAttempted = false;
        bool flagClearAttempted = false;
        bool flagClearCommitted = false;

        GameSocketSenderBindingProjectionResult Finish(
            GameSocketSenderBindingStop stop,
            uint? stopAtRaw,
            bool reachedCommonTail = false)
        {
            var unused = ImmutableArray.CreateBuilder<GameSocketSenderBindingInput>();
            foreach (var input in Enum.GetValues<GameSocketSenderBindingInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            return new GameSocketSenderBindingProjectionResult(
                inputs,
                stop,
                stopAtRaw,
                steps.ToImmutable(),
                instructions.ToImmutable(),
                unused.ToImmutable(),
                flagReadAttempted,
                flagReadAddress,
                flagReadValue,
                flagReadCommitted,
                flagCompareCompleted,
                compareFlags,
                alTestAttempted,
                testAlFlags,
                flagsAtStop,
                receiverAddress,
                queueAddress,
                queueToken,
                fallbackArgument,
                receiverArgumentHelperCallAttempted,
                predicateValueHelperCallAttempted,
                fallbackHelperCallAttempted,
                queueAppendCallAttempted,
                flagClearAttempted,
                flagClearAddress,
                flagClearValue,
                flagClearCommitted,
                reachedCommonTail);
        }

        if (admission is not (GameSocketPacketAdmissionOutcome.AcceptedZeroContext
            or GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext))
        {
            steps.Add(GameSocketSenderBindingStep.AdmissionRejected);
            consumed.Add(GameSocketSenderBindingInput.Admission);
            return Finish(GameSocketSenderBindingStop.AdmissionRejected, null);
        }

        consumed.Add(GameSocketSenderBindingInput.Admission);
        steps.Add(GameSocketSenderBindingStep.AdmissionAccepted);
        if (socketEsi is not uint socket)
        {
            steps.Add(GameSocketSenderBindingStep.SocketEsiMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingSocketEsiBeforeBindingCompare,
                BindingFlagCompareFileOffset);
        }

        consumed.Add(GameSocketSenderBindingInput.SocketEsi);
        flagReadAddress = unchecked(socket + (uint)BindingFlagOffset);
        flagReadAttempted = true;
        steps.Add(GameSocketSenderBindingStep.AttemptBindingFlagRead);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.CompareBindingFlag,
            BindingFlagCompareFileOffset,
            EffectiveAddress: flagReadAddress));

        if (bindingFlag is not byte flag)
        {
            steps.Add(GameSocketSenderBindingStep.BindingFlagValueMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingBindingFlagAtCompare,
                BindingFlagCompareFileOffset);
        }

        consumed.Add(GameSocketSenderBindingInput.BindingFlag);
        flagReadValue = flag;
        flagReadCommitted = false;
        flagCompareCompleted = true;
        compareFlags = GetCompareFlags(flag);
        flagsAtStop = compareFlags;
        steps.Add(GameSocketSenderBindingStep.BindingFlagCompared);
        bool bindingDisabled = flag == 0;
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.BranchIfBindingDisabled,
            BindingFlagBranchFileOffset,
            CommonTailFileOffset,
            BranchTaken: bindingDisabled));

        if (bindingDisabled)
        {
            steps.Add(GameSocketSenderBindingStep.BranchBindingDisabled);
            steps.Add(GameSocketSenderBindingStep.ReachCommonTail);
            return Finish(
                GameSocketSenderBindingStop.BindingDisabledAtCommonTail,
                CommonTailFileOffset,
                true);
        }

        steps.Add(GameSocketSenderBindingStep.BranchBindingEnabled);
        if (senderEbx is not uint sender)
        {
            steps.Add(GameSocketSenderBindingStep.SenderEbxMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingSenderEbxBeforePush,
                SenderEbxPushFileOffset);
        }

        consumed.Add(GameSocketSenderBindingInput.SenderEbx);
        steps.Add(GameSocketSenderBindingStep.PushSenderEbx);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.PushSenderEbx,
            SenderEbxPushFileOffset,
            OperandValue: sender));
        receiverAddress = unchecked(socket + (uint)ReceiverOffset);
        steps.Add(GameSocketSenderBindingStep.ComputeReceiverAddress);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.LoadReceiverArgumentAddress,
            ReceiverAddressLoadFileOffset,
            EffectiveAddress: receiverAddress));

        receiverArgumentHelperCallAttempted = true;
        steps.Add(GameSocketSenderBindingStep.CallReceiverArgumentHelper);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.CallReceiverArgumentHelper,
            ReceiverArgumentHelperCallFileOffset,
            TargetFileOffset: ReceiverArgumentHelperFileOffset));
        if (receiverArgumentHelperReturnedNormally is not bool receiverArgumentReturned)
        {
            steps.Add(GameSocketSenderBindingStep.ReceiverArgumentHelperReturnMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingReceiverArgumentHelperReturn,
                ReceiverArgumentHelperCallFileOffset + 5);
        }

        consumed.Add(GameSocketSenderBindingInput.ReceiverArgumentHelperReturnedNormally);
        if (!receiverArgumentReturned)
        {
            steps.Add(GameSocketSenderBindingStep.ReceiverArgumentHelperDidNotReturn);
            return Finish(
                GameSocketSenderBindingStop.ReceiverArgumentHelperDidNotReturn,
                ReceiverArgumentHelperCallFileOffset + 5);
        }

        flagsAtStop = null;
        steps.Add(GameSocketSenderBindingStep.ReceiverArgumentHelperReturned);
        steps.Add(GameSocketSenderBindingStep.MoveEcxFromEsiForPredicateHelper);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.MoveEcxFromEsi,
            0xBD3090));
        predicateValueHelperCallAttempted = true;
        steps.Add(GameSocketSenderBindingStep.CallPredicateValueHelper);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.CallPredicateValueHelper,
            PredicateHelperCallFileOffset,
            TargetFileOffset: PredicateHelperFileOffset));
        if (predicateValueHelperReturnedNormally is not bool predicateHelperReturned)
        {
            steps.Add(GameSocketSenderBindingStep.PredicateValueHelperReturnMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingPredicateValueHelperReturn,
                PredicateHelperCallFileOffset + 5);
        }

        consumed.Add(GameSocketSenderBindingInput.PredicateValueHelperReturnedNormally);
        if (!predicateHelperReturned)
        {
            steps.Add(GameSocketSenderBindingStep.PredicateValueHelperDidNotReturn);
            return Finish(
                GameSocketSenderBindingStop.PredicateValueHelperDidNotReturn,
                PredicateHelperCallFileOffset + 5);
        }

        flagsAtStop = null;
        steps.Add(GameSocketSenderBindingStep.PredicateValueHelperReturned);
        alTestAttempted = true;
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.TestAl,
            AlTestFileOffset));
        steps.Add(GameSocketSenderBindingStep.AttemptTestAl);
        if (predicateValueHelperAl is not byte al)
        {
            steps.Add(GameSocketSenderBindingStep.AlValueMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingAlForTest,
                AlTestFileOffset);
        }

        consumed.Add(GameSocketSenderBindingInput.PredicateValueHelperAl);
        testAlFlags = GetTestAlFlags(al);
        flagsAtStop = testAlFlags;
        bool alIsZero = al == 0;
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.BranchIfAlZero,
            AlZeroBranchFileOffset,
            FallbackArgumentPushFileOffset,
            BranchTaken: alIsZero));

        if (alIsZero)
        {
            steps.Add(GameSocketSenderBindingStep.SelectFallbackHelperPath);
            fallbackArgument = FallbackHelperArgument;
            steps.Add(GameSocketSenderBindingStep.PushFallbackHelperArgument);
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.PushFallbackHelperArgument,
                FallbackArgumentPushFileOffset,
                OperandValue: FallbackHelperArgument));
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.MoveEcxFromEsi,
                0xBD30A1));
            fallbackHelperCallAttempted = true;
            steps.Add(GameSocketSenderBindingStep.CallFallbackHelper);
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.CallFallbackHelper,
                FallbackHelperCallFileOffset,
                TargetFileOffset: FallbackHelperFileOffset));
            if (fallbackHelperReturnedNormally is not bool fallbackReturned)
            {
                steps.Add(GameSocketSenderBindingStep.FallbackHelperReturnMissing);
                return Finish(
                    GameSocketSenderBindingStop.AwaitingFallbackHelperReturn,
                    FallbackHelperCallFileOffset + 5);
            }

            consumed.Add(GameSocketSenderBindingInput.FallbackHelperReturnedNormally);
            if (!fallbackReturned)
            {
                steps.Add(GameSocketSenderBindingStep.FallbackHelperDidNotReturn);
                return Finish(
                    GameSocketSenderBindingStop.FallbackHelperDidNotReturn,
                    FallbackHelperCallFileOffset + 5);
            }

            flagsAtStop = null;
            steps.Add(GameSocketSenderBindingStep.FallbackHelperReturned);
            queueToken = GameSocketReceivedQueueEvent.PumpCloseRequest;
            steps.Add(GameSocketSenderBindingStep.SelectPumpCloseRequestToken);
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.PushPumpCloseRequestToken,
                PumpCloseTokenPushFileOffset,
                OperandValue: (uint)queueToken.Value));
        }
        else
        {
            queueToken = GameSocketReceivedQueueEvent.SenderBindingSucceeded;
            steps.Add(GameSocketSenderBindingStep.SelectSenderBindingSucceededToken);
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.PushSenderBindingSucceededToken,
                SenderBindingTokenPushFileOffset,
                OperandValue: (uint)queueToken.Value));
            instructions.Add(new(
                GameSocketSenderBindingInstructionKind.JumpToQueueAppend,
                SenderBindingTokenJumpFileOffset,
                CommonQueuePathFileOffset,
                BranchTaken: true));
        }

        queueAddress = unchecked(socket + (uint)QueueOffset);
        steps.Add(GameSocketSenderBindingStep.ComputeQueueAddress);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.LoadQueueAddress,
            QueueAddressLoadFileOffset,
            EffectiveAddress: queueAddress));
        queueAppendCallAttempted = true;
        steps.Add(GameSocketSenderBindingStep.CallQueueAppend);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.CallQueueAppend,
            QueueAppendHelperCallFileOffset,
            TargetFileOffset: QueueAppendHelperFileOffset));
        if (queueAppendReturnedNormally is not bool queueAppendReturned)
        {
            steps.Add(GameSocketSenderBindingStep.QueueAppendReturnMissing);
            return Finish(
                GameSocketSenderBindingStop.AwaitingQueueAppendReturn,
                QueueAppendHelperCallFileOffset + 5);
        }

        consumed.Add(GameSocketSenderBindingInput.QueueAppendReturnedNormally);
        if (!queueAppendReturned)
        {
            steps.Add(GameSocketSenderBindingStep.QueueAppendDidNotReturn);
            return Finish(
                GameSocketSenderBindingStop.QueueAppendDidNotReturn,
                QueueAppendHelperCallFileOffset + 5);
        }

        flagsAtStop = null;
        steps.Add(GameSocketSenderBindingStep.QueueAppendReturned);
        flagClearAttempted = true;
        flagClearAddress = flagReadAddress;
        flagClearValue = 0;
        steps.Add(GameSocketSenderBindingStep.AttemptClearBindingFlag);
        instructions.Add(new(
            GameSocketSenderBindingInstructionKind.ClearBindingFlag,
            BindingFlagClearFileOffset,
            EffectiveAddress: flagClearAddress,
            OperandValue: flagClearValue));
        flagClearCommitted = true;
        steps.Add(GameSocketSenderBindingStep.ClearBindingFlagCommitted);
        steps.Add(GameSocketSenderBindingStep.ReachCommonTail);
        return Finish(
            GameSocketSenderBindingStop.CompletedAtCommonTail,
            CommonTailFileOffset,
            true);
    }

    private const uint CommonQueuePathFileOffset = 0xBD30AA;

    public static GameSocketSenderBindingFlags GetCompareFlags(byte value) =>
        new(
            Carry: false,
            Parity: IsEvenParity(value),
            AuxiliaryCarry: false,
            Zero: value == 0,
            Sign: (value & 0x80) != 0,
            Overflow: false);

    public static GameSocketSenderBindingFlags GetTestAlFlags(byte al) =>
        new(
            Carry: false,
            Parity: IsEvenParity(al),
            AuxiliaryCarry: null,
            Zero: al == 0,
            Sign: (al & 0x80) != 0,
            Overflow: false);

    private static bool IsEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static bool IsSupplied(
        GameSocketSenderBindingProjectionInputs inputs,
        GameSocketSenderBindingInput input) => input switch
    {
        GameSocketSenderBindingInput.Admission => true,
        GameSocketSenderBindingInput.BindingFlag => inputs.BindingFlag.HasValue,
        GameSocketSenderBindingInput.SocketEsi => inputs.SocketEsi.HasValue,
        GameSocketSenderBindingInput.SenderEbx => inputs.SenderEbx.HasValue,
        GameSocketSenderBindingInput.ReceiverArgumentHelperReturnedNormally =>
            inputs.ReceiverArgumentHelperReturnedNormally.HasValue,
        GameSocketSenderBindingInput.PredicateValueHelperReturnedNormally =>
            inputs.PredicateValueHelperReturnedNormally.HasValue,
        GameSocketSenderBindingInput.PredicateValueHelperAl =>
            inputs.PredicateValueHelperAl.HasValue,
        GameSocketSenderBindingInput.FallbackHelperReturnedNormally =>
            inputs.FallbackHelperReturnedNormally.HasValue,
        GameSocketSenderBindingInput.QueueAppendReturnedNormally =>
            inputs.QueueAppendReturnedNormally.HasValue,
        _ => false
    };
}

public sealed record GameSocketSenderBindingProjectionInputs(
    GameSocketPacketAdmissionOutcome Admission,
    byte? BindingFlag,
    uint? SocketEsi,
    uint? SenderEbx,
    bool? ReceiverArgumentHelperReturnedNormally,
    bool? PredicateValueHelperReturnedNormally,
    byte? PredicateValueHelperAl,
    bool? FallbackHelperReturnedNormally,
    bool? QueueAppendReturnedNormally);

public sealed record GameSocketSenderBindingProjectionResult(
    GameSocketSenderBindingProjectionInputs Inputs,
    GameSocketSenderBindingStop Stop,
    uint? StopAtRaw,
    ImmutableArray<GameSocketSenderBindingStep> Steps,
    ImmutableArray<GameSocketSenderBindingInstruction> InstructionOrder,
    ImmutableArray<GameSocketSenderBindingInput> SuppliedButUnusedInputs,
    bool FlagReadAttempted,
    uint? FlagReadAddress,
    byte? FlagReadValue,
    bool FlagReadCommitted,
    bool FlagCompareCompleted,
    GameSocketSenderBindingFlags? FlagsAfterBindingFlagCompare,
    bool AlTestAttempted,
    GameSocketSenderBindingFlags? FlagsAfterAlTest,
    GameSocketSenderBindingFlags? FlagsAtStop,
    uint? ReceiverAddress,
    uint? QueueAddress,
    GameSocketReceivedQueueEvent? QueueToken,
    byte? FallbackHelperArgument,
    bool ReceiverArgumentHelperCallAttempted,
    bool PredicateValueHelperCallAttempted,
    bool FallbackHelperCallAttempted,
    bool QueueAppendCallAttempted,
    bool FlagClearAttempted,
    uint? FlagClearAddress,
    byte? FlagClearValue,
    bool FlagClearCommitted,
    bool ReachedCommonTail)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool IsCleanCompletion => ReachedCommonTail && !HasSuppliedButUnusedInputs;

    /// <summary>This offline projection never executes an opaque native helper.</summary>
    public bool OpaqueHelpersExecuted => false;
}
