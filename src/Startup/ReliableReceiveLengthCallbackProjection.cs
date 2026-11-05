using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveLengthCallbackCapture(
    bool EntryPriorReturnEaxNonzero,
    uint PayloadEdi,
    ushort? PayloadHeader,
    uint? PayloadDwordAt8,
    uint? PayloadDwordAt4,
    uint? IndirectTargetAtDword8Plus4,
    bool? IndirectCallReturnedNormally);

public enum ReliableReceiveLengthCallbackOutcome
{
    EntryGateNotMet,
    MissingPayloadHeader,
    UnreachedCaptureProvided,
    MissingPayloadDwordAt8,
    MissingPayloadDwordAt4,
    MissingIndirectTarget,
    MissingNormalReturn,
    IndirectCallDidNotReturnNormally,
    IndirectCallReturnedNormally
}

public readonly record struct ReliableReceiveLengthCallbackPlan(
    ReliableReceiveLengthCallbackCapture Capture,
    ReliableReceiveLengthCallbackOutcome Outcome,
    bool UnreachedPayloadHeaderCaptureProvided,
    bool UnreachedPayloadDwordAt8CaptureProvided,
    bool UnreachedPayloadDwordAt4CaptureProvided,
    bool UnreachedIndirectTargetCaptureProvided,
    bool UnreachedNormalReturnCaptureProvided,
    bool PayloadHeaderRead,
    uint? PayloadHeaderAddress,
    ushort? MaskedHeader,
    uint? ComputedLength,
    uint? EcxArgumentAddress,
    bool PayloadDwordAt8Read,
    uint? PayloadDwordAt8Address,
    bool PayloadDwordAt4Read,
    uint? PayloadDwordAt4Address,
    bool IndirectTargetRead,
    uint? IndirectTargetSlotAddress,
    uint? CapturedIndirectTarget,
    ImmutableArray<uint> StackPushes,
    ImmutableArray<uint> CallStackArguments,
    bool IndirectCallReached,
    bool? IndirectCallReturnedNormally,
    bool NormalCallBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveLengthCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the validated-entry length callback through its indirect call's
/// normal-return boundary, without assigning meaning to the captured values.
/// </summary>
public static class ReliableReceiveLengthCallbackProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE53BF;
    public const uint EntryVirtualAddress = 0x00FE5FBF;
    public const uint HeaderReadRawFileOffset = 0x00BE53BF;
    public const uint HeaderMaskRawFileOffset = 0x00BE53C7;
    public const uint EcxArgumentAddressRawFileOffset = 0x00BE53CA;
    public const uint LengthZeroExtendRawFileOffset = 0x00BE53CD;
    public const uint LengthSubtractRawFileOffset = 0x00BE53D0;
    public const uint LengthPushRawFileOffset = 0x00BE53D3;
    public const uint PayloadDwordAt8ReadRawFileOffset = 0x00BE53D4;
    public const uint PayloadDwordAt4PushRawFileOffset = 0x00BE53D7;
    public const uint IndirectCallRawFileOffset = 0x00BE53DA;
    public const uint NormalCallBoundaryRawFileOffset = 0x00BE53DD;
    public const uint NormalCallBoundaryVirtualAddress = 0x00FE5FDD;
    public const uint EndRawFileOffsetExclusive = NormalCallBoundaryRawFileOffset;

    public const ushort HeaderMask = 0x07FF;
    public const uint PayloadDwordAt8Offset = 8;
    public const uint PayloadDwordAt4Offset = 4;
    public const uint IndirectTargetOffset = 4;

    public const string CapturedCallbackBytes =
        "66 8B 07 B9 FF 07 00 00 66 23 C1 8D 4F 08 0F B7 C0 "
        + "83 E8 04 50 8B 47 08 FF 77 04 FF 50 04";

    public static ImmutableArray<ReliableReceiveLengthCallbackInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Read the WORD header at [EDI]",
                HeaderReadRawFileOffset,
                RawFileOffsetToVirtualAddress(HeaderReadRawFileOffset),
                "66 8B 07"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Load the 0x7FF mask",
                0x00BE53C2,
                RawFileOffsetToVirtualAddress(0x00BE53C2),
                "B9 FF 07 00 00"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Mask AX with CX",
                HeaderMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(HeaderMaskRawFileOffset),
                "66 23 C1"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Set ECX to the payload address EDI+8",
                EcxArgumentAddressRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxArgumentAddressRawFileOffset),
                "8D 4F 08"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Zero-extend the masked WORD into EAX",
                LengthZeroExtendRawFileOffset,
                RawFileOffsetToVirtualAddress(LengthZeroExtendRawFileOffset),
                "0F B7 C0"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Subtract four from EAX with DWORD wrap",
                LengthSubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(LengthSubtractRawFileOffset),
                "83 E8 04"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Push the computed length",
                LengthPushRawFileOffset,
                RawFileOffsetToVirtualAddress(LengthPushRawFileOffset),
                "50"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Independently read the DWORD at [EDI+8]",
                PayloadDwordAt8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordAt8ReadRawFileOffset),
                "8B 47 08"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Push the independently read DWORD at [EDI+4]",
                PayloadDwordAt4PushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordAt4PushRawFileOffset),
                "FF 77 04"),
            new ReliableReceiveLengthCallbackInstructionEvidence(
                "Call through the DWORD at the loaded [EDI+8] value plus four",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 50 04"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadAddress(uint payloadEdi, uint offset) =>
        unchecked(payloadEdi + offset);

    public static uint IndirectTargetSlotAddress(uint capturedPayloadDwordAt8) =>
        unchecked(capturedPayloadDwordAt8 + IndirectTargetOffset);

    public static ushort MaskHeader(ushort payloadHeader) =>
        (ushort)(payloadHeader & HeaderMask);

    public static uint ComputeLength(ushort payloadHeader) =>
        unchecked((uint)MaskHeader(payloadHeader) - 4u);

    public static ReliableReceiveLengthCallbackPlan Project(
        ReliableReceiveLengthCallbackCapture capture)
    {
        if (!capture.EntryPriorReturnEaxNonzero)
        {
            return Stop(
                capture: capture,
                outcome: ReliableReceiveLengthCallbackOutcome.EntryGateNotMet,
                unreachedHeader: capture.PayloadHeader.HasValue,
                unreachedPayloadDwordAt8: capture.PayloadDwordAt8.HasValue,
                unreachedPayloadDwordAt4: capture.PayloadDwordAt4.HasValue,
                unreachedTarget: capture.IndirectTargetAtDword8Plus4.HasValue,
                unreachedNormalReturn: capture.IndirectCallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint payloadHeaderAddress =
            PayloadAddress(capture.PayloadEdi, 0);
        if (capture.PayloadHeader is not ushort payloadHeader)
        {
            return Stop(
                capture: capture,
                outcome: HasAnyCapture(capture)
                    ? ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveLengthCallbackOutcome.MissingPayloadHeader,
                unreachedPayloadDwordAt8: capture.PayloadDwordAt8.HasValue,
                unreachedPayloadDwordAt4: capture.PayloadDwordAt4.HasValue,
                unreachedTarget: capture.IndirectTargetAtDword8Plus4.HasValue,
                unreachedNormalReturn: capture.IndirectCallReturnedNormally.HasValue,
                payloadHeaderAddress: payloadHeaderAddress,
                stopAt: HeaderReadRawFileOffset);
        }

        ushort maskedHeader = MaskHeader(payloadHeader);
        uint length = ComputeLength(payloadHeader);
        uint ecxArgumentAddress =
            PayloadAddress(capture.PayloadEdi, PayloadDwordAt8Offset);
        uint payloadDwordAt8Address = ecxArgumentAddress;
        if (capture.PayloadDwordAt8 is not uint payloadDwordAt8)
        {
            return Stop(
                capture: capture,
                outcome: HasAnyAfterPayloadDwordAt8(capture)
                    ? ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveLengthCallbackOutcome.MissingPayloadDwordAt8,
                unreachedPayloadDwordAt4: capture.PayloadDwordAt4.HasValue,
                unreachedTarget: capture.IndirectTargetAtDword8Plus4.HasValue,
                unreachedNormalReturn: capture.IndirectCallReturnedNormally.HasValue,
                payloadHeaderRead: true,
                payloadHeaderAddress: payloadHeaderAddress,
                maskedHeader: maskedHeader,
                computedLength: length,
                ecxArgumentAddress: ecxArgumentAddress,
                payloadDwordAt8Address: payloadDwordAt8Address,
                stackPushes: ImmutableArray.Create(length),
                stopAt: PayloadDwordAt8ReadRawFileOffset);
        }

        uint payloadDwordAt4Address =
            PayloadAddress(capture.PayloadEdi, PayloadDwordAt4Offset);
        if (capture.PayloadDwordAt4 is not uint payloadDwordAt4)
        {
            return Stop(
                capture: capture,
                outcome: HasAnyAfterPayloadDwordAt4(capture)
                    ? ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveLengthCallbackOutcome.MissingPayloadDwordAt4,
                unreachedTarget: capture.IndirectTargetAtDword8Plus4.HasValue,
                unreachedNormalReturn: capture.IndirectCallReturnedNormally.HasValue,
                payloadHeaderRead: true,
                payloadHeaderAddress: payloadHeaderAddress,
                maskedHeader: maskedHeader,
                computedLength: length,
                ecxArgumentAddress: ecxArgumentAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: payloadDwordAt8Address,
                payloadDwordAt4Address: payloadDwordAt4Address,
                stackPushes: ImmutableArray.Create(length),
                stopAt: PayloadDwordAt4PushRawFileOffset);
        }

        uint targetSlotAddress = IndirectTargetSlotAddress(payloadDwordAt8);
        ImmutableArray<uint> stackPushes =
            ImmutableArray.Create(length, payloadDwordAt4);
        ImmutableArray<uint> callStackArguments =
            ImmutableArray.Create(payloadDwordAt4, length);
        if (capture.IndirectTargetAtDword8Plus4 is not uint target)
        {
            return Stop(
                capture: capture,
                outcome: capture.IndirectCallReturnedNormally.HasValue
                    ? ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveLengthCallbackOutcome.MissingIndirectTarget,
                unreachedNormalReturn: capture.IndirectCallReturnedNormally.HasValue,
                payloadHeaderRead: true,
                payloadHeaderAddress: payloadHeaderAddress,
                maskedHeader: maskedHeader,
                computedLength: length,
                ecxArgumentAddress: ecxArgumentAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: payloadDwordAt8Address,
                payloadDwordAt4Read: true,
                payloadDwordAt4Address: payloadDwordAt4Address,
                indirectTargetSlotAddress: targetSlotAddress,
                stackPushes: stackPushes,
                callStackArguments: callStackArguments,
                stopAt: IndirectCallRawFileOffset);
        }

        if (capture.IndirectCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture: capture,
                outcome: ReliableReceiveLengthCallbackOutcome.MissingNormalReturn,
                payloadHeaderRead: true,
                payloadHeaderAddress: payloadHeaderAddress,
                maskedHeader: maskedHeader,
                computedLength: length,
                ecxArgumentAddress: ecxArgumentAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: payloadDwordAt8Address,
                payloadDwordAt4Read: true,
                payloadDwordAt4Address: payloadDwordAt4Address,
                indirectTargetRead: true,
                indirectTargetSlotAddress: targetSlotAddress,
                capturedIndirectTarget: target,
                stackPushes: stackPushes,
                callStackArguments: callStackArguments,
                indirectCallReached: true,
                stopAt: IndirectCallRawFileOffset);
        }

        if (!returnedNormally)
        {
            return Stop(
                capture: capture,
                outcome: ReliableReceiveLengthCallbackOutcome.IndirectCallDidNotReturnNormally,
                payloadHeaderRead: true,
                payloadHeaderAddress: payloadHeaderAddress,
                maskedHeader: maskedHeader,
                computedLength: length,
                ecxArgumentAddress: ecxArgumentAddress,
                payloadDwordAt8Read: true,
                payloadDwordAt8Address: payloadDwordAt8Address,
                payloadDwordAt4Read: true,
                payloadDwordAt4Address: payloadDwordAt4Address,
                indirectTargetRead: true,
                indirectTargetSlotAddress: targetSlotAddress,
                capturedIndirectTarget: target,
                stackPushes: stackPushes,
                callStackArguments: callStackArguments,
                indirectCallReached: true,
                indirectCallReturnedNormally: false,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture: capture,
            outcome: ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally,
            payloadHeaderRead: true,
            payloadHeaderAddress: payloadHeaderAddress,
            maskedHeader: maskedHeader,
            computedLength: length,
            ecxArgumentAddress: ecxArgumentAddress,
            payloadDwordAt8Read: true,
            payloadDwordAt8Address: payloadDwordAt8Address,
            payloadDwordAt4Read: true,
            payloadDwordAt4Address: payloadDwordAt4Address,
            indirectTargetRead: true,
            indirectTargetSlotAddress: targetSlotAddress,
            capturedIndirectTarget: target,
            stackPushes: stackPushes,
            callStackArguments: callStackArguments,
            indirectCallReached: true,
            indirectCallReturnedNormally: true,
            normalCallBoundaryReached: true,
            selectedDestinationRawFileOffset: NormalCallBoundaryRawFileOffset,
            stopAt: NormalCallBoundaryRawFileOffset);
    }

    private static bool HasAnyCapture(ReliableReceiveLengthCallbackCapture capture) =>
        capture.PayloadDwordAt8 is not null
        || capture.PayloadDwordAt4 is not null
        || capture.IndirectTargetAtDword8Plus4 is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static bool HasAnyAfterPayloadDwordAt8(
        ReliableReceiveLengthCallbackCapture capture) =>
        capture.PayloadDwordAt4 is not null
        || capture.IndirectTargetAtDword8Plus4 is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static bool HasAnyAfterPayloadDwordAt4(
        ReliableReceiveLengthCallbackCapture capture) =>
        capture.IndirectTargetAtDword8Plus4 is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static ReliableReceiveLengthCallbackPlan Stop(
        ReliableReceiveLengthCallbackCapture capture,
        ReliableReceiveLengthCallbackOutcome outcome,
        bool unreachedHeader = false,
        bool unreachedPayloadDwordAt8 = false,
        bool unreachedPayloadDwordAt4 = false,
        bool unreachedTarget = false,
        bool unreachedNormalReturn = false,
        bool payloadHeaderRead = false,
        uint? payloadHeaderAddress = null,
        ushort? maskedHeader = null,
        uint? computedLength = null,
        uint? ecxArgumentAddress = null,
        bool payloadDwordAt8Read = false,
        uint? payloadDwordAt8Address = null,
        bool payloadDwordAt4Read = false,
        uint? payloadDwordAt4Address = null,
        bool indirectTargetRead = false,
        uint? indirectTargetSlotAddress = null,
        uint? capturedIndirectTarget = null,
        ImmutableArray<uint> stackPushes = default,
        ImmutableArray<uint> callStackArguments = default,
        bool indirectCallReached = false,
        bool? indirectCallReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            unreachedHeader,
            unreachedPayloadDwordAt8,
            unreachedPayloadDwordAt4,
            unreachedTarget,
            unreachedNormalReturn,
            payloadHeaderRead,
            payloadHeaderAddress,
            maskedHeader,
            computedLength,
            ecxArgumentAddress,
            payloadDwordAt8Read,
            payloadDwordAt8Address,
            payloadDwordAt4Read,
            payloadDwordAt4Address,
            indirectTargetRead,
            indirectTargetSlotAddress,
            capturedIndirectTarget,
            stackPushes.IsDefault ? ImmutableArray<uint>.Empty : stackPushes,
            callStackArguments.IsDefault ? ImmutableArray<uint>.Empty : callStackArguments,
            indirectCallReached,
            indirectCallReturnedNormally,
            normalCallBoundaryReached,
            selectedDestinationRawFileOffset,
            stopAt);
}
