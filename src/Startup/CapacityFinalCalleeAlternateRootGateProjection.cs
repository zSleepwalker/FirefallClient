using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateRootGateCapture(
    uint? EntryEDX,
    uint? FreshDWORD);

public enum CapacityFinalCalleeAlternateRootGateOutcome
{
    MissingEntryEDX,
    MissingFreshDWORD,
    EqualToFallthrough,
    UnequalToAlternateTarget
}

public readonly record struct CapacityFinalCalleeAlternateRootGateCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateRootGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateRootGatePlan(
    CapacityFinalCalleeAlternateRootGateCapture Capture,
    CapacityFinalCalleeAlternateRootGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshDWORDReadReached { get; init; }
    public uint? FreshDWORDAddress { get; init; }
    public bool FreshDWORDCaptured { get; init; }
    public uint? FreshDWORD { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateRootGateCompareFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool LateFreshDWORDCaptureProvided { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects only the bounded CMP/JNZ gate at 90CA; it does not follow either destination.
/// </summary>
public static class CapacityFinalCalleeAlternateRootGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C90CA;
    public const uint EntryVirtualAddress = 0x016C9CCA;
    public const uint CompareRawFileOffset = 0x012C90CA;
    public const uint JnzRawFileOffset = 0x012C90D0;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C90D2;

    public const uint FreshDWORDGlobalVirtualAddress = 0x01EDADDC;
    public const uint AlternateDestinationRawFileOffset = 0x012C90F6;
    public const uint FallthroughDestinationRawFileOffset = 0x012C90D2;

    public const uint InboundXrefRawFileOffset = 0x012C8F1B;
    public const bool InboundXrefMore = false;
    public const uint AdjacentNonFallthroughJumpRawFileOffset = 0x012C90C5;
    public const uint AdjacentNonFallthroughJumpDestinationRawFileOffset = 0x012C9441;

    public const string CapturedWindowBytes =
        "3B 15 DC AD ED 01 75 24";

    public static ImmutableArray<CapacityFinalCalleeAlternateRootGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EDX with the fresh DWORD at the global PE address",
                CompareRawFileOffset, "3B 15 DC AD ED 01"),
            Instruction("Jump to 90F6 when the DWORD comparison is not equal",
                JnzRawFileOffset, "75 24",
                branchTargetRawFileOffset: AlternateDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeAlternateRootGatePlan Project(
        CapacityFinalCalleeAlternateRootGateCapture capture)
    {
        CapacityFinalCalleeAlternateRootGatePlan Finish(
            CapacityFinalCalleeAlternateRootGateOutcome outcome,
            bool entryEdxCaptured = false,
            bool freshReadReached = false,
            uint? freshAddress = null,
            bool freshCaptured = false,
            uint? freshValue = null,
            bool compareReached = false,
            uint? compareResult = null,
            CapacityFinalCalleeAlternateRootGateCompareFlags? compareFlags = null,
            bool jnzReached = false,
            bool? jnzTaken = null,
            uint? jnzDestination = null,
            bool lateFreshDWORDCaptureProvided = false,
            bool exactCleanAdmission = false,
            uint stopAt = CompareRawFileOffset) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEdxCaptured,
                FreshDWORDReadReached = freshReadReached,
                FreshDWORDAddress = freshAddress,
                FreshDWORDCaptured = freshCaptured,
                FreshDWORD = freshValue,
                CompareReached = compareReached,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                LateFreshDWORDCaptureProvided = lateFreshDWORDCaptureProvided,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeAlternateRootGateOutcome.MissingEntryEDX,
                lateFreshDWORDCaptureProvided: capture.FreshDWORD.HasValue);
        }

        if (capture.FreshDWORD is not uint freshDword)
        {
            return Finish(
                CapacityFinalCalleeAlternateRootGateOutcome.MissingFreshDWORD,
                entryEdxCaptured: true,
                freshReadReached: true,
                freshAddress: FreshDWORDGlobalVirtualAddress);
        }

        uint result = unchecked(entryEdx - freshDword);
        CapacityFinalCalleeAlternateRootGateCompareFlags flags =
            CalculateCompareFlags(entryEdx, freshDword, result);
        bool jnzTaken = result != 0;
        uint destination = jnzTaken
            ? AlternateDestinationRawFileOffset
            : FallthroughDestinationRawFileOffset;

        return Finish(
            jnzTaken
                ? CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
                : CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough,
            entryEdxCaptured: true,
            freshReadReached: true,
            freshAddress: FreshDWORDGlobalVirtualAddress,
            freshCaptured: true,
            freshValue: freshDword,
            compareReached: true,
            compareResult: result,
            compareFlags: flags,
            jnzReached: true,
            jnzTaken: jnzTaken,
            jnzDestination: destination,
            exactCleanAdmission: true,
            stopAt: destination);
    }

    private static CapacityFinalCalleeAlternateRootGateCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateRootGateInstructionEvidence Instruction(
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
