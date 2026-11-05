using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffHelperEntryCapture(
    uint EntryFrameEbp,
    uint EntryReceiverEcx,
    uint? ReceiverPlus0C,
    uint? ReceiverPlus10,
    uint? ReceiverPlus08,
    bool? HelperReturnedNormally);

public enum FrameHandoffHelperEntryOutcome
{
    MissingReceiverPlus0CCapture,
    MissingReceiverPlus10Capture,
    LowTwoSumBitsNonzero,
    ReceiverPlus08GreaterThanRoundedLength,
    MissingReceiverPlus08Capture,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    HelperReturnedNormally
}

public readonly record struct FrameHandoffHelperEntryPlan(
    FrameHandoffHelperEntryCapture Capture,
    FrameHandoffHelperEntryOutcome Outcome,
    uint? WrappingSum,
    byte? LowTwoSumBits,
    uint? WrappingPlusFour,
    uint? RoundedLength,
    uint? ReceiverPlus0CReadAddress,
    uint? ReceiverPlus0CReadValue,
    bool ReceiverPlus0CReadReached,
    bool ReceiverPlus0CCaptured,
    uint? ReceiverPlus10ReadAddress,
    uint? ReceiverPlus10ReadValue,
    bool ReceiverPlus10ReadReached,
    bool ReceiverPlus10Captured,
    uint? ReceiverPlus08ReadAddress,
    uint? ReceiverPlus08ReadValue,
    bool ReceiverPlus08ReadReached,
    bool ReceiverPlus08Captured,
    bool UnreachedReceiverPlus10CaptureProvided,
    bool UnreachedReceiverPlus08CaptureProvided,
    bool UnreachedHelperReturnedNormallyCaptureProvided,
    bool PayloadValuePushed,
    uint? PushedReceiverPlus10Value,
    bool ReceiverRestoredToEcx,
    uint? ReceiverPassedInEcx,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffHelperEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the first helper-entry gate through its call's normal return boundary.
/// </summary>
public static class FrameHandoffHelperEntryProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE64A0;
    public const uint EntryVirtualAddress = 0x00FE70A0;
    public const uint FirstReceiverReadRawFileOffset = 0x00BE64A6;
    public const uint SecondReceiverReadRawFileOffset = 0x00BE64A9;
    public const uint SumRawFileOffset = 0x00BE64AC;
    public const uint FirstBranchRawFileOffset = 0x00BE64B0;
    public const uint RoundedLengthRawFileOffset = 0x00BE64B2;
    public const uint ReceiverPlus08CompareRawFileOffset = 0x00BE64B8;
    public const uint SecondBranchRawFileOffset = 0x00BE64BB;
    public const uint PayloadPushRawFileOffset = 0x00BE64BD;
    public const uint ReceiverRestoreRawFileOffset = 0x00BE64BE;
    public const uint HelperCallRawFileOffset = 0x00BE64C0;
    public const uint HelperReturnRawFileOffset = 0x00BE64C5;
    public const uint FirstBranchDestinationRawFileOffset = HelperReturnRawFileOffset;
    public const uint SecondBranchDestinationRawFileOffset = HelperReturnRawFileOffset;
    public const uint FallthroughDestinationRawFileOffset = HelperReturnRawFileOffset;
    public const uint HelperCallTargetRawFileOffset = 0x00BE6B50;
    public const uint EndRawFileOffsetExclusive = HelperReturnRawFileOffset;

    public const uint ReceiverPlus0COffset = 0x0C;
    public const uint ReceiverPlus10Offset = 0x10;
    public const uint ReceiverPlus08Offset = 0x08;
    public const uint RoundedLengthIncrement = 4;
    public const int RoundedLengthShift = 2;

    public const string CapturedWindowBytes =
        "55 8B EC 56 8B F1 8B 46 0C 8B 4E 10 03 C1 A8 03 75 13 "
        + "8D 41 04 C1 E8 02 39 46 08 77 08 51 8B CE E8 8B 06 00 00";

    public static ImmutableArray<FrameHandoffHelperEntryInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffHelperEntryInstructionEvidence(
                "Save EBP and ESI, then preserve entry ECX in ESI",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "55 8B EC 56 8B F1"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Read DWORD [entry receiver+0x0C] into EAX",
                FirstReceiverReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstReceiverReadRawFileOffset),
                "8B 46 0C"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Read DWORD [entry receiver+0x10] into ECX",
                SecondReceiverReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondReceiverReadRawFileOffset),
                "8B 4E 10"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Add the captured DWORDs with 32-bit wrap and test AL bits 0..1",
                SumRawFileOffset,
                RawFileOffsetToVirtualAddress(SumRawFileOffset),
                "03 C1 A8 03"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Nonzero low two bits branch to the exclusive end boundary",
                FirstBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstBranchRawFileOffset),
                "75 13",
                FirstBranchDestinationRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstBranchDestinationRawFileOffset)),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Compute wrapping (captured + 4) followed by unsigned logical SHR 2",
                RoundedLengthRawFileOffset,
                RawFileOffsetToVirtualAddress(RoundedLengthRawFileOffset),
                "8D 41 04 C1 E8 02"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Compare DWORD [entry receiver+8] unsigned against the rounded length",
                ReceiverPlus08CompareRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus08CompareRawFileOffset),
                "39 46 08"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Unsigned JA branches to the exclusive end boundary",
                SecondBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondBranchRawFileOffset),
                "77 08",
                SecondBranchDestinationRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondBranchDestinationRawFileOffset)),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Push the captured DWORD from [entry receiver+0x10]",
                PayloadPushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadPushRawFileOffset),
                "51"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Restore ECX from ESI, which still holds the original receiver",
                ReceiverRestoreRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverRestoreRawFileOffset),
                "8B CE"),
            new FrameHandoffHelperEntryInstructionEvidence(
                "Call raw 0xBE6B50; normal return is the exclusive end boundary",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E8 8B 06 00 00",
                HelperCallTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallTargetRawFileOffset)));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverAddress(uint receiverBaseAddress, uint fieldOffset) =>
        unchecked(receiverBaseAddress + fieldOffset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static FrameHandoffHelperEntryPlan Project(FrameHandoffHelperEntryCapture capture)
    {
        if (capture.ReceiverPlus0C is not uint receiverPlus0C)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.MissingReceiverPlus0CCapture,
                receiverPlus0CReadReached: true,
                unreachedReceiverPlus10CaptureProvided: capture.ReceiverPlus10.HasValue,
                unreachedReceiverPlus08CaptureProvided: capture.ReceiverPlus08.HasValue,
                unreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                stopAt: FirstReceiverReadRawFileOffset);
        }

        if (capture.ReceiverPlus10 is not uint receiverPlus10)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.MissingReceiverPlus10Capture,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                unreachedReceiverPlus08CaptureProvided: capture.ReceiverPlus08.HasValue,
                unreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                stopAt: SecondReceiverReadRawFileOffset);
        }

        uint wrappingSum = unchecked(receiverPlus0C + receiverPlus10);
        byte lowTwoSumBits = (byte)(wrappingSum & 3);
        if (lowTwoSumBits != 0)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.LowTwoSumBitsNonzero,
                wrappingSum,
                lowTwoSumBits,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                unreachedReceiverPlus10CaptureProvided: false,
                unreachedReceiverPlus08CaptureProvided: capture.ReceiverPlus08.HasValue,
                unreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                selectedDestinationRawFileOffset: FirstBranchDestinationRawFileOffset,
                stopAt: FirstBranchDestinationRawFileOffset);
        }

        uint wrappingPlusFour = unchecked(receiverPlus10 + RoundedLengthIncrement);
        uint roundedLength = wrappingPlusFour >> RoundedLengthShift;
        if (capture.ReceiverPlus08 is not uint receiverPlus08)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.MissingReceiverPlus08Capture,
                wrappingSum,
                lowTwoSumBits,
                wrappingPlusFour,
                roundedLength,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus08ReadReached: true,
                unreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                stopAt: ReceiverPlus08CompareRawFileOffset);
        }

        if (receiverPlus08 > roundedLength)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.ReceiverPlus08GreaterThanRoundedLength,
                wrappingSum,
                lowTwoSumBits,
                wrappingPlusFour,
                roundedLength,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                unreachedHelperReturnedNormallyCaptureProvided: capture.HelperReturnedNormally.HasValue,
                selectedDestinationRawFileOffset: SecondBranchDestinationRawFileOffset,
                stopAt: SecondBranchDestinationRawFileOffset);
        }

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.MissingHelperReturnCapture,
                wrappingSum,
                lowTwoSumBits,
                wrappingPlusFour,
                roundedLength,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                payloadValuePushed: true,
                pushedReceiverPlus10Value: receiverPlus10,
                receiverRestoredToEcx: true,
                receiverPassedInEcx: capture.EntryReceiverEcx,
                helperCallReached: true,
                stopAt: HelperCallRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffHelperEntryOutcome.HelperDidNotReturnNormally,
                wrappingSum,
                lowTwoSumBits,
                wrappingPlusFour,
                roundedLength,
                receiverPlus0CReadReached: true,
                receiverPlus0CCaptured: true,
                receiverPlus10ReadReached: true,
                receiverPlus10Captured: true,
                receiverPlus08ReadReached: true,
                receiverPlus08Captured: true,
                payloadValuePushed: true,
                pushedReceiverPlus10Value: receiverPlus10,
                receiverRestoredToEcx: true,
                receiverPassedInEcx: capture.EntryReceiverEcx,
                helperCallReached: true,
                helperReturnedNormally: false,
                stopAt: HelperCallRawFileOffset);
        }

        return Stop(
            capture,
            FrameHandoffHelperEntryOutcome.HelperReturnedNormally,
            wrappingSum,
            lowTwoSumBits,
            wrappingPlusFour,
            roundedLength,
            receiverPlus0CReadReached: true,
            receiverPlus0CCaptured: true,
            receiverPlus10ReadReached: true,
            receiverPlus10Captured: true,
            receiverPlus08ReadReached: true,
            receiverPlus08Captured: true,
            payloadValuePushed: true,
            pushedReceiverPlus10Value: receiverPlus10,
            receiverRestoredToEcx: true,
            receiverPassedInEcx: capture.EntryReceiverEcx,
            helperCallReached: true,
            helperReturnedNormally: true,
            selectedDestinationRawFileOffset: FallthroughDestinationRawFileOffset,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static FrameHandoffHelperEntryPlan Stop(
        FrameHandoffHelperEntryCapture capture,
        FrameHandoffHelperEntryOutcome outcome,
        uint? wrappingSum = null,
        byte? lowTwoSumBits = null,
        uint? wrappingPlusFour = null,
        uint? roundedLength = null,
        bool receiverPlus0CReadReached = false,
        bool receiverPlus0CCaptured = false,
        bool receiverPlus10ReadReached = false,
        bool receiverPlus10Captured = false,
        bool receiverPlus08ReadReached = false,
        bool receiverPlus08Captured = false,
        bool unreachedReceiverPlus10CaptureProvided = false,
        bool unreachedReceiverPlus08CaptureProvided = false,
        bool unreachedHelperReturnedNormallyCaptureProvided = false,
        bool payloadValuePushed = false,
        uint? pushedReceiverPlus10Value = null,
        bool receiverRestoredToEcx = false,
        uint? receiverPassedInEcx = null,
        bool helperCallReached = false,
        bool? helperReturnedNormally = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            wrappingSum,
            lowTwoSumBits,
            wrappingPlusFour,
            roundedLength,
            receiverPlus0CReadReached
                ? ReceiverAddress(capture.EntryReceiverEcx, ReceiverPlus0COffset)
                : null,
            receiverPlus0CCaptured ? capture.ReceiverPlus0C : null,
            receiverPlus0CReadReached,
            receiverPlus0CCaptured,
            receiverPlus10ReadReached
                ? ReceiverAddress(capture.EntryReceiverEcx, ReceiverPlus10Offset)
                : null,
            receiverPlus10Captured ? capture.ReceiverPlus10 : null,
            receiverPlus10ReadReached,
            receiverPlus10Captured,
            receiverPlus08ReadReached
                ? ReceiverAddress(capture.EntryReceiverEcx, ReceiverPlus08Offset)
                : null,
            receiverPlus08Captured ? capture.ReceiverPlus08 : null,
            receiverPlus08ReadReached,
            receiverPlus08Captured,
            unreachedReceiverPlus10CaptureProvided,
            unreachedReceiverPlus08CaptureProvided,
            unreachedHelperReturnedNormallyCaptureProvided,
            payloadValuePushed,
            pushedReceiverPlus10Value,
            receiverRestoredToEcx,
            receiverPassedInEcx,
            helperCallReached,
            helperReturnedNormally,
            selectedDestinationRawFileOffset,
            stopAt);
}
