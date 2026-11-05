using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTailRootGateCapture(
    uint? EntryESI,
    uint? FreshGlobalDWORD);

public enum CapacityFinalCalleeCommonTailRootGateOutcome
{
    MissingEntryESIAndFreshGlobalDWORD,
    MissingEntryESI,
    MissingFreshGlobalDWORD,
    EqualToFallthrough,
    UnequalToBranchTarget
}

public readonly record struct CapacityFinalCalleeCommonTailRootGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonTailRootGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonTailRootGatePlan(
    CapacityFinalCalleeCommonTailRootGateCapture Capture,
    CapacityFinalCalleeCommonTailRootGateOutcome Outcome)
{
    public bool EntryESICaptured { get; init; }
    public bool FreshGlobalReadReached { get; init; }
    public uint? FreshGlobalAddress { get; init; }
    public bool FreshGlobalCaptured { get; init; }
    public uint? FreshGlobalDWORD { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonTailRootGateFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public bool OpaqueEntryReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects only the bounded CMP/JNZ gate at 92C8, without following either destination.
/// </summary>
public static class CapacityFinalCalleeCommonTailRootGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92C8;
    public const uint EntryVirtualAddress = 0x016C9EC8;
    public const uint CompareRawFileOffset = 0x012C92C8;
    public const uint JnzRawFileOffset = 0x012C92CE;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92D0;

    public const uint FreshGlobalDWORDVirtualAddress = 0x01EDADDC;
    public const uint BranchTargetRawFileOffset = 0x012C92EC;
    public const uint FallthroughRawFileOffset = 0x012C92D0;

    public const string CapturedWindowBytes =
        "3B 35 DC AD ED 01 75 1C";

    public static ImmutableArray<CapacityFinalCalleeCommonTailRootGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare entry ESI with the fresh DWORD at the global PE address",
                CompareRawFileOffset,
                "3B 35 DC AD ED 01"),
            Instruction(
                "Jump to 92EC when the DWORD comparison is not equal",
                JnzRawFileOffset,
                "75 1C",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonTailRootGatePlan Project(
        CapacityFinalCalleeCommonTailRootGateCapture capture)
    {
        CapacityFinalCalleeCommonTailRootGatePlan Finish(
            CapacityFinalCalleeCommonTailRootGateOutcome outcome,
            bool entryEsiCaptured = false,
            bool freshReadReached = true,
            bool freshCaptured = false,
            uint? freshValue = null,
            bool compareReached = false,
            uint? compareResult = null,
            CapacityFinalCalleeCommonTailRootGateFlags? compareFlags = null,
            bool jnzReached = false,
            bool? jnzTaken = null,
            uint? jnzDestination = null,
            bool exactCleanAdmission = false,
            uint stopAt = CompareRawFileOffset) =>
            new(capture, outcome)
            {
                EntryESICaptured = entryEsiCaptured,
                FreshGlobalReadReached = freshReadReached,
                FreshGlobalAddress = FreshGlobalDWORDVirtualAddress,
                FreshGlobalCaptured = freshCaptured,
                FreshGlobalDWORD = freshValue,
                CompareReached = compareReached,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        bool entryEsiCaptured = capture.EntryESI.HasValue;
        bool freshGlobalCaptured = capture.FreshGlobalDWORD.HasValue;
        uint? freshValue = capture.FreshGlobalDWORD;

        if (capture.EntryESI is not uint entryEsi)
        {
            return Finish(
                freshGlobalCaptured
                    ? CapacityFinalCalleeCommonTailRootGateOutcome.MissingEntryESI
                    : CapacityFinalCalleeCommonTailRootGateOutcome
                        .MissingEntryESIAndFreshGlobalDWORD,
                freshCaptured: freshGlobalCaptured,
                freshValue: freshValue);
        }

        if (capture.FreshGlobalDWORD is not uint freshGlobal)
        {
            return Finish(
                CapacityFinalCalleeCommonTailRootGateOutcome.MissingFreshGlobalDWORD,
                entryEsiCaptured: true);
        }

        uint result = unchecked(entryEsi - freshGlobal);
        CapacityFinalCalleeCommonTailRootGateFlags flags =
            CalculateCompareFlags(entryEsi, freshGlobal, result);
        bool jnzTaken = !flags.Zero;
        uint destination = jnzTaken
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;

        return Finish(
            jnzTaken
                ? CapacityFinalCalleeCommonTailRootGateOutcome.UnequalToBranchTarget
                : CapacityFinalCalleeCommonTailRootGateOutcome.EqualToFallthrough,
            entryEsiCaptured: true,
            freshCaptured: true,
            freshValue: freshGlobal,
            compareReached: true,
            compareResult: result,
            compareFlags: flags,
            jnzReached: true,
            jnzTaken: jnzTaken,
            jnzDestination: destination,
            exactCleanAdmission: true,
            stopAt: destination);
    }

    private static CapacityFinalCalleeCommonTailRootGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeCommonTailRootGateInstructionEvidence Instruction(
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
