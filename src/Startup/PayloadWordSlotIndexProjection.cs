using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordSlotIndexCapture(
    uint ReceiverPointer,
    uint? ReceiverDword8,
    uint? ReceiverDword10,
    uint? ReceiverDword0CBeforeAnd,
    uint? ReceiverDword0CAfterAnd,
    uint? ReceiverDword4,
    uint? SlotDword);

public enum PayloadWordSlotIndexOutcome
{
    MissingReceiverDword8Capture,
    MissingReceiverDword10Capture,
    MissingReceiverDword0CBeforeAndCapture,
    MissingReceiverDword0CAfterAndCapture,
    MissingReceiverDword4Capture,
    MissingSlotCapture,
    UnreachedCaptureProvided,
    SlotDwordNonzeroBranchesToBoundary,
    SlotDwordZeroFallsThroughToBoundary
}

public readonly record struct PayloadWordSlotIndexPlan(
    PayloadWordSlotIndexCapture Capture,
    PayloadWordSlotIndexOutcome Outcome,
    uint? ReceiverDword8Address,
    bool ReceiverDword8Read,
    uint? ReceiverDword10Address,
    bool ReceiverDword10Read,
    uint? ReceiverDword0CAddress,
    bool ReceiverDword0CBeforeAndRead,
    uint? ComputedMask,
    uint? ReceiverDword0CWriteValue,
    uint? ReceiverDword0CAfterAnd,
    bool ReceiverDword0CAfterAndRead,
    uint? AddedDword,
    uint? ShiftedDword,
    uint? DecrementedCount,
    uint? Index,
    uint? ReceiverDword4Address,
    bool ReceiverDword4Read,
    uint? ScaledOffset,
    uint? SlotAddress,
    bool SlotDwordRead,
    bool? SlotDwordIsNonzero,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadWordSlotIndexInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the bounded slot-index calculation only, ending at either the
/// conditional branch target or the instruction immediately after the branch.
/// </summary>
public static class PayloadWordSlotIndexProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE6325;
    public const uint GateStartVirtualAddress = 0x00FE6F25;
    public const uint GateEndRawFileOffsetExclusive = 0x00BE6352;
    public const uint GateEndVirtualAddressExclusive = 0x00FE6F52;
    public const uint BranchTargetRawFileOffset = 0x00BE636E;
    public const uint BranchTargetVirtualAddress = 0x00FE6F6E;

    public const uint ReceiverDword8Offset = 0x08;
    public const uint ReceiverDword10Offset = 0x10;
    public const uint ReceiverDword0COffset = 0x0C;
    public const uint ReceiverDword4Offset = 0x04;
    public const uint CountScale = 8;
    public const uint CountMaskSubtract = 1;
    public const uint CountDecrement = 1;
    public const uint RightShift = 3;
    public const uint SlotScale = 4;

    public const uint ReceiverDword8ReadRawFileOffset = 0x00BE6325;
    public const uint ReceiverDword10ReadRawFileOffset = 0x00BE632A;
    public const uint ReceiverDword0CBeforeAndReadRawFileOffset = 0x00BE6335;
    public const uint ReceiverDword0CWriteRawFileOffset = 0x00BE6335;
    public const uint ReceiverDword0CAfterAndReadRawFileOffset = 0x00BE6338;
    public const uint ReceiverDword4ReadRawFileOffset = 0x00BE6342;
    public const uint SlotIndexScaleRawFileOffset = 0x00BE6345;
    public const uint SlotDwordReadRawFileOffset = 0x00BE634C;
    public const uint SlotBranchRawFileOffset = 0x00BE6350;

    public const uint GateStartVirtualAddressFromPeSection =
        0x00400000 + 0x00001000 + (GateStartRawFileOffset - 0x00000400);

    public const string CapturedGateBytes =
        "8B 4E 08 53 57 8B 7E 10 8D 04 CD FF FF FF FF 49 21 46 0C "
        + "03 7E 0C 8B C7 C1 E8 03 23 C8 8B 46 04 8D 1C 8D 00 00 00 00 "
        + "83 3C 03 00 75 1C";

    public static uint GateStartVirtualAddressFromRawOffset =>
        RawFileOffsetToVirtualAddress(GateStartRawFileOffset);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static ReadOnlyCollection<PayloadWordSlotIndexInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadWordSlotIndexInstructionEvidence(
                "Capture the receiver DWORD at +8 into ECX",
                ReceiverDword8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword8ReadRawFileOffset),
                "8B 4E 08"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Push EBX",
                0x00BE6328,
                RawFileOffsetToVirtualAddress(0x00BE6328),
                "53"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Push EDI",
                0x00BE6329,
                RawFileOffsetToVirtualAddress(0x00BE6329),
                "57"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Capture the receiver DWORD at +0x10 into EDI",
                ReceiverDword10ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword10ReadRawFileOffset),
                "8B 7E 10"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Form wrapping ECX*8-1 in EAX",
                0x00BE632D,
                RawFileOffsetToVirtualAddress(0x00BE632D),
                "8D 04 CD FF FF FF FF"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Decrement ECX with 32-bit wraparound",
                0x00BE6334,
                RawFileOffsetToVirtualAddress(0x00BE6334),
                "49"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Read, AND, and write the receiver DWORD at +0x0C",
                ReceiverDword0CBeforeAndReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword0CBeforeAndReadRawFileOffset),
                "21 46 0C"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Independently reload the receiver DWORD at +0x0C and add it to EDI",
                ReceiverDword0CAfterAndReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword0CAfterAndReadRawFileOffset),
                "03 7E 0C"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Move EDI to EAX and shift it right as an unsigned DWORD",
                0x00BE633B,
                RawFileOffsetToVirtualAddress(0x00BE633B),
                "8B C7 C1 E8 03"),
            new PayloadWordSlotIndexInstructionEvidence(
                "AND the decremented count with the shifted DWORD to form the index",
                0x00BE6340,
                RawFileOffsetToVirtualAddress(0x00BE6340),
                "23 C8"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Capture the receiver DWORD at +4 into EAX",
                ReceiverDword4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword4ReadRawFileOffset),
                "8B 46 04"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Form the wrapping index*4 slot offset in EBX",
                SlotIndexScaleRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotIndexScaleRawFileOffset),
                "8D 1C 8D 00 00 00 00"),
            new PayloadWordSlotIndexInstructionEvidence(
                "Compare the DWORD at base+scaled offset against zero",
                SlotDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotDwordReadRawFileOffset),
                "83 3C 03 00"),
            new PayloadWordSlotIndexInstructionEvidence(
                "A nonzero slot DWORD branches to raw 0xBE636E; zero falls through to 0xBE6352",
                SlotBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotBranchRawFileOffset),
                "75 1C",
                BranchTargetRawFileOffset,
                BranchTargetVirtualAddress)
        });

    public static PayloadWordSlotIndexPlan Project(
        PayloadWordSlotIndexCapture capture)
    {
        uint dword8Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword8Offset);
        if (capture.ReceiverDword8 is not uint receiverDword8)
        {
            return Stop(
                capture,
                HasCaptureAfterDword8(capture)
                    ? PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotIndexOutcome.MissingReceiverDword8Capture,
                receiverDword8Address: dword8Address,
                stopAt: ReceiverDword8ReadRawFileOffset);
        }

        uint dword10Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword10Offset);
        if (capture.ReceiverDword10 is not uint receiverDword10)
        {
            return Stop(
                capture,
                HasCaptureAfterDword10(capture)
                    ? PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotIndexOutcome.MissingReceiverDword10Capture,
                receiverDword8Address: dword8Address,
                receiverDword8Read: true,
                receiverDword10Address: dword10Address,
                stopAt: ReceiverDword10ReadRawFileOffset);
        }

        uint mask = unchecked(receiverDword8 * CountScale - CountMaskSubtract);
        uint decrementedCount = unchecked(receiverDword8 - CountDecrement);
        uint dword0CAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword0COffset);
        if (capture.ReceiverDword0CBeforeAnd is not uint receiverDword0CBeforeAnd)
        {
            return Stop(
                capture,
                HasCaptureAfterDword0CBeforeAnd(capture)
                    ? PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotIndexOutcome.MissingReceiverDword0CBeforeAndCapture,
                receiverDword8Address: dword8Address,
                receiverDword8Read: true,
                receiverDword10Address: dword10Address,
                receiverDword10Read: true,
                receiverDword0CAddress: dword0CAddress,
                computedMask: mask,
                decrementedCount: decrementedCount,
                stopAt: ReceiverDword0CBeforeAndReadRawFileOffset);
        }

        uint writeValue = receiverDword0CBeforeAnd & mask;
        if (capture.ReceiverDword0CAfterAnd is not uint receiverDword0CAfterAnd)
        {
            return Stop(
                capture,
                HasCaptureAfterDword0CAfterAnd(capture)
                    ? PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotIndexOutcome.MissingReceiverDword0CAfterAndCapture,
                receiverDword8Address: dword8Address,
                receiverDword8Read: true,
                receiverDword10Address: dword10Address,
                receiverDword10Read: true,
                receiverDword0CAddress: dword0CAddress,
                receiverDword0CBeforeAndRead: true,
                computedMask: mask,
                receiverDword0CWriteValue: writeValue,
                decrementedCount: decrementedCount,
                stopAt: ReceiverDword0CAfterAndReadRawFileOffset);
        }

        uint addedDword = unchecked(receiverDword10 + receiverDword0CAfterAnd);
        uint shiftedDword = addedDword >> (int)RightShift;
        uint index = decrementedCount & shiftedDword;
        uint scaledOffset = unchecked(index * SlotScale);

        uint dword4Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword4Offset);
        if (capture.ReceiverDword4 is not uint receiverDword4)
        {
            return Stop(
                capture,
                HasCaptureAfterDword4(capture)
                    ? PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
                    : PayloadWordSlotIndexOutcome.MissingReceiverDword4Capture,
                receiverDword8Address: dword8Address,
                receiverDword8Read: true,
                receiverDword10Address: dword10Address,
                receiverDword10Read: true,
                receiverDword0CAddress: dword0CAddress,
                receiverDword0CBeforeAndRead: true,
                computedMask: mask,
                receiverDword0CWriteValue: writeValue,
                receiverDword0CAfterAnd: receiverDword0CAfterAnd,
                receiverDword0CAfterAndRead: true,
                addedDword: addedDword,
                shiftedDword: shiftedDword,
                decrementedCount: decrementedCount,
                index: index,
                receiverDword4Address: dword4Address,
                scaledOffset: scaledOffset,
                stopAt: ReceiverDword4ReadRawFileOffset);
        }

        uint slotAddress = unchecked(receiverDword4 + scaledOffset);
        if (capture.SlotDword is not uint slotDword)
        {
            return Stop(
                capture,
                PayloadWordSlotIndexOutcome.MissingSlotCapture,
                receiverDword8Address: dword8Address,
                receiverDword8Read: true,
                receiverDword10Address: dword10Address,
                receiverDword10Read: true,
                receiverDword0CAddress: dword0CAddress,
                receiverDword0CBeforeAndRead: true,
                computedMask: mask,
                receiverDword0CWriteValue: writeValue,
                receiverDword0CAfterAnd: receiverDword0CAfterAnd,
                receiverDword0CAfterAndRead: true,
                addedDword: addedDword,
                shiftedDword: shiftedDword,
                decrementedCount: decrementedCount,
                index: index,
                receiverDword4Address: dword4Address,
                receiverDword4Read: true,
                scaledOffset: scaledOffset,
                slotAddress: slotAddress,
                stopAt: SlotDwordReadRawFileOffset);
        }

        bool slotIsNonzero = slotDword != 0;
        return Stop(
            capture,
            slotIsNonzero
                ? PayloadWordSlotIndexOutcome.SlotDwordNonzeroBranchesToBoundary
                : PayloadWordSlotIndexOutcome.SlotDwordZeroFallsThroughToBoundary,
            receiverDword8Address: dword8Address,
            receiverDword8Read: true,
            receiverDword10Address: dword10Address,
            receiverDword10Read: true,
            receiverDword0CAddress: dword0CAddress,
            receiverDword0CBeforeAndRead: true,
            computedMask: mask,
            receiverDword0CWriteValue: writeValue,
            receiverDword0CAfterAnd: receiverDword0CAfterAnd,
            receiverDword0CAfterAndRead: true,
            addedDword: addedDword,
            shiftedDword: shiftedDword,
            decrementedCount: decrementedCount,
            index: index,
            receiverDword4Address: dword4Address,
            receiverDword4Read: true,
            scaledOffset: scaledOffset,
            slotAddress: slotAddress,
            slotDwordRead: true,
            slotDwordIsNonzero: slotIsNonzero,
            route: slotIsNonzero ? BranchTargetRawFileOffset : GateEndRawFileOffsetExclusive,
            stopAt: slotIsNonzero ? BranchTargetRawFileOffset : GateEndRawFileOffsetExclusive);
    }

    private static bool HasCaptureAfterDword8(PayloadWordSlotIndexCapture capture) =>
        capture.ReceiverDword10 is not null
        || capture.ReceiverDword0CBeforeAnd is not null
        || capture.ReceiverDword0CAfterAnd is not null
        || capture.ReceiverDword4 is not null
        || capture.SlotDword is not null;

    private static bool HasCaptureAfterDword10(PayloadWordSlotIndexCapture capture) =>
        capture.ReceiverDword0CBeforeAnd is not null
        || capture.ReceiverDword0CAfterAnd is not null
        || capture.ReceiverDword4 is not null
        || capture.SlotDword is not null;

    private static bool HasCaptureAfterDword0CBeforeAnd(
        PayloadWordSlotIndexCapture capture) =>
        capture.ReceiverDword0CAfterAnd is not null
        || capture.ReceiverDword4 is not null
        || capture.SlotDword is not null;

    private static bool HasCaptureAfterDword0CAfterAnd(
        PayloadWordSlotIndexCapture capture) =>
        capture.ReceiverDword4 is not null || capture.SlotDword is not null;

    private static bool HasCaptureAfterDword4(PayloadWordSlotIndexCapture capture) =>
        capture.SlotDword is not null;

    private static PayloadWordSlotIndexPlan Stop(
        PayloadWordSlotIndexCapture capture,
        PayloadWordSlotIndexOutcome outcome,
        uint? receiverDword8Address = null,
        bool receiverDword8Read = false,
        uint? receiverDword10Address = null,
        bool receiverDword10Read = false,
        uint? receiverDword0CAddress = null,
        bool receiverDword0CBeforeAndRead = false,
        uint? computedMask = null,
        uint? receiverDword0CWriteValue = null,
        uint? receiverDword0CAfterAnd = null,
        bool receiverDword0CAfterAndRead = false,
        uint? addedDword = null,
        uint? shiftedDword = null,
        uint? decrementedCount = null,
        uint? index = null,
        uint? receiverDword4Address = null,
        bool receiverDword4Read = false,
        uint? scaledOffset = null,
        uint? slotAddress = null,
        bool slotDwordRead = false,
        bool? slotDwordIsNonzero = null,
        uint? route = null,
        uint stopAt = GateStartRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword8Address,
            receiverDword8Read,
            receiverDword10Address,
            receiverDword10Read,
            receiverDword0CAddress,
            receiverDword0CBeforeAndRead,
            computedMask,
            receiverDword0CWriteValue,
            receiverDword0CAfterAnd,
            receiverDword0CAfterAndRead,
            addedDword,
            shiftedDword,
            decrementedCount,
            index,
            receiverDword4Address,
            receiverDword4Read,
            scaledOffset,
            slotAddress,
            slotDwordRead,
            slotDwordIsNonzero,
            route,
            stopAt);
}
