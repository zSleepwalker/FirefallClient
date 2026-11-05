using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan clean =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1);
        check("Outer indexed/tag equality enters fresh threshold reads and the five-store boundary",
            clean.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && clean.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildEqualToBounded9403
            && CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
                .IsExactOuterEquality(clean.ParentPlan)
            && clean.ParentPlan.ChildPlan?.ParentPlan.FreshEDX == 0x200
            && clean.DerivedEntryEDX == 0x200
            && clean.EntryEDXDerivedFromFreshIndexedRead
            && clean.InheritedEntryESI == 0x3000
            && clean.EntryESIInheritedFromOriginalParentCapture
            && clean.OriginalEntryEDI == 0x100
            && clean.EntryEDIDerivedFromOriginalIndexedSizeGate
            && clean.ParentChildDWORDConsumed == false
            && clean.ParentIterationPlanExists == false
            && clean.ParentFlagsAtThresholdEntry is { Zero: true }
            && clean.ThresholdRingPlan?.ParentPlan.FirstJBTaken == false
            && clean.ThresholdRingPlan?.ParentPlan.SecondJBTaken == false
            && clean.ThresholdRingPlan?.ChildEntryEAX == 1
            && clean.ThresholdRingPlan?.ChildEntryESI == 0x3000
            && clean.ThresholdRingPlan?.Writes is
            [
                { RawFileOffset: 0x012C941C, Address: 0x0D, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C941F, Address: 0x208, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C9422, Address: 0x3008, Value: 1, Committed: true },
                { RawFileOffset: 0x012C9425, Address: 0x300C, Value: 0x200, Committed: true },
                { RawFileOffset: 0x012C9428, Address: 0x3018, Value: 0, Committed: true }
            ]
            && clean.RingBoundaryReached
            && !clean.RingBoundaryInstructionEntered
            && !clean.DecAtBoundaryEntered
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && clean.ExactBoundedCompletion
            && clean.ExactCleanCompletion
            && clean.FlagsAtStop is { Carry: false, Zero: true });

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan nonEqual =
            Project(
                indexedDWORD: 0x300,
                tagDWORD: 0,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1);
        check("The unequal outer gate stops at 93E4 without admitting threshold or ring work",
            nonEqual.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .ParentEqualityNotSelected
            && nonEqual.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                    .ChildUnequalToFallThrough93E4
            && nonEqual.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93E4
            && nonEqual.FreshThresholdGlobalValueCaptureProvidedButUnused
            && nonEqual.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && !nonEqual.ThresholdGateEntered
            && nonEqual.ThresholdRingPlan is null
            && nonEqual.Writes.IsEmpty
            && !nonEqual.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan missingIndexed =
            Project(
                indexedDWORD: null,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan missingTag =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: null,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1);
        check("Missing indexed or tag inputs preserve the parent stop and never enter the threshold gate",
            missingIndexed.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93C0
            && missingIndexed.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingIndexed.ThresholdRingPlan is null
            && missingIndexed.FreshThresholdGlobalValueCaptureProvidedButUnused
            && missingTag.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93D8
            && missingTag.StopAtRawFileOffsetExclusive == 0x012C93D8
            && missingTag.ThresholdRingPlan is null
            && missingTag.FreshThresholdEDXPlus8CaptureProvidedButUnused);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan dirtyParentZero =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1,
                size: 0,
                mask: 0);
        check("Outer parent zero retains its bounded stop while unused indexed and threshold captures make it dirty",
            dirtyParentZero.ParentOutcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC
            && dirtyParentZero.ParentPlan.FreshIndexedDWORDCaptureProvidedButUnused
            && dirtyParentZero.ParentPlan.FreshEDXPlus4CaptureProvidedButUnused
            && dirtyParentZero.FreshThresholdGlobalValueCaptureProvidedButUnused
            && dirtyParentZero.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && dirtyParentZero.ThresholdRingPlan is null
            && dirtyParentZero.ExactBoundedCompletion
            && !dirtyParentZero.ExactCleanCompletion
            && dirtyParentZero.StopAtRawFileOffsetExclusive == 0x012C93AC);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan firstBelow =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x400);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan secondBelow =
            Project(
                indexedDWORD: 0x400,
                tagDWORD: 0x100,
                freshThresholdGlobal: 0x300,
                freshThresholdEDXPlus8: 0x200);
        check("Both unsigned JB outcomes bypass the ring at 8E76 after fresh threshold reads",
            firstBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == true
            && firstBelow.ThresholdRingPlan?.ParentPlan.SecondCompareReached == false
            && firstBelow.ThresholdRingPlan?.ChildPlan is null
            && firstBelow.Writes.IsEmpty
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8ReadReached == true
            && firstBelow.ExactBoundedCompletion
            && firstBelow.ExactCleanCompletion
            && secondBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == false
            && secondBelow.ThresholdRingPlan?.ParentPlan.SecondJBTaken == true
            && secondBelow.ThresholdRingPlan?.ChildPlan is null
            && secondBelow.Writes.IsEmpty
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && secondBelow.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan wrapping =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 0xFFFFFFF8,
                entryESI: 0xFFFFFFF8);
        check("Ring write pointer arithmetic wraps independently from the threshold EDX+8 read",
            wrapping.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 0x208
            && wrapping.ThresholdRingPlan?.ChildEntryEAX == 0xFFFFFFF8
            && wrapping.Writes is
            [
                { Address: 4, Value: 0xFFFFFFF8, Committed: true },
                { Address: 0x208, Value: 0xFFFFFFF8, Committed: true },
                { Address: 0, Value: 0xFFFFFFF8, Committed: true },
                { Address: 4, Value: 0x200, Committed: true },
                { Address: 0x10, Value: 0, Committed: true }
            ]
            && wrapping.ExactBoundedCompletion
            && wrapping.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan missingThresholdGlobal =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: null,
                freshThresholdEDXPlus8: 1);
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan missingThresholdPointer =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: null);
        check("Missing threshold reads preserve the outer equality CMP flags and prevent stores",
            missingThresholdGlobal.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingFreshThresholdGlobalValue
            && missingThresholdGlobal.ThresholdRingPlan?.ParentPlan.FirstCompareReached == false
            && missingThresholdGlobal.FlagsAtStop == missingThresholdGlobal.ParentFlagsAtThresholdEntry
            && missingThresholdGlobal.Writes.IsEmpty
            && !missingThresholdGlobal.ExactBoundedCompletion
            && missingThresholdPointer.Outcome
                == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleOutcome
                    .MissingFreshThresholdEDXPlus8
            && missingThresholdPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8ReadReached == true
            && missingThresholdPointer.ThresholdRingPlan?.ParentPlan.FirstCompareReached == false
            && missingThresholdPointer.FlagsAtStop == missingThresholdPointer.ParentFlagsAtThresholdEntry
            && missingThresholdPointer.Writes.IsEmpty);

        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan aliasedThresholdRead =
            Project(
                indexedDWORD: 0x200,
                tagDWORD: 0x100,
                freshThresholdGlobal: 1,
                freshThresholdEDXPlus8: 1,
                entryESI: unchecked(
                    CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress - 0x1C));
        check("The threshold global read remains fresh and unforwarded when an earlier node store aliases it",
            aliasedThresholdRead.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentWrite
                && effect.Address
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress
                && effect.Value == 0
                && effect.Committed)
            && aliasedThresholdRead.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                && effect.Address
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress
                && effect.Value == 1
                && effect.ValueCaptured
                && !effect.ForwardedFromPriorWrite
                && !effect.Committed));

        check("Ordered reads and writes preserve parent evidence, fresh thresholds, five stores, and flags",
            clean.OrderedMemoryEffects
                .Where(effect => effect.Kind
                    is CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[]
                {
                    0x012C93A1u,
                    0x012C93C0u,
                    0x012C93D8u,
                    0x012C9403u,
                    0x012C9409u
                })
            && clean.OrderedMemoryEffects.Where(effect => effect.Kind
                    is CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read)
                .All(effect => effect.Attempted
                    && effect.ValueCaptured
                    && !effect.Committed
                    && !effect.ForwardedFromPriorWrite)
            && clean.OrderedMemoryEffects.Where(effect =>
                    effect.Kind
                        == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .RingWriteAttempt)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[]
                {
                    0x012C941Cu,
                    0x012C941Fu,
                    0x012C9422u,
                    0x012C9425u,
                    0x012C9428u
                })
            && clean.OrderedMemoryEffects.Where(effect =>
                    effect.Kind
                        == CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleMemoryEffectKind
                            .RingWriteAttempt)
                .All(effect => effect.Attempted && effect.Committed)
            && clean.Writes.Length == 5
            && clean.Writes.All(write => write.Committed)
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && !clean.DecAtBoundaryEntered
            && clean.ThresholdRingPlan?.ChildStoresPreserveParentFlags == true);
    }

    private static CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecyclePlan Project(
        uint? indexedDWORD,
        uint? tagDWORD,
        uint? freshThresholdGlobal,
        uint? freshThresholdEDXPlus8,
        uint? entryESI = 0x3000,
        uint size = 0x100,
        uint mask = 1) =>
        CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection.Project(
            new(
                Parent: new(
                    Parent: new(
                        EntryEDI: size,
                        LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                        EntryESI: entryESI,
                        LateFreshGlobalValue: mask),
                    FreshIndexedDWORD: indexedDWORD,
                    FreshEDXPlus4: tagDWORD),
                FreshThresholdGlobalValue: freshThresholdGlobal,
                FreshThresholdEDXPlus8: freshThresholdEDXPlus8));
}
