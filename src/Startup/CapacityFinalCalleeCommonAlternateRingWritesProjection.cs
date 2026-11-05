using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Routes the alternate 012C9343 entry through the shared common-ring write projection.
/// </summary>
public static class CapacityFinalCalleeCommonAlternateRingWritesProjection
{
    public const uint EntryRawFileOffset = 0x012C9343;
    public const uint EntryFlatIdaAddress = 0x012C9343;
    public const uint EntryVirtualAddress = 0x016C9F43;
    public const uint FirstWriteRawFileOffset = 0x012C9343;
    public const uint SecondWriteRawFileOffset = 0x012C9346;
    public const uint ThirdWriteRawFileOffset = 0x012C9349;
    public const uint FourthWriteRawFileOffset = 0x012C934C;
    public const uint BranchRawFileOffset = 0x012C934F;
    public const uint BranchNextRawFileOffset = 0x012C9354;
    public const uint BranchTargetRawFileOffset = 0x012C9441;
    public const uint BoundaryRawFileOffsetExclusive = BranchTargetRawFileOffset;

    public const string CapturedWindowBytes =
        "89 72 08 89 70 0C 89 46 08 89 56 0C E9 ED 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence>
        Evidence { get; } =
        ImmutableArray.Create(
            Instruction(
                "Alternate store entry ESI at entry EDX+8",
                FirstWriteRawFileOffset,
                "89 72 08"),
            Instruction(
                "Alternate store entry ESI at entry EAX+C",
                SecondWriteRawFileOffset,
                "89 70 0C"),
            Instruction(
                "Alternate store entry EAX at entry ESI+8",
                ThirdWriteRawFileOffset,
                "89 46 08"),
            Instruction(
                "Alternate store entry EDX at entry ESI+C",
                FourthWriteRawFileOffset,
                "89 56 0C"),
            Instruction(
                "Alternate jump to raw offset 012C9441",
                BranchRawFileOffset,
                "E9 ED 00 00 00",
                BranchTargetRawFileOffset));

    public static CapacityFinalCalleeCommonRingWritesPlan Project(
        CapacityFinalCalleeCommonRingWritesCapture capture) =>
        CapacityFinalCalleeCommonRingWritesProjection.ProjectCore(
            capture,
            new(
                EntryRawFileOffset,
                FirstWriteRawFileOffset,
                SecondWriteRawFileOffset,
                ThirdWriteRawFileOffset,
                FourthWriteRawFileOffset,
                BranchRawFileOffset,
                BranchNextRawFileOffset,
                BranchTargetRawFileOffset,
                Evidence));

    private static CapacityFinalCalleeCommonRingWritesInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                    targetRaw)
                : null);
}
