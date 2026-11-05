using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateRootSizeLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The root gate feeds only the unequal 90F6 route into the size window",
            CapacityFinalCalleeAlternateRootGateProjection.AlternateDestinationRawFileOffset
                == CapacityFinalCalleeAlternateRootSizeLifecycleProjection
                    .GateAlternateRawFileOffset
            && CapacityFinalCalleeAlternateRootGateProjection.FallthroughDestinationRawFileOffset
                == CapacityFinalCalleeAlternateRootSizeLifecycleProjection
                    .GateFallthroughRawFileOffset
            && CapacityFinalCalleeAlternateAlignedSizeProjection.EntryRawFileOffset
                == CapacityFinalCalleeAlternateRootSizeLifecycleProjection
                    .GateAlternateRawFileOffset
            && CapacityFinalCalleeAlternateAlignedSizeProjection.StoreRawFileOffset
                < CapacityFinalCalleeAlternateAlignedSizeProjection.CompareRawFileOffset
            && CapacityFinalCalleeAlternateAlignedSizeProjection.CompareRawFileOffset
                < CapacityFinalCalleeAlternateAlignedSizeProjection.JnbRawFileOffset
            && CapacityFinalCalleeAlternateAlignedSizeProjection.FallthroughRawFileOffset
                == CapacityFinalCalleeAlternateRootSizeLifecycleProjection
                    .AlignedSizeFallthroughRawFileOffset
            && CapacityFinalCalleeAlternateAlignedSizeProjection.BranchTargetRawFileOffset
                == CapacityFinalCalleeAlternateRootSizeLifecycleProjection
                    .AlignedSizeBranchTargetRawFileOffset);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan belowThreshold =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 0,
                GateFreshGlobal1EDADDC: 1,
                AlignedEntryECX: 0xF8,
                AlignedEntryEDI: 0xFFFFFFFC,
                AlignedEntryEBP: 2));
        check("A clean unequal root and below-threshold size retain both gates and stop at 910C",
            belowThreshold.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .AlignedSizeBelowThresholdFallthrough
            && belowThreshold.GateExactCleanAdmission
            && belowThreshold.GateUnequalAlternateSelected
            && !belowThreshold.GateEqualFallthroughSelected
            && belowThreshold.GatePlan.CompareResult == 0xFFFFFFFF
            && belowThreshold.GatePlan.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowThreshold.GatePlan.JnzTaken == true
            && belowThreshold.GatePlan.JnzSelectedDestinationRawFileOffset == 0x012C90F6
            && belowThreshold.AlignedSizeHandoffReached
            && belowThreshold.AlignedSizeEntryValidatedAtHandoff
            && belowThreshold.AlignedSizePlan?.Capture.EntryValidated == true
            && belowThreshold.AlignedSizePlan?.AlignedECX == 0xF8
            && belowThreshold.AlignedSizePlan?.EDIAfterAdd == 0xF4
            && belowThreshold.AlignedSizePlan?.AddFlags is
            {
                Carry: true,
                AuxiliaryCarry: true,
                Zero: false
            }
            && belowThreshold.AlignedSizePlan?.AlignedSize == 0x1F
            && belowThreshold.AlignedSizePlan?.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0xFFFFFFFE,
                Value: 0x1F
            }
            && belowThreshold.AlignedSizePlan?.CompareResult == 0xFFFFFFFF
            && belowThreshold.AlignedSizePlan?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && belowThreshold.AlignedSizePlan?.JnbTaken == false
            && belowThreshold.AlignedSizePlan?.ExactCleanAdmission == true
            && belowThreshold.SelectedDestinationRawFileOffset == 0x012C910C
            && belowThreshold.AlignedSizeExactCleanAdmission
            && belowThreshold.ExactCleanAdmission
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan atThreshold =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 0x80000000,
                GateFreshGlobal1EDADDC: 0x80000001,
                AlignedEntryECX: 0x100,
                AlignedEntryEDI: 0xFFFFFF00,
                AlignedEntryEBP: 3));
        check("A clean unequal root and threshold size retain compare flags before JNB to 919B",
            atThreshold.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .AlignedSizeAtOrAboveThresholdBranchTarget
            && atThreshold.GateExactCleanAdmission
            && atThreshold.GatePlan.CompareResult == 0xFFFFFFFF
            && atThreshold.GatePlan.JnzTaken == true
            && atThreshold.GatePlan.JnzSelectedDestinationRawFileOffset == 0x012C90F6
            && atThreshold.AlignedSizePlan?.Capture.EntryValidated == true
            && atThreshold.AlignedSizePlan?.EDIAfterAdd == 0
            && atThreshold.AlignedSizePlan?.AddFlags is
            {
                Carry: true,
                Zero: true
            }
            && atThreshold.AlignedSizePlan?.AlignedSize == 0x20
            && atThreshold.AlignedSizePlan?.Store is
            {
                Address: 0xFFFFFFFF,
                Value: 0x20
            }
            && atThreshold.AlignedSizePlan?.FlagsAfterStore
                == atThreshold.AlignedSizePlan?.ShiftFlags
            && atThreshold.AlignedSizePlan?.CompareResult == 0
            && atThreshold.AlignedSizePlan?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && atThreshold.AlignedSizePlan?.JnbTaken == true
            && atThreshold.AlignedSizePlan?.ExactCleanAdmission == true
            && atThreshold.SelectedDestinationRawFileOffset == 0x012C919B
            && atThreshold.AlignedSizeExactCleanAdmission
            && atThreshold.ExactCleanAdmission
            && atThreshold.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan equalRoot =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 0,
                GateFreshGlobal1EDADDC: 0,
                AlignedEntryECX: 0,
                AlignedEntryEDI: 0,
                AlignedEntryEBP: 0));
        check("Equal root EDX stays bounded at 90D2 and rejects all later size captures",
            equalRoot.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .GateEqualFallthroughWithAlignedSizeCaptures
            && equalRoot.GateExactCleanAdmission
            && equalRoot.GateEqualFallthroughSelected
            && !equalRoot.GateUnequalAlternateSelected
            && equalRoot.GatePlan.CompareResult == 0
            && equalRoot.GatePlan.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalRoot.GatePlan.JnzTaken == false
            && equalRoot.GatePlan.JnzSelectedDestinationRawFileOffset == 0x012C90D2
            && !equalRoot.AlignedSizeHandoffReached
            && !equalRoot.AlignedSizeEntryValidatedAtHandoff
            && equalRoot.AlignedSizePlan is null
            && equalRoot.AlignedSizeCapturesRejected
            && equalRoot.UnreachedAlignedEntryECXCaptureProvided
            && equalRoot.UnreachedAlignedEntryEDICaptureProvided
            && equalRoot.UnreachedAlignedEntryEBPCaptureProvided
            && !equalRoot.AlignedSizeExactCleanAdmission
            && !equalRoot.ExactCleanAdmission
            && equalRoot.SelectedDestinationRawFileOffset is null
            && equalRoot.StopAtRawFileOffsetExclusive == 0x012C90D2);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan cleanEqual =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 0,
                GateFreshGlobal1EDADDC: 0,
                AlignedEntryECX: null,
                AlignedEntryEDI: null,
                AlignedEntryEBP: null));
        check("A clean equal root remains admitted only at its bounded 90D2 exit",
            cleanEqual.ExactCleanAdmission
            && cleanEqual.AlignedSizePlan is null
            && !cleanEqual.AlignedSizeHandoffReached
            && cleanEqual.StopAtRawFileOffsetExclusive == 0x012C90D2);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan missingEntry =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: null,
                GateFreshGlobal1EDADDC: 1,
                AlignedEntryECX: 0,
                AlignedEntryEDI: 0,
                AlignedEntryEBP: 0));
        CapacityFinalCalleeAlternateRootSizeLifecyclePlan missingGlobal =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 0,
                GateFreshGlobal1EDADDC: null,
                AlignedEntryECX: 0,
                AlignedEntryEDI: 0,
                AlignedEntryEBP: 0));
        check("Missing root EDX or global cannot be bypassed by later zero size captures",
            missingEntry.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .MissingGateEntryEDXWithAlignedSizeCaptures
            && !missingEntry.GatePlan.EntryEDXCaptured
            && !missingEntry.GatePlan.FreshDWORDReadReached
            && missingEntry.GatePlan.FreshDWORDAddress is null
            && missingEntry.GatePlan.LateFreshDWORDCaptureProvided
            && !missingEntry.GateExactCleanAdmission
            && !missingEntry.AlignedSizeHandoffReached
            && missingEntry.AlignedSizePlan is null
            && missingEntry.AlignedSizeCapturesRejected
            && missingEntry.UnreachedAlignedEntryECXCaptureProvided
            && missingEntry.UnreachedAlignedEntryEDICaptureProvided
            && missingEntry.UnreachedAlignedEntryEBPCaptureProvided
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C90CA
            && missingGlobal.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .MissingGateFreshGlobalWithAlignedSizeCaptures
            && missingGlobal.GatePlan.EntryEDXCaptured
            && missingGlobal.GatePlan.FreshDWORDReadReached
            && missingGlobal.GatePlan.FreshDWORDAddress == 0x01EDADDC
            && !missingGlobal.GatePlan.FreshDWORDCaptured
            && !missingGlobal.GatePlan.CompareReached
            && !missingGlobal.GateExactCleanAdmission
            && !missingGlobal.AlignedSizeHandoffReached
            && missingGlobal.AlignedSizePlan is null
            && missingGlobal.AlignedSizeCapturesRejected
            && missingGlobal.UnreachedAlignedEntryECXCaptureProvided
            && missingGlobal.UnreachedAlignedEntryEDICaptureProvided
            && missingGlobal.UnreachedAlignedEntryEBPCaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C90CA);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan missingECX =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 1,
                GateFreshGlobal1EDADDC: 0,
                AlignedEntryECX: null,
                AlignedEntryEDI: 0x100,
                AlignedEntryEBP: 0x200));
        CapacityFinalCalleeAlternateRootSizeLifecyclePlan missingEDI =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 1,
                GateFreshGlobal1EDADDC: 0,
                AlignedEntryECX: 0xF8,
                AlignedEntryEDI: null,
                AlignedEntryEBP: 0x200));
        CapacityFinalCalleeAlternateRootSizeLifecyclePlan missingEBP =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 1,
                GateFreshGlobal1EDADDC: 0,
                AlignedEntryECX: 0xF8,
                AlignedEntryEDI: 0x7FFFFFF8,
                AlignedEntryEBP: null));
        check("Missing size registers stop at their consumers and flag only later supplied captures",
            missingECX.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .MissingAlignedEntryECX
            && missingECX.AlignedSizeHandoffReached
            && missingECX.AlignedSizeEntryValidatedAtHandoff
            && missingECX.AlignedSizePlan?.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX
            && !missingECX.AlignedSizePlan!.AndReached
            && !missingECX.AlignedSizePlan.MoveEAXReached
            && !missingECX.AlignedSizePlan.AddReached
            && missingECX.UnreachedAlignedEntryEDICaptureProvided
            && missingECX.UnreachedAlignedEntryEBPCaptureProvided
            && !missingECX.ExactCleanAdmission
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C90F6
            && missingEDI.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .MissingAlignedEntryEDI
            && missingEDI.AlignedSizePlan?.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI
            && missingEDI.AlignedSizePlan?.AlignedECX == 0xF8
            && missingEDI.AlignedSizePlan?.FlagsAfterMove
                == missingEDI.AlignedSizePlan?.AndFlags
            && !missingEDI.AlignedSizePlan!.AddReached
            && missingEDI.UnreachedAlignedEntryEBPCaptureProvided
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C90FB
            && missingEBP.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .MissingAlignedEntryEBP
            && missingEBP.AlignedSizePlan?.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEBP
            && missingEBP.AlignedSizePlan?.AlignedSize == 0x1F
            && missingEBP.AlignedSizePlan?.ShiftFlags is not null
            && !missingEBP.AlignedSizePlan!.StoreReached
            && !missingEBP.AlignedSizePlan.CompareReached
            && !missingEBP.AlignedSizePlan.JnbReached
            && !missingEBP.ExactCleanAdmission
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9100);

        CapacityFinalCalleeAlternateRootSizeLifecyclePlan capturedSizeZero =
            CapacityFinalCalleeAlternateRootSizeLifecycleProjection.Project(new(
                GateEntryEDX: 2,
                GateFreshGlobal1EDADDC: 1,
                AlignedEntryECX: 0,
                AlignedEntryEDI: 0xFFFFFFFF,
                AlignedEntryEBP: 0));
        check("Captured zero size and wrapped zero-minus-four store remain distinct from missing inputs",
            capturedSizeZero.Outcome
                == CapacityFinalCalleeAlternateRootSizeLifecycleOutcome
                    .AlignedSizeBelowThresholdFallthrough
            && capturedSizeZero.AlignedSizePlan?.EntryECXCaptured == true
            && capturedSizeZero.AlignedSizePlan?.AlignedECX == 0
            && capturedSizeZero.AlignedSizePlan?.EDIAfterAdd == 0xFFFFFFFF
            && capturedSizeZero.AlignedSizePlan?.AlignedSize == 0
            && capturedSizeZero.AlignedSizePlan?.Store is
            {
                Address: 0xFFFFFFFC,
                Value: 0
            }
            && capturedSizeZero.AlignedSizePlan?.CompareResult == 0xFFFFFFE0
            && capturedSizeZero.AlignedSizePlan?.JnbTaken == false
            && capturedSizeZero.AlignedSizeExactCleanAdmission
            && capturedSizeZero.StopAtRawFileOffsetExclusive == 0x012C910C);
    }
}
