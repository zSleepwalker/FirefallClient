using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedBacklinkFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Only the clean downstream backlink equality hands off from 9160",
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .IndexedEqualBoundaryRawFileOffset
                == 0x012C9160
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .BacklinkEqualDestinationRawFileOffset == 0x012C9160
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .FinalFrameEntryRawFileOffset == 0x012C9160
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .BacklinkBelowBoundaryRawFileOffset == 0x012C915A
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .BacklinkUnequalBoundaryRawFileOffset == 0x012C915A
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .FinalFrameEqualDestinationRawFileOffset == 0x012C9167
            && CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection
                    .FinalFrameUnequalDestinationRawFileOffset == 0x012C917C);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan indexedEqual =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x01EDADF0),
                FreshCallerFrameMinus14: 0x20));
        check("An indexed equality at 9160 stays bounded and does not create a backlink child",
            indexedEqual.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .UpstreamIndexedEqualTo9160
            && indexedEqual.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UpstreamEqualTo9160
            && indexedEqual.IndexedBacklinkLifecycle.BacklinkGate is null
            && !indexedEqual.FinalFrameGateReached
            && indexedEqual.FinalFrameGate is null
            && !indexedEqual.ExactCleanHandoffToFinalFrameGate
            && indexedEqual.UnreachedFreshFrameMinus14CaptureProvided
            && indexedEqual.LateFreshFrameMinus14CaptureProvided
            && !indexedEqual.ExactCleanExitAdmitted
            && indexedEqual.StopAtRawFileOffsetExclusive == 0x012C9160
            && indexedEqual.Writes.Length == 1
            && indexedEqual.Writes[0].RawFileOffset == 0x012C914A
            && !indexedEqual.OpaqueCallReached);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan cleanIndexedEqual =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x01EDADF0),
                FreshCallerFrameMinus14: null));
        check("The clean indexed equality remains an admitted bounded stop at 9160",
            cleanIndexedEqual.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UpstreamEqualTo9160
            && cleanIndexedEqual.IndexedBacklinkLifecycle.ExactCleanExitAdmitted
            && cleanIndexedEqual.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .UpstreamIndexedEqualTo9160
            && !cleanIndexedEqual.FinalFrameGateReached
            && cleanIndexedEqual.FinalFrameGate is null
            && cleanIndexedEqual.ExactCleanExitAdmitted
            && !cleanIndexedEqual.ExactCleanHandoffToFinalFrameGate
            && cleanIndexedEqual.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan missingUpstream =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    upstreamEntryEbp: null,
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 0x44,
                    freshField: 0x44),
                FreshCallerFrameMinus14: 0x20));
        check("A missing upstream indexed prerequisite rejects frame 14 without a child",
            missingUpstream.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .UpstreamIncompleteWithDownstreamCapture
            && !missingUpstream.FinalFrameGateReached
            && missingUpstream.FinalFrameGate is null
            && missingUpstream.ChildEntryEBP is null
            && missingUpstream.ChildEntryECX is null
            && missingUpstream.UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstream.LateFreshFrameMinus14CaptureProvided
            && !missingUpstream.ExactCleanHandoffToFinalFrameGate
            && !missingUpstream.ExactCleanExitAdmitted
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingUpstream.Writes.IsEmpty);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan below915A =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x10,
                    callerEBX: 0x20),
                FreshCallerFrameMinus14: 0x99));
        check("The backlink JB boundary at 915A rejects a supplied frame-14 capture",
            below915A.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .BelowTo915A
            && below915A.BacklinkGate is
            {
                JbReached: true,
                JbTaken: true,
                JbSelectedDestinationRawFileOffset: 0x012C915A
            }
            && !below915A.FinalFrameGateReached
            && below915A.FinalFrameGate is null
            && below915A.UnreachedFreshFrameMinus14CaptureProvided
            && below915A.LateFreshFrameMinus14CaptureProvided
            && !below915A.ExactCleanExitAdmitted
            && below915A.StopAtRawFileOffsetExclusive == 0x012C915A
            && below915A.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan unequal915A =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 3,
                    freshField: 2),
                FreshCallerFrameMinus14: 0x99));
        check("The backlink unequal JZ boundary at 915A also rejects frame 14",
            unequal915A.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UnequalTo915A
            && unequal915A.BacklinkGate is
            {
                JzReached: true,
                JzTaken: false,
                JzSelectedDestinationRawFileOffset: 0x012C915A
            }
            && !unequal915A.FinalFrameGateReached
            && unequal915A.FinalFrameGate is null
            && unequal915A.UnreachedFreshFrameMinus14CaptureProvided
            && unequal915A.LateFreshFrameMinus14CaptureProvided
            && !unequal915A.ExactCleanExitAdmitted
            && unequal915A.StopAtRawFileOffsetExclusive == 0x012C915A
            && unequal915A.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan cleanEqual =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 0xAABBCCDD,
                    freshField: 0xAABBCCDD),
                FreshCallerFrameMinus14: 0x20));
        check("A clean downstream equality reaches CMP with retained EBP and backlink ECX",
            cleanEqual.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .ChildEqualToFallthrough
            && cleanEqual.IndexedBacklinkLifecycle.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.EqualTo9160
            && cleanEqual.BacklinkGate is
            {
                Outcome: CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget,
                JzTaken: true,
                JzSelectedDestinationRawFileOffset: 0x012C9160,
                ExactCleanBypassAdmitted: true
            }
            && cleanEqual.ExactCleanHandoffToFinalFrameGate
            && cleanEqual.FinalFrameGateReached
            && cleanEqual.FinalFrameGate is
            {
                EntryEBPCaptured: true,
                FrameMinus14ReadReached: true,
                FrameMinus14Address: 0xEC,
                FrameMinus14Captured: true,
                FrameMinus14: 0x20,
                CompareReached: true,
                CompareLeft: 0x20,
                CompareRight: 0x20,
                CompareResult: 0,
                CompareFlags: {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                JnzReached: true,
                JnzTaken: false,
                JnzSelectedDestinationRawFileOffset: 0x012C9167
            }
            && cleanEqual.ChildEntryEBP == 0x100
            && cleanEqual.ChildEntryEBP
                == cleanEqual.IndexedBacklinkLifecycle.Capture
                    .UpstreamCapture.EntryEBP
            && cleanEqual.ChildEntryECX == 0x20
            && cleanEqual.BacklinkEntryECX == 0x20
            && cleanEqual.ChildEntryECXSource
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource
                    .BacklinkEntryECX
            && cleanEqual.ChildECXCarriedToCompare
            && !cleanEqual.UnreachedDerivedChildECXAvailable
            && !cleanEqual.LateDerivedChildECXAvailable
            && cleanEqual.ExactCleanExitAdmitted
            && cleanEqual.StopAtRawFileOffsetExclusive == 0x012C9167
            && cleanEqual.Writes.Length == 1
            && cleanEqual.Writes[0]
                == new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                    0x012C914A, 0xF4, 0x01EDADF0)
            && cleanEqual.FinalFrameGate?.Writes.IsEmpty == true
            && !cleanEqual.OpaqueCallReached);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan cleanUnequal =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 0xAABBCCDD,
                    freshField: 0xAABBCCDD),
                FreshCallerFrameMinus14: 0x21));
        check("A clean downstream equality retains the exact alternate final branch and flags",
            cleanUnequal.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .ChildUnequalToBranchTarget
            && cleanUnequal.FinalFrameGate is
            {
                CompareLeft: 0x21,
                CompareRight: 0x20,
                CompareResult: 1,
                CompareFlags: {
                    Carry: false,
                    Parity: false,
                    Auxiliary: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                },
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C917C
            }
            && cleanUnequal.ChildEntryECX == 0x20
            && cleanUnequal.ChildECXCarriedToCompare
            && cleanUnequal.ExactCleanExitAdmitted
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C917C
            && cleanUnequal.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan missingFresh14 =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 0x44,
                    freshField: 0x44),
                FreshCallerFrameMinus14: null));
        check("The independently reached frame-14 load delays backlink ECX until CMP",
            missingFresh14.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .MissingFreshFrameMinus14
            && missingFresh14.FinalFrameGateReached
            && missingFresh14.FinalFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateFinalFrameGateOutcome
                    .MissingFreshFrameMinus14,
                Capture.EntryEBP: 0x100,
                FrameMinus14Address: 0xEC,
                FrameMinus14ReadReached: true,
                FrameMinus14Captured: false,
                EntryECXCaptured: false,
                CompareReached: false
            }
            && missingFresh14.ChildEntryEBP == 0x100
            && missingFresh14.ChildEntryECX is null
            && missingFresh14.ChildEntryECXSource
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleChildECXSource.None
            && missingFresh14.UnreachedDerivedChildECXAvailable
            && missingFresh14.LateDerivedChildECXAvailable
            && !missingFresh14.UnreachedFreshFrameMinus14CaptureProvided
            && !missingFresh14.LateFreshFrameMinus14CaptureProvided
            && !missingFresh14.ExactCleanExitAdmitted
            && missingFresh14.StopAtRawFileOffsetExclusive == 0x012C9160
            && missingFresh14.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan zeroCapture =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    upstreamEntryEbp: 0,
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0,
                    callerEBX: 0,
                    callerEDX: 0,
                    freshField: 0),
                FreshCallerFrameMinus14: 0));
        check("Zero-valued captures remain present through both compares and wrap EBP-14",
            zeroCapture.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .ChildEqualToFallthrough
            && zeroCapture.IndexedFrameGatePlan.FrameMinus4Captured
            && zeroCapture.IndexedFrameGatePlan.FrameMinus10Captured
            && zeroCapture.IndexedFrameGatePlan.StackStoreAddress == 0xFFFFFFF4
            && zeroCapture.IndexedFrameGatePlan.Writes.Length == 1
            && zeroCapture.BacklinkGate?.EntryEDXCaptured == true
            && zeroCapture.BacklinkGate?.FreshFieldCaptured == true
            && zeroCapture.FinalFrameGate is
            {
                FrameMinus14Address: 0xFFFFFFEC,
                FrameMinus14Captured: true,
                FrameMinus14: 0,
                EntryECXCaptured: true,
                CompareResult: 0,
                CompareFlags: { Zero: true },
                JnzTaken: false
            }
            && zeroCapture.ChildEntryEBP == 0
            && zeroCapture.ChildEntryECX == 0
            && zeroCapture.ChildECXCarriedToCompare
            && zeroCapture.ExactCleanExitAdmitted
            && zeroCapture.StopAtRawFileOffsetExclusive == 0x012C9167
            && zeroCapture.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkFinalLifecyclePlan wrappedAlias =
            CapacityFinalCalleeIndexedBacklinkFinalLifecycleProjection.Project(new(
                IndexedBacklinkCapture: Capture(
                    upstreamEntryEbp: 0x14,
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0xFFFFFFF4,
                    callerEBX: 0xFFFFFFF0,
                    callerEDX: 0x11,
                    freshField: 0x11),
                FreshCallerFrameMinus14: 0xFFFFFFF4));
        check("Aliased wrapped fresh reads remain independent and preserve one indexed store",
            wrappedAlias.Outcome
                == CapacityFinalCalleeIndexedBacklinkFinalLifecycleOutcome
                    .ChildEqualToFallthrough
            && wrappedAlias.IndexedFrameGatePlan.StackStoreAddress == 8
            && wrappedAlias.IndexedFrameGatePlan.Writes.Length == 1
            && wrappedAlias.BacklinkGate?.FreshFieldAddress == 0
            && wrappedAlias.BacklinkGate?.FreshFieldValue == 0x11
            && wrappedAlias.FinalFrameGate?.FrameMinus14Address == 0
            && wrappedAlias.FinalFrameGate?.FrameMinus14 == 0xFFFFFFF4
            && wrappedAlias.FinalFrameGate?.FrameMinus14
                != wrappedAlias.BacklinkGate?.FreshFieldValue
            && wrappedAlias.FinalFrameGate?.CompareRight == 0xFFFFFFF4
            && wrappedAlias.FinalFrameGate?.CompareResult == 0
            && wrappedAlias.ChildEntryEBP == 0x14
            && wrappedAlias.ChildEntryECX == 0xFFFFFFF4
            && wrappedAlias.ExactCleanExitAdmitted
            && wrappedAlias.StopAtRawFileOffsetExclusive == 0x012C9167
            && wrappedAlias.Writes.Length == 1
            && wrappedAlias.Writes[0].Address == 8
            && wrappedAlias.Writes[0].Value == 0x01EDADF0);
    }

    private static CapacityFinalCalleeIndexedBacklinkLifecycleCapture Capture(
        uint? freshFrameMinus4,
        uint? freshFrameMinus10,
        uint? upstreamEntryEbp = 0x100,
        uint? callerEBX = null,
        uint? callerEDX = null,
        uint? freshField = null) =>
        new(
            new CapacityFinalCalleeAlternateIndexedFrameGateCapture(
                upstreamEntryEbp,
                freshFrameMinus4,
                freshFrameMinus10),
            callerEBX,
            callerEDX,
            freshField);
}
