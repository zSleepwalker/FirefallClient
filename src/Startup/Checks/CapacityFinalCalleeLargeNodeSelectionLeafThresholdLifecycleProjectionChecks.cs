using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeNodeSelectionLeafThresholdLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan derivedEDX = Project(
            new(0x118, 0x31, 0x200, 0, 0),
            freshGlobalValue: 0x30,
            freshEbpMinusC: 0xAABBCCDD);
        check("The exact clean second-field-zero branch hands the saved frame EDX to 920B",
            derivedEDX.ParentPlan.ExactCleanExitAdmitted
            && derivedEDX.ParentPlan.Plus10BranchReached
            && derivedEDX.ParentPlan.Plus10BranchTaken == true
            && derivedEDX.ParentPlan.Plus10BranchSelectedDestinationRawFileOffset
                == 0x012C920B
            && derivedEDX.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C920B
            && derivedEDX.ChildReached
            && derivedEDX.ChildEntryEDX == 0x31
            && derivedEDX.ChildEntryEDXSourceRawFileOffset == 0x012C91F0
            && derivedEDX.ChildEntryEDX != derivedEDX.Capture.ParentSelection.EntryEAX
            && derivedEDX.ChildEntryEDX != derivedEDX.ParentPlan.Plus10LeaECXValue
            && derivedEDX.ChildPlan?.Capture.EntryEDX == 0x31
            && derivedEDX.ChildEntryEBP == 0x118
            && derivedEDX.ChildPlan?.StoreAddress == 0x114
            && derivedEDX.ChildPlan?.GlobalValue == 0x30
            && derivedEDX.ChildPlan?.StoreValue == 0x31
            && derivedEDX.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                    .UnsignedNotBelowFallthrough
            && derivedEDX.ChildPlan?.BranchSelectedDestinationRawFileOffset == 0x012C921D
            && derivedEDX.ExactCleanExitAdmitted
            && derivedEDX.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan missingGlobal = Project(
            new(0x118, 0x31, 0x200, 0, 0),
            freshGlobalValue: null,
            freshEbpMinusC: 0xABCDEF01);
        check("A missing child global retains the derived EDX and the parent CMP flags",
            missingGlobal.ChildReached
            && missingGlobal.ChildEntryEDX == 0x31
            && missingGlobal.ChildPlan?.EntryEDXCaptured == true
            && missingGlobal.ChildPlan?.GlobalReadReached == true
            && missingGlobal.ChildPlan?.GlobalValueCaptured == false
            && missingGlobal.ChildPlan?.CompareReached == false
            && missingGlobal.ChildEntryEBP is null
            && missingGlobal.ChildPlan?.UnreachedEntryEBPCaptureProvided == false
            && missingGlobal.ChildPlan?.StoreReached == false
            && missingGlobal.ChildPlan?.FreshFrameReadReached == false
            && missingGlobal.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C91F0, 0x012C91F3, 0x012C91FC, 0x012C920B })
            && missingGlobal.Writes.IsEmpty
            && missingGlobal.ParentCompareFlagsAtStop is { Carry: false, Zero: true }
            && missingGlobal.ParentPlan.Plus10CompareFlags is { Carry: false, Zero: true }
            && missingGlobal.ChildCompareFlagsAtStop is null
            && missingGlobal.FlagsAtStop == missingGlobal.ParentCompareFlagsAtStop
            && missingGlobal.ParentCompareFlagsPreservedWhenChildCompareMissing
            && missingGlobal.UnreachedFreshFrameCaptureProvided
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan missingFreshFrame = Project(
            new(0x104, 0x31, 0xF0, 0, 0),
            freshGlobalValue: 0x30,
            freshEbpMinusC: null);
        CapacityFinalCalleeLargeLeafThresholdGatePlan? missingFreshFrameChild =
            missingFreshFrame.ChildPlan;
        check("A missing later frame reload retains the child CMP, store, and ordered effects",
            missingFreshFrameChild is not null
            && missingFreshFrameChild.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshEbpMinusC
            && missingFreshFrameChild.CompareReached
            && missingFreshFrameChild.CompareFlags is
                { Carry: false, Zero: false, Parity: false }
            && missingFreshFrameChild.StoreAddress == 0x100
            && missingFreshFrameChild.StoreValue == 0x31
            && missingFreshFrameChild.FreshFrameReadReached
            && missingFreshFrameChild.FreshFrameAddress == 0xF8
            && !missingFreshFrameChild.FreshFrameCaptured
            && missingFreshFrame.ChildCompareFlagsAtStop
                != missingFreshFrame.ParentCompareFlagsAtStop
            && missingFreshFrame.FlagsAtStop == missingFreshFrame.ChildCompareFlagsAtStop
            && !missingFreshFrame.ParentCompareFlagsPreservedWhenChildCompareMissing
            && missingFreshFrame.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[]
                {
                    0x012C91F0,
                    0x012C91F3,
                    0x012C91FC,
                    0x012C920B,
                    0x012C9214
                })
            && missingFreshFrame.Writes is
                [{ RawFileOffset: 0x012C9211, Address: 0x100, Value: 0x31, Committed: true }]
            && missingFreshFrame.Accesses.Select(access => (access.Kind, access.RawFileOffset))
                .SequenceEqual(new[]
                {
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read, 0x012C91F0u),
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read, 0x012C91F3u),
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read, 0x012C91FCu),
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read, 0x012C920Bu),
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Write, 0x012C9211u),
                    (CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read, 0x012C9214u)
                })
            && !missingFreshFrame.ExactCleanExitAdmitted
            && missingFreshFrame.StopAtRawFileOffsetExclusive == 0x012C9214);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan below = Project(
            new(0x200, 0, 0x300, 0, 0),
            freshGlobalValue: 1,
            freshEbpMinusC: 0x44);
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan notBelow = Project(
            new(0x200, 0x80000000, 0x300, 0, 0),
            freshGlobalValue: 0x7FFFFFFF,
            freshEbpMinusC: 0x44);
        CapacityFinalCalleeLargeLeafThresholdGatePlan? belowChild = below.ChildPlan;
        CapacityFinalCalleeLargeLeafThresholdGatePlan? notBelowChild = notBelow.ChildPlan;
        check("Completed unsigned branches stop at 8E76 or 921D without entering clear or retry",
            below.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                    .UnsignedBelowBranchTarget
            && belowChild is not null
            && belowChild.BranchTaken == true
            && belowChild.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.Accesses[^1].RawFileOffset == 0x012C9214
            && below.ExactCleanExitAdmitted
            && notBelow.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                    .UnsignedNotBelowFallthrough
            && notBelowChild is not null
            && notBelowChild.BranchTaken == false
            && notBelowChild.BranchSelectedDestinationRawFileOffset == 0x012C921D
            && notBelow.StopAtRawFileOffsetExclusive == 0x012C921D
            && notBelow.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan wrapped = Project(
            new(0, 0, 0xFFFFFFF0, 0, 0),
            freshGlobalValue: 0,
            freshEbpMinusC: 0x12345678);
        check("Zero values and DWORD-wrapped parent and child addresses remain independent",
            wrapped.ParentPlan.FrameReadAddress == 0xFFFFFFFC
            && wrapped.ParentPlan.FreshEAXPlus14Address == 4
            && wrapped.ParentPlan.FreshEAXPlus10Address == 0
            && wrapped.ChildEntryEDX == 0
            && wrapped.ChildPlan?.StoreAddress == 0xFFFFFFFC
            && wrapped.ChildPlan?.FreshFrameAddress == 0xFFFFFFF4
            && wrapped.ChildPlan?.BranchTaken == false
            && wrapped.ChildPlan?.CompareFlags is { Zero: true }
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan aliasedReads = Project(
            new(0x104, 0x31, 0xF0, 0, 0),
            freshGlobalValue: 0x31,
            freshEbpMinusC: 0x31);
        check("Aliased parent frame and plus-10 reads remain distinct before the child store",
            aliasedReads.Reads[0].Address == aliasedReads.Reads[2].Address
            && aliasedReads.Reads[0].RawFileOffset == 0x012C91F0
            && aliasedReads.Reads[0].Value == 0x31
            && aliasedReads.Reads[2].RawFileOffset == 0x012C91FC
            && aliasedReads.Reads[2].Value == 0
            && aliasedReads.ChildPlan?.StoreAddress == aliasedReads.Reads[0].Address
            && aliasedReads.Accesses[0].Kind
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read
            && aliasedReads.Accesses[2].Kind
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Read
            && aliasedReads.Accesses[4].Kind
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdAccessKind.Write
            && aliasedReads.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan other9205WithUnusedChildValues =
            Project(
                new(0x118, 0x10, 0x100, 1, null),
                freshGlobalValue: 0x20,
                freshEbpMinusC: 0x40);
        check("The exact other 9205 route is bounded without a child and rejects unused gate values",
            other9205WithUnusedChildValues.ParentPlan.ExactCleanExitAdmitted
            && other9205WithUnusedChildValues.ParentPlan.StopAtRawFileOffsetExclusive
                == 0x012C9205
            && other9205WithUnusedChildValues.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome
                    .ParentBoundedAt9205
            && !other9205WithUnusedChildValues.ChildReached
            && other9205WithUnusedChildValues.ChildPlan is null
            && other9205WithUnusedChildValues.UnreachedFreshGlobalCaptureProvided
            && other9205WithUnusedChildValues.UnreachedFreshFrameCaptureProvided
            && !other9205WithUnusedChildValues.ExactCleanExitAdmitted
            && other9205WithUnusedChildValues.StopAtRawFileOffsetExclusive == 0x012C9205);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan other9205Clean = Project(
            new(0x118, 0x10, 0x100, 1, null),
            freshGlobalValue: null,
            freshEbpMinusC: null);
        check("The clean 9205 sibling endpoint remains bounded and does not invoke the gate",
            other9205Clean.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome.ParentBoundedAt9205
            && !other9205Clean.ChildReached
            && other9205Clean.ChildPlan is null
            && other9205Clean.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C91F0, 0x012C91F3 })
            && other9205Clean.Writes.IsEmpty
            && other9205Clean.ExactCleanExitAdmitted
            && other9205Clean.StopAtRawFileOffsetExclusive == 0x012C9205);

        CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan blockedParent = Project(
            new(0x118, 0x10, 0x100, null, 0),
            freshGlobalValue: 0x20,
            freshEbpMinusC: 0x40);
        check("A missing parent field read cannot be promoted into the 920B child route",
            blockedParent.ParentPlan.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingFreshEAXPlus14
            && blockedParent.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C91F3
            && blockedParent.Outcome
                == CapacityFinalCalleeLargeNodeSelectionLeafThresholdOutcome.ParentSelectionBlocked
            && !blockedParent.ChildReached
            && blockedParent.ChildPlan is null
            && blockedParent.UnreachedFreshGlobalCaptureProvided
            && blockedParent.UnreachedFreshFrameCaptureProvided
            && !blockedParent.ExactCleanExitAdmitted
            && blockedParent.StopAtRawFileOffsetExclusive == 0x012C91F3);
    }

    private static CapacityFinalCalleeLargeNodeSelectionLeafThresholdPlan Project(
        CapacityFinalCalleeLargeNodeFieldSelectionCapture parentSelection,
        uint? freshGlobalValue,
        uint? freshEbpMinusC) =>
        CapacityFinalCalleeLargeNodeSelectionLeafThresholdLifecycleProjection.Project(new(
            parentSelection,
            freshGlobalValue,
            freshEbpMinusC));
}
