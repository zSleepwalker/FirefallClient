using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedFrameGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus10);

public enum CapacityFinalCalleeLargeSavedFrameGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus10,
    ZeroToTarget,
    NonzeroToFallthrough
}

public enum CapacityFinalCalleeLargeSavedFrameGateEventKind
{
    Read,
    Test,
    Branch
}

public readonly record struct CapacityFinalCalleeLargeSavedFrameGateFlags(
    bool Carry,
    bool Parity,
    bool? Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedFrameGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedFrameGateEvent(
    CapacityFinalCalleeLargeSavedFrameGateEventKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    bool AddressCaptured = false,
    bool ValueCaptured = false,
    uint? Value = null,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeSavedFrameGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public readonly record struct CapacityFinalCalleeLargeSavedFrameGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeSavedFrameGatePlan(
    CapacityFinalCalleeLargeSavedFrameGateCapture Capture,
    CapacityFinalCalleeLargeSavedFrameGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FreshReadReached { get; init; }
    public uint? FreshFrameMinus10Address { get; init; }
    public bool FreshFrameMinus10Captured { get; init; }
    public uint? FreshFrameMinus10 { get; init; }
    public bool TestReached { get; init; }
    public uint? TestOperand { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeLargeSavedFrameGateFlags? TestFlags { get; init; }
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedFrameGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedFrameGateEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedFrameGateEvent>.Empty;
    public bool UnreachedFreshFrameMinus10CaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the single fresh EBP-10 DWORD load and its TEST/JZ gate.
/// The loaded value is independent evidence, not an inferred prior store.
/// </summary>
public static class CapacityFinalCalleeLargeSavedFrameGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9226;
    public const uint EntryVirtualAddress = 0x016C9E26;
    public const uint FreshReadRawFileOffset = 0x012C9226;
    public const uint TestEcxRawFileOffset = 0x012C9229;
    public const uint JzRawFileOffset = 0x012C922B;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9231;
    public const uint ZeroDestinationRawFileOffset = 0x012C92BD;
    public const uint NonzeroDestinationRawFileOffset = BoundaryRawFileOffsetExclusive;

    public const uint FrameMinus10Displacement = 0x10;

    public const string CapturedWindowBytes =
        "8B 4D F0 85 C9 0F 84 8C 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeLargeSavedFrameGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-10 into ECX",
                FreshReadRawFileOffset, "8B 4D F0"),
            Instruction("Test ECX against itself",
                TestEcxRawFileOffset, "85 C9"),
            Instruction("Jump to 92BD when ECX is zero",
                JzRawFileOffset, "0F 84 8C 00 00 00",
                branchTargetRawFileOffset: ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus10Address(uint entryEBP) =>
        unchecked(entryEBP - FrameMinus10Displacement);

    public static CapacityFinalCalleeLargeSavedFrameGatePlan Project(
        CapacityFinalCalleeLargeSavedFrameGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedFrameGateRead>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedFrameGateEvent>();
        bool entryEBPCaptured = false;
        bool freshReadReached = true;
        uint? freshAddress = null;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool testReached = false;
        uint? testOperand = null;
        uint? testResult = null;
        CapacityFinalCalleeLargeSavedFrameGateFlags? testFlags = null;
        bool jzReached = false;
        bool? jzTaken = null;
        uint? jzDestination = null;

        CapacityFinalCalleeLargeSavedFrameGatePlan Finish(
            CapacityFinalCalleeLargeSavedFrameGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = entryEBPCaptured,
                FreshReadReached = freshReadReached,
                FreshFrameMinus10Address = freshAddress,
                FreshFrameMinus10Captured = freshCaptured,
                FreshFrameMinus10 = freshValue,
                TestReached = testReached,
                TestOperand = testOperand,
                TestResult = testResult,
                TestFlags = testFlags,
                JzReached = jzReached,
                JzTaken = jzTaken,
                JzSelectedDestinationRawFileOffset = jzDestination,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                Events = events.ToImmutable(),
                UnreachedFreshFrameMinus10CaptureProvided =
                    !freshCaptured && capture.FreshFrameMinus10.HasValue,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        reached.Add(FreshReadRawFileOffset);
        if (capture.EntryEBP is not uint entryEBP)
        {
            reads.Add(new(
                FreshReadRawFileOffset,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null));
            events.Add(new(
                CapacityFinalCalleeLargeSavedFrameGateEventKind.Read,
                FreshReadRawFileOffset));
            return Finish(
                CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingEntryEBP,
                FreshReadRawFileOffset);
        }

        entryEBPCaptured = true;
        freshAddress = DeriveFrameMinus10Address(entryEBP);
        reads.Add(new(
            FreshReadRawFileOffset,
            freshAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshFrameMinus10.HasValue,
            Value: capture.FreshFrameMinus10));
        events.Add(new(
            CapacityFinalCalleeLargeSavedFrameGateEventKind.Read,
            FreshReadRawFileOffset,
            freshAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshFrameMinus10.HasValue,
            Value: capture.FreshFrameMinus10));
        if (capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingFreshFrameMinus10,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = freshFrameMinus10;
        reached.Add(TestEcxRawFileOffset);
        testReached = true;
        testOperand = freshFrameMinus10;
        testResult = freshFrameMinus10 & freshFrameMinus10;
        testFlags = CalculateTestFlags(testResult.Value);
        events.Add(new(
            CapacityFinalCalleeLargeSavedFrameGateEventKind.Test,
            TestEcxRawFileOffset,
            Left: freshFrameMinus10,
            Right: freshFrameMinus10,
            Result: testResult,
            Flags: testFlags));

        reached.Add(JzRawFileOffset);
        jzReached = true;
        jzTaken = testFlags.Value.Zero;
        jzDestination = jzTaken.Value
            ? ZeroDestinationRawFileOffset
            : NonzeroDestinationRawFileOffset;
        events.Add(new(
            CapacityFinalCalleeLargeSavedFrameGateEventKind.Branch,
            JzRawFileOffset,
            BranchTaken: jzTaken,
            SelectedDestinationRawFileOffset: jzDestination));

        return Finish(
            jzTaken.Value
                ? CapacityFinalCalleeLargeSavedFrameGateOutcome.ZeroToTarget
                : CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough,
            jzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeSavedFrameGateFlags CalculateTestFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeLargeSavedFrameGateInstructionEvidence Instruction(
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
