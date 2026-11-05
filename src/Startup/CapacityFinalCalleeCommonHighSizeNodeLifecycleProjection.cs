using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP,
    uint? EntryESI);

public enum CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome
{
    MissingEntryEDIAt9354,
    MissingLateEntryEBPAt9374,
    ParentZeroAt9384,
    ParentAboveCeilingAt9384,
    MissingEntryESIAt9384,
    InitializedAt93A1,
    InvalidParentHandoff,
    InvalidNodeHandoff
}

public enum CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource
{
    ParentZeroXor,
    ParentAboveCeilingMove31,
    IndexChildFinalLEA
}

public enum CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
{
    ParentFrameStore,
    NodeFirstStore,
    NodeSecondStore,
    NodeThirdStore
}

public enum CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage
{
    Parent,
    Node
}

public readonly record struct CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite(
    CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage Stage,
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionEvidence(
    CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage Stage,
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan(
    CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture Capture,
    CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan? ParentLifecycle { get; init; }
    public CapacityFinalCalleeCommonIndexedNodeInitializationPlan? NodeLifecycle { get; init; }
    public bool ParentOutcomeValidated { get; init; }
    public bool ParentHandoffValidated { get; init; }
    public bool NodeHandoffReached { get; init; }
    public bool UnreachedEntryESICaptureProvided =>
        NodeLifecycle is null && Capture.EntryESI.HasValue;
    public CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource? ECXSource { get; init; }
    public uint? DerivedECX { get; init; }
    public bool LateEntryEBPConsumed { get; init; }
    public bool LateEntryEBPProvidedButUnused { get; init; }
    public bool LateEntryEBPUnusedOnBypass { get; init; }
    public bool FrameStoreAttempted { get; init; }
    public uint? FrameStoreAddress { get; init; }
    public uint? FrameStoreValue { get; init; }
    public bool FrameStoreCommitted { get; init; }
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public uint? DerivedEBX { get; init; }
    public uint? DerivedEDX { get; init; }
    public CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? FlagsAtNodeEntry { get; init; }
    public CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? FlagsAtStop { get; init; }
    public bool NodeFlagsModified => false;
    public bool GlobalReadPerformed => false;
    public bool InstructionBeyondBoundaryExecuted => false;
    public ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> Writes
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite>.Empty;
    public bool ExactCleanAdmission { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Composes the exact 9384 exits of the high-size index lifecycle with the
/// bounded indexed-node initialization stores through the exclusive 93A1 boundary.
/// </summary>
public static class CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.EntryRawFileOffset;
    public const uint NodeEntryRawFileOffset =
        CapacityFinalCalleeCommonIndexedNodeInitializationProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeCommonIndexedNodeInitializationProjection
            .BoundaryRawFileOffsetExclusive;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
            .CodeVirtualAddressDeltaFromRawFileOffset;

    public static ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionEvidence>
        Instructions { get; } =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Instructions
                .Select(instruction =>
                    new CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionEvidence(
                        CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage.Parent,
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.FlatIdaAddress,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .Concat(
                    CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Instructions
                        .Select(instruction =>
                            new CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionEvidence(
                                CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage.Node,
                                instruction.Observation,
                                instruction.RawFileOffset,
                                instruction.FlatIdaAddress,
                                instruction.VirtualAddress,
                                instruction.Bytes)))
                .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan Project(
        CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(capture.EntryEDI, capture.LateEntryEBP));

        if (IsExactMissingEntry(parent, capture))
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryEDIAt9354,
                parent,
                parent.StopAtRawFileOffsetExclusive,
                parentOutcomeValidated: true);
        }

        if (IsExactMissingFramePointer(parent))
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingLateEntryEBPAt9374,
                parent,
                parent.StopAtRawFileOffsetExclusive,
                parentOutcomeValidated: true);
        }

        if (IsExactZeroExit(parent))
        {
            return ProjectNode(
                capture,
                parent,
                0,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.ParentZeroXor);
        }

        if (IsExactAboveCeilingExit(parent))
        {
            return ProjectNode(
                capture,
                parent,
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AbovePathECX,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource
                    .ParentAboveCeilingMove31);
        }

        if (IsExactIndexedExit(parent, out uint finalECX))
        {
            return ProjectNode(
                capture,
                parent,
                finalECX,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA);
        }

        return Stop(
            capture,
            CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InvalidParentHandoff,
            parent,
            parent.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan ProjectNode(
        CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture capture,
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent,
        uint ecx,
        CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource ecxSource)
    {
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan node =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.Project(
                new(capture.EntryESI, ecx));
        bool completed =
            node.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.Completed
            && capture.EntryESI is uint entryESI
            && node.Capture.EntryESI == entryESI
            && node.Capture.EntryECX == ecx
            && node.StopAtRawFileOffsetExclusive == BoundaryRawFileOffsetExclusive
            && node.FirstStoreReached
            && node.FirstStoreAddress
                == unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .FirstStoreFieldOffset)
            && node.FirstStoreValue == ecx
            && node.LEAReached
            && node.DerivedEBX
                == CapacityFinalCalleeCommonIndexedNodeInitializationProjection.DeriveEBX(ecx)
            && node.SecondStoreAddress
                == unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .SecondStoreFieldOffset)
            && node.SecondStoreValue == 0
            && node.SecondStoreReached
            && node.MoveEDXReached
            && node.DerivedEDX
                == CapacityFinalCalleeCommonIndexedNodeInitializationProjection.MoveEDXImmediate
            && node.ThirdStoreAddress
                == unchecked(entryESI
                    + CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                        .ThirdStoreFieldOffset)
            && node.ThirdStoreValue == 0
            && node.ThirdStoreReached
            && !node.InstructionBeyondBoundaryExecuted
            && node.Writes.Select(write => write.RawFileOffset).SequenceEqual(new uint[]
            {
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection.FirstStoreRawFileOffset,
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection.SecondStoreRawFileOffset,
                CapacityFinalCalleeCommonIndexedNodeInitializationProjection.ThirdStoreRawFileOffset
            })
            && node.Writes.Select(write => write.Value).SequenceEqual(new[] { ecx, 0u, 0u });
        CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome outcome =
            completed
                ? CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
                : node.Outcome
                    == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryESI
                    && node.StopAtRawFileOffsetExclusive == NodeEntryRawFileOffset
                    ? CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome
                        .MissingEntryESIAt9384
                    : CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InvalidNodeHandoff;
        bool validNodeHandoff = completed
            || outcome == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome
                .MissingEntryESIAt9384
                && capture.EntryESI is null
                && node.Capture.EntryESI is null
                && node.Capture.EntryECX == ecx
                && node.FirstStoreValue == ecx
                && node.FirstStoreAddress is null
                && !node.FirstStoreReached
                && !node.LEAReached
                && node.Writes.IsEmpty;

        if (!validNodeHandoff)
        {
            return Stop(
                capture,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InvalidNodeHandoff,
                parent,
                node.StopAtRawFileOffsetExclusive,
                parentOutcomeValidated: true,
                parentHandoffValidated: true,
                node: node,
                ecxSource: ecxSource,
                derivedECX: ecx);
        }

        return Stop(
            capture,
            outcome,
            parent,
            node.StopAtRawFileOffsetExclusive,
            parentOutcomeValidated: true,
            parentHandoffValidated: true,
            nodeHandoffReached: true,
            node: node,
            ecxSource: ecxSource,
            derivedECX: ecx,
            exactAdmission: completed
                && parent.ExactCleanAdmission
                && !parent.LateEntryEBPProvidedButUnused);
    }

    private static CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan Stop(
        CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture capture,
        CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome outcome,
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent,
        uint stopAt,
        bool parentOutcomeValidated = false,
        bool parentHandoffValidated = false,
        bool nodeHandoffReached = false,
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? node = null,
        CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource? ecxSource = null,
        uint? derivedECX = null,
        bool exactAdmission = false) =>
        new(capture, outcome)
        {
            ParentLifecycle = parent,
            NodeLifecycle = node,
            ParentOutcomeValidated = parentOutcomeValidated,
            ParentHandoffValidated = parentHandoffValidated,
            NodeHandoffReached = nodeHandoffReached,
            ECXSource = ecxSource,
            DerivedECX = derivedECX,
            LateEntryEBPConsumed = parent.LateEntryEBPConsumed,
            LateEntryEBPProvidedButUnused = parent.LateEntryEBPProvidedButUnused,
            LateEntryEBPUnusedOnBypass = parent.LateEntryEBPUnusedOnBypass,
            FrameStoreAttempted = parent.FrameStoreAttempted,
            FrameStoreAddress = parent.FrameStoreAddress,
            FrameStoreValue = parent.FrameStoreValue,
            FrameStoreCommitted = parent.FrameStoreCommitted,
            FirstStoreReached = node?.FirstStoreReached == true,
            FirstStoreAddress = node?.FirstStoreAddress,
            FirstStoreValue = node?.FirstStoreValue,
            DerivedEBX = node?.DerivedEBX,
            DerivedEDX = node?.DerivedEDX,
            FlagsAtNodeEntry = nodeHandoffReached ? parent.FlagsAtStop : null,
            FlagsAtStop = parent.FlagsAtStop,
            Writes = BuildWrites(parent, node),
            ExactCleanAdmission = exactAdmission,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> BuildWrites(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent,
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? node)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite>();
        if (parent.FrameStoreCommitted
            && parent.FrameStoreAddress is uint frameAddress
            && parent.FrameStoreValue is uint frameValue)
        {
            writes.Add(new(
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore,
                CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreRawFileOffset,
                frameAddress,
                frameValue));
        }

        if (node is not null)
        {
            foreach (CapacityFinalCalleeCommonIndexedNodeInitializationWrite write in node.Writes)
            {
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage stage =
                    write.RawFileOffset switch
                    {
                        CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                            .FirstStoreRawFileOffset =>
                            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore,
                        CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                            .SecondStoreRawFileOffset =>
                            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore,
                        CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                            .ThirdStoreRawFileOffset =>
                            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore,
                        _ => throw new ArgumentOutOfRangeException(nameof(node))
                    };
                writes.Add(new(stage, write.RawFileOffset, write.Address, write.Value));
            }
        }

        return writes.ToImmutable();
    }

    private static bool IsExactMissingEntry(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent,
        CapacityFinalCalleeCommonHighSizeNodeLifecycleCapture capture)
    {
        return capture.EntryEDI is null
            && parent.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.MissingEntryEDIAt9354
            && parent.ParentLifecycle is
            {
                Outcome: CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .MissingEntryEDIAt9354
            } parentLifecycle
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.Events.IsEmpty
            && parent.IndexLifecycle is null
            && !parent.ChildEntered
            && !parent.LateEntryEBPConsumed
            && parent.LateEntryEBPProvidedButUnused
                == capture.LateEntryEBP.HasValue
            && parent.LateEntryEBPUnusedOnBypass
                == capture.LateEntryEBP.HasValue
            && parent.Writes.IsEmpty
            && parentLifecycle.Writes.IsEmpty;
    }

    private static bool IsExactMissingFramePointer(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan? parentLifecycle =
            parent.ParentLifecycle;
        CapacityFinalCalleeCommonHighSizeBitScanPlan? index = parent.IndexLifecycle;
        if (parentLifecycle is null || index is null)
        {
            return false;
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .MissingLateEntryEBPAt9374
            || parentLifecycle.Outcome
                != CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildBelowOrEqualAt936FBeforeBSR
            || index.Outcome
                != CapacityFinalCalleeCommonHighSizeBitScanOutcome.MissingLateEntryEBPAt9374
            || !parent.ChildEntered
            || parent.DerivedChildEntryEAX is not uint childEAX
            || childEAX == 0
            || childEAX > CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Ceiling
            || !index.BSRReached
            || index.BSRInputEAX != childEAX
            || index.BSRDestinationDefined != true
            || !parent.FrameStoreAttempted
            || parent.FrameStoreCommitted
            || parent.FrameStoreAddress.HasValue
            || parent.Capture.LateEntryEBP.HasValue
            || parent.LateEntryEBPProvidedButUnused
            || parent.FrameStoreValue != index.BSRResultEAX
            || !parent.FrameStoreValue.HasValue
            || parent.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreRawFileOffset
            || !parent.Writes.IsEmpty
            || !index.Writes.IsEmpty
            || parent.FlagsAtStop != ConvertIndexFlags(index.BSRFlags))
        {
            return false;
        }

        return true;
    }

    private static bool IsExactZeroExit(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent)
    {
        return parent.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.ParentZeroAt9384
            && parent.ParentLifecycle is
            {
                Outcome: CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.ParentZeroAt9384
            } parentLifecycle
            && parentLifecycle.ExactCleanAdmission
            && parentLifecycle.ParentShiftReached
            && parentLifecycle.ParentShiftResultEAX == 0
            && parentLifecycle.ParentJNZReached
            && parentLifecycle.ParentJNZTaken == false
            && parentLifecycle.ParentXorReached
            && parentLifecycle.ParentXorResultECX == 0
            && parentLifecycle.FinalSelectedDestinationRawFileOffset == NodeEntryRawFileOffset
            && parent.SelectedDestinationRawFileOffset == NodeEntryRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == NodeEntryRawFileOffset
            && !parent.ChildEntered
            && parent.IndexLifecycle is null
            && parent.Writes.IsEmpty
            && parent.Reads.IsEmpty
            && parentLifecycle.Writes.IsEmpty
            && !parent.TargetEntered
            && parent.FlagsAtStop == ConvertParentFlags(parentLifecycle.FlagsAtStop)
            && !parent.LateEntryEBPConsumed
            && parent.LateEntryEBPProvidedButUnused
                == parent.Capture.LateEntryEBP.HasValue
            && parent.LateEntryEBPUnusedOnBypass
                == parent.Capture.LateEntryEBP.HasValue;
    }

    private static bool IsExactAboveCeilingExit(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan? parentLifecycle =
            parent.ParentLifecycle;
        if (parentLifecycle is null)
        {
            return false;
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .ParentAboveCeilingAt9384
            || parentLifecycle.Outcome
                != CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildAboveCeilingAt9384BeforeStore
            || !parentLifecycle.ExactCleanAdmission
            || !parentLifecycle.ChildEntered
            || parentLifecycle.ChildEntryEAX is not uint childEAX
            || childEAX <= CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Ceiling
            || parentLifecycle.ChildCompareInputEAX != childEAX
            || parentLifecycle.ChildCompareImmediate
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Ceiling
            || !parentLifecycle.ChildJBEReached
            || parentLifecycle.ChildJBETaken != false
            || !parentLifecycle.ChildMoveReached
            || parentLifecycle.ChildMoveResultECX
                != CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AbovePathECX
            || !parentLifecycle.ChildJMPReached
            || parentLifecycle.ChildJMPTaken != true
            || parentLifecycle.ChildJMPSelectedDestinationRawFileOffset != NodeEntryRawFileOffset
            || parent.SelectedDestinationRawFileOffset != NodeEntryRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != NodeEntryRawFileOffset
            || parent.IndexLifecycle is not null
            || !parent.Writes.IsEmpty
            || !parent.Reads.IsEmpty
            || !parentLifecycle.Writes.IsEmpty
            || parent.TargetEntered
            || parent.FlagsAtStop != ConvertParentFlags(parentLifecycle.FlagsAtStop)
            || parent.LateEntryEBPConsumed
            || parent.LateEntryEBPProvidedButUnused
                != parent.Capture.LateEntryEBP.HasValue
            || parent.LateEntryEBPUnusedOnBypass
                != parent.Capture.LateEntryEBP.HasValue)
        {
            return false;
        }

        return true;
    }

    private static bool IsExactIndexedExit(
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan parent,
        out uint finalECX)
    {
        finalECX = 0;
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan? parentLifecycle =
            parent.ParentLifecycle;
        CapacityFinalCalleeCommonHighSizeBitScanPlan? index = parent.IndexLifecycle;
        if (parentLifecycle is null || index is null)
        {
            return false;
        }

        if (parent.Outcome
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .IndexedResultAt9384BeforeFollowingStore
            || !parent.ExactCleanAdmission
            || parentLifecycle.Outcome
                != CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildBelowOrEqualAt936FBeforeBSR
            || !parentLifecycle.ExactCleanAdmission
            || !parentLifecycle.ChildEntered
            || parentLifecycle.ChildEntryEAX is not uint childEAX
            || childEAX == 0
            || childEAX
                > CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Ceiling
            || parentLifecycle.ChildCompareInputEAX != childEAX
            || parentLifecycle.ChildCompareImmediate
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Ceiling
            || !parentLifecycle.ChildJBEReached
            || parentLifecycle.ChildJBETaken != true
            || parentLifecycle.ChildJBESelectedDestinationRawFileOffset
                != CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.IndexEntryRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != NodeEntryRawFileOffset
            || parent.SelectedDestinationRawFileOffset !=
                CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.IndexEntryRawFileOffset
            || index.Outcome
                != CapacityFinalCalleeCommonHighSizeBitScanOutcome
                    .IndexedResultAt9384BeforeNextStore
            || index.Capture.EntryEAX != childEAX
            || !index.BSRReached
            || index.BSRDestinationDefined != true
            || !index.StoreCommitted
            || !index.MoveEDIToEAXReached
            || !index.ShiftReached
            || !index.AndReached
            || !index.FinalLeaReached
            || index.FinalIndexECX is not uint indexECX
            || indexECX > 31
            || index.StopAtRawFileOffsetExclusive != NodeEntryRawFileOffset
            || index.Capture.LateEntryEBP is null
            || index.Capture.LateEntryEDI != parent.Capture.EntryEDI
            || !parent.FrameStoreCommitted
            || !parent.LateEntryEBPConsumed
            || parent.LateEntryEBPProvidedButUnused
            || parent.FrameStoreAddress is null
            || parent.FrameStoreValue != index.BSRResultEAX
            || !parent.Writes.SequenceEqual(index.Writes)
            || parent.FlagsAtStop != ConvertIndexFlags(index.AndFlags))
        {
            return false;
        }

        finalECX = indexECX;
        return true;
    }

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertParentFlags(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertIndexFlags(
        CapacityFinalCalleeCommonHighSizeBitScanFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeBitScanFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
