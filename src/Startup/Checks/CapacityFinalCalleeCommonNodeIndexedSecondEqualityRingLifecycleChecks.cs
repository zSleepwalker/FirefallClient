using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class
    CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan clean =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x4000);
        check("Only the clean second masked equality admits the shared five-store ring helper",
            clean.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && clean.ExactSecondChildEqualityHandoff
            && clean.SecondTraversalPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondMaskedChildEqualsEntryEDIAt9403
            && clean.SecondTraversalPlan.FirstPlan.ExactCleanCompletion
            && clean.SecondTraversalPlan.FirstLoopBranchDestinationRawFileOffset
                == 0x012C93E4
            && clean.SecondTraversalPlan.SecondEntryEDX == 0x5000
            && clean.SecondTraversalPlan.SecondEntryEDI == 0x100
            && clean.SecondTraversalPlan.SecondIterationPlan?.FreshChildDWORD == 0x7000
            && clean.SecondTraversalPlan.SecondIterationPlan?.EDXAfterChildMove == 0x7000
            && clean.SecondTraversalPlan.SecondIterationPlan?.MaskedChildDWORD == 0x100
            && clean.SecondTraversalPlan.SecondIterationPlan?.CompareLeft == 0x100
            && clean.SecondTraversalPlan.SecondIterationPlan?.CompareRight == 0x100
            && clean.SecondTraversalPlan.SecondIterationPlan?.LoopBranchTaken == false
            && clean.SecondTraversalPlan.SecondIterationPlan?
                .LoopBranchSelectedDestinationRawFileOffset == 0x012C9403
            && clean.DerivedChildEDX == 0x7000
            && clean.ChildEDXDerivedFromSecondIteration
            && clean.OriginalEntryEDI == clean.SecondTraversalPlan.SecondEntryEDI
            && clean.EntryEDIDerivedFromSecondIteration
            && clean.OriginalRootEntryESI == 0x3000
            && clean.EntryESIInheritedFromOriginalRoot
            && clean.ThresholdRingPlan?.Capture.ParentEntryEDX == 0x7000
            && clean.ThresholdRingPlan?.Capture.LateEntryESI == 0x3000
            && clean.ThresholdRingPlan?.ParentPlan.FreshGlobal == 0x1000
            && clean.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 0x7008
            && clean.ThresholdRingPlan?.ChildEntryEAX == 0x4000
            && clean.ThresholdRingPlan?.ChildEntryESI == 0x3000
            && clean.ThresholdRingPlan?.ChildLateEntryEDX == 0x7000
            && clean.Writes is
            [
                { RawFileOffset: 0x012C941C, Address: 0x400C, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C941F, Address: 0x7008, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C9422, Address: 0x3008, Value: 0x4000, Committed: true },
                { RawFileOffset: 0x012C9425, Address: 0x300C, Value: 0x7000, Committed: true },
                { RawFileOffset: 0x012C9428, Address: 0x3018, Value: 0, Committed: true }
            ]
            && clean.RingBoundaryReached
            && !clean.RingBoundaryInstructionEntered
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && clean.ExactBoundedCompletion
            && clean.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan secondUnequal =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x109,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x3000);
        check("A second unequal compare stops at 93E4 and leaves ring captures unused",
            secondUnequal.SecondTraversalPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondMaskedChildDiffersFromEntryEDITo93E4
            && secondUnequal.SecondTraversalPlan.SecondIterationPlan?
                .LoopBranchSelectedDestinationRawFileOffset == 0x012C93E4
            && secondUnequal.SecondTraversalPlan.StopAtRawFileOffsetExclusive == 0x012C93E4
            && !secondUnequal.ThresholdGateEntered
            && secondUnequal.ThresholdRingPlan is null
            && secondUnequal.Writes.IsEmpty
            && secondUnequal.FreshThresholdGlobalValueCaptureProvidedButUnused
            && secondUnequal.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && secondUnequal.ExactBoundedCompletion
            && !secondUnequal.ExactCleanCompletion
            && secondUnequal.StopAtRawFileOffsetExclusive == 0x012C93E4);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan firstEqual =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x3000,
                lateFreshFirstChildPlus4: 0x107);
        check("An earlier first-pass equality cannot repeat traversal or enter the second ring gate",
            firstEqual.SecondTraversalPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .FirstTraversalNotEligible
            && firstEqual.SecondTraversalPlan.FirstPlan.IterationPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            && firstEqual.SecondTraversalPlan.SecondIterationPlan is null
            && firstEqual.SecondTraversalPlan.StopAtRawFileOffsetExclusive == 0x012C9403
            && !firstEqual.ThresholdGateEntered
            && firstEqual.Writes.IsEmpty
            && firstEqual.FreshThresholdGlobalValueCaptureProvidedButUnused
            && firstEqual.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && firstEqual.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan dirtySecondZero =
            Project(
                freshSecondChildDWORD: 0,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x3000);
        check("A dirty second zero-child exit blocks ring admission and retains the 945C stop",
            dirtySecondZero.SecondTraversalPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondZeroChildTo945C
            && dirtySecondZero.SecondTraversalPlan.SecondIterationPlan?
                .ChildZeroBranchTaken == true
            && dirtySecondZero.SecondTraversalPlan.SecondIterationPlan?
                .FreshChildPlus4ReadReached == false
            && dirtySecondZero.SecondChildPlus4CaptureProvidedButUnused
            && !dirtySecondZero.ThresholdGateEntered
            && dirtySecondZero.ThresholdRingPlan is null
            && dirtySecondZero.Writes.IsEmpty
            && dirtySecondZero.FreshThresholdGlobalValueCaptureProvidedButUnused
            && dirtySecondZero.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && dirtySecondZero.ExactBoundedCompletion
            && !dirtySecondZero.ExactCleanCompletion
            && dirtySecondZero.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan rootNullSecondZero =
            Project(
                freshSecondChildDWORD: 0,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x3000,
                entryESI: null);
        check("A root-null capture preserves its actual 9384 parent stop and leaves second-zero ring inputs unused",
            rootNullSecondZero.SecondTraversalPlan.FirstPlan.ParentPlan.ParentPlan.ParentLifecycle?
                .Outcome == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome
                    .MissingEntryESIAt9384
            && rootNullSecondZero.SecondTraversalPlan.StopAtRawFileOffsetExclusive == 0x012C9384
            && rootNullSecondZero.SecondTraversalPlan.SecondIterationPlan is null
            && rootNullSecondZero.SecondChildDWORDCaptureProvidedButUnused
            && rootNullSecondZero.SecondChildPlus4CaptureProvidedButUnused
            && rootNullSecondZero.FreshThresholdGlobalValueCaptureProvidedButUnused
            && rootNullSecondZero.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && !rootNullSecondZero.ThresholdGateEntered
            && rootNullSecondZero.Writes.IsEmpty
            && rootNullSecondZero.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan cleanSecondZero =
            Project(
                freshSecondChildDWORD: 0,
                lateFreshSecondChildPlus4: null,
                freshThresholdGlobalValue: null,
                freshThresholdEDXPlus8: null);
        check("A clean second zero-child exit needs neither tag nor threshold captures",
            cleanSecondZero.SecondTraversalPlan.SecondIterationPlan?
                .ChildZeroBranchTaken == true
            && !cleanSecondZero.ThresholdGateEntered
            && cleanSecondZero.ExactBoundedCompletion
            && cleanSecondZero.ExactCleanCompletion
            && cleanSecondZero.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan missingSecondTag =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: null,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x3000);
        check("A missing second-child tag stops at 93F9 without equality or ring admission",
            missingSecondTag.SecondTraversalPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondFreshChildPlus4At93F9
            && missingSecondTag.SecondTraversalPlan.SecondIterationPlan?
                .FreshChildPlus4ReadReached == true
            && missingSecondTag.SecondTraversalPlan.SecondIterationPlan?
                .FreshChildPlus4Captured == false
            && missingSecondTag.SecondTraversalPlan.SecondIterationPlan?.CompareReached == false
            && !missingSecondTag.ThresholdGateEntered
            && missingSecondTag.Writes.IsEmpty
            && missingSecondTag.FreshThresholdGlobalValueCaptureProvidedButUnused
            && missingSecondTag.FreshThresholdEDXPlus8CaptureProvidedButUnused
            && !missingSecondTag.ExactBoundedCompletion
            && missingSecondTag.StopAtRawFileOffsetExclusive == 0x012C93F9);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan firstBelow =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x80000000,
                freshThresholdEDXPlus8: 0x90000000);
        check("The first unsigned threshold bypass remains bounded without ring stores",
            firstBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstCompareLeft == 0x7000
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstCompareRight == 0x80000000
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == true
            && firstBelow.ThresholdRingPlan?.ParentPlan.SecondCompareReached == false
            && firstBelow.ThresholdRingPlan?.ChildPlan is null
            && firstBelow.Writes.IsEmpty
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ExactBoundedCompletion
            && firstBelow.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan secondBelow =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: 0x800);
        check("The independent second unsigned threshold bypass remains bounded without stores",
            secondBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == false
            && secondBelow.ThresholdRingPlan?.ParentPlan.SecondJBTaken == true
            && secondBelow.ThresholdRingPlan?.ParentPlan.SecondCompareFlags?.Carry == true
            && secondBelow.ThresholdRingPlan?.ChildPlan is null
            && secondBelow.Writes.IsEmpty
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && secondBelow.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan missingGlobal =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: null,
                freshThresholdEDXPlus8: 0x3000);
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan missingPointer =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 0x1000,
                freshThresholdEDXPlus8: null);
        check("The threshold global and second-EDX-plus-eight reads are independent required captures",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingFreshThresholdGlobalValue
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FreshGlobalReadReached == true
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FreshGlobalCaptured == false
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8ReadReached == false
            && missingGlobal.Writes.IsEmpty
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9403
            && !missingGlobal.ExactBoundedCompletion
            && missingPointer.Outcome
                == CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleOutcome
                    .MissingFreshThresholdEDXPlus8
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshGlobalCaptured == true
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8ReadReached == true
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 0x7008
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Captured == false
            && missingPointer.Writes.IsEmpty
            && missingPointer.StopAtRawFileOffsetExclusive == 0x012C9409
            && !missingPointer.ExactBoundedCompletion);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan wrapped =
            Project(
                freshFirstChildDWORD: 0xFFFFFFF8,
                freshSecondChildDWORD: 0xFFFFFFFC,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 1,
                freshThresholdEDXPlus8: 0xFFFFFFF8);
        check("Second-child and ring pointer arithmetic wrap independently at DWORD boundaries",
            wrapped.SecondTraversalPlan.SecondIterationPlan?.FreshChildPlus4Address == 0
            && wrapped.SecondTraversalPlan.SecondIterationPlan?.ChildFieldAddress == 8
            && wrapped.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 4
            && wrapped.ThresholdRingPlan?.ChildEntryEAX == 0xFFFFFFF8
            && wrapped.Writes is
            [
                { Address: 4, Value: 0x3000, Committed: true },
                { Address: 4, Value: 0x3000, Committed: true },
                { Address: 0x3008, Value: 0xFFFFFFF8, Committed: true },
                { Address: 0x300C, Value: 0xFFFFFFFC, Committed: true },
                { Address: 0x3018, Value: 0, Committed: true }
            ]
            && wrapped.ExactBoundedCompletion
            && wrapped.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan aliasedGlobal =
            Project(
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107,
                freshThresholdGlobalValue: 1,
                freshThresholdEDXPlus8: 0x3000,
                entryESI: unchecked(
                    CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress - 0x1C));
        check("The post-parent threshold global read remains fresh when an earlier node write aliases it",
            aliasedGlobal.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentWrite
                && effect.Address
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress
                && effect.Value == 0
                && effect.Committed)
            && aliasedGlobal.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                && effect.RawFileOffset == 0x012C9403
                && effect.Address
                    == CapacityFinalCalleeCommonDualPointerThresholdProjection
                        .FreshGlobalVirtualAddress
                && effect.Value == 1
                && effect.ValueCaptured
                && !effect.ForwardedFromPriorWrite
                && !effect.Committed)
            && aliasedGlobal.ThresholdRingPlan?.ParentPlan.FreshGlobal == 1);

        check("The combined trace preserves both traversal passes, independent threshold reads, and five stores",
            clean.OrderedMemoryEffects
                .Where(effect => effect.Kind
                    is CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshIndexedDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ParentFreshTagDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildDWORDRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .FreshTraversalChildPlus4Read
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[]
                {
                    0x012C93A1u,
                    0x012C93C0u,
                    0x012C93D8u,
                    0x012C93F1u,
                    0x012C93F9u,
                    0x012C93F1u,
                    0x012C93F9u,
                    0x012C9403u,
                    0x012C9409u
                })
            && clean.OrderedMemoryEffects.Where(effect =>
                    effect.Kind
                        == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
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
                        == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshGlobalRead
                    || effect.Kind
                        == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                            .ThresholdFreshEDXPlus8Read)
                .All(effect => effect.Attempted
                    && effect.ValueCaptured
                    && !effect.Committed
                    && !effect.ForwardedFromPriorWrite)
            && clean.Writes.Length == 5
            && clean.Writes.All(write => write.Committed)
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && !clean.RingBoundaryInstructionEntered);
    }

    private static CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecyclePlan Project(
        uint? freshSecondChildDWORD,
        uint? lateFreshSecondChildPlus4,
        uint? freshThresholdGlobalValue,
        uint? freshThresholdEDXPlus8,
        uint? freshFirstChildDWORD = 0x5000,
        uint? lateFreshFirstChildPlus4 = 0x109,
        uint? entryESI = 0x3000,
        uint size = 0x100,
        uint mask = 1) =>
        CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection.Project(new(
            Traversal: new(
                FirstTraversal: new(
                    Parent: new(
                        Parent: new(
                            EntryEDI: size,
                            LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                            EntryESI: entryESI,
                            LateFreshGlobalValue: mask),
                        FreshIndexedDWORD: 0x300,
                        FreshEDXPlus4: 0),
                    FreshChildDWORD: freshFirstChildDWORD,
                    LateFreshChildPlus4: lateFreshFirstChildPlus4),
                FreshSecondChildDWORD: freshSecondChildDWORD,
                LateFreshSecondChildPlus4: lateFreshSecondChildPlus4),
            FreshThresholdGlobalValue: freshThresholdGlobalValue,
            FreshThresholdEDXPlus8: freshThresholdEDXPlus8));
}
