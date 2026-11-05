using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFinalMaskCommonRootLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan cleanUnequal =
            Project(0x100, 0x20000000, 0x20000001, null, null, null,
                callerEdi: null, callerEsi: null, freshGlobal: null);
        check("A clean bounded unequal 917C exit rejects the fresh root read",
            cleanUnequal.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonCleanUnequalTo917C
            && cleanUnequal.FinalMaskCommon.ExactCleanExitAdmitted
            && cleanUnequal.CommonTailRootGate is null
            && !cleanUnequal.RootGateReached
            && !cleanUnequal.UnreachedFreshGlobalCaptureProvided
            && cleanUnequal.FreshGlobalAddress is null
            && cleanUnequal.OrderedWrites.IsEmpty
            && cleanUnequal.ExactCleanAdmission
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C917C);

        var unusedRootCapture = Project(0x100, 0x20000000, 0x20000001,
            null, null, null, null, null, 0);
        check("A supplied unused root DWORD contaminates the bounded unequal exit",
            unusedRootCapture.UnreachedFreshGlobalCaptureProvided
            && !unusedRootCapture.ExactCleanAdmission
            && !unusedRootCapture.RootGateReached
            && unusedRootCapture.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan contaminatedUpstream =
            Project(0x100, 0x20000000, 0x20000001, 0x40000000, 1, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 8, freshGlobal: 7);
        check("Contaminated unequal upstream evidence blocks the child and leaves the global unused",
            contaminatedUpstream.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonContaminatedAtBoundary
            && !contaminatedUpstream.RootGateReached
            && contaminatedUpstream.UnreachedFreshGlobalCaptureProvided
            && contaminatedUpstream.FinalMaskCommon
                .UnreachedCallerEntryEDICaptureProvided
            && contaminatedUpstream.FinalMaskCommon
                .UnreachedCallerEntryESICaptureProvided
            && contaminatedUpstream.OrderedWrites.IsEmpty
            && !contaminatedUpstream.ExactCleanAdmission
            && contaminatedUpstream.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan missingUpstream =
            Project(null, 0, 0, 0, 0, 0,
                callerEdi: 4, callerEsi: 8, freshGlobal: 0);
        check("Missing upstream evidence blocks the child and marks all later captures unused",
            missingUpstream.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .FinalMaskCommonIncomplete
            && !missingUpstream.RootGateReached
            && missingUpstream.UnreachedFreshGlobalCaptureProvided
            && missingUpstream.UnreachedCallerEntryEDICaptureProvided
            && missingUpstream.UnreachedCallerEntryESICaptureProvided
            && missingUpstream.OrderedWrites.IsEmpty
            && !missingUpstream.ExactCleanAdmission
            && missingUpstream.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan missingMaskGlobal =
            Project(0x10, 0x1234, 0x1234, 0x40000000, 1, null,
                callerEdi: 4, callerEsi: 8, freshGlobal: 0);
        check("A missing upstream mask global never reaches the common tail root gate",
            missingMaskGlobal.FinalMaskCommon.FinalMask.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshGlobalValue
            && !missingMaskGlobal.RootGateReached
            && missingMaskGlobal.UnreachedFreshGlobalCaptureProvided
            && missingMaskGlobal.OrderedWrites.IsEmpty
            && !missingMaskGlobal.ExactCleanAdmission
            && missingMaskGlobal.StopAtRawFileOffsetExclusive == 0x012C9171);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan missingChildEDI =
            Project(0x100, 0x40000000, 0x40000000, 0x1234, 0x20, 0xFFFFFFFF,
                callerEdi: null, callerEsi: 0x01EDADC4, freshGlobal: 1);
        check("A missing child EDI prevents the root read and preserves the earlier mask write",
            missingChildEDI.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .CommonTailMissingEntryEDI
            && !missingChildEDI.RootGateReached
            && missingChildEDI.UnreachedFreshGlobalCaptureProvided
            && missingChildEDI.OrderedWrites.Length == 1
            && missingChildEDI.OrderedWrites[0].Kind
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind.MaskGlobal
            && missingChildEDI.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan missingChildESI =
            Project(0x100, 0x1234, 0x1234, 0xFFFFFFFF, 0, 0,
                callerEdi: 0, callerEsi: null, freshGlobal: 0);
        check("A missing child ESI blocks the root read after the OR without using the fresh global",
            missingChildESI.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .CommonTailMissingEntryESI
            && !missingChildESI.RootGateReached
            && missingChildESI.UnreachedFreshGlobalCaptureProvided
            && missingChildESI.OrderedWrites.Length == 1
            && missingChildESI.OrderedWrites[0].Kind
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind.MaskGlobal
            && missingChildESI.FinalMaskCommon.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildESI.FlagsAtStop == missingChildESI.FinalMaskCommon.FlagsAtStop
            && missingChildESI.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan missingRootGlobal =
            Project(0x100, 0x40000000, 0x40000000, 0x1234, 0x20, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: 0x20, freshGlobal: null);
        check("A missing fresh root global retains parent flags and all three ordered writes",
            missingRootGlobal.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .RootGateMissingFreshGlobalDWORD
            && missingRootGlobal.RootGateReached
            && missingRootGlobal.ChildEntryESIRetained
            && missingRootGlobal.ChildEntryESI == 0x20
            && missingRootGlobal.FreshGlobalReadReached
            && missingRootGlobal.FreshGlobalAddress == 0x01EDADDC
            && !missingRootGlobal.FreshGlobalCaptured
            && missingRootGlobal.CommonTailRootGate?.CompareReached == false
            && missingRootGlobal.FlagsAtStop == missingRootGlobal.FinalMaskCommon.FlagsAtStop
            && missingRootGlobal.OrderedWrites.Length == 3
            && !missingRootGlobal.ExactCleanAdmission
            && missingRootGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan zeroEqual =
            Project(0x10, 0x80000001, 0x80000001, 0x80000001, 0xFFFFFFE0, 0xFFFFFFFF,
                callerEdi: 0, callerEsi: 0, freshGlobal: 0);
        check("A fresh zero compares equal to retained child ESI and stops exactly at 92D0",
            zeroEqual.FreshGlobalCaptured
            && zeroEqual.FreshGlobalDWORD == 0
            && zeroEqual.ChildEntryESI == 0
            && zeroEqual.CommonTailRootGate?.Capture.EntryESI == 0
            && zeroEqual.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroEqual.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .RootGateEqualToFallthrough92D0
            && zeroEqual.CommonTailRootGate?.JnzTaken == false
            && zeroEqual.CommonTailRootGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C92D0
            && zeroEqual.ExactCleanAdmission
            && zeroEqual.OrderedWrites.Length == 3
            && zeroEqual.StopAtRawFileOffsetExclusive == 0x012C92D0);

        uint globalAddress =
            CapacityFinalCalleeFinalMaskCommonRootLifecycleProjection
                .FreshGlobalDWORDVirtualAddress;
        uint aliasingEntryESI = unchecked(globalAddress - 4);
        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan aliasedStores =
            Project(0x100, 0x1234, 0x1234, 0x1234, 0, 0xFFFFFFFF,
                callerEdi: 4, callerEsi: aliasingEntryESI,
                freshGlobal: 0xDEADBEEF);
        check("Aliased common writes do not replace the independently captured fresh root DWORD",
            aliasedStores.OrderedWrites.Select(write =>
                (write.Kind, write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind.MaskGlobal,
                        0x012C9171u,
                        0x01EDADC8u,
                        0xFFFFEDCBu),
                    (
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind
                            .CommonTailFirstStore,
                        0x012C92C2u,
                        globalAddress,
                        5u),
                    (
                        CapacityFinalCalleeFinalMaskCommonRootLifecycleWriteKind
                            .CommonTailSecondStore,
                        0x012C92C5u,
                        globalAddress,
                        4u)
                })
            && aliasedStores.FreshGlobalReadReached
            && aliasedStores.FreshGlobalAddress == globalAddress
            && aliasedStores.FreshGlobalDWORD == 0xDEADBEEF
            && aliasedStores.CommonTailRootGate?.Capture.EntryESI == aliasingEntryESI
            && aliasedStores.CommonTailRootGate?.Capture.FreshGlobalDWORD == 0xDEADBEEF
            && aliasedStores.CommonTailRootGate?.CompareResult
                == unchecked(aliasingEntryESI - 0xDEADBEEF)
            && aliasedStores.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .RootGateUnequalToBranch92EC
            && aliasedStores.ExactCleanAdmission
            && aliasedStores.StopAtRawFileOffsetExclusive == 0x012C92EC);

        CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan allCompareFlags =
            Project(0x100, 0x20000000, 0x20000000, 0x40000000, 0x1234, 0xFFFFFFFF,
                callerEdi: 0, callerEsi: 0, freshGlobal: 1);
        check("All six flags at stop come from the computed child CMP",
            allCompareFlags.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allCompareFlags.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allCompareFlags.Outcome
                == CapacityFinalCalleeFinalMaskCommonRootLifecycleOutcome
                    .RootGateUnequalToBranch92EC
            && allCompareFlags.CommonTailRootGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C92EC
            && allCompareFlags.ExactCleanAdmission
            && allCompareFlags.StopAtRawFileOffsetExclusive == 0x012C92EC);
    }

    private static CapacityFinalCalleeFinalMaskCommonRootLifecyclePlan Project(
        uint? entryEbp,
        uint? freshFrameMinus14,
        uint? entryEcx,
        uint? freshFrameMinus8,
        uint? freshFrameMinus4,
        uint? maskFreshGlobal,
        uint? callerEdi,
        uint? callerEsi,
        uint? freshGlobal) =>
        CapacityFinalCalleeFinalMaskCommonRootLifecycleProjection.Project(new(
            new(
                new(
                    new(entryEbp, freshFrameMinus14, entryEcx),
                    freshFrameMinus8,
                    freshFrameMinus4,
                    maskFreshGlobal),
                callerEdi,
                callerEsi),
            freshGlobal));
}
