using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFrameMaskUpdateCapture(
    uint? ValidatedEntryEBP,
    uint? ValidatedEntryECX,
    uint? FreshFrameMinus14,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus8,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeFrameMaskUpdateOutcome
{
    MissingValidatedEntryEBP,
    MissingFreshFrameMinus14,
    MissingFreshFrameMinus4,
    FrameMismatch,
    MissingFreshFrameMinus8,
    MissingValidatedEntryECX,
    MissingFreshGlobalValue,
    GlobalValueUpdated
}

public readonly record struct CapacityFinalCalleeFrameMaskUpdateFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeFrameMaskUpdateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeFrameMaskUpdatePlan(
    CapacityFinalCalleeFrameMaskUpdateCapture Capture,
    CapacityFinalCalleeFrameMaskUpdateOutcome Outcome,
    uint? FreshFrameMinus14Address,
    bool FreshFrameMinus14ReadReached,
    bool FreshFrameMinus14Captured,
    uint? FreshFrameMinus14,
    uint? FreshFrameMinus4Address,
    bool FreshFrameMinus4ReadReached,
    bool FreshFrameMinus4Captured,
    uint? FreshFrameMinus4,
    bool CompareReached,
    uint? CompareResult,
    CapacityFinalCalleeFrameMaskUpdateFlags? CompareFlags,
    bool? Equal,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedBranchDestinationRawFileOffset,
    uint? FreshFrameMinus8Address,
    bool FreshFrameMinus8ReadReached,
    bool FreshFrameMinus8Captured,
    uint? FreshFrameMinus8,
    bool ShiftReached,
    uint? ShiftCount,
    uint? ShiftedValue,
    uint? NotMask,
    bool GlobalReadReached,
    bool GlobalValueCaptured,
    uint? GlobalValueBefore,
    bool GlobalWriteReached,
    uint? GlobalValueAfter,
    bool UnreachedEntryECXCaptureProvided,
    bool UnreachedFreshFrameMinus14CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the standalone fresh-frame CMP/JNZ/SHL/NOT/global-AND sequence.
/// Register inputs and every memory read are independent captures.
/// </summary>
public static class CapacityFinalCalleeFrameMaskUpdateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8EBE;
    public const uint EntryVirtualAddress = 0x016C9ABE;
    public const uint CompareRawFileOffset = 0x012C8EC1;
    public const uint BranchRawFileOffset = 0x012C8EC4;
    public const uint FrameMinus8LoadRawFileOffset = 0x012C8EC6;
    public const uint ShiftRawFileOffset = 0x012C8EC9;
    public const uint NotRawFileOffset = 0x012C8ECB;
    public const uint GlobalAndRawFileOffset = 0x012C8ECD;
    public const uint JumpRawFileOffset = 0x012C8ED3;
    public const uint WindowBoundaryRawFileOffsetExclusive = 0x012C8ED5;
    public const uint MismatchDestinationRawFileOffset = 0x012C8ED5;
    public const uint ContinuationRawFileOffset = 0x012C8EF8;

    public const uint FrameMinus14Displacement = 0x14;
    public const uint FrameMinus4Displacement = 4;
    public const uint FrameMinus8Displacement = 8;
    public const uint GlobalMaskVirtualAddress = 0x01DADAC8;
    public const string CapturedWindowBytes =
        "8B 45 EC 3B 45 FC 75 0F 8B 45 F8 D3 E0 F7 D0 21 05 C8 AD DA 01 EB 23";

    public static ImmutableArray<CapacityFinalCalleeFrameMaskUpdateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry EBP-14 into EAX",
                EntryRawFileOffset, "8B 45 EC"),
            Instruction("Compare that fresh EAX value with the fresh DWORD at EBP-4",
                CompareRawFileOffset, "3B 45 FC"),
            Instruction("JNZ to the next window when the compared DWORDs differ",
                BranchRawFileOffset, "75 0F",
                branchTargetRawFileOffset: MismatchDestinationRawFileOffset),
            Instruction("Load a fresh DWORD from EBP-8 after equality",
                FrameMinus8LoadRawFileOffset, "8B 45 F8"),
            Instruction("Shift EAX left by the low five bits of entry CL",
                ShiftRawFileOffset, "D3 E0"),
            Instruction("Invert all 32 EAX bits without changing flags",
                NotRawFileOffset, "F7 D0"),
            Instruction("AND the fresh global DWORD with EAX and write it back",
                GlobalAndRawFileOffset, "21 05 C8 AD DA 01",
                peVirtualAddressOperand: GlobalMaskVirtualAddress),
            Instruction("Jump unconditionally to the continuation",
                JumpRawFileOffset, "EB 23",
                branchTargetRawFileOffset: ContinuationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameAddress(uint entryEbp, uint displacement) =>
        unchecked(entryEbp - displacement);

    public static CapacityFinalCalleeFrameMaskUpdatePlan Project(
        CapacityFinalCalleeFrameMaskUpdateCapture capture)
    {
        if (capture.ValidatedEntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP,
                unreachedEntryECXCaptureProvided: capture.ValidatedEntryECX.HasValue,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameMinus14Address =
            DeriveFrameAddress(entryEbp, FrameMinus14Displacement);
        if (capture.FreshFrameMinus14 is not uint freshFrameMinus14)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus14,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                unreachedEntryECXCaptureProvided:
                    capture.ValidatedEntryECX.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameMinus4Address =
            DeriveFrameAddress(entryEbp, FrameMinus4Displacement);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus4,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                unreachedEntryECXCaptureProvided:
                    capture.ValidatedEntryECX.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: CompareRawFileOffset);
        }

        uint compareResult = unchecked(freshFrameMinus14 - freshFrameMinus4);
        CapacityFinalCalleeFrameMaskUpdateFlags compareFlags =
            CalculateFlags(freshFrameMinus14, freshFrameMinus4, compareResult);
        if (!compareFlags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                equal: false,
                branchReached: true,
                branchTaken: true,
                selectedBranchDestinationRawFileOffset:
                    MismatchDestinationRawFileOffset,
                unreachedEntryECXCaptureProvided:
                    capture.ValidatedEntryECX.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: WindowBoundaryRawFileOffsetExclusive);
        }

        uint frameMinus8Address =
            DeriveFrameAddress(entryEbp, FrameMinus8Displacement);
        if (capture.FreshFrameMinus8 is not uint freshFrameMinus8)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus8,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                equal: true,
                branchReached: true,
                branchTaken: false,
                freshFrameMinus8Address: frameMinus8Address,
                freshFrameMinus8ReadReached: true,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: FrameMinus8LoadRawFileOffset);
        }

        if (capture.ValidatedEntryECX is not uint entryEcx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryECX,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                equal: true,
                branchReached: true,
                branchTaken: false,
                freshFrameMinus8Address: frameMinus8Address,
                freshFrameMinus8ReadReached: true,
                freshFrameMinus8Captured: true,
                freshFrameMinus8: freshFrameMinus8,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: ShiftRawFileOffset);
        }

        uint shiftCount = entryEcx & 0x1F;
        uint shiftedValue = unchecked(freshFrameMinus8 << (int)shiftCount);
        uint notMask = ~shiftedValue;
        if (capture.FreshGlobalValue is not uint freshGlobalValue)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshGlobalValue,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: compareFlags,
                equal: true,
                branchReached: true,
                branchTaken: false,
                freshFrameMinus8Address: frameMinus8Address,
                freshFrameMinus8ReadReached: true,
                freshFrameMinus8Captured: true,
                freshFrameMinus8: freshFrameMinus8,
                shiftReached: true,
                shiftCount: shiftCount,
                shiftedValue: shiftedValue,
                notMask: notMask,
                globalReadReached: true,
                stopAt: GlobalAndRawFileOffset);
        }

        uint globalValueAfter = freshGlobalValue & notMask;
        return BuildPlan(
            capture,
            CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated,
            freshFrameMinus14Address: frameMinus14Address,
            freshFrameMinus14ReadReached: true,
            freshFrameMinus14Captured: true,
            freshFrameMinus14: freshFrameMinus14,
            freshFrameMinus4Address: frameMinus4Address,
            freshFrameMinus4ReadReached: true,
            freshFrameMinus4Captured: true,
            freshFrameMinus4: freshFrameMinus4,
            compareReached: true,
            compareResult: compareResult,
            compareFlags: compareFlags,
            equal: true,
            branchReached: true,
            branchTaken: false,
            freshFrameMinus8Address: frameMinus8Address,
            freshFrameMinus8ReadReached: true,
            freshFrameMinus8Captured: true,
            freshFrameMinus8: freshFrameMinus8,
            shiftReached: true,
            shiftCount: shiftCount,
            shiftedValue: shiftedValue,
            notMask: notMask,
            globalReadReached: true,
            globalValueCaptured: true,
            globalValueBefore: freshGlobalValue,
            globalWriteReached: true,
            globalValueAfter: globalValueAfter,
            stopAt: ContinuationRawFileOffset);
    }

    private static CapacityFinalCalleeFrameMaskUpdateFlags CalculateFlags(
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

    private static bool HasEvenParity(byte value)
    {
        value ^= (byte)(value >> 4);
        value &= 0x0F;
        return ((0x6996 >> value) & 1) == 0;
    }

    private static CapacityFinalCalleeFrameMaskUpdateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);

    private static CapacityFinalCalleeFrameMaskUpdatePlan BuildPlan(
        CapacityFinalCalleeFrameMaskUpdateCapture capture,
        CapacityFinalCalleeFrameMaskUpdateOutcome outcome,
        uint? freshFrameMinus14Address = null,
        bool freshFrameMinus14ReadReached = false,
        bool freshFrameMinus14Captured = false,
        uint? freshFrameMinus14 = null,
        uint? freshFrameMinus4Address = null,
        bool freshFrameMinus4ReadReached = false,
        bool freshFrameMinus4Captured = false,
        uint? freshFrameMinus4 = null,
        bool compareReached = false,
        uint? compareResult = null,
        CapacityFinalCalleeFrameMaskUpdateFlags? compareFlags = null,
        bool? equal = null,
        bool branchReached = false,
        bool? branchTaken = null,
        uint? selectedBranchDestinationRawFileOffset = null,
        uint? freshFrameMinus8Address = null,
        bool freshFrameMinus8ReadReached = false,
        bool freshFrameMinus8Captured = false,
        uint? freshFrameMinus8 = null,
        bool shiftReached = false,
        uint? shiftCount = null,
        uint? shiftedValue = null,
        uint? notMask = null,
        bool globalReadReached = false,
        bool globalValueCaptured = false,
        uint? globalValueBefore = null,
        bool globalWriteReached = false,
        uint? globalValueAfter = null,
        bool unreachedEntryECXCaptureProvided = false,
        bool unreachedFreshFrameMinus14CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshFrameMinus8CaptureProvided = false,
        bool unreachedFreshGlobalValueCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            freshFrameMinus14ReadReached ? freshFrameMinus14Address : null,
            freshFrameMinus14ReadReached,
            freshFrameMinus14Captured,
            freshFrameMinus14Captured ? freshFrameMinus14 : null,
            freshFrameMinus4ReadReached ? freshFrameMinus4Address : null,
            freshFrameMinus4ReadReached,
            freshFrameMinus4Captured,
            freshFrameMinus4Captured ? freshFrameMinus4 : null,
            compareReached,
            compareReached ? compareResult : null,
            compareReached ? compareFlags : null,
            compareReached ? equal : null,
            branchReached,
            branchReached ? branchTaken : null,
            branchReached && branchTaken == true
                ? selectedBranchDestinationRawFileOffset
                : null,
            freshFrameMinus8ReadReached ? freshFrameMinus8Address : null,
            freshFrameMinus8ReadReached,
            freshFrameMinus8Captured,
            freshFrameMinus8Captured ? freshFrameMinus8 : null,
            shiftReached,
            shiftReached ? shiftCount : null,
            shiftReached ? shiftedValue : null,
            shiftReached ? notMask : null,
            globalReadReached,
            globalValueCaptured,
            globalValueCaptured ? globalValueBefore : null,
            globalWriteReached,
            globalWriteReached ? globalValueAfter : null,
            unreachedEntryECXCaptureProvided
                || !shiftReached && capture.ValidatedEntryECX.HasValue,
            unreachedFreshFrameMinus14CaptureProvided
                || !freshFrameMinus14ReadReached
                    && capture.FreshFrameMinus14.HasValue,
            unreachedFreshFrameMinus4CaptureProvided
                || !freshFrameMinus4ReadReached
                    && capture.FreshFrameMinus4.HasValue,
            unreachedFreshFrameMinus8CaptureProvided
                || !freshFrameMinus8ReadReached
                    && capture.FreshFrameMinus8.HasValue,
            unreachedFreshGlobalValueCaptureProvided
                || !globalReadReached && capture.FreshGlobalValue.HasValue,
            stopAt);
}
