using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetStorageChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Storage helper sample hash, raw interval, code VA, and complete byte capture are pinned",
            UiShaderSourcePoolResetStorageProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionRawFileOffset == 0x006A5900
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionEndRawFileOffsetExclusive
                == 0x006A5979
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionSize == 0x79
            && UiShaderSourcePoolResetStorageProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionRawFileOffset
                + UiShaderSourcePoolResetStorageProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetStorageProjectionRules.FunctionCodeVirtualAddress
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionCodeVirtualAddress == 0x00AA6500
            && UiShaderSourcePoolResetStorageProjectionRules.CapturedFunctionBytes
                == "55 8B EC 53 8B 5D 08 56 33 F6 57 8B F9 85 DB 74 24 "
                    + "81 FB FF FF FF 3F 77 16 8D 04 9D 00 00 00 00 50 E8 "
                    + "8A 26 C1 00 8B F0 83 C4 04 85 F6 75 06 FF 15 24 44 "
                    + "AC 01 8B 0F 8B 47 04 2B C1 83 E0 FC 50 51 56 FF 15 "
                    + "9C 48 AC 01 8B 0F 83 C4 0C 8B 47 04 2B C1 C1 F8 02 "
                    + "89 45 08 85 C9 74 05 E8 FF 33 C2 00 8D 04 9E 89 37 "
                    + "89 47 08 8B 45 08 8D 04 86 89 47 04 5F 5E 5B 5D C2 04 00"
            && UiShaderSourcePoolResetStorageProjectionRules.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x79
            && UiShaderSourcePoolResetStorageProjectionRules.FunctionRawFileOffset
                + (uint)UiShaderSourcePoolResetStorageProjectionRules.CapturedFunctionBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolResetStorageProjectionRules.FunctionEndRawFileOffsetExclusive);

        check("Count limit, allocator, reporter-cell, copy-cell, release helper, and publication anchors match byte evidence",
            UiShaderSourcePoolResetStorageProjectionRules.MaximumRequestedDwordCount == 0x3FFFFFFF
            && UiShaderSourcePoolResetStorageProjectionRules.AllocationTargetRawFileOffset == 0x012B7FB0
            && UiShaderSourcePoolResetStorageProjectionRules.FailureReportCellVirtualAddress
                == 0x01AC4424
            && UiShaderSourcePoolResetStorageProjectionRules.CopyTargetCellVirtualAddress
                == 0x01AC489C
            && UiShaderSourcePoolResetStorageProjectionRules.ReleaseHelperTargetRawFileOffset
                == 0x012C8D60
            && UiShaderSourcePoolResetStorageProjectionRules.AllocationCallRawFileOffset
                + 5u + 0x00C1268Au
                == UiShaderSourcePoolResetStorageProjectionRules.AllocationTargetRawFileOffset
            && UiShaderSourcePoolResetStorageProjectionRules.FailureReportCallRawFileOffset
                == 0x006A592F
            && 0x006A5917u + 2u + 0x16u
                == UiShaderSourcePoolResetStorageProjectionRules.FailureReportCallRawFileOffset
            && UiShaderSourcePoolResetStorageProjectionRules.ReleaseHelperCallRawFileOffset
                + 5u + 0x00C233FFu
                == UiShaderSourcePoolResetStorageProjectionRules.ReleaseHelperTargetRawFileOffset
            && UiShaderSourcePoolResetStorageProjectionRules.InitialCopyBeginReadRawFileOffset
                == 0x006A5935
            && UiShaderSourcePoolResetStorageProjectionRules.InitialCopyEndReadRawFileOffset
                == 0x006A5937
            && UiShaderSourcePoolResetStorageProjectionRules.PostCopyBeginReadRawFileOffset
                == 0x006A5948
            && UiShaderSourcePoolResetStorageProjectionRules.PostCopyEndReadRawFileOffset
                == 0x006A594D
            && UiShaderSourcePoolResetStorageProjectionRules.RetainedCountArithmeticShiftRawFileOffset
                == 0x006A5952
            && UiShaderSourcePoolResetStorageProjectionRules.StorageEndValueComputeRawFileOffset
                == 0x006A5961
            && UiShaderSourcePoolResetStorageProjectionRules.EndValueComputeRawFileOffset == 0x006A596C
            && UiShaderSourcePoolResetStorageProjectionRules.BeginPublicationRawFileOffset
                == 0x006A5964
            && UiShaderSourcePoolResetStorageProjectionRules.StorageEndPublicationRawFileOffset
                == 0x006A5966
            && UiShaderSourcePoolResetStorageProjectionRules.EndPublicationRawFileOffset == 0x006A596F
            && UiShaderSourcePoolResetStorageProjectionRules.NormalReturnRawFileOffset == 0x006A5976
            && UiShaderSourcePoolResetStorageProjectionRules.NormalReturnStackPopBytes == 4
            && UiShaderSourcePoolResetStorageProjectionRules.Instructions.All(HasExactSlice));

        var maximumAccepted = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 0x3FFFFFFF,
                AllocatorReturnedBasePointer: 0x20000000,
                InitialDescriptorBeginPointer: 0x1000,
                InitialDescriptorEndPointer: 0x1013,
                CopyTargetReturnedNormally: true,
                PostCopyDescriptorBeginPointer: 0x3008,
                PostCopyDescriptorEndPointer: 0x3010,
                ReleaseHelperReturnedNormally: true));
        check("Maximum accepted count scales to 0xFFFFFFFC and publishes begin, storage end, then retained end",
            maximumAccepted.AllocationCallReached
            && maximumAccepted.RequestedAllocationBytes == 0xFFFFFFFC
            && !maximumAccepted.FailureReportCellCallReached
            && maximumAccepted.CopyCallReached
            && maximumAccepted.CopyTargetCellVirtualAddress == 0x01AC489C
            && maximumAccepted.CopyNewBaseArgument == 0x20000000
            && maximumAccepted.CopyOldBeginArgument == 0x1000
            && maximumAccepted.CopyLengthBytes == 0x10
            && maximumAccepted.InitialCopyDescriptorBeginRead == 0x1000
            && maximumAccepted.InitialCopyDescriptorEndRead == 0x1013
            && maximumAccepted.PostCopyDescriptorBeginRead == 0x3008
            && maximumAccepted.PostCopyDescriptorEndRead == 0x3010
            && maximumAccepted.RetainedDwordCount == 2
            && maximumAccepted.ReleaseHelperCallReached
            && maximumAccepted.ReleaseHelperPointerArgument == 0x3008
            && maximumAccepted.ReleaseHelperReturnedNormally == true
            && maximumAccepted.BeginPublicationReached
            && maximumAccepted.PublishedBeginPointer == 0x20000000
            && maximumAccepted.StorageEndPublicationReached
            && maximumAccepted.PublishedStorageEndPointer == 0x1FFFFFFC
            && maximumAccepted.EndPublicationReached
            && maximumAccepted.PublishedEndPointer == 0x20000008
            && maximumAccepted.ReturnedEax == 0x20000008
            && maximumAccepted.ReturnRawFileOffset == 0x006A5976
            && maximumAccepted.Boundary == UiShaderSourcePoolResetStorageBoundary.NormalReturn);

        var zeroCount = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 0,
                InitialDescriptorBeginPointer: 0x1000,
                InitialDescriptorEndPointer: 0x1013,
                CopyTargetReturnedNormally: true,
                PostCopyDescriptorBeginPointer: 0,
                PostCopyDescriptorEndPointer: 0x0C));
        check("Zero count skips allocation yet still reaches copy and null-begin publication using separate later captures",
            !zeroCount.AllocationCallReached
            && zeroCount.RequestedAllocationBytes == 0
            && !zeroCount.FailureReportCellCallReached
            && zeroCount.CopyNewBaseArgument == 0
            && zeroCount.CopyLengthBytes == 0x10
            && zeroCount.InitialCopyDescriptorBeginRead == 0x1000
            && zeroCount.InitialCopyDescriptorEndRead == 0x1013
            && zeroCount.PostCopyDescriptorReadsReached
            && zeroCount.PostCopyDescriptorBeginRead == 0
            && zeroCount.PostCopyDescriptorEndRead == 0x0C
            && zeroCount.RetainedDwordCount == 3
            && !zeroCount.ReleaseHelperCallReached
            && zeroCount.Boundary == UiShaderSourcePoolResetStorageBoundary.NormalReturn
            && zeroCount.PublishedBeginPointer == 0
            && zeroCount.PublishedStorageEndPointer == 0
            && zeroCount.PublishedEndPointer == 0x0C
            && zeroCount.ReturnedEax == 0x0C);

        var wrappedLengths = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 1,
                AllocatorReturnedBasePointer: 0x4000,
                InitialDescriptorBeginPointer: 0xFFFFFFF8,
                InitialDescriptorEndPointer: 0x00000005));
        check("The copy length uses wrapping subtraction followed by the four-byte mask",
            wrappedLengths.CopyLengthBytes == 0x0C
            && wrappedLengths.CopyNewBaseArgument == 0x4000
            && wrappedLengths.CopyOldBeginArgument == 0xFFFFFFF8
            && wrappedLengths.Boundary == UiShaderSourcePoolResetStorageBoundary.CopyCallBoundary
            && !wrappedLengths.PostCopyDescriptorReadsReached
            && wrappedLengths.PostCopyDescriptorBeginRead is null
            && wrappedLengths.PostCopyDescriptorEndRead is null);

        var oversized = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 0x40000000));
        check("An above-limit request stops at the failure report cell without allocation or later reads",
            !oversized.AllocationCallReached
            && oversized.RequestedAllocationBytes is null
            && oversized.FailureReportCellCallReached
            && !oversized.CopyCallReached
            && oversized.CopyTargetCellVirtualAddress is null
            && !oversized.InitialCopyDescriptorReadsReached
            && !oversized.PostCopyDescriptorReadsReached
            && !oversized.ReleaseHelperCallReached
            && !oversized.BeginPublicationReached
            && oversized.Boundary
                == UiShaderSourcePoolResetStorageBoundary.AllocationReportCellCallBoundary);

        var allocatorNull = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 7,
                AllocatorReturnedBasePointer: 0));
        check("A null allocator return reaches the report cell and stops before copy",
            allocatorNull.AllocationCallReached
            && allocatorNull.RequestedAllocationBytes == 28
            && allocatorNull.FailureReportCellCallReached
            && !allocatorNull.CopyCallReached
            && !allocatorNull.InitialCopyDescriptorReadsReached
            && allocatorNull.Boundary
                == UiShaderSourcePoolResetStorageBoundary.AllocationReportCellCallBoundary);

        var copyNoReturn = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 1,
                AllocatorReturnedBasePointer: 0x5000,
                InitialDescriptorBeginPointer: 0x100,
                InitialDescriptorEndPointer: 0x110,
                CopyTargetReturnedNormally: false));
        check("A captured nonreturning copy target stops before post-copy descriptor reads",
            copyNoReturn.CopyCallReached
            && copyNoReturn.CopyLengthBytes == 0x10
            && !copyNoReturn.PostCopyDescriptorReadsReached
            && copyNoReturn.PostCopyDescriptorBeginRead is null
            && copyNoReturn.RetainedDwordCount is null
            && !copyNoReturn.ReleaseHelperCallReached
            && copyNoReturn.Boundary
                == UiShaderSourcePoolResetStorageBoundary.CopyTargetDidNotReturn);

        var copyReturnedBeforeLaterReads = UiShaderSourcePoolResetStorageProjectionRules.Project(
            new UiShaderSourcePoolResetStorageCapture(
                RequestedDwordCount: 1,
                AllocatorReturnedBasePointer: 0x5000,
                InitialDescriptorBeginPointer: 0x100,
                InitialDescriptorEndPointer: 0x110,
                CopyTargetReturnedNormally: true,
                PostCopyDescriptorBeginPointer: 0x7000,
                PostCopyDescriptorEndPointer: 0x7008));
        check("Later begin/end captures enable retained arithmetic but stop at the unreached release-helper return",
            copyReturnedBeforeLaterReads.PostCopyDescriptorReadsReached
            && copyReturnedBeforeLaterReads.RetainedDwordCount == 2
            && copyReturnedBeforeLaterReads.ReleaseHelperCallReached
            && copyReturnedBeforeLaterReads.ReleaseHelperPointerArgument == 0x7000
            && copyReturnedBeforeLaterReads.ReleaseHelperReturnedNormally is null
            && !copyReturnedBeforeLaterReads.BeginPublicationReached
            && copyReturnedBeforeLaterReads.PublishedBeginPointer is null
            && copyReturnedBeforeLaterReads.PublishedStorageEndPointer is null
            && copyReturnedBeforeLaterReads.PublishedEndPointer is null
            && copyReturnedBeforeLaterReads.Boundary
                == UiShaderSourcePoolResetStorageBoundary.ReleaseHelperCallBoundary);

        var releaseNoReturn = UiShaderSourcePoolResetStorageProjectionRules.Project(
            copyReturnedBeforeLaterReads.Capture with { ReleaseHelperReturnedNormally = false });
        check("A captured nonreturning release helper stops before ordered descriptor publication",
            releaseNoReturn.ReleaseHelperCallReached
            && releaseNoReturn.ReleaseHelperReturnedNormally == false
            && !releaseNoReturn.BeginPublicationReached
            && !releaseNoReturn.StorageEndPublicationReached
            && !releaseNoReturn.EndPublicationReached
            && releaseNoReturn.ReturnedEax is null
            && releaseNoReturn.Boundary
                == UiShaderSourcePoolResetStorageBoundary.ReleaseHelperDidNotReturn);

        check("Unreached or missing captures are rejected instead of borrowing prior descriptor values",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetStorageProjectionRules.Project(
                    new UiShaderSourcePoolResetStorageCapture(
                        RequestedDwordCount: 1,
                        AllocatorReturnedBasePointer: 0x5000,
                        InitialDescriptorBeginPointer: 0x100,
                        InitialDescriptorEndPointer: 0x110,
                        CopyTargetReturnedNormally: true,
                        PostCopyDescriptorBeginPointer: 0x7000)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetStorageProjectionRules.Project(
                    new UiShaderSourcePoolResetStorageCapture(
                        RequestedDwordCount: 0x40000000,
                        InitialDescriptorBeginPointer: 1)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetStorageProjectionRules.Project(
                    new UiShaderSourcePoolResetStorageCapture(
                        RequestedDwordCount: 0x40000000,
                        AllocatorReturnedBasePointer: 0x5000)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetStorageProjectionRules.Project(
                    new UiShaderSourcePoolResetStorageCapture(
                        RequestedDwordCount: 1,
                        AllocatorReturnedBasePointer: 0x5000,
                        InitialDescriptorBeginPointer: 0x100,
                        InitialDescriptorEndPointer: 0x110,
                        CopyTargetReturnedNormally: false,
                        PostCopyDescriptorBeginPointer: 0x7000)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetStorageProjectionRules.Project(
                    new UiShaderSourcePoolResetStorageCapture(
                        RequestedDwordCount: 1,
                        AllocatorReturnedBasePointer: 0x5000,
                        InitialDescriptorBeginPointer: 0x100,
                        InitialDescriptorEndPointer: 0x110,
                        CopyTargetReturnedNormally: true,
                        PostCopyDescriptorBeginPointer: 0))));
    }

    private static bool HasExactSlice(
        UiShaderSourcePoolResetStorageInstructionEvidence instruction)
    {
        string[] bytes =
            UiShaderSourcePoolResetStorageProjectionRules.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint startRaw = UiShaderSourcePoolResetStorageProjectionRules.FunctionRawFileOffset;
        if (instruction.RawFileOffset < startRaw)
        {
            return false;
        }

        string[] expected = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(instruction.RawFileOffset - startRaw));
        return start + expected.Length <= bytes.Length
            && bytes.Skip(start).Take(expected.Length).SequenceEqual(expected);
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
