namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Branch-specific static observations for the UI shader-source wrapper clear paths.
/// These record wrapper field writes and a captured release boundary; they do not model
/// reference counts, allocator behavior, or unconditional detachment.
/// </summary>
public static class UiShaderSourceClearNativeLayout
{
    public const int WrapperBackingPointerOffset = 0x04;
    public const int WrapperControlPointerOffset = 0x08;
    public const int BackingHeaderFieldCOffset = 0x0C;

    public const uint ZeroFastPathInstructionRawFileOffset = 0x12CBD33;

    public const uint SlowZeroCaptureInstructionRawFileOffset = 0x12CBD4F;
    public const uint SlowZeroClearControlInstructionRawFileOffset = 0x12CBD52;
    public const uint SlowZeroClearBackingInstructionRawFileOffset = 0x12CBD55;
    public const uint SlowZeroReleaseEntryRawFileOffset = 0x12CBE24;
    public const string SlowZeroCapturedControlRegister = "EBX";

    public const uint Slot14EntryRawFileOffset = 0x12CBA00;
    public const uint Slot14HelperTargetRawFileOffset = 0x12CBCB0;

    public static IReadOnlyList<UiShaderSourceClearInstructionEvidence> SlowZeroPathInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceClearInstructionEvidence(
                "Capture old wrapper control pointer in EBX",
                SlowZeroCaptureInstructionRawFileOffset,
                WrapperRelativeOffset: WrapperControlPointerOffset),
            new UiShaderSourceClearInstructionEvidence(
                "Clear wrapper control pointer",
                SlowZeroClearControlInstructionRawFileOffset,
                WrapperRelativeOffset: WrapperControlPointerOffset),
            new UiShaderSourceClearInstructionEvidence(
                "Clear wrapper backing pointer",
                SlowZeroClearBackingInstructionRawFileOffset,
                WrapperRelativeOffset: WrapperBackingPointerOffset),
            new UiShaderSourceClearInstructionEvidence(
                "Enter captured old-control release path",
                SlowZeroReleaseEntryRawFileOffset)
        });

    public static IReadOnlyList<UiShaderSourceClearInstructionEvidence> ZeroFastPathInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceClearInstructionEvidence(
                "Write zero to backing header field C",
                ZeroFastPathInstructionRawFileOffset,
                BackingRecordRelativeOffset: BackingHeaderFieldCOffset)
        });
}

/// <summary>
/// One observed instruction and its raw-file address, with any data-field offset explicitly
/// identified as wrapper-relative or backing-record-relative.
/// </summary>
public readonly record struct UiShaderSourceClearInstructionEvidence(
    string Observation,
    uint InstructionRawFileOffset,
    int? WrapperRelativeOffset = null,
    int? BackingRecordRelativeOffset = null);
