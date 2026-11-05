using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRootPairedFieldLifecycleCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEdxPlusC,
    uint? FreshEdxPlus18,
    uint? FirstFreshEdxPlus14,
    uint? SecondFreshEdxPlus10);

public enum CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
{
    MissingRootEntryEDX,
    MissingRootFreshEdxPlusC,
    MissingRootFreshEdxPlus18,
    MissingRootEntryEBPForStore,
    RootUnequalToBoundedReject,
    RootUnequalWithRejectedChildCaptures,
    MissingFirstChildFreshEAX,
    MissingFirstChildEntryEBPForStore,
    FirstChildNonzeroToTarget,
    MissingSecondChildFreshEAX,
    MissingSecondChildEntryEBPForStore,
    SecondChildZeroToTarget,
    SecondChildNonzeroToFallthrough
}

public enum CapacityFinalCalleeLargeRootPairedFieldLifecycleStage
{
    RootGate,
    FirstChild,
    SecondChild
}

public readonly record struct CapacityFinalCalleeLargeRootPairedFieldLifecycleRead(
    CapacityFinalCalleeLargeRootPairedFieldLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeRootPairedFieldLifecycleWrite(
    CapacityFinalCalleeLargeRootPairedFieldLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeRootPairedFieldLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeRootPairedFieldLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan(
    CapacityFinalCalleeLargeRootPairedFieldLifecycleCapture Capture,
    CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameRootGatePlan RootGate { get; init; } = null!;
    public CapacityFinalCalleeLargePairedFieldTestLifecyclePlan? PairedFieldTestLifecycle
    {
        get; init;
    }
    public CapacityFinalCalleeLargeFrameFirstFieldTestPlan? FirstChildTest { get; init; }
    public CapacityFinalCalleeLargeFrameSecondFieldTestPlan? SecondChildTest { get; init; }
    public bool FirstChildTestReached { get; init; }
    public bool SecondChildTestReached { get; init; }
    public bool FirstChildEntryEDXRetained { get; init; }
    public uint? FirstChildEntryEDX { get; init; }
    public bool FirstChildEntryEBPRetained { get; init; }
    public uint? FirstChildEntryEBP { get; init; }
    public bool SecondChildEntryEDXRetained { get; init; }
    public uint? SecondChildEntryEDX { get; init; }
    public bool SecondChildEntryEBPRetained { get; init; }
    public uint? SecondChildEntryEBP { get; init; }
    public bool UnreachedFirstChildFreshEAXCaptureProvided { get; init; }
    public bool UnreachedSecondChildFreshEAXCaptureProvided { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRootPairedFieldLifecycleRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootPairedFieldLifecycleRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRootPairedFieldLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootPairedFieldLifecycleWrite>.Empty;
    public CapacityFinalCalleeLargeRootPairedFieldLifecycleFlags? FlagsAtStop { get; init; }
    public bool RootCleanExitAdmitted { get; init; }
    public bool FirstChildCleanExitAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the clean equal root gate at 91D6 with two independent fresh child
/// fields, preserving the root and repeated EBP-relative writes in execution order.
/// </summary>
public static class CapacityFinalCalleeLargeRootPairedFieldLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeFrameRootGateProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        CapacityFinalCalleeLargeFrameRootGateProjection.EntryVirtualAddress;
    public const uint RootUnequalDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameRootGateProjection.UnequalDestinationRawFileOffset;
    public const uint RootEqualDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameRootGateProjection.EqualDestinationRawFileOffset;
    public const uint FirstChildNonzeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.NonzeroDestinationRawFileOffset;
    public const uint SecondChildZeroDestinationRawFileOffset =
        CapacityFinalCalleeLargeFrameSecondFieldTestProjection.ZeroDestinationRawFileOffset;

    public const string CapturedWindowBytes =
        CapacityFinalCalleeLargeFrameRootGateProjection.CapturedWindowBytes
        + " "
        + CapacityFinalCalleeLargeFrameFirstFieldTestProjection.CapturedWindowBytes
        + " "
        + CapacityFinalCalleeLargeFrameSecondFieldTestProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeLargeRootPairedFieldLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        CapacityFinalCalleeLargeFrameRootGateProjection
            .RawFileOffsetToVirtualAddress(rawFileOffset);

    public static uint DeriveFreshEdxPlusCAddress(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEdxPlusCAddress(entryEDX);

    public static uint DeriveFreshEdxPlus18Address(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEdxPlus18Address(entryEDX);

    public static uint DeriveFirstFreshEdxPlus14Address(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection
            .DeriveFreshEDXPlus14Address(entryEDX);

    public static uint DeriveSecondFreshEdxPlus10Address(uint entryEDX) =>
        CapacityFinalCalleeLargeFrameSecondFieldTestProjection
            .DeriveFreshEDXPlus10Address(entryEDX);

    public static uint DeriveRootStoreAddress(uint entryEBP) =>
        CapacityFinalCalleeLargeFrameRootGateProjection.DeriveEbpMinus10Address(entryEBP);

    public static uint DeriveChildStoreAddress(uint entryEBP) =>
        CapacityFinalCalleeLargeFrameFirstFieldTestProjection.DeriveStoreAddress(entryEBP);

    public static CapacityFinalCalleeLargeRootPairedFieldLifecyclePlan Project(
        CapacityFinalCalleeLargeRootPairedFieldLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameRootGatePlan root =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(new(
                capture.EntryEDX,
                capture.EntryEBP,
                capture.FreshEdxPlusC,
                capture.FreshEdxPlus18));

        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootPairedFieldLifecycleRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootPairedFieldLifecycleWrite>();

        foreach (CapacityFinalCalleeLargeFrameRootGateRead read in root.Reads)
        {
            reads.Add(new(
                CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.RootGate,
                read.RawFileOffset,
                read.Address,
                read.ValueCaptured,
                read.Value));
            reached.Add(read.RawFileOffset);
        }

        foreach (CapacityFinalCalleeLargeFrameRootGateWrite write in root.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.RootGate,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        if (root.StoreReached)
        {
            reached.Add(CapacityFinalCalleeLargeFrameRootGateProjection.StoreRawFileOffset);
        }

        if (root.CompareReached)
        {
            reached.Add(CapacityFinalCalleeLargeFrameRootGateProjection.CompareRawFileOffset);
        }

        if (root.BranchReached)
        {
            reached.Add(CapacityFinalCalleeLargeFrameRootGateProjection.BranchRawFileOffset);
        }

        CapacityFinalCalleeLargePairedFieldTestLifecyclePlan? paired = null;
        CapacityFinalCalleeLargeFrameFirstFieldTestPlan? first = null;
        CapacityFinalCalleeLargeFrameSecondFieldTestPlan? second = null;
        bool rootCleanExit = false;
        bool firstCleanExit = false;
        bool exactCleanExit = false;
        bool firstReached = false;
        bool unreachedFirst = capture.FirstFreshEdxPlus14.HasValue;
        bool unreachedSecond = capture.SecondFreshEdxPlus10.HasValue;
        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome outcome;
        uint stopAt;

        switch (root.Outcome)
        {
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX:
                outcome = CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootEntryEDX;
                stopAt = root.StopAtRawFileOffsetExclusive;
                break;
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlusC:
                outcome = CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootFreshEdxPlusC;
                stopAt = root.StopAtRawFileOffsetExclusive;
                break;
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlus18:
                outcome = CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootFreshEdxPlus18;
                stopAt = root.StopAtRawFileOffsetExclusive;
                break;
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEBPForStore:
                outcome = CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                    .MissingRootEntryEBPForStore;
                stopAt = root.StopAtRawFileOffsetExclusive;
                break;
            case CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough:
            {
                bool exactRootRoute = IsExactRootRoute(
                    root, taken: false, RootUnequalDestinationRawFileOffset);
                bool rejectedCaptures = unreachedFirst || unreachedSecond;
                outcome = rejectedCaptures
                    ? CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                        .RootUnequalWithRejectedChildCaptures
                    : CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                        .RootUnequalToBoundedReject;
                stopAt = exactRootRoute
                    ? RootUnequalDestinationRawFileOffset
                    : root.StopAtRawFileOffsetExclusive;
                rootCleanExit = exactRootRoute && !rejectedCaptures;
                exactCleanExit = rootCleanExit;
                break;
            }
            case CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget:
            {
                if (!IsExactRootRoute(root, taken: true, RootEqualDestinationRawFileOffset))
                {
                    outcome = CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                        .RootUnequalToBoundedReject;
                    stopAt = root.StopAtRawFileOffsetExclusive;
                    break;
                }

                rootCleanExit = true;
                firstReached = true;
                unreachedFirst = false;
                paired = CapacityFinalCalleeLargePairedFieldTestLifecycleProjection.Project(new(
                    EntryEDX: capture.EntryEDX,
                    EntryEBP: capture.FirstFreshEdxPlus14.HasValue ? capture.EntryEBP : null,
                    FirstFreshEAX: capture.FirstFreshEdxPlus14,
                    SecondFreshEAX: capture.SecondFreshEdxPlus10));
                first = paired.FirstFieldTest;
                second = paired.SecondFieldTest;
                unreachedSecond = paired.UnreachedSecondFreshEAXCaptureProvided;

                reached.AddRange(paired.ReachedInstructions);
                foreach (CapacityFinalCalleeLargePairedFieldTestLifecycleRead read
                    in paired.Reads)
                {
                    reads.Add(new(
                        read.Stage == CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField
                            ? CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.FirstChild
                            : CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.SecondChild,
                        read.RawFileOffset,
                        read.Address,
                        read.ValueCaptured,
                        read.Value));
                }

                foreach (CapacityFinalCalleeLargePairedFieldTestLifecycleWrite write
                    in paired.Writes)
                {
                    writes.Add(new(
                        write.Stage == CapacityFinalCalleeLargePairedFieldTestLifecycleStage.FirstField
                            ? CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.FirstChild
                            : CapacityFinalCalleeLargeRootPairedFieldLifecycleStage.SecondChild,
                        write.RawFileOffset,
                        write.Address,
                        write.Address.HasValue,
                        write.Value.HasValue,
                        write.Value));
                }

                outcome = paired.Outcome switch
                {
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingFirstFreshEAX =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .MissingFirstChildFreshEAX,
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingEntryEBPForFirstStore =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .MissingFirstChildEntryEBPForStore,
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.FirstFreshEAXNonzeroToTarget =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .FirstChildNonzeroToTarget,
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.MissingSecondFreshEAX =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .MissingSecondChildFreshEAX,
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.SecondFreshEAXZeroToTarget =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .SecondChildZeroToTarget,
                    CapacityFinalCalleeLargePairedFieldTestLifecycleOutcome.SecondFreshEAXNonzeroToFallthrough =>
                        CapacityFinalCalleeLargeRootPairedFieldLifecycleOutcome
                            .SecondChildNonzeroToFallthrough,
                    _ => throw new InvalidOperationException(
                        "The paired child lifecycle must retain the clean root EDX.")
                };
                firstCleanExit = paired.FirstFieldCleanExitAdmitted;
                exactCleanExit = paired.ExactCleanExitAdmitted;
                stopAt = paired.StopAtRawFileOffsetExclusive;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled large-frame root-gate outcome.");
        }

        CapacityFinalCalleeLargeRootPairedFieldLifecycleFlags? flagsAtStop =
            paired?.FlagsAtStop is { } childFlags
                ? Convert(childFlags)
                : root.CompareFlags is { } rootFlags
                    ? Convert(rootFlags)
                    : null;
        bool branchReached =
            (second?.ZeroBranchReached ?? false)
            || (first?.NonzeroBranchReached ?? false)
            || root.BranchReached;
        bool? branchTaken =
            second?.ZeroBranchTaken
            ?? first?.NonzeroBranchTaken
            ?? root.BranchTaken;
        uint? branchDestination =
            second?.ZeroBranchSelectedDestinationRawFileOffset
            ?? first?.NonzeroBranchSelectedDestinationRawFileOffset
            ?? root.BranchSelectedDestinationRawFileOffset;

        return new(capture, outcome)
        {
            RootGate = root,
            PairedFieldTestLifecycle = paired,
            FirstChildTest = first,
            SecondChildTest = second,
            FirstChildTestReached = firstReached,
            SecondChildTestReached = paired?.SecondFieldTestReached ?? false,
            FirstChildEntryEDXRetained = firstReached && root.EntryEDXCaptured,
            FirstChildEntryEDX = firstReached && root.EntryEDXCaptured ? capture.EntryEDX : null,
            FirstChildEntryEBPRetained =
                first is { StoreReached: true, EntryEBPCaptured: true },
            FirstChildEntryEBP =
                first is { StoreReached: true, EntryEBPCaptured: true } ? capture.EntryEBP : null,
            SecondChildEntryEDXRetained = paired?.SecondFieldEntryEDXRetained ?? false,
            SecondChildEntryEDX = paired?.SecondFieldEntryEDX,
            SecondChildEntryEBPRetained = paired?.SecondFieldEntryEBPRetained ?? false,
            SecondChildEntryEBP = paired?.SecondFieldEntryEBP,
            UnreachedFirstChildFreshEAXCaptureProvided = unreachedFirst,
            UnreachedSecondChildFreshEAXCaptureProvided = unreachedSecond,
            ReachedInstructions = reached.ToImmutable(),
            Reads = reads.ToImmutable(),
            Writes = writes.ToImmutable(),
            FlagsAtStop = flagsAtStop,
            RootCleanExitAdmitted = rootCleanExit,
            FirstChildCleanExitAdmitted = firstCleanExit,
            ExactCleanExitAdmitted = exactCleanExit,
            BranchReached = branchReached,
            BranchTaken = branchTaken,
            BranchSelectedDestinationRawFileOffset = branchDestination,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static bool IsExactRootRoute(
        CapacityFinalCalleeLargeFrameRootGatePlan root,
        bool taken,
        uint destination) =>
        root.ExactCleanAdmission
        && root.BranchReached
        && root.BranchTaken == taken
        && root.BranchSelectedDestinationRawFileOffset == destination
        && root.StopAtRawFileOffsetExclusive == destination;

    private static CapacityFinalCalleeLargeRootPairedFieldLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameRootGateCompareFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeLargeRootPairedFieldLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameSecondFieldTestFlags flags) =>
        new(flags.Carry, flags.Parity, flags.AuxiliaryCarry, flags.Zero, flags.Sign, flags.Overflow);

    private static ImmutableArray<
        CapacityFinalCalleeLargeRootPairedFieldLifecycleInstructionEvidence> BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRootPairedFieldLifecycleInstructionEvidence>();
        foreach (CapacityFinalCalleeLargeFrameRootGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeFrameRootGateProjection.Instructions)
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
