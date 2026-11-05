using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthPublicationCapture(
    uint FramePointer,
    uint RetainedEsiGrowthDelta,
    uint RetainedEbxPublishedPointer,
    uint? FrameDwordMinus14ReceiverPointer,
    uint? ReceiverDword4,
    bool? OpaqueCallReturnedNormally,
    uint? ReceiverDword8OldCounter);

public enum PayloadGrowthPublicationOutcome
{
    MissingFrameDwordMinus14Capture,
    MissingReceiverDword4Capture,
    UnreachedCaptureProvided,
    OpaqueCallReturnStatusNotCaptured,
    OpaqueCallDidNotReturnNormally,
    MissingReceiverDword8OldCounterCapture,
    ReturnedNormally
}

public readonly record struct PayloadGrowthPublicationPlan
{
    public PayloadGrowthPublicationCapture Capture { get; init; }
    public PayloadGrowthPublicationOutcome Outcome { get; init; }
    public bool? OpaqueCallSkipped { get; init; }
    public bool OpaqueCallReached { get; init; }
    public bool? OpaqueCallReturnedNormally { get; init; }
    public uint? OpaqueCallTargetAddress { get; init; }
    public uint? OpaqueCallEcxValue { get; init; }
    public uint? FrameDwordMinus14ReadAddress { get; init; }
    public bool FrameDwordMinus14Read { get; init; }
    public uint? ReceiverPointer { get; init; }
    public uint? ReceiverDword4ReadAddress { get; init; }
    public bool ReceiverDword4Read { get; init; }
    public uint? ReceiverDword4Value { get; init; }
    public uint? ReceiverDword8ReadAddress { get; init; }
    public bool ReceiverDword8Read { get; init; }
    public uint? ReceiverDword8OldCounter { get; init; }
    public uint? ReceiverDword8WriteAddress { get; init; }
    public bool ReceiverDword8Written { get; init; }
    public uint? ReceiverDword8WriteValue { get; init; }
    public uint? ReceiverDword4PointerWriteAddress { get; init; }
    public bool ReceiverDword4PointerWritten { get; init; }
    public uint? ReceiverDword4PointerWriteValue { get; init; }
    public bool Ret4Reached { get; init; }
    public uint? Ret4StackBytes { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

public readonly record struct PayloadGrowthPublicationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? CallTargetAddress = null);

/// <summary>
/// Projects the bounded receiver counter and pointer publication at raw
/// 0xBE69F2..0xBE6A10. The optional helper is opaque; no helper result,
/// ownership behavior, or success guarantee is inferred.
/// </summary>
public static class PayloadGrowthPublicationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint StartRawFileOffset = 0x00BE69F2;
    public const uint CounterAddRawFileOffset = 0x00BE6A01;
    public const uint PointerStoreRawFileOffset = 0x00BE6A04;
    public const uint OpaqueCallRawFileOffset = 0x00BE69FC;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6A10;
    public const uint CapturedCodeLength =
        NormalBoundaryRawFileOffsetExclusive - StartRawFileOffset;

    public const uint FrameDwordMinus14Offset = 0x14;
    public const uint ReceiverDword4Offset = 0x04;
    public const uint ReceiverDword8Offset = 0x08;
    public const uint Ret4StackBytes = 0x04;
    public const uint OpaqueCallTargetAddress = 0x12C8D60;

    public const uint JumpZeroRawFileOffset = 0x00BE69FA;
    public const uint JumpZeroTargetRawFileOffset = CounterAddRawFileOffset;

    public const string CapturedCodeBytes =
        "8B 7D EC 8B 4F 04 85 C9 74 05 E8 5F 23 6E 00 01 77 08 "
        + "89 5F 04 5F 5E 5B 8B E5 5D C2 04 00";

    public static uint StartVirtualAddress =>
        RawFileOffsetToVirtualAddress(StartRawFileOffset);

    public static uint NormalBoundaryVirtualAddressExclusive =>
        RawFileOffsetToVirtualAddress(NormalBoundaryRawFileOffsetExclusive);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static ReadOnlyCollection<PayloadGrowthPublicationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthPublicationInstructionEvidence(
                "Independently reload the receiver pointer from framePointer-0x14",
                StartRawFileOffset,
                RawFileOffsetToVirtualAddress(StartRawFileOffset),
                "8B 7D EC"),
            new PayloadGrowthPublicationInstructionEvidence(
                "Read receiver DWORD +4 into ECX",
                0x00BE69F5,
                RawFileOffsetToVirtualAddress(0x00BE69F5),
                "8B 4F 04"),
            new PayloadGrowthPublicationInstructionEvidence(
                "Test captured receiver DWORD +4",
                0x00BE69F8,
                RawFileOffsetToVirtualAddress(0x00BE69F8),
                "85 C9"),
            new PayloadGrowthPublicationInstructionEvidence(
                "JZ skips the opaque call when receiver DWORD +4 is zero",
                JumpZeroRawFileOffset,
                RawFileOffsetToVirtualAddress(JumpZeroRawFileOffset),
                "74 05",
                JumpZeroTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(JumpZeroTargetRawFileOffset)),
            new PayloadGrowthPublicationInstructionEvidence(
                "Call the opaque helper with captured ECX and no explicit stack push",
                OpaqueCallRawFileOffset,
                RawFileOffsetToVirtualAddress(OpaqueCallRawFileOffset),
                "E8 5F 23 6E 00",
                CallTargetAddress: OpaqueCallTargetAddress),
            new PayloadGrowthPublicationInstructionEvidence(
                "Add retained ESI to receiver DWORD +8 with DWORD wrap",
                CounterAddRawFileOffset,
                RawFileOffsetToVirtualAddress(CounterAddRawFileOffset),
                "01 77 08"),
            new PayloadGrowthPublicationInstructionEvidence(
                "Publish retained EBX to receiver DWORD +4 after the counter update",
                PointerStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(PointerStoreRawFileOffset),
                "89 5F 04"),
            new PayloadGrowthPublicationInstructionEvidence(
                "Restore EDI, ESI, EBX, ESP, and EBP",
                0x00BE6A07,
                RawFileOffsetToVirtualAddress(0x00BE6A07),
                "5F 5E 5B 8B E5 5D"),
            new PayloadGrowthPublicationInstructionEvidence(
                "Return and pop the four-byte argument",
                0x00BE6A0D,
                RawFileOffsetToVirtualAddress(0x00BE6A0D),
                "C2 04 00")
        });

    public static PayloadGrowthPublicationPlan Project(
        PayloadGrowthPublicationCapture capture)
    {
        uint frameDwordMinus14ReadAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus14Offset);

        if (capture.FrameDwordMinus14ReceiverPointer is not uint receiverPointer)
        {
            return Stop(
                capture,
                HasCaptureAfterFrameDwordMinus14(capture)
                    ? PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPublicationOutcome.MissingFrameDwordMinus14Capture,
                frameDwordMinus14ReadAddress);
        }

        uint receiverDword4ReadAddress = ReceiverDwordAddress(
            receiverPointer,
            ReceiverDword4Offset);
        if (capture.ReceiverDword4 is not uint receiverDword4)
        {
            return Stop(
                capture,
                capture.OpaqueCallReturnedNormally is not null
                    || capture.ReceiverDword8OldCounter is not null
                    ? PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPublicationOutcome.MissingReceiverDword4Capture,
                frameDwordMinus14ReadAddress,
                frameDwordMinus14ReceiverPointer: receiverPointer,
                receiverDword4ReadAddress: receiverDword4ReadAddress,
                stopAt: 0x00BE69F5);
        }

        bool callSkipped = receiverDword4 == 0;
        if (callSkipped && capture.OpaqueCallReturnedNormally is not null)
        {
            return Stop(
                capture,
                PayloadGrowthPublicationOutcome.UnreachedCaptureProvided,
                frameDwordMinus14ReadAddress,
                frameDwordMinus14ReceiverPointer: receiverPointer,
                receiverDword4ReadAddress: receiverDword4ReadAddress,
                receiverDword4: receiverDword4,
                opaqueCallSkipped: true);
        }

        if (!callSkipped && capture.OpaqueCallReturnedNormally is not bool)
        {
            return Stop(
                capture,
                capture.ReceiverDword8OldCounter is not null
                    ? PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPublicationOutcome.OpaqueCallReturnStatusNotCaptured,
                frameDwordMinus14ReadAddress,
                frameDwordMinus14ReceiverPointer: receiverPointer,
                receiverDword4ReadAddress: receiverDword4ReadAddress,
                receiverDword4: receiverDword4,
                opaqueCallSkipped: false,
                opaqueCallReached: true,
                opaqueCallTargetAddress: OpaqueCallTargetAddress,
                opaqueCallEcxValue: receiverDword4,
                stopAt: OpaqueCallRawFileOffset);
        }

        if (!callSkipped && capture.OpaqueCallReturnedNormally == false)
        {
            return Stop(
                capture,
                capture.ReceiverDword8OldCounter is not null
                    ? PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPublicationOutcome.OpaqueCallDidNotReturnNormally,
                frameDwordMinus14ReadAddress,
                frameDwordMinus14ReceiverPointer: receiverPointer,
                receiverDword4ReadAddress: receiverDword4ReadAddress,
                receiverDword4: receiverDword4,
                opaqueCallSkipped: false,
                opaqueCallReached: true,
                opaqueCallReturnedNormally: false,
                opaqueCallTargetAddress: OpaqueCallTargetAddress,
                opaqueCallEcxValue: receiverDword4,
                stopAt: OpaqueCallRawFileOffset);
        }

        uint receiverDword8ReadAddress = ReceiverDwordAddress(
            receiverPointer,
            ReceiverDword8Offset);
        if (capture.ReceiverDword8OldCounter is not uint oldCounter)
        {
            return Stop(
                capture,
                PayloadGrowthPublicationOutcome.MissingReceiverDword8OldCounterCapture,
                frameDwordMinus14ReadAddress,
                frameDwordMinus14ReceiverPointer: receiverPointer,
                receiverDword4ReadAddress: receiverDword4ReadAddress,
                receiverDword4: receiverDword4,
                receiverDword8ReadAddress: receiverDword8ReadAddress,
                opaqueCallSkipped: callSkipped,
                opaqueCallReached: !callSkipped,
                opaqueCallReturnedNormally: callSkipped ? null : true,
                opaqueCallTargetAddress: callSkipped ? null : OpaqueCallTargetAddress,
                opaqueCallEcxValue: callSkipped ? null : receiverDword4,
                stopAt: CounterAddRawFileOffset);
        }

        uint updatedCounter = unchecked(
            oldCounter + capture.RetainedEsiGrowthDelta);
        return new()
        {
            Capture = capture,
            Outcome = PayloadGrowthPublicationOutcome.ReturnedNormally,
            FrameDwordMinus14ReadAddress = frameDwordMinus14ReadAddress,
            FrameDwordMinus14Read = true,
            ReceiverPointer = receiverPointer,
            ReceiverDword4ReadAddress = receiverDword4ReadAddress,
            ReceiverDword4Read = true,
            ReceiverDword4Value = receiverDword4,
            ReceiverDword8ReadAddress = receiverDword8ReadAddress,
            ReceiverDword8Read = true,
            ReceiverDword8OldCounter = oldCounter,
            ReceiverDword8WriteAddress = receiverDword8ReadAddress,
            ReceiverDword8Written = true,
            ReceiverDword8WriteValue = updatedCounter,
            ReceiverDword4PointerWriteAddress = receiverDword4ReadAddress,
            ReceiverDword4PointerWritten = true,
            ReceiverDword4PointerWriteValue = capture.RetainedEbxPublishedPointer,
            OpaqueCallSkipped = callSkipped,
            OpaqueCallReached = !callSkipped,
            OpaqueCallReturnedNormally = callSkipped ? null : true,
            OpaqueCallTargetAddress = callSkipped ? null : OpaqueCallTargetAddress,
            OpaqueCallEcxValue = callSkipped ? null : receiverDword4,
            Ret4Reached = true,
            Ret4StackBytes = Ret4StackBytes,
            StopAtRawFileOffsetExclusive = NormalBoundaryRawFileOffsetExclusive
        };
    }

    private static bool HasCaptureAfterFrameDwordMinus14(
        PayloadGrowthPublicationCapture capture) =>
        capture.ReceiverDword4 is not null
        || capture.OpaqueCallReturnedNormally is not null
        || capture.ReceiverDword8OldCounter is not null;

    private static PayloadGrowthPublicationPlan Stop(
        PayloadGrowthPublicationCapture capture,
        PayloadGrowthPublicationOutcome outcome,
        uint frameDwordMinus14ReadAddress,
        uint? frameDwordMinus14ReceiverPointer = null,
        uint? receiverDword4ReadAddress = null,
        uint? receiverDword4 = null,
        uint? receiverDword8ReadAddress = null,
        bool? opaqueCallSkipped = null,
        bool opaqueCallReached = false,
        bool? opaqueCallReturnedNormally = null,
        uint? opaqueCallTargetAddress = null,
        uint? opaqueCallEcxValue = null,
        uint stopAt = StartRawFileOffset) =>
        new()
        {
            Capture = capture,
            Outcome = outcome,
            FrameDwordMinus14ReadAddress = frameDwordMinus14ReadAddress,
            FrameDwordMinus14Read = frameDwordMinus14ReceiverPointer is not null,
            ReceiverPointer = frameDwordMinus14ReceiverPointer,
            ReceiverDword4ReadAddress = receiverDword4ReadAddress,
            ReceiverDword4Read = receiverDword4 is not null,
            ReceiverDword4Value = receiverDword4,
            ReceiverDword8ReadAddress = receiverDword8ReadAddress,
            OpaqueCallSkipped = opaqueCallSkipped,
            OpaqueCallReached = opaqueCallReached,
            OpaqueCallReturnedNormally = opaqueCallReturnedNormally,
            OpaqueCallTargetAddress = opaqueCallTargetAddress,
            OpaqueCallEcxValue = opaqueCallEcxValue,
            StopAtRawFileOffsetExclusive = stopAt
        };
}
