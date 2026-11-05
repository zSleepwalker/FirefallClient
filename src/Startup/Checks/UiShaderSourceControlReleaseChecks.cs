using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceControlReleaseChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pooled control release evidence is pinned to the requested image and vtable raw anchor",
            UiShaderSourceControlReleaseNativeLayout.PinnedBinarySha256
                == "97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445"
            && UiShaderSourceControlReleaseNativeLayout.ControlVtableVirtualAddress == 0x1C80C8C
            && UiShaderSourceControlReleaseNativeLayout.ControlVtableRawFileOffset == 0x187F88C
            && UiShaderSourceControlReleaseNativeLayout.ControlVtableSlotPointerBytes
                == "30 D5 6C 01 40 D5 6C 01 B0 D5 6C 01"
            && UiShaderSourceControlReleaseNativeLayout.HeaderReleaseSlotVirtualAddress == 0x16CD530
            && UiShaderSourceControlReleaseNativeLayout.HeaderReleaseSlotRawFileOffset == 0x12CC930
            && UiShaderSourceControlReleaseNativeLayout.Slot4DispatchVirtualAddress == 0x16CD540
            && UiShaderSourceControlReleaseNativeLayout.Slot4DispatchRawFileOffset == 0x12CC940
            && UiShaderSourceControlReleaseNativeLayout.DeletingDispatchVirtualAddress == 0x16CD5B0
            && UiShaderSourceControlReleaseNativeLayout.DeletingDispatchRawFileOffset == 0x12CC9B0);

        check("Slot 0 checks control header +C and reaches the recorded tail only for a nonnull capture",
            UiShaderSourceControlReleaseNativeLayout.ControlHeaderPointerOffset == 0x0C
            && UiShaderSourceControlReleaseNativeLayout.HeaderReleaseSlotBytes
                == "8B 49 0C 85 C9 0F 85 D5 00 00 00 C3"
            && UiShaderSourceControlReleaseNativeLayout.HeaderReleaseTailRawFileOffset == 0x12CCA10
            && UiShaderSourceControlReleaseNativeLayout.HeaderReleaseTailEntryBytes
                == "51 56 57 8B F9 83 7F 08 00 74 17"
            && 0x12CC930 + 3 + 2 + 6 + 0xD5 == 0x12CCA10
            && UiShaderSourceControlReleaseNativeLayout.ProjectHeaderRelease(false)
                == new UiShaderSourceControlHeaderReleaseProjection(false, false)
            && UiShaderSourceControlReleaseNativeLayout.ProjectHeaderRelease(true)
                == new UiShaderSourceControlHeaderReleaseProjection(true, true));

        check("Slot +4 skips null receivers and calls vtable +8 with captured argument one",
            UiShaderSourceControlReleaseNativeLayout.Slot4DispatchBytes
                == "85 C9 74 07 8B 01 6A 01 FF 50 08 C3"
            && UiShaderSourceControlReleaseNativeLayout.Slot4TargetVtableByteOffset == 0x08
            && UiShaderSourceControlReleaseNativeLayout.Slot4DeletingFlagsArgument == 1
            && UiShaderSourceControlReleaseNativeLayout.ProjectSlot4Dispatch(false)
                == new UiShaderSourceControlSlot4Projection(false, false, null)
            && UiShaderSourceControlReleaseNativeLayout.ProjectSlot4Dispatch(true)
                == new UiShaderSourceControlSlot4Projection(true, true, 1));

        check("Slot +8 replaces the vptr unconditionally and gates only the helper call on deleting bit zero",
            UiShaderSourceControlReleaseNativeLayout.DeletingDispatchBytes
                == "55 8B EC F6 45 08 01 56 8B F1 C7 06 7C 07 C6 01 74 05 E8 99 C3 FF FF 8B C6 5E 5D C2 04 00"
            && UiShaderSourceControlReleaseNativeLayout.DeletingFlagsStackArgumentOffset == 0x08
            && UiShaderSourceControlReleaseNativeLayout.DeletingFlagsMask == 1
            && UiShaderSourceControlReleaseNativeLayout.ReplacementControlVtableVirtualAddress == 0x1C6077C
            && UiShaderSourceControlReleaseNativeLayout.ConditionalStorageReleaseCallRawFileOffset == 0x12CC9C2
            && UiShaderSourceControlReleaseNativeLayout.ControlStorageReleaseHelperRawFileOffset == 0x12C8D60
            && 0x12CC9C7 - 0x3C67 == 0x12C8D60);

        uint[] deletingFlagsCaptures = { 0, 1, 2, uint.MaxValue };
        bool deletingFlagsMatchBitZero = true;
        foreach (uint flags in deletingFlagsCaptures)
        {
            UiShaderSourceControlDeletingDispatchProjection projection =
                UiShaderSourceControlReleaseNativeLayout.ProjectDeletingDispatch(flags);
            deletingFlagsMatchBitZero &=
                projection.DeletingFlags == flags
                && projection.ReplacesControlVptr
                && projection.CallsStorageReleaseHelper == ((flags & 1) != 0);
        }

        check("Deleting-flags captures zero, one, two, and maximum follow the observed bit-zero test",
            deletingFlagsMatchBitZero);

        check("Direct-entry evidence keeps header release distinct from deleting dispatch and has no field-clear projection",
            UiShaderSourceControlReleaseNativeLayout.DirectEntryInstructions.Count == 4
            && UiShaderSourceControlReleaseNativeLayout.DirectEntryInstructions[0].RawFileOffset == 0x12CC930
            && UiShaderSourceControlReleaseNativeLayout.DirectEntryInstructions[1].RawFileOffset == 0x12CCA10
            && UiShaderSourceControlReleaseNativeLayout.DirectEntryInstructions[2].RawFileOffset == 0x12CC940
            && UiShaderSourceControlReleaseNativeLayout.DirectEntryInstructions[3].RawFileOffset == 0x12CC9B0);
    }
}
