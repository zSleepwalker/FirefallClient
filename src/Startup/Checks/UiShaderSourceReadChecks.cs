using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceReadChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source fread import and captured call anchors are recorded",
            UiShaderSourceReadProjectionRules.FreadImportAddressVirtualAddress == 0x1AC490C
            && UiShaderSourceReadProjectionRules.FreadImportAddressRawFileOffset == 0x16C350C
            && UiShaderSourceReadProjectionRules.FreadImportModule == "MSVCR120.dll"
            && UiShaderSourceReadProjectionRules.FreadImportName == "fread"
            && UiShaderSourceReadProjectionRules.CapturedSizeLoadRawFileOffset == 0x12BB366
            && UiShaderSourceReadProjectionRules.InitialSizeZeroCheckRawFileOffset == 0x12BB3C8
            && UiShaderSourceReadProjectionRules.InitialSizeZeroBranchTargetRawFileOffset == 0x12BB42E
            && UiShaderSourceReadProjectionRules.ReceiverBufferVirtualCallRawFileOffset == 0x12BB403
            && UiShaderSourceReadProjectionRules.ReceiverBufferVirtualCallVtableSlotOffset == 0x04
            && UiShaderSourceReadProjectionRules.RequestedLengthCompareRawFileOffset == 0x12BB3D0
            && UiShaderSourceReadProjectionRules.RequestedLengthClampRawFileOffset == 0x12BB3D3
            && UiShaderSourceReadProjectionRules.RequestedLengthSlotSetupRawFileOffset == 0x12BB3DA
            && UiShaderSourceReadProjectionRules.RequestedLengthReceiverOffset == 0x1C
            && UiShaderSourceReadProjectionRules.RequestedLengthZeroTestRawFileOffset == 0x12BB3E3
            && UiShaderSourceReadProjectionRules.RequestedLengthZeroBranchRawFileOffset == 0x12BB3E5
            && UiShaderSourceReadProjectionRules.FreadCallRawFileOffset == 0x12BB40E
            && UiShaderSourceReadProjectionRules.FreadResultSuccessTestRawFileOffset == 0x12BB417
            && UiShaderSourceReadProjectionRules.FreadResultFailureBranchRawFileOffset == 0x12BB41A
            && UiShaderSourceReadProjectionRules.CumulativeByteOffsetAddRawFileOffset == 0x12BB420
            && UiShaderSourceReadProjectionRules.RemainingByteCountSubtractRawFileOffset == 0x12BB426
            && UiShaderSourceReadProjectionRules.ReadLoopContinuationBranchRawFileOffset == 0x12BB428
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupVirtualCallRawFileOffset == 0x12BB445
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupVirtualCallVtableSlotOffset == 0x14
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupContinuationRawFileOffset == 0x12BB448
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupCallReturnValueIsIgnored
            && UiShaderSourceReadProjectionRules.CommonCleanupRawFileOffset == 0x12BB372
            && UiShaderSourceReadProjectionRules.FailedReadReturnFlagClearRawFileOffset == 0x12BB37B
            && UiShaderSourceReadProjectionRules.CompletedReadReturnFlagSetRawFileOffset == 0x12BB438
            && UiShaderSourceReadProjectionRules.ReturnAlTransferRawFileOffset == 0x12BB45D
            && UiShaderSourceReadProjectionRules.ReturnCookieCheckCallRawFileOffset == 0x12BB474
            && UiShaderSourceReadProjectionRules.ReturnCookieCheckFunctionRawFileOffset == 0x136AC96
            && UiShaderSourceReadProjectionRules.ReturnCookieCheckMatchedReturnRawFileOffset == 0x136AC9E
            && UiShaderSourceReadProjectionRules.FunctionReturnRawFileOffset == 0x12BB47C);

        check("Shader-source read selects the remaining byte count up to the 16 MiB chunk cap",
            UiShaderSourceReadProjectionRules.MaximumChunkByteCount == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(0) == 0
            && UiShaderSourceReadProjectionRules.SelectChunk(1) == 1
            && UiShaderSourceReadProjectionRules.SelectChunk(0x01000000) == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(0x01000001) == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(uint.MaxValue) == 0x01000000);

        uint[] boundaryValues = { 0, 1, 0x7FFFFFFF, 0x80000000, uint.MaxValue };
        bool requestedLengthSelectionIsUnsigned = true;
        bool requestedLengthChunkCompositionIsBounded = true;

        for (int capturedIndex = 0; capturedIndex < boundaryValues.Length; capturedIndex++)
        {
            for (int capIndex = 0; capIndex < boundaryValues.Length; capIndex++)
            {
                uint expectedLength = boundaryValues[capturedIndex < capIndex ? capturedIndex : capIndex];
                uint selectedLength = UiShaderSourceReadProjectionRules.SelectRequestedLength(
                    boundaryValues[capturedIndex],
                    boundaryValues[capIndex]);

                requestedLengthSelectionIsUnsigned &= selectedLength == expectedLength;
                requestedLengthChunkCompositionIsBounded &=
                    UiShaderSourceReadProjectionRules.SelectChunk(selectedLength)
                    == (expectedLength < UiShaderSourceReadProjectionRules.MaximumChunkByteCount
                        ? expectedLength
                        : UiShaderSourceReadProjectionRules.MaximumChunkByteCount);
            }
        }

        check("Shader-source requested length uses an unsigned captured-size and caller-cap minimum",
            requestedLengthSelectionIsUnsigned
            && UiShaderSourceReadProjectionRules.SelectRequestedLength(0x80000000, 0x7FFFFFFF) == 0x7FFFFFFF
            && UiShaderSourceReadProjectionRules.SelectRequestedLength(0x7FFFFFFF, 0x80000000) == 0x7FFFFFFF);

        check("Shader-source requested length composes with the separate chunk cap",
            requestedLengthChunkCompositionIsBounded);

        bool readPlanMatchesCapturedGates = true;
        for (int capturedIndex = 0; capturedIndex < boundaryValues.Length; capturedIndex++)
        {
            for (int capIndex = 0; capIndex < boundaryValues.Length; capIndex++)
            {
                uint capturedSize = boundaryValues[capturedIndex];
                uint cap = boundaryValues[capIndex];
                uint expectedLength = capturedSize < cap ? capturedSize : cap;
                UiShaderSourceReadPlan plan =
                    UiShaderSourceReadProjectionRules.ProjectReadPlan(capturedSize, cap);

                readPlanMatchesCapturedGates &=
                    plan.RequestedLength == expectedLength
                    && plan.SetupCallReached == (capturedSize != 0)
                    && plan.ReadLoopReached == (expectedLength != 0)
                    && plan.ReturnAl == (expectedLength == 0 ? (byte?)1 : null);
            }
        }

        UiShaderSourceReadPlan zeroCapturedSizeWithZeroCap =
            UiShaderSourceReadProjectionRules.ProjectReadPlan(0, 0);
        UiShaderSourceReadPlan positiveCapturedSizeWithZeroCap =
            UiShaderSourceReadProjectionRules.ProjectReadPlan(1, 0);
        check("Shader-source plan separates initial-zero setup bypass from a zero-length post-clamp read bypass",
            readPlanMatchesCapturedGates
            && zeroCapturedSizeWithZeroCap is
                { RequestedLength: 0, SetupCallReached: false, ReadLoopReached: false, ReturnAl: 1 }
            && positiveCapturedSizeWithZeroCap is
                { RequestedLength: 0, SetupCallReached: true, ReadLoopReached: false, ReturnAl: 1 });

        check("fread success compares its element count to one, not its byte count",
            UiShaderSourceReadProjectionRules.FreadElementCount == 1
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(0) == false
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(1)
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(2) == false
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(uint.MaxValue) == false);

        const uint initialCumulativeOffset = 37;
        bool rejectedEmptyIteration = false;
        try
        {
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(0, initialCumulativeOffset, 1);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            rejectedEmptyIteration = exception.ParamName == "remainingByteCount";
        }
        check("Chunk continuation rejects a capture that never enters the read loop", rejectedEmptyIteration);

        UiShaderSourceChunkReadPlan failedChunk =
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(
                UiShaderSourceReadProjectionRules.MaximumChunkByteCount + 9,
                initialCumulativeOffset,
                0);
        UiShaderSourceChunkReadPlan failedOversizedItemCount =
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(9, initialCumulativeOffset, 2);
        check("A non-one fread result takes failure cleanup without advancing either count",
            failedChunk is
                {
                    ChunkByteCount: UiShaderSourceReadProjectionRules.MaximumChunkByteCount,
                    RemainingByteCount: UiShaderSourceReadProjectionRules.MaximumChunkByteCount + 9,
                    CumulativeByteOffset: initialCumulativeOffset,
                    FreadSucceeded: false,
                    FailureCleanupCallReached: true,
                    ContinueReadLoop: false,
                    ReturnAl: 0
                }
            && failedOversizedItemCount is
                {
                    RemainingByteCount: 9,
                    CumulativeByteOffset: initialCumulativeOffset,
                    FreadSucceeded: false,
                    FailureCleanupCallReached: true,
                    ContinueReadLoop: false,
                    ReturnAl: 0
                });

        UiShaderSourceChunkReadPlan successfulIntermediateChunk =
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(
                UiShaderSourceReadProjectionRules.MaximumChunkByteCount + 4,
                initialCumulativeOffset,
                1);
        UiShaderSourceChunkReadPlan successfulFinalChunk =
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(4, initialCumulativeOffset, 1);
        UiShaderSourceChunkReadPlan successfulWrappingOffset =
            UiShaderSourceReadProjectionRules.ProjectChunkReadPlan(5, uint.MaxValue - 2, 1);
        check("A complete fread advances the offset, subtracts its chunk, and returns only at the end",
            successfulIntermediateChunk is
                {
                    ChunkByteCount: UiShaderSourceReadProjectionRules.MaximumChunkByteCount,
                    RemainingByteCount: 4,
                    CumulativeByteOffset: initialCumulativeOffset + UiShaderSourceReadProjectionRules.MaximumChunkByteCount,
                    FreadSucceeded: true,
                    FailureCleanupCallReached: false,
                    ContinueReadLoop: true,
                    ReturnAl: null
                }
            && successfulFinalChunk is
                {
                    ChunkByteCount: 4,
                    RemainingByteCount: 0,
                    CumulativeByteOffset: initialCumulativeOffset + 4,
                    FreadSucceeded: true,
                    FailureCleanupCallReached: false,
                    ContinueReadLoop: false,
                    ReturnAl: 1
                }
            && successfulWrappingOffset.CumulativeByteOffset == 2);
    }
}
