using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeFrameBacklinkGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus14,
    uint? EntryEDX,
    uint? FreshBacklink);

public enum CapacityFinalCalleeLargeFrameBacklinkGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus14,
    MissingEntryEDX,
    MissingFreshBacklink,
    EqualToFallthrough,
    UnequalToTarget
}

public enum CapacityFinalCalleeLargeFrameBacklinkGateEventKind
{
    Read,
    Compare,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeFrameBacklinkGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeFrameBacklinkGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeFrameBacklinkGateEvent(
    CapacityFinalCalleeLargeFrameBacklinkGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeFrameBacklinkGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeFrameBacklinkGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeFrameBacklinkGatePlan(
    CapacityFinalCalleeLargeFrameBacklinkGateCapture Capture,
    CapacityFinalCalleeLargeFrameBacklinkGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinus14ReadReached { get; init; }
    public uint? FrameMinus14Address { get; init; }
    public bool FrameMinus14Captured { get; init; }
    public uint? FrameMinus14 { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool BacklinkReadReached { get; init; }
    public uint? BacklinkAddress { get; init; }
    public bool BacklinkCaptured { get; init; }
    public uint? BacklinkValue { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeFrameBacklinkGateFlags? CompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeFrameBacklinkGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameBacklinkGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameBacklinkGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameBacklinkGateEvent>.Empty;
    public bool UnreachedFreshFrameMinus14CaptureProvided { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshBacklinkCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered frame-minus-14 load and memory CMP/JNZ backlink gate.
/// </summary>
public static class CapacityFinalCalleeLargeFrameBacklinkGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91B9;
    public const uint EntryVirtualAddress = 0x016C9DB9;
    public const uint FrameMinus14ReadRawFileOffset = 0x012C91B9;
    public const uint BacklinkReadRawFileOffset = 0x012C91BC;
    public const uint CompareRawFileOffset = 0x012C91BC;
    public const uint JnzRawFileOffset = 0x012C91BF;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91C5;
    public const uint NonzeroDestinationRawFileOffset = 0x012C8E76;
    public const uint ZeroDestinationRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const uint FrameMinus14Displacement = 0x14;
    public const uint BacklinkDisplacement = 0x0C;

    public const string CapturedWindowBytes =
        "8B 5D EC 39 53 0C 0F 85 B1 FC FF FF";

    public static ImmutableArray<CapacityFinalCalleeLargeFrameBacklinkGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-14 into EBX",
                FrameMinus14ReadRawFileOffset, "8B 5D EC"),
            Instruction("Compare the fresh DWORD at EBX+C with entry EDX",
                CompareRawFileOffset, "39 53 0C"),
            Instruction("Jump to 8E76 when the fresh backlink differs from entry EDX",
                JnzRawFileOffset, "0F 85 B1 FC FF FF",
                branchTargetRawFileOffset: NonzeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus14Displacement);

    public static uint DeriveBacklinkAddress(uint frameMinus14) =>
        unchecked(frameMinus14 + BacklinkDisplacement);

    public static CapacityFinalCalleeLargeFrameBacklinkGatePlan Project(
        CapacityFinalCalleeLargeFrameBacklinkGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameBacklinkGateRead>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameBacklinkGateEvent>();
        bool ebpCaptured = false;
        bool frameReadReached = true;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool edxCaptured = false;
        bool backlinkReadReached = false;
        uint? backlinkAddress = null;
        bool backlinkCaptured = false;
        uint? backlinkValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeLargeFrameBacklinkGateFlags? compareFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;

        CapacityFinalCalleeLargeFrameBacklinkGatePlan Finish(
            CapacityFinalCalleeLargeFrameBacklinkGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinus14ReadReached = frameReadReached,
                FrameMinus14Address = frameAddress,
                FrameMinus14Captured = frameCaptured,
                FrameMinus14 = frameValue,
                EntryEDXCaptured = edxCaptured,
                BacklinkReadReached = backlinkReadReached,
                BacklinkAddress = backlinkAddress,
                BacklinkCaptured = backlinkCaptured,
                BacklinkValue = backlinkValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                Reads = reads.ToImmutable(),
                Events = events.ToImmutable(),
                UnreachedFreshFrameMinus14CaptureProvided =
                    !frameCaptured && capture.FreshFrameMinus14.HasValue,
                UnreachedEntryEDXCaptureProvided =
                    !edxCaptured && capture.EntryEDX.HasValue,
                UnreachedFreshBacklinkCaptureProvided =
                    !backlinkCaptured && capture.FreshBacklink.HasValue,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            reads.Add(new(
                FrameMinus14ReadRawFileOffset,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null));
            events.Add(new(
                CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read,
                FrameMinus14ReadRawFileOffset));
            return Finish(
                CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEBP,
                FrameMinus14ReadRawFileOffset);
        }

        ebpCaptured = true;
        frameAddress = DeriveFrameMinus14Address(entryEbp);
        reads.Add(new(
            FrameMinus14ReadRawFileOffset,
            frameAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshFrameMinus14.HasValue,
            Value: capture.FreshFrameMinus14));
        events.Add(new(
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read,
            FrameMinus14ReadRawFileOffset,
            frameAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshFrameMinus14.HasValue,
            Value: capture.FreshFrameMinus14));
        if (capture.FreshFrameMinus14 is not uint freshFrameMinus14)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshFrameMinus14,
                FrameMinus14ReadRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshFrameMinus14;

        edxCaptured = capture.EntryEDX.HasValue;
        backlinkReadReached = true;
        backlinkAddress = DeriveBacklinkAddress(freshFrameMinus14);
        reads.Add(new(
            BacklinkReadRawFileOffset,
            backlinkAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshBacklink.HasValue,
            Value: capture.FreshBacklink));
        events.Add(new(
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Read,
            BacklinkReadRawFileOffset,
            backlinkAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshBacklink.HasValue,
            Value: capture.FreshBacklink));
        if (capture.FreshBacklink is uint capturedBacklink)
        {
            backlinkCaptured = true;
            backlinkValue = capturedBacklink;
        }

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEDX,
                CompareRawFileOffset);
        }

        if (capture.FreshBacklink is not uint freshBacklink)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingFreshBacklink,
                CompareRawFileOffset);
        }

        backlinkCaptured = true;
        backlinkValue = freshBacklink;
        compareReached = true;
        compareLeft = freshBacklink;
        compareRight = entryEdx;
        compareResult = unchecked(freshBacklink - entryEdx);
        compareFlags = CalculateCompareFlags(
            freshBacklink, entryEdx, compareResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Compare,
            CompareRawFileOffset,
            Left: compareLeft,
            Right: compareRight,
            Result: compareResult,
            Flags: compareFlags));

        jnzReached = true;
        jnzTaken = !compareFlags.Value.Zero;
        jnzDestination = jnzTaken.Value
            ? NonzeroDestinationRawFileOffset
            : ZeroDestinationRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeFrameBacklinkGateEventKind.Branch,
            JnzRawFileOffset,
            BranchTaken: jnzTaken,
            SelectedDestinationRawFileOffset: jnzDestination));

        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeLargeFrameBacklinkGateOutcome.UnequalToTarget
                : CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough,
            jnzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeFrameBacklinkGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeLargeFrameBacklinkGateInstructionEvidence Instruction(
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
