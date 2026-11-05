using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingLifecycleCapture(
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
{
    MissingEntryEDIAt9354,
    ParentZeroAt9384,
    InvalidParentHandoff,
    InvalidCeilingHandoff,
    ChildBelowOrEqualAt936FBeforeBSR,
    ChildAboveCeilingAt9384BeforeStore
}

public enum CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate
{
    Entry,
    Ceiling
}

public enum CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind
{
    Move,
    ShiftRight,
    Test,
    BranchNotZero,
    Xor,
    Jump,
    Compare,
    BranchBelowOrEqual
}

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingLifecycleInstructionEvidence(
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate Gate,
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent(
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate Gate,
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind Kind,
    uint RawFileOffset,
    uint? Input = null,
    uint? Immediate = null,
    uint? Result = null,
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? Flags = null,
    bool? BranchTaken = null,
    uint? SelectedDestinationRawFileOffset = null);

public sealed record CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan(
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleCapture Capture,
    CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonHighSizeEntryGatePlan? EntryGate { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingGatePlan? CeilingGate { get; init; }
    public bool EntryEDICaptured { get; init; }
    public bool ParentShiftReached { get; init; }
    public uint? ParentShiftResultEAX { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? ParentTestFlags { get; init; }
    public bool ParentJNZReached { get; init; }
    public bool? ParentJNZTaken { get; init; }
    public uint? ParentJNZSelectedDestinationRawFileOffset { get; init; }
    public bool ParentXorReached { get; init; }
    public uint? ParentXorResultECX { get; init; }
    public bool ChildEntered { get; init; }
    public uint? ChildEntryEAX { get; init; }
    public bool ChildCompareReached { get; init; }
    public uint? ChildCompareInputEAX { get; init; }
    public uint? ChildCompareImmediate { get; init; }
    public uint? ChildCompareResult { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? ChildCompareFlags { get; init; }
    public bool ChildJBEReached { get; init; }
    public bool? ChildJBETaken { get; init; }
    public uint? ChildJBESelectedDestinationRawFileOffset { get; init; }
    public bool ChildMoveReached { get; init; }
    public uint? ChildMoveResultECX { get; init; }
    public bool ChildJMPReached { get; init; }
    public bool? ChildJMPTaken { get; init; }
    public uint? ChildJMPSelectedDestinationRawFileOffset { get; init; }
    public uint? ECXAtStop { get; init; }
    public bool ECXAtStopKnown { get; init; }
    public uint? FinalSelectedDestinationRawFileOffset { get; init; }
    public CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? FlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent>.Empty;
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Events.Select(item => item.RawFileOffset).ToImmutableArray();
    public bool FrameReadReached => false;
    public bool GlobalReadReached => false;
    public ImmutableArray<uint> Reads { get; init; } = ImmutableArray<uint>.Empty;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public bool TargetEntered => false;
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the common high-size entry gate at 9354 with the unsigned FFFF
/// ceiling gate at 9361, stopping before BSR or any store.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryRawFileOffset;
    public const uint HighSizeDestinationRawFileOffset =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.HighSizeDestinationRawFileOffset;
    public const uint ZeroSizeDestinationRawFileOffset =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.ZeroSizeDestinationRawFileOffset;
    public const uint BelowOrEqualDestinationRawFileOffset =
        CapacityFinalCalleeCommonHighSizeCeilingGateProjection.BelowOrEqualDestinationRawFileOffset;
    public const uint AboveDestinationRawFileOffset =
        CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AboveDestinationRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonHighSizeCeilingGateProjection.BoundaryRawFileOffsetExclusive;
    public const uint Ceiling = CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Ceiling;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.CodeVirtualAddressDeltaFromRawFileOffset;

    public const string EntryWindowBytes =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.CapturedWindowBytes;
    public const string CeilingWindowBytes =
        CapacityFinalCalleeCommonHighSizeCeilingGateProjection.CapturedWindowBytes;

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleInstructionEvidence>
        Instructions { get; } =
        CapacityFinalCalleeCommonHighSizeEntryGateProjection.Instructions
            .Select(instruction => new CapacityFinalCalleeCommonHighSizeCeilingLifecycleInstructionEvidence(
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                instruction.Observation,
                instruction.RawFileOffset,
                instruction.FlatIdaAddress,
                instruction.VirtualAddress,
                instruction.Bytes,
                instruction.BranchTargetRawFileOffset,
                instruction.BranchTargetVirtualAddress))
            .Concat(
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Instructions
                    .Select(instruction =>
                        new CapacityFinalCalleeCommonHighSizeCeilingLifecycleInstructionEvidence(
                            CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                            instruction.Observation,
                            instruction.RawFileOffset,
                            instruction.FlatIdaAddress,
                            instruction.VirtualAddress,
                            instruction.Bytes,
                            instruction.BranchTargetRawFileOffset,
                            instruction.BranchTargetVirtualAddress)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan Project(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeEntryGatePlan entry =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(capture.EntryEDI));
        ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent> entryEvents =
            entry.Events.Select(ConvertEntryEvent).ToImmutableArray();

        if (entry.Outcome
                == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.MissingEntryEDI
            && capture.EntryEDI is null
            && !entry.MoveReached
            && !entry.TargetEntered
            && entry.StopAtRawFileOffsetExclusive == EntryRawFileOffset)
        {
            return new(
                capture,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.MissingEntryEDIAt9354)
            {
                EntryGate = entry,
                Events = entryEvents,
                ExactCleanAdmission = false,
                StopAtRawFileOffsetExclusive = EntryRawFileOffset
            };
        }

        if (IsExactParentZeroExit(entry))
        {
            return new(
                capture,
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.ParentZeroAt9384)
            {
                EntryGate = entry,
                EntryEDICaptured = entry.EntryEDICaptured,
                ParentShiftReached = entry.ShiftReached,
                ParentShiftResultEAX = entry.ShiftResultEAX,
                ParentTestFlags = ConvertFlags(entry.TestFlags),
                ParentJNZReached = entry.JNZReached,
                ParentJNZTaken = entry.JNZTaken,
                ParentJNZSelectedDestinationRawFileOffset =
                    entry.JNZSelectedDestinationRawFileOffset,
                ParentXorReached = entry.XorReached,
                ParentXorResultECX = entry.XorResultECX,
                ECXAtStop = entry.XorResultECX,
                ECXAtStopKnown = entry.XorReached && entry.XorResultECX.HasValue,
                FinalSelectedDestinationRawFileOffset =
                    entry.JMPSelectedDestinationRawFileOffset,
                FlagsAtStop = entry.XorReached
                    ? ConvertFlags(entry.XorFlags)
                    : ConvertFlags(entry.TestFlags),
                Events = entryEvents,
                ExactCleanAdmission = entry.ExactCleanAdmission,
                StopAtRawFileOffsetExclusive = entry.StopAtRawFileOffsetExclusive
            };
        }

        if (!IsExactParentHighSizeHandoff(entry))
        {
            return InvalidParentHandoff(capture, entry, entryEvents);
        }

        uint? derivedChildEAX = entry.ShiftResultEAX;
        if (derivedChildEAX is not uint childEAX)
        {
            return InvalidParentHandoff(capture, entry, entryEvents);
        }

        CapacityFinalCalleeCommonHighSizeCeilingGatePlan ceiling =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: childEAX));
        ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent> events =
            entryEvents
                .AddRange(ceiling.Events.Select(ConvertCeilingEvent));

        bool belowOrEqual = IsExactChildBelowOrEqualExit(ceiling, childEAX);
        bool above = IsExactChildAboveCeilingExit(ceiling, childEAX);
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome outcome =
            belowOrEqual
                ? CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildBelowOrEqualAt936FBeforeBSR
                : above
                    ? CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                        .ChildAboveCeilingAt9384BeforeStore
                    : CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                        .InvalidCeilingHandoff;

        return new(capture, outcome)
        {
            EntryGate = entry,
            CeilingGate = ceiling,
            EntryEDICaptured = entry.EntryEDICaptured,
            ParentShiftReached = entry.ShiftReached,
            ParentShiftResultEAX = entry.ShiftResultEAX,
            ParentTestFlags = ConvertFlags(entry.TestFlags),
            ParentJNZReached = entry.JNZReached,
            ParentJNZTaken = entry.JNZTaken,
            ParentJNZSelectedDestinationRawFileOffset =
                entry.JNZSelectedDestinationRawFileOffset,
            ChildEntered = true,
            ChildEntryEAX = childEAX,
            ChildCompareReached = ceiling.CompareReached,
            ChildCompareInputEAX = ceiling.CompareInputEAX,
            ChildCompareImmediate = ceiling.CompareImmediate,
            ChildCompareResult = ceiling.CompareResult,
            ChildCompareFlags = ConvertFlags(ceiling.CompareFlags),
            ChildJBEReached = ceiling.JBEReached,
            ChildJBETaken = ceiling.JBETaken,
            ChildJBESelectedDestinationRawFileOffset =
                ceiling.JBESelectedDestinationRawFileOffset,
            ChildMoveReached = ceiling.MoveReached,
            ChildMoveResultECX = ceiling.MoveResultECX,
            ChildJMPReached = ceiling.JMPReached,
            ChildJMPTaken = ceiling.JMPTaken,
            ChildJMPSelectedDestinationRawFileOffset =
                ceiling.JMPSelectedDestinationRawFileOffset,
            ECXAtStop = ceiling.MoveReached ? ceiling.MoveResultECX : null,
            ECXAtStopKnown = ceiling.MoveReached && ceiling.MoveResultECX.HasValue,
            FinalSelectedDestinationRawFileOffset = belowOrEqual
                ? ceiling.JBESelectedDestinationRawFileOffset
                : ceiling.JMPReached
                    ? ceiling.JMPSelectedDestinationRawFileOffset
                    : ceiling.JBESelectedDestinationRawFileOffset,
            FlagsAtStop = ceiling.CompareReached
                ? ConvertFlags(ceiling.FlagsAtStop)
                : ConvertFlags(entry.FlagsAtStop),
            Events = events,
            ExactCleanAdmission = entry.ExactCleanAdmission && (belowOrEqual || above),
            StopAtRawFileOffsetExclusive = ceiling.StopAtRawFileOffsetExclusive
        };
    }

    private static CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan InvalidParentHandoff(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleCapture capture,
        CapacityFinalCalleeCommonHighSizeEntryGatePlan entry,
        ImmutableArray<CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent> events) =>
        new(capture, CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.InvalidParentHandoff)
        {
            EntryGate = entry,
            EntryEDICaptured = entry.EntryEDICaptured,
            ParentShiftReached = entry.ShiftReached,
            ParentShiftResultEAX = entry.ShiftResultEAX,
            ParentTestFlags = ConvertFlags(entry.TestFlags),
            ParentJNZReached = entry.JNZReached,
            ParentJNZTaken = entry.JNZTaken,
            ParentJNZSelectedDestinationRawFileOffset =
                entry.JNZSelectedDestinationRawFileOffset,
            ParentXorReached = entry.XorReached,
            ParentXorResultECX = entry.XorResultECX,
            ECXAtStop = entry.XorReached ? entry.XorResultECX : null,
            ECXAtStopKnown = entry.XorReached && entry.XorResultECX.HasValue,
            FinalSelectedDestinationRawFileOffset = entry.JMPReached
                ? entry.JMPSelectedDestinationRawFileOffset
                : entry.JNZSelectedDestinationRawFileOffset,
            FlagsAtStop = entry.XorReached
                ? ConvertFlags(entry.XorFlags)
                : ConvertFlags(entry.TestFlags),
            Events = events,
            ExactCleanAdmission = false,
            StopAtRawFileOffsetExclusive = entry.StopAtRawFileOffsetExclusive
        };

    private static bool IsExactParentZeroExit(
        CapacityFinalCalleeCommonHighSizeEntryGatePlan entry) =>
        entry.Outcome
            == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.ZeroSizeAt9384BeforeIndexedStore
        && entry.ExactCleanAdmission
        && entry.EntryEDICaptured
        && entry.TestReached
        && entry.TestFlags.HasValue
        && entry.JNZReached
        && entry.JNZTaken == false
        && entry.JNZSelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonHighSizeEntryGateProjection.XorRawFileOffset
        && entry.XorReached
        && entry.XorResultECX == 0
        && entry.JMPReached
        && entry.JMPTaken == true
        && entry.JMPSelectedDestinationRawFileOffset
            == ZeroSizeDestinationRawFileOffset
        && entry.StopAtRawFileOffsetExclusive == ZeroSizeDestinationRawFileOffset
        && entry.FlagsAtStop == entry.XorFlags
        && !entry.TargetEntered
        && !entry.FrameReadReached
        && !entry.GlobalReadReached
        && entry.Reads.IsEmpty
        && entry.Writes.IsEmpty;

    private static bool IsExactParentHighSizeHandoff(
        CapacityFinalCalleeCommonHighSizeEntryGatePlan entry) =>
        entry.Outcome
            == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.HighSizeAt9361BeforeCompare
        && entry.ExactCleanAdmission
        && entry.EntryEDICaptured
        && entry.TestReached
        && entry.TestFlags.HasValue
        && entry.JNZReached
        && entry.JNZTaken == true
        && entry.JNZSelectedDestinationRawFileOffset
            == HighSizeDestinationRawFileOffset
        && entry.StopAtRawFileOffsetExclusive == HighSizeDestinationRawFileOffset
        && entry.ShiftReached
        && entry.ShiftResultEAX is uint shiftedEAX
        && shiftedEAX != 0
        && entry.FlagsAtStop == entry.TestFlags
        && !entry.XorReached
        && !entry.JMPReached
        && !entry.TargetEntered
        && !entry.FrameReadReached
        && !entry.GlobalReadReached
        && entry.Reads.IsEmpty
        && entry.Writes.IsEmpty;

    private static bool IsExactChildBelowOrEqualExit(
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan ceiling,
        uint derivedEAX) =>
        ceiling.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                .UnsignedBelowOrEqualToCeilingAt936FBeforeBSR
        && ceiling.CompareReached
        && ceiling.CompareInputEAX == derivedEAX
        && ceiling.CompareImmediate == Ceiling
        && ceiling.JBEReached
        && ceiling.JBETaken == true
        && ceiling.JBESelectedDestinationRawFileOffset
            == BelowOrEqualDestinationRawFileOffset
        && !ceiling.MoveReached
        && !ceiling.JMPReached
        && ceiling.StopAtRawFileOffsetExclusive == BelowOrEqualDestinationRawFileOffset
        && !ceiling.TargetEntered
        && !ceiling.FrameReadReached
        && !ceiling.GlobalReadReached
        && ceiling.Reads.IsEmpty
        && ceiling.Writes.IsEmpty;

    private static bool IsExactChildAboveCeilingExit(
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan ceiling,
        uint derivedEAX) =>
        ceiling.Outcome
            == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                .UnsignedAboveCeilingAt9384BeforeStore
        && ceiling.CompareReached
        && ceiling.CompareInputEAX == derivedEAX
        && ceiling.CompareImmediate == Ceiling
        && ceiling.JBEReached
        && ceiling.JBETaken == false
        && ceiling.JBESelectedDestinationRawFileOffset
            == CapacityFinalCalleeCommonHighSizeCeilingGateProjection.MoveRawFileOffset
        && ceiling.MoveReached
        && ceiling.MoveResultECX
            == CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AbovePathECX
        && ceiling.JMPReached
        && ceiling.JMPTaken == true
        && ceiling.JMPSelectedDestinationRawFileOffset
            == AboveDestinationRawFileOffset
        && ceiling.StopAtRawFileOffsetExclusive == AboveDestinationRawFileOffset
        && !ceiling.TargetEntered
        && !ceiling.FrameReadReached
        && !ceiling.GlobalReadReached
        && ceiling.Reads.IsEmpty
        && ceiling.Writes.IsEmpty;

    private static CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent ConvertEntryEvent(
        CapacityFinalCalleeCommonHighSizeEntryGateEvent item) =>
        new(
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
            item.Kind switch
            {
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Move =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Move,
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.ShiftRight =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.ShiftRight,
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Test =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Test,
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.BranchNotZero =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.BranchNotZero,
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Xor =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Xor,
                CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Jump =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Jump,
                _ => throw new ArgumentOutOfRangeException(nameof(item))
            },
            item.RawFileOffset,
            item.Input,
            Result: item.Result,
            Flags: ConvertFlags(item.Flags),
            BranchTaken: item.BranchTaken,
            SelectedDestinationRawFileOffset: item.SelectedDestinationRawFileOffset);

    private static CapacityFinalCalleeCommonHighSizeCeilingLifecycleEvent ConvertCeilingEvent(
        CapacityFinalCalleeCommonHighSizeCeilingGateEvent item) =>
        new(
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
            item.Kind switch
            {
                CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Compare =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Compare,
                CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.BranchBelowOrEqual =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.BranchBelowOrEqual,
                CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Move =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Move,
                CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Jump =>
                    CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Jump,
                _ => throw new ArgumentOutOfRangeException(nameof(item))
            },
            item.RawFileOffset,
            item.Input,
            item.Immediate,
            item.Result,
            ConvertFlags(item.Flags),
            item.BranchTaken,
            item.SelectedDestinationRawFileOffset);

    private static CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeEntryGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeEntryGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeCeilingGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeCeilingGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
