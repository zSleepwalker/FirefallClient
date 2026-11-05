using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSizeComparisonCallCapture(
    bool CaptureValidatedEntry,
    uint EntryESI,
    uint EntryEAX,
    uint EntryESP,
    uint? FreshEsiPlus4,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeSizeComparisonCallOutcome
{
    EntryNotValidated,
    MissingFreshEsiPlus4,
    EqualSizesCallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public readonly record struct CapacityFinalCalleeSizeComparisonFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeSizeComparisonCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeSizeComparisonCallPlan(
    CapacityFinalCalleeSizeComparisonCallCapture Capture,
    CapacityFinalCalleeSizeComparisonCallOutcome Outcome,
    uint? FreshEsiPlus4Address,
    bool FreshEsiPlus4ReadReached,
    bool FreshEsiPlus4Captured,
    uint? FreshEsiPlus4,
    bool MaskReached,
    uint? MaskedFreshEsiPlus4,
    bool ShiftReached,
    uint? ShiftedEntryEax,
    bool CompareReached,
    uint? CompareResult,
    CapacityFinalCalleeSizeComparisonFlags? CompareFlags,
    bool? Equal,
    bool? UnsignedBelow,
    bool? SignedLess,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    uint? PreCallESI,
    uint? PreCallEAX,
    uint? PreCallESP,
    bool CallOutputsOpaque,
    bool UnreachedFreshEsiPlus4CaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the standalone fresh-size CMP/JZ/indirect-CALL window. The shifted
/// size is derived from entry EAX, independently of any earlier opaque calls.
/// </summary>
public static class CapacityFinalCalleeSizeComparisonCallProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8E87;
    public const uint EntryVirtualAddress = 0x016C9A87;
    public const uint MaskRawFileOffset = 0x012C8E8A;
    public const uint ShiftRawFileOffset = 0x012C8E8D;
    public const uint CompareRawFileOffset = 0x012C8E90;
    public const uint BranchRawFileOffset = 0x012C8E92;
    public const uint CallRawFileOffset = 0x012C8E94;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8E9A;
    public const uint BranchDestinationRawFileOffset = 0x012C8E9A;
    public const uint BranchDestinationVirtualAddress = 0x016C9A9A;

    public const uint CallCellVirtualAddress = 0x01AC4A24;
    public const uint FreshEsiPlus4Displacement = 4;
    public const uint SizeAlignmentMask = 0xFFFFFFF8;
    public const int EntryEaxShiftCount = 3;

    public const string CapturedWindowBytes =
        "8B 4E 04 83 E1 F8 C1 E0 03 3B C8 74 06 FF 15 24 4A AC 01";

    public static ImmutableArray<CapacityFinalCalleeSizeComparisonCallInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry ESI+4 into ECX",
                EntryRawFileOffset, "8B 4E 04"),
            Instruction("Clear the low three bits of the fresh DWORD in ECX",
                MaskRawFileOffset, "83 E1 F8"),
            Instruction("Shift independent entry EAX left by three, modulo DWORD",
                ShiftRawFileOffset, "C1 E0 03"),
            Instruction("Compare masked ECX with shifted entry EAX",
                CompareRawFileOffset, "3B C8"),
            Instruction("JZ to the exclusive boundary when the sizes are equal",
                BranchRawFileOffset, "74 06",
                branchTargetRawFileOffset: BranchDestinationRawFileOffset,
                branchTargetVirtualAddress: BranchDestinationVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC4A24 when the sizes differ",
                CallRawFileOffset, "FF 15 24 4A AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEsiPlus4Address(uint entryEsi) =>
        unchecked(entryEsi + FreshEsiPlus4Displacement);

    public static uint MaskFreshSize(uint freshEsiPlus4) =>
        freshEsiPlus4 & SizeAlignmentMask;

    public static uint ShiftEntryEax(uint entryEax) =>
        unchecked(entryEax << EntryEaxShiftCount);

    public static CapacityFinalCalleeSizeComparisonCallPlan Project(
        CapacityFinalCalleeSizeComparisonCallCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeSizeComparisonCallOutcome.EntryNotValidated,
                unreachedFreshEsiPlus4CaptureProvided: capture.FreshEsiPlus4.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint freshEsiPlus4Address = DeriveFreshEsiPlus4Address(capture.EntryESI);
        if (capture.FreshEsiPlus4 is not uint freshEsiPlus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeSizeComparisonCallOutcome.MissingFreshEsiPlus4,
                freshEsiPlus4Address: freshEsiPlus4Address,
                freshEsiPlus4ReadReached: true,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint maskedFreshEsiPlus4 = MaskFreshSize(freshEsiPlus4);
        uint shiftedEntryEax = ShiftEntryEax(capture.EntryEAX);
        uint compareResult = unchecked(maskedFreshEsiPlus4 - shiftedEntryEax);
        CapacityFinalCalleeSizeComparisonFlags flags =
            CalculateFlags(maskedFreshEsiPlus4, shiftedEntryEax, compareResult);
        if (flags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped,
                freshEsiPlus4Address: freshEsiPlus4Address,
                freshEsiPlus4ReadReached: true,
                freshEsiPlus4Captured: true,
                freshEsiPlus4: freshEsiPlus4,
                maskReached: true,
                maskedFreshEsiPlus4: maskedFreshEsiPlus4,
                shiftReached: true,
                shiftedEntryEax: shiftedEntryEax,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: flags,
                equal: true,
                unsignedBelow: false,
                signedLess: false,
                branchReached: true,
                branchTaken: true,
                selectedDestinationRawFileOffset: BranchDestinationRawFileOffset,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: BoundaryRawFileOffsetExclusive);
        }

        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallCellTarget,
                freshEsiPlus4Address: freshEsiPlus4Address,
                freshEsiPlus4ReadReached: true,
                freshEsiPlus4Captured: true,
                freshEsiPlus4: freshEsiPlus4,
                maskReached: true,
                maskedFreshEsiPlus4: maskedFreshEsiPlus4,
                shiftReached: true,
                shiftedEntryEax: shiftedEntryEax,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: flags,
                equal: false,
                unsignedBelow: flags.Carry,
                signedLess: flags.Sign != flags.Overflow,
                branchReached: true,
                branchTaken: false,
                callCellReadReached: true,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallReturnStatus,
                freshEsiPlus4Address: freshEsiPlus4Address,
                freshEsiPlus4ReadReached: true,
                freshEsiPlus4Captured: true,
                freshEsiPlus4: freshEsiPlus4,
                maskReached: true,
                maskedFreshEsiPlus4: maskedFreshEsiPlus4,
                shiftReached: true,
                shiftedEntryEax: shiftedEntryEax,
                compareReached: true,
                compareResult: compareResult,
                compareFlags: flags,
                equal: false,
                unsignedBelow: flags.Carry,
                signedLess: flags.Sign != flags.Overflow,
                branchReached: true,
                branchTaken: false,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                callReached: true,
                callOutputsOpaque: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally
                : CapacityFinalCalleeSizeComparisonCallOutcome.CallDidNotReturnNormally,
            freshEsiPlus4Address: freshEsiPlus4Address,
            freshEsiPlus4ReadReached: true,
            freshEsiPlus4Captured: true,
            freshEsiPlus4: freshEsiPlus4,
            maskReached: true,
            maskedFreshEsiPlus4: maskedFreshEsiPlus4,
            shiftReached: true,
            shiftedEntryEax: shiftedEntryEax,
            compareReached: true,
            compareResult: compareResult,
            compareFlags: flags,
            equal: false,
            unsignedBelow: flags.Carry,
            signedLess: flags.Sign != flags.Overflow,
            branchReached: true,
            branchTaken: false,
            callCellReadReached: true,
            callCellTargetCaptured: true,
            callCellTargetVirtualAddress: callCellTarget,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            callOutputsOpaque: true,
            stopAt: callReturnedNormally
                ? BoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static CapacityFinalCalleeSizeComparisonFlags CalculateFlags(
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

    private static CapacityFinalCalleeSizeComparisonCallInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeSizeComparisonCallPlan BuildPlan(
        CapacityFinalCalleeSizeComparisonCallCapture capture,
        CapacityFinalCalleeSizeComparisonCallOutcome outcome,
        uint? freshEsiPlus4Address = null,
        bool freshEsiPlus4ReadReached = false,
        bool freshEsiPlus4Captured = false,
        uint? freshEsiPlus4 = null,
        bool maskReached = false,
        uint? maskedFreshEsiPlus4 = null,
        bool shiftReached = false,
        uint? shiftedEntryEax = null,
        bool compareReached = false,
        uint? compareResult = null,
        CapacityFinalCalleeSizeComparisonFlags? compareFlags = null,
        bool? equal = null,
        bool? unsignedBelow = null,
        bool? signedLess = null,
        bool branchReached = false,
        bool? branchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool callOutputsOpaque = false,
        bool unreachedFreshEsiPlus4CaptureProvided = false,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            freshEsiPlus4ReadReached ? freshEsiPlus4Address : null,
            freshEsiPlus4ReadReached,
            freshEsiPlus4Captured,
            freshEsiPlus4Captured ? freshEsiPlus4 : null,
            maskReached,
            maskReached ? maskedFreshEsiPlus4 : null,
            shiftReached,
            shiftReached ? shiftedEntryEax : null,
            compareReached,
            compareReached ? compareResult : null,
            compareReached ? compareFlags : null,
            compareReached ? equal : null,
            compareReached ? unsignedBelow : null,
            compareReached ? signedLess : null,
            branchReached,
            branchReached ? branchTaken : null,
            branchReached && branchTaken == true
                ? selectedDestinationRawFileOffset
                : null,
            callCellReadReached,
            callCellTargetCaptured,
            callCellTargetCaptured ? callCellTargetVirtualAddress : null,
            callReached,
            callReached ? callReturnedNormally : null,
            callReached ? capture.EntryESI : null,
            callReached ? capture.EntryEAX : null,
            callReached ? capture.EntryESP : null,
            callOutputsOpaque,
            (!freshEsiPlus4ReadReached && capture.FreshEsiPlus4.HasValue)
                || unreachedFreshEsiPlus4CaptureProvided,
            (!callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue)
                || unreachedCallCellTargetCaptureProvided,
            (!callReached && capture.CallReturnedNormally.HasValue)
                || unreachedCallReturnStatusProvided,
            stopAt);
}
