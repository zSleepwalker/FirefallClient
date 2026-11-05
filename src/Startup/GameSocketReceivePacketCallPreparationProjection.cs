using System.Collections.Generic;
using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceivePacketCallPreparationProjectionInputs(
    GameSocketReceiveSenderPreparationProjectionInputs ParentInputs,
    bool? SenderHelperReturnedNormally,
    uint? SenderHelperReturnedEax,
    uint? FreshPostHelperEsp,
    uint? FreshCount);

public enum GameSocketReceivePacketCallPreparationInput
{
    SenderHelperReturnedNormally,
    SenderHelperReturnedEax,
    FreshPostHelperEsp,
    FreshCount
}

public enum GameSocketReceivePacketCallPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceivePacketCallPreparationCaptureStates(
    GameSocketReceivePacketCallPreparationCaptureState SenderHelperReturnedNormally,
    GameSocketReceivePacketCallPreparationCaptureState SenderHelperReturnedEax,
    GameSocketReceivePacketCallPreparationCaptureState FreshPostHelperEsp,
    GameSocketReceivePacketCallPreparationCaptureState FreshCount);

public enum GameSocketReceivePacketCallPreparationStop
{
    ParentDidNotReachSenderHelper,
    ParentSenderHelperBoundaryWasDirty,
    AwaitingSenderHelperReturnAtF19,
    SenderHelperDidNotReturnNormally,
    AwaitingSenderHelperEaxAtF1E,
    AwaitingPostHelperEspAtF1E,
    AwaitingFreshCountAtF1F,
    AwaitingInheritedReceiverAtF23,
    FixedCallBoundaryAtF25
}

public enum GameSocketReceivePacketCallPreparationInstructionKind
{
    PushSenderHelperResult,
    PushFreshCount,
    MoveInheritedReceiverToEcx,
    CallFixedTarget
}

public readonly record struct GameSocketReceivePacketCallPreparationInstruction(
    GameSocketReceivePacketCallPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    uint? TargetAddress = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null);

public enum GameSocketReceivePacketCallPreparationMemoryEffectKind
{
    SenderHelperResultStackWrite,
    FreshCountRead,
    FreshCountStackWrite
}

public readonly record struct GameSocketReceivePacketCallPreparationMemoryEffect(
    GameSocketReceivePacketCallPreparationMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        (Kind is GameSocketReceivePacketCallPreparationMemoryEffectKind.SenderHelperResultStackWrite
            or GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountStackWrite)
        && Address.HasValue
        && Value.HasValue;
}

public enum GameSocketReceivePacketCallPreparationCallKind
{
    DiagnosticImport,
    SenderHelper,
    FixedTargetBoundary
}

public readonly record struct GameSocketReceivePacketCallPreparationCall(
    GameSocketReceivePacketCallPreparationCallKind Kind,
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

public readonly record struct GameSocketReceivePacketCallPreparationTraceStep(
    int Sequence,
    GameSocketReceiveResultGateMemoryEffect? ResultGateMemoryEffect,
    GameSocketReceiveSenderPreparationMemoryEffect? SenderPreparationMemoryEffect,
    GameSocketReceivePacketCallPreparationMemoryEffect? PacketCallPreparationMemoryEffect);

public static class GameSocketReceivePacketCallPreparationProjection
{
    public const uint MethodRawFileOffset = 0x00BD2F19;
    public const uint SenderHelperCallRawFileOffset = 0x00BD2F19;
    public const uint SenderHelperTargetRawFileOffset = 0x00BE3A00;
    public const uint SenderHelperTargetVirtualAddress = 0x00FE4600;
    public const uint SenderResultPushRawFileOffset = 0x00BD2F1E;
    public const uint FreshCountPushRawFileOffset = 0x00BD2F1F;
    public const uint FreshCountOperandDisplacement = 0x14;
    public const uint InheritedReceiverMoveRawFileOffset = 0x00BD2F23;
    public const uint FixedCallRawFileOffset = 0x00BD2F25;
    public const uint FixedCallTargetRawFileOffset = 0x00BD3030;
    public const uint FixedCallTargetVirtualAddress = 0x00FD3C30;
    public static ImmutableArray<byte> SenderHelperCallBytes { get; } =
        ImmutableArray.Create<byte>(0xE8, 0xE2, 0x0A, 0x01, 0x00);
    public static ImmutableArray<byte> SenderResultPushBytes { get; } =
        ImmutableArray.Create<byte>(0x50);
    public static ImmutableArray<byte> FreshCountPushBytes { get; } =
        ImmutableArray.Create<byte>(0xFF, 0x74, 0x24, 0x14);
    public static ImmutableArray<byte> InheritedReceiverMoveBytes { get; } =
        ImmutableArray.Create<byte>(0x8B, 0xCE);
    public static ImmutableArray<byte> FixedCallBytes { get; } =
        ImmutableArray.Create<byte>(0xE8, 0x06, 0x01, 0x00, 0x00);

    public static GameSocketReceivePacketCallPreparationProjectionResult Project(
        GameSocketReceivePacketCallPreparationProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceiveSenderPreparationProjectionResult parent =
            GameSocketReceiveSenderPreparationProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceivePacketCallPreparationInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceivePacketCallPreparationMemoryEffect>();
        var calls = ImmutableArray.CreateBuilder<GameSocketReceivePacketCallPreparationCall>();
        var trace =
            ImmutableArray.CreateBuilder<GameSocketReceivePacketCallPreparationTraceStep>();
        var consumed = new HashSet<GameSocketReceivePacketCallPreparationInput>();

        foreach (GameSocketReceiveSenderPreparationTraceStep step in parent.OrderedMemoryEffects)
        {
            trace.Add(new(
                trace.Count,
                step.ParentMemoryEffect,
                step.PreparationMemoryEffect,
                null));
        }

        foreach (GameSocketReceiveSenderPreparationOpaqueCall parentCall in parent.OpaqueCalls)
        {
            bool isSenderHelper =
                parentCall.Kind == GameSocketReceiveSenderPreparationOpaqueCallKind.SenderHelper;
            calls.Add(new(
                isSenderHelper
                    ? GameSocketReceivePacketCallPreparationCallKind.SenderHelper
                    : GameSocketReceivePacketCallPreparationCallKind.DiagnosticImport,
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
                null,
                null,
                ImmutableArray<uint?>.Empty));
        }

        uint? firstPushEsp = null;
        uint? freshCountAddress = null;
        uint? secondPushEsp = null;
        uint? stackPointerAtFixedCall = null;
        uint? inheritedReceiver = null;
        bool senderHelperCallSiteReached = false;
        bool fixedCallSiteReached = false;
        bool parentHandoffAdmitted = false;
        bool freshCountReadAttempted = false;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;

        void AddMemoryEffect(GameSocketReceivePacketCallPreparationMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            trace.Add(new(trace.Count, null, null, effect));
        }

        GameSocketReceivePacketCallPreparationProjectionResult Finish(
            GameSocketReceivePacketCallPreparationStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceivePacketCallPreparationInput>();
            foreach (GameSocketReceivePacketCallPreparationInput input
                     in Enum.GetValues<GameSocketReceivePacketCallPreparationInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceivePacketCallPreparationCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedEax),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePacketCallPreparationInput.FreshPostHelperEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceivePacketCallPreparationInput.FreshCount));

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
                firstPushEsp,
                freshCountAddress,
                secondPushEsp,
                stackPointerAtFixedCall,
                inheritedReceiver,
                senderHelperCallSiteReached,
                freshCountReadAttempted,
                fixedCallSiteReached,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        if (parent.Stop
                != GameSocketReceiveSenderPreparationStop.SenderHelperCallBoundaryAtF19
            || !parent.ReachedBoundedEndpoint)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.ParentDidNotReachSenderHelper,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.ParentSenderHelperBoundaryWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        senderHelperCallSiteReached = true;
        for (int i = calls.Count - 1; i >= 0; i--)
        {
            if (calls[i].Kind != GameSocketReceivePacketCallPreparationCallKind.SenderHelper)
                continue;

            calls[i] = calls[i] with
            {
                Attempted = true,
                ReturnedNormally = inputs.SenderHelperReturnedNormally,
                ReturnedEax = inputs.SenderHelperReturnedNormally == true
                    ? inputs.SenderHelperReturnedEax : null,
                PostCallEsp = inputs.SenderHelperReturnedNormally == true
                    ? inputs.FreshPostHelperEsp : null
            };
            break;
        }

        if (inputs.SenderHelperReturnedNormally is not bool returnedNormally)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperReturnAtF19,
                SenderHelperCallRawFileOffset);
        }

        consumed.Add(GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedNormally);
        if (!returnedNormally)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.SenderHelperDidNotReturnNormally,
                SenderHelperCallRawFileOffset);
        }

        flagsAtStop = null;
        instructions.Add(new(
            GameSocketReceivePacketCallPreparationInstructionKind.PushSenderHelperResult,
            SenderResultPushRawFileOffset,
            EffectiveAddress: inputs.FreshPostHelperEsp is uint postHelperEsp
                ? unchecked(postHelperEsp - sizeof(uint))
                : null,
            OperandValue: inputs.SenderHelperReturnedEax,
            ResultValue: inputs.FreshPostHelperEsp is uint espForFirstPush
                ? unchecked(espForFirstPush - sizeof(uint))
                : null));
        if (inputs.SenderHelperReturnedEax.HasValue)
            consumed.Add(GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedEax);
        if (inputs.FreshPostHelperEsp is uint postEsp)
        {
            consumed.Add(GameSocketReceivePacketCallPreparationInput.FreshPostHelperEsp);
            firstPushEsp = unchecked(postEsp - sizeof(uint));
        }

        AddMemoryEffect(new(
            GameSocketReceivePacketCallPreparationMemoryEffectKind.SenderHelperResultStackWrite,
            SenderResultPushRawFileOffset,
            firstPushEsp,
            inputs.SenderHelperReturnedEax,
            true,
            firstPushEsp.HasValue && inputs.SenderHelperReturnedEax.HasValue));

        if (inputs.SenderHelperReturnedEax is null)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperEaxAtF1E,
                SenderResultPushRawFileOffset);
        }

        if (firstPushEsp is null)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.AwaitingPostHelperEspAtF1E,
                SenderResultPushRawFileOffset);
        }

        uint senderResult = inputs.SenderHelperReturnedEax.Value;
        freshCountAddress = unchecked(firstPushEsp.Value + FreshCountOperandDisplacement);
        freshCountReadAttempted = true;
        instructions.Add(new(
            GameSocketReceivePacketCallPreparationInstructionKind.PushFreshCount,
            FreshCountPushRawFileOffset,
            EffectiveAddress: freshCountAddress,
            OperandValue: inputs.FreshCount,
            ResultValue: unchecked(firstPushEsp.Value - sizeof(uint))));
        AddMemoryEffect(new(
            GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountRead,
            FreshCountPushRawFileOffset,
            freshCountAddress,
            inputs.FreshCount,
            true,
            inputs.FreshCount.HasValue));
        if (inputs.FreshCount is not uint freshCount)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.AwaitingFreshCountAtF1F,
                FreshCountPushRawFileOffset);
        }

        consumed.Add(GameSocketReceivePacketCallPreparationInput.FreshCount);
        secondPushEsp = unchecked(firstPushEsp.Value - sizeof(uint));
        AddMemoryEffect(new(
            GameSocketReceivePacketCallPreparationMemoryEffectKind.FreshCountStackWrite,
            FreshCountPushRawFileOffset,
            secondPushEsp,
            freshCount,
            true,
            true));

        instructions.Add(new(
            GameSocketReceivePacketCallPreparationInstructionKind.MoveInheritedReceiverToEcx,
            InheritedReceiverMoveRawFileOffset,
            OperandValue: parent.Inputs.ReceiverEsi));
        if (parent.Inputs.ReceiverEsi is not uint receiver)
        {
            return Finish(
                GameSocketReceivePacketCallPreparationStop.AwaitingInheritedReceiverAtF23,
                InheritedReceiverMoveRawFileOffset);
        }

        inheritedReceiver = receiver;
        stackPointerAtFixedCall = secondPushEsp;
        instructions.Add(new(
            GameSocketReceivePacketCallPreparationInstructionKind.CallFixedTarget,
            FixedCallRawFileOffset,
            FixedCallTargetRawFileOffset,
            FixedCallTargetVirtualAddress,
            EffectiveAddress: stackPointerAtFixedCall,
            OperandValue: receiver));
        fixedCallSiteReached = true;
        calls.Add(new(
            GameSocketReceivePacketCallPreparationCallKind.FixedTargetBoundary,
            FixedCallRawFileOffset,
            FixedCallTargetRawFileOffset,
            FixedCallTargetVirtualAddress,
            null,
            true,
            true,
            false,
            null,
            null,
            receiver,
            null,
            null,
            ImmutableArray.Create<uint?>(freshCount, senderResult)));

        return Finish(
            GameSocketReceivePacketCallPreparationStop.FixedCallBoundaryAtF25,
            FixedCallRawFileOffset,
            true);
    }

    private static GameSocketReceivePacketCallPreparationCaptureState GetCaptureState(
        GameSocketReceivePacketCallPreparationProjectionInputs inputs,
        HashSet<GameSocketReceivePacketCallPreparationInput> consumed,
        GameSocketReceivePacketCallPreparationStop stop,
        GameSocketReceivePacketCallPreparationInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceivePacketCallPreparationCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceivePacketCallPreparationCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedNormally =>
                stop == GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperReturnAtF19,
            GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedEax =>
                stop == GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperEaxAtF1E,
            GameSocketReceivePacketCallPreparationInput.FreshPostHelperEsp =>
                stop is GameSocketReceivePacketCallPreparationStop.AwaitingPostHelperEspAtF1E
                    or GameSocketReceivePacketCallPreparationStop.AwaitingSenderHelperEaxAtF1E,
            GameSocketReceivePacketCallPreparationInput.FreshCount =>
                stop == GameSocketReceivePacketCallPreparationStop.AwaitingFreshCountAtF1F,
            _ => false
        };
        return required
            ? GameSocketReceivePacketCallPreparationCaptureState.Missing
            : GameSocketReceivePacketCallPreparationCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceivePacketCallPreparationProjectionInputs inputs,
        GameSocketReceivePacketCallPreparationInput input) => input switch
    {
        GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedNormally =>
            inputs.SenderHelperReturnedNormally.HasValue,
        GameSocketReceivePacketCallPreparationInput.SenderHelperReturnedEax =>
            inputs.SenderHelperReturnedEax.HasValue,
        GameSocketReceivePacketCallPreparationInput.FreshPostHelperEsp =>
            inputs.FreshPostHelperEsp.HasValue,
        GameSocketReceivePacketCallPreparationInput.FreshCount => inputs.FreshCount.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceivePacketCallPreparationProjectionResult(
    GameSocketReceivePacketCallPreparationProjectionInputs Inputs,
    GameSocketReceiveSenderPreparationProjectionResult ParentProjection,
    bool ParentHandoffAdmitted,
    GameSocketReceivePacketCallPreparationStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceivePacketCallPreparationInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceivePacketCallPreparationMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceivePacketCallPreparationTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceivePacketCallPreparationCall> Calls,
    ImmutableArray<GameSocketReceivePacketCallPreparationInput> SuppliedButUnusedInputs,
    GameSocketReceivePacketCallPreparationCaptureStates CaptureStates,
    uint? SenderResultStackAddress,
    uint? FreshCountReadAddress,
    uint? FreshCountStackAddress,
    uint? StackPointerAtFixedCall,
    uint? InheritedReceiver,
    bool SenderHelperCallSiteReached,
    bool FreshCountReadAttempted,
    bool FixedCallSiteReached,
    GameSocketReceiveResultGateFlags? FlagsAtStop,
    bool ReachedBoundedEndpoint)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool FixedCallBoundaryReached => FixedCallSiteReached;

    public bool ExactBoundedCompletion =>
        ReachedBoundedEndpoint
        || (!ParentHandoffAdmitted && ParentProjection.ExactBoundedCompletion);

    public bool IsCleanCompletion =>
        ParentHandoffAdmitted
        && FixedCallBoundaryReached
        && ParentProjection.IsCleanCompletion
        && !HasSuppliedButUnusedInputs;

    public bool AnyMemoryWriteCommitted =>
        ParentProjection.AnyMemoryWriteCommitted
        || OrderedMemoryEffects.Any(step =>
            step.SenderPreparationMemoryEffect is { Committed: true }
            || step.PacketCallPreparationMemoryEffect is { Committed: true });

    public ImmutableArray<uint?> FixedCallStackArguments =>
        Calls.LastOrDefault(call =>
            call.Kind == GameSocketReceivePacketCallPreparationCallKind.FixedTargetBoundary)
            .StackArguments;

    public bool PacketReceiveCallExecuted => false;

    public bool PacketRepostModeled => false;

    public bool ReturnInstructionModeled => false;

    public bool OpaqueCallsExecuted => false;
}
