using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedCountTagLifecycleCapture(
    CapacityFinalCalleeCommonIndexedCountClassificationCapture Parent,
    uint? LateEntryEDI,
    uint? FreshEDXPlus4);

public enum CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
{
    ParentBlocked,
    ChildStoppedAt93D6,
    ChildStoppedAt93D8,
    ChildEqualToBounded9403,
    ChildUnequalToFallThrough93E4
}

public enum CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
{
    ParentIndexedDWORDAt93C0,
    ChildTagDWORDAt93D8
}

public readonly record struct CapacityFinalCalleeCommonIndexedCountTagLifecycleRead(
    CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage Stage,
    uint RawFileOffset,
    uint? AbsoluteAddress,
    bool ReadReached,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan(
    CapacityFinalCalleeCommonIndexedCountTagLifecycleCapture Capture,
    CapacityFinalCalleeCommonIndexedCountClassificationPlan ParentPlan,
    CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome Outcome)
{
    public bool ExactParentHandoffCompleted { get; init; }
    public bool ParentHandoffInstructionEntered => ChildPlan is not null;
    public bool DestinationEntered => false;
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? FlagsAtStop =>
        ChildPlan is null
            ? ParentPlan.FlagsAtStop is { } flags
                ? new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow)
                : null
            : ChildFlagsAtStop;
    public CapacityFinalCalleeCommonMaskedTagEqualityGatePlan? ChildPlan { get; init; }
    public uint? ChildEntryEAX { get; init; }
    public uint? ChildEntryEDI { get; init; }
    public uint? ChildEntryEDX { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? ChildFlagsAtEntry { get; init; }
    public CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? ChildFlagsAtStop { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>
        ChildOrderedEvents { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonIndexedCountTagLifecycleRead>
        OrderedReads { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedCountTagLifecycleRead>.Empty;
    public bool LateEntryEDICaptureAdmitted { get; init; }
    public bool LateEntryEDICaptureProvidedButUnused =>
        !LateEntryEDICaptureAdmitted && Capture.LateEntryEDI.HasValue
        || ChildPlan is not null && ChildPlan.EntryEDICaptureProvidedButUnused;
    public bool FreshEDXPlus4CaptureProvidedButUnused =>
        ChildPlan is null
            ? Capture.FreshEDXPlus4.HasValue
            : ChildPlan.FreshEDXPlus4CaptureProvidedButUnused;
    public bool ChildFreshReadReached => ChildPlan?.FreshReadReached == true;
    public uint? ChildFreshReadAddress => ChildPlan?.FreshReadAddress;
    public bool ChildBoundedTargetNotEntered =>
        ChildPlan is
        {
            ExactCleanBoundedDestination: true,
            StopAtRawFileOffsetExclusive: var stop,
            JzSelectedDestinationRawFileOffset: var destination
        }
        && stop == destination
        && ChildPlan.MaskReached
        && ChildPlan.ShiftReached
        && ChildPlan.ShiftExecuted
        && ChildPlan.CompareReached
        && ChildPlan.JzReached
        && !ChildPlan.EntryEAXCaptureProvidedButUnused
        && !ChildPlan.EntryEDICaptureProvidedButUnused
        && !ChildPlan.EntryEDXCaptureProvidedButUnused
        && !ChildPlan.FreshEDXPlus4CaptureProvidedButUnused
        && destination is
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .EqualDestinationRawFileOffset
            or CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                .FallThroughDestinationRawFileOffset;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Hands the completed 93C0 indexed-count calculation into the 93D4 masked-tag gate.
/// The child's tag DWORD remains an independent fresh read at [parent EDX+4].
/// </summary>
public static class CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset;
    public const uint ParentHandoffRawFileOffsetExclusive =
        CapacityFinalCalleeCommonIndexedCountClassificationProjection.BoundaryRawFileOffsetExclusive;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EntryRawFileOffset;

    public static CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan Project(
        CapacityFinalCalleeCommonIndexedCountTagLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonIndexedCountClassificationPlan parent =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(capture.Parent);

        ImmutableArray<CapacityFinalCalleeCommonIndexedCountTagLifecycleRead>.Builder reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonIndexedCountTagLifecycleRead>(2);
        if (parent.IndexedDWORDReadAttempted)
        {
            reads.Add(new(
                CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage.ParentIndexedDWORDAt93C0,
                CapacityFinalCalleeCommonIndexedCountClassificationProjection.FreshLoadRawFileOffset,
                parent.IndexedDWORDReadAddress,
                ReadReached: true,
                parent.IndexedDWORDReadCompleted,
                parent.IndexedDWORDReadCompleted ? parent.FreshEDX : null));
        }

        if (!IsExactParentHandoff(parent))
        {
            return new(
                capture,
                parent,
                CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ParentBlocked)
            {
                OrderedReads = reads.ToImmutable(),
                StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
            };
        }

        uint childEAX = parent.Outcome
            == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
                ? parent.XorEAXResult!.Value
                : parent.SubtractResult!.Value;
        uint childEDX = parent.FreshEDX!.Value;
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags parentFlags =
            ToChildFlags(parent.FlagsAtStop!.Value);
        var childCapture = new CapacityFinalCalleeCommonMaskedTagEqualityGateCapture(
            EntryEAX: childEAX,
            EntryEDI: capture.LateEntryEDI,
            EntryEDX: childEDX,
            FreshEDXPlus4: capture.FreshEDXPlus4);
        CapacityFinalCalleeCommonMaskedTagEqualityGatePlan child =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.Project(childCapture);

        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateReadAttempt read in child.ReadAttempts)
        {
            reads.Add(new(
                CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage.ChildTagDWORDAt93D8,
                CapacityFinalCalleeCommonMaskedTagEqualityGateProjection
                    .FreshEDXPlus4MoveRawFileOffset,
                read.AbsoluteAddress,
                read.ReadReached,
                read.ValueCaptured,
                read.Value));
        }

        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent> childEvents =
            CarryFlagsThroughChild(parentFlags, child);
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags childFlagsAtStop =
            childEvents.IsEmpty
                ? parentFlags
                : childEvents[^1].FlagsAfter ?? parentFlags;

        CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome outcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI =>
                    CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D6,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4 =>
                    CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D8,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.EqualToBounded9403 =>
                    CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildEqualToBounded9403,
                CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.UnequalToFallThrough93E4 =>
                    CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildUnequalToFallThrough93E4,
                _ => CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ParentBlocked
            };

        return new(capture, parent, outcome)
        {
            ExactParentHandoffCompleted = true,
            ChildPlan = child,
            ChildEntryEAX = childEAX,
            ChildEntryEDI = capture.LateEntryEDI,
            ChildEntryEDX = childEDX,
            ChildFlagsAtEntry = parentFlags,
            ChildFlagsAtStop = childFlagsAtStop,
            ChildOrderedEvents = childEvents,
            OrderedReads = reads.ToImmutable(),
            LateEntryEDICaptureAdmitted = true,
            StopAtRawFileOffsetExclusive = child.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsExactParentHandoff(
        CapacityFinalCalleeCommonIndexedCountClassificationPlan parent)
    {
        if (parent.Capture.EntryEBX is not uint entryEBX
            || parent.Capture.FreshIndexedDWORD is not uint freshIndexedDWORD
            || parent.Capture.LateEntryECX is not uint ecx
            || !parent.IndexedDWORDReadAttempted
            || !parent.IndexedDWORDReadCompleted
            || parent.IndexedDWORDReadAddress != entryEBX
            || parent.FreshEDX != freshIndexedDWORD
            || !parent.CompareReached
            || !parent.CompareInstructionReached
            || parent.CompareLeft != ecx
            || parent.CompareRight
                != CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareImmediate
            || parent.CompareResult
                != unchecked(ecx
                    - CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareImmediate)
            || parent.CompareFlags is null
            || !parent.JnzReached
            || parent.StopAtRawFileOffsetExclusive != ParentHandoffRawFileOffsetExclusive
            || !parent.Writes.IsEmpty
            || !parent.Reads.SequenceEqual(new[] { entryEBX })
            || parent.UnreachedFreshIndexedDWORDObservationProvided
            || parent.UnreachedLateEntryECXCaptureProvided)
        {
            return false;
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4)
        {
            return ecx
                    == CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareImmediate
                && parent.CompareFlags.Value.Zero == true
                && parent.JnzTaken == false
                && parent.JnzSelectedDestinationRawFileOffset
                    == CapacityFinalCalleeCommonIndexedCountClassificationProjection.XorRawFileOffset
                && parent.XorEAXReached
                && !parent.XorEAXInputKnown
                && parent.XorEAXResult == 0
                && parent.XorFlags is
                {
                    Carry: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
                && parent.JumpReached
                && parent.JumpSelectedDestinationRawFileOffset
                    == ParentHandoffRawFileOffsetExclusive
                && !parent.ShiftECXReached
                && !parent.MoveImmediateEAXReached
                && !parent.SubtractReached
                && parent.FlagsAtStop == parent.XorFlags;
        }

        if (parent.Outcome
            != CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4)
        {
            return false;
        }

        uint shifted = ecx >> 1;
        uint countResult = unchecked(
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.NonEqualEAXImmediate
            - shifted);
        return ecx
                != CapacityFinalCalleeCommonIndexedCountClassificationProjection.CompareImmediate
            && parent.CompareFlags.Value.Zero == false
            && parent.JnzTaken == true
            && parent.JnzSelectedDestinationRawFileOffset
                == CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .JnzBranchTargetRawFileOffset
            && parent.ShiftECXReached
            && parent.ShiftInputECX == ecx
            && parent.ShiftResultECX == shifted
            && parent.MoveImmediateEAXReached
            && parent.MoveImmediateEAXResult
                == CapacityFinalCalleeCommonIndexedCountClassificationProjection.NonEqualEAXImmediate
            && parent.SubtractReached
            && parent.SubtractLeft
                == CapacityFinalCalleeCommonIndexedCountClassificationProjection.NonEqualEAXImmediate
            && parent.SubtractRight == shifted
            && parent.SubtractResult == countResult
            && !parent.XorEAXReached
            && !parent.JumpReached
            && parent.FlagsAtStop == parent.SubtractFlags;
    }

    private static ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>
        CarryFlagsThroughChild(
            CapacityFinalCalleeCommonMaskedTagEqualityGateFlags incomingFlags,
            CapacityFinalCalleeCommonMaskedTagEqualityGatePlan child)
    {
        ImmutableArray<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>.Builder events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonMaskedTagEqualityGateEvent>(
                child.OrderedEvents.Length);
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags flags = incomingFlags;
        foreach (CapacityFinalCalleeCommonMaskedTagEqualityGateEvent instruction in child.OrderedEvents)
        {
            flags = instruction.Stage switch
            {
                CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MoveECXFromEAX
                or CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MoveEBXFromEDI
                or CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage
                    .FreshEDXPlus4MoveToEAX => flags,
                CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MaskEAXLowTagBits =>
                    child.MaskFlags ?? flags,
                CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.ShiftEBXByCL =>
                    child.ShiftFlags ?? flags,
                CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.CompareEAXAndEDI =>
                    child.CompareFlags ?? flags,
                CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.Jz => flags,
                _ => flags
            };
            events.Add(instruction with { FlagsAfter = flags });
        }

        return events.ToImmutable();
    }

    private static CapacityFinalCalleeCommonMaskedTagEqualityGateFlags ToChildFlags(
        CapacityFinalCalleeCommonIndexedCountClassificationFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.Auxiliary,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}
