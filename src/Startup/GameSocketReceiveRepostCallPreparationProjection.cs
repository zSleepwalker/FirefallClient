using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceiveRepostCallPreparationProjectionInputs(
    GameSocketReceiveBufferStatusGateProjectionInputs ParentInputs,
    bool? DiagnosticReturnedNormally,
    uint? FreshPostDiagnosticEsp,
    uint? FreshPostDiagnosticEsi,
    uint? FreshPostDiagnosticEbx,
    uint? FreshOperationDword,
    uint? FreshStackDword,
    uint? FreshSocketDword);

public enum GameSocketReceiveRepostCallPreparationInput
{
    DiagnosticReturnedNormally,
    FreshPostDiagnosticEsp,
    FreshPostDiagnosticEsi,
    FreshPostDiagnosticEbx,
    FreshOperationDword,
    FreshStackDword,
    FreshSocketDword
}

public enum GameSocketReceiveRepostCallPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceiveRepostCallPreparationCaptureStates(
    GameSocketReceiveRepostCallPreparationCaptureState DiagnosticReturnedNormally,
    GameSocketReceiveRepostCallPreparationCaptureState FreshPostDiagnosticEsp,
    GameSocketReceiveRepostCallPreparationCaptureState FreshPostDiagnosticEsi,
    GameSocketReceiveRepostCallPreparationCaptureState FreshPostDiagnosticEbx,
    GameSocketReceiveRepostCallPreparationCaptureState FreshOperationDword,
    GameSocketReceiveRepostCallPreparationCaptureState FreshStackDword,
    GameSocketReceiveRepostCallPreparationCaptureState FreshSocketDword);

public enum GameSocketReceiveRepostCallPreparationRoute
{
    None,
    DirectNonzero,
    ZeroDiagnostic
}

public enum GameSocketReceiveRepostCallPreparationStop
{
    ParentDidNotReachRecognizedEndpoint,
    ParentEndpointWasDirty,
    AwaitingDiagnosticReturnAtFB1,
    DiagnosticDidNotReturnNormally,
    AwaitingFreshPostDiagnosticEspAtFB7,
    AwaitingFreshPostDiagnosticEsiAtFBA,
    AwaitingFreshPostDiagnosticEbxAtFBA,
    AwaitingFreshOperationDwordAtFBA,
    AwaitingFreshStackDwordAtFC2,
    AwaitingFreshSocketDwordAtFDC,
    ExclusiveStopAtFE2
}

public enum GameSocketReceiveRepostCallPreparationInstructionKind
{
    Add10ToPostDiagnosticEsp,
    ReadOperationDwordFromEsiDf4,
    PushZero,
    ReadAndPushStackDwordFromEsp1C,
    Add18ToEax,
    PushEbx,
    PushEax,
    LeaEaxFromEsp2C,
    PushLeaEaxFromEsp2C,
    LeaEaxFromEsp34,
    PushLeaEaxFromEsp34,
    PushOne,
    LeaEaxFromEsp40,
    PushLeaEaxFromEsp40,
    ReadAndPushSocketDwordFromEsi118
}

public readonly record struct GameSocketReceiveRepostCallPreparationInstruction(
    GameSocketReceiveRepostCallPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null,
    uint? StackAddress = null,
    GameSocketReceiveResultGateFlags? FlagsAfter = null);

public enum GameSocketReceiveRepostCallPreparationMemoryEffectKind
{
    FreshOperationDwordRead,
    FreshStackDwordRead,
    FreshSocketDwordRead,
    PushStackWrite
}

public readonly record struct GameSocketReceiveRepostCallPreparationMemoryEffect(
    GameSocketReceiveRepostCallPreparationMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        Kind == GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite
        && Address.HasValue
        && Value.HasValue;
}

public readonly record struct GameSocketReceiveRepostCallPreparationTraceStep(
    int Sequence,
    GameSocketReceiveBufferStatusGateTraceStep? ParentTraceStep,
    GameSocketReceiveRepostCallPreparationMemoryEffect? PreparationMemoryEffect);

public static class GameSocketReceiveRepostCallPreparationProjection
{
    public const uint MethodRawFileOffset = 0x00BD2FBA;
    public const uint DiagnosticImportBoundaryRawFileOffset = 0x00BD2FB1;
    public const uint PostDiagnosticStackCleanupRawFileOffset = 0x00BD2FB7;
    public const uint PostDiagnosticStackCleanupBytes = 0x10;
    public const uint OperationReadRawFileOffset = 0x00BD2FBA;
    public const uint PushZeroRawFileOffset = 0x00BD2FC0;
    public const uint StackDwordReadPushRawFileOffset = 0x00BD2FC2;
    public const uint Add18ToEaxRawFileOffset = 0x00BD2FC6;
    public const uint PushEbxRawFileOffset = 0x00BD2FC9;
    public const uint PushEaxRawFileOffset = 0x00BD2FCA;
    public const uint LeaEaxFromEsp2CRawFileOffset = 0x00BD2FCB;
    public const uint PushLeaEaxFromEsp2CRawFileOffset = 0x00BD2FCF;
    public const uint LeaEaxFromEsp34RawFileOffset = 0x00BD2FD0;
    public const uint PushLeaEaxFromEsp34RawFileOffset = 0x00BD2FD4;
    public const uint PushOneRawFileOffset = 0x00BD2FD5;
    public const uint LeaEaxFromEsp40RawFileOffset = 0x00BD2FD7;
    public const uint PushLeaEaxFromEsp40RawFileOffset = 0x00BD2FDB;
    public const uint SocketDwordReadPushRawFileOffset = 0x00BD2FDC;
    public const uint ExclusiveStopRawFileOffset = 0x00BD2FE2;
    public const uint OperationFieldOffset = 0x0DF4;
    public const uint SocketFieldOffset = 0x118;
    public const uint StackReadDisplacement = 0x1C;
    public const uint EaxAddend = 0x18;
    public const uint FollowingCellVirtualAddress = 0x01AC4D54;

    public static ImmutableArray<byte> WindowBytes { get; } =
        ImmutableArray.Create<byte>(
            0x8B, 0x86, 0xF4, 0x0D, 0x00, 0x00,
            0x6A, 0x00,
            0xFF, 0x74, 0x24, 0x1C,
            0x83, 0xC0, 0x18,
            0x53,
            0x50,
            0x8D, 0x44, 0x24, 0x2C,
            0x50,
            0x8D, 0x44, 0x24, 0x34,
            0x50,
            0x6A, 0x01,
            0x8D, 0x44, 0x24, 0x40,
            0x50,
            0xFF, 0xB6, 0x18, 0x01, 0x00, 0x00);

    public static GameSocketReceiveRepostCallPreparationProjectionResult Project(
        GameSocketReceiveRepostCallPreparationProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceiveBufferStatusGateProjectionResult parent =
            GameSocketReceiveBufferStatusGateProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceiveRepostCallPreparationInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceiveRepostCallPreparationMemoryEffect>();
        var orderedTrace =
            ImmutableArray.CreateBuilder<GameSocketReceiveRepostCallPreparationTraceStep>();
        var consumed =
            new HashSet<GameSocketReceiveRepostCallPreparationInput>();
        foreach (GameSocketReceiveBufferStatusGateTraceStep step
                 in parent.OrderedMemoryEffects)
            orderedTrace.Add(new(orderedTrace.Count, step, null));

        bool parentHandoffAdmitted = false;
        GameSocketReceiveRepostCallPreparationRoute route =
            GameSocketReceiveRepostCallPreparationRoute.None;
        uint? effectiveEsi = null;
        uint? effectiveEbx = null;
        uint? effectiveEsp = null;
        uint? entryEsp = null;
        uint? operationAddress = null;
        uint? operationValue = null;
        uint? stackReadAddress = null;
        uint? socketReadAddress = null;
        uint? eaxAtStop = null;
        uint? eaxAfterAdd18 = null;
        uint? finalEsp = null;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;

        void AddMemoryEffect(GameSocketReceiveRepostCallPreparationMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            orderedTrace.Add(new(orderedTrace.Count, null, effect));
        }

        GameSocketReceiveRepostCallPreparationProjectionResult Finish(
            GameSocketReceiveRepostCallPreparationStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceiveRepostCallPreparationInput>();
            foreach (GameSocketReceiveRepostCallPreparationInput input
                     in Enum.GetValues<GameSocketReceiveRepostCallPreparationInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceiveRepostCallPreparationCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.DiagnosticReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsi),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEbx),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshOperationDword),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshStackDword),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveRepostCallPreparationInput.FreshSocketDword));

            return new(
                inputs,
                parent,
                parent.OrderedMemoryEffects,
                parentHandoffAdmitted,
                route,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                memoryEffects.ToImmutable(),
                orderedTrace.ToImmutable(),
                unused.ToImmutable(),
                captureStates,
                effectiveEsi,
                effectiveEbx,
                effectiveEsp,
                entryEsp,
                operationAddress,
                operationValue,
                stackReadAddress,
                socketReadAddress,
                eaxAtStop,
                eaxAfterAdd18,
                finalEsp,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        bool directNonzero =
            parent.Stop == GameSocketReceiveBufferStatusGateStop.NonzeroBranchTargetAtFBA
            && parent.ComparisonBranchTaken == true;
        bool zeroDiagnostic =
            parent.Stop == GameSocketReceiveBufferStatusGateStop.DiagnosticImportBoundaryAtFB1
            && parent.ComparisonBranchTaken == false
            && parent.DiagnosticCallSiteReached;

        if (!directNonzero && !zeroDiagnostic)
        {
            return Finish(
                GameSocketReceiveRepostCallPreparationStop.ParentDidNotReachRecognizedEndpoint,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceiveRepostCallPreparationStop.ParentEndpointWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        route = directNonzero
            ? GameSocketReceiveRepostCallPreparationRoute.DirectNonzero
            : GameSocketReceiveRepostCallPreparationRoute.ZeroDiagnostic;

        if (directNonzero)
        {
            effectiveEsi = parent.EffectiveEsi;
            effectiveEbx = parent.EbxAfterAdd;
            effectiveEsp = parent.EffectiveEsp;
            if (!effectiveEsi.HasValue || !effectiveEbx.HasValue || !effectiveEsp.HasValue)
            {
                parentHandoffAdmitted = false;
                route = GameSocketReceiveRepostCallPreparationRoute.None;
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop.ParentDidNotReachRecognizedEndpoint,
                    parent.StopAtRawFileOffset);
            }

            entryEsp = effectiveEsp;
            finalEsp = effectiveEsp;
        }
        else
        {
            if (inputs.DiagnosticReturnedNormally is not bool diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop.AwaitingDiagnosticReturnAtFB1,
                    DiagnosticImportBoundaryRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveRepostCallPreparationInput.DiagnosticReturnedNormally);
            if (!diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop.DiagnosticDidNotReturnNormally,
                    DiagnosticImportBoundaryRawFileOffset);
            }

            flagsAtStop = null;
            instructions.Add(new(
                GameSocketReceiveRepostCallPreparationInstructionKind.Add10ToPostDiagnosticEsp,
                PostDiagnosticStackCleanupRawFileOffset,
                OperandValue: PostDiagnosticStackCleanupBytes));
            if (inputs.FreshPostDiagnosticEsp is not uint freshPostDiagnosticEsp)
            {
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop
                        .AwaitingFreshPostDiagnosticEspAtFB7,
                    PostDiagnosticStackCleanupRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsp);
            uint afterCleanup = unchecked(
                freshPostDiagnosticEsp + PostDiagnosticStackCleanupBytes);
            effectiveEsp = afterCleanup;
            entryEsp = afterCleanup;
            finalEsp = afterCleanup;
            finalEsp = afterCleanup;
            instructions[^1] = instructions[^1] with
            {
                EffectiveAddress = freshPostDiagnosticEsp,
                ResultValue = afterCleanup,
                FlagsAfter = AddDwordFlags(
                    freshPostDiagnosticEsp,
                    PostDiagnosticStackCleanupBytes)
            };
            flagsAtStop = instructions[^1].FlagsAfter;

            if (inputs.FreshPostDiagnosticEsi is not uint freshPostDiagnosticEsi)
            {
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop
                        .AwaitingFreshPostDiagnosticEsiAtFBA,
                    MethodRawFileOffset);
            }

            effectiveEsi = freshPostDiagnosticEsi;
            consumed.Add(
                GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsi);
            if (inputs.FreshPostDiagnosticEbx is not uint freshPostDiagnosticEbx)
            {
                return Finish(
                    GameSocketReceiveRepostCallPreparationStop
                        .AwaitingFreshPostDiagnosticEbxAtFBA,
                    MethodRawFileOffset);
            }

            effectiveEbx = freshPostDiagnosticEbx;
            consumed.Add(
                GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEbx);
        }

        entryEsp = effectiveEsp;
        uint esp = entryEsp!.Value;
        finalEsp = esp;
        operationAddress = unchecked(effectiveEsi!.Value + OperationFieldOffset);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.ReadOperationDwordFromEsiDf4,
            OperationReadRawFileOffset,
            operationAddress,
            inputs.FreshOperationDword,
            FlagsAfter: flagsAtStop));
        AddMemoryEffect(new(
            GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshOperationDwordRead,
            OperationReadRawFileOffset,
            operationAddress,
            inputs.FreshOperationDword,
            true,
            inputs.FreshOperationDword.HasValue));
        if (inputs.FreshOperationDword is not uint freshOperation)
        {
            return Finish(
                GameSocketReceiveRepostCallPreparationStop.AwaitingFreshOperationDwordAtFBA,
                OperationReadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveRepostCallPreparationInput.FreshOperationDword);
        operationValue = freshOperation;
        eaxAtStop = freshOperation;

        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushZero,
            PushZeroRawFileOffset,
            OperandValue: 0,
            FlagsAfter: flagsAtStop));
        esp = Push(PushZeroRawFileOffset, 0, esp);

        stackReadAddress = unchecked(esp + StackReadDisplacement);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.ReadAndPushStackDwordFromEsp1C,
            StackDwordReadPushRawFileOffset,
            stackReadAddress,
            inputs.FreshStackDword,
            FlagsAfter: flagsAtStop));
        AddMemoryEffect(new(
            GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshStackDwordRead,
            StackDwordReadPushRawFileOffset,
            stackReadAddress,
            inputs.FreshStackDword,
            true,
            inputs.FreshStackDword.HasValue));
        if (inputs.FreshStackDword is not uint freshStack)
        {
            return Finish(
                GameSocketReceiveRepostCallPreparationStop.AwaitingFreshStackDwordAtFC2,
                StackDwordReadPushRawFileOffset);
        }

        consumed.Add(GameSocketReceiveRepostCallPreparationInput.FreshStackDword);
        esp = Push(StackDwordReadPushRawFileOffset, freshStack, esp);

        uint eaxAfterAdd = unchecked(freshOperation + EaxAddend);
        eaxAfterAdd18 = eaxAfterAdd;
        flagsAtStop = AddDwordFlags(freshOperation, EaxAddend);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.Add18ToEax,
            Add18ToEaxRawFileOffset,
            OperandValue: EaxAddend,
            ResultValue: eaxAfterAdd,
            FlagsAfter: flagsAtStop));
        eaxAtStop = eaxAfterAdd;

        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushEbx,
            PushEbxRawFileOffset,
            OperandValue: effectiveEbx,
            FlagsAfter: flagsAtStop));
        esp = Push(PushEbxRawFileOffset, effectiveEbx!.Value, esp);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushEax,
            PushEaxRawFileOffset,
            OperandValue: eaxAfterAdd,
            FlagsAfter: flagsAtStop));
        esp = Push(PushEaxRawFileOffset, eaxAfterAdd, esp);

        eaxAtStop = unchecked(esp + 0x2C);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.LeaEaxFromEsp2C,
            LeaEaxFromEsp2CRawFileOffset,
            esp,
            0x2C,
            eaxAtStop,
            FlagsAfter: flagsAtStop));
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushLeaEaxFromEsp2C,
            PushLeaEaxFromEsp2CRawFileOffset,
            OperandValue: eaxAtStop,
            FlagsAfter: flagsAtStop));
        esp = Push(PushLeaEaxFromEsp2CRawFileOffset, eaxAtStop.Value, esp);

        eaxAtStop = unchecked(esp + 0x34);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.LeaEaxFromEsp34,
            LeaEaxFromEsp34RawFileOffset,
            esp,
            0x34,
            eaxAtStop,
            FlagsAfter: flagsAtStop));
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushLeaEaxFromEsp34,
            PushLeaEaxFromEsp34RawFileOffset,
            OperandValue: eaxAtStop,
            FlagsAfter: flagsAtStop));
        esp = Push(PushLeaEaxFromEsp34RawFileOffset, eaxAtStop.Value, esp);

        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushOne,
            PushOneRawFileOffset,
            OperandValue: 1,
            FlagsAfter: flagsAtStop));
        esp = Push(PushOneRawFileOffset, 1, esp);

        eaxAtStop = unchecked(esp + 0x40);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.LeaEaxFromEsp40,
            LeaEaxFromEsp40RawFileOffset,
            esp,
            0x40,
            eaxAtStop,
            FlagsAfter: flagsAtStop));
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.PushLeaEaxFromEsp40,
            PushLeaEaxFromEsp40RawFileOffset,
            OperandValue: eaxAtStop,
            FlagsAfter: flagsAtStop));
        esp = Push(PushLeaEaxFromEsp40RawFileOffset, eaxAtStop.Value, esp);

        socketReadAddress = unchecked(effectiveEsi.Value + SocketFieldOffset);
        instructions.Add(new(
            GameSocketReceiveRepostCallPreparationInstructionKind.ReadAndPushSocketDwordFromEsi118,
            SocketDwordReadPushRawFileOffset,
            socketReadAddress,
            inputs.FreshSocketDword,
            FlagsAfter: flagsAtStop));
        AddMemoryEffect(new(
            GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshSocketDwordRead,
            SocketDwordReadPushRawFileOffset,
            socketReadAddress,
            inputs.FreshSocketDword,
            true,
            inputs.FreshSocketDword.HasValue));
        if (inputs.FreshSocketDword is not uint freshSocket)
        {
            finalEsp = esp;
            return Finish(
                GameSocketReceiveRepostCallPreparationStop.AwaitingFreshSocketDwordAtFDC,
                SocketDwordReadPushRawFileOffset);
        }

        consumed.Add(GameSocketReceiveRepostCallPreparationInput.FreshSocketDword);
        esp = Push(SocketDwordReadPushRawFileOffset, freshSocket, esp);
        finalEsp = esp;
        return Finish(
            GameSocketReceiveRepostCallPreparationStop.ExclusiveStopAtFE2,
            ExclusiveStopRawFileOffset,
            true);

        uint Push(uint rawFileOffset, uint value, uint currentEsp)
        {
            uint resultEsp = unchecked(currentEsp - sizeof(uint));
            finalEsp = resultEsp;
            AddMemoryEffect(new(
                GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite,
                rawFileOffset,
                resultEsp,
                value,
                true,
                true));
            instructions[^1] = rawFileOffset is StackDwordReadPushRawFileOffset
                or SocketDwordReadPushRawFileOffset
                ? instructions[^1] with
                {
                    StackAddress = resultEsp,
                    OperandValue = value,
                    ResultValue = resultEsp,
                    FlagsAfter = flagsAtStop
                }
                : instructions[^1] with
                {
                    EffectiveAddress = resultEsp,
                    OperandValue = value,
                    ResultValue = resultEsp,
                    FlagsAfter = flagsAtStop
                };
            return resultEsp;
        }
    }

    private static GameSocketReceiveRepostCallPreparationCaptureState GetCaptureState(
        GameSocketReceiveRepostCallPreparationProjectionInputs inputs,
        HashSet<GameSocketReceiveRepostCallPreparationInput> consumed,
        GameSocketReceiveRepostCallPreparationStop stop,
        GameSocketReceiveRepostCallPreparationInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceiveRepostCallPreparationCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceiveRepostCallPreparationCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceiveRepostCallPreparationInput.DiagnosticReturnedNormally =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingDiagnosticReturnAtFB1,
            GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsp =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEspAtFB7,
            GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsi =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEsiAtFBA,
            GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEbx =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEbxAtFBA,
            GameSocketReceiveRepostCallPreparationInput.FreshOperationDword =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshOperationDwordAtFBA,
            GameSocketReceiveRepostCallPreparationInput.FreshStackDword =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshStackDwordAtFC2,
            GameSocketReceiveRepostCallPreparationInput.FreshSocketDword =>
                stop == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshSocketDwordAtFDC,
            _ => false
        };
        return required
            ? GameSocketReceiveRepostCallPreparationCaptureState.Missing
            : GameSocketReceiveRepostCallPreparationCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceiveRepostCallPreparationProjectionInputs inputs,
        GameSocketReceiveRepostCallPreparationInput input) => input switch
    {
        GameSocketReceiveRepostCallPreparationInput.DiagnosticReturnedNormally =>
            inputs.DiagnosticReturnedNormally.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsp =>
            inputs.FreshPostDiagnosticEsp.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEsi =>
            inputs.FreshPostDiagnosticEsi.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshPostDiagnosticEbx =>
            inputs.FreshPostDiagnosticEbx.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshOperationDword =>
            inputs.FreshOperationDword.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshStackDword =>
            inputs.FreshStackDword.HasValue,
        GameSocketReceiveRepostCallPreparationInput.FreshSocketDword =>
            inputs.FreshSocketDword.HasValue,
        _ => false
    };

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

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}

public sealed record GameSocketReceiveRepostCallPreparationProjectionResult(
    GameSocketReceiveRepostCallPreparationProjectionInputs Inputs,
    GameSocketReceiveBufferStatusGateProjectionResult ParentProjection,
    ImmutableArray<GameSocketReceiveBufferStatusGateTraceStep> ParentTrace,
    bool ParentHandoffAdmitted,
    GameSocketReceiveRepostCallPreparationRoute Route,
    GameSocketReceiveRepostCallPreparationStop Stop,
    uint StopAtRawFileOffsetExclusive,
    ImmutableArray<GameSocketReceiveRepostCallPreparationInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceiveRepostCallPreparationMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceiveRepostCallPreparationTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceiveRepostCallPreparationInput> SuppliedButUnusedInputs,
    GameSocketReceiveRepostCallPreparationCaptureStates CaptureStates,
    uint? EffectiveEsi,
    uint? EffectiveEbx,
    uint? EffectiveEsp,
    uint? EntryEsp,
    uint? OperationReadAddress,
    uint? OperationDword,
    uint? StackReadAddress,
    uint? SocketReadAddress,
    uint? EaxAtStop,
    uint? EaxAfterAdd18,
    uint? FinalEsp,
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
        || MemoryEffects.Any(effect => effect.Committed);

    public uint? FollowingCellVirtualAddress =>
        Stop == GameSocketReceiveRepostCallPreparationStop.ExclusiveStopAtFE2
            ? GameSocketReceiveRepostCallPreparationProjection
                .FollowingCellVirtualAddress
            : null;

    public bool FollowingImportCellReadAttempted => false;
    public bool FollowingImportCallAttempted => false;
    public bool NativeInstructionsExecuted => false;
    public bool ApiResultModeled => false;
}
