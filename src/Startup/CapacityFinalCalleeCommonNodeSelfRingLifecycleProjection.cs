using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonNodeSelfRingLifecycleCapture(
    uint? EntryEDI,
    uint? LateEntryEBP,
    uint? EntryESI,
    uint? LateFreshGlobalValue);

public enum CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
{
    MissingEntryEDIAt9354,
    MissingLateEntryEBPAt9374,
    MissingEntryESIAt9384,
    ParentNotCleanAt93A1,
    MissingFreshGlobalValueAt93A1,
    NonzeroTestResultBypassesChild,
    InvalidParentHandoff,
    InvalidNodeHandoff,
    InvalidFreshMaskHandoff,
    InvalidZeroChildHandoff,
    MissingBothLatePointers,
    MissingLateEBX,
    MissingLateESI,
    Completed
}

public enum CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
{
    ParentFrameWrite,
    ParentNodeWrite,
    IndependentFreshGlobalRead,
    ChildGlobalWrite,
    ChildPointerStore
}

public readonly record struct CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect(
    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value,
    bool ValueCaptured,
    bool Committed,
    bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan(
    CapacityFinalCalleeCommonNodeSelfRingLifecycleCapture Capture,
    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan Parent { get; init; } = null!;
    public CapacityFinalCalleeCommonSelfRingTailPlan? Child { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect>
        OrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect>.Empty;

    public uint? ParentDerivedNodeEBX => Parent.ParentLifecycle?.DerivedEBX;
    public uint? ParentCapturedEntryESI => Parent.Capture.EntryESI;
    public uint? ChildEntryEAX => Child?.OrEAX;
    public uint? ChildEntryEDX => Child?.OrEDX;
    public uint? ChildEntryEBX => Child?.Capture.LateEBX;
    public uint? ChildEntryESI => Child?.Capture.LateESI;
    public bool ChildReached => Child is not null;
    public bool ChildORReached => Child?.OrReached == true;
    public bool ChildGlobalStoreReached => Child?.GlobalStoreReached == true;
    public bool ChildJumpReached => Child?.JumpReached == true;
    public bool ChildJumpTargetEntered => false;
    public uint? ChildJumpTargetRawFileOffset => Child?.JumpTargetRawFileOffset;
    public uint? ChildJumpTargetVirtualAddress => Child?.JumpTargetVirtualAddress;
    public bool Internal93ACEntered => ChildReached
        && CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset
            == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                .ChildEntryRawFileOffset;
    public bool ExactZeroChildHandoff =>
        CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
            .HasExactZeroChildHandoff(Parent);
    public bool ExactNonzeroParentBypass =>
        Parent.Outcome
            == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                .NonzeroTestResultToBounded93C0
        && Parent.ExactCleanHandoff
        && Parent.ExactCleanExitAdmitted
        && Parent.FreshGlobalReadReached
        && Parent.FreshGlobalValueCaptured
        && Parent.FreshGlobalReadAfterAllNodeWrites
        && Parent.FreshMaskGate is
        {
            Outcome: CapacityFinalCalleeCommonFreshMaskGateOutcome
                .NonzeroTestResultToBounded93C0,
            TestReached: true,
            JnzReached: true,
            JnzTaken: true,
            JnzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .NonzeroDestinationRawFileOffset,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .NonzeroDestinationRawFileOffset
        } gate
        && gate.TestResult is uint testResult
        && testResult != 0
        && !ChildReached
        && !Parent.LateFreshGlobalValueProvidedButUnused
        && !Parent.UnreachedLateFreshGlobalValueCaptureProvided
        && !Parent.ParentLifecycle!.LateEntryEBPProvidedButUnused
        && !Parent.ParentLifecycle.LateEntryEBPUnusedOnBypass;
    public bool ExactChildCompletion => IsExactChildCompletion();
    public bool ParentTestFlagsPreserved =>
        !ChildORReached
        && (Parent.FlagsAtStop is null
            || FlagsAtStop == ConvertParentFlags(Parent.FlagsAtStop.Value));
    public bool ChildStoresAndJumpPreserveFlags =>
        Child is null || !Child.OrReached || Child.FlagsPreserved;
    public bool FlagsPreserved =>
        ChildORReached
            ? ChildStoresAndJumpPreserveFlags
            : ParentTestFlagsPreserved;
    public CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags? ParentFlagsAtStop =>
        Parent.FlagsAtStop is { } flags ? ConvertParentFlags(flags) : null;
    public CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags? ChildORFlags =>
        Child?.OrFlags is { } flags ? ConvertChildFlags(flags) : null;
    public CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags? FlagsAtStop =>
        ChildORReached ? ChildORFlags : ParentFlagsAtStop;
    public bool ChildFlagsOverrideParentTest => ChildORReached;
    public bool FreshGlobalValueForwardedFromNodeWrite => false;
    public bool FreshGlobalReadAfterAllParentWrites =>
        Parent.FreshGlobalReadAfterAllNodeWrites
        && OrderedMemoryEffects.Any(effect =>
            effect.Kind
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                    .IndependentFreshGlobalRead);
    public bool ExactParentWrites =>
        CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
            .HasExactParentWrites(Parent);
    public bool ExactOrderedMemoryEffects =>
        CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
            .HasExactOrderedMemoryEffects(Parent, Child, OrderedMemoryEffects);
    public bool ExactBoundedExitAdmitted =>
        ExactNonzeroParentBypass || ExactChildCompletion;
    public uint StopAtRawFileOffsetExclusive =>
        Child?.StopAtRawFileOffsetExclusive ?? Parent.StopAtRawFileOffsetExclusive;

    private bool IsExactChildCompletion()
    {
        CapacityFinalCalleeCommonSelfRingTailPlan? child = Child;
        CapacityFinalCalleeCommonFreshMaskGatePlan? gate = Parent.FreshMaskGate;
        uint? derivedEBX = Parent.ParentLifecycle?.DerivedEBX;
        uint? capturedESI = Parent.Capture.EntryESI;

        return ExactZeroChildHandoff
            && ExactParentWrites
            && ExactOrderedMemoryEffects
            && FreshGlobalReadAfterAllParentWrites
            && gate?.Capture.FreshGlobalValue is uint freshGlobal
            && gate.ShiftResult is uint shiftedBit
            && derivedEBX is uint nodeEBX
            && capturedESI is uint entryESI
            && child is
            {
                Outcome: CapacityFinalCalleeCommonSelfRingTailOutcome.Completed,
                OrReached: true,
                GlobalStoreReached: true,
                FirstLocalStoreReached: true,
                SecondLocalStoreReached: true,
                ThirdLocalStoreReached: true,
                FourthLocalStoreReached: true,
                JumpReached: true,
                JumpTargetEntered: false,
                ExactBoundedExitAdmitted: true
            }
            && child.OrEAX == freshGlobal
            && child.OrEDX == shiftedBit
            && child.Capture.EntryEAX == freshGlobal
            && child.Capture.EntryEDX == shiftedBit
            && child.Capture.LateEBX == nodeEBX
            && child.Capture.LateESI == entryESI
            && child.Writes.Select(write => write.RawFileOffset).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonSelfRingTailProjection.GlobalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FirstLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.SecondLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.ThirdLocalStoreRawFileOffset,
                CapacityFinalCalleeCommonSelfRingTailProjection.FourthLocalStoreRawFileOffset
            })
            && child.Writes.Length == 5
            && child.JumpTargetRawFileOffset
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .ChildJumpTargetRawFileOffset
            && child.JumpTargetVirtualAddress
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .ChildJumpTargetVirtualAddress
            && child.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .ChildJumpTargetRawFileOffset;
    }

    private static CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags ConvertParentFlags(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags ConvertChildFlags(
        CapacityFinalCalleeCommonSelfRingTailFlags flags) =>
        new(
            flags.Carry,
            flags.Parity,
            flags.AuxiliaryCarry,
            flags.Zero,
            flags.Sign,
            flags.Overflow);
}

/// <summary>
/// Composes the complete parent node/fresh-mask route with its exact-zero self-ring tail.
/// </summary>
public static class CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.EntryRawFileOffset;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.ZeroDestinationRawFileOffset;
    public const uint NonzeroDestinationRawFileOffset =
        CapacityFinalCalleeCommonFreshMaskGateProjection.NonzeroDestinationRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset;
    public const uint ChildJumpTargetRawFileOffset =
        CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetRawFileOffset;
    public const uint ChildJumpTargetVirtualAddress =
        CapacityFinalCalleeCommonSelfRingTailProjection.JumpTargetVirtualAddress;

    public static CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan Project(
        CapacityFinalCalleeCommonNodeSelfRingLifecycleCapture capture)
    {
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(
                    capture.EntryEDI,
                    capture.LateEntryEBP,
                    capture.EntryESI,
                    capture.LateFreshGlobalValue));

        if (parent.Outcome
            != CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                .ZeroTestResultToBounded93AC)
        {
            return FinishWithoutChild(capture, parent, MapParentOutcome(parent.Outcome));
        }

        if (!HasExactZeroChildHandoff(parent))
        {
            return FinishWithoutChild(
                capture,
                parent,
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .InvalidZeroChildHandoff);
        }

        uint freshGlobal = parent.FreshMaskGate!.Capture.FreshGlobalValue!.Value;
        uint shiftedBit = parent.FreshMaskGate.ShiftResult!.Value;
        uint nodeEBX = parent.ParentLifecycle!.DerivedEBX!.Value;
        uint entryESI = capture.EntryESI!.Value;
        CapacityFinalCalleeCommonSelfRingTailPlan child =
            CapacityFinalCalleeCommonSelfRingTailProjection.Project(
                new(
                    freshGlobal,
                    shiftedBit,
                    nodeEBX,
                    entryESI));
        CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome outcome =
            child.Outcome switch
            {
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothLatePointers =>
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                        .MissingBothLatePointers,
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateEBX =>
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateEBX,
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateESI =>
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateESI,
                CapacityFinalCalleeCommonSelfRingTailOutcome.Completed =>
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed,
                _ => CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .InvalidZeroChildHandoff
            };
        ImmutableArray<CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect> effects =
            BuildEffects(parent, child);

        return new(capture, outcome)
        {
            Parent = parent,
            Child = child,
            OrderedMemoryEffects = effects
        };
    }

    internal static bool HasExactZeroChildHandoff(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonFreshMaskGatePlan? gate = parent.FreshMaskGate;
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan? node = parent.ParentLifecycle;
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? nodeInitialization =
            node?.NodeLifecycle;
        if (parent.Outcome
                != CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .ZeroTestResultToBounded93AC
            || !parent.ParentOutcomeValidated
            || !parent.ParentNodeInitializationCompleted
            || !parent.NodeFreshMaskHandoffValidated
            || !parent.ParentExactCleanAdmission
            || !parent.ExactCleanHandoff
            || !parent.ExactCleanExitAdmitted
            || !parent.FreshMaskGateReached
            || !parent.Internal93A1Entered
            || !parent.FreshGlobalReadReached
            || !parent.FreshGlobalValueCaptured
            || !parent.FreshGlobalReadAfterAllNodeWrites
            || parent.FreshGlobalValueForwardedFromNodeWrite
            || !HasExactParentWrites(parent)
            || node is null
            || nodeInitialization is null
            || !node.ExactCleanAdmission
            || node.LateEntryEBPProvidedButUnused
            || node.LateEntryEBPUnusedOnBypass
            || node.LateEntryEBPConsumed != parent.Capture.LateEntryEBP.HasValue
            || node.UnreachedEntryESICaptureProvided
            || node.Capture.EntryEDI != parent.Capture.EntryEDI
            || node.ParentLifecycle?.Capture.EntryEDI != parent.Capture.EntryEDI
            || parent.Capture.EntryEDI is null
            || node.NodeLifecycle?.Capture.EntryESI != parent.Capture.EntryESI
            || node.DerivedEBX != node.NodeLifecycle?.DerivedEBX
            || node.DerivedEBX != parent.ParentLifecycle?.DerivedEBX
            || parent.Capture.EntryESI is not uint entryESI
            || nodeInitialization.Capture.EntryESI != entryESI
            || parent.ParentLifecycle?.NodeLifecycle?.DerivedEBX is not uint nodeEBX
            || parent.ParentLifecycle.DerivedECX is not uint nodeIndex
            || nodeInitialization.Capture.EntryECX != nodeIndex
            || nodeInitialization.DerivedEBX != nodeEBX
            || gate is null
            || gate.Capture.FreshGlobalValue != parent.Capture.LateFreshGlobalValue
            || gate.Capture.EntryEDX != node.DerivedEDX
            || gate.Capture.EntryECX != node.DerivedECX
            || !gate.FreshGlobalReadReached
            || !gate.FreshGlobalValueCaptured
            || gate.ReadAttempts.Length != 1
            || gate.ReadAttempts[0] is not
            {
                RawFileOffset: CapacityFinalCalleeCommonFreshMaskGateProjection
                    .FreshGlobalMoveRawFileOffset,
                AbsoluteAddress: CapacityFinalCalleeCommonFreshMaskGateProjection
                    .FreshGlobalVirtualAddress,
                ReadReached: true,
                ValueCaptured: true
            }
            || gate.Outcome
                != CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC
            || !gate.ShiftExecuted
            || gate.ShiftResult is not uint shiftedBit
            || gate.ShiftInputEDX != node.DerivedEDX
            || node.DerivedEDX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .MoveEDXImmediate
            || gate.ShiftCountSource != node.DerivedECX
            || gate.ShiftCountMasked != (byte)(node.DerivedECX!.Value & 31)
            || !gate.TestReached
            || gate.TestResult != 0
            || gate.TestResult !=
                (gate.Capture.FreshGlobalValue!.Value & shiftedBit)
            || !gate.JnzReached
            || gate.JnzTaken != false
            || !gate.ExactCleanBoundedDestination
            || gate.JnzSelectedDestinationRawFileOffset != ZeroDestinationRawFileOffset
            || gate.StopAtRawFileOffsetExclusive != ZeroDestinationRawFileOffset
            || parent.StopAtRawFileOffsetExclusive != ZeroDestinationRawFileOffset
            || gate.EntryEDXCaptureProvidedButUnused
            || gate.EntryECXCaptureProvidedButUnused
            || gate.UnreachedEntryEDXCaptureProvided
            || gate.UnreachedEntryECXCaptureProvided
            || !gate.EntryEDXOperandConsumed
            || !gate.EntryECXOperandConsumed
            || gate.MemoryWritePerformed
            || CapacityFinalCalleeCommonSelfRingTailProjection.EntryRawFileOffset
                != ZeroDestinationRawFileOffset)
        {
            return false;
        }

        return true;
    }

    internal static bool HasExactParentWrites(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan? node =
            parent.ParentLifecycle;
        CapacityFinalCalleeCommonIndexedNodeInitializationPlan? initialization =
            node?.NodeLifecycle;
        if (node is null
            || initialization is null
            || parent.Writes.Length != node.Writes.Length
            || initialization.Writes.Length != 3
            || node.Writes.Count(write =>
                write.Stage
                    == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore)
                != (node.FrameStoreCommitted ? 1 : 0)
            || node.Writes.Count(write =>
                write.Stage
                    is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore)
                != 3)
        {
            return false;
        }

        if (parent.Capture.EntryESI is not uint entryESI
            || node.DerivedECX is not uint index)
        {
            return false;
        }
        uint derivedEBX = CapacityFinalCalleeCommonIndexedNodeInitializationProjection
            .DeriveEBX(index);
        uint[] expectedNodeOffsets =
        {
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.FirstStoreRawFileOffset,
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.SecondStoreRawFileOffset,
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.ThirdStoreRawFileOffset
        };
        uint[] expectedNodeAddresses =
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
        };
        uint[] expectedNodeValues = { index, 0, 0 };
        CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage[] expectedStages =
        {
            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore,
            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore,
            CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore
        };
        ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite> nodeWrites =
            node.Writes.Where(write =>
                write.Stage
                    is CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore
                        or CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore)
                .ToImmutableArray();
        if (!nodeWrites.Select(write => write.Stage).SequenceEqual(expectedStages)
            || !nodeWrites.Select(write => write.RawFileOffset).SequenceEqual(expectedNodeOffsets)
            || !nodeWrites.Select(write => write.Address).SequenceEqual(expectedNodeAddresses)
            || !nodeWrites.Select(write => write.Value).SequenceEqual(expectedNodeValues)
            || !initialization.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(expectedNodeOffsets)
            || !initialization.Writes.Select(write => write.Address)
                .SequenceEqual(expectedNodeAddresses)
            || !initialization.Writes.Select(write => write.Value)
                .SequenceEqual(expectedNodeValues)
            || !node.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(parent.Writes.Select(write => write.RawFileOffset))
            || !node.Writes.Select(write => write.Address)
                .SequenceEqual(parent.Writes.Select(write => write.Address))
            || !node.Writes.Select(write => write.Value)
                .SequenceEqual(parent.Writes.Select(write => write.Value))
            || node.DerivedEBX != derivedEBX
            || node.NodeLifecycle?.DerivedEBX != derivedEBX
            || node.DerivedEDX
                != CapacityFinalCalleeCommonIndexedNodeInitializationProjection
                    .MoveEDXImmediate
            || parent.ParentLifecycle?.DerivedEBX != derivedEBX
            || parent.OrderedMemoryEffects.Length != node.Writes.Length + 1
            || parent.OrderedMemoryEffects[^1] is not
            {
                Kind: CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind
                    .FreshGlobalRead,
                RawFileOffset: CapacityFinalCalleeCommonFreshMaskGateProjection
                    .FreshGlobalMoveRawFileOffset,
                Address: CapacityFinalCalleeCommonFreshMaskGateProjection
                    .FreshGlobalVirtualAddress,
                ForwardedFromPriorWrite: false
            })
        {
            return false;
        }

        if (node.FrameStoreCommitted)
        {
            CapacityFinalCalleeCommonHighSizeNodeLifecycleWrite frame = node.Writes[0];
            if (frame.Stage
                    != CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore
                || frame.RawFileOffset
                    != CapacityFinalCalleeCommonHighSizeBitScanProjection.FrameStoreRawFileOffset
                || frame.Address != node.FrameStoreAddress
                || frame.Value != node.FrameStoreValue)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool HasExactOrderedMemoryEffects(
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent,
        CapacityFinalCalleeCommonSelfRingTailPlan? child,
        ImmutableArray<CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect> effects)
    {
        var expected = ImmutableArray.CreateBuilder<
            CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write
                        when effect.WriteStage
                            == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                                .ParentFrameStore =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ParentFrameWrite,
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.FreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .IndependentFreshGlobalRead,
                    _ => throw new ArgumentOutOfRangeException(nameof(parent))
                };
            expected.Add(new(
                kind,
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                effect.ValueCaptured,
                effect.Kind
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write,
                effect.ForwardedFromPriorWrite));
        }

        if (child is not null)
        {
            foreach (CapacityFinalCalleeCommonSelfRingTailWrite write in child.Writes)
            {
                CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind kind =
                    write.RawFileOffset
                        == CapacityFinalCalleeCommonSelfRingTailProjection
                            .GlobalStoreRawFileOffset
                        ? CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ChildGlobalWrite
                        : CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ChildPointerStore;
                expected.Add(new(
                    kind,
                    write.RawFileOffset,
                    write.Address,
                    write.Value,
                    ValueCaptured: true,
                    Committed: true,
                    ForwardedFromPriorWrite: false));
            }
        }

        return effects.SequenceEqual(expected)
            && effects.Where(effect =>
                    effect.Kind
                        is CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ChildGlobalWrite
                            or CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                                .ChildPointerStore)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(child?.Writes.Select(write => write.RawFileOffset)
                    ?? Enumerable.Empty<uint>())
            && effects.All(effect => !effect.ForwardedFromPriorWrite);
    }

    private static CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan FinishWithoutChild(
        CapacityFinalCalleeCommonNodeSelfRingLifecycleCapture capture,
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent,
        CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome outcome)
    {
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan plan = new(capture, outcome)
        {
            Parent = parent,
            OrderedMemoryEffects = BuildEffects(parent, null)
        };
        return plan;
    }

    private static ImmutableArray<CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect>
        BuildEffects(
            CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan parent,
            CapacityFinalCalleeCommonSelfRingTailPlan? child)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffect>();
        foreach (CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect effect
            in parent.OrderedMemoryEffects)
        {
            CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind kind =
                effect.Kind switch
                {
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write
                        when effect.WriteStage
                            == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage
                                .ParentFrameStore =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ParentFrameWrite,
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.FreshGlobalRead =>
                        CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .IndependentFreshGlobalRead,
                    _ => throw new ArgumentOutOfRangeException(nameof(parent))
                };
            effects.Add(new(
                kind,
                effect.RawFileOffset,
                effect.Address,
                effect.Value,
                effect.ValueCaptured,
                effect.Kind
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write,
                effect.ForwardedFromPriorWrite));
        }

        if (child is not null)
        {
            foreach (CapacityFinalCalleeCommonSelfRingTailWrite write in child.Writes)
            {
                CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind kind =
                    write.RawFileOffset
                        == CapacityFinalCalleeCommonSelfRingTailProjection
                            .GlobalStoreRawFileOffset
                        ? CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ChildGlobalWrite
                        : CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                            .ChildPointerStore;
                effects.Add(new(
                    kind,
                    write.RawFileOffset,
                    write.Address,
                    write.Value,
                    ValueCaptured: true,
                    Committed: true,
                    ForwardedFromPriorWrite: false));
            }
        }

        return effects.ToImmutable();
    }

    private static CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome MapParentOutcome(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryEDIAt9354 =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryEDIAt9354,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingLateEntryEBPAt9374 =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateEntryEBPAt9374,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryESIAt9384 =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryESIAt9384,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.ParentNotCleanAt93A1 =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.ParentNotCleanAt93A1,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingFreshGlobalValueAt93A1 =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingFreshGlobalValueAt93A1,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                .NonzeroTestResultToBounded93C0 =>
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                        .NonzeroTestResultBypassesChild,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidParentHandoff =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidParentHandoff,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidNodeHandoff =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidNodeHandoff,
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.InvalidFreshMaskHandoff =>
                CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.InvalidFreshMaskHandoff,
            _ => CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                .InvalidZeroChildHandoff
        };
}
