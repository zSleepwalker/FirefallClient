using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record GameSocketReceiveSenderPreparationProjectionInputs(
    GameSocketReceiveResultGateProjectionInputs ParentInputs,
    uint? ReceiverEsi,
    uint? InitialOperation,
    uint? FreshReloadedOperation,
    bool? DiagnosticReturnedNormally,
    uint? FreshPostDiagnosticEsp,
    uint? EntryEsp);

public enum GameSocketReceiveSenderPreparationInput
{
    ReceiverEsi,
    InitialOperation,
    FreshReloadedOperation,
    DiagnosticReturnedNormally,
    FreshPostDiagnosticEsp,
    EntryEsp
}

public enum GameSocketReceiveSenderPreparationCaptureState
{
    Unreached,
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct GameSocketReceiveSenderPreparationCaptureStates(
    GameSocketReceiveSenderPreparationCaptureState ReceiverEsi,
    GameSocketReceiveSenderPreparationCaptureState InitialOperation,
    GameSocketReceiveSenderPreparationCaptureState FreshReloadedOperation,
    GameSocketReceiveSenderPreparationCaptureState DiagnosticReturnedNormally,
    GameSocketReceiveSenderPreparationCaptureState FreshPostDiagnosticEsp,
    GameSocketReceiveSenderPreparationCaptureState EntryEsp);

public enum GameSocketReceiveSenderPreparationStop
{
    ParentDidNotReachNonzeroEndpoint,
    ParentNonzeroEndpointWasDirty,
    AwaitingReceiverEsiAtEEB,
    AwaitingInitialOperationAtEEB,
    AwaitingEntryEspAtDiagnosticPush,
    AwaitingDiagnosticReturnAtF02,
    DiagnosticDidNotReturnNormally,
    AwaitingPostDiagnosticEspAtF08,
    AwaitingFreshReloadAtF0B,
    AwaitingEntryEspAtF11,
    SenderHelperCallBoundaryAtF19
}

public enum GameSocketReceiveSenderPreparationInstructionKind
{
    CompareInitialOperationToZero,
    BranchIfInitialOperationNonzero,
    PushDiagnosticArgument,
    CallDiagnosticImport,
    AddEsp10,
    ReloadFreshOperation,
    LeaLocalBuffer,
    PushLocalBufferPointer,
    AddEcx18,
    CallSenderHelper
}

public readonly record struct GameSocketReceiveSenderPreparationInstruction(
    GameSocketReceiveSenderPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? TargetRawFileOffset = null,
    uint? TargetAddress = null,
    bool? BranchTaken = null,
    uint? EffectiveAddress = null,
    uint? OperandValue = null,
    uint? ResultValue = null);

public enum GameSocketReceiveSenderPreparationMemoryEffectKind
{
    InitialOperationRead,
    FreshReloadedOperationRead,
    DiagnosticStackWrite,
    SenderArgumentStackWrite
}

public readonly record struct GameSocketReceiveSenderPreparationMemoryEffect(
    GameSocketReceiveSenderPreparationMemoryEffectKind Kind,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured)
{
    public bool Committed =>
        (Kind is GameSocketReceiveSenderPreparationMemoryEffectKind.DiagnosticStackWrite
            or GameSocketReceiveSenderPreparationMemoryEffectKind.SenderArgumentStackWrite)
        && Address.HasValue
        && Value.HasValue;
}

public enum GameSocketReceiveSenderPreparationOpaqueCallKind
{
    DiagnosticImport,
    SenderHelper
}

public readonly record struct GameSocketReceiveSenderPreparationOpaqueCall(
    GameSocketReceiveSenderPreparationOpaqueCallKind Kind,
    uint CallSiteRawFileOffset,
    uint? TargetRawFileOffset,
    uint? TargetAddress,
    uint? ImportCellVirtualAddress,
    bool TargetCaptured,
    bool CallSiteReached,
    bool Attempted,
    bool? ReturnedNormally,
    uint? EaxArgument,
    uint? EcxArgument);

public readonly record struct GameSocketReceiveSenderPreparationTraceStep(
    int Sequence,
    GameSocketReceiveResultGateMemoryEffect? ParentMemoryEffect,
    GameSocketReceiveSenderPreparationMemoryEffect? PreparationMemoryEffect);

public static class GameSocketReceiveSenderPreparationProjection
{
    public const uint MethodRawFileOffset = 0x00BD2EEB;
    public const uint InitialOperationCompareRawFileOffset = 0x00BD2EEB;
    public const uint InitialOperationBranchRawFileOffset = 0x00BD2EF2;
    public const uint DiagnosticPushZeroRawFileOffset = 0x00BD2EF4;
    public const uint DiagnosticPushTokenRawFileOffset = 0x00BD2EF6;
    public const uint DiagnosticPushAddressRawFileOffset = 0x00BD2EF8;
    public const uint DiagnosticPushAddressAgainRawFileOffset = 0x00BD2EFD;
    public const uint DiagnosticCallRawFileOffset = 0x00BD2F02;
    public const uint AddEspRawFileOffset = 0x00BD2F08;
    public const uint FreshReloadRawFileOffset = 0x00BD2F0B;
    public const uint LocalBufferLeaRawFileOffset = 0x00BD2F11;
    public const uint LocalBufferPushRawFileOffset = 0x00BD2F15;
    public const uint AddEcxRawFileOffset = 0x00BD2F16;
    public const uint SenderHelperCallRawFileOffset = 0x00BD2F19;
    public const uint BoundedSenderHelperTargetRawFileOffset = 0x00BE3A00;
    public const uint BoundedSenderHelperTargetVirtualAddress = 0x00FE4600;
    public const uint DiagnosticImportCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticArgumentVirtualAddress = 0x01C5EA58;
    public const uint OperationFieldOffset = 0x0DF4;
    public const uint LocalBufferOffset = 0x34;
    public const uint DiagnosticStackCleanupSize = 0x10;
    public const uint SenderReceiverFieldOffset = 0x18;

    public static GameSocketReceiveSenderPreparationProjectionResult Project(
        GameSocketReceiveSenderPreparationProjectionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        GameSocketReceiveResultGateProjectionResult parent =
            GameSocketReceiveResultGateProjection.Project(inputs.ParentInputs);
        var instructions =
            ImmutableArray.CreateBuilder<GameSocketReceiveSenderPreparationInstruction>();
        var memoryEffects =
            ImmutableArray.CreateBuilder<GameSocketReceiveSenderPreparationMemoryEffect>();
        var opaqueCalls =
            ImmutableArray.CreateBuilder<GameSocketReceiveSenderPreparationOpaqueCall>();
        var trace =
            ImmutableArray.CreateBuilder<GameSocketReceiveSenderPreparationTraceStep>();
        var consumed = new HashSet<GameSocketReceiveSenderPreparationInput>();
        foreach (GameSocketReceiveResultGateMemoryEffect effect in parent.MemoryEffects)
        {
            trace.Add(new(trace.Count, effect, null));
        }

        uint? initialOperationAddress = null;
        uint? freshReloadAddress = null;
        uint? localBufferPointer = null;
        uint? senderArgumentStackAddress = null;
        uint? stackPointerAtSenderCall = null;
        GameSocketReceiveResultGateFlags? flagsAfterInitialCompare = null;
        GameSocketReceiveResultGateFlags? flagsAfterDiagnosticStackCleanup = null;
        GameSocketReceiveResultGateFlags? flagsAfterSenderOffsetAdd = null;
        GameSocketReceiveResultGateFlags? flagsAtStop = parent.FlagsAtStop;
        bool initialOperationReadAttempted = false;
        bool freshReloadReadAttempted = false;
        bool diagnosticCallSiteReached = false;
        bool senderHelperCallSiteReached = false;
        bool parentHandoffAdmitted = false;
        bool? diagnosticBranchTaken = null;

        void AddMemoryEffect(GameSocketReceiveSenderPreparationMemoryEffect effect)
        {
            memoryEffects.Add(effect);
            trace.Add(new(trace.Count, null, effect));
        }

        GameSocketReceiveSenderPreparationProjectionResult Finish(
            GameSocketReceiveSenderPreparationStop stop,
            uint stopAtRaw,
            bool reachedBoundedEndpoint = false)
        {
            var unused =
                ImmutableArray.CreateBuilder<GameSocketReceiveSenderPreparationInput>();
            foreach (GameSocketReceiveSenderPreparationInput input
                     in Enum.GetValues<GameSocketReceiveSenderPreparationInput>())
            {
                if (!consumed.Contains(input) && IsSupplied(inputs, input))
                    unused.Add(input);
            }

            var captureStates = new GameSocketReceiveSenderPreparationCaptureStates(
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.ReceiverEsi),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.InitialOperation),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.FreshReloadedOperation),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.DiagnosticReturnedNormally),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.FreshPostDiagnosticEsp),
                GetCaptureState(inputs, consumed, stop,
                    GameSocketReceiveSenderPreparationInput.EntryEsp));

            return new(
                inputs,
                parent,
                parentHandoffAdmitted,
                stop,
                stopAtRaw,
                instructions.ToImmutable(),
                memoryEffects.ToImmutable(),
                trace.ToImmutable(),
                opaqueCalls.ToImmutable(),
                unused.ToImmutable(),
                captureStates,
                initialOperationAddress,
                freshReloadAddress,
                localBufferPointer,
                senderArgumentStackAddress,
                stackPointerAtSenderCall,
                initialOperationReadAttempted,
                freshReloadReadAttempted,
                diagnosticCallSiteReached,
                senderHelperCallSiteReached,
                diagnosticBranchTaken,
                flagsAfterInitialCompare,
                flagsAfterDiagnosticStackCleanup,
                flagsAfterSenderOffsetAdd,
                flagsAtStop,
                reachedBoundedEndpoint);
        }

        if (parent.Stop != GameSocketReceiveResultGateStop.NonzeroResultAtBounded2EEB
            || !parent.ReachedBoundedEndpoint)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.ParentDidNotReachNonzeroEndpoint,
                parent.StopAtRawFileOffset);
        }

        if (!parent.IsCleanCompletion)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.ParentNonzeroEndpointWasDirty,
                parent.StopAtRawFileOffset);
        }

        parentHandoffAdmitted = true;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.CompareInitialOperationToZero,
            InitialOperationCompareRawFileOffset));
        if (inputs.ReceiverEsi is not uint receiver)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.AwaitingReceiverEsiAtEEB,
                InitialOperationCompareRawFileOffset);
        }

        consumed.Add(GameSocketReceiveSenderPreparationInput.ReceiverEsi);
        initialOperationAddress = unchecked(receiver + OperationFieldOffset);
        initialOperationReadAttempted = true;
        AddMemoryEffect(new(
            GameSocketReceiveSenderPreparationMemoryEffectKind.InitialOperationRead,
            InitialOperationCompareRawFileOffset,
            initialOperationAddress,
            inputs.InitialOperation,
            true,
            inputs.InitialOperation.HasValue));
        instructions[^1] = instructions[^1] with
        {
            EffectiveAddress = initialOperationAddress,
            OperandValue = inputs.InitialOperation
        };
        if (inputs.InitialOperation is not uint initialOperation)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.AwaitingInitialOperationAtEEB,
                InitialOperationCompareRawFileOffset);
        }

        consumed.Add(GameSocketReceiveSenderPreparationInput.InitialOperation);
        flagsAfterInitialCompare =
            GameSocketReceiveResultGateProjection.GetCompareFlags(initialOperation, 0);
        flagsAtStop = flagsAfterInitialCompare;
        diagnosticBranchTaken = initialOperation == 0;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.BranchIfInitialOperationNonzero,
            InitialOperationBranchRawFileOffset,
            FreshReloadRawFileOffset,
            BranchTaken: !diagnosticBranchTaken));

        uint? diagnosticStackPointer = null;
        if (diagnosticBranchTaken == true)
        {
            if (inputs.EntryEsp is not uint entryEsp)
            {
                return Finish(
                    GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtDiagnosticPush,
                    DiagnosticPushZeroRawFileOffset);
            }

            consumed.Add(GameSocketReceiveSenderPreparationInput.EntryEsp);
            diagnosticStackPointer = entryEsp;

            PushDiagnosticArgument(DiagnosticPushZeroRawFileOffset, 0);
            PushDiagnosticArgument(DiagnosticPushTokenRawFileOffset, 0x5B);
            PushDiagnosticArgument(
                DiagnosticPushAddressRawFileOffset, DiagnosticArgumentVirtualAddress);
            PushDiagnosticArgument(
                DiagnosticPushAddressAgainRawFileOffset, DiagnosticArgumentVirtualAddress);

            diagnosticCallSiteReached = true;
            instructions.Add(new(
                GameSocketReceiveSenderPreparationInstructionKind.CallDiagnosticImport,
                DiagnosticCallRawFileOffset,
                EffectiveAddress: DiagnosticImportCellVirtualAddress));
            opaqueCalls.Add(new(
                GameSocketReceiveSenderPreparationOpaqueCallKind.DiagnosticImport,
                DiagnosticCallRawFileOffset,
                null,
                null,
                DiagnosticImportCellVirtualAddress,
                false,
                true,
                true,
                inputs.DiagnosticReturnedNormally,
                null,
                null));
            if (inputs.DiagnosticReturnedNormally is not bool diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveSenderPreparationStop.AwaitingDiagnosticReturnAtF02,
                    DiagnosticCallRawFileOffset);
            }

            consumed.Add(
                GameSocketReceiveSenderPreparationInput.DiagnosticReturnedNormally);
            if (!diagnosticReturned)
            {
                return Finish(
                    GameSocketReceiveSenderPreparationStop.DiagnosticDidNotReturnNormally,
                    DiagnosticCallRawFileOffset);
            }

            flagsAtStop = null;
            instructions.Add(new(
                GameSocketReceiveSenderPreparationInstructionKind.AddEsp10,
                AddEspRawFileOffset,
                EffectiveAddress: inputs.FreshPostDiagnosticEsp,
                OperandValue: DiagnosticStackCleanupSize));
            if (inputs.FreshPostDiagnosticEsp is not uint postDiagnosticEsp)
            {
                return Finish(
                    GameSocketReceiveSenderPreparationStop.AwaitingPostDiagnosticEspAtF08,
                    AddEspRawFileOffset);
            }

            consumed.Add(GameSocketReceiveSenderPreparationInput.FreshPostDiagnosticEsp);
            uint espAfterCleanup = unchecked(
                postDiagnosticEsp + DiagnosticStackCleanupSize);
            flagsAfterDiagnosticStackCleanup =
                GetAddFlags(postDiagnosticEsp, DiagnosticStackCleanupSize);
            flagsAtStop = flagsAfterDiagnosticStackCleanup;
            instructions[^1] = instructions[^1] with
            {
                ResultValue = espAfterCleanup
            };
            diagnosticStackPointer = espAfterCleanup;
        }

        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.ReloadFreshOperation,
            FreshReloadRawFileOffset));
        freshReloadAddress = unchecked(receiver + OperationFieldOffset);
        freshReloadReadAttempted = true;
        AddMemoryEffect(new(
            GameSocketReceiveSenderPreparationMemoryEffectKind.FreshReloadedOperationRead,
            FreshReloadRawFileOffset,
            freshReloadAddress,
            inputs.FreshReloadedOperation,
            true,
            inputs.FreshReloadedOperation.HasValue));
        instructions[^1] = instructions[^1] with
        {
            EffectiveAddress = freshReloadAddress,
            OperandValue = inputs.FreshReloadedOperation
        };
        if (inputs.FreshReloadedOperation is not uint reloadedOperation)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.AwaitingFreshReloadAtF0B,
                FreshReloadRawFileOffset);
        }

        consumed.Add(GameSocketReceiveSenderPreparationInput.FreshReloadedOperation);
        uint? espBeforeLea = diagnosticBranchTaken == true
            ? diagnosticStackPointer
            : inputs.EntryEsp;
        if (diagnosticBranchTaken != true && inputs.EntryEsp is uint)
            consumed.Add(GameSocketReceiveSenderPreparationInput.EntryEsp);
        localBufferPointer = espBeforeLea is uint stackBeforeLea
            ? unchecked(stackBeforeLea + LocalBufferOffset)
            : null;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.LeaLocalBuffer,
            LocalBufferLeaRawFileOffset,
            EffectiveAddress: espBeforeLea,
            OperandValue: LocalBufferOffset,
            ResultValue: localBufferPointer));
        if (espBeforeLea is not uint stackBeforePush)
        {
            return Finish(
                GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtF11,
                LocalBufferLeaRawFileOffset);
        }

        senderArgumentStackAddress = unchecked(stackBeforePush - sizeof(uint));
        stackPointerAtSenderCall = senderArgumentStackAddress;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.PushLocalBufferPointer,
            LocalBufferPushRawFileOffset,
            EffectiveAddress: senderArgumentStackAddress,
            OperandValue: localBufferPointer));
        AddMemoryEffect(new(
            GameSocketReceiveSenderPreparationMemoryEffectKind.SenderArgumentStackWrite,
            LocalBufferPushRawFileOffset,
            senderArgumentStackAddress,
            localBufferPointer,
            true,
            true));

        uint senderEcx = unchecked(reloadedOperation + SenderReceiverFieldOffset);
        flagsAfterSenderOffsetAdd = GetAddFlags(
            reloadedOperation, SenderReceiverFieldOffset);
        flagsAtStop = flagsAfterSenderOffsetAdd;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.AddEcx18,
            AddEcxRawFileOffset,
            OperandValue: SenderReceiverFieldOffset,
            ResultValue: senderEcx));

        senderHelperCallSiteReached = true;
        instructions.Add(new(
            GameSocketReceiveSenderPreparationInstructionKind.CallSenderHelper,
            SenderHelperCallRawFileOffset,
            TargetRawFileOffset: BoundedSenderHelperTargetRawFileOffset,
            TargetAddress: BoundedSenderHelperTargetVirtualAddress,
            OperandValue: senderEcx,
            ResultValue: localBufferPointer));
        opaqueCalls.Add(new(
            GameSocketReceiveSenderPreparationOpaqueCallKind.SenderHelper,
            SenderHelperCallRawFileOffset,
            BoundedSenderHelperTargetRawFileOffset,
            BoundedSenderHelperTargetVirtualAddress,
            null,
            true,
            true,
            false,
            null,
            localBufferPointer,
            senderEcx));

        return Finish(
            GameSocketReceiveSenderPreparationStop.SenderHelperCallBoundaryAtF19,
            SenderHelperCallRawFileOffset,
            true);

        void PushDiagnosticArgument(uint rawFileOffset, uint value)
        {
            uint? stackAddress = diagnosticStackPointer.HasValue
                ? unchecked(diagnosticStackPointer.Value - sizeof(uint))
                : null;
            if (stackAddress.HasValue)
                diagnosticStackPointer = stackAddress;

            instructions.Add(new(
                GameSocketReceiveSenderPreparationInstructionKind.PushDiagnosticArgument,
                rawFileOffset,
                EffectiveAddress: stackAddress,
                OperandValue: value));
            AddMemoryEffect(new(
                GameSocketReceiveSenderPreparationMemoryEffectKind.DiagnosticStackWrite,
                rawFileOffset,
                stackAddress,
                value,
                true,
                stackAddress.HasValue));
        }
    }

    public static GameSocketReceiveResultGateFlags GetAddFlags(uint left, uint right)
    {
        ulong wideResult = (ulong)left + right;
        uint result = unchecked(left + right);
        return new(
            Carry: wideResult > uint.MaxValue,
            Parity: (BitOperations.PopCount(result & 0xFF) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x8000_0000) != 0,
            Overflow: ((~(left ^ right) & (left ^ result)) & 0x8000_0000) != 0);
    }

    private static GameSocketReceiveSenderPreparationCaptureState GetCaptureState(
        GameSocketReceiveSenderPreparationProjectionInputs inputs,
        HashSet<GameSocketReceiveSenderPreparationInput> consumed,
        GameSocketReceiveSenderPreparationStop stop,
        GameSocketReceiveSenderPreparationInput input)
    {
        if (consumed.Contains(input))
            return GameSocketReceiveSenderPreparationCaptureState.Captured;
        if (IsSupplied(inputs, input))
            return GameSocketReceiveSenderPreparationCaptureState.SuppliedButUnused;

        bool required = input switch
        {
            GameSocketReceiveSenderPreparationInput.ReceiverEsi =>
                stop == GameSocketReceiveSenderPreparationStop.AwaitingReceiverEsiAtEEB,
            GameSocketReceiveSenderPreparationInput.InitialOperation =>
                stop == GameSocketReceiveSenderPreparationStop.AwaitingInitialOperationAtEEB,
            GameSocketReceiveSenderPreparationInput.FreshReloadedOperation =>
                stop == GameSocketReceiveSenderPreparationStop.AwaitingFreshReloadAtF0B,
            GameSocketReceiveSenderPreparationInput.DiagnosticReturnedNormally =>
                stop == GameSocketReceiveSenderPreparationStop.AwaitingDiagnosticReturnAtF02,
            GameSocketReceiveSenderPreparationInput.FreshPostDiagnosticEsp =>
                stop == GameSocketReceiveSenderPreparationStop.AwaitingPostDiagnosticEspAtF08,
            GameSocketReceiveSenderPreparationInput.EntryEsp =>
                stop is GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtF11
                    or GameSocketReceiveSenderPreparationStop.AwaitingEntryEspAtDiagnosticPush,
            _ => false
        };
        if (required)
            return GameSocketReceiveSenderPreparationCaptureState.Missing;

        return GameSocketReceiveSenderPreparationCaptureState.Unreached;
    }

    private static bool IsSupplied(
        GameSocketReceiveSenderPreparationProjectionInputs inputs,
        GameSocketReceiveSenderPreparationInput input) => input switch
    {
        GameSocketReceiveSenderPreparationInput.ReceiverEsi => inputs.ReceiverEsi.HasValue,
        GameSocketReceiveSenderPreparationInput.InitialOperation =>
            inputs.InitialOperation.HasValue,
        GameSocketReceiveSenderPreparationInput.FreshReloadedOperation =>
            inputs.FreshReloadedOperation.HasValue,
        GameSocketReceiveSenderPreparationInput.DiagnosticReturnedNormally =>
            inputs.DiagnosticReturnedNormally.HasValue,
        GameSocketReceiveSenderPreparationInput.FreshPostDiagnosticEsp =>
            inputs.FreshPostDiagnosticEsp.HasValue,
        GameSocketReceiveSenderPreparationInput.EntryEsp => inputs.EntryEsp.HasValue,
        _ => false
    };
}

public sealed record GameSocketReceiveSenderPreparationProjectionResult(
    GameSocketReceiveSenderPreparationProjectionInputs Inputs,
    GameSocketReceiveResultGateProjectionResult ParentProjection,
    bool ParentHandoffAdmitted,
    GameSocketReceiveSenderPreparationStop Stop,
    uint StopAtRawFileOffset,
    ImmutableArray<GameSocketReceiveSenderPreparationInstruction> InstructionOrder,
    ImmutableArray<GameSocketReceiveSenderPreparationMemoryEffect> MemoryEffects,
    ImmutableArray<GameSocketReceiveSenderPreparationTraceStep> OrderedMemoryEffects,
    ImmutableArray<GameSocketReceiveSenderPreparationOpaqueCall> OpaqueCalls,
    ImmutableArray<GameSocketReceiveSenderPreparationInput> SuppliedButUnusedInputs,
    GameSocketReceiveSenderPreparationCaptureStates CaptureStates,
    uint? InitialOperationReadAddress,
    uint? FreshReloadReadAddress,
    uint? LocalBufferPointer,
    uint? SenderArgumentStackAddress,
    uint? StackPointerAtSenderCall,
    bool InitialOperationReadAttempted,
    bool FreshReloadReadAttempted,
    bool DiagnosticCallSiteReached,
    bool SenderHelperCallSiteReached,
    bool? DiagnosticBranchTaken,
    GameSocketReceiveResultGateFlags? FlagsAfterInitialCompare,
    GameSocketReceiveResultGateFlags? FlagsAfterDiagnosticStackCleanup,
    GameSocketReceiveResultGateFlags? FlagsAfterSenderOffsetAdd,
    GameSocketReceiveResultGateFlags? FlagsAtStop,
    bool ReachedBoundedEndpoint)
{
    public bool HasSuppliedButUnusedInputs => !SuppliedButUnusedInputs.IsEmpty;

    public bool PreparationBoundedCallReached => ReachedBoundedEndpoint;

    public bool ExactBoundedCompletion =>
        PreparationBoundedCallReached
        || (!ParentHandoffAdmitted && ParentProjection.ReachedBoundedEndpoint);

    public bool IsCleanCompletion =>
        ParentHandoffAdmitted
        && PreparationBoundedCallReached
        && ParentProjection.IsCleanCompletion
        && !HasSuppliedButUnusedInputs;

    public bool OpaqueNativeCallsExecuted => false;

    public bool SenderHelperCallExecuted => false;

    public bool PacketReceiveCallModeled => false;

    public bool ReturnInstructionModeled => false;

    public bool AnyMemoryWriteCommitted =>
        MemoryEffects.Any(effect => effect.Committed);
}
