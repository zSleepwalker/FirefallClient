using Firefall.Reconstruction.Startup;

internal static class StackAllocationResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Stack allocation wrapper identifies the sample, raw range, VA mapping, and allocator call",
            StackAllocationResultProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && StackAllocationResultProjectionRules.EntryRawFileOffset == 0x12B7FB0
            && StackAllocationResultProjectionRules.EntryVirtualAddress == 0x16B8BB0
            && StackAllocationResultProjectionRules.EntryRawFileOffset
                + StackAllocationResultProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == StackAllocationResultProjectionRules.EntryVirtualAddress
            && StackAllocationResultProjectionRules.WrapperEndRawFileOffsetExclusive == 0x12B8021
            && StackAllocationResultProjectionRules.AllocatorCallRawFileOffset == 0x12B7FC7
            && StackAllocationResultProjectionRules.AllocatorCallNormalReturnRawFileOffset == 0x12B7FCC
            && StackAllocationResultProjectionRules.AllocatorFunctionRawFileOffset == 0x12C8A20
            && 0x12B7FCC + 0x10A54 == StackAllocationResultProjectionRules.AllocatorFunctionRawFileOffset);

        check("Captured wrapper bytes and instruction evidence retain the bounded raw interval",
            StackAllocationResultProjectionRules.CapturedWrapperBytes
                == "55 8B EC 83 EC 2C A1 00 50 DC 01 33 C5 89 45 FC 56 "
                    + "57 8B 7D 08 8B CF E8 54 0A 01 00 8B F0 85 F6 75 3F "
                    + "6A 27 50 88 45 D4 8D 45 D5 50 E8 19 59 2E 00 57 68 "
                    + "B8 EB C5 01 8D 45 D4 6A 27 50 FF 15 70 48 AC 01 8D "
                    + "45 D4 50 68 DA 00 00 00 68 58 EA C5 01 68 58 EA C5 "
                    + "01 FF 15 A0 5E C1 01 83 C4 2C 8B C6 8B 4D FC 5F 33 "
                    + "CD 5E E8 79 2C 0B 00 8B E5 5D C3"
            && StackAllocationResultProjectionRules.CapturedWrapperBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x71
            && StackAllocationResultProjectionRules.WrapperEndRawFileOffsetExclusive
                == StackAllocationResultProjectionRules.EntryRawFileOffset + 0x71
            && StackAllocationResultProjectionRules.Instructions.All(InstructionMatchesCapturedSlice)
            && StackAllocationResultProjectionRules.Instructions[0]
                == new StackAllocationResultInstructionEvidence(
                    "Load the requested byte count from the caller's stack argument",
                    0x12B7FC2,
                    "8B 7D 08")
            && StackAllocationResultProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12B7FC5).Bytes == "8B CF"
            && StackAllocationResultProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12B7FCC).Bytes
                == "8B F0 85 F6");

        check("Null and nonzero branches, diagnostic targets, and normal call continuations are explicit",
            StackAllocationResultProjectionRules.NonzeroResultBranchRawFileOffset == 0x12B7FD0
            && StackAllocationResultProjectionRules.NonzeroResultBranchTargetRawFileOffset == 0x12B8011
            && 0x12B7FD2 + 0x3F == StackAllocationResultProjectionRules.NonzeroResultBranchTargetRawFileOffset
            && StackAllocationResultProjectionRules.NullPathThunkRawFileOffset == 0x159D8FA
            && StackAllocationResultProjectionRules.NullPathThunkCallNormalReturnRawFileOffset
                == 0x12B7FE1
            && 0x12B7FE1 + 0x2E5919 == StackAllocationResultProjectionRules.NullPathThunkRawFileOffset
            && StackAllocationResultProjectionRules.FormatterCallRawFileOffset == 0x12B7FED
            && StackAllocationResultProjectionRules.FormatterCallNormalReturnRawFileOffset
                == 0x12B7FF3
            && StackAllocationResultProjectionRules.ReporterCallRawFileOffset == 0x12B8006
            && StackAllocationResultProjectionRules.ReporterCallNormalReturnRawFileOffset
                == 0x12B800C
            && StackAllocationResultProjectionRules.NullPathStackCleanupRawFileOffset == 0x12B800C
            && StackAllocationResultProjectionRules.CookieCheckFunctionRawFileOffset == 0x136AC96
            && StackAllocationResultProjectionRules.CookieCheckCallNormalReturnRawFileOffset
                == 0x12B801D
            && 0x12B801D + 0x0B2C79 == StackAllocationResultProjectionRules.CookieCheckFunctionRawFileOffset
            && StackAllocationResultProjectionRules.ReturnInstructionRawFileOffset == 0x12B8020
            && StackAllocationResultProjectionRules.Instructions
                .Where(instruction => instruction.NormalReturnContinuationRawFileOffset.HasValue)
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.NormalReturnContinuationRawFileOffset))
                .SequenceEqual(new (uint, uint?)[]
                {
                    (0x12B7FC7, 0x12B7FCC),
                    (0x12B7FDC, 0x12B7FE1),
                    (0x12B7FED, 0x12B7FF3),
                    (0x12B8006, 0x12B800C),
                    (0x12B8018, 0x12B801D)
                }));

        check("OOM formatting and reporting identify the captured request, format, cells, and report id",
            StackAllocationResultProjectionRules.OutOfMemoryFormatStringRawFileOffset == 0x185D7B8
            && StackAllocationResultProjectionRules.OutOfMemoryFormatStringVirtualAddress == 0x1C5EBB8
            && StackAllocationResultProjectionRules.OutOfMemoryFormatString == "OOM allocating %lu bytes."
            && StackAllocationResultProjectionRules.FormatterCellVirtualAddress == 0x1AC4870
            && StackAllocationResultProjectionRules.ReporterCellVirtualAddress == 0x1C15EA0
            && StackAllocationResultProjectionRules.OutOfMemoryReportId == 0xDA);

        StackAllocationResultProjection nonzero =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 96,
                    AllocatorResult: 0x1234,
                    NullPathThunkReturnedNormally: false,
                    FormatterReturnedNormally: false,
                    ReporterReturnedNormally: false,
                    CookieCheckReturnedNormally: true));
        check("A nonzero allocator result skips diagnostics and remains in EAX at the cookie-check call",
            !nonzero.TakesOutOfMemoryDiagnosticPath
            && !nonzero.ReachesNullPathThunk
            && !nonzero.ReachesFormatterCall
            && !nonzero.ReachesReporterCall
            && !nonzero.ReachesNullPathStackCleanup
            && !nonzero.ReachesAllocatorResultMoveToEax
            && nonzero.ReachesCookieCheck
            && nonzero.ReachesCookieCheckNormalContinuation
            && nonzero.ReachesReturnInstruction
            && nonzero.OutOfMemoryRequestedBytes is null
            && nonzero.OutOfMemoryReportId is null
            && nonzero.EaxAtCookieCheckCall == 0x1234);

        StackAllocationResultProjection nullResult =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 0x234,
                    AllocatorResult: 0,
                    NullPathThunkReturnedNormally: true,
                    FormatterReturnedNormally: true,
                    ReporterReturnedNormally: true,
                    CookieCheckReturnedNormally: true));
        check("A null allocator result reaches cleanup and restores EAX from ESI before the cookie-check call",
            nullResult.TakesOutOfMemoryDiagnosticPath
            && nullResult.ReachesNullPathThunk
            && nullResult.ReachesFormatterCall
            && nullResult.ReachesReporterCall
            && nullResult.ReachesNullPathStackCleanup
            && nullResult.ReachesAllocatorResultMoveToEax
            && nullResult.ReachesCookieCheck
            && nullResult.ReachesCookieCheckNormalContinuation
            && nullResult.ReachesReturnInstruction
            && nullResult.OutOfMemoryRequestedBytes == 0x234
            && nullResult.OutOfMemoryReportId == 0xDA
            && nullResult.EaxAtCookieCheckCall == 0);

        StackAllocationResultProjection diagnosticDoesNotReturn =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 12,
                    AllocatorResult: 0,
                    NullPathThunkReturnedNormally: false,
                    FormatterReturnedNormally: false,
                    ReporterReturnedNormally: false,
                    CookieCheckReturnedNormally: false));
        check("A nonreturning null-path thunk does not reach the formatter or later continuations",
            diagnosticDoesNotReturn.TakesOutOfMemoryDiagnosticPath
            && diagnosticDoesNotReturn.ReachesNullPathThunk
            && !diagnosticDoesNotReturn.ReachesFormatterCall
            && !diagnosticDoesNotReturn.ReachesReporterCall
            && !diagnosticDoesNotReturn.ReachesNullPathStackCleanup
            && !diagnosticDoesNotReturn.ReachesAllocatorResultMoveToEax
            && !diagnosticDoesNotReturn.ReachesCookieCheck
            && !diagnosticDoesNotReturn.ReachesReturnInstruction
            && diagnosticDoesNotReturn.OutOfMemoryRequestedBytes is null
            && diagnosticDoesNotReturn.OutOfMemoryReportId is null
            && diagnosticDoesNotReturn.EaxAtCookieCheckCall is null);

        StackAllocationResultProjection formatterDoesNotReturn =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 12,
                    AllocatorResult: 0,
                    NullPathThunkReturnedNormally: true,
                    FormatterReturnedNormally: false,
                    ReporterReturnedNormally: false,
                    CookieCheckReturnedNormally: false));
        check("A nonreturning formatter has its requested-size argument but does not reach the reporter",
            formatterDoesNotReturn.ReachesFormatterCall
            && !formatterDoesNotReturn.ReachesReporterCall
            && !formatterDoesNotReturn.ReachesNullPathStackCleanup
            && formatterDoesNotReturn.OutOfMemoryRequestedBytes == 12
            && formatterDoesNotReturn.OutOfMemoryReportId is null
            && !formatterDoesNotReturn.ReachesCookieCheck);

        StackAllocationResultProjection reporterDoesNotReturn =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 12,
                    AllocatorResult: 0,
                    NullPathThunkReturnedNormally: true,
                    FormatterReturnedNormally: true,
                    ReporterReturnedNormally: false,
                    CookieCheckReturnedNormally: false));
        check("A nonreturning reporter does not reach stack cleanup or the cookie-check call",
            reporterDoesNotReturn.ReachesReporterCall
            && !reporterDoesNotReturn.ReachesNullPathStackCleanup
            && reporterDoesNotReturn.OutOfMemoryRequestedBytes == 12
            && reporterDoesNotReturn.OutOfMemoryReportId == 0xDA
            && !reporterDoesNotReturn.ReachesCookieCheck);

        StackAllocationResultProjection cookieCheckDoesNotReturn =
            StackAllocationResultProjectionRules.Project(
                new StackAllocationResultCapture(
                    RequestedBytes: 12,
                    AllocatorResult: 0,
                    NullPathThunkReturnedNormally: true,
                    FormatterReturnedNormally: true,
                    ReporterReturnedNormally: true,
                    CookieCheckReturnedNormally: false));
        check("A nonreturning cookie check has no normal continuation or RET path",
            cookieCheckDoesNotReturn.ReachesReporterCall
            && cookieCheckDoesNotReturn.ReachesNullPathStackCleanup
            && cookieCheckDoesNotReturn.ReachesAllocatorResultMoveToEax
            && cookieCheckDoesNotReturn.ReachesCookieCheck
            && !cookieCheckDoesNotReturn.ReachesCookieCheckNormalContinuation
            && !cookieCheckDoesNotReturn.ReachesReturnInstruction
            && cookieCheckDoesNotReturn.EaxAtCookieCheckCall == 0);

        check("Normal-return captures are rejected for diagnostic and cookie calls that cannot be reached",
            RejectsUnreachedNormalReturn(
                new StackAllocationResultCapture(1, 1, true, false, false, false),
                nameof(StackAllocationResultCapture.NullPathThunkReturnedNormally))
            && RejectsUnreachedNormalReturn(
                new StackAllocationResultCapture(1, 0, false, true, false, false),
                nameof(StackAllocationResultCapture.FormatterReturnedNormally))
            && RejectsUnreachedNormalReturn(
                new StackAllocationResultCapture(1, 0, true, false, true, false),
                nameof(StackAllocationResultCapture.ReporterReturnedNormally))
            && RejectsUnreachedNormalReturn(
                new StackAllocationResultCapture(1, 0, true, true, false, true),
                nameof(StackAllocationResultCapture.CookieCheckReturnedNormally)));
    }

    private static bool InstructionMatchesCapturedSlice(
        StackAllocationResultInstructionEvidence instruction)
    {
        string[] captured = StackAllocationResultProjectionRules.CapturedWrapperBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] evidence = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (instruction.RawFileOffset < StackAllocationResultProjectionRules.EntryRawFileOffset)
            return false;

        int offset = checked((int)(
            instruction.RawFileOffset - StackAllocationResultProjectionRules.EntryRawFileOffset));
        return offset + evidence.Length <= captured.Length
            && captured.Skip(offset).Take(evidence.Length)
                .SequenceEqual(evidence, StringComparer.OrdinalIgnoreCase);
    }

    private static bool RejectsUnreachedNormalReturn(
        StackAllocationResultCapture capture,
        string expectedParameterName)
    {
        try
        {
            StackAllocationResultProjectionRules.Project(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == expectedParameterName;
        }
    }
}
