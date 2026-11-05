using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveDirectCallbackCapture(
    uint ReceiverEdi,
    uint PayloadEbx,
    ushort RetainedSi,
    bool EntryBoundaryValidated,
    uint? PayloadDwordAt4,
    uint? ReceiverDwordAt14,
    bool? IndirectCallReturnedNormally);

public enum UnreliableReceiveDirectCallbackOutcome
{
    EntryBoundaryNotValidated,
    UnreachedCaptureProvided,
    MissingPayloadDwordAt4,
    MissingReceiverDwordAt14,
    MissingNormalReturn,
    IndirectCallDidNotReturnNormally,
    IndirectCallReturnedNormally
}

public readonly record struct UnreliableReceiveDirectCallbackPlan(
    UnreliableReceiveDirectCallbackCapture Capture,
    UnreliableReceiveDirectCallbackOutcome Outcome,
    uint ZeroExtendedSi,
    uint PayloadDwordAt4Address,
    uint PayloadAddressAt8,
    bool UnreachedPayloadDwordAt4CaptureProvided,
    bool PayloadDwordAt4Read,
    uint? PayloadDwordAt4,
    bool UnreachedReceiverDwordAt14CaptureProvided,
    bool UnreachedNormalReturnCaptureProvided,
    bool ReceiverDwordAt14Read,
    uint? ReceiverDwordAt14Address,
    uint? CapturedCallbackTarget,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    bool IndirectCallReached,
    bool? IndirectCallReturnedNormally,
    bool NormalCallBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveDirectCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the direct callback's ordered pushes, receiver target reload, and normal-return boundary.
/// </summary>
public static class UnreliableReceiveDirectCallbackProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5B52;
    public const uint EntryVirtualAddress = 0x00FE6752;
    public const uint PayloadDwordAt4PushRawFileOffset = 0x00BE5B58;
    public const uint PayloadAddressAt8PushRawFileOffset = 0x00BE5B5E;
    public const uint ReceiverDwordAt14ReadRawFileOffset = 0x00BE5B5F;
    public const uint IndirectCallRawFileOffset = 0x00BE5B62;
    public const uint NormalCallBoundaryRawFileOffset = 0x00BE5B64;
    public const uint NormalCallBoundaryVirtualAddress = 0x00FE6764;
    public const uint EndRawFileOffsetExclusive = NormalCallBoundaryRawFileOffset;

    public const uint PayloadDwordAt4Offset = 4;
    public const uint PayloadAddressAt8Offset = 8;
    public const uint ReceiverDwordAt14Offset = 0x14;

    public const string CapturedCallbackBytes =
        "6A 00 0F B7 C6 50 FF 73 04 8D 43 08 50 8B 47 14 FF D0";

    public static ImmutableArray<UnreliableReceiveDirectCallbackInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Push the immediate zero",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "6A 00"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Zero-extend the retained SI WORD into EAX",
                0x00BE5B54,
                RawFileOffsetToVirtualAddress(0x00BE5B54),
                "0F B7 C6"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Push the zero-extended retained SI value",
                0x00BE5B57,
                RawFileOffsetToVirtualAddress(0x00BE5B57),
                "50"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Push the DWORD at [EBX+4]",
                PayloadDwordAt4PushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordAt4PushRawFileOffset),
                "FF 73 04"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Load the address EBX+8 into EAX",
                0x00BE5B5B,
                RawFileOffsetToVirtualAddress(0x00BE5B5B),
                "8D 43 08"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Push the EBX+8 address",
                PayloadAddressAt8PushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadAddressAt8PushRawFileOffset),
                "50"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Reload the callback target from [EDI+0x14]",
                ReceiverDwordAt14ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAt14ReadRawFileOffset),
                "8B 47 14"),
            new UnreliableReceiveDirectCallbackInstructionEvidence(
                "Call through the reloaded EAX target",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF D0"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadAddress(uint payloadEbx, uint offset) =>
        unchecked(payloadEbx + offset);

    public static uint ReceiverDwordAt14Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAt14Offset);

    public static UnreliableReceiveDirectCallbackPlan Project(
        UnreliableReceiveDirectCallbackCapture capture)
    {
        uint zeroExtendedSi = capture.RetainedSi;
        uint payloadDwordAt4Address = PayloadAddress(capture.PayloadEbx, PayloadDwordAt4Offset);
        uint payloadAddressAt8 = PayloadAddress(capture.PayloadEbx, PayloadAddressAt8Offset);

        if (!capture.EntryBoundaryValidated)
        {
            bool laterCaptureProvided =
                capture.PayloadDwordAt4.HasValue
                || capture.ReceiverDwordAt14.HasValue
                || capture.IndirectCallReturnedNormally.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveDirectCallbackOutcome.EntryBoundaryNotValidated,
                zeroExtendedSi,
                payloadDwordAt4Address,
                payloadAddressAt8,
                unreachedPayloadDwordAt4CaptureProvided:
                    capture.PayloadDwordAt4.HasValue,
                unreachedReceiverDwordAt14CaptureProvided:
                    capture.ReceiverDwordAt14.HasValue,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        ImmutableArray<uint> initialPushes = ImmutableArray.Create(0u, zeroExtendedSi);
        if (capture.PayloadDwordAt4 is not uint payloadDwordAt4)
        {
            bool laterCaptureProvided =
                capture.ReceiverDwordAt14.HasValue
                || capture.IndirectCallReturnedNormally.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveDirectCallbackOutcome.MissingPayloadDwordAt4,
                zeroExtendedSi,
                payloadDwordAt4Address,
                payloadAddressAt8,
                unreachedReceiverDwordAt14CaptureProvided:
                    capture.ReceiverDwordAt14.HasValue,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                stackPushesInInstructionOrder: initialPushes,
                stopAt: PayloadDwordAt4PushRawFileOffset);
        }

        ImmutableArray<uint> pushesThroughPayload = ImmutableArray.Create(
            0u,
            zeroExtendedSi,
            payloadDwordAt4,
            payloadAddressAt8);
        uint receiverDwordAt14Address = ReceiverDwordAt14Address(capture.ReceiverEdi);
        if (capture.ReceiverDwordAt14 is not uint callbackTarget)
        {
            return Stop(
                capture,
                capture.IndirectCallReturnedNormally.HasValue
                    ? UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveDirectCallbackOutcome.MissingReceiverDwordAt14,
                zeroExtendedSi,
                payloadDwordAt4Address,
                payloadAddressAt8,
                payloadDwordAt4Read: true,
                payloadDwordAt4: payloadDwordAt4,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                receiverDwordAt14Address: receiverDwordAt14Address,
                stackPushesInInstructionOrder: pushesThroughPayload,
                stopAt: ReceiverDwordAt14ReadRawFileOffset);
        }

        if (capture.IndirectCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveDirectCallbackOutcome.MissingNormalReturn,
                zeroExtendedSi,
                payloadDwordAt4Address,
                payloadAddressAt8,
                payloadDwordAt4Read: true,
                payloadDwordAt4: payloadDwordAt4,
                receiverDwordAt14Read: true,
                receiverDwordAt14Address: receiverDwordAt14Address,
                capturedCallbackTarget: callbackTarget,
                stackPushesInInstructionOrder: pushesThroughPayload,
                indirectCallReached: true,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            returnedNormally
                ? UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally
                : UnreliableReceiveDirectCallbackOutcome.IndirectCallDidNotReturnNormally,
            zeroExtendedSi,
            payloadDwordAt4Address,
            payloadAddressAt8,
            payloadDwordAt4Read: true,
            payloadDwordAt4: payloadDwordAt4,
            receiverDwordAt14Read: true,
            receiverDwordAt14Address: receiverDwordAt14Address,
            capturedCallbackTarget: callbackTarget,
            stackPushesInInstructionOrder: pushesThroughPayload,
            indirectCallReached: true,
            indirectCallReturnedNormally: returnedNormally,
            normalCallBoundaryReached: returnedNormally,
            selectedDestinationRawFileOffset: returnedNormally
                ? NormalCallBoundaryRawFileOffset
                : null,
            stopAt: returnedNormally
                ? NormalCallBoundaryRawFileOffset
                : IndirectCallRawFileOffset);
    }

    private static UnreliableReceiveDirectCallbackPlan Stop(
        UnreliableReceiveDirectCallbackCapture capture,
        UnreliableReceiveDirectCallbackOutcome outcome,
        uint zeroExtendedSi,
        uint payloadDwordAt4Address,
        uint payloadAddressAt8,
        bool unreachedPayloadDwordAt4CaptureProvided = false,
        bool payloadDwordAt4Read = false,
        uint? payloadDwordAt4 = null,
        bool unreachedReceiverDwordAt14CaptureProvided = false,
        bool unreachedNormalReturnCaptureProvided = false,
        bool receiverDwordAt14Read = false,
        uint? receiverDwordAt14Address = null,
        uint? capturedCallbackTarget = null,
        ImmutableArray<uint> stackPushesInInstructionOrder = default,
        bool indirectCallReached = false,
        bool? indirectCallReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            zeroExtendedSi,
            payloadDwordAt4Address,
            payloadAddressAt8,
            unreachedPayloadDwordAt4CaptureProvided,
            payloadDwordAt4Read,
            payloadDwordAt4,
            unreachedReceiverDwordAt14CaptureProvided,
            unreachedNormalReturnCaptureProvided,
            receiverDwordAt14Read,
            receiverDwordAt14Address,
            capturedCallbackTarget,
            stackPushesInInstructionOrder.IsDefault
                ? ImmutableArray<uint>.Empty
                : stackPushesInInstructionOrder,
            indirectCallReached,
            indirectCallReturnedNormally,
            normalCallBoundaryReached,
            selectedDestinationRawFileOffset,
            stopAt);
}
