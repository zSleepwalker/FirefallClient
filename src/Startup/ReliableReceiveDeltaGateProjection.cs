using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveDeltaGateCapture(
    uint PayloadBaseAddress,
    ushort? PayloadWord,
    uint ReceiverBaseAddress,
    ushort? ReceiverWord);

public enum ReliableReceiveDeltaGateOutcome
{
    MissingPayloadWordCapture,
    UnreachedReceiverWordCapture,
    MissingReceiverWordCapture,
    BranchTaken,
    FallthroughSelected
}

public readonly record struct ReliableReceiveDeltaGatePlan(
    ReliableReceiveDeltaGateCapture Capture,
    ReliableReceiveDeltaGateOutcome Outcome,
    bool PayloadWordRead,
    uint? PayloadWordAddress,
    bool UnreachedReceiverWordCaptureProvided,
    bool ReceiverWordRead,
    uint? ReceiverWordAddress,
    ushort? DifferenceLow16,
    uint? Difference32,
    uint? CompareIntermediate32,
    ushort? CompareLow16,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveDeltaGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public static class ReliableReceiveDeltaGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE5227;
    public const uint GateStartVirtualAddress = 0x00FE5E27;
    public const uint PayloadWordOffset = 0x02;
    public const uint ReceiverWordOffset = 0x2A;
    public const uint ArgumentStackSlotOffset = 0x08;
    public const ushort CompareImmediate = 0x7EFF;
    public const uint CompareSubtrahend = 0x100;

    public const uint ArgumentLoadRawFileOffset = 0x00BE5227;
    public const uint PayloadWordReadRawFileOffset = 0x00BE522A;
    public const uint PayloadAddressFormationRawFileOffset = 0x00BE522E;
    public const uint ReceiverWordReadRawFileOffset = 0x00BE5231;
    public const uint DifferenceSubtractRawFileOffset = 0x00BE5237;
    public const uint CompareLimitStoreRawFileOffset = 0x00BE5239;
    public const uint DifferenceTruncationRawFileOffset = 0x00BE5240;
    public const uint CompareSubtractionRawFileOffset = 0x00BE5243;
    public const uint CompareRawFileOffset = 0x00BE5249;
    public const uint BranchRawFileOffset = 0x00BE524D;
    public const uint BranchVirtualAddress = 0x00FE5E4D;
    public const uint BranchTakenDestinationRawFileOffset = 0x00BE528F;
    public const uint BranchTakenDestinationVirtualAddress = 0x00FE5E8F;
    public const uint FallthroughDestinationRawFileOffset = 0x00BE524F;
    public const uint FallthroughDestinationVirtualAddress = 0x00FE5E4F;
    public const uint GateEndRawFileOffsetExclusive = FallthroughDestinationRawFileOffset;

    public const string CapturedGateBytes =
        "8B 7D 08 0F B7 57 02 8D 4F 02 0F B7 5E 2A 8B C2 2B C3 "
        + "C7 45 08 FF 7E 00 00 0F B7 D8 8D 83 00 FF FF FF "
        + "66 3B 45 08 77 40";

    public static ImmutableArray<ReliableReceiveDeltaGateInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Load the payload pointer from the argument stack slot",
                ArgumentLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(ArgumentLoadRawFileOffset),
                "8B 7D 08"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Zero-extend the payload word at pointer offset 2 into EDX",
                PayloadWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadWordReadRawFileOffset),
                "0F B7 57 02"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Form the wrapping 32-bit payload address at pointer offset 2 in ECX",
                PayloadAddressFormationRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadAddressFormationRawFileOffset),
                "8D 4F 02"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Zero-extend the receiver word at offset 0x2A into EBX",
                ReceiverWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverWordReadRawFileOffset),
                "0F B7 5E 2A"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Copy the payload word to EAX and subtract the receiver word in 32-bit arithmetic",
                0x00BE5235,
                RawFileOffsetToVirtualAddress(0x00BE5235),
                "8B C2 2B C3"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Overwrite the argument stack slot with DWORD 0x7EFF for the later WORD comparison",
                CompareLimitStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(CompareLimitStoreRawFileOffset),
                "C7 45 08 FF 7E 00 00"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Truncate the difference to AX and zero-extend it into EBX",
                DifferenceTruncationRawFileOffset,
                RawFileOffsetToVirtualAddress(DifferenceTruncationRawFileOffset),
                "0F B7 D8"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Compute the 32-bit intermediate EBX minus 0x100",
                CompareSubtractionRawFileOffset,
                RawFileOffsetToVirtualAddress(CompareSubtractionRawFileOffset),
                "8D 83 00 FF FF FF"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Compare only AX against the low WORD at the overwritten argument slot",
                CompareRawFileOffset,
                RawFileOffsetToVirtualAddress(CompareRawFileOffset),
                "66 3B 45 08"),
            new ReliableReceiveDeltaGateInstructionEvidence(
                "Unsigned JA selects raw 0xBE528F; not-taken falls through to raw 0xBE524F",
                BranchRawFileOffset,
                BranchVirtualAddress,
                "77 40",
                BranchTakenDestinationRawFileOffset,
                BranchTakenDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PayloadWordAddress(uint payloadBaseAddress) =>
        unchecked(payloadBaseAddress + PayloadWordOffset);

    public static uint ReceiverWordAddress(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverWordOffset);

    public static ReliableReceiveDeltaGatePlan Project(ReliableReceiveDeltaGateCapture capture)
    {
        if (capture.PayloadWord is not ushort payloadWord)
        {
            bool receiverCaptureProvided = capture.ReceiverWord.HasValue;
            return new ReliableReceiveDeltaGatePlan(
                capture,
                receiverCaptureProvided
                    ? ReliableReceiveDeltaGateOutcome.UnreachedReceiverWordCapture
                    : ReliableReceiveDeltaGateOutcome.MissingPayloadWordCapture,
                PayloadWordRead: false,
                PayloadWordAddress: null,
                UnreachedReceiverWordCaptureProvided: receiverCaptureProvided,
                ReceiverWordRead: false,
                ReceiverWordAddress: null,
                DifferenceLow16: null,
                Difference32: null,
                CompareIntermediate32: null,
                CompareLow16: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: PayloadWordReadRawFileOffset);
        }

        uint payloadWordAddress = PayloadWordAddress(capture.PayloadBaseAddress);
        if (capture.ReceiverWord is not ushort receiverWord)
        {
            return new ReliableReceiveDeltaGatePlan(
                capture,
                ReliableReceiveDeltaGateOutcome.MissingReceiverWordCapture,
                PayloadWordRead: true,
                payloadWordAddress,
                UnreachedReceiverWordCaptureProvided: false,
                ReceiverWordRead: false,
                ReceiverWordAddress: ReceiverWordAddress(capture.ReceiverBaseAddress),
                DifferenceLow16: null,
                Difference32: null,
                CompareIntermediate32: null,
                CompareLow16: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverWordReadRawFileOffset);
        }

        uint difference32 = unchecked((uint)payloadWord - receiverWord);
        ushort differenceLow16 = unchecked((ushort)difference32);
        uint compareIntermediate32 = unchecked((uint)differenceLow16 - CompareSubtrahend);
        ushort compareLow16 = unchecked((ushort)compareIntermediate32);
        bool branchTaken = compareLow16 > CompareImmediate;

        return new ReliableReceiveDeltaGatePlan(
            capture,
            branchTaken
                ? ReliableReceiveDeltaGateOutcome.BranchTaken
                : ReliableReceiveDeltaGateOutcome.FallthroughSelected,
            PayloadWordRead: true,
            payloadWordAddress,
            UnreachedReceiverWordCaptureProvided: false,
            ReceiverWordRead: true,
            ReceiverWordAddress: ReceiverWordAddress(capture.ReceiverBaseAddress),
            differenceLow16,
            difference32,
            compareIntermediate32,
            compareLow16,
            branchTaken,
            branchTaken
                ? BranchTakenDestinationRawFileOffset
                : FallthroughDestinationRawFileOffset,
            GateEndRawFileOffsetExclusive);
    }
}
