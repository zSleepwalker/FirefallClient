using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateShiftedFrameGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus4,
    uint? EntryECX);

public enum CapacityFinalCalleeAlternateShiftedFrameGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus4,
    MissingEntryECX,
    EqualToBranchTarget,
    UnequalToOpaqueCallBoundary
}

public readonly record struct CapacityFinalCalleeAlternateShiftedFrameGateFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeAlternateShiftedFrameGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateShiftedFrameGatePlan(
    CapacityFinalCalleeAlternateShiftedFrameGateCapture Capture,
    CapacityFinalCalleeAlternateShiftedFrameGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus4ReadReached { get; init; }
    public uint? FrameMinus4Address { get; init; }
    public bool FrameMinus4Captured { get; init; }
    public uint? FrameMinus4 { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftInput { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeAlternateShiftedFrameGateFlags? ShiftFlags { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateShiftedFrameGateFlags? CompareFlags { get; init; }
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedFreshFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedEntryECXCaptureProvided { get; init; }
    public bool LateFreshFrameMinus4CaptureProvided { get; init; }
    public bool LateEntryECXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public bool OpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh EBP-4 load, SHL EAX,3, CMP ECX,EAX, and JZ only.
/// The frame DWORD is independent of prior stores, and the opaque call at
/// 9137 is not entered.
/// </summary>
public static class CapacityFinalCalleeAlternateShiftedFrameGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C912D;
    public const uint EntryVirtualAddress = 0x016C9D2D;
    public const uint FrameReadRawFileOffset = 0x012C912D;
    public const uint ShiftRawFileOffset = 0x012C9130;
    public const uint CompareRawFileOffset = 0x012C9133;
    public const uint JzRawFileOffset = 0x012C9135;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9137;
    public const uint OpaqueCallRawFileOffset = 0x012C9137;
    public const uint OpaqueCallCellVirtualAddress = 0x01AC4A24;
    public const uint EqualDestinationRawFileOffset = 0x012C913D;
    public const uint UnequalDestinationRawFileOffset = 0x012C9137;

    public const uint EbpMinus4Displacement = 4;
    public const int ShiftCount = 3;

    public const string CapturedWindowBytes =
        "8B 45 FC C1 E0 03 3B C8 74 06";

    public static ImmutableArray<CapacityFinalCalleeAlternateShiftedFrameGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-4 into EAX",
                FrameReadRawFileOffset, "8B 45 FC"),
            Instruction("Shift the fresh EAX value left by three",
                ShiftRawFileOffset, "C1 E0 03"),
            Instruction("Compare entry ECX with the shifted EAX value",
                CompareRawFileOffset, "3B C8"),
            Instruction("Jump to 913D when ECX equals shifted EAX",
                JzRawFileOffset, "74 06",
                branchTargetRawFileOffset: EqualDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static uint ShiftLeftByThree(uint value) =>
        unchecked(value << ShiftCount);

    public static CapacityFinalCalleeAlternateShiftedFrameGatePlan Project(
        CapacityFinalCalleeAlternateShiftedFrameGateCapture capture)
    {
        bool ebpCaptured = false;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool shiftReached = false;
        uint? shiftInput = null;
        uint? shiftResult = null;
        CapacityFinalCalleeAlternateShiftedFrameGateFlags? shiftFlags = null;
        bool ecxCaptured = false;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateShiftedFrameGateFlags? compareFlags = null;
        bool jzReached = false;
        bool? jzTaken = null;
        uint? jzDestination = null;
        bool unreachedFreshFrame = false;
        bool unreachedECX = false;
        bool lateFreshFrame = false;
        bool lateECX = false;

        CapacityFinalCalleeAlternateShiftedFrameGatePlan Finish(
            CapacityFinalCalleeAlternateShiftedFrameGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinus4ReadReached = frameReadReached,
                FrameMinus4Address = frameAddress,
                FrameMinus4Captured = frameCaptured,
                FrameMinus4 = frameValue,
                ShiftReached = shiftReached,
                ShiftInput = shiftInput,
                ShiftResult = shiftResult,
                ShiftFlags = shiftFlags,
                EntryECXCaptured = ecxCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JzReached = jzReached,
                JzTaken = jzTaken,
                JzSelectedDestinationRawFileOffset = jzDestination,
                UnreachedFreshFrameMinus4CaptureProvided = unreachedFreshFrame,
                UnreachedEntryECXCaptureProvided = unreachedECX,
                LateFreshFrameMinus4CaptureProvided = lateFreshFrame,
                LateEntryECXCaptureProvided = lateECX,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedFreshFrame = capture.FreshFrameMinus4.HasValue;
            unreachedECX = capture.EntryECX.HasValue;
            lateFreshFrame = unreachedFreshFrame;
            lateECX = unreachedECX;
            return Finish(
                CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryEBP,
                FrameReadRawFileOffset);
        }

        ebpCaptured = true;
        frameReadReached = true;
        frameAddress = DeriveEbpMinus4Address(entryEbp);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            lateECX = capture.EntryECX.HasValue;
            unreachedECX = lateECX;
            return Finish(
                CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingFreshFrameMinus4,
                FrameReadRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshFrameMinus4;
        shiftReached = true;
        shiftInput = freshFrameMinus4;
        shiftResult = ShiftLeftByThree(freshFrameMinus4);
        shiftFlags = CalculateShiftFlags(freshFrameMinus4, shiftResult.Value);

        if (capture.EntryECX is not uint entryEcx)
        {
            return Finish(
                CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryECX,
                CompareRawFileOffset);
        }

        ecxCaptured = true;
        compareReached = true;
        compareLeft = entryEcx;
        compareRight = shiftResult;
        compareResult = unchecked(entryEcx - shiftResult.Value);
        compareFlags = CalculateCompareFlags(
            entryEcx, shiftResult.Value, compareResult.Value);
        jzReached = true;
        jzTaken = compareFlags.Value.Zero;
        jzDestination = jzTaken.Value
            ? EqualDestinationRawFileOffset
            : UnequalDestinationRawFileOffset;

        return Finish(
            jzTaken.Value
                ? CapacityFinalCalleeAlternateShiftedFrameGateOutcome.EqualToBranchTarget
                : CapacityFinalCalleeAlternateShiftedFrameGateOutcome.UnequalToOpaqueCallBoundary,
            jzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateShiftedFrameGateFlags CalculateShiftFlags(
        uint value,
        uint result) =>
        new(
            Carry: (value & (1u << (32 - ShiftCount))) != 0,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static CapacityFinalCalleeAlternateShiftedFrameGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateShiftedFrameGateInstructionEvidence Instruction(
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
