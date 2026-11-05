using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeBacklinkFinalLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact 9151 backlink and 9160 final-frame boundaries",
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.UpstreamEntryRawFileOffset
                == 0x012C9151
            && CapacityFinalCalleeBacklinkFinalLifecycleProjection.FinalFrameEntryRawFileOffset
                == 0x012C9160
            && CapacityFinalCalleeBacklinkFinalLifecycleProjection
                .OpaqueCallBoundaryRawFileOffsetExclusive == 0x012C915A
            && CapacityFinalCalleeBacklinkFinalLifecycleProjection
                .FinalFrameEqualBoundaryRawFileOffsetExclusive == 0x012C9167
            && CapacityFinalCalleeBacklinkFinalLifecycleProjection
                .FinalFrameUnequalBoundaryRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan belowClean =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 0x10,
                    EntryEBX: 0x20,
                    EntryEDX: null,
                    FreshFieldAtEcxPlusC: null),
                CallerEBP: null,
                FreshCallerFrameMinus14: null));
        check("A clean below branch stops at 915A without entering the final gate",
            belowClean.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamBelowToOpaqueCallBoundary
            && belowClean.UpstreamBacklinkGate.JbTaken == true
            && belowClean.UpstreamBacklinkGate.ExactCleanExitAdmitted
            && !belowClean.FinalFrameGateReached
            && belowClean.FinalFrameGate is null
            && !belowClean.ExactCleanHandoffToFinalFrameGate
            && belowClean.OrderedReachedMemoryReads.IsEmpty
            && !belowClean.ChildCompareReached
            && belowClean.DerivedChildECX is null
            && !belowClean.UnreachedCallerEBPCaptureProvided
            && !belowClean.UnreachedFreshCallerFrameMinus14CaptureProvided
            && belowClean.ExactCleanExitAdmitted
            && belowClean.StopAtRawFileOffsetExclusive == 0x012C915A
            && !belowClean.OpaqueCallReached);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan contaminatedBelow =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 0,
                    EntryEBX: 1,
                    EntryEDX: 0x1234,
                    FreshFieldAtEcxPlusC: 0x5678),
                CallerEBP: 0,
                FreshCallerFrameMinus14: 0));
        check("A contaminated below route rejects all later captures at 915A",
            contaminatedBelow.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamBelowToOpaqueCallBoundary
            && contaminatedBelow.UpstreamBacklinkGate.UnusedEntryEDXCaptureProvided
            && contaminatedBelow.UpstreamBacklinkGate.UnusedFreshFieldCaptureProvided
            && contaminatedBelow.UnreachedCallerEBPCaptureProvided
            && contaminatedBelow.UnreachedFreshCallerFrameMinus14CaptureProvided
            && contaminatedBelow.LateCallerEBPCaptureProvided
            && contaminatedBelow.LateFreshCallerFrameMinus14CaptureProvided
            && !contaminatedBelow.FinalFrameGateReached
            && contaminatedBelow.FinalFrameGate is null
            && !contaminatedBelow.ExactCleanExitAdmitted
            && contaminatedBelow.StopAtRawFileOffsetExclusive == 0x012C915A
            && !contaminatedBelow.OpaqueCallReached);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan unequalOpaque =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 8,
                    EntryEBX: 7,
                    EntryEDX: 3,
                    FreshFieldAtEcxPlusC: 2),
                CallerEBP: 0x100,
                FreshCallerFrameMinus14: 0));
        check("The unequal 9155 compare is bounded at 915A and rejects child captures",
            unequalOpaque.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamUnequalToOpaqueCallBoundary
            && unequalOpaque.UpstreamBacklinkGate.JzTaken == false
            && unequalOpaque.UpstreamBacklinkGate.SecondCompareFlags is
                { Zero: false }
            && unequalOpaque.UpstreamBacklinkGate.ExactCleanExitAdmitted
            && unequalOpaque.UnreachedCallerEBPCaptureProvided
            && unequalOpaque.UnreachedFreshCallerFrameMinus14CaptureProvided
            && !unequalOpaque.FinalFrameGateReached
            && unequalOpaque.FinalFrameGate is null
            && !unequalOpaque.ExactCleanExitAdmitted
            && unequalOpaque.StopAtRawFileOffsetExclusive == 0x012C915A
            && !unequalOpaque.OpaqueCallReached);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingUpstreamEcx =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: null,
                    EntryEBX: 1,
                    EntryEDX: 2,
                    FreshFieldAtEcxPlusC: 3),
                CallerEBP: 0x100,
                FreshCallerFrameMinus14: 0));
        check("Missing upstream ECX blocks every later stage and records unreached captures",
            missingUpstreamEcx.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamMissingEntryECX
            && missingUpstreamEcx.UpstreamBacklinkGate
                .UnreachedEntryEBXCaptureProvided
            && missingUpstreamEcx.UpstreamBacklinkGate
                .LateEntryEDXCaptureProvided
            && missingUpstreamEcx.UpstreamBacklinkGate
                .LateFreshFieldCaptureProvided
            && missingUpstreamEcx.UnreachedCallerEBPCaptureProvided
            && missingUpstreamEcx.UnreachedFreshCallerFrameMinus14CaptureProvided
            && !missingUpstreamEcx.FinalFrameGateReached
            && missingUpstreamEcx.OrderedReachedMemoryReads.IsEmpty
            && !missingUpstreamEcx.ExactCleanExitAdmitted
            && missingUpstreamEcx.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingUpstreamEbx =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 1,
                    EntryEBX: null,
                    EntryEDX: 2,
                    FreshFieldAtEcxPlusC: 3),
                CallerEBP: null,
                FreshCallerFrameMinus14: 0));
        check("Missing upstream EBX stops before the first compare and rejects later frame data",
            missingUpstreamEbx.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamMissingEntryEBX
            && missingUpstreamEbx.UpstreamBacklinkGate.EntryECXCaptured
            && !missingUpstreamEbx.UpstreamBacklinkGate.FirstCompareReached
            && missingUpstreamEbx.UnreachedFreshCallerFrameMinus14CaptureProvided
            && !missingUpstreamEbx.FinalFrameGateReached
            && missingUpstreamEbx.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingUpstreamEdx =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 0x100,
                    EntryEBX: 0x100,
                    EntryEDX: null,
                    FreshFieldAtEcxPlusC: 4),
                CallerEBP: 0,
                FreshCallerFrameMinus14: 0));
        check("Missing upstream EDX records its reached fresh read but cannot reach the child",
            missingUpstreamEdx.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamMissingEntryEDX
            && missingUpstreamEdx.UpstreamFirstCompareFlags is
                { Zero: true, Carry: false }
            && missingUpstreamEdx.UpstreamBacklinkGate.FreshFieldReadReached
            && missingUpstreamEdx.OrderedReachedMemoryReads.SequenceEqual(new[]
            {
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .UpstreamFreshFieldAtEcxPlusC
            })
            && !missingUpstreamEdx.FinalFrameGateReached
            && missingUpstreamEdx.StopAtRawFileOffsetExclusive == 0x012C9155);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingUpstreamFresh =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 0x100,
                    EntryEBX: 0x100,
                    EntryEDX: 4,
                    FreshFieldAtEcxPlusC: null),
                CallerEBP: 0x100,
                FreshCallerFrameMinus14: 0));
        check("Missing the upstream fresh field prevents its CMP and all child execution",
            missingUpstreamFresh.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .UpstreamMissingFreshFieldAtEcxPlusC
            && missingUpstreamFresh.UpstreamBacklinkGate.EntryEDXCaptured
            && missingUpstreamFresh.UpstreamBacklinkGate.FreshFieldReadReached
            && !missingUpstreamFresh.UpstreamBacklinkGate.FreshFieldCaptured
            && !missingUpstreamFresh.UpstreamBacklinkGate.SecondCompareReached
            && !missingUpstreamFresh.FinalFrameGateReached
            && missingUpstreamFresh.UnreachedCallerEBPCaptureProvided
            && missingUpstreamFresh.StopAtRawFileOffsetExclusive == 0x012C9155);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingCallerEbp =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: EqualBacklinkCapture(0),
                CallerEBP: null,
                FreshCallerFrameMinus14: 0x20000000));
        check("The clean 9160 handoff reaches the child load but requires independent EBP",
            missingCallerEbp.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome.MissingCallerEBP
            && missingCallerEbp.ExactCleanHandoffToFinalFrameGate
            && missingCallerEbp.FinalFrameGateReached
            && missingCallerEbp.FinalFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateFinalFrameGateOutcome.MissingEntryEBP,
                UnreachedFreshFrameMinus14CaptureProvided: true,
                UnreachedEntryECXCaptureProvided: false,
                LateFreshFrameMinus14CaptureProvided: true,
                FrameMinus14ReadReached: false
            }
            && missingCallerEbp.UpstreamEntryECX == 0
            && missingCallerEbp.DerivedChildECX is null
            && !missingCallerEbp.ChildECXCarriedToCompare
            && missingCallerEbp.UnreachedDerivedChildECXAvailable
            && missingCallerEbp.LateDerivedChildECXAvailable
            && missingCallerEbp.OrderedReachedMemoryReads.SequenceEqual(new[]
            {
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .UpstreamFreshFieldAtEcxPlusC
            })
            && missingCallerEbp.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan missingChildFresh =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: EqualBacklinkCapture(0x20),
                CallerEBP: 0x14,
                FreshCallerFrameMinus14: null));
        check("The child EBP-14 reload is independent and gates forwarding ECX to CMP",
            missingChildFresh.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .MissingFreshCallerFrameMinus14
            && missingChildFresh.FinalFrameGateReached
            && missingChildFresh.FinalFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateFinalFrameGateOutcome
                    .MissingFreshFrameMinus14,
                EntryEBPCaptured: true,
                FrameMinus14ReadReached: true,
                FrameMinus14Address: 0,
                EntryECXCaptured: false,
                CompareReached: false,
                LateEntryECXCaptureProvided: false
            }
            && missingChildFresh.UpstreamEntryECX == 0x20
            && missingChildFresh.DerivedChildECX is null
            && !missingChildFresh.ChildECXCarriedToCompare
            && missingChildFresh.UnreachedDerivedChildECXAvailable
            && missingChildFresh.LateDerivedChildECXAvailable
            && missingChildFresh.OrderedReachedMemoryReads.SequenceEqual(new[]
            {
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .UpstreamFreshFieldAtEcxPlusC,
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .ChildFreshFrameMinus14
            })
            && missingChildFresh.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan equalZero =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: EqualBacklinkCapture(0),
                CallerEBP: 0,
                FreshCallerFrameMinus14: 0));
        check("A zero upstream ECX is forwarded only at CMP and equality falls through",
            equalZero.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome.ChildEqualToFallthrough
            && equalZero.UpstreamBacklinkGate.Outcome
                == CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget
            && equalZero.ExactCleanHandoffToFinalFrameGate
            && equalZero.FinalFrameGateReached
            && equalZero.FinalFrameGate is
            {
                FrameMinus14Address: 0xFFFFFFEC,
                FrameMinus14Captured: true,
                FrameMinus14: 0,
                EntryECXCaptured: true,
                CompareReached: true,
                CompareLeft: 0,
                CompareRight: 0,
                CompareResult: 0,
                CompareFlags: { Carry: false, Zero: true },
                JnzReached: true,
                JnzTaken: false,
                JnzSelectedDestinationRawFileOffset: 0x012C9167,
                ExactCleanExitAdmitted: true
            }
            && equalZero.UpstreamEntryECX == 0
            && equalZero.DerivedChildECX == 0
            && equalZero.ChildECXCarriedToCompare
            && !equalZero.UnreachedDerivedChildECXAvailable
            && !equalZero.LateDerivedChildECXAvailable
            && equalZero.ChildCompareFlags is { Zero: true, Carry: false }
            && equalZero.OrderedReachedMemoryReads.SequenceEqual(new[]
            {
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .UpstreamFreshFieldAtEcxPlusC,
                CapacityFinalCalleeBacklinkFinalLifecycleMemoryRead
                    .ChildFreshFrameMinus14
            })
            && equalZero.ExactCleanExitAdmitted
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C9167
            && !equalZero.OpaqueCallReached);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan childUnequal =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: EqualBacklinkCapture(0x20),
                CallerEBP: 0x14,
                FreshCallerFrameMinus14: 0));
        check("An unequal final CMP uses the upstream ECX and stops at 917C",
            childUnequal.Outcome
                == CapacityFinalCalleeBacklinkFinalLifecycleOutcome
                    .ChildUnequalToBranchTarget
            && childUnequal.FinalFrameGate is
            {
                CompareLeft: 0,
                CompareRight: 0x20,
                CompareResult: 0xFFFFFFE0,
                CompareFlags: { Carry: true, Zero: false },
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C917C,
                ExactCleanExitAdmitted: true
            }
            && childUnequal.DerivedChildECX == 0x20
            && childUnequal.ChildECXCarriedToCompare
            && childUnequal.ExactCleanExitAdmitted
            && childUnequal.StopAtRawFileOffsetExclusive == 0x012C917C
            && !childUnequal.OpaqueCallReached);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan independentCallerFrame =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: EqualBacklinkCapture(0x11223344),
                CallerEBP: 0x12345678,
                FreshCallerFrameMinus14: 0x87654321));
        check("Caller EBP and fresh frame data are independent of upstream ECX and prior stores",
            independentCallerFrame.FinalFrameGate is
            {
                FrameMinus14Address: 0x12345664,
                FrameMinus14: 0x87654321,
                CompareLeft: 0x87654321,
                CompareRight: 0x11223344,
                CompareReached: true
            }
            && independentCallerFrame.DerivedChildECX == 0x11223344
            && independentCallerFrame.ChildECXCarriedToCompare);

        CapacityFinalCalleeBacklinkFinalLifecyclePlan wrappedAddresses =
            CapacityFinalCalleeBacklinkFinalLifecycleProjection.Project(new(
                UpstreamBacklinkCapture: new(
                    EntryECX: 0xFFFFFFF4,
                    EntryEBX: 0xFFFFFFF4,
                    EntryEDX: 0,
                    FreshFieldAtEcxPlusC: 0),
                CallerEBP: 0x0C,
                FreshCallerFrameMinus14: 0xFFFFFFFF));
        check("Both upstream ECX+C and child EBP-14 use DWORD-wrapped addresses",
            wrappedAddresses.UpstreamBacklinkGate.FreshFieldAddress == 0
            && wrappedAddresses.UpstreamBacklinkGate.SecondCompareFlags
                is { Zero: true }
            && wrappedAddresses.FinalFrameGate is
            {
                FrameMinus14Address: 0xFFFFFFF8,
                CompareLeft: 0xFFFFFFFF,
                CompareRight: 0xFFFFFFF4,
                CompareResult: 0xB
            }
            && wrappedAddresses.DerivedChildECX == 0xFFFFFFF4
            && wrappedAddresses.StopAtRawFileOffsetExclusive == 0x012C917C);
    }

    private static CapacityFinalCalleeAlternateBacklinkGateCapture EqualBacklinkCapture(
        uint upstreamEcx) =>
        new(
            EntryECX: upstreamEcx,
            EntryEBX: upstreamEcx,
            EntryEDX: 0,
            FreshFieldAtEcxPlusC: 0);
}
