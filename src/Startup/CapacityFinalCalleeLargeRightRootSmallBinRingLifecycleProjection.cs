using System.Collections.Immutable;
using System.Linq;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleCapture(
    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleCapture Parent,
    uint? FreshSuccessorEDXPlus8,
    uint? FreshSuccessorGlobal1EDADD8);

public enum CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
{
    ParentStopped,
    ParentStoppedWithUnusedFreshSuccessorCaptures,
    ParentNotClean,
    ZeroRouteMissingParentECX,
    ZeroRouteMissingParentEDI,
    ZeroRouteMissingBothParentOperands,
    ZeroRouteParentHandoffNotVerified,
    ZeroRouteMissingChildEDXForFirstWrite,
    ZeroRouteMissingChildESIForFirstWrite,
    ZeroRouteMissingChildEAXForSecondWrite,
    ZeroRouteCompletedTo012C9441,
    NonzeroRouteParentMissingEntryEDX,
    NonzeroRouteParentMissingFreshEDXPlus8,
    NonzeroRouteParentMissingFreshGlobal1EDADD8,
    NonzeroRouteParentUnsignedBelowBranchTarget,
    NonzeroRouteParentHandoffNotVerified,
    NonzeroRouteChildMissingESIForFirstWrite,
    NonzeroRouteCompletedTo012C9441
}

public enum CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStageKind
{
    ParentIndexedMaskLifecycle,
    ZeroMaskOrRingLifecycle,
    NonzeroSuccessorRingLifecycle
}

public enum CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
{
    ParentRead,
    ParentWrite,
    ParentLateMaskGlobalRead,
    ZeroRouteGlobalWrite,
    ZeroRouteRingWrite,
    SuccessorEDXPlus8Read,
    SuccessorGlobal1EDADD8Read,
    SuccessorRingWrite
}

public readonly record struct
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind Kind,
        uint RawFileOffset,
        uint? Address,
        bool AddressCaptured,
        uint? Value,
        bool ValueCaptured,
        bool ReadAttempted,
        bool WriteAttempted,
        bool Committed,
        bool ForwardedFromPriorWrite);

public readonly record struct CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public sealed record CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage(
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStageKind Kind,
    ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        MemoryEffects,
    uint StopAtRawFileOffsetExclusive,
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? FlagsAtStop,
    bool ExactCleanStop);

public sealed record CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan(
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleCapture Capture,
    CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan Parent,
    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeCommonOrRingLifecyclePlan? ZeroRoute { get; init; }
    public CapacityFinalCalleeCommonSuccessorRingLifecyclePlan? NonzeroRoute { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage>
        Stages { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage>
                .Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        CombinedOrderedMemoryEffects { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
                .Empty;
    public CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? FlagsAtStop
        { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public bool ParentTestBranchValidated { get; init; }
    public bool UnreachedFreshSuccessorEDXPlus8CaptureProvided { get; init; }
    public bool UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided { get; init; }
    public bool FreshSuccessorCapturesUnused =>
        UnreachedFreshSuccessorEDXPlus8CaptureProvided
        || UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided;
    public bool ZeroRouteSelected =>
        ParentTestBranchValidated
        && Parent.ChildMaskSelectionGate is
        {
            JnzTaken: false,
            JnzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection
                    .ZeroRouteEntryRawFileOffset
        };
    public bool NonzeroRouteSelected =>
        ParentTestBranchValidated
        && Parent.ChildMaskSelectionGate is
        {
            JnzTaken: true,
            JnzSelectedDestinationRawFileOffset:
                CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection
                    .NonzeroRouteEntryRawFileOffset
        };
    public bool ParentActualExactBoundedCompletion =>
        Parent.ActualExactBoundedCompletion;
    public bool ExactBoundedCompletion =>
        Parent.ActualExactBoundedCompletion
        && (ZeroRouteSelected
            ? ZeroRoute?.ExactCleanCompletion == true
            : NonzeroRouteSelected
                ? NonzeroRoute?.ExactCleanCompletion == true
                : true);
    public bool ActualExactBoundedCompletion => ExactBoundedCompletion;
    public bool ExactCleanCompletion =>
        ExactBoundedCompletion
        && Parent.ExactCleanCompletion
        && !FreshSuccessorCapturesUnused;
    public bool BranchReached =>
        ZeroRoute?.BranchReached == true
        || NonzeroRoute?.BranchReached == true
        || NonzeroRoute?.Parent.JBReached == true;
    public uint? NonzeroBranchTargetRawFileOffset =>
        NonzeroRoute?.BranchTargetRawFileOffset
        ?? NonzeroRoute?.Parent.JBSelectedDestinationRawFileOffset;
    public uint? BranchTargetRawFileOffset =>
        ZeroRoute?.BranchTargetRawFileOffset
        ?? NonzeroBranchTargetRawFileOffset;
    public bool Internal931BOrReached => ZeroRoute?.ParentPlan?.OrReached == true;
    public bool Internal9334FirstReadAttempted =>
        NonzeroRoute?.Parent.FreshEdxPlus8ReadReached == true;
    public bool DestinationEntered => false;
    public bool EpilogueEntered => false;
    public bool OuterCalleeEntered => false;
}

/// <summary>
/// Joins the exact indexed-mask TEST/JNZ exits to their bounded zero and
/// nonzero small-bin continuations without repeating the indexed gates.
/// </summary>
public static class CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection.EntryRawFileOffset;
    public const uint ZeroRouteEntryRawFileOffset =
        CapacityFinalCalleeCommonOrRingLifecycleProjection.EntryRawFileOffset;
    public const uint NonzeroRouteEntryRawFileOffset =
        CapacityFinalCalleeCommonSuccessorRingLifecycleProjection.ParentEntryRawFileOffset;
    public const uint CompletionRawFileOffset =
        CapacityFinalCalleeCommonRingWritesProjection.BranchTargetRawFileOffset;
    public const uint BelowDestinationRawFileOffset =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.BranchTargetRawFileOffset;
    public const uint SuccessorGlobalVirtualAddress =
        CapacityFinalCalleeCommonSuccessorThresholdGateProjection.FreshGlobalVirtualAddress;

    public static CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleCapture capture)
    {
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan parent =
            CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection.Project(
                capture.Parent);
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
            parentEffects = ConvertParentEffects(parent);
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage>.Builder
            stages =
                ImmutableArray.CreateBuilder<
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage>();
        stages.Add(new(
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStageKind
                .ParentIndexedMaskLifecycle,
            parentEffects,
            parent.StopAtRawFileOffsetExclusive,
            Convert(parent.FlagsAtStop),
            parent.ExactCleanCompletion));

        bool testBranchValidated = IsExactTestBranch(parent);
        if (!testBranchValidated)
        {
            bool anyFreshSuccessorCapture =
                capture.FreshSuccessorEDXPlus8.HasValue
                || capture.FreshSuccessorGlobal1EDADD8.HasValue;
            bool cleanParent = parent.ExactCleanCompletion
                && !anyFreshSuccessorCapture;
            return Finish(
                capture,
                parent,
                outcome: !parent.ExactBoundedCompletion || !parent.ExactCleanCompletion
                    ? CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .ParentNotClean
                    : cleanParent
                        ? CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ParentStopped
                        : CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ParentStoppedWithUnusedFreshSuccessorCaptures,
                zeroRoute: null,
                nonzeroRoute: null,
                stages: stages,
                effects: parentEffects,
                testBranchValidated: testBranchValidated,
                unreachedEDXPlus8: capture.FreshSuccessorEDXPlus8.HasValue,
                unreachedSuccessorGlobal:
                    capture.FreshSuccessorGlobal1EDADD8.HasValue,
                flagsAtStop: Convert(parent.FlagsAtStop),
                stopAt: parent.StopAtRawFileOffsetExclusive);
        }

        if (parent.ChildMaskSelectionGate!.JnzTaken == false)
        {
            CapacityFinalCalleeCommonMaskSelectionGatePlan gate =
                parent.ChildMaskSelectionGate;
            var zeroCapture = new CapacityFinalCalleeCommonOrRingLifecycleCapture(
                ParentEntryECX: gate.Capture.FreshGlobalValue,
                ParentEntryEDI: gate.DestinationValueAfterShift,
                ChildEntryEDX: parent.IndexedAddressEDX,
                ChildEntryEAX: parent.CommonIndexedMaskLifecycle?.ParentDerivedEAX,
                ChildEntryESI: parent.Capture.Parent.Parent.Parent.LateEntryESI);
            CapacityFinalCalleeCommonOrRingLifecyclePlan zeroRoute =
                CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(zeroCapture);
            ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
                zeroEffects = ConvertZeroRouteEffects(zeroRoute);
            stages.Add(new(
                CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStageKind
                    .ZeroMaskOrRingLifecycle,
                zeroEffects,
                zeroRoute.StopAtRawFileOffsetExclusive,
                Convert(zeroRoute.FlagsAtStop),
                zeroRoute.ExactCleanCompletion));
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome outcome =
                zeroRoute.Outcome switch
                {
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentECX =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingParentECX,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentEDI =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingParentEDI,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome
                        .MissingBothParentOperands =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingBothParentOperands,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome.ParentHandoffNotVerified =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteParentHandoffNotVerified,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome
                        .MissingChildEDXForFirstWrite =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingChildEDXForFirstWrite,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome
                        .MissingChildESIForFirstWrite =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingChildESIForFirstWrite,
                    CapacityFinalCalleeCommonOrRingLifecycleOutcome
                        .MissingChildEAXForSecondWrite =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteMissingChildEAXForSecondWrite,
                    _ =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                            .ZeroRouteCompletedTo012C9441
                };
            return Finish(
                capture,
                parent,
                outcome: outcome,
                zeroRoute: zeroRoute,
                nonzeroRoute: null,
                stages: stages,
                effects: parentEffects.AddRange(zeroEffects),
                testBranchValidated: testBranchValidated,
                unreachedEDXPlus8: capture.FreshSuccessorEDXPlus8.HasValue,
                unreachedSuccessorGlobal:
                    capture.FreshSuccessorGlobal1EDADD8.HasValue,
                flagsAtStop: Convert(zeroRoute.FlagsAtStop),
                stopAt: zeroRoute.StopAtRawFileOffsetExclusive);
        }

        uint? inheritedESI = capture.Parent.Parent.Parent.Parent.LateEntryESI;
        bool successorNotBelowFromCapturedValues =
            capture.FreshSuccessorEDXPlus8 is uint freshSuccessor
            && capture.FreshSuccessorGlobal1EDADD8 is uint freshGlobal
            && freshSuccessor >= freshGlobal;
        var nonzeroCapture =
            new CapacityFinalCalleeCommonSuccessorRingLifecycleCapture(
                ParentEntryEDX: parent.IndexedAddressEDX,
                ParentFreshEDXPlus8: capture.FreshSuccessorEDXPlus8,
                ParentFreshGlobal1EDADD8: capture.FreshSuccessorGlobal1EDADD8,
                ChildEntryESI: successorNotBelowFromCapturedValues
                    ? inheritedESI
                    : null);
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan nonzeroRoute =
            CapacityFinalCalleeCommonSuccessorRingLifecycleProjection.Project(
                nonzeroCapture);
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
            nonzeroEffects = ConvertNonzeroRouteEffects(nonzeroRoute);
        stages.Add(new(
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStageKind
                .NonzeroSuccessorRingLifecycle,
            nonzeroEffects,
            nonzeroRoute.StopAtRawFileOffsetExclusive,
            Convert(nonzeroRoute.FlagsAtStop)
                ?? Convert(parent.FlagsAtStop),
            nonzeroRoute.ExactCleanCompletion));

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome nonzeroOutcome =
            nonzeroRoute.Outcome switch
            {
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentMissingEntryEDX =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteParentMissingEntryEDX,
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentMissingFreshEDXPlus8 =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteParentMissingFreshEDXPlus8,
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentMissingFreshGlobal1EDADD8 =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteParentMissingFreshGlobal1EDADD8,
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentUnsignedBelowBranchTarget =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteParentUnsignedBelowBranchTarget,
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentHandoffNotVerified =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteParentHandoffNotVerified,
                CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ChildMissingESIForFirstWrite =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteChildMissingESIForFirstWrite,
                _ =>
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome
                        .NonzeroRouteCompletedTo012C9441
            };

        return Finish(
            capture,
            parent,
            outcome: nonzeroOutcome,
            zeroRoute: null,
            nonzeroRoute: nonzeroRoute,
            stages: stages,
            effects: parentEffects.AddRange(nonzeroEffects),
            testBranchValidated: testBranchValidated,
            unreachedEDXPlus8:
                capture.FreshSuccessorEDXPlus8.HasValue
                && !nonzeroRoute.Parent.FreshEdxPlus8ReadReached,
            unreachedSuccessorGlobal:
                capture.FreshSuccessorGlobal1EDADD8.HasValue
                && !nonzeroRoute.Parent.FreshGlobalReadReached,
            flagsAtStop: Convert(nonzeroRoute.FlagsAtStop)
                ?? Convert(parent.FlagsAtStop),
            stopAt: nonzeroRoute.StopAtRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan Finish(
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleCapture capture,
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan parent,
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleOutcome outcome,
        CapacityFinalCalleeCommonOrRingLifecyclePlan? zeroRoute,
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan? nonzeroRoute,
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleStage>.Builder
            stages,
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
            effects,
        bool testBranchValidated,
        bool unreachedEDXPlus8,
        bool unreachedSuccessorGlobal,
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? flagsAtStop,
        uint stopAt)
    {
        return new(capture, parent, outcome)
        {
            ZeroRoute = zeroRoute,
            NonzeroRoute = nonzeroRoute,
            Stages = stages.ToImmutable(),
            CombinedOrderedMemoryEffects = effects,
            FlagsAtStop = flagsAtStop,
            StopAtRawFileOffsetExclusive = stopAt,
            ParentTestBranchValidated = testBranchValidated,
            UnreachedFreshSuccessorEDXPlus8CaptureProvided = unreachedEDXPlus8,
            UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided =
                unreachedSuccessorGlobal
        };
    }

    private static bool IsExactTestBranch(
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan parent)
    {
        if (!parent.ExactCleanCompletion
            || !parent.ExactBoundedCompletion
            || !parent.IndexedAddressEDX.HasValue
            || parent.CommonIndexedMaskLifecycle is not
                { ParentDerivedEAX: not null }
            || parent.ChildMaskSelectionGate is not
                {
                    TestReached: true,
                    JnzReached: true,
                    ExactCleanBoundedDestination: true,
                    LaterOrOrStoreReached: false,
                    MemoryWritePerformed: false,
                    Writes.IsEmpty: true,
                    JnzSelectedDestinationRawFileOffset:
                        ZeroRouteEntryRawFileOffset
                        or NonzeroRouteEntryRawFileOffset
                } gate
            || gate.TestResult is not uint testResult
            || gate.JnzTaken is not bool jnzTaken
            || (jnzTaken != (testResult != 0))
            || gate.JnzSelectedDestinationRawFileOffset
                != (jnzTaken
                    ? NonzeroRouteEntryRawFileOffset
                    : ZeroRouteEntryRawFileOffset)
            || parent.StopAtRawFileOffsetExclusive
                != gate.JnzSelectedDestinationRawFileOffset
            || parent.OuterZeroDestinationEntered
            || parent.OuterNonzeroDestinationEntered
            || parent.OuterCalleeEntered)
        {
            return false;
        }

        return parent.Outcome
            == (jnzTaken
                ? CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .UnequalBelowNonzeroTestToBounded9334
                : CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .UnequalBelowZeroTestToBounded931B);
    }

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        ConvertParentEffects(
            CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan parent) =>
        parent.CombinedOrderedMemoryEffects.Select(effect => new
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect(
                effect.Kind switch
                {
                    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                        .ParentRead =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentRead,
                    CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                        .ParentWrite =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentWrite,
                    _ =>
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .ParentLateMaskGlobalRead
                },
                effect.RawFileOffset,
                effect.Address,
                effect.AddressCaptured,
                effect.Value,
                effect.ValueCaptured,
                effect.ReadAttempted,
                effect.WriteAttempted,
                effect.Committed,
                effect.ForwardedFromPriorWrite))
            .ToImmutableArray();

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        ConvertZeroRouteEffects(
            CapacityFinalCalleeCommonOrRingLifecyclePlan plan) =>
        plan.Writes.Select(write =>
            new CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect(
                write.IsParentGlobalWrite
                    ? CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                        .ZeroRouteGlobalWrite
                    : CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                        .ZeroRouteRingWrite,
                write.RawFileOffset,
                write.Address,
                write.AddressCaptured,
                write.Value,
                write.ValueCaptured,
                ReadAttempted: false,
                WriteAttempted: true,
                Committed: write.Committed,
                ForwardedFromPriorWrite: false))
            .ToImmutableArray();

    private static ImmutableArray<
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
        ConvertNonzeroRouteEffects(
            CapacityFinalCalleeCommonSuccessorRingLifecyclePlan plan)
    {
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
            reads = plan.Parent.Reads.Select((read, index) =>
                new CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect(
                    index == 0
                        ? CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorEDXPlus8Read
                        : CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorGlobal1EDADD8Read,
                    read.RawFileOffset,
                    read.Address,
                    read.AddressCaptured,
                    read.Value,
                    read.ValueCaptured,
                    ReadAttempted: true,
                    WriteAttempted: false,
                    Committed: false,
                    ForwardedFromPriorWrite: false))
                .ToImmutableArray();
        ImmutableArray<CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect>
            writes = plan.Writes.Select(write =>
                new CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffect(
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                        .SuccessorRingWrite,
                    write.RawFileOffset,
                    write.Address,
                    write.AddressCaptured,
                    write.Value,
                    write.ValueCaptured,
                    ReadAttempted: false,
                    WriteAttempted: true,
                    Committed: write.Committed,
                    ForwardedFromPriorWrite: false))
                .ToImmutableArray();
        return reads.AddRange(writes);
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? Convert(
        CapacityFinalCalleeCommonMaskSelectionGateFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? Convert(
        CapacityFinalCalleeCommonMaskOrStoreFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.AuxiliaryCarry, value.Zero, value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags? Convert(
        CapacityFinalCalleeCommonSuccessorThresholdGateFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign,
                value.Overflow)
            : null;
}
