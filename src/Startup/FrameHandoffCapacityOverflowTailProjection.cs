using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityOverflowTailCapture(
    bool ValidatedOverflowEntry,
    uint? CallTargetAddress,
    bool? CallReturnedNormally);

public enum FrameHandoffCapacityOverflowTailOutcome
{
    EntryNotValidated,
    MissingCallTargetCapture,
    MissingCallReturnCapture,
    CallDidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct FrameHandoffCapacityOverflowTailPush(
    uint RawFileOffset,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacityOverflowTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacityOverflowTailPlan(
    FrameHandoffCapacityOverflowTailCapture Capture,
    FrameHandoffCapacityOverflowTailOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityOverflowTailPush> Pushes,
    bool PushReached,
    bool CallReached,
    bool? CallReturnedNormally,
    bool NormalBoundaryReached,
    bool UnreachedCallTargetCaptureProvided,
    bool UnreachedCallReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded overflow tail through its indirect call. It does not compose the loop.
/// </summary>
public static class FrameHandoffCapacityOverflowTailProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint OverflowBranchRawFileOffset = 0x00BE6B87;
    public const uint OverflowBranchTargetRawFileOffset = 0x00BE6CC0;
    public const uint OverflowEntryRawFileOffset = 0x00BE6CC0;
    public const uint OverflowEntryVirtualAddress = 0x00FE78C0;
    public const uint CallRawFileOffset = 0x00BE6CC5;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6CCB;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE78CB;
    public const uint CallTargetCellAddress = 0x01AC45C0;
    public const uint PushedValueVirtualAddress = 0x01C607A4;

    public const string OverflowBranchBytes = "0F 82 33 01 00 00";
    public const string CapturedWindowBytes =
        "68 A4 07 C6 01 FF 15 C0 45 AC 01";

    public static ImmutableArray<FrameHandoffCapacityOverflowTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "The overflow JB selects the bounded tail at raw 0xBE6CC0",
                OverflowBranchRawFileOffset,
                OverflowBranchBytes),
            Instruction(
                "Push the immediate VA 0x01C607A4",
                OverflowEntryRawFileOffset,
                "68 A4 07 C6 01"),
            Instruction(
                "Call through the DWORD cell at VA 0x01AC45C0",
                CallRawFileOffset,
                "FF 15 C0 45 AC 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static FrameHandoffCapacityOverflowTailPlan Project(
        FrameHandoffCapacityOverflowTailCapture capture)
    {
        var pushes = ImmutableArray.CreateBuilder<FrameHandoffCapacityOverflowTailPush>();
        if (!capture.ValidatedOverflowEntry)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityOverflowTailOutcome.EntryNotValidated,
                pushes,
                unreachedCallTargetCaptureProvided: capture.CallTargetAddress.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: OverflowEntryRawFileOffset);
        }

        pushes.Add(new(
            OverflowEntryRawFileOffset,
            "Immediate VA",
            PushedValueVirtualAddress));

        if (capture.CallTargetAddress is not uint)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityOverflowTailOutcome.MissingCallTargetCapture,
                pushes,
                pushReached: true,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityOverflowTailOutcome.MissingCallReturnCapture,
                pushes,
                pushReached: true,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? FrameHandoffCapacityOverflowTailOutcome.ReturnedNormally
                : FrameHandoffCapacityOverflowTailOutcome.CallDidNotReturnNormally,
            pushes,
            pushReached: true,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            normalBoundaryReached: callReturnedNormally,
            stopAt: callReturnedNormally
                ? NormalBoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static FrameHandoffCapacityOverflowTailInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);

    private static FrameHandoffCapacityOverflowTailPlan BuildPlan(
        FrameHandoffCapacityOverflowTailCapture capture,
        FrameHandoffCapacityOverflowTailOutcome outcome,
        ImmutableArray<FrameHandoffCapacityOverflowTailPush>.Builder pushes,
        bool pushReached = false,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool normalBoundaryReached = false,
        bool unreachedCallTargetCaptureProvided = false,
        bool unreachedCallReturnCaptureProvided = false,
        uint stopAt = CallRawFileOffset) =>
        new(
            capture,
            outcome,
            pushes.ToImmutable(),
            pushReached,
            callReached,
            callReached ? callReturnedNormally : null,
            normalBoundaryReached,
            unreachedCallTargetCaptureProvided,
            unreachedCallReturnCaptureProvided,
            stopAt);
}
