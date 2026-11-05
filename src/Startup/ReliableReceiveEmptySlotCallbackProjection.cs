using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveEmptySlotCallbackCapture(
    uint PayloadEdi,
    ushort? PayloadHeader,
    uint? PayloadDwordAt8,
    uint? IndirectTargetAtDword8PlusC,
    bool? IndirectCallReturnedNormally);

public enum ReliableReceiveEmptySlotCallbackOutcome
{
    MissingPayloadHeader,
    UnreachedCaptureProvided,
    MissingPayloadDwordAt8,
    MissingIndirectTarget,
    MissingNormalReturn,
    IndirectCallDidNotReturnNormally,
    HeaderBranchTaken,
    IndirectCallReturnedNormally
}

public readonly record struct ReliableReceiveEmptySlotCallbackPlan(
    ReliableReceiveEmptySlotCallbackCapture Capture,
    ReliableReceiveEmptySlotCallbackOutcome Outcome,
    byte? HeaderSelectorBits,
    uint PayloadHeaderAddress,
    uint? PayloadDwordAt8Address,
    uint? EcxArgumentAddress,
    uint? IndirectTargetSlotAddress,
    uint? CapturedIndirectTarget,
    byte ExplicitStackPushCount,
    bool IndirectCallReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveEmptySlotCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the exact empty-slot callback slice, without assigning parser,
/// record, or ownership semantics to its captured values.
/// </summary>
public static class ReliableReceiveEmptySlotCallbackProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE538D;
    public const uint HeaderReadRawFileOffset = 0x00BE538D;
    public const uint HeaderShiftRawFileOffset = 0x00BE5390;
    public const uint HeaderMaskRawFileOffset = 0x00BE5394;
    public const uint BranchRawFileOffset = 0x00BE5397;
    public const uint PayloadDwordReadRawFileOffset = 0x00BE5399;
    public const uint EcxArgumentAddressRawFileOffset = 0x00BE539C;
    public const uint IndirectCallRawFileOffset = 0x00BE539F;
    public const uint NormalReturnRawFileOffset = 0x00BE53A2;
    public const uint EndRawFileOffsetExclusive = 0x00BE53A2;
    public const uint HeaderBranchTargetRawFileOffset = 0x00BE53E9;

    public const uint PayloadHeaderOffset = 0;
    public const uint PayloadDwordOffset = 8;
    public const uint IndirectTargetSlotOffset = 0x0C;
    public const byte HeaderSelectorShift = 12;
    public const byte HeaderSelectorMask = 3;
    public const byte HeaderBranchDisplacement = 0x50;
    public const byte ExplicitStackPushCount = 0;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint NormalReturnVirtualAddress =
        NormalReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HeaderBranchTargetVirtualAddress =
        HeaderBranchTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedCallbackBytes =
        "66 8B 1F 66 C1 EB 0C 80 E3 03 74 50 8B 47 08 8D 4F 08 FF 50 0C";

    public static ImmutableArray<ReliableReceiveEmptySlotCallbackInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "Load the WORD at [EDI] into BX",
                HeaderReadRawFileOffset,
                RawFileOffsetToVirtualAddress(HeaderReadRawFileOffset),
                "66 8B 1F"),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "Shift BX right by 12 bits",
                HeaderShiftRawFileOffset,
                RawFileOffsetToVirtualAddress(HeaderShiftRawFileOffset),
                "66 C1 EB 0C"),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "Mask BL to its low two bits",
                HeaderMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(HeaderMaskRawFileOffset),
                "80 E3 03"),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "A zero selector branches to raw 0xBE53E9",
                BranchRawFileOffset,
                RawFileOffsetToVirtualAddress(BranchRawFileOffset),
                "74 50",
                HeaderBranchTargetRawFileOffset,
                HeaderBranchTargetVirtualAddress),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "On the nonzero-selector path, load the DWORD at [EDI+8]",
                PayloadDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordReadRawFileOffset),
                "8B 47 08"),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "Set ECX to EDI+8, not to the DWORD loaded from that address",
                EcxArgumentAddressRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxArgumentAddressRawFileOffset),
                "8D 4F 08"),
            new ReliableReceiveEmptySlotCallbackInstructionEvidence(
                "Call through the captured DWORD's slot at offset 0xC, with no explicit stack push",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 50 0C"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadAddress(uint payloadEdi, uint offset) =>
        unchecked(payloadEdi + offset);

    public static uint IndirectTargetSlotAddress(uint capturedPayloadDwordAt8) =>
        unchecked(capturedPayloadDwordAt8 + IndirectTargetSlotOffset);

    public static ReliableReceiveEmptySlotCallbackPlan Project(
        ReliableReceiveEmptySlotCallbackCapture capture)
    {
        uint payloadHeaderAddress =
            PayloadAddress(capture.PayloadEdi, PayloadHeaderOffset);

        if (capture.PayloadHeader is not ushort payloadHeader)
        {
            return Stop(
                capture,
                HasAnyAfterHeader(capture)
                    ? ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveEmptySlotCallbackOutcome.MissingPayloadHeader,
                payloadHeaderAddress,
                stopAt: HeaderReadRawFileOffset);
        }

        byte selectorBits = (byte)((payloadHeader >> HeaderSelectorShift) & HeaderSelectorMask);
        if (selectorBits == 0)
        {
            return Stop(
                capture,
                HasAnyAfterHeader(capture)
                    ? ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken,
                payloadHeaderAddress,
                headerSelectorBits: selectorBits,
                branchTaken: true,
                selectedDestinationRawFileOffset: HeaderBranchTargetRawFileOffset,
                stopAt: HeaderBranchTargetRawFileOffset);
        }

        uint payloadDwordAt8Address =
            PayloadAddress(capture.PayloadEdi, PayloadDwordOffset);
        if (capture.PayloadDwordAt8 is not uint capturedPayloadDwordAt8)
        {
            return Stop(
                capture,
                HasAnyAfterPayloadDword(capture)
                    ? ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveEmptySlotCallbackOutcome.MissingPayloadDwordAt8,
                payloadHeaderAddress,
                headerSelectorBits: selectorBits,
                payloadDwordAt8Address: payloadDwordAt8Address,
                branchTaken: false,
                stopAt: PayloadDwordReadRawFileOffset);
        }

        uint indirectTargetSlotAddress =
            IndirectTargetSlotAddress(capturedPayloadDwordAt8);
        if (capture.IndirectTargetAtDword8PlusC is not uint capturedIndirectTarget)
        {
            return Stop(
                capture,
                capture.IndirectCallReturnedNormally is not null
                    ? ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
                    : ReliableReceiveEmptySlotCallbackOutcome.MissingIndirectTarget,
                payloadHeaderAddress,
                headerSelectorBits: selectorBits,
                payloadDwordAt8Address: payloadDwordAt8Address,
                ecxArgumentAddress: payloadDwordAt8Address,
                indirectTargetSlotAddress: indirectTargetSlotAddress,
                branchTaken: false,
                stopAt: IndirectCallRawFileOffset);
        }

        if (capture.IndirectCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveEmptySlotCallbackOutcome.MissingNormalReturn,
                payloadHeaderAddress,
                headerSelectorBits: selectorBits,
                payloadDwordAt8Address: payloadDwordAt8Address,
                ecxArgumentAddress: payloadDwordAt8Address,
                indirectTargetSlotAddress: indirectTargetSlotAddress,
                capturedIndirectTarget: capturedIndirectTarget,
                indirectCallReached: true,
                branchTaken: false,
                stopAt: IndirectCallRawFileOffset);
        }

        if (!returnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveEmptySlotCallbackOutcome.IndirectCallDidNotReturnNormally,
                payloadHeaderAddress,
                headerSelectorBits: selectorBits,
                payloadDwordAt8Address: payloadDwordAt8Address,
                ecxArgumentAddress: payloadDwordAt8Address,
                indirectTargetSlotAddress: indirectTargetSlotAddress,
                capturedIndirectTarget: capturedIndirectTarget,
                indirectCallReached: true,
                branchTaken: false,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            ReliableReceiveEmptySlotCallbackOutcome.IndirectCallReturnedNormally,
            payloadHeaderAddress,
            headerSelectorBits: selectorBits,
            payloadDwordAt8Address: payloadDwordAt8Address,
            ecxArgumentAddress: payloadDwordAt8Address,
            indirectTargetSlotAddress: indirectTargetSlotAddress,
            capturedIndirectTarget: capturedIndirectTarget,
            indirectCallReached: true,
            branchTaken: false,
            selectedDestinationRawFileOffset: NormalReturnRawFileOffset,
            stopAt: NormalReturnRawFileOffset);
    }

    private static bool HasAnyAfterHeader(
        ReliableReceiveEmptySlotCallbackCapture capture) =>
        capture.PayloadDwordAt8 is not null
        || capture.IndirectTargetAtDword8PlusC is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static bool HasAnyAfterPayloadDword(
        ReliableReceiveEmptySlotCallbackCapture capture) =>
        capture.IndirectTargetAtDword8PlusC is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static ReliableReceiveEmptySlotCallbackPlan Stop(
        ReliableReceiveEmptySlotCallbackCapture capture,
        ReliableReceiveEmptySlotCallbackOutcome outcome,
        uint payloadHeaderAddress,
        byte? headerSelectorBits = null,
        uint? payloadDwordAt8Address = null,
        uint? ecxArgumentAddress = null,
        uint? indirectTargetSlotAddress = null,
        uint? capturedIndirectTarget = null,
        bool indirectCallReached = false,
        bool? branchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            headerSelectorBits,
            payloadHeaderAddress,
            payloadDwordAt8Address,
            ecxArgumentAddress,
            indirectTargetSlotAddress,
            capturedIndirectTarget,
            ExplicitStackPushCount,
            indirectCallReached,
            branchTaken,
            selectedDestinationRawFileOffset,
            stopAt);
}
