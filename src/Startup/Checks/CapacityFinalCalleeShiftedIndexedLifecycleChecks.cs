using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeShiftedIndexedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The shifted and indexed gates connect only at exact 913D and preserve route targets",
            CapacityFinalCalleeShiftedIndexedLifecycleProjection
                .ShiftedFrameGateEntryRawFileOffset == 0x012C912D
            && CapacityFinalCalleeShiftedIndexedLifecycleProjection
                .ShiftedUnequalOpaqueCallBoundaryRawFileOffset == 0x012C9137
            && CapacityFinalCalleeShiftedIndexedLifecycleProjection
                .IndexedFrameGateEntryRawFileOffset == 0x012C913D
            && CapacityFinalCalleeShiftedIndexedLifecycleProjection
                .IndexedUnequalDestinationRawFileOffset == 0x012C9151
            && CapacityFinalCalleeShiftedIndexedLifecycleProjection
                .IndexedEqualDestinationRawFileOffset == 0x012C9160);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan missingUpstream =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: null,
                shiftedFrame4: 0x20,
                shiftedEcx: 0x100,
                indexedFrame4: 0x10,
                indexedFrame10: 0x80));
        check("Missing upstream EBP blocks both gates and marks every later capture unreached",
            missingUpstream.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingUpstream.ShiftedFrameGatePlan.Outcome
                == CapacityFinalCalleeAlternateShiftedFrameGateOutcome.MissingEntryEBP
            && !missingUpstream.ShiftedFrameGatePlan.FrameMinus4ReadReached
            && missingUpstream.ShiftedFrameGatePlan.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstream.ShiftedFrameGatePlan.UnreachedEntryECXCaptureProvided
            && missingUpstream.IndexedFrameGatePlan is null
            && !missingUpstream.IndexedFrameGateReached
            && missingUpstream.RetainedUpstreamEBP is null
            && missingUpstream.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && missingUpstream.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && missingUpstream.LateFreshIndexedFrameMinus4CaptureProvided
            && missingUpstream.LateFreshIndexedFrameMinus10CaptureProvided
            && !missingUpstream.ExactCleanExitAdmitted
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C912D
            && missingUpstream.OrderedIndexedWrites.IsEmpty
            && !missingUpstream.OpaqueCallReached);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan missingUpstreamFrame =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x100,
                shiftedFrame4: null,
                shiftedEcx: 0x100,
                indexedFrame4: 0x10,
                indexedFrame10: 0x80));
        check("Missing shifted EBP-4 cannot be bypassed and downstream captures remain late",
            missingUpstreamFrame.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingUpstreamFrame.ShiftedFrameGatePlan is
            {
                Outcome: CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .MissingFreshFrameMinus4,
                FrameMinus4ReadReached: true,
                FrameMinus4Address: 0xFC,
                FrameMinus4Captured: false,
                ShiftReached: false,
                UnreachedEntryECXCaptureProvided: true,
                LateEntryECXCaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C912D
            }
            && missingUpstreamFrame.IndexedFrameGatePlan is null
            && !missingUpstreamFrame.IndexedFrameGateReached
            && missingUpstreamFrame.RetainedUpstreamEBP is null
            && missingUpstreamFrame.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && missingUpstreamFrame.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && missingUpstreamFrame.LateFreshIndexedFrameMinus4CaptureProvided
            && missingUpstreamFrame.LateFreshIndexedFrameMinus10CaptureProvided
            && !missingUpstreamFrame.ExactCleanExitAdmitted
            && missingUpstreamFrame.StopAtRawFileOffsetExclusive == 0x012C912D);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan missingUpstreamEcx =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x100,
                shiftedFrame4: 0x20000000,
                shiftedEcx: null,
                indexedFrame4: 0x10,
                indexedFrame10: 0x80));
        check("Missing shifted ECX stops at CMP and rejects later indexed captures",
            missingUpstreamEcx.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.ShiftedFrameGateBlocked
            && missingUpstreamEcx.ShiftedFrameGatePlan is
            {
                FrameMinus4Address: 0xFC,
                FrameMinus4: 0x20000000,
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
                JzReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9133
            }
            && missingUpstreamEcx.IndexedFrameGatePlan is null
            && missingUpstreamEcx.RetainedUpstreamEBP is null
            && missingUpstreamEcx.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && missingUpstreamEcx.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && missingUpstreamEcx.LateFreshIndexedFrameMinus4CaptureProvided
            && missingUpstreamEcx.LateFreshIndexedFrameMinus10CaptureProvided
            && !missingUpstreamEcx.ExactCleanExitAdmitted
            && missingUpstreamEcx.StopAtRawFileOffsetExclusive == 0x012C9133);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan clean9137 =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x100,
                shiftedFrame4: 1,
                shiftedEcx: 0,
                indexedFrame4: null,
                indexedFrame10: null));
        check("A clean shifted inequality is a bounded 9137 exit with no child gate",
            clean9137.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.ShiftedUnequalTo9137
            && clean9137.ShiftedFrameGatePlan is
            {
                JzReached: true,
                JzTaken: false,
                JzSelectedDestinationRawFileOffset: 0x012C9137,
                Outcome: CapacityFinalCalleeAlternateShiftedFrameGateOutcome
                    .UnequalToOpaqueCallBoundary,
                ExactCleanExitAdmitted: true,
                OpaqueCallReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9137
            }
            && clean9137.IndexedFrameGatePlan is null
            && !clean9137.IndexedFrameGateReached
            && clean9137.RetainedUpstreamEBP is null
            && !clean9137.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && !clean9137.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && clean9137.ExactCleanExitAdmitted
            && clean9137.StopAtRawFileOffsetExclusive == 0x012C9137
            && clean9137.OrderedIndexedWrites.IsEmpty
            && !clean9137.OpaqueCallReached);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan contaminated9137 =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x100,
                shiftedFrame4: 1,
                shiftedEcx: 0,
                indexedFrame4: 0x10,
                indexedFrame10: 0x80));
        check("Downstream captures contaminate 9137 and are rejected before the indexed gate",
            contaminated9137.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome
                    .ShiftedUnequalTo9137Contaminated
            && contaminated9137.ShiftedFrameGatePlan.JzTaken == false
            && contaminated9137.IndexedFrameGatePlan is null
            && !contaminated9137.IndexedFrameGateReached
            && contaminated9137.RetainedUpstreamEBP is null
            && contaminated9137.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && contaminated9137.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && contaminated9137.LateFreshIndexedFrameMinus4CaptureProvided
            && contaminated9137.LateFreshIndexedFrameMinus10CaptureProvided
            && !contaminated9137.ExactCleanExitAdmitted
            && contaminated9137.StopAtRawFileOffsetExclusive == 0x012C9137
            && contaminated9137.OrderedIndexedWrites.IsEmpty
            && !contaminated9137.OpaqueCallReached);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan missingIndexedFrame4 =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x204,
                shiftedFrame4: 0xE0000001,
                shiftedEcx: 8,
                indexedFrame4: null,
                indexedFrame10: 0x80));
        check("Selected 913D retains EBP for the indexed read and missing indexed EBP-4 blocks EBP-10",
            missingIndexedFrame4.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedFrameGateBlocked
            && missingIndexedFrame4.ShiftedFrameGatePlan.JzSelectedDestinationRawFileOffset
                == 0x012C913D
            && missingIndexedFrame4.IndexedFrameGatePlan is
            {
                Outcome: CapacityFinalCalleeAlternateIndexedFrameGateOutcome
                    .MissingFreshFrameMinus4,
                EntryEBPCaptured: true,
                FrameMinus4ReadReached: true,
                FrameMinus4Address: 0x200,
                FrameMinus4Captured: false,
                FrameMinus10ReadReached: false,
                IndexedAddressCalculationReached: false,
                StackStoreReached: false,
                CompareReached: false,
                UnreachedFreshFrameMinus10CaptureProvided: true,
                LateFreshFrameMinus10CaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C913D
            }
            && missingIndexedFrame4.IndexedFrameGateReached
            && missingIndexedFrame4.RetainedUpstreamEBP == 0x204
            && !missingIndexedFrame4.UnreachedFreshIndexedFrameMinus4CaptureProvided
            && missingIndexedFrame4.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && missingIndexedFrame4.LateFreshIndexedFrameMinus10CaptureProvided
            && !missingIndexedFrame4.ExactCleanExitAdmitted
            && missingIndexedFrame4.StopAtRawFileOffsetExclusive == 0x012C913D
            && missingIndexedFrame4.OrderedIndexedWrites.IsEmpty);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan missingIndexedFrame10 =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x20,
                shiftedFrame4: 0xE0000001,
                shiftedEcx: 8,
                indexedFrame4: 0xE0000001,
                indexedFrame10: null));
        check("Missing indexed EBP-10 reaches only that independent load after selected 913D",
            missingIndexedFrame10.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedFrameGateBlocked
            && missingIndexedFrame10.IndexedFrameGatePlan is
            {
                Outcome: CapacityFinalCalleeAlternateIndexedFrameGateOutcome
                    .MissingFreshFrameMinus10,
                FrameMinus4Address: 0x1C,
                FrameMinus4Captured: true,
                FrameMinus4: 0xE0000001,
                FrameMinus10ReadReached: true,
                FrameMinus10Address: 0x10,
                FrameMinus10Captured: false,
                IndexedAddressCalculationReached: false,
                StackStoreReached: false,
                CompareReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9140
            }
            && missingIndexedFrame10.IndexedFrameGateReached
            && missingIndexedFrame10.RetainedUpstreamEBP == 0x20
            && !missingIndexedFrame10.UnreachedFreshIndexedFrameMinus10CaptureProvided
            && !missingIndexedFrame10.LateFreshIndexedFrameMinus10CaptureProvided
            && !missingIndexedFrame10.ExactCleanExitAdmitted
            && missingIndexedFrame10.StopAtRawFileOffsetExclusive == 0x012C9140
            && missingIndexedFrame10.OrderedIndexedWrites.IsEmpty);

        uint wrappedIndexedAddress =
            CapacityFinalCalleeAlternateIndexedFrameGateProjection.CalculateIndexedAddress(
                0xFFFFFFFF);
        CapacityFinalCalleeShiftedIndexedLifecyclePlan cleanIndexedEqual =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 4,
                shiftedFrame4: 0xE0000001,
                shiftedEcx: 8,
                indexedFrame4: 0xFFFFFFFF,
                indexedFrame10: wrappedIndexedAddress));
        check("Clean 913D and indexed equality preserve fresh reloads, wraparound, flags, and ordered store",
            cleanIndexedEqual.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedEqualTo9160
            && cleanIndexedEqual.ShiftedFrameGatePlan is
            {
                FrameMinus4Address: 0,
                FrameMinus4: 0xE0000001,
                ShiftResult: 8,
                JzTaken: true,
                JzSelectedDestinationRawFileOffset: 0x012C913D
            }
            && cleanIndexedEqual.IndexedFrameGatePlan is
            {
                EntryEBPCaptured: true,
                FrameMinus4Address: 0,
                FrameMinus4: 0xFFFFFFFF,
                FrameMinus10Address: 0xFFFFFFF4,
                FrameMinus10: 0x01EDADE8,
                IndexedAddressCalculationReached: true,
                IndexedAddressInput: 0xFFFFFFFF,
                IndexedAddress: 0x01EDADE8,
                StackStoreReached: true,
                StackStoreAddress: 0xFFFFFFF8,
                StackStoreValue: 0x01EDADE8,
                CompareLeft: 0x01EDADE8,
                CompareRight: 0x01EDADE8,
                CompareResult: 0,
                CompareFlags: {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                JeTaken: true,
                JeSelectedDestinationRawFileOffset: 0x012C9160,
                StopAtRawFileOffsetExclusive: 0x012C9160
            }
            && wrappedIndexedAddress == 0x01EDADE8
            && cleanIndexedEqual.IndexedFrameGatePlan!.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                    0x012C914A, 0xFFFFFFF8, 0x01EDADE8)
            })
            && cleanIndexedEqual.OrderedIndexedWrites.Length == 1
            && cleanIndexedEqual.IndexedFrameGatePlan!.CompareReached
            && cleanIndexedEqual.IndexedFrameGateReached
            && cleanIndexedEqual.RetainedUpstreamEBP == 4
            && cleanIndexedEqual.ExactCleanExitAdmitted
            && cleanIndexedEqual.StopAtRawFileOffsetExclusive == 0x012C9160
            && !cleanIndexedEqual.OpaqueCallReached);

        CapacityFinalCalleeShiftedIndexedLifecyclePlan cleanIndexedUnequal =
            CapacityFinalCalleeShiftedIndexedLifecycleProjection.Project(Capture(
                entryEbp: 0x200,
                shiftedFrame4: 0,
                shiftedEcx: 0,
                indexedFrame4: 0,
                indexedFrame10: 0));
        check("Clean indexed inequality preserves CMP flags and the pre-branch ordered store at 9151",
            cleanIndexedUnequal.Outcome
                == CapacityFinalCalleeShiftedIndexedLifecycleOutcome.IndexedUnequalTo9151
            && cleanIndexedUnequal.ShiftedFrameGatePlan.JzSelectedDestinationRawFileOffset
                == 0x012C913D
            && cleanIndexedUnequal.IndexedFrameGatePlan is
            {
                IndexedAddress: 0x01EDADF0,
                StackStoreAddress: 0x1F4,
                StackStoreValue: 0x01EDADF0,
                CompareLeft: 0,
                CompareRight: 0x01EDADF0,
                CompareResult: 0xFE125210,
                CompareFlags: {
                    Carry: true,
                    Parity: false,
                    Auxiliary: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                JeTaken: false,
                JeSelectedDestinationRawFileOffset: 0x012C9151,
                StopAtRawFileOffsetExclusive: 0x012C9151
            }
            && cleanIndexedUnequal.OrderedIndexedWrites.SequenceEqual(new[]
            {
                new CapacityFinalCalleeAlternateIndexedFrameGateWrite(
                    0x012C914A, 0x1F4, 0x01EDADF0)
            })
            && cleanIndexedUnequal.ExactCleanExitAdmitted
            && cleanIndexedUnequal.StopAtRawFileOffsetExclusive == 0x012C9151
            && !cleanIndexedUnequal.OpaqueCallReached);
    }

    private static CapacityFinalCalleeShiftedIndexedLifecycleCapture Capture(
        uint? entryEbp,
        uint? shiftedFrame4,
        uint? shiftedEcx,
        uint? indexedFrame4,
        uint? indexedFrame10) =>
        new(
            new CapacityFinalCalleeAlternateShiftedFrameGateCapture(
                entryEbp,
                shiftedFrame4,
                shiftedEcx),
            indexedFrame4,
            indexedFrame10);
}
