namespace Firefall.Reconstruction.Startup;

public readonly record struct HighCountObjectHelperLifecycleCapture(
    HighCountObjectHelperEntryCapture Entry,
    uint? DwordAtFirstLoadedBase,
    HighCountObjectHelperReturnCapture Return);

public enum HighCountObjectHelperLifecycleOutcome
{
    EntryProjectionStopped,
    UnreachedBridgeCaptureProvided,
    MissingDwordAtFirstLoadedBase,
    ReturnEntryIdentityMismatch,
    ReturnProjectionStopped,
    Returned
}

public readonly record struct HighCountObjectHelperBridgeReadProjection(
    bool ReadSiteReached,
    uint? Address,
    bool CaptureProvided,
    bool MissingCapture,
    bool UnreachedCaptureProvided,
    uint? Value);

public readonly record struct HighCountObjectHelperLifecyclePlan(
    HighCountObjectHelperLifecycleCapture Capture,
    HighCountObjectHelperEntryPlan EntryPlan,
    HighCountObjectHelperLifecycleOutcome Outcome,
    HighCountObjectHelperBridgeReadProjection DwordAtFirstLoadedBase,
    uint? RetainedEsi,
    HighCountObjectHelperReturnCapture ConstructedReturnCapture,
    bool ReturnEntryEaxIdentityChecked,
    bool? ReturnEntryEaxIdentityMatches,
    bool ReturnEntryEsiIdentityChecked,
    bool? ReturnEntryEsiIdentityMatches,
    HighCountObjectHelperReturnPlan? ReturnPlan,
    bool UnreachedReturnEntryEaxCaptureProvided,
    bool UnreachedReturnEntryEsiCaptureProvided,
    bool UnreachedDwordAtEntryEaxPlus8CaptureProvided,
    bool UnreachedDwordAtEntryEsiPlus8CaptureProvided,
    bool UnreachedDwordAtEntryEsiPlus4CaptureProvided,
    bool UnreachedDwordAtComputedSlotCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool OwnershipInferred => false;
}

/// <summary>
/// Joins the entry zero-test, its nonzero-only ESI reload, and the shared return tail.
/// </summary>
public static class HighCountObjectHelperLifecycleProjection
{
    public const uint BridgeReadRawFileOffset = 0x00BE6456;
    public const uint BridgeReadVirtualAddress = 0x00FE7056;
    public const string BridgeReadBytes = "8B 36";

    public static HighCountObjectHelperLifecyclePlan Project(
        HighCountObjectHelperLifecycleCapture capture)
    {
        HighCountObjectHelperEntryPlan entryPlan =
            HighCountObjectHelperEntryProjection.Project(capture.Entry);

        bool entrySelectedRoute =
            entryPlan.Outcome
                is HighCountObjectHelperEntryOutcome.ZeroLoadedDwordBranchesToBoundary
                or HighCountObjectHelperEntryOutcome.NonzeroLoadedDwordFallsThrough;
        bool entryCapturesClear =
            !entryPlan.UnreachedHelperReturnEaxCaptureProvided
            && !entryPlan.UnreachedDwordAtHelperEaxCaptureProvided;

        bool entryEaxAvailable = entryPlan.HelperReturnEaxCaptured;
        uint? entryEax = entryEaxAvailable ? entryPlan.HelperReturnEax : null;
        bool zeroRoute = entryPlan.Outcome
            == HighCountObjectHelperEntryOutcome.ZeroLoadedDwordBranchesToBoundary;
        bool nonzeroRoute = entryPlan.Outcome
            == HighCountObjectHelperEntryOutcome.NonzeroLoadedDwordFallsThrough;

        HighCountObjectHelperBridgeReadProjection bridgeRead;
        uint? retainedEsi;
        if (!entrySelectedRoute || !entryCapturesClear)
        {
            bridgeRead = UnreachedBridgeRead(capture.DwordAtFirstLoadedBase);
            retainedEsi = null;
        }
        else if (zeroRoute)
        {
            bridgeRead = UnreachedBridgeRead(capture.DwordAtFirstLoadedBase);
            retainedEsi = 0;
        }
        else if (nonzeroRoute
            && entryPlan.DwordAtHelperEax is uint firstLoadedDword)
        {
            bridgeRead = ReachedBridgeRead(
                firstLoadedDword,
                capture.DwordAtFirstLoadedBase);
            retainedEsi = capture.DwordAtFirstLoadedBase;
        }
        else
        {
            bridgeRead = UnreachedBridgeRead(capture.DwordAtFirstLoadedBase);
            retainedEsi = null;
        }

        bool entryRouteReady = entrySelectedRoute && entryCapturesClear;
        bool bridgeReady = zeroRoute
            ? !bridgeRead.CaptureProvided
            : nonzeroRoute && bridgeRead.ReadSiteReached
                && !bridgeRead.MissingCapture;
        bool returnEntryIdentityInputsReached = entryRouteReady && bridgeReady;
        bool returnEntryEaxIdentityChecked =
            returnEntryIdentityInputsReached
            && entryEax.HasValue
            && capture.Return.EntryEax.HasValue;
        bool? returnEntryEaxIdentityMatches = returnEntryEaxIdentityChecked
            ? capture.Return.EntryEax == entryEax
            : null;
        bool returnEntryEsiIdentityChecked =
            returnEntryIdentityInputsReached
            && retainedEsi.HasValue
            && capture.Return.EntryEsi.HasValue;
        bool? returnEntryEsiIdentityMatches = returnEntryEsiIdentityChecked
            ? capture.Return.EntryEsi == retainedEsi
            : null;

        bool returnIdentitiesMatch =
            returnEntryEaxIdentityMatches != false
            && returnEntryEsiIdentityMatches != false;

        HighCountObjectHelperReturnCapture constructedReturnCapture = new(
            entryEax,
            retainedEsi,
            capture.Return.DwordAtEntryEaxPlus8,
            capture.Return.DwordAtEntryEsiPlus8,
            capture.Return.DwordAtEntryEsiPlus4,
            capture.Return.DwordAtComputedSlot);

        bool returnProjectionAllowed =
            entryRouteReady
            && bridgeReady
            && entryEax.HasValue
            && retainedEsi.HasValue
            && returnIdentitiesMatch;

        HighCountObjectHelperReturnPlan? returnPlan = returnProjectionAllowed
            ? HighCountObjectHelperReturnProjection.Project(constructedReturnCapture)
            : null;

        bool tailCapturesUnreached = returnPlan is null;
        bool unreachedDwordAtEntryEaxPlus8 =
            tailCapturesUnreached
                ? capture.Return.DwordAtEntryEaxPlus8.HasValue
                : returnPlan!.Value.DwordAtEntryEaxPlus8.UnreachedCaptureProvided;
        bool unreachedDwordAtEntryEsiPlus8 =
            tailCapturesUnreached
                ? capture.Return.DwordAtEntryEsiPlus8.HasValue
                : returnPlan!.Value.DwordAtEntryEsiPlus8.UnreachedCaptureProvided;
        bool unreachedDwordAtEntryEsiPlus4 =
            tailCapturesUnreached
                ? capture.Return.DwordAtEntryEsiPlus4.HasValue
                : returnPlan!.Value.DwordAtEntryEsiPlus4.UnreachedCaptureProvided;
        bool unreachedDwordAtComputedSlot =
            tailCapturesUnreached
                ? capture.Return.DwordAtComputedSlot.HasValue
                : returnPlan!.Value.DwordAtComputedSlot.UnreachedCaptureProvided;

        HighCountObjectHelperLifecycleOutcome outcome;
        uint stopAt;
        if (!entrySelectedRoute || !entryCapturesClear)
        {
            outcome = HighCountObjectHelperLifecycleOutcome.EntryProjectionStopped;
            stopAt = entryPlan.StopAtRawFileOffsetExclusive;
        }
        else if (zeroRoute && bridgeRead.UnreachedCaptureProvided)
        {
            outcome =
                HighCountObjectHelperLifecycleOutcome.UnreachedBridgeCaptureProvided;
            stopAt = HighCountObjectHelperReturnProjection.EntryRawFileOffset;
        }
        else if (nonzeroRoute && bridgeRead.MissingCapture)
        {
            outcome = HighCountObjectHelperLifecycleOutcome.MissingDwordAtFirstLoadedBase;
            stopAt = BridgeReadRawFileOffset;
        }
        else if (!returnIdentitiesMatch)
        {
            outcome = HighCountObjectHelperLifecycleOutcome.ReturnEntryIdentityMismatch;
            stopAt = HighCountObjectHelperReturnProjection.EntryRawFileOffset;
        }
        else if (returnPlan is HighCountObjectHelperReturnPlan projectedReturn
            && projectedReturn.Outcome == HighCountObjectHelperReturnOutcome.Returned)
        {
            outcome = HighCountObjectHelperLifecycleOutcome.Returned;
            stopAt = projectedReturn.StopAtRawFileOffsetExclusive;
        }
        else
        {
            outcome = HighCountObjectHelperLifecycleOutcome.ReturnProjectionStopped;
            stopAt = returnPlan?.StopAtRawFileOffsetExclusive
                ?? HighCountObjectHelperReturnProjection.EntryRawFileOffset;
        }

        return new(
            capture,
            entryPlan,
            outcome,
            bridgeRead,
            retainedEsi,
            constructedReturnCapture,
            returnEntryEaxIdentityChecked,
            returnEntryEaxIdentityMatches,
            returnEntryEsiIdentityChecked,
            returnEntryEsiIdentityMatches,
            returnPlan,
            !returnEntryIdentityInputsReached && capture.Return.EntryEax.HasValue,
            !returnEntryIdentityInputsReached && capture.Return.EntryEsi.HasValue,
            unreachedDwordAtEntryEaxPlus8,
            unreachedDwordAtEntryEsiPlus8,
            unreachedDwordAtEntryEsiPlus4,
            unreachedDwordAtComputedSlot,
            stopAt);
    }

    private static HighCountObjectHelperBridgeReadProjection ReachedBridgeRead(
        uint address,
        uint? capture) =>
        new(
            ReadSiteReached: true,
            Address: address,
            CaptureProvided: capture.HasValue,
            MissingCapture: !capture.HasValue,
            UnreachedCaptureProvided: false,
            Value: capture);

    private static HighCountObjectHelperBridgeReadProjection UnreachedBridgeRead(
        uint? capture) =>
        new(
            ReadSiteReached: false,
            Address: null,
            CaptureProvided: capture.HasValue,
            MissingCapture: false,
            UnreachedCaptureProvided: capture.HasValue,
            Value: null);
}
