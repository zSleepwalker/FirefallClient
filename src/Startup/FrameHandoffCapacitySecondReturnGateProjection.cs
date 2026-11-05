using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// EntryEbx is captured independently at this post-call gate entry, not inferred as callee-preserved.
/// </summary>
public readonly record struct FrameHandoffCapacitySecondReturnGateCapture(
    bool ValidatedNormalEntry,
    uint EntryEbp,
    uint EntryEbx,
    uint? FrameMinusC,
    uint? HelperReturnEax);

public enum FrameHandoffCapacitySecondReturnGateOutcome
{
    EntryNotValidated,
    MissingFrameMinusCCapture,
    MissingHelperReturnEaxCapture,
    FrameMinusCZeroBranchTaken,
    FrameMinusCNonzeroFallThrough
}

public readonly record struct FrameHandoffCapacitySecondReturnGateRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacitySecondReturnGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct FrameHandoffCapacitySecondReturnGatePlan(
    FrameHandoffCapacitySecondReturnGateCapture Capture,
    FrameHandoffCapacitySecondReturnGateOutcome Outcome,
    ImmutableArray<FrameHandoffCapacitySecondReturnGateRead> Reads,
    bool FrameMinusCReadReached,
    bool FrameMinusCCaptured,
    bool StackCleanupReached,
    bool EdxLeaReached,
    uint? EdxAfterLea,
    bool HelperReturnEaxConsumed,
    bool TestReached,
    bool? FrameMinusCIsZero,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool UnreachedFrameMinusCCaptureProvided,
    bool UnreachedHelperReturnEaxCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded second capacity-return gate through its conditional branch.
/// </summary>
public static class FrameHandoffCapacitySecondReturnGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C34;
    public const uint EntryVirtualAddress = 0x00FE7834;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C41;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE7841;
    public const uint ZeroBranchDestinationRawFileOffset = 0x00BE6C4A;
    public const uint ZeroBranchDestinationVirtualAddress = 0x00FE784A;
    public const uint FrameMinusCOffset = 0x0C;
    public const uint StackCleanupBytes = 0x0C;

    public const uint FrameMinusCReadRawFileOffset = 0x00BE6C34;
    public const uint StackCleanupRawFileOffset = 0x00BE6C37;
    public const uint EdxLeaRawFileOffset = 0x00BE6C3A;
    public const uint TestRawFileOffset = 0x00BE6C3D;
    public const uint ZeroBranchRawFileOffset = 0x00BE6C3F;

    public const string CapturedWindowBytes =
        "8B 4D F4 83 C4 0C 8D 14 03 85 C9 74 09";

    public static ImmutableArray<FrameHandoffCapacitySecondReturnGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Freshly read [EBP-0xC] into ECX",
                FrameMinusCReadRawFileOffset, "8B 4D F4"),
            Instruction("Clean up 12 stack bytes",
                StackCleanupRawFileOffset, "83 C4 0C"),
            Instruction(
                "Form EDX as independently captured post-call EBX plus opaque helper-return EAX with DWORD wraparound",
                EdxLeaRawFileOffset, "8D 14 03"),
            Instruction("Test ECX against itself",
                TestRawFileOffset, "85 C9"),
            Instruction("JZ to BE6C4A; otherwise continue at BE6C41",
                ZeroBranchRawFileOffset, "74 09",
                ZeroBranchDestinationRawFileOffset,
                ZeroBranchDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinusCOffset);

    public static uint AddEntryEbxAndHelperReturnEax(uint entryEbx, uint helperReturnEax) =>
        unchecked(entryEbx + helperReturnEax);

    public static FrameHandoffCapacitySecondReturnGatePlan Project(
        FrameHandoffCapacitySecondReturnGateCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacitySecondReturnGateRead>();

        if (!capture.ValidatedNormalEntry)
        {
            return Stop(capture,
                FrameHandoffCapacitySecondReturnGateOutcome.EntryNotValidated,
                reads,
                EntryRawFileOffset,
                unreachedFrameMinusCCaptureProvided: capture.FrameMinusC.HasValue,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue);
        }

        uint frameMinusCAddress = FrameMinusCAddress(capture.EntryEbp);
        reads.Add(new(FrameMinusCReadRawFileOffset, frameMinusCAddress, capture.FrameMinusC));
        if (capture.FrameMinusC is not uint frameMinusC)
        {
            return Stop(capture,
                FrameHandoffCapacitySecondReturnGateOutcome.MissingFrameMinusCCapture,
                reads,
                FrameMinusCReadRawFileOffset,
                frameMinusCReadReached: true,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue);
        }

        if (capture.HelperReturnEax is not uint helperReturnEax)
        {
            return Stop(capture,
                FrameHandoffCapacitySecondReturnGateOutcome.MissingHelperReturnEaxCapture,
                reads,
                EdxLeaRawFileOffset,
                frameMinusCReadReached: true,
                frameMinusCCaptured: true,
                stackCleanupReached: true);
        }

        uint edxAfterLea =
            AddEntryEbxAndHelperReturnEax(capture.EntryEbx, helperReturnEax);
        bool frameMinusCIsZero = frameMinusC == 0;
        uint destination = frameMinusCIsZero
            ? ZeroBranchDestinationRawFileOffset
            : NormalBoundaryRawFileOffsetExclusive;
        return BuildPlan(
            capture,
            frameMinusCIsZero
                ? FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
                : FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough,
            reads,
            frameMinusCReadReached: true,
            frameMinusCCaptured: true,
            stackCleanupReached: true,
            edxLeaReached: true,
            edxAfterLea: edxAfterLea,
            helperReturnEaxConsumed: true,
            testReached: true,
            frameMinusCIsZero: frameMinusCIsZero,
            branchReached: true,
            branchTaken: frameMinusCIsZero,
            selectedDestinationRawFileOffset: destination,
            stopAt: destination);
    }

    private static FrameHandoffCapacitySecondReturnGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes,
            branchTargetRawFileOffset, branchTargetVirtualAddress);

    private static FrameHandoffCapacitySecondReturnGatePlan Stop(
        FrameHandoffCapacitySecondReturnGateCapture capture,
        FrameHandoffCapacitySecondReturnGateOutcome outcome,
        ImmutableArray<FrameHandoffCapacitySecondReturnGateRead>.Builder reads,
        uint stopAt,
        bool frameMinusCReadReached = false,
        bool frameMinusCCaptured = false,
        bool stackCleanupReached = false,
        bool unreachedFrameMinusCCaptureProvided = false,
        bool unreachedHelperReturnEaxCaptureProvided = false) =>
        BuildPlan(capture, outcome, reads,
            frameMinusCReadReached,
            frameMinusCCaptured,
            stackCleanupReached,
            edxLeaReached: false,
            edxAfterLea: null,
            helperReturnEaxConsumed: false,
            testReached: false,
            frameMinusCIsZero: null,
            branchReached: false,
            branchTaken: null,
            selectedDestinationRawFileOffset: null,
            unreachedFrameMinusCCaptureProvided,
            unreachedHelperReturnEaxCaptureProvided,
            stopAt);

    private static FrameHandoffCapacitySecondReturnGatePlan BuildPlan(
        FrameHandoffCapacitySecondReturnGateCapture capture,
        FrameHandoffCapacitySecondReturnGateOutcome outcome,
        ImmutableArray<FrameHandoffCapacitySecondReturnGateRead>.Builder reads,
        bool frameMinusCReadReached,
        bool frameMinusCCaptured,
        bool stackCleanupReached,
        bool edxLeaReached,
        uint? edxAfterLea,
        bool helperReturnEaxConsumed,
        bool testReached,
        bool? frameMinusCIsZero,
        bool branchReached,
        bool? branchTaken,
        uint? selectedDestinationRawFileOffset,
        bool unreachedFrameMinusCCaptureProvided = false,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            frameMinusCReadReached,
            frameMinusCCaptured,
            stackCleanupReached,
            edxLeaReached,
            edxAfterLea,
            helperReturnEaxConsumed,
            testReached,
            frameMinusCIsZero,
            branchReached,
            branchTaken,
            selectedDestinationRawFileOffset,
            unreachedFrameMinusCCaptureProvided,
            unreachedHelperReturnEaxCaptureProvided,
            stopAt);
}
