using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePairedFrameGateLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The lifecycle boundaries preserve the clean 9122 handoff destinations",
            CapacityFinalCalleePairedFrameGateLifecycleProjection
                .FirstOpaqueCallBoundaryRawFileOffsetExclusive == 0x012C911C
            && CapacityFinalCalleePairedFrameGateLifecycleProjection
                .AlternateSecondFrameGateEntryRawFileOffset == 0x012C9122
            && CapacityFinalCalleePairedFrameGateLifecycleProjection
                .EqualSecondGateBoundaryRawFileOffsetExclusive == 0x012C9127
            && CapacityFinalCalleePairedFrameGateLifecycleProjection
                .UnequalSecondGateBoundaryRawFileOffsetExclusive == 0x012C912D);

        CapacityFinalCalleePairedFrameGateLifecyclePlan equalSecondGate =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                CleanUnequalCapture(freshFrameMinus10: 0x100));
        check("Clean unequal 9122 derives retained inputs and cleanly stops equal at 9127",
            equalSecondGate.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .SecondGateEqualToOpaqueCallBoundary
            && equalSecondGate.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .CompareUnequalToBranchTarget
            && equalSecondGate.ExactCleanHandoffToAlternateSecondFrameGate
            && equalSecondGate.AlternateSecondFrameGateReached
            && equalSecondGate.DerivedSecondGateEntryEDX == 0x100
            && equalSecondGate.DerivedSecondGateEntryEBP == 0x200
            && equalSecondGate.AlternateSecondFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                    .EqualToOpaqueCallBoundary,
                EntryEDXCaptured: true,
                EntryEBPCaptured: true,
                FrameMinus10ReadReached: true,
                FrameMinus10Address: 0x1F0,
                FrameMinus10Captured: true,
                FrameMinus10: 0x100,
                CompareLeft: 0x100,
                CompareRight: 0x100,
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
                JnzSelectedDestinationRawFileOffset: 0x012C9127,
                ExactCleanExitAdmitted: true,
                OpaqueCallReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9127
            }
            && !equalSecondGate.UnreachedFreshFrameMinus10CaptureProvided
            && equalSecondGate.ExactCleanExitAdmitted
            && equalSecondGate.StopAtRawFileOffsetExclusive == 0x012C9127
            && !equalSecondGate.FirstOpaqueCallReached
            && !equalSecondGate.SecondOpaqueCallReached
            && equalSecondGate.OrderedUpstreamWrites.SequenceEqual(
                new[]
                {
                    new CapacityFinalCalleeAlternateFrameLoadsWrite(
                        0x012C910F, 0x1F0, 0xDEADBEEF),
                    new CapacityFinalCalleeAlternateFrameLoadsWrite(
                        0x012C9115, 0x1EC, 0x200)
                })
            && equalSecondGate.AlternateSecondFrameGate?.FrameMinus10Address
                == equalSecondGate.OrderedUpstreamWrites[0].Address
            && equalSecondGate.AlternateSecondFrameGate?.FrameMinus10
                != equalSecondGate.OrderedUpstreamWrites[0].Value);

        CapacityFinalCalleePairedFrameGateLifecyclePlan unequalSecondGate =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                CleanUnequalCapture(freshFrameMinus10: 0x101));
        check("Clean unequal second-gate branch has exact flags and stops at 912D",
            unequalSecondGate.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .SecondGateUnequalTo912D
            && unequalSecondGate.AlternateSecondFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                    .UnequalToBranchTarget,
                CompareLeft: 0x100,
                CompareRight: 0x101,
                CompareResult: 0xFFFFFFFF,
                CompareFlags: {
                    Carry: true,
                    Parity: true,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                JnzReached: true,
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C912D,
                ExactCleanExitAdmitted: true,
                OpaqueCallReached: false,
                StopAtRawFileOffsetExclusive: 0x012C912D
            }
            && unequalSecondGate.ExactCleanExitAdmitted
            && unequalSecondGate.StopAtRawFileOffsetExclusive == 0x012C912D
            && unequalSecondGate.OrderedUpstreamWrites.Length == 2);

        CapacityFinalCalleePairedFrameGateLifecyclePlan missingSecondGateFresh =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                CleanUnequalCapture(freshFrameMinus10: null));
        check("Missing second-gate fresh read keeps both earlier stores and stops at 9122",
            missingSecondGateFresh.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .MissingSecondGateFreshFrameMinus10
            && missingSecondGateFresh.AlternateSecondFrameGateReached
            && missingSecondGateFresh.ExactCleanHandoffToAlternateSecondFrameGate
            && missingSecondGateFresh.AlternateSecondFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                    .MissingFreshFrameMinus10,
                FrameMinus10ReadReached: true,
                FrameMinus10Address: 0x1F0,
                FrameMinus10Captured: false,
                FrameMinus10: null,
                CompareReached: false,
                JnzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9122
            }
            && missingSecondGateFresh.OrderedUpstreamWrites.Length == 2
            && missingSecondGateFresh.OrderedUpstreamWrites[0]
                == new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    0x012C910F, 0x1F0, 0xDEADBEEF)
            && missingSecondGateFresh.OrderedUpstreamWrites[1]
                == new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    0x012C9115, 0x1EC, 0x200)
            && !missingSecondGateFresh.UnreachedFreshFrameMinus10CaptureProvided
            && !missingSecondGateFresh.ExactCleanExitAdmitted
            && missingSecondGateFresh.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleePairedFrameGateLifecyclePlan equalFirstGateWithCapture =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                CleanEqualCapture(freshFrameMinus10: 0x100));
        check("Equal 911C stays before its opaque call and rejects the later frame capture",
            equalFirstGateWithCapture.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsEqualWithDownstreamCapture
            && equalFirstGateWithCapture.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .CompareEqualToOpaqueCallBoundary
            && !equalFirstGateWithCapture.AlternateSecondFrameGateReached
            && equalFirstGateWithCapture.AlternateSecondFrameGate is null
            && !equalFirstGateWithCapture.ExactCleanHandoffToAlternateSecondFrameGate
            && equalFirstGateWithCapture.DerivedSecondGateEntryEDX is null
            && equalFirstGateWithCapture.DerivedSecondGateEntryEBP is null
            && equalFirstGateWithCapture.UnreachedFreshFrameMinus10CaptureProvided
            && !equalFirstGateWithCapture.ExactCleanExitAdmitted
            && equalFirstGateWithCapture.OrderedUpstreamWrites.Length == 2
            && equalFirstGateWithCapture.StopAtRawFileOffsetExclusive == 0x012C911C
            && !equalFirstGateWithCapture.FirstOpaqueCallReached
            && !equalFirstGateWithCapture.SecondOpaqueCallReached);

        CapacityFinalCalleePairedFrameGateLifecyclePlan equalFirstGate =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                CleanEqualCapture(freshFrameMinus10: null));
        check("Clean equal 911C remains an exact bounded stop when no later capture is supplied",
            equalFirstGate.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsEqualToOpaqueCallBoundary
            && !equalFirstGate.AlternateSecondFrameGateReached
            && !equalFirstGate.UnreachedFreshFrameMinus10CaptureProvided
            && equalFirstGate.ExactCleanExitAdmitted
            && equalFirstGate.StopAtRawFileOffsetExclusive == 0x012C911C
            && !equalFirstGate.FirstOpaqueCallReached);

        CapacityFinalCalleePairedFrameGateLifecyclePlan missingUpstreamRead =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                new(
                    new CapacityFinalCalleeAlternateFrameLoadsCapture(
                        EntryEDX: 0x100,
                        EntryEBP: 0x200,
                        FreshEdxPlus8: 0xDEADBEEF,
                        FreshEdxPlusC: null),
                    FreshFrameMinus10: 0x100));
        check("A missing upstream second load cannot be bypassed by the later fresh frame",
            missingUpstreamRead.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsIncompleteWithDownstreamCapture
            && missingUpstreamRead.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC
            && missingUpstreamRead.AlternateFrameLoads.FirstWriteReached
            && !missingUpstreamRead.AlternateFrameLoads.SecondWriteReached
            && missingUpstreamRead.OrderedUpstreamWrites.Length == 1
            && missingUpstreamRead.StopAtRawFileOffsetExclusive == 0x012C9112
            && !missingUpstreamRead.AlternateSecondFrameGateReached
            && missingUpstreamRead.AlternateSecondFrameGate is null
            && !missingUpstreamRead.ExactCleanHandoffToAlternateSecondFrameGate
            && missingUpstreamRead.DerivedSecondGateEntryEDX is null
            && missingUpstreamRead.DerivedSecondGateEntryEBP is null
            && missingUpstreamRead.UnreachedFreshFrameMinus10CaptureProvided
            && !missingUpstreamRead.ExactCleanExitAdmitted);

        CapacityFinalCalleePairedFrameGateLifecyclePlan missingUpstreamEbp =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                new(
                    new CapacityFinalCalleeAlternateFrameLoadsCapture(
                        EntryEDX: 0x100,
                        EntryEBP: null,
                        FreshEdxPlus8: 0xDEADBEEF,
                        FreshEdxPlusC: 0x200),
                    FreshFrameMinus10: 0x100));
        check("Missing upstream EBP prevents both frame stores and all later gate reads",
            missingUpstreamEbp.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsIncompleteWithDownstreamCapture
            && missingUpstreamEbp.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .MissingEntryEBPForFirstWrite
            && missingUpstreamEbp.AlternateFrameLoads.FreshEdxPlus8Captured
            && !missingUpstreamEbp.AlternateFrameLoads.FirstWriteReached
            && !missingUpstreamEbp.AlternateFrameLoads.SecondReadReached
            && missingUpstreamEbp.AlternateFrameLoads
                .UnreachedFreshEdxPlusCCaptureProvided
            && missingUpstreamEbp.OrderedUpstreamWrites.IsEmpty
            && !missingUpstreamEbp.AlternateSecondFrameGateReached
            && missingUpstreamEbp.DerivedSecondGateEntryEDX is null
            && missingUpstreamEbp.DerivedSecondGateEntryEBP is null
            && missingUpstreamEbp.UnreachedFreshFrameMinus10CaptureProvided
            && missingUpstreamEbp.StopAtRawFileOffsetExclusive == 0x012C910F);

        CapacityFinalCalleePairedFrameGateLifecyclePlan missingUpstreamEdx =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                new(
                    new CapacityFinalCalleeAlternateFrameLoadsCapture(
                        EntryEDX: null,
                        EntryEBP: 0x200,
                        FreshEdxPlus8: 0xDEADBEEF,
                        FreshEdxPlusC: 0x200),
                    FreshFrameMinus10: 0x100));
        check("Missing upstream EDX marks every unreached input without deriving gate registers",
            missingUpstreamEdx.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsIncompleteWithDownstreamCapture
            && missingUpstreamEdx.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEDX
            && missingUpstreamEdx.AlternateFrameLoads
                .UnreachedFreshEdxPlus8CaptureProvided
            && missingUpstreamEdx.AlternateFrameLoads
                .UnreachedEntryEBPCaptureProvided
            && missingUpstreamEdx.AlternateFrameLoads
                .UnreachedFreshEdxPlusCCaptureProvided
            && missingUpstreamEdx.OrderedUpstreamWrites.IsEmpty
            && !missingUpstreamEdx.AlternateSecondFrameGateReached
            && missingUpstreamEdx.DerivedSecondGateEntryEDX is null
            && missingUpstreamEdx.DerivedSecondGateEntryEBP is null
            && missingUpstreamEdx.UnreachedFreshFrameMinus10CaptureProvided
            && missingUpstreamEdx.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleePairedFrameGateLifecyclePlan missingUpstreamFirstRead =
            CapacityFinalCalleePairedFrameGateLifecycleProjection.Project(
                new(
                    new CapacityFinalCalleeAlternateFrameLoadsCapture(
                        EntryEDX: 0x100,
                        EntryEBP: 0x200,
                        FreshEdxPlus8: null,
                        FreshEdxPlusC: 0x200),
                    FreshFrameMinus10: 0x100));
        check("Missing upstream first read rejects every later supplied capture",
            missingUpstreamFirstRead.Outcome
                == CapacityFinalCalleePairedFrameGateLifecycleOutcome
                    .AlternateFrameLoadsIncompleteWithDownstreamCapture
            && missingUpstreamFirstRead.AlternateFrameLoads.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .MissingFreshEdxPlus8
            && missingUpstreamFirstRead.AlternateFrameLoads.FirstReadReached
            && missingUpstreamFirstRead.AlternateFrameLoads
                .UnreachedEntryEBPCaptureProvided
            && missingUpstreamFirstRead.AlternateFrameLoads
                .UnreachedFreshEdxPlusCCaptureProvided
            && missingUpstreamFirstRead.OrderedUpstreamWrites.IsEmpty
            && !missingUpstreamFirstRead.AlternateSecondFrameGateReached
            && missingUpstreamFirstRead.DerivedSecondGateEntryEDX is null
            && missingUpstreamFirstRead.DerivedSecondGateEntryEBP is null
            && missingUpstreamFirstRead.UnreachedFreshFrameMinus10CaptureProvided
            && missingUpstreamFirstRead.StopAtRawFileOffsetExclusive == 0x012C910C);
    }

    private static CapacityFinalCalleePairedFrameGateLifecycleCapture CleanUnequalCapture(
        uint? freshFrameMinus10) =>
        new(
            new CapacityFinalCalleeAlternateFrameLoadsCapture(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0xDEADBEEF,
                FreshEdxPlusC: 0x200),
            freshFrameMinus10);

    private static CapacityFinalCalleePairedFrameGateLifecycleCapture CleanEqualCapture(
        uint? freshFrameMinus10) =>
        new(
            new CapacityFinalCalleeAlternateFrameLoadsCapture(
                EntryEDX: 0x100,
                EntryEBP: 0x200,
                FreshEdxPlus8: 0xDEADBEEF,
                FreshEdxPlusC: 0x100),
            freshFrameMinus10);
}
