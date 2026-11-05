using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceivePostPacketStackPreparationProjectionInputs(
    GameSocketReceivePacketCallPreparationProjectionInputs ParentInputs,
    bool? OpaquePacketReturnedNormally,
    uint? FreshPostPacketEsi,
    uint? FreshPostPacketEsp,
    uint? FreshOperation);

public enum GameSocketReceivePostPacketStackPreparationInput
{
    OpaquePacketReturnedNormally,
    FreshPostPacketEsi,
    FreshPostPacketEsp,
    FreshOperation
}

public enum GameSocketReceivePostPacketStackPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceivePostPacketStackPreparationCaptureStates(
    GameSocketReceivePostPacketStackPreparationCaptureState OpaquePacketReturnedNormally,
    GameSocketReceivePostPacketStackPreparationCaptureState FreshPostPacketEsi,
    GameSocketReceivePostPacketStackPreparationCaptureState FreshPostPacketEsp,
    GameSocketReceivePostPacketStackPreparationCaptureState FreshOperation);

public enum GameSocketReceivePostPacketStackPreparationStop
{
    ParentDidNotReachPacketCall,
    ParentPacketCallBoundaryWasDirty,
    AwaitingOpaquePacketReturnAtF25,
    OpaquePacketDidNotReturnNormally,
    AwaitingFreshPostPacketEsiAtF2A,
    AwaitingFreshOperationAtF2A,
    AwaitingFreshPostPacketEspAtF37,
    NonzeroBranchTargetAtF64,
    DiagnosticImportBoundaryAtF5B
}

public enum GameSocketReceivePostPacketStackPreparationInstructionKind
{
    CompareFreshOperationToZero,
    LeaLocalPointer,
    StoreZeroAtEsp1C,
    StoreLocalPointerAtEsp28,
    Store578AtEsp24,
    BranchIfOperationNonzero,
    PushZero,
    PushToken5B,
    PushDiagnosticArgument,
    CallDiagnosticImport
}

public readonly record struct GameSocketReceivePostPacketStackPreparationInstruction(
    GameSocketReceivePostPacketStackPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    uint? TargetAddress = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null);

public enum GameSocketReceivePostPacketStackPreparationMemoryEffectKind
{
    FreshOperationRead,
    ZeroStackLocalWrite,
    LocalPointerStackWrite,
    SizeStackLocalWrite,
    PushZeroStackWrite,
    PushTokenStackWrite,
    PushDiagnosticArgumentStackWrite
}

public readonly record struct GameSocketReceivePostPacketStackPreparationMemoryEffect(
    GameSocketReceivePostPacketStackPreparationMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        Kind is not GameSocketReceivePostPacketStackPreparationMemoryEffectKind.FreshOperationRead
        && Address.HasValue
        && Value.HasValue;
}

public enum GameSocketReceivePostPacketStackPreparationCallKind
{
    ParentDiagnosticImport,
    ParentSenderHelper,
    PacketCall,
    DiagnosticImportBoundary
}

public readonly record struct GameSocketReceivePostPacketStackPreparationCall(
    GameSocketReceivePostPacketStackPreparationCallKind Kind,
    uint CallSiteRawFileOffset,
    uint? TargetRawFileOffset,
    uint? TargetAddress,
    uint? ImportCellVirtualAddress,
    bool TargetCaptured,
    bool CallSiteReached,
    bool Attempted,
    bool? ReturnedNormally,
    uint? EaxArgument,
    uint? EcxArgument,
    uint? ReturnedEax,
    uint? PostCallEsp,
    ImmutableArray<uint?> StackArguments);

public readonly record struct GameSocketReceivePostPacketStackPreparationTraceStep(
    int Sequence,
    GameSocketReceivePacketCallPreparationTraceStep? ParentMemoryEffect,
    GameSocketReceivePostPacketStackPreparationMemoryEffect? PostPacketMemoryEffect);

public static class GameSocketReceivePostPacketStackPreparationProjection
{
    public const uint MethodRawFileOffset = 0x00BD2F2A;
    public const uint PacketReturnBoundaryRawFileOffset = 0x00BD2F25;
    public const uint OperationCompareRawFileOffset = 0x00BD2F2A;
    public const uint LocalPointerLeaRawFileOffset = 0x00BD2F31;
    public const uint ZeroStackLocalStoreRawFileOffset = 0x00BD2F37;
    public const uint LocalPointerStackStoreRawFileOffset = 0x00BD2F3F;
    public const uint SizeStackLocalStoreRawFileOffset = 0x00BD2F43;
    public const uint OperationBranchRawFileOffset = 0x00BD2F4B;
    public const uint NonzeroBranchTargetRawFileOffset = 0x00BD2F64;
    public const uint PushZeroRawFileOffset = 0x00BD2F4D;
    public const uint PushTokenRawFileOffset = 0x00BD2F4F;
    public const uint PushDiagnosticArgumentRawFileOffset = 0x00BD2F51;
    public const uint PushDiagnosticArgumentAgainRawFileOffset = 0x00BD2F56;
    public const uint DiagnosticImportCallRawFileOffset = 0x00BD2F5B;
    public const uint DiagnosticImportCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticArgumentVirtualAddress = 0x01C5EA58;
    public const uint OperationFieldOffset = 0x0DF4;
    public const uint LocalPointerOffset = 0x0278;
    public const uint ZeroStackLocalOffset = 0x1C;
    public const uint LocalPointerStackOffset = 0x28;
    public const uint SizeStackLocalOffset = 0x24;
    public const uint SizeArgument = 0x0578;
    public const uint TokenArgument = 0x5B;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x83, 0xBE, 0xF4, 0x0D, 0x00, 0x00, 0x00,
            0x8D, 0x86, 0x78, 0x02, 0x00, 0x00,
            0xC7, 0x44, 0x24, 0x1C, 0x00, 0x00, 0x00, 0x00,
            0x89, 0x44, 0x24, 0x28,
            0xC7, 0x44, 0x24, 0x24, 0x78, 0x05, 0x00, 0x00,
            0x75, 0x17,
            0x6A, 0x00,
            0x6A, 0x5B,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
            0x83, 0xC4, 0x10);

    public static GameSocketReceivePostPacketStackPreparationProjectionResult Project(
        GameSocketReceivePostPacketStackPreparationProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceivePacketCallPreparationProjectionResult parent =
            GameSocketReceivePacketCallPreparationProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceivePostPacketStackPreparationInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceivePostPacketStackPreparationMemoryEffect>();
        var calls = ImmutableArray.CreateBuilder<GameSocketReceivePostPacketStackPreparationCall>();
        var trace =
            ImmutableArray.CreateBuilder<GameSocketReceivePostPacketStackPreparationTraceStep>();
        var consumed =
            new HashSet<GameSocketReceivePostPacketStackPreparationInput>();

        foreach (GameSocketReceivePacketCallPreparationTraceStep step in parent.OrderedMemoryEffects)
            trace.Add(new(trace.Count, step, null));

        foreach (GameSocketReceivePacketCallPreparationCall parentCall in parent.Calls)
        {
            calls.Add(new(
                parentCall.Kind switch
                {
                    GameSocketReceivePacketCallPreparationCallKind.DiagnosticImport =>
                        GameSocketReceivePostPacketStackPreparationCallKind.ParentDiagnosticImport,
                    GameSocketReceivePacketCallPreparationCallKind.SenderHelper =>
                        GameSocketReceivePostPacketStackPreparationCallKind.ParentSenderHelper,
                    GameSocketReceivePacketCallPreparationCallKind.FixedTargetBoundary =>
                        GameSocketReceivePostPacketStackPreparationCallKind.PacketCall,
                    _ => throw new InvalidOperationException("Unknown parent call kind.")
                },
                parentCall.CallSiteRawFileOffset,
                parentCall.TargetRawFileOffset,
                parentCall.TargetAddress,
                parentCall.ImportCellVirtualAddress,
                parentCall.TargetCaptured,
                parentCall.CallSiteReached,
                parentCall.Attempted,
                parentCall.ReturnedNormally,
                parentCall.EaxArgument,
                parentCall.EcxArgument,
                parentCall.ReturnedEax,
                parentCall.PostCallEsp,
                parentCall.StackArguments));
        }

        uint? operationAddress = null;
        uint? localPointer = null;
        uint? zeroStackLocalAddress = null;
        uint? localPointerStackAddress = null;
        uint? sizeStackLocalAddress = null;
        uint? stackPointerAtDiagnosticCall = null;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;
        bool parentHandoffAdmitted = false;
        bool packetCallSiteReached = false;
        bool operationReadAttempted = false;
        bool? nonzeroBranchTaken = null;
        bool diagnosticCallSiteReached = false;

        void AddMemoryEffect(GameSocketReceivePostPacketStackPreparationMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            trace.Add(new(trace.Count, null, effect));
        }

        GameSocketReceivePostPacketStackPreparationProjectionResult Finish(
            GameSocketReceivePostPacketStackPreparationStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceivePostPacketStackPreparationInput>();
            foreach (GameSocketReceivePostPacketStackPreparationInput input
                     in Enum.GetValues<GameSocketReceivePostPacketStackPreparationInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceivePostPacketStackPreparationCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePostPacketStackPreparationInput.OpaquePacketReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsi),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePostPacketStackPreparationInput.FreshOperation));

            return new(
                inputs,
                parent,
                parentHandoffAdmitted,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                memoryEffects.ToImmutable(),
                trace.ToImmutable(),
                calls.ToImmutable(),
                unused.ToImmutable(),
                captureStates,
                operationAddress,
                localPointer,
                zeroStackLocalAddress,
                localPointerStackAddress,
                sizeStackLocalAddress,
                stackPointerAtDiagnosticCall,
                packetCallSiteReached,
                operationReadAttempted,
                nonzeroBranchTaken,
                diagnosticCallSiteReached,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        if (parent.Stop
                != GameSocketReceivePacketCallPreparationStop.FixedCallBoundaryAtF25
            || !parent.ReachedBoundedEndpoint)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.ParentDidNotReachPacketCall,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.ParentPacketCallBoundaryWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        packetCallSiteReached = true;
        int packetCallIndex = -1;
        for (int i = calls.Count - 1; i >= 0; i--)
        {
            if (calls[i].CallSiteRawFileOffset
                != GameSocketReceivePacketCallPreparationProjection.FixedCallRawFileOffset)
                continue;
            packetCallIndex = i;
            calls[i] = calls[i] with
            {
                Attempted = true,
                ReturnedNormally = inputs.OpaquePacketReturnedNormally
            };
            break;
        }

        if (inputs.OpaquePacketReturnedNormally is not bool packetReturnedNormally)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.AwaitingOpaquePacketReturnAtF25,
                PacketReturnBoundaryRawFileOffset);
        }

        consumed.Add(
            GameSocketReceivePostPacketStackPreparationInput.OpaquePacketReturnedNormally);
        if (!packetReturnedNormally)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.OpaquePacketDidNotReturnNormally,
                PacketReturnBoundaryRawFileOffset);
        }

        flagsAtStop = null;
        if (packetCallIndex >= 0)
        {
            calls[packetCallIndex] = calls[packetCallIndex] with
            {
                ReturnedNormally = true
            };
        }

        if (inputs.FreshPostPacketEsi is not uint esi)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEsiAtF2A,
                OperationCompareRawFileOffset);
        }

        operationAddress = unchecked(esi + OperationFieldOffset);
        operationReadAttempted = true;
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.CompareFreshOperationToZero,
            OperationCompareRawFileOffset,
            EffectiveAddress: operationAddress,
            OperandValue: inputs.FreshOperation));
        AddMemoryEffect(new(
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.FreshOperationRead,
            OperationCompareRawFileOffset,
            operationAddress,
            inputs.FreshOperation,
            true,
            inputs.FreshOperation.HasValue));
        consumed.Add(GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsi);
        if (inputs.FreshOperation is not uint operation)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshOperationAtF2A,
                OperationCompareRawFileOffset);
        }

        consumed.Add(GameSocketReceivePostPacketStackPreparationInput.FreshOperation);
        flagsAtStop = CompareDwordToZeroFlags(operation);
        localPointer = unchecked(esi + LocalPointerOffset);
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.LeaLocalPointer,
            LocalPointerLeaRawFileOffset,
            EffectiveAddress: localPointer,
            ResultValue: localPointer));

        if (inputs.FreshPostPacketEsp is not uint postPacketEsp)
        {
            instructions.Add(new(
                GameSocketReceivePostPacketStackPreparationInstructionKind.StoreZeroAtEsp1C,
                ZeroStackLocalStoreRawFileOffset,
                EffectiveAddress: null,
                OperandValue: 0));
            AddMemoryEffect(new(
                GameSocketReceivePostPacketStackPreparationMemoryEffectKind.ZeroStackLocalWrite,
                ZeroStackLocalStoreRawFileOffset,
                null,
                0,
                true,
                false));
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEspAtF37,
                ZeroStackLocalStoreRawFileOffset);
        }

        consumed.Add(GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsp);
        zeroStackLocalAddress = unchecked(postPacketEsp + ZeroStackLocalOffset);
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.StoreZeroAtEsp1C,
            ZeroStackLocalStoreRawFileOffset,
            EffectiveAddress: zeroStackLocalAddress,
            OperandValue: 0));
        AddMemoryEffect(new(
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.ZeroStackLocalWrite,
            ZeroStackLocalStoreRawFileOffset,
            zeroStackLocalAddress,
            0,
            true,
            true));

        localPointerStackAddress = unchecked(postPacketEsp + LocalPointerStackOffset);
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.StoreLocalPointerAtEsp28,
            LocalPointerStackStoreRawFileOffset,
            EffectiveAddress: localPointerStackAddress,
            OperandValue: localPointer,
            ResultValue: localPointer));
        AddMemoryEffect(new(
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.LocalPointerStackWrite,
            LocalPointerStackStoreRawFileOffset,
            localPointerStackAddress,
            localPointer,
            true,
            true));

        sizeStackLocalAddress = unchecked(postPacketEsp + SizeStackLocalOffset);
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.Store578AtEsp24,
            SizeStackLocalStoreRawFileOffset,
            EffectiveAddress: sizeStackLocalAddress,
            OperandValue: SizeArgument));
        AddMemoryEffect(new(
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.SizeStackLocalWrite,
            SizeStackLocalStoreRawFileOffset,
            sizeStackLocalAddress,
            SizeArgument,
            true,
            true));

        bool isNonzero = operation != 0;
        nonzeroBranchTaken = isNonzero;
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.BranchIfOperationNonzero,
            OperationBranchRawFileOffset,
            BranchTaken: isNonzero,
            TargetRawFileOffset: NonzeroBranchTargetRawFileOffset));
        if (isNonzero)
        {
            return Finish(
                GameSocketReceivePostPacketStackPreparationStop.NonzeroBranchTargetAtF64,
                NonzeroBranchTargetRawFileOffset,
                true);
        }

        uint stackPointer = postPacketEsp;
        stackPointer = Push(
            GameSocketReceivePostPacketStackPreparationInstructionKind.PushZero,
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushZeroStackWrite,
            PushZeroRawFileOffset,
            0,
            stackPointer);
        stackPointer = Push(
            GameSocketReceivePostPacketStackPreparationInstructionKind.PushToken5B,
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushTokenStackWrite,
            PushTokenRawFileOffset,
            TokenArgument,
            stackPointer);
        stackPointer = Push(
            GameSocketReceivePostPacketStackPreparationInstructionKind.PushDiagnosticArgument,
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);
        stackPointer = Push(
            GameSocketReceivePostPacketStackPreparationInstructionKind.PushDiagnosticArgument,
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentAgainRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);

        stackPointerAtDiagnosticCall = stackPointer;
        diagnosticCallSiteReached = true;
        instructions.Add(new(
            GameSocketReceivePostPacketStackPreparationInstructionKind.CallDiagnosticImport,
            DiagnosticImportCallRawFileOffset,
            EffectiveAddress: DiagnosticImportCellVirtualAddress,
            OperandValue: stackPointer));
        calls.Add(new(
            GameSocketReceivePostPacketStackPreparationCallKind.DiagnosticImportBoundary,
            DiagnosticImportCallRawFileOffset,
            null,
            null,
            DiagnosticImportCellVirtualAddress,
            false,
            true,
            false,
            null,
            null,
            null,
            null,
            null,
            ImmutableArray.Create<uint?>(
                DiagnosticArgumentVirtualAddress,
                DiagnosticArgumentVirtualAddress,
                TokenArgument,
                0)));

        return Finish(
            GameSocketReceivePostPacketStackPreparationStop.DiagnosticImportBoundaryAtF5B,
            DiagnosticImportCallRawFileOffset,
            true);

        uint Push(
            GameSocketReceivePostPacketStackPreparationInstructionKind instructionKind,
            GameSocketReceivePostPacketStackPreparationMemoryEffectKind effectKind,
            uint rawFileOffset,
            uint value,
            uint currentEsp)
        {
            uint resultEsp = unchecked(currentEsp - sizeof(uint));
            instructions.Add(new(
                instructionKind,
                rawFileOffset,
                EffectiveAddress: resultEsp,
                OperandValue: value,
                ResultValue: resultEsp));
            AddMemoryEffect(new(
                effectKind,
                rawFileOffset,
                resultEsp,
                value,
                true,
                true));
            return resultEsp;
        }
    }

    private static GameSocketReceiveResultGateFlags CompareDwordToZeroFlags(uint value) =>
        new(
            false,
            (BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            false,
            value == 0,
            (value & 0x8000_0000) != 0,
            false);

    private static GameSocketReceivePostPacketStackPreparationCaptureState GetCaptureState(
        GameSocketReceivePostPacketStackPreparationProjectionInputs inputs,
        HashSet<GameSocketReceivePostPacketStackPreparationInput> consumed,
        GameSocketReceivePostPacketStackPreparationStop stop,
        GameSocketReceivePostPacketStackPreparationInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceivePostPacketStackPreparationCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceivePostPacketStackPreparationCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceivePostPacketStackPreparationInput.OpaquePacketReturnedNormally =>
                stop == GameSocketReceivePostPacketStackPreparationStop.AwaitingOpaquePacketReturnAtF25,
            GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsi =>
                stop == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEsiAtF2A,
            GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsp =>
                stop == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshPostPacketEspAtF37,
            GameSocketReceivePostPacketStackPreparationInput.FreshOperation =>
                stop == GameSocketReceivePostPacketStackPreparationStop.AwaitingFreshOperationAtF2A,
            _ => false
        };
        return required
            ? GameSocketReceivePostPacketStackPreparationCaptureState.Missing
            : GameSocketReceivePostPacketStackPreparationCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceivePostPacketStackPreparationProjectionInputs inputs,
        GameSocketReceivePostPacketStackPreparationInput input) => input switch
    {
        GameSocketReceivePostPacketStackPreparationInput.OpaquePacketReturnedNormally =>
            inputs.OpaquePacketReturnedNormally.HasValue,
        GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsi =>
            inputs.FreshPostPacketEsi.HasValue,
        GameSocketReceivePostPacketStackPreparationInput.FreshPostPacketEsp =>
            inputs.FreshPostPacketEsp.HasValue,
        GameSocketReceivePostPacketStackPreparationInput.FreshOperation =>
            inputs.FreshOperation.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceivePostPacketStackPreparationProjectionResult(
    GameSocketReceivePostPacketStackPreparationProjectionInputs Inputs,
    GameSocketReceivePacketCallPreparationProjectionResult ParentProjection,
    bool ParentHandoffAdmitted,
    GameSocketReceivePostPacketStackPreparationStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceivePostPacketStackPreparationInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceivePostPacketStackPreparationMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceivePostPacketStackPreparationTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceivePostPacketStackPreparationCall> Calls,
    ImmutableArray<GameSocketReceivePostPacketStackPreparationInput> SuppliedButUnusedInputs,
    GameSocketReceivePostPacketStackPreparationCaptureStates CaptureStates,
    uint? OperationAddress,
    uint? LocalPointer,
    uint? ZeroStackLocalAddress,
    uint? LocalPointerStackAddress,
    uint? SizeStackLocalAddress,
    uint? StackPointerAtDiagnosticCall,
    bool PacketCallSiteReached,
    bool OperationReadAttempted,
    bool? NonzeroBranchTaken,
    bool DiagnosticCallSiteReached,
    GameSocketReceiveResultGateFlags? FlagsAtStop,
    bool ReachedBoundedEndpoint)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentHandoffAdmitted && ParentProjection.ExactBoundedCompletion);

    public bool IsCleanCompletion =>
        ParentHandoffAdmitted
        && ReachedBoundedEndpoint
        && ParentProjection.IsCleanCompletion
        && !HasSuppliedButUnusedInputs;

    public bool AnyMemoryWriteCommitted =>
        ParentProjection.AnyMemoryWriteCommitted
        || OrderedMemoryEffects.Any(step =>
            step.PostPacketMemoryEffect is { Committed: true });

    public bool PacketReceiveCallExecuted => false;

    public bool DiagnosticImportExecuted => false;

    public bool PacketRepostModeled => false;

    public bool ReturnInstructionModeled => false;
}
