using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceiveResultGateProjectionInputs(
    uint? ResultEax,
    uint? ReceiverEsi,
    bool? ErrorAccessorReturnedNormally,
    uint? ErrorAccessorEax,
    uint? FreshReceiverVtable,
    uint? FreshCallbackSlot,
    bool? ErrorCallbackReturnedNormally);

public enum GameSocketReceiveResultGateInput
{
    ResultEax,
    ReceiverEsi,
    ErrorAccessorReturnedNormally,
    ErrorAccessorEax,
    FreshReceiverVtable,
    FreshCallbackSlot,
    ErrorCallbackReturnedNormally
}

public enum GameSocketReceiveResultGateCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceiveResultGateCaptureStates(
    GameSocketReceiveResultGateCaptureState ResultEax,
    GameSocketReceiveResultGateCaptureState ReceiverEsi,
    GameSocketReceiveResultGateCaptureState ErrorAccessorReturnedNormally,
    GameSocketReceiveResultGateCaptureState ErrorAccessorEax,
    GameSocketReceiveResultGateCaptureState FreshReceiverVtable,
    GameSocketReceiveResultGateCaptureState FreshCallbackSlot,
    GameSocketReceiveResultGateCaptureState ErrorCallbackReturnedNormally);

public readonly record struct GameSocketReceiveResultGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public enum GameSocketReceiveResultGateInstructionKind
{
    TestResultEax,
    BranchIfResultNonzero,
    CallErrorAccessor,
    CompareErrorAccessorEaxTo996,
    JumpToSharedErrorGate,
    BranchIfErrorIs996,
    ReadFreshReceiverVtable,
    MoveEcxFromReceiverEsi,
    PushErrorAccessorEax,
    CallFreshReceiverSlot
}

public readonly record struct GameSocketReceiveResultGateInstruction(
    GameSocketReceiveResultGateInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null);

public enum GameSocketReceiveResultGateMemoryEffectKind
{
    FreshReceiverVtableRead,
    FreshCallbackSlotRead
}

public readonly record struct GameSocketReceiveResultGateMemoryEffect(
    GameSocketReceiveResultGateMemoryEffectKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed => false;
}

public enum GameSocketReceiveResultGateOpaqueCallKind
{
    ErrorAccessor,
    ReceiverErrorCallback
}

public enum GameSocketReceiveResultGateOpaqueCallStatus
{
    AwaitingReturnCapture,
    DidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct GameSocketReceiveResultGateOpaqueCall(
    GameSocketReceiveResultGateOpaqueCallKind Kind,
    uint CallSiteRawFileOffset,
    uint? TargetAddress,
    bool TargetCaptured,
    bool CallSiteReached,
    bool Attempted,
    bool? ReturnedNormally,
    GameSocketReceiveResultGateOpaqueCallStatus? Status,
    uint? EaxArgument,
    uint? EcxArgument,
    uint? ImportCellVirtualAddress = null);

public enum GameSocketReceiveResultGateStop
{
    AwaitingResultEaxAtTest,
    NonzeroResultAtBounded2EEB,
    AwaitingErrorAccessorReturn,
    ErrorAccessorDidNotReturnNormally,
    AwaitingErrorAccessorEaxBeforeCompare,
    ErrorCode996AtBounded3001,
    AwaitingReceiverEsiBeforeVtableRead,
    AwaitingFreshReceiverVtableAtFF9,
    AwaitingFreshCallbackSlotAtFFE,
    AwaitingErrorCallbackReturn,
    ErrorCallbackDidNotReturnNormally,
    ErrorCallbackReturnedAtBounded3001
}

public static class GameSocketReceiveResultGateProjection
{
    public const uint MethodRawFileOffset = 0xBD2ED7;
    public const uint ResultTestRawFileOffset = 0xBD2ED7;
    public const uint ResultNonzeroBranchRawFileOffset = 0xBD2ED9;
    public const uint ErrorAccessorCallRawFileOffset = 0xBD2EDB;
    public const uint ErrorCodeCompareRawFileOffset = 0xBD2EE1;
    public const uint SharedGateJumpRawFileOffset = 0xBD2EE6;
    public const uint SharedErrorGateRawFileOffset = 0xBD2FF7;
    public const uint Error996BranchRawFileOffset = 0xBD2FF7;
    public const uint ReceiverVtableReadRawFileOffset = 0xBD2FF9;
    public const uint ReceiverEcxMoveRawFileOffset = 0xBD2FFB;
    public const uint ErrorEaxPushRawFileOffset = 0xBD2FFD;
    public const uint ReceiverCallbackCallRawFileOffset = 0xBD2FFE;
    public const uint BoundedSuccessRawFileOffset = 0xBD2EEB;
    public const uint BoundedErrorCleanupRawFileOffset = 0xBD3001;

    public const uint ErrorAccessorIatVirtualAddress = 0x01AC4DA0;
    public const uint ErrorAccessorIatRawFileOffset = 0x016C39A0;
    public const string ErrorAccessorImport = "WSOCK32.dll!ordinal 111";

    public const uint PriorResultCallIatVirtualAddress = 0x01AC4D64;
    public const string PriorResultCallImport = "WS2_32.dll!WSAGetOverlappedResult";
    public const uint ReceiverCallbackSlotOffset = 0x20;
    public const uint ErrorCode996 = 996;

    public static GameSocketReceiveResultGateProjectionResult Project(
        uint? resultEax,
        uint? receiverEsi,
        bool? errorAccessorReturnedNormally,
        uint? errorAccessorEax,
        uint? freshReceiverVtable,
        uint? freshCallbackSlot,
        bool? errorCallbackReturnedNormally) =>
        Project(new(
            resultEax,
            receiverEsi,
            errorAccessorReturnedNormally,
            errorAccessorEax,
            freshReceiverVtable,
            freshCallbackSlot,
            errorCallbackReturnedNormally));

    public static GameSocketReceiveResultGateProjectionResult Project(
        GameSocketReceiveResultGateProjectionInputs inputs)
    {
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceiveResultGateInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceiveResultGateMemoryEffect>();
        var opaqueCalls = ImmutableArray.CreateBuilder<GameSocketReceiveResultGateOpaqueCall>();
        var consumed = new HashSet<GameSocketReceiveResultGateInput>();
        uint? receiverVtableAddress = null;
        uint? callbackSlotAddress = null;
        GameSocketReceiveResultGateFlags? flagsAfterResultTest = null;
        GameSocketReceiveResultGateFlags? flagsAfterErrorCompare = null;
        GameSocketReceiveResultGateFlags? flagsAtStop = null;
        bool vtableReadAttempted = false;
        bool callbackSlotReadAttempted = false;
        bool callbackCallSiteReached = false;

        GameSocketReceiveResultGateProjectionResult Finish(
            GameSocketReceiveResultGateStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused = ImmutableArray.CreateBuilder<GameSocketReceiveResultGateInput>();
            foreach (var input in Enum.GetValues<GameSocketReceiveResultGateInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceiveResultGateCaptureStates(
                GetCaptureState(inputs, consumed, stop, GameSocketReceiveResultGateInput.ResultEax),
                GetCaptureState(inputs, consumed, stop, GameSocketReceiveResultGateInput.ReceiverEsi),
                GetCaptureState(
                    inputs, consumed, stop,
                    GameSocketReceiveResultGateInput.ErrorAccessorReturnedNormally),
                GetCaptureState(inputs, consumed, stop, GameSocketReceiveResultGateInput.ErrorAccessorEax),
                GetCaptureState(inputs, consumed, stop, GameSocketReceiveResultGateInput.FreshReceiverVtable),
                GetCaptureState(inputs, consumed, stop, GameSocketReceiveResultGateInput.FreshCallbackSlot),
                GetCaptureState(
                    inputs, consumed, stop,
                    GameSocketReceiveResultGateInput.ErrorCallbackReturnedNormally));

            return new(
                inputs,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                memoryEffects.ToImmutable(),
                opaqueCalls.ToImmutable(),
                unused.ToImmutable(),
                captureStates,
                receiverVtableAddress,
                callbackSlotAddress,
                vtableReadAttempted,
                callbackSlotReadAttempted,
                callbackCallSiteReached,
                flagsAfterResultTest,
                flagsAfterErrorCompare,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        instructions.Add(new(GameSocketReceiveResultGateInstructionKind.TestResultEax,
            ResultTestRawFileOffset, OperandValue: inputs.ResultEax));
        if (inputs.ResultEax is not uint result)
            return Finish(GameSocketReceiveResultGateStop.AwaitingResultEaxAtTest,
                ResultTestRawFileOffset);

        consumed.Add(GameSocketReceiveResultGateInput.ResultEax);
        flagsAfterResultTest = GetTestFlags(result);
        flagsAtStop = flagsAfterResultTest;
        bool resultNonzero = result != 0;
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.BranchIfResultNonzero,
            ResultNonzeroBranchRawFileOffset,
            BoundedSuccessRawFileOffset,
            resultNonzero));
        if (resultNonzero)
        {
            return Finish(
                GameSocketReceiveResultGateStop.NonzeroResultAtBounded2EEB,
                BoundedSuccessRawFileOffset,
                true);
        }

        var accessorCall = new GameSocketReceiveResultGateOpaqueCall(
            GameSocketReceiveResultGateOpaqueCallKind.ErrorAccessor,
            ErrorAccessorCallRawFileOffset,
            null,
            false,
            true,
            true,
            inputs.ErrorAccessorReturnedNormally,
            GetCallStatus(inputs.ErrorAccessorReturnedNormally),
            null,
            null,
            ErrorAccessorIatVirtualAddress);
        opaqueCalls.Add(accessorCall);
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.CallErrorAccessor,
            ErrorAccessorCallRawFileOffset,
            EffectiveAddress: ErrorAccessorIatVirtualAddress));
        if (inputs.ErrorAccessorReturnedNormally is not bool accessorReturned)
        {
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingErrorAccessorReturn,
                ErrorAccessorCallRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.ErrorAccessorReturnedNormally);
        if (!accessorReturned)
        {
            return Finish(
                GameSocketReceiveResultGateStop.ErrorAccessorDidNotReturnNormally,
                ErrorAccessorCallRawFileOffset);
        }

        flagsAtStop = null;
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.CompareErrorAccessorEaxTo996,
            ErrorCodeCompareRawFileOffset,
            OperandValue: inputs.ErrorAccessorEax));
        if (inputs.ErrorAccessorEax is not uint errorCode)
        {
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingErrorAccessorEaxBeforeCompare,
                ErrorCodeCompareRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.ErrorAccessorEax);
        flagsAfterErrorCompare = GetCompareFlags(errorCode, ErrorCode996);
        flagsAtStop = flagsAfterErrorCompare;
        instructions[^1] = instructions[^1] with { OperandValue = errorCode };
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.JumpToSharedErrorGate,
            SharedGateJumpRawFileOffset,
            SharedErrorGateRawFileOffset));
        bool isError996 = errorCode == ErrorCode996;
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.BranchIfErrorIs996,
            Error996BranchRawFileOffset,
            BoundedErrorCleanupRawFileOffset,
            isError996));
        if (isError996)
        {
            return Finish(
                GameSocketReceiveResultGateStop.ErrorCode996AtBounded3001,
                BoundedErrorCleanupRawFileOffset,
                true);
        }

        if (inputs.ReceiverEsi is not uint receiver)
        {
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingReceiverEsiBeforeVtableRead,
                ReceiverVtableReadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.ReceiverEsi);
        receiverVtableAddress = receiver;
        vtableReadAttempted = true;
        memoryEffects.Add(new(
            GameSocketReceiveResultGateMemoryEffectKind.FreshReceiverVtableRead,
            ReceiverVtableReadRawFileOffset,
            receiverVtableAddress.Value,
            inputs.FreshReceiverVtable,
            true,
            inputs.FreshReceiverVtable.HasValue));
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.ReadFreshReceiverVtable,
            ReceiverVtableReadRawFileOffset,
            EffectiveAddress: receiverVtableAddress));
        if (inputs.FreshReceiverVtable is not uint vtable)
        {
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingFreshReceiverVtableAtFF9,
                ReceiverVtableReadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.FreshReceiverVtable);
        uint slotAddress = unchecked(vtable + ReceiverCallbackSlotOffset);
        callbackSlotAddress = slotAddress;
        callbackSlotReadAttempted = true;
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.MoveEcxFromReceiverEsi,
            ReceiverEcxMoveRawFileOffset,
            OperandValue: receiver));
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.PushErrorAccessorEax,
            ErrorEaxPushRawFileOffset,
            OperandValue: errorCode));
        instructions.Add(new(
            GameSocketReceiveResultGateInstructionKind.CallFreshReceiverSlot,
            ReceiverCallbackCallRawFileOffset,
            EffectiveAddress: callbackSlotAddress,
            OperandValue: inputs.FreshCallbackSlot));
        memoryEffects.Add(new(
            GameSocketReceiveResultGateMemoryEffectKind.FreshCallbackSlotRead,
            ReceiverCallbackCallRawFileOffset,
            slotAddress,
            inputs.FreshCallbackSlot,
            true,
            inputs.FreshCallbackSlot.HasValue));
        callbackCallSiteReached = true;
        if (inputs.FreshCallbackSlot is not uint callbackTarget)
        {
            opaqueCalls.Add(new(
                GameSocketReceiveResultGateOpaqueCallKind.ReceiverErrorCallback,
                ReceiverCallbackCallRawFileOffset,
                null,
                false,
                true,
                false,
                null,
                null,
                errorCode,
                receiver));
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingFreshCallbackSlotAtFFE,
                ReceiverCallbackCallRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.FreshCallbackSlot);
        var callbackCall = new GameSocketReceiveResultGateOpaqueCall(
            GameSocketReceiveResultGateOpaqueCallKind.ReceiverErrorCallback,
            ReceiverCallbackCallRawFileOffset,
            callbackTarget,
            true,
            true,
            true,
            inputs.ErrorCallbackReturnedNormally,
            GetCallStatus(inputs.ErrorCallbackReturnedNormally),
            errorCode,
            receiver);
        opaqueCalls.Add(callbackCall);
        if (inputs.ErrorCallbackReturnedNormally is not bool callbackReturned)
        {
            return Finish(
                GameSocketReceiveResultGateStop.AwaitingErrorCallbackReturn,
                ReceiverCallbackCallRawFileOffset);
        }

        consumed.Add(GameSocketReceiveResultGateInput.ErrorCallbackReturnedNormally);
        if (!callbackReturned)
        {
            return Finish(
                GameSocketReceiveResultGateStop.ErrorCallbackDidNotReturnNormally,
                ReceiverCallbackCallRawFileOffset);
        }

        flagsAtStop = null;
        return Finish(
            GameSocketReceiveResultGateStop.ErrorCallbackReturnedAtBounded3001,
            BoundedErrorCleanupRawFileOffset,
            true);
    }

    public static GameSocketReceiveResultGateFlags GetTestFlags(uint value) =>
        new(
            Carry: false,
            Parity: (BitOperations.PopCount(value & 0xFF) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x8000_0000) != 0,
            Overflow: false);

    public static GameSocketReceiveResultGateFlags GetCompareFlags(uint left, uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (BitOperations.PopCount(result & 0xFF) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x8000_0000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x8000_0000) != 0);
    }

    private static GameSocketReceiveResultGateOpaqueCallStatus? GetCallStatus(bool? returnedNormally) =>
        returnedNormally switch
        {
            true => GameSocketReceiveResultGateOpaqueCallStatus.ReturnedNormally,
            false => GameSocketReceiveResultGateOpaqueCallStatus.DidNotReturnNormally,
            null => GameSocketReceiveResultGateOpaqueCallStatus.AwaitingReturnCapture
        };

    private static GameSocketReceiveResultGateCaptureState GetCaptureState(
        GameSocketReceiveResultGateProjectionInputs inputs,
        HashSet<GameSocketReceiveResultGateInput> consumed,
        GameSocketReceiveResultGateStop stop,
        GameSocketReceiveResultGateInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceiveResultGateCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceiveResultGateCaptureState.SuppliedButUnused;

        bool captureRequired = input switch
        {
            GameSocketReceiveResultGateInput.ResultEax =>
                stop == GameSocketReceiveResultGateStop.AwaitingResultEaxAtTest,
            GameSocketReceiveResultGateInput.ErrorAccessorReturnedNormally =>
                stop == GameSocketReceiveResultGateStop.AwaitingErrorAccessorReturn,
            GameSocketReceiveResultGateInput.ErrorAccessorEax =>
                stop == GameSocketReceiveResultGateStop.AwaitingErrorAccessorEaxBeforeCompare,
            GameSocketReceiveResultGateInput.ReceiverEsi =>
                stop == GameSocketReceiveResultGateStop.AwaitingReceiverEsiBeforeVtableRead,
            GameSocketReceiveResultGateInput.FreshReceiverVtable =>
                stop == GameSocketReceiveResultGateStop.AwaitingFreshReceiverVtableAtFF9,
            GameSocketReceiveResultGateInput.FreshCallbackSlot =>
                stop == GameSocketReceiveResultGateStop.AwaitingFreshCallbackSlotAtFFE,
            GameSocketReceiveResultGateInput.ErrorCallbackReturnedNormally =>
                stop == GameSocketReceiveResultGateStop.AwaitingErrorCallbackReturn,
            _ => false
        };
        return captureRequired
            ? GameSocketReceiveResultGateCaptureState.Missing
            : GameSocketReceiveResultGateCaptureState.NotReached;
    }

    private static bool IsSupplied(
        GameSocketReceiveResultGateProjectionInputs inputs,
        GameSocketReceiveResultGateInput input) => input switch
    {
        GameSocketReceiveResultGateInput.ResultEax => inputs.ResultEax.HasValue,
        GameSocketReceiveResultGateInput.ReceiverEsi => inputs.ReceiverEsi.HasValue,
        GameSocketReceiveResultGateInput.ErrorAccessorReturnedNormally =>
            inputs.ErrorAccessorReturnedNormally.HasValue,
        GameSocketReceiveResultGateInput.ErrorAccessorEax => inputs.ErrorAccessorEax.HasValue,
        GameSocketReceiveResultGateInput.FreshReceiverVtable => inputs.FreshReceiverVtable.HasValue,
        GameSocketReceiveResultGateInput.FreshCallbackSlot => inputs.FreshCallbackSlot.HasValue,
        GameSocketReceiveResultGateInput.ErrorCallbackReturnedNormally =>
            inputs.ErrorCallbackReturnedNormally.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceiveResultGateProjectionResult(
    GameSocketReceiveResultGateProjectionInputs Inputs,
    GameSocketReceiveResultGateStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceiveResultGateInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceiveResultGateMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceiveResultGateOpaqueCall> OpaqueCalls,
    ImmutableArray<GameSocketReceiveResultGateInput> SuppliedButUnusedInputs,
    GameSocketReceiveResultGateCaptureStates CaptureStates,
    uint? FreshReceiverVtableReadAddress,
    uint? FreshCallbackSlotReadAddress,
    bool FreshReceiverVtableReadAttempted,
    bool FreshCallbackSlotReadAttempted,
    bool ReceiverCallbackCallSiteReached,
    GameSocketReceiveResultGateFlags? FlagsAfterResultTest,
    GameSocketReceiveResultGateFlags? FlagsAfterErrorCompare,
    GameSocketReceiveResultGateFlags? FlagsAtStop,
    bool ReachedBoundedEndpoint)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool IsCleanCompletion => ReachedBoundedEndpoint && !HasSuppliedButUnusedInputs;

    public bool OpaqueCallsExecuted => false;

    public bool ResultCallReissued => false;

    public bool AnyMemoryWriteCommitted => false;
}
