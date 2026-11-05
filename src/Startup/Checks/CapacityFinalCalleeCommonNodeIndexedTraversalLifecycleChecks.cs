using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan ordinary =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x107);
        check("The clean nonzero parent enters one 93E4 iteration using the parent's shifted EBX",
            ordinary.ExactParentUnequalHandoff
            && ordinary.ParentNonzeroAdmission
            && ordinary.ParentInitialDestinationRawFileOffset == 0x012C93C0
            && ordinary.ParentChildTagDestinationRawFileOffset == 0x012C93E4
            && ordinary.ParentUnequalDestinationSelected
            && ordinary.InternalEntryReached
            && ordinary.InternalEntryRawFileOffset == 0x012C93E4
            && ordinary.DerivedEntryEBX
                == ordinary.ParentPlan.ChildPlan?.ChildPlan?.ShiftResult
            && ordinary.DerivedEntryEBX != ordinary.ParentPlan.DerivedParentEntryEDI
            && ordinary.EntryEBXDerivedFromParentChildTagShiftResult
            && ordinary.DerivedEntryEDX
                == ordinary.ParentPlan.ChildPlan?.ChildEntryEDX
            && ordinary.DerivedEntryEDX
                == ordinary.ParentPlan.ChildPlan?.ParentPlan.FreshEDX
            && ordinary.EntryEDXDerivedFromParentFreshIndexedLoad
            && ordinary.DerivedEntryEDI == 0x100
            && ordinary.DerivedEntryEDI == ordinary.ParentPlan.DerivedParentEntryEDI
            && ordinary.EntryEDIDerivedFromOriginalParentEntryEDI
            && ordinary.IterationPlan?.Capture.EntryEBX == ordinary.DerivedEntryEBX
            && ordinary.IterationPlan?.Capture.LateEntryEDX == ordinary.DerivedEntryEDX
            && ordinary.IterationPlan?.Capture.EntryEDI == ordinary.DerivedEntryEDI
            && ordinary.IterationPlan?.ChildFieldOffset == 16
            && ordinary.IterationPlan?.ChildFieldAddress == 0x310
            && ordinary.IterationPlan?.DoubleEBXFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && ordinary.IterationPlan?.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && ordinary.IterationPlan?.AddFourFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && ordinary.IterationPlan?.FreshChildDWORD == 0x5000
            && ordinary.IterationPlan?.FreshChildPlus4 == 0x107
            && ordinary.IterationPlan?.LoopBranchSelectedDestinationRawFileOffset
                == 0x012C9403
            && ordinary.StopAtRawFileOffsetExclusive == 0x012C9403
            && ordinary.ExactBoundedCompletion
            && ordinary.ExactCleanCompletion
            && ordinary.NoRepeated93D8Read
            && !ordinary.IterationTargetEntered
            && ordinary.IterationPlan?.Events.Count(item =>
                item.RawFileOffset == 0x012C93E4) == 1);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan zeroSized =
            Project(0, 1, 0x300, 8, 0x5000, 7);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan index30Count10 =
            Project(0x00BFFF00, 0x40000000, 0x220, 0, 0x5000, 0x00BFFF07);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan index31CountZero =
            Project(0x00FFFF00, 0x80000000, 0x200, 0, 0x5000, 0x00FFFF07);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan aboveCeiling =
            Project(0x01000000, 0x80000000, 0x400, 0, 0x5000, 0x01000007);
        check("Boundary sizes, count-10/count-zero cases, and late-EBP bypass all reach bounded stops",
            zeroSized.ExactParentUnequalHandoff
            && zeroSized.ParentPlan.ParentPlan.ParentLifecycle?.LateEntryEBPUnusedOnBypass
                == false
            && zeroSized.IterationPlan?.ChildFieldOffset == 16
            && zeroSized.ExactBoundedCompletion
            && index30Count10.ParentPlan.DerivedIndexECX == 30
            && index30Count10.ParentPlan.ChildPlan?.ParentPlan.SubtractResult == 10
            && index30Count10.ParentPlan.ChildPlan?.ChildEntryEAX == 10
            && index30Count10.DerivedEntryEBX == 0xFFFC0000
            && index30Count10.IterationPlan?.ChildFieldOffset == 20
            && index30Count10.ExactCleanCompletion
            && index31CountZero.ParentPlan.DerivedIndexECX == 31
            && index31CountZero.ParentPlan.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && index31CountZero.ParentPlan.ChildPlan?.ChildEntryEAX == 0
            && index31CountZero.DerivedEntryEBX == 0x00FFFF00
            && index31CountZero.IterationPlan?.ChildFieldOffset == 16
            && index31CountZero.ExactCleanCompletion
            && aboveCeiling.ParentPlan.DerivedIndexECX == 31
            && aboveCeiling.ParentPlan.ParentPlan.ParentLifecycle?.FrameStoreCommitted
                == false
            && aboveCeiling.ParentPlan.ParentPlan.ParentLifecycle?.LateEntryEBPUnusedOnBypass
                == false
            && aboveCeiling.ExactParentUnequalHandoff
            && aboveCeiling.FreshChildDWORDCaptureProvidedButUnused == false
            && aboveCeiling.LateFreshChildPlus4CaptureProvidedButUnused == false);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan zeroChild =
            Project(0x100, 1, 0x300, 0, 0, null);
        check("A zero fresh child exits at the unentered 945C boundary without probing child plus four",
            zeroChild.IterationPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C
            && zeroChild.IterationPlan?.ChildZeroBranchTaken == true
            && zeroChild.IterationPlan?.ChildZeroBranchDestinationRawFileOffset
                == 0x012C945C
            && zeroChild.IterationPlan?.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroChild.ExactBoundedCompletion
            && zeroChild.ExactCleanCompletion
            && !zeroChild.IterationTargetEntered
            && !zeroChild.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildPlus4Read));

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan zeroWithUnusedChild4 =
            Project(0x100, 1, 0x300, 0, 0, 0x12345678);
        check("A supplied child-plus-four value on the zero-child branch is visible and blocks clean completion",
            zeroWithUnusedChild4.ExactBoundedCompletion
            && !zeroWithUnusedChild4.ExactCleanCompletion
            && zeroWithUnusedChild4.LateFreshChildPlus4CaptureProvidedButUnused
            && zeroWithUnusedChild4.IterationPlan?.FreshChildPlus4ReadReached == false
            && zeroWithUnusedChild4.IterationPlan?.AndEAXReached == false
            && zeroWithUnusedChild4.IterationPlan?.CompareReached == false
            && zeroWithUnusedChild4.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan missingChild =
            Project(0x100, 1, 0x300, 0, null, null);
        check("A missing fresh child stops at 93F1 before TEST and retains all six last-ADD flags",
            missingChild.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingFreshChildDWORDAt93F1
            && missingChild.IterationPlan?.FreshChildReadReached == true
            && missingChild.IterationPlan?.FreshChildDWORDCaptured == false
            && missingChild.IterationPlan?.ChildTestFlags is null
            && missingChild.IterationPlan?.ChildZeroBranchReached == false
            && missingChild.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChild.FlagsAtStop == missingChild.IterationPlan?.AddFourFlags
            && missingChild.IterationPlan?.Events.Single(item =>
                item.Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.FormChildAddress)
                .Flags == missingChild.IterationPlan?.AddFourFlags
            && missingChild.IterationPlan?.Events.Single(item =>
                item.Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildDWORD)
                .Flags == missingChild.IterationPlan?.AddFourFlags
            && !missingChild.ExactBoundedCompletion
            && !missingChild.ExactCleanCompletion
            && missingChild.StopAtRawFileOffsetExclusive == 0x012C93F1);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan missingChildPlus4 =
            Project(0x100, 1, 0x300, 0, 0x5000, null);
        check("A missing fresh child-plus-four stops at 93F9 with TEST flags and no mask or compare",
            missingChildPlus4.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .MissingLateFreshChildPlus4At93F9
            && missingChildPlus4.IterationPlan?.FreshChildPlus4ReadReached == true
            && missingChildPlus4.IterationPlan?.FreshChildPlus4Captured == false
            && missingChildPlus4.IterationPlan?.AndEAXReached == false
            && missingChildPlus4.IterationPlan?.CompareReached == false
            && missingChildPlus4.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildPlus4.FlagsAtStop
                == missingChildPlus4.IterationPlan?.ChildTestFlags
            && missingChildPlus4.IterationPlan?.Events.Single(item =>
                item.Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.MoveChildToEDX)
                .Flags == missingChildPlus4.IterationPlan?.ChildTestFlags
            && missingChildPlus4.IterationPlan?.Events.Single(item =>
                item.Kind
                    == CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildPlus4)
                .Flags == missingChildPlus4.IterationPlan?.ChildTestFlags
            && missingChildPlus4.StopAtRawFileOffsetExclusive == 0x012C93F9
            && !missingChildPlus4.ExactBoundedCompletion
            && !missingChildPlus4.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan loopBoundary =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109);
        check("The unequal child comparison selects the 93E4 loop boundary but never enters another iteration",
            loopBoundary.IterationPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && loopBoundary.IterationPlan?.MaskedChildDWORD == 0x108
            && loopBoundary.IterationPlan?.CompareRight == 0x100
            && loopBoundary.IterationPlan?.LoopBranchReached == true
            && loopBoundary.IterationPlan?.LoopBranchTaken == true
            && loopBoundary.IterationSelectedDestinationRawFileOffset == 0x012C93E4
            && loopBoundary.IterationPlan?.StopAtRawFileOffsetExclusive == 0x012C93E4
            && loopBoundary.InternalEntryRawFileOffset == 0x012C93E4
            && loopBoundary.IterationPlan?.Events.Count(item =>
                item.RawFileOffset == 0x012C93E4) == 1
            && !loopBoundary.IterationTargetEntered
            && loopBoundary.ExactBoundedCompletion
            && loopBoundary.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan parentZeroNoCaptures =
            Project(0, 0, null, null, null, null);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan parentZeroWithCaptures =
            Project(0, 0, null, null, 0x1111, 0x2222);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan parentEqualNoCaptures =
            Project(0x100, 1, 0x200, 0x100, null, null);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan parentEqualWithCaptures =
            Project(0x100, 1, 0x200, 0x100, 0x1111, 0x2222);
        check("The outer 93AC and 9403 bypasses stay bounded while unused iteration captures spoil cleanliness",
            parentZeroNoCaptures.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                    .ParentZeroToBounded93AC
            && parentZeroNoCaptures.ExactBoundedCompletion
            && parentZeroNoCaptures.ExactCleanCompletion
            && parentZeroWithCaptures.ExactBoundedCompletion
            && !parentZeroWithCaptures.ExactCleanCompletion
            && parentZeroWithCaptures.FreshChildDWORDCaptureProvidedButUnused
            && parentZeroWithCaptures.LateFreshChildPlus4CaptureProvidedButUnused
            && parentZeroWithCaptures.IterationPlan is null
            && parentZeroWithCaptures.StopAtRawFileOffsetExclusive == 0x012C93AC
            && parentEqualNoCaptures.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome
                    .ChildEqualToBounded9403
            && parentEqualNoCaptures.ExactBoundedCompletion
            && parentEqualNoCaptures.ExactCleanCompletion
            && parentEqualWithCaptures.ExactBoundedCompletion
            && !parentEqualWithCaptures.ExactCleanCompletion
            && parentEqualWithCaptures.FreshChildDWORDCaptureProvidedButUnused
            && parentEqualWithCaptures.LateFreshChildPlus4CaptureProvidedButUnused
            && parentEqualWithCaptures.IterationPlan is null
            && parentEqualWithCaptures.StopAtRawFileOffsetExclusive == 0x012C9403);

        uint index30NodeAddress =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.DeriveEBX(30);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan? aliasedIndexedRead =
            new[] { 16u, 20u }
                .Select(slot =>
                {
                    uint indexedPointer = unchecked(index30NodeAddress - slot);
                    return Project(
                        0x00BFFF00,
                        0x40000000,
                        indexedPointer,
                        0,
                        0xA000,
                        0x00BFFF07,
                        entryESI: unchecked(index30NodeAddress - 0x1C));
                })
                .FirstOrDefault(plan =>
                    plan.ExactParentUnequalHandoff
                    && plan.IterationPlan?.ChildFieldAddress == index30NodeAddress);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan? aliasedTagRead =
            aliasedIndexedRead is null
                ? null
                : Project(
                    0x00BFFF00,
                    0x40000000,
                    aliasedIndexedRead.DerivedEntryEDX,
                    0,
                    aliasedIndexedRead.DerivedEntryEDX,
                    0xAABBCCDD,
                    entryESI: unchecked(index30NodeAddress - 0x1C));
        check("Fresh child values remain independent when child reads alias parent writes, index loads, and tag loads",
            aliasedIndexedRead is not null
            && aliasedIndexedRead.ExactCleanCompletion
            && aliasedIndexedRead.ParentPlan.OrderedEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite
                && effect.Address == index30NodeAddress)
            && aliasedIndexedRead.ParentPlan.OrderedEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead
                && effect.Address == index30NodeAddress
                && effect.Value == aliasedIndexedRead.DerivedEntryEDX)
            && aliasedIndexedRead.IterationPlan?.ChildFieldAddress == index30NodeAddress
            && aliasedIndexedRead.IterationPlan?.FreshChildDWORD == 0xA000
            && aliasedIndexedRead.IterationPlan?.FreshChildDWORD
                != aliasedIndexedRead.ParentPlan.ChildPlan?.ParentPlan.FreshEDX
            && aliasedIndexedRead.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildDWORDRead
                && effect.Address == index30NodeAddress
                && effect.Value == 0xA000
                && !effect.ForwardedFromPriorWrite
                && !effect.Committed)
            && aliasedTagRead is not null
            && aliasedTagRead.ExactParentUnequalHandoff
            && aliasedTagRead.IterationPlan?.FreshChildPlus4Address
                == aliasedTagRead.ParentPlan.ChildPlan?.ChildPlan?.FreshReadAddress
            && aliasedTagRead.ParentPlan.OrderedEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshTagDWORDRead
                && effect.Address == aliasedTagRead.IterationPlan?.FreshChildPlus4Address
                && effect.Value == 0)
            && aliasedTagRead.IterationPlan?.FreshChildPlus4 == 0xAABBCCDD
            && aliasedTagRead.OrderedMemoryEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleMemoryEffectKind
                        .FreshChildPlus4Read
                && effect.Address == aliasedTagRead.IterationPlan?.FreshChildPlus4Address
                && effect.Value == 0xAABBCCDD
                && !effect.ForwardedFromPriorWrite
                && !effect.Committed)
            && aliasedTagRead.OrderedMemoryEffects.Take(
                    aliasedTagRead.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(aliasedTagRead.ParentPlan.OrderedEffects.Select(
                    effect => effect.RawFileOffset))
            && aliasedTagRead.OrderedMemoryEffects.TakeLast(2).Select(effect =>
                effect.RawFileOffset).SequenceEqual(new uint[] { 0x012C93F1, 0x012C93F9 })
            && aliasedTagRead.OrderedMemoryEffects.Count(effect =>
                effect.RawFileOffset == 0x012C93D8) == 1
            && aliasedTagRead.NoRepeated93D8Read);

        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan wrap16 =
            Project(0x100, 1, 0xFFFFFFF8, 0, 0xFFFFFFFE, 0x100);
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan wrap20 =
            Project(0x00BFFF00, 0x40000000, 0xFFFFFFF0, 0,
                0xFFFFFFFE, 0x00BFFF07);
        check("Child slots 16 and 20 and both address additions use unchecked DWORD wrapping",
            wrap16.IterationPlan?.ChildFieldOffset == 16
            && wrap16.IterationPlan?.ChildFieldAddress == 8
            && wrap16.IterationPlan?.FreshChildPlus4Address == 2
            && wrap16.ExactBoundedCompletion
            && wrap20.IterationPlan?.ChildFieldOffset == 20
            && wrap20.IterationPlan?.ChildFieldAddress == 4
            && wrap20.IterationPlan?.FreshChildPlus4Address == 2
            && wrap20.ExactBoundedCompletion);

        CapacityFinalCalleeCommonTraversalIterationPlan missingDerivedEBX =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(
                new(
                    EntryEBX: null,
                    LateEntryEDX: null,
                    FreshChildDWORD: null,
                    LateFreshChildPlus4: null,
                    EntryEDI: null));
        check("A missing derived EBX still records the internal first MOV attempt at 93E4 and stops there",
            missingDerivedEBX.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEBXAt93E4
            && missingDerivedEBX.EntryMoveReached
            && !missingDerivedEBX.EntryEBXCaptured
            && missingDerivedEBX.Events[0].RawFileOffset == 0x012C93E4
            && missingDerivedEBX.StopAtRawFileOffsetExclusive == 0x012C93E4
            && !missingDerivedEBX.DoubleEBXReached
            && !missingDerivedEBX.TargetEntered);
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? entryESI = 0x3000) =>
        CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleProjection.Project(
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
                LateFreshChildPlus4: lateFreshChildPlus4));
}
