using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceiveBufferStatusGateProjectionInputs(
    GameSocketReceiveOperationReloadGateProjectionInputs ParentInputs,
    bool? DiagnosticReturnedNormally,
    uint? FreshPostDiagnosticEsp,
    uint? FreshPostDiagnosticEsi,
    uint? FreshFirstReadBase,
    uint? FreshComparisonValue);

public enum GameSocketReceiveBufferStatusGateInput
{
    DiagnosticReturnedNormally,
    FreshPostDiagnosticEsp,
    FreshPostDiagnosticEsi,
    FreshFirstReadBase,
    FreshComparisonValue
}

public enum GameSocketReceiveBufferStatusGateCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceiveBufferStatusGateCaptureStates(
    GameSocketReceiveBufferStatusGateCaptureState DiagnosticReturnedNormally,
    GameSocketReceiveBufferStatusGateCaptureState FreshPostDiagnosticEsp,
    GameSocketReceiveBufferStatusGateCaptureState FreshPostDiagnosticEsi,
    GameSocketReceiveBufferStatusGateCaptureState FreshFirstReadBase,
    GameSocketReceiveBufferStatusGateCaptureState FreshComparisonValue);

public enum GameSocketReceiveBufferStatusGateRoute
{
    None,
    DirectNonzero,
    ZeroDiagnostic
}

public enum GameSocketReceiveBufferStatusGateStop
{
    ParentDidNotReachCleanEndpoint,
    ParentEndpointWasDirty,
    AwaitingDiagnosticReturnAtF7F,
    DiagnosticDidNotReturnNormally,
    AwaitingFreshPostDiagnosticEspAtF85,
    AwaitingFreshPostDiagnosticEsiAtF88,
    AwaitingFreshFirstReadBaseAtF88,
    AwaitingFreshComparisonValueAtF9A,
    NonzeroBranchTargetAtFBA,
    DiagnosticImportBoundaryAtFB1
}

public enum GameSocketReceiveBufferStatusGateInstructionKind
{
    Add10ToPostDiagnosticEsp,
    ReadFirstBaseFromEsiDf4,
    Add98ToFirstBase,
    Store80AtEbx,
    CompareFreshValueAtEsiDf4,
    BranchIfComparisonNonzero,
    PushZero,
    PushToken5B,
    PushDiagnosticArgument,
    CallDiagnosticImport
}

public readonly record struct GameSocketReceiveBufferStatusGateInstruction(
    GameSocketReceiveBufferStatusGateInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null);

public enum GameSocketReceiveBufferStatusGateMemoryEffectKind
{
    FreshFirstReadBaseRead,
    Store80Write,
    FreshComparisonValueRead,
    PushZeroStackWrite,
    PushTokenStackWrite,
    PushDiagnosticArgumentStackWrite
}

public readonly record struct GameSocketReceiveBufferStatusGateMemoryEffect(
    GameSocketReceiveBufferStatusGateMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        Kind is not GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshFirstReadBaseRead
            and not GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshComparisonValueRead
        && Address.HasValue
        && Value.HasValue;
}

public enum GameSocketReceiveBufferStatusGateCallKind
{
    F7FDiagnosticImport,
    FB1DiagnosticImportBoundary
}

public readonly record struct GameSocketReceiveBufferStatusGateCall(
    GameSocketReceiveBufferStatusGateCallKind Kind,
    uint CallSiteRawFileOffset,
    uint? ImportCellVirtualAddress,
    bool TargetCaptured,
    bool CallSiteReached,
    bool Attempted,
    bool? ReturnedNormally,
    ImmutableArray<uint?> StackArguments);

public readonly record struct GameSocketReceiveBufferStatusGateTraceStep(
    int Sequence,
    GameSocketReceiveOperationReloadGateTraceStep? ParentMemoryEffect,
    GameSocketReceiveBufferStatusGateMemoryEffect? GateMemoryEffect);

public static class GameSocketReceiveBufferStatusGateProjection
{
    public const uint MethodRawFileOffset = 0x00BD2F88;
    public const uint FirstReadRawFileOffset = 0x00BD2F88;
    public const uint Add98RawFileOffset = 0x00BD2F8E;
    public const uint Store80RawFileOffset = 0x00BD2F94;
    public const uint ComparisonRawFileOffset = 0x00BD2F9A;
    public const uint BranchRawFileOffset = 0x00BD2FA1;
    public const uint NonzeroBranchTargetRawFileOffset = 0x00BD2FBA;
    public const uint PushZeroRawFileOffset = 0x00BD2FA3;
    public const uint PushTokenRawFileOffset = 0x00BD2FA5;
    public const uint PushDiagnosticArgumentRawFileOffset = 0x00BD2FA7;
    public const uint PushDiagnosticArgumentAgainRawFileOffset = 0x00BD2FAC;
    public const uint DiagnosticImportCallRawFileOffset = 0x00BD2FB1;
    public const uint PostImportStackCleanupRawFileOffset = 0x00BD2FB7;
    public const uint PostDiagnosticStackCleanupRawFileOffset = 0x00BD2F85;
    public const uint PostDiagnosticStackCleanupBytes = 0x10;
    public const uint OperationFieldOffset = 0x0DF4;
    public const uint FirstBaseAddend = 0x98;
    public const uint StoreValue = 0x80;
    public const uint TokenArgument = 0x5B;
    public const uint DiagnosticArgumentVirtualAddress = 0x01C5EA58;
    public const uint DiagnosticImportCellVirtualAddress = 0x01C15EA0;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x9E, 0xF4, 0x0D, 0x00, 0x00,
            0x81, 0xC3, 0x98, 0x00, 0x00, 0x00,
            0xC7, 0x03, 0x80, 0x00, 0x00, 0x00,
            0x83, 0xBE, 0xF4, 0x0D, 0x00, 0x00, 0x00,
            0x75, 0x17,
            0x6A, 0x00,
            0x6A, 0x5B,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0x68, 0x58, 0xEA, 0xC5, 0x01,
            0xFF, 0x15, 0xA0, 0x5E, 0xC1, 0x01,
            0x83, 0xC4, 0x10);

    public static GameSocketReceiveBufferStatusGateProjectionResult Project(
        GameSocketReceiveBufferStatusGateProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceiveOperationReloadGateProjectionResult parent =
            GameSocketReceiveOperationReloadGateProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceiveBufferStatusGateInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceiveBufferStatusGateMemoryEffect>();
        var trace =
            ImmutableArray.CreateBuilder<GameSocketReceiveBufferStatusGateTraceStep>();
        var calls = ImmutableArray.CreateBuilder<GameSocketReceiveBufferStatusGateCall>();
        var consumed = new HashSet<GameSocketReceiveBufferStatusGateInput>();

        foreach (GameSocketReceiveOperationReloadGateTraceStep step
                 in parent.OrderedMemoryEffects)
            trace.Add(new(trace.Count, step, null));

        bool parentHandoffAdmitted = false;
        uint? effectiveEsi = null;
        uint? effectiveEsp = null;
        uint? parentRouteEsp = null;
        uint? postDiagnosticEspAfterCleanup = null;
        uint? firstReadAddress = null;
        uint? ebxAfterAdd = null;
        uint? comparisonAddress = null;
        uint? stackPointerAtDiagnosticCall = null;
        bool? comparisonBranchTaken = null;
        bool diagnosticCallSiteReached = false;
        GameSocketReceiveBufferStatusGateRoute route =
            GameSocketReceiveBufferStatusGateRoute.None;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;

        void AddMemoryEffect(GameSocketReceiveBufferStatusGateMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            trace.Add(new(trace.Count, null, effect));
        }

        GameSocketReceiveBufferStatusGateProjectionResult Finish(
            GameSocketReceiveBufferStatusGateStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceiveBufferStatusGateInput>();
            foreach (GameSocketReceiveBufferStatusGateInput input
                     in Enum.GetValues<GameSocketReceiveBufferStatusGateInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceiveBufferStatusGateCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveBufferStatusGateInput.DiagnosticReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsi),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveBufferStatusGateInput.FreshFirstReadBase),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveBufferStatusGateInput.FreshComparisonValue));

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
                effectiveEsi,
                effectiveEsp,
                parentRouteEsp,
                postDiagnosticEspAfterCleanup,
                firstReadAddress,
                ebxAfterAdd,
                comparisonAddress,
                stackPointerAtDiagnosticCall,
                comparisonBranchTaken,
                diagnosticCallSiteReached,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        bool reachedF88 =
            parent.Stop == GameSocketReceiveOperationReloadGateStop.NonzeroBranchTargetAtF88
            && parent.NonzeroBranchTaken == true;
        bool reachedF7F =
            parent.Stop
                == GameSocketReceiveOperationReloadGateStop.DiagnosticImportBoundaryAtF7F
            && parent.NonzeroBranchTaken == false
            && parent.DiagnosticCallSiteReached;

        if (!reachedF88 && !reachedF7F)
        {
            return Finish(
                GameSocketReceiveBufferStatusGateStop.ParentDidNotReachCleanEndpoint,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceiveBufferStatusGateStop.ParentEndpointWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        route = reachedF88
            ? GameSocketReceiveBufferStatusGateRoute.DirectNonzero
            : GameSocketReceiveBufferStatusGateRoute.ZeroDiagnostic;

        if (reachedF88)
        {
            if (parent.Route == GameSocketReceiveOperationReloadGateRoute.DirectNonzero)
            {
                effectiveEsi = parent.ParentProjection.Inputs.FreshPostPacketEsi;
                parentRouteEsp = parent.ParentProjection.Inputs.FreshPostPacketEsp;
            }
            else if (parent.Route == GameSocketReceiveOperationReloadGateRoute.ZeroDiagnostic)
            {
                effectiveEsi = parent.Inputs.FreshPostDiagnosticEsi;
                parentRouteEsp = parent.PostDiagnosticEspAfterCleanup;
            }

            if (!effectiveEsi.HasValue || !parentRouteEsp.HasValue)
            {
                return Finish(
                    GameSocketReceiveBufferStatusGateStop.ParentDidNotReachCleanEndpoint,
                    parent.StopAtRawFileOffset);
            }

            effectiveEsp = parentRouteEsp;
        }

        if (reachedF7F)
        {
            calls.Add(new(
                GameSocketReceiveBufferStatusGateCallKind.F7FDiagnosticImport,
                parent.StopAtRawFileOffset,
                GameSocketReceiveOperationReloadGateProjection.DiagnosticImportCellVirtualAddress,
                false,
                true,
                true,
                inputs.DiagnosticReturnedNormally,
                parent.Calls[^1].StackArguments));
            if (inputs.DiagnosticReturnedNormally is not bool diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveBufferStatusGateStop.AwaitingDiagnosticReturnAtF7F,
                    parent.StopAtRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveBufferStatusGateInput.DiagnosticReturnedNormally);
            calls[0] = calls[0] with { ReturnedNormally = diagnosticReturned };
            if (!diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveBufferStatusGateStop.DiagnosticDidNotReturnNormally,
                    parent.StopAtRawFileOffset);
            }

            flagsAtStop = null;
            instructions.Add(new(
                GameSocketReceiveBufferStatusGateInstructionKind.Add10ToPostDiagnosticEsp,
                PostDiagnosticStackCleanupRawFileOffset,
                OperandValue: PostDiagnosticStackCleanupBytes));
            if (inputs.FreshPostDiagnosticEsp is not uint postDiagnosticEsp)
            {
                return Finish(
                    GameSocketReceiveBufferStatusGateStop
                        .AwaitingFreshPostDiagnosticEspAtF85,
                    PostDiagnosticStackCleanupRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsp);
            postDiagnosticEspAfterCleanup =
                unchecked(postDiagnosticEsp + PostDiagnosticStackCleanupBytes);
            effectiveEsp = postDiagnosticEspAfterCleanup;
            instructions[^1] = instructions[^1] with
            {
                EffectiveAddress = postDiagnosticEsp,
                ResultValue = postDiagnosticEspAfterCleanup
            };
            flagsAtStop = AddDwordFlags(
                postDiagnosticEsp,
                PostDiagnosticStackCleanupBytes);

            if (inputs.FreshPostDiagnosticEsi is not uint postDiagnosticEsi)
            {
                return Finish(
                    GameSocketReceiveBufferStatusGateStop
                        .AwaitingFreshPostDiagnosticEsiAtF88,
                    FirstReadRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsi);
            effectiveEsi = postDiagnosticEsi;
        }

        firstReadAddress = unchecked(effectiveEsi!.Value + OperationFieldOffset);
        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.ReadFirstBaseFromEsiDf4,
            FirstReadRawFileOffset,
            EffectiveAddress: firstReadAddress,
            OperandValue: inputs.FreshFirstReadBase));
        AddMemoryEffect(new(
            GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshFirstReadBaseRead,
            FirstReadRawFileOffset,
            firstReadAddress,
            inputs.FreshFirstReadBase,
            true,
            inputs.FreshFirstReadBase.HasValue));
        if (inputs.FreshFirstReadBase is not uint firstReadBase)
        {
            return Finish(
                GameSocketReceiveBufferStatusGateStop.AwaitingFreshFirstReadBaseAtF88,
                FirstReadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveBufferStatusGateInput.FreshFirstReadBase);
        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.Add98ToFirstBase,
            Add98RawFileOffset,
            EffectiveAddress: null,
            OperandValue: FirstBaseAddend,
            ResultValue: unchecked(firstReadBase + FirstBaseAddend)));
        ebxAfterAdd = unchecked(firstReadBase + FirstBaseAddend);
        flagsAtStop = AddDwordFlags(firstReadBase, FirstBaseAddend);

        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.Store80AtEbx,
            Store80RawFileOffset,
            EffectiveAddress: ebxAfterAdd,
            OperandValue: StoreValue));
        AddMemoryEffect(new(
            GameSocketReceiveBufferStatusGateMemoryEffectKind.Store80Write,
            Store80RawFileOffset,
            ebxAfterAdd,
            StoreValue,
            true,
            true));

        comparisonAddress = unchecked(effectiveEsi.Value + OperationFieldOffset);
        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.CompareFreshValueAtEsiDf4,
            ComparisonRawFileOffset,
            EffectiveAddress: comparisonAddress,
            OperandValue: inputs.FreshComparisonValue));
        AddMemoryEffect(new(
            GameSocketReceiveBufferStatusGateMemoryEffectKind.FreshComparisonValueRead,
            ComparisonRawFileOffset,
            comparisonAddress,
            inputs.FreshComparisonValue,
            true,
            inputs.FreshComparisonValue.HasValue));
        if (inputs.FreshComparisonValue is not uint comparisonValue)
        {
            return Finish(
                GameSocketReceiveBufferStatusGateStop.AwaitingFreshComparisonValueAtF9A,
                ComparisonRawFileOffset);
        }

        consumed.Add(GameSocketReceiveBufferStatusGateInput.FreshComparisonValue);
        flagsAtStop = CompareDwordToZeroFlags(comparisonValue);
        bool isNonzero = comparisonValue != 0;
        comparisonBranchTaken = isNonzero;
        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.BranchIfComparisonNonzero,
            BranchRawFileOffset,
            TargetRawFileOffset: NonzeroBranchTargetRawFileOffset,
            BranchTaken: isNonzero));
        if (isNonzero)
        {
            return Finish(
                GameSocketReceiveBufferStatusGateStop.NonzeroBranchTargetAtFBA,
                NonzeroBranchTargetRawFileOffset,
                true);
        }

        uint stackPointer = effectiveEsp!.Value;
        stackPointer = Push(
            GameSocketReceiveBufferStatusGateInstructionKind.PushZero,
            GameSocketReceiveBufferStatusGateMemoryEffectKind.PushZeroStackWrite,
            PushZeroRawFileOffset,
            0,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveBufferStatusGateInstructionKind.PushToken5B,
            GameSocketReceiveBufferStatusGateMemoryEffectKind.PushTokenStackWrite,
            PushTokenRawFileOffset,
            TokenArgument,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveBufferStatusGateInstructionKind.PushDiagnosticArgument,
            GameSocketReceiveBufferStatusGateMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);
        stackPointer = Push(
            GameSocketReceiveBufferStatusGateInstructionKind.PushDiagnosticArgument,
            GameSocketReceiveBufferStatusGateMemoryEffectKind.PushDiagnosticArgumentStackWrite,
            PushDiagnosticArgumentAgainRawFileOffset,
            DiagnosticArgumentVirtualAddress,
            stackPointer);

        stackPointerAtDiagnosticCall = stackPointer;
        diagnosticCallSiteReached = true;
        instructions.Add(new(
            GameSocketReceiveBufferStatusGateInstructionKind.CallDiagnosticImport,
            DiagnosticImportCallRawFileOffset,
            EffectiveAddress: DiagnosticImportCellVirtualAddress,
            OperandValue: stackPointer));
        calls.Add(new(
            GameSocketReceiveBufferStatusGateCallKind.FB1DiagnosticImportBoundary,
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
                0)));
        return Finish(
            GameSocketReceiveBufferStatusGateStop.DiagnosticImportBoundaryAtFB1,
            DiagnosticImportCallRawFileOffset,
            true);

        uint Push(
            GameSocketReceiveBufferStatusGateInstructionKind instructionKind,
            GameSocketReceiveBufferStatusGateMemoryEffectKind effectKind,
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

    private static GameSocketReceiveResultGateFlags CompareDwordToZeroFlags(uint value) =>
        new(
            false,
            HasEvenParity((byte)value),
            false,
            value == 0,
            (value & 0x8000_0000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static GameSocketReceiveBufferStatusGateCaptureState GetCaptureState(
        GameSocketReceiveBufferStatusGateProjectionInputs inputs,
        HashSet<GameSocketReceiveBufferStatusGateInput> consumed,
        GameSocketReceiveBufferStatusGateStop stop,
        GameSocketReceiveBufferStatusGateInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceiveBufferStatusGateCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceiveBufferStatusGateCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceiveBufferStatusGateInput.DiagnosticReturnedNormally =>
                stop == GameSocketReceiveBufferStatusGateStop.AwaitingDiagnosticReturnAtF7F,
            GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsp =>
                stop == GameSocketReceiveBufferStatusGateStop.AwaitingFreshPostDiagnosticEspAtF85,
            GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsi =>
                stop == GameSocketReceiveBufferStatusGateStop.AwaitingFreshPostDiagnosticEsiAtF88,
            GameSocketReceiveBufferStatusGateInput.FreshFirstReadBase =>
                stop == GameSocketReceiveBufferStatusGateStop.AwaitingFreshFirstReadBaseAtF88,
            GameSocketReceiveBufferStatusGateInput.FreshComparisonValue =>
                stop == GameSocketReceiveBufferStatusGateStop.AwaitingFreshComparisonValueAtF9A,
            _ => false
        };
        return required
            ? GameSocketReceiveBufferStatusGateCaptureState.Missing
            : GameSocketReceiveBufferStatusGateCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceiveBufferStatusGateProjectionInputs inputs,
        GameSocketReceiveBufferStatusGateInput input) => input switch
    {
        GameSocketReceiveBufferStatusGateInput.DiagnosticReturnedNormally =>
            inputs.DiagnosticReturnedNormally.HasValue,
        GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsp =>
            inputs.FreshPostDiagnosticEsp.HasValue,
        GameSocketReceiveBufferStatusGateInput.FreshPostDiagnosticEsi =>
            inputs.FreshPostDiagnosticEsi.HasValue,
        GameSocketReceiveBufferStatusGateInput.FreshFirstReadBase =>
            inputs.FreshFirstReadBase.HasValue,
        GameSocketReceiveBufferStatusGateInput.FreshComparisonValue =>
            inputs.FreshComparisonValue.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceiveBufferStatusGateProjectionResult(
    GameSocketReceiveBufferStatusGateProjectionInputs Inputs,
    GameSocketReceiveOperationReloadGateProjectionResult ParentProjection,
    bool ParentHandoffAdmitted,
    GameSocketReceiveBufferStatusGateRoute Route,
    GameSocketReceiveBufferStatusGateStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceiveBufferStatusGateInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceiveBufferStatusGateMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceiveBufferStatusGateTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceiveBufferStatusGateCall> Calls,
    ImmutableArray<GameSocketReceiveBufferStatusGateInput> SuppliedButUnusedInputs,
    GameSocketReceiveBufferStatusGateCaptureStates CaptureStates,
    uint? EffectiveEsi,
    uint? EffectiveEsp,
    uint? ParentRouteEsp,
    uint? PostDiagnosticEspAfterCleanup,
    uint? FirstReadAddress,
    uint? EbxAfterAdd,
    uint? ComparisonAddress,
    uint? StackPointerAtDiagnosticCall,
    bool? ComparisonBranchTaken,
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
            step.GateMemoryEffect is { Committed: true });

    public bool F7FDiagnosticImportAttempted =>
        Calls.Any(call =>
            call.Kind == GameSocketReceiveBufferStatusGateCallKind.F7FDiagnosticImport
            && call.Attempted);

    public bool FB1DiagnosticImportExecuted => false;
    public bool DiagnosticImportExecuted => false;
    public bool RepostApiModeled => false;
    public bool BranchTargetFBAEntered => false;
}
