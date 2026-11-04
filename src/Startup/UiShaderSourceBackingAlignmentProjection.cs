namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Projects the captured allocation-size and aligned-data-pointer arithmetic only.
/// A zero allocator result is treated as an arithmetic input; this does not model
/// subsequent execution, dereferences, or allocation success.
/// </summary>
public static class UiShaderSourceBackingAlignmentProjectionRules
{
    public const uint ConstructorEntryRawFileOffset = 0x12CB700;
    public const uint CapacityStoreRawFileOffset = 0x12CB70A;
    public const uint ZeroCapacityDiagnosticRawFileOffset = 0x12CB713;
    public const uint AllocationSizeAddRawFileOffset = 0x12CB732;
    public const uint AllocatorCallRawFileOffset = 0x12CB735;
    public const uint AllocatorTargetRawFileOffset = 0x12B8030;
    public const string CapacityStoreInstructionBytesHex = "89 06";

    public const int HeaderCapacityOffset = 0x00;
    public const int HeaderDataPointerOffset = 0x04;
    public const int HeaderAlignmentDisplacementOffset = 0x08;
    public const int HeaderLogicalLengthOffset = 0x0C;

    public static UiShaderSourceBackingAlignmentProjection? Project(
        uint capacity,
        uint alignment,
        uint capturedAllocatorPointer,
        bool capturedZeroCapacityDiagnosticReturned)
    {
        if (capacity == 0 && !capturedZeroCapacityDiagnosticReturned)
            return null;

        uint allocationByteCount = unchecked(capacity + alignment);
        uint alignmentDisplacement = alignment == 0
            ? 0
            : unchecked(alignment - capturedAllocatorPointer % alignment);
        uint dataPointer = unchecked(capturedAllocatorPointer + alignmentDisplacement);

        return new UiShaderSourceBackingAlignmentProjection(
            allocationByteCount,
            HeaderCapacity: capacity,
            dataPointer,
            alignmentDisplacement,
            LogicalLength: 0);
    }
}

public readonly record struct UiShaderSourceBackingAlignmentProjection(
    uint AllocationByteCount,
    uint HeaderCapacity,
    uint DataPointer,
    uint AlignmentDisplacement,
    uint LogicalLength);
