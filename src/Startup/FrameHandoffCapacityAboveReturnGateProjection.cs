using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveReturnGateCapture(
    bool ValidatedNormalEntry,
    uint EntryEbp,
    uint EntryEbx,
    uint EntryEsi,
    uint? HelperReturnEax,
    uint? FrameMinus4);

public enum FrameHandoffCapacityAboveReturnGateOutcome
{
    ZeroBranchTaken,
    NonzeroFallThrough
}

public readonly record struct FrameHandoffCapacityAboveReturnGateRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityAboveReturnGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct FrameHandoffCapacityAboveReturnGatePlan(
    FrameHandoffCapacityAboveReturnGateCapture Capture,
    FrameHandoffCapacityAboveReturnGateOutcome? Outcome,
    ImmutableArray<FrameHandoffCapacityAboveReturnGateRead> Reads,
    bool StackCleanupReached,
    bool EdiLeaReached,
    uint? EdiAfterLea,
    bool HelperReturnEaxConsumed,
    bool FrameMinus4ReadReached,
    bool FrameMinus4Captured,
    uint? EbxAfterFrameMinus4Read,
    bool TestReached,
    bool? EntryEsiIsZero,
    bool BranchReached,
    bool? BranchTaken,
    bool EcxMoveReached,
    uint? EcxAfterMove,
    uint? SelectedDestinationRawFileOffset,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool UnreachedFrameMinus4CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the validated normal-entry path through its return gate.
/// </summary>
public static class FrameHandoffCapacityAboveReturnGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C8F;
    public const uint EntryVirtualAddress = 0x00FE788F;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C9E;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE789E;
    public const uint ZeroBranchDestinationRawFileOffset = 0x00BE6CA2;
    public const uint ZeroBranchDestinationVirtualAddress = 0x00FE78A2;
    public const uint FrameMinus4Offset = 0x04;
    public const uint StackCleanupBytes = 0x18;

    public const uint StackCleanupRawFileOffset = 0x00BE6C8F;
    public const uint EdiLeaRawFileOffset = 0x00BE6C92;
    public const uint FrameMinus4ReadRawFileOffset = 0x00BE6C95;
    public const uint TestRawFileOffset = 0x00BE6C98;
    public const uint ZeroBranchRawFileOffset = 0x00BE6C9A;
    public const uint EcxMoveRawFileOffset = 0x00BE6C9C;

    public const string CapturedWindowBytes =
        "83 C4 18 8D 3C 03 8B 5D FC 85 F6 74 06 8B CE";

    public static ImmutableArray<FrameHandoffCapacityAboveReturnGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Clean up 24 stack bytes before the LEA",
                StackCleanupRawFileOffset, "83 C4 18"),
            Instruction(
                "Form EDI as independently captured entry EBX plus opaque helper-return EAX with DWORD wraparound",
                EdiLeaRawFileOffset, "8D 3C 03"),
            Instruction("Freshly read [EBP-4] into EBX",
                FrameMinus4ReadRawFileOffset, "8B 5D FC"),
            Instruction("Test independently captured entry ESI",
                TestRawFileOffset, "85 F6"),
            Instruction("JZ to BE6CA2; otherwise continue at BE6C9C",
                ZeroBranchRawFileOffset, "74 06",
                ZeroBranchDestinationRawFileOffset,
                ZeroBranchDestinationVirtualAddress),
            Instruction("Copy entry ESI into ECX on the nonzero fallthrough",
                EcxMoveRawFileOffset, "8B CE"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Offset);

    public static uint AddEntryEbxAndHelperReturnEax(uint entryEbx, uint helperReturnEax) =>
        unchecked(entryEbx + helperReturnEax);

    public static FrameHandoffCapacityAboveReturnGatePlan Project(
        FrameHandoffCapacityAboveReturnGateCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveReturnGateRead>();

        if (!capture.ValidatedNormalEntry)
        {
            return BuildPlan(
                capture,
                reads,
                stopAt: EntryRawFileOffset,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4.HasValue);
        }

        if (capture.HelperReturnEax is not uint helperReturnEax)
        {
            return BuildPlan(
                capture,
                reads,
                stackCleanupReached: true,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4.HasValue,
                stopAt: EdiLeaRawFileOffset);
        }

        uint ediAfterLea =
            AddEntryEbxAndHelperReturnEax(capture.EntryEbx, helperReturnEax);
        uint frameMinus4Address = FrameMinus4Address(capture.EntryEbp);
        reads.Add(new(
            FrameMinus4ReadRawFileOffset,
            frameMinus4Address,
            capture.FrameMinus4));

        if (capture.FrameMinus4 is not uint frameMinus4)
        {
            return BuildPlan(
                capture,
                reads,
                stackCleanupReached: true,
                ediLeaReached: true,
                ediAfterLea: ediAfterLea,
                helperReturnEaxConsumed: true,
                frameMinus4ReadReached: true,
                stopAt: FrameMinus4ReadRawFileOffset);
        }

        bool entryEsiIsZero = capture.EntryEsi == 0;
        uint destination = entryEsiIsZero
            ? ZeroBranchDestinationRawFileOffset
            : NormalBoundaryRawFileOffsetExclusive;
        return BuildPlan(
            capture,
            reads,
            outcome: entryEsiIsZero
                ? FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
                : FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough,
            stackCleanupReached: true,
            ediLeaReached: true,
            ediAfterLea: ediAfterLea,
            helperReturnEaxConsumed: true,
            frameMinus4ReadReached: true,
            frameMinus4Captured: true,
            ebxAfterFrameMinus4Read: frameMinus4,
            testReached: true,
            entryEsiIsZero: entryEsiIsZero,
            branchReached: true,
            branchTaken: entryEsiIsZero,
            ecxMoveReached: !entryEsiIsZero,
            ecxAfterMove: entryEsiIsZero ? null : capture.EntryEsi,
            selectedDestinationRawFileOffset: destination,
            stopAt: destination);
    }

    private static FrameHandoffCapacityAboveReturnGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes,
            branchTargetRawFileOffset, branchTargetVirtualAddress);

    private static FrameHandoffCapacityAboveReturnGatePlan BuildPlan(
        FrameHandoffCapacityAboveReturnGateCapture capture,
        ImmutableArray<FrameHandoffCapacityAboveReturnGateRead>.Builder reads,
        FrameHandoffCapacityAboveReturnGateOutcome? outcome = null,
        bool stackCleanupReached = false,
        bool ediLeaReached = false,
        uint? ediAfterLea = null,
        bool helperReturnEaxConsumed = false,
        bool frameMinus4ReadReached = false,
        bool frameMinus4Captured = false,
        uint? ebxAfterFrameMinus4Read = null,
        bool testReached = false,
        bool? entryEsiIsZero = null,
        bool branchReached = false,
        bool? branchTaken = null,
        bool ecxMoveReached = false,
        uint? ecxAfterMove = null,
        uint? selectedDestinationRawFileOffset = null,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        bool unreachedFrameMinus4CaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            stackCleanupReached,
            ediLeaReached,
            ediAfterLea,
            helperReturnEaxConsumed,
            frameMinus4ReadReached,
            frameMinus4Captured,
            ebxAfterFrameMinus4Read,
            testReached,
            entryEsiIsZero,
            branchReached,
            branchTaken,
            ecxMoveReached,
            ecxAfterMove,
            selectedDestinationRawFileOffset,
            unreachedHelperReturnEaxCaptureProvided,
            unreachedFrameMinus4CaptureProvided,
            stopAt);
}
