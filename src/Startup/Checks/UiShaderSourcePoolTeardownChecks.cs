using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolTeardownChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source pool teardown anchors match the pinned sample and raw/VA mapping",
            UiShaderSourcePoolTeardownNativeLayout.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolTeardownNativeLayout.EntryRawFileOffset == 0x12CC1D0
            && UiShaderSourcePoolTeardownNativeLayout.EntryReturnRawFileOffset == 0x12CC27E
            && UiShaderSourcePoolTeardownNativeLayout.EntryEndRawFileOffsetExclusive == 0x12CC27F
            && UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress == 0x16CCDD0
            && UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress
                - UiShaderSourcePoolTeardownNativeLayout.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolTeardownNativeLayout.EntryRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableVirtualAddress == 0x1C80CA0
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableRawFileOffset == 0x187F8A0
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotRawFileOffset == 0x187F8A8
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotBytes == "D0 CD 6C 01"
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotBytesTargetVirtualAddress
                == UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVirtualAddress == 0x1E00500
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectRawFileOffset == 0x19FE900
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVtableVirtualAddress
                == UiShaderSourcePoolTeardownNativeLayout.SingletonVtableVirtualAddress
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVtableBytes == "A0 0C C8 01");

        check("Fresh instruction anchors preserve capture, global publication, field-clear order, and release boundary",
            UiShaderSourcePoolTeardownNativeLayout.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ThisRelativeOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, int? ThisRelativeOffset)[]
                {
                    (0x12CC1F4, "80 7E 04 00", 4),
                    (0x12CC217, "39 35 A8 B0 E4 01", null),
                    (0x12CC236, "8B 7E 10", 0x10),
                    (0x12CC239, "8B 46 0C", 0x0C),
                    (0x12CC23C, "A3 A8 B0 E4 01", null),
                    (0x12CC241, "C7 46 0C 00 00 00 00", 0x0C),
                    (0x12CC248, "C7 46 10 00 00 00 00", 0x10),
                    (0x12CC24F, "85 FF", null),
                    (0x12CC251, "74 29", null)
                })
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadGlobalVirtualAddress
                == 0x1E4B0A8
            && UiShaderSourcePoolTeardownNativeLayout.ThisFlagReadOffset == 4
            && UiShaderSourcePoolTeardownNativeLayout.PoolPointerReleaseTestRawFileOffset == 0x12CC24F
            && UiShaderSourcePoolTeardownNativeLayout.PoolPointerReleaseBranchRawFileOffset == 0x12CC251);

        check("Later drain helpers are retained as byte-backed anchors only",
            UiShaderSourcePoolTeardownNativeLayout.DrainHelperAnchors
                .Select(anchor =>
                    (anchor.CallRawFileOffset, anchor.CallBytes, anchor.TargetRawFileOffset))
                .SequenceEqual(new (uint CallRawFileOffset, string CallBytes, uint TargetRawFileOffset)[]
                {
                    (0x12CC256, "E8 85 18 00 00", 0x12CDAE0),
                    (0x12CC262, "E8 F9 CA FF FF", 0x12C8D60)
                }));

        (uint Pool, uint Next)[] crossings =
        {
            (0, 0),
            (0, 0xA1B2C3D4),
            (0x10203040, 0),
            (0x10203040, 0xA1B2C3D4)
        };
        bool capturesRemainIndependent = true;
        bool orderedWritesMatch = true;
        bool poolBranchMatchesCapture = true;
        foreach ((uint pool, uint next) in crossings)
        {
            UiShaderSourcePoolTeardownPlan plan =
                UiShaderSourcePoolTeardownNativeLayout.Project(
                    new UiShaderSourcePoolTeardownCapture(
                        ThisPointer: 0x1000,
                        CapturedPoolPointer: pool,
                        CapturedNextPointer: next));

            capturesRemainIndependent &=
                plan.Capture.CapturedPoolPointer == pool
                && plan.Capture.CapturedNextPointer == next
                && plan.OrderedWrites[0].Value == next
                && plan.ReleaseBoundary.CapturedPoolPointer == pool;
            orderedWritesMatch &=
                plan.OrderedWrites.Length == 3
                && plan.OrderedWrites[0]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.GlobalSingletonListHead,
                        0x12CC23C,
                        0x1E4B0A8,
                        next)
                && plan.OrderedWrites[1]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.ThisField,
                        0x12CC241,
                        0x100C,
                        0,
                        0x0C)
                && plan.OrderedWrites[2]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.ThisField,
                        0x12CC248,
                        0x1010,
                        0,
                        0x10)
                && plan.OrderedWrites
                    .Where(write => write.Target == UiShaderSourcePoolTeardownWriteTarget.ThisField)
                    .Select(write => write.ThisRelativeOffset)
                    .SequenceEqual(new int?[] { 0x0C, 0x10 })
                && plan.OrderedWrites.All(write =>
                    write.ThisRelativeOffset != UiShaderSourcePoolTeardownNativeLayout.ThisFlagReadOffset);
            poolBranchMatchesCapture &=
                plan.ReleaseBoundary.TestRawFileOffset == 0x12CC24F
                && plan.ReleaseBoundary.BranchRawFileOffset == 0x12CC251
                && plan.ReleaseBoundary.SelectsUnmodeledPoolDrain == (pool != 0);
        }

        check("Null-pool/next-pointer crosses preserve independent captures and unconditional ordered detach writes",
            capturesRemainIndependent && orderedWritesMatch);
        check("The release boundary selects only from captured this + 0x10, not captured this + 0x0C",
            poolBranchMatchesCapture);

        UiShaderSourcePoolTeardownPlan wrappedAddresses =
            UiShaderSourcePoolTeardownNativeLayout.Project(
                new UiShaderSourcePoolTeardownCapture(
                    ThisPointer: 0xFFFFFFF8,
                    CapturedPoolPointer: 0x12345678,
                    CapturedNextPointer: 0x87654321));
        check("Captured this-field destination addresses wrap as 32-bit native addresses",
            wrappedAddresses.OrderedWrites[0].TargetVirtualAddress == 0x1E4B0A8
            && wrappedAddresses.OrderedWrites[1].TargetVirtualAddress == 0x00000004
            && wrappedAddresses.OrderedWrites[2].TargetVirtualAddress == 0x00000008
            && wrappedAddresses.OrderedWrites[1].Value == 0
            && wrappedAddresses.OrderedWrites[2].Value == 0);
    }
}
