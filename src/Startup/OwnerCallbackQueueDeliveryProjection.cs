using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum OwnerCallbackQueueDeliveryCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct OwnerCallbackQueueDeliveryCapture<T> where T : struct
{
    private OwnerCallbackQueueDeliveryCapture(
        OwnerCallbackQueueDeliveryCaptureKind kind,
        T? value)
    {
        Kind = kind;
        Value = value;
    }

    public OwnerCallbackQueueDeliveryCaptureKind Kind { get; }
    public T? Value { get; }

    public static OwnerCallbackQueueDeliveryCapture<T> Missing() =>
        new(OwnerCallbackQueueDeliveryCaptureKind.Missing, null);

    public static OwnerCallbackQueueDeliveryCapture<T> Unreached() =>
        new(OwnerCallbackQueueDeliveryCaptureKind.Unreached, null);

    public static OwnerCallbackQueueDeliveryCapture<T> Captured(T value) =>
        new(OwnerCallbackQueueDeliveryCaptureKind.Captured, value);
}

public readonly record struct OwnerCallbackQueueDeliveryCaptures(
    OwnerCallbackQueueDeliveryCapture<uint> PoppedPayload,
    OwnerCallbackQueueDeliveryCapture<uint> CallbackReceiver,
    OwnerCallbackQueueDeliveryCapture<bool> CallbackReturnedNormally,
    OwnerCallbackQueueDeliveryCapture<uint> ReloadedPayloadAfterCallback,
    OwnerCallbackQueueDeliveryCapture<uint> CounterBeforeXadd,
    OwnerCallbackQueueDeliveryCapture<bool> ReleaseCallReturnedNormally);

public enum OwnerCallbackQueueDeliveryOutcome
{
    MissingPopResultBoundary,
    PopResultZeroReachesContinuation,
    MissingCallbackReceiverBoundary,
    MissingCallbackReturnBoundary,
    CallbackDidNotReturn,
    MissingPayloadReloadBoundary,
    CallbackRewrotePayloadToNull,
    MissingCounterBoundary,
    CounterOldValueNotOneReachesContinuation,
    MissingReleaseReturnBoundary,
    ReleaseCallDidNotReturn,
    ReleaseCallReturnedToContinuation
}

public enum OwnerCallbackQueueDeliveryActionKind
{
    SavePopResultToLocal,
    TestPopResult,
    BranchToContinuationForNullPopResult,
    CaptureCallbackReceiver,
    FormCallbackArgumentAddress,
    PushCallbackStackArgument,
    LoadCallbackVtable,
    CallOpaqueCallbackVtableSlot,
    ReloadPayloadLocalAfterCallback,
    TestReloadedPayload,
    BranchToContinuationForNullPayload,
    PrepareCounterDecrement,
    FormPayloadCounterAddress,
    DecrementPayloadCounter,
    CompareCapturedOldCounter,
    BranchToContinuationForOldCounterNotOne,
    TestPayloadBeforeRelease,
    BranchToContinuationForNullReleasePayload,
    LoadReleaseVtable,
    PushReleaseArgument,
    CallOpaqueReleaseVtableSlot,
    ReachCommonContinuationBoundary,
    StopAtMissingPopResultCapture,
    StopAtMissingCallbackReceiverCapture,
    StopAtMissingCallbackReturnCapture,
    StopAtMissingPayloadReloadCapture,
    StopAtMissingCounterCapture,
    StopAtMissingReleaseReturnCapture,
    StopAtCallbackCallBoundary,
    StopAtReleaseCallBoundary
}

public readonly record struct OwnerCallbackQueueDeliveryAction(
    OwnerCallbackQueueDeliveryActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Pointer = null,
    uint? CapturedValue = null,
    uint? BeforeValue = null,
    uint? AfterValue = null,
    uint? Receiver = null,
    uint? StackArgumentValue = null,
    uint? StackArgumentAddressOffset = null,
    int? AddressOffset = null,
    int? VtableSlotByteOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    bool? CallReturnedNormally = null,
    bool? ConditionMet = null,
    bool? XaddPriorValueUsed = null,
    bool IndirectTargetOpaque = false);

public readonly record struct OwnerCallbackQueueDeliveryPlan(
    OwnerCallbackQueueDeliveryCaptures Captures,
    OwnerCallbackQueueDeliveryOutcome Outcome,
    ImmutableArray<OwnerCallbackQueueDeliveryAction> Actions,
    uint? CallbackReceiver,
    uint? PayloadAfterCallback,
    uint? PayloadCounterAddress,
    uint? CapturedOldCounterValue,
    uint? CounterAfterXadd,
    uint CallbackStackArgumentAddressOffset,
    int CallbackVtableSlotByteOffset,
    int ReleaseVtableSlotByteOffset,
    uint ReleaseArgument,
    uint ContinuationRawFileOffset,
    uint ContinuationVirtualAddress);

public readonly record struct OwnerCallbackQueueDeliveryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    bool EvidenceOnly = false);

/// <summary>
/// Projects the bounded owner-callback queue-delivery continuation. All
/// pointers, counter contents, and call-return captures are supplied values;
/// this does not execute native code, access memory, or infer object ownership
/// or deallocation semantics.
/// </summary>
public static class OwnerCallbackQueueDeliveryProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD523E;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD5277;
    public const uint EntryEndVirtualAddressExclusive =
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint CapturedBytesEndRawFileOffsetExclusive = 0x00BD527E;
    public const uint CapturedBytesEndVirtualAddressExclusive =
        CapturedBytesEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PopResultSaveRawFileOffset = 0x00BD523E;
    public const uint PopResultTestRawFileOffset = 0x00BD5242;
    public const uint PopResultNullBranchRawFileOffset = 0x00BD5244;
    public const uint CallbackReceiverCaptureRawFileOffset = 0x00BD5246;
    public const uint CallbackStackArgumentSetupRawFileOffset = 0x00BD524C;
    public const uint CallbackStackArgumentPushRawFileOffset = 0x00BD5250;
    public const uint CallbackVtableLoadRawFileOffset = 0x00BD5251;
    public const uint CallbackCallRawFileOffset = 0x00BD5253;
    public const uint PayloadReloadRawFileOffset = 0x00BD5256;
    public const uint PayloadNullTestRawFileOffset = 0x00BD525A;
    public const uint PayloadNullBranchRawFileOffset = 0x00BD525C;
    public const uint CounterDecrementSetupRawFileOffset = 0x00BD525E;
    public const uint CounterAddressSetupRawFileOffset = 0x00BD5261;
    public const uint CounterXaddRawFileOffset = 0x00BD5264;
    public const uint CounterCompareRawFileOffset = 0x00BD5268;
    public const uint CounterNotOneBranchRawFileOffset = 0x00BD526B;
    public const uint ReleasePayloadTestRawFileOffset = 0x00BD526D;
    public const uint ReleasePayloadNullBranchRawFileOffset = 0x00BD526F;
    public const uint ReleaseVtableLoadRawFileOffset = 0x00BD5271;
    public const uint ReleaseArgumentPushRawFileOffset = 0x00BD5273;
    public const uint ReleaseCallRawFileOffset = 0x00BD5274;
    public const uint CommonContinuationRawFileOffset = 0x00BD5277;

    public const int OwnerCallbackReceiverOffset = 0x10C;
    public const uint SavedLocalStackAddressOffset = 0x14;
    public const int PayloadCounterOffset = 8;
    public const int CallbackVtableSlotByteOffset = 4;
    public const int ReleaseVtableSlotByteOffset = 8;
    public const uint ReleaseArgument = 1;
    public const uint DecrementOperand = uint.MaxValue;

    public const string CapturedEvidenceBytes =
        "89 44 24 14 85 C0 74 31 8B 8E 0C 01 00 00 8D 54 24 14 " +
        "52 8B 01 FF 50 04 8B 4C 24 14 85 C9 74 19 83 CA FF " +
        "8D 41 08 F0 0F C1 10 83 FA 01 75 0A 85 C9 74 06 8B 01 " +
        "52 FF 50 08 83 BE 0C 01 00 00 00";

    public static uint EntryVirtualAddressFromRawFileOffset =>
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public static uint EntryEndVirtualAddressFromRawFileOffset =>
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public static uint CapturedBytesEndVirtualAddressFromRawFileOffset =>
        CapturedBytesEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public static uint CommonContinuationVirtualAddress =>
        CommonContinuationRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public static IReadOnlyList<OwnerCallbackQueueDeliveryInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Save the popped payload in the local at [esp+0x14]",
                PopResultSaveRawFileOffset,
                "89 44 24 14"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Test the captured pop result",
                PopResultTestRawFileOffset,
                "85 C0"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "A zero pop result branches to the common continuation",
                PopResultNullBranchRawFileOffset,
                "74 31",
                CommonContinuationRawFileOffset,
                CommonContinuationVirtualAddress),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Capture the callback receiver independently from the popped payload",
                CallbackReceiverCaptureRawFileOffset,
                "8B 8E 0C 01 00 00"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Form the saved-local address passed to the callback",
                CallbackStackArgumentSetupRawFileOffset,
                "8D 54 24 14"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Push the saved-local address as the callback stack argument",
                CallbackStackArgumentPushRawFileOffset,
                "52"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Load the callback receiver's vtable",
                CallbackVtableLoadRawFileOffset,
                "8B 01"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Call the opaque callback target at vtable byte offset +4",
                CallbackCallRawFileOffset,
                "FF 50 04"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Reload the local after callback execution, independently of the original pop result",
                PayloadReloadRawFileOffset,
                "8B 4C 24 14"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Test the independently reloaded payload",
                PayloadNullTestRawFileOffset,
                "85 C9"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "A null reloaded payload branches to the common continuation",
                PayloadNullBranchRawFileOffset,
                "74 19",
                CommonContinuationRawFileOffset,
                CommonContinuationVirtualAddress),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Prepare the wrapped 32-bit decrement operand",
                CounterDecrementSetupRawFileOffset,
                "83 CA FF"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Form the independently reloaded payload counter address at +8",
                CounterAddressSetupRawFileOffset,
                "8D 41 08"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Locked XADD decrements the payload counter and captures its old value in EDX",
                CounterXaddRawFileOffset,
                "F0 0F C1 10"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Compare the captured old counter value with one",
                CounterCompareRawFileOffset,
                "83 FA 01"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "A captured old counter other than one skips the release call",
                CounterNotOneBranchRawFileOffset,
                "75 0A",
                CommonContinuationRawFileOffset,
                CommonContinuationVirtualAddress),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Test the still-live reloaded payload before the release call",
                ReleasePayloadTestRawFileOffset,
                "85 C9"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "A null payload skips the release call",
                ReleasePayloadNullBranchRawFileOffset,
                "74 06",
                CommonContinuationRawFileOffset,
                CommonContinuationVirtualAddress),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Load the payload's vtable for the opaque release call",
                ReleaseVtableLoadRawFileOffset,
                "8B 01"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Push the release-call stack argument, one",
                ReleaseArgumentPushRawFileOffset,
                "52"),
            new OwnerCallbackQueueDeliveryInstructionEvidence(
                "Call the opaque payload vtable target at byte offset +8",
                ReleaseCallRawFileOffset,
                "FF 50 08"),
        });

    public static OwnerCallbackQueueDeliveryInstructionEvidence BoundaryInstructionEvidence { get; } =
        new(
            "Evidence only: common continuation instruction, outside the projected range",
            CommonContinuationRawFileOffset,
            "83 BE 0C 01 00 00 00",
            EvidenceOnly: true);

    public static OwnerCallbackQueueDeliveryPlan Project(
        OwnerCallbackQueueDeliveryCaptures captures)
    {
        var actions = ImmutableArray.CreateBuilder<OwnerCallbackQueueDeliveryAction>();

        if (captures.PoppedPayload.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("The pop result is captured at the entry instruction.", nameof(captures));
        }

        if (captures.PoppedPayload.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            RequireUnreached(captures.CallbackReceiver);
            RequireUnreached(captures.CallbackReturnedNormally);
            RequireUnreached(captures.ReloadedPayloadAfterCallback);
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingPopResultBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingPopResultCapture,
                PopResultSaveRawFileOffset);
        }

        uint poppedPayload = Value(captures.PoppedPayload);
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.SavePopResultToLocal,
            PopResultSaveRawFileOffset,
            CapturedValue: poppedPayload,
            StackArgumentAddressOffset: SavedLocalStackAddressOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.TestPopResult,
            PopResultTestRawFileOffset,
            CapturedValue: poppedPayload,
            ConditionMet: poppedPayload != 0));

        if (poppedPayload == 0)
        {
            RequireUnreached(captures.CallbackReceiver);
            RequireUnreached(captures.CallbackReturnedNormally);
            RequireUnreached(captures.ReloadedPayloadAfterCallback);
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            actions.Add(BranchToContinuation(
                OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullPopResult,
                PopResultNullBranchRawFileOffset));
            return AtContinuation(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.PopResultZeroReachesContinuation);
        }

        if (captures.CallbackReceiver.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("The nonzero pop path reads the callback receiver.", nameof(captures));
        }

        if (captures.CallbackReceiver.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            RequireUnreached(captures.CallbackReturnedNormally);
            RequireUnreached(captures.ReloadedPayloadAfterCallback);
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingCallbackReceiverBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingCallbackReceiverCapture,
                CallbackReceiverCaptureRawFileOffset);
        }

        uint callbackReceiver = Value(captures.CallbackReceiver);
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.CaptureCallbackReceiver,
            CallbackReceiverCaptureRawFileOffset,
            Pointer: callbackReceiver,
            AddressOffset: OwnerCallbackReceiverOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.FormCallbackArgumentAddress,
            CallbackStackArgumentSetupRawFileOffset,
            StackArgumentAddressOffset: SavedLocalStackAddressOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.PushCallbackStackArgument,
            CallbackStackArgumentPushRawFileOffset,
            StackArgumentAddressOffset: SavedLocalStackAddressOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.LoadCallbackVtable,
            CallbackVtableLoadRawFileOffset,
            Receiver: callbackReceiver));

        if (captures.CallbackReturnedNormally.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("The callback call is reached for a nonzero pop result.", nameof(captures));
        }

        if (captures.CallbackReturnedNormally.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            RequireUnreached(captures.ReloadedPayloadAfterCallback);
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            actions.Add(CallbackCall(callbackReceiver, null));
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingCallbackReturnBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingCallbackReturnCapture,
                CallbackCallRawFileOffset,
                callbackReceiver);
        }

        bool callbackReturned = Value(captures.CallbackReturnedNormally);
        actions.Add(CallbackCall(callbackReceiver, callbackReturned));
        if (!callbackReturned)
        {
            RequireUnreached(captures.ReloadedPayloadAfterCallback);
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.CallbackDidNotReturn,
                OwnerCallbackQueueDeliveryActionKind.StopAtCallbackCallBoundary,
                CallbackCallRawFileOffset,
                callbackReceiver);
        }

        if (captures.ReloadedPayloadAfterCallback.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("A normal callback return reaches the local reload.", nameof(captures));
        }

        if (captures.ReloadedPayloadAfterCallback.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingPayloadReloadBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingPayloadReloadCapture,
                PayloadReloadRawFileOffset,
                callbackReceiver);
        }

        uint payload = Value(captures.ReloadedPayloadAfterCallback);
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.ReloadPayloadLocalAfterCallback,
            PayloadReloadRawFileOffset,
            CapturedValue: payload,
            StackArgumentAddressOffset: SavedLocalStackAddressOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.TestReloadedPayload,
            PayloadNullTestRawFileOffset,
            Pointer: payload,
            ConditionMet: payload != 0));

        if (payload == 0)
        {
            RequireUnreached(captures.CounterBeforeXadd);
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            actions.Add(BranchToContinuation(
                OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullPayload,
                PayloadNullBranchRawFileOffset));
            return AtContinuation(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.CallbackRewrotePayloadToNull,
                callbackReceiver,
                payload);
        }

        if (captures.CounterBeforeXadd.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("A nonnull reloaded payload reaches the locked XADD.", nameof(captures));
        }

        if (captures.CounterBeforeXadd.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingCounterBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingCounterCapture,
                CounterXaddRawFileOffset,
                callbackReceiver,
                payload);
        }

        uint counterBefore = Value(captures.CounterBeforeXadd);
        uint counterAddress = unchecked(payload + (uint)PayloadCounterOffset);
        uint counterAfter = unchecked(counterBefore + DecrementOperand);
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.PrepareCounterDecrement,
            CounterDecrementSetupRawFileOffset,
            CapturedValue: DecrementOperand));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.FormPayloadCounterAddress,
            CounterAddressSetupRawFileOffset,
            Pointer: counterAddress,
            AddressOffset: PayloadCounterOffset));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.DecrementPayloadCounter,
            CounterXaddRawFileOffset,
            Pointer: counterAddress,
            BeforeValue: counterBefore,
            AfterValue: counterAfter,
            CapturedValue: counterBefore,
            XaddPriorValueUsed: true));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.CompareCapturedOldCounter,
            CounterCompareRawFileOffset,
            CapturedValue: counterBefore,
            ConditionMet: counterBefore == 1));

        if (counterBefore != 1)
        {
            RequireUnreached(captures.ReleaseCallReturnedNormally);
            actions.Add(BranchToContinuation(
                OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForOldCounterNotOne,
                CounterNotOneBranchRawFileOffset,
                conditionMet: true));
            return AtContinuation(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.CounterOldValueNotOneReachesContinuation,
                callbackReceiver,
                payload,
                counterAddress,
                counterBefore,
                counterAfter);
        }

        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.TestPayloadBeforeRelease,
            ReleasePayloadTestRawFileOffset,
            Pointer: payload,
            ConditionMet: true));

        if (captures.ReleaseCallReturnedNormally.Kind == OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("An old counter value of one reaches the release call.", nameof(captures));
        }

        if (captures.ReleaseCallReturnedNormally.Kind == OwnerCallbackQueueDeliveryCaptureKind.Missing)
        {
            actions.Add(BranchToContinuation(
                OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullReleasePayload,
                ReleasePayloadNullBranchRawFileOffset,
                conditionMet: false));
            AddReleaseCallSetup(actions, payload);
            actions.Add(ReleaseCall(null, payload));
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.MissingReleaseReturnBoundary,
                OwnerCallbackQueueDeliveryActionKind.StopAtMissingReleaseReturnCapture,
                ReleaseCallRawFileOffset,
                callbackReceiver,
                payload,
                counterAddress,
                counterBefore,
                counterAfter);
        }

        bool releaseReturned = Value(captures.ReleaseCallReturnedNormally);
        actions.Add(BranchToContinuation(
            OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullReleasePayload,
            ReleasePayloadNullBranchRawFileOffset,
            conditionMet: false));
        AddReleaseCallSetup(actions, payload);
        actions.Add(ReleaseCall(releaseReturned, payload));
        if (!releaseReturned)
        {
            return Stop(
                captures,
                actions,
                OwnerCallbackQueueDeliveryOutcome.ReleaseCallDidNotReturn,
                OwnerCallbackQueueDeliveryActionKind.StopAtReleaseCallBoundary,
                ReleaseCallRawFileOffset,
                callbackReceiver,
                payload,
                counterAddress,
                counterBefore,
                counterAfter);
        }

        return AtContinuation(
            captures,
            actions,
            OwnerCallbackQueueDeliveryOutcome.ReleaseCallReturnedToContinuation,
            callbackReceiver,
            payload,
            counterAddress,
            counterBefore,
            counterAfter);
    }

    private static OwnerCallbackQueueDeliveryAction CallbackCall(
        uint receiver,
        bool? returnedNormally) =>
        new(
            OwnerCallbackQueueDeliveryActionKind.CallOpaqueCallbackVtableSlot,
            CallbackCallRawFileOffset,
            Receiver: receiver,
            StackArgumentAddressOffset: SavedLocalStackAddressOffset,
            VtableSlotByteOffset: CallbackVtableSlotByteOffset,
            CallReturnedNormally: returnedNormally,
            IndirectTargetOpaque: true);

    private static void AddReleaseCallSetup(
        ImmutableArray<OwnerCallbackQueueDeliveryAction>.Builder actions,
        uint payload)
    {
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.LoadReleaseVtable,
            ReleaseVtableLoadRawFileOffset,
            Receiver: payload));
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.PushReleaseArgument,
            ReleaseArgumentPushRawFileOffset,
            StackArgumentValue: ReleaseArgument));
    }

    private static OwnerCallbackQueueDeliveryAction ReleaseCall(
        bool? returnedNormally,
        uint payload) =>
        new(
            OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot,
            ReleaseCallRawFileOffset,
            Receiver: payload,
            StackArgumentValue: ReleaseArgument,
            VtableSlotByteOffset: ReleaseVtableSlotByteOffset,
            CallReturnedNormally: returnedNormally,
            IndirectTargetOpaque: true);

    private static OwnerCallbackQueueDeliveryAction BranchToContinuation(
        OwnerCallbackQueueDeliveryActionKind kind,
        uint branchRawFileOffset,
        bool? conditionMet = true) =>
        new(
            kind,
            branchRawFileOffset,
            TargetRawFileOffset: CommonContinuationRawFileOffset,
            TargetVirtualAddress: CommonContinuationVirtualAddress,
            ConditionMet: conditionMet);

    private static OwnerCallbackQueueDeliveryPlan AtContinuation(
        OwnerCallbackQueueDeliveryCaptures captures,
        ImmutableArray<OwnerCallbackQueueDeliveryAction>.Builder actions,
        OwnerCallbackQueueDeliveryOutcome outcome,
        uint? callbackReceiver = null,
        uint? payload = null,
        uint? counterAddress = null,
        uint? oldCounterValue = null,
        uint? counterAfter = null)
    {
        actions.Add(new OwnerCallbackQueueDeliveryAction(
            OwnerCallbackQueueDeliveryActionKind.ReachCommonContinuationBoundary,
            CommonContinuationRawFileOffset,
            TargetVirtualAddress: CommonContinuationVirtualAddress));
        return Plan(
            captures,
            outcome,
            actions,
            callbackReceiver,
            payload,
            counterAddress,
            oldCounterValue,
            counterAfter);
    }

    private static OwnerCallbackQueueDeliveryPlan Stop(
        OwnerCallbackQueueDeliveryCaptures captures,
        ImmutableArray<OwnerCallbackQueueDeliveryAction>.Builder actions,
        OwnerCallbackQueueDeliveryOutcome outcome,
        OwnerCallbackQueueDeliveryActionKind kind,
        uint rawFileOffset,
        uint? callbackReceiver = null,
        uint? payload = null,
        uint? counterAddress = null,
        uint? oldCounterValue = null,
        uint? counterAfter = null)
    {
        actions.Add(new OwnerCallbackQueueDeliveryAction(kind, rawFileOffset));
        return Plan(
            captures,
            outcome,
            actions,
            callbackReceiver,
            payload,
            counterAddress,
            oldCounterValue,
            counterAfter);
    }

    private static OwnerCallbackQueueDeliveryPlan Plan(
        OwnerCallbackQueueDeliveryCaptures captures,
        OwnerCallbackQueueDeliveryOutcome outcome,
        ImmutableArray<OwnerCallbackQueueDeliveryAction>.Builder actions,
        uint? callbackReceiver,
        uint? payload,
        uint? counterAddress,
        uint? oldCounterValue,
        uint? counterAfter) =>
        new(
            captures,
            outcome,
            actions.ToImmutable(),
            callbackReceiver,
            payload,
            counterAddress,
            oldCounterValue,
            counterAfter,
            SavedLocalStackAddressOffset,
            CallbackVtableSlotByteOffset,
            ReleaseVtableSlotByteOffset,
            ReleaseArgument,
            CommonContinuationRawFileOffset,
            CommonContinuationVirtualAddress);

    private static uint Value(OwnerCallbackQueueDeliveryCapture<uint> capture)
    {
        if (capture.Kind != OwnerCallbackQueueDeliveryCaptureKind.Captured
            || capture.Value is not uint value)
        {
            throw new ArgumentException("A required capture value is missing.");
        }

        return value;
    }

    private static bool Value(OwnerCallbackQueueDeliveryCapture<bool> capture)
    {
        if (capture.Kind != OwnerCallbackQueueDeliveryCaptureKind.Captured
            || capture.Value is not bool value)
        {
            throw new ArgumentException("A required call-return capture is missing.");
        }

        return value;
    }

    private static void RequireUnreached<T>(
        OwnerCallbackQueueDeliveryCapture<T> capture)
        where T : struct
    {
        if (capture.Kind != OwnerCallbackQueueDeliveryCaptureKind.Unreached)
        {
            throw new ArgumentException("A capture was supplied for a path that does not reach it.");
        }
    }
}
