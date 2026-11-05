using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargePairedFieldTestLifecycleCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FirstFreshEAX,
    uint? SecondFreshEAX);

public enum CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome
{
    MissingEntryEDXForFirstRead,
    MissingFirstFreshEAX,
    MissingEntryEBPForFirstStore,
    FirstFreshEAXNonzeroToTarget,
    MissingSecondFreshEAX,
    SecondFreshEAXZeroToTarget,
    SecondFreshEAXNonzeroToFallthrough
}

public enum CapacityFinalCalleeLargePairedFieldTestLifecycleStage
{
    FirstField,
    SecondField
}

public readonly record struct CapacityFinalCalleeLargePairedFieldTestLifecycleRead(
    CapacityFinalCalleeLargePairedFieldTestLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargePairedFieldTestLifecycleWrite(
    CapacityFinalCalleeLargePairedFieldTestLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Completed);

public readonly record struct CapacityFinalCalleeLargePairedFieldTestLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargePairedFieldTestLifecyclePlan(
    CapacityFinalCalleeLargePairedFieldTestLifecycleCapture Capture,
    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameFirstFieldTestPlan FirstFieldTest { get; init; } = null!;
    public CapacityFinalCalleeLargeFrameSecondFieldTestPlan? SecondFieldTest { get; init; }
    public bool SecondFieldTestReached { get; init; }
    public bool SecondFieldEntryEDXRetained { get; init; }
    public uint? SecondFieldEntryEDX { get; init; }
    public bool SecondFieldEntryEBPRetained { get; init; }
    public uint? SecondFieldEntryEBP { get; init; }
    public bool UnreachedSecondFreshEAXCaptureProvided { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargePairedFieldTestLifecycleRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargePairedFieldTestLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargePairedFieldTestLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargePairedFieldTestLifecycleWrite>.Empty;
    public bool FirstFieldCleanExitAdmitted { get; init; }
    public CapacityFinalCalleeLargeFrameSecondFieldTestFlags? FlagsAtStop =>
        SecondFieldTest?.TestEAXFlags
        ?? (FirstFieldTest.TestEAXFlags is { } flags
            ? new(flags.Carry, flags.Parity, flags.Zero,
                flags.Sign, flags.Overflow, flags.AuxiliaryCarry)
            : null);
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the first-field zero fallthrough at 91E3 into the second fresh-field
/// TEST/JZ window, retaining the ordered frame writes and independent reads.
/// </summary>
public static class CapacityFinalCalleeLargePairedFieldTestLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.EntryVirtualAddress;
    public const uint FirstFieldNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.NonzeroDestinationRawFileOffset;
    public const uint SecondFieldZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameSecondFieldTestProjection.ZeroDestinationRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.CapturedWindowBytes
        + " "
        + CapacityFinalCalleeLargeFrameSecondFieldTestProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargePairedFieldTestLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection
            .RawFileOffsetToVirtualAddress(rawFileOffset);

    public static uint DeriveFreshEDXPlus14Address(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection
            .DeriveFreshEDXPlus14Address(entryEDX);

    public static uint DeriveFreshEDXPlus10Address(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameSecondFieldTestProjection
            .DeriveFreshEDXPlus10Address(entryEDX);

    public static uint DeriveStoreAddress(uint entryEBP) =>
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection
            .DeriveStoreAddress(entryEBP);

    public static CapacityFinalCalleeLargePairedFieldTestLifecyclePlan Project(
        CapacityFinalCalleeLargePairedFieldTestLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameFirstFieldTestPlan first =
            CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                capture.EntryEDX,
                capture.EntryEBP,
                capture.FirstFreshEAX));

        var reached =
            ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargePairedFieldTestLifecycleRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargePairedFieldTestLifecycleWrite>();

        reached.AddRange(first.ReachedInstructions);
        foreach (CapacityFinalCalleeLargeFrameFirstFieldTestRead read in first.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField,
                read.RawFileOffset,
                read.Address,
                read.ValueCaptured,
                read.Value));
        }

        foreach (CapacityFinalCalleeLargeFrameFirstFieldTestWrite write in first.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField,
                write.RawFileOffset,
                write.Address,
                write.Value,
                write.Completed));
        }

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan? second = null;
        bool secondReached = false;
        bool secondEDXRetained = false;
        uint? secondEDX = null;
        bool secondEBPRetained = false;
        uint? secondEBP = null;
        bool unusedSecondFreshEAX = capture.SecondFreshEAX.HasValue;
        bool firstChildGate = IsFirstFieldChildGate(first);

        if (firstChildGate)
        {
            secondReached = true;
            secondEDXRetained = first.EntryEDXCaptured;
            secondEDX = secondEDXRetained ? capture.EntryEDX : null;

            second = CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Project(new(
                EntryEDX: secondEDX,
                EntryEBP: capture.SecondFreshEAX.HasValue ? capture.EntryEBP : null,
                FreshEAX: capture.SecondFreshEAX));

            secondEBPRetained =
                second.StoreReached
                && second.FreshEAXCaptured
                && second.EntryEBPCaptured;
            secondEBP = secondEBPRetained ? capture.EntryEBP : null;

            reached.AddRange(second.ReachedInstructions);
            foreach (CapacityFinalCalleeLargeFrameSecondFieldTestRead read in second.Reads)
            {
                reads.Add(new(
                    CapacityFinalCalleeLargePairedFieldTestLifecycleStage.SecondField,
                    read.RawFileOffset,
                    read.Address,
                    read.ValueCaptured,
                    read.Value));
            }

            foreach (CapacityFinalCalleeLargeFrameSecondFieldTestWrite write in second.Writes)
            {
                writes.Add(new(
                    CapacityFinalCalleeLargePairedFieldTestLifecycleStage.SecondField,
                    write.RawFileOffset,
                    write.Address,
                    write.Value,
                    write.Completed));
            }

            unusedSecondFreshEAX = false;
        }

        bool firstCleanExitAdmitted =
            first.ExactCleanExitAdmitted
            && (firstChildGate || !unusedSecondFreshEAX);
        CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome outcome =
            GetOutcome(first, second);
        bool exactCleanExit =
            second is { ExactCleanExitAdmitted: true }
                ? !unusedSecondFreshEAX
                : second is null && firstCleanExitAdmitted;
        uint stopAt = second?.StopAtRawFileOffsetExclusive
            ?? first.StopAtRawFileOffsetExclusive;

        return new(capture, outcome)
        {
            FirstFieldTest = first,
            SecondFieldTest = second,
            SecondFieldTestReached = secondReached,
            SecondFieldEntryEDXRetained = secondEDXRetained,
            SecondFieldEntryEDX = secondEDX,
            SecondFieldEntryEBPRetained = secondEBPRetained,
            SecondFieldEntryEBP = secondEBP,
            UnreachedSecondFreshEAXCaptureProvided = unusedSecondFreshEAX,
            ReachedInstructions = reached.ToImmutable(),
            Reads = reads.ToImmutable(),
            Writes = writes.ToImmutable(),
            FirstFieldCleanExitAdmitted = firstCleanExitAdmitted,
            ExactCleanExitAdmitted = exactCleanExit,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsFirstFieldChildGate(
        CapacityFinalCalleeLargeFrameFirstFieldTestPlan first) =>
        first.ExactCleanExitAdmitted
        && first.Outcome
            == CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXZeroToFallthrough
        && first.NonzeroBranchReached
        && first.NonzeroBranchTaken == false
        && first.NonzeroBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryRawFileOffset
        && first.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeLargeFrameSecondFieldTestProjection.EntryRawFileOffset
        && !first.UnreachedEntryEBPCaptureProvided
        && !first.UnreachedFreshEAXCaptureProvided;

    private static CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome GetOutcome(
        CapacityFinalCalleeLargeFrameFirstFieldTestPlan first,
        CapacityFinalCalleeLargeFrameSecondFieldTestPlan? second)
    {
        if (second is null)
        {
            return first.Outcome switch
            {
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEDXForFreshRead =>
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingEntryEDXForFirstRead,
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingFreshEAX =>
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingFirstFreshEAX,
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEBPForStore =>
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingEntryEBPForFirstStore,
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXNonzeroToTarget =>
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.FirstFreshEAXNonzeroToTarget,
                _ => throw new InvalidOperationException(
                    "The first-field child gate did not match its clean zero branch.")
            };
        }

        return second.Outcome switch
        {
            CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingFreshEAX =>
                CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingSecondFreshEAX,
            CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXZeroToTarget =>
                CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.SecondFreshEAXZeroToTarget,
            CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXNonzeroToFallthrough =>
                CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.SecondFreshEAXNonzeroToFallthrough,
            _ => throw new InvalidOperationException(
                "The second-field read did not retain the parent EDX.")
        };
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargePairedFieldTestLifecycleInstructionEvidence> BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargePairedFieldTestLifecycleInstructionEvidence>();

        foreach (CapacityFinalCalleeLargeFrameFirstFieldTestInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        foreach (CapacityFinalCalleeLargeFrameSecondFieldTestInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameSecondFieldTestProjection.Instructions)
        {
            instructions.Add(new(
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress));
        }

        return instructions.ToImmutable();
    }
}
