using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePairedShiftedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The paired lifecycle connects only the unequal 912D edge to the shifted gate",
            CapacityFinalCalleePairedShiftedLifecycleProjection
                .FirstOpaqueCallBoundaryRawFileOffsetExclusive == 0x012C911C
            && CapacityFinalCalleePairedFrameGateLifecycleProjection
                .UnequalSecondGateBoundaryRawFileOffsetExclusive == 0x012C912D
            && CapacityFinalCalleePairedShiftedLifecycleProjection
                .ShiftedFrameGateEntryRawFileOffset == 0x012C912D
            && CapacityFinalCalleePairedShiftedLifecycleProjection
                .EqualSecondGateBoundaryRawFileOffsetExclusive == 0x012C9127
            && CapacityFinalCalleePairedShiftedLifecycleProjection
                .ShiftedUnequalOpaqueCallBoundaryRawFileOffset == 0x012C9137
            && CapacityFinalCalleePairedShiftedLifecycleProjection
                .ShiftedEqualDestinationRawFileOffset == 0x012C913D);

        CapacityFinalCalleePairedShiftedLifecyclePlan clean911C =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanEqualFrameLoads(), freshFrameMinus4: null, shiftedEntryECX: null));
        check("A clean 911C exit is bounded and does not reach either later gate",
            clean911C.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .EqualTo911COpaqueCallBoundary
            && clean911C.PairedFrameGatePlan.AlternateFrameLoads.Writes.Length == 2
            && clean911C.PairedFrameGatePlan.AlternateSecondFrameGate is null
            && clean911C.ShiftedFrameGatePlan is null
            && clean911C.ExactCleanExitAdmitted
            && clean911C.StopAtRawFileOffsetExclusive == 0x012C911C
            && !clean911C.FirstOpaqueCallReached
            && !clean911C.SecondOpaqueCallReached
            && !clean911C.ShiftedOpaqueCallReached);

        CapacityFinalCalleePairedShiftedLifecyclePlan contaminated911C =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanEqualFrameLoads(), freshFrameMinus4: 0x10, shiftedEntryECX: 0x80));
        check("Later shifted captures contaminate 911C and are rejected before either child gate",
            contaminated911C.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .EqualTo911COpaqueCallBoundaryContaminated
            && contaminated911C.PairedFrameGatePlan.AlternateSecondFrameGate is null
            && contaminated911C.ShiftedFrameGatePlan is null
            && contaminated911C.UnreachedFreshFrameMinus4CaptureProvided
            && contaminated911C.UnreachedShiftedEntryECXCaptureProvided
            && contaminated911C.LateFreshFrameMinus4CaptureProvided
            && contaminated911C.LateShiftedEntryECXCaptureProvided
            && !contaminated911C.ExactCleanExitAdmitted
            && contaminated911C.StopAtRawFileOffsetExclusive == 0x012C911C);

        CapacityFinalCalleePairedShiftedLifecyclePlan contaminated911CFrame10 =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanEqualFrameLoads(), freshFrameMinus10: 0x100,
                    freshFrameMinus4: null, shiftedEntryECX: null));
        check("A supplied later EBP-10 read also contaminates the 911C bounded exit",
            contaminated911CFrame10.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .EqualTo911COpaqueCallBoundaryContaminated
            && contaminated911CFrame10.PairedFrameGatePlan
                .UnreachedFreshFrameMinus10CaptureProvided
            && contaminated911CFrame10.ShiftedFrameGatePlan is null
            && !contaminated911CFrame10.ExactCleanExitAdmitted
            && contaminated911CFrame10.StopAtRawFileOffsetExclusive == 0x012C911C);

        CapacityFinalCalleePairedShiftedLifecyclePlan clean9127 =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x100,
                    freshFrameMinus4: null, shiftedEntryECX: null));
        check("A clean 9127 exit preserves the paired writes and does not reach the shifted gate",
            clean9127.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .EqualTo9127OpaqueCallBoundary
            && clean9127.PairedFrameGatePlan.AlternateSecondFrameGate is
            {
                JnzReached: true,
                JnzTaken: false,
                JnzSelectedDestinationRawFileOffset: 0x012C9127,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C9127
            }
            && clean9127.OrderedUpstreamWrites.SequenceEqual(ExpectedWrites())
            && clean9127.ShiftedFrameGatePlan is null
            && clean9127.ExactCleanExitAdmitted
            && clean9127.StopAtRawFileOffsetExclusive == 0x012C9127
            && !clean9127.SecondOpaqueCallReached);

        CapacityFinalCalleePairedShiftedLifecyclePlan contaminated9127 =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x100,
                    freshFrameMinus4: 0x10, shiftedEntryECX: 0x80));
        check("Later captures contaminate the bounded 9127 edge before the shifted gate",
            contaminated9127.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .EqualTo9127OpaqueCallBoundaryContaminated
            && contaminated9127.PairedFrameGatePlan.AlternateSecondFrameGate is not null
            && contaminated9127.ShiftedFrameGatePlan is null
            && contaminated9127.UnreachedFreshFrameMinus4CaptureProvided
            && contaminated9127.UnreachedShiftedEntryECXCaptureProvided
            && contaminated9127.LateFreshFrameMinus4CaptureProvided
            && contaminated9127.LateShiftedEntryECXCaptureProvided
            && !contaminated9127.ExactCleanExitAdmitted
            && contaminated9127.StopAtRawFileOffsetExclusive == 0x012C9127);

        CapacityFinalCalleePairedShiftedLifecyclePlan missingUpstream =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(
                    new CapacityFinalCalleeAlternateFrameLoadsCapture(
                        EntryEDX: 0x100,
                        EntryEBP: 0x200,
                        FreshEdxPlus8: 0xDEADBEEF,
                        FreshEdxPlusC: null),
                    freshFrameMinus10: 0x101,
                    freshFrameMinus4: 0x10,
                    shiftedEntryECX: 0x80));
        check("Missing upstream paired input cannot be bypassed with later frame or caller captures",
            missingUpstream.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome.UpstreamGateBlocked
            && missingUpstream.PairedFrameGatePlan.AlternateFrameLoads
                .Outcome == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC
            && missingUpstream.OrderedUpstreamWrites.Length == 1
            && missingUpstream.PairedFrameGatePlan.AlternateSecondFrameGate is null
            && missingUpstream.ShiftedFrameGatePlan is null
            && missingUpstream.DerivedShiftedEntryEBP is null
            && missingUpstream.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstream.UnreachedShiftedEntryECXCaptureProvided
            && !missingUpstream.ExactCleanExitAdmitted
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C9112);

        CapacityFinalCalleePairedShiftedLifecyclePlan missingSecondGateFrame =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: null,
                    freshFrameMinus4: 0x10, shiftedEntryECX: 0x80));
        check("A missing upstream fresh EBP-10 capture cannot be bypassed by shifted inputs",
            missingSecondGateFrame.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome.UpstreamGateBlocked
            && missingSecondGateFrame.PairedFrameGatePlan.AlternateSecondFrameGate is
            {
                Outcome: CapacityFinalCalleeAlternateSecondFrameGateOutcome
                    .MissingFreshFrameMinus10,
                StopAtRawFileOffsetExclusive: 0x012C9122
            }
            && missingSecondGateFrame.ShiftedFrameGatePlan is null
            && missingSecondGateFrame.DerivedShiftedEntryEBP is null
            && missingSecondGateFrame.UnreachedFreshFrameMinus4CaptureProvided
            && missingSecondGateFrame.UnreachedShiftedEntryECXCaptureProvided
            && !missingSecondGateFrame.ExactCleanExitAdmitted
            && missingSecondGateFrame.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleePairedShiftedLifecyclePlan missingFresh4 =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x101,
                    freshFrameMinus4: null, shiftedEntryECX: 0x80));
        check("Missing shifted EBP-4 stops at its own read and retains the selected-route EBP",
            missingFresh4.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingFresh4.OrderedUpstreamWrites.SequenceEqual(ExpectedWrites())
            && missingFresh4.ShiftedFrameGatePlan is
            {
                Outcome: CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .MissingFreshFrameMinus4,
                EntryEBPCaptured: true,
                FrameMinus4ReadReached: true,
                FrameMinus4Address: 0x1FC,
                FrameMinus4Captured: false,
                ShiftReached: false,
                UnreachedEntryECXCaptureProvided: true,
                LateEntryECXCaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C912D
            }
            && missingFresh4.ShiftedFrameGateReached
            && missingFresh4.DerivedShiftedEntryEBP == 0x200
            && !missingFresh4.UnreachedFreshFrameMinus4CaptureProvided
            && missingFresh4.UnreachedShiftedEntryECXCaptureProvided
            && missingFresh4.LateShiftedEntryECXCaptureProvided
            && !missingFresh4.ExactCleanExitAdmitted
            && missingFresh4.StopAtRawFileOffsetExclusive == 0x012C912D);

        CapacityFinalCalleePairedShiftedLifecyclePlan missingECX =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x101,
                    freshFrameMinus4: 0xE0000001, shiftedEntryECX: null));
        check("Missing caller ECX stops at CMP after preserving the fresh SHL value and flags",
            missingECX.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingECX.ShiftedFrameGatePlan is
            {
                FrameMinus4Address: 0x1FC,
                FrameMinus4: 0xE0000001,
                ShiftReached: true,
                ShiftResult: 8,
                ShiftFlags: {
                    Carry: true,
                    Parity: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: false,
                    Overflow: null
                },
                EntryECXCaptured: false,
                CompareReached: false,
                JzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9133
            }
            && missingECX.DerivedShiftedEntryEBP == 0x200
            && !missingECX.UnreachedShiftedEntryECXCaptureProvided
            && !missingECX.LateShiftedEntryECXCaptureProvided
            && !missingECX.ExactCleanExitAdmitted
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9133);

        CapacityFinalCalleePairedShiftedLifecyclePlan wrappedAlias =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 1,
                    freshFrameMinus4: 0xE0000001, shiftedEntryECX: 8));
        check("The EBP-10 alias stays fresh while the independent EBP-4 reload wraps through SHL",
            wrappedAlias.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedValueEqualTo913D
            && wrappedAlias.OrderedUpstreamWrites.SequenceEqual(ExpectedWrites())
            && wrappedAlias.PairedFrameGatePlan.AlternateSecondFrameGate is
            {
                FrameMinus10Address: 0x1F0,
                FrameMinus10: 1
            }
            && wrappedAlias.ShiftedFrameGatePlan is
            {
                FrameMinus4Address: 0x1FC,
                FrameMinus4: 0xE0000001,
                ShiftResult: 8,
                CompareLeft: 8,
                CompareRight: 8,
                CompareResult: 0,
                CompareFlags: {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                JzTaken: true,
                JzSelectedDestinationRawFileOffset: 0x012C913D,
                Writes.IsEmpty: true
            }
            && wrappedAlias.PairedFrameGatePlan.AlternateSecondFrameGate?
                .FrameMinus10Address == wrappedAlias.OrderedUpstreamWrites[0].Address
            && wrappedAlias.PairedFrameGatePlan.AlternateSecondFrameGate?
                .FrameMinus10 != wrappedAlias.OrderedUpstreamWrites[0].Value
            && wrappedAlias.Capture.FreshFrameMinus4 == 0xE0000001
            && wrappedAlias.ExactCleanExitAdmitted
            && wrappedAlias.StopAtRawFileOffsetExclusive == 0x012C913D);

        CapacityFinalCalleePairedShiftedLifecyclePlan cleanEqualShift =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x101,
                    freshFrameMinus4: 0xE0000001, shiftedEntryECX: 8));
        check("The clean shifted equality branch preserves exact 913D and defined flags",
            cleanEqualShift.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedValueEqualTo913D
            && cleanEqualShift.ShiftedFrameGatePlan?.ShiftFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && cleanEqualShift.ShiftedFrameGatePlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && cleanEqualShift.ShiftedFrameGatePlan?.JzTaken == true
            && cleanEqualShift.ShiftedFrameGatePlan?.JzSelectedDestinationRawFileOffset
                == 0x012C913D
            && cleanEqualShift.ExactCleanExitAdmitted
            && cleanEqualShift.StopAtRawFileOffsetExclusive == 0x012C913D);

        CapacityFinalCalleePairedShiftedLifecyclePlan cleanUnequalShift =
            CapacityFinalCalleePairedShiftedLifecycleProjection.Project(
                Capture(CleanUnequalFrameLoads(), freshFrameMinus10: 0x101,
                    freshFrameMinus4: 0xF0000000, shiftedEntryECX: 0x7FFFFFFF));
        check("The clean shifted inequality branch preserves exact 9137 and defined flags",
            cleanUnequalShift.Outcome
                == CapacityFinalCalleePairedShiftedLifecycleOutcome
                    .ShiftedValueUnequalTo9137
            && cleanUnequalShift.ShiftedFrameGatePlan?.ShiftResult == 0x80000000
            && cleanUnequalShift.ShiftedFrameGatePlan?.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: null
            }
            && cleanUnequalShift.ShiftedFrameGatePlan?.CompareResult == 0xFFFFFFFF
            && cleanUnequalShift.ShiftedFrameGatePlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && cleanUnequalShift.ShiftedFrameGatePlan?.JzTaken == false
            && cleanUnequalShift.ShiftedFrameGatePlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9137
            && cleanUnequalShift.ExactCleanExitAdmitted
            && cleanUnequalShift.StopAtRawFileOffsetExclusive == 0x012C9137
            && !cleanUnequalShift.ShiftedOpaqueCallReached);
    }

    private static CapacityFinalCalleePairedShiftedLifecycleCapture Capture(
        CapacityFinalCalleeAlternateFrameLoadsCapture frameLoads,
        uint? freshFrameMinus10 = null,
        uint? freshFrameMinus4 = null,
        uint? shiftedEntryECX = null) =>
        new(
            new CapacityFinalCalleePairedFrameGateLifecycleCapture(
                frameLoads,
                freshFrameMinus10),
            freshFrameMinus4,
            shiftedEntryECX);

    private static CapacityFinalCalleeAlternateFrameLoadsCapture CleanEqualFrameLoads() =>
        new(
            EntryEDX: 0x100,
            EntryEBP: 0x200,
            FreshEdxPlus8: 0xDEADBEEF,
            FreshEdxPlusC: 0x100);

    private static CapacityFinalCalleeAlternateFrameLoadsCapture CleanUnequalFrameLoads() =>
        new(
            EntryEDX: 0x100,
            EntryEBP: 0x200,
            FreshEdxPlus8: 0xDEADBEEF,
            FreshEdxPlusC: 0x200);

    private static CapacityFinalCalleeAlternateFrameLoadsWrite[] ExpectedWrites() =>
        new[]
        {
            new CapacityFinalCalleeAlternateFrameLoadsWrite(
                0x012C910F, 0x1F0, 0xDEADBEEF),
            new CapacityFinalCalleeAlternateFrameLoadsWrite(
                0x012C9115, 0x1EC, 0x200)
        };
}
