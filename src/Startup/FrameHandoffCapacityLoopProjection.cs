using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityLoopCapture(
    uint EntryEsi,
    uint EntryEdx);

public enum FrameHandoffCapacityLoopOutcome
{
    ExitAtRawFileOffset91,
    FailureAtRawFileOffset6CC0,
    DoubleAndLoopToRawFileOffset70
}

public readonly record struct FrameHandoffCapacityLoopPlan(
    FrameHandoffCapacityLoopCapture Capture,
    FrameHandoffCapacityLoopOutcome Outcome,
    bool SubtractReached,
    uint EaxAfterEntrySubtract,
    bool CompareEaxWithOneReached,
    bool? EntrySubtractResultIsZero,
    bool? ZeroDifferenceBranchTaken,
    bool CompareEsiWithEightReached,
    bool? EsiAtLeastEight,
    bool? ExitBranchTaken,
    bool LoadUpperConstantReached,
    uint? EaxAfterUpperConstantLoad,
    bool UpperSubtractReached,
    uint? EaxAfterUpperSubtract,
    bool CompareUpperSubtractWithEsiReached,
    bool? UpperSubtractResultLessThanEsi,
    bool? FailureBranchTaken,
    bool DoubleEsiReached,
    uint? EsiAfterDouble,
    bool LoopBackReached,
    uint SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacityLoopInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects exactly one execution of the bounded frame-handoff capacity-loop slice.
/// </summary>
public static class FrameHandoffCapacityLoopProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6B70;
    public const uint EntryVirtualAddress = 0x00FE7770;
    public const uint ExitRawFileOffset = 0x00BE6B91;
    public const uint FailureRawFileOffset = 0x00BE6CC0;
    public const uint LoopRawFileOffset = EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive = 0x00BE6B91;
    public const uint UpperSubtractConstant = 0x0FFFFFFF;
    public const uint ExitThreshold = 8;

    public const uint EntrySubtractRawFileOffset = 0x00BE6B72;
    public const uint EntryCompareRawFileOffset = 0x00BE6B74;
    public const uint ZeroDifferenceBranchRawFileOffset = 0x00BE6B77;
    public const uint EsiCompareRawFileOffset = 0x00BE6B79;
    public const uint ExitBranchRawFileOffset = 0x00BE6B7C;
    public const uint UpperConstantLoadRawFileOffset = 0x00BE6B7E;
    public const uint UpperSubtractRawFileOffset = 0x00BE6B83;
    public const uint FailureCompareRawFileOffset = 0x00BE6B85;
    public const uint FailureBranchRawFileOffset = 0x00BE6B87;
    public const uint EsiDoubleRawFileOffset = 0x00BE6B8D;
    public const uint LoopBranchRawFileOffset = 0x00BE6B8F;

    public const string CapturedWindowBytes =
        "8B C6 2B C2 83 F8 01 72 05 83 FE 08 73 13 B8 FF FF FF 0F "
        + "2B C6 3B C6 0F 82 33 01 00 00 03 F6 EB DF";

    public static ImmutableArray<FrameHandoffCapacityLoopInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Copy entry ESI to EAX",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "8B C6"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Subtract entry EDX from EAX with 32-bit wraparound",
                EntrySubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(EntrySubtractRawFileOffset),
                "2B C2"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Compare the subtraction result with one",
                EntryCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryCompareRawFileOffset),
                "83 F8 01"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "JB bypasses the ESI-versus-eight gate when the subtraction result is zero",
                ZeroDifferenceBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroDifferenceBranchRawFileOffset),
                "72 05",
                UpperConstantLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(UpperConstantLoadRawFileOffset)),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Compare ESI with eight",
                EsiCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiCompareRawFileOffset),
                "83 FE 08"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "JNB exits at raw 0xBE6B91 when ESI is at least eight",
                ExitBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ExitBranchRawFileOffset),
                "73 13",
                ExitRawFileOffset,
                RawFileOffsetToVirtualAddress(ExitRawFileOffset)),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Load the upper-subtract constant",
                UpperConstantLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(UpperConstantLoadRawFileOffset),
                "B8 FF FF FF 0F"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Subtract ESI from the constant with 32-bit wraparound",
                UpperSubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(UpperSubtractRawFileOffset),
                "2B C6"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Compare the upper-subtract result with ESI",
                FailureCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(FailureCompareRawFileOffset),
                "3B C6"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "JB selects failure at raw 0xBE6CC0",
                FailureBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(FailureBranchRawFileOffset),
                "0F 82 33 01 00 00",
                FailureRawFileOffset,
                RawFileOffsetToVirtualAddress(FailureRawFileOffset)),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Double ESI with 32-bit wraparound",
                EsiDoubleRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiDoubleRawFileOffset),
                "03 F6"),
            new FrameHandoffCapacityLoopInstructionEvidence(
                "Jump back to raw 0xBE6B70; the projection does not execute another iteration",
                LoopBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(LoopBranchRawFileOffset),
                "EB DF",
                LoopRawFileOffset,
                RawFileOffsetToVirtualAddress(LoopRawFileOffset)));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2u + (uint)(sbyte)displacement);

    public static uint RelativeBranchTarget(
        uint instructionRawFileOffset,
        uint instructionLength,
        uint displacement) =>
        unchecked(instructionRawFileOffset + instructionLength + displacement);

    public static FrameHandoffCapacityLoopPlan Project(
        FrameHandoffCapacityLoopCapture capture)
    {
        uint eaxAfterEntrySubtract = unchecked(capture.EntryEsi - capture.EntryEdx);
        bool entrySubtractResultIsZero = eaxAfterEntrySubtract == 0;
        bool compareEsiWithEightReached = !entrySubtractResultIsZero;

        if (compareEsiWithEightReached && capture.EntryEsi >= ExitThreshold)
        {
            return new(
                capture,
                FrameHandoffCapacityLoopOutcome.ExitAtRawFileOffset91,
                true,
                eaxAfterEntrySubtract,
                true,
                entrySubtractResultIsZero,
                false,
                true,
                true,
                true,
                false,
                null,
                false,
                null,
                false,
                null,
                null,
                false,
                null,
                false,
                ExitRawFileOffset,
                ExitRawFileOffset);
        }

        uint eaxAfterUpperSubtract = unchecked(UpperSubtractConstant - capture.EntryEsi);
        bool upperSubtractResultLessThanEsi = eaxAfterUpperSubtract < capture.EntryEsi;
        if (upperSubtractResultLessThanEsi)
        {
            return new(
                capture,
                FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0,
                true,
                eaxAfterEntrySubtract,
                true,
                entrySubtractResultIsZero,
                entrySubtractResultIsZero,
                compareEsiWithEightReached,
                compareEsiWithEightReached ? capture.EntryEsi >= ExitThreshold : null,
                compareEsiWithEightReached ? false : null,
                true,
                UpperSubtractConstant,
                true,
                eaxAfterUpperSubtract,
                true,
                true,
                true,
                false,
                null,
                false,
                FailureRawFileOffset,
                FailureRawFileOffset);
        }

        return new(
            capture,
            FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70,
            true,
            eaxAfterEntrySubtract,
            true,
            entrySubtractResultIsZero,
            entrySubtractResultIsZero,
            compareEsiWithEightReached,
            compareEsiWithEightReached ? capture.EntryEsi >= ExitThreshold : null,
            compareEsiWithEightReached ? false : null,
            true,
            UpperSubtractConstant,
            true,
            eaxAfterUpperSubtract,
            true,
            false,
            false,
            true,
            unchecked(capture.EntryEsi + capture.EntryEsi),
            true,
            LoopRawFileOffset,
            EndRawFileOffsetExclusive);
    }
}
