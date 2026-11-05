using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePairedFinalLifecycleCapture(
    CapacityFinalCalleeSuccessorPairedStoresCapture SuccessorPairedStoresCapture,
    uint? FreshGlobalForFinalCompare);

public enum CapacityFinalCalleePairedFinalLifecycleOutcome
{
    SuccessorPairedStoresBlocked,
    SuccessorZeroExitToTarget,
    SuccessorZeroExitContaminated,
    SuccessorUnsignedBelowToValidation,
    SuccessorUnsignedBelowContaminated,
    MissingFreshGlobalForFinalCompare,
    FinalEntryECXUnsignedBelowGlobalToValidation,
    MissingSuccessorEntryEAXForFinalFirstStore,
    FinalPairedStoresAndJumpToTarget
}

public readonly record struct CapacityFinalCalleePairedFinalLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleePairedFinalLifecyclePlan(
    CapacityFinalCalleePairedFinalLifecycleCapture Capture,
    CapacityFinalCalleeSuccessorPairedStoresPlan SuccessorPairedStoresPlan,
    CapacityFinalCalleeFinalPairedTailPlan? FinalPairedTailPlan,
    CapacityFinalCalleePairedFinalLifecycleOutcome Outcome)
{
    public bool FinalPairedTailReached { get; init; }
    public uint? DerivedFinalEntryECX { get; init; }
    public uint? DerivedFinalEntryEAX { get; init; }
    public bool UnreachedFreshGlobalForFinalCompareCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleePairedFinalLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleePairedFinalLifecycleWrite>.Empty;
}

/// <summary>
/// Joins the clean nonzero 0x908B successor result to the final paired tail.
/// The final ECX comes from the successor's fresh ESI+14 read; EAX remains
/// the successor-entry value and is supplied to the tail only after its guard.
/// </summary>
public static class CapacityFinalCalleePairedFinalLifecycleProjection
{
    public const uint SuccessorEntryRawFileOffset =
        CapacityFinalCalleeSuccessorPairedStoresProjection.EntryRawFileOffset;
    public const uint FinalTailEntryRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset;
    public const uint SuccessorZeroDestinationRawFileOffset =
        CapacityFinalCalleeSuccessorPairedStoresProjection.ZeroDestinationRawFileOffset;
    public const uint SuccessorUnsignedBelowDestinationRawFileOffset =
        CapacityFinalCalleeSuccessorPairedStoresProjection.ValidationDestinationRawFileOffset;
    public const uint FinalUnsignedBelowDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.ValidationDestinationRawFileOffset;
    public const uint FinalPairJumpDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.FinalJumpDestinationRawFileOffset;

    public static CapacityFinalCalleePairedFinalLifecyclePlan Project(
        CapacityFinalCalleePairedFinalLifecycleCapture capture)
    {
        CapacityFinalCalleeSuccessorPairedStoresPlan successor =
            CapacityFinalCalleeSuccessorPairedStoresProjection.Project(
                capture.SuccessorPairedStoresCapture);

        ImmutableArray<CapacityFinalCalleePairedFinalLifecycleWrite> CombinedWrites(
            CapacityFinalCalleeFinalPairedTailPlan? finalTail)
        {
            var writes =
                ImmutableArray.CreateBuilder<CapacityFinalCalleePairedFinalLifecycleWrite>();
            foreach (CapacityFinalCalleeSuccessorPairedStoresWrite write in successor.Writes)
            {
                writes.Add(new(write.RawFileOffset, write.Address, write.Value));
            }

            if (finalTail is not null)
            {
                foreach (CapacityFinalCalleeFinalPairedTailWrite write in finalTail.Writes)
                {
                    writes.Add(new(write.RawFileOffset, write.Address, write.Value));
                }
            }

            return writes.ToImmutable();
        }

        CapacityFinalCalleePairedFinalLifecyclePlan FinishWithoutFinalTail(
            CapacityFinalCalleePairedFinalLifecycleOutcome outcome,
            bool unreachedFinalGlobal,
            bool exactCleanExit) =>
            new(
                capture,
                successor,
                null,
                outcome)
            {
                UnreachedFreshGlobalForFinalCompareCaptureProvided = unreachedFinalGlobal,
                ExactCleanExitAdmitted = exactCleanExit,
                StopAtRawFileOffsetExclusive = successor.StopAtRawFileOffsetExclusive,
                Writes = CombinedWrites(finalTail: null)
            };

        if (IsCleanSuccessorZeroExit(successor))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutFinalTail(
                contaminated
                    ? CapacityFinalCalleePairedFinalLifecycleOutcome
                        .SuccessorZeroExitContaminated
                    : CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorZeroExitToTarget,
                unreachedFinalGlobal: contaminated,
                exactCleanExit: !contaminated);
        }

        if (IsCleanSuccessorBelowExit(successor))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutFinalTail(
                contaminated
                    ? CapacityFinalCalleePairedFinalLifecycleOutcome
                        .SuccessorUnsignedBelowContaminated
                    : CapacityFinalCalleePairedFinalLifecycleOutcome
                        .SuccessorUnsignedBelowToValidation,
                unreachedFinalGlobal: contaminated,
                exactCleanExit: !contaminated);
        }

        if (!IsCleanSuccessorNonzeroExit(successor)
            || successor.FreshEsiPlus14 is not uint freshEcx)
        {
            return FinishWithoutFinalTail(
                CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorPairedStoresBlocked,
                unreachedFinalGlobal: capture.FreshGlobalForFinalCompare.HasValue,
                exactCleanExit: false);
        }

        CapacityFinalCalleeFinalPairedTailPlan finalTailWithoutEax =
            CapacityFinalCalleeFinalPairedTailProjection.Project(
                new CapacityFinalCalleeFinalPairedTailCapture(
                    EntryECX: freshEcx,
                    FreshGlobalForCompare: capture.FreshGlobalForFinalCompare,
                    EntryEAX: null));

        if (finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome
                    .EntryECXUnsignedBelowGlobalToValidation
            || finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare)
        {
            return FinishFinalTail(
                finalTailWithoutEax,
                derivedFinalEax: null);
        }

        if (finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore
            && capture.SuccessorPairedStoresCapture.EntryEAX is uint retainedEax)
        {
            CapacityFinalCalleeFinalPairedTailPlan finalTailWithEax =
                CapacityFinalCalleeFinalPairedTailProjection.Project(
                    new CapacityFinalCalleeFinalPairedTailCapture(
                        EntryECX: freshEcx,
                        FreshGlobalForCompare: capture.FreshGlobalForFinalCompare,
                        EntryEAX: retainedEax));
            return FinishFinalTail(finalTailWithEax, retainedEax);
        }

        return FinishFinalTail(finalTailWithoutEax, derivedFinalEax: null);

        CapacityFinalCalleePairedFinalLifecyclePlan FinishFinalTail(
            CapacityFinalCalleeFinalPairedTailPlan finalTail,
            uint? derivedFinalEax)
        {
            CapacityFinalCalleePairedFinalLifecycleOutcome outcome =
                MapFinalOutcome(finalTail.Outcome);
            return new(
                capture,
                successor,
                finalTail,
                outcome)
            {
                FinalPairedTailReached = true,
                DerivedFinalEntryECX = freshEcx,
                DerivedFinalEntryEAX = derivedFinalEax,
                ExactCleanExitAdmitted = IsCleanFinalTailExit(finalTail),
                StopAtRawFileOffsetExclusive = finalTail.StopAtRawFileOffsetExclusive,
                Writes = CombinedWrites(finalTail)
            };
        }
    }

    private static bool IsCleanSuccessorZeroExit(
        CapacityFinalCalleeSuccessorPairedStoresPlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.Outcome
            == CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14ZeroToTarget
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == true
        && plan.ZeroBranchSelectedDestinationRawFileOffset == SuccessorZeroDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == SuccessorZeroDestinationRawFileOffset
        && !plan.UnreachedFreshGlobalValueCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus14CaptureProvided;

    private static bool IsCleanSuccessorBelowExit(
        CapacityFinalCalleeSuccessorPairedStoresPlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.Outcome
            == CapacityFinalCalleeSuccessorPairedStoresOutcome
                .EntryECXUnsignedBelowGlobalToValidation
        && plan.GuardBranchReached
        && plan.GuardBranchTaken == true
        && plan.GuardBranchSelectedDestinationRawFileOffset
            == SuccessorUnsignedBelowDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == SuccessorUnsignedBelowDestinationRawFileOffset
        && !plan.UnreachedEntryEAXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus14CaptureProvided;

    private static bool IsCleanSuccessorNonzeroExit(
        CapacityFinalCalleeSuccessorPairedStoresPlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.Outcome
            == CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14NonzeroToBoundary
        && plan.FirstStoreReached
        && plan.SecondStoreReached
        && plan.FreshEsiPlus14Captured
        && plan.TestFreshEcxReached
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == false
        && plan.ZeroBranchSelectedDestinationRawFileOffset == FinalTailEntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == FinalTailEntryRawFileOffset
        && plan.Writes.Length == 2
        && !plan.UnreachedFreshGlobalValueCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus14CaptureProvided;

    private static bool IsCleanFinalTailExit(
        CapacityFinalCalleeFinalPairedTailPlan plan)
    {
        if (!plan.ExactCleanExitAdmitted)
        {
            return false;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare =>
                plan.CompareGlobalReadReached
                && !plan.CompareGlobalCaptured
                && plan.StopAtRawFileOffsetExclusive == FinalTailEntryRawFileOffset,
            CapacityFinalCalleeFinalPairedTailOutcome
                    .EntryECXUnsignedBelowGlobalToValidation =>
                plan.GuardBranchReached
                && plan.GuardBranchTaken == true
                && plan.GuardBranchSelectedDestinationRawFileOffset
                    == FinalUnsignedBelowDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == FinalUnsignedBelowDestinationRawFileOffset
                && !plan.EntryEAXCaptured
                && !plan.UnreachedEntryEAXCaptureProvided,
            CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore =>
                plan.GuardBranchReached
                && plan.GuardBranchTaken == false
                && plan.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeFinalPairedTailProjection.FirstStoreRawFileOffset
                && !plan.EntryEAXCaptured,
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget =>
                plan.FinalJumpReached
                && plan.FinalJumpSelectedDestinationRawFileOffset
                    == FinalPairJumpDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive == FinalPairJumpDestinationRawFileOffset
                && plan.Writes.Length == 2,
            _ => false
        };
    }

    private static CapacityFinalCalleePairedFinalLifecycleOutcome MapFinalOutcome(
        CapacityFinalCalleeFinalPairedTailOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare =>
                CapacityFinalCalleePairedFinalLifecycleOutcome.MissingFreshGlobalForFinalCompare,
            CapacityFinalCalleeFinalPairedTailOutcome.EntryECXUnsignedBelowGlobalToValidation =>
                CapacityFinalCalleePairedFinalLifecycleOutcome
                    .FinalEntryECXUnsignedBelowGlobalToValidation,
            CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore =>
                CapacityFinalCalleePairedFinalLifecycleOutcome
                    .MissingSuccessorEntryEAXForFinalFirstStore,
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget =>
                CapacityFinalCalleePairedFinalLifecycleOutcome.FinalPairedStoresAndJumpToTarget,
            _ => CapacityFinalCalleePairedFinalLifecycleOutcome.SuccessorPairedStoresBlocked
        };
}
