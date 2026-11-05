using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateIndexedFrameGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus10);

public enum CapacityFinalCalleeAlternateIndexedFrameGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus4,
    MissingFreshFrameMinus10,
    EqualToBranchTarget,
    UnequalToFallthrough
}

public readonly record struct CapacityFinalCalleeAlternateIndexedFrameGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateIndexedFrameGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateIndexedFrameGateWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeAlternateIndexedFrameGatePlan(
    CapacityFinalCalleeAlternateIndexedFrameGateCapture Capture,
    CapacityFinalCalleeAlternateIndexedFrameGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus4ReadReached { get; init; }
    public uint? FrameMinus4Address { get; init; }
    public bool FrameMinus4Captured { get; init; }
    public uint? FrameMinus4 { get; init; }
    public bool FrameMinus10ReadReached { get; init; }
    public uint? FrameMinus10Address { get; init; }
    public bool FrameMinus10Captured { get; init; }
    public uint? FrameMinus10 { get; init; }
    public bool IndexedAddressCalculationReached { get; init; }
    public uint? IndexedAddressInput { get; init; }
    public uint? IndexedAddress { get; init; }
    public bool StackStoreReached { get; init; }
    public uint? StackStoreAddress { get; init; }
    public uint? StackStoreValue { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateIndexedFrameGateFlags? CompareFlags { get; init; }
    public bool JeReached { get; init; }
    public bool? JeTaken { get; init; }
    public uint? JeSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedFreshFrameMinus4CaptureProvided { get; init; }
    public bool UnreachedFreshFrameMinus10CaptureProvided { get; init; }
    public bool LateFreshFrameMinus4CaptureProvided { get; init; }
    public bool LateFreshFrameMinus10CaptureProvided { get; init; }
    public bool ExactCleanRouteAdmitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the two fresh frame loads, indexed LEA, local store, and CMP/JE
/// from 913D through the bounded destinations. LEA computes an address; it
/// does not read memory.
/// </summary>
public static class CapacityFinalCalleeAlternateIndexedFrameGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C913D;
    public const uint EntryVirtualAddress = 0x016C9D3D;
    public const uint FrameMinus4ReadRawFileOffset = 0x012C913D;
    public const uint FrameMinus10ReadRawFileOffset = 0x012C9140;
    public const uint IndexedAddressRawFileOffset = 0x012C9143;
    public const uint StackStoreRawFileOffset = 0x012C914A;
    public const uint CompareRawFileOffset = 0x012C914D;
    public const uint JeRawFileOffset = 0x012C914F;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9151;
    public const uint EqualDestinationRawFileOffset = 0x012C9160;
    public const uint UnequalDestinationRawFileOffset = 0x012C9151;

    public const uint EbpMinus4Displacement = 4;
    public const uint EbpMinus10Displacement = 0x10;
    public const uint EbpMinusCDisplacement = 0x0C;
    public const uint IndexedBaseAddress = 0x01EDADF0;
    public const uint IndexedScale = 8;

    public const string CapturedWindowBytes =
        "8B 45 FC 8B 4D F0 8D 04 C5 F0 AD ED 01 89 45 F4 3B C8 74 0F";

    public static ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-4 into EAX",
                FrameMinus4ReadRawFileOffset, "8B 45 FC"),
            Instruction("Load the independent fresh DWORD at entry EBP-10 into ECX",
                FrameMinus10ReadRawFileOffset, "8B 4D F0"),
            Instruction("Compute EAX*8 plus the indexed base address without a memory read",
                IndexedAddressRawFileOffset, "8D 04 C5 F0 AD ED 01"),
            Instruction("Store the computed EAX value at entry EBP-C",
                StackStoreRawFileOffset, "89 45 F4"),
            Instruction("Compare the fresh EBP-10 value in ECX with the indexed address in EAX",
                CompareRawFileOffset, "3B C8"),
            Instruction("Jump to 9160 when ECX equals EAX",
                JeRawFileOffset, "74 0F",
                branchTargetRawFileOffset: EqualDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static uint DeriveEbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus10Displacement);

    public static uint DeriveEbpMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - EbpMinusCDisplacement);

    public static uint CalculateIndexedAddress(uint frameMinus4) =>
        unchecked(frameMinus4 * IndexedScale + IndexedBaseAddress);

    public static CapacityFinalCalleeAlternateIndexedFrameGatePlan Project(
        CapacityFinalCalleeAlternateIndexedFrameGateCapture capture)
    {
        bool ebpCaptured = false;
        bool frameMinus4ReadReached = false;
        uint? frameMinus4Address = null;
        bool frameMinus4Captured = false;
        uint? frameMinus4 = null;
        bool frameMinus10ReadReached = false;
        uint? frameMinus10Address = null;
        bool frameMinus10Captured = false;
        uint? frameMinus10 = null;
        bool indexedAddressCalculationReached = false;
        uint? indexedAddressInput = null;
        uint? indexedAddress = null;
        bool stackStoreReached = false;
        uint? stackStoreAddress = null;
        uint? stackStoreValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateIndexedFrameGateFlags? compareFlags = null;
        bool jeReached = false;
        bool? jeTaken = null;
        uint? jeDestination = null;
        bool unreachedFrameMinus4 = false;
        bool unreachedFrameMinus10 = false;
        bool lateFrameMinus4 = false;
        bool lateFrameMinus10 = false;
        ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite> writes =
            ImmutableArray<CapacityFinalCalleeAlternateIndexedFrameGateWrite>.Empty;

        CapacityFinalCalleeAlternateIndexedFrameGatePlan Finish(
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome outcome,
            uint stopAt,
            bool exactCleanRouteAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinus4ReadReached = frameMinus4ReadReached,
                FrameMinus4Address = frameMinus4Address,
                FrameMinus4Captured = frameMinus4Captured,
                FrameMinus4 = frameMinus4,
                FrameMinus10ReadReached = frameMinus10ReadReached,
                FrameMinus10Address = frameMinus10Address,
                FrameMinus10Captured = frameMinus10Captured,
                FrameMinus10 = frameMinus10,
                IndexedAddressCalculationReached = indexedAddressCalculationReached,
                IndexedAddressInput = indexedAddressInput,
                IndexedAddress = indexedAddress,
                StackStoreReached = stackStoreReached,
                StackStoreAddress = stackStoreAddress,
                StackStoreValue = stackStoreValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JeReached = jeReached,
                JeTaken = jeTaken,
                JeSelectedDestinationRawFileOffset = jeDestination,
                UnreachedFreshFrameMinus4CaptureProvided = unreachedFrameMinus4,
                UnreachedFreshFrameMinus10CaptureProvided = unreachedFrameMinus10,
                LateFreshFrameMinus4CaptureProvided = lateFrameMinus4,
                LateFreshFrameMinus10CaptureProvided = lateFrameMinus10,
                ExactCleanRouteAdmitted = exactCleanRouteAdmitted,
                Writes = writes,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedFrameMinus4 = capture.FreshFrameMinus4.HasValue;
            unreachedFrameMinus10 = capture.FreshFrameMinus10.HasValue;
            lateFrameMinus4 = unreachedFrameMinus4;
            lateFrameMinus10 = unreachedFrameMinus10;
            return Finish(
                CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingEntryEBP,
                FrameMinus4ReadRawFileOffset);
        }

        ebpCaptured = true;
        frameMinus4ReadReached = true;
        frameMinus4Address = DeriveEbpMinus4Address(entryEbp);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            lateFrameMinus10 = capture.FreshFrameMinus10.HasValue;
            unreachedFrameMinus10 = lateFrameMinus10;
            return Finish(
                CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus4,
                FrameMinus4ReadRawFileOffset);
        }

        frameMinus4Captured = true;
        frameMinus4 = freshFrameMinus4;
        frameMinus10ReadReached = true;
        frameMinus10Address = DeriveEbpMinus10Address(entryEbp);
        if (capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return Finish(
                CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus10,
                FrameMinus10ReadRawFileOffset);
        }

        frameMinus10Captured = true;
        frameMinus10 = freshFrameMinus10;
        indexedAddressCalculationReached = true;
        indexedAddressInput = freshFrameMinus4;
        indexedAddress = CalculateIndexedAddress(freshFrameMinus4);
        stackStoreReached = true;
        stackStoreAddress = DeriveEbpMinusCAddress(entryEbp);
        stackStoreValue = indexedAddress;
        writes = ImmutableArray.Create(
            new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                StackStoreRawFileOffset,
                stackStoreAddress.Value,
                stackStoreValue.Value));

        compareReached = true;
        compareLeft = freshFrameMinus10;
        compareRight = indexedAddress;
        compareResult = unchecked(freshFrameMinus10 - indexedAddress.Value);
        compareFlags = CalculateCompareFlags(
            freshFrameMinus10, indexedAddress.Value, compareResult.Value);
        jeReached = true;
        jeTaken = compareFlags.Value.Zero;
        jeDestination = jeTaken.Value
            ? EqualDestinationRawFileOffset
            : UnequalDestinationRawFileOffset;

        return Finish(
            jeTaken.Value
                ? CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget
                : CapacityFinalCalleeAlternateIndexedFrameGateOutcome.UnequalToFallthrough,
            jeDestination.Value,
            exactCleanRouteAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateIndexedFrameGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeAlternateIndexedFrameGateInstructionEvidence Instruction(
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
