using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveJoinedHeaderCapture(
    uint PayloadBaseAddress,
    uint ReceiverBaseAddress,
    uint FrameBaseAddress,
    ushort? PayloadWordAtOffset2,
    ushort? PayloadHeader);

public enum UnreliableReceiveJoinedHeaderOutcome
{
    MissingPayloadWordAtOffset2,
    MissingPayloadWordAtOffset2WithUnreachedHeaderCapture,
    MissingPayloadHeader,
    ZeroBranchTaken,
    NonzeroBranchFallthrough
}

public readonly record struct UnreliableReceiveJoinedHeaderPlan(
    UnreliableReceiveJoinedHeaderCapture Capture,
    UnreliableReceiveJoinedHeaderOutcome Outcome,
    uint PayloadWordAtOffset2Address,
    uint ReceiverWordAtOffset1EAddress,
    uint PayloadHeaderAddress,
    uint EsiFrameLocalAddress,
    uint EcxFrameArgumentAddress,
    bool PayloadWordAtOffset2Read,
    bool ReceiverWordAtOffset1EWritten,
    bool UnreachedPayloadHeaderCaptureProvided,
    ushort? PayloadWordAtOffset2,
    bool PayloadHeaderRead,
    ushort? PayloadHeader,
    ushort? MaskedHeader,
    uint? SubtractedLength32,
    uint? EsiFrameLocalDword,
    uint? EcxFrameArgumentDword,
    bool? ZeroBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveJoinedHeaderInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the captured joined-header instructions through their conditional branch.
/// Captures describe observations only; this does not assert parser, ownership, or access guarantees.
/// </summary>
public static class UnreliableReceiveJoinedHeaderProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint StartRawFileOffset = 0x00BE5AF0;
    public const uint StartVirtualAddress = 0x00FE66F0;
    public const uint BranchRawFileOffset = 0x00BE5B14;
    public const uint BranchFallthroughRawFileOffset = 0x00BE5B16;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE5B32;
    public const uint EndRawFileOffsetExclusive = BranchFallthroughRawFileOffset;
    public const uint EndVirtualAddressExclusive = 0x00FE6716;

    public const uint PayloadWordAtOffset2ReadRawFileOffset = 0x00BE5AF0;
    public const uint ReceiverWordAtOffset1EWriteRawFileOffset = 0x00BE5AF4;
    public const uint PayloadHeaderReadRawFileOffset = 0x00BE5AF8;
    public const uint LengthMaskRawFileOffset = 0x00BE5B00;
    public const uint LengthSubtractRawFileOffset = 0x00BE5B05;
    public const uint BranchSelectorMaskRawFileOffset = 0x00BE5B08;
    public const uint EsiZeroExtendRawFileOffset = 0x00BE5B0B;
    public const uint EsiFrameLocalWriteRawFileOffset = 0x00BE5B0E;
    public const uint EcxFrameArgumentWriteRawFileOffset = 0x00BE5B11;

    public const uint PayloadWordAtOffset2 = 2;
    public const uint ReceiverWordAtOffset1E = 0x1E;
    public const uint EsiFrameLocalOffset = 0xFFFFFFFC;
    public const uint EcxFrameArgumentOffset = 8;
    public const ushort HeaderLengthMask = 0x07FF;
    public const ushort BranchSelectorMask = 3;

    public const string CapturedInstructionBytes =
        "66 8B 43 02 66 89 47 1E 0F B7 0B 8B C1 C1 E9 0C "
        + "25 FF 07 00 00 83 E8 04 80 E1 03 0F B7 F0 89 75 FC "
        + "89 4D 08 74 1C";

    public static ImmutableArray<UnreliableReceiveJoinedHeaderInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Read the WORD at [EBX+2]",
                PayloadWordAtOffset2ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadWordAtOffset2ReadRawFileOffset),
                "66 8B 43 02"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Write that WORD to [EDI+0x1E]",
                ReceiverWordAtOffset1EWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverWordAtOffset1EWriteRawFileOffset),
                "66 89 47 1E"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Independently read the WORD at [EBX]",
                PayloadHeaderReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadHeaderReadRawFileOffset),
                "0F B7 0B"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Copy the header to EAX, then shift ECX right by twelve",
                0x00BE5AFB,
                RawFileOffsetToVirtualAddress(0x00BE5AFB),
                "8B C1 C1 E9 0C"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Mask EAX with 0x7FF and subtract four with DWORD wrap",
                LengthMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(LengthMaskRawFileOffset),
                "25 FF 07 00 00 83 E8 04"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Mask CL with three; the resulting ECX is the branch selector",
                BranchSelectorMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(BranchSelectorMaskRawFileOffset),
                "80 E1 03"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Zero-extend AX into ESI, then store ESI at [EBP-4]",
                EsiZeroExtendRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiZeroExtendRawFileOffset),
                "0F B7 F0 89 75 FC"),
            new UnreliableReceiveJoinedHeaderInstructionEvidence(
                "Store ECX at [EBP+8], then JZ to raw 0xBE5B32",
                EcxFrameArgumentWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxFrameArgumentWriteRawFileOffset),
                "89 4D 08 74 1C",
                ZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchTargetRawFileOffset)));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadAddress(uint payloadBaseAddress, uint offset) =>
        unchecked(payloadBaseAddress + offset);

    public static uint ReceiverWordAtOffset1EAddress(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverWordAtOffset1E);

    public static uint EsiFrameLocalAddress(uint frameBaseAddress) =>
        unchecked(frameBaseAddress + EsiFrameLocalOffset);

    public static uint EcxFrameArgumentAddress(uint frameBaseAddress) =>
        unchecked(frameBaseAddress + EcxFrameArgumentOffset);

    public static ushort MaskHeaderLength(ushort payloadHeader) =>
        (ushort)(payloadHeader & HeaderLengthMask);

    public static uint SubtractHeaderLength(ushort payloadHeader) =>
        unchecked((uint)MaskHeaderLength(payloadHeader) - 4u);

    public static uint EsiValue(ushort payloadHeader) =>
        unchecked((ushort)SubtractHeaderLength(payloadHeader));

    public static uint BranchSelector(ushort payloadHeader) =>
        ((uint)payloadHeader >> 12) & BranchSelectorMask;

    public static UnreliableReceiveJoinedHeaderPlan Project(
        UnreliableReceiveJoinedHeaderCapture capture)
    {
        uint payloadWordAddress =
            PayloadAddress(capture.PayloadBaseAddress, PayloadWordAtOffset2);
        uint receiverWordAddress =
            ReceiverWordAtOffset1EAddress(capture.ReceiverBaseAddress);
        uint payloadHeaderAddress =
            PayloadAddress(capture.PayloadBaseAddress, 0);
        uint esiFrameLocalAddress = EsiFrameLocalAddress(capture.FrameBaseAddress);
        uint ecxFrameArgumentAddress = EcxFrameArgumentAddress(capture.FrameBaseAddress);

        if (capture.PayloadWordAtOffset2 is not ushort payloadWord)
        {
            return new(
                capture,
                capture.PayloadHeader.HasValue
                    ? UnreliableReceiveJoinedHeaderOutcome.MissingPayloadWordAtOffset2WithUnreachedHeaderCapture
                    : UnreliableReceiveJoinedHeaderOutcome.MissingPayloadWordAtOffset2,
                payloadWordAddress,
                receiverWordAddress,
                payloadHeaderAddress,
                esiFrameLocalAddress,
                ecxFrameArgumentAddress,
                PayloadWordAtOffset2Read: false,
                ReceiverWordAtOffset1EWritten: false,
                UnreachedPayloadHeaderCaptureProvided: capture.PayloadHeader.HasValue,
                PayloadWordAtOffset2: null,
                PayloadHeaderRead: false,
                PayloadHeader: null,
                MaskedHeader: null,
                SubtractedLength32: null,
                EsiFrameLocalDword: null,
                EcxFrameArgumentDword: null,
                ZeroBranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: PayloadWordAtOffset2ReadRawFileOffset);
        }

        if (capture.PayloadHeader is not ushort payloadHeader)
        {
            return new(
                capture,
                UnreliableReceiveJoinedHeaderOutcome.MissingPayloadHeader,
                payloadWordAddress,
                receiverWordAddress,
                payloadHeaderAddress,
                esiFrameLocalAddress,
                ecxFrameArgumentAddress,
                PayloadWordAtOffset2Read: true,
                ReceiverWordAtOffset1EWritten: true,
                UnreachedPayloadHeaderCaptureProvided: false,
                payloadWord,
                PayloadHeaderRead: false,
                PayloadHeader: null,
                MaskedHeader: null,
                SubtractedLength32: null,
                EsiFrameLocalDword: null,
                EcxFrameArgumentDword: null,
                ZeroBranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: PayloadHeaderReadRawFileOffset);
        }

        ushort maskedHeader = MaskHeaderLength(payloadHeader);
        uint subtractedLength32 = SubtractHeaderLength(payloadHeader);
        uint esiValue = EsiValue(payloadHeader);
        uint ecxValue = BranchSelector(payloadHeader);
        bool zeroBranchTaken = ecxValue == 0;
        uint destination = zeroBranchTaken
            ? ZeroBranchTargetRawFileOffset
            : BranchFallthroughRawFileOffset;

        return new(
            capture,
            zeroBranchTaken
                ? UnreliableReceiveJoinedHeaderOutcome.ZeroBranchTaken
                : UnreliableReceiveJoinedHeaderOutcome.NonzeroBranchFallthrough,
            payloadWordAddress,
            receiverWordAddress,
            payloadHeaderAddress,
            esiFrameLocalAddress,
            ecxFrameArgumentAddress,
            PayloadWordAtOffset2Read: true,
            ReceiverWordAtOffset1EWritten: true,
            UnreachedPayloadHeaderCaptureProvided: false,
            payloadWord,
            PayloadHeaderRead: true,
            payloadHeader,
            maskedHeader,
            subtractedLength32,
            esiValue,
            ecxValue,
            zeroBranchTaken,
            destination,
            destination);
    }
}
