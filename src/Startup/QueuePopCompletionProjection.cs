using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct QueuePopCompletionCapture(
    uint ReceiverPointer,
    uint ReceiverCounterBeforeXadd,
    uint SavedOldHeadFromStack,
    uint PayloadCapturedFromOldHead,
    uint GlobalPoolPointer,
    uint PoolCounterBeforeXadd,
    bool PushCallReturnedNormally);

public enum QueuePopCompletionOutcome
{
    PushCallContinuationNotReached,
    ReturnedCapturedPayload
}

public enum QueuePopCompletionActionKind
{
    PrepareDecrementOperand,
    FormReceiverCounterAddress,
    DecrementReceiverCounter,
    ReloadSavedOldHead,
    CapturePayloadFromOldHead,
    CaptureGlobalPoolPointer,
    FormPoolCounterAddress,
    DecrementPoolCounter,
    PushOldHeadToPool,
    CallPoolPush,
    RestoreSavedEdi,
    LoadCapturedPayloadForReturn,
    ReturnCapturedPayload
}

public readonly record struct QueuePopCompletionAction(
    QueuePopCompletionActionKind Kind,
    uint RawFileOffset,
    uint? Pointer = null,
    uint? CapturedValue = null,
    uint? BeforeValue = null,
    uint? AfterValue = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    bool? XaddPriorValueUsed = null);

public readonly record struct QueuePopCompletionPlan(
    QueuePopCompletionCapture Capture,
    QueuePopCompletionOutcome Outcome,
    ImmutableArray<QueuePopCompletionAction> Actions,
    uint ReceiverCounterAddress,
    uint ReceiverCounterAfterXadd,
    uint SavedOldHeadStackSlotAddressOffset,
    uint PayloadAddress,
    uint PoolPointerGlobalVirtualAddress,
    uint PoolCounterAddress,
    uint PoolCounterAfterXadd,
    uint PushReceiver,
    uint PushNodeArgument,
    uint PushTargetRawFileOffset,
    uint PushTargetVirtualAddress,
    uint? ReturnValue);

public readonly record struct QueuePopCompletionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects only the continuation after a successful pop compare-exchange.
/// Counter values and memory contents are caller-supplied captures: this does
/// not perform memory accesses, emulate either XADD, or model ownership/freeing.
/// The two individual XADD arithmetic results do not model the continuation as
/// one atomic operation. A retry or empty-pop path is outside this projection.
/// </summary>
public static class QueuePopCompletionProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CE475;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CE4A4;
    public const uint ReceiverDecrementSetupRawFileOffset = 0x12CE475;
    public const uint ReceiverCounterAddressSetupRawFileOffset = 0x12CE478;
    public const uint ReceiverCounterXaddRawFileOffset = 0x12CE47D;
    public const uint SavedOldHeadReloadRawFileOffset = 0x12CE481;
    public const uint PayloadCaptureRawFileOffset = 0x12CE485;
    public const uint GlobalPoolPointerLoadRawFileOffset = 0x12CE488;
    public const uint PoolCounterAddressSetupRawFileOffset = 0x12CE48E;
    public const uint PoolCounterXaddRawFileOffset = 0x12CE491;
    public const uint PushOldHeadRawFileOffset = 0x12CE495;
    public const uint PushCallRawFileOffset = 0x12CE496;
    public const uint RestoreSavedEdiRawFileOffset = 0x12CE49B;
    public const uint LoadPayloadForReturnRawFileOffset = 0x12CE49C;
    public const uint ReturnRawFileOffset = 0x12CE4A3;

    public const uint PoolPushTargetRawFileOffset = 0x12CDA60;
    public const uint PoolPointerGlobalVirtualAddress = 0x01E003BC;

    public const int ReceiverCounterOffset = 0x10;
    public const int PoolCounterOffset = 0x10;
    public const int PayloadOffsetFromOldHead = 4;
    public const uint SavedOldHeadStackSlotAddressOffset = 0x14;
    public const uint DecrementOperand = uint.MaxValue;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryEndVirtualAddressExclusive =
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PoolPushTargetVirtualAddress =
        PoolPushTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedFunctionBytes =
        "83 CF FF 83 C6 10 8B C7 F0 0F C1 06 8B 44 24 14 8B 70 04 " +
        "8B 0D BC 03 E0 01 8D 51 10 F0 0F C1 3A 50 E8 C5 F5 FF FF " +
        "5F 8B C6 5E 5B 8B E5 5D C3";

    public static IReadOnlyList<QueuePopCompletionInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueuePopCompletionInstructionEvidence(
                "Set EDI to the decrement operand",
                ReceiverDecrementSetupRawFileOffset,
                "83 CF FF"),
            new QueuePopCompletionInstructionEvidence(
                "Form the receiver counter address at receiver+0x10",
                ReceiverCounterAddressSetupRawFileOffset,
                "83 C6 10"),
            new QueuePopCompletionInstructionEvidence(
                "Copy the decrement operand to EAX",
                0x12CE47B,
                "8B C7"),
            new QueuePopCompletionInstructionEvidence(
                "Locked XADD decrements receiver+0x10; its prior value in EAX is overwritten by the next capture",
                ReceiverCounterXaddRawFileOffset,
                "F0 0F C1 06"),
            new QueuePopCompletionInstructionEvidence(
                "Reload the saved old head from [esp+0x14] (IDA stack expression: [esp+18h+var_4])",
                SavedOldHeadReloadRawFileOffset,
                "8B 44 24 14"),
            new QueuePopCompletionInstructionEvidence(
                "Capture the payload DWORD at savedOldHead+4",
                PayloadCaptureRawFileOffset,
                "8B 70 04"),
            new QueuePopCompletionInstructionEvidence(
                "Load the independent pool pointer from its global VA",
                GlobalPoolPointerLoadRawFileOffset,
                "8B 0D BC 03 E0 01"),
            new QueuePopCompletionInstructionEvidence(
                "Form the pool counter address at pool+0x10",
                PoolCounterAddressSetupRawFileOffset,
                "8D 51 10"),
            new QueuePopCompletionInstructionEvidence(
                "Locked XADD decrements pool+0x10; its prior value in EDI is later restored over",
                PoolCounterXaddRawFileOffset,
                "F0 0F C1 3A"),
            new QueuePopCompletionInstructionEvidence(
                "Push the separately reloaded old head as the push helper argument",
                PushOldHeadRawFileOffset,
                "50"),
            new QueuePopCompletionInstructionEvidence(
                "Call the pool push helper with the captured pool pointer in ECX",
                PushCallRawFileOffset,
                "E8 C5 F5 FF FF",
                PoolPushTargetRawFileOffset,
                PoolPushTargetVirtualAddress),
            new QueuePopCompletionInstructionEvidence(
                "Restore saved EDI after the helper returns",
                RestoreSavedEdiRawFileOffset,
                "5F"),
            new QueuePopCompletionInstructionEvidence(
                "Load the previously captured payload into EAX",
                LoadPayloadForReturnRawFileOffset,
                "8B C6"),
            new QueuePopCompletionInstructionEvidence(
                "Return normally with the captured payload in EAX",
                ReturnRawFileOffset,
                "C3")
        });

    /// <summary>
    /// Uses independent values captured at their respective instructions. If
    /// the helper has not returned normally, the model stops at its call.
    /// </summary>
    public static QueuePopCompletionPlan Project(
        QueuePopCompletionCapture capture)
    {
        uint receiverCounterAddress = unchecked(
            capture.ReceiverPointer + (uint)ReceiverCounterOffset);
        uint receiverCounterAfter = unchecked(
            capture.ReceiverCounterBeforeXadd + DecrementOperand);
        uint payloadAddress = unchecked(
            capture.SavedOldHeadFromStack + (uint)PayloadOffsetFromOldHead);
        uint poolCounterAddress = unchecked(
            capture.GlobalPoolPointer + (uint)PoolCounterOffset);
        uint poolCounterAfter = unchecked(
            capture.PoolCounterBeforeXadd + DecrementOperand);

        var actions = ImmutableArray.CreateBuilder<QueuePopCompletionAction>(
            capture.PushCallReturnedNormally ? 13 : 10);
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.PrepareDecrementOperand,
            ReceiverDecrementSetupRawFileOffset,
            CapturedValue: DecrementOperand));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.FormReceiverCounterAddress,
            ReceiverCounterAddressSetupRawFileOffset,
            Pointer: receiverCounterAddress));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.DecrementReceiverCounter,
            ReceiverCounterXaddRawFileOffset,
            Pointer: receiverCounterAddress,
            BeforeValue: capture.ReceiverCounterBeforeXadd,
            AfterValue: receiverCounterAfter,
            XaddPriorValueUsed: false));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.ReloadSavedOldHead,
            SavedOldHeadReloadRawFileOffset,
            CapturedValue: capture.SavedOldHeadFromStack));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.CapturePayloadFromOldHead,
            PayloadCaptureRawFileOffset,
            Pointer: payloadAddress,
            CapturedValue: capture.PayloadCapturedFromOldHead));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.CaptureGlobalPoolPointer,
            GlobalPoolPointerLoadRawFileOffset,
            Pointer: PoolPointerGlobalVirtualAddress,
            CapturedValue: capture.GlobalPoolPointer));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.FormPoolCounterAddress,
            PoolCounterAddressSetupRawFileOffset,
            Pointer: poolCounterAddress));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.DecrementPoolCounter,
            PoolCounterXaddRawFileOffset,
            Pointer: poolCounterAddress,
            BeforeValue: capture.PoolCounterBeforeXadd,
            AfterValue: poolCounterAfter,
            XaddPriorValueUsed: false));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.PushOldHeadToPool,
            PushOldHeadRawFileOffset,
            Pointer: capture.GlobalPoolPointer,
            CapturedValue: capture.SavedOldHeadFromStack));
        actions.Add(new QueuePopCompletionAction(
            QueuePopCompletionActionKind.CallPoolPush,
            PushCallRawFileOffset,
            Pointer: capture.GlobalPoolPointer,
            TargetRawFileOffset: PoolPushTargetRawFileOffset,
            TargetVirtualAddress: PoolPushTargetVirtualAddress));

        if (capture.PushCallReturnedNormally)
        {
            actions.Add(new QueuePopCompletionAction(
                QueuePopCompletionActionKind.RestoreSavedEdi,
                RestoreSavedEdiRawFileOffset));
            actions.Add(new QueuePopCompletionAction(
                QueuePopCompletionActionKind.LoadCapturedPayloadForReturn,
                LoadPayloadForReturnRawFileOffset,
                CapturedValue: capture.PayloadCapturedFromOldHead));
            actions.Add(new QueuePopCompletionAction(
                QueuePopCompletionActionKind.ReturnCapturedPayload,
                ReturnRawFileOffset,
                CapturedValue: capture.PayloadCapturedFromOldHead));
        }

        return new QueuePopCompletionPlan(
            capture,
            capture.PushCallReturnedNormally
                ? QueuePopCompletionOutcome.ReturnedCapturedPayload
                : QueuePopCompletionOutcome.PushCallContinuationNotReached,
            actions.ToImmutable(),
            receiverCounterAddress,
            receiverCounterAfter,
            SavedOldHeadStackSlotAddressOffset,
            payloadAddress,
            PoolPointerGlobalVirtualAddress,
            poolCounterAddress,
            poolCounterAfter,
            capture.GlobalPoolPointer,
            capture.SavedOldHeadFromStack,
            PoolPushTargetRawFileOffset,
            PoolPushTargetVirtualAddress,
            capture.PushCallReturnedNormally
                ? capture.PayloadCapturedFromOldHead
                : null);
    }
}
