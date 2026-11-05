using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNextFieldThresholdGateCapture(
    uint? EntryECX,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
{
    MissingBothCompareOperands,
    MissingEntryECX,
    MissingFreshGlobalValue,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public readonly record struct CapacityFinalCalleeLargeNextFieldThresholdGateFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeNextFieldThresholdGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeNextFieldThresholdGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeNextFieldThresholdGatePlan(
    CapacityFinalCalleeLargeNextFieldThresholdGateCapture Capture,
    CapacityFinalCalleeLargeNextFieldThresholdGateOutcome Outcome)
{
    public bool CompareInstructionReached { get; init; }
    public bool GlobalReadReached { get; init; }
    public uint? GlobalAddress { get; init; }
    public bool FreshGlobalValueCaptured { get; init; }
    public uint? FreshGlobalValue { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? FlagsAtStop => CompareFlags;
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeLargeNextFieldThresholdGateFlags? BranchFlags { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeNextFieldThresholdGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNextFieldThresholdGateRead>.Empty;
    public bool MemoryWriteReached => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the large path's independent CMP operands and bounded unsigned JB.
/// </summary>
public static class CapacityFinalCalleeLargeNextFieldThresholdGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9292;
    public const uint EntryVirtualAddress = 0x016C9E92;
    public const uint CompareRawFileOffset = 0x012C9292;
    public const uint BranchRawFileOffset = 0x012C9298;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C929E;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "3B 0D D8 AD ED 01 0F 82 D8 FB FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeNextFieldThresholdGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare entry ECX with the fresh DWORD at absolute address 01EDADD8",
                CompareRawFileOffset,
                "3B 0D D8 AD ED 01"),
            Instruction(
                "Branch to 8E76 when the unsigned CMP found entry ECX below the global DWORD",
                BranchRawFileOffset,
                "0F 82 D8 FB FF FF",
                BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeNextFieldThresholdGatePlan Project(
        CapacityFinalCalleeLargeNextFieldThresholdGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNextFieldThresholdGateRead>();

        bool compareInstructionReached = true;
        bool globalReadReached = true;
        bool freshGlobalValueCaptured = capture.FreshGlobalValue.HasValue;
        bool entryECXCaptured = capture.EntryECX.HasValue;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeLargeNextFieldThresholdGateFlags? compareFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        reads.Add(new(
            CompareRawFileOffset,
            GlobalValueVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalValueCaptured,
            Value: capture.FreshGlobalValue));

        CapacityFinalCalleeLargeNextFieldThresholdGatePlan Finish(
            CapacityFinalCalleeLargeNextFieldThresholdGateOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                CompareInstructionReached = compareInstructionReached,
                GlobalReadReached = globalReadReached,
                GlobalAddress = GlobalValueVirtualAddress,
                FreshGlobalValueCaptured = freshGlobalValueCaptured,
                FreshGlobalValue = capture.FreshGlobalValue,
                EntryECXCaptured = entryECXCaptured,
                CompareReached = compareReached,
                CompareLeft = capture.EntryECX,
                CompareRight = capture.FreshGlobalValue,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchFlags = branchReached ? compareFlags : null,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                Reads = reads.ToImmutable(),
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryECX
            || capture.FreshGlobalValue is not uint freshGlobalValue)
        {
            CapacityFinalCalleeLargeNextFieldThresholdGateOutcome outcome =
                (capture.EntryECX, capture.FreshGlobalValue) switch
                {
                    (null, null) =>
                        CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                            .MissingBothCompareOperands,
                    (null, _) =>
                        CapacityFinalCalleeLargeNextFieldThresholdGateOutcome.MissingEntryECX,
                    (_, null) =>
                        CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                            .MissingFreshGlobalValue,
                    _ => throw new InvalidOperationException()
                };
            return Finish(outcome, CompareRawFileOffset);
        }

        compareReached = true;
        compareResult = unchecked(entryECX - freshGlobalValue);
        compareFlags = SubtractFlags(entryECX, freshGlobalValue, compareResult.Value);

        branchReached = true;
        branchTaken = compareFlags.Value.Carry;
        branchDestination = branchTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;

        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                    .UnsignedBelowBranchTarget
                : CapacityFinalCalleeLargeNextFieldThresholdGateOutcome
                    .UnsignedNotBelowFallthrough,
            branchDestination.Value,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeNextFieldThresholdGateFlags SubtractFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: (BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeLargeNextFieldThresholdGateInstructionEvidence Instruction(
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
