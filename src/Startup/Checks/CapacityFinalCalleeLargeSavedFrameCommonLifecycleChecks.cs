using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedFrameCommonLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The large-frame gate and common-tail boundaries agree",
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection
                .CommonTailEntryRawFileOffset
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .ZeroDestinationRawFileOffset
            && CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection
                .NonzeroDestinationRawFileOffset
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .NonzeroDestinationRawFileOffset
            && CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection
                .CompletedBoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .BoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan allMissing =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(null, null),
                null,
                null));
        check("All missing captures stop at the parent's first read",
            allMissing.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome
                    .LargeSavedFrameGateIncomplete
            && allMissing.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingEntryEBP
            && allMissing.LargeSavedFrameGate.Reads.Length == 1
            && !allMissing.CommonTailReached
            && allMissing.CommonTail is null
            && allMissing.OrderedWrites.IsEmpty
            && !allMissing.UnreachedCallerEntryEDICaptureProvided
            && !allMissing.UnreachedCallerEntryESICaptureProvided
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .FreshReadRawFileOffset);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan missingParentContaminated =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(null, 0),
                0x10,
                0x20));
        check("Caller captures cannot admit a common tail when the parent EBP is missing",
            missingParentContaminated.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingEntryEBP
            && missingParentContaminated.CommonTail is null
            && missingParentContaminated.UnreachedCallerEntryEDICaptureProvided
            && missingParentContaminated.UnreachedCallerEntryESICaptureProvided
            && !missingParentContaminated.ExactCleanExitAdmitted
            && missingParentContaminated.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .FreshReadRawFileOffset);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan missingFreshContaminated =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x20, null),
                0x10,
                0x20));
        check("Caller captures cannot admit a common tail when fresh EBP-10 is missing",
            missingFreshContaminated.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingFreshFrameMinus10
            && missingFreshContaminated.LargeSavedFrameGate.FreshFrameMinus10Address
                == 0x10
            && missingFreshContaminated.LargeSavedFrameGate.UnreachedFreshFrameMinus10CaptureProvided
                == false
            && missingFreshContaminated.CommonTail is null
            && missingFreshContaminated.UnreachedCallerEntryEDICaptureProvided
            && missingFreshContaminated.UnreachedCallerEntryESICaptureProvided
            && !missingFreshContaminated.ExactCleanExitAdmitted
            && missingFreshContaminated.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .FreshReadRawFileOffset);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan cleanNonzero =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x20, 1),
                null,
                null));
        check("A clean nonzero TEST retains all six flags and stops at 9231",
            cleanNonzero.Outcome
                == CapacityFinalCalleeLargeSavedFrameCommonLifecycleOutcome.NonzeroTo9231
            && cleanNonzero.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough
            && cleanNonzero.LargeSavedFrameGate.JzReached
            && cleanNonzero.LargeSavedFrameGate.JzTaken == false
            && cleanNonzero.LargeSavedFrameGate.JzSelectedDestinationRawFileOffset
                == 0x012C9231
            && cleanNonzero.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && cleanNonzero.CommonTail is null
            && cleanNonzero.ExactCleanExitAdmitted
            && cleanNonzero.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan nonzeroContaminated =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x20, 0x80000001),
                4,
                8));
        check("Unused child captures contaminate the bounded nonzero admission",
            nonzeroContaminated.LargeSavedFrameGate.JzTaken == false
            && nonzeroContaminated.CommonTail is null
            && nonzeroContaminated.UnreachedCallerEntryEDICaptureProvided
            && nonzeroContaminated.UnreachedCallerEntryESICaptureProvided
            && !nonzeroContaminated.ExactCleanExitAdmitted
            && nonzeroContaminated.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan zeroMissingEDI =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x10, 0),
                null,
                0x20));
        check("The clean zero handoff retains TEST flags and flags unused ESI after missing EDI",
            zeroMissingEDI.LargeSavedFrameGate.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.ZeroToTarget
            && zeroMissingEDI.LargeSavedFrameGate.JzTaken == true
            && zeroMissingEDI.LargeSavedFrameGate.JzSelectedDestinationRawFileOffset
                == 0x012C92BD
            && zeroMissingEDI.ExactCleanHandoffToCommonTail
            && zeroMissingEDI.CommonTailReached
            && zeroMissingEDI.CommonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI,
                Capture: { EntryESI: null },
                OrInstructionReached: false,
                FirstStoreReached: false,
                SecondStoreReached: false
            }
            && zeroMissingEDI.UnreachedCallerEntryESICaptureProvided
            && !zeroMissingEDI.CallerEntryEDICaptured
            && !zeroMissingEDI.CallerEntryESICaptured
            && zeroMissingEDI.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroMissingEDI.OrderedWrites.IsEmpty
            && !zeroMissingEDI.ExactCleanExitAdmitted
            && zeroMissingEDI.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan zeroMissingESI =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x40, 0),
                0,
                null));
        check("Missing ESI retains the child's OR result and all six OR flags",
            zeroMissingESI.CommonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI,
                EntryEDICaptured: true,
                EAXRetained: 0,
                EAXAfterOr: 1,
                OrInstructionReached: true,
                OrFlags: {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                },
                EntryESICaptured: false,
                FirstStoreReached: false,
                SecondStoreReached: false
            }
            && zeroMissingESI.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && zeroMissingESI.StopAtRawFileOffsetExclusive == 0x012C92C2
            && !zeroMissingESI.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedFrameCommonLifecyclePlan completed =
            CapacityFinalCalleeLargeSavedFrameCommonLifecycleProjection.Project(new(
                new(0x0F, 0),
                4,
                0xFFFFFFFC));
        check("The fresh zero value and two ordered DWORD-wrapped aliasing writes complete cleanly",
            completed.LargeSavedFrameGate.FreshFrameMinus10Address == 0xFFFFFFFF
            && completed.LargeSavedFrameGate.FreshFrameMinus10 == 0
            && completed.LargeSavedFrameGate.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && completed.CommonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
                EntryEDICaptured: true,
                EntryESICaptured: true,
                EAXAfterOr: 5,
                OrFlags: {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            }
            && completed.OrderedWrites.Select(write =>
                (write.Kind, write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (
                        CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind
                            .CommonTailFirstStore,
                        0x012C92C2u,
                        0u,
                        5u),
                    (
                        CapacityFinalCalleeLargeSavedFrameCommonLifecycleWriteKind
                            .CommonTailSecondStore,
                        0x012C92C5u,
                        0u,
                        4u)
                })
            && completed.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && completed.ExactCleanHandoffToCommonTail
            && completed.ExactCleanExitAdmitted
            && completed.StopAtRawFileOffsetExclusive == 0x012C92C8);
    }
}
