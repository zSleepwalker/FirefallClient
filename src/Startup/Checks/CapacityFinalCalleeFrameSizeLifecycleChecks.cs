using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFrameSizeLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition joins only the exact frame and size boundaries",
            CapacityFinalCalleeFrameSizeLifecycleProjection.FrameBoundaryRawFileOffset
                == 0x012C8E87
            && CapacityFinalCalleeFrameSizeLifecycleProjection.SizeEntryRawFileOffset
                == 0x012C8E87
            && CapacityFinalCalleeFrameSizeLifecycleProjection.SizeBoundaryRawFileOffsetExclusive
                == 0x012C8E9A);

        CapacityFinalCalleeFrameSizeLifecyclePlan mismatchSkip =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                frameEntryEbp: 0x2004,
                frameEntryEsp: 0x7000,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 0,
                freshEsiPlus4: 0));
        check("A clean frame-mismatch skip derives only frame-entry ESI and ESP",
            mismatchSkip.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && mismatchSkip.FramePlan.StopAtRawFileOffsetExclusive == 0x012C8E87
            && mismatchSkip.SizeBoundaryReached
            && mismatchSkip.SizeEntryESICaptured
            && mismatchSkip.SizeEntryESI == 0x10
            && mismatchSkip.SizeEntryEAXCaptured
            && mismatchSkip.SizeEntryEAX == 0
            && mismatchSkip.SizeEntryESPCaptured
            && mismatchSkip.SizeEntryESP == 0x7000
            && mismatchSkip.SizeComparisonReached
            && mismatchSkip.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && mismatchSkip.SizePlan?.UnreachedFreshEsiPlus4CaptureProvided == false
            && mismatchSkip.SizePlan?.UnreachedCallCellTargetCaptureProvided == false
            && mismatchSkip.SizePlan?.UnreachedCallReturnStatusProvided == false
            && !mismatchSkip.UnreachedSizeEntryEAXCaptureProvided
            && !mismatchSkip.UnreachedFreshEsiPlus4CaptureProvided
            && !mismatchSkip.UnreachedSizeCallCellTargetCaptureProvided
            && !mismatchSkip.UnreachedSizeCallReturnStatusProvided
            && !mismatchSkip.UnreachedOpaqueCallSizeEntryESICaptureProvided
            && !mismatchSkip.UnreachedOpaqueCallSizeEntryESPCaptureProvided
            && mismatchSkip.SizePlan?.PreCallESI is null
            && mismatchSkip.SizePlan?.PreCallEAX is null
            && mismatchSkip.SizePlan?.PreCallESP is null
            && mismatchSkip.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.EqualSizesCallSkipped
            && mismatchSkip.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeFrameSizeLifecyclePlan normalFrameCall =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                frameEntryEbp: 0x2004,
                frameEntryEsp: 0x7000,
                freshFrameMinus4: 0x10,
                frameCallCellTarget: 0,
                frameCallReturnedNormally: true,
                opaqueCallSizeEntryEsi: 0x87654321,
                opaqueCallSizeEntryEsp: 0xABCDEF00,
                sizeEntryEax: 0,
                freshEsiPlus4: 0));
        check("A normally returned opaque frame call requires independent ESI and ESP captures",
            normalFrameCall.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.CallReturnedNormally
            && normalFrameCall.FramePlan.StopAtRawFileOffsetExclusive == 0x012C8E87
            && normalFrameCall.SizeEntryESI == 0x87654321
            && normalFrameCall.SizeEntryESP == 0xABCDEF00
            && normalFrameCall.SizeEntryESI != normalFrameCall.FramePlan.Capture.EntryESI
            && normalFrameCall.SizeEntryESP != normalFrameCall.FramePlan.Capture.EntryESP
            && normalFrameCall.SizeEntryEAX == 0
            && normalFrameCall.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && normalFrameCall.SizePlan?.UnreachedFreshEsiPlus4CaptureProvided == false
            && normalFrameCall.SizePlan?.UnreachedCallCellTargetCaptureProvided == false
            && normalFrameCall.SizePlan?.UnreachedCallReturnStatusProvided == false
            && !normalFrameCall.UnreachedSizeEntryEAXCaptureProvided
            && !normalFrameCall.UnreachedFreshEsiPlus4CaptureProvided
            && !normalFrameCall.UnreachedSizeCallCellTargetCaptureProvided
            && !normalFrameCall.UnreachedSizeCallReturnStatusProvided
            && !normalFrameCall.UnreachedOpaqueCallSizeEntryESICaptureProvided
            && !normalFrameCall.UnreachedOpaqueCallSizeEntryESPCaptureProvided
            && normalFrameCall.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeFrameSizeLifecyclePlan missingPostCallEsi =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                frameCallCellTarget: 0,
                frameCallReturnedNormally: true,
                opaqueCallSizeEntryEsi: null,
                opaqueCallSizeEntryEsp: 0x7000,
                sizeEntryEax: 0,
                freshEsiPlus4: 0));
        CapacityFinalCalleeFrameSizeLifecyclePlan missingPostCallEsp =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                frameCallCellTarget: 0,
                frameCallReturnedNormally: true,
                opaqueCallSizeEntryEsi: 0x12345678,
                opaqueCallSizeEntryEsp: null,
                sizeEntryEax: 0,
                freshEsiPlus4: 0));
        CapacityFinalCalleeFrameSizeLifecyclePlan missingSizeEax =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                freshFrameMinus4: 0x12345678,
                frameCallCellTarget: 0,
                frameCallReturnedNormally: true,
                opaqueCallSizeEntryEsi: 0x12345678,
                opaqueCallSizeEntryEsp: 0x7000,
                sizeEntryEax: null,
                freshEsiPlus4: 0));
        check("Missing post-call ESI, ESP, or independent EAX stops before the fresh size read",
            missingPostCallEsi.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeEntryRegisters
            && missingPostCallEsi.SizeBoundaryReached
            && !missingPostCallEsi.SizeEntryESICaptured
            && missingPostCallEsi.SizeEntryEAXCaptured
            && missingPostCallEsi.SizeEntryESPCaptured
            && !missingPostCallEsi.SizeComparisonReached
            && missingPostCallEsi.SizePlan is null
            && missingPostCallEsi.UnreachedFreshEsiPlus4CaptureProvided
            && missingPostCallEsi.StopAtRawFileOffsetExclusive == 0x012C8E87
            && missingPostCallEsp.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeEntryRegisters
            && missingPostCallEsp.SizeEntryESICaptured
            && !missingPostCallEsp.SizeEntryESPCaptured
            && !missingPostCallEsp.SizeComparisonReached
            && missingSizeEax.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeEntryRegisters
            && missingSizeEax.SizeEntryESICaptured
            && !missingSizeEax.SizeEntryEAXCaptured
            && missingSizeEax.SizeEntryESPCaptured
            && !missingSizeEax.SizeComparisonReached);

        CapacityFinalCalleeFrameSizeLifecyclePlan zeroCallCell =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                frameEntryEbp: 0x2004,
                frameEntryEsp: 0x7000,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 0,
                freshEsiPlus4: 0x21,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: true));
        check("Captured zero EAX, data words, and call-cell targets remain present values",
            zeroCallCell.SizeEntryEAXCaptured
            && zeroCallCell.SizeEntryEAX == 0
            && zeroCallCell.SizePlan?.FreshEsiPlus4Captured == true
            && zeroCallCell.SizePlan?.FreshEsiPlus4 == 0x21
            && zeroCallCell.SizePlan?.CallCellTargetCaptured == true
            && zeroCallCell.SizePlan?.CallCellTargetVirtualAddress == 0
            && zeroCallCell.SizePlan?.PreCallEAX == 0
            && zeroCallCell.SizePlan?.CallReturnedNormally == true
            && zeroCallCell.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.CallReturnedNormally
            && zeroCallCell.StopAtRawFileOffsetExclusive == 0x012C8E9A);

        CapacityFinalCalleeFrameSizeLifecyclePlan contaminatedSkip =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                frameCallCellTarget: 0,
                opaqueCallSizeEntryEsi: 0x1111,
                opaqueCallSizeEntryEsp: 0x2222,
                sizeEntryEax: 0,
                freshEsiPlus4: 0,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: true));
        check("An unreached frame-call capture contaminates a skip and blocks size projection",
            contaminatedSkip.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && contaminatedSkip.FramePlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedSkip.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.ContaminatedFrameBoundary
            && !contaminatedSkip.SizeBoundaryReached
            && !contaminatedSkip.SizeComparisonReached
            && contaminatedSkip.SizePlan is null
            && contaminatedSkip.UnreachedOpaqueCallSizeEntryESICaptureProvided
            && contaminatedSkip.UnreachedOpaqueCallSizeEntryESPCaptureProvided
            && contaminatedSkip.UnreachedSizeEntryEAXCaptureProvided
            && contaminatedSkip.UnreachedFreshEsiPlus4CaptureProvided
            && contaminatedSkip.UnreachedSizeCallCellTargetCaptureProvided
            && contaminatedSkip.StopAtRawFileOffsetExclusive == 0x012C8E87);

        CapacityFinalCalleeFrameSizeLifecyclePlan contaminatedSizeSkip =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                opaqueCallSizeEntryEsi: 0x1111,
                opaqueCallSizeEntryEsp: 0x2222,
                sizeEntryEax: 0,
                freshEsiPlus4: 0,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: true));
        CapacityFinalCalleeFrameSizeLifecyclePlan contaminatedSizeAfterNormalFrameCall =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x10,
                frameCallCellTarget: 0,
                frameCallReturnedNormally: true,
                opaqueCallSizeEntryEsi: 0x87654321,
                opaqueCallSizeEntryEsp: 0xABCDEF00,
                sizeEntryEax: 0,
                freshEsiPlus4: 0,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: true));
        check("Size bypasses with unreached call captures are contaminated on either frame route",
            contaminatedSizeSkip.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && contaminatedSizeSkip.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && contaminatedSizeSkip.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.ContaminatedSizeBoundary
            && contaminatedSizeSkip.UnreachedOpaqueCallSizeEntryESICaptureProvided
            && contaminatedSizeSkip.UnreachedOpaqueCallSizeEntryESPCaptureProvided
            && contaminatedSizeSkip.UnreachedSizeCallCellTargetCaptureProvided
            && contaminatedSizeSkip.UnreachedSizeCallReturnStatusProvided
            && contaminatedSizeSkip.StopAtRawFileOffsetExclusive == 0x012C8E9A
            && contaminatedSizeAfterNormalFrameCall.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.CallReturnedNormally
            && contaminatedSizeAfterNormalFrameCall.SizeEntryESI == 0x87654321
            && contaminatedSizeAfterNormalFrameCall.SizeEntryESP == 0xABCDEF00
            && contaminatedSizeAfterNormalFrameCall.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.EqualSizesCallSkipped
            && contaminatedSizeAfterNormalFrameCall.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.ContaminatedSizeBoundary
            && contaminatedSizeAfterNormalFrameCall.UnreachedSizeCallCellTargetCaptureProvided
            && contaminatedSizeAfterNormalFrameCall.UnreachedSizeCallReturnStatusProvided
            && contaminatedSizeAfterNormalFrameCall.StopAtRawFileOffsetExclusive
                == 0x012C8E9A);

        CapacityFinalCalleeFrameSizeLifecyclePlan missingFrameRead =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                freshFrameMinus4: null,
                sizeEntryEax: 0,
                freshEsiPlus4: 0));
        check("An unreached frame does not admit any size comparison",
            missingFrameRead.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.FrameDidNotReachBoundary
            && !missingFrameRead.SizeBoundaryReached
            && !missingFrameRead.SizeComparisonReached
            && missingFrameRead.SizePlan is null
            && missingFrameRead.UnreachedSizeEntryEAXCaptureProvided
            && missingFrameRead.UnreachedFreshEsiPlus4CaptureProvided
            && missingFrameRead.StopAtRawFileOffsetExclusive == 0x012C8E7C);

        CapacityFinalCalleeFrameSizeLifecyclePlan missingFreshSize =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 1,
                freshEsiPlus4: null,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: true));
        CapacityFinalCalleeFrameSizeLifecyclePlan missingSizeCallCell =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 1,
                freshEsiPlus4: 0x21,
                sizeCallCellTarget: null,
                sizeCallReturnedNormally: true));
        CapacityFinalCalleeFrameSizeLifecyclePlan missingSizeReturn =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 1,
                freshEsiPlus4: 0x21,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: null));
        CapacityFinalCalleeFrameSizeLifecyclePlan nonreturningSizeCall =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0x10,
                freshFrameMinus4: 0x11,
                sizeEntryEax: 1,
                freshEsiPlus4: 0x21,
                sizeCallCellTarget: 0,
                sizeCallReturnedNormally: false));
        check("Missing fresh size, cell, status, and nonreturn preserve the stop and upstream frame",
            missingFreshSize.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingFreshEsiPlus4
            && missingFreshSize.SizeBoundaryReached
            && missingFreshSize.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingFreshEsiPlus4
            && missingFreshSize.UnreachedSizeCallCellTargetCaptureProvided
            && missingFreshSize.UnreachedSizeCallReturnStatusProvided
            && missingFreshSize.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && missingFreshSize.StopAtRawFileOffsetExclusive == 0x012C8E87
            && missingSizeCallCell.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeCallCellTarget
            && missingSizeCallCell.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallCellTarget
            && missingSizeCallCell.UnreachedSizeCallReturnStatusProvided
            && missingSizeCallCell.StopAtRawFileOffsetExclusive == 0x012C8E94
            && missingSizeReturn.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.MissingSizeCallReturnStatus
            && missingSizeReturn.SizePlan?.Outcome
                == CapacityFinalCalleeSizeComparisonCallOutcome.MissingCallReturnStatus
            && missingSizeReturn.StopAtRawFileOffsetExclusive == 0x012C8E94
            && nonreturningSizeCall.Outcome
                == CapacityFinalCalleeFrameSizeLifecycleOutcome.SizeCallDidNotReturnNormally
            && nonreturningSizeCall.SizePlan?.CallReturnedNormally == false
            && nonreturningSizeCall.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && nonreturningSizeCall.StopAtRawFileOffsetExclusive == 0x012C8E94);

        CapacityFinalCalleeFrameSizeLifecyclePlan wrappedReads =
            CapacityFinalCalleeFrameSizeLifecycleProjection.Project(Capture(
                frameEntryEsi: 0xFFFFFFFE,
                frameEntryEbp: 0,
                frameEntryEsp: 0xFFFFFFFC,
                freshFrameMinus4: 0,
                sizeEntryEax: 0xE0000001,
                freshEsiPlus4: 0x0F));
        check("Frame EBP-4, size ESI+4, and shifted EAX use DWORD wraparound independently",
            wrappedReads.FramePlan.FreshFrameMinus4Address == 0xFFFFFFFC
            && wrappedReads.FramePlan.Outcome
                == CapacityFinalCalleeFrameComparisonCallOutcome.FrameMismatchCallSkipped
            && wrappedReads.SizeEntryESI == 0xFFFFFFFE
            && wrappedReads.SizeEntryESP == 0xFFFFFFFC
            && wrappedReads.SizePlan?.FreshEsiPlus4Address == 2
            && wrappedReads.SizePlan?.FreshEsiPlus4 == 0x0F
            && wrappedReads.SizePlan?.MaskedFreshEsiPlus4 == 8
            && wrappedReads.SizePlan?.ShiftedEntryEax == 8
            && wrappedReads.SizePlan?.Equal == true
            && wrappedReads.StopAtRawFileOffsetExclusive == 0x012C8E9A);
    }

    private static CapacityFinalCalleeFrameSizeLifecycleCapture Capture(
        uint frameEntryEsi = 0x12345678,
        uint frameEntryEbp = 0x1234567C,
        uint frameEntryEsp = 0x7000,
        uint? freshFrameMinus4 = 0x12345679,
        uint? frameCallCellTarget = null,
        bool? frameCallReturnedNormally = null,
        uint? opaqueCallSizeEntryEsi = null,
        uint? opaqueCallSizeEntryEsp = null,
        uint? sizeEntryEax = null,
        uint? freshEsiPlus4 = null,
        uint? sizeCallCellTarget = null,
        bool? sizeCallReturnedNormally = null) =>
        new(
            new CapacityFinalCalleeFrameComparisonCallCapture(
                CaptureValidatedEntry: true,
                EntryESI: frameEntryEsi,
                EntryEBP: frameEntryEbp,
                EntryESP: frameEntryEsp,
                FreshFrameMinus4: freshFrameMinus4,
                CallCellTargetVirtualAddress: frameCallCellTarget,
                CallReturnedNormally: frameCallReturnedNormally),
            opaqueCallSizeEntryEsi,
            opaqueCallSizeEntryEsp,
            sizeEntryEax,
            freshEsiPlus4,
            sizeCallCellTarget,
            sizeCallReturnedNormally);
}
