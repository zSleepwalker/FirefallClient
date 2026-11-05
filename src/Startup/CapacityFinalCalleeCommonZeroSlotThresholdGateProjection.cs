using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonZeroSlotThresholdGateCapture(
    uint? EntryECX,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
{
    MissingBothCompareOperands,
    MissingEntryECX,
    MissingFreshGlobalValue,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public readonly record struct CapacityFinalCalleeCommonZeroSlotThresholdGateFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonZeroSlotThresholdGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonZeroSlotThresholdGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonZeroSlotThresholdGatePlan(
    CapacityFinalCalleeCommonZeroSlotThresholdGateCapture Capture,
    CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome Outcome)
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
    public CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? FlagsAtStop => CompareFlags;
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? BranchFlags { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonZeroSlotThresholdGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonZeroSlotThresholdGateRead>.Empty;
    public bool MemoryWriteReached => false;
    public bool TargetEntered => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the common zero-slot path's post-RET CMP operands and bounded unsigned JB.
/// </summary>
public static class CapacityFinalCalleeCommonZeroSlotThresholdGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C945C;
    public const uint EntryVirtualAddress = 0x016CA05C;
    public const uint CompareRawFileOffset = 0x012C945C;
    public const uint BranchRawFileOffset = 0x012C9462;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9468;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "3B 0D D8 AD ED 01 0F 82 0E FA FF FF";

    public static ImmutableArray<CapacityFinalCalleeCommonZeroSlotThresholdGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare ECX with the fresh DWORD at absolute address 01EDADD8",
                CompareRawFileOffset,
                "3B 0D D8 AD ED 01"),
            Instruction(
                "Branch to 8E76 when the unsigned CMP found ECX below the global DWORD",
                BranchRawFileOffset,
                "0F 82 0E FA FF FF",
                BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonZeroSlotThresholdGatePlan Project(
        CapacityFinalCalleeCommonZeroSlotThresholdGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonZeroSlotThresholdGateRead>();

        const bool compareInstructionReached = true;
        const bool globalReadReached = true;
        bool freshGlobalValueCaptured = capture.FreshGlobalValue.HasValue;
        bool entryECXCaptured = capture.EntryECX.HasValue;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeCommonZeroSlotThresholdGateFlags? compareFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        reads.Add(new(
            CompareRawFileOffset,
            GlobalValueVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalValueCaptured,
            Value: capture.FreshGlobalValue));

        CapacityFinalCalleeCommonZeroSlotThresholdGatePlan Finish(
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome,
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
            CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome outcome =
                (capture.EntryECX, capture.FreshGlobalValue) switch
                {
                    (null, null) =>
                        CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                            .MissingBothCompareOperands,
                    (null, _) =>
                        CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome.MissingEntryECX,
                    (_, null) =>
                        CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
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
                ? CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedBelowBranchTarget
                : CapacityFinalCalleeCommonZeroSlotThresholdGateOutcome
                    .UnsignedNotBelowFallthrough,
            branchDestination.Value,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeCommonZeroSlotThresholdGateFlags SubtractFlags(
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

    private static CapacityFinalCalleeCommonZeroSlotThresholdGateInstructionEvidence Instruction(
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
