using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan clean =
            Project(
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0x300,
                freshTagDWORD: 0,
                freshChildDWORD: 0x5000,
                freshChildPlus4: 0x107,
                freshChildGlobalValue: 0x1000,
                freshChildEDXPlus8: 0x3000);
        check("The exact nonzero child equality fallthrough gates two independent reads and five stores",
            clean.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && clean.ExactChildEqualityHandoff
            && clean.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            && clean.TraversalPlan.ParentNonzeroAdmission
            && clean.TraversalPlan.ExactParentUnequalHandoff
            && clean.TraversalPlan.IterationPlan?.FreshChildDWORD == 0x5000
            && clean.TraversalPlan.IterationPlan?.ChildZeroBranchTaken == false
            && clean.TraversalPlan.IterationPlan?.EDXAfterChildMove == 0x5000
            && clean.TraversalPlan.IterationPlan?.MaskedChildDWORD == 0x100
            && clean.TraversalPlan.IterationPlan?.CompareRight == 0x100
            && clean.TraversalPlan.IterationPlan?.LoopBranchTaken == false
            && clean.TraversalPlan.IterationPlan?.LoopBranchSelectedDestinationRawFileOffset
                == 0x012C9403
            && clean.DerivedChildEDX == 0x5000
            && clean.ChildEDXDerivedFromIterationPlan
            && clean.ThresholdRingPlan?.ParentPlan.Capture.EntryEDX == 0x5000
            && clean.ThresholdRingPlan?.ChildEntryEAX == 0x3000
            && clean.InheritedEntryESI == 0x3000
            && clean.EntryESIInheritedFromTraversalParent
            && clean.ThresholdRingPlan?.ChildEntryESI == 0x3000
            && clean.ThresholdRingPlan?.ChildLateEntryEDX == 0x5000
            && clean.ThresholdFallthroughSelected
            && clean.ThresholdRingPlan?.Writes is
            [
                { RawFileOffset: 0x012C941C, Address: 0x300C, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C941F, Address: 0x5008, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C9422, Address: 0x3008, Value: 0x3000, Committed: true },
                { RawFileOffset: 0x012C9425, Address: 0x300C, Value: 0x5000, Committed: true },
                { RawFileOffset: 0x012C9428, Address: 0x3018, Value: 0, Committed: true }
            ]
            && clean.RingBoundaryReached
            && !clean.RingBoundaryInstructionEntered
            && clean.ThresholdRingPlan?.DecAtBoundaryEntered == false
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && clean.ExactBoundedCompletion
            && clean.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan loopBackedge =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x109, 0x1000, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan cleanZeroChild =
            Project(
                0x100, 1, 0x300, 0, 0, null, null, null);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan zeroChild =
            Project(
                0x100, 1, 0x300, 0, 0, null, 0x1000, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan cleanOuterEqual =
            Project(
                0x100, 1, 0x200, 0x100, null, null, null, null);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan outerEqual =
            Project(
                0x100, 1, 0x200, 0x100, 0x5000, 0x107, 0x1000, 0x3000);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan cleanParentZero =
            Project(
                0, 0, null, null, null, null, null, null);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan parentZero =
            Project(
                0, 0, null, null, null, null, 0x1000, 0x3000);
        check("Loop backedge, zero child, outer equality, and outer zero remain distinct bounded stops",
            loopBackedge.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && loopBackedge.TraversalPlan.IterationPlan?.LoopBranchTaken == true
            && loopBackedge.StopAtRawFileOffsetExclusive == 0x012C93E4
            && !loopBackedge.TraversalPlan.IterationTargetEntered
            && !loopBackedge.ThresholdGateEntered
            && zeroChild.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ZeroChildToBounded945C
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && !zeroChild.ExactCleanCompletion
            && !zeroChild.ThresholdGateEntered
            && cleanZeroChild.TraversalPlan.ExactCleanCompletion
            && cleanZeroChild.ExactCleanCompletion
            && cleanZeroChild.ExactBoundedCompletion
            && cleanZeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanOuterEqual.TraversalPlan.ExactCleanCompletion
            && cleanOuterEqual.ExactCleanCompletion
            && cleanOuterEqual.ExactBoundedCompletion
            && outerEqual.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentEqualToBounded9403
            && outerEqual.TraversalPlan.IterationPlan is null
            && outerEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && !outerEqual.ExactCleanCompletion
            && !outerEqual.ThresholdGateEntered
            && cleanParentZero.TraversalPlan.ExactCleanCompletion
            && cleanParentZero.ExactCleanCompletion
            && cleanParentZero.ExactBoundedCompletion
            && cleanParentZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && parentZero.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentZeroToBounded93AC
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && !parentZero.ThresholdGateEntered);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan dirtyOuterEqual =
            Project(
                0x100, 1, 0x200, 0x100, 0x5000, 0x107, 0x1000, 0x3000);
        check("Unused threshold captures cannot turn an earlier outer 9403 equality into the child gate",
            dirtyOuterEqual.TraversalPlan.ExactBoundedCompletion
            && !dirtyOuterEqual.TraversalPlan.ExactCleanCompletion
            && dirtyOuterEqual.FreshChildGlobalValueCaptureProvidedButUnused
            && dirtyOuterEqual.FreshChildEDXPlus8CaptureProvidedButUnused
            && dirtyOuterEqual.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentEqualToBounded9403
            && dirtyOuterEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && !dirtyOuterEqual.ThresholdGateEntered
            && dirtyOuterEqual.ThresholdRingPlan is null);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan firstBelow =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x107, 0x80000000, 0x90000000);
        check("The first threshold uses unsigned comparison and bypasses the ring at 8E76",
            firstBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesRing
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == true
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstCompareLeft == 0x5000
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstCompareRight == 0x80000000
            && firstBelow.ThresholdRingPlan?.ParentPlan.FirstCompareFlags?.Carry == true
            && firstBelow.ThresholdRingPlan?.ParentPlan.SecondCompareReached == false
            && firstBelow.ThresholdRingPlan?.ChildPlan is null
            && firstBelow.ThresholdRingPlan?.Writes.IsEmpty == true
            && firstBelow.ThresholdRingPlan?.LateEntryESIProvidedButUnused == true
            && firstBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBelow.ExactBoundedCompletion
            && firstBelow.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan secondBelow =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x107, 0x1000, 0x800);
        check("The second threshold can independently bypass after the first unsigned compare falls through",
            secondBelow.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesRing
            && secondBelow.ThresholdRingPlan?.ParentPlan.FirstJBTaken == false
            && secondBelow.ThresholdRingPlan?.ParentPlan.SecondJBTaken == true
            && secondBelow.ThresholdRingPlan?.ParentPlan.SecondCompareFlags?.Carry == true
            && secondBelow.ThresholdRingPlan?.ChildPlan is null
            && secondBelow.ThresholdRingPlan?.Writes.IsEmpty == true
            && secondBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBelow.ExactBoundedCompletion
            && secondBelow.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan missingGlobal =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x107, null, 0x3000);
        check("A missing 9403 global read preserves all six traversal CMP flags and tracks the unreached pointer input",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingFreshChildGlobalValue
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FreshGlobalReadReached == true
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FreshGlobalCaptured == false
            && missingGlobal.ThresholdRingPlan?.ParentPlan.FirstCompareReached == false
            && missingGlobal.ThresholdRingPlan?.OrderedMemoryEffects is
                [{ RawFileOffset: 0x012C9403, ValueCaptured: false, Committed: false }]
            && missingGlobal.FreshChildEDXPlus8CaptureProvidedButUnused
            && missingGlobal.FlagsAtStop == missingGlobal.TraversalFlagsAtStop
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9403
            && !missingGlobal.ExactBoundedCompletion
            && !missingGlobal.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan missingPointer =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x107, 0x1000, null);
        check("A missing 9409 child pointer keeps the reached gate read and preserves the prior six flags",
            missingPointer.Outcome
                == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleOutcome
                    .MissingFreshChildEDXPlus8
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshGlobalCaptured == true
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8ReadReached == true
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 0x5008
            && missingPointer.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Captured == false
            && missingPointer.ThresholdRingPlan?.ParentPlan.FirstCompareReached == false
            && missingPointer.ThresholdRingPlan?.OrderedMemoryEffects is
            [
                { RawFileOffset: 0x012C9403, ValueCaptured: true, Committed: false },
                { RawFileOffset: 0x012C9409, Address: 0x5008, ValueCaptured: false, Committed: false }
            ]
            && missingPointer.FlagsAtStop == missingPointer.TraversalFlagsAtStop
            && missingPointer.StopAtRawFileOffsetExclusive == 0x012C9409
            && !missingPointer.ExactBoundedCompletion);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan missingESI =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x5000,
                FreshEDXPlus8: 0x3000,
                LateEntryESI: null));
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan noInheritedESI =
            Project(
                0x100, 1, 0x300, 0, 0x5000, 0x107, 0x1000, 0x3000,
                entryESI: null);
        check("The real store API does not default missing ESI, while composition inherits only the nested parent capture",
            missingESI.ParentExactFallthroughHandoff
            && missingESI.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .MissingLateEntryESIForFirstWrite
            && missingESI.Writes is
            [{
                RawFileOffset: 0x012C941C,
                Address: 0x300C,
                AddressCaptured: true,
                Value: null,
                ValueCaptured: false,
                Committed: false
            }]
            && !missingESI.ExactCleanCompletion
            && noInheritedESI.InheritedEntryESI is null
            && !noInheritedESI.EntryESIInheritedFromTraversalParent
            && !noInheritedESI.ThresholdGateEntered
            && noInheritedESI.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome.ParentStopped
            && noInheritedESI.TraversalPlan.ParentPlan.ParentPlan.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryESIAt9384);

        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan wrappedAliased =
            Project(
                0x100,
                1,
                0x300,
                0,
                0xFFFFFFFC,
                0x107,
                0,
                0xFFFFFFF8,
                entryESI: 0xFFFFFFF4);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect aliasedRead =
            wrappedAliased.OrderedMemoryEffects.Single(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read);
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffect aliasedWrite =
            wrappedAliased.OrderedMemoryEffects.Single(effect =>
                effect.RawFileOffset == 0x012C941F);
        check("Fresh threshold captures remain independent across unsigned values, DWORD wrapping, and an aliased later store",
            wrappedAliased.ExactCleanCompletion
            && wrappedAliased.InheritedEntryESI == 0xFFFFFFF4
            && wrappedAliased.EntryESIInheritedFromTraversalParent
            && wrappedAliased.ThresholdRingPlan?.ChildEntryESI == 0xFFFFFFF4
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.FreshEDXPlus8Address == 4
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.FirstCompareLeft == 0xFFFFFFFC
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.FirstCompareRight == 0
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.SecondCompareLeft == 0xFFFFFFF8
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.SecondCompareRight == 0
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.FirstJBTaken == false
            && wrappedAliased.ThresholdRingPlan?.ParentPlan.SecondJBTaken == false
            && aliasedRead.Address == 4
            && aliasedRead.Value == 0xFFFFFFF8
            && aliasedRead.Attempted
            && aliasedRead.ValueCaptured
            && !aliasedRead.Committed
            && !aliasedRead.ForwardedFromPriorWrite
            && aliasedWrite.Address == aliasedRead.Address
            && aliasedWrite.Value == 0xFFFFFFF4
            && aliasedWrite.Attempted
            && aliasedWrite.Committed
            && wrappedAliased.ThresholdRingPlan?.Writes is
            [
                { Address: 4, Value: 0xFFFFFFF4, Committed: true },
                { Address: 4, Value: 0xFFFFFFF4, Committed: true },
                { Address: 0xFFFFFFFC, Value: 0xFFFFFFF8, Committed: true },
                { Address: 0, Value: 0xFFFFFFFC, Committed: true },
                { Address: 0x0C, Value: 0, Committed: true }
            ]);

        check("Parent effects precede fresh child threshold reads, which precede ordered store attempts",
            clean.TraversalPlan.OrderedMemoryEffects.Length > 0
            && clean.OrderedMemoryEffects.Take(
                    clean.TraversalPlan.OrderedMemoryEffects.Length)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(clean.TraversalPlan.OrderedMemoryEffects.Select(
                    effect => effect.RawFileOffset))
            && clean.OrderedMemoryEffects.TakeLast(7).Select(effect =>
                effect.RawFileOffset).SequenceEqual(
                new[]
                {
                    0x012C9403u,
                    0x012C9409u,
                    0x012C941Cu,
                    0x012C941Fu,
                    0x012C9422u,
                    0x012C9425u,
                    0x012C9428u
                })
            && clean.OrderedMemoryEffects.TakeLast(7).Take(2).All(effect =>
                (effect.Kind is
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshGlobalRead
                    or CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .ThresholdFreshEDXPlus8Read)
                && effect.Attempted
                && effect.ValueCaptured
                && !effect.Committed)
            && clean.OrderedMemoryEffects.TakeLast(5).All(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
                        .RingWriteAttempt
                && effect.Attempted
                && effect.Committed)
            && clean.OrderedMemoryEffects.Where(effect =>
                effect.Kind is
                    CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleMemoryEffectKind
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
                .All(effect => effect.Attempted && !effect.Committed));

        check("The sampled 93F9, 9401, and 9403 instructions retain their exact offsets and VA delta",
            InstructionEvidenceMatches());
    }

    private static bool InstructionEvidenceMatches()
    {
        CapacityFinalCalleeCommonTraversalIterationInstructionEvidence[] traversal =
            CapacityFinalCalleeCommonTraversalIterationProjection.Instructions.ToArray();
        CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence[] threshold =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Instructions.ToArray();
        return traversal.Single(instruction =>
                instruction.RawFileOffset == 0x012C93F9)
                is
                {
                    Bytes: "8B 42 04",
                    VirtualAddress: 0x016C9FF9
                }
            && traversal.Single(instruction =>
                instruction.RawFileOffset == 0x012C9401)
                is
                {
                    Bytes: "75 E1",
                    BranchTargetRawFileOffset: 0x012C93E4
                }
            && threshold[0].RawFileOffset == 0x012C9403
            && threshold[0].Bytes == "8B 0D D8 AD ED 01"
            && threshold[0].VirtualAddress == 0x016CA003
            && CapacityFinalCalleeCommonDualPointerThresholdProjection
                .FreshGlobalVirtualAddress == 0x01EDADD8
            && CapacityFinalCalleeCommonDualPointerThresholdProjection
                .RawFileOffsetToVirtualAddress(0x012C9403) == 0x016CA003
            && CapacityFinalCalleeCommonDualPointerThresholdProjection
                .CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00;
    }

    private static CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTagDWORD,
        uint? freshChildDWORD,
        uint? freshChildPlus4,
        uint? freshChildGlobalValue,
        uint? freshChildEDXPlus8,
        uint? entryESI = 0x3000) =>
        CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection.Project(new(
            Traversal: new(
                Parent: new(
                    Parent: new(
                        EntryEDI: size,
                        LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                        EntryESI: entryESI,
                        LateFreshGlobalValue: mask),
                    FreshIndexedDWORD: freshIndexedDWORD,
                    FreshEDXPlus4: freshTagDWORD),
                FreshChildDWORD: freshChildDWORD,
                LateFreshChildPlus4: freshChildPlus4),
            FreshChildGlobalValue: freshChildGlobalValue,
            FreshChildEDXPlus8: freshChildEDXPlus8));
}
