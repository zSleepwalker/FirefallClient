using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceClearChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI shader-source clear evidence keeps wrapper offsets distinct from raw instruction addresses",
            UiShaderSourceClearNativeLayout.WrapperBackingPointerOffset == 0x04
            && UiShaderSourceClearNativeLayout.WrapperControlPointerOffset == 0x08
            && UiShaderSourceClearNativeLayout.BackingHeaderFieldCOffset == 0x0C
            && UiShaderSourceClearNativeLayout.SlowZeroCaptureInstructionRawFileOffset == 0x12CBD4F
            && UiShaderSourceClearNativeLayout.SlowZeroClearControlInstructionRawFileOffset == 0x12CBD52
            && UiShaderSourceClearNativeLayout.SlowZeroClearBackingInstructionRawFileOffset == 0x12CBD55
            && UiShaderSourceClearNativeLayout.SlowZeroReleaseEntryRawFileOffset == 0x12CBE24);

        check("Slow-zero evidence orders old-control capture, wrapper clears, and release boundary",
            UiShaderSourceClearNativeLayout.SlowZeroCapturedControlRegister == "EBX"
            && UiShaderSourceClearNativeLayout.SlowZeroPathInstructions.SequenceEqual(new[]
            {
                new UiShaderSourceClearInstructionEvidence(
                    "Capture old wrapper control pointer in EBX", 0x12CBD4F, WrapperRelativeOffset: 0x08),
                new UiShaderSourceClearInstructionEvidence(
                    "Clear wrapper control pointer", 0x12CBD52, WrapperRelativeOffset: 0x08),
                new UiShaderSourceClearInstructionEvidence(
                    "Clear wrapper backing pointer", 0x12CBD55, WrapperRelativeOffset: 0x04),
                new UiShaderSourceClearInstructionEvidence(
                    "Enter captured old-control release path", 0x12CBE24)
            }));

        check("Fast zero evidence is confined to backing header field C, not a wrapper detach",
            UiShaderSourceClearNativeLayout.ZeroFastPathInstructions.SequenceEqual(new[]
            {
                new UiShaderSourceClearInstructionEvidence(
                    "Write zero to backing header field C", 0x12CBD33, BackingRecordRelativeOffset: 0x0C)
            })
            && UiShaderSourceClearNativeLayout.ZeroFastPathInstructions[0].WrapperRelativeOffset is null);

        check("Slot 14 entry and helper target retain their raw-file address roles",
            UiShaderSourceClearNativeLayout.Slot14EntryRawFileOffset == 0x12CBA00
            && UiShaderSourceClearNativeLayout.Slot14HelperTargetRawFileOffset == 0x12CBCB0);
    }
}
