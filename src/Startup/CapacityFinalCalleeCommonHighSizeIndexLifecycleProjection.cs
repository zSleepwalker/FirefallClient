using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeIndexLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP);

public enum CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
{
    MissingEntryEDIAt9354,
    ParentZeroAt9384,
    ParentAboveCeilingAt9384,
    MissingLateEntryEBPAt9374,
    IndexedResultAt9384BeforeFollowingStore,
    InvalidParentHandoff,
    InvalidCeilingHandoff,
    InvalidIndexHandoff
}

public enum CapacityFinalCalleeCommonHighSizeIndexLifecycleStage
{
    Entry,
    Ceiling,
    Index
}

public enum CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind
{
    Move,
    ShiftRight,
    Test,
    BranchNotZero,
    Xor,
    Jump,
    Compare,
    BranchBelowOrEqual,
    BitScanReverse,
    MoveEAXToEDX,
    StoreEDXToFrame,
    MoveEDIToEAX,
    LeaShiftCount,
    AndOne,
    LeaIndexedResult
}

public readonly record struct CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonHighSizeIndexLifecycleInstructionEvidence(
    CapacityFinalCalleeCommonHighSizeIndexLifecycleStage Stage,
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent(
    CapacityFinalCalleeCommonHighSizeIndexLifecycleStage Stage,
    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? Immediate = null,
    uint? Result = null,
    uint? Address = null,
    bool? DestinationDefined = null,
    bool? Committed = null,
    CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan(
    CapacityFinalCalleeCommonHighSizeIndexLifecycleCapture Capture,
    CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan? ParentLifecycle { get; init; }
    public CapacityFinalCalleeCommonHighSizeBitScanPlan? IndexLifecycle { get; init; }
    public bool ChildEntered { get; init; }
    public uint? DerivedChildEntryEAX { get; init; }
    public uint? OriginalEntryEDIFor9377 { get; init; }
    public bool OriginalEntryEDIReusedFor9377 { get; init; }
    public bool LateEntryEBPConsumed { get; init; }
    public bool LateEntryEBPProvidedButUnused { get; init; }
    public bool LateEntryEBPUnusedOnBypass { get; init; }
    public bool FrameStoreAttempted { get; init; }
    public uint? FrameStoreAddress { get; init; }
    public uint? FrameStoreValue { get; init; }
    public bool FrameStoreCommitted { get; init; }
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent>.Empty;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Events.Select(item => item.RawFileOffset).ToImmutableArray();
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent>
        CommittedFrameStores =>
        Events.Where(item =>
                item.Stage == CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index
                && item.Kind
                    == CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.StoreEDXToFrame
                && item.Committed == true)
            .ToImmutableArray();
    public CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? FlagsAtStop { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public bool TargetEntered => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the common high-size gate, its unsigned FFFF ceiling, and the
/// bounded BSR-to-index sequence, stopping before the following store at 9384.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.EntryRawFileOffset;
    public const uint IndexEntryRawFileOffset =
        CapacityFinalCalleeCommonHighSizeBitScanProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonHighSizeBitScanProjection.BoundaryRawFileOffsetExclusive;
    public const uint FollowingStoreRawFileOffset =
        CapacityFinalCalleeCommonHighSizeBitScanProjection.FollowingStoreRawFileOffset;
    public const uint Ceiling = CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Ceiling;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection
            .CodeVirtualAddressDeltaFromRawFileOffset;

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleInstructionEvidence>
        Instructions { get; } =
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Instructions
            .Select(instruction =>
                new CapacityFinalCalleeCommonHighSizeIndexLifecycleInstructionEvidence(
                    instruction.Gate
                        == CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry
                        ? CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry
                        : CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.FlatIdaAddress,
                    instruction.VirtualAddress,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
            .Concat(
                CapacityFinalCalleeCommonHighSizeBitScanProjection.Instructions
                    .Select(instruction =>
                        new CapacityFinalCalleeCommonHighSizeIndexLifecycleInstructionEvidence(
                            CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                            instruction.Observation,
                            instruction.RawFileOffset,
                            instruction.FlatIdaAddress,
                            instruction.VirtualAddress,
                            instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan Project(
        CapacityFinalCalleeCommonHighSizeIndexLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(capture.EntryEDI));
        ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent> parentEvents =
            parent.Events.Select(ConvertParentEvent).ToImmutableArray();

        if (parent.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.MissingEntryEDIAt9354)
        {
            bool exactMissing = capture.EntryEDI is null
                && parent.EntryGate?.Outcome
                    == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.MissingEntryEDI
                && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
                && parent.Events.IsEmpty
                && parent.Writes.IsEmpty
                && !parent.ChildEntered;
            if (!exactMissing)
            {
                return Stop(
                    capture,
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.InvalidParentHandoff,
                    parent,
                    parentEvents,
                    parent.StopAtRawFileOffsetExclusive,
                    flags: ConvertFlags(parent.FlagsAtStop),
                    selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                    lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
            }

            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.MissingEntryEDIAt9354,
                parent,
                parentEvents,
                parent.StopAtRawFileOffsetExclusive,
                flags: ConvertFlags(parent.FlagsAtStop),
                selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                exactAdmission: false,
                lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
        }

        if (IsExactZeroBypass(parent))
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.ParentZeroAt9384,
                parent,
                parentEvents,
                parent.StopAtRawFileOffsetExclusive,
                flags: ConvertFlags(parent.FlagsAtStop),
                selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                exactAdmission: parent.ExactCleanAdmission
                    && !capture.LateEntryEBP.HasValue,
                lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
        }

        if (IsExactAboveCeilingBypass(parent))
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.ParentAboveCeilingAt9384,
                parent,
                parentEvents,
                parent.StopAtRawFileOffsetExclusive,
                flags: ConvertFlags(parent.FlagsAtStop),
                selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                exactAdmission: parent.ExactCleanAdmission
                    && !capture.LateEntryEBP.HasValue,
                lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.InvalidCeilingHandoff)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.InvalidCeilingHandoff,
                parent,
                parentEvents,
                parent.StopAtRawFileOffsetExclusive,
                flags: ConvertFlags(parent.FlagsAtStop),
                selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
        }

        if (!IsExactIndexHandoff(parent, out uint childEAX))
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.InvalidParentHandoff,
                parent,
                parentEvents,
                parent.StopAtRawFileOffsetExclusive,
                flags: ConvertFlags(parent.FlagsAtStop),
                selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
                lateEBPUnusedOnBypass: capture.LateEntryEBP.HasValue);
        }

        CapacityFinalCalleeCommonHighSizeBitScanPlan index =
            CapacityFinalCalleeCommonHighSizeBitScanProjection.Project(
                new(
                    EntryEAX: childEAX,
                    LateEntryEBP: capture.LateEntryEBP,
                    LateEntryEDI: capture.EntryEDI));
        ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent> events =
            parentEvents.AddRange(index.Events.Select(ConvertIndexEvent));

        CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome outcome = index.Outcome switch
        {
            CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEBPAt9374 =>
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.MissingLateEntryEBPAt9374,
            CapacityFinalCalleeCommonHighSizeBitScanOutcome.IndexedResultAt9384BeforeNextStore =>
                CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .IndexedResultAt9384BeforeFollowingStore,
            _ => CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.InvalidIndexHandoff
        };
        bool exactSuccessfulIndexLifecycle =
            IsExactSuccessfulIndexLifecycle(index, parent, capture, childEAX);

        return Stop(
            capture,
            outcome,
            parent,
            events,
            index.StopAtRawFileOffsetExclusive,
            index: index,
            childEntered: true,
            childEAX: childEAX,
            flags: ConvertFlags(index.FlagsAtStop),
            selectedDestination: parent.FinalSelectedDestinationRawFileOffset,
            exactAdmission: parent.ExactCleanAdmission && exactSuccessfulIndexLifecycle,
            lateEBPConsumed: index.StoreAddress.HasValue,
            lateEBPUnusedOnBypass: false);
    }

    private static bool IsExactSuccessfulIndexLifecycle(
        CapacityFinalCalleeCommonHighSizeBitScanPlan index,
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent,
        CapacityFinalCalleeCommonHighSizeIndexLifecycleCapture capture,
        uint childEAX)
    {
        if (capture.EntryEDI is not uint entryEDI
            || capture.LateEntryEBP is not uint entryEBP
            || index.Outcome
                != CapacityFinalCalleeCommonHighSizeBitScanOutcome
                    .IndexedResultAt9384BeforeNextStore
            || index.Capture.EntryEAX != childEAX
            || index.Capture.LateEntryEBP != entryEBP
            || index.Capture.LateEntryEDI != entryEDI
            || parent.ParentShiftResultEAX != childEAX
            || parent.ChildEntryEAX != childEAX
            || childEAX == 0
            || childEAX > Ceiling
            || index.StopAtRawFileOffsetExclusive != BoundaryRawFileOffsetExclusive
            || !index.BSRReached
            || index.BSRInputEAX != childEAX
            || index.BSRDestinationDefined != true
            || index.BSRResultEAX is not uint bsrIndex
            || bsrIndex > 15
            || !index.StoreAttempted
            || !index.StoreCommitted
            || index.StoreAddress is not uint storeAddress
            || storeAddress != unchecked(entryEBP
                - CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreDisplacement)
            || index.StoreValue != bsrIndex
            || !index.MoveEDIToEAXReached
            || index.ShiftInputEAX != entryEDI
            || !index.ShiftReached
            || !index.AndReached
            || !index.FinalLeaReached
            || index.FinalIndexECX is not uint finalIndex
            || finalIndex > 31
            || !index.Writes.SequenceEqual(new[] { storeAddress })
            || parent.TargetEntered)
        {
            return false;
        }

        return true;
    }

    private static CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan Stop(
        CapacityFinalCalleeCommonHighSizeIndexLifecycleCapture capture,
        CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome outcome,
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent,
        ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent> events,
        uint stopAt,
        CapacityFinalCalleeCommonHighSizeBitScanPlan? index = null,
        bool childEntered = false,
        uint? childEAX = null,
        CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? flags = null,
        uint? selectedDestination = null,
        bool exactAdmission = false,
        bool lateEBPConsumed = false,
        bool lateEBPUnusedOnBypass = false) =>
        new(capture, outcome)
        {
            ParentLifecycle = parent,
            IndexLifecycle = index,
            ChildEntered = childEntered,
            DerivedChildEntryEAX = childEAX,
            OriginalEntryEDIFor9377 = capture.EntryEDI,
            OriginalEntryEDIReusedFor9377 = index?.MoveEDIToEAXReached == true,
            LateEntryEBPConsumed = lateEBPConsumed,
            LateEntryEBPProvidedButUnused =
                capture.LateEntryEBP.HasValue && !lateEBPConsumed,
            LateEntryEBPUnusedOnBypass = lateEBPUnusedOnBypass,
            FrameStoreAttempted = index?.StoreAttempted == true,
            FrameStoreAddress = index?.StoreAddress,
            FrameStoreValue = index?.StoreValue,
            FrameStoreCommitted = index?.StoreCommitted == true,
            Reads = index?.Reads ?? ImmutableArray<uint>.Empty,
            Writes = index?.Writes ?? ImmutableArray<uint>.Empty,
            Events = events,
            FlagsAtStop = flags,
            SelectedDestinationRawFileOffset = selectedDestination,
            ExactCleanAdmission = exactAdmission,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static bool IsExactZeroBypass(
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.ParentZeroAt9384
        && parent.ExactCleanAdmission
        && !parent.ChildEntered
        && parent.ParentShiftReached
        && parent.ParentShiftResultEAX == 0
        && parent.ParentJNZReached
        && parent.ParentJNZTaken == false
        && parent.ParentXorReached
        && parent.ParentXorResultECX == 0
        && parent.FinalSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
                .BoundaryRawFileOffsetExclusive
        && parent.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
                .BoundaryRawFileOffsetExclusive
        && parent.CeilingGate is null
        && parent.Writes.IsEmpty
        && !parent.TargetEntered;

    private static bool IsExactAboveCeilingBypass(
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent) =>
        parent.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                .ChildAboveCeilingAt9384BeforeStore
        && parent.ExactCleanAdmission
        && parent.ChildEntered
        && parent.ParentShiftResultEAX is uint shiftedEAX
        && shiftedEAX > Ceiling
        && parent.ChildEntryEAX == shiftedEAX
        && parent.ChildCompareReached
        && parent.ChildCompareInputEAX == shiftedEAX
        && parent.ChildCompareImmediate == Ceiling
        && parent.ChildJBEReached
        && parent.ChildJBETaken == false
        && parent.ChildMoveReached
        && parent.ChildMoveResultECX
            == CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AbovePathECX
        && parent.ChildJMPReached
        && parent.ChildJMPTaken == true
        && parent.ChildJMPSelectedDestinationRawFileOffset
            == BoundaryRawFileOffsetExclusive
        && parent.FinalSelectedDestinationRawFileOffset
            == BoundaryRawFileOffsetExclusive
        && parent.StopAtRawFileOffsetExclusive == BoundaryRawFileOffsetExclusive
        && parent.Writes.IsEmpty
        && !parent.TargetEntered;

    private static bool IsExactIndexHandoff(
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan parent,
        out uint childEAX)
    {
        childEAX = 0;
        if (parent.Outcome
                != CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildBelowOrEqualAt936FBeforeBSR
            || !parent.ExactCleanAdmission
            || !parent.ChildEntered
            || parent.ParentShiftResultEAX is not uint shiftedEAX
            || shiftedEAX == 0
            || shiftedEAX > Ceiling
            || parent.ChildEntryEAX != shiftedEAX
            || parent.ChildCompareInputEAX != shiftedEAX
            || parent.ChildCompareImmediate != Ceiling
            || !parent.ChildCompareReached
            || !parent.ChildJBEReached
            || parent.ChildJBETaken != true
            || parent.ChildJBESelectedDestinationRawFileOffset
                != IndexEntryRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != IndexEntryRawFileOffset
            || parent.FinalSelectedDestinationRawFileOffset != IndexEntryRawFileOffset
            || parent.Writes.Length != 0
            || parent.TargetEntered)
        {
            return false;
        }

        childEAX = shiftedEAX;
        return true;
    }

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent ConvertParentEvent(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent item) =>
        new(
            item.Gate == CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry
                ? CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry
                : CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
            item.Kind switch
            {
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Move =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.Move,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.ShiftRight =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.ShiftRight,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Test =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.Test,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.BranchNotZero =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.BranchNotZero,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Xor =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.Xor,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Jump =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.Jump,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Compare =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.Compare,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.BranchBelowOrEqual =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.BranchBelowOrEqual,
                _ => throw new ArgumentOutOfRangeException(nameof(item))
            },
            item.RawFileOffset,
            item.Input,
            item.Immediate,
            item.Result,
            Flags: ConvertFlags(item.Flags),
            BranchTaken: item.BranchTaken,
            SelectedDestinationRawFileOffset: item.SelectedDestinationRawFileOffset);

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleEvent ConvertIndexEvent(
        CapacityFinalCalleeCommonHighSizeBitScanEvent item) =>
        new(
            CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
            item.Kind switch
            {
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.BitScanReverse =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.BitScanReverse,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEAXToEDX =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.MoveEAXToEDX,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.StoreEDXToFrame =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.StoreEDXToFrame,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.MoveEDIToEAX =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.MoveEDIToEAX,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaShiftCount =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.LeaShiftCount,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.ShiftRight =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.ShiftRight,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.AndOne =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.AndOne,
                CapacityFinalCalleeCommonHighSizeBitScanEventKind.LeaIndexedResult =>
                    CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.LeaIndexedResult,
                _ => throw new ArgumentOutOfRangeException(nameof(item))
            },
            item.RawFileOffset,
            item.Input,
            item.Immediate,
            item.Result,
            item.Address,
            item.DestinationDefined,
            item.Committed,
            ConvertFlags(item.Flags));

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeBitScanFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeBitScanFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
