using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescriptorPairedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeDescriptorPairedLifecyclePlan firstEquality =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBP: 0x500,
                        freshFrameMinus10: 0x100),
                    freshFrameMinus4: 0x300,
                    freshGlobalValue: 0));
        check("First descriptor equality carries entry EAX and EBP into the paired writes",
            firstEquality.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed
            && firstEquality.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .FirstComparisonEqualCallSkipped
            && firstEquality.DescriptorBoundaryReached
            && firstEquality.DescriptorPlan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeDescriptorPairedLifecycleProjection
                    .DescriptorBoundaryRawFileOffsetExclusive
            && firstEquality.PairedFieldWritesReached
            && firstEquality.PairedEntryEAXCaptured
            && firstEquality.PairedEntryEAX == 0x100
            && firstEquality.PairedEntryEBPCaptured
            && firstEquality.PairedEntryEBP == 0x500
            && firstEquality.PairedFieldWritesPlan?.FirstWriteAddress == 0x30C
            && firstEquality.PairedFieldWritesPlan?.FirstWriteValue == 0x100
            && firstEquality.PairedFieldWritesPlan?.SecondWriteAddress == 0x108
            && firstEquality.PairedFieldWritesPlan?.SecondWriteValue == 0x300
            && firstEquality.PairedFieldWritesPlan?.EBXFresh == 0
            && firstEquality.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan memoryEquality =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBX: 0x100,
                        entryESI: 0,
                        entryEBP: 0x600,
                        freshFrameMinus10: 0x200,
                        freshEaxPlus8: 0),
                    freshFrameMinus4: 0x700,
                    freshGlobalValue: 0x800));
        check("Memory equality also bypasses the call and carries entry registers",
            memoryEquality.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed
            && memoryEquality.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .MemoryComparisonEqualCallSkipped
            && memoryEquality.DescriptorPlan.MemoryEqualityBranchTaken == true
            && memoryEquality.PairedEntryEAX == 0x100
            && memoryEquality.PairedEntryEBP == 0x600
            && memoryEquality.PairedFieldWritesPlan?.ECXLoaded == 0x700);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan unsignedBelowCall =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBX: 0x200,
                        entryEBP: 0x1000,
                        freshFrameMinus10: 0,
                        callCellTarget: 0x4000,
                        callReturnedNormally: true),
                    opaqueCallPairedEntryEAX: 0xABC,
                    opaqueCallPairedEntryEBP: 0xDEF,
                    freshFrameMinus4: 0x123,
                    freshGlobalValue: 0x456));
        check("Unsigned-below call uses independent post-call EAX and EBP captures",
            unsignedBelowCall.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed
            && unsignedBelowCall.DescriptorPlan.UnsignedBelow == true
            && unsignedBelowCall.DescriptorPlan.CallReturnedNormally == true
            && unsignedBelowCall.DescriptorPlan.CallOutputsOpaque
            && unsignedBelowCall.PairedEntryEAX == 0xABC
            && unsignedBelowCall.PairedEntryEBP == 0xDEF
            && unsignedBelowCall.PairedFieldWritesPlan?.FirstWriteAddress == 0x12F
            && unsignedBelowCall.PairedFieldWritesPlan?.FirstWriteValue == 0xABC
            && unsignedBelowCall.PairedFieldWritesPlan?.SecondWriteAddress == 0xAC4
            && unsignedBelowCall.PairedFieldWritesPlan?.SecondWriteValue == 0x123
            && !unsignedBelowCall.UnreachedOpaqueCallPairedEntryEAXCaptureProvided
            && !unsignedBelowCall.UnreachedOpaqueCallPairedEntryEBPCaptureProvided);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan mismatchCall =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x200,
                        entryEBX: 0x100,
                        entryESI: 0,
                        entryEBP: 0x2000,
                        freshFrameMinus10: 0,
                        freshEaxPlus8: 0x10,
                        callCellTarget: 0x4000,
                        callReturnedNormally: true),
                    opaqueCallPairedEntryEAX: 0x55,
                    opaqueCallPairedEntryEBP: 0x66,
                    freshFrameMinus4: 0x77,
                    freshGlobalValue: 0));
        check("Memory mismatch reaches the opaque call and does not infer register preservation",
            mismatchCall.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed
            && mismatchCall.DescriptorPlan.MemoryCompareEqual == false
            && mismatchCall.DescriptorPlan.CallReached
            && mismatchCall.DescriptorPlan.CallReturnedNormally == true
            && mismatchCall.PairedEntryEAX == 0x55
            && mismatchCall.PairedEntryEAX != mismatchCall.DescriptorPlan.PreCallEAX
            && mismatchCall.PairedEntryEBP == 0x66
            && mismatchCall.PairedEntryEBP != mismatchCall.DescriptorPlan.PreCallEBP);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan missingReturnStatus =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x200,
                        entryEBX: 0x100,
                        entryESI: 0,
                        freshFrameMinus10: 0,
                        freshEaxPlus8: 0x10,
                        callCellTarget: 0x4000),
                    opaqueCallPairedEntryEAX: 1,
                    opaqueCallPairedEntryEBP: 2,
                    freshFrameMinus4: 3,
                    freshGlobalValue: 4));
        CapacityFinalCalleeDescriptorPairedLifecyclePlan nonreturningCall =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x200,
                        entryEBX: 0x100,
                        entryESI: 0,
                        freshFrameMinus10: 0,
                        freshEaxPlus8: 0x10,
                        callCellTarget: 0x4000,
                        callReturnedNormally: false),
                    opaqueCallPairedEntryEAX: 1,
                    opaqueCallPairedEntryEBP: 2,
                    freshFrameMinus4: 3,
                    freshGlobalValue: 4));
        check("Missing and nonreturning call captures preserve call state and leave later captures unreached",
            missingReturnStatus.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .DescriptorDidNotReachBoundary
            && missingReturnStatus.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .MissingCallReturnStatus
            && missingReturnStatus.DescriptorPlan.CallReached
            && missingReturnStatus.DescriptorPlan.CallOutputsOpaque
            && missingReturnStatus.UnreachedOpaqueCallPairedEntryEAXCaptureProvided
            && missingReturnStatus.UnreachedOpaqueCallPairedEntryEBPCaptureProvided
            && missingReturnStatus.UnreachedFreshFrameMinus4CaptureProvided
            && missingReturnStatus.UnreachedFreshGlobalValueCaptureProvided
            && !missingReturnStatus.PairedFieldWritesReached
            && nonreturningCall.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome.CallDidNotReturnNormally
            && nonreturningCall.DescriptorPlan.CallReturnedNormally == false
            && nonreturningCall.UnreachedOpaqueCallPairedEntryEAXCaptureProvided
            && nonreturningCall.UnreachedOpaqueCallPairedEntryEBPCaptureProvided
            && !nonreturningCall.PairedFieldWritesReached);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan missingPostCallEAX =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x200,
                        entryEBX: 0x100,
                        entryESI: 0,
                        freshFrameMinus10: 0,
                        freshEaxPlus8: 0x10,
                        callCellTarget: 0x4000,
                        callReturnedNormally: true),
                    opaqueCallPairedEntryEBP: 0,
                    freshFrameMinus4: 0,
                    freshGlobalValue: 0));
        CapacityFinalCalleeDescriptorPairedLifecyclePlan missingPostCallEBP =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x200,
                        entryEBX: 0x100,
                        entryESI: 0,
                        freshFrameMinus10: 0,
                        freshEaxPlus8: 0x10,
                        callCellTarget: 0x4000,
                        callReturnedNormally: true),
                    opaqueCallPairedEntryEAX: 0,
                    freshFrameMinus4: 0,
                    freshGlobalValue: 0));
        check("A normal opaque return still requires both independently captured registers",
            missingPostCallEAX.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingPairedEntryRegisters
            && missingPostCallEAX.DescriptorBoundaryReached
            && !missingPostCallEAX.PairedEntryEAXCaptured
            && missingPostCallEAX.PairedEntryEBPCaptured
            && missingPostCallEAX.PairedEntryEBP == 0
            && !missingPostCallEAX.PairedFieldWritesReached
            && missingPostCallEAX.UnreachedFreshFrameMinus4CaptureProvided
            && missingPostCallEAX.UnreachedFreshGlobalValueCaptureProvided
            && missingPostCallEBP.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingPairedEntryRegisters
            && missingPostCallEBP.PairedEntryEAXCaptured
            && missingPostCallEBP.PairedEntryEAX == 0
            && !missingPostCallEBP.PairedEntryEBPCaptured
            && !missingPostCallEBP.PairedFieldWritesReached);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan bypassCallRegisterContamination =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBP: 0x500,
                        freshFrameMinus10: 0x100),
                    opaqueCallPairedEntryEAX: 0,
                    freshFrameMinus4: 0x300,
                    freshGlobalValue: 0));
        CapacityFinalCalleeDescriptorPairedLifecyclePlan nestedCaptureContamination =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBP: 0x500,
                        freshFrameMinus10: 0x100,
                        callCellTarget: 0),
                    freshFrameMinus4: 0x300,
                    freshGlobalValue: 0));
        check("Skipped-call registers and other unreached descriptor captures contaminate the exact boundary",
            bypassCallRegisterContamination.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .ContaminatedDescriptorBoundary
            && bypassCallRegisterContamination.UnreachedOpaqueCallPairedEntryEAXCaptureProvided
            && bypassCallRegisterContamination.UnreachedFreshFrameMinus4CaptureProvided
            && bypassCallRegisterContamination.UnreachedFreshGlobalValueCaptureProvided
            && !bypassCallRegisterContamination.PairedFieldWritesReached
            && nestedCaptureContamination.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .ContaminatedDescriptorBoundary
            && nestedCaptureContamination.DescriptorPlan
                .UnreachedCallCellTargetCaptureProvided
            && nestedCaptureContamination.UnreachedFreshFrameMinus4CaptureProvided
            && !nestedCaptureContamination.PairedFieldWritesReached);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan missingDescriptorFrame =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        freshFrameMinus10: null,
                        freshEaxPlus8: 0,
                        callCellTarget: 0,
                        callReturnedNormally: true),
                    opaqueCallPairedEntryEAX: 1,
                    opaqueCallPairedEntryEBP: 2,
                    freshFrameMinus4: 3,
                    freshGlobalValue: 4));
        check("Captures after an unavailable descriptor read remain explicitly unreached",
            missingDescriptorFrame.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .DescriptorDidNotReachBoundary
            && missingDescriptorFrame.DescriptorPlan.Outcome
                == CapacityFinalCalleeDescriptorComparisonCallOutcome
                    .MissingFreshFrameMinus10
            && missingDescriptorFrame.DescriptorPlan
                .UnreachedFreshEaxPlus8CaptureProvided
            && missingDescriptorFrame.DescriptorPlan
                .UnreachedCallCellTargetCaptureProvided
            && missingDescriptorFrame.DescriptorPlan
                .UnreachedCallReturnStatusProvided
            && missingDescriptorFrame.UnreachedOpaqueCallPairedEntryEAXCaptureProvided
            && missingDescriptorFrame.UnreachedOpaqueCallPairedEntryEBPCaptureProvided
            && missingDescriptorFrame.UnreachedFreshFrameMinus4CaptureProvided
            && missingDescriptorFrame.UnreachedFreshGlobalValueCaptureProvided
            && !missingDescriptorFrame.PairedFieldWritesReached);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan partialWrites =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBP: 0x500,
                        freshFrameMinus10: 0x100),
                    freshFrameMinus4: null,
                    freshGlobalValue: 0));
        CapacityFinalCalleeDescriptorPairedLifecyclePlan missingGlobal =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0x100,
                        entryEBP: 0x500,
                        freshFrameMinus10: 0x100),
                    freshFrameMinus4: 0x300,
                    freshGlobalValue: null));
        check("Nested partial reads preserve missing-versus-zero outcomes and ordered writes",
            partialWrites.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingFreshFrameMinus4
            && partialWrites.PairedFieldWritesPlan is
            {
                FreshFrameMinus4ReadReached: true,
                FreshFrameMinus4Captured: false,
                FirstWriteReached: false,
                SecondWriteReached: false,
                FreshGlobalReadReached: false,
                UnreachedFreshGlobalValueCaptureProvided: true
            }
            && partialWrites.DescriptorPlan.FirstCompareEqual == true
            && missingGlobal.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.PairedFieldWritesPlan is
            {
                FirstWriteReached: true,
                FirstWriteValue: 0x100,
                SecondWriteReached: true,
                SecondWriteValue: 0x300,
                FreshGlobalReadReached: true,
                FreshGlobalValueCaptured: false,
                EBXFresh: null
            }
            && !missingGlobal.UnreachedFreshGlobalValueCaptureProvided);

        CapacityFinalCalleeDescriptorPairedLifecyclePlan wrappedAliasing =
            CapacityFinalCalleeDescriptorPairedLifecycleProjection.Project(
                LifecycleCapture(
                    descriptor: Descriptor(
                        entryEAX: 0xFFFFFFFC,
                        entryEBP: 0,
                        freshFrameMinus10: 0xFFFFFFFC),
                    freshFrameMinus4: 0xFFFFFFF8,
                    freshGlobalValue: 0));
        check("The composed path preserves DWORD wrapping, write aliasing, and independent global zero",
            wrappedAliasing.Outcome
                == CapacityFinalCalleeDescriptorPairedLifecycleOutcome.Completed
            && wrappedAliasing.PairedFieldWritesPlan?.FreshFrameMinus4Address
                == 0xFFFFFFFC
            && wrappedAliasing.PairedFieldWritesPlan?.FirstWriteAddress == 4
            && wrappedAliasing.PairedFieldWritesPlan?.SecondWriteAddress == 4
            && wrappedAliasing.PairedFieldWritesPlan?.FirstWriteValue == 0xFFFFFFFC
            && wrappedAliasing.PairedFieldWritesPlan?.SecondWriteValue == 0xFFFFFFF8
            && wrappedAliasing.PairedFieldWritesPlan?.FreshGlobalValueCaptured == true
            && wrappedAliasing.PairedFieldWritesPlan?.EBXFresh == 0);
    }

    private static CapacityFinalCalleeDescriptorComparisonCallCapture Descriptor(
        bool captureValidatedEntry = true,
        uint entryEAX = 0x10,
        uint entryEBX = 0x10,
        uint entryESI = 0x10,
        uint entryEBP = 0x7000,
        uint entryESP = 0x7000,
        uint? freshFrameMinus10 = 0x20,
        uint? freshEaxPlus8 = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null) =>
        new(
            captureValidatedEntry,
            entryEAX,
            entryEBX,
            entryESI,
            entryEBP,
            entryESP,
            freshFrameMinus10,
            freshEaxPlus8,
            callCellTarget,
            callReturnedNormally);

    private static CapacityFinalCalleeDescriptorPairedLifecycleCapture LifecycleCapture(
        CapacityFinalCalleeDescriptorComparisonCallCapture? descriptor = null,
        uint? opaqueCallPairedEntryEAX = null,
        uint? opaqueCallPairedEntryEBP = null,
        uint? freshFrameMinus4 = 0x300,
        uint? freshGlobalValue = 0x400) =>
        new(
            descriptor ?? Descriptor(),
            opaqueCallPairedEntryEAX,
            opaqueCallPairedEntryEBP,
            freshFrameMinus4,
            freshGlobalValue);

}
