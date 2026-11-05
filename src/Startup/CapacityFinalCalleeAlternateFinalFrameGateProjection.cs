using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateFinalFrameGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus14,
    uint? EntryECX);

public enum CapacityFinalCalleeAlternateFinalFrameGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus14,
    MissingEntryECX,
    EqualToFallthrough,
    UnequalToBranchTarget
}

public readonly record struct CapacityFinalCalleeAlternateFinalFrameGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateFinalFrameGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateFinalFrameGatePlan(
    CapacityFinalCalleeAlternateFinalFrameGateCapture Capture,
    CapacityFinalCalleeAlternateFinalFrameGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus14ReadReached { get; init; }
    public uint? FrameMinus14Address { get; init; }
    public bool FrameMinus14Captured { get; init; }
    public uint? FrameMinus14 { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateFinalFrameGateFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedFreshFrameMinus14CaptureProvided { get; init; }
    public bool UnreachedEntryECXCaptureProvided { get; init; }
    public bool LateFreshFrameMinus14CaptureProvided { get; init; }
    public bool LateEntryECXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh EBP-14 load and following CMP EAX,ECX/JNZ only.
/// The frame DWORD is independent of prior stores, and both branch
/// destinations are exact stop points outside this seven-byte window.
/// </summary>
public static class CapacityFinalCalleeAlternateFinalFrameGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9160;
    public const uint EntryVirtualAddress = 0x016C9D60;
    public const uint FrameReadRawFileOffset = 0x012C9160;
    public const uint CompareRawFileOffset = 0x012C9163;
    public const uint JnzRawFileOffset = 0x012C9165;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9167;
    public const uint EqualDestinationRawFileOffset = 0x012C9167;
    public const uint UnequalDestinationRawFileOffset = 0x012C917C;

    public const uint EbpMinus14Displacement = 0x14;

    public const string CapturedWindowBytes = "8B 45 EC 3B C1 75 15";

    public static ImmutableArray<CapacityFinalCalleeAlternateFinalFrameGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-14 into EAX",
                FrameReadRawFileOffset, "8B 45 EC"),
            Instruction("Compare the fresh EBP-14 value in EAX with entry ECX",
                CompareRawFileOffset, "3B C1"),
            Instruction("Jump to 917C when the comparison is not equal",
                JnzRawFileOffset, "75 15",
                branchTargetRawFileOffset: UnequalDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus14Displacement);

    public static CapacityFinalCalleeAlternateFinalFrameGatePlan Project(
        CapacityFinalCalleeAlternateFinalFrameGateCapture capture)
    {
        bool ebpCaptured = false;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool ecxCaptured = false;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateFinalFrameGateFlags? compareFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;
        bool unreachedFreshFrame = false;
        bool unreachedECX = false;
        bool lateFreshFrame = false;
        bool lateECX = false;

        CapacityFinalCalleeAlternateFinalFrameGatePlan Finish(
            CapacityFinalCalleeAlternateFinalFrameGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinus14ReadReached = frameReadReached,
                FrameMinus14Address = frameAddress,
                FrameMinus14Captured = frameCaptured,
                FrameMinus14 = frameValue,
                EntryECXCaptured = ecxCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                UnreachedFreshFrameMinus14CaptureProvided = unreachedFreshFrame,
                UnreachedEntryECXCaptureProvided = unreachedECX,
                LateFreshFrameMinus14CaptureProvided = lateFreshFrame,
                LateEntryECXCaptureProvided = lateECX,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedFreshFrame = capture.FreshFrameMinus14.HasValue;
            unreachedECX = capture.EntryECX.HasValue;
            lateFreshFrame = unreachedFreshFrame;
            lateECX = unreachedECX;
            return Finish(
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryEBP,
                FrameReadRawFileOffset);
        }

        ebpCaptured = true;
        frameReadReached = true;
        frameAddress = DeriveEbpMinus14Address(entryEbp);
        if (capture.FreshFrameMinus14 is not uint freshFrameMinus14)
        {
            lateECX = capture.EntryECX.HasValue;
            unreachedECX = lateECX;
            return Finish(
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14,
                FrameReadRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshFrameMinus14;
        if (capture.EntryECX is not uint entryEcx)
        {
            return Finish(
                CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryECX,
                CompareRawFileOffset);
        }

        ecxCaptured = true;
        compareReached = true;
        compareLeft = freshFrameMinus14;
        compareRight = entryEcx;
        compareResult = unchecked(freshFrameMinus14 - entryEcx);
        compareFlags = CalculateCompareFlags(
            freshFrameMinus14, entryEcx, compareResult.Value);
        jnzReached = true;
        jnzTaken = !compareFlags.Value.Zero;
        jnzDestination = jnzTaken.Value
            ? UnequalDestinationRawFileOffset
            : EqualDestinationRawFileOffset;

        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeAlternateFinalFrameGateOutcome.UnequalToBranchTarget
                : CapacityFinalCalleeAlternateFinalFrameGateOutcome.EqualToFallthrough,
            jnzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateFinalFrameGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateFinalFrameGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
