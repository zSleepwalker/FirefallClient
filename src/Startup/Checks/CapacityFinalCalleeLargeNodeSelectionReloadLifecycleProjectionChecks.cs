using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeNodeSelectionReloadLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeNodeSelectionReloadPlan allMissing = Project(
            new(null, null, null, null, null),
            freshNodeDword: null);
        check("All missing parent captures block the child and stop at the frame read",
            allMissing.Outcome
                == CapacityFinalCalleeLargeNodeSelectionReloadOutcome.ParentSelectionBlocked
            && allMissing.ParentPlan.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.MissingEntryEBPForFrameRead
            && !allMissing.ChildReached
            && allMissing.ChildPlan is null
            && allMissing.Reads is
            [
                {
                    RawFileOffset: 0x012C91F0,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan missingParentWithChildValue = Project(
            new(null, null, null, null, null),
            freshNodeDword: 0xAABBCCDD);
        check("A supplied child value cannot bypass a missing parent and remains unused",
            !missingParentWithChildValue.ChildReached
            && missingParentWithChildValue.UnreachedFreshNodeCaptureProvided
            && missingParentWithChildValue.ChildPlan is null
            && !missingParentWithChildValue.ExactCleanExitAdmitted
            && missingParentWithChildValue.StopAtRawFileOffsetExclusive == 0x012C91F0);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan firstFieldNonzero = Project(
            new(0x118, 0xDEADBEEF, 0x100, 0x80000001, null),
            freshNodeDword: 0x76543210);
        check("The clean first-field nonzero handoff derives child ECX from reached plus-14 LEA",
            firstFieldNonzero.Outcome
                == CapacityFinalCalleeLargeNodeSelectionReloadOutcome.ChildReloadCompleted
            && firstFieldNonzero.ParentPlan.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.FirstFieldNonzeroToTarget
            && firstFieldNonzero.ParentPlan.Plus14BranchReached
            && firstFieldNonzero.ParentPlan.Plus14BranchTaken == true
            && firstFieldNonzero.ParentPlan.Plus14BranchSelectedDestinationRawFileOffset
                == 0x012C9205
            && firstFieldNonzero.ParentPlan.Plus14LeaReached
            && !firstFieldNonzero.ParentPlan.Plus10CompareReached
            && firstFieldNonzero.ChildReached
            && firstFieldNonzero.ChildEntryECX == 0x114
            && firstFieldNonzero.ChildECXSourceRawFileOffset == 0x012C91F7
            && firstFieldNonzero.ChildEntryECX != firstFieldNonzero.ParentPlan.FreshEAXPlus14
            && firstFieldNonzero.ChildFreshNodeAddress == 0x114
            && firstFieldNonzero.ChildFreshNodeDword == 0x76543210
            && firstFieldNonzero.ChildEAX == 0x76543210
            && firstFieldNonzero.ChildEDX == 0x114
            && firstFieldNonzero.ParentPlan.Plus14CompareFlags
                == firstFieldNonzero.ParentCompareFlagsAtStop
            && firstFieldNonzero.ParentCompareFlagsPreservedThroughChild
            && firstFieldNonzero.ChildJumpReached
            && firstFieldNonzero.ChildJumpTargetRawFileOffset == 0x012C91F3
            && firstFieldNonzero.StopAtRawFileOffsetExclusive == 0x012C91F3
            && firstFieldNonzero.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C91F0, 0x012C91F3, 0x012C9205 })
            && firstFieldNonzero.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan secondFieldNonzero = Project(
            new(0x200, 0x12345678, 0xFFFFFFF4, 0, 0x80000001),
            freshNodeDword: 0xCAFEBABE);
        check("The clean second-field nonzero handoff derives child ECX from reached plus-10 LEA",
            secondFieldNonzero.Outcome
                == CapacityFinalCalleeLargeNodeSelectionReloadOutcome.ChildReloadCompleted
            && secondFieldNonzero.ParentPlan.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldNonzeroToFallthrough
            && secondFieldNonzero.ParentPlan.Plus14BranchReached
            && secondFieldNonzero.ParentPlan.Plus14BranchTaken == false
            && secondFieldNonzero.ParentPlan.Plus10BranchReached
            && secondFieldNonzero.ParentPlan.Plus10BranchTaken == false
            && secondFieldNonzero.ParentPlan.Plus10BranchSelectedDestinationRawFileOffset
                == 0x012C9205
            && secondFieldNonzero.ParentPlan.Plus10LeaReached
            && secondFieldNonzero.ChildReached
            && secondFieldNonzero.ChildEntryECX == 4
            && secondFieldNonzero.ChildECXSourceRawFileOffset == 0x012C9200
            && secondFieldNonzero.ChildFreshNodeAddress == 4
            && secondFieldNonzero.ChildFreshNodeDword == 0xCAFEBABE
            && secondFieldNonzero.ChildEAX == 0xCAFEBABE
            && secondFieldNonzero.ChildEDX == 4
            && secondFieldNonzero.ParentPlan.Plus10CompareFlags
                == secondFieldNonzero.ParentCompareFlagsAtStop
            && secondFieldNonzero.ParentCompareFlagsPreservedThroughChild
            && secondFieldNonzero.ChildJumpTargetRawFileOffset == 0x012C91F3
            && secondFieldNonzero.StopAtRawFileOffsetExclusive == 0x012C91F3
            && secondFieldNonzero.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C91F0, 0x012C91F3, 0x012C91FC, 0x012C9205 })
            && secondFieldNonzero.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan secondFieldZero = Project(
            new(2, 0xCAFEBABE, 0xFFFFFFF4, 0, 0),
            freshNodeDword: null);
        check("The clean second-field zero route is bounded at 920B without entering the child",
            secondFieldZero.Outcome
                == CapacityFinalCalleeLargeNodeSelectionReloadOutcome.SecondFieldZeroToBoundedTarget
            && secondFieldZero.ParentPlan.Outcome
                == CapacityFinalCalleeLargeNodeFieldSelectionOutcome.SecondFieldZeroToTarget
            && secondFieldZero.ParentPlan.Plus14BranchTaken == false
            && secondFieldZero.ParentPlan.Plus10BranchTaken == true
            && secondFieldZero.ParentPlan.Plus10BranchSelectedDestinationRawFileOffset
                == 0x012C920B
            && !secondFieldZero.ChildReached
            && secondFieldZero.ChildPlan is null
            && secondFieldZero.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C91F0, 0x012C91F3, 0x012C91FC })
            && secondFieldZero.ParentCompareFlagsAtStop
                == secondFieldZero.ParentPlan.Plus10CompareFlags
            && !secondFieldZero.ParentCompareFlagsPreservedThroughChild
            && secondFieldZero.ExactCleanExitAdmitted
            && secondFieldZero.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan secondFieldZeroWithUnusedChildValue = Project(
            new(2, 0xCAFEBABE, 0xFFFFFFF4, 0, 0),
            freshNodeDword: 0x12345678);
        check("A supplied fresh-node value at 920B is unused and contaminates clean admission",
            !secondFieldZeroWithUnusedChildValue.ChildReached
            && secondFieldZeroWithUnusedChildValue.UnreachedFreshNodeCaptureProvided
            && secondFieldZeroWithUnusedChildValue.ParentPlan.ExactCleanExitAdmitted
            && !secondFieldZeroWithUnusedChildValue.ExactCleanExitAdmitted
            && secondFieldZeroWithUnusedChildValue.Reads.Length == 3
            && secondFieldZeroWithUnusedChildValue.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan aliasedFreshRead = Project(
            new(0x118, 0xDEADBEEF, 0x100, 0x80000001, null),
            freshNodeDword: 0);
        check("The child performs an independent fresh read even when its address aliases plus-14",
            aliasedFreshRead.ChildEntryECX == aliasedFreshRead.ParentPlan.FreshEAXPlus14Address
            && aliasedFreshRead.Reads.Length == 3
            && aliasedFreshRead.Reads[1] is
            {
                RawFileOffset: 0x012C91F3,
                Address: 0x114,
                ValueCaptured: true,
                Value: 0x80000001
            }
            && aliasedFreshRead.Reads[2] is
            {
                RawFileOffset: 0x012C9205,
                Address: 0x114,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0
            }
            && aliasedFreshRead.ChildFreshNodeCaptured
            && aliasedFreshRead.ChildEAX == 0
            && aliasedFreshRead.ChildEDX == 0x114);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan missingChildFresh = Project(
            new(0x118, 0xDEADBEEF, 0x100, 0x80000001, null),
            freshNodeDword: null);
        check("A missing child fresh value is not inferred from the aliased parent field read",
            missingChildFresh.Outcome
                == CapacityFinalCalleeLargeNodeSelectionReloadOutcome.MissingChildFreshNodeDword
            && missingChildFresh.ChildReached
            && missingChildFresh.ChildPlan?.Outcome
                == CapacityFinalCalleeLargeNodeReloadOutcome.MissingFreshNodeDword
            && missingChildFresh.ChildEntryECX == 0x114
            && missingChildFresh.ChildFreshNodeAddress == 0x114
            && !missingChildFresh.ChildFreshNodeCaptured
            && missingChildFresh.ChildEAX is null
            && missingChildFresh.ChildEDX is null
            && !missingChildFresh.ChildJumpReached
            && missingChildFresh.Reads.Length == 3
            && missingChildFresh.Reads[1].Value == 0x80000001
            && missingChildFresh.Reads[2] is
            {
                RawFileOffset: 0x012C9205,
                Address: 0x114,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingChildFresh.ParentCompareFlagsPreservedThroughChild
            && missingChildFresh.ParentCompareFlagsAtStop
                == missingChildFresh.ParentPlan.Plus14CompareFlags
            && !missingChildFresh.ExactCleanExitAdmitted
            && missingChildFresh.StopAtRawFileOffsetExclusive == 0x012C9205);

        CapacityFinalCalleeLargeNodeSelectionReloadPlan unusedPlus10 = Project(
            new(0x100, 0x2222, 1, 1, 0),
            freshNodeDword: 0xFFFFFFFF);
        check("A supplied unused plus-10 value blocks the first-nonzero child handoff",
            unusedPlus10.ParentPlan.UnreachedFreshEAXPlus10CaptureProvided
            && !unusedPlus10.ParentPlan.ExactCleanExitAdmitted
            && !unusedPlus10.ChildReached
            && unusedPlus10.ChildPlan is null
            && unusedPlus10.UnreachedFreshNodeCaptureProvided
            && !unusedPlus10.ExactCleanExitAdmitted
            && unusedPlus10.StopAtRawFileOffsetExclusive == 0x012C9205);
    }

    private static CapacityFinalCalleeLargeNodeSelectionReloadPlan Project(
        CapacityFinalCalleeLargeNodeFieldSelectionCapture fieldSelection,
        uint? freshNodeDword) =>
        CapacityFinalCalleeLargeNodeSelectionReloadLifecycleProjection.Project(new(
            fieldSelection,
            freshNodeDword));
}
