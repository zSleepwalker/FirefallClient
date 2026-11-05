using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargePostStoreZeroGateCapture(
    uint? EntryEAX);

public enum CapacityFinalCalleeLargePostStoreZeroGateOutcome
{
    MissingEntryEAX,
    ZeroTo92BD,
    NonzeroTo927C
}

public readonly record struct CapacityFinalCalleeLargePostStoreZeroGateFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargePostStoreZeroGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargePostStoreZeroGatePlan(
    CapacityFinalCalleeLargePostStoreZeroGateCapture Capture,
    CapacityFinalCalleeLargePostStoreZeroGateOutcome Outcome)
{
    public bool TestReached { get; init; }
    public bool EntryEAXCaptured { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeLargePostStoreZeroGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeLargePostStoreZeroGateFlags? FlagsAtStop => TestFlags;
    public bool JZReached { get; init; }
    public bool? JZTaken { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public uint? SelectedDestinationVirtualAddress { get; init; }
    public bool DestinationEntered => false;
    public bool ExactBoundedBranchCompletion { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the TEST/JZ zero gate and stops at its selected destination.
/// </summary>
public static class CapacityFinalCalleeLargePostStoreZeroGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9278;
    public const uint EntryFlatIdaAddress = 0x012C9278;
    public const uint EntryVirtualAddress = 0x016C9E78;
    public const uint TestRawFileOffset = 0x012C9278;
    public const uint JZRawFileOffset = 0x012C927A;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C927C;
    public const uint BranchTargetRawFileOffset = 0x012C92BD;
    public const uint FallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const byte JZRelativeDisplacement = 0x41;

    public const string CapturedWindowBytes = "85 C0 74 41";

    public static ImmutableArray<CapacityFinalCalleeLargePostStoreZeroGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Test entry EAX against itself",
                TestRawFileOffset,
                "85 C0"),
            Instruction(
                "Jump to 92BD when TEST found entry EAX zero",
                JZRawFileOffset,
                "74 41",
                BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint CalculateBranchTargetRawFileOffset() =>
        unchecked(BoundaryRawFileOffsetExclusive + JZRelativeDisplacement);

    public static CapacityFinalCalleeLargePostStoreZeroGatePlan Project(
        CapacityFinalCalleeLargePostStoreZeroGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        bool testReached = false;

        CapacityFinalCalleeLargePostStoreZeroGatePlan Finish(
            CapacityFinalCalleeLargePostStoreZeroGateOutcome outcome,
            uint stopAt,
            bool? jzTaken = null,
            uint? destination = null,
            CapacityFinalCalleeLargePostStoreZeroGateFlags? flags = null,
            uint? testResult = null,
            bool exactBoundedBranchCompletion = false) =>
            new(capture, outcome)
            {
                TestReached = testReached,
                EntryEAXCaptured = capture.EntryEAX.HasValue,
                TestResult = testResult,
                TestFlags = flags,
                JZReached = jzTaken.HasValue,
                JZTaken = jzTaken,
                SelectedDestinationRawFileOffset = destination,
                SelectedDestinationVirtualAddress = destination is uint raw
                    ? RawFileOffsetToVirtualAddress(raw)
                    : null,
                ExactBoundedBranchCompletion = exactBoundedBranchCompletion,
                ReachedInstructions = reached.ToImmutable(),
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEAX is not uint entryEAX)
        {
            return Finish(
                CapacityFinalCalleeLargePostStoreZeroGateOutcome.MissingEntryEAX,
                TestRawFileOffset);
        }

        CapacityFinalCalleeLargePostStoreZeroGateFlags flags = TestFlagsFor(entryEAX);
        testReached = true;
        reached.Add(TestRawFileOffset);
        reached.Add(JZRawFileOffset);
        bool jzTaken = flags.Zero;
        uint destination = jzTaken
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;

        return Finish(
            jzTaken
                ? CapacityFinalCalleeLargePostStoreZeroGateOutcome.ZeroTo92BD
                : CapacityFinalCalleeLargePostStoreZeroGateOutcome.NonzeroTo927C,
            destination,
            jzTaken,
            destination,
            flags,
            entryEAX,
            exactBoundedBranchCompletion: true);
    }

    private static CapacityFinalCalleeLargePostStoreZeroGateFlags TestFlagsFor(uint value) =>
        new(
            Carry: false,
            Parity: (BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeLargePostStoreZeroGateInstructionEvidence Instruction(
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
