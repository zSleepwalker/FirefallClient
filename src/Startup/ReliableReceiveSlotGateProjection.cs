using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveSlotGateValidatedInput
{
    private ReliableReceiveSlotGateValidatedInput(
        uint receiverBaseAddress,
        uint frameBaseAddress,
        ushort delta)
    {
        ReceiverBaseAddress = receiverBaseAddress;
        FrameBaseAddress = frameBaseAddress;
        Delta = delta;
        IsValidated = true;
    }

    public uint ReceiverBaseAddress { get; }
    public uint FrameBaseAddress { get; }
    public ushort Delta { get; }
    public bool IsValidated { get; }

    public static bool TryCreate(
        uint receiverBaseAddress,
        uint frameBaseAddress,
        ushort delta,
        out ReliableReceiveSlotGateValidatedInput input)
    {
        if (delta >= ReliableReceiveSlotGateProjection.DeltaUpperBoundExclusive)
        {
            input = default;
            return false;
        }

        input = new ReliableReceiveSlotGateValidatedInput(
            receiverBaseAddress,
            frameBaseAddress,
            delta);
        return true;
    }
}

public readonly record struct ReliableReceiveSlotGateCapture(
    uint? ReceiverDword,
    uint? SlotDword);

public enum ReliableReceiveSlotGateOutcome
{
    InvalidOrUnvalidatedDelta,
    MissingReceiverDwordCapture,
    UnreachedSlotDwordCapture,
    MissingSlotDwordCapture,
    ZeroSlotBranchSelected,
    NonzeroSlotFallthroughSelected
}

public readonly record struct ReliableReceiveSlotGatePlan(
    ReliableReceiveSlotGateValidatedInput Input,
    ReliableReceiveSlotGateCapture Capture,
    ReliableReceiveSlotGateOutcome Outcome,
    bool ReceiverDwordRead,
    uint? ReceiverDwordAddress,
    bool UnreachedSlotDwordCaptureProvided,
    uint? AddedDeltaDword,
    uint? MaskedDword,
    bool? MaskedSignBitSet,
    uint? DecrementedMaskedDword,
    uint? OrExtendedDword,
    uint? IncrementedOrExtendedDword,
    uint? NormalizedDword,
    ushort? IndexWord,
    uint? ZeroExtendedIndexDword,
    uint? SlotArrayOffsetDword,
    uint? SlotByteOffsetDword,
    uint? ComputedSlotAddress,
    bool FrameArgumentWriteReached,
    uint? FrameArgumentAddress,
    uint? FrameArgumentValue,
    bool SlotDwordRead,
    uint? SlotDwordAddress,
    bool? ZeroBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveSlotGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public static class ReliableReceiveSlotGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE5303;
    public const uint GateStartVirtualAddress = 0x00FE5F03;
    public const uint ReceiverDwordOffset = 0x478;
    public const uint FrameArgumentOffset = 0x08;
    public const ushort DeltaUpperBoundExclusive = 0x100;
    public const uint SignedRemainderMask = 0x800000FF;
    public const uint SignExtensionMask = 0xFFFFFF00;
    public const uint SlotArrayBaseOffset = 0x1E;

    public const uint ReceiverDwordReadRawFileOffset = 0x00BE5303;
    public const uint DeltaAddRawFileOffset = 0x00BE530B;
    public const uint MaskRawFileOffset = 0x00BE530D;
    public const uint SignBranchRawFileOffset = 0x00BE5313;
    public const uint DecrementRawFileOffset = 0x00BE5315;
    public const uint SignExtensionOrRawFileOffset = 0x00BE5316;
    public const uint IncrementRawFileOffset = 0x00BE531C;
    public const uint IndexZeroExtendRawFileOffset = 0x00BE531D;
    public const uint SlotArrayOffsetRawFileOffset = 0x00BE5320;
    public const uint SlotAddressRawFileOffset = 0x00BE5323;
    public const uint FrameArgumentWriteRawFileOffset = 0x00BE5326;
    public const uint SlotDwordReadRawFileOffset = 0x00BE5329;
    public const uint SlotDwordTestRawFileOffset = 0x00BE532B;
    public const uint ZeroBranchRawFileOffset = 0x00BE532D;
    public const uint ZeroBranchVirtualAddress = 0x00FE5F2D;
    public const uint ZeroBranchDestinationRawFileOffset = 0x00BE538D;
    public const uint ZeroBranchDestinationVirtualAddress = 0x00FE5F8D;
    public const uint NonzeroFallthroughRawFileOffset = 0x00BE532F;
    public const uint GateEndRawFileOffsetExclusive = NonzeroFallthroughRawFileOffset;

    public const string CapturedGateBytes =
        "8B 8E 78 04 00 00 8B C3 03 C8 81 E1 FF 00 00 80 79 08 49 "
        + "81 C9 00 FF FF FF 41 0F B7 C1 83 C0 1E 8D 04 86 89 45 08 "
        + "8B 00 85 C0 74 5E";

    public static ImmutableArray<ReliableReceiveSlotGateInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveSlotGateInstructionEvidence(
                "Read the receiver DWORD at receiver+0x478",
                ReceiverDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordReadRawFileOffset),
                "8B 8E 78 04 00 00"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Add the retained EBX delta to the receiver DWORD with 32-bit wrap",
                DeltaAddRawFileOffset,
                RawFileOffsetToVirtualAddress(DeltaAddRawFileOffset),
                "03 C8"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Keep only bit 31 and the low byte, then branch on the masked sign bit",
                MaskRawFileOffset,
                RawFileOffsetToVirtualAddress(MaskRawFileOffset),
                "81 E1 FF 00 00 80 79 08",
                0x00BE531D,
                RawFileOffsetToVirtualAddress(0x00BE531D)),
            new ReliableReceiveSlotGateInstructionEvidence(
                "When the masked value is negative, decrement, OR with 0xFFFFFF00, and increment in DWORD arithmetic",
                DecrementRawFileOffset,
                RawFileOffsetToVirtualAddress(DecrementRawFileOffset),
                "49 81 C9 00 FF FF FF 41"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Zero-extend CX and add the slot-array base index 0x1E",
                IndexZeroExtendRawFileOffset,
                RawFileOffsetToVirtualAddress(IndexZeroExtendRawFileOffset),
                "0F B7 C1 83 C0 1E"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Form receiver+EAX*4 as a wrapping DWORD slot address",
                SlotAddressRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotAddressRawFileOffset),
                "8D 04 86"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Store the computed slot address to frame+8 before reading through it",
                FrameArgumentWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameArgumentWriteRawFileOffset),
                "89 45 08"),
            new ReliableReceiveSlotGateInstructionEvidence(
                "Read and test only the slot DWORD; zero branches to raw 0xBE538D, nonzero falls through to raw 0xBE532F",
                SlotDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotDwordReadRawFileOffset),
                "8B 00 85 C0 74 5E",
                ZeroBranchDestinationRawFileOffset,
                ZeroBranchDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverDwordOffset);

    public static uint FrameArgumentAddress(uint frameBaseAddress) =>
        unchecked(frameBaseAddress + FrameArgumentOffset);

    public static ReliableReceiveSlotGatePlan Project(
        ReliableReceiveSlotGateValidatedInput input,
        ReliableReceiveSlotGateCapture capture)
    {
        if (!input.IsValidated || input.Delta >= DeltaUpperBoundExclusive)
        {
            return new ReliableReceiveSlotGatePlan(
                input,
                capture,
                ReliableReceiveSlotGateOutcome.InvalidOrUnvalidatedDelta,
                ReceiverDwordRead: false,
                ReceiverDwordAddress: null,
                UnreachedSlotDwordCaptureProvided: capture.SlotDword.HasValue,
                AddedDeltaDword: null,
                MaskedDword: null,
                MaskedSignBitSet: null,
                DecrementedMaskedDword: null,
                OrExtendedDword: null,
                IncrementedOrExtendedDword: null,
                NormalizedDword: null,
                IndexWord: null,
                ZeroExtendedIndexDword: null,
                SlotArrayOffsetDword: null,
                SlotByteOffsetDword: null,
                ComputedSlotAddress: null,
                FrameArgumentWriteReached: false,
                FrameArgumentAddress: null,
                FrameArgumentValue: null,
                SlotDwordRead: false,
                SlotDwordAddress: null,
                ZeroBranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverDwordReadRawFileOffset);
        }

        if (capture.ReceiverDword is not uint receiverDword)
        {
            bool laterCaptureProvided = capture.SlotDword.HasValue;
            return new ReliableReceiveSlotGatePlan(
                input,
                capture,
                laterCaptureProvided
                    ? ReliableReceiveSlotGateOutcome.UnreachedSlotDwordCapture
                    : ReliableReceiveSlotGateOutcome.MissingReceiverDwordCapture,
                ReceiverDwordRead: false,
                ReceiverDwordAddress: ReceiverDwordAddress(input.ReceiverBaseAddress),
                UnreachedSlotDwordCaptureProvided: laterCaptureProvided,
                AddedDeltaDword: null,
                MaskedDword: null,
                MaskedSignBitSet: null,
                DecrementedMaskedDword: null,
                OrExtendedDword: null,
                IncrementedOrExtendedDword: null,
                NormalizedDword: null,
                IndexWord: null,
                ZeroExtendedIndexDword: null,
                SlotArrayOffsetDword: null,
                SlotByteOffsetDword: null,
                ComputedSlotAddress: null,
                FrameArgumentWriteReached: false,
                FrameArgumentAddress: null,
                FrameArgumentValue: null,
                SlotDwordRead: false,
                SlotDwordAddress: null,
                ZeroBranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverDwordReadRawFileOffset);
        }

        uint addedDeltaDword = unchecked(receiverDword + input.Delta);
        uint maskedDword = addedDeltaDword & SignedRemainderMask;
        bool maskedSignBitSet = (maskedDword & 0x80000000) != 0;
        uint? decrementedMaskedDword = null;
        uint? orExtendedDword = null;
        uint? incrementedOrExtendedDword = null;
        uint normalizedDword = maskedDword;

        if (maskedSignBitSet)
        {
            uint decremented = unchecked(maskedDword - 1);
            uint orExtended = decremented | SignExtensionMask;
            uint incremented = unchecked(orExtended + 1);
            decrementedMaskedDword = decremented;
            orExtendedDword = orExtended;
            incrementedOrExtendedDword = incremented;
            normalizedDword = incremented;
        }

        ushort indexWord = unchecked((ushort)normalizedDword);
        uint zeroExtendedIndexDword = indexWord;
        uint slotArrayOffsetDword = unchecked(zeroExtendedIndexDword + SlotArrayBaseOffset);
        uint slotByteOffsetDword = unchecked(slotArrayOffsetDword * 4);
        uint computedSlotAddress = unchecked(input.ReceiverBaseAddress + slotByteOffsetDword);
        uint frameArgumentAddress = FrameArgumentAddress(input.FrameBaseAddress);

        if (capture.SlotDword is not uint slotDword)
        {
            return new ReliableReceiveSlotGatePlan(
                input,
                capture,
                ReliableReceiveSlotGateOutcome.MissingSlotDwordCapture,
                ReceiverDwordRead: true,
                ReceiverDwordAddress: ReceiverDwordAddress(input.ReceiverBaseAddress),
                UnreachedSlotDwordCaptureProvided: false,
                addedDeltaDword,
                maskedDword,
                maskedSignBitSet,
                decrementedMaskedDword,
                orExtendedDword,
                incrementedOrExtendedDword,
                normalizedDword,
                indexWord,
                zeroExtendedIndexDword,
                slotArrayOffsetDword,
                slotByteOffsetDword,
                computedSlotAddress,
                FrameArgumentWriteReached: true,
                frameArgumentAddress,
                computedSlotAddress,
                SlotDwordRead: false,
                SlotDwordAddress: computedSlotAddress,
                ZeroBranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: SlotDwordReadRawFileOffset);
        }

        bool zeroBranchTaken = slotDword == 0;
        return new ReliableReceiveSlotGatePlan(
            input,
            capture,
            zeroBranchTaken
                ? ReliableReceiveSlotGateOutcome.ZeroSlotBranchSelected
                : ReliableReceiveSlotGateOutcome.NonzeroSlotFallthroughSelected,
            ReceiverDwordRead: true,
            ReceiverDwordAddress: ReceiverDwordAddress(input.ReceiverBaseAddress),
            UnreachedSlotDwordCaptureProvided: false,
            addedDeltaDword,
            maskedDword,
            maskedSignBitSet,
            decrementedMaskedDword,
            orExtendedDword,
            incrementedOrExtendedDword,
            normalizedDword,
            indexWord,
            zeroExtendedIndexDword,
            slotArrayOffsetDword,
            slotByteOffsetDword,
            computedSlotAddress,
            FrameArgumentWriteReached: true,
            frameArgumentAddress,
            computedSlotAddress,
            SlotDwordRead: true,
            SlotDwordAddress: computedSlotAddress,
            zeroBranchTaken,
            zeroBranchTaken
                ? ZeroBranchDestinationRawFileOffset
                : NonzeroFallthroughRawFileOffset,
            GateEndRawFileOffsetExclusive);
    }
}
