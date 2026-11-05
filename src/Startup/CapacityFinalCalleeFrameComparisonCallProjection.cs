using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFrameComparisonCallCapture(
    bool CaptureValidatedEntry,
    uint EntryESI,
    uint EntryEBP,
    uint EntryESP,
    uint? FreshFrameMinus4,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeFrameComparisonCallOutcome
{
    EntryNotValidated,
    MissingFreshFrameMinus4,
    FrameMismatchCallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public readonly record struct CapacityFinalCalleeFrameComparisonFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeFrameComparisonCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeFrameComparisonCallPlan(
    CapacityFinalCalleeFrameComparisonCallCapture Capture,
    CapacityFinalCalleeFrameComparisonCallOutcome Outcome,
    uint? FreshFrameMinus4Address,
    bool FreshFrameMinus4ReadReached,
    bool FreshFrameMinus4Captured,
    uint? FreshFrameMinus4,
    uint? CompareResult,
    CapacityFinalCalleeFrameComparisonFlags? CompareFlags,
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
    uint? PreCallEBP,
    uint? PreCallESP,
    bool CallOutputsOpaque,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the standalone CMP/JNZ/indirect-CALL window. The frame DWORD is freshly
/// captured at entry EBP-4; earlier frame values and opaque calls are not inputs.
/// </summary>
public static class CapacityFinalCalleeFrameComparisonCallProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8E7C;
    public const uint EntryVirtualAddress = 0x016C9A7C;
    public const uint BranchRawFileOffset = 0x012C8E7F;
    public const uint CallRawFileOffset = 0x012C8E81;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8E87;
    public const uint BranchDestinationRawFileOffset = 0x012C8E87;
    public const uint BranchDestinationVirtualAddress = 0x016C9A87;

    public const uint CallCellVirtualAddress = 0x01AC4A24;
    public const uint FrameMinus4Displacement = 4;

    public const string CapturedWindowBytes =
        "3B 75 FC 75 06 FF 15 24 4A AC 01";

    public static ImmutableArray<CapacityFinalCalleeFrameComparisonCallInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ESI with the freshly loaded DWORD at entry EBP-4",
                EntryRawFileOffset, "3B 75 FC"),
            Instruction("JNZ to the exclusive boundary when the DWORDs differ",
                BranchRawFileOffset, "75 06",
                branchTargetRawFileOffset: BranchDestinationRawFileOffset,
                branchTargetVirtualAddress: BranchDestinationVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC4A24 when the DWORDs are equal",
                CallRawFileOffset, "FF 15 24 4A AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshFrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Displacement);

    public static CapacityFinalCalleeFrameComparisonCallPlan Project(
        CapacityFinalCalleeFrameComparisonCallCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameComparisonCallOutcome.EntryNotValidated,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameAddress = DeriveFreshFrameMinus4Address(capture.EntryEBP);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameComparisonCallOutcome.MissingFreshFrameMinus4,
                freshFrameMinus4Address: frameAddress,
                freshFrameMinus4ReadReached: true,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint compareResult = unchecked(capture.EntryESI - freshFrameMinus4);
        CapacityFinalCalleeFrameComparisonFlags flags =
            CalculateFlags(capture.EntryESI, freshFrameMinus4, compareResult);
        if (!flags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped,
                freshFrameMinus4Address: frameAddress,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareResult: compareResult,
                compareFlags: flags,
                equal: false,
                unsignedBelow: flags.Carry,
                signedLess: flags.Sign != flags.Overflow,
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
                CapacityFinalCalleeFrameComparisonCallOutcome.MissingCallCellTarget,
                freshFrameMinus4Address: frameAddress,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareResult: compareResult,
                compareFlags: flags,
                equal: true,
                unsignedBelow: false,
                signedLess: false,
                branchReached: true,
                branchTaken: false,
                callCellReadReached: true,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFrameComparisonCallOutcome.MissingCallReturnStatus,
                freshFrameMinus4Address: frameAddress,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                compareResult: compareResult,
                compareFlags: flags,
                equal: true,
                unsignedBelow: false,
                signedLess: false,
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
                ? CapacityFinalCalleeFrameComparisonCallOutcome.CallReturnedNormally
                : CapacityFinalCalleeFrameComparisonCallOutcome.CallDidNotReturnNormally,
            freshFrameMinus4Address: frameAddress,
            freshFrameMinus4ReadReached: true,
            freshFrameMinus4Captured: true,
            freshFrameMinus4: freshFrameMinus4,
            compareResult: compareResult,
            compareFlags: flags,
            equal: true,
            unsignedBelow: false,
            signedLess: false,
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

    private static CapacityFinalCalleeFrameComparisonFlags CalculateFlags(
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

    private static CapacityFinalCalleeFrameComparisonCallInstructionEvidence Instruction(
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

    private static CapacityFinalCalleeFrameComparisonCallPlan BuildPlan(
        CapacityFinalCalleeFrameComparisonCallCapture capture,
        CapacityFinalCalleeFrameComparisonCallOutcome outcome,
        uint? freshFrameMinus4Address = null,
        bool freshFrameMinus4ReadReached = false,
        bool freshFrameMinus4Captured = false,
        uint? freshFrameMinus4 = null,
        uint? compareResult = null,
        CapacityFinalCalleeFrameComparisonFlags? compareFlags = null,
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
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            freshFrameMinus4ReadReached ? freshFrameMinus4Address : null,
            freshFrameMinus4ReadReached,
            freshFrameMinus4Captured,
            freshFrameMinus4Captured ? freshFrameMinus4 : null,
            freshFrameMinus4Captured ? compareResult : null,
            freshFrameMinus4Captured ? compareFlags : null,
            freshFrameMinus4Captured ? equal : null,
            freshFrameMinus4Captured ? unsignedBelow : null,
            freshFrameMinus4Captured ? signedLess : null,
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
            callReached ? capture.EntryEBP : null,
            callReached ? capture.EntryESP : null,
            callOutputsOpaque,
            (!freshFrameMinus4ReadReached
                && capture.FreshFrameMinus4.HasValue)
                || unreachedFreshFrameMinus4CaptureProvided,
            !callCellReadReached
                && capture.CallCellTargetVirtualAddress.HasValue
                || unreachedCallCellTargetCaptureProvided,
            !callReached
                && capture.CallReturnedNormally.HasValue
                || unreachedCallReturnStatusProvided,
            stopAt);
}
