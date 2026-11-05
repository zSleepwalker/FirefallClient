namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityDispatchLifecycleCapture(
    FrameHandoffCapacityTransferLifecycleCapture FirstTransfer,
    FrameHandoffCapacityAboveBranchLifecycleCapture? AboveBranch,
    FrameHandoffCapacitySecondBranchLifecycleCapture? SecondBranch);

public enum FrameHandoffCapacityDispatchRoute
{
    AboveCapacity,
    AtOrBelowCapacity
}

public enum FrameHandoffCapacityDispatchLifecycleOutcome
{
    FirstTransferBlocked,
    FirstTransferHasUnreachedCaptures,
    FirstReturnEntryIdentityMismatch,
    FirstReturnBlocked,
    FirstReturnHasUnreachedCaptures,
    FirstRouteNotAdmitted,
    SelectedBranchCaptureMissing,
    UnselectedBranchCaptureProvided,
    AboveBranchBlocked,
    AboveBranchHasUnreachedCaptures,
    SecondBranchBlocked,
    SecondBranchHasUnreachedCaptures,
    ProjectionComplete
}

public readonly record struct FrameHandoffCapacityDispatchLifecyclePlan(
    FrameHandoffCapacityDispatchLifecycleCapture Capture,
    FrameHandoffCapacityDispatchLifecycleOutcome Outcome,
    FrameHandoffCapacityTransferLifecyclePlan FirstTransferPlan,
    FrameHandoffCapacityDispatchRoute? SelectedRoute,
    FrameHandoffCapacityAboveBranchLifecycleCapture? DerivedAboveBranchCapture,
    FrameHandoffCapacityAboveBranchLifecyclePlan? AboveBranchPlan,
    FrameHandoffCapacitySecondBranchLifecycleCapture? DerivedSecondBranchCapture,
    FrameHandoffCapacitySecondBranchLifecyclePlan? SecondBranchPlan,
    bool FirstRouteAdmissionValidated,
    bool SelectedBranchCaptureProvided,
    bool UnselectedBranchCaptureProvided,
    bool UnreachedAboveBranchCaptureProvided,
    bool UnreachedSecondBranchCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Admits the bounded first transfer return and dispatches only its selected successor.
/// </summary>
public static class FrameHandoffCapacityDispatchLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        FrameHandoffCapacityTransferLifecycleProjection.LifecycleStartRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        FrameHandoffCapacityAboveBranchLifecycleProjection.LifecycleEndRawFileOffsetExclusive;

    public static FrameHandoffCapacityDispatchLifecyclePlan Project(
        FrameHandoffCapacityDispatchLifecycleCapture capture)
    {
        FrameHandoffCapacityTransferLifecyclePlan firstPlan =
            FrameHandoffCapacityTransferLifecycleProjection.Project(capture.FirstTransfer);

        if (!TryAdmitRoute(firstPlan, out FrameHandoffCapacityDispatchRoute route))
        {
            return StopBeforeDispatch(capture, firstPlan, MapFirstOutcome(firstPlan),
                stopAt: firstPlan.StopAtRawFileOffsetExclusive);
        }

        if (route == FrameHandoffCapacityDispatchRoute.AboveCapacity)
        {
            bool unusedBranchProvided = capture.SecondBranch.HasValue;
            if (capture.AboveBranch is not FrameHandoffCapacityAboveBranchLifecycleCapture
                aboveCapture)
            {
                return StopBeforeSelectedBranch(
                    capture,
                    firstPlan,
                    route,
                    selectedBranchCaptureProvided: false,
                    unusedBranchProvided: unusedBranchProvided,
                    outcome: FrameHandoffCapacityDispatchLifecycleOutcome
                        .SelectedBranchCaptureMissing,
                    stopAt: FrameHandoffCapacityAboveTransferProjection.EntryRawFileOffset);
            }

            FrameHandoffCapacityTransferReturnValues values = GetReturnValues(firstPlan);
            FrameHandoffCapacityAboveBranchLifecycleCapture derivedCapture =
                aboveCapture with
                {
                    AboveLifecycle = aboveCapture.AboveLifecycle with
                    {
                        AboveTransfer = aboveCapture.AboveLifecycle.AboveTransfer with
                        {
                            ValidatedBranchEntry = true,
                            EntryEsi = values.EntryEsi,
                            EntryEcx = values.Receiver4,
                            EntryEdx = values.Edx
                        }
                    }
                };
            FrameHandoffCapacityAboveBranchLifecyclePlan abovePlan =
                FrameHandoffCapacityAboveBranchLifecycleProjection.Project(derivedCapture);
            FrameHandoffCapacityDispatchLifecycleOutcome outcome =
                MapAboveOutcome(abovePlan, unusedBranchProvided);

            return new(
                capture,
                outcome,
                firstPlan,
                route,
                derivedCapture,
                abovePlan,
                null,
                null,
                FirstRouteAdmissionValidated: true,
                SelectedBranchCaptureProvided: true,
                UnselectedBranchCaptureProvided: unusedBranchProvided,
                UnreachedAboveBranchCaptureProvided: false,
                UnreachedSecondBranchCaptureProvided: unusedBranchProvided,
                StopAtRawFileOffsetExclusive: abovePlan.StopAtRawFileOffsetExclusive);
        }

        bool unusedAboveBranchProvided = capture.AboveBranch.HasValue;
        if (capture.SecondBranch is not FrameHandoffCapacitySecondBranchLifecycleCapture
            secondCapture)
        {
            return StopBeforeSelectedBranch(
                capture,
                firstPlan,
                route,
                selectedBranchCaptureProvided: false,
                unusedBranchProvided: unusedAboveBranchProvided,
                outcome: FrameHandoffCapacityDispatchLifecycleOutcome
                    .SelectedBranchCaptureMissing,
                stopAt: FrameHandoffCapacitySecondTransferProjection.EntryRawFileOffset);
        }

        FrameHandoffCapacityTransferReturnValues returnValues = GetReturnValues(firstPlan);
        FrameHandoffCapacitySecondBranchLifecycleCapture derivedSecondCapture =
            secondCapture with
            {
                SecondTransfer = secondCapture.SecondTransfer with
                {
                    Transfer = secondCapture.SecondTransfer.Transfer with
                    {
                        EntryEcx = returnValues.Receiver4,
                        EntryEdi = returnValues.FrameMinus8,
                        EntryEsi = returnValues.EntryEsi,
                        EntryEbp = returnValues.EntryEbp
                    }
                }
            };
        FrameHandoffCapacitySecondBranchLifecyclePlan secondPlan =
            FrameHandoffCapacitySecondBranchLifecycleProjection.Project(
                derivedSecondCapture);
        FrameHandoffCapacityDispatchLifecycleOutcome secondOutcome =
            MapSecondOutcome(secondPlan, unusedAboveBranchProvided);

        return new(
            capture,
            secondOutcome,
            firstPlan,
            route,
            null,
            null,
            derivedSecondCapture,
            secondPlan,
            FirstRouteAdmissionValidated: true,
            SelectedBranchCaptureProvided: true,
            UnselectedBranchCaptureProvided: unusedAboveBranchProvided,
            UnreachedAboveBranchCaptureProvided: unusedAboveBranchProvided,
            UnreachedSecondBranchCaptureProvided: false,
            StopAtRawFileOffsetExclusive: secondPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool TryAdmitRoute(
        FrameHandoffCapacityTransferLifecyclePlan plan,
        out FrameHandoffCapacityDispatchRoute route)
    {
        route = default;
        if (plan.Outcome != FrameHandoffCapacityTransferLifecycleOutcome.ProjectionComplete
            || !plan.TransferAdmissionChecked
            || !plan.TransferSuccessAdmission
            || !plan.TransferUnreachedCapturesClear
            || !plan.ReturnEntryIdentitiesValidated
            || !plan.ReturnEntryEbxMatches
            || !plan.ReturnEntryEbpMatches
            || !plan.ReturnBranchAdmitted
            || plan.UnreachedReturnReceiver4CaptureProvided
            || plan.UnreachedReturnHelperReturnEaxCaptureProvided
            || plan.UnreachedReturnFrameMinus8CaptureProvided
            || plan.ReturnPlan is not FrameHandoffCapacityTransferReturnPlan returnPlan
            || !returnPlan.Capture.ValidatedNormalEntry
            || !returnPlan.Receiver4ReadReached
            || !returnPlan.Receiver4Captured
            || !returnPlan.StackCleanupReached
            || !returnPlan.EdxLeaReached
            || returnPlan.EdxAfterLea is null
            || !returnPlan.FrameMinus8ReadReached
            || !returnPlan.FrameMinus8Captured
            || !returnPlan.FrameMinus10StoreReached
            || returnPlan.FrameMinus10StoreValue != returnPlan.EdxAfterLea
            || !returnPlan.CompareReached
            || returnPlan.FrameMinus8UnsignedGreaterThanEntryEsi is not bool above
            || returnPlan.BranchTaken != above)
        {
            return false;
        }

        if (returnPlan.Capture.FrameMinus8 is not uint frameMinus8
            || (frameMinus8 > returnPlan.Capture.EntryEsi) != above)
        {
            return false;
        }

        if (above
            && returnPlan.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedGreaterThanEntryEsi
            && returnPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityTransferReturnProjection
                    .AboveCapacityDestinationRawFileOffset
            && returnPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferReturnProjection
                    .AboveCapacityDestinationRawFileOffset)
        {
            route = FrameHandoffCapacityDispatchRoute.AboveCapacity;
            return true;
        }

        if (!above
            && returnPlan.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedLessThanOrEqualToEntryEsi
            && returnPlan.SelectedDestinationRawFileOffset
                == FrameHandoffCapacityTransferReturnProjection
                    .NormalBoundaryRawFileOffsetExclusive
            && returnPlan.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityTransferReturnProjection
                    .NormalBoundaryRawFileOffsetExclusive)
        {
            route = FrameHandoffCapacityDispatchRoute.AtOrBelowCapacity;
            return true;
        }

        return false;
    }

    private static FrameHandoffCapacityTransferReturnValues GetReturnValues(
        FrameHandoffCapacityTransferLifecyclePlan plan)
    {
        FrameHandoffCapacityTransferReturnPlan returnPlan = plan.ReturnPlan!.Value;
        uint receiver4 = FindReadValue(
            returnPlan,
            FrameHandoffCapacityTransferReturnProjection.Receiver4ReadRawFileOffset);
        uint frameMinus8 = FindReadValue(
            returnPlan,
            FrameHandoffCapacityTransferReturnProjection.FrameMinus8ReadRawFileOffset);
        return new(
            returnPlan.Capture.EntryEsi,
            returnPlan.Capture.EntryEbp,
            receiver4,
            frameMinus8,
            returnPlan.EdxAfterLea!.Value);
    }

    private static uint FindReadValue(
        FrameHandoffCapacityTransferReturnPlan plan,
        uint rawFileOffset)
    {
        foreach (FrameHandoffCapacityTransferReturnRead read in plan.Reads)
        {
            if (read.RawFileOffset == rawFileOffset && read.Value is uint value)
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            $"Admitted return plan is missing its read at 0x{rawFileOffset:X8}.");
    }

    private static FrameHandoffCapacityDispatchLifecycleOutcome MapFirstOutcome(
        FrameHandoffCapacityTransferLifecyclePlan plan)
    {
        if (plan.Outcome is
            FrameHandoffCapacityTransferLifecycleOutcome.TransferBlocked
            or FrameHandoffCapacityTransferLifecycleOutcome.TransferHasUnreachedCaptures)
        {
            return plan.Outcome
                    == FrameHandoffCapacityTransferLifecycleOutcome.TransferHasUnreachedCaptures
                || plan.UnreachedReturnReceiver4CaptureProvided
                || plan.UnreachedReturnHelperReturnEaxCaptureProvided
                || plan.UnreachedReturnFrameMinus8CaptureProvided
                ? FrameHandoffCapacityDispatchLifecycleOutcome.FirstTransferHasUnreachedCaptures
                : FrameHandoffCapacityDispatchLifecycleOutcome.FirstTransferBlocked;
        }

        if (plan.Outcome
            == FrameHandoffCapacityTransferLifecycleOutcome.ReturnEntryIdentityMismatch)
        {
            return FrameHandoffCapacityDispatchLifecycleOutcome
                .FirstReturnEntryIdentityMismatch;
        }

        if (plan.Outcome is
            FrameHandoffCapacityTransferLifecycleOutcome.ReturnBlocked
            or FrameHandoffCapacityTransferLifecycleOutcome.ReturnHasUnreachedCaptures)
        {
            return plan.Outcome
                    == FrameHandoffCapacityTransferLifecycleOutcome.ReturnHasUnreachedCaptures
                || plan.UnreachedReturnReceiver4CaptureProvided
                || plan.UnreachedReturnHelperReturnEaxCaptureProvided
                || plan.UnreachedReturnFrameMinus8CaptureProvided
                ? FrameHandoffCapacityDispatchLifecycleOutcome.FirstReturnHasUnreachedCaptures
                : FrameHandoffCapacityDispatchLifecycleOutcome.FirstReturnBlocked;
        }

        return FrameHandoffCapacityDispatchLifecycleOutcome.FirstRouteNotAdmitted;
    }

    private static FrameHandoffCapacityDispatchLifecycleOutcome MapAboveOutcome(
        FrameHandoffCapacityAboveBranchLifecyclePlan plan,
        bool unusedBranchProvided)
    {
        if (plan.Outcome is
            FrameHandoffCapacityAboveBranchLifecycleOutcome.AboveLifecycleHasUnreachedCaptures
            or FrameHandoffCapacityAboveBranchLifecycleOutcome
                .AboveFinalLifecycleHasUnreachedCaptures)
        {
            return FrameHandoffCapacityDispatchLifecycleOutcome.AboveBranchHasUnreachedCaptures;
        }

        if (plan.Outcome != FrameHandoffCapacityAboveBranchLifecycleOutcome.ProjectionComplete)
        {
            return FrameHandoffCapacityDispatchLifecycleOutcome.AboveBranchBlocked;
        }

        return unusedBranchProvided
            ? FrameHandoffCapacityDispatchLifecycleOutcome.UnselectedBranchCaptureProvided
            : FrameHandoffCapacityDispatchLifecycleOutcome.ProjectionComplete;
    }

    private static FrameHandoffCapacityDispatchLifecycleOutcome MapSecondOutcome(
        FrameHandoffCapacitySecondBranchLifecyclePlan plan,
        bool unusedBranchProvided)
    {
        if (plan.Outcome is
            FrameHandoffCapacitySecondBranchLifecycleOutcome.SecondTransferHasUnreachedCaptures
            or FrameHandoffCapacitySecondBranchLifecycleOutcome
                .SecondReturnHasUnreachedCaptures
            or FrameHandoffCapacitySecondBranchLifecycleOutcome.FirstFinalHasUnreachedCaptures)
        {
            return FrameHandoffCapacityDispatchLifecycleOutcome.SecondBranchHasUnreachedCaptures;
        }

        if (plan.Outcome != FrameHandoffCapacitySecondBranchLifecycleOutcome.ProjectionComplete)
        {
            return FrameHandoffCapacityDispatchLifecycleOutcome.SecondBranchBlocked;
        }

        return unusedBranchProvided
            ? FrameHandoffCapacityDispatchLifecycleOutcome.UnselectedBranchCaptureProvided
            : FrameHandoffCapacityDispatchLifecycleOutcome.ProjectionComplete;
    }

    private static FrameHandoffCapacityDispatchLifecyclePlan StopBeforeDispatch(
        FrameHandoffCapacityDispatchLifecycleCapture capture,
        FrameHandoffCapacityTransferLifecyclePlan firstPlan,
        FrameHandoffCapacityDispatchLifecycleOutcome outcome,
        uint stopAt) =>
        new(
            capture,
            outcome,
            firstPlan,
            SelectedRoute: null,
            DerivedAboveBranchCapture: null,
            AboveBranchPlan: null,
            DerivedSecondBranchCapture: null,
            SecondBranchPlan: null,
            FirstRouteAdmissionValidated: false,
            SelectedBranchCaptureProvided: false,
            UnselectedBranchCaptureProvided: false,
            UnreachedAboveBranchCaptureProvided: capture.AboveBranch.HasValue,
            UnreachedSecondBranchCaptureProvided: capture.SecondBranch.HasValue,
            StopAtRawFileOffsetExclusive: stopAt);

    private static FrameHandoffCapacityDispatchLifecyclePlan StopBeforeSelectedBranch(
        FrameHandoffCapacityDispatchLifecycleCapture capture,
        FrameHandoffCapacityTransferLifecyclePlan firstPlan,
        FrameHandoffCapacityDispatchRoute route,
        bool selectedBranchCaptureProvided,
        bool unusedBranchProvided,
        FrameHandoffCapacityDispatchLifecycleOutcome outcome,
        uint stopAt) =>
        new(
            capture,
            outcome,
            firstPlan,
            route,
            DerivedAboveBranchCapture: null,
            AboveBranchPlan: null,
            DerivedSecondBranchCapture: null,
            SecondBranchPlan: null,
            FirstRouteAdmissionValidated: true,
            selectedBranchCaptureProvided,
            unusedBranchProvided,
            UnreachedAboveBranchCaptureProvided:
                route == FrameHandoffCapacityDispatchRoute.AboveCapacity
                    ? false
                    : capture.AboveBranch.HasValue,
            UnreachedSecondBranchCaptureProvided:
                route == FrameHandoffCapacityDispatchRoute.AtOrBelowCapacity
                    ? false
                    : capture.SecondBranch.HasValue,
            StopAtRawFileOffsetExclusive: stopAt);

    private readonly record struct FrameHandoffCapacityTransferReturnValues(
        uint EntryEsi,
        uint EntryEbp,
        uint Receiver4,
        uint FrameMinus8,
        uint Edx);
}
