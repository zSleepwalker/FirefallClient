using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceiveOperationReloadGateProjectionInputs(
    GameSocketReceivePostPacketStackPreparationProjectionInputs ParentInputs,
    bool? PostPacketDiagnosticReturnedNormally,
    uint? FreshPostDiagnosticEsp,
    uint? FreshPostDiagnosticEsi,
    uint? FreshReloadedOperation);

public enum GameSocketReceiveOperationReloadGateInput
{
    PostPacketDiagnosticReturnedNormally,
    FreshPostDiagnosticEsp,
    FreshPostDiagnosticEsi,
    FreshReloadedOperation
}

public enum GameSocketReceiveOperationReloadGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceiveOperationReloadGateCaptureStates(
    GameSocketReceiveOperationReloadGateCaptureState PostPacketDiagnosticReturnedNormally,
    GameSocketReceiveOperationReloadGateCaptureState FreshPostDiagnosticEsp,
    GameSocketReceiveOperationReloadGateCaptureState FreshPostDiagnosticEsi,
    GameSocketReceiveOperationReloadGateCaptureState FreshReloadedOperation);

public enum GameSocketReceiveOperationReloadGateRoute
{
    None,
    DirectNonzero,
    ZeroDiagnostic
}

public enum GameSocketReceiveOperationReloadGateStop
{
    ParentDidNotReachCleanEndpoint,
    ParentEndpointWasDirty,
    AwaitingPostPacketDiagnosticReturnAtF5B,
    PostPacketDiagnosticDidNotReturnNormally,
    AwaitingFreshPostDiagnosticEspAtF61,
    AwaitingFreshPostDiagnosticEsiAtF64,
    AwaitingFreshReloadedOperationAtF64,
    NonzeroBranchTargetAtF88,
    DiagnosticImportBoundaryAtF7F
}

public enum GameSocketReceiveOperationReloadGateInstructionKind
{
    Add10ToPostDiagnosticEsp,
    ReloadOperationFromEsiDf4,
    StoreOperationAtEsp18,
    TestOperation,
    BranchIfOperationNonzero,
    PushOperation,
    PushToken5B,
    PushDiagnosticArgument,
    CallDiagnosticImport
}

public readonly record struct GameSocketReceiveOperationReloadGateInstruction(
    GameSocketReceiveOperationReloadGateInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null);

public enum GameSocketReceiveOperationReloadGateMemoryEffectKind
{
    FreshReloadedOperationRead,
    OperationStackWrite,
    PushOperationStackWrite,
    PushTokenStackWrite,
    PushDiagnosticArgumentStackWrite
}

public readonly record struct GameSocketReceiveOperationReloadGateMemoryEffect(
    GameSocketReceiveOperationReloadGateMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        Kind is not GameSocketReceiveOperationReloadGateMemoryEffectKind.FreshReloadedOperationRead
        && Address.HasValue
        && Value.HasValue;
}

public enum GameSocketReceiveOperationReloadGateCallKind
{
    PostPacketDiagnosticImport,
    DiagnosticImportBoundary
}

public readonly record struct GameSocketReceiveOperationReloadGateCall(
    GameSocketReceiveOperationReloadGateCallKind Kind,
    uint CallSiteRawFileOffset,
    uint? ImportCellVirtualAddress,
    bool TargetCaptured,
    bool CallSiteReached,
    bool Attempted,
    bool? ReturnedNormally,
    ImmutableArray<uint?> StackArguments);

public readonly record struct GameSocketReceiveOperationReloadGateTraceStep(
    int Sequence,
    GameSocketReceivePostPacketStackPreparationTraceStep? ParentMemoryEffect,
    GameSocketReceiveOperationReloadGateMemoryEffect? ReloadGateMemoryEffect);

public static class GameSocketReceiveOperationReloadGateProjection
{
    public const uint MethodRawFileOffset = 0x00BD2F64;
    public const uint ParentDiagnosticCallRawFileOffset = 0x00BD2F5B;
    public const uint PostDiagnosticStackCleanupRawFileOffset = 0x00BD2F61;
    public const uint OperationReloadRawFileOffset = 0x00BD2F64;
    public const uint OperationStoreRawFileOffset = 0x00BD2F6A;
    public const uint OperationTestRawFileOffset = 0x00BD2F6E;
    public const uint OperationBranchRawFileOffset = 0x00BD2F70;
    public const uint NonzeroBranchTargetRawFileOffset = 0x00BD2F88;
    public const uint PushOperationRawFileOffset = 0x00BD2F72;
    public const uint PushTokenRawFileOffset = 0x00BD2F73;
    public const uint PushDiagnosticArgumentRawFileOffset = 0x00BD2F75;
    public const uint PushDiagnosticArgumentAgainRawFileOffset = 0x00BD2F7A;
    public const uint DiagnosticImportCallRawFileOffset = 0x00BD2F7F;
    public const uint PostDiagnosticStackCleanupBytes = 0x10;
    public const uint OperationFieldOffset = 0x0DF4;
    public const uint OperationStackOffset = 0x18;
    public const uint TokenArgument = 0x5B;
    public const uint DiagnosticArgumentVirtualAddress = 0x01C5EA58;
    public const uint DiagnosticImportCellVirtualAddress = 0x01C15EA0;

    // The byte window includes the F85 cleanup instruction; projection stops before the F7F call.
    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x86, 0xF4, 0x0D, 0x00, 0x00,
            0x89, 0x44, 0x24, 0x18,
            0x85, 0xC0,
            0x75, 0x16,
            0x50,
            0x6A, 0x5B,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
            0x83, 0xC4, 0x10);

    public static GameSocketReceiveOperationReloadGateProjectionResult Project(
        GameSocketReceiveOperationReloadGateProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceivePostPacketStackPreparationProjectionResult parent =
            GameSocketReceivePostPacketStackPreparationProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceiveOperationReloadGateInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceiveOperationReloadGateMemoryEffect>();
        var trace =
            ImmutableArray.CreateBuilder<GameSocketReceiveOperationReloadGateTraceStep>();
        var calls = ImmutableArray.CreateBuilder<GameSocketReceiveOperationReloadGateCall>();
        var consumed = new HashSet<GameSocketReceiveOperationReloadGateInput>();

        foreach (GameSocketReceivePostPacketStackPreparationTraceStep step
                 in parent.OrderedMemoryEffects)
            trace.Add(new(trace.Count, step, null));

        uint? postDiagnosticEspAfterCleanup = null;
        uint? operationAddress = null;
        uint? operationStackAddress = null;
        uint? stackPointerAtDiagnosticCall = null;
        bool parentHandoffAdmitted = false;
        bool diagnosticCallSiteReached = false;
        bool? nonzeroBranchTaken = null;
        GameSocketReceiveOperationReloadGateRoute route =
            GameSocketReceiveOperationReloadGateRoute.None;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;

        void AddMemoryEffect(GameSocketReceiveOperationReloadGateMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            trace.Add(new(trace.Count, null, effect));
        }

        GameSocketReceiveOperationReloadGateProjectionResult Finish(
            GameSocketReceiveOperationReloadGateStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceiveOperationReloadGateInput>();
            foreach (GameSocketReceiveOperationReloadGateInput input
                     in Enum.GetValues<GameSocketReceiveOperationReloadGateInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceiveOperationReloadGateCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveOperationReloadGateInput.PostPacketDiagnosticReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsi),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveOperationReloadGateInput.FreshReloadedOperation));

            return new(
                inputs,
                parent,
                parentHandoffAdmitted,
                route,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                memoryEffects.ToImmutable(),
                trace.ToImmutable(),
                calls.ToImmutable(),
                unused.ToImmutable(),
                captureStates,
                postDiagnosticEspAfterCleanup,
                operationAddress,
                operationStackAddress,
                stackPointerAtDiagnosticCall,
                nonzeroBranchTaken,
                diagnosticCallSiteReached,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        bool parentReachedDirectRoute =
            parent.Stop == GameSocketReceivePostPacketStackPreparationStop
                .NonzeroBranchTargetAtF64
            && parent.NonzeroBranchTaken == true;
        bool parentReachedDiagnosticRoute =
            parent.Stop == GameSocketReceivePostPacketStackPreparationStop
                .DiagnosticImportBoundaryAtF5B
            && parent.NonzeroBranchTaken == false;

        if (!parentReachedDirectRoute && !parentReachedDiagnosticRoute)
        {
            return Finish(
                GameSocketReceiveOperationReloadGateStop.ParentDidNotReachCleanEndpoint,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceiveOperationReloadGateStop.ParentEndpointWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        if (parentReachedDirectRoute)
        {
            route = GameSocketReceiveOperationReloadGateRoute.DirectNonzero;
        }
        else
        {
            route = GameSocketReceiveOperationReloadGateRoute.ZeroDiagnostic;
            calls.Add(new(
                GameSocketReceiveOperationReloadGateCallKind.PostPacketDiagnosticImport,
                ParentDiagnosticCallRawFileOffset,
                GameSocketReceivePostPacketStackPreparationProjection
                    .DiagnosticImportCellVirtualAddress,
                false,
                true,
                true,
                inputs.PostPacketDiagnosticReturnedNormally,
                ImmutableArray.Create<uint?>(
                    GameSocketReceivePostPacketStackPreparationProjection
                        .DiagnosticArgumentVirtualAddress,
                    GameSocketReceivePostPacketStackPreparationProjection
                        .DiagnosticArgumentVirtualAddress,
                    GameSocketReceivePostPacketStackPreparationProjection.TokenArgument,
                    0)));
            if (inputs.PostPacketDiagnosticReturnedNormally is not bool diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveOperationReloadGateStop
                        .AwaitingPostPacketDiagnosticReturnAtF5B,
                    ParentDiagnosticCallRawFileOffset);
            }

            consumed.Add(GameSocketReceiveOperationReloadGateInput
                .PostPacketDiagnosticReturnedNormally);
            calls[0] = calls[0] with { ReturnedNormally = diagnosticReturned };
            if (!diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveOperationReloadGateStop
                        .PostPacketDiagnosticDidNotReturnNormally,
                    ParentDiagnosticCallRawFileOffset);
            }

            flagsAtStop = null;
            instructions.Add(new(
                GameSocketReceiveOperationReloadGateInstructionKind
                    .Add10ToPostDiagnosticEsp,
                PostDiagnosticStackCleanupRawFileOffset,
                OperandValue: PostDiagnosticStackCleanupBytes));
            if (inputs.FreshPostDiagnosticEsp is not uint postDiagnosticEsp)
            {
                return Finish(
                    GameSocketReceiveOperationReloadGateStop
                        .AwaitingFreshPostDiagnosticEspAtF61,
                    PostDiagnosticStackCleanupRawFileOffset);
            }

            consumed.Add(GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsp);
            postDiagnosticEspAfterCleanup =
                unchecked(postDiagnosticEsp + PostDiagnosticStackCleanupBytes);
            instructions[^1] = instructions[^1] with
            {
                EffectiveAddress = postDiagnosticEsp,
                ResultValue = postDiagnosticEspAfterCleanup
            };
            flagsAtStop = AddDwordFlags(postDiagnosticEsp, PostDiagnosticStackCleanupBytes);
        }

        uint? esi;
        uint? esp;
        if (route == GameSocketReceiveOperationReloadGateRoute.DirectNonzero)
        {
            esi = parent.Inputs.FreshPostPacketEsi;
            esp = parent.Inputs.FreshPostPacketEsp;
        }
        else
        {
            esi = inputs.FreshPostDiagnosticEsi;
            esp = postDiagnosticEspAfterCleanup;
            if (inputs.FreshPostDiagnosticEsi is not uint)
            {
                return Finish(
                    GameSocketReceiveOperationReloadGateStop
                        .AwaitingFreshPostDiagnosticEsiAtF64,
                    OperationReloadRawFileOffset);
            }

            consumed.Add(GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsi);
        }

        operationAddress = unchecked(esi!.Value + OperationFieldOffset);
        instructions.Add(new(
            GameSocketReceiveOperationReloadGateInstructionKind.ReloadOperationFromEsiDf4,
            OperationReloadRawFileOffset,
            EffectiveAddress: operationAddress,
            OperandValue: inputs.FreshReloadedOperation));
        AddMemoryEffect(new(
            GameSocketReceiveOperationReloadGateMemoryEffectKind.FreshReloadedOperationRead,
            OperationReloadRawFileOffset,
            operationAddress,
            inputs.FreshReloadedOperation,
            true,
            inputs.FreshReloadedOperation.HasValue));

        if (inputs.FreshReloadedOperation is not uint operation)
        {
            return Finish(
                GameSocketReceiveOperationReloadGateStop
                    .AwaitingFreshReloadedOperationAtF64,
                OperationReloadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveOperationReloadGateInput.FreshReloadedOperation);
        operationStackAddress = unchecked(esp!.Value + OperationStackOffset);
        instructions.Add(new(
            GameSocketReceiveOperationReloadGateInstructionKind.StoreOperationAtEsp18,
            OperationStoreRawFileOffset,
            EffectiveAddress: operationStackAddress,
            OperandValue: operation,
            ResultValue: operation));
        AddMemoryEffect(new(
            GameSocketReceiveOperationReloadGateMemoryEffectKind.OperationStackWrite,
            OperationStoreRawFileOffset,
            operationStackAddress,
            operation,
            true,
            true));

        instructions.Add(new(
            GameSocketReceiveOperationReloadGateInstructionKind.TestOperation,
            OperationTestRawFileOffset,
            OperandValue: operation));
        flagsAtStop = TestDwordFlags(operation);
        bool isNonzero = operation != 0;
        nonzeroBranchTaken = isNonzero;
        instructions.Add(new(
            GameSocketReceiveOperationReloadGateInstructionKind.BranchIfOperationNonzero,
            OperationBranchRawFileOffset,
            TargetRawFileOffset: NonzeroBranchTargetRawFileOffset,
            BranchTaken: isNonzero));
        if (isNonzero)
        {
            return Finish(
                GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88,
                NonzeroBranchTargetRawFileOffset,
                true);
        }

        uint stackPointer = esp.Value;
        stackPointer = Push(
            GameSocketReceiveOperationReloadGateInstructionKind.PushOperation,
            GameSocketReceiveOperationReloadGateMemoryEffectKind.PushOperationStackWrite,
            PushOperationRawFileOffset,
            operation,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveOperationReloadGateInstructionKind.PushToken5B,
            GameSocketReceiveOperationReloadGateMemoryEffectKind.PushTokenStackWrite,
            PushTokenRawFileOffset,
            TokenArgument,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveOperationReloadGateInstructionKind.PushDiagnosticArgument,
            GameSocketReceiveOperationReloadGateMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveOperationReloadGateInstructionKind.PushDiagnosticArgument,
            GameSocketReceiveOperationReloadGateMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentAgainRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);
        stackPointerAtDiagnosticCall = stackPointer;
        diagnosticCallSiteReached = true;
        instructions.Add(new(
            GameSocketReceiveOperationReloadGateInstructionKind.CallDiagnosticImport,
            DiagnosticImportCallRawFileOffset,
            EffectiveAddress: DiagnosticImportCellVirtualAddress,
            OperandValue: stackPointer));
        calls.Add(new(
            GameSocketReceiveOperationReloadGateCallKind.DiagnosticImportBoundary,
            DiagnosticImportCallRawFileOffset,
            DiagnosticImportCellVirtualAddress,
            false,
            true,
            false,
            null,
            ImmutableArray.Create<uint?>(
                DiagnosticArgumentVirtualAddress,
                DiagnosticArgumentVirtualAddress,
                TokenArgument,
                operation)));
        return Finish(
            GameSocketReceiveOperationReloadGateStop.DiagnosticImportBoundaryAtF7F,
            DiagnosticImportCallRawFileOffset,
            true);

        uint Push(
            GameSocketReceiveOperationReloadGateInstructionKind instructionKind,
            GameSocketReceiveOperationReloadGateMemoryEffectKind effectKind,
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

    private static GameSocketReceiveResultGateFlags AddDwordFlags(uint left, uint right)
    {
        ulong sum = (ulong)left + right;
        uint result = (uint)sum;
        bool leftSign = (left & 0x8000_0000) != 0;
        bool rightSign = (right & 0x8000_0000) != 0;
        bool resultSign = (result & 0x8000_0000) != 0;
        return new(
            sum > uint.MaxValue,
            HasEvenParity((byte)result),
            ((left & 0xF) + (right & 0xF)) > 0xF,
            result == 0,
            resultSign,
            leftSign == rightSign && resultSign != leftSign);
    }

    private static GameSocketReceiveResultGateFlags TestDwordFlags(uint value) =>
        new(
            false,
            HasEvenParity((byte)value),
            null,
            value == 0,
            (value & 0x8000_0000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static GameSocketReceiveOperationReloadGateCaptureState GetCaptureState(
        GameSocketReceiveOperationReloadGateProjectionInputs inputs,
        HashSet<GameSocketReceiveOperationReloadGateInput> consumed,
        GameSocketReceiveOperationReloadGateStop stop,
        GameSocketReceiveOperationReloadGateInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceiveOperationReloadGateCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceiveOperationReloadGateCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceiveOperationReloadGateInput.PostPacketDiagnosticReturnedNormally =>
                stop == GameSocketReceiveOperationReloadGateStop
                    .AwaitingPostPacketDiagnosticReturnAtF5B,
            GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsp =>
                stop == GameSocketReceiveOperationReloadGateStop
                    .AwaitingFreshPostDiagnosticEspAtF61,
            GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsi =>
                stop == GameSocketReceiveOperationReloadGateStop
                    .AwaitingFreshPostDiagnosticEsiAtF64,
            GameSocketReceiveOperationReloadGateInput.FreshReloadedOperation =>
                stop == GameSocketReceiveOperationReloadGateStop
                    .AwaitingFreshReloadedOperationAtF64,
            _ => false
        };
        return required
            ? GameSocketReceiveOperationReloadGateCaptureState.Missing
            : GameSocketReceiveOperationReloadGateCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceiveOperationReloadGateProjectionInputs inputs,
        GameSocketReceiveOperationReloadGateInput input) => input switch
    {
        GameSocketReceiveOperationReloadGateInput.PostPacketDiagnosticReturnedNormally =>
            inputs.PostPacketDiagnosticReturnedNormally.HasValue,
        GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsp =>
            inputs.FreshPostDiagnosticEsp.HasValue,
        GameSocketReceiveOperationReloadGateInput.FreshPostDiagnosticEsi =>
            inputs.FreshPostDiagnosticEsi.HasValue,
        GameSocketReceiveOperationReloadGateInput.FreshReloadedOperation =>
            inputs.FreshReloadedOperation.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceiveOperationReloadGateProjectionResult(
    GameSocketReceiveOperationReloadGateProjectionInputs Inputs,
    GameSocketReceivePostPacketStackPreparationProjectionResult ParentProjection,
    bool ParentHandoffAdmitted,
    GameSocketReceiveOperationReloadGateRoute Route,
    GameSocketReceiveOperationReloadGateStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceiveOperationReloadGateInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceiveOperationReloadGateMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceiveOperationReloadGateTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceiveOperationReloadGateCall> Calls,
    ImmutableArray<GameSocketReceiveOperationReloadGateInput> SuppliedButUnusedInputs,
    GameSocketReceiveOperationReloadGateCaptureStates CaptureStates,
    uint? PostDiagnosticEspAfterCleanup,
    uint? OperationAddress,
    uint? OperationStackAddress,
    uint? StackPointerAtDiagnosticCall,
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
            step.ReloadGateMemoryEffect is { Committed: true });

    public bool DiagnosticImportExecuted => false;
    public bool RepostApiModeled => false;
    public bool BranchTargetF88Entered => false;
}
