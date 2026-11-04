using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthTakenSecondCopyCapture(
    uint ReceiverPointer,
    uint FramePointer,
    uint EntryEdi,
    bool? PriorCallReturnedNormally,
    uint? ReceiverDword4,
    uint? FrameDwordMinusC,
    uint? FrameDwordMinus4,
    bool? SecondCallReturnedNormally);

public enum PayloadGrowthTakenSecondCopyOutcome
{
    PriorCallReturnStatusNotCaptured,
    PriorCallDidNotReturn,
    MissingReceiverDword4Capture,
    MissingFrameDwordMinusCCapture,
    MissingFrameDwordMinus4Capture,
    UnreachedCaptureProvided,
    SecondCallReturnStatusNotCaptured,
    SecondCallDidNotReturn,
    ReturnedNormally
}

public readonly record struct PayloadGrowthTakenSecondCopyPlan
{
    public PayloadGrowthTakenSecondCopyCapture Capture { get; init; }
    public PayloadGrowthTakenSecondCopyOutcome Outcome { get; init; }

    public uint? PriorCallArgumentBytesStillPending { get; init; }

    public uint ReceiverDword4ReadAddress { get; init; }
    public bool ReceiverDword4Read { get; init; }
    public uint? EaxReloadedFromReceiverDword4 { get; init; }

    public uint FrameDwordMinusCReadAddress { get; init; }
    public bool FrameDwordMinusCRead { get; init; }
    public uint? FrameDwordMinusCReadValue { get; init; }

    public uint? SourceEcx { get; init; }
    public uint? EdxAfterSubtract { get; init; }
    public uint? EdxAfterAdd { get; init; }
    public int? SignedArithmeticShiftResult { get; init; }
    public uint? SignedArithmeticShiftResultBits { get; init; }
    public uint? CountEbx { get; init; }

    public bool CountPushed { get; init; }
    public bool SourcePushed { get; init; }
    public bool DestinationPushed { get; init; }
    public uint SecondCallArgumentBytesPushed { get; init; }

    public uint FrameDwordMinus4ReadAddress { get; init; }
    public bool FrameDwordMinus4Read { get; init; }
    public uint? FrameDwordMinus4ReadValue { get; init; }

    public uint? CallEntryEsp4 { get; init; }
    public uint? CallEntryEsp8 { get; init; }
    public uint? CallEntryEspC { get; init; }
    public uint? PendingArgumentBytesAtSecondCallEntry { get; init; }

    public uint CallRawFileOffset { get; init; }
    public bool CallReached { get; init; }
    public bool? CallReturnedNormally { get; init; }
    public uint? NormalBoundaryRawFileOffsetExclusive { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

public readonly record struct PayloadGrowthTakenSecondCopyInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the taken-path second-copy argument setup after the prior call's
/// normal return, stopping before the cleanup instruction at raw 0xBE69DF.
/// Captures are independent observations; this projection makes no claim about
/// the opaque call's target, memory access validity, copy semantics, or ownership.
/// </summary>
public static class PayloadGrowthTakenSecondCopyProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint PriorCallRawFileOffset = 0x00BE69BB;
    public const uint StartRawFileOffset = 0x00BE69BD;
    public const uint SecondCallRawFileOffset = 0x00BE69D9;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE69DF;
    public const uint CapturedCodeLength =
        NormalBoundaryRawFileOffsetExclusive - StartRawFileOffset;

    public const uint ReceiverDword4Offset = 0x04;
    public const uint FrameDwordMinusCOffset = 0x0C;
    public const uint FrameDwordMinus4Offset = 0x04;
    public const uint StackArgumentDestinationOffset = 0x04;
    public const uint StackArgumentSourceOffset = 0x08;
    public const uint StackArgumentCountOffset = 0x0C;
    public const uint PendingPriorCallArgumentBytes = 12;
    public const uint SecondCallArgumentBytes = 12;

    public const string CapturedCodeBytes =
        "8B 43 04 8B 55 F4 8D 0C 07 2B D1 03 D0 C1 FA 02 "
        + "8D 1C 95 00 00 00 00 53 51 FF 75 FC FF 15 9C 48 AC 01";

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static ReadOnlyCollection<PayloadGrowthTakenSecondCopyInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Overwrite prior-call EAX by reloading the receiver DWORD at EBX+4",
                0x00BE69BD,
                0x00FE75BD,
                "8B 43 04"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Read the independent DWORD at framePointer-0x0C into EDX",
                0x00BE69C0,
                0x00FE75C0,
                "8B 55 F4"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Form source ECX as entry EDI plus the reloaded DWORD with DWORD wrap",
                0x00BE69C3,
                0x00FE75C3,
                "8D 0C 07"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Subtract source ECX from EDX with DWORD wrap",
                0x00BE69C6,
                0x00FE75C6,
                "2B D1"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Add the reloaded receiver DWORD to EDX with DWORD wrap",
                0x00BE69C8,
                0x00FE75C8,
                "03 D0"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Arithmetic-shift EDX right by two as a signed DWORD",
                0x00BE69CA,
                0x00FE75CA,
                "C1 FA 02"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Scale the shifted EDX by four with DWORD wrap to form EBX",
                0x00BE69CD,
                0x00FE75CD,
                "8D 1C 95 00 00 00 00"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Push EBX as the count",
                0x00BE69D4,
                0x00FE75D4,
                "53"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Push ECX as the source",
                0x00BE69D5,
                0x00FE75D5,
                "51"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Independently read and push framePointer-4 as the destination",
                0x00BE69D6,
                0x00FE75D6,
                "FF 75 FC"),
            new PayloadGrowthTakenSecondCopyInstructionEvidence(
                "Call indirectly; the normal-return boundary is raw 0xBE69DF",
                SecondCallRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondCallRawFileOffset),
                "FF 15 9C 48 AC 01")
        });

    public static PayloadGrowthTakenSecondCopyPlan Project(
        PayloadGrowthTakenSecondCopyCapture capture)
    {
        PayloadGrowthTakenSecondCopyPlan plan = new()
        {
            Capture = capture,
            ReceiverDword4ReadAddress = ReceiverDwordAddress(
                capture.ReceiverPointer,
                ReceiverDword4Offset),
            FrameDwordMinusCReadAddress = FrameDwordAddress(
                capture.FramePointer,
                FrameDwordMinusCOffset),
            FrameDwordMinus4ReadAddress = FrameDwordAddress(
                capture.FramePointer,
                FrameDwordMinus4Offset),
            StopAtRawFileOffsetExclusive = PriorCallRawFileOffset
        };

        if (capture.PriorCallReturnedNormally is not bool priorCallReturnedNormally)
        {
            return plan with
            {
                Outcome = HasCaptureAfterPriorCall(capture)
                    ? PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthTakenSecondCopyOutcome.PriorCallReturnStatusNotCaptured
            };
        }

        if (!priorCallReturnedNormally)
        {
            return plan with
            {
                Outcome = HasCaptureAfterPriorCall(capture)
                    ? PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthTakenSecondCopyOutcome.PriorCallDidNotReturn
            };
        }

        plan = plan with
        {
            PriorCallArgumentBytesStillPending = PendingPriorCallArgumentBytes,
            StopAtRawFileOffsetExclusive = StartRawFileOffset
        };

        if (capture.ReceiverDword4 is not uint receiverDword4)
        {
            return plan with
            {
                Outcome = HasCaptureAfterReceiverDword4(capture)
                    ? PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthTakenSecondCopyOutcome.MissingReceiverDword4Capture
            };
        }

        plan = plan with
        {
            ReceiverDword4Read = true,
            EaxReloadedFromReceiverDword4 = receiverDword4,
            StopAtRawFileOffsetExclusive = 0x00BE69C0
        };

        if (capture.FrameDwordMinusC is not uint frameDwordMinusC)
        {
            return plan with
            {
                Outcome = HasCaptureAfterFrameDwordMinusC(capture)
                    ? PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthTakenSecondCopyOutcome.MissingFrameDwordMinusCCapture
            };
        }

        uint sourceEcx = unchecked(capture.EntryEdi + receiverDword4);
        uint edxAfterSubtract = unchecked(frameDwordMinusC - sourceEcx);
        uint edxAfterAdd = unchecked(edxAfterSubtract + receiverDword4);
        int signedArithmeticShiftResult = unchecked((int)edxAfterAdd) >> 2;
        uint signedArithmeticShiftResultBits =
            unchecked((uint)signedArithmeticShiftResult);
        uint countEbx = unchecked(signedArithmeticShiftResultBits * 4);

        plan = plan with
        {
            FrameDwordMinusCRead = true,
            FrameDwordMinusCReadValue = frameDwordMinusC,
            SourceEcx = sourceEcx,
            EdxAfterSubtract = edxAfterSubtract,
            EdxAfterAdd = edxAfterAdd,
            SignedArithmeticShiftResult = signedArithmeticShiftResult,
            SignedArithmeticShiftResultBits = signedArithmeticShiftResultBits,
            CountEbx = countEbx,
            CountPushed = true,
            SourcePushed = true,
            SecondCallArgumentBytesPushed = 8,
            StopAtRawFileOffsetExclusive = 0x00BE69D6
        };

        if (capture.FrameDwordMinus4 is not uint frameDwordMinus4)
        {
            return plan with
            {
                Outcome = capture.SecondCallReturnedNormally is not null
                    ? PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthTakenSecondCopyOutcome.MissingFrameDwordMinus4Capture
            };
        }

        plan = plan with
        {
            FrameDwordMinus4Read = true,
            FrameDwordMinus4ReadValue = frameDwordMinus4,
            DestinationPushed = true,
            SecondCallArgumentBytesPushed = SecondCallArgumentBytes,
            CallEntryEsp4 = frameDwordMinus4,
            CallEntryEsp8 = sourceEcx,
            CallEntryEspC = countEbx,
            StopAtRawFileOffsetExclusive = SecondCallRawFileOffset
        };

        if (capture.SecondCallReturnedNormally is not bool secondCallReturnedNormally)
        {
            return plan with
            {
                Outcome = PayloadGrowthTakenSecondCopyOutcome.SecondCallReturnStatusNotCaptured,
                CallRawFileOffset = SecondCallRawFileOffset,
                CallReached = true,
                PendingArgumentBytesAtSecondCallEntry =
                    PendingPriorCallArgumentBytes + SecondCallArgumentBytes
            };
        }

        if (!secondCallReturnedNormally)
        {
            return plan with
            {
                Outcome = PayloadGrowthTakenSecondCopyOutcome.SecondCallDidNotReturn,
                CallRawFileOffset = SecondCallRawFileOffset,
                CallReached = true,
                CallReturnedNormally = false,
                PendingArgumentBytesAtSecondCallEntry =
                    PendingPriorCallArgumentBytes + SecondCallArgumentBytes
            };
        }

        return plan with
        {
            Outcome = PayloadGrowthTakenSecondCopyOutcome.ReturnedNormally,
            CallRawFileOffset = SecondCallRawFileOffset,
            CallReached = true,
            CallReturnedNormally = true,
            PendingArgumentBytesAtSecondCallEntry =
                PendingPriorCallArgumentBytes + SecondCallArgumentBytes,
            NormalBoundaryRawFileOffsetExclusive =
                NormalBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive = NormalBoundaryRawFileOffsetExclusive
        };
    }

    private static bool HasCaptureAfterPriorCall(
        PayloadGrowthTakenSecondCopyCapture capture) =>
        capture.ReceiverDword4 is not null
        || capture.FrameDwordMinusC is not null
        || capture.FrameDwordMinus4 is not null
        || capture.SecondCallReturnedNormally is not null;

    private static bool HasCaptureAfterReceiverDword4(
        PayloadGrowthTakenSecondCopyCapture capture) =>
        capture.FrameDwordMinusC is not null
        || capture.FrameDwordMinus4 is not null
        || capture.SecondCallReturnedNormally is not null;

    private static bool HasCaptureAfterFrameDwordMinusC(
        PayloadGrowthTakenSecondCopyCapture capture) =>
        capture.FrameDwordMinus4 is not null
        || capture.SecondCallReturnedNormally is not null;
}
