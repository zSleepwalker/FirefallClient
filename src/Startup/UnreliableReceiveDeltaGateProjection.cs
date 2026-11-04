using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveDeltaGateCapture(
    uint PayloadBaseAddress,
    ushort? PayloadWord,
    uint ReceiverBaseAddress,
    ushort? ReceiverWord);

public enum UnreliableReceiveDeltaGateOutcome
{
    MissingReceiverWordCapture,
    UnreachedPayloadWordCapture,
    MissingPayloadWordCapture,
    BranchTaken,
    FallthroughSelected
}

public readonly record struct UnreliableReceiveDeltaGatePlan(
    UnreliableReceiveDeltaGateCapture Capture,
    UnreliableReceiveDeltaGateOutcome Outcome,
    bool ReceiverWordRead,
    uint? ReceiverWordAddress,
    bool UnreachedPayloadWordCaptureProvided,
    bool PayloadWordRead,
    uint? PayloadWordAddress,
    uint? Difference32,
    uint? Residue32,
    bool? SignedGreaterThanOrEqual,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveDeltaGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the unreliable-receive delta gate and stops at its selected destination.
/// </summary>
public static class UnreliableReceiveDeltaGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE5A6B;
    public const uint GateStartVirtualAddress = 0x00FE666B;
    public const uint ReceiverWordOffset = 0x1E;
    public const uint PayloadWordOffset = 0x02;

    public const uint ArgumentLoadRawFileOffset = 0x00BE5A6B;
    public const uint ReceiverWordReadRawFileOffset = 0x00BE5A6E;
    public const uint ReceiverToEaxRawFileOffset = 0x00BE5A72;
    public const uint PayloadWordReadRawFileOffset = 0x00BE5A74;
    public const uint DifferenceSubtractRawFileOffset = 0x00BE5A78;
    public const uint ResidueMaskRawFileOffset = 0x00BE5A7A;
    public const uint SignedCompareRawFileOffset = 0x00BE5A7F;
    public const uint BranchRawFileOffset = 0x00BE5A84;
    public const uint BranchVirtualAddress = 0x00FE6684;
    public const uint BranchTakenDestinationRawFileOffset = 0x00BE5AC2;
    public const uint BranchTakenDestinationVirtualAddress = 0x00FE66C2;
    public const uint FallthroughDestinationRawFileOffset = 0x00BE5A86;
    public const uint FallthroughDestinationVirtualAddress = 0x00FE6686;
    public const uint GateEndRawFileOffsetExclusive = FallthroughDestinationRawFileOffset;
    public const uint SignedCompareImmediate = 0x8000;
    public const uint ResidueMask = 0xFFFF;

    public const string CapturedGateBytes =
        "8B 5D 08 0F B7 57 1E 8B C2 0F B7 4B 02 2B C1 "
        + "25 FF FF 00 00 3D 00 80 00 00 7D 3C";

    public static ImmutableArray<UnreliableReceiveDeltaGateInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Load the payload argument into EBX",
                ArgumentLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(ArgumentLoadRawFileOffset),
                "8B 5D 08"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Read the receiver WORD at EDI+0x1E into EDX before the payload WORD",
                ReceiverWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverWordReadRawFileOffset),
                "0F B7 57 1E"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Copy the zero-extended receiver WORD from EDX to EAX",
                ReceiverToEaxRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverToEaxRawFileOffset),
                "8B C2"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Read the payload WORD at EBX+2 into ECX",
                PayloadWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadWordReadRawFileOffset),
                "0F B7 4B 02"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Subtract the zero-extended payload WORD from EAX using DWORD arithmetic",
                DifferenceSubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(DifferenceSubtractRawFileOffset),
                "2B C1"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Mask EAX to its low WORD while retaining the DWORD result",
                ResidueMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(ResidueMaskRawFileOffset),
                "25 FF FF 00 00"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Compare the masked DWORD against 0x8000",
                SignedCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(SignedCompareRawFileOffset),
                "3D 00 80 00 00"),
            new UnreliableReceiveDeltaGateInstructionEvidence(
                "Signed JGE selects raw 0xBE5AC2; not-taken falls through to raw 0xBE5A86",
                BranchRawFileOffset,
                BranchVirtualAddress,
                "7D 3C",
                BranchTakenDestinationRawFileOffset,
                BranchTakenDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverWordAddress(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverWordOffset);

    public static uint PayloadWordAddress(uint payloadBaseAddress) =>
        unchecked(payloadBaseAddress + PayloadWordOffset);

    public static UnreliableReceiveDeltaGatePlan Project(UnreliableReceiveDeltaGateCapture capture)
    {
        uint receiverWordAddress = ReceiverWordAddress(capture.ReceiverBaseAddress);
        if (capture.ReceiverWord is not ushort receiverWord)
        {
            bool payloadCaptureProvided = capture.PayloadWord.HasValue;
            return new UnreliableReceiveDeltaGatePlan(
                capture,
                payloadCaptureProvided
                    ? UnreliableReceiveDeltaGateOutcome.UnreachedPayloadWordCapture
                    : UnreliableReceiveDeltaGateOutcome.MissingReceiverWordCapture,
                ReceiverWordRead: false,
                receiverWordAddress,
                UnreachedPayloadWordCaptureProvided: payloadCaptureProvided,
                PayloadWordRead: false,
                PayloadWordAddress: null,
                Difference32: null,
                Residue32: null,
                SignedGreaterThanOrEqual: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverWordReadRawFileOffset);
        }

        uint payloadWordAddress = PayloadWordAddress(capture.PayloadBaseAddress);
        if (capture.PayloadWord is not ushort payloadWord)
        {
            return new UnreliableReceiveDeltaGatePlan(
                capture,
                UnreliableReceiveDeltaGateOutcome.MissingPayloadWordCapture,
                ReceiverWordRead: true,
                receiverWordAddress,
                UnreachedPayloadWordCaptureProvided: false,
                PayloadWordRead: false,
                payloadWordAddress,
                Difference32: null,
                Residue32: null,
                SignedGreaterThanOrEqual: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: PayloadWordReadRawFileOffset);
        }

        uint difference32 = unchecked((uint)receiverWord - payloadWord);
        uint residue32 = difference32 & ResidueMask;
        bool signedGreaterThanOrEqual = (int)residue32 >= (int)SignedCompareImmediate;

        return new UnreliableReceiveDeltaGatePlan(
            capture,
            signedGreaterThanOrEqual
                ? UnreliableReceiveDeltaGateOutcome.BranchTaken
                : UnreliableReceiveDeltaGateOutcome.FallthroughSelected,
            ReceiverWordRead: true,
            receiverWordAddress,
            UnreachedPayloadWordCaptureProvided: false,
            PayloadWordRead: true,
            payloadWordAddress,
            difference32,
            residue32,
            signedGreaterThanOrEqual,
            signedGreaterThanOrEqual,
            signedGreaterThanOrEqual
                ? BranchTakenDestinationRawFileOffset
                : FallthroughDestinationRawFileOffset,
            GateEndRawFileOffsetExclusive);
    }
}
