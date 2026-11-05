using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeIndexedFieldGateCapture(
    uint? EntryECX,
    uint? EntryEDX,
    uint? FreshECXPlus10);

public enum CapacityFinalCalleeLargeIndexedFieldGateOutcome
{
    MissingEntryECX,
    MissingFreshECXPlus10,
    MissingFreshECXPlus10AndEntryEDX,
    MissingEntryEDX,
    UnequalToJnz9275,
    EqualToFallthrough9270
}

public enum CapacityFinalCalleeLargeIndexedFieldGateEventKind
{
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeIndexedFieldGateFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldGateEvent(
    CapacityFinalCalleeLargeIndexedFieldGateEventKind Kind,
    uint RawFileOffset,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeIndexedFieldGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeIndexedFieldGatePlan(
    CapacityFinalCalleeLargeIndexedFieldGateCapture Capture,
    CapacityFinalCalleeLargeIndexedFieldGateOutcome Outcome)
{
    public bool CompareInstructionReached { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool FreshECXPlus10ReadReached { get; init; }
    public uint? FreshECXPlus10Address { get; init; }
    public bool FreshECXPlus10Captured { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeIndexedFieldGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeIndexedFieldGateFlags? FlagsAtStop => CompareFlags;
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateEvent>.Empty;
    public bool UnreachedFreshECXPlus10CaptureProvided { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool MemoryWriteReached => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the indexed-field CMP/JNZ gate at raw 926B through 9270.
/// </summary>
public static class CapacityFinalCalleeLargeIndexedFieldGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C926B;
    public const uint EntryVirtualAddress = 0x016C9E6B;
    public const uint CompareRawFileOffset = 0x012C926B;
    public const uint JnzRawFileOffset = 0x012C926E;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9270;
    public const uint JnzDestinationRawFileOffset = 0x012C9275;
    public const uint FreshECXPlus10Displacement = 0x10;

    public const string CapturedWindowBytes = "39 51 10 75 05";

    public static ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare the fresh DWORD at entry ECX+10 with entry EDX",
                CompareRawFileOffset,
                "39 51 10"),
            Instruction(
                "Jump to 9275 when the indexed DWORD differs from entry EDX",
                JnzRawFileOffset,
                "75 05",
                JnzDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshECXPlus10Address(uint entryECX) =>
        unchecked(entryECX + FreshECXPlus10Displacement);

    public static CapacityFinalCalleeLargeIndexedFieldGatePlan Project(
        CapacityFinalCalleeLargeIndexedFieldGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeIndexedFieldGateRead>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeIndexedFieldGateEvent>();

        bool compareInstructionReached = true;
        bool entryECXCaptured = false;
        bool freshReadReached = false;
        uint? freshAddress = null;
        bool freshCaptured = false;
        bool entryEDXCaptured = capture.EntryEDX.HasValue;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = capture.EntryEDX;
        uint? compareResult = null;
        CapacityFinalCalleeLargeIndexedFieldGateFlags? compareFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;
        reached.Add(CompareRawFileOffset);

        CapacityFinalCalleeLargeIndexedFieldGatePlan Finish(
            CapacityFinalCalleeLargeIndexedFieldGateOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                CompareInstructionReached = compareInstructionReached,
                EntryECXCaptured = entryECXCaptured,
                FreshECXPlus10ReadReached = freshReadReached,
                FreshECXPlus10Address = freshAddress,
                FreshECXPlus10Captured = freshCaptured,
                EntryEDXCaptured = entryEDXCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                Events = events.ToImmutable(),
                UnreachedFreshECXPlus10CaptureProvided =
                    !freshReadReached && capture.FreshECXPlus10.HasValue,
                UnreachedEntryEDXCaptureProvided = false,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryECX)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX,
                CompareRawFileOffset);
        }

        entryECXCaptured = true;
        freshReadReached = true;
        freshAddress = DeriveFreshECXPlus10Address(entryECX);
        reads.Add(new(
            CompareRawFileOffset,
            freshAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshECXPlus10.HasValue,
            Value: capture.FreshECXPlus10));
        if (capture.FreshECXPlus10 is not uint freshECXPlus10)
        {
            return Finish(
                capture.EntryEDX.HasValue
                    ? CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingFreshECXPlus10
                    : CapacityFinalCalleeLargeIndexedFieldGateOutcome
                        .MissingFreshECXPlus10AndEntryEDX,
                CompareRawFileOffset);
        }

        freshCaptured = true;
        compareLeft = freshECXPlus10;
        if (capture.EntryEDX is not uint entryEDX)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryEDX,
                CompareRawFileOffset);
        }

        entryEDXCaptured = true;
        compareReached = true;
        compareLeft = freshECXPlus10;
        compareRight = entryEDX;
        compareResult = unchecked(freshECXPlus10 - entryEDX);
        compareFlags = CalculateCompareFlags(
            freshECXPlus10,
            entryEDX,
            compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeIndexedFieldGateEventKind.Compare,
            CompareRawFileOffset,
            Left: freshECXPlus10,
            Right: entryEDX,
            Result: compareResult,
            Flags: compareFlags));

        branchReached = true;
        reached.Add(JnzRawFileOffset);
        branchTaken = !compareFlags.Value.Zero;
        branchDestination = branchTaken.Value
            ? JnzDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        events.Add(new(
            CapacityFinalCalleeLargeIndexedFieldGateEventKind.Branch,
            JnzRawFileOffset,
            Flags: compareFlags,
            BranchTaken: branchTaken,
            SelectedDestinationRawFileOffset: branchDestination));

        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeIndexedFieldGateOutcome.UnequalToJnz9275
                : CapacityFinalCalleeLargeIndexedFieldGateOutcome.EqualToFallthrough9270,
            branchDestination.Value,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeIndexedFieldGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeIndexedFieldGateInstructionEvidence Instruction(
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
