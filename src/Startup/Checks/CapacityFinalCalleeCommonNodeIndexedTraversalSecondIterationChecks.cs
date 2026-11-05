using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class
    CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan ordinary =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x107);
        CapacityFinalCalleeCommonTraversalIterationPlan? first =
            ordinary.FirstPlan.IterationPlan;
        CapacityFinalCalleeCommonTraversalIterationPlan? second =
            ordinary.SecondIterationPlan;
        check("Only a clean first 9401 loop branch admits one more generic traversal iteration",
            ordinary.FirstPlan.ExactCleanCompletion
            && ordinary.FirstLoopBranchSelected
            && ordinary.FirstLoopBranchDestinationRawFileOffset == 0x012C93E4
            && ordinary.SecondIterationAdmitted
            && ordinary.SecondEntryEBX == first?.EBXAfterDouble
            && ordinary.SecondEntryEBX == 0
            && ordinary.SecondEntryEDX == first?.EDXAfterChildMove
            && ordinary.SecondEntryEDX == 0x5000
            && ordinary.SecondEntryEDI == ordinary.OriginalParentEntryEDI
            && ordinary.SecondEntryEDI == 0x100
            && ordinary.SecondEBXDerivedFromFirstEBXAfterDouble
            && ordinary.SecondEDXDerivedFromFirstChildMove
            && ordinary.SecondEDIDerivedFromOriginalParentEntryEDI
            && ordinary.SecondEntryMovePreservedFirstCompareFlags
            && ordinary.FlagsBeforeSecondEntryMove == first?.CompareFlags
            && second?.Capture.EntryEBX == first?.EBXAfterDouble
            && second?.Capture.LateEntryEDX == first?.EDXAfterChildMove
            && second?.ChildFieldOffset == 16
            && second?.ChildFieldAddress == 0x5010
            && second?.FreshChildDWORD == 0x7000
            && second?.FreshChildPlus4 == 0x107
            && second?.LoopBranchSelectedDestinationRawFileOffset == 0x012C9403
            && ordinary.StopAtRawFileOffsetExclusive == 0x012C9403
            && ordinary.ExactBoundedCompletion
            && ordinary.ExactCleanCompletion
            && !ordinary.SecondIterationTargetEntered
            && !ordinary.ThirdIterationTargetEntered
            && second?.Events.Count(item => item.RawFileOffset == 0x012C93E4) == 1);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan highBitAlias =
            Project(
                0x00BFFF00,
                0x40000000,
                0x220,
                0,
                0x220,
                0x10A,
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x00BFFF07,
                entryESI: 0x1234);
        CapacityFinalCalleeCommonTraversalIterationPlan? highBitFirst =
            highBitAlias.FirstPlan.IterationPlan;
        CapacityFinalCalleeCommonTraversalIterationPlan? highBitSecond =
            highBitAlias.SecondIterationPlan;
        check("The second LEA uses first-child EDX and a second doubled EBX with DWORD wraparound",
            highBitAlias.FirstPlan.DerivedEntryEBX == 0xFFFC0000
            && highBitFirst?.EBXAfterDouble == 0xFFF80000
            && highBitAlias.SecondEntryEBX == 0xFFF80000
            && highBitSecond?.EBXAfterDouble == 0xFFF00000
            && highBitSecond?.ShiftResultEAX == 1
            && highBitSecond?.ChildFieldOffset == 20
            && highBitAlias.SecondEntryEDX == highBitFirst?.FreshChildDWORD
            && highBitAlias.SecondEntryEDX == 0x220
            && highBitSecond?.ChildFieldAddress == highBitFirst?.ChildFieldAddress
            && highBitSecond?.ChildFieldAddress == 0x234
            && highBitSecond?.FreshChildDWORD == 0x7000
            && highBitSecond?.FreshChildDWORD != highBitFirst?.FreshChildDWORD
            && highBitSecond?.FreshChildPlus4Address == 0x7004
            && highBitAlias.SecondEntryEDI == 0x00BFFF00
            && highBitAlias.RootEntryESI == 0x1234
            && highBitAlias.FirstPlan.ParentPlan.ParentPlan.ParentLifecycle?.Capture.EntryESI
                == 0x1234
            && highBitSecond?.LoopBranchSelectedDestinationRawFileOffset == 0x012C9403
            && highBitAlias.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan secondUnequal =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x109);
        check("A second unequal JNZ selects 93E4 but does not enter a third iteration",
            secondUnequal.SecondIterationPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && secondUnequal.SecondSelectedDestinationRawFileOffset == 0x012C93E4
            && secondUnequal.StopAtRawFileOffsetExclusive == 0x012C93E4
            && secondUnequal.ExactBoundedCompletion
            && secondUnequal.ExactCleanCompletion
            && !secondUnequal.ThirdIterationTargetEntered
            && secondUnequal.SecondIterationPlan?.Events.Count(item =>
                item.RawFileOffset == 0x012C93E4) == 1);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan secondZero =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
                freshSecondChildDWORD: 0,
                lateFreshSecondChildPlus4: 0x12345678);
        check("The second zero-child branch keeps the 945C bounded stop and unused tag dirty",
            secondZero.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .SecondZeroChildTo945C
            && secondZero.SecondIterationPlan?.ChildZeroBranchTaken == true
            && secondZero.SecondIterationPlan?.ChildZeroBranchDestinationRawFileOffset
                == 0x012C945C
            && secondZero.SecondIterationPlan?.FreshChildPlus4ReadReached == false
            && secondZero.LateFreshSecondChildPlus4CaptureProvidedButUnused
            && secondZero.ExactBoundedCompletion
            && !secondZero.ExactCleanCompletion
            && secondZero.StopAtRawFileOffsetExclusive == 0x012C945C);
        var cleanSecondZero = Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
            freshSecondChildDWORD: 0, lateFreshSecondChildPlus4: null);
        check("A clean zero second child does not require a tag capture",
            cleanSecondZero.ExactBoundedCompletion
            && cleanSecondZero.ExactCleanCompletion
            && cleanSecondZero.SecondIterationAdmitted
            && cleanSecondZero.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan missingSecondChild =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
                freshSecondChildDWORD: null,
                lateFreshSecondChildPlus4: 0x107);
        check("A missing second child stops at 93F1 with the second ADD flags and no completion",
            missingSecondChild.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondFreshChildDWORDAt93F1
            && missingSecondChild.SecondIterationPlan?.FreshChildReadReached == true
            && missingSecondChild.SecondIterationPlan?.FreshChildDWORDCaptured == false
            && missingSecondChild.SecondIterationPlan?.FreshChildPlus4ReadReached == false
            && missingSecondChild.FlagsAtStop
                == missingSecondChild.SecondIterationPlan?.AddFourFlags
            && !missingSecondChild.ExactBoundedCompletion
            && !missingSecondChild.ExactCleanCompletion
            && missingSecondChild.StopAtRawFileOffsetExclusive == 0x012C93F1);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan missingSecondTag =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x109,
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: null);
        check("A missing second tag stops at 93F9 with TEST flags and no compare or completion",
            missingSecondTag.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationOutcome
                    .MissingSecondFreshChildPlus4At93F9
            && missingSecondTag.SecondIterationPlan?.FreshChildPlus4ReadReached == true
            && missingSecondTag.SecondIterationPlan?.FreshChildPlus4Captured == false
            && missingSecondTag.SecondIterationPlan?.CompareReached == false
            && missingSecondTag.FlagsAtStop
                == missingSecondTag.SecondIterationPlan?.ChildTestFlags
            && !missingSecondTag.ExactBoundedCompletion
            && !missingSecondTag.ExactCleanCompletion
            && missingSecondTag.StopAtRawFileOffsetExclusive == 0x012C93F9);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan firstEqual =
            Project(0x100, 1, 0x300, 0, 0x5000, 0x107,
                freshSecondChildDWORD: 0x7000,
                lateFreshSecondChildPlus4: 0x108);
        check("A clean first equal fallthrough leaves both second-pass captures unused",
            firstEqual.FirstPlan.ExactCleanCompletion
            && firstEqual.FirstPlan.IterationPlan?.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildEqualsEntryEDIAt9403
            && !firstEqual.SecondIterationAdmitted
            && firstEqual.SecondIterationPlan is null
            && firstEqual.FreshSecondChildDWORDCaptureProvidedButUnused
            && firstEqual.LateFreshSecondChildPlus4CaptureProvidedButUnused
            && firstEqual.ExactBoundedCompletion
            && !firstEqual.ExactCleanCompletion
            && firstEqual.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan rootZero =
            Project(0, 0, null, null, null, null,
                freshSecondChildDWORD: 0x1111,
                lateFreshSecondChildPlus4: 0x2222);
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan rootEqual =
            Project(0x100, 1, 0x200, 0x100, null, null,
                freshSecondChildDWORD: 0x1111,
                lateFreshSecondChildPlus4: 0x2222);
        check("Parent zero and equal bypasses retain their original bounded stops",
            rootZero.FirstPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentZeroToBounded93AC
            && rootZero.ExactBoundedCompletion
            && !rootZero.ExactCleanCompletion
            && rootZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && rootEqual.FirstPlan.Outcome
                == CapacityFinalCalleeCommonNodeIndexedTraversalLifecycleOutcome
                    .ParentEqualToBounded9403
            && rootEqual.ExactBoundedCompletion
            && !rootEqual.ExactCleanCompletion
            && rootEqual.StopAtRawFileOffsetExclusive == 0x012C9403
            && rootEqual.FreshSecondChildDWORDCaptureProvidedButUnused
            && rootEqual.LateFreshSecondChildPlus4CaptureProvidedButUnused);

        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan wrapped =
            Project(0x100, 1, 0x300, 0, 0xFFFFFFF8, 0x109,
                freshSecondChildDWORD: 0xFFFFFFFC,
                lateFreshSecondChildPlus4: 0x109);
        check("Second child and tag addresses retain unchecked DWORD wrapping",
            wrapped.SecondIterationPlan?.FreshChildPlus4Address == 0
            && wrapped.SecondIterationPlan?.ChildFieldAddress == 8
            && wrapped.ExactBoundedCompletion);

        check("Second-pass evidence reuses the captured 93E4 through 9403 instruction window",
            InstructionEvidenceMatches());
    }

    private static CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationPlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag,
        uint? freshFirstChildDWORD,
        uint? lateFreshFirstChildPlus4,
        uint? freshSecondChildDWORD,
        uint? lateFreshSecondChildPlus4,
        uint? entryESI = 0x3000) =>
        CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection.Project(
            new(
                new(
                    new(
                        new(
                            EntryEDI: size,
                            LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                            EntryESI: entryESI,
                            LateFreshGlobalValue: mask),
                        FreshIndexedDWORD: freshIndexedDWORD,
                        FreshEDXPlus4: freshTag),
                    FreshChildDWORD: freshFirstChildDWORD,
                    LateFreshChildPlus4: lateFreshFirstChildPlus4),
                FreshSecondChildDWORD: freshSecondChildDWORD,
                LateFreshSecondChildPlus4: lateFreshSecondChildPlus4));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[] {
            (0x012C93E4, "8B C3"),
            (0x012C93E6, "03 DB"),
            (0x012C93E8, "C1 E8 1F"),
            (0x012C93EB, "83 C0 04"),
            (0x012C93EE, "8D 0C 82"),
            (0x012C93F1, "8B 01"),
            (0x012C93F3, "85 C0"),
            (0x012C93F5, "74 65"),
            (0x012C93F7, "8B D0"),
            (0x012C93F9, "8B 42 04"),
            (0x012C93FC, "83 E0 F8"),
            (0x012C93FF, "3B C7"),
            (0x012C9401, "75 E1")
        };
        return CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                .EntryRawFileOffset == 0x012C93E4
            && CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                .ZeroChildBranchTargetRawFileOffset == 0x012C945C
            && CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                .LoopBranchTargetRawFileOffset == 0x012C93E4
            && CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                .EqualFallthroughRawFileOffset == 0x012C9403
            && CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
                .Instructions.Select(item => (item.RawFileOffset, item.Bytes))
                .SequenceEqual(expected);
    }
}
