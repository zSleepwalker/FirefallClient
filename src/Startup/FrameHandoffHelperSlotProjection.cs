using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffHelperSlotCapture(
    uint ReceiverBaseEsi,
    uint? ReceiverPlus08,
    uint? ReceiverPlus10,
    uint? ReceiverPlus0COld,
    uint? ReceiverPlus0CFresh,
    uint? ReceiverPlus04,
    uint? FirstSlotDword);

public enum FrameHandoffHelperSlotOutcome
{
    MissingReceiverPlus08Capture,
    MissingReceiverPlus10Capture,
    MissingReceiverPlus0COldCapture,
    MissingReceiverPlus0CFreshCapture,
    MissingReceiverPlus04Capture,
    MissingFirstSlotDwordCapture,
    FirstSlotNonzeroBranches,
    FirstSlotZeroFallsThrough
}

public readonly record struct FrameHandoffHelperSlotPlan(
    FrameHandoffHelperSlotCapture Capture,
    FrameHandoffHelperSlotOutcome Outcome,
    uint? ReceiverPlus08ReadAddress,
    uint? ReceiverPlus08ReadValue,
    bool ReceiverPlus08ReadReached,
    bool ReceiverPlus08Captured,
    bool EbxPushReached,
    bool EdiPushReached,
    uint? ReceiverPlus10ReadAddress,
    uint? ReceiverPlus10ReadValue,
    bool ReceiverPlus10ReadReached,
    bool ReceiverPlus10Captured,
    uint? MaskFromReceiverPlus08,
    uint? DecrementedReceiverPlus08,
    uint? ReceiverPlus0COldReadAddress,
    uint? ReceiverPlus0COldReadValue,
    bool ReceiverPlus0COldReadReached,
    bool ReceiverPlus0COldCaptured,
    bool ReceiverPlus0CWriteReached,
    uint? ReceiverPlus0CWriteValue,
    bool UnreachedReceiverPlus0COldCaptureProvided,
    uint? ReceiverPlus0CFreshReadAddress,
    uint? ReceiverPlus0CFreshReadValue,
    bool ReceiverPlus0CFreshReadReached,
    bool ReceiverPlus0CFreshCaptured,
    uint? WrappingEdiPlusReceiverPlus0CFresh,
    uint? ShiftedEdiPlusReceiverPlus0CFresh,
    uint? FirstSlotIndex,
    uint? ReceiverPlus04ReadAddress,
    uint? ReceiverPlus04ReadValue,
    bool ReceiverPlus04ReadReached,
    bool ReceiverPlus04Captured,
    uint? FirstSlotByteOffset,
    uint? FirstSlotAddress,
    uint? FirstSlotDwordReadValue,
    bool FirstSlotDwordReadReached,
    bool FirstSlotDwordCaptured,
    bool FirstSlotZeroTestProjected,
    bool? FirstSlotNonzero,
    uint? SelectedDestinationRawFileOffset,
    bool UnreachedReceiverPlus10CaptureProvided,
    bool UnreachedReceiverPlus0CFreshCaptureProvided,
    bool UnreachedReceiverPlus04CaptureProvided,
    bool UnreachedFirstSlotDwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffHelperSlotInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the helper continuation through the first slot's zero/nonzero test.
/// </summary>
public static class FrameHandoffHelperSlotProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE64C5;
    public const uint EntryVirtualAddress = 0x00FE70C5;
    public const uint ReceiverPlus08ReadRawFileOffset = 0x00BE64C5;
    public const uint ReceiverPlus10ReadRawFileOffset = 0x00BE64CA;
    public const uint ReceiverPlus0COldReadWriteRawFileOffset = 0x00BE64D5;
    public const uint ReceiverPlus0CFreshReadRawFileOffset = 0x00BE64D8;
    public const uint ReceiverPlus04ReadRawFileOffset = 0x00BE64E2;
    public const uint FirstSlotAddressFormationRawFileOffset = 0x00BE64E5;
    public const uint FirstSlotTestRawFileOffset = 0x00BE64EC;
    public const uint FirstSlotBranchRawFileOffset = 0x00BE64F0;
    public const uint FirstSlotNonzeroBranchDestinationRawFileOffset = 0x00BE650E;
    public const uint FirstSlotZeroFallthroughRawFileOffset = 0x00BE64F2;
    public const uint EndRawFileOffsetExclusive = FirstSlotZeroFallthroughRawFileOffset;

    public const uint ReceiverPlus04Offset = 0x04;
    public const uint ReceiverPlus08Offset = 0x08;
    public const uint ReceiverPlus0COffset = 0x0C;
    public const uint ReceiverPlus10Offset = 0x10;
    public const uint SlotScale = 4;
    public const int EdiRightShift = 2;

    public const string CapturedWindowBytes =
        "8B 4E 08 53 57 8B 7E 10 8D 04 8D FF FF FF FF 49 21 46 0C "
        + "03 7E 0C 8B C7 C1 E8 02 23 C8 8B 46 04 8D 1C 8D 00 00 00 00 "
        + "83 3C 03 00 75 1C";

    public static ImmutableArray<FrameHandoffHelperSlotInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffHelperSlotInstructionEvidence(
                "Read the helper receiver's +8 DWORD into ECX",
                ReceiverPlus08ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus08ReadRawFileOffset),
                "8B 4E 08"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Save EBX",
                0x00BE64C8,
                RawFileOffsetToVirtualAddress(0x00BE64C8),
                "53"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Save the incoming EDI before replacing it",
                0x00BE64C9,
                RawFileOffsetToVirtualAddress(0x00BE64C9),
                "57"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Independently read the helper receiver's +0x10 DWORD into EDI",
                ReceiverPlus10ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus10ReadRawFileOffset),
                "8B 7E 10"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Form wrapping 4*ECX-1 as the receiver+0x0C mask",
                0x00BE64CD,
                RawFileOffsetToVirtualAddress(0x00BE64CD),
                "8D 04 8D FF FF FF FF"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Decrement ECX with DWORD wrap",
                0x00BE64D4,
                RawFileOffsetToVirtualAddress(0x00BE64D4),
                "49"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Read old [receiver+0x0C], AND it with the mask, and write it back",
                ReceiverPlus0COldReadWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus0COldReadWriteRawFileOffset),
                "21 46 0C"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Independently reload [receiver+0x0C] and add that fresh DWORD to EDI",
                ReceiverPlus0CFreshReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus0CFreshReadRawFileOffset),
                "03 7E 0C"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Copy EDI to EAX",
                0x00BE64DB,
                RawFileOffsetToVirtualAddress(0x00BE64DB),
                "8B C7"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Logically shift EAX right by two",
                0x00BE64DD,
                RawFileOffsetToVirtualAddress(0x00BE64DD),
                "C1 E8 02"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "AND shifted EAX with decremented ECX to derive the slot index",
                0x00BE64E0,
                RawFileOffsetToVirtualAddress(0x00BE64E0),
                "23 C8"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Independently read [receiver+4] as the first-slot base into EAX",
                ReceiverPlus04ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus04ReadRawFileOffset),
                "8B 46 04"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Form the wrapping index*4 slot byte offset in EBX",
                FirstSlotAddressFormationRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstSlotAddressFormationRawFileOffset),
                "8D 1C 8D 00 00 00 00"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "Test DWORD [receiver+4 base + index*4] against zero",
                FirstSlotTestRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstSlotTestRawFileOffset),
                "83 3C 03 00"),
            new FrameHandoffHelperSlotInstructionEvidence(
                "A nonzero first-slot DWORD branches to raw 0xBE650E; zero falls through to raw 0xBE64F2",
                FirstSlotBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstSlotBranchRawFileOffset),
                "75 1C",
                FirstSlotNonzeroBranchDestinationRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstSlotNonzeroBranchDestinationRawFileOffset)));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverAddress(uint receiverBaseAddress, uint fieldOffset) =>
        unchecked(receiverBaseAddress + fieldOffset);

    public static uint FirstSlotByteOffset(uint slotIndex) =>
        unchecked(slotIndex * SlotScale);

    public static uint FirstSlotAddress(uint receiverPlus04Base, uint slotIndex) =>
        unchecked(receiverPlus04Base + FirstSlotByteOffset(slotIndex));

    public static uint ComputeReceiverPlus08Mask(uint receiverPlus08) =>
        unchecked((receiverPlus08 * SlotScale) - 1);

    public static FrameHandoffHelperSlotPlan Project(
        FrameHandoffHelperSlotCapture capture)
    {
        if (capture.ReceiverPlus08 is not uint receiverPlus08)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingReceiverPlus08Capture,
                receiverPlus08ReadReached: true,
                unreachedReceiverPlus10CaptureProvided: capture.ReceiverPlus10.HasValue,
                unreachedReceiverPlus0COldCaptureProvided: capture.ReceiverPlus0COld.HasValue,
                unreachedReceiverPlus0CFreshCaptureProvided: capture.ReceiverPlus0CFresh.HasValue,
                unreachedReceiverPlus04CaptureProvided: capture.ReceiverPlus04.HasValue,
                unreachedFirstSlotDwordCaptureProvided: capture.FirstSlotDword.HasValue,
                stopAt: ReceiverPlus08ReadRawFileOffset);
        }

        if (capture.ReceiverPlus10 is not uint receiverPlus10)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingReceiverPlus10Capture,
                receiverPlus08,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                ebxPushReached: true,
                ediPushReached: true,
                receiverPlus10ReadReached: true,
                unreachedReceiverPlus0COldCaptureProvided: capture.ReceiverPlus0COld.HasValue,
                unreachedReceiverPlus0CFreshCaptureProvided: capture.ReceiverPlus0CFresh.HasValue,
                unreachedReceiverPlus04CaptureProvided: capture.ReceiverPlus04.HasValue,
                unreachedFirstSlotDwordCaptureProvided: capture.FirstSlotDword.HasValue,
                stopAt: ReceiverPlus10ReadRawFileOffset);
        }

        uint mask = ComputeReceiverPlus08Mask(receiverPlus08);
        uint decrementedReceiverPlus08 = unchecked(receiverPlus08 - 1);
        if (capture.ReceiverPlus0COld is not uint receiverPlus0COld)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingReceiverPlus0COldCapture,
                receiverPlus08,
                receiverPlus10,
                mask,
                decrementedReceiverPlus08,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                ebxPushReached: true,
                ediPushReached: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus0COldReadReached: true,
                unreachedReceiverPlus0COldCaptureProvided: false,
                unreachedReceiverPlus0CFreshCaptureProvided: capture.ReceiverPlus0CFresh.HasValue,
                unreachedReceiverPlus04CaptureProvided: capture.ReceiverPlus04.HasValue,
                unreachedFirstSlotDwordCaptureProvided: capture.FirstSlotDword.HasValue,
                stopAt: ReceiverPlus0COldReadWriteRawFileOffset);
        }

        uint receiverPlus0CWriteValue = receiverPlus0COld & mask;
        if (capture.ReceiverPlus0CFresh is not uint receiverPlus0CFresh)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingReceiverPlus0CFreshCapture,
                receiverPlus08,
                receiverPlus10,
                mask,
                decrementedReceiverPlus08,
                receiverPlus0COld,
                receiverPlus0CWriteValue,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                ebxPushReached: true,
                ediPushReached: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus0COldReadReached: true,
                receiverPlus0COldCaptured: true,
                receiverPlus0CWriteReached: true,
                receiverPlus0CFreshReadReached: true,
                unreachedReceiverPlus0COldCaptureProvided: false,
                unreachedReceiverPlus04CaptureProvided: capture.ReceiverPlus04.HasValue,
                unreachedFirstSlotDwordCaptureProvided: capture.FirstSlotDword.HasValue,
                stopAt: ReceiverPlus0CFreshReadRawFileOffset);
        }

        uint wrappingEdiPlusReceiverPlus0CFresh =
            unchecked(receiverPlus10 + receiverPlus0CFresh);
        uint shiftedEdiPlusReceiverPlus0CFresh =
            wrappingEdiPlusReceiverPlus0CFresh >> EdiRightShift;
        uint firstSlotIndex =
            decrementedReceiverPlus08 & shiftedEdiPlusReceiverPlus0CFresh;
        if (capture.ReceiverPlus04 is not uint receiverPlus04)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingReceiverPlus04Capture,
                receiverPlus08,
                receiverPlus10,
                mask,
                decrementedReceiverPlus08,
                receiverPlus0COld,
                receiverPlus0CWriteValue,
                receiverPlus0CFresh,
                wrappingEdiPlusReceiverPlus0CFresh,
                shiftedEdiPlusReceiverPlus0CFresh,
                firstSlotIndex,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                ebxPushReached: true,
                ediPushReached: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus0COldReadReached: true,
                receiverPlus0COldCaptured: true,
                receiverPlus0CWriteReached: true,
                receiverPlus0CFreshReadReached: true,
                receiverPlus0CFreshCaptured: true,
                receiverPlus04ReadReached: true,
                unreachedReceiverPlus0COldCaptureProvided: false,
                unreachedReceiverPlus04CaptureProvided: false,
                unreachedFirstSlotDwordCaptureProvided: capture.FirstSlotDword.HasValue,
                stopAt: ReceiverPlus04ReadRawFileOffset);
        }

        uint firstSlotByteOffset = FirstSlotByteOffset(firstSlotIndex);
        uint firstSlotAddress = unchecked(receiverPlus04 + firstSlotByteOffset);
        if (capture.FirstSlotDword is not uint firstSlotDword)
        {
            return Stop(
                capture,
                FrameHandoffHelperSlotOutcome.MissingFirstSlotDwordCapture,
                receiverPlus08,
                receiverPlus10,
                mask,
                decrementedReceiverPlus08,
                receiverPlus0COld,
                receiverPlus0CWriteValue,
                receiverPlus0CFresh,
                wrappingEdiPlusReceiverPlus0CFresh,
                shiftedEdiPlusReceiverPlus0CFresh,
                firstSlotIndex,
                receiverPlus04,
                firstSlotByteOffset,
                firstSlotAddress,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                ebxPushReached: true,
                ediPushReached: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus0COldReadReached: true,
                receiverPlus0COldCaptured: true,
                receiverPlus0CWriteReached: true,
                receiverPlus0CFreshReadReached: true,
                receiverPlus0CFreshCaptured: true,
                receiverPlus04ReadReached: true,
                receiverPlus04Captured: true,
                firstSlotDwordReadReached: true,
                unreachedReceiverPlus0COldCaptureProvided: false,
                unreachedReceiverPlus04CaptureProvided: false,
                unreachedFirstSlotDwordCaptureProvided: false,
                stopAt: FirstSlotTestRawFileOffset);
        }

        bool firstSlotNonzero = firstSlotDword != 0;
        return Stop(
            capture,
            firstSlotNonzero
                ? FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches
                : FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough,
            receiverPlus08,
            receiverPlus10,
            mask,
            decrementedReceiverPlus08,
            receiverPlus0COld,
            receiverPlus0CWriteValue,
            receiverPlus0CFresh,
            wrappingEdiPlusReceiverPlus0CFresh,
            shiftedEdiPlusReceiverPlus0CFresh,
            firstSlotIndex,
            receiverPlus04,
            firstSlotByteOffset,
            firstSlotAddress,
            firstSlotDword,
            receiverPlus08ReadReached: true,
            receiverPlus08Captured: true,
            ebxPushReached: true,
            ediPushReached: true,
            receiverPlus10ReadReached: true,
            receiverPlus10Captured: true,
            receiverPlus0COldReadReached: true,
            receiverPlus0COldCaptured: true,
            receiverPlus0CWriteReached: true,
            receiverPlus0CFreshReadReached: true,
            receiverPlus0CFreshCaptured: true,
            receiverPlus04ReadReached: true,
            receiverPlus04Captured: true,
            firstSlotDwordReadReached: true,
            firstSlotDwordCaptured: true,
            firstSlotZeroTestProjected: true,
            firstSlotNonzero: firstSlotNonzero,
            selectedDestinationRawFileOffset: firstSlotNonzero
                ? FirstSlotNonzeroBranchDestinationRawFileOffset
                : FirstSlotZeroFallthroughRawFileOffset,
            unreachedReceiverPlus0COldCaptureProvided: false,
            unreachedReceiverPlus0CFreshCaptureProvided: false,
            unreachedReceiverPlus04CaptureProvided: false,
            unreachedFirstSlotDwordCaptureProvided: false,
            stopAt: firstSlotNonzero
                ? FirstSlotNonzeroBranchDestinationRawFileOffset
                : FirstSlotZeroFallthroughRawFileOffset);
    }

    private static FrameHandoffHelperSlotPlan Stop(
        FrameHandoffHelperSlotCapture capture,
        FrameHandoffHelperSlotOutcome outcome,
        uint? receiverPlus08 = null,
        uint? receiverPlus10 = null,
        uint? maskFromReceiverPlus08 = null,
        uint? decrementedReceiverPlus08 = null,
        uint? receiverPlus0COld = null,
        uint? receiverPlus0CWriteValue = null,
        uint? receiverPlus0CFresh = null,
        uint? wrappingEdiPlusReceiverPlus0CFresh = null,
        uint? shiftedEdiPlusReceiverPlus0CFresh = null,
        uint? firstSlotIndex = null,
        uint? receiverPlus04 = null,
        uint? firstSlotByteOffset = null,
        uint? firstSlotAddress = null,
        uint? firstSlotDword = null,
        bool receiverPlus08ReadReached = false,
        bool receiverPlus08Captured = false,
        bool ebxPushReached = false,
        bool ediPushReached = false,
        bool receiverPlus10ReadReached = false,
        bool receiverPlus10Captured = false,
        bool receiverPlus0COldReadReached = false,
        bool receiverPlus0COldCaptured = false,
        bool receiverPlus0CWriteReached = false,
        bool receiverPlus0CFreshReadReached = false,
        bool receiverPlus0CFreshCaptured = false,
        bool receiverPlus04ReadReached = false,
        bool receiverPlus04Captured = false,
        bool firstSlotDwordReadReached = false,
        bool firstSlotDwordCaptured = false,
        bool firstSlotZeroTestProjected = false,
        bool? firstSlotNonzero = null,
        uint? selectedDestinationRawFileOffset = null,
        bool unreachedReceiverPlus10CaptureProvided = false,
        bool unreachedReceiverPlus0COldCaptureProvided = false,
        bool unreachedReceiverPlus0CFreshCaptureProvided = false,
        bool unreachedReceiverPlus04CaptureProvided = false,
        bool unreachedFirstSlotDwordCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverPlus08ReadReached
                ? ReceiverAddress(capture.ReceiverBaseEsi, ReceiverPlus08Offset)
                : null,
            receiverPlus08Captured ? receiverPlus08 : null,
            receiverPlus08ReadReached,
            receiverPlus08Captured,
            ebxPushReached,
            ediPushReached,
            receiverPlus10ReadReached
                ? ReceiverAddress(capture.ReceiverBaseEsi, ReceiverPlus10Offset)
                : null,
            receiverPlus10Captured ? receiverPlus10 : null,
            receiverPlus10ReadReached,
            receiverPlus10Captured,
            maskFromReceiverPlus08,
            decrementedReceiverPlus08,
            receiverPlus0COldReadReached
                ? ReceiverAddress(capture.ReceiverBaseEsi, ReceiverPlus0COffset)
                : null,
            receiverPlus0COldCaptured ? receiverPlus0COld : null,
            receiverPlus0COldReadReached,
            receiverPlus0COldCaptured,
            receiverPlus0CWriteReached,
            receiverPlus0CWriteReached ? receiverPlus0CWriteValue : null,
            unreachedReceiverPlus0COldCaptureProvided,
            receiverPlus0CFreshReadReached
                ? ReceiverAddress(capture.ReceiverBaseEsi, ReceiverPlus0COffset)
                : null,
            receiverPlus0CFreshCaptured ? receiverPlus0CFresh : null,
            receiverPlus0CFreshReadReached,
            receiverPlus0CFreshCaptured,
            wrappingEdiPlusReceiverPlus0CFresh,
            shiftedEdiPlusReceiverPlus0CFresh,
            firstSlotIndex,
            receiverPlus04ReadReached
                ? ReceiverAddress(capture.ReceiverBaseEsi, ReceiverPlus04Offset)
                : null,
            receiverPlus04Captured ? receiverPlus04 : null,
            receiverPlus04ReadReached,
            receiverPlus04Captured,
            firstSlotByteOffset,
            firstSlotAddress,
            firstSlotDwordCaptured ? firstSlotDword : null,
            firstSlotDwordReadReached,
            firstSlotDwordCaptured,
            firstSlotZeroTestProjected,
            firstSlotNonzero,
            selectedDestinationRawFileOffset,
            unreachedReceiverPlus10CaptureProvided,
            unreachedReceiverPlus0CFreshCaptureProvided,
            unreachedReceiverPlus04CaptureProvided,
            unreachedFirstSlotDwordCaptureProvided,
            stopAt);
}
