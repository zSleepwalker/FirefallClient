namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture(
    CapacityFinalCalleeDescriptorReloadCapture DescriptorCapture,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally,
    uint? PostCallEBP,
    uint? PostCallECX,
    uint? FreshFrameMinus14,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus8,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
{
    EntryNotValidated,
    MissingInitialFrameMinus10,
    MissingDescriptorFreshFrameMinus4,
    MissingDescriptorFreshFrameMinus10,
    DescriptorEntryEaxUnsignedBelowExit,
    MissingFreshEaxPlusC,
    DescriptorFinalCompareUnequalAtBoundary,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    MissingPostCallEBP,
    MissingPostCallECX,
    ContaminatedDescriptorRoute,
    ContaminatedCallRoute,
    ContaminatedFrameMaskRoute,
    MissingFreshFrameMinus14,
    MissingFreshFrameMinus4,
    FrameMismatch,
    MissingFreshFrameMinus8,
    MissingFreshGlobalValue,
    GlobalValueUpdated
}

public readonly record struct CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan(
    CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture Capture,
    CapacityFinalCalleeDescriptorReloadPlan DescriptorPlan,
    CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome Outcome,
    bool DescriptorEqualityDestinationReached,
    bool DescriptorBelowDestinationReached,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool OpaqueCallReached,
    bool? CallReturnedNormally,
    bool CallOutputsOpaque,
    uint? OpaqueCallPreCallEBP,
    uint? OpaqueCallPreCallECX,
    bool PostCallEBPCaptured,
    uint? PostCallEBP,
    bool PostCallECXCaptured,
    uint? PostCallECX,
    bool FrameMaskUpdateReached,
    uint? FrameMaskValidatedEntryEBP,
    uint? FrameMaskValidatedEntryECX,
    CapacityFinalCalleeFrameMaskUpdatePlan? FrameMaskPlan,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool UnreachedPostCallEBPCaptureProvided,
    bool UnreachedPostCallECXCaptureProvided,
    bool UnreachedFreshFrameMinus14CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the descriptor reload's clean equality routes directly to the frame
/// mask, and its clean EB8 routes through the opaque indirect call first.
/// </summary>
public static class CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection
{
    public const uint DescriptorEntryRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.EntryRawFileOffset;
    public const uint CallRawFileOffset =
        CapacityFinalCalleeDescriptorReloadProjection.BoundaryRawFileOffsetExclusive;
    public const uint FrameMaskEntryRawFileOffset =
        CapacityFinalCalleeFrameMaskUpdateProjection.EntryRawFileOffset;
    public const uint CallCellVirtualAddress = 0x01AC4A24;
    public const uint CallInstructionVirtualAddress = 0x016C9AB8;
    public const string CallInstructionBytes = "FF 15 24 4A AC 01";
    public const uint CallBoundaryRawFileOffsetExclusive = 0x012C8EBE;

    public static CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan Project(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture)
    {
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan =
            CapacityFinalCalleeDescriptorReloadProjection.Project(
                capture.DescriptorCapture);

        bool firstEqualityDestination =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && descriptorPlan.FirstBranchReached
            && descriptorPlan.FirstBranchTaken == true
            && descriptorPlan.FirstSelectedDestinationRawFileOffset
                == FrameMaskEntryRawFileOffset
            && descriptorPlan.StopAtRawFileOffsetExclusive
                == FrameMaskEntryRawFileOffset;
        bool finalEqualityDestination =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
            && descriptorPlan.FinalBranchTaken == true
            && descriptorPlan.FinalSelectedDestinationRawFileOffset
                == FrameMaskEntryRawFileOffset
            && descriptorPlan.StopAtRawFileOffsetExclusive
                == FrameMaskEntryRawFileOffset;
        bool equalityDestinationReached =
            firstEqualityDestination || finalEqualityDestination;

        bool belowDestinationReached =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit
            && descriptorPlan.SecondBranchReached
            && descriptorPlan.SecondBranchTaken == true
            && descriptorPlan.SecondSelectedDestinationRawFileOffset == CallRawFileOffset
            && descriptorPlan.StopAtRawFileOffsetExclusive == CallRawFileOffset;
        bool finalUnequalAtBoundary =
            descriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary
            && descriptorPlan.FinalBranchReached
            && descriptorPlan.FinalBranchTaken == false
            && descriptorPlan.FinalSelectedDestinationRawFileOffset is null
            && descriptorPlan.StopAtRawFileOffsetExclusive == CallRawFileOffset;

        if (!IsDescriptorRouteClean(descriptorPlan))
        {
            return BuildPlan(
                capture,
                descriptorPlan,
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedDescriptorRoute,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
        }

        if (!equalityDestinationReached && !belowDestinationReached
            && !finalUnequalAtBoundary)
        {
            bool callCaptureProvided = HasCallCapture(capture);
            bool maskCaptureProvided = HasMaskCapture(capture);
            return BuildPlan(
                capture,
                descriptorPlan,
                callCaptureProvided || maskCaptureProvided
                    ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .ContaminatedDescriptorRoute
                    : MapDescriptorOutcome(descriptorPlan.Outcome),
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                unreachedPostCallEBPCaptureProvided:
                    capture.PostCallEBP.HasValue,
                unreachedPostCallECXCaptureProvided:
                    capture.PostCallECX.HasValue,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: descriptorPlan.StopAtRawFileOffsetExclusive);
        }

        if (equalityDestinationReached)
        {
            bool bypassCallCaptureProvided = HasCallCapture(capture);
            if (bypassCallCaptureProvided)
            {
                return BuildPlan(
                    capture,
                    descriptorPlan,
                    CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .ContaminatedCallRoute,
                    descriptorEqualityDestinationReached: true,
                    unreachedCallCellTargetCaptureProvided:
                        capture.CallCellTargetVirtualAddress.HasValue,
                    unreachedCallReturnStatusProvided:
                        capture.CallReturnedNormally.HasValue,
                    unreachedPostCallEBPCaptureProvided:
                        capture.PostCallEBP.HasValue,
                    unreachedPostCallECXCaptureProvided:
                        capture.PostCallECX.HasValue,
                    unreachedFreshFrameMinus14CaptureProvided:
                        capture.FreshFrameMinus14.HasValue,
                    unreachedFreshFrameMinus4CaptureProvided:
                        capture.FreshFrameMinus4.HasValue,
                    unreachedFreshFrameMinus8CaptureProvided:
                        capture.FreshFrameMinus8.HasValue,
                    unreachedFreshGlobalValueCaptureProvided:
                        capture.FreshGlobalValue.HasValue,
                    stopAt: FrameMaskEntryRawFileOffset);
            }

            return ProjectFrameMask(
                capture,
                descriptorPlan,
                directEntryEBP: capture.DescriptorCapture.EntryEBP,
                directEntryECX: IsDirectShiftReached(capture)
                    ? descriptorPlan.InitialFrameMinus10
                    : null,
                equalityDestinationReached: true,
                callCellReadReached: false,
                callCellTargetCaptured: false,
                callCellTargetVirtualAddress: null,
                opaqueCallReached: false,
                callReturnedNormally: null,
                callOutputsOpaque: false,
                opaqueCallPreCallEBP: null,
                opaqueCallPreCallECX: null,
                postCallEBPCaptured: false,
                postCallEBP: null,
                postCallECXCaptured: false,
                postCallECX: null,
                unreachedCallCellTargetCaptureProvided:
                    capture.CallCellTargetVirtualAddress.HasValue,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                unreachedPostCallEBPCaptureProvided:
                    capture.PostCallEBP.HasValue,
                unreachedPostCallECXCaptureProvided:
                    capture.PostCallECX.HasValue);
        }

        return ProjectOpaqueCall(
            capture,
            descriptorPlan,
            belowDestinationReached);
    }

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan ProjectOpaqueCall(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture,
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan,
        bool belowDestinationReached)
    {
        uint? preCallEBP = capture.DescriptorCapture.EntryEBP;
        uint? preCallECX = descriptorPlan.InitialFrameMinus10;

        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            bool contaminated = capture.CallReturnedNormally.HasValue
                || capture.PostCallEBP.HasValue
                || capture.PostCallECX.HasValue
                || HasMaskCapture(capture);
            return BuildPlan(
                capture,
                descriptorPlan,
                contaminated
                    ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .ContaminatedCallRoute
                    : CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .MissingCallCellTarget,
                descriptorBelowDestinationReached: belowDestinationReached,
                callCellReadReached: true,
                opaqueCallPreCallEBP: preCallEBP,
                opaqueCallPreCallECX: preCallECX,
                unreachedCallReturnStatusProvided:
                    capture.CallReturnedNormally.HasValue,
                unreachedPostCallEBPCaptureProvided:
                    capture.PostCallEBP.HasValue,
                unreachedPostCallECXCaptureProvided:
                    capture.PostCallECX.HasValue,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            bool contaminated = capture.PostCallEBP.HasValue
                || capture.PostCallECX.HasValue
                || HasMaskCapture(capture);
            return BuildPlan(
                capture,
                descriptorPlan,
                contaminated
                    ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .ContaminatedCallRoute
                    : CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .MissingCallReturnStatus,
                descriptorBelowDestinationReached: belowDestinationReached,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                opaqueCallReached: true,
                callOutputsOpaque: true,
                opaqueCallPreCallEBP: preCallEBP,
                opaqueCallPreCallECX: preCallECX,
                unreachedPostCallEBPCaptureProvided:
                    capture.PostCallEBP.HasValue,
                unreachedPostCallECXCaptureProvided:
                    capture.PostCallECX.HasValue,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: CallRawFileOffset);
        }

        if (!callReturnedNormally)
        {
            bool contaminated = capture.PostCallEBP.HasValue
                || capture.PostCallECX.HasValue
                || HasMaskCapture(capture);
            return BuildPlan(
                capture,
                descriptorPlan,
                contaminated
                    ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .ContaminatedCallRoute
                    : CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .CallDidNotReturnNormally,
                descriptorBelowDestinationReached: belowDestinationReached,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                opaqueCallReached: true,
                callReturnedNormally: false,
                callOutputsOpaque: true,
                opaqueCallPreCallEBP: preCallEBP,
                opaqueCallPreCallECX: preCallECX,
                unreachedPostCallEBPCaptureProvided:
                    capture.PostCallEBP.HasValue,
                unreachedPostCallECXCaptureProvided:
                    capture.PostCallECX.HasValue,
                unreachedFreshFrameMinus14CaptureProvided:
                    capture.FreshFrameMinus14.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: CallRawFileOffset);
        }

        CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome? missingPostCallRegister =
            capture.PostCallEBP is null
                ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.MissingPostCallEBP
                : capture.PostCallECX is null
                    ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                        .MissingPostCallECX
                    : null;

        CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan frameMaskPlan =
            ProjectFrameMask(
                capture,
                descriptorPlan,
                directEntryEBP: capture.PostCallEBP,
                directEntryECX: capture.PostCallECX,
                equalityDestinationReached: false,
                descriptorBelowDestinationReached: belowDestinationReached,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                opaqueCallReached: true,
                callReturnedNormally: true,
                callOutputsOpaque: true,
                opaqueCallPreCallEBP: preCallEBP,
                opaqueCallPreCallECX: preCallECX,
                postCallEBPCaptured: capture.PostCallEBP.HasValue,
                postCallEBP: capture.PostCallEBP,
                postCallECXCaptured: capture.PostCallECX.HasValue,
                postCallECX: capture.PostCallECX,
                contamination: missingPostCallRegister);

        return frameMaskPlan;
    }

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan ProjectFrameMask(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture,
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan,
        uint? directEntryEBP,
        uint? directEntryECX,
        bool equalityDestinationReached,
        bool descriptorBelowDestinationReached = false,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool opaqueCallReached = false,
        bool? callReturnedNormally = null,
        bool callOutputsOpaque = false,
        uint? opaqueCallPreCallEBP = null,
        uint? opaqueCallPreCallECX = null,
        bool postCallEBPCaptured = false,
        uint? postCallEBP = null,
        bool postCallECXCaptured = false,
        uint? postCallECX = null,
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome? contamination = null,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        bool unreachedPostCallEBPCaptureProvided = false,
        bool unreachedPostCallECXCaptureProvided = false)
    {
        CapacityFinalCalleeFrameMaskUpdatePlan maskPlan =
            CapacityFinalCalleeFrameMaskUpdateProjection.Project(
                new CapacityFinalCalleeFrameMaskUpdateCapture(
                    ValidatedEntryEBP: directEntryEBP,
                    ValidatedEntryECX: directEntryECX,
                    FreshFrameMinus14: capture.FreshFrameMinus14,
                    FreshFrameMinus4: capture.FreshFrameMinus4,
                    FreshFrameMinus8: capture.FreshFrameMinus8,
                    FreshGlobalValue: capture.FreshGlobalValue));

        bool nestedMaskContamination =
            maskPlan.UnreachedEntryECXCaptureProvided
            || maskPlan.UnreachedFreshFrameMinus14CaptureProvided
            || maskPlan.UnreachedFreshFrameMinus4CaptureProvided
            || maskPlan.UnreachedFreshFrameMinus8CaptureProvided
            || maskPlan.UnreachedFreshGlobalValueCaptureProvided;
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome outcome =
            contamination
            ?? (nestedMaskContamination
                ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute
                : MapFrameMaskPlanOutcome(maskPlan));

        return BuildPlan(
            capture,
            descriptorPlan,
            outcome,
            descriptorEqualityDestinationReached: equalityDestinationReached,
            descriptorBelowDestinationReached: descriptorBelowDestinationReached,
            callCellReadReached: callCellReadReached,
            callCellTargetCaptured: callCellTargetCaptured,
            callCellTargetVirtualAddress: callCellTargetVirtualAddress,
            opaqueCallReached: opaqueCallReached,
            callReturnedNormally: callReturnedNormally,
            callOutputsOpaque: callOutputsOpaque,
            opaqueCallPreCallEBP: opaqueCallPreCallEBP,
            opaqueCallPreCallECX: opaqueCallPreCallECX,
            postCallEBPCaptured: postCallEBPCaptured,
            postCallEBP: postCallEBP,
            postCallECXCaptured: postCallECXCaptured,
            postCallECX: postCallECX,
            frameMaskUpdateReached: true,
            frameMaskValidatedEntryEBP: directEntryEBP,
            frameMaskValidatedEntryECX: directEntryECX,
            frameMaskPlan: maskPlan,
            unreachedCallCellTargetCaptureProvided:
                unreachedCallCellTargetCaptureProvided,
            unreachedCallReturnStatusProvided:
                unreachedCallReturnStatusProvided,
            unreachedPostCallEBPCaptureProvided:
                unreachedPostCallEBPCaptureProvided,
            unreachedPostCallECXCaptureProvided:
                unreachedPostCallECXCaptureProvided,
            unreachedFreshFrameMinus14CaptureProvided:
                maskPlan.UnreachedFreshFrameMinus14CaptureProvided,
            unreachedFreshFrameMinus4CaptureProvided:
                maskPlan.UnreachedFreshFrameMinus4CaptureProvided,
            unreachedFreshFrameMinus8CaptureProvided:
                maskPlan.UnreachedFreshFrameMinus8CaptureProvided,
            unreachedFreshGlobalValueCaptureProvided:
                maskPlan.UnreachedFreshGlobalValueCaptureProvided,
            stopAt: maskPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsDescriptorRouteClean(
        CapacityFinalCalleeDescriptorReloadPlan plan) =>
        !plan.UnreachedInitialFrameMinus10CaptureProvided
        && !plan.UnreachedFreshFrameMinus4CaptureProvided
        && !plan.UnreachedFreshFrameMinus10CaptureProvided
        && !plan.UnreachedFreshEaxPlusCCaptureProvided;

    private static bool IsDirectShiftReached(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture) =>
        capture.FreshFrameMinus14 is uint frameMinus14
        && capture.FreshFrameMinus4 is uint frameMinus4
        && frameMinus14 == frameMinus4
        && capture.FreshFrameMinus8.HasValue;

    private static bool HasCallCapture(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture) =>
        capture.CallCellTargetVirtualAddress.HasValue
        || capture.CallReturnedNormally.HasValue
        || capture.PostCallEBP.HasValue
        || capture.PostCallECX.HasValue;

    private static bool HasMaskCapture(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture) =>
        capture.FreshFrameMinus14.HasValue
        || capture.FreshFrameMinus4.HasValue
        || capture.FreshFrameMinus8.HasValue
        || capture.FreshGlobalValue.HasValue;

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome MapDescriptorOutcome(
        CapacityFinalCalleeDescriptorReloadOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeDescriptorReloadOutcome.EntryNotValidated =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.EntryNotValidated,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingInitialFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingDescriptorFreshFrameMinus4,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus10 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingDescriptorFreshFrameMinus10,
            CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .DescriptorEntryEaxUnsignedBelowExit,
            CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshEaxPlusC =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingFreshEaxPlusC,
            CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .DescriptorFinalCompareUnequalAtBoundary,
            CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
                or CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedDescriptorRoute,
            _ => throw new InvalidOperationException(
                $"Unexpected descriptor-reload outcome: {outcome}.")
        };

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
        MapFrameMaskPlanOutcome(CapacityFinalCalleeFrameMaskUpdatePlan plan)
    {
        if (plan.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch)
        {
            return plan.BranchReached
                && plan.BranchTaken == true
                && plan.CompareReached
                && plan.Equal == false
                && plan.FreshFrameMinus14Captured
                && plan.FreshFrameMinus4Captured
                && plan.SelectedBranchDestinationRawFileOffset
                    == CapacityFinalCalleeFrameMaskUpdateProjection
                        .MismatchDestinationRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeFrameMaskUpdateProjection
                        .MismatchDestinationRawFileOffset
                ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.FrameMismatch
                : CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute;
        }

        if (plan.Outcome == CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated)
        {
            return plan.BranchReached
                && plan.BranchTaken == false
                && plan.CompareReached
                && plan.Equal == true
                && plan.FreshFrameMinus14Captured
                && plan.FreshFrameMinus4Captured
                && plan.FreshFrameMinus8Captured
                && plan.SelectedBranchDestinationRawFileOffset is null
                && plan.ShiftReached
                && plan.GlobalWriteReached
                && plan.GlobalValueCaptured
                && plan.GlobalValueAfter.HasValue
                && plan.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeFrameMaskUpdateProjection
                        .ContinuationRawFileOffset
                ? CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.GlobalValueUpdated
                : CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryEBP =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.MissingPostCallEBP,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingValidatedEntryECX =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome.MissingPostCallECX,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus14 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingFreshFrameMinus14,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus4 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingFreshFrameMinus4,
            CapacityFinalCalleeFrameMaskUpdateOutcome.FrameMismatch =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshFrameMinus8 =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingFreshFrameMinus8,
            CapacityFinalCalleeFrameMaskUpdateOutcome.MissingFreshGlobalValue =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .MissingFreshGlobalValue,
            CapacityFinalCalleeFrameMaskUpdateOutcome.GlobalValueUpdated =>
                CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome
                    .ContaminatedFrameMaskRoute,
            _ => throw new InvalidOperationException(
                $"Unexpected frame-mask outcome: {plan.Outcome}.")
        };
    }

    private static CapacityFinalCalleeDescriptorFallbackMaskLifecyclePlan BuildPlan(
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleCapture capture,
        CapacityFinalCalleeDescriptorReloadPlan descriptorPlan,
        CapacityFinalCalleeDescriptorFallbackMaskLifecycleOutcome outcome,
        bool descriptorEqualityDestinationReached = false,
        bool descriptorBelowDestinationReached = false,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool opaqueCallReached = false,
        bool? callReturnedNormally = null,
        bool callOutputsOpaque = false,
        uint? opaqueCallPreCallEBP = null,
        uint? opaqueCallPreCallECX = null,
        bool postCallEBPCaptured = false,
        uint? postCallEBP = null,
        bool postCallECXCaptured = false,
        uint? postCallECX = null,
        bool frameMaskUpdateReached = false,
        uint? frameMaskValidatedEntryEBP = null,
        uint? frameMaskValidatedEntryECX = null,
        CapacityFinalCalleeFrameMaskUpdatePlan? frameMaskPlan = null,
        bool unreachedCallCellTargetCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        bool unreachedPostCallEBPCaptureProvided = false,
        bool unreachedPostCallECXCaptureProvided = false,
        bool unreachedFreshFrameMinus14CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshFrameMinus8CaptureProvided = false,
        bool unreachedFreshGlobalValueCaptureProvided = false,
        uint? stopAt = null) =>
        new(
            capture,
            descriptorPlan,
            outcome,
            descriptorEqualityDestinationReached,
            descriptorBelowDestinationReached,
            callCellReadReached,
            callCellTargetCaptured,
            callCellTargetCaptured ? callCellTargetVirtualAddress : null,
            opaqueCallReached,
            opaqueCallReached ? callReturnedNormally : null,
            callOutputsOpaque,
            opaqueCallReached ? opaqueCallPreCallEBP : null,
            opaqueCallReached ? opaqueCallPreCallECX : null,
            opaqueCallReached && postCallEBPCaptured,
            opaqueCallReached && postCallEBPCaptured ? postCallEBP : null,
            opaqueCallReached && postCallECXCaptured,
            opaqueCallReached && postCallECXCaptured ? postCallECX : null,
            frameMaskUpdateReached,
            frameMaskUpdateReached ? frameMaskValidatedEntryEBP : null,
            frameMaskUpdateReached ? frameMaskValidatedEntryECX : null,
            frameMaskUpdateReached ? frameMaskPlan : null,
            (!callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue)
                || unreachedCallCellTargetCaptureProvided,
            (!opaqueCallReached && capture.CallReturnedNormally.HasValue)
                || unreachedCallReturnStatusProvided,
            (!opaqueCallReached || callReturnedNormally != true)
                && capture.PostCallEBP.HasValue
                || unreachedPostCallEBPCaptureProvided,
            (!opaqueCallReached || callReturnedNormally != true)
                && capture.PostCallECX.HasValue
                || unreachedPostCallECXCaptureProvided,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus14CaptureProvided
                : unreachedFreshFrameMinus14CaptureProvided
                    || capture.FreshFrameMinus14.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus4CaptureProvided
                : unreachedFreshFrameMinus4CaptureProvided
                    || capture.FreshFrameMinus4.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshFrameMinus8CaptureProvided
                : unreachedFreshFrameMinus8CaptureProvided
                    || capture.FreshFrameMinus8.HasValue,
            frameMaskUpdateReached
                ? unreachedFreshGlobalValueCaptureProvided
                : unreachedFreshGlobalValueCaptureProvided
                    || capture.FreshGlobalValue.HasValue,
            stopAt ?? descriptorPlan.StopAtRawFileOffsetExclusive);
}
