using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeEaxGlobalThresholdGateCapture(
    uint? EntryEAX,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
{
    MissingBothCompareOperands,
    MissingEntryEAX,
    MissingFreshGlobalValue,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind
{
    Read,
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeEaxGlobalThresholdGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeLargeEaxGlobalThresholdGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeEaxGlobalThresholdGateEvent(
    CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan(
    CapacityFinalCalleeLargeEaxGlobalThresholdGateCapture Capture,
    CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeEaxGlobalThresholdGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeEaxGlobalThresholdGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeEaxGlobalThresholdGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeEaxGlobalThresholdGateEvent>.Empty;
    public bool FreshGlobalReadReached { get; init; }
    public uint? FreshGlobalAddress { get; init; }
    public bool FreshGlobalCaptured { get; init; }
    public uint? FreshGlobalValue { get; init; }
    public bool EntryEAXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags? FlagsAtStop => CompareFlags;
    public bool JBReached { get; init; }
    public bool? JBTaken { get; init; }
    public uint? JBSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the independently captured absolute operand, DWORD CMP, and
/// bounded unsigned JB without entering either destination.
/// </summary>
public static class CapacityFinalCalleeLargeEaxGlobalThresholdGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C927C;
    public const uint EntryVirtualAddress = 0x016C9E7C;
    public const uint CompareRawFileOffset = 0x012C927C;
    public const uint JBRawFileOffset = 0x012C9282;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9288;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint FreshGlobalVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "3B 05 D8 AD ED 01 0F 82 EE FB FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeEaxGlobalThresholdGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare entry EAX with the fresh DWORD at absolute VA 1EDADD8",
                CompareRawFileOffset,
                "3B 05 D8 AD ED 01"),
            Instruction(
                "Branch to 8E76 when the unsigned comparison was below",
                JBRawFileOffset,
                "0F 82 EE FB FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan Project(
        CapacityFinalCalleeLargeEaxGlobalThresholdGateCapture capture)
    {
        bool freshGlobalCaptured = capture.FreshGlobalValue.HasValue;
        bool entryEaxCaptured = capture.EntryEAX.HasValue;
        var read = new CapacityFinalCalleeLargeEaxGlobalThresholdGateRead(
            CompareRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalCaptured,
            Value: capture.FreshGlobalValue);
        var reads =
            ImmutableArray.Create(read);
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeEaxGlobalThresholdGateEvent>();
        events.Add(new(
            CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind.Read,
            CompareRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalCaptured,
            Value: capture.FreshGlobalValue));

        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags? compareFlags = null;
        bool jbReached = false;
        bool? jbTaken = null;
        uint? destination = null;

        CapacityFinalCalleeLargeEaxGlobalThresholdGatePlan Finish(
            CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                Reads = reads,
                Events = events.ToImmutable(),
                FreshGlobalReadReached = true,
                FreshGlobalAddress = FreshGlobalVirtualAddress,
                FreshGlobalCaptured = freshGlobalCaptured,
                FreshGlobalValue = capture.FreshGlobalValue,
                EntryEAXCaptured = entryEaxCaptured,
                CompareReached = compareReached,
                CompareLeft = capture.EntryEAX,
                CompareRight = capture.FreshGlobalValue,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JBReached = jbReached,
                JBTaken = jbTaken,
                JBSelectedDestinationRawFileOffset = destination,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!entryEaxCaptured || !freshGlobalCaptured)
        {
            CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome outcome =
                !entryEaxCaptured && !freshGlobalCaptured
                    ? CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                        .MissingBothCompareOperands
                    : !entryEaxCaptured
                        ? CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome.MissingEntryEAX
                        : CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                            .MissingFreshGlobalValue;
            return Finish(outcome, CompareRawFileOffset);
        }

        uint eax = capture.EntryEAX!.Value;
        uint global = capture.FreshGlobalValue!.Value;
        compareReached = true;
        compareResult = unchecked(eax - global);
        compareFlags = CalculateCompareFlags(eax, global, compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind.Compare,
            CompareRawFileOffset,
            Left: eax,
            Right: global,
            Result: compareResult,
            Flags: compareFlags));

        jbReached = true;
        jbTaken = compareFlags.Value.Carry;
        destination = jbTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeEaxGlobalThresholdGateEventKind.Branch,
            JBRawFileOffset,
            Flags: compareFlags,
            BranchTaken: jbTaken,
            SelectedDestinationRawFileOffset: destination));

        return Finish(
            jbTaken.Value
                ? CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedBelowBranchTarget
                : CapacityFinalCalleeLargeEaxGlobalThresholdGateOutcome
                    .UnsignedNotBelowFallthrough,
            destination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeLargeEaxGlobalThresholdGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeEaxGlobalThresholdGateInstructionEvidence Instruction(
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
