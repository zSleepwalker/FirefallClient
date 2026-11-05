using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleProjection;

internal static class CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan below =
            Project(0x100, 1, 0x300, 0, 0, null, 0x311);
        check("Only the exact clean zero-child JZ enters the 945C threshold using the 93EE LEA address",
            below.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ZeroChildToBounded945C
            && below.ExactParentZeroHandoff
            && below.ParentExactBoundedCompletion
            && below.ParentExactCleanCompletion
            && below.ChildZero945CSelected
            && below.ChildFieldAddressAt93EE == 0x310
            && below.ParentDerivedEntryEDX == 0x300
            && below.ThresholdEntryECX == below.ChildFieldAddressAt93EE
            && below.ThresholdEntryECX != below.ParentDerivedEntryEDX
            && below.ThresholdEntryECXIsDerivedFromChildFieldAddress
            && below.ThresholdPlan?.CompareLeft == 0x310
            && below.ThresholdPlan?.CompareRight == 0x311
            && below.Internal945CEntered
            && below.ThresholdCompareAttempted
            && below.ThresholdCompareReached
            && below.ThresholdGlobalReadReached
            && below.ThresholdBranchTaken == true
            && below.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.ExactBoundedCompletion
            && below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && !below.ChildZero945CBoundaryTargetEntered
            && !below.Outer8E76Entered
            && !below.Outer9468Entered
            && !below.ThresholdMemoryWriteReached
            && !below.TargetEntered
            && !below.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                        .ThresholdFreshGlobalRead
                && effect.Committed));

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan equal =
            Project(0x100, 1, 0xFFFFFFF0, 0, 0, null, 0);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan signedEdge =
            Project(0x100, 1, 0x7FFFFFF0, 0, 0, null, 1);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan maximum =
            Project(0x100, 1, 0xFFFFFFEF, 0, 0, null, 0);
        check("Unsigned CMP/JB selects 9468 for equal, signed-overflow, and maximum-DWORD boundaries",
            equal.ThresholdEntryECX == 0
            && equal.ThresholdPlan?.CompareResult == 0
            && equal.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.ThresholdBranchTaken == false
            && equal.ThresholdBranchSelectedDestinationRawFileOffset == 0x012C9468
            && equal.StopAtRawFileOffsetExclusive == 0x012C9468
            && equal.ExactBoundedCompletion
            && equal.ExactCleanCompletion
            && !equal.Outer9468Entered
            && signedEdge.ThresholdEntryECX == 0x80000000
            && signedEdge.ThresholdPlan?.CompareResult == 0x7FFFFFFF
            && signedEdge.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedEdge.ThresholdBranchTaken == false
            && maximum.ThresholdEntryECX == uint.MaxValue
            && maximum.ThresholdPlan?.CompareResult == uint.MaxValue
            && maximum.ThresholdPlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maximum.ThresholdBranchTaken == false);

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan index30 =
            Project(0x00BFFF00, 0x40000000, 0x220, 0, 0, null, uint.MaxValue);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan index31 =
            Project(0x00FFFF00, 0x80000000, 0x200, 0, 0, null, 0);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan wrap16 =
            Project(0x100, 1, 0xFFFFFFF8, 0, 0, null, 9);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan wrap20 =
            Project(0x00BFFF00, 0x40000000, 0xFFFFFFF0, 0, 0, null, 0);
        check("Index 30/count 10, index 31/count zero, and child slots 16/20 retain DWORD wraparound",
            index30.TraversalPlan.ParentPlan.DerivedIndexECX == 30
            && index30.TraversalPlan.ParentPlan.ChildPlan?.ParentPlan.SubtractResult == 10
            && index30.TraversalPlan.ParentPlan.ChildPlan?.ChildEntryEAX == 10
            && index30.IterationPlan?.ChildFieldOffset == 20
            && index30.ExactParentZeroHandoff
            && index30.ThresholdBranchTaken == true
            && index31.TraversalPlan.ParentPlan.DerivedIndexECX == 31
            && index31.TraversalPlan.ParentPlan.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && index31.TraversalPlan.ParentPlan.ChildPlan?.ChildEntryEAX == 0
            && index31.IterationPlan?.ChildFieldOffset == 16
            && index31.ExactParentZeroHandoff
            && index31.ThresholdBranchTaken == false
            && wrap16.IterationPlan?.ChildFieldOffset == 16
            && wrap16.ChildFieldAddressAt93EE == 8
            && wrap16.ThresholdBranchTaken == true
            && wrap20.IterationPlan?.ChildFieldOffset == 20
            && wrap20.ChildFieldAddressAt93EE == 4
            && wrap20.ThresholdBranchTaken == false);

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan dirtyChild4 =
            Project(0x100, 1, 0x300, 0, 0, 0xDEADBEEF, 1);
        check("A supplied but unreached child-plus-four blocks threshold admission without losing the bounded 945C exit",
            dirtyChild4.TraversalPlan.ExactBoundedCompletion
            && !dirtyChild4.TraversalPlan.ExactCleanCompletion
            && dirtyChild4.LateFreshChildPlus4CaptureProvidedButUnused
            && dirtyChild4.ChildZero945CSelected
            && !dirtyChild4.ExactParentZeroHandoff
            && !dirtyChild4.ChildZero945CEntered
            && dirtyChild4.ThresholdPlan is null
            && dirtyChild4.FreshGlobalValueCaptureProvidedButUnused
            && dirtyChild4.StopAtRawFileOffsetExclusive == 0x012C945C
            && dirtyChild4.ExactBoundedCompletion
            && !dirtyChild4.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan missingGlobal =
            Project(0x100, 1, 0x300, 0, 0, null, null);
        check("A missing fresh threshold global still attempts the 945C read and preserves all six child TEST flags",
            missingGlobal.ExactParentZeroHandoff
            && missingGlobal.Internal945CEntered
            && missingGlobal.ThresholdGlobalReadReached
            && missingGlobal.ThresholdPlan?.CompareInstructionReached == true
            && missingGlobal.ThresholdPlan?.GlobalAddress == 0x01EDADD8
            && missingGlobal.ThresholdPlan?.CompareLeft == 0x310
            && missingGlobal.ThresholdPlan?.CompareRight is null
            && missingGlobal.ThresholdPlan?.CompareReached == false
            && missingGlobal.ThresholdPlan?.BranchReached == false
            && missingGlobal.FlagsAtStop == missingGlobal.IterationPlan?.ChildTestFlags
            && missingGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingGlobal.OrderedReads[^1] is
            {
                Phase:
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleReadPhase
                        .ZeroSlotThreshold,
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null,
                ReadReached: true
            }
            && !missingGlobal.ExactBoundedCompletion
            && !missingGlobal.ExactCleanCompletion
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan missingChild =
            Project(0x100, 1, 0x300, 0, null, null, 0x11111111);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan missingParent =
            Project(0x100, 1, null, 0, 0, null, 0x22222222, entryESI: null);
        check("Missing child and parent captures stop before threshold admission and leave the new global unused",
            missingChild.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChild.IterationPlan?.FreshChildReadReached == true
            && missingChild.IterationPlan?.FreshChildDWORDCaptured == false
            && missingChild.ThresholdPlan is null
            && missingChild.FreshGlobalValueCaptureProvidedButUnused
            && missingChild.StopAtRawFileOffsetExclusive == 0x012C93F1
            && missingChild.FlagsAtStop == missingChild.IterationPlan?.AddFourFlags
            && missingParent.ThresholdPlan is null
            && missingParent.FreshGlobalValueCaptureProvidedButUnused
            && !missingParent.ExactParentZeroHandoff
            && missingParent.StopAtRawFileOffsetExclusive
                == missingParent.TraversalPlan.StopAtRawFileOffsetExclusive
            && missingParent.StopAtRawFileOffsetExclusive < 0x012C93E4);

        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parentZero =
            Project(0, 0, null, null, 0x11111111, 0x22222222, 0x33333333);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan parentEqual =
            Project(0x100, 1, 0x200, 0x100, 0x11111111, 0x22222222, 0x44444444);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan childEqual =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x107, 0x55555555);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan childLoop =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109, 0x66666666);
        check("Outer zero/equal and nonzero equal/different exits do not consume the threshold capture",
            parentZero.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentZeroToBounded93AC
            && parentZero.ExactBoundedCompletion
            && !parentZero.ExactCleanCompletion
            && parentZero.TraversalPlan.FreshChildDWORDCaptureProvidedButUnused
            && parentZero.LateFreshChildPlus4CaptureProvidedButUnused
            && parentZero.FreshGlobalValueCaptureProvidedButUnused
            && parentZero.ThresholdPlan is null
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && parentEqual.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentEqualToBounded9403
            && parentEqual.ExactBoundedCompletion
            && !parentEqual.ExactCleanCompletion
            && parentEqual.TraversalPlan.FreshChildDWORDCaptureProvidedButUnused
            && parentEqual.LateFreshChildPlus4CaptureProvidedButUnused
            && parentEqual.FreshGlobalValueCaptureProvidedButUnused
            && parentEqual.ThresholdPlan is null
            && parentEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && childEqual.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            && childEqual.ExactBoundedCompletion
            && !childEqual.ExactCleanCompletion
            && childEqual.FreshGlobalValueCaptureProvidedButUnused
            && childEqual.ThresholdPlan is null
            && childEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && childLoop.TraversalOutcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && childLoop.ExactBoundedCompletion
            && !childLoop.ExactCleanCompletion
            && childLoop.FreshGlobalValueCaptureProvidedButUnused
            && childLoop.ThresholdPlan is null
            && childLoop.StopAtRawFileOffsetExclusive == 0x012C93E4);

        uint thresholdAddress =
            CapacityFinalCalleeCommonZeroSlotThresholdGateProjection.GlobalValueVirtualAddress;
        uint writeAliasESI = unchecked(
            thresholdAddress
            - CapacityFinalCalleeCommonIndexedNodeInitializationProjection.FirstStoreFieldOffset);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan aliasedGlobal =
            Project(0x100, 1, 0x300, 0, 0, null, 0xAABBCCDD, writeAliasESI);
        CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleMemoryEffect?
            finalGlobalRead = aliasedGlobal.OrderedMemoryEffects.LastOrDefault(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                        .ThresholdFreshGlobalRead);
        check("The final 1EDADD8 load follows parent effects and remains fresh even when it aliases a parent store",
            aliasedGlobal.ExactParentZeroHandoff
            && aliasedGlobal.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                        .ParentWrite
                && effect.Address == thresholdAddress)
            && aliasedGlobal.OrderedMemoryEffects[^1] == finalGlobalRead
            && finalGlobalRead is
            {
                Kind:
                    CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecycleEffectKind
                        .ThresholdFreshGlobalRead,
                RawFileOffset: 0x012C945C,
                Address: 0x01EDADD8,
                AddressCaptured: true,
                Value: 0xAABBCCDD,
                ValueCaptured: true,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false
            }
            && aliasedGlobal.OrderedReads[^1].RawFileOffset == 0x012C945C
            && aliasedGlobal.OrderedReads.Count(read => read.RawFileOffset == 0x012C93D8) == 1
            && aliasedGlobal.TraversalPlan.NoRepeated93D8Read
            && aliasedGlobal.OrderedInstructions.Count(instruction =>
                instruction.RawFileOffset == 0x012C93E4) == 1);
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalZeroSlotLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? freshGlobalValue,
        uint? entryESI = 0x3000) =>
        Projection.Project(new(
            new(
                new(
                    new(
                        EntryEDI: size,
                        LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                        EntryESI: entryESI,
                        LateFreshGlobalValue: mask),
                    FreshIndexedDWORD: freshIndexedDWORD,
                    FreshEDXPlus4: freshTag),
                FreshChildDWORD: freshChildDWORD,
                LateFreshChildPlus4: lateFreshChildPlus4),
            FreshGlobalValue: freshGlobalValue));
}
