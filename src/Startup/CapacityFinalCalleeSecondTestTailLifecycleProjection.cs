namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSecondTestTailLifecycleCapture(
    CapacityFinalCalleeSecondFieldTestCapture SecondFieldTestCapture,
    uint? FreshGlobalForCompare,
    uint? CallerLiveInEAX);

public enum CapacityFinalCalleeSecondTestTailLifecycleOutcome
{
    SecondFieldTestBlocked,
    ZeroBypassToTarget,
    ZeroBypassContaminated,
    MissingFreshGlobalForCompare,
    EntryECXUnsignedBelowGlobalToValidation,
    MissingCallerLiveInEAXForFirstStore,
    PairedStoresAndJumpToTarget
}

public sealed record CapacityFinalCalleeSecondTestTailLifecyclePlan(
    CapacityFinalCalleeSecondTestTailLifecycleCapture Capture,
    CapacityFinalCalleeSecondFieldTestPlan SecondFieldTestPlan,
    CapacityFinalCalleeFinalPairedTailPlan? FinalPairedTailPlan,
    CapacityFinalCalleeSecondTestTailLifecycleOutcome Outcome)
{
    public bool FinalPairedTailReached { get; init; }
    public uint? DerivedFinalEntryECX { get; init; }
    public bool UnreachedFreshGlobalForCompareCaptureProvided { get; init; }
    public bool UnreachedCallerLiveInEAXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the clean nonzero 0x908B result to the final paired tail, carrying
/// only the freshly read ECX and independently captured caller-live EAX.
/// </summary>
public static class CapacityFinalCalleeSecondTestTailLifecycleProjection
{
    public const uint SecondFieldTestEntryRawFileOffset =
        CapacityFinalCalleeSecondFieldTestProjection.EntryRawFileOffset;
    public const uint FinalTailEntryRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset;
    public const uint ZeroBypassDestinationRawFileOffset =
        CapacityFinalCalleeSecondFieldTestProjection.ZeroDestinationRawFileOffset;
    public const uint UnsignedBelowDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.ValidationDestinationRawFileOffset;
    public const uint FinalPairJumpDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.FinalJumpDestinationRawFileOffset;

    public static CapacityFinalCalleeSecondTestTailLifecyclePlan Project(
        CapacityFinalCalleeSecondTestTailLifecycleCapture capture)
    {
        CapacityFinalCalleeSecondFieldTestPlan secondTest =
            CapacityFinalCalleeSecondFieldTestProjection.Project(
                capture.SecondFieldTestCapture);

        bool cleanZeroExit =
            secondTest.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.FreshECXZeroToTarget
            && IsCleanSecondTestExit(secondTest, zero: true);
        bool cleanNonzeroJoin =
            secondTest.Outcome
                == CapacityFinalCalleeSecondFieldTestOutcome.FreshECXNonzeroToBoundary
            && IsCleanSecondTestExit(secondTest, zero: false);

        if (cleanZeroExit)
        {
            bool unreachedGlobal = capture.FreshGlobalForCompare.HasValue;
            bool unreachedEax = capture.CallerLiveInEAX.HasValue;
            bool contaminated = unreachedGlobal || unreachedEax;
            return new(
                capture,
                secondTest,
                FinalPairedTailPlan: null,
                contaminated
                    ? CapacityFinalCalleeSecondTestTailLifecycleOutcome
                        .ZeroBypassContaminated
                    : CapacityFinalCalleeSecondTestTailLifecycleOutcome.ZeroBypassToTarget)
            {
                UnreachedFreshGlobalForCompareCaptureProvided = unreachedGlobal,
                UnreachedCallerLiveInEAXCaptureProvided = unreachedEax,
                ExactCleanExitAdmitted = !contaminated,
                StopAtRawFileOffsetExclusive = ZeroBypassDestinationRawFileOffset
            };
        }

        if (!cleanNonzeroJoin || secondTest.FreshECX is not uint freshEcx)
        {
            return new(
                capture,
                secondTest,
                FinalPairedTailPlan: null,
                CapacityFinalCalleeSecondTestTailLifecycleOutcome.SecondFieldTestBlocked)
            {
                UnreachedFreshGlobalForCompareCaptureProvided =
                    capture.FreshGlobalForCompare.HasValue,
                UnreachedCallerLiveInEAXCaptureProvided =
                    capture.CallerLiveInEAX.HasValue,
                StopAtRawFileOffsetExclusive =
                    secondTest.StopAtRawFileOffsetExclusive
            };
        }

        CapacityFinalCalleeFinalPairedTailPlan finalTail =
            CapacityFinalCalleeFinalPairedTailProjection.Project(
                new CapacityFinalCalleeFinalPairedTailCapture(
                    EntryECX: freshEcx,
                    FreshGlobalForCompare: capture.FreshGlobalForCompare,
                    EntryEAX: capture.CallerLiveInEAX));

        return new(
            capture,
            secondTest,
            finalTail,
            MapOutcome(finalTail.Outcome))
        {
            FinalPairedTailReached = true,
            DerivedFinalEntryECX = freshEcx,
            UnreachedFreshGlobalForCompareCaptureProvided =
                finalTail.UnreachedFreshGlobalForCompareCaptureProvided,
            UnreachedCallerLiveInEAXCaptureProvided =
                finalTail.UnreachedEntryEAXCaptureProvided,
            ExactCleanExitAdmitted =
                IsCleanFinalTailExit(finalTail),
            StopAtRawFileOffsetExclusive =
                finalTail.StopAtRawFileOffsetExclusive
        };
    }

    private static bool IsCleanSecondTestExit(
        CapacityFinalCalleeSecondFieldTestPlan plan,
        bool zero) =>
        plan.ExactCleanExitAdmitted
        && plan.TestECXReached
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == zero
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == (zero
                ? ZeroBypassDestinationRawFileOffset
                : FinalTailEntryRawFileOffset)
        && plan.StopAtRawFileOffsetExclusive
            == plan.ZeroBranchSelectedDestinationRawFileOffset
        && !plan.UnreachedFreshECXCaptureProvided;

    private static bool IsCleanFinalTailExit(
        CapacityFinalCalleeFinalPairedTailPlan plan)
    {
        if (!plan.ExactCleanExitAdmitted)
        {
            return false;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeFinalPairedTailOutcome
                    .EntryECXUnsignedBelowGlobalToValidation =>
                plan.GuardBranchReached
                && plan.GuardBranchTaken == true
                && plan.GuardBranchSelectedDestinationRawFileOffset
                    == UnsignedBelowDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == UnsignedBelowDestinationRawFileOffset
                && !plan.UnreachedEntryEAXCaptureProvided,
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget =>
                plan.FinalJumpReached
                && plan.FinalJumpSelectedDestinationRawFileOffset
                    == FinalPairJumpDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == FinalPairJumpDestinationRawFileOffset
                && plan.Writes.Length == 2,
            _ => false
        };
    }

    private static CapacityFinalCalleeSecondTestTailLifecycleOutcome MapOutcome(
        CapacityFinalCalleeFinalPairedTailOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare =>
                CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .MissingFreshGlobalForCompare,
            CapacityFinalCalleeFinalPairedTailOutcome
                .EntryECXUnsignedBelowGlobalToValidation =>
                CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .EntryECXUnsignedBelowGlobalToValidation,
            CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore =>
                CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .MissingCallerLiveInEAXForFirstStore,
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget =>
                CapacityFinalCalleeSecondTestTailLifecycleOutcome
                    .PairedStoresAndJumpToTarget,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
