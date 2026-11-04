namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selects the bounded byte count for one captured shader-source read and tests its fread item-count result.
/// This is a pure arithmetic projection; it does not model receiver state, offset advancement, or cleanup.
/// </summary>
public static class UiShaderSourceReadProjectionRules
{
    public const uint FreadImportAddressVirtualAddress = 0x1AC490C;
    public const uint FreadImportAddressRawFileOffset = 0x16C350C;
    public const string FreadImportModule = "MSVCR120.dll";
    public const string FreadImportName = "fread";
    public const uint CapturedSizeLoadRawFileOffset = 0x12BB366;
    public const uint ReceiverBufferVirtualCallRawFileOffset = 0x12BB403;
    public const uint ReceiverBufferVirtualCallVtableSlotOffset = 0x04;
    public const uint RequestedLengthCompareRawFileOffset = 0x12BB3D0;
    public const uint RequestedLengthClampRawFileOffset = 0x12BB3D3;
    public const uint RequestedLengthSlotSetupRawFileOffset = 0x12BB3DA;
    public const uint RequestedLengthReceiverOffset = 0x1C;
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

    public static bool IsRequestedLengthSetupNeeded(uint requestedLength) =>
        requestedLength != 0;

    public static bool IsCompleteChunkRead(uint capturedFreadElementCount) =>
        capturedFreadElementCount == FreadElementCount;
}
