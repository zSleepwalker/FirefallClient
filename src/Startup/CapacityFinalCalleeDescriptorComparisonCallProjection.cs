using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescriptorComparisonCallCapture(
    bool CaptureValidatedEntry,
    uint EntryEAX,
    uint EntryEBX,
    uint EntryESI,
    uint EntryEBP,
    uint EntryESP,
    uint? FreshFrameMinus10,
    uint? FreshEaxPlus8,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeDescriptorComparisonCallOutcome
{
    EntryNotValidated,
    MissingFreshFrameMinus10,
    FirstComparisonEqualCallSkipped,
    MissingFreshEaxPlus8,
    MemoryComparisonEqualCallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public readonly record struct CapacityFinalCalleeDescriptorComparisonFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeDescriptorComparisonCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeDescriptorComparisonCallPlan(
    CapacityFinalCalleeDescriptorComparisonCallCapture Capture,
    CapacityFinalCalleeDescriptorComparisonCallOutcome Outcome,
    uint? FreshFrameMinus10Address,
    bool FreshFrameMinus10ReadReached,
    bool FreshFrameMinus10Captured,
    uint? FreshFrameMinus10,
    uint? FirstCompareResult,
    CapacityFinalCalleeDescriptorComparisonFlags? FirstCompareFlags,
    bool? FirstCompareEqual,
    bool FirstEqualityBranchReached,
    bool? FirstEqualityBranchTaken,
    uint? FirstEqualityBranchDestinationRawFileOffset,
    uint? UnsignedCompareResult,
    CapacityFinalCalleeDescriptorComparisonFlags? UnsignedCompareFlags,
    bool? UnsignedBelow,
    bool UnsignedBelowBranchReached,
    bool? UnsignedBelowBranchTaken,
    uint? UnsignedBelowBranchDestinationRawFileOffset,
    uint? FreshEaxPlus8Address,
    bool FreshEaxPlus8ReadReached,
    bool FreshEaxPlus8Captured,
    uint? FreshEaxPlus8,
    uint? MemoryCompareResult,
    CapacityFinalCalleeDescriptorComparisonFlags? MemoryCompareFlags,
    bool? MemoryCompareEqual,
    bool MemoryEqualityBranchReached,
    bool? MemoryEqualityBranchTaken,
    uint? MemoryEqualityBranchDestinationRawFileOffset,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    uint? PreCallEAX,
    uint? PreCallEBX,
    uint? PreCallESI,
    uint? PreCallEBP,
    uint? PreCallESP,
    bool CallOutputsOpaque,
    bool UnreachedFreshFrameMinus10CaptureProvided,
    bool UnreachedFreshEaxPlus8CaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the ordered descriptor comparisons and indirect-call gate at raw 12C8ED5.
/// Each memory operand is an independent fresh capture; no earlier call output is inferred.
/// </summary>
public static class CapacityFinalCalleeDescriptorComparisonCallProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8ED5;
    public const uint EntryVirtualAddress = 0x016C9AD5;
    public const uint FirstEqualityBranchRawFileOffset = 0x012C8ED8;
    public const uint UnsignedCompareRawFileOffset = 0x012C8EDA;
    public const uint UnsignedBelowBranchRawFileOffset = 0x012C8EDC;
    public const uint MemoryCompareRawFileOffset = 0x012C8EDE;
    public const uint MemoryEqualityBranchRawFileOffset = 0x012C8EE1;
    public const uint CallRawFileOffset = 0x012C8EE3;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8EE9;
    public const uint BoundaryVirtualAddress = 0x016C9AE9;

    public const uint CallCellVirtualAddress = 0x01AC4A24;
    public const uint FrameMinus10Displacement = 0x10;
    public const uint EaxPlus8Displacement = 8;

    public const string CapturedWindowBytes =
        "3B 45 F0 74 0F 3B C3 72 05 39 70 08 74 06 FF 15 24 4A AC 01";

    public static ImmutableArray<CapacityFinalCalleeDescriptorComparisonCallInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EAX with the fresh DWORD at entry EBP-10",
                EntryRawFileOffset, "3B 45 F0"),
            Instruction("JZ to the exclusive boundary when entry EAX equals the fresh DWORD",
                FirstEqualityBranchRawFileOffset, "74 0F",
                branchTargetRawFileOffset: BoundaryRawFileOffsetExclusive,
                branchTargetVirtualAddress: BoundaryVirtualAddress),
            Instruction("Compare entry EAX with entry EBX",
                UnsignedCompareRawFileOffset, "3B C3"),
            Instruction("JB directly to the indirect call when entry EAX is unsigned-below entry EBX",
                UnsignedBelowBranchRawFileOffset, "72 05",
                branchTargetRawFileOffset: CallRawFileOffset,
                branchTargetVirtualAddress: RawFileOffsetToVirtualAddress(CallRawFileOffset)),
            Instruction("Compare the fresh DWORD at entry EAX+8 with entry ESI",
                MemoryCompareRawFileOffset, "39 70 08"),
            Instruction("JZ to the exclusive boundary when the fresh DWORD equals entry ESI",
                MemoryEqualityBranchRawFileOffset, "74 06",
                branchTargetRawFileOffset: BoundaryRawFileOffsetExclusive,
                branchTargetVirtualAddress: BoundaryVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC4A24",
                CallRawFileOffset, "FF 15 24 4A AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshFrameMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus10Displacement);

    public static uint DeriveFreshEaxPlus8Address(uint entryEax) =>
        unchecked(entryEax + EaxPlus8Displacement);

    public static CapacityFinalCalleeDescriptorComparisonCallPlan Project(
        CapacityFinalCalleeDescriptorComparisonCallCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.EntryNotValidated,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlus8CaptureProvided:
                    capture.FreshEaxPlus8.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameAddress = DeriveFreshFrameMinus10Address(capture.EntryEBP);
        if (capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingFreshFrameMinus10,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                unreachedFreshEaxPlus8CaptureProvided:
                    capture.FreshEaxPlus8.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint firstCompareResult = unchecked(capture.EntryEAX - freshFrameMinus10);
        CapacityFinalCalleeDescriptorComparisonFlags firstFlags =
            CalculateFlags(capture.EntryEAX, freshFrameMinus10, firstCompareResult);
        if (firstFlags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.FirstComparisonEqualCallSkipped,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                firstCompareEqual: true,
                firstEqualityBranchReached: true,
                firstEqualityBranchTaken: true,
                firstEqualityBranchDestinationRawFileOffset: BoundaryRawFileOffsetExclusive,
                unreachedFreshEaxPlus8CaptureProvided:
                    capture.FreshEaxPlus8.HasValue,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: BoundaryRawFileOffsetExclusive);
        }

        uint unsignedCompareResult = unchecked(capture.EntryEAX - capture.EntryEBX);
        CapacityFinalCalleeDescriptorComparisonFlags unsignedFlags =
            CalculateFlags(capture.EntryEAX, capture.EntryEBX, unsignedCompareResult);
        if (unsignedFlags.Carry)
        {
            return ProjectCall(
                capture,
                frameAddress,
                freshFrameMinus10,
                firstCompareResult,
                firstFlags,
                unsignedCompareResult,
                unsignedFlags,
                freshEaxPlus8ReadReached: false,
                unreachedFreshEaxPlus8CaptureProvided:
                    capture.FreshEaxPlus8.HasValue);
        }

        uint freshEaxPlus8Address = DeriveFreshEaxPlus8Address(capture.EntryEAX);
        if (capture.FreshEaxPlus8 is not uint freshEaxPlus8)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingFreshEaxPlus8,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                firstCompareEqual: false,
                firstEqualityBranchReached: true,
                firstEqualityBranchTaken: false,
                unsignedCompareResult: unsignedCompareResult,
                unsignedCompareFlags: unsignedFlags,
                unsignedBelow: false,
                unsignedBelowBranchReached: true,
                unsignedBelowBranchTaken: false,
                freshEaxPlus8Address: freshEaxPlus8Address,
                freshEaxPlus8ReadReached: true,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: MemoryCompareRawFileOffset);
        }

        uint memoryCompareResult = unchecked(freshEaxPlus8 - capture.EntryESI);
        CapacityFinalCalleeDescriptorComparisonFlags memoryFlags =
            CalculateFlags(freshEaxPlus8, capture.EntryESI, memoryCompareResult);
        if (memoryFlags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.MemoryComparisonEqualCallSkipped,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                firstCompareEqual: false,
                firstEqualityBranchReached: true,
                firstEqualityBranchTaken: false,
                unsignedCompareResult: unsignedCompareResult,
                unsignedCompareFlags: unsignedFlags,
                unsignedBelow: false,
                unsignedBelowBranchReached: true,
                unsignedBelowBranchTaken: false,
                freshEaxPlus8Address: freshEaxPlus8Address,
                freshEaxPlus8ReadReached: true,
                freshEaxPlus8Captured: true,
                freshEaxPlus8: freshEaxPlus8,
                memoryCompareResult: memoryCompareResult,
                memoryCompareFlags: memoryFlags,
                memoryCompareEqual: true,
                memoryEqualityBranchReached: true,
                memoryEqualityBranchTaken: true,
                memoryEqualityBranchDestinationRawFileOffset:
                    BoundaryRawFileOffsetExclusive,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                stopAt: BoundaryRawFileOffsetExclusive);
        }

        return ProjectCall(
            capture,
            frameAddress,
            freshFrameMinus10,
            firstCompareResult,
            firstFlags,
            unsignedCompareResult,
            unsignedFlags,
            freshEaxPlus8ReadReached: true,
            freshEaxPlus8Address: freshEaxPlus8Address,
            freshEaxPlus8Captured: true,
            freshEaxPlus8: freshEaxPlus8,
            memoryCompareResult: memoryCompareResult,
            memoryCompareFlags: memoryFlags,
            memoryCompareEqual: false,
            memoryEqualityBranchReached: true,
            memoryEqualityBranchTaken: false);
    }

    private static CapacityFinalCalleeDescriptorComparisonCallPlan ProjectCall(
        CapacityFinalCalleeDescriptorComparisonCallCapture capture,
        uint frameAddress,
        uint freshFrameMinus10,
        uint firstCompareResult,
        CapacityFinalCalleeDescriptorComparisonFlags firstFlags,
        uint unsignedCompareResult,
        CapacityFinalCalleeDescriptorComparisonFlags unsignedFlags,
        bool freshEaxPlus8ReadReached,
        uint? freshEaxPlus8Address = null,
        bool freshEaxPlus8Captured = false,
        uint? freshEaxPlus8 = null,
        uint? memoryCompareResult = null,
        CapacityFinalCalleeDescriptorComparisonFlags? memoryCompareFlags = null,
        bool? memoryCompareEqual = null,
        bool memoryEqualityBranchReached = false,
        bool? memoryEqualityBranchTaken = null,
        bool unreachedFreshEaxPlus8CaptureProvided = false)
    {
        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingCallCellTarget,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                firstCompareEqual: false,
                firstEqualityBranchReached: true,
                firstEqualityBranchTaken: false,
                unsignedCompareResult: unsignedCompareResult,
                unsignedCompareFlags: unsignedFlags,
                unsignedBelow: unsignedFlags.Carry,
                unsignedBelowBranchReached: true,
                unsignedBelowBranchTaken: unsignedFlags.Carry,
                unsignedBelowBranchDestinationRawFileOffset:
                    unsignedFlags.Carry ? CallRawFileOffset : null,
                freshEaxPlus8Address: freshEaxPlus8Address,
                freshEaxPlus8ReadReached: freshEaxPlus8ReadReached,
                freshEaxPlus8Captured: freshEaxPlus8Captured,
                freshEaxPlus8: freshEaxPlus8,
                memoryCompareResult: memoryCompareResult,
                memoryCompareFlags: memoryCompareFlags,
                memoryCompareEqual: memoryCompareEqual,
                memoryEqualityBranchReached: memoryEqualityBranchReached,
                memoryEqualityBranchTaken: memoryEqualityBranchTaken,
                callCellReadReached: true,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                unreachedFreshEaxPlus8CaptureProvided:
                    unreachedFreshEaxPlus8CaptureProvided,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorComparisonCallOutcome.MissingCallReturnStatus,
                freshFrameMinus10Address: frameAddress,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                firstCompareEqual: false,
                firstEqualityBranchReached: true,
                firstEqualityBranchTaken: false,
                unsignedCompareResult: unsignedCompareResult,
                unsignedCompareFlags: unsignedFlags,
                unsignedBelow: unsignedFlags.Carry,
                unsignedBelowBranchReached: true,
                unsignedBelowBranchTaken: unsignedFlags.Carry,
                unsignedBelowBranchDestinationRawFileOffset:
                    unsignedFlags.Carry ? CallRawFileOffset : null,
                freshEaxPlus8Address: freshEaxPlus8Address,
                freshEaxPlus8ReadReached: freshEaxPlus8ReadReached,
                freshEaxPlus8Captured: freshEaxPlus8Captured,
                freshEaxPlus8: freshEaxPlus8,
                memoryCompareResult: memoryCompareResult,
                memoryCompareFlags: memoryCompareFlags,
                memoryCompareEqual: memoryCompareEqual,
                memoryEqualityBranchReached: memoryEqualityBranchReached,
                memoryEqualityBranchTaken: memoryEqualityBranchTaken,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                callReached: true,
                callOutputsOpaque: true,
                unreachedFreshEaxPlus8CaptureProvided:
                    unreachedFreshEaxPlus8CaptureProvided,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? CapacityFinalCalleeDescriptorComparisonCallOutcome.CallReturnedNormally
                : CapacityFinalCalleeDescriptorComparisonCallOutcome.CallDidNotReturnNormally,
            freshFrameMinus10Address: frameAddress,
            freshFrameMinus10ReadReached: true,
            freshFrameMinus10Captured: true,
            freshFrameMinus10: freshFrameMinus10,
            firstCompareResult: firstCompareResult,
            firstCompareFlags: firstFlags,
            firstCompareEqual: false,
            firstEqualityBranchReached: true,
            firstEqualityBranchTaken: false,
            unsignedCompareResult: unsignedCompareResult,
            unsignedCompareFlags: unsignedFlags,
            unsignedBelow: unsignedFlags.Carry,
            unsignedBelowBranchReached: true,
            unsignedBelowBranchTaken: unsignedFlags.Carry,
            unsignedBelowBranchDestinationRawFileOffset:
                unsignedFlags.Carry ? CallRawFileOffset : null,
            freshEaxPlus8Address: freshEaxPlus8Address,
            freshEaxPlus8ReadReached: freshEaxPlus8ReadReached,
            freshEaxPlus8Captured: freshEaxPlus8Captured,
            freshEaxPlus8: freshEaxPlus8,
            memoryCompareResult: memoryCompareResult,
            memoryCompareFlags: memoryCompareFlags,
            memoryCompareEqual: memoryCompareEqual,
            memoryEqualityBranchReached: memoryEqualityBranchReached,
            memoryEqualityBranchTaken: memoryEqualityBranchTaken,
            callCellReadReached: true,
            callCellTargetCaptured: true,
            callCellTargetVirtualAddress: callCellTarget,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            callOutputsOpaque: true,
            unreachedFreshEaxPlus8CaptureProvided:
                unreachedFreshEaxPlus8CaptureProvided,
            stopAt: callReturnedNormally
                ? BoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static CapacityFinalCalleeDescriptorComparisonFlags CalculateFlags(
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

    private static CapacityFinalCalleeDescriptorComparisonCallInstructionEvidence Instruction(
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

    private static CapacityFinalCalleeDescriptorComparisonCallPlan BuildPlan(
        CapacityFinalCalleeDescriptorComparisonCallCapture capture,
        CapacityFinalCalleeDescriptorComparisonCallOutcome outcome,
        uint? freshFrameMinus10Address = null,
        bool freshFrameMinus10ReadReached = false,
        bool freshFrameMinus10Captured = false,
        uint? freshFrameMinus10 = null,
        uint? firstCompareResult = null,
        CapacityFinalCalleeDescriptorComparisonFlags? firstCompareFlags = null,
        bool? firstCompareEqual = null,
        bool firstEqualityBranchReached = false,
        bool? firstEqualityBranchTaken = null,
        uint? firstEqualityBranchDestinationRawFileOffset = null,
        uint? unsignedCompareResult = null,
        CapacityFinalCalleeDescriptorComparisonFlags? unsignedCompareFlags = null,
        bool? unsignedBelow = null,
        bool unsignedBelowBranchReached = false,
        bool? unsignedBelowBranchTaken = null,
        uint? unsignedBelowBranchDestinationRawFileOffset = null,
        uint? freshEaxPlus8Address = null,
        bool freshEaxPlus8ReadReached = false,
        bool freshEaxPlus8Captured = false,
        uint? freshEaxPlus8 = null,
        uint? memoryCompareResult = null,
        CapacityFinalCalleeDescriptorComparisonFlags? memoryCompareFlags = null,
        bool? memoryCompareEqual = null,
        bool memoryEqualityBranchReached = false,
        bool? memoryEqualityBranchTaken = null,
        uint? memoryEqualityBranchDestinationRawFileOffset = null,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool callOutputsOpaque = false,
        bool unreachedFreshFrameMinus10CaptureProvided = false,
        bool unreachedFreshEaxPlus8CaptureProvided = false,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            freshFrameMinus10ReadReached ? freshFrameMinus10Address : null,
            freshFrameMinus10ReadReached,
            freshFrameMinus10Captured,
            freshFrameMinus10Captured ? freshFrameMinus10 : null,
            freshFrameMinus10Captured ? firstCompareResult : null,
            freshFrameMinus10Captured ? firstCompareFlags : null,
            freshFrameMinus10Captured ? firstCompareEqual : null,
            firstEqualityBranchReached,
            firstEqualityBranchReached ? firstEqualityBranchTaken : null,
            firstEqualityBranchReached && firstEqualityBranchTaken == true
                ? firstEqualityBranchDestinationRawFileOffset
                : null,
            unsignedCompareResult,
            unsignedCompareFlags,
            unsignedBelow,
            unsignedBelowBranchReached,
            unsignedBelowBranchReached ? unsignedBelowBranchTaken : null,
            unsignedBelowBranchReached && unsignedBelowBranchTaken == true
                ? unsignedBelowBranchDestinationRawFileOffset
                : null,
            freshEaxPlus8ReadReached ? freshEaxPlus8Address : null,
            freshEaxPlus8ReadReached,
            freshEaxPlus8Captured,
            freshEaxPlus8Captured ? freshEaxPlus8 : null,
            freshEaxPlus8Captured ? memoryCompareResult : null,
            freshEaxPlus8Captured ? memoryCompareFlags : null,
            freshEaxPlus8Captured ? memoryCompareEqual : null,
            memoryEqualityBranchReached,
            memoryEqualityBranchReached ? memoryEqualityBranchTaken : null,
            memoryEqualityBranchReached && memoryEqualityBranchTaken == true
                ? memoryEqualityBranchDestinationRawFileOffset
                : null,
            callCellReadReached,
            callCellTargetCaptured,
            callCellTargetCaptured ? callCellTargetVirtualAddress : null,
            callReached,
            callReached ? callReturnedNormally : null,
            callReached ? capture.EntryEAX : null,
            callReached ? capture.EntryEBX : null,
            callReached ? capture.EntryESI : null,
            callReached ? capture.EntryEBP : null,
            callReached ? capture.EntryESP : null,
            callOutputsOpaque,
            (!freshFrameMinus10ReadReached && capture.FreshFrameMinus10.HasValue)
                || unreachedFreshFrameMinus10CaptureProvided,
            (!freshEaxPlus8ReadReached && capture.FreshEaxPlus8.HasValue)
                || unreachedFreshEaxPlus8CaptureProvided,
            (!callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue)
                || unreachedCallCellTargetCaptureProvided,
            (!callReached && capture.CallReturnedNormally.HasValue)
                || unreachedCallReturnStatusProvided,
            stopAt);
}
