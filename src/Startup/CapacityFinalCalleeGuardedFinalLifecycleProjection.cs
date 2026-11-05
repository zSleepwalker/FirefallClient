using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeGuardedFinalLifecycleCapture(
    CapacityFinalCalleeGuardedSuccessorLifecycleCapture GuardedSuccessorLifecycleCapture,
    uint? FreshGlobalForFinalCompare);

public enum CapacityFinalCalleeGuardedFinalLifecycleOutcome
{
    GuardedSuccessorLifecycleBlocked,
    GuardedUnsignedBelowToValidation,
    GuardedUnsignedBelowContaminated,
    SuccessorUnsignedBelowToValidation,
    SuccessorUnsignedBelowContaminated,
    SuccessorZeroToTarget,
    SuccessorZeroContaminated,
    SecondFieldZeroToTarget,
    SecondFieldZeroContaminated,
    MissingFreshGlobalForFinalCompare,
    FinalEntryECXUnsignedBelowGlobalToValidation,
    MissingRetainedEAXForFinalFirstStore,
    FinalPairedStoresAndJumpToTarget
}

public enum CapacityFinalCalleeGuardedFinalLifecycleECXSource
{
    None,
    SuccessorFreshEsiPlus14,
    IndependentSecondFieldFreshECX
}

public readonly record struct CapacityFinalCalleeGuardedFinalLifecycleWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeGuardedFinalLifecyclePlan(
    CapacityFinalCalleeGuardedFinalLifecycleCapture Capture,
    CapacityFinalCalleeGuardedSuccessorLifecyclePlan GuardedSuccessorLifecyclePlan,
    CapacityFinalCalleeFinalPairedTailPlan? FinalPairedTailPlan,
    CapacityFinalCalleeGuardedFinalLifecycleOutcome Outcome)
{
    public bool FinalPairedTailReached { get; init; }
    public CapacityFinalCalleeGuardedFinalLifecycleECXSource FinalEntryECXSource { get; init; }
    public uint? DerivedFinalEntryECX { get; init; }
    public uint? DerivedFinalEntryEAX { get; init; }
    public bool UnreachedFreshGlobalForFinalCompareCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleeGuardedFinalLifecycleWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeGuardedFinalLifecycleWrite>.Empty;
}

/// <summary>
/// Joins either clean 0x908B route from GuardedSuccessorLifecycle to FinalPairedTail.
/// </summary>
public static class CapacityFinalCalleeGuardedFinalLifecycleProjection
{
    public const uint GuardedEntryRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection.EntryRawFileOffset;
    public const uint SuccessorEntryRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SuccessorEntryRawFileOffset;
    public const uint SecondFieldEntryRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection.SecondFieldEntryRawFileOffset;
    public const uint FinalTailEntryRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.EntryRawFileOffset;
    public const uint GuardedValidationDestinationRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection
            .GuardedValidationDestinationRawFileOffset;
    public const uint SuccessorValidationDestinationRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection
            .SuccessorValidationDestinationRawFileOffset;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeGuardedSuccessorLifecycleProjection.ZeroDestinationRawFileOffset;
    public const uint FinalValidationDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.ValidationDestinationRawFileOffset;
    public const uint FinalJumpDestinationRawFileOffset =
        CapacityFinalCalleeFinalPairedTailProjection.FinalJumpDestinationRawFileOffset;

    public static CapacityFinalCalleeGuardedFinalLifecyclePlan Project(
        CapacityFinalCalleeGuardedFinalLifecycleCapture capture)
    {
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan guarded =
            CapacityFinalCalleeGuardedSuccessorLifecycleProjection.Project(
                capture.GuardedSuccessorLifecycleCapture);

        ImmutableArray<CapacityFinalCalleeGuardedFinalLifecycleWrite> EarlierWrites()
        {
            var writes =
                ImmutableArray.CreateBuilder<CapacityFinalCalleeGuardedFinalLifecycleWrite>();
            foreach (CapacityFinalCalleeGuardedFieldStoreWrite write
                in guarded.GuardedFieldStore.Writes)
            {
                writes.Add(new(write.RawFileOffset, write.Address, write.Value));
            }

            if (guarded.Successor is not null)
            {
                foreach (CapacityFinalCalleeSuccessorPairedStoresWrite write
                    in guarded.Successor.Writes)
                {
                    writes.Add(new(write.RawFileOffset, write.Address, write.Value));
                }
            }

            return writes.ToImmutable();
        }

        ImmutableArray<CapacityFinalCalleeGuardedFinalLifecycleWrite> CombinedWrites(
            CapacityFinalCalleeFinalPairedTailPlan? finalTail)
        {
            ImmutableArray<CapacityFinalCalleeGuardedFinalLifecycleWrite> earlier =
                EarlierWrites();
            if (finalTail is null)
            {
                return earlier;
            }

            var writes =
                ImmutableArray.CreateBuilder<CapacityFinalCalleeGuardedFinalLifecycleWrite>();
            writes.AddRange(earlier);
            foreach (CapacityFinalCalleeFinalPairedTailWrite write in finalTail.Writes)
            {
                writes.Add(new(write.RawFileOffset, write.Address, write.Value));
            }

            return writes.ToImmutable();
        }

        CapacityFinalCalleeGuardedFinalLifecyclePlan FinishWithoutTail(
            CapacityFinalCalleeGuardedFinalLifecycleOutcome outcome,
            bool unreachedFinalGlobal,
            bool exactCleanExit) =>
            new(capture, guarded, null, outcome)
            {
                UnreachedFreshGlobalForFinalCompareCaptureProvided = unreachedFinalGlobal,
                ExactCleanExitAdmitted = exactCleanExit,
                StopAtRawFileOffsetExclusive = guarded.StopAtRawFileOffsetExclusive,
                Writes = CombinedWrites(finalTail: null)
            };

        if (IsCleanSuccessorZero(guarded))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutTail(
                contaminated
                    ? CapacityFinalCalleeGuardedFinalLifecycleOutcome.SuccessorZeroContaminated
                    : CapacityFinalCalleeGuardedFinalLifecycleOutcome.SuccessorZeroToTarget,
                contaminated,
                !contaminated);
        }

        if (IsCleanSecondFieldZero(guarded))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutTail(
                contaminated
                    ? CapacityFinalCalleeGuardedFinalLifecycleOutcome.SecondFieldZeroContaminated
                    : CapacityFinalCalleeGuardedFinalLifecycleOutcome.SecondFieldZeroToTarget,
                contaminated,
                !contaminated);
        }

        if (IsCleanGuardedBelow(guarded))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutTail(
                contaminated
                    ? CapacityFinalCalleeGuardedFinalLifecycleOutcome
                        .GuardedUnsignedBelowContaminated
                    : CapacityFinalCalleeGuardedFinalLifecycleOutcome
                        .GuardedUnsignedBelowToValidation,
                contaminated,
                !contaminated);
        }

        if (IsCleanSuccessorBelow(guarded))
        {
            bool contaminated = capture.FreshGlobalForFinalCompare.HasValue;
            return FinishWithoutTail(
                contaminated
                    ? CapacityFinalCalleeGuardedFinalLifecycleOutcome
                        .SuccessorUnsignedBelowContaminated
                    : CapacityFinalCalleeGuardedFinalLifecycleOutcome
                        .SuccessorUnsignedBelowToValidation,
                contaminated,
                !contaminated);
        }

        if (!TryGetFinalECX(
                guarded,
                out uint finalEcx,
                out CapacityFinalCalleeGuardedFinalLifecycleECXSource ecxSource))
        {
            return FinishWithoutTail(
                CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .GuardedSuccessorLifecycleBlocked,
                capture.FreshGlobalForFinalCompare.HasValue,
                exactCleanExit: false);
        }

        CapacityFinalCalleeFinalPairedTailPlan finalTailWithoutEax =
            CapacityFinalCalleeFinalPairedTailProjection.Project(
                new(
                    EntryECX: finalEcx,
                    FreshGlobalForCompare: capture.FreshGlobalForFinalCompare,
                    EntryEAX: null));

        if (finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare
            || finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.EntryECXUnsignedBelowGlobalToValidation)
        {
            return FinishFinalTail(finalTailWithoutEax, derivedFinalEax: null);
        }

        if (finalTailWithoutEax.Outcome
                == CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore
            && TryGetRetainedEAX(guarded, out uint retainedEax))
        {
            CapacityFinalCalleeFinalPairedTailPlan finalTailWithEax =
                CapacityFinalCalleeFinalPairedTailProjection.Project(
                    new(
                        EntryECX: finalEcx,
                        FreshGlobalForCompare: capture.FreshGlobalForFinalCompare,
                        EntryEAX: retainedEax));
            return FinishFinalTail(finalTailWithEax, retainedEax);
        }

        return FinishFinalTail(finalTailWithoutEax, derivedFinalEax: null);

        CapacityFinalCalleeGuardedFinalLifecyclePlan FinishFinalTail(
            CapacityFinalCalleeFinalPairedTailPlan finalTail,
            uint? derivedFinalEax)
        {
            return new(
                capture,
                guarded,
                finalTail,
                MapFinalOutcome(finalTail.Outcome))
            {
                FinalPairedTailReached = true,
                FinalEntryECXSource = ecxSource,
                DerivedFinalEntryECX = finalEcx,
                DerivedFinalEntryEAX = derivedFinalEax,
                ExactCleanExitAdmitted =
                    guarded.Admitted && IsCleanFinalTailExit(finalTail),
                StopAtRawFileOffsetExclusive = finalTail.StopAtRawFileOffsetExclusive,
                Writes = CombinedWrites(finalTail)
            };
        }
    }

    private static bool TryGetFinalECX(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan,
        out uint finalEcx,
        out CapacityFinalCalleeGuardedFinalLifecycleECXSource source)
    {
        if (IsCleanSuccessorNonzero(plan)
            && plan.Successor?.FreshEsiPlus14 is uint successorFreshECX)
        {
            finalEcx = successorFreshECX;
            source = CapacityFinalCalleeGuardedFinalLifecycleECXSource
                .SuccessorFreshEsiPlus14;
            return true;
        }

        if (IsCleanSecondFieldNonzero(plan)
            && plan.SecondFieldTest?.FreshECX is uint secondFieldFreshECX)
        {
            finalEcx = secondFieldFreshECX;
            source = CapacityFinalCalleeGuardedFinalLifecycleECXSource
                .IndependentSecondFieldFreshECX;
            return true;
        }

        finalEcx = 0;
        source = CapacityFinalCalleeGuardedFinalLifecycleECXSource.None;
        return false;
    }

    private static bool TryGetRetainedEAX(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan,
        out uint retainedEax)
    {
        if (plan.Successor is not null
            && plan.DerivedSuccessorEntryEAX is uint successorEax)
        {
            retainedEax = successorEax;
            return true;
        }

        if (plan.SecondFieldTest is not null
            && plan.GuardedFieldStore.Capture.EntryEAX is uint guardedEax)
        {
            retainedEax = guardedEax;
            return true;
        }

        retainedEax = 0;
        return false;
    }

    private static bool IsCleanSuccessorNonzero(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .SuccessorFreshEsiPlus14NonzeroToBoundary
        && plan.Successor is
        {
            ExactCleanExitAdmitted: true,
            FirstStoreReached: true,
            SecondStoreReached: true,
            FreshEsiPlus14Captured: true,
            TestFreshEcxReached: true,
            ZeroBranchReached: true,
            ZeroBranchTaken: false,
            ZeroBranchSelectedDestinationRawFileOffset: FinalTailEntryRawFileOffset,
            StopAtRawFileOffsetExclusive: FinalTailEntryRawFileOffset,
            Writes.Length: 2,
            UnreachedFreshGlobalValueCaptureProvided: false,
            UnreachedFreshEsiPlus14CaptureProvided: false
        };

    private static bool IsCleanSecondFieldNonzero(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .SecondFieldFreshECXNonzeroToBoundary
        && plan.SecondFieldTest is
        {
            ExactCleanExitAdmitted: true,
            FreshECXCaptured: true,
            TestECXReached: true,
            ZeroBranchReached: true,
            ZeroBranchTaken: false,
            ZeroBranchSelectedDestinationRawFileOffset: FinalTailEntryRawFileOffset,
            StopAtRawFileOffsetExclusive: FinalTailEntryRawFileOffset,
            UnreachedFreshECXCaptureProvided: false
        };

    private static bool IsCleanSuccessorZero(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .SuccessorFreshEsiPlus14ZeroToTarget
        && plan.Successor is
        {
            ExactCleanExitAdmitted: true,
            ZeroBranchReached: true,
            ZeroBranchTaken: true,
            ZeroBranchSelectedDestinationRawFileOffset: ZeroDestinationRawFileOffset,
            StopAtRawFileOffsetExclusive: ZeroDestinationRawFileOffset
        };

    private static bool IsCleanSecondFieldZero(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .SecondFieldFreshECXZeroToTarget
        && plan.SecondFieldTest is
        {
            ExactCleanExitAdmitted: true,
            ZeroBranchReached: true,
            ZeroBranchTaken: true,
            ZeroBranchSelectedDestinationRawFileOffset: ZeroDestinationRawFileOffset,
            StopAtRawFileOffsetExclusive: ZeroDestinationRawFileOffset
        };

    private static bool IsCleanGuardedBelow(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .GuardedUnsignedBelowToValidation
        && plan.GuardedFieldStore.ExactCleanExitAdmitted
        && plan.GuardedFieldStore.BranchTaken == true
        && plan.GuardedFieldStore.BranchSelectedDestinationRawFileOffset
            == GuardedValidationDestinationRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == GuardedValidationDestinationRawFileOffset;

    private static bool IsCleanSuccessorBelow(
        CapacityFinalCalleeGuardedSuccessorLifecyclePlan plan) =>
        plan.Admitted
        && plan.Outcome
            == CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                .SuccessorUnsignedBelowToValidation
        && plan.Successor is
        {
            ExactCleanExitAdmitted: true,
            GuardBranchReached: true,
            GuardBranchTaken: true,
            GuardBranchSelectedDestinationRawFileOffset:
                SuccessorValidationDestinationRawFileOffset,
            StopAtRawFileOffsetExclusive: SuccessorValidationDestinationRawFileOffset,
            Writes.IsEmpty: true
        };

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
            CapacityFinalCalleeFinalPairedTailOutcome.EntryECXUnsignedBelowGlobalToValidation =>
                plan.GuardBranchReached
                && plan.GuardBranchTaken == true
                && plan.GuardBranchSelectedDestinationRawFileOffset
                    == FinalValidationDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == FinalValidationDestinationRawFileOffset
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
                    == FinalJumpDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive == FinalJumpDestinationRawFileOffset
                && plan.Writes.Length == 2,
            _ => false
        };
    }

    private static CapacityFinalCalleeGuardedFinalLifecycleOutcome MapFinalOutcome(
        CapacityFinalCalleeFinalPairedTailOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare =>
                CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .MissingFreshGlobalForFinalCompare,
            CapacityFinalCalleeFinalPairedTailOutcome.EntryECXUnsignedBelowGlobalToValidation =>
                CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalEntryECXUnsignedBelowGlobalToValidation,
            CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore =>
                CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .MissingRetainedEAXForFinalFirstStore,
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget =>
                CapacityFinalCalleeGuardedFinalLifecycleOutcome
                    .FinalPairedStoresAndJumpToTarget,
            _ => CapacityFinalCalleeGuardedFinalLifecycleOutcome
                .GuardedSuccessorLifecycleBlocked
        };
}
