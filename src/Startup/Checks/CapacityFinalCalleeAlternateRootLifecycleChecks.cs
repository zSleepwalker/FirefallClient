using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateRootLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition requires the exact equal fallthrough and ends at 9441",
            CapacityFinalCalleeAlternateRootLifecycleProjection.GateFallthroughRawFileOffset
                == CapacityFinalCalleeAlternateRootGateProjection.FallthroughDestinationRawFileOffset
            && CapacityFinalCalleeAlternateRootLifecycleProjection.GateAlternateRawFileOffset
                == CapacityFinalCalleeAlternateRootGateProjection.AlternateDestinationRawFileOffset
            && CapacityFinalCalleeAlternateRootLifecycleProjection.FinalJumpTargetRawFileOffset
                == CapacityFinalCalleeAlternateRootWritesProjection.JumpTargetRawFileOffset
            && CapacityFinalCalleeAlternateRootLifecycleProjection.FinalJumpTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeAlternateRootLifecycleProjection.FinalJumpTargetVirtualAddress
                == 0x016CA041
            && CapacityFinalCalleeAlternateRootGateProjection.FreshDWORDGlobalVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeAlternateRootWritesProjection.FreshGlobalPeVirtualAddress
                == 0x01EDADD0
            && CapacityFinalCalleeAlternateRootGateProjection.FreshDWORDGlobalVirtualAddress
                != CapacityFinalCalleeAlternateRootWritesProjection.FreshGlobalPeVirtualAddress);

        CapacityFinalCalleeAlternateRootLifecyclePlan equalZero =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0,
                gateFreshGlobal1Edaddc: 0,
                downstreamFreshGlobal1Edadd0: 0x10,
                downstreamEntryEdi: 0,
                downstreamEntryEsi: 0));
        check("Equal zero passes the gate and independently reloads the downstream global",
            equalZero.Outcome == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
            && equalZero.GateExactCleanAdmission
            && equalZero.GatePlan.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && equalZero.GatePlan.JnzTaken == false
            && equalZero.GatePlan.JnzSelectedDestinationRawFileOffset == 0x012C90D2
            && equalZero.GateEqualFallthroughSelected
            && equalZero.DownstreamWritesReached
            && equalZero.WritesPlan?.FreshGlobal == 0x10
            && equalZero.WritesPlan?.AddResult == 0x10
            && equalZero.WritesPlan?.FirstStoreValue == 0
            && equalZero.WritesPlan?.GlobalStoreValue == 0x10
            && equalZero.ExactCleanExitAdmitted
            && equalZero.FinalJumpTargetRawFileOffset == 0x012C9441
            && equalZero.FinalJumpTargetVirtualAddress == 0x016CA041
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C9441
            && !equalZero.UnreachedDownstreamFreshGlobalCaptureProvided
            && !equalZero.UnreachedDownstreamEntryEDICaptureProvided
            && !equalZero.UnreachedDownstreamEntryESICaptureProvided);

        CapacityFinalCalleeAlternateRootLifecyclePlan equalNonzeroChangedReload =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0x81234567,
                gateFreshGlobal1Edaddc: 0x81234567,
                downstreamFreshGlobal1Edadd0: 0xFFFFFFFE,
                downstreamEntryEdi: 3,
                downstreamEntryEsi: 0x100));
        check("Equal nonzero admits only the exact false-JNZ route and keeps the fresh reload independent",
            equalNonzeroChangedReload.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
            && equalNonzeroChangedReload.GateExactCleanAdmission
            && equalNonzeroChangedReload.GatePlan.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.EqualToFallthrough
            && equalNonzeroChangedReload.GatePlan.JnzReached
            && equalNonzeroChangedReload.GatePlan.JnzTaken == false
            && equalNonzeroChangedReload.GatePlan.JnzSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlternateRootLifecycleProjection.GateFallthroughRawFileOffset
            && equalNonzeroChangedReload.GateEqualFallthroughSelected
            && equalNonzeroChangedReload.WritesPlan?.FreshGlobal == 0xFFFFFFFE
            && equalNonzeroChangedReload.WritesPlan?.AddResult == 1
            && equalNonzeroChangedReload.WritesPlan?.GlobalStoreValue == 1
            && equalNonzeroChangedReload.ExactCleanExitAdmitted
            && equalNonzeroChangedReload.FinalJumpTargetRawFileOffset == 0x012C9441
            && equalNonzeroChangedReload.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeAlternateRootLifecyclePlan cleanUnequal =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0,
                gateFreshGlobal1Edaddc: 1,
                downstreamFreshGlobal1Edadd0: null,
                downstreamEntryEdi: null,
                downstreamEntryEsi: null));
        check("A clean unequal gate remains bounded at 90F6 and cannot enter the writes window",
            cleanUnequal.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.GateUnequalToAlternateTarget
            && cleanUnequal.GateExactCleanAdmission
            && cleanUnequal.GatePlan.Outcome
                == CapacityFinalCalleeAlternateRootGateOutcome.UnequalToAlternateTarget
            && cleanUnequal.GatePlan.JnzTaken == true
            && cleanUnequal.GatePlan.JnzSelectedDestinationRawFileOffset == 0x012C90F6
            && !cleanUnequal.GateEqualFallthroughSelected
            && !cleanUnequal.DownstreamWritesReached
            && cleanUnequal.WritesPlan is null
            && cleanUnequal.Writes.IsEmpty
            && cleanUnequal.ExactCleanExitAdmitted
            && cleanUnequal.FinalJumpTargetRawFileOffset is null
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C90F6
            && !cleanUnequal.UnreachedDownstreamFreshGlobalCaptureProvided
            && !cleanUnequal.UnreachedDownstreamEntryEDICaptureProvided
            && !cleanUnequal.UnreachedDownstreamEntryESICaptureProvided);

        CapacityFinalCalleeAlternateRootLifecyclePlan contaminatedUnequal =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 4,
                gateFreshGlobal1Edaddc: 5,
                downstreamFreshGlobal1Edadd0: 0,
                downstreamEntryEdi: 0,
                downstreamEntryEsi: 0));
        check("A clean unequal gate rejects every supplied downstream capture",
            contaminatedUnequal.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome
                    .UnequalGateWithDownstreamCaptures
            && contaminatedUnequal.GateExactCleanAdmission
            && !contaminatedUnequal.GateEqualFallthroughSelected
            && !contaminatedUnequal.DownstreamWritesReached
            && contaminatedUnequal.DownstreamCapturesRejected
            && contaminatedUnequal.WritesPlan is null
            && contaminatedUnequal.Writes.IsEmpty
            && contaminatedUnequal.UnreachedDownstreamFreshGlobalCaptureProvided
            && contaminatedUnequal.UnreachedDownstreamEntryEDICaptureProvided
            && contaminatedUnequal.UnreachedDownstreamEntryESICaptureProvided
            && !contaminatedUnequal.ExactCleanExitAdmitted
            && contaminatedUnequal.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlternateRootLifecyclePlan missingGateEntry =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: null,
                gateFreshGlobal1Edaddc: 0,
                downstreamFreshGlobal1Edadd0: 0x10,
                downstreamEntryEdi: 0x20,
                downstreamEntryEsi: 0x30));
        CapacityFinalCalleeAlternateRootLifecyclePlan missingGateGlobal =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0,
                gateFreshGlobal1Edaddc: null,
                downstreamFreshGlobal1Edadd0: 0x10,
                downstreamEntryEdi: 0x20,
                downstreamEntryEsi: 0x30));
        check("Late downstream values cannot bypass either missing gate read",
            missingGateEntry.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingGateEntryEDX
            && missingGateEntry.GatePlan.LateFreshDWORDCaptureProvided
            && !missingGateEntry.GateExactCleanAdmission
            && !missingGateEntry.GateEqualFallthroughSelected
            && !missingGateEntry.DownstreamWritesReached
            && missingGateEntry.WritesPlan is null
            && missingGateEntry.UnreachedDownstreamFreshGlobalCaptureProvided
            && missingGateEntry.UnreachedDownstreamEntryEDICaptureProvided
            && missingGateEntry.UnreachedDownstreamEntryESICaptureProvided
            && !missingGateEntry.ExactCleanExitAdmitted
            && missingGateEntry.StopAtRawFileOffsetExclusive == 0x012C90CA
            && missingGateGlobal.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingGateFreshGlobal
            && !missingGateGlobal.GatePlan.LateFreshDWORDCaptureProvided
            && !missingGateGlobal.GateExactCleanAdmission
            && !missingGateGlobal.DownstreamWritesReached
            && missingGateGlobal.WritesPlan is null
            && missingGateGlobal.UnreachedDownstreamFreshGlobalCaptureProvided
            && missingGateGlobal.UnreachedDownstreamEntryEDICaptureProvided
            && missingGateGlobal.UnreachedDownstreamEntryESICaptureProvided
            && !missingGateGlobal.ExactCleanExitAdmitted
            && missingGateGlobal.StopAtRawFileOffsetExclusive == 0x012C90CA);

        CapacityFinalCalleeAlternateRootLifecyclePlan missingDownstreamGlobal =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0,
                gateFreshGlobal1Edaddc: 0,
                downstreamFreshGlobal1Edadd0: null,
                downstreamEntryEdi: 0x20,
                downstreamEntryEsi: 0x30));
        CapacityFinalCalleeAlternateRootLifecyclePlan missingDownstreamEdi =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 7,
                gateFreshGlobal1Edaddc: 7,
                downstreamFreshGlobal1Edadd0: 0xFFFFFFFE,
                downstreamEntryEdi: null,
                downstreamEntryEsi: 0x30));
        CapacityFinalCalleeAlternateRootLifecyclePlan missingDownstreamEsi =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 9,
                gateFreshGlobal1Edaddc: 9,
                downstreamFreshGlobal1Edadd0: 0xFFFFFFFE,
                downstreamEntryEdi: 3,
                downstreamEntryEsi: null));
        check("Downstream missing values retain all prior reached read, add, and store history",
            missingDownstreamGlobal.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamFreshGlobal
            && missingDownstreamGlobal.DownstreamWritesReached
            && missingDownstreamGlobal.WritesPlan?.Outcome
                == CapacityFinalCalleeAlternateRootWritesOutcome.MissingFreshGlobal
            && missingDownstreamGlobal.WritesPlan?.FreshGlobalReadReached == true
            && missingDownstreamGlobal.WritesPlan?.Writes.IsEmpty == true
            && missingDownstreamGlobal.WritesPlan?.UnreachedEntryEDICaptureProvided == true
            && missingDownstreamGlobal.WritesPlan?.UnreachedEntryESICaptureProvided == true
            && !missingDownstreamGlobal.UnreachedDownstreamFreshGlobalCaptureProvided
            && missingDownstreamGlobal.UnreachedDownstreamEntryEDICaptureProvided
            && missingDownstreamGlobal.UnreachedDownstreamEntryESICaptureProvided
            && missingDownstreamGlobal.StopAtRawFileOffsetExclusive == 0x012C90D2
            && missingDownstreamEdi.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamEntryEDI
            && missingDownstreamEdi.WritesPlan?.FreshGlobal == 0xFFFFFFFE
            && !missingDownstreamEdi.WritesPlan!.AddInstructionReached
            && missingDownstreamEdi.WritesPlan.UnreachedEntryESICaptureProvided
            && !missingDownstreamEdi.UnreachedDownstreamEntryEDICaptureProvided
            && missingDownstreamEdi.UnreachedDownstreamEntryESICaptureProvided
            && missingDownstreamEdi.WritesPlan.Writes.IsEmpty
            && missingDownstreamEdi.StopAtRawFileOffsetExclusive == 0x012C90D8
            && missingDownstreamEsi.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamEntryESI
            && missingDownstreamEsi.WritesPlan?.AddResult == 1
            && missingDownstreamEsi.WritesPlan?.Writes.IsEmpty == true
            && missingDownstreamEsi.WritesPlan?.FirstStoreReached == false
            && !missingDownstreamEsi.UnreachedDownstreamEntryESICaptureProvided
            && missingDownstreamEsi.StopAtRawFileOffsetExclusive == 0x012C90DA);

        CapacityFinalCalleeAlternateRootLifecyclePlan wrappedAliasedStores =
            CapacityFinalCalleeAlternateRootLifecycleProjection.Project(Capture(
                gateEntryEdx: 0x80000000,
                gateFreshGlobal1Edaddc: 0x80000000,
                downstreamFreshGlobal1Edadd0: 0,
                downstreamEntryEdi: 4,
                downstreamEntryEsi: 0xFFFFFFFC));
        check("The final store sequence preserves DWORD wrapping and ordered aliases",
            wrappedAliasedStores.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
            && wrappedAliasedStores.ExactCleanExitAdmitted
            && wrappedAliasedStores.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C90DA, 0x012C90E2, 0x012C90EB, 0x012C90EE })
            && wrappedAliasedStores.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x01EDADDC, 0x01EDADD0, 0, 0 })
            && wrappedAliasedStores.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 0xFFFFFFFC, 4, 5, 4 })
            && wrappedAliasedStores.Writes[2].Address == wrappedAliasedStores.Writes[3].Address
            && wrappedAliasedStores.Writes[2].Address == 0
            && wrappedAliasedStores.FinalJumpTargetRawFileOffset == 0x012C9441
            && wrappedAliasedStores.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static CapacityFinalCalleeAlternateRootLifecycleCapture Capture(
        uint? gateEntryEdx,
        uint? gateFreshGlobal1Edaddc,
        uint? downstreamFreshGlobal1Edadd0,
        uint? downstreamEntryEdi,
        uint? downstreamEntryEsi) =>
        new(
            gateEntryEdx,
            gateFreshGlobal1Edaddc,
            downstreamFreshGlobal1Edadd0,
            downstreamEntryEdi,
            downstreamEntryEsi);
}
