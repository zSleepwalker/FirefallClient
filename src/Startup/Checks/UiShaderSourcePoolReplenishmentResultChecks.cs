using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReplenishmentResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Result-continuation sample identity, PE mapping, and raw bounds are exact",
            UiShaderSourcePoolReplenishmentResultProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReplenishmentResultProjectionRules.EntryRawFileOffset
                == 0x12CE11E
            && UiShaderSourcePoolReplenishmentResultProjectionRules.EntryVirtualAddress
                == 0x16CED1E
            && UiShaderSourcePoolReplenishmentResultProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolReplenishmentResultProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolReplenishmentResultProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolReplenishmentResultProjectionRules.CapturedContinuationEndRawFileOffsetExclusive
                == 0x12CE16A
            && UiShaderSourcePoolReplenishmentResultProjectionRules.PushHelperCallRawFileOffset
                + UiShaderSourcePoolReplenishmentResultProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CED65
            && UiShaderSourcePoolReplenishmentResultProjectionRules.PushHelperCallEndRawFileOffsetExclusive
                == UiShaderSourcePoolReplenishmentResultProjectionRules.PushHelperCallRawFileOffset + 5
            && UiShaderSourcePoolReplenishmentResultProjectionRules.NullPathThunkCallRawFileOffset + 5
                + 0x2CF7C7
                == UiShaderSourcePoolReplenishmentResultProjectionRules.NullPathThunkRawFileOffset
            && UiShaderSourcePoolReplenishmentResultProjectionRules.PushHelperCallEndRawFileOffsetExclusive
                - 0x70A
                == UiShaderSourcePoolReplenishmentResultProjectionRules.PushHelperRawFileOffset
            && UiShaderSourcePoolReplenishmentResultProjectionRules.FormatterCallRawFileOffset == 0x12CE13F
            && UiShaderSourcePoolReplenishmentResultProjectionRules.OutOfMemoryReportIdPushRawFileOffset
                == 0x12CE149
            && UiShaderSourcePoolReplenishmentResultProjectionRules.ReporterCallRawFileOffset == 0x12CE158
            && UiShaderSourcePoolReplenishmentResultProjectionRules.PoolReceiverLoadRawFileOffset
                == 0x12CE162
            && UiShaderSourcePoolReplenishmentResultProjectionRules.FormatterCellVirtualAddress
                == 0x01AC4870
            && UiShaderSourcePoolReplenishmentResultProjectionRules.OutOfMemoryFormatStringVirtualAddress
                == 0x01C5EBB8
            && UiShaderSourcePoolReplenishmentResultProjectionRules.ReporterCellVirtualAddress
                == 0x01C15EA0);

        check("Captured bytes cover the diagnostics, first push, and complete helper call",
            UiShaderSourcePoolReplenishmentResultProjectionRules.CapturedContinuationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x4C
            && UiShaderSourcePoolReplenishmentResultProjectionRules.Instructions.All(instruction =>
                CapturedBytesAt(instruction.RawFileOffset,
                    instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                == instruction.Bytes)
            && UiShaderSourcePoolReplenishmentResultProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.TargetRawFileOffset, instruction.AbsoluteVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    uint? TargetRawFileOffset,
                    uint? AbsoluteVirtualAddress)[]
                {
                    (0x12CE11E, null, null),
                    (0x12CE120, null, null),
                    (0x12CE122, 0x12CE161, null),
                    (0x12CE12E, 0x159D8FA, null),
                    (0x12CE133, null, null),
                    (0x12CE134, null, 0x01C5EBB8),
                    (0x12CE13F, null, 0x01AC4870),
                    (0x12CE149, null, null),
                    (0x12CE158, null, 0x01C15EA0),
                    (0x12CE161, null, null),
                    (0x12CE162, null, null),
                    (0x12CE165, 0x12CDA60, null)
                }));

        UiShaderSourcePoolReplenishmentResultProjection nonnull =
            UiShaderSourcePoolReplenishmentResultProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentResultCapture(
                    AllocatorResult: 0x12345678,
                    RequestedSizeBytes: 0x40,
                    PoolPointer: 0x20000000,
                    NullPathDiagnosticCallsReturnedNormally: false));
        check("A nonnull allocator result skips diagnostics and is preserved as call argument 0",
            !nonnull.TakesNullResultDiagnosticPath
            && nonnull.OutOfMemoryRequestedSizeBytes is null
            && nonnull.OutOfMemoryReportId is null
            && nonnull.ReachesFirstPush
            && nonnull.ReachesPushHelperCall
            && nonnull.PushHelperReceiver == 0x20000008
            && nonnull.PushHelperArgument0 == 0x12345678
            && nonnull.PushHelperTargetRawFileOffset == 0x12CDA60);

        UiShaderSourcePoolReplenishmentResultProjection nullWithNormalReturns =
            UiShaderSourcePoolReplenishmentResultProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentResultCapture(
                    AllocatorResult: 0,
                    RequestedSizeBytes: 0x345,
                    PoolPointer: 0x30000000,
                    NullPathDiagnosticCallsReturnedNormally: true));
        check("The null path models OOM reporting and the subsequent call only at its normal-return boundary",
            nullWithNormalReturns.TakesNullResultDiagnosticPath
            && nullWithNormalReturns.OutOfMemoryRequestedSizeBytes == 0x345
            && nullWithNormalReturns.OutOfMemoryReportId == 0xDA
            && nullWithNormalReturns.ReachesFirstPush
            && nullWithNormalReturns.ReachesPushHelperCall
            && nullWithNormalReturns.PushHelperReceiver == 0x30000008
            && nullWithNormalReturns.PushHelperArgument0 == 0
            && nullWithNormalReturns.PushHelperTargetRawFileOffset == 0x12CDA60);

        UiShaderSourcePoolReplenishmentResultProjection nullWithoutNormalReturns =
            UiShaderSourcePoolReplenishmentResultProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentResultCapture(
                    AllocatorResult: 0,
                    RequestedSizeBytes: 0x345,
                    PoolPointer: 0x30000000,
                    NullPathDiagnosticCallsReturnedNormally: false));
        check("Without captured normal diagnostic returns the model stops before the first push",
            nullWithoutNormalReturns.TakesNullResultDiagnosticPath
            && nullWithoutNormalReturns.OutOfMemoryRequestedSizeBytes == 0x345
            && nullWithoutNormalReturns.OutOfMemoryReportId == 0xDA
            && !nullWithoutNormalReturns.ReachesFirstPush
            && !nullWithoutNormalReturns.ReachesPushHelperCall
            && nullWithoutNormalReturns.PushHelperReceiver is null
            && nullWithoutNormalReturns.PushHelperArgument0 is null
            && nullWithoutNormalReturns.PushHelperTargetRawFileOffset is null);
    }

    private static string CapturedBytesAt(uint rawFileOffset, int byteLength)
    {
        string[] capturedBytes =
            UiShaderSourcePoolReplenishmentResultProjectionRules.CapturedContinuationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rawFileOffset < UiShaderSourcePoolReplenishmentResultProjectionRules.EntryRawFileOffset
            || byteLength < 0)
        {
            return string.Empty;
        }

        uint relativeOffset =
            rawFileOffset - UiShaderSourcePoolReplenishmentResultProjectionRules.EntryRawFileOffset;
        if (relativeOffset > int.MaxValue)
        {
            return string.Empty;
        }

        int start = (int)relativeOffset;
        if (start + byteLength > capturedBytes.Length)
        {
            return string.Empty;
        }

        return string.Join(" ", capturedBytes.Skip(start).Take(byteLength));
    }
}
