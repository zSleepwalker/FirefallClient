namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Projects the captured shader-source setup/read gates and bounded byte count, and tests the fread item-count result.
/// The receiver-buffer call is only a captured-path observation; the null-receiver guard, receiver state, and offset
/// advancement are not modeled. Zero-size paths can return normally with AL=1 when cleanup returns; that flow is not
/// emulated here.
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
    public const uint FreadFailureCleanupVirtualCallRawFileOffset = 0x12BB445;
    public const uint FreadFailureCleanupVirtualCallVtableSlotOffset = 0x14;

    public const uint MaximumChunkByteCount = 0x01000000;
    public const uint FreadElementCount = 1;

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
            ReadLoopReached: requestedLength != 0);
    }

    public static bool IsCompleteChunkRead(uint capturedFreadElementCount) =>
        capturedFreadElementCount == FreadElementCount;
}

public readonly record struct UiShaderSourceReadPlan(
    uint RequestedLength,
    bool SetupCallReached,
    bool ReadLoopReached);
