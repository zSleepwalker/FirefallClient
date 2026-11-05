using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTailSizeGateCapture(
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonTailSizeGateOutcome
{
    MissingEntryEDI,
    BelowUnsignedThresholdToFallthrough,
    AtOrAboveUnsignedThresholdToBranchTarget
}

public readonly record struct CapacityFinalCalleeCommonTailSizeGateShiftFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonTailSizeGateCompareFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonTailSizeGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonTailSizeGatePlan(
    CapacityFinalCalleeCommonTailSizeGateCapture Capture,
    CapacityFinalCalleeCommonTailSizeGateOutcome Outcome)
{
    public bool EntryEDICaptured { get; init; }
    public bool MoveReached { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftInput { get; init; }
    public byte? ShiftCount { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeCommonTailSizeGateShiftFlags? ShiftFlags { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonTailSizeGateCompareFlags? CompareFlags { get; init; }
    public bool JnbReached { get; init; }
    public bool? JnbTaken { get; init; }
    public uint? JnbSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded SHR/CMP/JNB size gate at 92EC without entering either destination.
/// </summary>
public static class CapacityFinalCalleeCommonTailSizeGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92EC;
    public const uint EntryVirtualAddress = 0x016C9EEC;
    public const uint MoveRawFileOffset = 0x012C92EC;
    public const uint ShiftRawFileOffset = 0x012C92EE;
    public const uint CompareRawFileOffset = 0x012C92F1;
    public const uint JnbRawFileOffset = 0x012C92F4;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92F6;

    public const byte ShiftCount = 3;
    public const uint CompareThreshold = 0x20;
    public const uint BranchTargetRawFileOffset = 0x012C9354;
    public const uint FallthroughRawFileOffset = 0x012C92F6;

    public const string CapturedWindowBytes =
        "8B CF C1 E9 03 83 F9 20 73 5E";

    public static ImmutableArray<CapacityFinalCalleeCommonTailSizeGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Move the sole captured entry EDI value into ECX",
                MoveRawFileOffset,
                "8B CF"),
            Instruction(
                "Shift ECX right by three",
                ShiftRawFileOffset,
                "C1 E9 03"),
            Instruction(
                "Compare shifted ECX with unsigned threshold 20h",
                CompareRawFileOffset,
                "83 F9 20"),
            Instruction(
                "Branch to 9354 when unsigned ECX is at least 20h",
                JnbRawFileOffset,
                "73 5E",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ShiftRight(uint value, int count) =>
        value >> (count & 31);

    public static CapacityFinalCalleeCommonTailSizeGatePlan Project(
        CapacityFinalCalleeCommonTailSizeGateCapture capture)
    {
        CapacityFinalCalleeCommonTailSizeGatePlan Finish(
            CapacityFinalCalleeCommonTailSizeGateOutcome outcome,
            bool entryCaptured = false,
            bool moveReached = false,
            bool shiftReached = false,
            uint? shiftInput = null,
            byte? shiftCount = null,
            uint? shiftResult = null,
            CapacityFinalCalleeCommonTailSizeGateShiftFlags? shiftFlags = null,
            bool compareReached = false,
            uint? compareLeft = null,
            uint? compareResult = null,
            CapacityFinalCalleeCommonTailSizeGateCompareFlags? compareFlags = null,
            bool jnbReached = false,
            bool? jnbTaken = null,
            uint? jnbDestination = null,
            bool exactCleanAdmission = false,
            uint stopAt = MoveRawFileOffset) =>
            new(capture, outcome)
            {
                EntryEDICaptured = entryCaptured,
                MoveReached = moveReached,
                ShiftReached = shiftReached,
                ShiftInput = shiftInput,
                ShiftCount = shiftCount,
                ShiftResult = shiftResult,
                ShiftFlags = shiftFlags,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareReached ? CompareThreshold : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnbReached = jnbReached,
                JnbTaken = jnbTaken,
                JnbSelectedDestinationRawFileOffset = jnbDestination,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDI is not uint entryEdi)
        {
            return Finish(CapacityFinalCalleeCommonTailSizeGateOutcome.MissingEntryEDI);
        }

        uint shifted = ShiftRight(entryEdi, ShiftCount);
        CapacityFinalCalleeCommonTailSizeGateShiftFlags shiftFlags =
            CalculateShiftFlags(entryEdi, shifted);
        uint compareResult = unchecked(shifted - CompareThreshold);
        CapacityFinalCalleeCommonTailSizeGateCompareFlags compareFlags =
            CalculateCompareFlags(shifted, CompareThreshold, compareResult);
        bool jnbTaken = !compareFlags.Carry;
        uint destination = jnbTaken
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;

        return Finish(
            jnbTaken
                ? CapacityFinalCalleeCommonTailSizeGateOutcome
                    .AtOrAboveUnsignedThresholdToBranchTarget
                : CapacityFinalCalleeCommonTailSizeGateOutcome
                    .BelowUnsignedThresholdToFallthrough,
            entryCaptured: true,
            moveReached: true,
            shiftReached: true,
            shiftInput: entryEdi,
            shiftCount: ShiftCount,
            shiftResult: shifted,
            shiftFlags: shiftFlags,
            compareReached: true,
            compareLeft: shifted,
            compareResult: compareResult,
            compareFlags: compareFlags,
            jnbReached: true,
            jnbTaken: jnbTaken,
            jnbDestination: destination,
            exactCleanAdmission: true,
            stopAt: destination);
    }

    private static CapacityFinalCalleeCommonTailSizeGateShiftFlags CalculateShiftFlags(
        uint input,
        uint result) =>
        new(
            Carry: (input & (1u << (ShiftCount - 1))) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static CapacityFinalCalleeCommonTailSizeGateCompareFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonTailSizeGateInstructionEvidence Instruction(
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
