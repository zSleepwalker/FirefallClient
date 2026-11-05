using System;
using Firefall.Reconstruction.Startup;

public static class CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var allMissing = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(null, null));
        Check(check, "missing entry stops parent without child",
            allMissing.Parent.Outcome == CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingEntryECX &&
            allMissing.Child is null &&
            allMissing.OrderedWrites.IsEmpty &&
            allMissing.Stop == CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop
                .ParentStoppedWithoutClean931BExit);

        var missingFreshGlobal = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(5, null));
        Check(check, "missing fresh global preserves parent shift flags without child",
            missingFreshGlobal.Parent.Outcome ==
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.MissingFreshGlobalValue &&
            missingFreshGlobal.Parent.ShiftReached &&
            !missingFreshGlobal.Parent.TestReached &&
            missingFreshGlobal.Child is null &&
            missingFreshGlobal.OrderedWrites.IsEmpty &&
            missingFreshGlobal.ParentFlagsAtStop?.Source ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ParentShift);

        var missingEntryWithUnusedGlobal =
            CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(new(null, 0x20));
        Check(check, "missing count does not consume captured global or enter child",
            missingEntryWithUnusedGlobal.Parent.UnreachedFreshGlobalCaptureProvided &&
            !missingEntryWithUnusedGlobal.Parent.FreshGlobalReadReached &&
            missingEntryWithUnusedGlobal.Child is null &&
            missingEntryWithUnusedGlobal.OrderedWrites.IsEmpty);

        var zeroSelection = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(5, 0x10));
        Check(check, "count five derives mask twenty and exact zero-test branch",
            zeroSelection.Parent.DestinationValueAfterShift == 0x20 &&
            zeroSelection.Parent.Outcome ==
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.ZeroTestResultToBounded931B &&
            zeroSelection.Parent.JnzReached &&
            zeroSelection.Parent.JnzTaken == false &&
            zeroSelection.Parent.JnzSelectedDestinationRawFileOffset ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.ZeroTestDestinationRawFileOffset &&
            zeroSelection.Parent.StopAtRawFileOffsetExclusive ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.ZeroTestDestinationRawFileOffset &&
            zeroSelection.Parent.ExactCleanBoundedDestination &&
            zeroSelection.ParentClean931BExitAdmitted);
        Check(check, "child receives parent fresh global and shifted destination",
            zeroSelection.ChildCapture?.EntryECX == zeroSelection.Parent.Capture.FreshGlobalValue &&
            zeroSelection.ChildCapture?.EntryECX == 0x10 &&
            zeroSelection.ChildCapture?.EntryEDI == zeroSelection.Parent.DestinationValueAfterShift &&
            zeroSelection.ChildCapture?.EntryEDI == 0x20 &&
            zeroSelection.Child?.OrECX == 0x10 &&
            zeroSelection.Child?.OrEDI == 0x20 &&
            zeroSelection.Child?.OrResult == 0x30);
        Check(check, "child performs no fresh global read",
            zeroSelection.Parent.FreshGlobalReadReached &&
            zeroSelection.Child is { GlobalReadReached: false } &&
            !zeroSelection.ChildPerformedFreshGlobalRead);
        Check(check, "zero path stores only global in ordered sequence",
            zeroSelection.ChildCleanExitAdmitted &&
            zeroSelection.OrderedWrites.Length == 1 &&
            zeroSelection.OrderedWrites[0].RawFileOffset ==
                CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalStoreRawFileOffset &&
            zeroSelection.OrderedWrites[0].Address ==
                CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress &&
            zeroSelection.OrderedWrites[0].Value == 0x30 &&
            zeroSelection.StopAtRawFileOffsetExclusive ==
                CapacityFinalCalleeCommonMaskOrStoreProjection.BoundaryRawFileOffsetExclusive &&
            !zeroSelection.Entered9323RingWrites);
        Check(check, "latest flags and store flags come from existing OR helper",
            zeroSelection.FlagsAtStop?.Source ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ChildOr &&
            zeroSelection.ChildFlagsAtStop == zeroSelection.Child?.OrFlags &&
            zeroSelection.FlagsAtStore == zeroSelection.Child?.OrFlags &&
            zeroSelection.FlagsAtStop?.Parity == true &&
            zeroSelection.FlagsAtStop?.Zero == false);

        var zeroFreshGlobal = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(5, 0));
        Check(check, "zero fresh global still ORs a nonzero shifted mask",
            zeroFreshGlobal.Parent.TestResult == 0 &&
            zeroFreshGlobal.ChildCapture?.EntryECX == 0 &&
            zeroFreshGlobal.ChildCapture?.EntryEDI == 0x20 &&
            zeroFreshGlobal.Child?.OrResult == 0x20 &&
            zeroFreshGlobal.OrderedWrites.Length == 1 &&
            zeroFreshGlobal.OrderedWrites[0].Value == 0x20 &&
            zeroFreshGlobal.FlagsAtStop?.Zero == false &&
            zeroFreshGlobal.FlagsAtStop?.Parity == false);

        var alreadySelected = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(2, 0xC));
        Check(check, "nonzero TEST is bounded at 9334 without child or writes",
            alreadySelected.Parent.Outcome ==
                CapacityFinalCalleeCommonMaskSelectionGateOutcome.NonzeroTestResultToBounded9334 &&
            alreadySelected.Parent.JnzReached &&
            alreadySelected.Parent.JnzTaken == true &&
            alreadySelected.Parent.JnzSelectedDestinationRawFileOffset ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.NonzeroTestDestinationRawFileOffset &&
            alreadySelected.Parent.StopAtRawFileOffsetExclusive ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.NonzeroTestDestinationRawFileOffset &&
            alreadySelected.Parent.ExactCleanBoundedDestination &&
            alreadySelected.Child is null &&
            alreadySelected.OrderedWrites.IsEmpty &&
            alreadySelected.Stop ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionStop.NonzeroTestBoundedAt9334);
        Check(check, "bounded nonzero flags come from parent TEST",
            alreadySelected.FlagsAtStop?.Source ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ParentTest &&
            alreadySelected.ParentFlagsAtStop?.Zero == false &&
            alreadySelected.ChildFlagsAtStop is null);

        var highBit = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(31, 0x40000000));
        Check(check, "shift thirty-one supplies high-bit mask to child",
            highBit.Parent.DestinationValueAfterShift == 0x80000000 &&
            highBit.ChildCapture?.EntryEDI == 0x80000000 &&
            highBit.ChildCapture?.EntryECX == 0x40000000 &&
            highBit.Child?.OrResult == 0xC0000000 &&
            highBit.OrderedWrites.Length == 1 &&
            highBit.OrderedWrites[0].Value == 0xC0000000);
        Check(check, "high-bit result flags are preserved through store",
            highBit.FlagsAtStore?.Sign == true &&
            highBit.FlagsAtStore?.Zero == false &&
            highBit.FlagsAtStore?.Parity == true &&
            highBit.FlagsAtStore?.Carry == false &&
            highBit.FlagsAtStore?.Overflow == false);

        var shift32 = CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
            new(32, 0xFFFFFFFE));
        Check(check, "shift count thirty-two uses masked zero-count result",
            shift32.Parent.ShiftCount == 0 &&
            shift32.Parent.DestinationValueAfterShift == 1 &&
            shift32.ChildCapture?.EntryECX == 0xFFFFFFFE &&
            shift32.ChildCapture?.EntryEDI == 1 &&
            shift32.Child?.OrResult == 0xFFFFFFFF &&
            shift32.OrderedWrites.Length == 1 &&
            shift32.OrderedWrites[0].Value == 0xFFFFFFFF);
        Check(check, "FFFFFFFF child OR flags are final flags-at-store",
            shift32.FlagsAtStop?.Source ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ChildOr &&
            shift32.FlagsAtStore?.Sign == true &&
            shift32.FlagsAtStore?.Zero == false &&
            shift32.FlagsAtStore?.Parity == true &&
            shift32.FlagsAtStore == shift32.Child?.OrFlags);

        var zeroCountMissingGlobal =
            CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjection.Project(
                new(32, null));
        var preservedUnknownShiftFlags = zeroCountMissingGlobal.Parent.ShiftFlags;
        Check(check, "zero effective shift preserves unknown parent flags",
            preservedUnknownShiftFlags.HasValue &&
            preservedUnknownShiftFlags.Value.Carry is null &&
            preservedUnknownShiftFlags.Value.Parity is null &&
            preservedUnknownShiftFlags.Value.Auxiliary is null &&
            preservedUnknownShiftFlags.Value.Zero is null &&
            preservedUnknownShiftFlags.Value.Sign is null &&
            preservedUnknownShiftFlags.Value.Overflow is null &&
            zeroCountMissingGlobal.FlagsAtStop?.Source ==
                CapacityFinalCalleeCommonMaskSelectionOrLifecycleProjectionFlagSource.ParentShift &&
            zeroCountMissingGlobal.Child is null);
    }

    private static void Check(Action<string, bool> check, string name, bool passed)
    {
        check(name, passed);
    }
}
