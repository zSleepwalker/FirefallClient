namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeGuardedSuccessorLifecycleCapture(
    CapacityFinalCalleeGuardedFieldStoreCapture GuardedFieldStoreCapture,
    uint? SuccessorFreshGlobalValue,
    uint? SuccessorFreshEsiPlus14,
    uint? SecondFieldFreshECX);

public enum CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
{
    GuardedFieldStoreIncomplete,
    GuardedFieldStoreContaminated,
    GuardedUnsignedBelowToValidation,
    SuccessorIncomplete,
    SuccessorContaminated,
    SuccessorUnsignedBelowToValidation,
    SuccessorFreshEsiPlus14ZeroToTarget,
    SuccessorFreshEsiPlus14NonzeroToBoundary,
    SecondFieldTestIncomplete,
    SecondFieldTestContaminated,
    SecondFieldFreshECXZeroToTarget,
    SecondFieldFreshECXNonzeroToBoundary
}

public sealed record CapacityFinalCalleeGuardedSuccessorLifecyclePlan(
    CapacityFinalCalleeGuardedSuccessorLifecycleCapture Capture,
    CapacityFinalCalleeGuardedSuccessorLifecycleOutcome Outcome)
{
    public CapacityFinalCalleeGuardedFieldStorePlan GuardedFieldStore { get; init; } = null!;
    public CapacityFinalCalleeSuccessorPairedStoresPlan? Successor { get; init; }
    public CapacityFinalCalleeSecondFieldTestPlan? SecondFieldTest { get; init; }
    public uint? DerivedSuccessorEntryECX { get; init; }
    public uint? DerivedSuccessorEntryEAX { get; init; }
    public uint? DerivedSuccessorEntryESI { get; init; }
    public bool UnselectedSuccessorFreshGlobalValueCaptureProvided { get; init; }
    public bool UnselectedSuccessorFreshEsiPlus14CaptureProvided { get; init; }
    public bool UnselectedSecondFieldFreshECXCaptureProvided { get; init; }
    public bool Admitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Joins the guarded 9058 field store to the 906E paired stores, or to the
/// independent 9080 field test when the guarded ESI+10 value is zero.
/// </summary>
public static class CapacityFinalCalleeGuardedSuccessorLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection.EntryRawFileOffset;
    public const uint SuccessorEntryRawFileOffset =
        CapacityFinalCalleeSuccessorPairedStoresProjection.EntryRawFileOffset;
    public const uint SecondFieldEntryRawFileOffset =
        CapacityFinalCalleeSecondFieldTestProjection.EntryRawFileOffset;
    public const uint GuardedValidationDestinationRawFileOffset =
        CapacityFinalCalleeGuardedFieldStoreProjection.ValidationDestinationRawFileOffset;
    public const uint SuccessorValidationDestinationRawFileOffset =
        CapacityFinalCalleeSuccessorPairedStoresProjection.ValidationDestinationRawFileOffset;
    public const uint ZeroDestinationRawFileOffset =
        CapacityFinalCalleeSecondFieldTestProjection.ZeroDestinationRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeSuccessorPairedStoresProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeGuardedSuccessorLifecyclePlan Project(
        CapacityFinalCalleeGuardedSuccessorLifecycleCapture capture)
    {
        CapacityFinalCalleeGuardedFieldStorePlan guarded =
            CapacityFinalCalleeGuardedFieldStoreProjection.Project(
                capture.GuardedFieldStoreCapture);

        CapacityFinalCalleeGuardedSuccessorLifecyclePlan Finish(
            CapacityFinalCalleeGuardedSuccessorLifecycleOutcome outcome,
            uint stopAt,
            bool admitted = false,
            CapacityFinalCalleeSuccessorPairedStoresPlan? successor = null,
            CapacityFinalCalleeSecondFieldTestPlan? secondFieldTest = null,
            uint? successorEcx = null,
            uint? successorEax = null,
            uint? successorEsi = null,
            bool unselectedSuccessorGlobal = false,
            bool unselectedSuccessorFresh14 = false,
            bool unselectedSecondFieldFresh = false) =>
            new(capture, outcome)
            {
                GuardedFieldStore = guarded,
                Successor = successor,
                SecondFieldTest = secondFieldTest,
                DerivedSuccessorEntryECX = successorEcx,
                DerivedSuccessorEntryEAX = successorEax,
                DerivedSuccessorEntryESI = successorEsi,
                UnselectedSuccessorFreshGlobalValueCaptureProvided =
                    unselectedSuccessorGlobal,
                UnselectedSuccessorFreshEsiPlus14CaptureProvided =
                    unselectedSuccessorFresh14,
                UnselectedSecondFieldFreshECXCaptureProvided =
                    unselectedSecondFieldFresh,
                Admitted = admitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (IsExactGuardedBelow(guarded))
        {
            bool contaminated =
                capture.SuccessorFreshGlobalValue.HasValue
                || capture.SuccessorFreshEsiPlus14.HasValue
                || capture.SecondFieldFreshECX.HasValue;
            return Finish(
                contaminated
                    ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .GuardedFieldStoreContaminated
                    : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .GuardedUnsignedBelowToValidation,
                GuardedValidationDestinationRawFileOffset,
                admitted: !contaminated,
                unselectedSuccessorGlobal:
                    capture.SuccessorFreshGlobalValue.HasValue,
                unselectedSuccessorFresh14:
                    capture.SuccessorFreshEsiPlus14.HasValue,
                unselectedSecondFieldFresh:
                    capture.SecondFieldFreshECX.HasValue);
        }

        if (IsExactGuardedZero(guarded))
        {
            bool unselectedSuccessorGlobal =
                capture.SuccessorFreshGlobalValue.HasValue;
            bool unselectedSuccessorFresh14 =
                capture.SuccessorFreshEsiPlus14.HasValue;
            if (unselectedSuccessorGlobal || unselectedSuccessorFresh14)
            {
                return Finish(
                    CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .GuardedFieldStoreContaminated,
                    guarded.StopAtRawFileOffsetExclusive,
                    unselectedSuccessorGlobal: unselectedSuccessorGlobal,
                    unselectedSuccessorFresh14: unselectedSuccessorFresh14);
            }

            CapacityFinalCalleeSecondFieldTestPlan second =
                CapacityFinalCalleeSecondFieldTestProjection.Project(new(
                    EntryESI: guarded.Capture.EntryESI,
                    FreshECX: capture.SecondFieldFreshECX));
            bool childClean = IsExactSecondFieldExit(second);
            bool contaminated = second.UnreachedFreshECXCaptureProvided;
            return Finish(
                contaminated
                    ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .SecondFieldTestContaminated
                    : childClean
                        ? second.ZeroBranchTaken == true
                            ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                                .SecondFieldFreshECXZeroToTarget
                            : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                                .SecondFieldFreshECXNonzeroToBoundary
                        : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SecondFieldTestIncomplete,
                second.StopAtRawFileOffsetExclusive,
                admitted: !contaminated && childClean,
                secondFieldTest: second,
                unselectedSecondFieldFresh: false);
        }

        if (IsExactGuardedNonzero(guarded))
        {
            if (capture.SecondFieldFreshECX.HasValue)
            {
                return Finish(
                    CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .GuardedFieldStoreContaminated,
                    guarded.StopAtRawFileOffsetExclusive,
                    unselectedSecondFieldFresh: true);
            }

            uint? successorEcx = guarded.FreshEsiPlus10;
            var successorCapture = new CapacityFinalCalleeSuccessorPairedStoresCapture(
                EntryECX: successorEcx,
                FreshGlobalValue: capture.SuccessorFreshGlobalValue,
                EntryEAX: null,
                EntryESI: null,
                FreshEsiPlus14: capture.SuccessorFreshEsiPlus14);
            CapacityFinalCalleeSuccessorPairedStoresPlan successor =
                CapacityFinalCalleeSuccessorPairedStoresProjection.Project(
                    successorCapture);

            if (successor.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome
                    .EntryECXUnsignedBelowGlobalToValidation)
            {
                bool clean = IsExactSuccessorBelow(successor);
                return Finish(
                    clean
                        ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorUnsignedBelowToValidation
                        : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorContaminated,
                    successor.StopAtRawFileOffsetExclusive,
                    admitted: clean,
                    successor: successor,
                    successorEcx: successorEcx,
                    unselectedSecondFieldFresh: false);
            }

            if (successor.Outcome
                == CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue)
            {
                return Finish(
                    successor.UnreachedFreshEsiPlus14CaptureProvided
                        ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorContaminated
                        : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorIncomplete,
                    successor.StopAtRawFileOffsetExclusive,
                    successor: successor,
                    successorEcx: successorEcx,
                    unselectedSecondFieldFresh: false);
            }

            if (!successor.GuardBranchReached
                || successor.GuardBranchTaken != false
                || successor.GuardBranchSelectedDestinationRawFileOffset
                    != CapacityFinalCalleeSuccessorPairedStoresProjection
                        .FirstStoreRawFileOffset)
            {
                return Finish(
                    successor.UnreachedFreshEsiPlus14CaptureProvided
                        ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorContaminated
                        : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorIncomplete,
                    successor.StopAtRawFileOffsetExclusive,
                    successor: successor,
                    successorEcx: successorEcx,
                    unselectedSecondFieldFresh: false);
            }

            // EAX is first consumed at the first store, ESI at the later fresh read.
            successorCapture = successorCapture with
            {
                EntryEAX = guarded.Capture.EntryEAX
            };
            successor = CapacityFinalCalleeSuccessorPairedStoresProjection.Project(
                successorCapture);
            if (successor.FirstStoreReached && successor.SecondStoreReached)
            {
                successorCapture = successorCapture with
                {
                    EntryESI = guarded.Capture.EntryESI
                };
                successor = CapacityFinalCalleeSuccessorPairedStoresProjection.Project(
                    successorCapture);
            }
            bool exactSuccessor = IsExactSuccessorExit(successor);
            bool successorContaminated = successor.UnreachedEntryEAXCaptureProvided
                || successor.UnreachedEntryESICaptureProvided
                || successor.UnreachedFreshEsiPlus14CaptureProvided;
            return Finish(
                successorContaminated
                    ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                        .SuccessorContaminated
                    : !exactSuccessor
                        ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                            .SuccessorIncomplete
                        : successor.ZeroBranchTaken == true
                            ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                                .SuccessorFreshEsiPlus14ZeroToTarget
                            : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                                .SuccessorFreshEsiPlus14NonzeroToBoundary,
                successor.StopAtRawFileOffsetExclusive,
                admitted: exactSuccessor && !successorContaminated,
                successor: successor,
                successorEcx: successorEcx,
                successorEax: successor.EntryEAXCaptured
                    ? guarded.Capture.EntryEAX
                    : null,
                successorEsi: successor.EntryESICaptured
                    ? guarded.Capture.EntryESI
                    : null,
                unselectedSecondFieldFresh: false);
        }

        bool guardedContaminated =
            guarded.UnreachedFreshGlobalForCompareCaptureProvided
            || guarded.UnreachedEntryECXCaptureProvided
            || guarded.UnreachedEntryESICaptureProvided
            || guarded.UnreachedFreshEsiPlus10CaptureProvided
            || capture.SuccessorFreshGlobalValue.HasValue
            || capture.SuccessorFreshEsiPlus14.HasValue
            || capture.SecondFieldFreshECX.HasValue;
        return Finish(
            guardedContaminated
                ? CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreContaminated
                : CapacityFinalCalleeGuardedSuccessorLifecycleOutcome
                    .GuardedFieldStoreIncomplete,
            guarded.StopAtRawFileOffsetExclusive,
            unselectedSuccessorGlobal:
                capture.SuccessorFreshGlobalValue.HasValue,
            unselectedSuccessorFresh14:
                capture.SuccessorFreshEsiPlus14.HasValue,
            unselectedSecondFieldFresh:
                capture.SecondFieldFreshECX.HasValue);
    }

    private static bool IsExactGuardedBelow(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.Outcome == CapacityFinalCalleeGuardedFieldStoreOutcome
            .EntryEAXUnsignedBelowGlobalToValidation
        && plan.ExactCleanExitAdmitted
        && plan.EntryEAXCaptured
        && plan.CompareGlobalReadReached
        && plan.CompareGlobalCaptured
        && plan.CompareReached
        && plan.CompareResult
            == unchecked(plan.Capture.EntryEAX!.Value
                - plan.Capture.FreshGlobalForCompare!.Value)
        && plan.CompareFlags == CompareFlags(
            plan.Capture.EntryEAX!.Value, plan.Capture.FreshGlobalForCompare!.Value)
        && plan.EntryEAXUnsignedBelowGlobal == true
        && plan.BranchReached
        && plan.BranchTaken == true
        && plan.BranchSelectedDestinationRawFileOffset
            == GuardedValidationDestinationRawFileOffset
        && !plan.StoreReached
        && !plan.FreshEsiPlus10ReadReached
        && !plan.UnreachedEntryECXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus10CaptureProvided
        && plan.StopAtRawFileOffsetExclusive == GuardedValidationDestinationRawFileOffset;

    private static bool IsExactGuardedZero(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.Outcome == CapacityFinalCalleeGuardedFieldStoreOutcome
            .FreshEsiPlus10ZeroToTarget
        && IsCleanGuardedFreshExit(plan, zero: true);

    private static bool IsExactGuardedNonzero(
        CapacityFinalCalleeGuardedFieldStorePlan plan) =>
        plan.Outcome == CapacityFinalCalleeGuardedFieldStoreOutcome
            .FreshEsiPlus10NonzeroToBoundary
        && IsCleanGuardedFreshExit(plan, zero: false);

    private static bool IsCleanGuardedFreshExit(
        CapacityFinalCalleeGuardedFieldStorePlan plan,
        bool zero) =>
        plan.ExactCleanExitAdmitted
        && plan.EntryEAXCaptured
        && plan.CompareGlobalReadReached
        && plan.CompareGlobalCaptured
        && plan.CompareReached
        && plan.CompareResult
            == unchecked(plan.Capture.EntryEAX!.Value
                - plan.Capture.FreshGlobalForCompare!.Value)
        && plan.CompareFlags == CompareFlags(
            plan.Capture.EntryEAX!.Value, plan.Capture.FreshGlobalForCompare!.Value)
        && plan.EntryEAXUnsignedBelowGlobal == false
        && plan.BranchReached
        && plan.BranchTaken == false
        && plan.BranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeGuardedFieldStoreProjection.StoreRawFileOffset
        && plan.EntryECXCaptured
        && plan.StoreReached
        && plan.StoreAddress == CapacityFinalCalleeGuardedFieldStoreProjection
            .DeriveStoreAddress(plan.Capture.EntryEAX!.Value)
        && plan.StoreValue == plan.Capture.EntryECX
        && plan.Writes.Length == 1
        && plan.Writes[0] == new CapacityFinalCalleeGuardedFieldStoreWrite(
            CapacityFinalCalleeGuardedFieldStoreProjection.StoreRawFileOffset,
            CapacityFinalCalleeGuardedFieldStoreProjection.DeriveStoreAddress(
                plan.Capture.EntryEAX!.Value),
            plan.Capture.EntryECX!.Value)
        && plan.EntryESICaptured
        && plan.FreshEsiPlus10ReadReached
        && plan.FreshEsiPlus10Address == CapacityFinalCalleeGuardedFieldStoreProjection
            .DeriveFreshEsiPlus10Address(plan.Capture.EntryESI!.Value)
        && plan.FreshEsiPlus10Captured
        && plan.FreshEsiPlus10.HasValue
        && plan.TestECXReached
        && plan.TestECXValue == plan.FreshEsiPlus10
        && plan.TestECXFlags == TestFlags(plan.FreshEsiPlus10!.Value)
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == zero
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == (zero
                ? CapacityFinalCalleeGuardedFieldStoreProjection.ZeroDestinationRawFileOffset
                : CapacityFinalCalleeGuardedFieldStoreProjection.BoundaryRawFileOffsetExclusive)
        && !plan.UnreachedFreshGlobalForCompareCaptureProvided
        && !plan.UnreachedEntryECXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus10CaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == plan.ZeroBranchSelectedDestinationRawFileOffset;

    private static bool IsExactSuccessorBelow(
        CapacityFinalCalleeSuccessorPairedStoresPlan plan) =>
        plan.ExactCleanExitAdmitted
        && plan.EntryECXCaptured
        && plan.CompareGlobalReadReached
        && plan.CompareGlobalValueCaptured
        && plan.CompareReached
        && plan.CompareResult
            == unchecked(plan.Capture.EntryECX!.Value
                - plan.Capture.FreshGlobalValue!.Value)
        && plan.CompareFlags == SuccessorCompareFlags(
            plan.Capture.EntryECX!.Value, plan.Capture.FreshGlobalValue!.Value)
        && plan.EntryECXUnsignedBelowGlobal == true
        && plan.GuardBranchReached
        && plan.GuardBranchTaken == true
        && plan.GuardBranchSelectedDestinationRawFileOffset
            == SuccessorValidationDestinationRawFileOffset
        && !plan.FirstStoreReached
        && !plan.SecondStoreReached
        && !plan.FreshEsiPlus14ReadReached
        && !plan.UnreachedEntryEAXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus14CaptureProvided
        && plan.StopAtRawFileOffsetExclusive == SuccessorValidationDestinationRawFileOffset;

    private static bool IsExactSuccessorExit(
        CapacityFinalCalleeSuccessorPairedStoresPlan plan) =>
        (plan.Outcome is CapacityFinalCalleeSuccessorPairedStoresOutcome
            .FreshEsiPlus14ZeroToTarget
            or CapacityFinalCalleeSuccessorPairedStoresOutcome
                .FreshEsiPlus14NonzeroToBoundary)
        && plan.ExactCleanExitAdmitted
        && plan.EntryECXCaptured
        && plan.CompareGlobalReadReached
        && plan.CompareGlobalValueCaptured
        && plan.CompareReached
        && plan.CompareResult
            == unchecked(plan.Capture.EntryECX!.Value
                - plan.Capture.FreshGlobalValue!.Value)
        && plan.CompareFlags == SuccessorCompareFlags(
            plan.Capture.EntryECX!.Value, plan.Capture.FreshGlobalValue!.Value)
        && plan.EntryECXUnsignedBelowGlobal == false
        && plan.GuardBranchReached
        && plan.GuardBranchTaken == false
        && plan.GuardBranchSelectedDestinationRawFileOffset
            == CapacityFinalCalleeSuccessorPairedStoresProjection.FirstStoreRawFileOffset
        && plan.EntryEAXCaptured
        && plan.FirstStoreReached
        && plan.FirstStoreAddress == CapacityFinalCalleeSuccessorPairedStoresProjection
            .DeriveFirstStoreAddress(plan.Capture.EntryEAX!.Value)
        && plan.FirstStoreValue == plan.Capture.EntryECX
        && plan.SecondStoreReached
        && plan.SecondStoreAddress == CapacityFinalCalleeSuccessorPairedStoresProjection
            .DeriveSecondStoreAddress(plan.Capture.EntryECX!.Value)
        && plan.SecondStoreValue == plan.Capture.EntryEAX
        && plan.Writes.Length == 2
        && plan.Writes[0] == new CapacityFinalCalleeSuccessorPairedStoresWrite(
            CapacityFinalCalleeSuccessorPairedStoresProjection.FirstStoreRawFileOffset,
            CapacityFinalCalleeSuccessorPairedStoresProjection.DeriveFirstStoreAddress(
                plan.Capture.EntryEAX!.Value),
            plan.Capture.EntryECX!.Value)
        && plan.Writes[1] == new CapacityFinalCalleeSuccessorPairedStoresWrite(
            CapacityFinalCalleeSuccessorPairedStoresProjection.SecondStoreRawFileOffset,
            CapacityFinalCalleeSuccessorPairedStoresProjection.DeriveSecondStoreAddress(
                plan.Capture.EntryECX!.Value),
            plan.Capture.EntryEAX!.Value)
        && plan.EntryESICaptured
        && plan.FreshEsiPlus14ReadReached
        && plan.FreshEsiPlus14Address == CapacityFinalCalleeSuccessorPairedStoresProjection
            .DeriveFreshEsiPlus14Address(plan.Capture.EntryESI!.Value)
        && plan.FreshEsiPlus14Captured
        && plan.FreshEsiPlus14.HasValue
        && plan.TestFreshEcxReached
        && plan.TestFreshEcxFlags == SuccessorTestFlags(plan.FreshEsiPlus14!.Value)
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == (plan.FreshEsiPlus14.Value == 0)
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == (plan.FreshEsiPlus14.Value == 0
                ? CapacityFinalCalleeSuccessorPairedStoresProjection
                    .ZeroDestinationRawFileOffset
                : BoundaryRawFileOffsetExclusive)
        && !plan.UnreachedFreshGlobalValueCaptureProvided
        && !plan.UnreachedEntryEAXCaptureProvided
        && !plan.UnreachedEntryESICaptureProvided
        && !plan.UnreachedFreshEsiPlus14CaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == plan.ZeroBranchSelectedDestinationRawFileOffset;

    private static bool IsExactSecondFieldExit(
        CapacityFinalCalleeSecondFieldTestPlan plan) =>
        (plan.Outcome is CapacityFinalCalleeSecondFieldTestOutcome.FreshECXZeroToTarget
            or CapacityFinalCalleeSecondFieldTestOutcome.FreshECXNonzeroToBoundary)
        && plan.ExactCleanExitAdmitted
        && plan.EntryESICaptured
        && plan.FreshEsiPlus14Address
            == CapacityFinalCalleeSecondFieldTestProjection.DeriveFreshEsiPlus14Address(
                plan.Capture.EntryESI!.Value)
        && plan.FreshECXReadReached
        && plan.FreshECXCaptured
        && plan.FreshECX.HasValue
        && plan.TestECXReached
        && plan.TestECXFlags == SecondFieldTestFlags(plan.FreshECX.Value)
        && plan.ZeroBranchReached
        && plan.ZeroBranchTaken == (plan.FreshECX.Value == 0)
        && plan.ZeroBranchSelectedDestinationRawFileOffset
            == (plan.FreshECX.Value == 0
                ? ZeroDestinationRawFileOffset
                : BoundaryRawFileOffsetExclusive)
        && !plan.UnreachedFreshECXCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == plan.ZeroBranchSelectedDestinationRawFileOffset;

    private static CapacityFinalCalleeGuardedFieldStoreCompareFlags CompareFlags(
        uint left, uint right)
    {
        uint result = unchecked(left - right);
        return new(left < right, EvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0, result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeSuccessorPairedStoresCompareFlags
        SuccessorCompareFlags(uint left, uint right)
    {
        uint result = unchecked(left - right);
        return new(left < right, EvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0, result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeGuardedFieldStoreTestFlags TestFlags(uint value) =>
        new(false, EvenParity((byte)value), value == 0,
            (value & 0x80000000) != 0, false);

    private static CapacityFinalCalleeSuccessorPairedStoresTestFlags
        SuccessorTestFlags(uint value) =>
        new(false, EvenParity((byte)value), value == 0,
            (value & 0x80000000) != 0, false);

    private static CapacityFinalCalleeSecondFieldTestFlags
        SecondFieldTestFlags(uint value) =>
        new(false, EvenParity((byte)value), value == 0,
            (value & 0x80000000) != 0, false);

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;
}
