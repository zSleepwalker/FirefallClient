using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonSuccessorThresholdGateCapture(
    uint? EntryEDX,
    uint? FreshEdxPlus8,
    uint? FreshGlobal1EDADD8);

public enum CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
{
    MissingEntryEDX,
    MissingFreshEdxPlus8,
    MissingFreshGlobal1EDADD8,
    UnsignedBelowBranchTarget,
    UnsignedNotBelowFallthrough
}

public enum CapacityFinalCalleeCommonSuccessorThresholdGateEventKind
{
    Read,
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeCommonSuccessorThresholdGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonSuccessorThresholdGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonSuccessorThresholdGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCommonSuccessorThresholdGateEvent(
    CapacityFinalCalleeCommonSuccessorThresholdGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeCommonSuccessorThresholdGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonSuccessorThresholdGatePlan(
    CapacityFinalCalleeCommonSuccessorThresholdGateCapture Capture,
    CapacityFinalCalleeCommonSuccessorThresholdGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonSuccessorThresholdGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonSuccessorThresholdGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonSuccessorThresholdGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonSuccessorThresholdGateEvent>.Empty;
    public bool FreshEdxPlus8ReadReached { get; init; }
    public uint? FreshEdxPlus8Address { get; init; }
    public bool FreshEdxPlus8Captured { get; init; }
    public uint? FreshEdxPlus8 { get; init; }
    public bool FreshGlobalReadReached { get; init; }
    public uint? FreshGlobalAddress { get; init; }
    public bool FreshGlobalCaptured { get; init; }
    public uint? FreshGlobal { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeCommonSuccessorThresholdGateFlags? FlagsAtStop => CompareFlags;
    public bool JBReached { get; init; }
    public bool? JBTaken { get; init; }
    public uint? JBSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedFreshGlobalCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus8CaptureProvided =>
        !FreshEdxPlus8ReadReached && Capture.FreshEdxPlus8.HasValue;
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered fresh reads and unsigned CMP/JB at 9334, stopping at
/// the selected edge without entering it or projecting later writes.
/// </summary>
public static class CapacityFinalCalleeCommonSuccessorThresholdGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9334;
    public const uint EntryVirtualAddress = 0x016C9F34;
    public const uint EdxPlus8ReadRawFileOffset = 0x012C9334;
    public const uint CompareRawFileOffset = 0x012C9337;
    public const uint JBRawFileOffset = 0x012C933D;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9343;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const uint EdxPlus8Displacement = 8;
    public const uint FreshGlobalVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "8B 42 08 3B 05 D8 AD ED 01 0F 82 33 FB FF FF";

    public static ImmutableArray<CapacityFinalCalleeCommonSuccessorThresholdGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load the fresh DWORD at entry EDX+8 into EAX",
                EdxPlus8ReadRawFileOffset,
                "8B 42 08"),
            Instruction(
                "Compare loaded EAX with the fresh DWORD at absolute VA 1EDADD8",
                CompareRawFileOffset,
                "3B 05 D8 AD ED 01"),
            Instruction(
                "Branch to 8E76 when the unsigned comparison was below",
                JBRawFileOffset,
                "0F 82 33 FB FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlus8Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus8Displacement);

    public static CapacityFinalCalleeCommonSuccessorThresholdGatePlan Project(
        CapacityFinalCalleeCommonSuccessorThresholdGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonSuccessorThresholdGateRead>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonSuccessorThresholdGateEvent>();
        bool entryEdxCaptured = false;
        bool edxPlus8ReadReached = false;
        uint? edxPlus8Address = null;
        bool edxPlus8Captured = false;
        uint? edxPlus8 = null;
        bool globalReadReached = false;
        bool globalCaptured = false;
        uint? global = null;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeCommonSuccessorThresholdGateFlags? compareFlags = null;
        bool jbReached = false;
        bool? jbTaken = null;
        uint? destination = null;

        CapacityFinalCalleeCommonSuccessorThresholdGatePlan Finish(
            CapacityFinalCalleeCommonSuccessorThresholdGateOutcome outcome,
            uint stopAt,
            bool exactCleanAdmission = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEdxCaptured,
                Reads = reads.ToImmutable(),
                Events = events.ToImmutable(),
                FreshEdxPlus8ReadReached = edxPlus8ReadReached,
                FreshEdxPlus8Address = edxPlus8Address,
                FreshEdxPlus8Captured = edxPlus8Captured,
                FreshEdxPlus8 = edxPlus8,
                FreshGlobalReadReached = globalReadReached,
                FreshGlobalAddress = globalReadReached
                    ? FreshGlobalVirtualAddress
                    : null,
                FreshGlobalCaptured = globalCaptured,
                FreshGlobal = global,
                CompareReached = compareReached,
                CompareLeft = compareReached ? edxPlus8 : null,
                CompareRight = compareReached ? global : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JBReached = jbReached,
                JBTaken = jbTaken,
                JBSelectedDestinationRawFileOffset = destination,
                UnreachedFreshGlobalCaptureProvided =
                    !globalReadReached && capture.FreshGlobal1EDADD8.HasValue,
                ExactCleanAdmission = exactCleanAdmission,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingEntryEDX,
                EdxPlus8ReadRawFileOffset);
        }

        entryEdxCaptured = true;
        edxPlus8ReadReached = true;
        edxPlus8Address = DeriveEdxPlus8Address(entryEdx);
        reads.Add(new(
            EdxPlus8ReadRawFileOffset,
            edxPlus8Address,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEdxPlus8.HasValue,
            Value: capture.FreshEdxPlus8));
        events.Add(new(
            CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Read,
            EdxPlus8ReadRawFileOffset,
            edxPlus8Address,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEdxPlus8.HasValue,
            Value: capture.FreshEdxPlus8));
        if (capture.FreshEdxPlus8 is not uint freshEdxPlus8)
        {
            return Finish(
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome.MissingFreshEdxPlus8,
                EdxPlus8ReadRawFileOffset);
        }

        edxPlus8Captured = true;
        edxPlus8 = freshEdxPlus8;

        globalReadReached = true;
        reads.Add(new(
            CompareRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshGlobal1EDADD8.HasValue,
            Value: capture.FreshGlobal1EDADD8));
        events.Add(new(
            CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Read,
            CompareRawFileOffset,
            FreshGlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshGlobal1EDADD8.HasValue,
            Value: capture.FreshGlobal1EDADD8));
        if (capture.FreshGlobal1EDADD8 is not uint freshGlobal)
        {
            return Finish(
                CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .MissingFreshGlobal1EDADD8,
                CompareRawFileOffset);
        }

        globalCaptured = true;
        global = freshGlobal;
        compareReached = true;
        compareResult = unchecked(freshEdxPlus8 - freshGlobal);
        compareFlags = CalculateCompareFlags(
            freshEdxPlus8, freshGlobal, compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Compare,
            CompareRawFileOffset,
            Left: freshEdxPlus8,
            Right: freshGlobal,
            Result: compareResult,
            Flags: compareFlags));

        jbReached = true;
        jbTaken = compareFlags.Value.Carry;
        destination = jbTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeCommonSuccessorThresholdGateEventKind.Branch,
            JBRawFileOffset,
            Flags: compareFlags,
            BranchTaken: jbTaken,
            SelectedDestinationRawFileOffset: destination));

        return Finish(
            jbTaken.Value
                ? CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedBelowBranchTarget
                : CapacityFinalCalleeCommonSuccessorThresholdGateOutcome
                    .UnsignedNotBelowFallthrough,
            destination.Value,
            exactCleanAdmission: true);
    }

    private static CapacityFinalCalleeCommonSuccessorThresholdGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeCommonSuccessorThresholdGateInstructionEvidence Instruction(
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
