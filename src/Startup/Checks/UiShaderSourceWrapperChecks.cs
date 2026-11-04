using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceWrapperChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI shader source wrapper addresses and distinct slot and backing-field offsets are recorded",
            UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress == 0x1C80CC4
            && UiShaderSourceWrapperNativeLayout.WrapperVtableRva == 0x1880CC4
            && UiShaderSourceWrapperNativeLayout.WrapperVtableRawFileOffset == 0x187F8C4
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlot4EntryVirtualAddress == 0x16CC560
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlot4EntryRawFileOffset == 0x12CB960
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryVirtualAddress == 0x16CC5C0
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryRawFileOffset == 0x12CB9C0
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlot4Offset == 0x04
            && UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCOffset == 0x0C
            && UiShaderSourceWrapperNativeLayout.BackingPointerFieldOffset == 0x04
            && UiShaderSourceWrapperNativeLayout.BackingDataPointerFieldOffset == 0x04
            && UiShaderSourceWrapperNativeLayout.BackingSizeFieldOffset == 0x0C);

        check("Captured size projection returns zero for a null backing pointer",
            UiShaderSourceWrapperSizeProjectionRules.ReadCapturedSize(0, null) == 0);

        check("Captured size projection preserves the complete captured size DWORD",
            UiShaderSourceWrapperSizeProjectionRules.ReadCapturedSize(
                0x80000001,
                0xFEDCBA98) == 0xFEDCBA98);

        bool missingSizeRejected = false;
        try
        {
            UiShaderSourceWrapperSizeProjectionRules.ReadCapturedSize(0x12345678, null);
        }
        catch (ArgumentException exception)
        {
            missingSizeRejected = exception.ParamName == "capturedBackingFieldC";
        }

        bool unexpectedSizeRejected = false;
        try
        {
            UiShaderSourceWrapperSizeProjectionRules.ReadCapturedSize(0, 0x80000000);
        }
        catch (ArgumentException exception)
        {
            unexpectedSizeRejected = exception.ParamName == "capturedBackingFieldC";
        }

        check("Captured size projection rejects either inconsistent pointer/field capture",
            missingSizeRejected && unexpectedSizeRejected);
    }
}
