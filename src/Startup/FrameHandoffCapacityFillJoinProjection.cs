using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFillJoinCapture(
    bool CaptureValidatedJoin,
    uint EntryEbp,
    uint EntryEdi,
    uint? FrameMinus4);

public enum FrameHandoffCapacityFillJoinOutcome
{
    JoinNotValidated,
    MissingFrameMinus4Read,
    ZeroEntryEdiBranchToRawFileOffsetBE6CA2,
    NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
}

public readonly record struct FrameHandoffCapacityFillJoinPlan(
    FrameHandoffCapacityFillJoinCapture Capture,
    FrameHandoffCapacityFillJoinOutcome Outcome,
    uint FrameMinus4ReadAddress,
    bool FrameMinus4ReadReached,
    bool FrameMinus4ReadCaptured,
    uint? EbxAfterFrameMinus4Read,
    bool UnreachedFrameMinus4ReadCaptureProvided,
    bool EntryEdiTestReached,
    bool? EntryEdiZeroFlag,
    bool ZeroBranchReached,
    bool? ZeroBranchTaken,
    uint? BranchTargetRawFileOffset,
    bool EcxEntryEdiMoveReached,
    uint? EcxAfterMove,
    bool EdiLoadedEbxMoveReached,
    uint? EdiAfterMove,
    bool JumpReached,
    uint? JumpTargetRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacityFillJoinInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the validated capacity-fill join from its frame read through its route transfer.
/// </summary>
public static class FrameHandoffCapacityFillJoinProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint PeImageBase = 0x00400000;
    public const uint TextSectionRva = 0x00001000;
    public const uint TextSectionRawFileOffset = 0x00000400;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        PeImageBase + TextSectionRva - TextSectionRawFileOffset;

    public const uint EntryRawFileOffset = 0x00BE6C4A;
    public const uint EntryVirtualAddress = 0x00FE784A;
    public const uint EndRawFileOffsetExclusive = 0x00BE6C57;
    public const uint EndVirtualAddressExclusive = 0x00FE7857;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE6CA2;
    public const uint NonzeroJumpTargetRawFileOffset = 0x00BE6C9E;
    public const uint FrameMinus4Offset = 4;
    public const uint CapturedWindowByteCount =
        EndRawFileOffsetExclusive - EntryRawFileOffset;

    public const string CapturedWindowBytes =
        "8B5DFC85FF74518BCF8BFBEB47";

    public static ImmutableArray<FrameHandoffCapacityFillJoinInstructionEvidence> Evidence { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "Read the DWORD at [EBP-4] into EBX",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "8B 5D FC"),
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "Test entry EDI without modifying it",
                0x00BE6C4D,
                RawFileOffsetToVirtualAddress(0x00BE6C4D),
                "85 FF"),
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "JZ routes zero entry EDI to raw 0xBE6CA2",
                0x00BE6C4F,
                RawFileOffsetToVirtualAddress(0x00BE6C4F),
                "74 51",
                ZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchTargetRawFileOffset)),
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "Copy entry EDI into ECX",
                0x00BE6C51,
                RawFileOffsetToVirtualAddress(0x00BE6C51),
                "8B CF"),
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "Copy the freshly loaded EBX into EDI",
                0x00BE6C53,
                RawFileOffsetToVirtualAddress(0x00BE6C53),
                "8B FB"),
            new FrameHandoffCapacityFillJoinInstructionEvidence(
                "Short jump routes the nonzero path to raw 0xBE6C9E",
                0x00BE6C55,
                RawFileOffsetToVirtualAddress(0x00BE6C55),
                "EB 47",
                NonzeroJumpTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroJumpTargetRawFileOffset)));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinus4ReadAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Offset);

    public static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2u + (uint)(sbyte)displacement);

    public static FrameHandoffCapacityFillJoinPlan Project(
        FrameHandoffCapacityFillJoinCapture capture)
    {
        uint readAddress = FrameMinus4ReadAddress(capture.EntryEbp);

        if (!capture.CaptureValidatedJoin)
        {
            return Stop(
                capture,
                FrameHandoffCapacityFillJoinOutcome.JoinNotValidated,
                readAddress,
                unreachedFrameMinus4ReadCaptureProvided: capture.FrameMinus4.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.FrameMinus4 is not uint loadedEbx)
        {
            return Stop(
                capture,
                FrameHandoffCapacityFillJoinOutcome.MissingFrameMinus4Read,
                readAddress,
                frameMinus4ReadReached: true,
                stopAt: EntryRawFileOffset);
        }

        bool entryEdiIsZero = capture.EntryEdi == 0;
        if (entryEdiIsZero)
        {
            return Stop(
                capture,
                FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2,
                readAddress,
                frameMinus4ReadReached: true,
                frameMinus4ReadCaptured: true,
                ebxAfterFrameMinus4Read: loadedEbx,
                entryEdiTestReached: true,
                entryEdiZeroFlag: true,
                zeroBranchReached: true,
                zeroBranchTaken: true,
                branchTargetRawFileOffset: ZeroBranchTargetRawFileOffset,
                stopAt: ZeroBranchTargetRawFileOffset);
        }

        return Stop(
            capture,
            FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E,
            readAddress,
            frameMinus4ReadReached: true,
            frameMinus4ReadCaptured: true,
            ebxAfterFrameMinus4Read: loadedEbx,
            entryEdiTestReached: true,
            entryEdiZeroFlag: false,
            zeroBranchReached: true,
            zeroBranchTaken: false,
            ecxEntryEdiMoveReached: true,
            ecxAfterMove: capture.EntryEdi,
            ediLoadedEbxMoveReached: true,
            ediAfterMove: loadedEbx,
            jumpReached: true,
            jumpTargetRawFileOffset: NonzeroJumpTargetRawFileOffset,
            stopAt: NonzeroJumpTargetRawFileOffset);
    }

    private static FrameHandoffCapacityFillJoinPlan Stop(
        FrameHandoffCapacityFillJoinCapture capture,
        FrameHandoffCapacityFillJoinOutcome outcome,
        uint frameMinus4ReadAddress,
        bool frameMinus4ReadReached = false,
        bool frameMinus4ReadCaptured = false,
        uint? ebxAfterFrameMinus4Read = null,
        bool unreachedFrameMinus4ReadCaptureProvided = false,
        bool entryEdiTestReached = false,
        bool? entryEdiZeroFlag = null,
        bool zeroBranchReached = false,
        bool? zeroBranchTaken = null,
        uint? branchTargetRawFileOffset = null,
        bool ecxEntryEdiMoveReached = false,
        uint? ecxAfterMove = null,
        bool ediLoadedEbxMoveReached = false,
        uint? ediAfterMove = null,
        bool jumpReached = false,
        uint? jumpTargetRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            frameMinus4ReadAddress,
            frameMinus4ReadReached,
            frameMinus4ReadCaptured,
            ebxAfterFrameMinus4Read,
            unreachedFrameMinus4ReadCaptureProvided,
            entryEdiTestReached,
            entryEdiZeroFlag,
            zeroBranchReached,
            zeroBranchTaken,
            branchTargetRawFileOffset,
            ecxEntryEdiMoveReached,
            ecxAfterMove,
            ediLoadedEbxMoveReached,
            ediAfterMove,
            jumpReached,
            jumpTargetRawFileOffset,
            stopAt);
}
