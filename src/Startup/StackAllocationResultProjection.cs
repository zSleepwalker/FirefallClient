namespace Firefall.Reconstruction.Startup;

public readonly record struct StackAllocationResultCapture(
    uint RequestedBytes,
    uint AllocatorResult,
    bool NullPathThunkReturnedNormally,
    bool FormatterReturnedNormally,
    bool ReporterReturnedNormally,
    bool CookieCheckReturnedNormally);

public readonly record struct StackAllocationResultProjection(
    StackAllocationResultCapture Capture,
    bool TakesOutOfMemoryDiagnosticPath,
    bool ReachesNullPathThunk,
    bool ReachesFormatterCall,
    bool ReachesReporterCall,
    bool ReachesNullPathStackCleanup,
    bool ReachesAllocatorResultMoveToEax,
    bool ReachesCookieCheck,
    bool ReachesCookieCheckNormalContinuation,
    bool ReachesReturnInstruction,
    uint? OutOfMemoryRequestedBytes,
    uint? OutOfMemoryReportId,
    uint? EaxAtCookieCheckCall);

public readonly record struct StackAllocationResultInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? AbsoluteVirtualAddress = null,
    uint? NormalReturnContinuationRawFileOffset = null);

/// <summary>
/// Projects the captured allocation result through the wrapper's observed diagnostics
/// and normal-call continuations without modeling allocator, diagnostic, or cookie-check effects.
/// </summary>
public static class StackAllocationResultProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12B7FB0;
    public const uint EntryVirtualAddress = 0x16B8BB0;
    public const uint WrapperEndRawFileOffsetExclusive = 0x12B8021;

    public const uint RequestedBytesLoadRawFileOffset = 0x12B7FC2;
    public const uint AllocatorReceiverForwardRawFileOffset = 0x12B7FC5;
    public const uint AllocatorCallRawFileOffset = 0x12B7FC7;
    public const uint AllocatorCallNormalReturnRawFileOffset = 0x12B7FCC;
    public const uint AllocatorFunctionRawFileOffset = 0x12C8A20;
    public const uint AllocatorResultCaptureRawFileOffset = 0x12B7FCC;
    public const uint AllocatorResultTestRawFileOffset = 0x12B7FCE;
    public const uint NonzeroResultBranchRawFileOffset = 0x12B7FD0;
    public const uint NonzeroResultBranchTargetRawFileOffset = 0x12B8011;

    public const uint NullPathThunkCallRawFileOffset = 0x12B7FDC;
    public const uint NullPathThunkCallNormalReturnRawFileOffset = 0x12B7FE1;
    public const uint NullPathThunkRawFileOffset = 0x159D8FA;
    public const uint OutOfMemoryFormatPushRawFileOffset = 0x12B7FE2;
    public const uint OutOfMemoryFormatStringRawFileOffset = 0x185D7B8;
    public const uint OutOfMemoryFormatStringVirtualAddress = 0x01C5EBB8;
    public const string OutOfMemoryFormatString = "OOM allocating %lu bytes.";
    public const uint FormatterCallRawFileOffset = 0x12B7FED;
    public const uint FormatterCallNormalReturnRawFileOffset = 0x12B7FF3;
    public const uint FormatterCellVirtualAddress = 0x01AC4870;
    public const uint OutOfMemoryReportIdPushRawFileOffset = 0x12B7FF7;
    public const uint OutOfMemoryReportId = 0xDA;
    public const uint ReporterCallRawFileOffset = 0x12B8006;
    public const uint ReporterCallNormalReturnRawFileOffset = 0x12B800C;
    public const uint ReporterCellVirtualAddress = 0x01C15EA0;

    public const uint NullPathStackCleanupRawFileOffset = 0x12B800C;
    public const uint AllocatorResultMoveToEaxRawFileOffset = 0x12B800F;
    public const uint CookieCheckCallRawFileOffset = 0x12B8018;
    public const uint CookieCheckCallNormalReturnRawFileOffset = 0x12B801D;
    public const uint CookieCheckFunctionRawFileOffset = 0x136AC96;
    public const uint ReturnInstructionRawFileOffset = 0x12B8020;

    public const string CapturedWrapperBytes =
        "55 8B EC 83 EC 2C A1 00 50 DC 01 33 C5 89 45 FC 56 " +
        "57 8B 7D 08 8B CF E8 54 0A 01 00 8B F0 85 F6 75 3F " +
        "6A 27 50 88 45 D4 8D 45 D5 50 E8 19 59 2E 00 57 68 " +
        "B8 EB C5 01 8D 45 D4 6A 27 50 FF 15 70 48 AC 01 8D " +
        "45 D4 50 68 DA 00 00 00 68 58 EA C5 01 68 58 EA C5 " +
        "01 FF 15 A0 5E C1 01 83 C4 2C 8B C6 8B 4D FC 5F 33 " +
        "CD 5E E8 79 2C 0B 00 8B E5 5D C3";

    public static IReadOnlyList<StackAllocationResultInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new StackAllocationResultInstructionEvidence(
                "Load the requested byte count from the caller's stack argument",
                RequestedBytesLoadRawFileOffset,
                "8B 7D 08"),
            new StackAllocationResultInstructionEvidence(
                "Forward the requested byte count in ECX",
                AllocatorReceiverForwardRawFileOffset,
                "8B CF"),
            new StackAllocationResultInstructionEvidence(
                "Call the allocator and continue normally at the result capture",
                AllocatorCallRawFileOffset,
                "E8 54 0A 01 00",
                AllocatorFunctionRawFileOffset,
                NormalReturnContinuationRawFileOffset: AllocatorCallNormalReturnRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Save and test the allocator result in ESI",
                AllocatorResultCaptureRawFileOffset,
                "8B F0 85 F6"),
            new StackAllocationResultInstructionEvidence(
                "Skip diagnostics for a nonzero result",
                NonzeroResultBranchRawFileOffset,
                "75 3F",
                NonzeroResultBranchTargetRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Call the null-path thunk",
                NullPathThunkCallRawFileOffset,
                "E8 19 59 2E 00",
                NullPathThunkRawFileOffset,
                NormalReturnContinuationRawFileOffset: NullPathThunkCallNormalReturnRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Pass the requested size to the OOM formatter",
                0x12B7FE1,
                "57"),
            new StackAllocationResultInstructionEvidence(
                "Push the OOM format string",
                OutOfMemoryFormatPushRawFileOffset,
                "68 B8 EB C5 01",
                AbsoluteVirtualAddress: OutOfMemoryFormatStringVirtualAddress),
            new StackAllocationResultInstructionEvidence(
                "Call the formatter through its absolute cell",
                FormatterCallRawFileOffset,
                "FF 15 70 48 AC 01",
                AbsoluteVirtualAddress: FormatterCellVirtualAddress,
                NormalReturnContinuationRawFileOffset: FormatterCallNormalReturnRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Push diagnostic report id 0xDA",
                OutOfMemoryReportIdPushRawFileOffset,
                "68 DA 00 00 00"),
            new StackAllocationResultInstructionEvidence(
                "Call the reporter through its absolute cell",
                ReporterCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                AbsoluteVirtualAddress: ReporterCellVirtualAddress,
                NormalReturnContinuationRawFileOffset: ReporterCallNormalReturnRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Clean up the null-path stack arguments",
                NullPathStackCleanupRawFileOffset,
                "83 C4 2C"),
            new StackAllocationResultInstructionEvidence(
                "Move the saved allocator result to EAX on the null path",
                AllocatorResultMoveToEaxRawFileOffset,
                "8B C6"),
            new StackAllocationResultInstructionEvidence(
                "Call the cookie check and continue normally at the common epilogue",
                CookieCheckCallRawFileOffset,
                "E8 79 2C 0B 00",
                CookieCheckFunctionRawFileOffset,
                NormalReturnContinuationRawFileOffset: CookieCheckCallNormalReturnRawFileOffset),
            new StackAllocationResultInstructionEvidence(
                "Return from the wrapper",
                ReturnInstructionRawFileOffset,
                "C3")
        });

    public static StackAllocationResultProjection Project(StackAllocationResultCapture capture)
    {
        bool takesOutOfMemoryDiagnosticPath = capture.AllocatorResult == 0;
        bool reachesNullPathThunk = takesOutOfMemoryDiagnosticPath;
        bool reachesFormatterCall =
            reachesNullPathThunk && capture.NullPathThunkReturnedNormally;
        bool reachesReporterCall =
            reachesFormatterCall && capture.FormatterReturnedNormally;
        bool reachesNullPathStackCleanup =
            reachesReporterCall && capture.ReporterReturnedNormally;
        bool reachesCookieCheck =
            !takesOutOfMemoryDiagnosticPath || reachesNullPathStackCleanup;
        bool reachesCookieCheckNormalContinuation =
            reachesCookieCheck && capture.CookieCheckReturnedNormally;
        ValidateNormalReturnCaptures(
            capture,
            reachesNullPathThunk,
            reachesFormatterCall,
            reachesReporterCall,
            reachesCookieCheck);

        return new(
            capture,
            takesOutOfMemoryDiagnosticPath,
            reachesNullPathThunk,
            reachesFormatterCall,
            reachesReporterCall,
            reachesNullPathStackCleanup,
            reachesNullPathStackCleanup,
            reachesCookieCheck,
            reachesCookieCheckNormalContinuation,
            reachesCookieCheckNormalContinuation,
            reachesFormatterCall ? capture.RequestedBytes : null,
            reachesReporterCall ? OutOfMemoryReportId : null,
            reachesCookieCheck ? capture.AllocatorResult : null);
    }

    private static void ValidateNormalReturnCaptures(
        StackAllocationResultCapture capture,
        bool reachesNullPathThunk,
        bool reachesFormatterCall,
        bool reachesReporterCall,
        bool reachesCookieCheck)
    {
        ValidateNormalReturnCapture(
            reachesNullPathThunk,
            capture.NullPathThunkReturnedNormally,
            nameof(capture.NullPathThunkReturnedNormally));
        ValidateNormalReturnCapture(
            reachesFormatterCall,
            capture.FormatterReturnedNormally,
            nameof(capture.FormatterReturnedNormally));
        ValidateNormalReturnCapture(
            reachesReporterCall,
            capture.ReporterReturnedNormally,
            nameof(capture.ReporterReturnedNormally));
        ValidateNormalReturnCapture(
            reachesCookieCheck,
            capture.CookieCheckReturnedNormally,
            nameof(capture.CookieCheckReturnedNormally));
    }

    private static void ValidateNormalReturnCapture(
        bool callWasReached,
        bool returnedNormally,
        string parameterName)
    {
        if (!callWasReached && returnedNormally)
            throw new ArgumentException(
                "A normal return cannot be captured for a call that was not reached.",
                parameterName);
    }
}
