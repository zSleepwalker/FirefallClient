using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeMaskCommonLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeMaskCommonLifecyclePlan allMissing =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                new(
                    EntryEBP: null,
                    FreshFrameMinus8: null,
                    FreshFrameMinus4: null,
                    FreshGlobalValue: null),
                CallerEntryEDI: 0x10,
                CallerEntryESI: 0x20));
        check("All missing mask inputs keep both caller registers unreached",
            allMissing.Outcome
                == CapacityFinalCalleeMaskCommonLifecycleOutcome.MaskTailIncomplete
            && allMissing.MaskTail.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingEntryEBP
            && !allMissing.ExactCleanHandoffToCommonTail
            && !allMissing.CommonTailReached
            && allMissing.CommonTail is null
            && allMissing.UnreachedCallerEntryEDICaptureProvided
            && allMissing.UnreachedCallerEntryESICaptureProvided
            && allMissing.OrderedWrites.IsEmpty
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlternateMaskTailProjection.EntryRawFileOffset);

        CapacityFinalCalleeMaskCommonLifecyclePlan contaminatedFlags =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(
                    frameMinus8: 0x40000000,
                    frameMinus4: 1,
                    global: 0xFFFFFFFF,
                    entryFlags: KnownFlags),
                CallerEntryEDI: 4,
                CallerEntryESI: 0x01EDADC4));
        check("Unused nonzero-count mask flags contaminate the successor and block the child",
            contaminatedFlags.MaskTail.UnusedEntryFlagsCaptureProvided
            && !contaminatedFlags.MaskTail.ExactCleanExitAdmitted
            && contaminatedFlags.Outcome
                == CapacityFinalCalleeMaskCommonLifecycleOutcome
                    .MaskTailContaminatedAtSuccessor
            && contaminatedFlags.MaskTail.GlobalWriteReached
            && contaminatedFlags.OrderedWrites.Length == 1
            && contaminatedFlags.OrderedWrites[0].Kind
                == CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal
            && !contaminatedFlags.CommonTailReached
            && contaminatedFlags.CommonTail is null
            && contaminatedFlags.UnreachedCallerEntryEDICaptureProvided
            && contaminatedFlags.UnreachedCallerEntryESICaptureProvided
            && !contaminatedFlags.ExactCleanExitAdmitted
            && contaminatedFlags.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeMaskCommonLifecycleProjection.CommonTailEntryRawFileOffset);

        CapacityFinalCalleeMaskCommonLifecyclePlan missingGlobal =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                new(0x100, 0x40000000, 1, null), 4, 8));
        check("Missing global evidence preserves shift flags without entering the common tail",
            missingGlobal.MaskTail.Outcome
                == CapacityFinalCalleeAlternateMaskTailOutcome.MissingFreshGlobalValue
            && missingGlobal.FlagsAtStop == missingGlobal.MaskTail.FlagsAfterNot
            && missingGlobal.FlagsAtStop is { Carry: false, Sign: true, Overflow: true }
            && missingGlobal.CommonTail is null
            && missingGlobal.OrderedWrites.IsEmpty
            && missingGlobal.UnreachedCallerEntryEDICaptureProvided
            && missingGlobal.UnreachedCallerEntryESICaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9171);

        CapacityFinalCalleeMaskCommonLifecyclePlan missingChildEDI =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(frameMinus8: 0x1234, frameMinus4: 0x20, global: 0xFFFFFFFF),
                CallerEntryEDI: null,
                CallerEntryESI: 0x01EDADC4));
        bool missingChildEDIHasExpectedTail =
            missingChildEDI.CommonTail is
            {
                Outcome: CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI,
                UnreachedEntryESICaptureProvided: true
            };
        check("A missing child EDI keeps its supplied later ESI unused but retains the global write",
            missingChildEDI.Outcome
                == CapacityFinalCalleeMaskCommonLifecycleOutcome.CommonTailMissingEntryEDI
            && missingChildEDI.ExactCleanHandoffToCommonTail
            && missingChildEDI.CommonTailReached
            && missingChildEDIHasExpectedTail
            && !missingChildEDI.CallerEntryEDICaptured
            && !missingChildEDI.CallerEntryESICaptured
            && missingChildEDI.OrderedWrites.Length == 1
            && missingChildEDI.OrderedWrites[0].Kind
                == CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal
            && missingChildEDI.FlagsAtStop == missingChildEDI.MaskTail.FinalFlags
            && !missingChildEDI.ExactCleanExitAdmitted
            && missingChildEDI.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection.LoadEAXRawFileOffset);

        CapacityFinalCalleeMaskCommonLifecyclePlan missingChildESI =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(frameMinus8: 0x1234, frameMinus4: 0x20, global: 0),
                CallerEntryEDI: 0,
                CallerEntryESI: null));
        CapacityFinalCalleeCommonTailTagWritesPlan? missingESITail =
            missingChildESI.CommonTail;
        check("A missing child ESI stops after OR without discarding the zero-valued global write",
            missingChildESI.Outcome
                == CapacityFinalCalleeMaskCommonLifecycleOutcome.CommonTailMissingEntryESI
            && missingESITail?.EAXAfterOr == 1
            && missingESITail?.OrFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildESI.OrderedWrites.Length == 1
            && missingChildESI.OrderedWrites[0].Kind
                == CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal
            && missingChildESI.OrderedWrites[0].Value == 0
            && missingChildESI.FlagsAtStop ==
                new CapacityFinalCalleeAlternateMaskTailFlags(
                    false, false, null, false, false, false)
            && !missingChildESI.ExactCleanExitAdmitted
            && missingChildESI.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset);

        CapacityFinalCalleeMaskCommonLifecyclePlan zeroCount =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(
                    frameMinus8: 0x80000001,
                    frameMinus4: 0xFFFFFFE0,
                    global: 0xFFFFFFFF,
                    entryFlags: KnownFlags),
                CallerEntryEDI: 4,
                CallerEntryESI: 0x01EDADC4));
        check("Masked count zero consumes entry flags and completes through exact 92C8",
            zeroCount.MaskTail.EffectiveShiftCount == 0
            && zeroCount.MaskTail.EntryFlagsConsumed
            && !zeroCount.MaskTail.UnusedEntryFlagsCaptureProvided
            && zeroCount.ExactCleanHandoffToCommonTail
            && zeroCount.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed
            && zeroCount.OrderedWrites.Select(write => write.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal,
                CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailFirstStore,
                CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailSecondStore
            })
            && zeroCount.ExactCleanExitAdmitted
            && zeroCount.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeMaskCommonLifecycleProjection
                    .CompletedBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeMaskCommonLifecyclePlan countOne =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(frameMinus8: 0x40000000, frameMinus4: 0xA1, global: 0xFFFFFFFF),
                CallerEntryEDI: 0xFFFFFFFE,
                CallerEntryESI: 0xFFFFFFFF));
        check("Count one leaves optional mask flags unused while the clean child computes its own OR flags",
            countOne.MaskTail.EffectiveShiftCount == 1
            && !countOne.MaskTail.EntryFlagsConsumed
            && !countOne.MaskTail.UnusedEntryFlagsCaptureProvided
            && countOne.CommonTail?.EAXAfterOr == 0xFFFFFFFF
            && countOne.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && countOne.ExactCleanExitAdmitted);

        CapacityFinalCalleeMaskCommonLifecyclePlan aliasedStores =
            CapacityFinalCalleeMaskCommonLifecycleProjection.Project(new(
                MaskCapture(frameMinus8: 0x1234, frameMinus4: 0, global: 0xFFFFFFFF),
                CallerEntryEDI: 4,
                CallerEntryESI: 0x01EDADC4));
        CapacityFinalCalleeCommonTailTagWritesPlan? aliasTail = aliasedStores.CommonTail;
        check("Aliased global and child stores remain ordered with independent values and final OR flags",
            aliasedStores.OrderedWrites.Select(write =>
                (write.Kind, write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (
                        CapacityFinalCalleeMaskCommonLifecycleWriteKind.MaskGlobal,
                        0x012C9171u,
                        0x01EDADC8u,
                        0xFFFFEDCBu),
                    (
                        CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailFirstStore,
                        0x012C92C2u,
                        0x01EDADC8u,
                        5u),
                    (
                        CapacityFinalCalleeMaskCommonLifecycleWriteKind.CommonTailSecondStore,
                        0x012C92C5u,
                        0x01EDADC8u,
                        4u)
                })
            && aliasTail?.Capture.EntryEDI == 4
            && aliasTail?.Capture.EntryESI == 0x01EDADC4
            && aliasTail?.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && aliasedStores.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && aliasedStores.ExactCleanExitAdmitted
            && aliasedStores.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeMaskCommonLifecycleProjection
                    .CompletedBoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeAlternateMaskTailCapture MaskCapture(
        uint frameMinus8,
        uint frameMinus4,
        uint global,
        CapacityFinalCalleeAlternateMaskTailFlags? entryFlags = null) =>
        new(
            EntryEBP: 0x100,
            FreshFrameMinus8: frameMinus8,
            FreshFrameMinus4: frameMinus4,
            FreshGlobalValue: global,
            EntryFlags: entryFlags);

    private static readonly CapacityFinalCalleeAlternateMaskTailFlags KnownFlags =
        new(true, false, true, false, true, false);
}
