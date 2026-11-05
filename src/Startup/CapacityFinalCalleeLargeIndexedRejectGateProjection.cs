using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeIndexedRejectGateCapture(
    uint? EntryECX,
    uint? EntryEBX);

public enum CapacityFinalCalleeLargeIndexedRejectGateOutcome
{
    MissingEntryECX,
    MissingEntryEBX,
    MissingEntryECXAndEBX,
    UnsignedBelowToTarget,
    UnsignedNotBelowToFallthrough
}

public enum CapacityFinalCalleeLargeIndexedRejectGateEventKind
{
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeIndexedRejectGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeIndexedRejectGateEvent(
    CapacityFinalCalleeLargeIndexedRejectGateEventKind Kind,
    uint RawFileOffset,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeIndexedRejectGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeIndexedRejectGatePlan(
    CapacityFinalCalleeLargeIndexedRejectGateCapture Capture,
    CapacityFinalCalleeLargeIndexedRejectGateOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool EntryEBXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeIndexedRejectGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeIndexedRejectGateFlags? FlagsAtStop => CompareFlags;
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent>.Empty;
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the register CMP/JB rejection gate at raw 9263 through 926B.
/// The entry registers are independent captures; no later field access is modeled.
/// </summary>
public static class CapacityFinalCalleeLargeIndexedRejectGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9263;
    public const uint EntryVirtualAddress = 0x016C9E63;
    public const uint CompareRawFileOffset = 0x012C9263;
    public const uint BranchRawFileOffset = 0x012C9265;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C926B;
    public const uint BranchTargetRawFileOffset = 0x012C8E76;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const string CapturedWindowBytes =
        "3B CB 0F 82 0B FC FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ECX with entry EBX",
                CompareRawFileOffset, "3B CB"),
            Instruction("Branch to 8E76 when unsigned ECX is below EBX",
                BranchRawFileOffset, "0F 82 0B FC FF FF",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeIndexedRejectGatePlan Project(
        CapacityFinalCalleeLargeIndexedRejectGateCapture capture)
    {
        bool ecxCaptured = capture.EntryECX.HasValue;
        bool ebxCaptured = capture.EntryEBX.HasValue;
        uint? compareLeft = capture.EntryECX;
        uint? compareRight = capture.EntryEBX;

        CapacityFinalCalleeLargeIndexedRejectGatePlan Finish(
            CapacityFinalCalleeLargeIndexedRejectGateOutcome outcome,
            bool compareReached,
            uint? compareResult,
            CapacityFinalCalleeLargeIndexedRejectGateFlags? compareFlags,
            bool branchReached,
            bool? branchTaken,
            uint? branchDestination,
            ImmutableArray<uint> reachedInstructions,
            ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent> events,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryECXCaptured = ecxCaptured,
                EntryEBXCaptured = ebxCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                ReachedInstructions = reachedInstructions,
                Events = events,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!ecxCaptured || !ebxCaptured)
        {
            CapacityFinalCalleeLargeIndexedRejectGateOutcome missingOutcome =
                (!ecxCaptured, !ebxCaptured) switch
                {
                    (true, true) =>
                        CapacityFinalCalleeLargeIndexedRejectGateOutcome
                            .MissingEntryECXAndEBX,
                    (true, false) =>
                        CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECX,
                    (false, true) =>
                        CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryEBX,
                    _ => throw new InvalidOperationException()
                };
            return Finish(
                missingOutcome,
                compareReached: false,
                compareResult: null,
                compareFlags: null,
                branchReached: false,
                branchTaken: null,
                branchDestination: null,
                reachedInstructions: ImmutableArray<uint>.Empty,
                events: ImmutableArray<CapacityFinalCalleeLargeIndexedRejectGateEvent>.Empty,
                stopAt: CompareRawFileOffset);
        }

        uint entryECX = capture.EntryECX!.Value;
        uint entryEBX = capture.EntryEBX!.Value;
        uint result = unchecked(entryECX - entryEBX);
        CapacityFinalCalleeLargeIndexedRejectGateFlags flags =
            CalculateCompareFlags(entryECX, entryEBX, result);
        bool branchTakenValue = flags.Carry;
        uint destination = branchTakenValue
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        var reached = ImmutableArray.Create(CompareRawFileOffset, BranchRawFileOffset);
        var events = ImmutableArray.Create(
            new CapacityFinalCalleeLargeIndexedRejectGateEvent(
                CapacityFinalCalleeLargeIndexedRejectGateEventKind.Compare,
                CompareRawFileOffset,
                Left: entryECX,
                Right: entryEBX,
                Result: result,
                Flags: flags),
            new CapacityFinalCalleeLargeIndexedRejectGateEvent(
                CapacityFinalCalleeLargeIndexedRejectGateEventKind.Branch,
                BranchRawFileOffset,
                Flags: flags,
                BranchTaken: branchTakenValue,
                SelectedDestinationRawFileOffset: destination));

        return Finish(
            branchTakenValue
                ? CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedBelowToTarget
                : CapacityFinalCalleeLargeIndexedRejectGateOutcome.UnsignedNotBelowToFallthrough,
            compareReached: true,
            compareResult: result,
            compareFlags: flags,
            branchReached: true,
            branchTaken: branchTakenValue,
            branchDestination: destination,
            reachedInstructions: reached,
            events: events,
            stopAt: destination,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeIndexedRejectGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence Instruction(
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
