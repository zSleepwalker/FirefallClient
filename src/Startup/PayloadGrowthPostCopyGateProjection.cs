using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthPostCopyGateCapture(
    uint ReceiverPointer,
    uint FramePointer,
    uint RetainedEdiByteCount,
    uint RetainedEsiGrowthDelta,
    bool? PriorCallReturnedNormally,
    uint? PriorCallEax,
    uint? ReceiverDword4,
    uint? FrameDwordMinus8);

public enum PayloadGrowthPostCopyGateOutcome
{
    PriorCallReturnStatusNotCaptured,
    PriorCallDidNotReturnNormally,
    MissingReceiverDword4Capture,
    MissingPriorCallEaxCapture,
    MissingFrameDwordMinus8Capture,
    UnreachedCaptureProvided,
    UnsignedGreaterThanBranchNotTaken,
    UnsignedGreaterThanBranchTaken
}

public readonly record struct PayloadGrowthPostCopyGatePlan(
    PayloadGrowthPostCopyGateCapture Capture,
    PayloadGrowthPostCopyGateOutcome Outcome,
    uint ReceiverDword4ReadAddress,
    bool ReceiverDword4Read,
    uint? ReceiverDword4Value,
    uint? StackCleanupBytes,
    uint? PriorCallEaxUsed,
    uint? Edx,
    uint? FrameDwordMinus8ReadAddress,
    bool FrameDwordMinus8Read,
    uint? FrameDwordMinus10WriteAddress,
    bool FrameDwordMinus10Written,
    uint? FrameDwordMinus10WriteValue,
    uint? ComparedEdi,
    uint? ComparedEsi,
    bool? UnsignedGreaterThan,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadGrowthPostCopyGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the bounded post-copy unsigned growth gate at raw 0xBE694D..0xBE6960.
/// The prior opaque call and its effects are represented only by separate captures.
/// </summary>
public static class PayloadGrowthPostCopyGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE694D;
    public const uint GateEndRawFileOffsetExclusive = 0x00BE6960;
    public const uint CapturedCodeLength =
        GateEndRawFileOffsetExclusive - GateStartRawFileOffset;
    public const uint ReceiverDword4Offset = 0x04;
    public const uint FrameDwordMinus8Offset = 0x08;
    public const uint FrameDwordMinus10Offset = 0x10;
    public const uint StackCleanupBytes = 0x0C;

    public const uint ReceiverDword4ReadRawFileOffset = 0x00BE694D;
    public const uint StackCleanupRawFileOffset = 0x00BE6950;
    public const uint PriorCallEaxUseRawFileOffset = 0x00BE6953;
    public const uint FrameDwordMinus8ReadRawFileOffset = 0x00BE6956;
    public const uint FrameDwordMinus10WriteRawFileOffset = 0x00BE6959;
    public const uint UnsignedCompareRawFileOffset = 0x00BE695C;
    public const uint UnsignedGreaterThanBranchRawFileOffset = 0x00BE695E;
    public const uint UnsignedGreaterThanBranchTargetRawFileOffset = 0x00BE69A7;
    public const uint FallthroughRawFileOffset = GateEndRawFileOffsetExclusive;

    public const string CapturedCodeBytes =
        "8B 4B 04 83 C4 0C 8D 14 07 8B 7D F8 89 55 F0 3B FE 77 47";

    public static uint GateStartVirtualAddress =>
        RawFileOffsetToVirtualAddress(GateStartRawFileOffset);

    public static uint GateEndVirtualAddressExclusive =>
        RawFileOffsetToVirtualAddress(GateEndRawFileOffsetExclusive);

    public static uint UnsignedGreaterThanBranchTargetVirtualAddress =>
        RawFileOffsetToVirtualAddress(UnsignedGreaterThanBranchTargetRawFileOffset);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static ReadOnlyCollection<PayloadGrowthPostCopyGateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Read receiver DWORD +4 into ECX",
                ReceiverDword4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword4ReadRawFileOffset),
                "8B 4B 04"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Clean up the prior call's three stack arguments after the receiver read",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 0C"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Form EDX from retained EDI byte count plus independently captured prior-call EAX",
                PriorCallEaxUseRawFileOffset,
                RawFileOffsetToVirtualAddress(PriorCallEaxUseRawFileOffset),
                "8D 14 07"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Independently read frame-8 into EDI",
                FrameDwordMinus8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameDwordMinus8ReadRawFileOffset),
                "8B 7D F8"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Store EDX to frame-10",
                FrameDwordMinus10WriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameDwordMinus10WriteRawFileOffset),
                "89 55 F0"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Compare captured frame-8 EDI against retained ESI as unsigned DWORDs",
                UnsignedCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(UnsignedCompareRawFileOffset),
                "3B FE"),
            new PayloadGrowthPostCopyGateInstructionEvidence(
                "Unsigned JA routes to raw 0xBE69A7; otherwise continue at raw 0xBE6960",
                UnsignedGreaterThanBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(UnsignedGreaterThanBranchRawFileOffset),
                "77 47",
                UnsignedGreaterThanBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(UnsignedGreaterThanBranchTargetRawFileOffset))
        });

    public static PayloadGrowthPostCopyGatePlan Project(
        PayloadGrowthPostCopyGateCapture capture)
    {
        uint receiverDword4ReadAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword4Offset);

        if (capture.PriorCallReturnedNormally is not bool priorCallReturnedNormally)
        {
            return Stop(
                capture,
                HasPostCallCapture(capture)
                    ? PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPostCopyGateOutcome.PriorCallReturnStatusNotCaptured,
                receiverDword4ReadAddress);
        }

        if (!priorCallReturnedNormally)
        {
            return Stop(
                capture,
                HasPostCallCapture(capture)
                    ? PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPostCopyGateOutcome.PriorCallDidNotReturnNormally,
                receiverDword4ReadAddress);
        }

        if (capture.ReceiverDword4 is not uint)
        {
            return Stop(
                capture,
                capture.PriorCallEax is not null || capture.FrameDwordMinus8 is not null
                    ? PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPostCopyGateOutcome.MissingReceiverDword4Capture,
                receiverDword4ReadAddress);
        }

        if (capture.PriorCallEax is not uint priorCallEax)
        {
            return Stop(
                capture,
                capture.FrameDwordMinus8 is not null
                    ? PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
                    : PayloadGrowthPostCopyGateOutcome.MissingPriorCallEaxCapture,
                receiverDword4ReadAddress,
                receiverDword4Read: true,
                receiverDword4Value: capture.ReceiverDword4,
                stackCleanupBytes: StackCleanupBytes,
                stopAt: PriorCallEaxUseRawFileOffset);
        }

        uint edx = unchecked(capture.RetainedEdiByteCount + priorCallEax);
        uint frameDwordMinus8ReadAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus8Offset);
        if (capture.FrameDwordMinus8 is not uint frameDwordMinus8)
        {
            return Stop(
                capture,
                PayloadGrowthPostCopyGateOutcome.MissingFrameDwordMinus8Capture,
                receiverDword4ReadAddress,
                receiverDword4Read: true,
                receiverDword4Value: capture.ReceiverDword4,
                stackCleanupBytes: StackCleanupBytes,
                priorCallEaxUsed: priorCallEax,
                edx: edx,
                frameDwordMinus8ReadAddress: frameDwordMinus8ReadAddress,
                stopAt: FrameDwordMinus8ReadRawFileOffset);
        }

        uint frameDwordMinus10WriteAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus10Offset);
        bool unsignedGreaterThan = frameDwordMinus8 > capture.RetainedEsiGrowthDelta;
        uint routeRawFileOffset = unsignedGreaterThan
            ? UnsignedGreaterThanBranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        return new(
            capture,
            unsignedGreaterThan
                ? PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchTaken
                : PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchNotTaken,
            receiverDword4ReadAddress,
            true,
            capture.ReceiverDword4,
            StackCleanupBytes,
            priorCallEax,
            edx,
            frameDwordMinus8ReadAddress,
            true,
            frameDwordMinus10WriteAddress,
            true,
            edx,
            frameDwordMinus8,
            capture.RetainedEsiGrowthDelta,
            unsignedGreaterThan,
            routeRawFileOffset,
            GateEndRawFileOffsetExclusive);
    }

    private static bool HasPostCallCapture(PayloadGrowthPostCopyGateCapture capture) =>
        capture.PriorCallEax is not null
        || capture.ReceiverDword4 is not null
        || capture.FrameDwordMinus8 is not null;

    private static PayloadGrowthPostCopyGatePlan Stop(
        PayloadGrowthPostCopyGateCapture capture,
        PayloadGrowthPostCopyGateOutcome outcome,
        uint receiverDword4ReadAddress,
        bool receiverDword4Read = false,
        uint? receiverDword4Value = null,
        uint? stackCleanupBytes = null,
        uint? priorCallEaxUsed = null,
        uint? edx = null,
        uint? frameDwordMinus8ReadAddress = null,
        bool frameDwordMinus8Read = false,
        uint? frameDwordMinus10WriteAddress = null,
        bool frameDwordMinus10Written = false,
        uint? frameDwordMinus10WriteValue = null,
        uint? comparedEdi = null,
        uint? comparedEsi = null,
        bool? unsignedGreaterThan = null,
        uint? routeRawFileOffset = null,
        uint stopAt = GateStartRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword4ReadAddress,
            receiverDword4Read,
            receiverDword4Value,
            stackCleanupBytes,
            priorCallEaxUsed,
            edx,
            frameDwordMinus8ReadAddress,
            frameDwordMinus8Read,
            frameDwordMinus10WriteAddress,
            frameDwordMinus10Written,
            frameDwordMinus10WriteValue,
            comparedEdi,
            comparedEsi,
            unsignedGreaterThan,
            routeRawFileOffset,
            stopAt);
}
