namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolResetPrimaryGateMode
{
    Initial,
    Repeat
}

public enum UiShaderSourcePoolResetPrimaryGateBoundary
{
    PopCallBoundary,
    SecondaryGate,
    AppendBranch
}

public readonly record struct UiShaderSourcePoolResetPrimaryGateCapture(
    UiShaderSourcePoolResetPrimaryGateMode Mode,
    uint ReceiverPointer,
    uint? PopResultPointer);

public readonly record struct UiShaderSourcePoolResetPrimaryGateProjection(
    UiShaderSourcePoolResetPrimaryGateCapture Capture,
    bool DescriptorZeroingReached,
    uint? DescriptorBeginPointer,
    uint? DescriptorEndPointer,
    uint? DescriptorStorageEndPointer,
    UiShaderSourcePoolResetPrimaryGateBoundary Boundary,
    uint? ContinuationRawFileOffset);

public readonly record struct UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawFileOffset = null);

/// <summary>
/// Projects the initial and repeated primary-pop calls and their result gates.
/// A missing pop result stops at the call boundary. The repeat call does not
/// advance the descriptor end here; that advance belongs to the append path.
/// </summary>
public static class UiShaderSourcePoolResetPrimaryGateProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetFunctionRawFileOffset = 0x12CDC50;
    public const uint InitialGateStartRawFileOffset = 0x12CDCC2;
    public const uint InitialGateEndRawFileOffsetExclusive = 0x12CDCE3;
    public const uint InitialReceiverLoadRawFileOffset = 0x12CDCD3;
    public const uint InitialPopCallRawFileOffset = 0x12CDCD8;
    public const uint InitialPopResultSaveRawFileOffset = 0x12CDCDD;
    public const uint InitialPopResultTestRawFileOffset = 0x12CDCDF;
    public const uint InitialZeroResultBranchRawFileOffset = 0x12CDCE1;

    public const uint RepeatGateStartRawFileOffset = 0x12CDD39;
    public const uint RepeatGateEndRawFileOffsetExclusive = 0x12CDD4D;
    public const uint RepeatReceiverLoadRawFileOffset = 0x12CDD39;
    public const uint AppendOwnedRepeatEndAdvanceRawFileOffset = 0x12CDD3C;
    public const uint AppendOwnedRepeatEndStoreRawFileOffset = 0x12CDD3F;
    public const uint RepeatPopCallRawFileOffset = 0x12CDD42;
    public const uint RepeatPopResultSaveRawFileOffset = 0x12CDD47;
    public const uint RepeatPopResultTestRawFileOffset = 0x12CDD49;
    public const uint RepeatNonzeroResultBranchRawFileOffset = 0x12CDD4B;

    public const uint PopCallTargetRawFileOffset = 0x12CDAE0;
    public const uint PopCallTargetVirtualAddress =
        PopCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint SecondaryGateContinuationRawFileOffset = 0x12CDD4D;
    public const uint AppendContinuationRawFileOffset = 0x12CDCE3;

    public const int DescriptorBeginStackOffset = -0x34;
    public const int DescriptorEndStackOffset = -0x30;
    public const int DescriptorStorageEndStackOffset = -0x2C;

    public const string CapturedInitialGateBytes =
        "33 F6 C7 45 CC 00 00 00 00 33 DB 89 75 D0 89 5D D4 " +
        "8B CF 89 5D FC E8 03 FE FF FF 8B F8 85 FF 74 6A";

    public const string CapturedRepeatGateBytes =
        "8B 4D E4 83 C6 04 89 75 D0 E8 99 FD FF FF 8B F8 " +
        "85 FF 75 96";

    public static IReadOnlyList<UiShaderSourcePoolResetPrimaryGateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Zero the local descriptor begin pointer on the initial path",
                0x12CDCC2,
                "33 F6 C7 45 CC 00 00 00 00"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Zero the local descriptor end and storage-end pointers on the initial path",
                0x12CDCCB,
                "33 DB 89 75 D0 89 5D D4"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Load the original EDI receiver into ECX for the initial pop",
                InitialReceiverLoadRawFileOffset,
                "8B CF"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Call the primary pop helper with the original receiver",
                InitialPopCallRawFileOffset,
                "E8 03 FE FF FF",
                CallTargetRawFileOffset: PopCallTargetRawFileOffset),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Save the initial pop result in EDI",
                InitialPopResultSaveRawFileOffset,
                "8B F8"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Test the initial pop result",
                InitialPopResultTestRawFileOffset,
                "85 FF"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "A zero initial pop result continues at the secondary gate",
                InitialZeroResultBranchRawFileOffset,
                "74 6A",
                BranchTargetRawFileOffset: SecondaryGateContinuationRawFileOffset),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Reload the saved receiver for the repeat pop",
                RepeatReceiverLoadRawFileOffset,
                "8B 4D E4"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "The append-owned end advance and store occur within this repeat-boundary byte window",
                AppendOwnedRepeatEndAdvanceRawFileOffset,
                "83 C6 04 89 75 D0"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Call the same primary pop helper with the reloaded receiver",
                RepeatPopCallRawFileOffset,
                "E8 99 FD FF FF",
                CallTargetRawFileOffset: PopCallTargetRawFileOffset),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Save the repeat pop result in EDI",
                RepeatPopResultSaveRawFileOffset,
                "8B F8"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "Test the repeat pop result",
                RepeatPopResultTestRawFileOffset,
                "85 FF"),
            new UiShaderSourcePoolResetPrimaryGateInstructionEvidence(
                "A nonzero repeat pop result returns to the append continuation",
                RepeatNonzeroResultBranchRawFileOffset,
                "75 96",
                BranchTargetRawFileOffset: AppendContinuationRawFileOffset)
        });

    public static UiShaderSourcePoolResetPrimaryGateProjection Project(
        UiShaderSourcePoolResetPrimaryGateCapture capture)
    {
        if (capture.Mode is not (
            UiShaderSourcePoolResetPrimaryGateMode.Initial
            or UiShaderSourcePoolResetPrimaryGateMode.Repeat))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capture),
                "The captured gate mode must be Initial or Repeat.");
        }

        bool initial = capture.Mode == UiShaderSourcePoolResetPrimaryGateMode.Initial;
        if (!capture.PopResultPointer.HasValue)
        {
            return new UiShaderSourcePoolResetPrimaryGateProjection(
                capture,
                DescriptorZeroingReached: initial,
                DescriptorBeginPointer: initial ? 0u : null,
                DescriptorEndPointer: initial ? 0u : null,
                DescriptorStorageEndPointer: initial ? 0u : null,
                UiShaderSourcePoolResetPrimaryGateBoundary.PopCallBoundary,
                ContinuationRawFileOffset: null);
        }

        bool resultWasZero = capture.PopResultPointer.Value == 0;
        return new UiShaderSourcePoolResetPrimaryGateProjection(
            capture,
            DescriptorZeroingReached: initial,
            DescriptorBeginPointer: initial ? 0u : null,
            DescriptorEndPointer: initial ? 0u : null,
            DescriptorStorageEndPointer: initial ? 0u : null,
            resultWasZero
                ? UiShaderSourcePoolResetPrimaryGateBoundary.SecondaryGate
                : UiShaderSourcePoolResetPrimaryGateBoundary.AppendBranch,
            resultWasZero
                ? SecondaryGateContinuationRawFileOffset
                : AppendContinuationRawFileOffset);
    }

    public static uint ResolveRelativeCallTargetRawFileOffset(
        uint callRawFileOffset,
        int displacement) =>
        unchecked(callRawFileOffset + 5 + (uint)displacement);

    public static uint ResolveShortBranchTargetRawFileOffset(
        uint branchRawFileOffset,
        sbyte displacement) =>
        unchecked(branchRawFileOffset + 2 + (uint)displacement);
}
