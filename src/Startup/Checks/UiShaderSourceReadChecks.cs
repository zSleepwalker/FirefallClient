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
            && UiShaderSourceReadProjectionRules.ReceiverBufferVirtualCallRawFileOffset == 0x12BB403
            && UiShaderSourceReadProjectionRules.ReceiverBufferVirtualCallVtableSlotOffset == 0x04
            && UiShaderSourceReadProjectionRules.RequestedLengthSlotSetupRawFileOffset == 0x12BB3DA
            && UiShaderSourceReadProjectionRules.RequestedLengthReceiverOffset == 0x1C
            && UiShaderSourceReadProjectionRules.FreadCallRawFileOffset == 0x12BB40E
            && UiShaderSourceReadProjectionRules.FreadResultSuccessTestRawFileOffset == 0x12BB417
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupVirtualCallRawFileOffset == 0x12BB445
            && UiShaderSourceReadProjectionRules.FreadFailureCleanupVirtualCallVtableSlotOffset == 0x14);

        check("Shader-source read selects the remaining byte count up to the 16 MiB chunk cap",
            UiShaderSourceReadProjectionRules.MaximumChunkByteCount == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(0) == 0
            && UiShaderSourceReadProjectionRules.SelectChunk(1) == 1
            && UiShaderSourceReadProjectionRules.SelectChunk(0x01000000) == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(0x01000001) == 0x01000000
            && UiShaderSourceReadProjectionRules.SelectChunk(uint.MaxValue) == 0x01000000);

        check("fread success compares its element count to one, not its byte count",
            UiShaderSourceReadProjectionRules.FreadElementCount == 1
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(0) == false
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(1)
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(2) == false
            && UiShaderSourceReadProjectionRules.IsCompleteChunkRead(uint.MaxValue) == false);
    }
}
