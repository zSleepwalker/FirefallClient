using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateMaskTailFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeAlternateMaskTailCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus8,
    uint? FreshFrameMinus4,
    uint? FreshGlobalValue,
    CapacityFinalCalleeAlternateMaskTailFlags? EntryFlags = null);

public enum CapacityFinalCalleeAlternateMaskTailOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus8,
    MissingFreshFrameMinus4,
    MissingFreshGlobalValue,
    ReachedSuccessor
}

public readonly record struct CapacityFinalCalleeAlternateMaskTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateMaskTailPlan(
    CapacityFinalCalleeAlternateMaskTailCapture Capture,
    CapacityFinalCalleeAlternateMaskTailOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public uint? FrameMinus8Address { get; init; }
    public bool FrameMinus8ReadReached { get; init; }
    public bool FrameMinus8Captured { get; init; }
    public uint? FrameMinus8 { get; init; }
    public uint? FrameMinus4Address { get; init; }
    public bool FrameMinus4ReadReached { get; init; }
    public bool FrameMinus4Captured { get; init; }
    public uint? FrameMinus4 { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftInput { get; init; }
    public byte? ShiftCountByte { get; init; }
    public int? EffectiveShiftCount { get; init; }
    public uint? ShiftResult { get; init; }
    public CapacityFinalCalleeAlternateMaskTailFlags? ShiftFlags { get; init; }
    public bool EntryFlagsConsumed { get; init; }
    public bool UnusedEntryFlagsCaptureProvided { get; init; }
    public bool NotReached { get; init; }
    public uint? NotInput { get; init; }
    public uint? NotResult { get; init; }
    public CapacityFinalCalleeAlternateMaskTailFlags? FlagsAfterNot { get; init; }
    public bool GlobalReadReached { get; init; }
    public uint? GlobalAddress { get; init; }
    public bool GlobalCaptured { get; init; }
    public uint? GlobalBefore { get; init; }
    public bool GlobalAndReached { get; init; }
    public bool GlobalWriteReached { get; init; }
    public uint? GlobalWriteAddress { get; init; }
    public uint? GlobalWriteValue { get; init; }
    public CapacityFinalCalleeAlternateMaskTailFlags? FinalFlags { get; init; }
    public bool FinalJumpReached { get; init; }
    public bool? FinalJumpTaken { get; init; }
    public uint? FinalJumpSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryFlagsCaptureProvided { get; init; }
    public bool UnreachedFreshFrameMinus8CaptureProvided { get; init; }
    public bool UnreachedFreshFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedFreshGlobalCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the alternate mask tail through its unconditional jump to 92BD.
/// The two frame values and the global value are independent fresh captures.
/// </summary>
public static class CapacityFinalCalleeAlternateMaskTailProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9167;
    public const uint EntryVirtualAddress = 0x016C9D67;
    public const uint FrameMinus8ReadRawFileOffset = 0x012C9167;
    public const uint FrameMinus4ReadRawFileOffset = 0x012C916A;
    public const uint ShiftRawFileOffset = 0x012C916D;
    public const uint NotRawFileOffset = 0x012C916F;
    public const uint GlobalAndRawFileOffset = 0x012C9171;
    public const uint FinalJumpRawFileOffset = 0x012C9177;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C917C;
    public const uint SuccessorRawFileOffset = 0x012C92BD;
    public const uint GlobalVirtualAddress = 0x01EDADC8;
    public const uint EbpMinus8Displacement = 8;
    public const uint EbpMinus4Displacement = 4;

    public const string CapturedWindowBytes =
        "8B 45 F8 8B 4D FC D3 E0 F7 D0 21 05 C8 AD ED 01 E9 41 01 00 00";

    public static ImmutableArray<CapacityFinalCalleeAlternateMaskTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD at entry EBP-8 into EAX",
                FrameMinus8ReadRawFileOffset, "8B 45 F8"),
            Instruction("Load the independent fresh DWORD at entry EBP-4 into ECX",
                FrameMinus4ReadRawFileOffset, "8B 4D FC"),
            Instruction("Shift EAX left by CL masked to five bits",
                ShiftRawFileOffset, "D3 E0"),
            Instruction("Complement the shifted EAX without changing flags",
                NotRawFileOffset, "F7 D0"),
            Instruction("AND the complemented EAX into the fresh absolute global DWORD",
                GlobalAndRawFileOffset, "21 05 C8 AD ED 01"),
            Instruction("Jump unconditionally to raw 92BD",
                FinalJumpRawFileOffset, "E9 41 01 00 00",
                branchTargetRawFileOffset: SuccessorRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus8Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus8Displacement);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static uint ShiftLeft(uint value, int count) =>
        unchecked(value << (count & 31));

    public static uint Complement(uint value) => ~value;

    public static CapacityFinalCalleeAlternateMaskTailPlan Project(
        CapacityFinalCalleeAlternateMaskTailCapture capture)
    {
        bool ebpCaptured = false;
        uint? frameMinus8Address = null;
        bool frameMinus8ReadReached = false;
        bool frameMinus8Captured = false;
        uint? frameMinus8 = null;
        uint? frameMinus4Address = null;
        bool frameMinus4ReadReached = false;
        bool frameMinus4Captured = false;
        uint? frameMinus4 = null;
        bool shiftReached = false;
        uint? shiftInput = null;
        byte? shiftCountByte = null;
        int? effectiveShiftCount = null;
        uint? shiftResult = null;
        CapacityFinalCalleeAlternateMaskTailFlags? shiftFlags = null;
        bool entryFlagsConsumed = false;
        bool unusedEntryFlags = false;
        bool notReached = false;
        uint? notInput = null;
        uint? notResult = null;
        CapacityFinalCalleeAlternateMaskTailFlags? flagsAfterNot = null;
        bool globalReadReached = false;
        bool globalCaptured = false;
        uint? globalBefore = null;
        bool globalAndReached = false;
        bool globalWriteReached = false;
        uint? globalWriteValue = null;
        CapacityFinalCalleeAlternateMaskTailFlags? finalFlags = null;
        bool finalJumpReached = false;
        bool? finalJumpTaken = null;
        uint? finalJumpDestination = null;
        bool unreachedEntryFlags = false;
        bool unreachedFrameMinus8 = false;
        bool unreachedFrameMinus4 = false;
        bool unreachedGlobal = false;

        CapacityFinalCalleeAlternateMaskTailPlan Finish(
            CapacityFinalCalleeAlternateMaskTailOutcome outcome,
            uint stopAt) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinus8Address = frameMinus8Address,
                FrameMinus8ReadReached = frameMinus8ReadReached,
                FrameMinus8Captured = frameMinus8Captured,
                FrameMinus8 = frameMinus8,
                FrameMinus4Address = frameMinus4Address,
                FrameMinus4ReadReached = frameMinus4ReadReached,
                FrameMinus4Captured = frameMinus4Captured,
                FrameMinus4 = frameMinus4,
                ShiftReached = shiftReached,
                ShiftInput = shiftInput,
                ShiftCountByte = shiftCountByte,
                EffectiveShiftCount = effectiveShiftCount,
                ShiftResult = shiftResult,
                ShiftFlags = shiftFlags,
                EntryFlagsConsumed = entryFlagsConsumed,
                UnusedEntryFlagsCaptureProvided = unusedEntryFlags,
                NotReached = notReached,
                NotInput = notInput,
                NotResult = notResult,
                FlagsAfterNot = flagsAfterNot,
                GlobalReadReached = globalReadReached,
                GlobalAddress = globalReadReached ? GlobalVirtualAddress : null,
                GlobalCaptured = globalCaptured,
                GlobalBefore = globalBefore,
                GlobalAndReached = globalAndReached,
                GlobalWriteReached = globalWriteReached,
                GlobalWriteAddress = globalWriteReached ? GlobalVirtualAddress : null,
                GlobalWriteValue = globalWriteValue,
                FinalFlags = finalFlags,
                FinalJumpReached = finalJumpReached,
                FinalJumpTaken = finalJumpTaken,
                FinalJumpSelectedDestinationRawFileOffset = finalJumpDestination,
                UnreachedEntryFlagsCaptureProvided = unreachedEntryFlags,
                UnreachedFreshFrameMinus8CaptureProvided = unreachedFrameMinus8,
                UnreachedFreshFrameMinus4CaptureProvided = unreachedFrameMinus4,
                UnreachedFreshGlobalCaptureProvided = unreachedGlobal,
                ExactCleanExitAdmitted = finalJumpReached && !unusedEntryFlags
                    && !unreachedEntryFlags && !unreachedFrameMinus8
                    && !unreachedFrameMinus4 && !unreachedGlobal,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedFrameMinus8 = capture.FreshFrameMinus8.HasValue;
            unreachedFrameMinus4 = capture.FreshFrameMinus4.HasValue;
            unreachedGlobal = capture.FreshGlobalValue.HasValue;
            unreachedEntryFlags = capture.EntryFlags.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateMaskTailOutcome.MissingEntryEBP,
                FrameMinus8ReadRawFileOffset);
        }

        ebpCaptured = true;
        frameMinus8Address = DeriveEbpMinus8Address(entryEbp);
        frameMinus8ReadReached = true;
        if (capture.FreshFrameMinus8 is not uint freshFrameMinus8)
        {
            unreachedFrameMinus4 = capture.FreshFrameMinus4.HasValue;
            unreachedGlobal = capture.FreshGlobalValue.HasValue;
            unreachedEntryFlags = capture.EntryFlags.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus8,
                FrameMinus8ReadRawFileOffset);
        }

        frameMinus8Captured = true;
        frameMinus8 = freshFrameMinus8;
        frameMinus4Address = DeriveEbpMinus4Address(entryEbp);
        frameMinus4ReadReached = true;
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            unreachedGlobal = capture.FreshGlobalValue.HasValue;
            unreachedEntryFlags = capture.EntryFlags.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshFrameMinus4,
                FrameMinus4ReadRawFileOffset);
        }

        frameMinus4Captured = true;
        frameMinus4 = freshFrameMinus4;
        shiftReached = true;
        shiftInput = freshFrameMinus8;
        shiftCountByte = (byte)freshFrameMinus4;
        int count = shiftCountByte.Value & 31;
        effectiveShiftCount = count;
        shiftResult = ShiftLeft(freshFrameMinus8, count);
        if (count == 0)
        {
            entryFlagsConsumed = capture.EntryFlags.HasValue;
            shiftFlags = capture.EntryFlags ?? UnknownFlags;
        }
        else
        {
            unusedEntryFlags = capture.EntryFlags.HasValue;
            shiftFlags = CalculateShiftFlags(freshFrameMinus8, count, shiftResult.Value);
        }

        notReached = true;
        notInput = shiftResult;
        notResult = Complement(shiftResult.Value);
        flagsAfterNot = shiftFlags;
        globalReadReached = true;
        if (capture.FreshGlobalValue is not uint freshGlobal)
        {
            return Finish(
                CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshGlobalValue,
                GlobalAndRawFileOffset);
        }

        globalCaptured = true;
        globalBefore = freshGlobal;
        globalAndReached = true;
        globalWriteReached = true;
        globalWriteValue = freshGlobal & notResult.Value;
        finalFlags = CalculateAndFlags(globalWriteValue.Value);
        finalJumpReached = true;
        finalJumpTaken = true;
        finalJumpDestination = SuccessorRawFileOffset;

        return Finish(
            CapacityFinalCalleeAlternateMaskTailOutcome.ReachedSuccessor,
            SuccessorRawFileOffset);
    }

    private static readonly CapacityFinalCalleeAlternateMaskTailFlags UnknownFlags =
        new(null, null, null, null, null, null);

    private static CapacityFinalCalleeAlternateMaskTailFlags CalculateShiftFlags(
        uint value,
        int count,
        uint result)
    {
        bool carry = (value & (1u << (32 - count))) != 0;
        return new(
            Carry: carry,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: count == 1
                ? ((result & 0x80000000) != 0) ^ carry
                : null);
    }

    private static CapacityFinalCalleeAlternateMaskTailFlags CalculateAndFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateMaskTailInstructionEvidence Instruction(
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
