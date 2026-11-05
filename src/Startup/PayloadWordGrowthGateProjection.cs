using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordGrowthGateCapture(
    uint ReceiverPointer,
    uint? ReceiverDword0C,
    uint? ReceiverDword10,
    uint? ReceiverDword8,
    bool? HandoffCallReturnedNormally);

public enum PayloadWordGrowthGateOutcome
{
    MissingReceiverDword0CCapture,
    MissingReceiverDword10Capture,
    MissingReceiverDword8Capture,
    UnreachedCaptureProvided,
    LowBitsBranchToBoundary,
    ReceiverBoundBranchesToBoundary,
    MissingHandoffCallReturnCapture,
    HandoffCallDidNotReturnNormally,
    HandoffCallReturnedNormally
}

public readonly record struct PayloadWordGrowthGatePlan(
    PayloadWordGrowthGateCapture Capture,
    PayloadWordGrowthGateOutcome Outcome,
    uint? ReceiverDword0CAddress,
    bool ReceiverDword0CRead,
    uint? ReceiverDword10Address,
    bool ReceiverDword10Read,
    uint? Sum32,
    byte? SumLowThreeBits,
    uint? ReceiverDword8Address,
    bool ReceiverDword8Read,
    uint? AlignedReceiverDword10PlusEight,
    bool? ReceiverBoundBranchTaken,
    bool HandoffCallReached,
    uint? HandoffCallPushedArgument,
    uint? HandoffCallReceiverPointer,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadWordGrowthGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? CallTargetRawFileOffset = null,
    uint? CallTargetVirtualAddress = null);

/// <summary>
/// Projects only the first guard and opaque handoff at raw 0xBE6306 through
/// the 0xBE6325 boundary. Captures are independent observations in instruction
/// order; this projection makes no identity, atomicity, or growth guarantees.
/// </summary>
public static class PayloadWordGrowthGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE6306;
    public const uint GateStartVirtualAddress = 0x00FE6F06;
    public const uint ReceiverDword8Offset = 0x08;
    public const uint ReceiverDword0COffset = 0x0C;
    public const uint ReceiverDword10Offset = 0x10;
    public const uint AlignmentAddend = 0x08;
    public const uint AlignmentShift = 3;

    public const uint ReceiverDword0CReadRawFileOffset = 0x00BE6306;
    public const uint ReceiverDword10ReadRawFileOffset = 0x00BE6309;
    public const uint SumRawFileOffset = 0x00BE630C;
    public const uint LowBitsTestRawFileOffset = 0x00BE630E;
    public const uint LowBitsBranchRawFileOffset = 0x00BE6310;
    public const uint AlignmentAddressRawFileOffset = 0x00BE6312;
    public const uint AlignmentShiftRawFileOffset = 0x00BE6315;
    public const uint ReceiverDword8CompareRawFileOffset = 0x00BE6318;
    public const uint ReceiverBoundBranchRawFileOffset = 0x00BE631B;
    public const uint HandoffPushRawFileOffset = 0x00BE631D;
    public const uint HandoffReceiverLoadRawFileOffset = 0x00BE631E;
    public const uint HandoffCallRawFileOffset = 0x00BE6320;
    public const uint HandoffCallReturnBoundaryRawFileOffset = 0x00BE6325;
    public const uint LowBitsBranchTargetRawFileOffset =
        HandoffCallReturnBoundaryRawFileOffset;
    public const uint ReceiverBoundBranchTargetRawFileOffset =
        HandoffCallReturnBoundaryRawFileOffset;
    public const uint HandoffCallTargetRawFileOffset = 0x00BE68A0;
    public const uint GateEndRawFileOffsetExclusive =
        HandoffCallReturnBoundaryRawFileOffset;

    public const uint GateStartVirtualAddressFromPeSection =
        0x00400000 + 0x00001000 + (GateStartRawFileOffset - 0x00000400);

    public const string CapturedGateBytes =
        "8B 46 0C 8B 4E 10 03 C1 A8 07 75 13 8D 41 08 C1 E8 03 "
        + "39 46 08 77 08 51 8B CE E8 7B 05 00 00";

    public static uint GateStartVirtualAddressFromRawOffset =>
        RawFileOffsetToVirtualAddress(GateStartRawFileOffset);

    public static uint HandoffCallTargetVirtualAddress =>
        RawFileOffsetToVirtualAddress(HandoffCallTargetRawFileOffset);

    public static ReadOnlyCollection<PayloadWordGrowthGateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadWordGrowthGateInstructionEvidence(
                "Capture the receiver DWORD at +0x0C",
                ReceiverDword0CReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword0CReadRawFileOffset),
                "8B 46 0C"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Capture the receiver DWORD at +0x10 into ECX",
                ReceiverDword10ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword10ReadRawFileOffset),
                "8B 4E 10"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Add the captured DWORD values with wrapping 32-bit arithmetic",
                SumRawFileOffset,
                RawFileOffsetToVirtualAddress(SumRawFileOffset),
                "03 C1"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Test the low three bits of AL",
                LowBitsTestRawFileOffset,
                RawFileOffsetToVirtualAddress(LowBitsTestRawFileOffset),
                "A8 07"),
            new PayloadWordGrowthGateInstructionEvidence(
                "A nonzero low-bit test branches to raw 0xBE6325",
                LowBitsBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(LowBitsBranchRawFileOffset),
                "75 13",
                LowBitsBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(LowBitsBranchTargetRawFileOffset)),
            new PayloadWordGrowthGateInstructionEvidence(
                "Form wrapping receiver+0x10+8 in EAX",
                AlignmentAddressRawFileOffset,
                RawFileOffsetToVirtualAddress(AlignmentAddressRawFileOffset),
                "8D 41 08"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Shift the 32-bit sum right by three",
                AlignmentShiftRawFileOffset,
                RawFileOffsetToVirtualAddress(AlignmentShiftRawFileOffset),
                "C1 E8 03"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Compare the captured receiver DWORD at +8 with EAX",
                ReceiverDword8CompareRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword8CompareRawFileOffset),
                "39 46 08"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Unsigned JA branches to raw 0xBE6325",
                ReceiverBoundBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverBoundBranchRawFileOffset),
                "77 08",
                ReceiverBoundBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverBoundBranchTargetRawFileOffset)),
            new PayloadWordGrowthGateInstructionEvidence(
                "Push the captured receiver DWORD from +0x10",
                HandoffPushRawFileOffset,
                RawFileOffsetToVirtualAddress(HandoffPushRawFileOffset),
                "51"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Load the receiver pointer into ECX",
                HandoffReceiverLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(HandoffReceiverLoadRawFileOffset),
                "8B CE"),
            new PayloadWordGrowthGateInstructionEvidence(
                "Call the opaque target at raw 0xBE68A0; normal return resumes at 0xBE6325",
                HandoffCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HandoffCallRawFileOffset),
                "E8 7B 05 00 00",
                CallTargetRawFileOffset: HandoffCallTargetRawFileOffset,
                CallTargetVirtualAddress: RawFileOffsetToVirtualAddress(
                    HandoffCallTargetRawFileOffset))
        });

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static PayloadWordGrowthGatePlan Project(
        PayloadWordGrowthGateCapture capture)
    {
        if (capture.ReceiverDword0C is not uint receiverDword0C)
        {
            return Stop(
                capture,
                HasCaptureAfterReceiverDword0C(capture)
                    ? PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
                    : PayloadWordGrowthGateOutcome.MissingReceiverDword0CCapture,
                stopAt: ReceiverDword0CReadRawFileOffset);
        }

        uint receiverDword0CAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword0COffset);
        if (capture.ReceiverDword10 is not uint receiverDword10)
        {
            return Stop(
                capture,
                capture.ReceiverDword8 is not null
                    || capture.HandoffCallReturnedNormally is not null
                    ? PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
                    : PayloadWordGrowthGateOutcome.MissingReceiverDword10Capture,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                receiverDword10Address: ReceiverDwordAddress(
                    capture.ReceiverPointer,
                    ReceiverDword10Offset),
                stopAt: ReceiverDword10ReadRawFileOffset);
        }

        uint receiverDword10Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword10Offset);
        uint sum32 = unchecked(receiverDword0C + receiverDword10);
        byte sumLowThreeBits = (byte)(sum32 & 0x07);
        if (sumLowThreeBits != 0)
        {
            return Stop(
                capture,
                capture.ReceiverDword8 is not null
                    || capture.HandoffCallReturnedNormally is not null
                    ? PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
                    : PayloadWordGrowthGateOutcome.LowBitsBranchToBoundary,
                receiverDword0CAddress,
                receiverDword0CRead: true,
                receiverDword10Address,
                receiverDword10Read: true,
                sum32,
                sumLowThreeBits,
                route: LowBitsBranchTargetRawFileOffset,
                stopAt: GateEndRawFileOffsetExclusive);
        }

        uint alignedReceiverDword10PlusEight =
            unchecked(receiverDword10 + AlignmentAddend) >> (int)AlignmentShift;
        uint receiverDword8Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword8Offset);
        if (capture.ReceiverDword8 is not uint receiverDword8)
        {
            return Stop(
                capture,
                capture.HandoffCallReturnedNormally is not null
                    ? PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
                    : PayloadWordGrowthGateOutcome.MissingReceiverDword8Capture,
                receiverDword0CAddress,
                receiverDword0CRead: true,
                receiverDword10Address,
                receiverDword10Read: true,
                sum32,
                sumLowThreeBits,
                receiverDword8Address,
                alignedReceiverDword10PlusEight: alignedReceiverDword10PlusEight,
                stopAt: ReceiverDword8CompareRawFileOffset);
        }

        bool receiverBoundBranchTaken =
            receiverDword8 > alignedReceiverDword10PlusEight;
        if (receiverBoundBranchTaken)
        {
            return Stop(
                capture,
                capture.HandoffCallReturnedNormally is not null
                    ? PayloadWordGrowthGateOutcome.UnreachedCaptureProvided
                    : PayloadWordGrowthGateOutcome.ReceiverBoundBranchesToBoundary,
                receiverDword0CAddress,
                receiverDword0CRead: true,
                receiverDword10Address,
                receiverDword10Read: true,
                sum32,
                sumLowThreeBits,
                receiverDword8Address,
                receiverDword8Read: true,
                alignedReceiverDword10PlusEight,
                receiverBoundBranchTaken: true,
                route: ReceiverBoundBranchTargetRawFileOffset,
                stopAt: GateEndRawFileOffsetExclusive);
        }

        if (capture.HandoffCallReturnedNormally is not bool callReturnedNormally)
        {
            return Stop(
                capture,
                PayloadWordGrowthGateOutcome.MissingHandoffCallReturnCapture,
                receiverDword0CAddress,
                receiverDword0CRead: true,
                receiverDword10Address,
                receiverDword10Read: true,
                sum32,
                sumLowThreeBits,
                receiverDword8Address,
                receiverDword8Read: true,
                alignedReceiverDword10PlusEight,
                receiverBoundBranchTaken: false,
                handoffCallReached: true,
                handoffCallPushedArgument: receiverDword10,
                handoffCallReceiverPointer: capture.ReceiverPointer,
                stopAt: HandoffCallReturnBoundaryRawFileOffset);
        }

        return Stop(
            capture,
            callReturnedNormally
                ? PayloadWordGrowthGateOutcome.HandoffCallReturnedNormally
                : PayloadWordGrowthGateOutcome.HandoffCallDidNotReturnNormally,
            receiverDword0CAddress,
            receiverDword0CRead: true,
            receiverDword10Address,
            receiverDword10Read: true,
            sum32,
            sumLowThreeBits,
            receiverDword8Address,
            receiverDword8Read: true,
            alignedReceiverDword10PlusEight,
            receiverBoundBranchTaken: false,
            handoffCallReached: true,
            handoffCallPushedArgument: receiverDword10,
            handoffCallReceiverPointer: capture.ReceiverPointer,
            route: callReturnedNormally
                ? HandoffCallReturnBoundaryRawFileOffset
                : null,
            stopAt: HandoffCallReturnBoundaryRawFileOffset);
    }

    private static bool HasCaptureAfterReceiverDword0C(
        PayloadWordGrowthGateCapture capture) =>
        capture.ReceiverDword10 is not null
        || capture.ReceiverDword8 is not null
        || capture.HandoffCallReturnedNormally is not null;

    private static PayloadWordGrowthGatePlan Stop(
        PayloadWordGrowthGateCapture capture,
        PayloadWordGrowthGateOutcome outcome,
        uint? receiverDword0CAddress = null,
        bool receiverDword0CRead = false,
        uint? receiverDword10Address = null,
        bool receiverDword10Read = false,
        uint? sum32 = null,
        byte? sumLowThreeBits = null,
        uint? receiverDword8Address = null,
        bool receiverDword8Read = false,
        uint? alignedReceiverDword10PlusEight = null,
        bool? receiverBoundBranchTaken = null,
        bool handoffCallReached = false,
        uint? handoffCallPushedArgument = null,
        uint? handoffCallReceiverPointer = null,
        uint? route = null,
        uint stopAt = GateStartRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword0CAddress,
            receiverDword0CRead,
            receiverDword10Address,
            receiverDword10Read,
            sum32,
            sumLowThreeBits,
            receiverDword8Address,
            receiverDword8Read,
            alignedReceiverDword10PlusEight,
            receiverBoundBranchTaken,
            handoffCallReached,
            handoffCallPushedArgument,
            handoffCallReceiverPointer,
            route,
            stopAt);
}
