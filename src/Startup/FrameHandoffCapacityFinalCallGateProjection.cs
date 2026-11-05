using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFinalCallGateCapture(
    bool CaptureValidatedJoin,
    uint EntryEbp,
    uint? FrameMinus14,
    uint? Receiver4,
    bool? NormalCallReturned);

public enum FrameHandoffCapacityFinalCallGateOutcome
{
    JoinNotValidated,
    MissingFrameMinus14,
    MissingReceiver4,
    ZeroReceiver4CallSkipped,
    ZeroReceiver4StatusCaptureUnreached,
    MissingNormalCallStatus,
    NormalCallDidNotReturn,
    NormalCallReturned
}

public readonly record struct FrameHandoffCapacityFinalCallGateRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityFinalCallGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? CallTargetRawFileOffset = null,
    uint? CallTargetVirtualAddress = null);

public readonly record struct FrameHandoffCapacityFinalCallGatePlan(
    FrameHandoffCapacityFinalCallGateCapture Capture,
    FrameHandoffCapacityFinalCallGateOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityFinalCallGateRead> Reads,
    bool FrameMinus14ReadReached,
    bool FrameMinus14Captured,
    uint? EdiAfterFrameMinus14Load,
    bool Receiver4ReadReached,
    bool Receiver4Captured,
    uint? Receiver4Address,
    bool TestReached,
    bool? Receiver4IsZero,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool CallReached,
    bool? CallReturned,
    uint? OutgoingEdi,
    bool NormalBoundaryAdmitted,
    bool UnreachedFrameMinus14CaptureProvided,
    bool UnreachedReceiver4CaptureProvided,
    bool UnreachedNormalCallStatusProvided,
    bool BoundaryAdmissionBlockedByUnreachedCallStatus,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the final capacity handoff gate through its optional call and normal boundary.
/// </summary>
public static class FrameHandoffCapacityFinalCallGateProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6CA2;
    public const uint EntryVirtualAddress = 0x00FE78A2;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6CB1;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE78B1;
    public const uint ZeroBranchDestinationRawFileOffset = 0x00BE6CB1;
    public const uint ZeroBranchDestinationVirtualAddress = 0x00FE78B1;
    public const uint CallTargetRawFileOffset = 0x012C8D60;
    public const uint CallTargetVirtualAddress = 0x016C9960;
    public const uint FrameMinus14Offset = 0x14;
    public const uint Receiver4Offset = 0x04;

    public const uint FrameMinus14ReadRawFileOffset = 0x00BE6CA2;
    public const uint Receiver4ReadRawFileOffset = 0x00BE6CA5;
    public const uint TestRawFileOffset = 0x00BE6CA8;
    public const uint ZeroBranchRawFileOffset = 0x00BE6CAA;
    public const uint CallRawFileOffset = 0x00BE6CAC;

    public const string CapturedWindowBytes =
        "8B 7D EC 8B 4F 04 85 C9 74 05 E8 AF 20 6E 00";

    public static ImmutableArray<FrameHandoffCapacityFinalCallGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Freshly read [EBP-0x14] into EDI",
                FrameMinus14ReadRawFileOffset, "8B 7D EC"),
            Instruction("Freshly read [EDI+4] into ECX",
                Receiver4ReadRawFileOffset, "8B 4F 04"),
            Instruction("Test ECX against itself",
                TestRawFileOffset, "85 C9"),
            Instruction("JZ to BE6CB1; otherwise continue at BE6CAC",
                ZeroBranchRawFileOffset, "74 05",
                branchTargetRawFileOffset: ZeroBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: ZeroBranchDestinationVirtualAddress),
            Instruction("Call raw 12C8D60 on the nonzero fallthrough",
                CallRawFileOffset, "E8 AF 20 6E 00",
                callTargetRawFileOffset: CallTargetRawFileOffset,
                callTargetVirtualAddress: CallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus14Offset);

    public static uint Receiver4Address(uint ediAfterFrameMinus14Load) =>
        unchecked(ediAfterFrameMinus14Load + Receiver4Offset);

    public static uint CallRelativeDisplacement =>
        unchecked(CallTargetVirtualAddress
            - RawFileOffsetToVirtualAddress(NormalBoundaryRawFileOffsetExclusive));

    public static FrameHandoffCapacityFinalCallGatePlan Project(
        FrameHandoffCapacityFinalCallGateCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityFinalCallGateRead>();

        if (!capture.CaptureValidatedJoin)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityFinalCallGateOutcome.JoinNotValidated,
                reads,
                unreachedFrameMinus14CaptureProvided: capture.FrameMinus14.HasValue,
                unreachedReceiver4CaptureProvided: capture.Receiver4.HasValue,
                unreachedNormalCallStatusProvided: capture.NormalCallReturned.HasValue);
        }

        uint frameMinus14Address = FrameMinus14Address(capture.EntryEbp);
        reads.Add(new(
            FrameMinus14ReadRawFileOffset,
            frameMinus14Address,
            capture.FrameMinus14));
        if (capture.FrameMinus14 is not uint frameMinus14)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityFinalCallGateOutcome.MissingFrameMinus14,
                reads,
                frameMinus14ReadReached: true,
                unreachedReceiver4CaptureProvided: capture.Receiver4.HasValue,
                unreachedNormalCallStatusProvided: capture.NormalCallReturned.HasValue,
                stopAt: FrameMinus14ReadRawFileOffset);
        }

        uint receiver4Address = Receiver4Address(frameMinus14);
        reads.Add(new(
            Receiver4ReadRawFileOffset,
            receiver4Address,
            capture.Receiver4));
        if (capture.Receiver4 is not uint receiver4)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityFinalCallGateOutcome.MissingReceiver4,
                reads,
                frameMinus14ReadReached: true,
                frameMinus14Captured: true,
                ediAfterFrameMinus14Load: frameMinus14,
                receiver4ReadReached: true,
                receiver4Address: receiver4Address,
                unreachedNormalCallStatusProvided: capture.NormalCallReturned.HasValue,
                stopAt: Receiver4ReadRawFileOffset);
        }

        bool receiver4IsZero = receiver4 == 0;
        if (receiver4IsZero)
        {
            bool statusProvided = capture.NormalCallReturned.HasValue;
            return BuildPlan(
                capture,
                statusProvided
                    ? FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4StatusCaptureUnreached
                    : FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped,
                reads,
                frameMinus14ReadReached: true,
                frameMinus14Captured: true,
                ediAfterFrameMinus14Load: frameMinus14,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4Address: receiver4Address,
                testReached: true,
                receiver4IsZero: true,
                branchReached: true,
                branchTaken: true,
                selectedDestinationRawFileOffset: ZeroBranchDestinationRawFileOffset,
                outgoingEdi: frameMinus14,
                normalBoundaryAdmitted: !statusProvided,
                unreachedNormalCallStatusProvided: statusProvided,
                boundaryAdmissionBlockedByUnreachedCallStatus: statusProvided,
                stopAt: ZeroBranchDestinationRawFileOffset);
        }

        if (capture.NormalCallReturned is not bool callReturned)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityFinalCallGateOutcome.MissingNormalCallStatus,
                reads,
                frameMinus14ReadReached: true,
                frameMinus14Captured: true,
                ediAfterFrameMinus14Load: frameMinus14,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4Address: receiver4Address,
                testReached: true,
                receiver4IsZero: false,
                branchReached: true,
                branchTaken: false,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        if (!callReturned)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityFinalCallGateOutcome.NormalCallDidNotReturn,
                reads,
                frameMinus14ReadReached: true,
                frameMinus14Captured: true,
                ediAfterFrameMinus14Load: frameMinus14,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4Address: receiver4Address,
                testReached: true,
                receiver4IsZero: false,
                branchReached: true,
                branchTaken: false,
                callReached: true,
                callReturned: false,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned,
            reads,
            frameMinus14ReadReached: true,
            frameMinus14Captured: true,
            ediAfterFrameMinus14Load: frameMinus14,
            receiver4ReadReached: true,
            receiver4Captured: true,
            receiver4Address: receiver4Address,
            testReached: true,
            receiver4IsZero: false,
            branchReached: true,
            branchTaken: false,
            selectedDestinationRawFileOffset: NormalBoundaryRawFileOffsetExclusive,
            callReached: true,
            callReturned: true,
            normalBoundaryAdmitted: true,
            stopAt: NormalBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityFinalCallGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null,
        uint? callTargetRawFileOffset = null,
        uint? callTargetVirtualAddress = null) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes,
            branchTargetRawFileOffset, branchTargetVirtualAddress,
            callTargetRawFileOffset, callTargetVirtualAddress);

    private static FrameHandoffCapacityFinalCallGatePlan BuildPlan(
        FrameHandoffCapacityFinalCallGateCapture capture,
        FrameHandoffCapacityFinalCallGateOutcome outcome,
        ImmutableArray<FrameHandoffCapacityFinalCallGateRead>.Builder reads,
        bool frameMinus14ReadReached = false,
        bool frameMinus14Captured = false,
        uint? ediAfterFrameMinus14Load = null,
        bool receiver4ReadReached = false,
        bool receiver4Captured = false,
        uint? receiver4Address = null,
        bool testReached = false,
        bool? receiver4IsZero = null,
        bool branchReached = false,
        bool? branchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        bool callReached = false,
        bool? callReturned = null,
        uint? outgoingEdi = null,
        bool normalBoundaryAdmitted = false,
        bool unreachedFrameMinus14CaptureProvided = false,
        bool unreachedReceiver4CaptureProvided = false,
        bool unreachedNormalCallStatusProvided = false,
        bool boundaryAdmissionBlockedByUnreachedCallStatus = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            frameMinus14ReadReached,
            frameMinus14Captured,
            ediAfterFrameMinus14Load,
            receiver4ReadReached,
            receiver4Captured,
            receiver4Address,
            testReached,
            receiver4IsZero,
            branchReached,
            branchTaken,
            selectedDestinationRawFileOffset,
            callReached,
            callReturned,
            outgoingEdi,
            normalBoundaryAdmitted,
            unreachedFrameMinus14CaptureProvided,
            unreachedReceiver4CaptureProvided,
            unreachedNormalCallStatusProvided,
            boundaryAdmissionBlockedByUnreachedCallStatus,
            stopAt);
}
