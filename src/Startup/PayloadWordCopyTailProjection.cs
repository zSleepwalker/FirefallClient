using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordCopyTailCapture(
    uint ReceiverPointer,
    uint RetainedEdi,
    uint RetainedEbx,
    uint FramePointer,
    uint? ReceiverDword4,
    uint? SlotPointer,
    uint? StackArgumentPointer,
    ushort? Word,
    uint? IncrementOldDword);

public enum PayloadWordCopyTailOutcome
{
    MissingReceiverDword4Capture,
    MissingSlotPointerCapture,
    MissingStackArgumentPointerCapture,
    MissingWordCapture,
    MissingIncrementOldDwordCapture,
    UnreachedCaptureProvided,
    CandidateZeroSkippedWordCopy,
    WordCopiedAndCounterIncremented
}

public readonly record struct PayloadWordCopyTailPlan(
    PayloadWordCopyTailCapture Capture,
    PayloadWordCopyTailOutcome Outcome,
    uint? ReceiverDword4ReadAddress,
    bool ReceiverDword4Read,
    uint? SlotPointerReadAddress,
    bool SlotPointerRead,
    uint MaskedEdi,
    uint? CandidateSlotAddress,
    bool? CandidateIsZero,
    uint? StackArgumentPointerReadAddress,
    bool StackArgumentPointerRead,
    uint? WordReadAddress,
    bool WordRead,
    uint? WordCopyDestinationAddress,
    ushort? WordCopyValue,
    bool WordCopyPerformed,
    uint? IncrementOldDwordReadAddress,
    bool IncrementOldDwordRead,
    uint? IncrementedDwordValue,
    uint? CounterDwordWriteAddress,
    uint? Ret4BoundaryRawFileOffsetExclusive,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadWordCopyTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the confirmed copy-and-increment tail at raw 0xBE636E through
/// the RET 4 boundary. Captured memory values are independent observations.
/// </summary>
public static class PayloadWordCopyTailProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint TailStartRawFileOffset = 0x00BE636E;
    public const uint ReceiverDword4Offset = 0x04;
    public const uint StackArgumentPointerOffset = 0x08;
    public const uint ReceiverCounterOffset = 0x10;
    public const uint EdiIndexMask = 0x07;
    public const uint CandidateIndexScale = 2;

    public const uint ReceiverDword4ReadRawFileOffset = 0x00BE636E;
    public const uint SlotPointerReadRawFileOffset = 0x00BE6374;
    public const uint CandidateAddressRawFileOffset = 0x00BE6377;
    public const uint CandidateTestRawFileOffset = 0x00BE637C;
    public const uint CandidateZeroBranchRawFileOffset = 0x00BE637E;
    public const uint CandidateZeroBranchTargetRawFileOffset = 0x00BE6389;
    public const uint StackArgumentPointerReadRawFileOffset = 0x00BE6380;
    public const uint WordReadRawFileOffset = 0x00BE6383;
    public const uint WordCopyRawFileOffset = 0x00BE6386;
    public const uint IncrementOldDwordReadRawFileOffset = 0x00BE6389;
    public const uint CounterDwordWriteRawFileOffset = 0x00BE6389;
    public const uint Ret4BoundaryRawFileOffsetExclusive = 0x00BE6391;

    public const string CapturedTailBytes =
        "8B 46 04 83 E7 07 8B 04 18 8D 0C 78 5F 5B 85 C9 74 09 "
        + "8B 45 08 66 8B 00 66 89 01 FF 46 10 5E 5D C2 04 00";

    public static uint TailStartVirtualAddress =>
        RawFileOffsetToVirtualAddress(TailStartRawFileOffset);

    public static ReadOnlyCollection<PayloadWordCopyTailInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadWordCopyTailInstructionEvidence(
                "Read the receiver DWORD at +4",
                ReceiverDword4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword4ReadRawFileOffset),
                "8B 46 04"),
            new PayloadWordCopyTailInstructionEvidence(
                "Mask retained EDI to its low three bits",
                0x00BE6371,
                RawFileOffsetToVirtualAddress(0x00BE6371),
                "83 E7 07"),
            new PayloadWordCopyTailInstructionEvidence(
                "Read the slot pointer at receiverDword4+retainedEBX",
                SlotPointerReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotPointerReadRawFileOffset),
                "8B 04 18"),
            new PayloadWordCopyTailInstructionEvidence(
                "Form slotPointer+(maskedEdi*2) as the candidate address",
                CandidateAddressRawFileOffset,
                RawFileOffsetToVirtualAddress(CandidateAddressRawFileOffset),
                "8D 0C 78"),
            new PayloadWordCopyTailInstructionEvidence(
                "Restore retained EDI and EBX",
                0x00BE637A,
                RawFileOffsetToVirtualAddress(0x00BE637A),
                "5F 5B"),
            new PayloadWordCopyTailInstructionEvidence(
                "Test the candidate address for zero",
                CandidateTestRawFileOffset,
                RawFileOffsetToVirtualAddress(CandidateTestRawFileOffset),
                "85 C9"),
            new PayloadWordCopyTailInstructionEvidence(
                "A zero candidate skips the stack-argument read and WORD copy",
                CandidateZeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(CandidateZeroBranchRawFileOffset),
                "74 09",
                CandidateZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(CandidateZeroBranchTargetRawFileOffset)),
            new PayloadWordCopyTailInstructionEvidence(
                "Read the stack argument pointer from framePointer+8",
                StackArgumentPointerReadRawFileOffset,
                RawFileOffsetToVirtualAddress(StackArgumentPointerReadRawFileOffset),
                "8B 45 08"),
            new PayloadWordCopyTailInstructionEvidence(
                "Read a WORD from the captured stack argument pointer",
                WordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(WordReadRawFileOffset),
                "66 8B 00"),
            new PayloadWordCopyTailInstructionEvidence(
                "Write the captured WORD to the candidate address",
                WordCopyRawFileOffset,
                RawFileOffsetToVirtualAddress(WordCopyRawFileOffset),
                "66 89 01"),
            new PayloadWordCopyTailInstructionEvidence(
                "Increment the receiver DWORD at +0x10 with wrapping arithmetic",
                IncrementOldDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(IncrementOldDwordReadRawFileOffset),
                "FF 46 10"),
            new PayloadWordCopyTailInstructionEvidence(
                "Restore ESI and EBP, then return while popping four stack bytes",
                0x00BE638C,
                RawFileOffsetToVirtualAddress(0x00BE638C),
                "5E 5D C2 04 00")
        });

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint SlotPointerReadAddress(uint receiverDword4, uint retainedEbx) =>
        unchecked(receiverDword4 + retainedEbx);

    public static uint CandidateAddress(uint slotPointer, uint retainedEdi) =>
        unchecked(slotPointer + ((retainedEdi & EdiIndexMask) * CandidateIndexScale));

    public static PayloadWordCopyTailPlan Project(
        PayloadWordCopyTailCapture capture)
    {
        uint receiverDword4Address = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword4Offset);
        uint maskedEdi = capture.RetainedEdi & EdiIndexMask;
        if (capture.ReceiverDword4 is not uint receiverDword4)
        {
            return Stop(
                capture,
                HasCaptureAfterReceiverDword4(capture)
                    ? PayloadWordCopyTailOutcome.UnreachedCaptureProvided
                    : PayloadWordCopyTailOutcome.MissingReceiverDword4Capture,
                receiverDword4ReadAddress: receiverDword4Address,
                maskedEdi: maskedEdi,
                stopAt: ReceiverDword4ReadRawFileOffset);
        }

        uint slotPointerReadAddress = SlotPointerReadAddress(
            receiverDword4,
            capture.RetainedEbx);
        if (capture.SlotPointer is not uint slotPointer)
        {
            return Stop(
                capture,
                HasCaptureAfterSlotPointer(capture)
                    ? PayloadWordCopyTailOutcome.UnreachedCaptureProvided
                    : PayloadWordCopyTailOutcome.MissingSlotPointerCapture,
                receiverDword4Address,
                receiverDword4Read: true,
                slotPointerReadAddress: slotPointerReadAddress,
                maskedEdi: maskedEdi,
                stopAt: SlotPointerReadRawFileOffset);
        }

        uint candidateAddress = CandidateAddress(slotPointer, capture.RetainedEdi);
        bool candidateIsZero = candidateAddress == 0;
        if (candidateIsZero)
        {
            if (capture.StackArgumentPointer is not null || capture.Word is not null)
            {
                return Stop(
                    capture,
                    PayloadWordCopyTailOutcome.UnreachedCaptureProvided,
                    receiverDword4Address,
                    receiverDword4Read: true,
                    slotPointerReadAddress,
                    slotPointerRead: true,
                    maskedEdi,
                    candidateAddress,
                    candidateIsZero,
                    stopAt: StackArgumentPointerReadRawFileOffset);
            }

            return ProjectCounter(
                capture,
                PayloadWordCopyTailOutcome.CandidateZeroSkippedWordCopy,
                receiverDword4Address,
                slotPointerReadAddress,
                maskedEdi,
                candidateAddress,
                candidateIsZero,
                wordCopyPerformed: false);
        }

        uint stackArgumentPointerReadAddress = ReceiverDwordAddress(
            capture.FramePointer,
            StackArgumentPointerOffset);
        if (capture.StackArgumentPointer is not uint stackArgumentPointer)
        {
            return Stop(
                capture,
                capture.Word is not null || capture.IncrementOldDword is not null
                    ? PayloadWordCopyTailOutcome.UnreachedCaptureProvided
                    : PayloadWordCopyTailOutcome.MissingStackArgumentPointerCapture,
                receiverDword4Address,
                receiverDword4Read: true,
                slotPointerReadAddress,
                slotPointerRead: true,
                maskedEdi,
                candidateAddress,
                candidateIsZero,
                stackArgumentPointerReadAddress,
                stopAt: StackArgumentPointerReadRawFileOffset);
        }

        if (capture.Word is not ushort word)
        {
            return Stop(
                capture,
                capture.IncrementOldDword is not null
                    ? PayloadWordCopyTailOutcome.UnreachedCaptureProvided
                    : PayloadWordCopyTailOutcome.MissingWordCapture,
                receiverDword4Address,
                receiverDword4Read: true,
                slotPointerReadAddress,
                slotPointerRead: true,
                maskedEdi,
                candidateAddress,
                candidateIsZero,
                stackArgumentPointerReadAddress,
                stackArgumentPointerRead: true,
                wordReadAddress: stackArgumentPointer,
                stopAt: WordReadRawFileOffset);
        }

        return ProjectCounter(
            capture,
            PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented,
            receiverDword4Address,
            slotPointerReadAddress,
            maskedEdi,
            candidateAddress,
            candidateIsZero,
            stackArgumentPointerReadAddress,
            wordReadAddress: stackArgumentPointer,
            wordCopyPerformed: true,
            wordCopyValue: word);
    }

    private static PayloadWordCopyTailPlan ProjectCounter(
        PayloadWordCopyTailCapture capture,
        PayloadWordCopyTailOutcome completedOutcome,
        uint receiverDword4ReadAddress,
        uint slotPointerReadAddress,
        uint maskedEdi,
        uint candidateAddress,
        bool candidateIsZero,
        uint? stackArgumentPointerReadAddress = null,
        uint? wordReadAddress = null,
        bool wordCopyPerformed = false,
        ushort? wordCopyValue = null)
    {
        uint counterAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverCounterOffset);
        if (capture.IncrementOldDword is not uint incrementOldDword)
        {
            return Stop(
                capture,
                PayloadWordCopyTailOutcome.MissingIncrementOldDwordCapture,
                receiverDword4ReadAddress,
                receiverDword4Read: true,
                slotPointerReadAddress,
                slotPointerRead: true,
                maskedEdi,
                candidateAddress,
                candidateIsZero,
                stackArgumentPointerReadAddress,
                stackArgumentPointerRead: stackArgumentPointerReadAddress is not null,
                wordReadAddress,
                wordRead: wordReadAddress is not null,
                wordCopyDestinationAddress: wordCopyPerformed ? candidateAddress : null,
                wordCopyValue,
                wordCopyPerformed,
                incrementOldDwordReadAddress: counterAddress,
                stopAt: IncrementOldDwordReadRawFileOffset);
        }

        return Stop(
            capture,
            completedOutcome,
            receiverDword4ReadAddress,
            receiverDword4Read: true,
            slotPointerReadAddress,
            slotPointerRead: true,
            maskedEdi,
            candidateAddress,
            candidateIsZero,
            stackArgumentPointerReadAddress,
            stackArgumentPointerRead: stackArgumentPointerReadAddress is not null,
            wordReadAddress,
            wordRead: wordReadAddress is not null,
            wordCopyDestinationAddress: wordCopyPerformed ? candidateAddress : null,
            wordCopyValue,
            wordCopyPerformed,
            incrementOldDwordReadAddress: counterAddress,
            incrementOldDwordRead: true,
            incrementedDwordValue: unchecked(incrementOldDword + 1),
            counterDwordWriteAddress: counterAddress,
            ret4Boundary: Ret4BoundaryRawFileOffsetExclusive,
            stopAt: Ret4BoundaryRawFileOffsetExclusive);
    }

    private static bool HasCaptureAfterReceiverDword4(
        PayloadWordCopyTailCapture capture) =>
        capture.SlotPointer is not null
        || capture.StackArgumentPointer is not null
        || capture.Word is not null
        || capture.IncrementOldDword is not null;

    private static bool HasCaptureAfterSlotPointer(
        PayloadWordCopyTailCapture capture) =>
        capture.StackArgumentPointer is not null
        || capture.Word is not null
        || capture.IncrementOldDword is not null;

    private static PayloadWordCopyTailPlan Stop(
        PayloadWordCopyTailCapture capture,
        PayloadWordCopyTailOutcome outcome,
        uint? receiverDword4ReadAddress = null,
        bool receiverDword4Read = false,
        uint? slotPointerReadAddress = null,
        bool slotPointerRead = false,
        uint maskedEdi = 0,
        uint? candidateSlotAddress = null,
        bool? candidateIsZero = null,
        uint? stackArgumentPointerReadAddress = null,
        bool stackArgumentPointerRead = false,
        uint? wordReadAddress = null,
        bool wordRead = false,
        uint? wordCopyDestinationAddress = null,
        ushort? wordCopyValue = null,
        bool wordCopyPerformed = false,
        uint? incrementOldDwordReadAddress = null,
        bool incrementOldDwordRead = false,
        uint? incrementedDwordValue = null,
        uint? counterDwordWriteAddress = null,
        uint? ret4Boundary = null,
        uint stopAt = TailStartRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword4ReadAddress,
            receiverDword4Read,
            slotPointerReadAddress,
            slotPointerRead,
            maskedEdi,
            candidateSlotAddress,
            candidateIsZero,
            stackArgumentPointerReadAddress,
            stackArgumentPointerRead,
            wordReadAddress,
            wordRead,
            wordCopyDestinationAddress,
            wordCopyValue,
            wordCopyPerformed,
            incrementOldDwordReadAddress,
            incrementOldDwordRead,
            incrementedDwordValue,
            counterDwordWriteAddress,
            ret4Boundary,
            stopAt);
}
