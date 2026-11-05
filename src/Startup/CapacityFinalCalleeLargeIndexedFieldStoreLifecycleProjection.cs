using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreLifecycleCapture(
    CapacityFinalCalleeLargeIndexedFieldGateCapture ParentCapture,
    uint? EntryEAX);

public enum CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
{
    ParentGateIncomplete,
    MissingEntryEAXForEqualStore,
    MissingEntryEAXForUnequalStore,
    EqualStoreToContinuation,
    UnequalStoreToContinuation
}

public enum CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind
{
    Compare,
    ConditionalBranch,
    Store,
    UnconditionalJump
}

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEvent(
    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Left = null,
    uint? Right = null,
    uint? Result = null,
    CapacityFinalCalleeLargeIndexedFieldGateFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null,
    uint? Address = null,
    uint? Value = null,
    bool? Committed = null);

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreLifecycleWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeIndexedFieldStoreLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan(
    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleCapture Capture,
    CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome Outcome,
    CapacityFinalCalleeLargeIndexedFieldGatePlan ParentGate)
{
    public CapacityFinalCalleeLargeIndexedFieldGateOutcome ParentGateOutcome =>
        ParentGate.Outcome;
    public bool ParentGateExactCompletion => ParentGate.ExactCleanCompletion;
    public bool ChildBaseCaptured { get; init; }
    public uint? ChildBase { get; init; }
    public bool StoreInstructionReached { get; init; }
    public uint? StoreFieldOffset { get; init; }
    public uint? StoreAddress { get; init; }
    public bool StoreValueCaptured { get; init; }
    public uint? StoreValue { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool FinalJumpReached { get; init; }
    public uint? FinalJumpDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeLargeIndexedFieldGateFlags? FlagsAtStop =>
        ParentGate.FlagsAtStop;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldGateRead> Reads =>
        ParentGate.Reads;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleWrite> Writes
    {
        get; init;
    } = ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleWrite>.Empty;
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEvent> Events
    {
        get; init;
    } = ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEvent>.Empty;
    public bool ExactCleanCompletion { get; init; }
    public bool ContinuationEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the exact-equal and unequal indexed-field stores through, but not into, 9278.
/// </summary>
public static class CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C926B;
    public const uint EntryVirtualAddress = 0x016C9E6B;
    public const uint EqualStoreRawFileOffset = 0x012C9270;
    public const uint EqualStoreFieldOffset = 0x10;
    public const uint EqualPathJumpRawFileOffset = 0x012C9273;
    public const uint UnequalStoreRawFileOffset = 0x012C9275;
    public const uint UnequalStoreFieldOffset = 0x14;
    public const uint ContinuationBoundaryRawFileOffsetExclusive = 0x012C9278;

    public const string CapturedWindowBytes =
        "39 51 10 75 05 89 41 10 EB 03 89 41 14";

    public static ImmutableArray<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Compare the fresh DWORD at entry ECX+10 with entry EDX",
                CapacityFinalCalleeLargeIndexedFieldGateProjection.CompareRawFileOffset,
                "39 51 10"),
            Instruction(
                "Branch to the unequal-field store at 9275",
                CapacityFinalCalleeLargeIndexedFieldGateProjection.JnzRawFileOffset,
                "75 05",
                CapacityFinalCalleeLargeIndexedFieldGateProjection.JnzDestinationRawFileOffset),
            Instruction(
                "On equality, store entry EAX at entry ECX+10",
                EqualStoreRawFileOffset,
                "89 41 10"),
            Instruction(
                "After the equal-field store, jump to the exclusive 9278 boundary",
                EqualPathJumpRawFileOffset,
                "EB 03",
                ContinuationBoundaryRawFileOffsetExclusive),
            Instruction(
                "On inequality, store entry EAX at entry ECX+14",
                UnequalStoreRawFileOffset,
                "89 41 14"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan Project(
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeIndexedFieldGatePlan parent =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(capture.ParentCapture);
        var reached = ImmutableArray.CreateBuilder<uint>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEvent>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeIndexedFieldStoreLifecycleWrite>();
        reached.AddRange(parent.ReachedInstructions);
        foreach (CapacityFinalCalleeLargeIndexedFieldGateEvent parentEvent in parent.Events)
        {
            events.Add(new(
                parentEvent.Kind
                    == CapacityFinalCalleeLargeIndexedFieldGateEventKind.Compare
                    ? CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Compare
                    : CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind
                        .ConditionalBranch,
                parentEvent.RawFileOffset,
                parentEvent.Left,
                parentEvent.Right,
                parentEvent.Result,
                parentEvent.Flags,
                parentEvent.BranchTaken,
                parentEvent.SelectedDestinationRawFileOffset));
        }

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan Finish(
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome outcome,
            bool storeInstructionReached,
            uint? fieldOffset,
            uint? storeAddress,
            bool storeValueCaptured,
            uint? storeValue,
            bool finalJumpReached,
            uint? finalJumpDestination,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome, parent)
            {
                ChildBaseCaptured = storeInstructionReached && parent.EntryECXCaptured,
                ChildBase = storeInstructionReached && parent.EntryECXCaptured
                    ? capture.ParentCapture.EntryECX
                    : null,
                StoreInstructionReached = storeInstructionReached,
                StoreFieldOffset = fieldOffset,
                StoreAddress = storeAddress,
                StoreValueCaptured = storeValueCaptured,
                StoreValue = storeValue,
                UnreachedEntryEAXCaptureProvided =
                    !storeInstructionReached && capture.EntryEAX.HasValue,
                FinalJumpReached = finalJumpReached,
                FinalJumpDestinationRawFileOffset = finalJumpDestination,
                Writes = writes.ToImmutable(),
                ReachedInstructions = reached.ToImmutable(),
                Events = events.ToImmutable(),
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        bool equalPath =
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.EqualToFallthrough9270
            && parent.BranchSelectedDestinationRawFileOffset == EqualStoreRawFileOffset
            && parent.BranchTaken == false;
        bool unequalPath =
            parent.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.UnequalToJnz9275
            && parent.BranchSelectedDestinationRawFileOffset == UnequalStoreRawFileOffset
            && parent.BranchTaken == true;
        if (!parent.ExactCleanCompletion
            || !parent.BranchReached
            || parent.StopAtRawFileOffsetExclusive != parent.BranchSelectedDestinationRawFileOffset
            || parent.UnreachedFreshECXPlus10CaptureProvided
            || parent.FlagsAtStop is null
            || (!equalPath && !unequalPath)
            || capture.ParentCapture.EntryECX is not uint entryECX)
        {
            return Finish(
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome.ParentGateIncomplete,
                storeInstructionReached: false,
                fieldOffset: null,
                storeAddress: null,
                storeValueCaptured: false,
                storeValue: null,
                finalJumpReached: false,
                finalJumpDestination: null,
                stopAt: parent.StopAtRawFileOffsetExclusive);
        }

        uint storeRawFileOffset = equalPath
            ? EqualStoreRawFileOffset
            : UnequalStoreRawFileOffset;
        uint fieldOffset = equalPath
            ? EqualStoreFieldOffset
            : UnequalStoreFieldOffset;
        uint storeAddress = unchecked(entryECX + fieldOffset);
        reached.Add(storeRawFileOffset);
        bool storeValueCaptured = capture.EntryEAX.HasValue;
        uint? storeValue = capture.EntryEAX;
        writes.Add(new(
            storeRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: storeValueCaptured,
            Value: storeValue));
        events.Add(new(
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Store,
            storeRawFileOffset,
            Flags: parent.FlagsAtStop,
            Address: storeAddress,
            Value: storeValue,
            Committed: storeValueCaptured));

        if (!storeValueCaptured)
        {
            return Finish(
                equalPath
                    ? CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                        .MissingEntryEAXForEqualStore
                    : CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                        .MissingEntryEAXForUnequalStore,
                storeInstructionReached: true,
                fieldOffset,
                storeAddress,
                storeValueCaptured: false,
                storeValue: null,
                finalJumpReached: false,
                finalJumpDestination: null,
                stopAt: storeRawFileOffset);
        }

        if (equalPath)
        {
            reached.Add(EqualPathJumpRawFileOffset);
            events.Add(new(
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind
                    .UnconditionalJump,
                EqualPathJumpRawFileOffset,
                Flags: parent.FlagsAtStop,
                BranchTaken: true,
                SelectedDestinationRawFileOffset:
                    ContinuationBoundaryRawFileOffsetExclusive));
        }

        return Finish(
            equalPath
                ? CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .EqualStoreToContinuation
                : CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .UnequalStoreToContinuation,
            storeInstructionReached: true,
            fieldOffset,
            storeAddress,
            storeValueCaptured: true,
            storeValue,
            finalJumpReached: equalPath,
            finalJumpDestination: equalPath
                ? ContinuationBoundaryRawFileOffsetExclusive
                : null,
            stopAt: ContinuationBoundaryRawFileOffsetExclusive,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeIndexedFieldStoreLifecycleInstructionEvidence
        Instruction(
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
