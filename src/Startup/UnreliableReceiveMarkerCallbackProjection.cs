using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveMarkerCallbackCapture(
    uint PayloadEbx,
    ushort RetainedSi,
    byte ValidatedMarker,
    uint? PayloadDwordAt8,
    uint? PayloadDwordAt4,
    uint? IndirectTargetAtPayloadDword8Plus4,
    bool? IndirectCallReturnedNormally);

public enum UnreliableReceiveMarkerCallbackOutcome
{
    MissingPayloadDwordAt8,
    MissingPayloadDwordAt4,
    MissingIndirectTarget,
    MissingNormalReturn,
    UnreachedCaptureProvided,
    IndirectCallDidNotReturnNormally,
    IndirectCallReturnedNormally
}

public readonly record struct UnreliableReceiveMarkerCallbackPlan(
    UnreliableReceiveMarkerCallbackCapture Capture,
    UnreliableReceiveMarkerCallbackOutcome Outcome,
    uint ZeroExtendedSiLength,
    uint EcxPayloadAddress,
    bool PayloadDwordAt8Read,
    uint? PayloadDwordAt8Address,
    uint? PayloadDwordAt8,
    bool UnreachedPayloadDwordAt4CaptureProvided,
    bool UnreachedIndirectTargetCaptureProvided,
    bool UnreachedNormalReturnCaptureProvided,
    bool PayloadDwordAt4Read,
    uint? PayloadDwordAt4Address,
    uint? PayloadDwordAt4,
    bool IndirectTargetRead,
    uint? IndirectTargetSlotAddress,
    uint? CapturedIndirectTarget,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    bool IndirectCallReached,
    bool? IndirectCallReturnedNormally,
    bool NormalCallBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveMarkerCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the validated marker callback through the call's normal-return boundary.
/// Stack pushes are reported in instruction order without assigning argument identities.
/// </summary>
public static class UnreliableReceiveMarkerCallbackProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5B16;
    public const uint EntryVirtualAddress = 0x00FE6716;
    public const uint PayloadDwordAt8ReadRawFileOffset = 0x00BE5B1D;
    public const uint PayloadDwordAt4PushRawFileOffset = 0x00BE5B1F;
    public const uint IndirectCallRawFileOffset = 0x00BE5B22;
    public const uint NormalCallBoundaryRawFileOffset = 0x00BE5B25;
    public const uint NormalCallBoundaryVirtualAddress = 0x00FE6725;
    public const uint EndRawFileOffsetExclusive = NormalCallBoundaryRawFileOffset;

    public const byte MinimumValidatedMarker = 1;
    public const byte MaximumValidatedMarker = 3;
    public const uint PayloadDwordAt8Offset = 8;
    public const uint PayloadDwordAt4Offset = 4;
    public const uint IndirectTargetOffset = 4;

    public const string CapturedCallbackBytes =
        "0F B7 C6 8D 4B 08 50 8B 01 FF 73 04 FF 50 04";

    public static ImmutableArray<UnreliableReceiveMarkerCallbackInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Zero-extend the retained SI WORD into EAX",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "0F B7 C6"),
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Set ECX to the payload address EBX+8",
                0x00BE5B19,
                RawFileOffsetToVirtualAddress(0x00BE5B19),
                "8D 4B 08"),
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Push the zero-extended retained SI value",
                0x00BE5B1C,
                RawFileOffsetToVirtualAddress(0x00BE5B1C),
                "50"),
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Independently read the DWORD at [ECX]",
                PayloadDwordAt8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordAt8ReadRawFileOffset),
                "8B 01"),
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Push the DWORD at [EBX+4]",
                PayloadDwordAt4PushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordAt4PushRawFileOffset),
                "FF 73 04"),
            new UnreliableReceiveMarkerCallbackInstructionEvidence(
                "Call through the DWORD at the loaded payload DWORD plus four",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 50 04"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadAddress(uint payloadEbx, uint offset) =>
        unchecked(payloadEbx + offset);

    public static uint IndirectTargetSlotAddress(uint payloadDwordAt8) =>
        unchecked(payloadDwordAt8 + IndirectTargetOffset);

    public static bool IsValidMarker(byte marker) =>
        marker >= MinimumValidatedMarker && marker <= MaximumValidatedMarker;

    public static void ValidateMarker(byte marker)
    {
        if (!IsValidMarker(marker))
        {
            throw new ArgumentOutOfRangeException(
                nameof(marker),
                marker,
                "The callback requires a validated marker in the range 1..3.");
        }
    }

    public static UnreliableReceiveMarkerCallbackPlan Project(
        UnreliableReceiveMarkerCallbackCapture capture)
    {
        ValidateMarker(capture.ValidatedMarker);

        uint length = capture.RetainedSi;
        uint ecxPayloadAddress = PayloadAddress(capture.PayloadEbx, PayloadDwordAt8Offset);

        if (capture.PayloadDwordAt8 is not uint payloadDwordAt8)
        {
            bool laterCaptureProvided =
                capture.PayloadDwordAt4.HasValue
                || capture.IndirectTargetAtPayloadDword8Plus4.HasValue
                || capture.IndirectCallReturnedNormally.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveMarkerCallbackOutcome.MissingPayloadDwordAt8,
                length,
                ecxPayloadAddress,
                payloadDwordAt8Address: ecxPayloadAddress,
                stackPushesInInstructionOrder: ImmutableArray.Create(length),
                unreachedPayloadDwordAt4CaptureProvided: capture.PayloadDwordAt4.HasValue,
                unreachedIndirectTargetCaptureProvided:
                    capture.IndirectTargetAtPayloadDword8Plus4.HasValue,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                stopAt: PayloadDwordAt8ReadRawFileOffset);
        }

        uint payloadDwordAt4Address = PayloadAddress(capture.PayloadEbx, PayloadDwordAt4Offset);
        if (capture.PayloadDwordAt4 is not uint payloadDwordAt4)
        {
            bool laterCaptureProvided =
                capture.IndirectTargetAtPayloadDword8Plus4.HasValue
                || capture.IndirectCallReturnedNormally.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveMarkerCallbackOutcome.MissingPayloadDwordAt4,
                length,
                ecxPayloadAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: ecxPayloadAddress,
                payloadDwordAt8: payloadDwordAt8,
                payloadDwordAt4Address: payloadDwordAt4Address,
                stackPushesInInstructionOrder: ImmutableArray.Create(length),
                unreachedIndirectTargetCaptureProvided:
                    capture.IndirectTargetAtPayloadDword8Plus4.HasValue,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                stopAt: PayloadDwordAt4PushRawFileOffset);
        }

        uint targetSlotAddress = IndirectTargetSlotAddress(payloadDwordAt8);
        ImmutableArray<uint> pushes = ImmutableArray.Create(length, payloadDwordAt4);
        if (capture.IndirectTargetAtPayloadDword8Plus4 is not uint indirectTarget)
        {
            return Stop(
                capture,
                capture.IndirectCallReturnedNormally.HasValue
                    ? UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveMarkerCallbackOutcome.MissingIndirectTarget,
                length,
                ecxPayloadAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: ecxPayloadAddress,
                payloadDwordAt8: payloadDwordAt8,
                payloadDwordAt4Read: true,
                payloadDwordAt4Address: payloadDwordAt4Address,
                payloadDwordAt4: payloadDwordAt4,
                indirectTargetSlotAddress: targetSlotAddress,
                stackPushesInInstructionOrder: pushes,
                unreachedNormalReturnCaptureProvided:
                    capture.IndirectCallReturnedNormally.HasValue,
                stopAt: IndirectCallRawFileOffset);
        }

        if (capture.IndirectCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveMarkerCallbackOutcome.MissingNormalReturn,
                length,
                ecxPayloadAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: ecxPayloadAddress,
                payloadDwordAt8: payloadDwordAt8,
                payloadDwordAt4Read: true,
                payloadDwordAt4Address: payloadDwordAt4Address,
                payloadDwordAt4: payloadDwordAt4,
                indirectTargetRead: true,
                indirectTargetSlotAddress: targetSlotAddress,
                capturedIndirectTarget: indirectTarget,
                stackPushesInInstructionOrder: pushes,
                indirectCallReached: true,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            returnedNormally
                ? UnreliableReceiveMarkerCallbackOutcome.IndirectCallReturnedNormally
                : UnreliableReceiveMarkerCallbackOutcome.IndirectCallDidNotReturnNormally,
            length,
            ecxPayloadAddress,
            payloadDwordAt8Read: true,
            payloadDwordAt8Address: ecxPayloadAddress,
            payloadDwordAt8: payloadDwordAt8,
            payloadDwordAt4Read: true,
            payloadDwordAt4Address: payloadDwordAt4Address,
            payloadDwordAt4: payloadDwordAt4,
            indirectTargetRead: true,
            indirectTargetSlotAddress: targetSlotAddress,
            capturedIndirectTarget: indirectTarget,
            stackPushesInInstructionOrder: pushes,
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

    private static UnreliableReceiveMarkerCallbackPlan Stop(
        UnreliableReceiveMarkerCallbackCapture capture,
        UnreliableReceiveMarkerCallbackOutcome outcome,
        uint length,
        uint ecxPayloadAddress,
        bool payloadDwordAt8Read = false,
        uint? payloadDwordAt8Address = null,
        uint? payloadDwordAt8 = null,
        bool unreachedPayloadDwordAt4CaptureProvided = false,
        bool unreachedIndirectTargetCaptureProvided = false,
        bool unreachedNormalReturnCaptureProvided = false,
        bool payloadDwordAt4Read = false,
        uint? payloadDwordAt4Address = null,
        uint? payloadDwordAt4 = null,
        bool indirectTargetRead = false,
        uint? indirectTargetSlotAddress = null,
        uint? capturedIndirectTarget = null,
        ImmutableArray<uint> stackPushesInInstructionOrder = default,
        bool indirectCallReached = false,
        bool? indirectCallReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            length,
            ecxPayloadAddress,
            payloadDwordAt8Read,
            payloadDwordAt8Address,
            payloadDwordAt8,
            unreachedPayloadDwordAt4CaptureProvided,
            unreachedIndirectTargetCaptureProvided,
            unreachedNormalReturnCaptureProvided,
            payloadDwordAt4Read,
            payloadDwordAt4Address,
            payloadDwordAt4,
            indirectTargetRead,
            indirectTargetSlotAddress,
            capturedIndirectTarget,
            stackPushesInInstructionOrder.IsDefault
                ? ImmutableArray<uint>.Empty
                : stackPushesInInstructionOrder,
            indirectCallReached,
            indirectCallReturnedNormally,
            normalCallBoundaryReached,
            selectedDestinationRawFileOffset,
            stopAt);
}
