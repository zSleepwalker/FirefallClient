namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolReplenishmentCapture(
    uint Field14Value,
    uint Field1CValue,
    uint Field20Value,
    uint Field18Value);

public readonly record struct UiShaderSourcePoolReplenishmentProjection(
    UiShaderSourcePoolReplenishmentCapture Capture,
    uint Field14PlusField1C,
    uint ScaledTerm,
    uint Field18PlusThree,
    uint RequestBytes,
    uint AllocatorCallRawFileOffset,
    uint AllocatorFunctionRawFileOffset);

public readonly record struct UiShaderSourcePoolReplenishmentInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? FieldOffset = null,
    uint? TargetRawFileOffset = null);

/// <summary>
/// Projects the captured 32-bit request arithmetic up to the allocator call.
/// It does not model the allocator result or any later control flow.
/// </summary>
public static class UiShaderSourcePoolReplenishmentProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CE0F0;
    public const uint EntryVirtualAddress = 0x16CECF0;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CE11E;

    public const uint Field14LoadRawFileOffset = 0x12CE105;
    public const uint Field1CAddRawFileOffset = 0x12CE108;
    public const uint Field20MultiplyRawFileOffset = 0x12CE10B;
    public const uint Field18LoadRawFileOffset = 0x12CE10F;
    public const uint AddThreeRawFileOffset = 0x12CE112;
    public const uint AddScaledTermRawFileOffset = 0x12CE115;
    public const uint RequestArgumentMoveRawFileOffset = 0x12CE117;
    public const uint AllocatorCallRawFileOffset = 0x12CE119;
    public const uint AllocatorFunctionRawFileOffset = 0x12C8A20;

    public const int Field14Offset = 0x14;
    public const int Field1COffset = 0x1C;
    public const int Field20Offset = 0x20;
    public const int Field18Offset = 0x18;

    public const string CapturedEntryBytes =
        "8B 57 14 03 57 1C 0F AF 57 20 8B 77 18 83 C6 03 " +
        "03 F2 8B CE E8 02 A9 FF FF";

    public static IReadOnlyList<UiShaderSourcePoolReplenishmentInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Load the first captured DWORD from this + 0x14",
                Field14LoadRawFileOffset,
                "8B 57 14",
                Field14Offset),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Add the separately captured DWORD at this + 0x1C",
                Field1CAddRawFileOffset,
                "03 57 1C",
                Field1COffset),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Multiply by the DWORD at this + 0x20, retaining the low 32 bits",
                Field20MultiplyRawFileOffset,
                "0F AF 57 20",
                Field20Offset),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Load the separately captured DWORD from this + 0x18",
                Field18LoadRawFileOffset,
                "8B 77 18",
                Field18Offset),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Add the immediate value 3",
                AddThreeRawFileOffset,
                "83 C6 03"),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Add the scaled term to the field-0x18 value",
                AddScaledTermRawFileOffset,
                "03 F2"),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Move the computed request into the allocator's ECX argument",
                RequestArgumentMoveRawFileOffset,
                "8B CE"),
            new UiShaderSourcePoolReplenishmentInstructionEvidence(
                "Call the allocator helper; projection stops at this boundary",
                AllocatorCallRawFileOffset,
                "E8 02 A9 FF FF",
                TargetRawFileOffset: AllocatorFunctionRawFileOffset)
        });

    public static UiShaderSourcePoolReplenishmentProjection Project(
        UiShaderSourcePoolReplenishmentCapture capture)
    {
        uint firstSum = unchecked(capture.Field14Value + capture.Field1CValue);
        uint scaledTerm = unchecked(firstSum * capture.Field20Value);
        uint field18PlusThree = unchecked(capture.Field18Value + 3);
        uint requestBytes = unchecked(scaledTerm + field18PlusThree);

        return new UiShaderSourcePoolReplenishmentProjection(
            capture,
            firstSum,
            scaledTerm,
            field18PlusThree,
            requestBytes,
            AllocatorCallRawFileOffset,
            AllocatorFunctionRawFileOffset);
    }
}
