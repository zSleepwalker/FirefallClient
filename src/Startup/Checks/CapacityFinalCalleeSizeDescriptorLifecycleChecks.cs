using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSizeDescriptorLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The size window joins descriptor reload only at the exact shared boundary",
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.SizeBoundaryRawFileOffsetExclusive
                == 0x012C8E9A
            && CapacityFinalCalleeSizeDescriptorLifecycleProjection.DescriptorEntryRawFileOffset
                == 0x012C8E9A
            && CapacityFinalCalleeSizeDescriptorLifecycleProjection.DescriptorBoundaryRawFileOffsetExclusive
                == 0x012C8EB8);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan equalSkip =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                descriptorEntryEbp: 0,
                descriptorEntryEbx: 0,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: null));
        check("The equal-size skip derives descriptor ESI and admits independent zero EBP and EBX",
            equalSkip.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFirstCompareEqualExit
            && equalSkip.SizePlan.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && equalSkip.SizeBoundaryReached
            && equalSkip.DescriptorReloadReached
            && equalSkip.DescriptorEntryESICaptured
            && equalSkip.DescriptorEntryESI == 0x100
            && equalSkip.DescriptorEntryEBPCaptured
            && equalSkip.DescriptorEntryEBP == 0
            && equalSkip.DescriptorEntryEBXCaptured
            && equalSkip.DescriptorEntryEBX == 0
            && equalSkip.DescriptorPlan is { } skipDescriptor
            && skipDescriptor.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && !skipDescriptor.UnreachedFreshEaxPlusCCaptureProvided
            && skipDescriptor.WrittenFrameMinus10
                != skipDescriptor.FreshFrameMinus10
            && skipDescriptor.FirstSelectedDestinationRawFileOffset == 0x012C8EBE
            && equalSkip.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan contaminatedFirstExit =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                descriptorEntryEbp: 0x20,
                descriptorEntryEbx: 0x30,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0x1234));
        check("An unreached EAX+0C capture contaminates a first-equality boundary but preserves its destination",
            contaminatedFirstExit.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedDescriptorBoundary
            && contaminatedFirstExit.DescriptorReloadReached
            && contaminatedFirstExit.DescriptorPlan is { } contaminatedFirstDescriptor
            && contaminatedFirstDescriptor.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && contaminatedFirstDescriptor.UnreachedFreshEaxPlusCCaptureProvided
            && contaminatedFirstExit.UnreachedFreshEaxPlusCCaptureProvided
            && !contaminatedFirstDescriptor.FreshEaxPlusCReadReached
            && contaminatedFirstDescriptor.FirstSelectedDestinationRawFileOffset == 0x012C8EBE
            && contaminatedFirstExit.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan returnedCall =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 0,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                opaqueCallDescriptorEntryEsi: 0,
                descriptorEntryEbp: 0,
                descriptorEntryEbx: 0,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: null));
        check("A normal opaque call requires and uses its independently captured ESI output",
            returnedCall.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFirstCompareEqualExit
            && returnedCall.SizePlan.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally
            && returnedCall.DescriptorEntryESICaptured
            && returnedCall.DescriptorEntryESI == 0
            && returnedCall.DescriptorEntryEBP == 0
            && returnedCall.DescriptorEntryEBX == 0
            && returnedCall.DescriptorPlan is { } returnedDescriptor
            && returnedDescriptor.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit
            && !returnedDescriptor.UnreachedFreshEaxPlusCCaptureProvided
            && returnedDescriptor.FirstSelectedDestinationRawFileOffset == 0x012C8EBE);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan missingIndependentRegisters =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 0,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                descriptorEntryEbp: null,
                descriptorEntryEbx: 0,
                opaqueCallDescriptorEntryEsi: null,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("Missing independent post-call registers cannot be supplied by the size window",
            missingIndependentRegisters.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingDescriptorEntryRegisters
            && missingIndependentRegisters.SizeBoundaryReached
            && !missingIndependentRegisters.DescriptorReloadReached
            && !missingIndependentRegisters.DescriptorEntryEBPCaptured
            && missingIndependentRegisters.DescriptorEntryEBXCaptured
            && missingIndependentRegisters.DescriptorEntryEBX == 0
            && !missingIndependentRegisters.DescriptorEntryESICaptured
            && missingIndependentRegisters.DescriptorPlan is null
            && missingIndependentRegisters.UnreachedInitialFrameMinus10CaptureProvided
            && missingIndependentRegisters.UnreachedFreshFrameMinus4CaptureProvided
            && missingIndependentRegisters.UnreachedFreshFrameMinus10CaptureProvided
            && missingIndependentRegisters.UnreachedFreshEaxPlusCCaptureProvided);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan contaminatedSkip =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                callCellTarget: 0x12345678,
                callReturnedNormally: true,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("An unreached cell capture contaminates a size-skip join without creating a descriptor plan",
            contaminatedSkip.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedSizeBoundary
            && contaminatedSkip.SizeBoundaryReached
            && !contaminatedSkip.DescriptorReloadReached
            && contaminatedSkip.DescriptorPlan is null
            && contaminatedSkip.SizePlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedSkip.SizePlan.UnreachedCallReturnStatusProvided
            && contaminatedSkip.UnreachedInitialFrameMinus10CaptureProvided
            && contaminatedSkip.UnreachedFreshFrameMinus4CaptureProvided
            && contaminatedSkip.UnreachedFreshFrameMinus10CaptureProvided
            && contaminatedSkip.UnreachedFreshEaxPlusCCaptureProvided
            && contaminatedSkip.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan suppliedSkippedCallOutput =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x55,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                opaqueCallDescriptorEntryEsi: 0x200,
                descriptorEntryEbp: 0x20,
                descriptorEntryEbx: 0x30));
        check("A skipped opaque call's supplied ESI output is unreached and cannot override entry ESI",
            suppliedSkippedCallOutput.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedSizeBoundary
            && suppliedSkippedCallOutput.UnreachedOpaqueCallDescriptorEntryESICaptureProvided
            && !suppliedSkippedCallOutput.DescriptorReloadReached
            && suppliedSkippedCallOutput.DescriptorPlan is null);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan missingSizeRead =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: null,
                descriptorEntryEbp: 0,
                descriptorEntryEbx: 0,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("Later descriptor captures cannot bypass a missing size read",
            missingSizeRead.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.SizeDidNotReachBoundary
            && missingSizeRead.SizePlan.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingFreshEsiPlus4
            && !missingSizeRead.SizeBoundaryReached
            && !missingSizeRead.DescriptorReloadReached
            && missingSizeRead.DescriptorPlan is null
            && missingSizeRead.UnreachedInitialFrameMinus10CaptureProvided
            && missingSizeRead.UnreachedFreshFrameMinus4CaptureProvided
            && missingSizeRead.UnreachedFreshFrameMinus10CaptureProvided
            && missingSizeRead.UnreachedFreshEaxPlusCCaptureProvided);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan unsignedBelow =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                normalCall: true,
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 0,
                opaqueCallDescriptorEntryEsi: 0x200,
                descriptorEntryEbp: 0x20,
                descriptorEntryEbx: 0x80000000,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x7FFFFFFF,
                freshFrameMinus10: 0x80000000,
                freshEaxPlusC: null));
        check("The descriptor JB route uses unsigned CMP at the high-bit boundary",
            unsignedBelow.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorEntryEaxUnsignedBelowExit
            && unsignedBelow.DescriptorReloadReached
            && unsignedBelow.DescriptorPlan is { } belowDescriptor
            && belowDescriptor.FirstCompareResult == 0xFFFFFFFF
            && belowDescriptor.FirstCompareEqual == false
            && belowDescriptor.EntryEaxUnsignedBelowEntryEbx == true
            && belowDescriptor.SecondBranchTaken == true
            && !belowDescriptor.UnreachedFreshEaxPlusCCaptureProvided
            && belowDescriptor.SecondSelectedDestinationRawFileOffset == 0x012C8EB8
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan contaminatedBelowExit =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                descriptorEntryEbp: 0x20,
                descriptorEntryEbx: 0x80000000,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x7FFFFFFF,
                freshFrameMinus10: 0x80000000,
                freshEaxPlusC: 0));
        check("An unreached EAX+0C capture contaminates the unsigned-below boundary without losing its destination",
            contaminatedBelowExit.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.ContaminatedDescriptorBoundary
            && contaminatedBelowExit.DescriptorReloadReached
            && contaminatedBelowExit.DescriptorPlan is { } contaminatedBelowDescriptor
            && contaminatedBelowDescriptor.Outcome
                == CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit
            && contaminatedBelowDescriptor.UnreachedFreshEaxPlusCCaptureProvided
            && contaminatedBelowExit.UnreachedFreshEaxPlusCCaptureProvided
            && !contaminatedBelowDescriptor.FreshEaxPlusCReadReached
            && contaminatedBelowDescriptor.SecondSelectedDestinationRawFileOffset == 0x012C8EB8
            && contaminatedBelowExit.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan finalEquality =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x55,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                descriptorEntryEbp: 0x20,
                descriptorEntryEbx: 0x300,
                initialFrameMinus10: 0,
                freshFrameMinus4: 0x400,
                freshFrameMinus10: 0x100,
                freshEaxPlusC: 0x55));
        check("The final descriptor equality route reads fresh EAX+0C independently",
            finalEquality.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFreshEaxPlusCEqualExit
            && finalEquality.DescriptorPlan is { } finalDescriptor
            && finalDescriptor.WrittenFrameMinus10
                != finalDescriptor.FreshFrameMinus10
            && finalDescriptor.FreshEaxPlusCAddress == 0x40C
            && finalDescriptor.FreshEaxPlusCReadReached
            && finalDescriptor.FreshEaxPlusC == 0x55
            && finalDescriptor.FinalBranchTaken == true
            && finalDescriptor.FinalSelectedDestinationRawFileOffset == 0x012C8EBE
            && finalEquality.StopAtRawFileOffsetExclusive == 0x012C8EBE);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan fallThrough =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                normalCall: true,
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 0,
                opaqueCallDescriptorEntryEsi: 0x10,
                descriptorEntryEbp: 8,
                descriptorEntryEbx: 0,
                initialFrameMinus10: 0xFFFFFFFF,
                freshFrameMinus4: 0xFFFFFFF8,
                freshFrameMinus10: 0,
                freshEaxPlusC: 1));
        check("The descriptor fall-through is a bounded EB8 outcome with wrapped addresses",
            fallThrough.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.DescriptorFellThroughToBoundary
            && fallThrough.DescriptorPlan is { } fallThroughDescriptor
            && fallThroughDescriptor.InitialFrameMinus10Address == 0xFFFFFFF8
            && fallThroughDescriptor.ComputedDescriptorAddress == 0x01EDADE8
            && fallThroughDescriptor.WrittenFrameMinus10 == 0x01EDADE8
            && fallThroughDescriptor.FreshEaxPlusCAddress == 4
            && fallThroughDescriptor.FinalBranchTaken == false
            && fallThroughDescriptor.FinalSelectedDestinationRawFileOffset is null
            && fallThrough.StopAtRawFileOffsetExclusive == 0x012C8EB8);

        CapacityFinalCalleeSizeDescriptorLifecyclePlan missingDescriptorReload =
            CapacityFinalCalleeSizeDescriptorLifecycleProjection.Project(Capture(
                sizeEntryEsi: 0x100,
                sizeEntryEax: 1,
                freshEsiPlus4: 8,
                descriptorEntryEbp: 0,
                descriptorEntryEbx: 0,
                initialFrameMinus10: null,
                freshFrameMinus4: 0,
                freshFrameMinus10: 0,
                freshEaxPlusC: 0));
        check("A missing descriptor read remains distinct from captured zero and later supplied reads",
            missingDescriptorReload.Outcome
                == CapacityFinalCalleeSizeDescriptorLifecycleOutcome.MissingInitialFrameMinus10
            && missingDescriptorReload.DescriptorReloadReached
            && missingDescriptorReload.DescriptorPlan is { } missingDescriptor
            && missingDescriptor.InitialFrameMinus10ReadReached
            && !missingDescriptor.InitialFrameMinus10Captured
            && missingDescriptor.InitialFrameMinus10 is null
            && missingDescriptor.UnreachedFreshFrameMinus4CaptureProvided
            && missingDescriptor.UnreachedFreshFrameMinus10CaptureProvided
            && missingDescriptor.UnreachedFreshEaxPlusCCaptureProvided);
    }

    private static CapacityFinalCalleeSizeDescriptorLifecycleCapture Capture(
        bool normalCall = false,
        bool captureValidatedEntry = true,
        uint sizeEntryEsi = 0x100,
        uint sizeEntryEax = 1,
        uint? freshEsiPlus4 = 8,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null,
        uint? opaqueCallDescriptorEntryEsi = null,
        uint? descriptorEntryEbp = 0x20,
        uint? descriptorEntryEbx = 0x30,
        uint? initialFrameMinus10 = 1,
        uint? freshFrameMinus4 = 0x400,
        uint? freshFrameMinus10 = 0x100,
        uint? freshEaxPlusC = 0x200) =>
        new(
            new CapacityFinalCalleeSizeComparisonCallCapture(
                captureValidatedEntry,
                sizeEntryEsi,
                sizeEntryEax,
                EntryESP: 0x7000,
                freshEsiPlus4,
                normalCall ? callCellTarget ?? 0x12345678 : callCellTarget,
                normalCall ? callReturnedNormally ?? true : callReturnedNormally),
            opaqueCallDescriptorEntryEsi,
            descriptorEntryEbp,
            descriptorEntryEbx,
            initialFrameMinus10,
            freshFrameMinus4,
            freshFrameMinus10,
            freshEaxPlusC);
}
