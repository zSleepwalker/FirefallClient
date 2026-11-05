namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolReplenishmentResultCapture(
    uint AllocatorResult,
    uint RequestedSizeBytes,
    uint PoolPointer,
    bool NullPathDiagnosticCallsReturnedNormally);

public readonly record struct UiShaderSourcePoolReplenishmentResultProjection(
    UiShaderSourcePoolReplenishmentResultCapture Capture,
    bool TakesNullResultDiagnosticPath,
    bool ReachesFirstPush,
    bool ReachesPushHelperCall,
    uint? OutOfMemoryRequestedSizeBytes,
    uint? OutOfMemoryReportId,
    uint? PushHelperReceiver,
    uint? PushHelperArgument0,
    uint? PushHelperTargetRawFileOffset);

public readonly record struct UiShaderSourcePoolReplenishmentResultInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? AbsoluteVirtualAddress = null);

/// <summary>
/// Projects the captured allocator result through its conditional diagnostics and
/// the first PushHelper call, without modeling that callee's effects.
/// The null path continues past diagnostics only when their normal return is captured.
/// </summary>
public static class UiShaderSourcePoolReplenishmentResultProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CE11E;
    public const uint EntryVirtualAddress = 0x16CED1E;
    public const uint CapturedContinuationEndRawFileOffsetExclusive = 0x12CE16A;

    public const uint AllocatorResultCaptureRawFileOffset = 0x12CE11E;
    public const uint NullResultBranchRawFileOffset = 0x12CE122;
    public const uint NullPathThunkCallRawFileOffset = 0x12CE12E;
    public const uint NullPathThunkRawFileOffset = 0x159D8FA;
    public const uint RequestedSizePushRawFileOffset = 0x12CE133;
    public const uint OutOfMemoryFormatPushRawFileOffset = 0x12CE134;
    public const uint OutOfMemoryFormatStringVirtualAddress = 0x01C5EBB8;
    public const uint FormatterCallRawFileOffset = 0x12CE13F;
    public const uint FormatterCellVirtualAddress = 0x01AC4870;
    public const uint ReporterCallRawFileOffset = 0x12CE158;
    public const uint ReporterCellVirtualAddress = 0x01C15EA0;
    public const uint OutOfMemoryReportIdPushRawFileOffset = 0x12CE149;
    public const uint OutOfMemoryReportId = 0xDA;

    public const uint FirstPushRawFileOffset = 0x12CE161;
    public const uint PoolReceiverLoadRawFileOffset = 0x12CE162;
    public const uint PoolReceiverOffset = 0x08;
    public const uint PushHelperCallRawFileOffset = 0x12CE165;
    public const uint PushHelperRawFileOffset = 0x12CDA60;
    public const uint PushHelperCallEndRawFileOffsetExclusive = 0x12CE16A;

    public const string CapturedContinuationBytes =
        "8B D8 85 DB 75 3D 6A 27 50 88 45 D4 8D 45 D5 50 " +
        "E8 C7 F7 2C 00 56 68 B8 EB C5 01 8D 45 D4 6A 27 50 " +
        "FF 15 70 48 AC 01 8D 45 D4 50 68 DA 00 00 00 " +
        "68 58 EA C5 01 68 58 EA C5 01 FF 15 A0 5E C1 01 " +
        "83 C4 2C 53 8D 4F 08 E8 F6 F8 FF FF";

    public static IReadOnlyList<UiShaderSourcePoolReplenishmentResultInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Capture the allocator return value in EBX",
                AllocatorResultCaptureRawFileOffset,
                "8B D8"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Test the captured allocator result",
                0x12CE120,
                "85 DB"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Branch over diagnostics when the captured result is nonnull",
                NullResultBranchRawFileOffset,
                "75 3D",
                FirstPushRawFileOffset),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Call the null-path thunk",
                NullPathThunkCallRawFileOffset,
                "E8 C7 F7 2C 00",
                NullPathThunkRawFileOffset),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Pass the requested size in ESI to the OOM formatting sequence",
                RequestedSizePushRawFileOffset,
                "56"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Push the OOM format string after the requested-size value",
                OutOfMemoryFormatPushRawFileOffset,
                "68 B8 EB C5 01",
                AbsoluteVirtualAddress: OutOfMemoryFormatStringVirtualAddress),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Call the formatter through its absolute cell",
                FormatterCallRawFileOffset,
                "FF 15 70 48 AC 01",
                AbsoluteVirtualAddress: FormatterCellVirtualAddress),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Push diagnostic report id 0xDA",
                OutOfMemoryReportIdPushRawFileOffset,
                "68 DA 00 00 00"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Call the reporter through its absolute cell",
                ReporterCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                AbsoluteVirtualAddress: ReporterCellVirtualAddress),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Push the captured allocator result as PushHelper argument 0",
                FirstPushRawFileOffset,
                "53"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Load the pool receiver at EDI plus 8 into ECX",
                PoolReceiverLoadRawFileOffset,
                "8D 4F 08"),
            new UiShaderSourcePoolReplenishmentResultInstructionEvidence(
                "Call the pool push helper",
                PushHelperCallRawFileOffset,
                "E8 F6 F8 FF FF",
                PushHelperRawFileOffset)
        });

    public static UiShaderSourcePoolReplenishmentResultProjection Project(
        UiShaderSourcePoolReplenishmentResultCapture capture)
    {
        bool takesNullResultDiagnosticPath = capture.AllocatorResult == 0;
        bool reachesContinuation =
            !takesNullResultDiagnosticPath || capture.NullPathDiagnosticCallsReturnedNormally;

        return new UiShaderSourcePoolReplenishmentResultProjection(
            capture,
            takesNullResultDiagnosticPath,
            reachesContinuation,
            reachesContinuation,
            takesNullResultDiagnosticPath ? capture.RequestedSizeBytes : null,
            takesNullResultDiagnosticPath ? OutOfMemoryReportId : null,
            reachesContinuation
                ? unchecked(capture.PoolPointer + PoolReceiverOffset)
                : null,
            reachesContinuation ? capture.AllocatorResult : null,
            reachesContinuation ? PushHelperRawFileOffset : null);
    }
}
