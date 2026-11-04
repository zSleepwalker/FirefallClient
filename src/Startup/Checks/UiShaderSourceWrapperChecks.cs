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

        check("Preserve helper, allocation call, and target use distinct raw file offsets",
            UiShaderSourceWrapperNativeLayout.PreserveRawHelperEntryRawFileOffset == 0x12CBCB0
            && UiShaderSourceWrapperNativeLayout.PreserveSlowPathAllocationCallRawFileOffset == 0x12CBDA6
            && UiShaderSourceWrapperNativeLayout.PreserveSlowPathAllocationTargetRawFileOffset == 0x12CBF30);

        check("Preserve copy-path header reads precede the copy call",
            UiShaderSourceWrapperNativeLayout.PreserveCopyPathInstructions.SequenceEqual(new[]
            {
                new UiShaderSourceWrapperInstructionEvidence("Read old header +4", 0x12CBDC8),
                new UiShaderSourceWrapperInstructionEvidence("Read new header +4", 0x12CBDD0),
                new UiShaderSourceWrapperInstructionEvidence("Copy call", 0x12CBDD3)
            }));

        check("Preserve no-copy path records the new-header load before the unguarded compare",
            UiShaderSourceWrapperNativeLayout.PreserveNoCopyPathInstructions.SequenceEqual(new[]
            {
                new UiShaderSourceWrapperInstructionEvidence("Load new header into EBX", 0x12CBDDB),
                new UiShaderSourceWrapperInstructionEvidence("Compare ESI with [EBX]", 0x12CBDDF)
            }));

        check("Preserve later stores remain distinct from the copy-path read and follow the path accesses",
            UiShaderSourceWrapperNativeLayout.PreservePostAccessWrites.SequenceEqual(new[]
            {
                new UiShaderSourceWrapperInstructionEvidence("Write logical size", 0x12CBDFD),
                new UiShaderSourceWrapperInstructionEvidence("Write wrapper +8", 0x12CBE07),
                new UiShaderSourceWrapperInstructionEvidence("Write wrapper +4", 0x12CBE15)
            })
            && UiShaderSourceWrapperNativeLayout.PreserveCopyPathInstructions[0].RawFileOffset
                != UiShaderSourceWrapperNativeLayout.PreservePostAccessWrites[0].RawFileOffset
            && UiShaderSourceWrapperNativeLayout.PreserveCopyPathInstructions[^1].RawFileOffset
                < UiShaderSourceWrapperNativeLayout.PreservePostAccessWrites[0].RawFileOffset
            && UiShaderSourceWrapperNativeLayout.PreserveNoCopyPathInstructions[^1].RawFileOffset
                < UiShaderSourceWrapperNativeLayout.PreservePostAccessWrites[0].RawFileOffset);

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
