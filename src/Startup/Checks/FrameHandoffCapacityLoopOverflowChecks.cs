using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityLoopOverflowChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the loop's exact overflow JB and the bounded tail entry",
            FrameHandoffCapacityLoopOverflowProjection.OverflowBranchRawFileOffset
                == 0x00BE6B87
            && FrameHandoffCapacityLoopProjection.FailureRawFileOffset
                == FrameHandoffCapacityOverflowTailProjection.OverflowEntryRawFileOffset
            && FrameHandoffCapacityOverflowTailProjection.OverflowBranchTargetRawFileOffset
                == FrameHandoffCapacityLoopOverflowProjection
                    .OverflowBranchRawFileOffset
                    + 6u
                    + 0x133u);

        FrameHandoffCapacityLoopOverflowPlan overflow = Project(
            0x08000000,
            0x08000000,
            0x12345678,
            true);
        check("A taken unsigned overflow JB alone validates tail entry and composes a normal return",
            overflow.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && overflow.PartialLoopPlan.EntrySubtractResultIsZero == true
            && !overflow.PartialLoopPlan.CompareEsiWithEightReached
            && overflow.PartialLoopPlan.EaxAfterUpperSubtract == 0x07FFFFFF
            && overflow.PartialLoopPlan.UpperSubtractResultLessThanEsi == true
            && overflow.OverflowBranchReached
            && overflow.OverflowBranchTaken == true
            && overflow.ValidatedOverflowEntry
            && overflow.Outcome == FrameHandoffCapacityLoopOverflowOutcome.ReturnedNormally
            && overflow.OverflowTailPlan.Capture.ValidatedOverflowEntry
            && overflow.OverflowTailPlan.PushReached
            && overflow.OverflowTailPlan.CallReached
            && overflow.OverflowTailPlan.NormalBoundaryReached
            && !overflow.UnreachedCallTargetCaptureProvided
            && !overflow.UnreachedCallReturnCaptureProvided
            && overflow.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacityLoopOverflowPlan doubling = Project(7, 6, 0, true);
        FrameHandoffCapacityLoopOverflowPlan earlyExit =
            Project(0x08000000, 0x07FFFFFF, 0, true);
        check("Doubling and the earlier unsigned ESI exit do not reach or validate the overflow tail",
            doubling.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && doubling.OverflowBranchReached
            && doubling.OverflowBranchTaken == false
            && !doubling.ValidatedOverflowEntry
            && doubling.OverflowTailPlan.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.EntryNotValidated
            && doubling.OverflowTailPlan.Pushes.IsEmpty
            && doubling.UnreachedCallTargetCaptureProvided
            && doubling.UnreachedCallReturnCaptureProvided
            && doubling.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.LoopRawFileOffset
            && earlyExit.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.ExitAtRawFileOffset91
            && earlyExit.PartialLoopPlan.CompareEsiWithEightReached
            && earlyExit.PartialLoopPlan.EsiAtLeastEight == true
            && earlyExit.PartialLoopPlan.ExitBranchTaken == true
            && !earlyExit.OverflowBranchReached
            && earlyExit.OverflowBranchTaken is null
            && !earlyExit.ValidatedOverflowEntry
            && earlyExit.OverflowTailPlan.Pushes.IsEmpty
            && earlyExit.UnreachedCallTargetCaptureProvided
            && earlyExit.UnreachedCallReturnCaptureProvided
            && earlyExit.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.ExitRawFileOffset);

        FrameHandoffCapacityLoopOverflowPlan strictThresholdBoundary =
            Project(0x07FFFFFF, 0x07FFFFFF, null, null);
        FrameHandoffCapacityLoopOverflowPlan strictThresholdOverflow =
            Project(0x08000000, 0x08000000, null, null);
        check("Unsigned overflow is strict at the loop's current upper-subtract boundary",
            strictThresholdBoundary.PartialLoopPlan.EaxAfterUpperSubtract
                == 0x08000000
            && strictThresholdBoundary.PartialLoopPlan.UpperSubtractResultLessThanEsi
                == false
            && strictThresholdBoundary.PartialLoopPlan.FailureBranchTaken == false
            && !strictThresholdBoundary.ValidatedOverflowEntry
            && strictThresholdBoundary.OverflowTailPlan.Pushes.IsEmpty
            && !strictThresholdBoundary.UnreachedCallTargetCaptureProvided
            && !strictThresholdBoundary.UnreachedCallReturnCaptureProvided
            && strictThresholdOverflow.PartialLoopPlan.EaxAfterUpperSubtract
                == 0x07FFFFFF
            && strictThresholdOverflow.PartialLoopPlan.UpperSubtractResultLessThanEsi
                == true
            && strictThresholdOverflow.OverflowBranchTaken == true
            && strictThresholdOverflow.ValidatedOverflowEntry
            && strictThresholdOverflow.Outcome
                == FrameHandoffCapacityLoopOverflowOutcome.MissingCallTargetCapture
            && strictThresholdOverflow.OverflowTailPlan.PushReached
            && strictThresholdOverflow.OverflowTailPlan.Pushes.Length == 1
            && !strictThresholdOverflow.OverflowTailPlan.CallReached
            && !strictThresholdOverflow.OverflowTailPlan.UnreachedCallReturnCaptureProvided
            && strictThresholdOverflow.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.CallRawFileOffset);

        FrameHandoffCapacityLoopOverflowPlan wrappedEntry =
            Project(1, 2, null, null);
        FrameHandoffCapacityLoopOverflowPlan wrappedOverflow =
            Project(uint.MaxValue, uint.MaxValue, 0, true);
        check("Entry and upper subtraction wrap as DWORDs without iterating the loop again",
            wrappedEntry.PartialLoopPlan.EaxAfterEntrySubtract == uint.MaxValue
            && wrappedEntry.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && wrappedEntry.PartialLoopPlan.EsiAfterDouble == 2
            && !wrappedEntry.ValidatedOverflowEntry
            && wrappedEntry.OverflowTailPlan.Pushes.IsEmpty
            && wrappedEntry.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.LoopRawFileOffset
            && wrappedOverflow.PartialLoopPlan.EaxAfterEntrySubtract == 0
            && wrappedOverflow.PartialLoopPlan.EaxAfterUpperSubtract == 0x10000000
            && wrappedOverflow.PartialLoopPlan.UpperSubtractResultLessThanEsi == true
            && wrappedOverflow.OverflowBranchReached
            && wrappedOverflow.OverflowBranchTaken == true
            && wrappedOverflow.ValidatedOverflowEntry
            && wrappedOverflow.Outcome
                == FrameHandoffCapacityLoopOverflowOutcome.ReturnedNormally
            && wrappedOverflow.OverflowTailPlan.Capture.CallTargetAddress == 0
            && wrappedOverflow.OverflowTailPlan.CallReached
            && wrappedOverflow.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacityLoopOverflowPlan nonreturningOverflow =
            Project(0x08000000, 0x08000000, 0x55, false);
        FrameHandoffCapacityLoopOverflowPlan nonzeroDifferenceHighEsi =
            Project(uint.MaxValue, uint.MaxValue - 1, 0x66, true);
        check("Only the current loop's reached upper-subtract condition can enter the tail",
            nonreturningOverflow.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && nonreturningOverflow.OverflowBranchReached
            && nonreturningOverflow.OverflowBranchTaken == true
            && nonreturningOverflow.ValidatedOverflowEntry
            && nonreturningOverflow.Outcome
                == FrameHandoffCapacityLoopOverflowOutcome.CallDidNotReturnNormally
            && nonreturningOverflow.OverflowTailPlan.PushReached
            && nonreturningOverflow.OverflowTailPlan.CallReached
            && !nonreturningOverflow.OverflowTailPlan.NormalBoundaryReached
            && nonreturningOverflow.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.CallRawFileOffset
            && nonzeroDifferenceHighEsi.PartialLoopPlan.Outcome
                == FrameHandoffCapacityLoopOutcome.ExitAtRawFileOffset91
            && nonzeroDifferenceHighEsi.PartialLoopPlan.ExitBranchTaken == true
            && !nonzeroDifferenceHighEsi.OverflowBranchReached
            && !nonzeroDifferenceHighEsi.ValidatedOverflowEntry
            && nonzeroDifferenceHighEsi.OverflowTailPlan.Pushes.IsEmpty
            && nonzeroDifferenceHighEsi.UnreachedCallTargetCaptureProvided
            && nonzeroDifferenceHighEsi.UnreachedCallReturnCaptureProvided);

        FrameHandoffCapacityLoopOverflowPlan missingReturn =
            Project(0x08000000, 0x08000000, 0, null);
        check("A captured zero target is valid, while missing or nonreturning calls preserve the tail's partial push",
            missingReturn.ValidatedOverflowEntry
            && missingReturn.Outcome
                == FrameHandoffCapacityLoopOverflowOutcome.MissingCallReturnCapture
            && missingReturn.OverflowTailPlan.Capture.CallTargetAddress == 0
            && missingReturn.OverflowTailPlan.Pushes.Length == 1
            && missingReturn.OverflowTailPlan.PushReached
            && missingReturn.OverflowTailPlan.CallReached
            && missingReturn.OverflowTailPlan.CallReturnedNormally is null
            && missingReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.CallRawFileOffset
            && nonreturningOverflow.OverflowTailPlan.Pushes.Length == 1
            && nonreturningOverflow.OverflowTailPlan.CallReached
            && nonreturningOverflow.OverflowTailPlan.CallReturnedNormally == false);
    }

    private static FrameHandoffCapacityLoopOverflowPlan Project(
        uint esi,
        uint edx,
        uint? callTargetAddress,
        bool? callReturnedNormally) =>
        FrameHandoffCapacityLoopOverflowProjection.Project(new(
            new(esi, edx),
            callTargetAddress,
            callReturnedNormally));
}
