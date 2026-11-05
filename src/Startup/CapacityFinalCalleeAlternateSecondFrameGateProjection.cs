using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateSecondFrameGateCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshFrameMinus10);

public enum CapacityFinalCalleeAlternateSecondFrameGateOutcome
{
    MissingEntryEDX,
    MissingEntryEBP,
    MissingFreshFrameMinus10,
    EqualToOpaqueCallBoundary,
    UnequalToBranchTarget
}

public readonly record struct CapacityFinalCalleeAlternateSecondFrameGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateSecondFrameGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateSecondFrameGateWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeAlternateSecondFrameGatePlan(
    CapacityFinalCalleeAlternateSecondFrameGateCapture Capture,
    CapacityFinalCalleeAlternateSecondFrameGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus10ReadReached { get; init; }
    public uint? FrameMinus10Address { get; init; }
    public bool FrameMinus10Captured { get; init; }
    public uint? FrameMinus10 { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateSecondFrameGateFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool LateFreshFrameMinus10CaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateSecondFrameGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateSecondFrameGateWrite>.Empty;
    public bool OpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh entry-frame CMP/JNZ at 9122 only. The frame operand
/// is independently captured and is not inferred from an earlier frame store.
/// </summary>
public static class CapacityFinalCalleeAlternateSecondFrameGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9122;
    public const uint EntryVirtualAddress = 0x016C9D22;
    public const uint CompareRawFileOffset = 0x012C9122;
    public const uint JnzRawFileOffset = 0x012C9125;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9127;
    public const uint OpaqueCallRawFileOffset = 0x012C9127;
    public const uint OpaqueCallCellVirtualAddress = 0x01AC4A24;
    public const uint EqualDestinationRawFileOffset = 0x012C9127;
    public const uint UnequalDestinationRawFileOffset = 0x012C912D;

    public const uint EbpMinus10Displacement = 0x10;

    public const string CapturedWindowBytes = "3B 55 F0 75 06";

    public static ImmutableArray<CapacityFinalCalleeAlternateSecondFrameGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EDX with the fresh DWORD at entry EBP-10",
                CompareRawFileOffset, "3B 55 F0"),
            Instruction("Jump to 912D when the comparison is not equal",
                JnzRawFileOffset, "75 06",
                branchTargetRawFileOffset: UnequalDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus10Displacement);

    public static CapacityFinalCalleeAlternateSecondFrameGatePlan Project(
        CapacityFinalCalleeAlternateSecondFrameGateCapture capture)
    {
        bool edxCaptured = false;
        bool ebpCaptured = false;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateSecondFrameGateFlags? compareFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;
        bool unreachedEbp = false;
        bool lateFrameCapture = false;

        CapacityFinalCalleeAlternateSecondFrameGatePlan Finish(
            CapacityFinalCalleeAlternateSecondFrameGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = edxCaptured,
                EntryEBPCaptured = ebpCaptured,
                FrameMinus10ReadReached = frameReadReached,
                FrameMinus10Address = frameAddress,
                FrameMinus10Captured = frameCaptured,
                FrameMinus10 = frameValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                UnreachedEntryEBPCaptureProvided = unreachedEbp,
                LateFreshFrameMinus10CaptureProvided = lateFrameCapture,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            unreachedEbp = capture.EntryEBP.HasValue;
            lateFrameCapture = capture.FreshFrameMinus10.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEDX,
                CompareRawFileOffset);
        }

        edxCaptured = true;
        if (capture.EntryEBP is not uint entryEbp)
        {
            lateFrameCapture = capture.FreshFrameMinus10.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEBP,
                CompareRawFileOffset);
        }

        ebpCaptured = true;
        frameReadReached = true;
        frameAddress = DeriveEbpMinus10Address(entryEbp);
        if (capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return Finish(
                CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingFreshFrameMinus10,
                CompareRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshFrameMinus10;
        compareReached = true;
        compareLeft = entryEdx;
        compareRight = freshFrameMinus10;
        compareResult = unchecked(entryEdx - freshFrameMinus10);
        compareFlags = CalculateCompareFlags(
            entryEdx, freshFrameMinus10, compareResult.Value);
        jnzReached = true;
        jnzTaken = !compareFlags.Value.Zero;
        jnzDestination = jnzTaken.Value
            ? UnequalDestinationRawFileOffset
            : EqualDestinationRawFileOffset;

        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeAlternateSecondFrameGateOutcome.UnequalToBranchTarget
                : CapacityFinalCalleeAlternateSecondFrameGateOutcome.EqualToOpaqueCallBoundary,
            jnzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateSecondFrameGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeAlternateSecondFrameGateInstructionEvidence Instruction(
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
