using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFinalMaskCommonLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeFinalMaskCommonLifecyclePlan cleanUnequal =
            Project(0x100, 0x20000000, 0x20000001, null, null, null,
                callerEdi: null, callerEsi: null);
        check("A clean bounded unequal 917C exit never enters the common tail",
            cleanUnequal.Outcome
                == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                    .FinalMaskCleanUnequalTo917C
            && cleanUnequal.FinalMask.ExactCleanExitAdmitted
            && cleanUnequal.FinalMask.UpstreamFinalFrameGate.JnzTaken == true
            && cleanUnequal.FinalMask.UpstreamFinalFrameGate
                .JnzSelectedDestinationRawFileOffset == 0x012C917C
            && cleanUnequal.CommonTail is null
            && !cleanUnequal.CommonTailReached
            && !cleanUnequal.ExactCleanHandoffToCommonTail
            && !cleanUnequal.UnreachedCallerEntryEDICaptureProvided
            && !cleanUnequal.UnreachedCallerEntryESICaptureProvided
            && cleanUnequal.OrderedWrites.IsEmpty
            && cleanUnequal.ExactCleanExitAdmitted
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan contaminatedUnequal =
            Project(0x100, 0x20000000, 0x20000001, 0x40000000, 1, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 8);
        check("Unreached mask and caller captures contaminate 917C and block later work",
            contaminatedUnequal.Outcome
                == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                    .FinalMaskContaminatedAtBoundary
            && !contaminatedUnequal.FinalMask.ExactCleanExitAdmitted
            && contaminatedUnequal.FinalMask.UnreachedFreshFrameMinus8CaptureProvided
            && contaminatedUnequal.FinalMask.UnreachedFreshFrameMinus4CaptureProvided
            && contaminatedUnequal.FinalMask.UnreachedFreshGlobalCaptureProvided
            && contaminatedUnequal.CommonTail is null
            && !contaminatedUnequal.CommonTailReached
            && contaminatedUnequal.UnreachedCallerEntryEDICaptureProvided
            && contaminatedUnequal.UnreachedCallerEntryESICaptureProvided
            && !contaminatedUnequal.ExactCleanExitAdmitted
            && contaminatedUnequal.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan missingUpstream =
            Project(null, 0, 0, 0, 0, 0, callerEdi: 4, callerEsi: 8);
        check("Missing upstream evidence rejects mask and caller captures",
            missingUpstream.Outcome
                == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome
                    .FinalMaskIncomplete
            && missingUpstream.FinalMask.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamMissingEntryEBP
            && missingUpstream.FinalMask.MaskTail is null
            && missingUpstream.FinalMask.UnreachedFreshFrameMinus8CaptureProvided
            && missingUpstream.FinalMask.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstream.FinalMask.UnreachedFreshGlobalCaptureProvided
            && missingUpstream.UnreachedCallerEntryEDICaptureProvided
            && missingUpstream.UnreachedCallerEntryESICaptureProvided
            && missingUpstream.CommonTail is null
            && !missingUpstream.ExactCleanExitAdmitted
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan missingMaskFrame =
            Project(0x10, 0x1234, 0x1234, null, 1, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 8);
        check("A missing fresh mask frame keeps the upstream CMP flags and rejects child captures",
            missingMaskFrame.FinalMask.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome
                    .MaskMissingFreshFrameMinus8
            && missingMaskFrame.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingMaskFrame.CommonTail is null
            && missingMaskFrame.UnreachedCallerEntryEDICaptureProvided
            && missingMaskFrame.UnreachedCallerEntryESICaptureProvided
            && missingMaskFrame.StopAtRawFileOffsetExclusive == 0x012C9167);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan missingGlobal =
            Project(0x20, 7, 7, 0x40000000, 1, null,
                callerEdi: 4, callerEsi: 8);
        check("Missing global evidence preserves shift and NOT flags without entering the child",
            missingGlobal.FinalMask.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshGlobalValue
            && missingGlobal.FinalMask.MaskTail?.NotReached == true
            && missingGlobal.FinalMask.MaskTail.FlagsAfterNot is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && missingGlobal.FlagsAtStop == missingGlobal.FinalMask.MaskTail.FlagsAfterNot
            && missingGlobal.OrderedWrites.IsEmpty
            && missingGlobal.CommonTail is null
            && missingGlobal.UnreachedCallerEntryEDICaptureProvided
            && missingGlobal.UnreachedCallerEntryESICaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9171);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan missingChildEDI =
            Project(0x100, 0x40000000, 0x40000000, 0x1234, 0x20, 0xFFFFFFFF,
                callerEdi: null, callerEsi: 0x01EDADC4);
        check("Missing child EDI retains the mask global write and leaves later ESI unused",
            missingChildEDI.Outcome
                == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome.CommonTailMissingEntryEDI
            && missingChildEDI.ExactCleanHandoffToCommonTail
            && missingChildEDI.CommonTailReached
            && missingChildEDI.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && missingChildEDI.CommonTail.UnreachedEntryESICaptureProvided
            && !missingChildEDI.CallerEntryEDICaptured
            && !missingChildEDI.CallerEntryESICaptured
            && missingChildEDI.OrderedWrites.Length == 1
            && missingChildEDI.OrderedWrites[0].Kind
                == CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal
            && missingChildEDI.OrderedWrites[0].Address == 0x01EDADC8
            && missingChildEDI.FlagsAtStop == missingChildEDI.FinalMask.MaskTail?.FinalFlags
            && !missingChildEDI.ExactCleanExitAdmitted
            && missingChildEDI.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan missingChildESI =
            Project(0x100, 0x1234, 0x1234, 0xFFFFFFFF, 0, 0,
                callerEdi: 0, callerEsi: null);
        check("Missing child ESI stops after OR and retains the global write",
            missingChildESI.Outcome
                == CapacityFinalCalleeFinalMaskCommonLifecycleOutcome.CommonTailMissingEntryESI
            && missingChildESI.CommonTail?.EAXAfterOr == 1
            && missingChildESI.CommonTail.OrFlags is
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
                == CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal
            && missingChildESI.OrderedWrites[0].Value == 0
            && missingChildESI.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && !missingChildESI.ExactCleanExitAdmitted
            && missingChildESI.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan zeroCount =
            Project(0x10, 0x80000001, 0x80000001, 0x80000001, 0xFFFFFFE0, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 0x01EDADC4);
        check("Zero masked count carries upstream CMP flags through the clean 92C8 exit",
            zeroCount.FinalMask.MaskTail?.EffectiveShiftCount == 0
            && zeroCount.FinalMask.MaskTail.EntryFlagsConsumed
            && zeroCount.FinalMask.UpstreamCompareFlagsDerivedForMaskHelper
            && zeroCount.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.Completed
            && zeroCount.OrderedWrites.Select(write => write.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal,
                CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailFirstStore,
                CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailSecondStore
            })
            && zeroCount.ExactCleanExitAdmitted
            && zeroCount.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan countOne =
            Project(0x40, 0x90000000, 0x90000000, 0x40000000, 0xA1, 0xFFFFFFFF,
                callerEdi: 0xFFFFFFFE, callerEsi: 0xFFFFFFFF);
        check("Count one uses shift flags, then exposes common-tail OR flags at stop",
            countOne.FinalMask.MaskTail?.EffectiveShiftCount == 1
            && !countOne.FinalMask.MaskTail.EntryFlagsConsumed
            && countOne.FinalMask.MaskTail.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: true
            }
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

        CapacityFinalCalleeFinalMaskCommonLifecyclePlan aliasedStores =
            Project(0x100, 0x1234, 0x1234, 0x1234, 0, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 0x01EDADC4);
        check("Aliased global and common stores keep their instruction order and independent values",
            aliasedStores.OrderedWrites.Select(write =>
                (write.Kind, write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (
                        CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.MaskGlobal,
                        0x012C9171u,
                        0x01EDADC8u,
                        0xFFFFEDCBu),
                    (
                        CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailFirstStore,
                        0x012C92C2u,
                        0x01EDADC8u,
                        5u),
                    (
                        CapacityFinalCalleeFinalMaskCommonLifecycleWriteKind.CommonTailSecondStore,
                        0x012C92C5u,
                        0x01EDADC8u,
                        4u)
                })
            && aliasedStores.FinalMask.MaskTail?.GlobalWriteValue == 0xFFFFEDCB
            && aliasedStores.CommonTail?.Capture.EntryEDI == 4
            && aliasedStores.CommonTail?.Capture.EntryESI == 0x01EDADC4
            && aliasedStores.CommonTail.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && aliasedStores.FlagsAtStop == new CapacityFinalCalleeAlternateMaskTailFlags(
                false, true, null, false, false, false)
            && aliasedStores.ExactCleanExitAdmitted
            && aliasedStores.StopAtRawFileOffsetExclusive == 0x012C92C8);
    }

    private static CapacityFinalCalleeFinalMaskCommonLifecyclePlan Project(
        uint? entryEbp,
        uint? freshFrameMinus14,
        uint? entryEcx,
        uint? freshFrameMinus8,
        uint? freshFrameMinus4,
        uint? freshGlobalValue,
        uint? callerEdi,
        uint? callerEsi) =>
        CapacityFinalCalleeFinalMaskCommonLifecycleProjection.Project(new(
            new(
                new(entryEbp, freshFrameMinus14, entryEcx),
                freshFrameMinus8,
                freshFrameMinus4,
                freshGlobalValue),
            callerEdi,
            callerEsi));
}
