namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetEntryCapture(
    uint CallbackArgument,
    uint? LaterPoolCounter,
    bool DiagnosticCallReturnedNormally = true);

public enum UiShaderSourcePoolResetEntryBoundary
{
    DiagnosticDidNotReturnNormally,
    ZeroCounterEarlyReturn,
    NonzeroCounterResetWorkUnmodeled
}

public readonly record struct UiShaderSourcePoolResetEntryProjection(
    UiShaderSourcePoolResetEntryCapture Capture,
    uint? SelectedDiagnosticId,
    UiShaderSourcePoolResetEntryBoundary Boundary,
    uint? ReturnEax);

public readonly record struct UiShaderSourcePoolResetEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ThisRelativeOffset = null,
    uint? DiagnosticId = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the callback-null diagnostic and the later pool-counter entry gate.
/// The counter is captured independently after the fence and any normally
/// returning diagnostic callback. Nonzero selects unmodeled reset work; no
/// reset, array construction, callback traversal, or ownership is projected.
/// </summary>
public static class UiShaderSourcePoolResetEntryProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CDC50;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint FenceRawFileOffset = 0x12CDC9D;
    public const uint PoolCounterCompareRawFileOffset = 0x12CDCA0;
    public const uint NonzeroCounterBranchRawFileOffset = 0x12CDCAA;
    public const uint NonzeroCounterContinuationRawFileOffset = 0x12CDCC2;
    public const uint ZeroCounterEaxClearRawFileOffset = 0x12CDCAC;
    public const uint ZeroCounterReturnRawFileOffset = 0x12CDCBF;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CDCC2;
    public const uint EntryEndVirtualAddressExclusive =
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const int ThisPoolCounterOffset = 0x10;
    public const uint CallbackNullDiagnosticId = 0x215;
    public const uint DiagnosticReporterCellVirtualAddress = 0x01C15EA0;

    public const string CapturedEntryBytes =
        "55 8B EC 6A FF 68 48 5A A9 01 64 A1 00 00 00 00 " +
        "50 83 EC 28 53 56 57 A1 00 50 DC 01 33 C5 50 8D " +
        "45 F4 64 A3 00 00 00 00 8B F9 89 7D E4 8B 45 08 " +
        "85 C0 75 19 50 68 15 02 00 00 68 BF 41 B7 01 68 " +
        "E3 41 B7 01 FF 15 A0 5E C1 01 83 C4 10 0F AE E8 " +
        "83 7F 10 00 8D 47 10 89 45 D8 75 16 33 C0 8B 4D " +
        "F4 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D C2 04 00";

    public static IReadOnlyList<UiShaderSourcePoolResetEntryInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Read the callback argument from [EBP+8]",
                0x12CDC7D,
                "8B 45 08"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Test the captured callback argument for null",
                0x12CDC80,
                "85 C0"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Skip diagnostic setup for a non-null callback and continue at the fence",
                0x12CDC82,
                "75 19",
                BranchTargetRawFileOffset: FenceRawFileOffset),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Push the null callback argument for diagnostic 0x215",
                0x12CDC84,
                "50"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Select diagnostic ID 0x215 (decimal 533)",
                0x12CDC85,
                "68 15 02 00 00",
                DiagnosticId: CallbackNullDiagnosticId),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Call the diagnostic reporter through its indirect cell",
                0x12CDC94,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterCellVirtualAddress),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Continue after a normally returning diagnostic report",
                0x12CDC9A,
                "83 C4 10"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Execute the load fence before the pool-counter read",
                FenceRawFileOffset,
                "0F AE E8"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Read and compare this + 0x10 as a separate later counter value",
                PoolCounterCompareRawFileOffset,
                "83 7F 10 00",
                ThisPoolCounterOffset),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Select unmodeled reset work for a nonzero counter",
                NonzeroCounterBranchRawFileOffset,
                "75 16",
                BranchTargetRawFileOffset: NonzeroCounterContinuationRawFileOffset),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Clear EAX on the zero-counter path",
                ZeroCounterEaxClearRawFileOffset,
                "33 C0"),
            new UiShaderSourcePoolResetEntryInstructionEvidence(
                "Return EAX=0, popping the callback argument",
                ZeroCounterReturnRawFileOffset,
                "C2 04 00")
        });

    public static UiShaderSourcePoolResetEntryProjection Project(
        UiShaderSourcePoolResetEntryCapture capture)
    {
        uint? selectedDiagnosticId =
            capture.CallbackArgument == 0 ? CallbackNullDiagnosticId : null;

        if (!selectedDiagnosticId.HasValue && !capture.DiagnosticCallReturnedNormally)
        {
            throw new ArgumentException(
                "A diagnostic return outcome is invalid when the callback argument is non-null.",
                nameof(capture));
        }

        if (selectedDiagnosticId.HasValue && !capture.DiagnosticCallReturnedNormally)
        {
            if (capture.LaterPoolCounter.HasValue)
            {
                throw new ArgumentException(
                    "The later pool counter is not read when the diagnostic call does not return.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolResetEntryProjection(
                capture,
                selectedDiagnosticId,
                UiShaderSourcePoolResetEntryBoundary.DiagnosticDidNotReturnNormally,
                null);
        }

        if (!capture.LaterPoolCounter.HasValue)
        {
            throw new ArgumentException(
                "The later pool counter is required when execution reaches its compare.",
                nameof(capture));
        }

        if (capture.LaterPoolCounter.Value == 0)
        {
            return new UiShaderSourcePoolResetEntryProjection(
                capture,
                selectedDiagnosticId,
                UiShaderSourcePoolResetEntryBoundary.ZeroCounterEarlyReturn,
                0);
        }

        return new UiShaderSourcePoolResetEntryProjection(
            capture,
            selectedDiagnosticId,
            UiShaderSourcePoolResetEntryBoundary.NonzeroCounterResetWorkUnmodeled,
            null);
    }
}
