using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthFirstCopyCapture(
    uint ReceiverPointer,
    uint FramePointer,
    uint RetainedEdi,
    uint? ReceiverDword4,
    uint? ReceiverDword8,
    uint? FrameDwordMinus4,
    uint? FrameDwordMinusC,
    uint? CallReturnEax,
    bool? CallReturnedNormally = null);

public enum PayloadGrowthFirstCopyOutcome
{
    MissingReceiverDword4Capture,
    MissingReceiverDword8Capture,
    MissingFrameDwordMinus4Capture,
    MissingFrameDwordMinusCCapture,
    UnreachedCaptureProvided,
    CallReturnStatusNotCaptured,
    CallDidNotReturn,
    ReturnedNormally
}

public readonly record struct PayloadGrowthFirstCopyPlan(
    PayloadGrowthFirstCopyCapture Capture,
    PayloadGrowthFirstCopyOutcome Outcome,
    uint ReceiverDword4ReadAddress,
    bool ReceiverDword4Read,
    uint FrameDwordMinusCWriteAddress,
    uint? FrameDwordMinusCWriteValue,
    uint? ReceiverDword8ReadAddress,
    bool ReceiverDword8Read,
    uint? FrameDwordMinus4ReadAddress,
    bool FrameDwordMinus4Read,
    uint? FrameDwordMinusCReadAddress,
    bool FrameDwordMinusCRead,
    uint? SourceEdx,
    uint? ReceiverDword8ShiftedLeft2,
    uint? SubtractSourceResult,
    uint? AddReceiverDword4Result,
    int? SignedArithmeticShiftResult,
    uint? SignedArithmeticShiftResultBits,
    uint? DestinationEax,
    uint? ByteCountEdi,
    uint? CountStackArgument,
    uint? SourceStackArgument,
    uint? DestinationStackArgument,
    uint? CallEntryEsp4,
    uint? CallEntryEsp8,
    uint? CallEntryEspC,
    bool CallReached,
    bool? CallReturnedNormally,
    uint? CallReturnEax,
    uint? NormalBoundaryRawFileOffsetExclusive,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadGrowthFirstCopyInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the confirmed first-copy argument setup at raw 0xBE691A through
/// the first opaque call ending at raw 0xBE694D.
/// </summary>
public static class PayloadGrowthFirstCopyProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint StartRawFileOffset = 0x00BE691A;
    public const uint FirstOpaqueCallRawFileOffset = 0x00BE6947;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE694D;
    public const uint CapturedCodeLength =
        NormalBoundaryRawFileOffsetExclusive - StartRawFileOffset;

    public const uint ReceiverDword4Offset = 0x04;
    public const uint ReceiverDword8Offset = 0x08;
    public const uint FrameDwordMinus4Offset = 0x04;
    public const uint FrameDwordMinusCOffset = 0x0C;
    public const uint StackArgumentCountOffset = 0x0C;
    public const uint StackArgumentSourceOffset = 0x08;
    public const uint StackArgumentDestinationOffset = 0x04;

    public const string CapturedCodeBytes =
        "8B 4B 04 8D 04 BD 00 00 00 00 89 45 F4 8D 14 08 "
        + "8B 43 08 C1 E0 02 2B C2 03 C1 C1 F8 02 "
        + "8D 3C 85 00 00 00 00 8B 45 FC 03 45 F4 57 52 50 "
        + "FF 15 9C 48 AC 01";

    public static uint StartVirtualAddress =>
        RawFileOffsetToVirtualAddress(StartRawFileOffset);

    public static ReadOnlyCollection<PayloadGrowthFirstCopyInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Read the receiver DWORD at +4 into ECX",
                0x00BE691A,
                0x00FE751A,
                "8B 4B 04"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Scale retained EDI by four with wrapping DWORD arithmetic",
                0x00BE691D,
                0x00FE751D,
                "8D 04 BD 00 00 00 00"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Write the scaled retained EDI value to framePointer-0x0C",
                0x00BE6924,
                0x00FE7524,
                "89 45 F4"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Form EDX from the frame-minus-C write value plus receiver DWORD +4",
                0x00BE6927,
                0x00FE7527,
                "8D 14 08"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Read the receiver DWORD at +8 into EAX",
                0x00BE692A,
                0x00FE752A,
                "8B 43 08"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Shift the captured receiver DWORD left by two with DWORD wrap",
                0x00BE692D,
                0x00FE752D,
                "C1 E0 02"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Subtract EDX, then add the captured receiver DWORD +4",
                0x00BE6930,
                0x00FE7530,
                "2B C2 03 C1"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Arithmetic-shift EAX right by two as a signed DWORD",
                0x00BE6934,
                0x00FE7534,
                "C1 F8 02"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Scale the shifted EAX by four with wrapping DWORD arithmetic for EDI",
                0x00BE6937,
                0x00FE7537,
                "8D 3C 85 00 00 00 00"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Independently read the DWORD at framePointer-4 into EAX",
                0x00BE693E,
                0x00FE753E,
                "8B 45 FC"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Independently reload framePointer-0x0C and add it to EAX",
                0x00BE6941,
                0x00FE7541,
                "03 45 F4"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Push count, source, then destination",
                0x00BE6944,
                0x00FE7544,
                "57 52 50"),
            new PayloadGrowthFirstCopyInstructionEvidence(
                "Call the first opaque function through its absolute pointer",
                FirstOpaqueCallRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstOpaqueCallRawFileOffset),
                "FF 15 9C 48 AC 01")
        });

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static uint ScaleByFour(uint value) =>
        unchecked(value * 4);

    public static uint ArithmeticShiftRightTwoBits(uint value) =>
        unchecked((uint)(unchecked((int)value) >> 2));

    public static PayloadGrowthFirstCopyPlan Project(
        PayloadGrowthFirstCopyCapture capture)
    {
        uint receiverDword4ReadAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword4Offset);
        uint frameDwordMinusCAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinusCOffset);
        if (capture.ReceiverDword4 is not uint receiverDword4)
        {
            return Stop(
                capture,
                HasCaptureAfterReceiverDword4(capture)
                    ? PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthFirstCopyOutcome.MissingReceiverDword4Capture,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                stopAt: StartRawFileOffset);
        }

        uint frameDwordMinusCWriteValue = ScaleByFour(capture.RetainedEdi);
        uint sourceEdx = unchecked(frameDwordMinusCWriteValue + receiverDword4);
        if (capture.ReceiverDword8 is not uint receiverDword8)
        {
            return Stop(
                capture,
                HasCaptureAfterReceiverDword8(capture)
                    ? PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthFirstCopyOutcome.MissingReceiverDword8Capture,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                receiverDword4Read: true,
                frameDwordMinusCWriteValue: frameDwordMinusCWriteValue,
                sourceEdx: sourceEdx,
                receiverDword8ReadAddress: ReceiverDwordAddress(
                    capture.ReceiverPointer,
                    ReceiverDword8Offset),
                stopAt: ReceiverDword8ReadRawFileOffset);
        }

        uint receiverDword8ShiftedLeft2 = ScaleByFour(receiverDword8);
        uint subtractSourceResult = unchecked(receiverDword8ShiftedLeft2 - sourceEdx);
        uint addReceiverDword4Result = unchecked(subtractSourceResult + receiverDword4);
        int signedArithmeticShiftResult =
            unchecked((int)addReceiverDword4Result) >> 2;
        uint signedArithmeticShiftResultBits = unchecked((uint)signedArithmeticShiftResult);
        uint byteCountEdi = ScaleByFour(signedArithmeticShiftResultBits);
        uint frameDwordMinus4ReadAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus4Offset);
        if (capture.FrameDwordMinus4 is not uint frameDwordMinus4)
        {
            return Stop(
                capture,
                capture.FrameDwordMinusC is not null
                    || capture.CallReturnEax is not null
                    || capture.CallReturnedNormally is not null
                    ? PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthFirstCopyOutcome.MissingFrameDwordMinus4Capture,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                receiverDword4Read: true,
                frameDwordMinusCWriteValue: frameDwordMinusCWriteValue,
                sourceEdx: sourceEdx,
                receiverDword8ReadAddress: ReceiverDwordAddress(
                    capture.ReceiverPointer,
                    ReceiverDword8Offset),
                receiverDword8Read: true,
                frameDwordMinus4ReadAddress: frameDwordMinus4ReadAddress,
                receiverDword8ShiftedLeft2: receiverDword8ShiftedLeft2,
                subtractSourceResult: subtractSourceResult,
                addReceiverDword4Result: addReceiverDword4Result,
                signedArithmeticShiftResult: signedArithmeticShiftResult,
                signedArithmeticShiftResultBits: signedArithmeticShiftResultBits,
                byteCountEdi: byteCountEdi,
                stopAt: FrameDwordMinus4ReadRawFileOffset);
        }

        if (capture.FrameDwordMinusC is not uint frameDwordMinusC)
        {
            return Stop(
                capture,
                capture.CallReturnEax is not null
                    || capture.CallReturnedNormally is not null
                    ? PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthFirstCopyOutcome.MissingFrameDwordMinusCCapture,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                receiverDword4Read: true,
                frameDwordMinusCWriteValue: frameDwordMinusCWriteValue,
                sourceEdx: sourceEdx,
                receiverDword8ReadAddress: ReceiverDwordAddress(
                    capture.ReceiverPointer,
                    ReceiverDword8Offset),
                receiverDword8Read: true,
                frameDwordMinus4ReadAddress: frameDwordMinus4ReadAddress,
                frameDwordMinus4Read: true,
                frameDwordMinusCReadAddress: frameDwordMinusCAddress,
                receiverDword8ShiftedLeft2: receiverDword8ShiftedLeft2,
                subtractSourceResult: subtractSourceResult,
                addReceiverDword4Result: addReceiverDword4Result,
                signedArithmeticShiftResult: signedArithmeticShiftResult,
                signedArithmeticShiftResultBits: signedArithmeticShiftResultBits,
                byteCountEdi: byteCountEdi,
                stopAt: FrameDwordMinusCReadRawFileOffset);
        }

        uint destinationEax = unchecked(frameDwordMinus4 + frameDwordMinusC);
        uint receiverDword8ReadAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword8Offset);
        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            if (capture.CallReturnEax is not null)
            {
                return Stop(
                    capture,
                    PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided,
                    receiverDword4ReadAddress,
                    frameDwordMinusCAddress,
                    receiverDword4Read: true,
                    frameDwordMinusCWriteValue: frameDwordMinusCWriteValue,
                    sourceEdx: sourceEdx,
                    receiverDword8ReadAddress: receiverDword8ReadAddress,
                    receiverDword8Read: true,
                    frameDwordMinus4ReadAddress: frameDwordMinus4ReadAddress,
                    frameDwordMinus4Read: true,
                    frameDwordMinusCReadAddress: frameDwordMinusCAddress,
                    frameDwordMinusCRead: true,
                    receiverDword8ShiftedLeft2: receiverDword8ShiftedLeft2,
                    subtractSourceResult: subtractSourceResult,
                    addReceiverDword4Result: addReceiverDword4Result,
                    signedArithmeticShiftResult: signedArithmeticShiftResult,
                    signedArithmeticShiftResultBits: signedArithmeticShiftResultBits,
                    destinationEax: destinationEax,
                    byteCountEdi: byteCountEdi,
                    callReached: true,
                    stopAt: FirstOpaqueCallRawFileOffset);
            }

            return CompleteCall(
                capture,
                PayloadGrowthFirstCopyOutcome.CallReturnStatusNotCaptured,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                frameDwordMinusCWriteValue,
                receiverDword8ReadAddress,
                frameDwordMinus4ReadAddress,
                sourceEdx,
                receiverDword8ShiftedLeft2,
                subtractSourceResult,
                addReceiverDword4Result,
                signedArithmeticShiftResult,
                signedArithmeticShiftResultBits,
                destinationEax,
                byteCountEdi,
                callReturnedNormally: null,
                callReturnEax: null,
                normalBoundary: null,
                stopAt: FirstOpaqueCallRawFileOffset);
        }

        if (!callReturnedNormally)
        {
            if (capture.CallReturnEax is not null)
            {
                return Stop(
                    capture,
                    PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided,
                    receiverDword4ReadAddress,
                    frameDwordMinusCAddress,
                    receiverDword4Read: true,
                    frameDwordMinusCWriteValue: frameDwordMinusCWriteValue,
                    sourceEdx: sourceEdx,
                    receiverDword8ReadAddress: receiverDword8ReadAddress,
                    receiverDword8Read: true,
                    frameDwordMinus4ReadAddress: frameDwordMinus4ReadAddress,
                    frameDwordMinus4Read: true,
                    frameDwordMinusCReadAddress: frameDwordMinusCAddress,
                    frameDwordMinusCRead: true,
                    receiverDword8ShiftedLeft2: receiverDword8ShiftedLeft2,
                    subtractSourceResult: subtractSourceResult,
                    addReceiverDword4Result: addReceiverDword4Result,
                    signedArithmeticShiftResult: signedArithmeticShiftResult,
                    signedArithmeticShiftResultBits: signedArithmeticShiftResultBits,
                    destinationEax: destinationEax,
                    byteCountEdi: byteCountEdi,
                    callReached: true,
                    callReturnedNormally: false,
                    stopAt: FirstOpaqueCallRawFileOffset);
            }

            return CompleteCall(
                capture,
                PayloadGrowthFirstCopyOutcome.CallDidNotReturn,
                receiverDword4ReadAddress,
                frameDwordMinusCAddress,
                frameDwordMinusCWriteValue,
                receiverDword8ReadAddress,
                frameDwordMinus4ReadAddress,
                sourceEdx,
                receiverDword8ShiftedLeft2,
                subtractSourceResult,
                addReceiverDword4Result,
                signedArithmeticShiftResult,
                signedArithmeticShiftResultBits,
                destinationEax,
                byteCountEdi,
                callReturnedNormally: false,
                callReturnEax: null,
                normalBoundary: null,
                stopAt: FirstOpaqueCallRawFileOffset);
        }

        return CompleteCall(
            capture,
            PayloadGrowthFirstCopyOutcome.ReturnedNormally,
            receiverDword4ReadAddress,
            frameDwordMinusCAddress,
            frameDwordMinusCWriteValue,
            receiverDword8ReadAddress,
            frameDwordMinus4ReadAddress,
            sourceEdx,
            receiverDword8ShiftedLeft2,
            subtractSourceResult,
            addReceiverDword4Result,
            signedArithmeticShiftResult,
            signedArithmeticShiftResultBits,
            destinationEax,
            byteCountEdi,
            callReturnedNormally: true,
            callReturnEax: capture.CallReturnEax,
            normalBoundary: NormalBoundaryRawFileOffsetExclusive,
            stopAt: NormalBoundaryRawFileOffsetExclusive);
    }

    private static bool HasCaptureAfterReceiverDword4(
        PayloadGrowthFirstCopyCapture capture) =>
        capture.ReceiverDword8 is not null
        || capture.FrameDwordMinus4 is not null
        || capture.FrameDwordMinusC is not null
        || capture.CallReturnEax is not null
        || capture.CallReturnedNormally is not null;

    private static bool HasCaptureAfterReceiverDword8(
        PayloadGrowthFirstCopyCapture capture) =>
        capture.FrameDwordMinus4 is not null
        || capture.FrameDwordMinusC is not null
        || capture.CallReturnEax is not null
        || capture.CallReturnedNormally is not null;

    private static PayloadGrowthFirstCopyPlan CompleteCall(
        PayloadGrowthFirstCopyCapture capture,
        PayloadGrowthFirstCopyOutcome outcome,
        uint receiverDword4ReadAddress,
        uint frameDwordMinusCAddress,
        uint frameDwordMinusCWriteValue,
        uint receiverDword8ReadAddress,
        uint frameDwordMinus4ReadAddress,
        uint sourceEdx,
        uint receiverDword8ShiftedLeft2,
        uint subtractSourceResult,
        uint addReceiverDword4Result,
        int signedArithmeticShiftResult,
        uint signedArithmeticShiftResultBits,
        uint destinationEax,
        uint byteCountEdi,
        bool? callReturnedNormally,
        uint? callReturnEax,
        uint? normalBoundary,
        uint stopAt) =>
        new(
            capture,
            outcome,
            receiverDword4ReadAddress,
            true,
            frameDwordMinusCAddress,
            frameDwordMinusCWriteValue,
            receiverDword8ReadAddress,
            true,
            frameDwordMinus4ReadAddress,
            true,
            frameDwordMinusCAddress,
            true,
            sourceEdx,
            receiverDword8ShiftedLeft2,
            subtractSourceResult,
            addReceiverDword4Result,
            signedArithmeticShiftResult,
            signedArithmeticShiftResultBits,
            destinationEax,
            byteCountEdi,
            byteCountEdi,
            sourceEdx,
            destinationEax,
            destinationEax,
            sourceEdx,
            byteCountEdi,
            true,
            callReturnedNormally,
            callReturnEax,
            normalBoundary,
            stopAt);

    private static PayloadGrowthFirstCopyPlan Stop(
        PayloadGrowthFirstCopyCapture capture,
        PayloadGrowthFirstCopyOutcome outcome,
        uint receiverDword4ReadAddress,
        uint frameDwordMinusCAddress,
        bool receiverDword4Read = false,
        uint? frameDwordMinusCWriteValue = null,
        uint? sourceEdx = null,
        uint? receiverDword8ReadAddress = null,
        bool receiverDword8Read = false,
        uint? frameDwordMinus4ReadAddress = null,
        bool frameDwordMinus4Read = false,
        uint? frameDwordMinusCReadAddress = null,
        bool frameDwordMinusCRead = false,
        uint? receiverDword8ShiftedLeft2 = null,
        uint? subtractSourceResult = null,
        uint? addReceiverDword4Result = null,
        int? signedArithmeticShiftResult = null,
        uint? signedArithmeticShiftResultBits = null,
        uint? destinationEax = null,
        uint? byteCountEdi = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        uint stopAt = StartRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword4ReadAddress,
            receiverDword4Read,
            frameDwordMinusCAddress,
            frameDwordMinusCWriteValue,
            receiverDword8ReadAddress,
            receiverDword8Read,
            frameDwordMinus4ReadAddress,
            frameDwordMinus4Read,
            frameDwordMinusCReadAddress,
            frameDwordMinusCRead,
            sourceEdx,
            receiverDword8ShiftedLeft2,
            subtractSourceResult,
            addReceiverDword4Result,
            signedArithmeticShiftResult,
            signedArithmeticShiftResultBits,
            destinationEax,
            byteCountEdi,
            null,
            null,
            null,
            null,
            null,
            null,
            callReached,
            callReturnedNormally,
            null,
            null,
            stopAt);

    private const uint ReceiverDword8ReadRawFileOffset = 0x00BE692A;
    private const uint FrameDwordMinus4ReadRawFileOffset = 0x00BE693E;
    private const uint FrameDwordMinusCReadRawFileOffset = 0x00BE6941;
}
