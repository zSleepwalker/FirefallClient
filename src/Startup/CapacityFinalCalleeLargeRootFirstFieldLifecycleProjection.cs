using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRootFirstFieldLifecycleCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEdxPlusC,
    uint? FreshEdxPlus18,
    uint? FreshEdxPlus14);

public enum CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
{
    MissingRootEntryEDX,
    MissingRootFreshEdxPlusC,
    MissingRootFreshEdxPlus18,
    MissingRootEntryEBPForStore,
    RootUnequalToBoundedReject,
    RootUnequalWithRejectedFirstFieldCapture,
    MissingFirstFieldFreshEdxPlus14,
    FirstFieldZeroToCleanFallthrough,
    FirstFieldNonzeroToCleanTarget
}

public enum CapacityFinalCalleeLargeRootFirstFieldLifecycleStage
{
    RootGate,
    FirstFieldTest
}

public readonly record struct CapacityFinalCalleeLargeRootFirstFieldLifecycleWrite(
    CapacityFinalCalleeLargeRootFirstFieldLifecycleStage Stage,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeRootFirstFieldLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan(
    CapacityFinalCalleeLargeRootFirstFieldLifecycleCapture Capture,
    CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeLargeFrameRootGatePlan RootGate { get; init; } = null!;
    public CapacityFinalCalleeLargeFrameFirstFieldTestPlan? FirstFieldTest { get; init; }
    public bool FirstFieldTestReached { get; init; }
    public bool FirstFieldEntryEDXRetained { get; init; }
    public uint? FirstFieldEntryEDX { get; init; }
    public bool FirstFieldEntryEBPRetained { get; init; }
    public uint? FirstFieldEntryEBP { get; init; }
    public CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags? FlagsAtStop { get; init; }
    public bool UnreachedFirstFieldFreshEdxPlus14CaptureProvided { get; init; }
    public bool RejectedFirstFieldCaptures { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeRootFirstFieldLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRootFirstFieldLifecycleWrite>.Empty;
    public bool RootCleanExitAdmitted { get; init; }
    public bool FirstFieldCleanExitAdmitted { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the clean equal root-gate route at 91D6 into an independent fresh
/// EDX+14 field load and its TEST/JNZ gate.
/// </summary>
public static class CapacityFinalCalleeLargeRootFirstFieldLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C919B;
    public const uint EntryVirtualAddress = 0x016C9D9B;
    public const uint RootUnequalDestinationRawFileOffset = 0x012C91A8;
    public const uint RootEqualDestinationRawFileOffset = 0x012C91D6;
    public const uint FirstFieldZeroDestinationRawFileOffset = 0x012C91E3;
    public const uint FirstFieldNonzeroDestinationRawFileOffset = 0x012C91F0;

    public const string CapturedWindowBytes =
        "8B 42 0C 8B 4A 18 89 4D F0 3B C2 74 2E "
        + "8B 42 14 8D 4A 14 89 4D FC 85 C0 75 0D";

    public static ImmutableArray<CapacityFinalCalleeLargeRootFirstFieldLifecycleInstructionEvidence>
        Instructions { get; } = BuildInstructions();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan Project(
        CapacityFinalCalleeLargeRootFirstFieldLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeFrameRootGatePlan root =
            CapacityFinalCalleeLargeFrameRootGateProjection.Project(new(
                capture.EntryEDX,
                capture.EntryEBP,
                capture.FreshEdxPlusC,
                capture.FreshEdxPlus18));

        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRootFirstFieldLifecycleWrite>();
        foreach (CapacityFinalCalleeLargeFrameRootGateWrite write in root.Writes)
        {
            writes.Add(new(
                CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.RootGate,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.ValueCaptured,
                write.Value));
        }

        bool lateFirstFieldCapture = capture.FreshEdxPlus14.HasValue;
        CapacityFinalCalleeLargeFrameFirstFieldTestPlan? firstField = null;
        bool firstFieldReached = false;
        bool firstFieldEBPRetained = false;
        CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags? flagsAtStop =
            root.CompareFlags is { } initialRootFlags ? Convert(initialRootFlags) : null;
        bool firstFieldCleanExitAdmitted = false;
        bool? branchTaken = root.BranchTaken;
        uint? branchDestination = root.BranchSelectedDestinationRawFileOffset;

        CapacityFinalCalleeLargeRootFirstFieldLifecyclePlan Finish(
            CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome outcome,
            uint stopAt,
            bool rootCleanExitAdmitted = false,
            bool exactCleanExitAdmitted = false,
            bool rejectedFirstFieldCaptures = false) =>
            new(capture, outcome)
            {
                RootGate = root,
                FirstFieldTest = firstField,
                FirstFieldTestReached = firstFieldReached,
                FirstFieldEntryEDXRetained = firstFieldReached,
                FirstFieldEntryEDX = firstFieldReached ? capture.EntryEDX : null,
                FirstFieldEntryEBPRetained = firstFieldEBPRetained,
                FirstFieldEntryEBP = firstFieldEBPRetained ? capture.EntryEBP : null,
                FlagsAtStop = flagsAtStop,
                UnreachedFirstFieldFreshEdxPlus14CaptureProvided = lateFirstFieldCapture,
                RejectedFirstFieldCaptures = rejectedFirstFieldCaptures,
                Writes = writes.ToImmutable(),
                RootCleanExitAdmitted = rootCleanExitAdmitted,
                FirstFieldCleanExitAdmitted = firstFieldCleanExitAdmitted,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                BranchReached = branchDestination.HasValue,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                StopAtRawFileOffsetExclusive = stopAt
            };

        switch (root.Outcome)
        {
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEDX:
                return Finish(
                    CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome.MissingRootEntryEDX,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlusC:
                return Finish(
                    CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome.MissingRootFreshEdxPlusC,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingFreshEdxPlus18:
                return Finish(
                    CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome.MissingRootFreshEdxPlus18,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.MissingEntryEBPForStore:
                return Finish(
                    CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome.MissingRootEntryEBPForStore,
                    root.StopAtRawFileOffsetExclusive);
            case CapacityFinalCalleeLargeFrameRootGateOutcome.UnequalToFallthrough:
            {
                bool exactUnequalRoute =
                    IsExactRootRoute(root, false, RootUnequalDestinationRawFileOffset);
                bool rejectedCapture = lateFirstFieldCapture;
                bool exactRootExit = exactUnequalRoute && !rejectedCapture;
                return Finish(
                    rejectedCapture
                        ? CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                            .RootUnequalWithRejectedFirstFieldCapture
                        : CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                            .RootUnequalToBoundedReject,
                    exactUnequalRoute
                        ? RootUnequalDestinationRawFileOffset
                        : root.StopAtRawFileOffsetExclusive,
                    rootCleanExitAdmitted: exactRootExit,
                    exactCleanExitAdmitted: exactRootExit,
                    rejectedFirstFieldCaptures: rejectedCapture);
            }
            case CapacityFinalCalleeLargeFrameRootGateOutcome.EqualToTarget:
                if (!IsExactRootRoute(root, true, RootEqualDestinationRawFileOffset))
                {
                    return Finish(
                        CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                            .RootUnequalToBoundedReject,
                        root.StopAtRawFileOffsetExclusive);
                }

                firstFieldReached = true;
                lateFirstFieldCapture = false;
                CapacityFinalCalleeLargeFrameFirstFieldTestPlan projectedFirstField =
                    CapacityFinalCalleeLargeFrameFirstFieldTestProjection.Project(new(
                        EntryEDX: capture.EntryEDX,
                        EntryEBP: capture.FreshEdxPlus14.HasValue ? capture.EntryEBP : null,
                        FreshEAX: capture.FreshEdxPlus14));
                firstField = projectedFirstField;
                firstFieldEBPRetained = projectedFirstField.StoreReached;
                flagsAtStop = projectedFirstField.TestEAXFlags is { } childFlags
                    ? Convert(childFlags)
                    : root.CompareFlags is { } rootFlags
                        ? Convert(rootFlags)
                        : null;
                foreach (CapacityFinalCalleeLargeFrameFirstFieldTestWrite write
                    in projectedFirstField.Writes)
                {
                    writes.Add(new(
                        CapacityFinalCalleeLargeRootFirstFieldLifecycleStage.FirstFieldTest,
                        write.RawFileOffset,
                        write.Address,
                        write.Address.HasValue,
                        write.Value.HasValue,
                        write.Value));
                }

                branchTaken = projectedFirstField.NonzeroBranchTaken ?? root.BranchTaken;
                branchDestination =
                    projectedFirstField.NonzeroBranchSelectedDestinationRawFileOffset
                    ?? root.BranchSelectedDestinationRawFileOffset;
                firstFieldCleanExitAdmitted = projectedFirstField.ExactCleanExitAdmitted;
                CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome outcome =
                    projectedFirstField.Outcome switch
                    {
                        CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingFreshEAX =>
                            CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                                .MissingFirstFieldFreshEdxPlus14,
                        CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXZeroToFallthrough =>
                            CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                                .FirstFieldZeroToCleanFallthrough,
                        CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXNonzeroToTarget =>
                            CapacityFinalCalleeLargeRootFirstFieldLifecycleOutcome
                                .FirstFieldNonzeroToCleanTarget,
                        _ => throw new InvalidOperationException(
                            "The selected first-field route must retain EDX and EBP.")
                    };
                return Finish(
                    outcome,
                    projectedFirstField.StopAtRawFileOffsetExclusive,
                    rootCleanExitAdmitted: true,
                    exactCleanExitAdmitted:
                        root.ExactCleanAdmission && projectedFirstField.ExactCleanExitAdmitted);
            default:
                throw new InvalidOperationException("Unhandled large-frame root-gate outcome.");
        }
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

    private static CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameRootGateCompareFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static CapacityFinalCalleeLargeRootFirstFieldLifecycleFlags Convert(
        CapacityFinalCalleeLargeFrameFirstFieldTestFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);

    private static ImmutableArray<
        CapacityFinalCalleeLargeRootFirstFieldLifecycleInstructionEvidence> BuildInstructions()
    {
        var instructions =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRootFirstFieldLifecycleInstructionEvidence>();
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

        return instructions.ToImmutable();
    }
}
