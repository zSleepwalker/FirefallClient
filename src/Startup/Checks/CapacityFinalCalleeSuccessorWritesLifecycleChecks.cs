using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSuccessorWritesLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The gate handoff and successor window are adjacent and bounded",
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.EntryRawFileOffset
                == 0x012C917C
            && CapacityFinalCalleeSuccessorWritesLifecycleProjection
                .SuccessorWritesEntryRawFileOffset == 0x012C9190
            && CapacityFinalCalleeSuccessorWritesLifecycleProjection
                .OpaqueCallBoundaryRawFileOffset == 0x012C918A
            && CapacityFinalCalleeSuccessorWritesLifecycleProjection
                .SuccessorWritesTargetRawFileOffset == 0x012C92BD);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan frameEqual =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 0x55,
                    entryEax: 0x55,
                    entryEcx: 8));
        check("Clean frame equality admits caller ECX and carries unchanged EAX into both stores",
            frameEqual.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome.CompletedAt92BD
            && frameEqual.Admitted
            && frameEqual.Gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .EqualFrameValueToEntryEAX
            && frameEqual.Gate.FrameJzReached
            && frameEqual.Gate.FrameJzTaken == true
            && frameEqual.Gate.FrameJzSelectedDestinationRawFileOffset == 0x012C9190
            && frameEqual.Gate.FrameCompareLeft == 0x55
            && frameEqual.Gate.FrameCompareRight == 0x55
            && frameEqual.Gate.FrameCompareResult == 0
            && frameEqual.Gate.FrameCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && frameEqual.Gate.ExactCleanExitAdmitted
            && frameEqual.Gate.ExactCleanBypassAdmitted
            && !frameEqual.Gate.RegisterCompareReached
            && !frameEqual.Gate.FreshFieldReadReached
            && NoUnreachedOrUnusedGateCaptures(frameEqual.Gate)
            && frameEqual.Gate.StopAtRawFileOffsetExclusive == 0x012C9190
            && frameEqual.SuccessorWritesReached
            && frameEqual.ExactCleanHandoffAdmitted
            && frameEqual.DownstreamEntryEAXDerived
            && frameEqual.DownstreamEntryEAX == 0x55
            && frameEqual.SuccessorWrites is
            {
                Capture.EntryECX: 8,
                Capture.EntryEAX: 0x55,
                FirstStoreAddress: 0x14,
                FirstStoreValue: 0x55,
                SecondStoreAddress: 0x5D,
                SecondStoreValue: 8,
                JumpReached: true,
                FlagsPreserved: true,
                ExactCleanExitAdmitted: true,
                JumpTargetRawFileOffset: 0x012C92BD,
                StopAtRawFileOffsetExclusive: 0x012C92BD
            }
            && frameEqual.SuccessorWrites!.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x14u, 0x55u),
                    (0x012C9193u, 0x5Du, 8u)
                })
            && frameEqual.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan fieldEqual =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 9,
                    entryEax: 8,
                    entryEbx: 7,
                    freshField: 0xAABBCCDD,
                    entryEdx: 0xAABBCCDD,
                    entryEcx: 0xFFFFFFF8));
        check("Clean fresh-field equality at 9190 independently admits the child",
            fieldEqual.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome.CompletedAt92BD
            && fieldEqual.Gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .EqualFreshFieldToEntryEDX
            && fieldEqual.Gate.FrameJzReached
            && fieldEqual.Gate.FrameJzTaken == false
            && fieldEqual.Gate.FrameJzSelectedDestinationRawFileOffset == 0x012C9181
            && fieldEqual.Gate.JbReached
            && fieldEqual.Gate.JbTaken == false
            && fieldEqual.Gate.JbSelectedDestinationRawFileOffset == 0x012C9185
            && fieldEqual.Gate.FieldJzReached
            && fieldEqual.Gate.FieldJzTaken == true
            && fieldEqual.Gate.FieldJzSelectedDestinationRawFileOffset == 0x012C9190
            && fieldEqual.Gate.FieldCompareLeft == 0xAABBCCDD
            && fieldEqual.Gate.FieldCompareRight == 0xAABBCCDD
            && fieldEqual.Gate.FieldCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && fieldEqual.Gate.ExactCleanExitAdmitted
            && fieldEqual.Gate.ExactCleanBypassAdmitted
            && NoUnreachedOrUnusedGateCaptures(fieldEqual.Gate)
            && fieldEqual.Gate.StopAtRawFileOffsetExclusive == 0x012C9190
            && fieldEqual.SuccessorWritesReached
            && fieldEqual.ExactCleanHandoffAdmitted
            && fieldEqual.DownstreamEntryEAX == 8
            && fieldEqual.SuccessorWrites is
            {
                FirstStoreAddress: 0x00000004,
                FirstStoreValue: 8,
                SecondStoreAddress: 0x10,
                SecondStoreValue: 0xFFFFFFF8,
                JumpReached: true,
                FlagsPreserved: true,
                ExactCleanExitAdmitted: true,
                JumpTargetRawFileOffset: 0x012C92BD,
                StopAtRawFileOffsetExclusive: 0x012C92BD
            }
            && fieldEqual.SuccessorWrites!.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x04, 0x10 })
            && fieldEqual.SuccessorWrites.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 8, 0xFFFFFFF8 })
            && fieldEqual.SuccessorWrites.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9190, 0x012C9193 })
            && fieldEqual.Admitted);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan aliasedStores =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 0,
                    entryEax: 0x104,
                    entryEbx: 0,
                    freshField: 9,
                    entryEdx: 9,
                    entryEcx: 0x100));
        check("The composed lifecycle retains both ordered writes when addresses alias",
            aliasedStores.Admitted
            && aliasedStores.SuccessorWrites is
            {
                FirstStoreAddress: 0x10C,
                FirstStoreValue: 0x104,
                SecondStoreAddress: 0x10C,
                SecondStoreValue: 0x100,
                JumpReached: true,
                FlagsPreserved: true
            }
            && aliasedStores.SuccessorWrites!.Writes.Length == 2
            && aliasedStores.SuccessorWrites.Writes[0]
                == new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    0x012C9190, 0x10C, 0x104)
            && aliasedStores.SuccessorWrites.Writes[1]
                == new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    0x012C9193, 0x10C, 0x100)
            && aliasedStores.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan childMissingECX =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 0x55,
                    entryEax: 0x55,
                    entryEcx: null));
        check("A missing child ECX stops exactly at the clean 9190 handoff",
            childMissingECX.SuccessorWritesReached
            && childMissingECX.ExactCleanHandoffAdmitted
            && childMissingECX.DownstreamEntryEAX == 0x55
            && childMissingECX.SuccessorWrites is
            {
                Outcome: CapacityFinalCalleeAlternateSuccessorWritesOutcome
                    .MissingEntryECX,
                EntryECXCaptured: false,
                EntryEAXCaptured: true,
                FirstStoreReached: false,
                SecondStoreReached: false,
                JumpReached: false,
                Writes.IsEmpty: true,
                StopAtRawFileOffsetExclusive: 0x012C9190
            }
            && childMissingECX.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                    .SuccessorWritesIncomplete
            && !childMissingECX.Admitted
            && childMissingECX.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan frameEqualWithCallerECX =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0,
                    frame: 4,
                    entryEax: 4,
                    entryEcx: 8));
        check("Caller ECX is consumed by successor stores after clean frame equality",
            frameEqualWithCallerECX.Gate.FrameJzTaken == true
            && frameEqualWithCallerECX.Gate.ExactCleanExitAdmitted
            && frameEqualWithCallerECX.Gate.StopAtRawFileOffsetExclusive == 0x012C9190
            && frameEqualWithCallerECX.SuccessorWritesReached
            && frameEqualWithCallerECX.ExactCleanHandoffAdmitted
            && frameEqualWithCallerECX.SuccessorWrites is
            {
                Capture.EntryECX: 8,
                Capture.EntryEAX: 4,
                FirstStoreAddress: 0x14,
                FirstStoreValue: 4,
                SecondStoreAddress: 0x0C,
                SecondStoreValue: 8,
                JumpReached: true,
                JumpTargetRawFileOffset: 0x012C92BD,
                FlagsPreserved: true,
                StopAtRawFileOffsetExclusive: 0x012C92BD
            }
            && !frameEqualWithCallerECX.UnreachedDownstreamEntryECXCaptureProvided
            && frameEqualWithCallerECX.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome.CompletedAt92BD
            && frameEqualWithCallerECX.Admitted
            && frameEqualWithCallerECX.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan frameEqualWithLateGateCaptures =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0,
                    frame: 4,
                    entryEax: 4,
                    entryEbx: 5,
                    freshField: 6,
                    entryEdx: 7,
                    entryEcx: null));
        check("Gate-side later captures also contaminate frame equality before the child",
            !frameEqualWithLateGateCaptures.Gate.ExactCleanExitAdmitted
            && frameEqualWithLateGateCaptures.Gate.UnusedEntryEBXCaptureProvided
            && frameEqualWithLateGateCaptures.Gate.UnusedFreshFieldCaptureProvided
            && frameEqualWithLateGateCaptures.Gate.UnusedEntryEDXCaptureProvided
            && !frameEqualWithLateGateCaptures.SuccessorWritesReached
            && frameEqualWithLateGateCaptures.SuccessorWrites is null
            && !frameEqualWithLateGateCaptures
                .UnreachedDownstreamEntryECXCaptureProvided
            && frameEqualWithLateGateCaptures.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                    .CleanGateRejected
            && frameEqualWithLateGateCaptures.StopAtRawFileOffsetExclusive
                == 0x012C9190);

        CapacityFinalCalleeSuccessorWritesLifecyclePlan below =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0,
                    frame: 1,
                    entryEax: 0,
                    entryEbx: 1,
                    entryEcx: null));
        CapacityFinalCalleeSuccessorWritesLifecyclePlan fieldUnequal =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 9,
                    entryEax: 8,
                    entryEbx: 7,
                    freshField: 2,
                    entryEdx: 3,
                    entryEcx: null));
        check("Both clean opaque 918A exits remain bounded without entering the child",
            below.Gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .BelowToOpaqueCallBoundary
            && below.Gate.JbTaken == true
            && below.Gate.JbSelectedDestinationRawFileOffset == 0x012C918A
            && below.Gate.ExactCleanExitAdmitted
            && fieldUnequal.Gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .UnequalFreshFieldToOpaqueCallBoundary
            && fieldUnequal.Gate.FieldJzTaken == false
            && fieldUnequal.Gate.FieldJzSelectedDestinationRawFileOffset
                == 0x012C918A
            && fieldUnequal.Gate.ExactCleanExitAdmitted
            && new[] { below, fieldUnequal }.All(plan =>
                plan.Gate.StopAtRawFileOffsetExclusive == 0x012C918A
                && !plan.Gate.OpaqueCallReached
                && !plan.SuccessorWritesReached
                && plan.SuccessorWrites is null
                && !plan.UnreachedDownstreamEntryECXCaptureProvided
                && plan.Outcome
                    == CapacityFinalCalleeSuccessorWritesLifecycleOutcome.OpaqueExitAt918A
                && plan.Admitted
                && plan.StopAtRawFileOffsetExclusive == 0x012C918A));

        CapacityFinalCalleeSuccessorWritesLifecyclePlan belowWithLaterECX =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0,
                    frame: 1,
                    entryEax: 0,
                    entryEbx: 1,
                    entryEcx: 8));
        CapacityFinalCalleeSuccessorWritesLifecyclePlan fieldUnequalWithLaterECX =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0x100,
                    frame: 9,
                    entryEax: 8,
                    entryEbx: 7,
                    freshField: 2,
                    entryEdx: 3,
                    entryEcx: 8));
        check("Both clean opaque exits reject later ECX before any child instruction",
            new[] { belowWithLaterECX, fieldUnequalWithLaterECX }.All(plan =>
                plan.Gate.ExactCleanExitAdmitted
                && plan.Gate.StopAtRawFileOffsetExclusive == 0x012C918A
                && !plan.Gate.OpaqueCallReached
                && !plan.SuccessorWritesReached
                && plan.SuccessorWrites is null
                && plan.UnreachedDownstreamEntryECXCaptureProvided
                && plan.Outcome
                    == CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .OpaqueExitAt918AWithDownstreamECXCapture
                && !plan.Admitted
                && plan.StopAtRawFileOffsetExclusive == 0x012C918A));

        CapacityFinalCalleeSuccessorWritesLifecyclePlan belowWithUnreachedGateCaptures =
            CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                Capture(
                    entryEbp: 0,
                    frame: 1,
                    entryEax: 0,
                    entryEbx: 1,
                    freshField: 2,
                    entryEdx: 3,
                    entryEcx: null));
        check("Unreached EAX+8 and EDX captures make an opaque below gate inadmissible",
            belowWithUnreachedGateCaptures.Gate.Outcome
                == CapacityFinalCalleeAlternateSuccessorGateOutcome
                    .BelowToOpaqueCallBoundary
            && !belowWithUnreachedGateCaptures.Gate.ExactCleanExitAdmitted
            && belowWithUnreachedGateCaptures.Gate.UnusedFreshFieldCaptureProvided
            && belowWithUnreachedGateCaptures.Gate.UnusedEntryEDXCaptureProvided
            && !belowWithUnreachedGateCaptures
                .UnreachedDownstreamEntryECXCaptureProvided
            && !belowWithUnreachedGateCaptures.SuccessorWritesReached
            && belowWithUnreachedGateCaptures.SuccessorWrites is null
            && belowWithUnreachedGateCaptures.Outcome
                == CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                    .OpaqueExitAt918AWithUnreachedGateCaptures
            && !belowWithUnreachedGateCaptures.Admitted
            && belowWithUnreachedGateCaptures.StopAtRawFileOffsetExclusive
                == 0x012C918A);

        CapacityFinalCalleeAlternateSuccessorGateCapture[] missingUpstreamCaptures =
        {
            new(null, 0, 0, 0, 0, 0),
            new(0, null, 0, 0, 0, 0),
            new(0, 0, null, 0, 0, 0),
            new(0, 1, 0, null, 0, 0),
            new(0, 1, 0, 0, null, 0),
            new(0, 1, 0, 0, 0, null)
        };
        bool upstreamEventsPreserved = missingUpstreamCaptures.All(gateCapture =>
        {
            CapacityFinalCalleeSuccessorWritesLifecyclePlan plan =
                CapacityFinalCalleeSuccessorWritesLifecycleProjection.Project(
                    new(gateCapture, DownstreamEntryECX: 0x20));
            CapacityFinalCalleeAlternateSuccessorGatePlan directGate =
                CapacityFinalCalleeAlternateSuccessorGateProjection.Project(gateCapture);
            return plan.Gate.Equals(directGate)
                && plan.Gate.Capture == gateCapture
                && !plan.SuccessorWritesReached
                && plan.SuccessorWrites is null
                && plan.UnreachedDownstreamEntryECXCaptureProvided
                && plan.Outcome
                    == CapacityFinalCalleeSuccessorWritesLifecycleOutcome
                        .GateIncompleteWithDownstreamECXCapture
                && plan.StopAtRawFileOffsetExclusive
                    == directGate.StopAtRawFileOffsetExclusive;
        });
        check("Every missing upstream capture preserves its gate events and blocks the child",
            upstreamEventsPreserved);
    }

    private static CapacityFinalCalleeSuccessorWritesLifecycleCapture Capture(
        uint? entryEbp,
        uint? frame,
        uint? entryEax,
        uint? entryEbx = null,
        uint? freshField = null,
        uint? entryEdx = null,
        uint? entryEcx = 0x200) =>
        new(
            new(
                EntryEBP: entryEbp,
                FreshFrameMinusC: frame,
                EntryEAX: entryEax,
                EntryEBX: entryEbx,
                FreshFieldAtEaxPlus8: freshField,
                EntryEDX: entryEdx),
            entryEcx);

    private static bool NoUnreachedOrUnusedGateCaptures(
        CapacityFinalCalleeAlternateSuccessorGatePlan gate) =>
        !gate.UnreachedFreshFrameMinusCCaptureProvided
        && !gate.UnreachedEntryEAXCaptureProvided
        && !gate.LateEntryEAXCaptureProvided
        && !gate.UnreachedEntryEBXCaptureProvided
        && !gate.LateEntryEBXCaptureProvided
        && !gate.UnreachedFreshFieldCaptureProvided
        && !gate.LateFreshFieldCaptureProvided
        && !gate.UnreachedEntryEDXCaptureProvided
        && !gate.LateEntryEDXCaptureProvided
        && !gate.UnusedEntryEBXCaptureProvided
        && !gate.UnusedFreshFieldCaptureProvided
        && !gate.UnusedEntryEDXCaptureProvided;
}
