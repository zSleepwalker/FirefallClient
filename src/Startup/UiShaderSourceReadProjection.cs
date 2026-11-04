namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Projects the captured shader-source setup/read gates, bounded chunk progression, and return byte.
/// The return byte assumes cleanup calls and the stack-cookie check return normally. Receiver-buffer state,
/// the receiver null guard, and the effects of the receiver's virtual calls are not modeled.
/// </summary>
public static class UiShaderSourceReadProjectionRules
{
    public const uint FreadImportAddressVirtualAddress = 0x1AC490C;
    public const uint FreadImportAddressRawFileOffset = 0x16C350C;
    public const string FreadImportModule = "MSVCR120.dll";
    public const string FreadImportName = "fread";
    public const uint CapturedSizeLoadRawFileOffset = 0x12BB366;
    public const uint InitialSizeZeroCheckRawFileOffset = 0x12BB3C8;
    public const uint InitialSizeZeroBranchTargetRawFileOffset = 0x12BB42E;
    public const uint ReceiverBufferVirtualCallRawFileOffset = 0x12BB403;
    public const uint ReceiverBufferVirtualCallVtableSlotOffset = 0x04;
    public const uint RequestedLengthCompareRawFileOffset = 0x12BB3D0;
    public const uint RequestedLengthClampRawFileOffset = 0x12BB3D3;
    public const uint RequestedLengthSlotSetupRawFileOffset = 0x12BB3DA;
    public const uint RequestedLengthReceiverOffset = 0x1C;
    public const uint RequestedLengthZeroTestRawFileOffset = 0x12BB3E3;
    public const uint RequestedLengthZeroBranchRawFileOffset = 0x12BB3E5;
    public const uint FreadCallRawFileOffset = 0x12BB40E;
    public const uint FreadResultSuccessTestRawFileOffset = 0x12BB417;
    public const uint FreadResultFailureBranchRawFileOffset = 0x12BB41A;
    public const uint CumulativeByteOffsetAddRawFileOffset = 0x12BB420;
    public const uint RemainingByteCountSubtractRawFileOffset = 0x12BB426;
    public const uint ReadLoopContinuationBranchRawFileOffset = 0x12BB428;
    public const uint FreadFailureCleanupVirtualCallRawFileOffset = 0x12BB445;
    public const uint FreadFailureCleanupVirtualCallVtableSlotOffset = 0x14;
    public const uint FreadFailureCleanupContinuationRawFileOffset = 0x12BB448;
    public const bool FreadFailureCleanupCallReturnValueIsIgnored = true;
    public const uint CommonCleanupRawFileOffset = 0x12BB372;
    public const uint FailedReadReturnFlagClearRawFileOffset = 0x12BB37B;
    public const uint CompletedReadReturnFlagSetRawFileOffset = 0x12BB438;
    public const uint ReturnAlTransferRawFileOffset = 0x12BB45D;
    public const uint ReturnCookieCheckCallRawFileOffset = 0x12BB474;
    public const uint ReturnCookieCheckFunctionRawFileOffset = 0x136AC96;
    public const uint ReturnCookieCheckMatchedReturnRawFileOffset = 0x136AC9E;
    public const uint FunctionReturnRawFileOffset = 0x12BB47C;

    public const uint MaximumChunkByteCount = 0x01000000;
    public const uint FreadElementCount = 1;
    public const byte FailureReturnAl = 0;
    public const byte SuccessReturnAl = 1;

    public static uint SelectChunk(uint remainingByteCount) =>
        remainingByteCount < MaximumChunkByteCount
            ? remainingByteCount
            : MaximumChunkByteCount;

    public static uint SelectRequestedLength(uint capturedSize, uint callerCap) =>
        capturedSize < callerCap
            ? capturedSize
            : callerCap;

    public static UiShaderSourceReadPlan ProjectReadPlan(uint capturedSize, uint callerCap)
    {
        uint requestedLength = SelectRequestedLength(capturedSize, callerCap);
        return new UiShaderSourceReadPlan(
            requestedLength,
            SetupCallReached: capturedSize != 0,
            ReadLoopReached: requestedLength != 0,
            ReturnAl: requestedLength == 0 ? SuccessReturnAl : null);
    }

    public static bool IsCompleteChunkRead(uint capturedFreadElementCount) =>
        capturedFreadElementCount == FreadElementCount;

    public static UiShaderSourceChunkReadPlan ProjectChunkReadPlan(
        uint remainingByteCount,
        uint cumulativeByteOffset,
        uint capturedFreadElementCount)
    {
        if (remainingByteCount == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(remainingByteCount),
                "A chunk-read plan represents an entered read-loop iteration.");
        }

        uint chunkByteCount = SelectChunk(remainingByteCount);
        if (!IsCompleteChunkRead(capturedFreadElementCount))
        {
            return new UiShaderSourceChunkReadPlan(
                chunkByteCount,
                remainingByteCount,
                cumulativeByteOffset,
                FreadSucceeded: false,
                FailureCleanupCallReached: true,
                ContinueReadLoop: false,
                ReturnAl: FailureReturnAl);
        }

        uint remainingAfterChunk = remainingByteCount - chunkByteCount;
        uint cumulativeOffsetAfterChunk = unchecked(cumulativeByteOffset + chunkByteCount);
        bool continueReadLoop = remainingAfterChunk != 0;
        return new UiShaderSourceChunkReadPlan(
            chunkByteCount,
            remainingAfterChunk,
            cumulativeOffsetAfterChunk,
            FreadSucceeded: true,
            FailureCleanupCallReached: false,
            ContinueReadLoop: continueReadLoop,
            ReturnAl: continueReadLoop ? null : SuccessReturnAl);
    }
}

public readonly record struct UiShaderSourceReadPlan(
    uint RequestedLength,
    bool SetupCallReached,
    bool ReadLoopReached,
    byte? ReturnAl);

public readonly record struct UiShaderSourceChunkReadPlan(
    uint ChunkByteCount,
    uint RemainingByteCount,
    uint CumulativeByteOffset,
    bool FreadSucceeded,
    bool FailureCleanupCallReached,
    bool ContinueReadLoop,
    byte? ReturnAl);
