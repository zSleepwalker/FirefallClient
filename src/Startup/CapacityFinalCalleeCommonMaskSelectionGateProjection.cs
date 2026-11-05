using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonMaskSelectionGateCapture(
    uint? EntryECX,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonMaskSelectionGateOutcome
{
    MissingEntryECX,
    MissingFreshGlobalValue,
    ZeroTestResultToBounded931B,
    NonzeroTestResultToBounded9334
}

public readonly record struct CapacityFinalCalleeCommonMaskSelectionGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonMaskSelectionGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonMaskSelectionGatePlan(
    CapacityFinalCalleeCommonMaskSelectionGateCapture Capture,
    CapacityFinalCalleeCommonMaskSelectionGateOutcome Outcome)
{
    public bool MoveEDIReached { get; init; }
    public uint? DestinationValueAfterMove { get; init; }
    public bool ShiftReached { get; init; }
    public uint? ShiftCount { get; init; }
    public uint? DestinationValueAfterShift { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionGateFlags? ShiftFlags { get; init; }
    public bool FreshGlobalReadReached { get; init; }
    public uint? FreshGlobalReadAddress =>
        FreshGlobalReadReached
            ? CapacityFinalCalleeCommonMaskSelectionGateProjection.FreshGlobalVirtualAddress
            : null;
    public bool FreshGlobalValueCaptured { get; init; }
    public bool UnreachedFreshGlobalCaptureProvided =>
        !FreshGlobalReadReached && Capture.FreshGlobalValue.HasValue;
    public bool TestReached { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeCommonMaskSelectionGateFlags? FlagsAtStop =>
        TestFlags ?? ShiftFlags;
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanBoundedDestination { get; init; }
    public bool LaterOrOrStoreReached => false;
    public bool MemoryWritePerformed => false;
    public ImmutableArray<uint> Writes => ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the mask shift and fresh-global TEST/JNZ gate, stopping at either bounded branch destination.
/// </summary>
public static class CapacityFinalCalleeCommonMaskSelectionGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C930A;
    public const uint EntryVirtualAddress = 0x016C9F0A;
    public const uint MoveEDIRawFileOffset = 0x012C930A;
    public const uint ShiftRawFileOffset = 0x012C930F;
    public const uint FreshGlobalMoveRawFileOffset = 0x012C9311;
    public const uint TestRawFileOffset = 0x012C9317;
    public const uint JnzRawFileOffset = 0x012C9319;
    public const uint ZeroDestinationRawFileOffset = 0x012C931B;
    public const uint NonzeroDestinationRawFileOffset = 0x012C9334;
    public const uint FreshGlobalVirtualAddress = 0x01EDADC8;

    public const string CapturedWindowBytes =
        "BF 01 00 00 00 D3 E7 8B 0D C8 AD ED 01 85 F9 75 19";

    public static ImmutableArray<CapacityFinalCalleeCommonMaskSelectionGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Set the destination register EDI to one",
                MoveEDIRawFileOffset,
                "BF 01 00 00 00"),
            Instruction(
                "Shift EDI left by the low five bits of entry ECX",
                ShiftRawFileOffset,
                "D3 E7"),
            Instruction(
                "Read the fresh global value into ECX",
                FreshGlobalMoveRawFileOffset,
                "8B 0D C8 AD ED 01"),
            Instruction(
                "TEST the fresh global value against the selected EDI mask",
                TestRawFileOffset,
                "85 F9"),
            Instruction(
                "Branch to 9334 when the TEST result is nonzero",
                JnzRawFileOffset,
                "75 19",
                branchTargetRawFileOffset: NonzeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonMaskSelectionGatePlan Project(
        CapacityFinalCalleeCommonMaskSelectionGateCapture capture)
    {
        CapacityFinalCalleeCommonMaskSelectionGatePlan Finish(
            CapacityFinalCalleeCommonMaskSelectionGateOutcome outcome,
            bool moveEdiReached = true,
            uint? destinationAfterMove = 1,
            bool shiftReached = false,
            uint? shiftCount = null,
            uint? destinationAfterShift = null,
            CapacityFinalCalleeCommonMaskSelectionGateFlags? shiftFlags = null,
            bool freshGlobalReadReached = false,
            bool freshGlobalValueCaptured = false,
            bool testReached = false,
            uint? testResult = null,
            CapacityFinalCalleeCommonMaskSelectionGateFlags? testFlags = null,
            bool jnzReached = false,
            bool? jnzTaken = null,
            uint? jnzDestination = null,
            bool exactCleanBoundedDestination = false,
            uint stopAt = ShiftRawFileOffset) =>
            new(capture, outcome)
            {
                MoveEDIReached = moveEdiReached,
                DestinationValueAfterMove = destinationAfterMove,
                ShiftReached = shiftReached,
                ShiftCount = shiftCount,
                DestinationValueAfterShift = destinationAfterShift,
                ShiftFlags = shiftFlags,
                FreshGlobalReadReached = freshGlobalReadReached,
                FreshGlobalValueCaptured = freshGlobalValueCaptured,
                TestReached = testReached,
                TestResult = testResult,
                TestFlags = testFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                ExactCleanBoundedDestination = exactCleanBoundedDestination,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryEcx)
        {
            return Finish(
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingEntryECX);
        }

        uint shiftCount = entryEcx & 0x1F;
        uint shiftedDestination = shiftCount == 0 ? 1u : 1u << (int)shiftCount;
        CapacityFinalCalleeCommonMaskSelectionGateFlags shiftFlags =
            CalculateShiftFlags(shiftCount, shiftedDestination);

        if (capture.FreshGlobalValue is not uint freshGlobal)
        {
            return Finish(
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingFreshGlobalValue,
                shiftReached: true,
                shiftCount: shiftCount,
                destinationAfterShift: shiftedDestination,
                shiftFlags: shiftFlags,
                freshGlobalReadReached: true,
                stopAt: FreshGlobalMoveRawFileOffset);
        }

        uint testResult = freshGlobal & shiftedDestination;
        CapacityFinalCalleeCommonMaskSelectionGateFlags testFlags =
            CalculateTestFlags(testResult);
        bool jnzTaken = testResult != 0;
        uint destination = jnzTaken
            ? NonzeroDestinationRawFileOffset
            : ZeroDestinationRawFileOffset;

        return Finish(
            jnzTaken
                ? CapacityFinalCalleeCommonMaskSelectionGateOutcome
                    .NonzeroTestResultToBounded9334
                : CapacityFinalCalleeCommonMaskSelectionGateOutcome
                    .ZeroTestResultToBounded931B,
            shiftReached: true,
            shiftCount: shiftCount,
            destinationAfterShift: shiftedDestination,
            shiftFlags: shiftFlags,
            freshGlobalReadReached: true,
            freshGlobalValueCaptured: true,
            testReached: true,
            testResult: testResult,
            testFlags: testFlags,
            jnzReached: true,
            jnzTaken: jnzTaken,
            jnzDestination: destination,
            exactCleanBoundedDestination: true,
            stopAt: destination);
    }

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags CalculateShiftFlags(
        uint shiftCount,
        uint result)
    {
        if (shiftCount == 0)
        {
            return UnknownFlags;
        }

        bool carry = ((1u >> (int)(32 - shiftCount)) & 1) != 0;
        bool? overflow = shiftCount == 1
            ? ((result & 0x80000000) != 0) ^ carry
            : null;

        return new(
            carry,
            HasEvenParity((byte)result),
            null,
            result == 0,
            (result & 0x80000000) != 0,
            overflow);
    }

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags CalculateTestFlags(
        uint result) =>
        new(
            false,
            HasEvenParity((byte)result),
            null,
            result == 0,
            (result & 0x80000000) != 0,
            false);

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags UnknownFlags =>
        new(null, null, null, null, null, null);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonMaskSelectionGateInstructionEvidence Instruction(
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
