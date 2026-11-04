namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetGrowthCapture(
    uint CapacityEndAtEntry,
    uint EndAtEntry,
    uint Request,
    uint? BeginAtUsedCalculation = null,
    uint? BeginAtCapacityCalculation = null);

public enum UiShaderSourcePoolResetGrowthBoundary
{
    ImmediateReturn,
    DiagnosticReporterCallUnmodeled,
    TailTransferToAllocatorUnmodeled
}

public readonly record struct UiShaderSourcePoolResetGrowthProjection(
    UiShaderSourcePoolResetGrowthCapture Capture,
    uint SpareCount,
    uint? UsedCount,
    uint? RemainingCapacityLimit,
    uint? CapacityCount,
    uint? RequiredCapacityCount,
    uint? GrowthCandidateCount,
    uint? SelectedCapacityCount,
    bool? CapacityGrowthOverflowed,
    uint? SelectedDiagnosticArgumentVirtualAddress,
    uint? ReturnEax,
    uint? TailTransferRawFileOffset,
    UiShaderSourcePoolResetGrowthBoundary Boundary);

public readonly record struct UiShaderSourcePoolResetGrowthInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? DescriptorFieldOffset = null,
    uint? DiagnosticArgumentVirtualAddress = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the capacity decision in the raw 0x6A5500 helper. It stops at the
/// diagnostic call when selected, or at the tail transfer to 0x6A5900; it
/// does not model reporter return state, allocation, or memory effects.
/// </summary>
public static class UiShaderSourcePoolResetGrowthProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x006A5500;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint FunctionEndRawFileOffsetExclusive = 0x006A5573;

    public const uint SpareCountCompareRawFileOffset = 0x006A5515;
    public const uint SpareCountReturnBranchRawFileOffset = 0x006A5517;
    public const uint UsedBeginReadRawFileOffset = 0x006A5519;
    public const uint CapacityLimitCompareRawFileOffset = 0x006A5528;
    public const uint CapacityLimitBranchRawFileOffset = 0x006A552A;
    public const uint ReporterCallRawFileOffset = 0x006A5531;
    public const uint GrowthBeginReadRawFileOffset = 0x006A5537;
    public const uint GrowthOverflowCompareRawFileOffset = 0x006A5544;
    public const uint GrowthOverflowBranchRawFileOffset = 0x006A5547;
    public const uint OverflowFallbackTailTransferRawFileOffset = 0x006A5556;
    public const uint GrowthTailTransferRawFileOffset = 0x006A5568;
    public const uint ImmediateReturnRawFileOffset = 0x006A5570;
    public const uint TailTargetRawFileOffset = 0x006A5900;
    public const uint ReporterCellVirtualAddress = 0x01AC45C0;
    public const uint ReporterArgumentVirtualAddress = 0x01C5EA14;
    public const uint MaximumCapacityCount = 0x3FFFFFFF;

    public const string CapturedFunctionBytes =
        "55 8B EC 8B 51 08 8B C2 56 8B 71 04 2B C6 57 8B " +
        "7D 08 C1 F8 02 3B C7 73 54 2B 31 53 BB FF FF FF " +
        "3F C1 FE 02 8B C3 2B C6 3B C7 73 0B 68 14 EA C5 " +
        "01 FF 15 C0 45 AC 01 2B 11 03 F7 C1 FA 02 8B C2 " +
        "D1 E8 2B D8 3B DA 5B 73 12 33 D2 3B D6 5F 0F 42 " +
        "D6 5E 89 55 08 5D E9 A5 03 00 00 03 D0 3B D6 5F " +
        "0F 42 D6 5E 89 55 08 5D E9 93 03 00 00 5F 5E 5D " +
        "C2 04 00";

    public static IReadOnlyList<UiShaderSourcePoolResetGrowthInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Capture the capacity end from descriptor + 8",
                0x006A5503,
                "8B 51 08",
                DescriptorFieldOffset: 8),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Capture the descriptor end from descriptor + 4",
                0x006A5509,
                "8B 71 04",
                DescriptorFieldOffset: 4),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Subtract the captured descriptor end from the capacity end",
                0x006A550C,
                "2B C6"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Arithmetic-shift the wrapped spare-byte difference by two",
                0x006A5512,
                "C1 F8 02"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Compare the spare-count bit pattern unsigned with the request",
                SpareCountCompareRawFileOffset,
                "3B C7"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Return directly when spare count is unsigned-greater-or-equal to the request",
                SpareCountReturnBranchRawFileOffset,
                "73 54",
                TargetRawFileOffset: 0x006A556D),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Read the begin pointer independently for used-count arithmetic",
                UsedBeginReadRawFileOffset,
                "2B 31",
                DescriptorFieldOffset: 0),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Arithmetic-shift the wrapped used-byte difference by two",
                0x006A5521,
                "C1 FE 02"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Compare 0x3FFFFFFF minus used count, using unsigned request ordering",
                CapacityLimitCompareRawFileOffset,
                "3B C7"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Skip the reporter when the remaining capacity limit covers the request",
                CapacityLimitBranchRawFileOffset,
                "73 0B",
                TargetRawFileOffset: GrowthBeginReadRawFileOffset),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Push the reporter argument",
                0x006A552C,
                "68 14 EA C5 01",
                DiagnosticArgumentVirtualAddress: ReporterArgumentVirtualAddress),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Call the reporter through its indirect cell; continuation is not projected",
                ReporterCallRawFileOffset,
                "FF 15 C0 45 AC 01",
                TargetVirtualAddress: ReporterCellVirtualAddress),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Read the begin pointer independently for capacity growth",
                GrowthBeginReadRawFileOffset,
                "2B 11",
                DescriptorFieldOffset: 0),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Add the request to the used count with 32-bit wrapping",
                0x006A5539,
                "03 F7"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Arithmetic-shift the wrapped capacity-byte difference by two",
                0x006A553B,
                "C1 FA 02"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Compute half-capacity with a logical shift",
                0x006A5540,
                "D1 E8"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Subtract half-capacity from the maximum count for the unsigned overflow guard",
                0x006A5542,
                "2B D8"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Compare the unsigned overflow guard for capacity plus half-capacity",
                GrowthOverflowCompareRawFileOffset,
                "3B DA"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Select the zero-growth fallback when the guard fails",
                GrowthOverflowBranchRawFileOffset,
                "73 12",
                TargetRawFileOffset: 0x006A555B),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Tail-transfer the overflow-fallback maximum to the allocator helper",
                OverflowFallbackTailTransferRawFileOffset,
                "E9 A5 03 00 00",
                TargetRawFileOffset: TailTargetRawFileOffset),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Add capacity and half-capacity with 32-bit wrapping on the guarded path",
                0x006A555B,
                "03 D0"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Compare the growth candidate with wrapped used-plus-request using unsigned ordering",
                0x006A555D,
                "3B D6"),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Tail-transfer the grown-capacity maximum to the allocator helper",
                GrowthTailTransferRawFileOffset,
                "E9 93 03 00 00",
                TargetRawFileOffset: TailTargetRawFileOffset),
            new UiShaderSourcePoolResetGrowthInstructionEvidence(
                "Return and pop the four-byte request on the spare-capacity path",
                ImmediateReturnRawFileOffset,
                "C2 04 00")
        });

    public static UiShaderSourcePoolResetGrowthProjection Project(
        UiShaderSourcePoolResetGrowthCapture capture)
    {
        uint spareCount = ArithmeticShiftRightTwo(
            unchecked(capture.CapacityEndAtEntry - capture.EndAtEntry));

        if (spareCount >= capture.Request)
        {
            if (capture.BeginAtUsedCalculation.HasValue
                || capture.BeginAtCapacityCalculation.HasValue)
            {
                throw new ArgumentException(
                    "Begin-pointer captures are not reached when spare count covers the request.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolResetGrowthProjection(
                capture,
                spareCount,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                spareCount,
                null,
                UiShaderSourcePoolResetGrowthBoundary.ImmediateReturn);
        }

        if (!capture.BeginAtUsedCalculation.HasValue)
        {
            throw new ArgumentException(
                "The begin pointer read at the used-count calculation is required.",
                nameof(capture));
        }

        uint usedCount = ArithmeticShiftRightTwo(
            unchecked(capture.EndAtEntry - capture.BeginAtUsedCalculation.Value));
        uint remainingCapacityLimit =
            unchecked(MaximumCapacityCount - usedCount);

        if (remainingCapacityLimit < capture.Request)
        {
            if (capture.BeginAtCapacityCalculation.HasValue)
            {
                throw new ArgumentException(
                    "The later begin-pointer read is not reached before the reporter call boundary.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolResetGrowthProjection(
                capture,
                spareCount,
                usedCount,
                remainingCapacityLimit,
                null,
                null,
                null,
                null,
                null,
                ReporterArgumentVirtualAddress,
                null,
                null,
                UiShaderSourcePoolResetGrowthBoundary.DiagnosticReporterCallUnmodeled);
        }

        if (!capture.BeginAtCapacityCalculation.HasValue)
        {
            throw new ArgumentException(
                "The independent begin-pointer read at the capacity calculation is required.",
                nameof(capture));
        }

        uint capacityCount = ArithmeticShiftRightTwo(
            unchecked(capture.CapacityEndAtEntry - capture.BeginAtCapacityCalculation.Value));
        uint halfCapacity = capacityCount >> 1;
        bool growthOverflowed =
            unchecked(MaximumCapacityCount - halfCapacity) < capacityCount;
        uint proposedCapacityCount = growthOverflowed
            ? 0
            : unchecked(capacityCount + halfCapacity);
        uint requiredCapacityCount = unchecked(usedCount + capture.Request);
        uint selectedCapacityCount = proposedCapacityCount < requiredCapacityCount
            ? requiredCapacityCount
            : proposedCapacityCount;
        uint tailTransferRawFileOffset = growthOverflowed
            ? OverflowFallbackTailTransferRawFileOffset
            : GrowthTailTransferRawFileOffset;

        return new UiShaderSourcePoolResetGrowthProjection(
            capture,
            spareCount,
            usedCount,
            remainingCapacityLimit,
            capacityCount,
            requiredCapacityCount,
            proposedCapacityCount,
            selectedCapacityCount,
            growthOverflowed,
            null,
            null,
            tailTransferRawFileOffset,
            UiShaderSourcePoolResetGrowthBoundary.TailTransferToAllocatorUnmodeled);
    }

    private static uint ArithmeticShiftRightTwo(uint value) =>
        unchecked((uint)((int)value >> 2));
}
