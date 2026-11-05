using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourceNonPoolReleaseCapture(
    uint HeaderPointer,
    uint? HeaderWordAtOffset4,
    uint? HeaderWordAtOffset8);

public readonly record struct UiShaderSourceNonPoolReleaseAction(
    UiShaderSourceNonPoolReleaseActionKind Kind,
    uint InstructionRawFileOffset,
    uint? HelperArgument = null,
    uint? TargetRawFileOffset = null);

public readonly record struct UiShaderSourceNonPoolReleasePlan(
    UiShaderSourceNonPoolReleaseCapture Capture,
    uint? WrappedHeaderDifference,
    ImmutableArray<UiShaderSourceNonPoolReleaseAction> Actions);

public enum UiShaderSourceNonPoolReleaseActionKind
{
    CallOffsetReleaseHelper,
    TailJumpOffsetReleaseHelper,
    Return
}

/// <summary>
/// Projects the captured nonpool control's slot-0 header-release path. It records
/// only the header pointer and the two captured words used to form the helper argument;
/// it does not dereference memory, model the helper, clear fields, or infer ownership.
/// </summary>
public static class UiShaderSourceNonPoolReleaseProjection
{
    public const string PinnedBinarySha256 =
        "97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445";

    public const uint DataVirtualAddressDeltaFromRawFileOffset = 0x00401400;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint NonPoolControlVtableVirtualAddress = 0x1C80CB0;
    public const uint NonPoolControlVtableRawFileOffset = 0x187F8B0;
    public const string NonPoolControlVtableSlotPointerBytes =
        "80 D5 6C 01 A0 D5 6C 01 D0 D5 6C 01";

    public const uint HeaderReleaseSlotVirtualAddress = 0x16CD580;
    public const uint HeaderReleaseSlotRawFileOffset = 0x12CC980;
    public const string HeaderReleaseSlotPointerBytes = "80 D5 6C 01";
    public const uint HeaderReleaseEntryEndRawFileOffsetExclusive = 0x12CC99D;
    public const string HeaderReleaseEntryBytes =
        "56 8B 71 0C 85 F6 74 13 8B 4E 04 2B 4E 08 E8 CD C3 FF FF " +
        "8B CE 5E E9 C5 C3 FF FF 5E C3";

    public const int HeaderPointerOffset = 0x0C;
    public const int HeaderWordAtOffset4 = 0x04;
    public const int HeaderWordAtOffset8 = 0x08;
    public const uint NullHeaderBranchRawFileOffset = 0x12CC986;
    public const uint NullHeaderReturnRawFileOffset = 0x12CC99B;
    public const string NullHeaderReturnBytes = "5E C3";

    public const uint DifferenceCallRawFileOffset = 0x12CC98E;
    public const string DifferenceCallBytes = "E8 CD C3 FF FF";
    public const uint HeaderTailJumpRawFileOffset = 0x12CC996;
    public const string HeaderTailJumpBytes = "E9 C5 C3 FF FF";
    public const uint OffsetReleaseHelperRawFileOffset = 0x12C8D60;

    public static IReadOnlyList<UiShaderSourceNonPoolReleaseInstructionEvidence>
        DirectEntryInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Save ESI before reading the control's captured header pointer",
                HeaderReleaseSlotRawFileOffset,
                "56"),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Read the captured header pointer from control +0xC",
                HeaderReleaseSlotRawFileOffset + 1,
                "8B 71 0C",
                ControlRelativeOffset: HeaderPointerOffset),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Test the captured header pointer before either helper transfer",
                HeaderReleaseSlotRawFileOffset + 4,
                "85 F6"),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Return through the null-header path when the captured pointer is zero",
                NullHeaderBranchRawFileOffset,
                "74 13",
                TargetRawFileOffset: NullHeaderReturnRawFileOffset),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Load captured header word +4 into ECX",
                DifferenceCallRawFileOffset - 6,
                "8B 4E 04",
                HeaderRelativeOffset: HeaderWordAtOffset4),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Subtract captured header word +8 from ECX",
                DifferenceCallRawFileOffset - 3,
                "2B 4E 08",
                HeaderRelativeOffset: HeaderWordAtOffset8),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Call the offset-release helper with the wrapped header difference in ECX",
                DifferenceCallRawFileOffset,
                DifferenceCallBytes,
                TargetRawFileOffset: OffsetReleaseHelperRawFileOffset),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Restore ECX from the captured header pointer before the tail jump",
                DifferenceCallRawFileOffset + 5,
                "8B CE"),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Tail-jump to the same helper with the captured header pointer in ECX",
                HeaderTailJumpRawFileOffset,
                HeaderTailJumpBytes,
                TargetRawFileOffset: OffsetReleaseHelperRawFileOffset),
            new UiShaderSourceNonPoolReleaseInstructionEvidence(
                "Null-header path restores ESI and returns",
                NullHeaderReturnRawFileOffset,
                NullHeaderReturnBytes)
        });

    /// <summary>
    /// Projects the null return or, for a nonnull header, both distinct helper
    /// transfers. The helper call remains present even when the wrapped difference
    /// is zero; no callee-side null guard is projected here.
    /// </summary>
    public static UiShaderSourceNonPoolReleasePlan Project(
        UiShaderSourceNonPoolReleaseCapture capture)
    {
        if (capture.HeaderPointer == 0)
        {
            return new UiShaderSourceNonPoolReleasePlan(
                capture,
                WrappedHeaderDifference: null,
                ImmutableArray.Create(
                    new UiShaderSourceNonPoolReleaseAction(
                        UiShaderSourceNonPoolReleaseActionKind.Return,
                        NullHeaderReturnRawFileOffset)));
        }

        uint headerWordAtOffset4 = capture.HeaderWordAtOffset4
            ?? throw new ArgumentException(
                "A nonnull captured header requires its word at offset +4.",
                nameof(capture));
        uint headerWordAtOffset8 = capture.HeaderWordAtOffset8
            ?? throw new ArgumentException(
                "A nonnull captured header requires its word at offset +8.",
                nameof(capture));
        uint wrappedDifference = unchecked(headerWordAtOffset4 - headerWordAtOffset8);

        return new UiShaderSourceNonPoolReleasePlan(
            capture,
            wrappedDifference,
            ImmutableArray.Create(
                new UiShaderSourceNonPoolReleaseAction(
                    UiShaderSourceNonPoolReleaseActionKind.CallOffsetReleaseHelper,
                    DifferenceCallRawFileOffset,
                    HelperArgument: wrappedDifference,
                    TargetRawFileOffset: OffsetReleaseHelperRawFileOffset),
                new UiShaderSourceNonPoolReleaseAction(
                    UiShaderSourceNonPoolReleaseActionKind.TailJumpOffsetReleaseHelper,
                    HeaderTailJumpRawFileOffset,
                    HelperArgument: capture.HeaderPointer,
                    TargetRawFileOffset: OffsetReleaseHelperRawFileOffset)));
    }
}

public readonly record struct UiShaderSourceNonPoolReleaseInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ControlRelativeOffset = null,
    uint? HeaderRelativeOffset = null,
    uint? TargetRawFileOffset = null);
