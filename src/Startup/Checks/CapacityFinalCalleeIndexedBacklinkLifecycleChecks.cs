using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeIndexedBacklinkLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedBacklinkLifecyclePlan upstreamEqual =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(freshFrameMinus4: 0, freshFrameMinus10: 0x01EDADF0));
        check("A clean upstream equal branch stops at 9160 with its indexed store",
            upstreamEqual.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UpstreamEqualTo9160
            && upstreamEqual.Upstream.Outcome
                == CapacityFinalCalleeAlternateIndexedFrameGateOutcome.EqualToBranchTarget
            && !upstreamEqual.BacklinkGateReached
            && upstreamEqual.BacklinkGate is null
            && upstreamEqual.DerivedBacklinkEntryECX is null
            && upstreamEqual.ExactCleanExitAdmitted
            && upstreamEqual.ExactCleanBypassAdmitted
            && upstreamEqual.StopAtRawFileOffsetExclusive == 0x012C9160
            && upstreamEqual.Upstream.Writes.Length == 1
            && upstreamEqual.Upstream.Writes[0]
                == new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                    0x012C914A, 0xFFFFFFF4, 0x01EDADF0)
            && !upstreamEqual.OpaqueCallReached);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan upstreamEqualContaminated =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x01EDADF0,
                    callerEBX: 1,
                    callerEDX: 2,
                    freshField: 3));
        check("An upstream equal branch rejects every downstream capture before the child",
            upstreamEqualContaminated.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .UpstreamEqualTo9160WithDownstreamCapture
            && !upstreamEqualContaminated.BacklinkGateReached
            && upstreamEqualContaminated.BacklinkGate is null
            && upstreamEqualContaminated.UnreachedCallerEBXCaptureProvided
            && upstreamEqualContaminated.UnreachedCallerEDXCaptureProvided
            && upstreamEqualContaminated.UnreachedFreshFieldCaptureProvided
            && !upstreamEqualContaminated.ExactCleanExitAdmitted
            && !upstreamEqualContaminated.ExactCleanBypassAdmitted
            && upstreamEqualContaminated.StopAtRawFileOffsetExclusive == 0x012C9160
            && upstreamEqualContaminated.Upstream.Writes.Length == 1);

        CheckMissingUpstream(check);
        CheckMissingDownstream(check);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan belowClean =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(freshFrameMinus4: 0, freshFrameMinus10: 0x10, callerEBX: 0x20));
        check("A clean child JB below route stops at 915A without later inputs",
            belowClean.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.BelowTo915A
            && belowClean.ExactCleanHandoffToBacklinkGate
            && belowClean.DerivedBacklinkEntryECX == 0x10
            && belowClean.BacklinkGate is
            {
                Outcome: CapacityFinalCalleeAlternateBacklinkGateOutcome
                    .BelowToOpaqueCallBoundary,
                FirstCompareLeft: 0x10,
                FirstCompareRight: 0x20,
                FirstCompareResult: 0xFFFFFFF0,
                FirstCompareFlags: {
                    Carry: true,
                    Parity: true,
                    Auxiliary: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                JbReached: true,
                JbTaken: true,
                JbSelectedDestinationRawFileOffset: 0x012C915A,
                EntryEDXReadReached: false,
                FreshFieldReadReached: false,
                SecondCompareReached: false,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C915A
            }
            && !belowClean.LateCallerEDXCaptureProvided
            && !belowClean.LateFreshFieldCaptureProvided
            && belowClean.ExactCleanExitAdmitted
            && !belowClean.ExactCleanBypassAdmitted
            && belowClean.Upstream.Writes.Length == 1
            && belowClean.StopAtRawFileOffsetExclusive == 0x012C915A
            && !belowClean.OpaqueCallReached);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan belowContaminated =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x10,
                    callerEBX: 0x20,
                    callerEDX: 0x1234,
                    freshField: 0x5678));
        check("Below rejects supplied EDX and field captures without feeding unreachable child inputs",
            belowContaminated.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .BelowTo915AWithUnusedCapture
            && belowContaminated.BacklinkGate is
            {
                JbReached: true,
                JbTaken: true,
                EntryEDXReadReached: false,
                FreshFieldReadReached: false,
                UnusedEntryEDXCaptureProvided: false,
                UnusedFreshFieldCaptureProvided: false,
                StopAtRawFileOffsetExclusive: 0x012C915A
            }
            && belowContaminated.UnusedCallerEDXCaptureProvided
            && belowContaminated.UnusedFreshFieldCaptureProvided
            && !belowContaminated.ExactCleanExitAdmitted
            && belowContaminated.Upstream.Writes.Length == 1);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan secondCompareEqual =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 0xAABBCCDD,
                    freshField: 0xAABBCCDD));
        check("The complete child second CMP takes the exact 9160 bypass",
            secondCompareEqual.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.EqualTo9160
            && secondCompareEqual.BacklinkGate is
            {
                FirstCompareReached: true,
                FirstCompareLeft: 0x20,
                FirstCompareRight: 0x10,
                FirstCompareFlags: {
                    Carry: false,
                    Parity: false,
                    Auxiliary: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                },
                JbTaken: false,
                EntryEDXReadReached: true,
                EntryEDXCaptured: true,
                FreshFieldReadReached: true,
                FreshFieldAddress: 0x2C,
                FreshFieldCaptured: true,
                SecondCompareReached: true,
                SecondCompareLeft: 0xAABBCCDD,
                SecondCompareRight: 0xAABBCCDD,
                SecondCompareResult: 0,
                SecondCompareFlags: {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                JzReached: true,
                JzTaken: true,
                JzSelectedDestinationRawFileOffset: 0x012C9160,
                ExactCleanExitAdmitted: true,
                ExactCleanBypassAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C9160
            }
            && secondCompareEqual.DerivedBacklinkEntryECX == 0x20
            && secondCompareEqual.ExactCleanExitAdmitted
            && secondCompareEqual.ExactCleanBypassAdmitted
            && secondCompareEqual.Upstream.Writes.Length == 1
            && secondCompareEqual.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan secondCompareUnequal =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0x0FC24A42,
                    freshFrameMinus10: 0x7FFFFFFF,
                    callerEBX: 0x10,
                    callerEDX: 3,
                    freshField: 2));
        check("The complete child second CMP rejects unequal at 915A with all flags",
            secondCompareUnequal.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UnequalTo915A
            && secondCompareUnequal.Upstream.CompareResult == 0xFFFFFFFF
            && secondCompareUnequal.Upstream.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && secondCompareUnequal.BacklinkGate is
            {
                FirstCompareReached: true,
                JbTaken: false,
                SecondCompareReached: true,
                SecondCompareLeft: 2,
                SecondCompareRight: 3,
                SecondCompareResult: 0xFFFFFFFF,
                SecondCompareFlags: {
                    Carry: true,
                    Parity: true,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                JzReached: true,
                JzTaken: false,
                JzSelectedDestinationRawFileOffset: 0x012C915A,
                ExactCleanExitAdmitted: true,
                ExactCleanBypassAdmitted: false,
                StopAtRawFileOffsetExclusive: 0x012C915A
            }
            && secondCompareUnequal.ExactCleanExitAdmitted
            && !secondCompareUnequal.ExactCleanBypassAdmitted
            && secondCompareUnequal.Upstream.Writes.Length == 1
            && secondCompareUnequal.Upstream.Writes[0].Value == 0x80000000
            && secondCompareUnequal.StopAtRawFileOffsetExclusive == 0x012C915A);

        CheckFreshAddressIndependence(check);
    }

    private static void CheckMissingUpstream(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedBacklinkLifecycleCapture[] missing = {
            Capture(
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                upstreamEntryEbp: null,
                callerEBX: 1,
                callerEDX: 2,
                freshField: 3),
            Capture(
                freshFrameMinus4: null,
                freshFrameMinus10: 0,
                callerEBX: 1,
                callerEDX: 2,
                freshField: 3),
            Capture(
                freshFrameMinus4: 0,
                freshFrameMinus10: null,
                callerEBX: 1,
                callerEDX: 2,
                freshField: 3)
        };
        CapacityFinalCalleeAlternateIndexedFrameGateOutcome[] expected = {
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingEntryEBP,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus4,
            CapacityFinalCalleeAlternateIndexedFrameGateOutcome.MissingFreshFrameMinus10
        };
        uint[] stops = { 0x012C913D, 0x012C913D, 0x012C9140 };

        for (int i = 0; i < missing.Length; i++)
        {
            CapacityFinalCalleeIndexedBacklinkLifecyclePlan plan =
                CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(missing[i]);
            check($"Missing upstream prerequisite {i + 1} cannot be bypassed by child captures",
                plan.Outcome
                    == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                        .UpstreamIncompleteWithDownstreamCapture
                && plan.Upstream.Outcome == expected[i]
                && !plan.BacklinkGateReached
                && plan.BacklinkGate is null
                && plan.UnreachedCallerEBXCaptureProvided
                && plan.UnreachedCallerEDXCaptureProvided
                && plan.UnreachedFreshFieldCaptureProvided
                && !plan.ExactCleanExitAdmitted
                && plan.StopAtRawFileOffsetExclusive == stops[i]
                && plan.Upstream.Writes.IsEmpty);
        }

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan noCaptures =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: null,
                    freshFrameMinus10: null,
                    upstreamEntryEbp: null));
        check("Missing upstream prerequisites without later captures remain an incomplete stop",
            noCaptures.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.UpstreamIncomplete
            && !noCaptures.BacklinkGateReached
            && !noCaptures.UnreachedCallerEBXCaptureProvided
            && !noCaptures.UnreachedCallerEDXCaptureProvided
            && !noCaptures.UnreachedFreshFieldCaptureProvided
            && !noCaptures.ExactCleanExitAdmitted
            && noCaptures.StopAtRawFileOffsetExclusive == 0x012C913D);
    }

    private static void CheckMissingDownstream(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedBacklinkLifecyclePlan missingEbx =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: null,
                    callerEDX: 3,
                    freshField: 4));
        check("Missing child EBX blocks the first CMP and rejects later captures",
            missingEbx.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.MissingBacklinkEntryEBX
            && missingEbx.BacklinkGate is
            {
                Outcome: CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEBX,
                FirstCompareReached: false,
                LateEntryEDXCaptureProvided: false,
                LateFreshFieldCaptureProvided: false,
                StopAtRawFileOffsetExclusive: 0x012C9151
            }
            && missingEbx.LateCallerEDXCaptureProvided
            && missingEbx.LateFreshFieldCaptureProvided
            && !missingEbx.ExactCleanExitAdmitted
            && missingEbx.Upstream.Writes.Length == 1
            && missingEbx.StopAtRawFileOffsetExclusive == 0x012C9151);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan missingEdx =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: null,
                    freshField: 4));
        check("Missing child EDX blocks only the second CMP after the first compare",
            missingEdx.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome.MissingBacklinkEntryEDX
            && missingEdx.BacklinkGate is
            {
                FirstCompareReached: true,
                JbTaken: false,
                EntryEDXReadReached: true,
                EntryEDXCaptured: false,
                FreshFieldReadReached: true,
                FreshFieldCaptured: true,
                SecondCompareReached: false,
                JzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9155
            }
            && !missingEdx.ExactCleanExitAdmitted
            && missingEdx.Upstream.Writes.Length == 1
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C9155);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan missingFresh =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0x20,
                    callerEBX: 0x10,
                    callerEDX: 4,
                    freshField: null));
        check("Missing child field blocks only the second CMP and preserves its read address",
            missingFresh.Outcome
                == CapacityFinalCalleeIndexedBacklinkLifecycleOutcome
                    .MissingFreshFieldAtEcxPlusC
            && missingFresh.BacklinkGate is
            {
                FirstCompareReached: true,
                JbTaken: false,
                EntryEDXCaptured: true,
                FreshFieldReadReached: true,
                FreshFieldAddress: 0x2C,
                FreshFieldCaptured: false,
                SecondCompareReached: false,
                JzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9155
            }
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.Upstream.Writes.Length == 1
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9155);
    }

    private static void CheckFreshAddressIndependence(Action<string, bool> check)
    {
        CapacityFinalCalleeIndexedBacklinkLifecyclePlan storeAlias =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    upstreamEntryEbp: 0x100,
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0xE8,
                    callerEBX: 0xE8,
                    callerEDX: 0x12345678,
                    freshField: 0xDEADBEEF));
        check("Fresh ECX+C may alias the indexed local store but uses its independent capture",
            storeAlias.Upstream.Writes.Length == 1
            && storeAlias.Upstream.Writes[0].Address == 0xF4
            && storeAlias.Upstream.Writes[0].Value == 0x01EDADF0
            && storeAlias.DerivedBacklinkEntryECX == 0xE8
            && storeAlias.BacklinkGate?.FreshFieldAddress == 0xF4
            && storeAlias.BacklinkGate?.FreshFieldValue == 0xDEADBEEF
            && storeAlias.BacklinkGate?.FreshFieldValue
                != storeAlias.Upstream.Writes[0].Value
            && storeAlias.BacklinkGate?.SecondCompareLeft == 0xDEADBEEF
            && storeAlias.BacklinkGate?.SecondCompareRight == 0x12345678
            && storeAlias.StopAtRawFileOffsetExclusive == 0x012C915A);

        CapacityFinalCalleeIndexedBacklinkLifecyclePlan wrappedField =
            CapacityFinalCalleeIndexedBacklinkLifecycleProjection.Project(
                Capture(
                    freshFrameMinus4: 0,
                    freshFrameMinus10: 0xFFFFFFF4,
                    callerEBX: 0xFFFFFFF0,
                    callerEDX: 0,
                    freshField: 1));
        check("Derived ECX and its fresh field address wrap independently as DWORD values",
            wrappedField.DerivedBacklinkEntryECX == 0xFFFFFFF4
            && wrappedField.BacklinkGate?.FreshFieldAddress == 0
            && wrappedField.BacklinkGate?.FreshFieldValue == 1
            && wrappedField.BacklinkGate?.FirstCompareLeft == 0xFFFFFFF4
            && wrappedField.BacklinkGate?.FirstCompareRight == 0xFFFFFFF0
            && wrappedField.BacklinkGate?.JbTaken == false
            && wrappedField.BacklinkGate?.SecondCompareLeft == 1
            && wrappedField.BacklinkGate?.SecondCompareRight == 0
            && wrappedField.StopAtRawFileOffsetExclusive == 0x012C915A);
    }

    private static CapacityFinalCalleeIndexedBacklinkLifecycleCapture Capture(
        uint? freshFrameMinus4,
        uint? freshFrameMinus10,
        uint? upstreamEntryEbp = 0,
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
