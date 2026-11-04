using Firefall.Reconstruction.Startup;

internal static class QueuePopCompletionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-pop completion anchors use the pinned sample, complete raw range, and raw-offset-to-VA mapping",
            QueuePopCompletionProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueuePopCompletionProjection.EntryRawFileOffset == 0x12CE475
            && QueuePopCompletionProjection.EntryEndRawFileOffsetExclusive == 0x12CE4A4
            && QueuePopCompletionProjection.EntryEndRawFileOffsetExclusive
                - QueuePopCompletionProjection.EntryRawFileOffset == 0x2F
            && QueuePopCompletionProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2F
            && QueuePopCompletionProjection.EntryVirtualAddress == 0x16CF075
            && QueuePopCompletionProjection.EntryEndVirtualAddressExclusive == 0x16CF0A4
            && QueuePopCompletionProjection.PoolPointerGlobalVirtualAddress == 0x01E003BC
            && QueuePopCompletionProjection.PoolPushTargetRawFileOffset == 0x12CDA60
            && QueuePopCompletionProjection.PoolPushTargetVirtualAddress == 0x16CE660
            && QueuePopCompletionProjection.CapturedFunctionBytes
                == "83 CF FF 83 C6 10 8B C7 F0 0F C1 06 8B 44 24 14 8B 70 04 "
                    + "8B 0D BC 03 E0 01 8D 51 10 F0 0F C1 3A 50 E8 C5 F5 FF FF "
                    + "5F 8B C6 5E 5B 8B E5 5D C3");

        check("Queue-pop completion evidence retains exact capture stages, counter instructions, direct target, and plain RET",
            QueuePopCompletionProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.TargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? TargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x12CE475, "83 CF FF", null, null),
                    (0x12CE478, "83 C6 10", null, null),
                    (0x12CE47B, "8B C7", null, null),
                    (0x12CE47D, "F0 0F C1 06", null, null),
                    (0x12CE481, "8B 44 24 14", null, null),
                    (0x12CE485, "8B 70 04", null, null),
                    (0x12CE488, "8B 0D BC 03 E0 01", null, null),
                    (0x12CE48E, "8D 51 10", null, null),
                    (0x12CE491, "F0 0F C1 3A", null, null),
                    (0x12CE495, "50", null, null),
                    (0x12CE496, "E8 C5 F5 FF FF", 0x12CDA60, 0x16CE660),
                    (0x12CE49B, "5F", null, null),
                    (0x12CE49C, "8B C6", null, null),
                    (0x12CE4A3, "C3", null, null)
                })
            && QueuePopCompletionProjection.PushCallRawFileOffset + 5 - 0xA3B
                == QueuePopCompletionProjection.PoolPushTargetRawFileOffset
            && QueuePopCompletionProjection.ReceiverCounterXaddRawFileOffset
                == QueuePopCompletionProjection.EntryRawFileOffset + 8
            && QueuePopCompletionProjection.SavedOldHeadReloadRawFileOffset
                > QueuePopCompletionProjection.ReceiverCounterXaddRawFileOffset
            && QueuePopCompletionProjection.GlobalPoolPointerLoadRawFileOffset
                > QueuePopCompletionProjection.PayloadCaptureRawFileOffset
            && QueuePopCompletionProjection.PoolCounterXaddRawFileOffset
                > QueuePopCompletionProjection.GlobalPoolPointerLoadRawFileOffset
            && QueuePopCompletionProjection.ReturnRawFileOffset
                == QueuePopCompletionProjection.EntryEndRawFileOffsetExclusive - 1);

        QueuePopCompletionPlan returned =
            QueuePopCompletionProjection.Project(
                new QueuePopCompletionCapture(
                    ReceiverPointer: 0xFFFFFFF8,
                    ReceiverCounterBeforeXadd: 0,
                    SavedOldHeadFromStack: 0xFFFFFFFE,
                    PayloadCapturedFromOldHead: 0x55667788,
                    GlobalPoolPointer: 0x10203040,
                    PoolCounterBeforeXadd: 0,
                    PushCallReturnedNormally: true));

        check("Captured stages stay separate; each counter decrement and pointer addition wraps as 32-bit arithmetic",
            returned.ReceiverCounterAddress == 8
            && returned.ReceiverCounterAfterXadd == uint.MaxValue
            && returned.SavedOldHeadStackSlotAddressOffset == 0x14
            && returned.PayloadAddress == 2
            && returned.PoolPointerGlobalVirtualAddress == 0x01E003BC
            && returned.PoolCounterAddress == 0x10203050
            && returned.PoolCounterAfterXadd == uint.MaxValue
            && returned.PushReceiver == 0x10203040
            && returned.PushNodeArgument == 0xFFFFFFFE
            && returned.PushTargetRawFileOffset == 0x12CDA60
            && returned.PushTargetVirtualAddress == 0x16CE660
            && returned.ReturnValue == 0x55667788
            && returned.Actions.SequenceEqual(new[]
            {
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.PrepareDecrementOperand,
                    0x12CE475,
                    CapturedValue: uint.MaxValue),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.FormReceiverCounterAddress,
                    0x12CE478,
                    Pointer: 8),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.DecrementReceiverCounter,
                    0x12CE47D,
                    Pointer: 8,
                    BeforeValue: 0,
                    AfterValue: uint.MaxValue,
                    XaddPriorValueUsed: false),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.ReloadSavedOldHead,
                    0x12CE481,
                    CapturedValue: 0xFFFFFFFE),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.CapturePayloadFromOldHead,
                    0x12CE485,
                    Pointer: 2,
                    CapturedValue: 0x55667788),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.CaptureGlobalPoolPointer,
                    0x12CE488,
                    Pointer: 0x01E003BC,
                    CapturedValue: 0x10203040),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.FormPoolCounterAddress,
                    0x12CE48E,
                    Pointer: 0x10203050),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.DecrementPoolCounter,
                    0x12CE491,
                    Pointer: 0x10203050,
                    BeforeValue: 0,
                    AfterValue: uint.MaxValue,
                    XaddPriorValueUsed: false),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.PushOldHeadToPool,
                    0x12CE495,
                    Pointer: 0x10203040,
                    CapturedValue: 0xFFFFFFFE),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.CallPoolPush,
                    0x12CE496,
                    Pointer: 0x10203040,
                    TargetRawFileOffset: 0x12CDA60,
                    TargetVirtualAddress: 0x16CE660),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.RestoreSavedEdi,
                    0x12CE49B),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.LoadCapturedPayloadForReturn,
                    0x12CE49C,
                    CapturedValue: 0x55667788),
                new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.ReturnCapturedPayload,
                    0x12CE4A3,
                    CapturedValue: 0x55667788)
            }));

        QueuePopCompletionPlan noNormalReturn =
            QueuePopCompletionProjection.Project(
                returned.Capture with { PushCallReturnedNormally = false });
        check("The payload return is reachable only after the pool push returns normally",
            noNormalReturn.Outcome
                == QueuePopCompletionOutcome.PushCallContinuationNotReached
            && noNormalReturn.ReturnValue is null
            && noNormalReturn.Actions.Length == 10
            && noNormalReturn.Actions[^1]
                == new QueuePopCompletionAction(
                    QueuePopCompletionActionKind.CallPoolPush,
                    0x12CE496,
                    Pointer: 0x10203040,
                    TargetRawFileOffset: 0x12CDA60,
                    TargetVirtualAddress: 0x16CE660));
    }
}
