using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition joins only the indexed equality destination to the final gate",
            CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .UpstreamEqualDestinationRawFileOffset
                == CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .FinalFrameEntryRawFileOffset
            && CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .FinalFrameEntryRawFileOffset == 0x012C9160
            && CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .UpstreamUnequalDestinationRawFileOffset == 0x012C9151
            && CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .FinalEqualDestinationRawFileOffset == 0x012C9167
            && CapacityFinalCalleeIndexedFinalLifecycleProjection
                    .FinalUnequalDestinationRawFileOffset == 0x012C917C);

        CapacityFinalCalleeIndexedFinalLifecyclePlan missingUpstreamEbp =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: null,
                    FreshFrameMinus4: 0x10,
                    FreshFrameMinus10: 0x20),
                FreshFrameMinus14: 0x20));
        check("Missing upstream EBP rejects all later captures before either child gate",
            missingUpstreamEbp.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome.UpstreamMissingEntryEBP
            && missingUpstreamEbp.FinalFrameGatePlan is null
            && !missingUpstreamEbp.FinalFrameGateReached
            && missingUpstreamEbp.ChildEntryEBP is null
            && missingUpstreamEbp.ChildEntryECX is null
            && missingUpstreamEbp.UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstreamEbp.LateFreshFrameMinus14CaptureProvided
            && !missingUpstreamEbp.UnreachedChildEntryECXCaptureProvided
            && !missingUpstreamEbp.LateChildEntryECXCaptureProvided
            && !missingUpstreamEbp.UpstreamCleanRouteAdmitted
            && !missingUpstreamEbp.ExactCleanExitAdmitted
            && missingUpstreamEbp.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingUpstreamEbp.Writes.IsEmpty);

        CapacityFinalCalleeIndexedFinalLifecyclePlan missingUpstreamFrame4 =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x100,
                    FreshFrameMinus4: null,
                    FreshFrameMinus10: 0x20),
                FreshFrameMinus14: 0x20));
        check("Missing upstream fresh EBP-4 rejects the downstream fresh EBP-14 capture",
            missingUpstreamFrame4.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome
                    .UpstreamMissingFreshFrameMinus4
            && missingUpstreamFrame4.FinalFrameGatePlan is null
            && !missingUpstreamFrame4.FinalFrameGateReached
            && missingUpstreamFrame4.UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstreamFrame4.LateFreshFrameMinus14CaptureProvided
            && !missingUpstreamFrame4.ExactCleanExitAdmitted
            && missingUpstreamFrame4.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingUpstreamFrame4.Writes.IsEmpty);

        CapacityFinalCalleeIndexedFinalLifecyclePlan missingUpstreamFrame10 =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x100,
                    FreshFrameMinus4: 1,
                    FreshFrameMinus10: null),
                FreshFrameMinus14: 0x20));
        check("Missing upstream fresh EBP-10 stops before the downstream child",
            missingUpstreamFrame10.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome
                    .UpstreamMissingFreshFrameMinus10
            && missingUpstreamFrame10.FinalFrameGatePlan is null
            && !missingUpstreamFrame10.FinalFrameGateReached
            && missingUpstreamFrame10.UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstreamFrame10.LateFreshFrameMinus14CaptureProvided
            && !missingUpstreamFrame10.ExactCleanExitAdmitted
            && missingUpstreamFrame10.StopAtRawFileOffsetExclusive == 0x012C9140
            && missingUpstreamFrame10.Writes.IsEmpty);

        uint retainedValue =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.CalculateIndexedAddress(3);
        CapacityFinalCalleeIndexedFinalLifecyclePlan missingFinalFrame14 =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x400,
                    FreshFrameMinus4: 3,
                    FreshFrameMinus10: retainedValue),
                FreshFrameMinus14: null));
        check("Missing child fresh EBP-14 delays the derived ECX until CMP and retains the prior store",
            missingFinalFrame14.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome.MissingFreshFrameMinus14
            && missingFinalFrame14.IndexedFrameGatePlan.ExactCleanRouteAdmitted
            && missingFinalFrame14.IndexedFrameGatePlan.JeSelectedDestinationRawFileOffset
                == 0x012C9160
            && missingFinalFrame14.FinalFrameGateReached
            && missingFinalFrame14.FinalFrameGatePlan?.Outcome
                == CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingFreshFrameMinus14
            && missingFinalFrame14.FinalFrameGatePlan?.FrameMinus14ReadReached == true
            && missingFinalFrame14.FinalFrameGatePlan?.FrameMinus14Address == 0x3EC
            && missingFinalFrame14.FinalFrameGatePlan?.FrameMinus14Captured == false
            && missingFinalFrame14.FinalFrameGatePlan?.Capture.EntryEBP == 0x400
            && missingFinalFrame14.ChildEntryEBP == 0x400
            && missingFinalFrame14.ChildEntryECX is null
            && missingFinalFrame14.ChildEntryECXSource
                == CapacityFinalCalleeIndexedFinalLifecycleChildECXSource.None
            && !missingFinalFrame14.UnreachedFreshFrameMinus14CaptureProvided
            && !missingFinalFrame14.LateFreshFrameMinus14CaptureProvided
            && !missingFinalFrame14.UnreachedChildEntryECXCaptureProvided
            && !missingFinalFrame14.LateChildEntryECXCaptureProvided
            && !missingFinalFrame14.ExactCleanExitAdmitted
            && missingFinalFrame14.StopAtRawFileOffsetExclusive == 0x012C9160
            && missingFinalFrame14.Writes.Length == 1
            && missingFinalFrame14.Writes[0]
                == new CapacityFinalCalleeIndexedFinalLifecycleWrite(
                    0x012C914A,
                    0x3F4,
                    retainedValue)
            && missingFinalFrame14.FinalFrameGatePlan?.Writes.IsEmpty == true);

        CapacityFinalCalleeIndexedFinalLifecyclePlan finalEqual =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x400,
                    FreshFrameMinus4: 3,
                    FreshFrameMinus10: retainedValue),
                FreshFrameMinus14: retainedValue));
        check("The final equality route is clean and preserves the ordered upstream store",
            finalEqual.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome.ChildEqualToFallthrough
            && finalEqual.IndexedFrameGatePlan.ExactCleanRouteAdmitted
            && finalEqual.IndexedFrameGatePlan.JeTaken == true
            && finalEqual.IndexedFrameGatePlan.JeSelectedDestinationRawFileOffset == 0x012C9160
            && finalEqual.FinalFrameGateReached
            && finalEqual.FinalFrameGatePlan?.Capture.EntryEBP == 0x400
            && finalEqual.FinalFrameGatePlan?.FrameMinus14Address == 0x3EC
            && finalEqual.FinalFrameGatePlan?.FrameMinus14 == retainedValue
            && finalEqual.FinalFrameGatePlan?.CompareLeft == retainedValue
            && finalEqual.FinalFrameGatePlan?.CompareRight == retainedValue
            && finalEqual.FinalFrameGatePlan?.JnzTaken == false
            && finalEqual.FinalFrameGatePlan?.JnzSelectedDestinationRawFileOffset
                == 0x012C9167
            && finalEqual.ChildEntryEBP == 0x400
            && finalEqual.ChildEntryECX == retainedValue
            && finalEqual.ChildEntryECXSource
                == CapacityFinalCalleeIndexedFinalLifecycleChildECXSource
                    .UpstreamFreshFrameMinus10
            && finalEqual.UpstreamCleanRouteAdmitted
            && finalEqual.ExactCleanExitAdmitted
            && finalEqual.StopAtRawFileOffsetExclusive == 0x012C9167
            && finalEqual.Writes.Length == 1
            && finalEqual.Writes[0]
                == new CapacityFinalCalleeIndexedFinalLifecycleWrite(
                    0x012C914A,
                    0x3F4,
                    retainedValue)
            && finalEqual.FinalFrameGatePlan?.Writes.IsEmpty == true);

        CapacityFinalCalleeIndexedFinalLifecyclePlan finalUnequal =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x400,
                    FreshFrameMinus4: 3,
                    FreshFrameMinus10: retainedValue),
                FreshFrameMinus14: unchecked(retainedValue + 1)));
        check("The final inequality route branches to 917C without altering the retained store",
            finalUnequal.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome
                    .ChildUnequalToBranchTarget
            && finalUnequal.FinalFrameGatePlan?.CompareLeft == unchecked(retainedValue + 1)
            && finalUnequal.FinalFrameGatePlan?.CompareRight == retainedValue
            && finalUnequal.FinalFrameGatePlan?.JnzTaken == true
            && finalUnequal.FinalFrameGatePlan?.JnzSelectedDestinationRawFileOffset
                == 0x012C917C
            && finalUnequal.ChildEntryECX == retainedValue
            && finalUnequal.ChildEntryECXSource
                == CapacityFinalCalleeIndexedFinalLifecycleChildECXSource
                    .UpstreamFreshFrameMinus10
            && finalUnequal.ExactCleanExitAdmitted
            && finalUnequal.StopAtRawFileOffsetExclusive == 0x012C917C
            && finalUnequal.Writes.Length == 1
            && finalUnequal.Writes[0].RawFileOffset == 0x012C914A
            && finalUnequal.Writes[0].Address == 0x3F4
            && finalUnequal.Writes[0].Value == retainedValue);

        uint indexedAddress =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.CalculateIndexedAddress(1);
        CapacityFinalCalleeIndexedFinalLifecyclePlan upstreamUnequal =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x400,
                    FreshFrameMinus4: 1,
                    FreshFrameMinus10: indexedAddress + 1),
                FreshFrameMinus14: null));
        check("A clean upstream inequality stops at 9151 without starting the final gate",
            upstreamUnequal.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome.UpstreamUnequalTo9151
            && upstreamUnequal.IndexedFrameGatePlan.ExactCleanRouteAdmitted
            && upstreamUnequal.IndexedFrameGatePlan.JeTaken == false
            && upstreamUnequal.IndexedFrameGatePlan.JeSelectedDestinationRawFileOffset
                == 0x012C9151
            && upstreamUnequal.FinalFrameGatePlan is null
            && !upstreamUnequal.FinalFrameGateReached
            && upstreamUnequal.ChildEntryEBP is null
            && upstreamUnequal.ChildEntryECX is null
            && !upstreamUnequal.UnreachedFreshFrameMinus14CaptureProvided
            && !upstreamUnequal.LateFreshFrameMinus14CaptureProvided
            && upstreamUnequal.UpstreamCleanRouteAdmitted
            && upstreamUnequal.ExactCleanExitAdmitted
            && upstreamUnequal.StopAtRawFileOffsetExclusive == 0x012C9151
            && upstreamUnequal.Writes.Length == 1
            && upstreamUnequal.Writes[0].RawFileOffset == 0x012C914A);

        CapacityFinalCalleeIndexedFinalLifecyclePlan upstreamUnequalContaminated =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x400,
                    FreshFrameMinus4: 1,
                    FreshFrameMinus10: indexedAddress + 1),
                FreshFrameMinus14: indexedAddress + 1));
        check("An upstream inequality rejects a supplied downstream frame capture",
            upstreamUnequalContaminated.Outcome
                == CapacityFinalCalleeIndexedFinalLifecycleOutcome
                    .UpstreamUnequalTo9151Contaminated
            && upstreamUnequalContaminated.FinalFrameGatePlan is null
            && !upstreamUnequalContaminated.FinalFrameGateReached
            && upstreamUnequalContaminated.UnreachedFreshFrameMinus14CaptureProvided
            && upstreamUnequalContaminated.LateFreshFrameMinus14CaptureProvided
            && upstreamUnequalContaminated.UpstreamCleanRouteAdmitted
            && !upstreamUnequalContaminated.ExactCleanExitAdmitted
            && upstreamUnequalContaminated.StopAtRawFileOffsetExclusive == 0x012C9151
            && upstreamUnequalContaminated.Writes.Length == 1
            && upstreamUnequalContaminated.Writes[0].RawFileOffset == 0x012C914A);

        uint wrappedIndexedAddress =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.CalculateIndexedAddress(
                0xFFFFFFFF);
        CapacityFinalCalleeIndexedFinalLifecyclePlan wrappedAliases =
            CapacityFinalCalleeIndexedFinalLifecycleProjection.Project(new(
                IndexedFrameGateCapture: new(
                    EntryEBP: 0x14,
                    FreshFrameMinus4: 0xFFFFFFFF,
                    FreshFrameMinus10: wrappedIndexedAddress),
                FreshFrameMinus14: wrappedIndexedAddress));
        check("Wrapped frame addresses and indexed arithmetic alias through both clean gates",
            wrappedIndexedAddress == 0x01EDADE8
            && wrappedAliases.IndexedFrameGatePlan.FrameMinus4Address == 0x10
            && wrappedAliases.IndexedFrameGatePlan.FrameMinus10Address == 4
            && wrappedAliases.IndexedFrameGatePlan.StackStoreAddress == 8
            && wrappedAliases.IndexedFrameGatePlan.JeTaken == true
            && wrappedAliases.FinalFrameGatePlan?.FrameMinus14Address == 0
            && wrappedAliases.ChildEntryEBP == 0x14
            && wrappedAliases.ChildEntryECX == wrappedIndexedAddress
            && wrappedAliases.FinalFrameGatePlan?.CompareResult == 0
            && wrappedAliases.FinalFrameGatePlan?.JnzTaken == false
            && wrappedAliases.ExactCleanExitAdmitted
            && wrappedAliases.StopAtRawFileOffsetExclusive == 0x012C9167
            && wrappedAliases.Writes.Length == 1
            && wrappedAliases.Writes[0].Address == 8
            && wrappedAliases.Writes[0].Value == wrappedIndexedAddress);
    }
}
