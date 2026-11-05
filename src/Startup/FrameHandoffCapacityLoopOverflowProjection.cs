namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityLoopOverflowCapture(
    FrameHandoffCapacityLoopCapture LoopCapture,
    uint? CallTargetAddress,
    bool? CallReturnedNormally);

public enum FrameHandoffCapacityLoopOverflowOutcome
{
    LoopDidNotTakeOverflowBranch,
    MissingCallTargetCapture,
    MissingCallReturnCapture,
    CallDidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct FrameHandoffCapacityLoopOverflowPlan(
    FrameHandoffCapacityLoopOverflowCapture Capture,
    FrameHandoffCapacityLoopPlan PartialLoopPlan,
    FrameHandoffCapacityLoopOverflowOutcome Outcome,
    bool OverflowBranchReached,
    bool? OverflowBranchTaken,
    bool ValidatedOverflowEntry,
    FrameHandoffCapacityOverflowTailPlan OverflowTailPlan,
    bool UnreachedCallTargetCaptureProvided,
    bool UnreachedCallReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes one bounded capacity-loop iteration with the overflow tail only when
/// the loop actually takes its unsigned JB at raw 0xBE6B87.
/// </summary>
public static class FrameHandoffCapacityLoopOverflowProjection
{
    public const uint OverflowBranchRawFileOffset =
        FrameHandoffCapacityLoopProjection.FailureBranchRawFileOffset;

    public static FrameHandoffCapacityLoopOverflowPlan Project(
        FrameHandoffCapacityLoopOverflowCapture capture)
    {
        FrameHandoffCapacityLoopPlan loopPlan =
            FrameHandoffCapacityLoopProjection.Project(capture.LoopCapture);
        bool overflowBranchReached = loopPlan.CompareUpperSubtractWithEsiReached;
        bool? overflowBranchTaken = overflowBranchReached
            ? loopPlan.FailureBranchTaken
            : null;
        bool validatedOverflowEntry = IsValidatedOverflowEntry(loopPlan);

        FrameHandoffCapacityOverflowTailPlan tailPlan =
            FrameHandoffCapacityOverflowTailProjection.Project(new(
                ValidatedOverflowEntry: validatedOverflowEntry,
                CallTargetAddress: capture.CallTargetAddress,
                CallReturnedNormally: capture.CallReturnedNormally));

        FrameHandoffCapacityLoopOverflowOutcome outcome = validatedOverflowEntry
            ? tailPlan.Outcome switch
            {
                FrameHandoffCapacityOverflowTailOutcome.MissingCallTargetCapture =>
                    FrameHandoffCapacityLoopOverflowOutcome.MissingCallTargetCapture,
                FrameHandoffCapacityOverflowTailOutcome.MissingCallReturnCapture =>
                    FrameHandoffCapacityLoopOverflowOutcome.MissingCallReturnCapture,
                FrameHandoffCapacityOverflowTailOutcome.CallDidNotReturnNormally =>
                    FrameHandoffCapacityLoopOverflowOutcome.CallDidNotReturnNormally,
                FrameHandoffCapacityOverflowTailOutcome.ReturnedNormally =>
                    FrameHandoffCapacityLoopOverflowOutcome.ReturnedNormally,
                _ => FrameHandoffCapacityLoopOverflowOutcome.LoopDidNotTakeOverflowBranch
            }
            : FrameHandoffCapacityLoopOverflowOutcome.LoopDidNotTakeOverflowBranch;

        return new(
            Capture: capture,
            PartialLoopPlan: loopPlan,
            Outcome: outcome,
            OverflowBranchReached: overflowBranchReached,
            OverflowBranchTaken: overflowBranchTaken,
            ValidatedOverflowEntry: validatedOverflowEntry,
            OverflowTailPlan: tailPlan,
            UnreachedCallTargetCaptureProvided:
                tailPlan.UnreachedCallTargetCaptureProvided,
            UnreachedCallReturnCaptureProvided:
                tailPlan.UnreachedCallReturnCaptureProvided,
            StopAtRawFileOffsetExclusive: validatedOverflowEntry
                ? tailPlan.StopAtRawFileOffsetExclusive
                : loopPlan.SelectedDestinationRawFileOffset);
    }

    private static bool IsValidatedOverflowEntry(FrameHandoffCapacityLoopPlan loopPlan) =>
        loopPlan.Outcome == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
        && loopPlan.SubtractReached
        && loopPlan.CompareEaxWithOneReached
        && loopPlan.LoadUpperConstantReached
        && loopPlan.EaxAfterUpperConstantLoad
            == FrameHandoffCapacityLoopProjection.UpperSubtractConstant
        && loopPlan.UpperSubtractReached
        && loopPlan.EaxAfterUpperSubtract
            == unchecked(
                FrameHandoffCapacityLoopProjection.UpperSubtractConstant
                - loopPlan.Capture.EntryEsi)
        && loopPlan.CompareUpperSubtractWithEsiReached
        && loopPlan.UpperSubtractResultLessThanEsi == true
        && loopPlan.FailureBranchTaken == true
        && !loopPlan.DoubleEsiReached
        && !loopPlan.LoopBackReached
        && loopPlan.SelectedDestinationRawFileOffset
            == FrameHandoffCapacityLoopProjection.FailureRawFileOffset
        && loopPlan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacityLoopProjection.FailureRawFileOffset;
}
