using Firefall.Reconstruction.Startup;

internal static class HighCountObjectHelperLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The nonzero bridge is the exact 8B 36 read at raw BE6456",
            HighCountObjectHelperLifecycleProjection.BridgeReadRawFileOffset
                == HighCountObjectHelperEntryProjection.NonzeroFallthroughRawFileOffset
            && HighCountObjectHelperLifecycleProjection.BridgeReadVirtualAddress
                == HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                    HighCountObjectHelperLifecycleProjection.BridgeReadRawFileOffset)
            && HighCountObjectHelperLifecycleProjection.BridgeReadVirtualAddress
                == 0x00FE7056
            && string.Concat(
                HighCountObjectHelperLifecycleProjection.BridgeReadBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B36"
            && HighCountObjectHelperEntryProjection.CapturedWindowBytes.EndsWith(
                "8B 36",
                StringComparison.Ordinal));

        HighCountObjectHelperLifecyclePlan zeroRoute =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x44, true, 0x2000, 0),
                    null,
                    new(null, null, 2, 3, 0x3000, 0x4000)));
        check("A zero first DWORD takes BE6458, supplies retained ESI zero, and leaves the bridge absent",
            zeroRoute.Outcome == HighCountObjectHelperLifecycleOutcome.Returned
            && zeroRoute.EntryPlan.Outcome
                == HighCountObjectHelperEntryOutcome.ZeroLoadedDwordBranchesToBoundary
            && zeroRoute.EntryPlan.NextRawFileOffset
                == HighCountObjectHelperReturnProjection.EntryRawFileOffset
            && !zeroRoute.DwordAtFirstLoadedBase.ReadSiteReached
            && !zeroRoute.DwordAtFirstLoadedBase.CaptureProvided
            && !zeroRoute.DwordAtFirstLoadedBase.UnreachedCaptureProvided
            && zeroRoute.RetainedEsi == 0
            && zeroRoute.ConstructedReturnCapture.EntryEax == 0x2000
            && zeroRoute.ConstructedReturnCapture.EntryEsi == 0
            && zeroRoute.ReturnPlan?.Outcome
                == HighCountObjectHelperReturnOutcome.Returned
            && zeroRoute.ReturnPlan?.ReturnEax == 0x4008
            && !zeroRoute.OwnershipInferred);

        HighCountObjectHelperLifecyclePlan zeroRouteWithBridgeCapture =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x45, true, 0x2000, 0),
                    0xDEAD,
                    new(0x2000, 0, 2, 3, 0x3000, 0x4000)));
        check("A supplied nonzero-only bridge capture blocks a zero route before all return-tail reads",
            zeroRouteWithBridgeCapture.Outcome
                == HighCountObjectHelperLifecycleOutcome.UnreachedBridgeCaptureProvided
            && zeroRouteWithBridgeCapture.EntryPlan.NextRawFileOffset
                == HighCountObjectHelperReturnProjection.EntryRawFileOffset
            && zeroRouteWithBridgeCapture.DwordAtFirstLoadedBase.UnreachedCaptureProvided
            && zeroRouteWithBridgeCapture.RetainedEsi == 0
            && zeroRouteWithBridgeCapture.ConstructedReturnCapture.EntryEax == 0x2000
            && zeroRouteWithBridgeCapture.ConstructedReturnCapture.EntryEsi == 0
            && !zeroRouteWithBridgeCapture.ReturnEntryEaxIdentityChecked
            && !zeroRouteWithBridgeCapture.ReturnEntryEsiIdentityChecked
            && zeroRouteWithBridgeCapture.UnreachedReturnEntryEaxCaptureProvided
            && zeroRouteWithBridgeCapture.UnreachedReturnEntryEsiCaptureProvided
            && zeroRouteWithBridgeCapture.ReturnPlan is null
            && zeroRouteWithBridgeCapture.UnreachedDwordAtEntryEaxPlus8CaptureProvided
            && zeroRouteWithBridgeCapture.UnreachedDwordAtEntryEsiPlus8CaptureProvided
            && zeroRouteWithBridgeCapture.UnreachedDwordAtEntryEsiPlus4CaptureProvided
            && zeroRouteWithBridgeCapture.UnreachedDwordAtComputedSlotCaptureProvided
            && zeroRouteWithBridgeCapture.StopAtRawFileOffsetExclusive
                == HighCountObjectHelperReturnProjection.EntryRawFileOffset);

        HighCountObjectHelperLifecyclePlan nonzeroRoute =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x55, true, 0x1234, 0x2000),
                    0x500,
                    new(0x1234, 0x500, 1, 2, 0x700, 0x900)));
        check("A nonzero first DWORD independently reads the bridge and retains its zero-or-nonzero result",
            nonzeroRoute.Outcome == HighCountObjectHelperLifecycleOutcome.Returned
            && nonzeroRoute.EntryPlan.Outcome
                == HighCountObjectHelperEntryOutcome.NonzeroLoadedDwordFallsThrough
            && nonzeroRoute.EntryPlan.NextRawFileOffset
                == HighCountObjectHelperLifecycleProjection.BridgeReadRawFileOffset
            && nonzeroRoute.DwordAtFirstLoadedBase.ReadSiteReached
            && nonzeroRoute.DwordAtFirstLoadedBase.Address == 0x2000
            && nonzeroRoute.DwordAtFirstLoadedBase.Value == 0x500
            && nonzeroRoute.RetainedEsi == 0x500
            && nonzeroRoute.ConstructedReturnCapture.EntryEax == 0x1234
            && nonzeroRoute.ConstructedReturnCapture.EntryEsi == 0x500
            && nonzeroRoute.ReturnPlan?.ReturnEax == 0x904
            && nonzeroRoute.RetainedEsi != nonzeroRoute.DwordAtFirstLoadedBase.Address
            && !nonzeroRoute.OwnershipInferred);

        HighCountObjectHelperLifecyclePlan missingFirstRead =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x66, true, 0x1234, null),
                    0x500,
                    new(0x1234, 0x500, 1, 2, 3, 4)));
        check("A missing entry first read preserves its entry plan and rejects bridge and tail captures as unreached",
            missingFirstRead.Outcome
                == HighCountObjectHelperLifecycleOutcome.EntryProjectionStopped
            && missingFirstRead.EntryPlan.Outcome
                == HighCountObjectHelperEntryOutcome.MissingDwordAtHelperEax
            && missingFirstRead.EntryPlan.DwordReadReached
            && missingFirstRead.EntryPlan.DwordReadAddress == 0x1234
            && !missingFirstRead.EntryPlan.DwordAtHelperEaxCaptured
            && !missingFirstRead.DwordAtFirstLoadedBase.ReadSiteReached
            && missingFirstRead.DwordAtFirstLoadedBase.UnreachedCaptureProvided
            && missingFirstRead.ReturnPlan is null
            && !missingFirstRead.ReturnEntryEaxIdentityChecked
            && !missingFirstRead.ReturnEntryEsiIdentityChecked
            && missingFirstRead.UnreachedReturnEntryEaxCaptureProvided
            && missingFirstRead.UnreachedReturnEntryEsiCaptureProvided
            && missingFirstRead.UnreachedDwordAtEntryEaxPlus8CaptureProvided
            && missingFirstRead.UnreachedDwordAtEntryEsiPlus8CaptureProvided
            && missingFirstRead.UnreachedDwordAtEntryEsiPlus4CaptureProvided
            && missingFirstRead.UnreachedDwordAtComputedSlotCaptureProvided
            && missingFirstRead.StopAtRawFileOffsetExclusive
                == HighCountObjectHelperEntryProjection.HelperReturnDwordReadRawFileOffset);

        HighCountObjectHelperLifecyclePlan missingBridge =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x77, true, 0x1234, 0x2000),
                    null,
                    new(0x1234, 0, 1, 2, 3, 4)));
        check("A missing nonzero bridge read keeps the first read and blocks every return-tail read",
            missingBridge.Outcome
                == HighCountObjectHelperLifecycleOutcome.MissingDwordAtFirstLoadedBase
            && missingBridge.EntryPlan.DwordAtHelperEax == 0x2000
            && missingBridge.DwordAtFirstLoadedBase.ReadSiteReached
            && missingBridge.DwordAtFirstLoadedBase.Address == 0x2000
            && missingBridge.DwordAtFirstLoadedBase.MissingCapture
            && missingBridge.ReturnPlan is null
            && !missingBridge.ReturnEntryEaxIdentityChecked
            && !missingBridge.ReturnEntryEsiIdentityChecked
            && missingBridge.UnreachedReturnEntryEaxCaptureProvided
            && missingBridge.UnreachedReturnEntryEsiCaptureProvided
            && missingBridge.UnreachedDwordAtEntryEaxPlus8CaptureProvided
            && missingBridge.UnreachedDwordAtEntryEsiPlus8CaptureProvided
            && missingBridge.UnreachedDwordAtEntryEsiPlus4CaptureProvided
            && missingBridge.UnreachedDwordAtComputedSlotCaptureProvided
            && missingBridge.StopAtRawFileOffsetExclusive
                == HighCountObjectHelperLifecycleProjection.BridgeReadRawFileOffset);

        HighCountObjectHelperLifecyclePlan missingTail =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x88, true, 0x1234, 0x2000),
                    0,
                    new(null, null, null, 2, 3, 4)));
        check("A missing return-tail read is delegated to the return projection without losing its reached address",
            missingTail.Outcome
                == HighCountObjectHelperLifecycleOutcome.ReturnProjectionStopped
            && missingTail.RetainedEsi == 0
            && missingTail.ConstructedReturnCapture.EntryEax == 0x1234
            && missingTail.ConstructedReturnCapture.EntryEsi == 0
            && missingTail.ReturnPlan?.Outcome
                == HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEaxPlus8
            && missingTail.ReturnPlan?.DwordAtEntryEaxPlus8.ReadSiteReached == true
            && missingTail.ReturnPlan?.DwordAtEntryEaxPlus8.Address == 0x123C
            && missingTail.ReturnPlan?.DwordAtEntryEaxPlus8.MissingCapture == true
            && missingTail.ReturnPlan?.DwordAtEntryEsiPlus8.UnreachedCaptureProvided == true);

        HighCountObjectHelperLifecyclePlan missingHelperStatus =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0x99, null, 0x1234, 0x2000),
                    0x3000,
                    new(0x1234, 0x3000, 1, 2, 3, 4)));
        HighCountObjectHelperLifecyclePlan nonreturningHelper =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0xAA, false, 0x1234, 0x2000),
                    0x3000,
                    new(0x1234, 0x3000, 1, 2, 3, 4)));
        check("Missing or nonreturning calls at BE644B block bridge and tail reads while exposing supplied captures",
            missingHelperStatus.EntryPlan.Outcome
                == HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
            && missingHelperStatus.Outcome
                == HighCountObjectHelperLifecycleOutcome.EntryProjectionStopped
            && !missingHelperStatus.DwordAtFirstLoadedBase.ReadSiteReached
            && missingHelperStatus.DwordAtFirstLoadedBase.UnreachedCaptureProvided
            && missingHelperStatus.ReturnPlan is null
            && missingHelperStatus.UnreachedDwordAtEntryEaxPlus8CaptureProvided
            && missingHelperStatus.UnreachedDwordAtEntryEsiPlus8CaptureProvided
            && missingHelperStatus.UnreachedDwordAtEntryEsiPlus4CaptureProvided
            && missingHelperStatus.UnreachedDwordAtComputedSlotCaptureProvided
            && missingHelperStatus.StopAtRawFileOffsetExclusive
                == HighCountObjectHelperEntryProjection.HelperCallRawFileOffset
            && nonreturningHelper.EntryPlan.Outcome
                == HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
            && nonreturningHelper.EntryPlan.HelperReturnedNormally == false
            && !nonreturningHelper.DwordAtFirstLoadedBase.ReadSiteReached
            && nonreturningHelper.DwordAtFirstLoadedBase.UnreachedCaptureProvided
            && nonreturningHelper.ReturnPlan is null
            && nonreturningHelper.UnreachedDwordAtComputedSlotCaptureProvided);

        HighCountObjectHelperLifecyclePlan wrongReturnEax =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0xBB, true, 0x1234, 0),
                    null,
                    new(0x9999, null, 1, 2, 3, 4)));
        HighCountObjectHelperLifecyclePlan wrongReturnEsi =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0xCC, true, 0x1234, 1),
                    0x500,
                    new(0x1234, 0x501, 1, 2, 3, 4)));
        check("User-supplied return identities are validated and cannot redirect the composed return",
            wrongReturnEax.Outcome
                == HighCountObjectHelperLifecycleOutcome.ReturnEntryIdentityMismatch
            && wrongReturnEax.ReturnEntryEaxIdentityChecked
            && wrongReturnEax.ReturnEntryEaxIdentityMatches == false
            && wrongReturnEax.ConstructedReturnCapture.EntryEax == 0x1234
            && wrongReturnEax.ConstructedReturnCapture.EntryEsi == 0
            && wrongReturnEax.ReturnPlan is null
            && wrongReturnEax.UnreachedDwordAtEntryEaxPlus8CaptureProvided
            && wrongReturnEsi.Outcome
                == HighCountObjectHelperLifecycleOutcome.ReturnEntryIdentityMismatch
            && wrongReturnEsi.ReturnEntryEsiIdentityChecked
            && wrongReturnEsi.ReturnEntryEsiIdentityMatches == false
            && wrongReturnEsi.ConstructedReturnCapture.EntryEsi == 0x500
            && wrongReturnEsi.ReturnPlan is null
            && wrongReturnEsi.UnreachedDwordAtComputedSlotCaptureProvided);

        HighCountObjectHelperLifecyclePlan wrapped =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0, 0, true, 0xFFFFFFF8, 1),
                    0,
                    new(0xFFFFFFF8, 0, 3, 0, 0xFFFFFFFC, uint.MaxValue)));
        check("The bridge and return calculations accept zero and preserve unchecked DWORD wrap",
            wrapped.Outcome == HighCountObjectHelperLifecycleOutcome.Returned
            && wrapped.DwordAtFirstLoadedBase.Address == 1
            && wrapped.RetainedEsi == 0
            && wrapped.ReturnPlan?.DwordAtEntryEaxPlus8.Address == 0
            && wrapped.ReturnPlan?.DwordAtEntryEsiPlus8.Address == 8
            && wrapped.ReturnPlan?.DecrementedEax == 0xFFFFFFFF
            && wrapped.ReturnPlan?.DwordAtEntryEsiPlus4.Address == 4
            && wrapped.ReturnPlan?.ComputedSlotAddress == 0xFFFFFFFC
            && wrapped.ReturnPlan?.ReturnEax == 0x0B);

        HighCountObjectHelperLifecyclePlan identitiesOptional =
            HighCountObjectHelperLifecycleProjection.Project(
                new(
                    new(0x1000, 0xDD, true, 0x1234, 1),
                    0x500,
                    new(null, null, 1, 2, 3, 4)));
        check("Return EAX and ESI are constructed from entry and bridge state when nullable identity captures are omitted",
            identitiesOptional.Outcome == HighCountObjectHelperLifecycleOutcome.Returned
            && !identitiesOptional.ReturnEntryEaxIdentityChecked
            && !identitiesOptional.ReturnEntryEsiIdentityChecked
            && identitiesOptional.ConstructedReturnCapture.EntryEax == 0x1234
            && identitiesOptional.ConstructedReturnCapture.EntryEsi == 0x500
            && identitiesOptional.ReturnPlan?.Capture.EntryEax == 0x1234
            && identitiesOptional.ReturnPlan?.Capture.EntryEsi == 0x500);
    }
}
