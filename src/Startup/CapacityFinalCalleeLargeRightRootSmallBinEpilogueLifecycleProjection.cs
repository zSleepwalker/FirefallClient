using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleCapture(
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleCapture ParentCapture,
    uint? LateChildEsp,
    uint? LateChildEbp,
    byte? TestedByte,
    uint? StackPopEdiValue,
    uint? StackPopEsiValue,
    uint? StackPopEbxValue,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally,
    uint? PostCallEbp,
    uint? StackPopEbpValue,
    uint? ReturnTargetVirtualAddress);

public enum CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedEpilogueCaptures,
    ParentNotClean,
    MissingLateChildEsp,
    MissingLateChildEbp,
    MissingLateChildEspAndEbp,
    EpilogueProjectionStarted
}

public readonly record struct CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public enum CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStageKind
{
    SmallBinRingLifecycle,
    GatedEpilogue
}

public enum CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
{
    ParentRead,
    ParentWrite,
    ParentLateMaskGlobalRead,
    ZeroRouteGlobalWrite,
    ZeroRouteRingWrite,
    SuccessorEDXPlus8Read,
    SuccessorGlobal1EDADD8Read,
    SuccessorRingWrite,
    TestByteRead,
    StackPopEdi,
    StackPopEsi,
    StackPopEbx,
    TestBranch,
    PushImmediate,
    CallCellRead,
    OpaqueCallReturnStatus,
    PostCallEbpCapture,
    MovEspFromEbp,
    StackPopEbp,
    ReturnTargetRead,
    ReturnInstruction
}

public readonly record struct CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect(
    CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind Kind,
    int SourceIndex,
    uint? RawFileOffset,
    uint? Address,
    uint? Value,
    bool? BooleanValue,
    bool ReadAttempted,
    bool WriteAttempted,
    bool Attempted,
    bool ValueCaptured,
    bool Committed,
    bool ForwardedFromPriorWrite);

public sealed record CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStage(
    CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStageKind Kind,
    ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
        Effects,
    uint StopAtRawFileOffsetExclusive,
    CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags? FlagsAtStop,
    bool ExactCleanStop);

public sealed record CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan(
    CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome Outcome,
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan ParentPlan)
{
    public CapacityFinalCalleeGatedEpiloguePlan? EpiloguePlan { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStage>
        Stages { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStage>
                .Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
        OrderedEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
                .Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome ParentOutcome =>
        ParentPlan.Outcome;
    public bool ParentExactZeroRingRoute =>
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
            .IsExactZeroRingRoute(ParentPlan);
    public bool ParentExactNonzeroRingRoute =>
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
            .IsExactNonzeroRingRoute(ParentPlan);
    public bool ParentRingCompletionTo9441 =>
        ParentExactZeroRingRoute || ParentExactNonzeroRingRoute;
    public bool ParentRingEpilogueAdmission =>
        ParentRingCompletionTo9441 && ParentPlan.ExactCleanCompletion;
    public bool ChildEntryPending =>
        ParentRingEpilogueAdmission && EpiloguePlan is null;
    public bool ChildEntryValidated => EpiloguePlan?.Capture.EntryValidated == true;
    public bool ChildEntryTargetEntered => EpiloguePlan?.TestReadReached == true;
    public bool Internal9441Entered => ChildEntryTargetEntered;
    public CapacityFinalCalleeGatedEpilogueOutcome? ChildOutcome =>
        EpiloguePlan?.Outcome;
    public uint ChildReturnInstructionRawFileOffset =>
        CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset;
    public bool ChildReturnReached => EpiloguePlan?.ReturnReached == true;
    public bool ChildBoundedExitReached =>
        ParentRingCompletionTo9441
        && EpiloguePlan is
        {
            ReturnReached: true,
            StopAtRawFileOffsetExclusive:
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
                    .ReturnBoundaryRawFileOffsetExclusive
        };
    public bool ChildExactCleanExitAdmitted =>
        EpiloguePlan is { ExactCleanExitAdmitted: true } epilogue
        && epilogue.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
                .ReturnBoundaryRawFileOffsetExclusive;
    public CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        ParentFlagsAtChildEntry => Convert(ParentPlan.FlagsAtStop);
    public CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags? FlagsAtStop =>
        EpiloguePlan is { CallReached: true, CallReturnedNormally: true }
            ? new(null, null, null, null, null, null)
            : EpiloguePlan is
            {
                TestedByteCaptured: true,
                TestFlags: { } testFlags
            }
                ? new(
                    testFlags.Carry,
                    testFlags.Parity,
                    testFlags.AuxiliaryCarry,
                    testFlags.Zero,
                    testFlags.Sign,
                    testFlags.Overflow)
                : ParentFlagsAtChildEntry;
    public bool ParentFlagsPreservedUntilChildTest =>
        EpiloguePlan?.TestedByteCaptured != true
        && FlagsAtStop is { } stopFlags
        && ParentFlagsAtChildEntry is { } entryFlags
        && stopFlags == entryFlags;
    public bool ChildTestReadReached => EpiloguePlan?.TestReadReached == true;
    public bool ChildCallReached => EpiloguePlan?.CallReached == true;
    public bool OpaqueCallTargetCaptured => EpiloguePlan?.CallCellTargetCaptured == true;
    public bool? OpaqueCallReturnedNormally => EpiloguePlan?.CallReturnedNormally;
    public bool OpaqueCallExecuted => false;
    public bool DestinationEntered => false;
    public bool OuterCalleeEntered => false;
    public bool CounterDecrementReached => false;
    public bool ParentActualExactBoundedCompletion =>
        ParentPlan.ActualExactBoundedCompletion;
    public bool UnreachedFreshSuccessorEDXPlus8CaptureProvided =>
        ParentPlan.UnreachedFreshSuccessorEDXPlus8CaptureProvided;
    public bool UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided =>
        ParentPlan.UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided;
    public bool ActualExactBoundedCompletion =>
        ParentPlan.ActualExactBoundedCompletion
        && (ParentRingEpilogueAdmission
            ? ChildBoundedExitReached
            : StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive);
    public bool ExactBoundedCompletion => ActualExactBoundedCompletion;
    public bool ExactBoundedExitAdmitted => ActualExactBoundedCompletion;
    public bool ExactCleanCompletion =>
        ParentRingCompletionTo9441
            ? ParentPlan.ExactCleanCompletion && ChildExactCleanExitAdmitted
            : ParentPlan.ExactCleanCompletion && !AnyEpilogueCaptureProvided;
    public bool ExactCleanExitAdmitted => ExactCleanCompletion;
    public uint? BranchTargetRawFileOffset =>
        ParentPlan.BranchTargetRawFileOffset;
    public bool BranchReached => ParentPlan.BranchReached;
    public uint? InheritedESIValue =>
        ParentPlan.ZeroRouteSelected
            ? ParentPlan.ZeroRoute?.Capture.ChildEntryESI
            : ParentPlan.NonzeroRouteSelected
                ? ParentPlan.NonzeroRoute?.Capture.ChildEntryESI
                : null;
    public bool InheritedESIDerivedFromParent =>
        InheritedESIValue is uint inheritedESI
        && inheritedESI
            == Capture.ParentCapture.Parent.Parent.Parent.Parent.LateEntryESI;
    public bool InheritedESIConsumedByParentRing =>
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
            .IsInheritedESIConsumed(ParentPlan);
    public ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        ParentOrderedMemoryEffects => ParentPlan.CombinedOrderedMemoryEffects;
    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> ChildStackAccesses =>
        EpiloguePlan?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
    public bool ParentMemoryEffectsCompleteBeforeChildTest =>
        ParentRingEpilogueAdmission
        && ChildTestReadReached
        && ParentPlan.CombinedOrderedMemoryEffects.Length > 0
        && OrderedEffects.Length > ParentPlan.CombinedOrderedMemoryEffects.Length
        && OrderedEffects.Take(ParentPlan.CombinedOrderedMemoryEffects.Length)
            .Select(effect => effect.RawFileOffset)
            .SequenceEqual(ParentPlan.CombinedOrderedMemoryEffects
                .Select(effect => (uint?)effect.RawFileOffset))
        && OrderedEffects[ParentPlan.CombinedOrderedMemoryEffects.Length] is
        {
            Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                .TestByteRead,
            RawFileOffset: CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
            Attempted: true
        };
    public bool LateChildEspProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEsp.HasValue;
    public bool LateChildEbpProvidedButUnused =>
        EpiloguePlan is null && Capture.LateChildEbp.HasValue;
    public bool UnreachedTestedByteCaptureProvided =>
        EpiloguePlan?.UnreachedTestedByteCaptureProvided ?? Capture.TestedByte.HasValue;
    public bool UnreachedStackPopEdiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEdiCaptureProvided
        ?? Capture.StackPopEdiValue.HasValue;
    public bool UnreachedStackPopEsiCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEsiCaptureProvided
        ?? Capture.StackPopEsiValue.HasValue;
    public bool UnreachedStackPopEbxCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbxCaptureProvided
        ?? Capture.StackPopEbxValue.HasValue;
    public bool UnreachedCallCellTargetCaptureProvided =>
        EpiloguePlan?.UnreachedCallCellTargetCaptureProvided
        ?? Capture.CallCellTargetVirtualAddress.HasValue;
    public bool UnreachedCallReturnStatusProvided =>
        EpiloguePlan?.UnreachedCallReturnStatusProvided
        ?? Capture.CallReturnedNormally.HasValue;
    public bool UnreachedPostCallEbpCaptureProvided =>
        EpiloguePlan?.UnreachedPostCallEbpCaptureProvided
        ?? Capture.PostCallEbp.HasValue;
    public bool UnreachedStackPopEbpCaptureProvided =>
        EpiloguePlan?.UnreachedStackPopEbpCaptureProvided
        ?? Capture.StackPopEbpValue.HasValue;
    public bool UnreachedReturnTargetCaptureProvided =>
        EpiloguePlan?.UnreachedReturnTargetCaptureProvided
        ?? Capture.ReturnTargetVirtualAddress.HasValue;

    private bool AnyEpilogueCaptureProvided =>
        Capture.LateChildEsp.HasValue
        || Capture.LateChildEbp.HasValue
        || Capture.TestedByte.HasValue
        || Capture.StackPopEdiValue.HasValue
        || Capture.StackPopEsiValue.HasValue
        || Capture.StackPopEbxValue.HasValue
        || Capture.CallCellTargetVirtualAddress.HasValue
        || Capture.CallReturnedNormally.HasValue
        || Capture.PostCallEbp.HasValue
        || Capture.StackPopEbpValue.HasValue
        || Capture.ReturnTargetVirtualAddress.HasValue;

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        Convert(CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.Auxiliary,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}

/// <summary>
/// Joins only the exact zero-OR or nonzero-CMP small-bin ring JMP to the bounded
/// 9441 epilogue; the internal TEST entry is not treated as an outer-root entry.
/// </summary>
public static class CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.EntryRawFileOffset;
    public const uint ChildEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint RingCompletionRawFileOffset =
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.CompletionRawFileOffset;
    public const uint ReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;
    public const uint ZeroRingFirstWriteRawFileOffset = 0x012C931D;
    public const uint ZeroRingSecondWriteRawFileOffset = 0x012C9323;
    public const uint ZeroRingThirdWriteRawFileOffset = 0x012C9326;
    public const uint ZeroRingFourthWriteRawFileOffset = 0x012C9329;
    public const uint ZeroRingFifthWriteRawFileOffset = 0x012C932C;
    public const uint NonzeroRingFirstWriteRawFileOffset = 0x012C9343;
    public const uint NonzeroRingSecondWriteRawFileOffset = 0x012C9346;
    public const uint NonzeroRingThirdWriteRawFileOffset = 0x012C9349;
    public const uint NonzeroRingFourthWriteRawFileOffset = 0x012C934C;

    private static readonly ImmutableArray<uint> ZeroRingWriteOffsets =
        ImmutableArray.Create(
            ZeroRingFirstWriteRawFileOffset,
            ZeroRingSecondWriteRawFileOffset,
            ZeroRingThirdWriteRawFileOffset,
            ZeroRingFourthWriteRawFileOffset,
            ZeroRingFifthWriteRawFileOffset);

    private static readonly ImmutableArray<uint> NonzeroRingWriteOffsets =
        ImmutableArray.Create(
            NonzeroRingFirstWriteRawFileOffset,
            NonzeroRingSecondWriteRawFileOffset,
            NonzeroRingThirdWriteRawFileOffset,
            NonzeroRingFourthWriteRawFileOffset);

    public static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent =
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.Project(
                capture.ParentCapture);
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
            parentEffects = ConvertParentEffects(parent);

        if (ParentRingCanAdmitEpilogue(parent))
        {
            if (capture.LateChildEsp is not uint childEsp
                || capture.LateChildEbp is not uint childEbp)
            {
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome pendingOutcome =
                    capture.LateChildEsp is null && capture.LateChildEbp is null
                        ? CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                            .MissingLateChildEspAndEbp
                        : capture.LateChildEsp is null
                            ? CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                                .MissingLateChildEsp
                            : CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                                .MissingLateChildEbp;
                return Finish(
                    capture,
                    parent,
                    null,
                    pendingOutcome,
                    parentEffects,
                    ChildEntryRawFileOffset);
            }

            CapacityFinalCalleeGatedEpiloguePlan epilogue =
                CapacityFinalCalleeGatedEpilogueProjection.Project(new(
                    EntryValidated: true,
                    EntryEsp: childEsp,
                    EntryEbp: childEbp,
                    TestedByte: capture.TestedByte,
                    StackPopEdiValue: capture.StackPopEdiValue,
                    StackPopEsiValue: capture.StackPopEsiValue,
                    StackPopEbxValue: capture.StackPopEbxValue,
                    CallCellTargetVirtualAddress: capture.CallCellTargetVirtualAddress,
                    CallReturnedNormally: capture.CallReturnedNormally,
                    PostCallEbp: capture.PostCallEbp,
                    StackPopEbpValue: capture.StackPopEbpValue,
                    ReturnTargetVirtualAddress: capture.ReturnTargetVirtualAddress));
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
                effects = parentEffects.AddRange(ConvertEpilogueEffects(epilogue));
            return Finish(
                capture,
                parent,
                epilogue,
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                    .EpilogueProjectionStarted,
                effects,
                epilogue.StopAtRawFileOffsetExclusive);
        }

        bool unusedChildCapture = AnyEpilogueCaptureProvided(capture);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome outcome =
            !parent.ActualExactBoundedCompletion
                ? CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                    .ParentNotClean
                : parent.ExactCleanCompletion && !unusedChildCapture
                    ? CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                        .ParentStopped
                    : CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                        .ParentStoppedWithUnusedEpilogueCaptures;
        return Finish(
            capture,
            parent,
            null,
            outcome,
            parentEffects,
            parent.StopAtRawFileOffsetExclusive);
    }

    public static bool IsExactZeroRingRoute(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonOrRingLifecyclePlan? route = parent.ZeroRoute;
        if (!parent.ParentTestBranchValidated
            || !parent.ZeroRouteSelected
            || !parent.ActualExactBoundedCompletion
            || route is not
            {
                Outcome: CapacityFinalCalleeCommonOrRingLifecycleOutcome.CompletedTo012C9441,
                BranchReached: true,
                BranchTargetRawFileOffset: RingCompletionRawFileOffset,
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                DestinationEntered: false,
                EpilogueEntered: false,
                Writes.Length: 5
            }
            || route.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(ZeroRingWriteOffsets) == false
            || route.Writes.Any(write =>
                !write.AddressCaptured || !write.ValueCaptured || !write.Committed)
            || route.Writes[0].IsParentGlobalWrite != true
            || route.Writes[0].Address
                != CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress
            || route.ParentPlan is not
                {
                    OrReached: true,
                    GlobalWriteReached: true,
                    GlobalWriteAddress:
                        CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress
                } parentOr
            || parentOr.GlobalWriteValue != parentOr.OrResult
            || route.Writes[0].Value != parentOr.OrResult
            || route.Writes.Skip(1).Any(write => write.IsParentGlobalWrite)
            || route.ParentPlan is not
                {
                    ExactCleanExitAdmitted: true,
                    GlobalWriteReached: true
                }
            || route.ChildPlan is not
                {
                    ExactCleanCompletion: true,
                    BranchReached: true,
                    BranchTargetRawFileOffset: RingCompletionRawFileOffset,
                    StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                    DestinationEntered: false,
                    Writes.Length: 4
                } child
            || child.Writes.Any(write =>
                !write.AddressCaptured || !write.ValueCaptured || !write.Committed)
            || !child.EntryESIReadReached
            || child.Capture.EntryESI !=
                parent.Capture.Parent.Parent.Parent.Parent.LateEntryESI)
        {
            return false;
        }

        return parent.BranchTargetRawFileOffset == RingCompletionRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset
            && !parent.DestinationEntered
            && !parent.EpilogueEntered
            && !parent.OuterCalleeEntered;
    }

    public static bool IsExactNonzeroRingRoute(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent)
    {
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan? route = parent.NonzeroRoute;
        if (!parent.ParentTestBranchValidated
            || !parent.NonzeroRouteSelected
            || !parent.ActualExactBoundedCompletion
            || route is not
            {
                Outcome:
                    CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                        .CompletedTo012C9441,
                BranchReached: true,
                BranchTargetRawFileOffset: RingCompletionRawFileOffset,
                StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                DestinationEntered: false,
                Writes.Length: 4
            }
            || route.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(NonzeroRingWriteOffsets) == false
            || route.Writes.Any(write =>
                !write.AddressCaptured || !write.ValueCaptured || !write.Committed)
            || route.Parent is not
                {
                    CompareReached: true,
                    JBReached: true,
                    JBTaken: false,
                    FreshEdxPlus8ReadReached: true,
                    FreshGlobalReadReached: true,
                    FreshEdxPlus8: not null,
                    FreshGlobal: not null,
                    Reads.Length: 2
                } threshold
            || threshold.Reads[0] is not
                {
                    RawFileOffset: 0x012C9334,
                    AddressCaptured: true,
                    ValueCaptured: true
                }
            || threshold.Reads[1] is not
                {
                    RawFileOffset: 0x012C9337,
                    Address: CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection
                        .SuccessorGlobalVirtualAddress,
                    AddressCaptured: true,
                    ValueCaptured: true
                }
            || threshold.CompareLeft is not uint left
            || threshold.CompareRight is not uint right
            || threshold.Reads[0].Value != left
            || threshold.Reads[1].Value != right
            || left < right
            || route.Child is not
                {
                    ExactCleanCompletion: true,
                    BranchReached: true,
                    BranchTargetRawFileOffset: RingCompletionRawFileOffset,
                    StopAtRawFileOffsetExclusive: RingCompletionRawFileOffset,
                    DestinationEntered: false
                } child
            || !child.EntryESIReadReached
            || child.Capture.EntryESI !=
                parent.Capture.Parent.Parent.Parent.Parent.LateEntryESI
            || child.Writes.Length != 4
            || child.Writes.Any(write =>
                !write.AddressCaptured || !write.ValueCaptured || !write.Committed))
        {
            return false;
        }

        return parent.BranchTargetRawFileOffset == RingCompletionRawFileOffset
            && parent.StopAtRawFileOffsetExclusive == RingCompletionRawFileOffset
            && !parent.DestinationEntered
            && !parent.EpilogueEntered
            && !parent.OuterCalleeEntered;
    }

    public static bool IsInheritedESIConsumed(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent)
    {
        uint? expectedESI = parent.Capture.Parent.Parent.Parent.Parent.LateEntryESI;
        if (expectedESI is not uint esi)
        {
            return false;
        }

        if (parent.ZeroRouteSelected
            && parent.ZeroRoute?.ChildPlan is { } zeroChild)
        {
            return zeroChild.EntryESIReadReached
                && zeroChild.Capture.EntryESI == esi
                && zeroChild.Writes.Length == 4
                && zeroChild.Writes[0].Value == esi
                && zeroChild.Writes[1].Value == esi;
        }

        if (parent.NonzeroRouteSelected
            && parent.NonzeroRoute?.Child is { } nonzeroChild)
        {
            return nonzeroChild.EntryESIReadReached
                && nonzeroChild.Capture.EntryESI == esi
                && nonzeroChild.Writes.Length == 4
                && nonzeroChild.Writes[0].Value == esi
                && nonzeroChild.Writes[1].Value == esi;
        }

        return false;
    }

    private static bool ParentRingCanAdmitEpilogue(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent) =>
        parent.ExactCleanCompletion
        && (IsExactZeroRingRoute(parent) || IsExactNonzeroRingRoute(parent));

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
        ConvertParentEffects(
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent) =>
        parent.CombinedOrderedMemoryEffects
            .Select((effect, index) =>
                new CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect(
                    effect.Kind switch
                    {
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentRead =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .ParentRead,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentWrite =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .ParentWrite,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentLateMaskGlobalRead =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .ParentLateMaskGlobalRead,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ZeroRouteGlobalWrite =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .ZeroRouteGlobalWrite,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ZeroRouteRingWrite =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .ZeroRouteRingWrite,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorEDXPlus8Read =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .SuccessorEDXPlus8Read,
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorGlobal1EDADD8Read =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .SuccessorGlobal1EDADD8Read,
                        _ =>
                            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                                .SuccessorRingWrite
                    },
                    index,
                    effect.RawFileOffset,
                    effect.Address,
                    effect.Value,
                    null,
                    effect.ReadAttempted,
                    effect.WriteAttempted,
                    effect.ReadAttempted || effect.WriteAttempted,
                    effect.ValueCaptured,
                    effect.Committed,
                    effect.ForwardedFromPriorWrite))
            .ToImmutableArray();

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
        ConvertEpilogueEffects(CapacityFinalCalleeGatedEpiloguePlan child)
    {
        var effects =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>();
        if (child.TestReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.TestByteRead,
                CapacityFinalCalleeGatedEpilogueProjection.TestRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.TestByteVirtualAddress,
                child.TestedByte,
                attempted: true,
                valueCaptured: child.TestedByteCaptured,
                readAttempted: true));
        }

        AddStackPop(
            effects,
            child.StackPopEdiReached,
            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.StackPopEdi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEdiRawFileOffset,
            child.StackPopEdiAddress,
            child.PoppedEdi);
        AddStackPop(
            effects,
            child.StackPopEsiReached,
            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.StackPopEsi,
            CapacityFinalCalleeGatedEpilogueProjection.PopEsiRawFileOffset,
            child.StackPopEsiAddress,
            child.PoppedEsi);
        AddStackPop(
            effects,
            child.StackPopEbxReached,
            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.StackPopEbx,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbxRawFileOffset,
            child.StackPopEbxAddress,
            child.PoppedEbx);

        if (child.BranchReached)
        {
            effects.Add(new(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.TestBranch,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.BranchRawFileOffset,
                null,
                null,
                child.BranchTaken,
                false,
                false,
                true,
                child.BranchTaken.HasValue,
                false,
                false));
        }

        if (child.PushImmediateReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.PushImmediate,
                CapacityFinalCalleeGatedEpilogueProjection.PushImmediateRawFileOffset,
                child.PushImmediateAddress,
                child.PushedImmediateVirtualAddress,
                attempted: true,
                valueCaptured: child.PushedImmediateVirtualAddress.HasValue,
                writeAttempted: true,
                committed: child.PushImmediateAddress.HasValue
                    && child.PushedImmediateVirtualAddress.HasValue));
        }

        if (child.CallCellReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.CallCellRead,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                CapacityFinalCalleeGatedEpilogueProjection.CallCellVirtualAddress,
                child.CallCellTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.CallCellTargetCaptured,
                readAttempted: true));
        }

        if (child.CallReached)
        {
            effects.Add(new(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .OpaqueCallReturnStatus,
                -1,
                CapacityFinalCalleeGatedEpilogueProjection.CallRawFileOffset,
                null,
                null,
                child.Capture.CallReturnedNormally,
                false,
                false,
                true,
                child.Capture.CallReturnedNormally.HasValue,
                false,
                false));
        }

        if (child.PostCallEbpCaptureReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .PostCallEbpCapture,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.PostCallEbp,
                attempted: true,
                valueCaptured: child.PostCallEbpCaptured));
        }

        if (child.MovEspFromEbpReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .MovEspFromEbp,
                CapacityFinalCalleeGatedEpilogueProjection.MovEspFromEbpRawFileOffset,
                null,
                child.MovEspSourceEbp,
                attempted: true,
                valueCaptured: child.MovEspSourceEbp.HasValue));
        }

        AddStackPop(
            effects,
            child.StackPopEbpReached,
            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind.StackPopEbp,
            CapacityFinalCalleeGatedEpilogueProjection.PopEbpRawFileOffset,
            child.StackPopEbpAddress,
            child.PoppedEbp);

        if (child.ReturnTargetReadReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .ReturnTargetRead,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.ReturnTargetCaptured,
                readAttempted: true));
        }

        if (child.ReturnReached)
        {
            effects.Add(ChildEffect(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .ReturnInstruction,
                CapacityFinalCalleeGatedEpilogueProjection.ReturnRawFileOffset,
                child.ReturnTargetStackAddress,
                child.ReturnTargetVirtualAddress,
                attempted: true,
                valueCaptured: child.ReturnTargetCaptured));
        }

        return effects.ToImmutable();
    }

    private static void AddStackPop(
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>.Builder
            effects,
        bool reached,
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind effectKind,
        uint rawFileOffset,
        uint? address,
        uint? value)
    {
        if (!reached)
        {
            return;
        }

        effects.Add(ChildEffect(
            effectKind,
            rawFileOffset,
            address,
            value,
            attempted: true,
            valueCaptured: value.HasValue,
            readAttempted: true));
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect ChildEffect(
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind kind,
        uint rawFileOffset,
        uint? address,
        uint? value,
        bool attempted,
        bool valueCaptured,
        bool readAttempted = false,
        bool writeAttempted = false,
        bool committed = false) =>
        new(
            kind,
            -1,
            rawFileOffset,
            address,
            value,
            null,
            readAttempted,
            writeAttempted,
            attempted,
            valueCaptured,
            committed,
            false);

    private static bool AnyEpilogueCaptureProvided(
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleCapture capture) =>
        capture.LateChildEsp.HasValue
        || capture.LateChildEbp.HasValue
        || capture.TestedByte.HasValue
        || capture.StackPopEdiValue.HasValue
        || capture.StackPopEsiValue.HasValue
        || capture.StackPopEbxValue.HasValue
        || capture.CallCellTargetVirtualAddress.HasValue
        || capture.CallReturnedNormally.HasValue
        || capture.PostCallEbp.HasValue
        || capture.StackPopEbpValue.HasValue
        || capture.ReturnTargetVirtualAddress.HasValue;

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan Finish(
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleCapture capture,
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent,
        CapacityFinalCalleeGatedEpiloguePlan? epilogue,
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome outcome,
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffect>
            effects,
        uint stopAt)
    {
        var stages =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStage>();
        int parentEffectCount = parent.CombinedOrderedMemoryEffects.Length;
        stages.Add(new(
            CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStageKind
                .SmallBinRingLifecycle,
            effects.Take(parentEffectCount).ToImmutableArray(),
            parent.StopAtRawFileOffsetExclusive,
            Convert(parent.FlagsAtStop),
            parent.ExactCleanCompletion));
        if (epilogue is { } child)
        {
            stages.Add(new(
                CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleStageKind
                    .GatedEpilogue,
                effects.Skip(parentEffectCount).ToImmutableArray(),
                child.StopAtRawFileOffsetExclusive,
                ConvertEpilogueStopFlags(child, parent),
                child.ExactCleanExitAdmitted));
        }

        return new(capture, outcome, parent)
        {
            EpiloguePlan = epilogue,
            Stages = stages.ToImmutable(),
            OrderedEffects = effects,
            StopAtRawFileOffsetExclusive = stopAt
        };
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        Convert(CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? flags) =>
            flags is { } value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.Auxiliary,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        ConvertFlags(CapacityFinalCalleeGatedEpilogueTestFlags? flags) =>
            flags is { } value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.AuxiliaryCarry,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        ConvertEpilogueStopFlags(
            CapacityFinalCalleeGatedEpiloguePlan epilogue,
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan parent) =>
        epilogue is { CallReached: true, CallReturnedNormally: true }
            ? new(null, null, null, null, null, null)
            : epilogue.TestedByteCaptured
                ? ConvertFlags(epilogue.TestFlags)
                : Convert(parent.FlagsAtStop);
}
