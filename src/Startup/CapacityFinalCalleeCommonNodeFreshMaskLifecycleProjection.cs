using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP,
    uint? EntryESI,
    uint? LateFreshGlobalValue);

public enum CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
{
    MissingEntryEDIAt9354,
    MissingLateEntryEBPAt9374,
    MissingEntryESIAt9384,
    MissingFreshGlobalValueAt93A1,
    ZeroTestResultToBounded93AC,
    NonzeroTestResultToBounded93C0,
    ParentNotCleanAt93A1,
    InvalidParentHandoff,
    InvalidNodeHandoff,
    InvalidFreshMaskHandoff
}

public enum CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind
{
    Write,
    FreshGlobalRead
}

public readonly record struct CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value,
    CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage? WriteStage,
    bool ValueCaptured,
    bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan(
    CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan? ParentLifecycle { get; init; }
    public CapacityFinalCalleeCommonFreshMaskGatePlan? FreshMaskGate { get; init; }
    public bool ParentOutcomeValidated { get; init; }
    public bool ParentNodeInitializationCompleted { get; init; }
    public bool NodeFreshMaskHandoffValidated { get; init; }
    public bool ParentExactCleanAdmission { get; init; }
    public bool ExactCleanHandoff { get; init; }
    public uint? ChildEDX { get; init; }
    public uint? ChildECX { get; init; }
    public bool ChildEDXDerivedFromNodeMove { get; init; }
    public bool ChildECXDerivedFromParentIndex { get; init; }
    public bool FreshMaskGateReached { get; init; }
    public bool Internal93A1Entered { get; init; }
    public bool FreshGlobalReadReached { get; init; }
    public bool FreshGlobalValueCaptured { get; init; }
    public uint? FreshGlobalReadAddress =>
        FreshGlobalReadReached
            ? CapacityFinalCalleeCommonFreshMaskGateProjection.FreshGlobalVirtualAddress
            : null;
    public bool LateFreshGlobalValueProvidedButUnused { get; init; }
    public bool UnreachedLateFreshGlobalValueCaptureProvided { get; init; }
    public bool FreshGlobalReadAfterAllNodeWrites { get; init; }
    public bool FreshGlobalValueForwardedFromNodeWrite => false;
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> Writes
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect>.Empty;
    public CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? FlagsAtGateEntry { get; init; }
    public CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? FlagsAfterShift { get; init; }
    public CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? FlagsAtStop { get; init; }
    public bool ShiftCountZeroPreservedParentFlags { get; init; }
    public bool TestOverwroteFlags { get; init; }
    public bool BoundedBranchDestinationSelected { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool ZeroDestinationInstructionEntered => false;
    public bool NonzeroDestinationInstructionEntered => false;
    public bool InstructionBeyondBoundaryExecuted => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the completed node stores at 93A1 with the independent fresh-global mask gate.
/// </summary>
public static class CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.EntryRawFileOffset;
    public const uint NodeInitializationBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.BoundaryRawFileOffsetExclusive;
    public const uint FreshGlobalMoveRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.FreshGlobalMoveRawFileOffset;
    public const uint FreshGlobalVirtualAddress =
        CapacityFinalCalleeCommonFreshMaskGateProjection.FreshGlobalVirtualAddress;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.ZeroDestinationRawFileOffset;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.NonzeroDestinationRawFileOffset;

    public static CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan parent =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(capture.EntryEDI, capture.LateEntryEBP, capture.EntryESI));

        if (parent.Outcome
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryEDIAt9354)
        {
            return StopBeforeGate(
                capture,
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryEDIAt9354,
                parent);
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingLateEntryEBPAt9374)
        {
            return StopBeforeGate(
                capture,
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingLateEntryEBPAt9374,
                parent);
        }

        if (parent.Outcome
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryESIAt9384)
        {
            return StopBeforeGate(
                capture,
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryESIAt9384,
                parent);
        }

        if (parent.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && !parent.ExactCleanAdmission)
        {
            return StopBeforeGate(
                capture,
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.ParentNotCleanAt93A1,
                parent);
        }

        if (!IsCompletedNodePath(parent, capture, out uint childECX, out uint childEDX))
        {
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome invalidOutcome =
                parent.ParentOutcomeValidated
                && parent.ParentHandoffValidated
                    ? CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidNodeHandoff
                    : CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidParentHandoff;
            return StopBeforeGate(capture, invalidOutcome, parent);
        }

        CapacityFinalCalleeCommonFreshMaskGatePlan gate =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(capture.LateFreshGlobalValue, childEDX, childECX));

        if (!IsExactFreshMaskHandoff(parent, gate, childECX, childEDX))
        {
            return StopAtGate(
                capture,
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidFreshMaskHandoff,
                parent,
                gate,
                childECX,
                childEDX,
                handoffValidated: false);
        }

        CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome outcome =
            gate.Outcome switch
            {
                CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingFreshGlobalValue =>
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                        .MissingFreshGlobalValueAt93A1,
                CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC =>
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                        .ZeroTestResultToBounded93AC,
                CapacityFinalCalleeCommonFreshMaskGateOutcome.NonzeroTestResultToBounded93C0 =>
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                        .NonzeroTestResultToBounded93C0,
                _ => CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .InvalidFreshMaskHandoff
            };

        return StopAtGate(
            capture,
            outcome,
            parent,
            gate,
            childECX,
            childEDX,
            handoffValidated: outcome
                != CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .InvalidFreshMaskHandoff);
    }

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan StopBeforeGate(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome outcome,
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan parent) =>
        new(capture, outcome)
        {
            ParentLifecycle = parent,
            ParentOutcomeValidated = parent.ParentOutcomeValidated,
            ParentNodeInitializationCompleted = parent.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1,
            ParentExactCleanAdmission = parent.ExactCleanAdmission,
            LateFreshGlobalValueProvidedButUnused = capture.LateFreshGlobalValue.HasValue,
            UnreachedLateFreshGlobalValueCaptureProvided =
                capture.LateFreshGlobalValue.HasValue,
            Writes = parent.Writes,
            OrderedMemoryEffects = BuildEffects(parent.Writes, null),
            FlagsAtStop = ConvertFlags(parent.FlagsAtStop),
            StopAtRawFileOffsetExclusive = parent.StopAtRawFileOffsetExclusive
        };

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan StopAtGate(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome outcome,
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan parent,
        CapacityFinalCalleeCommonFreshMaskGatePlan gate,
        uint childECX,
        uint childEDX,
        bool handoffValidated)
    {
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? parentFlags =
            ConvertFlags(parent.FlagsAtStop);
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? flagsAfterShift =
            !gate.ShiftExecuted
                ? null
                : gate.ShiftCountMasked == 0
                    ? parentFlags
                    : ConvertFlags(gate.ShiftFlags);
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? flagsAtStop =
            gate.TestReached
                ? ConvertFlags(gate.TestFlags)
                : gate.ShiftExecuted
                    ? flagsAfterShift
                    : parentFlags;
        ImmutableArray<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect> effects =
            BuildEffects(parent.Writes, gate);
        bool exactCleanHandoff = handoffValidated && parent.ExactCleanAdmission;

        return new(capture, outcome)
        {
            ParentLifecycle = parent,
            FreshMaskGate = gate,
            ParentOutcomeValidated = parent.ParentOutcomeValidated,
            ParentNodeInitializationCompleted = true,
            NodeFreshMaskHandoffValidated = handoffValidated,
            ParentExactCleanAdmission = parent.ExactCleanAdmission,
            ExactCleanHandoff = exactCleanHandoff,
            ChildEDX = childEDX,
            ChildECX = childECX,
            ChildEDXDerivedFromNodeMove = handoffValidated
                && parent.NodeLifecycle?.DerivedEDX == childEDX
                && childEDX
                    == CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .MoveEDXImmediate,
            ChildECXDerivedFromParentIndex = handoffValidated
                && parent.DerivedECX == childECX
                && parent.ECXSource.HasValue,
            FreshMaskGateReached = gate.FreshGlobalReadReached,
            Internal93A1Entered = gate.FreshGlobalReadReached,
            FreshGlobalReadReached = gate.FreshGlobalReadReached,
            FreshGlobalValueCaptured = gate.FreshGlobalValueCaptured,
            LateFreshGlobalValueProvidedButUnused =
                capture.LateFreshGlobalValue.HasValue && !gate.FreshGlobalReadReached,
            Writes = parent.Writes,
            OrderedMemoryEffects = effects,
            FreshGlobalReadAfterAllNodeWrites = gate.FreshGlobalReadReached
                && IsAllNodeWritesBeforeRead(parent.Writes, effects),
            FlagsAtGateEntry = parentFlags,
            FlagsAfterShift = flagsAfterShift,
            FlagsAtStop = flagsAtStop,
            ShiftCountZeroPreservedParentFlags = gate.ShiftExecuted
                && gate.ShiftCountMasked == 0
                && flagsAfterShift == parentFlags,
            TestOverwroteFlags = gate.TestReached
                && flagsAtStop == ConvertFlags(gate.TestFlags),
            BoundedBranchDestinationSelected = gate.JnzReached
                && gate.ExactCleanBoundedDestination
                && gate.JnzSelectedDestinationRawFileOffset
                    is ZeroDestinationRawFileOffset or NonzeroDestinationRawFileOffset,
            ExactCleanExitAdmitted = exactCleanHandoff && gate.ExactCleanExitAdmitted,
            StopAtRawFileOffsetExclusive = gate.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsCompletedNodePath(
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan parent,
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleCapture capture,
        out uint childECX,
        out uint childEDX)
    {
        childECX = 0;
        childEDX = 0;
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? node = parent.NodeLifecycle;
        if (parent.Outcome
                != CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            || !parent.ParentOutcomeValidated
            || !parent.ParentHandoffValidated
            || !parent.NodeHandoffReached
            || parent.UnreachedEntryESICaptureProvided
            || !parent.ExactCleanAdmission
            || parent.LateEntryEBPProvidedButUnused
            || parent.LateEntryEBPUnusedOnBypass
            || parent.LateEntryEBPConsumed != capture.LateEntryEBP.HasValue
            || parent.FrameStoreAttempted != capture.LateEntryEBP.HasValue
            || parent.FrameStoreCommitted != capture.LateEntryEBP.HasValue
            || (!parent.FrameStoreCommitted
                && (parent.FrameStoreAddress.HasValue || parent.FrameStoreValue.HasValue))
            || parent.InstructionBeyondBoundaryExecuted
            || parent.NodeFlagsModified
            || parent.GlobalReadPerformed
            || node is null
            || node.Outcome
                != CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.Completed
            || capture.EntryESI is not uint entryESI
            || parent.DerivedECX is not uint derivedECX
            || parent.DerivedEDX is not uint derivedEDX
            || node.Capture.EntryESI != entryESI
            || node.Capture.EntryECX != derivedECX
            || !node.EntryESICaptured
            || !node.EntryECXCaptured
            || node.FlagsModified
            || node.FlagsAtStopKnown
            || node.MemoryReadPerformed
            || !node.FirstStoreReached
            || !node.LEAReached
            || !node.AddressOnlyLEA
            || !node.MoveEDXReached
            || !node.SecondStoreReached
            || !node.ThirdStoreReached
            || parent.ECXSource is null
            || !parent.FirstStoreReached
            || parent.FirstStoreValue != derivedECX
            || parent.FirstStoreAddress != node.FirstStoreAddress
            || parent.DerivedEBX != node.DerivedEBX
            || node.FirstStoreValue != derivedECX
            || node.FirstStoreAddress
                != unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .FirstStoreFieldOffset)
            || node.DerivedEDX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection.MoveEDXImmediate
            || node.LEAIndex != derivedECX
            || node.LEAScaledIndex != unchecked(derivedECX * 4)
            || node.LEABaseAddress
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection.LEABaseVirtualAddress
            || node.DerivedEBX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .DeriveEBX(derivedECX)
            || node.SecondStoreAddress
                != unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .SecondStoreFieldOffset)
            || node.SecondStoreValue != 0
            || node.ThirdStoreAddress
                != unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .ThirdStoreFieldOffset)
            || node.ThirdStoreValue != 0
            || node.Writes.Length != 3
            || node.Writes.Select(write => write.RawFileOffset).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .FirstStoreRawFileOffset,
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .SecondStoreRawFileOffset,
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .ThirdStoreRawFileOffset
            }) == false
            || node.Writes.Select(write => write.Address).SequenceEqual(new[]
            {
                unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .FirstStoreFieldOffset),
                unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .SecondStoreFieldOffset),
                unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .ThirdStoreFieldOffset)
            }) == false
            || node.Writes.Select(write => write.Value)
                .SequenceEqual(new[] { derivedECX, 0u, 0u }) == false
            || node.InstructionBeyondBoundaryExecuted
            || parent.StopAtRawFileOffsetExclusive != NodeInitializationBoundaryRawFileOffsetExclusive
            || node.StopAtRawFileOffsetExclusive != NodeInitializationBoundaryRawFileOffsetExclusive
            || parent.Writes.Count(write =>
                write.Stage is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeFirstStore or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeSecondStore or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeThirdStore) != 3
            || parent.Writes.Length != 3 + (parent.FrameStoreCommitted ? 1 : 0))
        {
            return false;
        }

        ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> nodeWrites =
            parent.Writes.Where(write =>
                write.Stage is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeFirstStore or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeSecondStore or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                    .NodeThirdStore).ToImmutableArray();
        if (!nodeWrites.Select(write => write.Stage).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore
            })
            || !nodeWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(node.Writes.Select(write => write.RawFileOffset))
            || !nodeWrites.Select(write => write.Address)
                .SequenceEqual(node.Writes.Select(write => write.Address))
            || !nodeWrites.Select(write => write.Value)
                .SequenceEqual(node.Writes.Select(write => write.Value)))
        {
            return false;
        }

        if (parent.FrameStoreCommitted
            && (parent.Writes[0].Stage
                    != CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore
                || parent.Writes[0].RawFileOffset
                    != CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreRawFileOffset
                || parent.Writes[0].Address != parent.FrameStoreAddress
                || parent.Writes[0].Value != parent.FrameStoreValue))
        {
            return false;
        }

        if (parent.ECXSource
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA
            && parent.ParentLifecycle?.IndexLifecycle?.FinalIndexECX != derivedECX)
        {
            return false;
        }

        if (parent.ECXSource
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.ParentZeroXor
            && derivedECX != 0)
        {
            return false;
        }

        if (parent.ECXSource
            == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.ParentAboveCeilingMove31
            && derivedECX != CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AbovePathECX)
        {
            return false;
        }

        childECX = derivedECX;
        childEDX = derivedEDX;
        return true;
    }

    private static bool IsExactFreshMaskHandoff(
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan parent,
        CapacityFinalCalleeCommonFreshMaskGatePlan gate,
        uint childECX,
        uint childEDX)
    {
        if (!parent.ExactCleanAdmission
            || gate.Capture.EntryECX != childECX
            || gate.Capture.EntryEDX != childEDX
            || parent.DerivedECX != childECX
            || parent.NodeLifecycle?.DerivedEDX != childEDX
            || childEDX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection.MoveEDXImmediate
            || !gate.FreshGlobalReadReached
            || gate.FreshGlobalReadAddress != FreshGlobalVirtualAddress
            || gate.ReadAttempts.Length != 1
            || gate.ReadAttempts[0].RawFileOffset != FreshGlobalMoveRawFileOffset
            || gate.ReadAttempts[0].AbsoluteAddress != FreshGlobalVirtualAddress
            || gate.ReadAttempts[0].Value != gate.Capture.FreshGlobalValue
            || !gate.ReadAttempts[0].ReadReached
            || gate.MemoryWritePerformed)
        {
            return false;
        }

        if (gate.Outcome
            == CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingFreshGlobalValue)
        {
            return gate.Capture.FreshGlobalValue is null
                && !gate.FreshGlobalValueCaptured
                && !gate.ReadAttempts[0].ValueCaptured
                && gate.OrderedEvents.Length == 1
                && gate.OrderedEvents[0] is
                {
                    Stage: CapacityFinalCalleeCommonFreshMaskGateInstructionStage.FreshGlobalMove,
                    Reached: true,
                    Completed: false
                }
                && gate.UnreachedEntryEDXCaptureProvided
                && gate.UnreachedEntryECXCaptureProvided
                && gate.EntryEDXCaptureProvidedButUnused
                && gate.EntryECXCaptureProvidedButUnused
                && !gate.EntryEDXOperandConsumed
                && !gate.EntryECXOperandConsumed
                && !gate.ShiftInstructionReached
                && !gate.ShiftExecuted
                && !gate.TestReached
                && !gate.JnzReached
                && !gate.ExactCleanBoundedDestination
                && !gate.ExactCleanExitAdmitted
                && gate.JnzSelectedDestinationRawFileOffset is null
                && gate.OrderedEvents[0].EaxValue is null
                && gate.StopAtRawFileOffsetExclusive == FreshGlobalMoveRawFileOffset;
        }

        bool expectedZero = gate.Outcome
            == CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC;
        bool expectedNonzero = gate.Outcome
            == CapacityFinalCalleeCommonFreshMaskGateOutcome.NonzeroTestResultToBounded93C0;
        if (gate.Capture.FreshGlobalValue is not uint globalValue)
        {
            return false;
        }

        byte shiftCount = (byte)(childECX & 31);
        uint expectedShiftResult = unchecked(childEDX << shiftCount);
        uint expectedTestResult = globalValue & expectedShiftResult;
        uint expectedDestination = expectedTestResult == 0
            ? ZeroDestinationRawFileOffset
            : NonzeroDestinationRawFileOffset;
        if ((!expectedZero && !expectedNonzero)
            || expectedZero != (expectedTestResult == 0)
            || expectedNonzero != (expectedTestResult != 0)
            || !gate.FreshGlobalValueCaptured
            || !gate.ReadAttempts[0].ValueCaptured
            || gate.OrderedEvents.Length != 4
            || !gate.ShiftInstructionReached
            || !gate.ShiftExecuted
            || gate.ShiftCountSource != childECX
            || gate.ShiftCountLowByte != (byte)childECX
            || gate.ShiftCountMasked != shiftCount
            || gate.ShiftInputEDX != childEDX
            || gate.ShiftResult != expectedShiftResult
            || !gate.TestReached
            || gate.TestResult != expectedTestResult
            || gate.FlagsAtStop != gate.TestFlags
            || !gate.JnzReached
            || gate.JnzTaken != expectedNonzero
            || gate.ExactCleanBoundedDestination != true
            || gate.JnzSelectedDestinationRawFileOffset != expectedDestination
            || gate.StopAtRawFileOffsetExclusive != expectedDestination
            || !gate.ExactCleanExitAdmitted
            || gate.EntryEDXCaptureProvidedButUnused
            || gate.EntryECXCaptureProvidedButUnused
            || gate.UnreachedEntryEDXCaptureProvided
            || gate.UnreachedEntryECXCaptureProvided
            || !gate.EntryEDXOperandConsumed
            || !gate.EntryECXOperandConsumed)
        {
            return false;
        }

        return gate.OrderedEvents[0] is
                {
                    Stage: CapacityFinalCalleeCommonFreshMaskGateInstructionStage.FreshGlobalMove,
                    Reached: true,
                    Completed: true,
                    EaxValue: var eventGlobalValue
                }
            && eventGlobalValue == globalValue
            && gate.OrderedEvents[1] is
                {
                    Stage: CapacityFinalCalleeCommonFreshMaskGateInstructionStage.ShiftEDXByCL,
                    Reached: true,
                    Completed: true,
                    EdxValue: var eventEDX,
                    ShiftCount: var eventCount,
                    Result: var eventShiftResult
                }
            && eventEDX == childEDX
            && eventCount == shiftCount
            && eventShiftResult == expectedShiftResult
            && gate.OrderedEvents[2] is
                {
                    Stage: CapacityFinalCalleeCommonFreshMaskGateInstructionStage.TestEAXAndEDX,
                    Reached: true,
                    Completed: true,
                    EaxValue: var eventEAX,
                    EdxValue: var eventTestEDX,
                    Result: var eventTestResult
                }
            && eventEAX == globalValue
            && eventTestEDX == expectedShiftResult
            && eventTestResult == expectedTestResult
            && gate.OrderedEvents[3] is
                {
                    Stage: CapacityFinalCalleeCommonFreshMaskGateInstructionStage.Jnz,
                    Reached: true,
                    Completed: true,
                    BranchTaken: var eventBranch,
                    SelectedDestinationRawFileOffset: var eventDestination
                }
            && eventBranch == expectedNonzero
            && eventDestination == expectedDestination;
    }

    private static ImmutableArray<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect>
        BuildEffects(
            ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> writes,
            CapacityFinalCalleeCommonFreshMaskGatePlan? gate)
    {
        var effects =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect>(
                writes.Length + (gate?.FreshGlobalReadReached == true ? 1 : 0));
        foreach (CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite write in writes)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write,
                write.RawFileOffset,
                write.Address,
                write.Value,
                write.Stage,
                ValueCaptured: true,
                ForwardedFromPriorWrite: false));
        }

        if (gate?.ReadAttempts.FirstOrDefault() is
            CapacityFinalCalleeCommonFreshMaskGateReadAttempt
            {
                ReadReached: true
            } read)
        {
            effects.Add(new(
                CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.FreshGlobalRead,
                read.RawFileOffset,
                read.AbsoluteAddress,
                read.Value,
                WriteStage: null,
                ValueCaptured: read.ValueCaptured,
                ForwardedFromPriorWrite: false));
        }

        return effects.ToImmutable();
    }

    private static bool IsAllNodeWritesBeforeRead(
        ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> writes,
        ImmutableArray<CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect> effects)
    {
        int readIndex = -1;
        int lastNodeWriteIndex = -1;
        for (int index = 0; index < effects.Length; index++)
        {
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect effect = effects[index];
            if (effect.Kind
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.FreshGlobalRead)
            {
                readIndex = index;
            }

            if (effect.Kind == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write
                && effect.WriteStage
                    is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore)
            {
                lastNodeWriteIndex = index;
            }
        }
        return writes.Count(write =>
                write.Stage
                    is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore)
                == 3
            && readIndex == effects.Length - 1
            && lastNodeWriteIndex >= 0
            && lastNodeWriteIndex < readIndex;
    }

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonFreshMaskGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonFreshMaskGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
