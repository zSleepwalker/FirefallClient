using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordSlotAllocationCapture(
    uint ReceiverPointer,
    uint RetainedScaledOffset,
    uint? PrecedingSlotDword,
    bool? AllocatorCallReturnedNormally,
    uint? AllocatorEax,
    bool? ZeroPathIndirectCallReturnedNormally,
    uint? EcxAfterIndirectCall,
    uint? ReceiverDword4AtSlotWrite);

public enum PayloadWordSlotAllocationOutcome
{
    MissingPrecedingSlotDwordCapture,
    PrecedingSlotDwordNonzero,
    UnreachedCaptureProvided,
    MissingAllocatorCallReturnCapture,
    AllocatorCallDidNotReturnNormally,
    MissingAllocatorEaxCapture,
    MissingZeroPathIndirectCallReturnCapture,
    ZeroPathIndirectCallDidNotReturnNormally,
    MissingEcxAfterIndirectCallCapture,
    MissingReceiverDword4Capture,
    SlotDwordWritten
}

public readonly record struct PayloadWordSlotAllocationPlan(
    PayloadWordSlotAllocationCapture Capture,
    PayloadWordSlotAllocationOutcome Outcome,
    bool AllocatorCallReached,
    uint? AllocatorCallArgument,
    uint? AllocatorEax,
    uint? AllocatorStackCleanupBytes,
    bool ZeroPathIndirectCallReached,
    uint? ZeroPathIndirectCallCellVirtualAddress,
    uint? EcxForSlotWrite,
    uint? ReceiverDword4Address,
    bool ReceiverDword4Read,
    uint? SlotAddress,
    bool SlotDwordWritten,
    uint? SlotDwordWriteValue,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadWordSlotAllocationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    uint? NormalReturnContinuationRawFileOffset = null,
    uint? AbsoluteVirtualAddress = null);

/// <summary>
/// Projects the zero-slot allocation and write path only, bounded by raw
/// 0xBE6352..0xBE636E. It makes no allocation guarantee and models only
/// normal-call continuations explicitly captured by the caller.
/// </summary>
public static class PayloadWordSlotAllocationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE6352;
    public const uint GateStartVirtualAddress = 0x00FE6F52;
    public const uint GateEndRawFileOffsetExclusive = 0x00BE636E;
    public const uint GateEndVirtualAddressExclusive = 0x00FE6F6E;

    public const uint AllocatorArgument = 0x10;
    public const uint AllocatorCallRawFileOffset = 0x00BE6354;
    public const uint AllocatorCallNormalReturnRawFileOffset = 0x00BE6359;
    public const uint AllocatorTargetRawFileOffset = 0x012B7FB0;
    public const uint AllocatorResultToEcxRawFileOffset = 0x00BE6359;
    public const uint AllocatorStackCleanupRawFileOffset = 0x00BE635B;
    public const uint AllocatorStackCleanupBytes = 4;
    public const uint AllocatorResultTestRawFileOffset = 0x00BE635E;
    public const uint NonzeroResultBranchRawFileOffset = 0x00BE6360;
    public const uint NonzeroResultBranchTargetRawFileOffset = 0x00BE6368;
    public const uint ZeroPathIndirectCallRawFileOffset = 0x00BE6362;
    public const uint ZeroPathIndirectCallNormalReturnRawFileOffset = 0x00BE6368;
    public const uint ZeroPathIndirectCallCellVirtualAddress = 0x01AC4424;
    public const uint ReceiverDword4Offset = 0x04;
    public const uint ReceiverDword4ReloadRawFileOffset = 0x00BE6368;
    public const uint SlotWriteRawFileOffset = 0x00BE636B;

    public const uint AllocatorTargetVirtualAddress =
        AllocatorTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint GateStartVirtualAddressFromPeSection =
        0x00400000 + 0x00001000 + (GateStartRawFileOffset - 0x00000400);

    public const string CapturedGateBytes =
        "6A 10 E8 57 1C 6D 00 8B C8 83 C4 04 85 C9 75 06 "
        + "FF 15 24 44 AC 01 8B 46 04 89 0C 03";

    public static uint GateStartVirtualAddressFromRawOffset =>
        RawFileOffsetToVirtualAddress(GateStartRawFileOffset);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static ReadOnlyCollection<PayloadWordSlotAllocationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadWordSlotAllocationInstructionEvidence(
                "Push allocator argument 0x10",
                GateStartRawFileOffset,
                RawFileOffsetToVirtualAddress(GateStartRawFileOffset),
                "6A 10"),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Call allocator target raw 0x12B7FB0; normal return resumes at raw 0xBE6359",
                AllocatorCallRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorCallRawFileOffset),
                "E8 57 1C 6D 00",
                AllocatorTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorTargetRawFileOffset),
                AllocatorCallNormalReturnRawFileOffset),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Move allocator EAX into ECX",
                AllocatorResultToEcxRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorResultToEcxRawFileOffset),
                "8B C8"),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Clean up the pushed allocator argument after normal return",
                AllocatorStackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorStackCleanupRawFileOffset),
                "83 C4 04"),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Test the allocator result retained in ECX",
                AllocatorResultTestRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorResultTestRawFileOffset),
                "85 C9"),
            new PayloadWordSlotAllocationInstructionEvidence(
                "A nonzero allocator result skips the indirect call to raw 0xBE6368",
                NonzeroResultBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroResultBranchRawFileOffset),
                "75 06",
                NonzeroResultBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroResultBranchTargetRawFileOffset)),
            new PayloadWordSlotAllocationInstructionEvidence(
                "The zero-result path calls through absolute cell VA 0x01AC4424; normal return resumes at raw 0xBE6368",
                ZeroPathIndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroPathIndirectCallRawFileOffset),
                "FF 15 24 44 AC 01",
                NormalReturnContinuationRawFileOffset:
                    ZeroPathIndirectCallNormalReturnRawFileOffset,
                AbsoluteVirtualAddress: ZeroPathIndirectCallCellVirtualAddress),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Independently reload receiver DWORD +4 into EAX",
                ReceiverDword4ReloadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword4ReloadRawFileOffset),
                "8B 46 04"),
            new PayloadWordSlotAllocationInstructionEvidence(
                "Write the retained ECX DWORD to [EAX+EBX]",
                SlotWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotWriteRawFileOffset),
                "89 0C 03")
        });

    public static PayloadWordSlotAllocationPlan Project(
        PayloadWordSlotAllocationCapture capture)
    {
        if (capture.PrecedingSlotDword is not uint precedingSlotDword)
        {
            return Stop(
                capture,
                HasCaptureAfterPrecedingSlot(capture)
                    ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotAllocationOutcome.MissingPrecedingSlotDwordCapture,
                stopAt: GateStartRawFileOffset);
        }

        if (precedingSlotDword != 0)
        {
            return Stop(
                capture,
                HasCaptureAfterPrecedingSlot(capture)
                    ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotAllocationOutcome.PrecedingSlotDwordNonzero,
                stopAt: GateStartRawFileOffset);
        }

        if (capture.AllocatorCallReturnedNormally is not bool allocatorReturnedNormally)
        {
            return Stop(
                capture,
                HasCaptureAfterAllocatorReturn(capture)
                    ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotAllocationOutcome.MissingAllocatorCallReturnCapture,
                allocatorCallReached: true,
                allocatorCallArgument: AllocatorArgument,
                stopAt: AllocatorCallNormalReturnRawFileOffset);
        }

        if (!allocatorReturnedNormally)
        {
            return Stop(
                capture,
                HasCaptureAfterAllocatorReturn(capture)
                    ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotAllocationOutcome.AllocatorCallDidNotReturnNormally,
                allocatorCallReached: true,
                allocatorCallArgument: AllocatorArgument,
                stopAt: AllocatorCallNormalReturnRawFileOffset);
        }

        uint? allocatorStackCleanupBytes = AllocatorStackCleanupBytes;
        if (capture.AllocatorEax is not uint allocatorEax)
        {
            return Stop(
                capture,
                HasCaptureAfterAllocatorEax(capture)
                    ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotAllocationOutcome.MissingAllocatorEaxCapture,
                allocatorCallReached: true,
                allocatorCallArgument: AllocatorArgument,
                stopAt: AllocatorResultToEcxRawFileOffset);
        }

        uint? ecxForSlotWrite;
        bool zeroPathIndirectCallReached = allocatorEax == 0;
        if (zeroPathIndirectCallReached)
        {
            if (capture.ZeroPathIndirectCallReturnedNormally
                is not bool indirectCallReturnedNormally)
            {
                return Stop(
                    capture,
                    HasCaptureAfterIndirectCallReturn(capture)
                        ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                        : PayloadWordSlotAllocationOutcome
                            .MissingZeroPathIndirectCallReturnCapture,
                    allocatorCallReached: true,
                    allocatorCallArgument: AllocatorArgument,
                    allocatorEax: allocatorEax,
                    allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                    zeroPathIndirectCallReached: true,
                    zeroPathIndirectCallCellVirtualAddress:
                        ZeroPathIndirectCallCellVirtualAddress,
                    stopAt: ZeroPathIndirectCallNormalReturnRawFileOffset);
            }

            if (!indirectCallReturnedNormally)
            {
                return Stop(
                    capture,
                    HasCaptureAfterIndirectCallReturn(capture)
                        ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                        : PayloadWordSlotAllocationOutcome
                            .ZeroPathIndirectCallDidNotReturnNormally,
                    allocatorCallReached: true,
                    allocatorCallArgument: AllocatorArgument,
                    allocatorEax: allocatorEax,
                    allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                    zeroPathIndirectCallReached: true,
                    zeroPathIndirectCallCellVirtualAddress:
                        ZeroPathIndirectCallCellVirtualAddress,
                    stopAt: ZeroPathIndirectCallNormalReturnRawFileOffset);
            }

            if (capture.EcxAfterIndirectCall is not uint ecxAfterIndirectCall)
            {
                return Stop(
                    capture,
                    capture.ReceiverDword4AtSlotWrite is not null
                        ? PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided
                        : PayloadWordSlotAllocationOutcome
                            .MissingEcxAfterIndirectCallCapture,
                    allocatorCallReached: true,
                    allocatorCallArgument: AllocatorArgument,
                    allocatorEax: allocatorEax,
                    allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                    zeroPathIndirectCallReached: true,
                    zeroPathIndirectCallCellVirtualAddress:
                        ZeroPathIndirectCallCellVirtualAddress,
                    stopAt: ZeroPathIndirectCallNormalReturnRawFileOffset);
            }

            ecxForSlotWrite = ecxAfterIndirectCall;
        }
        else
        {
            if (capture.ZeroPathIndirectCallReturnedNormally is not null
                || capture.EcxAfterIndirectCall is not null)
            {
                return Stop(
                    capture,
                    PayloadWordSlotAllocationOutcome.UnreachedCaptureProvided,
                    allocatorCallReached: true,
                    allocatorCallArgument: AllocatorArgument,
                    allocatorEax: allocatorEax,
                    allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                    stopAt: ZeroPathIndirectCallRawFileOffset);
            }

            ecxForSlotWrite = allocatorEax;
        }

        uint receiverDword4Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword4Offset);
        if (capture.ReceiverDword4AtSlotWrite is not uint receiverDword4)
        {
            return Stop(
                capture,
                PayloadWordSlotAllocationOutcome.MissingReceiverDword4Capture,
                allocatorCallReached: true,
                allocatorCallArgument: AllocatorArgument,
                allocatorEax: allocatorEax,
                allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                zeroPathIndirectCallReached: zeroPathIndirectCallReached,
                zeroPathIndirectCallCellVirtualAddress: zeroPathIndirectCallReached
                    ? ZeroPathIndirectCallCellVirtualAddress
                    : null,
                ecxForSlotWrite: ecxForSlotWrite,
                receiverDword4Address: receiverDword4Address,
                stopAt: ReceiverDword4ReloadRawFileOffset);
        }

        uint slotAddress = unchecked(
            receiverDword4 + capture.RetainedScaledOffset);
        return Stop(
            capture,
            PayloadWordSlotAllocationOutcome.SlotDwordWritten,
            allocatorCallReached: true,
            allocatorCallArgument: AllocatorArgument,
            allocatorEax: allocatorEax,
            allocatorStackCleanupBytes: allocatorStackCleanupBytes,
            zeroPathIndirectCallReached: zeroPathIndirectCallReached,
            zeroPathIndirectCallCellVirtualAddress: zeroPathIndirectCallReached
                ? ZeroPathIndirectCallCellVirtualAddress
                : null,
            ecxForSlotWrite: ecxForSlotWrite,
            receiverDword4Address: receiverDword4Address,
            receiverDword4Read: true,
            slotAddress: slotAddress,
            slotDwordWritten: true,
            slotDwordWriteValue: ecxForSlotWrite,
            stopAt: GateEndRawFileOffsetExclusive);
    }

    private static bool HasCaptureAfterPrecedingSlot(
        PayloadWordSlotAllocationCapture capture) =>
        capture.AllocatorCallReturnedNormally is not null
        || capture.AllocatorEax is not null
        || capture.ZeroPathIndirectCallReturnedNormally is not null
        || capture.EcxAfterIndirectCall is not null
        || capture.ReceiverDword4AtSlotWrite is not null;

    private static bool HasCaptureAfterAllocatorReturn(
        PayloadWordSlotAllocationCapture capture) =>
        capture.AllocatorEax is not null
        || capture.ZeroPathIndirectCallReturnedNormally is not null
        || capture.EcxAfterIndirectCall is not null
        || capture.ReceiverDword4AtSlotWrite is not null;

    private static bool HasCaptureAfterAllocatorEax(
        PayloadWordSlotAllocationCapture capture) =>
        capture.ZeroPathIndirectCallReturnedNormally is not null
        || capture.EcxAfterIndirectCall is not null
        || capture.ReceiverDword4AtSlotWrite is not null;

    private static bool HasCaptureAfterIndirectCallReturn(
        PayloadWordSlotAllocationCapture capture) =>
        capture.EcxAfterIndirectCall is not null
        || capture.ReceiverDword4AtSlotWrite is not null;

    private static PayloadWordSlotAllocationPlan Stop(
        PayloadWordSlotAllocationCapture capture,
        PayloadWordSlotAllocationOutcome outcome,
        bool allocatorCallReached = false,
        uint? allocatorCallArgument = null,
        uint? allocatorEax = null,
        uint? allocatorStackCleanupBytes = null,
        bool zeroPathIndirectCallReached = false,
        uint? zeroPathIndirectCallCellVirtualAddress = null,
        uint? ecxForSlotWrite = null,
        uint? receiverDword4Address = null,
        bool receiverDword4Read = false,
        uint? slotAddress = null,
        bool slotDwordWritten = false,
        uint? slotDwordWriteValue = null,
        uint stopAt = GateStartRawFileOffset) =>
        new(
            capture,
            outcome,
            allocatorCallReached,
            allocatorCallArgument,
            allocatorEax,
            allocatorStackCleanupBytes,
            zeroPathIndirectCallReached,
            zeroPathIndirectCallCellVirtualAddress,
            ecxForSlotWrite,
            receiverDword4Address,
            receiverDword4Read,
            slotAddress,
            slotDwordWritten,
            slotDwordWriteValue,
            stopAt);
}
