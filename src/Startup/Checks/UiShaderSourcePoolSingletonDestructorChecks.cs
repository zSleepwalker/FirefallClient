using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolSingletonDestructorChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Singleton deleting slot maps from its vtable to raw 0x12CC280, distinct from detach slot +8",
            UiShaderSourcePoolSingletonDestructor.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolSingletonDestructor.SingletonVtableVirtualAddress == 0x1C80CA0
            && UiShaderSourcePoolSingletonDestructor.SingletonVtableRawFileOffset == 0x187F8A0
            && UiShaderSourcePoolSingletonDestructor.SingletonVtableBytes
                == "80 CE 6C 01 10 CD 6C 01 D0 CD 6C 01"
            && UiShaderSourcePoolSingletonDestructor.SingletonDeletingSlotVirtualAddress == 0x16CCE80
            && UiShaderSourcePoolSingletonDestructor.SingletonDeletingSlotRawFileOffset == 0x12CC280
            && UiShaderSourcePoolSingletonDestructor.SingletonDeletingSlotVirtualAddress
                - UiShaderSourcePoolSingletonDestructor.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolSingletonDestructor.SingletonDeletingSlotRawFileOffset
            && UiShaderSourcePoolSingletonDestructor.SingletonDetachSlotVirtualAddress == 0x16CCDD0
            && UiShaderSourcePoolSingletonDestructor.SingletonDetachSlotRawFileOffset == 0x12CC1D0
            && UiShaderSourcePoolSingletonDestructor.EntryRawFileOffset
                + UiShaderSourcePoolSingletonDestructor.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolSingletonDestructor.EntryVirtualAddress
            && UiShaderSourcePoolSingletonDestructor.EntryVirtualAddress == 0x16CCE80);

        check("Destructor body anchors preserve exact instruction offsets and branch/call targets",
            UiShaderSourcePoolSingletonDestructor.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ThisRelativeOffset,
                        instruction.TargetRawFileOffset, instruction.TargetVirtualAddress))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, int? ThisRelativeOffset,
                        uint? TargetRawFileOffset, uint? TargetVirtualAddress)[]
                {
                    (0x12CC280, "55", null, null, null),
                    (0x12CC281, "8B EC", null, null, null),
                    (0x12CC283, "F6 45 08 01", null, null, null),
                    (0x12CC287, "56", null, null, null),
                    (0x12CC288, "8B F1", null, null, null),
                    (0x12CC28A, "C7 06 BC E9 C5 01", 0, null, 0x1C5E9BC),
                    (0x12CC290, "C7 46 08 00 00 00 00", 8, null, null),
                    (0x12CC297, "C7 46 0C 00 00 00 00", 0x0C, null, null),
                    (0x12CC29E, "74 05", null, null, null),
                    (0x12CC2A0, "E8 BB CA FF FF", null, 0x12C8D60, null),
                    (0x12CC2A5, "8B C6", null, null, null),
                    (0x12CC2A7, "5E", null, null, null),
                    (0x12CC2A8, "5D", null, null, null),
                    (0x12CC2A9, "C2 04 00", null, null, null)
                })
            && UiShaderSourcePoolSingletonDestructor.EntryEndRawFileOffsetExclusive == 0x12CC2AC
            && UiShaderSourcePoolSingletonDestructor.AdjacentPoolDrainHelperRawFileOffset == 0x12CC2B0
            && UiShaderSourcePoolSingletonDestructor.EntryEndRawFileOffsetExclusive
                < UiShaderSourcePoolSingletonDestructor.AdjacentPoolDrainHelperRawFileOffset
            && UiShaderSourcePoolSingletonDestructor.ConditionalHelperCallRawFileOffset + 5
                - 0x3545 == UiShaderSourcePoolSingletonDestructor.ConditionalAllocatorBackendHelperRawFileOffset
            && UiShaderSourcePoolSingletonDestructor.DeletingFlagBranchRawFileOffset + 2 + 5
                == UiShaderSourcePoolSingletonDestructor.ReturnReceiverLoadRawFileOffset
            && UiShaderSourcePoolSingletonDestructor.DeletingFlagsStackArgumentOffsetFromEbp == 8
            && UiShaderSourcePoolSingletonDestructor.DeletingFlagsMask == 1
            && UiShaderSourcePoolSingletonDestructor.ReturnInstructionRawFileOffset == 0x12CC2A9
            && UiShaderSourcePoolSingletonDestructor.ReturnPopByteCount == 4);

        check("Bounded body has no direct pool/listhead access, touches receiver offsets 0/8/C, and returns this",
            UiShaderSourcePoolSingletonDestructor.Instructions
                .Where(instruction => instruction.ThisRelativeOffset.HasValue)
                .Select(instruction => instruction.ThisRelativeOffset!.Value)
                .SequenceEqual(new[] { 0, 8, 0x0C })
            && UiShaderSourcePoolSingletonDestructor.Instructions
                .Where(instruction => instruction.TargetVirtualAddress.HasValue)
                .Select(instruction => instruction.TargetVirtualAddress!.Value)
                .SequenceEqual(new[] { 0x1C5E9BCu })
            && UiShaderSourcePoolSingletonDestructor.Instructions
                .Where(instruction => instruction.TargetRawFileOffset.HasValue)
                .Select(instruction => instruction.TargetRawFileOffset!.Value)
                .SequenceEqual(new[] { 0x12C8D60u })
            && UiShaderSourcePoolSingletonDestructor.ReturnReceiverLoadRawFileOffset == 0x12CC2A5
            && UiShaderSourcePoolSingletonDestructor.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC2A5).Bytes == "8B C6");

        bool everyDeletingByteMatchesBitZero = true;
        for (int flags = byte.MinValue; flags <= byte.MaxValue; flags++)
        {
            uint receiver = 0xA1B2C3D4;
            UiShaderSourcePoolSingletonDestructorProjection projection =
                UiShaderSourcePoolSingletonDestructor.Project(receiver, (byte)flags);
            bool expectedHelperCall = (flags & 1) != 0;
            everyDeletingByteMatchesBitZero &=
                projection.ThisPointer == receiver
                && projection.DeletingFlags == flags
                && projection.OrderedUnconditionalWrites.SequenceEqual(new[]
                {
                    new UiShaderSourcePoolSingletonDestructorWrite(0, 0x12CC28A, 0x1C5E9BC),
                    new UiShaderSourcePoolSingletonDestructorWrite(8, 0x12CC290, 0),
                    new UiShaderSourcePoolSingletonDestructorWrite(0x0C, 0x12CC297, 0)
                })
                && projection.CallsAllocatorBackendHelper == expectedHelperCall
                && projection.AllocatorBackendReceiverInEcx
                    == (expectedHelperCall ? receiver : null)
                && projection.ReturnValue == receiver
                && projection.ReturnPopByteCount == 4;
        }

        check("All deleting-flag byte values gate only the helper on bit zero and preserve receiver return",
            everyDeletingByteMatchesBitZero);
    }
}
