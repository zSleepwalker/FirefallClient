using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSecondShiftedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The clean unequal 912D route joins the shifted gate with exact destinations",
            CapacityFinalCalleeSecondShiftedLifecycleProjection.SecondFrameGateEntryRawFileOffset
                == 0x012C9122
            && CapacityFinalCalleeSecondShiftedLifecycleProjection.ShiftedFrameGateEntryRawFileOffset
                == 0x012C912D
            && CapacityFinalCalleeSecondShiftedLifecycleProjection.EqualOpaqueCallBoundaryRawFileOffset
                == 0x012C9127
            && CapacityFinalCalleeSecondShiftedLifecycleProjection.ShiftedUnequalOpaqueCallBoundaryRawFileOffset
                == 0x012C9137
            && CapacityFinalCalleeSecondShiftedLifecycleProjection.ShiftedEqualDestinationRawFileOffset
                == 0x012C913D);

        CapacityFinalCalleeSecondShiftedLifecyclePlan missingUpstream =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: null,
                entryEBP: 0x200,
                freshFrameMinus10: 0,
                freshShiftedFrameMinus4: 0x1234,
                shiftedEntryECX: 0x5678));
        check("A missing upstream EDX read preserves its stop and rejects all shifted captures",
            missingUpstream.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.SecondFrameGateBlocked
            && missingUpstream.SecondFrameGatePlan.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEDX
            && !missingUpstream.SecondFrameGatePlan.FrameMinus10ReadReached
            && missingUpstream.SecondFrameGatePlan.UnreachedEntryEBPCaptureProvided
            && missingUpstream.SecondFrameGatePlan.LateFreshFrameMinus10CaptureProvided
            && missingUpstream.ShiftedFrameGatePlan is null
            && !missingUpstream.ShiftedFrameGateReached
            && missingUpstream.DerivedShiftedEntryEBP is null
            && missingUpstream.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && missingUpstream.UnreachedShiftedEntryECXCaptureProvided
            && missingUpstream.LateFreshShiftedFrameMinus4CaptureProvided
            && missingUpstream.LateShiftedEntryECXCaptureProvided
            && !missingUpstream.ExactCleanExitAdmitted
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C9122
            && !missingUpstream.OpaqueCallReached);

        CapacityFinalCalleeSecondShiftedLifecyclePlan missingUpstreamFrame =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0x100,
                entryEBP: 0x200,
                freshFrameMinus10: null,
                freshShiftedFrameMinus4: 0x1234,
                shiftedEntryECX: 0x5678));
        check("A missing upstream fresh EBP-10 read cannot be bypassed by shifted captures",
            missingUpstreamFrame.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.SecondFrameGateBlocked
            && missingUpstreamFrame.SecondFrameGatePlan.FrameMinus10ReadReached
            && missingUpstreamFrame.SecondFrameGatePlan.FrameMinus10Address == 0x1F0
            && !missingUpstreamFrame.SecondFrameGatePlan.FrameMinus10Captured
            && !missingUpstreamFrame.SecondFrameGatePlan.CompareReached
            && missingUpstreamFrame.ShiftedFrameGatePlan is null
            && missingUpstreamFrame.DerivedShiftedEntryEBP is null
            && missingUpstreamFrame.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && missingUpstreamFrame.UnreachedShiftedEntryECXCaptureProvided
            && !missingUpstreamFrame.ExactCleanExitAdmitted
            && missingUpstreamFrame.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleeSecondShiftedLifecyclePlan missingUpstreamEbp =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0x100,
                entryEBP: null,
                freshFrameMinus10: 0x100,
                freshShiftedFrameMinus4: 0x1234,
                shiftedEntryECX: 0x5678));
        check("Missing upstream EBP gates both the prior and shifted frame reads",
            missingUpstreamEbp.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.SecondFrameGateBlocked
            && missingUpstreamEbp.SecondFrameGatePlan.Outcome
                == CapacityFinalCalleeAlternateSecondFrameGateOutcome.MissingEntryEBP
            && missingUpstreamEbp.SecondFrameGatePlan.EntryEDXCaptured
            && !missingUpstreamEbp.SecondFrameGatePlan.FrameMinus10ReadReached
            && missingUpstreamEbp.SecondFrameGatePlan.LateFreshFrameMinus10CaptureProvided
            && missingUpstreamEbp.ShiftedFrameGatePlan is null
            && missingUpstreamEbp.DerivedShiftedEntryEBP is null
            && missingUpstreamEbp.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && missingUpstreamEbp.UnreachedShiftedEntryECXCaptureProvided
            && !missingUpstreamEbp.ExactCleanExitAdmitted
            && missingUpstreamEbp.StopAtRawFileOffsetExclusive == 0x012C9122);

        CapacityFinalCalleeSecondShiftedLifecyclePlan cleanEqual =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0,
                entryEBP: 0x300,
                freshFrameMinus10: 0,
                freshShiftedFrameMinus4: null,
                shiftedEntryECX: null));
        check("Equal 9122 comparison stops at the opaque 9127 boundary without entering it",
            cleanEqual.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome
                    .EqualTo9127OpaqueCallBoundary
            && cleanEqual.SecondFrameGatePlan.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && cleanEqual.SecondFrameGatePlan.JnzTaken == false
            && cleanEqual.SecondFrameGatePlan.JnzSelectedDestinationRawFileOffset
                == 0x012C9127
            && cleanEqual.ShiftedFrameGatePlan is null
            && !cleanEqual.ShiftedFrameGateReached
            && cleanEqual.DerivedShiftedEntryEBP is null
            && cleanEqual.SecondFrameGatePlan.Writes.IsEmpty
            && !cleanEqual.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && !cleanEqual.UnreachedShiftedEntryECXCaptureProvided
            && cleanEqual.ExactCleanExitAdmitted
            && cleanEqual.StopAtRawFileOffsetExclusive == 0x012C9127
            && !cleanEqual.OpaqueCallReached
            && !cleanEqual.SecondFrameGatePlan.OpaqueCallReached);

        CapacityFinalCalleeSecondShiftedLifecyclePlan contaminatedEqual =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0,
                entryEBP: 0x100,
                freshFrameMinus10: 0,
                freshShiftedFrameMinus4: 0x20000000,
                shiftedEntryECX: 0));
        check("Shifted captures contaminate equal 9127 and cannot enter the shifted gate",
            contaminatedEqual.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome
                    .EqualTo9127OpaqueCallBoundaryContaminated
            && contaminatedEqual.SecondFrameGatePlan.JnzTaken == false
            && contaminatedEqual.SecondFrameGatePlan.StopAtRawFileOffsetExclusive
                == 0x012C9127
            && contaminatedEqual.ShiftedFrameGatePlan is null
            && !contaminatedEqual.ShiftedFrameGateReached
            && contaminatedEqual.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && contaminatedEqual.UnreachedShiftedEntryECXCaptureProvided
            && contaminatedEqual.LateFreshShiftedFrameMinus4CaptureProvided
            && contaminatedEqual.LateShiftedEntryECXCaptureProvided
            && !contaminatedEqual.ExactCleanExitAdmitted
            && contaminatedEqual.StopAtRawFileOffsetExclusive == 0x012C9127
            && !contaminatedEqual.OpaqueCallReached);

        CapacityFinalCalleeSecondShiftedLifecyclePlan missingShiftedFrame =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0x100,
                entryEBP: 0x204,
                freshFrameMinus10: 0x200,
                freshShiftedFrameMinus4: null,
                shiftedEntryECX: 0x100));
        check("Clean 912D derives EBP only at the shifted fresh-frame consumer and late ECX stays flagged",
            missingShiftedFrame.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingShiftedFrame.SecondFrameGatePlan.JnzTaken == true
            && missingShiftedFrame.SecondFrameGatePlan.JnzSelectedDestinationRawFileOffset
                == 0x012C912D
            && missingShiftedFrame.ShiftedFrameGatePlan is
            {
                FrameMinus4ReadReached: true,
                FrameMinus4Address: 0x200,
                FrameMinus4Captured: false,
                ShiftReached: false,
                UnreachedFreshFrameMinus4CaptureProvided: false,
                UnreachedEntryECXCaptureProvided: true,
                LateFreshFrameMinus4CaptureProvided: false,
                LateEntryECXCaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C912D
            }
            && missingShiftedFrame.ShiftedFrameGateReached
            && missingShiftedFrame.DerivedShiftedEntryEBP == 0x204
            && !missingShiftedFrame.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && missingShiftedFrame.UnreachedShiftedEntryECXCaptureProvided
            && !missingShiftedFrame.LateFreshShiftedFrameMinus4CaptureProvided
            && missingShiftedFrame.LateShiftedEntryECXCaptureProvided
            && !missingShiftedFrame.ExactCleanExitAdmitted
            && missingShiftedFrame.StopAtRawFileOffsetExclusive == 0x012C912D);

        CapacityFinalCalleeSecondShiftedLifecyclePlan missingShiftedEcx =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0x101,
                entryEBP: 0x104,
                freshFrameMinus10: 0x100,
                freshShiftedFrameMinus4: 0x20000000,
                shiftedEntryECX: null));
        check("Missing shifted ECX preserves SHL flags and waits at CMP",
            missingShiftedEcx.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingShiftedEcx.ShiftedFrameGatePlan is
            {
                FrameMinus4Address: 0x100,
                FrameMinus4: 0x20000000,
                ShiftReached: true,
                ShiftResult: 0,
                ShiftFlags: {
                    Carry: true,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: null
                },
                EntryECXCaptured: false,
                CompareReached: false,
                CompareResult: null,
                JzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9133
            }
            && missingShiftedEcx.ShiftedFrameGateReached
            && missingShiftedEcx.DerivedShiftedEntryEBP == 0x104
            && !missingShiftedEcx.UnreachedFreshShiftedFrameMinus4CaptureProvided
            && !missingShiftedEcx.UnreachedShiftedEntryECXCaptureProvided
            && !missingShiftedEcx.LateShiftedEntryECXCaptureProvided
            && !missingShiftedEcx.ExactCleanExitAdmitted
            && missingShiftedEcx.StopAtRawFileOffsetExclusive == 0x012C9133);

        CapacityFinalCalleeSecondShiftedLifecyclePlan shiftedEqual =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0,
                entryEBP: 0x40,
                freshFrameMinus10: 1,
                freshShiftedFrameMinus4: 0xE0000001,
                shiftedEntryECX: 8));
        check("The independent fresh EBP-4 value wraps through SHL and selects exact 913D equality",
            shiftedEqual.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedValueEqualTo913D
            && shiftedEqual.SecondFrameGatePlan.FrameMinus10 == 1
            && shiftedEqual.ShiftedFrameGatePlan?.Capture.FreshFrameMinus4 == 0xE0000001
            && shiftedEqual.ShiftedFrameGatePlan?.Capture.EntryECX == 8
            && shiftedEqual.DerivedShiftedEntryEBP == 0x40
            && shiftedEqual.ShiftedFrameGatePlan?.ShiftResult == 8
            && shiftedEqual.ShiftedFrameGatePlan?.ShiftFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && shiftedEqual.ShiftedFrameGatePlan?.CompareResult == 0
            && shiftedEqual.ShiftedFrameGatePlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && shiftedEqual.ShiftedFrameGatePlan?.JzTaken == true
            && shiftedEqual.ShiftedFrameGatePlan?.JzSelectedDestinationRawFileOffset
                == 0x012C913D
            && shiftedEqual.ShiftedFrameGatePlan?.Writes.IsEmpty == true
            && shiftedEqual.ExactCleanExitAdmitted
            && shiftedEqual.StopAtRawFileOffsetExclusive == 0x012C913D
            && !shiftedEqual.OpaqueCallReached);

        CapacityFinalCalleeSecondShiftedLifecyclePlan shiftedUnequal =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0,
                entryEBP: 0x20,
                freshFrameMinus10: 1,
                freshShiftedFrameMinus4: 0xF0000000,
                shiftedEntryECX: 0x7FFFFFFF));
        check("Signed shifted comparison flags route unequal values to 9137 without entering the call",
            shiftedUnequal.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome
                    .ShiftedValueUnequalTo9137
            && shiftedUnequal.ShiftedFrameGatePlan?.ShiftResult == 0x80000000
            && shiftedUnequal.ShiftedFrameGatePlan?.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: null
            }
            && shiftedUnequal.ShiftedFrameGatePlan?.CompareResult == 0xFFFFFFFF
            && shiftedUnequal.ShiftedFrameGatePlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && shiftedUnequal.ShiftedFrameGatePlan?.JzTaken == false
            && shiftedUnequal.ShiftedFrameGatePlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9137
            && shiftedUnequal.ShiftedFrameGatePlan?.Writes.IsEmpty == true
            && shiftedUnequal.ExactCleanExitAdmitted
            && shiftedUnequal.StopAtRawFileOffsetExclusive == 0x012C9137
            && !shiftedUnequal.OpaqueCallReached);

        CapacityFinalCalleeSecondShiftedLifecyclePlan shiftedZero =
            CapacityFinalCalleeSecondShiftedLifecycleProjection.Project(Capture(
                entryEDX: 0,
                entryEBP: 0,
                freshFrameMinus10: 1,
                freshShiftedFrameMinus4: 0,
                shiftedEntryECX: 0));
        check("Zero shifted value keeps defined SHL flags and takes the exact equal branch",
            shiftedZero.Outcome
                == CapacityFinalCalleeSecondShiftedLifecycleOutcome.ShiftedValueEqualTo913D
            && shiftedZero.ShiftedFrameGatePlan?.FrameMinus4Address == 0xFFFFFFFC
            && shiftedZero.ShiftedFrameGatePlan?.ShiftResult == 0
            && shiftedZero.ShiftedFrameGatePlan?.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && shiftedZero.ShiftedFrameGatePlan?.CompareResult == 0
            && shiftedZero.ShiftedFrameGatePlan?.JzTaken == true
            && shiftedZero.ShiftedFrameGatePlan?.JzSelectedDestinationRawFileOffset
                == 0x012C913D
            && shiftedZero.ExactCleanExitAdmitted
            && shiftedZero.StopAtRawFileOffsetExclusive == 0x012C913D);
    }

    private static CapacityFinalCalleeSecondShiftedLifecycleCapture Capture(
        uint? entryEDX,
        uint? entryEBP,
        uint? freshFrameMinus10,
        uint? freshShiftedFrameMinus4,
        uint? shiftedEntryECX) =>
        new(
            new CapacityFinalCalleeAlternateSecondFrameGateCapture(
                entryEDX,
                entryEBP,
                freshFrameMinus10),
            freshShiftedFrameMinus4,
            shiftedEntryECX);
}
