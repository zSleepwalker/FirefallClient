using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolConfigurationCapture(
    uint SizeArgument,
    uint CountArgument,
    uint AlignmentArgument,
    uint CountAt9E,
    uint AlignmentAtBE,
    uint AlignmentAtDE,
    uint PaddingSizeAtC02,
    uint PaddingAlignmentAtC05);

public readonly record struct UiShaderSourcePoolConfigurationFieldWrite(
    int FieldOffset,
    uint Value,
    uint InstructionRawFileOffset);

public enum UiShaderSourcePoolConfigurationBoundary
{
    SelectedDiagnosticsReturnedNormally
}

public readonly record struct UiShaderSourcePoolConfigurationProjection(
    UiShaderSourcePoolConfigurationCapture Capture,
    ImmutableArray<UiShaderSourcePoolConfigurationFieldWrite> FieldWrites,
    ImmutableArray<uint> DiagnosticIds,
    uint PaddingBytes,
    UiShaderSourcePoolConfigurationBoundary Boundary);

public readonly record struct UiShaderSourcePoolConfigurationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? FieldOffset = null,
    uint? DiagnosticId = null,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the observed ordered field writes, diagnostic selections, and DWORD
/// padding arithmetic. The entry arguments and each later field read are captured
/// independently, not as a coherent snapshot; reporter callbacks may change values
/// observed by subsequent reads. The projection is bounded to the path where each
/// selected diagnostic reporter returns normally; it does not model allocation,
/// ownership, or memory mutation.
/// </summary>
public static class UiShaderSourcePoolConfigurationProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x12CDB40;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticAndPaddingEndRawFileOffsetExclusive = 0x12CDC1B;
    public const uint DiagnosticAndPaddingEndVirtualAddressExclusive =
        DiagnosticAndPaddingEndRawFileOffsetExclusive
        + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint NormalReturnInstructionRawFileOffset =
        DiagnosticAndPaddingEndRawFileOffsetExclusive;

    public const int Field0Offset = 0x00;
    public const int Field4Offset = 0x04;
    public const int Field8Offset = 0x08;
    public const int FieldCOffset = 0x0C;
    public const int Field10Offset = 0x10;
    public const int Field14Offset = 0x14;
    public const int Field18Offset = 0x18;
    public const int Field1COffset = 0x1C;
    public const int Field20Offset = 0x20;

    public const uint ZeroSizeDiagnosticId = 501;
    public const uint ZeroCountDiagnosticId = 502;
    public const uint ZeroAlignmentDiagnosticId = 503;
    public const uint NonPowerOfTwoAlignmentDiagnosticId = 509;

    public const string CapturedInitializerBytes =
        "55 8B EC 8B 45 10 56 8B F1 8B 4D 08 C7 06 00 00 00 00 " +
        "C7 46 04 00 00 00 00 C7 46 08 00 00 00 00 C7 46 0C 00 00 00 00 " +
        "C7 46 10 00 00 00 00 89 46 18 8B 45 0C 89 4E 14 C7 46 1C 00 00 00 00 " +
        "89 46 20 85 C9 75 19 51 68 F5 01 00 00 68 5F 41 B7 01 68 83 41 B7 01 " +
        "FF 15 A0 5E C1 01 83 C4 10 83 7E 20 00 77 1A 6A 00 68 F6 01 00 00 " +
        "68 77 41 B7 01 68 FB 41 B7 01 FF 15 A0 5E C1 01 83 C4 10 " +
        "83 7E 18 00 77 1A 6A 00 68 F7 01 00 00 68 EF 41 B7 01 68 13 42 B7 01 " +
        "FF 15 A0 5E C1 01 83 C4 10 8B 4E 18 8D 41 FF 85 C1 74 1A " +
        "6A 00 68 FD 01 00 00 68 07 42 B7 01 68 CB 41 B7 01 FF 15 A0 5E C1 01 " +
        "83 C4 10 8B 4E 14 8B 46 18 8D 51 FF 03 D0 48 F7 D0 23 D0 8B C6 " +
        "2B D1 89 56 1C 5E 5D";

    public static ImmutableArray<UiShaderSourcePoolConfigurationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x00",
                0x12CDB4C,
                "C7 06 00 00 00 00",
                Field0Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x04",
                0x12CDB52,
                "C7 46 04 00 00 00 00",
                Field4Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x08",
                0x12CDB59,
                "C7 46 08 00 00 00 00",
                Field8Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x0C",
                0x12CDB60,
                "C7 46 0C 00 00 00 00",
                FieldCOffset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x10",
                0x12CDB67,
                "C7 46 10 00 00 00 00",
                Field10Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Write the captured alignment argument to field 0x18",
                0x12CDB6E,
                "89 46 18",
                Field18Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Write the captured size argument to field 0x14",
                0x12CDB74,
                "89 4E 14",
                Field14Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Zero field 0x1C before validation",
                0x12CDB77,
                "C7 46 1C 00 00 00 00",
                Field1COffset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Write the captured count argument to field 0x20",
                0x12CDB7E,
                "89 46 20",
                Field20Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Test the captured size argument",
                0x12CDB81,
                "85 C9"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Skip diagnostic 501 for nonzero size",
                0x12CDB83,
                "75 19",
                BranchTargetRawFileOffset: 0x12CDB9E),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Select diagnostic 501 for zero size",
                0x12CDB86,
                "68 F5 01 00 00",
                DiagnosticId: ZeroSizeDiagnosticId),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Call the indirect diagnostic reporter",
                0x12CDB95,
                "FF 15 A0 5E C1 01"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Compare captured count at field 0x20 with zero",
                0x12CDB9E,
                "83 7E 20 00",
                Field20Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Skip diagnostic 502 when count is nonzero",
                0x12CDBA2,
                "77 1A",
                BranchTargetRawFileOffset: 0x12CDBBE),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Select diagnostic 502 for zero count",
                0x12CDBA6,
                "68 F6 01 00 00",
                DiagnosticId: ZeroCountDiagnosticId),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Call the indirect diagnostic reporter",
                0x12CDBB5,
                "FF 15 A0 5E C1 01"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Compare captured alignment at field 0x18 with zero",
                0x12CDBBE,
                "83 7E 18 00",
                Field18Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Skip diagnostic 503 when alignment is nonzero",
                0x12CDBC2,
                "77 1A",
                BranchTargetRawFileOffset: 0x12CDBDE),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Select diagnostic 503 for zero alignment",
                0x12CDBC6,
                "68 F7 01 00 00",
                DiagnosticId: ZeroAlignmentDiagnosticId),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Call the indirect diagnostic reporter",
                0x12CDBD5,
                "FF 15 A0 5E C1 01"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Load alignment and form alignment-minus-one",
                0x12CDBDE,
                "8B 4E 18 8D 41 FF",
                Field18Offset),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Test alignment & (alignment - 1)",
                0x12CDBE4,
                "85 C1"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Skip diagnostic 509 when the power-of-two predicate is zero",
                0x12CDBE6,
                "74 1A",
                BranchTargetRawFileOffset: 0x12CDC02),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Select diagnostic 509 when alignment & (alignment - 1) is nonzero",
                0x12CDBEA,
                "68 FD 01 00 00",
                DiagnosticId: NonPowerOfTwoAlignmentDiagnosticId),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Call the indirect diagnostic reporter",
                0x12CDBF9,
                "FF 15 A0 5E C1 01"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Load size and alignment for the padding expression",
                0x12CDC02,
                "8B 4E 14 8B 46 18"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Form (size - 1) + alignment with DWORD arithmetic",
                0x12CDC08,
                "8D 51 FF 03 D0"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Form the mask ~(alignment - 1) and apply it",
                0x12CDC0D,
                "48 F7 D0 23 D0"),
            new UiShaderSourcePoolConfigurationInstructionEvidence(
                "Subtract size and store the result at field 0x1C",
                0x12CDC14,
                "2B D1 89 56 1C",
                Field1COffset));

    public static UiShaderSourcePoolConfigurationProjection Project(
        UiShaderSourcePoolConfigurationCapture capture)
    {
        var diagnosticIds = ImmutableArray.CreateBuilder<uint>(4);
        if (capture.SizeArgument == 0)
        {
            diagnosticIds.Add(ZeroSizeDiagnosticId);
        }

        if (capture.CountAt9E == 0)
        {
            diagnosticIds.Add(ZeroCountDiagnosticId);
        }

        if (capture.AlignmentAtBE == 0)
        {
            diagnosticIds.Add(ZeroAlignmentDiagnosticId);
        }

        if ((capture.AlignmentAtDE & unchecked(capture.AlignmentAtDE - 1)) != 0)
        {
            diagnosticIds.Add(NonPowerOfTwoAlignmentDiagnosticId);
        }

        uint alignmentMinusOne = unchecked(capture.PaddingAlignmentAtC05 - 1);
        uint sizeMinusOnePlusAlignment =
            unchecked(unchecked(capture.PaddingSizeAtC02 - 1) + capture.PaddingAlignmentAtC05);
        uint alignedSize = sizeMinusOnePlusAlignment & ~alignmentMinusOne;
        uint paddingBytes = unchecked(alignedSize - capture.PaddingSizeAtC02);

        var fieldWrites = ImmutableArray.Create(
            new UiShaderSourcePoolConfigurationFieldWrite(Field0Offset, 0, 0x12CDB4C),
            new UiShaderSourcePoolConfigurationFieldWrite(Field4Offset, 0, 0x12CDB52),
            new UiShaderSourcePoolConfigurationFieldWrite(Field8Offset, 0, 0x12CDB59),
            new UiShaderSourcePoolConfigurationFieldWrite(FieldCOffset, 0, 0x12CDB60),
            new UiShaderSourcePoolConfigurationFieldWrite(Field10Offset, 0, 0x12CDB67),
            new UiShaderSourcePoolConfigurationFieldWrite(
                Field18Offset, capture.AlignmentArgument, 0x12CDB6E),
            new UiShaderSourcePoolConfigurationFieldWrite(
                Field14Offset, capture.SizeArgument, 0x12CDB74),
            new UiShaderSourcePoolConfigurationFieldWrite(Field1COffset, 0, 0x12CDB77),
            new UiShaderSourcePoolConfigurationFieldWrite(
                Field20Offset, capture.CountArgument, 0x12CDB7E),
            new UiShaderSourcePoolConfigurationFieldWrite(
                Field1COffset, paddingBytes, 0x12CDC16));

        return new UiShaderSourcePoolConfigurationProjection(
            capture,
            fieldWrites,
            diagnosticIds.ToImmutable(),
            paddingBytes,
            UiShaderSourcePoolConfigurationBoundary.SelectedDiagnosticsReturnedNormally);
    }
}
